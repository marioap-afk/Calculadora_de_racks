using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.UI.Controls;
using RackCad.UI.Shell;
using Xunit;
using static RackCad.UI.Tests.CustomPropertiesWindowTestKit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-54 G7 — la ventana REAL de RACKPROPIEDADES (Proposal V5 D-18.3..D-18.6; §12.7 U-01..U-10; ADR-0029 D6, D7 y D9).
    ///
    /// <para>
    /// La ventana no es autoridad: recibe un workspace puro y, para un rack escribible, lo que el comando capturo de cada vista,
    /// y devuelve UNA operacion por id o UNA unificacion confirmada. Lo que se comprueba aqui es exactamente eso: que presenta lo
    /// que recibe, que los estados de solo lectura abren bloqueados y con su motivo, que no ofrece ningun descarte, que la
    /// unificacion solo aparece cuando Application la ofrece y exige origen y casilla, y que cerrar nunca devuelve nada.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesWindowTests
    {
        private static readonly string[] CrudButtons = { "NewButton", "RenameButton", "ChangeValueButton", "DeleteButton" };

        /// <summary>Cada estado que la ventana puede recibir, con su nombre de caso.</summary>
        public static TheoryData<string> AllStates()
        {
            var data = new TheoryData<string> { "rack-single", "rack-repeated", "project-readable", "project-absent", "rack-divergent", "rack-divergent-sin-unificar" };

            foreach (var item in ReadOnlyCases())
            {
                data.Add(item.Name);
            }

            return data;
        }

        public static TheoryData<string> ReadOnlyStates()
        {
            var data = new TheoryData<string>();

            foreach (var item in ReadOnlyCases())
            {
                data.Add(item.Name);
            }

            return data;
        }

        /// <summary>La ventana de un caso, con lo mostrado cuando el rack es escribible, como la abre el comando.</summary>
        private static RackCustomPropertiesWindow OpenState(string state)
        {
            switch (state)
            {
                case "rack-single":
                    return Open(Rack(SingleRack()), Displayed(SingleRack()));
                case "rack-repeated":
                    var repeated = SingleRack(Props(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "CLIENTE", "Otro"), Entry(IdC, "Área", "Norte")));
                    return Open(Rack(repeated), Displayed(repeated));
                case "project-readable":
                    return Open(Project(Base));
                case "project-absent":
                    return Open(ProjectAbsent());
                case "rack-divergent":
                    return Open(Rack(DivergentRack()), Displayed(DivergentRack()));
                case "rack-divergent-sin-unificar":
                    return Open(Rack(DivergentRackWithoutUnify()), Displayed(DivergentRackWithoutUnify()));
                default:
                    return Open(ReadOnly(state).Build());
            }
        }

        // ================================================================ U-01 filas Nombre — Valor

        [Fact]
        public void U01_UnRackEditableListaNombreValorEnOrden_YCadaFilaGuardaSuId()
        {
            StaTestRunner.Run(() =>
            {
                var workspace = Rack(SingleRack());
                Assert.Equal(CustomPropertiesWorkspaceState.Editable, workspace.State);

                var window = Open(workspace, Displayed(SingleRack()));
                var items = List(window).Items.Cast<RackCustomPropertiesWindow.PropertyItem>().ToList();

                Assert.Equal(new[] { "Cliente — ACME", "Área — Norte" }, items.Select(item => item.Text));
                Assert.Equal(workspace.Rows.Select(row => row.Id), items.Select(item => item.Id));
                Assert.Equal(new[] { IdA, IdB }, items.Select(item => item.Id.ToString()), StringComparer.OrdinalIgnoreCase);
                Assert.Contains("Rack «Rack A»", Named<TextBlock>(window, "ScopeText").Text);
                Assert.Contains("Propiedades personalizadas", window.Title);
                Assert.Contains("Rack «Rack A»", window.Title);
            });
        }

        [Fact]
        public void U01_ElProyectoUsaLaMismaVentana_ConLaEtiquetaProyecto()
        {
            StaTestRunner.Run(() =>
            {
                var window = Open(Project(Base));
                var items = List(window).Items.Cast<RackCustomPropertiesWindow.PropertyItem>().ToList();

                Assert.Equal(new[] { "Cliente — ACME", "Área — Norte" }, items.Select(item => item.Text));
                Assert.Equal("Proyecto", Named<TextBlock>(window, "ScopeText").Text);
                Assert.Contains("Proyecto", window.Title);
                Assert.True(Button(window, "NewButton").IsEnabled);
            });
        }

        [Fact]
        public void U01_ElProyectoAusenteEsEditableYVacio()
        {
            StaTestRunner.Run(() =>
            {
                var window = Open(ProjectAbsent());

                Assert.Empty(List(window).Items);
                Assert.True(Button(window, "NewButton").IsEnabled);
                Assert.True(Named<TextBox>(window, "NameBox").IsEnabled);
                Assert.True(Named<TextBox>(window, "ValueBox").IsEnabled);
                Assert.Equal(string.Empty, PresenterText(window, "StateBanner"));
            });
        }

        [Fact]
        public void U01_ElIdNoEsElTextoVisibleDeLaFila()
        {
            StaTestRunner.Run(() =>
            {
                var window = Open(Rack(SingleRack()), Displayed(SingleRack()));

                foreach (var item in List(window).Items.Cast<RackCustomPropertiesWindow.PropertyItem>())
                {
                    Assert.DoesNotContain(item.Id.ToString(), item.Text, StringComparison.OrdinalIgnoreCase);
                    Assert.DoesNotContain(item.Id.ToString(), item.ToString(), StringComparison.OrdinalIgnoreCase);
                }

                Assert.DoesNotContain(IdA, AllText(window), StringComparison.OrdinalIgnoreCase);
            });
        }

        // ================================================================ U-02 solo lectura desde la apertura

        /// <summary>La frase que el motivo tiene que mostrar, por motivo del workspace.</summary>
        private static string ReasonPhrase(CustomPropertiesReadOnlyReason reason)
        {
            switch (reason)
            {
                case CustomPropertiesReadOnlyReason.XrefRejected: return "referencia externa";
                case CustomPropertiesReadOnlyReason.NoIdentity: return "no tiene identidad";
                case CustomPropertiesReadOnlyReason.IndeterminateMembership: return "no puede interpretar";
                case CustomPropertiesReadOnlyReason.MixedKind: return "mismo tipo de sistema";
                case CustomPropertiesReadOnlyReason.UnknownKind: return "no es uno que esta versión";
                case CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly: return "alguna vista";
                case CustomPropertiesReadOnlyReason.PresentButUnreadable: return "no se pueden leer";
                case CustomPropertiesReadOnlyReason.AmbiguousIdentity: return "identificadores repetidos";
                case CustomPropertiesReadOnlyReason.IncompatibleMajor: return "versión de formato";
                default: return "profundidad máxima";
            }
        }

        /// <summary>La frase del estado de una vista que bloquea un rack.</summary>
        private static string ViewStatePhrase(CustomPropertiesReadOutcome state)
        {
            switch (state)
            {
                case CustomPropertiesReadOutcome.PresentButUnreadable: return "no se pueden leer";
                case CustomPropertiesReadOutcome.AmbiguousIdentity: return "identificadores repetidos";
                case CustomPropertiesReadOutcome.IncompatibleMajor: return "versión de formato";
                default: return "profundidad máxima";
            }
        }

        [Theory]
        [MemberData(nameof(ReadOnlyStates))]
        public void U02_CadaEstadoDeSoloLecturaAbreBloqueado_ConSuMotivoVisibleDesdeLaApertura(string state)
        {
            StaTestRunner.Run(() =>
            {
                var item = ReadOnly(state);
                var workspace = item.Build();

                // El caso es el que dice ser: el motivo lo decide Application, no la prueba.
                Assert.Equal(CustomPropertiesWorkspaceState.ReadOnly, workspace.State);
                Assert.Equal(item.Reason, workspace.ReadOnlyReason);

                var window = Open(workspace);
                var banner = PresenterText(window, "StateBanner");

                Assert.StartsWith("Solo lectura.", banner, StringComparison.Ordinal);
                Assert.Contains(ReasonPhrase(item.Reason), banner, StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrWhiteSpace(workspace.Diagnostic))
                {
                    Assert.Contains(workspace.Diagnostic, banner, StringComparison.Ordinal);
                }

                if (item.ViewState.HasValue)
                {
                    Assert.Contains(ViewStatePhrase(item.ViewState.Value), AllText(window), StringComparison.OrdinalIgnoreCase);
                }

                // TODAS las escrituras, eliminar incluido, bloqueadas y con su motivo legible aun apagadas.
                foreach (var name in CrudButtons)
                {
                    var button = Button(window, name);
                    Assert.False(button.IsEnabled, name + " habilitado en " + state);
                    Assert.False(string.IsNullOrWhiteSpace(button.ToolTip as string), name + " sin motivo en " + state);
                    Assert.Contains("Solo lectura", (string)button.ToolTip, StringComparison.Ordinal);
                    Assert.True(ToolTipService.GetShowOnDisabled(button));
                }

                Assert.False(Named<TextBox>(window, "NameBox").IsEnabled);
                Assert.False(Named<TextBox>(window, "ValueBox").IsEnabled);
                Assert.Empty(List(window).Items);
                Assert.Null(window.FindName("UnifyButton"));

                // Pulsar no intenta nada: no hay una escritura que se descubra bloqueada despues.
                Named<TextBox>(window, "NameBox").Text = "Cliente";
                Named<TextBox>(window, "ValueBox").Text = "ACME";

                foreach (var name in CrudButtons)
                {
                    EditorWindowTestSupport.ClickNamed(window, name);
                }

                Assert.Null(window.Intent);
                Assert.Null(window.UnifyIntent);
            });
        }

        [Fact]
        public void U02_LaPertenenciaIndeterminadaNombraLasDefinicionesQueNoSeInterpretan()
        {
            StaTestRunner.Run(() =>
            {
                var window = Open(ReadOnly("rack-indeterminate").Build());

                Assert.Contains("RACK_9", AllText(window), StringComparison.Ordinal);
                Assert.Equal(Visibility.Visible, Named<FrameworkElement>(window, "ViewsPanel").Visibility);
            });
        }

        // ================================================================ U-03 sin descarte destructivo

        private static readonly string[] DiscardWords =
        {
            "descart", "restablec", "restaur", "reset", "reinici", "ignor", "sobrescrib", "reemplaz", "forz", "limpi", "vaciar",
            "borrar todo", "repar", "discard", "overwrite", "force", "replace", "purg",
        };

        [Theory]
        [MemberData(nameof(AllStates))]
        public void U03_NingunEstadoOfreceUnControlDeDescarte(string state)
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState(state);

                var actionable = EditorWindowTestSupport.Descendants(window)
                    .OfType<ButtonBase>()
                    .Select(control => (control.Name, Text: control.Content is TextBlock block ? block.Text : control.Content as string ?? string.Empty))
                    .ToList();

                foreach (var (name, text) in actionable)
                {
                    foreach (var word in DiscardWords)
                    {
                        Assert.DoesNotContain(word, text, StringComparison.OrdinalIgnoreCase);
                        Assert.DoesNotContain(word, name ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                }

                // Lista CERRADA de acciones: las cuatro por id, Unificar solo si Application la ofrece, y Cerrar.
                var expected = new List<string> { "Cambiar valor", "Cerrar", "Eliminar", "Nueva", "Renombrar" };

                if (state == "rack-divergent")
                {
                    expected.Add("Unificar");
                }

                Assert.Equal(expected.OrderBy(label => label, StringComparer.Ordinal), ButtonLabels(window));
                Assert.Empty(EditorWindowTestSupport.FindAll<MenuItem>(window));
            });
        }

        // ================================================================ U-04 unificar seguro

        [Fact]
        public void U04_ElPanelDeUnificarSoloApareceCuandoApplicationLoOfrece()
        {
            StaTestRunner.Run(() =>
            {
                var divergent = OpenState("rack-divergent");
                Assert.True(Rack(DivergentRack()).UnifyAvailable);
                Assert.Equal(Visibility.Visible, Named<FrameworkElement>(divergent, "UnifyPanel").Visibility);
                Assert.NotNull(divergent.FindName("UnifyButton"));

                foreach (var state in new[] { "rack-divergent-sin-unificar", "rack-single", "project-readable", "rack-depth", "project-major" })
                {
                    var window = OpenState(state);

                    Assert.Equal(Visibility.Collapsed, Named<FrameworkElement>(window, "UnifyPanel").Visibility);
                    Assert.Null(window.FindName("UnifyButton"));
                    Assert.Empty(UnifySources(window));
                }
            });
        }

        [Fact]
        public void U04_MuestraElContenidoDeCadaVista_YUnaVistaAusenteComoVacioSinPropiedades()
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState("rack-divergent");
                var text = AllText(window);

                foreach (var expected in new[]
                         {
                             "frontal", "RACK_1", "Cliente — ACME", "Área — Norte",
                             "lateral", "sección 4", "RACK_2", "Cliente — OTRO",
                             "planta", "RACK_3", "vacío / sin propiedades",
                         })
                {
                    Assert.Contains(expected, text, StringComparison.Ordinal);
                }

                Assert.Equal("vacío / sin propiedades", RackCustomPropertiesWindow.EmptyCollectionText);
            });
        }

        [Fact]
        public void U04_LosOrigenesSonExactamenteLosQueApplicationMarca_YNingunoVieneElegido()
        {
            StaTestRunner.Run(() =>
            {
                var workspace = Rack(DivergentRack());
                var window = Open(workspace, Displayed(DivergentRack()));
                var sources = UnifySources(window);

                var offered = workspace.ViewSummaries.Where(summary => summary.IsUnifySourceAvailable).Select(summary => summary.Handle).ToList();

                Assert.NotEmpty(offered);
                Assert.Equal(offered, sources.Select(radio => (string)radio.Tag));
                Assert.All(sources, radio => Assert.False(radio.IsChecked == true));

                var unify = Button(window, "UnifyButton");
                Assert.False(unify.IsEnabled);
                Assert.False(string.IsNullOrWhiteSpace(unify.ToolTip as string));
                Assert.False(Named<CheckBox>(window, "UnifyConfirmCheck").IsChecked == true);
            });
        }

        [Fact]
        public void U04_UnaVistaAusenteElegibleSeOfreceComoVacioSinPropiedades()
        {
            StaTestRunner.Run(() =>
            {
                var workspace = Rack(DivergentRack());
                var planta = Assert.Single(workspace.ViewSummaries, summary => summary.Handle == "3");

                Assert.Equal(CustomPropertiesReadOutcome.Absent, planta.State);
                Assert.True(planta.IsUnifySourceAvailable, "el caso necesita un origen ausente disponible");

                var window = Open(workspace, Displayed(DivergentRack()));
                var radio = Assert.Single(UnifySources(window), source => (string)source.Tag == "3");

                Assert.Contains("vacío / sin propiedades", RadioText(radio), StringComparison.Ordinal);
                Assert.Contains("planta", RadioText(radio), StringComparison.Ordinal);
            });
        }

        [Fact]
        public void U04_UnificarExigeOrigenYCasilla_YDevuelveElIntentConLoMostrado()
        {
            StaTestRunner.Run(() =>
            {
                var authority = DivergentRack();
                var displayed = Displayed(authority);
                var window = Open(Rack(authority), displayed);

                // Sin origen ni casilla: nada.
                EditorWindowTestSupport.ClickNamed(window, "UnifyButton");
                Assert.Null(window.UnifyIntent);

                // Con origen y sin casilla: sigue bloqueado y con motivo.
                Assert.Single(UnifySources(window), source => (string)source.Tag == "3").IsChecked = true;
                Assert.False(Button(window, "UnifyButton").IsEnabled);
                Assert.Contains("casilla", (string)Button(window, "UnifyButton").ToolTip, StringComparison.OrdinalIgnoreCase);
                EditorWindowTestSupport.ClickNamed(window, "UnifyButton");
                Assert.Null(window.UnifyIntent);

                // Con origen y casilla: una unificacion confirmada desde ESA vista, con lo que el comando capturo.
                Named<CheckBox>(window, "UnifyConfirmCheck").IsChecked = true;
                Assert.True(Button(window, "UnifyButton").IsEnabled);
                EditorWindowTestSupport.ClickNamed(window, "UnifyButton");

                Assert.NotNull(window.UnifyIntent);
                Assert.Equal("3", window.UnifyIntent.SourceHandle);
                Assert.True(window.UnifyIntent.Confirmed);
                Assert.Same(displayed, window.UnifyIntent.Displayed);
                Assert.Null(window.Intent);
            });
        }

        [Fact]
        public void U04_CambiarDeOrigenRetiraLaConfirmacion()
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState("rack-divergent");
                var sources = UnifySources(window);
                Assert.True(sources.Count >= 2, "el caso necesita dos origenes");

                sources[0].IsChecked = true;
                Named<CheckBox>(window, "UnifyConfirmCheck").IsChecked = true;
                Assert.True(Button(window, "UnifyButton").IsEnabled);

                sources[1].IsChecked = true;

                Assert.False(Named<CheckBox>(window, "UnifyConfirmCheck").IsChecked == true);
                Assert.False(Button(window, "UnifyButton").IsEnabled);
            });
        }

        [Fact]
        public void U04_UnaVentanaConUnificacionNoSeAbreSinLoMostrado()
        {
            StaTestRunner.Run(() =>
            {
                Assert.Throws<ArgumentException>(() => new RackCustomPropertiesWindow(Rack(DivergentRack())));
                Assert.Throws<ArgumentNullException>(() => new RackCustomPropertiesWindow(null));
            });
        }

        // ================================================================ U-05 intents por id

        [Fact]
        public void U05_NuevaLlevaNombreYValor_YNingunaIdentidadInventada()
        {
            StaTestRunner.Run(() =>
            {
                var window = Open(ProjectAbsent());
                Named<TextBox>(window, "NameBox").Text = "Cliente";
                Named<TextBox>(window, "ValueBox").Text = "ACME";

                EditorWindowTestSupport.ClickNamed(window, "NewButton");

                Assert.NotNull(window.Intent);
                Assert.Equal(CustomPropertiesIntentKind.Create, window.Intent.Kind);
                Assert.True(window.Intent.Id.IsEmpty);
                Assert.Equal("Cliente", window.Intent.Name);
                Assert.Equal("ACME", window.Intent.Value);
                Assert.Null(window.UnifyIntent);
            });
        }

        [Fact]
        public void U05_RenombrarCambiarValorYEliminarViajanPorElIdDeLaFilaElegida()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (button, kind) in new[]
                         {
                             ("RenameButton", CustomPropertiesIntentKind.Rename),
                             ("ChangeValueButton", CustomPropertiesIntentKind.ChangeValue),
                             ("DeleteButton", CustomPropertiesIntentKind.Delete),
                         })
                {
                    var workspace = Rack(SingleRack());
                    var window = Open(workspace, Displayed(SingleRack()));

                    List(window).SelectedIndex = 1;
                    Named<TextBox>(window, "NameBox").Text = "Zona";
                    Named<TextBox>(window, "ValueBox").Text = "Sur";

                    EditorWindowTestSupport.ClickNamed(window, button);

                    Assert.NotNull(window.Intent);
                    Assert.Equal(kind, window.Intent.Kind);
                    Assert.Equal(workspace.Rows[1].Id, window.Intent.Id);

                    switch (kind)
                    {
                        case CustomPropertiesIntentKind.Rename:
                            Assert.Equal("Zona", window.Intent.Name);
                            Assert.Null(window.Intent.Value);
                            break;
                        case CustomPropertiesIntentKind.ChangeValue:
                            Assert.Equal("Sur", window.Intent.Value);
                            Assert.Null(window.Intent.Name);
                            break;
                        default:
                            Assert.Null(window.Intent.Name);
                            Assert.Null(window.Intent.Value);
                            break;
                    }
                }
            });
        }

        [Fact]
        public void U05_ConNombresRepetidosElIdDistingueLasFilas_NuncaElNombre()
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState("rack-repeated");

                List(window).SelectedIndex = 1;
                Assert.Equal("CLIENTE", Named<TextBox>(window, "NameBox").Text);

                EditorWindowTestSupport.ClickNamed(window, "DeleteButton");

                Assert.Equal(CustomPropertiesIntentKind.Delete, window.Intent.Kind);
                Assert.Equal(IdB, window.Intent.Id.ToString(), StringComparer.OrdinalIgnoreCase);
            });
        }

        [Fact]
        public void U05_ElegirUnaFilaCargaSuNombreYSuValor_YSinFilaLasAccionesPorIdQuedanBloqueadasConMotivo()
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState("rack-single");

                foreach (var name in new[] { "RenameButton", "ChangeValueButton", "DeleteButton" })
                {
                    Assert.False(Button(window, name).IsEnabled);
                    Assert.Contains("Elige una propiedad", (string)Button(window, name).ToolTip, StringComparison.Ordinal);
                    EditorWindowTestSupport.ClickNamed(window, name);
                }

                Assert.Null(window.Intent);
                Assert.True(Button(window, "NewButton").IsEnabled);

                List(window).SelectedIndex = 0;

                Assert.Equal("Cliente", Named<TextBox>(window, "NameBox").Text);
                Assert.Equal("ACME", Named<TextBox>(window, "ValueBox").Text);

                foreach (var name in new[] { "RenameButton", "ChangeValueButton", "DeleteButton" })
                {
                    Assert.True(Button(window, name).IsEnabled);
                    Assert.Null(Button(window, name).ToolTip);
                }
            });
        }

        [Fact]
        public void U05_PedirUnCambioNoMutaElWorkspaceNiLasFilasDeLaVentana()
        {
            StaTestRunner.Run(() =>
            {
                var workspace = Rack(SingleRack());
                var before = workspace.Rows.Select(row => row.Id + "|" + row.Name + "|" + row.Value + "|" + row.NombreRepetido).ToList();
                var window = Open(workspace, Displayed(SingleRack()));

                List(window).SelectedIndex = 0;
                Named<TextBox>(window, "NameBox").Text = "Otro nombre";
                EditorWindowTestSupport.ClickNamed(window, "RenameButton");

                Assert.Equal(CustomPropertiesIntentKind.Rename, window.Intent.Kind);
                Assert.Equal(before, workspace.Rows.Select(row => row.Id + "|" + row.Name + "|" + row.Value + "|" + row.NombreRepetido));
                Assert.Equal(
                    new[] { "Cliente — ACME", "Área — Norte" },
                    List(window).Items.Cast<RackCustomPropertiesWindow.PropertyItem>().Select(item => item.Text));
            });
        }

        // ================================================================ U-06 la UI no es autoridad

        /// <summary>Lo que la ventana no puede ver ni nombrar (orden de G7): el dibujo, el store, la autoridad, el commit y el borde.</summary>
        private static readonly string[] ForbiddenInWindow =
        {
            "Database", "ObjectId", "Transaction", "RackEmbedDocument", "RackEmbedStore", "CustomPropertiesStore",
            "RackCustomPropertiesAuthority", "RackCustomPropertiesAuthorityResult", "CustomPropertiesCommit", "CustomPropertiesExecutor",
            "CustomPropertiesMutations", "CustomPropertiesPreflight", "CustomPropertiesCanonicalForm", "CustomPropertiesWriteGuard",
            "CustomPropertiesDocument", "CustomPropertyEntryDocument", "CustomPropertiesData", "Capture", "Autodesk",
        };

        /// <summary>Vacia si ni el code-behind ni el XAML, sin comentarios, nombran lo prohibido ni un MessageBox.</summary>
        internal static IReadOnlyList<string> WindowViolations(string code, string xaml)
        {
            var visible = CodeWithoutComments(code) + "\n" + XamlWithoutComments(xaml);
            var violations = ForbiddenInWindow
                .Where(forbidden => Regex.IsMatch(visible, @"\b" + forbidden + @"\b"))
                .Select(forbidden => "la ventana nombra " + forbidden)
                .ToList();

            if (Regex.IsMatch(visible, @"\bMessageBox\b|\bEditorDiscardPrompt\b"))
            {
                violations.Add("la ventana abre un MessageBox");
            }

            return violations;
        }

        [Fact]
        public void U06_LaVentanaSoloPresentaElWorkspace_SinStoreAutoridadCommitBordeNiAutoCAD()
        {
            Assert.Empty(WindowViolations(WindowCodeText(), WindowXamlText()));
        }

        public static TheoryData<string> WindowMutations()
        {
            var data = new TheoryData<string>();

            foreach (var forbidden in ForbiddenInWindow)
            {
                data.Add(forbidden);
            }

            data.Add("MessageBox");
            return data;
        }

        [Theory]
        [MemberData(nameof(WindowMutations))]
        public void U06_LaGuardaDetectaUnaVentanaQueSeHaceAutoridad(string forbidden)
        {
            var code = WindowCodeText().Replace("private void Ask(", "private object Leak() => " + forbidden + ".Value;\n\n        private void Ask(");

            Assert.Contains("private void Ask(", WindowCodeText());
            Assert.NotEmpty(WindowViolations(code, WindowXamlText()));
        }

        [Fact]
        public void U06_NingunMiembroDeLaVentanaExponeTiposDelDibujoNiDeLaAutoridad()
        {
            var type = typeof(RackCustomPropertiesWindow);
            const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var types = type.GetFields(all).Select(field => field.FieldType)
                .Concat(type.GetProperties(all).Select(property => property.PropertyType))
                .Concat(type.GetConstructors(all).SelectMany(constructor => constructor.GetParameters()).Select(parameter => parameter.ParameterType))
                .Concat(type.GetMethods(all).SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)))
                .Distinct()
                .ToList();

            Assert.NotEmpty(types);

            foreach (var used in types)
            {
                Assert.False((used.Namespace ?? string.Empty).StartsWith("Autodesk", StringComparison.Ordinal), used.FullName);
                Assert.DoesNotContain(used.Name, ForbiddenInWindow);
            }

            Assert.DoesNotContain(
                type.Assembly.GetReferencedAssemblies(),
                reference => new[] { "AcCoreMgd", "AcDbMgd", "AcMgd" }.Contains(reference.Name, StringComparer.OrdinalIgnoreCase)
                             || reference.Name.StartsWith("Autodesk", StringComparison.OrdinalIgnoreCase));
        }

        // ================================================================ U-07 Divergent

        [Fact]
        public void U07_DivergentMuestraResumenesPorVista_SinFilasDeRack_YConLaEdicionBloqueada()
        {
            StaTestRunner.Run(() =>
            {
                var workspace = Rack(DivergentRack());
                Assert.Equal(CustomPropertiesWorkspaceState.Divergent, workspace.State);

                var window = Open(workspace, Displayed(DivergentRack()));

                // Ninguna fila: los valores de una hermana nunca se presentan como los del rack.
                Assert.Empty(List(window).Items);
                Assert.Equal(Visibility.Visible, Named<FrameworkElement>(window, "ViewsPanel").Visibility);
                Assert.Equal(workspace.ViewSummaries.Count, Named<Panel>(window, "SummariesList").Children.Count);

                var banner = PresenterText(window, "StateBanner");
                Assert.Contains("vistas", banner, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("distintas", banner, StringComparison.OrdinalIgnoreCase);

                foreach (var name in CrudButtons)
                {
                    Assert.False(Button(window, name).IsEnabled);
                    Assert.Contains("unifícalas", (string)Button(window, name).ToolTip, StringComparison.Ordinal);
                }

                Assert.False(Named<TextBox>(window, "NameBox").IsEnabled);
                Assert.False(Named<TextBox>(window, "ValueBox").IsEnabled);
            });
        }

        [Fact]
        public void U07_DivergentSinUnificacionExplicaPorQueNoSeEdita_YSigueMostrandoCadaVista()
        {
            StaTestRunner.Run(() =>
            {
                var workspace = Rack(DivergentRackWithoutUnify());
                Assert.False(workspace.UnifyAvailable);

                var window = Open(workspace, Displayed(DivergentRackWithoutUnify()));
                var text = AllText(window);

                Assert.Empty(List(window).Items);
                Assert.Contains("Cliente — ACME", text, StringComparison.Ordinal);
                Assert.Contains("Cliente — OTRO", text, StringComparison.Ordinal);

                foreach (var name in CrudButtons)
                {
                    Assert.False(Button(window, name).IsEnabled);
                    Assert.Contains("ninguna vista", (string)Button(window, name).ToolTip, StringComparison.Ordinal);
                }
            });
        }

        // ================================================================ U-08 nombres repetidos

        [Fact]
        public void U08_LosNombresRepetidosSeAvisanSinVolverSoloLectura()
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState("rack-repeated");
                var items = List(window).Items.Cast<RackCustomPropertiesWindow.PropertyItem>().ToList();

                Assert.Equal(new[] { true, true, false }, items.Select(item => item.NombreRepetido));
                Assert.All(items.Take(2), item => Assert.False(string.IsNullOrWhiteSpace(item.Marker)));
                Assert.True(string.IsNullOrEmpty(items[2].Marker));

                var warning = Named<EditorStatusPresenter>(window, "RepeatedNamesPresenter");
                Assert.Equal(Visibility.Visible, warning.Visibility);
                Assert.Equal(EditorStatusSeverity.Warning, warning.CurrentSeverity);
                Assert.Contains("repetid", warning.Message.Text, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Cliente", warning.Message.Text, StringComparison.OrdinalIgnoreCase);

                // Sigue siendo editable: repetir un nombre es un aviso, no un bloqueo.
                Assert.Equal(string.Empty, PresenterText(window, "StateBanner"));
                Assert.True(Button(window, "NewButton").IsEnabled);
                List(window).SelectedIndex = 0;
                Assert.True(Button(window, "RenameButton").IsEnabled);
                Assert.True(Button(window, "DeleteButton").IsEnabled);
            });
        }

        [Fact]
        public void U08_SinNombresRepetidosNoHayAviso()
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState("rack-single");

                Assert.Equal(Visibility.Collapsed, Named<EditorStatusPresenter>(window, "RepeatedNamesPresenter").Visibility);
                Assert.All(List(window).Items.Cast<RackCustomPropertiesWindow.PropertyItem>(), item => Assert.True(string.IsNullOrEmpty(item.Marker)));
            });
        }

        // ================================================================ U-09 sin MessageBox; Cerrar, Escape y Enter

        [Fact]
        public void U09_LaVentanaNoAbreNingunMessageBox()
        {
            var visible = CodeWithoutComments(WindowCodeText()) + XamlWithoutComments(WindowXamlText());

            Assert.DoesNotContain("MessageBox", visible, StringComparison.Ordinal);
            Assert.DoesNotContain("EditorDiscardPrompt", visible, StringComparison.Ordinal);
            Assert.DoesNotContain("ShowDialog", visible, StringComparison.Ordinal);
        }

        [Theory]
        [MemberData(nameof(AllStates))]
        public void U09_CerrarEsLaAccionDeEscape_EnterNoDisparaNinguna_YCerrarNoDevuelveNada(string state)
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState(state);
                var close = Button(window, "CloseButton");

                Assert.Equal("Cerrar", close.Content);
                Assert.True(close.IsCancel);
                Assert.False(close.IsDefault);
                Assert.True(close.IsEnabled);
                Assert.All(Buttons(window), button => Assert.False(button.IsDefault, (button.Content as string) + " es la accion de Enter"));
                Assert.Single(Buttons(window), button => button.IsCancel);

                if (Named<TextBox>(window, "NameBox").IsEnabled)
                {
                    Named<TextBox>(window, "NameBox").Text = "sin enviar";
                }

                EditorWindowTestSupport.ClickNamed(window, "CloseButton");

                Assert.Null(window.Intent);
                Assert.Null(window.UnifyIntent);
            });
        }

        [Fact]
        public void U09_CerrarPorCualquierOtroCaminoTampocoDevuelveNinguna()
        {
            StaTestRunner.Run(() =>
            {
                // Alt+F4 y el boton del sistema llegan como Close(): la misma politica que el boton y Escape.
                var editable = OpenState("rack-single");
                List(editable).SelectedIndex = 0;
                Named<TextBox>(editable, "NameBox").Text = "cambiado sin pedir";
                editable.Close();

                Assert.Null(editable.Intent);
                Assert.Null(editable.UnifyIntent);

                var divergent = OpenState("rack-divergent");
                UnifySources(divergent)[0].IsChecked = true;
                Named<CheckBox>(divergent, "UnifyConfirmCheck").IsChecked = true;
                divergent.Close();

                Assert.Null(divergent.Intent);
                Assert.Null(divergent.UnifyIntent);
            });
        }

        [Fact]
        public void U09_ElFocoInicialEsDeterminista_YNuncaCaeEnUnaAccionDestructivaNiBloqueada()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (state, expected) in new[]
                         {
                             ("rack-single", "PropertiesList"),
                             ("project-readable", "PropertiesList"),
                             ("project-absent", "NameBox"),
                             ("rack-divergent", "CloseButton"),
                             ("rack-divergent-sin-unificar", "CloseButton"),
                             ("rack-xref", "CloseButton"),
                             ("project-depth", "CloseButton"),
                         })
                {
                    var window = OpenState(state);
                    var focused = FocusManager.GetFocusedElement(window) as FrameworkElement;

                    Assert.NotNull(focused);
                    Assert.Equal(expected, focused.Name);
                    Assert.True(focused.IsEnabled, state + ": el foco cae en algo bloqueado");
                    Assert.NotEqual("DeleteButton", focused.Name);
                    Assert.NotEqual("UnifyButton", focused.Name);
                }
            });
        }

        [Fact]
        public void U09_ElEstadoQueDevuelveElComandoSeMuestraEnLaVentana()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackCustomPropertiesWindow(ProjectAbsent(), null, EditorStatusMessage.Error("No se escribió nada: el nombre ya existe."));
                var presenter = Named<EditorStatusPresenter>(window, "StatusPresenter");

                Assert.Equal(Visibility.Visible, presenter.Visibility);
                Assert.Equal(EditorStatusSeverity.Error, presenter.CurrentSeverity);
                Assert.Equal("No se escribió nada: el nombre ya existe.", presenter.Message.Text);

                Assert.Equal(Visibility.Collapsed, Named<EditorStatusPresenter>(Open(ProjectAbsent()), "StatusPresenter").Visibility);
            });
        }

        // ================================================================ U-10 arquetipo C

        [Fact]
        public void U10_LaVentanaAdoptaElChromeDelArquetipoC_ConSuUbicacionYSuTamano()
        {
            StaTestRunner.Run(() =>
            {
                var styles = new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };
                var expected = (Style)styles[DialogWindowChrome.StyleKey];
                var window = OpenState("rack-single");

                Assert.NotNull(window.Style);
                Assert.Equal(expected.TargetType, window.Style.TargetType);
                Assert.Equal(expected.Setters.Count, window.Style.Setters.Count);
                Assert.Equal(((SolidColorBrush)styles["WindowBackgroundBrush"]).Color, ((SolidColorBrush)window.Background).Color);
                Assert.Equal("Segoe UI", window.FontFamily?.Source);

                // D9: modal centrada en su dueño, con un tamaño y un minimo declarados por la propia ventana.
                Assert.Equal(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
                Assert.True(window.Width > 0 && !double.IsNaN(window.Width));
                Assert.True(window.Height > 0 && !double.IsNaN(window.Height));
                Assert.True(window.MinWidth > 0 && window.MinWidth <= window.Width);
                Assert.True(window.MinHeight > 0 && window.MinHeight <= window.Height);
            });
        }

        [Fact]
        public void U10_ElChromeSaleDeLaFuenteUnica_YNoSeRepiteAMano()
        {
            var code = CodeWithoutComments(WindowCodeText());
            var xaml = XamlWithoutComments(WindowXamlText());
            var root = Regex.Match(xaml, @"<Window\b[^>]*>", RegexOptions.Singleline).Value;

            Assert.Contains("DialogWindowChrome.Apply(this)", code, StringComparison.Ordinal);
            Assert.Contains("x:Class=\"RackCad.UI.RackCustomPropertiesWindow\"", root, StringComparison.Ordinal);
            Assert.DoesNotContain("Background=", root, StringComparison.Ordinal);
            Assert.DoesNotContain("FontFamily=", root, StringComparison.Ordinal);
            Assert.DoesNotContain("AppStyles.xaml", xaml, StringComparison.Ordinal);
        }

        [Theory]
        [MemberData(nameof(AllStates))]
        public void U10_LasAccionesSalenDeLaFabricaComun_ConSuMotivoLegibleAunApagadas(string state)
        {
            StaTestRunner.Run(() =>
            {
                var window = OpenState(state);
                var primary = ShellResources.Get<Style>("PrimaryButtonStyle");
                var secondary = ShellResources.Get<Style>("SecondaryButtonStyle");

                Assert.NotNull(primary);
                Assert.NotNull(secondary);
                Assert.NotEmpty(Buttons(window));

                foreach (var button in Buttons(window))
                {
                    Assert.True(ToolTipService.GetShowOnDisabled(button), (button.Content as string) + " no sale de EditorActions");
                    Assert.True(ReferenceEquals(button.Style, primary) || ReferenceEquals(button.Style, secondary), (button.Content as string) + " con estilo propio");

                    if (!button.IsEnabled)
                    {
                        Assert.False(string.IsNullOrWhiteSpace(button.ToolTip as string), (button.Content as string) + " apagado sin motivo en " + state);
                    }
                }
            });
        }
    }
}
