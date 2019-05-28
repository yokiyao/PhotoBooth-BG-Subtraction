using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DBUploader : MonoBehaviour {


    public string PHP_filePath;
    public string ipAddress;
    [HideInInspector]  public string PHP_url;
 
    public string status = "";

    private void Awake()
    {
        StartCoroutine(CheckConnection());

    }

   

    public IEnumerator CheckConnection()
    {
        status = "";
        WWWForm form = new WWWForm();
        form.AddField("function", "CheckConnection");
        form.AddField("filePath", "testConnection  " + System.DateTime.Now);
        
        PHP_url = ipAddress + PHP_filePath;
        Debug.Log("PHP url: " + PHP_url);
        using (var w = new WWW(PHP_url, form))
        {
            yield return w;
            //Debug.Log(w.error);
            if (w.error != null)
            {
                status = "Fail";
                Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + "      status: " + status + "  error: " + w.error );
                //Debug.Log("not ok");
            }
            else
            {
                status = "Good";
                Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + w.text + "      status: " + status);
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
                Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + w.error);
            }
            else
            {
                
                Debug.Log(DateTime.Now.ToString("HH:mm:ss ") + w.text);
            }
        }
    }

}
