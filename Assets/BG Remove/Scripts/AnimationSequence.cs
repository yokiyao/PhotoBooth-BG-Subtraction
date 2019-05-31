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

    public List<Texture> m_BG_AnimTextures_1;
    public List<Texture> m_BG_AnimTextures_2;
    public List<Texture> m_BG_AnimTextures_3;

    public List<List<Texture>> m_BGTexureList;

    private WaitForSeconds m_FrameRateWait;                         // The delay between frames.
    private int m_CurrentTextureIndex;                              // The index of the textures array.
    public bool m_Playing;                                         // Whether the textures are currently being looped through.

    public RawImage shotImageRawImage;
    public RawImage finalShotImageRawImage;
    RawImage targetRawimage;

    public RawImage backgroundRawImage;
    public Texture textureAtThisFrame;

    public bool m_Playing_AssignToTexture;
    int m_CurrentTextureIndex_AssignToTexture;
    WaitForSeconds m_FrameRateWait_AssignToTexture;
    int m_FrameRate_AssignTotexture = 30;


    private void Awake()
    {
        //Texture = GetComponent<RawImage>();
        // The delay between frames is the number of seconds (one) divided by the number of frames that should play during those seconds (frame rate).
        m_FrameRateWait = new WaitForSeconds(1f / m_FrameRate);
        m_FrameRate = (int) GetComponent<Recorder>().m_BufferSize;
        m_BGTexureList = new List<List<Texture>>();
        m_BGTexureList.Add(m_BG_AnimTextures_1);
        m_BGTexureList.Add(m_BG_AnimTextures_2);
        m_BGTexureList.Add(m_BG_AnimTextures_3);

        //assign to textrue 
        m_FrameRateWait_AssignToTexture = new WaitForSeconds(1f / m_FrameRate_AssignTotexture);
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



    /// ////////////////////////////////////////////////////////////////assign to textrue only

    public void PlayLoop_AssignToTexture(int id)
    {
        m_Playing_AssignToTexture = true;
        m_CurrentTextureIndex_AssignToTexture = 0;
        StopAllCoroutines();
        StartCoroutine(PlayTextures_AssignToTexture(id));
    }

    private IEnumerator PlayTextures_AssignToTexture(int id)
    {
        // So long as the textures should be playing...
        while (m_Playing_AssignToTexture)
        {

            // Set the texture to the texture indicated by the index of the textures array.
            textureAtThisFrame = m_BGTexureList[id][m_CurrentTextureIndex_AssignToTexture];
            //Texture.SetNativeSize();
            m_CurrentTextureIndex_AssignToTexture = (m_CurrentTextureIndex_AssignToTexture + 1) % m_BGTexureList[id].Count;

            yield return m_FrameRateWait_AssignToTexture;
        }

    }

    public void Stop_AssignToTexture()
    {
        //StartCoroutine(WaitandStop());
        m_Playing_AssignToTexture = false;

    }



    IEnumerator WaitandStop()
    {
        yield return new WaitForSeconds(1f);
        m_Playing = false;
        targetRawimage.texture = m_AnimTextures[0];
    }
}

