# into3D Media Player For Nreal

<img width="800" src="https://user-images.githubusercontent.com/1683122/205247852-1e877cd4-386b-4f51-a880-633a44189288.png" />

### [Download the Latest APK here](https://github.com/jakedowns/Nreal-Unity-Demos/releases)

### Features
- [x] VLC-based, so it supports pretty much every video/audio codec VLC does
- [x] URL input: point player to a video file and it will play it. (Youtube URL support currently broken) 
- [x] File Picker: load files locally from your phone, or over google drive
- [x] Pairs Great with Jellyfin or other Home Media Servers
- [x] Support for SBS Stereo 3D playback on Nreal Air & Nreal Light

### Coming Soon
- [ ] Subtitle Support
- [ ] Alternate Audio Track Selection

### How to use with Jellyfin
> Install Jellyfin Server on your PC and set up your Movie Library
> 
> Then, install the Jellyfin android app and connect to the server (or use the web UI)
> 
> click the ... next to the movie you want to watch in 3D and click "Copy Stream URL"
> 
> then, open nrealDemo (will rename it soon) and paste the URL into the Input field (Click the Globe Icon, then the SMALL play icon)
> 
> BOOM you're streaming 3D video from your computer

**Known Issues:**
1. [Android Build Fails](https://github.com/jakedowns/Nreal-Unity-Demos/issues/1) 
     - Need to figure out a way to distribute binaries without including them in source control
1. [VLC not loading youtube urls](https://code.videolan.org/videolan/vlc-unity/-/issues/168) - [Issue #2](https://github.com/jakedowns/Nreal-Unity-Demos/issues/2)
1. ~~[File picker not working](https://github.com/yasirkula/UnityNativeFilePicker/issues/31) - [Issue #3](https://github.com/jakedowns/Nreal-Unity-Demos/issues/3)~~ **SOLVED**
1. "Open In..." not wired up. The app appears in the share sheet, but I need to wire up receiving the file path
1. The code for Over/Under || Top/Bottom videos isn't set up correctly, will fix this soon.
1. Video is often too bright. I need to add some brightness/contrast/gamma sliders for adjustment. Coming Soon!
1. Subtitle Support is not complete (partially implemented)
1. Selecting Alternative Audio Tracks is not yet complete (partially implemented)
1. the "lock screen" icon is supposed to be setting screen brightness to 0.1, it also could be Pausing controller tracking. looking into that...

### Building

> NOTE: you must reimport the following packages (i'll figure out how to distrubte binaries outside of git later)
> - [VLC for Unity (Android) Free Trial Version 0.1.6](https://videolabs.io/solutions/unity/#:~:text=more%20platform%20support.-,Free%20trial%20version,-We%20offer%20a)
> - [Native File Picker Plugin v1.2.9](https://github.com/yasirkula/UnityNativeFilePicker)
> - [NRSDK 1.9.5](https://developer.nreal.ai/download)

i was having a hard time getting it to build in Unity directly,

i ended up having to Export it to the nested Android folder

each time i run "Export" i have to then delete any changes that unity makes to the manifests

from there i open it in android studio and perform the build.

it's kind of a pain, i just haven't been able to figure out how to solve building it directly in unity yet.

The main scene is "Assets/JakeDowns/Scenes/MVP_002 - SBS Test.unity"

![Unity_SkSJVaT9gc](https://user-images.githubusercontent.com/1683122/205026934-7a8e1fdf-78f1-46fa-919b-5b3be9c0c2de.png)

![studio64_6oeY0ZeMBj](https://user-images.githubusercontent.com/1683122/205026956-890d6b33-e16b-4553-8704-dde989dd9827.png)

### Updates

> 11/29/22
>
> Found [VLC Unity Plugin](https://code.videolan.org/videolan/vlc-unity) Project. Opting to use this instead of MPV

**Bonus Goal:** how thin of a unity layer can we get?

> really, there's so many helpful things in the NRSDK, it kind of makes sense to just use Unity. I don't really want to deal with the custom glue code and setting up Activities and Android Layout Views for the Phone vs the Virtual display. It would be fun at some point to see if I could get a Unity-less or almost-Unity-less demo set up, but someone will probably beat me to that.

**Q: why make a video player if Nreal is releasing an official one soon?** 

> **A:** I'm mostly doing this for fun / dipping my toes into Android & Unity development and publishing. Seemed like a nice, straight-forward path to start with. Before I venture off into more exploratory areas.

### MVP Todos:

- [x] get VLC unity plugin loading

- [x] Test MKV support

- [x] Set up shader to render half of sbs output to one eye, and the other half to the other eye

- [x] Add on-phone file picker moment for choosing a local file to play

- [x] basic playback controls

- [x] basic tracking mode-switching

---

#### Short-Term

- [x] set up repo

- [x] import unity base project

- [x] ~Proof-of-concept get mpv rendering to a surface in Unity
	(Base off of NRSDK\Demos\RGBCamera-Capture.unity (it has an example Unity Video player surface)~ Did it using VLC for Unity plugin instead

- [x] mock up virtual controller with an "input lock" toggle that requies a double-tap to bring the controls back. so you can put phone in pocket while viewing

- [x] add basic controls: stop, pause, play, seek, volume, source url input

- [x] Compare MPV and VLC codec support, maybe it makes more sense to base this on [VLC Android](https://github.com/videolan/vlc-android) **going with VLC**

- [x] add control to toggle SBS and over-under video format playback

- [ ] detect and account for half-sbs vs full-sbs

#### Mid-Term

- [x] toggle for tracking mode (6dof, 3dof, 0dof, 0dof stablized)

#### Long-Term

- [ ] ability to pin player anywhere around you (facing you)
- [ ] ability to pin player against surfaces
- [ ] test multi audio track switching
- [ ] test subtitles


(nice to haves / feature parity)

- Player UI
	- current timestamp, total length
	- seek bar
	- subtitles toggle
	- audio track switcher
	- play/pause button
	- switch between HW and SW decoder
	- playback speed toggle 

	- open external audio...
	- open external subtitle...
	- playlist
		- shuffle
		- repeat
		- pick file...
		- open url...

	- advanced playback controls
		- contrast
		- video brightness
		- gamma
		- toggle stats
		- audio delay
		- aspect ratio
		- 3D mode toggles 

- Settings > General
	- save playback position on quit
	- default audio language
	- default subtitle language
	- default file manager path
	
- Settings > Video
	- upscaling filter
	- downscaling filter
	- debanding
	- interpolation
	- temporal interpolation filter
	- low-quality video decoding

- Settings > UI
	- continue playback during ui popups (never, audio files only, always (video too))

- Settings > Touch Gestures
	- Smoother Seeking
	- Horizontal Drag (none, seek, volume, brightness)
	- Vertical Drag
	- Double-tap (left)
	- Double-tap (center)
	- Double-tap (right)
	* when any gesture is set to Custom it can be bound to any mpv command by editing input.conf
		left:   0x10001
		center: 0x10002
		right:  0x10003
		example: 0x1003 no-osd seek 6

- Settings > Developer
	- show stats
	- Ignore Audio Focus 
	- Enable OpenGL Debugging

- Settings > Advanced
	- Edit mpv.conf
	- Edit inpuut.conf
	- Version Info Screen
