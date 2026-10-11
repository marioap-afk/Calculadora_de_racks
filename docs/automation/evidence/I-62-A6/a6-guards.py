#!/usr/bin/env python3
"""Guardas mecánicas de la enmienda candidata I-62 A-6 (instrumento de hechos fijado por blob para el Controller de FX-02 en F6, B-min).

Autoridad: decisiones §76, punto 5 («texto normativo exacto; matriz M-01..M-08; referencias a los ocho puntos del dictamen; guardas mecánicas;
casos negativos y mutantes; compatibilidad con A-1..A-5; delimitación de los insumos canónicos; identidad por blob de las herramientas; efectos sobre
D.5 y la futura Owner Validation; alcance exacto de las decisiones del Owner»). Modeladas sobre docs/automation/evidence/I-62-A5/a5-guards.py.

Uso (siempre con `python -I -B`; solo lectura del repositorio; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I -B a6-guards.py self-test --repo <repo> --a6 <I-62-A-6.md> --out <json> [--base <sha>] [--tmp <dir>]
      Autocomprobación por mutantes con entradas reales leídas del repositorio en la base (HEAD si no se da): cada guarda pasa sobre la entrada real
      (control) y FALLA, con un hallazgo nuevo, sobre cada copia mutada en memoria que le corresponde. Registra ToolBlob y A6TextBlob.
  python -I -B a6-guards.py preview --repo <repo> --a6 <I-62-A-6.md> --pkgdir <dir del paquete> --out <json>
                                    [--cand-repo <clon temporal> --cand-head <sha>] [--tmp <dir>]
      Antes de cualquier commit: todas las guardas sobre la HEAD (G2 NOT_RUN) o sobre un commit candidato hipotético construido solo en un clon
      temporal (base = su padre). La autoprueba custodiada se lee de <pkgdir>/a6-selftest.json.
  python -I -B a6-guards.py run --repo <repo> --base <sha> --head <sha> --out <json> [--tmp <dir>]
      Sobre el commit real que publique A-6 (base = su padre): todas las guardas, con A-6, a6-guards.py y a6-selftest.json leídos de head.

Guardas:
  G1 cabecera (Freeze, A-1..A-5 AGREED con sus blobs, Applies-to, State PROPUESTA, Architect review NOT LAUNCHED, sin veredicto del Coordinator);
     bloques literales de §2 (cada línea igual a la línea fijada de su fuente, por blob; decisiones en la base y sin desplazamiento frente al blob
     fijado); cobertura de citas «…» (literal fijado con su referencia de línea, cita del propio delta, término propio o notación); prefijos
     «xxxxxxxx…» iguales al objeto real en la base; identificadores de 40 hexadecimales existentes; SHA-256 de 64 hexadecimales conocidos y
     recalculados; privacidad (sin rutas de perfil, correos ni identidades del host leídas en tiempo de ejecución).
  G2 rutas del commit (base = padre de head): solo docs/initiatives/I-62-A-6.md y docs/automation/evidence/I-62-A6/, todos añadidos (A); blob de
     A-6 igual al texto comprobado; privacidad de lo añadido.
  G3 superficies prohibidas: la lista de §6.1 de A-6 = la lista de las guardas, y cada superficie con el mismo objeto en base y head; objetos
     congelados (V14, Freeze, A-1..A-5, AUTOMATION_PLAN, WORKFLOW, LIFECYCLE, PROMPT_TEMPLATES, ADR-0046, ADR-0048, RAE, descriptor codex-cli,
     árbol agent-execution, mapa y esquema, clause_map.py, MAP-INVALID, paquete B-min y sus piezas, dictamen, kit de FX-02, I-62-A5) en base y head;
     C-20b (`clause_map.py check <bb0d5522> head` = EQUAL) y ninguna ruta del commit bajo las superficies del mapa.
  G4 dictamen: tabla de §3.2 con los ocho puntos, literales de `A6.MinimalDeltaScope` y en su orden; cada punto con al menos un delta existente y
     cada delta con al menos un punto, coherente con sus líneas «Cubre:»; tabla M-01..M-08 con una fila por disparador y los valores del dictamen;
     dictamen MATERIAL, A-6 necesaria y dos consecuencias OWNER-RESERVED; compatibilidad A-1..A-5, una fila cada una y sin conflicto abierto.
  G5 delta: nueve párrafos (A6-1..A6-9) con su ubicación; aplicación sobre V14 + A-2 + A-4 + A-5 (como en a5-guards.py): A6-1, A6-2, A6-4, A6-7,
     A6-9 y A6-8 tras el fin de A5-2 (fin de D.3), A6-5 y A6-6 tras V14 L2554 (fin de D.4), A6-3 tras V14 L2587 (fin de D.6); añadido puro (ninguna
     línea de V14 reescrita), por ancla y en conjunto; solo cambian D.3, D.4, D.6 y sus ancestros; literal de topes intacto y único; cláusula de
     alcance; frases exigidas (incluidas las prohibiciones de decisiones §76, punto 2) y prohibidas; sin filas de tabla, encabezados ni vallas;
     sin nombres de producto (salvo la etiqueta congelada «**Codex, total**»); cifras de A6-1 iguales a V14 L2442; prohibiciones de §3.13.
  G6 líneas del Owner: §7 con un solo bloque y exactamente dos líneas «NOMBRE = A (…)», nombres fijados y distintos, cada una completa y sin el
     contenido de la otra; trío, blobs y commit iguales a los fijados y al registro de decisiones en la base.
  G7 identidad de los instrumentos: tabla de §3.3 (instrumento, aceptante, especificación) con blob = `git rev-parse <base>:<ruta>` = blob en el
     commit del paquete = blob del dictamen, SHA-256 = SHA-256 del contenido del blob, y e2e-summary.json coherente.
  G8 estado y declaraciones: State PROPUESTA y «No aplicada»; frase de decisiones §76, punto 5, en la cabecera y en §11; ninguna declaración de
     AGREED, aplicación, PASS ni excepción vigente.
  G5b autoprueba custodiada: Result PASS, ToolBlob = blob de a6-guards.py, A6TextBlob = blob de la A-6, todos los mutantes muertos.
Los blobs de los textos se calculan con CRLF -> LF (igual que la conversión de entrada de core.autocrlf=true). Sin escrituras salvo --out y un
directorio temporal propio (--tmp; por defecto, uno nuevo del sistema que se borra al terminar). Nunca escribe la ruta del repositorio ni identidades
del host.
"""
import argparse, copy, difflib, hashlib, importlib.util, json, os, re, shutil, subprocess, sys, tempfile

# ---------------------------------------------------------------- identidades fijadas
PREP_BASE = "1652dd61ed55409db193311d7eda122126fa5062"   # HEAD de la rama sobre la que se preparó A-6
BASE_RC = "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"     # base del mapa (C-20b)
FREEZE_SHA = "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43"
V14_COMMIT = "4c617e82b32b6c810b68d75fc19472efed22b393"
PKG_COMMIT = "9356edf141c795ea718a360e266334ded10d6a76"  # commit del paquete B-min
V14 = "docs/initiatives/I-62-proposal-v14.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
A1, A2, A3, A4, A5 = ("docs/initiatives/I-62-A-%d.md" % i for i in (1, 2, 3, 4, 5))
A6 = "docs/initiatives/I-62-A-6.md"
EVID = "docs/automation/evidence/I-62-A6/"
GUARDS, SELFTEST = EVID + "a6-guards.py", EVID + "a6-selftest.json"
DEC = "docs/automation/decisions/I-62.md"
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
E2E = BMIN + "/e2e/out/e2e-summary.json"
RULING_DIR = "docs/automation/evidence/I-62-architect-bmin"
RULING = RULING_DIR + "/R20261011T162328Z-5c5e/output.json"
KITCC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
A5EVID = "docs/automation/evidence/I-62-A5"

TOOL_BLOB = "9a829b0eeb1210238052b1feac06cfa8f7964aed"
ACC_BLOB = "434ef1fccb0d15762b30244b4a60928120698411"
SPEC_BLOB = "4d448c6fc4670cdf97b21dcd17fc00ba1bedcc0c"
INSTRUMENTS = [(TOOL, TOOL_BLOB), (ACC, ACC_BLOB), (SPEC, SPEC_BLOB)]

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
}
FORBIDDEN_SURFACES = [
    "src/", "tests/", "eng/", "tools/", "deploy/", "assets/", "docs/adr/", "docs/automation/agent-execution/", AP, WF, IL, "docs/FOUNDATIONS.md",
    "AGENTS.md", "CLAUDE.md", "Directory.Build.props", "Directory.Build.targets", "RackCad.sln", "global.json", "NuGet.Config", V14, FREEZE,
    A1, A2, A3, A4, A5, PT, "docs/automation/evidence/I-62-F4/", MAPINV + "/", BMIN + "/", "docs/automation/evidence/I-62-F6/kits/FX-02/",
    RULING_DIR + "/", A5EVID + "/",
]
MAP_SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md",
                "docs/adr/", "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
