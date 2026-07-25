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
            // OWNER DECISIONS 2026-07-24 (they SUPERSEDE the previous rules). Three changes land on the LATERAL and one
            // on the rear frontal:
            //  * the LOW IN/OUT beam no longer carries any displacement — it is bolted where its TROQUEL_CAMA meets the
            //    rail's TROQUEL_IN, so it returns to the resolver's snapped exit elevation;
            //  * the REAR beam is the one that now drops onto the bed-origin line, tangent at its measured contact edge;
            //  * the rear TOPE anchors on the POST's TROQUEL_SEPARADOR axis and its orientation is INVERTED.
            // The orientation flip alone is what moves frontal-posterior, and it returns EXACTLY to the pin it had before
            // the previous run (67511108…) — the anchor there is the beam's transverse datum, which did not change.
            // frontal-entrada (no rear tope, and the low beam was never shifted in that view), planta and the BOM are
            // UNCHANGED. Previous: lateral/lateral-corte0 110DB452…, frontal-posterior 1DA69F5E…
            // OWNER DECISION 2026-07-24 (final) on the rear tope's ANCHOR POINT PER VIEW, audited against the real rows
            // of connection-layout.csv. Four pins move, all of them for the stop and only for it:
            //  * the two LATERALS: the elevation grid is now measured from the post's TROQUEL_SEPARADOR in the LATERAL
            //    view. It used to be read from TROQUEL_LARGUERO in the FRONTAL view — the wrong point AND the wrong
            //    view (the post does not even publish TROQUEL_LARGUERO in LATERAL, so the grid was silently based at 0);
            //  * frontal-posterior: anchored by the post's own TROQUEL_TOPE in FRONTAL, whose X follows the post PERALTE;
            //  * planta: anchored by TROQUEL_TOPE in PLANTA, and the block's orientation inverted.
            // frontal-entrada carries no rear tope and the BOM counts the same pieces with the same SAQUE/LONGITUD, so
            // both stay UNCHANGED — which is what bounds this correction to the stop.
            // Previous: lateral/lateral-corte0 67F63860…, frontal-posterior 67511108…, planta 33A87C65…
            ["lateral"] = "17815678B5C9D9E0D1D9ADC7DBEB717F5A3843E36956136828489391C3B7B364",
            ["lateral-corte0"] = "17815678B5C9D9E0D1D9ADC7DBEB717F5A3843E36956136828489391C3B7B364",
            ["frontal-entrada"] = "C652265C592E4834A976C6E03ABC1282FA353E861DBF8A5AEC4F7C3E3CCE3974",
            ["frontal-posterior"] = "5553A6C1B660EEA3DC8CDA368A71A6CADC83AE8D424CAD1994FF317815E8BD90",
            ["planta"] = "666BBD2B40B32E8B9707A7D801F9F90C7C91AB889EFC5BD37FAA1EE019D90558",
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
