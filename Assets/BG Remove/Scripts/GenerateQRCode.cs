using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ZXing;
using ZXing.QrCode;
using Klak.Video;

public class GenerateQRCode : MonoBehaviour {

    ProcAmp pa;
    bool isInDebugMode;
    private void Start()
    {
        pa = GetComponent<ProcAmp>();
        isInDebugMode = pa.isInDebugMode;
    }

    #region generate qrcode
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    public Texture2D generateQR(string url, string qrcodeFilePath)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(url, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        //StartCoroutine(saveQRCode(encoded.EncodeToPNG(), url, qrcodeFilePath));

        return encoded;
    }


    IEnumerator saveQRCode(byte[] bytes, string url, string qrcodeFilePath)
    {

        yield return new WaitForEndOfFrame();
       
        File.WriteAllBytes(qrcodeFilePath, bytes);
        if(isInDebugMode) Debug.Log(System.DateTime.Now + "   save QRCode to file:  " + qrcodeFilePath);
        if(isInDebugMode) Debug.Log("QRcode pointing at:" + url);
    }


    IEnumerator UploadToServer()
    {
        yield return new WaitForEndOfFrame();


    }

    #endregion
}
