#!/usr/bin/env python3
"""Guardas mecánicas de la enmienda candidata I-62 A-5 (A5-1: fixture B1 conforme al mapa de cláusulas; A5-2: sesión de Principal de FX-U2).

Autoridad: decisiones §71, punto 4 («ejecutar guardas de alcance y pruebas negativas; confirmar que el delta no altera contratos de producción»).
Modeladas sobre docs/automation/evidence/I-62-A4/a4-guards.py (G1 literales/cabecera/privacidad, G2 rutas, G3 blobs congelados, G4 C-20b, G5
pureza del delta, G5b autoprueba).

Uso (siempre con `python -I -B`; solo lectura del repositorio; sin red; nunca ejecuta runtimes de IA ni CLI de modelos):
  python -I -B a5-guards.py self-test --repo <worktree> --a5 <I-62-A-5.md> --out <json> [--tmp <dir>]
      Autocomprobación por mutantes con entradas reales leídas del repositorio: cada guarda pasa sobre la entrada real (control) y FALLA, con un
      hallazgo nuevo, sobre cada copia mutada en memoria que le corresponde. Registra ToolBlob (este archivo) y A5TextBlob.
  python -I -B a5-guards.py preview --repo <worktree> --a5 <I-62-A-5.md> --pkgdir <dir del paquete> --out <json>
                                    [--cand-repo <clon temporal> --cand-head <sha>] [--tmp <dir>]
      Antes de cualquier commit: G1 (con privacidad de todo el paquete), G3 y G4 en la HEAD (o en el commit candidato hipotético, construido solo
      en un clon temporal), G2 sobre ese commit candidato si se da, G5 y G5b (a5-selftest.json del paquete frente a este archivo y a la A-5).
  python -I -B a5-guards.py run --repo <repo> --base <sha> --head <sha> --out <json> [--tmp <dir>]
      Sobre el commit real que publique A-5 (base = su padre): G1..G5 y G5b, con A-5, a5-guards.py y a5-selftest.json leídos de head.

Guardas:
  G1 cabecera (Freeze, A-1..A-4 AGREED con sus blobs, Applies-to, State PROPUESTA, Architect review NOT LAUNCHED, sin veredicto del Coordinator);
     bloques literales de §2 (cada línea igual a la línea fijada de su fuente, por blob); cobertura de citas «…»: toda cita de nivel 0 fuera de los
     deltas es un literal fijado (fuente por blob, rango de líneas y referencia de línea junto a la cita), una cita del propio delta o notación; las
     citas dentro de los deltas, literales fijados o términos propios declarados; prefijos de blob «xxxxxxxx…» iguales a los reales en la base;
     privacidad: sin rutas de perfil, sin correos, sin el nombre de usuario, el correo ni el nombre Git del host (leídos en tiempo de ejecución).
  G2 rutas del commit (base = padre de head): A-5 añadida con el blob de su texto; solo el paquete del Architect, docs/automation/evidence/I-62-A5/,
     evidencia y decisiones por añadido, estado y README de F6; privacidad de lo añadido.
  G3 blobs congelados (V14, Freeze, A-1..A-4, AUTOMATION_PLAN, WORKFLOW, LIFECYCLE, PROMPT_TEMPLATES, ADR-0046, ADR-0048, mapa, esquema,
     árbol agent-execution, clause_map.py, mv_check.py, árbol MAP-INVALID) en base y head; árboles y archivos de producción iguales en base y head.
  G4 C-20b: ninguna ruta cambiada bajo las superficies del mapa; mapa = 4d49d3e1 en head; `clause_map.py check <bb0d5522> head` (extraído por
     blob) = EQUAL.
  G5 delta: §3.2 (A5-1) y §3.3 (A5-2) con un párrafo normativo cada uno y su ubicación; aplicación sobre V14 + A-2 + A-4 (aplicadas como en
     a2/a4-guards): A5-1 tras V14 L2406 (fin de D.1), A5-2 tras el fin de A4-6 (fin de D.3); añadido puro con A5-1 sola, A5-2 sola y las dos;
     solo cambian D.1 o D.3 y sus ancestros; separabilidad (ninguna nombra a la otra); cláusula de alcance; sin filas de tabla, encabezados ni
     vallas; sin nombres de producto (salvo la etiqueta congelada «**Codex, total**»); frases exigidas y prohibidas; cifras de A5-2 iguales al
     literal de V14 L2442; la cita interna de A5-1 igual a V14 L2391.
  G5b autoprueba custodiada: Result PASS, ToolBlob = blob de a5-guards.py, A5TextBlob = blob de la A-5, todos los mutantes muertos.
Sin escrituras salvo --out y un directorio temporal propio (--tmp, por defecto junto a este archivo; se borra al terminar). Nunca escribe la ruta del
repositorio ni identidades del host.
"""
import argparse, copy, difflib, hashlib, importlib.util, json, os, re, shutil, subprocess, sys

# ---------------------------------------------------------------- identidades fijadas
PREP_BASE = "efcfcbc5a9f89de0dc4f4f21a426b4723ef18245"   # HEAD de la rama sobre la que se preparó A-5
BASE_RC = "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"     # base del mapa (C-20b): EFF^1 de la derivación
FREEZE_SHA = "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43"
V14_COMMIT = "4c617e82b32b6c810b68d75fc19472efed22b393"
V14 = "docs/initiatives/I-62-proposal-v14.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
A1, A2, A3, A4 = ("docs/initiatives/I-62-A-%d.md" % i for i in (1, 2, 3, 4))
A5 = "docs/initiatives/I-62-A-5.md"
PKG = "docs/initiatives/I-62-architect-package-A-5.md"
EVID = "docs/automation/evidence/I-62-A5/"
GUARDS, SELFTEST = EVID + "a5-guards.py", EVID + "a5-selftest.json"
EV, DEC = "docs/automation/evidence/I-62-evidence.md", "docs/automation/decisions/I-62.md"
STATE, F6README = "docs/automation/state/I-62.yml", "docs/automation/evidence/I-62-F6/README.md"
AP, WF, IL = "docs/AUTOMATION_PLAN.md", "docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md"
PT = "docs/initiatives/PROMPT_TEMPLATES.md"
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
MAP = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
SCHEMA = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
AE = "docs/automation/agent-execution"
CLAUSE_MAP = "docs/automation/evidence/I-62-F4/compat/clause_map.py"
MAPINV = "docs/automation/evidence/I-62-F6/MAP-INVALID"
MV_CHECK = MAPINV + "/mv_check.py"
U14 = "docs/automation/evidence/I-62-F6/FX-02/s62/u14-reconciliation.md"
B1DRAFT = "docs/automation/evidence/I-62-F6/requests/B1-fixture-request-draft-2026-10-11.md"
CHAR, PLAN, EFFECTS = MAPINV + "/characterization.md", MAPINV + "/reconstruction-plan.md", MAPINV + "/effects.md"
CFA = "docs/automation/evidence/I-62-F6/FX-02/s70/controller-failure-analysis.md"

