using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System.IO;
using System.Collections;
using DoozyUI;

using ZXing;
using ZXing.QrCode;
using Moments;

namespace Klak.Video
{
    [ExecuteInEditMode]
    public class ProcAmp : MonoBehaviour
    {
        public WebCamTexture webcamTexture;
        #region Video source properties


        //[SerializeField] VideoPlayer _sourceVideo;

        //public VideoPlayer sourceVideo {
        //    get { return _sourceVideo; }
        //    set { _sourceVideo = value; }
        //}

        //[SerializeField] Texture _webcamTex;
        //public Texture webcamTex {
        //    get { return _webcamTex; }
        //    set { _webcamTex = value; }
        //}


        [SerializeField] Texture _sourceTexture;

        public Texture sourceTexture {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        #endregion

        #region Basic adjustment properties

        [SerializeField, Range(-1, 1)] float _brightness = 0;

        public float brightness {
            get { return _brightness; }
            set { _brightness = value; }
        }

        [SerializeField, Range(-1, 2)] float _contrast = 1;

        public float contrast {
            get { return _contrast; }
            set { _contrast = value; }
        }

        [SerializeField, Range(0, 2)] float _saturation = 1;

        public float saturation {
            get { return _saturation; }
            set { _saturation = value; }
        }

        #endregion

        #region Color balance properties

        [SerializeField, Range(-1, 1)] float _temperature = 0;

        public float temperature {
            get { return _temperature; }
            set { _temperature = value; }
        }


        [SerializeField, Range(-1, 1)] float _tint = 0;

        public float tint {
            get { return _tint; }
            set { _tint = value; }
        }

        #endregion

        #region Keying properties

        [SerializeField] bool _keying;

        public bool keying {
            get { return _keying; }
            set { _keying = value; }
        }

        [SerializeField, ColorUsage(false)] Color _keyColor = Color.green;

        public Color keyColor {
            get { return _keyColor; }
            set { _keyColor = value; }
        }

        [SerializeField, Range(0, 1)] float _keyThreshold = 0.5f;

        public float keyThreshold {
            get { return _keyThreshold; }
            set { _keyThreshold = value; }
        }

        [SerializeField, Range(0, 1)] float _keyTolerance = 0.2f;

        public float keyTolerance {
            get { return _keyTolerance; }
            set { _keyTolerance = value; }
        }

        [SerializeField, Range(0, 1)] float _spillRemoval = 0.5f;

        public float spillRemoval {
            get { return _spillRemoval; }
            set { _spillRemoval = value; }
        }

        #endregion

        #region Transform properties

        [SerializeField] Vector4 _trim = Vector4.zero;

        public Vector4 trim {
            get { return _trim; }
            set { _trim = value; }
        }

        [SerializeField] Vector2 _scale = Vector2.one;

        public Vector2 scale {
            get { return _scale; }
            set { _scale = value; }
        }

        [SerializeField] Vector2 _offset = Vector2.zero;

        public Vector2 offset {
            get { return _offset; }
            set { _offset = value; }
        }

        #endregion

        #region Final tweak properties

        [SerializeField] Color _fadeToColor = Color.clear;

        public Color fadeToColor {
            get { return _fadeToColor; }
            set { _fadeToColor = value; }
        }

        [SerializeField, Range(0, 1)] float _opacity = 1;

        public float opacity {
            get { return _opacity; }
            set { _opacity = value; }
        }

        #endregion

        #region Destination properties

        [SerializeField] RenderTexture _targetTexture;

        public RenderTexture targetTexture {
            get { return _targetTexture; }
            set { _targetTexture = value; }
        }

        [SerializeField] RawImage _targetImage;

        public RawImage targetImage {
            get { return _targetImage; }
            set { _targetImage = value; }
        }

        [SerializeField] bool _blitToScreen = true;

        public bool blitToScreen {
            get { return _blitToScreen; }
            set { _blitToScreen = value; }
        }

        #endregion

        #region Public utility functions (shared with the editor code)

        // YCgCo color space conversion
        public static Vector3 RGB2YCgCo(Color rgb)
        {
            var y  =  0.25f * rgb.r + 0.5f * rgb.g + 0.25f * rgb.b;
            var cg = -0.25f * rgb.r + 0.5f * rgb.g - 0.25f * rgb.b;
            var co =  0.50f * rgb.r                - 0.50f * rgb.b;
            return new Vector3(y, cg, co);
        }

        // An analytical model of chromaticity of the standard illuminant, by Judd et al.
        // http://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D
        // Slightly modifed to adjust it with the D65 white point (x=0.31271, y=0.32902).
        public static float StandardIlluminantY(float x)
        {
            return 2.87f * x - 3.0f * x * x - 0.27509507f;
        }

        // CIE xy chromaticity to CAT02 LMS.
        // http://en.wikipedia.org/wiki/LMS_color_space#CAT02
        public static Vector3 CIExyToLMS(float x, float y)
        {
            var Y = 1.0f;
            var X = Y * x / y;
            var Z = Y * (1.0f - x - y) / y;

            var L =  0.7328f * X + 0.4296f * Y - 0.1624f * Z;
            var M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
            var S =  0.0030f * X + 0.0136f * Y + 0.9834f * Z;

            return new Vector3(L, M, S);
        }

        // Calculate the color balance coefficients.
        public static Vector3 CalculateColorBalance(float temp, float tint)
        {
            // Get the CIE xy chromaticity of the reference white point.
            // Note: 0.31271 = x value on the D65 white point
            var x = 0.31271f - temp * (temp < 0.0f ? 0.1f : 0.05f);
            var y = StandardIlluminantY(x) + tint * 0.05f;

            // Calculate the coefficients in the LMS space.
            var w1 = new Vector3(0.949237f, 1.03542f, 1.08728f); // D65 white point
            var w2 = CIExyToLMS(x, y);
            return new Vector3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);
        }

