using System.Collections.Generic;
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
    /// I-47 gate G12 + I-48 gate G4C — lo que el editor Selectivo REAL enseña y devuelve cuando la holgura
    /// vertical está gobernada por una variable.
    ///
    /// <para>
    /// La semántica pura ya está probada en el Core (sesión de edición y reconciler). Aquí se comprueba lo
    /// único que solo puede comprobarse con la ventana de verdad: que el campo enseña <c>=Nombre</c> con el
    /// efectivo disponible, que el literal congelado del authored sobrevive a abrir y guardar, y que el
    /// documento final lo produce el reconciler y no la ventana.
    /// </para>
    /// <para>
    /// <b>Cambio de comportamiento deliberado (Proposal V2 R-11).</b> Hasta G4C el campo gobernado quedaba
    /// <c>IsReadOnly</c> y desvincular era un botón. Ahora el mismo campo admite edición: teclear un número no
    /// desvincula — crea un borrador — y solo Enter lo confirma. Queda registrado como tal para la validación
    /// del dueño.
    /// </para>
    /// </summary>
    public sealed class SelectiveEditorBindingTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const string RackId = "GUID-SEL-G12";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";

        private static SelectivePalletDesign MinimalDesign(double clearance)
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

        private static SelectivePalletDesignDocument Authored(bool bound)
        {
            var document = SelectivePalletDesignDocument.From(MinimalDesign(6.0), RackId, "Selectivo G12");

            if (bound)
            {
                document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [ProjectPropertyIds.SelectiveVerticalClearanceToken] =
                        SelectivePropertyValueDocument.ToProjectVariable(VarId),
                };
                document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return document;
        }

        private static ProjectVariablesReadResult Registro(double value)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = VarId,
                    Name = "Holgura estándar",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };

            return ProjectVariablesReadResult.Readable(document);
        }

        /// <summary>Las variables ACREDITADAS que la ventana recibe, como se las da el Plugin.</summary>
        private static IReadOnlyList<LinkedPropertyOption> Opciones(ProjectVariablesReadResult registro)
        {
            var options = LinkedPropertyOptions.ForProperty(
                ProjectPropertyIds.SelectiveVerticalClearance, registro);
            Assert.True(options.IsUsable);
            return options.Options;
        }

        /// <summary>Abre la ventana como lo hace RACKEDITAR: opciones acreditadas + estado authored.</summary>
        private static RackSelectiveWindow Abrir(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult registro)
        {
            var open = SelectiveEditorOpen.Resolve(authored, registro);
            Assert.True(open.IsOpen);

            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.SetProjectVariables(Opciones(registro));
            window.LoadExisting(authored, open.Design, open.VerticalClearanceState);
            return window;
        }

        private static LinkedPropertyEditor Editor(RackSelectiveWindow window)
            => EditorWindowTestSupport.Find<LinkedPropertyEditor>(window, e => e.Name == "ClearanceEditor");

        // ================================================================ 23, 24: lo que el campo ensena

        /// <summary>
        /// I-48 G4C, prueba 24. Vinculado, el campo ensena <c>=Nombre</c>, el efectivo esta disponible, y el
        /// literal congelado del authored NO se pierde. Y —cambio deliberado de V2 R-11— el campo SI se edita.
        /// </summary>
        [Fact]
        public void BOUND_EL_CAMPO_ENSENA_LA_VARIABLE_Y_CONSERVA_EL_LITERAL_CONGELADO()
        {
            var (text, editable, effective, frozen, isReference) = StaTestRunner.Run(() =>
            {
                var editor = Editor(Abrir(Authored(bound: true), Registro(10.0)));
                editor.Session.TryGetEffectiveValue(out var value);

                return (editor.Box.Text,
                    !editor.Box.IsReadOnly && editor.Box.IsEnabled,
                    value,
                    editor.FinalState.CommittedLiteral,
                    editor.FinalState.IsReference);
            });

            Assert.Equal("=Holgura estándar", text);
            Assert.Equal(10.0, effective);
            Assert.Equal(6.0, frozen);
            Assert.True(isReference);

            // V2 R-11: un campo gobernado deja de ser read-only. Es el cambio de comportamiento que el consenso
            // decidio y que la validacion del dueno tiene registrado.
            Assert.True(editable);
        }

        /// <summary>I-48 G4C, prueba 23: sin vinculo, el literal se ve y se edita.</summary>
        [Fact]
        public void UNBOUND_EL_CAMPO_ENSENA_EL_LITERAL_Y_SE_EDITA()
        {
            var (text, editable, isReference) = StaTestRunner.Run(() =>
            {
                var editor = Editor(Abrir(Authored(bound: false), ProjectVariablesReadResult.Absent()));
                return (editor.Box.Text, !editor.Box.IsReadOnly && editor.Box.IsEnabled, editor.FinalState.IsReference);
            });

            Assert.Equal("6", text);
            Assert.True(editable);
            Assert.False(isReference);
        }

        /// <summary>La carga historica de un argumento —biblioteca, tests— sigue siendo el caso sin vinculo.</summary>
        [Fact]
        public void LA_CARGA_HISTORICA_SIGUE_SIENDO_EL_CASO_SIN_VINCULO()
        {
            var (text, isReference) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(Authored(bound: false));

                var editor = Editor(window);
                return (editor.Box.Text, editor.FinalState.IsReference);
            });

            Assert.Equal("6", text);
            Assert.False(isReference);
        }

        // ================================================================ 28: guardar vinculado y sin tocar

        /// <summary>
        /// I-48 G4C, prueba 28 — la critica. Se abre un rack vinculado, no se toca NADA y se pulsa
        /// «Actualizar».
        ///
        /// <para>
        /// El campo ensena el EFECTIVO (10) porque es el valor en vigor, asi que el diseno que sale lo lleva y
        /// el dibujo lo refleja. Pero el documento que se persiste tiene que seguir congelando el 6: si el
        /// efectivo se copiase sobre el literal, el dia que el usuario desvincule gobernaria 10 en lugar del
        /// numero que el mismo congelo, y nada habria fallado por el camino.
        /// </para>
        /// </summary>
        [Fact]
        public void BOUND_GUARDAR_SIN_TOCAR_NADA_NO_COPIA_EL_EFECTIVO_SOBRE_EL_LITERAL_CONGELADO()
        {
            var authored = Authored(bound: true);
            var registro = Registro(10.0);

            var (salida, finales) = StaTestRunner.Run(() =>
            {
                var window = Abrir(authored, registro);
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                return (window.DesignToInsert, window.LinkedPropertyFinalStates);
            });

            Assert.NotNull(salida);
            Assert.Equal(10.0, salida.VerticalClearance); // el dibujo refleja el efectivo

            // Y el documento final lo produce el RECONCILER, no la ventana.
            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, salida, finales, registro, RackId, "Selectivo G12");

            Assert.True(reconciled.IsSuccess);
            Assert.Equal(6.0, reconciled.Authored.VerticalClearance); // el congelado sobrevive
            Assert.True(reconciled.Authored.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(VarId), id);
            Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
        }

        // ================================================================ 25, 26: 20.13 por la ventana real

        /// <summary>
        /// I-48 G4C, prueba 25. Se teclea 7 sobre el literal 6 y se vincula SIN comprometerlo: congela 6.
        /// </summary>
        [Fact]
        public void PRUEBA_25_TECLEAR_Y_VINCULAR_SIN_COMPROMETER_CONGELA_EL_LITERAL_ANTERIOR()
        {
            var authored = Authored(bound: false);
            var registro = Registro(10.0);

            var finales = StaTestRunner.Run(() =>
            {
                var window = Abrir(authored, registro);
                var editor = Editor(window);

                editor.Box.Text = "7";                                   // draft, sin comprometer
                editor.Session.TrySelect(VariableId.Parse(VarId), out _); // seleccion explicita

                return window.LinkedPropertyFinalStates;
            });

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, MinimalDesign(10.0), finales, registro, RackId, "Selectivo G12");

            Assert.True(reconciled.IsSuccess);
            Assert.Equal(6.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
        }

        /// <summary>
        /// I-48 G4C, prueba 26. El mismo gesto pero comprometiendo el 7 antes de vincular: congela 7.
        /// </summary>
        [Fact]
        public void PRUEBA_26_TECLEAR_COMPROMETER_Y_VINCULAR_CONGELA_EL_LITERAL_NUEVO()
        {
            var authored = Authored(bound: false);
            var registro = Registro(10.0);

            var finales = StaTestRunner.Run(() =>
            {
                var window = Abrir(authored, registro);
                var editor = Editor(window);

                editor.Box.Text = "7";
                Assert.True(editor.Session.TryCommitByEnter(out _));      // commit explicito
                editor.Session.TrySelect(VariableId.Parse(VarId), out _);

                return window.LinkedPropertyFinalStates;
            });

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, MinimalDesign(10.0), finales, registro, RackId, "Selectivo G12");

            Assert.True(reconciled.IsSuccess);
            Assert.Equal(7.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
        }

        // ================================================================ 27: desvincular tecleando un numero

        /// <summary>
        /// I-48 G4C, prueba 27. Vinculado, el usuario teclea un numero y pulsa Enter: el documento final se
        /// queda SIN vinculo y con ese numero. Sin boton «Desvincular» de por medio.
        /// </summary>
        [Fact]
        public void PRUEBA_27_UN_NUMERO_CON_ENTER_SOBRE_UNA_VARIABLE_DESVINCULA()
        {
            var authored = Authored(bound: true);
            var registro = Registro(10.0);

            var finales = StaTestRunner.Run(() =>
            {
                var window = Abrir(authored, registro);
                var editor = Editor(window);

                editor.Box.Text = "7";
                Assert.True(editor.Session.TryCommitByEnter(out _));

                return window.LinkedPropertyFinalStates;
            });

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, MinimalDesign(7.0), finales, registro, RackId, "Selectivo G12");

            Assert.True(reconciled.IsSuccess);
            Assert.False(reconciled.Authored.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out _));
            Assert.Equal(7.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(7.0, reconciled.Effective.VerticalClearance);
        }
    }
}
