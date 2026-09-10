using System.Collections.Generic;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-47 gate G16 — la ventana central de variables de proyecto, con la ventana REAL.
    ///
    /// <para>
    /// La ventana no decide nada: recibe una proyección pura de lo que hay en el dibujo y devuelve un INTENT
    /// por identidad. Lo que se comprueba aquí es exactamente eso — que presenta lo que recibe, que dirige por
    /// <c>VariableId</c> y no por nombre, que no muta su propio modelo al pedir un cambio, y que las dos
    /// acciones irreversibles no salen sin confirmación explícita.
    /// </para>
    /// <para>
    /// Y el estado bloqueado: sobre un registro que esta versión no puede leer no se ofrece crear nada.
    /// Escribir encima destruiría en silencio todo lo que ese registro tuviera.
    /// </para>
    /// </summary>
    public sealed class ProjectVariablesWindowTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Otra = "11111111-2222-3333-4444-555555555555";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance, PalletDepth = 48.0 };
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

        private static SelectivePalletDesignDocument Doc(double literal, string variableId)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(literal), RackA, "Rack A");
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [Token] = SelectivePropertyValueDocument.ToProjectVariable(variableId),
            };
            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            return doc;
        }

        private static ProjectVariablesDocument Registro(double value = 10.0, string id = VarId)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                new ProjectVariableDocument
                {
                    VariableId = id,
                    Name = "Holgura",
                    Type = VariableType.Length.ToString(),
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
                },
            };

            return document;
        }

        private static ProjectVariablesWorkspace Workspace(
            ProjectVariablesReadResult registry, params ProjectVariableScanEntry[] entries)
            => ProjectVariablesWorkspace.Build(registry, entries);

        private static ProjectVariablesWorkspace Vacio()
            => Workspace(ProjectVariablesReadResult.Absent());

        private static ProjectVariablesWorkspace ConUnaVariable(params ProjectVariableScanEntry[] entries)
            => Workspace(ProjectVariablesReadResult.Readable(Registro()), entries);

        private static ProjectVariablesWorkspace ConUnaRota()
            => Workspace(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                ProjectVariableScanEntry.Selective("D1", RackA, Doc(6.0, VarId), 1));

        /// <summary>
        /// Un rack con dos vinculos no resolubles, uno de ellos sobre una propiedad que esta version no conoce.
        /// El rack entero queda BLOQUEADO: ninguna fila es accionable, ni la que por si sola seria reparable.
        /// </summary>
        private static ProjectVariablesWorkspace ConUnRackBloqueado()
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackA, "Rack A");
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [Token] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
                ["selective.noExiste"] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
            };
            doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;

            return Workspace(
                ProjectVariablesReadResult.Readable(Registro(10.0, id: Otra)),
                ProjectVariableScanEntry.Selective("D1", RackA, doc, 1));
        }

        private static T Control<T>(RackProjectVariablesWindow window, string name)
            where T : System.Windows.FrameworkElement
            => EditorWindowTestSupport.Find<T>(window, element => element.Name == name);

        // ================================================================ 1, 2: lo que presenta

        [Fact]
        public void UN_REGISTRO_VACIO_NO_MUESTRA_VARIABLES()
        {
            var count = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(Vacio());
                return Control<ListBox>(window, "VariablesList").Items.Count;
            });

            Assert.Equal(0, count);
        }

        [Fact]
        public void LAS_VARIABLES_SE_MUESTRAN_CON_NOMBRE_TIPO_Y_VALOR()
        {
            var (count, name, type, value) = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(ConUnaVariable());
                var list = Control<ListBox>(window, "VariablesList");
                list.SelectedIndex = 0;

                var row = (ProjectVariableRow)list.SelectedItem;
                return (list.Items.Count, row.Name, row.Type, row.LiteralValue);
            });

            Assert.Equal(1, count);
            Assert.Equal("Holgura", name);
            Assert.Equal(VariableType.Length, type);
            Assert.Equal(10.0, value);
        }

        // ================================================================ 3, 4, 5: la identidad manda

        [Fact]
        public void LA_SELECCION_VIAJA_POR_VariableId()
        {
            var intent = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(ConUnaVariable());
                Control<ListBox>(window, "VariablesList").SelectedIndex = 0;
                EditorWindowTestSupport.ClickNamed(window, "DeleteButton");
                return window.Intent;
            });

            Assert.NotNull(intent);
            Assert.Equal(ProjectVariableIntentKind.Delete, intent.Kind);
            Assert.Equal(VariableId.Parse(VarId), intent.VariableId);
        }

        [Fact]
        public void CREAR_LLEVA_NOMBRE_Y_VALOR_Y_NINGUNA_IDENTIDAD_INVENTADA()
        {
            var intent = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(Vacio());
                Control<TextBox>(window, "NameBox").Text = "Holgura nueva";
                Control<TextBox>(window, "ValueBox").Text = "14";
                EditorWindowTestSupport.ClickNamed(window, "NewButton");
                return window.Intent;
            });

            Assert.NotNull(intent);
            Assert.Equal(ProjectVariableIntentKind.Create, intent.Kind);
            Assert.Equal("Holgura nueva", intent.Name);
            Assert.Equal(14.0, intent.Value);
            Assert.Equal(default, intent.VariableId);
        }

        /// <summary>Renombrar mantiene el id EXACTO: el nombre nunca es autoridad.</summary>
        [Fact]
        public void RENOMBRAR_CONSERVA_LA_IDENTIDAD()
        {
            var intent = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(ConUnaVariable());
                Control<ListBox>(window, "VariablesList").SelectedIndex = 0;
                Control<TextBox>(window, "NameBox").Text = "Holgura General";
                EditorWindowTestSupport.ClickNamed(window, "RenameButton");
                return window.Intent;
            });

            Assert.Equal(ProjectVariableIntentKind.Rename, intent.Kind);
            Assert.Equal(VariableId.Parse(VarId), intent.VariableId);
            Assert.Equal("Holgura General", intent.Name);
        }

        // ================================================================ 6: pedir no es cambiar

        /// <summary>
        /// La ventana no es la autoridad: pedir un cambio produce un intent, y el modelo que tiene delante
        /// sigue diciendo lo que el dibujo dice. Quien manda es el DWG, y se relee después.
        /// </summary>
        [Fact]
        public void PEDIR_UN_CAMBIO_DE_VALOR_NO_MUTA_EL_MODELO_LOCAL()
        {
            var (intentValue, rowValue) = StaTestRunner.Run(() =>
            {
                var workspace = ConUnaVariable();
                var window = new RackProjectVariablesWindow(workspace);
                Control<ListBox>(window, "VariablesList").SelectedIndex = 0;
                Control<TextBox>(window, "ValueBox").Text = "12";
                EditorWindowTestSupport.ClickNamed(window, "ChangeValueButton");

                return (window.Intent.Value, workspace.Variables[0].LiteralValue);
            });

            Assert.Equal(12.0, intentValue);
            Assert.Equal(10.0, rowValue);
        }

        // ================================================================ 7, 8: borrar y desvincular

        [Fact]
        public void BORRAR_CON_CONSUMIDORES_ENSENA_LAS_DEPENDENCIAS()
        {
            var text = StaTestRunner.Run(() =>
            {
                var workspace = ConUnaVariable(
                    ProjectVariableScanEntry.Selective("D1", RackA, Doc(6.0, VarId), 1));
                var window = new RackProjectVariablesWindow(workspace);
                Control<ListBox>(window, "VariablesList").SelectedIndex = 0;

                return Control<TextBlock>(window, "ConsumersText").Text;
            });

            Assert.Contains("Rack A", text);
            Assert.Contains(Token, text);
        }

        [Fact]
        public void DESVINCULAR_TODOS_Y_ELIMINAR_EXIGE_CONFIRMACION_EXPLICITA()
        {
            var (enabledSin, intentSin, enabledCon, intentCon) = StaTestRunner.Run(() =>
            {
                var workspace = ConUnaVariable(
                    ProjectVariableScanEntry.Selective("D1", RackA, Doc(6.0, VarId), 1));
                var window = new RackProjectVariablesWindow(workspace);
                Control<ListBox>(window, "VariablesList").SelectedIndex = 0;

                var button = Control<Button>(window, "UnlinkAllDeleteButton");
                EditorWindowTestSupport.ClickNamed(window, "UnlinkAllDeleteButton");
                var sin = (button.IsEnabled, window.Intent);

                Control<CheckBox>(window, "UnlinkConfirmCheck").IsChecked = true;
                EditorWindowTestSupport.ClickNamed(window, "UnlinkAllDeleteButton");

                return (sin.IsEnabled, sin.Intent, button.IsEnabled, window.Intent);
            });

            Assert.False(enabledSin);
            Assert.Null(intentSin);
            Assert.True(enabledCon);
            Assert.Equal(ProjectVariableIntentKind.UnlinkAllAndDelete, intentCon.Kind);
            Assert.True(intentCon.Confirmed);
        }

        // ================================================================ 9, 10: reparar

        [Fact]
        public void UNA_REPARACION_AVISA_DE_QUE_SE_USARA_EL_LITERAL_ALMACENADO()
        {
            var (count, warning, literal) = StaTestRunner.Run(() =>
            {
                var workspace = ConUnaRota();
                var window = new RackProjectVariablesWindow(workspace);
                var list = Control<ListBox>(window, "BrokenList");
                list.SelectedIndex = 0;

                return (list.Items.Count,
                    Control<TextBlock>(window, "RepairWarningText").Text,
                    ((BrokenBindingRow)list.SelectedItem).StoredLiteral);
            });

            Assert.Equal(1, count);
            Assert.Equal(6.0, literal);
            Assert.Contains("literal almacenado", warning);
            Assert.Contains("geometría puede cambiar", warning);
        }

        [Fact]
        public void REPARAR_NO_SE_EJECUTA_SIN_CONFIRMAR()
        {
            var (enabledSin, intentSin, intentCon) = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(ConUnaRota());
                Control<ListBox>(window, "BrokenList").SelectedIndex = 0;

                var button = Control<Button>(window, "RepairButton");
                EditorWindowTestSupport.ClickNamed(window, "RepairButton");
                var sin = (button.IsEnabled, window.Intent);

                Control<CheckBox>(window, "RepairConfirmCheck").IsChecked = true;
                EditorWindowTestSupport.ClickNamed(window, "RepairButton");

                return (sin.IsEnabled, sin.Intent, window.Intent);
            });

            Assert.False(enabledSin);
            Assert.Null(intentSin);
            Assert.Equal(ProjectVariableIntentKind.RepairBroken, intentCon.Kind);
            Assert.Equal(RackA, intentCon.RackId);
            Assert.True(intentCon.Confirmed);

            // I-48 G4B: reparar es del RACK. El intent ya no lleva alcance de propiedad, porque una reparacion
            // parcial no se puede aplicar: el ejecutor necesita un diseno efectivo COMPLETO.
            Assert.Null(intentCon.PropertyId);
        }

        // ---------------------------------------------------------------- I-48 G4B: el rack bloqueado

        [Fact]
        public void UN_RACK_BLOQUEADO_MUESTRA_SUS_FILAS_PERO_NO_DEJA_REPARAR()
        {
            var (count, enabled, warning, intent) = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(ConUnRackBloqueado());
                var list = Control<ListBox>(window, "BrokenList");
                list.SelectedIndex = 0;

                // Incluso confirmando: el boton sigue apagado y el clic no produce intent.
                Control<CheckBox>(window, "RepairConfirmCheck").IsChecked = true;
                EditorWindowTestSupport.ClickNamed(window, "RepairButton");

                return (list.Items.Count,
                    Control<Button>(window, "RepairButton").IsEnabled,
                    Control<TextBlock>(window, "RepairWarningText").Text,
                    window.Intent);
            });

            // Diagnostico SI: las dos filas se ven.
            Assert.Equal(2, count);

            // Accionable NO.
            Assert.False(enabled);
            Assert.Null(intent);
        }

        [Fact]
        public void UN_RACK_BLOQUEADO_NO_PROMETE_EL_LITERAL_ALMACENADO()
        {
            // Prometerlo seria prometer un cambio que no se puede aplicar en absoluto mientras el rack lleve un
            // estado fatal. La ventana no deduce eso: lo lee del veredicto que trae la fila.
            var warning = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(ConUnRackBloqueado());
                Control<ListBox>(window, "BrokenList").SelectedIndex = 0;
                return Control<TextBlock>(window, "RepairWarningText").Text;
            });

            Assert.Contains("no se puede reparar", warning);
            Assert.Contains("diagnóstico", warning);
            Assert.DoesNotContain("literal almacenado", warning);
        }

        // ================================================================ 11, 12: el registro que no se lee

        [Fact]
        public void UN_REGISTRO_ILEGIBLE_DEJA_LA_VENTANA_BLOQUEADA()
        {
            var (editable, banner, newEnabled) = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(
                    Workspace(ProjectVariablesReadResult.Unreadable("registro corrupto")));

                return (Control<System.Windows.Controls.Panel>(window, "EditorPanel").IsEnabled,
                    Control<TextBlock>(window, "BlockedText").Text,
                    Control<Button>(window, "NewButton").IsEnabled);
            });

            Assert.False(editable);
            Assert.Contains("registro corrupto", banner);
            Assert.False(newEnabled);
        }

        [Fact]
        public void UN_MAJOR_INCOMPATIBLE_DEJA_LA_VENTANA_BLOQUEADA()
        {
            var editable = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(
                    Workspace(ProjectVariablesReadResult.IncompatibleMajor("major 9")));

                return Control<System.Windows.Controls.Panel>(window, "EditorPanel").IsEnabled;
            });

            Assert.False(editable);
        }

        /// <summary>Sobre un registro corrupto tampoco se ofrece «crea tu primera variable».</summary>
        [Fact]
        public void UN_REGISTRO_ILEGIBLE_NO_PRODUCE_NINGUN_INTENT()
        {
            var intent = StaTestRunner.Run(() =>
            {
                var window = new RackProjectVariablesWindow(
                    Workspace(ProjectVariablesReadResult.Unreadable("registro corrupto")));

                Control<TextBox>(window, "NameBox").Text = "Holgura";
                Control<TextBox>(window, "ValueBox").Text = "10";
                EditorWindowTestSupport.ClickNamed(window, "NewButton");
                return window.Intent;
            });

            Assert.Null(intent);
        }
    }
}
