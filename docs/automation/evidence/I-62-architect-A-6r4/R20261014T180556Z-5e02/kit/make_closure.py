"""I-62, decisiones §85, punto 6: EffectiveInputClosure de la ÚNICA REVISIÓN FORMAL de la RONDA 4 de la enmienda A-6 (docs/initiatives/I-62-A-6.md, blob de
la ronda 4 de kit-params.json, PROPUESTA) por un ARCHITECT independiente, acotada a A62-A6R3-R1 y a sus consecuencias directas, en una celda
`claude-cli` medida y elegible (autorización del Owner CLAUDE-CLI-I62 = A: revisiones de ARCHITECT de I-62, solo lectura). La revisión solo se
lanza con la compuerta 11 de launch.py en Ok: una disposición del Coordinator posterior a §85, fijada en owner_gate.py (número, encabezado y
SHA-256 de su texto, con el blob completo), y ninguna sección posterior.

Adaptado del make_closure.py del kit acreditado de la ronda 3 (R20261013T183701Z-9f80): la misma estructura de cierre, rangos, run y transporte; el
objeto es la A-6 de la ronda 4 en el commit del clon (CloneCommit de kit-params.json: un commit que contiene decisiones §85, la publicación de la
ronda 4 y la custodia del resultado `run` de sus guardas). El run tiene TRES archivos: order.txt (= cuerpo §85, 2 805 bytes), prompt.md
(renderizado desde prompt.template.md) y a6-r3.md (el texto de la ronda 3, blob e62f27b8, premisa: la revisión de la ronda 3 lo citó y la pureza
del delta se juzga frente a él).

Cierre canónico, en el commit del clon, cada archivo fijado por su blob (los directorios por su árbol):
  * A-6 de la ronda 4, entera (objeto): mapa de secciones; reglas de cada delta; las líneas de A6-3, regla 6 (b), sus apartados (i)-(v) y su frase
    final; las tablas y los párrafos de §3.17 y §3.18; la tabla de derivación de §3.12 (A6-9, regla 4) y G1-G6; la fila de §4 de N4 y nc1; las
    líneas del Owner y los párrafos «Ronda 3» y «Ronda 4» de §7; y las filas de §8 Q-A6R3-01..04 y Q-A6R4-01..04;
  * docs/automation/evidence/I-62-A6/r4/ entero (con readable/ y su MANIFEST comprobado);
  * de r3/: r1probes/ entero (las sondas que criticó A62-A6R3-R1), sus copias legibles de r3/readable/ (con el MANIFEST, comprobado entero) y los
    archivos de r3/ que citan r4/ o A-6 (lista fijada R3_CITED, comprobada mecánicamente sobre los textos);
  * de r2/: independence/ entero (escenario del banco de la ronda 4) y sus copias legibles de r2/readable/ (con el MANIFEST, comprobado entero);
  * la salida de la revisión de la ronda 3 (output.json, 691bbc8f), con rangos por A62-A6R3-R1, A62-A6R3-O1..O7 y NegativeCaseCoherence;
  * el paquete B-min entero (árbol 3caa0dfa: bmin_accept.py y bmin_facts.py, que cita la regla 6 (b) y las sondas);
  * la base normativa de la ronda 3 (V14, AUTOMATION_PLAN, ADR-0046, ADR-0048, LIFECYCLE, A-1..A-5, agent-execution README con §10 L219-L228,
    routing.md, adapters/codex-cli.md, cinco esquemas, kits/FX-02/controller-contracts.md con L23 y L44-L45, decisiones §43, §57, §61, §64 y §73
    en adelante, con las filas de §84 y §85);
  * toda fuente que liste G11 en el resultado `run` custodiado de las guardas de la ronda 4 (Sources y GuardSources; igual a
    a6r4-sources-for-reviewer.json), con sus rangos, incluida la evidencia (I-62-evidence.md, §140 en adelante; evidencia y no autoridad).
  Fuera: docs/automation/evidence/I-62-C1C3/ (C1-C3 y la nota de aislamiento de I-65) y todo material de I-65; el resto de r2/ y r3/; la evidencia
  de la ronda 1; las salidas de las revisiones de las rondas 1 y 2 y el dictamen de B-min; los demás archivos de los directorios de las revisiones.
Un archivo del cierre que no sea texto UTF-8 se informa (todos, con su ruta) y detiene el script; los archivos con líneas de más de 2 000
caracteres se informan con su copia legible (ReportedFiles). Los rangos se calculan aquí, mecánicamente, desde los encabezados, las filas y las
líneas citadas (con su primera línea comprobada); el script falla si algo no coincide.

Uso:
  python -B make_closure.py --render-prompt    → renderiza kit/prompt.md desde kit/prompt.template.md (clon de kit-params.json)
  python -B make_closure.py                    → escribe kit/closure.json (exige que kit/prompt.md sea el renderizado y que el run tenga
                                                  exactamente order.txt, prompt.md y a6-r3.md, iguales a las copias del kit)
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

CLONE_FIXED, RUN_FIXED = r"D:\r62-arch-a6r4", r"D:\r62-arch-a6r4-run"
IDS = {"RunId": "R20261014T180556Z-5e02", "InvocationId": "I20261014T180556Z-5e02", "LogicalReviewRequestId": "L20261014T180556Z-5e02",
       "AttemptSeq": 1}
SESSION_ID = "4cfdcf7d-0aa5-4a96-98ac-0c0a4832ec5b"          # --session-id fijado para el único lanzamiento (uuid4; prep/ids.txt)
OBJECT = "docs/initiatives/I-62-A-6.md"
A6R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"        # ronda 1 (decisiones §77, punto 1)
A6R2_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"        # ronda 2 (decisiones §80)
A6R3_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"        # ronda 3 (decisiones §83-§84): en el padre de la publicación y en a6-r3.md
R2_DIR = "docs/automation/evidence/I-62-A6/r2"
R2_INDEP = R2_DIR + "/independence"
R3_DIR = "docs/automation/evidence/I-62-A6/r3"
R3_PROBES = R3_DIR + "/r1probes"
R3_CITED = [R3_DIR + "/" + n for n in ("a6r3-guards.py", "launchfix/README.md", "make_candidate_commit_a6r3.py", "neg_controls_a6r3.py",
                                       "preflight.md", "r2static/README.md")]   # archivos de r3/ fuera de r1probes/ que citan r4/ o A-6 (comprobado)
R4_DIR = "docs/automation/evidence/I-62-A6/r4"
R4_GUARDS, R4_SELFTEST, R4_RUN_RESULT = R4_DIR + "/a6r4-guards.py", R4_DIR + "/a6r4-selftest.json", R4_DIR + "/a6r4-guards-result-run.json"
R4_SOURCES = R4_DIR + "/a6r4-sources-for-reviewer.json"
PKG_DIR = "docs/automation/evidence/I-62-F6/FX-02/bmin"
ACCEPTOR, TOOL = PKG_DIR + "/accept/bmin_accept.py", PKG_DIR + "/tool/bmin_facts.py"
R3_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6r3"
R3_REVIEW_OUT = R3_REVIEW_DIR + "/R20261013T183701Z-9f80/output.json"
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
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"          # DENTRO del cierre (como en las rondas 2 y 3): evidencia, no autoridad
C1C3_DIR = "docs/automation/evidence/I-62-C1C3"                 # FUERA del cierre (C1-C3 y la nota de I-65; decisiones §81-§82)
READABLE_DIRS = (R2_DIR + "/readable", R3_DIR + "/readable", R4_DIR + "/readable")   # copias legibles (líneas <= 1 900) de los JSON de una línea
CHAR_RECORD = "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json"   # lo fija la compuerta; fuera del cierre
PINNED = {   # blob fijado (sin cambio desde las rondas anteriores, o custodiado después de ellas); el commit del clon debe conservarlo
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534",
    ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e", LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f", A5: "ad94ceedacbf3dbf70f0217a583978d0e32592fc",
    RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055", ROUTING: "bba08fc4686d7192deeb559752636a467103b4b1",
    CODEX_CLI: "155f3469e345e2397fa26347a9cb52d6fdb91122", S_RI: "d54a7ae783ec83d055babdae090899908b616082",
    S_IC: "6bdf71843e2c70362a5e2d0ea19c8891e22bcff6", S_CV2: "e1faa9eca5c50b2b3a9f91575575614366962720",
    S_DEL2: "dfdbe461270e8fd92200c36bab78b9fdc3385014", S_WH: "cc3c9cf76b2247f3312e99bd447554326cc3700a",
    CC: "90929ad1c49c028f41f58f654f8713bd0b35f27b",
    R3_REVIEW_OUT: "691bbc8fabdefaa853c1d6e29465b6c1a1889107",
    ACCEPTOR: "434ef1fccb0d15762b30244b4a60928120698411", TOOL: "9a829b0eeb1210238052b1feac06cfa8f7964aed",
    CHAR_RECORD: "541956eb8f5d154414b91338c655919ae0ade697",
}
PINNED_TREES = {PKG_DIR: "3caa0dfa73d372af9251cf5c5c194261de8c7685", "docs/automation/agent-execution": "809fd58f450d3379a4fcc958cf8907fcdc5acff3",
                R2_DIR: "411949b0f20cb6edb1a01a208ccdfb1b3b19db76", R2_INDEP: "c4f969df3a1d5dad13b3ec7ed82038e90fc90b6c",
                R3_DIR: "a4e93edba524476d28e1d7d50df6e9ef1fccb5ab", R3_REVIEW_DIR: "f4442fdf2cdeb9a4e3abbe041443f32f77139745"}
DECISIONS_PREFIX_BLOB = "4e090ed4687330a5584ca8f43468bcd2c7b009aa"   # decisiones en ceb85977 (§85 registrada): prefijo exacto de las del clon
BASE_SECTION = 85                                              # §85: última sección de decisiones en el commit del clon (= owner_gate.BASE_SECTION)
RUN_FILES = ["order.txt", "prompt.md", "a6-r3.md"]
PREMISE_RUN_FILES = ["order.txt", "a6-r3.md"]                # prompt.md nunca
ORDER_TEXT = ("85", "d74e2aa1ce5eacc7dd6323cf3f3acec37fd6eb25f905ca4f2479dbca0cde5232", 2805)
MATERIALITY_IDS = ["M-0%d" % i for i in range(1, 9)]
MATERIALITY_A6_EXPECTED = {"M-01": "no", "M-02": "yes", "M-03": "yes", "M-04": "yes", "M-05": "yes", "M-06": "no", "M-07": "no", "M-08": "yes"}
ROUND3_FINDING_IDS = ["A62-A6R3-R1"]
ROUND3_OPTIONAL_IDS = ["A62-A6R3-O%d" % i for i in range(1, 8)]
NEGATIVE_CASES = ["N4", "NC1"]
CLOSURE_RULE_CASES = ["N4_ABSENT_DELIVERY", "NC1_NONEXISTENT_COMMIT", "ABSENT_NULL_EMPTY_FIELDS"]
CLOSURE_SUBRULES = ["i", "ii", "iii", "iv", "v"]
EVIDENCE_ASPECTS = ["DERIVATION", "INDEPENDENT_IMPLEMENTATION", "SUBSTITUTIONS", "ADDED_OR_INVENTED_INPUTS", "ACCEPTOR_RUNS", "O3_STRICT"]
MANDATORY_EVIDENCE_ASPECTS = EVIDENCE_ASPECTS[:5]             # las pruebas obligatorias de decisiones §85, punto 3 (O3_STRICT: punto 4)
ROUND4_QUESTION_IDS = ["Q-A6R4-0%d" % i for i in range(1, 5)]
GAP_IDS = ["G%d" % i for i in range(1, 7)]
DIRECT_READ = {"Path": V14, "Start": 2333, "End": 2334, "Token": "Esperado independiente"}
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
LAUNCH_COMMAND = "python -B kit/launch.py"   # la orden congelada de la ronda 3 (FROZEN_COMMAND de launch.py)
TIMEOUT_S = 3600               # el tope de la ronda 3
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


# ------------------------------------------------------------------ rangos mecánicos (los de los kits de las rondas 2 y 3)
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


def para(lines, rng, prefix, label):
    """Párrafo dentro de `rng` que empieza en la línea con `prefix` y sigue hasta antes de la primera línea en blanco."""
    first = row(lines, rng, lambda x: x.startswith(prefix), label)["Start"]
    end = first
    while end < rng["End"] and lines[end].strip() != "":
        end += 1
    return {"Section": label, "Start": first, "End": end}


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


def fenced_after(lines, rng, prefix, label):
    """Desde la línea de `rng` que empieza por `prefix` hasta el cierre del primer bloque ``` que la sigue (la tabla de derivación de §3.12)."""
    first = row(lines, rng, lambda x: x.startswith(prefix), label)["Start"]
    fences = [i + 1 for i in range(first, rng["End"]) if lines[i].startswith("```")]
    assert len(fences) >= 2, "%s: sin bloque ``` cerrado después de la línea %d" % (label, first)
    return {"Section": label, "Start": first, "End": fences[1]}


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


