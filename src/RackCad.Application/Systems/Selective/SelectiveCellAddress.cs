using System;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The address of ONE selective cell, on the THREE axes the Selectivo actually has (I-43):
    /// <c>fondo = FondoIndex</c>, <c>frente = (FondoIndex, FrontIndex)</c>,
    /// <c>celda = (FondoIndex, FrontIndex, LevelIndex)</c>.
    /// <para>
    /// It is deliberately NOT <c>DynamicRackCellAddress</c>, for the same reason <c>RackModuleHeaderScope</c> is not a
    /// matrix scope: the axes differ. A Push Back cell lives on ONE frente x nivel grid, so two indices name it; a
    /// Selectivo cell lives on a grid PER FONDO, and each fondo may carry its own frente count and its own level count
    /// per frente (doble profundidad, layout en esquina). Dropping the fondo axis to reuse the two-index type would
    /// make two different cells share an address, which is exactly the cross-talk this initiative must prevent.
    /// </para>
    /// <para>
    /// Ordering is part of the contract, not a convenience: fondo, then frente, then nivel, all ascending. That is the
    /// canonical order <see cref="SelectiveTargetResolver"/> emits, so the same inputs always produce the same list.
    /// </para>
    /// </summary>
    public readonly struct SelectiveCellAddress : IEquatable<SelectiveCellAddress>, IComparable<SelectiveCellAddress>
    {
        public SelectiveCellAddress(int fondoIndex, int frontIndex, int levelIndex)
        {
            FondoIndex = fondoIndex;
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
        }

        /// <summary>The fondo (depth line) this cell belongs to; 0 is the master grid.</summary>
        public int FondoIndex { get; }

        /// <summary>The frente (bay) index INSIDE <see cref="FondoIndex"/>. The same number in another fondo is another frente.</summary>
        public int FrontIndex { get; }

        /// <summary>The level index inside that frente; 0 is the lowest editable level of the column.</summary>
        public int LevelIndex { get; }

        public bool Equals(SelectiveCellAddress other)
            => FondoIndex == other.FondoIndex && FrontIndex == other.FrontIndex && LevelIndex == other.LevelIndex;

        public override bool Equals(object obj) => obj is SelectiveCellAddress other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = FondoIndex;
                hash = (hash * 397) ^ FrontIndex;
                return (hash * 397) ^ LevelIndex;
            }
        }

        /// <summary>Canonical order: fondo, then frente, then nivel — every one ascending.</summary>
        public int CompareTo(SelectiveCellAddress other)
        {
            if (FondoIndex != other.FondoIndex) return FondoIndex.CompareTo(other.FondoIndex);
            if (FrontIndex != other.FrontIndex) return FrontIndex.CompareTo(other.FrontIndex);
            return LevelIndex.CompareTo(other.LevelIndex);
        }

        public static bool operator ==(SelectiveCellAddress left, SelectiveCellAddress right) => left.Equals(right);

        public static bool operator !=(SelectiveCellAddress left, SelectiveCellAddress right) => !left.Equals(right);

        /// <summary>Diagnostic form, e.g. <c>fondo 1 / frente 2 / nivel 0</c>.</summary>
        public override string ToString()
            => "fondo " + FondoIndex + " / frente " + FrontIndex + " / nivel " + LevelIndex;
    }
}
