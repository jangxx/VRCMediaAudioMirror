using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using VRC;
using VRCSDK2;
using UnhollowerRuntimeLib;
using NAudio.Wave;
using UIExpansionKit;

namespace VRCMediaAudioMirror
{
    public class MediaAudioMirrorMain : MelonMod
    {
        private const int MAX_RETRIES = 30;

        private int ticksSinceLastUpdate = 0;
        // in the beginning were gonna try to get the player more frequently for a few attempts
        // and then drop the check frequency way down
        private int getPlayerRetries = 0;
        private ISet<int> mirroredObjectIds = new HashSet<int>();

        private WaveOutEvent waveOutEvent;
        private MixingWaveProvider16 globalMixer;

        private float pref_Volume = 1;
        private bool isEnabled = false;

        private QuickMenuSettings settingsUi = new QuickMenuSettings();

        public MediaAudioMirrorMain()
        {

            //this.waveOutEvent = new WaveOutEvent() {  DesiredLatency = 100 };
            this.globalMixer = new MixingWaveProvider16();

            //this.waveOutEvent.Init(this.globalMixer);
        }

        public override void OnApplicationStart()
        {
            foreach (var resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                LoggerInstance.Msg(resource);
            }

            ClassInjector.RegisterTypeInIl2Cpp<AudioMirrorFilter>();

            MelonPreferences.CreateCategory("PlayerAudioMirror", "Player Audio Mirror");
            MelonPreferences.CreateEntry<float>("PlayerAudioMirror", "Volume", 1f, "Volume (0.0-1.0)");

            this.settingsUi.AudioDeviceChangedEvent += OnAudioDeviceChanged;
            this.settingsUi.MenuInteractionEvent += OnMenuInteraction;
            this.settingsUi.Init();

            OnPreferencesSaved(); // load preferences on initialization

            //this.waveOutEvent.Play();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            this.mirroredObjectIds = new HashSet<int>();
            this.ticksSinceLastUpdate = 0;
            this.getPlayerRetries = 0;
        }

        public override void OnPreferencesSaved()
        {
            pref_Volume = MelonPreferences.GetEntryValue<float>("PlayerAudioMirror", "Volume");
            if (this.waveOutEvent != null)
            {
                this.waveOutEvent.Volume = Math.Max(0, Math.Min(1, pref_Volume));
            }
        }

        public void OnAudioDeviceChanged(object sender, EventArgs args)
        {
            var audioEventArgs = (AudioDeviceChangedEventArgs)args;

            if (this.waveOutEvent != null)
            {
                this.waveOutEvent.Stop();
                this.waveOutEvent.Dispose();
            }

            if (audioEventArgs.Disable)
            {
                isEnabled = false;
                var filters = UnityEngine.Object.FindObjectsOfType<AudioMirrorFilter>();
                foreach (var filter in filters)
                {
                    filter.enabled = isEnabled;
                }
            }
            else
            {
                LoggerInstance.Msg("Changing audio device to " + audioEventArgs.DeviceNumber);

                this.waveOutEvent = new WaveOutEvent() { DesiredLatency = 100, DeviceNumber = audioEventArgs.DeviceNumber, Volume = pref_Volume };
                this.waveOutEvent.Init(this.globalMixer);
                isEnabled = true;

                // clear all buffers to remove any delay that could be caused by the switch
                var filters = UnityEngine.Object.FindObjectsOfType<AudioMirrorFilter>();
                foreach (var filter in filters)
                {
                    filter.ClearBuffer();
                    filter.enabled = isEnabled;
                }

                this.waveOutEvent.Play();
            }
        }

        public void OnMenuInteraction(object sender, EventArgs args)
        {
            var menuEventArgs = (MenuInteractionEventArgs)args;

            switch(menuEventArgs.Type)
            {
                case MenuInteractionEventArgs.MenuInteractionType.TRIGGER_UPDATE:
                    UpdateAndHookAudioSources(true, !menuEventArgs.BoolParams[0]);
                    break;

                case MenuInteractionEventArgs.MenuInteractionType.UNHOOK_ALL:
                    DestroyAllFilters();
                    break;
            }
        }

