using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1C/H11, contrato del dueño) — LA PUERTA DE SALIDA.
    ///
    /// <para>
    /// Un diseño con diagnostico BLOQUEANTE no produce salida final, y el usuario sabe por que: el editor ya lo
    /// impedia y los comandos de AutoCAD consumen ahora la MISMA autoridad, no una segunda regla propia.
    /// </para>
    /// </summary>
    public class PushBackOutputGateTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---------------------------------------------------------------- fixtures

        /// <summary>Un rack compuesto sano.</summary>
        private static PushBackSystem Valid()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: 2, levelsB: 2, gap: 0.0);
            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>
        /// Un rack cuya celda NO CABE: su estructura manual es mas corta que la cama que la celda pide, que es el
        /// caso auditado (<c>RequiredBedLength &gt; AvailableBedSpan</c>).
        /// </summary>
        private static PushBackSystem Blocking()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 8, deepB: 8, levelsA: 2, levelsB: 2, gap: 0.0);
            design.Composite.StructureOverrideA = PushBackCellDepth.MinimumPalletsDeep;
            design.Composite.StructureOverrideB = PushBackCellDepth.MinimumPalletsDeep;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        // ---------------------------------------------------------------- la puerta

        [Fact]
        public void ValidPushBack_AllowsBomCommands()
        {
            var verdict = RackBomOutputGate.For(Valid());

            Assert.True(verdict.Allowed);
            Assert.Null(verdict.Reason);
        }

        [Fact]
        public void BlockingDiagnostic_DeniesOutput()
        {
            var system = Blocking();
            var diagnostics = PushBackCompositeDiagnostics.Evaluate(system);

            Assert.Contains(diagnostics, diagnostic => diagnostic.IsBlocking);

            var verdict = RackBomOutputGate.For(system);
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }

        [Fact]
        public void BlockingDiagnostic_CommandReturnsReason()
        {
            // El motivo es el que redacto la autoridad de diagnosticos: la puerta no lo reescribe.
            var system = Blocking();
            var expected = PushBackCompositeDiagnostics.Evaluate(system).First(d => d.IsBlocking).Message;

            Assert.Equal(expected, RackBomOutputGate.For(system).Reason);
        }

        [Fact]
        public void WarningDiagnostic_DoesNotBlockBomCommands()
        {
            // Un override de estructura POR DEBAJO de la propuesta es un AVISO, no un bloqueo: el rack se cotiza.
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 6, deepB: 6, levelsA: 2, levelsB: 2, gap: 0.0);
            design.Composite.StructureOverrideA = 5;
            for (var slot = 0; slot < design.Fronts.Count; slot++)
            {
                design.Fronts[slot].DefaultPalletsDeep = 5;
                if (design.SideB.FrontConfigs[slot] != null)
                {
                    design.SideB.FrontConfigs[slot].DefaultPalletsDeep = 5;
                }
            }

            var system = new PushBackResolver(Catalog).Resolve(design);
            var diagnostics = PushBackCompositeDiagnostics.Evaluate(system);

            Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.IsBlocking);
            Assert.True(RackBomOutputGate.For(system).Allowed);
        }

        [Fact]
        public void NullSystem_IsNotThisGatesBusiness()
        {
            // Un diseño ilegible lo reporta el camino del BOM con su propio aviso; la puerta no lo suplanta.
            Assert.True(RackBomOutputGate.For(null).Allowed);
        }

        // ---------------------------------------------------------------- el mensaje

        [Fact]
        public void MixedValidAndBlocking_RackBomTotalDoesNotSilentlyOmitInvalidRack()
        {
            var reason = RackBomOutputGate.For(Blocking()).Reason;
            var message = RackBomOutputGate.DescribeBlocked(new[]
            {
                new KeyValuePair<string, string>("PB-INVALIDO", reason),
            });

            Assert.Contains("PB-INVALIDO", message, StringComparison.Ordinal);
            Assert.Contains(reason, message, StringComparison.Ordinal);
            Assert.Contains("no se genera el listado", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UnreadableRack_IsNeverSkippedSilently()
        {
            var message = RackBomOutputGate.DescribeUnreadable("PB-ILEGIBLE");

            Assert.Contains("PB-ILEGIBLE", message, StringComparison.Ordinal);
            Assert.Contains("FUERA del listado", message, StringComparison.Ordinal);
        }

        [Fact]
        public void DescribeBlocked_NamesEveryBlockedRack()
        {
            var message = RackBomOutputGate.DescribeBlocked(new[]
            {
                new KeyValuePair<string, string>("PB-1", "motivo uno"),
                new KeyValuePair<string, string>("PB-2", "motivo dos"),
            });

            Assert.Contains("PB-1", message, StringComparison.Ordinal);
            Assert.Contains("PB-2", message, StringComparison.Ordinal);
            Assert.Contains("motivo uno", message, StringComparison.Ordinal);
            Assert.Contains("motivo dos", message, StringComparison.Ordinal);
            Assert.Equal(string.Empty, RackBomOutputGate.DescribeBlocked(null));
        }

    }
}
