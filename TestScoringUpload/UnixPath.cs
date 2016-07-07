using System;

namespace JMU.TestScoring
{
    public static class UnixPath
    {
        public static string Combine(params string[] paths)
        {
            return String.Join("/", paths);
        }

        public static string Combine(string path1, string path2)
        {
            return String.Join("/", path1, path2);
        }

        public static string Combine(string path1, string path2, string path3)
        {
            return String.Join("/", path1, path2, path3);
        }

        public static string GetFileName(string path)
        {
            return path.Substring(path.LastIndexOf("/") + 1);
        }
    }
}
