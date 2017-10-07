using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using PlayGround;
using SoundDataUtils;
using Utils;

namespace PlayGroundNET
{
    public static class Player
    {
        public static void Play(string filePath)
        {
            using (var enumerator = new MMDeviceEnumerator())
            using (var device = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active).Last())
            using (var source =
                CodecFactory.Instance.GetCodec(filePath)
                    .ToSampleSource()
                    .ToMono()
                    .ToWaveSource())

            using (
                var soundOut = new WasapiOut() { Latency = 100, Device = device })
            {
                soundOut.Initialize(source);
                soundOut.Play();
                Thread.Sleep(source.GetLength());
                soundOut.Stop();
            }
        }
        public static void Play(IWaveSource source)
        {
            using (var enumerator = new MMDeviceEnumerator())
            using (var device = enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active).Last())
            using (
                var soundOut = new WasapiOut() { Latency = 100, Device = device })
            {
                soundOut.Initialize(source);
                soundOut.Play();
                Thread.Sleep(source.GetLength());
                soundOut.Stop();
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            ReadAndPlay(args);
        }



        static void ReadAndPlay(string[] args)
        {

        }
        static void Convert(string[] args)
        {
            var initialFilesDir = @"G:\Data\Initial";
            var convertedFilesDir = @"G:\Data\Converted".CreateDirIfNotExists();
            var toTextFilesDir = @"G:\Data\ToTexts".CreateDirIfNotExists();

            int sampleRate = 8000;// 44100;
            int bitsPerSample = 16;// 16;
            int channels = 1;// 2;

            var wav = new WaveFormat(sampleRate, bitsPerSample, channels);
            var wavGood = new WaveFormat(44100, 16, 2);
            toTextFilesDir.EnsureDirEmpty();
            Parallel.ForEach(Directory.EnumerateFiles(initialFilesDir), fileName =>
            {
                //Player.Play(new DmoResampler(CodecFactory.Instance.GetCodec(fileName), wav));
                //Player.Play(new DmoResampler(CodecFactory.Instance.GetCodec(fileName), wavGood));
                //AudioConverter.Convert(fileName, convertedFilesDir, wav);
                int ctr = 1;
                var resampled = new DmoResampler(CodecFactory.Instance.GetCodec(fileName), wav);
                foreach (var chunk in resampled.Tob64Strings(wav, TimeSpan.FromSeconds(10)))
                {
                    var outPath = Path.Combine(toTextFilesDir,
                        Path.GetFileNameWithoutExtension(fileName) + $"_chunk_{ctr++}.txt");
                    File.WriteAllText(outPath, chunk);
                    ///*  AudioToText.ToAudioAgain(chunk, Path.Combine(Path.GetDirectoryName(outPath), */Path.GetFileNameWithoutExtension(outPath) + ".wav"), wav);

                }
            });

            //foreach (var fileName in Directory.EnumerateFiles(convertedFilesDir))
            //    File.WriteAllText(Path.Combine(toTextFilesDir.EnsureDirEmpty(), Path.GetFileNameWithoutExtension(fileName) + ".txt"), CodecFactory.Instance.GetCodec(fileName).Tob64String(wav));


            //Console.OutputEncoding = System.Text.Encoding.UTF8;
            //LinqPadLikeExtensions.Init(s=>MessageBox.Show(s));
            //var stemmer = new Stemmer(@"G:\Source\Repos\TextAnalyser\TextAnalyser\StemmerWrap\database", @"G:\Source\Repos\TextAnalyser\TextAnalyser\StemmerWrap\termTypes.json");
            //stemmer.Lemmatize(new[] { "ცხენმა", "ლომს", "მგლის", "ტყეთა", "წლის" }).Dump();
            ////Console.ReadKey();

        }
    }
}
