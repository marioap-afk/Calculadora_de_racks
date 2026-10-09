#!/usr/bin/env python3
"""Guardas mecánicas de la enmienda candidata I-62 A-4 (recuperación de FX-02 ante F6-OBS-03; huecos H-1..H-3).

Autoridad: decisiones §61, punto 2 («completar las guardas mecánicas, comparar el delta exacto con V14 y A-2, y comprobar que las cinco
reglas propuestas no alteran superficies fuera de alcance»).

Uso (siempre con `python -I -B`; solo lectura del repositorio; sin red; nunca ejecuta runtimes de IA):
  python -I -B a4-guards.py self-test --repo <worktree> --out <json> [--a4 <A-4 corregida> --pkg <paquete corregido>] [--tmp <dir>]
      Autocomprobación por mutantes, en dos escenarios con entradas reales leídas del repositorio: P, el commit de publicación (A4_BASE..A4_PUB);
      C, la corrección previa a la revisión (A-4 y paquete corregidos, de --a4/--pkg o, si no se dan, de la HEAD del repositorio cuando ya los
      contiene), simulada sobre la publicación. Cada guarda se ejecuta sobre la entrada real y sobre copias mutadas en memoria; cada mutante
      debe hacer FALLAR su guarda con un hallazgo nuevo del motivo esperado (p. ej., un literal cambiado en un carácter hace fallar G1; una
      ruta fuera de alcance, G2; una edición de más en A-4 o una ruta de más en la corrección, G2C; un blob congelado cambiado, G3; una
      superficie del mapa o un C-20b distinto de EQUAL, G4; una regla sin su cláusula de alcance, G5; un self-test custodiado que no casa,
      G5b). El control C (G1, G2C, G5 y G5b sobre la corrección sin mutar) debe pasar. Registra los blobs de los textos corregidos y el de
      este archivo (ToolBlob).
  python -I -B a4-guards.py preview --repo <worktree> --a4 <A-4 corregida> --pkg <paquete corregido> --out <json> [--tmp <dir>]
      Sobre las copias corregidas, antes de cualquier commit: G1, G2D (el diff de A-4 frente a 27ffa26b es exactamente las dos ediciones
      declaradas y el del paquete frente a d314afeb, solo las líneas declaradas; la HEAD todavía tiene esos blobs), G3 en la HEAD y G5 (con
      los cinco párrafos idénticos a los de 27ffa26b). Incluye el resumen del self-test.
  python -I -B a4-guards.py run --repo <worktree> --base <sha> --head <sha> --out <json> [--also-at <sha>] [--a4 ... --pkg ...] [--tmp <dir>]
      Con head = A4_PUB (4710084b), la publicación: G1..G5 (base = su padre, A4_BASE). Con otro head, la CORRECCIÓN previa a la revisión:
      base = padre de head, con A-4 = 27ffa26b y paquete = d314afeb; G1, G2C (A-4 y paquete MODIFICADOS solo en las líneas declaradas y con
      los blobs fijados, a4-guards.py y a4-selftest.json AÑADIDOS en docs/automation/evidence/I-62-A4/, evidencia y decisiones solo por
      añadido, estado modificado; nada más), G3, G4, G5 y G5b (a4-selftest.json de head con Result PASS, ToolBlob = blob de a4-guards.py en
      head y A4TextBlob/PkgTextBlob = blobs de A-4 y del paquete en head). Con --also-at, G3 y G5 en otra revisión. Incluye el resumen del
      self-test.
  Corrección 2 (A62-A4-01 y A62-A4-O1..O8 de la revisión R20261009T122416Z-bce3): cuando la base (o, en preview, la HEAD) tiene A-4 =
      7d863219 y paquete = 085f30f6. `run` aplica G1, G2C2, G3, G4, G5 y G5b; `preview`, G1, G2D2, G3 y G5. G2C2/G2D2: el diff de A-4 frente a
      7d863219 solo toca las regiones declaradas (cabecera: Classification y Architect review; §3.2, §3.5, §3.7, §4, §5, §6, §8, §10 y anexo); en
      §3.2 solo cambian las reglas 2 y 3 de A4-1, por inserción, con los términos de A62-A4-O1/O2, y en §3.5 solo la regla 5 de A4-4, con los
      consumidores y el discriminador de A62-A4-01; frases exigidas y retiradas (§5 M-02/M-05 = sí; §3.7 calificada); el paquete nombra el blob
      nuevo, A62-A4-01 y el delta frente a 7d863219; blobs fijados (A4_CORR2_BLOB, PKG_CORR2_BLOB); en el commit, a4-guards.py y
      a4-selftest.json MODIFICADOS, los resultados de las guardas opcionales, evidencia y decisiones solo por añadido, estado modificado, nada más.
      G5 exige además A4-2, A4-3 y A4-5 idénticos a 7d863219, A4-4 idéntica salvo la regla 5 y A4-1 salvo las reglas 2 y 3. El self-test añade el
      escenario C2 (control y mutantes) sobre la base real que contiene 7d863219.
  Archivos del kit de FX-02 que otros commits cambian (README, contratos, plantilla O4): su blob distinto del fijado es INFO si las líneas que A-4
      cita no cambian (G1 lo comprueba línea a línea); G3 los informa sin fallar, salvo que el commit comprobado los toque.

Guardas:
  G1 literales y cabecera: cada literal que A-4 cita en §2, y toda cita «…» de texto congelado en el resto de A-4 (con referencia de línea o
     sin ella), igual a su fuente en el blob fijado (git cat-file), línea a línea (el bloque de §2 por igualdad exacta de cada línea; las citas
     en línea por fragmentos, en orden y con mayúsculas exactas, dentro del rango citado); la referencia de línea que acompaña a cada cita es la
     declarada; cobertura: ninguna cita queda sin comprobar. Cabecera: Freeze, A-1 y A-2 AGREED con sus blobs; A-3 por prefijo de blob (su
     estado «candidata en revisión formal» se informa como INFO: era cierto al publicarse). Prefijos de blob abreviados (§6, §9, cabecera)
     iguales a los blobs reales en la base. Privacidad de A-4 y del paquete: sin rutas de perfil con nombre de usuario (también en forma
     POSIX), sin correos y sin el nombre de usuario ni el correo del host, que se leen en tiempo de ejecución y nunca se escriben.
  G2 rutas del commit de publicación: base = padre de head = A4_BASE; A-4 y su paquete añadidos (A) con los blobs fijados; solo las rutas
     permitidas (evidencia y decisiones solo por añadido, README de F6, estado y la solicitud única de desbloqueo de FX-02); privacidad de
     todo archivo añadido o modificado (en los de solo añadido, de lo añadido). INFO: el diff desde la base de preparación que declara A-4 §6.
  G3 blobs congelados (V14, Freeze, A-1, A-2, A-3, ADR-0046, ADR-0048, clause_map.py) y los que A-4 §6 declara sin cambio, iguales a los
     fijados en base y en head (y en --also-at, con A-4 igual a su blob).
  G4 C-20b: ninguna ruta cambiada en las superficies del mapa (clause_map.py extraído por blob) y `clause_map.py check <merge-base> head`
     = EQUAL.
  G5 delta frente a V14 y A-2: los cinco párrafos normativos de A-4 §3.2-§3.6, exactos; cadena de ubicaciones; aplicación sobre V14 + A-2
     (A-2 aplicada como en a2-guards.py) tras «Ningún otro tope cambia.» (A-2 L234): añadido puro (ningún texto previo se borra ni se
     sustituye), solo cambian las mismas secciones que cambia A-2 (título, Anexo D y D.3; función `sections` de clause_map.py, por blob),
     también sin A4-5 (separable); cada regla abre con su cláusula de alcance (FX-02, Topología A, F6 y su rol o binding); sin nombres de
     producto (salvo la etiqueta congelada «**Codex, total**» que A-4 declara citar); citas internas verbatim en las fuentes; el literal de
     topes de la fila «Codex, total» sin cambio y las cifras de las reglas iguales a él; los invalidadores de A4-1 iguales a los de A-2;
     toda mención de otro escenario, rol, regla (A-1/A-2/A-3, P-01, OD-n) o superficie de producción, en una frase negativa, de conservación o
     de restricción (se listan todas para el revisor, con su frase; las referencias a reglas vigentes que las reglas aplican se listan aparte).
Sin escrituras salvo --out y un directorio temporal propio (--tmp). Nunca escribe la ruta del repositorio ni identidades del host.
"""
import argparse, copy, difflib, hashlib, importlib.util, json, os, re, subprocess, sys, tempfile

# ---------------------------------------------------------------- identidades fijadas
A4_BASE = "e6fe2e0890c927e0f3e936ff8032279442404857"   # padre del commit que publica A-4
A4_PUB = "4710084b7589e27c511572d409b8742b75cfcbf4"    # commit que publica A-4, su paquete y la solicitud única de FX-02
PREP_BASE = "524b293e4667810f485c64d6cbb545d5b2dd8db5"  # base de preparación que declaran A-4 §6 y el paquete (no es el padre)
PREP_A41 = "c8d69fcb35e18ddc660152dca943684a351a7321"   # A4-1 y A4-2 preparadas aquí (A-4 «Origin»; §2: literales «iguales en c8d69fcb»)
V14 = "docs/initiatives/I-62-proposal-v14.md"
V14_COMMIT = "4c617e82b32b6c810b68d75fc19472efed22b393"
FREEZE_SHA = "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
A1 = "docs/initiatives/I-62-A-1.md"
A2 = "docs/initiatives/I-62-A-2.md"
A3 = "docs/initiatives/I-62-A-3.md"
A4 = "docs/initiatives/I-62-A-4.md"
PKG = "docs/initiatives/I-62-architect-package-A-4.md"
A4_BLOB = "27ffa26b35ecacfae083bf9d360fe8460b8d5564"    # A-4 publicada en 4710084b
PKG_BLOB = "d314afeb46d5d43269a6be9cb94d3dd8319e7c90"
A4_CORR_BLOB = "7d863219a271b5ec427ef8669435826e19be8f11"   # A-4 corregida antes de su revisión formal (decisión de la sesión principal)
PKG_CORR_BLOB = "085f30f602532a619734b47b6b8d60035d8fb278"
GUARDS = "docs/automation/evidence/I-62-A4/a4-guards.py"
SELFTEST = "docs/automation/evidence/I-62-A4/a4-selftest.json"
RESULT_RUN = "docs/automation/evidence/I-62-A4/a4-guards-result.json"
RESULT_PREVIEW = "docs/automation/evidence/I-62-A4/a4-guards-result-preview.json"
# Corrección 2: A62-A4-01 y A62-A4-O1..O8 de la revisión R20261009T122416Z-bce3 (custodiada en a5b50c68), sobre 7d863219 / 085f30f6
A4_CORR2_BLOB = "0d9543761e3e3a45a94338776f7c1ba763224ab3"
PKG_CORR2_BLOB = "59052b847ccc13e8be539189ae1b77dc7f9d3623"
F4V = "tests/RackCad.Tests/I62/StateV2Validator.Orchestration.cs"  # validador de producción de F4 que cita A4-4, regla 5 (corrección 2)
A3_OLD_LINE = "A-3 candidata en revisión formal (decisiones §58, punto 3; blob ea6721f7…). A-4 no la modifica, no depende de ella ni la interpreta (Q-A4-07)."
A3_NEW_LINE = ("A-3 AGREED (decisiones §61, punto 1; docs/initiatives/I-62-A-3.md, blob ea6721f78cb63fbc4f9d99563f3262eb36a32c5d). A-4 no la modifica, "
               "no depende de ella ni la interpreta (Q-A4-07).")
# Ediciones declaradas sobre los blobs publicados: ("sub", línea, texto, nuevo) sustituye una vez dentro de esa línea; ("lines", línea, [anteriores],
# [nuevas]) sustituye esas líneas completas. «{a4blob}» = blob de la A-4 corregida que acompaña al paquete.
A4_EDITS = [("lines", 18, [A3_OLD_LINE], [A3_NEW_LINE]),
            ("sub", 341, "AUTOMATION_PLAN L1122-L1123 («sin aceptación fingida»)", "AUTOMATION_PLAN L1122-L1123 («Sin aceptación fingida»)")]
PKG_EDITS = [("lines", 16, ["  docs/initiatives/I-62-A-4.md        blob <el del commit de publicación> (borrador de esta pasada: " + A4_BLOB + ")"],
              ["  docs/initiatives/I-62-A-4.md        blob <el del commit de publicación> (borrador de esta pasada: {a4blob})"]),
             ("lines", 124, ["- **Autoridad:** una disposición del Coordinator que autorice la revisión formal sobre el blob exacto publicado. La revisión no "
                             "interrumpe la", "  revisión formal de A-3 en curso (decisiones §60, punto 0)."],
              ["- **Autoridad:** una disposición del Coordinator que autorice la revisión formal sobre el blob exacto publicado. A-3 AGREED (decisiones §61)."])]
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
CLAUSE_MAP = "docs/automation/evidence/I-62-F4/compat/clause_map.py"
CLAUSE_MAP_BLOB = "ffe6ea57257f30fa9eec3e9aaf5bf9982bf59d08"
AP = "docs/AUTOMATION_PLAN.md"
RAE = "docs/automation/agent-execution/README.md"
CODEX = "docs/automation/agent-execution/adapters/codex-cli.md"
CLAUDE = "docs/automation/agent-execution/adapters/claude-cli.md"
ROUTING = "docs/automation/agent-execution/routing.md"
CATALOG = "docs/automation/agent-execution/model-catalog.md"
KIT = "docs/automation/evidence/I-62-F6/kits/FX-02/README.md"
CC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
O4 = "docs/automation/evidence/I-62-F6/kits/FX-02/order-FX-U1-O4.template.md"
DEC = "docs/automation/decisions/I-62.md"
EV = "docs/automation/evidence/I-62-evidence.md"
F6README = "docs/automation/evidence/I-62-F6/README.md"
STATE = "docs/automation/state/I-62.yml"
REQUEST = "docs/automation/evidence/I-62-F6/requests/FX-02-unblock-request-2026-10-09.md"
EVAL = "docs/automation/evidence/I-62-F6/FX-02/F6-OBS-03-cmd-route-evaluation.md"
A2P2_RESULT = "docs/automation/evidence/I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json"
CLAUDE_CHAR = "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/README.md"

FROZEN = {  # decisiones §61, punto 2, y el encargo de las guardas
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f",
    A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d",
    ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534",
    ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e",
    CLAUSE_MAP: CLAUSE_MAP_BLOB,
}
DECLARED = {  # A-4 §6, «Blobs sin cambio» (además de los de FROZEN)
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5",
    RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055",
    CODEX: "155f3469e345e2397fa26347a9cb52d6fdb91122",
    CLAUDE: "ae570380509cacabf11f757a27573bf6fe616a36",
    ROUTING: "bba08fc4686d7192deeb559752636a467103b4b1",
    CATALOG: "166d978d114e635bbef67e295c2ec1da73af6d1f",
    KIT: "fd7ef02dcccd782fca44f00dc00676285c3b87d1",
    CC: "90929ad1c49c028f41f58f654f8713bd0b35f27b",
    O4: "d92f7d3e13b122bf7669639cc0571a248650beda",
    F4V: "355e1b35f7d8c7e318f5dbd6801429d4ad19899e",  # citado desde la corrección 2 (§3.7 y §6); sin cambio desde F4-G
}
EVOLVING = {KIT, CC, O4}  # archivos del kit que otros commits pueden cambiar: sus líneas citadas no deben cambiar (G1); su blob, INFO (G3)
SOURCE_PATHS = {"V14": V14, "A2": A2, "AP": AP, "RAE": RAE, "CODEX": CODEX, "CLAUDE": CLAUDE, "ROUTING": ROUTING, "KIT": KIT, "CC": CC, "F4V": F4V,
                "O4": O4, "DEC": DEC, "EVAL": EVAL}
SOURCES = {t: (FROZEN.get(p) or DECLARED.get(p)) for t, p in SOURCE_PATHS.items()}
SOURCES["DEC"] = "625a7075e06f10edf25c0574067202f6f214c7e0"   # decisiones en 524b293e = en la base (hasta §60); creció por añadido después
SOURCES["EVAL"] = "83a960cacc951a3c643af5049d1fc1f8aae16866"  # evaluación de la ruta cmd.exe (cabecera de A-4: «blob 83a960ca…»)
BQ_SOURCES = {"A2"}  # fuentes cuyas líneas citadas son de un bloque citado de Markdown («> »): se comparan sin el marcador
APPEND_SOURCES = {"DEC": DEC}
EVOLVING_SOURCES = {t for t, p in SOURCE_PATHS.items() if p in EVOLVING or t == "EVAL"}  # fuentes que crecen solo por añadido: en una base posterior, su texto empieza por el del blob fijado

# Prefijos de blob abreviados que A-4 escribe («`abcdef12…`» o «blob abcdef12…») -> ruta cuyo blob real en la base debe empezar así
SHORT_BLOBS_C2 = [("355e1b35", F4V), ("ceb38284", KIT), ("7d863219", A4)]  # prefijos que solo escribe la corrección 2
SHORT_BLOBS = [("34ad80ea", V14), ("0f6b8608", FREEZE), ("c01899a7", A1), ("f1e1d6f0", A2), ("ea6721f7", A3), ("e1bd8d91", ADR48),
               ("f525cb1e", AP), ("592dcfd4", RAE), ("155f3469", CODEX), ("ae570380", CLAUDE), ("bba08fc4", ROUTING), ("166d978d", CATALOG),
               ("fd7ef02d", KIT), ("90929ad1", CC), ("d92f7d3e", O4), ("83a960ca", EVAL), ("b4a5679b", A2P2_RESULT), ("0b682899", CLAUDE_CHAR)]