SOURCES = {"V14": (V14, FROZEN[V14]), "AP": (AP, FROZEN[AP]), "RAE": (RAE, FROZEN[RAE]), "ADR-0046": (ADR46, FROZEN[ADR46]),
           "ADR-0048": (ADR48, FROZEN[ADR48]), "LIFECYCLE": (IL, FROZEN[IL]), "A-2": (A2, FROZEN[A2]), "A-4": (A4, FROZEN[A4]),
           "A-5": (A5, FROZEN[A5])}
BQ = {"A-2", "A-4", "A-5"}   # fuentes cuyas líneas citadas son de un bloque citado («> »): se comparan sin el marcador
DEC_BLOB = "228475b01def62985785042a86bf9d6bc68f9b07"   # decisiones en 1652dd61 (hasta §76); crecen solo por añadido

HEADER_FACTS = [
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
    "Applies-to: I-62",
    "State:\nPROPUESTA. No aplicada",
    "Architect review:\nNOT LAUNCHED.",
    "Amendment: A-6 (candidata;",
    "Owner decision:\nREQUIRED antes del acuerdo (OWNER-RESERVED)",
    "solo el EXECUTION_CONTROLLER de FX-02 en F6 (Topología A)",
]
HEADER_FORBIDDEN = ["Coordinator verdict", "A-6 AGREED","Architect review:\nAGREED", "State:\nAGREED", "State:\nAPLICADA",
                    "A-6 AGREED (", "Owner decision:\nNOT REQUIRED"]
S765 = "A-6 no se presenta a acuerdo mientras exista una consecuencia OWNER-RESERVED sin disponer"

# ---------------------------------------------------------------- literales citados: (fuente, línea inicial, línea final, texto exacto, referencia junto a la cita)
LIT = [
    ("DEC", 1539, 1539, "como insumos canónicos", "DEC L1539"),
    ("DEC", 1539, 1539, "Ni aplicar A-6 ni declararla AGREED", "DEC L1539"),
    ("DEC", 1542, 1542, S765, "DEC L1542"),
    ("V14", 2523, 2523, "| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |", "V14 L2523"),
    ("V14", 2528, 2528, "Ningún tope autoriza gasto ni ejecución ahora.", "V14 L2528"),
    ("V14", 960, 960, "| OV-I62-03 topología A | F6 | C-24 | ejecución compacta (D.5) |", "V14 L960"),
    ("V14", 1043, 1043, "elige una decisión del Owner;", "V14 L1043"),
    ("V14", 974, 974, "autorización previa de apertura y consumo, no transporte", "V14 L974"),
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
]
NOTATION = {"V14 L…", "AP L…", "RAE L…", "DEC L…"}
OWN_TERMS = {"Controller más instrumento"}
DELTA_INNER_LIT = {"**Codex, total**": ("V14", 2442, 2442), "+ 2 sondas": ("V14", 2523, 2523)}
SHORT_BLOBS = {"34ad80ea": V14, "0f6b8608": FREEZE, "c01899a7": A1, "f1e1d6f0": A2, "ea6721f7": A3, "5e2ba68e": A4, "ad94ceed": A5,
               "f525cb1e": AP, "446c32ba": WF, "f19896a8": IL, "384d0b0c": PT, "da275e1a": ADR46, "e1bd8d91": ADR48, "592dcfd4": RAE,
               "155f3469": CODEXCLI, "809fd58f": AE, "90929ad1": KITCC, "3caa0dfa": BMIN, "ee74d0f9": RULING_DIR, "96c3efbc": RULING,
               "f03a0048": A5EVID, "9a829b0e": TOOL, "434ef1fc": ACC}
NON_GIT_HASHES = {"d13914ce"}                    # BinaryHash de codex-cli (SHA-256), no es un objeto Git
BINARY_HASH = "d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec"
APP_VERSION = "26.1007.2314.0"
FINGERPRINT = "F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113"
KNOWN_SHA256 = {  # valor (minúsculas) -> ("blob", ruta) recalculado en la base, o ("dec", texto) presente en decisiones en la base
    "b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200": ("blob", TOOL),
    "173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0": ("blob", ACC),
    "510143f24522211ed0480c813d496dcf47e4c4f8feba8ceb02148ddfa4932fb9": ("blob", SPEC),
    BINARY_HASH: ("dec", BINARY_HASH),
    FINGERPRINT.lower(): ("dec", FINGERPRINT),
}
BLOCK_HEADER = re.compile(r"^(V14|AP|RAE|ADR-0046|ADR-0048|LIFECYCLE) L(\d+)-L(\d+), literal", re.M)

# ---------------------------------------------------------------- dictamen
RULING_VERDICT = "MATERIAL"
M_YES = {"M-02", "M-03", "M-04", "M-05", "M-08"}
OWNER_NAMES = ["A6-ADR0046-9-FX02", "A6-CONTROLLER-MEDICION-CONSUMO"]

