using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using VRC;
using UnhollowerRuntimeLib;

namespace VRCPlayerAudioMirror
{
    public class PlayerAudioMirror : MelonMod
    {
        //public override void OnUpdate()
        //{
        //    if (Input.GetKeyDown(KeyCode.T))
        //    {
        //        MelonLogger.Msg("Hello World");
        //    }
        //}

        private List<AudioSource> audioSources = new List<AudioSource>();
        private int ticksSinceLastUpdate = 0;

        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AudioMirrorFilter>();
        }

        //public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        //{
        //    MelonLogger.Msg("Scene " + buildIndex + " was LOADED with name " + sceneName);
        //    var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

        //    MelonLogger.Msg("Found " + players.Length + " speakers");

        //    foreach (var player in players)
        //    {
        //        AudioSource source = player.GetComponent<AudioSource>();

        //        if (source != null && source.gameObject != null)
        //        {
        //            MelonLogger.Msg("Found AudioSource, adding MirrorFilter to parent GameObject");
        //            source.gameObject.AddComponent<AudioMirrorFilter>();
        //        }
        //    }
        //}

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            this.audioSources = new List<AudioSource>();

            MelonLogger.Msg("Scene " + buildIndex + " was INITIALIZED with name " + sceneName);
            var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

            MelonLogger.Msg("Found " + players.Length + " speakers");

            foreach (var player in players)
            {
                AudioSource source = player.GetComponent<AudioSource>();

                if (source != null && source.gameObject != null)
                {
                    MelonLogger.Msg("Found AudioSource");
                    this.audioSources.Append(source);
                    //break;
                    //source.gameObject.AddComponent<AudioMirrorFilter>();
                }
            }

            //MelonLogger.Msg(this.audioSources);
        }

        public override void OnUpdate()
        {
            ticksSinceLastUpdate += 1;

            if (ticksSinceLastUpdate == 1000)
            {
                this.audioSources = new List<AudioSource>();

                var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();
                MelonLogger.Msg("Found " + players.Length + " speakers");

                foreach (var player in players)
                {
                    AudioSource source = player.GetComponent<AudioSource>();

                    if (source != null && source.gameObject != null)
                    {
                        MelonLogger.Msg("Found AudioSource");
                        this.audioSources.Add(source);
                        //break;
                        //source.gameObject.AddComponent<AudioMirrorFilter>();
                    }
                }

                MelonLogger.Msg("Updated " + this.audioSources.Count + " audio sources");
            }
        }

        public override void OnFixedUpdate()
        {
            //MelonLogger.Msg("OnFidexUpdate, " + this.audioSources);

            if (this.audioSources == null) return;

            //var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

            //foreach (var player in players)
            //{
            //    AudioSource source = player.GetComponent<AudioSource>();

            foreach (var source in this.audioSources)
            { 
                if (source.isPlaying)
                {
                    float[] audioData = source.GetOutputData(1024, 0);
                    MelonLogger.Msg("Got " + audioData.Length + " samples with audioData[0] = " + audioData[0]);
                }
            }
        }
    }
}
