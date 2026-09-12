using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-47 gate G17 — vincular y desvincular la holgura vertical desde el editor Selectivo REAL.
    ///
    /// <para>
    /// La ventana no resuelve nada: recibe las opciones compatibles ya filtradas y devuelve un intent dirigido
    /// por IDENTIDAD. Eso es lo que hace que dos variables homónimas sigan siendo dos: el nombre es texto que
    /// el usuario edita, y elegir por nombre dejaría de funcionar el día que alguien repita uno.
    /// </para>
    /// <para>
    /// Mientras el vínculo está activo la caja de holgura sigue deshabilitada —como en G12—, así que guardar
    /// no puede desvincular en silencio. Desvincular es un gesto explícito y no un efecto secundario.
    /// </para>
    /// </summary>
    public sealed class SelectiveBindingUiTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const string RackId = "GUID-SEL-G17";
        private const string VarX = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string VarY = "11111111-2222-3333-4444-555555555555";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = clearance,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
            };

            var bay = new SelectiveBayDesign { FloorBeam = true };

            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0,
                });
            }

            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Doc(string variableId = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackId, "Selectivo G17");

            if (variableId != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(variableId),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static ProjectVariablesDocument Registro(params (string Id, string Name, double Value)[] variables)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = variables.Select(variable => new ProjectVariableDocument
            {
                VariableId = variable.Id,
                Name = variable.Name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = variable.Value },
            }).ToList();

            return document;
        }

        /// <summary>Abre la ventana real con el rack ya resuelto y las opciones ACREDITADAS cargadas.</summary>
        private static RackSelectiveWindow Abrir(
            SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
        {
            var read = ProjectVariablesReadResult.Readable(registry);
            var open = SelectiveEditorOpen.Resolve(authored, read);
            Assert.True(open.IsOpen);

            var options = LinkedPropertyOptions.ForProperty(ProjectPropertyIds.SelectiveVerticalClearance, read);
            Assert.True(options.IsUsable);

            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.SetProjectVariables(options.Options);
            window.LoadExisting(authored, open.Design, open.LinkedPropertyStates);
            return window;
        }

        private static T Control<T>(RackSelectiveWindow window, string name)
            where T : System.Windows.FrameworkElement
            => EditorWindowTestSupport.Find<T>(window, element => element.Name == name);

        private static LinkedPropertyEditor Editor(RackSelectiveWindow window)
            => Control<LinkedPropertyEditor>(window, "ClearanceEditor");

        // ================================================================ las opciones que se ofrecen

        [Fact]
        public void SE_OFRECEN_LAS_VARIABLES_DE_LONGITUD_COMPATIBLES()
        {
            var count = StaTestRunner.Run(() =>
                Editor(Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Otra", 12.0))))
                    .Session.Candidates.Count);

            Assert.Equal(2, count);
        }

        [Fact]
        public void CADA_OPCION_LLEVA_SU_IDENTIDAD()
        {
            var ids = StaTestRunner.Run(() =>
                Editor(Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Otra", 12.0))))
                    .Session.Candidates.Select(option => option.VariableId.Value).ToList());

            Assert.Contains(VarX, ids);
            Assert.Contains(VarY, ids);
        }

        /// <summary>
        /// I-48 G4C, prueba 11. Dos variables pueden llamarse igual: el nombre no es identidad, la lista no las
        /// funde, y cada opcion ensena un fragmento de su identidad para que una persona pueda distinguirlas
        /// (Proposal V2 R-06 — el desambiguador es obligatorio, no decoracion).
        /// </summary>
        [Fact]
        public void DOS_HOMONIMAS_APARECEN_LAS_DOS_Y_SE_DISTINGUEN_EN_PANTALLA()
        {
            var (count, names, displays) = StaTestRunner.Run(() =>
            {
                var candidates = Editor(Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Holgura", 10.0))))
                    .Session.Candidates;

                return (candidates.Count,
                    candidates.Select(o => o.Name).ToList(),
                    candidates.Select(o => o.DisplayText).ToList());
            });

            Assert.Equal(2, count);
            Assert.All(names, name => Assert.Equal("Holgura", name));

            // Mismo nombre Y mismo valor: solo el fragmento de identidad las separa.
            Assert.NotEqual(displays[0], displays[1]);
        }

        /// <summary>I-48 G4C, prueba 9/10: la seleccion explicita porta el VariableId, nunca el nombre.</summary>
        [Fact]
        public void DOS_HOMONIMAS_SE_ELIGEN_POR_SEPARADO_POR_IDENTIDAD()
        {
            var final = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Holgura", 12.0)));
                var editor = Editor(window);

                var segunda = editor.Session.Candidates.First(o => o.LiteralValue == 12.0);
                Assert.True(editor.Session.TrySelect(segunda.VariableId, out _));

                return editor.FinalState;
            });

            Assert.True(final.IsReference);
            Assert.Equal(VariableId.Parse(VarY), final.Source.VariableId);
        }

        [Fact]
        public void SIN_VARIABLES_COMPATIBLES_NO_HAY_NADA_QUE_ELEGIR()
        {
            var count = StaTestRunner.Run(() => Editor(Abrir(Doc(), Registro())).Session.Candidates.Count);

            Assert.Equal(0, count);
        }

        // ================================================================ el estado que se lee

        [Fact]
        public void VINCULADO_SE_MUESTRA_LA_VARIABLE_SU_VALOR_EN_VIGOR_Y_EL_CONGELADO()
        {
            var text = StaTestRunner.Run(() =>
                Control<TextBlock>(Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0))), "ClearanceStateText").Text);

            Assert.Contains("Holgura General", text);
            Assert.Contains("10", text);

            // Y el literal congelado tambien se lee: el usuario tiene que saber a que numero volveria.
            Assert.Contains("6", text);
        }

        /// <summary>
        /// I-48 G4C. El campo gobernado SI se edita (Proposal V2 R-11), y eso es el cambio de comportamiento
        /// que el consenso decidio: teclear un numero NO desvincula todavia, crea un borrador.
        /// </summary>
        [Fact]
        public void VINCULADO_EL_CAMPO_SE_EDITA_Y_TECLEAR_NO_DESVINCULA()
        {
            var (text, editable, sigueVinculada, draft) = StaTestRunner.Run(() =>
            {
                var editor = Editor(Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0))));
                var inicial = editor.Box.Text;
                var puedeEditar = !editor.Box.IsReadOnly && editor.Box.IsEnabled;

                editor.Box.Text = "7";

                return (inicial, puedeEditar, editor.FinalState.IsReference, editor.Session.Draft);
            });

            Assert.Equal("=Holgura General", text);
            Assert.True(editable);
            Assert.True(sigueVinculada);
            Assert.Equal(LinkedPropertyDraftKind.DraftLiteral, draft);
        }

        /// <summary>I-48 G4C, prueba 27: desvincular sigue siendo un gesto EXPLICITO — Enter sobre un numero.</summary>
        [Fact]
        public void DESVINCULAR_SIGUE_SIENDO_UN_GESTO_EXPLICITO()
        {
            var (traseLostFocus, trasEnter) = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0)));
                var editor = Editor(window);

                editor.Box.Text = "7";

                // Perder el foco hacia OTRO control de la ventana NO desvincula.
                editor.HandleFocusLeaving(Control<TextBox>(window, "NameBox"));
                var despuesDeFoco = editor.FinalState.IsReference;

                Assert.True(editor.Session.TryCommitByEnter(out _));
                return (despuesDeFoco, editor.FinalState.IsReference);
            });

            Assert.True(traseLostFocus);
            Assert.False(trasEnter);
        }

        /// <summary>
        /// I-48 G4E: el estado NOMBRA la propiedad, porque ya hay mas de una. Antes bastaba decir
        /// «literal»; con dos propiedades vinculables, no decir cual seria ambiguo.
        /// </summary>
        [Fact]
        public void SIN_VINCULO_EL_ESTADO_DICE_QUE_ES_UN_LITERAL_Y_DE_QUE_PROPIEDAD()
        {
            var (holgura, tolerancia) = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(), Registro((VarX, "Holgura", 10.0)));
                return (Control<TextBlock>(window, "ClearanceStateText").Text,
                    Control<TextBlock>(window, "ToleranceStateText").Text);
            });

            Assert.Contains("Holgura vertical", holgura);
            Assert.Contains("literal", holgura);

            // Y la SEGUNDA propiedad tambien se describe, por el mismo camino.
            Assert.Contains("Tolerancia horizontal", tolerancia);
            Assert.Contains("literal", tolerancia);
        }

        // ================================================================ 12: foco compuesto

        /// <summary>
        /// I-48 G4C, prueba 12 — la que hace verdadero el caso 1 de 20.13. Mover el foco del texto a la lista
        /// de sugerencias del MISMO control no es que el compuesto pierda el foco, asi que no compromete nada.
        /// Sin esta regla, abrir la lista convertiria el caso 1 en el caso 2.
        /// </summary>
        [Fact]
        public void MOVER_EL_FOCO_A_LA_LISTA_INTERNA_NO_ES_UN_LOSTFOCUS_DEL_COMPUESTO()
        {
            var (ownsBox, ownsList, comprometido, draft) = StaTestRunner.Run(() =>
            {
                var editor = Editor(Abrir(Doc(), Registro((VarX, "Holgura", 10.0))));

                editor.Box.Text = "7"; // draft sobre un literal: LostFocus SI podria comprometerlo

                var dentroTexto = editor.OwnsFocus(editor.Box);
                var dentroLista = editor.OwnsFocus(editor.Candidates);

                // El foco se mueve a la lista del propio control: nada debe comprometerse.
                editor.HandleFocusLeaving(editor.Candidates);

                return (dentroTexto, dentroLista, editor.FinalState.CommittedLiteral, editor.Session.Draft);
            });

            Assert.True(ownsBox);
            Assert.True(ownsList);

            // Sigue en 6 y el borrador sigue vivo.
            Assert.Equal(6.0, comprometido);
            Assert.Equal(LinkedPropertyDraftKind.DraftLiteral, draft);
        }

        // ================================================================ 13, 14: C4

        /// <summary>
        /// I-48 G4C, pruebas 13 y 14. Un borrador que cambia la FUENTE bloquea una frontera generica de
        /// escritura, no se convierte solo, y el foco vuelve al campo que la bloqueo.
        /// </summary>
        [Fact]
        public void UN_BORRADOR_QUE_CAMBIA_LA_FUENTE_BLOQUEA_ACTUALIZAR_Y_DEVUELVE_EL_FOCO()
        {
            var (pidioInsertar, sigueVinculada, tieneFoco, status) = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0)));
                var editor = Editor(window);

                editor.Box.Text = "7"; // Reference -> DraftLiteral, sin Enter

                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                return (window.InsertRequested,
                    editor.FinalState.IsReference,
                    editor.HasFocus,
                    Control<TextBlock>(window, "StatusText").Text);
            });

            // La frontera NO pasa...
            Assert.False(pidioInsertar);

            // ...la fuente NO se convierte en silencio...
            Assert.True(sigueVinculada);

            // ...y el usuario acaba DENTRO del campo que tiene que arreglar.
            Assert.True(tieneFoco);
            Assert.Contains("Enter", status);
            Assert.Contains("Escape", status);
        }

        /// <summary>I-48 G4C, prueba 15: un borrador que CONSERVA la fuente sigue el protocolo normal.</summary>
        [Fact]
        public void UN_BORRADOR_QUE_CONSERVA_LA_FUENTE_SE_COMPROMETE_EN_LA_FRONTERA()
        {
            var (pidioInsertar, literal) = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(), Registro((VarX, "Holgura", 10.0)));
                var editor = Editor(window);

                editor.Box.Text = "9"; // Literal -> Literal

                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                return (window.InsertRequested, editor.FinalState.CommittedLiteral);
            });

            Assert.True(pidioInsertar);
            Assert.Equal(9.0, literal);
        }

        // ================================================================ 16: cerrar no es C4

        /// <summary>
        /// I-48 G4C, prueba 16. Cerrar sin guardar NO es una frontera de escritura: un borrador invalido o
        /// pendiente no puede atrapar al usuario dentro de la ventana.
        /// </summary>
        [Fact]
        public void UN_BORRADOR_PENDIENTE_NO_IMPIDE_CERRAR_SIN_GUARDAR()
        {
            var (cerro, pidioInsertar) = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0)));
                var editor = Editor(window);

                editor.Box.Text = "=sin resolver"; // consulta pendiente, que ademas cambiaria la fuente

                EditorWindowTestSupport.ClickByContent(window, "Cerrar");

                return (!window.IsVisible, window.InsertRequested);
            });

            Assert.True(cerro);
            Assert.False(pidioInsertar); // y nada se persiste
        }

        // ================================================================ la reparacion no vive aqui

        [Fact]
        public void EL_EDITOR_NO_OFRECE_REPARAR()
        {
            var found = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0)));

                return EditorWindowTestSupport.FindAll<Button>(window)
                    .Any(button => (button.Content as string ?? string.Empty).Contains("Reparar"));
            });

            Assert.False(found);
        }
    }
}