HEADER_FACTS = [
    ("FREEZE_SHA", "FREEZE_SHA     = " + FREEZE_SHA),
    ("registro del Freeze", "record         = %s (blob %s)" % (FREEZE, FROZEN[FREEZE])),
    ("Proposal", "proposal       = " + V14),
    ("commit de V14", "  commit       = " + V14_COMMIT),
    ("blob de V14", "  blob         = " + FROZEN[V14]),
    ("A-1 AGREED", "A-1 AGREED (decisiones §43; %s, blob %s)" % (A1, FROZEN[A1])),
    ("A-2 AGREED", "A-2 AGREED (decisiones §57, punto 1; %s, blob %s)" % (A2, FROZEN[A2])),
    ("Applies-to", "Applies-to: I-62"),
]
A3_HEADER = re.compile(r"^A-3 (.+?) \(decisiones §(\d+), punto (\d+); (?:docs/initiatives/I-62-A-3\.md, )?blob ([0-9a-f]{7,40})(…?)\)", re.M)

# ---------------------------------------------------------------- literales citados (fuente, líneas, texto exacto tal como lo escribe A-4)
# Ref: la referencia de línea que A-4 pone junto a la cita (None si A-4 no la pone; la cita se comprueba igual en la línea fijada aquí).
# RefAfter: la referencia va después de la cita. Where: dónde está la cita en A-4.
LIT = [
    ("L01", "V14", 2515, 2515, "| Architect (`claude-cli`, invocación distinta) | 1 | +1 | 2 |", "§2"),
    ("L02", "V14", 2523, 2523, "| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |", "§2"),
    ("L03", "V14", 2528, 2528, "Ningún tope autoriza gasto ni ejecución ahora.", "§2"),
    ("L04", "V14", 2544, 2544, "| FX-02 PASS | OD-5 + **OD-7** + OD-2 + medición del binario | UNVERIFIED; F6 pendiente |", "§2"),
    ("L05", "V14", 2552, 2552, "una limitación de `claude-cli` afecta al Reviewer y al Architect de B;", "§2"),
    ("L06", "V14", 2560, 2560, "FX-02, pasos hasta VERIFIED: Codex planificación 1 + verificación 1 + nc1 1 + pool 2 = 5; Worker 1 (+1); Principal 1;", "§2"),
    ("L07", "V14", 972, 972, "| OD-3 | autenticar Claude CLI | `claude-cli` en B | antes de F6 B |", "§2"),
    ("L08", "V14", 974, 974, "(autorización previa de apertura y consumo, no transporte)", "§2"),
    ("L09", "A2", 222, 223, "puede autorizarse **un solo** bloque de medición nuevo por cada actualización observada, de como máximo **dos** sondas de "
                            "solo lectura para el par exacto (`BinaryHash`, `AppVersion`) nuevo", "§2"),
    ("L10", "A2", 233, 233, "Las sondas de un bloque no consumen los lanzamientos de Codex de ninguna ronda ni el sumando «+ 2 sondas» de los totales", "§2"),
    ("L11", "A2", 238, 239, "`BinaryHash`, `AppVersion`, huella, estado de autenticación, modelo, effort, blobs de catálogo y de routing, e instancia del host", "§2"),
    ("L12", "AP", 542, 543, "stdin cerrado; el `pwsh` del runtime de Codex delante en el `PATH` del proceso hijo; tope de 600 s", "§2"),
    ("L13", "CODEX", 24, 24, "… stdin cerrado y el `pwsh` del runtime primero en el `PATH`", "§2"),
    ("L14", "CODEX", 53, 53, "| `RuntimeShellFirstInPath` | YES si el `pwsh` del runtime va primero en el `PATH` del proceso hijo |", "§2"),
    ("L15", "AP", 1069, 1070, "invocación **medida** de la celda (MEASURED), consumo cubierto (OFFICIAL o MEASURED, nunca UNKNOWN) y celda no `Stale`", "§2"),
    ("L16", "AP", 1093, 1093, "**Sin celda elegible no se invoca** (P-10)", "§2"),
    ("L17", "AP", 1159, 1161, "Si falta un REQUIRED, el binding del revisor o del verificador **no se acepta** […] Un PREFERRED no satisfecho se registra.", "§2"),
    ("L18", "CC", 127, 128, "`Scope` = pass solo con la salida reproducible de `git diff --name-only BaseSha..CurrentSha` y la entrada de "
                            "`AllowedWriteScope` que contiene cada ruta; si no, `not_run`", "§2"),
    ("L19", "CC", 217, 217, "D.3: 1 lanzamiento dentro de los 11 de Codex (reejecuciones del pool)", "§2"),
    ("L20", "KIT", 44, 44, "ARCHITECT (REVIEW_DESIGN; frente a AUTHOR: REQUIRED ×3, Proveedor PREFERRED)", "§2"),
    ("L21", "V14", 386, 387, "aceptación de bindings nuevos;", "§2"),
    ("L22", "V14", 1999, 1999, "| bindings | los ya aceptados, en `planned_roles` | aceptación de bindings nuevos (Worker, Controller de verificación) | "
                               "custodiados por el manifiesto |", "§2"),
    ("L23", "V14", 3081, 3081, "| 3 | P, Coordinator | sin escritura | binding W, preflight W, aceptación A1'-A8' de d, nc4 | **ACCEPTED_OPEN** | — |", "§2"),
    ("L24", "V14", 614, 614, "alguna de A1'-A8' en fail | Coordinator | diario; Q7 con cierre NOT_ACCEPTED | ningún Worker", "§2"),
    ("L25", "V14", 1884, 1884, "Con INDIVIDUAL_DECISION: `DecisionRef` = `{Path, Marker}`, la ruta del archivo de decisiones de la unidad y el marcador de la "
                               "decisión del Coordinator (§8.6), sin blob para no crear ciclos", "§2"),
    ("L26", "AP", 1101, 1101, "`ACCEPTED` con `Basis` = INDIVIDUAL_DECISION: decisión del Coordinator en `decisions/<unit>.md` (`DecisionRef`)", "§2"),
    ("L27", "AP", 1108, 1109, "**Materialización autorizada.** Solo para ARCHITECT, […] y para REVIEWER", "§2"),
    ("L28", "AP", 1122, 1123, "**Sin aceptación fingida:** un binding materializado nunca lleva un `DecisionRef` que simule una decisión individual que "
                              "no ocurrió", "§2"),
    ("L29", "AP", 1339, 1339, "o tras el cierre declarado de la ventana", "§2"),
    ("L30", "AP", 1349, 1349, "**W-2:** entre el Q0 y el cierre no hay escrituras Git de la sesión", "§2"),
    ("L31", "RAE", 387, 387, "La disposición es la más grave de las fallidas", "§2"),
    ("L32", "RAE", 399, 399, "`DecisionRef` = una decisión registrada del Coordinator: el marcador existe en el archivo de decisiones de la unidad y la "
                             "nombra; nunca un marcador fabricado", "§2"),
    ("L33", "RAE", 437, 437, "en Q7, toda referencia TRANSIENT se resuelve a CUSTODIED por el manifiesto de custodia", "§2"),
    ("L34", "O4", 95, 95, "todas antes del Q0; dentro de la ventana no se publica ninguna", "§2"),
    ("L35", "ROUTING", 114, 114, "`<PERFIL>.effort`: el effort de la delegación", "§2"),
    ("L36", "V14", 1860, 1860, "Para una celda candidata sin sesión: `Assurance` = NONE e `InstanceId` = `\"NOT_STARTED\"`", "§2"),
    ("L37", "V14", 1139, 1139, "El intento custodia la identidad observada en su `relay-record/v2` (`Participant.Observation`)", "§2"),
    ("L38", "V14", 2176, 2176, "todo resultado ingerido de ARCHITECT tiene un `runtime_evidence` cuyo actor observado difiere del de cada autor IA del "
                               "objeto (§11)", "§2"),
    ("L39", "V14", 657, 657, "un lanzamiento con resultado incierto se cuenta como lanzado", "§2"),
    ("L40", "V14", 788, 788, "el resultado no se ingiere como dictamen; el intento cuenta", "§2"),
    ("L41", "AP", 1146, 1147, "La independencia se evalúa sobre identidades **observadas**, nunca sobre identificadores de binding ni sobre "
                              "autodeclaraciones", "§2"),
    ("L42", "AP", 1156, 1156, "UNKNOWN en una dimensión REQUIRED no satisface; `Assurance` NONE deja la dimensión en UNKNOWN", "§2"),
    ("L43", "AP", 1163, 1163, "Es el conjunto de actores que escribieron el rango evaluado", "§2"),
    ("L44", "AP", 1168, 1168, "| Toda delegación | EXECUTION_CONTROLLER (verificación) | WORKER | REQUIRED | REQUIRED | REQUIRED | NOT_REQUIRED | no es "
                              "posible VERIFIED: BLOCKED de planificación |", "§2"),
    ("L45", "AP", 1121, 1121, "**UNKNOWN cuenta como NOT_SATISFIED**", "§2"),
    ("L46", "RAE", 396, 396, "toda dimensión REQUIRED está en SATISFIED (§14.5)", "§2"),
    ("L47", "RAE", 446, 446, "UNKNOWN con `Assurance` NONE", "§2"),
    ("L48", "CC", 212, 212, "Evaluación de un candidato no arrancado: OQ-04", "§2"),
    ("L49", "V14", 2444, 2444, "+1 solo si hay REWORK (corrección, cuenta en `attempts`) | 2", "§2"),
    ("L50", "V14", 2535, 2535, "violación observada", "§2"),
    ("L51", "V14", 2536, 2536, "falta una precondición", "§2"),
    ("L52", "AP", 538, 538, "Tras un REWORK se vuelve a (1) con el commit de `attempts`", "§2"),
    ("L53", "AP", 610, 610, "como máximo dos reejecuciones por (`TaskId`, fase)", "§2"),
    ("L54", "AP", 612, 612, "una invocación que lo excedería no se lanza: STOP (P-07)", "§2"),
    ("L55", "KIT", 44, 44, "`CorrectionsAuthorized` true", "§2"),
    ("L56", "DEC", 1173, 1173, "Si la ruta alternativa requiere una modificación material del contrato, preparar la disposición o enmienda correspondiente "
                               "antes de ejecutarla. Para el Architect, preferir `claude-cli` si el contrato permite ese adapter y cumple "
                               "independencia, capacidad y consumo", "§2"),
    ("L57", "DEC", 1083, 1083, "fixture — […] Reviewer y Architect de FX-03 y Architects de FX-06", "§2"),
    ("L58", "DEC", 1085, 1085, "roles ARCHITECT y REVIEWER de I-62", "§2"),
    # fuera de §2, con referencia de línea
    ("L59", "A2", 234, 234, "Ningún otro tope cambia.", "§3.2 (ubicación)"),
    ("L60", "DEC", 1161, 1162, "la celda del Architect no ejecuta comandos", "§8 Q-A4-04"),
    ("L61", "KIT", 167, 167, "candidata sin credencial", "§8 Q-A4-08"),
    ("L62", "V14", 972, 972, "`claude-cli` en B", "§8 Q-A4-09"),
    ("L63", "AP", 1122, 1123, "Sin aceptación fingida", "§8 Q-A4-10"),  # 27ffa26b decía «sin…» (solo mayúsculas); corregido en 7d863219
    ("L64", "AP", 1121, 1121, "UNKNOWN cuenta como NOT_SATISFIED", "§8 Q-A4-13"),
    # sin referencia de línea en A-4 (texto congelado citado; se fija aquí su línea)
    ("L65", "V14", 2537, 2537, "capacidad medida ausente", "§8 Q-A4-02 (D.4)"),
    ("L66", "RAE", 399, 399, "la nombra", "§8 Q-A4-10 (B9)"),
    ("L67", "KIT", 248, 248, "reintentos de transporte", "§8 Q-A4-14"),
    ("L68", "V14", 2523, 2523, "+ 2 sondas", "A4-1, §4 y §8"),
    ("L69", "AP", 1166, 1166, "Si falta un REQUIRED", "A4-4 y §4"),
    ("L70", "V14", 2444, 2444, "+1 solo si hay REWORK", "A4-5"),
    ("L71", "V14", 2442, 2442, "**Codex, total**", "A4-5"),
    ("L72", "V14", 2442, 2442, "pool **4** (≤ 2 por fase)", "A4-5"),
]
LIT_C2 = [("L73", "V14", 1884, 1884, "con **todos** en SATISFIED", "cabecera, §3.7 y §5 (corrección 2)")]  # citas nuevas de la corrección 2
NO_REF = {"L65", "L66", "L67", "L68", "L69", "L70", "L71", "L72", "L73"}
REF_AFTER = {"L59", "L64"}
NOTATION = {"V14 L…", "DEC L…"}                              # notación de §2, no son citas
SELF_QUOTES = {"la comprobación de esa declaración": "A4-1"}  # citas del propio texto normativo de A-4
BLOCK_HEADER = re.compile(r"^V14 D\.3, Topología A, ((?:L\d+(?:, | y ))*L\d+), en ese orden")
# Referencias sin cita que A-4 hace a líneas concretas: se comprueba un testigo en la línea (aviso, no fallo: el encargo exige las citas).
REF_TOKENS = [
    ("V14", 2446, 2446, ["N10"], "V14 L2446"), ("V14", 2544, 2544, ["FX-02 PASS"], "V14 L2544"),
    ("V14", 2515, 2515, ["Architect (`claude-cli`"], "V14 L2515"), ("V14", 2552, 2552, ["afecta al Reviewer y al Architect de B"], "V14 L2552"),
    ("V14", 2442, 2442, ["**Codex, total**"], "L2442 (arriba)"), ("V14", 974, 974, ["OD-5"], "V14 L974"),
    ("V14", 2176, 2176, ["ARCHITECT"], "V14 L2176"), ("A2", 236, 239, ["observación queda obsoleta"], "A-2 L236-L239"),
    ("RAE", 398, 398, ["| B8 |"], "L398, B8"), ("CC", 120, 121, ["AuthorityResolution[]", "ClauseMapBlob"], "L120-L121"),
    ("DEC", 1114, 1114, ["2.1.293", "ARCHITECT y REVIEWER"], "DEC L1114"), ("DEC", 1152, 1152, ["OD-2 = A", "6518EFAB"], "DEC L1152"),
    ("CLAUDE", 23, 30, ["| 2 | observar | UNVERIFIED", "| 9 | declarar la huella | UNVERIFIED"], "L23-L30"),
    ("KIT", 140, 140, ["S18b"], "L140"), ("KIT", 144, 144, ["S21b"], "L144"), ("KIT", 180, 180, ["S30"], "L180"),
    ("KIT", 243, 249, ["6. Worker"], "L243-L249"), ("O4", 28, 31, ["{BINDING_ACCEPTANCE_RULE}", "{IN_WINDOW_STOP_RULE}"], "L28-L31"),
    ("O4", 44, 44, ["{CORRECTION_RULE}"], "L44"), ("EVAL", 51, 51, ["V1."], "L51"), ("EVAL", 13, 13, ["`py`"], "L13"),
]
REF_TOKENS_C2 = [
    ("F4V", 119, 124, ["AUTHORIZED_MATERIALIZATION", "criterion not SATISFIED"], "L119-L124"),
]

# ---------------------------------------------------------------- G5: delta
ANCHOR_V14 = "Ningún tope autoriza gasto ni ejecución ahora."    # ancla de A-2 (A-2 §2.3)
ANCHOR_A2_END = "Ningún otro tope cambia."                        # fin del párrafo de A2-P2 (A-2 L234), ancla de A-4
D3 = "### D.3 Hoja de invocaciones y escenarios"
D4 = "### D.4 "
CAPS = "| **Codex, total** | | **11** | | pool **4** (≤ 2 por fase) | **15** |"
CAPS_LINE = 2442
RULE_IDS = ["A4-1", "A4-2", "A4-3", "A4-4", "A4-5"]
LOCATIONS = {
    "A4-1": "Ubicación: al final de V14 D.3, a continuación del párrafo de A2-P2, que termina en «Ningún otro tope cambia.» (A-2 L234).",
    "A4-2": "Ubicación: a continuación del párrafo de A4-1.",
    "A4-3": "Ubicación: a continuación del párrafo de A4-2.",
    "A4-4": "Ubicación: a continuación del párrafo de A4-3.",
    "A4-5": "Ubicación: a continuación del párrafo de A4-4.",
}
SCOPE = {  # la primera frase tras el título de la regla: abre con «Solo para» y contiene todo esto
    "A4-1": ["Solo para la celda de EXECUTION_CONTROLLER de la Topología A", "(FX-02)", "en F6"],
    "A4-2": ["Solo para el Architect de la Topología A", "revisión del contrato", "FX-02", "en F6"],
    "A4-3": ["Solo para la tarea de FX-02 de la Topología A", "en F6", "solo para los dos bindings nuevos",
             "el del Worker y el del Controller de verificación"],
    "A4-4": ["Solo para FX-02 de la Topología A", "en F6", "solo para los bindings con Actor, Sesión o Contexto REQUIRED", "el Controller",
             "frente a WORKER", "el Architect frente a AUTHOR"],
    "A4-5": ["Solo para la tarea de FX-02 de la Topología A", "en F6"],
}
IN_SCOPE_ROLES = {"A4-1": {"Controller", "EXECUTION_CONTROLLER"}, "A4-2": {"Architect", "ARCHITECT"}, "A4-3": {"Worker", "Controller"},
                  "A4-4": {"Controller", "EXECUTION_CONTROLLER", "Architect", "ARCHITECT", "WORKER", "AUTHOR"}, "A4-5": {"Worker"}}
