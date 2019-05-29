using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;
using Klak.Video;



public class PlayWebcam : MonoBehaviour {

    WebCamTexture webcamTexture;
    ProcAmp pa;
    //public RawImage ri;
    WebCamDevice[] devices;
    bool isInDebugMode;

    void Awake()
    {

        //EDSDK.EdsSendCommand(camera, EDSDK.CameraCommand_TakePicture, 0); 
        //uint err = EDSDK.EdsInitializeSDK();
        //EDSDK.EdsSendCommand(IntPtr.Zero, EDSDK.CameraCommand_TakePicture, 0);
       
        webcamTexture = new WebCamTexture(1920, 1080, 30);
        devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            if (isInDebugMode) Debug.Log("No devices cameras found");
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (isInDebugMode) Debug.Log("Device Name: " + devices[i].name);
        }
  
    }

    private void Start()
    {
        pa = GetComponent<ProcAmp>();
        isInDebugMode = pa.isInDebugMode;
    }

    void OnDestroy()
    {
        if (webcamTexture.isPlaying) webcamTexture.Stop();
    }

    public IEnumerator RunWebcam()
    {
        yield return new WaitForEndOfFrame();
        if (devices.Length > 0)
        {
            //ri.texture = webcamTexture;
            //ri.material.mainTexture = webcamTexture;
            webcamTexture.Play();
            if(isInDebugMode) Debug.Log("webcam size: " + webcamTexture.width + " , " + webcamTexture.height);
        }

        if (webcamTexture.isPlaying)
        {
            pa.webcamTexture = webcamTexture;
               
                
        }

    }

    public void ShutDownWebcam()
    {
        if (webcamTexture.isPlaying) webcamTexture.Stop();
    }

       
}

