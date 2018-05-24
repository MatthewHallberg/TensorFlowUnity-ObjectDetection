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
        var cropped = TextureTools.CropTexture(webcamTexture);
        //scale
        var scaled = TextureTools.scaled(cropped, 112, 112, FilterMode.Bilinear);
        //run detection
        return scaled.GetPixels32();
    }
}
