namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The geometric measurements of ONE family, composed into <see cref="StructuralSectionDefinition"/>.
    ///
    /// This interface exists for a family boundary, not for polymorphism: there is one implementation per
    /// family and consumers pattern-match on the concrete type, because a W has a flange and an HSS does not
    /// and no shared abstraction over both is honest. The alternative — a single type with dozens of optional
    /// fields where <c>Ht</c> and <c>bf</c> are mutual nulls and nothing forbids an impossible combination —
    /// is exactly what ADR-0020 rejects.
    ///
    /// All lengths are INCHES, the canonical internal unit of RackCad (ADR-0005, unchanged by ADR-0021).
    /// A value that the source does not tabulate for a given shape is <c>null</c>, NEVER zero: a zero would be
    /// indistinguishable from a real measurement lost to a parse failure.
    /// </summary>
    public interface IStructuralSectionDimensions
    {
        StructuralSectionFamily Family { get; }
    }
}