        #endregion

        #region Private members

        [SerializeField] Shader _shader;
        [SerializeField] Mesh _quadMesh;
        [SerializeField] Texture _offlineTexture;

        Material _material;
        RenderTexture _buffer;

        bool isImageEffect {
            get { return GetComponent<Camera>() != null; }
        }

        Texture currentSource {
            get {
                if (Application.isPlaying)
                    if (webcamTexture != null)
                    {
                        
                        //return _sourceVideo.texture;
                        return webcamTexture;
                    }
                       
                    else
                        return _sourceTexture;
                else
                    return _offlineTexture;
            }
        }

        [SerializeField] Texture[] backgroundSource;


        void UpdateMaterialProperties(int index)
        {
            // Input
            _material.SetTexture("_MainTex", currentSource);
            _material.SetTexture("_BGTex", backgroundSource[index]);

            // Basic adjustment
            _material.SetFloat("_Brightness", _brightness);
            _material.SetFloat("_Contrast", _contrast);
            _material.SetFloat("_Saturation", _saturation);

            // Color balance
            var balance = CalculateColorBalance(_temperature, _tint);
            _material.SetVector("_ColorBalance", balance);

            // Keying
            if (_keying)
            {
                _keyColor = pickedColor;
                var ycgco = RGB2YCgCo(_keyColor);
                _material.SetVector("_KeyCgCo", new Vector2(ycgco.y, ycgco.z));
                _material.SetFloat("_KeyThreshold", _keyThreshold);
                _material.SetFloat("_KeyTolerance", _keyTolerance);
                _material.SetFloat("_SpillRemoval", _spillRemoval);
                _material.EnableKeyword("_KEYING");
            }
            else
            {
                _material.DisableKeyword("_KEYING");
            }

            // Transform
            _material.SetVector("_TrimParams", new Vector4(
                _trim.x, _trim.w,
                1 / (1 - _trim.x - _trim.z),
                1 / (1 - _trim.y - _trim.w)
            ));
            _material.SetVector("_Scale", _scale);
            _material.SetVector("_Offset", _offset);

            // Final tweaks
            _material.SetVector("_FadeToColor", _fadeToColor);
            _material.SetFloat("_Opacity", _opacity);

            // Blend mode
            if (_keying || _opacity < 1)
            {
                _material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                _material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            }
            else
            {
                _material.SetInt("_SrcBlend", (int)BlendMode.One);
                _material.SetInt("_DstBlend", (int)BlendMode.Zero);
            }
        }

