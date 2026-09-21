using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    internal static class I55ProductCharacterizationTestSupport
    {
        internal static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir;
        }

        internal static string Read(params string[] relative)
        {
            var path = Path.Combine(RepoRoot().FullName, Path.Combine(relative));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        internal static string Code(params string[] relative)
        {
            var source = Read(relative);
            source = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(source, @"//[^\n]*", string.Empty);
        }

        internal static string Body(string source, string marker)
        {
            var at = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(at >= 0, "No se encontro: " + marker);
            var open = source.IndexOf('{', at);
            Assert.True(open >= 0, "No se encontro el cuerpo de: " + marker);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(open, i - open + 1);
            }
            throw new InvalidOperationException("Cuerpo sin cerrar: " + marker);
        }

        internal static int Count(string source, string needle)
        {
            var count = 0;
            for (var at = source.IndexOf(needle, StringComparison.Ordinal); at >= 0;
                 at = source.IndexOf(needle, at + needle.Length, StringComparison.Ordinal)) count++;
            return count;
        }
    }
}
