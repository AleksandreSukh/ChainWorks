using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedResourceReader
{
    public class EmbeddedResourceReader
    {
        public static byte[] ReadResource(string res, Assembly executingAssembly)
        {
            var names = executingAssembly.GetManifestResourceNames();
            var resourceName = names.Single(n => n.EndsWith(res, StringComparison.OrdinalIgnoreCase));

            using (Stream stream = executingAssembly.GetManifestResourceStream(resourceName))
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