        #endregion

        #region MonoBehaviour functions


        string fileCreateTime;
        string encodedFileName;
        string photoFilePath;
        string qrCodeFilePath;
        string gifFilepath;
        string gifFilepath_withIP;
        string qrcodeURLPath;
        string qrcodeURLPath_withIP;
        Texture2D generatedQRTexture;

        string photoTargetFolder;
        string qrcodeTargetFolder;
        string gifTargetFolder;
        public int userPickedIndex;
        public bool isEnterActiveCamera = false;
        public Image countdownImage;
        public Sprite[] countdownTextures;
        public Image shootImage;
        public Sprite shootTexture_camera;
        public Sprite shootTextrue_video;
        public Image processCircleImage;
        public Image gifloadingImage;



        Texture2D myTexture;
        public RawImage shotRawImage;
        public RawImage finalRawImage;
        public RawImage qrcodeRawImage;
       

        public UIElement[] UIE;
        
        /*
         * UIE-0:  MenuBG
         *    -1:  Instruction
         *    -2:  Active Camera
         *    -3:  Countdown
         *    -4:  Photo Part
         *    -5:  Success
         *    -6:  Final Photo
         *    -7:  QR Code
         *    -8:  Email Page
         *    -9:  End page
         *    -10: Color Adjustment
         *    -11: Photo ot Gif
        */
        public UIButton[] UIB;
        /*
         * UIB-0:  Open Instruction
         *    -1:  Option 1
         *    -2:  Option 2
         *    -3:  Option 3
         *    -4:  Quit Instruction
         *    -5:  Last BG
         *    -6:  Next BG
         *    -7:  Shoot
         *    -8:  Retake Photo
         *    -9:  Save Photo
         *    -10: Home
         *    -11: Email Me
         *    -12: Send Email
         *    -13: Ending Home
         *    -14: Choose Photo
         *    -15: Choose Gif
        */

        public float countdownTimer;
        bool isCountdownTimerStarted = false;
        public float waitTime = 3;

        public float gifTimer;
        bool isStartRecording = false;
        public float gifRecordTime;

        bool isTimerReachTwo = false;
        bool isTimerReachOne = false;
        bool isTimerReachZero = false;
        bool IsReadyToTakePhoto = false;

        public bool isGameTimerStarted = false;
        public float currentGameTimer;
        public float idleGameTime = 180;
        string serverPath;
        string ipAddress;

        [HideInInspector] public Color pickedColor;

        public Slider brightnessSlider;
        public Slider contrastSlider;
        public Slider saturationSlider;
        public Slider tempreatureSlider;
        public Slider tintSlider;
        public Slider keyThresholdSlider;
        public Slider keyToleranceSlider;
        public Slider spillRemovalSlider;
        public Image colorIndicatorImage;
        public Toggle keyingToggle;

        int photoOrGif; //photo: 0    video: 1
       

        public bool isEnterColorAdjustment = false;

        Recorder m_recorder;
        UniGifTest m_uniGifTest;

        AnimationSequence m_animationSequence;
        void Start()
        {
            m_recorder = GetComponent<Recorder>();
            m_uniGifTest = GetComponent<UniGifTest>();
            m_animationSequence = GetComponent<AnimationSequence>();
            gifRecordTime = m_recorder.m_BufferSize;  //Record time

            keyingToggle.isOn = true;
            //targetFolder = @"Assets\saved\";
            serverPath = "C:\\xampp\\htdocs\\";
            ipAddress = GetComponent<DBUploader>().ipAddress;
            photoTargetFolder = "PhotoBooth\\PB_images\\";
          
            qrcodeTargetFolder = "PhotoBooth\\PB_qrcodes\\";
         
            gifTargetFolder = "PhotoBooth\\PB_gifs\\";

            LoadPlayerPrefs();

        }

        bool isShot = false;

        public void ShotorNot()
        {
            isShot = true;
            
        }
        bool isCameraOpened = false;

