using System;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.Dynamic;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-50, T-18 (G3) — el editor del Dinámico (C-07: <c>ReadAnnotationOptions</c> y <c>RestoreFrom</c>) y sus tres
    /// casillas de cotas por TIPO de vista. Mismo contrato que T-17: un legacy (<c>null</c>) se muestra con las tres
    /// activas y se guarda <c>null</c> sin tocar; un valor presente pinta sus bits 1/2/4 y vuelve EXACTO; tocar = que el
    /// USUARIO cambie una casilla, nunca una carga, una recarga, un recálculo, el nivel ni el estilo.
    ///
    /// <para>
    /// «Guardar» es el diseño que arma el recálculo real de la ventana (<c>BuildDesignForTest</c>, el mismo
    /// <c>Recompose</c> que insertar y actualizar). <c>RestoreFrom</c> repetido nunca convierte <c>null</c> en 7 ni pierde
    /// bits desconocidos.
    /// </para>
    /// </summary>
    public sealed class DynamicDimensionViewsWindowTests
    {
        private const string PostId = "POSTE_OMEGA_3X3";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static CheckBox Box(Window window, string name)
            => window.FindName(name) as CheckBox
               ?? throw new InvalidOperationException($"No hay casilla «{name}» en {window.GetType().Name}.");

        private static CheckBox Frontal(Window window) => Box(window, "DimensionsFrontalCheck");

        private static CheckBox Lateral(Window window) => Box(window, "DimensionsLateralCheck");

        private static CheckBox Planta(Window window) => Box(window, "DimensionsPlantaCheck");

        private static string Shown(Window window)
            => $"F={Frontal(window).IsChecked} L={Lateral(window).IsChecked} P={Planta(window).IsChecked}";

        private static string On(bool frontal, bool lateral, bool planta)
            => $"F={frontal} L={lateral} P={planta}";

        private static int? AsInt(DimensionViewVisibility? value) => value.HasValue ? (int)value.Value : (int?)null;

        private static int? Saved(RackDynamicSystemWindow window)
        {
            var design = window.BuildDesignForTest(out var ok);
            Assert.True(ok && design != null, "el recálculo de la ventana tenía que armar un diseño");
            return AsInt(design.DimensionViews);
        }

        private static void Load(RackDynamicSystemWindow window, int? policy, DimensionDetail detail = DimensionDetail.Standard)
            => window.LoadExisting(Design(policy, detail), "GUID-DIN", "Dinámico A");

        /// <summary>Un sistema dinámico de dos frentes armado por el ensamblador productivo, con la política dada.</summary>
        internal static DynamicRackDesign Design(int? policy, DimensionDetail detail = DimensionDetail.Standard)
        {
            var catalog = Catalog;
            var builder = new DynamicRackSystemBuilder(catalog);
            var assembler = new DynamicEditorDesignAssembler(catalog, builder, new DynamicRackSystemResolver(catalog));
            var system = builder.BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"), 5, RackFrameTemplateCatalog.Default, PostId, 132.0, 3.0);
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);

            return assembler.BuildDesign(
                system, matrix,
                levels: 3, firstLevel: 6.0, beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                headerPostCatalogId: PostId, palletsDeep: 5, postPeralte: 3.0,
                palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                annotations: new DynamicAnnotationOptions
                {
                    Dimensions = detail,
                    DimensionViews = policy.HasValue ? (DimensionViewVisibility)policy.Value : (DimensionViewVisibility?)null
                },
                safetySelections: null);
        }

        [Fact]
        public void T18_ANewSystem_ShowsTheThreeViewsOn_AndSavesLegacyNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T18_ALegacySystem_LoadsWithTheThreeViewsOn_AndSavedUntouchedStaysNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, null);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        /// <summary>MIN-4: Planta apagada por el usuario antes de restaurar un legacy; <c>RestoreFrom</c> la enciende de verdad
        /// y ese Checked no toca, así que guardar sin más gestos sigue dando <c>null</c>.</summary>
        [Fact]
        public void T18_MIN4_ARealProgrammaticTransitionDuringRestoreFrom_DoesNotTouch_AndLegacyStaysNull()
        {
            var (before, checkedDuringLoad, shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Planta(window).IsChecked = false;
                var wasOff = Planta(window).IsChecked == false;

                var raised = 0;
                RoutedEventHandler count = (s, e) => raised++;
                Planta(window).Checked += count;
                Load(window, null);
                Planta(window).Checked -= count;

                return (wasOff, raised, Shown(window), Saved(window));
            });

            Assert.True(before, "la casilla Planta tenía que estar apagada antes de restaurar");
            Assert.True(checkedDuringLoad > 0, "RestoreFrom tenía que encender Planta de verdad (Checked)");
            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T18_ChangingOnlyTheDimensionsLevelOrStyle_KeepsLegacyNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, null);
                window.SetDimensionStyles(new[] { "ESTILO_A" });
                var levels = (ComboBox)window.FindName("DimensionsBox");
                levels.SelectedIndex = (int)DimensionDetail.Detailed;
                ((ComboBox)window.FindName("DimStyleBox")).SelectedItem = "ESTILO_A";
                levels.SelectedIndex = (int)DimensionDetail.None;
                levels.SelectedIndex = (int)DimensionDetail.Minimal;
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T18_TurningLateralOffOnALegacySystem_SavesAnExplicitFrontalPlanta()
        {
            var saved = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, null);
                Lateral(window).IsChecked = false;
                return Saved(window);
            });

            Assert.Equal((int)(DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta), saved);
        }

        [Fact]
        public void T18_13_ShowsFrontalAndPlanta_AndSavedUntouchedStays13()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, 13);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, false, true), shown);
            Assert.Equal(13, saved);
        }

        [Fact]
        public void T18_Minus8_ShowsTheThreeOff_AndSavedUntouchedStaysMinus8()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, -8);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(false, false, false), shown);
            Assert.Equal(-8, saved);
        }

        [Fact]
        public void T18_Minus8_PlusTheUserCheckingFrontal_SavesMinus7()
        {
            var saved = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, -8);
                Frontal(window).IsChecked = true;
                return Saved(window);
            });

            Assert.Equal(-7, saved);
        }

        /// <summary>
        /// <c>RestoreFrom</c> REPETIDO: restaurar dos veces el mismo legacy sigue guardando <c>null</c>; restaurar -8 dos veces,
        /// -8. Tras tocar Frontal sobre -8, restaurar 13 guarda 13, y restaurar un legacy —que enciende de verdad las tres
        /// casillas apagadas— vuelve a <c>null</c>.
        /// </summary>
        [Fact]
        public void T18_RepeatedRestoreFrom_NeverMaterializesNull_NorLosesUnknownBits_NorKeepsAnOldTouch()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, null);
                Load(window, null);
                var legacyTwice = Saved(window);

                Load(window, -8);
                Load(window, -8);
                var minus8Twice = Saved(window);

                Frontal(window).IsChecked = true;
                Load(window, 13);
                var shown13 = Shown(window);
                var saved13 = Saved(window);

                Load(window, -8);
                Load(window, null);
                return (LegacyTwice: legacyTwice, Minus8Twice: minus8Twice, Shown13: shown13, Saved13: saved13,
                    ShownLegacy: Shown(window), SavedLegacy: Saved(window));
            });

            Assert.Null(r.LegacyTwice);
            Assert.Equal(-8, r.Minus8Twice);
            Assert.Equal(On(true, false, true), r.Shown13);
            Assert.Equal(13, r.Saved13);
            Assert.Equal(On(true, true, true), r.ShownLegacy);
            Assert.Null(r.SavedLegacy);
        }

        [Fact]
        public void T18_TheNoneLevel_NeverChangesTheBoxesThePolicyOrTouched()
        {
            var (shownNone, savedNone, shownBack, savedBack) = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(window, 13);
                var levels = (ComboBox)window.FindName("DimensionsBox");
                levels.SelectedIndex = (int)DimensionDetail.None;
                var none = (Shown(window), Saved(window));
                levels.SelectedIndex = (int)DimensionDetail.Standard;
                return (none.Item1, none.Item2, Shown(window), Saved(window));
            });

            Assert.Equal(On(true, false, true), shownNone);
            Assert.Equal(13, savedNone);
            Assert.Equal(On(true, false, true), shownBack);
            Assert.Equal(13, savedBack);
        }

        [Fact]
        public void T18_TheUpdatePayload_CarriesTheExactPolicy_AndTheUsersChoice()
        {
            var (design13, system13, designEdited, systemEdited) = StaTestRunner.Run(() =>
            {
                var untouched = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(untouched, 13);
                EditorWindowTestSupport.ClickNamed(untouched, "UpdateButton");

                var edited = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                Load(edited, null);
                Lateral(edited).IsChecked = false;
                EditorWindowTestSupport.ClickNamed(edited, "UpdateButton");

                return (AsInt(untouched.DesignToInsert?.DimensionViews), AsInt(untouched.SystemToInsert?.DimensionViews),
                    AsInt(edited.DesignToInsert?.DimensionViews), AsInt(edited.SystemToInsert?.DimensionViews));
            });

            Assert.Equal(13, design13);
            Assert.Equal(13, system13);
            Assert.Equal(5, designEdited);
            Assert.Equal(5, systemEdited);
        }
    }
}
