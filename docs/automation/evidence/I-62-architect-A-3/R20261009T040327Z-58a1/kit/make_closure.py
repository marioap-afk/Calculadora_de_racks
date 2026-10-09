"""I-62 A-3 (docs/initiatives/I-62-A-3.md, blob ea6721f7, publicada en 91e29886; recibo de publicación 49288525, que añade el resultado de
las guardas; sus demás cambios son registros): EffectiveInputClosure de la UNA revisión formal independiente del Architect por una celda
`claude-cli` medida y elegible (disposición §58, punto 3; decisiones §57, punto 3; autorización del Owner CLAUDE-CLI-I62 = A).

Kit adaptado del kit acreditado de la re-revisión de A-2 (R20261008T184326Z-f1e2, auditor v5.1, ACCREDITED; decisiones §57, punto 1). El cierre
canónico es la tabla de insumos del paquete §2 en el commit del recibo, más el propio paquete. Los rangos de líneas se calculan aquí,
mecánicamente, desde los encabezados y las filas del texto en el commit; cada rango se comprueba (título exacto y fila única) y el script falla
si algo no coincide. Los blobs se calculan con git en el commit; los marcadores del paquete (<A3_BLOB>, <ANNEX_BLOB>, <DEC_BLOB>, <EV_BLOB>,
<GUARDS_BLOB>, <SELFTEST_BLOB>) se resuelven aquí como recibo de publicación, y cada blob fijado por el paquete se compara con el del commit.

Diferencias frente al kit de A-2, fijadas antes del lanzamiento (README, desviaciones):
  * objeto nuevo, sin objeto formal anterior ni hallazgos formales anteriores: no hay delta ni PriorFindingDispositions;
  * archivos del run (clase RUN, con hash en el cierre): order.txt (cuerpo de la disposición §58 del Coordinator), prompt.md (stdin literal) y
    los textos fijos de las órdenes §56 y §57 (order-s56.txt y order-s57.txt), que el paquete §5 exige custodiados antes del lanzamiento y cuya
    numeración citan A-3 y el paquete («orden, punto n»); su SHA-256 se contrasta con el que declara el archivo de decisiones en el commit;
  * owner-decision-packets.md y la evidencia crecieron solo por añadido entre el blob que nombra el paquete y el commit del recibo: se comprueba
    que el texto del blob nombrado es prefijo exacto del texto del commit;
  * el resto del contrato (herramientas, compuerta D-01, flags, plantilla de argumentos) es el del kit de A-2.

Uso: python make_closure.py            → escribe closure.json junto a este script (el directorio del kit)
     python make_closure.py --ranges   → solo imprime los rangos (para redactar el prompt); no escribe nada
No escribe en el clon, en el run ni en el repositorio.
"""
import hashlib
import importlib.util
import json
import os
import re
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("transport_gate", os.path.join(KIT, "transport_gate.py"))
TG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(TG)
REV = "492885254b58203f0bc0099345db2998b25d2a2b"          # recibo de publicación: añade a3-guards-result.json; lo demás son registros
PUB_COMMIT = "91e29886a4870dd673372c66453d76ea503a22b9"   # commit que publica A-3 (G2 y G5b de a3-guards-result.json lo nombran como Head)
A3_BASE = "76b48e785d87af5591f4363246751b7eb1182506"      # padre del commit de publicación (A3_BASE de las guardas)
PREP_BASE = "b088421649204742ff682e1eb1ebdafc762823d0"    # base de preparación de A-3 (sus literales de §2 se leen en este commit)
CLONE = r"D:\r62-arch-a3"
RUN = r"D:\r62-arch-a3-run"
IDS = {"RunId": "R20261009T040327Z-58a1", "InvocationId": "I20261009T040327Z-58a1", "LogicalReviewRequestId": "L20261009T040327Z-58a1",
       "AttemptSeq": 1}
