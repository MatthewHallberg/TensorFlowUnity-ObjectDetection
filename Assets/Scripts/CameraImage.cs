using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraImage : MonoBehaviour {

    WebCamTexture webcamTexture;
    RawImage image;

    void Start() {
        //delay initialize camera
        webcamTexture = new WebCamTexture();
        image = GetComponent<RawImage>();
        image.texture = webcamTexture;
        webcamTexture.Play();
    }

    public Color32[] ProcessImage(){
        //crop
        var cropped = CropTexture(webcamTexture);
        //scale
        var scaled = TextureTools.scaled(cropped, 224, 224, FilterMode.Bilinear);
        //run detection
        return scaled.GetPixels32();
    }

    private Texture2D CropTexture(WebCamTexture tex) {
        var smallest = tex.width < tex.height ?
            tex.width : tex.height;
        var snap = TextureTools.CropWithRect(tex,
            new Rect(0, 0, smallest, smallest),
            TextureTools.RectOptions.Center, 0, 0);
        return snap;
    }
}
