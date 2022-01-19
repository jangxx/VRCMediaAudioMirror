using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VRCPlayerAudioMirror
{
    internal class WavWriter
    {
        private FileStream fileStream;
        private BinaryWriter binaryWriter;
        //private List<short[]> samples;
        private List<short[]> samples;
        private int sampleCount = 0;
        private bool locked = false;

        public WavWriter(String filename)
        {
            this.fileStream = new FileStream(filename, FileMode.OpenOrCreate);
            this.binaryWriter = new BinaryWriter(this.fileStream);
            this.samples = new List<short[]>();
        }

        public void writeHeader(short samplelength, short numchannels, int samplerate)
        {
            binaryWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
            binaryWriter.Write(36 + (this.sampleCount * numchannels * samplelength));
            binaryWriter.Write(Encoding.ASCII.GetBytes("WAVE"));
            binaryWriter.Write(Encoding.ASCII.GetBytes("fmt "));
            binaryWriter.Write(16);
            binaryWriter.Write((short)1); // Encoding
            binaryWriter.Write((short)numchannels); // Channels
            binaryWriter.Write((int)(samplerate)); // Sample rate
            binaryWriter.Write((int)(samplerate * samplelength * numchannels)); // Average bytes per second
            binaryWriter.Write((short)(samplelength * numchannels)); // block align
            binaryWriter.Write((short)(8 * samplelength)); // bits per sample
            //binaryWriter.Write((short)(numsamples * samplelength)); // Extra size
            binaryWriter.Write(Encoding.ASCII.GetBytes("data"));
            binaryWriter.Write((int)this.sampleCount * numchannels * samplelength);
        }

        public int getSampleCount()
        {
            return this.sampleCount;
        }

        public void writeData()
        {
            this.locked = true;
            foreach(short[] samples in this.samples)
            {
                foreach(short sample in samples)
                {
                    binaryWriter.Write(sample);
                }
            }
            this.locked = false;
        }

        public void addSamples(short[] samples, int numchannels)
        {
            if (this.locked) return;

            this.samples.Add(samples);
            this.sampleCount += samples.Length / numchannels;
        }

        public void close()
        {
            binaryWriter.Close();
            fileStream.Close();
        }
    }
}
