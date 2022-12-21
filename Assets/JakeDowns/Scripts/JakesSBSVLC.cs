﻿using UnityEngine;
using System;
using System.Threading.Tasks;
using LibVLCSharp;
//using NRKernal;
using System.Collections.Generic;
//using UnityEngine.Device;
using UnityEngine.UI;
using Application = UnityEngine.Device.Application;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using NRKernal;
using System.Collections;
//using static JakesSBSVLC;
//using static UnityEditor.Experimental.GraphView.GraphView;



public class JakesSBSVLC : MonoBehaviour
{
    [SerializeField]
    public enum VideoMode 
    {
        Mono,
        SBSHalf,
        SBSFull,
        TB,
        _360_2D,
        _180_2D,
        _360_3D,
        _180_3D
    }


    private float nextActionTime = 0.0f;
    public float period = 1.0f; 

    public JakesRemoteController jakesRemoteController;


    LibVLC libVLC;
    public MediaPlayer mediaPlayer;

    AndroidJavaClass _brightnessHelper;

    [SerializeField]
    GameObject NRCameraRig;
    Camera LeftCamera;
    Camera CenterCamera;
    Camera RightCamera;

    [SerializeField]
    public MyIAPHandler myIAPHandler;

    GameObject _hideWhenLocked;
    GameObject _menuToggleButton;
    GameObject _logo;
    public GameObject _360Sphere;
    GameObject _2DDisplaySet;

    GameObject _plane2SphereSet;
    GameObject _plane2SphereLeftEye;
    GameObject _plane2SphereRightEye;

    Vector3 _startPosition;

    Renderer _morphDisplayLeftRenderer;
    Renderer _morphDisplayRightRenderer;

    [SerializeField]
    public UnityEngine.UI.Slider fovBar;

    [SerializeField]
    public Slider nrealFOVBar;

    [SerializeField]
    public UnityEngine.UI.Slider scaleBar;

    [SerializeField]
    public UnityEngine.UI.Slider distanceBar;

    [SerializeField]
    public UnityEngine.UI.Slider deformBar;

    /*[SerializeField]
    public UnityEngine.UI.Slider brightnessBar;

    [SerializeField]
    public UnityEngine.UI.Slider contrastBar;

    [SerializeField]
    public UnityEngine.UI.Slider saturationBar;

    [SerializeField]
    public UnityEngine.UI.Slider hueBar;

    [SerializeField]
    public UnityEngine.UI.Slider gammaBar;

    [SerializeField]
    public UnityEngine.UI.Slider sharpnessBar;*/

    [SerializeField]
    public UnityEngine.UI.Slider horizontalBar;

    [SerializeField]
    public UnityEngine.UI.Slider verticalBar;

    GameObject _cone;
    GameObject _pointLight;

    bool _screenLocked = false;
    int _brightnessOnLock = 0;
    int _brightnessModeOnLock = 0;

    bool _flipStereo = false;

    [SerializeField]
    public VideoMode _videoMode = VideoMode.Mono;// 2d by default

    // Flat Left
    [SerializeField]
    public GameObject leftEye;

    // Flat Right
    [SerializeField]
    public GameObject rightEye;

    // Sphere Left
    [SerializeField]
    public GameObject leftEyeSphere;
    // Sphere Right
    [SerializeField]
    public GameObject rightEyeSphere;

    Renderer m_lRenderer;
    Renderer m_rRenderer;

    Renderer m_l360Renderer;
    Renderer m_r360Renderer;

    public Material m_lMaterial;
    public Material m_rMaterial;
    public Material m_monoMaterial;
    public Material m_leftEyeTBMaterial;
    public Material m_rightEyeTBMaterial;

    public Material m_leftEye360Material;
    public Material m_rightEye360Material;

    // TODO: combine 180 and 360 into 2 materials instead of 4?
    public Material m_leftEye180Material;
    public Material m_rightEye180Material;

    public Material m_3602DSphericalMaterial;
    public Material m_1802DSphericalMaterial;

    /// <summary> The NRInput. </summary>
    [SerializeField]
    private NRInput m_NRInput;

    Texture2D _vlcTexture = null; //This is the texture libVLC writes to directly. It's private.
    public RenderTexture texture = null; //We copy it into this texture which we actually use in unity.

    bool _is360 = false;
    bool _is180 = false;

    float Yaw;
    float Pitch;
    float Roll;

    bool _3DModeLocked = true;

    int _3DTrialPlaybackStartedAt = 0;
    float _MaxTrialPlaybackSeconds = 5.0f; //15.0f;
    bool _isTrialing3DMode = false;

    float _aspectRatio;
    bool m_updatedARSinceOpen = false;

    // TODO: support overriding the current aspect ratio
    float _aspectRatioOverride;

    /// <summary> The previous position. </summary>
    private Vector2 m_PreviousPos;

    float fov = 20.0f; // 20 for 2D 140 for spherical
    float nreal_fov = 20.0f;

    //public string path = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"; //Can be a web path or a local path
    public string path = "https://jakedowns.com/media/sbs2.mp4"; // Render a nice lil SBS and 180 and 360 video that can play when you switch modes

    public bool flipTextureX = false; //No particular reason you'd need this but it is sometimes useful
    public bool flipTextureY = true; //Set to false on Android, to true on Windows

    public bool automaticallyFlipOnAndroid = true; //Automatically invert Y on Android

    public bool playOnAwake = true; //Open path and Play during Awake

    public bool logToConsole = true; //Log function calls and LibVLC logs to Unity console

    AndroidJavaClass unityPlayer;
    AndroidJavaObject activity;
    AndroidJavaObject context;

