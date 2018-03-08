using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.Codecs.WAV;
using CSCore.MediaFoundation;

namespace SoundDataUtils
{
    public static class AudioToText
    {
        public static IEnumerable<byte[]> GetByteChunks(this string base64String, char separator = ' ')
        {
            var samples64 = base64String.Split(separator);
            return GetByteChunks(samples64);
            //foreach (var bytese in GetByteChunks(samples64)) yield return bytese;
        }

        private static IEnumerable<byte[]> GetByteChunks(string[] samples64)
        {
            foreach (var element in samples64.Select(s => Convert.FromBase64String(s)))
            {
                yield return element;
            }
        }
        private static IEnumerable<byte[]> GetByteChunks(short[] samples64)
        {
            foreach (var element in samples64.Select(s => BitConverter.GetBytes(s)))
            {
                yield return element;
            }
        }

        public static string Tob64String(this IWaveSource source, WaveFormat format)
        {
            var listOfChunks = string.Empty;
            int ctr = 0;

            byte[] buffer = new byte[format.BytesPerSecond / format.SampleRate];
            int read;

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                var base64 = Convert.ToBase64String(buffer);
                listOfChunks += base64;
                if (read == buffer.Length)
                    listOfChunks += ' ';
                if (ctr++ % 1000 == 0)
                    Console.WriteLine(Math.Round((source.Position / (double)source.Length) * 100));
            }
            return listOfChunks;

        }
        public static IEnumerable<string> Tob64Strings(this IWaveSource source, WaveFormat format, TimeSpan timeChunks)
        {
            var listOfChunks = string.Empty;
            int ctr = 0;

            byte[] buffer = new byte[format.BytesPerSecond / format.SampleRate];
            int read;

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                var base64 = Convert.ToBase64String(buffer);
                listOfChunks += base64;
                if (read == buffer.Length)
                    listOfChunks += ' ';
                ctr++;
                if (ctr >= format.SampleRate * timeChunks.TotalSeconds)
                {
                    ctr = 0;
                    yield return listOfChunks;
                    listOfChunks = string.Empty;
                }

                if (ctr % 1000 == 0)
                    Console.WriteLine(Math.Round((source.Position / (double)source.Length) * 100));
            }
            yield return listOfChunks;

        }
        public static short[] ToShortArray(this IWaveSource source, WaveFormat format)
        {
            var bytesPerSample = format.BytesPerSecond / format.SampleRate;

            var chunkLength = source.Length / bytesPerSample;
            var chunk = new short[chunkLength];

            byte[] buffer = new byte[bytesPerSample];
            int read;
            int ctr = 0;

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                chunk[ctr] = BitConverter.ToInt16(buffer, 0);
                ctr++;
            }
            return chunk;

        }
        public static IEnumerable<short[]> ToShorts(this IWaveSource source, WaveFormat format, TimeSpan timeChunks)
        {
            int ctr = 0;
            var bytesPerSample = format.BytesPerSecond / format.SampleRate;

            var chunkLength = (int)(format.SampleRate * timeChunks.TotalSeconds);
            var chunk = new short[(int)chunkLength];

            byte[] buffer = new byte[bytesPerSample];
            int read;

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {

                ctr++;
                if (ctr == chunkLength)
                {
                    yield return chunk;
                    ctr = 0;
                    chunk = new short[(int)chunkLength];
                }
                else
                {
                    chunk[ctr] = BitConverter.ToInt16(buffer, 0);
                }
#if DEBUG
                if ((ctr*bytesPerSample) % (source.Length / 100) == 0)
                {
                    Console.WriteLine(Math.Round((source.Position / (double)source.Length) * 100));
                }
#endif
            }

        }
        public static void ToAudioAgain(string base64Chunks, string outputFile, WaveFormat wav)
        {
            using (var encoder = MediaFoundationEncoder.CreateMP3Encoder(wav, outputFile))
            {
                foreach (var element in base64Chunks.GetByteChunks())
                {
                    encoder.Write(element, 0, element.Length);
                }
            }
        }

        public static IWaveSource ToAudioAgain(this string base64Chunks, WaveFormat format)
        {
            return new WaveFileReaderRaw(new MemoryStream(GetByteChunks(base64Chunks)
                .SelectMany(b => b).ToArray()), format);
        }

        public static IWaveSource ToAudioAgain(this short[] shortChunks, WaveFormat format)
        {
            return new WaveFileReaderRaw(new MemoryStream(GetByteChunks(shortChunks)
                .SelectMany(b => b).ToArray()), format);
        }

        public static IWaveSource ToAudioAgain(this string[] base64Chunks, WaveFormat format)
        {
            return new WaveFileReaderRaw(new MemoryStream(GetByteChunks(base64Chunks)
                .SelectMany(b => b).ToArray()), format);

            //var bytes = GetByteChunks(base64Chunks);
            //var ms = new MemoryStream();
            //{
            //    var waveWriter = new WaveWriter(ms, format);
            //    {
            //        foreach (var sample in bytes)
            //        {
            //            waveWriter.Write(sample, 0, sample.Length);
            //        }
            //    }

            //    return new WaveFileReaderRaw(ms, format);
            //    //			ms.Position = 0;
            //    //			return new WaveFileReader(ms);
            //    //return CodecFactory.Instance.GetCodec(ms, "wav");
            //}

        }
        //public static IWaveSource ToAudioAgain(this string base64Chunks, WaveFormat format)
        //{
        //    //using (
        //        var ms = new MemoryStream()
        //    ;
        //        //)
        //    {
        //        //using (
        //        var encoder = MediaFoundationEncoder.CreateMP3Encoder(format, ms);
        //        {
        //            foreach (var element in base64Chunks.GetByteChunks())
        //            {
        //                encoder.Write(element, 0, element.Length);
        //            }
        //        }
        //        ms.Position = 0;
        //        return new DmoMp3Decoder(ms);


        //    }
        //    //var bytes = GetByteChunks(base64Chunks);
        //    //var ms = new MemoryStream();
        //    //{
        //    //    var waveWriter = new WaveWriter(ms, format);
        //    //    {
        //    //        foreach (var sample in bytes)
        //    //        {
        //    //            waveWriter.Write(sample, 0, sample.Length);
        //    //        }
        //    //    }
        //    //    ms.Position = 0;
        //    //    return new WaveFileReader(ms);
        //    //    //return CodecFactory.Instance.GetCodec(ms, "wav");
        //    //}

        //}
    }
}