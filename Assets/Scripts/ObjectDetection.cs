using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TensorFlow;
using System.Threading;
using System.Threading.Tasks;

public class ObjectDetection : MonoBehaviour {

    [Header("Constants")]
    private const float MIN_SCORE = .25f;
    private const int INPUT_SIZE = 112;
    private const int IMAGE_MEAN = 0;
    private const float IMAGE_STD = 1;

    [Header("Inspector Stuff")]
    public CameraImage cameraImage;
    public TextAsset labelMap;
    public TextAsset model;
    public Color objectColor;
    public Texture2D tex;

    [Header("Private member")]
    private GUIStyle style = new GUIStyle();
    private TFGraph graph;
    private TFSession session;
    private IEnumerable<CatalogItem> _catalog;
    private List<CatalogItem> items = new List<CatalogItem>();

    [Header("Thread stuff")]
    Thread _thread;
    byte[] pixels;
    Color32 pixel;
    Color32[] colorPixels;
    TFTensor[] output;
    bool pixelsUpdated = false;
    bool processingImage = true;

	// Use this for initialization
    IEnumerator Start() {
        #if UNITY_ANDROID
        TensorFlowSharp.Android.NativeBinding.Init();
        #endif

        pixels = new byte[INPUT_SIZE * INPUT_SIZE * 3];
        _catalog = CatalogUtil.ReadCatalogItems(labelMap.text);
        Debug.Log("Loading graph...");
        graph = new TFGraph();
        graph.Import(model.bytes);
        session = new TFSession(graph);
        Debug.Log("Graph Loaded!!!");

        //set style of labels and boxes
        style.normal.background = tex;
        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = 80;
        style.fontStyle = FontStyle.Bold;
        style.contentOffset = new Vector2(0, 50);
        style.normal.textColor = objectColor;

        // Begin our heavy work on a new thread.
        _thread = new Thread(ThreadedWork);
        _thread.Start();
        //do this to avoid warnings
        processingImage = true;
        yield return new WaitForEndOfFrame();
        processingImage = false;
    }


    void ThreadedWork() {
        while (true) {
            if (pixelsUpdated) {
                TFShape shape = new TFShape(1, INPUT_SIZE, INPUT_SIZE, 3);
                var tensor = TFTensor.FromBuffer(shape, pixels, 0, pixels.Length);
                var runner = session.GetRunner();
                runner.AddInput(graph["image_tensor"][0], tensor).Fetch(
                    graph["detection_boxes"][0],
                    graph["detection_scores"][0],
                    graph["num_detections"][0],
                    graph["detection_classes"][0]);
                output = runner.Run();

                var boxes = (float[,,])output[0].GetValue(jagged: false);
                var scores = (float[,])output[1].GetValue(jagged: false);
                var num = (float[])output[2].GetValue(jagged: false);
                var classes = (float[,])output[3].GetValue(jagged: false);
                items.Clear();
                //loop through all detected objects
                for (int i = 0; i < num.Length; i++) {
                    for (int j = 0; j < scores.GetLength(i); j++) {
                        float score = scores[i, j];
                        if (score > MIN_SCORE) {
                            CatalogItem catalogItem = _catalog.FirstOrDefault(item => item.Id == Convert.ToInt32(classes[i, j]));
                            catalogItem.Score = score;
                            float ymin = boxes[i, j, 0] * Screen.height;
                            float xmin = boxes[i, j, 1] * Screen.width;
                            float ymax = boxes[i, j, 2] * Screen.height;
                            float xmax = boxes[i, j, 3] * Screen.width;
                            catalogItem.Box = Rect.MinMaxRect(xmin, Screen.height - ymax, xmax, Screen.height - ymin);
                            items.Add(catalogItem);
                            Debug.Log(catalogItem.DisplayName);
                        }
                    }
                }
                pixelsUpdated = false;
            }
        }
    }

    IEnumerator ProcessImage(){
        colorPixels = cameraImage.ProcessImage();
        //update pixels (Cant use Color32[] on non monobehavior thread
        for (int i = 0; i < colorPixels.Length; ++i) {
            pixel = colorPixels[i];
            pixels[i * 3 + 0] = (byte)((pixel.r - IMAGE_MEAN) / IMAGE_STD);
            pixels[i * 3 + 1] = (byte)((pixel.g - IMAGE_MEAN) / IMAGE_STD);
            pixels[i * 3 + 2] = (byte)((pixel.b - IMAGE_MEAN) / IMAGE_STD);
        }
        //flip bool so other thread will execute
        pixelsUpdated = true;
        //Resources.UnloadUnusedAssets();
        processingImage = false;
        yield return null;
    }

	private void Update() {
        if (!pixelsUpdated && !processingImage){
            processingImage = true;
            StartCoroutine(ProcessImage());
        }
	}

	void OnGUI() {
        try {
            foreach (CatalogItem item in items) {
                GUI.backgroundColor = objectColor;
                //display score and label
                //GUI.Box(item.Box, item.DisplayName + '\n' + Mathf.RoundToInt(item.Score*100) + "%", style);
                //display only score
                GUI.Box(item.Box, item.DisplayName, style);
            }
        } catch (InvalidOperationException e) {
            Debug.Log("Collection modified during Execution " + e);
        }
    }
}

