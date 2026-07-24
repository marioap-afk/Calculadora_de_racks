using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// One informative card of the Push Back frente × nivel matrix (PB-VAL-01). Pure presentation data — no WPF types —
    /// derived exclusively from the editing authority (<see cref="PushBackEditorState"/>: the shared
    /// <c>DynamicFrontMatrix</c> structure/selection plus the parallel Push Back per-cell values). The window renders
    /// these cards; it never stores a second copy of the state.
    /// </summary>
    internal sealed class PushBackMatrixCard
    {
        public int FrontIndex { get; set; }
        public int LevelIndex { get; set; }

        /// <summary>False for a GHOST slot: a display row above this front's own level count (jagged matrix).</summary>
        public bool IsActive { get; set; }

        /// <summary>The primary (last-touched) cell — the one the cell editor is loaded with.</summary>
        public bool IsPrimary { get; set; }

        /// <summary>In the multi-selection (checked), primary or not.</summary>
        public bool IsIncluded { get; set; }

        public bool TopeActive { get; set; }

        /// <summary>The card body: positions, fondos (+ start), IN/OUT and rear peraltes, and the tope state.</summary>
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Builds the card view-models for every (front, display level) slot of the matrix, exactly one card per slot of a
    /// JAGGED grid padded to the tallest front (ghost slots are inactive). Reads the state; never mutates it.
    /// </summary>
    internal static class PushBackMatrixCardModel
    {
        /// <summary>The card text for one ACTIVE cell (InvariantCulture; three compact lines).</summary>
        public static string CardText(PushBackEditorState state, int frontIndex, int levelIndex)
        {
            var front = state.Structure.Fronts[frontIndex];
            var cell = front.Cells.Count > 0
                ? front.Cells[Math.Max(0, Math.Min(levelIndex, front.Cells.Count - 1))]
                : new DynamicEditorCell();
            var push = state.Cell(frontIndex, levelIndex);

            return string.Format(
                CultureInfo.InvariantCulture,
                "×{0} · {1}F ini {2}\nIN/OUT {3:0.##}\" · Post {4:0.##}\"\n{5}",
                front.PalletCount,
                front.PalletsDeep,
                front.DepthStartPosition,
                cell.InOutBeamDepth,
                push.HighEndBeamPeralte,
                push.RearTopeEnabled ? "Tope ✔" : "Sin tope");
        }

        /// <summary>Every card of the padded jagged grid, in (front, level) order. Levels run 0..MaxLoadLevels-1; a level
        /// at or above a front's own count yields an inactive ghost card that is not editable and never selected.</summary>
        public static IReadOnlyList<PushBackMatrixCard> Build(PushBackEditorState state)
        {
            var cards = new List<PushBackMatrixCard>();
            if (state == null || state.Structure.Count == 0)
            {
                return cards;
            }

            var maxLevels = state.Structure.MaxLoadLevels();
            for (var frontIndex = 0; frontIndex < state.Structure.Count; frontIndex++)
            {
                var front = state.Structure.Fronts[frontIndex];
                var ownLevels = Math.Max(1, front.LoadLevels);
                for (var levelIndex = 0; levelIndex < maxLevels; levelIndex++)
                {
                    var active = levelIndex < ownLevels;
                    cards.Add(new PushBackMatrixCard
                    {
                        FrontIndex = frontIndex,
                        LevelIndex = levelIndex,
                        IsActive = active,
                        IsPrimary = active
                                    && frontIndex == state.Structure.SelectedFrontIndex
                                    && levelIndex == state.Structure.SelectedLevelIndex,
                        IsIncluded = active && state.Structure.IsSelected(frontIndex, levelIndex),
                        TopeActive = active && state.Cell(frontIndex, levelIndex).RearTopeEnabled,
                        Text = active ? CardText(state, frontIndex, levelIndex) : "—"
                    });
                }
            }

            return cards;
        }
    }
}
