using System;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-50, T-17 (G3) — el editor del Selectivo (C-01) y sus tres casillas de cotas por TIPO de vista.
    ///
    /// <para>
    /// Contrato de ADR-0035 y de la Proposal V1.2 (P-10, P-11): un rack legacy (<c>DimensionViews = null</c>) se
    /// muestra con las tres casillas activas y, si el usuario no las toca, se guarda <c>null</c>, nunca 7. Un valor
    /// presente pinta cada casilla con su bit (1, 2, 4), no muestra los bits desconocidos y se guarda EXACTO sin
    /// tocar. Tocar = que el USUARIO cambie una de las tres casillas; cargar, recargar y recalcular no tocan, aunque
    /// cambien programáticamente una casilla (MIN-4), y cambiar el nivel o el estilo tampoco. El valor guardado lo
    /// decide <c>DimensionViewPolicy.FromEditor</c>; la ventana no hace aritmética de bits.
    /// </para>
    /// <para>
    /// «El usuario cambia una casilla» es asignar su <c>IsChecked</c> fuera de una carga, que dispara los mismos
    /// Checked/Unchecked que un clic (patrón de <see cref="EditorWindowTestSupport.SetFrontBlank"/>).
    /// </para>
    /// </summary>
    public sealed class SelectiveDimensionViewsWindowTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

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

        /// <summary>Lo que guardaría la ventana ahora: el mismo <c>BuildDesign</c> que usan insertar y actualizar.</summary>
        private static int? Saved(RackSelectiveWindow window)
        {
            var design = window.BuildDesignForTest(out var error);
            Assert.True(design != null, error);
            return AsInt(design.DimensionViews);
        }

        private static void Load(RackSelectiveWindow window, int? policy, DimensionDetail detail = DimensionDetail.Standard)
            => window.LoadExisting(SelectivePalletDesignDocument.From(Design(policy, detail), "GUID-SEL", "Selectivo A"));

        private static SelectivePalletDesign Design(int? policy, DimensionDetail detail)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
                Dimensions = detail,
                DimensionViews = policy.HasValue ? (DimensionViewVisibility)policy.Value : (DimensionViewVisibility?)null
            };

            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            design.Bays.Add(bay);
            return design;
        }

        [Fact]
        public void T17_ANewRack_ShowsTheThreeViewsOn_AndSavesLegacyNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T17_ALegacyRack_LoadsWithTheThreeViewsOn_AndSavedUntouchedStaysNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(window, null);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        /// <summary>
        /// MIN-4: una transición REAL durante la carga. La casilla Lateral está apagada (y la ventana, tocada) antes de
        /// cargar un legacy; la carga la enciende programáticamente —su Checked se dispara de verdad— y ese evento no
        /// toca. Guardar sin ningún gesto posterior sigue dando <c>null</c>.
        /// </summary>
        [Fact]
        public void T17_MIN4_ARealProgrammaticTransitionDuringLoad_DoesNotTouch_AndLegacyStaysNull()
        {
            var (before, checkedDuringLoad, shown, saved) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Lateral(window).IsChecked = false;
                var wasOff = Lateral(window).IsChecked == false;

                var raised = 0;
                RoutedEventHandler count = (s, e) => raised++;
                Lateral(window).Checked += count;
                Load(window, null);
                Lateral(window).Checked -= count;

                return (wasOff, raised, Shown(window), Saved(window));
            });

            Assert.True(before, "la casilla Lateral tenía que estar apagada antes de cargar");
            Assert.True(checkedDuringLoad > 0, "la carga tenía que encender Lateral de verdad (Checked)");
            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T17_ChangingOnlyTheDimensionsLevelOrStyle_KeepsLegacyNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
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
        public void T17_TurningLateralOffOnALegacyRack_SavesAnExplicitFrontalPlanta()
        {
            var saved = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(window, null);
                Lateral(window).IsChecked = false;
                return Saved(window);
            });

            Assert.Equal((int)(DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta), saved);
        }

        [Fact]
        public void T17_13_ShowsFrontalAndPlanta_AndSavedUntouchedStays13()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(window, 13);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, false, true), shown);
            Assert.Equal(13, saved);
        }

        [Fact]
        public void T17_Minus8_ShowsTheThreeOff_AndSavedUntouchedStaysMinus8()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(window, -8);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(false, false, false), shown);
            Assert.Equal(-8, saved);
        }

        [Fact]
        public void T17_Minus8_PlusTheUserCheckingFrontal_SavesMinus7()
        {
            var saved = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(window, -8);
                Frontal(window).IsChecked = true;
                return Saved(window);
            });

            Assert.Equal(-7, saved);
        }

        /// <summary>
        /// Una recarga explícita REEMPLAZA la política y olvida el gesto anterior: tras tocar Frontal sobre -8, recargar
        /// 13 guarda 13 (no -7 ni 5); y recargar un legacy —que enciende programáticamente las tres casillas apagadas—
        /// vuelve a guardar <c>null</c>.
        /// </summary>
        [Fact]
        public void T17_AnExplicitReload_ReplacesThePolicy_AndDoesNotContaminateTouched()
        {
            var (shown13, saved13, shownLegacy, savedLegacy) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(window, -8);
                Frontal(window).IsChecked = true;

                Load(window, 13);
                var s13 = (Shown(window), Saved(window));

                Load(window, -8);
                Load(window, null);
                return (s13.Item1, s13.Item2, Shown(window), Saved(window));
            });

            Assert.Equal(On(true, false, true), shown13);
            Assert.Equal(13, saved13);
            Assert.Equal(On(true, true, true), shownLegacy);
            Assert.Null(savedLegacy);
        }

        /// <summary>«Ninguna» sigue ganando al dibujar, pero elegirla no cambia las casillas, ni la política, ni «tocado».</summary>
        [Fact]
        public void T17_TheNoneLevel_NeverChangesTheBoxesThePolicyOrTouched()
        {
            var (shownNone, savedNone, shownBack, savedBack) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
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

        /// <summary>Por el camino real de «Actualizar»: el diseño y el sistema del payload llevan la política de la ventana.</summary>
        [Fact]
        public void T17_TheUpdatePayload_CarriesTheExactPolicy_AndTheUsersChoice()
        {
            var (design13, system13, designEdited, systemEdited) = StaTestRunner.Run(() =>
            {
                var untouched = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                Load(untouched, 13);
                EditorWindowTestSupport.ClickNamed(untouched, "UpdateButton");

                var edited = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
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
