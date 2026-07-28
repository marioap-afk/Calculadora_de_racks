using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// The one entry point that turns a catalogued section into geometry, with a lazy per-section cache.
    ///
    /// Building all 983 shapes at both detail levels on startup would be pure waste: a session touches a
    /// handful. So nothing is built until it is asked for, and what is built is kept.
    ///
    /// The cache is keyed by section id AND detail level, and scoped to a CATALOG INSTANCE. That last part is
    /// what makes invalidation a non-problem: <c>Load()</c> returns a new catalog whenever the files change,
    /// and a new catalog gets a new factory, so a stale geometry cannot outlive the data it came from. A
    /// static cache keyed only by id would have needed an invalidation protocol, and those are exactly the
    /// things that get forgotten.
    ///
    /// Pure: no WPF, no AutoCAD, no ambient state. Safe for concurrent reads; a race can compute the same
    /// geometry twice and both results are equal, which is cheaper than locking.
    /// </summary>
    public sealed class StructuralSectionGeometryFactory
    {
        private readonly StructuralSectionCatalog _catalog;
        private readonly ConcurrentDictionary<CacheKey, StructuralSectionGeometry> _cache =
            new ConcurrentDictionary<CacheKey, StructuralSectionGeometry>();

        public StructuralSectionGeometryFactory(StructuralSectionCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public StructuralSectionCatalog Catalog => _catalog;

        /// <summary>How many geometries the cache is holding. Diagnostics and tests only.</summary>
        public int CachedCount => _cache.Count;

        /// <summary>
        /// The geometry of a section by id, at the requested detail. Resolves DISABLED sections too: a design
        /// that already references one must keep drawing (owner decision 15 of I-36A).
        /// </summary>
        public StructuralSectionGeometry Get(StructuralSectionId sectionId, SectionDetailLevel detail)
        {
            if (!_catalog.TryGetById(sectionId, out var section))
            {
                throw new StructuralSectionGeometryNotFoundException(sectionId);
            }

            return Get(section, detail);
        }

        public StructuralSectionGeometry Get(StructuralSectionDefinition section, SectionDetailLevel detail)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            return _cache.GetOrAdd(new CacheKey(section.SectionId, detail), _ => Build(section, detail));
        }

        /// <summary>
        /// Builds without touching the cache. The dispatch is an explicit switch over the family, not
        /// reflection or a registry: the families are the whole universe, and a new one should not be
        /// possible to add without the compiler noticing — which is exactly how I-36D added S.
        /// </summary>
        public static StructuralSectionGeometry Build(
            StructuralSectionDefinition section, SectionDetailLevel detail)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (section.Dimensions == null || section.Dimensions.Family != section.Family)
            {
                throw new InvalidOperationException(
                    "La seccion '" + section.SectionId + "' declara la familia " + section.Family +
                    " y sus dimensiones no corresponden.");
            }

            switch (section.Family)
            {
                case StructuralSectionFamily.W:
                    return WSectionGeometryBuilder.Build(section, detail);
                case StructuralSectionFamily.HssRectangular:
                    return HssRectangularSectionGeometryBuilder.Build(section, detail);
                case StructuralSectionFamily.Channel:
                    return ChannelSectionGeometryBuilder.Build(section, detail);
                case StructuralSectionFamily.Angle:
                    return AngleSectionGeometryBuilder.Build(section, detail);
                case StructuralSectionFamily.S:
                    return SSectionGeometryBuilder.Build(section, detail);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(section), section.Family, "Familia sin constructor de geometria.");
            }
        }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly StructuralSectionId _sectionId;
            private readonly SectionDetailLevel _detail;

            public CacheKey(StructuralSectionId sectionId, SectionDetailLevel detail)
            {
                _sectionId = sectionId;
                _detail = detail;
            }

            public bool Equals(CacheKey other) => _sectionId.Equals(other._sectionId) && _detail == other._detail;

            public override bool Equals(object obj) => obj is CacheKey other && Equals(other);

            public override int GetHashCode() => (_sectionId, _detail).GetHashCode();
        }
    }

    /// <summary>Raised when a geometry is asked for a section the catalog does not contain.</summary>
    public sealed class StructuralSectionGeometryNotFoundException : KeyNotFoundException
    {
        public StructuralSectionGeometryNotFoundException(StructuralSectionId sectionId)
            : base("El catalogo no contiene la seccion '" + sectionId + "', asi que no hay geometria que generar.")
        {
            SectionId = sectionId;
        }

        public StructuralSectionId SectionId { get; }
    }
}
