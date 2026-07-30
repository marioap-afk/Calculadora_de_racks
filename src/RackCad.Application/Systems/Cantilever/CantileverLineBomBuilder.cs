using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The component BOM of a whole line.
    ///
    /// It does NOT re-derive the station's components. It asks <see cref="CantileverStationBomBuilder"/> for
    /// each station and MERGES equal recipes across stations, then adds what the intervals contribute. Writing
    /// the station recipes again here would be a second authority for the same purchase order line, and the two
    /// would diverge the first time an arm's recipe changed.
    ///
    /// Merging is by RECIPE and never by position, exactly as the station merges its arms: two stations with the
    /// same column and the same arms are one line of quantity two, and a line of ten identical stations quotes
    /// ten of one component rather than ten components of one (ADR-0026, D8, one level up).
    /// </summary>
    public static class CantileverLineBomBuilder
    {
        /// <summary>A separator, as a component: its profile plus the two column plates it bolts to.</summary>
        public const string SeparatorCategory = "Separador";

        /// <summary>A braced panel's diagonal, as a component.</summary>
        public const string BraceCategory = "Tensor";

        /// <summary>The profile line inside a separator component.</summary>
        public const string SeparatorProfileCategory = "Perfil de separador";

        /// <summary>The rod of a cold-rolled brace.</summary>
        public const string ColdRolledRodCategory = "Varilla cold rolled";

        /// <summary>The end adapter of a cold-rolled brace.</summary>
        public const string AdapterCategory = "Adaptador de tensor";

        /// <summary>A gusset of an adapter. Counted and described by GAUGE; no thickness is invented.</summary>
        public const string GussetCategory = "Cartabon";

        /// <summary>The profile line inside a structural brace component.</summary>
        public const string BraceProfileCategory = "Perfil de tensor";

        /// <summary>
        /// Builds the line BOM.
        ///
        /// A BLOCKED line returns an EMPTY component BOM, following the station's rule: quoting a line that
        /// cannot be built is worse than quoting nothing, because the numbers look usable.
        /// </summary>
        public static BillOfMaterials Build(CantileverLineAssembly line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (line.IsBlocked || line.Stations.Count == 0)
            {
                return new BillOfMaterials(Array.Empty<BomComponent>());
            }

            var merged = new Dictionary<string, BomComponent>(StringComparer.Ordinal);
            var order = new List<string>();

            foreach (var placement in line.Stations)
            {
                foreach (var component in CantileverStationBomBuilder.Build(placement.Station).Components)
                {
                    Accumulate(merged, order, ComponentKey(component), component);
                }
            }

            foreach (var interval in line.Intervals)
            {
                foreach (var separator in interval.Separators)
                {
                    var plates = interval.ColumnPlates
                        .Where(p => p.SeparatorIndex == separator.SeparatorIndex)
                        .ToList();

                    var component = SeparatorComponent(separator, plates);
                    Accumulate(merged, order, ComponentKey(component), component);
                }

                foreach (var brace in interval.Braces)
                {
                    var component = BraceComponent(brace);
                    Accumulate(merged, order, ComponentKey(component), component);
                }
            }

            return new BillOfMaterials(order.Select(k => merged[k]).ToList());
        }

        /// <summary>
        /// The grouping key of a component: its category, its profile, its length, its description and its
        /// PIECE LIST.
        ///
        /// The piece list is in the key on purpose. Two components can agree on category, profile and length and
        /// still be different products — a separator whose plates carry a different punch pattern is a different
        /// thing to buy — and merging them would hide the difference behind a quantity.
        /// </summary>
        public static string ComponentKey(BomComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            return string.Join(";", new[]
            {
                "cat=" + component.Category,
                "prf=" + component.ProfileId,
                "len=" + Format(component.Length),
                "dsc=" + component.Description,
                "pcs=" + string.Join("+", (component.Pieces ?? new List<BomLine>()).Select(PieceKey))
            });
        }

        private static string PieceKey(BomLine piece) => string.Format(
            CultureInfo.InvariantCulture,
            "{0}|{1}|{2}|{3}",
            piece.Category, piece.ProfileId, Format(piece.Length), piece.Quantity);

        /// <summary>
        /// Adds a component to the accumulator, summing quantities when the recipe is already there.
        ///
        /// It copies before storing. Accumulating into the component the station builder returned would mutate
        /// that BOM, so asking a station for its own BOM after asking the line for one would report the line's
        /// quantities.
        /// </summary>
        private static void Accumulate(
            IDictionary<string, BomComponent> merged,
            ICollection<string> order,
            string key,
            BomComponent component)
        {
            if (merged.TryGetValue(key, out var existing))
            {
                existing.Quantity += component.Quantity;
                return;
            }

            merged[key] = new BomComponent
            {
                Category = component.Category,
                ProfileId = component.ProfileId,
                Description = component.Description,
                Length = component.Length,
                Quantity = component.Quantity,
                Pieces = (component.Pieces ?? new List<BomLine>())
                    .Select(p => new BomLine
                    {
                        Category = p.Category,
                        ProfileId = p.ProfileId,
                        Description = p.Description,
                        Length = p.Length,
                        Quantity = p.Quantity
                    })
                    .ToList()
            };

            order.Add(key);
        }

        /// <summary>
        /// One separator, as a component: its profile and the two column plates its ends bolt to.
        ///
        /// The plates are PIECES of the separator and not components of their own. They exist because that
        /// separator exists, they are cut for it, and a line that quoted them separately would let somebody
        /// order separators without their plates.
        /// </summary>
        private static BomComponent SeparatorComponent(
            CantileverSeparatorPlan separator,
            IReadOnlyList<CantileverSeparatorColumnPlatePlan> plates)
        {
            var pieces = new List<BomLine>
            {
                new BomLine
                {
                    Category = SeparatorProfileCategory,
                    ProfileId = separator.Member.SectionId.Value,
                    Description = SeparatorProfileCategory + " " + separator.Member.SectionId.Value,
                    Length = Round(separator.CutLength),
                    Quantity = 1
                }
            };

            if (plates.Count > 0)
            {
                // The two plates are IDENTICAL — same size, same thickness, one centred hole each — so they are
                // one line of quantity two. Reading the count rather than writing 2 keeps this true if a future
                // end ever needs a different plate.
                pieces.Add(PlateLine(plates[0], plates.Count));
            }

            return new BomComponent
            {
                Category = SeparatorCategory,
                ProfileId = separator.Member.SectionId.Value,
                Description = string.Format(
                    CultureInfo.InvariantCulture,
                    "Separador {0} de {1:0.##}\" · 4 troqueles",
                    separator.Member.SectionId.Value, separator.CutLength),
                Length = Round(separator.CutLength),
                Quantity = 1,
                Pieces = pieces
            };
        }

        private static BomLine PlateLine(CantileverSeparatorColumnPlatePlan plate, int quantity)
        {
            var size = CantileverPlateInPlaneDimensions.Measure(plate.Plate);

            return new BomLine
            {
                Category = CantileverStationBomBuilder.PlateCategory,
                // Same convention the station uses: Length = 0 because a plate has no linear length, and the
                // recipe — both in-plane dimensions, the thickness and the hole — lives in the id, so two plates
                // of different sizes cannot collapse into one line.
                ProfileId = string.Format(
                    CultureInfo.InvariantCulture,
                    "Placa de columna para separador|{0}x{1}|t{2}|d{3}",
                    Format(size.Width), Format(size.Height), Format(plate.Plate.Thickness),
                    Format(plate.Punch.Diameter)),
                Description = string.Format(
                    CultureInfo.InvariantCulture,
                    "Placa de columna para separador {0:0.##}\" x {1:0.##}\" x {2:0.###}\" · 1 troquel",
                    size.Width, size.Height, plate.Plate.Thickness),
                Length = 0.0,
                Quantity = quantity
            };
        }

        /// <summary>
        /// One diagonal, as a component.
        ///
        /// A structural brace is a profile. A cold-rolled brace is a ROD plus two adapters, each with its
        /// gussets — and the gussets are described by GAUGE, as <c>CAL_10</c>, with no thickness. The repository
        /// has no gauge table and nothing that converts a gauge to a decimal, so inventing one here would put a
        /// number on a drawing that no source backs (decision 12.30).
        /// </summary>
        private static BomComponent BraceComponent(CantileverBracePlan brace)
        {
            switch (brace.Kind)
            {
                case CantileverBraceBodyKind.StructuralSection:
                {
                    var pieces = new List<BomLine>
                    {
                        new BomLine
                        {
                            Category = BraceProfileCategory,
                            ProfileId = brace.Member.SectionId.Value,
                            Description = BraceProfileCategory + " " + brace.Member.SectionId.Value,
                            Length = Round(brace.Member.NominalCutLength),
                            Quantity = 1
                        }
                    };

                    return new BomComponent
                    {
                        Category = BraceCategory,
                        ProfileId = brace.Member.SectionId.Value,
                        Description = string.Format(
                            CultureInfo.InvariantCulture,
                            "Tensor {0} de {1:0.##}\" · 2 troqueles",
                            brace.Member.SectionId.Value, brace.Member.NominalCutLength),
                        Length = Round(brace.Member.NominalCutLength),
                        Quantity = 1,
                        Pieces = pieces
                    };
                }

                case CantileverBraceBodyKind.ColdRolledRound:
                {
                    var rodId = string.Format(
                        CultureInfo.InvariantCulture, "CR-ROUND-{0}", Format(brace.RoundDiameter));

                    var pieces = new List<BomLine>
                    {
                        new BomLine
                        {
                            Category = ColdRolledRodCategory,
                            ProfileId = rodId,
                            Description = string.Format(
                                CultureInfo.InvariantCulture,
                                "Varilla cold rolled {0:0.###}\" de diametro",
                                brace.RoundDiameter),
                            Length = Round(brace.BodyLength),
                            Quantity = 1
                        }
                    };

                    if (brace.Adapters.Count > 0)
                    {
                        var adapter = brace.Adapters[0];

                        pieces.Add(new BomLine
                        {
                            Category = AdapterCategory,
                            ProfileId = string.Format(
                                CultureInfo.InvariantCulture,
                                "L{0}x{0}x{1}|{2}|d{3}",
                                Format(adapter.Leg), Format(adapter.Thickness),
                                Format(adapter.CutLength), Format(adapter.SeparatorFacePunch.Diameter)),
                            Description = string.Format(
                                CultureInfo.InvariantCulture,
                                "Adaptador L{0:0.##}\" x {1:0.##}\" x {2:0.###}\" de {3:0.##}\"",
                                adapter.Leg, adapter.Leg, adapter.Thickness, adapter.CutLength),
                            Length = Round(adapter.CutLength),
                            Quantity = brace.Adapters.Count
                        });

                        pieces.Add(new BomLine
                        {
                            Category = GussetCategory,
                            ProfileId = "CAL_" + adapter.GussetGaugeNumber.ToString(CultureInfo.InvariantCulture),
                            Description = adapter.GussetDescription,
                            Length = 0.0,
                            Quantity = brace.Adapters.Sum(a => a.GussetCount)
                        });
                    }

                    return new BomComponent
                    {
                        Category = BraceCategory,
                        ProfileId = rodId,
                        Description = string.Format(
                            CultureInfo.InvariantCulture,
                            "Tensor cold rolled {0:0.###}\" de {1:0.##}\" · {2} adaptadores",
                            brace.RoundDiameter, brace.BodyLength, brace.Adapters.Count),
                        Length = Round(brace.BodyLength),
                        Quantity = 1,
                        Pieces = pieces
                    };
                }

                default:
                    throw new InvalidOperationException(
                        "El tipo de cuerpo de tensor '" + brace.Kind + "' no tiene linea de BOM.");
            }
        }

        private static double Round(double value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

        private static string Format(double value) =>
            Round(value).ToString("0.####", CultureInfo.InvariantCulture);
    }
}