SESSION_ID = "ae3590eb-b508-4949-84da-8aa7340df993"       # --session-id fijado en el kit para el único lanzamiento (uuid4)
OBJECT = "docs/initiatives/I-62-A-3.md"
ANNEX = "docs/initiatives/I-62-A-3-annex-maf-codex.md"
PKG = "docs/initiatives/I-62-architect-package-A-3.md"
OD2_PACKET = "docs/automation/evidence/I-62-A3/od2-material-baseline-owner-packet.md"
OD2_EVOLUTION = "docs/automation/evidence/I-62-A3/od2-evolution.md"
MAF = "docs/automation/evidence/I-62-A3/maf-codex.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A1 = "docs/initiatives/I-62-A-1.md"
A2 = "docs/initiatives/I-62-A-2.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
AP = "docs/AUTOMATION_PLAN.md"
README_AE = "docs/automation/agent-execution/README.md"
ROUTING = "docs/automation/agent-execution/routing.md"
CODEX = "docs/automation/agent-execution/adapters/codex-cli.md"
CLAUDE = "docs/automation/agent-execution/adapters/claude-cli.md"
PREFLIGHT = "docs/automation/agent-execution/schemas/preflight.v1.schema.json"
BINDING = "docs/automation/agent-execution/schemas/binding.v1.schema.json"
FACTS = "docs/automation/agent-execution/schemas/adapters/codex-cli.facts.v1.schema.json"
CATALOG = "docs/automation/agent-execution/model-catalog.md"
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
DECISIONS = "docs/automation/decisions/I-62.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"
PACKETS = "docs/automation/evidence/I-62-prep/owner-decision-packets.md"
GUARDS = "docs/automation/evidence/I-62-A3/a3-guards.py"
SELFTEST = "docs/automation/evidence/I-62-A3/a3-selftest.json"
GUARDS_RESULT = "docs/automation/evidence/I-62-A3/a3-guards-result.json"
# blobs completos que el kit fija (recibo de publicación en REV); el script falla si el commit no los tiene
EXPECTED_BLOBS = {
    OBJECT: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d",
    ANNEX: "f410f7fced25440be3866473974f417d2f6a9b50",
    PKG: "d289329d31f163739ea8e55f5a3ad3227a5cc62e",
    OD2_PACKET: "b2c8ec8a295188d05c0e07d153442f9268aaa20e",
    OD2_EVOLUTION: "3b8dc62039210a48c26674c3c964466192b8b31e",
    MAF: "a297efb2078931965ea15e14e05162cfb14dcbd8",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f",
    A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
    DECISIONS: "27591f161aabb2de30610b0d17341597fd2a4079",
    EVIDENCE: "6855110a61b70a5ceb7e3928b5de4671cb6af7d6",
    PACKETS: "f39c0eff",                                    # prefijo: el paquete nombra cdb86114 (ver APPEND_ONLY)
    GUARDS: "215b2e43e40c67bc8d47cbfc5ccc3814b80451ea",
    SELFTEST: "38a79ca082617781e1332daa3a4d127cb433dacc",
    GUARDS_RESULT: "70b23c0dbcae830cd86394a204d94ee39d764a0b",
}
# lo que fija el propio paquete §2 (prefijo o marcador) → se comprueba contra el blob del commit
PACKAGE_DECLARED = {
    OBJECT: "<A3_BLOB>", ANNEX: "<ANNEX_BLOB>", OD2_PACKET: "b2c8ec8a295188d05c0e07d153442f9268aaa20e",
    OD2_EVOLUTION: "3b8dc62039210a48c26674c3c964466192b8b31e", MAF: "a297efb2078931965ea15e14e05162cfb14dcbd8", V14: "34ad80ea", A1: "c01899a7",
    A2: "f1e1d6f0", FREEZE: "0f6b8608", LIFECYCLE: "f19896a8", AP: "f525cb1e", README_AE: "592dcfd4", ROUTING: "bba08fc4", CODEX: "155f3469",
    CLAUDE: "ae570380", PREFLIGHT: "6a054079", BINDING: "13501476", FACTS: "3d1b7478", CATALOG: "166d978d", ADR46: "da275e1a",
    DECISIONS: "<DEC_BLOB> (al preparar, 27591f16)", EVIDENCE: "<EV_BLOB> (al preparar, 6a2b1569)", PACKETS: "cdb86114",
    GUARDS: "<GUARDS_BLOB>", SELFTEST: "<SELFTEST_BLOB>", GUARDS_RESULT: "(en el commit siguiente al de publicación)", PKG: "(el paquete no lleva su propio blob)",
}
# archivos que crecieron solo por añadido entre el blob nombrado (en el commit indicado) y REV: el texto nombrado es prefijo exacto del de REV
APPEND_ONLY = {PACKETS: (PUB_COMMIT, "cdb86114"), EVIDENCE: (PREP_BASE, "6a2b1569")}
RECEIPT = {"<A3_BLOB>": OBJECT, "<ANNEX_BLOB>": ANNEX, "<DEC_BLOB>": DECISIONS, "<EV_BLOB>": EVIDENCE, "<GUARDS_BLOB>": GUARDS, "<SELFTEST_BLOB>": SELFTEST}
RUN_FILES = ["order.txt", "prompt.md", "order-s56.txt", "order-s57.txt"]
PREMISE_RUN_FILES = ["order.txt", "order-s56.txt", "order-s57.txt"]   # prompt.md nunca es premisa
ORDER_SHA = "3ed7e135eb914a7307bdc83bd9da6cf4dd4cf68aadc3e04d71a619e82b709a1c"   # disposición §58 del Coordinator: 2 767 bytes
ORDER_TEXTS = {"order-s56.txt": ("56", "e8b003284843374ba1e8497434f375f3206da2cd9a9829f2f451ff73af06b081", 8562),
               "order-s57.txt": ("57", "00a13ae527863117472bc7356458c849a8ef6f6e6255b417ee736dc3428023a3", 3654)}
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
ADD_DIR = ["--add-dir", RUN]   # no medido en C1; medido en C3 de la caracterización (plantilla con marcador); lo ata kit/transport-gate.json (D-01)
TIMEOUT_S = 7200


