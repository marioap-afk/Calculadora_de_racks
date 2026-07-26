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
            // PB-004 (I-32, Owner decision 2026-07-25): the bed rises 7/16" per commercial foot, so a 204" rack rises
            // 7.4375" instead of the 11.2" the Owner measured. The high end of the axis is now DERIVED from the
            // troquel-snapped low mate through that one rule, instead of being read from the rear beam's own second,
            // independent snap plus a 4.9342" jump between two different catalog datums. THREE pins move, and only
            // these three, because only the LATERAL frame and the REAR FRONTAL carry elevations that depend on the bed:
            //  * lateral / lateral-corte0: the bed assembly (rotation + anchor), the intermediate supports that are
            //    tangent to the rail-origin line, the rear TROQUEL_REDONDO beam that drops onto that line, and the rear
            //    tope, which keeps its approved rule (rise above the rear larguero, snap to the post's grid, +4") and
            //    therefore follows the larguero down;
            //  * frontal-posterior: the SAME physical rear beam and its stop, now drawn at the SAME elevation as in the
            //    lateral. They differed by 1.18" before this change and would have differed by ~4.9" after it (D14 of
            //    the Owner's AutoCAD matrix demands the frontal be coherent with the lateral cuts).
            // frontal-entrada carries no rear beam and no bed; planta has no elevation at all; and the BOM counts the
            // same pieces with the same lengths (the bed's commercial length is the structural span and the beams'
            // lengths are transverse) — so those three stay UNCHANGED, which is what bounds this correction.
            // Previous: lateral/lateral-corte0 17815678…, frontal-posterior 55AF6395…
            // PB-004, round 2 (I-32) — el Owner RECHAZO la validacion manual round 1 y SUSTITUYO la regla: la subida de
            // 7/16" por pie es un OBJETIVO NOMINAL, no la subida final literal. Ahora el larguero POSTERIOR es el ANCLA
            // y conserva el troquel que le dio el resolver; el de ENTRADA/SALIDA se DERIVA de el por la pendiente
            // nominal y se ajusta a SU propio troquel; y la cama se traza entre los dos contactos fisicos reales, con
            // la pendiente que salga de ese ajuste. Antes se hacia al reves —anclar el bajo y arrastrar el posterior
            // FUERA de la reticula— y por eso se rechazo: un larguero siempre va atornillado a un troquel.
            //
            // Se mueven CUATRO pines, y cada uno por un motivo distinto:
            //  * lateral / lateral-corte0: el larguero de entrada/salida sube a su troquel derivado, la cama se recalcula
            //    entre los dos contactos reales, los intermedios la siguen y el posterior VUELVE a su troquel;
            //  * frontal-entrada: ese mismo larguero bajo se dibuja aqui a la elevacion derivada — una pieza fisica, una
            //    elevacion en todas las vistas;
            //  * frontal-posterior: vuelve EXACTAMENTE al valor que tenia antes del round 1 (55AF6395...), que es la
            //    comprobacion mas limpia de que el posterior regreso a su elevacion de resolver sin desplazamiento.
            // planta (sin elevacion) y bom (mismas piezas, mismas longitudes) quedan INTACTOS, y eso acota el cambio.
            // Anteriores: lateral/lateral-corte0 894A4822..., frontal-entrada C652265C..., frontal-posterior 602522B7...
            ["lateral"] = "16D20B37CAFD7A0B10CAD951987886D8583219272095F33005529129814D6841",
            ["lateral-corte0"] = "16D20B37CAFD7A0B10CAD951987886D8583219272095F33005529129814D6841",
            ["frontal-entrada"] = "DAC83E0CBBD4D908F50BCD2587F36C54CC2608B42C79C5F36634306A72500EE0",
            // OWNER CLARIFICATION 2026-07-25: the LARGUERO_ESCALON_TOPE_DE_3 block mates by its ORIGIN, so the stop's
            // insertion must land on the POST's TROQUEL_TOPE in world coordinates — resolved from the POST instance of
            // the plan, not from the rear beam's insertion (which is what kept it on the larguero troquel). Exactly the
            // two views the Owner rejected move: frontal-posterior (X now exactly on the post's stop column; the Y keeps
            // the approved rise-and-snap +4" on that same column) and planta (both coordinates coincide, no elevation).
            // LATERAL is byte-identical and the BOM is unchanged — the correction touches only those two views.
            // Previous: frontal-posterior 5553A6C1…, planta 666BBD2B…
            ["frontal-posterior"] = "55AF63952A2C5DB36BEA5FA6818E55EAE09658314A9D4A95FFE070080CDF5211",
            ["planta"] = "4797ED85A9F9344C900BD5C6A882A6BE33DA8AA2DCD1AF837C28604A18DA4C64",
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
