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
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-G1, contrato del dueño) — A Y B COMPARTEN UNA SOLA RETICULA TRANSVERSAL FISICA.
    ///
    /// <para>
    /// El ancho de una bahia y sus calles los declaran los dos lados, pero la bahia es UNA: no puede medir una cosa
    /// vista desde A y otra vista desde B. Medido antes del cambio, con un larguero manual de 100" en A y 120" en B:
    /// la estructura compuesta ponia su segunda linea en 123.49, el marco local de A en 103.49 y el de B en 123.49
    /// —tres geometrias para una sola retícula—. Los cortes heredaban la local, mientras las botas y las defensas se
    /// anclan al frame COMPUESTO: una bota resuelta en X=123.49 caia a 20" del poste mas cercano del corte de A.
    /// </para>
    /// <para>
    /// Lo que se comparte es SOLO lo transversal. Niveles, elevaciones, fondos y tarimas siguen siendo de cada lado.
    /// </para>
    /// </summary>
    public class PushBackSharedTransverseGridTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(
            int slots = 2, int levelsA = 2, int levelsB = 2, int deepA = 4, int deepB = 4)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slots, slotsB: slots, deepA: deepA, deepB: deepB, levelsA: levelsA, levelsB: levelsB, gap: 0.0);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            design.DefensePieceId = "DEFENSA_MONTACARGAS";
            design.SideB.DefensePieceId = "DEFENSA_MONTACARGAS";
            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design)
            => new PushBackResolver(Catalog).Resolve(design);

        private static IReadOnlyList<double> Posts(DynamicRackSystem structure)
            => structure == null
                ? Array.Empty<double>()
                : DynamicFrontGeometry.Compute(structure, Catalog).PostPositions
                    .Select(x => Math.Round(x, 6))
                    .ToList();

        private static IReadOnlyList<double> PostsOf(PushBackSystem system, PushBackSide side)
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

        private static bool SitsOnAPost(double anchor, IReadOnlyList<double> posts)
            => posts.Any(post => Math.Abs(post - anchor) <= 1e-6);

        // ---------------------------------------------------------------- las tres geometrias

        [Fact]
        public void CompositeGrid_LocalA_LocalB_AndCompositeHaveSamePostPositions()
        {
            foreach (var design in new[]
                     {
                         Design(),                                             // control
                         WithFrontOverride(Design(), 0, 100.0, 120.0),         // los dos lados, distintos
                         WithFrontOverride(Design(), 0, 100.0, null),          // solo A
                         WithFrontOverride(Design(), 0, null, 120.0),          // solo B
                         WithPalletCount(Design(), 0, 3, 1),                   // calles distintas
                     })
            {
                var system = Resolve(design);
                var composite = Posts(system.Structure);
                var localA = PostsOf(system, PushBackSide.A);
                var localB = PostsOf(system, PushBackSide.B);

                Assert.NotEmpty(composite);
                Assert.Equal(composite, localA);   // linea por linea, no solo el conteo
                Assert.Equal(composite, localB);
            }
        }

        private static PushBackDesign WithFrontOverride(
            PushBackDesign design, int slot, double? sideA, double? sideB)
        {
            design.Structure.Fronts[slot].BeamLengthOverride = sideA;
            design.SideB.Fronts[slot].BeamLengthOverride = sideB;
            return design;
        }

        private static PushBackDesign WithPalletCount(PushBackDesign design, int slot, int sideA, int sideB)
        {
            design.Structure.Fronts[slot].PalletCount = sideA;
            design.SideB.Fronts[slot].PalletCount = sideB;
            return design;
        }

        // ---------------------------------------------------------------- la regla de envolvente

        [Fact]
        public void CompositeGrid_UnilateralBeamLengthOverrideUsesSharedEnvelope()
        {
            // Solo A declara 100": la bahia mide 100 para los dos, no 100 en A y el automatico en B.
            var onlyA = Resolve(WithFrontOverride(Design(), 0, 100.0, null));
            Assert.Equal(100.0, onlyA.Structure.Fronts[0].BeamLength, 6);
            Assert.Equal(100.0, onlyA.Composite.Of(PushBackSide.B).Local.Structure.Fronts[0].BeamLength, 6);

            // Y simetrico: solo B declara 120".
            var onlyB = Resolve(WithFrontOverride(Design(), 0, null, 120.0));
            Assert.Equal(120.0, onlyB.Structure.Fronts[0].BeamLength, 6);
            Assert.Equal(120.0, onlyB.Composite.Of(PushBackSide.A).Local.Structure.Fronts[0].BeamLength, 6);
        }

        [Fact]
        public void CompositeGrid_TwoOverridesUseTheLargest_NotAnAverage()
        {
            // Con los dos lados declarando, gobierna la MAYOR demanda: es la regla vigente de Compose, no un promedio.
            var system = Resolve(WithFrontOverride(Design(), 0, 100.0, 120.0));

            Assert.Equal(120.0, system.Structure.Fronts[0].BeamLength, 6);
            Assert.Equal(120.0, system.Composite.Of(PushBackSide.A).Local.Structure.Fronts[0].BeamLength, 6);
            Assert.Equal(120.0, system.Composite.Of(PushBackSide.B).Local.Structure.Fronts[0].BeamLength, 6);
        }

        [Fact]
        public void CompositeGrid_AsymmetricPalletCountsUseSharedPhysicalGrid()
        {
            // Las CALLES tambien gobiernan la retícula: 3 calles en A y 1 en B son 3 calles fisicas en la bahia.
            var system = Resolve(WithPalletCount(Design(), 0, 3, 1));

            Assert.Equal(3, system.Structure.Fronts[0].PalletCount);
            Assert.Equal(3, system.Composite.Of(PushBackSide.A).Local.Structure.Fronts[0].PalletCount);
            Assert.Equal(3, system.Composite.Of(PushBackSide.B).Local.Structure.Fronts[0].PalletCount);
            Assert.Equal(Posts(system.Structure), PostsOf(system, PushBackSide.B));
        }

        // ---------------------------------------------------------------- BOM

        [Fact]
        public void CompositeBom_SharedBayDoesNotUseDifferentTransverseBeamLengthsPerSide()
        {
            var system = Resolve(WithFrontOverride(Design(), 0, 100.0, 120.0));
            var bom = PushBackBomBuilder.Build(system, Catalog);

            // La bahia comparte su ancho: la longitud de 100" —la que solo A declaraba— no puede comprarse.
            var lengths = bom.Lines
                .Where(line => line.Category != null && line.Category.IndexOf("arguero", StringComparison.Ordinal) >= 0)
                .Select(line => Math.Round(line.Length, 2))
                .Distinct()
                .ToList();

            Assert.NotEmpty(lengths);
            Assert.DoesNotContain(100.0, lengths);
            Assert.Contains(lengths, length => Math.Abs(length - 120.0) < 1e-6);
        }

        // ---------------------------------------------------------------- cortes y seguridad

        [Fact]
        public void CompositeCut_DefenseAnchorMatchesItsPhysicalPostOnSharedGrid()
        {
            var system = Resolve(WithFrontOverride(Design(), 0, 100.0, 120.0));
            var defenses = PushBackDefensePlan.Resolve(system, Catalog);

            Assert.NotEmpty(defenses);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var posts = CutPosts(system, side);
                Assert.NotEmpty(posts);
                foreach (var defense in defenses.Where(defense => defense.Side == side))
                {
                    Assert.True(
                        SitsOnAPost(defense.LineX, posts),
                        FormattableString.Invariant(
                            $"la defensa de {side} en X={defense.LineX:0.####} no cae sobre ningun poste del corte"));
                }
            }
        }

        [Fact]
        public void CompositeCut_BootAnchorMatchesItsPhysicalPostOnSharedGrid()
        {
            var system = Resolve(WithFrontOverride(Design(), 0, 100.0, 120.0));
            var boots = PushBackBootPlan.Resolve(system, Catalog);

            Assert.NotEmpty(boots);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var posts = CutPosts(system, side);
                foreach (var boot in boots.Where(boot => boot.Side == side))
                {
                    Assert.True(
                        SitsOnAPost(boot.LineX, posts),
                        FormattableString.Invariant(
                            $"la bota de {side} en X={boot.LineX:0.####} no cae sobre ningun poste del corte"));
                }
            }
        }

        [Fact]
        public void CompositeCuts_DrawTheSameGridOnBothSides()
        {
            var system = Resolve(WithFrontOverride(Design(), 0, 100.0, 120.0));

            Assert.Equal(CutPosts(system, PushBackSide.A), CutPosts(system, PushBackSide.B));
        }

        // ---------------------------------------------------------------- lo que NO se comparte

        [Fact]
        public void SharedGrid_DoesNotForceSideLevelsOrDepthsToMatch()
        {
            // Retícula transversal compartida NO significa dos lados iguales: niveles y fondos siguen siendo suyos.
            var design = Design(levelsA: 3, levelsB: 1, deepA: 6, deepB: 3);
            design.Structure.Fronts[0].BeamLengthOverride = 100.0;

            var system = Resolve(design);
            var localA = system.Composite.Of(PushBackSide.A).Local.Structure;
            var localB = system.Composite.Of(PushBackSide.B).Local.Structure;

            Assert.Equal(Posts(system.Structure), Posts(localA));
            Assert.Equal(Posts(system.Structure), Posts(localB));

            Assert.NotEqual(
                DynamicFrontActivation.EffectiveLoadLevels(localA.Fronts[0]),
                DynamicFrontActivation.EffectiveLoadLevels(localB.Fronts[0]));
            Assert.NotEqual(localA.PalletsDeep, localB.PalletsDeep);
        }

        [Fact]
        public void SingleSidedRack_KeepsItsOwnGrid()
        {
            // Un rack de un solo sentido no tiene con quien compartir: su retícula es la de siempre.
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new RackCad.Domain.Systems.Shared.PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0,
                },
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 2, LoadLevels = 2, PalletsDeep = 4, BeamLengthOverride = 90.0,
            });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });

            var system = Resolve(design);

            Assert.False(system.IsComposite);
            Assert.Equal(90.0, system.Structure.Fronts[0].BeamLength, 6);
        }
    }
}
