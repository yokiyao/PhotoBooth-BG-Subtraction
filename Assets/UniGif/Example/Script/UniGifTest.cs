/*
UniGif
Copyright (c) 2015 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UniGifTest : MonoBehaviour
{
    //[SerializeField]
    //private InputField m_inputField;
    [SerializeField]
    public UniGifImage m_uniGifImage;

    private bool m_mutex;

    public void StartLoadGif(string gifpath)
    {
        if (m_mutex || m_uniGifImage == null || string.IsNullOrEmpty(gifpath))
        {
            return;
        }

        m_mutex = true;
        StartCoroutine(ViewGifCoroutine(gifpath));
    }

    private IEnumerator ViewGifCoroutine(string gifpath)
    {
        yield return StartCoroutine(m_uniGifImage.SetGifFromUrlCoroutine(gifpath));
        m_mutex = false;
    }
}