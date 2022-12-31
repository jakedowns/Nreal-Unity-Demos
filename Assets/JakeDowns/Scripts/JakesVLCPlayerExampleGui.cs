using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LibVLCSharp;
using UnityEditor;


///This script controls all the GUI for the VLC Unity Canvas Example
///It sets up event handlers and updates the GUI every frame
///This example shows how to safely set up LibVLC events and a simple way to call Unity functions from them
public class JakesVLCPlayerExampleGui : MonoBehaviour
{
	public JakesSBSVLC vlcPlayer;

	//GUI Elements
	//public RawImage screen;
	//public AspectRatioFitter screenAspectRatioFitter;
	public Button Rewind10Button;
	public Button FFW10Button;
	public Slider seekBar;
	public Slider scaleBar;
	public Button playButton;
	public Button pauseButton;
	public Button stopButton;
	public Button pathButton;
	public Button fileButton;
	public Button tracksButton;
	public Button volumeButton;
	public GameObject pathGroup; //Group containing pathInputField and openButton
	public InputField pathInputField;
	public Button openButton;
	public GameObject tracksButtonsGroup; //Group containing buttons to switch video, audio, and subtitle tracks
	public Slider volumeBar;
	public GameObject trackButtonPrefab;
	public GameObject trackLabelPrefab;
	public Color unselectedButtonColor; //Used for unselected track text
	public Color selectedButtonColor; //Used for selected track text

	public Text currentTimecode;

	public Slider ARWidthBar;
	public Slider ARHeightBar;
	public Slider ARComboBar;

	bool _isDraggingARWidthBar = false;
    bool _isDraggingARHeightBar = false;
	bool _isDraggingARComboBar = false;

    //Configurable Options
    public int maxVolume = 100; //The highest volume the slider can reach. 100 is usually good but you can go higher.

	//State variables
	bool _isPlaying = false; //We use VLC events to track whether we are playing, rather than relying on IsPlaying 
	bool _isDraggingSeekBar = false; //We advance the seek bar every frame, unless the user is dragging it

	bool _isDraggingScaleBar = false;
	
	///Unity wants to do everything on the main thread, but VLC events use their own thread.
	///These variables can be set to true in a VLC event handler indicate that a function should be called next Update.
	///This is not actually thread safe and should be gone soon!
	bool _shouldUpdateTracks = false; //Set this to true and the Tracks menu will regenerate next frame
	bool _shouldClearTracks = false; //Set this to true and the Tracks menu will clear next frame

	List<Button> _videoTracksButtons = new List<Button>();
	List<Button> _audioTracksButtons = new List<Button>();
	List<Button> _textTracksButtons = new List<Button>();


