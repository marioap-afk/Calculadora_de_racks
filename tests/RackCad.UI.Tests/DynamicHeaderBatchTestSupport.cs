using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.Dynamic;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D (G7): drives the REAL cabecera surface of the Dinamico editor —«Editar cabecera», «Tomar como origen»,
    /// «Cabeceras destino», «Aplicar origen a destinos», «Configuración de cabecera» and the rebuild report— the way a user
    /// does, and reads back what the gesture left.
    /// <para>
    /// Every lookup asserts presence before it interacts, so a missing control fails a test by ASSERTION and never by an
    /// accidental exception. The designs that bring a custom cabecera or a manual length are FIXTURES: they set the scene the
    /// way a saved or drawn rack brings it (<c>Modules[].Header</c> + provenance); they are not the gesture under test.
    /// </para>
    /// </summary>
    internal static class DynamicHeaderBatchTestSupport
    {
        internal const string PostId = "POSTE_OMEGA_3X3";

        /// <summary>The PanelClear that makes a fixture's custom cabecera recognizable (the standard one is 44).</summary>
        internal const double Marker = 41.5;

        private static readonly Lazy<RackCatalog> LazyCatalog =
            new Lazy<RackCatalog>(() => JsonRackCatalogProvider.FromBaseDirectory().Load());

        internal static RackCatalog Catalog => LazyCatalog.Value;

        // ---- Designs (fixtures) ------------------------------------------------------------------------------------

        /// <summary>
        /// A Dinamico rack built by the productive assembler with EXPLICIT modules and fronts that span them, so loading it
        /// and recomposing it as it is does not rebuild. One front of five: M1 cabecera de inicio, M2 separador, M3 cabecera
        /// intermedia, M4 separador, M5 cabecera final.
        /// </summary>
        internal static DynamicRackDesign Design(int palletsDeep = 5)
            => Design(palletsDeep, (palletsDeep, true));

        /// <summary>The same, with one front per entry: its fondos (from position 1) and whether it is active.</summary>
        internal static DynamicRackDesign Design(int palletsDeep, params (int Deep, bool Active)[] fronts)
        {
            var builder = new DynamicRackSystemBuilder(Catalog);
            var assembler = new DynamicEditorDesignAssembler(Catalog, builder, new DynamicRackSystemResolver(Catalog));

            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(fronts.Length);
            for (var i = 0; i < fronts.Length; i++)
            {
                matrix.Fronts[i].PalletsDeep = fronts[i].Deep;
                matrix.Fronts[i].DepthStartPosition = 1;
                matrix.Fronts[i].IsActive = fronts[i].Active;
            }

            // The module sequence follows the depth layout the fronts resolve to, exactly as the window builds it.
            var system = builder.BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                DynamicDepthGeometry.Resolve(matrix.BuildFrontDesigns(), palletsDeep),
                RackFrameTemplateCatalog.Default,
                PostId,
                132.0,
                3.0);

            return assembler.BuildDesign(
                system,
                matrix,
                levels: 3,
                firstLevel: 6.0,
                beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                headerPostCatalogId: PostId,
                palletsDeep: palletsDeep,
                postPeralte: 3.0,
                palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                annotations: new DynamicAnnotationOptions(),
                safetySelections: null);
        }

        /// <summary>Fixture: <paramref name="moduleId"/> arrives as a CUSTOM cabecera recognizable by its PanelClear.</summary>
        internal static DynamicRackDesign WithCustom(DynamicRackDesign design, string moduleId, double panelClear = Marker)
        {
            var intent = Intent(design, moduleId);
            Assert.True(intent.IsHeader, "Fixture: " + moduleId + " no es una cabecera.");
            var configuration = new DynamicRackSystemBuilder(Catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, PostId, 132.0, intent.Length, design.PostPeralte);
            configuration.PanelClear = panelClear;
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            intent.HeaderConfiguration = configuration;
            intent.UseCalculatedHeaderConfiguration = false;
            return design;
        }

        /// <summary>Fixture: <paramref name="moduleId"/> arrives with a MANUAL length.</summary>
        internal static DynamicRackDesign WithManualLength(DynamicRackDesign design, string moduleId, double length)
        {
            var intent = Intent(design, moduleId);
            intent.Length = length;
            intent.IsManualOverride = true;
            intent.IsCalculated = false;
            return design;
        }

        private static DynamicRackModuleDesign Intent(DynamicRackDesign design, string moduleId)
        {
            var intent = design.Modules.SingleOrDefault(module => module.ModuleId == moduleId);
            Assert.True(intent != null, "Fixture: el diseño no tiene el módulo " + moduleId + ".");
            return intent;
        }

        /// <summary>The store half of RACKEDITAR and of «Abrir proyecto»: what the drawing or the file keeps, read back.</summary>
        internal static RackProject RoundTrip(DynamicRackDesign design)
        {
            var store = new RackProjectStore();
            return store.Deserialize(store.Serialize(RackProject.ForDynamic(design)));
        }

        // ---- The window -------------------------------------------------------------------------------------------

        /// <summary>
        /// Open the REAL editor on <paramref name="design"/> as RACKEDITAR does (<c>LoadExisting</c>), shown off-screen —the
        /// configurator takes it as Owner, and a window never shown cannot own another— with the advanced panel open.
        /// </summary>
        internal static RackDynamicSystemWindow Open(DynamicRackDesign design = null)
        {
            var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = -10000;
            window.Top = -10000;
            window.ShowInTaskbar = false;
            window.Show();
            window.LoadExisting(design ?? Design(), "GUID-I53D", "Dinamico I-53D");
            var advanced = window.FindName("AdvancedToggle") as CheckBox;
            Assert.True(advanced != null, "No existe «Avanzado» (AdvancedToggle).");
            advanced.IsChecked = true;
            return window;
        }

        /// <summary>Run <paramref name="body"/> on the STA thread against a freshly opened editor, and close it afterwards.</summary>
        internal static T Run<T>(Func<RackDynamicSystemWindow, T> body, DynamicRackDesign design = null)
            => StaTestRunner.Run(() =>
            {
                var window = Open(design);
                try
                {
                    return body(window);
                }
                finally
                {
                    window.Close();
                }
            });

        internal static DataGrid Grid(RackDynamicSystemWindow window)
        {
            var grid = window.FindName("ModulesGrid") as DataGrid;
            Assert.True(grid != null, "No existe la tabla de módulos (ModulesGrid).");
            return grid;
        }

        /// <summary>The modules exactly as the table shows them: the resolved system's own instances.</summary>
        internal static IReadOnlyList<DynamicRackModule> Modules(RackDynamicSystemWindow window)
        {
            var items = Grid(window).ItemsSource as IEnumerable<DynamicRackModule>;
            Assert.True(items != null, "La tabla de módulos no muestra un sistema resuelto.");
            return items.ToList();
        }

        internal static DynamicRackModule Module(RackDynamicSystemWindow window, string moduleId)
        {
            var module = Modules(window).SingleOrDefault(candidate => candidate.ModuleId == moduleId);
            Assert.True(module != null, "La tabla de módulos no tiene " + moduleId + ".");
            return module;
        }

        internal static IReadOnlyList<string> HeaderIds(RackDynamicSystemWindow window)
            => Modules(window).Where(module => module.IsHeader).OrderBy(module => module.Index).Select(module => module.ModuleId).ToList();

        /// <summary>Select a module in the table, as a click on its row does.</summary>
        internal static void Select(RackDynamicSystemWindow window, string moduleId) => Grid(window).SelectedItem = Module(window, moduleId);

        internal static void TakeSource(RackDynamicSystemWindow window) => Click(window, "TakeHeaderSourceButton");

        internal static void Apply(RackDynamicSystemWindow window) => Click(window, "ApplyHeaderBatchButton");

        internal static void EditHeader(RackDynamicSystemWindow window) => Click(window, "EditHeaderButton");

        internal static void Click(RackDynamicSystemWindow window, string name)
        {
            var button = window.FindName(name) as ButtonBase;
            Assert.True(button != null, "No existe el botón " + name + ".");
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }

        /// <summary>«Actualizar vista (recalcular)»: the window's own recomposition with the inputs as typed.</summary>
        internal static void Recalculate(RackDynamicSystemWindow window)
            => EditorWindowTestSupport.ClickByContent(window, "Actualizar vista (recalcular)");

        /// <summary>«Restaurar layout»: «Restaurar estándar» of the Dinamico.</summary>
        internal static void RestoreLayout(RackDynamicSystemWindow window)
            => EditorWindowTestSupport.ClickByContent(window, "Restaurar layout");

        /// <summary>Another pallet depth, typed and recalculated: a rebuild.</summary>
        internal static void ChangePalletDepth(RackDynamicSystemWindow window, double depth)
        {
            var box = window.FindName("DepthBox") as TextBox;
            Assert.True(box != null, "No existe «Fondo tarima» (DepthBox).");
            box.Text = depth.ToString(CultureInfo.InvariantCulture);
            Recalculate(window);
        }

        /// <summary>Another number of fondos for every front through «Todos los frentes»: a rebuild.</summary>
        internal static void ChangeFondosOfAllFronts(RackDynamicSystemWindow window, int palletsDeep)
        {
            var box = window.FindName("SelectedPalletsDeepBox") as TextBox;
            Assert.True(box != null, "No existe «Fondos» del frente (SelectedPalletsDeepBox).");
            box.Text = palletsDeep.ToString(CultureInfo.InvariantCulture);
            EditorWindowTestSupport.ClickByContent(window, "Todos los frentes");
        }

        /// <summary>The rack-wide post peralte, typed and committed: a recomposition WITHOUT rebuild.</summary>
        internal static void ChangePostPeralte(RackDynamicSystemWindow window, double peralte)
        {
            var box = window.FindName("PostPeralteBox") as TextBox;
            Assert.True(box != null, "No existe «Peralte de poste» (PostPeralteBox).");
            box.Text = peralte.ToString(CultureInfo.InvariantCulture);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>«Aplicar» of the selected module with another kind: an in-place edit, WITHOUT rebuild.</summary>
        internal static void ChangeKind(RackDynamicSystemWindow window, string moduleId, string kindLabel)
        {
            Select(window, moduleId);
            var kind = window.FindName("KindBox") as ComboBox;
            Assert.True(kind != null, "No existe «Tipo» (KindBox).");
            kind.SelectedItem = kindLabel;
            Click(window, "ApplyModuleButton");
        }

        // ---- «Cabeceras destino» ----------------------------------------------------------------------------------

        internal static StackPanel TargetsList(RackDynamicSystemWindow window)
        {
            var list = window.FindName("ModuleTargetsList") as StackPanel;
            Assert.True(list != null, "No existe el selector «Cabeceras destino» (ModuleTargetsList).");
            return list;
        }

        internal static string TargetsCaption(RackDynamicSystemWindow window)
            => (window.FindName("ModuleTargetsButton") as ToggleButton)?.Content as string;

        internal static void TargetsCurrent(RackDynamicSystemWindow window) => PressTargets(window, "Actual");

        internal static void TargetsAll(RackDynamicSystemWindow window) => PressTargets(window, "Todas");

        /// <summary>Aim «Cabeceras destino» at exactly these modules, as the user would: the wanted boxes on, the rest off.</summary>
        internal static void Targets(RackDynamicSystemWindow window, params string[] moduleIds)
        {
            TargetsCurrent(window);
            foreach (var id in moduleIds)
            {
                Toggle(window, id, true);
            }

            foreach (var id in TargetBoxes(window).Where(id => !moduleIds.Contains(id)).ToList())
            {
                Toggle(window, id, false);
            }
        }

        /// <summary>The boxes «Cabeceras destino» offers, in order.</summary>
        internal static IReadOnlyList<string> TargetBoxes(RackDynamicSystemWindow window)
            => TargetsList(window).Children.OfType<CheckBox>().Select(box => box.Content as string).ToList();

        private static void PressTargets(RackDynamicSystemWindow window, string label)
        {
            var button = TargetsList(window).Children.OfType<Button>()
                .FirstOrDefault(candidate => (candidate.Content as string) == label || (candidate.Content as string) == "✓ " + label);
            Assert.True(button != null, "«Cabeceras destino» no ofrece la acción «" + label + "».");
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }

        private static void Toggle(RackDynamicSystemWindow window, string moduleId, bool wanted)
        {
            var box = TargetsList(window).Children.OfType<CheckBox>().FirstOrDefault(candidate => (candidate.Content as string) == moduleId);
            Assert.True(box != null, "«Cabeceras destino» no ofrece " + moduleId + ".");
            if ((box.IsChecked == true) == wanted)
            {
                return;
            }

            box.IsChecked = wanted;
        }

        // ---- Texts the window shows --------------------------------------------------------------------------------

        internal static string Text(RackDynamicSystemWindow window, string name)
        {
            var block = window.FindName(name) as TextBlock;
            Assert.True(block != null, "No existe el texto " + name + ".");
            return block.Text ?? string.Empty;
        }

        internal static string Status(RackDynamicSystemWindow window) => Text(window, "StatusText");

        internal static string SourceCaption(RackDynamicSystemWindow window) => Text(window, "HeaderSourceText");

        /// <summary>The rebuild report as the user sees it: empty when it is collapsed.</summary>
        internal static string RebuildReport(RackDynamicSystemWindow window)
        {
            var block = window.FindName("RebuildReportText") as TextBlock;
            Assert.True(block != null, "No existe el informe de reconstrucción (RebuildReportText).");
            return block.Visibility == Visibility.Visible ? block.Text ?? string.Empty : string.Empty;
        }

        /// <summary>The entries of «Configuración de cabecera», whatever form the combo keeps them in.</summary>
        internal static IReadOnlyList<string> ConfigEntries(RackDynamicSystemWindow window)
            => ConfigBox(window).Items.Cast<object>().Select(item => item is ComboBoxItem entry ? entry.Content as string : item as string).ToList();

        internal static ComboBox ConfigBox(RackDynamicSystemWindow window)
        {
            var box = window.FindName("ConfigBox") as ComboBox;
            Assert.True(box != null, "No existe «Configuración de cabecera» (ConfigBox).");
            return box;
        }

        /// <summary>Choose «Calculada» in «Configuración de cabecera» for the selected module, as a pick from the list does.</summary>
        internal static void ChooseCalculated(RackDynamicSystemWindow window)
        {
            var box = ConfigBox(window);
            var index = ConfigEntries(window).ToList().IndexOf("Calculada");
            Assert.True(index >= 0, "«Configuración de cabecera» no ofrece «Calculada».");
            if (box.SelectedIndex == index)
            {
                box.SelectedIndex = -1;
            }

            box.SelectedIndex = index;
        }

        // ---- Configurator presenters -------------------------------------------------------------------------------

        /// <summary>The advanced editor's way: record the mode, edit the configuration the configurator shows IN PLACE, close.</summary>
        internal static Action<RackFrameConfiguratorWindow> Edit(Action<RackFrameConfiguration> edit, List<bool> modes = null)
            => configurator =>
            {
                modes?.Add(configurator.ViewModel.IsAdvancedEditor);
                edit?.Invoke(configurator.ViewModel.Configuration);
                configurator.Close();
            };

        /// <summary>«Configuración rápida»: a height and «Aplicar», which REPLACES the ViewModel's configuration.</summary>
        internal static Action<RackFrameConfiguratorWindow> QuickConfigAt(double height)
            => configurator =>
            {
                configurator.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                configurator.ViewModel.ApplySimpleConfiguration();
                configurator.Close();
            };

        // ---- Read-only fingerprints ------------------------------------------------------------------------------

        /// <summary>
        /// The recipe of a cabecera: its persisted projection WITHOUT what the Dinamico recompute imposes on each module (fondo
        /// = the module's length, peralte = the rack's). Two modules with the same recipe carry the same customization.
        /// </summary>
        internal static string Recipe(RackFrameConfiguration configuration)
        {
            Assert.NotNull(configuration);
            var copy = new RackFrameProjectStore().DeepCopy(configuration);
            copy.Depth = 1.0;
            copy.PostPeralte = 1.0;
            return new RackProjectStore().Serialize(RackProject.ForSelective(copy));
        }

        /// <summary>
        /// Every object a recompute of the rack would replace or regenerate, by IDENTITY: the modules, their cabeceras and
        /// the cabeceras' physical members. <c>Refresh</c> and <c>ApplyPostPeralte</c> regenerate the members; a recomposition
        /// of the window replaces the modules.
        /// </summary>
        internal static HashSet<object> Graph(IEnumerable<DynamicRackModule> modules)
        {
            var graph = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var module in modules)
            {
                graph.Add(module);
                var configuration = module.AssociatedFrameConfiguration;
                if (configuration == null)
                {
                    continue;
                }

                graph.Add(configuration);
                foreach (var member in configuration.Members)
                {
                    graph.Add(member);
                }
            }

            return graph;
        }

        /// <summary>Strict correspondence (I-24): the full drawing built from the design (resolved) equals the one built from the system.</summary>
        internal static bool Corresponds(DynamicRackDesign design, DynamicRackSystem system)
        {
            var resolved = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            resolved.Name = system.Name;
            return FullDrawingSignature(resolved) == FullDrawingSignature(system);
        }

        /// <summary>
        /// The FULL drawing of a resolved system (I-24): every instance of every lateral corte, the frontal exit, the frontal
        /// entrance and the planta, annotations included, deterministically ordered.
        /// </summary>
        internal static string FullDrawingSignature(DynamicRackSystem system)
            => string.Join("\n", ViewKeys(system).OrderBy(key => key, StringComparer.Ordinal));

        /// <summary>One view type of the drawing: "lateral", "frontal" or "planta".</summary>
        internal static string ViewSignature(DynamicRackSystem system, string view)
            => string.Join("\n", ViewKeys(system).Where(key => key.StartsWith(view, StringComparison.Ordinal)).OrderBy(key => key, StringComparer.Ordinal));

        private static IEnumerable<string> ViewKeys(DynamicRackSystem system)
        {
            foreach (var corte in new DynamicSystemLateralBuilder().Cortes(system, Catalog))
            {
                foreach (var instance in corte.Plan.Flatten().Instances)
                {
                    yield return InstanceKey("lateral#" + corte.PostIndex.ToString(CultureInfo.InvariantCulture), instance);
                }
            }

            var frontal = new DynamicSystemFrontalBuilder();
            foreach (var instance in frontal.Build(system, Catalog, DynamicRackEnd.Exit))
            {
                yield return InstanceKey("frontal-exit", instance);
            }

            foreach (var instance in frontal.Build(system, Catalog, DynamicRackEnd.Entrance))
            {
                yield return InstanceKey("frontal-entrance", instance);
            }

            foreach (var instance in new DynamicSystemPlantaBuilder().Build(system, Catalog))
            {
                yield return InstanceKey("planta", instance);
            }
        }

        private static string InstanceKey(string viewTag, HeaderBlockInstance i)
        {
            var parameters = string.Join(";", i.DynamicParameters
                .OrderBy(k => k.Key, StringComparer.Ordinal)
                .Select(k => k.Key + "=" + k.Value.ToString("R", CultureInfo.InvariantCulture)));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4}|{5:R},{6:R}|{7:R},{8:R}|{9:R}|{10}{11}|{12}|{13:R}|{14}|{15}",
                viewTag, (int)i.Role, i.BlockName, i.PieceId, i.View,
                i.Insertion.X, i.Insertion.Y, i.ConnectionAnchor.X, i.ConnectionAnchor.Y,
                i.RotationRadians, i.MirroredX ? 1 : 0, i.MirroredY ? 1 : 0,
                parameters, i.DimensionOffset, i.Text ?? string.Empty, i.DimensionStyleName ?? string.Empty);
        }
    }
}
