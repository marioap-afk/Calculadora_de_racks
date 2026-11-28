using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1b — CT-PBT1B-12: the criterion that blocks the output of the editor, taken out of the window. The
    /// lines are LITERAL (M-07 of the earlier task): the status text of each blocking case is exactly the one the window
    /// shows today.
    /// </summary>
    public sealed class PushBackEditorOutputGateTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>A healthy composite rack (the system of <c>PushBackOutputGateTests.Valid()</c>).</summary>
        private static PushBackSystem Valid()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 4, deepB: 4, levelsA: 2, levelsB: 2, gap: 0.0);
            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>A rack whose cell does NOT fit (the system of <c>PushBackOutputGateTests.Blocking()</c>).</summary>
        private static PushBackSystem Blocking()
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 8, deepB: 8, levelsA: 2, levelsB: 2, gap: 0.0);
            design.Composite.StructureOverrideA = PushBackCellDepth.MinimumPalletsDeep;
            design.Composite.StructureOverrideB = PushBackCellDepth.MinimumPalletsDeep;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static PushBackCompositeEditorState WithIntent(PushBackCompositeDesign intent)
        {
            var state = new PushBackCompositeEditorState();
            state.LoadComposite(intent);
            return state;
        }

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        private static string Line(List<string> log, string label, PushBackCompositeEditorState state, PushBackSystem system)
            => label + " bloqueado=" + Paso(log, () => PushBackEditorOutputGate.IsBlocked(state, system))
               + " estado=" + Paso(log, () => PushBackEditorOutputGate.StatusOr(state, system, "RESERVA"));

        [Fact]
        public void CT_PBT1B_12_TheOutputGateBlocksWithTheLiteralTextsOfTheWindow()
        {
            var previous = CultureInfo.CurrentCulture;
            var log = new List<string>();
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                var valid = Valid();
                var blocking = Blocking();

                log.Add(Line(log, "valido", WithIntent(new PushBackCompositeDesign()), valid));
                log.Add(Line(log, "hueco-negativo", WithIntent(new PushBackCompositeDesign { Gap = -1 }), valid));
                log.Add(Line(log, "separador-sin-hueco", WithIntent(new PushBackCompositeDesign { CentralSeparator = true, Gap = 0 }), valid));
                log.Add(Line(log, "override-no-construible", WithIntent(new PushBackCompositeDesign { StructureOverrideA = 1 }), valid));

                var clean = WithIntent(new PushBackCompositeDesign());
                var first = PushBackCompositeDiagnostics.Evaluate(blocking).FirstOrDefault(diagnostic => diagnostic.IsBlocking);
                log.Add("celda-no-cabe bloqueado=" + Paso(log, () => PushBackEditorOutputGate.IsBlocked(clean, blocking))
                    + " estado-igual-al-primer-bloqueante-de-Evaluate="
                    + (first != null && string.Equals(Paso(log, () => PushBackEditorOutputGate.StatusOr(clean, blocking, "RESERVA")), first.Message, StringComparison.Ordinal)));

                log.Add("RackBomOutputGate.For(Valid()).Allowed=" + RackBomOutputGate.For(valid).Allowed);
                log.Add("RackBomOutputGate.For(Blocking()).Allowed=" + RackBomOutputGate.For(blocking).Allowed);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }

            Assert.Equal(
                new[]
                {
                    "valido bloqueado=False estado=RESERVA",
                    "hueco-negativo bloqueado=True estado=El hueco (-1\") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor.",
                    "separador-sin-hueco bloqueado=True estado=El separador central necesita un hueco mayor que cero: sin el no hay donde colocarlo.",
                    "override-no-construible bloqueado=True estado=Lado A: la estructura manual (1) no es construible; el minimo fisico es 2 fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.",
                    "celda-no-cabe bloqueado=True estado-igual-al-primer-bloqueante-de-Evaluate=True",
                    "RackBomOutputGate.For(Valid()).Allowed=True",
                    "RackBomOutputGate.For(Blocking()).Allowed=False",
                },
                log);
        }
    }
}