	void Start()
	{
		if (vlcPlayer is null)
		{
            Debug.LogError("VLC Player not found. Please assign a JakesSBSVLC component to the vlcPlayer variable in the inspector.");
			return;
        }
        if (vlcPlayer?.mediaPlayer is null)
		{
			Debug.LogError("VLC Player mediaPlayer not found");
			return;
		}
        
		//VLC Event Handlers
		vlcPlayer.mediaPlayer.Playing += (object sender, EventArgs e) => {
			//Always use Try/Catch for VLC Events
			try
			{
				//Because many Unity functions can only be used on the main thread, they will fail in VLC event handlers
				//A simple way around this is to set flag variables which cause functions to be called on the next Update
				_isPlaying = true;//Switch to the Pause button next update
				_shouldUpdateTracks = true;//Regenerate tracks next update


			}
			catch (Exception ex)
			{
				Debug.LogError("Exception caught in mediaPlayer.Play: \n" + ex.ToString());
			}
		};

		vlcPlayer.mediaPlayer.Paused += (object sender, EventArgs e) => {
			//Always use Try/Catch for VLC Events
			try
			{
				_isPlaying = false;//Switch to the Play button next update
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception caught in mediaPlayer.Paused: \n" + ex.ToString());
			}
		};

		vlcPlayer.mediaPlayer.Stopped += (object sender, EventArgs e) => {
			//Always use Try/Catch for VLC Events
			try
			{
				_isPlaying = false;//Switch to the Play button next update
				_shouldClearTracks = true;//Clear tracks next update
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception caught in mediaPlayer.Stopped: \n" + ex.ToString());
			}
		};

		//Buttons
		Rewind10Button.onClick.AddListener(() => {
			Debug.Log("Rewind10Button");
			vlcPlayer.SeekBack10();
        });
		FFW10Button.onClick.AddListener(() =>
		{
            Debug.Log("FFW10Button");
            vlcPlayer.SeekForward10();
        });
        pauseButton.onClick.AddListener(() => { vlcPlayer.Pause(); });
		playButton.onClick.AddListener(() => { vlcPlayer.Play(); });
		stopButton.onClick.AddListener(() => { vlcPlayer.Stop(); });
		pathButton.onClick.AddListener(() => { 
			if(ToggleElement(pathGroup))
				pathInputField.Select();
		});
        fileButton.onClick.AddListener(() =>
        {
			vlcPlayer.promptUserFilePicker();
        });
        tracksButton.onClick.AddListener(() => { ToggleElement(tracksButtonsGroup); SetupTrackButtons(); });
		volumeButton.onClick.AddListener(() => { ToggleElement(volumeBar.gameObject); });
		openButton.onClick.AddListener(() => { ToggleElement(pathGroup); vlcPlayer.Open(pathInputField.text); });

		UpdatePlayPauseButton(vlcPlayer.playOnAwake);

		//Seek Bar Events
		var seekBarEvents = seekBar.GetComponent<EventTrigger>();

		EventTrigger.Entry seekBarPointerDown = new EventTrigger.Entry();
		seekBarPointerDown.eventID = EventTriggerType.PointerDown;
		seekBarPointerDown.callback.AddListener((data) => { _isDraggingSeekBar = true; });
		seekBarEvents.triggers.Add(seekBarPointerDown);

		EventTrigger.Entry seekBarPointerUp = new EventTrigger.Entry();
		seekBarPointerUp.eventID = EventTriggerType.PointerUp;
		seekBarPointerUp.callback.AddListener((data) => { 
			_isDraggingSeekBar = false;
			vlcPlayer.SetTime((long)((double)vlcPlayer.Duration * seekBar.value));
		});
		seekBarEvents.triggers.Add(seekBarPointerUp);

        // Scale Bar Events
        /*var scaleBarEvents = scaleBar.GetComponent<EventTrigger>();

        EventTrigger.Entry scaleBarPointerDown = new EventTrigger.Entry();
        scaleBarPointerDown.eventID = EventTriggerType.PointerDown;
        scaleBarPointerDown.callback.AddListener((data) => { _isDraggingScaleBar = true; });
        scaleBarEvents.triggers.Add(scaleBarPointerDown);

        EventTrigger.Entry scaleBarPointerUp = new EventTrigger.Entry();
        scaleBarPointerUp.eventID = EventTriggerType.PointerUp;
        scaleBarPointerUp.callback.AddListener((data) => {
            _isDraggingScaleBar = false;
            GameObject.Find("SphereDisplay").transform.localScale = new Vector3(scaleBar.value, scaleBar.value, scaleBar.value);
        });
        scaleBarEvents.triggers.Add(scaleBarPointerUp);
		*/
        // AR Width Bar Events
        var arWidthBarEvents = ARWidthBar.GetComponent<EventTrigger>();

        EventTrigger.Entry arWidthBarPointerDown = new EventTrigger.Entry();
        arWidthBarPointerDown.eventID = EventTriggerType.PointerDown;
        arWidthBarPointerDown.callback.AddListener((data) => { _isDraggingARWidthBar = true; });
        arWidthBarEvents.triggers.Add(arWidthBarPointerDown);

        EventTrigger.Entry arWidthBarPointerUp = new EventTrigger.Entry();
        arWidthBarPointerUp.eventID = EventTriggerType.PointerUp;
        arWidthBarPointerUp.callback.AddListener((data) =>
        {
			if (_isDraggingARWidthBar)
			{
                IngestARWidthHeightInput();
            }
            _isDraggingARWidthBar = false;
            
        });
		arWidthBarEvents.triggers.Add(arWidthBarPointerUp);
        ARWidthBar.onValueChanged.AddListener((value) =>
        {
			if (_isDraggingARWidthBar)
			{ 
				IngestARWidthHeightInput();
			}
        });

        // AR Height Bar Events
        var arHeightBarEvents = ARHeightBar.GetComponent<EventTrigger>();

        EventTrigger.Entry arHeightBarPointerDown = new EventTrigger.Entry();
        arHeightBarPointerDown.eventID = EventTriggerType.PointerDown;
        arHeightBarPointerDown.callback.AddListener((data) => { _isDraggingARHeightBar = true; });
        arHeightBarEvents.triggers.Add(arHeightBarPointerDown);

        EventTrigger.Entry arHeightBarPointerUp = new EventTrigger.Entry();
        arHeightBarPointerUp.eventID = EventTriggerType.PointerUp;
        arHeightBarPointerUp.callback.AddListener((data) =>
        {
			if (_isDraggingARHeightBar)
			{
                IngestARWidthHeightInput();
            }
            _isDraggingARHeightBar = false;
			
        });
        arHeightBarEvents.triggers.Add(arHeightBarPointerUp);
		ARHeightBar.onValueChanged.AddListener((value) =>
		{
			if (_isDraggingARHeightBar)
			{
				IngestARWidthHeightInput();
			}
		});

        // AR Combo Bar Events
        var arComboBarEvents = ARComboBar.GetComponent<EventTrigger>();

        EventTrigger.Entry arComboBarPointerDown = new EventTrigger.Entry();
        arComboBarPointerDown.eventID = EventTriggerType.PointerDown;
        arComboBarPointerDown.callback.AddListener((data) => { _isDraggingARComboBar = true; });
        arComboBarEvents.triggers.Add(arComboBarPointerDown);

        EventTrigger.Entry arComboBarPointerUp = new EventTrigger.Entry();
        arComboBarPointerUp.eventID = EventTriggerType.PointerUp;
        arComboBarPointerUp.callback.AddListener((data) =>
        {
			if (_isDraggingARComboBar)
			{
                UpdateARWidthAndHeightFromCombo();
            }
            _isDraggingARComboBar = false;
        });
        arComboBarEvents.triggers.Add(arComboBarPointerUp);
        ARComboBar.onValueChanged.AddListener((value) =>
        {
            UpdateARWidthAndHeightFromCombo();
        });

        //Path Input
        pathInputField.text = vlcPlayer.path;
		pathGroup.SetActive(false);

		//Track Selection Buttons
		tracksButtonsGroup.SetActive(false);

		//Volume Bar
		volumeBar.wholeNumbers = true;
		volumeBar.maxValue = maxVolume; //You can go higher than 100 but you risk audio clipping
		volumeBar.value = vlcPlayer.Volume;
		volumeBar.onValueChanged.AddListener((data) => { vlcPlayer.SetVolume((int)volumeBar.value);	});
		volumeBar.gameObject.SetActive(false);

	}

