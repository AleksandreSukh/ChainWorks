using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using SoundDataUtils;

namespace Sample_AudioToText
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = DateTime.Now.ToString("yyMMddHHMMss") + ".wav";
            var txtFile = fileName + ".txt";
            //var outputFile = fileName + ".random.mp3";
            var audioLength = TimeSpan.FromSeconds(10);

            //NOTE! commented out values are better quality configurations but it generates audio which is 10 times larger in size
            //So I chose lowest hearable quality to reduce processing time to 1/10 of this standard (good) quality audio

            int sampleRate = 8000;// 44100;
            int bitsPerSample = 16;// 16;
            int channels = 1;// 2;

            var wav = new WaveFormat(sampleRate, bitsPerSample, channels);


            Recorder.RecordTo(fileName, audioLength, wav);
            Console.WriteLine("Playing initial audio");
            Player.Play(fileName);

            Console.WriteLine("Converting audio to base64 string");
            var tstring = CodecFactory.Instance.GetCodec(fileName).Tob64String(wav);

            Console.WriteLine("Writing converted base64 string:");

            Console.WriteLine(tstring);

            //Now write this text to file and read from it to ensure data consistency

            //You can use ASCII encoding for smaller size text file 
            var textEncoding = Encoding.ASCII;
            File.WriteAllText(txtFile, tstring, textEncoding);
            var allBase64s = File.ReadAllText(txtFile, textEncoding);



            Player.Play(AudioToText.ToAudioAgain(allBase64s, wav));

        }

    }
    public class AudioConverter
    {
        public static void Convert(string inputPath, string outDir, WaveFormat targetFormat)
        {
            IWaveSource source;
            source = CodecFactory.Instance.GetCodec(inputPath);
            var target = new DmoResampler(source, targetFormat);

            var writer = new WaveWriter(Path.Combine(outDir, Path.GetFileName(inputPath)), target.WaveFormat);
            byte[] buffer = new byte[target.WaveFormat.BytesPerSecond / 2];
            int read;
            while ((read = target.Read(buffer, 0, buffer.Length)) > 0)
                writer.Write(buffer, 0, read);
        }

    }
}