def json_member(L, key, label):
    """Miembro de primer nivel de un JSON con indent=1 (« "Key": [» o « "Key": {») hasta su cierre (« ]» o « }», con o sin coma)."""
    w = {"Start": 1, "End": len(L)}
    first = row(L, w, lambda x: x.startswith(' "%s": ' % key), label)["Start"]
    opener = L[first - 1].rstrip()[-1]
    closer = {"[": "]", "{": "}"}.get(opener)
    if closer is None:
        return {"Section": label, "Start": first, "End": first}
    end = first
    while end < len(L) and L[end].rstrip(",") != " " + closer:
        end += 1
    return {"Section": label, "Start": first, "End": end + 1}


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
        cited(L, 219, 228, "Se ejecutan una sola vez", "§10, controles negativos (L219-L228)"),
        cited(L, 561, 561, "| I2 |", "§15.2, regla I2 (L561)"), cited(L, 565, 568, "| I6 |", "§15.2, regla I6 y auditoría de lecturas (L565-L568)")] + \
        g11_ranges(RAE)


def ranges_cc(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 23, 23, "| nc1, nc2, nc3, N4, N5, N6, N7a, N7b |", "negativos con invocación: mismo prompt y Target de la verificación real (L23)"),
        cited(L, 44, 45, "| `AuthorityRevision` |", "AuthorityRevision y CanonicalInputs del kit (L44-L45)")] + g11_ranges(CC)


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


R3_CORRECTION_ROWS = ["R1 (a)", "R1 (b)", "R1 (c)", "R1 (d)", "R2 (a)", "R2 (b)", "O1", "O2", "O3", "O4", "O5"]
R3_PRECONDITION_ROWS = [("reconstrucción", "| reconstrucción de las ejecuciones desde los eventos reales"),
                        ("comparación", "| comparación de argv, entradas, bytes y resultados"),
                        ("identidad", "| identidad en el momento de uso"),
                        ("cobertura", "| cobertura completa de las lecturas legítimas"),
                        ("rechazo", "| rechazo ante evidencia incompleta o inconsistente")]
R4_CORRECTION_ROWS = ["R1"] + ["O%d" % i for i in range(1, 8)]
R4_BULLETS = [("derivación", "- **Derivación.**"), ("cierre independiente", "- **Cierre calculado de forma independiente.**"),
              ("aceptante fijado", "- **Aceptante fijado.**"), ("insumos y sustituciones", "- **Insumos añadidos o inventados y sustituciones.**")]
R4_PARAS = [("cambio normativo", "Cambio normativo solo en A6-3, regla 6 (b)"), ("N4 y nc1", "**N4 y nc1 con el cierre normativo**"),
            ("relación con A6-9", "**Relación con A6-9, regla 4**"), ("prueba previa", "**Momento y efecto de la prueba previa**"),
            ("G1-G6", "**Riesgos residuales G1-G6 en la ronda 4.**"), ("kit", "**Kit de la revisión de la ronda 4**")]
SUBRULE_RX = r"^>    \((i|ii|iii|iv|v)\) "
Q_ROWS = ["Q-A6R3-0%d" % i for i in range(1, 5)] + ROUND4_QUESTION_IDS


