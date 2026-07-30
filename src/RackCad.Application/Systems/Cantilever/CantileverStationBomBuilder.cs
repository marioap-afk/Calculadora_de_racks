using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The BOM of a resolved Cantilever station, BY COMPONENTS.
    ///
    /// A component is what gets bought or fabricated as one unit and bolted to the rest — the same definition
    /// <c>BomBuilder.Components</c> already uses for cabeceras. A station has exactly two kinds: ONE
    /// column-with-base component, and one component per physical arm, grouped by recipe (ADR-0026, D8).
    ///
    /// It derives everything from the RESOLVED station and nothing from the design, the matrix, a view or a
    /// block. A BOM computed from the intent counts what the user asked for; a BOM computed from the
    /// resolution counts what the model actually built, and those differ exactly when something was rejected
    /// or adjusted — which is when the difference matters most.
    ///
    /// Punches are NOT pieces. They are holes.
    /// </summary>
    public static class CantileverStationBomBuilder
    {
        // ---- categories. Spanish, because they are what the user reads. ---------------------------------

        public const string ColumnBaseCategory = "Columna con base";
        public const string ArmCategory = "Brazo";
        public const string ColumnCategory = "Columna";
        public const string BaseCategory = "Base";
        public const string ArmProfileCategory = "Perfil de brazo";
        public const string PlateCategory = "Placa";
        public const string GussetCategory = "Cartabon";

        /// <summary>
        /// Builds the station BOM.
        ///
        /// A BLOCKED station returns an EMPTY component BOM rather than a partial one: quoting a station that
        /// cannot be built is worse than quoting nothing, because the numbers look usable.
        /// </summary>
        public static BillOfMaterials Build(CantileverStationAssembly station)
        {
            if (station == null)
            {
                throw new ArgumentNullException(nameof(station));
            }

            if (station.IsBlocked || station.ColumnBase == null)
            {
                return new BillOfMaterials(Array.Empty<BomComponent>());
            }

            var components = new List<BomComponent> { ColumnBaseComponent(station.ColumnBase) };
            components.AddRange(ArmComponents(station));

            return new BillOfMaterials(components);
        }

        /// <summary>
        /// The ONE column–base component.
        ///
        /// One, whatever the face mode. A double station is not two column–base components: it is this one with
        /// two bases, two front plates, two rear plates and two gussets in its recipe, and exactly ONE column
        /// and ONE column bottom plate. Producing two would ask the buyer for two columns (ADR-0026, D8).
        /// </summary>
        private static BomComponent ColumnBaseComponent(CantileverStationColumnBaseAssembly columnBase)
        {
            var pieces = new List<BomLine>
            {
                Profile(ColumnCategory, columnBase.Column),
                Plate(columnBase.ColumnBottomPlate, "Placa inferior de columna", 1)
            };

            var sides = columnBase.Sides.Count;

            // The base pieces are IDENTICAL on both sides — one configuration, one resolve, mirrored — so they
            // group into one line of quantity `sides` rather than two lines of one. That is also what makes
            // the single and double recipes the same shape.
            var first = columnBase.Sides[0];

            pieces.Add(Profile(BaseCategory, first.Member, sides));
            pieces.Add(Plate(first.FrontPlate, "Placa frontal de base", sides));
            pieces.Add(Plate(first.RearPlate, "Placa posterior de base", sides));
            pieces.Add(Gusset(first.Gusset, sides));

            return new BomComponent
            {
                Category = ColumnBaseCategory,
                ProfileId = columnBase.Column.SectionId.Value,
                Description = DescribeColumnBase(columnBase),
                Length = Round(columnBase.Column.NominalCutLength),
                Quantity = 1,
                Pieces = pieces
            };
        }

        /// <summary>
        /// One component per DISTINCT arm recipe, with its count.
        ///
        /// Grouped by physical recipe and never by position: the signature carries the arrangement, the
        /// section, the cut length, the slope and the plates, and it carries NO side, level, station index,
        /// world coordinate or owner token. Two arms that are the same piece are the same line even when one
        /// hangs off +Y and the other off −Y — by the owner's decision, and until a right/left variant exists
        /// they are physically interchangeable (ADR-0026, D8).
        /// </summary>
        private static IReadOnlyList<BomComponent> ArmComponents(CantileverStationAssembly station)
        {
            var bySignature = new Dictionary<string, BomComponent>(StringComparer.Ordinal);
            var order = new List<string>();

            foreach (var arm in station.Arms)
            {
                var signature = ArmSignature(arm);

                if (!bySignature.TryGetValue(signature, out var component))
                {
                    component = new BomComponent
                    {
                        Category = ArmCategory,
                        ProfileId = arm.Body.Members[0].SectionId.Value,
                        Description = DescribeArm(arm),
                        Length = Round(arm.Body.CutLength),
                        Quantity = 0,
                        Pieces = ArmPieces(arm)
                    };

                    bySignature[signature] = component;
                    order.Add(signature);
                }

                component.Quantity++;
            }

            return order.Select(s => bySignature[s]).ToList();
        }

        /// <summary>
        /// The recipe of ONE arm: its one or two body profiles, its mounting plate and its optional end plate.
        ///
        /// No punches. A hole is not a piece, so the mounting punches never appear — which also means the
        /// piece count of an arm does not change when somebody adds a bolt row.
        /// </summary>
        private static IReadOnlyList<BomLine> ArmPieces(CantileverArmAssembly arm)
        {
            var pieces = new List<BomLine>
            {
                // Both members of a paired body have the SAME section and the same cut, so they are one line of
                // quantity two rather than two lines. I-37B guarantees it; this reads it rather than assuming.
                Profile(ArmProfileCategory, arm.Body.Members[0], arm.Body.Members.Count),
                Plate(arm.MountingPlate, "Placa de conexion de brazo", 1)
            };

            if (arm.EndPlate != null)
            {
                pieces.Add(Plate(
                    arm.EndPlate,
                    arm.EndPlateMode == CantileverArmEndPlateMode.Stop
                        ? "Tope de brazo"
                        : "Tapa de brazo",
                    1));
            }

            return pieces;
        }

        /// <summary>
        /// The grouping key of an arm: its PHYSICAL recipe.
        ///
        /// Everything that makes two arms different pieces, and nothing that only makes them different
        /// instances. The exclusions are the whole point: side, level, station index, world coordinates and
        /// owner token would all split one purchase order line into several.
        /// </summary>
        public static string ArmSignature(CantileverArmAssembly arm)
        {
            if (arm == null)
            {
                throw new ArgumentNullException(nameof(arm));
            }

            var body = arm.Body;
            var mount = arm.MountingPlate;
            var end = arm.EndPlate;

            return string.Join(";", new[]
            {
                "arr=" + body.Arrangement,
                "sec=" + body.Members[0].SectionId.Value,
                "n=" + body.Members.Count.ToString(CultureInfo.InvariantCulture),
                "cut=" + Format(body.Members[0].NominalCutLength),
                "slope=" + Format(body.SlopeRisePer12),
                "mount=" + Format(mount.Thickness) + "@" + PlateSize(mount),
                "rows=" + arm.ConnectionPattern.VerticalPunchCount.ToString(CultureInfo.InvariantCulture),
                "end=" + arm.EndPlateMode +
                    (end == null ? string.Empty : ":" + Format(end.Thickness) + "@" + PlateSize(end)),
                "stop=" + Format(StopExtension(arm))
            });
        }

        /// <summary>
        /// How far a stop reaches above what a cap would have covered.
        ///
        /// Derived from the resolved plate rather than read from the design, because the BOM is derived from the
        /// station (ADR-0026, D7). Zero for <c>None</c> and <c>Cap</c>.
        /// </summary>
        private static double StopExtension(CantileverArmAssembly arm)
        {
            if (arm.EndPlateMode != CantileverArmEndPlateMode.Stop || arm.EndPlate == null)
            {
                return 0.0;
            }

            // The plate's own height minus the body depth it closes: what the stop adds.
            var height = PlateExtents(arm.EndPlate).Height;
            return Math.Max(0.0, height - arm.Body.SectionHeight);
        }

        // ---- flat pieces ---------------------------------------------------------------------------------

        private static BomLine Profile(
            string category, CantileverStructuralMemberPlan member, int quantity = 1) =>
            new BomLine
            {
                Category = category,
                // A structural profile's identity IS its section id, and its length its nominal cut. That is
                // the same convention I-37A and I-37B use, and it is what a supplier is asked for.
                ProfileId = member.SectionId.Value,
                Description = category + " " + member.SectionId.Value,
                Length = Round(member.NominalCutLength),
                Quantity = quantity
            };

        /// <summary>
        /// A plate line.
        ///
        /// <c>Length = 0</c>, following the vigent precedent of <c>BomBuilder.AddPlate</c>: a plate has no
        /// linear length, and putting one of its two in-plane dimensions there would make it look like a bar.
        /// Its real size lives in the DESCRIPTION, where a reader can see both dimensions and the thickness.
        /// </summary>
        private static BomLine Plate(CantileverPlatePlan plate, string what, int quantity)
        {
            var extents = PlateExtents(plate);

            return new BomLine
            {
                Category = PlateCategory,
                ProfileId = what,
                Description = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1:0.##}\" x {2:0.##}\" x {3:0.###}\"",
                    what, extents.Width, extents.Height, plate.Thickness),
                Length = 0.0,
                Quantity = quantity
            };
        }

        private static BomLine Gusset(CantileverGussetPlan gusset, int quantity) =>
            new BomLine
            {
                Category = GussetCategory,
                ProfileId = "Cartabon",
                Description = string.Format(
                    CultureInfo.InvariantCulture,
                    "Cartabon {0:0.##}\" x {1:0.##}\" x {2:0.###}\"",
                    gusset.VerticalLeg, gusset.HorizontalLeg, gusset.Thickness),
                Length = 0.0,
                Quantity = quantity
            };

        /// <summary>
        /// The two IN-PLANE dimensions of a plate, largest first.
        ///
        /// Measured from the outline and sorted, so a plate lying in XZ and one lying in XY describe themselves
        /// the same way. Sorting matters: without it the same physical plate would produce two different
        /// descriptions depending on which plane it happens to occupy, and the BOM would split one line in two.
        /// </summary>
        private static (double Width, double Height) PlateExtents(CantileverPlatePlan plate)
        {
            var spans = new[]
            {
                plate.Outline.Max(p => p.X) - plate.Outline.Min(p => p.X),
                plate.Outline.Max(p => p.Y) - plate.Outline.Min(p => p.Y),
                plate.Outline.Max(p => p.Z) - plate.Outline.Min(p => p.Z)
            }
            .OrderByDescending(v => v)
            .ToList();

            return (spans[0], spans[1]);
        }

        private static string PlateSize(CantileverPlatePlan plate)
        {
            var extents = PlateExtents(plate);
            return Format(extents.Width) + "x" + Format(extents.Height);
        }

        // ---- descriptions --------------------------------------------------------------------------------

        private static string DescribeColumnBase(CantileverStationColumnBaseAssembly columnBase) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "Columna {0} de {1:0.##}\" con base {2} de {3:0.##}\" · {4}",
                columnBase.Column.SectionId.Value,
                columnBase.Column.NominalCutLength,
                columnBase.Sides[0].Member.SectionId.Value,
                columnBase.Sides[0].Member.NominalCutLength,
                columnBase.FaceMode == CantileverStationFaceMode.Double
                    ? "gondola doble (2 bases)"
                    : "gondola sencilla (1 base)");

        private static string DescribeArm(CantileverArmAssembly arm) =>
            string.Format(
                CultureInfo.InvariantCulture,
                "Brazo {0} de {1:0.##}\" · {2} · pendiente {3:0.##}/12 · {4}",
                arm.Body.Members[0].SectionId.Value,
                arm.Body.CutLength,
                ArrangementLabel(arm.Body.Arrangement),
                arm.Body.SlopeRisePer12,
                EndPlateLabel(arm.EndPlateMode));

        /// <summary>Spanish labels of the arrangements, exhaustive with a throwing default.</summary>
        private static string ArrangementLabel(CantileverArmBodyArrangement arrangement)
        {
            switch (arrangement)
            {
                case CantileverArmBodyArrangement.Single:
                    return "perfil sencillo";
                case CantileverArmBodyArrangement.DoubleChannelFacing:
                    return "canal doble encontrado";
                case CantileverArmBodyArrangement.DoubleChannelBackToBack:
                    return "canal doble espalda con espalda";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(arrangement), arrangement, "El arreglo no tiene etiqueta de BOM.");
            }
        }

        private static string EndPlateLabel(CantileverArmEndPlateMode mode)
        {
            switch (mode)
            {
                case CantileverArmEndPlateMode.None:
                    return "sin tapa";
                case CantileverArmEndPlateMode.Cap:
                    return "con tapa";
                case CantileverArmEndPlateMode.Stop:
                    return "con tope";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(mode), mode, "El modo de placa final no tiene etiqueta de BOM.");
            }
        }

        private static double Round(double value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

        private static string Format(double value) =>
            Math.Round(value, 6, MidpointRounding.AwayFromZero).ToString("0.######", CultureInfo.InvariantCulture);
    }
}
