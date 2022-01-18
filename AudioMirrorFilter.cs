using System;
using UnityEngine;
using MelonLoader;
using UnhollowerBaseLib;

namespace VRCPlayerAudioMirror
{
    //[UnityEngine.RequireComponent(typeof(AudioSource))]
    public class AudioMirrorFilter : MonoBehaviour
    {
        private System.Random rand = new System.Random();
        private AudioSource source;

        public AudioMirrorFilter(IntPtr obj0) : base(obj0)
        {
        }

        public void Start()
        {
            MelonLogger.Msg("AudioMirrorFilter was started");
            this.source = this.gameObject.GetComponent<AudioSource>();
        }

        //public void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
        //{
        //    if (this.source != null)
        //    {
        //        float[] audioData = this.source.GetOutputData(data.Length / channels, 0);
        //        MelonLogger.Msg("Got " + audioData.Length + " samples with audioData[0] = " + audioData[0]);
        //    }

        //    //if (rand.NextDouble() < 0.1)
        //    //{
        //    //    MelonLogger.Msg("Got " + data.Length + " pieces of audio data for " + channels + " channels");
        //    ////}
        //    //for (int i = 0; i < data.Length; i++)
        //    //{
        //    //    if (i == 100)
        //    //    {
        //    //        MelonLogger.Msg("data[100] = " + data[i]);
        //    //    }
        //    //    data[i] *= 0.01f;
        //    //}
        //}
    }
}
