using SoundDataUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;

namespace TextToAudio
{
    class Program
    {
        static int sampleRate = 8000;// 44100;
        static int bitsPerSample = 16;// 16;
        static int channels = 1;// 2;

        static WaveFormat wav = new WaveFormat(sampleRate, bitsPerSample, channels);

        static void Main(string[] args)
        {
            foreach (var file in Directory.EnumerateFiles(@"G:\Data\TestTextInput"))
            {
                Console.WriteLine(file);
                ConvertToAudioAndPlay(File.ReadAllText(file));
            }
        }
        private static void ConvertToAudioAndPlay(string allBase64S)
        {
            using (var src = allBase64S.ToAudioAgain(wav))
            {
                Task.Run(() => Console.WriteLine(string.Join(" ", allBase64S)));
                Player.Play(src);
            }
        }
        private static void ConvertToAudioAndPlay(string[] allBase64S)
        {
            using (var src = allBase64S.ToAudioAgain(wav))
            {
                Task.Run(() => Console.WriteLine(string.Join(" ", allBase64S)));
                Player.Play(src);
            }
        }
    }
}
