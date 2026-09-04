using System;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// A position in the VISIBLE matrix of the selective editor: <c>(FrontIndex, LevelIndex)</c>, and deliberately
    /// nothing else (I-43).
    /// <para>
    /// This is the type the future multi-selection speaks. What the user picks on screen is a set of positions of the
    /// grid they are looking at, not a set of cells of a particular fondo: the fondo is the OTHER, independent axis,
    /// chosen separately as <see cref="SelectiveFondoTargets"/>. Resolving <see cref="SelectiveApplyScope.Selected"/>
    /// is therefore a PROJECTION — each position lands on every target fondo — exactly as it already works in
    /// Dinamico/Push Back, where the selection is likewise a set of frente x nivel positions.
    /// </para>
    /// <para>
    /// Being fondo-less is enforced by the type, not by a convention: a selection cannot carry a
    /// <c>FondoIndex</c> to be honoured or ignored, so no caller can accumulate a per-fondo selection by accident
    /// and no resolver has to decide what a selection's own fondo would mean. A <see cref="SelectiveCellAddress"/>
    /// only appears once a position has been projected onto a fondo.
    /// </para>
    /// </summary>
    public readonly struct SelectiveMatrixPosition : IEquatable<SelectiveMatrixPosition>, IComparable<SelectiveMatrixPosition>
    {
        public SelectiveMatrixPosition(int frontIndex, int levelIndex)
        {
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
        }

        /// <summary>The frente (bay) column of the visible matrix.</summary>
        public int FrontIndex { get; }

        /// <summary>The level row of that column; 0 is the lowest editable level.</summary>
        public int LevelIndex { get; }

        /// <summary>This position seen as a cell of <paramref name="fondoIndex"/> — the projection itself. Whether
        /// that cell EXISTS is a question for the topology, not for this method.</summary>
        public SelectiveCellAddress InFondo(int fondoIndex) => new SelectiveCellAddress(fondoIndex, FrontIndex, LevelIndex);

        public bool Equals(SelectiveMatrixPosition other)
            => FrontIndex == other.FrontIndex && LevelIndex == other.LevelIndex;

        public override bool Equals(object obj) => obj is SelectiveMatrixPosition other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (FrontIndex * 397) ^ LevelIndex;
            }
        }

        /// <summary>Canonical order: frente, then nivel, both ascending — the inner half of the target order.</summary>
        public int CompareTo(SelectiveMatrixPosition other)
            => FrontIndex != other.FrontIndex ? FrontIndex.CompareTo(other.FrontIndex) : LevelIndex.CompareTo(other.LevelIndex);

        public static bool operator ==(SelectiveMatrixPosition left, SelectiveMatrixPosition right) => left.Equals(right);

        public static bool operator !=(SelectiveMatrixPosition left, SelectiveMatrixPosition right) => !left.Equals(right);

        /// <summary>Diagnostic form, e.g. <c>frente 2 / nivel 0</c>.</summary>
        public override string ToString() => "frente " + FrontIndex + " / nivel " + LevelIndex;
    }
}
