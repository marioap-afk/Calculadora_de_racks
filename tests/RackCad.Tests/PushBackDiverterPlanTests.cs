using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B-D1, contrato del dueño) — LA AUTORIDAD FISICA DEL DESVIADOR.
    ///
    /// <para>
    /// Un desviador guia la tarima al ENTRAR, asi que pertenece al extremo BAJO de la cama que lo justifica y a la
    /// linea de postes donde se atornilla. Su identidad fisica es <b>linea × nivel × pasillo</b>, y se traza siempre
    /// a una cama concreta: nunca se deduce de un lado historico, de un espejo, del nombre de una vista ni del
    /// extremo del rack.
    /// </para>
    /// <para>
    /// <b>Lo que corrige.</b> El BOM enumeraba los desviadores recorriendo los DOS cortes frontales de la estructura
    /// compuesta sin preguntar que pasillos existen. Medido con 3 lineas y 2 niveles: solo A, solo B y corrida —un
    /// unico pasillo, 6 piezas fisicas— cobraban <b>12</b>; encontradas acertaba por casualidad, porque ahi los dos
    /// pasillos existen de verdad.
    /// </para>
    /// <para>
    /// Cada vista colapsa un eje distinto —la planta los niveles, el lateral las lineas, un corte la profundidad—,
    /// asi que sus cuentas no tienen por que coincidir entre si. Lo que si es obligatorio: ninguna vista puede
    /// mostrar una identidad que no exista en el conjunto fisico, y el BOM cuenta ese conjunto exactamente una vez.
    /// </para>
    /// </summary>
    public class PushBackDiverterPlanTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DiverterId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DesviadorType)).Id;

        /// <summary>Un compuesto de 2 ranuras y 2 niveles —3 lineas de postes— con la topologia pedida.</summary>
        private static PushBackSystem Resolve(
            PushBackCellTopology topology, PushBackRunDirection direction = PushBackRunDirection.AToB, int slots = 2)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slots, slotsB: slots, deepA: 4, deepB: 4, levelsA: 2, levelsB: 2, gap: 0.0);
            design.Composite.DefaultTopology = topology;
            design.Composite.DefaultDirection = direction;
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<ResolvedDiverter> Physical(PushBackSystem system)
            => PushBackDiverterPlan.Resolve(system, Catalog);

        private static IReadOnlyList<HeaderBlockInstance> Instances(HeaderRunPlan plan)
            => plan.Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, DiverterId, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private static IReadOnlyList<HeaderBlockInstance> Plant(PushBackSystem system)
            => Instances(new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog));

        private static IReadOnlyList<HeaderBlockInstance> Lateral(PushBackSystem system)
            => Instances(new PushBackSystemLateralBuilder().Build(system, Catalog));

        private static IReadOnlyList<HeaderBlockInstance> Cut(
            PushBackSystem system, PushBackFrontalEnd end, PushBackSide side)
            => Instances(new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end, side));

        private static int CutTotal(PushBackSystem system)
        {
            var ends = new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior };
            var sides = new[] { PushBackSide.A, PushBackSide.B };
            return ends.Sum(end => sides.Sum(side => Cut(system, end, side).Count));
        }

        private static int Bom(PushBackSystem system)
            => PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, DiverterId, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);

        private static int Lines(PushBackSystem system) => system.Structure.Fronts.Count + 1;

        /// <summary>
        /// El PASILLO al que cae una X del dibujo. Una vista ancla su pieza en la cara del pasillo y la autoridad
        /// mide el contacto de la cama, asi que las dos X no son la misma cifra: lo que tiene que coincidir es el
        /// pasillo, no el decimal.
        /// </summary>
        private static PushBackSide AisleOf(PushBackSystem system, double x)
            => x < system.Structure.TotalLength / 2.0 ? PushBackSide.A : PushBackSide.B;

        // ==================================================================== la autoridad

        /// <summary>Cada pieza fisica se traza a una cama que carga por ese pasillo en ese nivel, y solo a una.</summary>
        [Fact]
        public void PushBackDiverterPlan_EveryPieceComesFromExactlyOneRun()
        {
            foreach (var system in Scenarios())
            {
                var runs = PushBackRuns.Resolve(system).Runs;
                var pieces = Physical(system);

                Assert.NotEmpty(pieces);
                Assert.Equal(pieces.Count, pieces.Select(piece => piece.Identity).Distinct().Count());
                Assert.All(pieces, piece => Assert.Contains(
                    runs,
                    run => run.LowSide == piece.LowSide
                           && run.Level == piece.Level
                           && (run.Slot == piece.PostLine || run.Slot == piece.PostLine - 1)));
            }
        }

        /// <summary>Y va en el extremo BAJO de esa cama: el pasillo por el que se carga.</summary>
        [Fact]
        public void PushBackDiverterPlan_PieceIsAtRunLowBoundary()
        {
            var system = Resolve(PushBackCellTopology.Encontradas);
            var runs = PushBackRuns.Resolve(system);
            var axes = runs.Runs.ToDictionary(
                run => run, run => PushBackRunGeometry.Axis(run, Catalog, runs.MirrorAxis));

            foreach (var piece in Physical(system))
            {
                var expected = axes
                    .Where(pair => pair.Key.LowSide == piece.LowSide && pair.Value.HasValue)
                    .Select(pair => Math.Round(pair.Value.Value.LowContact.X, 3))
                    .Distinct()
                    .ToList();

                Assert.Contains(Math.Round(piece.LowX, 3), expected);
            }
        }

        /// <summary>Una corrida es UNA cama por ranura y nivel: una pieza por linea y nivel, en su unico pasillo.</summary>
        [Fact]
        public void PushBackDiverterPlan_CorridaHasOneLowPiecePerPhysicalLineLevel()
        {
            var system = Resolve(PushBackCellTopology.Corrida);

            Assert.Equal(Lines(system) * 2, Physical(system).Count);
            Assert.All(Physical(system), piece => Assert.Equal(PushBackSide.A, piece.LowSide));
        }

        /// <summary>Y dos camas encontradas conservan su identidad: mismo sitio no es misma pieza.</summary>
        [Fact]
        public void PushBackDiverterPlan_EncontradasPreserveDistinctRunIdentity()
        {
            var system = Resolve(PushBackCellTopology.Encontradas);
            var pieces = Physical(system);

            Assert.Equal(Lines(system) * 2 * 2, pieces.Count);
            Assert.Equal(2, pieces.Select(piece => piece.LowSide).Distinct().Count());
            Assert.All(
                pieces.GroupBy(piece => (piece.PostLine, piece.Level)),
                group => Assert.Equal(2, group.Count()));   // la misma linea y nivel, dos pasillos: dos piezas
        }

        /// <summary>El pasillo lo dice la CAMA. Cambiar el lado historico de la seleccion no mueve ninguna pieza.</summary>
        [Fact]
        public void PushBackDiverterPlan_DoesNotInferLowFromMirrorOrSafetySide()
        {
            foreach (var topology in new[] { PushBackCellTopology.SoloA, PushBackCellTopology.SoloB })
            {
                var system = Resolve(topology);
                var before = Physical(system).Select(piece => piece.Identity).OrderBy(v => v, StringComparer.Ordinal).ToList();

                foreach (var selection in system.Structure.SafetySelections)
                {
                    selection.Side = SafetySide.Right;   // el lado historico, invertido a proposito
                }

                Assert.Equal(
                    before,
                    Physical(system).Select(piece => piece.Identity).OrderBy(v => v, StringComparer.Ordinal).ToList());
            }
        }

        // ==================================================================== solo A / solo B

        [Fact]
        public void CompositeDiverter_SoloA_HasOnlyALowPieces()
        {
            var system = Resolve(PushBackCellTopology.SoloA);

            Assert.Equal(Lines(system) * 2, Physical(system).Count);
            Assert.All(Physical(system), piece => Assert.Equal(PushBackSide.A, piece.LowSide));
        }

        [Fact]
        public void CompositeDiverter_SoloB_HasOnlyBLowPieces()
        {
            var system = Resolve(PushBackCellTopology.SoloB);

            Assert.Equal(Lines(system) * 2, Physical(system).Count);
            Assert.All(Physical(system), piece => Assert.Equal(PushBackSide.B, piece.LowSide));
        }

        /// <summary>H15 — el desviador de un rack que solo carga por B aparece en planta, y en SU pasillo.</summary>
        [Fact]
        public void CompositePlant_SoloB_DiverterAppears()
        {
            var system = Resolve(PushBackCellTopology.SoloB);
            var aisle = Physical(system).Select(piece => piece.LowSide).Distinct().Single();

            Assert.Equal(PushBackSide.B, aisle);
            Assert.NotEmpty(Plant(system));
            Assert.All(Plant(system), instance => Assert.Equal(aisle, AisleOf(system, instance.Insertion.X)));
        }

        [Fact]
        public void CompositePlant_SoloA_DoesNotCreateBPiece()
        {
            var system = Resolve(PushBackCellTopology.SoloA);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.NotEmpty(Plant(system));
            Assert.All(Plant(system), instance => Assert.True(instance.Insertion.X < middle));
        }

        [Fact]
        public void CompositePlant_SoloB_DoesNotCreateAPiece()
        {
            var system = Resolve(PushBackCellTopology.SoloB);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.NotEmpty(Plant(system));
            Assert.All(Plant(system), instance => Assert.True(instance.Insertion.X > middle));
        }

        // ==================================================================== corrida

        [Fact]
        public void CorridaAtoB_DivertersBelongOnlyToRunLowA()
            => Assert.All(
                Physical(Resolve(PushBackCellTopology.Corrida, PushBackRunDirection.AToB)),
                piece => Assert.Equal(PushBackSide.A, piece.LowSide));

        [Fact]
        public void CorridaBtoA_DivertersBelongOnlyToRunLowB()
            => Assert.All(
                Physical(Resolve(PushBackCellTopology.Corrida, PushBackRunDirection.BToA)),
                piece => Assert.Equal(PushBackSide.B, piece.LowSide));

        /// <summary>Una corrida no duplica su desviador porque el rack tenga dos lados.</summary>
        [Fact]
        public void Corrida_DiverterPhysicalCountIsNotDoubledByTwoSides()
        {
            var corrida = Resolve(PushBackCellTopology.Corrida);
            var encontradas = Resolve(PushBackCellTopology.Encontradas);

            Assert.Equal(Physical(encontradas).Count, 2 * Physical(corrida).Count);
        }

        [Fact]
        public void Corrida_DiverterBomMatchesPhysicalCount()
        {
            var system = Resolve(PushBackCellTopology.Corrida);

            Assert.Equal(Physical(system).Count, Bom(system));
        }

        // ==================================================================== encontradas

        [Fact]
        public void Encontradas_EachRunContributesItsOwnLowDiverter()
        {
            var system = Resolve(PushBackCellTopology.Encontradas);

            Assert.Equal(Lines(system) * 2, Physical(system).Count(piece => piece.LowSide == PushBackSide.A));
            Assert.Equal(Lines(system) * 2, Physical(system).Count(piece => piece.LowSide == PushBackSide.B));
        }

        [Fact]
        public void Encontradas_DiverterBomMatchesPhysicalCount()
        {
            var system = Resolve(PushBackCellTopology.Encontradas);

            Assert.Equal(Physical(system).Count, Bom(system));
        }

        // ==================================================================== cortes

        /// <summary>Los cuatro cortes, entre todos, no muestran ni una identidad que no exista fisicamente.</summary>
        [Fact]
        public void CompositeCuts_DivertersAreSubsetOfPhysicalPlan()
        {
            foreach (var system in Scenarios())
            {
                Assert.True(CutTotal(system) <= Physical(system).Count);
            }
        }

        [Fact]
        public void CompositeCuts_DoNotDuplicateAIntoB()
        {
            var system = Resolve(PushBackCellTopology.SoloA);

            Assert.NotEmpty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Empty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B));
        }

        [Fact]
        public void CompositeCuts_DoNotDuplicateBIntoA()
        {
            var system = Resolve(PushBackCellTopology.SoloB);

            Assert.NotEmpty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
            Assert.Empty(Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A));
        }

        /// <summary>Un corte solo muestra los desviadores de SU pasillo: los posteriores no llevan ninguno.</summary>
        [Fact]
        public void CompositeCut_ShowsDiverterOnlyAtItsPhysicalBoundary()
        {
            var system = Resolve(PushBackCellTopology.Encontradas);

            Assert.Equal(Lines(system) * 2, Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.A).Count);
            Assert.Equal(Lines(system) * 2, Cut(system, PushBackFrontalEnd.EntradaSalida, PushBackSide.B).Count);
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.A));
            Assert.Empty(Cut(system, PushBackFrontalEnd.Posterior, PushBackSide.B));
        }

        // ==================================================================== paridad

        /// <summary>La planta colapsa NIVELES: cada pieza que dibuja es un pasillo y una linea que existen.</summary>
        [Fact]
        public void Diverter_PlantProjectionContainsOnlyResolvedPhysicalPieces()
        {
            foreach (var system in Scenarios())
            {
                var aisles = Physical(system).Select(piece => piece.LowSide).Distinct().ToList();
                var lines = Physical(system).Select(piece => Math.Round(piece.LineX, 1)).Distinct().ToList();

                Assert.NotEmpty(Plant(system));
                Assert.All(Plant(system), instance =>
                {
                    Assert.Contains(AisleOf(system, instance.Insertion.X), aisles);
                    Assert.Contains(Math.Round(instance.Insertion.Y, 1), lines);
                });
            }
        }

        /// <summary>El lateral colapsa LINEAS: cada pieza que dibuja esta en un pasillo que existe.</summary>
        [Fact]
        public void Diverter_LateralProjectionContainsOnlyResolvedPhysicalPieces()
        {
            foreach (var system in Scenarios())
            {
                var aisles = Physical(system).Select(piece => piece.LowSide).Distinct().ToList();

                Assert.NotEmpty(Lateral(system));
                Assert.All(Lateral(system), instance =>
                    Assert.Contains(AisleOf(system, instance.Insertion.X), aisles));
            }
        }

        /// <summary>Y un corte no muestra mas piezas de las que su pasillo tiene.</summary>
        [Fact]
        public void Diverter_CutProjectionContainsOnlyResolvedPhysicalPieces()
        {
            foreach (var system in Scenarios())
            {
                foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
                {
                    var expected = Physical(system).Count(piece => piece.LowSide == side);
                    Assert.Equal(expected, Cut(system, PushBackFrontalEnd.EntradaSalida, side).Count);
                }
            }
        }

        /// <summary>Y el BOM cuenta cada pieza fisica exactamente una vez, en las cinco topologias.</summary>
        [Fact]
        public void Diverter_BomCountsEveryResolvedPhysicalPieceExactlyOnce()
        {
            foreach (var system in Scenarios())
            {
                Assert.Equal(Physical(system).Count, Bom(system));
            }
        }

        private static IEnumerable<PushBackSystem> Scenarios()
        {
            yield return Resolve(PushBackCellTopology.SoloA);
            yield return Resolve(PushBackCellTopology.SoloB);
            yield return Resolve(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            yield return Resolve(PushBackCellTopology.Corrida, PushBackRunDirection.BToA);
            yield return Resolve(PushBackCellTopology.Encontradas);
        }

        // ==================================================================== bites

        /// <summary>
        /// BITE A — la enumeracion anterior recorria los DOS cortes frontales de la estructura compuesta: con un
        /// solo pasillo cobraba el doble. Es exactamente la diferencia que el BOM ya no tiene.
        /// </summary>
        [Fact]
        public void Bite_LegacyBomEnumeration_DoublesTheSingleAisleCount()
        {
            var system = Resolve(PushBackCellTopology.SoloA);
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var plateId = DynamicFrontGeometry.PlateId(system.Structure, Catalog);
            var builder = new DynamicSafetyMultiViewBuilder();
            var legacy = new List<HeaderBlockInstance>();
            builder.AppendFrontal(legacy, system.Structure, Catalog, layout, plateId, DynamicRackEnd.Exit);
            builder.AppendFrontal(legacy, system.Structure, Catalog, layout, plateId, DynamicRackEnd.Entrance);

            var legacyCount = legacy.Count(instance =>
                string.Equals(instance.PieceId, DiverterId, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(2 * Physical(system).Count, legacyCount);
            Assert.Equal(Physical(system).Count, Bom(system));
        }

        /// <summary>
        /// BITE B — si el pasillo se dedujera del lado historico, solo A y solo B darian el MISMO conjunto. La cama
        /// es quien lo sabe, y por eso difieren.
        /// </summary>
        [Fact]
        public void Bite_CollapsedSafetySide_WouldMergeSoloAAndSoloB()
        {
            var soloA = Physical(Resolve(PushBackCellTopology.SoloA));
            var soloB = Physical(Resolve(PushBackCellTopology.SoloB));

            Assert.Equal(soloA.Count, soloB.Count);
            Assert.Empty(soloA.Select(piece => piece.Identity).Intersect(soloB.Select(piece => piece.Identity)));
            Assert.NotEqual(Math.Round(soloA[0].LowX, 1), Math.Round(soloB[0].LowX, 1));
        }

        /// <summary>
        /// BITE C — si cada sistema local materializara su propio desviador, un rack que solo carga por A tendria
        /// piezas tambien en los cortes de B. No las tiene.
        /// </summary>
        [Fact]
        public void Bite_LocalSystemsMaterializing_WouldPutDivertersInBothSides()
        {
            var system = Resolve(PushBackCellTopology.SoloA);

            Assert.Equal(Physical(system).Count, CutTotal(system));
            Assert.All(Physical(system), piece => Assert.Equal(PushBackSide.A, piece.LowSide));
        }

        /// <summary>
        /// BITE D — sin el pasillo en la identidad, las dos camas encontradas de una misma linea y nivel se
        /// fundirian en una: la mitad de las piezas desapareceria.
        /// </summary>
        [Fact]
        public void Bite_IdentityWithoutAisle_HalvesEncontradas()
        {
            var pieces = Physical(Resolve(PushBackCellTopology.Encontradas));
            var withoutAisle = pieces.Select(piece => (piece.PostLine, piece.Level)).Distinct().Count();

            Assert.Equal(pieces.Count / 2, withoutAisle);
            Assert.Equal(pieces.Count, pieces.Select(piece => piece.Identity).Distinct().Count());
        }
    }
}
