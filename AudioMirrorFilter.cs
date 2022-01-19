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
        private WavWriter writer;
        private bool writingFinished = false;

        public AudioMirrorFilter(IntPtr obj0) : base(obj0)
        {
            this.writer = new WavWriter(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav");
        }

        public void Start()
        {
            MelonLogger.Msg("AudioMirrorFilter was started");
            this.source = this.gameObject.GetComponent<AudioSource>();
        }

        public void FinishWav()
        {
            if (!this.writingFinished) {
                writer.writeHeader(2, 2, 48000);
                writer.writeData();
                writer.close();
                this.writingFinished = true;
                MelonLogger.Msg("Wrote header and data");
            }
        }

        public void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
        {
            //if (this.source != null)
            //{
            //    float[] audioData = this.source.GetOutputData(data.Length / channels, 0);
            //    MelonLogger.Msg("Got " + audioData.Length + " samples with audioData[0] = " + audioData[0]);
            //}

            //if (rand.NextDouble() < 0.1)
            //{
            //MelonLogger.Msg("Got " + data.Length + " pieces of audio data for " + channels + " channels, data[0] = " + data[0]);

            if (!this.writingFinished)
            {
                //if (writer.getSampleCount() > 48000 * 5)
                //{
                //    writer.writeHeader(2, 2, 48000);
                //    writer.writeData();
                //    writer.close();
                //    this.writingFinished = true;
                //    MelonLogger.Msg("Wrote header and data");
                //}
                //else
                //{
                    MelonLogger.Msg("Total samples: " + writer.getSampleCount());

                    short[] shortSamples = new short[data.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        shortSamples[i] = (short)(data[i] * 32768);
                    }
                    writer.addSamples(shortSamples, channels);
                    MelonLogger.Msg("Wrote " + shortSamples.Length + " samples to WavWriter");
                //}
            }

            ////}
            //for (int i = 0; i < data.Length; i++)
            //{
            //    if (i == 100)
            //    {
            //        MelonLogger.Msg("data[100] = " + data[i]);
            //    }
            //    data[i] *= 0.01f;
            //}
        }
    }
}
