using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
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

        private static string PlanSignature(HeaderRunPlan plan)
            => string.Join("\n", plan.Flatten().Instances.Select(Row).OrderBy(s => s, StringComparer.Ordinal));

        private static string BomSignature(BillOfMaterials bom)
            => string.Join("\n", bom.Components
                .Select(c => FormattableString.Invariant($"{c.Category}|{c.ProfileId}|{c.Length:0.####}|{c.Quantity}|{c.Pieces.Count}"))
                .OrderBy(s => s, StringComparer.Ordinal));

        private static string Sha(string content)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        internal static RackCatalog CatalogForDiff => Catalog;
        internal static PushBackSystem ScenarioForDiff(RackCatalog catalog) => Scenario(catalog);

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
            // PB-004, round 3 (I-32) — dos precisiones del coordinador sobre la MISMA regla, que mueven TRES pines:
            //  * la subida NOMINAL se mide sobre la LONGITUD COMERCIAL de la cama (ResolveBedLength), la pieza que se
            //    compra y se dibuja, y no sobre la distancia entre contactos, que es algo mas corta;
            //  * el CONTACTO del larguero posterior es la arista que elige la GEOMETRIA (RearBeamTangencyPointWorld,
            //    la de mayor X en mundo) y no un lado fijo del catalogo: con el bloque espejado la arista buena es
            //    INICIO_IZQUIERDO, no INICIO_DERECHO.
            // Ambas mueven la elevacion derivada del larguero de entrada/salida, y con ella la cama, los intermedios y
            // ese mismo larguero en el corte frontal bajo. El POSTERIOR no se mueve —sigue en su troquel— y por eso
            // frontal-posterior, planta y bom quedan INTACTOS.
            // Anteriores: lateral/lateral-corte0 16D20B37..., frontal-entrada DAC83E0C...
            //
            // ACLARACION FINAL DEL OWNER (2026-07-26) — geometria ASIMETRICA de la cama. Se mueven TRES pines.
            //
            // Que cambio, y por que lo exige la regla:
            //  1. La ROTACION del bloque de la cama. Antes se derivaba como atan2(HighMate - ExitMate), es decir
            //     tratando los dos contactos como si estuvieran en la MISMA recta. No lo estan: ExitMate vive en la
            //     linea de TROQUEL_IN y HighMate en la del ORIGEN, que son PARALELAS y estan separadas por 1.25".
            //     Con la rotacion antigua el contacto posterior quedaba exactamente 1.25" fuera de su linea. Ahora
            //     la rotacion se resuelve con Ex*sin(t) - Ey*cos(t) = m.Y, y el posterior cae sobre la linea del
            //     origen. Eso mueve el riel, su tope, sus rodillos y los intermedios -> lateral y lateral-corte0.
            //  2. La ELEVACION del larguero de entrada/salida. El criterio de seleccion del troquel dejo de ser
            //     "ajustar la subida nominal" y pasa a ser "minimizar |tan(t) - 7/192| sobre la reticula de 2"".
            //     En este escenario el frente 0 sube UN troquel: 10.6053 -> 12.6053 en el nivel 1 y 82.6053 ->
            //     84.6053 en el nivel 2 (delta +2.0000", pendiente resultante 0.034398). El frente 1 no se mueve
            //     (ya estaba en el optimo, pendiente 0.040668). Ese larguero se dibuja tambien en el corte bajo
            //     -> frontal-entrada.
            //
            // Que NO se movio, y por que:
            //  * frontal-posterior: el larguero posterior es el ANCLA y conserva su troquel resuelto.
            //  * planta: no lleva elevaciones.
            //  * bom: la LONGITUD de la cama sigue siendo el fondo estructural completo, y los conteos no cambian.
            //
            // Anteriores: lateral/lateral-corte0 A7040D72..., frontal-entrada C124825D...
            //
            // DECISIÓN FINAL DEL DUEÑO (2026-08) — se INVIERTE la autoridad vertical: manda el extremo BAJO.
            //
            // La regla anterior fijaba el larguero POSTERIOR en la elevación del resolver y ELEGÍA el de entrada
            // sobre la retícula. Consecuencia física que el dueño rechazó en AutoCAD: el larguero por el que se
            // carga NO quedaba a la altura pedida —subía o bajaba según el fondo de la cama—, de modo que «Alto 1er
            // nivel» dejaba de significar nada. Ahora el BAJO es el ancla y conserva EXACTAMENTE su troquel; el
            // POSTERIOR se deriva y se elige sobre la retícula.
            //
            // EVIDENCIA MEDIDA sobre este mismo escenario (inserciones, en pulgadas):
            //
            //             ANTES (alto fijo)          AHORA (bajo fijo)         resolver
            //   F0 N1     low 12.6053  high 16.6053  low  6.6053  high 10.6053  exit  6.6053  entrance 16.6053
            //   F0 N2     low 84.6053  high 88.6053  low 78.6053  high 82.6053  exit 78.6053  entrance 88.6053
            //   F1 N1     low 12.6053  high 12.6053  low  6.6053  high  6.6053  exit  6.6053  entrance 12.6053
            //   F1 N2     low 84.6053  high 84.6053  low 78.6053  high 78.6053  exit 78.6053  entrance 84.6053
            //
            // Tres lecturas de esa tabla, y son las que justifican los pines:
            //  * el larguero BAJO vuelve EXACTAMENTE a la elevación de salida del resolver —la altura pedida—, en los
            //    dos frentes y en los dos niveles. Antes estaba 6" por encima;
            //  * la PENDIENTE de cada cama NO cambia: 0.034398 en el frente 0 y 0.040668 en el frente 1, idénticas
            //    antes y después. La celda entera baja 6.0000"; la cama no se reinclina. Es la comprobación de que
            //    esto invierte el ancla y no toca el criterio de selección;
            //  * el frente 1, más corto, tiene su alto en el MISMO troquel que su bajo: la subida la aporta la
            //    geometría del larguero posterior, no un salto de troquel. Nada que ver con «no sube».
            //
            // Se mueven CUATRO pines y cada uno por su pieza:
            //  * lateral / lateral-corte0: bajan los dos largueros de extremo, la cama, sus rodillos, los apoyos
            //    intermedios y el tope posterior, que cuelga del larguero alto;
            //  * frontal-entrada: el mismo larguero bajo, dibujado en su corte. Vuelve a C652265C…, el valor que
            //    tenía cuando ese larguero estaba en la elevación del resolver — la comprobación más limpia de que
            //    el ancla regresó a su sitio;
            //  * frontal-posterior: el larguero alto y su tope. ESTE pin es además un ARREGLO: hasta ahora el corte
            //    posterior leía la elevación del resolver directamente, así que era una SEGUNDA autoridad vertical
            //    para la misma pieza física. Ahora consume el mismo contexto de elevaciones que el lateral.
            // planta (sin elevaciones) y bom (mismas piezas, mismas longitudes) quedan INTACTOS, y eso acota el
            // cambio a lo que el dueño decidió.
            //
            // Anteriores: lateral/lateral-corte0 1808DB21…, frontal-entrada 2B993BAA…, frontal-posterior 55AF6395…
            // I-42 (ronda post-82e918b) — el TOPE cuelga de su larguero alto, y desde la inversion vertical ese
            // larguero esta en la elevacion DERIVADA. El tope seguia midiendose desde la que el resolver compartido
            // le dio al nivel, asi que quedaba flotando sobre un larguero que ya no esta ahi — y discrepando del
            // corte frontal, que si consumia la derivada.
            //
            // Medido en este escenario: los largueros altos estan en 10.6053/82.6053 (frente 0) y 6.6053/78.6053
            // (frente 1), mientras el resolver decia 16.6053/88.6053 y 12.6053/84.6053. Los topes bajan esos 6" y
            // se apoyan donde toca. La REGLA no cambia: sigue siendo el rise-and-snap canonico mas DOS troqueles
            // (PB-VAL-03), y sigue viviendo en un solo sitio (PushBackRearTopeBuilder.ElevationY).
            //
            // Solo se mueven los dos pines LATERALES: el frontal posterior ya media desde la derivada, la planta no
            // lleva elevacion y el BOM cuenta las mismas piezas con las mismas longitudes.
            // Anteriores: lateral/lateral-corte0 1272488B...
            ["lateral"] = "523862527623A874B333E201A8CADF0774610E39CD496E352BF8A9B594760B69",
            ["lateral-corte0"] = "523862527623A874B333E201A8CADF0774610E39CD496E352BF8A9B594760B69",
            ["frontal-entrada"] = "C652265C592E4834A976C6E03ABC1282FA353E861DBF8A5AEC4F7C3E3CCE3974",
            // OWNER CLARIFICATION 2026-07-25: the LARGUERO_ESCALON_TOPE_DE_3 block mates by its ORIGIN, so the stop's
            // insertion must land on the POST's TROQUEL_TOPE in world coordinates — resolved from the POST instance of
            // the plan, not from the rear beam's insertion (which is what kept it on the larguero troquel). Exactly the
            // two views the Owner rejected move: frontal-posterior (X now exactly on the post's stop column; the Y keeps
            // the approved rise-and-snap +4" on that same column) and planta (both coordinates coincide, no elevation).
            // LATERAL is byte-identical and the BOM is unchanged — the correction touches only those two views.
            // Previous: frontal-posterior 5553A6C1…, planta 666BBD2B…
            ["frontal-posterior"] = "234EDFA63829C7EA5895AAC48FD38AE9C29CB57C8D340A90407E2F6D439501BF",
            // I-42 (ronda post-82e918b) — en PLANTA un frente se identifica por su posicion TRANSVERSAL, no por la
            // X de profundidad. Se buscaba «el frente cuyo EndX esta mas cerca» y, como los dos frentes de este
            // escenario ACABAN en la misma posicion, devolvia siempre el frente 0: el larguero posterior del frente
            // 1 salia con el PERALTE del frente 0 (5") en vez del suyo (3.5"), y su tope se decidia leyendo la
            // configuracion del otro frente. En un rack compuesto el mismo defecto dejaba el tope solo en el primer
            // frente, que es lo que el dueño reporto.
            //
            // Medido en este escenario: el larguero posterior de Y=54.24 pasa de PERALTE 5 a PERALTE 3.5, que es el
            // suyo. Los dos topes siguen apareciendo y en la misma columna. Solo se mueve el pin de PLANTA: la X de
            // profundidad no cambia, asi que el lateral, los dos frontales y el BOM quedan INTACTOS.
            // Anterior: planta 4797ED85...
            ["planta"] = "02EE98BAC846356957D892BB23E0A8B4AE31C849A6FA1370DDF22A849CB90D69",
            // BOM pin updated by the length-coherence fix (rear tope LONGITUD = beamLength + LengthAllowance; end beams
            // per cell). The FIVE view pins are UNCHANGED (with no per-level override the cell length equals the front
            // length). Previous BOM hash: 139C18EFDD0BCF1DBC9CABB867E3C40499B2BD264E1BED4F4CBC7DCEE74C57AC.
            // I-42 (correccion aislada 5B/5C) — el dueño RETIRO la regla de la ronda 5 («ultimo poste / primer poste
            // / poste interior») y la SUSTITUYO por esta: el larguero de salida debe llevar EXACTAMENTE la
            // orientacion que tendria un larguero INTERMEDIO colocado en esa misma posicion fisica. No es una regla
            // nueva — el programa ya orienta bien los intermedios y ahora el alto CONSUME esa misma autoridad.
            //
            // Consecuencia sobre este escenario: su frontera alta esta en X=300, que es donde TERMINA una CABECERA, y
            // un intermedio apoyado ahi va espejado. Los siete flags que la ronda 5 habia volteado vuelven, uno por
            // uno, al valor que el dueño valido en la ronda 4B — y los dos pines regresan EXACTAMENTE a los hashes
            // que tenian entonces, que es la comprobacion mas limpia de que la regla retirada era la anomalia:
            //   LATERAL  larguero alto X=300 Y=10.6053     mirrored False -> True
            //   LATERAL  larguero alto X=300 Y=82.6053     mirrored False -> True
            //   LATERAL  tope          X=299.125 Y=94.1563 mirrored True  -> False
            //   PLANTA   larguero alto X=300 Y=0.75        mirrored False -> True
            //   PLANTA   larguero alto X=300 Y=54.244      mirrored False -> True
            //   PLANTA   tope          X=299.125 Y=1.5     mirrored True  -> False
            //   PLANTA   tope          X=299.125 Y=54.994  mirrored True  -> False
            //
            // Siete primitivas, y en las siete cambia SOLO el espejo: ni una X, ni una Y, ni un anclaje, ni una
            // rotacion, ni una pieza, ni una cantidad. Las X de los topes son las de la ronda 4B y no se mueven —
            // posicion y orientacion del tope son dos autoridades separadas, por decision del dueño. Los dos
            // frontales quedan INTACTOS: su espejo es el de la retícula transversal y no es esta pregunta.
            // Anterior (ronda 5, retirada): lateral y lateral-corte0 DC546899..., planta E2A173A8...
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
