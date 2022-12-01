# Nreal Unity Demos 001: Simple 3D Video Player

![image](https://user-images.githubusercontent.com/1683122/205008687-1d3cd009-47a9-411b-bfc7-e8c15baebe10.png)

Goal 1: a simple VLC-based video player, enabling SBS and OU 3D Stereo video playback of local files AND files served over Google Drive or other Network Storage Servers

> NOTE: If you use Jellyfin Server on your PC and Jellyfin android app, you should be able to set NrealVideoPlayer as the default external player 
Moreover anywhere there is a Share... or Open With... moment for video files, if you point them to this app, it will (ideally) Launch this XR viewer
You _may_ have to load the nebula app first each time, which would be a bummer, but maybe someday that won't be a hard requirement to enter XR mode

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

- [ ] Test MKV support

- [x] Set up shader to render half of sbs output to one eye, and the other half to the other eye

- [ ] Add on-phone file picker moment for choosing a local file to play

- [x] basic playback controls

- [x] basic tracking mode-switching

---

#### Short-Term

- [x] set up repo

- [x] import unity base project

- [x] ~Proof-of-concept get mpv rendering to a surface in Unity
	(Base off of NRSDK\Demos\RGBCamera-Capture.unity (it has an example Unity Video player surface)~ Did it using VLC for Unity plugin instead

- [ ] mock up virtual controller with an "input lock" toggle that requies a double-tap to bring the controls back. so you can put phone in pocket while viewing

- [x] add basic controls: stop, pause, play, seek, volume, source url input

- [x] Compare MPV and VLC codec support, maybe it makes more sense to base this on [VLC Android](https://github.com/videolan/vlc-android) **going with VLC**

- [ ] add control to toggle SBS and over-under video format playback

- [ ] detect and account for half-sbs vs full-sbs

#### Mid-Term

- [x] toggle for tracking mode (6dof, 3dof, 0dof, 0dof stablized)

#### Long-Term

- [ ] ability to pin player anywhere around you (facing you)
- [ ] ability to pin player against surfaces
- [ ] test multi audio track switching
- [ ] test subtitles


MPV Feature Overview:
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
