using System;
using System.Collections.Generic;
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
    /// PB-005 (I-32) — the rear stop's PIECE is chosen from the catalog instead of being a compile-time constant. One
    /// rule (<see cref="PushBackRearTopeBuilder.ResolvePieceId"/>) answers it for the three views and the BOM, so they
    /// cannot disagree, and it falls back to the historical piece for a blank or unknown id — a rack must never end up
    /// with no stop at all because a catalog row was renamed.
    /// </summary>
    public class PushBackRearTopePieceTests
    {
        private const string Alternative = "POSTE_3_1_5_8_TOPE";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign BaseStructure() => new DynamicRackDesign
        {
            Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
            PalletsDeep = 4,
            LoadLevels = 2,
            FirstLevelHeight = 6.0,
            BeamDepth = 4.0
        };

        private static PushBackSystem System(RackCatalog catalog, string pieceId)
        {
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.PieceId = pieceId;
            return new PushBackResolver(catalog).Resolve(design);
        }

        private static string[] TopePieces(IReadOnlyList<HeaderBlockInstance> instances)
            => instances.Where(i => i.Role == HeaderBlockRole.Tope).Select(i => i.PieceId).Distinct().ToArray();

        // ---- The rule ----

        [Fact]
        public void ResolvePieceId_TakesTheChoice_AndFallsBackForBlankOrUnknown()
        {
            var catalog = Catalog;
            Assert.Equal(Alternative, PushBackRearTopeBuilder.ResolvePieceId(catalog, new PushBackRearTopeConfig { PieceId = Alternative }));
            Assert.Equal(PushBackRearTopeBuilder.TopePieceId, PushBackRearTopeBuilder.ResolvePieceId(catalog, new PushBackRearTopeConfig()));
            Assert.Equal(PushBackRearTopeBuilder.TopePieceId, PushBackRearTopeBuilder.ResolvePieceId(catalog, new PushBackRearTopeConfig { PieceId = "  " }));
            Assert.Equal(PushBackRearTopeBuilder.TopePieceId, PushBackRearTopeBuilder.ResolvePieceId(catalog, new PushBackRearTopeConfig { PieceId = "NO_EXISTE" }));

            // A real catalog id of ANOTHER family is not a stop: it must fall back, never be placed.
            Assert.Equal(PushBackRearTopeBuilder.TopePieceId, PushBackRearTopeBuilder.ResolvePieceId(catalog, new PushBackRearTopeConfig { PieceId = "PROTECTOR_BOTA_H_3_16_18" }));
        }

        // ---- The choice reaches the three views AND the BOM ----

        [Fact]
        public void ChosenPiece_ReachesLateralFrontalPlantaAndBom()
        {
            var catalog = Catalog;
            var system = System(catalog, Alternative);
            var front = system.Structure.Fronts[0];

            var lateral = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            var frontal = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;
            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;
            var bom = PushBackBomBuilder.Build(system, catalog);

            Assert.NotEmpty(lateral);
            Assert.Equal(new[] { Alternative }, lateral.Select(i => i.PieceId).Distinct().ToArray());
            Assert.Equal(new[] { Alternative }, TopePieces(frontal));
            Assert.Equal(new[] { Alternative }, TopePieces(planta));

            var topeComponents = bom.Components.Where(c => c.Category == PushBackBomBuilder.RearTope).ToList();
            Assert.NotEmpty(topeComponents);
            Assert.All(topeComponents, component => Assert.Equal(Alternative, component.ProfileId));
        }

        [Fact]
        public void NoChoice_KeepsTheHistoricalPiece_Everywhere()
        {
            var catalog = Catalog;
            var system = System(catalog, null);
            var front = system.Structure.Fronts[0];

            var lateral = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;
            var bom = PushBackBomBuilder.Build(system, catalog);

            Assert.Equal(new[] { PushBackRearTopeBuilder.TopePieceId }, lateral.Select(i => i.PieceId).Distinct().ToArray());
            Assert.Equal(new[] { PushBackRearTopeBuilder.TopePieceId }, TopePieces(planta));
            Assert.All(
                bom.Components.Where(c => c.Category == PushBackBomBuilder.RearTope),
                component => Assert.Equal(PushBackRearTopeBuilder.TopePieceId, component.ProfileId));
        }

        // ---- Persistence: round-trip + legacy fallback + I-11 metadata ----

        [Fact]
        public void PieceId_SurvivesTheDocumentRoundTrip()
        {
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.PieceId = Alternative;

            var restored = PushBackDesignDocument.FromDomain(design).ToDomain();
            Assert.Equal(Alternative, restored.RearTope.PieceId);
        }

        [Fact]
        public void LegacyDocumentWithoutTheField_LoadsAsTheDefaultAndIsNotRewritten()
        {
            // A document written before PB-005: the property is simply absent.
            var legacy = PushBackDesignDocument.FromDomain(new PushBackDesign { Structure = BaseStructure() });
            Assert.Null(legacy.RearTopePieceId);

            var design = legacy.ToDomain();
            Assert.Null(design.RearTope.PieceId);
            Assert.Equal(PushBackRearTopeBuilder.TopePieceId, PushBackRearTopeBuilder.ResolvePieceId(Catalog, design.RearTope));

            // Re-saving an untouched legacy rack does not start writing the field.
            Assert.Null(PushBackDesignDocument.FromDomain(design, legacy).RearTopePieceId);
        }

        [Fact]
        public void PieceId_SurvivesDeepCopyResolveAndSnapshot()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.PieceId = Alternative;

            Assert.Equal(Alternative, design.RearTope.DeepCopy().PieceId);

            var resolver = new PushBackResolver(catalog);
            var system = resolver.Resolve(design);
            Assert.Equal(Alternative, system.RearTope.PieceId);
            Assert.Equal(Alternative, resolver.Snapshot(system).RearTope.PieceId);
        }

        // ---- Editor state: the five boundaries ----

        [Fact]
        public void EditorState_CarriesThePieceThroughConfigLoadSnapshotAndDesign()
        {
            var catalog = Catalog;
            var state = new PushBackEditorState();
            state.LoadNew();

            var config = state.RearTopeConfig();
            config.PieceId = Alternative;
            state.LoadRearTopeConfig(config);
            Assert.Equal(Alternative, state.RearTopePieceId);
            Assert.Equal(Alternative, state.RearTopeConfig().PieceId);

            var snapshot = state.Snapshot();
            state.RearTopePieceId = null;
            state.Restore(snapshot);
            Assert.Equal(Alternative, state.RearTopePieceId);

            var design = new PushBackEditorDesignAssembler(catalog).BuildDesign(state, PushBackEditorInputs.NewDesign());
            Assert.Equal(Alternative, design.RearTope.PieceId);
        }

        [Fact]
        public void NewRack_StartsOnTheDefaultPiece_NotThePreviousRacks()
        {
            var catalog = Catalog;
            var state = new PushBackEditorState();
            state.LoadNew();
            var config = state.RearTopeConfig();
            config.PieceId = Alternative;
            state.LoadRearTopeConfig(config);

            state.LoadNew();   // a brand-new rack in the same window
            Assert.Null(state.RearTopePieceId);
        }

        [Fact]
        public void LoadedRack_RecoversItsOwnPiece()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.PieceId = Alternative;

            var state = new PushBackEditorState();
            state.LoadFromDesign(design, new PushBackResolver(catalog));
            Assert.Equal(Alternative, state.RearTopePieceId);
        }
    }
}
