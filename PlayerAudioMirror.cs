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

        //private List<AudioSource> audioSources = new List<AudioSource>();
        private int ticksSinceLastUpdate = 0;
        private ISet<int> mirroredObjectIds = new HashSet<int>();

        //private BufferedWaveProvider waveProvider;

        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AudioMirrorFilter>();

            //using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCPlayerAudioMirror.NAudio.dll"))
            //{
            //    byte[] assemblyData = new byte[stream.Length];
            //    stream.Read(assemblyData, 0, assemblyData.Length);
            //    System.Reflection.Assembly.Load(assemblyData);
            //}
            //using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCPlayerAudioMirror.NAudio.Core.dll"))
            //{
            //    byte[] assemblyData = new byte[stream.Length];
            //    stream.Read(assemblyData, 0, assemblyData.Length);
            //    System.Reflection.Assembly.Load(assemblyData);
            //}
            //using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCPlayerAudioMirror.NAudio.Wasapi.dll"))
            //{
            //    byte[] assemblyData = new byte[stream.Length];
            //    stream.Read(assemblyData, 0, assemblyData.Length);
            //    System.Reflection.Assembly.Load(assemblyData);
            //}
            //using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCPlayerAudioMirror.NAudio.WinMM.dll"))
            //{
            //    byte[] assemblyData = new byte[stream.Length];
            //    stream.Read(assemblyData, 0, assemblyData.Length);
            //    System.Reflection.Assembly.Load(assemblyData);
            //}

            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            //TestNAudioStuff();
        }

        //public void TestNAudioStuff()
        //{
        //    var waveFormat = new WaveFormat(48000, 16, 2);
        //    var waveProvider = new BufferedWaveProvider(waveFormat);
        //    WaveFileWriter.CreateWaveFile(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav", waveProvider);
        //}

        //private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    //using (var assembly = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCPlayerAudioMirror.NAudio.dll"))
        //    LoggerInstance.Msg("Trying to resolve " + args.Name);

        //    throw new NotImplementedException();
        //}

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
            this.mirroredObjectIds = new HashSet<int>();
            //this.audioSources = new List<AudioSource>();

            //MelonLogger.Msg("Scene " + buildIndex + " was INITIALIZED with name " + sceneName);
            //var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

            //MelonLogger.Msg("Found " + players.Length + " speakers");

            //foreach (var player in players)
            //{
            //    AudioSource source = player.GetComponent<AudioSource>();

            //    if (source != null && source.gameObject != null)
            //    {
            //        MelonLogger.Msg("Found AudioSource");
            //        this.audioSources.Append(source);
            //        //break;
            //        //source.gameObject.AddComponent<AudioMirrorFilter>();
            //    }
            //}

            //MelonLogger.Msg(this.audioSources);
        }

        public override void OnUpdate()
        {
            ticksSinceLastUpdate += 1;

            if (ticksSinceLastUpdate == 90*5)
            {
                ticksSinceLastUpdate = 0;
                //this.audioSources = new List<AudioSource>();

                var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();
                LoggerInstance.Msg("Found " + players.Length + " speakers");

                int count = 0;

                foreach (var player in players)
                {
                    AudioSource source = player.GetComponent<AudioSource>();

                    if (source != null && source.gameObject != null && source.isPlaying && !this.mirroredObjectIds.Contains(source.gameObject.GetInstanceID()))
                    {
                        LoggerInstance.Msg("Found AudioSource");
                        //this.audioSources.Add(source);
                        //break;
                        source.gameObject.AddComponent<AudioMirrorFilter>();
                        this.mirroredObjectIds.Add(source.gameObject.GetInstanceID());
                        count += 1;
                        
                    }
                }

                //MelonLogger.Msg("Updated " + this.audioSources.Count + " audio sources");
                LoggerInstance.Msg("Updated " + count + " audio sources");
            }

            if (Input.GetKey(KeyCode.L))
            {
                var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();
                foreach (var player in players)
                {
                    AudioMirrorFilter mirrorFilter = player.GetComponent<AudioMirrorFilter>();

                    if (mirrorFilter != null)
                    {
                        mirrorFilter.FinishWav();
                    }
                }
            }
        }

        //public override void OnFixedUpdate()
        //{
        //    //MelonLogger.Msg("OnFidexUpdate, " + this.audioSources);

        //    if (this.audioSources == null) return;

        //    //var players = UnityEngine.Object.FindObjectsOfType<VRC.SDK3.Video.Components.AVPro.VRCAVProVideoSpeaker>();

        //    //foreach (var player in players)
        //    //{
        //    //    AudioSource source = player.GetComponent<AudioSource>();

        //    foreach (var source in this.audioSources)
        //    { 
        //        if (source.isPlaying)
        //        {
        //            float[] audioData = source.GetOutputData(1024, 0);
        //            MelonLogger.Msg("Got " + audioData.Length + " samples with audioData[0] = " + audioData[0]);
        //        }
        //    }
        //}
    }
}
