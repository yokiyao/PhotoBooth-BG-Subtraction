using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;


namespace Klak.Video
{


    public class PlayWebcam : MonoBehaviour {

        WebCamTexture webcamTexture;
        public ProcAmp pa;
        public RawImage ri;
        WebCamDevice[] devices;


        void Awake()
        {

            //EDSDK.EdsSendCommand(camera, EDSDK.CameraCommand_TakePicture, 0); 
            //uint err = EDSDK.EdsInitializeSDK();
            //EDSDK.EdsSendCommand(IntPtr.Zero, EDSDK.CameraCommand_TakePicture, 0);

            webcamTexture = new WebCamTexture(1920, 1080, 30);
            devices = WebCamTexture.devices;

            if (devices.Length == 0)
            {
                Debug.Log("No devices cameras found");
                return;
            }

            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log("Device Name: " + devices[i].name);
            }
  
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

            }

            if (webcamTexture.isPlaying)
            {
                pa.webcamTexture = webcamTexture;
                //Debug.Log("webcam size: " + webcamTexture.width + " , " + webcamTexture.height);
                
            }

        }

        public void ShutDownWebcam()
        {
            if (webcamTexture.isPlaying) webcamTexture.Stop();
        }

       
    }

}