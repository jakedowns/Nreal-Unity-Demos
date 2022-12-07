using UnityEngine;
using System;
using System.Threading.Tasks;
using LibVLCSharp;
//using NRKernal;
using System.Collections.Generic;
//using UnityEngine.Device;
//using UnityEngine.UI;
using Application = UnityEngine.Device.Application;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using NRKernal;
//using static JakesSBSVLC;
//using static UnityEditor.Experimental.GraphView.GraphView;

/**
 * @TODO: change camera FOV when switching to 360 mode
 */


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


    LibVLC libVLC;
    public MediaPlayer mediaPlayer;
    const int seekTimeDelta = 5000;
    //Texture2D tex = null;
    bool playing = false;

    AndroidJavaClass _brightnessHelper;

    [SerializeField]
    GameObject NRCameraRig;
    Camera LeftCamera;
    Camera CenterCamera;
    Camera RightCamera;

    GameObject _hideWhenLocked;
    GameObject _menuToggleButton;
    GameObject _logo;
    public GameObject _360Sphere;
    GameObject _2DDisplaySet;

    [SerializeField]
    public UnityEngine.UI.Slider fovBar;

    GameObject _cone;

    bool _screenLocked = false;
    int _brightnessOnLock = 0;

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

    Material m_lMaterial;
    Material m_rMaterial;
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

    float _aspectRatio;
    bool m_updatedARSinceOpen = false;

    // TODO: support overriding the current aspect ratio
    float _aspectRatioOverride;

    /// <summary> The previous position. </summary>
    private Vector2 m_PreviousPos;

    // TODO: add fov slider
    int fov = 180;

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
            unityPlayer = new AndroidJavaClass("com.jakedowns.VLC3D.VLC3DActivity");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");


            _brightnessHelper = new AndroidJavaClass("com.jakedowns.BrightnessHelper");
            if (_brightnessHelper == null)
            {
                Debug.Log("error loading _brightnessHelper");
            }
        }
