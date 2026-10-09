"""I-62 A-4, corrección 2 (docs/initiatives/I-62-A-4.md, blob 0d954376, publicada en a3332495 sobre a5b50c68; recibo cca8c60e, decisiones §62):
EffectiveInputClosure de la UNA re-revisión formal independiente del Architect por una celda `claude-cli` medida y elegible (disposición del
Coordinator §62, punto 2; decisiones §57, punto 3; autorización del Owner CLAUDE-CLI-I62 = A).

Kit adaptado del kit de la revisión 1 de A-4 (R20261009T122416Z-bce3, auditor v5.1-a4; NOT_ACCREDITED por una cita desplazada una línea) y, para
las disposiciones de los hallazgos anteriores, del kit de la re-revisión de A-2 (R20261008T184326Z-f1e2: delta.diff y objeto anterior en el run).
El cierre canónico es la tabla de insumos del paquete §2 (59052b84) en el commit del recibo, el propio paquete, las guardas de A-4 (corrección 2)
y, por orden del Coordinator, el registro de la revisión 1, la solicitud de desbloqueo de FX-02 y su clasificación (desviación declarada: el
paquete §2 no los lista). El objeto anterior (7d863219, la fila «en a5b50c68» del paquete §2) entra como archivo del run, con delta.diff. Los rangos
se calculan aquí, mecánicamente, desde los encabezados, las filas y las líneas que cita el paquete (con su primera línea comprobada), y el script
falla si algo no coincide. Los blobs se calculan con git en el commit; cada blob que fija el paquete se compara con el del commit, y los que
crecieron por añadido se comprueban como prefijo exacto.

Uso: python make_closure.py            → escribe closure.json junto a este script (el directorio del kit)
     python make_closure.py --ranges   → solo imprime los rangos (para redactar el prompt); no escribe nada
No escribe en el clon, en el run ni en el repositorio.
"""
import difflib
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
REV = None                                                   # SHA completo del recibo: lo fija main() desde HEAD del clon
REV_SHORT = "cca8c60e"                                       # recibo: decisiones §62 (corrección 2 con guardas PASS)
CORRECTION2 = "a33324957f8778dd286a5a871881d403fd5a2d14"     # corrección 2: A-4 0d954376, paquete 59052b84, guardas ampliadas
REVIEW1 = "a5b50c689ef33aa7b6fa7f7949fd72f58202e369"         # custodia de la revisión 1 (output.json, registro): A-4 = 7d863219
CORRECTION1 = "7f065a3a56102b8034247c7c64514e3f256f7b74"     # corrección previa a la revisión 1: A-4 7d863219
FIRST_PUB = "4710084b7589e27c511572d409b8742b75cfcbf4"       # primera publicación: candidata 27ffa26b
PREP_BASE = "524b293e4667810f485c64d6cbb545d5b2dd8db5"       # base de preparación: el paquete §2 nombra sus blobs en este commit
CLONE = r"D:\r62-arch-a4r"
RUN = r"D:\r62-arch-a4r-run"
IDS = {"RunId": "R20261009T150948Z-fdc2", "InvocationId": "I20261009T150948Z-fdc2", "LogicalReviewRequestId": "L20261009T150948Z-fdc2",
       "AttemptSeq": 1}
