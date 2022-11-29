# MPV-Android-Nreal
A Unity Android App wrapper around the MPV player, enabling SBS and OU 3D Stereo video playback of local files AND files served over Google Drive or other Network Storage Servers

# NOTE
If you use Jellyfin Server on your PC and Jellyfin android app, you should be able to set MPV-Android-Nreal as the default external player 
Moreover anywhere there is a Share... or Open With... moment for video files, if you point them to this app, it will Launch this XR viewer

### TODOs

#### Short-Term

- [x] set up repo

- [ ] import unity base project

- [ ] Proof-of-concept get mpv rendering to a surface in Unity
	(Base off of NRSDK\Demos\RGBCamera-Capture.unity (it has an example Unity Video player surface)

- [ ] mock up virtual controller with an "input lock" toggle that requies a double-tap to bring the controls back. so you can put phone in pocket while viewing

- [ ] Compare MPV and VLC codec support, maybe it makes more sense to base this on [VLC Android](https://github.com/videolan/vlc-android)

- [ ] add control to toggle SBS and over-under video format playback

#### Mid-Term

- [] toggle for tracking mode (6dof, 3dof, 0dof, 0dof stablized)

#### Long-Term

- [ ] ability to pin player anywhere around you (facing you)
- [ ] ability to pin player against surfaces


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
