using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FileUploader : MonoBehaviour {

    

    string[] fileInfo;
    


    public IEnumerator UploadPNG(byte[] bytes, string fileName)
    {
        yield return new WaitForEndOfFrame();
        //load file
        //byte[] localFile = File.ReadAllBytes(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "images/" + fileName));
        byte[] localFile = bytes;
        yield return localFile;

        if (!(localFile.Length > 0))
        {
            Debug.Log("localFile length < 0 ");
            yield break; // stop the coroutine here
        }

        WWWForm postForm = new WWWForm();
        postForm.AddField("function", "UploadImage");
        postForm.AddBinaryData("theFile", localFile, fileName, "image/png");

        // Upload to a cgi script
        using (var w = UnityWebRequest.Post(GetComponent<DBUploader>().PHP_url, postForm))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            {
                print(w.error);
            }
            else
            {
                print("Finished Uploading image");
            }
        }

    }

    public IEnumerator UploadGIF(byte[] bytes, string fileName)
    {
        yield return new WaitForEndOfFrame();
        //load file
        //byte[] localFile = File.ReadAllBytes(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "images/" + fileName));
        byte[] localFile = bytes;
        yield return localFile;

        if (!(localFile.Length > 0))
        {
            Debug.Log("localFile length < 0 ");
            yield break; // stop the coroutine here
        }

        WWWForm postForm = new WWWForm();
        postForm.AddField("function", "UploadGIF");
        postForm.AddBinaryData("theFile", localFile, fileName, "image/gif");

        // Upload to a cgi script
        using (var w = UnityWebRequest.Post(GetComponent<DBUploader>().PHP_url, postForm))
        {
            yield return w.SendWebRequest();
            if (w.isNetworkError || w.isHttpError)
            {
                print(w.error);
            }
            else
            {
                print("Finished Uploading gif");
            }
        }

    }



    string[] GetDirectoryInfo(string p)
    {
        string[] fileInfo = Directory.GetFiles(p, "*.*");
        return fileInfo;
    }


}
