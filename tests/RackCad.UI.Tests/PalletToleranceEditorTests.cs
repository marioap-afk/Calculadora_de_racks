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
    /// I-48 gate G4E — la segunda propiedad real EN SU SUPERFICIE.
    ///
    /// <para>
    /// Registrar el descriptor no basta: si el usuario no puede editar la tolerancia desde el editor Selectivo
    /// con la misma UX vinculable, la segunda propiedad existe para la maquina y no para la persona. Y la
    /// prueba de que la UX es REUSABLE es que la tolerancia no tiene control propio — es otra instancia del
    /// mismo <see cref="LinkedPropertyEditor"/>, con la misma clase y las mismas reglas.
    /// </para>
    /// <para>
    /// Lo que solo se puede comprobar aqui: que hay DOS instancias, que ambas participan del protocolo C4 de
    /// dos fases, y que guardar un rack con la tolerancia vinculada y sin tocar nada conserva su literal
    /// congelado — el equivalente real de la prueba 28 de la holgura.
    /// </para>
    /// </summary>
    public class PalletToleranceEditorTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarX = "11111111-1111-1111-1111-111111111111";

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static PropertyId Tolerance => ProjectPropertyIds.SelectivePalletTolerance;

        private static PropertyId Clearance => ProjectPropertyIds.SelectiveVerticalClearance;

        private static SelectivePalletDesign Diseno(double tolerance = 4.0, double clearance = 6.0)
        {
            var design = new SelectivePalletDesign
            {
                PostId = "POSTE_A",
                PostPeralte = 3.0,
                PalletTolerance = tolerance,
                VerticalClearance = clearance,
                PalletDepth = 48.0,
            };

            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        /// <summary>El rack guardado: tolerancia 4 congelada y gobernada por X; holgura 6 literal.</summary>
        private static SelectivePalletDesignDocument Authored(string toleranceBoundTo = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(), RackId, "Rack A");

            if (toleranceBoundTo != null)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [ProjectPropertyIds.SelectivePalletToleranceToken] =
                        SelectivePropertyValueDocument.ToProjectVariable(toleranceBoundTo),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        /// <summary>El registro: X vale 9.</summary>
        private static ProjectVariablesReadResult Registro()
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = VarX,
                    Name = "Tolerancia General",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 9.0 },
                },
            };

            return ProjectVariablesReadResult.Readable(document);
        }

        private static RackSelectiveWindow Abrir(SelectivePalletDesignDocument authored, ProjectVariablesReadResult read)
        {
            var open = SelectiveEditorOpen.Resolve(authored, read);
            Assert.True(open.IsOpen);

            var options = LinkedPropertyOptions.ForProperty(Tolerance, read);
            Assert.True(options.IsUsable);

            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.SetProjectVariables(options.Options);
            window.LoadExisting(authored, open.Design, open.LinkedPropertyStates);
            return window;
        }

        /// <summary>
        /// Abre la ventana, ejecuta <paramref name="body"/> y la CIERRA.
        ///
        /// <para>
        /// Cerrarla no es cosmetico. La suite de UI comparte un unico hilo STA, asi que una ventana que sigue
        /// viva despues de su test —y mas si se quedo con el foco de teclado, cosa que hacen las pruebas de C4
        /// al devolverlo al campo ofensivo— es estado que el siguiente test hereda. Es la misma higiene que ya
        /// aplican otras suites de ventana del repositorio, y sin ella esta clase resultaba intermitente.
        /// </para>
        /// </summary>
        private static T Con<T>(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult read,
            System.Func<RackSelectiveWindow, T> body)
            => StaTestRunner.Run(() =>
            {
                var window = Abrir(authored, read);

                try
                {
                    return body(window);
                }
                finally
                {
                    window.Close();
                }
            });

        private static T Control<T>(RackSelectiveWindow window, string name)
            where T : System.Windows.FrameworkElement
            => EditorWindowTestSupport.Find<T>(window, element => element.Name == name);

        // ================================================================ H. la MISMA clase, otra instancia

        /// <summary>
        /// G4E (H). La UX vinculable es REUSABLE: la tolerancia no tiene un control propio, tiene otra
        /// instancia del mismo. Si alguien hubiera copiado el editor, habria dos clases y esta prueba lo diria.
        /// </summary>
        [Fact]
        public void LA_TOLERANCIA_USA_OTRA_INSTANCIA_DEL_MISMO_EDITOR_REUSABLE()
        {
            var (clearance, tolerance, total) = Con(Authored(), Registro(), window =>
            {
                return (Control<LinkedPropertyEditor>(window, "ClearanceEditor"),
                    Control<LinkedPropertyEditor>(window, "ToleranceEditor"),
                    EditorWindowTestSupport.FindAll<LinkedPropertyEditor>(window).Count);
            });

            Assert.NotNull(clearance);
            Assert.NotNull(tolerance);

            // Dos instancias, no dos clases.
            Assert.NotSame(clearance, tolerance);
            Assert.Same(clearance.GetType(), tolerance.GetType());
            Assert.Equal(2, total);
        }

        [Fact]
        public void LA_TOLERANCIA_VINCULADA_SE_ENSENA_COMO_REFERENCIA_Y_CONSERVA_SU_CONGELADO()
        {
            var (texto, efectivo, congelado) = Con(Authored(VarX), Registro(), window =>
            {
                var editor = Control<LinkedPropertyEditor>(window, "ToleranceEditor");
                editor.Session.TryGetEffectiveValue(out var value);

                return (editor.Box.Text, value, editor.FinalState.CommittedLiteral);
            });

            Assert.Equal("=Tolerancia General", texto);
            Assert.Equal(9.0, efectivo);
            Assert.Equal(4.0, congelado);   // el literal del AUTHORED, no el de la variable
        }

        [Fact]
        public void LA_TOLERANCIA_SIN_VINCULO_SE_ENSENA_COMO_SU_LITERAL()
        {
            var texto = Con(Authored(), Registro(), window =>
                Control<LinkedPropertyEditor>(window, "ToleranceEditor").Box.Text);

            Assert.Equal("4", texto);
        }

        // ================================================================ K. guardar vinculado y sin tocar

        /// <summary>
        /// G4E (K) — el equivalente REAL de la prueba 28, ahora para la tolerancia. Se abre un rack con la
        /// tolerancia vinculada, no se toca NADA y se pulsa «Actualizar».
        ///
        /// <para>
        /// El campo ensena el EFECTIVO (9) y el dibujo lo refleja, pero el documento persistido tiene que seguir
        /// congelando el 4: si el efectivo se copiase sobre el literal, el dia que el usuario desvincule
        /// gobernaria 9 en lugar del numero que el mismo congelo.
        /// </para>
        /// </summary>
        [Fact]
        public void GUARDAR_LA_TOLERANCIA_VINCULADA_SIN_TOCAR_NADA_NO_PISA_SU_LITERAL_CONGELADO()
        {
            var authored = Authored(VarX);
            var registro = Registro();

            var (salida, finales) = Con(authored, registro, window =>
            {
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                return (window.DesignToInsert, window.LinkedPropertyFinalStates);
            });

            Assert.NotNull(salida);
            Assert.Equal(9.0, salida.PalletTolerance);   // el dibujo refleja el efectivo

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, salida, finales, registro, RackId, "Rack A");

            Assert.True(reconciled.IsSuccess);
            Assert.Equal(4.0, reconciled.Authored.PalletTolerance);   // el congelado sobrevive
            Assert.True(reconciled.Authored.TryGetBinding(Tolerance, out var bound));
            Assert.Equal(Id(VarX), bound);
            Assert.Equal(9.0, reconciled.Effective.PalletTolerance);

            // Y la holgura, que nadie vinculo, sigue siendo su literal.
            Assert.False(reconciled.Authored.TryGetBinding(Clearance, out _));
            Assert.Equal(6.0, reconciled.Authored.VerticalClearance);
        }

        [Fact]
        public void EL_ESTADO_FINAL_LLEVA_LAS_DOS_PROPIEDADES()
        {
            var finales = Con(Authored(VarX), Registro(), window => window.LinkedPropertyFinalStates);

            Assert.Equal(2, finales.Count);
            Assert.True(finales[Tolerance].IsReference);
            Assert.False(finales[Clearance].IsReference);
        }

        // ================================================================ L. C4 con DOS editores

        /// <summary>
        /// G4E (L). Un borrador que cambia la FUENTE en la TOLERANCIA bloquea la frontera, devuelve el foco a
        /// ESE editor y no aplica nada — ni siquiera lo del otro, que estaba limpio.
        ///
        /// <para>
        /// Es la prueba de que C4 no asume que solo hay un editor vinculable: si la ventana solo estacionara el
        /// primero, este borrador pasaria sin validarse.
        /// </para>
        /// </summary>
        [Fact]
        public void UN_BORRADOR_DE_LA_TOLERANCIA_QUE_CAMBIA_LA_FUENTE_BLOQUEA_LA_FRONTERA()
        {
            var (pidioInsertar, sigueVinculada, foco, status) = Con(Authored(VarX), Registro(), window =>
            {
                var tolerancia = Control<LinkedPropertyEditor>(window, "ToleranceEditor");

                tolerancia.Box.Text = "7";   // Reference -> DraftLiteral, sin Enter

                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                return (window.InsertRequested,
                    tolerancia.FinalState.IsReference,
                    tolerancia.HasFocus,
                    Control<TextBlock>(window, "StatusText").Text);
            });

            Assert.False(pidioInsertar);
            Assert.True(sigueVinculada);
            Assert.True(foco);
            Assert.Contains("Tolerancia horizontal", status);
            Assert.Contains("Enter", status);
        }

        /// <summary>El inverso: el borrador vive en la HOLGURA y la tolerancia esta limpia.</summary>
        [Fact]
        public void UN_BORRADOR_DE_LA_HOLGURA_QUE_CAMBIA_LA_FUENTE_TAMBIEN_BLOQUEA()
        {
            // Esta vez la HOLGURA es la vinculada.
            var doc = SelectivePalletDesignDocument.From(Diseno(), RackId, "Rack A");
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] =
                    SelectivePropertyValueDocument.ToProjectVariable(VarX),
            };
            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;

            var (pidioInsertar, toleranciaIntacta, status) = Con(doc, Registro(), window =>
            {
                Control<LinkedPropertyEditor>(window, "ClearanceEditor").Box.Text = "7";

                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                return (window.InsertRequested,
                    Control<LinkedPropertyEditor>(window, "ToleranceEditor").FinalState.CommittedLiteral,
                    Control<TextBlock>(window, "StatusText").Text);
            });

            Assert.False(pidioInsertar);

            // Nada parcial: la tolerancia, que estaba limpia, no se movio.
            Assert.Equal(4.0, toleranciaIntacta);
            Assert.Contains("Holgura vertical", status);
        }

        /// <summary>
        /// Los dos borradores que CONSERVAN la fuente se comprometen juntos en la frontera.
        ///
        /// <para>
        /// Se afirma sobre los DOS literales comprometidos y no sobre si el redibujo prospero: lo que este test
        /// decide es que la frontera generica estaciona y aplica ambos editores, y atar eso al exito del
        /// pipeline de dibujo lo haria sensible a cosas —el catalogo, la geometria— que no son su asunto.
        /// </para>
        /// </summary>
        [Fact]
        public void DOS_BORRADORES_QUE_CONSERVAN_LA_FUENTE_SE_COMPROMETEN_LOS_DOS()
        {
            var (tolerancia, holgura, sucios) = Con(Authored(), Registro(), window =>
            {
                Control<LinkedPropertyEditor>(window, "ToleranceEditor").Box.Text = "5.5";
                Control<LinkedPropertyEditor>(window, "ClearanceEditor").Box.Text = "8";

                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                var tol = Control<LinkedPropertyEditor>(window, "ToleranceEditor");
                var clr = Control<LinkedPropertyEditor>(window, "ClearanceEditor");

                return (tol.FinalState.CommittedLiteral, clr.FinalState.CommittedLiteral,
                    tol.IsDirty || clr.IsDirty);
            });

            // Los DOS se comprometieron, no solo el primero.
            Assert.Equal(5.5, tolerancia);
            Assert.Equal(8.0, holgura);

            // Y ninguno quedo pendiente.
            Assert.False(sucios);
        }

        // ================================================================ M. las opciones vienen de Application

        [Fact]
        public void LOS_DOS_EDITORES_RECIBEN_LAS_MISMAS_OPCIONES_COMPATIBLES()
        {
            var (deLaHolgura, deLaTolerancia) = Con(Authored(), Registro(), window =>
            {
                return (Control<LinkedPropertyEditor>(window, "ClearanceEditor")
                        .Session.Candidates.Select(o => o.VariableId.Value).ToArray(),
                    Control<LinkedPropertyEditor>(window, "ToleranceEditor")
                        .Session.Candidates.Select(o => o.VariableId.Value).ToArray());
            });

            // Ambas son Length, asi que comparten conjunto compatible — pero cada editor lo recibio de
            // Application, no lo dedujo.
            Assert.Equal(new[] { VarX }, deLaHolgura);
            Assert.Equal(new[] { VarX }, deLaTolerancia);
        }

        // ================================================================ vincular desde el editor real

        /// <summary>
        /// El gesto completo por la ventana real: teclear «=», seleccionar por identidad y reconciliar. El
        /// literal congelado es el COMPROMETIDO, no el efectivo que el campo mostraba.
        /// </summary>
        [Fact]
        public void VINCULAR_LA_TOLERANCIA_DESDE_EL_EDITOR_CONGELA_SU_LITERAL_COMPROMETIDO()
        {
            var authored = Authored();
            var registro = Registro();

            var finales = Con(authored, registro, window =>
            {
                var editor = Control<LinkedPropertyEditor>(window, "ToleranceEditor");

                editor.Box.Text = "=Tolerancia";
                Assert.True(editor.Session.TrySelect(Id(VarX), out _));

                return window.LinkedPropertyFinalStates;
            });

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, Diseno(tolerance: 9.0), finales, registro, RackId, "Rack A");

            Assert.True(reconciled.IsSuccess);
            Assert.Equal(4.0, reconciled.Authored.PalletTolerance);
            Assert.True(reconciled.Authored.TryGetBinding(Tolerance, out _));
            Assert.Equal(9.0, reconciled.Effective.PalletTolerance);
        }
    }
}
