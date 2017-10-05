using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace PlayGround
{
    using Utils;

    partial class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            //LinqPadLikeExtensions.Init(s=>MessageBox);
            var stemmer = new Stemmer(@"G:\Source\Repos\TextAnalyser\TextAnalyser\StemmerWrap\database", @"G:\Source\Repos\TextAnalyser\TextAnalyser\StemmerWrap\termTypes.json");
            stemmer.Lemmatize(new[] { "ცხენმა", "ლომს", "მგლის", "ტყეთა", "წლის" }).Dump();
        }
    }

}