def ranges_a6(L):
    s = {h[2].split(" ")[0]: section(L, h[2]) for h in headings(L)[1:] if h[1] <= 3}
    s4, s7, s8, s312, s317, s318 = s["4."], s["7."], s["8."], s["3.12"], s["3.17"], s["3.18"]
    deltas = {"A6-%d" % n: s["3.%d" % (n + 3)] for n in range(1, 10)}
    rules = []
    for d, rng in deltas.items():
        rs = block_items(L, rng, r"^> (\d+)\. ", lambda x: x.startswith(">"), (lambda d: lambda m: "%s, regla %s" % (d, m.group(1)))(d))
        assert [r["Section"] for r in rs] == ["%s, regla %d" % (d, k + 1) for k in range(len(rs))] and rs, "A-6 %s: reglas no numeradas 1..n" % d
        rules += rs
    r6 = [r for r in rules if r["Section"] == "A6-3, regla 6"][0]
    b0 = row(L, r6, lambda x: x.startswith(">    (b) `Repo` = el `-C`"), "A6-3, regla 6 (b)")["Start"]
    c0 = row(L, r6, lambda x: x.startswith(">    (c) `Verification` = "), "A6-3, regla 6 (c)")["Start"]
    r6b = {"Section": "A6-3, regla 6 (b)", "Start": b0, "End": c0 - 1}
    final = row(L, r6b, lambda x: x.startswith(">    La prueba previa de la regla de A-6"), "A6-3, regla 6 (b), frase final")["Start"]
    subrules = block_items(L, {"Start": b0, "End": final - 1}, SUBRULE_RX, lambda x: x.startswith(">    "),
                           lambda m: "A6-3, regla 6 (b) (%s)" % m.group(1))
    assert [x["Section"] for x in subrules] == ["A6-3, regla 6 (b) (%s)" % k for k in CLOSURE_SUBRULES], "A6-3, regla 6 (b): apartados (i)-(v)"
    r6b_parts = [r6b, {"Section": "A6-3, regla 6 (b), encabezado (`Closure`)", "Start": b0, "End": subrules[0]["Start"] - 1}] + subrules + [
        {"Section": "A6-3, regla 6 (b), frase final (prueba previa y medición)", "Start": final, "End": c0 - 1}]
    gaps = block_items(L, s312, r"^- \*\*(G[1-6])\. ", lambda x: x.startswith("  "), lambda m: "§3.12, %s" % m.group(1))
    assert [g["Section"] for g in gaps] == ["§3.12, %s" % g for g in GAP_IDS], "A-6: G1..G6 no están, en orden, en §3.12"
    line_a = row(L, s7, lambda x: x.startswith("A6-ADR0046-9-FX02 = A ("), "§7, línea del Owner (a) A6-ADR0046-9-FX02")
    assert OG.line_sha(L[line_a["Start"] - 1]) == OG.A6_LINE_SHA, "A-6 §7: la línea (a) no es la fijada (SHA-256 83152eae…)"
    return whole(L) + [header_block(L)] + all_sections(L) + r6b_parts + [
        fenced_after(L, s312, "**Tabla de derivación del cierre del aceptante**", "§3.12, tabla de derivación del cierre del aceptante (A6-9, regla 4)"),
        para(L, s312, "La tabla no es mecánica desde", "§3.12, párrafo que sigue a la tabla de derivación"),
        table_at(L, s317, lambda x: x.startswith("| Hallazgo de la ronda 2 | Corrección |"), "§3.17, tabla de las correcciones de la ronda 3"),
        table_at(L, s317, lambda x: x.startswith("| Precondición | Regla |"), "§3.17, tabla de las precondiciones de R2"),
        para(L, s317, "**Precondiciones de R2**", "§3.17, precondiciones de R2 (párrafo)"),
        para(L, s317, "Límites de la reconstrucción de referencia", "§3.17, límites de la reconstrucción de referencia"),
        para(L, s317, "**Riesgos residuales G1-G6 en la ronda 3.**", "§3.17, riesgos residuales G1-G6 en la ronda 3"),
        para(L, s317, "**Procedimiento de lanzamiento del kit siguiente**", "§3.17, procedimiento de lanzamiento del kit siguiente"),
        table_at(L, s318, lambda x: x.startswith("| Hallazgo de la ronda 3 | Corrección |"), "§3.18, tabla de la corrección de la ronda 4")] + [
        row(L, s317, (lambda r: lambda x: x.startswith("| %s:" % r))(r), "§3.17, %s" % r) for r in R3_CORRECTION_ROWS] + [
        row(L, s317, (lambda p: lambda x: x.startswith(p))(p), "§3.17, precondición: %s" % k) for k, p in R3_PRECONDITION_ROWS] + [
        row(L, s318, (lambda r: lambda x: x.startswith("| %s:" % r))(r), "§3.18, %s" % r) for r in R4_CORRECTION_ROWS] + [
        para(L, s318, p, "§3.18, %s" % k) for k, p in R4_PARAS] + [
        dict(block_items(L, s318, "^" + re.escape(p), lambda x: x.startswith("  "), (lambda k: lambda m: "§3.18, viñeta: %s" % k)(k))[0])
        for k, p in R4_BULLETS] + [
        row(L, s4, lambda x: x.startswith("| Cierre del aceptante con una entrada que falta o un commit inexistente (N4, nc1) |"),
            "§4, fila de N4 y nc1"),
        line_a,
        row(L, s7, lambda x: x.startswith("A6-CONTROLLER-MEDICION-CONSUMO = A ("), "§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO"),
        row(L, s7, lambda x: x.startswith("- **(a) Determinación sobre ADR-0046 #9"), "§7, consecuencia (a)"),
        row(L, s7, lambda x: x.startswith("- **(b) Autorización de consumo"), "§7, consecuencia (b)"),
        para(L, s7, "**Ronda 3.**", "§7, ronda 3 y blobs fijados"),
        para(L, s7, "**Ronda 4.**", "§7, ronda 4 y blobs fijados")] + [
        row(L, s8, (lambda q: lambda x: x.startswith("| %s |" % q))(q), "§8, %s" % q) for q in Q_ROWS] + gaps + rules


def ranges_grr(L):
    return whole(L) + [cited(L, 2, 4, '"Mode": "RUN"', "Mode, Base y Head (L2-L4)"),
                       row(L, {"Start": 1, "End": len(L)}, lambda x: x in (' "Result": "PASS"', ' "Result": "PASS",'), "Result de conjunto")]


def ranges_selftest(L):
    a = row(L, {"Start": 1, "End": len(L)}, lambda x: x == ' "Summary": {', "")["Start"]
    b = row(L, {"Start": 1, "End": len(L)}, lambda x: x.startswith(' "A6TextBlob": '), "")["Start"]
    assert a <= b, "autoprueba: Summary después de A6TextBlob"
    return whole(L) + [{"Section": "Summary, Result, Mode, Repo, ToolBlob y A6TextBlob (L%d-L%d)" % (a, b), "Start": a, "End": b}]


def review_output_ranges(finding_ids, extra_members=()):
    def f(L):
        w = {"Start": 1, "End": len(L)}
        head = {"Section": "RunId, ids, objeto revisado y Verdict (cabecera)", "Start": 2,
                "End": row(L, w, lambda x: x.startswith(' "Verdict": '), "")["Start"]}
        out = whole(L) + [head]
        for fid in finding_ids:
            n = row(L, w, (lambda f_: lambda x: x == '   "FindingId": "%s",' % f_)(fid), fid)["Start"]
            e = n
            while e < len(L) and not L[e].startswith("  }"):
                e += 1
            out.append({"Section": "hallazgo %s" % fid, "Start": n - 1, "End": e + 1})
        for key in extra_members:
            out.append(json_member(L, key, key))
        out.append(row(L, w, lambda x: x.startswith(' "KnownLimitations": ['), "KnownLimitations"))
        return out
    return f


