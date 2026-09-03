using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The remembered "Fondos destino" choice of the selective editor (I-43, gate 8 correction): the user's INTENT,
    /// carried between openings so the editor comes back the way they left it.
    /// <para>
    /// It is an EDITOR preference, not part of the rack: it lives with the other per-user settings
    /// (<c>%APPDATA%\RackCad\settings.json</c>) and never reaches <c>SelectivePalletDesign</c>, the .dwg or the
    /// save/load of a design. A preference that travelled inside the document would change a drawing depending on who
    /// opened it, and would make two racks disagree about a value that describes the editor rather than the product.
    /// </para>
    /// <para>
    /// The intent is stored, never the resolved set: "Todos" and "Actual" are re-resolved against the rack being
    /// opened, so they land on ITS fondos and ITS visible fondo. An explicit set keeps only the indices that rack
    /// really has, and one that survives nothing at all falls back to "Todos" — the default — instead of aiming the
    /// editor at a fondo the user never chose.
    /// </para>
    /// </summary>
    public sealed class SelectiveTargetPreference
    {
        private const string AllToken = "Todos";
        private const string CurrentToken = "Actual";

        private SelectiveTargetPreference(SelectiveTargetMode mode, IReadOnlyList<int> fondos)
        {
            Mode = mode;
            Fondos = fondos ?? Array.Empty<int>();
        }

        /// <summary>Which of the three intents this is.</summary>
        public SelectiveTargetMode Mode { get; }

        /// <summary>The chosen fondo indices (0-based, ascending, distinct); empty unless <see cref="Mode"/> is
        /// <see cref="SelectiveTargetMode.Explicit"/>.</summary>
        public IReadOnlyList<int> Fondos { get; }

        /// <summary>"Todos" — also the DEFAULT for an installation that has never expressed a preference.</summary>
        public static SelectiveTargetPreference All { get; } =
            new SelectiveTargetPreference(SelectiveTargetMode.All, Array.Empty<int>());

        /// <summary>"Actual": whichever fondo the editor opens on.</summary>
        public static SelectiveTargetPreference Current { get; } =
            new SelectiveTargetPreference(SelectiveTargetMode.FollowCurrent, Array.Empty<int>());

        /// <summary>A deliberate set of fondos. An empty request is "Todos", the default, because no destination at
        /// all is not a state the editor can hold.</summary>
        public static SelectiveTargetPreference Of(IEnumerable<int> fondos)
        {
            var valid = (fondos ?? Enumerable.Empty<int>()).Where(index => index >= 0).Distinct().OrderBy(index => index).ToList();
            return valid.Count > 0 ? new SelectiveTargetPreference(SelectiveTargetMode.Explicit, valid) : All;
        }

        /// <summary>Read the intent currently in force in <paramref name="state"/>, ready to be stored.</summary>
        public static SelectiveTargetPreference Capture(SelectiveEditorState state)
        {
            if (state == null) return All;
            switch (state.TargetMode)
            {
                case SelectiveTargetMode.All: return All;
                case SelectiveTargetMode.FollowCurrent: return Current;
                default: return Of(state.TargetFondos.Fondos);
            }
        }

        /// <summary>
        /// Aim <paramref name="state"/> at this preference, resolved against the rack it actually has: "Todos" and
        /// "Actual" stay living modes, and an explicit set is filtered down to the fondos that exist. When nothing of
        /// an explicit set survives, "Todos" takes over rather than silently retargeting to some other fondo.
        /// </summary>
        public void ApplyTo(SelectiveEditorState state)
        {
            if (state == null) return;

            if (Mode == SelectiveTargetMode.FollowCurrent)
            {
                state.FollowCurrentFondo();
                return;
            }

            if (Mode == SelectiveTargetMode.Explicit)
            {
                var count = state.FondoCount;
                var surviving = Fondos.Where(index => index < count).ToList();
                if (surviving.Count > 0)
                {
                    state.SetTargetFondos(surviving);
                    return;
                }
            }

            state.FollowAllFondos();
        }

        /// <summary>The stored form: <c>"Todos"</c>, <c>"Actual"</c>, or the 0-based indices as <c>"0,2"</c>.</summary>
        public string Encode()
        {
            if (Mode == SelectiveTargetMode.All) return AllToken;
            if (Mode == SelectiveTargetMode.FollowCurrent) return CurrentToken;
            return string.Join(",", Fondos.Select(index => index.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Parse a stored value. ANY unusable input — absent, blank, unknown word, non-numeric, negative, or a list
        /// that parses to nothing — is "Todos", the default: a preference file is best-effort and must never leave the
        /// editor without a destination.
        /// </summary>
        public static SelectiveTargetPreference Decode(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return All;

            var text = stored.Trim();
            if (text.Equals(AllToken, StringComparison.OrdinalIgnoreCase)) return All;
            if (text.Equals(CurrentToken, StringComparison.OrdinalIgnoreCase)) return Current;

            var fondos = new List<int>();
            foreach (var token in text.Split(new[] { ',', ';', ' ', '+' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) || index < 0)
                {
                    return All; // a corrupt entry is not silently reinterpreted as a smaller set
                }

                fondos.Add(index);
            }

            return Of(fondos);
        }
    }
}
