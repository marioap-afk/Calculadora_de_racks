using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18a — FIXED golden signatures (finding 6). The signature captures View, Role, PieceId, BlockName, Insertion,
    /// ConnectionAnchor, RotationRadians, MirroredX/Y and the ordered dynamic params (LONGITUD/PERALTE/SAQUE/…). Any
    /// change to a coordinate, mirror, peralte, length, rotation, anchor or quantity breaks the SHA-256 pin. The rich
    /// scenario has two fronts with different fondos/DepthStartPosition, two levels with different rear peraltes, one
    /// deactivated tope cell and a Both safety selection (materialized low-only).
    /// </summary>
    public class PushBackGoldenTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackSystem Scenario(RackCatalog catalog)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 });
            var f0 = new PushBackFrontConfig(); f0.HighEndBeamPeraltes.Add(5.0); f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            design.RearTope.Disable(0, 0);
            design.Structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1, Side = SafetySide.Both });
            return new PushBackResolver(catalog).Resolve(design);
        }

        private static string Row(HeaderBlockInstance i)
            => FormattableString.Invariant(
                $"{i.View}|{i.Role}|{i.PieceId}|{i.BlockName}|{i.Insertion.X:0.####}|{i.Insertion.Y:0.####}|{i.ConnectionAnchor.X:0.####}|{i.ConnectionAnchor.Y:0.####}|{i.RotationRadians:0.######}|{(i.MirroredX ? 1 : 0)}|{(i.MirroredY ? 1 : 0)}|{Params(i)}");

        private static string Params(HeaderBlockInstance i)
            => string.Join(",", i.DynamicParameters.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => FormattableString.Invariant($"{p.Key}={p.Value:0.####}")));

        private static string PlanSignature(DynamicSystemPlan plan)
            => string.Join("\n", plan.Flatten().Instances.Select(Row).OrderBy(s => s, StringComparer.Ordinal));

        private static string BomSignature(BillOfMaterials bom)
            => string.Join("\n", bom.Components
                .Select(c => FormattableString.Invariant($"{c.Category}|{c.ProfileId}|{c.Length:0.####}|{c.Quantity}|{c.Pieces.Count}"))
                .OrderBy(s => s, StringComparer.Ordinal));

        private static string Sha(string content)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        private static Dictionary<string, string> Signatures()
        {
            var catalog = Catalog;
            var system = Scenario(catalog);
            var lateral = new PushBackSystemLateralBuilder();
            var frontal = new PushBackSystemFrontalBuilder();
            return new Dictionary<string, string>
            {
                ["lateral"] = Sha(PlanSignature(lateral.Build(system, catalog))),
                ["lateral-corte0"] = Sha(PlanSignature(lateral.Build(system, catalog, 0))),
                ["frontal-entrada"] = Sha(PlanSignature(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida))),
                ["frontal-posterior"] = Sha(PlanSignature(frontal.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior))),
                ["planta"] = Sha(PlanSignature(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog))),
                ["bom"] = Sha(BomSignature(PushBackBomBuilder.Build(system, catalog)))
            };
        }

        // FIXED pins (SHA-256 of the detailed signature). Regenerate ONLY on an intended geometry/BOM change.
        private static readonly IReadOnlyDictionary<string, string> Expected = new Dictionary<string, string>
        {
            // I-18b round 1 of the Owner's manual-gate rejection: the three pins carrying the REAR TOPE moved on purpose
            // (PB-VAL-02 orientation + PB-VAL-03 the exact 4" rise). frontal-entrada (no rear tope) and planta (top view,
            // no elevation, keeps the beam's plan mirror) are UNCHANGED, which is what bounds the correction.
            // Previous: lateral/lateral-corte0 FB9C83F6…, frontal-posterior A2FC3231…
            // Round 2 (PB-VAL-05, low-beam tangency onto the bed-origin line) moves ONLY the two lateral pins again —
            // LowBeams is lateral-only, so frontal-entrada, frontal-posterior, planta and the BOM stay put.
            // Previous (round 1): lateral/lateral-corte0 E3E3EA9F…
            //
            // OWNER RETEST of PB-VAL-02 (the stop was still inverted): the rear tope now takes its world anchor from the
            // rear beam's measured load-side contact point (lateral only, the view seen along the depth) and faces the
            // LOAD side in both elevations. So exactly THREE pins move: the two laterals (anchor + orientation) and
            // frontal-posterior (orientation). frontal-entrada carries no rear tope, planta keeps the beam's plan mirror
            // and the BOM counts the same pieces with the same SAQUE/LONGITUD — those three are UNCHANGED, which is what
            // bounds this correction. Previous: lateral/lateral-corte0 4C22005A…, frontal-posterior 67511108…
            ["lateral"] = "110DB452132A58A933839AE6159403C35B192D9ADC4EF78D4D532A9F3278915F",
            ["lateral-corte0"] = "110DB452132A58A933839AE6159403C35B192D9ADC4EF78D4D532A9F3278915F",
            ["frontal-entrada"] = "C652265C592E4834A976C6E03ABC1282FA353E861DBF8A5AEC4F7C3E3CCE3974",
            ["frontal-posterior"] = "1DA69F5EA299624C75B1F69BBEAF758ED63CEF2FA64B49AB8EE4A8E0D456C3D6",
            ["planta"] = "33A87C650DF93AAF45E1F600B348E515E4D1379510E9095A4C6564E3F766E82C",
            // BOM pin updated by the length-coherence fix (rear tope LONGITUD = beamLength + LengthAllowance; end beams
            // per cell). The FIVE view pins are UNCHANGED (with no per-level override the cell length equals the front
            // length). Previous BOM hash: 139C18EFDD0BCF1DBC9CABB867E3C40499B2BD264E1BED4F4CBC7DCEE74C57AC.
            ["bom"] = "057C6D2D30548D4F8FE65F1DA38678D0588792C2A65B43CD23CE4F8B7ECC59A3"
        };

        [Fact]
        public void Golden_AllSixSignatures_MatchTheFixedPins()
        {
            var actual = Signatures();
            var diff = Expected.Where(kv => actual[kv.Key] != kv.Value)
                .Select(kv => $"{kv.Key}: expected {kv.Value} actual {actual[kv.Key]}")
                .ToList();
            Assert.True(diff.Count == 0, "golden mismatch:\n" + string.Join("\n", diff));
        }
    }
}
