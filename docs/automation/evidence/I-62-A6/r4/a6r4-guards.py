#!/usr/bin/env python3
"""Guardas mecánicas de la candidata de la ronda 4 de la enmienda I-62 A-6 (decisiones §85).

Adaptadas de docs/automation/evidence/I-62-A6/r3/a6r3-guards.py (ronda 3, blob 103d69c6…), que no se modifica, con adapt_guards_r4.py
(sustituciones exactas; diff en a6r4-guards.diff). Autoridad: decisiones §85, punto 5 («Autoprueba y mutantes del texto corregido; negativos sobre
N4 y nc1; compatibilidad con los otros deltas de A-6; verificación de los blobs fijados del instrumento y del aceptante; integridad de las
autorizaciones del Owner; guardas sobre el commit exacto»).

Uso (siempre con `python -I -B`; solo lectura del repositorio; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I -B a6r4-guards.py self-test --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--tmp <dir>]
  python -I -B a6r4-guards.py preview --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--pkgdir <dir>] [--r4dir <dir>]
                                      [--selftest <json>] [--cand-repo <clon temporal> --cand-head <sha>] [--final] [--tmp <dir>]
  python -I -B a6r4-guards.py run --repo <repo> --base <sha> --head <sha> --out <json> [--tmp <dir>]

Guardas (las de la ronda 3, adaptadas a la ronda 4):
  G1  cabecera de la ronda 4; bloques literales de §2; citas «…»; prefijos, commits abreviados, identificadores de 40 hexadecimales y SHA-256
      conocidos o del paquete r4/; referencias «A6-n, regla k» existentes y, en el texto conservado de la ronda 3, a la misma regla; privacidad.
  G2  rutas del commit: solo docs/initiatives/I-62-A-6.md modificado (de la ronda 3, blob e62f27b8…, a la ronda 4) y archivos añadidos bajo
      docs/automation/evidence/I-62-A6/r4/; privacidad de lo añadido.
  G3  superficies prohibidas (más las revisiones de las rondas 1 a 3 y todo I-62-A6/ fuera de r4/), objetos congelados (entre ellos los árboles
      r2/ y r3/ y la revisión de la ronda 3), C-20b y rutas del commit.
  G4  dictamen, M-01..M-08 (columnas Dictamen, Ronda 1 y Ronda 2), revisión de la ronda 1 (§3.16), de la ronda 2 (§3.17, que la ronda 4 solo
      amplía por añadido en la celda «Prueba» de R1 (b)-(d)) y de la ronda 3 (R1 y O1-O7 en §3.18), ubicaciones coherentes con las líneas
      «Cubre» (la de A6-3 nombra R1 de la ronda 3), evidencia existente y compatibilidad con A-1..A-5.
  G5  pureza del delta (nueve párrafos en las mismas anclas); A6-1, A6-2 y A6-4 a A6-9 idénticos byte a byte a la ronda 3 (párrafo y sección);
      en A6-3, todo salvo la regla 6 (b) y el segmento de la ronda 3 de su línea «Cubre», byte a byte; la regla 6 (b) distinta de la de la ronda 3,
      con `RelayFacts` antes de `Closure`, los apartados (i)-(v) y la frase final, y cada frase exigida una sola vez y en su apartado; las secciones
      de §3 que la ronda 4 no cambia, más §6 (cabecera) y §9, idénticas; frases exigidas y prohibidas; cifras de topes; prohibiciones de §3.13.
  G6  líneas del Owner idénticas a las de la ronda anterior; (a) en decisiones §78.
  G7  identidad por blob del instrumento, del aceptante y de la especificación.
  G8  estado: PROPUESTA (ronda 4), ronda 4 NOT LAUNCHED, sin veredicto del Coordinator sobre la ronda 4, sin AGREED, FX-02 PASS, C-24 PASS ni
      F6 GATE PASS, ninguna línea del Owner pedida; en `run` (o --final), ningún marcador pendiente.
  G9  alcance de R5 (solo VERIFY, PLAN excluida).
  G10 resultado del aceptante: la tabla de A6-5, regla 2 (con la condición de R4 sobre `Verification` de A6-3, regla 6 (c)), frente a los bytes
      del aceptante fijado; entradas de la solicitud en A6-3, reglas 5-6 (`Closure` por su definición); correspondencias fijas de A6-5 con AP, RAE
      y V14.
  G11 fuentes y rangos de líneas para el revisor siguiente, incluidas las fuentes añadidas en la ronda 4 (§2).
  G5b autoprueba custodiada: Result PASS, ToolBlob = blob de a6r4-guards.py, A6TextBlob = blob de la A-6, todos los mutantes muertos.
Los blobs de los textos se calculan con CRLF -> LF. Todas las lecturas Git con GIT_NO_REPLACE_OBJECTS=1 y GIT_OPTIONAL_LOCKS=0. Sin escrituras
salvo --out y un directorio temporal propio. Nunca escribe la ruta del repositorio ni identidades del host.
"""
import argparse, ast, copy, difflib, hashlib, importlib.util, json, os, re, shutil, subprocess, sys, tempfile

# ---------------------------------------------------------------- identidades fijadas
PREP_BASE = "1652dd61ed55409db193311d7eda122126fa5062"   # commit de los literales de las fuentes (sin cambio en la base de la ronda 2)
BASE_RC = "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"     # base del mapa (C-20b)
FREEZE_SHA = "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43"
V14_COMMIT = "4c617e82b32b6c810b68d75fc19472efed22b393"
PKG_COMMIT = "9356edf141c795ea718a360e266334ded10d6a76"  # commit del paquete B-min
R1_COMMIT = "7b7aba4645ff2430d583c641d817630282a56fdf"   # commit que publicó la ronda 1
R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"     # A-6 ronda 1
R2_COMMIT = "ae9fe0e94b2dea9bc840d358d876deb6a6407391"   # commit que publicó la ronda 2
R2_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"     # A-6 ronda 2
R3_COMMIT = "a128efef8f83890f8deae66c286c553bed2663e3"   # commit que publicó la ronda 3
R3_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"     # A-6 ronda 3: texto anterior de la ronda 4
PREV_BLOB = R3_BLOB
V14 = "docs/initiatives/I-62-proposal-v14.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
A1, A2, A3, A4, A5 = ("docs/initiatives/I-62-A-%d.md" % i for i in (1, 2, 3, 4, 5))
A6 = "docs/initiatives/I-62-A-6.md"
EVID = "docs/automation/evidence/I-62-A6/"
EVID_R2 = EVID + "r2/"
EVID_R3 = EVID + "r3/"
EVID_R4 = EVID + "r4/"
GUARDS, SELFTEST = EVID_R4 + "a6r4-guards.py", EVID_R4 + "a6r4-selftest.json"
DEC = "docs/automation/decisions/I-62.md"
EVIDENCE_FILE = "docs/automation/evidence/I-62-evidence.md"
AP, WF, IL = "docs/AUTOMATION_PLAN.md", "docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md"
PT = "docs/initiatives/PROMPT_TEMPLATES.md"
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
AE = "docs/automation/agent-execution"
RAE = AE + "/README.md"
CODEXCLI = AE + "/adapters/codex-cli.md"
MAP = AE + "/compatibility/I62-clause-map.json"
SCHEMA = AE + "/compatibility/clause-map.schema.json"
CLAUSE_MAP = "docs/automation/evidence/I-62-F4/compat/clause_map.py"
MAPINV = "docs/automation/evidence/I-62-F6/MAP-INVALID"
BMIN = "docs/automation/evidence/I-62-F6/FX-02/bmin"
TOOL, ACC, SPEC = BMIN + "/tool/bmin_facts.py", BMIN + "/accept/bmin_accept.py", BMIN + "/SPEC.md"
ACC_NOTES = BMIN + "/accept/ACCEPT-NOTES.md"
E2E = BMIN + "/e2e/out/e2e-summary.json"
RULING_DIR = "docs/automation/evidence/I-62-architect-bmin"
RULING = RULING_DIR + "/R20261011T162328Z-5c5e/output.json"
KITCC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
A5EVID = "docs/automation/evidence/I-62-A5"
R1_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6"
R1_REVIEW = R1_REVIEW_DIR + "/R20261011T215735Z-c434/output.json"
R1_REVIEW_TREE = "520e7a1f67e8280e8432a7666624fcf548c51a66"
R1_REVIEW_BLOB = "e90d685bb829fc7d30eda9bdb05deb3ac66d24f5"
EVID_R1_TREE = "7f504a1710c593f896390474562574623552813c"   # árbol de I-62-A6/ de la ronda 1, recalculado sin la entrada r2
R2_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6r2"
R2_REVIEW = R2_REVIEW_DIR + "/R20261012T195511Z-e5c3/output.json"
R2_REVIEW_TREE = "acf2fa30bc7a1db4756e3f743cb1dcc227734b8b"
R2_REVIEW_BLOB = "fc357d09f1d345fa5ad55ca5aeffb6dc7510abaa"
EVID_R2_TREE = "411949b0f20cb6edb1a01a208ccdfb1b3b19db76"   # árbol de I-62-A6/r2/ (ronda 2)
EVID_R12_TREE = "1c7f8fbb25ebe173dad12f56157a6e6aeaa84a3c"  # árbol de I-62-A6/ en la base de la ronda 3 (rondas 1 y 2), sin r3
R3_REVIEW_DIR = "docs/automation/evidence/I-62-architect-A-6r3"
R3_REVIEW = R3_REVIEW_DIR + "/R20261013T183701Z-9f80/output.json"
R3_REVIEW_TREE = "f4442fdf2cdeb9a4e3abbe041443f32f77139745"
R3_REVIEW_BLOB = "691bbc8fabdefaa853c1d6e29465b6c1a1889107"
EVID_R3_TREE = "a4e93edba524476d28e1d7d50df6e9ef1fccb5ab"   # árbol de I-62-A6/r3/ (ronda 3)
EVID_R123_TREE = "b03ca6bd2b14ea69bdee733263eb2c2b6b8ebe41"  # árbol de I-62-A6/ en la base de la ronda 4 (rondas 1 a 3), sin r4

TOOL_BLOB = "9a829b0eeb1210238052b1feac06cfa8f7964aed"
ACC_BLOB = "434ef1fccb0d15762b30244b4a60928120698411"
SPEC_BLOB = "4d448c6fc4670cdf97b21dcd17fc00ba1bedcc0c"
INSTRUMENTS = [(TOOL, TOOL_BLOB), (ACC, ACC_BLOB), (SPEC, SPEC_BLOB)]
OWNER_A_SHA256 = "83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14"

