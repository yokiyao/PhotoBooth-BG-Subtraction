using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klak.Video
{
    public class ColorPicker : MonoBehaviour {

        ProcAmp pa;

        Texture2D tex;
        Vector3 mpos;

        bool isEnterColorAdjustment;

        bool isInDebugMode;

        private void Start()
        {
            pa = GetComponent<ProcAmp>();
            isInDebugMode = pa.isInDebugMode;
        }

        private void Update()
        {
         
            if (Input.GetMouseButtonDown(1))   //RMC
            {
                if (pa.isEnterColorAdjustment)
                {
                    moonrake();
                }
              
            }
         
        }

        public void moonrake()
        {
            mpos = Input.mousePosition;
            StartCoroutine(sshot());

        }

        Color bla;
        IEnumerator sshot()//:Texture2D
        {

            var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            yield return new WaitForEndOfFrame();
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            bla = tex.GetPixel((int)mpos.x, (int)mpos.y);
            pa.pickedColor =  bla;
            if (isInDebugMode) Debug.Log("get color: " + bla);
        }

    }
}