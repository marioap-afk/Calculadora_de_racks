using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.UI.StructuralSections
{
    /// <summary>
    /// Everything the section inspector KNOWS, with no WPF in sight.
    ///
    /// The window is a thin shell over this: filtering, searching, validating the length and rebuilding the
    /// plan all happen here, so they can be tested without a dispatcher and so the same state could drive a
    /// different surface later. It is the same split I-20 and I-21 applied to the selective and dynamic
    /// editors, for the same reason.
    ///
    /// What it deliberately does NOT have, and will not grow: role, material, loads, levels, arms, connectors,
    /// fabrication, library persistence or round-trip editing. This is an inspector, not a member configurator
    /// (owner decisions 22 and 23); I-37 builds that.
    /// </summary>
    public sealed class StructuralSectionInspectorState
    {
        /// <summary>What a user types when they just want "a couple of feet".</summary>
        public const double DefaultLengthInches = 120.0;

        private readonly StructuralSectionGeometryFactory _factory;
        private StructuralSectionDefinition _selected;
        private string _search = string.Empty;
        private StructuralSectionFamily? _family;
        private double _length = DefaultLengthInches;

        public StructuralSectionInspectorState(StructuralSectionCatalog catalog)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _factory = new StructuralSectionGeometryFactory(catalog);
            _selected = Matches().FirstOrDefault();
        }

        public StructuralSectionCatalog Catalog { get; }

        // ---- Filters -------------------------------------------------------------------------------------

        /// <summary>Free text matched against BOTH published designations. Empty means no filter.</summary>
        public string Search
        {
            get => _search;
            set => _search = value ?? string.Empty;
        }

        /// <summary>Family filter; null means all four.</summary>
        public StructuralSectionFamily? Family
        {
            get => _family;
            set => _family = value;
        }

        /// <summary>
        /// The sections offered for a NEW selection: enabled only.
        ///
        /// A disabled section still RESOLVES by id — a saved design must keep drawing (owner decision 15 of
        /// I-36A) — but it must not be offered again, and the inspector is exactly the "offer" path.
        /// </summary>
        public IReadOnlyList<StructuralSectionDefinition> Matches()
        {
            IEnumerable<StructuralSectionDefinition> candidates = _family.HasValue
                ? Catalog.ByFamily(_family.Value)
                : Catalog.Enabled;

            if (!string.IsNullOrWhiteSpace(_search) &&
                StructuralSectionDesignationNormalizer.TryNormalize(_search, out var needle))
            {
                candidates = candidates.Where(section =>
                    section.Identity.NormalizedManualLabel.Contains(needle, StringComparison.Ordinal) ||
                    section.Identity.NormalizedEdiDesignation.Contains(needle, StringComparison.Ordinal));
            }

            return candidates.ToArray();
        }

        // ---- Selection -----------------------------------------------------------------------------------

        /// <summary>The section being inspected. Null only when the filters match nothing.</summary>
        public StructuralSectionDefinition Selected
        {
            get => _selected;
            set => _selected = value;
        }

        public bool HasSelection => _selected != null;

        /// <summary>Re-selects the first match when the current selection falls outside the filters.</summary>
        public void EnsureSelectionIsVisible()
        {
            var matches = Matches();

            if (_selected == null || !matches.Contains(_selected))
            {
                _selected = matches.FirstOrDefault();
            }
        }

        // ---- Length --------------------------------------------------------------------------------------

        /// <summary>Length in inches. Rejects zero, negative and non-finite values by keeping the previous one.</summary>
        public double Length
        {
            get => _length;
            set
            {
                if (IsValidLength(value))
                {
                    _length = value;
                }
            }
        }

        public static bool IsValidLength(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;

        /// <summary>Parses what the user typed. Returns false and leaves the length untouched on rubbish.</summary>
        public bool TrySetLength(string text)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var parsed) ||
                double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                if (IsValidLength(parsed))
                {
                    _length = parsed;
                    return true;
                }
            }

            return false;
        }

        // ---- Representation ------------------------------------------------------------------------------

        public SectionViewKind View { get; set; } = SectionViewKind.CrossSection;

        public SectionDetailLevel Detail { get; set; } = SectionDetailLevel.Tabulated;

        public SectionRepresentationMode Mode { get; set; } = SectionRepresentationMode.Wireframe;

        public double RotationDegrees { get; set; }

        public bool Mirrored { get; set; }

        public bool ShowAxis { get; set; }

        public bool ShowEnvelope { get; set; }

        // ---- Result --------------------------------------------------------------------------------------

        /// <summary>
        /// The plan for the current settings, or null when nothing is selected.
        ///
        /// This is the ONE artefact the preview draws and the one AutoCAD would materialise. The inspector
        /// never computes a coordinate of its own.
        /// </summary>
        public StructuralSectionRepresentationPlan BuildPlan()
        {
            if (_selected == null)
            {
                return null;
            }

            var geometry = _factory.Get(_selected, Detail);
            var instance = PrismaticSectionInstance.Create(
                _selected.SectionId, _length, null, RotationDegrees, Mirrored);

            return StructuralSectionPlanBuilder.Build(geometry, instance, new SectionRepresentationOptions
            {
                Viewpoint = SectionViewpoint.Standard(View),
                Mode = Mode,
                Detail = Detail,
                IncludeAxis = ShowAxis,
                IncludeEnvelope = ShowEnvelope
            });
        }

        /// <summary>Total weight of the run, in the section's native unit, from I-36A's authority.</summary>
        public double TotalWeight() =>
            _selected == null ? 0.0 : StructuralSectionUnits.Weight(_selected, _length);

        /// <summary>The weight line the user reads: native unit first, then the equivalent (ADR-0021).</summary>
        public string WeightSummary()
        {
            if (_selected == null)
            {
                return string.Empty;
            }

            var pounds = StructuralSectionUnits.WeightInPounds(_selected, _length);
            var kilograms = StructuralSectionUnits.WeightInKilograms(_selected, _length);

            return StructuralSectionLabelFormatter.FormatWeightWithDesignation(_selected) +
                   "  ·  total " + pounds.ToString("0.#", CultureInfo.InvariantCulture) + " lb (" +
                   kilograms.ToString("0.#", CultureInfo.InvariantCulture) + " kg)";
        }

        /// <summary>A one-line, human description of the fidelity the current detail achieved.</summary>
        /// <summary>
        /// The authority line: who authored the contour on screen.
        ///
        /// Empty for a tabulated-constrained family, and a WARNING for a visually derived one. It is
        /// deliberately not a setting: ADR-0023 decision 22 makes the warning mandatory and non-configurable,
        /// so there is no flag here to turn it off.
        /// </summary>
        public string AuthoritySummary()
        {
            if (_selected == null)
            {
                return string.Empty;
            }

            return _factory.Get(_selected, Detail).Authority == SectionGeometryAuthority.VisualDerived
                ? "GEOMETRIA VISUAL DERIVADA (ADR-0023): la inclinacion del patin y el filete son una " +
                  "convencion de RackCad, no un dato de AISC. Es aproximada, no esta garantizada por ningun " +
                  "fabricante y NO es apta para CNC ni fabricacion. Dimensiones, area, peso y propiedades " +
                  "siguen siendo los que tabula AISC."
                : string.Empty;
        }

        /// <summary>True when the current selection draws under a declared RackCad convention.</summary>
        public bool IsVisualDerived =>
            _selected != null &&
            _factory.Get(_selected, Detail).Authority == SectionGeometryAuthority.VisualDerived;

        public string FidelitySummary()
        {
            if (_selected == null)
            {
                return string.Empty;
            }

            switch (_factory.Get(_selected, Detail).Fidelity)
            {
                case SectionFidelity.Simplified:
                    return "Simplificada: esquinas vivas, por peticion.";
                case SectionFidelity.TabulatedComplete:
                    return "Tabulada completa: incluye todo el detalle que la fuente permite derivar.";
                case SectionFidelity.TabulatedDerived:
                    return "Tabulada derivada: los radios se derivan de la fuente, que no publica todo el " +
                           "detalle de la forma real.";
                case SectionFidelity.DegradedToSimplified:
                    return "DEGRADADA a simplificada: falto un dato para el detalle pedido.";
                default:
                    return string.Empty;
            }
        }

        /// <summary>The diagnostics of the current geometry, for the user to read.</summary>
        public IReadOnlyList<SectionGeometryDiagnostic> Diagnostics() =>
            _selected == null
                ? new SectionGeometryDiagnostic[0]
                : _factory.Get(_selected, Detail).Diagnostics;
    }
}
