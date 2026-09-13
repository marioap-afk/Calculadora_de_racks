using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-50, T-19 (G3) — el editor de Push Back (C-12: <c>LoadFromModel</c> y <c>ReadInputs</c>) y sus tres casillas de cotas
    /// por TIPO de vista. Mismo contrato que T-17 y T-18, con lo propio de Push Back: <c>LoadFromModel</c> recalcula en el
    /// acto, así que el diseño de ese recálculo inmediato ya tiene que llevar la política cargada EXACTA; y un rack
    /// compuesto A/B la conserva a través de la estructura del lado A (C-15) y del ensamblador compuesto.
    ///
    /// <para>
    /// «Guardar» es el diseño de la última computación válida de la ventana, la que sale del recálculo real. H1 queda
    /// caracterizado, no corregido: el editor de Push Back sigue dejando <c>DimensionStyle = null</c> en su diseño.
    /// </para>
    /// </summary>
    public sealed class PushBackDimensionViewsWindowTests
    {
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

        private static DimensionViewVisibility? Policy(int? value)
            => value.HasValue ? (DimensionViewVisibility)value.Value : (DimensionViewVisibility?)null;

        /// <summary>Lo que guardaría la ventana ahora: el diseño de su última computación válida.</summary>
        private static int? Saved(RackPushBackSystemWindow window)
        {
            Assert.True(window.LastComputation != null && window.LastComputation.IsValid, "la ventana tenía que tener un sistema válido");
            return AsInt(window.LastComputation.Design.Structure.DimensionViews);
        }

        private static void Load(RackPushBackSystemWindow window, int? policy)
            => window.LoadExisting(SingleSided(policy), "GUID-PB", "PB A");

        /// <summary>Un sentido: dos frentes de fondo e inicio distintos, cotas Estándar y la política dada.</summary>
        private static PushBackDesign SingleSided(int? policy, string dimensionStyle = null)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0,
                    Dimensions = DimensionDetail.Standard,
                    DimensionStyle = dimensionStyle,
                    DimensionViews = Policy(policy)
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 });
            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            var f1 = new PushBackFrontConfig();
            f1.HighEndBeamPeraltes.Add(4.5);
            f1.HighEndBeamPeraltes.Add(4.5);
            design.Fronts.Add(f1);
            return design;
        }

        /// <summary>Compuesto encontrado A/B: dos ranuras por lado, A con 3 niveles y fondo 5, B con 2 niveles y fondo 4.</summary>
        private static PushBackDesign Composite(int? policy)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 3,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0,
                    Dimensions = DimensionDetail.Standard,
                    DimensionViews = Policy(policy)
                },
                SideB = new PushBackSideDesign { IsPresent = true, LoadLevels = 2, FirstLevelHeight = 4.0 },
                Composite = new PushBackCompositeDesign { Gap = 0.0, DefaultTopology = PushBackCellTopology.Encontradas }
            };

            for (var slot = 0; slot < 2; slot++)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 5, DepthStartPosition = 1 });
                design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 5 });
                design.SideB.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
                design.SideB.FrontConfigs.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            }

            return design;
        }

        // ---- T-20: la superficie existe y vive en el panel lateral ----------------------------------------------------

        [Fact]
        public void T20_ThePushBackWindow_NamesTheThreeBoxes_InItsSidebar()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var shell = (RackEditorVisualShell)window.Content;
                foreach (var name in new[] { "DimensionsFrontalCheck", "DimensionsLateralCheck", "DimensionsPlantaCheck" })
                {
                    var box = Box(window, name);
                    Assert.True(
                        EditorWindowTestSupport.Descendants((DependencyObject)shell.SidePanelContent).Contains(box),
                        name + " tiene que vivir en el panel lateral (SidePanelContent)");
                }
            });
        }

        // ---- T-19 ------------------------------------------------------------------------------------------------------

        [Fact]
        public void T19_ANewRack_ShowsTheThreeViewsOn_AndItsFirstRecomputeSavesLegacyNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        /// <summary>La carga recalcula en el acto: el diseño de ese recálculo inmediato ya lleva la política EXACTA, sin gesto.</summary>
        [Theory]
        [InlineData(null, true, true, true)]
        [InlineData(13, true, false, true)]
        [InlineData(-8, false, false, false)]
        [InlineData(2, false, true, false)]
        public void T19_TheImmediateRecomputeOfTheLoad_CarriesThePolicyExact_AndTheBoxesShowItsBits(int? policy, bool frontal, bool lateral, bool planta)
        {
            var (passes, shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var before = window.RecomputePassesForTest;
                Load(window, policy);
                return (window.RecomputePassesForTest - before, Shown(window), Saved(window));
            });

            Assert.True(passes > 0, "LoadFromModel tenía que recalcular");
            Assert.Equal(On(frontal, lateral, planta), shown);
            Assert.Equal(policy, saved);
        }

        /// <summary>MIN-4: Lateral apagada por el usuario (y recalculada) antes de cargar un legacy; la carga la enciende de
        /// verdad, recalcula en el acto y el diseño resultante sigue siendo legacy <c>null</c>.</summary>
        [Fact]
        public void T19_MIN4_ARealProgrammaticTransitionDuringLoadFromModel_DoesNotTouch_AndLegacyStaysNull()
        {
            var (userSaved, before, checkedDuringLoad, shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Lateral(window).IsChecked = false;
                var afterGesture = Saved(window);
                var wasOff = Lateral(window).IsChecked == false;

                var raised = 0;
                RoutedEventHandler count = (s, e) => raised++;
                Lateral(window).Checked += count;
                Load(window, null);
                Lateral(window).Checked -= count;

                return (afterGesture, wasOff, raised, Shown(window), Saved(window));
            });

            Assert.Equal((int)(DimensionViewVisibility.Frontal | DimensionViewVisibility.Planta), userSaved);
            Assert.True(before, "la casilla Lateral tenía que estar apagada antes de cargar");
            Assert.True(checkedDuringLoad > 0, "la carga tenía que encender Lateral de verdad (Checked)");
            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T19_ChangingOnlyTheDimensionsLevel_KeepsLegacyNull()
        {
            var (shown, saved) = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Load(window, null);
                var levels = (ComboBox)window.FindName("DimensionsBox");
                levels.SelectedIndex = (int)DimensionDetail.Detailed;
                levels.SelectedIndex = (int)DimensionDetail.None;
                levels.SelectedIndex = (int)DimensionDetail.Minimal;
                return (Shown(window), Saved(window));
            });

            Assert.Equal(On(true, true, true), shown);
            Assert.Null(saved);
        }

        [Fact]
        public void T19_UserGestures_KeepUnknownAndSignBits()
        {
            var (lateralOff, minus8PlusFrontal, thirteenMinusPlanta) = StaTestRunner.Run(() =>
            {
                var legacy = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Load(legacy, null);
                Lateral(legacy).IsChecked = false;

                var minus8 = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Load(minus8, -8);
                Frontal(minus8).IsChecked = true;

                var thirteen = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Load(thirteen, 13);
                Planta(thirteen).IsChecked = false;

                return (Saved(legacy), Saved(minus8), Saved(thirteen));
            });

            Assert.Equal(5, lateralOff);          // legacy + apagar Lateral = F|P explícito
            Assert.Equal(-7, minus8PlusFrontal);  // -8 + Frontal conserva los bits de signo
            Assert.Equal(9, thirteenMinusPlanta); // 13 sin Planta conserva el bit desconocido 8
        }

        /// <summary>«Restaurar» recarga desde el último sistema válido: adopta su política y olvida el gesto, sin materializar.</summary>
        [Fact]
        public void T19_RestoreAndReload_ReplaceThePolicy_AndDoNotContaminateTouched()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Load(window, -8);
                Frontal(window).IsChecked = true;                           // -7, válido
                EditorWindowTestSupport.ClickNamed(window, "RestoreButton"); // restaura el último válido: -7
                var restored = (Shown: Shown(window), Saved: Saved(window));

                Load(window, 13);
                var reloaded = (Shown: Shown(window), Saved: Saved(window));

                Load(window, -8);
                Load(window, null);
                return (Restored: restored, Reloaded: reloaded, LegacyShown: Shown(window), LegacySaved: Saved(window));
            });

            Assert.Equal(On(true, false, false), r.Restored.Shown);
            Assert.Equal(-7, r.Restored.Saved);
            Assert.Equal(On(true, false, true), r.Reloaded.Shown);
            Assert.Equal(13, r.Reloaded.Saved);
            Assert.Equal(On(true, true, true), r.LegacyShown);
            Assert.Null(r.LegacySaved);
        }

        [Fact]
        public void T19_ACompositeRack_KeepsThePolicyExact_OnTheSharedStructureAndBothSides()
        {
            var r = StaTestRunner.Run(() =>
            {
                var untouched = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                untouched.LoadExisting(Composite(13), "GUID-PB-AB", "PB AB");
                var system = untouched.LastComputation?.System;

                var edited = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                edited.LoadExisting(Composite(-8), "GUID-PB-AB2", "PB AB");
                Frontal(edited).IsChecked = true;

                return (Shown: Shown(untouched), Saved: Saved(untouched),
                    Composite: system?.IsComposite == true,
                    SideA: AsInt(system?.Composite?.SideA?.Local?.Structure?.DimensionViews),
                    SideB: AsInt(system?.Composite?.SideB?.Local?.Structure?.DimensionViews),
                    EditedSaved: Saved(edited));
            });

            Assert.True(r.Composite, "el rack cargado tenía que resolverse compuesto");
            Assert.Equal(On(true, false, true), r.Shown);
            Assert.Equal(13, r.Saved);
            Assert.Equal(13, r.SideA);
            Assert.Equal(13, r.SideB);
            Assert.Equal(-7, r.EditedSaved);
        }

        /// <summary>H1, caracterizado y NO corregido: el editor de Push Back no tiene control de estilo, y su diseño sale con
        /// <c>DimensionStyle = null</c> aunque el documento cargado trajera uno, antes y después de tocar las casillas.</summary>
        [Fact]
        public void T19_H1_TheDimensionStyleStillComesOutNull()
        {
            var (afterLoad, afterGesture) = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(SingleSided(null, dimensionStyle: "ESTILO_PB"), "GUID-PB-H1", "PB H1");
                var loaded = window.LastComputation?.Design?.Structure?.DimensionStyle;
                Lateral(window).IsChecked = false;
                return (loaded, window.LastComputation?.Design?.Structure?.DimensionStyle);
            });

            Assert.Null(afterLoad);
            Assert.Null(afterGesture);
        }

        // ---- T-21 equivalente: la política elegida llega al dibujo por «Actualizar» ------------------------------------

        [Fact]
        public void T21_Update_ThePolicyTheUserChose_ReachesEveryPushBackViewByType()
        {
            var outcomes = StaTestRunner.Run(() => new[]
            {
                UpdateWith("legacy"),
                UpdateWith("F", "DimensionsLateralCheck", "DimensionsPlantaCheck"),
                UpdateWith("L", "DimensionsFrontalCheck", "DimensionsPlantaCheck"),
                UpdateWith("F|P", "DimensionsLateralCheck")
            }.ToDictionary(outcome => outcome.Case));

            var legacy = outcomes["legacy"];
            Assert.Null(legacy.Policy);
            Assert.True(legacy.Frontal.Dimensions > 0 && legacy.Lateral.Dimensions > 0 && legacy.Planta.Dimensions > 0,
                "el rack legacy dibuja cotas en sus tres tipos de vista");

            foreach (var (key, policy, frontal, lateral, planta) in new[]
                     {
                         ("F", 1, true, false, false),
                         ("L", 2, false, true, false),
                         ("F|P", 5, true, false, true)
                     })
            {
                var outcome = outcomes[key];
                Assert.Equal(policy, outcome.Policy);
                Assert.Equal(policy, outcome.SystemPolicy);
                AssertView(key + " frontal", frontal, legacy.Frontal, outcome.Frontal);
                AssertView(key + " lateral", lateral, legacy.Lateral, outcome.Lateral);
                AssertView(key + " planta", planta, legacy.Planta, outcome.Planta);
            }
        }

        private static void AssertView(string what, bool on, (int Dimensions, string Signature) legacy, (int Dimensions, string Signature) actual)
        {
            if (on)
            {
                Assert.True(legacy.Signature == actual.Signature, what + ": un tipo encendido tiene que dibujar exactamente sus vistas legacy");
            }
            else
            {
                Assert.True(actual.Dimensions == 0, $"{what}: un tipo apagado dibujó {actual.Dimensions} cota(s)");
            }
        }

        private static (string Case, int? Policy, int? SystemPolicy, (int Dimensions, string Signature) Frontal, (int Dimensions, string Signature) Lateral, (int Dimensions, string Signature) Planta)
            UpdateWith(string key, params string[] untick)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Load(window, null);
            foreach (var name in untick)
            {
                Box(window, name).IsChecked = false;
            }

            EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
            var system = window.SystemToInsert;
            var catalog = Catalog;
            var frontal = new PushBackSystemFrontalBuilder();
            var lateral = new PushBackSystemLateralBuilder();
            return (key,
                AsInt(window.DesignToInsert?.Structure?.DimensionViews),
                AsInt(system?.Structure?.DimensionViews),
                Sign(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances
                    .Concat(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances)),
                Sign(lateral.Build(system, catalog).Flatten().Instances
                    .Concat(lateral.Cortes(system, catalog).SelectMany(corte => corte.Plan.Flatten().Instances))),
                Sign(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances));
        }

        private static (int Dimensions, string Signature) Sign(IEnumerable<HeaderBlockInstance> instances)
        {
            var list = instances.ToList();
            return (list.Count(i => i.Role == HeaderBlockRole.Dimension),
                string.Join("\n", list.Select(Key).OrderBy(s => s, StringComparer.Ordinal)));
        }

        private static string Key(HeaderBlockInstance i)
        {
            var parameters = string.Join(";", i.DynamicParameters
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => FormattableString.Invariant($"{p.Key}={p.Value:R}")));
            return FormattableString.Invariant(
                $"{i.View}|{i.Role}|{i.PieceId}|{i.BlockName}|{i.Text}|{i.Insertion.X:R}|{i.Insertion.Y:R}|{i.ConnectionAnchor.X:R}|{i.ConnectionAnchor.Y:R}|{i.RotationRadians:R}|{(i.MirroredX ? 1 : 0)}|{(i.MirroredY ? 1 : 0)}|{i.DimensionOffset:R}|{i.TextHeight:R}|{i.DimensionStyleName}|{parameters}");
        }
    }
}
