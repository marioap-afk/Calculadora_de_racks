"""I-62 A-5 (docs/initiatives/I-62-A-5.md, blob ad94ceed, publicada en bf2cf857 sobre efcfcbc5; guardas `run` PASS custodiadas en 1a0db7b0;
decisiones §72 en b631c46f): EffectiveInputClosure de la PRIMERA revisión formal independiente del Architect por una celda `claude-cli` medida y
elegible (disposición del Coordinator §72, punto 2; autorización del Owner CLAUDE-CLI-I62 = A).

Kit adaptado del kit de la segunda re-revisión de A-4 (R20261009T195811Z-2df6, auditor v5.1-a4r2, ACCREDITED; decisiones §64, punto 1). A-5 no
tiene paquete del Architect (A-5 §6: el paquete solo se crea «cuando una disposición autorice la revisión») ni revisiones anteriores: el cierre
canónico lo fijan el objeto, lo que el objeto cita con blob o con número de línea y la orden del Coordinator (§72, punto 2: los ocho temas). Lo
componen, en el commit de decisiones §72 (b631c46f): A-5 entera; V14 (D.1, D.2, D.3, D.4, D.5, D.6, D.8, Anexo C filas C-22/C-25a/C-39, §17 y
§18); A-1 y A-3 (cabeceras), A-2 y A-4; el Freeze; LIFECYCLE §3, §5, §6 y §8; AUTOMATION_PLAN 16.13 y 16.14; el mapa de cláusulas y su esquema;
`clause_map.py` de F4; todo `docs/automation/evidence/I-62-A5/` (preflight, guardas, autoprueba, resultados y `out/`); de
`I-62-F6/MAP-INVALID/`, `characterization.md`, `effects.md`, `reconstruction-plan.md` y `mv_check.py`; la conciliación de U-14 y el borrador de
B1; decisiones §46, §48, §51 y §63-§72; evidencia §115-§133; el README de F6; y, solo como referencia de formato, el registro y la salida de la
segunda re-revisión de A-4. Los rangos se calculan aquí, mecánicamente, desde los encabezados, las filas y las líneas que cita A-5 (con su primera
línea comprobada), y el script falla si algo no coincide. Los blobs se calculan con git en el commit; cada blob que A-5 o decisiones §72 fijan se
compara con el del commit; decisiones y evidencia, que crecieron después de la publicación, se comprueban como prefijo exacto del blob de bf2cf857.

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
REV = None                                                   # SHA completo de decisiones §72: lo fija main() desde HEAD del clon
REV_SHORT = "b631c46f"                                       # decisiones §72: revisión formal de A-5 autorizada
PUBLICATION = "bf2cf857b69ada371590cde2c8ddfbc8b3581fab"     # A-5 candidata publicada (blob ad94ceed), con preflight PASS
PREP_BASE = "efcfcbc5a9f89de0dc4f4f21a426b4723ef18245"       # base de la publicación (decisiones §71); padre de bf2cf857
GUARDS_RUN = "1a0db7b03b1804145571a6733ca4eb079cbd1f08"      # guardas `run` sobre bf2cf857 = PASS (custodia)
MAP_BASE = "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"        # base del mapa (C-20b): EFF^1 de la derivación
CLONE = r"D:\r62-arch-a5"
RUN = r"D:\r62-arch-a5-run"
IDS = {"RunId": "R20261011T065226Z-dbb0", "InvocationId": "I20261011T065226Z-dbb0", "LogicalReviewRequestId": "L20261011T065226Z-dbb0",
       "AttemptSeq": 1}
SESSION_ID = "dcb1b798-b53a-4a31-9693-61cd4a2dfe51"          # --session-id fijado en el kit para el único lanzamiento (uuid4)
OBJECT = "docs/initiatives/I-62-A-5.md"
OBJECT_BLOB = "ad94ceedacbf3dbf70f0217a583978d0e32592fc"
V14 = "docs/initiatives/I-62-proposal-v14.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
A1, A2, A3, A4 = ("docs/initiatives/I-62-A-%d.md" % i for i in (1, 2, 3, 4))
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
AP = "docs/AUTOMATION_PLAN.md"
MAP = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
MAP_SCHEMA = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
CLAUSE_MAP = "docs/automation/evidence/I-62-F4/compat/clause_map.py"
EVID = "docs/automation/evidence/I-62-A5/"
GUARDS, SELFTEST, GUARDS_RUN_RESULT = EVID + "a5-guards.py", EVID + "a5-selftest.json", EVID + "a5-guards-result-run.json"
PREFLIGHT = EVID + "preflight.md"
MAPINV = "docs/automation/evidence/I-62-F6/MAP-INVALID"
CHARMD, EFFECTS, PLAN, MV_CHECK = (MAPINV + "/" + n for n in ("characterization.md", "effects.md", "reconstruction-plan.md", "mv_check.py"))
U14 = "docs/automation/evidence/I-62-F6/FX-02/s62/u14-reconciliation.md"
B1DRAFT = "docs/automation/evidence/I-62-F6/requests/B1-fixture-request-draft-2026-10-11.md"
DECISIONS = "docs/automation/decisions/I-62.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"
F6README = "docs/automation/evidence/I-62-F6/README.md"
R3RECORD = "docs/initiatives/I-62-architect-review-A-4-r3.md"
R3OUTPUT = "docs/automation/evidence/I-62-architect-A-4/R20261009T195811Z-2df6/output.json"
AE_TREE = "docs/automation/agent-execution"
DECLARED = {   # blob que fija A-5 (cabecera, §2, §6 y §9) o decisiones §72 (objeto); None = el objeto no lo fija
    OBJECT: "ad94ceed (decisiones §72, punto 1)", V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f",
    LIFECYCLE: "f19896a8", AP: "f525cb1e", MAP: "4d49d3e1", MAP_SCHEMA: "6b54dda9", CLAUSE_MAP: "ffe6ea57", MV_CHECK: "9536d051",
    CHARMD: "1d74c2da", PLAN: "d0b2fd25", EFFECTS: "c817adc0", U14: "05f62382", B1DRAFT: "541d19e5",
}
DECLARED_TREES = {MAPINV: "b0b3b258", AE_TREE: "809fd58f"}   # árboles que fijan A-5 §6 y §9 (G3)
APPEND_SINCE_PUBLICATION = (DECISIONS, EVIDENCE)               # blob en bf2cf857 = prefijo exacto del blob del commit
RUN_FILES = ["order.txt", "prompt.md", "delta.diff"]
PREMISE_RUN_FILES = ["order.txt", "delta.diff"]               # prompt.md nunca
ORDER_TEXT = ("72", "0baac461aec463162df278220174ed5527af46c0bc242b8eac23bbcc623333fc", 2556)
FOCUS_TOPICS = ["A5-1_CONFORMS_AP_16_13_16_14", "ORIGINAL_FIXTURE_HISTORY_INTEGRITY", "IMPACT_C-22_C-25a", "BUDGET_SEPARATION_A4-6_A5-2",
                "NO_RESETS_NO_QUOTA_TRANSFERS", "D5_AND_OWNER_VALIDATION", "OWNER_RESERVED_DECISIONS", "GUARDS_AND_NEGATIVES_SUFFICIENCY"]
SCOPE_CONFIRMATION_KEYS = ["A51AppendAtEndOfD1", "A52AppendAtEndOfD3AfterA46", "NoFrozenTextReplaced", "A51InnerQuoteEqualsV14L2391",
                           "A52FiguresEqualV14L2442"]
OWNER_LINES = ["A5-FIXTURE-B1-REPO", "A5-PRINCIPAL-FXU2-CONSUMO"]
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
ADD_DIR = ["--add-dir", RUN]   # medido en C3 de la caracterización (plantilla con marcador); lo ata kit/transport-gate.json (D-01)
TIMEOUT_S = 3600               # las revisiones de A-4 (cierres de 2,2 MB) duraron unos 11-21 minutos
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


def cited(lines, start, end, token, label):
    """Rango que A-5 (o el preflight) cita por número de línea: se comprueba que su primera línea contiene `token`."""
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


def rule_items(L, rng, label):
    """Ítems `> n. ` de una regla citada (bloque `>`): cada uno desde su línea hasta la anterior al ítem siguiente o al final del bloque."""
    starts = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if re.match(r"^> \d+\. ", L[i])]
    out = []
    for k, s in enumerate(starts):
        e = s
        while e < rng["End"] and L[e].startswith(">") and not re.match(r"^> \d+\. ", L[e]):
            e += 1
        n = int(re.match(r"^> (\d+)\. ", L[s - 1]).group(1))
        assert n == k + 1, (label, starts)
        out.append({"Section": "%s, regla %d" % (label, n), "Start": s, "End": e})
    return out


def quote_block(L, rng, label):
    """Bloque `> «…»` del párrafo normativo propuesto dentro de una sección del delta."""
    lines = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if L[i].startswith("> ")]
    assert lines and L[lines[0] - 1].startswith("> «") and L[lines[-1] - 1].endswith("»"), label
    assert lines == list(range(lines[0], lines[-1] + 1)), label
    return {"Section": label, "Start": lines[0], "End": lines[-1]}


def ranges_object(L):
    s32, s33, s5, s7, s8 = section(L, "3.2 "), section(L, "3.3 "), section(L, "5. "), section(L, "7. "), section(L, "8. ")
    q51, q52 = quote_block(L, s32, "§3.2, párrafo A5-1"), quote_block(L, s33, "§3.3, párrafo A5-2")
    fence = [i + 1 for i in range(s7["Start"] - 1, s7["End"]) if L[i].startswith("```")]
    assert len(fence) == 2, fence
    return whole(L) + [header_block(L)] + all_sections(L) + [q51, q52] + rule_items(L, q51, "§3.2 A5-1") + rule_items(L, q52, "§3.3 A5-2") + [
        row(L, s5, (lambda m: lambda x: x.startswith("| %s |" % m))(m), "§5, fila %s" % m) for m in ("M-0%d" % i for i in range(1, 9))] + [
        {"Section": "§7, líneas exactas del Owner (bloque text)", "Start": fence[0], "End": fence[1]}] + [
        row(L, s8, (lambda q: lambda x: x.startswith("| %s |" % q))(q), "§8, %s" % q) for q in ("Q-A5-%02d" % i for i in range(1, 13))]


def ranges_v14(L):
    d1, d3, d4, d5 = section(L, "D.1 "), section(L, "D.3 "), section(L, "D.4 "), section(L, "D.5 ")
    annex_c = section(L, "Anexo C ")
    s18 = section(L, "18. ")
    return [annex_c, row(L, annex_c, lambda x: x.startswith("| C-22 |"), "Anexo C, C-22 (L2361; A-5 §2)"),
            row(L, annex_c, lambda x: x.startswith("| C-25a |"), "Anexo C, C-25a"), row(L, annex_c, lambda x: x.startswith("| C-39 |"), "Anexo C, C-39"),
            d1, cited(L, 2388, 2397, "Lo crea la sesión de supervisión", "D.1, introducción y pasos 1-3 (L2388-L2397; A-5 §2)"),
            cited(L, 2391, 2391, "copias **byte a byte** de las autoridades de RackCad en MC_I62", "D.1, paso 2, primera viñeta (L2391; A5-1)"),
            cited(L, 2394, 2394, "`FIXTURE-MANIFEST.json`", "D.1, paso 2, manifiesto (L2394; Q-A5-04)"),
            cited(L, 2395, 2395, "con el commit **F_norm**", "D.1, paso 3 (L2395; Q-A5-02)"),
            cited(L, 2398, 2406, "4. Unidad de prueba **FX-U1**", "D.1, pasos 4-6 (L2398-L2406; A-5 §2)"),
            section(L, "D.2 "), d3,
            cited(L, 2437, 2437, "| Principal A (", "D.3, fila Principal A (L2437)"),
            cited(L, 2442, 2445, "| **Codex, total** |", "D.3, «Codex, total», sondas previas, Worker y Reviewer (L2442-L2445)"),
            cited(L, 2523, 2523, "| A | ≤ 15 + 2 sondas |", "D.3, totales de la ronda A (L2523)"),
            cited(L, 2528, 2528, "Ningún tope autoriza gasto ni ejecución ahora.", "D.3, cierre (L2528)"),
            d4, cited(L, 2544, 2544, "| FX-02 PASS |", "D.4, FX-02 PASS (L2544)"),
            d5, cited(L, 2558, 2558, "Se resiembra el fixture desde el Candidato", "D.5, resiembra para OV (L2558; Q-A5-01)"),
            section(L, "D.6 "), section(L, "D.8 "), section(L, "17. "), s18,
            row(L, s18, lambda x: x.startswith("| OV-I62-03 "), "§18, OV-I62-03"), row(L, s18, lambda x: x.startswith("| OV-I62-06 "), "§18, OV-I62-06"),
            cited(L, 970, 975, "| OD-1 |", "§18, filas OD-1..OD-7 (L970-L975)"),
            cited(L, 974, 974, "| OD-5 |", "§18, OD-5 (L974)"), cited(L, 975, 975, "| OD-7 |", "§18, OD-7 (L975)")]


def ranges_ap(L):
    s13, s14 = section(L, "16.13 "), section(L, "16.14 ")
    return [s13, s14, cited(L, 731, 731, "| Mapa de cláusulas |", "16.13, mapa en I62_EFFECTIVE_SHA (L731)"),
            cited(L, 822, 822, "Cualquier fallo → MAP_INVALID", "16.13, Resolve paso 2 (L822)"),
            cited(L, 902, 908, "| MV-1 |", "16.13, Validate(M): MV-1..MV-7 (L902-L908)"),
            cited(L, 920, 921, "**Con MAP_INVALID, ningún contrato I61 pasa `Authority` tras EFF.**", "16.13, MAP_INVALID (L920-L921)"),
            cited(L, 945, 946, "**Punto efectivo:**", "16.14, punto efectivo (L945-L946)")]


def ranges_a2(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 148, 148, "Principal B de FX-04a, Principal de la Topología B y Principal A de D.8)", "A2-P1, filas (L148)"),
        cited(L, 156, 156, "> 3. su lanzamiento físico queda registrado", "A2-P1, regla 3 (L156)"),
        cited(L, 159, 159, "la aprobación de OD-5 (consumo dentro de los topes de D", "A2-P1, regla 4 (L159)"),
        cited(L, 170, 170, "Esta regla no se aplica a D.5", "A2-P1, cierre (L170)"),
        cited(L, 233, 233, "Las sondas de un bloque no consumen", "A2-P2 (L233)")]


def ranges_a4(L):
    s7 = section(L, "7. ")
    fence = [i + 1 for i in range(s7["Start"] - 1, s7["End"]) if L[i].startswith("```")]
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 203, 203, "Su tope es uno en total para FX-02 en F6", "A4-1, regla 3 (L203)"),
        cited(L, 351, 351, "designado por T16", "A4-6 (L351)"), cited(L, 355, 355, "> 2. ese tope y ese total suben en **una** sesión", "A4-6, regla 2 (L355)"),
        cited(L, 361, 361, "> 5. no es la reejecución extraordinaria de A-2", "A4-6, regla 5 (L361)"),
        {"Section": "§7, líneas del Owner (bloque text)", "Start": fence[0], "End": fence[1]}]


def ranges_header_only(L, label):
    return [header_block(L, label)]


def ranges_lifecycle(L):
    return [section(L, "3. "), section(L, "5. "), section(L, "6. "), section(L, "8. ")]


def ranges_decisions(L):
    s = {n: section(L, "%d. " % n) for n in (46, 48, 51) + tuple(range(63, 73))}
    return [s[46], s[48], s[51], span(s[63], s[72], "§63-§72"),
            cited(L, 1265, 1265, "| **2. U-14** |", "§63, punto 2 (DEC L1265)"),
            cited(L, 1454, 1454, "| **1. Controller** |", "§71, punto 1 (DEC L1454)"),
            cited(L, 1456, 1456, "| **3. Fixture B1** |", "§71, punto 3 (DEC L1456)"),
            cited(L, 1458, 1458, "| **5. Límites** |", "§71, punto 5 (DEC L1458)"),
            cited(L, 1469, 1470, "| **1. A-5** |", "§72, puntos 1 y 2 (objeto y revisión)")] + [s[n] for n in range(63, 73)]


def ranges_evidence(L):
    s = {n: section(L, "%d. " % n) for n in range(115, 134)}
    return [span(s[115], s[133], "§115-§133")] + [s[n] for n in range(115, 134)]


def ranges_doc(L, label="cabecera"):
    return whole(L) + [header_block(L, label)] + all_sections(L)


def ranges_json(L):
    keys = [(i + 1, m.group(1)) for i, m in ((i, re.match(r'^ "([A-Za-z0-9_]+)": ', L[i])) for i in range(len(L))) if m]
    out = whole(L)
    for k, (n, name) in enumerate(keys):
        end = (keys[k + 1][0] - 1) if k + 1 < len(keys) else len(L) - 1
        out.append({"Section": name, "Start": n, "End": end})
    return out


def ranges_whole(L):
    return whole(L)


canonical = [
    (OBJECT, "objeto de la revisión: A-5 entera (A5-1, A5-2, Q-A5-01..Q-A5-12)", "ENTIRE", ranges_object),
    (V14, "Proposal V14 congelada: D.1 (pasos 1-6; L2388-L2406), D.2, D.3 (L2437, L2442-L2445, L2523, L2528), D.4 (L2544), D.5 (L2558), D.6, D.8, "
          "Anexo C (C-22, C-25a, C-39), §17 y §18 (OV-I62-03/06; OD-1..OD-7, L970-L975)", "SECTIONS", ranges_v14),
    (A2, "A-2 acordada (A2-P1 L148, L156, L159, L170; A2-P2 L233): presupuestos de recuperación que A-5 no amplía", "ENTIRE", ranges_a2),
    (A4, "A-4 acordada (A4-1 L203; A4-6 L351, L355, L361; §7): el cupo de A4-6 que A5-2 no debe tocar", "ENTIRE", ranges_a4),
    (A1, "A-1 acordada: solo la cabecera (A-5 no la modifica)", "SECTIONS", lambda L: ranges_header_only(L, "cabecera (A-5 no la modifica)")),
    (A3, "A-3 acordada: solo la cabecera (A-5 no la modifica)", "SECTIONS", lambda L: ranges_header_only(L, "cabecera (A-5 no la modifica)")),
    (FREEZE, "identidad del Freeze", "ENTIRE", ranges_whole),
    (LIFECYCLE, "§3 (M-01..M-08 y OWNER-RESERVED), §5, §6 (veredicto formal y enmiendas) y §8", "SECTIONS", ranges_lifecycle),
    (AP, "AUTOMATION_PLAN 16.13 (Evaluate/Resolve/Validate, MV-1..MV-7; L731, L822, L902-L908, L920-L921) y 16.14 (punto efectivo, L945-L946)",
     "SECTIONS", ranges_ap),
    (MAP, "mapa de cláusulas de MC_I62 (Files, FileKind, BaseBlob, EffBlob, Entries): sin cambio por A-5", "ENTIRE", ranges_whole),
    (MAP_SCHEMA, "esquema del mapa de cláusulas: sin cambio por A-5", "ENTIRE", ranges_whole),
    (CLAUSE_MAP, "comprobador C-20b de F4 que usan G4 y el contraste del preflight (sin cambio)", "ENTIRE", ranges_whole),
    (CHARMD, "caracterización de MAP_INVALID del fixture actual (A-5 §3.1)", "ENTIRE", ranges_doc),
    (EFFECTS, "efectos de la reconstrucción sobre pruebas y evidencia anteriores (C-22, C-25a, historia del fixture)", "ENTIRE", ranges_doc),
    (PLAN, "plan de reconstrucción de la activación (opciones (a)-(d); orden recomendado)", "ENTIRE", ranges_doc),
    (MV_CHECK, "comprobador de MV-1..MV-7 usado por el preflight (sin cambio)", "ENTIRE", ranges_whole),
    (U14, "conciliación de las sesiones de Principal de la ronda A (A y R) y presupuesto (A5-2)", "ENTIRE", ranges_doc),
    (B1DRAFT, "borrador de la solicitud de B1 (origen de A-5)", "ENTIRE", ranges_doc),
    (DECISIONS, "§46 y §48 (OD-5, OD-7 y repositorio público), §51 (C-22 PASS), §63 (U-14) y §64-§72 (§71 prepara A-5; §72 ordena esta revisión)",
     "SECTIONS", ranges_decisions),
    (EVIDENCE, "§115-§133 (FX-02, MAP_INVALID, bloque A2-P2, preparación, publicación y guardas de A-5)", "SECTIONS", ranges_evidence),
    (F6README, "README de la evidencia de F6 (escenarios, identidad del fixture, obligaciones)", "ENTIRE", ranges_doc),
    (R3RECORD, "registro de la segunda re-revisión de A-4 (solo referencia de formato; no es insumo del juicio sobre A-5)", "ENTIRE", ranges_doc),
    (R3OUTPUT, "salida de la segunda re-revisión de A-4 (solo referencia de formato; no es insumo del juicio sobre A-5)", "ENTIRE", ranges_json),
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


def evidence_dir_files():
    names = git("ls-tree", "-r", "--name-only", REV, EVID).decode("utf-8").split("\n")
    names = [n for n in names if n]
    assert names and all(n.startswith(EVID) for n in names), names
    return sorted(names)


def rec(p):
    b = git("show", REV + ":" + p)
    r, t = describe(b)
    blob = git("rev-parse", REV + ":" + p).decode().strip()
    r = dict({"Path": p, "Blob": blob}, **r)
    d = DECLARED.get(p)
    r["DeclaredBlob"] = d
    if d:
        want = d.split()[0]
        assert blob.startswith(want), "%s: blob %s distinto del que fija A-5 o decisiones §72 (%s)" % (p, blob, d)
    else:
        r["AddedBeyondObjectCitations"] = True
    if p in APPEND_SINCE_PUBLICATION:
        old_blob = git("rev-parse", PUBLICATION + ":" + p).decode().strip()
        old = git("cat-file", "-p", old_blob)
        assert b.startswith(old) and len(b) >= len(old), "%s: el blob de la publicación no es prefijo exacto del commit" % p
        r["AppendOnlySincePublication"] = {"PublicationBlob": old_blob, "PublicationLines": len(lines_of(old.decode("utf-8"))),
                                           "Note": "el texto de bf2cf857 es prefijo exacto del texto del commit (solo añadido)"}
    return r, t, lines_of(t)


def question_ids(L):
    s8 = section(L, "8. ")
    rows = {m.group(1): i + 1 for i, m in ((i, re.match(r"^\| (Q-A5-\d\d) \|", L[i])) for i in range(s8["Start"] - 1, s8["End"])) if m}
    ids = sorted(rows)
    assert ids == ["Q-A5-%02d" % n for n in range(1, 13)], ids
    return ids, rows


def order_text_check(dec_lines):
    num, want, size = ORDER_TEXT
    s = section(dec_lines, "%s. " % num)
    text = "\n".join(dec_lines[s["Start"] - 1:s["End"]])
    return ("`%s`" % want) in text and ("%d %03d bytes" % (size // 1000, size % 1000)) in text


def identity_checks(texts):
    """Lo que el invocador comprueba de la identidad antes de lanzar (el revisor solo puede leerlo)."""
    for c in (PUBLICATION, GUARDS_RUN, REV):
        assert git("rev-parse", c + ":" + OBJECT).decode().strip() == OBJECT_BLOB, c
    assert subprocess.run(["git", "-C", CLONE, "cat-file", "-e", PREP_BASE + ":" + OBJECT], capture_output=True).returncode != 0, \
        "A-5 ya existía en la base de la publicación"
    assert git("rev-parse", PUBLICATION + "^").decode().strip() == PREP_BASE
    assert git("rev-parse", GUARDS_RUN + "^").decode().strip() == PUBLICATION
    assert git("merge-base", "--is-ancestor", GUARDS_RUN, REV) == b""
    for p in (GUARDS, SELFTEST, PREFLIGHT):
        assert git("rev-parse", PUBLICATION + ":" + p) == git("rev-parse", REV + ":" + p), p
    st = json.loads(texts[SELFTEST])
    tool_blob = git("rev-parse", REV + ":" + GUARDS).decode().strip()
    assert st["Result"] == "PASS" and st["A5TextBlob"] == OBJECT_BLOB and st["ToolBlob"] == tool_blob and \
        st["Summary"]["Killed"] == st["Summary"]["Mutants"], "a5-selftest.json inesperado"
    gr = json.loads(texts[GUARDS_RUN_RESULT])
    assert gr["Mode"] == "RUN" and gr["Base"] == PREP_BASE and gr["Head"] == PUBLICATION and gr["Result"] == "PASS", "a5-guards-result-run.json inesperado"
    for t, want in DECLARED_TREES.items():
        got = git("rev-parse", REV + ":" + t).decode().strip()
        assert got.startswith(want), (t, got)
    dec = texts[DECISIONS]
    assert "`docs/initiatives/I-62-A-5.md`, blob `%s`" % OBJECT_BLOB[:8] in dec, "decisiones §72 no nombra el blob del objeto"
    added_after = git("diff", "--name-status", PUBLICATION, GUARDS_RUN, "--", EVID).decode("utf-8").split("\n")
    return {"PublicationCommit": PUBLICATION, "PublicationParent": PREP_BASE, "GuardsRunCommit": GUARDS_RUN, "Decisions72Commit": REV,
            "ObjectBlobAt": {c[:8]: OBJECT_BLOB for c in (PUBLICATION, GUARDS_RUN, REV)}, "ObjectAbsentAtPublicationParent": True,
            "GuardsSelftestPreflightUnchangedSincePublication": True, "SelftestToolBlob": tool_blob, "SelftestA5TextBlob": OBJECT_BLOB,
            "SelftestRepoRecorded": st.get("Repo"),
            "GuardsRunResult": {"Mode": gr["Mode"], "Base": gr["Base"], "Head": gr["Head"], "Result": gr["Result"]},
            "Trees": {t: git("rev-parse", REV + ":" + t).decode().strip() for t in DECLARED_TREES},
            "EvidenceFilesAddedAfterPublication": [x for x in added_after if x],
            "Note": "A-5 se publicó en bf2cf857 (padre efcfcbc5, donde no existía); 1a0db7b0 custodia el resultado `run` de las guardas sobre bf2cf857 "
                    "y añade a5-guards-result-preview.json, a5-guards-result-run.json y out/candidate-commit.json; b631c46f añade decisiones §72. "
                    "El blob de A-5, a5-guards.py, a5-selftest.json y preflight.md no cambian después de bf2cf857"}


def run_identity():
    delta = git("diff", PREP_BASE, PUBLICATION, "--", OBJECT)
    dl = delta.decode("utf-8").split("\n")
    assert dl[0] == "diff --git a/%s b/%s" % (OBJECT, OBJECT) and dl[1] == "new file mode 100644" and \
        dl[2] == "index 00000000..%s" % OBJECT_BLOB[:8] and dl[3] == "--- /dev/null" and dl[4] == "+++ b/" + OBJECT, dl[:5]
    obj = git("show", REV + ":" + OBJECT).decode("utf-8").split("\n")
    body = [x[1:] for x in dl[6:] if x.startswith("+")]
    assert body == lines_of("\n".join(obj)), "delta.diff no reproduce el objeto línea a línea"
    return {"delta.diff": delta}


def main(print_only):
    global REV
    REV = git("rev-parse", "HEAD").decode().strip()
    assert REV.startswith(REV_SHORT), "HEAD del clon distinto de decisiones §72"
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    expected_run = run_identity()
    items = list(canonical) + [(p, "preparación de A-5: %s (evidencia de apoyo, no autoridad)" % p[len(EVID):], "ENTIRE",
                                ranges_json if p.endswith(".json") else (ranges_doc if p.endswith(".md") else ranges_whole))
                               for p in evidence_dir_files()]
    can, corpus, texts = [], set(), {}
    qids = qrows = dec_lines = None
    for p, why, scope, rf in items:
        r, t, L = rec(p)
        r["Why"], r["Scope"], r["LineRanges"] = why, scope, rf(L)
        for g in r["LineRanges"]:
            assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
        can.append(r)
        corpus |= {c for c in t if ord(c) > 127}
        texts[p] = t
        if p == OBJECT:
            qids, qrows = question_ids(L)
        if p == DECISIONS:
            dec_lines = L
    assert len({r["Path"] for r in can}) == len(can), "rutas repetidas en el cierre"
    ident = identity_checks(texts)
    ranges = {r["Path"]: [(g["Section"][:60], g["Start"], g["End"]) for g in r["LineRanges"]] for r in can}
    if print_only:
        print(json.dumps({"files": {r["Path"]: (r["Bytes"], r["Lines"], r["Blob"][:8], r["LinesOver2000Chars"]) for r in can},
                          "ranges": ranges, "questions": qids, "questionRows": qrows}, ensure_ascii=False, indent=1))
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
    num, want, size = ORDER_TEXT
    assert run["order.txt"]["Sha256"] == want and run["order.txt"]["Bytes"] == size and order_text_check(dec_lines), \
        "order.txt distinto del texto que custodia decisiones §72"
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §72 del Coordinator (2 556 bytes; SHA-256 0baac461…, el que declara decisiones §72 "
                            "en el commit); su punto 2 ordena esta revisión y fija sus ocho temas",
               "prompt.md": "copia de kit/prompt.md; stdin literal del proceso claude-cli; nunca es premisa",
               "delta.diff": "salida literal de `git diff %s %s -- %s` en el clon (archivo nuevo; línea index 00000000..%s); sus líneas «+» son el "
                             "objeto, línea a línea" % (PREP_BASE[:8], PUBLICATION[:8], OBJECT, OBJECT_BLOB[:8])}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    run_hashes["order.txt"]["IdentityCheck"] = {"DecisionsSection": "§" + num, "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": size}
    run_hashes["delta.diff"]["IdentityCheck"] = {"GitCommand": "git diff %s %s -- %s" % (PREP_BASE, PUBLICATION, OBJECT),
                                                 "IndexLine": "index 00000000..%s" % OBJECT_BLOB[:8], "AddedLinesEqualObject": True}
    blob_of = {r["Path"]: r["Blob"] for r in can}
    receipt = {"Commit": REV, "PublicationCommit": PUBLICATION, "PreparationBase": PREP_BASE, "GuardsRunCommit": GUARDS_RUN,
               "Object": {"Path": OBJECT, "Blob": OBJECT_BLOB},
               "Guards": {GUARDS: blob_of[GUARDS], SELFTEST: blob_of[SELFTEST], GUARDS_RUN_RESULT: blob_of[GUARDS_RUN_RESULT]},
               "Identity": ident}
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "ObjectIntroducedBy": PUBLICATION, "PreparationBase": PREP_BASE, "GuardsRunCommit": GUARDS_RUN,
        "MapBase": MAP_BASE,
        "ObjectPath": OBJECT, "ObjectBlob": OBJECT_BLOB, "PackagePath": None,
        "PackageNote": "A-5 no tiene paquete del Architect: A-5 §6 lo prevé solo «cuando una disposición autorice la revisión», y la disposición §72 "
                       "fija directamente el objeto y los ocho temas; el prompt hace de paquete",
        "PublicationReceipt": receipt,
        "PreviousObject": None, "PriorFindingIds": [],
        "PriorFindingSource": "ninguno: es la primera revisión formal de A-5 (A-5 cabecera y §10: NOT LAUNCHED)",
        "Harness": GUARDS, "HarnessExecution": "ninguna por el revisor: no tiene herramientas de ejecución; lee el código y los resultados custodiados",
        "FocusTopics": FOCUS_TOPICS,
        "FocusSource": "disposición §72, punto 2 (order.txt, líneas 10-19): conformidad de A5-1 con AP 16.13 (y 16.14, que fija el punto efectivo); "
                       "integridad del historial del fixture original; impacto sobre C-22 y C-25a; separación de los presupuestos de A4-6 y A5-2; "
                       "prohibición de reinicios y transferencias de cupos; aplicabilidad de D.5 y Owner Validation; decisiones reservadas al Owner; "
                       "suficiencia de las guardas y sus negativos",
        "ScopeConfirmationKeys": SCOPE_CONFIRMATION_KEYS,
        "OwnerAuthorizationLines": OWNER_LINES,
        "QuestionIds": qids, "QuestionRows": qrows, "QuestionSource": "A-5 §8 (Q-A5-01..Q-A5-12)",
        "SeparateVerdicts": {"A51Verdict": "A5-1 (separable)", "A52Verdict": "A5-2 (separable)",
                             "Values": "AGREED | CHANGES REQUIRED | EXCLUDE | BLOCKED — OWNER DECISION; NOT_ASSESSED solo con NOT_ACCREDITED"},
        "D5Disposition": {"Disposition": "DEFER_UNTIL_OV_PREPARATION | NO_D5_IMPACT | COVER_IN_A5 | OWNER_DECISION_REQUIRED; NOT_DETERMINED solo con "
                                         "NOT_ACCREDITED",
                          "OwnerValidationAffected": "YES | NO; NOT_DETERMINED solo con NOT_ACCREDITED",
                          "Source": "A-5 §3.4 («No cubre D.5») y Q-A5-01; disposición §72, puntos 2 y 6 («decisión sobre D.5»)"},
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
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a5\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S,
                      "AuthSettleSeconds": AUTH_SETTLE_S,
                      "AuthSettle": "launch.py espera AUTH_SETTLE_S segundos entre el final de `auth status` (preflight 5) y el lanzamiento, y registra en "
                                    "run.json como TRANSPORT_AUTH_REFRESH_CONFLICT un result con «Failed to refresh OAuth token» y 0 tokens (sin "
                                    "reintento); heredado del intento 2 de A-3 y de los kits de A-4",
                      "Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; aceptada como evidencia "
                                          "de elegibilidad vigente en decisiones §57, punto 3; vigencia del binario comprobada en decisiones §72): C1 "
                                          "completada, C2 cancelada por terminación, C3 PASS sobre la plantilla con --add-dir"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos tres archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json) "
                                "y el auditor v5.1-a5 lo vuelve a verificar después; el revisor contrasta lo que puede leer: delta.diff (línea index y "
                                "rutas), order.txt (punto 1), decisiones §72 (punto 1), a5-guards-result-run.json (Mode, Base, Head y Result) y "
                                "a5-selftest.json (ToolBlob y A5TextBlob)",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` señala las secciones que cita A-5 o pide la orden (Scope SECTIONS) o el archivo entero "
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
                        "order.txt y delta.diff (Path absoluto en " + RUN + " o el nombre suelto); nunca de prompt.md; una lectura fallida nunca "
                        "cuenta; una línea larga truncada por Read no cuenta como entregada; los números de línea se citan exactos (LineStart..LineEnd "
                        "deben contener la cita)",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
            "AbsentCommits": "los commits del Freeze (b64a3b64) y de V14 (4c617e82) son anteriores a un rebase y no están en el clon: V14 se "
                             "identifica por su blob en el commit (34ad80ea)",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md (lecturas iniciales, paso 4 «dotnet test»)", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "documentation-governance y el Context Pack de I-62", "Reason": "sin CLAUDE.md inyectado no hay obligación transitiva; el cierre lo fijan el objeto y la orden del Coordinator"},
            {"Source": "enlaces de A-5, del preflight y de la evidencia a documentos fuera del cierre (el resto de MAP-INVALID/ y de su constructor, "
                       "el análisis del Controller de s70, el paquete del candidato (b), los kits y sondas de FX-02, ADR-0046 y ADR-0048, WORKFLOW, "
                       "PROMPT_TEMPLATES, state/I-62.yml, el fixture y sus clones)",
             "Reason": "citas descriptivas; el cierre lo fijan el objeto, lo que cita con blob o línea y la orden del Coordinator"},
            {"Source": "a5-guards.py self-test, preview y run; build_candidate_a5.py, neg_controls_a5.py, a5_conformance.py y preflight_run.py",
             "Reason": "sin herramientas de ejecución; los resultados custodiados son a5-selftest.json, a5-guards-result-run.json y out/"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: correo del usuario (identidad) y estado de Git del cwd (HEAD separado del clon, sin rama, y asuntos de los commits recientes)",
            "adjunto environment: cwd D:\\r62-arch-a5, plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para D:\\r62-arch-a5 (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión autora, del Coordinator y de cualquier otra sesión, incluidas las revisiones de A-4)",
            "otras sesiones, incluidas las revisiones de A-1..A-4 (solo el registro y la salida de la segunda re-revisión de A-4 están en el cierre, como formato)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*)",
            "los archivos de " + RUN + " distintos de los tres del run, incluido el subdirectorio launch/",
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
                      "questions": len(qids), "large": closure["ReadPolicy"]["LargeFiles"],
                      "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    main("--ranges" in sys.argv[1:])
