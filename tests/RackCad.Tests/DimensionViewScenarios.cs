using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    /// <summary>
    /// I-50 (G4) — los racks y las vistas sobre los que T-06, T-07, T-09 y T-10 asignan la política a sistemas YA
    /// resueltos. Son los escenarios de la caracterización T-03..T-05 (numeración, nombre y estilo de cota activos),
    /// recorridos por el mismo camino que dibuja el Plugin. Cada vista declara su TIPO, que es lo que decide qué bit la
    /// gobierna. En G4 la política no viaja todavía por diseños, resolvers ni DTO: se asigna al sistema resuelto.
    /// </summary>
    internal static class DimensionViewScenarios
    {
        internal const string RackName = "RACK I50";
        internal const string DimensionStyle = "I50_ESTILO";

        internal static readonly IReadOnlyList<DimensionDetail> ActiveLevels = new[]
        {
            DimensionDetail.Minimal, DimensionDetail.Standard, DimensionDetail.Detailed
        };

        /// <summary>Las ocho combinaciones de los tres bits, más valores presentes con bits desconocidos y de signo.</summary>
        internal static readonly IReadOnlyList<int> Policies = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 13, -1, -8 };

        internal static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        internal sealed class View
        {
            internal View(string key, DimensionViewKind kind, IEnumerable<HeaderBlockInstance> instances)
            {
                Key = key;
                Kind = kind;
                Instances = instances.ToList();
            }

            internal string Key { get; }
            internal DimensionViewKind Kind { get; }
            internal IReadOnlyList<HeaderBlockInstance> Instances { get; }
        }

        internal static DimensionViewVisibility? Policy(int? value)
            => value.HasValue ? (DimensionViewVisibility)value.Value : (DimensionViewVisibility?)null;

        /// <summary>Si el bit del tipo está encendido en un valor presente (1 = Frontal, 2 = Lateral, 4 = Planta).</summary>
        internal static bool BitIsOn(int policy, DimensionViewKind kind)
        {
            var bit = kind == DimensionViewKind.Frontal ? 1 : kind == DimensionViewKind.Lateral ? 2 : 4;
            return (policy & bit) != 0;
        }

        // ---- Selectivo ------------------------------------------------------------------------------------------

        /// <summary>Un fondo (tres frentes de cuatro niveles) o dos fondos en esquina (el segundo con 40" de fondo, dos
        /// frentes y tres niveles más bajos).</summary>
        internal static SelectiveRackSystem Selective(DimensionDetail detail, bool twoFondos, RackCatalog catalog)
        {
            var design = new SelectivePalletDesign
            {
                PostId = TestCatalogIds.Profiles.Posts.Standard,
                PostPeralte = 3.0,
                PalletDepth = 48.0,
                NumberFronts = true,
                NumberLevels = true,
                DrawRackName = true,
                Dimensions = detail,
                DimensionStyle = DimensionStyle
            };

            for (var b = 0; b < 3; b++)
            {
                design.Bays.Add(SelectiveBay(levels: 4, palletHeight: 60.0));
            }

            if (twoFondos)
            {
                design.DepthCount = 2;
                design.SeparatorLengths.Add(12.0);
                design.ExtraFondoDepths.Add(40.0);
                design.ExtraFondoBays.Add(new List<SelectiveBayDesign>
                {
                    SelectiveBay(levels: 3, palletHeight: 48.0),
                    SelectiveBay(levels: 3, palletHeight: 48.0)
                });
            }

            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            system.Name = RackName;
            return system;
        }

        private static SelectiveBayDesign SelectiveBay(int levels, double palletHeight)
        {
            var bay = new SelectiveBayDesign();
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = palletHeight },
                    PalletCount = 2,
                    BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>La frontal de cada fondo por <c>FondoSystemView</c> (C-05), cada corte lateral y la planta.</summary>
        internal static IReadOnlyList<View> SelectiveViews(SelectiveRackSystem system, RackCatalog catalog)
        {
            var views = new List<View>();
            var fondos = SelectiveDepthLayout.Offsets(system).Count;
            for (var k = 0; k < fondos; k++)
            {
                var fondoView = SelectiveDepthLayout.FondoSystemView(system, k);
                fondoView.Name = RackName;
                views.Add(new View($"frontal-fondo{k}", DimensionViewKind.Frontal,
                    new SelectiveFrontalBuilder().BuildPlan(fondoView, catalog).Flatten().Instances));
            }

            foreach (var corte in new SelectiveLateralBuilder().Cortes(system, catalog))
            {
                views.Add(new View($"lateral-corte{corte.PostIndex}", DimensionViewKind.Lateral, corte.Largueros));
            }

            views.Add(new View("planta", DimensionViewKind.Planta,
                new SelectivePlantaBuilder().BuildPlan(system, catalog).Flatten().Instances));
            return views;
        }

        // ---- Dinámico ------------------------------------------------------------------------------------------

        /// <summary>El escenario jagged de la golden del Dinámico: frentes con distinto número de niveles, profundidad y
        /// altura de primer nivel.</summary>
        internal static DynamicRackSystem Dynamic(DimensionDetail detail, RackCatalog catalog)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
                NumberLevels = true,
                NumberFronts = true,
                DrawRackName = true,
                Dimensions = detail,
                DimensionStyle = DimensionStyle
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 4.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1, FirstLevelHeight = 12.0 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 8.0 });
            foreach (var family in DynamicSafetyDefaults.Build(catalog))
            {
                design.SafetySelections.Add(family);
            }

            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            system.Name = RackName;
            return system;
        }

        /// <summary>La frontal de salida y la de entrada (las dos Frontal), el lateral entero y sus cortes, y la planta.</summary>
        internal static IReadOnlyList<View> DynamicViews(DynamicRackSystem system, RackCatalog catalog)
        {
            var frontal = new DynamicSystemFrontalBuilder();
            var lateral = new DynamicSystemLateralBuilder();
            var views = new List<View>
            {
                new View("frontal-salida", DimensionViewKind.Frontal, frontal.BuildPlan(system, catalog, DynamicRackEnd.Exit).Flatten().Instances),
                new View("frontal-entrada", DimensionViewKind.Frontal, frontal.BuildPlan(system, catalog, DynamicRackEnd.Entrance).Flatten().Instances),
                new View("lateral", DimensionViewKind.Lateral, lateral.Build(system, catalog).Flatten().Instances)
            };

            foreach (var corte in lateral.Cortes(system, catalog))
            {
                views.Add(new View($"lateral-corte{corte.PostIndex}", DimensionViewKind.Lateral, corte.Plan.Flatten().Instances));
            }

            views.Add(new View("planta", DimensionViewKind.Planta, new DynamicSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances));
            return views;
        }

        // ---- Push Back de un sentido (solo T-10 en G4) ------------------------------------------------------------

        /// <summary>El escenario de un sentido de la golden de Push Back. El compuesto A/B y la cobertura por vista de Push
        /// Back (T-08) son de G6: su política necesita la copia compartida C-15.</summary>
        internal static PushBackSystem PushBackSingleSided(DimensionDetail detail, RackCatalog catalog)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0,
                    NumberFronts = true,
                    NumberLevels = true,
                    DrawRackName = true,
                    Dimensions = detail,
                    DimensionStyle = DimensionStyle
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 });
            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            design.RearTope.Disable(0, 0);
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1, Side = SafetySide.Both });

            var system = new PushBackResolver(catalog).Resolve(design);
            system.Name = RackName;
            return system;
        }

        internal static IReadOnlyList<View> PushBackSingleSidedViews(PushBackSystem system, RackCatalog catalog)
        {
            var frontal = new PushBackSystemFrontalBuilder();
            var lateral = new PushBackSystemLateralBuilder();
            var views = new List<View>
            {
                new View("frontal-entrada-salida", DimensionViewKind.Frontal, frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances),
                new View("frontal-posterior", DimensionViewKind.Frontal, frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances),
                new View("lateral", DimensionViewKind.Lateral, lateral.Build(system, catalog).Flatten().Instances)
            };

            foreach (var corte in lateral.Cortes(system, catalog))
            {
                views.Add(new View($"lateral-corte{corte.PostIndex}", DimensionViewKind.Lateral, corte.Plan.Flatten().Instances));
            }

            views.Add(new View("planta", DimensionViewKind.Planta, new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances));
            return views;
        }
    }
}
