using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace VRCMediaAudioMirror
{
    public class AudioDelayRingbuffer
    {
        private float[] audioBuffer;
        private int bufferPointer = 0;
        private long samplesWritten = 0;
        private long samplesRead = 0;
        private MelonLogger.Instance LoggerInstance = new MelonLogger.Instance("AudioDelayRingbuffer");

        public AudioDelayRingbuffer(int sizeInSamples, int channels)
        {
            this.audioBuffer = new float[sizeInSamples * channels];
        }

        public void AddSamples(float[] data)
        {
            if (data.Length > audioBuffer.Length) return;

            for (int i = 0; i < data.Length; i++)
            {
                audioBuffer[bufferPointer] = data[i];
                bufferPointer = (bufferPointer + 1) % audioBuffer.Length;
            }

            samplesWritten += data.Length;
        }

        public float RetrieveSample(int offset)
        {
            samplesRead += 1;

            return audioBuffer[(bufferPointer + offset) % audioBuffer.Length];
        }

        public bool HasSamples()
        {
            return samplesWritten > samplesRead;
        }

        public void Clear()
        {
            Array.Clear(audioBuffer, 0, audioBuffer.Length);
            samplesWritten = 0;
            samplesRead = 0;
        }
    }
}
