using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackCad.UI.Systems.Selective.RackSelectiveWindow"/> (initiative I-24). Insert/update run through the
    /// window's OWN button handlers (a genuine WPF Click → <c>*_Click</c> → <c>RequestDraw</c> → ConfirmPendingCellEdits →
    /// BuildSystem → SetModel → session → typed payload → Close), NOT by calling <c>session.RequestInsert/RequestUpdate</c>
    /// directly. <c>SelectiveEditorStateAdoptionTests</c> already locks the load→build GEOMETRY; here the orthogonal
    /// dimension is GUID/name/view/UpdateOnly conservation, the concrete request type and a STRICTLY corresponding payload:
    /// the full resolved drawing (frontal of every fondo + planta + lateral cortes) built from the payload's Design equals
    /// the one built directly from the payload's System. Deterministic: no timing, no pixels.
    /// </summary>
    public sealed class SelectiveEditorWindowTests
    {
        // Real catalog ids shipped next to the test binaries (mirrors SelectiveEditorStateAdoptionTests).
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- Pure load identity (kept: distinct from the real-handler tests) ----

        [Fact]
        public void LoadExisting_AdoptsDrawnGuidAndName()
        {
            // RACKEDITAR path: reopening a drawn selective rack adopts its GUID + name so a re-save keeps identity.
            var (id, name) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                return (window.RackId, window.RackName);
            });

            Assert.Equal("GUID-SEL", id);
            Assert.Equal("Selectivo A", name);
        }

        // ---- Real insert/update through the window's own button handlers ----

        [Fact]
        public void NewRack_InsertFrontal_ViaButton_MintsGuid_AndBuildsTheRealPayload()
        {
            // The REAL "Insertar frontal" handler runs (RequestDraw → BuildSystem → SetModel → session): a fresh GUID is
            // minted, the typed name is captured, the request is a SelectiveInsertionRequest for the frontal view, and the
            // payload's design+system STRICTLY correspond (full drawing signature of design == of system).
            var (requested, view, updateOnly, id, name, requestType, corresponds) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadForNew(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-TEMPLATE", "Plantilla"));
                EditorWindowTestSupport.SetText(window, "NameBox", "Selectivo nuevo");
                EditorWindowTestSupport.ClickByContent(window, "Insertar frontal");
                return Capture(window);
            });

            Assert.True(requested);
            Assert.Equal("frontal", view);
            Assert.False(updateOnly);
            Assert.True(Guid.TryParse(id, out _)); // fresh GUID minted by the real handler (template opened as new)
            Assert.Equal("Selectivo nuevo", name);  // the handler read NameBox.Text
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);                // full drawing signatures of design and system match
        }

        [Fact]
        public void ExistingRack_Update_ViaButton_KeepsGuidAndName_RedrawsInPlace()
        {
            // The REAL "Actualizar" handler on an existing rack: GUID + name preserved, in-place redraw (view null,
            // UpdateOnly true), typed SelectiveInsertionRequest, strictly corresponding payload.
            var (requested, view, updateOnly, id, name, requestType, corresponds) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                return Capture(window);
            });

            Assert.True(requested);
            Assert.True(updateOnly);
            Assert.Null(view);
            Assert.Equal("GUID-SEL", id);
            Assert.Equal("Selectivo A", name);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);
        }

        [Fact]
        public void ExistingRack_InsertLateral_ViaButton_KeepsGuidAndName_LinkedView()
        {
            // The REAL "Insertar lateral" handler on an existing rack: a linked lateral view keeps the GUID + name and
            // carries the normalized view; typed SelectiveInsertionRequest with a strictly corresponding payload.
            var (requested, view, updateOnly, id, name, requestType, corresponds) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(MinimalDesign(), "GUID-SEL", "Selectivo A"));
                EditorWindowTestSupport.ClickNamed(window, "InsertLateralButton");
                return Capture(window);
            });

            Assert.True(requested);
            Assert.Equal("lateral", view);
            Assert.False(updateOnly);
            Assert.Equal("GUID-SEL", id);
            Assert.Equal("Selectivo A", name);
            Assert.Equal(nameof(SelectiveInsertionRequest), requestType);
            Assert.True(corresponds);
        }

        // ---- I-50, T-21: the policy the user chose reaches the drawing through the real "Actualizar" ----

        /// <summary>
        /// I-50 T-21 (G3). The three «Mostrar cotas en» boxes are driven like a user would (outside a load), the REAL
        /// "Actualizar" handler builds the payload, and the drawing built from it follows the policy per view TYPE: an
        /// enabled type draws exactly its legacy view (same signature as the untouched legacy rack), a disabled type draws
        /// no dimension. The payload still corresponds strictly (design → resolve == system), so the policy travels in the
        /// DESIGN that gets embedded, not only in the system. Legacy stays null.
        /// </summary>
        [Fact]
        public void T21_ExistingRack_Update_ThePolicyTheUserChose_ReachesEveryDrawnViewByType()
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
            Assert.True(legacy.Corresponds);
            Assert.True(legacy.Frontal.Dimensions > 0 && legacy.Lateral.Dimensions > 0 && legacy.Planta.Dimensions > 0,
                "the legacy rack draws dimensions in its three view types");

            foreach (var (key, policy, frontal, lateral, planta) in new[]
                     {
                         ("F", 1, true, false, false),
                         ("L", 2, false, true, false),
                         ("F|P", 5, true, false, true)
                     })
            {
                var outcome = outcomes[key];
                Assert.Equal(policy, outcome.Policy);
                Assert.True(outcome.Corresponds, key + ": payload design and system must correspond");
                AssertView(key + " frontal", frontal, legacy.Frontal, outcome.Frontal);
                AssertView(key + " lateral", lateral, legacy.Lateral, outcome.Lateral);
                AssertView(key + " planta", planta, legacy.Planta, outcome.Planta);
            }
        }

        private static void AssertView(string what, bool on, (int Dimensions, string Signature) legacy, (int Dimensions, string Signature) actual)
        {
            if (on)
            {
                Assert.True(legacy.Signature == actual.Signature, what + ": an enabled view type must draw exactly its legacy view");
            }
            else
            {
                Assert.True(actual.Dimensions == 0, $"{what}: a disabled view type drew {actual.Dimensions} dimension(s)");
            }
        }

        /// <summary>Open an existing rack with dimensions on (Standard, legacy policy), untick the named boxes as the user
        /// would, press the real "Actualizar", and sign each view type of the payload's system.</summary>
        private static (string Case, int? Policy, bool Corresponds, (int Dimensions, string Signature) Frontal, (int Dimensions, string Signature) Lateral, (int Dimensions, string Signature) Planta)
            UpdateWith(string key, params string[] untick)
        {
            var design = MinimalDesign();
            design.Dimensions = DimensionDetail.Standard;
            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.LoadExisting(SelectivePalletDesignDocument.From(design, "GUID-SEL", "Selectivo A"));
            foreach (var name in untick)
            {
                ((CheckBox)window.FindName(name)).IsChecked = false;
            }

            EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
            var system = window.SystemToInsert;
            var payload = window.DesignToInsert;
            var policy = payload?.DimensionViews;
            return (key,
                policy.HasValue ? (int)policy.Value : (int?)null,
                system != null && payload != null && Corresponds(payload, system),
                ViewSignature(FrontalInstances(system)),
                ViewSignature(LateralInstances(system)),
                ViewSignature(PlantaInstances(system)));
        }

        private static (int Dimensions, string Signature) ViewSignature(IEnumerable<HeaderBlockInstance> instances)
        {
            var list = instances.ToList();
            return (list.Count(i => i.Role == HeaderBlockRole.Dimension),
                string.Join("\n", list.Select(InstanceKey).OrderBy(s => s, StringComparer.Ordinal)));
        }

        // ---- Helpers ----

        private static (bool Requested, string View, bool UpdateOnly, string Id, string Name, string RequestType, bool Corresponds) Capture(RackSelectiveWindow window)
        {
            var system = window.SystemToInsert;
            var design = window.DesignToInsert;
            var corresponds = system != null && design != null && Corresponds(design, system);
            return (window.InsertRequested, window.InsertView, window.UpdateOnly, window.RackId, window.RackName,
                window.Session.InsertionRequest?.GetType().Name, corresponds);
        }

        /// <summary>Strict correspondence: the full resolved drawing built from the payload's Design equals the one built
        /// directly from the payload's System. The window sets the display name on the system after resolving, so the
        /// resolved design's system is given that same Name before signing (so any name-dependent annotation lines up).</summary>
        private static bool Corresponds(SelectivePalletDesign design, SelectiveRackSystem system)
        {
            var resolved = new SelectiveGeometryResolver().Resolve(design, Catalog);
            resolved.Name = system.Name;
            return DrawingSignature(resolved) == DrawingSignature(system);
        }

        /// <summary>Full resolved-drawing signature of a selective system: frontal of every fondo + planta + lateral
        /// cortes, EVERY instance (including Annotation/Dimension: their Text, DimensionOffset and DimensionStyleName are in
        /// InstanceKey), plus the resolved height. Mirrors the pattern in SelectiveEditorStateAdoptionTests.</summary>
        private static string DrawingSignature(SelectiveRackSystem system)
        {
            var instances = new List<HeaderBlockInstance>();
            instances.AddRange(FrontalInstances(system));
            instances.AddRange(PlantaInstances(system));
            instances.AddRange(LateralInstances(system));

            var keys = instances.Select(InstanceKey).OrderBy(s => s, StringComparer.Ordinal);
            return "H=" + system.Height.ToString("R", CultureInfo.InvariantCulture) + "\n" + string.Join("\n", keys);
        }

        /// <summary>The frontal of every fondo (each through <c>FondoSystemView</c>, as the plugin draws it).</summary>
        private static IEnumerable<HeaderBlockInstance> FrontalInstances(SelectiveRackSystem system)
        {
            var catalog = Catalog;
            var frontal = new SelectiveFrontalBuilder();
            var instances = new List<HeaderBlockInstance>();
            for (var fondo = 0; fondo < SelectiveDepthLayout.Count(system); fondo++)
            {
                instances.AddRange(frontal.Build(SelectiveDepthLayout.FondoSystemView(system, fondo), catalog));
            }

            return instances;
        }

        private static IEnumerable<HeaderBlockInstance> PlantaInstances(SelectiveRackSystem system)
            => new SelectivePlantaBuilder().Build(system, Catalog);

        private static IEnumerable<HeaderBlockInstance> LateralInstances(SelectiveRackSystem system)
            => new SelectiveLateralBuilder().Cortes(system, Catalog).SelectMany(c => c.Largueros);

        private static string InstanceKey(HeaderBlockInstance i)
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

        /// <summary>A minimal but valid single-fondo selective design (one bay, two levels, floor beam).</summary>
        private static SelectivePalletDesign MinimalDesign()
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
                DrawBasePlate = true
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
    }
}
