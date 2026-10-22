"""I-62, decisiones §94: EffectiveInputClosure de la ÚNICA REVISIÓN FORMAL de la RONDA 2 de la enmienda A-7 (docs/initiatives/I-62-A-7.md, blob
8a528133… de kit-params.json, PROPUESTA ronda 2) por un ARCHITECT independiente, acotada a A62-A7-R1..R3 y a sus consecuencias (decisiones §94,
punto 6), en una celda `claude-cli` medida y elegible (autorización del Owner CLAUDE-CLI-I62 = A: revisiones de ARCHITECT de I-62, solo lectura).
La revisión solo se lanza con la compuerta 11 de launch.py en Ok: una disposición del Coordinator POSTERIOR a §94 que nombre esta revisión, fijada en
owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo), y ninguna sección posterior. En el kit entregado la autorización
está PENDIENTE (owner_gate.py con AUTH_* en None): la compuerta 11 bloquea.

Adaptado del make_closure.py del kit de A-7 ronda 1 (R20261019T010200Z-4a72, con la corrección de fidelidad de la fase 1): las mismas funciones de
rangos mecánicos, de registro por blob, de copias legibles y fieles, de transporte, de run y de política de lectura; cambian el objeto (la ronda
2), el alcance del cierre, las comprobaciones de identidad y el renderizado del prompt. El run tiene, además de las copias fieles, TRES archivos:
order.txt (= cuerpo pegado de §94, 3 522 bytes; premisa), a7-r1.md (el texto de la ronda 1, blob 6c29747d…, byte a byte; premisa: los hallazgos de
la ronda 1 lo citan y la pureza del delta se juzga frente a él) y prompt.md (renderizado desde prompt.template.md; nunca premisa).

Cierre canónico, en el commit del clon (CloneCommit de kit-params.json: b432c40f, con la publicación de la ronda 2, la custodia del resultado `run`
de sus guardas, las disposiciones propuestas de O1-O8 y decisiones §94), cada archivo fijado por su blob (los directorios por su árbol):
  * A-7 ronda 2, entera (objeto): cabecera, mapa de secciones, filas L1-L17 de §2, el texto de A7-1 con sus ocho puntos y las categorías (a)-(d), la
    tabla de §3.3, §3.6 (correcciones de la ronda 2, fila por fila), las viñetas de §3.1, §3.4 y §6, los párrafos de §3.5, las tablas de §4, §5,
    §8 (Q-A7-01..06 y Q-A7R2-01..03) y §11;
  * el preflight de la ronda 2 (docs/automation/evidence/I-62-A7/r2/ entero, con su MANIFEST.sha256 comprobado) y las disposiciones propuestas de
    O1-O8 (r2-dispositions/optional-dispositions.md);
  * el preflight de la ronda 1 (docs/automation/evidence/I-62-A7/ fuera de r2/ y r2-dispositions/, sin cambio desde la ronda 1);
  * la revisión de la ronda 1 (output.json con R1-R3 y O1-O8, audit.json y README.md de R20261019T010200Z-4a72);
  * las fuentes literales de A-7 §2 con sus rangos (V14 §20.3.1 y B.9; AUTOMATION_PLAN 16.3, 16.8, 16.9, 16.13 y 16.24; RAE §10 y §15.2; A-6, reglas
    citadas, con A6-3, regla 6 (b), y A6-5, regla 2), la A-5 (A5-1, A5-2) y el fin de A6-8 (A-6 L815-L816);
  * decisiones §90-§94 (con §62, U-19; §73, A-5 AGREED; §78, la línea del Owner), entera como archivo y con sus rangos;
  * la determinación de §91 y el resto de s91/, los borradores de §92 (s92/), del banco README, designaciones, p1-build.json, p3-n4.json y
    p4-nc1.json (los resultados de N4 y nc1 con los que compara el preflight de la ronda 2) y readings/readings.md: orientación para mencionar si
    R1-R3 cambian D-1 o las propuestas FC (no se revisan otra vez);
  * lo que cita el preflight de la ronda 1 de otras evidencias (r4/r1probes con sus copias legibles) y el kit a6m7-measure entero;
  * el paquete B-min entero (árbol 3caa0dfa: instrumento y aceptante fijados);
  * la base normativa: LIFECYCLE, ADR-0046, ADR-0048, el registro del Freeze, A-1..A-4 (cabecera como orientación), controller-contracts.md, el mapa
    de cláusulas y su esquema, los esquemas rackcad-input-closure/v1 y rackcad-role-invocation/v1;
  * la evidencia de I-62 (I-62-evidence.md, §187-§191 como orientación; evidencia y no autoridad).
  Fuera: docs/automation/evidence/I-62-C1C3/ y todo material de I-65; el resto de I-62-A6/; los kits de las revisiones anteriores (salvo los tres
  archivos de la revisión de la ronda 1); transcripciones, memoria, fixture y el repositorio de la entrega.
Fidelidad de lectura (decisiones §94, punto 5; fidelity.py): cada archivo del cierre pasa por el detector (BOM, UTF-8 inválido, NUL, CR suelto,
otros controles, líneas de más de 1 900 unidades UTF-16, U+FEFF interior, separadores de línea Unicode, extensiones que Read interpreta). Cada
archivo marcado tiene una COPIA FIEL (RACKCAD-COPIA-FIEL/1: los bytes exactos, sin pérdida, con la misma numeración de líneas) que es un archivo más
del run (fiel__<ruta con / → __>.txt) y que el revisor lee EN LUGAR del original; el cierre la lista en FaithfulCopies (blob y SHA-256 del original,
marcas con sus líneas, SHA-256 de la copia) y el prompt manda leerla, nunca el original. Si la copia de un archivo marcado no decodificara en sus
bytes exactos o no fuera legible fielmente, el script se detiene (STOP). Los archivos premisa del run (order.txt, a7-r1.md) no pueden estar
marcados (STOP). Un archivo que no sea UTF-8 ya no detiene el script: queda marcado INVALID_UTF8 y se lee por su copia. Fase 2: también se marca
todo archivo con un punto de código no ASCII fuera del conjunto medido (UNMEASURED_CHAR). Los rangos se calculan aquí, mecánicamente,
desde los encabezados, las filas y las líneas citadas (con su primera línea comprobada); el script falla si algo no coincide.

Uso:
  python -B make_closure.py --render-prompt    → renderiza kit/prompt.md desde kit/prompt.template.md (clon de kit-params.json)
  python -B make_closure.py --faithful-copies  → escribe en kit/ las copias fieles (fiel__*.txt) de los archivos marcados del cierre y borra
                                                  las fiel__*.txt que sobren; quien termina el kit las copia byte a byte al run
  python -B make_closure.py                    → escribe kit/closure.json (exige que kit/prompt.md sea el renderizado y que el run tenga
                                                  exactamente order.txt, prompt.md, a7-r1.md y las copias fieles, iguales a las
                                                  copias del kit)
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
_spec = importlib.util.spec_from_file_location("fidelity", os.path.join(KIT, "fidelity.py"))
FID = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(FID)
BYTES = {}                                                    # bytes de cada archivo del cierre (git show), para el detector y las copias

CLONE_FIXED, RUN_FIXED = r"D:\r62-arch-a7r2", r"D:\r62-arch-a7r2-run"
IDS = {"RunId": "R20261019T201030Z-d1bf", "InvocationId": "I20261019T201030Z-d1bf", "LogicalReviewRequestId": "L20261019T201030Z-d1bf",
       "AttemptSeq": 1}
SESSION_ID = "3dd5ac31-ddf5-4e4a-b732-b913933a9ae3"          # --session-id fijado para el único lanzamiento (uuid4; prep/p2/ids.txt)
OBJECT = "docs/initiatives/I-62-A-7.md"
OBJECT_BLOB = OG.OBJECT_BLOB                                  # 8a528133…: la ronda 2 (decisiones §94)
PREV_BLOB = "6c29747d61c5f689a195095ad3712e6d9dba62aa"        # la ronda 1 (commit 6fee8fd6): en el padre de la publicación y en a7-r1.md
A7_DIR = "docs/automation/evidence/I-62-A7"                   # preflight de la ronda 1 (sin cambio) y, debajo, los de la ronda 2
A7_RUN_RESULT = A7_DIR + "/a7-guards-result-run.json"          # resultado `run` de la ronda 1 (custodiado en 5e3141a2)
A7R2_DIR = A7_DIR + "/r2"
A7R2_RUN_RESULT, A7R2_SELFTEST = A7R2_DIR + "/a7-guards-result-run.json", A7R2_DIR + "/q7-guards-selftest.json"
A7R2_SUMMARY, A7R2_PREFLIGHT, A7R2_MANIFEST = A7R2_DIR + "/summary.json", A7R2_DIR + "/preflight.md", A7R2_DIR + "/MANIFEST.sha256"
A7R2_DISP = A7_DIR + "/r2-dispositions/optional-dispositions.md"
R1_REVIEW = "docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72"
R1_OUTPUT, R1_AUDIT, R1_README = R1_REVIEW + "/output.json", R1_REVIEW + "/audit.json", R1_REVIEW + "/README.md"
R1_RUN_ID = "R20261019T010200Z-4a72"
M7 = "docs/automation/evidence/I-62-A6/m7"
S91, S92 = M7 + "/s91", M7 + "/s92"
S91_DET = S91 + "/i6-determination.md"
S92_DRAFT, S92_PROPOSALS, S92_NOTES = S92 + "/contract/disposition-draft.md", S92 + "/readings/proposals.md", S92 + "/coordinator-notes.md"
BENCH = M7 + "/bench"
BENCH_README, BENCH_P1, BENCH_DESIG = BENCH + "/README.md", BENCH + "/p1-build.json", BENCH + "/designations"
BENCH_N4, BENCH_NC1 = BENCH + "/p3-n4.json", BENCH + "/p4-nc1.json"   # los resultados de N4 y nc1 con los que compara el preflight de la ronda 2 (q3)
READINGS_MD = M7 + "/readings/readings.md"
R4_DIR = "docs/automation/evidence/I-62-A6/r4"
R4_CITED = [R4_DIR + "/r1probes/" + n for n in ("p7-derive.json", "p9-n4.json", "p10-nc1.json")]   # los que cita el preflight de A-7, §5
R4_READABLE = R4_DIR + "/readable"
KITM7 = "docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure"
PKG_DIR = "docs/automation/evidence/I-62-F6/FX-02/bmin"
ACCEPTOR, TOOL = PKG_DIR + "/accept/bmin_accept.py", PKG_DIR + "/tool/bmin_facts.py"
AP = "docs/AUTOMATION_PLAN.md"
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
A1, A2, A3, A4, A5, A6 = ("docs/initiatives/I-62-A-%d.md" % i for i in (1, 2, 3, 4, 5, 6))
AE = "docs/automation/agent-execution/"
RAE = AE + "README.md"
CMAP, CMAP_SCHEMA = AE + "compatibility/I62-clause-map.json", AE + "compatibility/clause-map.schema.json"
S_IC, S_RI = AE + "schemas/input-closure.v1.schema.json", AE + "schemas/role-invocation.v1.schema.json"
CC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
DECISIONS = "docs/automation/decisions/I-62.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"          # DENTRO del cierre: evidencia, no autoridad (A-7 cita la evidencia §181)
C1C3_DIR = "docs/automation/evidence/I-62-C1C3"                 # FUERA del cierre (C1-C3 y la nota de I-65; decisiones §81-§82)
PINNED = {   # blob fijado (A-7 los cita en su cabecera y en §2, o el commit que los custodió); el commit del clon debe conservarlo
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534",
    ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e", LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f", A5: "ad94ceedacbf3dbf70f0217a583978d0e32592fc",
    A6: OG.A6_AGREED_BLOB, RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055", CC: "90929ad1c49c028f41f58f654f8713bd0b35f27b",
    S_IC: "6bdf71843e2c70362a5e2d0ea19c8891e22bcff6", S_RI: "d54a7ae783ec83d055babdae090899908b616082",
    CMAP: "4d49d3e1a63d2b6128c9d9eeac473156a421571a", CMAP_SCHEMA: "6b54dda9579d694200d8fc9ec26a4a846eef529b",
    ACCEPTOR: "434ef1fccb0d15762b30244b4a60928120698411", TOOL: "9a829b0eeb1210238052b1feac06cfa8f7964aed",
}
PINNED_TREES = {   # árbol en el commit del clon = el del commit que lo custodió (y, para B-min y agent-execution, el de las revisiones de A-6)
    PKG_DIR: ("3caa0dfa73d372af9251cf5c5c194261de8c7685", "9356edf141c795ea718a360e266334ded10d6a76"),
    "docs/automation/agent-execution": ("809fd58f450d3379a4fcc958cf8907fcdc5acff3", "088b3227a22ef6ab28b7ba43d5aa90de68d5b08f"),
    S91: ("3a96324c34f184c4ee5221d4857458c3d67d24f4", "703f1c3f9ee733ddfaa863d7b6c582625e3f7ac1"),
    S92: ("990226a88bae3ab08fd3cf3803689376d15a30a0", "5e3141a2f10bbfc5e4fe1dfe5823382da841efd1"),
    BENCH: ("3e53ba92b8106f033321665820c1a9a85c698765", "1e73c39b83159acfec588b2c3efbd2541a4b59ae"),
    M7 + "/readings": ("d982d5d28d542670775c2d5297da649c8d98f187", "1e73c39b83159acfec588b2c3efbd2541a4b59ae"),
    KITM7: ("0140eff55568db86d2b2d7889ad7822c59286ae1", "718a7a5f196d595fe1032af8e7ca6a906a1be0f7"),
    A7R2_DIR: ("85f0a0997634ce11156097b2d47155718b94118d", "b432c40f8ca7b1e915dff531cb9a8cad6466bb0c"),
    R1_REVIEW: ("a37a9d892ed20986ddae6454697f086188bdd03f", "e71400a2c7e842b4c8662e51675932623429bedf"),
}
DECISIONS_PREFIX_BLOB = "1de169d072c79217f545f9eb7c19285df53dddc1"   # decisiones en 5158e4bc (§93 registrada): prefijo exacto de las del clon
DECISIONS_AT_PUBLICATION = "b3be42d5445e47aeb435f4368a717a46091e007d"   # decisiones con §94 (b9334f22), iguales en la publicación y en el clon
BASE_SECTION, LAST_SECTION = 94, 94                            # = owner_gate.BASE_SECTION; §94 es la última sección del commit del clon
RUN_FILES = ["order.txt", "prompt.md", "a7-r1.md"]
PREMISE_RUN_FILES = ["order.txt", "a7-r1.md"]                 # prompt.md nunca
ORDER_TEXT = ("94", "515ccfa37205aa1d9d1fccbd3eb3c2ed0c2b56b8bb67c56102d1a8b0bea3db0e", 3522)
MATERIALITY_IDS = ["M-0%d" % i for i in range(1, 9)]
MATERIALITY_A7_EXPECTED = {"M-01": "no", "M-02": "yes", "M-03": "yes", "M-04": "yes", "M-05": "yes", "M-06": "no", "M-07": "no", "M-08": "no"}
ROUND1_FINDINGS = ["A62-A7-R1", "A62-A7-R2", "A62-A7-R3"]                                       # decisiones §94, punto 1
ROUND1_OPTIONALS = ["A62-A7-O%d" % i for i in range(1, 9)]                                       # decisiones §94, punto 4
REGRESSION_TOPICS = ["T04_EFF_EFF1_B1_MAP_VALIDATION", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE", "T06_CLAUSEMAP_COVERAGE_RULE",
                     "T07_MULTI_CATEGORY_ENTRIES", "T09_NO_UNAUTHORIZED_CHANGES"]               # los temas CONFORMS de la ronda 1
DELTA_CHANGES = ["R1_CATEGORY_C_ROLES", "R2_INVALIDATOR_RULE_AND_PROCEDURE", "R3_A62_RULE1_PRECISION", "O1_M04_MOTIVE", "O7_S31_MV6_MV7_CONTENT",
                 "O2_S35_REVERIFICATION", "P1_RED_PER_16_8", "P2_MEASUREMENT_WITHOUT_RELAY_FACTS", "P3_DERIVATION_AS_INVALIDATOR_PIECE",
                 "P4_RED_UNDETERMINED"]                                                           # A-7 §3.6 y el preflight de la ronda 2
QUESTION_IDS = ["Q-A7R2-01", "Q-A7R2-02", "Q-A7R2-03"]                                           # A-7 §8, preguntas de la ronda 2
R1_QUESTION_IDS = ["Q-A7-0%d" % i for i in range(1, 7)]
LIT_IDS = ["L%d" % i for i in range(1, 18)]
LIT_SOURCES = {"V14": V14, "AP": AP, "RAE": RAE, "A-6": A6}
DIRECT_READ = {"Path": AP, "Start": 616, "End": 617, "Token": "Una entrega **exige RED** si"}   # AP 16.8 (L16; A7-1, punto 1 (c))
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.299\f0584be0fbaf\claude.exe",
          "Sha256": "295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b",
          "Version": "2.1.299 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
LAUNCH_COMMAND = "python -B kit/launch.py"   # la orden congelada de las rondas de A-6 (FROZEN_COMMAND de launch.py)
TIMEOUT_S = 3600               # el tope de A-6 ronda 4 y de §93, punto 6
AUTH_SETTLE_S = 120            # mitigación del intento 1 de A-3: espera entre `auth status` (preflight 5) y el lanzamiento
LARGE = 60000
CLONE, RUN, REPO, REV, OUT = CLONE_FIXED, RUN_FIXED, CLONE_FIXED, None, KIT
PARAMS = {}
A7LIT = {}                     # ruta → [(id, a, b)] de las filas L1-L15 de A-7 §2 (parse_literals)


def git(*a):
    return subprocess.check_output(["git", "-C", REPO] + list(a))


def git_ok(*a):
    return subprocess.run(["git", "-C", REPO] + list(a), capture_output=True).returncode == 0


def git_blob(b):
    return hashlib.sha1(b"blob %d\x00" % len(b) + b).hexdigest()


# ------------------------------------------------------------------ rangos mecánicos (los de los kits de A-6, byte a byte)
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


# ------------------------------------------------------------------ A-7 §2: fuentes literales (L1-L17)
def parse_literals(L):
    """Filas L1-L17 de A-7 §2 → {Id: {Source, Path, Ranges}} (fuente por su prefijo: V14, AP, RAE o A-6; líneas «a», «a-b», separadas por comas).
    Ronda 2: una fila puede tener varias fuentes separadas por «;» (L16: AP 16.8; AP 16.9; L17: A-6 A6-3; A6-5); un grupo sin prefijo de fuente
    es de la fuente del grupo anterior, y todos los grupos de una fila tienen que ser del mismo archivo."""
    s2 = section(L, "2. ")
    out = {}
    for lid in LIT_IDS:
        n = row(L, s2, (lambda k: lambda x: x.startswith("| %s |" % k))(lid), "§2, %s" % lid)["Start"]
        cells = [c.strip() for c in L[n - 1].split("|", 4)]
        src, lines_cell = cells[2], cells[3]
        paths, cur = set(), None
        for grp in src.split(";"):
            key = grp.strip().split(" ")[0]
            cur = LIT_SOURCES.get(key, cur)
            assert cur, "A-7 §2, %s: grupo sin fuente reconocible (%r)" % (lid, grp)
            paths.add(cur)
        assert len(paths) == 1, "A-7 §2, %s: fuentes de archivos distintos en una fila (%s)" % (lid, sorted(paths))
        path = paths.pop()
        ranges = [(int(a), int(b or a)) for a, b in re.findall(r"(\d+)(?:-(\d+))?", lines_cell)]
        assert ranges, "A-7 §2, %s: sin líneas" % lid
        out[lid] = {"Source": src, "Path": path, "Ranges": ranges, "Line": n}
    return out


def literal_ranges(path):
    out = []
    for lid, a, b in A7LIT.get(path, []):
        out.append({"Section": "A-7 §2, %s (L%s)" % (lid, ("%d" % a) if a == b else "%d-L%d" % (a, b)), "Start": a, "End": b, "A7Lit": True})
    return out


EXTRA_A7_REFS = [(A6, 815, 816, "su uso exige otra corrección de A-6 y otra determinación del", "A-7 §1 y §3.2: fin de A6-8 (L815-L816)")]


# ------------------------------------------------------------------ rangos por archivo
def ranges_a7(L):
    s = {h[2].split(" ")[0]: section(L, h[2]) for h in headings(L)[1:] if h[1] <= 3}
    quote = para(L, s["3.2"], "> «**Admisión de los insumos del cierre de VERIFY de FX-02 (A-7).**", "§3.2, A7-1 (texto de la regla)")
    points = block_items(L, quote, r"^> (\d)\. \*\*", lambda x: x.startswith(">"), lambda m: "A7-1, punto %s" % m.group(1))
    assert [p["Section"] for p in points] == ["A7-1, punto %d" % k for k in range(1, 9)], "A7-1: puntos 1..8 (ronda 2: el 7 es el invalidador)"
    cats = block_items(L, points[0], r"^>    \(([a-d])\) \*\*", lambda x: x.startswith(">    "), lambda m: "A7-1, punto 1 (%s)" % m.group(1))
    assert [c["Section"] for c in cats] == ["A7-1, punto 1 (%s)" % k for k in "abcd"], "A7-1, punto 1: categorías (a)-(d)"
    bullets = lambda key, label: block_items(L, s[key], r"^- \*\*([^*]+)\*\*", lambda x: x.startswith("  "),
                                             lambda m: "%s, %s" % (label, m.group(1).rstrip(".:")))
    return whole(L) + [header_block(L)] + all_sections(L) + [
        row(L, s["2."], (lambda k: lambda x: x.startswith("| %s |" % k))(k), "§2, %s" % k) for k in LIT_IDS] + [quote] + points + cats + [
        table_at(L, s["3.3"], lambda x: x.startswith("| Categoría |"), "§3.3, tabla de categorías")] + [
        row(L, s["3.3"], (lambda c: lambda x: x.startswith("| **(%s) " % c))(c), "§3.3, fila (%s)" % c) for c in "abcd"] + \
        bullets("3.1", "§3.1") + bullets("3.4", "§3.4") + [
        para(L, s["3.5"], "No admite lecturas", "§3.5, lo que A-7 no hace"),
        para(L, s["3.5"], "**Residuo declarado", "§3.5, residuo declarado (clausemap)"),
        table_at(L, s["3.6"], lambda x: x.startswith("| Hallazgo de la ronda 1 |"), "§3.6, tabla de correcciones de la ronda 2")] + [
        row(L, s["3.6"], (lambda k: lambda x: x.startswith("| %s" % k))(k), "§3.6, fila %s" % k.rstrip(":"))
        for k in ("R1:", "R2:", "R3:", "O1:", "O7:", "O2, O3-O6, O8 |")] + [
        table_at(L, s["4."], lambda x: x.startswith("| Aspecto |"), "§4, tabla antes y después")] + [
        row(L, s["5."], (lambda m: lambda x: x.startswith("| %s |" % m))(m), "§5, %s" % m) for m in MATERIALITY_IDS] + \
        bullets("6.", "§6") + [
        para(L, s["7."], "Ninguna identificada.", "§7, consecuencias reservadas al Owner")] + [
        row(L, s["8."], (lambda q: lambda x: x.startswith("| %s |" % q))(q), "§8, %s" % q) for q in R1_QUESTION_IDS + QUESTION_IDS] + [
        para(L, s["8."], "Respuestas de la revisión de la ronda 1", "§8, respuestas de la ronda 1"),
        table_at(L, s["11."], lambda x: x.startswith("| Autoridad |"), "§11, estado de revisión")]


def ranges_v14(L):
    s18 = section(L, "18. ")
    s203, s2031 = section(L, "20.3 "), section(L, "20.3.1 ")
    intro_end = s2031["Start"] - 1
    while L[intro_end - 1].strip() == "":
        intro_end -= 1
    s203 = {"Section": "§20.3, introducción (hasta §20.3.1)", "Start": s203["Start"], "End": intro_end}
    d = {k: section(L, k + " ") for k in ("D.3", "D.6")}
    return [s18, cited(L, 960, 960, "| OV-I62-03 topología A |", "§18, OV-I62-03 (L960)"), cited(L, 974, 974, "| OD-5 |", "§18, OD-5 (L974)"),
            s203, s2031,
            cited(L, 1079, 1079, "| `CanonicalInputs[]` |", "§20.3.1, clase CanonicalInputs (L1079)"),
            cited(L, 1088, 1099, "2. En cada archivo se identifican", "§20.3.1, pasos 2-4 (L1088-L1099)"),
            section(L, "B.9 "),
            cited(L, 2231, 2233, "| `Target` |", "B.9, Target, AuthorityRevision y CanonicalInputs (L2231-L2233)"),
            cited(L, 2250, 2251, "El adapter renderiza la invocación concreta", "B.9, el revisor lee CanonicalInputs y nada más (L2250-L2251)"),
            cited(L, 2363, 2363, "| C-24 |", "Anexo C, C-24 (L2363)")] + [d[k] for k in d] + [
            cited(L, 2433, 2447, "**Topología A**", "D.3, Topología A: tabla de roles, adapters y topes (L2433-L2447)")] + literal_ranges(V14)


def ranges_ap(L):
    keys = ("16.3", "16.4", "16.5", "16.6", "16.7", "16.8", "16.9", "16.13", "16.14", "16.22", "16.24")
    s = {k: section(L, k + " ") for k in keys}
    s24 = s["16.24"]
    return [s[k] for k in s] + [
        cited(L, 490, 490, "`AuthorityRevision` es el SHA del commit", "16.3, AuthorityRevision (L490)"),
        cited(L, DIRECT_READ["Start"], DIRECT_READ["End"], DIRECT_READ["Token"],
              "16.8, RED de la cadena: una entrega «exige RED» (L616-L617): LECTURA DIRECTA OBLIGATORIA"),
        cited(L, 630, 643, "| 1 | `Termination` |", "16.9, filas 1-14 de la tabla de comprobaciones (L630-L643)"),
        table_at(L, s["16.9"], lambda x: x.startswith("| # | Id | Comprueba |"), "16.9, tabla de las catorce comprobaciones"),
        cited(L, 819, 827, "0. Precondición: Evaluate pasó E1-E3", "16.13, Resolve, pasos 0-4 (L819-L827)"),
        cited(L, 902, 908, "| MV-1 |", "16.13, Validate(M): MV-1..MV-7 (L902-L908)"),
        cited(L, 910, 918, "| Defecto | Efecto |", "16.13, tabla de defectos de Validate(M): MAP_INVALID (L910-L918)"),
        cited(L, 1264, 1264, "| `CanonicalInputs[]` |", "16.24, clase CanonicalInputs (L1264)"),
        cited(L, 1265, 1267, "| `AllowedTransitiveInputs[]` |", "16.24, clases AllowedTransitiveInputs, DeclaredRuntimeContext y ForbiddenInputs (L1265-L1267)"),
        cited(L, 1271, 1276, "2. cada obligación se clasifica", "16.24, cálculo del cierre, pasos 2-4 (L1271-L1276)"),
        cited(L, 1279, 1280, "ninguna prueba local", "16.24, auditoría de lecturas: P-22 y Contexto (L1279-L1280)"),
        cited(L, 1287, 1288, "**Fidelidad de los insumos**", "16.24, fidelidad de los insumos (L1287-L1288)"),
        table_at(L, s24, lambda x: x.startswith("| P-22 |"), "16.24, P-22..P-25")] + literal_ranges(AP)


def ranges_lifecycle(L):
    s3 = section(L, "3. ")
    return [s3] + [row(L, s3, (lambda m: lambda x: x.startswith("| **%s** |" % m))(m), "§3, %s" % m) for m in MATERIALITY_IDS] + [
        dict(row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "§3, UNKNOWN cuenta como activado"),
             End=row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "")["End"] + 1),
        row(L, s3, lambda x: x.startswith("Un cambio de alcance, no-objetivos, decision Owner"), "§3, OWNER-RESERVED"),
        section(L, "5. "), row(L, section(L, "5. "), lambda x: x.startswith("Un hallazgo es `REQUIRED`"), "§5, REQUIRED y OPTIONAL"),
        row(L, section(L, "5. "), lambda x: x.startswith("Solo quien emitio un REQUIRED"), "§5, veredictos"),
        section(L, "6. ")]


def ranges_adr(path):
    def f(L):
        dec = section(L, "Decisión")
        return whole(L) + [header_block(L)] + all_sections(L) + numbered_items(L, dec, "Decisión")
    return f


def ranges_rae(L):
    keep = ("5. ", "8. ", "10. ", "14. ", "14.", "15. ", "15.")
    return whole(L) + [section(L, h[2]) for h in headings(L) if h[1] in (2, 3) and h[2].startswith(keep)] + [
        cited(L, 219, 228, "Se ejecutan una sola vez", "§10, controles negativos (L219-L228)"),
        cited(L, 561, 561, "| I2 |", "§15.2, regla I2 (L561)"), cited(L, 565, 568, "| I6 |", "§15.2, regla I6 y auditoría de lecturas (L565-L568)")] + \
        literal_ranges(RAE)


def ranges_cc(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 23, 23, "| nc1, nc2, nc3, N4, N5, N6, N7a, N7b |", "negativos con invocación: mismo prompt y Target de la verificación real (L23)"),
        cited(L, 44, 45, "| `AuthorityRevision` |", "AuthorityRevision y CanonicalInputs del kit (L44-L45)")]


def ranges_a5(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 175, 199, "> «**Construcción del fixture conforme al mapa de cláusulas (A-5).**", "§3.2, párrafo A5-1 (L175-L199)"),
        cited(L, 216, 216, "(**11** base, pool **4**", "§3.3, A5-2, regla 4 (L216)"),
        cited(L, 221, 221, "extiende a esta fila.»", "§3.3, fin de A5-2 y de D.3 con A-1..A-5 (L221)")]


def ranges_a6(L):
    s = {h[2].split(" ")[0]: section(L, h[2]) for h in headings(L)[1:] if h[1] <= 3}
    deltas = {"A6-%d" % n: s["3.%d" % (n + 3)] for n in range(1, 10)}
    rules = []
    for d, rng in deltas.items():
        rs = block_items(L, rng, r"^> (\d+)\. ", lambda x: x.startswith(">"), (lambda d: lambda m: "%s, regla %s" % (d, m.group(1)))(d))
        assert [r["Section"] for r in rs] == ["%s, regla %d" % (d, k + 1) for k in range(len(rs))] and rs, "A-6 %s: reglas no numeradas 1..n" % d
        rules += rs
    r6 = [r for r in rules if r["Section"] == "A6-3, regla 6"][0]
    b0 = row(L, r6, lambda x: x.startswith(">    (b) `Repo` = el `-C`"), "A6-3, regla 6 (b)")["Start"]
    c0 = row(L, r6, lambda x: x.startswith(">    (c) `Verification` = "), "A6-3, regla 6 (c)")["Start"]
    s7 = s["7."]
    line_a = row(L, s7, lambda x: x.startswith("A6-ADR0046-9-FX02 = A ("), "§7, línea del Owner (a) A6-ADR0046-9-FX02")
    assert OG.line_sha(L[line_a["Start"] - 1]) == OG.A6_LINE_SHA, "A-6 §7: la línea (a) no es la fijada (SHA-256 83152eae…)"
    extra = [cited(L, a, b, tok, label) for p, a, b, tok, label in EXTRA_A7_REFS if p == A6]
    return whole(L) + [header_block(L)] + all_sections(L) + [{"Section": "A6-3, regla 6 (b)", "Start": b0, "End": c0 - 1},
        fenced_after(L, s["3.12"], "**Tabla de derivación del cierre del aceptante**", "§3.12, tabla de derivación del cierre del aceptante (A6-9, regla 4)"),
        line_a, row(L, s7, lambda x: x.startswith("A6-CONTROLLER-MEDICION-CONSUMO = A ("), "§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO")] + \
        [row(L, s["5."], (lambda m: lambda x: x.startswith("| %s |" % m))(m), "§5, %s" % m) for m in MATERIALITY_IDS] + rules + extra + literal_ranges(A6)


def ranges_header_only(L):
    return whole(L) + [header_block(L)]


def ranges_decisions(L):
    present = sorted(int(m.group(1)) for m in (re.match(r"^## (\d+)\. ", x) for x in L) if m)
    s = {n: section(L, "%d. " % n) for n in present if n in (62, 73, 78) or n >= 90}
    rows_of = lambda n, k: [row(L, s[n], (lambda j: lambda x: x.startswith("| **%d. " % j))(j), "§%d, punto %d" % (n, j)) for j in range(1, k + 1)]
    src = lambda n: row(L, s[n], lambda x: "extraído de la transcripción de la sesión, suma " in x, "§%d, bytes y SHA-256 del cuerpo pegado" % n)
    obj93 = para(L, s[93], "**Objeto identificado y verificado (punto 1):**", "§93, objeto identificado y verificado (punto 1)")
    agreed90 = para(L, s[90], "**Acuerdo registrado (punto 1):**", "§90, acuerdo registrado de A-6 (blob 4bc919dd)")
    reg92 = para(L, s[92], "**Registro de lecturas (punto 5):**", "§92, registro de las lecturas (punto 5)")
    owner78 = row(L, s[78], lambda x: OG.line_sha(x) == OG.A6_LINE_SHA, "§78, línea del Owner A6-ADR0046-9-FX02")
    u19 = [row(L, s[62], lambda x: x.startswith("U-19 = «/**» final"), "§62, U-19 (lectura registrada)"),
           row(L, s[62], lambda x: x.startswith("| U-19 | entera |"), "§62, U-19 dispuesta entera")]
    last = max(n for n in s)
    assert last == LAST_SECTION, "decisiones del clon: la última sección no es §94"
    return [span(s[90], s[last], "§90-§%d" % last)] + [s[n] for n in sorted(s)] + [src(n) for n in (90, 91, 92, 93, 94)] + rows_of(90, 7) + \
        rows_of(91, 8) + rows_of(92, 9) + rows_of(93, 9) + rows_of(94, 9) + [obj93, agreed90, reg92, owner78] + u19


def ranges_evidence(L):
    secs = [section(L, h[2]) for h in headings(L) if h[1] == 2 and re.match(r"^(\d+)\. ", h[2]) and int(h[2].split(".")[0]) >= 187]
    return whole(L) + secs


def ranges_doc(L):
    return whole(L) + [header_block(L)] + all_sections(L)


def ranges_whole(L):
    return whole(L)


def ranges_guards_run(L):
    w = {"Start": 1, "End": len(L)}
    return whole(L) + [row(L, w, (lambda k: lambda x: x.startswith(' "%s": ' % k))(k), "%s (resultado `run`)" % k)
                       for k in ("A7Blob", "Base", "Head", "Mode", "Result")]


def ranges_summary(L):
    w = {"Start": 1, "End": len(L)}
    return whole(L) + [json_member(L, "Candidate", "Candidate (blob de la candidata)"),
                       {"Section": "Hold, Cases y NotHolding", "Start": row(L, w, lambda x: x.startswith(' "Hold": '), "")["Start"],
                        "End": row(L, w, lambda x: x.startswith(' "NotHolding": '), "")["Start"]},
                       json_member(L, "Selftest", "Selftest (autoprueba de las guardas)")]


def ranges_guards_selftest(L):
    w = {"Start": 1, "End": len(L)}
    return whole(L) + [json_member(L, "Candidate", "Candidate (vista previa de la candidata)")] + [
        row(L, w, (lambda k: lambda x: x.startswith(' "%s": ' % k))(k), "%s (autoprueba)" % k) for k in ("Killed", "MutantsTotal", "Result")]


def ranges_dispositions(L):
    w = {"Start": 1, "End": len(L)}
    return ranges_doc(L) + [row(L, w, (lambda o: lambda x: x.startswith("| **%s** |" % o))(o), "disposición propuesta de A62-A7-%s" % o)
                            for o in ("O%d" % i for i in range(1, 9))] + [
        para(L, w, "La comprobación FC-01", "nota de FC-01")]


def ranges_r1_output(L):
    return whole(L) + [json_member(L, k, "%s (revisión de la ronda 1)" % k) for k in (
        "RunId", "ReviewedBlob", "Verdict", "RequiredFindings", "OptionalFindings", "TopicVerdicts", "Materiality", "OwnerReservedConsequence",
        "RelatedImpacts", "QuestionAnswers")]


def ranges_r1_audit(L):
    return whole(L) + [json_member(L, "Accreditation", "Accreditation (auditoría de la ronda 1)"),
                       json_member(L, "Fidelity", "Fidelity (auditoría de la ronda 1)")]


def evidence_ranges(n):
    if n in (A7_RUN_RESULT, A7R2_RUN_RESULT):
        return ranges_guards_run
    if n == A7R2_SUMMARY or n == A7_DIR + "/summary.json":
        return ranges_summary
    if n == A7_DIR + "/p4-guards-selftest.json":
        return ranges_guards_selftest
    if n == A7R2_SELFTEST:
        return ranges_guards_selftest
    return ranges_doc if n.endswith(".md") else ranges_whole


# (ruta, por qué, alcance, función de rangos, etiqueta corta para el prompt)
canonical = [
    (V14, "Proposal V14 congelada: §18 (Owner Validation y decisiones del Owner: OV-I62-03, OD-5), §20.3 y §20.3.1 (cierre efectivo de insumos; "
          "L1079, L1098-L1099), B.9 (`rackcad-role-invocation/v1`: L2231-L2233; L2250-L2251), Anexo C (C-24, L2363), D.3 (donde A7-1 se añade, "
          "tras A6-8) y D.6; filas L1-L5 de A-7 §2", "SECTIONS", ranges_v14,
     "§18, §20.3, §20.3.1, B.9, Anexo C (C-24), D.3 y D.6"),
    (AP, "AUTOMATION_PLAN: 16.3 (AuthorityRevision, L490), 16.4, 16.5, 16.6 (RunId), 16.7 (reverificación), 16.8 (RED de la cadena: «exige RED», "
         "L616-L617, LECTURA DIRECTA OBLIGATORIA), 16.9 (comprobaciones, filas L630-L643), 16.13 (Resolve L819-L827; Validate(M) MV-1..MV-7 "
         "L902-L908; MAP_INVALID L910-L918), 16.14, 16.22 y 16.24 (cierre de insumos L1264-L1280; fidelidad; P-22..P-25); filas L6-L9 y L16 de A-7 "
         "§2", "SECTIONS", ranges_ap, "16.3, 16.4, 16.5, 16.6, 16.7, 16.8, 16.9, 16.13, 16.14, 16.22 y 16.24"),
    (RAE, "procedimiento de agent-execution: §10 (controles negativos, L219-L228: ruta propia y copias de las entradas; oráculo relativo; nc1) y §15 "
          "(invocación de rol, cierre; I6, L565-L568); filas L10 y L15 de A-7 §2", "SECTIONS", ranges_rae, "§5, §8, §10, §14 y §15"),
    (A6, "A-6 AGREED (decisiones §90; blob 4bc919dd): A6-2, reglas 1 y 3; A6-3, regla 6 (b); A6-5, regla 2; A6-7, reglas 1, 5 y 7; fin de A6-8 "
         "(L815-L816); A6-9, regla 4, y su tabla (§3.12); §5; las líneas del Owner de §7; filas L11-L14 y L17 de A-7 §2", "ENTIRE", ranges_a6, "entera"),
    (A5, "A-5 AGREED (A5-1, construcción de B1 conforme al mapa de cláusulas; A5-2)", "ENTIRE", ranges_a5, "entera"),
    (A4, "A-4 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A3, "A-3 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A2, "A-2 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (A1, "A-1 acordada (cabecera como orientación; archivo entero en el cierre)", "HEADER", ranges_header_only, "cabecera (orientación)"),
    (FREEZE, "registro del Freeze de I-62 (FREEZE_SHA y la Proposal V14 que A-7 cita en su cabecera)", "ENTIRE", ranges_doc, "entero"),
    (LIFECYCLE, "LIFECYCLE §3 (M-01..M-08; OWNER-RESERVED), §5 (REQUIRED/OPTIONAL y veredictos) y §6 (enmiendas)", "SECTIONS",
     ranges_lifecycle, "§3, §5 y §6"),
    (ADR46, "ADR-0046 aceptado (protocolo de ejecución delegada; #9 nivel A): M-08", "ENTIRE", ranges_adr(ADR46), "entero"),
    (ADR48, "ADR-0048 (roles, binding por capacidad, independencia por dimensiones; conserva #9, Level A): M-08", "ENTIRE", ranges_adr(ADR48), "entero"),
    (CC, "contratos de las invocaciones del Controller de FX-02 (negativos con invocación: mismo prompt y Target de la verificación real, L23; "
         "AuthorityRevision y CanonicalInputs del kit, L44-L45)", "ENTIRE", ranges_cc, "entero"),
    (CMAP, "mapa de cláusulas I62 (Files y Entries: MODIFIED, ADDED, ENTRY; el que leen Resolve y Validate en EFF)", "ENTIRE", ranges_whole, "entero"),
    (CMAP_SCHEMA, "esquema del mapa de cláusulas", "ENTIRE", ranges_whole, "entero"),
    (S_IC, "esquema rackcad-input-closure/v1 (el contrato cuyo significado persistido cambia según A-7 §5, M-02)", "ENTIRE", ranges_whole, "entero"),
    (S_RI, "esquema rackcad-role-invocation/v1 (Target, AuthorityRevision y CanonicalInputs de B.9)", "ENTIRE", ranges_whole, "entero"),
    (DECISIONS, "decisiones §62 (U-19), §73 (A-5 AGREED), §78 (determinación del Owner A6-ADR0046-9-FX02) y §90-§94 (A-6 AGREED; retención de "
                "A6-7 y determinación de I6; preparación de A-7, D-1 y lecturas; revisión de la ronda 1; §94: R1-R3 aceptados, segunda candidata y "
                "esta revisión, todavía sin autorizar)", "SECTIONS", ranges_decisions, "§62, §73, §78 y §90-§94"),
    (EVIDENCE, "evidencia de I-62 (registro de las sesiones que prepararon A-7 y su ronda 2: hechos, resúmenes y valoraciones; evidencia, no "
               "autoridad); §187-§191 como orientación, con riesgo de anclaje declarado", "SECTIONS", ranges_evidence, "§187-§191 (orientación)"),
]


def lines_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    if lines and lines[-1] == "":
        lines = lines[:-1]
    return lines


class NotUtf8(Exception):
    pass


def describe(b, path):
    """Un archivo que no es UTF-8 ya no detiene: se describe con surrogateescape, queda marcado INVALID_UTF8 por fidelity.scan y se lee por su
    copia fiel (decisiones §94, punto 5)."""
    try:
        b.decode("utf-8")
        charset = "UTF-8"
    except UnicodeDecodeError:
        charset = "NOT-UTF-8"
    t = b.decode("utf-8", "surrogateescape")
    return {"Bytes": len(b), "Lines": len(lines_of(t)), "Sha256": hashlib.sha256(b).hexdigest(),
            "LinesOver2000Chars": [i + 1 for i, x in enumerate(t.split("\n")) if len(x) > 2000],
            "Encoding": {"Charset": charset, "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF",
                         "FinalNewline": b.endswith(b"\n")}}, t


def dir_files(rev, d):
    names = [n for n in git("ls-tree", "-r", "--name-only", rev, "--", d + "/").decode("utf-8").split("\n") if n]
    assert names and all(n.startswith(d + "/") for n in names), "el commit no tiene %s/" % d
    for n in names:
        assert re.match(r"^[A-Za-z0-9_./-]+$", n), "ruta con caracteres fuera de [A-Za-z0-9_./-]: %s" % n
    return sorted(names)


def rec(p, rev, why, scope, rf, label):
    b = git("show", rev + ":" + p)
    BYTES[p] = b
    r, t = describe(b, p)
    blob = git("rev-parse", rev + ":" + p).decode().strip()
    r = dict({"Path": p, "Blob": blob}, **r)
    if p in PINNED:
        assert blob == PINNED[p], "%s: blob %s distinto del fijado (%s)" % (p, blob, PINNED[p])
        r["PinnedBlob"] = PINNED[p]
    if p == OBJECT:
        assert blob == PARAMS["Object"]["Blob"] == OBJECT_BLOB, "A-7: blob %s distinto del de kit-params.json o del fijado" % blob
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


def materiality_a7(L):
    s5 = section(L, "5. ")
    out = {}
    for m in MATERIALITY_IDS:
        cell = L[row(L, s5, (lambda m: lambda x: x.startswith("| %s |" % m))(m), m)["Start"] - 1].split("|")[2].strip()
        out[m] = {"**sí**": "yes", "no": "no"}[cell]
    return out


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


def sha256_manifest(rev, d):
    """MANIFEST.sha256 de `d` (líneas «<sha256>  <ruta relativa>»): cada ruta existe en el commit con ese SHA-256 y el MANIFEST lista todo `d` salvo a sí
    mismo (y, en I-62-A7/, salvo el resultado `run` de las guardas, custodiado después de la publicación)."""
    man = git("show", rev + ":" + d + "/MANIFEST.sha256").decode("utf-8").split("\n")
    listed = {}
    for l in man:
        if not l.strip():
            continue
        m = re.match(r"^([0-9a-f]{64})  (.+)$", l)
        assert m, "%s/MANIFEST.sha256: línea mal formada: %r" % (d, l[:80])
        listed[d + "/" + m.group(2)] = m.group(1)
    for p, h in listed.items():
        assert hashlib.sha256(git("show", rev + ":" + p)).hexdigest() == h, "%s: SHA-256 distinto del MANIFEST" % p
    rest = set(dir_files(rev, d)) - set(listed) - {d + "/MANIFEST.sha256"}
    return {"Dir": d, "Listed": len(listed), "NotListed": sorted(rest)}


def identity_checks():
    P = PARAMS
    pub, parent, grc = P["Publication"], P["PublicationParent"], P["GuardsRunCommit"]
    obj_blob = P["Object"]["Blob"]
    assert obj_blob == OBJECT_BLOB, "kit-params.json: Object.Blob distinto del fijado en owner_gate.py"
    assert (P.get("PreviousObject") or {}).get("Blob") == PREV_BLOB, "kit-params.json: PreviousObject distinto de la ronda 1 (6c29747d)"
    for c in (REV, pub, parent, grc):
        assert git_ok("cat-file", "-e", c + "^{commit}"), "commit ausente en el clon: %s" % c
    assert git("rev-parse", pub + "^").decode().strip() == parent, "el padre de la publicación no es %s" % parent[:8]
    assert git("rev-parse", parent + ":" + OBJECT).decode().strip() == PREV_BLOB, "el padre de la publicación no tiene la ronda 1 (6c29747d)"
    for c in (pub, grc, REV):
        assert git("rev-parse", c + ":" + OBJECT).decode().strip() == obj_blob, "blob de A-7 (ronda 2) distinto en %s" % c
    for c in (parent, pub, grc, REV):
        assert git("rev-parse", c + ":" + A6).decode().strip() == OG.A6_AGREED_BLOB, "A-6 distinta de la acordada en %s" % c
    for a, b in ((pub, grc), (grc, REV)):
        assert git_ok("merge-base", "--is-ancestor", a, b), "%s no es antecesor de %s" % (a, b)
    pub_files, now_files = dir_files(pub, A7R2_DIR), dir_files(REV, A7R2_DIR)
    unchanged = all(git("rev-parse", pub + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip() for p in pub_files)
    added = set(now_files) - set(pub_files)
    assert unchanged and added == {A7R2_RUN_RESULT} and set(P.get("A7EvidenceAddedAfterPublication") or []) == added, \
        "I-62-A7/r2/ cambió después de la publicación además de añadir el resultado `run` (añadidos: %s)" % sorted(added)
    r1_parent, r1_now = r1_evidence_files(parent), r1_evidence_files(REV)
    assert r1_parent == r1_now and all(git("rev-parse", parent + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip()
                                       for p in r1_now), "el preflight de la ronda 1 (I-62-A7/ fuera de r2/) cambió desde el padre de la publicación"
    trees = {}
    for d, (t, custody) in PINNED_TREES.items():
        got, at_custody = git("rev-parse", REV + ":" + d).decode().strip(), git("rev-parse", custody + ":" + d).decode().strip()
        assert got == t == at_custody, "árbol %s distinto del fijado o del del commit que lo custodió (%s)" % (d, custody[:8])
        trees[d] = {"Tree": t, "CustodyCommit": custody}
    old = git("cat-file", "-p", DECISIONS_PREFIX_BLOB)
    dec_b = git("show", REV + ":" + DECISIONS)
    assert dec_b.startswith(old), "decisiones del clon no crece solo por añadido desde la registrada con §93 (blob 1de169d0, commit 5158e4bc)"
    for c in (pub, grc, REV):
        assert git("rev-parse", c + ":" + DECISIONS).decode().strip() == DECISIONS_AT_PUBLICATION, "decisiones en %s distinta de la de §94" % c[:8]
    dec = dec_b.decode("utf-8").split("\n")
    numbered = [(i, int(m.group(1))) for i, m in ((i, OG.NUMBERED_RX.match(l)) for i, l in enumerate(dec)) if m]
    s_last = [i for i, n in numbered if n == LAST_SECTION]
    assert BASE_SECTION == OG.BASE_SECTION and len(s_last) == 1 and max(n for _, n in numbered) == LAST_SECTION, \
        "decisiones del clon: §94 no es la última sección numerada, una sola vez"
    s94_text = OG.section_text(dec, s_last[0])
    for tok in ("A62-A7-R1", "R2", "R3", "segunda candidata", "Sin revisión formal sin una disposición nueva del Coordinator"):
        assert tok in s94_text, "§94 no registra «%s»" % tok
    assert OG.authorization_pending(OG.pinned_authorization()), "owner_gate.py con una autorización fijada: este kit se entrega PENDIENTE"
    run_res = json.loads(git("show", REV + ":" + A7R2_RUN_RESULT).decode("utf-8"))
    assert run_res.get("Mode") == "run" and len(str(run_res.get("Base"))) >= 8 and parent.startswith(str(run_res.get("Base"))) and \
        len(str(run_res.get("Head"))) >= 8 and pub.startswith(str(run_res.get("Head"))) and run_res.get("A7Blob") == obj_blob and \
        run_res.get("Result") == "PASS", "el resultado `run` custodiado de la ronda 2 no es Mode run / Base = padre / Head = publicación / A7Blob = objeto / PASS"
    st = json.loads(git("show", REV + ":" + A7R2_SELFTEST).decode("utf-8"))
    assert st.get("Result") == "PASS" and st.get("Killed") == st.get("MutantsTotal") and st.get("MutantsTotal") and st.get("Base") == parent and \
        (st.get("Candidate") or {}).get("CandidateBlob") == obj_blob, "la autoprueba custodiada de las guardas de la ronda 2 no es PASS sobre el objeto"
    summ = json.loads(git("show", REV + ":" + A7R2_SUMMARY).decode("utf-8"))
    assert (summ.get("Candidate") or {}).get("Blob") == obj_blob and summ.get("Hold") == summ.get("Cases") and summ.get("Cases") and \
        summ.get("NotHolding") == [] and summ.get("Base") == parent and summ.get("PreviousRound") == PREV_BLOB, \
        "summary.json de la ronda 2 no es el de la candidata con todos los casos cumplidos"
    out1 = json.loads(git("show", REV + ":" + R1_OUTPUT).decode("utf-8"))
    aud1 = json.loads(git("show", REV + ":" + R1_AUDIT).decode("utf-8"))
    assert out1.get("RunId") == R1_RUN_ID and out1.get("ReviewedBlob") == PREV_BLOB and \
        [f.get("FindingId") for f in out1.get("RequiredFindings") or []] == ROUND1_FINDINGS and \
        [f.get("FindingId") for f in out1.get("OptionalFindings") or []] == ROUND1_OPTIONALS and out1.get("Verdict") == "CHANGES REQUIRED" and \
        (aud1.get("Accreditation") or {}).get("Status") == "NOT_ACCREDITED", "la revisión de la ronda 1 custodiada no es la de R1-R3 y O1-O8"
    a7 = git("show", REV + ":" + OBJECT).decode("utf-8")
    a6 = git("show", REV + ":" + A6).decode("utf-8").split("\n")
    assert sum(1 for l in a6 if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "A-6 acordada sin la línea (a) fijada, una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "decisiones del clon sin la línea (a) del Owner (§78), una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.CLAUDE_LINE_SHA) == 1, "decisiones sin la línea CLAUDE-CLI-I62 = A fijada"
    header = a7.split("```")[1]
    header_claims = {}
    for p in (A1, A2, A3, A4, A5, A6, FREEZE, V14):
        b = git("rev-parse", REV + ":" + p).decode().strip()
        header_claims[p] = OG.token_in(header, b, "0-9a-f")
        assert header_claims[p], "la cabecera de A-7 no cita el blob %s de %s" % (b[:8], p)
    assert OG.token_in(header, PREV_BLOB, "0-9a-f"), "la cabecera de A-7 (Previous round) no cita el blob completo de la ronda 1"
    manifests = [sha256_manifest(REV, d) for d in (A7R2_DIR, S91, S92)]
    assert manifests[0]["NotListed"] == [A7R2_RUN_RESULT] and not manifests[1]["NotListed"] and not manifests[2]["NotListed"], \
        "MANIFEST.sha256 incompletos: %s" % [(m["Dir"], m["NotListed"]) for m in manifests]
    r1man = sha256_manifest(REV, A7_DIR)
    assert [p for p in r1man["NotListed"] if not p.startswith((A7R2_DIR + "/", A7_DIR + "/r2-dispositions/"))] == [A7_RUN_RESULT], \
        "MANIFEST.sha256 de la ronda 1 incompleto: %s" % r1man["NotListed"][:5]
    readable = readable_manifest(REV, R4_READABLE)
    assert {e["Canonical"] for e in readable} >= set(R4_CITED), "r4/readable/ sin copia legible de algún archivo citado por el preflight de A-7"
    changed = [x for x in git("diff", "--name-status", parent, REV).decode("utf-8").split("\n") if x]
    return {"CloneCommit": REV, "Publication": pub, "PublicationParent": parent, "GuardsRunCommit": grc,
            "Round1AtPublicationParent": PREV_BLOB, "ObjectBlobAtPublicationGuardsRunAndClone": obj_blob,
            "A6AgreedBlobAtParentPublicationGuardsRunAndClone": OG.A6_AGREED_BLOB,
            "R2EvidenceUnchangedSincePublicationExceptRunResult": True, "A7EvidenceAddedAfterPublication": sorted(added),
            "R1EvidenceUnchangedSinceParent": True, "R1EvidenceFiles": len(r1_now),
            "A7R2EvidenceTree": git("rev-parse", REV + ":" + A7R2_DIR).decode().strip(), "PinnedTrees": trees,
            "DecisionsAppendOnlySinceSection93Record": True, "DecisionsAtPublicationGuardsRunAndClone": DECISIONS_AT_PUBLICATION,
            "Decisions94": {"Line": s_last[0] + 1, "Heading": dec[s_last[0]], "TextSha256": hashlib.sha256(s94_text.encode("utf-8")).hexdigest(),
                            "LastNumberedSection": True},
            "AuthorizationPending": True,
            "GuardsRunResult": {"Mode": "run", "Base": run_res.get("Base"), "Head": run_res.get("Head"), "A7Blob": obj_blob, "Result": "PASS"},
            "GuardsSelfTest": {"Result": "PASS", "Base": parent, "CandidateBlob": obj_blob, "MutantsTotal": st.get("MutantsTotal"),
                               "Killed": st.get("Killed")},
            "PreflightSummary": {"CandidateBlob": obj_blob, "Hold": summ.get("Hold"), "Cases": summ.get("Cases"), "PreviousRound": PREV_BLOB},
            "Round1Review": {"RunId": R1_RUN_ID, "ReviewedBlob": PREV_BLOB, "RequiredFindings": ROUND1_FINDINGS, "OptionalFindings": ROUND1_OPTIONALS,
                             "Verdict": "CHANGES REQUIRED", "Accreditation": "NOT_ACCREDITED"},
            "OwnerLineOnceInA6AndInDecisions78": True, "ClaudeCliLinePinnedOnce": True, "A7HeaderCitesBlobs": header_claims,
            "A7HeaderCitesRound1Blob": True, "Sha256Manifests": manifests + [r1man], "ReadableCopies": readable, "ChangedSincePublicationParent": changed}


def fmt_n(n):
    return "{:,}".format(n).replace(",", " ")


def range_text(g):
    return ("%d" % g["Start"]) if g["Start"] == g["End"] else ("%d-%d" % (g["Start"], g["End"]))


def short_label(path, s):
    m = re.match(r"^#+ (.*)$", s)
    if not m:
        return re.sub(r" \(L\d+(?:-L\d+)?\)$", "", s)
    t = m.group(1)
    for rx, fmt in ((r"^(\d+(?:\.\d+)+) ", "%s" if path == AP else "§%s"), (r"^(\d+)\. ", "§%s"), (r"^(D\.\d+) ", "%s"), (r"^(B\.\d+) ", "%s"),
                    (r"^(Anexo [A-Z]) ", "%s")):
        mm = re.match(rx, t)
        if mm:
            return fmt % mm.group(1)
    return t.split(" — ")[0][:40]


def prompt_entry(k, r):
    rs = [g for g in r["LineRanges"] if g["Section"] not in ("entero",) and not g.get("Span") and not g.get("A7Lit")]
    head = "%d. `%s` (%s bytes, %s líneas; blob `%s`)" % (k, r["Path"], fmt_n(r["Bytes"]), fmt_n(r["Lines"]), r["Blob"][:8])
    body = r["PromptLabel"]
    if rs:
        body += ": " + "; ".join("%s %s" % (short_label(r["Path"], g["Section"]), range_text(g)) for g in rs)
    fc = (r.get("Fidelity") or {}).get("FaithfulCopy")
    return head + ", " + body + "." + (" **Léelo solo en su copia fiel** `%s` (run; misma numeración)." % fc if fc else "")


def readable_selection(rev, rdir, canonicals):
    """MANIFEST.json de `rdir` y las copias legibles cuyo canónico está en `canonicals` (el MANIFEST se comprueba entero en identity_checks)."""
    man = json.loads(git("show", rev + ":" + rdir + "/MANIFEST.json").decode("utf-8"))
    return [rdir + "/MANIFEST.json"] + sorted(e["Readable"] for e in man["Files"] if e["Canonical"] in canonicals)


def r1_evidence_files(rev):
    """Preflight de la ronda 1: docs/automation/evidence/I-62-A7/ salvo r2/ y r2-dispositions/ (sin cambio desde la ronda 1)."""
    return [n for n in dir_files(rev, A7_DIR) if not n.startswith((A7R2_DIR + "/", A7_DIR + "/r2-dispositions/"))]


def group_items(rev):
    a7r2 = [(n, "preflight dirigido y guardas de la ronda 2 (q0-q7: TRX de CurrentSha y RedSha y exigencia de RED de 16.8, RelayFacts, N4 y nc1, "
                "tipo B1, identidad del invalidador, compatibilidad con A-5 y A-6, guardas G1-G7 y su autoprueba; constructor, validador, formato "
                "de designación a7-designation/2 y derivación del aceptante; resultado `run`): %s" % n[len(A7R2_DIR) + 1:], "ENTIRE",
             evidence_ranges(n), "preflight de la ronda 2, entero") for n in dir_files(rev, A7R2_DIR)]
    disp = [(A7R2_DISP, "disposiciones propuestas de A62-A7-O1..O8 y nota de FC-01 (decisiones §94, punto 4; propuestas del Principal, no deciden)",
             "ENTIRE", ranges_dispositions, "disposiciones propuestas de O1-O8")]
    a7 = [(n, "preflight y guardas de la ronda 1 (reutilizados por identidad en la ronda 2: reproducción sobre R7-A, negativos de (a), (b) y (d), "
              "compatibilidad, reconstrucción estática, guardas sin cambio): %s" % n[len(A7_DIR) + 1:], "ENTIRE", evidence_ranges(n),
           "preflight de la ronda 1, entero") for n in r1_evidence_files(rev)]
    r1rev = [(R1_OUTPUT, "salida estructurada de la revisión de la ronda 1 (R20261019T010200Z-4a72): A62-A7-R1..R3 y O1..O8, temas, materialidad, "
                         "impactos relacionados y preguntas", "ENTIRE", ranges_r1_output, "revisión de la ronda 1: output.json"),
             (R1_AUDIT, "auditoría de la revisión de la ronda 1 (NOT_ACCREDITED por la lectura DEGRADED de un TRX con BOM)", "ENTIRE", ranges_r1_audit,
              "revisión de la ronda 1: audit.json"),
             (R1_README, "README de la custodia de la revisión de la ronda 1", "ENTIRE", ranges_doc, "revisión de la ronda 1: README")]
    m7_names = dir_files(rev, S91) + dir_files(rev, S92) + [BENCH_README, BENCH_P1, BENCH_N4, BENCH_NC1] + dir_files(rev, BENCH_DESIG) + [READINGS_MD]
    m7 = [(n, "evidencia de la medición de A6-7 (m7/; orientación: D-1 y las propuestas FC no se revisan otra vez, solo se menciona si R1-R3 las "
              "cambian): %s" % n[len(M7) + 1:], "ENTIRE", ranges_doc if n.endswith(".md") else ranges_whole,
           "evidencia de m7 (selección)") for n in m7_names]
    r4_names = list(R4_CITED) + readable_selection(rev, R4_READABLE, set(R4_CITED))
    r4 = [(n, "evidencia de la ronda 4 de A-6 que cita el preflight de A-7 (§5: negativos reutilizados por identidad) y su copia legible: %s"
           % n[len(R4_DIR) + 1:], "ENTIRE", ranges_whole, "evidencia de A-6 ronda 4 (selección)") for n in r4_names]
    km = [(n, "kit de la medición a6m7-measure (congelado, sin lanzar; A-7 §10): %s" % n[len(KITM7) + 1:], "ENTIRE",
           ranges_doc if n.endswith(".md") else ranges_whole, "kit a6m7-measure, entero") for n in dir_files(rev, KITM7)]
    pk = [(n, "paquete B-min, %s" % n[len(PKG_DIR) + 1:], "ENTIRE", ranges_doc if n.endswith(".md") else ranges_whole, "paquete B-min, entero")
          for n in dir_files(rev, PKG_DIR)]
    return [(OBJECT, "objeto: la enmienda A-7, ronda 2 (PROPUESTA), entera", "ENTIRE", ranges_a7, "objeto, entero")] + a7r2 + disp + a7 + r1rev + \
        m7 + r4 + km + pk


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
    assert EVIDENCE in texts, "la evidencia I-62-evidence.md entra en el cierre (A-7 cita la evidencia §181)"
    assert not any(p.startswith(C1C3_DIR + "/") for p in texts), "C1-C3 no entra en el cierre"
    assert not any("I-65" in p for p in texts), "ningún material de I-65 entra en el cierre"
    missing = [p for p in A7LIT if p not in texts]
    assert not missing, "fuentes de A-7 §2 fuera del cierre: %s" % missing
    return can, texts, Ls


def table(rows):
    w = max(len(r["Path"]) for r in rows)
    fc = lambda r: (r.get("Fidelity") or {}).get("FaithfulCopy")
    return "\n".join("%-*s  blob %s  %s bytes  %s líneas%s" % (w, r["Path"], r["Blob"], fmt_n(r["Bytes"]), fmt_n(r["Lines"]),
                                                             ("  → léelo en la copia fiel del run " + fc(r)) if fc(r) else "") for r in rows)


def lit_checklist(lits, by):
    out = []
    for lid in LIT_IDS:
        x = lits[lid]
        rs = ", ".join(("L%d" % a) if a == b else ("L%d-L%d" % (a, b)) for a, b in x["Ranges"])
        out.append("- %s (A-7 §2, línea %d): `%s` (%s; blob `%s`): %s" % (lid, x["Line"], x["Path"], x["Source"], by[x["Path"]]["Blob"][:8], rs))
    for p, a, b, tok, label in EXTRA_A7_REFS:
        out.append("- %s: `%s` (blob `%s`): L%d-L%d" % (label.split(":")[0], p, by[p]["Blob"][:8], a, b))
    out.append("- determinación de §91 que A-7 cita como origen: `%s` (blob `%s`), entera" % (S91_DET, by[S91_DET]["Blob"][:8]))
    return "\n".join(out)


def readable_of(can_paths, readable):
    return {e["Canonical"]: e["Readable"] for e in readable if e["Canonical"] in can_paths}


def faithful_copies(can):
    """Detector y copias fieles (decisiones §94, punto 5). → (informe del detector por ruta, [(nombre de la copia, bytes)], FaithfulCopies).
    STOP si la copia de un archivo marcado no decodifica en sus bytes exactos, no es canónica o no es legible fielmente."""
    fid, copies, entries, names = {}, [], [], set(n.lower() for n in RUN_FILES)
    for r in can:
        p = r["Path"]
        info = FID.scan(p, BYTES[p])
        assert info["Blob"] == r["Blob"] and info["Lines"] == r["Lines"], "detector: blob o líneas distintos de los del registro (%s)" % p
        fid[p] = info
        if not info["Flagged"]:
            continue
        name = FID.copy_name(p)
        assert name.lower() not in names, "nombre de copia repetido: %s" % name
        names.add(name.lower())
        cb = FID.encode(p, BYTES[p])
        if FID.verify(p, BYTES[p], cb) or FID.scan(name, cb)["Flagged"]:
            raise SystemExit("STOP: la copia fiel de %s no entrega sus bytes exactos o no es legible fielmente" % p)
        lay = FID.layout(cb)
        cl = cb.decode("utf-8").split("\n")[:-1]
        cont = sorted(n for n, ls in lay.items() if len(ls) > 1)
        extra = sum(len(lay[n]) - 1 for n in cont)
        entries.append({"Original": p, "OriginalBlob": r["Blob"], "OriginalSha256": r["Sha256"], "OriginalBytes": r["Bytes"],
                        "OriginalLines": info["Lines"], "Flags": info["Flags"], "Copy": name, "CopySha256": FID.sha256(cb), "CopyBytes": len(cb),
                        "CopyLines": len(cl), "CopyMaxLineChars": max(len(x) for x in cl), "ContinuedOriginalLines": cont,
                        "AppendixLines": [info["Lines"] + 2, info["Lines"] + 1 + extra] if extra else None, "Format": FID.FORMAT})
        copies.append((name, cb))
    return fid, copies, entries


def marks_list(flags):
    def lines(v):
        if v == [0]:
            return "el archivo entero"
        s = ", ".join(str(x) for x in v[:12]) + ((" y %d más" % (len(v) - 12)) if len(v) > 12 else "")
        return ("línea " if len(v) == 1 else "líneas ") + s
    return "; ".join("%s: %s" % (k, lines(v)) for k, v in flags.items())


def read_path(p, flagged):
    """Ruta absoluta que el revisor lee con Read: la copia fiel en el run si el archivo está marcado; si no, el archivo en el clon."""
    return (RUN + "\\" + flagged[p]["Copy"]) if p in flagged else (CLONE + "\\" + p.replace("/", "\\"))


def order_points(run_lines):
    """Líneas de los puntos 1-9 de order.txt (el cuerpo pegado de §93: «<n>. <título>» al principio de línea)."""
    starts = []
    for n in range(1, 10):
        hits = [i + 1 for i, l in enumerate(run_lines) if re.match(r"^%d\. [A-ZÁÉÍÓÚ]" % n, l)]
        assert len(hits) == 1, "order.txt: punto %d (%s)" % (n, hits)
        starts.append(hits[0])
    out = {}
    for k, s in enumerate(starts):
        e = (starts[k + 1] - 1) if k + 1 < len(starts) else len(run_lines)
        while e > s and run_lines[e - 1].strip() == "":
            e -= 1
        out[k + 1] = {"Start": s, "End": e}
    return out


def render_prompt(can, Ls, run, ident, lits, fc_entries):
    tpl = open(os.path.join(KIT, "prompt.template.md"), encoding="utf-8").read()
    by = {r["Path"]: r for r in can}
    flagged = {e["Original"]: e for e in fc_entries}
    faithful = "\n".join("- `%s` (%s) → `%s\\%s` (%s líneas%s)" % (
        e["Original"], marks_list(e["Flags"]), RUN, e["Copy"], fmt_n(e["CopyLines"]),
        ("; las continuaciones de %s están en el apéndice, líneas %d-%d de la copia" % (
            ("la línea %d" % e["ContinuedOriginalLines"][0]) if len(e["ContinuedOriginalLines"]) == 1 else
            ("las líneas %s" % ", ".join(str(x) for x in e["ContinuedOriginalLines"][:12]) +
             ((" y %d más" % (len(e["ContinuedOriginalLines"]) - 12)) if len(e["ContinuedOriginalLines"]) > 12 else "")),
            e["AppendixLines"][0], e["AppendixLines"][1])) if e["AppendixLines"] else "") for e in fc_entries)
    g = {p: {x["Section"]: x for x in by[p]["LineRanges"]} for p in by}
    dec, a7, ga, a6 = g[DECISIONS], by[OBJECT], g[OBJECT], g[A6]
    a7_map = "; ".join("%s %s" % ((x["Section"] if x["Section"] in ("cabecera",) else
                                   re.sub(r"^#+ ", "", x["Section"]).split(" — ")[0].split(" (")[0]), range_text(x))
                       for x in a7["LineRanges"] if x["Section"] != "entero" and (x["Section"] == "cabecera" or x["Section"].startswith("#")))
    large = [r["Path"] for r in can if r["Bytes"] > LARGE and r["Path"] not in flagged] + \
            [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE] + [e["Copy"] for e in fc_entries if e["CopyBytes"] > LARGE]
    grouped_prefixes = (A7_DIR + "/", M7 + "/", R4_DIR + "/", KITM7 + "/", PKG_DIR + "/", R1_REVIEW + "/")
    entries = [prompt_entry(k + 1, r) for k, r in enumerate([r for r in can if r["Path"] != OBJECT and not r["Path"].startswith(grouped_prefixes)])]
    sec = lambda p: range_text([x for x in a7["LineRanges"] if x["Section"].startswith("### " + p + " ") or x["Section"].startswith("## " + p + " ")][0])
    d = lambda n: [x for k, x in dec.items() if k.startswith("## %d. " % n)][0]
    pts = order_points(run["order.txt"]["_Lines"])
    grr2, sm2, r1o, r1a, dsp = g[A7R2_RUN_RESULT], g[A7R2_SUMMARY], g[R1_OUTPUT], g[R1_AUDIT], g[A7R2_DISP]
    apd = DIRECT_READ
    rule = lambda dl, n: range_text(a6["%s, regla %d" % (dl, n)])
    r1_members = ("RunId", "ReviewedBlob", "Verdict", "RequiredFindings", "OptionalFindings", "TopicVerdicts", "Materiality",
                  "OwnerReservedConsequence", "RelatedImpacts", "QuestionAnswers")
    r1m = lambda k: range_text(r1o["%s (revisión de la ronda 1)" % k])
    r1_files = [p for p in by if p.startswith(A7_DIR + "/") and not p.startswith((A7R2_DIR + "/", A7_DIR + "/r2-dispositions/"))]
    rep = {
        "{{RUN_ID}}": IDS["RunId"], "{{INVOCATION_ID}}": IDS["InvocationId"], "{{LOGICAL_ID}}": IDS["LogicalReviewRequestId"],
        "{{CLONE}}": CLONE, "{{RUN}}": RUN,
        "{{CLONE_COMMIT}}": REV, "{{PUBLICATION}}": PARAMS["Publication"], "{{PUB_PARENT}}": PARAMS["PublicationParent"],
        "{{PUBLICATION8}}": PARAMS["Publication"][:8], "{{PUB_PARENT8}}": PARAMS["PublicationParent"][:8],
        "{{OBJECT_BLOB}}": a7["Blob"], "{{OBJECT_BLOB8}}": a7["Blob"][:8],
        "{{OBJECT_BYTES}}": fmt_n(a7["Bytes"]), "{{OBJECT_LINES}}": fmt_n(a7["Lines"]),
        "{{A7R1_BYTES}}": fmt_n(run["a7-r1.md"]["Bytes"]), "{{A7R1_LINES}}": fmt_n(run["a7-r1.md"]["Lines"]),
        "{{ORDER_LINES}}": str(run["order.txt"]["Lines"]),
        "{{ORDER_P1}}": range_text(pts[1]), "{{ORDER_P2}}": range_text(pts[2]), "{{ORDER_P3}}": range_text(pts[3]), "{{ORDER_P4}}": range_text(pts[4]),
        "{{ORDER_P5}}": range_text(pts[5]), "{{ORDER_P6}}": range_text(pts[6]), "{{ORDER_P7}}": range_text(pts[7]), "{{ORDER_P8}}": range_text(pts[8]),
        "{{ORDER_P9}}": range_text(pts[9]),
        "{{A7_MAP}}": a7_map,
        "{{A7_RULE}}": range_text(ga["§3.2, A7-1 (texto de la regla)"]),
        "{{A7_POINTS}}": "; ".join("%d L%s" % (k, range_text(ga["A7-1, punto %d" % k])) for k in range(1, 9)),
        "{{A7_CATS}}": "; ".join("(%s) L%s" % (c, range_text(ga["A7-1, punto 1 (%s)" % c])) for c in "abcd"),
        "{{A7_S33_T}}": range_text(ga["§3.3, tabla de categorías"]), "{{A7_S34}}": sec("3.4"), "{{A7_S35}}": sec("3.5"),
        "{{A7_S35_RES}}": range_text(ga["§3.5, residuo declarado (clausemap)"]), "{{A7_S36}}": sec("3.6"),
        "{{A7_S36_ROWS}}": "; ".join("%s L%s" % (k, range_text(ga["§3.6, fila %s" % k])) for k in ("R1", "R2", "R3", "O1", "O7")) +
                           "; O2, O3-O6, O8 L%s" % range_text(ga["§3.6, fila O2, O3-O6, O8 |"]),
        "{{A7_S5}}": sec("5."), "{{A7_S7}}": sec("7."), "{{A7_S8}}": sec("8."),
        "{{A7_QR2}}": "%s-%s" % (ga["§8, Q-A7R2-01"]["Start"], ga["§8, Q-A7R2-03"]["End"]),
        "{{DEC94}}": range_text(d(94)),
        "{{DEC94_SOURCE}}": range_text(dec["§94, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC94_P1}}": range_text(dec["§94, punto 1"]), "{{DEC94_P2}}": range_text(dec["§94, punto 2"]), "{{DEC94_P4}}": range_text(dec["§94, punto 4"]),
        "{{DEC94_P6}}": range_text(dec["§94, punto 6"]), "{{DEC94_P7}}": range_text(dec["§94, punto 7"]), "{{DEC94_P9}}": range_text(dec["§94, punto 9"]),
        "{{DEC78_LINE}}": range_text(dec["§78, línea del Owner A6-ADR0046-9-FX02"]),
        "{{GRR2}}": A7R2_RUN_RESULT.replace("/", "\\"), "{{GRR2_LINES}}": str(by[A7R2_RUN_RESULT]["Lines"]),
        "{{GRR2_KEYS}}": "; ".join("%s L%s" % (k, range_text(grr2["%s (resultado `run`)" % k])) for k in ("A7Blob", "Base", "Head", "Mode", "Result")),
        "{{SUMMARY2}}": A7R2_SUMMARY.replace("/", "\\"), "{{SUMMARY2_CAND}}": range_text(sm2["Candidate (blob de la candidata)"]),
        "{{SUMMARY2_HOLD}}": range_text(sm2["Hold, Cases y NotHolding"]), "{{SUMMARY2_ST}}": range_text(sm2["Selftest (autoprueba de las guardas)"]),
        "{{R1OUT}}": R1_OUTPUT.replace("/", "\\"),
        "{{R1OUT_KEYS}}": "; ".join("%s L%s" % (k, r1m(k)) for k in ("RunId", "ReviewedBlob", "RequiredFindings")),
        "{{R1OUT_RANGES}}": "; ".join("%s L%s" % (k, r1m(k)) for k in r1_members),
        "{{R1OUT_REQ}}": "`RequiredFindings`, L%s" % r1m("RequiredFindings"),
        "{{R1OUT_TOPICS}}": "`TopicVerdicts`, L%s" % r1m("TopicVerdicts"),
        "{{R1AUDIT_RANGES}}": "`Accreditation` L%s; `Fidelity` L%s" % (range_text(r1a["Accreditation (auditoría de la ronda 1)"]),
                                                                       range_text(r1a["Fidelity (auditoría de la ronda 1)"])),
        "{{DISP}}": A7R2_DISP, "{{DISP_ROWS}}": "; ".join("O%d L%s" % (i, range_text(dsp["disposición propuesta de A62-A7-O%d" % i])) for i in range(1, 9)) +
                                               "; nota de FC-01 L%s" % range_text(dsp["nota de FC-01"]),
        "{{A6_LINE_A}}": range_text(a6["§7, línea del Owner (a) A6-ADR0046-9-FX02"]),
        "{{A6_LINE_B}}": range_text(a6["§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO"]),
        "{{A6_A62_R1}}": rule("A6-2", 1), "{{A6_A62_R3}}": rule("A6-2", 3), "{{A6_A63_R6B}}": range_text(a6["A6-3, regla 6 (b)"]),
        "{{A6_A65_R2}}": rule("A6-5", 2), "{{A6_A67_R1}}": rule("A6-7", 1), "{{A6_A67_R5}}": rule("A6-7", 5), "{{A6_A67_R7}}": rule("A6-7", 7),
        "{{A6_A69_R4}}": rule("A6-9", 4),
        "{{A6_DERIV}}": range_text(a6["§3.12, tabla de derivación del cierre del aceptante (A6-9, regla 4)"]),
        "{{A7R2EVID_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(A7R2_DIR + "/")]),
        "{{A7R2EVID_COUNT}}": str(sum(1 for p in by if p.startswith(A7R2_DIR + "/"))),
        "{{A7EVID_TABLE}}": table([by[p] for p in sorted(r1_files)]), "{{A7EVID_COUNT}}": str(len(r1_files)),
        "{{R1REV_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R1_REVIEW + "/")]),
        "{{M7_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(M7 + "/")]),
        "{{M7_COUNT}}": str(sum(1 for p in by if p.startswith(M7 + "/"))),
        "{{R4_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R4_DIR + "/")]),
        "{{KITM7_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(KITM7 + "/")]),
        "{{KITM7_COUNT}}": str(sum(1 for p in by if p.startswith(KITM7 + "/"))),
        "{{PKG_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(PKG_DIR + "/")]),
        "{{PKG_COUNT}}": str(sum(1 for p in by if p.startswith(PKG_DIR + "/"))),
        "{{ACCEPTOR}}": ACCEPTOR, "{{ACCEPTOR_LINES}}": fmt_n(by[ACCEPTOR]["Lines"]), "{{TOOL}}": TOOL, "{{TOOL_LINES}}": fmt_n(by[TOOL]["Lines"]),
        "{{AP_DIRECT}}": "%d-%d" % (apd["Start"], apd["End"]),
        "{{AP_DIRECT_OFFSET}}": str(apd["Start"] - 8), "{{AP_DIRECT_LIMIT}}": str(apd["End"] - apd["Start"] + 17),
        "{{LIT_CHECKLIST}}": lit_checklist(lits, by),
        "{{EVIDENCE_RANGES}}": ", ".join("§%s" % re.sub(r"^## (\d+)\..*$", r"\1", x["Section"]) for x in by[EVIDENCE]["LineRanges"]
                                         if x["Section"].startswith("## ")),
        "{{CLOSURE_LIST}}": "\n".join(entries),
        "{{LARGE_FILES}}": ", ".join("`%s`" % p for p in large),
        "{{FAITHFUL_COPIES}}": faithful if faithful else "(ninguna: ningún archivo del cierre está marcado)",
        "{{FAITHFUL_COUNT}}": str(len(fc_entries)),
        "{{READ_AP}}": read_path(AP, flagged), "{{READ_DEC}}": read_path(DECISIONS, flagged), "{{READ_A7}}": read_path(OBJECT, flagged),
        "{{READ_GRR2}}": read_path(A7R2_RUN_RESULT, flagged), "{{READ_SUMMARY2}}": read_path(A7R2_SUMMARY, flagged),
        "{{READ_R1OUT}}": read_path(R1_OUTPUT, flagged),
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
    global CLONE, RUN, REPO, REV, OUT, PARAMS, A7LIT
    CLONE, RUN, REPO, REV, OUT, PARAMS = repo, run_dir, repo, rev, out_dir, params
    head = git("rev-parse", "HEAD").decode().strip()
    assert head == REV, "HEAD del clon (%s) distinto de CloneCommit (%s)" % (head, REV)
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    a7_lines = lines_of(git("show", REV + ":" + OBJECT).decode("utf-8"))
    lits = parse_literals(a7_lines)
    A7LIT = {}
    for lid in LIT_IDS:
        for a, b in lits[lid]["Ranges"]:
            A7LIT.setdefault(lits[lid]["Path"], []).append((lid, a, b))
    ident = identity_checks()
    items = group_items(REV) + canonical
    can, texts, Ls = build(items)
    for p, entries in A7LIT.items():
        for lid, a, b in entries:
            assert 1 <= a <= b <= len(Ls[p]), "A-7 §2, %s: L%d-L%d fuera de %s" % (lid, a, b, p)
    assert order_text_check(Ls[DECISIONS]), "decisiones §93 no declara el SHA-256 y los bytes del cuerpo pegado"
    mat = materiality_a7(Ls[OBJECT])
    assert mat == MATERIALITY_A7_EXPECTED, "A-7 §5 (Activado) distinto de lo esperado: %s" % mat
    fid, copies, fc_entries = faithful_copies(can)
    for r in can:
        r["Fidelity"] = {"Flags": fid[r["Path"]]["Flags"], "FaithfulCopy": FID.copy_name(r["Path"]) if fid[r["Path"]]["Flagged"] else None}
    run_files = RUN_FILES + [n for n, _ in copies]
    if mode == "copies":
        stale = [n for n in os.listdir(out_dir) if n.startswith(FID.COPY_PREFIX) and n not in dict(copies)]
        for n in stale:
            os.remove(os.path.join(out_dir, n))
        for n, cb in copies:
            with open(os.path.join(out_dir, n), "wb") as f:
                f.write(cb)
        return {"Written": [{"Copy": n, "Original": e["Original"], "Bytes": len(cb), "Sha256": FID.sha256(cb), "Flags": e["Flags"]}
                            for (n, cb), e in zip(copies, fc_entries)], "Removed": stale}
    rd = run_files_described(run_dir, out_dir)
    run = {n: rd[n][0] for n in rd}
    assert run["order.txt"]["Sha256"] == ORDER_TEXT[1] and run["order.txt"]["Bytes"] == ORDER_TEXT[2], "order.txt distinto del cuerpo §94 custodiado"
    assert run["a7-r1.md"]["GitBlob"] == PREV_BLOB, "a7-r1.md no es el blob 6c29747d de la ronda 1"
    for n in PREMISE_RUN_FILES:
        pb = open(os.path.join(run_dir if os.path.isfile(os.path.join(run_dir, n)) else out_dir, n), "rb").read()
        if FID.scan(n, pb)["Flagged"]:
            raise SystemExit("STOP: el archivo premisa del run %s no es legible fielmente por Read: %s" % (n, FID.scan(n, pb)["Flags"]))
    run["order.txt"]["_Lines"] = lines_of(rd["order.txt"][1])
    prompt = render_prompt(can, Ls, run, ident, lits, fc_entries)
    run["order.txt"].pop("_Lines")
    if mode == "render":
        return prompt
    assert open(os.path.join(out_dir, "prompt.md"), "rb").read() == prompt.encode("utf-8"), "prompt.md distinto del renderizado: --render-prompt"
    listing = sorted(os.listdir(run_dir))
    assert listing == sorted(run_files), "el run debe tener exactamente %s (tiene %s)" % (run_files, listing)
    corpus = set()
    for r in can:
        corpus |= {c for c in texts[r["Path"]] if ord(c) > 127}
    for n in run_files:
        b = open(os.path.join(run_dir, n), "rb").read()
        assert b == open(os.path.join(out_dir, n), "rb").read(), "%s del run distinto de la copia del kit" % n
        if n in PREMISE_RUN_FILES:
            corpus |= {c for c in rd[n][1] if ord(c) > 127}
    for n, cb in copies:
        assert open(os.path.join(run_dir, n), "rb").read() == cb, "copia fiel del run distinta de la generada desde el clon: %s" % n
        run[n] = describe(cb, n)[0]
        assert not FID.scan(n, cb)["Flagged"] and run[n]["Encoding"]["Charset"] == "UTF-8", "copia fiel no legible: %s" % n
    pb = open(os.path.join(run_dir, "prompt.md"), "rb").read()
    assert not pb.endswith(b"\n") and b"\r" not in pb, "prompt.md con salto final o CR"
    run["prompt.md"] = describe(pb, "prompt.md")[0]
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §94 del Coordinator (3 522 bytes; SHA-256 515ccfa3…, el que declara decisiones §94 "
                            "en el commit): acepta R1-R3 (punto 1), autoriza la segunda candidata con cambios limitados a R1-R3 (punto 2), las pruebas "
                            "(punto 3), los opcionales (punto 4), la fidelidad del kit (punto 5), un kit de revisión limitado a R1-R3 sin revisión "
                            "formal sin una disposición nueva (punto 6), D-1 y las lecturas (punto 7) y los límites (punto 8)",
               "a7-r1.md": "texto de A-7 de la ronda 1 (blob 6c29747d, commit 6fee8fd6), byte a byte: lo revisó R20261019T010200Z-4a72 y sus hallazgos lo "
                           "citan; la pureza del delta de la ronda 2 se juzga frente a él",
               "prompt.md": "kit/prompt.md renderizado desde kit/prompt.template.md; stdin literal del proceso claude-cli; nunca es premisa"}
    for e in fc_entries:
        sources[e["Copy"]] = ("copia fiel %s de %s (blob %s, SHA-256 %s…; marcas: %s): el revisor la lee EN LUGAR del original; nunca es premisa ni "
                              "referencia (se cita el original con el número de línea que muestra la copia)"
                              % (FID.FORMAT, e["Original"], e["OriginalBlob"][:8], e["OriginalSha256"][:12], marks_list(e["Flags"])))
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in run_files}
    run_hashes["order.txt"]["IdentityCheck"] = {"DecisionsSection": "§" + ORDER_TEXT[0], "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": ORDER_TEXT[2]}
    run_hashes["a7-r1.md"]["GitBlob"] = PREV_BLOB
    rmap = readable_of({r["Path"] for r in can}, ident["ReadableCopies"])
    fmap = {e["Original"]: e["Copy"] for e in fc_entries}
    reported = {"NonUtf8": [r["Path"] for r in can if "INVALID_UTF8" in r["Fidelity"]["Flags"]],
                "LinesOver2000Chars": [{"Path": r["Path"], "Lines": r["LinesOver2000Chars"], "ReadableCopy": rmap.get(r["Path"]),
                                        "FaithfulCopy": fmap.get(r["Path"])} for r in can if r["LinesOver2000Chars"]] +
                                      [{"Path": n, "Lines": run[n]["LinesOver2000Chars"], "ReadableCopy": None, "FaithfulCopy": None}
                                       for n in run_files if run[n]["LinesOver2000Chars"]],
                "Fidelity": {"Detector": "fidelity.py (%s; marca más de %d unidades UTF-16 por línea; copias de %d caracteres por línea como máximo)"
                                         % (FID.FORMAT, FID.FLAG_LINE_LIMIT, FID.COPY_LINE_LIMIT),
                             "Files": len(can), "Flagged": len(fc_entries),
                             "ByClass": {k: [r["Path"] for r in can if k in r["Fidelity"]["Flags"]] for k in FID.CLASSES
                                         if any(k in r["Fidelity"]["Flags"] for r in can)},
                             "FlaggedWithoutFaithfulCopy": [r["Path"] for r in can if r["Fidelity"]["Flags"] and r["Path"] not in fmap]}}
    reported["LinesOver2000CharsWithoutReadableCopy"] = [x["Path"] for x in reported["LinesOver2000Chars"] if not x["ReadableCopy"] and not x["FaithfulCopy"]]
    assert not reported["Fidelity"]["FlaggedWithoutFaithfulCopy"] and \
        not [p for p in reported["LinesOver2000CharsWithoutReadableCopy"] if p not in run_files], "archivos del cierre marcados sin copia fiel"
    for r in can:
        r.pop("PromptLabel", None)
    project = re.sub(r"[^A-Za-z0-9]", "-", CLONE)            # nombre del directorio de proyecto de claude-cli (D:\r62-arch-a7 → D--r62-arch-a7)
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "Provisional": bool(params.get("Provisional")), "Synthetic": bool(params.get("Synthetic")),
        "ObjectKind": "FILE", "ObjectPath": OBJECT, "ObjectBlob": params["Object"]["Blob"], "ObjectFiles": {OBJECT: params["Object"]["Blob"]},
        "PreviousObject": {"Path": OBJECT, "Blob": PREV_BLOB, "RunFile": "a7-r1.md", "Commit": "6fee8fd6a96ce4ae45d116e34affee4ed1b1fd40",
                           "Review": R1_RUN_ID},
        "Publication": {"Commit": params["Publication"], "Parent": params["PublicationParent"], "GuardsRunCommit": params["GuardsRunCommit"]},
        "PackagePath": None,
        "PackageNote": "sin paquete del Architect: el prompt hace de paquete (decisiones §94, punto 6: un kit de revisión limitado a R1-R3)",
        "Identity": ident,
        "PriorFindingIds": ROUND1_FINDINGS + ROUND1_OPTIONALS,
        "Question": "decisiones §94 (order.txt), punto 6: revisión formal independiente de la ronda 2 de A-7, acotada a A62-A7-R1..R3 y a sus "
                    "consecuencias: la resolución de R1, R2 y R3; la pureza del delta (cambios normativos solo por R1-R3; los descriptivos O1, O7 y O2 y "
                    "las precisiones de la exigencia de RED según 16.8, la medición sin hechos del relevo, la derivación como pieza del invalidador y "
                    "la exigencia de RED indeterminada, dentro de R1 o R2); la ausencia de regresión en los temas que la ronda 1 encontró CONFORMS; "
                    "la reconfirmación de la materialidad y de las consecuencias OWNER-RESERVED; las preguntas Q-A7R2-01..03; opcionalmente, la "
                    "conformidad de las disposiciones propuestas de O1-O8; los impactos relacionados D-1/FC solo si R1-R3 los cambian. AGREED solo "
                    "con cero REQUIRED; sin acreditación excepcional",
        "MaterialityA7": mat, "Round1Findings": ROUND1_FINDINGS, "Round1Optionals": ROUND1_OPTIONALS, "RegressionTopics": REGRESSION_TOPICS,
        "DeltaChanges": DELTA_CHANGES, "QuestionIds": QUESTION_IDS,
        "A7Literals": [{"Id": lid, "Source": lits[lid]["Source"], "Path": lits[lid]["Path"], "Ranges": lits[lid]["Ranges"], "A7Line": lits[lid]["Line"]}
                       for lid in LIT_IDS],
        "AcceptorPath": ACCEPTOR, "ToolPath": TOOL,
        "DirectRead": {k: DIRECT_READ[k] for k in ("Path", "Start", "End")},
        "OwnerGate": {"Check": OG.CHECK, "A6LineSha256": OG.A6_LINE_SHA, "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA, "RunId": OG.RUN_ID,
                      "BaseSection": OG.BASE_SECTION, "ObjectBlob": OG.OBJECT_BLOB, "A6AgreedBlob": OG.A6_AGREED_BLOB,
                      "AuthorizationState": "PENDING",
                      "Authorization": "PENDIENTE en el kit entregado: una disposición del Coordinator POSTERIOR a §94 que nombre esta revisión (decisiones "
                                       "§94, punto 6: «Sin revisión formal sin una disposición nueva del Coordinator»), todavía inexistente. Se fijará en "
                                       "owner_gate.py (AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA; pin_authorization.py calcula los valores) y el kit se "
                                       "volverá a congelar; fijada, exactamente una sección con ese número, el encabezado fijado, el blob completo de A-7 "
                                       "como token literal, el SHA-256 de su texto fijado y ninguna sección posterior en el momento del lanzamiento; "
                                       "launch.py lo exige en las decisiones del worktree (owner_gate.py). Mientras tanto la compuerta 11 bloquea con el "
                                       "único motivo de la autorización pendiente. Este cierre no cambia al fijarla"},
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ["--add-dir", RUN], "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ["--add-dir", RUN] + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "LaunchCommand": LAUNCH_COMMAND,
                      "AddDirStatus": "medido en C3 de la caracterización R20261016T185242Z-char de claude-cli 2.1.299 sobre la plantilla exacta "
                                      "con marcadores (decisiones §87, puntos 2-4; procedimiento de §56, punto 7): kit/transport-gate.json en MEASURED "
                                      "con el SHA-256 del registro fijado (kit/claude-cli-characterization.json, copia del registro del commit 1c960c0a)",
                      "Gate": "kit/transport-gate.json (transport_gate.py byte a byte el de A-6 ronda 4 y de la ronda 3: la compuerta D-01 y "
                              "cli_version y auth_status con la salida en bytes y UTF-8 estricto)",
                      "Stdin": "prompt.md del run (bytes literales)",
                      "JsonSchema": "kit/result.schema.json en forma compacta: json.dumps(separators=(',', ':'), ensure_ascii=True)",
                      "ExpectedInit": {"tools": ["Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "dontAsk", "mcp_servers": [],
                                       "claude_code_version": "2.1.299"},
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\" + project + "\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S, "AuthSettleSeconds": AUTH_SETTLE_S,
                      "AuthSettle": "launch.py espera AUTH_SETTLE_S segundos entre el final de `auth status` y el lanzamiento, y registra como "
                                    "TRANSPORT_AUTH_REFRESH_CONFLICT un result con «Failed to refresh OAuth token» y 0 tokens (sin reintento)",
                      "Characterization": "claude-cli-characterization.json de R20261016T185242Z-char (decisiones §87, puntos 2-3; evidencia §174), "
                                          "copia en kit/ del registro del commit 1c960c0a (blob 19b8a27e): C1 completada, C3 PASS sobre la plantilla "
                                          "con --add-dir"},
        "RunDir": RUN, "RunFiles": run_files, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "FaithfulCopies": fc_entries,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "los archivos del run (order.txt, prompt.md, a7-r1.md y las copias fieles) y, después del lanzamiento, el subdirectorio "
                      "launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json, "
                                "con la reproducción de las guardas de la ronda 2 y la verificación de cada copia fiel) y el auditor lo vuelve a "
                                "verificar después; el revisor contrasta lo que puede leer: AUTOMATION_PLAN L616-L617, order.txt, decisiones §94 "
                                "(bytes, SHA-256, puntos), el resultado `run` de las guardas de la ronda 2, el resumen de su preflight y la salida "
                                "de la revisión de la ronda 1",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` orienta (Scope SECTIONS o HEADER) o cubre el archivo entero (ENTIRE). Los rangos no "
                       "limitan: un archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo; un archivo marcado por el detector de fidelidad se lee SOLO por su copia fiel (FaithfulCopies)",
            "FaithfulCopies": "decisiones §94, punto 5: cada archivo del cierre cuyos bytes Read no puede entregar fielmente (fidelity.py: BOM, UTF-8 "
                              "inválido, NUL, CR suelto, controles, líneas de más de 1 900 unidades UTF-16, U+FEFF interior, separadores de línea "
                              "Unicode, puntos de código no medidos, extensiones que Read interpreta) tiene una copia fiel %s en el run, con la misma "
                              "numeración de líneas; el "
                              "prompt manda leer la copia, nunca el original. El auditor comprueba que cada copia es la codificación canónica de los "
                              "bytes exactos del original (blob fijado) y acredita la línea n del original solo si Read entregó fielmente la línea n "
                              "de la copia y todas sus continuaciones del apéndice; una lectura del original cuya entrega esté alterada sigue siendo "
                              "DEGRADED (motivo) y la eliminación silenciosa de un BOM nunca es fiel" % FID.FORMAT,
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run (I-62-A7/r2/, m7/s91/, m7/s92/, "
                    "m7/bench/designations/, el kit a6m7-measure y bmin/ están enteros en el cierre; el resto de m7/, de r4/, de I-62-A6/ y de "
                    "I-62-architect-A-7/ no)",
            "LargeFileThresholdBytes": LARGE, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > LARGE] + [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE] +
                          [e["Copy"] for e in fc_entries if e["CopyBytes"] > LARGE],
            "LongLines": {r["Path"]: r["Fidelity"]["Flags"]["LONG_LINE"] for r in can if "LONG_LINE" in r["Fidelity"]["Flags"]},
            "ClauseRefs": "toda referencia (`ruta:línea` o `ruta:inicio-fin`) apunta a líneas leídas enteras con Read en ese mismo archivo, o en su "
                          "copia fiel, y entregadas fielmente: de archivos del cierre (ruta relativa al clon) o de order.txt y a7-r1.md (nombre "
                          "suelto); nunca de prompt.md ni de una copia fiel (se cita el original). Una cita de una fuente dentro de A-7 no es una "
                          "lectura de la fuente",
            "DirectRead": "AUTOMATION_PLAN 16.8 L616-L617 se leen con Read en docs/AUTOMATION_PLAN.md o en su copia fiel (Paso 0) y constan en "
                          "DirectReads",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
        },
        "ReportedFiles": reported,
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "docs/automation/evidence/I-62-C1C3/ (diseño de C1-C3 y nota de aislamiento preventivo de I-65) y todo material de I-65",
             "Reason": "fuera del cierre (decisiones §94, punto 8): ajeno a esta revisión"},
            {"Source": "docs/automation/evidence/I-62-A6/ fuera de lo listado (r1-r4 salvo los tres archivos de r4/r1probes/ que cita el preflight "
                       "de la ronda 1 y sus copias legibles; m7/ fuera de s91/, s92/, bench/README.md, bench/p1-build.json, bench/p3-n4.json, "
                       "bench/p4-nc1.json, bench/designations/ y readings/readings.md)",
             "Reason": "revisión acotada a R1-R3 y sus consecuencias (decisiones §94, punto 6); los resultados del banco que A-7 usa constan, con sus "
                       "SHA-256, en los preflights de las dos rondas y en el README del banco"},
            {"Source": "el kit, la transcripción y los registros del lanzamiento de la revisión de la ronda 1 (salvo output.json, audit.json y "
                       "README.md) y los directorios de las revisiones anteriores",
             "Reason": "los hallazgos de la ronda 1 y su auditoría están en output.json y audit.json; el resto no es necesario para disponerlos"},
            {"Source": "el repositorio de la entrega del fixture FX-U1 (contrato, delegación, entrega y TRX reales), el bundle MAP-INVALID/candidate-b.bundle "
                       "(binario) y los clones temporales de los preflights",
             "Reason": "fuera del repositorio de I-62 o no texto; los preflights registran sus resultados y las copias custodiadas en b1-like/"},
            {"Source": "ejecución de las guardas, del constructor y del validador del cierre de A7-1, de la derivación del aceptante, del "
                       "instrumento, del aceptante o del lanzador",
             "Reason": "sin herramientas de ejecución; los resultados custodiados están en el cierre y el invocador reprodujo las guardas de la ronda 2"},
            {"Source": "WORKFLOW.md, FOUNDATIONS, AGENTS.md, state/I-62.yml y las rutas que A-7 o sus preflights solo nombran",
             "Reason": "A-7 no las cita por líneas; las guardas G2 comprueban sus objetos en la base y en la cabeza"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.299 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: identidad de la cuenta y estado de Git del cwd (HEAD separado del clon, sin rama, y asuntos de los commits recientes)",
            "adjunto environment: cwd " + CLONE + ", plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para el clon (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de las sesiones que prepararon A-7 y su ronda 2, de la revisión de la ronda 1, del "
            "Coordinator y de cualquier otra sesión)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*) y las áreas de trabajo de los preflights de A-7 y de los kits de revisión (D:\\r62-a7\\*)",
            "los archivos de " + RUN + " distintos de los del run (order.txt, prompt.md, a7-r1.md y las copias fieles), incluido el subdirectorio launch/",
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
        p = r["Path"]
        k = ("A7r2" if p.startswith(A7R2_DIR + "/") else "A7") if p.startswith(A7_DIR + "/") else (
            "m7" if p.startswith(M7 + "/") else ("r4" if p.startswith(R4_DIR + "/") else ("kit-m7" if p.startswith(KITM7 + "/") else (
                "bmin" if p.startswith(PKG_DIR + "/") else ("r1-review" if p.startswith(R1_REVIEW + "/") else "resto")))))
        g = by_group.setdefault(k, {"Files": 0, "Bytes": 0, "Lines": 0})
        g["Files"] += 1
        g["Bytes"] += r["Bytes"]
        g["Lines"] += r["Lines"]
    return {"ids": IDS, "canonical": len(can), "bytes": sum(r["Bytes"] for r in can), "lines": sum(r["Lines"] for r in can), "groups": by_group,
            "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in run_files},
            "faithful": {e["Original"]: [e["Copy"], e["CopyBytes"], e["Flags"]] for e in fc_entries},
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
    if "--faithful-copies" in argv:
        print(json.dumps(build_kit(CLONE_FIXED, params["CloneCommit"], RUN_FIXED, KIT, params, "copies"), ensure_ascii=False, indent=1))
        return 0
    print(json.dumps(build_kit(CLONE_FIXED, params["CloneCommit"], RUN_FIXED, KIT, params, "closure"), ensure_ascii=False, indent=1))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
