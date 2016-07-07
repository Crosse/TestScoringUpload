using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Security;
using System.Runtime.InteropServices;

namespace Crosse.ExtensionMethods
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a <see cref="System.Security.SecureString"/> to an unencrypted string.
        /// </summary>
        /// <param name="secureString">The <see cref="System.Security.SecureString"/> to unencrypt.</param>
        /// <returns></returns>
        public static string ConvertToUnsecureString(this SecureString secureString)
        {
            if (secureString == null)
                return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static SecureString ConvertToSecureString(this string plaintextString)
        {
            if (plaintextString == null)
                return new SecureString();

            var securePassword = new SecureString();

            foreach (char c in plaintextString.ToCharArray())
            {
                securePassword.AppendChar(c);
            }
            securePassword.MakeReadOnly();
            return securePassword;
        }

        public static string ToIsoDateTimeString(this DateTime dateTime, bool useSeparators = true, bool withTimeZone = false)
        {
            if (dateTime == null)
                return null;

            string format;
            
            if (useSeparators)
                format = "yyyy-MM-dd HH:mm:ss";
            else
                format = "yyyyMMddHHmmss";

            if (withTimeZone)
                format += "K";

            return dateTime.ToString(format);
        }

        static string RegexReplaceCharacters(char[] chars, string input, string replacement)
        {
            string invalidCharacters = "[" + String.Join("", chars) + "]";
            Regex regex = new Regex(invalidCharacters);
            return regex.Replace(input, replacement);
        }

        public static string SanitizeFileName(this string fileName, string replacement)
        {
            return RegexReplaceCharacters(Path.GetInvalidFileNameChars(), fileName, replacement);
        }

        public static string SanitizePath(this string path, string replacement)
        {
            return RegexReplaceCharacters(Path.GetInvalidPathChars(), path, replacement);
        }
    }
}
