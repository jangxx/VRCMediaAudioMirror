using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using VRC;
using UnhollowerRuntimeLib;
using NAudio.Wave;
using UIExpansionKit;

namespace VRCMediaAudioMirror
{
    public class MediaAudioMirrorMain : MelonMod
    {
        private const int MAX_RETRIES = 10;

        private int ticksSinceLastUpdate = 0;
        // in the beginning were gonna try to get the player more frequently for a few attempts
        // and then drop the check frequency way down
        private int getPlayerRetries = 0;
        private ISet<int> mirroredObjectIds = new HashSet<int>();

        private float pref_Volume = 1;

        private WaveOutEvent waveOutEvent;
        private MixingWaveProvider16 globalMixer;
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
            ClassInjector.RegisterTypeInIl2Cpp<AudioMirrorFilter>();

            MelonPreferences.CreateCategory("PlayerAudioMirror", "Player Audio Mirror");
            MelonPreferences.CreateEntry<float>("PlayerAudioMirror", "Volume", 1f, "Volume (0.0-1.0)");

            this.settingsUi.AudioDeviceChangedEvent += OnAudioDeviceChanged;
            this.settingsUi.Init();

            OnPreferencesSaved(); // load preferences on initialization

            //this.waveOutEvent.Play();
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

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            this.mirroredObjectIds = new HashSet<int>();
            this.ticksSinceLastUpdate = 0;
            this.getPlayerRetries = 0;
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

        public override void OnUpdate()
        {
            ticksSinceLastUpdate += 1;

            if ((getPlayerRetries < MAX_RETRIES && ticksSinceLastUpdate == 90) || (ticksSinceLastUpdate == 90 * 10))
            {
                ticksSinceLastUpdate = 0;
                getPlayerRetries += 1;
                //this.audioSources = new List<AudioSource>();

                var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

                if (getPlayerRetries < 10)
                {
                    LoggerInstance.Msg("Found " + players.Length + " speakers");
                }

                int count = 0;

                foreach (var player in players)
                {
                    AudioSource source = player.GetComponent<AudioSource>();

                    if (source != null && source.gameObject != null && source.isPlaying && !this.mirroredObjectIds.Contains(source.gameObject.GetInstanceID()))
                    {
                        LoggerInstance.Msg("Found AudioSource");

                        var filter = source.gameObject.AddComponent<AudioMirrorFilter>();
                        filter.SetParentMixer(globalMixer);
                        filter.enabled = isEnabled;

                        this.mirroredObjectIds.Add(source.gameObject.GetInstanceID());
                        count += 1;
                        getPlayerRetries = MAX_RETRIES; // if we're successful once just go into slow update mode directly
                    }
                }

                if (getPlayerRetries < 10 || count > 0)
                {
                    LoggerInstance.Msg("Updated " + count + " audio sources");
                }
            }
        }
    }
}
