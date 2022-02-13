using System;
using System.Collections.Generic;
using NAudio.Wave;
using MelonLoader;

namespace VRCMediaAudioMirror
{
    /// <summary>
    /// WaveProvider that mixes together one or more 16 bit integer streams
    /// Mostly copy-pasted from the MixingWaveProvider32 from NAudio.Wave with a few modifications:
    /// - always returns a full buffer so the output doesn't stop
    /// - summing is done without unsafe operations
    /// </summary>
    public class MixingWaveProvider16 : IWaveProvider
    {
        private List<IWaveProvider> inputs;
        private WaveFormat waveFormat;
        private int bytesPerSample;
        private bool allowMultipleInputs = false; // do we _actually_ allow multiple inputs to send data? if set to false we only take samples from the first input that supplies them and discard the rest
        private MelonLogger.Instance LoggerInstance = new MelonLogger.Instance("VRCMediaAudioMirror::MixingWaveProvider16");

        public MixingWaveProvider16()
        {
            this.waveFormat = new WaveFormat(48000, 16, 2);
            this.bytesPerSample = 2;
            this.inputs = new List<IWaveProvider>();
        }

        public void AddInputStream(IWaveProvider waveProvider)
        {
            if (waveProvider.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                throw new ArgumentException("Must be PCM", "waveProvider.WaveFormat");
            if (waveProvider.WaveFormat.BitsPerSample != 16)
                throw new ArgumentException("Only 16 bit audio currently supported", "waveProvider.WaveFormat");

            if (inputs.Count == 0)
            {
                // first one - set the format
                int sampleRate = waveProvider.WaveFormat.SampleRate;
                int channels = waveProvider.WaveFormat.Channels;
                this.waveFormat = new WaveFormat(sampleRate, 16, channels);
            }
            else
            {
                if (!waveProvider.WaveFormat.Equals(waveFormat))
                    throw new ArgumentException("All incoming channels must have the same format", "waveProvider.WaveFormat");
            }

            lock (inputs)
            {
                this.inputs.Add(waveProvider);
            }
        }

        public void SetAllowMultipleInputs(bool value)
        {
            lock (inputs)
            {
                this.allowMultipleInputs = value;
            }

            //if (value)
            //{
            //    LoggerInstance.Msg("Allowing multiple inputs");
            //} 
            //else
            //{
            //    LoggerInstance.Msg("Not allowing multiple inputs");
            //}
        }

        public void RemoveInputStream(IWaveProvider waveProvider)
        {
            lock (inputs)
            {
                this.inputs.Remove(waveProvider);
            }
        }

        public int InputCount
        {
            get { return this.inputs.Count; }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (count % bytesPerSample != 0)
            {
                throw new ArgumentException("Must read an whole number of samples", "count");
            }

            // blank the buffer
            Array.Clear(buffer, offset, count);
            int bytesRead = 0;

            //LoggerInstance.Msg("MixingWaveProvider16 Read " + offset + ", " + count);

            // sum the channels in
            byte[] readBuffer = new byte[count];
            lock (inputs)
            {
                bool alreadyGotSamples = false;

                foreach (var input in inputs)
                {
                    int readFromThisStream = input.Read(readBuffer, 0, count);

                    bool containsData = false;

                    // skip the data check if we allow multiple inputs anyway
                    if (!allowMultipleInputs)
                    {
                        containsData = !IsAllZero(readBuffer);
                    }

                    // don't worry if input stream returns less than we requested - may indicate we have got to the end
                    bytesRead = Math.Max(bytesRead, readFromThisStream);
                    if (readFromThisStream > 0 && (allowMultipleInputs || (containsData && !alreadyGotSamples)))
                    {
                        Sum16BitAudio(buffer, offset, readBuffer, readFromThisStream);
                        alreadyGotSamples = true;
                    }
                }
            }

            // we zeroed the buffer so we can just say we returned all zeroes in case there are no inputs
            if (inputs.Count == 0)
            {
                bytesRead = count;
            }

            return bytesRead;
        }

        /// <summary>
        /// Actually performs the mixing
        /// </summary>
        void Sum16BitAudio(byte[] destBuffer, int offset, byte[] sourceBuffer, int bytesRead)
        {
            int samplesRead = bytesRead / 2;
            for (int n = 0; n < samplesRead; n++)
            {
                short src = GetShortFromBuffer(sourceBuffer, n * 2);
                short dst = GetShortFromBuffer(destBuffer, n * 2);
                WriteShortToBuffer(destBuffer, n * 2, ClampToShort(dst + src));
            }
        }

        static short GetShortFromBuffer(byte[] buffer, int offset)
        {
            short lower = buffer[offset];
            short upper = buffer[offset + 1];
            return (short)(lower + (upper << 8));
        }

        static void WriteShortToBuffer(byte[] buffer, int offset, short value)
        {
            buffer[offset] = (byte)(value & 0xff);
            buffer[offset + 1] = (byte)((value >> 8) & 0xff);
        }

        static short ClampToShort(int input)
        {
            return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, input));
        }

        static bool IsAllZero(byte[] buffer)
        {
            foreach (byte b in buffer)
            {
                if (b != 0) return false;
            }
            return true;
        }

        public WaveFormat WaveFormat
        {
            get { return this.waveFormat; }
        }
    }
}
