using System;
using System.IO;
using System.Text.RegularExpressions;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-46 (ID12) — the four TOPE options belong to the SELECTIVO and must not leak into the Dinámico.
    /// <para>
    /// The three rich editors share ONE safety window, so "the Dinámico is unaffected" cannot be argued from the
    /// dialog alone: it is true only because <c>RackDynamicSystemWindow.Safety_Click</c> hands that window a
    /// WHITELIST of families — bota, lateral, desviador, defensa and guía — that does not include the tope. Without a
    /// tope row there is no "Configurar…" button, so <c>SafetyTopeGridWindow</c> is never constructed there and its
    /// side selector cannot reach the Dinámico at all.
    /// </para>
    /// <para>
    /// That whitelist is a private local inside an event handler, so no behavioural seam reaches it; this guard reads
    /// the UI source as TEXT, the same way the Plugin guards do. It exists because widening that filter would silently
    /// give the Dinámico a family it has never had — and the failure would show up as a drawing change, not as a
    /// compile error. It is a guard about the FILTER, not about how the tope draws.
    /// </para>
    /// </summary>
    public class SelectiveTopeFamilyIsolationGuardTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "Could not locate the repo root (RackCad.sln) from the test output directory.");
            return dir;
        }

        private static string ReadUiSource(params string[] relative)
        {
            var path = Path.Combine(RepoRoot().FullName, Path.Combine("src", "RackCad.UI"), Path.Combine(relative));
            Assert.True(File.Exists(path), $"UI source not found: {path}");
            return File.ReadAllText(path);
        }

        private static string DynamicWindow => ReadUiSource("Systems", "Dynamic", "RackDynamicSystemWindow.xaml.cs");

        /// <summary>The body of the Dinámico's <c>Safety_Click</c> element filter, up to its <c>.ToList()</c>.</summary>
        private static string DynamicSafetyFilter()
        {
            var source = DynamicWindow;
            var start = source.IndexOf("private void Safety_Click", StringComparison.Ordinal);
            Assert.True(start >= 0, "Safety_Click not found in RackDynamicSystemWindow.");
            var end = source.IndexOf(".ToList();", start, StringComparison.Ordinal);
            Assert.True(end > start, "The element filter of Safety_Click no longer ends in .ToList().");
            return source.Substring(start, end - start);
        }

        // ---- The Dinámico's whitelist offers five families, and the tope is not one of them ----
        [Fact]
        public void DynamicSafetyWindow_NeverReceivesTheTopeFamily()
        {
            var filter = DynamicSafetyFilter();

            Assert.DoesNotContain(nameof(SelectiveSafetyDefaults.TopeType), filter);
            Assert.Contains(nameof(SelectiveSafetyDefaults.BotaType), filter);      // the sibling families ARE there,
            Assert.Contains(nameof(SelectiveSafetyDefaults.LateralType), filter);   // so the absence above is meaningful
            Assert.Contains(nameof(SelectiveSafetyDefaults.DesviadorType), filter);
            Assert.Contains(nameof(SelectiveSafetyDefaults.DefensaType), filter);
            Assert.Contains(nameof(SelectiveSafetyDefaults.GuiaType), filter);
        }

        // ---- ...and it never opens the tope dialog by any other route ----
        [Fact]
        public void DynamicWindow_NeverConstructsTheTopeDialog()
            => Assert.DoesNotContain("SafetyTopeGridWindow", DynamicWindow);

        // ---- The Dinámico keeps its OWN boot vocabulary; I-46 touches the tope, not that decision ----
        [Fact]
        public void DynamicWindow_KeepsItsPlacementNamedBoot()
            => Assert.Matches(new Regex(@"bootUsesPlacementNames:\s*true"), DynamicWindow);
    }
}
