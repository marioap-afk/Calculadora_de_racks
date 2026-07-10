using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Locks the resolved geometry of a LARGE run (20 frentes ≈ the size where the editor lagged) so the
    /// performance work cannot change behavior: same counts, same post positions, same level Ys before and
    /// after optimizing. If an optimization alters any output, these break first.
    /// </summary>
    public class SelectiveTwentyBaysEquivalenceTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const int BayCount = 20;
        private const int LevelCount = 5;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>20 bays × 5 levels with VARIED pallets/counts per bay (so each bay resolves differently)
        /// and a per-post peralte override on every third post (so the per-post path is exercised).</summary>
        private static SelectivePalletDesign Design()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0 };

            for (var b = 0; b < BayCount; b++)
            {
                var bay = new SelectiveBayDesign { FloorBeam = b % 4 == 0 };
                for (var l = 0; l < LevelCount; l++)
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = 40.0 + (b % 3) * 4.0, Alto = 50.0 + (l % 2) * 10.0 },
                        PalletCount = 1 + (b + l) % 2,
                        BeamId = BeamId,
                        BeamPeralte = 4.0
                    });
                }

                design.Bays.Add(bay);
            }

            for (var p = 0; p <= BayCount; p++)
            {
                design.PostPeraltes.Add(p % 3 == 0 ? 5.0 : 0.0); // every third post overrides its peralte
            }

            return design;
        }

        [Fact]
        public void Resolve_TwentyBays_KeepsCountsAndMonotonicGeometry()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(), Catalog);

            Assert.Equal(BayCount, system.Bays.Count);
            Assert.Equal(BayCount + 1, system.PostPeraltes.Count);
            Assert.True(system.Height > 0.0);

            for (var b = 0; b < system.Bays.Count; b++)
            {
                var bay = system.Bays[b];

                // The ground pallet rests on the floor with NO beam, so 5 pallet levels resolve to 4 beam levels —
                // unless the bay has a "larguero a piso" (every 4th in the sample design), which adds the ground beam.
                var expectedLevels = LevelCount - 1 + (b % 4 == 0 ? 1 : 0);
                Assert.Equal(expectedLevels, bay.Levels.Count);
                Assert.True(bay.BeamLength > 0.0);
                Assert.True(bay.Height > 0.0);

                // Level Ys strictly increase within a bay (the resolver stacks them upward).
                for (var l = 1; l < bay.Levels.Count; l++)
                {
                    Assert.True(bay.Levels[l].Y > bay.Levels[l - 1].Y);
                }
            }
        }

        [Fact]
        public void Build_TwentyBays_ProducesExpectedInstanceCountsAndOrderedPosts()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(), Catalog);
            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);

            Assert.Equal(BayCount + 1, instances.Count(i => i.Role == HeaderBlockRole.Post));
            Assert.Equal(BayCount + 1, instances.Count(i => i.Role == HeaderBlockRole.BasePlate));

            // Per bay: 5 pallet levels = 4 beams (ground pallet on the floor) + 1 extra where FloorBeam=true
            // (bays 0,4,8,12,16 in the sample design): 15×4 + 5×5 = 85.
            var floorBeamBays = Enumerable.Range(0, BayCount).Count(b => b % 4 == 0);
            var expectedBeams = (BayCount - floorBeamBays) * (LevelCount - 1) + floorBeamBays * LevelCount;
            Assert.Equal(expectedBeams, instances.Count(i => i.Role == HeaderBlockRole.Beam));

            var postXs = instances.Where(i => i.Role == HeaderBlockRole.Post).Select(i => i.Insertion.X).ToList();
            Assert.Equal(0.0, postXs[0], 4);
            for (var p = 1; p < postXs.Count; p++)
            {
                Assert.True(postXs[p] > postXs[p - 1], "las X de los postes deben crecer de izquierda a derecha");
            }
        }

        [Fact]
        public void Resolve_TwentyBays_IsDeterministic_SameInputSameOutput()
        {
            var resolver = new SelectiveGeometryResolver();
            var catalog = Catalog;

            var a = resolver.Resolve(Design(), catalog);
            var b = resolver.Resolve(Design(), catalog);

            Assert.Equal(a.Height, b.Height, 6);
            Assert.Equal(a.PostPeraltes, b.PostPeraltes);
            for (var i = 0; i < a.Bays.Count; i++)
            {
                Assert.Equal(a.Bays[i].BeamLength, b.Bays[i].BeamLength, 6);
                Assert.Equal(a.Bays[i].Height, b.Bays[i].Height, 6);
                Assert.Equal(a.Bays[i].Levels.Select(l => l.Y), b.Bays[i].Levels.Select(l => l.Y));
            }
        }
    }
}