#endif
        
        Debug.Log($"[VLC] LibVLC version and architecture {libVLC.Changeset}");
        Debug.Log($"[VLC] LibVLCSharp version {typeof(LibVLC).Assembly.GetName().Version}");

        LeftCamera = NRCameraRig.transform.Find("LeftCamera").GetComponent<Camera>();
        CenterCamera = NRCameraRig.transform.Find("CenterCamera").GetComponent<Camera>();
        RightCamera = NRCameraRig.transform.Find("RightCamera").GetComponent<Camera>();
        OnFOVSliderUpdated();

        _cone = GameObject.Find("CONE_PARENT");

        //_360Sphere = GameObject.Find("SphereDisplay");
        /*if (_360Sphere is null)
            Debug.LogError("SphereDisplay not found");
        else
            _360Sphere.SetActive(false);*/

        //leftEyeSphere = _360Sphere.transform.Find("LeftEye").gameObject;
        //rightEyeSphere = _360Sphere.transform.Find("RightEye").gameObject;
        m_l360Renderer = leftEyeSphere.GetComponent<Renderer>();
        m_r360Renderer = rightEyeSphere.GetComponent<Renderer>();

        _2DDisplaySet = GameObject.Find("SBSDisplay/DisplaySet");

        // TODO: extract lockscreen logic into a separate script
        _hideWhenLocked = GameObject.Find("HideWhenScreenLocked");
        _logo = GameObject.Find("logo");
        _menuToggleButton = GameObject.Find("MenuToggleButton");

        //Setup Screen
        /*if (screen == null)
            screen = GetComponent<Renderer>();
        if (canvasScreen == null)
            canvasScreen = GetComponent<RawImage>();*/

        m_lRenderer = leftEye.GetComponent<Renderer>();
        m_rRenderer = rightEye.GetComponent<Renderer>();

        // read material reference
        m_lMaterial = m_lRenderer.material;
        m_rMaterial = m_rRenderer.material;

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

    public void Demo3602D()
    {
        Open("https://streams.videolan.org/streams/360/eagle_360.mp4");
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

    public void OnFOVSliderUpdated()
    {
        fov = (int)fovBar.value;
        
        if (_is360)
        {
            LeftCamera.fieldOfView = fov;
            CenterCamera.fieldOfView = fov;
            RightCamera.fieldOfView = fov;
            Do360Navigation();
        }
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

    void OnGUI()
    {
        if(Array.IndexOf(_SphericalModes, _videoMode) == -1)
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

        Debug.Log($"80x {eighty_or_delta_x} 80y {eighty_or_delta_y}");


        if (Input.GetKey(KeyCode.RightArrow) || deltaMove.x > 0)
        {
            mediaPlayer.UpdateViewpoint(Yaw + (float)(eighty_or_delta_x * +40 / range), Pitch, Roll, fov);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || deltaMove.x < 0)
        {
            mediaPlayer.UpdateViewpoint(Yaw - (float)(eighty_or_delta_x * +40 / range), Pitch, Roll, fov);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || deltaMove.y < 0)
        {
            mediaPlayer.UpdateViewpoint(Yaw, Pitch + (float)(eighty_or_delta_y * +20 / range), Roll, fov);
        }
        else if (Input.GetKey(KeyCode.UpArrow) || deltaMove.y > 0)
        {
            mediaPlayer.UpdateViewpoint(Yaw, Pitch - (float)(eighty_or_delta_y * +20 / range), Roll, fov);
        }
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

        mediaPlayer.Play();
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


        // clear to black
        _vlcTexture = null;
        texture = null;

    }

    public void Seek(long timeDelta)
    {
        Log("VLCPlayerExample Seek " + timeDelta);
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

            if (!m_updatedARSinceOpen)
            {
                m_updatedARSinceOpen = true;
                _aspectRatio = (float)texture.width / (float)texture.height;
                Debug.Log($"[SBSVLC] aspect ratio {_aspectRatio}");
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

    public void SetVideoMode(VideoMode mode)
    {
        _videoMode = mode;
        Debug.Log($"[JakeDowns] set video mode {mode}");

        if (Array.IndexOf(_SphericalModes, mode) > -1)
        {
            // TODO: set SBS material mainTextures to null to save battery
            Debug.Log("Trying to set Sphere to Active!");
            _360Sphere.SetActive(true);
            _2DDisplaySet.SetActive(false);

            if(mode == VideoMode._360_2D || mode == VideoMode._180_2D)
            {
                // 2D
                leftEyeSphere.SetActive(true);
                rightEyeSphere.SetActive(false);
                leftEyeSphere.layer = LayerMask.NameToLayer("Default");
            }
            else
            {
                // 3D
                leftEyeSphere.SetActive(true);
                rightEyeSphere.SetActive(true);
                leftEyeSphere.layer = LayerMask.NameToLayer("LeftEyeOnly");
                rightEyeSphere.layer = LayerMask.NameToLayer("RightEyeOnly");
            }

            if(mode == VideoMode._360_3D)
            {
                m_l360Renderer.material = m_leftEye360Material;
                m_r360Renderer.material = m_rightEye360Material;
                
            }
            else if(mode == VideoMode._360_2D)
            {
                m_l360Renderer.material = m_3602DSphericalMaterial;
            }
            else if(mode == VideoMode._180_3D)
            {
                m_l360Renderer.material = m_leftEye180Material;
                m_r360Renderer.material = m_rightEye180Material;
            }
            else if(mode == VideoMode._180_2D)
            {
                m_l360Renderer.material = m_1802DSphericalMaterial;
            }

            if(mode == VideoMode._360_2D || mode == VideoMode._180_2D)
            {
                m_l360Renderer.material = m_3602DSphericalMaterial;
                m_l360Renderer.material.mainTexture = texture;
            }
            else
            {
                // 3D
                m_l360Renderer.material.mainTexture = texture;
                m_r360Renderer.material.mainTexture = texture;
            }

            OnFOVSliderUpdated();
        }
        else
        {
            if(LeftCamera is not null)
                LeftCamera.fieldOfView = 20;

            if (CenterCamera is not null)
                CenterCamera.fieldOfView = 20;
            
            if (RightCamera is not null)
                RightCamera.fieldOfView = 20;
            
            // TODO: set 360 material mainTextures to null to save battery
            Debug.Log("setting sphere inactive");
            _360Sphere.SetActive(false);
            _2DDisplaySet.SetActive(true);
            
            if (mode is VideoMode.SBSHalf or VideoMode.SBSFull)
            {
                m_lRenderer.material = _flipStereo ? m_rMaterial : m_lMaterial;
                m_rRenderer.material = _flipStereo ? m_lMaterial : m_rMaterial;
                leftEye.layer = LayerMask.NameToLayer("LeftEyeOnly");
                rightEye.layer = LayerMask.NameToLayer("RightEyeOnly");
            }
            else if (mode is VideoMode.Mono)
            {
                m_lRenderer.material = m_monoMaterial;
                m_rRenderer.material = m_monoMaterial;
                leftEye.layer = LayerMask.NameToLayer("Default");
                rightEye.layer = LayerMask.NameToLayer("Default");
            }
            else if (mode is VideoMode.TB)
            {
                // TODO: new TB materials and shaders
                // & add support for flipStereo
                m_lRenderer.material = m_leftEyeTBMaterial;
                m_rRenderer.material = m_rightEyeTBMaterial;
                leftEye.layer = LayerMask.NameToLayer("LeftEyeOnly");
                rightEye.layer = LayerMask.NameToLayer("RightEyeOnly");
            }

            if (m_lRenderer != null)
                m_lRenderer.material.mainTexture = texture;
            if (m_rRenderer != null)
                m_rRenderer.material.mainTexture = texture;
        }

        
    }

    // https://answers.unity.com/questions/1549639/enum-as-a-function-param-in-a-button-onclick.html?page=2&pageSize=5&sort=votes

    public void SetVideoModeMono() => SetVideoMode(VideoMode.Mono);
    public void SetVideoModeSBSHalf() => SetVideoMode(VideoMode.SBSHalf);
    public void SetVideoModeSBSFull() => SetVideoMode(VideoMode.SBSFull);
    public void SetVideoModeTB() => SetVideoMode(VideoMode.TB);

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
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
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
                // get int from _brightnessHelper
                _brightnessOnLock = (int)(_brightnessHelper?.CallStatic<int>("GetBrightness"));

                Debug.Log($"lockbrightness Android brightness on lock {_brightnessOnLock}");

                // Set it to 0? 0.1?
                //Debug.Log($"set brightness with unity");
                //Screen.brightness = 0.1f;

                object _args = new object[2] { context, 1 };

                // call _brightnessHelper
                _brightnessHelper?.CallStatic("SetBrightness", _args);
            }
#endif
        }
        else
        {
#if UNITY_ANDROID
            if (!Application.isEditor)
            {
                object _args = new object[2] { context, _brightnessOnLock };
                _brightnessHelper?.CallStatic("SetBrightness", _args);
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
}