def git(*a):
    return subprocess.check_output(["git", "-C", CLONE] + list(a))


# ------------------------------------------------------------------ rangos mecánicos
def headings(lines):
    out, fence = [], False
    for i, l in enumerate(lines):
        if l.startswith("```"):
            fence = not fence
            continue
        m = re.match(r"^(#{1,6}) (.*)$", l)
        if m and not fence:
            out.append((i + 1, len(m.group(1)), m.group(2)))
    return out


def section(lines, prefix, label=None):
    """Rango [inicio, fin] de la sección cuyo título empieza por `prefix` (único); termina antes del siguiente encabezado de nivel ≤."""
    hs = headings(lines)
    hit = [h for h in hs if h[2].startswith(prefix)]
    assert len(hit) == 1, "sección %r: %d coincidencias" % (prefix, len(hit))
    n, lv, title = hit[0]
    nxt = [h[0] for h in hs if h[0] > n and h[1] <= lv]
    end = (nxt[0] - 1) if nxt else len(lines)
    while end > n and lines[end - 1].strip() == "":
        end -= 1
    return {"Section": label or ("%s %s" % ("#" * lv, title))[:120], "Start": n, "End": end}


def span(first, last, label):
    return {"Section": label, "Start": first["Start"], "End": last["End"]}


def row(lines, rng, pred, label):
    hits = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if pred(lines[i])]
    assert len(hits) == 1, "fila %r: %d coincidencias en %s" % (label, len(hits), rng)
    return {"Section": label, "Start": hits[0], "End": hits[0]}


def fixed(lines, start, end, prefix, label):
    """Rango citado por número de línea en el paquete (p. ej., «A2 L222-227»): se comprueba que la primera línea contiene `prefix`."""
    assert prefix and prefix in lines[start - 1], "línea %d no contiene %r" % (start, prefix)
    return {"Section": label, "Start": start, "End": end}


def whole(lines):
    return [{"Section": "entero", "Start": 1, "End": len(lines)}]


def header_block(L, label="cabecera (bloque de identidad)"):
    hs = headings(L)
    end = hs[1][0] - 1
    while L[end - 1].strip() == "":
        end -= 1
    return {"Section": label, "Start": 1, "End": end}


def all_sections(L, level_max=3):
    return [section(L, h[2]) for h in headings(L)[1:] if h[1] <= level_max]


def rule_spans(L):
    s32 = section(L, "3.2 ")
    starts = [(i + 1, int(m.group(1))) for i, m in ((i, re.match(r"^> (\d+)\. \*\*", L[i])) for i in range(s32["Start"] - 1, s32["End"])) if m]
    assert [n for _, n in starts] == list(range(1, 12)), starts
    tail = row(L, s32, lambda x: x.startswith("> Las invariantes de esta regla"), "cierre del párrafo")
    out = []
    for k, (ln, n) in enumerate(starts):
        end = (starts[k + 1][0] - 1) if k + 1 < len(starts) else tail["Start"] - 1
        while L[end - 1].strip() in ("", ">"):
            end -= 1
        out.append({"Section": "§3.2, regla %d" % n, "Start": ln, "End": end})
    return out


def ranges_object(L):
    out = whole(L) + [header_block(L)] + all_sections(L)
    out += rule_spans(L)
    return out


def ranges_package(L):
    return whole(L) + [header_block(L), row(L, {"Start": 1, "End": 60}, lambda x: x.startswith("> **Identidad exacta.**"), "nota «Identidad exacta»")] + \
        [section(L, p) for p in ("1. ", "2. ", "3. ", "4. ", "5. ", "6. ")]


def ranges_entire_with_sections(L):
    return whole(L) + all_sections(L, 2)