PRODUCT_TOKENS = re.compile(r"(?i)\b(codex|claude|openai|anthropic|gpt-|opus|sonnet|haiku|chatgpt|gemini|google|copilot|mistral|llama|deepseek|grok"
                            r"|xai|cursor|pwsh|powershell|cmd\.exe)|\b[a-z0-9]+-cli\b")
PRODUCT_EXEMPT = {"A4-5": ["**Codex, total**"]}  # etiqueta congelada de V14 L2442 que A-4 declara citar (paquete, §3, último punto)
CROSS = [  # (categoría, patrón): toda mención debe estar en una frase negativa, de conservación o de restricción
    ("ESCENARIO", r"\bFX-0(?:1|3|4a|4b|5|6)\b|\bD\.[58]\b|\bTopología B\b|\bronda B\b|\bOV\b"),
    ("ROL", r"\b(?:PRINCIPAL_COORDINATOR|Principal|PRINCIPAL|Reviewer|REVIEWER|Worker|WORKER|Controller|EXECUTION_CONTROLLER|Architect|ARCHITECT"
            r"|AUTHOR)\b"),
    ("REGLA", r"\bA-[123]\b|\bA2-P[12]\b|regla de nueva medición|\bP-01\b|\bOD-\d\b|\bOD-2[a-z]?\b|\bCLAUDE-CLI-I62\b"),
    ("SUPERFICIE", r"\breceta\b|\bdescriptor(?:es)?\b|\bAUTOMATION_PLAN\b|\brouting\b|\bcatálogo\b|\besquemas?\b|\bADR\b|\bstate/v2\b|\bproducción\b"
                   r"|\bB\.1-B\.11\b"),
]
INVOKED = r"\bP-(?:07|10|20)\b|\bD\.4\b|\b16\.\d+\b|\bB\.\d(?:\.\d)?\b|§\d+(?:\.\d+)?|\bW-2\b|\bT3'|\bT10\b|\bB\d+(?:-B\d+)?\b|\bA7'|\bQ0\b|\bQ7\b" \
          r"|\bCoordinator\b|\bOwner\b|relay-record/v2|Participant\.Observation"
MARK_PRES = re.compile(r"(?i)\b(conservan?|se conserva|sin cambio|tal como están?|rigen? sin cambio|no cambian?|no amplía|no se aplica)\b")
MARK_NEG = re.compile(r"(?i)\b(no|ni|nunca|ningún|ninguna|ninguno|sin|tampoco)\b")
MARK_RESTR = re.compile(r"(?i)\b(solo|fuera de|aparte|únicamente)\b")

# ---------------------------------------------------------------- G2: rutas
ALLOWED_STATUS = {A4: {"A"}, PKG: {"A"}, REQUEST: {"A"}, EV: {"M"}, DEC: {"M"}, F6README: {"M"}, STATE: {"M"}}
APPEND_ONLY = (EV, DEC)
REQUIRED_ADDED = (A4, PKG)
ALLOWED_A4_S6 = {A4, PKG, EV, DEC, F6README, STATE}  # A-4 §6: «Diff permitido sobre 524b293e» (más evidencia de I-62 por añadido)
CORR_STATUS = {A4: {"M"}, PKG: {"M"}, GUARDS: {"A"}, SELFTEST: {"A"}, EV: {"M"}, DEC: {"M"}, STATE: {"M"}}  # commit de corrección
CORR_REQUIRED = {A4: "M", PKG: "M", GUARDS: "A", SELFTEST: "A"}
CORR2_STATUS = {A4: {"M"}, PKG: {"M"}, GUARDS: {"M"}, SELFTEST: {"M"}, EV: {"M"}, DEC: {"M"}, STATE: {"M"}, RESULT_RUN: {"M"}, RESULT_PREVIEW: {"M"}}
CORR2_REQUIRED = {A4: "M", PKG: "M", GUARDS: "M", SELFTEST: "M"}
# Regiones de A-4 que la corrección 2 puede cambiar (frente a 7d863219); §3.2 y §3.5 solo en las reglas declaradas
C2_REGIONS = {"header:Classification", "header:Architect review", "§3.2", "§3.5", "§3.7", "§4", "§5", "§6", "§8", "§10", "Anexo"}
C2_ITEMS = {"A4-1": {"2": ["`Identity`", "`Remote`", "`CleanTree`", "`Trailer`", "estado e ignorados", "(historia)"],
                     "3": ["(`-C`)", "en total para FX-02 en F6", "solo la regla 4 abre otra"]},
            "A4-4": {"5": ["`PreflightRef`", "`Assurance` NONE", "`InstanceId` NOT_STARTED", "(a)-(d)", "aceptante de A7'", "Q7", "S29", "F4",
                           "cualquier otro UNKNOWN", "al pie de la letra", "«UNKNOWN cuenta como NOT_SATISFIED» rige sin cambio"]}}
C2_INSERT_ONLY = {("A4-1", "2"), ("A4-1", "3")}  # precisiones de A62-A4-O1 y O2: solo inserciones (nada del texto anterior se borra)
C2_REQUIRED_PHRASES = ["M-02, M-03, M-04 y M-05 sí; M-01 y M-06..M-08 no (§5)", "| M-02 | **sí** |", "| M-05 | **sí** |",
                       "A4-4 es una enmienda de semántica, acotada a FX-02 en F6, de B6, del criterio INDEPENDENCE de la materialización y de la "
                       "condición de B.5 «con **todos** en SATISFIED»", "al estilo de A-1 D1-21", "no aplica la excepción y no se cambia",
                       "Re-revisión de esta versión: PENDING", "A62-A4-O8"]
C2_FORBIDDEN_PHRASES = ["M-03 y M-04 sí; M-01, M-02 y M-05..M-08 no", "| M-02 | no |", "| M-05 | no |", "Guardas mecánicas: PENDIENTES",
                        "tampoco producción, F4, esquemas", "A4-3 y A4-4 son excepciones acotadas a FX-02 en F6 sobre cuándo"]

# ---------------------------------------------------------------- privacidad (patrón de a3-guards.py)
PRIVACY = re.compile(r"(?i)(?:\b[a-z]:|(?<![\w.])/(?:mnt/)?[a-z])[\\/]+users[\\/]+[^\\/\s%<*]|\b[\w.+-]+@[\w-]+\.[a-z]{2,}\b")


def host_identities(repo):  # nombre de usuario y correo del host, leídos en tiempo de ejecución; nunca se escriben ni se imprimen
    ids = [os.environ.get("USERNAME", ""), os.environ.get("USER", ""), os.path.basename(os.environ.get("USERPROFILE", "").rstrip("\\/"))]
    try:
        r = subprocess.run(["git", "-C", repo, "config", "user.email"], capture_output=True)
        ids.append(r.stdout.decode("utf-8", "replace").strip() if r.returncode == 0 else "")
    except OSError:
        pass
    # `git config user.name` no entra: es el propietario público de los repositorios (como en a3-guards.py)
    return sorted({i for i in ids if len(i) >= 3}, key=len, reverse=True)


def privacy_findings(name, text, ids):
    f = []
    hits = sorted(set(m.group(0) for m in PRIVACY.finditer(text)))
    if hits:
        f.append("%s con una ruta de usuario o un correo (%d coincidencias)" % (name, len(hits)))
    low = text.lower()
    if any(re.search(r"(?<![\w.@-])" + re.escape(i.lower()) + r"(?![\w@-])", low) for i in ids):
        f.append("%s con el nombre de usuario o el correo del host" % name)
    return f


# ---------------------------------------------------------------- utilidades
def norm(s):
    return " ".join(s.split())


def blob_id(raw):
    return hashlib.sha1(b"blob %d\0" % len(raw) + raw).hexdigest()


def quotes(text):
    """(inicio, fin, profundidad) de cada «…» equilibrado, en orden; None si no está equilibrado."""
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


def unquote_md(s):
    return norm(re.sub(r"\n>[ \t]?", "\n", s))


def header_block(md):
    m = re.search(r"^```text\n(.*?)\n```", md, re.S | re.M)
    return m.group(1) if m else ""


def section(md, pat, stop):
    m = re.search(r"^%s.*?(?=^%s|\Z)" % (pat, stop), md, re.S | re.M)
    return m.group(0) if m else ""


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

    def cat(self, blob):
        return self("cat-file", "-p", blob).replace("\r\n", "\n")

    def blob(self, rev, path):
        out = self("rev-parse", "-q", "--verify", "%s:%s" % (rev, path), check=False)
        return out.strip() if out else None


