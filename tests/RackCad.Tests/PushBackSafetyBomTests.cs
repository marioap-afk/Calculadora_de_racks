using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18a corrective run — the single low-end safety authority (finding 4) and the per-cell IN/OUT BOM (finding 3):
    /// a "Both" selection materializes ONLY on the low (entrance/exit) end and is counted once physically; the low IN/OUT
    /// beam is resolved per cell, never from a global field.
    /// </summary>
    public class PushBackSafetyBomTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string Bota = "PROTECTOR_BOTA_H_3_16_18";

        private static DynamicRackDesign BaseStructure()
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        // ---- Finding 3: BOM low IN/OUT resolved per cell ----------------------------------------------------

        [Fact]
        public void Bom_LowInOut_UsesTheResolvedCellPeralteAndLength()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = BaseStructure() });
            var bom = PushBackBomBuilder.Build(system, catalog);

            var front = system.Structure.Fronts[0];
            var cell = DynamicRackLevelGeometry.At(system.Structure, front, 1);   // resolved cell (level 1)
            var inOut = bom.Components.Where(c => c.Category == SystemBomBuilder.InOutBeam).ToList();

            Assert.NotEmpty(inOut);
            // Length and peralte come from the per-cell resolution (DynamicRackLevelGeometry.At), not a global field.
            Assert.All(inOut, c => Assert.Equal(cell.BeamLength > 0.0 ? cell.BeamLength : front.BeamLength, c.Length, 3));
            Assert.Contains(inOut, c => c.Description.Contains(FormattableString.Invariant($"{cell.InOutBeamDepth:0.##}")));
            Assert.All(inOut, c => Assert.Equal(cell.InOutBeamCatalogId, c.ProfileId));
        }

        // ---- Finding 4: single low-end safety authority ----------------------------------------------------

        [Fact]
        public void Safety_BothSelection_MaterializesOnlyLow_NeverRear_CountedOncePhysically()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = Bota, Quantity = 1, Side = SafetySide.Both });

            var system = new PushBackResolver(catalog).Resolve(design);

            // The resolver restricts the authorized selection to the low (Left/exit) end.
            var bota = Assert.Single(system.SafetySelections, s => s.ElementId == Bota);
            Assert.Equal(SafetySide.Left, bota.Side);

            var frontal = new PushBackSystemFrontalBuilder();
            var entrada = frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances;
            var posterior = frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;
            var lateral = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;
            var planta = new PushBackSystemPlantaBuilder().Build(system, catalog);

            Assert.Contains(entrada, i => i.PieceId == Bota);                          // low cut has it
            Assert.Contains(lateral, i => i.PieceId == Bota);                          // lateral (low-only) not empty
            Assert.DoesNotContain(posterior, i => i.Role == HeaderBlockRole.Safety);   // rear cut: no normal safety
            // BOM carries the bota and counts the LOW physical quantity (a Both selection is NOT doubled).
            var botaLines = PushBackBomBuilder.Build(system, catalog).Lines.Where(l => (l.ProfileId ?? string.Empty).Contains("BOTA")).ToList();
            Assert.NotEmpty(botaLines);

            // A rear frontal cut never carries a bota; planta's botas are the low projection (not empty here).
            Assert.DoesNotContain(posterior, i => i.PieceId == Bota);
            Assert.Contains(planta, i => i.PieceId == Bota);
        }

        [Fact]
        public void Safety_Guia_NeverAppears_InAnyViewOrBom()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1, Side = SafetySide.Both });

            var system = new PushBackResolver(catalog).Resolve(design);
            var frontal = new PushBackSystemFrontalBuilder();

            bool NoGuia(IEnumerable<HeaderBlockInstance> instances) => !instances.Any(i => (i.PieceId ?? string.Empty).Contains("GUIA"));

            Assert.True(NoGuia(new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances));
            Assert.True(NoGuia(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances));
            Assert.True(NoGuia(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances));
            Assert.True(NoGuia(new PushBackSystemPlantaBuilder().Build(system, catalog)));
            Assert.DoesNotContain(PushBackBomBuilder.Build(system, catalog).Lines, l => (l.ProfileId ?? string.Empty).Contains("GUIA"));
        }
    }
}
