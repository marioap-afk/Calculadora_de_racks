using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.ProjectVariables;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-48 gate G4C — lo que solo el CONTROL puede probar.
    ///
    /// <para>
    /// La semantica entera vive en <c>LinkedPropertyEditSession</c> y se prueba en el Core sin WPF. Lo que
    /// queda aqui es exactamente lo que WPF anade y por tanto puede romper por su cuenta: que el raton
    /// comprometa la opcion que el usuario pincho, que la navegacion de teclado mueva la seleccion, que la
    /// lista NO deje una opcion seleccionada al teclear —porque entonces Enter confirmaria una referencia que
    /// nadie eligio— y que el foco dentro del propio popup no cuente como perder el foco.
    /// </para>
    /// </summary>
    public class LinkedPropertyEditorControlTests
    {
        private const string IdX = "11111111-1111-1111-1111-111111111111";
        private const string IdY = "22222222-2222-2222-2222-222222222222";

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static IReadOnlyList<LinkedPropertyOption> Opciones()
            => new[]
            {
                new LinkedPropertyOption(Id(IdX), "Holgura General", VariableType.Length, 10.0),
                new LinkedPropertyOption(Id(IdY), "Holgura Estrecha", VariableType.Length, 12.0),
            };

        /// <summary>Un editor sembrado con un literal y las dos opciones, dentro de una ventana real.</summary>
        private static LinkedPropertyEditor Editor(LinkedPropertyEditState committed = null)
        {
            var editor = new LinkedPropertyEditor { Label = "Holgura vertical" };
            var window = new Window { Content = editor, Width = 200, Height = 80 };
            window.Show();
            window.Hide();

            editor.Attach(new LinkedPropertyEditSession(
                committed ?? LinkedPropertyEditState.Literal(6.0), Opciones()));

            return editor;
        }

        // ================================================================ el raton

        /// <summary>I-48 G4C, prueba 9: pinchar una opcion compromete SU identidad exacta.</summary>
        [Fact]
        public void EL_RATON_COMPROMETE_LA_IDENTIDAD_DE_LA_OPCION_PINCHADA()
        {
            var final = StaTestRunner.Run(() =>
            {
                var editor = Editor();

                editor.Box.Text = "=holgura";                 // abre la consulta
                editor.Candidates.SelectedIndex = 1;          // el usuario pincha la SEGUNDA
                editor.SelectFromList();

                return editor.FinalState;
            });

            Assert.True(final.IsReference);
            Assert.Equal(Id(IdY), final.Source.VariableId);
            Assert.Equal(6.0, final.CommittedLiteral); // y congela el literal comprometido, no el texto
        }

        // ================================================================ el teclado

        /// <summary>I-48 G4C, prueba 10: navegar con el teclado mueve la seleccion, y Enter la confirma.</summary>
        [Fact]
        public void LA_NAVEGACION_DE_TECLADO_MUEVE_LA_SELECCION_Y_ENTER_LA_CONFIRMA()
        {
            var (indiceTrasBajar, final) = StaTestRunner.Run(() =>
            {
                var editor = Editor();

                editor.Box.Text = "=holgura";
                editor.MoveSelectionForTest(1);   // Abajo
                var indice = editor.Candidates.SelectedIndex;
                editor.CommitByEnterForTest();

                return (indice, editor.FinalState);
            });

            Assert.Equal(0, indiceTrasBajar);
            Assert.True(final.IsReference);
            Assert.Equal(Id(IdX), final.Source.VariableId);
        }

        // ================================================================ sin auto-seleccion

        /// <summary>
        /// I-48 G4C, prueba 7 al nivel del control. Teclear deja la lista SIN seleccion, incluso cuando el
        /// filtro deja un unico resultado: si quedase seleccionada, Enter confirmaria una referencia que el
        /// usuario no eligio, solo por haber escrito texto.
        /// </summary>
        [Fact]
        public void TECLEAR_NO_DEJA_NINGUNA_OPCION_SELECCIONADA_NI_CON_UN_UNICO_CANDIDATO()
        {
            var (candidatos, indice, esReferencia) = StaTestRunner.Run(() =>
            {
                var editor = Editor();

                editor.Box.Text = "=Estrecha"; // un unico candidato

                var count = editor.Candidates.Items.Count;
                var selected = editor.Candidates.SelectedIndex;

                editor.CommitByEnterForTest(); // Enter sin seleccion humana

                return (count, selected, editor.FinalState.IsReference);
            });

            Assert.Equal(1, candidatos);
            Assert.Equal(-1, indice);
            Assert.False(esReferencia);
        }

        [Fact]
        public void UNA_SELECCION_PREVIA_SE_LIMPIA_AL_SEGUIR_TECLEANDO()
        {
            var indice = StaTestRunner.Run(() =>
            {
                var editor = Editor();

                editor.Box.Text = "=holgura";
                editor.MoveSelectionForTest(1);
                Assert.Equal(0, editor.Candidates.SelectedIndex);

                editor.Box.Text = "=holgura g"; // sigue escribiendo
                return editor.Candidates.SelectedIndex;
            });

            Assert.Equal(-1, indice);
        }

        // ================================================================ foco compuesto

        /// <summary>
        /// I-48 G4C, prueba 12 al nivel del control: el popup pertenece al compuesto, asi que el foco dentro de
        /// el no es perder el foco y no compromete nada.
        /// </summary>
        [Fact]
        public void EL_POPUP_Y_SUS_ELEMENTOS_PERTENECEN_AL_COMPUESTO()
        {
            var (propio, ajeno, comprometido) = StaTestRunner.Run(() =>
            {
                var editor = Editor(LinkedPropertyEditState.Reference(4.0, Id(IdX)));
                var otro = new TextBox();

                editor.Box.Text = "7"; // draft que cambiaria la fuente

                var dentro = editor.OwnsFocus(editor.Candidates);
                var fuera = editor.OwnsFocus(otro);

                editor.HandleFocusLeaving(editor.Candidates);

                return (dentro, fuera, editor.FinalState.IsReference);
            });

            Assert.True(propio);
            Assert.False(ajeno);
            Assert.True(comprometido); // sigue vinculada: ni el popup ni LostFocus cambian la fuente
        }

        // ================================================================ Escape en el control

        [Fact]
        public void ESCAPE_EN_EL_CONTROL_REPINTA_EL_ESTADO_COMPROMETIDO_Y_CIERRA_LA_LISTA()
        {
            var (texto, esReferencia) = StaTestRunner.Run(() =>
            {
                var editor = Editor(LinkedPropertyEditState.Reference(4.0, Id(IdX)));

                editor.Box.Text = "=otra cosa";
                editor.ResetToCommitted();

                return (editor.Box.Text, editor.FinalState.IsReference);
            });

            Assert.Equal("=Holgura General", texto);
            Assert.True(esReferencia);
        }

        // ================================================================ protocolo de pendientes

        [Fact]
        public void EL_CONTROL_BLOQUEA_LA_FASE_1_NOMBRANDO_EL_CAMPO()
        {
            var (ok, error) = StaTestRunner.Run(() =>
            {
                var editor = Editor(LinkedPropertyEditState.Reference(4.0, Id(IdX)));
                editor.Box.Text = "7";

                var staged = editor.TryStage(out var reason);
                return (staged, reason);
            });

            Assert.False(ok);
            Assert.Contains("Holgura vertical", error);
            Assert.Contains("Enter", error);
        }

        [Fact]
        public void EL_CONTROL_APLICA_LA_FASE_2_SOLO_DE_LO_QUE_LA_FASE_1_ACEPTO()
        {
            var literal = StaTestRunner.Run(() =>
            {
                var editor = Editor();
                editor.Box.Text = "9";

                Assert.True(editor.TryStage(out _));
                Assert.Equal(6.0, editor.FinalState.CommittedLiteral); // fase 1 no muta

                editor.ApplyStaged();
                return editor.FinalState.CommittedLiteral;
            });

            Assert.Equal(9.0, literal);
        }
    }
}
