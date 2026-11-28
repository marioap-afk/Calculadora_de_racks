#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Systems.Selective;
using RackCad.UI.Views;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>T6 bom: the classic Selectivo editor shown off screen, and a click whose handler opens a modal dialog owned by it.</summary>
    internal static class SelectiveClassicShown
    {
        /// <summary>The classic editor opened as RACKEDITAR opens it, shown off screen so its dialogs have an owner.</summary>
        internal static RackSelectiveWindow OpenShown(SelectivePalletDesign design)
        {
            var authored = Authored(design);
            var register = ProjectVariablesReadResult.Absent();
            var open = SelectiveEditorOpen.Resolve(Store.Deserialize(authored), register);
            Assert.True(open.IsOpen, open.Error);
            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.SetProjectVariables(LinkedPropertyOptions.ForProperty(Clearance, register));
            window.LoadExisting(Store.Deserialize(authored), open.Design, open.LinkedPropertyStates);
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.ShowInTaskbar = false;
            window.ShowActivated = false;
            window.Left = -10000;
            window.Top = -10000;
            window.Show();
            return window;
        }

        /// <summary>
        /// Clicks <paramref name="button"/>, whose handler opens a modal dialog owned by the editor; <paramref name="onDialog"/> runs inside it
        /// (from the nested loop of its <c>ShowDialog</c>) and every dialog still open is closed after it, so the test never hangs.
        /// </summary>
        internal static void ClickWithDialog(RackSelectiveWindow window, string button, Action<Window> onDialog)
        {
            Exception? captured = null;
            var ran = false;
            var operation = window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                try
                {
                    foreach (var dialog in window.OwnedWindows.OfType<Window>().ToList())
                    {
                        ran = true;
                        onDialog(dialog);
                    }
                }
                catch (Exception ex) { captured = ex; }
                finally
                {
                    foreach (var dialog in window.OwnedWindows.OfType<Window>().ToList())
                    {
                        if (dialog.IsVisible) dialog.Close();
                    }
                }
            }));

            EditorWindowTestSupport.ClickByContent(window, button);
            if (operation.Status == DispatcherOperationStatus.Pending) operation.Abort();
            if (captured != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(captured).Throw();
            Assert.True(ran, "«" + button + "» no abrió ningún diálogo.");
        }
    }

    /// <summary>
    /// T6 bom (ARQ-43-10 in the classic editor): the readers of the drawing of a Selectivo (the BOM, the cuts, the planta) impose the depth of
    /// its fondo on every custom cabecera they read, in place (<see cref="SelectiveCabeceraAuthority.EffectiveCustomAt"/>, deferred on
    /// purpose and pinned by <c>SelectiveCabeceraViewPurityTests</c>). The classic editor (<see cref="RackSelectiveWindow"/>) resolved its
    /// system straight from its design, whose custom cabeceras are the objects of its state, so «Lista de materiales» and «Insertar varias…»
    /// changed what the editor writes. The window cut it in <c>SelectiveDraftEditor.Recompose</c>; the classic now gives its system copies of
    /// its own. The rack: two fondos, P1 of fondo 2 with a custom cabecera of 42 in while the rule of that fondo is 34 in (a document of before
    /// can carry it; the load only adjusts the ones of fondo 0).
    /// </summary>
    public sealed class SelectiveClassicCustomCabeceraReadTests
    {
        private static string Json(SelectivePalletDesign? design)
            => Store.Serialize(SelectivePalletDesignDocument.From(design!, RackId, "Selectivo A"));

        private static double StoredDepth(SelectivePalletDesign design) => design.ExtraFondoPostCabeceras[0][0]!.Depth;

        private static IReadOnlyList<string> Lines(IEnumerable<BomComponent> components)
            => components.Select(c => string.Join("|", c.Category, c.ProfileId, c.Description, c.Length.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
                    c.Quantity, string.Join(";", c.Pieces.Select(p => p.ProfileId + "x" + p.Quantity + "@" + p.Length.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)))))
                .OrderBy(line => line, StringComparer.Ordinal).ToList();

        [Fact]
        public void TheBomOfTheClassicCountsLikeTheWrittenDesignAndChangesNoCustomCabeceraOfIt()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveClassicShown.OpenShown(TwoFondosWithACustomCabeceraOutOfItsRule());
                try
                {
                    var design = window.BuildDesignForTest(out var error);
                    Assert.Null(error);
                    Assert.Equal(42.0, StoredDepth(design!));
                    var before = Json(design);

                    IReadOnlyList<BomComponent>? shown = null;
                    SelectiveClassicShown.ClickWithDialog(window, "Lista de materiales", dialog =>
                    {
                        var grid = (DataGrid)dialog.FindName("ComponentGrid");
                        shown = ((IEnumerable<BomComponent>)grid.ItemsSource).ToList();
                    });

                    // Reading the BOM changes nothing of what the classic writes: the custom cabecera keeps the 42 in it was stored with.
                    var after = window.BuildDesignForTest(out error);
                    Assert.Null(error);
                    Assert.Equal(42.0, StoredDepth(after!));
                    Assert.Equal(before, Json(after));

                    // And it counts what the drawing counts: the BOM of the written design, read and resolved again (the readers impose the
                    // rule there, on objects of their own).
                    var catalog = window.Session.Catalog;
                    var written = new SelectiveGeometryResolver().Resolve(Store.Deserialize(before)!.ToDomain(), catalog);
                    Assert.NotNull(shown);
                    Assert.Equal(Lines(SelectiveBomBuilder.Build(written, catalog).Components), Lines(shown!));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void InsertarVariasOfTheClassicChangesNothingCancelledAndSendsTheDesignTheUserSaw()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveClassicShown.OpenShown(TwoFondosWithACustomCabeceraOutOfItsRule());
                try
                {
                    var before = Json(window.BuildDesignForTest(out _));

                    // Opened and cancelled: its options read the cuts of the system, which impose on what they read.
                    SelectiveClassicShown.ClickWithDialog(window, "Insertar varias…", dialog => Assert.IsType<RackViewBatchDialog>(dialog));
                    Assert.False(window.InsertRequested);
                    Assert.Equal(before, Json(window.BuildDesignForTest(out _)));

                    // Accepted with every view: what goes to AutoCAD is the design the user saw, and its system shares no custom cabecera
                    // with it (the drawing imposes the rule on the system, never on the design that is written).
                    SelectiveClassicShown.ClickWithDialog(window, "Insertar varias…", dialog =>
                    {
                        var presenter = (RackViewBatchDialogPresenter)dialog.DataContext;
                        foreach (var option in presenter.Available.ToList()) presenter.Add(option);
                        EditorWindowTestSupport.ClickByContent(dialog, "Insertar cola");
                    });

                    Assert.True(window.InsertRequested);
                    var sent = window.DesignToInsert!;
                    Assert.Equal(42.0, StoredDepth(sent));
                    Assert.Equal(before, Json(sent));
                    var system = window.SystemToInsert!;
                    for (var k = 0; k < SelectiveDepthLayout.Count(system); k++)
                    {
                        for (var post = 0; post <= SelectiveDepthLayout.BaysOfFondo(system, k).Count; post++)
                        {
                            var stored = SelectiveCabeceraAuthority.CustomAt(sent, k, post);
                            if (stored != null) Assert.NotSame(stored, SelectiveCabeceraAuthority.CustomAt(system, k, post));
                        }
                    }

                    // The drawing still reads the rule: the effective cabecera of the system is 34 in.
                    Assert.Equal(34.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 0)!.Depth, 6);
                    Assert.Equal(42.0, StoredDepth(sent));
                }
                finally { if (window.IsVisible) window.Close(); }
            });
        }

        // ------------------------------------------------------------------ point 2 of the review of 9dd5baa5: the other paths of the classic

        private static string N(double value) => value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);

        private static string Instance(HeaderBlockInstance i)
            => string.Join("|", i.Role, i.PieceId, i.BlockName, i.View, N(i.Insertion.X), N(i.Insertion.Y), N(i.ConnectionAnchor.X), N(i.ConnectionAnchor.Y),
                N(i.RotationRadians), i.MirroredX, i.MirroredY, i.Text,
                string.Join(",", i.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Key + "=" + N(p.Value))));

        /// <summary>What the drawing reads of a system: every cut (its cabecera as the cut gives it, and its largueros), the planta and the frontal of
        /// every fondo.</summary>
        private static string Geometry(SelectiveRackSystem system, RackCatalog catalog)
        {
            var lines = new List<string>();
            var frames = new RackFrameProjectStore();
            foreach (var cut in new SelectiveLateralBuilder().Cortes(system, catalog))
            {
                lines.Add("corte " + cut.PostIndex + " " + N(cut.X) + " " + (cut.Cabecera == null ? "-" : frames.Serialize(cut.Cabecera))
                          + " " + string.Join(";", cut.Largueros.Select(Instance)));
            }

            lines.Add("planta " + string.Join(";", new SelectivePlantaBuilder().Build(system, catalog).Select(Instance)));
            for (var k = 0; k < SelectiveDepthLayout.Count(system); k++)
            {
                lines.Add("frontal " + k + " " + string.Join(";", new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, k), catalog).Select(Instance)));
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// What the classic hands to AutoCAD after <paramref name="button"/>: the design the user saw (42, its JSON as before), a system that
        /// shares no custom cabecera with it, and the same geometry as a system resolved again from the JSON that is written.
        /// </summary>
        private static void SendsTheDesignTheUserSawWithTheSameGeometry(string button)
        {
            var window = SelectiveClassicShown.OpenShown(TwoFondosWithACustomCabeceraOutOfItsRule());
            try
            {
                var before = Json(window.BuildDesignForTest(out _));
                EditorWindowTestSupport.ClickByContent(window, button);

                Assert.True(window.InsertRequested, "«" + button + "» no pidió dibujar.");
                var sent = window.DesignToInsert!;
                Assert.Equal(42.0, StoredDepth(sent));
                Assert.Equal(before, Json(sent));
                var system = window.SystemToInsert!;
                for (var k = 0; k < SelectiveDepthLayout.Count(system); k++)
                {
                    for (var post = 0; post <= SelectiveDepthLayout.BaysOfFondo(system, k).Count; post++)
                    {
                        var stored = SelectiveCabeceraAuthority.CustomAt(sent, k, post);
                        if (stored != null) Assert.NotSame(stored, SelectiveCabeceraAuthority.CustomAt(system, k, post));
                    }
                }

                var catalog = window.Session.Catalog;
                var written = new SelectiveGeometryResolver().Resolve(Store.Deserialize(before)!.ToDomain(), catalog);
                Assert.Equal(Geometry(written, catalog), Geometry(system, catalog));
                Assert.Equal(before, Json(sent)); // reading the geometry of the system did not reach the design
            }
            finally { if (window.IsVisible) window.Close(); }
        }

        [Fact]
        public void ActualizarOfTheClassicSendsTheDesignTheUserSawWithTheSameGeometry()
            => StaTestRunner.Run(() => SendsTheDesignTheUserSawWithTheSameGeometry("Actualizar"));

        [Fact]
        public void InsertarPlantaOfTheClassicSendsTheDesignTheUserSawWithTheSameGeometry()
            => StaTestRunner.Run(() => SendsTheDesignTheUserSawWithTheSameGeometry("Insertar planta"));

        [Fact]
        public void ElementosDeSeguridadOfTheClassicOpenedAndCancelledChangesNoCustomCabecera()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveClassicShown.OpenShown(TwoFondosWithACustomCabeceraOutOfItsRule());
                try
                {
                    var before = Json(window.BuildDesignForTest(out _));
                    SelectiveClassicShown.ClickWithDialog(window, "Elementos de seguridad…", dialog => Assert.IsType<RackCad.UI.SelectiveSafetyWindow>(dialog));
                    var after = window.BuildDesignForTest(out _)!;
                    Assert.Equal(42.0, StoredDepth(after));
                    Assert.Equal(before, Json(after));
                }
                finally { window.Close(); }
            });
        }
    }
}