FROZEN = {  # ruta -> blob o árbol fijado (base y head)
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f",
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", WF: "446c32ba6a91f9a712a0758038ae0a46ab1c970f",
    IL: "f19896a8f1a7c82f74bac7231636f68462a87271", PT: "384d0b0c3734ab7df881ad17fa740fdb8a6422d5",
    ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534", ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e",
    MAP: "4d49d3e1a63d2b6128c9d9eeac473156a421571a", SCHEMA: "6b54dda9579d694200d8fc9ec26a4a846eef529b",
    AE: "809fd58f450d3379a4fcc958cf8907fcdc5acff3", CLAUSE_MAP: "ffe6ea57257f30fa9eec3e9aaf5bf9982bf59d08",
    MV_CHECK: "9536d051372cb03643ac1392d85b3e8a563d35bc", MAPINV: "b0b3b2586a98883353dfda1c73b400355cbc51b0",
}
PROD = ["src", "tests", "eng", "tools", "deploy", "assets", "docs/adr", AE, AP, WF, IL, "docs/FOUNDATIONS.md", "AGENTS.md", "CLAUDE.md",
        "Directory.Build.props", "Directory.Build.targets", "RackCad.sln", "global.json", "NuGet.Config", "docs/automation/evidence/I-62-F4"]
SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md", "docs/adr/",
            "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
SOURCES = {"V14": (V14, FROZEN[V14]), "AP": (AP, FROZEN[AP]), "A2": (A2, FROZEN[A2]), "A4": (A4, FROZEN[A4])}
DEC_BLOB = "8c96375c6437b9b943422eb2f2dd79f4d34ff594"   # decisiones en efcfcbc5 (hasta §71); crecen solo por añadido
BQ = {"A2", "A4"}  # fuentes cuyas líneas citadas son de un bloque citado («> »): se comparan sin el marcador

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
    "Applies-to: I-62",
    "State:\nPROPUESTA.",
    "Architect review:\nNOT LAUNCHED.",
    "Amendment: A-5 (candidata;",
]
HEADER_FORBIDDEN = ["Coordinator verdict", "AGREED (decisiones §7", "Architect review:\nAGREED", "State:\nAGREED"]

# ---------------------------------------------------------------- literales citados: (fuente, línea inicial, línea final, texto exacto, referencia junto a la cita)
LIT = [
    ("V14", 2391, 2391, "copias **byte a byte** de las autoridades de RackCad en MC_I62 (WORKFLOW, LIFECYCLE, AUTOMATION_PLAN, agent-execution, esquemas, "
                        "`compatibility/`)", None),
    ("V14", 2395, 2395, "con el commit **F_norm**", "V14 L2395"),
    ("V14", 2406, 2406, "(F_eff). **Contrato válido** (A1'/A6').", "V14 L2406"),
    ("V14", 2442, 2442, "**Codex, total**", None),
    ("V14", 2523, 2523, "| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |", "V14 L2523"),
    ("V14", 2528, 2528, "Ningún tope autoriza gasto ni ejecución ahora.", "V14 L2528"),
    ("V14", 2361, 2361, "traza D.1 hasta un contrato I62 válido", "V14 L2361"),
    ("V14", 2544, 2544, "| FX-02 PASS | OD-5 + **OD-7** + OD-2 + medición del binario | UNVERIFIED; F6 pendiente |", "V14 L2544"),
    ("V14", 2558, 2558, "Se resiembra el fixture desde el Candidato y se ejecutan:", "V14 L2558"),
    ("V14", 974, 974, "autorización previa de apertura y consumo, no transporte", "V14 L974"),
    ("V14", 975, 975, "| OD-7 | remoto del fixture con CI | **cierre de F6** (FX-02 no puede ser PASS sin CI); FX-04b | antes de F6 |", "V14 L975"),
    ("AP", 731, 731, "**`I62_EFFECTIVE_SHA`**, nunca `MainSha`", "AP L731"),
    ("AP", 822, 822, "Cualquier fallo → MAP_INVALID → `Authority` fail; STOP (S-12; P-15).", "L822"),
    ("AP", 920, 920, "**Con MAP_INVALID, ningún contrato I61 pasa `Authority` tras EFF.**", "AP L920"),
    ("AP", 920, 921, "La corrección es un cambio normativo con su propia autoridad, nunca un parche local del resolver.", "AP L920-L921"),
    ("AP", 945, 946, "`I62_EFFECTIVE_SHA` es el primer merge en first-parent de `origin/main` cuyo segundo padre alcanza el **único** commit con el "
                     "trailer […] y cuyo primer padre no lo alcanza", "AP L945-L946"),
    ("A2", 148, 148, "Principal B de FX-04a, Principal de la Topología B y Principal A de D.8)", "A-2 L148"),
    ("A2", 156, 156, "su lanzamiento físico queda registrado y cuenta en el tope de su fila", "A-2 L156"),
    ("A2", 159, 159, "la aprobación de OD-5 (consumo dentro de los topes de D.3) no lo cubre", "A-2 L159"),
    ("A2", 170, 170, "Esta regla no se aplica a D.5 ni a sesiones que no sean de Principal.", "A-2 L170"),
    ("A2", 233, 233, "Las sondas de un bloque no consumen los lanzamientos de Codex de ninguna ronda", "A-2 L233"),
    ("A4", 203, 203, "Su tope es uno en total para FX-02 en F6", "A-4 L203"),
    ("A4", 351, 351, "titular A2 de FX-02, designado por T16", "A-4 L351"),
    ("A4", 355, 355, "ese tope y ese total suben en **una** sesión, de 2 a 3, solo para abrir la sesión de A2", "A-4 L355"),
    ("A4", 361, 361, "la regla de A-2 sobre un lanzamiento inválido rige sin cambio, también para A2.", "A-4 L361"),
    ("DEC", 1456, 1456, "no se declara conformidad con una interpretación informal contraria al texto congelado", "DEC L1456"),
    ("DEC", 1456, 1456, "Se autoriza la **PREPARACIÓN, no la aplicación,** de una enmienda material **A-5** con dos deltas separados", "DEC L1456"),
    ("DEC", 1458, 1458, "Ni repositorio B1, ni FX-U2, ni sonda 2. Ni edición de A-1..A-4, ni C-24 PASS ni F6 GATE PASS", "DEC L1458"),
    ("DEC", 1454, 1454, "La sonda 2 del bloque A2-P2 queda **HELD**", "DEC L1454"),
    ("DEC", 1265, 1265, "R no se reclasifica a posteriori y ningún presupuesto se reinicia", "DEC L1265"),
]
NOTATION = {"V14 L…", "AP L…", "DEC L…"}
OWN_TERMS = {"Principal FX-U2"}                 # términos que el propio delta introduce entre comillas
DELTA_INNER_LIT = {"copias **byte a byte** de las autoridades de RackCad en MC_I62 (WORKFLOW, LIFECYCLE, AUTOMATION_PLAN, agent-execution, esquemas, "
                   "`compatibility/`)", "**Codex, total**"}