# ---------------------------------------------------------------- delta
DELTAS = ["A6-%d" % i for i in range(1, 10)]
SEC = {"A6-1": "### 3.4 A6-1 — ", "A6-2": "### 3.5 A6-2 — ", "A6-3": "### 3.6 A6-3 — ", "A6-4": "### 3.7 A6-4 — ", "A6-5": "### 3.8 A6-5 — ",
       "A6-6": "### 3.9 A6-6 — ", "A6-7": "### 3.10 A6-7 — ", "A6-8": "### 3.11 A6-8 — ", "A6-9": "### 3.12 A6-9 — "}
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
H_D1 = "### D.1 Arranque del fixture (análisis; nada se crea ahora)"
H_D2 = "### D.2 "
H_D3 = "### D.3 Hoja de invocaciones y escenarios"
H_D4 = "### D.4 Resultados y cobertura"
H_D5 = "### D.5 "
H_D6 = "### D.6 Entradas y aislamiento (cobertura declarada)"
H_D7 = "### D.7 "
CAPS = "| **Codex, total** | | **11** | | pool **4** (≤ 2 por fase) | **15** |"
CAPS_LINE = 2442
SCOPE = {
    "A6-1": "Solo para la celda de EXECUTION_CONTROLLER de la Topología A (FX-02), en F6",
    "A6-2": "Solo para cada invocación del Controller de FX-02 en F6",
    "A6-3": "Solo para el instrumento de la regla de A-6 en D.3 y solo en F6",
    "A6-4": "Solo para cada invocación del Controller de FX-02 en F6",
    "A6-5": "Solo para FX-02 en F6",
    "A6-6": "Solo para FX-02 en F6",
    "A6-7": "Solo para la celda de EXECUTION_CONTROLLER de la Topología A (FX-02), en F6",
    "A6-8": "Las reglas de A-6 sobre el instrumento del Controller de FX-02",
    "A6-9": "Solo para FX-02 en F6",
}
REQUIRED = {
    "A6-1": ["un único instrumento determinista de solo lectura", "el archivo cuyo blob fija A-6", "ejecutado desde su objeto Git", "en modo aislado",
             "por su ruta, su versión y el SHA-256 de su ejecutable", "no emite ningún veredicto", "ni PASS, ni FAIL, ni VERIFIED",
             "ningún elemento de la receta del adapter cambia", "Tampoco cambian el descriptor ni el contrato del adapter, los esquemas de salida",
             "el rol ni la autoridad del Controller", "no es un lanzamiento", "Los presupuestos congelados no cambian",
             "(**11** base, pool **4** con ≤ 2 por fase, tope **15**)", "ningún contador se reinicia",
             "los insumos que esta regla añade a una invocación son solo tres",
             "ni el oráculo del Coordinator entran en ninguna invocación del Controller", "es un invalidador", "no entra en F_seed, en F_norm",
             "ningún contrato de producción", "esta regla no cambia el comportamiento de producción"],
    "A6-2": ["sin cambiar el texto de §20.3.1, de AUTOMATION_PLAN 16.24 ni de su procedimiento subordinado",
             "`CanonicalInputs` incluye una entrada `{Path, Blob}` del archivo del instrumento", "Es la única excepción a la clase de §20.3.1",
             "no aporta obligaciones de lectura", "`Class` = ACTION_COMPATIBLE", "como fuente de la obligación (`RequiredBy`), esta regla",
             "`DecisionRef` de la disposición del Coordinator", "Ninguna otra acción se añade", "no contra la `AuthorityRevision` del fixture",
             "Las demás entradas conservan sin cambio esa comprobación", "un blob distinto obliga a recalcular el cierre antes de lanzar",
             "no son entradas del cierre", "no se aplica a ningún otro rol, invocación, unidad ni escenario"],
    "A6-3": ["nunca de un árbol de trabajo", "antes de la salida de AUTOMATION_PLAN 16.4", "fuera de toda cesión", "solo como objeto Git suelto",
             "sin archivo en el árbol, sin índice, sin commit, sin ref y sin push", "si no coincide, no hay lanzamiento",
             "el instrumento no entra en F_seed, en F_norm", "El worktree real de I-62", "entradas privadas prohibidas",
             "fuera de la sesión de Principal del fixture"],
    "A6-4": ["tiene su registro de fidelidad", "comparado por blob", "sin FAITHFUL no hay lanzamiento (P-24)",
             "el SHA-256 de los hechos de cada salida usada", "al menos con la orden Git primaria", "nunca basta por sí sola",
             "se auditan contra el cierre efectivo de insumos de la invocación", "INVALID_REVIEW_CONTEXT (P-22)", "deja Contexto en UNKNOWN",
             "la cobertura se declara"],
    "A6-5": ["la comprobación es `not_run` y cuenta como fail", "nunca es pass",
             "Una discrepancia entre un hecho del instrumento y la orden Git primaria también la deja en `not_run`", "sin elegir entre las dos",
             "como fija 16.9, sin cambio", "Ningún código del instrumento es por sí mismo una clasificación", "R1-R10", "dan REJECT_FACTS",
             "Coordinator rechaza el VERIFIED", "el lanzamiento cuenta y no se devuelve", "FX-02 no es PASS en esa ronda",
             "es FAIL si el rechazo muestra una violación observada", "UNVERIFIED con la causa si falta una precondición",
             "esta regla no abre ninguna invocación"],
    "A6-6": ["se calcula sin el instrumento, sin el aceptante y sin la especificación del instrumento", "la fija el Coordinator, no esa especificación",
             "el oráculo se sella", "antes de la primera invocación del Controller de FX-02 con el instrumento", "nunca se entrega al Controller",
             "demuestra la configuración «Controller más instrumento»", "no el protocolo de producción sin instrumento",
             "no se extiende a D.5, a la Owner Validation"],
    "A6-7": ["tras **una** invocación medida adicional, de solo lectura", "esa invocación es una medición, no una invocación de rol de FX-02",
             "ni VERIFIED, ni C-24 PASS", "la autorización de consumo del Owner", "Se cuenta aparte", "del sumando «+ 2 sondas» de los totales",
             "de los bloques de la regla de nueva medición de A-2", "de la invocación de la regla de A-4 sobre el shell",
             "No usa ni repone un bloque agotado ni una medición consumida", "Su tope es uno en total para FX-02 en F6",
             "P-07 lo comprueba antes de lanzarla", "la invocación no se repite bajo esta regla", "el escenario sigue UNVERIFIED con esa causa (D.4)",
             "blob del instrumento o intérprete declarado", "sin reutilización", "P-01 y OD-2 rigen sin cambio"],
    "A6-8": ["solo tienen efecto si el Owner determina", "antes del acuerdo de A-6", "como decisión reservada", "ADR-0046 #9",
             "FX-02 en F6, Topología A", "la celda de EXECUTION_CONTROLLER",
             "Quedan excluidos de forma expresa la producción, cualquier otra unidad, escenario, rol o topología, D.5 y la Owner Validation",
             "A-6 no concede esa determinación", "no modifica ADR-0046 ni ADR-0048", "no crea un precedente",
             "ninguna excepción está vigente sin la decisión registrada del Owner", "ni el Coordinator ni el Principal la sustituyen",
             "las reglas de A-6 no se aplican", "no autoriza ninguna invocación ni ningún consumo",
             "exige su propia autorización de consumo del Owner"],
    "A6-9": ["el instrumento no emite PASS, FAIL ni VERIFIED", "El Controller los interpreta, los contrasta y emite su resultado estructurado",
             "no se delegan en el instrumento", "incluida la tabla de defectos de `Validate(M)`",
             "A-6 no reinterpreta 16.13 ni adopta la lectura de la especificación del instrumento", "se devuelve al Coordinator, que la decide",
             "el aceptante reproduce R1-R10 y rechaza toda discrepancia", "una implementación de autor distinto",
             "escrita sin ver el código del instrumento", "se deriva mecánicamente del cierre efectivo de insumos de la invocación",
             "fuera de la sesión de Principal del fixture", "fuera de toda cesión", "el oráculo de C-24 nunca se entrega al Controller"],
}
FORBIDDEN_ALL = ["AGREED", "se concede la excepción", "queda vigente", "en producción", "se amplía", "se reponen los", "workspace-write",
                 "repo-write", "sube en", "cualquier rol puede", "otras unidades pueden"]
FORBIDDEN = {
    "A6-1": ["varios instrumentos", "la receta cambia"],
    "A6-3": ["lee el worktree real", "con ref dedicada"],
    "A6-6": ["se entrega al Controller antes", "con el instrumento, sin el aceptante"],
    "A6-7": ["Su tope es dos", "reutiliza el bloque", "usar el bloque agotado"],
    "A6-8": ["A-6 concede", "se modifica ADR-0046"],
}
PRODUCT_TOKENS = re.compile(r"(?i)\b(codex|claude|openai|anthropic|gpt-|opus\b|sonnet\b|haiku\b|chatgpt|gemini|google|copilot|mistral|llama\b|deepseek|grok\b|python)")
PRODUCT_EXEMPT = {"A6-1": ["**Codex, total**"]}
PROHIBITIONS_313 = ["no cambia la receta 16.4", "no cambia el adapter de la celda", "no cambia ningún esquema de salida",
                    "no cambia ningún presupuesto congelado", "no cambia los roles ni la autoridad del Controller",
                    "no cambia el comportamiento de producción fuera de la excepción propuesta"]

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
]

ALLOWED_STATUS = {A6: "A"}

# ---------------------------------------------------------------- privacidad
PRIVACY = re.compile(r"(?i)(?:\b[a-z]:|(?<![\w.])/(?:mnt/)?[a-z])[\\/]+users[\\/]+[^\\/\s%<*]|\b[\w.+-]+@[\w-]+\.[a-z]{2,}\b")
PRIVACY_OK = {"fixture@example.invalid", "neg@example.invalid"}


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
        loc = next((l for l in body.split("\n") if l.startswith("Ubicación:")), "")
        cov = next((l for l in body.split("\n") if l.startswith("Cubre:")), "")
        bq = [l for l in body.split("\n") if l.startswith(">")]
        out[rid] = {"Paragraph": quoted_paragraph("\n".join(bq)), "Location": loc, "Covers": cov, "Start": s, "End": e,
                    "BlockSpan": (md.find(bq[0], s), md.find(bq[-1], s) + len(bq[-1])) if bq else (s, s)}
    return out


def covers_points(line):
    m = re.match(r"^Cubre: puntos? ([0-9, y]+) del dictamen", line)
    return sorted({int(x) for x in re.findall(r"\d+", m.group(1))}) if m else []


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


def virtual_base(v14, a2, a4, a5):
    """V14 + A-2 + A-4 + A-5 aplicadas como en a2/a4/a5-guards.py (añadidos al final de D.3 y de D.1)."""
    p1 = quoted_paragraph("\n".join(l for l in a2_section(a2, "2.3 Delta exacto").split("\n") if l.startswith(">")))
    p2 = quoted_paragraph("\n".join(l for l in a2_section(a2, "3.3 Delta exacto").split("\n") if l.startswith(">")))
    t = v14.replace(ANCHOR_V14, ANCHOR_V14 + "\n\n" + p1 + "\n\n" + p2, 1)
    paras = a4_paragraphs(a4)
    t = t.replace(ANCHOR_A2_END, ANCHOR_A2_END + "\n\n" + "\n\n".join(p for _, p in paras), 1)
    a5p = a5_paragraphs(a5)
    t = t.replace(ANCHOR_D1 + "\n", ANCHOR_D1 + "\n" + "\n" + a5p.get("A5-1", "") + "\n", 1)
    t = t.replace(ANCHOR_A46_END, ANCHOR_A46_END + "\n\n" + a5p.get("A5-2", ""), 1)
    return t, [r for r, _ in paras], sorted(a5p)


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