def load_clause_map(git, td):
    path = os.path.join(td, "clause_map_pinned.py")
    with open(path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(git.cat(CLAUSE_MAP_BLOB))
    spec = importlib.util.spec_from_file_location("clause_map_pinned", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod, path


# ---------------------------------------------------------------- recogida de entradas (única parte que lee el repositorio)
def collect_common(git, base):
    inp = {"Sources": {t: git.cat(b) for t, b in SOURCES.items()}}
    inp["SourceBlobsAtBase"] = {t: git.blob(base, p) for t, p in SOURCE_PATHS.items()}
    inp["SourceBlobsAtPrep"] = {rev[:8]: {t: git.blob(rev, p) for t, p in SOURCE_PATHS.items()} for rev in (PREP_BASE, PREP_A41)}
    inp["ShortBlobsAtBase"] = {p: git.blob(base, p) for _, p in SHORT_BLOBS + SHORT_BLOBS_C2}
    inp["SourceTextsAtBase"] = {t: git.show(base, SOURCE_PATHS[t]) for t in EVOLVING_SOURCES if inp["SourceBlobsAtBase"].get(t) != SOURCES[t]}
    inp["FreezeCommit"] = {"V14": git.blob(FREEZE_SHA, V14), "Record": git.blob(FREEZE_SHA, FREEZE), "V14AtV14Commit": git.blob(V14_COMMIT, V14)}
    inp["DecBase"] = git.show(base, DEC)
    return inp


PREV = {"C1": (A4_BLOB, PKG_BLOB), "C2": (A4_CORR_BLOB, PKG_CORR_BLOB)}  # versión de la corrección -> blobs de A-4 y del paquete sobre los que se aplica


def collect_preview(git, a4_text, pkg_text):
    head = git("rev-parse", "HEAD").strip()
    inp = collect_common(git, head)
    paths = sorted(set(FROZEN) | set(DECLARED))
    repo = {A4: git.blob(head, A4), PKG: git.blob(head, PKG)}
    version = "C2" if repo[A4] == A4_CORR_BLOB else "C1"  # la HEAD decide sobre qué versión publicada se aplicarían las copias
    inp.update({"Mode": "PREVIEW", "Version": version, "Base": head, "Head": head, "A4": a4_text, "PKG": pkg_text,
                "A4Prev": git.cat(PREV[version][0]), "PkgPrev": git.cat(PREV[version][1]), "RepoBlobs": repo,
                "Blobs": {"Head": {p: git.blob(head, p) for p in paths}}})
    return inp


def collect(git, base, head, also, cm, tool, td):
    b0, h0 = git("rev-parse", base).strip(), git("rev-parse", head).strip()
    inp = collect_common(git, b0)
    inp.update({"Base": b0, "Head": h0})
    base, head = b0, h0
    inp["HeadParent"] = git("rev-parse", head + "^1").strip()
    inp["A4"] = git.show(head, A4)
    inp["PKG"] = git.show(head, PKG)
    base_a4 = git.blob(base, A4)
    if head == A4_PUB:
        inp["Mode"], inp["Version"] = "PUBLICATION", "P"
    elif base_a4 == A4_CORR_BLOB:
        inp["Mode"], inp["Version"] = "CORRECTION2", "C2"
    else:
        inp["Mode"], inp["Version"] = "CORRECTION", "C1"
    if inp["Version"] in PREV:
        inp["A4Prev"], inp["PkgPrev"] = git.cat(PREV[inp["Version"]][0]), git.cat(PREV[inp["Version"]][1])
    rows = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", base, head).splitlines() if l]
    inp["Status"] = {r[-1]: r[0] for r in rows}
    inp["HeadTexts"] = {p: git.show(head, p) for p, st in inp["Status"].items() if st in ("A", "M")}
    inp["BaseTexts"] = {p: git.show(base, p) for p, st in inp["Status"].items() if st == "M"}
    inp["HeadBlobs"] = {p: git.blob(head, p) for p in (A4, PKG, GUARDS, SELFTEST)}
    inp["BaseBlobs4"] = {p: git.blob(base, p) for p in (A4, PKG)}
    paths = sorted(set(FROZEN) | set(DECLARED))
    inp["Blobs"] = {"Base": {p: git.blob(base, p) for p in paths}, "Head": {p: git.blob(head, p) for p in paths}}
    if also:
        a = git("rev-parse", also).strip()
        inp["AlsoAt"] = a
        inp["Blobs"]["AlsoAt"] = {p: git.blob(a, p) for p in paths + [A4]}
        inp["A4AlsoAt"] = git.show(a, A4)
        inp["DecAlsoAt"] = git.show(a, DEC)
    prep = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", PREP_BASE, head).splitlines() if l]
    inp["PrepStatus"] = {r[-1]: r[0] for r in prep}
    prep_base = [l.split("\t") for l in git("diff", "--name-status", "--no-renames", PREP_BASE, base).splitlines() if l]
    inp["PrepToBase"] = {r[-1]: r[0] for r in prep_base}
    mb = git("merge-base", head, "origin/main").strip()
    r = subprocess.run([sys.executable, "-I", "-B", tool, "check", mb, head, os.path.join(td, "c20b.json")], cwd=git.repo, capture_output=True)
    inp["C20b"] = {"Base": mb, "ExitCode": r.returncode, "Output": (r.stdout + r.stderr).decode("utf-8", "replace").strip()[-400:]}
    return inp


# ---------------------------------------------------------------- G1
def literal_match(quote, seg):
    """Fragmentos de la cita ([…] separa; … inicial = elisión) en orden, exactos, en las líneas seg unidas. Devuelve (ok, ajuste)."""
    q = quote.strip()
    if q.startswith("…"):
        q = q[1:].strip()
    frags = [norm(x) for x in q.split("[…]")]
    parts = [norm(l) for l in seg]
    hay = " ".join(p for p in parts if p)
    pos, spans = 0, []
    for fr in frags:
        i = hay.find(fr, pos)
        if i < 0:
            folded = all(x.casefold() in hay.casefold() for x in frags)  # diagnóstico: ¿difiere solo en mayúsculas/minúsculas?
            return False, "CASE_ONLY" if folded else None
        spans.append((i, i + len(fr)))
        pos = i + len(fr)
    s, e = spans[0][0], spans[-1][1]
    tight = True
    if len(seg) > 1:
        first = len(parts[0])
        last_start = len(hay) - len(parts[-1])
        tight = s < first and e > last_start
    return True, tight


REF = re.compile(r"\bL(\d+)(?:-L(\d+))?\b")


def g1_literals(inp, ids):
    f, info, warn = [], [], []
    a4 = inp["A4"] or ""
    c2 = inp.get("Version") == "C2"
    lit = LIT + (LIT_C2 if c2 else [])
    ref_tokens = REF_TOKENS + (REF_TOKENS_C2 if c2 else [])
    src = {t: txt.split("\n") for t, txt in inp["Sources"].items()}
    cited = {}  # fuente -> líneas citadas (literales y testigos), para las fuentes del kit que otros commits pueden cambiar
    for x in lit:
        cited.setdefault(x[1], set()).update(range(x[2], x[3] + 1))
    for x in ref_tokens:
        cited.setdefault(x[0], set()).update(range(x[1], x[2] + 1))
    for t, b in SOURCES.items():  # cada fuente fijada es la de la base, la de 524b293e y la de c8d69fcb (A-4 §2: «literales en 524b293e,
        if t in APPEND_SOURCES:   # iguales en c8d69fcb»); las de solo añadido, en una base posterior, empiezan por el texto fijado
            if not (inp.get("DecBase") or "").startswith(inp["Sources"][t]):
                f.append("fuente %s: en la base no empieza por el texto del blob fijado %s (no creció solo por añadido)" % (t, b))
        elif inp["SourceBlobsAtBase"].get(t) != b and t in EVOLVING_SOURCES:
            now = (inp.get("SourceTextsAtBase", {}).get(t) or "").split("\n")
            moved = sorted(n for n in cited.get(t, ()) if n > len(now) or now[n - 1] != src[t][n - 1])
            if moved:
                f.append("fuente %s: en la base (blob %s) cambian líneas citadas: %s" % (t, inp["SourceBlobsAtBase"].get(t), moved[:10]))
            else:
                info.append("fuente %s: blob en la base %s distinto del fijado %s (cambio fuera de A-4); las %d líneas citadas son idénticas" % (
                    t, (inp["SourceBlobsAtBase"].get(t) or "")[:8], b[:8], len(cited.get(t, ()))))
        elif inp["SourceBlobsAtBase"].get(t) != b:
            f.append("fuente %s: blob en la base %s distinto del fijado %s" % (t, inp["SourceBlobsAtBase"].get(t), b))
        for rev, blobs in inp["SourceBlobsAtPrep"].items():
            if blobs.get(t) != b:
                f.append("fuente %s: blob en %s %s distinto del fijado %s" % (t, rev, blobs.get(t), b))
    sec2 = section(a4, r"## 2\. ", r"## 3\. ")
    # bloque de §2: igualdad exacta línea a línea
    block_n, block = 0, []
    lines2 = sec2.split("\n")
    for i, l in enumerate(lines2):
        m = BLOCK_HEADER.match(l)
        if not m:
            continue
        nums = [int(x) for x in re.findall(r"L(\d+)", m.group(1))]
        j = i + 1
        while j < len(lines2) and lines2[j] == "":
            j += 1
        while j < len(lines2) and lines2[j].startswith(">"):
            block.append(lines2[j][2:] if lines2[j].startswith("> ") else "")
            j += 1
        if len(block) != len(nums):
            f.append("bloque de §2: %d líneas citadas para %d referencias" % (len(block), len(nums)))
        for n, q in zip(nums, block):
            block_n += 1
            if q != src["V14"][n - 1]:
                f.append("bloque de §2, V14 L%d: literal distinto de la fuente" % n)
    if block_n == 0:
        f.append("§2 sin el bloque literal de V14 D.3")
    # citas en línea: texto exacto en A-4, referencia declarada junto a la cita y fragmentos exactos en la fuente
    na = norm(re.sub(r"(?m)^>[ \t]?", "", a4))
    refs = [(m.start(), m.end(), int(m.group(1)), int(m.group(2) or m.group(1))) for m in REF.finditer(na)]
    checked, case_variants = [], set()
    for lid, tag, a, b, q, where in lit:
        occ = [m.start() for m in re.finditer(re.escape("«" + norm(q) + "»"), na)]
        row = {"Id": lid, "Source": tag, "Lines": "L%d" % a if a == b else "L%d-L%d" % (a, b), "Where": where, "Result": "PASS"}
        if not occ:
            ci = re.search(re.escape("«" + norm(q) + "»"), na, re.I)
            if not ci:
                f.append("%s: la cita no está en A-4 tal como la fija la guarda" % lid)
                row["Result"] = "FAIL"
                checked.append(row)
                continue
            got = na[ci.start() + 1:ci.end() - 1]  # A-4 la cita con otras mayúsculas: la guarda fija el literal de la fuente
            case_variants.add(got)
            f.append("%s (%s %s, %s): A-4 cita «%s»; la fuente dice «%s» (solo difiere en mayúsculas/minúsculas)" % (lid, tag, row["Lines"], where,
                                                                                                                       got, norm(q)))
            row["Result"] = "FAIL"
            row["Detail"] = "A-4 «%s» frente a la fuente «%s»" % (got, norm(q))
            occ = [ci.start()]
        if lid not in NO_REF:
            ok_ref = False
            for s in occ:
                e = s + len(norm(q)) + 2
                if lid in REF_AFTER:
                    cand = [r for r in refs if r[0] >= e and r[0] - e <= 60]
                    cand = cand[:1]
                else:
                    cand = [r for r in refs if r[1] <= s and s - r[1] <= 250]
                    cand = cand[-1:]
                if cand and (cand[0][2], cand[0][3]) == (a, b):
                    ok_ref = True
            if not ok_ref:
                f.append("%s: la referencia de línea junto a la cita no es L%d%s" % (lid, a, "" if a == b else "-L%d" % b))
                row["Result"] = "FAIL"
        seg = src[tag][a - 1:b]
        if tag in BQ_SOURCES:
            seg = [re.sub(r"^>[ \t]?", "", l) for l in seg]
        ok, tight = literal_match(q, seg)
        if not ok:
            why = ""
            if tight == "CASE_ONLY":
                hay = norm(" ".join(seg))
                i = hay.casefold().find(norm(q).casefold())
                why = " (solo difiere en mayúsculas/minúsculas: A-4 «%s», fuente «%s»)" % (norm(q), hay[i:i + len(norm(q))] if i >= 0 else "?")
            f.append("%s (%s %s, %s): literal distinto de la fuente%s" % (lid, tag, row["Lines"], where, why))
            row["Result"] = "FAIL"
            row["Detail"] = why.strip(" ()") or "fragmento ausente en el rango citado"
        elif not tight:
            info.append("%s (%s %s): la cita no toca todas las líneas del rango (rango más amplio que la cita)" % (lid, tag, row["Lines"]))
        checked.append(row)
    # cobertura: toda cita de A-4 (salvo los cinco párrafos del delta, sus citas internas aparte) está comprobada
    qs = quotes(a4)
    known = {norm(x[4]) for x in lit} | NOTATION | set(SELF_QUOTES) | case_variants
    uncovered, paragraphs = [], 0
    if qs is None:
        f.append("A-4 con comillas angulares desequilibradas")
        qs = []
    para_spans = []
    for s, e, d in qs:
        if d == 0 and re.match(r"^\*\*[^*]+\(A-4\)\.\*\*", unquote_md(a4[s + 1:e])):
            para_spans.append((s, e))
    paragraphs = len(para_spans)
    for s, e, d in qs:
        inside = [p for p in para_spans if p[0] <= s and e <= p[1]]
        if (s, e) in para_spans:
            continue
        if (d == 0 and not inside) or (d == 1 and inside):
            q = unquote_md(a4[s + 1:e])
            if q not in known:
                uncovered.append(q[:80])
    for q, rid in SELF_QUOTES.items():
        if not any(q in unquote_md(a4[s + 1:e]) for s, e in para_spans):
            f.append("cita propia «%s» ausente del texto normativo (%s)" % (q, rid))
    if uncovered:
        f.append("citas sin comprobar: %s" % uncovered)
    # testigos de referencias de línea sin cita (aviso)
    for tag, a, b, toks, ref in ref_tokens:
        hay = "\n".join(src[tag][a - 1:b])
        miss = [t for t in toks if t not in hay]
        if ref not in a4:
            warn.append("referencia «%s» no encontrada en A-4" % ref)
        if miss:
            warn.append("%s %s: testigos ausentes en la línea citada: %s" % (tag, ref, miss))
    # cabecera
    hdr = header_block(a4)
    for k, v in HEADER_FACTS:
        if v not in hdr:
            f.append("cabecera sin %s" % k)
    fc = inp["FreezeCommit"]
    if fc["V14"] != FROZEN[V14] or fc["Record"] != FROZEN[FREEZE] or fc["V14AtV14Commit"] != FROZEN[V14]:
        f.append("FREEZE_SHA / commit de V14 no contienen los blobs fijados de V14 y del registro")
    dec = inp["DecBase"] or ""
    s43 = section(dec, r"## 43\. ", r"## 44\. ")
    s57 = section(dec, r"## 57\. ", r"## 58\. ")
    s58 = section(dec, r"## 58\. ", r"## 59\. ")
    if "A-1" not in s43 or "AGREED" not in s43:
        f.append("decisiones §43 no registra el acuerdo de A-1")
    if "**1. A-2 = AGREED**" not in s57 or FROZEN[A2] not in s57:
        f.append("decisiones §57, punto 1, no registra A-2 AGREED con su blob")
    m3 = A3_HEADER.search(hdr)
    a3_state = None
    if not m3:
        f.append("cabecera sin la línea de A-3 (decisiones §n, punto m; blob …)")
    elif m3.group(1) == "AGREED":  # forma corregida: «A-3 AGREED (decisiones §61, punto 1; <ruta>, blob <40 hex>)», registrada en la base
        a3_state = "AGREED"
        s61 = section(dec, r"## 61\. ", r"## 62\. ")
        if A3_NEW_LINE not in hdr:
            f.append("cabecera: la línea de A-3 AGREED no es la fijada (decisiones §61, punto 1; ruta; blob completo)")
        if m3.group(4) != FROZEN[A3] or m3.group(5):
            f.append("cabecera: blob de A-3 distinto de %s o abreviado" % FROZEN[A3])
        if (m3.group(2), m3.group(3)) != ("61", "1") or "**1. A-3 = AGREED**" not in s61 or FROZEN[A3] not in s61:
            f.append("cabecera: A-3 AGREED no casa con decisiones §61, punto 1, en la base (A-3 = AGREED con su blob)")
    else:
        a3_state = m3.group(1)
        if (m3.group(2), m3.group(3)) != ("58", "3") or not m3.group(5):
            f.append("cabecera: la línea de A-3 no es la de la publicación (decisiones §58, punto 3; blob abreviado)")
        if not FROZEN[A3].startswith(m3.group(4)):
            f.append("cabecera: prefijo del blob de A-3 distinto de %s" % FROZEN[A3])
        if "**3. A-3**" not in s58 or FROZEN[A3] not in s58:
            f.append("decisiones §58, punto 3, no nombra la revisión formal de A-3 con su blob")
        if "AGREED" not in a3_state:
            later = inp.get("DecAlsoAt") or ""
            agreed_later = "**1. A-3 = AGREED**" in later and FROZEN[A3] in later
            info.append("cabecera: A-3 «%s» — %s al publicarse%s" % (
                a3_state, "cierto (la base no registra A-3 = AGREED)" if "A-3 = AGREED" not in dec else "NO cierto (la base ya registra A-3 = AGREED)",
                "; desde decisiones §61, punto 1, A-3 = AGREED con el mismo blob ea6721f7 (registro en --also-at); la cabecera queda desfasada, "
                "no errónea para su fecha" if agreed_later else ""))
    # prefijos de blob abreviados
    for pre, p in SHORT_BLOBS + (SHORT_BLOBS_C2 if c2 else []):
        if not re.search(re.escape(pre) + "…", a4):
            f.append("A-4 no cita el prefijo %s… (%s)" % (pre, p))
        got = inp["ShortBlobsAtBase"].get(p)
        pinned = FROZEN.get(p) or DECLARED.get(p) or ""
        if (not got or not got.startswith(pre)) and not (p in EVOLVING and pinned.startswith(pre)):
            f.append("prefijo %s… distinto del blob de %s en la base (%s)" % (pre, p, got))
    # privacidad
    for name, text in (("A-4", inp["A4"]), ("paquete de A-4", inp["PKG"])):
        if text is None:
            f.append("%s ausente en head" % name)
        else:
            f += privacy_findings(name, text, ids)
    n2 = sum(1 for x in lit if x[5] == "§2")
    return {"Check": "G1 literales, cabecera y privacidad", "BlockLines": block_n, "Version": inp.get("Version"), "InlineLiterals": len(lit), "InlineLiteralsSection2": n2,
            "Paragraphs": paragraphs, "Literals": checked, "A3HeaderState": a3_state, "Info": info, "Warnings": warn, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G2
def g2_paths(inp, ids):
    f, info = [], []
    base, head, st = inp["Base"], inp["Head"], inp["Status"]
    if base != A4_BASE:
        f.append("base distinta de A4_BASE")
    if head != A4_PUB:
        f.append("head distinto del commit de publicación A4_PUB")
    if inp["HeadParent"] != base:
        f.append("base distinta del padre de head")
    for p in REQUIRED_ADDED:
        if st.get(p) != "A":
            f.append("%s no aparece como añadido (A)" % p)
    if inp["HeadBlobs"].get(A4) != A4_BLOB:
        f.append("blob de A-4 en head distinto de A4_BLOB")
    if inp["HeadBlobs"].get(PKG) != PKG_BLOB:
        f.append("blob del paquete en head distinto de PKG_BLOB")
    bad = sorted(p for p in st if p not in ALLOWED_STATUS)
    if bad:
        f.append("rutas no permitidas: %s" % bad)
    for p, s in sorted(st.items()):
        if p in ALLOWED_STATUS and s not in ALLOWED_STATUS[p]:
            f.append("%s con estado %s (permitido: %s)" % (p, s, sorted(ALLOWED_STATUS[p])))
    grown = {}
    for p in APPEND_ONLY:
        if p in st:
            new, old = inp["HeadTexts"].get(p) or "", inp["BaseTexts"].get(p) or ""
            if st[p] != "M" or not new.startswith(old):
                f.append("%s no cambia solo por añadido" % p)
            else:
                grown[p] = {"BaseBytes": len(old.encode("utf-8")), "HeadBytes": len(new.encode("utf-8"))}
    priv = []
    for p, s in sorted(st.items()):
        if s == "A":
            text = inp["HeadTexts"].get(p) or ""
        elif s == "M" and p in APPEND_ONLY:
            new, old = inp["HeadTexts"].get(p) or "", inp["BaseTexts"].get(p) or ""
            text = new[len(old):] if new.startswith(old) else new
        elif s == "M":
            text = inp["HeadTexts"].get(p) or ""
        else:
            continue
        priv.append(p)
        f += privacy_findings(p, text, ids)
    extra = sorted(p for p in inp["PrepStatus"] if p not in ALLOWED_A4_S6 and p not in st)
    if extra:
        stray = [p for p in extra if p not in inp["PrepToBase"]]
        if stray:
            f.append("rutas del diff %s..head que no vienen de %s..base ni de la publicación: %s" % (PREP_BASE[:8], PREP_BASE[:8], stray))
        prefixes = sorted({"/".join(p.split("/")[:5]) for p in extra})
        info.append("A-4 §6 declara el diff permitido «sobre 524b293e»; el padre real es %s: %d rutas de %s..%s (fuera de la lista de §6) vienen "
                    "de ese intervalo, no de la publicación (%s)" % (A4_BASE[:8], len(extra), PREP_BASE[:8], A4_BASE[:8], ", ".join(prefixes)))
    if REQUEST in st:
        info.append("la solicitud única de FX-02 (añadida) no figura nominalmente en A-4 §6; es evidencia de I-62 por añadido")
    return {"Check": "G2 rutas del commit de publicación", "Base": base, "Head": head, "Changed": st, "AppendOnlyGrowth": grown,
            "PrivacyChecked": priv, "Info": info, "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G3
def g3_frozen(inp, labels=("Base", "Head")):
    want = dict(FROZEN)
    want.update(DECLARED)
    mism, info = {}, []
    for lab in labels:
        got = inp["Blobs"].get(lab, {})
        for p, w in sorted(want.items()):
            if got.get(p) != w and p in EVOLVING:  # kit de FX-02: lo pueden cambiar otros commits; G1 comprueba sus líneas citadas
                info.append("%s:%s con blob %s distinto del fijado %s (cambio fuera de A-4; líneas citadas en G1)" % (lab, p, (got.get(p) or "")[:8], w[:8]))
            elif got.get(p) != w:
                mism["%s:%s" % (lab, p)] = {"Expected": w, "Got": got.get(p)}
    if "Base" in labels and "Head" in labels:
        for p in sorted(EVOLVING):
            if inp["Blobs"]["Base"].get(p) != inp["Blobs"]["Head"].get(p):
                mism["Base->Head:" + p] = {"Expected": inp["Blobs"]["Base"].get(p), "Got": inp["Blobs"]["Head"].get(p)}
        if lab == "AlsoAt" and got.get(A4) != A4_BLOB:
            mism["AlsoAt:" + A4] = {"Expected": A4_BLOB, "Got": got.get(A4)}
    f = ["blob distinto en %s: esperado %s, obtenido %s" % (k, v["Expected"], v["Got"]) for k, v in sorted(mism.items())]
    return {"Check": "G3 blobs congelados y declarados sin cambio (%s)" % ", ".join(labels), "Paths": len(want), "Mismatches": mism,
            "Info": info, "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G4
def g4_c20b(inp, cm):
    in_surf = sorted(p for p in inp["Status"] if cm.in_surfaces(p))
    r = inp["C20b"]
    equal = r["ExitCode"] == 0 and "EQUAL" in r["Output"]
    f = []
    if in_surf:
        f.append("rutas cambiadas en las superficies del mapa: %s" % in_surf)
    if not equal:
        f.append("clause_map.py check distinto de EQUAL (código %s)" % r["ExitCode"])
    return {"Check": "G4 C-20b", "Base": r["Base"], "SurfacesTouched": in_surf, "ExitCode": r["ExitCode"], "Output": r["Output"][-300:],
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G2C / G2D: corrección previa a la revisión
def apply_edits(old, edits, a4blob):
    """Texto esperado tras las ediciones declaradas (de abajo arriba); ([] , texto) o (hallazgos, None) si una edición no casa."""
    lines, f = old.split("\n"), []
    for e in sorted(edits, key=lambda x: -x[1]):
        kind, n = e[0], e[1]
        if kind == "sub":
            if lines[n - 1].count(e[2]) != 1:
                f.append("edición declarada no aplicable en L%d" % n)
                continue
            lines[n - 1] = lines[n - 1].replace(e[2], e[3], 1)
        else:
            olds, news = e[2], [x.replace("{a4blob}", a4blob) for x in e[3]]
            if lines[n - 1:n - 1 + len(olds)] != olds:
                f.append("edición declarada no aplicable en L%d-L%d" % (n, n + len(olds) - 1))
                continue
            lines[n - 1:n - 1 + len(olds)] = news
    return (f, None) if f else ([], "\n".join(lines))


def hunks(old, new):
    out = []
    a, b = old.split("\n"), new.split("\n")
    for op, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        if op != "equal":
            out.append({"Op": op, "Old": "L%d-L%d" % (i1 + 1, i2) if i2 > i1 else "L%d+" % i1, "New": "L%d-L%d" % (j1 + 1, j2) if j2 > j1 else "-",
                        "Removed": a[i1:i2], "Added": b[j1:j2]})
    return out


def diff_limit(inp):
    """El diff de A-4 frente a 27ffa26b es exactamente A4_EDITS y el del paquete frente a d314afeb, exactamente PKG_EDITS."""
    f = []
    a4o, a4n, po, pn = inp.get("A4Prev") or "", inp.get("A4") or "", inp.get("PkgPrev") or "", inp.get("PKG") or ""
    if blob_id(a4o.encode("utf-8")) != A4_BLOB or blob_id(po.encode("utf-8")) != PKG_BLOB:
        f.append("los textos anteriores no son los blobs publicados 27ffa26b / d314afeb")
    a4b, pb = blob_id(a4n.encode("utf-8")), blob_id(pn.encode("utf-8"))
    for name, old, new, edits in (("A-4", a4o, a4n, A4_EDITS), ("paquete", po, pn, PKG_EDITS)):
        ef, want = apply_edits(old, edits, a4b)
        f += ["%s: %s" % (name, x) for x in ef]
        if want is not None and new != want:
            extra = hunks(want, new)
            f.append("%s: el diff frente al blob publicado no es exactamente el declarado (%d diferencias de más o de menos, primera en %s)" % (
                name, len(extra), extra[0]["Old"] if extra else "?"))
    if a4b != A4_CORR_BLOB:
        f.append("blob de la A-4 corregida %s distinto de A4_CORR_BLOB" % a4b)
    if pb != PKG_CORR_BLOB:
        f.append("blob del paquete corregido %s distinto de PKG_CORR_BLOB" % pb)
    return f, {"A4": {"Blob": a4b, "Hunks": hunks(a4o, a4n)}, "Package": {"Blob": pb, "Hunks": hunks(po, pn)}}


def g2_diff_only(inp, ids):
    f, dl = diff_limit(inp)
    rb = inp.get("RepoBlobs") or {}
    if rb.get(A4) != A4_BLOB or rb.get(PKG) != PKG_BLOB:
        f.append("la HEAD del repositorio ya no tiene A-4 = 27ffa26b y paquete = d314afeb: la corrección no se aplica sobre ella")
    return {"Check": "G2D límite del diff de la corrección (copias frente a 27ffa26b / d314afeb)", "Diff": dl, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


def g2_correction(inp, ids):
    f, info = [], []
    base, head, st = inp["Base"], inp["Head"], inp["Status"]
    if inp["HeadParent"] != base:
        f.append("base distinta del padre de head")
    if inp["BaseBlobs4"].get(A4) != A4_BLOB or inp["BaseBlobs4"].get(PKG) != PKG_BLOB:
        f.append("la base no contiene la A-4 publicada (27ffa26b) y su paquete (d314afeb)")
    for p, s in CORR_REQUIRED.items():
        if st.get(p) != s:
            f.append("%s no aparece como %s" % (p, "modificado (M)" if s == "M" else "añadido (A)"))
    bad = sorted(p for p in st if p not in CORR_STATUS)
    if bad:
        f.append("rutas no permitidas: %s" % bad)
    for p, s in sorted(st.items()):
        if p in CORR_STATUS and s not in CORR_STATUS[p]:
            f.append("%s con estado %s (permitido: %s)" % (p, s, sorted(CORR_STATUS[p])))
    for p in (A4, PKG):
        if (inp["HeadTexts"].get(p) or "") != (inp["A4"] if p == A4 else inp["PKG"]):
            f.append("%s: texto de head incoherente" % p)
    df, dl = diff_limit(inp)
    f += df
    if inp["HeadBlobs"].get(A4) != A4_CORR_BLOB or inp["HeadBlobs"].get(PKG) != PKG_CORR_BLOB:
        f.append("blobs de A-4 y del paquete en head distintos de A4_CORR_BLOB / PKG_CORR_BLOB")
    grown = {}
    for p in APPEND_ONLY:
        if p in st:
            new, old = inp["HeadTexts"].get(p) or "", inp["BaseTexts"].get(p) or ""
            if st[p] != "M" or not new.startswith(old):
                f.append("%s no cambia solo por añadido" % p)
            else:
                grown[p] = {"BaseBytes": len(old.encode("utf-8")), "HeadBytes": len(new.encode("utf-8"))}
    priv = []
    for p, s in sorted(st.items()):
        if s == "A" or (s == "M" and p not in APPEND_ONLY):
            text = inp["HeadTexts"].get(p) or ""
        elif s == "M":
            new, old = inp["HeadTexts"].get(p) or "", inp["BaseTexts"].get(p) or ""
            text = new[len(old):] if new.startswith(old) else new
        else:
            continue
        priv.append(p)
        f += privacy_findings(p, text, ids)
    if STATE in st:
        info.append("estado modificado (permitido como en las guardas de A-2 y A-3; no es un archivo de solo añadido)")
    return {"Check": "G2C rutas y límite del diff del commit de corrección", "Base": base, "Head": head, "Changed": st, "Diff": dl,
            "AppendOnlyGrowth": grown, "PrivacyChecked": priv, "Info": info, "Findings": f, "Result": "PASS" if not f else "FAIL"}


def g5b_selftest(inp):  # el self-test custodiado corresponde a estas guardas y a la A-4 y el paquete exactos de head
    f = []
    try:
        st = json.loads(inp["HeadTexts"].get(SELFTEST) or "")
    except ValueError:
        st, f = {}, ["a4-selftest.json ilegible en head"]
    hb = inp["HeadBlobs"]
    if st.get("Result") != "PASS":
        f.append("a4-selftest.json sin Result PASS")
    if st.get("ToolBlob") != hb.get(GUARDS):
        f.append("ToolBlob distinto del blob de a4-guards.py en head")
    if st.get("A4TextBlob") != hb.get(A4):
        f.append("A4TextBlob distinto del blob de A-4 en head")
    if st.get("PkgTextBlob") != hb.get(PKG):
        f.append("PkgTextBlob distinto del blob del paquete en head")
    return {"Check": "G5b self-test custodiado", "ToolBlob": st.get("ToolBlob"), "A4TextBlob": st.get("A4TextBlob"), "PkgTextBlob": st.get("PkgTextBlob"),
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G2C2 / G2D2: corrección 2 (A62-A4-01 y A62-A4-O1..O8)
def md_regions(md):
    """Regiones de A-4: título, campos de la cabecera («header:<campo>») y secciones por número (§1..§10, §3.1..§3.7, Anexo)."""
    out, key, state = {}, "title", "pre"
    for l in md.split("\n"):
        if state == "pre" and l == "```text":
            state, key = "hdr", "header:(inicio)"
        elif state == "hdr" and l == "```":
            state, key = "body", "header:(fin)"
        elif state == "hdr":
            m = re.match(r"^([A-Z][A-Za-z /-]*):", l)
            if m and (not out.get(key) or out[key][-1] == "" or key == "header:(inicio)"):
                key = "header:" + m.group(1)
        else:
            m = re.match(r"^#{2,3} (\d+(?:\.\d+)?|Anexo)\b", l)
            if m:
                key = "Anexo" if m.group(1) == "Anexo" else "§" + m.group(1)
        out.setdefault(key, []).append(l)
    return {k: "\n".join(v) for k, v in out.items()}


def para_items(para):
    """{«intro», «1», «2», …}: el texto de cada regla numerada de un párrafo normativo (líneas tal cual)."""
    items, label, cur = {}, "intro", []
    for l in para.split("\n"):
        m = re.match(r"^(\d+)\. ", l)
        if m:
            items[label] = "\n".join(cur)
            label, cur = m.group(1), [l]
        else:
            cur.append(l)
    items[label] = "\n".join(cur)
    return items


def tokens(text):
    return re.findall(r"\w+|[^\w\s]", text)


def is_insertion(old, new):
    """El texto anterior es subsecuencia del nuevo, por palabras y signos: la edición solo inserta."""
    it = iter(tokens(new))
    return all(any(t == u for u in it) for t in tokens(old))


def diff_limit2(inp):
    """La A-4 de la corrección 2 frente a 7d863219 cambia solo en las regiones declaradas (C2_REGIONS) y, dentro de §3.2 y §3.5, solo en
    A4-1 (reglas 2 y 3, por inserción) y A4-4 (regla 5); contiene lo que exige A62-A4-01 y ya no lo que retira; el paquete nombra el blob nuevo."""
    f = []
    old, new, po, pn = inp.get("A4Prev") or "", inp.get("A4") or "", inp.get("PkgPrev") or "", inp.get("PKG") or ""
    if blob_id(old.encode("utf-8")) != A4_CORR_BLOB or blob_id(po.encode("utf-8")) != PKG_CORR_BLOB:
        f.append("los textos anteriores no son los blobs 7d863219 / 085f30f6")
    ro, rn = md_regions(old), md_regions(new)
    if set(ro) != set(rn):
        f.append("aparecen o desaparecen regiones: %s" % sorted(set(ro) ^ set(rn)))
    changed = sorted(k for k in ro if k in rn and ro[k] != rn[k])
    outside = [k for k in changed if k not in C2_REGIONS]
    if outside:
        f.append("cambios fuera de las regiones declaradas: %s" % outside)
    po_r, pn_r = a4_rules(old), a4_rules(new)
    for rid, sec in (("A4-1", "§3.2"), ("A4-4", "§3.5")):
        strip = lambda t: "\n".join(l for l in t.split("\n") if not l.startswith(">"))
        if strip(ro.get(sec, "")) != strip(rn.get(sec, "")):
            f.append("%s: cambia texto fuera del párrafo normativo" % sec)
        io, inn = para_items((po_r.get(rid) or {}).get("Paragraph", "")), para_items((pn_r.get(rid) or {}).get("Paragraph", ""))
        if set(io) != set(inn):
            f.append("%s: cambian las reglas numeradas" % rid)
            continue
        bad = sorted(k for k in io if io[k] != inn[k] and k not in C2_ITEMS[rid])
        if bad:
            f.append("%s: ítems no declarados cambian: %s" % (rid, bad))
        for k, terms in C2_ITEMS[rid].items():
            nn = norm(inn.get(k, ""))
            miss = [t for t in terms if norm(t) not in nn]
            if io.get(k) == inn.get(k):
                f.append("%s, regla %s: no cambia (precisión declarada ausente)" % (rid, k))
            if miss:
                f.append("%s, regla %s: faltan términos de la precisión: %s" % (rid, k, miss))
            if (rid, k) in C2_INSERT_ONLY and not is_insertion(io.get(k, ""), inn.get(k, "")):
                f.append("%s, regla %s: la precisión no es solo inserción (borra texto anterior)" % (rid, k))
    nt = norm(new)
    f += ["frase exigida ausente: «%s»" % x[:70] for x in C2_REQUIRED_PHRASES if norm(x) not in nt]
    f += ["frase prohibida presente: «%s»" % x[:70] for x in C2_FORBIDDEN_PHRASES if norm(x) in nt]
    a4b, pb = blob_id(new.encode("utf-8")), blob_id(pn.encode("utf-8"))
    if "(borrador de esta pasada: %s)" % a4b not in pn:
        f.append("paquete: no nombra el blob de la A-4 corregida (%s)" % a4b)
    if "A62-A4-01" not in pn or "7d863219" not in pn:
        f.append("paquete: sin el hallazgo a re-revisar (A62-A4-01) o sin el delta frente a 7d863219")
    if a4b != A4_CORR2_BLOB:
        f.append("blob de la A-4 de la corrección 2 %s distinto de A4_CORR2_BLOB" % a4b)
    if pb != PKG_CORR2_BLOB:
        f.append("blob del paquete de la corrección 2 %s distinto de PKG_CORR2_BLOB" % pb)
    hk = hunks(old, new)
    return f, {"A4": {"Blob": a4b, "ChangedRegions": changed, "Hunks": [{"Op": h["Op"], "Old": h["Old"], "New": h["New"]} for h in hk]},
               "Package": {"Blob": pb, "Hunks": [{"Op": h["Op"], "Old": h["Old"], "New": h["New"]} for h in hunks(po, pn)]}}


def g2_diff_only2(inp, ids):
    f, dl = diff_limit2(inp)
    rb = inp.get("RepoBlobs") or {}
    if rb.get(A4) != A4_CORR_BLOB or rb.get(PKG) != PKG_CORR_BLOB:
        f.append("la HEAD del repositorio no tiene A-4 = 7d863219 y paquete = 085f30f6: la corrección 2 no se aplica sobre ella")
    for name, text in (("A-4 (copia)", inp.get("A4") or ""), ("paquete (copia)", inp.get("PKG") or "")):
        f += privacy_findings(name, text, ids)
    return {"Check": "G2D2 límite del diff de la corrección 2 (copias frente a 7d863219 / 085f30f6)", "Diff": dl, "Findings": f,
            "Result": "PASS" if not f else "FAIL"}


def g2_correction2(inp, ids):
    f, info = [], []
    base, head, st = inp["Base"], inp["Head"], inp["Status"]
    if inp["HeadParent"] != base:
        f.append("base distinta del padre de head")
    if inp["BaseBlobs4"].get(A4) != A4_CORR_BLOB or inp["BaseBlobs4"].get(PKG) != PKG_CORR_BLOB:
        f.append("la base no contiene la A-4 corregida (7d863219) y su paquete (085f30f6)")
    for p_, s_ in CORR2_REQUIRED.items():
        if st.get(p_) != s_:
            f.append("%s no aparece como modificado (M)" % p_)
    bad = sorted(p_ for p_ in st if p_ not in CORR2_STATUS)
    if bad:
        f.append("rutas no permitidas: %s" % bad)
    for p_, s_ in sorted(st.items()):
        if p_ in CORR2_STATUS and s_ not in CORR2_STATUS[p_]:
            f.append("%s con estado %s (permitido: %s)" % (p_, s_, sorted(CORR2_STATUS[p_])))
    for p_ in (A4, PKG):
        if (inp["HeadTexts"].get(p_) or "") != (inp["A4"] if p_ == A4 else inp["PKG"]):
            f.append("%s: texto de head incoherente" % p_)
    df, dl = diff_limit2(inp)
    f += df
    if inp["HeadBlobs"].get(A4) != A4_CORR2_BLOB or inp["HeadBlobs"].get(PKG) != PKG_CORR2_BLOB:
        f.append("blobs de A-4 y del paquete en head distintos de A4_CORR2_BLOB / PKG_CORR2_BLOB")
    grown = {}
    for p_ in APPEND_ONLY:
        if p_ in st:
            new, old = inp["HeadTexts"].get(p_) or "", inp["BaseTexts"].get(p_) or ""
            if st[p_] != "M" or not new.startswith(old):
                f.append("%s no cambia solo por añadido" % p_)
            else:
                grown[p_] = {"BaseBytes": len(old.encode("utf-8")), "HeadBytes": len(new.encode("utf-8"))}
    priv = []
    for p_, s_ in sorted(st.items()):
        if s_ == "A" or (s_ == "M" and p_ not in APPEND_ONLY):
            text = inp["HeadTexts"].get(p_) or ""
        elif s_ == "M":
            new, old = inp["HeadTexts"].get(p_) or "", inp["BaseTexts"].get(p_) or ""
            text = new[len(old):] if new.startswith(old) else new
        else:
            continue
        priv.append(p_)
        f += privacy_findings(p_, text, ids)
    return {"Check": "G2C2 rutas y regiones del commit de la corrección 2", "Base": base, "Head": head, "Changed": st, "Diff": dl,
            "AppendOnlyGrowth": grown, "PrivacyChecked": priv, "Info": info, "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- G5
def a2_section(md, title):
    m = re.search(r"^### %s\n(.*?)(?=^### |^## )" % re.escape(title), md, re.S | re.M)
    return m.group(1) if m else ""


def quoted_paragraph(block):  # como en a2-guards.py
    lines = [l[2:] if l.startswith("> ") else ("" if l == ">" else None) for l in block.split("\n")]
    q = "\n".join(l for l in lines if l is not None).strip()
    return q[1:-1] if q.startswith("«") and q.endswith("»") else q


def a4_rules(a4):
    """{id: (párrafo, ubicación)} de §3.2-§3.6, en orden."""
    out = {}
    for m in re.finditer(r"^### 3\.(\d) (A4-\d) — [^\n]*\n(.*?)(?=^### |^## )", a4, re.S | re.M):
        body = m.group(3)
        loc = next((l for l in body.split("\n") if l.startswith("Ubicación:")), "")
        para = quoted_paragraph("\n".join(l for l in body.split("\n") if l.startswith(">")))
        out[m.group(2)] = {"Section": "3.%s" % m.group(1), "Paragraph": para, "Location": loc}
    return out


def apply_a2(v14, a2):
    p1 = quoted_paragraph("\n".join(l for l in a2_section(a2, "2.3 Delta exacto").split("\n") if l.startswith(">")))
    p2 = quoted_paragraph("\n".join(l for l in a2_section(a2, "3.3 Delta exacto").split("\n") if l.startswith(">")))
    return v14.replace(ANCHOR_V14, ANCHOR_V14 + "\n\n" + p1 + "\n\n" + p2, 1), p1, p2


def changed_sections(cm, before, after):
    b = {(lv, h): t for lv, h, t in cm.sections(before)}
    a = {(lv, h): t for lv, h, t in cm.sections(after)}
    return sorted(set(a) - set(b)), sorted(set(b) - set(a)), sorted(k for k in b if k in a and b[k] != a[k])


def clauses(para):
    """[(ítem, frase)]: ítems numerados y frases separadas por «.» o «;» seguidos de espacio."""
    items, cur, label = [], [], "intro"
    for l in para.split("\n"):
        m = re.match(r"^(\d+)\. (.*)$", l)
        if m:
            items.append((label, " ".join(cur)))
            label, cur = m.group(1), [m.group(2)]
        else:
            cur.append(l)
    items.append((label, " ".join(cur)))
    out = []
    for lab, txt in items:
        for c in re.split(r"(?<=[.;])\s+(?=\S)", norm(txt)):
            if c:
                out.append((lab, c))
    return out


def strip_quotes(text, keep_out):
    """Quita del texto las citas «…» cuyo contenido está en keep_out (exenciones verificadas)."""
    for q in keep_out:
        text = text.replace("«" + q + "»", "«»")
    return text


def g5_delta(inp, cm, a4_text=None):
    f, info = [], []
    a4 = inp["A4"] if a4_text is None else a4_text
    v14, a2 = inp["Sources"]["V14"], inp["Sources"]["A2"]
    rules = a4_rules(a4 or "")
    if list(rules) != RULE_IDS:
        f.append("§3.2-§3.6 no contienen exactamente A4-1..A4-5 en orden: %s" % list(rules))
    paras = [rules[r]["Paragraph"] for r in RULE_IDS if r in rules]
    pick = lambda d, r, k: (d.get(r) or {}).get(k)
    if inp.get("A4Prev") is not None and inp.get("Version") == "C2":  # corrección 2: frente a 7d863219, solo A4-1 (2, 3) y A4-4 (5)
        prev, moved = a4_rules(inp["A4Prev"]), []
        for r in RULE_IDS:
            if pick(prev, r, "Location") != pick(rules, r, "Location"):
                moved.append(r + " (ubicación)")
            io, inn = para_items(pick(prev, r, "Paragraph") or ""), para_items(pick(rules, r, "Paragraph") or "")
            if set(io) != set(inn):
                moved.append(r + " (ítems)")
                continue
            bad = sorted(k for k in io if io[k] != inn[k] and k not in C2_ITEMS.get(r, {}))
            if bad:
                moved.append("%s (ítems %s)" % (r, bad))
        if moved:
            f.append("cambian frente a 7d863219 partes de las reglas que la corrección 2 no declara: %s" % moved)
        else:
            info.append("frente a 7d863219: A4-2, A4-3 y A4-5 idénticos; A4-4 cambia solo en la regla 5 y A4-1 solo en las reglas 2 y 3")
    elif inp.get("A4Prev") is not None:  # corrección previa a la revisión: el delta no cambia frente a la A-4 publicada (27ffa26b)
        prev = a4_rules(inp["A4Prev"])
        moved = [r for r in RULE_IDS if pick(prev, r, "Paragraph") != pick(rules, r, "Paragraph") or pick(prev, r, "Location") != pick(rules, r, "Location")]
        if moved:
            f.append("los párrafos normativos o sus ubicaciones cambian frente a 27ffa26b: %s" % moved)
        else:
            info.append("los cinco párrafos normativos y sus ubicaciones son idénticos a los de 27ffa26b")
    for r in RULE_IDS:
        if r in rules and not rules[r]["Location"].startswith(LOCATIONS[r]):
            f.append("%s: ubicación distinta de «%s»" % (r, LOCATIONS[r]))
    a2_lines = a2.split("\n")
    if not a2_lines[233].endswith(ANCHOR_A2_END + "»"):
        f.append("A-2 L234 no termina el párrafo de A2-P2 en «%s»" % ANCHOR_A2_END)
    # V14 + A-2 (como a2-guards.py) y comprobación de que A-2 queda al final de D.3
    if v14.count(ANCHOR_V14) != 1 or not (v14.index(D3) < v14.find(ANCHOR_V14) < v14.index(D4)):
        f.append("ancla de A-2 ausente, repetida o fuera de D.3")
    base, p1, p2 = apply_a2(v14, a2)
    if not p2.endswith(ANCHOR_A2_END):
        f.append("el párrafo de A2-P2 aplicado no termina en «%s»" % ANCHOR_A2_END)
    if base.count(ANCHOR_A2_END) != 1:
        f.append("«%s» aparece %d veces en V14 + A-2 (ancla no única)" % (ANCHOR_A2_END, base.count(ANCHOR_A2_END)))
    pos = base.find(ANCHOR_A2_END) + len(ANCHOR_A2_END)
    if base[pos:base.index(D4)].strip():
        f.append("tras «%s» hay texto antes de D.4: A-2 no termina D.3" % ANCHOR_A2_END)
    a2_add, a2_rem, a2_chg = changed_sections(cm, v14, base)
    pattern = {k for k in a2_chg}
    # aplicación de A-4 (y variante sin A4-5, separable)
    variants = {"A4-1..A4-5": paras, "A4-1..A4-4 (sin A4-5)": paras[:4]}
    applied_info = {}
    for name, ps in variants.items():
        if not ps:
            continue
        ins = "\n\n" + "\n\n".join(ps)
        applied = base.replace(ANCHOR_A2_END, ANCHOR_A2_END + ins, 1)
        bl, al = base.split("\n"), applied.split("\n")
        ops = [o for o in difflib.SequenceMatcher(None, bl, al, autojunk=False).get_opcodes() if o[0] != "equal"]
        pure = len(ops) == 1 and ops[0][0] == "insert" and applied.replace(ins, "", 1) == base
        if not pure:
            f.append("%s: la aplicación no es un añadido puro (%s)" % (name, [o[0] for o in ops]))
        v_ops = [o[0] for o in difflib.SequenceMatcher(None, v14.split("\n"), al, autojunk=False).get_opcodes() if o[0] != "equal"]
        if set(v_ops) - {"insert"}:
            f.append("%s: frente a V14 hay operaciones distintas de añadir: %s" % (name, sorted(set(v_ops))))
        add, rem, chg = changed_sections(cm, base, applied)
        if add or rem:
            f.append("%s: al aplicar aparecen o desaparecen secciones: %s %s" % (name, add, rem))
        if set(chg) != pattern:
            f.append("%s: secciones cambiadas %s distintas de las de A-2 %s" % (name, [k[1][:40] for k in chg], [k[1][:40] for k in pattern]))
        if not any(k[1] == cm.norm(D3) for k in chg):
            f.append("%s: al aplicar no cambia D.3" % name)
        if applied.count(CAPS) != 1 or al[CAPS_LINE - 1] != CAPS:
            f.append("%s: el literal de topes de «Codex, total» cambia, se repite o se mueve" % name)
        bad_lines = [l for l in "\n".join(ps).split("\n") if re.match(r"^\s*(\||#|```)", l)]
        if bad_lines:
            f.append("%s: el delta trae filas de tabla, encabezados o vallas: %s" % (name, [l[:40] for l in bad_lines]))
        applied_info[name] = {"PureAppend": pure, "InsertedLines": (ops[0][4] - ops[0][3]) if ops else 0,
                              "InsertedAfterLine": ops[0][1] if ops else None, "ChangedSections": [k[1][:80] for k in chg]}
    if v14.split("\n")[CAPS_LINE - 1] != CAPS or v14.count(CAPS) != 1:
        f.append("V14 L%d no es el literal de topes fijado" % CAPS_LINE)
    if len(paras) == 5 and any("A4-5" in p for p in paras[:4]):
        f.append("A4-1..A4-4 mencionan A4-5: A4-5 no es separable")
    # cláusulas de alcance, nombres de producto, citas internas, cifras de topes y menciones fuera de alcance
    scope_rows, mentions, invoked, inner, exempt_used = [], [], [], [], []
    allsrc = {t: txt.split("\n") for t, txt in inp["Sources"].items()}
    for rid, p in zip(RULE_IDS, paras):
        np_ = norm(p)
        m = re.match(r"^\*\*([^*]+) \(A-4\)\.\*\* (.*)$", np_)
        if not m:
            f.append("%s: sin título «**… (A-4).**»" % rid)
            continue
        first = re.split(r"(?<=\.)\s+(?=\S)", m.group(2))[0]
        miss = [t for t in SCOPE[rid] if t not in first]
        opens = first.startswith("Solo para")
        other = re.findall(CROSS[0][1], first)
        ok = opens and not miss and not other
        scope_rows.append({"Rule": rid, "Title": m.group(1), "ScopeClause": first, "Missing": miss, "OtherScenarios": other,
                           "Result": "PASS" if ok else "FAIL"})
        if not ok:
            f.append("%s: la regla no abre con su cláusula de alcance (abre con «Solo para»: %s; faltan %s; otros escenarios %s)" % (
                rid, opens, miss, other))
        # citas internas
        qs = quotes(p) or []
        verified = []
        for s, e, d in qs:
            if d != 0:
                continue
            q = norm(p[s + 1:e])
            where = [(t, i + 1) for t, ls in allsrc.items() for i, l in enumerate(ls) if q in l]
            inner.append({"Rule": rid, "Quote": q, "FoundIn": ["%s L%d" % w for w in where[:3]], "Result": "PASS" if where else "FAIL"})
            if not where:
                f.append("%s: cita interna «%s» que no está en ninguna fuente fijada" % (rid, q))
            else:
                verified.append(q)
        ex = [q for q in PRODUCT_EXEMPT.get(rid, []) if q in verified]
        for q in ex:
            exempt_used.append({"Rule": rid, "Quote": q, "Source": "V14 L%d" % CAPS_LINE})
        prods = sorted(set(x.group(0) for x in PRODUCT_TOKENS.finditer(strip_quotes(p, ex))))
        if prods:
            f.append("%s: nombres de producto en la regla: %s" % (rid, prods))
        # cifras de topes en la regla, iguales al literal de V14
        for pat, want in ((r"\((\d+) base", "11"), (r"pool \**(\d+)\**", "4"), (r"≤ (\d+) por fase", "2"), (r"\btope (\d+)\b", "15")):
            for x in re.findall(pat, np_):
                if x != want:
                    f.append("%s: cifra de tope %s distinta de V14 (%s)" % (rid, x, want))
        # invalidadores de A4-1 = los de A-2 (lectura vigente, L238-L239)
        if rid == "A4-1":
            mi = re.search(r"cualquier cambio de un invalidador \(([^)]*)\)", np_)
            a2inv = norm(" ".join(a2_lines[237:239]))
            a2m = re.search(r"Los invalidadores son (.*?)\. Si cambia", a2inv)
            items = lambda s: sorted(x.strip() for x in re.split(r",\s*(?:e\s+|y\s+)?|\s+e\s+(?=instancia)", s) if x.strip())
            if not mi or not a2m or items(mi.group(1)) != items(a2m.group(1)):
                f.append("A4-1: invalidadores distintos de los de A-2 L238-L239")
            else:
                info.append("A4-1, regla 5: invalidadores iguales a los de A-2 L238-L239 (%d)" % len(items(mi.group(1))))
        # menciones
        for item, c in clauses(p):
            for cat, pat in CROSS:
                for x in re.finditer(pat, c):
                    tok = x.group(0)
                    if cat == "ROL" and tok in IN_SCOPE_ROLES[rid]:
                        continue
                    mk = MARK_PRES.search(c) or MARK_NEG.search(c) or MARK_RESTR.search(c)
                    kind = None
                    if MARK_PRES.search(c):
                        kind = "CONSERVACIÓN"
                    elif MARK_NEG.search(c):
                        kind = "NEGATIVA"
                    elif MARK_RESTR.search(c):
                        kind = "RESTRICCIÓN"
                    mentions.append({"Rule": rid, "Item": item, "Category": cat, "Token": tok, "Statement": kind or "NINGUNA",
                                     "Marker": mk.group(0) if mk else None, "Sentence": c, "Result": "PASS" if kind else "FAIL"})
                    if not kind:
                        f.append("%s, ítem %s: mención de %s «%s» fuera de una frase negativa, de conservación o de restricción" % (
                            rid, item, cat, tok))
            toks = sorted(set(x.group(0) for x in re.finditer(INVOKED, c)))
            if toks:
                invoked.append({"Rule": rid, "Item": item, "Tokens": toks, "Sentence": c})
    a3_mentions = [m_ for m_ in mentions if m_["Token"] == "A-3"]
    if not a3_mentions:
        info.append("ninguna de las cinco reglas menciona A-3")
    return {"Check": "G5 delta frente a V14 y A-2", "Rules": [r for r in RULE_IDS if r in rules],
            "ParagraphSha256": {r: hashlib.sha256(rules[r]["Paragraph"].encode("utf-8")).hexdigest() for r in RULE_IDS if r in rules},
            "A2Pattern": {"Added": a2_add, "Removed": a2_rem, "Changed": [k[1][:80] for k in a2_chg]}, "Application": applied_info,
            "Scope": scope_rows, "InnerQuotes": inner, "ProductExemptions": exempt_used, "CrossScopeMentions": mentions,
            "InvokedReferences": invoked, "Info": info, "Findings": f, "Result": "PASS" if not f else "FAIL"}


# ---------------------------------------------------------------- self-test por mutantes
def mut_text(key, old, new, count=1):
    def fn(inp):
        if old not in (inp[key] or ""):
            raise LookupError("texto de la mutación ausente")
        inp[key] = inp[key].replace(old, new, count)
    return fn


def mut_set(path, value):
    def fn(inp):
        d = inp
        for k in path[:-1]:
            d = d[k]
        d[path[-1]] = value
    return fn


def mut_source(tag, line, old, new):
    def fn(inp):
        ls = inp["Sources"][tag].split("\n")
        if old not in ls[line - 1]:
            raise LookupError("texto de la mutación ausente en la fuente")
        ls[line - 1] = ls[line - 1].replace(old, new, 1)
        inp["Sources"][tag] = "\n".join(ls)
    return fn


def mut_add_path(path, status, text):
    def fn(inp):
        inp["Status"][path] = status
        inp["HeadTexts"][path] = text
    return fn


def mut_append_only_rewrite(inp):
    old = inp["BaseTexts"][EV]
    inp["HeadTexts"][EV] = old[:100] + "X" + old[101:] + inp["HeadTexts"][EV][len(old):]


SYNTH_ID = "zq-host-" + "user-4f1c"  # identidad sintética del host para el self-test (nunca una real; compuesta: este archivo no la contiene)
# muestras sintéticas de privacidad, compuestas en tiempo de ejecución para que este archivo no contenga ninguna coincidencia literal
SYNTH_WIN_PATH = "C:" + "\\" + "Us" + "ers" + "\\" + "someone" + "\\" + "x"
SYNTH_POSIX_PATH = "/c/" + "Us" + "ers" + "/someone/repo"
SYNTH_MAIL = "a.b" + "@" + "example" + ".com"
MUTANTS = [
    # G1
    ("M1-01", "G1", "literal del bloque de §2 cambiado en un carácter (V14 L2439)", mut_text("A4", "> | Controller | verificación | 1 |",
                                                                                          "> | Controller | verificacion | 1 |"), "literal distinto"),
    ("M1-02", "G1", "literal en línea cambiado en un carácter (L2515: +1 → +2)", mut_text("A4", "invocación distinta) | 1 | +1 | 2 |»",
                                                                                         "invocación distinta) | 1 | +2 | 2 |»"), "L01"),
    ("M1-03", "G1", "referencia de línea cambiada (L2515 → L2516)", mut_text("A4", "(sin cambio), L2515:", "(sin cambio), L2516:"), "L01: la referencia"),
    ("M1-04", "G1", "fuente fijada con un carácter distinto (AP L1093)", mut_source("AP", 1093, "elegible no se invoca", "elegible no se invocan"), "L16"),
    ("M1-05", "G1", "cita nueva sin comprobar en §2", mut_text("A4", "Totales (sin cambio): L2523", "Totales (sin cambio, «texto inventado»): L2523"),
     "citas sin comprobar"),
    ("M1-06", "G1", "cabecera: FREEZE_SHA cambiado en un carácter", mut_text("A4", "FREEZE_SHA     = b64a3b64", "FREEZE_SHA     = b64a3b65"), "FREEZE_SHA"),
    ("M1-07", "G1", "cabecera: A-2 sin AGREED", mut_text("A4", "A-2 AGREED (decisiones §57", "A-2 PROPUESTA (decisiones §57"), "A-2 AGREED"),
    ("M1-08", "G1", "cabecera: prefijo del blob de A-3 distinto", mut_text("A4", "blob ea6721f7…). A-4", "blob ea6721f8…). A-4"), "A-3"),
    ("M1-09", "G1", "privacidad: ruta de perfil con nombre de usuario", mut_text("A4", "## 9. Evidencia", "## 9. Evidencia\n\n" + SYNTH_WIN_PATH),
     "ruta de usuario"),
    ("M1-10", "G1", "privacidad: correo", mut_text("PKG", "## 6.", "contacto: " + SYNTH_MAIL + "\n\n## 6."), "ruta de usuario o un correo"),
    ("M1-11", "G1", "privacidad: identidad del host (sintética)", mut_text("A4", "## 9. Evidencia", "## 9. Evidencia\n\nautor " + SYNTH_ID),
     "nombre de usuario o el correo del host"),
    ("M1-12", "G1", "prefijo de blob abreviado de §6 distinto del real", mut_text("A4", "RAE `592dcfd4…`", "RAE `592dcfd5…`"), "592dcfd4"),
    ("M1-13", "G1", "fuente distinta en la base de preparación 524b293e", mut_set(("SourceBlobsAtPrep", PREP_BASE[:8], "V14"), "6" * 40),
     "en 524b293e"),
    ("M1-14", "G1", "literal que difiere de la fuente solo en mayúsculas (AP L1093)", mut_source("AP", 1093, "**Sin celda elegible", "**sin celda elegible"),
     "solo difiere en mayúsculas"),
    ("M1-15", "G1", "cita del texto cambiada sin cambiar la guarda (L2528)", mut_text("A4", "«Ningún tope autoriza gasto ni ejecución ahora.»",
                                                                                    "«Ningún tope autoriza gasto ni ejecución.»"), "L03: la cita no está"),
    # G2
    ("M2-01", "G2", "ruta fuera de alcance añadida", mut_add_path("src/RackCad.Plugin/Fuera.cs", "A", "// x\n"), "rutas no permitidas"),
    ("M2-02", "G2", "evidencia reescrita (no solo añadido)", mut_append_only_rewrite, "no cambia solo por añadido"),
    ("M2-03", "G2", "A-4 modificada (M) en lugar de añadida", mut_set(("Status", A4), "M"), "no aparece como añadido"),
    ("M2-04", "G2", "base distinta del padre", mut_set(("HeadParent",), "0" * 40), "padre de head"),
    ("M2-05", "G2", "A-2 tocada por el commit", mut_add_path(A2, "M", "x"), "rutas no permitidas"),
    ("M2-06", "G2", "blob de A-4 en head distinto", mut_set(("HeadBlobs", A4), "1" * 40), "A4_BLOB"),
    ("M2-07", "G2", "privacidad en un archivo añadido", mut_add_path(REQUEST, "A", "ver " + SYNTH_POSIX_PATH + "\n"), "ruta de usuario"),
    ("M2-08", "G2", "ruta del diff desde 524b293e que no viene del padre ni de la publicación", mut_set(("PrepStatus", "docs/otra/ruta.md"), "A"),
     "no vienen de"),
    # G3
    ("M3-01", "G3", "blob de V14 cambiado en head", mut_set(("Blobs", "Head", V14), "2" * 40), "Head:" + V14),
    ("M3-02", "G3", "blob de A-3 cambiado entre base y head", mut_set(("Blobs", "Head", A3), "3" * 40), "Head:" + A3),
    ("M3-03", "G3", "clause_map.py cambiado en la base", mut_set(("Blobs", "Base", CLAUSE_MAP), "4" * 40), "Base:" + CLAUSE_MAP),
    ("M3-04", "G3", "blob declarado en §6 (AUTOMATION_PLAN) cambiado", mut_set(("Blobs", "Head", AP), "5" * 40), "Head:" + AP),
    # G4
    ("M4-01", "G4", "ruta de una superficie del mapa cambiada", mut_add_path(AP, "M", "x"), "superficies del mapa"),
    ("M4-02", "G4", "clause_map.py check distinto de EQUAL", mut_set(("C20b", "Output"), '{"result": ["MV-6"]}'), "distinto de EQUAL"),
    # G5
    ("M5-01", "G5", "A4-3 sin su cláusula de alcance", mut_text("A4", "> «**Aceptación preautorizada de bindings dentro de la ventana (A-4).** Solo para la "
                                                                      "tarea de FX-02 de la Topología A, en F6, y solo para",
                                                              "> «**Aceptación preautorizada de bindings dentro de la ventana (A-4).** Para"),
     "A4-3: la regla no abre"),
    ("M5-02", "G5", "A4-1 con alcance a la Topología B", mut_text("A4", "de EXECUTION_CONTROLLER de la Topología A", "de EXECUTION_CONTROLLER de la Topología B"),
     "A4-1: la regla no abre"),
    ("M5-03", "G5", "A4-5 sin alcance (sin «Solo para»)", mut_text("A4", "(A-4).** Solo para la tarea de FX-02 de la Topología A, en F6. Tras",
                                                                 "(A-4).** Para la tarea de FX-02 de la Topología A, en F6. Tras"), "A4-5: la regla no abre"),
    ("M5-04", "G5", "nombre de producto en A4-2", mut_text("A4", "a otro adapter cuya celda", "a claude-cli cuya celda"), "nombres de producto"),
    ("M5-05", "G5", "producto citado entre comillas no exento", mut_text("A4", "a otro adapter cuya celda", "a otro adapter («`codex-cli`») cuya celda"),
     "nombres de producto"),
    ("M5-06", "G5", "mención de otro escenario sin negación", mut_text("A4", "todas las filas\n>    de FX-04a, FX-04b, la Topología B, D.5 y D.8.»",
                                                                     "todas las filas\n>    de FX-04a, FX-04b, la Topología B, D.5 y D.8. Esta regla se aplica "
                                                                     "también a FX-03.»"), "ESCENARIO «FX-03»"),
    ("M5-07", "G5", "regla de A-2 alterada", mut_text("A4", "> 6. no amplía permisos", "> 6. la regla de nueva medición de A-2 admite tres sondas. No amplía permisos"),
     "REGLA «A-2»"),
    ("M5-08", "G5", "encabezado introducido en el delta", mut_text("A4", "> 5. mientras dure la sustitución", "> ### D.3 bis\n> 5. mientras dure la sustitución"),
     "aparecen o desaparecen secciones"),
    ("M5-09", "G5", "fila de tabla que reescribe los topes", mut_text("A4", "> 2. P-07 comprueba antes del Q0",
                                                                     "> | **Codex, total** | | **12** | | pool **4** (≤ 2 por fase) | **16** |\n> 2. P-07 comprueba antes del Q0"),
     "filas de tabla"),
    ("M5-10", "G5", "cifra de tope distinta (tope 15 → 16)", mut_text("A4", "≤ 2 por fase, tope 15)", "≤ 2 por fase, tope 16)"), "cifra de tope"),
    ("M5-11", "G5", "ubicación de A4-1 cambiada", mut_text("A4", "que termina en «Ningún otro tope cambia.» (A-2 L234).", "tras D.4."), "A4-1: ubicación"),
    ("M5-12", "G5", "ancla no única en V14 + A-2", mut_source("V14", 2528, "ahora.", "ahora. Ningún otro tope cambia."), "ancla no única"),
    ("M5-13", "G5", "invalidador de A4-1 distinto de A-2", mut_text("A4", "modelo, effort, blobs de catálogo y de routing,\n>    instancia del host)",
                                                                 "modelo, blobs de catálogo y de routing,\n>    instancia del host)"), "invalidadores"),
    ("M5-14", "G5", "A4-1..A4-4 dependen de A4-5", mut_text("A4", "> 6. no amplía permisos", "> 6. con A4-5, no amplía permisos"), "no es separable"),
    ("M5-15", "G5", "cita interna inventada", mut_text("A4", "del sumando «+ 2 sondas» de los totales y de los\n>    bloques",
                                                       "del sumando «+ 3 sondas» de los totales y de los\n>    bloques"), "cita interna"),
    ("M5-16", "G5", "regla A4-5 retirada (cuatro reglas)", lambda inp: inp.__setitem__("A4", re.sub(r"### 3\.6 A4-5.*?(?=### 3\.7)", "", inp["A4"], flags=re.S)),
     "exactamente A4-1..A4-5"),
]


def mut_corr(key, old, new):
    """Mutación del texto corregido (A-4 o paquete) en el escenario C: el texto, el de head y su blob."""
    path = A4 if key == "A4" else PKG

    def fn(inp):
        if old not in (inp[key] or ""):
            raise LookupError("texto de la mutación ausente")
        inp[key] = inp[key].replace(old, new, 1)
        inp["HeadTexts"][path] = inp[key]
        inp["HeadBlobs"][path] = blob_id(inp[key].encode("utf-8"))
    return fn


def mut_selftest(field, value):
    def fn(inp):
        st = json.loads(inp["HeadTexts"][SELFTEST])
        st[field] = value
        inp["HeadTexts"][SELFTEST] = json.dumps(st)
    return fn


def mut_del_status(path):
    def fn(inp):
        inp["Status"].pop(path)
    return fn


def mut_evidence_rewrite(inp):
    inp["Status"][EV] = "M"
    inp["BaseTexts"][EV] = "## 1. registro\nlínea anterior\n"
    inp["HeadTexts"][EV] = "## 1. registro\nlínea reescrita\n## 2. añadido\n"


MUTANTS_C = [  # escenario C: corrección previa a la revisión (copias corregidas sobre la publicación)
    ("MC-01", "G2C", "edición de más en A-4", mut_corr("A4", "## 9. Evidencia", "## 9. Evidencias"), "A-4: el diff frente al blob publicado"),
    ("MC-02", "G2C", "ruta de más en el commit de corrección", mut_add_path("src/RackCad.Plugin/Fuera.cs", "A", "// x\n"), "rutas no permitidas"),
    ("MC-03", "G2C", "edición de más en el paquete", mut_corr("PKG", "## 6. Lo que este paquete no hace", "## 6. Lo que el paquete no hace"),
     "paquete: el diff frente al blob publicado"),
    ("MC-04", "G2C", "falta una de las dos ediciones (L341 sin corregir)", mut_corr("A4", "(«Sin aceptación fingida»)", "(«sin aceptación fingida»)"),
     "A-4: el diff frente al blob publicado"),
    ("MC-05", "G2C", "A-4 añadida (A) en lugar de modificada", mut_set(("Status", A4), "A"), "no aparece como modificado"),
    ("MC-06", "G2C", "a4-guards.py no añadido", mut_del_status(GUARDS), GUARDS + " no aparece como añadido"),
    ("MC-07", "G2C", "la base no tiene la A-4 publicada", mut_set(("BaseBlobs4", A4), "7" * 40), "la base no contiene"),
    ("MC-08", "G2C", "evidencia reescrita (no solo añadido)", mut_evidence_rewrite, "no cambia solo por añadido"),
    ("MC-09", "G2C", "paquete con otro blob de A-4 en L16", mut_corr("PKG", A4_CORR_BLOB, "8" * 40), "paquete: el diff frente al blob publicado"),
    ("MC-10", "G2C", "otra ruta añadida en evidence/I-62-A4", mut_add_path("docs/automation/evidence/I-62-A4/extra.json", "A", "{}\n"), "rutas no permitidas"),
    ("MC-11", "G2C", "decisiones con estado A", mut_set(("Status", DEC), "A"), "con estado A"),
    ("MC-12", "G2D", "edición de más en A-4 (preview)", mut_corr("A4", "## 9. Evidencia", "## 9. Evidencias"), "A-4: el diff frente al blob publicado"),
    ("MC-13", "G2D", "la HEAD ya no tiene la A-4 publicada (preview)", mut_set(("RepoBlobs", A4), A4_CORR_BLOB), "la HEAD del repositorio ya no tiene"),
    ("MC-14", "G5B", "self-test custodiado sin PASS", mut_selftest("Result", "FAIL"), "sin Result PASS"),
    ("MC-15", "G5B", "self-test de otra A-4", mut_selftest("A4TextBlob", "9" * 40), "A4TextBlob distinto"),
    ("MC-16", "G5B", "self-test de otras guardas", mut_selftest("ToolBlob", "a" * 40), "ToolBlob distinto"),
    ("MC-17", "G1", "cabecera A-3 AGREED con otro blob", mut_corr("A4", "blob ea6721f78cb63fbc4f9d99563f3262eb36a32c5d). A-4 no la modifica, no depende",
                                                              "blob ea6721f78cb63fbc4f9d99563f3262eb36a32c5e). A-4 no la modifica, no depende"),
     "blob de A-3 distinto"),
    ("MC-18", "G1", "cabecera A-3 AGREED con §60", mut_corr("A4", "A-3 AGREED (decisiones §61, punto 1;", "A-3 AGREED (decisiones §60, punto 1;"),
     "no casa con decisiones §61"),
    ("MC-19", "G1", "la base no registra A-3 = AGREED en §61", mut_text("DecBase", "**1. A-3 = AGREED**", "**1. A-3 = PENDIENTE**"),
     "no casa con decisiones §61"),
    ("MC-20", "G1", "L341 vuelve a minúscula", mut_corr("A4", "(«Sin aceptación fingida»)", "(«sin aceptación fingida»)"), "solo difiere en mayúsculas"),
    ("MC-21", "G5", "párrafo normativo cambiado frente a 27ffa26b", mut_corr("A4", "Su tope es uno, y P-07", "Su tope es uno y P-07"),
     "cambian frente a 27ffa26b"),
]
MUTANTS_C2 = [  # escenario C2: corrección 2 (copias corregidas sobre 7d863219, en la base que la contiene)
    ("MC2-01", "G2C2", "cambio fuera de las regiones declaradas (§2)", mut_corr("A4", "Topología B, precedente del mismo rol", "Topología B, precedentes del mismo rol"),
     "fuera de las regiones declaradas"),
    ("MC2-02", "G2C2", "párrafo de A4-2 cambiado", mut_corr("A4", "a otro adapter cuya celda esté medida", "a otro adapter, cuya celda esté medida"), "§3.3"),
    ("MC2-03", "G2C2", "A4-4, regla 2, cambiada", mut_corr("A4", "con el conjunto de referencia vacío", "con el conjunto vacío"), "A4-4: ítems no declarados"),
    ("MC2-04", "G2C2", "A4-1, regla 2: la precisión borra texto", mut_corr("A4", "(historia), el cálculo de blobs y hashes, la lectura", "(historia), la lectura"),
     "no es solo inserción"),
    ("MC2-05", "G2C2", "A4-1, regla 5, cambiada", mut_corr("A4", "sin reutilización;\n> 6.", "sin reutilizarla;\n> 6."), "A4-1: ítems no declarados"),
    ("MC2-06", "G2C2", "M-02 vuelve a «no»", mut_corr("A4", "| M-02 | **sí** |", "| M-02 | no |"), "frase exigida ausente"),
    ("MC2-07", "G2C2", "discriminador sin `InstanceId` NOT_STARTED", mut_corr("A4", "tiene `Assurance` NONE e `InstanceId` NOT_STARTED, cuando", "tiene `Assurance` NONE, cuando"),
     "faltan términos"),
    ("MC2-08", "G2C2", "ruta de más", mut_add_path("src/RackCad.Plugin/Fuera.cs", "A", "// x\n"), "rutas no permitidas"),
    ("MC2-09", "G2C2", "paquete sin el blob nuevo de A-4", lambda inp: mut_corr("PKG", "(borrador de esta pasada: " + blob_id(inp["A4"].encode("utf-8")) + ")",
                                                                            "(borrador de esta pasada: " + A4_CORR_BLOB + ")")(inp), "paquete"),
    ("MC2-10", "G2C2", "campo Origin de la cabecera cambiado", mut_corr("A4", "decisiones §60, punto 2 (DEC L1173); F6-OBS-03", "decisiones §60, punto 2 (DEC L1173), F6-OBS-03"),
     "header:Origin"),
    ("MC2-11", "G2C2", "a4-guards.py sin modificar", mut_del_status(GUARDS), GUARDS + " no aparece como modificado"),
    ("MC2-12", "G2C2", "la base no tiene la A-4 corregida 7d863219", mut_set(("BaseBlobs4", A4), A4_BLOB), "la base no contiene"),
    ("MC2-13", "G5", "párrafo de A4-5 cambiado", mut_corr("A4", "se admite **una** corrección", "se admite **una sola** corrección"), "cambian frente a 7d863219"),
    ("MC2-14", "G2D2", "cambio fuera de las regiones (§9, preview)", mut_corr("A4", "## 9. Evidencia", "## 9. Evidencias"), "fuera de las regiones declaradas"),
    ("MC2-15", "G2D2", "la HEAD ya tiene otra A-4 (preview)", mut_set(("RepoBlobs", A4), A4_CORR2_BLOB), "la HEAD del repositorio"),
    ("MC2-16", "G1", "cita nueva sin negrita de la fuente", mut_corr("A4", "B.5 «con **todos** en SATISFIED» (A62-A4-01)", "B.5 «con todos en SATISFIED» (A62-A4-01)"),
     "citas sin comprobar"),
    ("MC2-17", "G5", "nombre de producto en A4-4, regla 5", mut_corr("A4", "validador de producción que materializó F4 no la aplica",
                                                                    "validador de producción que materializó F4 en codex no la aplica"), "nombres de producto"),
    ("MC2-18", "G2C2", "§3.7 recupera la negación sin calificar", mut_corr("A4", "  No autoriza ninguna invocación ni ningún consumo.\n",
                                                                          "  No autoriza ninguna invocación ni ningún consumo; tampoco producción, F4, esquemas.\n"),
     "frase prohibida presente"),
    ("MC2-19", "G5B", "self-test de otra A-4 (corrección 2)", mut_selftest("A4TextBlob", "b" * 40), "A4TextBlob distinto"),
    ("MC2-20", "G1", "prefijo del blob nuevo del kit distinto", mut_corr("A4", "`ceb38284…`", "`ceb38285…`"), "ceb38284"),
]
GUARD_IDS = ("G1", "G2", "G3", "G4", "G5", "G2C", "G2D", "G5B", "G2C2", "G2D2")


def run_guard(gid, inp, cm, ids):
    if gid == "G1":
        return g1_literals(inp, ids)
    if gid == "G2":
        return g2_paths(inp, ids)
    if gid == "G2C":
        return g2_correction(inp, ids)
    if gid == "G2D":
        return g2_diff_only(inp, ids)
    if gid == "G2C2":
        return g2_correction2(inp, ids)
    if gid == "G2D2":
        return g2_diff_only2(inp, ids)
    if gid == "G3":
        return g3_frozen(inp)
    if gid == "G4":
        return g4_c20b(inp, cm)
    if gid == "G5B":
        return g5b_selftest(inp)
    return g5_delta(inp, cm)


def synth_correction(pub, a4_new, pkg_new, dec, tool_text):
    """Escenario C: el commit de corrección simulado sobre la publicación con los textos corregidos (sin escribir nada)."""
    c = copy.deepcopy(pub)
    sel = json.dumps({"Result": "PASS", "ToolBlob": blob_id(tool_text.encode("utf-8")), "A4TextBlob": blob_id(a4_new.encode("utf-8")),
                      "PkgTextBlob": blob_id(pkg_new.encode("utf-8"))})
    c.update({"Mode": "CORRECTION", "Version": "C1", "Base": A4_PUB, "HeadParent": A4_PUB, "Head": "c" * 40, "A4": a4_new, "PKG": pkg_new,
              "A4Prev": pub["A4"], "PkgPrev": pub["PKG"], "DecBase": dec,
              "Status": {A4: "M", PKG: "M", GUARDS: "A", SELFTEST: "A"},
              "HeadTexts": {A4: a4_new, PKG: pkg_new, GUARDS: tool_text, SELFTEST: sel}, "BaseTexts": {A4: pub["A4"], PKG: pub["PKG"]},
              "HeadBlobs": {A4: blob_id(a4_new.encode("utf-8")), PKG: blob_id(pkg_new.encode("utf-8")), GUARDS: blob_id(tool_text.encode("utf-8")),
                            SELFTEST: blob_id(sel.encode("utf-8"))},
              "BaseBlobs4": {A4: A4_BLOB, PKG: PKG_BLOB}, "RepoBlobs": {A4: A4_BLOB, PKG: PKG_BLOB}})
    return c


def synth_correction2(pub, base2, c1_a4, c1_pkg, a4_new, pkg_new, tool_text):
    """Escenario C2: el commit de la corrección 2 simulado sobre la base real que contiene 7d863219 (sin escribir nada)."""
    c = copy.deepcopy(pub)
    c.update(copy.deepcopy(base2["Common"]))
    sel = json.dumps({"Result": "PASS", "ToolBlob": blob_id(tool_text.encode("utf-8")), "A4TextBlob": blob_id(a4_new.encode("utf-8")),
                      "PkgTextBlob": blob_id(pkg_new.encode("utf-8"))})
    c.update({"Mode": "CORRECTION2", "Version": "C2", "Base": base2["Sha"], "HeadParent": base2["Sha"], "Head": "d" * 40, "A4": a4_new, "PKG": pkg_new,
              "A4Prev": c1_a4, "PkgPrev": c1_pkg, "Status": {A4: "M", PKG: "M", GUARDS: "M", SELFTEST: "M"},
              "HeadTexts": {A4: a4_new, PKG: pkg_new, GUARDS: tool_text, SELFTEST: sel},
              "BaseTexts": {A4: c1_a4, PKG: c1_pkg, GUARDS: "# guardas anteriores\n", SELFTEST: "{}\n"},
              "HeadBlobs": {A4: blob_id(a4_new.encode("utf-8")), PKG: blob_id(pkg_new.encode("utf-8")), GUARDS: blob_id(tool_text.encode("utf-8")),
                            SELFTEST: blob_id(sel.encode("utf-8"))},
              "BaseBlobs4": {A4: A4_CORR_BLOB, PKG: PKG_CORR_BLOB}, "RepoBlobs": {A4: A4_CORR_BLOB, PKG: PKG_CORR_BLOB}})
    return c


def kill_rows(inp, mutants, baseline, cm, ids, scenario):
    rows = []
    for mid, gid, desc, fn, expect in mutants:
        m = copy.deepcopy(inp)
        try:
            fn(m)
        except LookupError as e:
            rows.append({"Id": mid, "Scenario": scenario, "Guard": gid, "Mutation": desc, "Result": "FAIL", "Detail": "mutante no aplicable: %s" % e})
            continue
        r = run_guard(gid, m, cm, ids)
        new = [x for x in r["Findings"] if x not in baseline[gid]["Findings"]]
        hit = [x for x in new if expect in x]
        ok = r["Result"] == "FAIL" and bool(hit)
        rows.append({"Id": mid, "Scenario": scenario, "Guard": gid, "Mutation": desc, "Expect": expect, "GuardResult": r["Result"],
                     "NewFindings": [x[:200] for x in new][:3], "Result": "PASS" if ok else "FAIL"})
    return rows


def self_test(pub, corr, dec, cm, tool_text, corr2=None):
    ids = [SYNTH_ID]  # en el self-test, solo la identidad sintética (las reales no entran en los mutantes)
    base_p = {g: run_guard(g, copy.deepcopy(pub), cm, ids) for g in ("G1", "G2", "G3", "G4", "G5")}
    rows = kill_rows(pub, MUTANTS, base_p, cm, ids, "P")
    findings, scen_c, scen_c2 = [], None, None
    if corr2 is None:
        findings.append("sin la corrección 2 (copias --a4/--pkg o A-4 de la corrección 2 en head o en la HEAD) o sin una base con 7d863219: "
                        "el escenario C2 no se comprueba")
    else:
        c2 = synth_correction2(pub, corr2["Base"], corr2["C1A4"], corr2["C1Pkg"], corr2["A4"], corr2["PKG"], tool_text)
        base_c2 = {g: run_guard(g, copy.deepcopy(c2), cm, ids) for g in ("G1", "G2C2", "G2D2", "G5", "G5B")}
        rows += kill_rows(c2, MUTANTS_C2, base_c2, cm, ids, "C2")
        ok2 = all(v["Result"] == "PASS" for v in base_c2.values())
        if not ok2:
            findings.append("control C2: la corrección 2 sin mutar no pasa %s" % [g for g, v in base_c2.items() if v["Result"] != "PASS"])
        scen_c2 = {"TextSource": corr2["Source"], "Base": corr2["Base"]["Sha"], "A4TextBlob": blob_id(corr2["A4"].encode("utf-8")),
                   "PkgTextBlob": blob_id(corr2["PKG"].encode("utf-8")), "ControlResults": {g: v["Result"] for g, v in base_c2.items()},
                   "ControlFindings": {g: v["Findings"] for g, v in base_c2.items() if v["Findings"]}, "Control": "PASS" if ok2 else "FAIL"}
    if corr is None:
        findings.append("sin textos corregidos (--a4/--pkg, o A-4 corregida en head o en la HEAD): el escenario C no se comprueba")
    else:
        a4_new, pkg_new, source = corr
        c = synth_correction(pub, a4_new, pkg_new, dec, tool_text)
        base_c = {g: run_guard(g, copy.deepcopy(c), cm, ids) for g in ("G1", "G2C", "G2D", "G5", "G5B")}
        rows += kill_rows(c, MUTANTS_C, base_c, cm, ids, "C")
        control_ok = all(v["Result"] == "PASS" for v in base_c.values())
        if not control_ok:
            findings.append("control C: la corrección sin mutar no pasa %s" % [g for g, v in base_c.items() if v["Result"] != "PASS"])
        scen_c = {"TextSource": source, "A4TextBlob": blob_id(a4_new.encode("utf-8")), "PkgTextBlob": blob_id(pkg_new.encode("utf-8")),
                  "ControlResults": {g: v["Result"] for g, v in base_c.items()},
                  "ControlFindings": {g: v["Findings"] for g, v in base_c.items() if v["Findings"]}, "Control": "PASS" if control_ok else "FAIL"}
    per = {}
    for g in GUARD_IDS:
        rs = [x for x in rows if x["Guard"] == g]
        per[g] = {"Mutants": len(rs), "Killed": sum(1 for x in rs if x["Result"] == "PASS")}
    ok = not findings and all(x["Result"] == "PASS" for x in rows) and all(v["Mutants"] > 0 for v in per.values())
    last = scen_c2 or scen_c  # el self-test custodiado se liga a la última corrección comprobada (G5b)
    return {"Tool": "a4-guards.py self-test",
            "ScenarioP": {"Base": pub["Base"], "Head": pub["Head"], "BaselineResults": {g: v["Result"] for g, v in base_p.items()},
                          "BaselineFindings": {g: v["Findings"] for g, v in base_p.items() if v["Findings"]}},
            "ScenarioC": scen_c, "ScenarioC2": scen_c2, "A4TextBlob": last["A4TextBlob"] if last else None,
            "PkgTextBlob": last["PkgTextBlob"] if last else None,
            "PerGuard": per, "Total": {"Mutants": len(rows), "Killed": sum(1 for x in rows if x["Result"] == "PASS")}, "Mutants": rows,
            "Findings": findings, "Result": "PASS" if ok else "FAIL"}


def summary(st):
    return {"Result": st["Result"], "Total": st["Total"], "PerGuard": st["PerGuard"], "BaselineP": st["ScenarioP"]["BaselineResults"],
            "ControlC": (st["ScenarioC"] or {}).get("ControlResults"), "ControlC2": (st.get("ScenarioC2") or {}).get("ControlResults"),
            "A4TextBlob": st["A4TextBlob"], "PkgTextBlob": st["PkgTextBlob"],
            "Findings": st["Findings"]}


# ---------------------------------------------------------------- principal
def write_json(path, res, ids):
    text = json.dumps(res, ensure_ascii=False, indent=1) + "\n"
    leak = privacy_findings("resultado", text, ids)
    if leak:  # nunca se escribe un resultado con una ruta de perfil ni una identidad del host
        res = {"Tool": res.get("Tool"), "Result": "FAIL", "Findings": leak}
        text = json.dumps(res, ensure_ascii=False, indent=1) + "\n"
    with open(path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(text)
    return res


def read_text(path):
    return open(path, "rb").read().decode("utf-8").replace("\r\n", "\n")


def corrected_texts(git, a, head=None):
    """Corrección 1 (escenario C): siempre los blobs fijados 7d863219 / 085f30f6 del repositorio."""
    return git.cat(A4_CORR_BLOB), git.cat(PKG_CORR_BLOB), "blobs 7d863219/085f30f6"


def correction2_context(git, a, head=None):
    """Corrección 2 (escenario C2): textos de --a4/--pkg o de head / HEAD cuando su A-4 no es 27ffa26b ni 7d863219; base = el primer commit
    (padre de head, HEAD o sus antecesores de primer padre) cuya A-4 es 7d863219."""
    texts = None
    if getattr(a, "a4", None) and getattr(a, "pkg", None):
        t4, tp = read_text(a.a4), read_text(a.pkg)
        if blob_id(t4.encode("utf-8")) not in (A4_BLOB, A4_CORR_BLOB):
            texts = (t4, tp, "args")
    for rev in [x for x in (head, "HEAD") if x and texts is None]:
        t = git.show(rev, A4)
        if t is not None and blob_id(t.encode("utf-8")) not in (A4_BLOB, A4_CORR_BLOB):
            texts = (t, git.show(rev, PKG), "head" if rev == head else "HEAD")
    if texts is None:
        return None
    cands = ([head + "^1"] if head else []) + git("rev-list", "--first-parent", "-n", "400", "HEAD").split()
    base = next((c for c in cands if git.blob(c, A4) == A4_CORR_BLOB), None)
    if base is None:
        return None
    sha = git("rev-parse", base).strip()
    return {"A4": texts[0], "PKG": texts[1], "Source": texts[2], "C1A4": git.cat(A4_CORR_BLOB), "C1Pkg": git.cat(PKG_CORR_BLOB),
            "Base": {"Sha": sha, "Common": collect_common(git, sha)}}


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("self-test")
    s.add_argument("--repo", required=True); s.add_argument("--out", required=True); s.add_argument("--a4"); s.add_argument("--pkg")
    s.add_argument("--tmp")
    p = sub.add_parser("preview")
    p.add_argument("--repo", required=True); p.add_argument("--a4", required=True); p.add_argument("--pkg", required=True)
    p.add_argument("--out", required=True); p.add_argument("--tmp")
    r = sub.add_parser("run")
    r.add_argument("--repo", required=True); r.add_argument("--base", required=True); r.add_argument("--head", required=True)
    r.add_argument("--out", required=True); r.add_argument("--also-at"); r.add_argument("--a4"); r.add_argument("--pkg"); r.add_argument("--tmp")
    a = ap.parse_args()
    tmp_root = a.tmp or os.path.join(os.path.dirname(os.path.abspath(a.out)), "tmp")
    os.makedirs(tmp_root, exist_ok=True)
    git = Git(a.repo)
    ids = host_identities(a.repo)
    raw = open(os.path.abspath(__file__), "rb").read().replace(b"\r\n", b"\n")
    tool_text = raw.decode("utf-8")
    with tempfile.TemporaryDirectory(dir=tmp_root) as td:
        cm, tool = load_clause_map(git, td)
        pub = collect(git, A4_BASE, A4_PUB, None, cm, tool, td)
        if a.cmd == "self-test":
            res = self_test(pub, corrected_texts(git, a), git.show("HEAD", DEC), cm, tool_text, correction2_context(git, a))
        elif a.cmd == "preview":
            a4_text, pkg_text = read_text(a.a4), read_text(a.pkg)
            inp = collect_preview(git, a4_text, pkg_text)
            g2d = g2_diff_only2(inp, ids) if inp["Version"] == "C2" else g2_diff_only(inp, ids)
            checks = [g1_literals(inp, ids), g2d, g3_frozen(inp, ("Head",)), g5_delta(inp, cm)]
            if inp["Version"] == "C2":
                st = self_test(pub, corrected_texts(git, a), inp["DecBase"], cm, tool_text, correction2_context(git, a))
            else:
                st = self_test(pub, (a4_text, pkg_text, "args"), inp["DecBase"], cm, tool_text, correction2_context(git, a))
            ok = all(c["Result"] == "PASS" for c in checks) and st["Result"] == "PASS"
            res = {"Tool": "a4-guards.py preview", "Version": inp["Version"], "RepoHead": inp["Head"], "A4Blob": blob_id(a4_text.encode("utf-8")),
                   "PkgBlob": blob_id(pkg_text.encode("utf-8")), "Checks": checks, "SelfTest": summary(st), "Result": "PASS" if ok else "FAIL"}
        else:
            head = git("rev-parse", a.head).strip()
            inp = pub if (git("rev-parse", a.base).strip(), head) == (A4_BASE, A4_PUB) and not a.also_at else \
                collect(git, a.base, a.head, a.also_at, cm, tool, td)
            if inp["Mode"] == "PUBLICATION":
                checks = [g1_literals(inp, ids), g2_paths(inp, ids), g3_frozen(inp), g4_c20b(inp, cm), g5_delta(inp, cm)]
            elif inp["Mode"] == "CORRECTION2":
                checks = [g1_literals(inp, ids), g2_correction2(inp, ids), g3_frozen(inp), g4_c20b(inp, cm), g5_delta(inp, cm), g5b_selftest(inp)]
            else:
                checks = [g1_literals(inp, ids), g2_correction(inp, ids), g3_frozen(inp), g4_c20b(inp, cm), g5_delta(inp, cm), g5b_selftest(inp)]
            also = None
            if a.also_at:
                g5x = g5_delta(inp, cm, inp["A4AlsoAt"])
                also = {"Rev": inp["AlsoAt"], "A4BlobAtRev": inp["Blobs"]["AlsoAt"].get(A4),
                        "Checks": [g3_frozen(inp, ("AlsoAt",)), {k: g5x[k] for k in ("Check", "Rules", "ParagraphSha256", "Application", "Findings", "Result")}]}
                also["SameAsHead"] = g5x["ParagraphSha256"] == checks[4]["ParagraphSha256"]
            dec_c = git.show(head if inp["Mode"] != "PUBLICATION" else "HEAD", DEC)  # el escenario C necesita decisiones con §61 (A-3 AGREED)
            st = self_test(pub, corrected_texts(git, a, head), dec_c, cm, tool_text, correction2_context(git, a, head))
            ok = all(c["Result"] == "PASS" for c in checks) and (also is None or all(c["Result"] == "PASS" for c in also["Checks"]))
            res = {"Tool": "a4-guards.py run", "Mode": inp["Mode"], "A4Base": A4_BASE, "Base": inp["Base"], "Head": inp["Head"],
                   "A4Blob": inp["HeadBlobs"].get(A4), "Checks": checks, "AlsoAt": also, "SelfTest": summary(st),
                   "Result": "PASS" if ok and st["Result"] == "PASS" else "FAIL"}
    tool_priv = privacy_findings("a4-guards.py", tool_text, ids)  # el propio archivo se publicará: sin rutas, correos ni identidades
    res["ToolBlob"] = blob_id(raw)
    res["ToolPrivacy"] = {"Findings": tool_priv, "Result": "PASS" if not tool_priv else "FAIL"}
    if tool_priv:
        res["Result"] = "FAIL"
    res = write_json(a.out, res, ids)
    print(res["Result"])
    sys.exit(0 if res["Result"] == "PASS" else 1)


if __name__ == "__main__":
    main()
