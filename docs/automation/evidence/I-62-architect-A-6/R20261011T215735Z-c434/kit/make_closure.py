"""I-62, decisiones §77, punto 4: EffectiveInputClosure de la PRIMERA REVISIÓN FORMAL de la enmienda A-6 (docs/initiatives/I-62-A-6.md, blob
eac473c0, PROPUESTA) por un ARCHITECT independiente, en una celda `claude-cli` medida y elegible (autorización del Owner CLAUDE-CLI-I62 = A:
revisiones de ARCHITECT de I-62, solo lectura). La revisión solo se lanza con la compuerta del Owner de launch.py (11-OwnerGate) en Ok.

Kit adaptado del kit del dictamen de materialidad de B-min (R20261011T162328Z-5c5e, auditor v5.1-bmin, ACCREDITED; la estructura de cierre,
rangos, run y transporte) y del de la primera revisión formal de A-5 (R20261011T065226Z-dbb0; la forma de la revisión de una enmienda). El objeto
es UN archivo (A-6) en el commit del clon (CloneCommit de kit-params.json; por defecto, la HEAD de la rama al preparar: 7b5ffac9, decisiones §77
registradas). El run tiene DOS archivos (order.txt = cuerpo §77 y prompt.md, renderizado desde prompt.template.md).

Cierre canónico, en el commit del clon, cada archivo fijado por su blob (PINNED, medidos en 7b5ffac9; los directorios por su árbol): A-6 entera
(objeto); docs/automation/evidence/I-62-A6/ entero (guardas, autoprueba, resultados y preflight); el paquete B-min entero (para Q-A6-12: el
revisor compara tool/bmin_facts.py y accept/bmin_accept.py); el dictamen de materialidad (output.json y README.md de
R20261011T162328Z-5c5e); V14 (§18 como orientación de la matriz OV y las OD, §20.2-§20.3.1, Anexo C con C-24 y L2333-L2334, D.1, D.3, D.4, D.5 y
D.6); AUTOMATION_PLAN 16.4, 16.9, 16.10, 16.11, 16.13-16.14 y 16.18-16.24; ADR-0046 y ADR-0048 enteros; LIFECYCLE §3, §5 (definición de
REQUIRED/OPTIONAL y de los veredictos, orientación) y §6; A-1..A-5 (A-4 y A-5 con sus secciones; A-1..A-3, la cabecera como orientación);
agent-execution README §5, §8, §10, §14 y §15; routing.md; adapters/codex-cli.md; los esquemas input-closure.v1, role-invocation.v1,
controller-verification.v2, delegation.v2 y worker-handoff; kits/FX-02/controller-contracts.md; decisiones §73-§77 enteras. EXCLUIDA la
evidencia (docs/automation/evidence/I-62-evidence.md): contiene los resúmenes y valoraciones de la sesión que preparó A-6 y este kit; el revisor
juzga sin ese anclaje. Excluidos también transcripciones, memoria, probe_tools.py y las referencias de las sondas.
Los rangos se calculan aquí, mecánicamente, desde los encabezados, las filas y las líneas citadas (con su primera línea comprobada); el script
falla si algo no coincide.

Uso:
  python -B make_closure.py --preview          → solo lectura de git en el worktree de I-62, en el commit de preparación: archivos y rangos
  python -B make_closure.py --render-prompt    → renderiza kit/prompt.md desde kit/prompt.template.md (clon de kit-params.json)
  python -B make_closure.py                    → escribe kit/closure.json (exige que kit/prompt.md sea el renderizado y que el run tenga
                                                  exactamente order.txt y prompt.md, iguales a las copias del kit)
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

CLONE_FIXED, RUN_FIXED = r"D:\r62-arch-a6", r"D:\r62-arch-a6-run"
WORKTREE = os.path.join(os.environ.get("USERPROFILE", ""), ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
PREP_COMMIT = "7b5ffac9d9c04f90b9708a867089d32e885dc815"          # HEAD de la rama al preparar: decisiones §77 registradas (evidencia §150)
PUBLICATION, PUB_PARENT = "7b7aba4645ff2430d583c641d817630282a56fdf", "108d4ca1a395c64c43cfae4f17f5854fe2d01536"
GUARDS_RUN_COMMIT = "0c817e1d37409ae6dfb1246c20e08c4509ad86d6"   # custodia del resultado `run` de las guardas de A-6 (PASS)
IDS = {"RunId": "R20261011T215735Z-c434", "InvocationId": "I20261011T215735Z-c434", "LogicalReviewRequestId": "L20261011T215735Z-c434",
       "AttemptSeq": 1}
SESSION_ID = "35cd6b11-5a2b-415a-8e87-a3b68fb0817d"          # --session-id fijado para el único lanzamiento (uuid4)
OBJECT, OBJECT_BLOB = "docs/initiatives/I-62-A-6.md", "eac473c0d2e58cf1c4594f48a8816b3833af6509"
EVID_DIR = "docs/automation/evidence/I-62-A6"
GUARDS, SELFTEST, GRR = EVID_DIR + "/a6-guards.py", EVID_DIR + "/a6-selftest.json", EVID_DIR + "/a6-guards-result-run.json"
PKG_DIR = "docs/automation/evidence/I-62-F6/FX-02/bmin"
RULING_DIR = "docs/automation/evidence/I-62-architect-bmin/R20261011T162328Z-5c5e"
RULING_OUT, RULING_README = RULING_DIR + "/output.json", RULING_DIR + "/README.md"
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
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"          # EXCLUIDA del cierre (anclaje)
CHAR_RECORD = "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json"   # lo fija la compuerta; fuera del cierre
PINNED = {   # blob en el commit de preparación (7b5ffac9); el commit del clon debe conservarlo
    OBJECT: OBJECT_BLOB,
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534",
    ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e", LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", A1: "c01899a72b940503bb85a0fab42bc085c603fd0f", A2: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    A3: "ea6721f78cb63fbc4f9d99563f3262eb36a32c5d", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f", A5: "ad94ceedacbf3dbf70f0217a583978d0e32592fc",
    RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055", ROUTING: "bba08fc4686d7192deeb559752636a467103b4b1",
    CODEX_CLI: "155f3469e345e2397fa26347a9cb52d6fdb91122", S_RI: "d54a7ae783ec83d055babdae090899908b616082",
    S_IC: "6bdf71843e2c70362a5e2d0ea19c8891e22bcff6", S_CV2: "e1faa9eca5c50b2b3a9f91575575614366962720",
    S_DEL2: "dfdbe461270e8fd92200c36bab78b9fdc3385014", S_WH: "cc3c9cf76b2247f3312e99bd447554326cc3700a",
    CC: "90929ad1c49c028f41f58f654f8713bd0b35f27b", DECISIONS: "04c415ee7897d30dc7b8b9f82b2824f2ac54ff0d",
    RULING_OUT: "96c3efbc81e9c6814b232774fd8ac7e900deda7d", RULING_README: "8ce105e0c2c6efd18eb720799492fdca0b4f7315",
    GUARDS: "c5cc6168d4a29f94623a8fab90a2d4f5f8df9ffa", SELFTEST: "8a61a8c48aeb007d05772cab53679b8f58a97fe4",
    GRR: "a8c9ba8bba3047849cfb8bf583dd9d707ffea610",
    CHAR_RECORD: "541956eb8f5d154414b91338c655919ae0ade697",
}
PINNED_TREES = {PKG_DIR: "3caa0dfa73d372af9251cf5c5c194261de8c7685", EVID_DIR: "7f504a1710c593f896390474562574623552813c",
                "docs/automation/agent-execution": "809fd58f450d3379a4fcc958cf8907fcdc5acff3"}
DECISIONS_AT_PUB_PARENT = "228475b01def62985785042a86bf9d6bc68f9b07"   # decisiones en 108d4ca1 (A-6 preparada sobre §76): prefijo exacto
RUN_FILES = ["order.txt", "prompt.md"]
PREMISE_RUN_FILES = ["order.txt"]                            # prompt.md nunca
ORDER_TEXT = ("77", "e23faecbacc7ae33976f211ee381020b88dd08f38067eaa938c5b54ec36c4537", 2918)
TOPICS = ["Q-A6-01", "Q-A6-02", "Q-A6-06/07", "Q-A6-12"]
REQUIRED_TOPICS = ["Q-A6-02", "Q-A6-06/07", "Q-A6-12"]      # «REQUIRED REVIEW TOPIC» en decisiones §77, punto 2
DELTAS = ["A6-%d" % i for i in range(1, 10)]
MATERIALITY_IDS = ["M-0%d" % i for i in range(1, 9)]
MATERIALITY_A6 = {"M-01": "no", "M-02": "yes", "M-03": "yes", "M-04": "yes", "M-05": "yes", "M-06": "no", "M-07": "no", "M-08": "yes"}
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
TIMEOUT_S = 3600               # igual que en los kits de A-4, A-5 y B-min
AUTH_SETTLE_S = 120            # mitigación del intento 1 de A-3: espera entre `auth status` (preflight 5) y el lanzamiento
LARGE = 60000
CLONE, RUN, REPO, REV = CLONE_FIXED, RUN_FIXED, CLONE_FIXED, None


def git(*a):
    return subprocess.check_output(["git", "-C", REPO] + list(a))


def git_ok(*a):
    return subprocess.run(["git", "-C", REPO] + list(a), capture_output=True).returncode == 0


# ------------------------------------------------------------------ rangos mecánicos (los del kit de B-min)
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
        table_at(L, s24, lambda x: x.startswith("| P-22 |"), "16.24, P-22..P-25")]


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
            annex_c, cited(L, 2333, 2334, "Esperado independiente", "Anexo C, esperado independiente (L2333-L2334)"),
            cited(L, 2363, 2363, "| C-24 |", "Anexo C, C-24 (L2363)")] + [d[k] for k in d] + [
            cited(L, 2433, 2447, "**Topología A**", "D.3, Topología A: tabla de roles, adapters y topes (L2433-L2447)"),
            cited(L, 2523, 2523, "| A | ≤ 15 + 2 sondas |", "D.3, totales de la ronda A (L2523)"),
            cited(L, 2534, 2537, "| **PASS** |", "D.4, resultados PASS/FAIL/UNVERIFIED/UNSUPPORTED (L2534-L2537)"),
            cited(L, 2544, 2544, "| FX-02 PASS |", "D.4, matriz de cierre: FX-02 PASS (L2544)"),
            cited(L, 2558, 2560, "Se resiembra el fixture", "D.5, resiembra y FX-02 (L2558-L2560)"),
            cited(L, 2572, 2573, "| **Dependencias técnicas permitidas** |", "D.6, dependencias técnicas permitidas y privadas prohibidas (L2572-L2573)"),
            cited(L, 2586, 2587, "El oráculo, fuera del host", "D.6, último párrafo, ancla de A6-3 (L2586-L2587)")]


def ranges_lifecycle(L):
    s3 = section(L, "3. ")
    return [s3] + [row(L, s3, (lambda m: lambda x: x.startswith("| **%s** |" % m))(m), "§3, %s" % m) for m in MATERIALITY_IDS] + [
        dict(row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "§3, UNKNOWN cuenta como activado"),
             End=row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "")["End"] + 1),
        row(L, s3, lambda x: x.startswith("Un cambio de alcance, no-objetivos, decision Owner"), "§3, OWNER-RESERVED"),
        section(L, "5. "), row(L, section(L, "5. "), lambda x: x.startswith("Un hallazgo es `REQUIRED`"), "§5, REQUIRED y OPTIONAL"),
        row(L, section(L, "5. "), lambda x: x.startswith("Solo quien emitio un REQUIRED"), "§5, veredictos"),
        section(L, "6. ")]


def ranges_adr(L):
    dec = section(L, "Decisión")
    return whole(L) + [header_block(L)] + all_sections(L) + numbered_items(L, dec, "Decisión")


def ranges_rae(L):
    keep = ("5. ", "8. ", "10. ", "14. ", "14.", "15. ", "15.")
    return whole(L) + [section(L, h[2]) for h in headings(L) if h[1] in (2, 3) and h[2].startswith(keep)] + [
        cited(L, 184, 184, "(INVALID_OUTPUT)", "§8, salida inválida (INVALID_OUTPUT) (L184)"),
        cited(L, 561, 561, "| I2 |", "§15.2, regla I2 (L561)"), cited(L, 565, 568, "| I6 |", "§15.2, regla I6 y auditoría de lecturas (L565-L568)")]


def ranges_a4(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 169, 169, "- **Hueco.** D.3 supone que la receta basta", "§2, hueco (L169)"),
        cited(L, 192, 192, "> 1. la invocación del rol puede declarar", "§3.2, A4-1, regla 1 (L192)"),
        cited(L, 203, 207, "regla de nueva medición de A-2. Su tope es uno", "§3.2, A4-1, reglas 3-4 (L203-L207)"),
        cited(L, 393, 393, "| M-03 | **sí** |", "§5, M-03 (L393)")]


def ranges_a5(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 175, 199, "> «**Construcción del fixture conforme al mapa de cláusulas (A-5).**", "§3.2, párrafo A5-1 (L175-L199)"),
        cited(L, 216, 216, "(**11** base, pool **4**", "§3.3, A5-2, regla 4 (L216)"),
        cited(L, 221, 221, "extiende a esta fila.»", "§3.3, fin de A5-2 y de D.3 con A-1..A-5 (L221)")]


def ranges_header_only(L):
    return whole(L) + [header_block(L)]


def ranges_a6(L):
    s = {h[2].split(" ")[0]: section(L, h[2]) for h in headings(L)[1:] if h[1] <= 3}
    s5, s7, s8 = s["5."], s["7."], s["8."]
    return whole(L) + [header_block(L)] + all_sections(L) + [
        table_at(L, s["3.2"], lambda x: x.startswith("| Punto | Alcance mínimo del dictamen |"), "§3.2, tabla de los ocho puntos"),
        table_at(L, s["3.3"], lambda x: x.startswith("| Pieza | Ruta de procedencia |"), "§3.3, tabla de identidad por blob"),
        table_at(L, s5, lambda x: x.startswith("| M | Activado | Dictamen | Motivo |"), "§5, tabla M-01..M-08")] + [
        row(L, s5, (lambda m: lambda x: x.startswith("| %s |" % m))(m), "§5, %s" % m) for m in MATERIALITY_IDS] + [
        cited(L, OG.A6_LINE_NO, OG.A6_LINE_NO, "A6-ADR0046-9-FX02 = A (", "§7, línea del Owner (a) A6-ADR0046-9-FX02 (L%d)" % OG.A6_LINE_NO),
        cited(L, OG.A6_LINE_NO + 1, OG.A6_LINE_NO + 1, "A6-CONTROLLER-MEDICION-CONSUMO = A (", "§7, línea del Owner (b) (L%d)" % (OG.A6_LINE_NO + 1)),
        row(L, s7, lambda x: x.startswith("- **(a) Determinación sobre ADR-0046 #9"), "§7, consecuencia (a)"),
        row(L, s7, lambda x: x.startswith("- **(b) Autorización de consumo"), "§7, consecuencia (b)")] + [
        row(L, s8, (lambda q: lambda x: x.startswith("| %s |" % q))(q), "§8, %s" % q) for q in ("Q-A6-01", "Q-A6-02", "Q-A6-06", "Q-A6-07", "Q-A6-12")]


def ranges_grr(L):
    return whole(L) + [cited(L, 2, 4, '"Mode": "RUN"', "Mode, Base y Head (L2-L4)"),
                       row(L, {"Start": 1, "End": len(L)}, lambda x: x == ' "Result": "PASS"', "Result de conjunto")]


def ranges_selftest(L):
    a = row(L, {"Start": 1, "End": len(L)}, lambda x: x == ' "Summary": {', "")["Start"]
    b = row(L, {"Start": 1, "End": len(L)}, lambda x: x.startswith(' "A6TextBlob": '), "")["Start"]
    return whole(L) + [{"Section": "Summary, Result, Mode, Repo, ToolBlob y A6TextBlob (L%d-L%d)" % (a, b), "Start": a, "End": b}]


def ranges_decisions(L):
    s = {n: section(L, "%d. " % n) for n in (73, 74, 75, 76, 77)}
    s77 = s[77]
    rows77 = [row(L, s77, (lambda k: lambda x: x.startswith("| **%d. " % k))(k), "§77, punto %d" % k) for k in range(1, 9)]
    src = row(L, s77, lambda x: x.startswith("cuerpo pegado, extraído de la transcripción de la sesión, suma "), "§77, bytes y SHA-256 del cuerpo pegado")
    return [span(s[73], s[77], "§73-§77")] + [s[n] for n in (73, 74, 75, 76, 77)] + [src] + rows77


def ranges_doc(L):
    return whole(L) + [header_block(L)] + all_sections(L)


def ranges_whole(L):
    return whole(L)


# (ruta, por qué, alcance, función de rangos, etiqueta corta para el prompt)
canonical = [
    (V14, "Proposal V14 congelada: §18 (matriz OV y OD, orientación), §20.2 (AUT-I1), §20.3 y §20.3.1 (cierre efectivo de insumos), Anexo C "
          "(L2333-L2334; C-24, L2363), D.1, D.3 (tabla de la Topología A y totales), D.4 (resultados y matriz de cierre), D.5 y D.6", "SECTIONS",
     ranges_v14, "§18, §20.2, §20.3, §20.3.1, Anexo C, D.1, D.3, D.4, D.5 y D.6"),
    (AP, "AUTOMATION_PLAN: 16.4 (receta), 16.9 (comprobaciones y modos de fallo; INVALID_OUTPUT L653), 16.10, 16.11 (recuperación), 16.13-16.14 "
         "(compatibilidad, Validate(M)) y 16.18-16.24 (adapters, binding, independencia, aceptación, invocación de rol, cierre de insumos y fidelidad)",
     "SECTIONS", ranges_ap, "16.4, 16.9, 16.10, 16.11, 16.13-16.14 y 16.18-16.24"),
    (ADR46, "ADR-0046 aceptado (protocolo de ejecución delegada; #6 verificación fail-closed; #9 nivel A)", "ENTIRE", ranges_adr, "entero"),
    (ADR48, "ADR-0048 (roles, binding por capacidad, independencia por dimensiones; conserva #9, Level A)", "ENTIRE", ranges_adr, "entero"),
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
    (CC, "contratos de las invocaciones del Controller de FX-02 (codex-cli, solo lectura); sin valores esperados", "ENTIRE", ranges_doc, "entero"),
    (DECISIONS, "decisiones §73-§77 (§77: esta revisión, punto 4; temas del punto 2)", "SECTIONS", ranges_decisions, "§73-§77"),
]


def lines_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    if lines and lines[-1] == "":
        lines = lines[:-1]
    return lines


def describe(b, path):
    try:
        t = b.decode("utf-8")
    except UnicodeDecodeError:
        raise SystemExit("%s no es texto UTF-8: el cierre por líneas no lo admite (STOP: decidir antes de lanzar)" % path)
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


def identity_checks():
    for c in (REV, PREP_COMMIT, PUBLICATION, PUB_PARENT, GUARDS_RUN_COMMIT):
        assert git_ok("cat-file", "-e", c + "^{commit}"), "commit ausente en el clon: %s" % c
    assert git("rev-parse", PUBLICATION + "^").decode().strip() == PUB_PARENT, "el padre de la publicación no es 108d4ca1"
    assert not git_ok("cat-file", "-e", PUB_PARENT + ":" + OBJECT), "A-6 ya existía en el padre de la publicación"
    for c in (PUBLICATION, GUARDS_RUN_COMMIT, REV):
        assert git("rev-parse", c + ":" + OBJECT).decode().strip() == OBJECT_BLOB, "blob de A-6 distinto en %s" % c
    for a, b in ((PUBLICATION, GUARDS_RUN_COMMIT), (GUARDS_RUN_COMMIT, PREP_COMMIT), (PREP_COMMIT, REV)):
        assert git_ok("merge-base", "--is-ancestor", a, b), "%s no es antecesor de %s" % (a, b)
    pub_files = dir_files(PUBLICATION, EVID_DIR)
    now_files = dir_files(REV, EVID_DIR)
    unchanged = all(git("rev-parse", PUBLICATION + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip() for p in pub_files)
    assert unchanged and set(now_files) - set(pub_files) == {GRR}, "I-62-A6/ cambió después de la publicación además de añadir el resultado `run`"
    assert git("rev-parse", PUBLICATION + ":" + PKG_DIR).decode().strip() == PINNED_TREES[PKG_DIR], "árbol del paquete B-min distinto en la publicación"
    for d, t in PINNED_TREES.items():
        assert git("rev-parse", REV + ":" + d).decode().strip() == t, "árbol %s distinto del fijado" % d
    old = git("cat-file", "-p", DECISIONS_AT_PUB_PARENT)
    assert git("show", REV + ":" + DECISIONS).startswith(old), "decisiones no crece solo por añadido desde 108d4ca1"
    a6 = git("show", REV + ":" + OBJECT).decode("utf-8").split("\n")
    card = git("show", REV + ":" + OG.CARD_PATH).decode("utf-8").split("\n")
    pin_a6 = OG.line_sha(a6[OG.A6_LINE_NO - 1]) == OG.A6_LINE_SHA
    pin_card = OG.line_sha(card[OG.CARD_LINE_NO - 1]) == OG.A6_LINE_SHA
    assert pin_a6 and pin_card, "la línea fijada del Owner no es la de A-6 L679 y de la tarjeta L22"
    dec = git("show", REV + ":" + DECISIONS).decode("utf-8").split("\n")
    assert sum(1 for l in dec if OG.line_sha(l) == OG.CLAUDE_LINE_SHA) == 1, "decisiones sin la línea CLAUDE-CLI-I62 = A fijada"
    assert not any(OG.line_sha(l) == OG.A6_LINE_SHA for l in dec), "decisiones del clon ya contiene la línea del Owner de A-6 (inesperado)"
    changed = [x for x in git("diff", "--name-status", PUBLICATION, REV).decode("utf-8").split("\n") if x]
    return {"CloneCommit": REV, "PreparationCommit": PREP_COMMIT, "Publication": PUBLICATION, "PublicationParent": PUB_PARENT,
            "GuardsRunCommit": GUARDS_RUN_COMMIT, "ObjectAbsentAtPublicationParent": True, "ObjectBlobAtPublicationGuardsRunAndClone": OBJECT_BLOB,
            "EvidenceDirUnchangedSincePublicationExceptRunResult": True, "DecisionsAppendOnlySincePublicationParent": True,
            "PinnedTrees": PINNED_TREES, "OwnerLinePinnedToA6L679AndCardL22": True, "ClaudeCliLinePinnedOnce": True,
            "OwnerLineAbsentInCloneDecisions": True, "ChangedSincePublication": changed}


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
    rs = [g for g in r["LineRanges"] if g["Section"] not in ("entero",) and not g.get("Span")]
    head = "%d. `%s` (%s bytes, %s líneas; blob `%s`)" % (k, r["Path"], fmt_n(r["Bytes"]), fmt_n(r["Lines"]), r["Blob"][:8])
    body = r["PromptLabel"]
    if rs:
        body += ": " + "; ".join("%s %s" % (short_label(r["Path"], g["Section"]), range_text(g)) for g in rs)
    return head + ", " + body + "."


def group_items(rev):
    ev = [(n, "evidencia de A-6 (guardas, autoprueba, resultados y preflight): %s" % n[len(EVID_DIR) + 1:], "ENTIRE",
           ranges_grr if n == GRR else (ranges_selftest if n == SELFTEST else (ranges_doc if n.endswith(".md") else ranges_whole)),
           "evidencia de A-6, entera") for n in dir_files(rev, EVID_DIR)]
    pk = [(n, "paquete B-min, %s" % n[len(PKG_DIR) + 1:], "ENTIRE", ranges_doc if n.endswith(".md") else ranges_whole, "paquete B-min, entero")
          for n in dir_files(rev, PKG_DIR)]
    ru = [(RULING_OUT, "dictamen de materialidad de B-min (R20261011T162328Z-5c5e, MATERIAL, ACCREDITED): salida estructurada", "ENTIRE",
           ranges_whole, "dictamen, entero"),
          (RULING_README, "dictamen de materialidad de B-min: README de su kit custodiado", "ENTIRE", ranges_doc, "dictamen, entero")]
    return [(OBJECT, "objeto: la enmienda A-6 (PROPUESTA), entera", "ENTIRE", ranges_a6, "objeto, entero")] + ev + pk + ru


def build(items):
    can, texts, Ls = [], {}, {}
    for p, why, scope, rf, label in items:
        r, t, L = rec(p, REV, why, scope, rf, label)
        can.append(r)
        texts[p], Ls[p] = t, L
    assert len({r["Path"] for r in can}) == len(can), "rutas repetidas en el cierre"
    assert EVIDENCE not in texts, "la evidencia I-62-evidence.md no entra en el cierre"
    return can, texts, Ls


def table(rows):
    w = max(len(r["Path"]) for r in rows)
    return "\n".join("%-*s  blob %s  %s bytes  %s líneas" % (w, r["Path"], r["Blob"], fmt_n(r["Bytes"]), fmt_n(r["Lines"])) for r in rows)


def render_prompt(can, Ls):
    tpl = open(os.path.join(KIT, "prompt.template.md"), encoding="utf-8").read()
    by = {r["Path"]: r for r in can}
    g = {p: {x["Section"]: x for x in by[p]["LineRanges"]} for p in by}
    dec = g[DECISIONS]
    a6 = by[OBJECT]
    a6_map = "; ".join("%s %s" % ((x["Section"] if x["Section"] in ("cabecera",) else
                                   re.sub(r"^#+ ", "", x["Section"]).split(" — ")[0].split(" (")[0]), range_text(x))
                       for x in a6["LineRanges"] if x["Section"] != "entero" and (x["Section"] == "cabecera" or x["Section"].startswith("#")))
    grr = g[GRR]
    st = [x for x in by[SELFTEST]["LineRanges"] if x["Section"].startswith("Summary")][0]
    large = [r["Path"] for r in can if r["Bytes"] > LARGE]
    long_lines = ["%s %s" % (r["Path"], ", ".join(str(n) for n in r["LinesOver2000Chars"])) for r in can if r["LinesOver2000Chars"]]
    grouped = (OBJECT, RULING_OUT, RULING_README)
    entries = [prompt_entry(k + 1, r) for k, r in enumerate([r for r in can if r["Path"] not in grouped and
                                                             not r["Path"].startswith((EVID_DIR + "/", PKG_DIR + "/"))])]
    q = {k: range_text(g[OBJECT]["§8, %s" % k]) for k in ("Q-A6-01", "Q-A6-02", "Q-A6-06", "Q-A6-07", "Q-A6-12")}
    sec = lambda p: range_text([x for x in by[OBJECT]["LineRanges"] if x["Section"].startswith("### " + p + " ") or
                                x["Section"].startswith("## " + p + " ")][0])
    rep = {
        "{{CLONE_COMMIT}}": REV,
        "{{OBJECT_BYTES}}": fmt_n(a6["Bytes"]), "{{OBJECT_LINES}}": fmt_n(a6["Lines"]),
        "{{A6_MAP}}": a6_map,
        "{{EVID_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(EVID_DIR + "/")]),
        "{{EVID_COUNT}}": str(sum(1 for p in by if p.startswith(EVID_DIR + "/"))),
        "{{PKG_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(PKG_DIR + "/")]),
        "{{PKG_COUNT}}": str(sum(1 for p in by if p.startswith(PKG_DIR + "/"))),
        "{{RULING_TABLE}}": table([by[RULING_OUT], by[RULING_README]]),
        "{{DEC77}}": range_text(dec["## 77. Disposición del Coordinator: A-6 lista para revisión y decisión OWNER-RESERVED sobre ADR-0046 #9 (2026-10-11 UTC)"]),
        "{{DEC77_SOURCE}}": range_text(dec["§77, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC77_P1}}": range_text(dec["§77, punto 1"]), "{{DEC77_P2}}": range_text(dec["§77, punto 2"]),
        "{{DEC77_P4}}": range_text(dec["§77, punto 4"]),
        "{{GRR_HEAD}}": range_text(grr["Mode, Base y Head (L2-L4)"]), "{{GRR_RESULT}}": range_text(grr["Result de conjunto"]),
        "{{GRR_LINES}}": str(by[GRR]["Lines"]),
        "{{ST_TAIL}}": range_text(st),
        "{{A6_LINE_A}}": str(OG.A6_LINE_NO), "{{A6_LINE_B}}": str(OG.A6_LINE_NO + 1),
        "{{A6_S33}}": sec("3.3"), "{{A6_S36}}": sec("3.6"), "{{A6_S38}}": sec("3.8"), "{{A6_S312}}": sec("3.12"),
        "{{A6_S315}}": sec("3.15"), "{{A6_S5}}": sec("5."), "{{A6_S62}}": sec("6.2"), "{{A6_S7}}": sec("7."), "{{A6_S9}}": sec("9."),
        "{{A6_S1}}": sec("1."), "{{A6_S313}}": sec("3.13"),
        "{{Q01}}": q["Q-A6-01"], "{{Q02}}": q["Q-A6-02"], "{{Q06}}": q["Q-A6-06"], "{{Q07}}": q["Q-A6-07"], "{{Q12}}": q["Q-A6-12"],
        "{{CLOSURE_LIST}}": "\n".join(entries),
        "{{LARGE_FILES}}": ", ".join("`%s`" % p for p in large),
        "{{LONG_LINES}}": "; ".join(long_lines) if long_lines else "ninguna",
    }
    for k, v in rep.items():
        assert k in tpl, "marcador ausente en la plantilla: %s" % k
        tpl = tpl.replace(k, v)
    assert not re.search("[{][{][A-Z0-9_]+[}][}]", tpl), "marcadores sin sustituir en el prompt"
    return tpl.rstrip("\n")


def load_params():
    p = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
    assert re.match(r"^[0-9a-f]{40}$", p.get("CloneCommit") or ""), "kit-params.json: CloneCommit no es un SHA completo"
    return p


def main(argv):
    global CLONE, RUN, REPO, REV
    if "--preview" in argv:
        REPO, REV = WORKTREE, PREP_COMMIT
        can, texts, Ls = build(group_items(REV) + canonical)
        print(json.dumps({"files": len(can), "bytes": sum(r["Bytes"] for r in can),
                          "ranges": {r["Path"]: [(g["Section"][:70], g["Start"], g["End"]) for g in r["LineRanges"] if g["Section"] != "entero"]
                                     for r in can if not r["Path"].startswith((EVID_DIR + "/", PKG_DIR + "/"))}}, ensure_ascii=False, indent=1))
        return 0
    params = load_params()
    assert params.get("Provisional") is False, "kit-params.json provisional: el cierre real exige Provisional = false"
    assert params.get("Clone") == CLONE_FIXED and params.get("Run") == RUN_FIXED, "kit-params.json con otro clon o run"
    REPO = CLONE
    REV = params["CloneCommit"]
    head = git("rev-parse", "HEAD").decode().strip()
    assert head == REV, "HEAD del clon (%s) distinto de CloneCommit (%s)" % (head, REV)
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    ident = identity_checks()
    can, texts, Ls = build(group_items(REV) + canonical)
    assert order_text_check(Ls[DECISIONS]), "decisiones §77 no declara el SHA-256 y los bytes del cuerpo pegado"
    prompt = render_prompt(can, Ls)
    if "--render-prompt" in argv:
        with open(os.path.join(KIT, "prompt.md"), "w", encoding="utf-8", newline="\n") as f:
            f.write(prompt)
        b = prompt.encode("utf-8")
        print(json.dumps({"Written": "prompt.md", "Bytes": len(b), "Lines": len(lines_of(prompt)), "Sha256": hashlib.sha256(b).hexdigest()}))
        return 0
    assert open(os.path.join(KIT, "prompt.md"), "rb").read() == prompt.encode("utf-8"), "kit/prompt.md distinto del renderizado: --render-prompt"
    listing = sorted(os.listdir(RUN))
    assert listing == sorted(RUN_FILES), "el run debe tener exactamente %s (tiene %s)" % (RUN_FILES, listing)
    run, corpus = {}, set()
    for r in can:
        corpus |= {c for c in texts[r["Path"]] if ord(c) > 127}
    for n in RUN_FILES:
        b = open(os.path.join(RUN, n), "rb").read()
        assert b == open(os.path.join(KIT, n), "rb").read(), "%s del run distinto de la copia del kit" % n
        d, t = describe(b, n)
        run[n] = d
        if n != "prompt.md":
            corpus |= {c for c in t if ord(c) > 127}
    num, want, size = ORDER_TEXT
    assert run["order.txt"]["Sha256"] == want and run["order.txt"]["Bytes"] == size, "order.txt distinto del cuerpo §77 custodiado"
    pb = open(os.path.join(RUN, "prompt.md"), "rb").read()
    assert not pb.endswith(b"\n") and b"\r" not in pb, "prompt.md con salto final o CR"
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §77 del Coordinator (2 918 bytes; SHA-256 e23faecb…, el que declara decisiones §77 en "
                            "el commit); su punto 4 (líneas 19-24) ordena esta revisión y su punto 2 (líneas 10-14) fija los cuatro temas",
               "prompt.md": "kit/prompt.md renderizado desde kit/prompt.template.md; stdin literal del proceso claude-cli; nunca es premisa"}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    run_hashes["order.txt"]["IdentityCheck"] = {"DecisionsSection": "§" + num, "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": size}
    for r in can:
        r.pop("PromptLabel", None)
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "PreparationCommit": PREP_COMMIT, "Provisional": False,
        "ObjectKind": "FILE", "ObjectPath": OBJECT, "ObjectBlob": OBJECT_BLOB, "ObjectFiles": {OBJECT: OBJECT_BLOB},
        "Publication": {"Commit": PUBLICATION, "Parent": PUB_PARENT, "GuardsRunCommit": GUARDS_RUN_COMMIT},
        "PackagePath": None,
        "PackageNote": "sin paquete del Architect: el prompt hace de paquete (A-6 §6 y el Anexo prevén la revisión formal con una disposición nueva: §77)",
        "Identity": ident,
        "PriorFindingIds": [], "PreviousObject": None,
        "Question": "decisiones §77, punto 4 (order.txt, líneas 19-24): revisión formal de A-6 con veredictos explícitos sobre los cuatro temas del "
                    "punto 2 (líneas 10-14) y sobre los nueve deltas",
        "Topics": TOPICS, "RequiredTopics": REQUIRED_TOPICS, "Deltas": DELTAS, "MaterialityIds": MATERIALITY_IDS, "MaterialityA6": MATERIALITY_A6,
        "OwnerGate": {"Check": OG.CHECK, "A6LineSha256": OG.A6_LINE_SHA, "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA,
                      "Source": "%s L%d (blob %s) = %s L%d" % (OG.A6_PATH, OG.A6_LINE_NO, OG.A6_BLOB[:8], OG.CARD_PATH, OG.CARD_LINE_NO),
                      "Note": "la línea del Owner no está en el clon (el clon es anterior a su registro); launch.py la exige en las decisiones del "
                              "worktree en el momento del lanzamiento (owner_gate.py)"},
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ["--add-dir", RUN], "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ["--add-dir", RUN] + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "AddDirStatus": "medido en C3 de la caracterización R20261008T183600Z-char sobre la plantilla exacta con marcadores (decisiones §56, "
                                      "punto 7; §57, punto 3): kit/transport-gate.json en MEASURED con el SHA-256 del registro fijado",
                      "Gate": "kit/transport-gate.json (transport_gate.py, byte a byte el de los kits de A-4, A-5 y B-min)",
                      "Stdin": "prompt.md del run (bytes literales)",
                      "JsonSchema": "kit/result.schema.json en forma compacta: json.dumps(separators=(',', ':'), ensure_ascii=True)",
                      "ExpectedInit": {"tools": ["Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "dontAsk", "mcp_servers": [],
                                       "claude_code_version": "2.1.293"},
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a6\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S, "AuthSettleSeconds": AUTH_SETTLE_S,
                      "AuthSettle": "launch.py espera AUTH_SETTLE_S segundos entre el final de `auth status` y el lanzamiento, y registra como "
                                    "TRANSPORT_AUTH_REFRESH_CONFLICT un result con «Failed to refresh OAuth token» y 0 tokens (sin reintento)",
                      "Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; §57, punto 3): C1 "
                                          "completada, C3 PASS sobre la plantilla con --add-dir"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos dos archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json, "
                                "con la reproducción de las guardas de A-6) y el auditor lo vuelve a verificar después; el revisor contrasta lo que "
                                "puede leer: order.txt, decisiones §77 (bytes, SHA-256, puntos 1, 2 y 4), el resultado `run` de las guardas y la "
                                "autoprueba (A6TextBlob)",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` orienta (Scope SECTIONS o HEADER) o cubre el archivo entero (ENTIRE). Los rangos no "
                       "limitan: un archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo",
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run (I-62-A6/ y bmin/ están enteros en el cierre)",
            "LargeFileThresholdBytes": LARGE, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > LARGE],
            "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can if r["LinesOver2000Chars"]},
            "ClauseRefs": "toda referencia (`ruta:línea` o `ruta:inicio-fin`) apunta a líneas leídas enteras y entregadas fielmente por Read: de archivos "
                          "del cierre (ruta relativa al clon) o de order.txt (nombre suelto); nunca de prompt.md",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "docs/automation/evidence/I-62-evidence.md (evidencia §140-§150)",
             "Reason": "excluida: contiene los resúmenes y valoraciones de la sesión que preparó A-6 y este kit; el revisor juzga sin ese anclaje"},
            {"Source": "la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud futura de medición (I-62-F6/requests/)",
             "Reason": "materiales de la sesión que prepara; las líneas del Owner están literales en A-6 §7"},
            {"Source": "probe_tools.py, los reference.json / comparison.json de las sondas y los análisis citados por la evidencia",
             "Reason": "excluidos: la revisión se hace contra el texto normativo, no contra el oráculo de C-24 (§75.3)"},
            {"Source": "WORKFLOW, el mapa de cláusulas y su esquema, clause_map.py, mv_check.py, gate-contract.v2 y los demás esquemas",
             "Reason": "fuera de la lista de §77.4 y de la sesión principal; las guardas (custodiadas en el cierre) los comprueban mecánicamente"},
            {"Source": "ejecución de las guardas, del paquete B-min o de sus pruebas",
             "Reason": "sin herramientas de ejecución; los resultados custodiados están en el cierre y el invocador reprodujo las guardas"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: correo del usuario (identidad) y estado de Git del cwd (HEAD separado del clon, sin rama, y asuntos de los commits recientes)",
            "adjunto environment: cwd " + CLONE + ", plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para el clon (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión que preparó A-6, del Coordinator y de cualquier otra sesión)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*)",
            "los archivos de " + RUN + " distintos de los dos del run, incluido el subdirectorio launch/",
            "cualquier archivo del clon fuera de CanonicalInputs (también la evidencia I-62-evidence.md y el registro de caracterización)",
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
                      "large": closure["ReadPolicy"]["LargeFiles"], "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=False, indent=1))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