    //Unity Awake, OnDestroy, and Update functions
    #region unity
    void Awake()
    {
        //Setup LibVLC
        if (libVLC == null)
            CreateLibVLC();

        //Setup Media Player
        CreateMediaPlayer();

#if UNITY_ANDROID            
        if (!Application.isEditor)
        {
            GetContext();



            _brightnessHelper = new AndroidJavaClass("com.jakedowns.BrightnessHelper");
            if (_brightnessHelper == null)
            {
                Debug.Log("error loading _brightnessHelper");
            }
        }
#endif

        Debug.Log($"[VLC] LibVLC version and architecture {libVLC.Changeset}");
        Debug.Log($"[VLC] LibVLCSharp version {typeof(LibVLC).Assembly.GetName().Version}");

        if (fovBar is not null)
        {
            fovBar.value = fov;
        }

        if (nrealFOVBar is not null)
        {
            nrealFOVBar.value = nreal_fov;
        }

        _plane2SphereSet = GameObject.Find("NewDisplay");
        _plane2SphereLeftEye = GameObject.Find("plane2sphereLeftEye");
        _plane2SphereRightEye = GameObject.Find("plane2sphereRightEye");

        _startPosition = new Vector3(
            _plane2SphereSet.transform.position.x,
            _plane2SphereSet.transform.position.y,
            _plane2SphereSet.transform.position.z
        );

        UpdateCameraReferences();
        // init
        OnFOVSliderUpdated();
        OnSplitFOVSliderUpdated();

        _cone = GameObject.Find("CONE_PARENT");
        _pointLight = GameObject.Find("Point Light");

        //_360Sphere = GameObject.Find("SphereDisplay");
        /*if (_360Sphere is null)
            Debug.LogError("SphereDisplay not found");
        else
            _360Sphere.SetActive(false);*/

        //leftEyeSphere = _360Sphere.transform.Find("LeftEye").gameObject;
        //rightEyeSphere = _360Sphere.transform.Find("RightEye").gameObject;
        /*m_l360Renderer = leftEyeSphere.GetComponent<Renderer>();
        m_r360Renderer = rightEyeSphere.GetComponent<Renderer>();*/

        /*_2DDisplaySet = GameObject.Find("SBSDisplay/DisplaySet");*/

        // TODO: extract lockscreen logic into a separate script
        _hideWhenLocked = GameObject.Find("HideWhenScreenLocked");
        _logo = GameObject.Find("logo");
        _menuToggleButton = GameObject.Find("MenuToggleButton");

        //Setup Screen
        /*if (screen == null)
            screen = GetComponent<Renderer>();
        if (canvasScreen == null)
            canvasScreen = GetComponent<RawImage>();*/

        _morphDisplayLeftRenderer = _plane2SphereLeftEye.GetComponent<Renderer>();
        _morphDisplayRightRenderer = _plane2SphereRightEye.GetComponent<Renderer>();

        // read material reference
        //m_lMaterial = _morphDisplayLeftRenderer.material;
        //m_rMaterial = _morphDisplayRightRenderer.material;

        //Automatically flip on android
        if (automaticallyFlipOnAndroid && UnityEngine.Application.platform == RuntimePlatform.Android)
            flipTextureY = !flipTextureY;

        SetVideoModeMono();

        //Play On Start
        if (playOnAwake)
            Open();
    }

    void OnDestroy()
    {
        //Dispose of mediaPlayer, or it will stay in nemory and keep playing audio
        DestroyMediaPlayer();
    }

    void Update()
    {

        if (_isTrialing3DMode && (bool)mediaPlayer?.IsPlaying)
        {
            if (UnityEngine.Time.time > nextActionTime)
            {
                nextActionTime = UnityEngine.Time.time + period;
                CheckTrialExceeded();
            }
        }


#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
        }
#endif
        //Get size every frame
        uint height = 0;
        uint width = 0;
        mediaPlayer?.Size(0, ref width, ref height);

        //Automatically resize output textures if size changes
        if (_vlcTexture == null || _vlcTexture.width != width || _vlcTexture.height != height)
        {
            ResizeOutputTextures(width, height);
        }