def collect_common(git, base, head, a6_text):
    inp = {"Base": base, "Head": head}
    inp["Sources"] = {t: git.cat(b) for t, (p, b) in SOURCES.items()}
    inp["Sources"]["DEC"] = git.show(base, DEC) or ""
    inp["DecPinned"] = git.cat(DEC_BLOB)
    inp["ShortAtBase"] = {pre: git.obj(base, p) for pre, p in SHORT_BLOBS.items()}
    paths = sorted(set(FROZEN) | set(FORBIDDEN_SURFACES))
    inp["Objs"] = {"Base": {p: git.obj(base, p) for p in paths}, "Head": {p: git.obj(head, p) for p in paths}}
    inp["FreezeCommit"] = {"V14": git.obj(FREEZE_SHA, V14), "Record": git.obj(FREEZE_SHA, FREEZE), "V14AtCommit": git.obj(V14_COMMIT, V14)}
    inp["Ruling"] = json.loads(git.show(base, RULING) or "null")
    inp["E2E"] = json.loads(git.show(base, E2E) or "null")
    inp["PkgCommitBlobs"] = {p: git.obj(PKG_COMMIT, p) for p, _ in INSTRUMENTS}
    inp["BlobSha256"] = {}
    for p, _ in INSTRUMENTS:
        o = git.obj(base, p)
        inp["BlobSha256"][p] = hashlib.sha256(git.cat_raw(o)).hexdigest() if o else None
    inp["Hex40Exists"] = {h: git.exists(h) for h in sorted(set(HEX40.findall(a6_text)))}
    return inp


def sanitize(text, *paths):
    """Quita de un texto de salida las rutas locales (repositorio, temporal y perfil); nunca se escriben."""
    for p in sorted({x for x in paths + (os.environ.get("USERPROFILE", ""), os.environ.get("TEMP", "")) if x}, key=len, reverse=True):
        for v in {p, p.replace("\\", "/"), p.replace("/", "\\")}:
            text = text.replace(v, "<local>")
    return text


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


# ---------------------------------------------------------------- G1
def g1_literals(inp, ids):
    f, checked = [], {"Header": 0, "Blocks": 0, "BlockLines": 0, "Lit": 0, "Self": 0, "Own": 0, "Notation": 0, "DeltaInner": 0, "ShortBlobs": 0,
                      "Hex40": 0, "Hex64": 0}
    md = inp["A6"] or ""
    hb = header_block(md)
    for fact in HEADER_FACTS:
        checked["Header"] += 1
        if fact not in hb:
            f.append("cabecera sin «%s»" % fact.split("\n")[0][:60])
    for bad in HEADER_FORBIDDEN:
        if bad in hb:
            f.append("cabecera con «%s» (prohibido en esta preparación)" % bad.split("\n")[0])
    fc = inp.get("FreezeCommit") or {}
    if fc and (fc.get("V14") != FROZEN[V14] or fc.get("Record") != FROZEN[FREEZE] or fc.get("V14AtCommit") != FROZEN[V14]):
        f.append("los blobs de V14 y del registro en el commit del Freeze no son los fijados")
    src_lines = {t: inp["Sources"][t].split("\n") for t in inp["Sources"]}
    masked = mask_fences(md)
    s2, e2 = section(md, "## 2. ", r"^## ")
    block_spans = []
    if s2 is None:
        f.append("falta §2")
    else:
        for m in BLOCK_HEADER.finditer(md, s2, e2):
            tag, a, b = m.group(1), int(m.group(2)), int(m.group(3))
            k = md.find("\n>", m.end())
            if k < 0 or k > e2:
                f.append("bloque %s L%d-L%d sin líneas citadas" % (tag, a, b))
                continue
            lines, pos = [], k + 1
            while pos < len(md) and md.startswith(">", pos):
                eol = md.find("\n", pos)
                eol = len(md) if eol < 0 else eol
                lines.append(md[pos:eol])
                pos = eol + 1
            block_spans.append((k + 1, pos))
            want = src_lines[tag][a - 1:b]
            got = [l[2:] if l.startswith("> ") else ("" if l == ">" else l) for l in lines]
            checked["Blocks"] += 1
            checked["BlockLines"] += len(got)
            if got != want:
                bad = [a + i for i, (x, y) in enumerate(zip(got, want)) if x != y] or ["longitud %d frente a %d" % (len(got), len(want))]
                f.append("bloque %s L%d-L%d distinto de la fuente (líneas %s)" % (tag, a, b, bad[:4]))
    rules = a6_rules(md)
    deltas = {r: v["Paragraph"] for r, v in rules.items()}
    dspans = [v["BlockSpan"] for v in rules.values()]
    qs = quotes(masked)
    if qs is None:
        f.append("comillas angulares desequilibradas en A-6")
        qs = []
    lit_by_text = {}
    for row in LIT:
        lit_by_text.setdefault(norm(row[3]), []).append(row)
    inner_lit = {norm(k): v for k, v in DELTA_INNER_LIT.items()}
    own = {norm(x) for x in OWN_TERMS}
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
                lines = src_lines[tag][a - 1:b]
                if tag in BQ:
                    lines = [l[2:] if l.startswith("> ") else l for l in lines]
                if tag == "DEC":
                    pinned = inp["DecPinned"].split("\n")[a - 1:b]
                    if lines != pinned:
                        f.append("decisiones: L%d-L%d ya no es la línea fijada (el archivo no creció solo por añadido)" % (a, b))
                if literal_match(text, lines):
                    win = norm(md[max(0, s - 260):e + 260])
                    if ref is None or ref in win:
                        ok_any = True
                        break
            if ok_any:
                checked["Lit"] += 1
            else:
                f.append("cita literal sin casar con su fuente o sin su referencia de línea: «%s»" % inner[:60])
            continue
        if any(norm(inner) in norm(d) for d in deltas.values()):
            checked["Self"] += 1
            continue
        unclassified.append("«%s»" % inner[:60])
    if unclassified:
        f.append("citas sin clasificar (ni literal fijado, ni del propio delta, ni término propio, ni notación): %s" % unclassified[:6])
    for m in re.finditer(r"(?<![0-9a-fA-F])([0-9a-f]{8})…", masked):
        pre = m.group(1)
        checked["ShortBlobs"] += 1
        if pre in NON_GIT_HASHES:
            continue
        if pre not in SHORT_BLOBS:
            f.append("prefijo de blob sin fijar en las guardas: %s…" % pre)
        elif not (inp["ShortAtBase"].get(pre) or "").startswith(pre):
            f.append("prefijo %s… distinto del objeto real de %s en la base" % (pre, SHORT_BLOBS[pre]))
    for h in sorted(set(HEX40.findall(md))):
        checked["Hex40"] += 1
        if not inp.get("Hex40Exists", {}).get(h):
            f.append("identificador de 40 hexadecimales que no es un objeto del repositorio: %s" % h[:12])
    dec = inp["Sources"].get("DEC") or ""
    for h in sorted(set(x.lower() for x in HEX64.findall(md))):
        checked["Hex64"] += 1
        kind = KNOWN_SHA256.get(h)
        if kind is None:
            f.append("SHA-256 sin fijar en las guardas: %s" % h[:16])
        elif kind[0] == "blob" and inp["BlobSha256"].get(kind[1]) != h:
            f.append("SHA-256 distinto del contenido de %s en la base: %s" % (kind[1], h[:16]))
        elif kind[0] == "dec" and kind[1] not in dec:
            f.append("valor %s… ausente del registro de decisiones en la base" % h[:12])
    priv = ["A-6"]
    f += privacy_findings("A-6", md, ids)
    for name, text in sorted((inp.get("PkgTexts") or {}).items()):
        priv.append(name)
        f += privacy_findings(name, text, ids)
    return {"Check": "G1 cabecera, literales, citas, identificadores y privacidad", "Checked": checked, "PrivacyChecked": len(priv),
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G2
def g2_paths(inp, ids):
    f = []
    st = inp.get("Status")
    if st is None:
        return {"Check": "G2 rutas del commit", "Result": "NOT_RUN", "Info": ["sin commit candidato"], "Findings": []}
    if inp.get("HeadParent") != inp["Base"]:
        f.append("base distinta del padre de head")
    if st.get(A6) != "A":
        f.append("%s no aparece como añadido (A)" % A6)
    if inp.get("HeadObj", {}).get(A6) != text_blob(inp["A6"] or ""):
        f.append("blob de A-6 en head distinto del texto comprobado")
    for p, s in sorted(st.items()):
        if p.startswith(EVID):
            if s != "A":
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID))
        elif p not in ALLOWED_STATUS:
            f.append("ruta fuera de alcance: %s (%s)" % (p, s))
        elif s != ALLOWED_STATUS[p]:
            f.append("%s con estado %s (permitido: %s)" % (p, s, ALLOWED_STATUS[p]))
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
    return re.findall(r"`([^`]+)`", m.group(0)) if m else None


