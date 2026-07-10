using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Round-trips a pallet-driven selective design (with Id + Name) through JSON, preserving every field.</summary>
    public class SelectivePalletDesignDocumentTests
    {
        private static SelectivePalletDesign SampleDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = "POST_X",
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0
            };

            var bay0 = new SelectiveBayDesign { FloorBeam = true, HeightOverride = 300.0 };
            bay0.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = "BEAM_A", BeamPeralte = 4.0 });
            bay0.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 48, Alto = 50 }, PalletCount = 1, BeamId = "BEAM_A", BeamPeralte = 4.5, BeamLengthOverride = 123.0, ClearOverride = 52.0 });

            var bay1 = new SelectiveBayDesign { FloorBeam = false, HeightOverride = null };
            bay1.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = "BEAM_B", BeamPeralte = 5.0 });

            design.Bays.Add(bay0);
            design.Bays.Add(bay1);
            return design;
        }

        [Fact]
        public void RoundTrip_PreservesANonDefaultPalletDepth()
        {
            var design = SampleDesign();
            design.PalletDepth = 42.0; // non-default fondo must survive the trip

            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "n"))).ToDomain();

            Assert.Equal(42.0, restored.PalletDepth, 4);
        }

        [Fact]
        public void ToDomain_LegacyDocumentWithoutFondo_FallsBackToTheDefault()
        {
            var document = SelectivePalletDesignDocument.From(SampleDesign(), "id", "n");
            document.PalletDepth = 0.0; // documents older than the fondo field

            Assert.Equal(SelectiveRackDefaults.DefaultPalletDepth, document.ToDomain().PalletDepth, 4);
        }

        [Fact]
        public void RoundTrip_PreservesDrawingToggles()
        {
            var design = SampleDesign();
            design.DrawBasePlate = false;
            design.NumberFronts = true;
            design.NumberLevels = true;
            design.DrawRackName = true;

            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "n"))).ToDomain();

            Assert.False(restored.DrawBasePlate);
            Assert.True(restored.NumberFronts);
            Assert.True(restored.NumberLevels);
            Assert.True(restored.DrawRackName);
        }

        [Fact]
        public void ToDomain_LegacyWithoutDrawBasePlate_DefaultsToDrawingIt()
        {
            var document = SelectivePalletDesignDocument.From(SampleDesign(), "id", "n");
            document.DrawBasePlate = null; // a design older than the toggle

            Assert.True(document.ToDomain().DrawBasePlate);
        }

        [Fact]
        public void RoundTrip_PreservesPerPostPeraltes()
        {
            var design = SampleDesign(); // 2 bays -> 3 posts
            design.PostPeraltes.Add(0.0); // post 0 inherits the global
            design.PostPeraltes.Add(6.0); // post 1 override
            design.PostPeraltes.Add(4.0); // post 2 override

            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "n"))).ToDomain();

            Assert.Equal(new[] { 0.0, 6.0, 4.0 }, restored.PostPeraltes);
        }

        [Fact]
        public void RoundTrip_PreservesIdentityAndEveryField()
        {
            var design = SampleDesign();
            var store = new SelectivePalletDesignStore();

            var json = store.Serialize(SelectivePalletDesignDocument.From(design, "id-123", "Rack A"));
            var back = store.Deserialize(json);

            Assert.Equal("id-123", back.Id);
            Assert.Equal("Rack A", back.Name);

            var restored = back.ToDomain();
            Assert.Equal(design.PostId, restored.PostId);
            Assert.Equal(design.PostPeralte, restored.PostPeralte, 4);
            Assert.Equal(design.PalletTolerance, restored.PalletTolerance, 4);
            Assert.Equal(design.VerticalClearance, restored.VerticalClearance, 4);
            Assert.Equal(design.FloorBeamRise, restored.FloorBeamRise, 4);
            Assert.Equal(2, restored.Bays.Count);

            var b0 = restored.Bays[0];
            Assert.True(b0.FloorBeam);
            Assert.True(b0.HeightOverride.HasValue);
            Assert.Equal(300.0, b0.HeightOverride.Value, 4);
            Assert.Equal(2, b0.Levels.Count);

            var c01 = b0.Levels[1];
            Assert.Equal(48.0, c01.Pallet.Frente, 4);
            Assert.Equal(50.0, c01.Pallet.Alto, 4);
            Assert.Equal(1, c01.PalletCount);
            Assert.Equal("BEAM_A", c01.BeamId);
            Assert.Equal(4.5, c01.BeamPeralte, 4);
            Assert.True(c01.BeamLengthOverride.HasValue);
            Assert.Equal(123.0, c01.BeamLengthOverride.Value, 4);
            Assert.True(c01.ClearOverride.HasValue);
            Assert.Equal(52.0, c01.ClearOverride.Value, 4);

            var b1 = restored.Bays[1];
            Assert.False(b1.FloorBeam);
            Assert.Null(b1.HeightOverride);
            Assert.Single(b1.Levels);
            Assert.Null(b1.Levels[0].BeamLengthOverride);
            Assert.Null(b1.Levels[0].ClearOverride);
            Assert.Equal(5.0, b1.Levels[0].BeamPeralte, 4);
        }

        [Fact]
        public void RoundTrip_PreservesPerPostCabeceras()
        {
            var design = SampleDesign();
            design.PostCabeceras.Add(new RackFrameConfiguration
            {
                Height = 240.0,
                Depth = 42.0,
                LeftBasePlate = new BasePlatePlacement { PostSide = PostSide.Left, PlateCatalogId = "PL", PeralteOverride = 6.5 }
            });
            design.PostCabeceras.Add(null); // this post uses the run default

            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "Rack A"))).ToDomain();

            Assert.Equal(2, restored.PostCabeceras.Count);
            Assert.NotNull(restored.PostCabeceras[0]);
            Assert.True(restored.PostCabeceras[0].LeftBasePlate.PeralteOverride.HasValue);
            Assert.Equal(6.5, restored.PostCabeceras[0].LeftBasePlate.PeralteOverride.Value, 4);
            Assert.Null(restored.PostCabeceras[1]);
        }
    }
}