SHORT_BLOBS = {"34ad80ea": V14, "0f6b8608": FREEZE, "c01899a7": A1, "f1e1d6f0": A2, "ea6721f7": A3, "5e2ba68e": A4, "f525cb1e": AP,
               "446c32ba": WF, "f19896a8": IL, "384d0b0c": PT, "da275e1a": ADR46, "e1bd8d91": ADR48, "4d49d3e1": MAP, "6b54dda9": SCHEMA,
               "809fd58f": AE, "ffe6ea57": CLAUSE_MAP, "9536d051": MV_CHECK, "b0b3b258": MAPINV, "05f62382": U14, "541d19e5": B1DRAFT,
               "1d74c2da": CHAR, "d0b2fd25": PLAN, "c817adc0": EFFECTS, "dc4bb758": CFA}
NON_GIT_HASHES = {"d13914ce"}                    # BinaryHash de codex-cli (SHA-256), no es un objeto Git
BLOCK_HEADER = re.compile(r"^(V14|AP) L(\d+)-L(\d+), literal", re.M)

# ---------------------------------------------------------------- delta (§3.2 y §3.3)
SEC_A51 = "### 3.2 A5-1 — "
SEC_A52 = "### 3.3 A5-2 — "
LOC_A51 = "Ubicación: al final de V14 D.1, a continuación del paso 6"
LOC_A52 = "Ubicación: al final de V14 D.3, a continuación del párrafo de A4-6"
ANCHOR_D1 = "   (F_eff). **Contrato válido** (A1'/A6')."     # V14 L2406, fin de D.1
ANCHOR_D1_LINE = 2406
ANCHOR_V14 = "Ningún tope autoriza gasto ni ejecución ahora."   # ancla de A-2 (A-2 §2.3)
ANCHOR_A2_END = "Ningún otro tope cambia."                      # fin de A2-P2, ancla de A-4
ANCHOR_A46_END = "también para A2."                             # fin de A4-6 (A-4 L361), ancla de A5-2
H_TITLE = "# I-62 — Proposal V14: portabilidad del Principal, binding de roles independiente del proveedor y orquestación autónoma de roles"
H_ANEXO_D = "## Anexo D — Pilotos y portabilidad (plano c)"
H_D1 = "### D.1 Arranque del fixture (análisis; nada se crea ahora)"
H_D2 = "### D.2 "
H_D3 = "### D.3 Hoja de invocaciones y escenarios"
H_D4 = "### D.4 "
CAPS = "| **Codex, total** | | **11** | | pool **4** (≤ 2 por fase) | **15** |"
CAPS_LINE = 2442
SCOPE = {"A5-1": "Solo para un repositorio de fixture nuevo de F6 (B1)", "A5-2": "Solo para la ronda A de F6 (Topología A)"}
REQUIRED = {
    "A5-1": ["`FileKind` MODIFIED, copia byte a byte de su `BaseBlob`", "ningún archivo que el mapa lista como ADDED o ENTRY, ni el propio mapa",
             "con el mismo blob que en F_norm", "es el único commit de la rama `fixture/i62-norm`, con F_seed como padre",
             "Es el único commit del repositorio con el trailer", "merge sin avance rápido", "con F_seed como primer padre",
             "solo por su segundo padre", "antes del paso 4", "`Validate(M)` pasa MV-1..MV-7", "antes de cualquier reclamo",
             "el repositorio no se usa", "FX-U2", "sin trasladar estado, custodia, bindings ni evidencia de FX-U1",
             "AUTOMATION_PLAN 16.13 y 16.14 y las demás autoridades normativas no cambian", "no se reescriben ni se sustituyen",
             "el MAP_INVALID de su activación se conserva", "no autoriza la creación del repositorio", "exige la autorización del Owner"],
    "A5-2": ["**1** sesión, sin reapertura", "tope **1**", "que sube en **una** sesión solo para esta fila",
             "no transfiere, no usa y no reasigna el cupo de la regla de A-4 para el titular A2", "sigue reservado a ese titular y sin usar",
             "ningún contador se reinicia ni se descuenta", "no cambian, no se reponen y continúan con lo ya consumido",
             "(**11** base, pool **4** con ≤ 2 por fase, tope **15**)", "Ningún otro tope ni total cambia", "P-07 comprueba antes de abrir la sesión",
             "la autorización de consumo del Owner", "aprobación de OD-5 (consumo dentro de los topes de D.3)", "La sesión no es fungible",
             "esta regla no abre otra", "no se extiende a esta fila"],
}
FORBIDDEN = {
    "A5-1": ["A5-2", "Principal FX-U2", "reapertura", "cupo", "se reasigna", "parche"],
    "A5-2": ["A5-1", "B1", "+1 reapertura", "se transfiere", "transfiere el cupo", "se reasigna", "de 2 a 3"],
}
PRODUCT_TOKENS = re.compile(r"(?i)\b(codex|claude|openai|anthropic|gpt-|opus|sonnet|haiku|chatgpt|gemini|google|copilot|mistral|llama|deepseek|grok)")
PRODUCT_EXEMPT = {"A5-2": ["**Codex, total**"]}

ALLOWED = {A5: {"A"}, PKG: {"A"}, EV: {"M"}, DEC: {"M"}, STATE: {"M"}, F6README: {"M"}}
APPEND_ONLY = (EV, DEC)

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


def blob_id(raw):
    return hashlib.sha1(b"blob %d\0" % len(raw) + raw).hexdigest()


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
    """Texto con los bloques ``` sustituidos por espacios (conserva las posiciones)."""
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


def a5_rules(md):
    out = {}
    for rid, head in (("A5-1", SEC_A51), ("A5-2", SEC_A52)):
        s, e = section(md, head)
        if s is None:
            continue
        body = md[s:e]
        loc = next((l for l in body.split("\n") if l.startswith("Ubicación:")), "")
        bq = [l for l in body.split("\n") if l.startswith(">")]
        out[rid] = {"Paragraph": quoted_paragraph("\n".join(bq)), "Location": loc, "Start": s, "End": e,
                    "BlockSpan": (md.find(bq[0], s), md.find(bq[-1], s) + len(bq[-1])) if bq else (s, s)}
    return out


def a2_section(md, title):
    m = re.search(r"^### %s\n(.*?)(?=^### |^## )" % re.escape(title), md, re.S | re.M)
    return m.group(1) if m else ""