def g3_surfaces(inp):
    f, mism = [], {}
    md = inp["A6"] or ""
    listed = forbidden_list_in_a6(md)
    if listed is None:
        f.append("A-6 no declara la lista de superficies prohibidas (§6.1)")
    else:
        if len(listed) != len(set(listed)):
            f.append("lista de superficies prohibidas con duplicados")
        if set(listed) != set(FORBIDDEN_SURFACES):
            f.append("lista de superficies prohibidas distinta de la de las guardas: faltan %s; sobran %s"
                     % (sorted(set(FORBIDDEN_SURFACES) - set(listed))[:4], sorted(set(listed) - set(FORBIDDEN_SURFACES))[:4]))
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
    touched = sorted(p for p in (inp.get("Status") or {}) if in_map_surfaces(p) or any(p == s or (s.endswith("/") and p.startswith(s))
                                                                                      for s in FORBIDDEN_SURFACES))
    if touched:
        f.append("rutas del commit bajo las superficies prohibidas o del mapa: %s" % touched[:6])
    r = inp["C20b"]
    if not r.get("BaseIsAncestor"):
        f.append("la base del mapa bb0d5522 no es ancestro de head")
    if not (r["ExitCode"] == 0 and "EQUAL" in r["Output"]):
        f.append("clause_map.py check distinto de EQUAL (código %s)" % r["ExitCode"])
    return {"Check": "G3 superficies prohibidas, objetos congelados y C-20b", "Listed": len(listed or []), "Frozen": len(FROZEN),
            "Mismatches": mism, "C20b": {"ExitCode": r["ExitCode"], "Output": r["Output"][-200:]}, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G4
def g4_ruling(inp):
    f = []
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
    mrows = table_rows(md, "## 5. ", r"^## ")
    mat = {m.get("Id"): m.get("Activated") for m in ru.get("Materiality") or []}
    got = {}
    for r in mrows or []:
        if r and re.match(r"^M-0[1-8]$", r[0]):
            if r[0] in got:
                f.append("%s aparece más de una vez en la tabla M" % r[0])
            got[r[0]] = r
    if sorted(got) != ["M-0%d" % i for i in range(1, 9)]:
        f.append("la tabla M no tiene M-01..M-08 exactamente: %s" % sorted(got))
    for mid, r in sorted(got.items()):
        act = r[1].replace("*", "").strip() if len(r) > 1 else ""
        want = "sí" if mid in M_YES else "no"
        if act != want:
            f.append("%s = %s en A-6; el dictamen acreditado dice %s" % (mid, act, want))
        if len(r) < 4 or r[2] != mat.get(mid) or not r[3].strip():
            f.append("%s: columna Dictamen distinta del dictamen o motivo vacío" % mid)
        if (mat.get(mid) == "yes") != (mid in M_YES):
            f.append("%s: el dictamen en la base no coincide con los valores fijados" % mid)
    if len(ru.get("OwnerReserved") or []) != 2:
        f.append("el dictamen no tiene dos consecuencias OWNER-RESERVED")
    crow = table_rows(md, "### 6.2 ") or []
    cids = [r[0] for r in crow if r]
    if cids != ["A-1", "A-2", "A-3", "A-4", "A-5"]:
        f.append("la tabla de compatibilidad no cubre A-1..A-5 una vez cada una y en orden: %s" % cids)
    for r in crow:
        if len(r) != 4 or not r[2] or not r[3].startswith("ninguno"):
            f.append("compatibilidad con %s sin interacción o con un conflicto abierto" % (r[0] if r else "?"))
    return {"Check": "G4 dictamen: ocho puntos, M-01..M-08, OWNER-RESERVED y compatibilidad", "Points": len(rows), "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G5
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
    secs = lambda t: {(lv, h): s for lv, h, s in cm.sections(t)}
    sb = secs(base)
    for name, use in (("D.3", ["D.3"]), ("D.4", ["D.4"]), ("D.6", ["D.6"]), ("A-6 completa", ["D.3", "D.4", "D.6"])):
        applied, ins = base, []
        for g in use:
            order, anc, pre, post = groups[g]
            txt = pre + "\n\n".join(paras[r] for r in order) + post
            applied = applied.replace(anc, anc + txt, 1)
            ins.append((txt, anc))
        ops = [o for o in difflib.SequenceMatcher(None, base.split("\n"), applied.split("\n"), autojunk=False).get_opcodes() if o[0] != "equal"]
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
        for ph in REQUIRED[rid]:
            if norm(ph) not in norm(para):
                f.append("%s: falta la frase exigida «%s»" % (rid, ph[:60]))
        for ph in FORBIDDEN_ALL + FORBIDDEN.get(rid, []):
            if ph in para:
                f.append("%s: contiene «%s» (prohibido)" % (rid, ph))
        scrub = para
        for ex in PRODUCT_EXEMPT.get(rid, []):
            scrub = scrub.replace(ex, "")
        hits = sorted(set(m.group(0).lower() for m in PRODUCT_TOKENS.finditer(scrub)))
        if hits:
            f.append("%s: nombres de producto en el delta: %s" % (rid, hits))
        if re.search(r"(?<!ningún contador )se reinicia", para):
            f.append("%s: «se reinicia» fuera de «ningún contador se reinicia»" % rid)
    m = re.search(r"\(\*\*(\d+)\*\* base, pool \*\*(\d+)\*\* con ≤ (\d+) por fase, tope \*\*(\d+)\*\*\)", paras["A6-1"])
    cm_caps = re.search(r"\*\*(\d+)\*\* \| \| pool \*\*(\d+)\*\* \(≤ (\d+) por fase\) \| \*\*(\d+)\*\*", CAPS)
    if not m or m.groups() != cm_caps.groups():
        f.append("A6-1: las cifras de la fila «Codex, total» no son las de V14 L2442 (%s)" % (str(m.groups()) if m else "sin cifras",))
    s313, e313 = section(md, "### 3.13 ")
    body313 = norm(md[s313:e313]) if s313 is not None else ""
    for ph in PROHIBITIONS_313:
        if norm(ph) not in body313:
            f.append("§3.13 sin la prohibición «%s»" % ph)
    info.append("A6-1, A6-2, A6-4, A6-7, A6-9 y A6-8 tras A5-2 (fin de D.3); A6-5 y A6-6 tras V14 L2554 (fin de D.4); A6-3 tras V14 L2587 (fin de D.6)")
    return {"Check": "G5 pureza del delta", "Paragraphs": {r: len(paras[r]) for r in DELTAS}, "Info": info, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G6
OWNER_REQ = {
    "A6-ADR0046-9-FX02": ["determinación OWNER-RESERVED", "previa al acuerdo de A-6", "ADR-0046 #9", "conservado por ADR-0048",
                          "EXECUTION_CONTROLLER de FX-02 en F6, Topología A", "solo el instrumento de hechos de blob " + TOOL_BLOB,
                          "solo el aceptante de blob " + ACC_BLOB, PKG_COMMIT, "R20261011T162328Z-5c5e",
                          "excluye la producción, cualquier otra unidad, escenario, rol o topología, D.5 y la Owner Validation",
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
    "A6-ADR0046-9-FX02": ["1 invocación", "codex-cli", "trío", "gpt-", "medición", "reintento"],
    "A6-CONTROLLER-MEDICION-CONSUMO": ["determinación", "ADR-0046 #9", "excepción", "dentro de A2-P2", "reutilizando"],
}


def g6_owner(inp):
    f = []
    md = inp["A6"] or ""
    s7, e7 = section(md, "## 7. ", r"^## ")
    if s7 is None:
        return {"Check": "G6 líneas del Owner", "Findings": ["falta §7"], "Result": "FAIL"}
    body = md[s7:e7]
    blocks = re.findall(r"^```text\n(.*?)\n```", body, re.S | re.M)
    if len(blocks) != 1:
        f.append("§7 no tiene exactamente un bloque de líneas del Owner (%d)" % len(blocks))
    lines = [l for l in (blocks[0] if blocks else "").split("\n") if l.strip()]
    if len(lines) != 2:
        f.append("§7 no tiene exactamente dos líneas del Owner (%d)" % len(lines))
    names = []
    for l in lines:
        m = re.match(r"^(A6-[A-Z0-9-]+) = A \((.+)\)$", l)
        if not m:
            f.append("línea del Owner mal formada: %s" % l[:50])
            continue
        names.append(m.group(1))
        name, content = m.group(1), m.group(2)
        if name in OWNER_REQ:
            for ph in OWNER_REQ[name]:
                if ph not in content:
                    f.append("%s sin «%s»" % (name, ph[:60]))
            for ph in OWNER_FORBIDDEN[name]:
                if ph in content:
                    f.append("%s contiene «%s» (contenido de la otra línea o prohibido)" % (name, ph))
        if content.count(" = A (") or re.search(r"\) A6-", content):
            f.append("%s parece fundir dos líneas" % name)
    if names != OWNER_NAMES:
        f.append("nombres de las líneas del Owner distintos de los fijados o repetidos: %s" % names)
    if len(set(names)) != len(names):
        f.append("líneas del Owner con el mismo nombre")
    if norm("separadas y distintas") not in norm(body) or "**(a)" not in body or "**(b)" not in body:
        f.append("§7 no declara (a) y (b) como consecuencias separadas y distintas")
    dec = inp["Sources"].get("DEC") or ""
    for v in (BINARY_HASH, APP_VERSION, FINGERPRINT):
        if v not in dec:
            f.append("valor del trío ausente del registro de decisiones en la base: %s" % v[:16])
    return {"Check": "G6 líneas del Owner", "Lines": len(lines), "Names": names, "Findings": f, "Result": "PASS" if not f else "FAIL"}


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
            f.append("%s: blob en A-6 %s distinto del fijado %s" % (p, (blob or "")[:12], b[:12]))
        if inp["Objs"]["Base"].get(p) != b:
            f.append("%s: blob en la base %s distinto del fijado" % (p, (inp["Objs"]["Base"].get(p) or "")[:12]))
        if inp["PkgCommitBlobs"].get(p) != b:
            f.append("%s: blob en el commit del paquete distinto del fijado" % p)
        if sha != inp["BlobSha256"].get(p):
            f.append("%s: SHA-256 en A-6 distinto del contenido del blob en la base" % p)
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
    for short, full in (("9a829b0e", TOOL_BLOB), ("434ef1fc", ACC_BLOB)):
        if not full.startswith(short):
            f.append("prefijo %s… distinto del blob fijado" % short)
    return {"Check": "G7 identidad por blob de los instrumentos", "Rows": len(got), "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G8
def g8_status(inp):
    f = []
    md = inp["A6"] or ""
    hb = header_block(md)
    if "State:\nPROPUESTA. No aplicada" not in hb:
        f.append("cabecera sin «State: PROPUESTA. No aplicada»")
    if norm(S765) not in norm(hb):
        f.append("cabecera sin la frase de decisiones §76, punto 5")
    s11, e11 = section(md, "## 11. ", r"^## ")
    if s11 is None or norm(S765) not in norm(md[s11:e11]):
        f.append("§11 sin la frase de decisiones §76, punto 5")
    masked = mask_fences(md)
    for pat, what in CLAIMS:
        if re.search(pat, masked) or re.search(pat, hb):
            f.append("declaración prohibida en esta preparación: %s" % what)
    if re.search(r"(?m)^State:\s*\n(?!PROPUESTA)", hb):
        f.append("estado distinto de PROPUESTA")
    return {"Check": "G8 estado y declaraciones", "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G5b
def g5b_selftest(inp):
    f = []
    st = inp.get("SelfTest")
    if not st:
        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a6-selftest.json"], "Result": "FAIL"}
    if st.get("Result") != "PASS":
        f.append("a6-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a6-guards.py")
    if st.get("A6TextBlob") != text_blob(inp["A6"] or ""):
        f.append("A6TextBlob de la autoprueba distinto del blob de la A-6 comprobada")
    s = st.get("Summary") or {}
    if not s.get("Mutants") or s.get("Killed") != s.get("Mutants") or s.get("ControlFailures") or s.get("Mutants", 0) < 30:
        f.append("autoprueba con menos de 30 mutantes, mutantes vivos o controles fallidos: %s" % {k: s.get(k) for k in ("Mutants", "Killed")})
    return {"Check": "G5b autoprueba custodiada", "Summary": s, "Findings": f, "Result": "PASS" if not f else "FAIL"}


GUARDS_FN = {"G1": lambda inp, cm, ids: g1_literals(inp, ids), "G2": lambda inp, cm, ids: g2_paths(inp, ids), "G3": lambda inp, cm, ids: g3_surfaces(inp),
             "G4": lambda inp, cm, ids: g4_ruling(inp), "G5": lambda inp, cm, ids: g5_delta(inp, cm), "G6": lambda inp, cm, ids: g6_owner(inp),
             "G7": lambda inp, cm, ids: g7_instruments(inp), "G8": lambda inp, cm, ids: g8_status(inp), "G5B": lambda inp, cm, ids: g5b_selftest(inp)}


# ---------------------------------------------------------------- autoprueba por mutantes
SYNTH_ID = "zq-host-" + "user-7b2e"
SYNTH_WIN_PATH = "C:" + "\\" + "Us" + "ers" + "\\" + "someone" + "\\" + "x"
SYNTH_MAIL = "a.b" + "@" + "example" + ".com"
OWN_A = "A6-ADR0046-9-FX02 = A ("
OWN_B = "\nA6-CONTROLLER-MEDICION-CONSUMO = A ("


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


MUTANTS = [
    # G1: literales, cabecera, citas, identificadores y privacidad
    ("G1", "literal cambiado en un carácter (V14 L2523)", m_text("V14 L2523 «| A | ≤ 15 + 2 sondas", "V14 L2523 «| A | ≤ 16 + 2 sondas")),
    ("G1", "referencia de línea cambiada (A-4 L203)", m_text("A-4 L203: «Su tope", "A-4 L204: «Su tope")),
    ("G1", "FREEZE_SHA cambiado", m_text("FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43", "FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd44")),
    ("G1", "blob de A-5 de la cabecera cambiado", m_text("blob ad94ceedacbf3dbf70f0217a583978d0e32592fc). A-6", "blob ad94ceedacbf3dbf70f0217a583978d0e32592fd). A-6")),
    ("G1", "veredicto del Coordinator añadido", m_text("Owner decision:\n", "Coordinator verdict:\nAGREED\n\nOwner decision:\n")),
    ("G1", "cita sin clasificar", m_text("## 1. Alcance\n", "## 1. Alcance\n\nUna cita «texto que no existe en ninguna fuente».\n")),
    ("G1", "línea de un bloque literal de §2 cambiada (receta 16.4)", m_text("`-s read-only`; `-m`", "`-s workspace-write`; `-m`")),
    ("G1", "prefijo de blob abreviado falso", m_text("AUTOMATION_PLAN `f525cb1e…`", "AUTOMATION_PLAN `f525cb1f…`")),
    ("G1", "ancla citada alterada (V14 L2587)", m_text("termina en «automáticas y la auditoría, con su límite.»", "termina en «automáticas y la auditoría, sin su límite.»")),
    ("G1", "cita de decisiones alterada (DEC L1541)", m_text("«El Coordinator no sustituye esa decisión»", "«El Coordinator sustituye esa decisión»")),
    ("G1", "cita interna del delta alterada («**Codex, total**»)", m_text("«**Codex, total**» (**11**", "«**Codex total**» (**11**")),
    ("G1", "identificador de 40 hexadecimales inexistente", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nObjeto 0123456789abcdef0123456789abcdef01234567.\n")),
    ("G1", "SHA-256 sin fijar", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nSHA-256 " + "ab" * 32 + ".\n")),
    ("G1", "ruta de perfil de usuario", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nRuta " + SYNTH_WIN_PATH + "\n")),
    ("G1", "correo", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nContacto " + SYNTH_MAIL + "\n")),
    ("G1", "identidad del host", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nSesión de " + SYNTH_ID + "\n")),
    # G2: rutas del commit
    ("G2", "ruta de producción en el commit", m_status("src/RackCad.Core/Nuevo.cs", "A")),
    ("G2", "V14 modificada en el commit", m_status(V14, "M")),
    ("G2", "A-6 como modificada (M)", m_set(["Status", A6], "M")),
    ("G2", "evidencia de I-62 modificada en el mismo commit", m_status("docs/automation/evidence/I-62-evidence.md", "M")),
    ("G2", "ruta fuera de I-62-A6 (kit de FX-02)", m_status("docs/automation/evidence/I-62-F6/kits/FX-02/x.md", "A")),
    ("G2", "archivo de I-62-A6 modificado en lugar de añadido", m_status(EVID + "preflight.md", "M")),
    ("G2", "privacidad en un archivo añadido", m_status(EVID + "nota.md", "A", "ruta " + SYNTH_WIN_PATH + "\n")),
    ("G2", "blob de A-6 en head distinto", m_set(["HeadObj", A6], "0" * 40)),
    # G3: superficies prohibidas, objetos congelados y C-20b
    ("G3", "V14 cambiada en head", m_set(["Objs", "Head", V14], "1" * 40)),
    ("G3", "blob del instrumento cambiado en head", m_set(["Objs", "Head", TOOL], "2" * 40)),
    ("G3", "árbol agent-execution cambiado (descriptor o esquemas)", m_set(["Objs", "Head", AE], "3" * 40)),
    ("G3", "AUTOMATION_PLAN cambiado (receta 16.4)", m_set(["Objs", "Head", AP], "4" * 40)),
    ("G3", "dictamen cambiado en la base", m_set(["Objs", "Base", RULING], "5" * 40)),
    ("G3", "árbol src distinto entre base y head", m_set(["Objs", "Head", "src/"], "6" * 40)),
    ("G3", "superficie prohibida retirada de la lista de A-6", m_text("`docs/adr/`, `docs/automation/agent-execution/`, ", "`docs/automation/agent-execution/`, ")),
    ("G3", "ruta de superficie en el commit (WORKFLOW)", m_status(WF, "M")),
    ("G3", "C-20b distinto de EQUAL", m_set(["C20b", "Output"], '{"result": ["MV-3"]}')),
    # G4: dictamen
    ("G4", "un punto del dictamen retirado (punto 3)", m_re(r"^\| 3 \| Vía del blob[^\n]*\n", "")),
    ("G4", "texto de un punto alterado (punto 5)", m_text("efecto de los códigos de fallo cerrado de la herramienta y del REJECT_FACTS", "efecto de los códigos de la herramienta y del REJECT_FACTS")),
    ("G4", "punto 7 asignado a otro delta", m_re(r"(^\| 7 \| [^\n]*\| )A6-7 \|$", r"\1A6-1 |")),
    ("G4", "línea «Cubre» incoherente (A6-6)", m_text("Cubre: punto 6 del dictamen.", "Cubre: punto 2 del dictamen.")),
    ("G4", "M-02 cambiado a no", m_text("| M-02 | **sí** | yes |", "| M-02 | no | yes |")),
    ("G4", "M-06 cambiado a sí", m_text("| M-06 | no | no |", "| M-06 | **sí** | no |")),
    ("G4", "fila M-07 retirada", m_re(r"^\| M-07 \|[^\n]*\n", "")),
    ("G4", "fila M-08 duplicada", m_re(r"(^\| M-08 \|[^\n]*\n)", r"\1\1")),
    ("G4", "compatibilidad sin A-3", m_re(r"^\| A-3 \| AGREED[^\n]*\n", "")),
    ("G4", "dictamen con una sola consecuencia OWNER-RESERVED", lambda inp: inp["Ruling"].__setitem__("OwnerReserved", inp["Ruling"]["OwnerReserved"][:1])),
    # G5: pureza del delta
    ("G5", "ubicación de A6-3 en D.5", m_text("Ubicación: al final de V14 D.6, a continuación", "Ubicación: al final de V14 D.5, a continuación")),
    ("G5", "orden de D.3 alterado (A6-7 tras A6-2)", m_text("Ubicación: a continuación del párrafo de A6-4 (al final de V14 D.3)", "Ubicación: a continuación del párrafo de A6-2 (al final de V14 D.3)")),
    ("G5", "tope de Codex distinto en A6-1", m_text("tope **15**), las demás filas", "tope **16**), las demás filas")),
    ("G5", "fila de tabla en el delta", m_text(">    invocación con una huella sin aceptar.»", ">    invocación con una huella sin aceptar.\n> | fila | nueva |»")),
    ("G5", "nombre de producto en A6-7", m_text("intérprete declarado las operaciones", "intérprete declarado de codex-cli las operaciones")),
    ("G5", "A6-2 sin cláusula de alcance", m_text("Solo para cada invocación del Controller de FX-02 en F6\n> que use el instrumento de la regla anterior", "Para cada invocación del Controller de FX-02 en F6\n> que use el instrumento de la regla anterior")),
    ("G5", "A6-1 permite cambiar la receta", m_text("ningún elemento de la receta del adapter cambia", "la receta cambia si hace falta")),
    ("G5", "A6-1 amplía presupuestos congelados", m_text("Los presupuestos congelados no cambian", "Los presupuestos congelados se amplían")),
    ("G5", "A6-7 con tope dos", m_text("Su tope es uno en total para FX-02 en F6, y P-07", "Su tope es dos en total para FX-02 en F6, y P-07")),
    ("G5", "A6-7 reutiliza el bloque agotado", m_text("No usa ni repone un bloque agotado ni una medición consumida.", "Puede usar el bloque agotado.")),
    ("G5", "A6-3 lleva el instrumento a F_seed", m_text("> 2. el instrumento no entra en F_seed, en F_norm, en el manifiesto", "> 2. el instrumento entra en F_seed, no en F_norm, ni en el manifiesto")),
    ("G5", "A6-6 calcula el oráculo con el instrumento", m_text("se calcula sin el instrumento, sin el aceptante", "se calcula con el instrumento, sin el aceptante")),
    ("G5", "A6-9 aceptante en la sesión de Principal", m_text("> 4. el aceptante se ejecuta fuera de la sesión de Principal del fixture", "> 4. el aceptante se ejecuta dentro de la sesión de Principal del fixture")),
    ("G5", "A6-8 concede la excepción", m_text("A-6 no concede esa determinación", "A-6 concede esa determinación")),
    ("G5", "A6-8 sin la exclusión de D.5", m_text("rol o topología, D.5 y la\n>    Owner Validation", "rol o topología y la\n>    Owner Validation")),
    ("G5", "A6-5 sin el rechazo del VERIFIED", m_text("Coordinator rechaza el VERIFIED, que no se ingiere", "Coordinator registra el VERIFIED, que se ingiere")),
    ("G5", "A6-4 sin auditoría contra el cierre", m_text("se auditan contra el cierre efectivo de insumos de la", "se registran junto al cierre efectivo de insumos de la")),
    ("G5", "§3.13 sin la prohibición de la receta", m_text("- no cambia la receta 16.4 (AP L541-L543);", "- (sin efecto sobre la receta 16.4) (AP L541-L543);")),
    # G6: líneas del Owner
    ("G6", "las dos líneas del Owner fundidas en una", m_text("la disposición del Coordinator que la cite)" + OWN_B, "la disposición del Coordinator que la cite); A6-CONTROLLER-MEDICION-CONSUMO = A (")),
    ("G6", "línea (b) retirada", m_re(r"\nA6-CONTROLLER-MEDICION-CONSUMO = A \([^\n]*\)", "")),
    ("G6", "línea (a) sin la exclusión de D.5", m_text("escenario, rol o topología, D.5 y la Owner Validation; no modifica", "escenario, rol o topología y la Owner Validation; no modifica")),
    ("G6", "línea (b) reutiliza A2-P2", m_text("fuera de A2-P2, sin reutilizar su bloque agotado", "dentro de A2-P2, reutilizando su bloque agotado")),
    ("G6", "blob del instrumento cambiado en la línea (b)", m_text("con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed y el intérprete", "con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aee y el intérprete")),
    ("G6", "línea (a) autoriza una medición", m_text("no autoriza ninguna invocación ni consumo ni acepta", "autoriza 1 invocación de medición ni acepta")),
    ("G6", "línea (b) con el nombre de la (a)", m_text("\nA6-CONTROLLER-MEDICION-CONSUMO = A (1 invocación", "\nA6-ADR0046-9-FX02 = A (1 invocación")),
    ("G6", "trío cambiado en la línea (b) (AppVersion)", m_text("/26.1007.2314.0/F909F480", "/26.1007.2315.0/F909F480")),
    # G7: identidad de los instrumentos
    ("G7", "blob del instrumento cambiado en §3.3", m_text("| `9a829b0eeb1210238052b1feac06cfa8f7964aed` | `b03c3a19", "| `9a829b0eeb1210238052b1feac06cfa8f7964aef` | `b03c3a19")),
    ("G7", "SHA-256 del aceptante cambiado en §3.3", m_text("`173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0` |", "`173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb1` |")),
    ("G7", "fila de la especificación retirada de §3.3", m_re(r"^\| Especificación \(no normativa\)[^\n]*\n", "")),
    ("G7", "e2e-summary con otro blob de herramienta", lambda inp: inp["E2E"].__setitem__("ToolBlob", "7" * 40)),
    # G8: estado y declaraciones
    ("G8", "A-6 declarada AGREED en §11", m_text("## 11. Estado de revisión\n", "## 11. Estado de revisión\n\nA-6 = AGREED.\n")),
    ("G8", "frase de §76.5 retirada de la cabecera", m_text("REQUIRED antes del acuerdo (OWNER-RESERVED): la determinación sobre ADR-0046 #9 de §7 (a). " + S765 + ".",
                                                         "REQUIRED antes del acuerdo (OWNER-RESERVED): la determinación sobre ADR-0046 #9 de §7 (a).")),
    ("G8", "frase de §76.5 retirada de §11", m_text("\n\n" + S765 + ".\n\nMientras A-6", "\n\nMientras A-6")),
    ("G8", "Coordinator = AGREED en §11", m_text("| Coordinator | sin veredicto | — |", "| Coordinator = AGREED | sin veredicto | — |")),
    ("G8", "C-24 PASS declarado", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nC-24 = PASS.\n")),
    ("G8", "estado AGREED", m_text("State:\nPROPUESTA. No aplicada", "State:\nAGREED. No aplicada")),
    ("G8", "excepción declarada vigente", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nLa excepción queda vigente.\n")),
    ("G8", "A-6 declarada aplicada", m_text("## 10. Evidencia\n", "## 10. Evidencia\n\nA-6 queda aplicada.\n")),
    # G5b
    ("G5B", "ToolBlob de la autoprueba distinto", m_set(["SelfTest", "ToolBlob"], "8" * 40)),
    ("G5B", "autoprueba con Result FAIL", m_set(["SelfTest", "Result"], "FAIL")),
]


def run_guard(gid, inp, cm, ids):
    try:
        return GUARDS_FN[gid](inp, cm, ids)
    except Exception as e:  # una excepción en una guarda cuenta como FAIL con hallazgo
        return {"Check": gid, "Findings": ["excepción: %s" % sanitize(str(e))], "Result": "FAIL"}


def self_test(inp, cm, ids):
    base = copy.deepcopy(inp)
    base["Status"] = {A6: "A", EVID + "a6-guards.py": "A", EVID + "preflight.md": "A"}
    base["HeadTexts"] = {A6: base["A6"], EVID + "a6-guards.py": "# guardas\n", EVID + "preflight.md": "# preflight\n"}
    base["HeadParent"] = base["Base"]
    base["HeadObj"] = {A6: text_blob(base["A6"])}
    base["Objs"] = {"Base": dict(base["Objs"]["Base"]), "Head": dict(base["Objs"]["Base"])}
    base["SelfTest"] = {"Result": "PASS", "ToolBlob": base["ToolBlob"], "A6TextBlob": text_blob(base["A6"]),
                        "Summary": {"Mutants": 30, "Killed": 30, "ControlFailures": []}}
    control = {g: run_guard(g, copy.deepcopy(base), cm, ids) for g in GUARDS_FN}
    ctrl_fail = [g for g, r in control.items() if r["Result"] != "PASS"]
    rows = []
    for gid, name, mut in MUTANTS:
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
            "Result": "PASS" if not ctrl_fail and summary["Killed"] == summary["Mutants"] and summary["Mutants"] >= 30 else "FAIL"}


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
        fh.write(json.dumps(res, ensure_ascii=False, indent=1) + "\n")


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
    ap.add_argument("--cand-repo")
    ap.add_argument("--cand-head")
    ap.add_argument("--base")
    ap.add_argument("--head")
    ap.add_argument("--out", required=True)
    ap.add_argument("--tmp")
    a = ap.parse_args()
    os.environ["GIT_OPTIONAL_LOCKS"] = "0"   # ninguna lectura refresca ni bloquea el índice del repositorio leído
    td = a.tmp or tempfile.mkdtemp(prefix="a6-guards-")
    os.makedirs(td, exist_ok=True)
    try:
        git = Git(a.repo)
        ids = host_identities(a.repo)
        cm, cm_path = load_clause_map(git, td)
        tool_blob = blob_id(lf(read_bytes(os.path.abspath(__file__))))
        if a.mode == "self-test":
            base = git("rev-parse", a.base or "HEAD").strip()
            a6 = read_text(a.a6)
            inp = collect_common(git, base, base, a6)
            inp.update({"Mode": "SELF_TEST", "A6": a6, "ToolBlob": tool_blob,
                        "C20b": {"Base": BASE_RC, "BaseIsAncestor": True, "ExitCode": 0, "Output": '{"result": "EQUAL (MV-2..MV-6)"}'}})
            res = self_test(inp, cm, ids)
            res.update({"Mode": "SELF_TEST", "Repo": "base " + base, "ToolBlob": tool_blob, "A6TextBlob": text_blob(a6)})
        elif a.mode == "preview":
            head = git("rev-parse", "HEAD").strip()
            a6 = read_text(a.a6)
            if a.cand_repo and a.cand_head:
                cg = Git(a.cand_repo)
                base = cg("rev-parse", a.cand_head + "^1").strip()
                inp = collect_common(cg, base, a.cand_head, a6)
                collect_commit(cg, base, a.cand_head, inp)
                inp["C20b"] = run_c20b(cg, cm_path, a.cand_head, td)
                inp["Candidate"] = {"Base": base, "Head": a.cand_head, "Note": "commit hipotético construido solo en un clon temporal"}
            else:
                inp = collect_common(git, head, head, a6)
                inp["C20b"] = run_c20b(git, cm_path, head, td)
            inp.update({"Mode": "PREVIEW", "A6": a6, "ToolBlob": tool_blob, "PkgTexts": pkg_texts(a.pkgdir) if a.pkgdir else {}})
            stp = os.path.join(a.pkgdir or os.path.dirname(os.path.abspath(__file__)), "a6-selftest.json")
            inp["SelfTest"] = json.load(open(stp, encoding="utf-8")) if os.path.exists(stp) else None
            res = {"Mode": "PREVIEW", "RepoHead": head, "ToolBlob": tool_blob, "A6TextBlob": text_blob(a6),
                   "Guards": {g: run_guard(g, inp, cm, ids) for g in GUARDS_FN}, "Candidate": inp.get("Candidate")}
            res["Result"] = "PASS" if all(r["Result"] in ("PASS", "NOT_RUN") for r in res["Guards"].values()) else "FAIL"
        else:
            base, head = git("rev-parse", a.base).strip(), git("rev-parse", a.head).strip()
            a6 = git.show(head, A6) or ""
            inp = collect_common(git, base, head, a6)
            collect_commit(git, base, head, inp)
            inp["C20b"] = run_c20b(git, cm_path, head, td)
            inp.update({"Mode": "RUN", "A6": a6, "ToolBlob": git.obj(head, GUARDS), "SelfTest": json.loads(git.show(head, SELFTEST) or "null")})
            res = {"Mode": "RUN", "Base": base, "Head": head, "Guards": {g: run_guard(g, inp, cm, ids) for g in GUARDS_FN}}
            res["Result"] = "PASS" if all(r["Result"] == "PASS" for r in res["Guards"].values()) else "FAIL"
        write_json(a.out, res)
        summ = {k: v for k, v in res.items() if k in ("Mode", "Result", "Summary")}
        if "Guards" in res:
            summ["Guards"] = {g: r["Result"] for g, r in res["Guards"].items()}
        print(json.dumps(summ, ensure_ascii=False))
        sys.exit(0 if res["Result"] == "PASS" else 1)
    finally:
        if not a.tmp:
            shutil.rmtree(td, ignore_errors=True)


if __name__ == "__main__":
    main()
