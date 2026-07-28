using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The distributed catalog parsed but is not valid: a manifest that disagrees with the files, a hash that
    /// does not match, metadata that describes a different source, or an overlay pointing at a section that no
    /// longer exists.
    ///
    /// It is deliberately distinct from <see cref="StructuralSectionCsvException"/>, which means "this FILE is
    /// malformed at line N". This one means "these files are individually well formed and collectively a lie",
    /// which is what a half-published import or a swapped catalog folder looks like.
    /// </summary>
    public sealed class StructuralSectionCatalogException : Exception
    {
        public StructuralSectionCatalogException(string directory, string message, Exception inner = null)
            : base((directory == null ? string.Empty : directory + ": ") + message, inner)
        {
            Directory = directory ?? string.Empty;
        }

        /// <summary>The catalog folder the failure refers to.</summary>
        public string Directory { get; }
    }
}
