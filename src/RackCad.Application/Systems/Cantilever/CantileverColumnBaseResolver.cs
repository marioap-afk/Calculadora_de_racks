using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// Turns the editable intent of a column–base sub-assembly into resolved geometry.
    ///
    /// Everything dimensional it needs comes from <c>StructuralSectionGeometry.Bounds</c> — the envelope of
    /// the contour that will actually be drawn — and never from <c>d</c>, <c>bf</c>, <c>tw</c>, <c>tf</c>,
    /// from the concrete dimension types or from the designation text (ADR-0024, D5). Composing against one
    /// number and drawing another is how a plate ends up not matching its profile.
    ///
    /// It resolves the two sections INDEPENDENTLY and validates only their COMBINATION, so a column and a
    /// base may share a section or not, with no branch either way.
    /// </summary>
    public sealed class CantileverColumnBaseResolver
    {
        private const double FitTolerance = 1e-9;

        private readonly StructuralSectionCatalog _catalog;
        private readonly StructuralSectionGeometryFactory _geometry;
        private readonly ICantileverColumnBaseSectionPolicy _policy;

        public CantileverColumnBaseResolver(
            StructuralSectionCatalog catalog,
            StructuralSectionGeometryFactory geometry,
            ICantileverColumnBaseSectionPolicy policy)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        /// <summary>The policy in force. Exposed so a caller can list what a NEW design may offer.</summary>
        public ICantileverColumnBaseSectionPolicy Policy => _policy;

        public CantileverColumnBaseAssembly Resolve(CantileverColumnBaseDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            var diagnostics = new List<CantileverDiagnostic>();
            var pre = ResolveHeightIndependent(design, diagnostics);

            if (pre == null)
            {
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

            var column = pre.Column;
            var basePiece = pre.BasePiece;
            var columnSection = pre.ColumnSection;
            var baseSection = pre.BaseSection;
            var parameters = pre.Parameters;
            var pattern = pre.Pattern;
            var columnFrame = pre.ColumnFrame;
            var baseFrame = pre.BaseFrame;
            var columnMinX = pre.ColumnMinX;
            var columnMaxX = pre.ColumnMaxX;
            var columnMinY = pre.ColumnMinY;
            var columnMaxY = pre.ColumnMaxY;
            var baseMinX = pre.BaseMinX;
            var baseMaxX = pre.BaseMaxX;
            var baseTopZ = pre.BaseTopZ;
            var baseFarY = pre.BaseFarY;
            var rearThickness = pre.RearPlateThickness;
            var baseBottomZ = CantileverColumnBaseDatum.FloorZ;

            // The height is checked HERE and not in the seam above, because it is the one input the seam does
            // not read. Same rule, same message, same code — only the position moved (I-37C).
            RequirePositive(column.Height, "la altura de la columna", diagnostics);

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

            // The physical top: the plate lifts the column, it does not lengthen it. The NOMINAL cut length is
            // untouched, which is what stops the thickness being counted twice.
            //
            // The start is READ from the placement and not recomputed. Recomputing it agreed with the frame by
            // arithmetic rather than by construction, and a regression that returned the column to the floor
            // moved the punches without moving the column — two elevations that must be one.
            var bottomPlateThickness = column.BottomPlate?.Thickness ?? 0.0;
            var columnStartZ = pre.ColumnStartZ;
            var columnTopZ = CantileverColumnBaseDatum.ColumnTopZ(bottomPlateThickness, column.Height);

            if (pattern.LastConnectionElevation > columnTopZ + FitTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "La columna mide " + Format(column.Height) +
                    " in y no alcanza el ultimo troquel de conexion, que queda en z = " +
                    Format(pattern.LastConnectionElevation) + " in."));
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

            // ---- 6. members -------------------------------------------------------------------------------
            var owner = CantileverPieceTokens.ColumnBaseOwner;

            var columnMember = CantileverStructuralMemberPlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.Column),
                CantileverMemberRole.Column,
                owner,
                PrismaticSectionInstance.Create(columnSection.SectionId, column.Height, columnFrame));

            var baseMember = CantileverStructuralMemberPlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.Base),
                CantileverMemberRole.Base,
                owner,
                PrismaticSectionInstance.Create(baseSection.SectionId, basePiece.Length, baseFrame));

            // ---- 7. flat pieces ---------------------------------------------------------------------------
            var frontPlate = CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.BaseFrontPlate),
                CantileverPlateKind.BaseFront,
                basePiece.FrontPlate.Thickness,
                Vector3D.UnitY,
                baseFarY,
                RectangleXZ(baseMinX, baseMaxX, baseBottomZ, baseTopZ, baseFarY));

            var rearPlate = CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.BaseRearPlate),
                CantileverPlateKind.BaseRear,
                rearThickness,
                Vector3D.UnitY,
                CantileverColumnBaseDatum.ConnectionPlaneY,
                RectangleXZ(baseMinX, baseMaxX, baseBottomZ, pattern.RearPlateTopZ,
                    CantileverColumnBaseDatum.ConnectionPlaneY));

            var bottomPlate = CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.ColumnBottomPlate),
                CantileverPlateKind.ColumnBottom,
                column.BottomPlate.Thickness,
                -Vector3D.UnitZ,

                // Its outline is its BEARING face — the one the column stands on — and the plate extends down
                // from there to the floor. It used to hang BELOW the floor, which put the column through it.
                CantileverColumnBaseDatum.ColumnBottomPlateTopZ(bottomPlateThickness),
                RectangleXY(
                    columnMinX, columnMaxX, columnMinY, columnMaxY,
                    CantileverColumnBaseDatum.ColumnBottomPlateTopZ(bottomPlateThickness)));

            var gusset = BuildGusset(owner, basePiece.Gusset.Thickness, baseFrame.Origin.Y, baseTopZ, pattern, diagnostics);

            if (gusset == null)
            {
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

            // ---- 8. punches -------------------------------------------------------------------------------
            var rearPlatePunches = BuildConnectionPunches(
                owner, CantileverPieceTokens.RearPlatePunch, pattern,
                CantileverPunchSurface.BaseRearPlate,
                CantileverColumnBaseDatum.ConnectionPlaneY + (rearThickness / 2.0));

            // The SHARED datums do not move: the rear plate and the column face drill the same absolute
            // elevations, which is the whole point of one pattern. What the lifted column changes is which of
            // them fall inside it — a hole below its bearing face would be drilled in air.
            var columnConnectionPunches = BuildConnectionPunches(
                owner, CantileverPieceTokens.ColumnConnectionPunch, pattern,
                CantileverPunchSurface.ColumnFace,
                CantileverColumnBaseDatum.ConnectionPlaneY,
                lowerEdgeZ: columnStartZ,
                upperEdgeZ: columnTopZ);

            var columnRegularPunches = BuildRegularPunches(owner, pattern, columnTopZ, parameters, diagnostics);

            var bottomPlatePunches = BuildBottomPlatePunches(
                owner, pattern, columnMinY, columnMaxY, column.BottomPlate.Thickness, parameters, diagnostics);

            return CantileverColumnBaseAssembly.Create(
                columnMember, baseMember, frontPlate, rearPlate, bottomPlate, gusset,
                rearPlatePunches, columnConnectionPunches, columnRegularPunches, bottomPlatePunches,
                pattern, diagnostics);
        }

        // ---- the height-independent seam ------------------------------------------------------------------

        /// <summary>
        /// Everything about a column–base that does NOT depend on the column's height.
        ///
        /// It exists because I-37C's station has to choose which punches each level bolts to BEFORE it knows
        /// how tall the column will be — the height depends on where the levels land. Extracting this seam is
        /// what lets the station ask for the regular grid without inventing a provisional height, which
        /// ADR-0026 D5 forbids.
        ///
        /// Note what is NOT here: the column's top elevation, the regular punches and every piece. Those are
        /// the height-dependent half, and they stay in <see cref="Resolve"/>.
        /// </summary>
        private sealed class HeightIndependentResolution
        {
            public CantileverColumnDesign Column { get; set; }

            public CantileverBaseDesign BasePiece { get; set; }

            public CantileverSectionResolution ColumnSection { get; set; }

            public CantileverSectionResolution BaseSection { get; set; }

            public CantileverColumnBaseVariant Variant { get; set; }

            public CantileverResolvedPunchParameters Parameters { get; set; }

            public StructuralSectionGeometry ColumnGeometry { get; set; }

            public StructuralSectionGeometry BaseGeometry { get; set; }

            public LocalFrame3D ColumnFrame { get; set; }

            /// <summary>
            /// The elevation the COLUMN section starts at: the top face of its bottom plate.
            ///
            /// Published rather than recomputed by the caller, because it and the frame have to be the same
            /// number and «the same by arithmetic» is not the same as «the same by construction».
            /// </summary>
            public double ColumnStartZ { get; set; }

            public LocalFrame3D BaseFrame { get; set; }

            public double ColumnMinX { get; set; }

            public double ColumnMaxX { get; set; }

            public double ColumnMinY { get; set; }

            public double ColumnMaxY { get; set; }

            public double BaseMinX { get; set; }

            public double BaseMaxX { get; set; }

            public double BaseTopZ { get; set; }

            public double BaseFarY { get; set; }

            public double RearPlateThickness { get; set; }

            public CantileverColumnBaseConnectionPattern Pattern { get; set; }
        }

        /// <summary>
        /// The REGULAR PUNCH GRID of a column–base, resolved without a height.
        ///
        /// Public so a station can consume it (I-37C). It returns null and records blocking diagnostics for
        /// exactly the same reasons <see cref="Resolve"/> would, and it runs the same code path — so a design
        /// the station accepts here cannot be one the full resolve rejects for a different reason.
        ///
        /// The height in <paramref name="design"/> is NOT read.
        /// </summary>
        public CantileverColumnRegularPunchGrid ResolveRegularPunchGrid(
            CantileverColumnBaseDesign design, ICollection<CantileverDiagnostic> diagnostics)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            var pre = ResolveHeightIndependent(design, diagnostics);

            return pre == null ? null : CantileverColumnRegularPunchGrid.FromPattern(pre.Pattern);
        }

        private HeightIndependentResolution ResolveHeightIndependent(
            CantileverColumnBaseDesign design, ICollection<CantileverDiagnostic> diagnostics)
        {
            var column = design.Column ?? new CantileverColumnDesign();
            var basePiece = design.Base ?? new CantileverBaseDesign();
            var connection = design.Connection ?? new CantileverColumnBaseConnectionDesign();
            var punches = connection.Punches ?? new CantileverPunchParameters();

            // ---- 1. sections: the single parse + lookup boundary -------------------------------------------
            var columnSection = CantileverSectionResolver.Resolve(
                _catalog, column.SectionId, CantileverMemberRole.Column, diagnostics);
            var baseSection = CantileverSectionResolver.Resolve(
                _catalog, basePiece.SectionId, CantileverMemberRole.Base, diagnostics);

            if (!columnSection.IsResolved || !baseSection.IsResolved)
            {
                return null;
            }

            // ---- 2. eligibility ---------------------------------------------------------------------------
            var variant = ResolveVariant(columnSection.Section, baseSection.Section, connection, diagnostics);
            var parameters = ResolveParameters(punches, diagnostics);
            ValidateLengths(column, basePiece, diagnostics);

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return null;
            }

            // ---- 3. contours ------------------------------------------------------------------------------
            var columnGeometry = _geometry.Get(columnSection.SectionId, variant.Detail);
            var baseGeometry = _geometry.Get(baseSection.SectionId, variant.Detail);

            ReportVisualAuthority(columnGeometry, CantileverMemberRole.Column, diagnostics);
            ReportVisualAuthority(baseGeometry, CantileverMemberRole.Base, diagnostics);

            var columnBounds = columnGeometry.Bounds;
            var baseBounds = baseGeometry.Bounds;

            // ---- 4. placement: the frame authority reads the REGISTERED orientation -----------------------
            // The frames are not built here. CantileverColumnBaseFrameResolver owns them, so the variant's
            // declared orientation is what decides them instead of being data nobody reads.
            // The column stands on the TOP FACE of its bottom plate, not on the floor. The base does not move
            // with it: it is a piece that rests on the ground (owner decision, I-37D).
            var columnStartZ = CantileverColumnBaseDatum.ColumnStartZ(column.BottomPlate?.Thickness ?? 0.0);

            var columnFrame = CantileverColumnBaseFrameResolver.ColumnFrame(
                variant.ColumnOrientation, columnGeometry, columnStartZ);

            var rearThickness = basePiece.RearPlate.Thickness;
            var baseFrame = CantileverColumnBaseFrameResolver.BaseFrame(
                variant.BaseOrientation, baseGeometry, rearThickness);

            var baseHalfWidth = baseBounds.Width / 2.0;
            var baseMinX = -baseHalfWidth;
            var baseMaxX = baseHalfWidth;
            var baseBottomZ = CantileverColumnBaseDatum.FloorZ;
            var baseTopZ = baseBottomZ + baseBounds.Height;

            var columnMinX = columnFrame.Origin.X + columnBounds.MinX;
            var columnMaxX = columnFrame.Origin.X + columnBounds.MaxX;

            // ---- 5. the shared authority ------------------------------------------------------------------
            var pattern = CantileverColumnBaseConnectionPattern.Build(
                columnMinX, columnMaxX, baseMinX, baseMaxX, baseBottomZ, baseTopZ, parameters, diagnostics);

            if (pattern == null)
            {
                return null;
            }

            return new HeightIndependentResolution
            {
                Column = column,
                BasePiece = basePiece,
                ColumnSection = columnSection,
                BaseSection = baseSection,
                Variant = variant,
                Parameters = parameters,
                ColumnGeometry = columnGeometry,
                BaseGeometry = baseGeometry,
                ColumnFrame = columnFrame,
                ColumnStartZ = columnStartZ,
                BaseFrame = baseFrame,
                ColumnMinX = columnMinX,
                ColumnMaxX = columnMaxX,
                ColumnMinY = columnFrame.Origin.Y + columnBounds.MinY,
                ColumnMaxY = columnFrame.Origin.Y + columnBounds.MaxY,
                BaseMinX = baseMinX,
                BaseMaxX = baseMaxX,
                BaseTopZ = baseTopZ,
                BaseFarY = baseFrame.Origin.Y + basePiece.Length,
                RearPlateThickness = rearThickness,
                Pattern = pattern
            };
        }

        // ---- eligibility ----------------------------------------------------------------------------------


        private CantileverColumnBaseVariant ResolveVariant(
            StructuralSectionDefinition columnSection,
            StructuralSectionDefinition baseSection,
            CantileverColumnBaseConnectionDesign connection,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            foreach (var pair in new[]
                     {
                         (Section: columnSection, Role: CantileverMemberRole.Column),
                         (Section: baseSection, Role: CantileverMemberRole.Base)
                     })
            {
                if (!_policy.AllowedFamilies.Contains(pair.Section.Family))
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.SectionFamilyNotEligible,
                        "La familia '" + pair.Section.Family + "' de " +
                        (pair.Role == CantileverMemberRole.Column ? "la columna" : "la base") +
                        " no esta admitida por este diseno de Cantilever."));
                }
            }

            if (!_policy.TryGetVariant(columnSection.SectionId, baseSection.SectionId, out var variant))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.CombinationNotEligible,
                    "La combinacion columna '" + columnSection.SectionId.Value + "' con base '" +
                    baseSection.SectionId.Value + "' no esta registrada como un diseno valido."));
                return null;
            }

            if (variant.Kind != connection.Variant)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.VariantMismatch,
                    "El diseno declara la variante '" + connection.Variant +
                    "' pero la combinacion esta registrada como '" + variant.Kind + "'."));
                return null;
            }

            // A variant may be registered with an orientation that has no frame rule. Checking it here
            // turns what would be an exception out of the frame authority into a diagnostic the user can
            // read, and keeps the authority itself free to fail closed.
            if (!CantileverColumnBaseFrameResolver.IsSupported(variant.ColumnOrientation))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.OrientationNotSupported,
                    "La orientacion de columna '" + variant.ColumnOrientation +
                    "' registrada en la variante no tiene regla de marco."));
                return null;
            }

            if (!CantileverColumnBaseFrameResolver.IsSupported(variant.BaseOrientation))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.OrientationNotSupported,
                    "La orientacion de base '" + variant.BaseOrientation +
                    "' registrada en la variante no tiene regla de marco."));
                return null;
            }

            return variant;
        }

        private static CantileverResolvedPunchParameters ResolveParameters(
            CantileverPunchParameters punches,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            RequirePositive(punches.Diameter, "el diametro de troquel", diagnostics);
            RequirePositive(punches.HorizontalEndOffset, "el offset horizontal de troquel", diagnostics);
            RequirePositive(punches.ConnectionPitch, "el pitch de conexion", diagnostics);
            RequirePositive(punches.RearPlateVerticalEndOffset, "el offset vertical de la placa posterior", diagnostics);
            RequirePositive(punches.RegularColumnPitch, "el pitch regular de la columna", diagnostics);
            RequirePositive(punches.ColumnBottomPlatePitch, "el pitch de la placa inferior", diagnostics);

            if (punches.ConnectionPunchesAboveBase < 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El numero de troqueles sobre la seccion de la base no puede ser negativo."));
            }

            // The two margins the design used to have to supply are GONE (I-37D ronda 2, motivo 1). The owner
            // rejected them as parameters without product utility: what limits a hole is not a number somebody
            // types, it is whether the HOLE ITSELF fits inside the piece. The rule is now the radius, and it
            // lives in the pattern and the grid. The properties survive on the integrated I-37A design type as
            // legacy data that nothing reads — removing them would break an API already in main.

            // ---- geometric compatibility of the punches themselves ----------------------------------------
            // Every offset above is measured edge-to-CENTRE, so a hole physically fits only if the offset is
            // at least its RADIUS; every pitch is centre-to-centre, so consecutive holes only clear each
            // other if the pitch is at least a whole DIAMETER. Without these, a legal-looking design draws
            // holes that spill past an edge or merge into a slot, and nothing says so.
            var radius = punches.Diameter / 2.0;

            RequireAtLeast(punches.HorizontalEndOffset, radius,
                "el offset horizontal de troquel", "el radio del troquel",
                CantileverDiagnostics.EdgeOffsetBelowRadius, diagnostics);

            RequireAtLeast(punches.RearPlateVerticalEndOffset, radius,
                "el offset vertical de la placa posterior", "el radio del troquel",
                CantileverDiagnostics.EdgeOffsetBelowRadius, diagnostics);

            RequireAtLeast(punches.ConnectionPitch, punches.Diameter,
                "el pitch de conexion", "el diametro del troquel",
                CantileverDiagnostics.PitchBelowDiameter, diagnostics);

            RequireAtLeast(punches.RegularColumnPitch, punches.Diameter,
                "el pitch regular de la columna", "el diametro del troquel",
                CantileverDiagnostics.PitchBelowDiameter, diagnostics);

            RequireAtLeast(punches.ColumnBottomPlatePitch, punches.Diameter,
                "el pitch de la placa inferior", "el diametro del troquel",
                CantileverDiagnostics.PitchBelowDiameter, diagnostics);

            return new CantileverResolvedPunchParameters(
                punches.Diameter,
                punches.HorizontalEndOffset,
                punches.ConnectionPitch,
                punches.RearPlateVerticalEndOffset,
                punches.RegularColumnPitch,
                punches.ConnectionPunchesAboveBase,
                punches.ColumnBottomPlatePitch,

                // The two legacy margins are NOT read. They travel as zero so the resolved record keeps its
                // shape for the integrated I-37A API, and every rule that used them now uses the radius.
                legacyBottomPlateEndOffset: 0.0,
                legacyColumnTopPunchOffset: 0.0);
        }

        /// <summary>
        /// The lengths and thicknesses that do NOT depend on the column height.
        ///
        /// The height is validated by <see cref="RequireUsableHeight"/> instead, in the height-DEPENDENT half.
        /// Splitting them is what lets a station ask for the regular grid before it has a height: a check on a
        /// value the caller has not chosen yet would reject every such request (I-37C).
        /// </summary>
        private static void ValidateLengths(
            CantileverColumnDesign column,
            CantileverBaseDesign basePiece,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            RequirePositive(basePiece.Length, "la longitud de la base", diagnostics);
            RequirePositive(column.BottomPlate.Thickness, "el espesor de la placa inferior de la columna", diagnostics);
            RequirePositive(basePiece.FrontPlate.Thickness, "el espesor de la placa frontal", diagnostics);
            RequirePositive(basePiece.RearPlate.Thickness, "el espesor de la placa posterior", diagnostics);
            RequirePositive(basePiece.Gusset.Thickness, "el espesor del cartabon", diagnostics);
        }

        private static void ReportVisualAuthority(
            StructuralSectionGeometry geometry,
            CantileverMemberRole role,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (!geometry.IsVisualDerived)
            {
                return;
            }

            // ADR-0023's warning must travel with the geometry. The long text lives in the consumers that
            // show it; repeating it here would be a second copy to keep in step.
            diagnostics.Add(CantileverDiagnostic.Warning(
                CantileverDiagnostics.SectionVisualDerived,
                "El contorno de " + (role == CantileverMemberRole.Column ? "la columna" : "la base") +
                " ('" + geometry.SectionId.Value +
                "') incorpora una convencion visual propia de RackCad (ADR-0023): es aproximado y no apto " +
                "para CNC ni fabricacion."));
        }

        // ---- pieces ---------------------------------------------------------------------------------------

        private static CantileverGussetPlan BuildGusset(
            string owner,
            double thickness,
            double baseNearY,
            double baseTopZ,
            CantileverColumnBaseConnectionPattern pattern,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            // The legs are EQUAL, so the hypotenuse is at 45 degrees, and the vertical one is how far the
            // rear plate reaches above the base section. It is NOT three pitches: how far the plate reaches
            // depends on where the last punch INSIDE the base fell.
            var leg = pattern.TopExtension;

            if (leg <= FitTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "La placa posterior no sobresale de la seccion de la base, asi que el cartabon no tiene cateto."));
                return null;
            }

            var faceX = -thickness / 2.0;

            return CantileverGussetPlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.Gusset),
                thickness,
                Vector3D.UnitX,
                faceX,
                new Point3D(faceX, baseNearY, baseTopZ),
                new Point3D(faceX, baseNearY, baseTopZ + leg),
                new Point3D(faceX, baseNearY + leg, baseTopZ),
                leg,
                leg);
        }

        private static IReadOnlyList<CantileverPunchPlan> BuildConnectionPunches(
            string owner,
            string token,
            CantileverColumnBaseConnectionPattern pattern,
            CantileverPunchSurface surface,
            double y,
            double? lowerEdgeZ = null,
            double? upperEdgeZ = null)
        {
            var id = CantileverPieceId.Create(owner, token);
            var list = new List<CantileverPunchPlan>(2 * pattern.Elevations.Count);
            var index = 0;

            for (var row = 0; row < 2; row++)
            {
                for (var i = 0; i < pattern.Elevations.Count; i++)
                {
                    var datum = pattern.DatumAt(row, i);

                    if (!FitsBetween(datum.V, datum.Diameter / 2.0, lowerEdgeZ, upperEdgeZ))
                    {
                        continue;
                    }

                    list.Add(new CantileverPunchPlan(
                        id.At(index++),
                        surface,
                        new Point3D(datum.U, y, datum.V),
                        datum));
                }
            }

            return list;
        }

        /// <summary>
        /// The ONE fitting rule, applied to a host with optional edges: a WHOLE hole must clear both.
        ///
        /// A null edge means «this host does not bound the hole there» — the base's rear plate spans the whole
        /// connection region by construction, so it passes none. Tangent counts as fitting: the floor is
        /// inclusive, exactly as it is for the regular grid.
        /// </summary>
        private static bool FitsBetween(double centre, double radius, double? lowerEdge, double? upperEdge) =>
            (lowerEdge == null || centre - radius >= lowerEdge.Value - FitTolerance) &&
            (upperEdge == null || centre + radius <= upperEdge.Value + FitTolerance);

        private static IReadOnlyList<CantileverPunchPlan> BuildRegularPunches(
            string owner,
            CantileverColumnBaseConnectionPattern pattern,
            double columnTopZ,
            CantileverResolvedPunchParameters parameters,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            // The regular grid CONTINUES the connection region; it is not a lattice recomputed from the
            // floor. But WHERE its elevations fall is no longer decided here: since I-37C,
            // CantileverColumnRegularPunchGrid is the one authority, and this resolver is one of its two
            // consumers — the station is the other. Two copies of one spacing formula is PB-004
            // (ADR-0026, D5). The extraction was mechanical and is pinned by the characterization suite.
            var grid = CantileverColumnRegularPunchGrid.FromPattern(pattern);
            var elevations = grid.ElevationsUpTo(columnTopZ);

            if (elevations.Count == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.NoRegularPunchFits,
                    "La columna no admite ningun troquel regular: el primero quedaria en z = " +
                    Format(grid.FirstElevation) + " in y el ultimo agujero entero cabe hasta z = " +
                    Format(columnTopZ - grid.PunchRadius) + " in."));
                return Array.Empty<CantileverPunchPlan>();
            }

            var id = CantileverPieceId.Create(owner, CantileverPieceTokens.ColumnRegularPunch);
            var list = new List<CantileverPunchPlan>(2 * elevations.Count);
            var index = 0;

            foreach (var rowX in grid.RowX)
            {
                foreach (var z in elevations)
                {
                    var datum = new CantileverPunchDatum(
                        CantileverPunchAxis.AlongY, rowX, z, parameters.Diameter);

                    list.Add(new CantileverPunchPlan(
                        id.At(index++),
                        CantileverPunchSurface.ColumnFace,
                        new Point3D(rowX, CantileverColumnBaseDatum.ConnectionPlaneY, z),
                        datum));
                }
            }

            return list;
        }

        private static IReadOnlyList<CantileverPunchPlan> BuildBottomPlatePunches(
            string owner,
            CantileverColumnBaseConnectionPattern pattern,
            double columnMinY,
            double columnMaxY,
            double plateThickness,
            CantileverResolvedPunchParameters parameters,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            // "Starting from the centre, symmetric" means PAIRS at +/- p/2, +/- 3p/2, +/- 5p/2 ... There is
            // deliberately NO punch on the centre line: an odd pattern would put one there, and the owner's
            // description is explicit that the two rows start from the centre as a pair.
            var centre = (columnMinY + columnMaxY) / 2.0;
            // The plate keeps every WHOLE hole that fits: a centre is legal when the hole clears both edges by
            // its own radius, and by nothing more. No margin is added, no circle is clipped and no half hole is
            // invented (I-37D ronda 2, motivo 1).
            var radius = parameters.Diameter / 2.0;
            var lowLimit = columnMinY + radius;
            var highLimit = columnMaxY - radius;

            var offsets = new List<double>();

            for (var k = 1; ; k++)
            {
                var offset = ((2 * k) - 1) * parameters.ColumnBottomPlatePitch / 2.0;

                // Both members of the pair have to respect the offset from BOTH ends; by symmetry they fail
                // together, and testing both keeps that explicit instead of relying on it.
                if (centre - offset < lowLimit - FitTolerance || centre + offset > highLimit + FitTolerance)
                {
                    break;
                }

                offsets.Add(offset);
            }

            if (offsets.Count == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.NoBottomPlatePunchFits,
                    "La placa inferior de la columna no admite ningun par de troqueles con un offset de " +
                    Format(parameters.ColumnBottomPlateEndOffset) + " in desde sus extremos."));
                return Array.Empty<CantileverPunchPlan>();
            }

            var id = CantileverPieceId.Create(owner, CantileverPieceTokens.ColumnBottomPlatePunch);
            var list = new List<CantileverPunchPlan>(4 * offsets.Count);
            var index = 0;
            // The mid-plane of the plate, which now sits ABOVE the floor.
            var z = CantileverColumnBaseDatum.ColumnBottomPlateBottomZ + (plateThickness / 2.0);

            foreach (var rowX in pattern.RowX)
            {
                foreach (var y in offsets.SelectMany(o => new[] { centre - o, centre + o }).OrderBy(v => v))
                {
                    var datum = new CantileverPunchDatum(
                        CantileverPunchAxis.AlongZ, rowX, y, parameters.Diameter);

                    list.Add(new CantileverPunchPlan(
                        id.At(index++),
                        CantileverPunchSurface.ColumnBottomPlate,
                        new Point3D(rowX, y, z),
                        datum));
                }
            }

            return list;
        }

        // ---- helpers --------------------------------------------------------------------------------------

        private static IReadOnlyList<Point3D> RectangleXZ(
            double minX, double maxX, double minZ, double maxZ, double y) =>
            new[]
            {
                new Point3D(minX, y, minZ),
                new Point3D(maxX, y, minZ),
                new Point3D(maxX, y, maxZ),
                new Point3D(minX, y, maxZ)
            };

        private static IReadOnlyList<Point3D> RectangleXY(
            double minX, double maxX, double minY, double maxY, double z) =>
            new[]
            {
                new Point3D(minX, minY, z),
                new Point3D(maxX, minY, z),
                new Point3D(maxX, maxY, z),
                new Point3D(minX, maxY, z)
            };

        private static void RequirePositive(double value, string what, ICollection<CantileverDiagnostic> diagnostics)
        {
            if (!GeometryTolerance.IsFinite(value) || value <= 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El valor de " + what + " debe ser un numero positivo; se recibio " + Format(value) + "."));
            }
        }

        /// <summary>
        /// A geometric floor with its OWN diagnostic code. Exact equality passes: an offset of exactly the
        /// radius puts the hole tangent to the edge, which is a real product decision and not a defect.
        /// </summary>
        private static void RequireAtLeast(
            double value,
            double minimum,
            string what,
            string minimumName,
            string code,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (!GeometryTolerance.IsFinite(value) || value + FitTolerance < minimum)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    code,
                    "El valor de " + what + " (" + Format(value) + " in) es menor que " + minimumName +
                    " (" + Format(minimum) + " in): el troquel no cabe."));
            }
        }

        private static double? RequireSupplied(
            double? value, string what, ICollection<CantileverDiagnostic> diagnostics)
        {
            if (value == null)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.RequiredParameterMissing,
                    "El diseno debe declarar " + what + ": no existe un valor por omision aprobado."));
                return null;
            }

            if (!GeometryTolerance.IsFinite(value.Value) || value.Value < 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El valor de " + what + " no puede ser negativo; se recibio " + Format(value.Value) + "."));
                return null;
            }

            return value;
        }

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
