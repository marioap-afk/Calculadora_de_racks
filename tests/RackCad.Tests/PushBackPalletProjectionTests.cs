using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D3, contrato del dueño) — LAS TARIMAS DE UN CORTE COMPUESTO SON LA PROYECCION DE UNA CAMA FISICA.
    ///
    /// <para>
    /// Una celda dibuja tarima en un plano si la cama que la sirve tiene un apoyo ahi y la celda lo pide. Ni el
    /// nombre del corte, ni el lado, ni el sistema local deciden nada: <see cref="PushBackRunSupports"/> dice el
    /// papel, <see cref="PushBackRuns"/> dice la cama, y la intencion de I-41 sigue intacta.
    /// </para>
    /// </summary>
    public class PushBackPalletProjectionTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static readonly PushBackSide[] Sides = { PushBackSide.A, PushBackSide.B };

        private static readonly PushBackFrontalEnd[] Ends =
            { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior };

        /// <summary>El rack compuesto de la ronda: 2 ranuras x 2 niveles, sin hueco, con tarimas pedidas.</summary>
        private static PushBackSystem Build(
            PushBackCellTopology topology,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            int? shortDepthA = null,
            int? hideLevel = null,
            double? palletHeightOfLevel2 = null)
        {
            const int levels = 2;
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: levels, levelsB: levels, gap: 0.0);
            design.Composite.DefaultTopology = topology;
            design.Composite.DefaultDirection = direction;

            for (var slot = 0; slot < design.Fronts.Count; slot++)
            {
                for (var level = 0; level < levels; level++)
                {
                    var draw = hideLevel != level;
                    design.Fronts[slot].DrawPallets.Add(draw);
                    design.SideB.FrontConfigs[slot]?.DrawPallets.Add(draw);
                    if (shortDepthA.HasValue)
                    {
                        design.Fronts[slot].PalletsDeepOverrides.Add(shortDepthA.Value);
                    }
                }
            }

            if (palletHeightOfLevel2.HasValue)
            {
                for (var level = 0; level < levels; level++)
                {
                    design.Structure.Fronts[0].Levels.Add(new DynamicRackLevelDesign
                    {
                        PalletHeight = level == 1 ? palletHeightOfLevel2 : null,
                    });
                }
            }

            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<HeaderBlockInstance> Pallets(
            PushBackSystem system, PushBackSide side, PushBackFrontalEnd end)
            => new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side)
                .Flatten().Instances
                .Where(PushBackPalletProjection.IsPallet)
                .ToList();

        private static IReadOnlyList<ResolvedPalletRow> Rows(
            PushBackSystem system, PushBackSide side, PushBackFrontalEnd end)
            => PushBackPalletProjection.Resolve(system, Catalog, side, end);

        /// <summary>Las camas que ese plano DEBE mostrar, preguntado directamente a las autoridades.</summary>
        private static IReadOnlyList<PushBackRun> Expected(
            PushBackSystem system, PushBackSide side, PushBackFrontalEnd end)
        {
            var runs = PushBackRuns.Resolve(system);
            return runs.Runs
                .Where(run => PushBackRunSupports.At(system, runs, run, side, end) != PushBackSupportRole.None)
                .Where(run => run.Source.DrawPalletAt(run.SourceFrontIndex, run.SourceLevel - 1))
                .ToList();
        }

        // ---------------------------------------------------------------- el gate de apoyo

        [Fact]
        public void CompositePallets_OnlyAppearWhereRunHasSupportAtCut()
        {
            foreach (var topology in new[]
                     {
                         PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                         PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida,
                     })
            {
                var system = Build(topology);
                foreach (var side in Sides)
                foreach (var end in Ends)
                {
                    var expected = Expected(system, side, end);
                    var rows = Rows(system, side, end);

                    Assert.Equal(
                        expected.Select(PushBackPalletProjection.RunIdentityOf).OrderBy(id => id, StringComparer.Ordinal),
                        rows.Select(row => row.RunIdentity).OrderBy(id => id, StringComparer.Ordinal));
                    Assert.Equal(rows.Count, Pallets(system, side, end).Count);   // una calle por celda en este rack
                }
            }
        }

        [Fact]
        public void CompositePallets_SupportNoneNeverProjectsPallet()
        {
            var system = Build(PushBackCellTopology.SoloA);
            var runs = PushBackRuns.Resolve(system);

            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                Assert.All(Rows(system, side, end), row => Assert.NotEqual(PushBackSupportRole.None, row.Role));
                if (runs.Runs.All(run => PushBackRunSupports.At(system, runs, run, side, end) == PushBackSupportRole.None))
                {
                    Assert.Empty(Pallets(system, side, end));
                }
            }
        }

        [Fact]
        public void CompositePallets_ShortRunDoesNotProjectPastItsEnd()
        {
            // Las camas de A ocupan 2 de las 4 posiciones de su estructura: su ALTO termina ANTES de la linea
            // interior, asi que ese corte no tiene apoyo de ellas y no puede mostrar su tarima.
            var system = Build(PushBackCellTopology.Encontradas, shortDepthA: 2);
            var runs = PushBackRuns.Resolve(system);
            var shortRuns = runs.Runs.Where(run => run.LowSide == PushBackSide.A).ToList();

            Assert.NotEmpty(shortRuns);
            Assert.All(shortRuns, run => Assert.Equal(
                PushBackSupportRole.None,
                PushBackRunSupports.At(system, runs, run, PushBackSide.A, PushBackFrontalEnd.Posterior)));

            Assert.Empty(Rows(system, PushBackSide.A, PushBackFrontalEnd.Posterior));
            Assert.Empty(Pallets(system, PushBackSide.A, PushBackFrontalEnd.Posterior));

            // Y no se han mudado al siguiente plano: solo estan donde SI tienen apoyo, su propio pasillo.
            var identities = shortRuns.Select(PushBackPalletProjection.RunIdentityOf).ToHashSet(StringComparer.Ordinal);
            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                var mine = Rows(system, side, end).Where(row => identities.Contains(row.RunIdentity)).ToList();
                var isOwnAisle = side == PushBackSide.A && end == PushBackFrontalEnd.EntradaSalida;
                Assert.Equal(isOwnAisle ? shortRuns.Count : 0, mine.Count);
            }
        }

        // ---------------------------------------------------------------- corrida: UNA cama, UNA fila

        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void CompositePallets_CorridaIsNotDuplicatedByLocalSides(PushBackRunDirection direction)
        {
            var system = Build(PushBackCellTopology.Corrida, direction);
            var runs = PushBackRuns.Resolve(system);

            // Una celda corrida es UNA cama, aunque la miren los dos lados.
            Assert.Equal(4, runs.Runs.Count);   // 2 ranuras x 2 niveles
            Assert.All(runs.Runs, run => Assert.Equal(PushBackCellTopology.Corrida, run.Topology));

            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                var rows = Rows(system, side, end);
                Assert.Equal(rows.Count, rows.Select(row => row.RunIdentity).Distinct(StringComparer.Ordinal).Count());
                Assert.Equal(rows.Count, rows.Select(row => (row.Slot, row.Level)).Distinct().Count());
                Assert.Equal(rows.Count, Pallets(system, side, end).Count);
            }
        }

        [Fact]
        public void CompositePallets_CorridaAtoBIsNotDuplicatedByLocalSides()
            => CompositePallets_CorridaIsNotDuplicatedByLocalSides(PushBackRunDirection.AToB);

        [Fact]
        public void CompositePallets_CorridaBtoAIsNotDuplicatedByLocalSides()
            => CompositePallets_CorridaIsNotDuplicatedByLocalSides(PushBackRunDirection.BToA);

        // ---------------------------------------------------------------- encontradas: DOS camas

        [Fact]
        public void CompositePallets_EncontradasPreserveDistinctRunIdentity()
        {
            var system = Build(PushBackCellTopology.Encontradas);
            var all = new List<ResolvedPalletRow>();
            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                all.AddRange(Rows(system, side, end));
            }

            // La celda (0,1) tiene DOS camas: cada una aparece en sus dos planos, con identidad propia.
            var cell = all.Where(row => row.Slot == 0 && row.Level == 1).ToList();
            Assert.Equal(4, cell.Count);
            Assert.Equal(2, cell.Select(row => row.RunIdentity).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(4, cell.Select(row => row.Identity).Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void CoincidentProjectionsOfDistinctRunsAreNotCollapsed()
        {
            // Con hueco cero y los dos lados iguales, la cama de A vista desde su pasillo y la de B vista desde el
            // suyo caen en la MISMA X y la MISMA Y. Son dos piezas fisicas distintas: no se funden en una.
            var system = Build(PushBackCellTopology.Encontradas);
            var fromA = Rows(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida)
                .First(row => row.Slot == 0 && row.Level == 1);
            var fromB = Rows(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida)
                .First(row => row.Slot == 0 && row.Level == 1);

            Assert.Equal(fromA.AnchorX, fromB.AnchorX, 6);
            Assert.Equal(fromA.SupportY, fromB.SupportY, 6);
            Assert.NotEqual(fromA.RunIdentity, fromB.RunIdentity);
            Assert.NotEqual(fromA.Identity, fromB.Identity);
        }

        // ---------------------------------------------------------------- un lado no inventa el otro

        [Fact]
        public void CompositePallets_SoloADoesNotCreateBSideProjection()
        {
            var system = Build(PushBackCellTopology.SoloA);

            Assert.NotEmpty(Pallets(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida));
            foreach (var end in Ends)
            {
                Assert.Empty(Rows(system, PushBackSide.B, end));
                Assert.Empty(Pallets(system, PushBackSide.B, end));
            }
        }

        [Fact]
        public void CompositePallets_SoloBDoesNotCreateASideProjection()
        {
            var system = Build(PushBackCellTopology.SoloB);

            Assert.NotEmpty(Pallets(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida));
            foreach (var end in Ends)
            {
                Assert.Empty(Rows(system, PushBackSide.A, end));
                Assert.Empty(Pallets(system, PushBackSide.A, end));
            }
        }

        // ---------------------------------------------------------------- la intencion de I-41, intacta

        [Fact]
        public void CompositePallets_DrawPalletFalseIsRespected()
        {
            // Nivel 1 oculto, nivel 2 visible: ni una sola proyeccion del nivel oculto, en ninguno de los cuatro
            // cortes, y las del visible siguen ahi.
            var system = Build(PushBackCellTopology.Encontradas, hideLevel: 0);

            var rows = new List<ResolvedPalletRow>();
            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                rows.AddRange(Rows(system, side, end));
            }

            Assert.NotEmpty(rows);
            Assert.All(rows, row => Assert.Equal(2, row.Level));
        }

        [Fact]
        public void CompositePallets_PerCellPalletHeightIsPreserved()
        {
            // El nivel 2 de la ranura 0 del lado A lleva una tarima mas alta: su proyeccion conserva ESA altura y
            // no la del nivel vecino, la del otro lado ni un default.
            var system = Build(PushBackCellTopology.Encontradas, palletHeightOfLevel2: 72.0);
            var rows = Rows(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida);

            Assert.Equal(72.0, rows.Single(row => row.Slot == 0 && row.Level == 2).PalletHeight, 6);
            Assert.Equal(60.0, rows.Single(row => row.Slot == 0 && row.Level == 1).PalletHeight, 6);
            Assert.Equal(60.0, rows.Single(row => row.Slot == 1 && row.Level == 2).PalletHeight, 6);

            var drawn = Pallets(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida)
                .Select(instance => instance.DynamicParameters.TryGetValue("ALTURA", out var value)
                    ? Convert.ToDouble(value)
                    : 0.0)
                .ToList();
            Assert.Contains(72.0, drawn);
            Assert.Contains(60.0, drawn);
        }

        // ---------------------------------------------------------------- la elevacion es la de la cama fisica

        [Fact]
        public void CompositePallets_UseResolvedRunElevation()
        {
            // Una corrida es UNA cama con UNA pendiente continua: su apoyo sube monotonamente del pasillo bajo al
            // alto, pasando por la linea interior. Con la cama LOCAL de cada lado —4 posiciones, no 8— el corte del
            // extremo alto quedaria a la altura del final de una cama corta, muy por debajo.
            var system = Build(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            var low = Rows(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida)
                .Single(row => row.Slot == 0 && row.Level == 1);
            var middle = Rows(system, PushBackSide.A, PushBackFrontalEnd.Posterior)
                .Single(row => row.Slot == 0 && row.Level == 1);
            var high = Rows(system, PushBackSide.B, PushBackFrontalEnd.EntradaSalida)
                .Single(row => row.Slot == 0 && row.Level == 1);

            Assert.Equal(PushBackSupportRole.Low, low.Role);
            Assert.Equal(PushBackSupportRole.Intermediate, middle.Role);
            Assert.Equal(PushBackSupportRole.High, high.Role);
            Assert.True(low.SupportY < middle.SupportY, "el apoyo intermedio esta por encima del bajo");
            Assert.True(middle.SupportY < high.SupportY, "el apoyo alto esta por encima del intermedio");

            // Y la altura del extremo alto es la de la CAMA CORRIDA, no la de la cama local del lado.
            var runs = PushBackRuns.Resolve(system);
            var run = runs.Runs.Single(candidate => candidate.Slot == 0 && candidate.Level == 1);
            var supportLocalY = PushBackTarimaPlacement.SupportLocalY(Catalog);
            var runAxis = PushBackFlowBedGeometry.Resolve(run.Source, Catalog, run.Front())
                .Single(candidate => candidate.LevelNumber == run.SourceLevel);
            Assert.Equal(
                PushBackTarimaPlacement.SupportYAt(runAxis, supportLocalY, runAxis.HighMate.X),
                high.SupportY,
                6);

            var localB = system.Composite.Of(PushBackSide.B).Local;
            var localAxis = PushBackFlowBedGeometry
                .Resolve(localB, Catalog, localB.Structure.Fronts[0])
                .Single(candidate => candidate.LevelNumber == 1);
            Assert.NotEqual(
                PushBackTarimaPlacement.SupportYAt(localAxis, supportLocalY, localAxis.HighMate.X),
                high.SupportY,
                6);
        }

        [Fact]
        public void CompositePallets_AllFourCutsFollowRunSupportAuthority()
        {
            foreach (var system in new[]
                     {
                         Build(PushBackCellTopology.SoloA),
                         Build(PushBackCellTopology.SoloB),
                         Build(PushBackCellTopology.Corrida, PushBackRunDirection.AToB),
                         Build(PushBackCellTopology.Corrida, PushBackRunDirection.BToA),
                         Build(PushBackCellTopology.Encontradas),
                         Build(PushBackCellTopology.Encontradas, shortDepthA: 2),
                     })
            {
                foreach (var side in Sides)
                foreach (var end in Ends)
                {
                    Assert.Equal(Expected(system, side, end).Count, Pallets(system, side, end).Count);
                }
            }
        }

        // ---------------------------------------------------------------- lo que NO cambia

        [Fact]
        public void SingleSidedRack_KeepsItsOwnPalletRows()
        {
            // Un rack de un solo sentido no pasa por esta autoridad: su corte sigue siendo el de I-41.
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
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4, DrawPallets = { true, true } });

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.False(system.IsComposite);
            Assert.Empty(PushBackPalletProjection.Resolve(system, Catalog, PushBackSide.A, PushBackFrontalEnd.EntradaSalida));
            Assert.Equal(2, Pallets(system, PushBackSide.A, PushBackFrontalEnd.EntradaSalida).Count);
        }

        [Fact]
        public void PalletsNeverReachTheBom()
        {
            var system = Build(PushBackCellTopology.Encontradas);
            var bom = PushBackBomBuilder.Build(system, Catalog);

            Assert.DoesNotContain(bom.Lines, line => string.Equals(
                line.ProfileId, RackCad.Domain.Systems.Selective.SelectiveRackDefaults.PalletPieceId,
                StringComparison.OrdinalIgnoreCase));
        }
    }
}
