﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
