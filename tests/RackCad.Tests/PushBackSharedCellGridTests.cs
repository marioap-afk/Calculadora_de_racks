using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-G2, contrato del dueño) — LA DEMANDA TRANSVERSAL DE UNA CELDA TAMBIEN ES DEL RACK.
    ///
    /// <para>
    /// A3-G1 dejo compartidos los campos transversales del FRENTE. Pero el ancho de una bahia tambien lo sube una
    /// CELDA: su propio override de larguero, o un frente de tarima mayor que ensancha su calle. Esa capa se
    /// resolvia dentro de cada marco por separado. Medido antes del cambio, con un override por celda de 150" solo
    /// en el lado A: la compuesta y el marco de B ponian su segunda linea en 53.49 y el de A en 153.49; las botas y
    /// las defensas —ancladas al frame compuesto— quedaban a 100" del poste mas cercano del corte de A.
    /// </para>
    /// <para>
    /// Lo que se comparte es la geometria fisica DERIVADA. La intencion de cada lado no se copia: si solo A declaro
    /// el override, B sigue sin declararlo.
    /// </para>
    /// </summary>
    public class PushBackSharedCellGridTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(int levels = 2, int deepA = 4, int deepB = 4)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: deepA, deepB: deepB, levelsA: levels, levelsB: levels, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = "DEFENSA_MONTACARGAS";
            design.SideB.DefensePieceId = "DEFENSA_MONTACARGAS";
            return design;
        }

        /// <summary>Declara <paramref name="levels"/> celdas en un frente, con el valor pedido en la celda <paramref name="at"/>.</summary>
        private static void Cells(
            DynamicRackFrontDesign front, int levels, int at, double? beamOverride = null, double? palletFront = null)
        {
            for (var index = 0; index < levels; index++)
            {
                front.Levels.Add(index == at
                    ? new DynamicRackLevelDesign { BeamLengthOverride = beamOverride, PalletFront = palletFront }
                    : new DynamicRackLevelDesign());
            }
        }

        private static PushBackSystem Resolve(PushBackDesign design)
            => new PushBackResolver(Catalog).Resolve(design);

        private static IReadOnlyList<double> Posts(DynamicRackSystem structure)
            => structure == null
                ? Array.Empty<double>()
                : DynamicFrontGeometry.Compute(structure, Catalog).PostPositions
                    .Select(x => Math.Round(x, 6))
                    .ToList();

        private static IReadOnlyList<double> LocalPosts(PushBackSystem system, PushBackSide side)
            => Posts(system.Composite?.Of(side)?.Local?.Structure);

        private static IReadOnlyList<double> CutPosts(PushBackSystem system, PushBackSide side)
            => new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, side)
                .Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .Select(instance => Math.Round(instance.Insertion.X, 6))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

        private static void AssertOneGrid(PushBackSystem system)
        {
            var composite = Posts(system.Structure);
            Assert.NotEmpty(composite);
            Assert.Equal(composite, LocalPosts(system, PushBackSide.A));
            Assert.Equal(composite, LocalPosts(system, PushBackSide.B));
        }

        // ---------------------------------------------------------------- override por celda

        [Fact]
        public void CompositeGrid_CellBeamLengthOverrideUsesSharedPhysicalGrid()
        {
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, beamOverride: 150.0);

            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(150.0, system.Structure.Fronts[0].BeamLength, 6);

            // Y la intencion de B sigue siendo la suya: nadie le escribio un override.
            Assert.Null(design.SideB.Fronts[0].BeamLengthOverride);
            Assert.All(design.SideB.Fronts[0].Levels, level => Assert.Null(level.BeamLengthOverride));
        }

        [Fact]
        public void CompositeGrid_CellBeamLengthOverrideOnSideBUsesSharedPhysicalGrid()
        {
            var design = Design();
            Cells(design.SideB.Fronts[0], 2, 0, beamOverride: 150.0);

            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(150.0, system.Structure.Fronts[0].BeamLength, 6);
            Assert.Null(design.Structure.Fronts[0].BeamLengthOverride);
            Assert.All(design.Structure.Fronts[0].Levels, level => Assert.Null(level.BeamLengthOverride));
        }

        [Fact]
        public void CompositeGrid_CellBeamLengthOverridesUseMaximumEffectiveDemand()
        {
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, beamOverride: 140.0);
            Cells(design.SideB.Fronts[0], 2, 0, beamOverride: 160.0);

            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(160.0, system.Structure.Fronts[0].BeamLength, 6);   // el maximo, no el promedio (150)
        }

        [Fact]
        public void CompositeGrid_PerCellPalletWidthUsesSharedPhysicalGrid()
        {
            // Sin ningun override: una celda de A con la tarima mas ancha ensancha la bahia por la regla automatica.
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, palletFront: 60.0);

            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.True(
                system.Structure.Fronts[0].BeamLength > system.Structure.Fronts[1].BeamLength,
                "la bahia con la tarima mas ancha mide mas que la de al lado");
        }

        [Fact]
        public void CompositeGrid_HighestEffectiveLevelDemandOwnsSharedBayWidth()
        {
            // La demanda mayor vive en el TERCER nivel: mirar solo el primero no la veria.
            var design = Design(levels: 3);
            Cells(design.Structure.Fronts[0], 3, 2, beamOverride: 170.0);

            var system = Resolve(design);

            AssertOneGrid(system);
            Assert.Equal(170.0, system.Structure.Fronts[0].BeamLength, 6);
        }

        // ---------------------------------------------------------------- la precedencia canonica

        [Fact]
        public void EffectiveLevelBeamLengthDemand_UsesCanonicalOverridePrecedence()
        {
            // Tres ramas distinguibles: la celda manda sobre el frente, y el frente sobre el automatico.
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletTolerance = 0.0,
            };
            var front = new DynamicRackFrontDesign { PalletCount = 1, BeamLengthOverride = 90.0 };
            var withCell = new DynamicRackLevelDesign { BeamLengthOverride = 150.0 };
            var withoutCell = new DynamicRackLevelDesign();

            var auto = DynamicFrontGeometry.AutoBeamLength(42.0, 1, 0.0);
            Assert.NotEqual(90.0, auto);
            Assert.NotEqual(150.0, auto);

            // 1) la celda gana
            Assert.Equal(
                150.0,
                DynamicRackLevelGeometry.EffectiveBeamLengthDemand(design, front, withCell, 1),
                6);

            // 2) sin celda, el frente
            Assert.Equal(
                90.0,
                DynamicRackLevelGeometry.EffectiveBeamLengthDemand(design, front, withoutCell, 1),
                6);

            // 3) sin frente, el automatico de sus calles
            var bare = new DynamicRackFrontDesign { PalletCount = 1 };
            Assert.Equal(
                auto,
                DynamicRackLevelGeometry.EffectiveBeamLengthDemand(design, bare, withoutCell, 1),
                6);

            // Y el automatico usa el frente de tarima de LA CELDA cuando lo tiene.
            Assert.Equal(
                DynamicFrontGeometry.AutoBeamLength(60.0, 1, 0.0),
                DynamicRackLevelGeometry.EffectiveBeamLengthDemand(
                    design, bare, new DynamicRackLevelDesign { PalletFront = 60.0 }, 1),
                6);
        }

        [Fact]
        public void EffectiveFrontDemand_IsTheLargestOfItsLevels()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletTolerance = 0.0,
            };
            var front = new DynamicRackFrontDesign { PalletCount = 1 };
            front.Levels.Add(new DynamicRackLevelDesign());
            front.Levels.Add(new DynamicRackLevelDesign { BeamLengthOverride = 175.0 });
            front.Levels.Add(new DynamicRackLevelDesign());

            Assert.Equal(175.0, DynamicRackLevelGeometry.EffectiveBeamLengthDemand(design, front, 3, 1), 6);
        }

        // ---------------------------------------------------------------- intencion intacta

        [Fact]
        public void SharedGrid_DoesNotCopyCellBeamLengthOverrideAcrossSides()
        {
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, beamOverride: 150.0);
            Cells(design.SideB.Fronts[0], 2, 0);

            Resolve(design);

            // Despues de construir y resolver, cada lado conserva EXACTAMENTE lo que declaro.
            Assert.Equal(150.0, design.Structure.Fronts[0].Levels[0].BeamLengthOverride);
            Assert.Null(design.Structure.Fronts[0].Levels[1].BeamLengthOverride);
            Assert.All(design.SideB.Fronts[0].Levels, level => Assert.Null(level.BeamLengthOverride));
        }

        // ---------------------------------------------------------------- cortes y seguridad

        [Fact]
        public void CompositeCut_BootAnchorMatchesPost_WithCellBeamLengthOverride()
        {
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, beamOverride: 150.0);
            var system = Resolve(design);
            var boots = PushBackBootPlan.Resolve(system, Catalog);

            Assert.NotEmpty(boots);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var posts = CutPosts(system, side);
                foreach (var boot in boots.Where(boot => boot.Side == side))
                {
                    Assert.True(
                        posts.Any(post => Math.Abs(post - boot.LineX) <= 1e-6),
                        FormattableString.Invariant(
                            $"la bota de {side} en X={boot.LineX:0.####} no cae sobre ningun poste del corte"));
                }
            }
        }

        [Fact]
        public void CompositeCut_DefenseAnchorMatchesPost_WithCellBeamLengthOverride()
        {
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, beamOverride: 150.0);
            var system = Resolve(design);
            var defenses = PushBackDefensePlan.Resolve(system, Catalog);

            Assert.NotEmpty(defenses);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var posts = CutPosts(system, side);
                foreach (var defense in defenses.Where(defense => defense.Side == side))
                {
                    Assert.True(
                        posts.Any(post => Math.Abs(post - defense.LineX) <= 1e-6),
                        FormattableString.Invariant(
                            $"la defensa de {side} en X={defense.LineX:0.####} no cae sobre ningun poste del corte"));
                }
            }
        }

        // ---------------------------------------------------------------- BOM

        [Fact]
        public void CompositeBom_CellLevelWidthDemandUsesSharedTransverseLength()
        {
            var design = Design();
            Cells(design.Structure.Fronts[0], 2, 0, beamOverride: 150.0);
            var system = Resolve(design);
            var bom = PushBackBomBuilder.Build(system, Catalog);

            var lengths = bom.Lines
                .Where(line => line.Category != null && line.Category.IndexOf("arguero", StringComparison.Ordinal) >= 0)
                .Select(line => Math.Round(line.Length, 2))
                .Distinct()
                .ToList();

            Assert.NotEmpty(lengths);
            Assert.Contains(lengths, length => Math.Abs(length - 150.0) < 1e-6);
            // La bahia mide 150 para los DOS lados: la longitud automatica de 50 no puede seguir comprandose ahi.
            Assert.DoesNotContain(lengths, length => Math.Abs(length - 50.0) < 1e-6 && lengths.Count == 1);
        }

        // ---------------------------------------------------------------- lo que NO se comparte

        [Fact]
        public void SharedCellGrid_DoesNotForceSideDepthLevelsOrPalletIntentToMatch()
        {
            var design = Design(levels: 3, deepA: 6, deepB: 3);
            Cells(design.Structure.Fronts[0], 3, 0, beamOverride: 150.0, palletFront: 60.0);
            design.SideB.Fronts[0].LoadLevels = 1;

            var system = Resolve(design);
            var localA = system.Composite.Of(PushBackSide.A).Local.Structure;
            var localB = system.Composite.Of(PushBackSide.B).Local.Structure;

            AssertOneGrid(system);

            // Niveles, fondos y la intencion de tarima siguen siendo de cada lado.
            Assert.NotEqual(
                DynamicFrontActivation.EffectiveLoadLevels(localA.Fronts[0]),
                DynamicFrontActivation.EffectiveLoadLevels(localB.Fronts[0]));
            Assert.NotEqual(localA.PalletsDeep, localB.PalletsDeep);
            Assert.Equal(60.0, design.Structure.Fronts[0].Levels[0].PalletFront);
            Assert.Empty(design.SideB.Fronts[0].Levels.Where(level => level.PalletFront.HasValue));
        }
    }
}
