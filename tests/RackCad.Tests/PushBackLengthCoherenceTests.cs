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
    /// I-18a final corrective run — beam-length coherence: the low IN/OUT and the high TROQUEL_REDONDO of a cell share
    /// the SAME per-front×level transverse length (never front.BeamLength for every level), and the rear tope's LONGITUD
    /// is that length + <see cref="SelectiveTopePlacement.LengthAllowance"/> in every view and in the BOM.
    /// </summary>
    public class PushBackLengthCoherenceTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string InOut = "LARGUERO_IN_OUT_C6";
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";
        private const string Tope = "LARGUERO_ESCALON_TOPE_DE_3";

        // ---- Finding: end-beam length is per cell (two levels with different BeamLengthOverride) --------------

        [Fact]
        public void EndBeams_LengthIsPerCell_BomAndLateralAgreePerLevel()
        {
            var catalog = Catalog;
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            var front = new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 };
            front.Levels.Add(new DynamicRackLevelDesign { BeamLengthOverride = 100.0 });   // level 1
            front.Levels.Add(new DynamicRackLevelDesign { BeamLengthOverride = 120.0 });   // level 2
            design.Structure.Fronts.Add(front);

            var system = new PushBackResolver(catalog).Resolve(design);
            var bom = PushBackBomBuilder.Build(system, catalog);

            // BOM IN/OUT and TROQUEL_REDONDO both carry BOTH lengths, one each.
            var inOutLengths = bom.Components.Where(c => c.Category == SystemBomBuilder.InOutBeam)
                .Select(c => Math.Round(c.Length, 3)).OrderBy(v => v).ToList();
            var redondoLengths = bom.Components.Where(c => c.Category == PushBackBomBuilder.HighEndBeam)
                .Select(c => Math.Round(c.Length, 3)).OrderBy(v => v).ToList();
            Assert.Equal(new List<double> { 100.0, 120.0 }, inOutLengths);
            Assert.Equal(new List<double> { 100.0, 120.0 }, redondoLengths);
            Assert.All(bom.Components.Where(c => c.Category == SystemBomBuilder.InOutBeam || c.Category == PushBackBomBuilder.HighEndBeam),
                c => Assert.Equal(1, c.Quantity));

            // The lateral geometry (sectioned at the front's post) agrees per level: high beam LONGITUD == the BOM lengths.
            var highLongitudes = new PushBackSystemLateralBuilder().Build(system, catalog, 0).Flatten().Instances
                .Where(i => i.PieceId == Redondo)
                .Select(i => Math.Round(i.DynamicParameters[SelectiveRackDefaults.LengthParam], 3))
                .OrderBy(v => v).ToList();
            Assert.Equal(new List<double> { 100.0, 120.0 }, highLongitudes);
        }

        // ---- Finding: rear tope LONGITUD = beamLength + LengthAllowance in every view and the BOM -------------

        [Fact]
        public void RearTope_Longitud_IsBeamLengthPlusAllowance_AcrossViewsAndBom()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            });

            var front = system.Structure.Fronts[0];
            var expected = Math.Round(PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1) + SelectiveTopePlacement.LengthAllowance, 3);

            var frontal = new PushBackSystemFrontalBuilder();
            var lateralTopes = TopeLongitudes(new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances);
            var frontalTopes = TopeLongitudes(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances);
            var plantaTopes = TopeLongitudes(new PushBackSystemPlantaBuilder().Build(system, catalog));

            Assert.NotEmpty(lateralTopes);
            Assert.NotEmpty(frontalTopes);
            Assert.NotEmpty(plantaTopes);
            Assert.All(lateralTopes, l => Assert.Equal(expected, l, 3));
            Assert.All(frontalTopes, l => Assert.Equal(expected, l, 3));
            Assert.All(plantaTopes, l => Assert.Equal(expected, l, 3));

            // The BOM rear-tope component uses exactly the same length.
            var bomTope = PushBackBomBuilder.Build(system, catalog).Components
                .Where(c => c.Category == PushBackBomBuilder.RearTope).ToList();
            Assert.NotEmpty(bomTope);
            Assert.All(bomTope, c => Assert.Equal(expected, Math.Round(c.Length, 3), 3));
        }

        private static IReadOnlyList<double> TopeLongitudes(IEnumerable<HeaderBlockInstance> instances)
            => instances.Where(i => i.Role == HeaderBlockRole.Tope && i.PieceId == Tope
                    && i.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam))
                .Select(i => Math.Round(i.DynamicParameters[SelectiveRackDefaults.LengthParam], 3))
                .ToList();
    }
}
