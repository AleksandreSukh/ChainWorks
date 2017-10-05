using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PlayGround;
using Utils;

namespace PlayGroundNET
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            LinqPadLikeExtensions.Init(s=>MessageBox.Show(s));
            var stemmer = new Stemmer(@"G:\Source\Repos\TextAnalyser\TextAnalyser\StemmerWrap\database", @"G:\Source\Repos\TextAnalyser\TextAnalyser\StemmerWrap\termTypes.json");
            stemmer.Lemmatize(new[] { "ცხენმა", "ლომს", "მგლის", "ტყეთა", "წლის" }).Dump();
            Console.ReadKey();

        }
    }
}