        void Update()
        {           
            if (isEnterColorAdjustment)
            {
                EditKeyingParams();
            }
            //timer start
            if (isCountdownTimerStarted)
            {
                countdownTimer -= Time.deltaTime;
               
                if (countdownTimer < 2 && !isTimerReachTwo)
                {
                    //Debug.Log("timer < 2");
                    isTimerReachTwo = true;
                    countdownImage.sprite = countdownTextures[1];

                }
                if (countdownTimer < 1 && !isTimerReachOne)
                {
                    //Debug.Log("timer < 1");
                    isTimerReachOne = true;
                    countdownImage.sprite = countdownTextures[0];

                }
                if (countdownTimer < 0 && !isTimerReachZero)
                {
                    //Debug.Log("timer < 0");
                    isTimerReachZero = true;
                    isCountdownTimerStarted = false;
                    UIE[3].Hide(false);  //UIE_Countdown
                    IsReadyToTakePhoto = true;
                }
            }

            if (isGameTimerStarted)
            {
                currentGameTimer += Time.deltaTime;

                if (currentGameTimer > idleGameTime)
                {
                    ResetTheGame();
                }
            }




            if (isEnterActiveCamera )
            {
                if (Input.GetKeyDown("s") && !isShot)
                {
                    //show color adjustment panel

                    ColorAdjustmentControl();
                    

                }
                if (!isCameraOpened)
                {
                    isCameraOpened = true;
                    StartCoroutine(GetComponent<PlayWebcam>().RunWebcam());

                }

                if (isStartRecording)
                {
                    gifTimer -= Time.deltaTime;
                    processCircleImage.fillAmount = Mathf.Clamp(1 - gifTimer / gifRecordTime, 0, 1); 

                    if (gifTimer < 0)
                    {
                        isStartRecording = false;
                        StartCoroutine(ShowGif());
                    }

                }


                // Material instantiation.
                if (_material == null)
                {
                    _material = new Material(_shader);
                    _material.hideFlags = HideFlags.DontSave;
                }

                // Material update.
                UpdateMaterialProperties(userPickedIndex);

                // Release previous frames.
                if (_buffer != null) RenderTexture.ReleaseTemporary(_buffer);
                _buffer = null;

                // Do nothing here if image effect mode.
                if (isImageEffect) return;

                // Do nothing if no source is given.
                var source = currentSource;
                if (source == null) return;

                // Determine the destination.
                var dest = _targetTexture;
                if (dest == null)
                {
                    if (_targetImage == null) return; // No target, do nothing.
                                                      // Allocate an internal temporary buffer.
                    _buffer = RenderTexture.GetTemporary(source.width, source.height);
                    dest = _buffer;
                }

                // Invoke the ProcAmp shader.
                Graphics.Blit(source, dest, _material, 0);

                // Update the UI image target.
                if (_targetImage != null) _targetImage.texture = dest;



                if (IsReadyToTakePhoto)
                {
                    IsReadyToTakePhoto = false;
                    if (photoOrGif == 0)
                    {
                        TakeAndShowthePhoto();
                    }
                    else
                    {
                        //photoOrVideo = 1 : video
                        RecordGIF();
                    }
                    
                    
               
                }
            }
            else
            {
                if (webcamTexture != null)
                {
                    GetComponent<PlayWebcam>().ShutDownWebcam();
                    isCameraOpened = false;
                }
                    
            }

        }

        void OnDestroy()
        {

            if (_material != null)
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);

            //if (webcamTexture.isPlaying ) webcamTexture.Stop();
        }

        #endregion

        #region main functions: take, save, upload photo, generate qrcode


        public void ChooseShootingPhoto()
        {
            processCircleImage.fillAmount = 0;
            isEnterActiveCamera = true;
            shootImage.sprite = shootTexture_camera;
            photoOrGif = 0;
            isShot = false;
        }

        public void ChooseShootingGIF()
        {
            m_animationSequence.SwitchTargetToShotImage();
            isEnterActiveCamera = true;
            shootImage.sprite = shootTextrue_video;
            photoOrGif = 1;
            processCircleImage.fillAmount = 0;
            isShot = false;
        }


