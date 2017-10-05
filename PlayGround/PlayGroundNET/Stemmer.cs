using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PlayGround
{
    public class Stemmer
    {
        internal JsonSerializer Gson;
        internal TermType[] TermTypes;
        readonly HashSet<string> _termList = new HashSet<string>();

        public Stemmer(string nounDatabasePath, string termTypesPath)
        {
            Gson = new JsonSerializer();
            foreach (var s in File.ReadAllLines(nounDatabasePath).Distinct().OrderBy(a => a))
                _termList.Add(s);

            TermTypes = (TermType[])Gson.Deserialize(new JsonTextReader(new StringReader(File.ReadAllText(termTypesPath))), typeof(TermType[]));
        }

        string LemmatizeTerm(string obj0)
        {
            var result = (from termType in TermTypes
                          from termTypeForm in termType.Forms
                          let str1 = termTypeForm.TryMatchForm(obj0)
                          where str1 != null
                          let str2 = termType.GenerateStem(str1)
                          where _termList.Contains(str2)
                          select new { form = termTypeForm, stem = str2 })
                .OrderBy(a => a.form.Priority).Select(a => a.stem).FirstOrDefault();
            return result;
        }

        public string[] Lemmatize(string[] terms) => terms.Select(t => LemmatizeTerm(t)).ToArray();

      
    }
    public class TermType
    {
        public string Lemma { get; set; }
        public string Description { get; set; }
        public TermTypeForm[] Forms { get; set; }

        public string GenerateStem(string partialStem) => Lemma.Replace("*", partialStem);
    }
    public class TermTypeForm
    {
        public string Form { get; set; }
        public int Priority { get; set; }

        public string TryMatchForm(string term)
        {
            var regex = new Regex(Form.Replace("*", "(.+)"));
            if (!regex.IsMatch(term)) return null;
            var partialStem = regex.Match(term).Groups[1].Value;
            return partialStem;
        }
    }

}

