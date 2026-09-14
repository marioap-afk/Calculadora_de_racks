using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.UI.Systems.Dynamic;
using Xunit;
using D = RackCad.UI.Tests.DynamicHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D, G7 — lo que sobrevive a la ventana: RACKEDITAR (D-35), Actualizar y las tres vistas (D-36), guardar y reabrir
    /// (D-37) y el aspecto de un rack sin personalizaciones (D-38). Proposal V2 §7.13: las copias caen en
    /// <c>Modules[].Header</c> + <c>UseCalculatedHeaderConfiguration</c>, ya persistidos; sin DTO ni campo nuevo.
    /// <para>
    /// Las firmas de dibujo son las de I-24 (cada instancia de cada corte lateral, frontal de salida y de entrada, y planta,
    /// anotaciones incluidas). No hay goldens: cada prueba compara dos caminos productivos entre si.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderBatchDrawingTests
    {
        private sealed class Payload
        {
            public DynamicRackDesign Design { get; set; }
            public DynamicRackSystem System { get; set; }
        }

        /// <summary>«Actualizar» del editor abierto desde el dibujo: el payload real (Recompose → SetModel → sesión).</summary>
        private static Payload Update(RackDynamicSystemWindow window)
        {
            D.Click(window, "UpdateButton");
            Assert.True(window.InsertRequested, "«Actualizar» tenia que pedir el redibujo en sitio");
            return new Payload { Design = window.DesignToInsert, System = window.SystemToInsert };
        }

        private sealed class ModuleView
        {
            public string Id { get; set; }
            public DynamicRackModuleKind Kind { get; set; }
            public bool Calculated { get; set; }
            public double Length { get; set; }
            public bool Manual { get; set; }
            public string Recipe { get; set; }
            public object Configuration { get; set; }
        }

        private static List<ModuleView> View(IEnumerable<DynamicRackModule> modules)
            => modules.OrderBy(module => module.Index).Select(module => new ModuleView
            {
                Id = module.ModuleId,
                Kind = module.Kind,
                Calculated = module.UseCalculatedHeaderConfiguration,
                Length = module.Length,
                Manual = module.IsManualOverride,
                Recipe = module.IsHeader && !module.UseCalculatedHeaderConfiguration ? D.Recipe(module.AssociatedFrameConfiguration) : null,
                Configuration = module.AssociatedFrameConfiguration
            }).ToList();

        private static string Describe(IEnumerable<ModuleView> modules)
            => string.Join(" ", modules.Select(module => module.Id + ":" + module.Kind + ":" + (module.Calculated ? "calc" : "pers") + ":" + module.Length.ToString("R") + ":" + module.Manual));

        // =============================================================================================================
        // D-35 — RACKEDITAR con cabeceras distribuidas
        // =============================================================================================================

        [Fact]
        public void D35_RACKEDITAR_ConCabecerasDistribuidas_MismosIdsProcedenciaYConfiguracionesIndependientes()
        {
            var r = StaTestRunner.Run(() =>
            {
                var editor = D.Open(D.WithCustom(D.Design(), "M1"));
                D.Select(editor, "M1");
                D.TakeSource(editor);
                D.Targets(editor, "M3");
                D.Apply(editor);
                var outcome = editor.LastHeaderBatchOutcome;
                var drawn = Update(editor); // cierra el editor, como en AutoCAD

                // RACKEDITAR: lo que el bloque guarda (el diseño serializado) se lee y se abre en un editor nuevo.
                var embedded = D.RoundTrip(drawn.Design);
                var reopened = D.Open(embedded.DynamicDesign);
                try
                {
                    return (
                        Outcome: outcome,
                        Drawn: View(drawn.System.Modules),
                        Reopened: View(D.Modules(reopened)),
                        SourceRecipe: D.Recipe(drawn.System.Modules.Single(module => module.ModuleId == "M1").AssociatedFrameConfiguration));
                }
                finally
                {
                    reopened.Close();
                }
            });

            var committed = Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(new[] { "M3" }, committed.Applied.Select(address => address.ModuleId));

            // Mismos ids, tipos, procedencia y longitudes: nada se reinventa al reabrir.
            Assert.Equal(Describe(r.Drawn), Describe(r.Reopened));
            var reopened = r.Reopened.ToDictionary(module => module.Id);
            Assert.False(reopened["M1"].Calculated);
            Assert.False(reopened["M3"].Calculated);
            Assert.True(reopened["M5"].Calculated);            // ningun default inventado: la calculada sigue calculada
            Assert.Equal(r.SourceRecipe, reopened["M1"].Recipe);
            Assert.Equal(r.SourceRecipe, reopened["M3"].Recipe); // la personalizacion distribuida sobrevive identica
            Assert.False(ReferenceEquals(reopened["M1"].Configuration, reopened["M3"].Configuration)); // sin copia compartida
        }

        // =============================================================================================================
        // D-36 — Actualizar, lateral, frontal y planta con cabeceras personalizadas (caracterizacion verde + E2E)
        // =============================================================================================================

        [Fact]
        public void D36_ConCabecerasPersonalizadas_Actualizar_YLasTresVistas_CorrespondenYReflejanLaPersonalizacion()
        {
            var r = StaTestRunner.Run(() =>
            {
                var legacyEditor = D.Open(D.Design());
                var legacy = Update(legacyEditor);

                var customEditor = D.Open(D.WithCustom(D.WithCustom(D.Design(), "M1", 36.0), "M3", 36.0));
                var custom = Update(customEditor);

                // Y otra vez desde el dibujo: RACKEDITAR + Actualizar.
                var again = D.Open(D.RoundTrip(custom.Design).DynamicDesign);
                var redrawn = Update(again);
                return (
                    LegacyLateral: D.ViewSignature(legacy.System, "lateral"),
                    CustomLateral: D.ViewSignature(custom.System, "lateral"),
                    Corresponds: D.Corresponds(custom.Design, custom.System),
                    CustomFull: D.FullDrawingSignature(custom.System),
                    RedrawnFull: D.FullDrawingSignature(redrawn.System),
                    RedrawnCorresponds: D.Corresponds(redrawn.Design, redrawn.System),
                    Custom: custom.System.Modules.Where(module => module.IsHeader).Select(module => module.ModuleId + ":" + module.UseCalculatedHeaderConfiguration).ToArray());
            });

            Assert.Equal(new[] { "M1:False", "M3:False", "M5:True" }, r.Custom);
            Assert.True(r.Corresponds, "el diseño del payload y su sistema dibujan exactamente lo mismo en las tres vistas");
            Assert.NotEqual(r.LegacyLateral, r.CustomLateral); // la personalizacion llega al corte lateral
            Assert.True(r.RedrawnCorresponds);
            Assert.Equal(r.CustomFull, r.RedrawnFull);           // y sobrevive a RACKEDITAR + Actualizar sin moverse
        }

        // =============================================================================================================
        // D-37 — guardar y reabrir un caso distribuido (los historicos de Modules[].Header siguen en RackProjectStoreTests)
        // =============================================================================================================

        [Fact]
        public void D37_DistribuirGuardarYReabrir_ConservaConfiguracionesIndependientes_SinCambiarElFormato()
        {
            var path = Path.Combine(Path.GetTempPath(), "rackcad-i53d-d37-" + Guid.NewGuid().ToString("N") + RackProjectStore.FileExtension);
            try
            {
                var r = StaTestRunner.Run(() =>
                {
                    var editor = D.Open(D.WithCustom(D.Design(), "M1"));
                    D.Select(editor, "M1");
                    D.TakeSource(editor);
                    D.TargetsAll(editor);
                    D.Apply(editor);
                    var outcome = editor.LastHeaderBatchOutcome;
                    var saved = editor.BuildDesignForTest(out var ok); // el mismo Recompose que «Guardar proyecto»
                    var sourceRecipe = D.Recipe(D.Module(editor, "M1").AssociatedFrameConfiguration);
                    editor.Close();

                    new RackProjectStore().Save(RackProject.ForDynamic(saved), path);
                    var loaded = new RackProjectStore().Load(path);
                    var file = loaded.DynamicDesign.Modules
                        .Where(module => module.IsHeader)
                        .Select(module => module.ModuleId + ":" + module.UseCalculatedHeaderConfiguration + ":" + (module.HeaderConfiguration != null))
                        .ToArray();

                    var reopened = D.Open(loaded.DynamicDesign);
                    try
                    {
                        var headers = View(D.Modules(reopened).Where(module => module.IsHeader));
                        return (Ok: ok, Outcome: outcome, File: file, Headers: headers, SourceRecipe: sourceRecipe);
                    }
                    finally
                    {
                        reopened.Close();
                    }
                });

                Assert.True(r.Ok);
                Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
                Assert.Equal(new[] { "M1:False:True", "M3:False:True", "M5:False:True" }, r.File); // Modules[].Header + procedencia
                Assert.All(r.Headers, header => Assert.False(header.Calculated));
                Assert.All(r.Headers, header => Assert.Equal(r.SourceRecipe, header.Recipe));
                Assert.Equal(r.Headers.Count, r.Headers.Select(header => header.Configuration).Distinct(ReferenceEqualityComparer.Instance).Count());
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void D37_TrasDistribuir_ElBomDelRackEsElDeLaRutaDeRACKBOMTOTAL_YCotizaLasCopias()
        {
            var r = StaTestRunner.Run(() =>
            {
                var legacyEditor = D.Open(D.WithCustom(D.Design(), "M1"));
                var withoutDistribution = Update(legacyEditor);

                var editor = D.Open(D.WithCustom(D.Design(), "M1"));
                D.Select(editor, "M1");
                D.TakeSource(editor);
                D.TargetsAll(editor);
                D.Apply(editor);
                var outcome = editor.LastHeaderBatchOutcome;
                var distributed = Update(editor);

                // RACKBOMTOTAL: lo que el dibujo guarda, leido por el store y resuelto por el resolver del Dinamico.
                var embedded = D.RoundTrip(distributed.Design).DynamicDesign;
                var bomTotal = new DynamicRackSystemResolver(D.Catalog).Resolve(embedded).System;
                return (
                    Outcome: outcome,
                    Without: BomSignature(withoutDistribution.System),
                    Editor: BomSignature(distributed.System),
                    Total: BomSignature(bomTotal),
                    MembersPresent: distributed.System.Modules.Where(module => module.IsHeader)
                        .All(module => module.AssociatedFrameConfiguration.Members.Count > 0));
            });

            Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.True(r.MembersPresent);
            Assert.NotEqual(r.Without, r.Editor); // las copias distribuidas se cotizan
            Assert.Equal(r.Editor, r.Total);      // y el BOM del rack es el mismo por la ruta de RACKBOMTOTAL
        }

        private static string BomSignature(DynamicRackSystem system)
            => string.Join("|", SystemBomBuilder.Build(system, D.Catalog).Lines
                .Select(line => line.Category + ":" + line.ProfileId + ":" + line.Description + ":"
                                + line.Length.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ":" + line.Quantity)
                .OrderBy(text => text, StringComparer.Ordinal));

        // =============================================================================================================
        // D-38 — legacy: un rack sin personalizaciones dibuja lo mismo por cualquier ruta de reconstruccion
        // =============================================================================================================

        [Theory]
        [InlineData("sin-cambios")]
        [InlineData("tarima")]
        [InlineData("fondos")]
        [InlineData("restaurar")]
        public void D38_RackSinPersonalizaciones_DibujaIgualQueElResolverSinModulosExplicitos(string route)
        {
            var r = StaTestRunner.Run(() =>
            {
                var editor = D.Open(D.Design());
                switch (route)
                {
                    case "tarima":
                        D.ChangePalletDepth(editor, 52.0);
                        break;
                    case "fondos":
                        D.ChangeFondosOfAllFronts(editor, 7);
                        break;
                    case "restaurar":
                        D.RestoreLayout(editor);
                        break;
                }

                var report = D.RebuildReport(editor);
                var drawn = Update(editor);

                // La autoridad independiente de la ruta de la ventana: el MISMO diseño sin modulos explicitos, que el resolver
                // construye con su propio layout por defecto (DynamicRackSystemResolver.Resolve).
                var oracleDesign = D.RoundTrip(drawn.Design).DynamicDesign;
                oracleDesign.Modules.Clear();
                var oracle = new DynamicRackSystemResolver(D.Catalog).Resolve(oracleDesign).System;
                oracle.Name = drawn.System.Name;
                return (
                    Report: report,
                    AllCalculated: drawn.System.Modules.Where(module => module.IsHeader).All(module => module.UseCalculatedHeaderConfiguration),
                    NoManual: drawn.System.Modules.All(module => !module.IsManualOverride),
                    Drawn: D.FullDrawingSignature(drawn.System),
                    Oracle: D.FullDrawingSignature(oracle),
                    Corresponds: D.Corresponds(drawn.Design, drawn.System));
            });

            Assert.Equal(string.Empty, r.Report);
            Assert.True(r.AllCalculated);
            Assert.True(r.NoManual);
            Assert.True(r.Corresponds);
            Assert.Equal(r.Oracle, r.Drawn);
        }
    }
}
