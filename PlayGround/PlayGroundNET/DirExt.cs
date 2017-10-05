using System.IO;

namespace PlayGroundNET
{
    public static class DirExt
    {
        public static string CreateDirIfNotExists(this string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string EnsureDirEmpty(this string path)
        {
            foreach (var file in Directory.EnumerateFiles(path.CreateDirIfNotExists()))
            {
                File.Delete(file);
            }
            return path;
        }
    }
}