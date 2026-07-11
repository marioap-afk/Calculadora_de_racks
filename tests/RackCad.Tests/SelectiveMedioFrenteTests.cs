using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// "Medio frente" generalizado (Fase 2 of doble profundidad): a bay is split into N tramos with N-1 INTERMEDIATE
    /// posts. Each tramo carries a larguero length and a loaded flag; the LAST tramo's length is CALCULATED (the
    /// remainder). Loaded tramos get largueros of their own length; empty tramos stay bare (letting you tie one side,
    /// the other, or both). The bay's shared end posts / span are unchanged. Fewer than 2 tramos = a full-width bay.
    /// </summary>
    public class SelectiveMedioFrenteTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const string LengthParam = "LONGITUD";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign SegmentsDesign(params (double length, bool loaded)[] segments)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            foreach (var (length, loaded) in segments)
            {
                bay.Segments.Add(new SelectiveSegment { Length = length, Loaded = loaded });
            }

            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 45 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 45 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            return design;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design) => new SelectiveGeometryResolver().Resolve(design, Catalog);

        private static double[] BeamLengths(System.Collections.Generic.IEnumerable<HeaderBlockInstance> instances)
            => instances.Where(i => i.Role == HeaderBlockRole.Beam).Select(i => i.DynamicParameters[LengthParam]).ToArray();

        [Fact]
        public void NoSegments_IsUnchanged_FullBay()
        {
            var system = Resolve(SegmentsDesign());

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);
            Assert.Equal(2, instances.Count(i => i.Role == HeaderBlockRole.Post)); // just the two shared posts
            Assert.All(BeamLengths(instances), l => Assert.Equal(system.Bays[0].BeamLength, l, 4));
        }

        [Fact]
        public void LeftTramoLoaded_ShortLarguerosPlusOneIntermediatePost()
        {
            var system = Resolve(SegmentsDesign((30.0, true), (0.0, false))); // classic ½frente: load left, empty right

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);
            var posts = instances.Where(i => i.Role == HeaderBlockRole.Post).ToList();
            var beams = BeamLengths(instances);

            Assert.Equal(3, posts.Count); // 2 shared + 1 intermediate
            Assert.Equal(2, beams.Length); // only the left tramo is loaded → one larguero per level
            Assert.All(beams, l => Assert.Equal(30.0, l, 4));
            Assert.True(30.0 < system.Bays[0].BeamLength);

            var xs = posts.Select(p => p.Insertion.X).OrderBy(x => x).ToList();
            Assert.True(xs[1] > xs[0] + 1e-6 && xs[1] < xs[2] - 1e-6, $"intermediate {xs[1]} not between {xs[0]} and {xs[2]}");
        }

        [Fact]
        public void RightTramoLoaded_LargueroIsTheCalculatedRemainder()
        {
            var system = Resolve(SegmentsDesign((30.0, false), (0.0, true))); // empty left, load the calculated right

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);
            var beams = BeamLengths(instances);

            Assert.Equal(3, instances.Count(i => i.Role == HeaderBlockRole.Post)); // still 2 shared + 1 intermediate
            Assert.Equal(2, beams.Length); // only the right tramo is loaded
            Assert.All(beams, l => Assert.NotEqual(30.0, l, 4)); // it's the remainder, not the 30" tramo
            Assert.All(beams, l => Assert.True(l > 0.0 && l < system.Bays[0].BeamLength));
        }

        [Fact]
        public void BothTramosLoaded_IntermediateModule_TwoDistinctLargueroLengths()
        {
            var system = Resolve(SegmentsDesign((30.0, true), (0.0, true))); // both sides loaded = an intermediate module

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);
            var beams = BeamLengths(instances);

            Assert.Equal(3, instances.Count(i => i.Role == HeaderBlockRole.Post));
            Assert.Equal(4, beams.Length); // 2 loaded tramos × 2 levels
            var distinct = beams.Select(l => System.Math.Round(l, 4)).Distinct().ToList();
            Assert.Equal(2, distinct.Count); // the 30" tramo + the calculated remainder
            Assert.Contains(distinct, l => System.Math.Abs(l - 30.0) < 1e-4);
        }

        [Fact]
        public void ThreeTramos_TwoIntermediatePosts_LastCalculated()
        {
            var system = Resolve(SegmentsDesign((20.0, true), (25.0, true), (0.0, true)));

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);
            var beams = BeamLengths(instances).Select(l => System.Math.Round(l, 4)).ToArray();

            Assert.Equal(4, instances.Count(i => i.Role == HeaderBlockRole.Post)); // 2 shared + 2 intermediate
            Assert.Equal(6, beams.Length); // 3 loaded tramos × 2 levels
            Assert.Contains(beams, l => System.Math.Abs(l - 20.0) < 1e-4);
            Assert.Contains(beams, l => System.Math.Abs(l - 25.0) < 1e-4);
            // The remainder tramo is a third length, positive and smaller than the full bay.
            var remainder = beams.First(l => System.Math.Abs(l - 20.0) > 1e-4 && System.Math.Abs(l - 25.0) > 1e-4);
            Assert.True(remainder > 0.0 && remainder < system.Bays[0].BeamLength);
        }

        [Fact]
        public void TramosLongerThanTheBay_DoNotFit_DrawnAsFullBay()
        {
            var system = Resolve(SegmentsDesign((50.0, true), (50.0, true), (0.0, true))); // 100" of tramos in an ~84" bay

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);

            Assert.Equal(2, instances.Count(i => i.Role == HeaderBlockRole.Post)); // no intermediate posts — it doesn't fit
            Assert.All(BeamLengths(instances), l => Assert.Equal(system.Bays[0].BeamLength, l, 4));
        }

        [Fact]
        public void Planta_TwoTramos_IntermediateCabecera_ShortLargueros()
        {
            var system = Resolve(SegmentsDesign((30.0, true), (0.0, false)));

            var instances = new SelectivePlantaBuilder().Build(system, Catalog);

            // A 1-bay planta has 2 frames × 2 posts (front/back) = 4; the medio frente adds ONE intermediate frame (2 posts) = 6.
            Assert.Equal(6, instances.Count(i => i.Role == HeaderBlockRole.Post));
            var beams = BeamLengths(instances);
            Assert.NotEmpty(beams);
            Assert.All(beams, l => Assert.Equal(30.0, l, 4)); // only the loaded tramo draws largueros (front + back)
        }

        [Fact]
        public void RoundTrip_PreservesSegments()
        {
            var design = SegmentsDesign((30.0, true), (18.0, false), (0.0, true));
            var store = new SelectivePalletDesignStore();

            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "n"))).ToDomain();

            var segs = restored.Bays[0].Segments;
            Assert.Equal(3, segs.Count);
            Assert.Equal(30.0, segs[0].Length, 4);
            Assert.True(segs[0].Loaded);
            Assert.Equal(18.0, segs[1].Length, 4);
            Assert.False(segs[1].Loaded);
            Assert.True(segs[2].Loaded);
        }

        [Fact]
        public void Legacy_MedioFrenteLength_MapsToTwoTramos()
        {
            // A document from the pre-N-way build (single MedioFrenteLength, no Segments) still loads.
            var document = new SelectivePalletDesignDocument { PalletDepth = 48.0 };
            document.Bays.Add(new SelectiveBayDocument { MedioFrenteLength = 42.0 });

            var design = document.ToDomain();

            var segs = design.Bays[0].Segments;
            Assert.Equal(2, segs.Count);
            Assert.Equal(42.0, segs[0].Length, 4);
            Assert.True(segs[0].Loaded); // the custom tramo carries largueros
            Assert.False(segs[1].Loaded); // the calculated remainder stays empty (the classic ½frente)
        }
    }
}
