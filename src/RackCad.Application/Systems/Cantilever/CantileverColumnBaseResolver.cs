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
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

            // ---- 2. eligibility ---------------------------------------------------------------------------
            var variant = ResolveVariant(columnSection.Section, baseSection.Section, connection, diagnostics);
            var parameters = ResolveParameters(punches, diagnostics);
            ValidateLengths(column, basePiece, diagnostics);

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

            // ---- 3. contours ------------------------------------------------------------------------------
            var columnGeometry = _geometry.Get(columnSection.SectionId, variant.Detail);
            var baseGeometry = _geometry.Get(baseSection.SectionId, variant.Detail);

            ReportVisualAuthority(columnGeometry, CantileverMemberRole.Column, diagnostics);
            ReportVisualAuthority(baseGeometry, CantileverMemberRole.Base, diagnostics);

            var columnBounds = columnGeometry.Bounds;
            var baseBounds = baseGeometry.Bounds;

            // ---- 4. placement, all of it derived from Bounds ----------------------------------------------
            // Column: vertical, section local X across the run and local Y along the base direction. Its
            // origin is pushed so the CONNECTING FACE lands on the datum plane and the transverse centre on
            // x = 0. The frame origin sits at the section CENTROID, so the shift is a bounds offset — never
            // "half of d".
            var columnFrame = LocalFrame3D.Create(
                new Point3D(
                    -columnBounds.Center.X,
                    CantileverColumnBaseDatum.ConnectionPlaneY - columnBounds.MaxY,
                    CantileverColumnBaseDatum.FloorZ),
                Vector3D.UnitZ,
                Vector3D.UnitX);

            var columnMinX = columnFrame.Origin.X + columnBounds.MinX;
            var columnMaxX = columnFrame.Origin.X + columnBounds.MaxX;
            var columnMinY = columnFrame.Origin.Y + columnBounds.MinY;
            var columnMaxY = columnFrame.Origin.Y + columnBounds.MaxY;
            var columnTopZ = CantileverColumnBaseDatum.FloorZ + column.Height;

            // Base: longitudinal axis +Y, section depth VERTICAL. Asking for local Y to map to world +Z
            // means referenceX = up x forward = Z x Y = -X, which LocalFrame3D.Create then completes.
            var rearThickness = basePiece.RearPlate.Thickness;
            var baseFrame = LocalFrame3D.Create(
                new Point3D(
                    baseBounds.Center.X,
                    CantileverColumnBaseDatum.ConnectionPlaneY + rearThickness,
                    CantileverColumnBaseDatum.FloorZ - baseBounds.MinY),
                Vector3D.UnitY,
                Vector3D.UnitZ.Cross(Vector3D.UnitY));

            var baseHalfWidth = baseBounds.Width / 2.0;
            var baseMinX = -baseHalfWidth;
            var baseMaxX = baseHalfWidth;
            var baseBottomZ = CantileverColumnBaseDatum.FloorZ;
            var baseTopZ = baseBottomZ + baseBounds.Height;
            var baseFarY = baseFrame.Origin.Y + basePiece.Length;

            // ---- 5. the shared authority ------------------------------------------------------------------
            var pattern = CantileverColumnBaseConnectionPattern.Build(
                columnMinX, columnMaxX, baseMinX, baseMaxX, baseBottomZ, baseTopZ, parameters, diagnostics);

            if (pattern == null)
            {
                return CantileverColumnBaseAssembly.Blocked(diagnostics);
            }

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
                CantileverColumnBaseDatum.FloorZ,
                RectangleXY(columnMinX, columnMaxX, columnMinY, columnMaxY, CantileverColumnBaseDatum.FloorZ));

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

            var columnConnectionPunches = BuildConnectionPunches(
                owner, CantileverPieceTokens.ColumnConnectionPunch, pattern,
                CantileverPunchSurface.ColumnFace,
                CantileverColumnBaseDatum.ConnectionPlaneY);

            var columnRegularPunches = BuildRegularPunches(owner, pattern, columnTopZ, parameters, diagnostics);

            var bottomPlatePunches = BuildBottomPlatePunches(
                owner, pattern, columnMinY, columnMaxY, column.BottomPlate.Thickness, parameters, diagnostics);

            return CantileverColumnBaseAssembly.Create(
                columnMember, baseMember, frontPlate, rearPlate, bottomPlate, gusset,
                rearPlatePunches, columnConnectionPunches, columnRegularPunches, bottomPlatePunches,
                pattern, diagnostics);
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

            // The two the owner did NOT approve a default for. Missing is REJECTED, never filled in: a value
            // invented here would be indistinguishable from an approved one.
            var bottomOffset = RequireSupplied(
                punches.ColumnBottomPlateEndOffset,
                "el offset desde los extremos de la placa inferior de la columna a sus troqueles",
                diagnostics);

            var topOffset = RequireSupplied(
                punches.ColumnTopPunchOffset,
                "el offset superior de la columna al ultimo troquel regular",
                diagnostics);

            return new CantileverResolvedPunchParameters(
                punches.Diameter,
                punches.HorizontalEndOffset,
                punches.ConnectionPitch,
                punches.RearPlateVerticalEndOffset,
                punches.RegularColumnPitch,
                punches.ConnectionPunchesAboveBase,
                punches.ColumnBottomPlatePitch,
                bottomOffset ?? 0.0,
                topOffset ?? 0.0);
        }

        private static void ValidateLengths(
            CantileverColumnDesign column,
            CantileverBaseDesign basePiece,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            RequirePositive(column.Height, "la altura de la columna", diagnostics);
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
            double y)
        {
            var id = CantileverPieceId.Create(owner, token);
            var list = new List<CantileverPunchPlan>(2 * pattern.Elevations.Count);
            var index = 0;

            for (var row = 0; row < 2; row++)
            {
                for (var i = 0; i < pattern.Elevations.Count; i++)
                {
                    var datum = pattern.DatumAt(row, i);
                    list.Add(new CantileverPunchPlan(
                        id.At(index++),
                        surface,
                        new Point3D(datum.U, y, datum.V),
                        datum));
                }
            }

            return list;
        }

        private static IReadOnlyList<CantileverPunchPlan> BuildRegularPunches(
            string owner,
            CantileverColumnBaseConnectionPattern pattern,
            double columnTopZ,
            CantileverResolvedPunchParameters parameters,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            // The regular grid CONTINUES the connection region; it is not a lattice recomputed from the
            // floor. The first regular punch sits one REGULAR pitch above the last connection punch — not
            // one connection pitch, and never a duplicate of it.
            var first = pattern.LastConnectionElevation + parameters.RegularColumnPitch;
            var ceiling = columnTopZ - parameters.ColumnTopPunchOffset;

            var elevations = new List<double>();

            for (var z = first; z <= ceiling + FitTolerance; z += parameters.RegularColumnPitch)
            {
                elevations.Add(z);
            }

            if (elevations.Count == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.NoRegularPunchFits,
                    "La columna no admite ningun troquel regular: el primero quedaria en z = " +
                    Format(first) + " in y el limite superior esta en z = " + Format(ceiling) + " in."));
                return Array.Empty<CantileverPunchPlan>();
            }

            var id = CantileverPieceId.Create(owner, CantileverPieceTokens.ColumnRegularPunch);
            var list = new List<CantileverPunchPlan>(2 * elevations.Count);
            var index = 0;

            foreach (var rowX in pattern.RowX)
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
            var lowLimit = columnMinY + parameters.ColumnBottomPlateEndOffset;
            var highLimit = columnMaxY - parameters.ColumnBottomPlateEndOffset;

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
            var z = CantileverColumnBaseDatum.FloorZ - (plateThickness / 2.0);

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
