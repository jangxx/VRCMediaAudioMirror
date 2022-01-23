# VRChat Media Audio Mirror

A mod for VRChat that redirects audio from AudioSources attached to media players in worlds to another output device at full volume.
The main purpose of this mod (for me) was to make my Woojer Vest finally usable in VRChat, but you could also use this to only play the music in your room on other speakers while still having all sounds including voices inside the headset.

## Installation

0. Have MelonLoader 0.5 or later installed.

1. Install UiExpansionKit, for example by using [VRCMelonAssistant](https://github.com/knah/VRCMelonAssistant).

2. Go to the [releases](https://github.com/jangxx/VRCMediaAudioMirror/releases) section and download the latest release.

3. Put NAudio.dll from _UserLibs/_ into the _UserLibs/_ folder within the VRChat directory.

4. Put MediaAudioMirror.dll into the _Mods/_ folder within the VRChat directory.

## Usage

The mod puts two button into the quick menu **Audio Mirror Setup** and **Audio Mirror Status**.

### Audio Mirror Setup

This is the main menu that you use to access the functionality of the mod.
You simply click on the name of an audio device and that immediately starts sending audio to that device.
Once you click on another one, the output will be switched over.

![Audio Mirror Setup screenshot](github/Audio_Mirror_Setup.png)

### Audio Mirror Status

This is an additional menu that you can open in case things don't work as well they should.
At the top you can see the number of AudioSources that are currently mirrored.
If the number is zero you can try clicking on *Retry hooking*.
Normally the mod will check the world for media players for the first 30 seconds after joining the world, but if a player gets created even later than that, a manual search can be required.

If the number is still 0 and no audio is playing you can try the nuclear option by checking _(Disable Filter)_.
This will make the *Retry hooking* button look for all AudioSources in the world that are playing (`isPlaying = true`) and that are feeding audio into the world mixer (i.e. they are controlled by the world volume slider).
These can (and probably will) include AudioSources that are not media players but sound effects for example though, so disabling this filter should really only be a last ditch attempt if everything else fails.

Finally, the _Unhook All_ button can be used to reset all attached filters in case you're not happy with the results of the _Retry hooking_ function.

![Audio Mirror Status screenshot](github/Audio_Mirror_Status.png)