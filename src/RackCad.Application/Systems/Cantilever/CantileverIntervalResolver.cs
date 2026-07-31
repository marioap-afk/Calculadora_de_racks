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
    /// Where the bracing of an interval attaches: the two column faces it spans and the plane it lies in.
    ///
    /// It is measured ONCE, from the resolved columns, and handed to the resolver. The alternative — letting
    /// the interval resolver reach into two station assemblies and re-derive the faces — would put the same
    /// extent computation in the plate rule, the separator rule and the brace rule, and a line whose columns
    /// changed section would then need three edits instead of one.
    /// </summary>
    public sealed class CantileverBracingAttachment
    {
        public CantileverBracingAttachment(
            double leftColumnFaceX,
            double rightColumnFaceX,
            double bracingFaceY,
            double outwardSign)
        {
            GeometryTolerance.RequireFinite(leftColumnFaceX, nameof(leftColumnFaceX));
            GeometryTolerance.RequireFinite(rightColumnFaceX, nameof(rightColumnFaceX));
            GeometryTolerance.RequireFinite(bracingFaceY, nameof(bracingFaceY));

            if (outwardSign != 1.0 && outwardSign != -1.0)
            {
                throw new ArgumentException(
                    "El sentido hacia afuera de la cara de arriostramiento es +1 o -1.", nameof(outwardSign));
            }

            LeftColumnFaceX = leftColumnFaceX;
            RightColumnFaceX = rightColumnFaceX;
            BracingFaceY = bracingFaceY;
            OutwardSign = outwardSign;
        }

        /// <summary>World X of the +X face of the column at the interval's LEFT end.</summary>
        public double LeftColumnFaceX { get; }

        /// <summary>World X of the −X face of the column at the interval's RIGHT end.</summary>
        public double RightColumnFaceX { get; }

        /// <summary>World Y of the column face the bracing is welded to.</summary>
        public double BracingFaceY { get; }

        /// <summary>+1 when the bracing grows towards +Y from that face, −1 when towards −Y.</summary>
        public double OutwardSign { get; }

        /// <summary>The clear distance between the two columns, face to face.</summary>
        public double ClearSpanX => RightColumnFaceX - LeftColumnFaceX;

        /// <summary>The world Y a plate of the given thickness reaches, measured from the column face.</summary>
        public double FaceAt(double distanceFromColumn) => BracingFaceY + (OutwardSign * distanceFromColumn);

        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture,
            "x=[{0:0.####},{1:0.####}] y={2:0.####} out={3:+0;-0}",
            LeftColumnFaceX, RightColumnFaceX, BracingFaceY, OutwardSign);
    }

    /// <summary>
    /// THE authority that turns one interval's bracing intention into geometry: its column plates, its
    /// separators, its braced panels and — when the braces are cold rolled — their adapters.
    ///
    /// The order of derivation is the point of this class, and it runs one way only:
    ///
    /// <list type="number">
    ///   <item>the layout says WHICH elevations carry a separator;</item>
    ///   <item>each elevation gets two column plates, whose holes are placed at the separator's own edge
    ///         distance from the column face;</item>
    ///   <item>the separator's cut length is DERIVED from the distance between those two holes;</item>
    ///   <item>the separator's four punches are placed on its own length, and the two outer ones are then
    ///         CHECKED against the plate datums.</item>
    /// </list>
    ///
    /// Running it the other way — cutting the separator to the clear span and then putting plates where its
    /// ends happen to land — would be the same numbers today and two authorities tomorrow (ADR-0027, D5).
    /// </summary>
    public static class CantileverIntervalResolver
    {
        /// <summary>
        /// How far the rod hole of an adapter sits from its separator hole, along the brace's axis.
        ///
        /// It is HALF the adapter's cut length, because the two holes are each centred on their own square
        /// face and those faces are perpendicular: the second hole's centre is one half-cut along the leg. It
        /// is derived and not a constant of its own, so an adapter cut differently moves its rod hole with it.
        /// </summary>
        public static double RodHoleAxialOffset(double adapterCutLength) => adapterCutLength / 2.0;

        public static CantileverIntervalAssembly Resolve(
            int intervalIndex,
            CantileverBracingDesign bracing,
            CantileverBracingAttachment attachment,
            double columnHeight,
            bool heightIsManual,
            StructuralSectionCatalog catalog,
            StructuralSectionGeometryFactory geometryFactory)
        {
            if (intervalIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalIndex));
            }

            if (bracing == null)
            {
                throw new ArgumentNullException(nameof(bracing));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (geometryFactory == null)
            {
                throw new ArgumentNullException(nameof(geometryFactory));
            }

            var diagnostics = new List<CantileverDiagnostic>();

            var layout = CantileverBracingLayoutResolver.Resolve(bracing, columnHeight, heightIsManual);
            diagnostics.AddRange(layout.Diagnostics);

            var plates = new List<CantileverSeparatorColumnPlatePlan>();
            var separators = new List<CantileverSeparatorPlan>();
            var panels = new List<CantileverBracedPanelPlan>();

            if (layout.IsBlocked)
            {
                // No elevations means nothing to place. Carrying on would produce a separator of a length
                // derived from a layout that already said it does not fit.
                return new CantileverIntervalAssembly(
                    intervalIndex, intervalIndex, intervalIndex + 1, attachment.ClearSpanX,
                    layout.SeparatorElevations, plates, separators, panels, layout, diagnostics);
            }

            var separatorSection = CantileverSectionResolver.Resolve(
                catalog, bracing.SeparatorSectionId, CantileverMemberRole.Separator, diagnostics);

            var braceSection = ResolveBraceSection(bracing, catalog, diagnostics);

            if (!separatorSection.IsResolved || (braceSection != null && !braceSection.IsResolved))
            {
                return new CantileverIntervalAssembly(
                    intervalIndex, intervalIndex, intervalIndex + 1, attachment.ClearSpanX,
                    layout.SeparatorElevations, plates, separators, panels, layout, diagnostics);
            }

            var separatorGeometry = geometryFactory.Get(
                separatorSection.Section, SectionDetailLevel.Tabulated);

            var plateThickness = CantileverLineDefaults.SeparatorColumnPlateThickness;
            var edge = CantileverLineDefaults.SeparatorColumnPunchEdgeDistance;
            var punchDiameter = CantileverLineDefaults.SeparatorPunchDiameter;
            var braceOffset = CantileverLineDefaults.SeparatorBracePunchOffset;

            // The bracing plane: the plate laps the column face, the separator laps the plate.
            var plateFarY = attachment.FaceAt(plateThickness);

            var leftHoleX = attachment.LeftColumnFaceX + edge;
            var rightHoleX = attachment.RightColumnFaceX - edge;
            var cutLength = (rightHoleX - leftHoleX) + (2.0 * edge);

            var minimumCut = (2.0 * edge) + (2.0 * braceOffset);

            if (cutLength <= minimumCut)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SeparatorTooShortForItsPunches,
                    "El separador mide " + cutLength.ToString("0.###", CultureInfo.InvariantCulture) +
                    " pulgadas y sus cuatro troqueles necesitan mas de " +
                    minimumCut.ToString("0.###", CultureInfo.InvariantCulture) +
                    " pulgadas; acerca las estaciones menos o reduce el desplazamiento del troquel de tensor."));

                return new CantileverIntervalAssembly(
                    intervalIndex, intervalIndex, intervalIndex + 1, attachment.ClearSpanX,
                    layout.SeparatorElevations, plates, separators, panels, layout, diagnostics);
            }

            var owner = CantileverPieceTokens.IntervalOwnerOf(intervalIndex);

            for (var k = 0; k < layout.SeparatorElevations.Count; k++)
            {
                var z = layout.SeparatorElevations[k];

                var leftPlate = BuildColumnPlate(
                    intervalIndex, CantileverIntervalSide.Left, k, leftHoleX, z,
                    attachment, plateThickness, punchDiameter);

                var rightPlate = BuildColumnPlate(
                    intervalIndex, CantileverIntervalSide.Right, k, rightHoleX, z,
                    attachment, plateThickness, punchDiameter);

                plates.Add(leftPlate);
                plates.Add(rightPlate);

                var separator = BuildSeparator(
                    owner, intervalIndex, k, z, cutLength, leftHoleX - edge, plateFarY,
                    edge, braceOffset, punchDiameter, separatorSection.Section.SectionId, separatorGeometry);

                separators.Add(separator);

                RequireDatumsMatch(separator, leftPlate, rightPlate, diagnostics);
            }

            for (var p = 0; p < layout.BracedPanels.Count; p++)
            {
                var lowerIndex = layout.LowerSeparatorIndexOf(p);
                var lower = separators[lowerIndex];
                var upper = separators[lowerIndex + 1];

                var braceA = BuildBrace(
                    owner, intervalIndex, p, 'A',
                    lower.LeftBracePunch.Centre, upper.RightBracePunch.Centre,
                    bracing, braceSection, geometryFactory, diagnostics);

                var braceB = BuildBrace(
                    owner, intervalIndex, p, 'B',
                    lower.RightBracePunch.Centre, upper.LeftBracePunch.Centre,
                    bracing, braceSection, geometryFactory, diagnostics);

                if (braceA == null || braceB == null)
                {
                    // A brace that could not be built is already a blocking diagnostic. Half a panel is not a
                    // thing this system knows how to draw, so the panel is dropped rather than emitted lame.
                    break;
                }

                panels.Add(new CantileverBracedPanelPlan(intervalIndex, p, lower, upper, braceA, braceB));
            }

            return new CantileverIntervalAssembly(
                intervalIndex, intervalIndex, intervalIndex + 1, attachment.ClearSpanX,
                layout.SeparatorElevations, plates, separators, panels, layout, diagnostics);
        }

        /// <summary>
        /// The brace's section, or null when its body is not a catalogued section.
        ///
        /// A cold-rolled rod has no catalogue row — it is a round bar, not a standard shape — so asking the
        /// catalogue for one would either fail or force a fictional entry (ADR-0027, D7).
        /// </summary>
        private static CantileverSectionResolution ResolveBraceSection(
            CantileverBracingDesign bracing,
            StructuralSectionCatalog catalog,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            switch (bracing.BraceKind)
            {
                case CantileverBraceBodyKind.ColdRolledRound:
                    if (bracing.ColdRolled == null || bracing.ColdRolled.Diameter <= 0.0)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.ColdRolledDiameterNotPositive,
                            "El diametro del tensor cold rolled debe ser positivo."));
                    }

                    return null;

                case CantileverBraceBodyKind.StructuralSection:
                    if (string.IsNullOrWhiteSpace(bracing.BraceSectionId))
                    {
                        // Distinct from SectionIdMissing on purpose: the design did not merely leave a field
                        // empty, it declared a structural brace and then did not say which section. The
                        // remedy is a different one — pick a section, or pick cold rolled.
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.BraceSectionMissing,
                            "El arriostramiento declara tensores de perfil estructural pero no dice cual seccion."));
                        return CantileverSectionResolution.Failed(default, diagnostics.Last());
                    }

                    return CantileverSectionResolver.Resolve(
                        catalog, bracing.BraceSectionId, CantileverMemberRole.Brace, diagnostics);

                default:
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.BraceBodyKindNotSupported,
                        "El tipo de cuerpo de tensor '" + bracing.BraceKind + "' no tiene regla."));
                    return CantileverSectionResolution.Failed(default, diagnostics.Last());
            }
        }

        private static CantileverSeparatorColumnPlatePlan BuildColumnPlate(
            int intervalIndex,
            CantileverIntervalSide side,
            int separatorIndex,
            double holeX,
            double z,
            CantileverBracingAttachment attachment,
            double thickness,
            double punchDiameter)
        {
            var width = CantileverLineDefaults.SeparatorColumnPlateWidth;
            var height = CantileverLineDefaults.SeparatorColumnPlateHeight;

            var halfHeight = height / 2.0;

            // LA PLACA APOYA EN LA CARA DEL ALMA Y CRECE HACIA SU TRAMO. Corrección del dueño en la ronda 3.
            //
            // Antes se centraba en su agujero, que es el datum, y de ahí salía una placa de 3 in cuyo agujero
            // está a 1.25 in de la cara: sobresalía 0.25 in por delante de la cara… del ALMA, que mide 0.435
            // in de canto. Es decir, la cruzaba entera y asomaba 1.28 in por el otro lado, invadiendo el tramo
            // vecino. Eso es lo que el dueño reportaba como «atraviesa el alma».
            //
            // El DATUM NO SE MUEVE: el agujero sigue a `edge` de la cara, y la longitud del separador se sigue
            // midiendo entre los dos agujeros. Lo que cambia es de dónde cuelga el rectángulo — de la cara y
            // no del agujero—, así que el agujero deja de estar centrado en la placa y queda a `edge` de su
            // borde interior, que es exactamente lo que una distancia a la orilla significa.
            var faceX = side == CantileverIntervalSide.Left
                ? attachment.LeftColumnFaceX
                : attachment.RightColumnFaceX;

            // Hacia dónde crece: el tramo queda a +X de la columna izquierda y a −X de la derecha.
            var inward = side == CantileverIntervalSide.Left ? 1.0 : -1.0;

            var nearX = faceX;
            var farX = faceX + (inward * width);

            var minX = Math.Min(nearX, farX);
            var maxX = Math.Max(nearX, farX);

            var faceY = attachment.BracingFaceY;
            var outline = new[]
            {
                new Point3D(minX, faceY, z - halfHeight),
                new Point3D(maxX, faceY, z - halfHeight),
                new Point3D(maxX, faceY, z + halfHeight),
                new Point3D(minX, faceY, z + halfHeight)
            };

            var normal = new Vector3D(0.0, attachment.OutwardSign, 0.0);
            var endOwner = CantileverPieceTokens.IntervalEndOwner(intervalIndex, side);

            var plate = CantileverPlatePlan.Create(
                CantileverPieceId.Create(endOwner, CantileverPieceTokens.SeparatorColumnPlate).At(separatorIndex),
                CantileverPlateKind.SeparatorColumn,
                thickness,
                normal,
                faceY * attachment.OutwardSign,
                outline);

            var punch = new CantileverPunchPlan(
                CantileverPieceId.Create(endOwner, CantileverPieceTokens.SeparatorColumnPlatePunch).At(separatorIndex),
                CantileverPunchSurface.SeparatorColumnPlate,
                new Point3D(holeX, attachment.FaceAt(thickness / 2.0), z),
                new CantileverPunchDatum(CantileverPunchAxis.AlongY, holeX, z, punchDiameter));

            return new CantileverSeparatorColumnPlatePlan(intervalIndex, side, separatorIndex, plate, punch);
        }

        private static CantileverSeparatorPlan BuildSeparator(
            string owner,
            int intervalIndex,
            int separatorIndex,
            double z,
            double cutLength,
            double startX,
            double plateFarY,
            double edge,
            double braceOffset,
            double punchDiameter,
            StructuralSectionId sectionId,
            StructuralSectionGeometry geometry)
        {
            var placement = CantileverLineFrameResolver.Separator(startX, plateFarY, z, geometry);

            var placed = PrismaticSectionInstance.Create(
                sectionId, cutLength, placement.Frame, 0.0, placement.Mirrored);

            var member = CantileverStructuralMemberPlan.Create(
                CantileverPieceId.Create(owner, CantileverPieceTokens.Separator).At(separatorIndex),
                CantileverMemberRole.Separator,
                owner,
                placed);

            var leftColumn = SeparatorPunch(
                owner, CantileverPieceTokens.SeparatorEndPunch, separatorIndex, 0,
                startX + edge, z, plateFarY, punchDiameter);

            var leftBrace = SeparatorPunch(
                owner, CantileverPieceTokens.SeparatorBracePunch, separatorIndex, 0,
                startX + edge + braceOffset, z, plateFarY, punchDiameter);

            var rightBrace = SeparatorPunch(
                owner, CantileverPieceTokens.SeparatorBracePunch, separatorIndex, 1,
                startX + cutLength - edge - braceOffset, z, plateFarY, punchDiameter);

            var rightColumn = SeparatorPunch(
                owner, CantileverPieceTokens.SeparatorEndPunch, separatorIndex, 1,
                startX + cutLength - edge, z, plateFarY, punchDiameter);

            var punches = new[] { leftColumn, leftBrace, rightBrace, rightColumn };

            return new CantileverSeparatorPlan(
                intervalIndex, separatorIndex, z, member, punches,
                leftColumn, leftBrace, rightBrace, rightColumn);
        }

        private static CantileverPunchPlan SeparatorPunch(
            string owner, string token, int separatorIndex, int endIndex,
            double x, double z, double webBackY, double diameter)
        {
            var id = CantileverPieceId
                .Create(owner, token)
                .At(separatorIndex)
                .At(endIndex);

            // Recorded on the BACK OF THE WEB, which is a Bounds-derived plane. Mid-thickness would need the
            // web's own thickness, and reading tw is exactly what ADR-0024 D5 forbids.
            return new CantileverPunchPlan(
                id,
                CantileverPunchSurface.Separator,
                new Point3D(x, webBackY, z),
                new CantileverPunchDatum(CantileverPunchAxis.AlongY, x, z, diameter));
        }

        private static CantileverBracePlan BuildBrace(
            string owner,
            int intervalIndex,
            int panelIndex,
            char diagonal,
            Point3D lowerBolt,
            Point3D upperBolt,
            CantileverBracingDesign bracing,
            CantileverSectionResolution braceSection,
            StructuralSectionGeometryFactory geometryFactory,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            var axis = (upperBolt - lowerBolt);
            var boltToBolt = axis.Length;

            if (boltToBolt <= GeometryTolerance.Length)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.BracingDoesNotFitTheColumn,
                    "Los dos extremos del tensor del panel " + panelIndex + " coinciden."));
                return null;
            }

            var direction = axis.Normalized();

            switch (bracing.BraceKind)
            {
                case CantileverBraceBodyKind.ColdRolledRound:
                {
                    var cut = CantileverLineDefaults.AdapterCutLength;
                    var inset = RodHoleAxialOffset(cut);
                    var rodLower = lowerBolt + (direction * inset);
                    var rodUpper = upperBolt + (direction * -inset);

                    if ((rodUpper - rodLower).Length <= GeometryTolerance.Length)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.BracingDoesNotFitTheColumn,
                            "El panel " + panelIndex + " es mas corto que los dos adaptadores de su tensor."));
                        return null;
                    }

                    var adapters = new[]
                    {
                        BuildAdapter(owner, panelIndex, diagonal, 0, lowerBolt, rodLower, bracing),
                        BuildAdapter(owner, panelIndex, diagonal, 1, upperBolt, rodUpper, bracing)
                    };

                    return new CantileverBracePlan(
                        intervalIndex, panelIndex, diagonal, CantileverBraceBodyKind.ColdRolledRound,
                        rodLower, rodUpper, null, Array.Empty<CantileverPunchPlan>(),
                        bracing.ColdRolled.Diameter, adapters);
                }

                case CantileverBraceBodyKind.StructuralSection:
                {
                    var edge = CantileverLineDefaults.BracePunchEdgeDistance;
                    var cutLength = boltToBolt + (2.0 * edge);
                    var start = lowerBolt + (direction * -edge);

                    var geometry = geometryFactory.Get(braceSection.Section, SectionDetailLevel.Tabulated);
                    var placement = CantileverLineFrameResolver.Brace(start, direction, geometry);

                    var placed = PrismaticSectionInstance.Create(
                        braceSection.Section.SectionId, cutLength, placement.Frame, 0.0, placement.Mirrored);

                    var member = CantileverStructuralMemberPlan.Create(
                        CantileverPieceId.Create(owner, CantileverPieceTokens.BraceToken(panelIndex, diagonal)),
                        CantileverMemberRole.Brace,
                        owner,
                        placed);

                    var punches = new[]
                    {
                        BracePunch(owner, panelIndex, diagonal, 0, lowerBolt, bracing),
                        BracePunch(owner, panelIndex, diagonal, 1, upperBolt, bracing)
                    };

                    return new CantileverBracePlan(
                        intervalIndex, panelIndex, diagonal, CantileverBraceBodyKind.StructuralSection,
                        lowerBolt, upperBolt, member, punches, double.NaN,
                        Array.Empty<CantileverColdRolledAdapterPlan>());
                }

                default:
                    // Unreachable while ResolveBraceSection gates the kind; kept so a value added there
                    // without a rule here fails loudly instead of drawing nothing.
                    throw new InvalidOperationException(
                        "El tipo de cuerpo de tensor '" + bracing.BraceKind + "' no tiene regla de colocacion.");
            }
        }

        private static CantileverPunchPlan BracePunch(
            string owner, int panelIndex, char diagonal, int endIndex, Point3D bolt,
            CantileverBracingDesign bracing)
        {
            // The DIAGONAL is in the id. Without it the two braces of one panel produce the same punch ids, and
            // anything that keys the line's pieces loses half of them.
            var id = CantileverPieceId
                .Create(owner, CantileverPieceTokens.BracePunch + diagonal)
                .At(panelIndex)
                .At(endIndex);

            return new CantileverPunchPlan(
                id,
                CantileverPunchSurface.Brace,
                bolt,
                new CantileverPunchDatum(
                    CantileverPunchAxis.AlongY, bolt.X, bolt.Z,
                    CantileverLineDefaults.SeparatorPunchDiameter));
        }

        private static CantileverColdRolledAdapterPlan BuildAdapter(
            string owner,
            int panelIndex,
            char diagonal,
            int endIndex,
            Point3D bolt,
            Point3D rodHole,
            CantileverBracingDesign bracing)
        {
            var id = CantileverPieceId.Create(
                owner, CantileverPieceTokens.AdapterToken(panelIndex, diagonal, endIndex));

            var punch = new CantileverPunchPlan(
                CantileverPieceId.Create(owner, CantileverPieceTokens.ColdRolledAdapterPunch + diagonal)
                    .At(panelIndex)
                    .At(endIndex),
                CantileverPunchSurface.ColdRolledAdapter,
                bolt,
                new CantileverPunchDatum(
                    CantileverPunchAxis.AlongY, bolt.X, bolt.Z,
                    CantileverLineDefaults.SeparatorPunchDiameter));

            return new CantileverColdRolledAdapterPlan(
                id,
                bolt,
                StructuralSectionId.Parse(CantileverLineDefaults.AdapterAngleSectionId),
                CantileverLineDefaults.AdapterAngleLeg,
                CantileverLineDefaults.AdapterCutLength,
                CantileverLineDefaults.AdapterAngleThickness,
                punch,
                rodHole,
                CantileverLineDefaults.SeparatorPunchDiameter,
                CantileverLineDefaults.GussetsPerAdapter,
                CantileverLineDefaults.GussetGaugeNumber);
        }

        /// <summary>
        /// Checks that the separator's two end punches really are the plates' holes.
        ///
        /// It is a full pass over what was just built, not an assumption. The cut length was derived from the
        /// plate datums a few lines above, so under every intended input this check passes — which is the
        /// point: the day somebody adds an offset to one of the two rules, this is what says so, instead of a
        /// drawing where the bolt misses the plate by a quarter of an inch.
        /// </summary>
        private static void RequireDatumsMatch(
            CantileverSeparatorPlan separator,
            CantileverSeparatorColumnPlatePlan leftPlate,
            CantileverSeparatorColumnPlatePlan rightPlate,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            Check(separator.LeftColumnPunch, leftPlate, "izquierdo", diagnostics);
            Check(separator.RightColumnPunch, rightPlate, "derecho", diagnostics);
        }

        private static void Check(
            CantileverPunchPlan punch,
            CantileverSeparatorColumnPlatePlan plate,
            string which,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (punch.Datum.ApproxEquals(plate.Punch.Datum))
            {
                return;
            }

            diagnostics.Add(CantileverDiagnostic.Blocking(
                CantileverDiagnostics.BracingDatumMismatch,
                "El troquel " + which + " del separador " + separatorLabel(punch) +
                " no coincide con el de su placa de columna: " + punch.Datum + " contra " + plate.Punch.Datum + "."));

            string separatorLabel(CantileverPunchPlan p) => p.Id.Value;
        }
    }
}
