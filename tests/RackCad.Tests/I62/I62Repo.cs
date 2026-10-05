#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RackCad.Tests
{
    /// <summary>Repository access and Git object identities shared by the I-62 F4 implementation and its guards.</summary>
    public static class I62Repo
    {
        public static string Root()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new InvalidOperationException("RackCad.sln not found above " + AppContext.BaseDirectory);
        }

        public static string FullPath(string relative) => Path.Combine(Root(), relative.Replace('/', Path.DirectorySeparatorChar));

        public static string ReadText(string relative) => File.ReadAllText(FullPath(relative));

        /// <summary>Git blob id of bytes exactly as given.</summary>
        public static string GitBlobSha1(byte[] content)
        {
            var header = Encoding.ASCII.GetBytes("blob " + content.Length + "\0");
            return Convert.ToHexString(SHA1.HashData(header.Concat(content).ToArray())).ToLowerInvariant();
        }

        /// <summary>Git blob id of text as stored in the repository (LF line ends, UTF-8 without BOM).</summary>
        public static string GitBlobSha1(string text) => GitBlobSha1(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal)));
    }
}
