using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// One pass of the editor's recompute: the design it started from, the line it resolved, its BOM and the
    /// three view plans.
    ///
    /// It is a RESULT and not a state: the editor keeps the last VALID computation to render, and a failed pass
    /// replaces nothing. That is what stops a window from drawing one line while its fields describe another.
    /// </summary>
    public sealed class CantileverLineEditorComputation
    {
        internal CantileverLineEditorComputation(
            CantileverLineDesign design,
            CantileverLineAssembly line,
            BillOfMaterials bom,
            IReadOnlyList<CantileverViewPlan> views)
        {
            Design = design;
            Line = line;
            Bom = bom;
            Views = views ?? Array.Empty<CantileverViewPlan>();
        }

        /// <summary>A deep copy of the design this pass resolved. Never the editor's live instance.</summary>
        public CantileverLineDesign Design { get; }

        public CantileverLineAssembly Line { get; }

        /// <summary>The component BOM, or null when the line is blocked (there is nothing to quote).</summary>
        public BillOfMaterials Bom { get; }

        /// <summary>The initial view set: frontal, planta and ONE lateral. Empty when the line is blocked.</summary>
        public IReadOnlyList<CantileverViewPlan> Views { get; }

        public bool IsValid => Line != null && !Line.IsBlocked;

        public IReadOnlyList<CantileverDiagnostic> Diagnostics =>
            Line?.Diagnostics ?? (IReadOnlyList<CantileverDiagnostic>)Array.Empty<CantileverDiagnostic>();

        /// <summary>The first blocking message, for a status band. Null when the line resolved.</summary>
        public string Error => Diagnostics.FirstOrDefault(d => d.IsBlocking)?.Message;

        /// <summary>Every non-blocking diagnostic, so a usable line can still say what it had to say.</summary>
        public IReadOnlyList<CantileverDiagnostic> Warnings =>
            Diagnostics.Where(d => !d.IsBlocking).ToList();
    }

    /// <summary>
    /// THE recompute authority of the Cantilever editor.
    ///
    /// The window reads controls into a <see cref="CantileverLineDesign"/> and hands it here; everything else —
    /// which policies apply, how the line resolves, what its BOM is and what its views are — happens once, in
    /// this class, in the layer that owns those answers. A window that called the resolver, the BOM builder and
    /// the view builder itself would be a second orchestration of the same three steps, and the Plugin's own
    /// path would be free to disagree with it (the I-20/I-21 lesson, one system further on).
    ///
    /// It computes NO geometry: it composes the authorities of I-37A to I-37D and adds none of its own.
    /// </summary>
    public sealed class CantileverLineEditorAssembler
    {
        private readonly ICantileverColumnBaseSectionPolicy _columnBasePolicy;
        private readonly ICantileverArmSectionPolicy _armPolicy;

        /// <summary>
        /// Builds an assembler over an already-loaded, already-validated section catalogue.
        ///
        /// The catalogue arrives loaded because Cantilever never reads a file, and the policies are the
        /// PRODUCTION ones — the editor does not get to decide what a valid Cantilever section is.
        /// </summary>
        public CantileverLineEditorAssembler(
            StructuralSectionCatalog catalogue, StructuralSectionGeometryFactory geometryFactory = null)
        {
            Catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
            GeometryFactory = geometryFactory ?? new StructuralSectionGeometryFactory(catalogue);
            _columnBasePolicy = CantileverCataloguePolicies.ColumnBase(catalogue);
            _armPolicy = CantileverCataloguePolicies.Arm(catalogue);
        }

        public StructuralSectionCatalog Catalogue { get; }

        public StructuralSectionGeometryFactory GeometryFactory { get; }

        /// <summary>
        /// Resolves a design into a full computation.
        ///
        /// A design that cannot be resolved comes back BLOCKED — with its diagnostics and no BOM and no views —
        /// rather than as an exception: an editor recomputes on every keystroke, and half of those passes are
        /// expected to be invalid while the user is still typing.
        /// </summary>
        public CantileverLineEditorComputation Build(CantileverLineDesign design, int lateralStationIndex = 0)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            // The computation keeps its OWN copy. The editor goes on mutating its live design, and a result that
            // held that instance would silently describe a line nobody resolved.
            var snapshot = design.DeepCopy();
            var line = CantileverLineResolver.Resolve(
                snapshot, Catalogue, GeometryFactory, _columnBasePolicy, _armPolicy);

            if (line.IsBlocked)
            {
                return new CantileverLineEditorComputation(snapshot, line, null, Array.Empty<CantileverViewPlan>());
            }

            var index = lateralStationIndex >= 0 && lateralStationIndex < line.Stations.Count
                ? lateralStationIndex
                : 0;

            return new CantileverLineEditorComputation(
                snapshot,
                line,
                CantileverLineBomBuilder.Build(line),
                CantileverViewPlanBuilder.BuildInitialSet(line, GeometryFactory, index));
        }

        /// <summary>
        /// One view of an already-resolved line, for a preview that changes view without re-resolving.
        ///
        /// Re-resolving to look at the planta would make the picture depend on when it was asked for; the line
        /// is the model, and a view is a projection of it.
        /// </summary>
        public CantileverViewPlan View(
            CantileverLineAssembly line, CantileverViewKind view, int stationIndex = 0)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            return CantileverViewPlanBuilder.Build(line, view, GeometryFactory, stationIndex);
        }

        /// <summary>The catalogue sections a family offers, ordered as the catalogue holds them. For a picker.</summary>
        public IReadOnlyList<StructuralSectionDefinition> SectionsOf(StructuralSectionFamily family) =>
            Catalogue.ByFamily(family);
    }
}
