using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Characterization (STA): construct the REAL <see cref="RackCad.UI.RackSelectiveWindow"/>, load a representative
    /// selective design, rebuild it through the window's build path and assert the FULL resolved drawing (frontal of
    /// every fondo + planta + lateral cortes, plus the resolved height) is identical to the design that was loaded.
    ///
    /// This locks the observable behavior of the load→build pipeline across the I-20 state extraction: written against
    /// the current window (state in private fields) it passes; after the state moves to
    /// <c>SelectiveEditorState</c> in Application, the SAME assertions must still pass — proving the window kept the
    /// design, matrices-by-fondo, per-post cabeceras and BuildDesign assembly byte-for-byte equivalent. The scenarios
    /// avoid the two load-time normalizations (empty-frente padding, per-post cabecera depth coercion) so load→build is
    /// geometry-preserving and the equivalence is exact.
    /// </summary>
    public sealed class SelectiveEditorStateAdoptionTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const string TopeId = "LARGUERO_ESCALON_TOPE_DE_3";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void SingleFondo_VariedLevels_FloorBeam_PerPostPeralte_RoundTripsThroughTheWindow()
            => AssertLoadBuildPreservesDrawing(SingleFondoDesign());

        [Fact]
        public void DobleProfundidad_PerFondoLevels_Separators_ExtraDepth_RoundTripsThroughTheWindow()
            => AssertLoadBuildPreservesDrawing(DobleProfundidadDesign());

        [Fact]
        public void MedioFrente_Tramos_RoundTripsThroughTheWindow()
            => AssertLoadBuildPreservesDrawing(MedioFrenteDesign());

        [Fact]
        public void SafetyTope_AndDrawingToggles_RoundTripThroughTheWindow()
            => AssertLoadBuildPreservesDrawing(SafetyAndTogglesDesign());

        /// <summary>
        /// Load <paramref name="design"/> into the real window (as a fresh library open), rebuild it via the window's
        /// build seam and assert the full resolved drawing signature is unchanged. Runs on the STA thread the WPF window
        /// requires.
        /// </summary>
        private static void AssertLoadBuildPreservesDrawing(SelectivePalletDesign design)
        {
            var (built, error) = StaTestRunner.Run(() =>
            {
                var window = new RackCad.UI.RackSelectiveWindow(canInsertInAutoCad: true);
                window.LoadForNew(SelectivePalletDesignDocument.From(design, "GUID-20", "Rack 20"));
                var rebuilt = window.BuildDesignForTest(out var err);
                return (rebuilt, err);
            });

            Assert.Null(error);
            Assert.NotNull(built);

            var catalog = Catalog;
            Assert.Equal(DrawingSignature(design, catalog), DrawingSignature(built, catalog));
        }

        // ---- Representative designs (no empty frentes, cabecera depths on the rule → load is geometry-preserving) ----

        private static SelectiveCell Cell(double frente = 42.0, double alto = 60.0, int count = 2, double peralte = 4.0)
            => new SelectiveCell { Pallet = new Tarima { Frente = frente, Alto = alto }, PalletCount = count, BeamId = BeamId, BeamPeralte = peralte };

        private static SelectiveBayDesign Bay(int levels, bool floor = false, double frente = 42.0, double alto = 60.0)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floor };
            for (var l = 0; l < levels; l++) bay.Levels.Add(Cell(frente, alto));
            return bay;
        }

        private static SelectivePalletDesign SingleFondoDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                FloorBeamRise = 4.0, PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true
            };
            design.Bays.Add(Bay(3, floor: true, frente: 40.0));
            design.Bays.Add(Bay(2, frente: 44.0, alto: 55.0));
            design.PostPeraltes.Add(5.0); // post 0 overrides its peralte
            design.PostPeraltes.Add(0.0);
            design.PostPeraltes.Add(0.0);
            design.CabeceraFondoOverrides.Add(0.0);
            return design;
        }

        private static SelectivePalletDesign DobleProfundidadDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                FloorBeamRise = 4.0, PalletDepth = 48.0, DepthCount = 2, DrawBasePlate = true
            };
            design.SeparatorLengths.Add(8.0);
            design.ExtraFondoDepths.Add(40.0); // fondo 1 shallower
            design.Bays.Add(Bay(3, floor: true));
            design.Bays.Add(Bay(3, floor: true));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, floor: true), Bay(2, floor: true) });
            return design;
        }

        private static SelectivePalletDesign MedioFrenteDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                FloorBeamRise = 4.0, PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true
            };
            var bay = Bay(3, floor: true, frente: 40.0);
            bay.Segments.Add(new SelectiveSegment { Length = 60.0, Loaded = true });
            bay.Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true }); // last tramo calculated
            design.Bays.Add(bay);
            design.Bays.Add(Bay(2, floor: true));
            return design;
        }

        private static SelectivePalletDesign SafetyAndTogglesDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                FloorBeamRise = 4.0, PalletDepth = 48.0, DepthCount = 1,
                DrawBasePlate = false, NumberFronts = true, NumberLevels = true, DrawRackName = true,
                DrawPallets = true, AnnotationScale = 1.5, Dimensions = DimensionDetail.Standard
            };
            design.Bays.Add(Bay(3, floor: true));
            design.Bays.Add(Bay(3, floor: true));
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TopeId, Side = SafetySide.Both, TopeShared = true, TopeFrontal = true, TopeSaque = 3.0
            });
            return design;
        }

        // ---- Full resolved-drawing signature: frontal (every fondo) + planta + lateral cortes + height ----

        private static string DrawingSignature(SelectivePalletDesign design, RackCatalog catalog)
        {
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            var instances = new List<HeaderBlockInstance>();

            var fondoCount = SelectiveDepthLayout.Count(system);
            var frontal = new SelectiveFrontalBuilder();
            for (var fondo = 0; fondo < fondoCount; fondo++)
            {
                instances.AddRange(frontal.Build(SelectiveDepthLayout.FondoSystemView(system, fondo), catalog));
            }

            instances.AddRange(new SelectivePlantaBuilder().Build(system, catalog));
            instances.AddRange(new SelectiveLateralBuilder().Cortes(system, catalog).SelectMany(c => c.Largueros));

            var keys = instances.Select(InstanceKey).OrderBy(s => s, System.StringComparer.Ordinal);
            return "H=" + system.Height.ToString("R", CultureInfo.InvariantCulture) + "\n" + string.Join("\n", keys);
        }

        private static string InstanceKey(HeaderBlockInstance i)
        {
            var parameters = string.Join(";", i.DynamicParameters
                .OrderBy(k => k.Key, System.StringComparer.Ordinal)
                .Select(k => k.Key + "=" + k.Value.ToString("R", CultureInfo.InvariantCulture)));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4:R},{5:R}|{6:R},{7:R}|{8:R}|{9}|{10}",
                (int)i.Role, i.BlockName, i.PieceId, i.View,
                i.Insertion.X, i.Insertion.Y, i.ConnectionAnchor.X, i.ConnectionAnchor.Y,
                i.RotationRadians, i.MirroredX ? 1 : 0, parameters);
        }
    }
}