SESSION_ID = "9eb98aef-1bc0-44b9-9dc8-7e963f22bdf2"          # --session-id fijado en el kit para el único lanzamiento (uuid4)
OBJECT = "docs/initiatives/I-62-A-4.md"
OBJECT_BLOB = "0d9543761e3e3a45a94338776f7c1ba763224ab3"
PREVIOUS_BLOB = "7d863219a271b5ec427ef8669435826e19be8f11"
CANDIDATE_BLOB = "27ffa26b35ecacfae083bf9d360fe8460b8d5564"
PKG = "docs/initiatives/I-62-architect-package-A-4.md"
OUTPUT1 = "docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json"
VALIDATOR = "tests/RackCad.Tests/I62/StateV2Validator.Orchestration.cs"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A2 = "docs/initiatives/I-62-A-2.md"
A1 = "docs/initiatives/I-62-A-1.md"
A3 = "docs/initiatives/I-62-A-3.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
AP = "docs/AUTOMATION_PLAN.md"
RAE = "docs/automation/agent-execution/README.md"
DECISIONS = "docs/automation/decisions/I-62.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"
OBS03 = "docs/automation/evidence/I-62-F6/FX-02/F6-OBS-03-cmd-route-evaluation.md"
BLOCK = "docs/automation/evidence/I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json"
CC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
KITREADME = "docs/automation/evidence/I-62-F6/kits/FX-02/README.md"
O4 = "docs/automation/evidence/I-62-F6/kits/FX-02/order-FX-U1-O4.template.md"
CODEX = "docs/automation/agent-execution/adapters/codex-cli.md"
CLAUDE = "docs/automation/agent-execution/adapters/claude-cli.md"
ROUTING = "docs/automation/agent-execution/routing.md"
CATALOG = "docs/automation/agent-execution/model-catalog.md"
CHAR = "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/README.md"
RECORD1 = "docs/initiatives/I-62-architect-review-A-4.md"
UNBLOCK = "docs/automation/evidence/I-62-F6/requests/FX-02-unblock-request-2026-10-09.md"
CLASSIF = "docs/automation/evidence/I-62-F6/requests/FX-02-decision-classification-2026-10-09.md"
GUARDS = "docs/automation/evidence/I-62-A4/a4-guards.py"
SELFTEST = "docs/automation/evidence/I-62-A4/a4-selftest.json"
GUARDS_RESULT = "docs/automation/evidence/I-62-A4/a4-guards-result-c2.json"
EXPECTED_BLOBS = {
    OBJECT: OBJECT_BLOB, PKG: "59052b847ccc13e8be539189ae1b77dc7f9d3623",
    OUTPUT1: "8477374495d809bed64283d8af2976e3ce2929e8", VALIDATOR: "355e1b35f7d8c7e318f5dbd6801429d4ad19899e",
    RECORD1: "b464f2c53d40d91cfde46e4a662995bc86c9f08a",
    GUARDS: "6595d8730456d7a06fc3edbd833dfa1eef11a1bc", SELFTEST: "ddaf18f6", GUARDS_RESULT: "2fce7b58",
    UNBLOCK: "203d288750bedd3d4b8560ff01374376d9760d21", CLASSIF: "79ad2db4c957895cf98db94fd1b33a93e86e56c1",
    DECISIONS: "83269536", EVIDENCE: "a50b1dcb",
}
PACKAGE_DECLARED = {   # lo que fija el paquete §2 (59052b84; «blobs en 524b293e») o su cabecera; None = no lo lista el paquete
    OBJECT: "el del recibo (borrador de esta pasada: 0d9543761e3e3a45a94338776f7c1ba763224ab3)", PKG: "(el paquete no lleva su propio blob)",
    OUTPUT1: "84773744", VALIDATOR: "355e1b35",
    V14: "34ad80ea", A2: "f1e1d6f0", A1: "c01899a7", A3: "ea6721f7", FREEZE: "0f6b8608", LIFECYCLE: "f19896a8", AP: "f525cb1e", RAE: "592dcfd4",
    DECISIONS: "625a7075", EVIDENCE: "d132626e", OBS03: "83a960ca", BLOCK: "b4a5679b", CC: "90929ad1", KITREADME: "fd7ef02d", O4: "d92f7d3e",
    CODEX: "155f3469", CLAUDE: "ae570380", ROUTING: "bba08fc4", CATALOG: "166d978d", CHAR: "0b682899",
    RECORD1: None, UNBLOCK: None, CLASSIF: None, GUARDS: None, SELFTEST: None, GUARDS_RESULT: None,
}
APPEND_ONLY = {DECISIONS: PREP_BASE, EVIDENCE: PREP_BASE, KITREADME: PREP_BASE}   # el blob nombrado (en PREP_BASE) es prefijo exacto del texto del commit...
CITED_SECTIONS = {EVIDENCE: (84, 93, 94, 95)}               # ...o, si no, las secciones que cita el paquete son idénticas y el cambio cae fuera
RUN_FILES = ["order.txt", "prompt.md", "order-s61.txt", "order-s60.txt", "delta.diff", "A-4.7d863219.md"]
PREMISE_RUN_FILES = ["order.txt", "order-s61.txt", "order-s60.txt", "delta.diff", "A-4.7d863219.md"]   # prompt.md nunca es premisa
ORDER_TEXTS = {"order.txt": ("62", "560e043ddec6c64d5ab0ce5ef9025603cb2723b26b017f94924b9dd06479f952", 3035),
               "order-s61.txt": ("61", "c82456877990c172110d577d21cc592ed87ebc94dea20387eecb4061c17634a7", 3196),
               "order-s60.txt": ("60", "3a7257e16c955e586b2e6c300f42ba20079f0b29481dbb63f461c2a1c2217699", 2047)}
PRIOR_IDS = ["A62-A4-01"] + ["A62-A4-O%d" % n for n in range(1, 9)]
FOCUS_TOPICS = ["H1_A4-3_NO_FABRICATED_ACCEPTANCE", "H2_A4-4_REAL_INDEPENDENCE", "H3_A4-5_NO_BUDGET_RESET", "F6-OBS-03_A4-1_CMD_ROUTE",
                "A4-2_ALT_ARCHITECT_CLAUDE_CLI", "MATERIALITY_M02_M05", "U-09e", "A4-5_SEPARABILITY"]
DELTA_CONFIRMATION_KEYS = ["A42IdenticalTo7d863219", "A43IdenticalTo7d863219", "A45IdenticalTo7d863219", "A44Rules1to4IdenticalTo7d863219"]
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
ADD_DIR = ["--add-dir", RUN]   # medido en C3 de la caracterización (plantilla con marcador); lo ata kit/transport-gate.json (D-01)
TIMEOUT_S = 3600               # la revisión 1 de A-4 (mismo objeto en tamaño, cierre parecido) duró 21 minutos; margen igual que en el kit de A-4
AUTH_SETTLE_S = 120            # mitigación del intento 1 de A-3: espera entre `auth status` (preflight 5) y el lanzamiento


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


def pkg(lines, start, end, token, label):
    """Rango que cita el paquete por número de línea: se comprueba que su primera línea contiene `token`."""
    assert 1 <= start <= end <= len(lines) and token in lines[start - 1], "L%d-L%d: la línea %d no contiene %r" % (start, end, start, token)
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


def rule_item(L, rng, n, label):
    """Ítem `> n. ` de una regla citada (bloque `>`): desde su línea hasta la anterior al ítem siguiente o al final del bloque."""
    starts = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if re.match(r"^> %d\. " % n, L[i])]
    assert len(starts) == 1, (label, starts)
    s = starts[0]
    e = s
    while e < rng["End"] and L[e].startswith(">") and not re.match(r"^> \d+\. ", L[e]):
        e += 1
    return {"Section": label, "Start": s, "End": e}


