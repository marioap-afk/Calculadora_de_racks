using System;
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
    /// I-42 (ronda de correccion) — los defectos que el Coordinador y el Owner encontraron sobre el candidato
    /// 6c9f778. Cada prueba fija la regla FISICA corregida, con coordenadas, longitudes, rotaciones y cantidades:
    /// una prueba que solo comprueba «no vacio» no habria detectado ninguno de estos.
    /// </summary>
    public class PushBackCompositeCorrectionTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            int deepA = 4, int deepB = 8, int levelsA = 1, int levelsB = 1,
            double gap = 0.0, int slotsA = 1, int slotsB = 1)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slotsA, slotsB: slotsB, deepA: deepA, deepB: deepB,
                levelsA: levelsA, levelsB: levelsB, gap: gap);
            design.Composite.DefaultTopology = topology;
            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design) => new PushBackResolver(Catalog).Resolve(design);

        private static LateralHeaderLayout Lateral(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten();

        // ================= 8. El falso error de capacidad 4 + 8 =================================================

        [Fact]
        public void FourAndEight_IsValid_AndEachBedIsMeasuredAgainstItsOwnStructure()
        {
            var system = Resolve(Design(PushBackCellTopology.Encontradas, deepA: 4, deepB: 8));
            var cell = system.Composite.Cell(0, 1);

            Assert.True(cell.IsValid);
            Assert.Equal(2, cell.Beds.Count);   // encontradas: DOS camas fisicas independientes

            var fromA = cell.BedFrom(PushBackSide.A);
            var fromB = cell.BedFrom(PushBackSide.B);
            Assert.NotNull(fromA);
            Assert.NotNull(fromB);
            // Cada cama contra SU estructura: la de B, mas larga, no se compara con el espacio de A.
            Assert.True(fromA.RequiredBedLength <= fromA.AvailableBedSpan + PushBackBedSpan.Tolerance);
            Assert.True(fromB.RequiredBedLength <= fromB.AvailableBedSpan + PushBackBedSpan.Tolerance);
            Assert.True(fromB.RequiredBedLength > fromA.AvailableBedSpan);   // el cruce que producia el falso error
            Assert.DoesNotContain(PushBackCompositeDiagnostics.Evaluate(system), d => d.IsBlocking);
        }

        [Theory]
        [InlineData(4, 8)]
        [InlineData(8, 5)]
        [InlineData(3, 9)]
        [InlineData(8, 4)]
        public void AsymmetricDepths_NeverProduceAFalseCapacityError(int deepA, int deepB)
        {
            foreach (var topology in new[]
                     {
                         PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                         PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
                     })
            {
                var system = Resolve(Design(topology, deepA: deepA, deepB: deepB));
                Assert.All(
                    system.Composite.Cells,
                    cell => Assert.True(cell.IsValid, topology + " " + deepA + "/" + deepB + ": " + cell.DisabledReason));
                Assert.Equal(deepA, system.Composite.SideA.ProposedStructure);
                Assert.Equal(deepB, system.Composite.SideB.ProposedStructure);
            }
        }

        [Fact]
        public void ACellThatReallyExceedsItsOwnStructure_IsStillBlocked_AndNamesItsSide()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 4, deepB: 8);
            design.Composite.StructureOverrideB = 3;   // B no cabe en su propia estructura

            var system = Resolve(design);
            var cell = system.Composite.Cell(0, 1);

            Assert.False(cell.IsValid);
            Assert.True(cell.BedFrom(PushBackSide.A).IsValid);    // la cama de A si cabe
            Assert.False(cell.BedFrom(PushBackSide.B).IsValid);
            Assert.Contains("Lado B", cell.DisabledReason);
            Assert.Contains("frente 1", cell.DisabledReason);
            Assert.Contains("nivel 1", cell.DisabledReason);
        }

        [Fact]
        public void PerCellOverrides_BlockOnlyTheCellThatExceedsItsOwnSpan()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 4, deepB: 8, levelsA: 3, levelsB: 3);
            design.Composite.StructureOverrideA = 4;
            design.Fronts[0].PalletsDeepOverrides.Add(null);   // nivel 1: hereda 4
            design.Fronts[0].PalletsDeepOverrides.Add(9);      // nivel 2: pide 9 sobre una estructura de 4
            design.Fronts[0].PalletsDeepOverrides.Add(null);   // nivel 3: hereda 4

            var system = Resolve(design);

            Assert.True(system.Composite.Cell(0, 1).IsValid);
            Assert.False(system.Composite.Cell(0, 2).IsValid);
            Assert.True(system.Composite.Cell(0, 3).IsValid);
        }

        // ================= 9. La corrida NO ocupa todo el sistema ===============================================

        [Fact]
        public void ACorrida_TakesOnlyItsDemand_InsideTheSameStructure()
        {
            // Estructura 5 + 8. La celda DECLARA 10 fondos propios: ni 5, ni 8, ni 13. La cama es mas corta
            // que el rack y las dos estructuras siguen intactas.
            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);

            var system = Resolve(design);
            var cell = system.Composite.Cell(0, 1);
            var bed = Assert.Single(cell.Beds);

            Assert.Equal(10, bed.DemandPositions);
            Assert.True(bed.IsValid);
            // La estructura NO se toca: sigue siendo 5 + 8.
            Assert.Equal(5, system.Composite.SideA.EffectiveStructure);
            Assert.Equal(8, system.Composite.SideB.EffectiveStructure);
            // Y la cama es mas corta que el rack entero.
            Assert.True(bed.RequiredBedLength < system.Structure.TotalLength - 1.0);
            Assert.Equal(system.Structure.TotalLength, bed.AvailableBedSpan, 6);
        }

        [Fact]
        public void AShorterCorrida_DoesNotChangeTheProposedStructure()
        {
            var full = Resolve(Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8));

            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);
            var shorter = Resolve(design);

            // El fondo de la corrida es una autoridad de CAMA, no de estructura: no mueve ninguna de las dos.
            Assert.Equal(full.Composite.SideA.ProposedStructure, shorter.Composite.SideA.ProposedStructure);
            Assert.Equal(full.Composite.SideB.ProposedStructure, shorter.Composite.SideB.ProposedStructure);
            Assert.Equal(5, shorter.Composite.SideA.EffectiveStructure);
            Assert.Equal(8, shorter.Composite.SideB.EffectiveStructure);

            // Y dos niveles con fondos de corrida distintos conviven sobre esa MISMA estructura.
            var mixed = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8, levelsA: 2, levelsB: 2);
            mixed.Composite.SetCorridaDepth(0, 0, 10);   // nivel 1: corrida corta
            mixed.Composite.SetCorridaDepth(0, 1, 13);   // nivel 2: corrida completa
            var system = Resolve(mixed);

            Assert.Equal(5, system.Composite.SideA.EffectiveStructure);
            Assert.Equal(8, system.Composite.SideB.EffectiveStructure);
            Assert.True(system.Composite.Cell(0, 1).Beds[0].RequiredBedLength
                        < system.Composite.Cell(0, 2).Beds[0].RequiredBedLength);
        }

        [Fact]
        public void AShorterCorrida_AnchorsAtTheLowEnd_InBothDirections()
        {
            foreach (var direction in new[] { PushBackRunDirection.AToB, PushBackRunDirection.BToA })
            {
                var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
                design.Composite.DefaultDirection = direction;
                design.Composite.SetCorridaDepth(0, 0, 10);

                var system = Resolve(design);
                var total = system.Structure.TotalLength;
                var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();

                // El extremo BAJO —por donde se carga y se descarga— queda pegado al poste EXTERIOR de su lado. El
                // que se mete hacia dentro cuando la cama pide menos fondo del disponible es el ALTO.
                if (direction == PushBackRunDirection.AToB)
                {
                    Assert.True(axis.LowContact.X < total * 0.15, "el bajo tiene que estar en la orilla de A");
                    Assert.True(axis.HighContact.X < total - 1.0, "el alto tiene que haberse metido hacia dentro");
                }
                else
                {
                    Assert.True(axis.LowContact.X > total * 0.85, "el bajo tiene que estar en la orilla de B");
                    Assert.True(axis.HighContact.X > 1.0, "el alto tiene que haberse metido hacia dentro");
                }

                // La pendiente no cambia: el alto sigue siendo el extremo elevado.
                Assert.True(axis.HighContact.Y > axis.LowContact.Y);
                Assert.True(axis.Length < total - 1.0);
            }
        }

        [Fact]
        public void ACorridaBed_IsQuotedAtItsOwnLength_NotAtTheRackLength()
        {
            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);

            var system = Resolve(design);
            var bed = PushBackBomBuilder.Build(system, Catalog).Components
                .Single(component => component.Category == SystemBomBuilder.Cama);

            Assert.True(bed.Length < system.Structure.TotalLength - 1.0);
            Assert.Equal(1, bed.Quantity);
        }

        // ================= CONTRATO: el GAP pertenece a la ESTRUCTURA ===========================================

        /// <summary>
        /// PRUEBA VINCULANTE del contrato de capacidad: el HUECO pertenece a la ESTRUCTURA, asi que aumenta lo
        /// DISPONIBLE sin tocar lo EXIGIDO, y por eso puede volver valida una cama que sin el no cabe.
        ///
        /// <para>
        /// Misma demanda y misma estructura por lados en los dos escenarios; lo unico que cambia es el hueco. No se
        /// comprueba ningun conteo de fondos: se comparan LONGITUDES FISICAS.
        /// </para>
        /// </summary>
        [Fact]
        public void APositiveGap_RescuesABedThatDoesNotFit_WithoutChangingItsDemand()
        {
            PushBackDesign WithGap(double gap)
            {
                // Estructura fija por lados y demanda fija de la celda: 13 fondos declarados sobre una estructura
                // que solo ofrece 5 + 4 posiciones.
                var design = Design(PushBackCellTopology.Corrida, deepA: 8, deepB: 5, levelsA: 1, levelsB: 1);
                design.Composite.StructureOverrideA = 5;
                design.Composite.StructureOverrideB = 4;
                design.Composite.SetCorridaDepth(0, 0, 13);
                design.Composite.Gap = gap;
                return design;
            }

            var tight = Resolve(WithGap(0.0));
            var tightBed = tight.Composite.Cell(0, 1).Beds.Single();

            // Sin hueco NO cabe: la demanda excede lo que la estructura ofrece.
            Assert.False(tightBed.IsValid);
            Assert.True(tightBed.RequiredBedLength > tightBed.AvailableBedSpan);

            // Lo que le falta, exactamente, y un poco mas para que quepa con holgura.
            var missing = tightBed.RequiredBedLength - tightBed.AvailableBedSpan;
            Assert.True(missing > 0.0);
            var gap = missing + 6.0;

            var loose = Resolve(WithGap(gap));
            var looseBed = loose.Composite.Cell(0, 1).Beds.Single();

            // 1) La DEMANDA no cambia: ni los fondos ni la longitud MINIMA que exigen. (La longitud FISICA si puede
            //    moverse: la cama apoya donde la estructura le da apoyo, y el hueco cambia la estructura.)
            Assert.Equal(tightBed.DemandPositions, looseBed.DemandPositions);
            Assert.Equal(tightBed.RequiredBedLength, looseBed.RequiredBedLength, 6);

            // 2) Lo DISPONIBLE crece exactamente el incremento fisico del hueco.
            Assert.Equal(tightBed.AvailableBedSpan + gap, looseBed.AvailableBedSpan, 6);

            // 3) El estado cambia: invalida -> valida.
            Assert.True(looseBed.IsValid);
            Assert.True(looseBed.RequiredBedLength <= looseBed.AvailableBedSpan + PushBackBedSpan.Tolerance);
            Assert.DoesNotContain(PushBackCompositeDiagnostics.Evaluate(loose), d => d.IsBlocking);

            // 4) Y la estructura por lados es la MISMA en los dos escenarios: solo cambio el hueco.
            Assert.Equal(tight.Composite.SideA.EffectiveStructure, loose.Composite.SideA.EffectiveStructure);
            Assert.Equal(tight.Composite.SideB.EffectiveStructure, loose.Composite.SideB.EffectiveStructure);
        }

        /// <summary>
        /// El hueco no toca la DEMANDA. La longitud FISICA si puede moverse, y debe: para alcanzar su decimo fondo
        /// la cama tiene que cruzar el hueco, asi que el apoyo valido queda mas lejos. Lo que NUNCA ocurre es que la
        /// cama quede flotando entre dos apoyos.
        /// </summary>
        [Fact]
        public void ChangingOnlyTheGap_DoesNotChangeTheDemand_AndTheBedStaysOnARealSupport()
        {
            PushBackDesign WithGap(double gap)
            {
                var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8, levelsA: 1, levelsB: 1);
                design.Composite.SetCorridaDepth(0, 0, 10);   // AUTORIDAD PROPIA: 10 fondos, no 5 + 8
                design.Composite.Gap = gap;
                return design;
            }

            var tight = Resolve(WithGap(0.0));
            var loose = Resolve(WithGap(18.0));

            var tightBed = tight.Composite.Cell(0, 1).Beds.Single();
            var looseBed = loose.Composite.Cell(0, 1).Beds.Single();

            // 1) La demanda —fondos y longitud minima— es identica.
            Assert.Equal(10, tightBed.DemandPositions);
            Assert.Equal(tightBed.DemandPositions, looseBed.DemandPositions);
            Assert.Equal(tightBed.RequiredBedLength, looseBed.RequiredBedLength, 6);

            // 2) La estructura SI crece, y con ella lo disponible.
            Assert.Equal(tight.Structure.TotalLength + 18.0, loose.Structure.TotalLength, 6);
            Assert.Equal(tightBed.AvailableBedSpan + 18.0, looseBed.AvailableBedSpan, 6);

            // 3) En los dos casos: Required <= Resolved <= Available, la cama dibujada mide lo resuelto, y ese
            //    extremo bajo cae sobre una linea de modulo real — nunca en un punto intermedio.
            foreach (var (system, bed) in new[] { (tight, tightBed), (loose, looseBed) })
            {
                var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();
                Assert.True(bed.RequiredBedLength <= bed.ResolvedBedLength + PushBackBedSpan.Tolerance);
                Assert.True(bed.ResolvedBedLength <= bed.AvailableBedSpan + PushBackBedSpan.Tolerance);
                Assert.Equal(bed.ResolvedBedLength, axis.Length, 6);
                Assert.Contains(
                    system.Structure.Modules,
                    module => Math.Abs((system.Structure.TotalLength - module.StartX) - axis.Length) < 1e-6);
            }
        }

        [Fact]
        public void TheBomOfAPartialCorrida_QuotesItsResolvedLength()
        {
            (double Bom, double Resolved, double Total) Quote(double gap)
            {
                var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8, levelsA: 1, levelsB: 1);
                design.Composite.SetCorridaDepth(0, 0, 10);
                design.Composite.Gap = gap;
                var system = Resolve(design);
                var bom = PushBackBomBuilder.Build(system, Catalog).Components
                    .Single(component => component.Category == SystemBomBuilder.Cama).Length;
                return (bom, system.Composite.Cell(0, 1).Beds.Single().ResolvedBedLength, system.Structure.TotalLength);
            }

            foreach (var gap in new[] { 0.0, 24.0 })
            {
                var quote = Quote(gap);
                // Se cotiza el riel que se FABRICA: el resuelto. Ni el minimo teorico, ni el rack entero.
                Assert.Equal(quote.Resolved, quote.Bom, 4);
                Assert.True(quote.Bom < quote.Total - 1.0);
            }
        }

        [Fact]
        public void ASideBed_IsNeverAffectedByTheGap()
        {
            // Una cama de un solo lado no atraviesa la interfaz: ni su demanda ni su capacidad ven el hueco.
            PushBackSystem WithGap(double gap)
            {
                var design = Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 8, levelsA: 1, levelsB: 1);
                design.Composite.Gap = gap;
                return Resolve(design);
            }

            var tight = WithGap(0.0).Composite.Cell(0, 1);
            var loose = WithGap(20.0).Composite.Cell(0, 1);

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                Assert.Equal(tight.BedFrom(side).RequiredBedLength, loose.BedFrom(side).RequiredBedLength, 6);
                Assert.Equal(tight.BedFrom(side).AvailableBedSpan, loose.BedFrom(side).AvailableBedSpan, 6);
            }
        }

        // ================= 10. Una corrida corta NO crea otra estructura ========================================

        [Fact]
        public void AShorterCorrida_DoesNotMaterializeASecondStructure()
        {
            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);
            var system = Resolve(design);

            var instances = Lateral(system).Instances;
            var posts = instances.Count(instance => instance.Role == HeaderBlockRole.Post);
            var plates = instances.Count(instance => instance.Role == HeaderBlockRole.BasePlate);

            // La MISMA estructura que dibuja un rack sin ninguna corrida: la receta sintetica no aporta ni un poste.
            var encontradas = Resolve(Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 8));
            var reference = Lateral(encontradas).Instances;

            Assert.Equal(reference.Count(instance => instance.Role == HeaderBlockRole.Post), posts);
            Assert.Equal(reference.Count(instance => instance.Role == HeaderBlockRole.BasePlate), plates);
            Assert.Equal(
                reference.Count(instance => instance.Role == HeaderBlockRole.Separator),
                instances.Count(instance => instance.Role == HeaderBlockRole.Separator));
        }

        [Fact]
        public void TheCorridaBom_HasTheSameStructureAsTheSameRackWithoutCorridas()
        {
            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);

            string Structural(BillOfMaterials bom) => string.Join(
                ";",
                bom.Components
                    .Where(component => component.Category != SystemBomBuilder.Cama
                        && component.Category != SystemBomBuilder.InOutBeam
                        && component.Category != PushBackBomBuilder.HighEndBeam
                        && component.Category != PushBackBomBuilder.RearTope
                        && component.Category != SystemBomBuilder.IntermediateBeam)
                    .OrderBy(component => component.Category, StringComparer.Ordinal)
                    .ThenBy(component => component.ProfileId, StringComparer.Ordinal)
                    .Select(component => component.Category + "|" + component.ProfileId + "|" + component.Quantity));

            var corrida = Structural(PushBackBomBuilder.Build(Resolve(design), Catalog));
            var encontradas = Structural(
                PushBackBomBuilder.Build(Resolve(Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 8)), Catalog));

            Assert.Equal(encontradas, corrida);
        }

        // ================= 6. Largueros intermedios =============================================================

        private static int Intermediates(LateralHeaderLayout layout)
            => layout.Instances.Count(instance => string.Equals(
                instance.PieceId,
                DynamicRackDefaults.IntermediateBeamCatalogId,
                StringComparison.OrdinalIgnoreCase));

        [Fact]
        public void EncontradasIntermediates_BelongToTheirOwnBed_WithOppositeSlopes()
        {
            var system = Resolve(Design(PushBackCellTopology.Encontradas, deepA: 8, deepB: 8));
            var beams = Lateral(system).Instances
                .Where(instance => string.Equals(
                    instance.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(beams);
            var total = system.Structure.TotalLength;
            // Ningun intermedio cruza a la cama contraria: los de A viven en su mitad y los de B en la suya.
            Assert.All(beams, beam => Assert.True(beam.Insertion.X > -1e-6 && beam.Insertion.X < total + 1e-6));
            Assert.Contains(beams, beam => beam.Insertion.X < total / 2.0);
            Assert.Contains(beams, beam => beam.Insertion.X > total / 2.0);
            // Y los de cada lado se reparten simetricamente: mismas cantidades con lados simetricos.
            Assert.Equal(
                beams.Count(beam => beam.Insertion.X < total / 2.0),
                beams.Count(beam => beam.Insertion.X > total / 2.0));
        }

        [Fact]
        public void DeeperSides_GetMoreIntermediates_ThanShallowOnes()
        {
            var shallow = Intermediates(Lateral(Resolve(Design(PushBackCellTopology.SoloA, deepA: 4, deepB: 4))));
            var deep = Intermediates(Lateral(Resolve(Design(PushBackCellTopology.SoloA, deepA: 8, deepB: 4))));

            Assert.True(deep > shallow, "una cama mas profunda necesita mas soportes intermedios");
        }

        [Fact]
        public void SideBIntermediates_MirrorSideAOnes_WhenTheSidesAreSymmetric()
        {
            var onlyA = Lateral(Resolve(Design(PushBackCellTopology.SoloA, deepA: 8, deepB: 8)));
            var onlyB = Lateral(Resolve(Design(PushBackCellTopology.SoloB, deepA: 8, deepB: 8)));

            Assert.Equal(Intermediates(onlyA), Intermediates(onlyB));
            Assert.True(Intermediates(onlyA) > 0);
        }

        [Fact]
        public void AShorterCorrida_HasNoIntermediatesOutsideItsRealBed()
        {
            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);
            var system = Resolve(design);

            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();
            var low = Math.Min(axis.LowContact.X, axis.HighContact.X);
            var high = Math.Max(axis.LowContact.X, axis.HighContact.X);

            var beams = Lateral(system).Instances
                .Where(instance => string.Equals(
                    instance.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(beams);
            Assert.All(beams, beam => Assert.InRange(beam.Insertion.X, low - 1e-6, high + 1e-6));
        }

        [Fact]
        public void ACorrida_HasOneSetOfIntermediates_NotTwo()
        {
            var system = Resolve(Design(PushBackCellTopology.Corrida, deepA: 8, deepB: 8));
            var beams = Lateral(system).Instances
                .Where(instance => string.Equals(
                    instance.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(beams);
            // UNA cama continua: ningun soporte se emite dos veces en la misma X (que es lo que pasaria si se
            // montaran un juego de A y otro de B sobre la misma cama).
            var positions = beams.Select(beam => Math.Round(beam.Insertion.X, 4)).ToList();
            Assert.Equal(positions.Count, positions.Distinct().Count());

            // Y todos caen DENTRO de la unica cama, subiendo con su pendiente: no hay un segundo juego montado
            // sobre ella. (Los pares espejados de un poste derivado reforzado son la MISMA pieza fisica duplicada
            // por el refuerzo, no un segundo juego, y por eso comparten X.)
            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();
            var low = Math.Min(axis.LowContact.X, axis.HighContact.X);
            var high = Math.Max(axis.LowContact.X, axis.HighContact.X);
            Assert.All(beams, beam => Assert.InRange(beam.Insertion.X, low - 1e-6, high + 1e-6));

            var ordered = beams.Where(beam => !beam.MirroredX).OrderBy(beam => beam.Insertion.X).ToList();
            for (var index = 1; index < ordered.Count; index++)
            {
                Assert.True(ordered[index].Insertion.Y >= ordered[index - 1].Insertion.Y - 1e-6);
            }
        }

        [Fact]
        public void IntermediatesFollowTheirBedSlope()
        {
            var system = Resolve(Design(PushBackCellTopology.Encontradas, deepA: 8, deepB: 8));
            var axes = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog);
            var fromA = axes.Single(axis => axis.FlowsForward);
            var total = system.Structure.TotalLength;

            var beams = Lateral(system).Instances
                .Where(instance => string.Equals(
                    instance.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .Where(instance => instance.Insertion.X < total / 2.0)
                .OrderBy(instance => instance.Insertion.X)
                .ToList();

            Assert.True(beams.Count >= 2);
            // Suben con la pendiente de SU cama: mas lejos del pasillo, mas alto.
            for (var index = 1; index < beams.Count; index++)
            {
                Assert.True(beams[index].Insertion.Y > beams[index - 1].Insertion.Y - 1e-6);
            }

            Assert.True(fromA.Slope > 0.0);
        }

        // ================= 2. Cotas y elevaciones por lado ======================================================

        [Fact]
        public void LateralDimensions_UseEachSideOwnElevations()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 6, deepB: 6, levelsA: 2, levelsB: 2);
            design.Structure.FirstLevelHeight = 4.0;
            design.SideB.FirstLevelHeight = 30.0;      // el lado B carga mucho mas alto
            design.Structure.NumberLevels = true;
            design.Structure.Dimensions = RackCad.Domain.Systems.Shared.DimensionDetail.Standard;

            var system = Resolve(design);
            var total = system.Structure.TotalLength;
            var labels = Lateral(system).Instances
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .ToList();

            Assert.NotEmpty(labels);
            var nearA = labels.Where(label => label.Insertion.X < total / 2.0).ToList();
            var nearB = labels.Where(label => label.Insertion.X > total / 2.0).ToList();
            Assert.NotEmpty(nearA);
            Assert.NotEmpty(nearB);

            // Las etiquetas de nivel del lado B viven a SU elevacion, no a la de A.
            var levelsNearB = nearB.Where(label => label.Text != "A" && label.Text != "B").ToList();
            var levelsNearA = nearA.Where(label => label.Text != "A" && label.Text != "B").ToList();
            if (levelsNearA.Count > 0 && levelsNearB.Count > 0)
            {
                Assert.True(levelsNearB.Max(label => label.Insertion.Y)
                            > levelsNearA.Max(label => label.Insertion.Y) + 1.0);
            }
        }

        [Fact]
        public void MirroredAnnotations_AreNotFlipped()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 6, deepB: 6, levelsA: 2, levelsB: 2);
            design.Structure.NumberLevels = true;
            design.Structure.NumberFronts = true;

            var annotations = Lateral(Resolve(design)).Instances
                .Where(instance => instance.Role == HeaderBlockRole.Annotation)
                .ToList();

            Assert.NotEmpty(annotations);
            // Un texto no es una pieza: reflejarlo lo dejaria escrito del reves.
            Assert.All(annotations, annotation => Assert.False(annotation.MirroredX));
            Assert.All(annotations, annotation => Assert.Equal(0.0, annotation.RotationRadians, 9));
        }

        // ================= 3. Planta all-corrida ================================================================

        [Fact]
        public void AnAllCorridaSlot_HasNoInterfaceRearBeamsInPlanta()
        {
            var corrida = Resolve(Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 5, levelsA: 2, levelsB: 2));
            var encontradas = Resolve(Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 5, levelsA: 2, levelsB: 2));

            int Rears(PushBackSystem system) => new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Count(instance => string.Equals(
                    instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase));

            // Encontradas: dos largueros posteriores (uno por interfaz). Corrida: UNO, el de su extremo alto.
            Assert.Equal(2, Rears(encontradas));
            Assert.Equal(1, Rears(corrida));
        }

        [Fact]
        public void OneNonCorridaLevel_IsEnoughForThePlantaToShowItsRearBeam()
        {
            var design = Design(PushBackCellTopology.Corrida, deepA: 5, deepB: 5, levelsA: 2, levelsB: 2);
            design.Composite.SetCell(0, 1, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var system = Resolve(design);

            var rears = new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => string.Equals(
                    instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .Select(instance => Math.Round(instance.Insertion.X, 4))
                .OrderBy(x => x)
                .ToList();

            // TRES largueros posteriores, y se comprueban por POSICION porque el conteo solo no distingue el
            // defecto: el nivel ENCONTRADAS aporta dos —uno por lado, en la interfaz— y el nivel CORRIDO aporta el
            // suyo en el extremo ALTO de su recorrido, que es el otro extremo del rack. La planta dibujaba los tres
            // en la interfaz, asi que dos coincidian y parecian uno.
            var interfaceX = Math.Round(system.Composite.Of(PushBackSide.A).Local.Structure.TotalLength, 4);
            var runHighX = Math.Round(
                PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog)
                    .First(axis => axis.Level == 1)
                    .HighContact.X,
                0);

            Assert.Equal(3, rears.Count);
            Assert.Equal(2, rears.Count(x => Math.Abs(x - interfaceX) < 1e-6));
            Assert.Single(rears, x => Math.Abs(x - runHighX) < 1.0);
            Assert.True(runHighX > interfaceX + 1.0, "la corrida A->B acaba MAS ALLA de la interfaz");
        }

        // ================= 4. Entradas invalidas =================================================================

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-500.0)]
        public void ANegativeGap_IsPreservedAndBlocked_NeverCoerced(double gap)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.SetFrontCount(1);
            state.SetSideBPresent(true);
            state.SideB.SetFrontCount(1);
            state.SetGap(gap);

            Assert.Equal(gap, state.Gap, 6);
            Assert.False(state.GapIsValid);
            var diagnostics = state.IntentDiagnostics();
            Assert.Contains(diagnostics, d => d.Code == PushBackCompositeCodes.GapInvalid && d.IsBlocking);

            var computation = new PushBackCompositeEditorAssembler(Catalog)
                .Build(state, PushBackEditorInputs.NewDesign(), Catalog);
            Assert.False(computation.IsValid);
            Assert.True(computation.HasBlocking);
            Assert.Equal(gap, state.Gap, 6);   // sigue intacto tras el intento
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void AnInvalidStructureOverride_IsNotARestore(int positions)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.SetFrontCount(1);
            state.SetSideBPresent(true);
            state.SideB.SetFrontCount(1);

            state.SetStructureOverride(PushBackSide.A, positions);
            Assert.Equal(positions, state.StructureOverrideA);          // se conserva, no se vuelve null
            Assert.True(state.StructureOverrideIsInvalid(PushBackSide.A));
            Assert.True(state.HasBlockingIntent());

            // Corregirlo despues devuelve el rack a la normalidad, sin haber perdido la intencion por el camino.
            state.SetStructureOverride(PushBackSide.A, 6);
            Assert.False(state.HasBlockingIntent());
            Assert.Equal(6, state.StructureOverrideA);

            // Y RESTAURAR sigue siendo lo unico que pone null.
            state.RestoreStructure(PushBackSide.A);
            Assert.Null(state.StructureOverrideA);
        }

        [Fact]
        public void AnInvalidIntent_RollsBackWithTheSnapshot()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.SetFrontCount(1);
            state.SetSideBPresent(true);
            state.SideB.SetFrontCount(1);
            state.SetGap(10.0);
            var snapshot = state.Snapshot();

            state.SetGap(-3.0);
            state.SetStructureOverride(PushBackSide.B, 1);
            Assert.True(state.HasBlockingIntent());

            state.Restore(snapshot);
            Assert.Equal(10.0, state.Gap, 6);
            Assert.Null(state.StructureOverrideB);
            Assert.False(state.HasBlockingIntent());
        }

        // ================= 5. I-40 sobrevive a la recomposicion ==================================================

        [Fact]
        public void GrowingASide_KeepsTheSurvivingModulesIdentityAndConfiguration()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 5);
            var first = Resolve(design);

            // Se personaliza la cabecera exterior del lado A (la que no se mueve al crecer la estructura).
            var snapshot = new PushBackResolver(Catalog).Snapshot(first);
            var header = snapshot.Structure.Modules.First(module => module.IsHeader);
            var customId = header.ModuleId;
            header.UseCalculatedHeaderConfiguration = false;
            header.HeaderConfiguration = new RackCad.Domain.RackFrames.RackFrameConfiguration { Height = 222.0 };
            snapshot.SideB = design.SideB;
            snapshot.Composite = design.Composite;

            // Ahora crece la estructura de A.
            snapshot.Composite.StructureOverrideA = 8;
            var grown = Resolve(snapshot);

            var survivor = grown.Structure.Modules.FirstOrDefault(module => module.ModuleId == customId);
            Assert.NotNull(survivor);
            Assert.False(survivor.UseCalculatedHeaderConfiguration);
            Assert.Equal(222.0, survivor.AssociatedFrameConfiguration.Height, 6);
            Assert.Equal(8, grown.Composite.SideA.EffectiveStructure);
        }

        [Fact]
        public void ShrinkingASide_DropsTheModulesThatNoLongerExist_WithoutMovingTheirOverrideElsewhere()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 8, deepB: 5);
            var first = Resolve(design);
            var snapshot = new PushBackResolver(Catalog).Snapshot(first);
            snapshot.SideB = design.SideB;
            snapshot.Composite = design.Composite;

            // La ULTIMA cabecera de A (la interior) se personaliza y luego la estructura de A encoge.
            var lastHeader = snapshot.Structure.Modules
                .TakeWhile(module => module.ModuleId != PushBackCompositeStructure.GapModuleId)
                .Last(module => module.IsHeader);
            var vanishingId = lastHeader.ModuleId;
            lastHeader.UseCalculatedHeaderConfiguration = false;
            lastHeader.HeaderConfiguration = new RackCad.Domain.RackFrames.RackFrameConfiguration { Height = 333.0 };

            snapshot.Composite.StructureOverrideA = 4;
            var shrunk = Resolve(snapshot);

            // La pieza dejo de existir: su configuracion no reaparece sobre otra.
            Assert.DoesNotContain(
                shrunk.Structure.Modules,
                module => module.AssociatedFrameConfiguration != null
                    && Math.Abs(module.AssociatedFrameConfiguration.Height - 333.0) < 1e-6);
            Assert.Equal(4, shrunk.Composite.SideA.EffectiveStructure);
            Assert.NotEqual(vanishingId, shrunk.Structure.Modules.Last().ModuleId);
        }

        [Fact]
        public void ChangingTheGap_DoesNotRebuildTheModulesOfEitherSide()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 5, gap: 0.0);
            var first = Resolve(design);
            var idsBefore = first.Structure.Modules.Select(module => module.ModuleId).ToList();

            var snapshot = new PushBackResolver(Catalog).Snapshot(first);
            snapshot.SideB = design.SideB;
            snapshot.Composite = design.Composite;
            snapshot.Composite.Gap = 24.0;
            var widened = Resolve(snapshot);

            Assert.Equal(idsBefore, widened.Structure.Modules.Select(module => module.ModuleId).ToList());
            Assert.Equal(first.Structure.TotalLength + 24.0, widened.Structure.TotalLength, 6);
        }

        // ================= 1. Solo-A + Solo-B simultaneos ========================================================

        [Fact]
        public void MixedExclusiveSlots_ProduceCorrectBedsAndBom()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 4, slotsA: 4, slotsB: 4);
            design.SideB.Fronts[1] = null;          // ranura 1: solo A
            design.SideB.FrontConfigs[1] = null;
            design.Composite.AbsentSlotsA.Add(2);   // ranura 2: solo B

            var system = Resolve(design);
            var runs = PushBackRuns.Resolve(system);

            // Ranuras 0 y 3: encontradas (dos camas). Ranura 1: solo A. Ranura 2: solo B.
            Assert.Equal(2, runs.Runs.Count(run => run.Slot == 0));
            Assert.Single(runs.Runs, run => run.Slot == 1);
            Assert.Single(runs.Runs, run => run.Slot == 2);
            Assert.Equal(2, runs.Runs.Count(run => run.Slot == 3));
            Assert.Equal(PushBackSide.A, runs.Runs.Single(run => run.Slot == 1).LowSide);
            Assert.Equal(PushBackSide.B, runs.Runs.Single(run => run.Slot == 2).LowSide);

            var bom = PushBackBomBuilder.Build(system, Catalog);
            var beds = bom.Components.Where(c => c.Category == SystemBomBuilder.Cama).Sum(c => c.Quantity);
            Assert.Equal(runs.Runs.Count, beds);   // una cama por ejecucion fisica, ni una mas
        }

        [Fact]
        public void MixedExclusiveSlots_SurviveASaveAndLoad()
        {
            var design = Design(PushBackCellTopology.Encontradas, deepA: 5, deepB: 4, slotsA: 4, slotsB: 4);
            design.SideB.Fronts[1] = null;
            design.SideB.FrontConfigs[1] = null;
            design.Composite.AbsentSlotsA.Add(2);

            var json = System.Text.Json.JsonSerializer.Serialize(
                RackCad.Application.Persistence.PushBackDesignDocument.FromDomain(design));
            var restored = System.Text.Json.JsonSerializer
                .Deserialize<RackCad.Application.Persistence.PushBackDesignDocument>(json).ToDomain();

            var before = Resolve(design);
            var after = Resolve(restored);

            Assert.Equal(before.Structure.TotalLength, after.Structure.TotalLength, 6);
            Assert.Equal(before.Structure.Fronts.Count, after.Structure.Fronts.Count);
            Assert.Null(after.Composite.SideB.Front(1));
            Assert.Null(after.Composite.SideA.Front(2));
            Assert.Equal(
                PushBackRuns.Resolve(before).Runs.Count,
                PushBackRuns.Resolve(after).Runs.Count);
        }
    }
}