def ranges_v14(L):
    s6, s13, s18, c = section(L, "6. "), section(L, "13. "), section(L, "18. "), section(L, "Anexo C ")
    return [section(L, "1. "), section(L, "4.2 "), section(L, "5. "), s6,
            row(L, s6, lambda x: x.startswith("| **Observación de capacidad**"), "§6, fila de la observación de capacidad"),
            row(L, s6, lambda x: x.strip() == "no una foto anterior.", "§6, ancla («no una foto anterior.»)"),
            section(L, "7. "), section(L, "10. "), s13,
            row(L, s13, lambda x: x.startswith("| P-11 |"), "§13, fila P-11"),
            span(row(L, s13, lambda x: x.startswith("| P-22 |"), "P-22"), row(L, s13, lambda x: x.startswith("| P-24 |"), "P-24"), "§13, filas P-22..P-24"),
            s18, row(L, s18, lambda x: x.startswith("| OD-2 |"), "§18, fila OD-2"),
            row(L, s18, lambda x: x.startswith("Cada solicitud va aparte"), "§18, L977 («El silencio no es decisión»)"),
            section(L, "20.3 "), section(L, "20.5.1 "), section(L, "B.2 "), section(L, "B.3 "), section(L, "B.4 "), section(L, "B.5 "),
            span(row(L, c, lambda x: x.startswith("| C-06 |"), "C-06"), row(L, c, lambda x: x.startswith("| C-11 |"), "C-11"), "Anexo C, filas C-06..C-11"),
            row(L, c, lambda x: x.startswith("| C-21 |"), "Anexo C, fila C-21"), row(L, c, lambda x: x.startswith("| C-40 |"), "Anexo C, fila C-40"),
            row(L, c, lambda x: x.startswith("| C-41 |"), "Anexo C, fila C-41"), section(L, "D.3 "), section(L, "D.4 ")]


def ranges_a1(L):
    return [header_block(L, "cabecera (formato de una A-n)"), section(L, "4. Materialidad"), section(L, "5. Efecto en obligaciones"),
            section(L, "11. Veredictos")]


def ranges_a2(L):
    return [header_block(L), section(L, "3. A2-P2"), section(L, "3.3 "), fixed(L, 222, 227, "3. puede autorizarse **un solo** bloque", "A2 L222-227 (A2-P2, reglas 3 y 4)")]


def ranges_lifecycle(L):
    s3, s6 = section(L, "3. "), section(L, "6. ")
    return [s3, fixed(L, 94, 94, "Un cambio de alcance", "L94 (OWNER-RESERVED)"), section(L, "5. "), s6,
            fixed(L, 221, 221, "Una M material exige Architect + Coordinator", "L221 (autoridades de una A-n)")]


def ranges_ap(L):
    s164 = section(L, "16.4 ")
    return [s164, fixed(L, 522, 523, "3. **Entrada:**", "16.4, entrada (L522-523)"), fixed(L, 541, 543, "**Receta de Codex:**", "16.4, receta de Codex (L541-543)"),
            section(L, "16.9 "), section(L, "16.11 "), span(section(L, "16.14 "), section(L, "16.16 "), "16.14-16.16"),
            section(L, "16.14 "), section(L, "16.15 "), section(L, "16.16 "),
            span(section(L, "16.18 "), section(L, "16.20 "), "16.18-16.20"), section(L, "16.18 "), section(L, "16.19 "), section(L, "16.20 "),
            section(L, "16.22 ")]


def ranges_readme(L):
    s = [section(L, "%d. " % n) for n in (12, 13, 14)]
    return [span(s[0], s[-1], "§12-§14")] + s


def ranges_routing(L):
    return [section(L, "5. "), section(L, "8. "), section(L, "9. ")]


def ranges_codex(L):
    return whole(L) + [fixed(L, 30, 30, "| 9 | declarar la huella |", "L30 (huella declarada)")]


def ranges_adr46(L):
    return [section(L, "Decisión"), section(L, "Consecuencias"), fixed(L, 95, 95, "  - los efectos laterales de Codex", "L95 («Vigilar»)")]


def ranges_decisions(L):
    s = [section(L, "%d. " % n) for n in range(46, 58)]
    s54, s56, s57 = s[54 - 46], s[56 - 46], s[57 - 46]
    return [span(s[0], s[-1], "§46-§57")] + s + [
        fixed(L, 1043, 1043, "| Codex / OD-2d |", "§54, L1043"), row(L, s56, lambda x: x.startswith("| **11-17. A-3** |"), "§56, fila 11-17 (L1100)"),
        row(L, s57, lambda x: x.startswith("| **4. A-3** |"), "§57, punto 4 (L1115)")]


def ranges_evidence(L):
    nums = (64, 67, 68, 70, 71, 75, 82, 83, 84, 85, 86, 87)
    s = {n: section(L, "%d. " % n) for n in nums}
    return [s[64], span(s[67], s[68], "§67-§68"), span(s[70], s[71], "§70-§71"), s[75], span(s[82], s[87], "§82-§87")] + [s[n] for n in nums]


