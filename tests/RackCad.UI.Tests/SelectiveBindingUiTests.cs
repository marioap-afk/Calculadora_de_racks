using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
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

        /// <summary>Abre la ventana real con el rack ya resuelto y las opciones compatibles cargadas.</summary>
        private static RackSelectiveWindow Abrir(
            SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
        {
            var open = SelectiveEditorOpen.Resolve(authored, ProjectVariablesReadResult.Readable(registry));
            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);

            window.SetProjectVariables(SelectiveBindingOptions.ForLength(registry));
            window.LoadExisting(authored, open.Design, open.VerticalClearance);
            return window;
        }

        private static T Control<T>(RackSelectiveWindow window, string name)
            where T : System.Windows.FrameworkElement
            => EditorWindowTestSupport.Find<T>(window, element => element.Name == name);

        // ================================================================ 1-3: las opciones

        [Fact]
        public void SE_LISTAN_LAS_VARIABLES_DE_LONGITUD_COMPATIBLES()
        {
            var count = StaTestRunner.Run(() =>
                Control<ComboBox>(Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Otra", 12.0))),
                    "ClearanceVariableBox").Items.Count);

            Assert.Equal(2, count);
        }

        [Fact]
        public void CADA_OPCION_LLEVA_SU_IDENTIDAD()
        {
            var ids = StaTestRunner.Run(() =>
                Control<ComboBox>(Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Otra", 12.0))),
                        "ClearanceVariableBox")
                    .Items.Cast<ProjectVariableOption>().Select(option => option.Id.Value).ToList());

            Assert.Contains(VarX, ids);
            Assert.Contains(VarY, ids);
        }

        /// <summary>Dos variables pueden llamarse igual: el nombre no es identidad y la lista no las funde.</summary>
        [Fact]
        public void DOS_HOMONIMAS_APARECEN_LAS_DOS()
        {
            var (count, names) = StaTestRunner.Run(() =>
            {
                var box = Control<ComboBox>(
                    Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Holgura", 12.0))), "ClearanceVariableBox");

                return (box.Items.Count, box.Items.Cast<ProjectVariableOption>().Select(o => o.Name).ToList());
            });

            Assert.Equal(2, count);
            Assert.All(names, name => Assert.Equal("Holgura", name));
        }

        [Fact]
        public void DOS_HOMONIMAS_SE_ELIGEN_POR_SEPARADO()
        {
            var intent = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(), Registro((VarX, "Holgura", 10.0), (VarY, "Holgura", 12.0)));
                var box = Control<ComboBox>(window, "ClearanceVariableBox");

                box.SelectedItem = box.Items.Cast<ProjectVariableOption>().First(o => o.Value == 12.0);
                EditorWindowTestSupport.ClickNamed(window, "LinkClearanceButton");
                return window.BindingIntent;
            });

            Assert.NotNull(intent);
            Assert.Equal(SelectiveBindingIntentKind.Link, intent.Kind);
            Assert.Equal(VariableId.Parse(VarY), intent.VariableId);
        }

        // ================================================================ 5, 8: cuándo se puede vincular

        [Fact]
        public void SIN_VARIABLES_COMPATIBLES_NO_SE_PUEDE_VINCULAR()
        {
            var enabled = StaTestRunner.Run(() =>
                Control<Button>(Abrir(Doc(), Registro()), "LinkClearanceButton").IsEnabled);

            Assert.False(enabled);
        }

        [Fact]
        public void SIN_VINCULO_SE_PUEDE_VINCULAR_ELIGIENDO_UNA()
        {
            var (before, after) = StaTestRunner.Run(() =>
            {
                var window = Abrir(Doc(), Registro((VarX, "Holgura", 10.0)));
                var button = Control<Button>(window, "LinkClearanceButton");
                var box = Control<ComboBox>(window, "ClearanceVariableBox");

                var initial = button.IsEnabled;
                box.SelectedIndex = 0;
                return (initial, button.IsEnabled);
            });

            Assert.False(before);
            Assert.True(after);
        }

        // ================================================================ 6, 7, 9: el estado vinculado

        [Fact]
        public void VINCULADO_SE_MUESTRA_LA_VARIABLE_Y_SU_VALOR_EN_VIGOR()
        {
            var text = StaTestRunner.Run(() =>
                Control<TextBlock>(Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0))), "ClearanceStateText").Text);

            Assert.Contains("Holgura General", text);
            Assert.Contains("10", text);
        }

        [Fact]
        public void VINCULADO_LA_CAJA_SIGUE_SIN_EDITARSE()
        {
            var (text, readOnly, enabled) = StaTestRunner.Run(() =>
            {
                var box = Control<TextBox>(Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0))), "ClearanceBox");
                return (box.Text, box.IsReadOnly, box.IsEnabled);
            });

            Assert.Equal("10", text);
            Assert.True(readOnly);
            Assert.False(enabled);
        }

        [Fact]
        public void DESVINCULAR_ES_UN_GESTO_EXPLICITO()
        {
            var (enabledUnbound, intent) = StaTestRunner.Run(() =>
            {
                var sinVinculo = Control<Button>(Abrir(Doc(), Registro((VarX, "Holgura", 10.0))), "UnlinkClearanceButton")
                    .IsEnabled;

                var window = Abrir(Doc(VarX), Registro((VarX, "Holgura General", 10.0)));
                EditorWindowTestSupport.ClickNamed(window, "UnlinkClearanceButton");

                return (sinVinculo, window.BindingIntent);
            });

            Assert.False(enabledUnbound);
            Assert.NotNull(intent);
            Assert.Equal(SelectiveBindingIntentKind.Unlink, intent.Kind);
            Assert.Equal(RackId, intent.RackId);
            Assert.Equal(Token, intent.PropertyId);
        }

        [Fact]
        public void SIN_VINCULO_EL_ESTADO_DICE_QUE_ES_UN_LITERAL()
        {
            var text = StaTestRunner.Run(() =>
                Control<TextBlock>(Abrir(Doc(), Registro((VarX, "Holgura", 10.0))), "ClearanceStateText").Text);

            Assert.Contains("Literal", text);
        }

        // ================================================================ 10: la reparación no vive aquí

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
