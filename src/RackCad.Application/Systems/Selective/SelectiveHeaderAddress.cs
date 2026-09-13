using System;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The address of ONE cabecera of the Selectivo: post <see cref="PostIndex"/> of fondo <see cref="FondoIndex"/>, the
    /// <c>(FondoIndex, PostIndex)</c> authority of I-43 (I-53 contract §3.1, §6).
    /// <para>
    /// It is what the editor remembers as the SOURCE of a distribution — an address, never a configuration. The
    /// configuration is read from the address when the batch is prepared, so an edit of that cabecera made after it was
    /// chosen is the one that travels (contract §3.3). An address is enough here, without a signature: fondos grow and
    /// shrink at the end and cabecera rows are pruned without resurrection, so a remembered address designates the same
    /// position or stops existing (§6.2).
    /// </para>
    /// <para>
    /// An address says nothing about existence: whether the fondo and the post exist is a question for the current
    /// topology, and a negative post is not an address of the taxonomy at all. Ordering is part of the contract: fondo,
    /// then post, both ascending (§3.12).
    /// </para>
    /// </summary>
    public readonly struct SelectiveHeaderAddress : IEquatable<SelectiveHeaderAddress>, IComparable<SelectiveHeaderAddress>
    {
        public SelectiveHeaderAddress(int fondoIndex, int postIndex)
        {
            FondoIndex = fondoIndex;
            PostIndex = postIndex;
        }

        /// <summary>The fondo (depth line) of the cabecera; 0 is the master grid.</summary>
        public int FondoIndex { get; }

        /// <summary>The main post inside that fondo: a fondo with C frentes has posts 0..C.</summary>
        public int PostIndex { get; }

        public bool Equals(SelectiveHeaderAddress other)
            => FondoIndex == other.FondoIndex && PostIndex == other.PostIndex;

        public override bool Equals(object obj) => obj is SelectiveHeaderAddress other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (FondoIndex * 397) ^ PostIndex;
            }
        }

        /// <summary>Canonical order: fondo, then post, both ascending.</summary>
        public int CompareTo(SelectiveHeaderAddress other)
            => FondoIndex != other.FondoIndex ? FondoIndex.CompareTo(other.FondoIndex) : PostIndex.CompareTo(other.PostIndex);

        public static bool operator ==(SelectiveHeaderAddress left, SelectiveHeaderAddress right) => left.Equals(right);

        public static bool operator !=(SelectiveHeaderAddress left, SelectiveHeaderAddress right) => !left.Equals(right);

        /// <summary>Diagnostic form, e.g. <c>fondo 1 / poste 3</c>.</summary>
        public override string ToString() => "fondo " + FondoIndex + " / poste " + PostIndex;
    }
}
