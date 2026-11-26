using System;
using System.IO;
using System.Linq;
using RackCad.Application.VentanaRackCad;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T4 integration: the window names and places its files with the rules of Windows on any system (the CI runs Core on Linux, where
    /// <see cref="Path"/> has other rules). The rules are fixed here, and on Windows they are those of <see cref="Path"/>.
    /// </summary>
    public sealed class WindowsFileNamesTests
    {
        private static readonly string[] Paths =
        {
            @"C:\Planos\Nave A.dwg", @"C:\Nave.dwg", @"D:\a\b\c.rackcad.json", @"\\servidor\planos\Nave.dwg", @"\Nave.dwg",
            "C:/Planos/Nave.dwg", "Nave.dwg", ".dwg", "Nave", @"C:\Planos\Nave.v2.dwg",
        };

        [Fact]
        public void TheRulesAreThoseOfWindowsOnAnySystem()
        {
            Assert.Equal("Nave 3_ pasillo_B", WindowsFileNames.Clean("Nave 3: pasillo/B"));
            Assert.Equal("a_b_c_d_e_f_g_h", WindowsFileNames.Clean("a*b?c<d>e|f\"g\\h"));
            Assert.Equal("tab_", WindowsFileNames.Clean("tab\t"));
            Assert.True(WindowsFileNames.IsRooted(@"C:\L\a.rackcad.json"));
            Assert.True(WindowsFileNames.IsRooted(@"\\servidor\a"));
            Assert.False(WindowsFileNames.IsRooted("a.rackcad.json"));
            Assert.False(WindowsFileNames.IsRooted(" "));
            Assert.Equal(@"C:\Planos", WindowsFileNames.DirectoryOf(@"C:\Planos\Nave A.dwg"));
            Assert.Equal(@"C:\", WindowsFileNames.DirectoryOf(@"C:\Nave.dwg"));
            Assert.Null(WindowsFileNames.DirectoryOf("Nave.dwg"));
            Assert.Equal("Nave A", WindowsFileNames.FileNameWithoutExtensionOf(@"C:\Planos\Nave A.dwg"));
            Assert.Equal("Nave.v2", WindowsFileNames.FileNameWithoutExtensionOf(@"C:\Planos\Nave.v2.dwg"));
            Assert.Equal(@"C:\L\x.json", WindowsFileNames.Combine(@"C:\L", "x.json"));
            Assert.Equal(@"C:\L\x.json", WindowsFileNames.Combine(@"C:\L\", "x.json"));
            Assert.Equal("x.json", WindowsFileNames.Combine(null, "x.json"));
        }

        [Fact]
        public void OnWindowsTheyGiveWhatPathGives()
        {
            if (!OperatingSystem.IsWindows()) return; // the equivalence is with Path on Windows; elsewhere the rules above hold

            Assert.Equal(new string(Path.GetInvalidFileNameChars().OrderBy(c => c).ToArray()),
                new string(WindowsFileNames.InvalidChars.OrderBy(c => c).ToArray()));
            foreach (var path in Paths)
            {
                Assert.Equal(Path.IsPathRooted(path), WindowsFileNames.IsRooted(path));
                var folder = Path.GetDirectoryName(path);
                Assert.Equal(string.IsNullOrEmpty(folder) ? null : folder, WindowsFileNames.DirectoryOf(path)?.Replace('/', '\\'));
                Assert.Equal(Path.GetFileNameWithoutExtension(path), WindowsFileNames.FileNameWithoutExtensionOf(path));
            }

            Assert.Equal(Path.Combine(@"C:\L", "x.json"), WindowsFileNames.Combine(@"C:\L", "x.json"));
        }
    }
}
