using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Moments;
public class AnimationSequence : MonoBehaviour
{

    [SerializeField]
    private int m_FrameRate = 30;
    [SerializeField]
    public List<Texture> m_AnimTextures;

    private WaitForSeconds m_FrameRateWait;                         // The delay between frames.
    private int m_CurrentTextureIndex;                              // The index of the textures array.
    public bool m_Playing;                                         // Whether the textures are currently being looped through.
    public RawImage shotImageRawImage;
    public RawImage finalShotImageRawImage;
    RawImage targetRawimage;
    
    private void Awake()
    {
        //Texture = GetComponent<RawImage>();
        // The delay between frames is the number of seconds (one) divided by the number of frames that should play during those seconds (frame rate).
        m_FrameRateWait = new WaitForSeconds(1f / m_FrameRate);
        m_FrameRate = (int) GetComponent<Recorder>().m_BufferSize;
    }

    void Start()
    {
        // m_Playing = true;
        //PlayLoop();
        //StartCoroutine(PlayTextures());
        targetRawimage = shotImageRawImage;
    }

    public void SwitchTargetToShotImage()
    {
        
        targetRawimage = shotImageRawImage;
 
    }

    public void SwitchTargetToFinalShotImage()
    {

        targetRawimage = finalShotImageRawImage;

    }

    public void PlayAnimation()
    {
        m_CurrentTextureIndex = 0;
        
        StopAllCoroutines();
        StartCoroutine(playAnimation());

    }
    private IEnumerator playAnimation()
    {
       
        while (m_CurrentTextureIndex < m_AnimTextures.Count)
        {

            // Set the texture of the mesh renderer to the texture indicated by the index of the textures array.
            targetRawimage.texture = m_AnimTextures[m_CurrentTextureIndex];

            m_CurrentTextureIndex++;

            yield return m_FrameRateWait;
        }      
    }

    public void PlayLoop()
    {
       
        m_Playing = true;
        m_CurrentTextureIndex = 0;
        StopAllCoroutines();
        StartCoroutine(PlayTextures());
    }
    private IEnumerator PlayTextures()
    {
        // So long as the textures should be playing...
        while (m_Playing)
        {

            // Set the texture of the mesh renderer to the texture indicated by the index of the textures array.
            targetRawimage.texture = m_AnimTextures[m_CurrentTextureIndex];
            //Texture.SetNativeSize();
            m_CurrentTextureIndex = (m_CurrentTextureIndex + 1) % m_AnimTextures.Count;

            yield return m_FrameRateWait;
        }
       
    }

    public void Stop()
    {
        //StartCoroutine(WaitandStop());
        m_Playing = false;
       
    }
    
    IEnumerator WaitandStop()
    {
        yield return new WaitForSeconds(1f);
        m_Playing = false;
        targetRawimage.texture = m_AnimTextures[0];
    }
}