def ranges_packets(L):
    od2, c, d = section(L, "OD-2 — "), section(L, "OD-2c "), section(L, "OD-2d (vigente)")
    return [od2, fixed(L, 93, 93, "| Seguridad | solo el hash del archivo y nombres saneados; nunca valores", "L93 (términos de OD-2; citada por A-3)"), c, fixed(L, 182, 182, "| Runtime observado (RUNTIME_OBSERVED", "L182"), d, fixed(L, 199, 200, "| Claves cambiadas (frente a", "L199-200")]


canonical = [
    (OBJECT, "objeto de la revisión: A-3 entera (paquete §2: alcance, literales con líneas, clase, delta (reglas 1-11), cota, predicado, "
             "resultados, OD-2, composición con A-2, guardas, huella material, antes y después, obligaciones y pruebas, materialidad, no impacto, "
             "consecuencias del Owner, preguntas, veredictos y registro de la revisión previa)", "ENTIRE", ranges_object),
    (ANNEX, "anexo no normativo de la regla 11 para codex-cli (paquete §2)", "ENTIRE", ranges_entire_with_sections),
    (PKG, "paquete de la revisión (cabecera e identidad exacta, veredicto pedido §1, insumos §2, resumen §3, preguntas §4, condiciones §5, §6)",
     "ENTIRE", ranges_package),
    (OD2_PACKET, "paquete de la decisión del Owner OD-2-MAT, PENDIENTE (paquete §2)", "ENTIRE", ranges_entire_with_sections),
    (OD2_EVOLUTION, "investigación de la evolución de OD-2 (paquete §2)", "ENTIRE", ranges_entire_with_sections),
    (MAF, "investigación de la huella material (paquete §2)", "ENTIRE", ranges_entire_with_sections),
    (V14, "cláusulas cuya lectura rige A-3 (paquete §2): §1, §4.2, §5, §6 (ancla y fila), §7, §10, §13 (P-11, P-22..P-24), §18 (OD-2 y L977), "
          "§20.3, §20.5.1, B.2, B.3, B.4, B.5, Anexo C (C-06..C-11, C-21, C-40, C-41), D.3 y D.4", "SECTIONS", ranges_v14),
    (A1, "formato de una A-n acordada, con «Efecto en obligaciones y pruebas» (paquete §2)", "SECTIONS", ranges_a1),
    (A2, "A2-P2 acordada y L222-227 (paquete §2)", "SECTIONS", ranges_a2),
    (FREEZE, "identidad del Freeze", "ENTIRE", lambda L: whole(L)),
    (LIFECYCLE, "§3 (M-01..M-08, regla de duda, OWNER-RESERVED L94), §5 (REQUIRED) y §6 (formato y autoridades de una A-n, L221)", "SECTIONS",
     ranges_lifecycle),
    (AP, "16.4 (entrada L522-523 y receta L541-543), 16.9, 16.11, 16.14-16.16, 16.18-16.20 y 16.22 (paquete §2)", "SECTIONS", ranges_ap),
    (README_AE, "§12-§14 (paquete §2)", "SECTIONS", ranges_readme),
    (ROUTING, "§5, §8 y §9 (paquete §2)", "SECTIONS", ranges_routing),
    (CODEX, "descriptor de codex-cli (paquete §2; huella declarada en L30)", "ENTIRE", ranges_codex),
    (CLAUDE, "descriptor de claude-cli (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (PREFLIGHT, "esquema preflight/v1 (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (BINDING, "esquema binding/v1 (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (FACTS, "esquema de hechos de codex-cli (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (CATALOG, "catálogo de modelos (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (ADR46, "ADR-0046 #4 y «Vigilar» (L95) (paquete §2)", "SECTIONS", ranges_adr46),
    (DECISIONS, "§46-§57 (paquete §2): OD-2 y su cadena, §54 (L1043), §55 (puntos 9 y 11), §56 (puntos 1-10 y 18, y la fila 11-17) y §57 (puntos 1-4)",
     "SECTIONS", ranges_decisions),
    (EVIDENCE, "§64, §67-§68, §70-§71, §75 y §82-§87 (paquete §2)", "SECTIONS", ranges_evidence),
    (PACKETS, "paquetes OD-2c y OD-2d (L182, L199 y L200) (paquete §2), y OD-2 (L93, citada por A-3)", "SECTIONS", ranges_packets),
    (GUARDS, "guardas: modelo G5, preview y run (G1-G5) (paquete §2; evidencia de apoyo, no autoridad)", "ENTIRE", lambda L: whole(L)),
    (SELFTEST, "resultado de self-test --a3 docs/initiatives/I-62-A-3.md (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (GUARDS_RESULT, "resultado de run --base 76b48e78 --head 91e29886 (paquete §2; su campo Head nombra el commit de publicación)", "ENTIRE",
     lambda L: whole(L)),
]


def lines_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    if lines and lines[-1] == "":
        lines = lines[:-1]
    return lines


def describe(b):
    t = b.decode("utf-8")
    return {"Bytes": len(b), "Lines": len(lines_of(t)), "Sha256": hashlib.sha256(b).hexdigest(),
            "LinesOver2000Chars": [i + 1 for i, x in enumerate(t.split("\n")) if len(x) > 2000],
            "Encoding": {"Charset": "UTF-8", "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF",
                         "FinalNewline": b.endswith(b"\n")}}, t


def rec(p):
    b = git("show", REV + ":" + p)
    r, t = describe(b)
    blob = git("rev-parse", REV + ":" + p).decode().strip()
    r = dict({"Path": p, "Blob": blob}, **r)
    if p in EXPECTED_BLOBS:
        assert blob.startswith(EXPECTED_BLOBS[p]), "blob inesperado de %s: %s" % (p, blob)
    d = PACKAGE_DECLARED[p]
    r["PackageDeclaredBlob"] = d
    if re.match(r"^[0-9a-f]{8,40}$", d):
        if p in APPEND_ONLY:
            commit, prefix = APPEND_ONLY[p]
            old_blob = git("rev-parse", commit + ":" + p).decode().strip()
            assert old_blob.startswith(prefix), (p, old_blob)
            old = git("cat-file", "-p", old_blob)
            assert b.startswith(old) and len(b) > len(old), "%s: el blob nombrado no es prefijo exacto del texto del commit" % p
            r["AppendOnly"] = {"NamedBlob": old_blob, "NamedIn": commit, "NamedLines": len(lines_of(old.decode("utf-8"))), "NamedBytes": len(old),
                               "Note": "el texto del blob nombrado es prefijo exacto del texto del commit (solo añadido)"}
        else:
            assert blob.startswith(d), "%s: blob %s distinto del que fija el paquete (%s)" % (p, blob, d)
    elif p in APPEND_ONLY:   # marcador con «al preparar»
        commit, prefix = APPEND_ONLY[p]
        old_blob = git("rev-parse", commit + ":" + p).decode().strip()
        assert old_blob.startswith(prefix), (p, old_blob)
        old = git("cat-file", "-p", old_blob)
        assert b.startswith(old), "%s: el blob de preparación no es prefijo exacto" % p
        r["AppendOnly"] = {"NamedBlob": old_blob, "NamedIn": commit, "NamedLines": len(lines_of(old.decode("utf-8"))), "NamedBytes": len(old),
                           "Note": "marcador resuelto en el recibo; el blob de preparación es prefijo exacto (solo añadido)"}
    return r, t, lines_of(t)


def question_ids(L):
    s9 = section(L, "9. ")
    ids = [m.group(1) for m in (re.match(r"^\| (Q-A3-\d\d) \|", L[i]) for i in range(s9["Start"] - 1, s9["End"])) if m]
    assert sorted(ids) == ["Q-A3-%02d" % n for n in range(1, 23)] and len(ids) == 22, ids
    rows = {m.group(1): i + 1 for i, m in ((i, re.match(r"^\| (Q-A3-\d\d) \|", L[i])) for i in range(s9["Start"] - 1, s9["End"])) if m}
    withdrawn = sorted(q for q, n in rows.items() if re.match(r"^\| Q-A3-\d\d \| Retirada", L[n - 1]))
    return sorted(ids), rows, withdrawn


def focus_items(L):
    s4 = section(L, "4. ")
    items = [(int(m.group(1)), L[i]) for i, m in ((i, re.match(r"^(\d+)\. ", L[i])) for i in range(s4["Start"] - 1, s4["End"])) if m]
    assert [n for n, _ in items] == list(range(1, len(items) + 1)), items
    q = [n for n, text in items if "Q-A3-01..Q-A3-22" in text]
    assert q == [len(items)], "el último ítem del paquete §4 debe ser el de las preguntas Q-A3: %s" % q
    return [n for n, _ in items if n not in q], q[0]


def order_text_sha(dec_lines, num):
    s = section(dec_lines, "%s. " % num)
    text = "\n".join(dec_lines[s["Start"] - 1:s["End"]])
    return set(re.findall(r"`([0-9a-f]{64})`", text)), text


def main(print_only):
    assert git("rev-parse", "HEAD").decode().strip() == REV, "HEAD del clon distinto del commit"
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    can, corpus = [], set()
    qids = qrows = withdrawn = focus = qitem = dec_lines = None
    for p, why, scope, rf in canonical:
        r, t, L = rec(p)
        r["Why"], r["Scope"], r["LineRanges"] = why, scope, rf(L)
        for g in r["LineRanges"]:
            assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
        can.append(r)
        corpus |= {c for c in t if ord(c) > 127}
        if p == OBJECT:
            qids, qrows, withdrawn = question_ids(L)
        if p == PKG:
            focus, qitem = focus_items(L)
        if p == DECISIONS:
            dec_lines = L
    ranges = {r["Path"]: [(g["Section"][:60], g["Start"], g["End"]) for g in r["LineRanges"]] for r in can}
    receipt = {k: {"Path": v, "Blob": next(r["Blob"] for r in can if r["Path"] == v)} for k, v in RECEIPT.items()}
    if print_only:
        print(json.dumps({"files": {r["Path"]: (r["Bytes"], r["Lines"], r["Blob"][:8], r["LinesOver2000Chars"]) for r in can},
                          "ranges": ranges, "questions": qids, "questionRows": qrows, "withdrawn": withdrawn, "focus": focus,
                          "receipt": receipt}, ensure_ascii=False, indent=1))
        return
    run = {}
    for n in RUN_FILES:
        b = open(os.path.join(RUN, n), "rb").read()
        assert b == open(os.path.join(KIT, n), "rb").read(), "%s del run distinto de la copia del kit" % n
        d, t = describe(b)
        run[n] = d
        if n != "prompt.md":
            corpus |= {c for c in t if ord(c) > 127}
    assert run["order.txt"]["Sha256"] == ORDER_SHA, "order.txt distinto del cuerpo de la disposición §58"
    order_checks = {}
    for n, (num, want, size) in ORDER_TEXTS.items():
        shas, text = order_text_sha(dec_lines, num)
        assert run[n]["Sha256"] == want and want in shas and run[n]["Bytes"] == size, "%s distinto del texto que custodia decisiones §%s" % (n, num)
        assert ("%d %03d bytes" % (size // 1000, size % 1000)) in text, "decisiones §%s no declara %d bytes" % (num, size)
        order_checks[n] = {"DecisionsSection": "§" + num, "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": size}
    sources = {"order.txt": "cuerpo exacto pegado por el Owner de la disposición §58 del Coordinator (2 767 bytes; SHA-256 3ed7e135…); es la "
                            "autoridad de esta revisión (punto 3); no está en el clon: decisiones §58 se registró después del recibo",
               "prompt.md": "copia de kit/prompt.md; stdin literal del proceso claude-cli; nunca es premisa",
               "order-s56.txt": "texto fijo de la orden §56 del Coordinator (8 562 bytes; SHA-256 e8b00328…, declarado en decisiones §56 del commit); "
                                "su numeración es la que citan A-3 y el paquete («orden, punto n»; puntos 11-17 = A-3)",
               "order-s57.txt": "texto fijo de la orden §57 del Coordinator (3 654 bytes; SHA-256 00a13ae5…, declarado en decisiones §57 del commit); "
                                "el punto 4 amplía A-3 (MATERIAL_ADAPTER_FINGERPRINT)"}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    for n, c in order_checks.items():
        run_hashes[n]["IdentityCheck"] = c
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "ObjectIntroducedBy": PUB_COMMIT, "PublicationBase": A3_BASE, "PreparationBase": PREP_BASE,
        "ObjectPath": OBJECT, "ObjectBlob": EXPECTED_BLOBS[OBJECT], "AnnexPath": ANNEX, "AnnexBlob": EXPECTED_BLOBS[ANNEX], "PackagePath": PKG,
        "PublicationReceipt": {"Commit": REV, "PublicationCommit": PUB_COMMIT, "Placeholders": receipt,
                               "Note": "el paquete fija los blobs del objeto, del anexo, de decisiones, de la evidencia, de las guardas y del self-test con "
                                       "marcadores que resuelve el recibo de publicación; el kit los resuelve con los blobs del commit del recibo"},
        "PreviousObject": None, "PriorFormalFindings": "ninguno: no hay revisión formal anterior de A-3; las rondas adversariales de A-3 §12 son "
                                                        "internas y no son hallazgos formales",
        "Harness": GUARDS, "HarnessExecution": "ninguna por el revisor: no tiene herramientas de ejecución; lee el código y los resultados custodiados "
                                               "(a3-selftest.json y a3-guards-result.json)",
        "FocusItems": len(focus), "FocusSource": "paquete §4, preguntas %s (la %d son las Q-A3 de A-3 §9)" % (", ".join(map(str, focus)), qitem),
        "QuestionIds": qids, "QuestionRows": qrows, "WithdrawnQuestions": withdrawn,
        "QuestionSource": "A-3 §9 (paquete §1: disposición de Q-A3-01..Q-A3-22; A-3 marca %s como retiradas y el paquete pide disponer las 22)" % " y ".join(withdrawn),
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ADD_DIR, "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ADD_DIR + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "AddDirStatus": "no medido en C1; medido en C3 de la caracterización R20261008T183600Z-char sobre la plantilla exacta con marcadores "
                                      "(decisiones §56, punto 7; §57, punto 3): kit/transport-gate.json se entrega en MEASURED con el SHA-256 del "
                                      "registro fijado; launch.py vuelve a evaluar la compuerta y el auditor la re-evalúa sobre las copias de launch/",
                      "Gate": "kit/transport-gate.json (transport_gate.py)",
                      "Stdin": "prompt.md del run (bytes literales)",
                      "JsonSchema": "kit/result.schema.json en forma compacta: json.dumps(separators=(',', ':'), ensure_ascii=True)",
                      "ExpectedInit": {"tools": ["Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "dontAsk", "mcp_servers": [],
                                       "claude_code_version": "2.1.293"},
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a3\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S,
                      "Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; aceptada como evidencia "
                                          "de elegibilidad vigente en decisiones §57, punto 3): C1 completada, C2 cancelada por terminación, C3 PASS "
                                          "sobre la plantilla con --add-dir"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos cuatro archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json) "
                                "y el auditor v5.1 lo vuelve a verificar después (HEAD, rama única, sin remotos, árbol limpio con ignorados, blobs "
                                "designados); el revisor contrasta lo que puede leer: order.txt, la cabecera del paquete, a3-guards-result.json (Head, "
                                "G3 y G5b) y a3-selftest.json (A3Text y A3TextBlob)",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1), así que las "
                            "lecturas iniciales de CLAUDE.md y AGENTS.md (opción A de GAP-07 de A-1) no se activan; el prompt es la única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` señala las secciones que pide el paquete §2 (Scope SECTIONS) o el archivo entero "
                       "(ENTIRE). Los rangos orientan, no limitan: un archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo",
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run; un patrón que alcance cualquier otro archivo es "
                    "una lectura fuera del cierre aunque no se lea ese archivo",
            "LargeFileThresholdBytes": 60000, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > 60000],
            "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can if r["LinesOver2000Chars"]},
            "Premises": "toda premisa (PremiseRefs) se cita de líneas leídas enteras con Read: de archivos del cierre (Path relativo al clon) o de "
                        "order.txt, order-s56.txt y order-s57.txt (Path absoluto en " + RUN + " o el nombre suelto); nunca de prompt.md; una "
                        "lectura fallida nunca cuenta; una línea larga truncada por Read no cuenta como entregada",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
            "AbsentCommits": "los commits del Freeze (b64a3b64) y de V14 (4c617e82) son anteriores a un rebase y no están en el clon: V14 se "
                             "identifica por su blob en el commit (34ad80ea)",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md (lecturas iniciales, paso 4 «dotnet test»)", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "documentation-governance y el Context Pack de I-62", "Reason": "sin CLAUDE.md inyectado no hay obligación transitiva; el paquete §2 fija el cierre"},
            {"Source": "enlaces de A-3, del anexo, del paquete y de la evidencia a documentos fuera del cierre (la solicitud de recuperación de F6, "
                       "clause_map.py, ADR-0048, V14 §11.1, los resultados de las sondas pasivas, el tablero de F6, state/I-62.yml, el fixture)",
             "Reason": "citas descriptivas; el cierre lo fija la tabla de insumos del paquete §2 («no hace falta expandir los documentos que cita su prosa»)"},
            {"Source": "a3-guards.py self-test, preview y run", "Reason": "sin herramientas de ejecución; los resultados custodiados son a3-selftest.json y a3-guards-result.json"},
            {"Source": "decisiones §58 (la disposición que autoriza esta revisión)", "Reason": "se registró después del recibo y no está en el clon; su cuerpo literal es order.txt"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: correo del usuario (identidad) y estado de Git del cwd (rama main y asuntos de los commits recientes del clon)",
            "adjunto environment: cwd D:\\r62-arch-a3, plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para D:\\r62-arch-a3 (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión autora, del Coordinator y de cualquier otra sesión)",
            "otras sesiones, incluidas las revisiones de A-2 y las rondas adversariales de A-3 (su registro solo como A-3 §12)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*)",
            "los archivos de " + RUN + " distintos de los cuatro del run, incluido el subdirectorio launch/",
            "cualquier archivo del clon fuera de CanonicalInputs (también el registro de caracterización de claude-cli que fija la compuerta)",
            "Grep o Glob cuyo alcance sea un directorio con archivos fuera del cierre; listados de directorios",
            "red (fetch, web)", "invocar otros agentes o subagentes", "escribir, hacer commit o push",
        ],
        "CorpusDistinctNonAscii": len(corpus),
    }
    with open(os.path.join(KIT, "closure.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(closure, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps({"ids": IDS, "canonical": len(can), "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in RUN_FILES},
                      "questions": len(qids), "withdrawn": withdrawn, "focus": focus, "large": closure["ReadPolicy"]["LargeFiles"],
                      "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    main("--ranges" in sys.argv[1:])