def a4_paragraphs(a4):
    paras = []
    for m in re.finditer(r"^### 3\.(\d) (A4-\d) — [^\n]*\n(.*?)(?=^### |^## )", a4, re.S | re.M):
        paras.append((m.group(2), quoted_paragraph("\n".join(l for l in m.group(3).split("\n") if l.startswith(">")))))
    return paras


def virtual_base(v14, a2, a4):
    """V14 + A-2 + A-4 aplicadas como en a2-guards.py y a4-guards.py (añadidos al final de D.3)."""
    p1 = quoted_paragraph("\n".join(l for l in a2_section(a2, "2.3 Delta exacto").split("\n") if l.startswith(">")))
    p2 = quoted_paragraph("\n".join(l for l in a2_section(a2, "3.3 Delta exacto").split("\n") if l.startswith(">")))
    t = v14.replace(ANCHOR_V14, ANCHOR_V14 + "\n\n" + p1 + "\n\n" + p2, 1)
    paras = a4_paragraphs(a4)
    t = t.replace(ANCHOR_A2_END, ANCHOR_A2_END + "\n\n" + "\n\n".join(p for _, p in paras), 1)
    return t, [r for r, _ in paras]


class Git:
    def __init__(self, repo):
        self.repo = repo

    def __call__(self, *a, check=True):
        r = subprocess.run(["git", "-C", self.repo] + list(a), capture_output=True)
        if check and r.returncode != 0:
            raise RuntimeError("git %s: código %d" % (a[0], r.returncode))
        return r.stdout.decode("utf-8") if r.returncode == 0 else None

    def show(self, rev, path):
        out = self("show", "%s:%s" % (rev, path), check=False)
        return out.replace("\r\n", "\n") if out is not None else None

    def cat(self, obj):
        return self("cat-file", "-p", obj).replace("\r\n", "\n")

    def obj(self, rev, path):
        out = self("rev-parse", "-q", "--verify", "%s:%s" % (rev, path), check=False)
        return out.strip() if out else None