def ranges_decisions(L):
    present = sorted(int(m.group(1)) for m in (re.match(r"^## (\d+)\. ", x) for x in L) if m)
    s = {n: section(L, "%d. " % n) for n in present if n in (43, 57, 61, 64) or n >= 73}
    s84, s85 = s[84], s[85]
    rows84 = [row(L, s84, (lambda k: lambda x: x.startswith("| **%d. " % k))(k), "§84, punto %d" % k) for k in range(1, 11)]
    rows85 = [row(L, s85, (lambda k: lambda x: x.startswith("| **%d. " % k))(k), "§85, punto %d" % k) for k in range(1, 9)]
    src84 = row(L, s84, lambda x: "extraído de la transcripción de la sesión, suma " in x, "§84, bytes y SHA-256 del cuerpo pegado")
    src85 = row(L, s85, lambda x: "extraído de la transcripción de la sesión, suma " in x, "§85, bytes y SHA-256 del cuerpo pegado")
    owner78 = row(L, s[78], lambda x: OG.line_sha(x) == OG.A6_LINE_SHA, "§78, línea del Owner A6-ADR0046-9-FX02")
    last = max(n for n in s)
    return [span(s[73], s[last], "§73-§%d" % last), span(s[84], s[85], "§84-§85")] + [s[n] for n in sorted(s)] + [src84] + rows84 + \
        [src85] + rows85 + [owner78] + g11_ranges(DECISIONS)


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
          "(L2333-L2334, LECTURA DIRECTA OBLIGATORIA; C-24, L2363), D.1, D.3 (tabla de la Topología A, negativos y totales), D.4, D.5 y D.6; "
          "rangos citados por A-6 (G11)", "SECTIONS", ranges_v14, "§18, §20.2, §20.3, §20.3.1, Anexo C, D.1, D.3, D.4, D.5 y D.6"),
    (AP, "AUTOMATION_PLAN: 16.4 (receta), 16.9 (comprobaciones y modos de fallo; INVALID_OUTPUT L653), 16.10, 16.11 (recuperación), 16.13-16.14 "
         "(compatibilidad, Validate(M)) y 16.18-16.24 (adapters, binding, independencia, aceptación, invocación de rol, cierre de insumos y "
         "fidelidad); rangos citados por A-6 (G11)", "SECTIONS", ranges_ap, "16.4, 16.9, 16.10, 16.11, 16.13-16.14 y 16.18-16.24"),
    (ADR46, "ADR-0046 aceptado (protocolo de ejecución delegada; #6 verificación fail-closed; #9 nivel A)", "ENTIRE", ranges_adr(ADR46), "entero"),
    (ADR48, "ADR-0048 (roles, binding por capacidad, independencia por dimensiones; conserva #9, Level A)", "ENTIRE", ranges_adr(ADR48), "entero"),
    (LIFECYCLE, "LIFECYCLE §3 (M-01..M-08; OWNER-RESERVED), §5 (REQUIRED/OPTIONAL y veredictos) y §6 (enmiendas)", "SECTIONS",
     ranges_lifecycle, "§3, §5 y §6"),
    (A1, "A-1 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A2, "A-2 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A3, "A-3 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A4, "A-4 acordada (A4-1: precedente de A6-1 y A6-7; A4-6)", "ENTIRE", ranges_a4, "entera"),
    (A5, "A-5 acordada (A5-1; A5-2, fin de D.3 con A-1..A-5)", "ENTIRE", ranges_a5, "entera"),
    (RAE, "procedimiento de agent-execution: §5 (invocación del Controller), §8 (coherencia de la verificación; INVALID_OUTPUT), §10 (controles "
          "negativos, L219-L228: copias de las entradas salvo el campo mutado, oráculo relativo, nc1), §14 (binding, independencia) y §15 "
          "(invocación de rol, cierre e I2/I6, fidelidad)", "SECTIONS", ranges_rae, "§5, §8, §10 (L219-L228), §14 y §15"),
    (ROUTING, "enrutamiento estable (clases, elegibilidad de una celda, el propio Controller)", "ENTIRE", ranges_doc, "entero"),
    (CODEX_CLI, "descriptor del adapter codex-cli de la celda del Controller", "ENTIRE", ranges_doc, "entero"),
    (S_IC, "esquema rackcad-input-closure/v1 (contrato consumido: E de la tabla de derivación)", "ENTIRE", ranges_whole, "entero"),
    (S_RI, "esquema rackcad-role-invocation/v1 (contrato consumido: I de la tabla de derivación)", "ENTIRE", ranges_whole, "entero"),
    (S_CV2, "esquema rackcad-controller-verification/v2 (contrato consumido; lo lee R4 del aceptante)", "ENTIRE", ranges_whole, "entero"),
    (S_DEL2, "esquema rackcad-delegation/v2 (contrato consumido: D de la tabla de derivación)", "ENTIRE", ranges_whole, "entero"),
    (S_WH, "esquema rackcad-worker-handoff/v1 (contrato consumido: H de la tabla de derivación)", "ENTIRE", ranges_whole, "entero"),
    (CC, "contratos de las invocaciones del Controller de FX-02 (codex-cli, solo lectura; Target de la verificación real en L23; CanonicalInputs en "
         "L44-L45); sin valores esperados", "ENTIRE", ranges_cc, "entero"),
    (DECISIONS, "decisiones §43, §57, §61, §64 (acuerdos de A-1..A-4) y §73 en adelante (§84: revisión de la ronda 3; §85: su disposición, la "
                "corrección acotada de la ronda 4 y la preparación de este kit)", "SECTIONS", ranges_decisions,
     "§43, §57, §61, §64 y §73 en adelante"),
    (EVIDENCE, "evidencia de I-62 (registro de la sesión que preparó A-6 y este kit: hechos, resúmenes y valoraciones; evidencia, no autoridad), que "
               "A-6 cita (§140 en adelante; §166-§169: kit y revisión de la ronda 3, §85 y publicación de la ronda 4); en el cierre como en las "
               "rondas 2 y 3 (decisiones §79, punto 5: acceso directo a toda fuente citada), con riesgo de anclaje declarado", "SECTIONS",
     ranges_evidence, "§140 en adelante (citadas por A-6)"),
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
    """Fuentes y rangos de G11 del resultado `run` custodiado de las guardas de la ronda 4 (= a6r4-sources-for-reviewer.json, comprobado)."""
    global G11
    G11 = json.loads(git("show", rev + ":" + R4_RUN_RESULT).decode("utf-8"))["Guards"]["G11"]
    assert G11.get("Sources"), "G11 sin Sources"
    src = json.loads(git("show", rev + ":" + R4_SOURCES).decode("utf-8"))
    assert src.get("Sources") == G11.get("Sources") and src.get("GuardSources") == G11.get("GuardSources"), \
        "a6r4-sources-for-reviewer.json distinto de G11 del resultado `run` custodiado"
    return G11


def readable_manifest(rev, rdir):
    out = []
    man = json.loads(git("show", rev + ":" + rdir + "/MANIFEST.json").decode("utf-8"))
    listed = {rdir + "/MANIFEST.json"}
    for e in man["Files"]:
        assert git("rev-parse", rev + ":" + e["Canonical"]).decode().strip() == e["CanonicalBlob"], "readable: blob canónico distinto: %s" % e["Canonical"]
        rb = git("show", rev + ":" + e["Readable"])
        assert hashlib.sha256(rb).hexdigest() == e["ReadableSha256"], "readable: SHA-256 de la copia distinto: %s" % e["Readable"]
        assert max(len(x) for x in rb.decode("utf-8").split("\n")) <= 2000, "readable: línea de más de 2 000 caracteres: %s" % e["Readable"]
        out.append({"Readable": e["Readable"], "Canonical": e["Canonical"], "CanonicalBlob": e["CanonicalBlob"]})
        listed.add(e["Readable"])
    assert set(dir_files(rev, rdir)) == listed, "readable: archivos de %s fuera del MANIFEST" % rdir
    return out


def r3_cited_files(rev):
    """Archivos de r3/ fuera de r1probes/ que citan, por su ruta, la A-6 de la ronda 4 o algún archivo de r4/ (token `r3/<ruta>` o
    `docs/automation/evidence/I-62-A6/r3/<ruta>`, con la ruta de un archivo del árbol r3/)."""
    r3_files = set(dir_files(rev, R3_DIR))
    rx = re.compile(r"(?<![A-Za-z0-9_-])(?:docs/automation/evidence/I-62-A6/)?r3/([A-Za-z0-9_./-]+)")
    hits = set()
    for p in [OBJECT] + dir_files(rev, R4_DIR):
        try:
            t = git("show", rev + ":" + p).decode("utf-8")
        except UnicodeDecodeError:
            continue
        for m in rx.finditer(t):
            f = R3_DIR + "/" + m.group(1).rstrip(".")
            if f in r3_files and not f.startswith(R3_PROBES + "/"):
                hits.add(f)
    return sorted(hits)


def identity_checks():
    P = PARAMS
    pub, parent, grc = P["Publication"], P["PublicationParent"], P["GuardsRunCommit"]
    obj_blob = P["Object"]["Blob"]
    for c in (REV, pub, parent, grc):
        assert git_ok("cat-file", "-e", c + "^{commit}"), "commit ausente en el clon: %s" % c
    assert git("rev-parse", pub + "^").decode().strip() == parent, "el padre de la publicación no es %s" % parent[:8]
    assert git("rev-parse", parent + ":" + OBJECT).decode().strip() == A6R3_BLOB, "el padre de la publicación no tiene la A-6 de la ronda 3"
    assert obj_blob not in (A6R1_BLOB, A6R2_BLOB, A6R3_BLOB), "el objeto no puede ser el blob de las rondas 1, 2 o 3"
    for c in (pub, grc, REV):
        assert git("rev-parse", c + ":" + OBJECT).decode().strip() == obj_blob, "blob de A-6 distinto en %s" % c
    for a, b in ((pub, grc), (grc, REV)):
        assert git_ok("merge-base", "--is-ancestor", a, b), "%s no es antecesor de %s" % (a, b)
    pub_files = dir_files(pub, R4_DIR)
    now_files = dir_files(REV, R4_DIR)
    unchanged = all(git("rev-parse", pub + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip() for p in pub_files)
    added = set(now_files) - set(pub_files)
    allowed_added = set(P.get("R4AddedAfterPublication") or [R4_RUN_RESULT])
    assert unchanged and R4_RUN_RESULT in set(now_files) and added <= allowed_added and all(a == R4_RUN_RESULT for a in added), \
        "r4/ cambió después de la publicación además de añadir el resultado `run` (añadidos: %s)" % sorted(added)
    for d, t in PINNED_TREES.items():
        assert git("rev-parse", REV + ":" + d).decode().strip() == t, "árbol %s distinto del fijado" % d
    old = git("cat-file", "-p", DECISIONS_PREFIX_BLOB)
    dec_b = git("show", REV + ":" + DECISIONS)
    assert dec_b.startswith(old), "decisiones del clon no crece solo por añadido desde la registrada con §85 (blob 4e090ed4, commit ceb85977)"
    dec = dec_b.decode("utf-8").split("\n")
    numbered = [(i, int(m.group(1))) for i, m in ((i, OG.NUMBERED_RX.match(l)) for i, l in enumerate(dec)) if m]
    s85 = [i for i, n in numbered if n == BASE_SECTION]
    assert BASE_SECTION == OG.BASE_SECTION and len(s85) == 1 and max(n for _, n in numbered) == BASE_SECTION, \
        "decisiones del clon: §85 no es la última sección numerada, una sola vez (la autorización del lanzamiento tiene que ser posterior)"
    s85_text = OG.section_text(dec, s85[0])
    run_res = json.loads(git("show", REV + ":" + R4_RUN_RESULT).decode("utf-8"))
    assert run_res.get("Mode") == "RUN" and run_res.get("Base") == parent and run_res.get("Head") == pub and run_res.get("Result") == "PASS", \
        "el resultado `run` custodiado no es Mode RUN / Base = padre / Head = publicación / PASS"
    st = json.loads(git("show", REV + ":" + R4_SELFTEST).decode("utf-8"))
    tool_blob = git("rev-parse", REV + ":" + R4_GUARDS).decode().strip()
    summ = st.get("Summary") or {}
    assert st.get("Result") == "PASS" and st.get("A6TextBlob") == obj_blob and st.get("ToolBlob") == tool_blob and \
        summ.get("Mutants") == summ.get("Killed") and summ.get("Mutants"), \
        "la autoprueba custodiada no es PASS con A6TextBlob = objeto, ToolBlob = a6r4-guards.py y todos los mutantes muertos"
    a6 = git("show", REV + ":" + OBJECT).decode("utf-8").split("\n")
    assert sum(1 for l in a6 if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "A-6 ronda 4 sin la línea (a) fijada, una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "decisiones del clon sin la línea (a) del Owner (§78), una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.CLAUDE_LINE_SHA) == 1, "decisiones sin la línea CLAUDE-CLI-I62 = A fijada"
    for x in G11.get("Sources", []) + G11.get("GuardSources", []):
        if x["Path"] in (DECISIONS, EVIDENCE):      # crecen solo por añadido (comprobado abajo)
            continue
        assert git("rev-parse", REV + ":" + x["Path"]).decode().strip() == x["Blob"], "G11: %s con otro blob en el clon" % x["Path"]
    for path in (DECISIONS, EVIDENCE):
        for x in [y for y in G11.get("Sources", []) if y["Path"] == path]:
            assert git("show", REV + ":" + path).startswith(git("cat-file", "-p", x["Blob"])), "%s del clon no crece solo por añadido desde G11" % path
    readable = []
    for rdir in READABLE_DIRS:
        readable += readable_manifest(REV, rdir)
    for d in (R2_DIR, R3_DIR, R4_DIR):
        assert not any(p.startswith(C1C3_DIR + "/") for p in dir_files(REV, d)), "C1-C3 dentro de %s (inesperado)" % d
    cited_r3 = r3_cited_files(REV)
    assert cited_r3 == sorted(R3_CITED), "archivos de r3/ citados por A-6 o r4/ distintos de R3_CITED: %s" % cited_r3
    changed = [x for x in git("diff", "--name-status", pub, REV).decode("utf-8").split("\n") if x]
    return {"CloneCommit": REV, "Publication": pub, "PublicationParent": parent, "GuardsRunCommit": grc,
            "ObjectRound3AtPublicationParent": A6R3_BLOB, "ObjectBlobAtPublicationGuardsRunAndClone": obj_blob,
            "R4DirUnchangedSincePublicationExceptRunResult": True, "R4AddedAfterPublication": sorted(added),
            "R4Tree": git("rev-parse", REV + ":" + R4_DIR).decode().strip(), "PinnedTrees": PINNED_TREES,
            "DecisionsAppendOnlySinceSection85Record": True,
            "Decisions85": {"Line": s85[0] + 1, "Heading": dec[s85[0]], "TextSha256": hashlib.sha256(s85_text.encode("utf-8")).hexdigest(),
                            "LastNumberedSection": True},
            "GuardsRunResult": {"Mode": "RUN", "Base": parent, "Head": pub, "Result": "PASS"},
            "SelfTest": {"Result": "PASS", "A6TextBlob": obj_blob, "ToolBlob": tool_blob, "Repo": st.get("Repo"), "Mutants": summ.get("Mutants"),
                         "Killed": summ.get("Killed")},
            "G11EqualsSourcesForReviewer": True,
            "OwnerLineOnceInA6R4AndInDecisions78": True, "ClaudeCliLinePinnedOnce": True, "G11SourcesBlobsMatchClone": True,
            "DecisionsAndEvidenceAppendOnlySinceG11": True, "ReadableCopies": readable, "R3CitedFiles": cited_r3,
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


def evidence_ranges(n):
    if n == R4_RUN_RESULT:
        return ranges_grr
    if n == R4_SELFTEST:
        return ranges_selftest
    return ranges_doc if n.endswith(".md") else ranges_whole


def readable_selection(rev, rdir, canonicals):
    """MANIFEST.json de `rdir` y las copias legibles cuyo canónico está en `canonicals` (el MANIFEST se comprueba entero en identity_checks)."""
    man = json.loads(git("show", rev + ":" + rdir + "/MANIFEST.json").decode("utf-8"))
    return [rdir + "/MANIFEST.json"] + sorted(e["Readable"] for e in man["Files"] if e["Canonical"] in canonicals)


def group_items(rev):
    r4 = [(n, "evidencia de la ronda 4 de A-6 (derivación del cierre con la regla corregida, implementación independiente, sustituciones, insumos "
              "añadidos o inventados, pruebas del aceptante fijado, guardas, autoprueba, resultado `run`, negativos, preflight y copias legibles): %s"
           % n[len(R4_DIR) + 1:], "ENTIRE", evidence_ranges(n), "evidencia de la ronda 4, entera") for n in dir_files(rev, R4_DIR)]
    probes3 = [n for n in dir_files(rev, R3_DIR) if n.startswith(R3_PROBES + "/")]
    r3_names = probes3 + sorted(R3_CITED) + readable_selection(rev, R3_DIR + "/readable", set(probes3) | set(R3_CITED))
    r3 = [(n, "evidencia de la ronda 3 de A-6 (sondas r1probes/ que criticó A62-A6R3-R1, sus copias legibles y los archivos de r3/ que citan r4/ o "
              "A-6): %s" % n[len(R3_DIR) + 1:], "ENTIRE", evidence_ranges(n), "evidencia de la ronda 3 (selección)") for n in r3_names]
    indep = [n for n in dir_files(rev, R2_DIR) if n.startswith(R2_INDEP + "/")]
    r2_names = indep + readable_selection(rev, R2_DIR + "/readable", set(indep))
    r2 = [(n, "evidencia de la ronda 2 de A-6: independence/ (escenario del banco de la ronda 4) y sus copias legibles: %s" % n[len(R2_DIR) + 1:],
           "ENTIRE", evidence_ranges(n), "evidencia de la ronda 2 (independence/)") for n in r2_names]
    pk = [(n, "paquete B-min, %s" % n[len(PKG_DIR) + 1:], "ENTIRE",
           ranges_g11_only(n) if n in (ACCEPTOR, PKG_DIR + "/accept/ACCEPT-NOTES.md") else (ranges_doc if n.endswith(".md") else ranges_whole),
           "paquete B-min, entero") for n in dir_files(rev, PKG_DIR)]
    ru = [(R3_REVIEW_OUT, "revisión formal de la ronda 3 (R20261013T183701Z-9f80; auditor ACCREDITED; CHANGES REQUIRED; decisiones §84-§85): salida "
                          "estructurada con A62-A6R3-R1 y A62-A6R3-O1..O7 literales y su NegativeCaseCoherence", "ENTIRE",
           review_output_ranges(ROUND3_FINDING_IDS + ROUND3_OPTIONAL_IDS, ("NegativeCaseCoherence",)), "revisión de la ronda 3, entera")]
    return [(OBJECT, "objeto: la enmienda A-6, ronda 4 (PROPUESTA), entera", "ENTIRE", ranges_a6, "objeto, entero")] + r4 + r3 + r2 + pk + ru


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
    assert EVIDENCE in texts, "la evidencia I-62-evidence.md entra en el cierre (como en las rondas 2 y 3)"
    assert not any(p.startswith(C1C3_DIR + "/") for p in texts), "C1-C3 no entra en el cierre"
    assert not any("I-65" in p for p in texts), "ningún material de I-65 entra en el cierre"
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


def readable_of(can_paths, readable):
    return {e["Canonical"]: e["Readable"] for e in readable if e["Canonical"] in can_paths}


def render_prompt(can, Ls, run, ident):
    tpl = open(os.path.join(KIT, "prompt.template.md"), encoding="utf-8").read()
    by = {r["Path"]: r for r in can}
    g = {p: {x["Section"]: x for x in by[p]["LineRanges"]} for p in by}
    dec, a6, r3o = g[DECISIONS], by[OBJECT], g[R3_REVIEW_OUT]
    a6_map = "; ".join("%s %s" % ((x["Section"] if x["Section"] in ("cabecera",) else
                                   re.sub(r"^#+ ", "", x["Section"]).split(" — ")[0].split(" (")[0]), range_text(x))
                       for x in a6["LineRanges"] if x["Section"] != "entero" and (x["Section"] == "cabecera" or x["Section"].startswith("#")))
    grr = g[R4_RUN_RESULT]
    st = [x for x in by[R4_SELFTEST]["LineRanges"] if x["Section"].startswith("Summary")][0]
    large = [r["Path"] for r in can if r["Bytes"] > LARGE] + [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE]
    rmap = readable_of(set(by), ident["ReadableCopies"])
    long_lines = ["%s %s%s" % (r["Path"], ", ".join(str(n) for n in r["LinesOver2000Chars"]),
                               (" (copia legible: `%s`)" % rmap[r["Path"]]) if r["Path"] in rmap else "")
                  for r in can if r["LinesOver2000Chars"]] + \
                 ["%s %s" % (n, ", ".join(str(k) for k in run[n]["LinesOver2000Chars"])) for n in PREMISE_RUN_FILES if run[n]["LinesOver2000Chars"]]
    grouped = (OBJECT, R3_REVIEW_OUT)
    entries = [prompt_entry(k + 1, r) for k, r in enumerate([r for r in can if r["Path"] not in grouped and
                                                             not r["Path"].startswith((R2_DIR + "/", R3_DIR + "/", R4_DIR + "/", PKG_DIR + "/"))])]
    ga = g[OBJECT]
    sec = lambda p: range_text([x for x in a6["LineRanges"] if x["Section"].startswith("### " + p + " ") or x["Section"].startswith("## " + p + " ")][0])
    rule = lambda d, n: range_text(ga["%s, regla %d" % (d, n)])
    d85 = [x for k, x in dec.items() if k.startswith("## 85. ")][0]
    ev_g11 = [x for x in G11.get("Sources", []) if x["Path"] == EVIDENCE]
    cc_g11 = sorted({(a, b) for x in G11.get("Sources", []) if x["Path"] == CC for a, b in x["Ranges"]})
    v14d = DIRECT_READ
    r6b_sub = "; ".join("(%s) L%s" % (k, range_text(ga["A6-3, regla 6 (b) (%s)" % k])) for k in CLOSURE_SUBRULES)
    rep = {
        "{{RUN_ID}}": IDS["RunId"], "{{INVOCATION_ID}}": IDS["InvocationId"], "{{LOGICAL_ID}}": IDS["LogicalReviewRequestId"],
        "{{CLONE}}": CLONE, "{{RUN}}": RUN,
        "{{CLONE_COMMIT}}": REV, "{{PUBLICATION}}": PARAMS["Publication"], "{{PUB_PARENT}}": PARAMS["PublicationParent"],
        "{{GUARDS_RUN_COMMIT}}": PARAMS["GuardsRunCommit"],
        "{{OBJECT_BLOB}}": a6["Blob"], "{{OBJECT_BLOB8}}": a6["Blob"][:8],
        "{{OBJECT_BYTES}}": fmt_n(a6["Bytes"]), "{{OBJECT_LINES}}": fmt_n(a6["Lines"]),
        "{{A6R3_BYTES}}": fmt_n(run["a6-r3.md"]["Bytes"]), "{{A6R3_LINES}}": fmt_n(run["a6-r3.md"]["Lines"]),
        "{{ORDER_LINES}}": str(run["order.txt"]["Lines"]),
        "{{A6_MAP}}": a6_map,
        "{{R4_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R4_DIR + "/")]),
        "{{R4_COUNT}}": str(sum(1 for p in by if p.startswith(R4_DIR + "/"))),
        "{{R3_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R3_DIR + "/")]),
        "{{R3_COUNT}}": str(sum(1 for p in by if p.startswith(R3_DIR + "/"))),
        "{{R2_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R2_DIR + "/")]),
        "{{R2_COUNT}}": str(sum(1 for p in by if p.startswith(R2_DIR + "/"))),
        "{{PKG_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(PKG_DIR + "/")]),
        "{{PKG_COUNT}}": str(sum(1 for p in by if p.startswith(PKG_DIR + "/"))),
        "{{R3OUT_TABLE}}": table([by[R3_REVIEW_OUT]]),
        "{{V14_DIRECT}}": "%d-%d" % (v14d["Start"], v14d["End"]),
        "{{V14_DIRECT_OFFSET}}": str(v14d["Start"] - 8), "{{V14_DIRECT_LIMIT}}": str(v14d["End"] - v14d["Start"] + 17),
        "{{DEC85}}": "%d-%d" % (d85["Start"], d85["End"]),
        "{{DEC85_SOURCE}}": range_text(dec["§85, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC85_P1}}": range_text(dec["§85, punto 1"]), "{{DEC85_P2}}": range_text(dec["§85, punto 2"]),
        "{{DEC85_P3}}": range_text(dec["§85, punto 3"]), "{{DEC85_P4}}": range_text(dec["§85, punto 4"]),
        "{{DEC85_P5}}": range_text(dec["§85, punto 5"]), "{{DEC85_P6}}": range_text(dec["§85, punto 6"]),
        "{{DEC85_P7}}": range_text(dec["§85, punto 7"]), "{{DEC85_P8}}": range_text(dec["§85, punto 8"]),
        "{{DEC85_HEAD}}": "%d" % d85["Start"],
        "{{DEC78_LINE}}": range_text(dec["§78, línea del Owner A6-ADR0046-9-FX02"]),
        "{{GRR}}": R4_RUN_RESULT.replace("/", "\\"), "{{STJ}}": R4_SELFTEST.replace("/", "\\"),
        "{{GRR_HEAD}}": range_text(grr["Mode, Base y Head (L2-L4)"]), "{{GRR_RESULT}}": range_text(grr["Result de conjunto"]),
        "{{GRR_LINES}}": str(by[R4_RUN_RESULT]["Lines"]), "{{ST_TAIL}}": range_text(st),
        "{{R3OUT_HEAD}}": range_text(r3o["RunId, ids, objeto revisado y Verdict (cabecera)"]),
        "{{R3OUT_R}}": "; ".join("%s %s" % (f[-2:], range_text(r3o["hallazgo %s" % f])) for f in ROUND3_FINDING_IDS),
        "{{R3OUT_O}}": "; ".join("%s %s" % (f[-2:], range_text(r3o["hallazgo %s" % f])) for f in ROUND3_OPTIONAL_IDS),
        "{{R3OUT_NEG}}": range_text(r3o["NegativeCaseCoherence"]),
        "{{A6_LINE_A}}": range_text(ga["§7, línea del Owner (a) A6-ADR0046-9-FX02"]),
        "{{A6_LINE_B}}": range_text(ga["§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO"]),
        "{{A6_S36}}": sec("3.6"), "{{A6_S312}}": sec("3.12"), "{{A6_S317}}": sec("3.17"), "{{A6_S318}}": sec("3.18"), "{{A6_S4}}": sec("4."),
        "{{A6_S7}}": sec("7."), "{{A6_S8}}": sec("8."),
        "{{A6_R6B}}": range_text(ga["A6-3, regla 6 (b)"]), "{{A6_R6B_SUB}}": r6b_sub,
        "{{A6_R6B_HEAD}}": range_text(ga["A6-3, regla 6 (b), encabezado (`Closure`)"]),
        "{{A6_R6B_FINAL}}": range_text(ga["A6-3, regla 6 (b), frase final (prueba previa y medición)"]),
        "{{A6_DERIV}}": range_text(ga["§3.12, tabla de derivación del cierre del aceptante (A6-9, regla 4)"]),
        "{{A6_DERIV_PARA}}": range_text(ga["§3.12, párrafo que sigue a la tabla de derivación"]),
        "{{A6_S318_T}}": range_text(ga["§3.18, tabla de la corrección de la ronda 4"]),
        "{{A6_S318_ROWS}}": "; ".join("%s L%s" % (r, range_text(ga["§3.18, %s" % r])) for r in R4_CORRECTION_ROWS),
        "{{A6_S318_PARAS}}": "; ".join("%s L%s" % (k, range_text(ga["§3.18, %s" % k])) for k, _ in R4_PARAS),
        "{{A6_S318_BULLETS}}": "; ".join("%s L%s" % (k, range_text(ga["§3.18, viñeta: %s" % k])) for k, _ in R4_BULLETS),
        "{{A6_S317_T1}}": range_text(ga["§3.17, tabla de las correcciones de la ronda 3"]),
        "{{A6_S317_ROWS_R1}}": "; ".join("%s L%s" % (r, range_text(ga["§3.17, %s" % r])) for r in R3_CORRECTION_ROWS[:4]),
        "{{A6_S4_ROW}}": range_text(ga["§4, fila de N4 y nc1"]),
        "{{A6_S7_R4}}": range_text(ga["§7, ronda 4 y blobs fijados"]),
        "{{A6_A63_R6}}": rule("A6-3", 6), "{{A6_A65_R2}}": rule("A6-5", 2), "{{A6_A65_R4}}": rule("A6-5", 4), "{{A6_A65_R6}}": rule("A6-5", 6),
        "{{A6_A65_R7}}": rule("A6-5", 7), "{{A6_A67_R1}}": rule("A6-7", 1), "{{A6_A67_R7}}": rule("A6-7", 7), "{{A6_A69_R4}}": rule("A6-9", 4),
        "{{QR3}}": "%s-%s" % (ga["§8, Q-A6R3-01"]["Start"], ga["§8, Q-A6R3-04"]["End"]),
        "{{QR4}}": "%s-%s" % (ga["§8, Q-A6R4-01"]["Start"], ga["§8, Q-A6R4-04"]["End"]),
        "{{ACCEPTOR}}": ACCEPTOR, "{{ACCEPTOR_LINES}}": fmt_n(by[ACCEPTOR]["Lines"]),
        "{{CC_G11}}": ", ".join(("L%d" % a) if a == b else ("L%d-L%d" % (a, b)) for a, b in cc_g11) if cc_g11 else "ninguno",
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
    assert order_text_check(Ls[DECISIONS]), "decisiones §85 no declara el SHA-256 y los bytes del cuerpo pegado"
    mat = materiality_a6(Ls[OBJECT])
    assert mat == MATERIALITY_A6_EXPECTED, "A-6 §5 (Activado) distinto de lo esperado: %s" % mat
    rd = run_files_described(run_dir, out_dir)
    run = {n: rd[n][0] for n in rd}
    assert run["order.txt"]["Sha256"] == ORDER_TEXT[1] and run["order.txt"]["Bytes"] == ORDER_TEXT[2], "order.txt distinto del cuerpo §85 custodiado"
    assert run["a6-r3.md"]["GitBlob"] == A6R3_BLOB, "a6-r3.md no es el blob e62f27b8 de la ronda 3"
    prompt = render_prompt(can, Ls, run, ident)
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
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §85 del Coordinator (2 805 bytes; SHA-256 d74e2aa1…, el que declara decisiones §85 "
                            "en el commit); su punto 6 (líneas 46-49) ordena preparar este kit, acotado a R1 y a sus consecuencias directas",
               "prompt.md": "kit/prompt.md renderizado desde kit/prompt.template.md; stdin literal del proceso claude-cli; nunca es premisa",
               "a6-r3.md": "texto de A-6 de la ronda 3 (blob e62f27b8, decisiones §83; commit a128efef), byte a byte, para comparar A62-A6R3-R1, "
                           "A62-A6R3-O1..O7 (que citan sus líneas) y la pureza del delta con la ronda 4"}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    run_hashes["order.txt"]["IdentityCheck"] = {"DecisionsSection": "§" + ORDER_TEXT[0], "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": ORDER_TEXT[2]}
    run_hashes["a6-r3.md"]["GitBlob"] = A6R3_BLOB
    rmap = readable_of({r["Path"] for r in can}, ident["ReadableCopies"])
    reported = {"NonUtf8": [],
                "LinesOver2000Chars": [{"Path": r["Path"], "Lines": r["LinesOver2000Chars"], "ReadableCopy": rmap.get(r["Path"])}
                                       for r in can if r["LinesOver2000Chars"]] +
                                      [{"Path": n, "Lines": run[n]["LinesOver2000Chars"], "ReadableCopy": None}
                                       for n in RUN_FILES if run[n]["LinesOver2000Chars"]]}
    reported["LinesOver2000CharsWithoutReadableCopy"] = [x["Path"] for x in reported["LinesOver2000Chars"] if not x["ReadableCopy"]]
    for r in can:
        r.pop("PromptLabel", None)
    project = re.sub(r"[^A-Za-z0-9]", "-", CLONE)            # nombre del directorio de proyecto de claude-cli (D:\r62-arch-a6r4 → D--r62-arch-a6r4)
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "Provisional": bool(params.get("Provisional")), "Synthetic": bool(params.get("Synthetic")),
        "ObjectKind": "FILE", "ObjectPath": OBJECT, "ObjectBlob": params["Object"]["Blob"], "ObjectFiles": {OBJECT: params["Object"]["Blob"]},
        "PreviousObject": {"Path": OBJECT, "Blob": A6R3_BLOB, "RunFile": "a6-r3.md"},
        "Publication": {"Commit": params["Publication"], "Parent": params["PublicationParent"], "GuardsRunCommit": params["GuardsRunCommit"]},
        "PackagePath": None,
        "PackageNote": "sin paquete del Architect: el prompt hace de paquete (decisiones §85, punto 6: un kit de revisión acotado a R1 y a sus "
                       "consecuencias directas)",
        "Identity": ident,
        "PriorFindingIds": ROUND3_FINDING_IDS + ROUND3_OPTIONAL_IDS,
        "Question": "decisiones §85, punto 6 (order.txt, líneas 46-49): revisión formal independiente de la ronda 4 de A-6, acotada a A62-A6R3-R1 y a "
                    "sus consecuencias directas: el cierre de A62-A6R3-R1; si A6-3, regla 6 (b) (i)-(v), es total y mecánica, sin paso de juicio, "
                    "en N4, en nc1 y con campos ausentes, null o vacíos, y si introduce o sustituye insumos; la compatibilidad con A6-9, regla 4, y su "
                    "tabla; la coherencia de N4 y nc1; la adecuación de la evidencia de r4/r1probes/; la pureza del delta; el tratamiento de "
                    "O1-O7 según decisiones §85, punto 4; y las preguntas Q-A6R4-01..04. Lo que dependa de una sesión real sigue como precondición "
                    "con fallo cerrado; sin acreditación excepcional",
        "MaterialityA6": mat,
        "Round3FindingIds": ROUND3_FINDING_IDS, "Round3OptionalIds": ROUND3_OPTIONAL_IDS, "NegativeCases": NEGATIVE_CASES,
        "ClosureRuleCases": CLOSURE_RULE_CASES, "ClosureSubRules": CLOSURE_SUBRULES, "EvidenceAspects": EVIDENCE_ASPECTS,
        "MandatoryEvidenceAspects": MANDATORY_EVIDENCE_ASPECTS, "Round4QuestionIds": ROUND4_QUESTION_IDS, "AcceptorPath": ACCEPTOR,
        "DirectRead": {k: DIRECT_READ[k] for k in ("Path", "Start", "End")},
        "G11": {"Source": "Guards.G11 de %s en el commit del clon (= %s)" % (R4_RUN_RESULT, R4_SOURCES),
                "Refs": G11.get("Refs"), "Sources": [{"Path": x["Path"], "Blob": x["Blob"], "Ranges": x["Ranges"]} for x in G11.get("Sources", [])],
                "GuardSources": [{"Path": x["Path"], "Blob": x["Blob"], "Ranges": x["Ranges"]} for x in G11.get("GuardSources", [])],
                "Excluded": []},
        "OwnerGate": {"Check": OG.CHECK, "A6LineSha256": OG.A6_LINE_SHA, "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA, "RunId": OG.RUN_ID,
                      "BaseSection": OG.BASE_SECTION,
                      "Authorization": "una disposición del Coordinator POSTERIOR a §85 (decisiones §85, punto 6: «No lanzar revisión formal de ronda 4 "
                                       "hasta nueva disposición del Coordinator»), que no existe al congelar el kit. Se fija en owner_gate.py "
                                       "(AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA, que el kit entrega en None; pin_authorization.py calcula los "
                                       "valores) y se vuelve a congelar el kit; mientras está pendiente, la compuerta 11 bloquea con «authorization "
                                       "pending: …». Fijada, exactamente una sección con ese número, el encabezado fijado, el blob completo de la "
                                       "ronda 4 como token literal, el SHA-256 de su texto fijado y ninguna sección posterior en el momento del "
                                       "lanzamiento; launch.py lo exige en las decisiones del worktree (owner_gate.py). Este cierre no cambia al "
                                       "fijarla"},
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ["--add-dir", RUN], "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ["--add-dir", RUN] + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "LaunchCommand": LAUNCH_COMMAND,
                      "AddDirStatus": "medido en C3 de la caracterización R20261008T183600Z-char sobre la plantilla exacta con marcadores (decisiones §56, "
                                      "punto 7; §57, punto 3): kit/transport-gate.json en MEASURED con el SHA-256 del registro fijado",
                      "Gate": "kit/transport-gate.json (transport_gate.py byte a byte el de la ronda 3: la compuerta D-01 y cli_version y auth_status "
                              "con la salida en bytes y UTF-8 estricto)",
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
                                "con la reproducción de las guardas de la ronda 4) y el auditor lo vuelve a verificar después; el revisor contrasta lo "
                                "que puede leer: V14 L2333-L2334, order.txt, decisiones §85 (bytes, SHA-256, puntos), el resultado `run` de las "
                                "guardas, la autoprueba (A6TextBlob) y la salida de la ronda 3",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` orienta (Scope SECTIONS o HEADER) o cubre el archivo entero (ENTIRE). Los rangos no "
                       "limitan: un archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo",
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run (r4/, r3/r1probes/, r2/independence/ y bmin/ están "
                    "enteros en el cierre; el resto de r2/ y de r3/ no)",
            "LargeFileThresholdBytes": LARGE, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > LARGE] + [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE],
            "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can if r["LinesOver2000Chars"]},
            "ClauseRefs": "toda referencia (`ruta:línea` o `ruta:inicio-fin`) apunta a líneas leídas enteras con Read en ese mismo archivo y entregadas "
                          "fielmente: de archivos del cierre (ruta relativa al clon) o de order.txt y a6-r3.md (nombre suelto); nunca de prompt.md. Una "
                          "cita de una fuente dentro de A-6 no es una lectura de la fuente",
            "DirectRead": "V14 L2333-L2334 se leen con Read en docs/initiatives/I-62-proposal-v14.md (Paso 0) y constan en DirectReads",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
        },
        "ReportedFiles": reported,
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "docs/automation/evidence/I-62-C1C3/ (diseño de C1-C3 y nota de aislamiento preventivo de I-65; decisiones §81 y §82, punto 6) y "
                       "todo material de I-65",
             "Reason": "fuera del cierre (decisiones §85, punto 7; orden del kit): ajeno a esta revisión; A-6 no se amplía"},
            {"Source": "docs/automation/evidence/I-62-A6/r2/ fuera de independence/ (y sus copias legibles), r3/ fuera de r1probes/ y de los archivos "
                       "que citan r4/ o A-6 (R3_CITED), los directorios de r3/ que A-6 solo nombra (r2static/, launchfix/), la evidencia de la ronda 1",
             "Reason": "revisión acotada a A62-A6R3-R1 (decisiones §85, punto 6); G3 de las guardas comprueba los árboles r2/ y r3/ y la pureza del "
                       "delta se juzga frente a a6-r3.md y G5"},
            {"Source": "las salidas de las revisiones de las rondas 1 y 2, el dictamen de materialidad de B-min y el resto de los directorios de las "
                       "revisiones (audit.json, README.md, kit/, launch/, registros del lanzamiento)",
             "Reason": "el cierre lleva la salida estructurada de la revisión de la ronda 3, la que dispone esta ronda; G3 comprueba esos objetos"},
            {"Source": "rutas que A-6 solo nombra como superficies o por blob (G11 Paths sin rangos de líneas: FOUNDATIONS, WORKFLOW, el registro del "
                       "Freeze, PROMPT_TEMPLATES y directorios)",
             "Reason": "A-6 no las cita por líneas; G3 de las guardas compara sus objetos en la base y en la cabeza"},
            {"Source": "la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud futura de medición (I-62-F6/requests/)",
             "Reason": "materiales de la sesión que prepara; las líneas del Owner están literales en A-6 §7 y la (a) en decisiones §78"},
            {"Source": "probe_tools.py, los reference.json / comparison.json de las sondas de F6 y los análisis citados por la evidencia",
             "Reason": "excluidos: la revisión se hace contra el texto normativo, no contra el oráculo de C-24 (§75.3)"},
            {"Source": "ejecución de las guardas, del paquete B-min, de las sondas de r4/ (incluida la implementación independiente) o del lanzador",
             "Reason": "sin herramientas de ejecución; los resultados custodiados están en el cierre y el invocador reprodujo las guardas"},
            {"Source": "la disposición del Coordinator posterior a §85 que autoriza el lanzamiento",
             "Reason": "posterior al commit del clon; la comprueba la compuerta 11 en las decisiones del worktree antes de lanzar"},
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
    by_group = {}
    for r in can:
        k = "r4" if r["Path"].startswith(R4_DIR + "/") else ("r3" if r["Path"].startswith(R3_DIR + "/") else
                                                           ("r2" if r["Path"].startswith(R2_DIR + "/") else
                                                            ("bmin" if r["Path"].startswith(PKG_DIR + "/") else "resto")))
        g = by_group.setdefault(k, {"Files": 0, "Bytes": 0, "Lines": 0})
        g["Files"] += 1
        g["Bytes"] += r["Bytes"]
        g["Lines"] += r["Lines"]
    return {"ids": IDS, "canonical": len(can), "bytes": sum(r["Bytes"] for r in can), "lines": sum(r["Lines"] for r in can), "groups": by_group,
            "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in RUN_FILES},
            "large": closure["ReadPolicy"]["LargeFiles"], "reported": reported}


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