def ranges_object(L):
    s35, s5, s8 = section(L, "3.5 "), section(L, "5. "), section(L, "8. ")
    disp = row(L, s8, lambda x: x.startswith("**Disposición de la revisión 1**"), "§8, «Disposición de la revisión 1» (primera línea)")
    return whole(L) + [header_block(L)] + all_sections(L) + [
        rule_item(L, s35, 5, "§3.5 A4-4, regla 5"),
        row(L, s5, lambda x: x.startswith("| M-02 |"), "§5, fila M-02"), row(L, s5, lambda x: x.startswith("| M-05 |"), "§5, fila M-05"),
        {"Section": "§8, «Disposición de la revisión 1»", "Start": disp["Start"], "End": s8["End"]}]


def ranges_package(L):
    return whole(L) + [header_block(L), row(L, {"Start": 1, "End": 40}, lambda x: x.startswith("> **Identidad exacta.**"), "nota «Identidad exacta»")] + \
        [section(L, p) for p in ("0. ", "1. ", "2. ", "3. ", "4. ", "5. ", "6. ")]


def ranges_json_top(L, items_of=("RequiredFindings", "OptionalFindings")):
    """Claves de primer nivel de un JSON con indent=1, y los elementos de las listas `items_of` (etiquetados por FindingId)."""
    keys = [(i + 1, m.group(1)) for i, m in ((i, re.match(r'^ "([A-Za-z0-9]+)": ', L[i])) for i in range(len(L))) if m]
    assert L[-1] == "}" and keys, "JSON inesperado"
    out = []
    for k, (n, name) in enumerate(keys):
        end = (keys[k + 1][0] - 1) if k + 1 < len(keys) else len(L) - 1
        out.append({"Section": name, "Start": n, "End": end})
        if name in items_of:
            i = n
            while i < end:
                if L[i] == "  {":
                    j = i
                    while L[j] not in ("  }", "  },"):
                        j += 1
                    fid = [m.group(1) for m in (re.match(r'^   "FindingId": "([^"]+)"', L[x]) for x in range(i, j)) if m]
                    assert len(fid) == 1, (name, i)
                    out.append({"Section": "%s %s" % (name, fid[0]), "Start": i + 1, "End": j + 1})
                    i = j
                i += 1
    return whole(L) + out


def ranges_validator(L):
    return [pkg(L, 119, 124, "var binding = StateTreeReader.Json", "L119-L124 (binding materializado: todos los criterios en SATISFIED)")]


def ranges_v14(L):
    s13 = section(L, "13. ")
    return [section(L, "D.3 "), row(L, section(L, "D.3 "), lambda x: x.startswith("| **Codex, total** |"), "D.3, fila «Codex, total» (topes)"),
            section(L, "D.4 "), section(L, "D.5 "),
            pkg(L, 970, 975, "| OD-1 |", "§18, filas OD (L970-L975)"),
            section(L, "8.4 "), pkg(L, 613, 621, "| T3 |", "§9.2, T3, T3' y T10 (L613-L621)"), section(L, "9.3 "),
            row(L, s13, lambda x: x.startswith("| P-22 |"), "§13, P-22"), pkg(L, 1132, 1139, "#### 20.3.2 ", "§20.3.2 (L1132-L1139)"), section(L, "B.4 "), section(L, "B.5 "),
            pkg(L, 1953, 1955, "`…last_window.closure`", "B.8.1, `closure` (L1953-L1955)"), section(L, "B.8.3 "),
            pkg(L, 2171, 2176, "**I-S18**", "I-S18 (L2171-L2176)"), section(L, "F.1 ")]


def ranges_a2(L):
    return [header_block(L), section(L, "3. A2-P2")]


def ranges_a1(L):
    s2 = section(L, "2. FC-01")
    return [header_block(L, "cabecera (formato de una A-n)"), s2, row(L, s2, lambda x: x.startswith("| D1-21 |"), "§2, fila D1-21 (enmienda de semántica)")]


def ranges_a3(L):
    return [header_block(L, "cabecera (solo contexto: paquete §2)")]


def ranges_lifecycle(L):
    return [section(L, "3. "), section(L, "5. "), section(L, "6. ")]


def ranges_ap(L):
    return [pkg(L, 507, 543, "### 16.4 ", "16.4 (L507-L543)"), pkg(L, 600, 618, "### 16.8 ", "16.8 (L600-L618)"),
            pkg(L, 1043, 1180, "### 16.19 ", "16.19-16.21 (L1043-L1180)"), pkg(L, 1325, 1373, "### 16.25 ", "16.25 (L1325-L1373)")]


def ranges_rae(L):
    return [section(L, "14. "), pkg(L, 387, 403, "Fase 1:", "§14.2, B1-B10 (L387-L403)"), pkg(L, 429, 437, "### 14.4 ", "§14.4 (L429-L437)"),
            pkg(L, 439, 486, "### 14.5 ", "§14.5 (L439-L486)"), pkg(L, 500, 509, "### 14.7 ", "§14.7, A1'-A8' (L500-L509)")]


def ranges_decisions(L):
    s = {n: section(L, "%d. " % n) for n in range(55, 63)}
    return [span(s[55], s[62], "§55-§62"), span(s[55], s[60], "§55-§60 (paquete §2)")] + [s[n] for n in range(55, 63)]


