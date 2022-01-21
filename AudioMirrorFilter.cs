using System;
using UnityEngine;
using MelonLoader;
using UnhollowerBaseLib;
using NAudio.Wave;

namespace VRCPlayerAudioMirror
{
    public class AudioMirrorFilter : MonoBehaviour
    {
        private MelonLogger.Instance LoggerInstance = new MelonLogger.Instance("AudioMirrorFilter");
        private BufferedWaveProvider waveProvider;
        private MixingWaveProvider16 parentMixer;

        private byte[] audioBuffer = new byte[2048 * 2];

        public AudioMirrorFilter(IntPtr obj0) : base(obj0)
        {
            var waveFormat = new WaveFormat(48000, 16, 2);
            this.waveProvider = new BufferedWaveProvider(waveFormat);
        }

        public void SetParentMixer(MixingWaveProvider16 mixer)
        {
            this.parentMixer = mixer;
            this.parentMixer.AddInputStream(waveProvider);
            LoggerInstance.Msg("Added filter to parent mixer");
        }

        public void Start()
        {
            LoggerInstance.Msg("AudioMirrorFilter was started");
        }

        public void ClearBuffer()
        {
            waveProvider.ClearBuffer();
        }

        public void OnDestroy()
        {
            if (this.parentMixer != null)
            {
                this.parentMixer.RemoveInputStream(waveProvider);
            }
        }

        public void OnDisable()
        {
            LoggerInstance.Msg("AudioMirrorFilter was disabled");
            waveProvider.ClearBuffer();
        }

        public void OnAudioFilterRead(Il2CppStructArray<float> data, int channels)
        {
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

            try
            {
                waveProvider.AddSamples(audioBuffer, 0, audioBuffer.Length);
            } 
            catch (System.InvalidOperationException e)
            {
                waveProvider.ClearBuffer();
            }
        }
    }
}