        void TakeAndShowthePhoto()
        {

            //shotRawImage.rectTransform.localScale = new Vector3(1, 1, 1);
            UIB[8].EnableButton();  //retake photo button
            UIB[9].EnableButton();  //save photo button
           
            
            //display the current photo
            myTexture = new Texture2D(_targetImage.texture.width, _targetImage.texture.height, TextureFormat.ARGB32, false);
            myTexture.ReadPixels(new Rect(0, 0, _targetImage.texture.width, _targetImage.texture.height), 0, 0, false);
            myTexture.Apply();
            shotRawImage.texture = myTexture;
            finalRawImage.texture = myTexture;
            UIE[2].Hide(false); //UIE_ActiveCamera
            UIE[4].Show(false); //UIE_PhotoPart

            //Debug.Log(myTexture.format);

        }
       
        void RecordGIF()
        {
            CreateEncodedName();
            gifTimer = gifRecordTime;
            isStartRecording = true;
            gifFilepath = serverPath + gifTargetFolder + "PB_" + encodedFileName + ".gif";
            qrcodeURLPath = gifTargetFolder + "PB_" + encodedFileName + ".gif";

            gifFilepath_withIP = (ipAddress + gifTargetFolder + "PB_" + encodedFileName + ".gif").Replace("\\", "/");
            GetComponent<Record>().StartRecordingGif(gifFilepath, encodedFileName);
        }

        public void StopGifLoop()
        {
            if (m_animationSequence.m_Playing)
            {
                m_animationSequence.Stop();
            }
           
        }

        bool isStartLoadGif = false;

        FileInfo fileinfo;
        
        IEnumerator ShowGif()
        {
            while(m_recorder.State != RecorderState.finishedRecording)
            {
                yield return null;
            }

            Debug.Log("in show gif, after finished recording");
            m_animationSequence.m_AnimTextures = m_recorder.m_Frames_tex_array;
            //shotRawImage.rectTransform.localScale = new Vector3(m_recorder.m_Width / 1920, m_recorder.m_Height / 1080, 1);
            Debug.Log(m_recorder.m_Width / 1920 + "       " + m_recorder.m_Height / 1080);
            m_animationSequence.PlayLoop();
            UIE[2].Hide(false); //UIE_ActiveCamera
            UIE[4].Show(false); //UIE_PhotoPart


            UIB[8].EnableButton();  //retake photo button
            UIB[9].EnableButton();  //save photo button

        }