def ranges_evidence(L):
    s = {n: section(L, "%d. " % n) for n in (84, 93, 94, 95, 101, 102, 103, 104, 105)}
    return [s[84], span(s[93], s[95], "§93-§95"), s[93], s[94], s[95],
            span(s[101], s[105], "§101-§105 (guardas, revisión 1 y corrección 2; añadidas después de 524b293e)"),
            s[101], s[102], s[103], s[104], s[105]]


def ranges_cc(L):
    s7 = section(L, "7. ")
    return [section(L, "3.2 "), section(L, "4. "), s7, pkg(L, 210, 212, "| Binding |", "§7, L210-L212")]


def ranges_kitreadme(L):
    return [pkg(L, 44, 44, "| Contrato T1 |", "§0, L44 (contrato T1)"), pkg(L, 140, 140, "| S18b |", "S18b (L140)"),
            pkg(L, 144, 144, "| S21b |", "S21b (L144)"), pkg(L, 167, 167, "| N8 |", "N8 (L167)"), pkg(L, 180, 180, "| S30 |", "S30 (L180)"),
            pkg(L, 243, 249, "6. Worker:", "§4, punto 6 (L243-L249)")]


def ranges_o4(L):
    return [pkg(L, 28, 31, "`{BINDING_ACCEPTANCE_RULE}`", "marcadores de la ventana (L28-L31)"),
            pkg(L, 44, 44, "`{CORRECTION_RULE}`", "`{CORRECTION_RULE}` (L44)"), pkg(L, 88, 124, "**Bindings (16.18-16.21)**", "puntos 5-7 y 13-16 (L88-L124)")]


def ranges_routing(L):
    return [section(L, "5. "), section(L, "8. ")]


def ranges_record(L):
    return whole(L) + [header_block(L, "cabecera (resumen de la revisión 1)")] + all_sections(L)


def ranges_unblock(L):
    s21 = section(L, "2.1 ")
    s22 = section(L, "2.2 ")
    return whole(L) + all_sections(L) + [row(L, s22, lambda x: x.startswith("| U-09 |"), "§2.2, fila U-09"),
                                         row(L, s21, lambda x: x.startswith("| H-1 |"), "§2.1, fila H-1")]


def ranges_classif(L):
    s1 = section(L, "§1. ")
    return whole(L) + all_sections(L) + [row(L, s1, lambda x: x.startswith("| 9 | U-09 (e)"), "§1, fila 9: U-09 (e)")]


def ranges_guards_result(L):
    def first(token):
        hits = [i + 1 for i, x in enumerate(L) if token in x]
        assert len(hits) == 1, (token, hits)
        return hits[0]
    g2c2, g3, g4, g5, g5b = (first('"Check": "%s' % t) for t in ("G2C2 ", "G3 ", "G4 ", "G5 ", "G5b "))
    info = first("frente a 7d863219: A4-2, A4-3 y A4-5 idénticos")
    st = first('"SelfTest": {')
    return whole(L) + [{"Section": "cabecera (Tool, Mode, A4Base, Base, Head, A4Blob)", "Start": 1, "End": 7},
                       {"Section": "G2C2 (diff de la corrección 2 frente a 7d863219)", "Start": g2c2 - 1, "End": g3 - 2},
                       {"Section": "G3", "Start": g3 - 1, "End": g4 - 2}, {"Section": "G4", "Start": g4 - 1, "End": g5 - 2},
                       {"Section": "G5 (con la nota «frente a 7d863219»)", "Start": g5 - 1, "End": g5b - 2},
                       {"Section": "G5, Info «frente a 7d863219»", "Start": info, "End": info},
                       {"Section": "G5b", "Start": g5b - 1, "End": st - 2}, {"Section": "SelfTest", "Start": st, "End": len(L) - 1}]


def ranges_selftest(L):
    hits = [i + 1 for i, x in enumerate(L) if '"A4TextBlob": "%s"' % OBJECT_BLOB in x]
    assert len(hits) == 2, hits
    return whole(L) + [{"Section": "A4TextBlob = 0d954376 (escenario C2 y nivel superior)", "Start": hits[0], "End": hits[1] + 1}]