def load_clause_map(git, td):
    path = os.path.join(td, "clause_map_pinned.py")
    with open(path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(git.cat(FROZEN[CLAUSE_MAP]))
    spec = importlib.util.spec_from_file_location("clause_map_pinned", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod, path


# ---------------------------------------------------------------- recogida de entradas (única parte que lee el repositorio)
def collect_common(git, base, head):
    inp = {"Base": base, "Head": head}
    inp["Sources"] = {t: git.cat(b) for t, (p, b) in SOURCES.items()}
    inp["Sources"]["DEC"] = git.show(base, DEC) or ""
    inp["DecPinned"] = git.cat(DEC_BLOB)
    inp["ShortAtBase"] = {pre: git.obj(base, p) for pre, p in SHORT_BLOBS.items()}
    paths = sorted(set(FROZEN) | set(PROD))
    inp["Objs"] = {"Base": {p: git.obj(base, p) for p in paths}, "Head": {p: git.obj(head, p) for p in paths}}
    inp["FreezeCommit"] = {"V14": git.obj(FREEZE_SHA, V14), "Record": git.obj(FREEZE_SHA, FREEZE), "V14AtCommit": git.obj(V14_COMMIT, V14)}
    return inp


def run_c20b(git, cm_path, head, td):
    r = subprocess.run([sys.executable, "-I", "-B", cm_path, "check", BASE_RC, head, os.path.join(td, "c20b.json")], cwd=git.repo, capture_output=True)
    anc = git("merge-base", "--is-ancestor", BASE_RC, head, check=False) is not None
    return {"Base": BASE_RC, "BaseIsAncestor": anc, "ExitCode": r.returncode, "Output": (r.stdout + r.stderr).decode("utf-8", "replace").strip()[-400:]}


def collect_commit(git, base, head, inp):
    rows = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", base, head).splitlines() if l]
    inp["Status"] = {r[-1]: r[0] for r in rows}
    inp["HeadParent"] = git("rev-parse", head + "^1").strip()
    inp["HeadTexts"] = {p: git.show(head, p) for p, s in inp["Status"].items() if s in ("A", "M")}
    inp["BaseTexts"] = {p: git.show(base, p) for p, s in inp["Status"].items() if s == "M"}
    inp["HeadObj"] = {p: git.obj(head, p) for p in (A5, GUARDS, SELFTEST, MAP)}


# ---------------------------------------------------------------- G1
def g1_literals(inp, ids):
    f, info, checked = [], [], {"Header": 0, "Blocks": 0, "BlockLines": 0, "Lit": 0, "Self": 0, "Notation": 0, "DeltaInner": 0, "ShortBlobs": 0}
    md = inp["A5"] or ""
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
    # bloques literales de §2
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
    # citas
    rules = a5_rules(md)
    deltas = {r: v["Paragraph"] for r, v in rules.items()}
    dspans = [v["BlockSpan"] for v in rules.values()]
    qs = quotes(masked)
    if qs is None:
        f.append("comillas angulares desequilibradas en A-5")
        qs = []
    lit_by_text = {}
    for row in LIT:
        lit_by_text.setdefault(norm(row[3]), []).append(row)
    unclassified = []
    for s, e, depth in qs:
        if any(a <= s < b for a, b in block_spans):
            continue
        inner = md[s + 1:e]
        in_delta = any(a <= s < b for a, b in dspans)
        if in_delta:
            if depth == 0:
                continue                      # el párrafo normativo mismo
            key = norm(inner.replace("\n> ", "\n").replace("\n>", "\n"))
            if key in {norm(x) for x in DELTA_INNER_LIT} | OWN_TERMS:
                checked["DeltaInner"] += 1
                for row in lit_by_text.get(key, []):
                    tag, a, b, text, _ = row
                    if not literal_match(text, [l[2:] if tag in BQ and l.startswith("> ") else l for l in src_lines[tag][a - 1:b]]):
                        f.append("cita interna del delta distinta de %s L%d: «%s»" % (tag, a, text[:50]))
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
        f.append("citas sin clasificar (ni literal fijado, ni del propio delta, ni notación): %s" % unclassified[:6])
    # prefijos de blob abreviados
    for m in re.finditer(r"([0-9a-f]{8})…", masked):
        pre = m.group(1)
        checked["ShortBlobs"] += 1
        if pre in NON_GIT_HASHES:
            continue
        if pre not in SHORT_BLOBS:
            f.append("prefijo de blob sin fijar en las guardas: %s…" % pre)
        elif not (inp["ShortAtBase"].get(pre) or "").startswith(pre):
            f.append("prefijo %s… distinto del objeto real de %s en la base" % (pre, SHORT_BLOBS[pre]))
    # privacidad
    priv = ["A-5"]
    f += privacy_findings("A-5", md, ids)
    for name, text in sorted((inp.get("PkgTexts") or {}).items()):
        priv.append(name)
        f += privacy_findings(name, text, ids)
    return {"Check": "G1 literales, cabecera y privacidad", "Checked": checked, "PrivacyChecked": len(priv), "Info": info, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G2
def g2_paths(inp, ids):
    f, info = [], []
    st = inp.get("Status")
    if st is None:
        return {"Check": "G2 rutas del commit", "Result": "NOT_RUN", "Info": ["sin commit candidato"], "Findings": []}
    if inp.get("HeadParent") != inp["Base"]:
        f.append("base distinta del padre de head")
    if st.get(A5) != "A":
        f.append("%s no aparece como añadido (A)" % A5)
    if inp.get("HeadObj", {}).get(A5) != blob_id((inp["A5"] or "").encode("utf-8")):
        f.append("blob de A-5 en head distinto del texto comprobado")
    for p, s in sorted(st.items()):
        if p.startswith(EVID):
            if s != "A" and not (s == "M" and inp.get("Mode") == "RUN_CORRECTION"):
                f.append("%s con estado %s (en %s solo se añade)" % (p, s, EVID))
        elif p not in ALLOWED:
            f.append("ruta fuera de alcance: %s (%s)" % (p, s))
        elif s not in ALLOWED[p]:
            f.append("%s con estado %s (permitido: %s)" % (p, s, sorted(ALLOWED[p])))
    grown = {}
    for p in APPEND_ONLY:
        if p in st:
            new, old = inp["HeadTexts"].get(p) or "", inp["BaseTexts"].get(p) or ""
            if st[p] != "M" or not new.startswith(old):
                f.append("%s no cambia solo por añadido" % p)
            else:
                grown[p] = len(new.encode("utf-8")) - len(old.encode("utf-8"))
    for p, s in sorted(st.items()):
        if s not in ("A", "M"):
            continue
        text = inp["HeadTexts"].get(p) or ""
        if p in APPEND_ONLY and s == "M":
            old = inp["BaseTexts"].get(p) or ""
            text = text[len(old):] if text.startswith(old) else text
        f += privacy_findings(p, text, ids)
    return {"Check": "G2 rutas del commit", "Base": inp["Base"], "Head": inp["Head"], "Changed": st, "AppendOnlyGrowth": grown, "Info": info,
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G3
def g3_frozen(inp):
    f, mism = [], {}
    for lab in ("Base", "Head"):
        got = inp["Objs"][lab]
        for p, w in sorted(FROZEN.items()):
            if got.get(p) != w:
                mism["%s:%s" % (lab, p)] = {"Expected": w, "Got": got.get(p)}
    for p in PROD:
        if inp["Objs"]["Base"].get(p) != inp["Objs"]["Head"].get(p):
            mism["Base->Head:" + p] = {"Expected": inp["Objs"]["Base"].get(p), "Got": inp["Objs"]["Head"].get(p)}
    f = ["objeto distinto en %s: esperado %s, obtenido %s" % (k, (v["Expected"] or "")[:12], (v["Got"] or "")[:12]) for k, v in sorted(mism.items())]
    return {"Check": "G3 objetos congelados y producción sin cambio", "Frozen": len(FROZEN), "Production": len(PROD), "Mismatches": mism,
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G4
def in_surfaces(p):
    return any(p == s or (s.endswith("/") and p.startswith(s)) for s in SURFACES)


def g4_c20b(inp):
    f = []
    touched = sorted(p for p in (inp.get("Status") or {}) if in_surfaces(p))
    if touched:
        f.append("rutas cambiadas bajo las superficies del mapa: %s" % touched)
    if inp["Objs"]["Head"].get(MAP) != FROZEN[MAP]:
        f.append("mapa de cláusulas distinto de 4d49d3e1 en head")
    r = inp["C20b"]
    if not r.get("BaseIsAncestor"):
        f.append("la base del mapa bb0d5522 no es ancestro de head")
    if not (r["ExitCode"] == 0 and "EQUAL" in r["Output"]):
        f.append("clause_map.py check distinto de EQUAL (código %s)" % r["ExitCode"])
    return {"Check": "G4 C-20b", "Base": r["Base"], "SurfacesTouched": touched, "ExitCode": r["ExitCode"], "Output": r["Output"][-200:],
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G5
def g5_delta(inp, cm):
    f, info = [], []
    md = inp["A5"] or ""
    rules = a5_rules(md)
    if sorted(rules) != ["A5-1", "A5-2"]:
        f.append("§3.2 y §3.3 no contienen exactamente A5-1 y A5-2: %s" % sorted(rules))
        return {"Check": "G5 delta", "Findings": f, "Result": "FAIL"}
    p1, p2 = rules["A5-1"]["Paragraph"], rules["A5-2"]["Paragraph"]
    if not rules["A5-1"]["Location"].startswith(LOC_A51) or "V14 L2406" not in rules["A5-1"]["Location"] + md[rules["A5-1"]["Start"]:rules["A5-1"]["Start"] + 600]:
        f.append("A5-1: ubicación distinta de «%s» (V14 L2406)" % LOC_A51)
    if not rules["A5-2"]["Location"].startswith(LOC_A52):
        f.append("A5-2: ubicación distinta de «%s»" % LOC_A52)
    v14, a2, a4 = inp["Sources"]["V14"], inp["Sources"]["A2"], inp["Sources"]["A4"]
    vl = v14.split("\n")
    if vl[ANCHOR_D1_LINE - 1] != ANCHOR_D1 or v14.count(ANCHOR_D1) != 1:
        f.append("ancla de A5-1 (V14 L2406) ausente, repetida o desplazada")
    if not (v14.index(H_D1) < v14.find(ANCHOR_D1) < v14.index(H_D2)) or v14[v14.find(ANCHOR_D1) + len(ANCHOR_D1):v14.index(H_D2)].strip():
        f.append("el ancla de A5-1 no es el final de D.1")
    if vl[CAPS_LINE - 1] != CAPS:
        f.append("el literal de topes de V14 L2442 no es el fijado")
    base, a4ids = virtual_base(v14, a2, a4)
    if a4ids != ["A4-1", "A4-2", "A4-3", "A4-4", "A4-5", "A4-6"]:
        f.append("A-4 no aporta exactamente A4-1..A4-6: %s" % a4ids)
    if base.count(ANCHOR_A46_END) != 1:
        f.append("«%s» aparece %d veces en V14 + A-2 + A-4 (ancla de A5-2 no única)" % (ANCHOR_A46_END, base.count(ANCHOR_A46_END)))
    else:
        pos = base.find(ANCHOR_A46_END) + len(ANCHOR_A46_END)
        if base[pos:base.index(H_D4)].strip() or not (base.index(H_D3) < pos < base.index(H_D4)):
            f.append("A4-6 no es el final de D.3 en V14 + A-2 + A-4")
    anchor_d1 = ANCHOR_D1 + "\n"
    allowed = {"A5-1": {(1, norm(H_TITLE)), (2, norm(H_ANEXO_D)), (3, norm(H_D1))}, "A5-2": {(1, norm(H_TITLE)), (2, norm(H_ANEXO_D)), (3, norm(H_D3))}}
    must = {"A5-1": (3, norm(H_D1)), "A5-2": (3, norm(H_D3))}
    secs = lambda t: {(lv, h): s for lv, h, s in cm.sections(t)}
    sb = secs(base)
    for name, use in (("A5-1", ["A5-1"]), ("A5-2", ["A5-2"]), ("A5-1 + A5-2", ["A5-1", "A5-2"])):
        applied, ins = base, []
        if "A5-1" in use:
            applied = applied.replace(anchor_d1, anchor_d1 + "\n" + p1 + "\n", 1)
            ins.append(("\n" + p1 + "\n", anchor_d1))
        if "A5-2" in use:
            applied = applied.replace(ANCHOR_A46_END, ANCHOR_A46_END + "\n\n" + p2, 1)
            ins.append(("\n\n" + p2, ANCHOR_A46_END))
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
        want = set().union(*(allowed[u] for u in use))
        if chg - want:
            f.append("%s: cambian secciones fuera de las declaradas: %s" % (name, sorted(chg - want)))
        for u in use:
            if must[u] not in chg:
                f.append("%s: no cambia %s" % (name, must[u][1][:30]))
        al = applied.split("\n")
        caps_idx = [i for i, l in enumerate(al) if l == CAPS]
        if len(caps_idx) != 1:
            f.append("%s: el literal de topes de «Codex, total» cambia o se repite" % name)
    for rid, para in (("A5-1", p1), ("A5-2", p2)):
        body = para.split("\n")
        if [l for l in body if re.match(r"^\s*(\||#|```)", l)]:
            f.append("%s: el delta trae filas de tabla, encabezados o vallas" % rid)
        first = re.sub(r"^\*\*[^*]+\*\*\s*", "", norm(para))
        if not first.startswith(SCOPE[rid]):
            f.append("%s: no abre con su cláusula de alcance «%s»" % (rid, SCOPE[rid]))
        for ph in REQUIRED[rid]:
            if norm(ph) not in norm(para):
                f.append("%s: falta la frase exigida «%s»" % (rid, ph[:60]))
        for ph in FORBIDDEN[rid]:
            if ph in para:
                f.append("%s: contiene «%s» (prohibido: alcance o separabilidad)" % (rid, ph))
        scrub = para
        for ex in PRODUCT_EXEMPT.get(rid, []):
            scrub = scrub.replace(ex, "")
        hits = sorted(set(m.group(0).lower() for m in PRODUCT_TOKENS.finditer(scrub)))
        if hits:
            f.append("%s: nombres de producto en el delta: %s" % (rid, hits))
        if re.search(r"(?<!ningún contador )se reinicia", para):
            f.append("%s: «se reinicia» fuera de «ningún contador se reinicia»" % rid)
    m = re.search(r"\(\*\*(\d+)\*\* base, pool \*\*(\d+)\*\* con ≤ (\d+) por fase, tope \*\*(\d+)\*\*\)", p2)
    cm_caps = re.search(r"\*\*(\d+)\*\* \| \| pool \*\*(\d+)\*\* \(≤ (\d+) por fase\) \| \*\*(\d+)\*\*", CAPS)
    if not m or m.groups() != cm_caps.groups():
        f.append("A5-2: las cifras de la fila «Codex, total» no son las de V14 L2442 (%s)" % (str(m.groups()) if m else "sin cifras",))
    inner = re.search(r"«(copias \*\*byte a byte\*\*[^»]*)»", p1)
    if not inner or norm("- " + inner.group(1) + ";") != norm(vl[2391 - 1]):
        f.append("A5-1: la viñeta citada no es V14 L2391 exacta")
    if "la aprobación de OD-5 (consumo dentro de los topes de D.3) no lo cubre" not in a2:
        f.append("A-2 ya no contiene la fórmula de OD-5 que A5-2 reutiliza")
    info.append("A5-1 tras V14 L2406 (fin de D.1) y A5-2 tras el fin de A4-6 (fin de D.3): añadidos puros, por separado y juntas")
    return {"Check": "G5 pureza del delta y separabilidad", "Paragraphs": {"A5-1": len(p1), "A5-2": len(p2)}, "Info": info, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G5b
def g5b_selftest(inp):
    f = []
    st = inp.get("SelfTest")
    if not st:
        return {"Check": "G5b autoprueba custodiada", "Findings": ["sin a5-selftest.json"], "Result": "FAIL"}
    if st.get("Result") != "PASS":
        f.append("a5-selftest.json con Result %s" % st.get("Result"))
    if st.get("ToolBlob") != inp.get("ToolBlob"):
        f.append("ToolBlob de la autoprueba distinto del blob de a5-guards.py")
    if st.get("A5TextBlob") != blob_id((inp["A5"] or "").encode("utf-8")):
        f.append("A5TextBlob de la autoprueba distinto del blob de la A-5 comprobada")
    s = st.get("Summary") or {}
    if not s.get("Mutants") or s.get("Killed") != s.get("Mutants") or s.get("ControlFailures"):
        f.append("autoprueba con mutantes vivos o controles fallidos: %s" % s)
    return {"Check": "G5b autoprueba custodiada", "Summary": s, "Findings": f, "Result": "PASS" if not f else "FAIL"}


GUARDS_FN = {"G1": lambda inp, cm, ids: g1_literals(inp, ids), "G2": lambda inp, cm, ids: g2_paths(inp, ids), "G3": lambda inp, cm, ids: g3_frozen(inp),
             "G4": lambda inp, cm, ids: g4_c20b(inp), "G5": lambda inp, cm, ids: g5_delta(inp, cm), "G5B": lambda inp, cm, ids: g5b_selftest(inp)}


# ---------------------------------------------------------------- autoprueba por mutantes
SYNTH_ID = "zq-host-" + "user-7b2e"
SYNTH_WIN_PATH = "C:" + "\\" + "Us" + "ers" + "\\" + "someone" + "\\" + "x"
SYNTH_MAIL = "a.b" + "@" + "example" + ".com"


def m_text(old, new, count=1):
    def f(inp):
        assert old in inp["A5"], "mutante no aplicable: %s" % old[:40]
        inp["A5"] = inp["A5"].replace(old, new, count)
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
        if status == "M":
            inp["BaseTexts"][path] = "base\n"
    return f


MUTANTS = [
    # G1
    ("G1", "literal cambiado en un carácter (V14 L2523)", m_text("«| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |»", "«| A | ≤ 16 + 2 sondas | ≤ 4 | 0 | ≤ 2 |»")),
    ("G1", "referencia de línea cambiada (A-2 L159)", m_text("regla 4, A-2 L159:", "regla 4, A-2 L160:")),
    ("G1", "FREEZE_SHA cambiado", m_text("FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43", "FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd44")),
    ("G1", "blob de A-4 de la cabecera cambiado", m_text("blob 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f)", "blob 5e2ba68efdc1b902aa1c28aff4453b4b6973be70)")),
    ("G1", "veredicto del Coordinator añadido", m_text("Owner decision:\n", "Coordinator verdict:\nAGREED\n\nOwner decision:\n")),
    ("G1", "estado distinto de PROPUESTA", m_text("State:\nPROPUESTA.", "State:\nAPLICADA.")),
    ("G1", "cita sin clasificar", m_text("## 1. Alcance\n", "## 1. Alcance\n\nUna cita «texto que no existe en ninguna fuente».\n")),
    ("G1", "línea de un bloque literal de §2 cambiada", m_text(">    - una biblioteca .NET mínima y su CI;", ">    - una biblioteca .NET mínima y sus CI;")),
    ("G1", "prefijo de blob abreviado falso", m_text("AUTOMATION_PLAN (`f525cb1e…`", "AUTOMATION_PLAN (`f525cb1f…`")),
    ("G1", "ruta de perfil de usuario", m_text("## 9. Evidencia\n", "## 9. Evidencia\n\nRuta " + SYNTH_WIN_PATH + "\n")),
    ("G1", "correo", m_text("## 9. Evidencia\n", "## 9. Evidencia\n\nContacto " + SYNTH_MAIL + "\n")),
    ("G1", "identidad del host", m_text("## 9. Evidencia\n", "## 9. Evidencia\n\nSesión de " + SYNTH_ID + "\n")),
    ("G1", "cita interna del delta alterada", m_text("`compatibility/`)») no rige", "`compat/`)») no rige")),
    # G2
    ("G2", "ruta de producción en el commit", m_status("src/RackCad.Core/Nuevo.cs", "A")),
    ("G2", "V14 modificada en el commit", m_status(V14, "M")),
    ("G2", "A-5 como modificada (M)", m_set(["Status", A5], "M")),
    ("G2", "evidencia reescrita (no solo añadido)", lambda inp: (inp["Status"].__setitem__(EV, "M"), inp["BaseTexts"].__setitem__(EV, "antes\n"),
                                                               inp["HeadTexts"].__setitem__(EV, "otro\n"))),
    ("G2", "ruta fuera de I-62-A5 (kit de FX-02)", m_status("docs/automation/evidence/I-62-F6/kits/FX-02/x.md", "A")),
    ("G2", "archivo de I-62-A5 modificado en lugar de añadido", m_status(EVID + "preflight.md", "M")),
    ("G2", "privacidad en un archivo añadido", m_status(EVID + "nota.md", "A", "ruta " + SYNTH_WIN_PATH + "\n")),
    ("G2", "blob de A-5 en head distinto", m_set(["HeadObj", A5], "0" * 40)),
    # G3
    ("G3", "V14 cambiada en head", m_set(["Objs", "Head", V14], "1" * 40)),
    ("G3", "A-4 cambiada en head", m_set(["Objs", "Head", A4], "2" * 40)),
    ("G3", "árbol agent-execution cambiado", m_set(["Objs", "Head", AE], "3" * 40)),
    ("G3", "mapa cambiado en base", m_set(["Objs", "Base", MAP], "4" * 40)),
    ("G3", "mv_check.py cambiado", m_set(["Objs", "Head", MV_CHECK], "5" * 40)),
    ("G3", "árbol src distinto entre base y head", m_set(["Objs", "Head", "src"], "6" * 40)),
    ("G3", "ADR-0048 cambiado", m_set(["Objs", "Head", ADR48], "7" * 40)),
    # G4
    ("G4", "ruta de superficie en el commit", m_status(WF, "M")),
    ("G4", "C-20b distinto de EQUAL", m_set(["C20b", "Output"], '{"result": ["MV-3"]}')),
    ("G4", "C-20b con código 1", m_set(["C20b", "ExitCode"], 1)),
    # G5
    ("G5", "ubicación de A5-1 en D.2", m_text("Ubicación: al final de V14 D.1, a continuación del paso 6", "Ubicación: al principio de V14 D.2, a continuación del paso 6")),
    ("G5", "ubicación de A5-2 en D.4", m_text("Ubicación: al final de V14 D.3, a continuación del párrafo de A4-6", "Ubicación: al final de V14 D.4, a continuación del párrafo de A4-6")),
    ("G5", "A5-2 nombra A5-1 (separabilidad)", m_text("y ejecuta en ella FX-02:", "y ejecuta en ella FX-02 (A5-1):")),
    ("G5", "A5-1 nombra A5-2 (separabilidad)", m_text("y la creación del\n>    repositorio exige", "y, como A5-2, la creación del\n>    repositorio exige")),
    ("G5", "tope de Codex distinto en A5-2", m_text("tope **15**)", "tope **16**)")),
    ("G5", "fila de tabla en el delta", m_text(">    repositorio exige la autorización del Owner.»", ">    repositorio exige la autorización del Owner.\n> | fila | nueva |»")),
    ("G5", "nombre de producto en A5-1", m_text("con su remoto bare\n> local", "con su remoto bare de codex-cli\n> local")),
    ("G5", "A5-2 sin cláusula de alcance", m_text("Solo para la ronda A de F6 (Topología A) y solo", "Para la ronda A de F6 (Topología A) y solo")),
    ("G5", "A5-2 transfiere el cupo de A4-6", m_text("no transfiere, no usa y no reasigna el cupo", "transfiere el cupo")),
    ("G5", "A5-2 con reapertura", m_text("**1** sesión, sin reapertura,", "**1** sesión, +1 reapertura,")),
    ("G5", "A5-2 sin «ningún contador se reinicia»", m_text("ningún contador se reinicia ni se descuenta", "los contadores se reinician")),
    ("G5", "A5-1 sin la precondición antes del reclamo", m_text("registra en la evidencia real antes de cualquier reclamo", "registra en la evidencia real después del reclamo")),
    ("G5", "A5-1 permite dos commits en la rama", m_text("es el único commit de la rama `fixture/i62-norm`", "es el último commit de la rama `fixture/i62-norm`")),
    ("G5", "A5-1 con F_seed en el primer padre cambiado", m_text("con F_seed como primer padre", "con F_norm como primer padre")),
    ("G5", "A5-2 con «de 2 a 3»", m_text("que sube en **una** sesión solo para esta fila", "que sube en **una** sesión solo para esta fila, de 2 a 3")),
    # G5b
    ("G5B", "ToolBlob de la autoprueba distinto", m_set(["SelfTest", "ToolBlob"], "8" * 40)),
    ("G5B", "autoprueba con Result FAIL", m_set(["SelfTest", "Result"], "FAIL")),
]


def run_guard(gid, inp, cm, ids):
    try:
        return GUARDS_FN[gid](inp, cm, ids)
    except Exception as e:  # una excepción en una guarda cuenta como FAIL con hallazgo
        return {"Check": gid, "Findings": ["excepción: %s" % e], "Result": "FAIL"}


def self_test(inp, cm, ids):
    base = copy.deepcopy(inp)
    base["Status"] = {A5: "A", EVID + "a5-guards.py": "A", EVID + "preflight.md": "A"}
    base["HeadTexts"] = {A5: base["A5"], EVID + "a5-guards.py": "# guardas\n", EVID + "preflight.md": "# preflight\n"}
    base["BaseTexts"] = {}
    base["HeadParent"] = base["Base"]
    base["HeadObj"] = {A5: blob_id(base["A5"].encode("utf-8"))}
    base["Objs"] = {"Base": dict(base["Objs"]["Base"]), "Head": dict(base["Objs"]["Base"])}
    base["SelfTest"] = {"Result": "PASS", "ToolBlob": base["ToolBlob"], "A5TextBlob": blob_id(base["A5"].encode("utf-8")),
                        "Summary": {"Mutants": 1, "Killed": 1, "ControlFailures": []}}
    control = {g: run_guard(g, copy.deepcopy(base), cm, ids) for g in GUARDS_FN}
    ctrl_fail = [g for g, r in control.items() if r["Result"] != "PASS"]
    rows = []
    for gid, name, mut in MUTANTS:
        m = copy.deepcopy(base)
        m_ids = list(ids) + ([SYNTH_ID] if "identidad del host" in name else [])
        try:
            mut(m)
            applied = True
        except AssertionError as e:
            applied = False
        r = run_guard(gid, m, cm, m_ids) if applied else {"Result": "NOT_APPLIED", "Findings": []}
        new = [x for x in r.get("Findings", []) if x not in control[gid].get("Findings", [])]
        killed = applied and r["Result"] == "FAIL" and bool(new)
        rows.append({"Guard": gid, "Mutant": name, "Applied": applied, "Result": r["Result"], "Killed": killed, "NewFinding": new[:1]})
    summary = {"Mutants": len(rows), "Killed": sum(x["Killed"] for x in rows), "ControlFailures": ctrl_fail,
               "ByGuard": {g: "%d/%d" % (sum(x["Killed"] for x in rows if x["Guard"] == g), sum(1 for x in rows if x["Guard"] == g)) for g in GUARDS_FN}}
    return {"Control": {g: {"Result": r["Result"], "Findings": r.get("Findings", [])} for g, r in control.items()}, "Mutants": rows, "Summary": summary,
            "Result": "PASS" if not ctrl_fail and summary["Killed"] == summary["Mutants"] else "FAIL"}


# ---------------------------------------------------------------- principal
def read_bytes(path):
    with open(path, "rb") as fh:
        return fh.read()


def pkg_texts(pkgdir):
    out = {}
    for root, dirs, files in os.walk(pkgdir):
        dirs[:] = [d for d in dirs if not d.startswith("tmp")]
        for n in files:
            if n.endswith((".md", ".py", ".json", ".txt")):
                p = os.path.join(root, n)
                out[os.path.relpath(p, pkgdir).replace("\\", "/")] = read_bytes(p).decode("utf-8", "replace")
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
    ap.add_argument("--a5")
    ap.add_argument("--pkgdir")
    ap.add_argument("--cand-repo")
    ap.add_argument("--cand-head")
    ap.add_argument("--base")
    ap.add_argument("--head")
    ap.add_argument("--out", required=True)
    ap.add_argument("--tmp")
    a = ap.parse_args()
    here = os.path.dirname(os.path.abspath(__file__))
    td = a.tmp or os.path.join(here, "tmp-guards")
    os.makedirs(td, exist_ok=True)
    try:
        git = Git(a.repo)
        ids = host_identities(a.repo)
        cm, cm_path = load_clause_map(git, td)
        tool_blob = blob_id(read_bytes(os.path.abspath(__file__)))
        if a.mode == "self-test":
            head = git("rev-parse", "HEAD").strip()
            inp = collect_common(git, head, head)
            inp.update({"Mode": "SELF_TEST", "A5": read_bytes(a.a5).decode("utf-8"), "ToolBlob": tool_blob,
                        "C20b": {"Base": BASE_RC, "BaseIsAncestor": True, "ExitCode": 0, "Output": '{"result": "EQUAL (MV-2..MV-6)"}'}})
            res = self_test(inp, cm, ids)
            res.update({"Mode": "SELF_TEST", "Repo": "HEAD " + head, "ToolBlob": tool_blob, "A5TextBlob": blob_id(inp["A5"].encode("utf-8"))})
        elif a.mode == "preview":
            head = git("rev-parse", "HEAD").strip()
            a5 = read_bytes(a.a5).decode("utf-8")
            if a.cand_repo and a.cand_head:
                cg = Git(a.cand_repo)
                base = cg("rev-parse", a.cand_head + "^1").strip()
                inp = collect_common(cg, base, a.cand_head)
                collect_commit(cg, base, a.cand_head, inp)
                inp["C20b"] = run_c20b(cg, cm_path, a.cand_head, td)
                inp["Candidate"] = {"Base": base, "Head": a.cand_head, "Note": "commit hipotético construido solo en un clon temporal compartido"}
            else:
                inp = collect_common(git, head, head)
                inp["C20b"] = run_c20b(git, cm_path, head, td)
            inp.update({"Mode": "PREVIEW", "A5": a5, "ToolBlob": tool_blob, "PkgTexts": pkg_texts(a.pkgdir) if a.pkgdir else {}})
            stp = os.path.join(a.pkgdir or here, "a5-selftest.json")
            inp["SelfTest"] = json.load(open(stp, encoding="utf-8")) if os.path.exists(stp) else None
            res = {"Mode": "PREVIEW", "RepoHead": head, "ToolBlob": tool_blob, "A5TextBlob": blob_id(a5.encode("utf-8")),
                   "Guards": {g: run_guard(g, inp, cm, ids) for g in GUARDS_FN}, "Candidate": inp.get("Candidate")}
            res["Result"] = "PASS" if all(r["Result"] in ("PASS", "NOT_RUN") for r in res["Guards"].values()) else "FAIL"
        else:
            base, head = git("rev-parse", a.base).strip(), git("rev-parse", a.head).strip()
            inp = collect_common(git, base, head)
            collect_commit(git, base, head, inp)
            inp["C20b"] = run_c20b(git, cm_path, head, td)
            inp.update({"Mode": "RUN", "A5": git.show(head, A5) or "", "ToolBlob": git.obj(head, GUARDS),
                        "SelfTest": json.loads(git.show(head, SELFTEST) or "null")})
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
