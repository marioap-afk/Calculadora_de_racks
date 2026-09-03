using System;
using System.Collections.Generic;
using System.Windows.Controls;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6C: el contrato del campo pendiente, probado directamente sobre el control.
    /// <para>
    /// INV-14 dice que hacer <c>Show</c> sobre un campo con una edición SIN RESOLVER es un defecto, y O-43-02 dice
    /// que retipear el mismo valor visible CUENTA como edición. Juntas cierran un hueco que no se ve desde la
    /// ventana: si la protección dependiera de que el texto pendiente difiera del comprometido, una intención
    /// explícita que acaba en el mismo texto podría descartarse en silencio.
    /// </para>
    /// <para>
    /// Las tres salidas de una edición pendiente son distintas y esta clase las separa: <c>Show</c> NO puede
    /// resolverla, <c>Reset</c> la descarta a propósito, y un commit la consume.
    /// </para>
    /// </summary>
    public class PendingTextFieldTests
    {
        /// <summary>Un campo de prueba sobre un committed que el propio test controla.</summary>
        private sealed class Harness
        {
            internal Harness(string committed)
            {
                Committed = committed;
                Box = new TextBox { Text = committed };
                Field = new PendingTextField<string>(
                    Box,
                    "Campo",
                    text => string.IsNullOrWhiteSpace(text)
                        ? PendingParse<string>.Invalid("vacío")
                        : PendingParse<string>.Valid(text),
                    value => { Applied.Add(value); Committed = value; },
                    () => Committed);
            }

            internal TextBox Box { get; }

            internal PendingTextField<string> Field { get; }

            internal string Committed { get; private set; }

            internal List<string> Applied { get; } = new List<string>();

            /// <summary>Teclear de verdad: se pasa por un valor intermedio, como cualquier persona.</summary>
            internal void Retype(string text)
            {
                Box.Text = string.Empty;
                Box.Text = text;
            }
        }

        // ---- La protección de INV-14 no depende del texto ----

        [Fact]
        public void ShowOnADirtyField_ThrowsEvenWhenTheTypedTextEqualsTheCommittedOne()
        {
            // El caso que se colaba: el usuario retipea "2" sobre un committed "2". La intención es explícita
            // (O-43-02), pero como los textos coinciden un Show la borraría sin que nadie lo note.
            var thrown = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                h.Retype("2");
                Assert.True(h.Field.IsDirty);

                try
                {
                    h.Field.Show(h.Committed); // mismo texto que la caja
                    return (string)null;
                }
                catch (InvalidOperationException ex)
                {
                    return ex.Message;
                }
            });

            Assert.NotNull(thrown);
            Assert.Contains("Campo", thrown);
        }

        [Fact]
        public void ShowOnADirtyField_ThrowsWhenTheTypedTextDiffers()
        {
            var thrown = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                h.Retype("5");
                try
                {
                    h.Field.Show(h.Committed);
                    return (string)null;
                }
                catch (InvalidOperationException ex)
                {
                    return ex.Message;
                }
            });

            Assert.NotNull(thrown);
        }

        [Fact]
        public void ShowOnACleanField_IsFine()
        {
            var (text, dirty) = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                h.Field.Show("7"); // reflejar un committed que cambió por otra vía
                return (h.Box.Text, h.Field.IsDirty);
            });

            Assert.Equal("7", text);
            Assert.False(dirty); // una escritura programática nunca marca edición
        }

        // ---- Las dos salidas que SÍ resuelven una edición ----

        [Fact]
        public void ResetDiscardsADirtyEditOnPurpose_EvenWhenTheTextMatches()
        {
            var (dirty, text, applied) = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                h.Retype("2");
                h.Field.ResetToCommitted(); // descarte EXPLÍCITO: auto-reparación o carga
                return (h.Field.IsDirty, h.Box.Text, h.Applied.Count);
            });

            Assert.False(dirty);
            Assert.Equal("2", text);
            Assert.Equal(0, applied); // descartar no es aplicar
        }

        [Fact]
        public void ACommitConsumesTheDirtyEdit_AndThenShowIsAllowed()
        {
            var (applied, dirtyAfterApply, text) = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                h.Retype("2"); // el mismo valor: sigue siendo una edición deliberada

                Assert.True(h.Field.TryStage(out var error));
                Assert.Null(error);
                h.Field.ApplyStaged();
                var afterApply = h.Field.IsDirty;

                h.Field.ShowCommitted(); // ahora sí: la edición está resuelta
                return (h.Applied.ToArray(), afterApply, h.Box.Text);
            });

            Assert.Equal(new[] { "2" }, applied); // retipear el mismo valor APLICA (O-43-02)
            Assert.False(dirtyAfterApply);
            Assert.Equal("2", text);
        }

        // ---- Fase 1 sin mutar, y el campo limpio que no aporta nada ----

        [Fact]
        public void PreparingAnInvalidValue_ReportsItWithoutApplying()
        {
            var (ok, error, applied) = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                h.Retype("   ");
                var result = h.Field.TryStage(out var err);
                return (result, err, h.Applied.Count);
            });

            Assert.False(ok);
            Assert.Equal("vacío", error);
            Assert.Equal(0, applied);
        }

        [Fact]
        public void ACleanFieldStagesNothing_AndApplyingItDoesNothing()
        {
            var applied = StaTestRunner.Run(() =>
            {
                var h = new Harness("2");
                Assert.True(h.Field.TryStage(out _));
                h.Field.ApplyStaged();
                return h.Applied.Count;
            });

            Assert.Equal(0, applied); // idempotencia: sin edición no hay escritura
        }
    }
}