canonical = [
    (OBJECT, "objeto de la re-revisión: A-4 (corrección 2) entera (paquete §2)", "ENTIRE", ranges_object),
    (PKG, "paquete de la re-revisión (cabecera, §0 hallazgo y delta, veredicto §1, insumos §2, resumen §3, preguntas §4, condiciones §5, §6)",
     "ENTIRE", ranges_package),
    (OUTPUT1, "salida de la revisión 1 (paquete §2): A62-A4-01 y A62-A4-O1..O8 (`CorrectionRequired`, `Note`); premisa de las disposiciones",
     "ENTIRE", ranges_json_top),
    (VALIDATOR, "validador de producción de F4 (paquete §2): L119-L124, binding materializado (A4-4, regla 5)", "SECTIONS", ranges_validator),
    (V14, "paquete §2: D.3, D.4, D.5, §18 filas OD; para A4-3..A4-5: §8.4, §9.2 (T3, T3', T10), §9.3, §13 P-22, §20.3.2, B.4, B.5, B.8.1 `closure`, "
          "B.8.3, I-S18 y F.1", "SECTIONS", ranges_v14),
    (A2, "A2-P2 (paquete §2): régimen de sondas que A4-1 no debe alterar; formato de una A-n acordada", "SECTIONS", ranges_a2),
    (A1, "presupuestos de bucle del Architect que A4-2 conserva (paquete §2); D1-21 como precedente de enmienda de semántica", "SECTIONS", ranges_a1),
    (A3, "solo contexto (paquete §2: A-4 no depende de ella; no hace falta leerla entera)", "SECTIONS", ranges_a3),
    (FREEZE, "identidad del Freeze", "ENTIRE", lambda L: whole(L)),
    (LIFECYCLE, "§3 (M-01..M-08), §5 y §6 (paquete §2)", "SECTIONS", ranges_lifecycle),
    (AP, "16.4, 16.8, 16.19-16.21 y 16.25 (paquete §2)", "SECTIONS", ranges_ap),
    (RAE, "§14: B1-B10, referencias, independencia y A1'-A8' (paquete §2)", "SECTIONS", ranges_rae),
    (DECISIONS, "§55-§60 (paquete §2) y, por orden del Coordinator, §61 y §62 (§62 ordena esta re-revisión; añadidas después de 524b293e)",
     "SECTIONS", ranges_decisions),
    (EVIDENCE, "§84 y §93-§95 (paquete §2), y §101-§105 (guardas, kit y resultado de la revisión 1, corrección 2; añadidas después de 524b293e)",
     "SECTIONS", ranges_evidence),
    (OBS03, "evaluación de la ruta cmd.exe: hechos, operaciones de VERIFY no demostradas y vías V1-V4 (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (BLOCK, "resultado literal de las dos sondas del bloque A2-P2 (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (CC, "contratos del Controller de FX-02: §3.2, §4 y §7 (paquete §2)", "SECTIONS", ranges_cc),
    (KITREADME, "kit de FX-02: L44, L140, L144, L167, L180 y §4, punto 6 (paquete §2)", "SECTIONS", ranges_kitreadme),
    (O4, "plantilla de la orden O4: marcadores de la ventana, `{CORRECTION_RULE}` y puntos 5-7 y 13-16 (paquete §2)", "SECTIONS", ranges_o4),
    (CODEX, "descriptor de codex-cli (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (CLAUDE, "descriptor de claude-cli (paquete §2; Q-A4-05)", "ENTIRE", lambda L: whole(L)),
    (ROUTING, "§5 y §8 (paquete §2)", "SECTIONS", ranges_routing),
    (CATALOG, "catálogo de modelos (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (CHAR, "caracterización de claude-cli 2.1.293 (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (RECORD1, "registro de la revisión 1 (hallazgos y acreditación): añadido por orden del Coordinator", "ENTIRE", ranges_record),
    (UNBLOCK, "solicitud de desbloqueo de FX-02 (H-1..H-3, filas U-09, U-10, U-23 y U-25): añadida por orden del Coordinator; registro de propuestas, "
              "no de autorizaciones; las partes ordinarias que dispone decisiones §62, punto 3, se toman de su §3", "ENTIRE", ranges_unblock),
    (CLASSIF, "clasificación de las decisiones de FX-02 en cinco grupos (U-09 (e)): añadida por orden del Coordinator", "ENTIRE", ranges_classif),
    (GUARDS, "guardas de A-4 con el modo de la corrección 2 (evidencia de apoyo, no autoridad)", "ENTIRE", lambda L: whole(L)),
    (SELFTEST, "resultado de self-test de las guardas (A4TextBlob 0d954376)", "ENTIRE", ranges_selftest),
    (GUARDS_RESULT, "resultado de `a4-guards.py run` en modo CORRECTION2 sobre a3332495 (Head, A4Blob, G2C2, G5 y su nota frente a 7d863219, G5b)",
     "ENTIRE", ranges_guards_result),
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
    if d and re.match(r"^[0-9a-f]{8,40}$", d):
        if p in APPEND_ONLY:
            old_blob = git("rev-parse", APPEND_ONLY[p] + ":" + p).decode().strip()
            assert old_blob.startswith(d), (p, old_blob)
            old = git("cat-file", "-p", old_blob)
            r["AppendOnly"] = {"NamedBlob": old_blob, "NamedIn": APPEND_ONLY[p], "NamedLines": len(lines_of(old.decode("utf-8"))), "NamedBytes": len(old)}
            if b.startswith(old) and len(b) > len(old):
                r["AppendOnly"]["Note"] = "el texto del blob nombrado por el paquete es prefijo exacto del texto del commit (solo añadido)"
            else:
                assert p in CITED_SECTIONS, "%s: el blob nombrado no es prefijo exacto del texto del commit" % p
                OL, NL = lines_of(old.decode("utf-8")), lines_of(t)
                changes = [(tag, i1, i2, j1, j2) for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, OL, NL, autojunk=False).get_opcodes()
                           if tag != "equal" and not (tag == "insert" and i1 == len(OL))]
                for n in CITED_SECTIONS[p]:
                    so, sn = section(OL, "%d. " % n), section(NL, "%d. " % n)
                    assert OL[so["Start"] - 1:so["End"]] == NL[sn["Start"] - 1:sn["End"]], "%s §%d cambió" % (p, n)
                    assert all(i2 < so["Start"] or i1 >= so["End"] for _, i1, i2, _, _ in changes), "%s: un cambio cae en §%d" % (p, n)
                r["AppendOnly"]["NonAppendChanges"] = [{"Op": tag, "OldLines": "L%d-L%d" % (i1 + 1, i2), "NewLines": "L%d-L%d" % (j1 + 1, j2),
                                                        "OldText": OL[i1:i2], "NewText": NL[j1:j2]} for tag, i1, i2, j1, j2 in changes]
                r["AppendOnly"]["Note"] = ("crecimiento por añadido salvo los cambios de NonAppendChanges, fuera de las secciones que cita el paquete "
                                           "(%s), que son idénticas" % ", ".join("§%d" % n for n in CITED_SECTIONS[p]))
        else:
            assert blob.startswith(d), "%s: blob %s distinto del que fija el paquete (%s)" % (p, blob, d)
    if d is None:
        r["AddedBeyondPackage"] = True
    return r, t, lines_of(t)


def question_ids(L):
    s8 = section(L, "8. ")
    rows = {m.group(1): i + 1 for i, m in ((i, re.match(r"^\| (Q-A4-\d\d) \|", L[i])) for i in range(s8["Start"] - 1, s8["End"])) if m}
    ids = sorted(rows)
    assert ids == ["Q-A4-%02d" % n for n in range(1, 18)], ids
    return ids, rows


def package_questions(L):
    s4 = section(L, "4. ")
    items = [(int(m.group(1)), L[i]) for i, m in ((i, re.match(r"^(\d+)\. ", L[i])) for i in range(s4["Start"] - 1, s4["End"])) if m]
    assert [n for n, _ in items] == list(range(1, 12)), items
    assert "Q-A4-15..Q-A4-17" in items[-1][1], items[-1]
    return [n for n, _ in items]


def prior_ids_from_output(text):
    o = json.loads(text)
    assert o["ReviewedBlob"] == PREVIOUS_BLOB and o["ReviewedPath"] == OBJECT and o["Verdict"] == "CHANGES REQUIRED", "output.json no es la revisión de 7d863219"
    ids = [f["FindingId"] for f in o["RequiredFindings"]] + [f["FindingId"] for f in o["OptionalFindings"]]
    assert ids == PRIOR_IDS, ids
    return ids


def order_text_check(dec_lines, num, want, size):
    s = section(dec_lines, "%s. " % num)
    text = "\n".join(dec_lines[s["Start"] - 1:s["End"]])
    return ("`%s`" % want) in text and ("%d %03d bytes" % (size // 1000, size % 1000)) in text


def run_identity():
    """delta.diff y A-4.7d863219.md: salida literal de git en el clon; se comprueba contra los blobs y contra el diff blob a blob."""
    delta = git("diff", REVIEW1, CORRECTION2, "--", OBJECT)
    blobdiff = git("diff", PREVIOUS_BLOB, OBJECT_BLOB)
    prev = git("show", REVIEW1 + ":" + OBJECT)
    assert git("rev-parse", REVIEW1 + ":" + OBJECT).decode().strip() == PREVIOUS_BLOB
    assert git("rev-parse", CORRECTION1 + ":" + OBJECT).decode().strip() == PREVIOUS_BLOB
    assert git("rev-parse", CORRECTION2 + ":" + OBJECT).decode().strip() == OBJECT_BLOB
    assert git("rev-parse", FIRST_PUB + ":" + OBJECT).decode().strip() == CANDIDATE_BLOB
    assert git("rev-parse", REVIEW1 + ":" + PKG).decode().strip().startswith("085f30f6")
    assert hashlib.sha1(b"blob %d\0" % len(prev) + prev).hexdigest() == PREVIOUS_BLOB
    dl, bl = delta.decode("utf-8").split("\n"), blobdiff.decode("utf-8").split("\n")
    assert dl[1] == "index %s..%s 100644" % (PREVIOUS_BLOB[:8], OBJECT_BLOB[:8]) and dl[0] == "diff --git a/%s b/%s" % (OBJECT, OBJECT), dl[:2]
    assert dl[1] == bl[1] and dl[4:] == bl[4:], "las líneas de cambio del diff por ruta y del diff blob a blob difieren"
    return {"delta.diff": delta, "A-4.7d863219.md": prev}


def main(print_only):
    global REV
    REV = git("rev-parse", "HEAD").decode().strip()
    assert REV.startswith(REV_SHORT), "HEAD del clon distinto del recibo"
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    expected_run = run_identity()
    can, corpus = [], set()
    qids = qrows = pq = dec_lines = prior = None
    for p, why, scope, rf in canonical:
        r, t, L = rec(p)
        r["Why"], r["Scope"], r["LineRanges"] = why, scope, rf(L)
        for g in r["LineRanges"]:
            assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
        can.append(r)
        corpus |= {c for c in t if ord(c) > 127}
        if p == OBJECT:
            qids, qrows = question_ids(L)
        if p == PKG:
            pq = package_questions(L)
        if p == DECISIONS:
            dec_lines = L
        if p == OUTPUT1:
            prior = prior_ids_from_output(t)
    ranges = {r["Path"]: [(g["Section"][:60], g["Start"], g["End"]) for g in r["LineRanges"]] for r in can}
    if print_only:
        print(json.dumps({"files": {r["Path"]: (r["Bytes"], r["Lines"], r["Blob"][:8], r["LinesOver2000Chars"]) for r in can},
                          "ranges": ranges, "questions": qids, "questionRows": qrows, "packageQuestions": pq, "prior": prior},
                         ensure_ascii=False, indent=1))
        return
    run = {}
    for n in RUN_FILES:
        b = open(os.path.join(RUN, n), "rb").read()
        assert b == open(os.path.join(KIT, n), "rb").read(), "%s del run distinto de la copia del kit" % n
        if n in expected_run:
            assert b == expected_run[n], "%s del run distinto de la salida de git en el clon" % n
        d, t = describe(b)
        run[n] = d
        if n != "prompt.md":
            corpus |= {c for c in t if ord(c) > 127}
    for n, (num, want, size) in ORDER_TEXTS.items():
        assert run[n]["Sha256"] == want and run[n]["Bytes"] == size and order_text_check(dec_lines, num, want, size), \
            "%s distinto del texto que custodia decisiones §%s" % (n, num)
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §62 del Coordinator (3 035 bytes; SHA-256 560e043d…, el que declara decisiones §62 "
                            "en el commit); su punto 2 ordena esta re-revisión y sus ocho temas",
               "prompt.md": "copia de kit/prompt.md; stdin literal del proceso claude-cli; nunca es premisa",
               "order-s61.txt": "cuerpo exacto pegado de la disposición §61 del Coordinator (3 196 bytes; SHA-256 c8245687…, el que declara decisiones "
                                "§61 en el commit); su punto 2 ordenó la revisión 1 y nombra la candidata 27ffa26b",
               "order-s60.txt": "cuerpo exacto pegado de la disposición §60 del Coordinator (2 047 bytes; SHA-256 3a7257e1…, el que declara decisiones "
                                "§60 en el commit); su punto 2 es el origen de A-4",
               "delta.diff": "salida literal de `git diff %s %s -- %s` en el clon (línea index %s..%s); sus líneas de cambio son las de `git diff "
                             "%s %s` (diff blob a blob)" % (REVIEW1[:8], CORRECTION2[:8], OBJECT, PREVIOUS_BLOB[:8], OBJECT_BLOB[:8],
                                                             PREVIOUS_BLOB[:8], OBJECT_BLOB[:8]),
               "A-4.7d863219.md": "objeto anterior literal: salida de `git show %s:%s` (blob %s, el revisado en la revisión 1; igual en %s)"
                                  % (REVIEW1[:8], OBJECT, PREVIOUS_BLOB, CORRECTION1[:8])}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    for n, (num, want, size) in ORDER_TEXTS.items():
        run_hashes[n]["IdentityCheck"] = {"DecisionsSection": "§" + num, "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": size}
    run_hashes["delta.diff"]["IdentityCheck"] = {"GitCommand": "git diff %s %s -- %s" % (REVIEW1, CORRECTION2, OBJECT),
                                                 "IndexLine": "index %s..%s 100644" % (PREVIOUS_BLOB[:8], OBJECT_BLOB[:8]),
                                                 "BlobToBlobChangeLinesEqual": True}
    run_hashes["A-4.7d863219.md"]["IdentityCheck"] = {"GitCommand": "git show %s:%s" % (REVIEW1, OBJECT), "GitBlob": PREVIOUS_BLOB}
    receipt = {"Commit": REV, "Correction2Commit": CORRECTION2, "Review1Commit": REVIEW1, "Correction1Commit": CORRECTION1, "FirstPublication": FIRST_PUB,
               "CandidateHistory": [CANDIDATE_BLOB, PREVIOUS_BLOB, OBJECT_BLOB],
               "Object": {"Path": OBJECT, "Blob": OBJECT_BLOB}, "Package": {"Path": PKG, "Blob": EXPECTED_BLOBS[PKG]},
               "Guards": {GUARDS: EXPECTED_BLOBS[GUARDS], SELFTEST: [r["Blob"] for r in can if r["Path"] == SELFTEST][0],
                          GUARDS_RESULT: [r["Blob"] for r in can if r["Path"] == GUARDS_RESULT][0]},
               "Note": "el paquete designa el objeto como «blob <el del commit de publicación> (borrador de esta pasada: 0d954376…)» y nombra sus "
                       "insumos con blobs; el kit los resuelve con los blobs del commit del recibo"}
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "ObjectIntroducedBy": CORRECTION2, "Review1Commit": REVIEW1, "FirstPublication": FIRST_PUB,
        "PreparationBase": PREP_BASE,
        "ObjectPath": OBJECT, "ObjectBlob": OBJECT_BLOB, "CandidateBlob": CANDIDATE_BLOB, "PackagePath": PKG, "PackageBlob": EXPECTED_BLOBS[PKG],
        "PublicationReceipt": receipt,
        "PreviousObject": {"Path": OBJECT, "Blob": PREVIOUS_BLOB, "Review": "R20261009T122416Z-bce3", "RunFile": "A-4.7d863219.md",
                           "Delta": "delta.diff", "Verdict": "CHANGES REQUIRED (auditor NOT_ACCREDITED; decisiones §62, punto 1)"},
        "PriorFindingIds": prior,
        "PriorFindingSource": "revisión 1 (output.json): A62-A4-01 (REQUIRED; decisiones §62, punto 1: ACCEPTED REQUIRED) y A62-A4-O1..O8 "
                              "(OPTIONAL; ACCEPTED NON-BLOCKING)",
        "PriorFindingDispositions": {"A62-A4-01": ["CLOSED", "STILL_OPEN"], "A62-A4-O1..O8": ["APPLIED", "NOT_APPLIED", "N/A"]},
        "Harness": GUARDS, "HarnessExecution": "ninguna por el revisor: no tiene herramientas de ejecución; lee el código y los resultados custodiados",
        "FocusTopics": FOCUS_TOPICS,
        "FocusSource": "disposición §62, punto 2 (order.txt L21-L28): H-1 y ausencia de aceptación fabricada; H-2 e independencia real; H-3 y "
                       "presupuestos sin reinicios; F6-OBS-03 y la ruta cmd.exe; Architect alternativo por claude-cli; materialidad M-02/M-05; "
                       "U-09 (e); separabilidad de A4-5",
        "DeltaConfirmationKeys": DELTA_CONFIRMATION_KEYS,
        "PackageQuestions": pq,
        "QuestionIds": qids, "QuestionRows": qrows, "QuestionSource": "A-4 §8 (paquete §1 y §4, punto 11: Q-A4-01..Q-A4-17)",
        "A45Verdict": "veredicto separado de A4-5 (paquete §1: «el acuerdo puede excluirla»): AGREED | CHANGES REQUIRED | EXCLUDE | "
                      "BLOCKED — OWNER DECISION; NOT_ASSESSED solo con NOT_ACCREDITED",
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
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a4r\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S,
                      "AuthSettleSeconds": AUTH_SETTLE_S,
                      "AuthSettle": "launch.py espera AUTH_SETTLE_S segundos entre el final de `auth status` (preflight 5) y el lanzamiento, y registra en "
                                    "run.json como TRANSPORT_AUTH_REFRESH_CONFLICT un result con «Failed to refresh OAuth token» y 0 tokens (sin "
                                    "reintento); heredado del intento 2 de A-3 y del kit de A-4",
                      "Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; aceptada como evidencia "
                                          "de elegibilidad vigente en decisiones §57, punto 3): C1 completada, C2 cancelada por terminación, C3 PASS "
                                          "sobre la plantilla con --add-dir"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos seis archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json) "
                                "y el auditor v5.1-a4r lo vuelve a verificar después; el revisor contrasta lo que puede leer: delta.diff (línea index y "
                                "rutas), la cabecera y §0 del paquete, a4-guards-result-c2.json (Mode, Head, A4Blob y la nota de G5 frente a 7d863219) "
                                "y a4-selftest.json (A4TextBlob)",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
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
                        "order.txt, order-s61.txt, order-s60.txt, delta.diff y A-4.7d863219.md (Path absoluto en " + RUN + " o el nombre suelto); "
                        "nunca de prompt.md; una lectura fallida nunca cuenta; una línea larga truncada por Read no cuenta como entregada; los "
                        "números de línea se citan exactos (LineStart..LineEnd deben contener la cita)",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
            "AbsentCommits": "los commits del Freeze (b64a3b64) y de V14 (4c617e82) son anteriores a un rebase y no están en el clon: V14 se "
                             "identifica por su blob en el commit (34ad80ea)",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md (lecturas iniciales, paso 4 «dotnet test»)", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "documentation-governance y el Context Pack de I-62", "Reason": "sin CLAUDE.md inyectado no hay obligación transitiva; el cierre lo fijan el paquete §2 y la orden del Coordinator"},
            {"Source": "enlaces de A-4, del paquete, del registro, de la solicitud y de la evidencia a documentos fuera del cierre (audit.json y "
                       "runtime-evidence.json de la revisión 1, el resto del kit de FX-02, `frontiers.json`, el resto del bloque A2-P2, ADR-0046 y "
                       "ADR-0048, el tablero de F6, state/I-62.yml, el fixture)",
             "Reason": "citas descriptivas; el cierre lo fijan la tabla de insumos del paquete §2 y la orden del Coordinator"},
            {"Source": "a4-guards.py self-test, preview y run", "Reason": "sin herramientas de ejecución; los resultados custodiados son a4-selftest.json y a4-guards-result-c2.json"},
            {"Source": "a4-guards-result.json y a4-guards-result-preview.json (modo CORRECTION de la revisión 1 y vista previa)",
             "Reason": "fuera de la orden del Coordinator: rige el resultado del modo CORRECTION2 sobre el commit exacto"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: correo del usuario (identidad) y estado de Git del cwd (rama main y asuntos de los commits recientes del clon)",
            "adjunto environment: cwd D:\\r62-arch-a4r, plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para D:\\r62-arch-a4r (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión autora, del Coordinator y de cualquier otra sesión, incluida la revisión 1)",
            "otras sesiones, incluidas las revisiones de A-2, A-3 y la revisión 1 de A-4 (solo su output.json custodiado, que está en el cierre)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*)",
            "los archivos de " + RUN + " distintos de los seis del run, incluido el subdirectorio launch/",
            "cualquier archivo del clon fuera de CanonicalInputs (también el registro de caracterización de claude-cli que fija la compuerta)",
            "Grep o Glob cuyo alcance sea un directorio con archivos fuera del cierre; listados de directorios",
            "red (fetch, web)", "invocar otros agentes o subagentes", "escribir, hacer commit o push",
        ],
        "CorpusDistinctNonAscii": len(corpus),
    }
    with open(os.path.join(KIT, "closure.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(closure, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps({"ids": IDS, "canonical": len(can), "bytes": sum(r["Bytes"] for r in can),
                      "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in RUN_FILES},
                      "questions": len(qids), "prior": prior, "large": closure["ReadPolicy"]["LargeFiles"],
                      "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    main("--ranges" in sys.argv[1:])
