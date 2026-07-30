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
    /// Turns the editable intent of ONE arm into resolved geometry, against a column that is already resolved.
    ///
    /// It consumes <see cref="CantileverColumnBaseAssembly"/> rather than re-deriving anything about the
    /// column: the transverse punch columns, their elevations, their datums and the column's own envelope all
    /// come from there. That is what keeps one authority for one set of bolts (ADR-0025, D5), and it is why
    /// this type has no spacing constant of its own.
    ///
    /// Every exterior dimension comes from <c>StructuralSectionGeometry.Bounds</c>. Nothing reads <c>d</c>,
    /// <c>bf</c>, a concrete dimension type or the designation.
    /// </summary>
    public sealed class CantileverArmResolver
    {
        private const double FitTolerance = 1e-9;

        private readonly StructuralSectionCatalog _catalog;
        private readonly StructuralSectionGeometryFactory _geometry;
        private readonly ICantileverArmSectionPolicy _policy;

        public CantileverArmResolver(
            StructuralSectionCatalog catalog,
            StructuralSectionGeometryFactory geometry,
            ICantileverArmSectionPolicy policy)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public ICantileverArmSectionPolicy Policy => _policy;

        /// <summary>
        /// Resolves one arm.
        /// </summary>
        /// <param name="design">The editable intent.</param>
        /// <param name="column">The column this arm bolts to, already resolved by I-37A.</param>
        /// <param name="owner">
        /// A deterministic owner token supplied by the CALLER. The arm does not invent one, because the future
        /// station is what will name it; until then a caller may pass <c>CantileverPieceTokens.ArmOwner</c>.
        /// </param>
        public CantileverArmAssembly Resolve(
            CantileverArmDesign design, CantileverColumnBaseAssembly column, string owner)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Un brazo necesita el token de su propietario.", nameof(owner));
            }

            var diagnostics = new List<CantileverDiagnostic>();
            var body = design.Body ?? new CantileverArmBodyDesign();
            var mount = design.MountingPlate ?? new CantileverArmMountingPlateDesign();
            var end = design.EndPlate ?? new CantileverArmEndPlateDesign();
            var side = design.Side;

            if (column.IsBlocked || column.Pattern == null || column.ColumnBottomPlate == null)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmColumnAssemblyBlocked,
                    "El subensamble de columna esta bloqueado: un brazo no puede resolverse contra el."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            if (!CantileverArmFrameResolver.IsSupported(side))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSideNotSupported,
                    "El lado de brazo '" + side + "' no tiene regla de marco."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            if (!CantileverArmBodyArrangementResolver.IsSupported(body.Arrangement))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmArrangementNotSupported,
                    "El arreglo de cuerpo '" + body.Arrangement + "' no tiene regla de colocacion."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            // ---- 1. section: the single parse + lookup boundary of I-37A, reused ---------------------------
            var section = CantileverSectionResolver.Resolve(
                _catalog, body.SectionId, CantileverMemberRole.Arm, diagnostics);

            if (!section.IsResolved)
            {
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            // ---- 2. eligibility ---------------------------------------------------------------------------
            if (!_policy.TryGetVariant(section.SectionId, body.Arrangement, out var variant))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.CombinationNotEligible,
                    "La combinacion seccion '" + section.SectionId.Value + "' con arreglo '" +
                    body.Arrangement + "' no esta registrada como un diseno valido de brazo."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            if (!CantileverArmFrameResolver.IsSupported(variant.Orientation))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSideNotSupported,
                    "La orientacion de cuerpo '" + variant.Orientation + "' no tiene regla de marco."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            if (CantileverArmBodyArrangementResolver.RequiresChannel(body.Arrangement) &&
                section.Section.Family != StructuralSectionFamily.Channel)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmArrangementRequiresChannel,
                    "El arreglo '" + body.Arrangement + "' solo admite familia Channel; '" +
                    section.SectionId.Value + "' es de familia " + section.Section.Family + "."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            // ---- 3. parameters ----------------------------------------------------------------------------
            var verticalEndOffset = ValidateParameters(body, mount, end, column.Pattern.Parameters.Diameter, diagnostics);

            // A slope can be finite and non-negative and still leave the frame undefined: atan converges on
            // 90 degrees, so a large enough rise produces an axis that is vertical to within floating point.
            // Gated HERE, before any frame is built, so a typed number comes back as a diagnostic.
            if (!diagnostics.Any(d => d.Code == CantileverDiagnostics.ArmSlopeInvalid) &&
                !CantileverArmFrameResolver.IsRepresentableSlope(side, body.SlopeRisePer12))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSlopeFrameUndefined,
                    "La pendiente " + Format(body.SlopeRisePer12) +
                    " deja el brazo practicamente vertical: la proyeccion de +Z sobre su plano transversal se " +
                    "anula y el marco no queda definido."));
            }

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            // ---- 4. the column's punches, SELECTED not recreated ------------------------------------------
            var pattern = CantileverArmColumnConnectionPattern.Build(
                column.ColumnRegularPunches,
                mount.LowerColumnPunchIndex,
                mount.VerticalPunchCount,
                verticalEndOffset.Value,
                diagnostics);

            if (pattern == null)
            {
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            // ---- 5. contour and combined section ----------------------------------------------------------
            var geometry = _geometry.Get(section.SectionId, variant.Detail);
            ReportVisualAuthority(geometry, diagnostics);

            if (!CantileverArmBodyArrangementResolver.TouchesWithoutOverlap(
                    body.Arrangement, geometry, FitTolerance, out var gap, out var overlap))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmChannelsCannotTouch,
                    "Los dos canales no quedan en contacto exacto: separacion " + Format(gap) +
                    " in y traslape " + Format(overlap) + " in."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            var combined = CantileverArmBodyArrangementResolver.CombinedBounds(body.Arrangement, geometry);

            // ---- 6. geometry of the connection plane ------------------------------------------------------
            var columnOutline = column.ColumnBottomPlate.Outline;
            var columnMinX = columnOutline.Min(p => p.X);
            var columnMaxX = columnOutline.Max(p => p.X);
            var columnMinY = columnOutline.Min(p => p.Y);
            var columnMaxY = columnOutline.Max(p => p.Y);

            // The plate's INNER face lies on the column face of this side; its OUTER face is where the
            // profile's cut plane has its ORIGIN. With slope that is not flush — see the declared
            // approximation reported further down — so this is a datum, not a claim of no overlap.
            var innerY = side == CantileverArmSide.PositiveY ? columnMaxY : columnMinY;
            var outward = side == CantileverArmSide.PositiveY ? 1.0 : -1.0;
            var plateNormal = side == CantileverArmSide.PositiveY ? Vector3D.UnitY : -Vector3D.UnitY;
            var outerY = innerY + (outward * mount.Thickness);
            var midY = innerY + (outward * mount.Thickness / 2.0);

            var angle = CantileverArmFrameResolver.AngleRadians(body.SlopeRisePer12);
            var run = Math.Cos(angle);

            // The body sits so its LOWER envelope at the connection plane lands on the plate's bottom edge.
            // The section's depth axis makes an angle with the vertical, so its apparent vertical extent there
            // is scaled by cos(angle) — that factor is why the check below is not simply "plate >= depth".
            var bodyBottomAtConnection = pattern.PlateBottomZ;
            var bodyTopAtConnection = bodyBottomAtConnection + (run * combined.Height);

            if (pattern.PlateTopZ < bodyTopAtConnection - FitTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPlateTooShortForBody,
                    "La placa de conexion llega a z = " + Format(pattern.PlateTopZ) +
                    " in y el cuerpo alcanza z = " + Format(bodyTopAtConnection) +
                    " in en el plano de conexion. Aumenta VerticalPunchCount: mas troqueles extienden la placa " +
                    "hacia arriba. La placa NO se estira sin mas agujeros."));
                return CantileverArmAssembly.Blocked(owner, side, diagnostics);
            }

            var originZ = bodyBottomAtConnection - (run * combined.MinY);

            // ---- 7. members -------------------------------------------------------------------------------
            var placements = CantileverArmBodyArrangementResolver.Placements(body.Arrangement, geometry);
            var bodyId = CantileverPieceId.Create(owner, CantileverPieceTokens.ArmBody);
            var members = new List<CantileverStructuralMemberPlan>(placements.Count);

            // The BODY's own frame: centred on the column's transverse centre, starting on the plate's outer
            // face. Every member is this frame shifted along its X, and the plates are sized against it.
            var bodyOrigin = new Point3D(CantileverColumnBaseDatum.CentrePlaneX, outerY, originZ);
            var bodyFrame = CantileverArmFrameResolver.MemberFrame(
                side, variant.Orientation, angle, bodyOrigin, 0.0);

            for (var i = 0; i < placements.Count; i++)
            {
                var frame = CantileverArmFrameResolver.MemberFrame(
                    side, variant.Orientation, angle, bodyOrigin, placements[i].TransverseOffset);

                members.Add(CantileverStructuralMemberPlan.Create(
                    bodyId.At(i),
                    CantileverMemberRole.Arm,
                    owner,
                    PrismaticSectionInstance.Create(
                        section.SectionId, body.CutLength, frame, 0.0, placements[i].Mirrored)));
            }

            // A square-cut profile against a VERTICAL plate is not flush once there is slope, and the two
            // halves of that misfit are different magnitudes: the part of the section above its own origin
            // PENETRATES the plate, the part below leaves CLEARANCE. Reported separately and derived from both
            // ends of the section, because a section need not be symmetric about its origin — an angle is not.
            //
            // World Y of a start-face point at section height y is `outerY -/+ sin(angle) * y` depending on the
            // side, and the sign works out the same in magnitude either way.
            if (body.SlopeRisePer12 > FitTolerance)
            {
                var rise = Math.Sin(angle);
                var intrusion = rise * Math.Max(0.0, combined.MaxY);
                var clearance = rise * Math.Max(0.0, -combined.MinY);

                diagnostics.Add(CantileverDiagnostic.Info(
                    CantileverDiagnostics.ArmSquareCutAtSlopedPlate,
                    "El perfil va cortado a escuadra y la placa de conexion es un plano vertical, asi que la " +
                    "cara de arranque NO queda a ras: penetra hasta " + Format(intrusion) +
                    " in por su parte alta y deja hasta " + Format(clearance) +
                    " in de holgura por la baja. Es una aproximacion visual declarada; resolver un corte " +
                    "inclinado o una preparacion de extremo sigue fuera de alcance."));
            }

            var bodyPlan = CantileverArmBodyPlan.Create(
                body.Arrangement, side, members, body.SlopeRisePer12, angle, combined, bodyFrame);

            // ---- 8. plates and punches --------------------------------------------------------------------
            var mountingPlate = CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.ArmMountingPlate),
                CantileverPlateKind.ArmMounting,
                mount.Thickness,
                plateNormal,
                innerY * outward,
                RectangleXZ(columnMinX, columnMaxX, pattern.PlateBottomZ, pattern.PlateTopZ, innerY));

            var mountingPunches = BuildMountingPunches(owner, pattern, midY);

            var endPlate = BuildEndPlate(owner, end, combined, bodyFrame, body.CutLength);

            return CantileverArmAssembly.Create(
                owner, side, bodyPlan, mountingPlate, mountingPunches, endPlate, end.Mode, pattern, diagnostics);
        }

        // ---- validation -----------------------------------------------------------------------------------

        private static double? ValidateParameters(
            CantileverArmBodyDesign body,
            CantileverArmMountingPlateDesign mount,
            CantileverArmEndPlateDesign end,
            double diameter,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            RequirePositive(body.CutLength, "la longitud de corte del brazo", diagnostics);
            RequirePositive(mount.Thickness, "el espesor de la placa de conexion", diagnostics);

            if (!GeometryTolerance.IsFinite(body.SlopeRisePer12) || body.SlopeRisePer12 < 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSlopeInvalid,
                    "La pendiente debe ser un numero finito no negativo; se recibio " +
                    Format(body.SlopeRisePer12) + ". Cero es valido; bajar no."));
            }

            ValidateEndPlate(end, diagnostics);

            // The one parameter of I-37B with no approved default. Missing is REJECTED, never filled in.
            if (mount.VerticalEndOffset == null)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.RequiredParameterMissing,
                    "El diseno debe declarar el margen vertical de la placa de conexion: no existe un valor " +
                    "por omision aprobado."));
                return null;
            }

            var offset = mount.VerticalEndOffset.Value;

            if (!GeometryTolerance.IsFinite(offset) || offset < 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El margen vertical de la placa de conexion no puede ser negativo; se recibio " +
                    Format(offset) + "."));
                return null;
            }

            // Edge-to-CENTRE, so the hole only fits if the margin is at least its RADIUS. Same rule and same
            // code as I-37A.
            if (offset + FitTolerance < diameter / 2.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.EdgeOffsetBelowRadius,
                    "El margen vertical de la placa de conexion (" + Format(offset) +
                    " in) es menor que el radio del troquel (" + Format(diameter / 2.0) +
                    " in): el agujero no cabe."));
                return null;
            }

            return offset;
        }

        /// <summary>
        /// The end plate, validated EXHAUSTIVELY.
        ///
        /// One <c>switch</c> over every declared mode with a closed <c>default</c>, because the previous form
        /// — guarded cases with no default — let an undeclared value through silently, and
        /// <see cref="BuildEndPlate"/> then materialised it as a cap.
        ///
        /// The thickness rule differs per mode on purpose. Under <c>None</c> there is no plate, so the
        /// thickness is DORMANT DATA: a design that carried a zero there from an earlier edit must keep
        /// resolving, and blocking on it would reject a valid arm for a number nobody uses.
        /// </summary>
        private static void ValidateEndPlate(
            CantileverArmEndPlateDesign end, ICollection<CantileverDiagnostic> diagnostics)
        {
            switch (end.Mode)
            {
                case CantileverArmEndPlateMode.None:
                    // Thickness is dormant and deliberately NOT checked.
                    RequireFiniteZero(end.ExtraStopHeight, end.Mode, diagnostics);
                    break;

                case CantileverArmEndPlateMode.Cap:
                    RequirePositive(end.Thickness, "el espesor de la placa final", diagnostics);
                    RequireFiniteZero(end.ExtraStopHeight, end.Mode, diagnostics);
                    break;

                case CantileverArmEndPlateMode.Stop:
                    RequirePositive(end.Thickness, "el espesor de la placa final", diagnostics);

                    if (!GeometryTolerance.IsFinite(end.ExtraStopHeight) || end.ExtraStopHeight <= 0.0)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.ArmStopWithoutHeight,
                            "Un tope necesita una altura adicional finita y positiva; se recibio " +
                            Format(end.ExtraStopHeight) + "."));
                    }

                    break;

                default:
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.ArmEndPlateModeNotSupported,
                        "El modo de placa final '" + end.Mode +
                        "' no esta declarado. Anadir un modo exige escribir su regla."));
                    break;
            }
        }

        /// <summary>
        /// An extra stop height that must be finite AND zero.
        ///
        /// Finiteness is checked FIRST because <c>Math.Abs(NaN) &gt; tolerance</c> is false: a comparison
        /// alone would let a NaN through as if it were zero, and it would surface later as a plate with no
        /// corners.
        /// </summary>
        private static void RequireFiniteZero(
            double extraStopHeight,
            CantileverArmEndPlateMode mode,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (!GeometryTolerance.IsFinite(extraStopHeight) || Math.Abs(extraStopHeight) > FitTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmEndPlateHeightWithoutStop,
                    "El modo '" + mode + "' exige una altura adicional de tope finita e igual a cero; se recibio " +
                    Format(extraStopHeight) + "."));
            }
        }

        private static void ReportVisualAuthority(
            StructuralSectionGeometry geometry, ICollection<CantileverDiagnostic> diagnostics)
        {
            if (!geometry.IsVisualDerived)
            {
                return;
            }

            diagnostics.Add(CantileverDiagnostic.Warning(
                CantileverDiagnostics.SectionVisualDerived,
                "El contorno del brazo ('" + geometry.SectionId.Value +
                "') incorpora una convencion visual propia de RackCad (ADR-0023): es aproximado y no apto " +
                "para CNC ni fabricacion."));
        }

        // ---- pieces ---------------------------------------------------------------------------------------

        private static IReadOnlyList<CantileverPunchPlan> BuildMountingPunches(
            string owner, CantileverArmColumnConnectionPattern pattern, double y)
        {
            var id = CantileverPieceId.Create(owner, CantileverPieceTokens.ArmMountingPunch);
            var list = new List<CantileverPunchPlan>(pattern.SelectedDatums.Count);
            var index = 0;

            foreach (var datum in pattern.SelectedDatums)
            {
                // The datum is the COLUMN's, verbatim. Only the Y of the centre differs, because the plate is
                // a different surface — which is exactly what the datum excludes.
                list.Add(new CantileverPunchPlan(
                    id.At(index++),
                    CantileverPunchSurface.ArmMountingPlate,
                    new Point3D(datum.U, y, datum.V),
                    datum));
            }

            return list;
        }

        private static CantileverPlatePlan BuildEndPlate(
            string owner,
            CantileverArmEndPlateDesign end,
            Bounds2D combined,
            LocalFrame3D bodyFrame,
            double cutLength)
        {
            // Exhaustive over every declared mode, with a closed default. The previous form — an `if None`
            // followed by a ternary on Stop — turned ANY other value into a cap, so an enum member added
            // tomorrow would have been drawn instead of rejected.
            double extra;

            switch (end.Mode)
            {
                case CantileverArmEndPlateMode.None:
                    return null;
                case CantileverArmEndPlateMode.Cap:
                    extra = 0.0;
                    break;
                case CantileverArmEndPlateMode.Stop:
                    extra = end.ExtraStopHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(end), end.Mode,
                        "El modo de placa final '" + end.Mode + "' no tiene regla de construccion.");
            }

            // The plate is perpendicular to the SLOPED axis, and its height direction is the arm's own depth
            // axis — world +Z projected onto the plate's plane. That is what makes a stop grow visually
            // upwards without anybody choosing a direction (ADR-0025, D7).
            //
            // The transverse axis is the frame's own X, and `combined` is expressed against the BODY's centre
            // origin — not against one profile's shifted origin — so the plate lands centred on the body.
            var axis = bodyFrame.AxisZ;
            var across = bodyFrame.AxisX;
            var up = bodyFrame.AxisY;
            var centre = bodyFrame.Origin + (axis * cutLength);

            var top = combined.MaxY + extra;

            var corners = new[]
            {
                Corner(centre, across, up, combined.MinX, combined.MinY),
                Corner(centre, across, up, combined.MaxX, combined.MinY),
                Corner(centre, across, up, combined.MaxX, top),
                Corner(centre, across, up, combined.MinX, top)
            };

            return CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.ArmEndPlate),
                CantileverPlateKind.ArmEnd,
                end.Thickness,
                axis,
                centre.X * axis.X + centre.Y * axis.Y + centre.Z * axis.Z,
                corners);
        }

        private static Point3D Corner(Point3D origin, Vector3D across, Vector3D up, double x, double y) =>
            origin + (across * x) + (up * y);

        private static IReadOnlyList<Point3D> RectangleXZ(
            double minX, double maxX, double minZ, double maxZ, double y) =>
            new[]
            {
                new Point3D(minX, y, minZ),
                new Point3D(maxX, y, minZ),
                new Point3D(maxX, y, maxZ),
                new Point3D(minX, y, maxZ)
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

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
