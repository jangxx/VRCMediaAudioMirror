using System;
using UnityEngine;
using MelonLoader;
using UnhollowerBaseLib;
using NAudio.Wave;

namespace VRCPlayerAudioMirror
{
    //[UnityEngine.RequireComponent(typeof(AudioSource))]
    public class AudioMirrorFilter : MonoBehaviour
    {
        private System.Random rand = new System.Random();
        private AudioSource source;
        private MelonLogger.Instance LoggerInstance = new MelonLogger.Instance("PlayerAudioMirror");
        private BufferedWaveProvider waveProvider;
        private WaveOutEvent waveOutEvent;
        //private WaveFileWriter waveFileWriter;
        private bool finished = false;
        private byte[] audioBuffer = new byte[2048 * 2];

        public AudioMirrorFilter(IntPtr obj0) : base(obj0)
        {
            var waveFormat = new WaveFormat(48000, 16, 2);
            this.waveProvider = new BufferedWaveProvider(waveFormat);
            this.waveOutEvent = new WaveOutEvent();
            this.waveOutEvent.Init(this.waveProvider);
            //this.waveFileWriter = new WaveFileWriter(waveFileWriter)
            //WaveFileWriter.CreateWaveFile(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav", this.waveProvider);
        }

        public void Start()
        {
            LoggerInstance.Msg("AudioMirrorFilter was started");
            this.source = this.gameObject.GetComponent<AudioSource>();
            this.waveOutEvent.Play();
        }

        public void OnDestroy()
        {
            this.waveOutEvent.Dispose();
        }

        public void FinishWav()
        {
            if (!finished)
            {
                finished = true;
            }
        }

        public void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
        {
            if (this.finished) return;

            //var processData = new float[data.Length];
            //data.CopyTo(processData, 0);

            //for (int i = 0; i < processData.Length; i++)
            //{
            //    processData[i] *= 0.5f;
            //}

            //Buffer.BlockCopy(processData, 0, audioBuffer, 0, audioBuffer.Length);

            int sampleIndex = 0;
            int pcmIndex = 0;

            while (sampleIndex < data.Length)
            {
                var outsample = (short)(data[sampleIndex] * short.MaxValue);
                audioBuffer[pcmIndex] = (byte)(outsample & 0xff);
                audioBuffer[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            waveProvider.AddSamples(audioBuffer, 0, audioBuffer.Length);
            //if (this.source != null)
            //{
            //    float[] audioData = this.source.GetOutputData(data.Length / channels, 0);
            //    MelonLogger.Msg("Got " + audioData.Length + " samples with audioData[0] = " + audioData[0]);
            //}

            //if (rand.NextDouble() < 0.1)
            //{
            //MelonLogger.Msg("Got " + data.Length + " pieces of audio data for " + channels + " channels, data[0] = " + data[0]);

            //if (!this.writingFinished)
            //{
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
                    //MelonLogger.Msg("Total samples: " + writer.getSampleCount());

                    //short[] shortSamples = new short[data.Length];
                    //for (int i = 0; i < data.Length; i++)
                    //{
                    //    //shortSamples[i] = (short)(data[i] * 32768);
                    //    waveProvider.
                    //}
                    //writer.addSamples(shortSamples, channels);
                    //MelonLogger.Msg("Wrote " + shortSamples.Length + " samples to WavWriter");
                //}
            //}

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
