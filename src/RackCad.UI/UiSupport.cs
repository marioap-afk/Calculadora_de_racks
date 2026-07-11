using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;

namespace RackCad.UI
{
    /// <summary>
    /// Small helpers shared by every window/view-model so they are not re-implemented per file:
    /// resilient catalog loading, invariant numeric parsing of user input, building the
    /// DisplayName/Id options the combos bind to, and the shared status-bar styling.
    /// </summary>
    internal static class UiSupport
    {
        // Shared status palette across the rack windows: red #B00020 error / green #2F855A ok. Frozen once
        // (SetStatus used to allocate a fresh SolidColorBrush per call in every window).
        private static readonly Brush StatusErrorBrush = FrozenBrush(Color.FromRgb(0xB0, 0x00, 0x20));
        private static readonly Brush StatusOkBrush = FrozenBrush(Color.FromRgb(0x2F, 0x85, 0x5A));

        /// <summary>The ONE status-bar styling (text + error/ok color) every window's SetStatus forwards to.</summary>
        public static void SetStatus(TextBlock target, string message, bool isError)
        {
            if (target == null) return;
            target.Text = message ?? string.Empty;
            target.Foreground = isError ? StatusErrorBrush : StatusOkBrush;
        }

        /// <summary>A frozen (shareable, allocation-once) solid brush.</summary>
        public static Brush FrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// The ONE "guardar en la biblioteca de diseños" prompt (was copy-pasted per window): resolves the library
        /// folder (creating it best-effort), sanitizes <paramref name="preferredName"/> into a file name (falling back
        /// to <paramref name="fallbackName"/> when blank) and shows the standard <c>.rackcad.json</c> dialog.
        /// Returns the chosen path, or null when the user cancels.
        /// </summary>
        public static string PromptSaveToLibrary(System.Windows.Window owner, string preferredName, string fallbackName)
        {
            var libraryFolder = RackCad.Application.Settings.UserSettingsStore.ResolveDesignLibraryPath(
                RackCad.Application.Settings.UserSettingsStore.Load());
            try { System.IO.Directory.CreateDirectory(libraryFolder); } catch { /* best-effort default folder */ }

            var baseName = string.IsNullOrWhiteSpace(preferredName)
                ? fallbackName
                : string.Join("_", preferredName.Trim().Split(System.IO.Path.GetInvalidFileNameChars()));

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Proyecto RackCad (*.rackcad.json)|*.rackcad.json|JSON (*.json)|*.json",
                FileName = baseName + RackCad.Application.Persistence.RackProjectStore.FileExtension,
                InitialDirectory = libraryFolder
            };

            return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
        }
        /// <summary>Loads the catalog, or an empty one if the files are missing/corrupt (UI keeps working).</summary>
        public static RackCatalog LoadCatalogSafe()
        {
            try
            {
                return JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch
            {
                return new RackCatalog();
            }
        }

        /// <summary>Parses a user-typed number: invariant first, then the user's culture (so "96.5" and "96,5" both work).</summary>
        public static bool TryNum(string text, out double value)
        {
            text = (text ?? string.Empty).Trim();
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value);
        }

        /// <summary>Parses an OPTIONAL positive number: empty/whitespace → null (auto); a valid &gt; 0 value → that value;
        /// anything else (non-numeric or &lt;= 0) → false, so the caller can report an error instead of silently defaulting.</summary>
        public static bool TryOptionalNum(string text, out double? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (TryNum(text, out var v) && v > 0.0)
            {
                value = v;
                return true;
            }

            return false;
        }

        /// <summary>Distinct, ordered DisplayName/Id options for a combo (skips blank ids).</summary>
        public static List<CatalogOption> ToOptions<T>(IEnumerable<T> entries) where T : CatalogEntryBase
        {
            return (entries ?? Enumerable.Empty<T>())
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Id))
                .GroupBy(entry => entry.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(entry => entry.Label, StringComparer.CurrentCultureIgnoreCase)
                .Select(entry => new CatalogOption(entry.Id.Trim(), entry.Label))
                .ToList();
        }
    }
}