FROZEN = {  # ruta -> blob o árbol fijado (base y head)
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f",
    A5: "ad94ceedacbf3dbf70f0217a583978d0e32592fc",
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", WF: "446c32ba6a91f9a712a0758038ae0a46ab1c970f",
    IL: "f19896a8f1a7c82f74bac7231636f68462a87271", PT: "384d0b0c3734ab7df881ad17fa740fdb8a6422d5",
    ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534", ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e",
    AE: "809fd58f450d3379a4fcc958cf8907fcdc5acff3", RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055",
    CODEXCLI: "155f3469e345e2397fa26347a9cb52d6fdb91122",
    MAP: "4d49d3e1a63d2b6128c9d9eeac473156a421571a", SCHEMA: "6b54dda9579d694200d8fc9ec26a4a846eef529b",
    CLAUSE_MAP: "ffe6ea57257f30fa9eec3e9aaf5bf9982bf59d08", MAPINV: "b0b3b2586a98883353dfda1c73b400355cbc51b0",
    BMIN: "3caa0dfa73d372af9251cf5c5c194261de8c7685", TOOL: TOOL_BLOB, ACC: ACC_BLOB, SPEC: SPEC_BLOB,
    RULING_DIR: "ee74d0f9c61d49e3cda8340e75c1f09d88ae73e9", RULING: "96c3efbc81e9c6814b232774fd8ac7e900deda7d",
    KITCC: "90929ad1c49c028f41f58f654f8713bd0b35f27b", A5EVID: "f03a0048e7b2c8788a4535d45392c9d49c2a3080",
    R1_REVIEW_DIR: R1_REVIEW_TREE, R1_REVIEW: R1_REVIEW_BLOB,
    R2_REVIEW_DIR: R2_REVIEW_TREE, R2_REVIEW: R2_REVIEW_BLOB, EVID_R2: EVID_R2_TREE,
    R3_REVIEW_DIR: R3_REVIEW_TREE, R3_REVIEW: R3_REVIEW_BLOB, EVID_R3: EVID_R3_TREE,
}
R1_SURFACES = [
    "src/", "tests/", "eng/", "tools/", "deploy/", "assets/", "docs/adr/", "docs/automation/agent-execution/", AP, WF, IL, "docs/FOUNDATIONS.md",
    "AGENTS.md", "CLAUDE.md", "Directory.Build.props", "Directory.Build.targets", "RackCad.sln", "global.json", "NuGet.Config", V14, FREEZE,
    A1, A2, A3, A4, A5, PT, "docs/automation/evidence/I-62-F4/", MAPINV + "/", BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/",
    RULING_DIR + "/", A5EVID + "/",
]
FORBIDDEN_SURFACES = R1_SURFACES + [R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/", R3_REVIEW_DIR + "/"]
EVID_OUTSIDE_R2_PHRASE = "todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r4/`"
MAP_SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md",
                "docs/adr/", "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
SOURCES = {"V14": (V14, FROZEN[V14]), "AP": (AP, FROZEN[AP]), "RAE": (RAE, FROZEN[RAE]), "ADR-0046": (ADR46, FROZEN[ADR46]),
           "ADR-0048": (ADR48, FROZEN[ADR48]), "LIFECYCLE": (IL, FROZEN[IL]), "A-1": (A1, FROZEN[A1]), "A-2": (A2, FROZEN[A2]),
           "A-3": (A3, FROZEN[A3]), "A-4": (A4, FROZEN[A4]), "A-5": (A5, FROZEN[A5]), "A-6": (A6, PREV_BLOB)}
BQ = {"A-2", "A-4", "A-5"}   # fuentes cuyas líneas citadas son de un bloque citado («> »): se comparan sin el marcador
DEC_BLOB = "228475b01def62985785042a86bf9d6bc68f9b07"   # decisiones en 1652dd61 (hasta §76); crecen solo por añadido

# ---------------------------------------------------------------- cabecera de la ronda 4
HEADER_EXACT = [
    "FREEZE_SHA     = " + FREEZE_SHA,
    "record         = %s (blob %s)" % (FREEZE, FROZEN[FREEZE]),
    "proposal       = " + V14,
    "  commit       = " + V14_COMMIT,
    "  blob         = " + FROZEN[V14],
    "A-1 AGREED (decisiones §43; %s, blob %s)" % (A1, FROZEN[A1]),
    "A-2 AGREED (decisiones §57, punto 1; %s, blob %s)" % (A2, FROZEN[A2]),
    "A-3 AGREED (decisiones §61, punto 1; %s, blob %s)" % (A3, FROZEN[A3]),
    "A-4 AGREED (decisiones §64, punto 1; %s, blob %s)" % (A4, FROZEN[A4]),
    "A-5 AGREED (decisiones §73, punto 1; %s, blob %s)" % (A5, FROZEN[A5]),
    "Amendment: A-6 (candidata, ronda 4;",
    "Previous round:\nronda 1 = blob %s (commit %s)." % (R1_BLOB, R1_COMMIT[:8]),
    "ronda 2 = blob %s (commit %s). Revisión formal R20261012T195511Z-e5c3" % (R2_BLOB, R2_COMMIT[:8]),
    "ronda 3 = blob %s (commit %s). Revisión formal R20261013T183701Z-9f80" % (R3_BLOB, R3_COMMIT[:8]),
    "Applies-to: I-62",
    "Scope:\nsolo las invocaciones VERIFY",
    "Architect review:\nronda 1: CHANGES REQUIRED, NOT_ACCREDITED (R20261011T215735Z-c434). Ronda 2: CHANGES REQUIRED, NOT_ACCREDITED "
    "(R20261012T195511Z-e5c3). Ronda 3:\nCHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80). Ronda 4: NOT LAUNCHED.",
    "Owner decision:\n(a) A6-ADR0046-9-FX02 = A, registrada (decisiones §78)",
    "State:\nPROPUESTA (ronda 4). No aplicada",
]
HEADER_NORM = [
    "Revisión formal R20261011T215735Z-c434",
    "Architect CHANGES REQUIRED (A62-A6-R1..R5 y A62-A6-O1..O3); auditor NOT_ACCREDITED",
    "del EXECUTION_CONTROLLER de FX-02 en F6 (Topología A)",
    "La planificación (PLAN) sigue el procedimiento vigente, sin el instrumento",
    "M-02, M-03, M-04, M-05 y M-08 sí; M-01, M-06 y M-07 no",
    "que las rondas 2 a 4 no amplían",
    "Architect CHANGES REQUIRED (A62-A6R2-R1 y A62-A6R2-R2; A62-A6R2-O1..O5); auditor NOT_ACCREDITED",
    "sin usar el dictamen como acuerdo formal (decisiones §83, punto 1)",
    "en la ronda 3 solo en A6-3, regla 6, A6-5 y A6-7 (§3.17)",
    "y, en la ronda 4, solo en A6-3, regla 6 (b) (§3.18)",
    "auditor ACCREDITED; Architect CHANGES REQUIRED (A62-A6R3-R1; A62-A6R3-O1..O7)",
    "Decisiones §85, punto 1: A62-A6R3-R1 ACCEPTED REQUIRED y A62-A6R2-R2 CORRECTED",
    "Las rondas 2 a 4 conservan los blobs del instrumento y del aceptante que fija (a)",
    "(b) A6-CONTROLLER-MEDICION-CONSUMO: no pedida",
]
HEADER_FORBIDDEN = [r"Coordinator verdict", r"A-6 AGREED \(decisiones", r"Architect review:\n\s*AGREED", r"State:\n\s*(?:AGREED|APLICADA)",
                    r"Owner decision:\n\s*(?:NOT )?REQUIRED", r"(?i)ronda 4:\s*\**(?:AGREED|CHANGES REQUIRED|LAUNCHED)",
                    r"(?i)ronda 3:\s*\**(?:AGREED|NOT LAUNCHED)", r"(?i)ronda 2:\s*\**AGREED"]

# ---------------------------------------------------------------- literales citados: (fuente, línea inicial, línea final, texto exacto, referencia)
S765 = "A-6 no se presenta a acuerdo mientras exista una consecuencia OWNER-RESERVED sin disponer"
LIT = [
    ("DEC", 1539, 1539, "como insumos canónicos", "DEC L1539"),
    ("DEC", 1539, 1539, "Ni aplicar A-6 ni declararla AGREED", "DEC L1539"),
    ("DEC", 1542, 1542, S765, "DEC L1542"),
    ("V14", 2523, 2523, "| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |", "V14 L2523"),
    ("V14", 2528, 2528, "Ningún tope autoriza gasto ni ejecución ahora.", "V14 L2528"),
    ("V14", 960, 960, "| OV-I62-03 topología A | F6 | C-24 | ejecución compacta (D.5) |", "V14 L960"),
    ("V14", 1043, 1043, "elige una decisión del Owner;", "V14 L1043"),
    ("V14", 974, 974, "autorización previa de apertura y consumo, no transporte", "V14 L974"),
    ("V14", 2536, 2536, "registro incompleto", None),
    ("AP", 646, 646, "Nunca bastan stdout, un código de salida o un JSON conforme;", "AP L646"),
    ("AP", 623, 624, "Con varias en `fail`: STOP > BLOCKED > REWORK.", "AP L623-L624"),
    ("AP", 1221, 1221, "permisos mínimos: ARCHITECT, EXECUTION_CONTROLLER y REVIEWER son de solo lectura (`Permissions` = READ_ONLY);", "AP L1221"),
    ("AP", 1240, 1240, "PLAN y VERIFY no son intercambiables.", "AP L1240"),
    ("AP", 1093, 1093, "**Sin celda elegible no se invoca** (P-10)", "AP L1093"),
    ("A-4", 192, 192, "la invocación del rol puede declarar en su texto un shell alternativo explícito, disponible en el host y nombrado en la orden",
     "A-4 L192"),
    ("A-4", 203, 203, "Su tope es uno en total para FX-02 en F6", "A-4 L203"),
    ("A-4", 206, 207, "Solo una actualización observada del runtime, por la regla de nueva medición de A-2, u otra A-n abren otra medición",
     "A-4 L206-L207"),
    ("A-4", 393, 393, "cambia el comportamiento observable del piloto: el Controller ejecuta con un shell declarado;", "A-4 L393"),
    ("A-4", 169, 169, "D.3 supone que la receta basta para ejecutar comandos y no prevé otro adapter para el Architect de A.", "A-4 L169"),
    ("A-5", 216, 216, "(**11** base, pool **4** con ≤ 2 por fase, tope **15**)", "A-5 L216"),
    ("A-5", 221, 221, "extiende a esta fila.", "A-5 L221"),
    ("DEC", 1488, 1488, "Obligación expresa: resolver la construcción del fixture para OV-I62-03, OV-I62-05 (a), OV-I62-05 (b) y OV-I62-06 mediante "
                        "una enmienda material acordada antes de READY-01 y READY-08", "DEC L1488"),
    ("DEC", 1507, 1507, "Bloque A2-P2 del runtime `d13914ce…` = **2/2 AGOTADO**", "DEC L1507"),
    ("DEC", 1508, 1508, "Se registra como **capacidad observada** la lectura fiel posterior mediante `python -c` con JSON de escapes ASCII", "DEC L1508"),
    ("DEC", 1510, 1510, "El Controller sigue verificando y emitiendo una decisión estructurada, sin delegar su autoridad en las herramientas",
     "DEC L1510"),
    ("DEC", 1523, 1523, "Las herramientas no entran en F_seed, F_norm ni en los contratos de producción", "DEC L1523"),
    ("DEC", 1524, 1524, "El oráculo de C-24 no se comparte con el Controller", "DEC L1524"),
    ("DEC", 1539, 1539, "No cambian la receta 16.4, el adapter `codex-cli`, los esquemas de salida, los presupuestos congelados, los roles ni la "
                        "autoridad del Controller, ni el comportamiento de producción fuera de la excepción propuesta", "DEC L1539"),
    ("DEC", 1541, 1541, "El Coordinator no sustituye esa decisión", "DEC L1541"),
    ("DEC", 1543, 1543, "Sin invocación nueva y sin reutilizar presupuesto agotado", "DEC L1543"),
    ("DEC", 1545, 1545, "Ni iniciativa nueva ni ampliación de A-6 fuera de FX-02", "DEC L1545"),
    ("V14", 2587, 2587, "automáticas y la auditoría, con su límite.", "V14 L2587"),
    ("V14", 2554, 2554, "eso solo lo decide FX-04a.", "V14 L2554"),
    ("OWNER_A", 0, 0, "en su verificación", None),         # línea (a) del Owner (decisiones §78)
    ("A6R1", 0, 0, "un clon del fixture", None),            # texto de la ronda 1 (A6-3, regla 4)
    ("A6R1", 0, 0, "FAIL de aislamiento", None),            # texto de la ronda 1 (A6-3, regla 3)
]
NOTATION = {"V14 L…", "AP L…", "RAE L…", "DEC L…"}
OWN_TERMS = {"Controller más instrumento"}
DELTA_INNER_LIT = {"**Codex, total**": ("V14", 2442, 2442), "+ 2 sondas": ("V14", 2523, 2523)}
SHORT_BLOBS = {"34ad80ea": V14, "0f6b8608": FREEZE, "c01899a7": A1, "f1e1d6f0": A2, "ea6721f7": A3, "5e2ba68e": A4, "ad94ceed": A5,
               "f525cb1e": AP, "446c32ba": WF, "f19896a8": IL, "384d0b0c": PT, "da275e1a": ADR46, "e1bd8d91": ADR48, "592dcfd4": RAE,
               "155f3469": CODEXCLI, "809fd58f": AE, "90929ad1": KITCC, "3caa0dfa": BMIN, "ee74d0f9": RULING_DIR, "96c3efbc": RULING,
               "f03a0048": A5EVID, "9a829b0e": TOOL, "434ef1fc": ACC, "4d448c6f": SPEC, "520e7a1f": R1_REVIEW_DIR, "e90d685b": R1_REVIEW,
               "c5cc6168": EVID + "a6-guards.py", "8a61a8c4": EVID + "a6-selftest.json", "fc357d09": R2_REVIEW, "acf2fa30": R2_REVIEW_DIR,
               "411949b0": EVID_R2, "691bbc8f": R3_REVIEW, "f4442fdf": R3_REVIEW_DIR, "a4e93edb": EVID_R3}
SHORT_AT_REV = {"eac473c0": (R1_COMMIT, A6), "86da0ae7": (R2_COMMIT, A6), "e62f27b8": (R3_COMMIT, A6)}   # rondas anteriores, en su commit
SHORT_EVID_R1 = "7f504a17"                                                   # árbol de I-62-A6/ de la ronda 1: recalculado sin r2/, r3/ ni r4/
NON_GIT_HASHES = {"d13914ce", "83152eae"}         # BinaryHash de codex-cli y SHA-256 de la línea (a): no son objetos Git
BINARY_HASH = "d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec"
APP_VERSION = "26.1007.2314.0"
FINGERPRINT = "F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113"
KNOWN_SHA256 = {  # valor (minúsculas) -> ("blob", ruta) recalculado en la base, ("dec", texto) presente en decisiones, o ("owner_a", None)
    "b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200": ("blob", TOOL),
    "173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0": ("blob", ACC),
    "510143f24522211ed0480c813d496dcf47e4c4f8feba8ceb02148ddfa4932fb9": ("blob", SPEC),
    BINARY_HASH: ("dec", BINARY_HASH),
    FINGERPRINT.lower(): ("dec", FINGERPRINT),
    OWNER_A_SHA256: ("owner_a", None),
}
BLOCK_HEADER = re.compile(r"^(V14|AP|RAE|ADR-0046|ADR-0048|LIFECYCLE) L(\d+)-L(\d+)((?: y L\d+-L\d+)*), literal", re.M)

# ---------------------------------------------------------------- dictamen y revisión de la ronda 1
RULING_VERDICT = "MATERIAL"
M_YES = {"M-02", "M-03", "M-04", "M-05", "M-08"}
OWNER_NAMES = ["A6-ADR0046-9-FX02", "A6-CONTROLLER-MEDICION-CONSUMO"]
R1_FINDINGS = ["R1", "R2", "R3", "R4", "R5", "O1", "O2", "O3"]
R2_ROWS = ["R1 (a)", "R1 (b)", "R1 (c)", "R1 (d)", "R2 (a)", "R2 (b)", "O1", "O2", "O3", "O4", "O5"]   # filas de §3.17
R2_FINDINGS = ["R1", "R2", "O1", "O2", "O3", "O4", "O5"]
R2_ROW_RX = re.compile(r"^((?:R[12] \([a-d]\))|O[1-5]):")
R3_ROWS = ["R1", "O1", "O2", "O3", "O4", "O5", "O6", "O7"]   # filas de §3.18 (hallazgos de la ronda 3)
R3_FINDINGS = list(R3_ROWS)
R3_ROW_RX = re.compile(r"^(R1|O[1-7]):")
R3_317_APPENDED = ["R1 (b)", "R1 (c)", "R1 (d)"]   # filas de §3.17 cuya celda «Prueba» la ronda 4 solo amplía por añadido

# ---------------------------------------------------------------- delta
DELTAS = ["A6-%d" % i for i in range(1, 10)]
SEC = {"A6-1": "### 3.4 A6-1 — ", "A6-2": "### 3.5 A6-2 — ", "A6-3": "### 3.6 A6-3 — ", "A6-4": "### 3.7 A6-4 — ", "A6-5": "### 3.8 A6-5 — ",
       "A6-6": "### 3.9 A6-6 — ", "A6-7": "### 3.10 A6-7 — ", "A6-8": "### 3.11 A6-8 — ", "A6-9": "### 3.12 A6-9 — "}
R1_SEC = SEC
IDENTICAL_TO_R1 = ["A6-1", "A6-2", "A6-4", "A6-5", "A6-6", "A6-7", "A6-8", "A6-9"]   # párrafo idéntico byte a byte al de la ronda 3
SECTION_IDENTICAL = list(IDENTICAL_TO_R1)                                            # y sección entera idéntica
A63_SAME_RULES = [0, 1, 2, 3, 4, 5, 7]                                  # en A6-3 solo cambia la regla 6, y en ella solo el apartado (b)
SAME_SECTIONS = ["## 3. ", "### 3.2 ", "### 3.3 ", "### 3.13 ", "### 3.14 ", "### 3.15 ", "### 3.16 ",   # §3 sin cambio en la ronda 4
                 "## 6. ", "## 9. "]                                                                # cabecera de §6 y §9
R6_MARK = re.compile(r"^>\s+(?:\(([a-d])\)|\(([ivx]+)\)|(La prueba previa )|(El aceptante vuelve a ejecutar ))")
R6_ORDER = ["6", "a", "b", "i", "ii", "iii", "iv", "v", "cierre", "c", "d", "final"]   # regla 6 de A6-3 en la ronda 4
R6_ORDER_PREV = ["6", "a", "b", "c", "d", "final"]                                       # y en la ronda 3
R6B_KEYS = ["b", "i", "ii", "iii", "iv", "v", "cierre"]                                  # el apartado (b) completo
R6B_REQUIRED = [  # (parte del apartado (b), frase exigida una sola vez en (b))
    ("b", "con su tabla aplicada así cuando una de sus entradas no da un valor"), ("i", "faltan si E no fija su blob"),
    ("i", "un único objeto JSON en UTF-8 sin claves repetidas"), ("i", "Se omite entonces todo término de la tabla que lee la entrada que falta"),
    ("ii", "no está, es `null` o está vacío no aporta ningún término"), ("ii", "40 caracteres hexadecimales en minúscula"),
    ("ii", "`git --no-replace-objects cat-file -t`"), ("iii", "da el conjunto vacío"), ("iv", "no se sustituye por ningún otro valor"),
    ("v", "registra cada término omitido con su causa"), ("v", "se custodia con la solicitud y el cierre"),
    ("cierre", "derivan su cierre con esta misma regla"),
    ("ii", "si no (en nc1, H.CurrentSha), se omite"),   # añadida por las guardas: el valor de commit inválido se omite
]
LOC = {
    "A6-1": "Ubicación: al final de V14 D.3, a continuación del párrafo de A5-2 (A-5 §3.3)",
    "A6-2": "Ubicación: a continuación del párrafo de A6-1 (al final de V14 D.3)",
    "A6-4": "Ubicación: a continuación del párrafo de A6-2 (al final de V14 D.3)",
    "A6-7": "Ubicación: a continuación del párrafo de A6-4 (al final de V14 D.3)",
    "A6-9": "Ubicación: a continuación del párrafo de A6-7 (al final de V14 D.3)",
    "A6-8": "Ubicación: a continuación del párrafo de A6-9 (al final de V14 D.3",
    "A6-5": "Ubicación: al final de V14 D.4, a continuación de su última línea",
    "A6-6": "Ubicación: a continuación del párrafo de A6-5 (al final de V14 D.4)",
    "A6-3": "Ubicación: al final de V14 D.6, a continuación de su último párrafo",
}
ORDER_D3 = ["A6-1", "A6-2", "A6-4", "A6-7", "A6-9", "A6-8"]
ORDER_D4 = ["A6-5", "A6-6"]
ORDER_D6 = ["A6-3"]
ANCHOR_V14 = "Ningún tope autoriza gasto ni ejecución ahora."   # ancla de A-2
ANCHOR_A2_END = "Ningún otro tope cambia."                      # fin de A2-P2, ancla de A-4
ANCHOR_A46_END = "también para A2."                             # fin de A4-6, ancla de A5-2
ANCHOR_D1 = "   (F_eff). **Contrato válido** (A1'/A6')."        # V14 L2406, ancla de A5-1
ANCHOR_D1_LINE = 2406
ANCHOR_A52_END = "extiende a esta fila."                        # fin de A5-2 (A-5 L221), ancla de A-6 en D.3
ANCHOR_D4 = "- ninguna limitación de B demuestra que el Principal B sea incapaz de reconstruir ni acredita la portabilidad: eso solo lo decide FX-04a."
ANCHOR_D4_LINE = 2554
ANCHOR_D6 = "automáticas y la auditoría, con su límite."
ANCHOR_D6_LINE = 2587
H_TITLE = "# I-62 — Proposal V14: portabilidad del Principal, binding de roles independiente del proveedor y orquestación autónoma de roles"
H_ANEXO_D = "## Anexo D — Pilotos y portabilidad (plano c)"
H_D3 = "### D.3 Hoja de invocaciones y escenarios"
H_D4 = "### D.4 Resultados y cobertura"
H_D5 = "### D.5 "
H_D6 = "### D.6 Entradas y aislamiento (cobertura declarada)"
H_D7 = "### D.7 "
CAPS = "| **Codex, total** | | **11** | | pool **4** (≤ 2 por fase) | **15** |"
CAPS_LINE = 2442
SCOPE = {
    "A6-1": "Solo para la celda de EXECUTION_CONTROLLER de la Topología A (FX-02), en F6, solo en sus invocaciones VERIFY",
    "A6-2": "Solo para cada invocación del Controller de FX-02 en F6",
    "A6-3": "Solo para el instrumento de la regla de A-6 en D.3 y solo en F6",
    "A6-4": "Solo para cada invocación VERIFY del Controller de FX-02 en F6",
    "A6-5": "Solo para FX-02 en F6, en las invocaciones VERIFY",
    "A6-6": "Solo para FX-02 en F6",
    "A6-7": "Solo para la celda de EXECUTION_CONTROLLER de la Topología A (FX-02), en F6",
    "A6-8": "Las reglas de A-6 sobre el instrumento del Controller de FX-02",
    "A6-9": "Solo para FX-02 en F6, en las invocaciones VERIFY",
}
REQUIRED = {
    "A6-1": ["un único instrumento determinista de solo lectura", "el archivo cuyo blob fija A-6", "ejecutado desde su objeto Git",
             "--no-replace-objects cat-file blob <blob> | <intérprete> -I - <orden> …`", "en modo aislado",
             "por su ruta, su versión y el SHA-256 de su ejecutable", "la planificación (PLAN) no usa el instrumento",
             "sin su objeto en el cierre y sin el aceptante", "ninguna regla de A-6 se aplica a ella", "no emite ningún veredicto",
             "ni PASS, ni FAIL, ni VERIFIED", "ningún elemento de la receta del adapter cambia",
             "Tampoco cambian el descriptor ni el contrato del adapter, los esquemas de salida", "el rol ni la autoridad del Controller",
             "no es un lanzamiento", "Los presupuestos congelados no cambian", "(**11** base, pool **4** con ≤ 2 por fase, tope **15**)",
             "ningún contador se reinicia", "los insumos que esta regla añade a una invocación VERIFY son solo tres",
             "ni el oráculo del Coordinator entran en ninguna invocación del Controller",
             "comprueba con la orden Git primaria que la revisión existe y que el archivo se puede leer",
             "toda ejecución del instrumento que conste en el registro de la invocación entra en la reproducción del aceptante",
             "es un invalidador", "no entra en F_seed, en F_norm", "ningún contrato de producción", "esta regla no cambia el comportamiento de producción"],
    "A6-2": ["sin cambiar el texto de §20.3.1, de AUTOMATION_PLAN 16.24 ni de su procedimiento subordinado",
             "`CanonicalInputs` incluye una entrada `{Path, Blob}` del archivo del instrumento", "Es la única excepción a la clase de §20.3.1",
             "no aporta obligaciones de lectura", "`Class` = ACTION_COMPATIBLE", "como fuente de la obligación (`RequiredBy`), esta regla",
             "`DecisionRef` de la disposición del Coordinator", "Ninguna otra acción se añade", "no contra la `AuthorityRevision` del fixture",
             "Las demás entradas conservan sin cambio esa comprobación", "un blob distinto obliga a recalcular el cierre antes de lanzar",
             "no son entradas del cierre", "no se aplica a ningún otro rol, invocación, unidad ni escenario"],
    "A6-3": ["**escritura de preparación:**", "nunca de un árbol de trabajo", "solo como objeto Git suelto", "`git hash-object -w --no-filters --stdin`",
             "sin archivo en el árbol, sin índice, sin commit, sin ref y sin push", "fuera de toda cesión",
             "antes de que se abra la ventana de la unidad", "así que W-2 no cambia",
             "Registra en la evidencia real la `DecisionRef` de la disposición que la autoriza", "incluidas las de `refs/replace/`",
             "que solo pueden diferir en", "dejan la ronda sin instrumento", "el instrumento no entra en F_seed, en F_norm",
             "**identidad del objeto efectivo en el punto de uso:**", "inmediatamente antes de cada lanzamiento VERIFY con el instrumento",
             "(i) ninguna ref bajo `refs/replace/`", "ni `info/grafts` ni `objects/info/alternates`", "el repositorio no es superficial",
             "`git --no-replace-objects cat-file -t <blob>` = `blob`", "`git hash-object --no-filters --stdin`, el blob fijado",
             "`git fsck --full` sin errores", "con la ruta, la versión y el SHA-256 de la disposición",
             "deja la invocación sin lanzar y sin contar", "evidencia contradictoria (S-04), con STOP",
             "el Controller lee el instrumento solo por su blob y con la orden declarada", "worktree real de I-62",
             "INVALID_REVIEW_CONTEXT (P-22)", "No es el FAIL de aislamiento de FX-04a", "**lugar y momento del aceptante:**",
             "sobre el mismo repositorio y la misma ruta del `-C` en que corrió el instrumento",
             "antes de cualquier escritura de la sesión en ese directorio", "La sesión queda retenida en ese intervalo",
             "fuera de la sesión de Principal del fixture y de toda cesión", "su árbol de procesos terminó antes de liberar la retención", "(P-02)",
             "la supervisión construye la solicitud del aceptante de forma mecánica",
             "`Runs` = toda ejecución del instrumento que conste en el registro de sesión del adapter",
             "solo para una invocación VERIFY en que corrió el instrumento",
             "reconstruida de ese registro con la regla de reconstrucción que demostró la medición",
             "El registro tiene que estar completo", "Es ejecución del instrumento toda orden registrada que contiene el blob fijado",
             "que tiene exactamente la forma de la orden declarada",
             "exactamente un sobre del instrumento, en su serialización canónica y terminado en LF, sin alterar ningún byte",
             "no hay solicitud y la salida queda sin aceptar",
             "solo si esos tres archivos existen y son legibles y ninguna de las comprobaciones `Identity`, `Scope`, `CleanTree`, `Trailer` y "
             "`FreeText` está en `not_run` en esa salida",
             "entre ellos N4 (entrega ausente), la solicitud no lleva `Verification` y declara R4 no aplicable, con la causa",
             "compara con el sobre registrado sus argumentos, entradas, llamadas a Git, hechos y error (R3)",
             "en una invocación VERIFY sin ejecuciones no hay solicitud ni corrida del aceptante",
             "la interpretación del Controller nunca es autoridad para él",
             "ninguna de estas comprobaciones ni el aceptante se ejecutan sobre un clon distinto del `-C`"],
    "A6-4": ["tiene su registro de fidelidad", "Su `VerificationMethod` es el recálculo del blob sobre los bytes que entrega esa orden",
             "nunca la comparación del nombre del objeto", "sin FAITHFUL no hay lanzamiento (P-24)",
             "el SHA-256 de los hechos de cada salida usada", "al menos con la orden Git primaria", "nunca basta por sí sola",
             "se auditan contra el cierre efectivo de insumos", "cada llamada de `GitCalls` usa una suborden de la lista cerrada del instrumento",
             "cada revisión de su argv es un commit del cierre derivado", "incluidas las de `hashes --paths`",
             "es una ruta permitida del cierre derivado", "cada entrada `git-blob` de `Inputs` es un `{Rev, Path, Blob}` del cierre derivado",
             "corresponde a un `--file` del argv con su `--sha256`", "auditadas por sus argumentos, no lecturas internas del runtime",
             "La comprobación R9 del aceptante aplica esa correspondencia", "INVALID_REVIEW_CONTEXT (P-22)", "deja Contexto en UNKNOWN",
             "la cobertura se declara"],
    "A6-5": ["**en la invocación:**", "no es evidencia", "queda sin establecer (`not_run`; AUTOMATION_PLAN 16.22)", "con la regla de 16.9 sin cambio",
             "es evidencia contradictoria", "sin elegir entre las dos", "`Disposition` STOP y S-04 en `TriggeredStopConditions` (16.11)",
             "como fija 16.9, sin cambio", "ningún código del instrumento es por sí mismo una clasificación", "**resultado real del aceptante:**",
             "La `Decision` sola no basta", "esta regla lee las `Checks`", "**salida aceptada:** se ingiere", "**salida no aceptada:**",
             "es una salida conforme pero falsa o no demostrada", "INVALID_OUTPUT de 16.9 (nunca VERIFIED; transporte BLOCKED)",
             "con el efecto de un resultado de rol inválido de 16.23 (P-19: no avanza)", "Su `Classification` y su `Disposition` no se aplican",
             "ni corrección, ni `attempts`, ni contador por clase, ni STOP derivado de ella",
             "reejecución con `RunId` nuevo, como máximo dos por (`TaskId`, fase) y dos por control negativo", "la tercera → STOP (P-04)",
             "en un control negativo, además control no superado", "Cada reejecución es un lanzamiento que cuenta en los topes de D.3",
             "esta regla no abre ninguna invocación", "Una corrida del aceptante con código 64 es un error de la supervisión, no un resultado",
             "**rechazo de la evidencia y falla del producto:**",
             "una salida no aceptada nunca es por sí misma un FAIL de la entrega del Worker ni de FX-02",
             "FX-02 solo es PASS con salidas ingeridas (aceptadas, o sin ejecuciones por la regla 7) y conformes al oráculo sellado",
             "FAIL sigue siendo una violación observada en una salida ingerida",
             "FX-02 queda UNVERIFIED con esa causa (registro incompleto)", "**lecturas normativas del aceptante:**",
             "declara, regla por regla, que coincide con la del aceptante fijado",
             "cambiarlo exige otro blob, otra corrección de A-6 y otra determinación del Owner", "ambigüedad material (S-03), con STOP",
             "con el ámbito de la regla 7",
             "R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera",
             "Una salida `EXECUTION_VERIFIED` solo queda aceptada con R4 en `MATCH`",
             "para R4, `fail` y `not_run` son por igual distintos de `pass` (16.22)", "la juzga 16.9 sin cambio",
             "el juicio del control sigue siendo su oráculo relativo (RAE §10)", "un registro que no permite reconstruir las ejecuciones",
             "una salida sin ejecuciones que cita una orden del instrumento o un `FactsSha256`",
             "la sesión no la reejecuta: la devuelve al Coordinator como evidencia contradictoria (S-04), con STOP",
             "con un cierre derivado del cierre efectivo real de las invocaciones VERIFY del kit",
             "o si una lectura que la verificación necesita queda fuera de ese cierre", "**ámbito:**",
             "Ninguna está obligada a ejecutar el instrumento", "**con ejecuciones:**", "Solo a ellas se aplican el aceptante",
             "**sin ejecuciones:**", "No hay solicitud ni aceptante",
             "con 16.9, 16.11 y 16.22 sin cambio, y no puede apoyarse en ningún hecho del instrumento", "**registro insuficiente:**",
             "**dónde no corresponde ejecutarlo:**", "En N4 (entrega ausente) no hay ninguna orden del instrumento sobre la entrega",
             "en nc1 (`CurrentSha` inexistente) ninguna sobre ese `CurrentSha`"],
    "A6-6": ["se calcula sin el instrumento, sin el aceptante y sin la especificación del instrumento", "la fija el Coordinator, no esa especificación",
             "el oráculo se sella", "antes de la primera invocación del Controller de FX-02 con el instrumento", "nunca se entrega al Controller",
             "demuestra la configuración «Controller más instrumento»", "no el protocolo de producción sin instrumento",
             "no se extiende a D.5, a la Owner Validation"],
    "A6-7": ["solo para las invocaciones VERIFY de esta topología", "tras **una** invocación medida adicional, de solo lectura",
             "la entrega exacta de los bytes del objeto por la orden declarada en el shell de la invocación",
             "La medición no demuestra ninguna operación de la planificación", "esa invocación es una medición, no una invocación de rol de FX-02",
             "ni VERIFIED, ni C-24 PASS", "el instrumento escrito como fija la regla de A-6 en D.6 y su comprobación del punto de uso",
             "la autorización de consumo del Owner", "Se cuenta aparte", "del sumando «+ 2 sondas» de los totales",
             "de los bloques de la regla de nueva medición de A-2", "de la invocación de la regla de A-4 sobre el shell",
             "No usa ni repone un bloque agotado ni una medición consumida", "Su tope es uno en total para FX-02 en F6",
             "P-07 lo comprueba antes de lanzarla", "la invocación no se repite bajo esta regla", "el escenario sigue UNVERIFIED con esa causa (D.4)",
             "blob del instrumento, orden declarada", "sin reutilización",
             "frente a la disposición en la comprobación del punto de uso de cada invocación VERIFY",
             "y otra vez en su Entrada", "una diferencia en la primera deja la invocación sin lanzar, y en la segunda deja su salida sin aceptar",
             "P-01 y OD-2 rigen sin cambio",
             "Sobre el registro de sesión real de esa misma invocación demuestra además",
             "que `git`, resuelto por nombre en el entorno de la invocación, es el Git declarado",
             "la reconstrucción de `Runs` de todas sus ejecuciones del instrumento", "sin alterar ningún byte",
             "una corrida del aceptante fijado sobre esas `Runs`", "que da `ACCEPT_FACTS` con R1, R2, R3 y R9 en `MATCH`",
             "R4 y R10 no aplicables, porque la medición no produce una verificación", "regla de reconstrucción de `Runs` o cierre de VERIFY de la regla 7",
             "antes de la medición, una disposición del Coordinator fija cómo el cierre efectivo de las invocaciones VERIFY de FX-02",
             "en particular las de `CurrentSha`", "o limita las órdenes del instrumento a las lecturas que ese cierre admite",
             "Sin esa disposición no hay medición", "la celda no es elegible con el instrumento y el aceptante no se usa",
             "Ninguna de estas condiciones se da por cumplida con evidencia que todavía no existe"],
    "A6-8": ["solo tienen efecto si el Owner determina", "antes del acuerdo de A-6", "como decisión reservada", "ADR-0046 #9",
             "usar ese instrumento y su aceptante en la verificación del Controller de FX-02", "FX-02 en F6, Topología A",
             "la celda de EXECUTION_CONTROLLER en su verificación, es decir, en sus invocaciones VERIFY",
             "Quedan excluidos de forma expresa la planificación (PLAN), la producción, cualquier otra unidad, escenario, rol o topología, D.5 y la "
             "Owner Validation", "A-6 no concede esa determinación", "no la amplía", "no modifica ADR-0046 ni ADR-0048", "no crea un precedente",
             "ninguna excepción está vigente sin la decisión registrada del Owner", "ni el Coordinator ni el Principal la sustituyen",
             "ni eligen una lectura más amplia de ella", "las reglas de A-6 no se aplican", "no autoriza ninguna invocación ni ningún consumo",
             "exige su propia autorización de consumo del Owner", "un instrumento o un aceptante con otro blob no está cubierto por la determinación"],
    "A6-9": ["el instrumento no emite PASS, FAIL ni VERIFIED", "El Controller los interpreta, los contrasta y emite su resultado estructurado",
             "no se delegan en el instrumento", "incluida la tabla de defectos de `Validate(M)`",
             "A-6 no reinterpreta 16.13 ni adopta la lectura de la especificación del instrumento", "se devuelve al Coordinator, que la decide",
             "con la evidencia de independencia efectiva custodiada por blob antes de la", "cada uno ejecutado sobre una entrada que lo active de forma observable",
             "de modo que ni R1 ni R3 puedan detectarlo", "a los que no se atribuye detección", "sin un oráculo que importe el aceptante",
             "sin declararla no aplicable", "la tabla de primitivas de cada pieza por hecho crítico", "la lista expresa de lo que sigue sin demostrar",
             "el aceptante no se usa tal como está", "se deriva mecánicamente del cierre efectivo de insumos de la invocación",
             "sin ser una segunda definición de las lecturas permitidas", "fuera de la sesión de Principal del fixture", "fuera de toda cesión",
             "el oráculo de C-24 nunca se entrega al Controller"],
}
FORBIDDEN_ALL = ["AGREED", "se concede la excepción", "queda vigente", "en producción", "se amplía", "se reponen los", "workspace-write",
                 "repo-write", "sube en", "cualquier rol puede", "otras unidades pueden", "invocaciones de planificación",
                 "para la planificación, la verificación y los negativos"]
FORBIDDEN = {
    "A6-1": ["varios instrumentos", "la receta cambia"],
    "A6-3": ["lee el worktree real", "con ref dedicada", "sobre un clon del fixture", "siguen siendo entradas privadas prohibidas",
             "con su argv y los bytes de su salida tal como constan", "normalizando el fin de línea", "con un registro simulado"],
    "A6-4": ["comparado por blob"],
    "A6-5": ["también la deja en `not_run`", "es FAIL si el rechazo", "dan REJECT_FACTS", "Coordinator rechaza el VERIFIED",
             "R1, R2, R3, R4, R9 y R10 están en `MATCH`", "el instrumento es obligatorio", "R4 siempre en `MATCH`", "se reejecuta aunque registre S-04"],
    "A6-6": ["se entrega al Controller antes", "con el instrumento, sin el aceptante"],
    "A6-7": ["Su tope es dos", "reutiliza el bloque", "usar el bloque agotado", "con un registro simulado", "basta un registro simulado"],
    "A6-8": ["A-6 concede", "se modifica ADR-0046"],
    "A6-9": ["una implementación de autor distinto", "escrita sin ver el código"],
}
PRODUCT_TOKENS = re.compile(r"(?i)\b(codex|claude|openai|anthropic|gpt-|opus\b|sonnet\b|haiku\b|chatgpt|gemini|google|copilot|mistral|llama\b|deepseek|grok\b|python)")
PRODUCT_EXEMPT = {"A6-1": ["**Codex, total**"]}
PROHIBITIONS_313 = ["no cambia la receta 16.4", "no cambia el adapter de la celda", "no cambia ningún esquema de salida",
                    "no cambia ningún presupuesto congelado", "no cambia los roles ni la autoridad del Controller",
                    "no cambia el comportamiento de producción fuera de la excepción propuesta",
                    "No cambia la planificación (PLAN) del Controller: sigue el procedimiento vigente, sin el instrumento ni el aceptante"]

# ---------------------------------------------------------------- estado y declaraciones
CLAIMS = [
    (r"A-6\s+(?:=|es|queda|está)\s+\**AGREED", "A-6 declarada AGREED"),
    (r"A6-\d\s*=\s*\**AGREED", "un delta declarado AGREED"),
    (r"Architect\s*=\s*\**AGREED", "Architect = AGREED"),
    (r"Coordinator\s*=\s*\**AGREED", "Coordinator = AGREED"),
    (r"A-6\s+(?:queda\s+|está\s+|ya\s+)?aplicada", "A-6 declarada aplicada"),
    (r"FX-02\s*(?:=|es|queda)\s*\**PASS", "FX-02 PASS declarado"),
    (r"C-24\s*(?:=|es|queda)\s*\**PASS", "C-24 PASS declarado"),
    (r"F6 GATE PASS\s*(?:=|declarad|alcanzad|obtenid)", "F6 GATE PASS declarado"),
    (r"(?<!ninguna )excepción\s+(?:queda|está)\s+vigente", "excepción declarada vigente"),
    (r"(?<!sin )la excepción de ADR-0046 #9 (?:queda|está) concedida", "excepción declarada concedida"),
    (r"(?i)ronda 4\s*[:=]\s*\**(?!NOT\b)(?:AGREED|LAUNCHED|CHANGES REQUIRED|PASS)", "veredicto o lanzamiento de la ronda 4 declarado"),
    (r"(?i)(?<!ninguna )revisión (?:formal )?de la ronda 4 (?:lanzada|ejecutada|AGREED)", "revisión de la ronda 4 declarada lanzada"),
    (r"(?i)ronda 3\s*[:=]\s*\**(?:AGREED|PASS)", "ronda 3 declarada AGREED"),
    (r"(?i)ronda 2\s*[:=]\s*\**(?:AGREED|PASS)", "ronda 2 declarada AGREED"),
]
REQUEST_PATTERNS = [  # peticiones al Owner (se exige una negación en las 40 posiciones anteriores)
    r"(?i)\bse (?:pide|solicita|pedirá|solicitará)\b[^.\n]{0,60}\b(?:Owner|línea|determinación|autorización)",
    r"(?i)\b(?:pedimos|solicitamos|se requiere)\b[^.\n]{0,60}\bOwner",
    r"(?i)\b(?:una|otra) (?:línea|determinación|autorización) (?:nueva )?del Owner\b[^.\n]{0,40}\b(?:se pide|se solicita|hace falta|es necesaria)",
    r"(?i)\b(?:nueva línea|línea nueva) del Owner\b",
]
NEGATION = re.compile(r"(?i)(?:\bno\b|\bsin\b|\bni\b|\bningun[ao]?\b|\bningún\b|\bnunca\b)[^.\n]{0,40}$")
PLACEHOLDER = re.compile(r"PENDIENTE [A-Z][A-Z0-9_]+|@@[A-Z0-9_. ]+@@")

# ---------------------------------------------------------------- G9 (alcance de R5)
PLAN_TERM = re.compile(r"\bPLAN\b|planificaci[oó]n")
PLAN_NEG = re.compile(r"(?i)\b(?:no|sin|nunca|ningun[ao]?|ningún|excluid[oa]s?|fuera)\b")
G9_SITES = [  # (delta, regla, frase exigida)
    ("A6-1", 0, "solo en sus invocaciones VERIFY"),
    ("A6-1", 1, "en sus invocaciones VERIFY de esta topología (la verificación y los controles negativos con invocación, que se ejecutan con "
                "`CONTROLLER_VERIFICATION` y devuelven `controller-verification/v2`)"),
    ("A6-1", 2, "la planificación (PLAN) no usa el instrumento"),
    ("A6-1", 2, "sin el instrumento, sin su objeto en el cierre y sin el aceptante"),
    ("A6-1", 2, "ninguna regla de A-6 se aplica a ella"),
    ("A6-1", 6, "los insumos que esta regla añade a una invocación VERIFY"),
    ("A6-7", 1, "la celda solo es elegible con el instrumento, y solo para las invocaciones VERIFY de esta topología"),
    ("A6-7", 1, "La medición no demuestra ninguna operación de la planificación, que no usa el instrumento"),
    ("A6-8", 0, "usar ese instrumento y su aceptante en la verificación del Controller de FX-02"),
    ("A6-8", 1, "la celda de EXECUTION_CONTROLLER en su verificación, es decir, en sus invocaciones VERIFY"),
    ("A6-8", 1, "Quedan excluidos de forma expresa la planificación (PLAN)"),
]
G9_SOURCES = [  # (fuente, línea, frases que la línea fijada contiene)
    ("AP", 1236, ["| EXECUTION_CONTROLLER / PLAN | `rackcad-delegation/v2` | la planificación de 16.5 |"]),
    ("AP", 1237, ["| EXECUTION_CONTROLLER / VERIFY | `rackcad-controller-verification/v2` | la verificación de 16.9 |"]),
    ("AP", 1240, ["PLAN y VERIFY no son intercambiables."]),
    ("RAE", 220, ["Los ejecuta el Controller Codex real con `CONTROLLER_VERIFICATION`"]),
    ("KIT", 19, ["| Invocación | `Action` | `OutputContract` (AP 16.23) |"]),
    ("KIT", 21, ["| planificación | PLAN | `rackcad-delegation/v2` |"]),
    ("KIT", 22, ["| verificación | VERIFY | `rackcad-controller-verification/v2` |"]),
    ("KIT", 23, ["| nc1, nc2, nc3, N4, N5, N6, N7a, N7b | VERIFY (mismo prompt", "`rackcad-controller-verification/v2`"]),
]
G9_QUOTED = [("AP", 1237), ("AP", 1240), ("RAE", 220)]   # líneas que §2 debe citar literalmente
OWNER_A_SCOPE = "puede usar en su verificación"
EXTENSION = re.compile(r"(?i)\bampli(?:ar|a|an|ación|aciones|ada|ado)\b")

# ---------------------------------------------------------------- G10 (resultado del aceptante)
STATE_TOKENS = re.compile(r"\b[PS]-\d{2}\b|\b(?:INVALID|EXECUTION)_[A-Z_]+\b|\bnot_run\b|\b(?:PASS|FAIL|VERIFIED|UNVERIFIED|STOP|BLOCKED|REWORK)\b")
ACCEPTOR_TOKENS = {"ACCEPT_FACTS", "REJECT_FACTS", "MATCH", "MISMATCH", "NOT_APPLICABLE", "MANUAL"}
CORRESPONDENCE = [  # (regla de A6-5, frase de A-6, fuente, línea, frase de la línea fijada)
    (1, "queda sin establecer (`not_run`; AUTOMATION_PLAN 16.22)", "AP", 1204, "es `not_run`, y `not_run` cuenta como fail"),
    (1, "es evidencia contradictoria", "AP", 687, "| S-04 | contradictory evidence | STOP |"),
    (1, "`Disposition` STOP y S-04 en `TriggeredStopConditions`", "AP", 687, "| S-04 | contradictory evidence | STOP |"),
    (4, "INVALID_OUTPUT de 16.9 (nunca VERIFIED; transporte BLOCKED)", "AP", 653, "(`INVALID_OUTPUT`) o Controller (`Handoff`) | nunca VERIFIED; transporte `BLOCKED`"),
    (4, "de 16.23 (P-19: no avanza)", "AP", 1251, "| P-19 | salida de rol inválida o incompleta | no avanza;"),
    (4, "como máximo dos por (`TaskId`, fase) y dos por control negativo; la tercera → STOP (P-04)", "AP", 610,
     "como máximo dos reejecuciones por (`TaskId`, fase), y por control negativo; la tercera → STOP (P-04)"),
    (4, "reejecución con `RunId` nuevo", "RAE", 225, "reejecutable con `RunId` nuevo hasta dos veces por control"),
    (4, "en un control negativo, además control no superado", "RAE", 226, "STOP (P-04) y control no superado."),
    (5, "FX-02 solo es PASS con salidas ingeridas (aceptadas, o sin ejecuciones por la regla 7) y conformes al oráculo sellado", "V14", 2534,
     "| **PASS** | ejecutado y conforme al oráculo |"),
    (5, "FAIL sigue siendo una violación observada", "V14", 2535, "| **FAIL** | violación observada;"),
    (5, "FX-02 queda UNVERIFIED con esa causa (registro incompleto)", "V14", 2536, "| **UNVERIFIED** | falta una precondición (OD, capacidad sin medir, registro incompleto) |"),
    (6, "ambigüedad material (S-03), con STOP", "AP", 686, "| S-03 | material ambiguity | STOP |"),
    (2, "para R4, `fail` y `not_run` son por igual distintos de `pass` (16.22)", "AP", 1204, "es `not_run`, y `not_run` cuenta como fail"),
    (2, "sin ella si una comprobación anterior en `fail` la dejó sin entrada", "AP", 623,
     "salvo el causado porque una comprobación anterior en `fail` dejó sin entrada a la comprobación"),
    (4, "como evidencia contradictoria (S-04), con STOP", "AP", 687, "| S-04 | contradictory evidence | STOP |"),
    (7, "en nc1 (`CurrentSha` inexistente)", "RAE", 228, "entrega con `CurrentSha` inexistente → `Identity`"),
    (7, "En N4 (entrega ausente)", "V14", 2441, "N4 handoff ausente → BLOCKED"),
]
REQ_KEY_TERMS = {  # entrada de la solicitud del aceptante -> términos que la nombran en A6-3, reglas 5-6
    "Repo": ["sobre el mismo repositorio"], "Tool": ["el instrumento desde el mismo objeto"], "Interpreter": ["con el intérprete declarado"],
    "Runs": ["`Runs`"], "Verification": ["`Verification`"], "Closure": ["`Closure` = el cierre"], "RelayFacts": ["`RelayFacts`", "relevo"],
    "NotApplicable": ["`NotApplicable`"],
}

# ---------------------------------------------------------------- G11 (fuentes del revisor)
G11_REF = re.compile(r"\b(V14|AP|RAE|ADR-0046|ADR-0048|LIFECYCLE|DEC|A-[1-6]) (L\d+(?:-L\d+)?(?:(?:, | y )L\d+(?:-L\d+)?)*)( en adelante)?")
G11_PATHREF = re.compile(r"(?<![\w/.-])((?:docs|src|tests)/[^\s`:()]+?\.(?:md|py|json|txt)|[\w.-]+\.(?:md|py|json|txt)):(\d+)(?:-(\d+))?(?![\w])")
G11_DECSEC = re.compile(r"(?i)\bdecisiones (§\d+(?:(?:-| y |, )§\d+)*)")
G11_EVSEC = re.compile(r"(?i)\bevidencia (§\d+(?:(?:-| y |, )§\d+)*)")
G11_REQUIRED = [("V14", 2333, 2334)]
G11_REQUIRED_REFS = [AE + "/README.md:219-220", KITCC + ":23"]   # fuentes añadidas en la ronda 4 (§2), además de la revisión de la ronda 3
REPO_PATH = re.compile(r"`((?:docs|src|tests)/[^`\s]+)`")
GUARD_SOURCES = [  # líneas de las fuentes en que se apoyan G9 y G10 (además de las que cita A-6), para el cierre del revisor
    (AP, FROZEN[AP], [(1236, 1237), (1240, 1240), (610, 611), (623, 623), (625, 625), (653, 653), (686, 687), (1204, 1204), (1251, 1251)]),
    (RAE, FROZEN[RAE], [(220, 220), (225, 226), (228, 228)]),
    (V14, FROZEN[V14], [(2441, 2441), (2534, 2536)]),
    (KITCC, FROZEN[KITCC], [(19, 23)]),
    (ACC_NOTES, None, [(41, 42)]),
    (ACC, ACC_BLOB, None),   # rangos calculados del árbol sintáctico: constantes y funciones que lee G10
]
ACC_FUNCS = ["check_r5", "check_r6", "check_r7", "_compare_r5", "_compare_r7", "_run_comparisons", "check_r8", "check_r10", "check_r2",
             "check_r1", "check_r9", "accept", "main", "check_r3", "check_r4"]
ACC_CONSTS = ["CHECK_IDS", "WAIVABLE", "R5_COMMANDS", "R7_COMMANDS", "COMPARED_COMMANDS", "R4_CHECKS"]


def acceptor_ranges(src):
    mod = ast.parse(src)
    out = []
    for n in mod.body:
        if isinstance(n, ast.Assign) and isinstance(n.targets[0], ast.Name) and n.targets[0].id in ACC_CONSTS:
            out.append([n.lineno, n.end_lineno])
        if isinstance(n, ast.FunctionDef) and n.name in ACC_FUNCS:
            out.append([n.lineno, n.end_lineno])
    return sorted(out)
BASENAME_DIRS = [BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/", R1_REVIEW_DIR + "/", R2_REVIEW_DIR + "/", R3_REVIEW_DIR + "/",
                 RULING_DIR + "/", EVID]

# ---------------------------------------------------------------- privacidad
PRIVACY = re.compile(r"(?i)(?:\b[a-z]:|(?<![\w.])/(?:mnt/)?[a-z])[\\/]+users[\\/]+[^\\/\s%<*]|\b[\w.+-]+@[\w-]+\.[a-z]{2,}\b")
PRIVACY_OK = {"fixture@example.invalid", "neg@example.invalid", "synth@example.invalid", "banco-r4@example.invalid"}


def host_identities(repo):
    """Nombre de usuario (también su forma corta 8.3), correo y nombre Git del host, leídos en tiempo de ejecución; nunca se escriben."""
    ids = [os.environ.get("USERNAME", ""), os.environ.get("USER", ""), os.path.basename(os.environ.get("USERPROFILE", "").rstrip("\\/"))]
    for var in ("TEMP", "TMP"):
        m = re.search(r"(?i)[\\/]users[\\/]([^\\/]+)", os.environ.get(var, ""))
        if m:
            ids.append(m.group(1))
    for key in ("user.email", "user.name"):
        try:
            r = subprocess.run(["git", "-C", repo, "config", key], capture_output=True)
            ids.append(r.stdout.decode("utf-8", "replace").strip() if r.returncode == 0 else "")
        except OSError:
            pass
    return sorted({i for i in ids if len(i) >= 3}, key=len, reverse=True)


def privacy_findings(name, text, ids):
    f = []
    hits = sorted(set(m.group(0) for m in PRIVACY.finditer(text)) - PRIVACY_OK)
    if hits:
        f.append("%s con una ruta de perfil o un correo (%d coincidencias)" % (name, len(hits)))
    low = text.lower()
    if any(re.search(r"(?<![\w.@-])" + re.escape(i.lower()) + r"(?![\w@-])", low) for i in ids):
        f.append("%s con una identidad del host (usuario, correo o nombre Git)" % name)
    return f


# ---------------------------------------------------------------- utilidades
def norm(s):
    return " ".join(s.split())


def lf(raw):
    return raw.replace(b"\r\n", b"\n")


def blob_id(raw):
    return hashlib.sha1(b"blob %d\0" % len(raw) + raw).hexdigest()


def text_blob(text):
    return blob_id(lf(text.encode("utf-8")))


def tree_hash(entries):
    """Identificador Git del árbol formado por [(modo, tipo, sha, nombre)] (orden de Git: los árboles se comparan con «/» final)."""
    def key(e):
        return e[3].encode("utf-8") + (b"/" if e[1] == "tree" else b"")
    body = b"".join((e[0].lstrip("0") if e[1] == "tree" else e[0]).encode("ascii") + b" " + e[3].encode("utf-8") + b"\0" + bytes.fromhex(e[2])
                    for e in sorted(entries, key=key))
    return hashlib.sha1(b"tree %d\0" % len(body) + body).hexdigest()


def quotes(text):
    """[(inicio, fin, profundidad)] de cada «…» equilibrado; None si no está equilibrado."""
    out, stack = [], []
    for i, ch in enumerate(text):
        if ch == "«":
            stack.append(i)
        elif ch == "»":
            if not stack:
                return None
            s = stack.pop()
            out.append((s, i, len(stack)))
    return None if stack else sorted(out)


def literal_match(quote, lines):
    frags = [norm(x) for x in quote.split("[…]")]
    hay = norm(" ".join(lines))
    pos = 0
    for fr in frags:
        i = hay.find(fr, pos)
        if i < 0:
            return False
        pos = i + len(fr)
    return True


def mask_fences(md):
    out, fence = [], False
    for line in md.split("\n"):
        if line.lstrip().startswith("```"):
            fence = not fence
            out.append(" " * len(line))
            continue
        out.append(" " * len(line) if fence else line)
    return "\n".join(out)


def fences(md):
    """[(inicio, fin, contenido)] de cada bloque ```text … ```."""
    return [(m.start(), m.end(), m.group(1)) for m in re.finditer(r"^```text\n(.*?)\n```", md, re.S | re.M)]


def header_block(md):
    m = re.search(r"^```text\n(.*?)\n```", md, re.S | re.M)
    return m.group(1) if m else ""


def section(md, start, stop_pat=r"^### |^## "):
    i = md.find("\n" + start)
    if i < 0:
        return None, None
    j = re.compile(stop_pat, re.M).search(md, i + 1 + len(start))
    return i + 1, (j.start() if j else len(md))


def quoted_paragraph(block):
    lines = [l[2:] if l.startswith("> ") else ("" if l == ">" else None) for l in block.split("\n")]
    q = "\n".join(l for l in lines if l is not None).strip()
    return q[1:-1] if q.startswith("«") and q.endswith("»") else q


def a6_rules(md):
    out = {}
    for rid in DELTAS:
        s, e = section(md, SEC[rid])
        if s is None:
            continue
        body = md[s:e]
        loc = para_starting(body, "Ubicación:")
        cov = para_starting(body, "Cubre:")
        bq = [l for l in body.split("\n") if l.startswith(">")]
        out[rid] = {"Paragraph": quoted_paragraph("\n".join(bq)), "Location": loc, "Covers": cov, "Start": s, "End": e, "Section": body,
                    "Block": "\n".join(bq), "BlockSpan": (md.find(bq[0], s), md.find(bq[-1], s) + len(bq[-1])) if bq else (s, s)}
    return out


def para_starting(body, lead):
    """Párrafo (líneas hasta la primera en blanco o citada, unidas con espacios) que empieza por lead."""
    ls = body.split("\n")
    for i, l in enumerate(ls):
        if l.startswith(lead):
            out = [l]
            for x in ls[i + 1:]:
                if not x.strip() or x.startswith(">"):
                    break
                out.append(x)
            return " ".join(out)
    return ""


def rule_items(paragraph):
    """{0: cabecera y cláusula de alcance, k: texto normalizado de la regla k} de un párrafo de A-6."""
    items, cur, buf = {}, 0, []
    for line in paragraph.split("\n"):
        m = re.match(r"^(\d+)\. ", line)
        if m:
            items[cur] = norm(" ".join(buf))
            cur, buf = int(m.group(1)), [line[m.end():]]
        else:
            buf.append(line)
    items[cur] = norm(" ".join(buf))
    return items


def covers_points(line):
    m = re.match(r"^Cubre: puntos? ([0-9, y]+) del dictamen", line)
    return sorted({int(x) for x in re.findall(r"\d+", m.group(1))}) if m else []


def covers_findings(line, rnd=1):
    """Hallazgos de la ronda rnd que nombra la línea «Cubre» (segmentos separados por «;» que terminan en «de la ronda N»)."""
    m = re.search(r"\bdel dictamen(.*)$", line)
    tail = m.group(1) if m else ""
    out = set()
    for seg in tail.split(";"):
        mm = re.search(r"de la ronda (\d)\b", seg)
        if mm and int(mm.group(1)) == rnd:
            out |= set(re.findall(r"\b([RO][1-9])\b", seg))
    return out


def table_rows(md, start, stop=r"^## |^### "):
    s, e = section(md, start, stop)
    if s is None:
        return None
    rows = []
    for line in md[s:e].split("\n"):
        if line.startswith("|") and not re.match(r"^\|\s*-", line):
            rows.append([c.strip() for c in line.strip().strip("|").split("|")])
    return rows[1:] if rows else []


def a2_section(md, title):
    m = re.search(r"^### %s\n(.*?)(?=^### |^## )" % re.escape(title), md, re.S | re.M)
    return m.group(1) if m else ""


def a4_paragraphs(a4):
    paras = []
    for m in re.finditer(r"^### 3\.(\d) (A4-\d) — [^\n]*\n(.*?)(?=^### |^## )", a4, re.S | re.M):
        paras.append((m.group(2), quoted_paragraph("\n".join(l for l in m.group(3).split("\n") if l.startswith(">")))))
    return paras


def a5_paragraphs(a5):
    out = {}
    for rid, head in (("A5-1", "### 3.2 A5-1 — "), ("A5-2", "### 3.3 A5-2 — ")):
        s, e = section(a5, head)
        if s is not None:
            out[rid] = quoted_paragraph("\n".join(l for l in a5[s:e].split("\n") if l.startswith(">")))
    return out


_VB_MEMO = {}


def virtual_base(v14, a2, a4, a5):
    """V14 + A-2 + A-4 + A-5 aplicadas como en a2/a4/a5-guards.py (añadidos al final de D.3 y de D.1); memorizada por contenido."""
    key = (hash(v14), hash(a2), hash(a4), hash(a5), len(v14), len(a2), len(a4), len(a5))
    if key in _VB_MEMO:
        return _VB_MEMO[key]
    p1 = quoted_paragraph("\n".join(l for l in a2_section(a2, "2.3 Delta exacto").split("\n") if l.startswith(">")))
    p2 = quoted_paragraph("\n".join(l for l in a2_section(a2, "3.3 Delta exacto").split("\n") if l.startswith(">")))
    t = v14.replace(ANCHOR_V14, ANCHOR_V14 + "\n\n" + p1 + "\n\n" + p2, 1)
    paras = a4_paragraphs(a4)
    t = t.replace(ANCHOR_A2_END, ANCHOR_A2_END + "\n\n" + "\n\n".join(p for _, p in paras), 1)
    a5p = a5_paragraphs(a5)
    t = t.replace(ANCHOR_D1 + "\n", ANCHOR_D1 + "\n" + "\n" + a5p.get("A5-1", "") + "\n", 1)
    t = t.replace(ANCHOR_A46_END, ANCHOR_A46_END + "\n\n" + a5p.get("A5-2", ""), 1)
    _VB_MEMO[key] = (t, [r for r, _ in paras], sorted(a5p))
    return _VB_MEMO[key]


def phrase_regex(phrase):
    """Expresión que casa la frase en el texto de A-6 con cualquier salto de línea y marcador «> » entre palabras."""
    return r"(?:\s|>)+".join(re.escape(w) for w in phrase.split())


def neg_before(text, pos):
    return bool(NEGATION.search(text[max(0, pos - 48):pos]))


class Git:
    def __init__(self, repo):
        self.repo = repo

    def __call__(self, *a, check=True, raw=False):
        r = subprocess.run(["git", "-C", self.repo] + list(a), capture_output=True)
        if check and r.returncode != 0:
            raise RuntimeError("git %s: código %d" % (a[0], r.returncode))
        if r.returncode != 0:
            return None
        return r.stdout if raw else r.stdout.decode("utf-8")

    def show(self, rev, path):
        out = self("show", "%s:%s" % (rev, path), check=False)
        return out.replace("\r\n", "\n") if out is not None else None

    def cat(self, obj):
        return self("cat-file", "-p", obj).replace("\r\n", "\n")

    def cat_raw(self, obj):
        return self("cat-file", "blob", obj, raw=True)

    def obj(self, rev, path):
        out = self("rev-parse", "-q", "--verify", "%s:%s" % (rev, path.rstrip("/")), check=False)
        return out.strip() if out else None

    def exists(self, sha):
        return self("cat-file", "-e", sha, check=False) is not None

    def batch(self, specs):
        """{especificación: identificador o None} en un solo proceso (`cat-file --batch-check`); «rev:ruta», nombres o abreviaturas."""
        specs = list(dict.fromkeys(specs))
        if not specs:
            return {}
        data = "".join(s + "\n" for s in specs).encode("utf-8")
        r = subprocess.run(["git", "-C", self.repo, "cat-file", "--batch-check=%(objectname)"], input=data, capture_output=True)
        lines = r.stdout.decode("utf-8", "replace").split("\n")
        out = {}
        for s, l in zip(specs, lines):
            l = l.strip()
            out[s] = l if re.fullmatch(r"[0-9a-f]{40}", l) else None
        return out

    def objs(self, rev, paths):
        b = self.batch(["%s:%s" % (rev, p.rstrip("/")) for p in paths])
        return {p: b.get("%s:%s" % (rev, p.rstrip("/"))) for p in paths}

    def resolve(self, short):
        out = self("rev-parse", "-q", "--verify", short + "^{object}", check=False)
        return out.strip() if out else None

    def ls_top(self, rev, path):
        raw = self("ls-tree", "-z", "%s:%s" % (rev, path.rstrip("/")), check=False, raw=True)
        out = []
        for rec in (raw or b"").split(b"\0"):
            if rec:
                meta, name = rec.split(b"\t", 1)
                mode, typ, sha = meta.decode("ascii").split(" ")
                out.append((mode, typ, sha, name.decode("utf-8")))
        return out

    def ls_r(self, rev, path):
        raw = self("ls-tree", "-r", "-z", rev, "--", path, check=False, raw=True)
        out = {}
        for rec in (raw or b"").split(b"\0"):
            if rec:
                meta, name = rec.split(b"\t", 1)
                out[name.decode("utf-8")] = meta.decode("ascii").split(" ")[2]
        return out


def load_clause_map(git, td):
    path = os.path.join(td, "clause_map_pinned.py")
    with open(path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(git.cat(FROZEN[CLAUSE_MAP]))
    spec = importlib.util.spec_from_file_location("clause_map_pinned", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod, path


# ---------------------------------------------------------------- recogida de entradas (única parte que lee el repositorio)
HEX40 = re.compile(r"(?<![0-9A-Fa-f])[0-9a-f]{40}(?![0-9A-Fa-f])")
HEX64 = re.compile(r"(?<![0-9A-Fa-f])[0-9A-Fa-f]{64}(?![0-9A-Fa-f])")
HEX8 = re.compile(r"(?<![0-9A-Za-z_])(?=[0-9]*[a-f])[0-9a-f]{8}(?![0-9A-Za-z_…])")


def path_refs(text):
    return sorted({m.group(1) for m in G11_PATHREF.finditer(norm(text))})


def collect_common(git, base, head, a6_text):
    inp = {"Base": base, "Head": head}
    raws = {t: git.cat_raw(b) for t, (p, b) in SOURCES.items()}
    inp["Sources"] = {t: raw.decode("utf-8").replace("\r\n", "\n") for t, raw in raws.items()}
    inp["SourceBlobOk"] = {t: blob_id(raws[t]) == b for t, (p, b) in SOURCES.items()}
    inp["Sources"]["DEC"] = git.show(base, DEC) or ""
    inp["Sources"]["KIT"] = git.cat(FROZEN[KITCC])
    inp["DecPinned"] = git.cat(DEC_BLOB)
    prep = [p for t, (p, b) in SOURCES.items() if t != "A-6"] + [DEC]
    inp["PrepBlobs"] = git.objs(PREP_BASE, prep)
    inp["BaseBlobs"] = git.objs(base, [DEC, EVIDENCE_FILE])
    inp["Evidence"] = git.show(base, EVIDENCE_FILE) or ""
    inp["R1Text"] = inp["Sources"]["A-6"]          # texto de la ronda anterior (ronda 3): referencia de G5, G6 y de las reglas conservadas
    inp["R1Orig"] = git.cat_raw(R1_BLOB).decode("utf-8").replace("\r\n", "\n")   # texto de la ronda 1 (citas de la ronda 1)
    inp["R1CommitA6"] = git.obj(R1_COMMIT, A6)
    inp["R2CommitA6"] = git.obj(R2_COMMIT, A6)
    inp["Review2"] = json.loads(git.show(base, R2_REVIEW) or "null")
    inp["R3CommitA6"] = git.obj(R3_COMMIT, A6)
    inp["Review3"] = json.loads(git.show(base, R3_REVIEW) or "null")
    inp["BaseA6"] = git.obj(base, A6)
    inp["Review"] = json.loads(git.show(base, R1_REVIEW) or "null")
    inp["AccSrc"] = git.cat_raw(ACC_BLOB).decode("utf-8")
    inp["AccBlobOk"] = blob_id(git.cat_raw(ACC_BLOB)) == ACC_BLOB
    inp["AccNotes"] = git.show(base, ACC_NOTES) or ""
    sb = git.objs(base, sorted(set(SHORT_BLOBS.values())))
    inp["ShortAtBase"] = {pre: sb.get(p) for pre, p in SHORT_BLOBS.items()}
    for pre, (rev, p) in SHORT_AT_REV.items():
        inp["ShortAtBase"][pre] = git.obj(rev, p)
    paths = sorted(set(FROZEN) | set(FORBIDDEN_SURFACES))
    inp["Objs"] = {"Base": git.objs(base, paths), "Head": git.objs(head, paths)}
    inp["EvidTop"] = {"Base": git.ls_top(base, EVID), "Head": git.ls_top(head, EVID)}
    inp["EvidFiles"] = {"Base": git.ls_r(base, EVID), "Head": git.ls_r(head, EVID)}
    ents = [e for e in inp["EvidTop"]["Base"] if e[3] not in ("r2", "r3", "r4")]
    inp["ShortAtBase"][SHORT_EVID_R1] = tree_hash(ents) if ents else None
    inp["R2BaseFiles"] = {p[len(EVID_R2):]: b for p, b in inp["EvidFiles"]["Base"].items() if p.startswith(EVID_R2)}
    inp["R3BaseFiles"] = {p[len(EVID_R3):]: b for p, b in inp["EvidFiles"]["Base"].items() if p.startswith(EVID_R3)}
    fz = git.batch(["%s:%s" % (FREEZE_SHA, V14), "%s:%s" % (FREEZE_SHA, FREEZE), "%s:%s" % (V14_COMMIT, V14)])
    inp["FreezeCommit"] = {"V14": fz.get("%s:%s" % (FREEZE_SHA, V14)), "Record": fz.get("%s:%s" % (FREEZE_SHA, FREEZE)),
                           "V14AtCommit": fz.get("%s:%s" % (V14_COMMIT, V14))}
    inp["Ruling"] = json.loads(git.show(base, RULING) or "null")
    inp["E2E"] = json.loads(git.show(base, E2E) or "null")
    inp["PkgCommitBlobs"] = git.objs(PKG_COMMIT, [p for p, _ in INSTRUMENTS])
    inp["BlobSha256"] = {}
    for p, _ in INSTRUMENTS:
        o = inp["Objs"]["Base"].get(p)
        inp["BlobSha256"][p] = hashlib.sha256(git.cat_raw(o)).hexdigest() if o else None
    h40 = sorted(set(HEX40.findall(a6_text)))
    bx = git.batch(h40)
    inp["Hex40Exists"] = {h: bx.get(h) == h for h in h40}
    h8 = sorted(set(HEX8.findall(a6_text)))
    inp["Hex8Resolved"] = git.batch(h8)
    bn = {}
    for d in BASENAME_DIRS:
        for p in git.ls_r(base, d):
            bn.setdefault(p.rsplit("/", 1)[-1], []).append(p)
    inp["Basenames"] = {k: sorted(set(v)) for k, v in bn.items()}
    inp["PathFiles"] = {p: git.show(base, p) for p in (TOOL, ACC, SPEC, ACC_NOTES, KITCC, R1_REVIEW, R2_REVIEW, R3_REVIEW)}
    inp["RepoPaths"] = git.objs(base, sorted(set(REPO_PATH.findall(a6_text))))
    for p in path_refs(a6_text):
        cands = [p] if "/" in p else inp["Basenames"].get(p, [])
        for c in cands:
            inp["PathFiles"][c] = git.show(base, c)
    inp["R4Head"] = None
    inp["R4Local"] = None
    return inp


def file_ids(raw):
    raw = lf(raw)
    return {"Blob": blob_id(raw), "Sha256": hashlib.sha256(raw).hexdigest()}


def local_tree(d):
    out = {}
    for root, dirs, files in os.walk(d):
        dirs[:] = [x for x in dirs if not x.startswith("tmp") and x != "__pycache__"]
        for n in files:
            p = os.path.join(root, n)
            with open(p, "rb") as fh:
                out[os.path.relpath(p, d).replace("\\", "/")] = file_ids(fh.read())
    return out


def sanitize(text, *paths):
    """Quita de un texto de salida las rutas locales (repositorio, temporal y perfil); nunca se escriben."""
    for p in sorted({x for x in paths + (os.environ.get("USERPROFILE", ""), os.environ.get("TEMP", ""), os.environ.get("TMP", "")) if x},
                    key=len, reverse=True):
        for v in {p, p.replace("\\", "/"), p.replace("/", "\\")}:
            text = text.replace(v, "<local>")
    return text


HOST_IDS = []


def sanitize_obj(obj, paths):
    """Rutas locales -> <local> e identidades del host -> <usuario>, en todo texto de una salida."""
    if isinstance(obj, str):
        t = sanitize(obj, *paths)
        for i in HOST_IDS:
            t = re.sub(re.escape(i), "<usuario>", t, flags=re.I)
        return t
    if isinstance(obj, list):
        return [sanitize_obj(x, paths) for x in obj]
    if isinstance(obj, dict):
        return {sanitize(k, *paths): sanitize_obj(v, paths) for k, v in obj.items()}
    return obj


def run_c20b(git, cm_path, head, td):
    r = subprocess.run([sys.executable, "-I", "-B", cm_path, "check", BASE_RC, head, os.path.join(td, "c20b.json")], cwd=git.repo, capture_output=True)
    anc = git("merge-base", "--is-ancestor", BASE_RC, head, check=False) is not None
    out = sanitize((r.stdout + r.stderr).decode("utf-8", "replace").strip(), git.repo, td)
    return {"Base": BASE_RC, "BaseIsAncestor": anc, "ExitCode": r.returncode, "Output": out[-400:]}


def collect_commit(git, base, head, inp):
    rows = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", base, head).splitlines() if l]
    inp["Status"] = {r[-1]: r[0] for r in rows}
    inp["HeadParent"] = git("rev-parse", head + "^1").strip()
    inp["HeadTexts"] = {p: git.show(head, p) for p, s in inp["Status"].items() if s in ("A", "M")}
    inp["HeadObj"] = {p: git.obj(head, p) for p in (A6, GUARDS, SELFTEST)}
    inp["R4Head"] = {}
    for p, b in git.ls_r(head, EVID_R4).items():
        raw = git.cat_raw(b)
        inp["R4Head"][p[len(EVID_R4):]] = {"Blob": b, "Sha256": hashlib.sha256(raw).hexdigest()}


def r2_ids(inp):
    """Identificadores de los archivos del paquete r4/ disponibles (cabeza del commit o paquete local)."""
    out = {}
    for src in (inp.get("R4Head"), inp.get("R4Local")):
        for p, v in (src or {}).items():
            out.setdefault(p, v)
    return out


# ---------------------------------------------------------------- §2: bloques literales
def literal_blocks(md):
    """([(etiqueta, a, b, líneas citadas, (inicio, fin))], hallazgos de estructura) de los bloques literales de §2."""
    f, blocks = [], []
    s2, e2 = section(md, "## 2. ", r"^## ")
    if s2 is None:
        return blocks, ["falta §2"]
    claimed = []
    for m in BLOCK_HEADER.finditer(md, s2, e2):
        tag = m.group(1)
        ranges = [(int(m.group(2)), int(m.group(3)))] + [(int(a), int(b)) for a, b in re.findall(r"L(\d+)-L(\d+)", m.group(4) or "")]
        pos = m.end()
        for a, b in ranges:
            nxt = BLOCK_HEADER.search(md, pos, e2)
            limit = nxt.start() if nxt else e2
            k = md.find("\n>", pos, limit)
            if k < 0:
                f.append("bloque %s L%d-L%d sin líneas citadas" % (tag, a, b))
                continue
            gap = md[pos:k]
            if gap.strip() and not gap.strip().endswith(":") and gap.strip() != ")":
                gap_lines = [l for l in gap.split("\n") if l.strip()]
                if any(not l.rstrip().endswith(":") for l in gap_lines[1:]):
                    f.append("bloque %s L%d-L%d: texto entre la cabecera y la cita" % (tag, a, b))
            lines, p = [], k + 1
            while p < len(md) and md.startswith(">", p):
                eol = md.find("\n", p)
                eol = len(md) if eol < 0 else eol
                lines.append(md[p:eol])
                p = eol + 1
            blocks.append((tag, a, b, [l[2:] if l.startswith("> ") else ("" if l == ">" else l) for l in lines], (k + 1, p)))
            claimed.append((k + 1, p))
            pos = p
    # todo bloque «> » de §2 debe tener su cabecera literal
    for m in re.finditer(r"(?m)^>.*(?:\n>.*)*", md[s2:e2]):
        st = s2 + m.start()
        if not any(a <= st < b for a, b in claimed):
            f.append("bloque citado en §2 sin cabecera «X La-Lb, literal»: «%s»" % m.group(0)[:50])
    for m in re.finditer(r"(?m)^(\S+) L(\d+)-L(\d+)[^\n]*, literal", md[s2:e2]):
        if not BLOCK_HEADER.match(m.group(0)):
            f.append("cabecera literal no reconocida: «%s»" % m.group(0)[:60])
    return blocks, f


# ---------------------------------------------------------------- G1
RULE_REF = re.compile(r"\b(A6-[1-9]), reglas? (\d+(?:(?:-| y |, )\d+)*)(?: \(([ivx]+|[a-e])\))?")


def rule_refs(md):
    """[(delta, [reglas], letra o None, contexto con las referencias enmascaradas)] de las referencias «A6-n, regla(s) k» fuera de los
    párrafos de los deltas. El contexto (80 posiciones antes y 20 después) sirve para reconocer el texto conservado de la ronda 1."""
    rules = a6_rules(md)
    spans = [v["BlockSpan"] for v in rules.values()]
    text = md
    for a, b in sorted(spans, reverse=True):
        text = text[:a] + " " * (b - a) + text[b:]
    t = norm(text)
    masked = RULE_REF.sub(lambda m: m.group(1) + ", regla #", t)
    out = []
    shift = 0
    for m in RULE_REF.finditer(t):
        nums = set()
        for part in re.split(r", | y ", m.group(2)):
            if "-" in part:
                a, b = part.split("-")
                nums |= set(range(int(a), int(b) + 1))
            else:
                nums.add(int(part))
        ms = m.start() + shift
        rep = m.group(1) + ", regla #"
        shift += len(rep) - len(m.group(0))
        out.append((m.group(1), sorted(nums), m.group(3), masked[max(0, ms - 80):ms + len(rep) + 20]))
    return out


def rule_map(r1_items, items):
    """{regla de la ronda 1: regla de la ronda 2 más parecida} para un delta."""
    out = {}
    for k, v in r1_items.items():
        if k and items:
            out[k] = max((kk for kk in items if kk), key=lambda kk: difflib.SequenceMatcher(None, v.split(), items[kk].split(), autojunk=False).ratio())
    return out


def generic_literal(inner, window, src_lines, inp):
    """Una cita «…» que no está en la lista fijada se acepta solo si una referencia «TAG La[-Lb]» o «ruta:línea» de su entorno inmediato
    contiene literalmente su texto en la fuente fijada (V14, AP, RAE, ADR, LIFECYCLE, A-n por blob; decisiones en la base)."""
    q = norm(inner)
    if len(q) < 3:
        return False
    for m in G11_REF.finditer(window):
        tag = m.group(1)
        lines = src_lines.get(tag)
        if lines is None:
            continue
        for a, b in re.findall(r"L(\d+)(?:-L(\d+))?", m.group(2)):
            a, b = int(a), int(b or a)
            seg = lines[a - 1:b]
            if tag in BQ or tag == "A-6":
                seg = [l[2:] if l.startswith("> ") else l for l in seg]
            if seg and literal_match(q, seg):
                return True
    for m in G11_PATHREF.finditer(window):
        p, a = m.group(1), int(m.group(2))
        b = int(m.group(3)) if m.group(3) else a
        cands = [p] if "/" in p else (inp.get("Basenames") or {}).get(p, [])
        text = (inp.get("PathFiles") or {}).get(cands[0]) if len(cands) == 1 else None
        if text is not None and literal_match(q, text.split("\n")[a - 1:b]):
            return True
    return False


def g1_literals(inp, ids):
    f, checked = [], {"Header": 0, "Blocks": 0, "BlockLines": 0, "Lit": 0, "LitByRef": 0, "Self": 0, "Own": 0, "Notation": 0, "DeltaInner": 0,
                      "ShortBlobs": 0, "Hex8": 0, "Hex40": 0, "Hex64": 0, "RuleRefs": 0}
    md = inp["A6"] or ""
    hb = header_block(md)
    for fact in HEADER_EXACT:
        checked["Header"] += 1
        if fact not in hb:
            f.append("cabecera sin «%s»" % fact.replace("\n", " / ")[:80])
    for fact in HEADER_NORM:
        checked["Header"] += 1
        if norm(fact) not in norm(hb):
            f.append("cabecera sin «%s»" % fact[:80])
    for pat in HEADER_FORBIDDEN:
        if re.search(pat, hb):
            f.append("cabecera con «%s» (prohibido en la ronda 4)" % pat)
    fc = inp.get("FreezeCommit") or {}
    if fc and (fc.get("V14") != FROZEN[V14] or fc.get("Record") != FROZEN[FREEZE] or fc.get("V14AtCommit") != FROZEN[V14]):
        f.append("los blobs de V14 y del registro en el commit del Freeze no son los fijados")
    if inp.get("R1CommitA6") != R1_BLOB:
        f.append("el commit %s no contiene la ronda 1 (blob %s)" % (R1_COMMIT[:8], R1_BLOB[:8]))
    if inp.get("R2CommitA6") != R2_BLOB:
        f.append("el commit %s no contiene la ronda 2 (blob %s)" % (R2_COMMIT[:8], R2_BLOB[:8]))
    if inp.get("R3CommitA6") != R3_BLOB:
        f.append("el commit %s no contiene la ronda 3 (blob %s)" % (R3_COMMIT[:8], R3_BLOB[:8]))
    bad_src = sorted(t for t, ok in (inp.get("SourceBlobOk") or {}).items() if not ok)
    if bad_src:
        f.append("fuentes cuyo contenido no da su blob fijado: %s" % bad_src)
    pb = inp.get("PrepBlobs") or {}
    bad_prep = sorted(p for t, (p, b) in SOURCES.items() if t != "A-6" and pb.get(p) != b)
    if pb.get(DEC) != DEC_BLOB:
        bad_prep.append(DEC)
    if bad_prep:
        f.append("en %s (commit declarado de los literales) los blobs no son los fijados: %s" % (PREP_BASE[:8], bad_prep[:4]))
    src_lines = {t: inp["Sources"][t].split("\n") for t in inp["Sources"]}
    masked = mask_fences(md)
    blocks, bf = literal_blocks(md)
    f += bf
    block_spans = [x[4] for x in blocks]
    for tag, a, b, got, _ in blocks:
        want = src_lines[tag][a - 1:b]
        checked["Blocks"] += 1
        checked["BlockLines"] += len(got)
        if got != want:
            bad = [a + i for i, (x, y) in enumerate(zip(got, want)) if x != y] or ["longitud %d frente a %d" % (len(got), len(want))]
            f.append("bloque %s L%d-L%d distinto de la fuente (líneas %s)" % (tag, a, b, bad[:4]))
    rules = a6_rules(md)
    deltas = {r: v["Paragraph"] for r, v in rules.items()}
    dspans = [v["BlockSpan"] for v in rules.values()]
    # ninguna línea «> » fuera de §2 y de los deltas
    for m in re.finditer(r"(?m)^>", masked):
        if not any(a <= m.start() < b for a, b in block_spans + dspans):
            eol = masked.find("\n", m.start())
            f.append("línea citada («> ») fuera de los bloques literales de §2 y de los deltas: «%s»" % masked[m.start():eol][:50])
            break
    qs = quotes(masked)
    if qs is None:
        f.append("comillas angulares desequilibradas en A-6")
        qs = []
    lit_by_text = {}
    for row in LIT:
        lit_by_text.setdefault(norm(row[3]), []).append(row)
    inner_lit = {norm(k): v for k, v in DELTA_INNER_LIT.items()}
    own = {norm(x) for x in OWN_TERMS}
    owner_a = owner_lines(inp["R1Text"]).get("A6-ADR0046-9-FX02", "")
    r1n = norm(inp.get("R1Orig") or "")
    unclassified = []
    for s, e, depth in qs:
        if any(a <= s < b for a, b in block_spans):
            continue
        inner = md[s + 1:e]
        in_delta = any(a <= s < b for a, b in dspans)
        if in_delta:
            if depth == 0:
                continue
            key = norm(inner.replace("\n> ", "\n").replace("\n>", "\n"))
            if key in inner_lit:
                checked["DeltaInner"] += 1
                tag, a, b = inner_lit[key]
                if not literal_match(key, src_lines[tag][a - 1:b]):
                    f.append("cita interna del delta distinta de %s L%d: «%s»" % (tag, a, key[:50]))
            elif key in own:
                checked["Own"] += 1
            else:
                unclassified.append("dentro del delta: «%s»" % inner[:50])
            continue
        if depth != 0:
            continue
        if norm(inner) in NOTATION:
            checked["Notation"] += 1
            continue
        rows = lit_by_text.get(norm(inner))
        if rows:
            ok_any = False
            for tag, a, b, text, ref in rows:
                if tag == "OWNER_A":
                    ok_any = norm(text) in norm(owner_a)
                elif tag == "A6R1":
                    ok_any = norm(text) in r1n
                else:
                    lines = src_lines[tag][a - 1:b]
                    if tag in BQ:
                        lines = [l[2:] if l.startswith("> ") else l for l in lines]
                    if tag == "DEC":
                        pinned = inp["DecPinned"].split("\n")[a - 1:b]
                        if lines != pinned:
                            f.append("decisiones: L%d-L%d ya no es la línea fijada (el archivo no creció solo por añadido)" % (a, b))
                    if literal_match(text, lines):
                        win = norm(md[max(0, s - 260):e + 260])
                        ok_any = ref is None or ref in win
                if ok_any:
                    break
            if ok_any:
                checked["Lit"] += 1
            else:
                f.append("cita literal sin casar con su fuente o sin su referencia de línea: «%s»" % inner[:60])
            continue
        if any(norm(inner) in norm(d) for d in deltas.values()):
            checked["Self"] += 1
            continue
        if generic_literal(inner, norm(md[max(0, s - 200):e + 120]), src_lines, inp):
            checked["LitByRef"] += 1
            continue
        unclassified.append("«%s»" % inner[:60])
    if unclassified:
        f.append("citas sin clasificar (ni literal fijado, ni del propio delta, ni término propio, ni notación): %s" % unclassified[:6])
    r2 = r2_ids(inp)
    r2_blobs = {v["Blob"] for v in r2.values()}
    full_in_text = set(HEX40.findall(md))
    for m in re.finditer(r"(?<![0-9a-fA-F])([0-9a-f]{8})…", masked):
        pre = m.group(1)
        checked["ShortBlobs"] += 1
        if pre in NON_GIT_HASHES:
            continue
        if pre in SHORT_BLOBS:
            if not (inp["ShortAtBase"].get(pre) or "").startswith(pre):
                f.append("prefijo %s… distinto del objeto real de %s en la base" % (pre, SHORT_BLOBS[pre]))
        elif pre in SHORT_AT_REV or pre == SHORT_EVID_R1:
            if not (inp["ShortAtBase"].get(pre) or "").startswith(pre):
                f.append("prefijo %s… distinto del objeto real de la ronda anterior" % pre)
        elif not any(b.startswith(pre) for b in r2_blobs | {h for h in full_in_text if inp.get("Hex40Exists", {}).get(h)}):
            f.append("prefijo de blob sin fijar en las guardas ni en el paquete r4/: %s…" % pre)
    for h in sorted(set(HEX8.findall(masked)) | set(HEX8.findall(hb))):
        checked["Hex8"] += 1
        if not inp.get("Hex8Resolved", {}).get(h) and not any(b.startswith(h) for b in r2_blobs):
            f.append("identificador abreviado que no es un objeto del repositorio: %s" % h)
    for h in sorted(set(HEX40.findall(md))):
        checked["Hex40"] += 1
        if not inp.get("Hex40Exists", {}).get(h) and h not in r2_blobs:
            f.append("identificador de 40 hexadecimales que no es un objeto del repositorio ni del paquete r4/: %s" % h[:12])
    dec = inp["Sources"].get("DEC") or ""
    r2_sha = {v["Sha256"] for v in r2.values()}
    for h in sorted(set(x.lower() for x in HEX64.findall(md))):
        checked["Hex64"] += 1
        kind = KNOWN_SHA256.get(h)
        if kind is None:
            if h not in r2_sha and h not in dec.lower() and h not in (inp.get("Evidence") or "").lower():
                f.append("SHA-256 sin fijar en las guardas, ni del paquete r4/, ni registrado en decisiones o evidencia: %s" % h[:16])
        elif kind[0] == "blob" and inp["BlobSha256"].get(kind[1]) != h:
            f.append("SHA-256 distinto del contenido de %s en la base: %s" % (kind[1], h[:16]))
        elif kind[0] == "dec" and kind[1] not in dec:
            f.append("valor %s… ausente del registro de decisiones en la base" % h[:12])
        elif kind[0] == "owner_a" and hashlib.sha256(owner_a.encode("utf-8")).hexdigest() != h:
            f.append("SHA-256 de la línea (a) distinto: %s" % h[:16])
    # referencias «A6-n, regla k»: la regla existe; en el texto conservado de la ronda 1, es la misma regla
    items = {r: rule_items(v["Paragraph"]) for r, v in rules.items()}
    r1_items = {r: rule_items(v["Paragraph"]) for r, v in a6_rules(inp["R1Text"]).items()}
    r1_refs = {}
    for d, nums, letter, ctx in rule_refs(inp["R1Text"]):
        r1_refs.setdefault((d, ctx), nums)
    maps = {}
    for d, nums, letter, ctx in rule_refs(md):
        checked["RuleRefs"] += 1
        for k in nums:
            if k not in items.get(d, {}) or k == 0:
                f.append("referencia a %s, regla %d, que no existe (%s tiene %d reglas): «…%s…»" % (d, k, d, max(items.get(d, {0: 0})), ctx[-70:]))
            elif letter and "(%s)" % letter not in items[d][k]:
                f.append("referencia a %s, regla %d (%s), sin ese apartado" % (d, k, letter))
        old = r1_refs.get((d, ctx))
        if old is not None and d in items and d in r1_items:
            if d not in maps:
                maps[d] = rule_map(r1_items[d], items[d])
            want = sorted({maps[d].get(k, k) for k in old})
            if want != nums:
                f.append("texto conservado de la ronda 3 cita %s, regla %s; la ronda 3 citaba la %s, que en la ronda 4 es la %s: «…%s…»"
                         % (d, nums, old, want, ctx[-80:]))
    priv = ["A-6"]
    f += privacy_findings("A-6", md, ids)
    for name, text in sorted((inp.get("PkgTexts") or {}).items()):
        priv.append(name)
        f += privacy_findings(name, text, ids)
    return {"Check": "G1 cabecera, literales, citas, identificadores, referencias a reglas y privacidad", "Checked": checked,
            "PrivacyChecked": len(priv), "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G2
def g2_paths(inp, ids):
    f = []
    st = inp.get("Status")
    if st is None:
        return {"Check": "G2 rutas del commit", "Result": "NOT_RUN", "Info": ["sin commit candidato"], "Findings": []}
    if inp.get("HeadParent") != inp["Base"]:
        f.append("base distinta del padre de head")
    if st.get(A6) != "M":
        f.append("%s no aparece como modificado (M): %s" % (A6, st.get(A6)))
    if inp.get("BaseA6") != PREV_BLOB:
        f.append("%s en la base no es la ronda 3 (blob %s…)" % (A6, PREV_BLOB[:8]))
    if inp.get("HeadObj", {}).get(A6) != text_blob(inp["A6"] or ""):
        f.append("blob de A-6 en head distinto del texto comprobado")
    for p, s in sorted(st.items()):
        if p == A6:
            continue
        if p.startswith(EVID_R4):
            if s != "A":
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID_R4))
        else:
            f.append("ruta fuera de alcance: %s (%s)" % (p, s))
    for p, s in sorted(st.items()):
        if s in ("A", "M"):
            f += privacy_findings(p, inp["HeadTexts"].get(p) or "", ids)
    return {"Check": "G2 rutas del commit", "Base": inp["Base"], "Head": inp["Head"], "Changed": st, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G3
def in_map_surfaces(p):
    return any(p == s or (s.endswith("/") and p.startswith(s)) for s in MAP_SURFACES)


def forbidden_list_in_a6(md):
    m = re.search(r"^\*\*Superficies prohibidas\*\*[^\n]*$", md, re.M)
    if not m:
        return None, False
    line = m.group(0)
    return re.findall(r"`([^`]+)`", line.replace(EVID_OUTSIDE_R2_PHRASE, "")), EVID_OUTSIDE_R2_PHRASE in line


def g3_surfaces(inp):
    f, mism = [], {}
    md = inp["A6"] or ""
    listed, outside = forbidden_list_in_a6(md)
    if listed is None:
        f.append("A-6 no declara la lista de superficies prohibidas (§6.1)")
    else:
        if len(listed) != len(set(listed)):
            f.append("lista de superficies prohibidas con duplicados")
        if set(listed) != set(FORBIDDEN_SURFACES):
            f.append("lista de superficies prohibidas distinta de la de las guardas: faltan %s; sobran %s"
                     % (sorted(set(FORBIDDEN_SURFACES) - set(listed))[:4], sorted(set(listed) - set(FORBIDDEN_SURFACES))[:4]))
        if not outside:
            f.append("la lista de superficies prohibidas no incluye «%s»" % EVID_OUTSIDE_R2_PHRASE)
    for p in FORBIDDEN_SURFACES:
        b, h = inp["Objs"]["Base"].get(p), inp["Objs"]["Head"].get(p)
        if b != h:
            mism["Base->Head:" + p] = {"Expected": b, "Got": h}
    for lab in ("Base", "Head"):
        got = inp["Objs"][lab]
        for p, w in sorted(FROZEN.items()):
            if got.get(p) != w:
                mism["%s:%s" % (lab, p)] = {"Expected": w, "Got": got.get(p)}
    f += ["objeto distinto en %s: esperado %s, obtenido %s" % (k, (v["Expected"] or "")[:12], (v["Got"] or "")[:12]) for k, v in sorted(mism.items())]
    ev = inp.get("EvidFiles") or {}
    rb = {p: b for p, b in (ev.get("Base") or {}).items() if not p.startswith(EVID_R4)}
    rh = {p: b for p, b in (ev.get("Head") or {}).items() if not p.startswith(EVID_R4)}
    if rb != rh:
        diff = sorted(set(rb.items()) ^ set(rh.items()))
        f.append("archivos de las rondas 1 a 3 de %s distintos entre base y head (fuera de r4/): %s" % (EVID, sorted({p for p, _ in diff})[:4]))
    for lab in ("Base", "Head"):
        ents = [e for e in (inp.get("EvidTop") or {}).get(lab) or [] if e[3] != "r4"]
        th = tree_hash(ents) if ents else None
        if th != EVID_R123_TREE:
            f.append("árbol de %s sin r4/ en %s distinto del de las rondas 1 a 3 (%s…): %s" % (EVID, lab, EVID_R123_TREE[:8], (th or "")[:12]))
    touched = sorted(p for p in (inp.get("Status") or {}) if in_map_surfaces(p) or any(p == s or (s.endswith("/") and p.startswith(s))
                                                                                      for s in FORBIDDEN_SURFACES)
                     or (p.startswith(EVID) and not p.startswith(EVID_R4)))
    if touched:
        f.append("rutas del commit bajo las superficies prohibidas, del mapa o de las rondas 1 a 3: %s" % touched[:6])
    r = inp["C20b"]
    if not r.get("BaseIsAncestor"):
        f.append("la base del mapa bb0d5522 no es ancestro de head")
    if not (r["ExitCode"] == 0 and "EQUAL" in r["Output"]):
        f.append("clause_map.py check distinto de EQUAL (código %s)" % r["ExitCode"])
    return {"Check": "G3 superficies prohibidas, objetos congelados, rondas 1 a 3 intactas y C-20b", "Listed": len(listed or []), "Frozen": len(FROZEN),
            "Mismatches": mism, "C20b": {"ExitCode": r["ExitCode"], "Output": r["Output"][-200:]}, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G4
def where_deltas(cell):
    return sorted(set(re.findall(r"\bA6-\d+\b", cell)))


def g4_ruling(inp):
    f, info = [], []
    md = inp["A6"] or ""
    ru = inp.get("Ruling") or {}
    pts = (ru.get("A6") or {}).get("MinimalDeltaScope") or []
    if ru.get("Verdict") != RULING_VERDICT or (ru.get("A6") or {}).get("Needed") != "yes":
        f.append("el dictamen en la base no es MATERIAL con A-6 necesaria")
    if len(pts) != 8:
        f.append("el dictamen no tiene ocho puntos en A6.MinimalDeltaScope (%d)" % len(pts))
    rows = table_rows(md, "### 3.2 ")
    rules = a6_rules(md)
    table_map = {}
    if rows is None:
        f.append("falta §3.2")
        rows = []
    nums = [r[0] for r in rows]
    if nums != [str(i) for i in range(1, 9)]:
        f.append("la tabla de §3.2 no tiene los puntos 1..8, una vez cada uno y en orden: %s" % nums)
    for r in rows:
        if len(r) != 3 or not r[0].isdigit():
            f.append("fila de §3.2 mal formada: %s" % r[:1])
            continue
        n = int(r[0])
        if 1 <= n <= len(pts) and r[1] != pts[n - 1]:
            f.append("punto %d de §3.2 distinto del literal del dictamen" % n)
        ds = re.findall(r"A6-\d", r[2])
        if not ds:
            f.append("punto %d sin delta" % n)
        for d in ds:
            if d not in rules:
                f.append("punto %d cita %s, que no existe en §3" % (n, d))
            table_map.setdefault(d, set()).add(n)
    for d in DELTAS:
        if d not in rules:
            continue
        cov = set(covers_points(rules[d]["Covers"]))
        if not cov:
            f.append("%s sin línea «Cubre:» con puntos del dictamen" % d)
        if cov != table_map.get(d, set()):
            f.append("%s: «Cubre» (%s) distinto de la tabla de §3.2 (%s)" % (d, sorted(cov), sorted(table_map.get(d, set()))))
    # revisión de la ronda 1
    rv = inp.get("Review") or {}
    req_ids = [x.get("FindingId") for x in rv.get("RequiredFindings") or []]
    opt_ids = [x.get("FindingId") for x in rv.get("OptionalFindings") or []]
    if req_ids != ["A62-A6-R%d" % i for i in range(1, 6)] or opt_ids != ["A62-A6-O%d" % i for i in range(1, 4)]:
        f.append("la revisión de la ronda 1 en la base no tiene exactamente A62-A6-R1..R5 y A62-A6-O1..O3: %s %s" % (req_ids, opt_ids))
    if rv.get("Verdict") != "CHANGES REQUIRED":
        f.append("la revisión de la ronda 1 en la base no es CHANGES REQUIRED")
    frows = table_rows(md, "### 3.16 ")
    if frows is None:
        f.append("falta §3.16 (correcciones de la ronda 2)")
        frows = []
    fmap = {}
    order = []
    for r in frows:
        m = re.match(r"^([RO][1-5]):", r[0]) if r else None
        if not m:
            continue
        order.append(m.group(1))
        if len(r) != 4:
            f.append("fila %s de §3.16 mal formada (%d columnas)" % (m.group(1), len(r)))
            continue
        fmap.setdefault(m.group(1), []).append(r)
    if order != R1_FINDINGS:
        f.append("§3.16 no tiene R1-R5 y O1-O3, una fila cada uno y en orden: %s" % order)
    cov_find = {d: covers_findings(v["Covers"]) for d, v in rules.items()}
    items = {d: rule_items(v["Paragraph"]) for d, v in rules.items()}
    r2 = r2_ids(inp)
    for fid in R1_FINDINGS:
        for r in fmap.get(fid, [])[:1]:
            corr, where, evid = r[1], r[2], r[3]
            if len(norm(corr)) < 20:
                f.append("%s: corrección vacía o insuficiente en §3.16" % fid)
            ds = where_deltas(where)
            if not ds:
                f.append("%s: la columna «Dónde» de §3.16 no nombra ningún delta" % fid)
            for d in ds:
                if d not in rules:
                    f.append("%s: «Dónde» cita %s, que no existe" % (fid, d))
            for m in re.finditer(r"\b(A6-[1-9]), reglas? (\d+(?:(?:-| y |, )\d+)*)", where):
                d = m.group(1)
                for part in re.split(r", | y ", m.group(2)):
                    lo, hi = (part.split("-") + [part])[:2] if "-" in part else (part, part)
                    for k in range(int(lo), int(hi) + 1):
                        if k not in items.get(d, {}):
                            f.append("%s: «Dónde» cita %s, regla %d, que no existe" % (fid, d, k))
            named = {d for d in ds if d in rules}
            covering = {d for d, c in cov_find.items() if fid in c}
            if named != covering:
                f.append("%s: «Dónde» de §3.16 (%s) distinto de los deltas cuya línea «Cubre» lo nombra (%s)" % (fid, sorted(named), sorted(covering)))
            f += evidence_paths(fid, evid, inp, info)
    f += g4_round2(inp, md, rules, info)
    f += g4_317_append(md, inp["R1Text"])
    f += g4_round3(inp, md, rules, info)
    # materialidad
    rv2 = inp.get("Review2") or {}
    conf2 = {m.get("Id"): m for m in rv2.get("MaterialityConfirmation") or []}
    mrows = table_rows(md, "## 5. ", r"^## ")
    mat = {m.get("Id"): m.get("Activated") for m in ru.get("Materiality") or []}
    conf = {m.get("Id"): m for m in rv.get("MaterialityConfirmation") or []}
    where_of = {fid: where_deltas(r[2]) for fid, rs in fmap.items() for r in rs[:1] if len(r) == 4}
    got = {}
    for r in mrows or []:
        if r and re.match(r"^M-0[1-8]$", r[0]):
            if r[0] in got:
                f.append("%s aparece más de una vez en la tabla M" % r[0])
            got[r[0]] = r
    if sorted(got) != ["M-0%d" % i for i in range(1, 9)]:
        f.append("la tabla M no tiene M-01..M-08 exactamente: %s" % sorted(got))
    for mid, r in sorted(got.items()):
        if len(r) != 6:
            f.append("%s: fila de la tabla M sin las seis columnas (M, Activado, Dictamen, Ronda 1, Ronda 2, Motivo)" % mid)
            continue
        c2 = conf2.get(mid) or {}
        want2 = "%s (%s)" % (c2.get("ReviewerAssessment"), c2.get("Agreement"))
        if r[4] != want2:
            f.append("%s: columna Ronda 2 «%s» distinta de MaterialityConfirmation de la revisión de la ronda 2 («%s»)" % (mid, r[4], want2))
        if c2.get("A6Declared") != ("yes" if mid in M_YES else "no"):
            f.append("%s: la revisión de la ronda 2 no registra el valor declarado de la ronda 2" % mid)
        act = r[1].replace("*", "").strip()
        want = "sí" if mid in M_YES else "no"
        if act != want:
            f.append("%s = %s en A-6; el dictamen acreditado dice %s" % (mid, act, want))
        if r[2] != mat.get(mid) or not r[5].strip():
            f.append("%s: columna Dictamen distinta del dictamen o motivo vacío" % mid)
        if (mat.get(mid) == "yes") != (mid in M_YES):
            f.append("%s: el dictamen en la base no coincide con los valores fijados" % mid)
        c = conf.get(mid) or {}
        if c.get("A6Declared") != ("yes" if mid in M_YES else "no"):
            f.append("%s: la revisión de la ronda 1 no registra el valor declarado de la ronda 1" % mid)
        cell = r[3]
        agr = c.get("Agreement")
        cell_agr = set(re.findall(r"\b(AGREE|DISAGREE)\b", cell))
        if not agr or cell_agr != {agr}:
            f.append("%s: columna Ronda 1 «%s» distinta de MaterialityConfirmation (Agreement %s)" % (mid, cell, agr))
        if agr != "AGREE" and not re.search(r"\b%s\b" % re.escape(c.get("ReviewerAssessment") or "?"), cell):
            f.append("%s: columna Ronda 1 sin la evaluación del revisor (%s)" % (mid, c.get("ReviewerAssessment")))
        if agr != "AGREE":
            fids = [x.replace("A62-A6-", "") for x in c.get("FindingIds") or []]
            miss = [fid for fid in fids if not any(re.search(r"\b%s\b" % d, r[5]) for d in where_of.get(fid, []))]
            if not fids or miss:
                f.append("%s: el motivo no resuelve la observación de la ronda 1 citando un delta corregido por cada hallazgo (%s; sin delta de %s)"
                         % (mid, fids, miss))
    if len(ru.get("OwnerReserved") or []) != 2:
        f.append("el dictamen no tiene dos consecuencias OWNER-RESERVED")
    crow = table_rows(md, "### 6.2 ") or []
    cids = [r[0] for r in crow if r]
    if cids != ["A-1", "A-2", "A-3", "A-4", "A-5"]:
        f.append("la tabla de compatibilidad no cubre A-1..A-5 una vez cada una y en orden: %s" % cids)
    for r in crow:
        if len(r) != 4 or not r[2] or not r[3].startswith("ninguno"):
            f.append("compatibilidad con %s sin interacción o con un conflicto abierto" % (r[0] if r else "?"))
    return {"Check": "G4 dictamen, revisiones de las rondas 1 (R1-R5, O1-O3), 2 (R1, R2, O1-O5) y 3 (R1, O1-O7), M-01..M-08 y compatibilidad",
            "Points": len(rows), "Findings": f, "Info": info, "Result": "PASS" if not f else "FAIL"}


def evidence_paths(fid, evid, inp, info):
    """Rutas de evidencia de una fila: `r2/…` y `r3/…` deben existir en la base; `r4/…` en el paquete r4/ disponible."""
    f = []
    for rd, key in (("r2", "R2BaseFiles"), ("r3", "R3BaseFiles")):
        base_rd = inp.get(key) or {}
        for p in re.findall(r"`%s/([^`]*)`" % rd, evid):
            if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in base_rd):
                f.append("%s: evidencia `%s/%s` ausente de %s/ en la base" % (fid, rd, p, rd))
    r4 = r2_ids(inp)
    for p in re.findall(r"`r4/([^`]*)`", evid):
        if r4:
            if not any(q == p or q.startswith(p.rstrip("/") + "/") for q in r4):
                f.append("%s: evidencia `r4/%s` ausente del paquete r4/" % (fid, p))
        else:
            info.append("%s: evidencia `r4/%s` sin comprobar (paquete r4/ no disponible)" % (fid, p))
    return f


def g4_317_append(md, prev):
    """§3.17 de la ronda 3, sin cambio salvo la celda «Prueba» de R1 (b), R1 (c) y R1 (d), que la ronda 4 solo amplía por añadido."""
    f = []
    s, e = section(md, "### 3.17 ")
    ps, pe = section(prev, "### 3.17 ")
    if s is None or ps is None:
        return ["falta §3.17 en la ronda 4 o en la ronda 3"]
    new, old = md[s:e].rstrip("\n").split("\n"), prev[ps:pe].rstrip("\n").split("\n")
    nt, ot = [l for l in new if l.startswith("|")], [l for l in old if l.startswith("|")]
    if [l for l in new if not l.startswith("|")] != [l for l in old if not l.startswith("|")]:
        f.append("§3.17: el texto fuera de las tablas no es idéntico byte a byte al de la ronda 3")
    if len(nt) != len(ot):
        f.append("§3.17: las tablas tienen %d líneas; en la ronda 3, %d" % (len(nt), len(ot)))
    for x, y in zip(nt, ot):
        if x == y:
            continue
        cx, cy = [c.strip() for c in x.strip().strip("|").split("|")], [c.strip() for c in y.strip().strip("|").split("|")]
        m = R2_ROW_RX.match(cy[0]) if cy else None
        lab = m.group(1) if m else (cy[0][:20] if cy else "?")
        if (lab in R3_317_APPENDED and len(cx) == len(cy) == 4 and cx[:3] == cy[:3] and cx[3].startswith(cy[3])
                and len(cx[3]) > len(cy[3])):
            continue
        f.append("§3.17: la fila %s cambia respecto de la ronda 3 fuera del añadido permitido (celda «Prueba» de R1 (b)-(d))" % lab)
    return f


def g4_round3(inp, md, rules, info):
    """§3.18: hallazgos de la revisión de la ronda 3 (R1 y O1-O7), una fila cada uno y en orden, con corrección, ubicación coherente con las
    líneas «Cubre» (ronda 3; la de A6-3 nombra R1) y evidencia existente; la revisión de la ronda 3 en la base tiene exactamente esos hallazgos,
    es CHANGES REQUIRED sobre el blob de la ronda 3 y no evaluó la materialidad (§5)."""
    f = []
    rv = inp.get("Review3") or {}
    req = [x.get("FindingId") for x in rv.get("RequiredFindings") or []]
    opt = [x.get("FindingId") for x in rv.get("OptionalFindings") or []]
    if req != ["A62-A6R3-R1"] or opt != ["A62-A6R3-O%d" % i for i in range(1, 8)]:
        f.append("la revisión de la ronda 3 en la base no tiene exactamente A62-A6R3-R1 y A62-A6R3-O1..O7: %s %s" % (req, opt))
    if rv.get("Verdict") != "CHANGES REQUIRED" or rv.get("ReviewedBlob") != PREV_BLOB:
        f.append("la revisión de la ronda 3 en la base no es CHANGES REQUIRED sobre el blob de la ronda 3")
    s5, e5 = section(md, "## 5. ", r"^## ")
    if s5 is None or "la revisión acotada de la ronda 3 no evaluó la materialidad" not in norm(md[s5:e5]):
        f.append("§5 no declara que la revisión acotada de la ronda 3 no evaluó la materialidad")
    if "MaterialityConfirmation" in rv:
        f.append("la revisión de la ronda 3 evaluó la materialidad: la tabla M necesitaría su columna")
    rows = table_rows(md, "### 3.18 ")
    if rows is None:
        return f + ["falta §3.18 (correcciones de la ronda 4)"]
    got, order = {}, []
    for r in rows:
        m = R3_ROW_RX.match(r[0]) if r else None
        if not m:
            continue
        order.append(m.group(1))
        if len(r) != 4:
            f.append("fila %s de §3.18 mal formada (%d columnas)" % (m.group(1), len(r)))
            continue
        got.setdefault(m.group(1), r)
    if order != R3_ROWS:
        f.append("§3.18 no tiene R1 y O1-O7, una fila cada uno y en orden: %s" % order)
    items = {d: rule_items(v["Paragraph"]) for d, v in rules.items()}
    named = {}
    for rid, r in got.items():
        corr, where, evid = r[1], r[2], r[3]
        if len(norm(corr)) < 20:
            f.append("%s: corrección vacía o insuficiente en §3.18" % rid)
        for d in where_deltas(where):
            if d not in rules:
                f.append("%s: «Dónde» de §3.18 cita %s, que no existe" % (rid, d))
        for m in re.finditer(r"\b(A6-[1-9]), reglas? (\d+(?:(?:-| y |, )\d+)*)", where):
            d = m.group(1)
            for part in re.split(r", | y ", m.group(2)):
                lo, hi = (part.split("-") + [part])[:2] if "-" in part else (part, part)
                for k in range(int(lo), int(hi) + 1):
                    if k not in items.get(d, {}):
                        f.append("%s: «Dónde» de §3.18 cita %s, regla %d, que no existe" % (rid, d, k))
        named.setdefault(rid, set()).update(d for d in where_deltas(where) if d in rules)
        f += evidence_paths(rid, evid, inp, info)
    cov = {d: covers_findings(v["Covers"], 3) for d, v in rules.items()}
    if "A6-3" in rules and "R1" not in cov.get("A6-3", set()):
        f.append("la línea «Cubre» de A6-3 no nombra R1 de la ronda 3")
    for fid in R3_FINDINGS:
        covering = {d for d, c in cov.items() if fid in c}
        if named.get(fid, set()) != covering:
            f.append("%s (ronda 3): «Dónde» de §3.18 (%s) distinto de los deltas cuya línea «Cubre» lo nombra para la ronda 3 (%s)"
                     % (fid, sorted(named.get(fid, set())), sorted(covering)))
    return f


def g4_round2(inp, md, rules, info):
    """§3.17: hallazgos de la revisión de la ronda 2 (R1 (a)-(d), R2 (a)-(b), O1-O5), una fila cada uno y en orden, con corrección, ubicación
    coherente con las líneas «Cubre» (ronda 2) y evidencia existente; la revisión de la ronda 2 en la base tiene exactamente esos hallazgos."""
    f = []
    rv = inp.get("Review2") or {}
    req = [x.get("FindingId") for x in rv.get("RequiredFindings") or []]
    opt = [x.get("FindingId") for x in rv.get("OptionalFindings") or []]
    if req != ["A62-A6R2-R1", "A62-A6R2-R2"] or opt != ["A62-A6R2-O%d" % i for i in range(1, 6)]:
        f.append("la revisión de la ronda 2 en la base no tiene exactamente A62-A6R2-R1, R2 y A62-A6R2-O1..O5: %s %s" % (req, opt))
    if rv.get("Verdict") != "CHANGES REQUIRED" or rv.get("ReviewedBlob") != R2_BLOB:
        f.append("la revisión de la ronda 2 en la base no es CHANGES REQUIRED sobre el blob de la ronda 2")
    rows = table_rows(md, "### 3.17 ")
    if rows is None:
        return f + ["falta §3.17 (correcciones de la ronda 3)"]
    got, order = {}, []
    for r in rows:
        m = R2_ROW_RX.match(r[0]) if r else None
        if not m:
            continue
        order.append(m.group(1))
        if len(r) != 4:
            f.append("fila %s de §3.17 mal formada (%d columnas)" % (m.group(1), len(r)))
            continue
        got.setdefault(m.group(1), r)
    if order != R2_ROWS:
        f.append("§3.17 no tiene R1 (a)-(d), R2 (a)-(b) y O1-O5, una fila cada uno y en orden: %s" % order)
    items = {d: rule_items(v["Paragraph"]) for d, v in rules.items()}
    named = {}
    for rid, r in got.items():
        corr, where, evid = r[1], r[2], r[3]
        if len(norm(corr)) < 20:
            f.append("%s: corrección vacía o insuficiente en §3.17" % rid)
        for d in where_deltas(where):
            if d not in rules:
                f.append("%s: «Dónde» de §3.17 cita %s, que no existe" % (rid, d))
        for m in re.finditer(r"\b(A6-[1-9]), reglas? (\d+(?:(?:-| y |, )\d+)*)", where):
            d = m.group(1)
            for part in re.split(r", | y ", m.group(2)):
                lo, hi = (part.split("-") + [part])[:2] if "-" in part else (part, part)
                for k in range(int(lo), int(hi) + 1):
                    if k not in items.get(d, {}):
                        f.append("%s: «Dónde» de §3.17 cita %s, regla %d, que no existe" % (rid, d, k))
        named.setdefault(rid.split(" ")[0], set()).update(d for d in where_deltas(where) if d in rules)
        f += evidence_paths(rid, evid, inp, info)
    cov = {d: covers_findings(v["Covers"], 2) for d, v in rules.items()}
    for fid in R2_FINDINGS:
        covering = {d for d, c in cov.items() if fid in c}
        if named.get(fid, set()) != covering:
            f.append("%s (ronda 2): «Dónde» de §3.17 (%s) distinto de los deltas cuya línea «Cubre» lo nombra para la ronda 2 (%s)"
                     % (fid, sorted(named.get(fid, set())), sorted(covering)))
    return f


# ---------------------------------------------------------------- G5
_SECS_MEMO = {}


def g5_delta(inp, cm):
    f, info = [], []
    md = inp["A6"] or ""
    rules = a6_rules(md)
    if sorted(rules) != sorted(DELTAS):
        f.append("§3.4..§3.12 no contienen exactamente A6-1..A6-9: %s" % sorted(rules))
        return {"Check": "G5 pureza del delta", "Findings": f, "Result": "FAIL"}
    paras = {r: v["Paragraph"] for r, v in rules.items()}
    for r in DELTAS:
        if not rules[r]["Location"].startswith(LOC[r]):
            f.append("%s: ubicación distinta de «%s»" % (r, LOC[r][:70]))
        if not paras[r].strip():
            f.append("%s: sin párrafo citado" % r)
    r1rules = a6_rules(inp["R1Text"])
    for r in IDENTICAL_TO_R1:
        if rules[r]["Block"] != r1rules.get(r, {}).get("Block"):
            f.append("%s: el párrafo no es idéntico byte a byte al de la ronda 3" % r)
        if r in SECTION_IDENTICAL and rules[r]["Section"].rstrip("\n") != r1rules.get(r, {}).get("Section", "").rstrip("\n"):
            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 3" % r)
    it3, it3p = rule_items(paras["A6-3"]), rule_items(r1rules.get("A6-3", {}).get("Paragraph", ""))
    for k in A63_SAME_RULES:
        if it3.get(k) != it3p.get(k):
            f.append("A6-3: la %s no es idéntica a la de la ronda 3 (en la ronda 4 solo cambia la regla 6 (b))" % ("cabecera" if k == 0 else "regla %d" % k))
    if sorted(it3) != sorted(it3p):
        f.append("A6-3: el número de reglas cambió respecto de la ronda 3 (%s)" % sorted(it3))
    f += g5_rule6b(rules["A6-3"], r1rules.get("A6-3") or {})
    f += g5_a63_section(rules["A6-3"], r1rules.get("A6-3") or {})
    for h in SAME_SECTIONS:
        s, e = section(md, h)
        ps, pe = section(inp["R1Text"], h)
        if s is None or ps is None or md[s:e].rstrip("\n") != inp["R1Text"][ps:pe].rstrip("\n"):
            f.append("%s: la sección no es idéntica byte a byte a la de la ronda 3 (la ronda 4 no la cambia)" % h.strip())
    v14, a2, a4, a5 = (inp["Sources"][k] for k in ("V14", "A-2", "A-4", "A-5"))
    vl = v14.split("\n")
    for line, anchor, name in ((ANCHOR_D1_LINE, ANCHOR_D1, "D.1"), (ANCHOR_D4_LINE, ANCHOR_D4, "D.4"), (ANCHOR_D6_LINE, ANCHOR_D6, "D.6")):
        if vl[line - 1] != anchor or v14.count(anchor) != 1:
            f.append("ancla de %s (V14 L%d) ausente, repetida o desplazada" % (name, line))
    if not (v14.index(H_D4) < v14.find(ANCHOR_D4) < v14.index(H_D5)) or v14[v14.find(ANCHOR_D4) + len(ANCHOR_D4):v14.index(H_D5)].strip():
        f.append("el ancla de A6-5 no es el final de D.4")
    if not (v14.index(H_D6) < v14.find(ANCHOR_D6) < v14.index(H_D7)) or v14[v14.find(ANCHOR_D6) + len(ANCHOR_D6):v14.index(H_D7)].strip():
        f.append("el ancla de A6-3 no es el final de D.6")
    if vl[CAPS_LINE - 1] != CAPS:
        f.append("el literal de topes de V14 L2442 no es el fijado")
    base, a4ids, a5ids = virtual_base(v14, a2, a4, a5)
    if a4ids != ["A4-1", "A4-2", "A4-3", "A4-4", "A4-5", "A4-6"] or a5ids != ["A5-1", "A5-2"]:
        f.append("A-4 o A-5 no aportan exactamente A4-1..A4-6 y A5-1..A5-2: %s %s" % (a4ids, a5ids))
    if base.count(ANCHOR_A52_END) != 1:
        f.append("«%s» aparece %d veces en V14 + A-2 + A-4 + A-5 (ancla de A-6 en D.3 no única)" % (ANCHOR_A52_END, base.count(ANCHOR_A52_END)))
        return {"Check": "G5 pureza del delta", "Findings": f, "Result": "FAIL"}
    pos = base.find(ANCHOR_A52_END) + len(ANCHOR_A52_END)
    if base[pos:base.index(H_D4)].strip() or not (base.index(H_D3) < pos < base.index(H_D4)):
        f.append("A5-2 no es el final de D.3 en V14 + A-2 + A-4 + A-5")
    groups = {"D.3": (ORDER_D3, ANCHOR_A52_END, "\n\n", ""), "D.4": (ORDER_D4, ANCHOR_D4 + "\n", "\n", "\n"),
              "D.6": (ORDER_D6, ANCHOR_D6 + "\n", "\n", "\n")}
    allowed = {"D.3": (3, norm(H_D3)), "D.4": (3, norm(H_D4)), "D.6": (3, norm(H_D6))}

    def secs(t):
        k = (hash(t), len(t))
        if k not in _SECS_MEMO:
            _SECS_MEMO[k] = {(lv, h): s for lv, h, s in cm.sections(t)}
        return _SECS_MEMO[k]
    sb = secs(base)
    base_lines = base.split("\n")
    for name, use in (("D.3", ["D.3"]), ("D.4", ["D.4"]), ("D.6", ["D.6"]), ("A-6 completa", ["D.3", "D.4", "D.6"])):
        applied, ins = base, []
        for g in use:
            order, anc, pre, post = groups[g]
            txt = pre + "\n\n".join(paras[r] for r in order) + post
            applied = applied.replace(anc, anc + txt, 1)
            ins.append((txt, anc))
        ops = [o for o in difflib.SequenceMatcher(None, base_lines, applied.split("\n"), autojunk=False).get_opcodes() if o[0] != "equal"]
        back = applied
        for txt, anc in ins:
            back = back.replace(anc + txt, anc, 1)
        if set(o[0] for o in ops) - {"insert"} or len(ops) != len(use) or back != base:
            f.append("%s: la aplicación no es un añadido puro (%s)" % (name, [o[0] for o in ops]))
        sa = secs(applied)
        if set(sa) != set(sb):
            f.append("%s: al aplicar aparecen o desaparecen secciones: %s" % (name, sorted(set(sa) ^ set(sb))[:4]))
        chg = {k for k in sb if k in sa and sb[k] != sa[k]}
        want = {(1, norm(H_TITLE)), (2, norm(H_ANEXO_D))} | {allowed[g] for g in use}
        if chg - want:
            f.append("%s: cambian secciones fuera de las declaradas: %s" % (name, sorted(chg - want)[:4]))
        for g in use:
            if allowed[g] not in chg:
                f.append("%s: no cambia %s" % (name, g))
        if len([l for l in applied.split("\n") if l == CAPS]) != 1:
            f.append("%s: el literal de topes de «Codex, total» cambia o se repite" % name)
    for rid in DELTAS:
        para = paras[rid]
        if [l for l in para.split("\n") if re.match(r"^\s*(\||#|```)", l)]:
            f.append("%s: el delta trae filas de tabla, encabezados o vallas" % rid)
        first = re.sub(r"^\*\*[^*]+\*\*\s*", "", norm(para))
        if not first.startswith(SCOPE[rid]):
            f.append("%s: no abre con su cláusula de alcance «%s»" % (rid, SCOPE[rid]))
        np_ = norm(para)
        for ph in REQUIRED[rid]:
            if norm(ph) not in np_:
                f.append("%s: falta la frase exigida «%s»" % (rid, ph[:60]))
        for ph in FORBIDDEN_ALL + FORBIDDEN.get(rid, []):
            if norm(ph) in np_:
                f.append("%s: contiene «%s» (prohibido)" % (rid, ph))
        scrub = para
        for ex in PRODUCT_EXEMPT.get(rid, []):
            scrub = scrub.replace(ex, "")
        hits = sorted(set(m.group(0).lower() for m in PRODUCT_TOKENS.finditer(scrub)))
        if hits:
            f.append("%s: nombres de producto en el delta: %s" % (rid, hits))
        if re.search(r"(?<!ningún contador )se reinicia", np_):
            f.append("%s: «se reinicia» fuera de «ningún contador se reinicia»" % rid)
    m = re.search(r"\(\*\*(\d+)\*\* base, pool \*\*(\d+)\*\* con ≤ (\d+) por fase, tope \*\*(\d+)\*\*\)", paras["A6-1"])
    cm_caps = re.search(r"\*\*(\d+)\*\* \| \| pool \*\*(\d+)\*\* \(≤ (\d+) por fase\) \| \*\*(\d+)\*\*", CAPS)
    if not m or m.groups() != cm_caps.groups():
        f.append("A6-1: las cifras de la fila «Codex, total» no son las de V14 L2442 (%s)" % (str(m.groups()) if m else "sin cifras",))
    s313, e313 = section(md, "### 3.13 ")
    body313 = norm(md[s313:e313]) if s313 is not None else ""
    for ph in PROHIBITIONS_313:
        if norm(ph) not in body313:
            f.append("§3.13 sin la prohibición «%s»" % ph[:70])
    info.append("A6-1, A6-2, A6-4, A6-7, A6-9 y A6-8 tras A5-2 (fin de D.3); A6-5 y A6-6 tras V14 L2554 (fin de D.4); A6-3 tras V14 L2587 (fin de D.6)")
    return {"Check": "G5 pureza del delta", "Paragraphs": {r: len(paras[r]) for r in DELTAS}, "Info": info, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


def r6_parts(block):
    """(líneas, [(marca, inicio, fin)]) de la regla 6 de un bloque citado de A6-3: «6» (cabecera), (a)-(d), (i)-(v), «cierre» (la frase que
    empieza por «La prueba previa») y «final» (el párrafo que empieza por «El aceptante vuelve a ejecutar»), en el orden del texto."""
    ls = block.split("\n")
    st = next((i for i, l in enumerate(ls) if re.match(r"^> 6\. ", l)), None)
    if st is None:
        return ls, []
    en = next((i for i in range(st + 1, len(ls)) if re.match(r"^> \d+\. ", ls[i])), len(ls))
    marks = [("6", st)]
    for i in range(st + 1, en):
        m = R6_MARK.match(ls[i])
        if m:
            marks.append((m.group(1) or m.group(2) or ("cierre" if m.group(3) else "final"), i))
    return ls, [(k, a, marks[j + 1][1] if j + 1 < len(marks) else en) for j, (k, a) in enumerate(marks)]


def r6_text(ls, parts, keys):
    return norm(" ".join(ls[i][2:] if ls[i].startswith("> ") else ls[i].lstrip(">") for k, a, b in parts if k in keys for i in range(a, b)))


def g5_rule6b(v, pv):
    """A6-3, regla 6: la cabecera, (a), (c), (d) y el párrafo final, y todo el párrafo salvo (b), byte a byte como en la ronda 3; (b) distinta de
    la de la ronda 3, con `RelayFacts` antes de `Closure`, los apartados (i)-(v) y la frase final, y cada frase exigida una vez y en su apartado."""
    f = []
    ls, parts = r6_parts(v.get("Block") or "")
    pls, pparts = r6_parts(pv.get("Block") or "")
    keys, pkeys = [k for k, _, _ in parts], [k for k, _, _ in pparts]
    if keys != R6_ORDER:
        f.append("A6-3, regla 6: no tiene (a), (b) con (i)-(v) y la frase final, (c), (d) y el párrafo final, una vez cada uno y en orden: %s" % keys)
    if pkeys != R6_ORDER_PREV:
        f.append("A6-3, regla 6, de la ronda 3 sin su forma fijada: %s" % pkeys)
    cur, old = {}, {}
    for k, a, b in parts:
        cur.setdefault(k, ls[a:b])
    for k, a, b in pparts:
        old.setdefault(k, pls[a:b])
    for k, lab in (("6", "la cabecera de la regla 6"), ("a", "el apartado (a)"), ("c", "el apartado (c)"), ("d", "el apartado (d)"),
                   ("final", "el párrafo final («El aceptante vuelve a ejecutar…»)")):
        if cur.get(k) != old.get(k):
            f.append("A6-3, regla 6: %s cambia respecto de la ronda 3 (debe ser igual byte a byte)" % lab)
    rest = [l for i, l in enumerate(ls) if not any(a <= i < b for k, a, b in parts if k in R6B_KEYS)]
    prest = [l for i, l in enumerate(pls) if not any(a <= i < b for k, a, b in pparts if k in R6B_KEYS)]
    if rest != prest:
        f.append("A6-3: fuera de la regla 6 (b), el párrafo no es idéntico byte a byte al de la ronda 3")
    btext, pbtext = r6_text(ls, parts, R6B_KEYS), r6_text(pls, pparts, R6B_KEYS)
    if not btext or btext == pbtext:
        f.append("A6-3, regla 6 (b): igual a la de la ronda 3 o vacía (la ronda 4 la corrige)")
    for k in ("i", "ii", "iii", "iv", "v", "cierre"):
        if k in keys and len(r6_text(ls, parts, [k])) < 20:
            f.append("A6-3, regla 6 (b): el apartado %s está vacío" % k)
    for k, ph in R6B_REQUIRED:
        c = btext.count(norm(ph))
        if c != 1:
            f.append("A6-3, regla 6 (b): la frase exigida «%s» aparece %d veces (exactamente una)" % (ph[:60], c))
        elif norm(ph) not in r6_text(ls, parts, [k]):
            f.append("A6-3, regla 6 (b): la frase exigida «%s» está fuera de su apartado (%s)" % (ph[:60], k))
    head = r6_text(ls, parts, ["b"])
    i1, i2 = head.find("`RelayFacts` ="), head.find("`Closure` =")
    if i1 < 0 or i2 < 0 or i1 > i2:
        f.append("A6-3, regla 6 (b): `RelayFacts` no está antes de `Closure` en la cabecera del apartado")
    return f


def g5_a63_section(v, pv):
    """Sección de A6-3: fuera del párrafo citado y de la línea «Cubre», idéntica a la de la ronda 3; la línea «Cubre», la de la ronda 3 más un
    segmento «… de la ronda 3.»."""
    f = []

    def rest(x):
        body = (x.get("Section") or "").replace(x.get("Block") or "\0", "", 1)
        ls = body.split("\n")
        i = next((k for k, l in enumerate(ls) if l.startswith("Cubre:")), None)
        if i is not None:
            j = i + 1
            while j < len(ls) and ls[j].strip() and not ls[j].startswith(">"):
                j += 1
            ls = ls[:i] + ls[j:]
        return "\n".join(ls).rstrip("\n")
    if rest(v) != rest(pv):
        f.append("A6-3: la sección, fuera del párrafo citado y de la línea «Cubre», no es idéntica byte a byte a la de la ronda 3")
    pc, nc = pv.get("Covers") or "", v.get("Covers") or ""
    stem = pc[:-1] if pc.endswith(".") else pc
    if not stem or not nc.startswith(stem + "; ") or not re.fullmatch(r"[^;]* de la ronda 3\.", nc[len(stem) + 2:]):
        f.append("A6-3: la línea «Cubre» no es la de la ronda 3 más un segmento «… de la ronda 3.»: «%s»" % nc[len(stem):][:70])
    return f


# ---------------------------------------------------------------- G6
OWNER_LINE = re.compile(r"^(A6-[A-Z0-9-]+) = A \((.+)\)$")
OWNER_SHAPE = re.compile(r"(?m)^[A-Z0-9][A-Z0-9-]+ = [A-Z] \(")
OWNER_REQ = {
    "A6-ADR0046-9-FX02": ["determinación OWNER-RESERVED", "previa al acuerdo de A-6", "ADR-0046 #9", "conservado por ADR-0048",
                          "EXECUTION_CONTROLLER de FX-02 en F6, Topología A", "puede usar en su verificación",
                          "solo el instrumento de hechos de blob " + TOOL_BLOB, "solo el aceptante de blob " + ACC_BLOB, PKG_COMMIT,
                          "R20261011T162328Z-5c5e", "excluye la producción, cualquier otra unidad, escenario, rol o topología, D.5 y la Owner Validation",
                          "no modifica ADR-0046 ni ADR-0048 ni crea precedente", "no autoriza ninguna invocación ni consumo",
                          "ni acepta ningún resultado", "solo tiene efecto con A-6 AGREED"],
    "A6-CONTROLLER-MEDICION-CONSUMO": ["1 invocación de medición read-only de codex-cli", "celda gpt-6-luna/high",
                                       "EXECUTION_CONTROLLER de FX-02 en F6", "con el instrumento de blob " + TOOL_BLOB,
                                       "el intérprete que nombre la disposición",
                                       "para el trío exacto %s/%s/%s" % (BINARY_HASH, APP_VERSION, FINGERPRINT),
                                       "con huella, binario y versión antes y después", "fuera de A2-P2",
                                       "sin reutilizar su bloque agotado ni la invocación consumida de A4-1", "sin reintento",
                                       "solo con A-6 AGREED", "con A6-ADR0046-9-FX02 = A", "la disposición del Coordinator que la nombre",
                                       "distinta de A6-ADR0046-9-FX02 y no implícita en ella", "no acepta su resultado"],
}
OWNER_FORBIDDEN = {
    "A6-ADR0046-9-FX02": ["1 invocación", "codex-cli", "trío", "gpt-", "medición", "reintento", "planificación", "PLAN"],
    "A6-CONTROLLER-MEDICION-CONSUMO": ["determinación", "ADR-0046 #9", "excepción", "dentro de A2-P2", "reutilizando"],
}


def owner_lines(text):
    """{nombre: línea} de las líneas del Owner de §7 (primer bloque ```text con líneas «NOMBRE = A (…)»)."""
    s7, e7 = section(text, "## 7. ", r"^## ")
    if s7 is None:
        return {}
    for _, _, body in fences(text[s7:e7]):
        ls = [l for l in body.split("\n") if OWNER_LINE.match(l)]
        if ls:
            return {OWNER_LINE.match(l).group(1): l for l in ls}
    return {}


def dec_section(dec, num):
    lines = dec.split("\n")
    start = next((i for i, l in enumerate(lines) if l.startswith("## %d. " % num)), None)
    if start is None:
        return None, None, lines
    end = next((j for j in range(start + 1, len(lines)) if lines[j].startswith("## ")), len(lines))
    return start, end, lines


def g6_owner(inp):
    f = []
    md = inp["A6"] or ""
    s7, e7 = section(md, "## 7. ", r"^## ")
    if s7 is None:
        return {"Check": "G6 líneas del Owner", "Findings": ["falta §7"], "Result": "FAIL"}
    body = md[s7:e7]
    blocks = [b for _, _, b in fences(body)]
    with_lines = [b for b in blocks if OWNER_SHAPE.search(b)]
    if len(with_lines) != 1:
        f.append("§7 no tiene exactamente un bloque con líneas del Owner (%d)" % len(with_lines))
    lines = [l for l in (with_lines[0] if with_lines else "").split("\n") if l.strip()]
    if len(lines) != 2:
        f.append("el bloque de §7 no tiene exactamente dos líneas del Owner (%d)" % len(lines))
    other = [b for b in fences(md) if OWNER_SHAPE.search(b[2]) and not (s7 <= b[0] < e7)]
    if other:
        f.append("hay líneas con forma de línea del Owner fuera de §7 (%d bloques)" % len(other))
    r1 = owner_lines(inp["R1Text"])
    names = []
    for l in lines:
        m = OWNER_LINE.match(l)
        if not m:
            f.append("línea del Owner mal formada: %s" % l[:50])
            continue
        names.append(m.group(1))
        name, content = m.group(1), m.group(2)
        if r1.get(name) != l:
            f.append("%s: la línea no es idéntica byte a byte a la de la ronda anterior" % name)
        if name in OWNER_REQ:
            for ph in OWNER_REQ[name]:
                if ph not in content:
                    f.append("%s sin «%s»" % (name, ph[:60]))
            for ph in OWNER_FORBIDDEN[name]:
                if re.search(r"(?<![\w-])" + re.escape(ph) + r"(?![\w])", content):
                    f.append("%s contiene «%s» (contenido de la otra línea o prohibido)" % (name, ph))
        if content.count(" = A (") or re.search(r"\) A6-", content):
            f.append("%s parece fundir dos líneas" % name)
    if names != OWNER_NAMES:
        f.append("nombres de las líneas del Owner distintos de los fijados o repetidos: %s" % names)
    la = next((l for l in lines if l.startswith(OWNER_NAMES[0] + " = A (")), "")
    if hashlib.sha256(la.encode("utf-8")).hexdigest() != OWNER_A_SHA256:
        f.append("SHA-256 de la línea (a) distinto de %s…" % OWNER_A_SHA256[:8])
    if norm("separadas y distintas") not in norm(body) or "**(a)" not in body or "**(b)" not in body:
        f.append("§7 no declara (a) y (b) como consecuencias separadas y distintas")
    dec = inp["Sources"].get("DEC") or ""
    st, en, dl = dec_section(dec, 78)
    if st is None:
        f.append("decisiones en la base sin la sección «## 78.»")
    elif not la or la not in dl[st + 1:en]:
        f.append("la línea (a) no está como línea completa en decisiones §78 en la base")
    for v in (BINARY_HASH, APP_VERSION, FINGERPRINT):
        if v not in dec:
            f.append("valor del trío ausente del registro de decisiones en la base: %s" % v[:16])
    return {"Check": "G6 líneas del Owner (idénticas a la ronda anterior; (a) registrada en §78)", "Lines": len(lines), "Names": names,
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G7
def g7_instruments(inp):
    f = []
    md = inp["A6"] or ""
    rows = table_rows(md, "### 3.3 ") or []
    got = {}
    for r in rows:
        if len(r) >= 4 and r[1].startswith("`"):
            got[r[1].strip("`")] = (r[2].strip("`"), r[3].strip("`"))
    want_paths = [p for p, _ in INSTRUMENTS]
    if sorted(got) != sorted(want_paths):
        f.append("la tabla de §3.3 no fija exactamente el instrumento, el aceptante y la especificación: %s" % sorted(got))
    for p, b in INSTRUMENTS:
        blob, sha = got.get(p, (None, None))
        if blob != b:
            f.append("%s: blob en A-6 %s distinto del fijado %s" % (p, blob, b))
        if inp["Objs"]["Base"].get(p) != b:
            f.append("%s: blob en la base %s distinto del fijado" % (p, (inp["Objs"]["Base"].get(p) or "")[:12]))
        if inp["PkgCommitBlobs"].get(p) != b:
            f.append("%s: blob en el commit del paquete distinto del fijado" % p)
        if sha != inp["BlobSha256"].get(p):
            f.append("%s: SHA-256 en A-6 distinto del contenido del blob en la base" % p)
    if not inp.get("AccBlobOk"):
        f.append("el contenido leído del aceptante no da su blob fijado")
    ru = inp.get("Ruling") or {}
    rfiles = {x.get("Path"): x.get("Blob") for x in ((ru.get("Object") or {}).get("Files") or [])}
    for p, b in INSTRUMENTS:
        if rfiles.get(p) != b:
            f.append("%s: blob del objeto del dictamen distinto del fijado" % p)
    e2e = inp.get("E2E") or {}
    if e2e.get("ToolBlob") != TOOL_BLOB or e2e.get("AcceptorBlob") != ACC_BLOB:
        f.append("e2e-summary.json con blobs de herramienta o aceptante distintos de los fijados")
    if e2e.get("ToolSha256") != inp["BlobSha256"].get(TOOL) or e2e.get("AcceptorSha256") != inp["BlobSha256"].get(ACC):
        f.append("e2e-summary.json con SHA-256 distintos del contenido de los blobs fijados")
    return {"Check": "G7 identidad por blob de los instrumentos", "Rows": len(got), "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G8
def g8_status(inp):
    f, info = [], []
    md = inp["A6"] or ""
    hb = header_block(md)
    if "State:\nPROPUESTA (ronda 4). No aplicada" not in hb:
        f.append("cabecera sin «State: PROPUESTA (ronda 4). No aplicada»")
    if re.search(r"(?m)^State:\s*\n(?!PROPUESTA \(ronda 4\))", hb):
        f.append("estado distinto de PROPUESTA (ronda 4)")
    if "Ronda 4: NOT LAUNCHED." not in hb:
        f.append("cabecera sin «Ronda 4: NOT LAUNCHED.»")
    if not re.search(r"(?m)^\(a\) A6-ADR0046-9-FX02 = A, registrada", hb) or "(b) A6-CONTROLLER-MEDICION-CONSUMO: no pedida" not in norm(hb):
        f.append("cabecera sin (a) registrada y (b) no pedida")
    rows = {r[0]: r for r in (table_rows(md, "## 11. ", r"^## ") or []) if r}
    arch = norm(" ".join(rows.get("Architect independiente", [])))
    coord = norm(" ".join(rows.get("Coordinator", [])))
    owner = norm(" ".join(rows.get("Owner", [])))
    if "ronda 4: NOT LAUNCHED" not in arch:
        f.append("§11 sin «ronda 4: NOT LAUNCHED» para el Architect")
    if "sin veredicto sobre la ronda 4" not in coord:
        f.append("§11 sin «sin veredicto sobre la ronda 4» para el Coordinator")
    if "sin usar el dictamen como acuerdo" not in coord:
        f.append("§11 sin «sin usar el dictamen como acuerdo» para el Coordinator (decisiones §83, punto 1)")
    if "§7 (b) solo para la aplicación" not in owner:
        f.append("§11 sin «§7 (b) solo para la aplicación» para el Owner")
    masked = mask_fences(md)
    for pat, what in CLAIMS:
        if re.search(pat, masked) or re.search(pat, hb):
            f.append("declaración prohibida en esta preparación: %s" % what)
    for pat in REQUEST_PATTERNS:
        for m in re.finditer(pat, masked):
            if not neg_before(masked, m.start()) and not neg_before(masked, m.start() + 3):
                f.append("petición de una línea o decisión del Owner: «%s»" % norm(m.group(0))[:70])
    ph = sorted(set(PLACEHOLDER.findall(md)))
    if ph:
        if inp.get("Final"):
            f.append("marcadores pendientes en el texto publicado: %s" % ph[:6])
        else:
            info.append("marcadores pendientes (solo se rechazan en run o con --final): %s" % ph[:6])
    return {"Check": "G8 estado y declaraciones", "Findings": f, "Info": info, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G9
def clauses(text):
    t = norm(text)
    return [c for c in re.split(r"(?<=[.;:])\s+|\s+(?=\d+\. )", t) if c.strip()]


def g9_scope(inp):
    f, checked = [], {"Clauses": 0, "PlanClauses": 0, "Sites": 0, "Sources": 0}
    md = inp["A6"] or ""
    rules = a6_rules(md)
    for d in DELTAS:
        if d not in rules:
            f.append("%s ausente" % d)
            continue
        for c in clauses(rules[d]["Paragraph"]):
            checked["Clauses"] += 1
            if PLAN_TERM.search(c):
                checked["PlanClauses"] += 1
                if not PLAN_NEG.search(c):
                    f.append("%s admite PLAN o la planificación sin excluirla: «%s»" % (d, c[:90]))
    items = {d: rule_items(v["Paragraph"]) for d, v in rules.items()}
    for d, k, ph in G9_SITES:
        checked["Sites"] += 1
        if norm(ph) not in items.get(d, {}).get(k, ""):
            f.append("%s, %s: falta «%s»" % (d, "cabecera" if k == 0 else "regla %d" % k, ph[:70]))
    src = {"AP": inp["Sources"]["AP"].split("\n"), "RAE": inp["Sources"]["RAE"].split("\n"), "KIT": inp["Sources"]["KIT"].split("\n")}
    for tag, line, phs in G9_SOURCES:
        checked["Sources"] += 1
        got = src[tag][line - 1] if line - 1 < len(src[tag]) else ""
        for ph in phs:
            if ph not in got:
                f.append("%s L%d (fijado) sin «%s»: la fuente no sostiene verificación = VERIFY" % (tag, line, ph[:60]))
    blocks, _ = literal_blocks(md)
    for tag, line in G9_QUOTED:
        if not any(t == tag and a <= line <= b for t, a, b, _, _ in blocks):
            f.append("§2 no cita literalmente %s L%d (fuente de verificación = VERIFY)" % (tag, line))
    owner_a = owner_lines(md).get(OWNER_NAMES[0], "")
    if OWNER_A_SCOPE not in owner_a:
        f.append("la línea (a) de §7 no dice «%s»" % OWNER_A_SCOPE)
    masked = mask_fences(md)
    s2, e2 = section(md, "## 2. ", r"^## ")
    for m in EXTENSION.finditer(masked):
        if s2 is not None and s2 <= m.start() < e2:
            continue
        if not neg_before(masked, m.start()):
            ctx = norm(masked[max(0, m.start() - 60):m.end() + 40])
            f.append("posible petición de ampliar la determinación del Owner: «…%s…»" % ctx[:100])
    s7, e7 = section(md, "## 7. ", r"^## ")
    if s7 is None or "no se pide ninguna ampliación" not in norm(md[s7:e7]):
        f.append("§7 no declara que no se pide ninguna ampliación de la determinación del Owner")
    return {"Check": "G9 alcance de R5: solo VERIFY, PLAN excluida", "Checked": checked, "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G10
def _lit(node, consts):
    if isinstance(node, ast.Name):
        return consts.get(node.id)
    if isinstance(node, ast.Call) and isinstance(node.func, ast.Name) and node.func.id == "frozenset" and node.args:
        return set(ast.literal_eval(node.args[0]))
    try:
        return ast.literal_eval(node)
    except Exception:
        return None


def analyze_acceptor(src):
    """Hechos del aceptante fijado leídos de sus bytes (árbol sintáctico, sin ejecutarlo)."""
    mod = ast.parse(src)
    consts, funcs = {}, {}
    for n in mod.body:
        if isinstance(n, ast.Assign) and len(n.targets) == 1 and isinstance(n.targets[0], ast.Name):
            v = _lit(n.value, consts)
            if v is not None:
                consts[n.targets[0].id] = v
        elif isinstance(n, ast.FunctionDef):
            funcs[n.name] = n
        elif isinstance(n, ast.ClassDef):
            for m in n.body:
                if isinstance(m, ast.FunctionDef):
                    funcs[n.name + "." + m.name] = m
    out = {"CheckIds": list(consts.get("CHECK_IDS") or []), "Waivable": sorted(consts.get("WAIVABLE") or []), "Commands": {}, "Branches": {},
           "ExtraSources": {}, "ManualChecks": [], "RequestKeys": {}, "DecisionStatuses": [], "StatusValues": set(), "ExitCodes": {}}
    for rid in ("R5", "R6", "R7"):
        fn = funcs.get("check_" + rid.lower())
        for c in ast.walk(fn) if fn else []:
            if isinstance(c, ast.Call) and isinstance(c.func, ast.Name) and c.func.id == "_run_comparisons" and len(c.args) >= 2:
                out["Commands"][rid] = sorted(_lit(c.args[1], consts) or [])
        extra = set()
        for c in ast.walk(fn) if fn else []:
            if (isinstance(c, ast.Call) and isinstance(c.func, ast.Name) and c.func.id == "dig" and len(c.args) >= 2
                    and isinstance(c.args[0], ast.Attribute) and c.args[0].attr == "cv" and isinstance(c.args[1], ast.Constant)):
                extra.add(c.args[1].value)
        out["ExtraSources"][rid] = sorted(extra)
    for rid, fname in (("R5", "_compare_r5"), ("R7", "_compare_r7")):
        br = set()
        for c in ast.walk(funcs.get(fname) or ast.Module(body=[], type_ignores=[])):
            if (isinstance(c, ast.Compare) and isinstance(c.left, ast.Name) and c.left.id == "cmd" and len(c.ops) == 1
                    and isinstance(c.ops[0], ast.Eq) and isinstance(c.comparators[0], ast.Constant)):
                br.add(c.comparators[0].value)
        out["Branches"][rid] = sorted(br)
    for rid in out["CheckIds"]:
        fn = funcs.get("check_" + rid.lower())
        keys = set()
        for c in ast.walk(fn) if fn else []:
            if isinstance(c, ast.Return) and isinstance(c.value, ast.Tuple) and c.value.elts and isinstance(c.value.elts[0], ast.Constant):
                if c.value.elts[0].value == "MANUAL":
                    out["ManualChecks"].append(rid)
                out["StatusValues"].add(c.value.elts[0].value)
            if (isinstance(c, ast.Call) and isinstance(c.func, ast.Attribute) and c.func.attr == "get" and isinstance(c.func.value, ast.Attribute)
                    and c.func.value.attr == "req" and c.args and isinstance(c.args[0], ast.Constant)):
                keys.add(c.args[0].value)
            if isinstance(c, ast.Attribute) and isinstance(c.value, ast.Name) and c.value.id == "ctx":
                if c.attr == "runs":
                    keys.add("Runs")
                if c.attr == "cv":
                    keys.add("Verification")
                if c.attr in ("repo",):
                    keys.add("Repo")
                if c.attr in ("tool_cfg", "tool_object_bytes", "tool_file_bytes"):
                    keys.add("Tool")
        for c in ast.walk(fn) if fn else []:
            if isinstance(c, ast.Call) and isinstance(c.func, ast.Name) and c.func.id == "rerun_tool":
                keys |= {"Tool", "Repo"}
            if isinstance(c, ast.Call) and isinstance(c.func, ast.Name) and c.func.id == "_run_comparisons":
                keys |= {"Runs", "Repo"}
        out["RequestKeys"][rid] = sorted(keys)
    st = funcs.get("_status")
    for c in ast.walk(st) if st else []:
        if isinstance(c, ast.IfExp):
            out["StatusValues"] |= {c.body.value, c.orelse.value}
    acc = funcs.get("accept")
    for c in ast.walk(acc) if acc else []:
        if isinstance(c, ast.Tuple) and c.elts and isinstance(c.elts[0], ast.Constant) and c.elts[0].value in ("NOT_APPLICABLE", "MISMATCH"):
            out["StatusValues"].add(c.elts[0].value)
        if isinstance(c, ast.Assign) and isinstance(c.targets[0], ast.Name) and c.targets[0].id == "decision" and isinstance(c.value, ast.IfExp):
            ie = c.value
            out["DecisionTrue"], out["DecisionFalse"] = ie.body.value, ie.orelse.value
            for g in ast.walk(ie.test):
                if isinstance(g, ast.Compare) and isinstance(g.ops[0], ast.In) and isinstance(g.comparators[0], ast.Tuple):
                    out["DecisionStatuses"] = sorted(e.value for e in g.comparators[0].elts)
                    out["DecisionAll"] = isinstance(ie.test, ast.Call) and getattr(ie.test.func, "id", "") == "all"
    mn = funcs.get("main")
    for c in ast.walk(mn) if mn else []:
        if isinstance(c, ast.Assign) and isinstance(c.targets[0], ast.Name) and c.targets[0].id == "rc":
            if isinstance(c.value, ast.IfExp):
                t = c.value.test
                if isinstance(t, ast.Compare) and isinstance(t.comparators[0], ast.Constant) and t.comparators[0].value == "ACCEPT_FACTS":
                    out["ExitCodes"]["ACCEPT_FACTS"] = c.value.body.value
                    out["ExitCodes"]["REJECT_FACTS"] = c.value.orelse.value
            elif isinstance(c.value, ast.Constant):
                out["ExitCodes"]["AcceptError"] = c.value.value
    out["StatusValues"] = sorted(out["StatusValues"])
    out["ManualChecks"] = sorted(set(out["ManualChecks"]))
    out["R4Checks"] = list(consts.get("R4_CHECKS") or [])
    r4p = set()
    for c in ast.walk(funcs.get("check_r4") or ast.Module(body=[], type_ignores=[])):
        if (isinstance(c, ast.Call) and isinstance(c.func, ast.Name) and c.func.id == "read_pinned" and c.args
                and isinstance(c.args[0], ast.Call) and isinstance(c.args[0].func, ast.Attribute) and c.args[0].func.attr == "get"
                and c.args[0].args and isinstance(c.args[0].args[0], ast.Constant)):
            r4p.add(c.args[0].args[0].value)
    out["R4Pinned"] = sorted(r4p)
    out["R4NotRunIsNonPass"] = ('r if r in ("pass", "fail", "not_run") else None' in src
                                and 'good = res is not None and ctl is not None and ((ctl == "pass") == res)' in src)
    out["R4AuthorityRule"] = 'row["Rule"] = "map part: any defect forbids pass"' in src and 'good = ctl is not None and not (defects and ctl == "pass")' in src
    out["R3NoRunsNoData"] = "def check_r3(ctx):\n    if not ctx.runs:\n        return None" in src
    out["R9NoRunsError"] = 'raise AcceptError("MISSING_INPUT", "no tool envelope to check against the closure")' in src
    out["NaWaivableOnly"] = "if na is not None and rid in WAIVABLE:\n                res = (\"NOT_APPLICABLE\"" in src
    out["NaWithDataMismatch"] = ("elif na is not None and res[0] != \"MANUAL\":\n            res = (\"MISMATCH\", "
                                 "{\"Problem\": \"declared not applicable but data was supplied\"") in src
    return out


def rset(s):
    out = set()
    for a, b in re.findall(r"\bR(\d+)-R(\d+)\b", s):
        out |= {"R%d" % i for i in range(int(a), int(b) + 1)}
    return out | set(re.findall(r"\bR\d+\b", re.sub(r"\bR\d+-R\d+\b", "", s)))


def g10_acceptor(inp):
    f, info = [], []
    md = inp["A6"] or ""
    rules = a6_rules(md)
    if "A6-5" not in rules or "A6-3" not in rules:
        return {"Check": "G10 resultado del aceptante", "Findings": ["falta A6-5 o A6-3"], "Result": "FAIL"}
    try:
        acc = analyze_acceptor(inp["AccSrc"])
    except SyntaxError:
        return {"Check": "G10 resultado del aceptante", "Findings": ["el aceptante fijado no se puede analizar"], "Result": "FAIL"}
    it = rule_items(rules["A6-5"]["Paragraph"])
    r2 = it.get(2, "")
    m = re.search(r"emite un objeto con `Checks` (R\d+-R\d+), cada una en ((?:`[A-Z_]+`(?:, | o )?)+), y una `Decision`, "
                  r"((?:`[A-Z_]+`(?: o )?)+); su código de salida es (\d+), (\d+) o (\d+) \(error de uso\)", r2)
    if not m:
        f.append("A6-5, regla 2: no se reconoce la descripción del objeto, la Decision y los códigos de salida")
    else:
        if sorted(rset(m.group(1)), key=lambda x: int(x[1:])) != acc["CheckIds"]:
            f.append("A6-5, regla 2: `Checks` %s distintas de CHECK_IDS del aceptante %s" % (m.group(1), acc["CheckIds"]))
        sts = sorted(re.findall(r"`([A-Z_]+)`", m.group(2)))
        if sts != acc["StatusValues"]:
            f.append("A6-5, regla 2: estados %s distintos de los que emite el aceptante %s" % (sts, acc["StatusValues"]))
        decs = sorted(re.findall(r"`([A-Z_]+)`", m.group(3)))
        if decs != sorted({acc.get("DecisionTrue"), acc.get("DecisionFalse")} - {None}):
            f.append("A6-5, regla 2: valores de Decision %s distintos de los del aceptante" % decs)
        codes = sorted(int(x) for x in m.group(4, 5, 6))
        if codes != sorted(acc["ExitCodes"].values()) or len(acc["ExitCodes"]) != 3:
            f.append("A6-5, regla 2: códigos de salida %s distintos de los del aceptante %s" % (codes, acc["ExitCodes"]))
        if acc["ExitCodes"].get("AcceptError") != int(m.group(6)):
            f.append("A6-5, regla 2: el código de error de uso no es el del aceptante (%s)" % acc["ExitCodes"].get("AcceptError"))
    notes = inp.get("AccNotes") or ""
    if "Exit code: 0 = `ACCEPT_FACTS`, 1 = `REJECT_FACTS`, 64 = usage error" not in notes:
        f.append("ACCEPT-NOTES en la base sin «Exit code: 0 = ACCEPT_FACTS, 1 = REJECT_FACTS, 64 = usage error»")
    if acc["ExitCodes"] != {"ACCEPT_FACTS": 0, "REJECT_FACTS": 1, "AcceptError": 64}:
        f.append("los códigos de salida del aceptante no son 0/1/64: %s" % acc["ExitCodes"])
    t = re.search(r"\*\*aceptada\*\* solo si el código es (\d+), la `Decision` es `([A-Z_]+)` y, además: (.+?) están en `MATCH`; (.+?) está en "
                  r"`MANUAL`; (R\d+) está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D\.6 la "
                  r"dejó fuera; y (.+?) están en `MATCH` o en `NOT_APPLICABLE`, esto último solo cuando la solicitud lo declaró por esta tabla", r2)
    if not t:
        f.append("A6-5, regla 2: no se reconoce la tabla de aceptación (MATCH, MANUAL, R4 condicionada a `Verification`, MATCH o NOT_APPLICABLE)")
        sets = None
    else:
        if int(t.group(1)) != acc["ExitCodes"].get("ACCEPT_FACTS") or t.group(2) != acc.get("DecisionTrue"):
            f.append("A6-5, regla 2: la aceptación no exige el código y la Decision de ACCEPT_FACTS del aceptante")
        sets = {"MATCH": rset(t.group(3)), "MANUAL": rset(t.group(4)), "NA": rset(t.group(6)) | {t.group(5)}}
        if t.group(5) != "R4" or "Verification" not in acc["RequestKeys"].get("R4", []):
            f.append("A6-5, regla 2: la comprobación condicionada a `Verification` (%s) no es la que lee `Verification` en el aceptante (R4)" % t.group(5))
        allc = set(acc["CheckIds"])
        if sets["MATCH"] & sets["MANUAL"] or sets["MATCH"] & sets["NA"] or sets["MANUAL"] & sets["NA"]:
            f.append("A6-5, regla 2: una comprobación aparece en más de un grupo de la tabla: %s" % {k: sorted(v) for k, v in sets.items()})
        if sets["MATCH"] | sets["MANUAL"] | sets["NA"] != allc:
            f.append("A6-5, regla 2: la tabla no cubre exactamente %s: faltan %s; sobran %s"
                     % (acc["CheckIds"], sorted(allc - (sets["MATCH"] | sets["MANUAL"] | sets["NA"])),
                        sorted((sets["MATCH"] | sets["MANUAL"] | sets["NA"]) - allc)))
        if not sets["NA"] <= set(acc["Waivable"]):
            f.append("A6-5, regla 2: admite NOT_APPLICABLE en %s, que el aceptante no permite declarar (WAIVABLE = %s)"
                     % (sorted(sets["NA"] - set(acc["Waivable"])), acc["Waivable"]))
        if sets["MANUAL"] != set(acc["ManualChecks"]):
            f.append("A6-5, regla 2: MANUAL en %s; el aceptante solo devuelve MANUAL en %s" % (sorted(sets["MANUAL"]), acc["ManualChecks"]))
        if set(acc["DecisionStatuses"]) != {"MATCH", "MANUAL", "NOT_APPLICABLE"} or not acc.get("DecisionAll"):
            f.append("la fórmula de Decision del aceptante no es ACCEPT_FACTS si y solo si toda comprobación está en MATCH, NOT_APPLICABLE o MANUAL: %s"
                     % acc["DecisionStatuses"])
        allowed = {"MATCH"} | ({"MANUAL"} if sets["MANUAL"] else set()) | ({"NOT_APPLICABLE"} if sets["NA"] else set())
        if not allowed <= set(acc["DecisionStatuses"]):
            f.append("A6-5, regla 2: una salida conforme a la tabla no da ACCEPT_FACTS con el aceptante (estados %s fuera de su fórmula %s)"
                     % (sorted(allowed - set(acc["DecisionStatuses"])), acc["DecisionStatuses"]))
        stricter = sorted(set(acc["Waivable"]) - sets["NA"])
        if set(acc["DecisionStatuses"]) - {"MATCH"} and "La `Decision` sola no basta" not in r2:
            f.append("A6-5, regla 2: el aceptante da ACCEPT_FACTS con NOT_APPLICABLE o MANUAL y la regla no dice que la Decision sola no basta")
        info.append("la tabla exige MATCH en %s, que el aceptante permite declarar no aplicables: más estricta que su Decision" % stricter)
        if not acc["NaWaivableOnly"] or not acc["NaWithDataMismatch"]:
            f.append("el aceptante no trata NotApplicable como se supone (solo WAIVABLE sin datos; con datos, MISMATCH)")
    na = re.search(r"\(R5: (.+?); R6: (.+?); R7: (.+?)\)", r2)
    if not na:
        f.append("A6-5, regla 2: no se reconocen los conjuntos de órdenes de R5, R6 y R7")
    else:
        for rid, cell in zip(("R5", "R6", "R7"), na.groups()):
            cmds = sorted(re.findall(r"`([a-z]+)`", cell))
            if cmds != acc["Commands"].get(rid):
                f.append("A6-5, regla 2: %s compara %s según el aceptante, no %s" % (rid, acc["Commands"].get(rid), cmds))
            if rid in acc["Branches"] and acc["Branches"][rid] != acc["Commands"].get(rid):
                f.append("aceptante: %s selecciona %s pero compara %s" % (rid, acc["Commands"].get(rid), acc["Branches"][rid]))
            for extra in acc["ExtraSources"].get(rid, []):
                if extra not in cell:
                    f.append("A6-5, regla 2: %s también compara `%s` de la verificación (check_%s del aceptante), que la tabla no nombra: con `%s` "
                             "no vacío, una declaración NotApplicable de %s da MISMATCH" % (rid, extra, rid.lower(), extra, rid))
    # condición de R4 (A6-3, regla 6 (c)) frente al aceptante: los tres archivos que lee y las cinco comprobaciones que recalcula
    a3c = rule_items(rules["A6-3"]["Paragraph"]).get(6, "")
    if acc.get("R4Pinned") != ["Controller", "Delegation", "Handoff"]:
        f.append("aceptante: R4 no lee exactamente la salida del Controller, la delegación y la entrega (%s)" % acc.get("R4Pinned"))
    if "`Verification` = la salida `controller-verification/v2` de la invocación" not in a3c:
        f.append("A6-3, regla 6 (c): la solicitud no define `Verification` como la salida `controller-verification/v2` de la invocación")
    if "solo si esos tres archivos existen y son legibles" not in a3c:
        f.append("A6-3, regla 6 (c): `Verification` no se condiciona a que existan los tres archivos que lee R4")
    mm = re.search(r"ninguna de las comprobaciones ((?:`[A-Za-z]+`(?:, | y )?)+) está en `not_run`", a3c)
    five = set(re.findall(r"`([A-Za-z]+)`", mm.group(1))) if mm else set()
    if not acc.get("R4Checks") or five != set(acc["R4Checks"]) - {"Authority"}:
        f.append("A6-3, regla 6 (c): las comprobaciones de la condición de `not_run` (%s) no son las que R4 recalcula (%s, sin Authority)"
                 % (sorted(five), acc.get("R4Checks")))
    if not acc.get("R4NotRunIsNonPass"):
        f.append("aceptante: R4 no trata `not_run` como distinto de `pass`, como dice A6-5, regla 2")
    if not acc.get("R4AuthorityRule") or "exige que una `Authority` en `pass` no tenga defectos de mapa" not in r2:
        f.append("A6-5, regla 2: la regla de `Authority` de R4 no coincide con el aceptante (un defecto de mapa prohíbe pass)")
    if not (acc.get("R3NoRunsNoData") and acc.get("R9NoRunsError")) or "Ninguna está obligada a ejecutar el instrumento" not in it.get(7, ""):
        f.append("A6-5, regla 7: sin `Runs`, el aceptante fijado nunca acepta (R3 sin datos, R9 sin sobres); la regla debe excluir esas invocaciones")
    # entradas de la solicitud que necesitan las comprobaciones exigidas, frente a A6-3, reglas 5-6
    a3 = rule_items(rules["A6-3"]["Paragraph"])
    a3t = a3.get(5, "") + " " + a3.get(6, "")
    if sets:
        need = {}
        for rid in sorted(sets["MATCH"] | sets["NA"]):
            for k in acc["RequestKeys"].get(rid, []):
                need.setdefault(k, set()).add(rid)
        if sets["NA"]:
            need.setdefault("NotApplicable", set()).update(sets["NA"])
        for k, rids in sorted(need.items()):
            terms = REQ_KEY_TERMS.get(k, ["`%s`" % k])
            if not any(x in a3t for x in terms):
                f.append("A6-3, reglas 5-6: la solicitud del aceptante no incluye `%s`, que necesita %s (exigida en %s por A6-5, regla 2)"
                         % (k, ", ".join(sorted(rids, key=lambda x: int(x[1:]))), "MATCH" if rids & sets["MATCH"] else "MATCH o NOT_APPLICABLE"))
    if not re.search(r"`NotApplicable`, solo por la tabla de la regla de A-6 (?:sobre el resultado del aceptante|en D\.4)", a3.get(6, "")):
        f.append("A6-3, regla 6: `NotApplicable` no se limita a la tabla de A6-5")
    # estados normativos nombrados por A6-5: en los bloques literales de §2
    blocks, _ = literal_blocks(md)
    src_lines = {k: inp["Sources"][k].split("\n") for k in ("V14", "AP", "RAE", "ADR-0046", "ADR-0048", "LIFECYCLE")}
    quoted = "\n".join("\n".join(l) for _, _, _, l, _ in blocks)
    ok_blocks = all(l == src_lines[t][a - 1:b] for t, a, b, l, _ in blocks)
    if not ok_blocks:
        f.append("algún bloque literal de §2 difiere de su fuente (ver G1): los estados no se pueden acreditar")
    para5 = rules["A6-5"]["Paragraph"]
    for tok in sorted(set(STATE_TOKENS.findall(para5)) | {"INVALID_OUTPUT", "P-19", "P-04", "S-03", "S-04", "not_run"}):
        if tok not in para5:
            f.append("A6-5 no nombra «%s»" % tok)
        if not re.search(r"(?<![\w-])%s(?![\w-])" % re.escape(tok), quoted):
            where = ["%s L%d" % (k, i + 1) for k in ("AP", "RAE", "V14") for i, l in enumerate(src_lines[k])
                     if re.search(r"(?<![\w-])%s(?![\w-])" % re.escape(tok), l)][:3]
            f.append("A6-5 nombra «%s», que no está en ningún bloque literal de §2 (está en %s)" % (tok, where or "ninguna fuente"))
    for tok in sorted(set(re.findall(r"\b(?:ACCEPT|REJECT)_FACTS\b|\bNOT_APPLICABLE\b|\bMISMATCH\b|\bMANUAL\b", para5))):
        if tok not in acc["StatusValues"] + [acc.get("DecisionTrue"), acc.get("DecisionFalse")]:
            f.append("A6-5 nombra «%s», que el aceptante no emite" % tok)
    for k, ph, tag, line, sph in CORRESPONDENCE:
        if norm(ph) not in it.get(k, ""):
            f.append("A6-5, regla %d: falta la correspondencia «%s»" % (k, ph[:70]))
        if sph not in src_lines[tag][line - 1]:
            f.append("%s L%d (fijado) sin «%s»" % (tag, line, sph[:60]))
        if not any(t == tag and a <= line <= b for t, a, b, _, _ in blocks):
            f.append("la correspondencia «%s» se apoya en %s L%d, que §2 no cita literalmente" % (ph[:40], tag, line))
    return {"Check": "G10 resultado del aceptante frente a sus bytes y estados normativos citados", "Acceptor": {k: v for k, v in acc.items()},
            "Findings": f, "Info": info, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G11
def heading_ranges(text, pat):
    lines = text.split("\n")
    heads = [(i, int(m.group(1))) for i, l in enumerate(lines) for m in [re.match(pat, l)] if m]
    out = {}
    for k, (i, n) in enumerate(heads):
        end = heads[k + 1][0] if k + 1 < len(heads) else len(lines)
        while end > i + 1 and not lines[end - 1].strip():
            end -= 1
        out[n] = (i + 1, end)
    return out


def nlines(text):
    if text is None:
        return 0
    return len(text.split("\n")) - (1 if text.endswith("\n") else 0)


def sec_numbers(spec):
    out = set()
    for a, b in re.findall(r"§(\d+)-§(\d+)", spec):
        out |= set(range(int(a), int(b) + 1))
    return out | {int(x) for x in re.findall(r"§(\d+)", re.sub(r"§\d+-§\d+", "", spec))}


def g11_sources(inp):
    f, refs = [], []
    md = inp["A6"] or ""
    t = norm(md)
    files = {}
    tagfile = {k: (p, b, inp["Sources"].get(k)) for k, (p, b) in SOURCES.items()}
    bb = inp.get("BaseBlobs") or {}
    tagfile["DEC"] = (DEC, bb.get(DEC), inp["Sources"].get("DEC"))
    pblobs = {p: text_blob(t) if t is not None else None for p, t in (inp.get("PathFiles") or {}).items()}

    def add(path, blob, text, a, b, ref):
        n = nlines(text)
        e = files.setdefault(path, {"Path": path, "Blob": blob, "Lines": n, "Ranges": [], "Refs": 0})
        e["Blob"] = e["Blob"] or blob
        e["Refs"] += 1
        if text is None:
            f.append("referencia a un archivo que no existe en la base: %s" % ref)
            return
        if a < 1 or b < a:
            f.append("rango de líneas inválido en «%s»" % ref)
            return
        if b > n:
            f.append("línea fuera del archivo en «%s» (%s tiene %d líneas)" % (ref, path, n))
            return
        e["Ranges"].append([a, b])
    for m in G11_REF.finditer(t):
        tag = m.group(1)
        path, blob, text = tagfile[tag]
        for a, b in re.findall(r"L(\d+)(?:-L(\d+))?", m.group(2)):
            a = int(a)
            b = int(b) if b else (nlines(text) if m.group(3) else a)
            if m.group(3) and not re.search(r"L\d+-L\d+", m.group(2)):
                b = nlines(text)
            refs.append(m.group(0))
            add(path, blob, text, a, b, m.group(0))
    for m in G11_PATHREF.finditer(t):
        p, a = m.group(1), int(m.group(2))
        b = int(m.group(3)) if m.group(3) else a
        cands = [p] if "/" in p else (inp.get("Basenames") or {}).get(p, [])
        if len(cands) != 1:
            f.append("referencia «%s» sin un archivo único (%d candidatos)" % (m.group(0), len(cands)))
            continue
        text = (inp.get("PathFiles") or {}).get(cands[0])
        refs.append(m.group(0))
        add(cands[0], pblobs.get(cands[0]), text, a, b, m.group(0))
    dec = inp["Sources"].get("DEC") or ""
    decs = heading_ranges(dec, r"^## (\d+)\. ")
    evs = heading_ranges(inp.get("Evidence") or "", r"^## (\d+)\. ")
    sections_out = []
    for rx, table, path, text in ((G11_DECSEC, decs, DEC, dec), (G11_EVSEC, evs, EVIDENCE_FILE, inp.get("Evidence") or "")):
        for m in rx.finditer(t):
            for n in sorted(sec_numbers(m.group(1))):
                if n not in table:
                    f.append("«%s»: §%d no existe en %s en la base" % (m.group(0), n, path))
                    continue
                a, b = table[n]
                sections_out.append({"Path": path, "Section": n, "Lines": [a, b]})
                add(path, bb.get(path), text, a, b, "%s §%d" % (path, n))
    for tag, a, b in G11_REQUIRED:
        path = tagfile[tag][0]
        if not any(x[0] <= a and b <= x[1] for x in files.get(path, {}).get("Ranges", [])):
            f.append("las fuentes del revisor no incluyen %s L%d-L%d (decisiones §79, punto 5)" % (tag, a, b))
    for r in G11_REQUIRED_REFS:
        if r not in refs:
            f.append("las fuentes del revisor no incluyen «%s» (fuente añadida en la ronda 4, §2)" % r)
    out = []
    for p, e in sorted(files.items()):
        rs = sorted(e["Ranges"])
        merged = []
        for a, b in rs:
            if merged and a <= merged[-1][1] + 1:
                merged[-1][1] = max(merged[-1][1], b)
            else:
                merged.append([a, b])
        e["Ranges"] = merged
        out.append(e)
    uniq = []
    for s in sections_out:
        if s not in uniq:
            uniq.append(s)
    # rutas del repositorio que el texto nombra (archivos o árboles completos)
    rp = inp.get("RepoPaths") or {}
    r2 = r2_ids(inp)
    paths_out, pending = [], []
    for p in sorted(set(REPO_PATH.findall(md))):
        if p == A6:
            continue
        if p.startswith(EVID_R4):
            rel = p[len(EVID_R4):]
            if r2 and not any(q == rel or q.startswith(rel.rstrip("/") + "/") for q in r2) and rel:
                f.append("ruta del paquete r4/ que no existe en el paquete: %s" % p)
            pending.append(p)
            continue
        oid = rp.get(p)
        if not oid:
            f.append("ruta citada que no existe en la base: %s" % p)
            continue
        paths_out.append({"Path": p, "Object": oid})
    if R3_REVIEW not in {x["Path"] for x in paths_out}:
        f.append("las fuentes del revisor no incluyen la salida de la revisión de la ronda 3 (%s)" % R3_REVIEW)
    # líneas en que se apoyan G9 y G10
    gs = []
    for path, blob, ranges in GUARD_SOURCES:
        if path == ACC:
            ranges = acceptor_ranges(inp.get("AccSrc") or "")
            text = inp.get("AccSrc")
        elif path == KITCC:
            text = inp["Sources"].get("KIT")
        elif path == ACC_NOTES:
            text = (inp.get("PathFiles") or {}).get(ACC_NOTES)
            blob = text_blob(text) if text is not None else None
        else:
            text = inp["Sources"].get({AP: "AP", RAE: "RAE", V14: "V14"}[path])
        n = nlines(text)
        bad = [list(r) for r in ranges if not (1 <= r[0] <= r[1] <= n)]
        if bad:
            f.append("líneas de las guardas fuera de %s: %s" % (path, bad))
        gs.append({"Path": path, "Blob": blob, "Lines": n, "Ranges": [list(r) for r in ranges]})
    return {"Check": "G11 fuentes y rangos de líneas para el revisor siguiente", "Refs": len(refs), "Sources": out, "Sections": uniq,
            "Paths": paths_out, "PendingR4Paths": pending, "GuardSources": gs, "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G5b
MIN_MUTANTS = 150


def g5b_selftest(inp):
    f = []
    st = inp.get("SelfTest")
    if not st:
        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a6r4-selftest.json"], "Result": "FAIL"}
    if st.get("Result") != "PASS":
        f.append("a6r4-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a6r4-guards.py")
    if st.get("A6TextBlob") != text_blob(inp["A6"] or ""):
        f.append("A6TextBlob de la autoprueba distinto del blob de la A-6 comprobada")
    s = st.get("Summary") or {}
    if (not s.get("Mutants") or s.get("Killed") != s.get("Mutants") or s.get("ControlFailures") or s.get("NotApplied")
            or s.get("Mutants", 0) < MIN_MUTANTS):
        f.append("autoprueba con menos de %d mutantes, mutantes vivos, sin aplicar o controles fallidos: %s"
                 % (MIN_MUTANTS, {k: s.get(k) for k in ("Mutants", "Killed", "NotApplied")}))
    by = s.get("ByGuard") or {}
    for g in GUARDS_FN:
        if g == "G5B":
            continue
        k, _, n = (by.get(g) or "0/0").partition("/")
        if not n or int(n) == 0 or k != n:
            f.append("autoprueba sin mutantes muertos para %s (%s)" % (g, by.get(g)))
    return {"Check": "G5b autoprueba custodiada", "Summary": s, "Findings": f, "Result": "PASS" if not f else "FAIL"}


GUARDS_FN = {"G1": lambda inp, cm, ids: g1_literals(inp, ids), "G2": lambda inp, cm, ids: g2_paths(inp, ids), "G3": lambda inp, cm, ids: g3_surfaces(inp),
             "G4": lambda inp, cm, ids: g4_ruling(inp), "G5": lambda inp, cm, ids: g5_delta(inp, cm), "G6": lambda inp, cm, ids: g6_owner(inp),
             "G7": lambda inp, cm, ids: g7_instruments(inp), "G8": lambda inp, cm, ids: g8_status(inp), "G9": lambda inp, cm, ids: g9_scope(inp),
             "G10": lambda inp, cm, ids: g10_acceptor(inp), "G11": lambda inp, cm, ids: g11_sources(inp),
             "G5B": lambda inp, cm, ids: g5b_selftest(inp)}


# ---------------------------------------------------------------- autoprueba por mutantes
SYNTH_ID = "zq-host-" + "user-7b2e"
SYNTH_WIN_PATH = "C:" + "\\" + "Us" + "ers" + "\\" + "someone" + "\\" + "x"
SYNTH_MAIL = "a.b" + "@" + "example" + ".com"
EV_HEAD = "## 10. Evidencia\n"


def m_text(old, new, count=1):
    def f(inp):
        assert old in inp["A6"], "mutante no aplicable: %s" % old[:40]
        inp["A6"] = inp["A6"].replace(old, new, count)
    return f


def m_re(pat, new):
    def f(inp):
        t, n = re.subn(pat, new, inp["A6"], count=1, flags=re.M | re.S)
        assert n == 1, "mutante no aplicable: %s" % pat[:40]
        inp["A6"] = t
    return f


def m_set(path_keys, value):
    def f(inp):
        d = inp
        for k in path_keys[:-1]:
            d = d[k]
        d[path_keys[-1]] = value
    return f


def m_status(path, status, text="x\n"):
    def f(inp):
        inp["Status"][path] = status
        inp["HeadTexts"][path] = text
    return f


def m_in_delta(rid, phrase, new):
    """Sustituye cada aparición de la frase dentro del párrafo citado del delta (con saltos de línea y «> » entre palabras)."""
    def f(inp):
        md = inp["A6"]
        r = a6_rules(md).get(rid)
        assert r, "mutante no aplicable: %s" % rid
        a, b = r["BlockSpan"]
        blk, n = re.subn(phrase_regex(phrase), new, md[a:b])
        assert n >= 1, "mutante no aplicable: %s" % phrase[:40]
        inp["A6"] = md[:a] + blk + md[b:]
    return f


def m_rule_item(rid, k, new_first_words):
    """Reescribe el comienzo de la regla k del delta (desactiva la regla sin tocar la numeración)."""
    def f(inp):
        md = inp["A6"]
        r = a6_rules(md).get(rid)
        assert r, "mutante no aplicable: %s" % rid
        a, b = r["BlockSpan"]
        blk = md[a:b]
        mm = re.search(r"(?m)^> %d\. (.*)$" % k, blk)
        assert mm, "mutante no aplicable: %s regla %d" % (rid, k)
        blk = blk[:mm.start(1)] + new_first_words + blk[mm.end(1):]
        inp["A6"] = md[:a] + blk + md[b:]
    return f


def m_drop_delta(rid):
    def f(inp):
        md = inp["A6"]
        r = a6_rules(md).get(rid)
        assert r, "mutante no aplicable: %s" % rid
        a, b = r["BlockSpan"]
        inp["A6"] = md[:a] + "> «»" + md[b:]
    return f


def m_acc(old, new):
    def f(inp):
        assert old in inp["AccSrc"], "mutante no aplicable: %s" % old[:40]
        inp["AccSrc"] = inp["AccSrc"].replace(old, new, 1)
    return f


def m_src_line(tag, line, old, new):
    def f(inp):
        ls = inp["Sources"][tag].split("\n")
        assert old in ls[line - 1], "mutante no aplicable: %s L%d" % (tag, line)
        ls[line - 1] = ls[line - 1].replace(old, new, 1)
        inp["Sources"][tag] = "\n".join(ls)
    return f


def m_insert_ev(text):
    return m_text(EV_HEAD, EV_HEAD + "\n" + text + "\n")


def m_cell(table_start, row_prefix, col, new):
    """Sustituye la celda col (0 = primera) de la fila de la tabla de la sección que empieza por row_prefix."""
    def f(inp):
        md = inp["A6"]
        s, e = section(md, table_start, r"^## |^### ")
        assert s is not None, "mutante no aplicable: %s" % table_start
        for mm in re.finditer(r"(?m)^\|.*$", md[s:e]):
            cells = mm.group(0).strip().strip("|").split("|")
            if cells and cells[0].strip().startswith(row_prefix):
                cells[col] = " " + new + " "
                line = "|" + "|".join(cells) + "|"
                inp["A6"] = md[:s + mm.start()] + line + md[s + mm.end():]
                return
        raise AssertionError("mutante no aplicable: fila %s" % row_prefix)
    return f


def m_seq(*fs):
    """Aplica varios mutantes en orden sobre la misma entrada (ronda 2: mutantes compuestos)."""
    def f(inp):
        for g in fs:
            g(inp)
    return f


def m_row_sub(table_start, row_prefix, old, new):
    """Sustituye todas las apariciones de old en la fila de la tabla de la sección cuya primera celda empieza por row_prefix."""
    def f(inp):
        md = inp["A6"]
        s, e = section(md, table_start, r"^## |^### ")
        assert s is not None, "mutante no aplicable: %s" % table_start
        for mm in re.finditer(r"(?m)^\|.*$", md[s:e]):
            cells = mm.group(0).strip().strip("|").split("|")
            if cells and cells[0].strip().startswith(row_prefix) and old in mm.group(0):
                inp["A6"] = md[:s + mm.start()] + mm.group(0).replace(old, new) + md[s + mm.end():]
                return
        raise AssertionError("mutante no aplicable: fila %s" % row_prefix)
    return f


def m_cell_append(table_start, row_prefix, col, suffix):
    """Añade suffix al final de la celda col de la fila de la tabla de la sección cuya primera celda empieza por row_prefix."""
    def f(inp):
        md = inp["A6"]
        s, e = section(md, table_start, r"^## |^### ")
        assert s is not None, "mutante no aplicable: %s" % table_start
        for mm in re.finditer(r"(?m)^\|.*$", md[s:e]):
            cells = mm.group(0).strip().strip("|").split("|")
            if cells and cells[0].strip().startswith(row_prefix):
                cells[col] = cells[col].rstrip() + suffix + " "
                inp["A6"] = md[:s + mm.start()] + "|" + "|".join(cells) + "|" + md[s + mm.end():]
                return
        raise AssertionError("mutante no aplicable: fila %s" % row_prefix)
    return f


def m_r6b(fn):
    """Reescribe las líneas de la regla 6 del bloque citado de A6-3 con fn(inp, líneas, partes)."""
    def f(inp):
        md = inp["A6"]
        r = a6_rules(md).get("A6-3")
        assert r, "mutante no aplicable: A6-3"
        a, b = r["BlockSpan"]
        ls, parts = r6_parts(md[a:b])
        new = fn(inp, ls, parts)
        assert new is not None and new != ls, "mutante no aplicable: regla 6 de A6-3"
        inp["A6"] = md[:a] + "\n".join(new) + md[b:]
    return f


def m_drop_r6b(key):
    """Retira de la regla 6 de A6-3 la parte key (un apartado (i)-(v) o la frase final)."""
    def fn(inp, ls, parts):
        for k, i0, i1 in parts:
            if k == key:
                return ls[:i0] + ls[i1:]
        return None
    return m_r6b(fn)


def m_r6b_prev():
    """Vuelve a poner en A6-3 el apartado (b) de la regla 6 de la ronda 3."""
    def fn(inp, ls, parts):
        pls, pparts = r6_parts(a6_rules(inp["R1Text"])["A6-3"]["Block"])
        pb = [pls[i] for k, i0, i1 in pparts if k == "b" for i in range(i0, i1)]
        idx = [i for k, i0, i1 in parts if k in R6B_KEYS for i in range(i0, i1)]
        if not idx or not pb:
            return None
        return ls[:idx[0]] + pb + ls[idx[-1] + 1:]
    return m_r6b(fn)


def m_r6b_insert(before_key, line):
    """Inserta una línea citada en la regla 6 de A6-3 antes de la parte before_key."""
    def fn(inp, ls, parts):
        for k, i0, i1 in parts:
            if k == before_key:
                return ls[:i0] + [line] + ls[i0:]
        return None
    return m_r6b(fn)


OWN_A = "A6-ADR0046-9-FX02 = A ("
OWN_B = "\nA6-CONTROLLER-MEDICION-CONSUMO = A ("

MUTANTS = [
    # G1: cabecera, literales, citas, identificadores, referencias a reglas y privacidad
    ("G1", "literal cambiado en un carácter (V14 L2523)", m_text("V14 L2523 «| A | ≤ 15 + 2 sondas", "V14 L2523 «| A | ≤ 16 + 2 sondas")),
    ("G1", "referencia de línea cambiada (A-4 L203)", m_text("A-4 L203: «Su tope", "A-4 L204: «Su tope")),
    ("G1", "FREEZE_SHA cambiado", m_text("FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43", "FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd44")),
    ("G1", "blob de A-5 de la cabecera cambiado", m_text("blob ad94ceedacbf3dbf70f0217a583978d0e32592fc). A-6", "blob ad94ceedacbf3dbf70f0217a583978d0e32592fd). A-6")),
    ("G1", "veredicto del Coordinator añadido a la cabecera", m_text("Owner decision:\n", "Coordinator verdict:\nAGREED\n\nOwner decision:\n")),
    ("G1", "ronda anterior con otro commit", m_text("(commit 7b7aba46). Revisión", "(commit 108d4ca1). Revisión")),
    ("G1", "ronda anterior con otro blob", m_text("ronda 1 = blob eac473c0d2e58cf1c4594f48a8816b3833af6509", "ronda 1 = blob c5cc6168d4a29f94623a8fab90a2d4f5f8df9ffa")),
    ("G1", "Scope sin VERIFY", m_text("Scope:\nsolo las invocaciones VERIFY", "Scope:\nlas invocaciones")),
    ("G1", "Scope sin la exclusión de PLAN", m_text("La\nplanificación (PLAN) sigue el procedimiento vigente, sin el instrumento", "La\nplanificación (PLAN) también usa el instrumento")),
    ("G1", "cabecera: Architect review con la ronda 4 AGREED", m_text("Ronda 4: NOT LAUNCHED. Ninguna", "Ronda 4: AGREED. Ninguna")),
    ("G1", "cabecera: la ronda 3 vuelve a NOT LAUNCHED", m_text("Ronda 3:\nCHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80)", "Ronda 3:\nNOT LAUNCHED")),
    ("G1", "cabecera: la ronda 3 AGREED", m_text("Ronda 3:\nCHANGES REQUIRED, ACCREDITED", "Ronda 3:\nAGREED, ACCREDITED")),
    ("G1", "cabecera: la ronda 3 NOT_ACCREDITED", m_text("Ronda 3:\nCHANGES REQUIRED, ACCREDITED", "Ronda 3:\nCHANGES REQUIRED, NOT_ACCREDITED")),
    ("G1", "cabecera: la ronda 3 con otra revisión", m_text("(commit a128efef). Revisión formal R20261013T183701Z-9f80", "(commit a128efef). Revisión formal R20261013T183701Z-9f81")),
    ("G1", "cabecera: la ronda 3 con otro commit", m_text("e62f27b8b1329f1b9cfa69a707feb31d22bad1d1 (commit a128efef)", "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1 (commit 9cd074aa)")),
    ("G1", "cabecera: hallazgos de la ronda 3 sin O7", m_text("(A62-A6R3-R1; A62-A6R3-O1..O7)", "(A62-A6R3-R1; A62-A6R3-O1..O6)")),
    ("G1", "cabecera: A62-A6R2-R2 sin CORRECTED", m_text("A62-A6R2-R2 CORRECTED", "A62-A6R2-R2 OPEN")),
    ("G1", "cabecera: Classification amplía la ronda 4 a A6-9", m_text("en la ronda 4, solo en A6-3, regla 6 (b) (§3.18)", "en la ronda 4, en A6-3 y A6-9 (§3.18)")),
    ("G1", "cabecera: Authority con las rondas 2 y 3", m_text("que las rondas 2 a 4 no amplían", "que las rondas 2 y 3 no amplían")),
    ("G1", "cabecera: ronda 4 con CHANGES REQUIRED", m_text("Ronda 4: NOT LAUNCHED.", "Ronda 4: CHANGES REQUIRED.")),
    ("G1", "commit de la ronda 3 sin la ronda 3", m_set(["R3CommitA6"], "0" * 40)),
    ("G1", "prefijo de la ronda 3 falso (e62f27b8…)", m_text("el blob `e62f27b8…` del commit `a128efef`", "el blob `e62f27b9…` del commit `a128efef`")),
    ("G1", "prefijo del árbol de la revisión de la ronda 3 falso", m_text("`I-62-architect-A-6r3/` `f4442fdf…`", "`I-62-architect-A-6r3/` `f4442fde…`")),
    ("G1", "prefijo de output.json de la revisión de la ronda 3 falso", m_text("(`output.json` `691bbc8f…`)", "(`output.json` `691bbc8e…`)")),
    ("G1", "prefijo del árbol r3/ falso", m_text("árbol `I-62-A6/r3/` `a4e93edb…`", "árbol `I-62-A6/r3/` `a4e93edc…`")),
    ("G1", "commit de la ronda 2 sin la ronda 2", m_set(["R2CommitA6"], "0" * 40)),
    ("G1", "prefijo de la ronda 2 falso (86da0ae7…)", m_text("el blob `86da0ae7…` del commit `ae9fe0e9`", "el blob `86da0ae8…` del commit `ae9fe0e9`")),
    ("G1", "prefijo del árbol de la ronda 1 de I-62-A6/ falso (7f504a17…)", m_text("de la ronda 1 `7f504a17…` fuera de `r2/`", "de la ronda 1 `7f504a18…` fuera de `r2/`")),
    ("G1", "prefijo del árbol de la revisión de la ronda 2 falso", m_text("`I-62-architect-A-6r2/` `acf2fa30…`", "`I-62-architect-A-6r2/` `acf2fa31…`")),
    ("G1", "cabecera: Owner decision REQUIRED (línea pendiente)", m_text("Owner decision:\n(a) A6-ADR0046-9-FX02 = A, registrada", "Owner decision:\nREQUIRED (a) A6-ADR0046-9-FX02 = A, registrada")),
    ("G1", "cabecera: auditor ACCREDITED", m_text("auditor NOT_ACCREDITED, sin acreditación", "auditor ACCREDITED, sin acreditación")),
    ("G1", "cabecera: M-01 sí", m_text("M-05 y M-08 sí; M-01, M-06 y M-07 no, reevaluadas", "M-05, M-08 y M-01 sí; M-06 y M-07 no, reevaluadas")),
    ("G1", "cita sin clasificar", m_text("## 1. Alcance\n", "## 1. Alcance\n\nUna cita «texto que no existe en ninguna fuente».\n")),
    ("G1", "cita con referencia de línea que no la contiene (V14 L2536)", m_insert_ev("V14 L2536: «capacidad medida presente».")),
    ("G1", "línea de un bloque literal de §2 cambiada (receta 16.4)", m_text("`-s read-only`; `-m`", "`-s workspace-write`; `-m`")),
    ("G1", "línea de un bloque nuevo de §2 cambiada (AP L653)", m_text("> | Salida conforme pero vacía o falsa | sesión (`INVALID_OUTPUT`)", "> | Salida conforme pero vacía o falsa | sesión (`INVALID_RESULT`)")),
    ("G1", "segundo bloque de una cabecera doble cambiado (AP L1240)", m_text("> PLAN y VERIFY no son intercambiables. El triple", "> PLAN y VERIFY son intercambiables. El triple")),
    ("G1", "cabecera de bloque con otro rango (RAE L219-L226 → L218-L225)", m_text("RAE L219-L226, literal", "RAE L218-L225, literal")),
    ("G1", "bloque citado sin cabecera literal en §2", m_text("Revisión de la ronda 1 (`docs/automation/evidence/I-62-architect-A-6/", "> línea citada sin fuente\n\nRevisión de la ronda 1 (`docs/automation/evidence/I-62-architect-A-6/")),
    ("G1", "línea citada fuera de §2 y de los deltas", m_insert_ev("> una cita en bloque fuera de §2")),
    ("G1", "prefijo de blob abreviado falso", m_text("AUTOMATION_PLAN `f525cb1e…`", "AUTOMATION_PLAN `f525cb1f…`")),
    ("G1", "prefijo del árbol de la revisión de la ronda 1 falso", m_text("`I-62-architect-A-6/` `520e7a1f…`", "`I-62-architect-A-6/` `520e7a1e…`")),
    ("G1", "prefijo sin fijar ni del paquete r2/", m_insert_ev("Objeto `abcdef12…`.")),
    ("G1", "commit abreviado inexistente", m_insert_ev("Commit `deadbee1`.")),
    ("G1", "ancla citada alterada (V14 L2587)", m_text("termina en «automáticas y la auditoría, con su límite.»", "termina en «automáticas y la auditoría, sin su límite.»")),
    ("G1", "cita de decisiones alterada (DEC L1541)", m_text("«El Coordinator no sustituye esa decisión»", "«El Coordinator sustituye esa decisión»")),
    ("G1", "cita de la línea (a) alterada («en su verificación»)", m_text("«en su verificación»; decisiones §78", "«en su planificación»; decisiones §78")),
    ("G1", "cita de la ronda 1 alterada («un clon del fixture»)", m_text("«un clon del fixture»", "«un clon cualquiera del fixture»")),
    ("G1", "cita interna del delta alterada («**Codex, total**»)", m_in_delta("A6-1", "«**Codex, total**»", "«**Codex total**»")),
    ("G1", "identificador de 40 hexadecimales inexistente", m_insert_ev("Objeto 0123456789abcdef0123456789abcdef01234567.")),
    ("G1", "SHA-256 sin fijar", m_insert_ev("SHA-256 " + "ab" * 32 + ".")),
    ("G1", "SHA-256 de la línea (a) alterado", m_insert_ev("SHA-256 de (a): " + OWNER_A_SHA256[:-1] + "9.")),
    ("G1", "referencia a una regla inexistente (A6-1, regla 12)", m_insert_ev("Ver A6-1, regla 12.")),
    ("G1", "referencia a un apartado inexistente (A6-3, regla 3 (xi))", m_insert_ev("Ver A6-3, regla 3 (xi).")),
    ("G1", "referencia de la §3.16 a una regla inexistente (A6-8, regla 9)", m_text("A6-7, regla 1; A6-8, reglas 1 y 5 |", "A6-7, regla 1; A6-8, reglas 1 y 9 |")),
    ("G1", "texto conservado de la ronda 1 con una regla renumerada (§6.2: A6-7, regla 4 → 2)", m_text("(A6-7, regla 4) | ninguno (tratado en A6-7, reglas 3 y 4)", "(A6-7, regla 2) | ninguno (tratado en A6-7, reglas 3 y 4)")),
    ("G1", "referencia de la ronda 1 sin actualizar tras la renumeración (§3.3: A6-1, regla 8 → 6)", m_text("(A6-1,\nregla 8; A6-7, regla 5)", "(A6-1,\nregla 6; A6-7, regla 5)")),
    ("G1", "ruta de perfil de usuario", m_insert_ev("Ruta " + SYNTH_WIN_PATH)),
    ("G1", "correo", m_insert_ev("Contacto " + SYNTH_MAIL)),
    ("G1", "identidad del host", m_insert_ev("Sesión de " + SYNTH_ID)),
    ("G1", "fuente fijada con contenido alterado", m_set(["SourceBlobOk", "AP"], False)),
    ("G1", "commit de la ronda 1 sin la ronda 1", m_set(["R1CommitA6"], "0" * 40)),
    ("G1", "commit declarado de los literales con otro AUTOMATION_PLAN", lambda inp: inp["PrepBlobs"].__setitem__(AP, "0" * 40)),
    ("G1", "decisiones en 1652dd61 distintas de las fijadas", lambda inp: inp["PrepBlobs"].__setitem__(DEC, "0" * 40)),
    ("G1", "decisiones reescritas en la base (no solo por añadido)", lambda inp: inp["Sources"].__setitem__("DEC", inp["Sources"]["DEC"].replace("El Coordinator no sustituye esa decisión", "El Coordinator sí sustituye esa decisión", 1))),
    # G2: rutas del commit
    ("G2", "ruta de producción en el commit", m_status("src/RackCad.Core/Nuevo.cs", "A")),
    ("G2", "V14 modificada en el commit", m_status(V14, "M")),
    ("G2", "A-6 como añadida (A)", m_set(["Status", A6], "A")),
    ("G2", "A-6 ausente del commit", lambda inp: inp["Status"].pop(A6)),
    ("G2", "base sin la ronda 1", m_set(["BaseA6"], "1" * 40)),
    ("G2", "evidencia de I-62 modificada en el mismo commit", m_status("docs/automation/evidence/I-62-evidence.md", "M")),
    ("G2", "decisiones modificadas en el mismo commit", m_status(DEC, "M")),
    ("G2", "archivo de la ronda 1 modificado (I-62-A6/preflight.md)", m_status(EVID + "preflight.md", "M")),
    ("G2", "archivo nuevo de I-62-A6/ fuera de r2/", m_status(EVID + "nuevo.md", "A")),
    ("G2", "archivo de r2/ modificado en lugar de añadido", m_status(EVID_R2 + "preflight.md", "M")),
    ("G2", "archivo de r2/ borrado", m_status(EVID_R2 + "viejo.md", "D")),
    ("G2", "archivo de r3/ modificado (evidencia de la ronda 3)", m_status(EVID_R3 + "preflight.md", "M")),
    ("G2", "archivo nuevo en r3/ (fuera de r4/)", m_status(EVID_R3 + "nuevo.md", "A")),
    ("G2", "archivo de r4/ modificado en lugar de añadido", m_status(EVID_R4 + "preflight.md", "M")),
    ("G2", "archivo de r4/ borrado", m_status(EVID_R4 + "viejo.md", "D")),
    ("G2", "base con la ronda 1 en lugar de la ronda 3", m_set(["BaseA6"], R1_BLOB)),
    ("G2", "base con la ronda 2 en lugar de la ronda 3", m_set(["BaseA6"], R2_BLOB)),
    ("G2", "revisión de la ronda 3 tocada", m_status(R3_REVIEW_DIR + "/R20261013T183701Z-9f80/nota.md", "A")),
    ("G2", "revisión de la ronda 1 tocada", m_status(R1_REVIEW_DIR + "/R20261011T215735Z-c434/nota.md", "A")),
    ("G2", "privacidad en un archivo añadido", m_status(EVID_R4 + "nota.md", "A", "ruta " + SYNTH_WIN_PATH + "\n")),
    ("G2", "blob de A-6 en head distinto", m_set(["HeadObj", A6], "0" * 40)),
    ("G2", "base distinta del padre de head", m_set(["HeadParent"], "2" * 40)),
    # G3: superficies prohibidas, objetos congelados, ronda 1 intacta y C-20b
    ("G3", "V14 cambiada en head", m_set(["Objs", "Head", V14], "1" * 40)),
    ("G3", "blob del instrumento cambiado en head", m_set(["Objs", "Head", TOOL], "2" * 40)),
    ("G3", "árbol agent-execution cambiado", m_set(["Objs", "Head", AE], "3" * 40)),
    ("G3", "AUTOMATION_PLAN cambiado", m_set(["Objs", "Head", AP], "4" * 40)),
    ("G3", "dictamen cambiado en la base", m_set(["Objs", "Base", RULING], "5" * 40)),
    ("G3", "árbol src distinto entre base y head", m_set(["Objs", "Head", "src/"], "6" * 40)),
    ("G3", "árbol de la revisión de la ronda 1 cambiado en head", m_set(["Objs", "Head", R1_REVIEW_DIR], "7" * 40)),
    ("G3", "output.json de la revisión de la ronda 1 cambiado en la base", m_set(["Objs", "Base", R1_REVIEW], "8" * 40)),
    ("G3", "archivo de la ronda 1 de I-62-A6/ cambiado en head", lambda inp: inp["EvidFiles"]["Head"].__setitem__(EVID + "preflight.md", "9" * 40)),
    ("G3", "archivo añadido a I-62-A6/ fuera de r2/ en head", lambda inp: inp["EvidFiles"]["Head"].__setitem__(EVID + "otro.md", "a" * 40)),
    ("G3", "árbol de I-62-A6/ sin r2/ distinto en head", lambda inp: inp["EvidTop"]["Head"].append(("100644", "blob", "b" * 40, "intruso.md"))),
    ("G3", "superficie prohibida retirada de la lista de A-6", m_text("`docs/adr/`, `docs/automation/agent-execution/`, ", "`docs/automation/agent-execution/`, ")),
    ("G3", "revisión de la ronda 1 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1), ", "")),
    ("G3", "revisión de la ronda 2 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6r2/` (la revisión de la ronda 2), ", "")),
    ("G3", "revisión de la ronda 3 retirada de la lista de A-6", m_text("`docs/automation/evidence/I-62-architect-A-6r3/` (la revisión de la ronda 3 y su kit) y ", "y ")),
    ("G3", "cláusula «fuera de r4/» retirada de la lista", m_text(" y todos los archivos de `docs/automation/evidence/I-62-A6/` fuera de `r4/` (la evidencia de las rondas 1 a 3)", "")),
    ("G3", "cláusula «fuera de r3/» en lugar de «fuera de r4/»", m_text("`docs/automation/evidence/I-62-A6/` fuera de `r4/`", "`docs/automation/evidence/I-62-A6/` fuera de `r3/`")),
    ("G3", "árbol de la revisión de la ronda 3 cambiado en head", m_set(["Objs", "Head", R3_REVIEW_DIR], "7" * 40)),
    ("G3", "output.json de la revisión de la ronda 3 cambiado en la base", m_set(["Objs", "Base", R3_REVIEW], "8" * 40)),
    ("G3", "árbol de r3/ cambiado en head", m_set(["Objs", "Head", EVID_R3], "9" * 40)),
    ("G3", "archivo de r3/ cambiado en head", lambda inp: inp["EvidFiles"]["Head"].__setitem__(EVID_R3 + "preflight.md", "9" * 40)),
    ("G3", "archivo de r3/ en el commit", m_status(EVID_R3 + "a6r3-guards.py", "M")),
    ("G3", "árbol de I-62-A6/ sin r4/ con un r3/ distinto en head", lambda inp: inp["EvidTop"]["Head"].__setitem__(
        [k for k, e in enumerate(inp["EvidTop"]["Head"]) if e[3] == "r3"][0], ("040000", "tree", "c" * 40, "r3"))),
    ("G3", "árbol de la revisión de la ronda 2 cambiado en head", m_set(["Objs", "Head", R2_REVIEW_DIR], "7" * 40)),
    ("G3", "output.json de la revisión de la ronda 2 cambiado en la base", m_set(["Objs", "Base", R2_REVIEW], "8" * 40)),
    ("G3", "árbol de r2/ cambiado en head", m_set(["Objs", "Head", EVID_R2], "9" * 40)),
    ("G3", "archivo de r2/ cambiado en head", lambda inp: inp["EvidFiles"]["Head"].__setitem__(EVID_R2 + "preflight.md", "9" * 40)),
    ("G3", "ruta de superficie en el commit (WORKFLOW)", m_status(WF, "M")),
    ("G3", "archivo de la ronda 1 de I-62-A6/ en el commit", m_status(EVID + "a6-guards.py", "M")),
    ("G3", "C-20b distinto de EQUAL", m_set(["C20b", "Output"], '{"result": ["MV-3"]}')),
    ("G3", "base del mapa no ancestro", m_set(["C20b", "BaseIsAncestor"], False)),
    # G4: dictamen, revisión de la ronda 1, M-01..M-08 y compatibilidad
    ("G4", "un punto del dictamen retirado (punto 3)", m_re(r"^\| 3 \| Vía del blob[^\n]*\n", "")),
    ("G4", "texto de un punto alterado (punto 5)", m_text("efecto de los códigos de fallo cerrado de la herramienta y del REJECT_FACTS", "efecto de los códigos de la herramienta y del REJECT_FACTS")),
    ("G4", "punto 7 asignado a otro delta", m_re(r"(^\| 7 \| [^\n]*\| )A6-7 \|$", r"\1A6-1 |")),
    ("G4", "línea «Cubre» incoherente con los puntos (A6-6)", m_text("Cubre: punto 6 del dictamen.", "Cubre: punto 2 del dictamen.")),
    ("G4", "línea «Cubre» de A6-9 sin R3", m_text("decisiones §76, punto 3; R3 de la ronda 1.", "decisiones §76, punto 3.")),
    ("G4", "M-02 cambiado a no", m_text("| M-02 | **sí** | yes |", "| M-02 | no | yes |")),
    ("G4", "M-06 cambiado a sí", m_text("| M-06 | no | no |", "| M-06 | **sí** | no |")),
    ("G4", "M-01 con la columna Ronda 1 en AGREE", m_text("| M-01 | no | no | UNKNOWN (DISAGREE) |", "| M-01 | no | no | AGREE |")),
    ("G4", "M-01 sin la evaluación UNKNOWN del revisor", m_text("| M-01 | no | no | UNKNOWN (DISAGREE) |", "| M-01 | no | no | DISAGREE |")),
    ("G4", "M-04 con la columna Ronda 1 en DISAGREE", m_text("| M-04 | **sí** | yes | AGREE |", "| M-04 | **sí** | yes | DISAGREE |")),
    ("G4", "M-01 sin ningún delta de R3 (A6-9 ni A6-5)", m_seq(m_row_sub("## 5. ", "M-01", "A6-9", "A6-x"), m_row_sub("## 5. ", "M-01", "A6-5", "A6-y"))),
    ("G4", "fila M-07 retirada", m_re(r"^\| M-07 \|[^\n]*\n", "")),
    ("G4", "fila M-08 duplicada", m_re(r"(^\| M-08 \|[^\n]*\n)", r"\1\1")),
    ("G4", "columna Ronda 1 retirada de M-03", m_text("| M-03 | **sí** | yes | AGREE |", "| M-03 | **sí** | yes |")),
    ("G4", "compatibilidad sin A-3", m_re(r"^\| A-3 \| AGREED[^\n]*\n", "")),
    ("G4", "dictamen con una sola consecuencia OWNER-RESERVED", lambda inp: inp["Ruling"].__setitem__("OwnerReserved", inp["Ruling"]["OwnerReserved"][:1])),
    ("G4", "revisión de la ronda 1 sin O3", lambda inp: inp["Review"].__setitem__("OptionalFindings", inp["Review"]["OptionalFindings"][:2])),
    ("G4", "revisión de la ronda 1 con otro veredicto", m_set(["Review", "Verdict"], "AGREED")),
    ("G4", "§3.16 sin la fila de R1", m_re(r"^\| R1: [^\n]*\n", "")),
    ("G4", "§3.16 sin la fila de O2", m_re(r"^\| O2: [^\n]*\n", "")),
    ("G4", "§3.16 con R4 duplicado", m_re(r"(^\| R4: [^\n]*\n)", r"\1\1")),
    ("G4", "§3.16 con R2 y R3 en otro orden", m_re(r"(^\| R2: [^\n]*\n)(^\| R3: [^\n]*\n)", r"\2\1")),
    ("G4", "§3.16: corrección de R3 vacía", m_cell("### 3.16 ", "R3:", 1, "—")),
    ("G4", "§3.16: «Dónde» de R4 sin delta", m_cell("### 3.16 ", "R4:", 2, "§3.6")),
    ("G4", "§3.16: «Dónde» de R5 sin A6-8", m_cell("### 3.16 ", "R5:", 2, "A6-1, reglas 1 y 2; A6-7, regla 1")),
    ("G4", "§3.16: «Dónde» de O1 con un delta inexistente", m_cell("### 3.16 ", "O1:", 2, "A6-4, reglas 3 y 4; A6-10")),
    ("G4", "§3.16: evidencia r2/ ausente de r2/ en la base", lambda inp: inp.__setitem__("R2BaseFiles", {"a6r2-guards.py": "0" * 40})),
    ("G4", "§3.17: evidencia r3/ ausente de r3/ en la base", lambda inp: inp.__setitem__("R3BaseFiles", {"a6r3-guards.py": "0" * 40})),
    ("G4", "§3.18: evidencia r4/ ausente del paquete r4/", lambda inp: inp.__setitem__("R4Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),
    ("G4", "§3.18 sin la fila R1", m_re(r"(### 3\.18 .*?\n)\| R1: [^\n]*\n", r"\1")),
    ("G4", "§3.18 sin la fila O4", m_re(r"(### 3\.18 .*?\n)\| O4: [^\n]*\n", r"\1")),
    ("G4", "§3.18 sin la fila O7", m_re(r"^\| O7: [^\n]*\n", "")),
    ("G4", "§3.18 con O6 y O7 en otro orden", m_re(r"(^\| O6: [^\n]*\n)(^\| O7: [^\n]*\n)", r"\2\1")),
    ("G4", "§3.18 con la fila R1 duplicada", m_re(r"(### 3\.18 .*?\n)(\| R1: [^\n]*\n)", r"\1\2\2")),
    ("G4", "§3.18: corrección de R1 insuficiente", m_cell("### 3.18 ", "R1:", 1, "corregida")),
    ("G4", "§3.18: «Dónde» de R1 sin A6-3", m_cell("### 3.18 ", "R1:", 2, "§3.12")),
    ("G4", "§3.18: «Dónde» de R1 con una regla inexistente (A6-3, regla 9)", m_cell("### 3.18 ", "R1:", 2, "A6-3, regla 9 (b)")),
    ("G4", "§3.18: «Dónde» de O1 con un delta inexistente", m_cell("### 3.18 ", "O1:", 2, "A6-10")),
    ("G4", "§3.18: «Dónde» de O3 con A6-9 (sin «Cubre»)", m_cell("### 3.18 ", "O3:", 2, "A6-9, regla 4")),
    ("G4", "§3.18: «Dónde» de R1 también en A6-5", m_cell("### 3.18 ", "R1:", 2, "A6-3, regla 6 (b); A6-5, regla 2")),
    ("G4", "línea «Cubre» de A6-3 sin el segmento de la ronda 3", m_re(r"(^Cubre: punto 3 del dictamen;[^\n]*; R1 y R2 de la ronda 2); [^;\n]* de la ronda 3\.$", r"\1.")),
    ("G4", "línea «Cubre» de A6-5 con R1 de la ronda 3", m_text("R1, R2 y O5 de la ronda 2.", "R1, R2 y O5 de la ronda 2; R1 de la ronda 3.")),
    ("G4", "revisión de la ronda 3 sin O7", lambda inp: inp["Review3"].__setitem__("OptionalFindings", inp["Review3"]["OptionalFindings"][:6])),
    ("G4", "revisión de la ronda 3 con otro veredicto", m_set(["Review3", "Verdict"], "AGREED")),
    ("G4", "revisión de la ronda 3 sobre otro blob", m_set(["Review3", "ReviewedBlob"], R2_BLOB)),
    ("G4", "revisión de la ronda 3 con dos hallazgos exigidos", lambda inp: inp["Review3"]["RequiredFindings"].append({"FindingId": "A62-A6R3-R2"})),
    ("G4", "revisión de la ronda 3 con evaluación de la materialidad", m_set(["Review3", "MaterialityConfirmation"], [])),
    ("G4", "§5 sin «no evaluó la materialidad»", m_text("la revisión acotada de la ronda 3 no evaluó la materialidad", "la revisión acotada de la ronda 3 confirmó la materialidad")),
    ("G4", "§3.17: celda «Prueba» de R1 (b) reescrita, no ampliada", m_row_sub("### 3.17 ", "R1 (b):", "con una ejecución con error, rechazo (con el cierre", "con una ejecución con error, aceptación (con el cierre")),
    ("G4", "§3.17: celda «Prueba» de O3 ampliada (fuera de R1 (b)-(d))", m_cell_append("### 3.17 ", "O3:", 3, " (ampliada en la ronda 4)")),
    ("G4", "§3.17: celda «Corrección» de R1 (b) ampliada", m_cell_append("### 3.17 ", "R1 (b):", 1, " y algo más")),
    ("G4", "§3.17: texto fuera de las tablas cambiado", m_text("**Precondiciones de R2** (decisiones §83, punto 4)", "**Precondiciones de R2** (decisiones §83, puntos 4 y 5)")),
    ("G4", "§3.17 sin la fila R1 (b)", m_re(r"^\| R1 \(b\): [^\n]*\n", "")),
    ("G4", "§3.17 con O4 y O5 en otro orden", m_re(r"(^\| O4: [^\n]*\n)(^\| O5: [^\n]*\n)", r"\2\1")),
    ("G4", "§3.17: «Dónde» de O5 sin A6-5", m_cell("### 3.17 ", "O5:", 2, "§3.12")),
    ("G4", "§3.17: «Dónde» de R2 (a) con un delta inexistente", m_cell("### 3.17 ", "R2 (a):", 2, "A6-3, regla 6 (a); A6-10")),
    ("G4", "§3.17: «Dónde» de R1 (b) con una regla inexistente (A6-5, regla 9)", m_cell("### 3.17 ", "R1 (b):", 2, "A6-3, regla 6 (c); A6-5, regla 9")),
    ("G4", "línea «Cubre» de A6-7 sin R2 y O3 de la ronda 2", m_text("R5 y O3 de la ronda 1; R2 y O3 de la ronda 2.", "R5 y O3 de la ronda 1.")),
    ("G4", "línea «Cubre» de A6-3 con O5 de la ronda 2", m_text("R1, R4, O2 y O3 de la ronda 1; R1 y R2 de la ronda 2;", "R1, R4, O2 y O3 de la ronda 1; R1, R2 y O5 de la ronda 2;")),
    ("G4", "revisión de la ronda 2 sin O5", lambda inp: inp["Review2"].__setitem__("OptionalFindings", inp["Review2"]["OptionalFindings"][:4])),
    ("G4", "revisión de la ronda 2 con otro veredicto", m_set(["Review2", "Verdict"], "AGREED")),
    ("G4", "revisión de la ronda 2 sobre otro blob", m_set(["Review2", "ReviewedBlob"], "1" * 40)),
    # G6: líneas del Owner
    ("G6", "las dos líneas del Owner fundidas en una", m_text("la disposición del Coordinator que la cite)" + OWN_B, "la disposición del Coordinator que la cite); A6-CONTROLLER-MEDICION-CONSUMO = A (")),
    ("G6", "línea (b) retirada", m_re(r"\nA6-CONTROLLER-MEDICION-CONSUMO = A \([^\n]*\)", "")),
    ("G6", "línea (a) sin la exclusión de D.5", m_text("escenario, rol o topología, D.5 y la Owner Validation; no modifica", "escenario, rol o topología y la Owner Validation; no modifica")),
    ("G6", "línea (a) ampliada a la planificación", m_text("puede usar en su verificación solo el instrumento", "puede usar en su planificación y en su verificación solo el instrumento")),
    ("G6", "línea (a) con un espacio final", m_text("la disposición del Coordinator que la cite)\nA6-CONTROLLER", "la disposición del Coordinator que la cite) \nA6-CONTROLLER")),
    ("G6", "línea (b) reutiliza A2-P2", m_text("fuera de A2-P2, sin reutilizar su bloque agotado", "dentro de A2-P2, reutilizando su bloque agotado")),
    ("G6", "blob del instrumento cambiado en la línea (b)", m_text("con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed y el intérprete", "con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aee y el intérprete")),
    ("G6", "línea (a) autoriza una medición", m_text("no autoriza ninguna invocación ni consumo ni acepta", "autoriza 1 invocación de medición ni acepta")),
    ("G6", "línea (b) con el nombre de la (a)", m_text("\nA6-CONTROLLER-MEDICION-CONSUMO = A (1 invocación", "\nA6-ADR0046-9-FX02 = A (1 invocación")),
    ("G6", "trío cambiado en la línea (b) (AppVersion)", m_text("/26.1007.2314.0/F909F480", "/26.1007.2315.0/F909F480")),
    ("G6", "tercera línea con forma de línea del Owner en §7", m_text(OWN_B, "\nA6-PLAN-EXT = A (ampliación a PLAN)" + OWN_B)),
    ("G6", "línea del Owner fuera de §7", m_insert_ev("```text\nA6-OTRA = A (algo)\n```")),
    ("G6", "§7 sin «separadas y distintas»", m_text("**separadas y distintas**", "**distintas**")),
    ("G6", "decisiones sin la línea (a) en §78", lambda inp: inp["Sources"].__setitem__("DEC", inp["Sources"]["DEC"].replace("\n" + owner_lines(inp["R1Text"])["A6-ADR0046-9-FX02"] + "\n", "\n"))),
    ("G6", "decisiones con la línea (a) solo fuera de §78", lambda inp: inp["Sources"].__setitem__("DEC", inp["Sources"]["DEC"].replace("\n" + owner_lines(inp["R1Text"])["A6-ADR0046-9-FX02"] + "\n", "\n", 1) + "\n" + owner_lines(inp["R1Text"])["A6-ADR0046-9-FX02"] + "\n")),
    ("G6", "decisiones sin la sección 78", lambda inp: inp["Sources"].__setitem__("DEC", inp["Sources"]["DEC"].replace("\n## 78. ", "\n## 78bis. "))),
    # G7: identidad de los instrumentos
    ("G7", "blob del instrumento cambiado en §3.3", m_text("| `9a829b0eeb1210238052b1feac06cfa8f7964aed` | `b03c3a19", "| `9a829b0eeb1210238052b1feac06cfa8f7964aef` | `b03c3a19")),
    ("G7", "SHA-256 del aceptante cambiado en §3.3", m_text("`173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0` |", "`173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb1` |")),
    ("G7", "fila de la especificación retirada de §3.3", m_re(r"^\| Especificación \(no normativa\)[^\n]*\n", "")),
    ("G7", "e2e-summary con otro blob de herramienta", lambda inp: inp["E2E"].__setitem__("ToolBlob", "7" * 40)),
    ("G7", "aceptante leído con contenido que no da su blob", m_set(["AccBlobOk"], False)),
    ("G7", "blob del aceptante distinto en el commit del paquete", m_set(["PkgCommitBlobs", ACC], "c" * 40)),
    # G8: estado y declaraciones
    ("G8", "A-6 declarada AGREED en §11", m_text("## 11. Estado de revisión\n", "## 11. Estado de revisión\n\nA-6 = AGREED.\n")),
    ("G8", "estado AGREED", m_text("State:\nPROPUESTA (ronda 4). No aplicada", "State:\nAGREED (ronda 4). No aplicada")),
    ("G8", "estado PROPUESTA sin «ronda 4»", m_text("State:\nPROPUESTA (ronda 4). No aplicada", "State:\nPROPUESTA. No aplicada")),
    ("G8", "estado PROPUESTA (ronda 3)", m_text("State:\nPROPUESTA (ronda 4). No aplicada", "State:\nPROPUESTA (ronda 3). No aplicada")),
    ("G8", "ronda 4 lanzada en la cabecera", m_text("Ronda 4: NOT LAUNCHED.", "Ronda 4: LAUNCHED.")),
    ("G8", "§11: Architect con la ronda 4 AGREED", m_text("ronda 4: NOT LAUNCHED, hasta una disposición posterior", "ronda 4: AGREED, hasta una disposición posterior")),
    ("G8", "§11: Coordinator con veredicto", m_text("sin veredicto sobre la ronda 4", "veredicto AGREED sobre la ronda 4")),
    ("G8", "§11: Coordinator usa el dictamen de la ronda 2 como acuerdo", m_text("sin usar el dictamen como acuerdo; ronda 3:", "usando el dictamen como acuerdo; ronda 3:")),
    ("G8", "ronda 2 declarada AGREED", m_insert_ev("Ronda 2: AGREED.")),
    ("G8", "ronda 3 declarada AGREED", m_insert_ev("Ronda 3: AGREED.")),
    ("G8", "ronda 4 declarada PASS", m_insert_ev("Ronda 4 = PASS.")),
    ("G8", "§11: Owner con (b) pedida", m_text("§7 (b) solo para la aplicación", "§7 (b) pedida ahora")),
    ("G8", "Coordinator = AGREED", m_text("| Coordinator | ronda 1:", "| Coordinator = AGREED | ronda 1:")),
    ("G8", "C-24 PASS declarado", m_insert_ev("C-24 = PASS.")),
    ("G8", "FX-02 PASS declarado", m_insert_ev("FX-02 = PASS.")),
    ("G8", "F6 GATE PASS declarado", m_insert_ev("F6 GATE PASS = alcanzado.")),
    ("G8", "excepción declarada vigente", m_insert_ev("La excepción queda vigente.")),
    ("G8", "A-6 declarada aplicada", m_insert_ev("A-6 queda aplicada.")),
    ("G8", "revisión de la ronda 4 declarada lanzada", m_insert_ev("La revisión formal de la ronda 4 lanzada el 2026-10-14.")),
    ("G8", "(b) pedida en la cabecera", m_text("(b) A6-CONTROLLER-MEDICION-CONSUMO: no pedida;", "(b) A6-CONTROLLER-MEDICION-CONSUMO: pedida;")),
    ("G8", "se pide una línea del Owner", m_insert_ev("Se pide al Owner la línea A6-CONTROLLER-MEDICION-CONSUMO.")),
    ("G8", "una línea nueva del Owner", m_insert_ev("La ronda 2 necesita una línea nueva del Owner.")),
    ("G8", "marcador pendiente en run", lambda inp: (inp.__setitem__("Final", True), m_insert_ev("PENDIENTE PRUEBA_MARCADOR")(inp))),
    ("G8", "marcador de render sin sustituir en run", lambda inp: (inp.__setitem__("Final", True), m_insert_ev("@@KEEP 3.99@@")(inp))),
    # G5: pureza del delta
    ("G5", "ubicación de A6-3 en D.5", m_text("Ubicación: al final de V14 D.6, a continuación", "Ubicación: al final de V14 D.5, a continuación")),
    ("G5", "orden de D.3 alterado (A6-7 tras A6-2)", m_text("Ubicación: a continuación del párrafo de A6-4 (al final de V14 D.3)", "Ubicación: a continuación del párrafo de A6-2 (al final de V14 D.3)")),
    ("G5", "tope de Codex distinto en A6-1", m_in_delta("A6-1", "tope **15**), las demás filas", "tope **16**), las demás filas")),
    ("G5", "fila de tabla en el delta", m_in_delta("A6-7", "con evidencia que todavía no existe.»", "con evidencia que todavía no existe.\n> | fila | nueva |»")),
    ("G5", "A6-3, regla 2, alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "el árbol, el índice, las refs y el estado canónico del fixture no cambian", "el árbol y el índice no cambian")),
    ("G5", "A6-9 alterado (debe ser idéntico a la ronda 3)", m_in_delta("A6-9", "su resultado queda del lado del Coordinator", "su resultado queda del lado de la sesión")),
    ("G5", "sección de A6-1 alterada fuera del párrafo (Cubre)", m_text("Cubre: punto 1 del dictamen; R1, R2 y R5 de la ronda 1.", "Cubre: punto 1 del dictamen; R1, R2 y R5 de la ronda 1; R1 de la ronda 2.")),
    ("G5", "A6-5 sin la regla 4 de la ronda 2 (renumeración)", m_text("> 4. **salida no aceptada:**", ">    **salida no aceptada:**")),
    ("G5", "A6-5 vuelve a la tabla de la ronda 2 (R4 siempre en MATCH)", m_in_delta("A6-5", "R1, R2, R3, R9 y R10 están en `MATCH`", "R1, R2, R3, R4, R9 y R10 están en `MATCH`")),
    ("G5", "regla 6 (b) sin el apartado (i)", m_drop_r6b("i")),
    ("G5", "regla 6 (b) sin el apartado (ii)", m_drop_r6b("ii")),
    ("G5", "regla 6 (b) sin el apartado (iii)", m_drop_r6b("iii")),
    ("G5", "regla 6 (b) sin el apartado (iv)", m_drop_r6b("iv")),
    ("G5", "regla 6 (b) sin el apartado (v)", m_drop_r6b("v")),
    ("G5", "regla 6 (b) sin la frase final (prueba previa y medición)", m_drop_r6b("cierre")),
    ("G5", "regla 6 (b): «no se sustituye» → «se sustituye»", m_in_delta("A6-3", "un término omitido no se sustituye por ningún otro valor", "un término omitido se sustituye por otro valor")),
    ("G5", "regla 6 (b) sin «sin claves repetidas»", m_in_delta("A6-3", "en UTF-8 sin claves repetidas", "en UTF-8")),
    ("G5", "regla 6 (b): «minúscula» → «mayúscula»", m_in_delta("A6-3", "hexadecimales en minúscula", "hexadecimales en mayúscula")),
    ("G5", "regla 6 (b): «Se omite entonces» → «Se conserva entonces»", m_in_delta("A6-3", "Se omite entonces todo término", "Se conserva entonces todo término")),
    ("G5", "regla 6 (b) (ii): «se omite» → «se conserva»", m_in_delta("A6-3", "H.CurrentSha), se omite;", "H.CurrentSha), se conserva;")),
    ("G5", "regla 6 (b) de la ronda 3 reintroducida", m_r6b_prev()),
    ("G5", "regla 6 (b): `Closure` antes de `RelayFacts` (orden de la ronda 3)", m_in_delta("A6-3", "`RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256 citado; `Closure` = el cierre derivado por la regla de A-6 sobre la independencia,", "`Closure` = el cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256 citado,")),
    ("G5", "regla 6 (b) con un apartado (vi) añadido", m_r6b_insert("cierre", ">    (vi) un término omitido se repone con la entrega de otra invocación;")),
    ("G5", "regla 6 (b) con una frase exigida repetida", m_r6b_insert("iv", ">    y un extremo omitido da el conjunto vacío;")),
    ("G5", "regla 6 (b) con (iii) y (iv) en otro orden", m_r6b(lambda inp, ls, parts: (lambda d: ls[:d["iii"][0]] + ls[d["iv"][0]:d["iv"][1]] + ls[d["iii"][0]:d["iii"][1]] + ls[d["iv"][1]:])({k: (a, b) for k, a, b in parts}))),
    ("G5", "regla 6 (a) alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "y su código de salida consta", "y su código de salida queda registrado")),
    ("G5", "regla 6 (c) alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "la ruta ignorada del paquete", "la ruta ignorada del paquete y su árbol")),
    ("G5", "regla 6 (c) con un espacio doble (byte, mismo texto normalizado)", m_in_delta("A6-3", "existen y son legibles y ninguna", "existen y son legibles  y ninguna")),
    ("G5", "regla 6 (d) alterada (debe ser idéntica a la ronda 3)", m_in_delta("A6-3", "(d) `NotApplicable`, solo por la tabla", "(d) `NotApplicable`, solo y siempre por la tabla")),
    ("G5", "párrafo final de la regla 6 alterado", m_in_delta("A6-3", "y la retención termina con ese registro", "y la retención termina después")),
    ("G5", "cabecera de la regla 6 alterada", m_in_delta("A6-3", "y la custodia con el resultado:", "y la custodia con el resultado y el registro:")),
    ("G5", "A6-3, regla 7, con un espacio doble (byte)", m_in_delta("A6-3", "ni sobre el directorio de trabajo durante una", "ni sobre el directorio de trabajo  durante una")),
    ("G5", "A6-3: «Ubicación» alterada fuera del párrafo", m_text("antes de `### D.7`. Ninguna línea de D.6 cambia de texto.", "antes de `### D.7`. Ninguna línea de D.6 cambia.")),
    ("G5", "línea «Cubre» de A6-3 cambiada en la ronda 1", m_text("Cubre: punto 3 del dictamen; R1, R4, O2 y O3 de la ronda 1;", "Cubre: punto 3 del dictamen; R1, R4 y O2 de la ronda 1;")),
    ("G5", "línea «Cubre» de A6-3 con el segmento de la ronda 3 sin «de la ronda 3»", m_re(r"(^Cubre: punto 3 del dictamen;[^\n]*; R1 y R2 de la ronda 2; [^;\n]*?) de la ronda 3\.$", r"\1.")),
    ("G5", "A6-5 alterado en un byte (espacio doble)", m_in_delta("A6-5", "solo queda aceptada con R4 en `MATCH`", "solo queda aceptada con R4 en  `MATCH`")),
    ("G5", "A6-7 alterado (debe ser idéntico a la ronda 3)", m_in_delta("A6-7", "Sin esa disposición no hay medición", "Sin esa disposición no hay medición alguna")),
    ("G5", "sección de A6-5 alterada fuera del párrafo (Ubicación)", m_text("antes de `### D.5`.\nNinguna línea de D.4 cambia de texto.", "antes de `### D.5`.\nNinguna línea de D.4 cambia.")),
    ("G5", "sección de A6-7 alterada fuera del párrafo (Cubre)", m_text("Cubre: punto 7 del dictamen; R5 y O3 de la ronda 1; R2 y O3 de la ronda 2.", "Cubre: punto 7 del dictamen; R5 y O3 de la ronda 1; R2 de la ronda 2.")),
    ("G5", "tabla de derivación de A6-9 alterada (§3.12)", m_text("U {MergeBase(D.MainSha, K.AuthorityRevision)}", "U {MergeBase(D.MainSha, H.CurrentSha)}")),
    ("G5", "sección de A6-9 alterada fuera del párrafo", m_text("(A6-9, regla 4; mecánica, sin juicio)", "(A6-9, regla 4; mecánica)")),
    ("G5", "nombre de producto en A6-7", m_in_delta("A6-7", "no amplía permisos, alcance de escritura", "no amplía permisos de codex-cli, alcance de escritura")),
    ("G5", "nombre de producto en A6-3 (python)", m_in_delta("A6-3", "ni sobre el directorio de trabajo durante una", "ni sobre el directorio de trabajo de python durante una")),
    ("G5", "A6-2 alterado en un carácter (no idéntico a la ronda 1)", m_in_delta("A6-2", "no aporta obligaciones de lectura", "no aporta obligaciones  de lectura")),
    ("G5", "A6-6 alterado (no idéntico a la ronda 1)", m_in_delta("A6-6", "se registra con esa calificación", "se registra con su calificación")),
    ("G5", "sección de A6-2 alterada fuera del párrafo", m_text("Cubre: punto 2 del dictamen.", "Cubre: punto 2 del dictamen (sin cambio).")),
    ("G5", "A6-4 sin cláusula VERIFY", m_in_delta("A6-4", "Solo para cada invocación VERIFY del Controller", "Solo para cada invocación del Controller")),
    ("G5", "A6-1 permite cambiar la receta", m_in_delta("A6-1", "ningún elemento de la receta del adapter cambia", "la receta cambia si hace falta")),
    ("G5", "A6-1 amplía presupuestos congelados", m_in_delta("A6-1", "Los presupuestos congelados no cambian", "Los presupuestos congelados se amplían")),
    ("G5", "A6-7 con tope dos", m_in_delta("A6-7", "Su tope es uno en total para FX-02 en F6", "Su tope es dos en total para FX-02 en F6")),
    ("G5", "A6-7 reutiliza el bloque agotado", m_in_delta("A6-7", "No usa ni repone un bloque agotado ni una medición consumida.", "Puede usar el bloque agotado.")),
    ("G5", "A6-3 lleva el instrumento a F_seed", m_in_delta("A6-3", "el instrumento no entra en F_seed, en F_norm, en el manifiesto", "el instrumento entra en F_seed, no en F_norm, ni en el manifiesto")),
    ("G5", "A6-3 vuelve al clon del fixture (R4)", m_in_delta("A6-3", "sobre el mismo repositorio y la misma ruta del `-C` en que corrió el instrumento", "sobre un clon del fixture en los mismos SHA")),
    ("G5", "A6-3 sin --no-replace-objects en la comprobación", m_in_delta("A6-3", "`git --no-replace-objects cat-file -t <blob>` = `blob`", "`git cat-file -t <blob>` = `blob`")),
    ("G5", "A6-3 con la escritura dentro de la ventana", m_in_delta("A6-3", "antes de que se abra la ventana de la unidad", "cuando se abra la ventana de la unidad")),
    ("G5", "A6-4 vuelve a «comparado por blob»", m_in_delta("A6-4", "nunca la comparación del nombre del objeto", "comparado por blob")),
    ("G5", "A6-5 vuelve a «FAIL si el rechazo»", m_in_delta("A6-5", "una salida no aceptada nunca es por sí misma un FAIL de la entrega del Worker ni de FX-02", "es FAIL si el rechazo muestra una violación")),
    ("G5", "A6-6 calcula el oráculo con el instrumento", m_in_delta("A6-6", "se calcula sin el instrumento, sin el aceptante", "se calcula con el instrumento, sin el aceptante")),
    ("G5", "A6-9 aceptante en la sesión de Principal", m_in_delta("A6-9", "el aceptante se ejecuta fuera de la sesión de Principal del fixture", "el aceptante se ejecuta dentro de la sesión de Principal del fixture")),
    ("G5", "A6-9 vuelve a la autoría declarada (R3)", m_in_delta("A6-9", "el aceptante no se usa tal como está (regla", "el aceptante, una implementación de autor distinto, sigue (regla")),
    ("G5", "A6-8 concede la excepción", m_in_delta("A6-8", "A-6 no concede esa determinación", "A-6 concede esa determinación")),
    ("G5", "A6-8 sin la exclusión de D.5", m_in_delta("A6-8", "rol o topología, D.5 y la Owner Validation", "rol o topología y la Owner Validation")),
    ("G5", "§3.13 sin la prohibición de la receta", m_text("- no cambia la receta 16.4 (AP L541-L543);", "- (sin efecto sobre la receta 16.4) (AP L541-L543);")),
    ("G5", "§3.13 sin «No cambia la planificación»", m_text("- No cambia la planificación (PLAN) del Controller:", "- La planificación (PLAN) del Controller:")),
    ("G5", "ancla de V14 desplazada", lambda inp: inp["Sources"].__setitem__("V14", inp["Sources"]["V14"].replace(ANCHOR_D6 + "\n", ANCHOR_D6 + "\nlínea añadida\n", 1))),
    # G9: alcance de R5
    ("G9", "A6-1 admite la planificación", m_in_delta("A6-1", "en sus invocaciones VERIFY de esta topología (la verificación", "en sus invocaciones de planificación y VERIFY de esta topología (la verificación")),
    ("G9", "A6-1, regla 2, admite PLAN", m_in_delta("A6-1", "la planificación (PLAN) no usa el instrumento", "la planificación (PLAN) usa el instrumento")),
    ("G9", "frase nueva que admite el aceptante en PLAN (A6-9)", m_in_delta("A6-9", "6. el oráculo de C-24 nunca se entrega al Controller.»", "6. el oráculo de C-24 nunca se entrega al Controller; el aceptante también juzga la planificación.»")),
    ("G9", "A6-7 elegible también para PLAN", m_in_delta("A6-7", "y solo para las invocaciones VERIFY de esta topología", "y para las invocaciones PLAN y VERIFY de esta topología")),
    ("G9", "A6-8 sin la exclusión de PLAN", m_in_delta("A6-8", "Quedan excluidos de forma expresa la planificación (PLAN), la", "Quedan excluidos de forma expresa la")),
    ("G9", "fuente AP L1237 sin la verificación de 16.9", m_src_line("AP", 1237, "la verificación de 16.9", "la verificación")),
    ("G9", "fuente RAE L220 sin CONTROLLER_VERIFICATION", m_src_line("RAE", 220, "`CONTROLLER_VERIFICATION`", "`CONTROLLER_PLAN`")),
    ("G9", "kit L23: negativos como PLAN", m_src_line("KIT", 23, "| VERIFY (mismo prompt", "| PLAN (mismo prompt")),
    ("G9", "§2 sin el bloque de AP L1237", m_text("AP L1237-L1237 y L1240-L1241, literal", "AP L1236-L1236 y L1240-L1241, literal")),
    ("G9", "petición de ampliar la determinación", m_insert_ev("La ronda 2 pide ampliar la determinación del Owner a PLAN.")),
    ("G9", "§7 sin «no se pide ninguna ampliación»", m_text("y no se pide\n  ninguna ampliación", "y se pide\n  la ampliación")),
    # G10: resultado del aceptante
    ("G10", "tabla: R4 en NOT_APPLICABLE sin la condición de `Verification`", m_in_delta("A6-5", "R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera; y R5, R6 y R7", "y R4, R5, R6 y R7")),
    ("G10", "condición de R4 con Authority en lugar de FreeText", m_in_delta("A6-3", "`Trailer` y `FreeText` está en `not_run`", "`Trailer` y `Authority` está en `not_run`")),
    ("G10", "condición de R4 sin los tres archivos", m_in_delta("A6-3", "solo si esos tres archivos existen y son legibles y ninguna", "solo si ninguna")),
    ("G10", "aceptante: R4_CHECKS sin FreeText", m_acc('R4_CHECKS = ("Identity", "Scope", "CleanTree", "Trailer", "FreeText", "Authority")', 'R4_CHECKS = ("Identity", "Scope", "CleanTree", "Trailer", "Authority")')),
    ("G10", "aceptante: R4 sin not_run como distinto de pass", m_acc('r if r in ("pass", "fail", "not_run") else None', 'r if r in ("pass", "fail") else None')),
    ("G10", "aceptante: R3 con datos sin ejecuciones", m_acc("def check_r3(ctx):\n    if not ctx.runs:\n        return None", "def check_r3(ctx):\n    if not ctx.runs:\n        return (\"MATCH\", {})")),
    ("G10", "aceptante: R4 lee otra entrada además de la entrega", m_acc('read_pinned(v.get("Handoff"), "Verification.Handoff")', 'read_pinned(v.get("Worker"), "Verification.Handoff")')),
    ("G10", "A6-5, regla 7, sin «Ninguna está obligada»", m_in_delta("A6-5", "Ninguna está obligada a ejecutar el instrumento", "Toda invocación ejecuta el instrumento")),
    ("G10", "tabla: R1 en NOT_APPLICABLE (no WAIVABLE)", m_in_delta("A6-5", "y R5, R6 y R7 están en `MATCH` o en `NOT_APPLICABLE`", "y R1, R5, R6 y R7 están en `MATCH` o en `NOT_APPLICABLE`")),
    ("G10", "tabla: R8 en MATCH en lugar de MANUAL", m_in_delta("A6-5", "R8 está en `MANUAL`", "R2 está en `MANUAL`")),
    ("G10", "tabla: R9 en dos grupos", m_in_delta("A6-5", "y R5, R6 y R7 están en `MATCH` o en `NOT_APPLICABLE`", "y R5, R6, R7 y R9 están en `MATCH` o en `NOT_APPLICABLE`")),
    ("G10", "Checks R1-R9", m_in_delta("A6-5", "objeto con `Checks` R1-R10", "objeto con `Checks` R1-R9")),
    ("G10", "estado inventado en los Checks", m_in_delta("A6-5", "`NOT_APPLICABLE` o\n>    `MANUAL`, y una `Decision`", "`NOT_APPLICABLE` o\n>    `WAIVED`, y una `Decision`")),
    ("G10", "código de salida de error 2", m_in_delta("A6-5", "es 0, 1 o 64 (error de uso)", "es 0, 1 o 2 (error de uso)")),
    ("G10", "aceptada con código 1", m_in_delta("A6-5", "solo si el código es 0, la", "solo si el código es 1, la")),
    ("G10", "R5 sin hashes", m_in_delta("A6-5", "(R5: una orden `identity`, `diff`, `scope` o `hashes` en `Runs`;", "(R5: una orden `identity`, `diff` o `scope` en `Runs`;")),
    ("G10", "R7 con identity", m_in_delta("A6-5", "R7: una orden `scope`, `contract` o `clausemap` en `Runs`)", "R7: una orden `scope`, `contract`, `identity` o `clausemap` en `Runs`)")),
    ("G10", "sin «La Decision sola no basta»", m_in_delta("A6-5", "La `Decision` sola no basta", "La `Decision` basta")),
    ("G10", "INVALID_OUTPUT retirado de A6-5", m_in_delta("A6-5", "INVALID_OUTPUT de 16.9", "resultado inválido de 16.9")),
    ("G10", "estado no citado en §2 (P-31)", m_in_delta("A6-5", "(P-19: no avanza)", "(P-31: no avanza)")),
    ("G10", "recuperación: tres reejecuciones", m_in_delta("A6-5", "como máximo dos por (`TaskId`, fase) y dos por control negativo", "como máximo tres por (`TaskId`, fase) y dos por control negativo")),
    ("G10", "UNVERIFIED sin «registro incompleto»", m_in_delta("A6-5", "FX-02 queda UNVERIFIED con esa causa (registro incompleto)", "FX-02 queda UNVERIFIED con esa causa")),
    ("G10", "S-03 cambiado por S-05", m_in_delta("A6-5", "ambigüedad material (S-03)", "ambigüedad material (S-05)")),
    ("G10", "A6-3, regla 6: sin `Verification`", m_in_delta("A6-3", "`Verification` = la salida", "la verificación = la salida")),
    ("G10", "A6-3, regla 6: NotApplicable libre", m_in_delta("A6-3", "`NotApplicable`, solo por la tabla de la regla de A-6", "`NotApplicable`, cuando la supervisión lo decida y no por la tabla de la regla de A-6")),
    ("G10", "aceptante: R6 sin AuthorityResolution (fuente extra retirada)", m_acc('ar = dig(ctx.cv, "AuthorityResolution")', 'ar = dig(ctx.cv, "OtraFuente")')),
    ("G10", "A6-3, regla 6: sin el cierre derivado", m_in_delta("A6-3", "`Closure` = el cierre derivado por la regla de A-6 sobre la independencia", "`Closure` = la regla de A-6 sobre la independencia")),
    # G11: fuentes y rangos de líneas para el revisor siguiente
    ("G11", "referencia a una línea fuera de V14 (L99999)", m_insert_ev("Ver V14 L99999.")),
    ("G11", "referencia a una línea fuera de decisiones (DEC L99999)", m_insert_ev("Ver DEC L99999.")),
    ("G11", "referencia a una línea fuera de la ronda 1 (A-6 L9999)", m_insert_ev("Ver A-6 L9999.")),
    ("G11", "rango invertido (AP L20-L10)", m_insert_ev("Ver AP L20-L10.")),
    ("G11", "sección de decisiones inexistente (§99)", m_insert_ev("Ver decisiones §99.")),
    ("G11", "sección de evidencia inexistente (§999)", m_insert_ev("Ver evidencia §999.")),
    ("G11", "ruta:línea fuera del archivo (bmin_accept.py:99999)", m_insert_ev("Ver `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py:99999`.")),
    ("G11", "ruta:línea sin archivo único (README.md:3)", m_insert_ev("Ver README.md:3.")),
    ("G11", "ruta:línea de un archivo inexistente", m_insert_ev("Ver `docs/automation/evidence/I-62-A6/no-existe.md:3`.")),
    ("G11", "sin V14 L2333-L2334 en las fuentes del revisor", lambda inp: inp.__setitem__("A6", inp["A6"].replace("V14 L2333-L2334", "V14 L2335-L2336"))),
    ("G11", "ruta citada que no existe en la base", m_insert_ev("Ver `docs/automation/evidence/I-62-F6/no-existe.md`.")),
    ("G11", "ruta del paquete r4/ ausente del paquete disponible", lambda inp: inp.__setitem__("R4Local", {"otro.json": {"Blob": "0" * 40, "Sha256": "0" * 64}})),
    ("G11", "sin la fuente RAE L219-L220 de la ronda 4", m_text("(docs/automation/agent-execution/README.md:219-220)", "(docs/automation/agent-execution/README.md)")),
    ("G11", "sin la línea 23 del kit (fuente de la ronda 4)", m_text("controller-contracts.md:23)", "controller-contracts.md)")),
    ("G11", "sin la salida de la revisión de la ronda 3", m_text("R20261013T183701Z-9f80/output.json`, ACCREDITED)", "R20261013T183701Z-9f80/`, ACCREDITED)")),
    ("G11", "líneas de las guardas fuera del kit", lambda inp: inp["Sources"].__setitem__("KIT", "\n".join(inp["Sources"]["KIT"].split("\n")[:20]))),
    ("G10", "aceptante: WAIVABLE con R1", m_acc('WAIVABLE = frozenset(["R4", "R5", "R6", "R7", "R10"])', 'WAIVABLE = frozenset(["R4", "R6", "R7", "R10"])')),
    ("G10", "aceptante: R5 compara otras órdenes", m_acc('R5_COMMANDS = frozenset(["identity", "diff", "scope", "hashes"])', 'R5_COMMANDS = frozenset(["identity", "diff", "scope"])')),
    ("G10", "aceptante: Decision sin MANUAL", m_acc('c["Status"] in ("MATCH", "NOT_APPLICABLE", "MANUAL") for c in checks', 'c["Status"] in ("MATCH", "NOT_APPLICABLE") for c in checks')),
    ("G10", "aceptante: código de error 2", m_acc("        rc = 64\n", "        rc = 2\n")),
    ("G10", "ACCEPT-NOTES sin los códigos de salida", m_set(["AccNotes"], "")),
    ("G10", "fuente AP L687 sin S-04", m_src_line("AP", 687, "| S-04 |", "| S-14 |")),
]


def m_pos(pos, ins):
    def f(inp):
        inp["A6"] = inp["A6"][:pos] + ins + inp["A6"][pos:]
    return f


def auto_mutants(md=""):
    """Mutantes que desactivan por turno cada sitio comprobado: bloques literales y citas fijadas (G1), frases exigidas y párrafos (G5),
    sitios de G9, correspondencias de G10, hechos de la cabecera, filas de §3.16 y motivos de M-01..M-08 (G4)."""
    out = []
    blocks, _ = literal_blocks(md)
    for tag, a, b, lines, (st, en) in blocks:
        eol = md.find("\n", st)
        out.append(("G1", "bloque literal %s L%d-L%d alterado en su primera línea" % (tag, a, b), m_pos(eol if 0 <= eol < en else en, " ·")))
    masked = mask_fences(md)
    spans = [x[4] for x in blocks] + [v["BlockSpan"] for v in a6_rules(md).values()]
    lit_texts = {norm(row[3]) for row in LIT}
    for s, e, depth in quotes(masked) or []:
        if depth or any(a <= s < b for a, b in spans):
            continue
        inner = md[s + 1:e]
        if norm(inner) in lit_texts and len(inner) > 2:
            out.append(("G1", "cita literal fijada alterada en un carácter: «%s»" % inner[:40], m_pos(s + 2, "x")))
    for rid in DELTAS:
        out.append(("G5", "%s sin su párrafo citado (regla desactivada)" % rid, m_drop_delta(rid)))
        for k, ph in enumerate(REQUIRED[rid]):
            out.append(("G5", "%s: frase exigida %d desactivada («%s»)" % (rid, k + 1, ph[:40]), m_in_delta(rid, ph, "[retirada]")))
    for d, k, ph in G9_SITES:
        out.append(("G9", "sitio de G9 desactivado: %s, %s («%s»)" % (d, "cabecera" if k == 0 else "regla %d" % k, ph[:40]), m_in_delta(d, ph, "[retirada]")))
    for tag, line, phs in G9_SOURCES:
        out.append(("G9", "fuente de G9 desactivada: %s L%d" % (tag, line), m_src_line(tag, line, phs[0], "[retirada]")))
    for k, ph, tag, line, sph in CORRESPONDENCE:
        out.append(("G10", "correspondencia desactivada en A6-5, regla %d («%s»)" % (k, ph[:40]), m_in_delta("A6-5", ph, "[retirada]")))
    for fact in HEADER_EXACT:
        first = fact.split("\n")[-1]
        out.append(("G1", "hecho de la cabecera desactivado: «%s»" % first[:40], m_text(fact, fact.replace(first, "[retirado]"))))
    for fid in R1_FINDINGS:
        out.append(("G4", "§3.16: «Dónde» de %s vaciado" % fid, m_cell("### 3.16 ", fid + ":", 2, "—")))
    for i in range(1, 9):
        mid = "M-0%d" % i
        out.append(("G4", "%s: motivo vaciado" % mid, m_cell("## 5. ", mid, 5, "")))
        out.append(("G4", "%s: columna Ronda 2 alterada" % mid, m_cell("## 5. ", mid, 4, "UNKNOWN (DISAGREE)")))
    for rid in R2_ROWS:
        out.append(("G4", "§3.17: corrección de %s vaciada" % rid, m_cell("### 3.17 ", rid + ":", 1, "—")))
    for rid in R3_ROWS:
        out.append(("G4", "§3.18: corrección de %s vaciada" % rid, m_cell("### 3.18 ", rid + ":", 1, "—")))
    for k, ph in R6B_REQUIRED:
        out.append(("G5", "regla 6 (b) (%s): frase exigida desactivada («%s»)" % (k, ph[:40]), m_in_delta("A6-3", ph, "[retirada]")))
    for h in SAME_SECTIONS:
        s, e = section(md, h)
        if s is not None:
            out.append(("G5", "%s alterada (sección sin cambio en la ronda 4)" % h.strip(), m_pos(md.find("\n", s), " ·")))
    return out


def run_guard(gid, inp, cm, ids):
    try:
        return GUARDS_FN[gid](inp, cm, ids)
    except Exception as e:  # una excepción en una guarda cuenta como FAIL con hallazgo
        return {"Check": gid, "Findings": ["excepción: %s: %s" % (type(e).__name__, sanitize(str(e)))], "Result": "FAIL"}


def self_test_base(inp):
    base = copy.deepcopy(inp)
    base["Status"] = {A6: "M", GUARDS: "A", SELFTEST: "A", EVID_R4 + "preflight.md": "A"}
    base["HeadTexts"] = {A6: base["A6"], GUARDS: "# guardas\n", SELFTEST: "{}\n", EVID_R4 + "preflight.md": "# preflight\n"}
    base["HeadParent"] = base["Base"]
    base["HeadObj"] = {A6: text_blob(base["A6"])}
    base["Objs"] = {"Base": dict(base["Objs"]["Base"]), "Head": dict(base["Objs"]["Base"])}
    base["EvidTop"] = {"Base": list(base["EvidTop"]["Base"]), "Head": list(base["EvidTop"]["Base"]) + [("040000", "tree", "d" * 40, "r4")]}
    base["EvidFiles"] = {"Base": dict(base["EvidFiles"]["Base"]), "Head": dict(base["EvidFiles"]["Base"], **{GUARDS: "e" * 40})}
    base["SelfTest"] = {"Result": "PASS", "ToolBlob": base["ToolBlob"], "A6TextBlob": text_blob(base["A6"]),
                        "Summary": {"Mutants": MIN_MUTANTS, "Killed": MIN_MUTANTS, "NotApplied": 0, "ControlFailures": [],
                                    "ByGuard": {g: "1/1" for g in GUARDS_FN if g != "G5B"}}}
    return base


SELFTEST_G5B = [
    ("G5B", "ToolBlob de la autoprueba distinto", m_set(["SelfTest", "ToolBlob"], "8" * 40)),
    ("G5B", "autoprueba con Result FAIL", m_set(["SelfTest", "Result"], "FAIL")),
    ("G5B", "autoprueba de otra A-6", m_set(["SelfTest", "A6TextBlob"], "9" * 40)),
    ("G5B", "autoprueba con un mutante vivo", lambda inp: inp["SelfTest"]["Summary"].__setitem__("Killed", MIN_MUTANTS - 1)),
    ("G5B", "autoprueba con pocos mutantes", lambda inp: inp["SelfTest"]["Summary"].update({"Mutants": 30, "Killed": 30})),
    ("G5B", "autoprueba sin mutantes de G10", lambda inp: inp["SelfTest"]["Summary"]["ByGuard"].pop("G10")),
]


def self_test(inp, cm, ids):
    base = self_test_base(inp)
    control = {g: run_guard(g, copy.deepcopy(base), cm, ids) for g in GUARDS_FN}
    ctrl_fail = [g for g, r in control.items() if r["Result"] != "PASS"]
    rows = []
    for gid, name, mut in MUTANTS + auto_mutants(base["A6"]) + SELFTEST_G5B:
        m = copy.deepcopy(base)
        m_ids = list(ids) + ([SYNTH_ID] if "identidad del host" in name else [])
        try:
            mut(m)
            applied = True
        except AssertionError:
            applied = False
        r = run_guard(gid, m, cm, m_ids) if applied else {"Result": "NOT_APPLIED", "Findings": []}
        new = [x for x in r.get("Findings", []) if x not in control[gid].get("Findings", [])]
        killed = applied and r["Result"] == "FAIL" and bool(new)
        rows.append({"Guard": gid, "Mutant": name, "Applied": applied, "Result": r["Result"], "Killed": killed, "NewFinding": new[:1]})
    summary = {"Mutants": len(rows), "Killed": sum(x["Killed"] for x in rows), "NotApplied": sum(not x["Applied"] for x in rows),
               "Survivors": [x["Mutant"] for x in rows if not x["Killed"]], "ControlFailures": ctrl_fail,
               "ByGuard": {g: "%d/%d" % (sum(x["Killed"] for x in rows if x["Guard"] == g), sum(1 for x in rows if x["Guard"] == g)) for g in GUARDS_FN}}
    return {"Control": {g: {"Result": r["Result"], "Findings": r.get("Findings", [])} for g, r in control.items()}, "Mutants": rows, "Summary": summary,
            "Result": "PASS" if not ctrl_fail and summary["Killed"] == summary["Mutants"] and summary["Mutants"] >= MIN_MUTANTS else "FAIL"}


# ---------------------------------------------------------------- principal
def read_bytes(path):
    with open(path, "rb") as fh:
        return fh.read()


def read_text(path):
    return lf(read_bytes(path)).decode("utf-8")


def pkg_texts(pkgdir):
    out = {}
    for root, dirs, files in os.walk(pkgdir):
        dirs[:] = [d for d in dirs if not d.startswith("tmp") and d != "__pycache__"]
        for n in files:
            if n.endswith((".md", ".py", ".json", ".txt")):
                p = os.path.join(root, n)
                out[os.path.relpath(p, pkgdir).replace("\\", "/")] = read_text(p)
    return out


def write_json(path, res):
    with open(path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(res, ensure_ascii=False, indent=1, default=sorted) + "\n")


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("mode", choices=["self-test", "preview", "run"])
    ap.add_argument("--repo", required=True)
    ap.add_argument("--a6")
    ap.add_argument("--pkgdir")
    ap.add_argument("--r4dir")
    ap.add_argument("--cand-repo")
    ap.add_argument("--cand-head")
    ap.add_argument("--base")
    ap.add_argument("--head")
    ap.add_argument("--final", action="store_true")
    ap.add_argument("--selftest")
    ap.add_argument("--out", required=True)
    ap.add_argument("--tmp")
    a = ap.parse_args()
    os.environ["GIT_OPTIONAL_LOCKS"] = "0"        # ninguna lectura refresca ni bloquea el índice del repositorio leído
    os.environ["GIT_NO_REPLACE_OBJECTS"] = "1"    # ninguna lectura sigue objetos de reemplazo (R1)
    td = a.tmp or tempfile.mkdtemp(prefix="a6r4-guards-")
    os.makedirs(td, exist_ok=True)
    try:
        git = Git(a.repo)
        ids = host_identities(a.repo)
        HOST_IDS[:] = ids
        cm, cm_path = load_clause_map(git, td)
        tool_blob = blob_id(lf(read_bytes(os.path.abspath(__file__))))
        if a.mode == "self-test":
            base = git("rev-parse", a.base or "HEAD").strip()
            a6 = read_text(a.a6)
            inp = collect_common(git, base, base, a6)
            inp.update({"Mode": "SELF_TEST", "A6": a6, "ToolBlob": tool_blob, "Final": False,
                        "C20b": {"Base": BASE_RC, "BaseIsAncestor": True, "ExitCode": 0, "Output": '{"result": "EQUAL (MV-2..MV-6)"}'}})
            res = self_test(inp, cm, ids)
            res.update({"Mode": "SELF_TEST", "Repo": "base " + base, "ToolBlob": tool_blob, "A6TextBlob": text_blob(a6)})
        elif a.mode == "preview":
            base0 = git("rev-parse", a.base or "HEAD").strip()
            a6 = read_text(a.a6)
            if a.cand_repo and a.cand_head:
                cg = Git(a.cand_repo)
                base = cg("rev-parse", a.cand_head + "^1").strip()
                inp = collect_common(cg, base, a.cand_head, a6)
                collect_commit(cg, base, a.cand_head, inp)
                inp["C20b"] = run_c20b(cg, cm_path, a.cand_head, td)
                inp["Candidate"] = {"Base": base, "Head": a.cand_head, "Note": "commit hipotético construido solo en un clon temporal"}
            else:
                inp = collect_common(git, base0, base0, a6)
                inp["C20b"] = run_c20b(git, cm_path, base0, td)
            pk = a.pkgdir or os.path.dirname(os.path.abspath(__file__))
            texts = pkg_texts(pk)
            if a.r4dir:
                texts.update({"r4/" + k: v for k, v in pkg_texts(a.r4dir).items()})
                inp["R4Local"] = local_tree(a.r4dir)
            inp.update({"Mode": "PREVIEW", "A6": a6, "ToolBlob": tool_blob, "PkgTexts": texts, "Final": bool(a.final)})
            stp = a.selftest or os.path.join(pk, "a6r4-selftest.json")
            inp["SelfTest"] = json.load(open(stp, encoding="utf-8")) if os.path.exists(stp) else None
            res = {"Mode": "PREVIEW", "RepoBase": base0, "ToolBlob": tool_blob, "A6TextBlob": text_blob(a6),
                   "Guards": {g: run_guard(g, inp, cm, ids) for g in GUARDS_FN}, "Candidate": inp.get("Candidate")}
            res["Result"] = "PASS" if all(r["Result"] in ("PASS", "NOT_RUN") for r in res["Guards"].values()) else "FAIL"
        else:
            base, head = git("rev-parse", a.base).strip(), git("rev-parse", a.head).strip()
            a6 = git.show(head, A6) or ""
            inp = collect_common(git, base, head, a6)
            collect_commit(git, base, head, inp)
            inp["C20b"] = run_c20b(git, cm_path, head, td)
            inp.update({"Mode": "RUN", "A6": a6, "ToolBlob": git.obj(head, GUARDS), "Final": True,
                        "SelfTest": json.loads(git.show(head, SELFTEST) or "null")})
            res = {"Mode": "RUN", "Base": base, "Head": head, "Guards": {g: run_guard(g, inp, cm, ids) for g in GUARDS_FN}}
            res["Result"] = "PASS" if all(r["Result"] == "PASS" for r in res["Guards"].values()) else "FAIL"
        out = sanitize_obj(json.loads(json.dumps(res, ensure_ascii=False, default=sorted)),
                           tuple(x for x in (a.repo, td, a.cand_repo, a.r4dir, a.pkgdir, os.path.abspath(a.a6) if a.a6 else None) if x))
        write_json(a.out, out)
        summ = {k: v for k, v in out.items() if k in ("Mode", "Result", "Summary")}
        if "Guards" in out:
            summ["Guards"] = {g: r["Result"] for g, r in out["Guards"].items()}
        print(json.dumps(summ, ensure_ascii=False))
        sys.exit(0 if out["Result"] == "PASS" else 1)
    finally:
        if not a.tmp:
            shutil.rmtree(td, ignore_errors=True)


if __name__ == "__main__":
    main()