        if (_vlcTexture != null)
        {
            //Update the vlc texture (tex)
            var texptr = mediaPlayer.GetTexture(width, height, out bool updated);
            if (updated)
            {
                _vlcTexture.UpdateExternalTexture(texptr);

                //Copy the vlc texture into the output texture, flipped over
                var flip = new Vector2(flipTextureX ? -1 : 1, flipTextureY ? -1 : 1);
                Graphics.Blit(_vlcTexture, texture, flip, Vector2.zero); //If you wanted to do post processing outside of VLC you could use a shader here.
            }
        }
    }
    #endregion

    void OnDisable() 
    {
        DestroyMediaPlayer();
    }

    void OnApplicationQuit()
    {
        DestroyMediaPlayer();
    }

    public void Demo3602D()
    {
        //Open("https://streams.videolan.org/streams/360/eagle_360.mp4");
        Open("https://streams.videolan.org/streams/360/kolor-balloon-icare-full-hd.mp4");
        SetVideoMode3602D();
    }

    public void ToggleSphere()
    {
        _360Sphere.SetActive(!_360Sphere.activeSelf);
    }

    public void ToggleSBSDisplay()
    {
        _2DDisplaySet.SetActive(!_2DDisplaySet.activeSelf);
    }

    public void OnScaleSliderUpdated()
    {
        float newScale = (float)scaleBar.value;
        //_2DDisplaySet.transform.localScale = new Vector3(newScale, newScale, 1.0f);

        _plane2SphereSet.transform.localScale = new Vector3(newScale, newScale, newScale);

        /*_sphereScale = (float)scaleBar.value;
        _360Sphere = GameObject.Find("SphereDisplay");
        Debug.Log("sphere scale " + _sphereScale);
        _360Sphere.transform.localScale = new Vector3(_sphereScale, _sphereScale, _sphereScale);*/
    }

    public void OnDeformSliderUpdated()
    {
        if(deformBar is null)
        {
            return;
        }
        
        float value = (float)deformBar.value;

        if(_plane2SphereLeftEye is not null)
        {
            _plane2SphereLeftEye.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, value);
            _plane2SphereLeftEye.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(1, value);
        }

        if (_plane2SphereRightEye is not null)
        {
            _plane2SphereRightEye.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, value);
            _plane2SphereRightEye.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(1, value);
        }
    }

    float lerpDuration = 1; // TODO: dynamic duration based on startValue
    float startValue = 0;
    float endValue = 10;
    IEnumerator lerpLZero;
    IEnumerator lerpLOne;
    IEnumerator lerpRZero;
    IEnumerator lerpROne;
    //float valueToLerp;

    public void TogglePlaneToSphere()
    {
        float current = _plane2SphereLeftEye.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(0);
        if (current < 50)
        {
            AnimatePlaneToSphere();
        }
        else
        {
            AnimateSphereToPlane();
        }
    }
    public void AnimatePlaneToSphere()
    {
        DoPlaneSphereLerp(100.0f);
    }

    public void AnimateSphereToPlane()
    {
        DoPlaneSphereLerp(0.0f);
    }

    public void DoPlaneSphereLerp(float _endValue)
    {
        if (lerpLZero is not null)
            StopCoroutine(lerpLZero);
                
        if (lerpLOne is not null)
            StopCoroutine(lerpLOne);

        if (lerpRZero is not null)
            StopCoroutine(lerpRZero);
        
        if(lerpROne is not null)
            StopCoroutine(lerpROne);

        endValue = _endValue;

        lerpLZero = LerpPlaneToSphere(_plane2SphereLeftEye.GetComponent<SkinnedMeshRenderer>(), 0);
        lerpLOne = LerpPlaneToSphere(_plane2SphereLeftEye.GetComponent<SkinnedMeshRenderer>(), 1);

        StartCoroutine(lerpLZero);
        StartCoroutine(lerpLOne);

        lerpRZero = LerpPlaneToSphere(_plane2SphereRightEye.GetComponent<SkinnedMeshRenderer>(), 0);
        lerpROne = LerpPlaneToSphere(_plane2SphereRightEye.GetComponent<SkinnedMeshRenderer>(), 1);

        StartCoroutine(lerpRZero);
        StartCoroutine(lerpROne);
    }

    IEnumerator LerpPlaneToSphere(SkinnedMeshRenderer renderer, int ShapeIndex)
    {
        float timeElapsed = 0;
        startValue = renderer.GetBlendShapeWeight(ShapeIndex);
        while (timeElapsed < lerpDuration)
        {
            renderer.SetBlendShapeWeight(ShapeIndex,Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration));
            timeElapsed += UnityEngine.Time.deltaTime;
            yield return null;
        }
        renderer.SetBlendShapeWeight(ShapeIndex, endValue);
    }

    public void OnDistanceSliderUpdated()
    {
        float newDistance = (float)distanceBar.value;
        _plane2SphereSet.transform.localPosition = new Vector3(0.0f, 0.0f, newDistance);
    }

    /* Horizontal (X) axis offset for screen */
    public void OnHorizontalSliderUpdated()
    {
        float newOffset = (float)horizontalBar.value;
        _plane2SphereSet.transform.localPosition = new Vector3(newOffset, 0.0f, 0.0f);
    }

    /* Vertical (Y) axis offset for screen */
    public void OnVerticalSliderUpdated()
    {
        float newOffset = (float)verticalBar.value;
        _plane2SphereSet.transform.localPosition = new Vector3(0.0f, newOffset, 0.0f);
    }

    public void OnFOVSliderUpdated()
    {
        if(fovBar is null)
        {
            Debug.LogWarning("fovBar null");
            return;
        }
        fov = (float)fovBar.value;
        Debug.Log("fov " + fov);

        Do360Navigation();
        //}
    }

    public void UpdateCameraReferences()
    {
        LeftCamera = GameObject.Find("LeftCamera")?.GetComponent<Camera>();
        CenterCamera = GameObject.Find("CenterCamera")?.GetComponent<Camera>();
        RightCamera = GameObject.Find("RightCamera")?.GetComponent<Camera>();
    }

    public void OnSplitFOVSliderUpdated()
    {
        // NOTE: NRSDK doesn't support custom FOV on cameras
        // NOTE: TESTING COMMENTING OUT camera.projectionMatrix = statements in NRHMDPoseTracker
        //return;

        
        UpdateCameraReferences();
        if (nrealFOVBar is null)
        {
            Debug.LogWarning("nrealFOVBar null");
            return;
        }
        
        if(LeftCamera is null || CenterCamera is null || RightCamera is null)
        {
            Debug.LogWarning("camera null " + $" {LeftCamera}, {CenterCamera}, {RightCamera}");
            return;
        }
        Debug.Log("fov before: " + LeftCamera.fieldOfView + ", " + CenterCamera.fieldOfView + ", " + RightCamera.fieldOfView);

        nreal_fov = (float)nrealFOVBar.value;

        LeftCamera.fieldOfView = nreal_fov;
        CenterCamera.fieldOfView = nreal_fov;
        RightCamera.fieldOfView = nreal_fov;

        Debug.Log("fov after: " + LeftCamera.fieldOfView + ", " + CenterCamera.fieldOfView + ", " + RightCamera.fieldOfView);

        Do360Navigation();

        Debug.Log("fov after 360 nav" + LeftCamera.fieldOfView + ", " + CenterCamera.fieldOfView + ", " + RightCamera.fieldOfView);
    }
    public void SetVideoMode1802D()
    {
        SetVideoMode(VideoMode._180_2D);
    }

    public void SetVideoMode3602D()
    {
        SetVideoMode(VideoMode._360_2D);
    }

    public void SetVideoMode1803D()
    {
        SetVideoMode(VideoMode._180_3D);
    }

    public void SetVideoMode3603D()
    {
        SetVideoMode(VideoMode._360_3D);
    }

    public void SetAR4_3()
    {
        if (mediaPlayer is not null)
            mediaPlayer.AspectRatio = "4:3";
    }
    
    public void SetAR169()
    {
        if (mediaPlayer is not null)
            mediaPlayer.AspectRatio = "16:9";
    }

    public void SetAR16_10()
    {
        if (mediaPlayer is not null)
            mediaPlayer.AspectRatio = "16:10";
    }

    public void SetAR_2_35_to_1()
    {
        if (mediaPlayer is not null)
            mediaPlayer.AspectRatio = "2.35:1";
    }

    public void SetARNull()
    {
        if (mediaPlayer is not null)
            mediaPlayer.AspectRatio = null;
    }

    /*
    public void PlayPause()
    {
        Debug.Log ("[VLC] Toggling Play Pause !");
        if (mediaPlayer == null)
        {
            mediaPlayer = new MediaPlayer(libVLC);
        }
        if (mediaPlayer.IsPlaying)
        {
            mediaPlayer.Pause();
        }
        else
        {
            playing = true;

            if(mediaPlayer.Media == null)
            {
                // download https://streams.videolan.org/streams/360/eagle_360.mp4 
                // to your computer (to avoid network requests for smoother navigation)
                // and adjust the Uri to the local path
                // var media = new Media(new Uri("https://streams.videolan.org/streams/360/eagle_360.mp4"));
                //var media = new Media(new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
                
                var media = new Media(new Uri(path));
                
                Task.Run(async () =>
                {
                    var result = await media.ParseAsync(libVLC, MediaParseOptions.ParseNetwork);
                    var trackList = media.TrackList(TrackType.Video);
                    _is360 = trackList[0].Data.Video.Projection == VideoProjection.Equirectangular;

                    Debug.Log($"projection {trackList[0].Data.Video.Projection}");


                    if (_is360)
                    {
                        Debug.Log("The video is a 360 video");
                        _360Sphere.SetActive(true);
                        _2DDisplaySet.SetActive(false);
                    }

                    else
                    {
                        Debug.Log("The video was not identified as a 360 video by VLC, make sure it is properly tagged");
                        _360Sphere.SetActive(false);
                        _2DDisplaySet.SetActive(true);
                    }

                    trackList.Dispose();
                });
                
                mediaPlayer.Media = media;
            }

            mediaPlayer.Play();
        }
    } */

    /*void Update()
    {
        if(!playing) return;

        if (tex == null)
        {
            // If received size is not null, it and scale the texture
            uint i_videoHeight = 0;
            uint i_videoWidth = 0;

            mediaPlayer.Size(0, ref i_videoWidth, ref i_videoHeight);
            var texptr = mediaPlayer.GetTexture(i_videoWidth, i_videoHeight, out bool updated);
            if (i_videoWidth != 0 && i_videoHeight != 0 && updated && texptr != IntPtr.Zero)
            {
                Debug.Log("Creating texture with height " + i_videoHeight + " and width " + i_videoWidth);
                tex = Texture2D.CreateExternalTexture((int)i_videoWidth,
                    (int)i_videoHeight,
                    TextureFormat.RGBA32,
                    false,
                    true,
                    texptr);
                //GetComponent<Renderer>().material.mainTexture = tex;
                m_lRenderer.material.mainTexture = tex;
                m_rRenderer.material.mainTexture = tex;
            }
        }
        else if (tex != null)
        {
            var texptr = mediaPlayer.GetTexture((uint)tex.width, (uint)tex.height, out bool updated);
            if (updated)
            {
                tex.UpdateExternalTexture(texptr);
            }
        }
    }*/

    VideoMode[] _SphericalModes = new VideoMode[4] { VideoMode._180_2D, VideoMode._360_2D, VideoMode._180_3D, VideoMode._360_3D };
    private float _sphereScale;

    void OnGUI()
    {
        if (!jakesRemoteController.OGMenuVisible())
        {
            return;
        }
        if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
        {
            m_PreviousPos = NRInput.GetTouch();
        }
        else if (NRInput.GetButton(ControllerButton.TRIGGER))
        {
            //UpdateScroll();
            Do360Navigation();
        }
        else if (NRInput.GetButtonUp(ControllerButton.TRIGGER))
        {
            //m_PreviousPos = Vector2.zero;
        }

        
    }

    void Do360Navigation()
    {
        
        var range = Math.Max(UnityEngine.Screen.width, UnityEngine.Screen.height);

        Yaw = mediaPlayer.Viewpoint.Yaw;
        Pitch = mediaPlayer.Viewpoint.Pitch;
        Roll = mediaPlayer.Viewpoint.Roll;


        Vector2 deltaMove = NRInput.GetTouch() - m_PreviousPos;
        m_PreviousPos = NRInput.GetTouch();

        float absX = Mathf.Abs(deltaMove.x);
        float absY = Mathf.Abs(deltaMove.y);

        float eighty_or_delta_x = absX > 0 ? absX * 10000 : 80;
        float eighty_or_delta_y = absY > 0 ? absY * 10000 : 80;

        Debug.Log($"*80x {eighty_or_delta_x} 80y {eighty_or_delta_y} fov {fov} fov2 {nreal_fov}");

        bool? result = null;
        try
        {
            if (Input.GetKey(KeyCode.RightArrow) || deltaMove.x > 0)
            {
                result = mediaPlayer.UpdateViewpoint(Yaw + (float)(eighty_or_delta_x * +40 / range), Pitch, Roll, fov, true);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || deltaMove.x < 0)
            {
                result = mediaPlayer.UpdateViewpoint(Yaw - (float)(eighty_or_delta_x * +40 / range), Pitch, Roll, fov, true);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || deltaMove.y < 0)
            {
                result = mediaPlayer.UpdateViewpoint(Yaw, Pitch + (float)(eighty_or_delta_y * +20 / range), Roll, fov, true);
            }
            else if (Input.GetKey(KeyCode.UpArrow) || deltaMove.y > 0)
            {
                result = mediaPlayer.UpdateViewpoint(Yaw, Pitch - (float)(eighty_or_delta_y * +20 / range), Roll, fov, true);
            }
        }catch(Exception e)
        {
            Debug.LogWarning("error updating viewpoint " + e);
        }

        Debug.Log("Update Viewpoint Result " + result.ToString());
    }

    //Public functions that expose VLC MediaPlayer functions in a Unity-friendly way. You may want to add more of these.
        #region vlc
    public void Open(string path)
    {
        Log("VLCPlayerExample Open " + path);
        this.path = path;
        Open();
    }

    public void Open()
    {
        Log("VLCPlayerExample Open");
        if (mediaPlayer?.Media != null)
            mediaPlayer.Media.Dispose();

        var trimmedPath = path.Trim(new char[] { '"' });//Windows likes to copy paths with quotes but Uri does not like to open them
        mediaPlayer.Media = new Media(new Uri(trimmedPath));

        Task.Run(async () =>
        {
            var result = await mediaPlayer.Media.ParseAsync(libVLC, MediaParseOptions.ParseNetwork);
            var trackList = mediaPlayer.Media.TrackList(TrackType.Video);
            _is360 = trackList[0].Data.Video.Projection == VideoProjection.Equirectangular;

            Debug.Log($"projection {trackList[0].Data.Video.Projection}");

            if (_is360)
            {
                Debug.Log("The video is a 360 video");
                SetVideoMode(VideoMode._360_2D);
            }

            else
            {
                Debug.Log("The video was not identified as a 360 video by VLC");
                SetVideoMode(VideoMode.Mono);
            }

            trackList.Dispose();
        });

        // flag to read and store the texture aspect ratio
        m_updatedARSinceOpen = false;

        Play();
    }

    public void OpenExternal()
    {
        // TODO: Prompt user for path
    }

    public void Play()
    {
        Log("VLCPlayerExample Play");

        _cone?.SetActive(false); // hide cone logo
        _pointLight?.SetActive(false);

        mediaPlayer.Play();

        CheckTrialExceeded();
    }

    public void Pause()
    {
        Log("VLCPlayerExample Pause");
        mediaPlayer.Pause();
    }

    public void Stop()
    {
        Log("VLCPlayerExample Stop");
        mediaPlayer?.Stop();

        // TODO: encapsulate this
        if (m_lRenderer?.material is not null)
            m_lRenderer.material.mainTexture = null;

        if (m_rRenderer?.material is not null)
            m_rRenderer.material.mainTexture = null;

        if (m_l360Renderer?.material is not null)
            m_l360Renderer.material.mainTexture = null;

        if (m_r360Renderer?.material is not null)
            m_r360Renderer.material.mainTexture = null;


        // show cone
        _cone?.SetActive(true);
        _pointLight?.SetActive(true);


        // clear to black
        _vlcTexture = null;
        texture = null;

    }

    public void SeekForward10()
    {
        SeekSeconds((float)10);
    }
    
    public void SeekBack10()
    {
        SeekSeconds((float)-10);
    }

    public void SeekSeconds(float seconds)
    {
        Seek((long)seconds * 1000);
    }

    public void Seek(long timeDelta)
    {
        Debug.Log("VLCPlayerExample Seek " + timeDelta);
        mediaPlayer.SetTime(mediaPlayer.Time + timeDelta);
    }

    public void SetTime(long time)
    {
        Log("VLCPlayerExample SetTime " + time);
        mediaPlayer.SetTime(time);
    }

    public void SetVolume(int volume = 100)
    {
        Log("VLCPlayerExample SetVolume " + volume);
        mediaPlayer.SetVolume(volume);
    }

    public int Volume
    {
        get
        {
            if (mediaPlayer == null)
                return 0;
            return mediaPlayer.Volume;
        }
    }

    public bool IsPlaying
    {
        get
        {
            if (mediaPlayer == null)
                return false;
            return mediaPlayer.IsPlaying;
        }
    }

    public long Duration
    {
        get
        {
            if (mediaPlayer == null || mediaPlayer.Media == null)
                return 0;
            return mediaPlayer.Media.Duration;
        }
    }

    public long Time
    {
        get
        {
            if (mediaPlayer == null)
                return 0;
            return mediaPlayer.Time;
        }
    }

    public List<MediaTrack> Tracks(TrackType type)
    {
        Log("VLCPlayerExample Tracks " + type);
        return ConvertMediaTrackList(mediaPlayer?.Tracks(type));
    }

    public MediaTrack SelectedTrack(TrackType type)
    {
        Log("VLCPlayerExample SelectedTrack " + type);
        return mediaPlayer?.SelectedTrack(type);
    }

    public void Select(MediaTrack track)
    {
        Log("VLCPlayerExample Select " + track.Name);
        mediaPlayer?.Select(track);
    }

    public void Unselect(TrackType type)
    {
        Log("VLCPlayerExample Unselect " + type);
        mediaPlayer?.Unselect(type);
    }

    //This returns the video orientation for the currently playing video, if there is one
    public VideoOrientation? GetVideoOrientation()
    {
        var tracks = mediaPlayer?.Tracks(TrackType.Video);

        if (tracks == null || tracks.Count == 0)
            return null;

        var orientation = tracks[0]?.Data.Video.Orientation; //At the moment we're assuming the track we're playing is the first track

        return orientation;
    }

    #endregion

    //Private functions create and destroy VLC objects and textures
    #region internal
    //Create a new static LibVLC instance and dispose of the old one. You should only ever have one LibVLC instance.
    void CreateLibVLC()
    {
        Log("VLCPlayerExample CreateLibVLC");
        //Dispose of the old libVLC if necessary
        if (libVLC != null)
        {
            libVLC.Dispose();
            libVLC = null;
        }

        Core.Initialize(Application.dataPath); //Load VLC dlls
        libVLC = new LibVLC(enableDebugLogs: true); //You can customize LibVLC with advanced CLI options here https://wiki.videolan.org/VLC_command-line_help/

        //Setup Error Logging
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        libVLC.Log += (s, e) =>
        {
            //Always use try/catch in LibVLC events.
            //LibVLC can freeze Unity if an exception goes unhandled inside an event handler.
            try
            {
                if (logToConsole)
                {
                    Log(e.FormattedLog);
                }
            }
            catch (Exception ex)
            {
                Log("Exception caught in libVLC.Log: \n" + ex.ToString());
            }

        };
    }

    //Create a new MediaPlayer object and dispose of the old one. 
    void CreateMediaPlayer()
    {
        Log("VLCPlayerExample CreateMediaPlayer");
        if (mediaPlayer != null)
        {
            DestroyMediaPlayer();
        }
        mediaPlayer = new MediaPlayer(libVLC);
        Log("Media Player SET!");
    }

    //Dispose of the MediaPlayer object. 
    void DestroyMediaPlayer()
    {
        if(m_lRenderer?.material is not null)
            m_lRenderer.material.mainTexture = null;

        if(m_rRenderer?.material is not null)
            m_rRenderer.material.mainTexture = null;
        
        if(m_l360Renderer is not null && m_l360Renderer?.material is not null)
            m_l360Renderer.material.mainTexture = null;

        if(m_r360Renderer is not null && m_r360Renderer?.material is not null)
            m_r360Renderer.material.mainTexture = null;

        _vlcTexture = null;

        mediaPlayer?.Stop();
        mediaPlayer?.Dispose();
        mediaPlayer = null;

        libVLC?.Dispose();
        libVLC = null;
        
        Log("JakesSBSVLC DestroyMediaPlayer");
        mediaPlayer?.Stop();
        mediaPlayer?.Dispose();
        mediaPlayer = null;
    }

    //Resize the output textures to the size of the video
    void ResizeOutputTextures(uint px, uint py)
    {
        if(mediaPlayer is null)
        {
            return;
        }
        var texptr = mediaPlayer.GetTexture(px, py, out bool updated);
        if (px != 0 && py != 0 && updated && texptr != IntPtr.Zero)
        {
            //If the currently playing video uses the Bottom Right orientation, we have to do this to avoid stretching it.
            if (GetVideoOrientation() == VideoOrientation.BottomRight)
            {
                uint swap = px;
                px = py;
                py = swap;
            }

            _vlcTexture = Texture2D.CreateExternalTexture((int)px, (int)py, TextureFormat.RGBA32, false, true, texptr); //Make a texture of the proper size for the video to output to
            texture = new RenderTexture(_vlcTexture.width, _vlcTexture.height, 0, RenderTextureFormat.ARGB32); //Make a renderTexture the same size as vlctex

            Debug.Log($"texture size {px} {py} | {_vlcTexture.width} {_vlcTexture.height}");

            if (!m_updatedARSinceOpen)
            {
                m_updatedARSinceOpen = true;
                _aspectRatio = (float)texture.width / (float)texture.height;
                Debug.Log($"[SBSVLC] aspect ratio {_aspectRatio}");
                mediaPlayer.AspectRatio = $"{texture.width}:{texture.height}";


            }

            if (m_lRenderer != null)
                m_lRenderer.material.mainTexture = texture;
            if (m_rRenderer != null)
                m_rRenderer.material.mainTexture = texture;

            if (m_l360Renderer != null)
                m_l360Renderer.material.mainTexture = texture;

            if (m_r360Renderer != null)
                m_r360Renderer.material.mainTexture = texture;
        }
    }

    //Converts MediaTrackList objects to Unity-friendly generic lists. Might not be worth the trouble.
    List<MediaTrack> ConvertMediaTrackList(MediaTrackList tracklist)
    {
        if (tracklist == null)
            return new List<MediaTrack>(); //Return an empty list

        var tracks = new List<MediaTrack>((int)tracklist.Count);
        for (uint i = 0; i < tracklist.Count; i++)
        {
            tracks.Add(tracklist[i]);
        }
        return tracks;
    }

    public void ToggleFlipStereo()
    {
        _flipStereo = !_flipStereo;
        SetVideoMode(_videoMode);
    }

    public bool CheckTrialExceeded()
    {
        if (!_3DModeLocked)
        {
            return false;
        }
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        Debug.Log("trial exceeded? " + $"cur_time {cur_time} start {_3DTrialPlaybackStartedAt} diff {cur_time - _3DTrialPlaybackStartedAt} v {_MaxTrialPlaybackSeconds}");
        bool trialExceeded = _3DTrialPlaybackStartedAt == 0 ? false : (cur_time - _3DTrialPlaybackStartedAt) > _MaxTrialPlaybackSeconds;

        if (_videoMode == VideoMode._180_3D || _videoMode == VideoMode._360_3D)
        {
            // !myIAPHandler.HasReceiptFor3DMode()
            if (trialExceeded)
            {
                jakesRemoteController.ShowLockedPopup();
                _videoMode = VideoMode.SBSHalf;
                // unset flag
                //_isTrialing3DMode = false;
                Pause();
            } else
            {
                if(_3DTrialPlaybackStartedAt == 0){
                    _3DTrialPlaybackStartedAt = cur_time;
                    _isTrialing3DMode = true;
                }
            }
        }
        return trialExceeded;
    }

    public void SetVideoMode(VideoMode mode)
    {
        _videoMode = mode;
        CheckTrialExceeded();
        Debug.Log($"[JakeDowns] set video mode {mode}");

        if(_plane2SphereLeftEye is not null)
        {
            _plane2SphereLeftEye.GetComponent<Renderer>().material = m_monoMaterial;
            _plane2SphereLeftEye.GetComponent<Renderer>().material.mainTexture = texture;
        }

        if(_plane2SphereRightEye is not null)
        {
            _plane2SphereRightEye.GetComponent<Renderer>().material = m_monoMaterial;
            _plane2SphereRightEye.GetComponent<Renderer>().material.mainTexture = texture;
        }

        if (Array.IndexOf(_SphericalModes, mode) > -1)
        {
            flipTextureX = false;// true;          

            /* TOGGLE VISIBILITY */
            if(mode == VideoMode._360_2D || mode == VideoMode._180_2D)
            {
                // 2D
                _plane2SphereLeftEye.layer = LayerMask.NameToLayer("Default");
                _plane2SphereRightEye.SetActive(false);
            }
            else
            {
                // 3D

                _plane2SphereLeftEye.layer = LayerMask.NameToLayer("LeftEyeOnly");

                _plane2SphereRightEye.SetActive(true);
                _plane2SphereRightEye.layer = LayerMask.NameToLayer("RightEyeOnly");
            }

            /* SET MATERIALS */

            if(mode == VideoMode._360_3D)
            {
                _morphDisplayLeftRenderer.material = _flipStereo ? m_rightEye360Material : m_leftEye360Material;
                _morphDisplayRightRenderer.material = _flipStereo ? m_leftEye360Material : m_rightEye360Material;

            }
            else if(mode == VideoMode._360_2D)
            {
                _morphDisplayLeftRenderer.material = m_3602DSphericalMaterial;
            }
            else if(mode == VideoMode._180_3D)
            {
                _morphDisplayLeftRenderer.material = _flipStereo ? m_rightEye180Material : m_leftEye180Material;
                _morphDisplayRightRenderer.material = _flipStereo ? m_leftEye180Material : m_rightEye180Material;
            }
            else if(mode == VideoMode._180_2D)
            {
                _morphDisplayLeftRenderer.material = m_1802DSphericalMaterial;
                
            }

            /* SET TEXTURE */
            if(mode == VideoMode._360_2D || mode == VideoMode._180_2D)
            {
                // 2D
                _morphDisplayLeftRenderer.material.mainTexture = texture;
                _morphDisplayRightRenderer.material = null;
            }
            else
            {
                // 3D
                _morphDisplayLeftRenderer.material.mainTexture = texture;
                _morphDisplayRightRenderer.material.mainTexture = texture;
            }
        }
        else
        {
            flipTextureX = false;
            
            /*fov = 20.0f;
            if (LeftCamera is not null)
                LeftCamera.fieldOfView = 20;

            if (CenterCamera is not null)
                CenterCamera.fieldOfView = 20;
            
            if (RightCamera is not null)
                RightCamera.fieldOfView = 20;*/

            // TODO: set 360 material mainTextures to null to save battery
            /*Debug.Log("setting sphere inactive");
            _360Sphere.SetActive(false);
            _2DDisplaySet.SetActive(true);*/

            if (mode is VideoMode.SBSHalf or VideoMode.SBSFull)
            {
                _plane2SphereRightEye.SetActive(true);
                
                _morphDisplayLeftRenderer.material = _flipStereo ? m_rMaterial : m_lMaterial;
                _morphDisplayRightRenderer.material = _flipStereo ? m_lMaterial : m_rMaterial;
                _plane2SphereLeftEye.layer = LayerMask.NameToLayer("LeftEyeOnly");
                _plane2SphereRightEye.layer = LayerMask.NameToLayer("RightEyeOnly");
            }
            else if (mode is VideoMode.Mono)
            {
                _plane2SphereRightEye.SetActive(false);

                _morphDisplayLeftRenderer.material = m_monoMaterial;
                _plane2SphereLeftEye.layer = LayerMask.NameToLayer("Default");
            }
            else if (mode is VideoMode.TB)
            {
                _plane2SphereRightEye.SetActive(true);
                _morphDisplayLeftRenderer.material = m_leftEyeTBMaterial;
                _morphDisplayRightRenderer.material = m_rightEyeTBMaterial;
                _plane2SphereLeftEye.layer = LayerMask.NameToLayer("LeftEyeOnly");
                _plane2SphereRightEye.layer = LayerMask.NameToLayer("RightEyeOnly");
                
            }

            if (_morphDisplayLeftRenderer != null)
                _morphDisplayLeftRenderer.material.mainTexture = texture;
            
            if (_morphDisplayRightRenderer != null)
                _morphDisplayRightRenderer.material.mainTexture = texture;
        }

        /*if (_vlcTexture is not null)
        {
            if (mode == VideoMode.Mono || mode == VideoMode._180_2D || mode == VideoMode._360_2D)
            {
                // 2D
                mediaPlayer.AspectRatio = $"{_vlcTexture.width}:{_vlcTexture.height}";
            }
            else
            {
                // SBS
                mediaPlayer.AspectRatio = $"{_vlcTexture.width / 2}:{_vlcTexture.height}";
                if (mode == VideoMode.TB)
                {
                    mediaPlayer.AspectRatio = $"{_vlcTexture.width}:{_vlcTexture.height / 2}";
                }
            }
        }*/
        
        //fovBar.value = fov;
        //OnFOVSliderUpdated();

        
    }

    // https://answers.unity.com/questions/1549639/enum-as-a-function-param-in-a-button-onclick.html?page=2&pageSize=5&sort=votes

    public void SetVideoModeMono() => SetVideoMode(VideoMode.Mono);
    public void SetVideoModeSBSHalf() => SetVideoMode(VideoMode.SBSHalf);
    public void SetVideoModeSBSFull() => SetVideoMode(VideoMode.SBSFull);
    public void SetVideoModeTB() => SetVideoMode(VideoMode.TB);

    public void ResetScreen()
    {
        _plane2SphereLeftEye.transform.localPosition = _startPosition;
        _plane2SphereLeftEye.transform.localRotation = Quaternion.identity;
        _plane2SphereLeftEye.transform.localScale = new Vector3(1, 1, 1);

        _plane2SphereRightEye.transform.localPosition = _startPosition;
        _plane2SphereRightEye.transform.localRotation = Quaternion.identity;
        _plane2SphereRightEye.transform.localScale = new Vector3(1, 1, 1);
    }

    public void promptUserFilePicker()
    {
#if UNITY_ANDROID
        // Use MIMEs on Android
        string[] fileTypes = new string[] { "video/*" };
#else
		// Use UTIs on iOS
		string[] fileTypes = new string[] { "public.movie" };
#endif
        
        // Pick image(s) and/or video(s)
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
                Debug.Log("Operation cancelled");
            else
            {
                Debug.Log("Picked file: " + path);
                Open(path);
            }
        }, fileTypes);

        if (permission is not NativeFilePicker.Permission.Granted)
        {
            _ShowAndroidToastMessage($"Permission result: {permission}");
            Debug.Log("Permission result: " + permission);
        }
    }

    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.jakedowns.VLC3D.VLC3DActivity");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    public void OnSingleTap(string name)    
    {
        Debug.Log($"[SBSVLC] Single Tap Triggered {name}");
        if (name == "LockScreenButton")
        {
            if (!_screenLocked)
            {
                ToggleScreenLock();
            }
        }
    }

    // we require a double-tap to unlock
    public void OnDoubleTap(string name)
    {
        Debug.Log($"[SBSVLC] Double Tap Triggered {name}");
        if (name == "LockScreenButton")
        {
            if (_screenLocked)
            {
                ToggleScreenLock();
            }
        }
    }

    void GetContext()
    {
        unityPlayer = new AndroidJavaClass("com.jakedowns.VLC3D.VLC3DActivity");
        try
        {
            activity = unityPlayer?.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity?.Call<AndroidJavaObject>("getApplicationContext");
        }
        catch (Exception e)
        {
            Debug.Log("error getting context " + e.ToString());
        }
    }

    public void ToggleScreenLock()
    {
        _screenLocked = !_screenLocked;

        if (_screenLocked)
        {
            // Hide All UI except for the lock button
            _hideWhenLocked.SetActive(false);
            _logo.SetActive(false);
            _menuToggleButton.SetActive(false);
            // Lower Brightness
            float _unityBrightnessOnLock = Screen.brightness;
            Debug.Log($"lockbrightness Unity brightness on lock {_unityBrightnessOnLock}");

#if UNITY_ANDROID
            if (!Application.isEditor)
            {
                try
                {
                    // get int from _brightnessHelper
                    _brightnessOnLock = (int)(_brightnessHelper?.CallStatic<int>("getBrightness"));

                    _brightnessModeOnLock = (int)(_brightnessHelper?.CallStatic<int>("getBrightnessMode"));

                    Debug.Log($"lockbrightness Android brightness on lock {_brightnessOnLock}");
                }catch(Exception e)
                {
                    Debug.Log("Error getting brightness " + e.ToString());
                }

                // Set it to 0? 0.1?
                //Debug.Log($"set brightness with unity");
                //Screen.brightness = 0.1f;

                if (context is null)
                {
                    Debug.Log("context is null");
                    GetContext();
                }
                if (context is not null)
                {
                    // TODO: maybe try to fetch it again now?

                    object _args = new object[2] { context, 1 };

                    // call _brightnessHelper
                    _brightnessHelper?.CallStatic("SetBrightness", _args);
                }
                 

            }
#endif
        }
        else
        {
#if UNITY_ANDROID
            if (!Application.isEditor)
            {
                if (context is null)
                {
                    Debug.Log("context is null");
                    GetContext();
                }
                if (context is not null)
                {
                    try
                    {
                        object _args = new object[2] { context, _brightnessOnLock };
                        _brightnessHelper?.CallStatic("setBrightness", _args);

                        // restore brightness mode
                        object _args_mode = new object[2] { context, _brightnessModeOnLock };
                        _brightnessHelper?.CallStatic("setBrightnessMode", _args_mode);
                    }
                    catch(Exception e)
                    {
                        Debug.Log("error setting brightness " + e.ToString());
                    }
                    
                }
                
            }
#else
            // Restore Brightness
            Screen.brightness = _brightnessOnLock;
#endif

            // Show All UI when screen is unlocked
            _hideWhenLocked.SetActive(true);
            _logo.SetActive(true);
            _menuToggleButton.SetActive(true);
        }
    }

    void Log(object message)
    {
        if (logToConsole)
            Debug.Log($"[VLC] {message}");
    }
#endregion

    public void Unlock3DMode()
    {
        _3DModeLocked = false;
    }
}
