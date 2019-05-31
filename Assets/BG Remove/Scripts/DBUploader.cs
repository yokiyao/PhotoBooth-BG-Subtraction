using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Klak.Video;
public class DBUploader : MonoBehaviour {


    string PHP_filePath;
    string ipAddress;
    string PHP_url;
 
    public string status = "";
    ProcAmp pa;
    bool isInDebugMode;
  

    private void Start()
    {
        pa = GetComponent<ProcAmp>();
        ipAddress = pa.ipAddress;
        PHP_filePath = pa.PHP_filePath;
        PHP_url = pa.PHP_url;
        StartCoroutine(CheckConnection());
        isInDebugMode = pa.isInDebugMode;
    
    }



    public IEnumerator CheckConnection()
    {
        status = "";
        WWWForm form = new WWWForm();
        form.AddField("function", "CheckConnection");
        form.AddField("photoFilePath", "testConnection  " + System.DateTime.Now);
        
        
        if (isInDebugMode) Debug.Log("PHP url: " + PHP_url);
        using (var w = new WWW(PHP_url, form))
        {
            yield return w;
            //Debug.Log(w.error);
            if (w.error != null)
            {
                status = "Fail";
                pa.ShowBugPage("Failed to connect to server");
                if (isInDebugMode) Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "      status: " + status + "  error: " + w.error );
                //Debug.Log("not ok");
            }
            else
            {
                status = "Good";
                if (isInDebugMode) Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + w.text + "      status: " + status);
            }
        }
    }

    public IEnumerator UpdateDB(string filePath, int type)
    {
        WWWForm form = new WWWForm();
        form.AddField("dateTime", System.DateTime.Now.ToString());

        //type = 0: photo
        //type = 1: gif
        if (type == 0)
        {
            form.AddField("function", "InsertNewPhotoData");
            form.AddField("photoFilePath", filePath);         
            //Debug.Log("Updating " + filePath + " to Server");
            //form.AddField("filePath", "/images/" + scanTexture.instance.fileName);
        }
        else
        {
            form.AddField("function", "InsertNewGifData");
            form.AddField("gifFilePath", filePath);

        }

        using (var w = new WWW(PHP_url, form))
        {
            yield return w;
            if (w.error != null)
            {
                if (isInDebugMode) Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + w.error);
            }
            else
            {

                if (isInDebugMode) Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + w.text);
            }
        }
    }

}
