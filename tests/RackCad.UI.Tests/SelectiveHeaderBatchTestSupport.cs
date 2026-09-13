using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53S (G5): drives the REAL «Reutilizar cabecera» surface of the selective editor — «Tomar como origen», «Postes
    /// destino» and «Aplicar origen a destinos» — the way a user does, and reads back what the gesture produced.
    /// <para>
    /// Every lookup asserts presence before it interacts, so a missing control fails the test by ASSERTION and never by an
    /// accidental exception. The fixtures that PLACE a custom cabecera write the state directly and recompute: they set the
    /// scene, they are not the gesture under test.
    /// </para>
    /// </summary>
    internal static class SelectiveHeaderBatchTestSupport
    {
        internal const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";

        /// <summary>A usable cabecera recipe built by the real factory, at <paramref name="height"/>.</summary>
        internal static RackFrameConfiguration Recipe(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        internal static ComboBox FondoSelector(RackSelectiveWindow window) => (ComboBox)window.FindName("FondoSelectorBox");

        internal static ComboBox PostSelector(RackSelectiveWindow window) => (ComboBox)window.FindName("PostSelectBox");

        /// <summary>Show fondo <paramref name="oneBased"/> through the real «Editando fondo» selector.</summary>
        internal static void ShowFondo(RackSelectiveWindow window, int oneBased) => FondoSelector(window).SelectedIndex = oneBased - 1;

        /// <summary>Select «Poste <paramref name="oneBased"/>» through the real post selector.</summary>
        internal static void SelectPost(RackSelectiveWindow window, int oneBased) => PostSelector(window).SelectedIndex = oneBased - 1;

        /// <summary>Give ONE fondo its own frentes through the real pending field and its LostFocus commit.</summary>
        internal static void SetFrentesOfFondo(RackSelectiveWindow window, int oneBased, int frentes)
        {
            ShowFondo(window, oneBased);
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            EditorWindowTestSupport.SetText(window, "BayCountBox", frentes.ToString(CultureInfo.InvariantCulture));
            SelectiveWindowTestSupport.RaiseLostFocus(window, "BayCountBox");
        }

        /// <summary>
        /// Fixture: a custom cabecera at <c>(fondo, poste)</c>, one-based, written into the state with the depth of that fondo,
        /// then a recompute so the resolution in force describes it.
        /// </summary>
        internal static void PlaceCustom(RackSelectiveWindow window, int fondoOneBased, int postOneBased, double height)
        {
            var state = window.EditorState;
            state.SyncPostCabeceras();
            var fondo = fondoOneBased - 1;
            var post = postOneBased - 1;
            Assert.True(state.PostExistsIn(fondo, post), "Fixture: el poste " + postOneBased + " no existe en el fondo " + fondoOneBased + ".");

            var recipe = Recipe(window, height);
            SelectiveCabeceraAuthority.ImposeFondoDepth(recipe, state.CabeceraDepthOfFondo(fondo));
            var row = fondo == 0 ? state.PostCabeceras : state.ExtraFondoPostCabeceras[fondo - 1];
            while (row.Count <= post) row.Add(null);
            row[post] = recipe;

            window.RefreshPostStatusForTest();
            window.Session.Recompute.Request();
        }

        internal static void TakeSource(RackSelectiveWindow window) => EditorWindowTestSupport.ClickNamed(window, "TakeHeaderSourceButton");

        internal static void Apply(RackSelectiveWindow window) => EditorWindowTestSupport.ClickNamed(window, "ApplyHeaderBatchButton");

        /// <summary>Aim «Postes destino» at exactly these posts (one-based), as the user would: wanted first, then the rest off.</summary>
        internal static void SetPostTargets(RackSelectiveWindow window, params int[] oneBased)
        {
            PressPostTargets(window, "Actual");
            foreach (var post in oneBased) TogglePost(window, post, true);

            var count = window.EditorState.MaxFrenteCount() + 1;
            for (var post = 1; post <= count; post++)
            {
                if (!oneBased.Contains(post)) TogglePost(window, post, false);
            }
        }

        internal static void SetAllPostTargets(RackSelectiveWindow window) => PressPostTargets(window, "Todos");

        internal static void SetCurrentPostTarget(RackSelectiveWindow window) => PressPostTargets(window, "Actual");

        /// <summary>The closed caption of «Postes destino».</summary>
        internal static string PostTargetsCaption(RackSelectiveWindow window)
            => (window.FindName("PostTargetsButton") as ToggleButton)?.Content as string;

        /// <summary>The «Poste N» boxes of «Postes destino», in order.</summary>
        internal static string[] PostTargetBoxes(RackSelectiveWindow window)
        {
            var host = window.FindName("PostTargetsList") as StackPanel;
            Assert.True(host != null, "No existe el selector «Postes destino» (PostTargetsList).");
            return host.Children.OfType<CheckBox>().Select(box => box.Content as string).ToArray();
        }

        private static void PressPostTargets(RackSelectiveWindow window, string label)
        {
            var host = window.FindName("PostTargetsList") as StackPanel;
            Assert.True(host != null, "No existe el selector «Postes destino» (PostTargetsList).");
            var button = host.Children.OfType<Button>()
                .FirstOrDefault(b => (b.Content as string) == label || (b.Content as string) == "✓ " + label);
            Assert.True(button != null, "«Postes destino» no ofrece la accion «" + label + "».");
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }

        private static void TogglePost(RackSelectiveWindow window, int oneBased, bool wanted)
        {
            var host = window.FindName("PostTargetsList") as StackPanel;
            Assert.True(host != null, "No existe el selector «Postes destino» (PostTargetsList).");
            var box = host.Children.OfType<CheckBox>().FirstOrDefault(c => (c.Content as string) == "Poste " + oneBased);
            Assert.True(box != null, "«Postes destino» no ofrece «Poste " + oneBased + "».");
            if ((box.IsChecked == true) == wanted) return;
            box.IsChecked = wanted;
        }

        /// <summary>
        /// Every cabecera row of every fondo plus the per-post peraltes, serialized: two equal values mean ZERO mutation of
        /// ID6/ID7, scalars included.
        /// </summary>
        internal static string CabeceraRows(RackSelectiveWindow window)
        {
            var state = window.EditorState;
            var store = new RackProjectStore();
            var parts = new List<string>();
            for (var fondo = 0; fondo < state.FondoCount; fondo++)
            {
                for (var post = 0; post <= state.MaxFrenteCount(); post++)
                {
                    var custom = state.CabeceraAt(fondo, post);
                    parts.Add(fondo + "/" + post + "=" + (custom == null ? "-" : store.Serialize(RackProject.ForSelective(custom))));
                }
            }

            parts.Add("peraltes=" + string.Join(",", state.PostPeraltes.Select(p => p.ToString("R", CultureInfo.InvariantCulture))));
            return string.Join("\n", parts);
        }

        /// <summary>Addresses as <c>fondo/poste</c>, zero-based, in the given order.</summary>
        internal static string Addresses(IEnumerable<SelectiveHeaderAddress> addresses)
            => string.Join(" ", addresses.Select(a => a.FondoIndex + "/" + a.PostIndex));

        internal static string Omissions(IEnumerable<HeaderOmission<SelectiveHeaderAddress>> omissions)
            => string.Join(" ", omissions.Select(o => o.Address.FondoIndex + "/" + o.Address.PostIndex + ":" + o.Reason));

        /// <summary>
        /// Show the editor off-screen for the duration of <paramref name="body"/>: the configurator takes it as Owner, and a
        /// window that was never shown cannot own another one.
        /// </summary>
        internal static T WithShownWindow<T>(RackSelectiveWindow window, Func<T> body)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = -10000;
            window.Top = -10000;
            window.ShowInTaskbar = false;
            window.Show();
            try
            {
                return body();
            }
            finally
            {
                window.Close();
            }
        }

        /// <summary>
        /// A configurator presenter that records the mode the real window opened in, edits the configuration IN PLACE —
        /// the advanced editor's way — and closes. No modal loop.
        /// </summary>
        internal static Action<RackFrameConfiguratorWindow> EditIncrementally(Action<RackFrameConfiguration> edit, List<bool> modes = null)
            => configurator =>
            {
                modes?.Add(configurator.ViewModel.IsAdvancedEditor);
                edit?.Invoke(configurator.ViewModel.Configuration);
                configurator.Close();
            };

        // ---- Designs, recipes and drawing signatures (S-23, S-29, C2-4) ----

        internal const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static readonly Lazy<RackCad.Application.Catalogs.RackCatalog> LazyCatalog =
            new Lazy<RackCad.Application.Catalogs.RackCatalog>(() => RackCad.Application.Catalogs.JsonRackCatalogProvider.FromBaseDirectory().Load());

        internal static RackCad.Application.Catalogs.RackCatalog Catalog => LazyCatalog.Value;

        /// <summary>A usable cabecera recipe at <paramref name="height"/>, without a window.</summary>
        internal static RackFrameConfiguration Recipe(double height)
            => new RackFrameConfigurationFactory(Catalog).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        /// <summary>A valid selective design with one entry per fondo in <paramref name="frentesPerFondo"/> (two levels per frente).</summary>
        internal static RackCad.Domain.Systems.Selective.SelectivePalletDesign Design(params int[] frentesPerFondo)
        {
            var design = new RackCad.Domain.Systems.Selective.SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = frentesPerFondo.Length,
                DrawBasePlate = true
            };

            for (var bay = 0; bay < frentesPerFondo[0]; bay++) design.Bays.Add(Bay());
            for (var fondo = 1; fondo < frentesPerFondo.Length; fondo++)
            {
                var bays = new List<RackCad.Domain.Systems.Selective.SelectiveBayDesign>();
                for (var bay = 0; bay < frentesPerFondo[fondo]; bay++) bays.Add(Bay());
                design.ExtraFondoBays.Add(bays);
            }

            return design;
        }

        private static RackCad.Domain.Systems.Selective.SelectiveBayDesign Bay()
        {
            var bay = new RackCad.Domain.Systems.Selective.SelectiveBayDesign { FloorBeam = true };
            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new RackCad.Domain.Systems.Selective.SelectiveCell
                {
                    Pallet = new RackCad.Domain.Systems.Selective.Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>The custom cabecera of every (fondo, post) of the master grid: its height, <c>-</c> for the standard
        /// one and <c>x</c> for a post the fondo does not have.</summary>
        internal static string CustomMap(SelectiveEditorState state)
        {
            var parts = new List<string>();
            for (var fondo = 0; fondo < state.FondoCount; fondo++)
            {
                for (var post = 0; post <= state.MaxFrenteCount(); post++)
                {
                    var custom = state.CabeceraAt(fondo, post);
                    parts.Add(fondo + "/" + post + "=" + (!state.PostExistsIn(fondo, post) ? "x" : custom == null ? "-" : R(custom.Height)));
                }
            }

            return string.Join(" ", parts);
        }

        /// <summary>The same map read off a RESOLVED system through the drawing's own authority.</summary>
        internal static string CustomMap(RackCad.Domain.Systems.Selective.SelectiveRackSystem system, int fondos, int posts)
        {
            var parts = new List<string>();
            for (var fondo = 0; fondo < fondos; fondo++)
            {
                for (var post = 0; post < posts; post++)
                {
                    var bays = SelectiveDepthLayout.BaysOfFondo(system, fondo);
                    var exists = bays != null && post <= bays.Count;
                    var custom = exists ? SelectiveCabeceraAuthority.UsableCustomAt(system, fondo, post) : null;
                    parts.Add(fondo + "/" + post + "=" + (!exists ? "x" : custom == null ? "-" : R(custom.Height)));
                }
            }

            return string.Join(" ", parts);
        }

        internal static string R(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        /// <summary>The system resolved from <paramref name="design"/>, named like the window names its own.</summary>
        internal static RackCad.Domain.Systems.Selective.SelectiveRackSystem Resolve(RackCad.Domain.Systems.Selective.SelectivePalletDesign design, string name)
        {
            var resolved = new SelectiveGeometryResolver().Resolve(design, Catalog);
            resolved.Name = name;
            return resolved;
        }

        /// <summary>Strict correspondence (I-24): the full drawing built from the payload's design equals the one built from its system.</summary>
        internal static bool Corresponds(RackCad.Domain.Systems.Selective.SelectivePalletDesign design, RackCad.Domain.Systems.Selective.SelectiveRackSystem system)
            => DrawingSignature(Resolve(design, system.Name)) == DrawingSignature(system);

        /// <summary>Full drawing: the frontal of every fondo, the planta and the lateral cortes, plus the resolved height.</summary>
        internal static string DrawingSignature(RackCad.Domain.Systems.Selective.SelectiveRackSystem system)
            => "H=" + system.Height.ToString("R", CultureInfo.InvariantCulture)
               + "\nF=" + string.Join("\n", Enumerable.Range(0, SelectiveDepthLayout.Count(system)).Select(fondo => FrontalSignature(system, fondo)))
               + "\nP=" + PlantaSignature(system)
               + "\nL=" + LateralSignature(system);

        internal static string FrontalSignature(RackCad.Domain.Systems.Selective.SelectiveRackSystem system, int fondo)
            => Keys(new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, fondo), Catalog));

        internal static string PlantaSignature(RackCad.Domain.Systems.Selective.SelectiveRackSystem system)
            => Keys(new SelectivePlantaBuilder().Build(system, Catalog));

        internal static string LateralSignature(RackCad.Domain.Systems.Selective.SelectiveRackSystem system)
            => Keys(new SelectiveLateralBuilder().Cortes(system, Catalog).SelectMany(corte => corte.Largueros));

        internal static string BomSignature(RackCad.Domain.Systems.Selective.SelectiveRackSystem system)
            => string.Join("|", SelectiveBomBuilder.Build(system, Catalog).Lines
                .Select(line => line.Category + ":" + line.ProfileId + ":" + line.Description + ":"
                                + line.Length.ToString("0.###", CultureInfo.InvariantCulture) + ":" + line.Quantity)
                .OrderBy(text => text, StringComparer.Ordinal));

        private static string Keys(IEnumerable<RackCad.Application.Drawing.HeaderBlockInstance> instances)
            => string.Join("\n", instances.Select(InstanceKey).OrderBy(key => key, StringComparer.Ordinal));

        private static string InstanceKey(RackCad.Application.Drawing.HeaderBlockInstance i)
        {
            var parameters = string.Join(";", i.DynamicParameters
                .OrderBy(k => k.Key, StringComparer.Ordinal)
                .Select(k => k.Key + "=" + k.Value.ToString("R", CultureInfo.InvariantCulture)));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4:R},{5:R}|{6:R},{7:R}|{8:R}|{9}{10}|{11}|{12:R}|{13}|{14}",
                (int)i.Role, i.BlockName, i.PieceId, i.View,
                i.Insertion.X, i.Insertion.Y, i.ConnectionAnchor.X, i.ConnectionAnchor.Y,
                i.RotationRadians, i.MirroredX ? 1 : 0, i.MirroredY ? 1 : 0, parameters,
                i.DimensionOffset, i.Text ?? string.Empty, i.DimensionStyleName ?? string.Empty);
        }
    }
}