        public void EnableAllFilters(bool enabled)
        {
            var filters = UnityEngine.Object.FindObjectsOfType<AudioMirrorFilter>();

            LoggerInstance.Msg("Found " + filters.Count + " filters to enable/disable");

            foreach (var filter in filters)
            {
                filter.enabled = enabled;
            }
        }

        public void DestroyAllFilters()
        {
            var filters = UnityEngine.Object.FindObjectsOfType<AudioMirrorFilter>();

            LoggerInstance.Msg("Found " + filters.Count + " filters to destroy");

            foreach (var filter in filters)
            {
                UnityEngine.Object.Destroy(filter);
            }

            this.mirroredObjectIds = new HashSet<int>();
            settingsUi.UpdateStatusMenuStatus(new StatusMenuStatus() { CurrentlyHooked = this.mirroredObjectIds.Count });
        }

        public override void OnUpdate()
        {
            ticksSinceLastUpdate += 1;

            if (getPlayerRetries < MAX_RETRIES && ticksSinceLastUpdate == 90)
            {
                ticksSinceLastUpdate = 0;
                getPlayerRetries += 1;

                var count = UpdateAndHookAudioSources(getPlayerRetries < MAX_RETRIES);
                if (count > 0)
                {
                    getPlayerRetries = MAX_RETRIES; // if we're successful once finish the initial updating phase
                }
            }
        }

        public int UpdateAndHookAudioSources(bool showLog = true, bool useFilter = true, bool hookEverything = false)
        {
            ISet<Component> components = new HashSet<Component>();

            if (useFilter || !hookEverything)
            {
                int playerCount = 0;
                var avpro_players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

                components.UnionWith(avpro_players);
                playerCount += avpro_players.Count;

                // special case to support usharpvideo
                var usharpvideo = GameObject.Find("USharpVideo");

                if (usharpvideo != null)
                {
                    components.UnionWith(usharpvideo.GetComponentsInChildren<AudioSource>());
                }

                var sdk2_players = UnityEngine.Object.FindObjectsOfType<VRCSDK2.VRC_VideoSpeaker>();

                components.UnionWith(sdk2_players);
                playerCount += sdk2_players.Count;

                if (showLog)
                {
                    LoggerInstance.Msg("Found " + playerCount + " speakers, " + components.Count + " components");
                }
            } 
            else
            {
                var allAudioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
                
                components.UnionWith(allAudioSources);

                if (showLog)
                {
                    LoggerInstance.Msg("Found " + components.Count + " components");
                }
            }

            int count = 0;

            foreach (var component in components)
            {
                AudioSource source = component.GetComponent<AudioSource>();

                if (source != null
                    && source.gameObject != null
                    && (source.isPlaying || hookEverything) // hook everything means we also attach to non-playing sources
                    && source.outputAudioMixerGroup != null
                    && source.outputAudioMixerGroup.name == "World"
                    && !this.mirroredObjectIds.Contains(source.gameObject.GetInstanceID()))
                {
                    LoggerInstance.Msg("Found AudioSource");

                    var filter = source.gameObject.AddComponent<AudioMirrorFilter>();
                    filter.SetParentMixer(this.globalMixer);
                    filter.enabled = isEnabled;

                    this.mirroredObjectIds.Add(source.gameObject.GetInstanceID());
                    count += 1;
                }
            }

            if (showLog || count > 0)
            {
                LoggerInstance.Msg("Updated " + count + " audio sources");
            }

            settingsUi.UpdateStatusMenuStatus(new StatusMenuStatus() { CurrentlyHooked = this.mirroredObjectIds.Count });

            return count;
        }
    }
}
