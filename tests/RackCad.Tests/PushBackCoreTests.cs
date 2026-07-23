using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18a — pure Push Back core: it REUSES the dynamic structure (headers/separators/derived posts, fronts with
    /// different fondo counts and DepthStartPosition, the 7/16"/ft slope and the 2" troquel snap the dynamic resolver
    /// already applies) and diverges only where Push Back is LIFO: the explicit high-end beam peralte, the full-span
    /// bed (no −4"), the pushback bed (no brakes), and the rear pallet-stop (active by default, deactivable per cell).
    /// Persistence round-trips the design and preserves unknown fields + schema version (I-11).
    /// </summary>
    public class PushBackCoreTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign Structure(int palletsDeep = 4, int loadLevels = 3)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep,
                LoadLevels = loadLevels,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        private static PushBackDesign Design() => new PushBackDesign { Structure = Structure() };

        // ---- Defaults ---------------------------------------------------------------------------------------

        [Fact]
        public void Defaults_HighEndBeam_IsTroquelRedondo_WithExplicit35Default()
        {
            Assert.Equal("LARGUERO_ESCALON_TROQUEL_REDONDO", PushBackDefaults.HighEndBeamCatalogId);
            Assert.Equal(3.5, PushBackDefaults.HighEndBeamDefaultPeralte);
            Assert.Equal("INICIO_IZQUIERDO", PushBackDefaults.HighEndBeamLeftBedMatePoint);
            Assert.Equal("INICIO_DERECHO", PushBackDefaults.HighEndBeamRightBedMatePoint);
            // A fresh design already carries the explicit default.
            Assert.Equal(3.5, new PushBackDesign().HighEndBeamPeralte);
        }

        // ---- Rear tope: active by default, deactivable per cell, persist deactivations only ------------------

        [Fact]
        public void RearTope_IsActiveEverywhereByDefault_WithoutAnyStoredPositiveCells()
        {
            var tope = new PushBackRearTopeConfig();

            Assert.Empty(tope.OffCells);                 // no positive list persisted
            Assert.True(tope.At(0, 0));
            Assert.True(tope.At(5, 2));                    // every (front, level) is active
            Assert.Equal(PushBackDefaults.RearTopeSaque, tope.Saque);
        }

        [Fact]
        public void RearTope_Disable_TurnsOffOneCellAndPersistsOnlyThatDeactivation()
        {
            var tope = new PushBackRearTopeConfig();

            tope.Disable(1, 2);

            Assert.False(tope.At(1, 2));
            Assert.True(tope.At(1, 1));                     // neighbours stay active
            var cell = Assert.Single(tope.OffCells);
            Assert.Equal(1, cell.Frente);
            Assert.Equal(2, cell.Level);

            tope.Disable(1, 2);                             // idempotent
            Assert.Single(tope.OffCells);
        }

        [Fact]
        public void RearTope_DeepCopy_IsIndependent()
        {
            var tope = new PushBackRearTopeConfig { Saque = 4.0 };
            tope.Disable(0, 0);

            var copy = tope.DeepCopy();
            copy.Disable(2, 2);

            Assert.Equal(4.0, copy.Saque);
            Assert.False(copy.At(0, 0));
            Assert.True(tope.At(2, 2));                     // mutating the copy does not touch the original
            Assert.Single(tope.OffCells);
        }

        // ---- Resolver: explicit 3.5 high-end peralte (NOT the first catalog row) -----------------------------

        [Fact]
        public void Resolver_HighEndPeralte_DefaultsToExplicit35_NotTheFirstCatalogValue()
        {
            var resolver = new PushBackResolver(Catalog);
            var allowed = resolver.AllowedHighEndPeraltes();

            Assert.Contains(3.0, allowed);                  // the catalog's FIRST value is 3.0…
            Assert.Equal(3.0, allowed[0], 4);
            Assert.Equal(3.5, resolver.ResolveHighEndBeamPeralte(0.0), 4);   // …but the default is 3.5
        }

        [Theory]
        [InlineData(5.0, 5.0)]     // a valid requested value is honoured
        [InlineData(4.5, 4.5)]
        [InlineData(7.0, 3.5)]     // an invalid value falls back to the explicit default
        [InlineData(0.0, 3.5)]     // unset falls back to the explicit default
        public void Resolver_HighEndPeralte_HonoursValidRequestElseFallsBackTo35(double requested, double expected)
        {
            var resolver = new PushBackResolver(Catalog);
            Assert.Equal(expected, resolver.ResolveHighEndBeamPeralte(requested), 4);
        }

        // ---- Resolver: reuses the dynamic structure (slope, elevations, name) --------------------------------

        [Fact]
        public void Resolver_ReusesDynamicStructure_WithSlopeRaisedEntranceElevations()
        {
            var resolver = new PushBackResolver(Catalog);

            var system = resolver.Resolve(Design());

            Assert.NotNull(system.Structure);
            Assert.NotEmpty(system.Structure.Modules);                       // headers + separators resolved
            Assert.NotEmpty(system.Fronts);
            Assert.Equal(3, system.Structure.LoadBeamLevels.Count);
            // The dynamic resolver already applies the 7/16"/ft slope: entrance (high) is above exit (low).
            var level0 = system.Structure.LoadBeamLevels[0];
            Assert.True(level0.EntranceElevation > level0.ExitElevation);
            Assert.Equal(PushBackDefaults.HighEndBeamCatalogId, system.HighEndBeamCatalogId);
            Assert.Equal(3.5, system.HighEndBeamPeralte, 4);
        }

        [Fact]
        public void Resolver_Name_PassesThroughToTheSharedStructure()
        {
            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(Design());

            system.Name = "PB-1";

            Assert.Equal("PB-1", system.Structure.Name);
        }

        [Fact]
        public void Resolver_MultiFront_DifferentLengths_HonourDepthStartPositionAndDistinctRanges()
        {
            var design = new PushBackDesign { Structure = Structure(palletsDeep: 6) };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, PalletsDeep = 3, DepthStartPosition = 4 });

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(2, system.Fronts.Count);
            Assert.Equal(1, system.Fronts[0].DepthStartPosition);
            Assert.Equal(4, system.Fronts[1].DepthStartPosition);
            // Fronts of different length share the base structure but resolve to distinct longitudinal ranges.
            Assert.NotEqual(system.Fronts[0].EndX - system.Fronts[0].StartX,
                            system.Fronts[1].EndX - system.Fronts[1].StartX);
        }

        // ---- Bed: full structural span (no −4") and pushback (no brakes) -------------------------------------

        [Fact]
        public void Bed_Length_IsTheFullStructuralSpan_WithoutTheDynamicMinus4Clearance()
        {
            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(Design());

            var pushBackLength = PushBackFlowBedLateralBuilder.ResolveBedLength(system);
            var dynamicLength = DynamicFlowBedGeometry.ResolveBedLength(system.Structure);

            Assert.Equal(system.TotalLength, pushBackLength, 4);                              // full span
            Assert.Equal(DynamicRackDefaults.FlowBedLengthClearance, pushBackLength - dynamicLength, 4); // exactly the 4" the dynamic bed drops
        }

        [Fact]
        public void Bed_LocalAssembly_UsesPushbackWithNoBrakes_AndRailLongitudEqualsTheFullSpan()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(Design());

            var assembly = new PushBackFlowBedLateralBuilder().BuildLocalAssembly(system, catalog);

            Assert.DoesNotContain(assembly, instance => instance.Role == HeaderBlockRole.Brake); // pushback: NO frenos
            Assert.Contains(assembly, instance => instance.Role == HeaderBlockRole.Rail);
            Assert.Contains(assembly, instance => instance.Role == HeaderBlockRole.Stop);
            Assert.Contains(assembly, instance => instance.Role == HeaderBlockRole.Roller);

            var rail = assembly.Single(instance => instance.Role == HeaderBlockRole.Rail);
            Assert.Equal(system.TotalLength, rail.DynamicParameters[SelectiveRackDefaults.LengthParam], 4);
        }

        // ---- Persistence: round-trip, legacy, unknown-field preservation (I-11) -------------------------------

        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = false };

        [Fact]
        public void Persistence_RoundTrip_PreservesStructureHighEndBeamAndRearTope()
        {
            var design = Design();
            design.HighEndBeamPeralte = 5.0;
            design.RearTope.Saque = 4.0;
            design.RearTope.Disable(0, 1);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design), Json);
            var back = JsonSerializer.Deserialize<PushBackDesignDocument>(json, Json).ToDomain();

            Assert.Equal(4, back.Structure.PalletsDeep);
            Assert.Equal(3, back.Structure.LoadLevels);
            Assert.Equal(5.0, back.HighEndBeamPeralte, 4);
            Assert.Equal(4.0, back.RearTope.Saque, 4);
            Assert.False(back.RearTope.At(0, 1));
            Assert.True(back.RearTope.At(0, 0));
        }

        [Fact]
        public void Persistence_LegacyJson_WithoutSchemaVersionOrPushBackFields_LoadsWithDefaults()
        {
            // Only the shared structure was written (a legacy/minimal document): push-back fields absent.
            var structureJson = JsonSerializer.Serialize(
                DynamicRackSystemDocument.From(Structure()), Json);
            var legacy = "{\"Structure\":" + structureJson + "}";

            var design = JsonSerializer.Deserialize<PushBackDesignDocument>(legacy, Json).ToDomain();

            Assert.Equal(4, design.Structure.PalletsDeep);
            Assert.Equal(3.5, design.HighEndBeamPeralte, 4);          // explicit default
            Assert.Equal(PushBackDefaults.RearTopeSaque, design.RearTope.Saque, 4);
            Assert.Empty(design.RearTope.OffCells);                    // active everywhere
        }

        [Fact]
        public void Persistence_UnknownTopLevelField_SurvivesLoadAndSave_ViaExtensionData()
        {
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(Design()), Json);
            var node = JsonNode.Parse(json).AsObject();
            node["futureField"] = "keep-me";                          // a field a newer build might add

            var reserialized = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<PushBackDesignDocument>(node.ToJsonString(), Json), Json);

            Assert.Contains("futureField", reserialized);
            Assert.Contains("keep-me", reserialized);
        }

        [Fact]
        public void Persistence_WriteVersion_IsTheCurrentSchemaVersion()
        {
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(Design()), Json);
            using var parsed = JsonDocument.Parse(json);
            Assert.Equal(PushBackDesignDocument.CurrentSchemaVersion, parsed.RootElement.GetProperty("SchemaVersion").GetString());
        }
    }
}
