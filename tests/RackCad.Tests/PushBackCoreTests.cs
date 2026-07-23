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
    /// already applies) and diverges only where Push Back is LIFO: the high-end beam peralte PER FRONT AND LEVEL, the
    /// full-span bed (no −4"), the pushback bed (no brakes), the rear pallet-stop (active by default, deactivable per
    /// cell), and NO entrance guides. Persistence round-trips the design and preserves unknown fields + a non-downgraded
    /// schema version (I-11).
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
            Assert.Equal(3.5, new PushBackDesign().LegacyHighEndBeamPeralte);   // legacy fallback carries the explicit default
        }

        // ---- Rear tope: active by default, deactivable per cell, persist deactivations only ------------------

        [Fact]
        public void RearTope_IsActiveEverywhereByDefault_WithoutAnyStoredPositiveCells()
        {
            var tope = new PushBackRearTopeConfig();

            Assert.Empty(tope.OffCells);
            Assert.True(tope.At(0, 0));
            Assert.True(tope.At(5, 2));
            Assert.Equal(PushBackDefaults.RearTopeSaque, tope.Saque);
        }

        [Fact]
        public void RearTope_Disable_TurnsOffOneCellAndPersistsOnlyThatDeactivation()
        {
            var tope = new PushBackRearTopeConfig();

            tope.Disable(1, 2);

            Assert.False(tope.At(1, 2));
            Assert.True(tope.At(1, 1));
            var cell = Assert.Single(tope.OffCells);
            Assert.Equal(1, cell.Frente);
            Assert.Equal(2, cell.Level);

            tope.Disable(1, 2);
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
            Assert.True(tope.At(2, 2));
            Assert.Single(tope.OffCells);
        }

        // ---- Resolver: explicit 3.5 high-end peralte, PER FRONT AND LEVEL -----------------------------------

        [Fact]
        public void Resolver_HighEndPeralte_DefaultsToExplicit35_NotTheFirstCatalogValue()
        {
            var resolver = new PushBackResolver(Catalog);
            var allowed = resolver.AllowedHighEndPeraltes();

            Assert.Contains(3.0, allowed);
            Assert.Equal(3.0, allowed[0], 4);                                  // the catalog's FIRST value is 3.0…
            Assert.Equal(3.5, resolver.ResolveHighEndBeamPeralte(0.0), 4);      // …but the default is 3.5
        }

        [Theory]
        [InlineData(5.0, 5.0)]
        [InlineData(4.5, 4.5)]
        [InlineData(7.0, 3.5)]     // invalid -> explicit default
        [InlineData(0.0, 3.5)]     // unset -> explicit default
        public void Resolver_HighEndPeralte_HonoursValidRequestElseFallsBackTo35(double requested, double expected)
        {
            var resolver = new PushBackResolver(Catalog);
            Assert.Equal(expected, resolver.ResolveHighEndBeamPeralte(requested), 4);
        }

        [Fact]
        public void Resolver_HighEndPeralte_DiffersBetweenFrontsAndBetweenLevels()
        {
            var design = new PushBackDesign { Structure = Structure(loadLevels: 2) };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });

            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);   // front 0, level 1
            f0.HighEndBeamPeraltes.Add(4.0);   // front 0, level 2  -> differs by LEVEL
            var f1 = new PushBackFrontConfig();
            f1.HighEndBeamPeraltes.Add(6.0);   // front 1, level 1  -> differs by FRONT
            f1.HighEndBeamPeraltes.Add(6.0);
            design.Fronts.Add(f0);
            design.Fronts.Add(f1);

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(5.0, system.HighEndBeamPeralteAt(0, 0), 4);
            Assert.Equal(4.0, system.HighEndBeamPeralteAt(0, 1), 4);            // by LEVEL
            Assert.Equal(6.0, system.HighEndBeamPeralteAt(1, 0), 4);            // by FRONT
            Assert.NotEqual(system.HighEndBeamPeralteAt(0, 0), system.HighEndBeamPeralteAt(0, 1));
            Assert.NotEqual(system.HighEndBeamPeralteAt(0, 0), system.HighEndBeamPeralteAt(1, 0));
        }

        [Fact]
        public void Resolver_HighEndPeralte_CellWithoutValue_FallsBackTo35()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design());   // no per-cell values, legacy = 3.5

            Assert.NotEmpty(system.HighEndBeams);
            Assert.Equal(3.5, system.HighEndBeamPeralteAt(0, 0), 4);
        }

        // ---- Resolver: reuses the dynamic structure ---------------------------------------------------------

        [Fact]
        public void Resolver_ReusesDynamicStructure_WithSlopeRaisedEntranceElevations()
        {
            var resolver = new PushBackResolver(Catalog);

            var system = resolver.Resolve(Design());

            Assert.NotNull(system.Structure);
            Assert.NotEmpty(system.Structure.Modules);
            Assert.NotEmpty(system.Fronts);
            Assert.Equal(3, system.Structure.LoadBeamLevels.Count);
            var level0 = system.Structure.LoadBeamLevels[0];
            Assert.True(level0.EntranceElevation > level0.ExitElevation);       // 7/16"/ft slope, from the dynamic resolver
            Assert.Equal(PushBackDefaults.HighEndBeamCatalogId, system.HighEndBeamCatalogId);
            Assert.Equal(3.5, system.HighEndBeamPeralteAt(0, 0), 4);
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
            Assert.NotEqual(system.Fronts[0].EndX - system.Fronts[0].StartX,
                            system.Fronts[1].EndX - system.Fronts[1].StartX);
        }

        // ---- Push Back admits NO entrance guides ------------------------------------------------------------

        [Fact]
        public void Resolver_StripsEntranceGuides_KeepsOtherSafetyFamilies()
        {
            var design = Design();
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1 });
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1 });

            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(design);

            Assert.True(resolver.IsEntranceGuide(new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA" }));
            Assert.DoesNotContain(system.SafetySelections, s => s.ElementId == "GUIA_ENTRADA");   // no GUIA
            Assert.Contains(system.SafetySelections, s => s.ElementId == "PROTECTOR_BOTA_H_3_16_18"); // bota kept
        }

        // ---- Bed: full structural span (no −4") and pushback (no brakes) -------------------------------------

        [Fact]
        public void Bed_Length_IsTheFullStructuralSpan_WithoutTheDynamicMinus4Clearance()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design());

            var pushBackLength = PushBackFlowBedLateralBuilder.ResolveBedLength(system);
            var dynamicLength = DynamicFlowBedGeometry.ResolveBedLength(system.Structure);

            Assert.Equal(system.TotalLength, pushBackLength, 4);
            Assert.Equal(DynamicRackDefaults.FlowBedLengthClearance, pushBackLength - dynamicLength, 4);
        }

        [Fact]
        public void Bed_LocalAssembly_UsesPushbackWithNoBrakes_AndRailLongitudEqualsTheFullSpan()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(Design());

            var assembly = new PushBackFlowBedLateralBuilder().BuildLocalAssembly(system, catalog);

            Assert.DoesNotContain(assembly, instance => instance.Role == HeaderBlockRole.Brake);   // pushback: NO frenos
            Assert.Contains(assembly, instance => instance.Role == HeaderBlockRole.Rail);
            Assert.Contains(assembly, instance => instance.Role == HeaderBlockRole.Stop);
            Assert.Contains(assembly, instance => instance.Role == HeaderBlockRole.Roller);

            var rail = assembly.Single(instance => instance.Role == HeaderBlockRole.Rail);
            Assert.Equal(system.TotalLength, rail.DynamicParameters[SelectiveRackDefaults.LengthParam], 4);
        }

        // ---- Persistence: round-trip through the DOMAIN, legacy, non-downgrade, unknown fields (I-11) --------

        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { WriteIndented = false };

        [Fact]
        public void Persistence_RoundTrip_PreservesStructurePerCellPeraltesAndRearTope()
        {
            var design = Design();
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            var front = new PushBackFrontConfig();
            front.HighEndBeamPeraltes.Add(5.0);
            front.HighEndBeamPeraltes.Add(null);   // inherit
            design.Fronts.Add(front);
            design.RearTope.Saque = 4.0;
            design.RearTope.Disable(0, 1);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design), Json);
            var back = JsonSerializer.Deserialize<PushBackDesignDocument>(json, Json).ToDomain();

            Assert.Equal(4, back.Structure.PalletsDeep);
            Assert.Equal(5.0, back.Fronts[0].HighEndBeamPeraltes[0]);   // per-cell value survives
            Assert.Null(back.Fronts[0].HighEndBeamPeraltes[1]);          // an inherited (null) cell stays null
            Assert.Equal(4.0, back.RearTope.Saque, 4);
            Assert.False(back.RearTope.At(0, 1));
            Assert.True(back.RearTope.At(0, 0));
        }

        [Fact]
        public void Persistence_LegacyJson_WithoutSchemaVersionOrPushBackFields_LoadsWithDefaults()
        {
            var structureJson = JsonSerializer.Serialize(DynamicRackSystemDocument.From(Structure()), Json);
            var legacy = "{\"Structure\":" + structureJson + "}";

            var design = JsonSerializer.Deserialize<PushBackDesignDocument>(legacy, Json).ToDomain();

            Assert.Equal(4, design.Structure.PalletsDeep);
            Assert.Equal(3.5, design.LegacyHighEndBeamPeralte, 4);
            Assert.Equal(PushBackDefaults.RearTopeSaque, design.RearTope.Saque, 4);
            Assert.Empty(design.Fronts);
            Assert.Empty(design.RearTope.OffCells);
        }

        [Fact]
        public void Persistence_DocumentDomainDocument_PreservesUnknownFieldsAndDoesNotDowngradeAHigherMinor()
        {
            // A newer build wrote SchemaVersion 1.5 (same major, supported) plus a field this build does not know.
            var initial = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(WithPerCell()), Json);
            var node = JsonNode.Parse(initial).AsObject();
            node["SchemaVersion"] = "1.5";
            node["futureField"] = "keep-me";

            // Load -> traverse the DOMAIN -> re-save inheriting the source's metadata.
            var source = JsonSerializer.Deserialize<PushBackDesignDocument>(node.ToJsonString(), Json);
            var design = source.ToDomain();
            var resaved = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design, source), Json);

            using var parsed = JsonDocument.Parse(resaved);
            Assert.Equal("1.5", parsed.RootElement.GetProperty("SchemaVersion").GetString());   // NOT downgraded to 1.0
            Assert.Contains("futureField", resaved);                                            // unknown field survived
            Assert.Contains("keep-me", resaved);
            // The domain value really made the round trip (not just DTO reserialization).
            Assert.Equal(5.0, design.FrontConfig(0).HighEndBeamPeraltes[0]);
        }

        [Fact]
        public void Persistence_FreshWrite_IsCurrentSchemaVersion()
        {
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(Design()), Json);
            using var parsed = JsonDocument.Parse(json);
            Assert.Equal(PushBackDesignDocument.CurrentSchemaVersion, parsed.RootElement.GetProperty("SchemaVersion").GetString());
        }

        private static PushBackDesign WithPerCell()
        {
            var design = Design();
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            var front = new PushBackFrontConfig();
            front.HighEndBeamPeraltes.Add(5.0);
            front.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(front);
            return design;
        }
    }
}
