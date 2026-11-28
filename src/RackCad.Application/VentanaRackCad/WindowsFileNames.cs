#nullable enable
using System;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// The rules of Windows for the names and the paths of the files the window writes (a design of the library, a BOM next to the
    /// drawing), whatever the system that runs the code. RackCad runs on Windows, where <see cref="System.IO.Path"/> gives exactly these
    /// rules; the Core tests also run on Linux (CI), where <see cref="System.IO.Path.GetInvalidFileNameChars"/> admits «:», «*» or «\»
    /// and a path like «C:\Planos\Nave.dwg» is not rooted. So the rules are written once here, as Windows has them (T4 integration: the
    /// CI of the fourth batch failed on Linux with <see cref="System.IO.Path"/>).
    /// </summary>
    public static class WindowsFileNames
    {
        /// <summary>The characters Windows does not admit in a file name: «"», «&lt;», «&gt;», «|», «:», «*», «?», «\», «/» and the control ones.</summary>
        public static readonly char[] InvalidChars = BuildInvalidChars();

        private static char[] BuildInvalidChars()
        {
            var chars = new char[32 + 9];
            for (var i = 0; i < 32; i++) chars[i] = (char)i;
            "\"<>|:*?\\/".CopyTo(0, chars, 32, 9);
            return chars;
        }

        /// <summary>True for a character Windows does not admit in a file name.</summary>
        public static bool IsInvalid(char c) => Array.IndexOf(InvalidChars, c) >= 0;

        /// <summary>The name with every character Windows does not admit as «_» (the name is not trimmed here).</summary>
        public static string Clean(string name)
        {
            if (string.IsNullOrEmpty(name)) return name ?? string.Empty;
            var chars = name.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (IsInvalid(chars[i])) chars[i] = '_';
            return new string(chars);
        }

        /// <summary>Rooted as Windows says it (<c>Path.IsPathRooted</c> on Windows): «C:…», «\…» or «/…».</summary>
        public static bool IsRooted(string? path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (path![0] == '\\' || path[0] == '/') return true;
            return path.Length >= 2 && path[1] == ':' && char.IsLetter(path[0]);
        }

        /// <summary>
        /// The folder of a file, as <c>Path.GetDirectoryName</c> gives it on Windows for the paths the window uses: «C:\a\b.dwg» → «C:\a»,
        /// «C:\b.dwg» → «C:\», «\b.dwg» → «\»; null when the path has no folder.
        /// </summary>
        public static string? DirectoryOf(string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var i = LastSeparator(path!);
            if (i < 0) return null;
            if (i == 0) return path!.Substring(0, 1);
            if (i == 2 && path![1] == ':') return path.Substring(0, 3);
            return path!.Substring(0, i);
        }

        /// <summary>The name of the file without its extension, as <c>Path.GetFileNameWithoutExtension</c> gives it on Windows.</summary>
        public static string FileNameWithoutExtensionOf(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            var name = path!.Substring(LastSeparator(path) + 1);
            if (name.Length >= 2 && name[1] == ':' && char.IsLetter(name[0])) name = name.Substring(2); // «C:Nave.dwg»
            var dot = name.LastIndexOf('.');
            return dot < 0 ? name : name.Substring(0, dot);
        }

        /// <summary>The folder and the file joined with the separator of Windows.</summary>
        public static string Combine(string? folder, string file)
        {
            if (string.IsNullOrEmpty(folder)) return file;
            var last = folder![folder.Length - 1];
            return last == '\\' || last == '/' ? folder + file : folder + "\\" + file;
        }

        private static int LastSeparator(string path) => Math.Max(path.LastIndexOf('\\'), path.LastIndexOf('/'));
    }
}
