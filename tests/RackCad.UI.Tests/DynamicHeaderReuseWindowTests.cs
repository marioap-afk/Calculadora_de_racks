using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.UI.Systems.Dynamic;
using Xunit;
using D = RackCad.UI.Tests.DynamicHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D, G7 — ID6 REUSE + ID7 BATCH DISTRIBUTION in the Dinamico window (Proposal V2 §7.3-§7.7, §7.11; D-26, D-27).
    /// <para>
    /// The window only collects intent: «Tomar como origen» remembers a <c>ModuleId</c> signed with the sequence in force,
    /// «Cabeceras destino» is <see cref="DynamicModuleTargets"/> (Actual / explícito / Todas), and «Aplicar origen a destinos»
    /// hands both to <see cref="DynamicHeaderBatch"/>. What a copy is, who receives one, what is omitted and why, and which
    /// flags change are Application's. After G7 that is the ONLY reuse of an authored cabecera: the presets «Personalizada N»
    /// are gone, and «Calculada» stays the historical per-module reset.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderReuseWindowTests
    {
        private static HeaderBatchOutcome<DynamicHeaderAddress>.Committed Committed(HeaderBatchOutcome<DynamicHeaderAddress> outcome)
            => Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(outcome);

        private static string Ids(IEnumerable<DynamicHeaderAddress> addresses)
            => string.Join(" ", addresses.Select(address => address.ModuleId));

        private static string Omissions(IEnumerable<HeaderOmission<DynamicHeaderAddress>> omitted)
            => string.Join(" ", omitted.Select(omission => omission.Address.ModuleId + ":" + omission.Reason));

        // =============================================================================================================
        // D-27 — «Tomar como origen», «Cabeceras destino» y «Aplicar origen a destinos»: el único mecanismo de reutilización
        // =============================================================================================================

        [Fact]
        public void D27_TomarComoOrigen_RecuerdaSoloLaDireccion_SinCopiarNiCambiarLaGeometria()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                var graph = D.Graph(D.Modules(window));
                var recomputes = window.RecomputeCount;
                D.TakeSource(window);
                return (
                    Source: window.HeaderBatchStateForTest.Source?.Address.ModuleId,
                    Caption: D.SourceCaption(window),
                    SameRack: graph.SetEquals(D.Graph(D.Modules(window))),
                    Recomputes: window.RecomputeCount - recomputes);
            }, D.WithCustom(D.Design(), "M1"));

            Assert.Equal("M1", r.Source);
            Assert.Contains("M1", r.Caption);
            Assert.Contains("personalizada", r.Caption);
            Assert.True(r.SameRack, "«Tomar como origen» no copia ni recalcula nada");
            Assert.Equal(0, r.Recomputes);
        }

        [Fact]
        public void D27_OrigenEnVivo_EditarElOrigenDespuesDeTomarlo_DistribuyeElValorNuevo()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);

                // Se edita el ORIGEN despues de tomarlo, sin volver a tomarlo.
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 37.25);
                D.EditHeader(window);
                var edit = window.LastHeaderBatchOutcome;

                D.Targets(window, "M3");
                D.Apply(window);
                return (
                    Edit: edit,
                    Outcome: window.LastHeaderBatchOutcome,
                    Source: D.Module(window, "M1").AssociatedFrameConfiguration.PanelClear,
                    Target: D.Module(window, "M3").AssociatedFrameConfiguration.PanelClear);
            }, D.WithCustom(D.Design(), "M1"));

            Committed(r.Edit);
            Assert.Equal("M3", Ids(Committed(r.Outcome).Applied));
            Assert.Equal(37.25, r.Source, 4);
            Assert.Equal(37.25, r.Target, 4); // el valor de la cabecera AL APLICAR, no el del momento en que se tomó
        }

        [Fact]
        public void D27_AplicarAUna_AVarias_YATodas_CadaDestinoRecibeSuPropiaCopia()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);

                D.Targets(window, "M3");
                D.Apply(window);
                var one = Committed(window.LastHeaderBatchOutcome);

                D.Targets(window, "M5", "M7");
                D.Apply(window);
                var several = Committed(window.LastHeaderBatchOutcome);

                D.TargetsAll(window);
                D.Apply(window);
                var all = Committed(window.LastHeaderBatchOutcome);

                var headers = D.HeaderIds(window).Select(id => D.Module(window, id)).ToList();
                return (
                    One: Ids(one.Applied),
                    Several: Ids(several.Applied),
                    All: Ids(all.Applied),
                    AllOmitted: Omissions(all.Omitted),
                    Recipes: headers.Select(module => D.Recipe(module.AssociatedFrameConfiguration)).Distinct().Count(),
                    Instances: headers.Select(module => module.AssociatedFrameConfiguration).Distinct(ReferenceEqualityComparer.Instance).Count(),
                    Headers: headers.Count,
                    Custom: headers.All(module => !module.UseCalculatedHeaderConfiguration));
            }, D.WithCustom(D.Design(9), "M1"));

            Assert.Equal("M3", r.One);
            Assert.Equal("M5 M7", r.Several);
            Assert.Equal("M3 M5 M7 M9", r.All);
            Assert.Equal("M1:IsSource", r.AllOmitted);
            Assert.Equal(1, r.Recipes);               // todas llevan la personalización del origen
            Assert.Equal(r.Headers, r.Instances);     // y cada una es una instancia propia: ninguna copia compartida
            Assert.True(r.Custom);
        }

        [Fact]
        public void D27_CopiasIndependientes_EditarUnDestinoNoMueveNiAlOrigenNiALosDemas()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsAll(window);
                D.Apply(window);

                D.Select(window, "M3");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 35.75);
                D.EditHeader(window);

                return (
                    M1: D.Module(window, "M1").AssociatedFrameConfiguration.PanelClear,
                    M3: D.Module(window, "M3").AssociatedFrameConfiguration.PanelClear,
                    M5: D.Module(window, "M5").AssociatedFrameConfiguration.PanelClear);
            }, D.WithCustom(D.Design(), "M1"));

            Assert.Equal(D.Marker, r.M1, 4);
            Assert.Equal(35.75, r.M3, 4);
            Assert.Equal(D.Marker, r.M5, 4);
        }

        [Fact]
        public void D27_UnaCabeceraQueNoSeDibuja_SeOmite_YElInformeLoDice()
        {
            // Frentes [activo 4][en blanco 4][en blanco 10][en blanco 4][activo 4]: los modulos que solo alcanzan las fronteras
            // entre frentes en blanco existen y no se dibujan (I-33).
            var design = D.WithCustom(D.Design(10, (4, true), (4, false), (10, false), (4, false), (4, true)), "M1");
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsAll(window);
                D.Apply(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Status: D.Status(window),
                    Untouched: new[] { "M6", "M8", "M10" }.All(id => D.Module(window, id).UseCalculatedHeaderConfiguration));
            }, design);

            var committed = Committed(r.Outcome);
            Assert.Equal("M4", Ids(committed.Applied));
            Assert.Equal("M1:IsSource M6:NotPhysicallyPresent M8:NotPhysicallyPresent M10:NotPhysicallyPresent", Omissions(committed.Omitted));
            Assert.True(r.Untouched);
            Assert.Contains("M6 (no se dibuja)", r.Status);
        }

        [Fact]
        public void D27_ActualSinSeleccion_EsNoTargets_YLaVentanaNoReorientaLaSeleccion()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsCurrent(window);

                // Hecho historico que se conserva: toda recomposicion deja la seleccion vacia.
                D.ChangePostPeralte(window, 3.5);
                var selectionAfterRecompose = D.Grid(window).SelectedItem;
                D.Apply(window);
                return (
                    SelectionAfterRecompose: selectionAfterRecompose,
                    SelectionAfterApply: D.Grid(window).SelectedItem,
                    Outcome: window.LastHeaderBatchOutcome,
                    Status: D.Status(window));
            }, D.WithCustom(D.Design(), "M1"));

            Assert.Null(r.SelectionAfterRecompose);
            Assert.Equal(HeaderRejectionCode.NoTargets,
                Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Rejected>(r.Outcome).Code);
            Assert.Null(r.SelectionAfterApply); // ningun modulo «parecido» fue elegido por la ventana
            Assert.Contains("no hay destinos", r.Status);
        }

        [Fact]
        public void D27_AbrirOtroDiseno_OlvidaElOrigenYLosDestinos_SinRecordarPreferencias()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.Targets(window, "M3");
                var before = (Source: window.HeaderBatchStateForTest.Source != null, Mode: window.HeaderBatchStateForTest.Targets.Mode);

                // RACKEDITAR o «Abrir proyecto» de otro rack con los mismos ids: nada de lo elegido viaja a el.
                window.LoadExisting(D.WithCustom(D.Design(), "M1"), "GUID-OTRO", "Otro dinamico");
                var state = window.HeaderBatchStateForTest;
                return (
                    Before: before,
                    Source: state.Source,
                    Mode: state.Targets.Mode,
                    SourceCaption: D.SourceCaption(window),
                    TargetsCaption: D.TargetsCaption(window));
            }, D.WithCustom(D.Design(), "M1"));

            Assert.True(r.Before.Source);
            Assert.Equal(DynamicModuleTargetMode.Explicit, r.Before.Mode);
            Assert.Null(r.Source);
            Assert.Equal(DynamicModuleTargetMode.FollowCurrent, r.Mode);
            Assert.Equal("Sin origen.", r.SourceCaption);
            Assert.Equal("Actual", r.TargetsCaption);
        }

        [Fact]
        public void D27_TrasEditarCabecera_NoAparecePersonalizadaN_NiQuedaBibliotecaDeConfiguraciones()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 39.0);
                D.EditHeader(window);

                D.Select(window, "M3");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 38.0);
                D.EditHeader(window);

                // Una cabecera calculada no puede recibir por el desplegable la configuración que se editó en otra.
                D.Select(window, "M5");
                var box = D.ConfigBox(window);
                return (
                    Entries: D.ConfigEntries(window).ToArray(),
                    SelectableEntries: box.Items.Cast<object>()
                        .Count(item => !(item is ComboBoxItem entry) || entry.IsEnabled),
                    M5Calculated: D.Module(window, "M5").UseCalculatedHeaderConfiguration);
            });

            Assert.DoesNotContain(r.Entries, entry => Regex.IsMatch(entry ?? string.Empty, @"^Personalizada \d+$"));
            Assert.Equal(new[] { "Calculada", "Personalizada" }, r.Entries); // procedencia, no una lista de configuraciones
            Assert.Equal(1, r.SelectableEntries);                              // solo «Calculada» se elige
            Assert.True(r.M5Calculated);
        }

        [Fact]
        public void D27_GUARD_LaVentanaNoGuardaConfiguracionesComoBibliotecaNiPortapapeles()
        {
            // Ningun campo de la ventana es una coleccion de configuraciones de cabecera ni de algo que las lleve (el preset
            // «Personalizada N» era List<HeaderPreset> con su Config), y ningun tipo anidado las guarda.
            var window = typeof(RackDynamicSystemWindow);
            var libraries = window.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => IsLibraryOfConfigurations(field.FieldType))
                .Select(field => field.Name)
                .ToList();
            Assert.Empty(libraries);
            Assert.DoesNotContain(
                window.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic),
                nested => CarriesConfiguration(nested));

            // El origen es una direccion con su firma: no hay copia escondida que se pegue despues (ID6).
            Assert.DoesNotContain(
                typeof(DynamicHeaderSource).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                property => typeof(RackFrameConfiguration).IsAssignableFrom(property.PropertyType));

            // Y en la fuente no queda nada de la ruta del preset.
            var source = Source("RackDynamicSystemWindow.xaml.cs");
            var xaml = Source("RackDynamicSystemWindow.xaml");
            foreach (var fragment in new[] { "headerPresets", "HeaderPreset", "\"Personalizada \" +" })
            {
                Assert.DoesNotContain(fragment, source, StringComparison.Ordinal);
            }

            Assert.DoesNotContain("Personalizada N", xaml, StringComparison.Ordinal);
            Assert.Contains("new DynamicHeaderBatchState()", source, StringComparison.Ordinal);
        }

        private static bool IsLibraryOfConfigurations(Type type)
        {
            if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type))
            {
                return false;
            }

            var element = type.IsArray
                ? type.GetElementType()
                : type.GetInterfaces().Concat(new[] { type })
                    .Where(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(candidate => candidate.GetGenericArguments()[0])
                    .FirstOrDefault();
            return element != null && (typeof(RackFrameConfiguration).IsAssignableFrom(element) || CarriesConfiguration(element));
        }

        private static bool CarriesConfiguration(Type type)
            => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   .Any(property => typeof(RackFrameConfiguration).IsAssignableFrom(property.PropertyType))
               || type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                   .Any(field => typeof(RackFrameConfiguration).IsAssignableFrom(field.FieldType));

        // =============================================================================================================
        // D-26 — IsManualOverride = SOLO longitud manual; «Calculada» caracterizada
        // =============================================================================================================

        [Fact]
        public void D26_Distribuir_NuncaFijaIsManualOverride_NiTocaLongitudNiIsCalculated()
        {
            var design = D.WithManualLength(D.WithCustom(D.Design(), "M1"), "M3", 50.0);
            var r = D.Run(window =>
            {
                var m3Before = (D.Module(window, "M3").Length, D.Module(window, "M3").IsManualOverride, D.Module(window, "M3").IsCalculated);
                var m5Before = (D.Module(window, "M5").Length, D.Module(window, "M5").IsManualOverride, D.Module(window, "M5").IsCalculated);
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsAll(window);
                D.Apply(window);
                var m3 = D.Module(window, "M3");
                var m5 = D.Module(window, "M5");
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    M3Before: m3Before,
                    M3After: (m3.Length, m3.IsManualOverride, m3.IsCalculated),
                    M5Before: m5Before,
                    M5After: (m5.Length, m5.IsManualOverride, m5.IsCalculated),
                    Custom: !m3.UseCalculatedHeaderConfiguration && !m5.UseCalculatedHeaderConfiguration);
            }, design);

            Assert.Equal("M3 M5", Ids(Committed(r.Outcome).Applied));
            Assert.Equal((50.0, true, false), r.M3Before);
            Assert.Equal(r.M3Before, r.M3After); // la longitud manual previa sigue siendo la del usuario
            Assert.False(r.M5Before.IsManualOverride);
            Assert.Equal(r.M5Before, r.M5After); // y ninguna copia la inventa
            Assert.True(r.Custom);               // la personalizacion se expresa con la procedencia
        }

        [Fact]
        public void D26_LaPersonalizacionSeVeComoProcedencia_NoComoLongitudManual()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.Targets(window, "M3");
                D.Apply(window);

                D.Select(window, "M3");
                var box = D.ConfigBox(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Shown: box.SelectedItem is ComboBoxItem item ? item.Content as string : box.SelectedItem as string,
                    Manual: D.Module(window, "M3").IsManualOverride,
                    Info: D.Text(window, "SelectedInfoText"));
            }, D.WithCustom(D.Design(), "M1"));

            Committed(r.Outcome);
            Assert.Equal("Personalizada", r.Shown);
            Assert.False(r.Manual);
            Assert.DoesNotContain("[override]", r.Info);
        }

        [Fact]
        public void D26_EditarConOtroFondo_AplicaLaReglaDeLongitudManual_YSinCambiarElFondoNoLaToca()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M3");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.Depth = 50.0);
                D.EditHeader(window);
                var m3 = D.Module(window, "M3");
                var withFondo = (m3.Length, m3.IsManualOverride, m3.IsCalculated, Custom: !m3.UseCalculatedHeaderConfiguration);

                D.Select(window, "M5");
                var m5LengthBefore = D.Module(window, "M5").Length;
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 40.0);
                D.EditHeader(window);
                var m5 = D.Module(window, "M5");
                var withoutFondo = (m5.Length, m5.IsManualOverride, m5.IsCalculated, Custom: !m5.UseCalculatedHeaderConfiguration);
                return (WithFondo: withFondo, WithoutFondo: withoutFondo, M5LengthBefore: m5LengthBefore);
            });

            Assert.Equal((50.0, true, false, true), r.WithFondo);
            Assert.Equal((r.M5LengthBefore, false, true, true), r.WithoutFondo);
        }

        [Fact]
        public void D26_Calculada_RegeneraLaCabecera_YQuitaLaLongitudManual_ComoAntes()
        {
            // Caracterizacion de «Calculada» (N-02): no es un preset; es el restablecimiento historico por modulo.
            var design = D.WithManualLength(D.WithCustom(D.Design(), "M3"), "M3", 50.0);
            var r = D.Run(window =>
            {
                var standardHeight = D.Module(window, "M5").AssociatedFrameConfiguration.Height;
                D.Select(window, "M3");
                D.ChooseCalculated(window);
                var m3 = D.Module(window, "M3");
                return (
                    Calculated: m3.UseCalculatedHeaderConfiguration,
                    Manual: m3.IsManualOverride,
                    m3.Length,
                    PanelClear: m3.AssociatedFrameConfiguration.PanelClear,
                    Height: m3.AssociatedFrameConfiguration.Height,
                    StandardHeight: standardHeight,
                    Peralte: m3.AssociatedFrameConfiguration.PostPeralte,
                    Status: D.Status(window));
            }, design);

            Assert.True(r.Calculated);
            Assert.False(r.Manual);             // N-02: el reset de la configuracion tambien quita la marca de longitud manual
            Assert.Equal(50.0, r.Length, 4);    // sin cambiar la longitud
            Assert.NotEqual(D.Marker, r.PanelClear);
            Assert.Equal(r.StandardHeight, r.Height, 4);
            Assert.Equal(3.0, r.Peralte, 4);
            Assert.Equal("Configuración 'Calculada' aplicada al módulo.", r.Status);
        }

        [Fact]
        public void D26_GUARD_LaVentanaNoFijaLasBanderasDeLaReutilizacion_NiConservaLaRutaDelPreset()
        {
            var source = Source("RackDynamicSystemWindow.xaml.cs");

            // La unica escritura de IsManualOverride = true es la longitud editada a mano en «Aplicar» del modulo; la unica
            // de false es «Calculada». La procedencia personalizada la escribe Application.
            Assert.Single(Regex.Matches(source, @"IsManualOverride = true"));
            Assert.Contains("IsManualOverride = true", BodyOf(source, "private void ApplyModule_Click("));
            Assert.Single(Regex.Matches(source, @"IsManualOverride = false"));
            Assert.Contains("IsManualOverride = false", BodyOf(source, "private void ConfigBox_SelectionChanged("));
            Assert.Empty(Regex.Matches(source, @"UseCalculatedHeaderConfiguration = false"));

            foreach (var signature in new[]
                     {
                         "private void EditHeader_Click(",
                         "private void TakeHeaderSource_Click(",
                         "private void ApplyHeaderBatch_Click(",
                         "private void RunHeaderBatch(",
                     })
            {
                var body = BodyOf(source, signature);
                foreach (var write in new[] { "IsManualOverride", "UseCalculatedHeaderConfiguration =", "IsCalculated =", ".Length =", "AssociatedFrameConfiguration =" })
                {
                    Assert.DoesNotContain(write, body, StringComparison.Ordinal);
                }
            }
        }

        // ---- source helpers ----

        internal static string Source(string fileName)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizó la raíz del repositorio (RackCad.sln).");
            return File.ReadAllText(Path.Combine(dir.FullName, "src", "RackCad.UI", "Systems", "Dynamic", fileName));
        }

        /// <summary>The body of the member whose declaration starts with <paramref name="signature"/>, by brace matching.</summary>
        internal static string BodyOf(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(start >= 0, "No se encontró '" + signature + "' en la ventana del Dinámico.");
            var open = source.IndexOf('{', start);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}' && --depth == 0)
                {
                    return source.Substring(start, i - start + 1);
                }
            }

            return source.Substring(start);
        }
    }
}
