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
using Utils.TextLoggerNet;

namespace PlayGroundNET
{


    class Program
    {
        static void Main(string[] args)
        {
            ReadAndPlay(args);
        }



        static void ReadAndPlay(string[] args)
        {
            var initialFilesDir = @"G:\Data\Initial";
            var convertedFilesDir = @"G:\Data\Converted".CreateDirIfNotExists();
            var toTextFilesDir = @"G:\Data\ToTexts2".CreateDirIfNotExists();

            int sampleRate = 8000;// 44100;
            int bitsPerSample = 16;// 16;
            int channels = 1;// 2;

            var wav = new WaveFormat(sampleRate, bitsPerSample, channels);
            var wavGood = new WaveFormat(44100, 16, 2);
            toTextFilesDir.EnsureDirEmpty();
            //Parallel.ForEach(Directory.EnumerateFiles(initialFilesDir), fileName =>
            var chain = new SampleDeepChain(3);
            foreach (var fileName in Directory.EnumerateFiles(initialFilesDir, "*.mp3"))
            {
                Console.WriteLine($"{DateTime.Now}Decoding:{fileName}");

                var rdr = new MediaFoundationDecoder(fileName);// new WaveFileReader(fileName);
                //Player.Play(rdr);
                var timeChunk = TimeSpan.FromSeconds(10);
                var chunks = new DmoResampler(rdr, wav).ToShorts(wav, timeChunk);
                foreach (var chunk in chunks)
                {
                    Console.WriteLine($"{DateTime.Now}Feeding:{fileName} chunk: {timeChunk.ToVerboseStringHMS()}");
                    try
                    {
                        chain.feed(chunk);
                    }
                    catch (OutOfMemoryException e)
                    {
                        chain.save(fileName + $"{chunks.Count()}.xml");
                        chain = new SampleDeepChain(3);
                    }

                }


                Console.WriteLine($"{DateTime.Now}Fed:{fileName}, Saving");
                chain.save(fileName + $"_Final.xml");

                Console.WriteLine($"{DateTime.Now}Saved:{fileName}");

                continue;
                List<short[]> randomSounds = new List<short[]>();
                for (int i = 0; i < 10000; i++)
                {
                    var randomSound = chain.generateSentence();

                    randomSounds.Add(randomSound);
                }

                Player.Play(randomSounds.SelectMany(s => s).ToArray().ToAudioAgain(wav));
                //{
                //    var outPath = Path.Combine(toTextFilesDir,
                //        Path.GetFileNameWithoutExtension(fileName) + $"_chunk_{0}.txt");
                //    using (var fs = new StreamWriter(File.OpenWrite(outPath)))
                //        foreach (var shortval in chunk)
                //        {
                //            fs.Write(shortval + " ");
                //        }
                //}
            }
            //);
        }
        static void Convert(string[] args)
        {
            var initialFilesDir = @"G:\Data\Initial";
            var convertedFilesDir = @"G:\Data\Converted".CreateDirIfNotExists();
            var toTextFilesDir = @"G:\Data\ToTexts2".CreateDirIfNotExists();

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