	void IngestARWidthHeightInput()
	{
		if (_isDraggingARComboBar)
		{
			return;
		}
        float width_rounded = (float)Math.Round((double)ARWidthBar.value, 2);
        float height_rounded = (float)Math.Round((double)ARHeightBar.value, 2);
        vlcPlayer.SetCurrentAR($"{width_rounded}:{height_rounded}");
		UpdateARComboFromWidthAndHeight(width_rounded, height_rounded);

    }

    int[] AspectRatioFractionFromDecimal(float aspectRatio)
	{
		// Multiply the aspect ratio by 100 to produce a whole number
		int wholeNumber = (int)(aspectRatio * 100f);

		// Find the GCD of the whole number and 100
		int gcd = GCD(wholeNumber, 100);

		// Divide the whole number and 100 by the GCD to reduce the fraction to its lowest terms
		int numerator = wholeNumber / gcd;
		int denominator = 100 / gcd;
		int[] fraction = new int[] { numerator, denominator };
		return fraction;
    }

    void UpdateARComboFromWidthAndHeight(float width, float height)
	{
		if(_isDraggingARWidthBar || _isDraggingARHeightBar)
		{
			return;
		}
        float ar_combo = Mathf.Round((width / height) * 100f) / 100f;
        ARComboBar.value = ar_combo;

        vlcPlayer.jakesRemoteController.UpdateCustomARPopupValuePreviewText();
    }

