"""I-62, decisiones §80, punto 6: EffectiveInputClosure de la ÚNICA REVISIÓN FORMAL de la RONDA 2 de la enmienda A-6 (docs/initiatives/I-62-A-6.md,
blob de la ronda 2 de kit-params.json, PROPUESTA) por un ARCHITECT independiente, en una celda `claude-cli` medida y elegible (autorización del
Owner CLAUDE-CLI-I62 = A: revisiones de ARCHITECT de I-62, solo lectura). La revisión solo se lanza con la compuerta 11 de launch.py en Ok, que
exige una disposición posterior del Coordinator con el blob y el RunId (owner_gate.py).

Adaptado del make_closure.py del kit de la ronda 1 (R20261011T215735Z-c434): la misma estructura de cierre, rangos, run y transporte; el objeto es
la A-6 de la ronda 2 en el commit del clon (CloneCommit de kit-params.json: la publicación de la ronda 2 o un commit posterior que custodie el
resultado `run` de sus guardas). El run tiene TRES archivos: order.txt (= cuerpo §80, 2 833 bytes), prompt.md (renderizado desde
prompt.template.md) y a6-r1.md (el texto de la ronda 1, blob eac473c0, para comparar R1-R5).

Cierre canónico, en el commit del clon, cada archivo fijado por su blob (los directorios por su árbol):
  * A-6 de la ronda 2, entera (objeto);
  * docs/automation/evidence/I-62-A6/r2/ entero (guardas, autoprueba, resultados, preflight, reproducción r1r4/, independencia, repro/);
  * el paquete B-min entero (para el contraste del texto normativo con accept/bmin_accept.py y para la independencia);
  * el dictamen de materialidad (output.json y README.md de R20261011T162328Z-5c5e) y la salida de la revisión de la ronda 1
    (output.json de R20261011T215735Z-c434: hallazgos R1-R5 y O1-O3 literales);
  * la base normativa del kit de la ronda 1 (V14, AUTOMATION_PLAN, ADR-0046, ADR-0048, LIFECYCLE, A-1..A-5, agent-execution README, routing.md,
    adapters/codex-cli.md, cinco esquemas, kits/FX-02/controller-contracts.md, decisiones), con los rangos de orientación de la ronda 1 y los
    nuevos (decisiones §43, §57, §61, §64 y §73-§80; A-6: §3.16, G1-G6, reglas de A6-1, A6-3, A6-4 y A6-5);
  * toda fuente que liste G11 en el resultado `run` custodiado de las guardas de la ronda 2 (Sources y GuardSources), con sus rangos como
    orientación, INCLUIDA la evidencia (I-62-evidence.md): decisión de la sesión principal en la fase 2 (decisiones §79, punto 5: acceso directo a
    toda fuente que el texto cite), desviación deliberada frente a la ronda 1 con el riesgo de anclaje declarado (contiene los resúmenes y
    valoraciones de la sesión que preparó A-6 y este kit); el prompt la presenta como evidencia, no como autoridad;
  * r2/readable/ (copias legibles, líneas de 1 900 caracteres como máximo, de los JSON de una sola línea de r2/), con su MANIFEST comprobado
    (blob canónico y SHA-256 de cada copia en el commit del clon); docs/automation/evidence/I-62-C1C3/ queda FUERA (decisiones §81, ajeno).
Un archivo del cierre que no sea texto UTF-8 se informa (todos, con su ruta) y detiene el script: nunca se omite en silencio.
Los rangos se calculan aquí, mecánicamente, desde los encabezados, las filas y las líneas citadas (con su primera línea comprobada); el script
falla si algo no coincide.

Uso:
  python -B make_closure.py --render-prompt    → renderiza kit/prompt.md desde kit/prompt.template.md (clon de kit-params.json)
  python -B make_closure.py                    → escribe kit/closure.json (exige que kit/prompt.md sea el renderizado y que el run tenga
                                                  exactamente order.txt, prompt.md y a6-r1.md, iguales a las copias del kit)
  (prep/make_synthetic_env.py lo importa y llama a build_kit() con un clon, un run y un directorio de salida sintéticos: autoprueba de la fase 1)
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
_spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(KIT, "owner_gate.py"))
OG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(OG)

CLONE_FIXED, RUN_FIXED = r"D:\r62-arch-a6r2", r"D:\r62-arch-a6r2-run"
IDS = {"RunId": "R20261012T195511Z-e5c3", "InvocationId": "I20261012T195511Z-e5c3", "LogicalReviewRequestId": "L20261012T195511Z-e5c3",
       "AttemptSeq": 1}
SESSION_ID = "d95fcaba-f7db-4408-af85-497e565d114e"          # --session-id fijado para el único lanzamiento (uuid4, fase 1)
OBJECT = "docs/initiatives/I-62-A-6.md"
A6R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"        # ronda 1 (decisiones §77, punto 1): en el padre de la publicación y en a6-r1.md
R2_DIR = "docs/automation/evidence/I-62-A6/r2"
R2_GUARDS, R2_SELFTEST, R2_RUN_RESULT = R2_DIR + "/a6r2-guards.py", R2_DIR + "/a6r2-selftest.json", R2_DIR + "/a6r2-guards-result-run.json"
PKG_DIR = "docs/automation/evidence/I-62-F6/FX-02/bmin"
ACCEPTOR, TOOL = PKG_DIR + "/accept/bmin_accept.py", PKG_DIR + "/tool/bmin_facts.py"
RULING_DIR = "docs/automation/evidence/I-62-architect-bmin/R20261011T162328Z-5c5e"
RULING_OUT, RULING_README = RULING_DIR + "/output.json", RULING_DIR + "/README.md"
R1_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6"
R1_REVIEW_OUT = R1_REVIEW_DIR + "/R20261011T215735Z-c434/output.json"
AP = "docs/AUTOMATION_PLAN.md"
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A1, A2, A3, A4, A5 = ("docs/initiatives/I-62-A-%d.md" % i for i in (1, 2, 3, 4, 5))
AE = "docs/automation/agent-execution/"
RAE, ROUTING, CODEX_CLI = AE + "README.md", AE + "routing.md", AE + "adapters/codex-cli.md"
SCH = AE + "schemas/"
S_IC, S_RI, S_CV2, S_DEL2, S_WH = (SCH + n for n in ("input-closure.v1.schema.json", "role-invocation.v1.schema.json",
                                                    "controller-verification.v2.schema.json", "delegation.v2.schema.json",
                                                    "worker-handoff.schema.json"))
CC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
DECISIONS = "docs/automation/decisions/I-62.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"          # DENTRO del cierre en la ronda 2 (decisiones §79, punto 5; fase 2); anclaje declarado
C1C3_DIR = "docs/automation/evidence/I-62-C1C3"                 # FUERA del cierre (decisiones §81: C1-C3, ajeno a esta revisión)
READABLE_DIR = R2_DIR + "/readable"                             # copias legibles (líneas <= 1 900) de los JSON de una sola línea de r2/
CHAR_RECORD = "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json"   # lo fija la compuerta; fuera del cierre
PINNED = {   # blob fijado (sin cambio desde la ronda 1, o custodiado después de ella); el commit del clon debe conservarlo
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534",
    ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e", LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f", A5: "ad94ceedacbf3dbf70f0217a583978d0e32592fc",
    RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055", ROUTING: "bba08fc4686d7192deeb559752636a467103b4b1",
    CODEX_CLI: "155f3469e345e2397fa26347a9cb52d6fdb91122", S_RI: "d54a7ae783ec83d055babdae090899908b616082",
    S_IC: "6bdf71843e2c70362a5e2d0ea19c8891e22bcff6", S_CV2: "e1faa9eca5c50b2b3a9f91575575614366962720",
    S_DEL2: "dfdbe461270e8fd92200c36bab78b9fdc3385014", S_WH: "cc3c9cf76b2247f3312e99bd447554326cc3700a",
    CC: "90929ad1c49c028f41f58f654f8713bd0b35f27b",
    RULING_OUT: "96c3efbc81e9c6814b232774fd8ac7e900deda7d", RULING_README: "8ce105e0c2c6efd18eb720799492fdca0b4f7315",
    R1_REVIEW_OUT: "e90d685bb829fc7d30eda9bdb05deb3ac66d24f5",
    ACCEPTOR: "434ef1fccb0d15762b30244b4a60928120698411", TOOL: "9a829b0eeb1210238052b1feac06cfa8f7964aed",
    CHAR_RECORD: "541956eb8f5d154414b91338c655919ae0ade697",
}
PINNED_TREES = {PKG_DIR: "3caa0dfa73d372af9251cf5c5c194261de8c7685", "docs/automation/agent-execution": "809fd58f450d3379a4fcc958cf8907fcdc5acff3",
                R1_REVIEW_DIR: "520e7a1f67e8280e8432a7666624fcf548c51a66"}
DECISIONS_PREFIX_BLOB = "d094823ab3d25f558c5c85fbd4155589ea5d3f5c"   # decisiones en 6a681cec (§80 registrada): prefijo exacto de las del clon
RUN_FILES = ["order.txt", "prompt.md", "a6-r1.md"]
PREMISE_RUN_FILES = ["order.txt", "a6-r1.md"]                # prompt.md nunca
ORDER_TEXT = ("80", "1342b5be1a5de104ac08a1115ddc999d2e6dfbe80100affaa16ea44439fa7f99", 2833)
TOPICS = ["Q-A6-01", "Q-A6-02", "Q-A6-06/07", "Q-A6-12"]
REQUIRED_TOPICS = ["Q-A6-02", "Q-A6-06/07", "Q-A6-12"]      # «REQUIRED REVIEW TOPIC» en decisiones §77, punto 2
DELTAS = ["A6-%d" % i for i in range(1, 10)]
MATERIALITY_IDS = ["M-0%d" % i for i in range(1, 9)]
MATERIALITY_A6_EXPECTED = {"M-01": "no", "M-02": "yes", "M-03": "yes", "M-04": "yes", "M-05": "yes", "M-06": "no", "M-07": "no", "M-08": "yes"}
ROUND1_FINDING_IDS = ["A62-A6-R%d" % i for i in range(1, 6)]
ROUND1_OPTIONAL_IDS = ["A62-A6-O%d" % i for i in range(1, 4)]
GAP_IDS = ["G%d" % i for i in range(1, 7)]
ACCEPTOR_CLAIMS = ["CHECK_IDS", "STATUS_VALUES", "WAIVABLE", "DECISION_FORMULA", "EXIT_CODES", "R8_MANUAL", "R5_DATA_CONDITION",
                   "R6_DATA_CONDITION", "R7_DATA_CONDITION", "R4_INPUTS", "R10_RELAY_FACTS", "REQUEST_REPO", "REQUEST_TOOL_FROM_OBJECT",
                   "REQUEST_INTERPRETER", "REQUEST_RUNS", "REQUEST_CLOSURE", "REQUEST_NOT_APPLICABLE"]
PROVENANCE_ITEMS = ["PREPARATION_WRITE", "TIMING_AT_USE", "POU_I_NO_REPLACE_REFS", "POU_II_NO_GRAFTS_ALTERNATES_SHALLOW",
                    "POU_III_TYPE_AND_BYTES_REHASH", "POU_IV_FSCK", "POU_V_INTERPRETER_AND_GIT", "POU_VI_NO_GIT_ENV",
                    "POU_VII_CONFIG_NO_INCLUDE", "POU_VIII_NO_SHADOW_EXECUTABLES", "DECLARED_ORDER", "FIDELITY_BY_BYTES", "FAIL_CLOSED"]
DIRECT_READ = {"Path": V14, "Start": 2333, "End": 2334, "Token": "Esperado independiente"}
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
TIMEOUT_S = 3600               # igual que en los kits de A-4, A-5, B-min y la ronda 1 de A-6
AUTH_SETTLE_S = 120            # mitigación del intento 1 de A-3: espera entre `auth status` (preflight 5) y el lanzamiento
LARGE = 60000
CLONE, RUN, REPO, REV, OUT = CLONE_FIXED, RUN_FIXED, CLONE_FIXED, None, KIT
PARAMS = {}


def git(*a):
    return subprocess.check_output(["git", "-C", REPO] + list(a))


def git_ok(*a):
    return subprocess.run(["git", "-C", REPO] + list(a), capture_output=True).returncode == 0


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


# ------------------------------------------------------------------ rangos mecánicos (los del kit de la ronda 1)
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
    return {"Section": label, "Start": first["Start"], "End": last["End"], "Span": True}


def row(lines, rng, pred, label):
    hits = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if pred(lines[i])]
    assert len(hits) == 1, "fila %r: %d coincidencias en %s" % (label, len(hits), rng)
    return {"Section": label, "Start": hits[0], "End": hits[0]}


def cited(lines, start, end, token, label):
    """Rango citado por número de línea: su primera línea debe contener `token`."""
    assert 1 <= start <= end <= len(lines) and token in lines[start - 1], "L%d-L%d: la línea %d no contiene %r" % (start, end, start, token)
    return {"Section": label, "Start": start, "End": end}


def table_at(lines, rng, pred, label):
    first = row(lines, rng, pred, label)["Start"]
    end = first
    while end < len(lines) and lines[end].startswith("|"):
        end += 1
    return {"Section": label, "Start": first, "End": end}


def whole(lines):
    return [{"Section": "entero", "Start": 1, "End": len(lines)}]


def header_block(L, label="cabecera"):
    hs = headings(L)
    end = (hs[1][0] - 1) if len(hs) > 1 else len(L)
    while end > 1 and L[end - 1].strip() == "":
        end -= 1
    return {"Section": label, "Start": 1, "End": end}


def all_sections(L, level_max=3):
    return [section(L, h[2]) for h in headings(L)[1:] if h[1] <= level_max]


def numbered_items(L, rng, label):
    starts = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if re.match(r"^\d+\. \*\*", L[i])]
    out = []
    for k, s in enumerate(starts):
        e = (starts[k + 1] - 1) if k + 1 < len(starts) else rng["End"]
        while e > s and L[e - 1].strip() == "":
            e -= 1
        n = int(re.match(r"^(\d+)\. ", L[s - 1]).group(1))
        assert n == k + 1, (label, starts)
        out.append({"Section": "%s, punto %d" % (label, n), "Start": s, "End": e})
    return out


def block_items(L, rng, rx, cont, label_fn):
    """Bloques dentro de `rng` que empiezan en una línea que casa con `rx` y siguen mientras la línea siguiente cumple `cont` (sin empezar otro)."""
    starts = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if re.match(rx, L[i])]
    out = []
    for s in starts:
        e = s
        while e < rng["End"] and cont(L[e]) and not re.match(rx, L[e]):
            e += 1
        out.append({"Section": label_fn(re.match(rx, L[s - 1])), "Start": s, "End": e})
    return out


def g11_ranges(path):
    src = [x for x in G11.get("Sources", []) + G11.get("GuardSources", []) if x.get("Path") == path]
    out = []
    for x in src:
        for a, b in x.get("Ranges", []):
            out.append({"Section": "G11, L%d-L%d" % (a, b) if a != b else "G11, L%d" % a, "Start": a, "End": b, "G11": True})
    seen, uniq = set(), []
    for g in out:
        if (g["Start"], g["End"]) not in seen:
            seen.add((g["Start"], g["End"]))
            uniq.append(g)
    return uniq


def ranges_ap(L):
    keys = ("16.4", "16.9", "16.10", "16.11", "16.13", "16.14", "16.18", "16.19", "16.20", "16.21", "16.22", "16.23", "16.24")
    s = {k: section(L, k + " ") for k in keys}
    s24 = s["16.24"]
    return [span(s["16.13"], s["16.14"], "16.13-16.14"), span(s["16.18"], s["16.24"], "16.18-16.24")] + [s[k] for k in s] + [
        row(L, s["16.4"], lambda x: x.startswith("**Transporte:**"), "16.4, transporte"),
        cited(L, 541, 543, "**Receta de Codex:**", "16.4, receta de Codex (L541-L543)"),
        table_at(L, s["16.9"], lambda x: x.startswith("| # | Id | Comprueba |"), "16.9, tabla de las catorce comprobaciones"),
        cited(L, 623, 624, "fila, salvo el causado", "16.9, primer fallo y precedencia (L623-L624)"),
        cited(L, 646, 646, "Nunca bastan stdout", "16.9, nunca bastan stdout ni un JSON conforme (L646)"),
        cited(L, 653, 653, "(`INVALID_OUTPUT`)", "16.9, salida conforme pero vacía o falsa: INVALID_OUTPUT (L653)"),
        cited(L, 676, 676, "**Recuperación:**", "16.11, recuperación (L676)"),
        cited(L, 731, 731, "| Mapa de cláusulas |", "16.13, mapa en I62_EFFECTIVE_SHA (L731)"),
        cited(L, 902, 908, "| MV-1 |", "16.13, Validate(M): MV-1..MV-7 (L902-L908)"),
        cited(L, 910, 918, "| Defecto | Efecto |", "16.13, tabla de defectos de Validate(M) (L910-L918)"),
        cited(L, 945, 946, "**Punto efectivo:**", "16.14, punto efectivo (L945-L946)"),
        cited(L, 1045, 1046, "**Contrato del adapter.**", "16.19, contrato del adapter (L1045-L1046)"),
        cited(L, 1048, 1052, "**Descriptor.**", "16.19, descriptor y Facts (L1048-L1052)"),
        cited(L, 1093, 1093, "**Sin celda elegible no se invoca** (P-10)", "16.20, sin celda elegible no se invoca (L1093)"),
        cited(L, 1204, 1207, "**Comparación no establecida.**", "16.22, comparación no establecida (L1204-L1207)"),
        cited(L, 1221, 1221, "- permisos mínimos:", "16.23, permisos mínimos (L1221)"),
        cited(L, 1240, 1240, "PLAN y VERIFY no son intercambiables.", "16.23, PLAN y VERIFY (L1240)"),
        cited(L, 1264, 1264, "| `CanonicalInputs[]` |", "16.24, clase CanonicalInputs (L1264)"),
        cited(L, 1265, 1267, "| `AllowedTransitiveInputs[]` |", "16.24, clases AllowedTransitiveInputs, DeclaredRuntimeContext y ForbiddenInputs (L1265-L1267)"),
        cited(L, 1271, 1276, "2. cada obligación se clasifica", "16.24, cálculo del cierre, pasos 2-4 (L1271-L1276)"),
        cited(L, 1279, 1280, "ninguna prueba local", "16.24, auditoría de lecturas: P-22 y Contexto (L1279-L1280)"),
        row(L, s24, lambda x: x.startswith("**Identidad del revisor.**"), "16.24, identidad del revisor"),
        cited(L, 1287, 1288, "**Fidelidad de los insumos**", "16.24, fidelidad de los insumos (L1287-L1288)"),
        table_at(L, s24, lambda x: x.startswith("| P-22 |"), "16.24, P-22..P-25")] + g11_ranges(AP)


def ranges_v14(L):
    s18 = section(L, "18. ")
    s20, s203, s2031 = section(L, "20.2 "), section(L, "20.3 "), section(L, "20.3.1 ")
    intro_end = s2031["Start"] - 1
    while L[intro_end - 1].strip() == "":
        intro_end -= 1
    s203 = {"Section": "§20.3, introducción (hasta §20.3.1)", "Start": s203["Start"], "End": intro_end}
    annex_c = section(L, "Anexo C ")
    d = {k: section(L, k + " ") for k in ("D.1", "D.3", "D.4", "D.5", "D.6")}
    return [s18, cited(L, 960, 960, "| OV-I62-03 topología A |", "§18, OV-I62-03 (L960)"),
            cited(L, 974, 974, "| OD-5 |", "§18, OD-5 (L974)"),
            cited(L, 1039, 1047, "**Invariante de autoridad (AUT-I1).**", "§20.2, invariante de autoridad AUT-I1 (L1039-L1047)"), s20, s203, s2031,
            cited(L, 1079, 1079, "| `CanonicalInputs[]` |", "§20.3.1, clase CanonicalInputs (L1079)"),
            cited(L, 1088, 1099, "2. En cada archivo se identifican", "§20.3.1, pasos 2-4 (L1088-L1099)"),
            cited(L, 1126, 1130, "**Verificación.**", "§20.3.1, verificación (L1126-L1130)"),
            annex_c, cited(L, DIRECT_READ["Start"], DIRECT_READ["End"], DIRECT_READ["Token"],
                           "Anexo C, esperado independiente (L2333-L2334): LECTURA DIRECTA OBLIGATORIA"),
            cited(L, 2363, 2363, "| C-24 |", "Anexo C, C-24 (L2363)")] + [d[k] for k in d] + [
            cited(L, 2433, 2447, "**Topología A**", "D.3, Topología A: tabla de roles, adapters y topes (L2433-L2447)"),
            cited(L, 2523, 2523, "| A | ≤ 15 + 2 sondas |", "D.3, totales de la ronda A (L2523)"),
            cited(L, 2534, 2537, "| **PASS** |", "D.4, resultados PASS/FAIL/UNVERIFIED/UNSUPPORTED (L2534-L2537)"),
            cited(L, 2544, 2544, "| FX-02 PASS |", "D.4, matriz de cierre: FX-02 PASS (L2544)"),
            cited(L, 2558, 2560, "Se resiembra el fixture", "D.5, resiembra y FX-02 (L2558-L2560)"),
            cited(L, 2572, 2573, "| **Dependencias técnicas permitidas** |", "D.6, dependencias técnicas permitidas y privadas prohibidas (L2572-L2573)"),
            cited(L, 2586, 2587, "El oráculo, fuera del host", "D.6, último párrafo, ancla de A6-3 (L2586-L2587)")] + g11_ranges(V14)


def ranges_lifecycle(L):
    s3 = section(L, "3. ")
    return [s3] + [row(L, s3, (lambda m: lambda x: x.startswith("| **%s** |" % m))(m), "§3, %s" % m) for m in MATERIALITY_IDS] + [
        dict(row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "§3, UNKNOWN cuenta como activado"),
             End=row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "")["End"] + 1),
        row(L, s3, lambda x: x.startswith("Un cambio de alcance, no-objetivos, decision Owner"), "§3, OWNER-RESERVED"),
        section(L, "5. "), row(L, section(L, "5. "), lambda x: x.startswith("Un hallazgo es `REQUIRED`"), "§5, REQUIRED y OPTIONAL"),
        row(L, section(L, "5. "), lambda x: x.startswith("Solo quien emitio un REQUIRED"), "§5, veredictos"),
        section(L, "6. ")] + g11_ranges(LIFECYCLE)


def ranges_adr(path):
    def f(L):
        dec = section(L, "Decisión")
        return whole(L) + [header_block(L)] + all_sections(L) + numbered_items(L, dec, "Decisión") + g11_ranges(path)
    return f


def ranges_rae(L):
    keep = ("5. ", "8. ", "10. ", "14. ", "14.", "15. ", "15.")
    return whole(L) + [section(L, h[2]) for h in headings(L) if h[1] in (2, 3) and h[2].startswith(keep)] + [
        cited(L, 184, 184, "(INVALID_OUTPUT)", "§8, salida inválida (INVALID_OUTPUT) (L184)"),
        cited(L, 561, 561, "| I2 |", "§15.2, regla I2 (L561)"), cited(L, 565, 568, "| I6 |", "§15.2, regla I6 y auditoría de lecturas (L565-L568)")] + \
        g11_ranges(RAE)


def ranges_a4(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 169, 169, "- **Hueco.** D.3 supone que la receta basta", "§2, hueco (L169)"),
        cited(L, 192, 192, "> 1. la invocación del rol puede declarar", "§3.2, A4-1, regla 1 (L192)"),
        cited(L, 203, 207, "regla de nueva medición de A-2. Su tope es uno", "§3.2, A4-1, reglas 3-4 (L203-L207)"),
        cited(L, 393, 393, "| M-03 | **sí** |", "§5, M-03 (L393)")] + g11_ranges(A4)


def ranges_a5(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 175, 199, "> «**Construcción del fixture conforme al mapa de cláusulas (A-5).**", "§3.2, párrafo A5-1 (L175-L199)"),
        cited(L, 216, 216, "(**11** base, pool **4**", "§3.3, A5-2, regla 4 (L216)"),
        cited(L, 221, 221, "extiende a esta fila.»", "§3.3, fin de A5-2 y de D.3 con A-1..A-5 (L221)")] + g11_ranges(A5)


def ranges_header_only(L):
    return whole(L) + [header_block(L)]


def ranges_a6(L):
    s = {h[2].split(" ")[0]: section(L, h[2]) for h in headings(L)[1:] if h[1] <= 3}
    s5, s7, s8, s312, s316 = s["5."], s["7."], s["8."], s["3.12"], s["3.16"]
    deltas = {"A6-%d" % n: s["3.%d" % (n + 3)] for n in range(1, 10)}
    rules = []
    for d, rng in deltas.items():
        rs = block_items(L, rng, r"^> (\d+)\. ", lambda x: x.startswith(">"), (lambda d: lambda m: "%s, regla %s" % (d, m.group(1)))(d))
        assert [r["Section"] for r in rs] == ["%s, regla %d" % (d, k + 1) for k in range(len(rs))] and rs, "A-6 %s: reglas no numeradas 1..n" % d
        rules += rs
    gaps = block_items(L, s312, r"^- \*\*(G[1-6])\. ", lambda x: x.startswith("  "), lambda m: "§3.12, %s" % m.group(1))
    assert [g["Section"] for g in gaps] == ["§3.12, %s" % g for g in GAP_IDS], "A-6: G1..G6 no están, en orden, en §3.12"
    line_a = row(L, s7, lambda x: x.startswith("A6-ADR0046-9-FX02 = A ("), "§7, línea del Owner (a) A6-ADR0046-9-FX02")
    assert OG.line_sha(L[line_a["Start"] - 1]) == OG.A6_LINE_SHA, "A-6 §7: la línea (a) no es la fijada (SHA-256 83152eae…)"
    return whole(L) + [header_block(L)] + all_sections(L) + [
        table_at(L, s["3.2"], lambda x: x.startswith("| Punto | Alcance mínimo del dictamen |"), "§3.2, tabla de los ocho puntos"),
        table_at(L, s["3.3"], lambda x: x.startswith("| Pieza | Ruta de procedencia |"), "§3.3, tabla de identidad por blob"),
        table_at(L, s5, lambda x: x.startswith("| M | Activado | Dictamen | Ronda 1 | Motivo |"), "§5, tabla M-01..M-08"),
        table_at(L, s316, lambda x: x.startswith("| Hallazgo de la ronda 1 | Corrección |"), "§3.16, tabla de las correcciones de la ronda 2")] + [
        row(L, s5, (lambda m: lambda x: x.startswith("| %s |" % m))(m), "§5, %s" % m) for m in MATERIALITY_IDS] + [
        row(L, s316, (lambda r: lambda x: x.startswith("| %s:" % r))(r), "§3.16, %s" % r) for r in ("R1", "R2", "R3", "R4", "R5", "O1", "O2", "O3")] + [
        line_a,
        row(L, s7, lambda x: x.startswith("A6-CONTROLLER-MEDICION-CONSUMO = A ("), "§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO"),
        row(L, s7, lambda x: x.startswith("- **(a) Determinación sobre ADR-0046 #9"), "§7, consecuencia (a)"),
        row(L, s7, lambda x: x.startswith("- **(b) Autorización de consumo"), "§7, consecuencia (b)")] + [
        row(L, s8, (lambda q: lambda x: x.startswith("| %s |" % q))(q), "§8, %s" % q)
        for q in ("Q-A6-01", "Q-A6-02", "Q-A6-06, Q-A6-07", "Q-A6-12", "Q-A6R2-01", "Q-A6R2-02", "Q-A6R2-03", "Q-A6R2-04", "Q-A6R2-05", "Q-A6R2-06")] + \
        gaps + rules


def ranges_grr(L):
    return whole(L) + [cited(L, 2, 4, '"Mode": "RUN"', "Mode, Base y Head (L2-L4)"),
                       row(L, {"Start": 1, "End": len(L)}, lambda x: x in (' "Result": "PASS"', ' "Result": "PASS",'), "Result de conjunto")]


def ranges_selftest(L):
    a = row(L, {"Start": 1, "End": len(L)}, lambda x: x == ' "Summary": {', "")["Start"]
    b = row(L, {"Start": 1, "End": len(L)}, lambda x: x.startswith(' "A6TextBlob": '), "")["Start"]
    return whole(L) + [{"Section": "Summary, Result, Mode, Repo, ToolBlob y A6TextBlob (L%d-L%d)" % (a, b), "Start": a, "End": b}]


def ranges_r1out(L):
    w = {"Start": 1, "End": len(L)}
    head = {"Section": "RunId, ids, objeto revisado y Verdict (cabecera)", "Start": 2,
            "End": row(L, w, lambda x: x.startswith(' "Verdict": '), "")["Start"]}
    out = whole(L) + [head]
    for fid in ROUND1_FINDING_IDS + ROUND1_OPTIONAL_IDS:
        n = row(L, w, (lambda f: lambda x: x == '   "FindingId": "%s",' % f)(fid), fid)["Start"]
        e = n
        while e < len(L) and not L[e].startswith("  }"):
            e += 1
        out.append({"Section": "hallazgo %s" % fid, "Start": n - 1, "End": e + 1})
    out.append(row(L, w, lambda x: x.startswith(' "KnownLimitations": ['), "KnownLimitations"))
    return out


def ranges_decisions(L):
    present = sorted(int(m.group(1)) for m in (re.match(r"^## (\d+)\. ", x) for x in L) if m)
    s = {n: section(L, "%d. " % n) for n in present if n in (43, 57, 61, 64) or n >= 73}
    s79, s80 = s[79], s[80]
    rows79 = [row(L, s79, (lambda k: lambda x: x.startswith("| **%d. " % k))(k), "§79, punto %d" % k) for k in range(1, 8)]
    rows80 = [row(L, s80, (lambda k: lambda x: x.startswith("| **%d. " % k))(k), "§80, punto %d" % k) for k in range(1, 9)]
    src79 = row(L, s79, lambda x: x.startswith("cuerpo pegado, extraído de la transcripción de la sesión, suma "), "§79, bytes y SHA-256 del cuerpo pegado")
    src80 = row(L, s80, lambda x: x.startswith("cuerpo pegado, extraído de la transcripción de la sesión, suma "), "§80, bytes y SHA-256 del cuerpo pegado")
    owner78 = row(L, s[78], lambda x: OG.line_sha(x) == OG.A6_LINE_SHA, "§78, línea del Owner A6-ADR0046-9-FX02")
    last = max(n for n in s)
    return [span(s[73], s[last], "§73-§%d" % last), span(s[79], s[80], "§79-§80")] + [s[n] for n in sorted(s)] + [src79] + rows79 + \
        [src80] + rows80 + [owner78] + g11_ranges(DECISIONS)


def ranges_evidence(L):
    secs = [section(L, h[2]) for h in headings(L) if h[1] == 2 and re.match(r"^(\d+)\. ", h[2]) and int(h[2].split(".")[0]) >= 140]
    return whole(L) + secs + g11_ranges(EVIDENCE)


def ranges_g11_only(path):
    def f(L):
        return whole(L) + [header_block(L)] + all_sections(L) + g11_ranges(path) if path.endswith(".md") else whole(L) + g11_ranges(path)
    return f


def ranges_doc(L):
    return whole(L) + [header_block(L)] + all_sections(L)


def ranges_whole(L):
    return whole(L)


# (ruta, por qué, alcance, función de rangos, etiqueta corta para el prompt)
canonical = [
    (V14, "Proposal V14 congelada: §18 (matriz OV y OD, orientación), §20.2 (AUT-I1), §20.3 y §20.3.1 (cierre efectivo de insumos), Anexo C "
          "(L2333-L2334, LECTURA DIRECTA OBLIGATORIA; C-24, L2363), D.1, D.3 (tabla de la Topología A y totales), D.4 (resultados y matriz de "
          "cierre), D.5 y D.6; rangos citados por A-6 (G11)", "SECTIONS", ranges_v14, "§18, §20.2, §20.3, §20.3.1, Anexo C, D.1, D.3, D.4, D.5 y D.6"),
    (AP, "AUTOMATION_PLAN: 16.4 (receta), 16.9 (comprobaciones y modos de fallo; INVALID_OUTPUT L653), 16.10, 16.11 (recuperación), 16.13-16.14 "
         "(compatibilidad, Validate(M)) y 16.18-16.24 (adapters, binding, independencia, aceptación, invocación de rol, cierre de insumos y "
         "fidelidad); rangos citados por A-6 (G11)", "SECTIONS", ranges_ap, "16.4, 16.9, 16.10, 16.11, 16.13-16.14 y 16.18-16.24"),
    (ADR46, "ADR-0046 aceptado (protocolo de ejecución delegada; #6 verificación fail-closed; #9 nivel A)", "ENTIRE", ranges_adr(ADR46), "entero"),
    (ADR48, "ADR-0048 (roles, binding por capacidad, independencia por dimensiones; conserva #9, Level A)", "ENTIRE", ranges_adr(ADR48), "entero"),
    (LIFECYCLE, "LIFECYCLE §3 (M-01..M-08; OWNER-RESERVED), §5 (REQUIRED/OPTIONAL y veredictos, orientación) y §6 (enmiendas)", "SECTIONS",
     ranges_lifecycle, "§3, §5 y §6"),
    (A1, "A-1 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A2, "A-2 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A3, "A-3 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A4, "A-4 acordada (A4-1: precedente de A6-1 y A6-7; A4-6)", "ENTIRE", ranges_a4, "entera"),
    (A5, "A-5 acordada (A5-1; A5-2, fin de D.3 con A-1..A-5)", "ENTIRE", ranges_a5, "entera"),
    (RAE, "procedimiento de agent-execution: §5 (invocación del Controller), §8 (coherencia de la verificación; INVALID_OUTPUT), §10 (controles "
          "negativos), §14 (binding, independencia) y §15 (invocación de rol, cierre e I2/I6, fidelidad)", "SECTIONS", ranges_rae, "§5, §8, §10, §14 y §15"),
    (ROUTING, "enrutamiento estable (clases, elegibilidad de una celda, el propio Controller)", "ENTIRE", ranges_doc, "entero"),
    (CODEX_CLI, "descriptor del adapter codex-cli de la celda del Controller", "ENTIRE", ranges_doc, "entero"),
    (S_IC, "esquema rackcad-input-closure/v1 (contrato consumido)", "ENTIRE", ranges_whole, "entero"),
    (S_RI, "esquema rackcad-role-invocation/v1 (contrato consumido)", "ENTIRE", ranges_whole, "entero"),
    (S_CV2, "esquema rackcad-controller-verification/v2 (contrato consumido)", "ENTIRE", ranges_whole, "entero"),
    (S_DEL2, "esquema rackcad-delegation/v2 (contrato consumido)", "ENTIRE", ranges_whole, "entero"),
    (S_WH, "esquema rackcad-worker-handoff/v1 (contrato consumido)", "ENTIRE", ranges_whole, "entero"),
    (CC, "contratos de las invocaciones del Controller de FX-02 (codex-cli, solo lectura); sin valores esperados", "ENTIRE", ranges_g11_only(CC), "entero"),
    (DECISIONS, "decisiones §43, §57, §61, §64 (acuerdos de A-1..A-4) y §73-§80 (§79: ronda 1 y correcciones R1-R5; §80: esta revisión, punto 6)",
     "SECTIONS", ranges_decisions, "§43, §57, §61, §64 y §73 en adelante"),
    (EVIDENCE, "evidencia de I-62 (registro de la sesión que preparó A-6 y este kit: hechos, resúmenes y valoraciones; evidencia, no autoridad), que A-6 cita (§140 en adelante); incluida en la ronda 2 por decisiones §79, punto 5 (acceso directo a toda fuente citada), con riesgo de anclaje declarado", "SECTIONS", ranges_evidence, "§140 en adelante (citadas por A-6)"),
]


def lines_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    if lines and lines[-1] == "":
        lines = lines[:-1]
    return lines


class NotUtf8(Exception):
    pass


def describe(b, path):
    try:
        t = b.decode("utf-8")
    except UnicodeDecodeError as e:
        raise NotUtf8("%s: %s" % (path, e))
    return {"Bytes": len(b), "Lines": len(lines_of(t)), "Sha256": hashlib.sha256(b).hexdigest(),
            "LinesOver2000Chars": [i + 1 for i, x in enumerate(t.split("\n")) if len(x) > 2000],
            "Encoding": {"Charset": "UTF-8", "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF",
                         "FinalNewline": b.endswith(b"\n")}}, t


def dir_files(rev, d):
    names = [n for n in git("ls-tree", "-r", "--name-only", rev, "--", d + "/").decode("utf-8").split("\n") if n]
    assert names and all(n.startswith(d + "/") for n in names), "el commit no tiene %s/" % d
    for n in names:
        assert re.match(r"^[A-Za-z0-9_./-]+$", n), "ruta con caracteres fuera de [A-Za-z0-9_./-]: %s" % n
    return sorted(names)


def rec(p, rev, why, scope, rf, label):
    b = git("show", rev + ":" + p)
    r, t = describe(b, p)
    blob = git("rev-parse", rev + ":" + p).decode().strip()
    r = dict({"Path": p, "Blob": blob}, **r)
    if p in PINNED:
        assert blob == PINNED[p], "%s: blob %s distinto del fijado (%s)" % (p, blob, PINNED[p])
        r["PinnedBlob"] = PINNED[p]
    if p == OBJECT:
        assert blob == PARAMS["Object"]["Blob"], "A-6: blob %s distinto del de kit-params.json" % blob
        r["PinnedBlob"] = blob
    L = lines_of(t)
    r.update({"Why": why, "Scope": scope, "PromptLabel": label, "LineRanges": rf(L)})
    for g in r["LineRanges"]:
        assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
    return r, t, L


def order_text_check(dec_lines):
    num, want, size = ORDER_TEXT
    s = section(dec_lines, "%s. " % num)
    text = " ".join(dec_lines[s["Start"] - 1:s["End"]])
    return ("`%s`" % want) in text and ("%d %03d bytes" % (size // 1000, size % 1000)) in text


def materiality_a6(L):
    s5 = section(L, "5. ")
    out = {}
    for m in MATERIALITY_IDS:
        cell = L[row(L, s5, (lambda m: lambda x: x.startswith("| %s |" % m))(m), m)["Start"] - 1].split("|")[2].strip()
        out[m] = {"**sí**": "yes", "no": "no"}[cell]
    return out


G11 = {}


def load_g11(rev):
    """Fuentes y rangos de G11 del resultado `run` custodiado de las guardas de la ronda 2 (o de PARAMS["G11File"] en el entorno sintético)."""
    global G11
    if PARAMS.get("G11File"):
        G11 = json.load(open(PARAMS["G11File"], encoding="utf-8"))
    else:
        G11 = json.loads(git("show", rev + ":" + R2_RUN_RESULT).decode("utf-8"))["Guards"]["G11"]
    assert G11.get("Sources"), "G11 sin Sources"
    return G11


def identity_checks():
    P = PARAMS
    pub, parent, grc = P["Publication"], P["PublicationParent"], P["GuardsRunCommit"]
    obj_blob = P["Object"]["Blob"]
    for c in (REV, pub, parent, grc):
        assert git_ok("cat-file", "-e", c + "^{commit}"), "commit ausente en el clon: %s" % c
    assert git("rev-parse", pub + "^").decode().strip() == parent, "el padre de la publicación no es %s" % parent[:8]
    assert git("rev-parse", parent + ":" + OBJECT).decode().strip() == A6R1_BLOB, "el padre de la publicación no tiene la A-6 de la ronda 1"
    for c in (pub, grc, REV):
        assert git("rev-parse", c + ":" + OBJECT).decode().strip() == obj_blob, "blob de A-6 distinto en %s" % c
    for a, b in ((pub, grc), (grc, REV)):
        assert git_ok("merge-base", "--is-ancestor", a, b), "%s no es antecesor de %s" % (a, b)
    pub_files = dir_files(pub, R2_DIR)
    now_files = dir_files(REV, R2_DIR)
    unchanged = all(git("rev-parse", pub + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip() for p in pub_files)
    added = set(now_files) - set(pub_files)
    allowed_added = set(P.get("R2AddedAfterPublication") or [R2_RUN_RESULT])
    assert unchanged and R2_RUN_RESULT in set(now_files) and added <= allowed_added, \
        "r2/ cambió después de la publicación además de añadir el resultado `run` (añadidos: %s)" % sorted(added)
    skip = set(P.get("SyntheticSkip") or []) if P.get("Synthetic") is True else set()   # solo el entorno sintético de la fase 1
    for d, t in PINNED_TREES.items():
        if "PinnedTree:" + d not in skip:
            assert git("rev-parse", REV + ":" + d).decode().strip() == t, "árbol %s distinto del fijado" % d
    old = git("cat-file", "-p", DECISIONS_PREFIX_BLOB)
    dec_b = git("show", REV + ":" + DECISIONS)
    assert dec_b.startswith(old), "decisiones del clon no crece solo por añadido desde la registrada con §80 (blob d094823a, commit 6a681cec)"
    run_res = json.loads(git("show", REV + ":" + R2_RUN_RESULT).decode("utf-8"))
    assert run_res.get("Mode") == "RUN" and run_res.get("Base") == parent and run_res.get("Head") == pub and run_res.get("Result") == "PASS", \
        "el resultado `run` custodiado no es Mode RUN / Base = padre / Head = publicación / PASS"
    st = json.loads(git("show", REV + ":" + R2_SELFTEST).decode("utf-8"))
    tool_blob = git("rev-parse", REV + ":" + R2_GUARDS).decode().strip()
    assert st.get("Result") == "PASS" and st.get("A6TextBlob") == obj_blob and st.get("ToolBlob") == tool_blob, \
        "la autoprueba custodiada no es PASS con A6TextBlob = objeto y ToolBlob = a6r2-guards.py"
    a6 = git("show", REV + ":" + OBJECT).decode("utf-8").split("\n")
    dec = dec_b.decode("utf-8").split("\n")
    assert sum(1 for l in a6 if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "A-6 ronda 2 sin la línea (a) fijada, una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "decisiones del clon sin la línea (a) del Owner (§78), una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.CLAUDE_LINE_SHA) == 1, "decisiones sin la línea CLAUDE-CLI-I62 = A fijada"
    for x in G11.get("Sources", []) + G11.get("GuardSources", []):
        if x["Path"] in (DECISIONS, EVIDENCE):      # crecen solo por añadido (decisiones: prefijo comprobado arriba; la evidencia, abajo)
            continue
        assert git("rev-parse", REV + ":" + x["Path"]).decode().strip() == x["Blob"], "G11: %s con otro blob en el clon" % x["Path"]
    ev_g11 = [x for x in G11.get("Sources", []) if x["Path"] == EVIDENCE]
    if ev_g11:
        old_ev = git("cat-file", "-p", ev_g11[0]["Blob"])
        assert git("show", REV + ":" + EVIDENCE).startswith(old_ev), "la evidencia del clon no crece solo por añadido desde la que fija G11"
    readable = []
    if git_ok("cat-file", "-e", REV + ":" + READABLE_DIR + "/MANIFEST.json"):
        man = json.loads(git("show", REV + ":" + READABLE_DIR + "/MANIFEST.json").decode("utf-8"))
        for e in man["Files"]:
            assert git("rev-parse", REV + ":" + e["Canonical"]).decode().strip() == e["CanonicalBlob"], "readable: blob canónico distinto: %s" % e["Canonical"]
            rb = git("show", REV + ":" + e["Readable"])
            assert hashlib.sha256(rb).hexdigest() == e["ReadableSha256"], "readable: SHA-256 de la copia distinto: %s" % e["Readable"]
            assert max(len(x) for x in rb.decode("utf-8").split("\n")) <= 2000, "readable: línea de más de 2 000 caracteres: %s" % e["Readable"]
            readable.append({"Readable": e["Readable"], "Canonical": e["Canonical"], "CanonicalBlob": e["CanonicalBlob"]})
    assert not any(p.startswith(C1C3_DIR + "/") for p in now_files), "C1-C3 dentro de r2/ (inesperado)"
    changed = [x for x in git("diff", "--name-status", pub, REV).decode("utf-8").split("\n") if x]
    return {"CloneCommit": REV, "Publication": pub, "PublicationParent": parent, "GuardsRunCommit": grc,
            "ObjectRound1AtPublicationParent": A6R1_BLOB, "ObjectBlobAtPublicationGuardsRunAndClone": obj_blob,
            "R2DirUnchangedSincePublicationExceptRunResult": True, "R2AddedAfterPublication": sorted(added),
            "R2Tree": git("rev-parse", REV + ":" + R2_DIR).decode().strip(), "PinnedTrees": PINNED_TREES,
            "DecisionsAppendOnlySinceSection80Record": True, "GuardsRunResult": {"Mode": "RUN", "Base": parent, "Head": pub, "Result": "PASS"},
            "SelfTest": {"Result": "PASS", "A6TextBlob": obj_blob, "ToolBlob": tool_blob, "Repo": st.get("Repo")},
            "OwnerLineOnceInA6R2AndInDecisions78": True, "ClaudeCliLinePinnedOnce": True, "G11SourcesBlobsMatchClone": True,
            "EvidenceAppendOnlySinceG11": bool(ev_g11), "ReadableCopies": readable,
            "ChangedSincePublication": changed}


def fmt_n(n):
    return "{:,}".format(n).replace(",", " ")


def range_text(g):
    return ("%d" % g["Start"]) if g["Start"] == g["End"] else ("%d-%d" % (g["Start"], g["End"]))


def short_label(path, s):
    m = re.match(r"^#+ (.*)$", s)
    if not m:
        return re.sub(r" \(L\d+(?:-L\d+)?\)$", "", s)
    t = m.group(1)
    for rx, fmt in ((r"^(\d+(?:\.\d+)+) ", "%s" if path == AP else "§%s"), (r"^(\d+)\. ", "§%s"), (r"^(D\.\d+) ", "%s"), (r"^(Anexo [A-Z]) ", "%s")):
        mm = re.match(rx, t)
        if mm:
            return fmt % mm.group(1)
    return t.split(" — ")[0][:40]


def prompt_entry(k, r):
    rs = [g for g in r["LineRanges"] if g["Section"] not in ("entero",) and not g.get("Span") and not g.get("G11")]
    head = "%d. `%s` (%s bytes, %s líneas; blob `%s`)" % (k, r["Path"], fmt_n(r["Bytes"]), fmt_n(r["Lines"]), r["Blob"][:8])
    body = r["PromptLabel"]
    if rs:
        body += ": " + "; ".join("%s %s" % (short_label(r["Path"], g["Section"]), range_text(g)) for g in rs)
    return head + ", " + body + "."


def group_items(rev):
    r2 = [(n, "evidencia de la ronda 2 de A-6 (guardas, autoprueba, resultados, preflight, reproducción e independencia): %s" % n[len(R2_DIR) + 1:],
           "ENTIRE", ranges_grr if n == R2_RUN_RESULT else (ranges_selftest if n == R2_SELFTEST else (ranges_doc if n.endswith(".md") else ranges_whole)),
           "evidencia de la ronda 2, entera") for n in dir_files(rev, R2_DIR)]
    pk = [(n, "paquete B-min, %s" % n[len(PKG_DIR) + 1:], "ENTIRE",
           ranges_g11_only(n) if n in (ACCEPTOR, PKG_DIR + "/accept/ACCEPT-NOTES.md") else (ranges_doc if n.endswith(".md") else ranges_whole),
           "paquete B-min, entero") for n in dir_files(rev, PKG_DIR)]
    ru = [(RULING_OUT, "dictamen de materialidad de B-min (R20261011T162328Z-5c5e, MATERIAL, ACCREDITED): salida estructurada", "ENTIRE",
           ranges_whole, "dictamen, entero"),
          (RULING_README, "dictamen de materialidad de B-min: README de su kit custodiado", "ENTIRE", ranges_doc, "dictamen, entero"),
          (R1_REVIEW_OUT, "revisión formal de la ronda 1 (R20261011T215735Z-c434; CHANGES REQUIRED; auditor NOT_ACCREDITED, decisiones §79): salida "
                          "estructurada con A62-A6-R1..R5 y A62-A6-O1..O3 literales", "ENTIRE", ranges_r1out, "revisión de la ronda 1, entera")]
    return [(OBJECT, "objeto: la enmienda A-6, ronda 2 (PROPUESTA), entera", "ENTIRE", ranges_a6, "objeto, entero")] + r2 + pk + ru


def g11_extra(items):
    have = {p for p, *_ in items}
    out = []
    for x in G11.get("Sources", []) + G11.get("GuardSources", []):
        p = x["Path"]
        if p in have or p in {q for q, *_ in out}:
            continue
        out.append((p, "fuente citada por A-6 (G11 del resultado `run` de sus guardas)", "SECTIONS", ranges_g11_only(p), "rangos citados por A-6 (G11)"))
    return out


def build(items):
    can, texts, Ls, not_utf8 = [], {}, {}, []
    for p, why, scope, rf, label in items:
        try:
            r, t, L = rec(p, REV, why, scope, rf, label)
        except NotUtf8 as e:
            not_utf8.append(str(e))
            continue
        can.append(r)
        texts[p], Ls[p] = t, L
    if not_utf8:
        raise SystemExit("archivos del cierre que no son texto UTF-8 (STOP: decidir antes de lanzar; no se omiten): %s" % not_utf8)
    assert len({r["Path"] for r in can}) == len(can), "rutas repetidas en el cierre"
    assert EVIDENCE in texts, "la evidencia I-62-evidence.md entra en el cierre en la ronda 2 (decisiones §79, punto 5)"
    assert not any(p.startswith(C1C3_DIR + "/") for p in texts), "C1-C3 no entra en el cierre"
    missing = [x["Path"] for x in G11.get("Sources", []) + G11.get("GuardSources", []) if x["Path"] not in texts]
    assert not missing, "fuentes de G11 fuera del cierre: %s" % missing
    return can, texts, Ls


def table(rows):
    w = max(len(r["Path"]) for r in rows)
    return "\n".join("%-*s  blob %s  %s bytes  %s líneas" % (w, r["Path"], r["Blob"], fmt_n(r["Bytes"]), fmt_n(r["Lines"])) for r in rows)


def g11_checklist(by=None):
    merged = {}
    for x in G11.get("Sources", []) + G11.get("GuardSources", []):
        m = merged.setdefault(x["Path"], {"Blob": x["Blob"], "Ranges": set()})
        m["Ranges"] |= {(a, b) for a, b in x["Ranges"]}
    out = []
    for p in sorted(merged):
        rs = ", ".join(("L%d" % a) if a == b else ("L%d-L%d" % (a, b)) for a, b in sorted(merged[p]["Ranges"]))
        tag = "  [registro de la sesión que preparó A-6: evidencia, no autoridad]" if p == EVIDENCE else ""
        cur = ((by or {}).get(p) or {}).get("Blob")
        blob = ("blob `%s`" % merged[p]["Blob"][:8]) if not cur or cur == merged[p]["Blob"] else \
            ("G11 sobre el blob `%s`, prefijo exacto del blob del cierre `%s`: mismas líneas" % (merged[p]["Blob"][:8], cur[:8]))
        out.append("- `%s` (%s): %s%s" % (p, blob, rs, tag))
    return "\n".join(out)


def render_prompt(can, Ls, run):
    tpl = open(os.path.join(KIT, "prompt.template.md"), encoding="utf-8").read()
    by = {r["Path"]: r for r in can}
    g = {p: {x["Section"]: x for x in by[p]["LineRanges"]} for p in by}
    dec, a6, r1o = g[DECISIONS], by[OBJECT], g[R1_REVIEW_OUT]
    a6_map = "; ".join("%s %s" % ((x["Section"] if x["Section"] in ("cabecera",) else
                                   re.sub(r"^#+ ", "", x["Section"]).split(" — ")[0].split(" (")[0]), range_text(x))
                       for x in a6["LineRanges"] if x["Section"] != "entero" and (x["Section"] == "cabecera" or x["Section"].startswith("#")))
    grr = g[R2_RUN_RESULT]
    st = [x for x in by[R2_SELFTEST]["LineRanges"] if x["Section"].startswith("Summary")][0]
    large = [r["Path"] for r in can if r["Bytes"] > LARGE] + [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE]
    long_lines = ["%s %s" % (r["Path"], ", ".join(str(n) for n in r["LinesOver2000Chars"])) for r in can if r["LinesOver2000Chars"]] + \
                 ["%s %s" % (n, ", ".join(str(k) for k in run[n]["LinesOver2000Chars"])) for n in PREMISE_RUN_FILES if run[n]["LinesOver2000Chars"]]
    grouped = (OBJECT, RULING_OUT, RULING_README, R1_REVIEW_OUT)
    entries = [prompt_entry(k + 1, r) for k, r in enumerate([r for r in can if r["Path"] not in grouped and
                                                             not r["Path"].startswith((R2_DIR + "/", PKG_DIR + "/"))])]
    ga = g[OBJECT]
    sec = lambda p: range_text([x for x in a6["LineRanges"] if x["Section"].startswith("### " + p + " ") or x["Section"].startswith("## " + p + " ")][0])
    q = lambda k: range_text(ga["§8, %s" % k])
    s80 = dec["§80, punto 6"]
    d79 = [x for k, x in dec.items() if k.startswith("## 79. ")][0]
    d80 = [x for k, x in dec.items() if k.startswith("## 80. ")][0]
    ev_g11 = [x for x in G11.get("Sources", []) if x["Path"] == EVIDENCE]
    v14d = DIRECT_READ
    rep = {
        "{{RUN_ID}}": IDS["RunId"], "{{INVOCATION_ID}}": IDS["InvocationId"], "{{LOGICAL_ID}}": IDS["LogicalReviewRequestId"],
        "{{CLONE}}": CLONE, "{{RUN}}": RUN,
        "{{CLONE_COMMIT}}": REV, "{{PUBLICATION}}": PARAMS["Publication"], "{{PUB_PARENT}}": PARAMS["PublicationParent"],
        "{{GUARDS_RUN_COMMIT}}": PARAMS["GuardsRunCommit"],
        "{{OBJECT_BLOB}}": a6["Blob"], "{{OBJECT_BLOB8}}": a6["Blob"][:8],
        "{{OBJECT_BYTES}}": fmt_n(a6["Bytes"]), "{{OBJECT_LINES}}": fmt_n(a6["Lines"]),
        "{{A6R1_BYTES}}": fmt_n(run["a6-r1.md"]["Bytes"]), "{{A6R1_LINES}}": fmt_n(run["a6-r1.md"]["Lines"]),
        "{{ORDER_LINES}}": str(run["order.txt"]["Lines"]),
        "{{A6_MAP}}": a6_map,
        "{{R2_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R2_DIR + "/")]),
        "{{R2_COUNT}}": str(sum(1 for p in by if p.startswith(R2_DIR + "/"))),
        "{{PKG_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(PKG_DIR + "/")]),
        "{{PKG_COUNT}}": str(sum(1 for p in by if p.startswith(PKG_DIR + "/"))),
        "{{RULING_TABLE}}": table([by[RULING_OUT], by[RULING_README], by[R1_REVIEW_OUT]]),
        "{{V14_DIRECT}}": "%d-%d" % (v14d["Start"], v14d["End"]),
        "{{V14_DIRECT_OFFSET}}": str(v14d["Start"] - 8), "{{V14_DIRECT_LIMIT}}": str(v14d["End"] - v14d["Start"] + 17),
        "{{DEC79_80}}": "%d-%d" % (d79["Start"], d80["End"]),
        "{{DEC79_P1}}": range_text(dec["§79, punto 1"]), "{{DEC79_P2}}": range_text(dec["§79, punto 2"]), "{{DEC79_P4}}": range_text(dec["§79, punto 4"]),
        "{{DEC80_SOURCE}}": range_text(dec["§80, bytes y SHA-256 del cuerpo pegado"]), "{{DEC80_P2}}": range_text(dec["§80, punto 2"]),
        "{{DEC80_P3}}": range_text(dec["§80, punto 3"]), "{{DEC80_P5}}": range_text(dec["§80, punto 5"]), "{{DEC80_P6}}": range_text(s80),
        "{{DEC78_LINE}}": range_text(dec["§78, línea del Owner A6-ADR0046-9-FX02"]),
        "{{GRR}}": R2_RUN_RESULT.replace("/", "\\"), "{{STJ}}": R2_SELFTEST.replace("/", "\\"),
        "{{GRR_HEAD}}": range_text(grr["Mode, Base y Head (L2-L4)"]), "{{GRR_RESULT}}": range_text(grr["Result de conjunto"]),
        "{{GRR_LINES}}": str(by[R2_RUN_RESULT]["Lines"]), "{{ST_TAIL}}": range_text(st),
        "{{R1OUT_HEAD}}": range_text(r1o["RunId, ids, objeto revisado y Verdict (cabecera)"]),
        "{{R1OUT_R}}": "; ".join("%s %s" % (f[-2:], range_text(r1o["hallazgo %s" % f])) for f in ROUND1_FINDING_IDS),
        "{{R1OUT_O}}": "; ".join("%s %s" % (f[-2:], range_text(r1o["hallazgo %s" % f])) for f in ROUND1_OPTIONAL_IDS),
        "{{A6_LINE_A}}": range_text(ga["§7, línea del Owner (a) A6-ADR0046-9-FX02"]),
        "{{A6_LINE_B}}": range_text(ga["§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO"]),
        "{{A6_S1}}": sec("1."), "{{A6_S33}}": sec("3.3"), "{{A6_S34}}": sec("3.4"), "{{A6_S36}}": sec("3.6"), "{{A6_S37}}": sec("3.7"),
        "{{A6_S38}}": sec("3.8"), "{{A6_S312}}": sec("3.12"), "{{A6_S313}}": sec("3.13"), "{{A6_S315}}": sec("3.15"), "{{A6_S316}}": sec("3.16"),
        "{{A6_S5}}": sec("5."), "{{A6_S62}}": sec("6.2"), "{{A6_S7}}": sec("7."), "{{A6_S8}}": sec("8."), "{{A6_S9}}": sec("9."),
        "{{A6_G}}": "; ".join("%s %s" % (k, range_text(ga["§3.12, %s" % k])) for k in GAP_IDS),
        "{{A6_A63_R1}}": range_text(ga["A6-3, regla 1"]), "{{A6_A63_R3}}": range_text(ga["A6-3, regla 3"]),
        "{{A6_A63_R5}}": range_text(ga["A6-3, regla 5"]), "{{A6_A63_R6}}": range_text(ga["A6-3, regla 6"]),
        "{{A6_A65_R2}}": range_text(ga["A6-5, regla 2"]), "{{A6_A61_R1}}": range_text(ga["A6-1, regla 1"]),
        "{{A6_A64_R1}}": range_text(ga["A6-4, regla 1"]),
        "{{Q01}}": q("Q-A6-01"), "{{Q02}}": q("Q-A6-02"), "{{Q0607}}": q("Q-A6-06, Q-A6-07"), "{{Q12}}": q("Q-A6-12"),
        "{{QR2}}": "%s-%s" % (ga["§8, Q-A6R2-01"]["Start"], ga["§8, Q-A6R2-06"]["End"]),
        "{{ACCEPTOR}}": ACCEPTOR, "{{ACCEPTOR_LINES}}": fmt_n(by[ACCEPTOR]["Lines"]),
        "{{G11_CHECKLIST}}": g11_checklist(by),
        "{{EVIDENCE_G11}}": (", ".join(("L%d" % a) if a == b else ("L%d-L%d" % (a, b)) for a, b in ev_g11[0]["Ranges"]) if ev_g11 else "ninguno"),
        "{{CLOSURE_LIST}}": "\n".join(entries),
        "{{LARGE_FILES}}": ", ".join("`%s`" % p for p in large),
        "{{LONG_LINES}}": "; ".join(long_lines) if long_lines else "ninguna",
    }
    for k, v in rep.items():
        assert k in tpl, "marcador ausente en la plantilla: %s" % k
        tpl = tpl.replace(k, v)
    assert not re.search("[{][{][A-Z0-9_]+[}][}]", tpl), "marcadores sin sustituir en el prompt: %s" % re.findall("[{][{][A-Z0-9_]+[}][}]", tpl)[:5]
    return tpl.rstrip("\n")


def run_files_described(run_dir, kit_dir):
    run = {}
    for n in RUN_FILES:
        p = os.path.join(run_dir, n)
        if not os.path.isfile(p):
            p = os.path.join(kit_dir, n)
        if not os.path.isfile(p):
            continue
        d, t = describe(open(p, "rb").read(), n)
        d["GitBlob"] = git_blob(open(p, "rb").read())
        run[n] = (d, t)
    return run


def build_kit(repo, rev, run_dir, out_dir, params, mode):
    """mode: 'render' → devuelve el prompt; 'closure' → escribe <out_dir>/closure.json (exige run = copias de <out_dir>)."""
    global CLONE, RUN, REPO, REV, OUT, PARAMS
    CLONE, RUN, REPO, REV, OUT, PARAMS = repo, run_dir, repo, rev, out_dir, params
    head = git("rev-parse", "HEAD").decode().strip()
    assert head == REV, "HEAD del clon (%s) distinto de CloneCommit (%s)" % (head, REV)
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    load_g11(REV)
    ident = identity_checks()
    items = group_items(REV) + canonical
    items += g11_extra(items)
    can, texts, Ls = build(items)
    assert order_text_check(Ls[DECISIONS]), "decisiones §80 no declara el SHA-256 y los bytes del cuerpo pegado"
    mat = materiality_a6(Ls[OBJECT])
    assert mat == MATERIALITY_A6_EXPECTED, "A-6 §5 (Activado) distinto de lo esperado: %s" % mat
    rd = run_files_described(run_dir, out_dir)
    run = {n: rd[n][0] for n in rd}
    assert run["order.txt"]["Sha256"] == ORDER_TEXT[1] and run["order.txt"]["Bytes"] == ORDER_TEXT[2], "order.txt distinto del cuerpo §80 custodiado"
    assert run["a6-r1.md"]["GitBlob"] == A6R1_BLOB, "a6-r1.md no es el blob eac473c0 de la ronda 1"
    prompt = render_prompt(can, Ls, run)
    if mode == "render":
        return prompt
    assert open(os.path.join(out_dir, "prompt.md"), "rb").read() == prompt.encode("utf-8"), "prompt.md distinto del renderizado: --render-prompt"
    listing = sorted(os.listdir(run_dir))
    assert listing == sorted(RUN_FILES), "el run debe tener exactamente %s (tiene %s)" % (RUN_FILES, listing)
    corpus = set()
    for r in can:
        corpus |= {c for c in texts[r["Path"]] if ord(c) > 127}
    for n in RUN_FILES:
        b = open(os.path.join(run_dir, n), "rb").read()
        assert b == open(os.path.join(out_dir, n), "rb").read(), "%s del run distinto de la copia del kit" % n
        if n != "prompt.md":
            corpus |= {c for c in rd[n][1] if ord(c) > 127}
    pb = open(os.path.join(run_dir, "prompt.md"), "rb").read()
    assert not pb.endswith(b"\n") and b"\r" not in pb, "prompt.md con salto final o CR"
    run["prompt.md"] = describe(pb, "prompt.md")[0]
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §80 del Coordinator (2 833 bytes; SHA-256 1342b5be…, el que declara decisiones §80 en "
                            "el commit); su punto 6 (líneas 41-51) ordena el contenido de esta revisión",
               "prompt.md": "kit/prompt.md renderizado desde kit/prompt.template.md; stdin literal del proceso claude-cli; nunca es premisa",
               "a6-r1.md": "texto de A-6 de la ronda 1 (blob eac473c0, decisiones §77, punto 1; commit 7b7aba46), byte a byte, para comparar R1-R5 y O1-O3 "
                           "con la ronda 2"}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    run_hashes["order.txt"]["IdentityCheck"] = {"DecisionsSection": "§" + ORDER_TEXT[0], "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": ORDER_TEXT[2]}
    run_hashes["a6-r1.md"]["GitBlob"] = A6R1_BLOB
    for r in can:
        r.pop("PromptLabel", None)
    ev_g11 = [x for x in G11.get("Sources", []) if x["Path"] == EVIDENCE]
    project = re.sub(r"[^A-Za-z0-9]", "-", CLONE)            # nombre del directorio de proyecto de claude-cli (D:\r62-arch-a6r2 → D--r62-arch-a6r2)
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "Provisional": bool(params.get("Provisional")), "Synthetic": bool(params.get("Synthetic")),
        "ObjectKind": "FILE", "ObjectPath": OBJECT, "ObjectBlob": params["Object"]["Blob"], "ObjectFiles": {OBJECT: params["Object"]["Blob"]},
        "PreviousObject": {"Path": OBJECT, "Blob": A6R1_BLOB, "RunFile": "a6-r1.md"},
        "Publication": {"Commit": params["Publication"], "Parent": params["PublicationParent"], "GuardsRunCommit": params["GuardsRunCommit"]},
        "PackagePath": None,
        "PackageNote": "sin paquete del Architect: el prompt hace de paquete (A-6 §11 y el Anexo prevén la revisión formal de la ronda 2 con una "
                       "disposición posterior del Coordinator: decisiones §79, punto 7; §80, puntos 5 y 6)",
        "Identity": ident,
        "PriorFindingIds": ROUND1_FINDING_IDS + ROUND1_OPTIONAL_IDS,
        "Question": "decisiones §80, punto 6 (order.txt, líneas 41-51): revisión formal de la ronda 2 de A-6 con lectura directa de V14 L2333-L2334, "
                    "R1-R5 y O1-O3 de la ronda 1, G1-G6 uno por uno, materialidad M-01, contraste del texto normativo con el código real del "
                    "aceptante y procedencia e integridad en el momento de uso; sin acreditación excepcional",
        "Topics": TOPICS, "RequiredTopics": REQUIRED_TOPICS, "Deltas": DELTAS, "MaterialityIds": MATERIALITY_IDS, "MaterialityA6": mat,
        "Round1FindingIds": ROUND1_FINDING_IDS, "Round1OptionalIds": ROUND1_OPTIONAL_IDS, "GapIds": GAP_IDS,
        "AcceptorClaims": ACCEPTOR_CLAIMS, "ProvenanceItems": PROVENANCE_ITEMS, "AcceptorPath": ACCEPTOR,
        "DirectRead": {k: DIRECT_READ[k] for k in ("Path", "Start", "End")},
        "G11": {"Source": "Guards.G11 de %s en el commit del clon" % R2_RUN_RESULT if not params.get("G11File") else "archivo sintético (fase 1)",
                "Refs": G11.get("Refs"), "Sources": [{"Path": x["Path"], "Blob": x["Blob"], "Ranges": x["Ranges"]} for x in G11.get("Sources", [])],
                "GuardSources": [{"Path": x["Path"], "Blob": x["Blob"], "Ranges": x["Ranges"]} for x in G11.get("GuardSources", [])],
                "Excluded": []},
        "OwnerGate": {"Check": OG.CHECK, "A6LineSha256": OG.A6_LINE_SHA, "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA, "RunId": OG.RUN_ID,
                      "Authorization": "una disposición del Coordinator numerada después de §80 que contenga el blob completo de la ronda 2 y el "
                                       "RunId; no está en el clon (es posterior); launch.py la exige en las decisiones del worktree en el momento "
                                       "del lanzamiento (owner_gate.py)"},
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ["--add-dir", RUN], "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ["--add-dir", RUN] + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "AddDirStatus": "medido en C3 de la caracterización R20261008T183600Z-char sobre la plantilla exacta con marcadores (decisiones §56, "
                                      "punto 7; §57, punto 3): kit/transport-gate.json en MEASURED con el SHA-256 del registro fijado",
                      "Gate": "kit/transport-gate.json (transport_gate.py, byte a byte el de los kits de A-4, A-5, B-min y la ronda 1 de A-6)",
                      "Stdin": "prompt.md del run (bytes literales)",
                      "JsonSchema": "kit/result.schema.json en forma compacta: json.dumps(separators=(',', ':'), ensure_ascii=True)",
                      "ExpectedInit": {"tools": ["Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "dontAsk", "mcp_servers": [],
                                       "claude_code_version": "2.1.293"},
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\" + project + "\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S, "AuthSettleSeconds": AUTH_SETTLE_S,
                      "AuthSettle": "launch.py espera AUTH_SETTLE_S segundos entre el final de `auth status` y el lanzamiento, y registra como "
                                    "TRANSPORT_AUTH_REFRESH_CONFLICT un result con «Failed to refresh OAuth token» y 0 tokens (sin reintento)",
                      "Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; §57, punto 3): C1 "
                                          "completada, C3 PASS sobre la plantilla con --add-dir"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos tres archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json, "
                                "con la reproducción de las guardas de la ronda 2) y el auditor lo vuelve a verificar después; el revisor contrasta lo "
                                "que puede leer: V14 L2333-L2334, order.txt, decisiones §79-§80 (bytes, SHA-256, puntos), el resultado `run` de las "
                                "guardas, la autoprueba (A6TextBlob) y la salida de la ronda 1",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` orienta (Scope SECTIONS o HEADER) o cubre el archivo entero (ENTIRE). Los rangos no "
                       "limitan: un archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo",
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run (r2/ y bmin/ están enteros en el cierre)",
            "LargeFileThresholdBytes": LARGE, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > LARGE] + [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE],
            "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can if r["LinesOver2000Chars"]},
            "ClauseRefs": "toda referencia (`ruta:línea` o `ruta:inicio-fin`) apunta a líneas leídas enteras con Read en ese mismo archivo y entregadas "
                          "fielmente: de archivos del cierre (ruta relativa al clon) o de order.txt y a6-r1.md (nombre suelto); nunca de prompt.md. Una "
                          "cita de una fuente dentro de A-6 no es una lectura de la fuente",
            "DirectRead": "V14 L2333-L2334 se leen con Read en docs/initiatives/I-62-proposal-v14.md (Paso 0) y constan en DirectReads",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "docs/automation/evidence/I-62-C1C3/ y los diseños de C1-C3 (decisiones §81)",
             "Reason": "fuera del cierre por decisión de la sesión principal: disposición del Owner sobre C1-C3, ajena a esta revisión; A-6 no se amplía (§81, punto 4)"},
            {"Source": "docs/automation/evidence/I-62-A6/ fuera de r2/ (evidencia de la ronda 1) y el resto del directorio de la revisión de la ronda 1 "
                       "(audit.json, README.md, kit/, launch/)",
             "Reason": "la orden del kit nombra r2/ y la salida de la ronda 1; la evidencia de la ronda 1 no cambia (G3 de las guardas lo comprueba)"},
            {"Source": "rutas que A-6 solo nombra como superficies o por blob (G11 Paths sin rangos de líneas: FOUNDATIONS, WORKFLOW, el registro del "
                       "Freeze, PROMPT_TEMPLATES y directorios)",
             "Reason": "A-6 no las cita por líneas; G3 de las guardas compara sus objetos en la base y en la cabeza"},
            {"Source": "la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud futura de medición (I-62-F6/requests/)",
             "Reason": "materiales de la sesión que prepara; las líneas del Owner están literales en A-6 §7 y la (a) en decisiones §78"},
            {"Source": "probe_tools.py, los reference.json / comparison.json de las sondas y los análisis citados por la evidencia",
             "Reason": "excluidos: la revisión se hace contra el texto normativo, no contra el oráculo de C-24 (§75.3)"},
            {"Source": "ejecución de las guardas, del paquete B-min, de la reproducción o de sus pruebas",
             "Reason": "sin herramientas de ejecución; los resultados custodiados están en el cierre y el invocador reprodujo las guardas"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: identidad de la cuenta y estado de Git del cwd (HEAD separado del clon, sin rama, y asuntos de los commits recientes)",
            "adjunto environment: cwd " + CLONE + ", plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para el clon (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión que preparó A-6, del Coordinator y de cualquier otra sesión)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*)",
            "los archivos de " + RUN + " distintos de los tres del run, incluido el subdirectorio launch/",
            "cualquier archivo del clon fuera de CanonicalInputs (también docs/automation/evidence/I-62-C1C3/ y el registro de caracterización)",
            "Grep o Glob cuyo alcance sea un directorio con archivos fuera del cierre; listados de directorios",
            "red (fetch, web)", "invocar otros agentes o subagentes", "escribir, hacer commit o push",
        ],
        "CorpusDistinctNonAscii": len(corpus),
    }
    with open(os.path.join(out_dir, "closure.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(closure, f, ensure_ascii=False, indent=1)
        f.write("\n")
    return {"ids": IDS, "canonical": len(can), "bytes": sum(r["Bytes"] for r in can),
            "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in RUN_FILES},
            "large": closure["ReadPolicy"]["LargeFiles"], "long": closure["ReadPolicy"]["LongLines"]}


def load_params():
    p = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
    for k in ("CloneCommit", "Publication", "PublicationParent", "GuardsRunCommit"):
        assert re.match(r"^[0-9a-f]{40}$", p.get(k) or ""), "kit-params.json: %s no es un SHA completo" % k
    assert re.match(r"^[0-9a-f]{40}$", (p.get("Object") or {}).get("Blob") or ""), "kit-params.json: Object.Blob no es un blob completo"
    assert p.get("Provisional") is False and not p.get("Synthetic"), "kit-params.json provisional o sintético: el cierre real exige Provisional = false"
    assert p.get("Clone") == CLONE_FIXED and p.get("Run") == RUN_FIXED, "kit-params.json con otro clon o run"
    return p


def main(argv):
    params = load_params()
    if "--render-prompt" in argv:
        prompt = build_kit(CLONE_FIXED, params["CloneCommit"], RUN_FIXED, KIT, params, "render")
        with open(os.path.join(KIT, "prompt.md"), "w", encoding="utf-8", newline="\n") as f:
            f.write(prompt)
        b = prompt.encode("utf-8")
        print(json.dumps({"Written": "prompt.md", "Bytes": len(b), "Lines": len(lines_of(prompt)), "Sha256": hashlib.sha256(b).hexdigest()}))
        return 0
    print(json.dumps(build_kit(CLONE_FIXED, params["CloneCommit"], RUN_FIXED, KIT, params, "closure"), ensure_ascii=False, indent=1))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
