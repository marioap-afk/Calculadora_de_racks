using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-S1B, contrato del dueño) — LA VALIDEZ DE UNA CAMA TIENE DOS EJES, Y EL HUECO SOLO ESTA EN UNO.
    ///
    /// <list type="bullet">
    /// <item><b>CAPACIDAD DE ALMACENAMIENTO</b> — ¿hay tantas posiciones que alojan tarima como fondos pide? El
    /// hueco no anade ninguna, por grande que sea.</item>
    /// <item><b>SPAN FISICO</b> — ¿hay un apoyo cuya distancia satisface la longitud exigida? Aqui el hueco SI
    /// suma: alarga la distancia entre el extremo bajo y ese apoyo.</item>
    /// </list>
    ///
    /// <para>
    /// <b>Un hallazgo de esta ronda, medido y no supuesto.</b> Con la definicion vigente de la demanda —la suma de
    /// las longitudes de las N primeras posiciones de ESE marco— el segundo eje se cumple SIEMPRE que se cumple el
    /// primero: la distancia fisica hasta un apoyo es la suma de las MISMAS longitudes mas, quiza, el hueco, asi que
    /// nunca es menor. De ahi que «capacidad suficiente pero span insuficiente» no exista hoy: no se encontro ni un
    /// caso en 432 configuraciones de fondos, hueco y demanda. Las dos condiciones se comprueban por separado de
    /// todos modos, para que ninguna definicion futura de la demanda las colapse en silencio.
    /// </para>
    /// </summary>
    public class PushBackCorridaDualValidityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(
            double gap,
            int corridaDepth,
            int deepA = 3,
            int deepB = 3,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            bool separator = false)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: deepA, deepB: deepB, levelsA: 1, levelsB: 1, gap: gap);
            design.Composite.DefaultTopology = PushBackCellTopology.Corrida;
            design.Composite.DefaultDirection = direction;
            design.Composite.CentralSeparator = separator;
            design.Composite.SetCorridaDepth(0, 0, corridaDepth);
            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design)
            => new PushBackResolver(Catalog).Resolve(design);

        private static DynamicRackSystem Frame(PushBackSystem system)
            => PushBackRuns.Resolve(system).Runs.Single().Source.Structure;

        private static PushBackBedSpan.PushBackResolvedSpan Span(PushBackDesign design)
        {
            var system = Resolve(design);
            var demand = system.Composite.Cells.Single().Beds.Single().DemandPositions;
            return PushBackBedSpan.ResolveSpan(Frame(system), demand);
        }

        private static double PhysicalSpan(DynamicRackSystem frame)
            => frame.Modules[frame.Modules.Count - 1].EndX - frame.Modules[0].StartX;

        // ---------------------------------------------------------------- el hueco no sustituye posiciones

        [Fact]
        public void Corrida_LargeGapCannotReplaceMissingStoragePosition()
        {
            // 7 fondos sobre 6 posiciones. El hueco de 200" da longitud fisica de sobra y no cambia nada.
            foreach (var gap in new[] { 0.0, 54.0, 200.0, 500.0 })
            {
                var design = Design(gap, 7);
                var span = Span(design);
                var bed = Resolve(design).Composite.Cells.Single().Beds.Single();

                Assert.Equal(6, PushBackBedSpan.StorageCapacity(Frame(Resolve(design))));
                Assert.False(span.Fits);
                Assert.False(bed.IsValid);
            }
        }

        [Fact]
        public void Corrida_GapDoesNotChangeStorageCapacity()
        {
            var capacities = new[] { 0.0, 12.0, 54.0, 200.0 }
                .Select(gap => PushBackBedSpan.StorageCapacity(Frame(Resolve(Design(gap, 6)))))
                .Distinct()
                .ToList();

            Assert.Single(capacities);
            Assert.Equal(6, capacities[0]);
        }

        [Fact]
        public void Corrida_GapCanChangeAvailablePhysicalSpan()
        {
            var tight = Span(Design(0.0, 6));
            var loose = Span(Design(54.0, 6));

            // La demanda y la capacidad no se mueven; la longitud fisica si.
            Assert.Equal(tight.RequiredLength, loose.RequiredLength, 6);
            Assert.Equal(tight.StorageAvailableLength, loose.StorageAvailableLength, 6);
            Assert.Equal(tight.AvailableLength + 54.0, loose.AvailableLength, 6);
            Assert.True(loose.ResolvedLength > tight.ResolvedLength);
        }

        [Fact]
        public void Corrida_SeparatorDoesNotChangeStorageCapacity()
        {
            var bare = Frame(Resolve(Design(54.0, 6)));
            var separated = Frame(Resolve(Design(54.0, 6, separator: true)));

            Assert.Equal(PushBackBedSpan.StorageCapacity(bare), PushBackBedSpan.StorageCapacity(separated));
            Assert.Equal(
                bare.Modules.Sum(PushBackBedSpan.StorageContribution),
                separated.Modules.Sum(PushBackBedSpan.StorageContribution),
                6);
        }

        // ---------------------------------------------------------------- los dos ejes, por separado

        [Fact]
        public void Corrida_StorageCapacityAndPhysicalSpanAreIndependentConstraints()
        {
            // El hueco mueve UNO de los dos ejes y deja el otro intacto: por eso son independientes.
            var tight = Design(0.0, 6);
            var loose = Design(200.0, 6);

            Assert.Equal(
                PushBackBedSpan.StorageCapacity(Frame(Resolve(tight))),
                PushBackBedSpan.StorageCapacity(Frame(Resolve(loose))));
            Assert.True(PhysicalSpan(Frame(Resolve(loose))) > PhysicalSpan(Frame(Resolve(tight))));
        }

        /// <summary>
        /// LA DEMOSTRACION del hallazgo: mientras la demanda se mida como la suma de las longitudes de las primeras
        /// posiciones de ALMACENAMIENTO del marco, satisfacerla en almacenamiento implica satisfacerla en distancia
        /// fisica. La distancia hasta un apoyo suma las mismas longitudes y ademas el hueco.
        /// </summary>
        [Fact]
        public void StorageDemandMet_AlwaysImpliesPhysicalSpanMet_ByConstruction()
        {
            foreach (var gap in new[] { 0.0, 6.0, 54.0, 200.0 })
            {
                var frame = Frame(Resolve(Design(gap, 6)));
                var low = frame.Modules[0].StartX;
                var storage = 0.0;

                foreach (var module in frame.Modules)
                {
                    storage += PushBackBedSpan.StorageContribution(module);
                    var physical = module.EndX - low;
                    Assert.True(
                        physical >= storage - 1e-9,
                        FormattableString.Invariant($"gap={gap}: la distancia fisica {physical:0.###} ")
                            + FormattableString.Invariant(
                                $"no puede ser menor que el almacenamiento cruzado {storage:0.###}"));
                }
            }
        }

        [Fact]
        public void Corrida_SufficientStorageIsNeverBlockedByPhysicalSpan()
        {
            // Barrido: con capacidad suficiente, la cama SIEMPRE cabe. Si algun dia dejara de ser cierto, este test
            // lo dice y habra que decidir que hace el hueco en ese caso.
            foreach (var deepA in new[] { 2, 3, 5 })
            foreach (var deepB in new[] { 2, 3, 4 })
            foreach (var gap in new[] { 0.0, 6.0, 54.0, 200.0 })
            {
                var capacity = PushBackBedSpan.StorageCapacity(Frame(Resolve(Design(gap, 1, deepA, deepB))));
                for (var depth = 1; depth <= capacity; depth++)
                {
                    var span = Span(Design(gap, depth, deepA, deepB));
                    Assert.True(
                        span.Fits,
                        FormattableString.Invariant(
                                $"A={deepA} B={deepB} gap={gap} demanda={depth} capacidad={capacity}: ")
                            + "con capacidad suficiente la cama debe caber");
                    Assert.True(span.RequiredLength <= span.ResolvedLength + PushBackBedSpan.Tolerance);
                    Assert.True(span.ResolvedLength <= span.AvailableLength + PushBackBedSpan.Tolerance);
                }
            }
        }

        [Fact]
        public void Corrida_InsufficientStorageIsAlwaysBlocked_WhateverTheGap()
        {
            foreach (var deepA in new[] { 2, 3 })
            foreach (var deepB in new[] { 2, 3 })
            foreach (var gap in new[] { 0.0, 54.0, 500.0 })
            {
                var capacity = PushBackBedSpan.StorageCapacity(Frame(Resolve(Design(gap, 1, deepA, deepB))));
                var span = Span(Design(gap, capacity + 1, deepA, deepB));

                Assert.False(
                    span.Fits,
                    FormattableString.Invariant(
                        $"A={deepA} B={deepB} gap={gap}: {capacity + 1} fondos sobre {capacity} posiciones no caben"));
            }
        }

        // ---------------------------------------------------------------- direccion y demanda corta

        [Fact]
        public void Corrida_AtoB_UsesBothConstraints()
        {
            var valid = Span(Design(54.0, 6, direction: PushBackRunDirection.AToB));
            var invalid = Span(Design(54.0, 7, direction: PushBackRunDirection.AToB));

            Assert.True(valid.Fits);
            Assert.False(invalid.Fits);
        }

        [Fact]
        public void Corrida_BtoA_UsesBothConstraints()
        {
            var valid = Span(Design(54.0, 6, direction: PushBackRunDirection.BToA));
            var invalid = Span(Design(54.0, 7, direction: PushBackRunDirection.BToA));

            Assert.True(valid.Fits);
            Assert.False(invalid.Fits);
        }

        [Fact]
        public void Corrida_RequiredResolvedAvailableInvariantStillHolds()
        {
            foreach (var gap in new[] { 0.0, 12.0, 54.0, 200.0 })
            foreach (var depth in new[] { 1, 3, 6 })
            {
                var span = Span(Design(gap, depth));

                Assert.True(span.Fits);
                Assert.True(span.RequiredLength <= span.ResolvedLength + PushBackBedSpan.Tolerance);
                Assert.True(span.ResolvedLength <= span.AvailableLength + PushBackBedSpan.Tolerance);
            }
        }
    }
}