    void UpdateARWidthAndHeightFromCombo()
	{
		float arDecimal = Mathf.Round(ARComboBar.value * 100f) / 100f;
        // Get the aspect ratio fraction from the decimal
        int[] fraction = AspectRatioFractionFromDecimal(arDecimal);

        // Set the AR width and height bars to the fraction
        ARWidthBar.value = fraction[0]/100f;
        ARHeightBar.value = fraction[1]/100f;

        vlcPlayer.SetCurrentAR($"{fraction[0]}:{fraction[1]}");

        vlcPlayer.jakesRemoteController.UpdateCustomARPopupValuePreviewText();

    }
    int GCD(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    void Update()
	{
		//Update screen aspect ratio. Doing this every frame is probably more than is necessary.

		//if(vlcPlayer.texture != null)
		//	screenAspectRatioFitter.aspectRatio = (float)vlcPlayer.texture.width / (float)vlcPlayer.texture.height;

		UpdatePlayPauseButton(_isPlaying);

		UpdateSeekBar();

		if (_isDraggingScaleBar)
		{
			/*GameObject _sphere = GameObject.Find("SphereDisplay");
			if(_sphere is not null)
				_sphere.transform.localScale = new Vector3(scaleBar.value, scaleBar.value, scaleBar.value);*/
        }

		if (_shouldUpdateTracks)
		{
			SetupTrackButtons();
			_shouldUpdateTracks = false;
		}

		if (_shouldClearTracks)
		{
			ClearTrackButtons();
			_shouldClearTracks = false;
		}

	}

	//Show the Pause button if we are playing, or the Play button if we are paused or stopped
	void UpdatePlayPauseButton(bool playing)
	{
		pauseButton.gameObject.SetActive(playing);
		playButton.gameObject.SetActive(!playing);
	}

	//Update the position of the Seek slider to the match the VLC Player
	void UpdateSeekBar()
	{
        // Get the current playback time as a TimeSpan object
        long currentTime = vlcPlayer.Time;
        TimeSpan currentTimeSpan = TimeSpan.FromMilliseconds(currentTime);

        // Format the TimeSpan object as a string in the desired format
        string timecode = currentTimeSpan.ToString(@"hh\:mm\:ss");

        currentTimecode.text = timecode;

        if (!_isDraggingSeekBar)
		{
			var duration = vlcPlayer.Duration;
			if (duration > 0)
				seekBar.value = (float)((double)vlcPlayer.Time / duration);
		}
	}

	//Enable a GameObject if it is disabled, or disable it if it is enabled
	bool ToggleElement(GameObject element)
	{
		bool toggled = !element.activeInHierarchy;
		element.SetActive(toggled);
		return toggled;
	}

	//Create Audio, Video, and Subtitles button groups
	void SetupTrackButtons()
	{
		Debug.Log("SetupTrackButtons");
		ClearTrackButtons();
		SetupTrackButtonsGroup(TrackType.Video, "Video Tracks", _videoTracksButtons);
		SetupTrackButtonsGroup(TrackType.Audio, "Audio Tracks", _audioTracksButtons);
		SetupTrackButtonsGroup(TrackType.Text, "Subtitle Tracks", _textTracksButtons, true);

	}

	//Clear the track buttons menu
	void ClearTrackButtons()
	{
		var childCount = tracksButtonsGroup.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Destroy(tracksButtonsGroup.transform.GetChild(i).gameObject);
		}
	}

	//Create Audio, Video, or Subtitle button groups
	void SetupTrackButtonsGroup(TrackType type, string label, List<Button> buttonList, bool includeNone = false)
	{
		buttonList.Clear();
		var tracks = vlcPlayer.Tracks(type);
		var selected = vlcPlayer.SelectedTrack(type);

		if (tracks.Count > 0)
		{
			var newLabel = Instantiate(trackLabelPrefab, tracksButtonsGroup.transform);
			newLabel.GetComponentInChildren<Text>().text = label;

			for (int i = 0; i < tracks.Count; i++)
			{
				var track = tracks[i];
				var newButton = Instantiate(trackButtonPrefab, tracksButtonsGroup.transform).GetComponent<Button>();
				var textMeshPro = newButton.GetComponentInChildren<Text>();
				textMeshPro.text = track.Name;
				if (selected != null && track.Id == selected.Id)
					textMeshPro.color = selectedButtonColor;
				else
					textMeshPro.color = unselectedButtonColor;

				buttonList.Add(newButton);
				newButton.onClick.AddListener(() => {
					foreach (var button in buttonList)
						button.GetComponentInChildren<Text>().color = unselectedButtonColor;
					textMeshPro.color = selectedButtonColor;
					vlcPlayer.Select(track);
				});
			}
			if (includeNone)
			{
				var newButton = Instantiate(trackButtonPrefab, tracksButtonsGroup.transform).GetComponent<Button>();
				var textMeshPro = newButton.GetComponentInChildren<Text>();
				textMeshPro.text = "None";
				if (selected == null)
					textMeshPro.color = selectedButtonColor;
				else
					textMeshPro.color = unselectedButtonColor;

				buttonList.Add(newButton); 
				newButton.onClick.AddListener(() => {
					foreach (var button in buttonList)
						button.GetComponentInChildren<Text>().color = unselectedButtonColor;
					textMeshPro.color = selectedButtonColor;
					vlcPlayer.Unselect(type);
				});
			}

		}

	}
}