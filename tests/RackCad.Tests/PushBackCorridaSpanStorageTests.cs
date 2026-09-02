using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-S1, contrato del dueño) — EL HUECO ALARGA LA CAMA, NO LA ALIMENTA.
    ///
    /// <para>
    /// La demanda de una corrida se mide en posiciones de ALMACENAMIENTO; el hueco es estructura: se atraviesa. La
    /// colocacion comparaba la demanda contra la DISTANCIA FISICA acumulada —hueco incluido—, asi que un hueco
    /// grande la satisfacia antes de tiempo. Medido sobre 3 x 54 + hueco + 3 x 54 con demanda 6: con hueco 54 el
    /// extremo alto caia en el modulo 6 (258" de almacenamiento cruzado contra 312" exigidos) y con hueco 108 en el
    /// 5 (210"). Y en el limite, una demanda de 7 fondos sobre 6 posiciones se declaraba VALIDA porque el hueco
    /// completaba la longitud que faltaba.
    /// </para>
    /// </summary>
    public class PushBackCorridaSpanStorageTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Una corrida sobre 3 + 3 posiciones, con el hueco, el sentido y la demanda que se pidan.</summary>
        private static PushBackDesign Design(
            double gap,
            int corridaDepth,
            PushBackRunDirection direction = PushBackRunDirection.AToB,
            bool separator = false)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: 3, deepB: 3, levelsA: 1, levelsB: 1, gap: gap);
            design.Composite.DefaultTopology = PushBackCellTopology.Corrida;
            design.Composite.DefaultDirection = direction;
            design.Composite.CentralSeparator = separator;
            design.Composite.SetCorridaDepth(0, 0, corridaDepth);
            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design)
            => new PushBackResolver(Catalog).Resolve(design);

        private static PushBackCellBed Bed(PushBackSystem system)
            => system.Composite.Cells.Single().Beds.Single();

        /// <summary>El marco en el que la corrida se resuelve (identidad o espejo, segun el sentido).</summary>
        private static DynamicRackSystem Frame(PushBackSystem system)
            => PushBackRuns.Resolve(system).Runs.Single().Source.Structure;

        private static PushBackBedSpan.PushBackResolvedSpan Span(PushBackSystem system)
            => PushBackBedSpan.ResolveSpan(Frame(system), Bed(system).DemandPositions);

        /// <summary>El almacenamiento acumulado hasta un apoyo (1-based), con la contribucion canonica.</summary>
        private static double StorageUpTo(DynamicRackSystem frame, int endPosition)
            => frame.Modules.Take(endPosition).Sum(PushBackBedSpan.StorageContribution);

        // ---------------------------------------------------------------- el defecto medido

        [Fact]
        public void Corrida_GapEqualToStorageModule_DoesNotConsumeOneDepth()
        {
            var withoutGap = Resolve(Design(0.0, 6));
            var withGap = Resolve(Design(54.0, 6));

            // El hueco mide exactamente lo que una posicion: si la colocacion lo contara, el alto caeria un modulo
            // antes. La demanda es la misma en los dos.
            Assert.Equal(
                withoutGap.Composite.Cells.Single().Beds.Single().RequiredBedLength,
                withGap.Composite.Cells.Single().Beds.Single().RequiredBedLength,
                6);

            var span = Span(withGap);
            Assert.Equal(7, span.EndPosition);
            Assert.True(span.Fits);
            Assert.True(
                StorageUpTo(Frame(withGap), span.EndPosition) >= span.RequiredLength - PushBackBedSpan.Tolerance,
                "el apoyo elegido recorre suficientes posiciones de almacenamiento");
        }

        [Fact]
        public void Corrida_BtoA_GapDoesNotConsumeStorageDemand()
        {
            var system = Resolve(Design(54.0, 6, PushBackRunDirection.BToA));
            var span = Span(system);

            Assert.Equal(7, span.EndPosition);
            Assert.True(span.Fits);
            Assert.True(
                StorageUpTo(Frame(system), span.EndPosition) >= span.RequiredLength - PushBackBedSpan.Tolerance);
        }

        [Fact]
        public void Corrida_CentralSeparatorDoesNotConsumeStorageDemand()
        {
            var withoutSeparator = Resolve(Design(54.0, 6));
            var withSeparator = Resolve(Design(54.0, 6, separator: true));

            var bare = Span(withoutSeparator);
            var separated = Span(withSeparator);

            Assert.Equal(bare.RequiredLength, separated.RequiredLength, 6);
            Assert.Equal(bare.EndPosition, separated.EndPosition);
            Assert.Equal(bare.ResolvedLength, separated.ResolvedLength, 6);

            // Y la pieza del separador no cuenta como almacenamiento en ningun punto del recorrido.
            var frame = Frame(withSeparator);
            var interfaceModule = frame.Modules.Single(module =>
                !PushBackCompositeStructure.IsStoragePosition(module));
            Assert.Equal(0.0, PushBackBedSpan.StorageContribution(interfaceModule));
        }

        [Fact]
        public void Corrida_LargeGapCannotMakeInsufficientStorageFit()
        {
            // 7 fondos sobre 6 posiciones de almacenamiento: no caben. Un hueco de 108" añade longitud fisica de
            // sobra —lo DISPONIBLE supera lo EXIGIDO— y aun asi la cama no cabe.
            var system = Resolve(Design(108.0, 7));
            var bed = Bed(system);
            var span = Span(system);

            Assert.True(bed.RequiredBedLength <= bed.AvailableBedSpan + PushBackBedSpan.Tolerance);
            Assert.False(span.Fits);
            Assert.False(bed.IsValid);
            Assert.Contains("almacenamiento", bed.DisabledReason, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(
                PushBackCompositeDiagnostics.Evaluate(system), diagnostic => diagnostic.IsBlocking);
        }

        [Fact]
        public void Corrida_SmallGapStillContributesNoStorage()
        {
            // Un hueco menor que una posicion tampoco aporta una fraccion de fondo.
            var system = Resolve(Design(12.0, 6));
            var span = Span(system);

            Assert.Equal(7, span.EndPosition);
            Assert.True(span.Fits);
        }

        [Fact]
        public void Corrida_ShortDemandStillEndsBeforeTheGap()
        {
            // No se sobrecorrige: una demanda que se satisface antes del hueco no lo cruza.
            var system = Resolve(Design(54.0, 2));
            var span = Span(system);

            Assert.Equal(2, span.EndPosition);
            Assert.True(span.Fits);
            Assert.True(span.ResolvedLength < span.AvailableLength);
        }

        [Fact]
        public void Corrida_GapZeroIsUnchanged()
        {
            var system = Resolve(Design(0.0, 6));
            var span = Span(system);

            Assert.Equal(7, span.EndPosition);
            Assert.Equal(span.RequiredLength, span.ResolvedLength, 6);
            Assert.True(span.Fits);
        }

        // ---------------------------------------------------------------- la autoridad

        [Fact]
        public void ResolveSpan_TracksStorageDemandSeparatelyFromPhysicalSpan()
        {
            var system = Resolve(Design(108.0, 6));
            var frame = Frame(system);
            var span = Span(system);

            var storage = StorageUpTo(frame, span.EndPosition);
            var physical = frame.Modules[span.EndPosition - 1].EndX - frame.Modules[0].StartX;

            // Las dos magnitudes existen y son DISTINTAS: la fisica incluye el hueco, la de almacenamiento no.
            Assert.Equal(physical, span.ResolvedLength, 6);
            Assert.True(physical > storage, "la distancia fisica incluye el hueco");
            Assert.True(storage >= span.RequiredLength - PushBackBedSpan.Tolerance);
            Assert.Equal(storage, span.StorageAvailableLength, 6);   // aqui se agoto el almacenamiento
        }

        [Fact]
        public void StorageContribution_IsZeroForTheInterfaceAndItsSeparator()
        {
            foreach (var separator in new[] { false, true })
            {
                var frame = Frame(Resolve(Design(54.0, 6, separator: separator)));
                var interfaceModule = frame.Modules.Single(module =>
                    !PushBackCompositeStructure.IsStoragePosition(module));

                Assert.Equal(0.0, PushBackBedSpan.StorageContribution(interfaceModule));
                Assert.True(interfaceModule.Length > 0.0, "la interfaz mide algo fisicamente");
            }
        }

        [Fact]
        public void DemandLength_AndResolveSpan_ShareTheSameContribution()
        {
            // La demanda y la colocacion miden lo mismo: si no, una podria exigir lo que la otra no cuenta.
            var system = Resolve(Design(54.0, 6));
            var frame = Frame(system);
            var span = Span(system);

            Assert.Equal(
                PushBackBedSpan.DemandLength(frame, span.DemandPositions, PushBackBedAnchor.Outer),
                span.RequiredLength,
                6);
            Assert.Equal(
                frame.Modules.Sum(PushBackBedSpan.StorageContribution),
                span.StorageAvailableLength,
                6);
        }

        // ---------------------------------------------------------------- el contrato Required <= Resolved <= Available

        [Fact]
        public void Corrida_RequiredResolvedAvailableStayOrdered()
        {
            foreach (var gap in new[] { 0.0, 12.0, 54.0, 108.0 })
            {
                var span = Span(Resolve(Design(gap, 6)));

                Assert.True(span.Fits);
                Assert.True(span.RequiredLength <= span.ResolvedLength + PushBackBedSpan.Tolerance);
                Assert.True(span.ResolvedLength <= span.AvailableLength + PushBackBedSpan.Tolerance);
            }
        }

        // ---------------------------------------------------------------- tarimas y BOM

        [Fact]
        public void Corrida_AllRequiredPalletPositionsFallWithinResolvedPhysicalBed()
        {
            var system = Resolve(Design(54.0, 6));
            var run = PushBackRuns.Resolve(system).Runs.Single();
            var frame = run.Source.Structure;
            var span = Span(system);

            // Las posiciones de almacenamiento que la demanda exige, enumeradas sobre el marco, caen todas dentro
            // del apoyo resuelto.
            var storage = frame.Modules.Where(PushBackCompositeStructure.IsStoragePosition).ToList();
            var low = frame.Modules[0].StartX;
            var high = low + span.ResolvedLength;

            Assert.Equal(6, storage.Count);
            Assert.All(storage, module => Assert.True(
                module.EndX <= high + PushBackBedSpan.Tolerance,
                FormattableString.Invariant(
                    $"la posicion {module.Index + 1} termina en {module.EndX:0.##} y el alto esta en {high:0.##}")));
        }

        [Fact]
        public void Corrida_BomBedLengthUsesResolvedSupportAfterStorageDemandIsMet()
        {
            var system = Resolve(Design(54.0, 6));
            var span = Span(system);
            var bed = PushBackBomBuilder.Build(system, Catalog).Components
                .Single(component => component.Category == SystemBomBuilder.Cama);

            // Lo que se fabrica es la longitud FISICA resuelta, no la exigida: la cama cruza el hueco de verdad.
            Assert.Equal(span.ResolvedLength, bed.Length, 2);
            Assert.True(bed.Length > span.RequiredLength);
        }

        [Fact]
        public void Corrida_RunConsumesTheCorrectedSpan()
        {
            // La autoridad es una: PushBackRuns coloca la cama con el span corregido, sin una regla propia.
            var system = Resolve(Design(54.0, 6));
            var frame = Frame(system);
            var span = Span(system);

            Assert.Equal(1, frame.Fronts[0].DepthStartPosition);
            Assert.Equal(span.EndPosition, frame.Fronts[0].PalletsDeep);
        }
    }
}
