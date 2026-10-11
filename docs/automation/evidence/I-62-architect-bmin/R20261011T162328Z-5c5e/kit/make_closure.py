"""I-62, decisiones §75, punto 4: EffectiveInputClosure del dictamen independiente y acotado del Architect sobre la MATERIALIDAD del paquete
experimental B-min (docs/automation/evidence/I-62-F6/FX-02/bmin/), por una celda `claude-cli` medida y elegible (autorización del Owner
CLAUDE-CLI-I62 = A: revisiones de ARCHITECT de I-62, solo lectura). No es la revisión formal de una enmienda (§75.4, última frase).

Kit adaptado del kit de la primera revisión formal de A-5 (R20261011T065226Z-dbb0, auditor v5.1-a5, ACCREDITED). Diferencias de estructura:
el objeto es un DIRECTORIO (todos los archivos de bmin/ en el commit del paquete, sea cual sea su lista), que no existía en la base de la
preparación (460665ab, decisiones §75 registradas); el commit del paquete (PKG_COMMIT) no existe cuando se prepara el kit, así que el cierre, el
prompt (kit/prompt.md, renderizado desde kit/prompt.template.md), el clon y el manifiesto se generan después, parametrizados por
kit/kit-params.json (lo escribe prepare_clone.py). El run tiene DOS archivos (order.txt = cuerpo §75 y prompt.md).

Cierre canónico, en el commit del clon (CloneCommit; por defecto = PkgCommit): el paquete entero; AUTOMATION_PLAN 16.4, 16.9, 16.10, 16.13-16.14 y
16.18-16.24; ADR-0046 y ADR-0048; LIFECYCLE §2, §3 (M-01..M-08) y §6; Proposal V14 (§20 AUT-I1, §20.3, Anexo C con C-24 y L2333-L2334, Anexo
D.1-D.6, con la tabla de roles de D.3 y L2572-L2573); A-4 y A-5 enteras; agent-execution README (§5, §8, §10, §14, §15), routing.md y el
descriptor codex-cli; los esquemas role-invocation.v1, input-closure.v1, controller-verification.v2, delegation.v2, worker-handoff (v1) y
gate-contract.v2; los contratos de las invocaciones del Controller de FX-02 (kits/FX-02/controller-contracts.md) y decisiones §73-§75 (la
evidencia §140-§145 queda fuera para evitar el anclaje en la valoración previa de §144). Los rangos se calculan aquí, mecánicamente, desde los encabezados, las filas y las líneas citadas (con su primera línea comprobada); el
script falla si algo no coincide. Cada archivo normativo se compara con el blob que tenía en la base de la preparación; decisiones y evidencia se
comprueban como prefijo exacto (solo añadido) del blob de la base.

Uso:
  python -B make_closure.py --preview                 → solo lectura de git en el worktree de I-62, en la base: imprime archivos y rangos (sin
                                                         paquete); no escribe nada
  python -B make_closure.py --render-prompt           → renderiza kit/prompt.md desde kit/prompt.template.md (clon de kit-params.json)
  python -B make_closure.py                           → escribe kit/closure.json (exige que kit/prompt.md sea el renderizado y que el run
                                                         tenga exactamente order.txt y prompt.md, iguales a las copias del kit)
  --provisional <clon> <run>                          → con cualquiera de los dos anteriores: clon y run provisionales (solo para autoprobar
                                                         el kit antes de que exista el paquete; kit-params.json con Provisional = true)
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

CLONE_FIXED, RUN_FIXED = r"D:\r62-arch-bmin", r"D:\r62-arch-bmin-run"
WORKTREE = os.path.join(os.environ.get("USERPROFILE", ""), ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
BRANCH = "architecture/portabilidad-coordinador-principal"
BASE_AT_PREP = "460665abd46e4f9f75c896eceecabe11689a6a40"   # decisiones §75 registradas (evidencia §145); bmin/ no existe en ella
IDS = {"RunId": "R20261011T162328Z-5c5e", "InvocationId": "I20261011T162328Z-5c5e", "LogicalReviewRequestId": "L20261011T162328Z-5c5e",
       "AttemptSeq": 1}
SESSION_ID = "cd0a1e5c-605d-4363-9e49-57087fd26a47"          # --session-id fijado para el único lanzamiento (uuid4)
PKG_DIR = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SPEC = PKG_DIR + "/SPEC.md"
AP = "docs/AUTOMATION_PLAN.md"
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A4, A5 = "docs/initiatives/I-62-A-4.md", "docs/initiatives/I-62-A-5.md"
AE = "docs/automation/agent-execution/"
RAE, ROUTING, CODEX_CLI = AE + "README.md", AE + "routing.md", AE + "adapters/codex-cli.md"
SCH = AE + "schemas/"
S_RI, S_IC, S_CV2, S_DEL2, S_WH, S_GC2 = (SCH + n for n in ("role-invocation.v1.schema.json", "input-closure.v1.schema.json",
                                                            "controller-verification.v2.schema.json", "delegation.v2.schema.json",
                                                            "worker-handoff.schema.json", "gate-contract.v2.schema.json"))
CC = "docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md"
DECISIONS = "docs/automation/decisions/I-62.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"
CHAR_RECORD = "docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/claude-cli-characterization.json"   # lo fija la compuerta; fuera del cierre
PINNED = {   # blob en la base de la preparación (460665ab); el commit del clon debe conservarlo
    AP: "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5", ADR46: "da275e1a141a750fee2a8f501b36aa280d7aa534",
    ADR48: "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e", LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa", A4: "5e2ba68efdc1b902aa1c28aff4453b4b6973be7f", A5: "ad94ceedacbf3dbf70f0217a583978d0e32592fc",
    RAE: "592dcfd45e8743fd83b3423249fbd3c73e3d3055", ROUTING: "bba08fc4686d7192deeb559752636a467103b4b1",
    CODEX_CLI: "155f3469e345e2397fa26347a9cb52d6fdb91122", S_RI: "d54a7ae783ec83d055babdae090899908b616082",
    S_IC: "6bdf71843e2c70362a5e2d0ea19c8891e22bcff6", S_CV2: "e1faa9eca5c50b2b3a9f91575575614366962720",
    S_DEL2: "dfdbe461270e8fd92200c36bab78b9fdc3385014", S_WH: "cc3c9cf76b2247f3312e99bd447554326cc3700a",
    S_GC2: "f644bb1941656b65c62ee474bf37752d328fd091", CC: "90929ad1c49c028f41f58f654f8713bd0b35f27b",
    CHAR_RECORD: "541956eb8f5d154414b91338c655919ae0ade697",
}
APPEND_ONLY_SINCE_BASE = {DECISIONS: "b761393dfe1d2bcb85548c90c96da931b83d6230", EVIDENCE: "872d52d250eaf0b782a1246a980d9f0d0513fc87"}
RUN_FILES = ["order.txt", "prompt.md"]
PREMISE_RUN_FILES = ["order.txt"]                            # prompt.md nunca
ORDER_TEXT = ("75", "21435cd1c0821d8c934117a10e850cb9fa124a06f8dd589719aff8f5b711d630", 3288)
MATERIALITY_IDS = ["M-0%d" % i for i in range(1, 9)]
SURFACES_REQUIRED = ["AP_16_4", "AP_16_19", "AP_16_24", "ADR_0046", "V14_D3", "CONSUMED_CONTRACTS"]
DIMENSIONS = ["ACTOR", "SESSION", "CONTEXT", "PROVIDER", "C24_ORACLE", "TOOL_ACCEPTOR_DIVERSITY"]
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
TIMEOUT_S = 3600               # igual que en los kits de A-4 y A-5
AUTH_SETTLE_S = 120            # mitigación del intento 1 de A-3: espera entre `auth status` (preflight 5) y el lanzamiento
LARGE = 60000
CLONE, RUN, REPO, REV, PKG_COMMIT, PROVISIONAL = CLONE_FIXED, RUN_FIXED, CLONE_FIXED, None, None, False


def git(*a):
    return subprocess.check_output(["git", "-C", REPO] + list(a))


def git_ok(*a):
    return subprocess.run(["git", "-C", REPO] + list(a), capture_output=True).returncode == 0


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
    return {"Section": label, "Start": first["Start"], "End": last["End"], "Span": True}


def row(lines, rng, pred, label):
    hits = [i + 1 for i in range(rng["Start"] - 1, rng["End"]) if pred(lines[i])]
    assert len(hits) == 1, "fila %r: %d coincidencias en %s" % (label, len(hits), rng)
    return {"Section": label, "Start": hits[0], "End": hits[0]}


def cited(lines, start, end, token, label):
    """Rango citado por número de línea (en el prompt o en la orden): su primera línea debe contener `token`."""
    assert 1 <= start <= end <= len(lines) and token in lines[start - 1], "L%d-L%d: la línea %d no contiene %r" % (start, end, start, token)
    return {"Section": label, "Start": start, "End": end}


def table_at(lines, rng, pred, label):
    """Tabla markdown cuya primera fila cumple `pred`, hasta la primera línea que no empieza por `|`."""
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
    """Ítems `N. **…**` de primer nivel dentro de una sección: cada uno hasta la línea anterior al siguiente ítem o al final de la sección."""
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
    s = {k: section(L, k + " ") for k in ("16.4", "16.9", "16.10", "16.13", "16.14", "16.18", "16.19", "16.20", "16.21", "16.22", "16.23", "16.24")}
    s24 = s["16.24"]
    return [span(s["16.13"], s["16.14"], "16.13-16.14"), span(s["16.18"], s["16.24"], "16.18-16.24")] + [s[k] for k in s] + [
        row(L, s["16.4"], lambda x: x.startswith("**Transporte:**"), "16.4, transporte"),
        row(L, s["16.4"], lambda x: x.startswith("**Receta de Codex:**"), "16.4, receta de Codex"),
        table_at(L, s["16.9"], lambda x: x.startswith("| # | Id | Comprueba |"), "16.9, tabla de las catorce comprobaciones"),
        cited(L, 731, 731, "| Mapa de cláusulas |", "16.13, mapa en I62_EFFECTIVE_SHA (L731)"),
        cited(L, 902, 908, "| MV-1 |", "16.13, Validate(M): MV-1..MV-7 (L902-L908)"),
        cited(L, 945, 946, "**Punto efectivo:**", "16.14, punto efectivo (L945-L946)"),
        cited(L, 1045, 1046, "**Contrato del adapter.**", "16.19, contrato del adapter (L1045-L1046)"),
        cited(L, 1048, 1052, "**Descriptor.**", "16.19, descriptor y Facts (L1048-L1052)"),
        cited(L, 1264, 1264, "| `CanonicalInputs[]` |", "16.24, clase CanonicalInputs (L1264)"),
        cited(L, 1265, 1267, "| `AllowedTransitiveInputs[]` |", "16.24, clases AllowedTransitiveInputs, DeclaredRuntimeContext y ForbiddenInputs (L1265-L1267)"),
        cited(L, 1271, 1274, "2. cada obligación se clasifica", "16.24, cálculo del cierre, paso 2 (L1271-L1274)"),
        row(L, s24, lambda x: x.startswith("**Identidad del revisor.**"), "16.24, identidad del revisor"),
        row(L, s24, lambda x: x.startswith("**Fidelidad de los insumos**"), "16.24, fidelidad de los insumos"),
        table_at(L, s24, lambda x: x.startswith("| P-22 |"), "16.24, P-22..P-25")]


def ranges_v14(L):
    s20, s203, s2031 = section(L, "20.2 "), section(L, "20.3 "), section(L, "20.3.1 ")
    intro_end = s2031["Start"] - 1
    while L[intro_end - 1].strip() == "":
        intro_end -= 1
    s203 = {"Section": "§20.3, introducción (hasta §20.3.1)", "Start": s203["Start"], "End": intro_end}
    annex_c = section(L, "Anexo C ")
    d = {k: section(L, k + " ") for k in ("D.1", "D.2", "D.3", "D.4", "D.5", "D.6")}
    return [cited(L, 1039, 1047, "**Invariante de autoridad (AUT-I1).**", "§20.2, invariante de autoridad AUT-I1 (L1039-L1047)"), s20, s203, s2031,
            cited(L, 1079, 1079, "| `CanonicalInputs[]` |", "§20.3.1, clase CanonicalInputs (L1079)"),
            annex_c, cited(L, 2333, 2334, "Esperado independiente", "Anexo C, esperado independiente (L2333-L2334)"),
            cited(L, 2363, 2363, "| C-24 |", "Anexo C, C-24 (L2363)"),
            span(d["D.1"], d["D.6"], "Anexo D, D.1-D.6")] + [d[k] for k in d] + [
            cited(L, 2433, 2447, "**Topología A**", "D.3, Topología A: tabla de roles, adapters y topes (L2433-L2447)"),
            cited(L, 2572, 2573, "| **Dependencias técnicas permitidas** |", "D.6, dependencias técnicas permitidas y privadas prohibidas (L2572-L2573)")]


def ranges_lifecycle(L):
    s3 = section(L, "3. ")
    return [section(L, "2. "), s3] + [row(L, s3, (lambda m: lambda x: x.startswith("| **%s** |" % m))(m), "§3, %s" % m) for m in MATERIALITY_IDS] + [
        dict(row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "§3, UNKNOWN cuenta como activado"),
             End=row(L, s3, lambda x: "UNKNOWN en un disparador cuenta como activado" in x, "")["End"] + 1),
        row(L, s3, lambda x: x.startswith("Un cambio de alcance, no-objetivos, decision Owner"), "§3, OWNER-RESERVED"),
        section(L, "6. ")]


def ranges_adr(L):
    dec = section(L, "Decisión")
    return whole(L) + [header_block(L)] + all_sections(L) + numbered_items(L, dec, "Decisión")


def ranges_rae(L):
    keep = ("5. ", "8. ", "10. ", "14. ", "14.", "15. ", "15.")
    return whole(L) + [section(L, h[2]) for h in headings(L) if h[1] in (2, 3) and h[2].startswith(keep)]


def ranges_a5(L):
    return whole(L) + [header_block(L)] + all_sections(L) + [
        cited(L, 175, 199, "> «**Construcción del fixture conforme al mapa de cláusulas (A-5).**", "§3.2, párrafo A5-1 (L175-L199)"),
        cited(L, 178, 186, "> 1. **F_seed**", "§3.2, A5-1, reglas 1-2 (L178-L186)")]


def ranges_decisions(L):
    s = {n: section(L, "%d. " % n) for n in (73, 74, 75)}
    s75 = s[75]
    rows75 = [row(L, s75, (lambda k: lambda x: x.startswith("| **%d. " % k))(k), "§75, punto %d" % k) for k in range(1, 9)]
    src = row(L, s75, lambda x: x.startswith("pegado, extraído de la transcripción de la sesión, suma "), "§75, bytes y SHA-256 del cuerpo pegado")
    return [span(s[73], s[75], "§73-§75")] + [s[n] for n in (73, 74, 75)] + [src] + rows75


def ranges_evidence(L):
    s = {n: section(L, "%d. " % n) for n in range(140, 146)}
    return [span(s[140], s[145], "§140-§145")] + [s[n] for n in range(140, 146)]


def ranges_doc(L):
    return whole(L) + [header_block(L)] + all_sections(L)


def ranges_whole(L):
    return whole(L)


# (ruta, por qué, alcance, función de rangos, etiqueta corta para el prompt)
canonical = [
    (AP, "AUTOMATION_PLAN: 16.4 (transporte, relevo, receta de Codex), 16.9 (las catorce comprobaciones), 16.10, 16.13-16.14 (compatibilidad, mapa, "
         "punto efectivo) y 16.18-16.24 (observación de capacidad, frontera de adapters, binding, independencia, aceptación, invocación de rol, cierre "
         "de insumos y fidelidad; CanonicalInputs en L1264; clasificación READ/ACTION_* en L1271-L1274)", "SECTIONS", ranges_ap),
    (ADR46, "ADR-0046 aceptado (protocolo de ejecución delegada; #6 verificación fail-closed; #9 nivel A, sin scripts operativos)", "ENTIRE", ranges_adr),
    (ADR48, "ADR-0048 aceptado (roles, binding por capacidad, independencia por dimensiones, nivel A)", "ENTIRE", ranges_adr),
    (LIFECYCLE, "LIFECYCLE §2, §3 (M-01..M-08; UNKNOWN cuenta como activado; OWNER-RESERVED) y §6 (enmiendas)", "SECTIONS", ranges_lifecycle),
    (V14, "Proposal V14 congelada: §20.2 (AUT-I1, L1039-L1047), §20.3 y §20.3.1 (CanonicalInputs, L1079), Anexo C (L2333-L2334; C-24, L2363) y "
          "Anexo D.1-D.6 (tabla de roles de D.3, L2433-L2447; D.6, L2572-L2573)", "SECTIONS", ranges_v14),
    (A4, "A-4 acordada (A4-1: ruta del Controller ante una limitación medida; A4-6)", "ENTIRE", ranges_doc),
    (A5, "A-5 acordada (B1; A5-1, reglas 1-2, L178-L186)", "ENTIRE", ranges_a5),
    (RAE, "procedimiento de agent-execution: §5 (invocación del Controller), §8 (coherencia de la verificación), §10 (controles negativos), §14 "
          "(binding, independencia, A1'-A8') y §15 (invocación de rol, cierre y fidelidad)", "SECTIONS", ranges_rae),
    (ROUTING, "enrutamiento estable (clases, elegibilidad de una celda, el propio Controller, requisitos por perfil y acción)", "ENTIRE", ranges_doc),
    (CODEX_CLI, "descriptor del adapter codex-cli (operaciones, hechos, límites)", "ENTIRE", ranges_doc),
    (S_RI, "esquema rackcad-role-invocation/v1 (contrato consumido)", "ENTIRE", ranges_whole),
    (S_IC, "esquema rackcad-input-closure/v1 (contrato consumido)", "ENTIRE", ranges_whole),
    (S_CV2, "esquema rackcad-controller-verification/v2 (contrato consumido)", "ENTIRE", ranges_whole),
    (S_DEL2, "esquema rackcad-delegation/v2 (contrato consumido)", "ENTIRE", ranges_whole),
    (S_WH, "esquema rackcad-worker-handoff/v1 (contrato consumido)", "ENTIRE", ranges_whole),
    (S_GC2, "esquema rackcad-gate-contract/v2 (contrato consumido; la SPEC de B-min, §2.4, lee sus nombres de campo)", "ENTIRE", ranges_whole),
    (CC, "contratos de las invocaciones del Controller de FX-02 (codex-cli, solo lectura): qué recibe y qué devuelve cada invocación; sin valores "
         "esperados", "ENTIRE", ranges_doc),
    (DECISIONS, "decisiones §73 (A-5 AGREED; sonda 2 al Controller), §74 (CONTROLLER_V5; estrategia nueva) y §75 (B-min; esta revisión, punto 4)",
     "SECTIONS", ranges_decisions),
    # evidencia §140-§145 EXCLUIDA por la supervisión: §144 recoge la valoración previa de la sesión que prepara («dudosa (M-03/M-04 probable)») y
    # su recomendación; el revisor independiente decide sin ese anclaje (decisiones §75.4). Las decisiones §73-§75 sí entran.
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


def package_files(rev):
    names = [n for n in git("ls-tree", "-r", "--name-only", rev, "--", PKG_DIR + "/").decode("utf-8").split("\n") if n]
    assert names and all(n.startswith(PKG_DIR + "/") for n in names), "el commit no tiene el paquete B-min en %s/" % PKG_DIR
    assert SPEC in names, "el paquete no tiene SPEC.md"
    for n in names:
        assert re.match(r"^[A-Za-z0-9_./-]+$", n), "ruta del paquete con caracteres fuera de [A-Za-z0-9_./-]: %s" % n
    return sorted(names)


def rec(p, rev, why, scope, rf, label):
    b = git("show", rev + ":" + p)
    r, t = describe(b, p)
    blob = git("rev-parse", rev + ":" + p).decode().strip()
    r = dict({"Path": p, "Blob": blob}, **r)
    if p in PINNED:
        assert blob == PINNED[p], "%s: blob %s distinto del de la base de la preparación (%s)" % (p, blob, PINNED[p])
        r["PinnedBlobAtBase"] = PINNED[p]
    if p in APPEND_ONLY_SINCE_BASE and rev != BASE_AT_PREP:
        old = git("cat-file", "-p", APPEND_ONLY_SINCE_BASE[p])
        assert b.startswith(old), "%s: el blob de la base no es prefijo exacto del commit" % p
        r["AppendOnlySinceBase"] = {"BaseBlob": APPEND_ONLY_SINCE_BASE[p], "BaseLines": len(lines_of(old.decode("utf-8"))),
                                    "Note": "el texto de la base es prefijo exacto del texto del commit (solo añadido)"}
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


def identity_checks(pkg_names):
    for c in (PKG_COMMIT, REV, BASE_AT_PREP):
        assert git_ok("cat-file", "-e", c + "^{commit}"), "commit ausente en el clon: %s" % c
    assert git_ok("merge-base", "--is-ancestor", BASE_AT_PREP, PKG_COMMIT) and PKG_COMMIT != BASE_AT_PREP, "la base no es antecesora estricta del paquete"
    assert git_ok("merge-base", "--is-ancestor", PKG_COMMIT, REV), "el commit del paquete no es antecesor (o igual) del commit del clon"
    assert not git_ok("cat-file", "-e", BASE_AT_PREP + ":" + PKG_DIR), "el paquete ya existía en la base"
    t_pkg = git("rev-parse", PKG_COMMIT + ":" + PKG_DIR).decode().strip()
    t_rev = git("rev-parse", REV + ":" + PKG_DIR).decode().strip()
    assert t_pkg == t_rev, "el árbol del paquete difiere entre el commit del paquete y el del clon"
    changed = [x for x in git("diff", "--name-status", BASE_AT_PREP, REV).decode("utf-8").split("\n") if x]
    return {"BaseAtPreparation": BASE_AT_PREP, "PkgCommit": PKG_COMMIT, "CloneCommit": REV, "PackageTree": t_pkg,
            "PackageAbsentAtBase": True, "PackageTreeEqualAtPkgAndClone": True, "PackageFiles": len(pkg_names),
            "ChangedSinceBase": changed,
            "ChangedOutsidePackage": [x for x in changed if not x.split("\t")[-1].startswith(PKG_DIR + "/")]}


def fmt_n(n):
    return "{:,}".format(n).replace(",", " ")


def range_text(g):
    return ("%d" % g["Start"]) if g["Start"] == g["End"] else ("%d-%d" % (g["Start"], g["End"]))


def short_label(path, s):
    """Etiqueta breve de un rango para el prompt: «16.4» en AUTOMATION_PLAN, «§N» en las secciones numeradas, «D.n» y «Anexo X» en V14."""
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
    rs = [g for g in r["LineRanges"] if g["Section"] not in ("entero", "cabecera") and not g.get("Span")]
    head = "%d. `%s` (%s bytes, %s líneas; blob `%s`)" % (k, r["Path"], fmt_n(r["Bytes"]), fmt_n(r["Lines"]), r["Blob"][:8])
    body = r["PromptLabel"]
    if rs:
        body += ": " + "; ".join("%s %s" % (short_label(r["Path"], g["Section"]), range_text(g)) for g in rs)
    return head + ", " + body + "."


def build(rev_items):
    can, texts, Ls = [], {}, {}
    for p, why, scope, rf, label in rev_items:
        r, t, L = rec(p, REV, why, scope, rf, label)
        can.append(r)
        texts[p], Ls[p] = t, L
    assert len({r["Path"] for r in can}) == len(can), "rutas repetidas en el cierre"
    return can, texts, Ls


def items_for(pkg_names):
    pk = [(n, "objeto: paquete B-min, %s" % n[len(PKG_DIR) + 1:], "ENTIRE", ranges_doc if n.endswith(".md") else ranges_whole,
           "objeto (paquete B-min), entero") for n in pkg_names]
    return pk + [(p, w, s, f, lb) for p, w, s, f, lb in
                 [(c[0], c[1], c[2], c[3], {AP: "16.4, 16.9, 16.10, 16.13-16.14 y 16.18-16.24", ADR46: "entero", ADR48: "entero",
                                            LIFECYCLE: "§2, §3 y §6", V14: "§20.2, §20.3, Anexo C y Anexo D.1-D.6", A4: "entera", A5: "entera",
                                            RAE: "§5, §8, §10, §14 y §15", ROUTING: "entero", CODEX_CLI: "entero", S_RI: "entero", S_IC: "entero",
                                            S_CV2: "entero", S_DEL2: "entero", S_WH: "entero", S_GC2: "entero", CC: "entero",
                                            DECISIONS: "§73-§75", EVIDENCE: "§140-§145"}[c[0]]) for c in canonical]]


def render_prompt(can, Ls, pkg_names, ident):
    tpl = open(os.path.join(KIT, "prompt.template.md"), encoding="utf-8").read()
    by = {r["Path"]: r for r in can}
    pkg = [by[n] for n in pkg_names]
    w = max(len(r["Path"]) for r in pkg)
    table = "\n".join("%-*s  blob %s  %s bytes  %s líneas" % (w, r["Path"], r["Blob"], fmt_n(r["Bytes"]), fmt_n(r["Lines"])) for r in pkg)
    dec = by[DECISIONS]["LineRanges"]
    g = {x["Section"]: x for x in dec}
    spec_head = next(x for x in by[SPEC]["LineRanges"] if x["Section"] == "cabecera")
    spec_head_text = " ".join(Ls[SPEC][spec_head["Start"] - 1:spec_head["End"]])
    assert "§75" in spec_head_text, "la cabecera de SPEC.md no nombra las decisiones §75: el Paso 0, punto 4, del prompt no se cumpliría"
    large = [r["Path"] for r in can if r["Bytes"] > LARGE]
    long_lines = ["%s %s" % (r["Path"], ", ".join(str(n) for n in r["LinesOver2000Chars"])) for r in can if r["LinesOver2000Chars"]]
    entries = [prompt_entry(k + 1, r) for k, r in enumerate([r for r in can if not r["Path"].startswith(PKG_DIR + "/")])]
    rep = {
        "{{PKG_COMMIT}}": PKG_COMMIT, "{{CLONE_COMMIT}}": REV, "{{PKG_COUNT}}": str(len(pkg)), "{{PKG_TABLE}}": table,
        "{{PKG_BYTES}}": fmt_n(sum(r["Bytes"] for r in pkg)),
        "{{DEC75}}": range_text(g["## 75. Disposición del Coordinator: preparación de B-min y recuperación del Controller (2026-10-11 UTC)"]),
        "{{DEC75_SOURCE}}": range_text(g["§75, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC75_P4}}": range_text(g["§75, punto 4"]),
        "{{SPEC_HEADER}}": range_text(spec_head),
        "{{CLOSURE_LIST}}": "\n".join(entries),
        "{{LARGE_FILES}}": ", ".join("`%s`" % p for p in large),
        "{{LONG_LINES}}": "; ".join(long_lines) if long_lines else "ninguna",
    }
    for k, v in rep.items():
        assert k in tpl, "marcador ausente en la plantilla: %s" % k
        tpl = tpl.replace(k, v)
    assert "{{" not in tpl and "}}" not in tpl, "marcadores sin sustituir en el prompt"
    return tpl.rstrip("\n")


def load_params():
    p = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
    for k in ("PkgCommit", "CloneCommit"):
        assert re.match(r"^[0-9a-f]{40}$", p.get(k) or ""), "kit-params.json: %s no es un SHA completo" % k
    return p


def main(argv):
    global CLONE, RUN, REPO, REV, PKG_COMMIT, PROVISIONAL
    if "--preview" in argv:
        REPO, REV = WORKTREE, BASE_AT_PREP
        can, texts, Ls = build(items_for([]))
        print(json.dumps({"files": {r["Path"]: (r["Bytes"], r["Lines"], r["Blob"][:8], r["LinesOver2000Chars"]) for r in can},
                          "ranges": {r["Path"]: [(g["Section"][:70], g["Start"], g["End"]) for g in r["LineRanges"]] for r in can}},
                         ensure_ascii=False, indent=1))
        return 0
    params = load_params()
    if "--provisional" in argv:
        i = argv.index("--provisional")
        CLONE, RUN = argv[i + 1], argv[i + 2]
        assert params.get("Provisional") is True, "--provisional exige kit-params.json con Provisional = true"
        PROVISIONAL = True
    else:
        assert params.get("Provisional") is False, "kit-params.json provisional: el cierre real exige Provisional = false"
        assert params.get("Clone") == CLONE_FIXED and params.get("Run") == RUN_FIXED, "kit-params.json con otro clon o run"
    REPO = CLONE
    PKG_COMMIT, REV = params["PkgCommit"], params["CloneCommit"]
    head = git("rev-parse", "HEAD").decode().strip()
    assert head == REV, "HEAD del clon (%s) distinto de CloneCommit (%s)" % (head, REV)
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    pkg_names = package_files(REV)
    ident = identity_checks(pkg_names)
    can, texts, Ls = build(items_for(pkg_names))
    dec_lines = Ls[DECISIONS]
    assert order_text_check(dec_lines), "decisiones §75 no declara el SHA-256 y los bytes del cuerpo pegado"
    prompt = render_prompt(can, Ls, pkg_names, ident)
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
    assert run["order.txt"]["Sha256"] == want and run["order.txt"]["Bytes"] == size, "order.txt distinto del cuerpo §75 custodiado"
    pb = open(os.path.join(RUN, "prompt.md"), "rb").read()
    assert not pb.endswith(b"\n") and b"\r" not in pb, "prompt.md con salto final o CR"
    sources = {"order.txt": "cuerpo exacto pegado de la disposición §75 del Coordinator (3 288 bytes; SHA-256 21435cd1…, el que declara decisiones §75 en "
                            "el commit); su punto 4 (líneas 27-40) ordena este dictamen",
               "prompt.md": "kit/prompt.md renderizado desde kit/prompt.template.md; stdin literal del proceso claude-cli; nunca es premisa"}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    run_hashes["order.txt"]["IdentityCheck"] = {"DecisionsSection": "§" + num, "Sha256DeclaredInDecisions": True, "BytesDeclaredInDecisions": size}
    pkg = [r for r in can if r["Path"].startswith(PKG_DIR + "/")]
    for r in can:
        r.pop("PromptLabel", None)
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "PkgCommit": PKG_COMMIT, "BaseAtPreparation": BASE_AT_PREP, "Provisional": PROVISIONAL,
        "ObjectKind": "DIRECTORY", "ObjectDir": PKG_DIR, "ObjectFiles": {r["Path"]: r["Blob"] for r in pkg}, "PackagePath": None,
        "PackageNote": "no es la revisión formal de una enmienda (§75.4, última frase): no hay paquete del Architect; el prompt hace de paquete",
        "Identity": ident,
        "PriorFindingIds": [], "PreviousObject": None,
        "Question": "decisiones §75, punto 4 (order.txt, línea 30)",
        "Deliverables": ["ApplicableClauses", "Materiality (M-01..M-08)", "AffectedSurfaces", "Independence", "A6", "OwnerReserved",
                         "ActionCompatibility (AP 16.24, paso 2)", "CanonicalInputAdmissible (AP L1264)", "SingleInvocationMeasurement (observación)"],
        "MaterialityIds": MATERIALITY_IDS, "SurfacesRequired": SURFACES_REQUIRED, "Dimensions": DIMENSIONS,
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ["--add-dir", RUN], "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ["--add-dir", RUN] + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "AddDirStatus": "medido en C3 de la caracterización R20261008T183600Z-char sobre la plantilla exacta con marcadores (decisiones §56, "
                                      "punto 7; §57, punto 3): kit/transport-gate.json en MEASURED con el SHA-256 del registro fijado",
                      "Gate": "kit/transport-gate.json (transport_gate.py, byte a byte el de los kits de A-4 y A-5)",
                      "Stdin": "prompt.md del run (bytes literales)",
                      "JsonSchema": "kit/result.schema.json en forma compacta: json.dumps(separators=(',', ':'), ensure_ascii=True)",
                      "ExpectedInit": {"tools": ["Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "dontAsk", "mcp_servers": [],
                                       "claude_code_version": "2.1.293"},
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-bmin\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": TIMEOUT_S, "AuthSettleSeconds": AUTH_SETTLE_S,
                      "AuthSettle": "launch.py espera AUTH_SETTLE_S segundos entre el final de `auth status` y el lanzamiento, y registra como "
                                    "TRANSPORT_AUTH_REFRESH_CONFLICT un result con «Failed to refresh OAuth token» y 0 tokens (sin reintento)",
                      "Characterization": "claude-cli-characterization.json de R20261008T183600Z-char (decisiones §56, punto 7; §57, punto 3): C1 "
                                          "completada, C3 PASS sobre la plantilla con --add-dir"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos dos archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json) "
                                "y el auditor lo vuelve a verificar después; el revisor contrasta lo que puede leer: order.txt, decisiones §75 (bytes, "
                                "SHA-256 y punto 4), el conjunto de archivos del paquete (Glob sobre bmin/) y la cabecera de SPEC.md",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1); el prompt es la "
                            "única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` orienta (Scope SECTIONS) o cubre el archivo entero (ENTIRE). Los rangos no limitan: un "
                       "archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo",
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run (el Paso 0, punto 3, usa bmin/, todo él en el cierre)",
            "LargeFileThresholdBytes": LARGE, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > LARGE],
            "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can if r["LinesOver2000Chars"]},
            "ClauseRefs": "toda referencia (`ruta:línea` o `ruta:inicio-fin`) apunta a líneas leídas enteras y entregadas fielmente por Read: de archivos "
                          "del cierre (ruta relativa al clon) o de order.txt (nombre suelto); nunca de prompt.md; un Excerpt de ApplicableClauses es "
                          "texto literal de esas líneas (≤ 200 caracteres; espacios colapsados)",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "análisis citados por la evidencia y no incluidos (controller-strategy.md, mv-static-analysis.md, mv4_readings.py, "
                       "controller-failure-analysis.md, controller-options.md), el resto de kits/FX-02/ y la corrida CONTROLLER_V5",
             "Reason": "citas descriptivas; el cierre lo fija la orden §75.4 y la lista de la sesión principal"},
            {"Source": "probe_tools.py y los reference.json / comparison.json de las sondas",
             "Reason": "excluidos expresamente: el dictamen se hace contra el texto normativo, no contra el oráculo de C-24 (§75.3)"},
            {"Source": "WORKFLOW, el mapa de cláusulas y su esquema, clause_map.py, mv_check.py y los demás esquemas de agent-execution",
             "Reason": "la SPEC de B-min los nombra como entradas de órdenes concretas; la materialidad del uso no depende de su contenido"},
            {"Source": "pruebas del paquete B-min (ejecución)", "Reason": "sin herramientas de ejecución; el README del paquete trae los resultados"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: correo del usuario (identidad) y estado de Git del cwd (HEAD separado del clon, sin rama, y asuntos de los commits recientes)",
            "adjunto environment: cwd " + CLONE + ", plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para el clon (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión que preparó B-min, de sus implementadores, del Coordinator y de cualquier otra sesión)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*)",
            "los archivos de " + RUN + " distintos de los dos del run, incluido el subdirectorio launch/",
            "cualquier archivo del clon fuera de CanonicalInputs (también el registro de caracterización que fija la compuerta)",
            "Grep o Glob cuyo alcance sea un directorio con archivos fuera del cierre; listados de directorios",
            "red (fetch, web)", "invocar otros agentes o subagentes", "escribir, hacer commit o push",
        ],
        "CorpusDistinctNonAscii": len(corpus),
    }
    with open(os.path.join(KIT, "closure.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(closure, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps({"ids": IDS, "provisional": PROVISIONAL, "canonical": len(can), "bytes": sum(r["Bytes"] for r in can), "package": len(pkg),
                      "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in RUN_FILES},
                      "large": closure["ReadPolicy"]["LargeFiles"], "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=False, indent=1))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
