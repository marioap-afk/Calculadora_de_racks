using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D4, contrato del dueño) — UN LARGUERO DE EXTREMO SE BUSCA CON LA MISMA IDENTIDAD CON LA QUE SE
    /// COLOCA: su FRENTE, su nivel y el extremo del rack. Nunca con la posicion de una coleccion interpretada como
    /// si fuera un poste, y nunca por cercania.
    ///
    /// <para>
    /// El fixture es deliberadamente HETEROGENEO —primer nivel distinto en cada frente de cada lado— porque es lo
    /// unico que separa las dos identidades: con todos los frentes a la misma altura, preguntar por el frente y
    /// preguntar por el poste dan el mismo numero y el defecto es invisible.
    /// </para>
    /// </summary>
    public class PushBackEndBeamIdentityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static readonly PushBackSide[] Sides = { PushBackSide.A, PushBackSide.B };

        private static readonly PushBackFrontalEnd[] Ends =
            { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior };

        /// <summary>
        /// 3 ranuras x 2 niveles, primer nivel 4/16/28 en A y 10/22/34 en B, y topologias MIXTAS: la ranura 0 es
        /// Solo A, la 1 una corrida B-&gt;A y la 2 Solo B. Asi cada corte tiene celdas que debe mostrar y celdas que
        /// debe excluir, que es donde un fallo de identidad se convierte en una pieza fantasma.
        /// </summary>
        private static PushBackSystem Heterogeneous(bool heterogeneous = true)
        {
            const int levels = 2;
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 3, slotsB: 3, deepA: 4, deepB: 4, levelsA: levels, levelsB: levels, gap: 0.0);

            var heightsA = heterogeneous ? new[] { 4.0, 16.0, 28.0 } : new[] { 4.0, 4.0, 4.0 };
            var heightsB = heterogeneous ? new[] { 10.0, 22.0, 34.0 } : new[] { 10.0, 10.0, 10.0 };
            for (var slot = 0; slot < 3; slot++)
            {
                design.Structure.Fronts[slot].FirstLevelHeight = heightsA[slot];
                design.SideB.Fronts[slot].FirstLevelHeight = heightsB[slot];
            }

            for (var level = 0; level < levels; level++)
            {
                design.Composite.Topologies.Add(new PushBackTopologyCell
                {
                    Frente = 0, Level = level, Topology = PushBackCellTopology.SoloA,
                });
                design.Composite.Topologies.Add(new PushBackTopologyCell
                {
                    Frente = 1, Level = level,
                    Topology = PushBackCellTopology.Corrida, Direction = PushBackRunDirection.BToA,
                });
                design.Composite.Topologies.Add(new PushBackTopologyCell
                {
                    Frente = 2, Level = level, Topology = PushBackCellTopology.SoloB,
                });
            }

            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<HeaderBlockInstance> EndBeams(HeaderRunPlan plan)
            => plan.Flatten().Instances.Where(PushBackPlanComposer.IsDynamicEndBeam).ToList();

        private static IReadOnlyList<HeaderBlockInstance> RearBeams(HeaderRunPlan plan)
            => plan.Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Beam
                                   && string.Equals(
                                       instance.PieceId, PushBackDefaults.HighEndBeamCatalogId,
                                       StringComparison.OrdinalIgnoreCase))
                .ToList();

        private static int Unidentified(PushBackSystem system, PushBackSide side)
        {
            var local = system.Composite.Of(side).Local;
            var context = PushBackElevations.Context(local, Catalog);
            var instances = new PushBackSystemFrontalBuilder()
                .BuildPlan(local, Catalog, PushBackFrontalEnd.EntradaSalida)
                .Flatten().Instances;
            return PushBackSystemFrontalBuilder.UnidentifiedEndBeams(local.Structure, Catalog, context, instances);
        }

        private static IReadOnlyList<PushBackRun> RunsWith(
            PushBackSystem system, PushBackSide side, PushBackFrontalEnd end, PushBackSupportRole role)
        {
            var runs = PushBackRuns.Resolve(system);
            return runs.Runs.Where(run => PushBackRunSupports.At(system, runs, run, side, end) == role).ToList();
        }

        private static double ColumnXOf(PushBackSystem system, PushBackSide side, int slot)
        {
            var view = system.Composite.Of(side);
            var layout = DynamicFrontGeometry.Compute(view.Local.Structure, Catalog);
            return DynamicEndBeamIdentity.ColumnX(layout, view.LocalIndexBySlot[slot]);
        }

        // ---------------------------------------------------------------- el defecto medido (H1)

        [Fact]
        public void CompositeCuts_HaveNoUnidentifiedEndBeams_WithHeterogeneousFirstLevels()
        {
            var system = Heterogeneous();

            foreach (var side in Sides)
            {
                Assert.Equal(0, Unidentified(system, side));
            }
        }

        [Fact]
        public void CompositeEndBeamIdentity_DoesNotUseFrontIndexAsPostIdentity()
        {
            var system = Heterogeneous();

            foreach (var side in Sides)
            {
                var local = system.Composite.Of(side).Local;
                var context = PushBackElevations.Context(local, Catalog);
                var differ = 0;
                for (var position = 0; position < local.Structure.Fronts.Count; position++)
                {
                    var front = local.Structure.Fronts[position];
                    foreach (var level in DynamicFrontGeometry.LoadBeamLevels(local.Structure, front))
                    {
                        var byFront = context.OrFront(front.Index, level.LevelNumber, level.ExitElevation);
                        var byPost = context.OrPost(position, level.LevelNumber, level.ExitElevation);
                        if (Math.Abs(byFront - byPost) > DynamicEndBeamIdentity.Tolerance)
                        {
                            differ++;
                        }

                        // La identidad canonica es la del FRENTE: es la que coloco la pieza.
                        Assert.Equal(
                            byFront,
                            DynamicEndBeamIdentity.ElevationOf(context, front, level, DynamicRackEnd.Exit),
                            6);
                    }
                }

                // Y este fixture SI las separa: si dejara de hacerlo, la prueba no probaria nada.
                Assert.True(differ > 0, "el fixture debe separar la identidad de frente de la de poste");
            }
        }

        [Fact]
        public void EndBeamLookup_UsesTheSameSemanticIdentityAsPlacement()
        {
            var system = Heterogeneous();

            foreach (var side in Sides)
            {
                var local = system.Composite.Of(side).Local;
                var structure = local.Structure;
                var layout = DynamicFrontGeometry.Compute(structure, Catalog);
                var context = PushBackElevations.Context(local, Catalog);
                var placed = new PushBackSystemFrontalBuilder()
                    .BuildPlan(local, Catalog, PushBackFrontalEnd.EntradaSalida)
                    .Flatten().Instances
                    .Where(PushBackPlanComposer.IsDynamicEndBeam)
                    .ToList();

                Assert.NotEmpty(placed);
                var lookedUp = new List<string>();
                foreach (var instance in placed)
                {
                    var match = DynamicEndBeamIdentity.Match(
                        structure, layout, context, instance.Insertion, DynamicRackEnd.Exit);
                    Assert.True(match.Matched, "todo larguero colocado se localiza con su identidad");
                    lookedUp.Add(match.Identity);
                }

                // Las claves del PLACEMENT: frente x nivel, con la identidad del frente, no su posicion.
                var placement = DynamicEndBeamIdentity
                    .KeysOf(structure, layout, context, DynamicRackEnd.Exit)
                    .Select(key => key.Identity)
                    .ToList();

                Assert.Equal(
                    placement.OrderBy(id => id, StringComparer.Ordinal),
                    lookedUp.OrderBy(id => id, StringComparer.Ordinal));
                Assert.All(lookedUp, id => Assert.StartsWith("F", id, StringComparison.Ordinal));
            }
        }

        [Fact]
        public void HeterogeneousComposite_DoesNotRelyOnFailOpenToKeepEndBeams()
        {
            // El contrato observable: cada corte lleva EXACTAMENTE los largueros IN/OUT de las camas cuyo extremo
            // BAJO cae en el. Con la busqueda equivocada esos largueros no se identificaban, el filtro por celda no
            // podia retirarlos y sobrevivian por fail-open: piezas fantasma en cortes que no las llevan.
            var system = Heterogeneous();

            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                var expected = RunsWith(system, side, end, PushBackSupportRole.Low);
                var beams = EndBeams(PushBackCompositeFrontal.Build(system, Catalog, end, side));

                Assert.Equal(expected.Count, beams.Count);
            }
        }

        [Fact]
        public void HeterogeneousComposite_MatchesTheHomogeneousContract()
        {
            // Mismo rack, misma topologia, unica diferencia el primer nivel: los dos deben contar lo mismo.
            var heterogeneous = Heterogeneous();
            var homogeneous = Heterogeneous(heterogeneous: false);

            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                Assert.Equal(
                    EndBeams(PushBackCompositeFrontal.Build(homogeneous, Catalog, end, side)).Count,
                    EndBeams(PushBackCompositeFrontal.Build(heterogeneous, Catalog, end, side)).Count);
            }
        }

        // ---------------------------------------------------------------- LOW y HIGH, contra su cama

        [Fact]
        public void CompositeEndBeamIdentity_LowBeamMatchesItsRunSource()
        {
            var system = Heterogeneous();

            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                var beams = EndBeams(PushBackCompositeFrontal.Build(system, Catalog, end, side));
                var expected = RunsWith(system, side, end, PushBackSupportRole.Low);

                Assert.Equal(expected.Count, beams.Count);
                foreach (var run in expected)
                {
                    var elevation = PushBackElevations.LowInsertions(run.Source, Catalog, run.Front())[run.SourceLevel];
                    var columnX = ColumnXOf(system, side, run.Slot);
                    Assert.Contains(beams, beam =>
                        Math.Abs(beam.Insertion.X - columnX) <= DynamicEndBeamIdentity.Tolerance
                        && Math.Abs(beam.Insertion.Y - elevation) <= DynamicEndBeamIdentity.Tolerance);
                }
            }
        }

        [Fact]
        public void CompositeEndBeamIdentity_HighBeamMatchesItsRunSource()
        {
            var system = Heterogeneous();

            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                var beams = RearBeams(PushBackCompositeFrontal.Build(system, Catalog, end, side));
                var expected = RunsWith(system, side, end, PushBackSupportRole.High);

                Assert.Equal(expected.Count, beams.Count);
                foreach (var run in expected)
                {
                    var elevation = PushBackElevations.HighInsertions(run.Source, Catalog, run.Front())[run.SourceLevel];
                    var columnX = ColumnXOf(system, side, run.Slot);
                    Assert.Contains(beams, beam =>
                        Math.Abs(beam.Insertion.X - columnX) <= DynamicEndBeamIdentity.Tolerance
                        && Math.Abs(beam.Insertion.Y - elevation) <= DynamicEndBeamIdentity.Tolerance);
                }
            }
        }

        [Fact]
        public void CompositeEndBeamIdentity_MixedTopologiesRemainDistinct()
        {
            var system = Heterogeneous();
            var runs = PushBackRuns.Resolve(system);

            // Las tres ranuras tienen topologias distintas y ninguna se confunde con otra.
            Assert.Equal(
                new[] { PushBackCellTopology.SoloA, PushBackCellTopology.Corrida, PushBackCellTopology.SoloB },
                runs.Runs.OrderBy(run => run.Slot).Select(run => run.Topology).Distinct().ToArray());

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var side in Sides)
            foreach (var end in Ends)
            {
                var local = system.Composite.Of(side).Local;
                var layout = DynamicFrontGeometry.Compute(local.Structure, Catalog);
                var context = PushBackElevations.Context(local, Catalog);
                foreach (var beam in EndBeams(PushBackCompositeFrontal.Build(system, Catalog, end, side)))
                {
                    var match = DynamicEndBeamIdentity.Match(
                        local.Structure, layout, context, beam.Insertion, DynamicRackEnd.Exit);
                    Assert.True(match.Matched);
                    Assert.True(
                        seen.Add(FormattableString.Invariant($"{side}|{end}|{match.Identity}")),
                        "cada larguero de un corte tiene una identidad propia");
                }
            }

            Assert.NotEmpty(seen);
        }

        // ---------------------------------------------------------------- lo que NO cambia

        [Fact]
        public void SingleSidedRack_KeepsItsEndBeams()
        {
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
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, FirstLevelHeight = 4.0,
            });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, FirstLevelHeight = 20.0,
            });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });

            var system = new PushBackResolver(Catalog).Resolve(design);
            var plan = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida);

            // Dos frentes x dos niveles: el rack de un solo sentido sigue dibujando sus cuatro largueros, y cada uno
            // se identifica con su propia elevacion aunque los dos frentes arranquen a alturas distintas.
            var beams = EndBeams(plan);
            Assert.Equal(4, beams.Count);
            Assert.Equal(0, PushBackSystemFrontalBuilder.UnidentifiedEndBeams(
                system.Structure, Catalog, PushBackElevations.Context(system, Catalog), plan.Flatten().Instances));
        }
    }
}