        IEnumerator ShowSaveGifStatus()
        {

            while (m_recorder.State != RecorderState.Paused)
            {
                Debug.Log("inside pause");
                
                yield return null;
            }

            Debug.Log("after showgif - state != paused     " + gifFilepath);
            Debug.Log("see if existed" + File.Exists(gifFilepath));

            while (!File.Exists(gifFilepath))
            {
                Debug.Log("not exist");
                yield return null;
            }

            Debug.Log(" exist !!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            fileinfo = new FileInfo(gifFilepath);

            while (IsFileLocked(fileinfo))
            {
                Debug.Log("locked");
                yield return null;
            }

            Debug.Log(" not locked !!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            gifloadingImage.GetComponent<CanvasGroup>().alpha = 0;

            m_animationSequence.PlayLoop();


            UIB[10].EnableButton();  //home button
            UIB[11].EnableButton();  //email me button

            //UploadImage(myTextureBytes);
            GenerateTheQRCode();
            UpdateDB();

            byte[] myGifBytes = File.ReadAllBytes(gifFilepath);
            UploadGIF(myGifBytes);

            #region reload gif to scene
            //if (m_recorder.State == RecorderState.Paused)
            //{                      
            //    Debug.Log("start loading");
            //    isStartLoadGif = true;
            //    m_uniGifTest.StartLoadGif(gifFilepath_withIP);

            //}

            //while (m_uniGifTest.m_uniGifImage.nowState == UniGifImage.State.Loading)
            //{
            //    gifloadingImage.GetComponent<CanvasGroup>().alpha = 1;
            //    Debug.Log("unigif loading");
            //    yield return null;
            //}

            //if (m_uniGifTest.m_uniGifImage.nowState == UniGifImage.State.Ready)
            //{
            //    Debug.Log("state = play");
            //    gifloadingImage.GetComponent<CanvasGroup>().alpha = 0;
            //    UIE[2].Hide(false); //UIE_ActiveCamera
            //    UIE[4].Show(false); //UIE_PhotoPart
            //    m_uniGifTest.m_uniGifImage.Play();
            //}
            #endregion


        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public void Retake()
        {
            isShot = false;
            //reset some params
            isStartLoadGif = false;
            processCircleImage.fillAmount = 0;
            //gifloadingImage.GetComponent<CanvasGroup>().alpha = 0;

            m_animationSequence.Stop();

        }

        public void CreateEncodedName()
        {
            fileCreateTime = System.DateTime.Now.ToString("yyMMddHHmmssff");
            encodedFileName = GetComponent<CreateMD5>().callEncry(fileCreateTime);
        }

        public void SaveImageToFile()
        {

           
            isEnterActiveCamera = false;
            CreateEncodedName();

            //0: photo 1:gif
            if (photoOrGif == 0)
            {
                //shooting photo
                photoFilePath = serverPath + photoTargetFolder + "PB_" + encodedFileName + ".png";
                qrcodeURLPath = photoTargetFolder + "PB_" + encodedFileName + ".png";
                FileStream file = File.Open(photoFilePath, FileMode.Create);
                byte[] myTextureBytes = myTexture.EncodeToPNG();
                BinaryWriter binary = new BinaryWriter(file);
                binary.Write(myTextureBytes);
                file.Close();
                Debug.Log(System.DateTime.Now + "   save image to file:  " + photoFilePath);

                UploadImage(myTextureBytes);
                GenerateTheQRCode();
                UpdateDB();

                UIB[10].EnableButton();  //home button
                UIB[11].EnableButton();  //email me button
            }
            else
            {

                gifloadingImage.GetComponent<CanvasGroup>().alpha = 1;
                m_animationSequence.Stop();
                m_animationSequence.SwitchTargetToFinalShotImage();
                //gif path and name already created in RecordGif()
                m_recorder.ProcessTheGif();  // save to folder
                StartCoroutine(ShowSaveGifStatus());


            }

         
            


        }
   
        public void HideColorAdjustment()
        {
            if (isEnterColorAdjustment)
            {
                Debug.Log("quit picking color");
                isEnterColorAdjustment = false;
                UIE[10].Hide(false);
                StartGameTimer();
            }
           
        }

        void GenerateTheQRCode()
        {
            qrCodeFilePath = serverPath + qrcodeTargetFolder + " QR_" + encodedFileName + ".png";

            if (photoOrGif == 0)
            {
                qrcodeURLPath_withIP = ipAddress + qrcodeURLPath.Replace("\\", "/");
                generatedQRTexture = GetComponent<GenerateQRCode>().generateQR(qrcodeURLPath_withIP, qrCodeFilePath);
                
            }
            else
            {
                generatedQRTexture = GetComponent<GenerateQRCode>().generateQR(gifFilepath_withIP, qrCodeFilePath);
               
            }
            qrcodeRawImage.texture = generatedQRTexture;

        }

        void UpdateDB()
        {
            if (photoOrGif == 0)
            {
                StartCoroutine(GetComponent<DBUploader>().UpdateDB(qrcodeURLPath.Replace("\\", "/"), 0));
            }
            else
            {
                StartCoroutine(GetComponent<DBUploader>().UpdateDB(qrcodeURLPath.Replace("\\", "/"), 1));
            }
        }

        void UploadImage(byte[] myTextureBytes)
        {
            string filename = "PB_" + encodedFileName + ".png";
            StartCoroutine(GetComponent<FileUploader>().UploadPNG(myTextureBytes, filename));
        }

        void UploadGIF(byte[] myGIFBytes)
        {
            string filename = "PB_" + encodedFileName + ".gif";
            StartCoroutine(GetComponent<FileUploader>().UploadGIF(myGIFBytes, filename));
        }

        public InputField emailInputfield;

        public void SendEmail()
        {
            string filename = "";
            string filepath = "";
            if (photoOrGif == 0)
            {
                filepath = "PB_UploadedImages\\PB_" + encodedFileName + ".png";
                filename = "PB_" + encodedFileName + ".png"; ;
            }
            else
            {
                filepath = "PB_UploadedGifs\\PB_" + encodedFileName + ".gif";
                filename = "PB_" + encodedFileName + ".gif"; ;

            }
            GetComponent<EmailSender>().SendEmail(filepath, filename);

        }

        #endregion

        #region color adjustment with playerprefs

        void LoadPlayerPrefs()
        {
            //player prefs
            //pickedColor
            if (PlayerPrefs.HasKey("pickedColorR"))
            {
                pickedColor.r = PlayerPrefs.GetFloat("pickedColorR");
            }
            if (PlayerPrefs.HasKey("pickedColorG"))
            {
                pickedColor.g = PlayerPrefs.GetFloat("pickedColorG");
            }
            if (PlayerPrefs.HasKey("pickedColorB"))
            {
                pickedColor.b = PlayerPrefs.GetFloat("pickedColorB");
            }
            _keyColor.r = pickedColor.r;
            _keyColor.g = pickedColor.g;
            _keyColor.b = pickedColor.b;
            _keyColor.a = pickedColor.a = 1;
            colorIndicatorImage.color = pickedColor;



            //params
            if (PlayerPrefs.HasKey("brightness"))
            {
                _brightness = brightnessSlider.value = PlayerPrefs.GetFloat("brightness");
            }
            if (PlayerPrefs.HasKey("contrast"))
            {
                _contrast = contrastSlider.value = PlayerPrefs.GetFloat("contrast");
            }
            if (PlayerPrefs.HasKey("saturation"))
            {
                _saturation = saturationSlider.value = PlayerPrefs.GetFloat("saturation");
            }
            if (PlayerPrefs.HasKey("temperature"))
            {
                _temperature = tempreatureSlider.value = PlayerPrefs.GetFloat("temperature");
            }
            if (PlayerPrefs.HasKey("tint"))
            {
                _tint = tintSlider.value = PlayerPrefs.GetFloat("tint");
            }
            if (PlayerPrefs.HasKey("keyThresold"))
            {
                _keyThreshold = keyThresholdSlider.value = PlayerPrefs.GetFloat("keyThresold");

            }
            if (PlayerPrefs.HasKey("keyTolerance"))
            {
                _keyTolerance = keyToleranceSlider.value = PlayerPrefs.GetFloat("keyTolerance");
            }
            if (PlayerPrefs.HasKey("spillRemoval"))
            {
                _spillRemoval = spillRemovalSlider.value = PlayerPrefs.GetFloat("spillRemoval");
            }


        }


       

        public void ColorAdjustmentControl()
        {
            //show color adjustment panel
            if (!isEnterColorAdjustment)
            {
                //Debug.Log("start to pick color");
                isEnterColorAdjustment = true;
                UIE[10].Show(false);
                StopGameTimer();

            }
            else
            {
                //Debug.Log("quit picking color");
                isEnterColorAdjustment = false;
                UIE[10].Hide(false);
                StartGameTimer();
            }
            
        }

        void EditKeyingParams()
        {
            _keying = keyingToggle.isOn;
            _keyColor = pickedColor;
            _brightness = brightnessSlider.value;
            _contrast = contrastSlider.value;
            _saturation = saturationSlider.value;
            _temperature = tempreatureSlider.value;
            _tint = tintSlider.value;
            _keyThreshold = keyThresholdSlider.value;
            _keyTolerance = keyToleranceSlider.value;
            _spillRemoval = spillRemovalSlider.value;

        }

        void OnDisable()
        {
            pickedColor = _keyColor;
            PlayerPrefs.SetFloat("pickedColorR", pickedColor.r);
            PlayerPrefs.SetFloat("pickedColorG", pickedColor.g);
            PlayerPrefs.SetFloat("pickedColorB", pickedColor.b);

            PlayerPrefs.SetFloat("brightness", _brightness);
            PlayerPrefs.SetFloat("contrast", _contrast);
            PlayerPrefs.SetFloat("saturation", _saturation);
            PlayerPrefs.SetFloat("temperature", _temperature);
            PlayerPrefs.SetFloat("tint", _tint);
            PlayerPrefs.SetFloat("keyThresold", _keyThreshold);
            PlayerPrefs.SetFloat("keyTolerance", _keyTolerance);
            PlayerPrefs.SetFloat("spillRemoval", _spillRemoval);


        }

        #endregion

        #region tiny functions

        public void SelectBackground(int index)
        {
            //isEnterActiveCamera = true;
            userPickedIndex = index;
        }


        public void SwitchToNextBG()
        {
            if (userPickedIndex != backgroundSource.Length - 1)
            {
                userPickedIndex += 1;
            }
            else
            {
                userPickedIndex = 0;
            }
            _material.SetTexture("_BGTex", backgroundSource[userPickedIndex]);
        }

        public void SwitchToLastBG()
        {
            if (userPickedIndex != 0)
            {
                userPickedIndex -= 1;
            }
            else
            {
                userPickedIndex = backgroundSource.Length - 1;
            }
            _material.SetTexture("_BGTex", backgroundSource[userPickedIndex]);
        }


        public void StartCountDown()
        {
            countdownTimer = waitTime;
            countdownImage.sprite = countdownTextures[2];
            isTimerReachZero = false;
            isTimerReachOne = false;
            isTimerReachTwo = false;
            isCountdownTimerStarted = true;

        }

        public void StartGameTimer()
        {
            isGameTimerStarted = true;
        }

        public void StopGameTimer()
        {
            isGameTimerStarted = false;
            currentGameTimer = 0;
        }

        public void ResetGameTimer()
        {
            currentGameTimer = 0;
        }

        public void DisableThisButton(GameObject gameobject)
        {
            gameobject.GetComponent<Button>().interactable = false;
        }

        public void EnableThisButton(GameObject gameobject)
        {
          
            gameobject.GetComponent<Button>().interactable = true;
            
        }

       
        void ResetTheGame()
        {
            isGameTimerStarted = false;
            currentGameTimer = 0;
            if (m_animationSequence.m_Playing)
            {
                m_animationSequence.Stop();
            }
            //reset UIE
            for (int i = 1; i < UIE.Length; i++)
            {
                if (UIE[i].GetComponent<Canvas>().enabled == true)   //shown
                {
                    UIE[i].Show(true);
                    UIE[i].Hide(false);
                }              
                
            }

             UIE[0].Show(false);

            //reset UIB
            for (int j = 4; j < UIB.Length; j++)
            {
                UIB[j].DisableButton();
            }

            for (int m = 0; m < 4; m++)
            {
                UIB[m].EnableButton();
            }

        }

        #endregion

        #region test doozy show hide functions
        public void tryShow(UIElement go)
        {
            Debug.Log(go.elementName +  go.elementCategory);
            //UIManager.HideUiElement(go.elementName,  true);
            go.Show(true);
        }

        public void tryHide(UIElement go)
        {
            Debug.Log(go.elementName + go.elementCategory);
            //UIManager.HideUiElement(go.elementName,  true);
            go.Show(true);
            go.Hide(false);
        }

        #endregion


        #region rubbish


        //void OnRenderObject()
        //{

        //    if (!_blitToScreen || isImageEffect || _material == null) return;

        //    // Use the simple blit pass when we already have a processed image.
        //    var processed = _buffer != null ? _buffer : _targetTexture;
        //    if (processed != null)
        //    {
        //        _material.SetTexture("_MainTex", processed);
        //        _material.SetPass(2);
        //    }
        //    else
        //    {
        //        // Blit with ProcAmp pass
        //        _material.SetTexture("_MainTex", currentSource);
        //        _material.SetPass(1);
        //    }

        //    Graphics.DrawMeshNow(_quadMesh, Matrix4x4.identity);
        //}

        //void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{
        //    var video = currentSource;
        //    if (video != null && _material != null)
        //    {
        //        // Coefficients for aspect ratio conversion.
        //        var screenAspect = (float)source.height / source.width;
        //        var textureAspect = (float)video.height / video.width;
        //        var aspectConv = screenAspect / textureAspect;
        //        if (aspectConv > 1)
        //            _material.SetVector("_AspectConv", new Vector2(1, aspectConv));
        //        else
        //            _material.SetVector("_AspectConv", new Vector2(1 / aspectConv, 1));

        //        // Composite with the source.
        //        _material.SetTexture("_BaseTex", source);
        //        Graphics.Blit(video, destination, _material, 3);
        //    }
        //    else
        //    {
        //        // Do nothing because the video source is not ready.
        //        Graphics.Blit(source, destination);
        //    }
        //}

        #endregion


    }


    
}
