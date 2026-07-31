using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The base pieces of ONE side of a station: the profile, its three plates, its gusset and the punches of
    /// its rear plate.
    ///
    /// It is a side and not a sub-assembly: there is no column here, and no column bottom plate. That is the
    /// whole point of the type — a double station has two of THESE and one of those (ADR-0026, D1).
    /// </summary>
    public sealed class CantileverStationBaseSide
    {
        internal CantileverStationBaseSide(
            CantileverArmSide side,
            CantileverStructuralMemberPlan member,
            CantileverPlatePlan frontPlate,
            CantileverPlatePlan rearPlate,
            CantileverGussetPlan gusset,
            IReadOnlyList<CantileverPunchPlan> rearPlatePunches)
        {
            Side = side;
            Member = member;
            FrontPlate = frontPlate;
            RearPlate = rearPlate;
            Gusset = gusset;
            RearPlatePunches = rearPlatePunches;
        }

        public CantileverArmSide Side { get; }

        public CantileverStructuralMemberPlan Member { get; }

        public CantileverPlatePlan FrontPlate { get; }

        public CantileverPlatePlan RearPlate { get; }

        public CantileverGussetPlan Gusset { get; }

        /// <summary>
        /// The punches of this side's rear plate.
        ///
        /// In a double station the two sides' punches share their DATUMS, and that is correct rather than a
        /// collision: one bolt passes through the negative plate, the column and the positive plate, so the
        /// three holes are one logical hole. The datum excludes the coordinate along its own axis precisely so
        /// that this comes out true instead of needing a subtraction (ADR-0024, D6).
        /// </summary>
        public IReadOnlyList<CantileverPunchPlan> RearPlatePunches { get; }

        public IReadOnlyList<CantileverPlatePlan> Plates => new[] { FrontPlate, RearPlate };

        /// <summary>
        /// The extent of this side.
        ///
        /// The member contributes its two END POINTS and not a section envelope: the plates already span the
        /// section's own width and depth, so the axis is the only thing they do not cover. Same convention
        /// I-37A uses for its own envelope.
        /// </summary>
        public CantileverEnvelope3D Envelope() =>
            FrontPlate.Envelope()
                .Union(RearPlate.Envelope())
                .Union(Gusset.Envelope())
                .Union(CantileverEnvelope3D.FromPoints(new[] { Member.Start, Member.End }));

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "side={0};base={1}@{2:0.######};plates={3:0.######},{4:0.######};gusset={5:0.######};pch={6}",
            Side,
            Member.SectionId.Value,
            Member.GeometricLength,
            FrontPlate.Thickness,
            RearPlate.Thickness,
            Gusset.Thickness,
            RearPlatePunches.Count);

        public override string ToString() => "BaseSide " + Signature();
    }

    /// <summary>
    /// THE authority that turns the base half of a resolved I-37A sub-assembly into one SIDE of a station.
    ///
    /// The positive side is I-37A's own geometry, re-owned. The negative side is that same geometry
    /// MIRRORED about the column's MID-DEPTH plane — not a second resolve, and not a copy of I-37A's
    /// formulas.
    ///
    /// <para>El plano era <c>y = 0</c> hasta la ronda 3 de I-37D, y estaba mal: <c>y = 0</c> es la CARA de
    /// conexión de la columna y no su plano medio, así que la base negativa aterrizaba encima de la columna.
    /// El plano se MIDE ahora de la huella de la propia columna —la placa inferior, construida con
    /// <c>columnMinY..columnMaxY</c>— y no de una cota tabulada, que es lo que ADR-0024 D5 exige.</para>
    ///
    /// Why a mirror and not a second <c>Resolve</c>: resolving twice would produce a second column and a
    /// second column bottom plate, which then have to be discarded by hand. A discarded piece is a piece
    /// somebody eventually forgets to discard, and the BOM would ask for two columns (ADR-0026, D1). It also
    /// means "the same base on both sides" is true by construction: there is one configuration and one
    /// resolve behind both.
    ///
    /// Nothing here re-derives a length, a thickness, an offset or a punch position. Every number is read
    /// from the pieces I-37A already produced, which is what keeps ADR-0024's authorities intact.
    /// </summary>
    public static class CantileverStationBaseSideResolver
    {
        /// <summary>Whether this side has a composition rule.</summary>
        public static bool IsSupported(CantileverArmSide side) =>
            side == CantileverArmSide.PositiveY || side == CantileverArmSide.NegativeY;

        /// <summary>
        /// Composes one side from a resolved column–base sub-assembly.
        /// </summary>
        /// <param name="assembly">The I-37A result. Its base half is the source of every dimension.</param>
        /// <param name="side">Which side to produce. The positive one is composed; the negative one mirrored.</param>
        public static CantileverStationBaseSide Compose(
            CantileverColumnBaseAssembly assembly, CantileverArmSide side)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.IsBlocked)
            {
                throw new ArgumentException(
                    "No se puede componer un lado a partir de un subensamble bloqueado.", nameof(assembly));
            }

            var owner = CantileverPieceTokens.StationBaseOwner(side);
            var planeY = MirrorPlaneY(assembly);

            switch (side)
            {
                case CantileverArmSide.PositiveY:
                    // I-37A's geometry IS the positive side. Only the ids change, because a station may hold
                    // two of these and two pieces with one id is a BOM that counts one of them.
                    return new CantileverStationBaseSide(
                        side,
                        ReOwn(assembly.Base, owner, CantileverPieceTokens.Base),
                        ReOwn(assembly.BaseFrontPlate, owner, CantileverPieceTokens.BaseFrontPlate),
                        ReOwn(assembly.BaseRearPlate, owner, CantileverPieceTokens.BaseRearPlate),
                        ReOwn(assembly.Gusset, owner, CantileverPieceTokens.Gusset),
                        ReOwn(assembly.RearPlatePunches, owner, CantileverPieceTokens.RearPlatePunch));

                case CantileverArmSide.NegativeY:
                    return new CantileverStationBaseSide(
                        side,
                        Mirror(assembly.Base, owner, CantileverPieceTokens.Base, planeY),
                        Mirror(assembly.BaseFrontPlate, owner, CantileverPieceTokens.BaseFrontPlate, planeY),
                        Mirror(assembly.BaseRearPlate, owner, CantileverPieceTokens.BaseRearPlate, planeY),
                        Mirror(assembly.Gusset, owner, CantileverPieceTokens.Gusset, planeY),
                        Mirror(assembly.RearPlatePunches, owner, CantileverPieceTokens.RearPlatePunch, planeY));

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(side), side,
                        "El lado '" + side + "' no tiene regla de composicion de base.");
            }
        }

        // ---- the mirror -----------------------------------------------------------------------------------

        /// <summary>
        /// El plano medio de la columna, MEDIDO de su propia huella.
        ///
        /// La placa inferior se construye con el rango <c>columnMinY..columnMaxY</c> del contorno resuelto de
        /// la columna, así que su punto medio en Y ES el plano medio de la columna. Leerlo de ahí en vez de
        /// tomar <c>d</c> del catálogo es lo que ADR-0024 D5 pide: la geometría manda sobre la tabla, y una
        /// columna girada o espejada reporta el plano que de verdad ocupa.
        /// </summary>
        public static double MirrorPlaneY(CantileverColumnBaseAssembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return MirrorPlaneY(assembly.ColumnBottomPlate);
        }

        /// <summary>
        /// El mismo plano, medido de la placa inferior suelta.
        ///
        /// La sobrecarga existe porque la estación RE-POSEE esa placa con otro id y ya no tiene a mano el
        /// subensamble de I-37A del que salió. Las dos leen el MISMO contorno, así que no hay dos reglas: hay
        /// una regla y dos formas de llegar a su entrada.
        /// </summary>
        public static double MirrorPlaneY(CantileverPlatePlan columnBottomPlate)
        {
            if (columnBottomPlate == null)
            {
                throw new ArgumentNullException(nameof(columnBottomPlate));
            }

            var outline = columnBottomPlate.Outline;
            var span = outline.Min(p => p.Y) + outline.Max(p => p.Y);

            return span / 2.0;
        }

        /// <summary>
        /// Reflects a point through the column's mid-depth plane. DELEGATES to the frame authority, so there
        /// is one reflection in the system and not two that could disagree about which plane.
        /// </summary>
        public static Point3D MirrorY(Point3D p, double planeY) =>
            CantileverColumnBaseFrameResolver.Reflect(p, planeY);

        /// <summary>Reflects a direction through the same plane. Also delegated.</summary>
        public static Vector3D MirrorY(Vector3D v) => CantileverColumnBaseFrameResolver.Reflect(v);

        private static CantileverStructuralMemberPlan ReOwn(
            CantileverStructuralMemberPlan member, string owner, string token) =>
            CantileverStructuralMemberPlan.Create(
                CantileverPieceId.Create(owner, token), member.Role, owner, member.Placement);

        private static CantileverStructuralMemberPlan Mirror(
            CantileverStructuralMemberPlan member, string owner, string token, double planeY)
        {
            var placement = member.Placement;
            var frame = placement.Frame;

            // The frame comes from the FRAME AUTHORITY, not from here: building a placement frame is what
            // that type is for, and a source guard enforces it. What this method does is pair the reflected
            // frame with the flipped Mirrored flag — the reflection is improper, so the handedness has to
            // live in the section instance. Taking the frame and forgetting the flag would silently
            // un-mirror an asymmetric section.
            var mirrored = CantileverColumnBaseFrameResolver.MirrorAboutCentralPlane(frame, planeY);

            return CantileverStructuralMemberPlan.Create(
                CantileverPieceId.Create(owner, token),
                member.Role,
                owner,
                PrismaticSectionInstance.Create(
                    placement.SectionId,
                    placement.Length,
                    mirrored,
                    placement.RotationRadians,
                    !placement.Mirrored));
        }

        private static CantileverPlatePlan ReOwn(CantileverPlatePlan plate, string owner, string token) =>
            CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, token),
                plate.Kind,
                plate.Thickness,
                plate.Normal,
                plate.NearOffset,
                plate.Outline);

        private static CantileverPlatePlan Mirror(
            CantileverPlatePlan plate, string owner, string token, double planeY) =>
            CantileverPlatePlan.Create(
                CantileverPieceId.Create(owner, token),
                plate.Kind,
                plate.Thickness,
                MirrorY(plate.Normal),

                // El offset se mide A LO LARGO del normal, y el normal también se volteó. Con el plano en
                // y = p, una cara en y = c —normal +Y, offset c— va a parar a y = 2p − c con normal −Y, y un
                // offset o sobre ese normal pone la cara en y = −o: luego o = c − 2p.
                //
                // Con el plano viejo en cero eso se reducía a dejar el offset intacto, que es lo que hacía y
                // por eso funcionaba mientras el plano fue cero. Con el plano en el medio de la columna hay
                // que restar de verdad, o la cara de referencia y el contorno dirían cosas distintas.
                plate.NearOffset - (2.0 * planeY),
                plate.Outline.Select(q => MirrorY(q, planeY)).ToList());

        private static CantileverGussetPlan ReOwn(CantileverGussetPlan gusset, string owner, string token) =>
            CantileverGussetPlan.Create(
                CantileverPieceId.Create(owner, token),
                gusset.Thickness,
                gusset.Normal,
                gusset.NearOffset,
                gusset.Vertices[0],
                gusset.Vertices[1],
                gusset.Vertices[2],
                gusset.VerticalLeg,
                gusset.HorizontalLeg);

        private static CantileverGussetPlan Mirror(
            CantileverGussetPlan gusset, string owner, string token, double planeY) =>
            CantileverGussetPlan.Create(
                CantileverPieceId.Create(owner, token),
                gusset.Thickness,
                // The gusset's normal is TRANSVERSE (+X), and a reflection about y = 0 leaves X alone. So the
                // normal and the offset are untouched here while the plates' normal flips — the difference is
                // real geometry, not an inconsistency.
                gusset.Normal,
                gusset.NearOffset,
                MirrorY(gusset.Vertices[0], planeY),
                MirrorY(gusset.Vertices[1], planeY),
                MirrorY(gusset.Vertices[2], planeY),
                gusset.VerticalLeg,
                gusset.HorizontalLeg);

        private static IReadOnlyList<CantileverPunchPlan> ReOwn(
            IReadOnlyList<CantileverPunchPlan> punches, string owner, string token)
        {
            var id = CantileverPieceId.Create(owner, token);

            return punches
                .Select((p, i) => new CantileverPunchPlan(id.At(i), p.Surface, p.Centre, p.Datum))
                .ToList();
        }

        private static IReadOnlyList<CantileverPunchPlan> Mirror(
            IReadOnlyList<CantileverPunchPlan> punches, string owner, string token, double planeY)
        {
            var id = CantileverPieceId.Create(owner, token);

            // The DATUM is carried across untouched. A punch along Y has its datum in (x, z), and reflecting
            // about ANY plane normal to Y moves neither, así que el agujero espejado y el original comparten
            // datum: en una estación doble un mismo tornillo pasa por las dos placas traseras y por la
            // columna. Con el plano corregido eso pasa a ser cierto FÍSICAMENTE y no sólo en el datum — las
            // dos placas quedan en las dos caras de la columna, con la columna en medio, en vez de las dos
            // pegadas a la misma cara.
            return punches
                .Select((p, i) => new CantileverPunchPlan(
                    id.At(i), p.Surface, MirrorY(p.Centre, planeY), p.Datum))
                .ToList();
        }
    }
}
