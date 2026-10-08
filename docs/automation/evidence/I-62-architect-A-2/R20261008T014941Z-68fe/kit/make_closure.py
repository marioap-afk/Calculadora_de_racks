"""I-62 A-2 (candidata, blob d47f71b6, introducida por b280f707, publicada en 4a059afa): EffectiveInputClosure de la UNA revisión formal del
Architect preparada según la disposición del Coordinator (decisiones §55, punto 8: paquete completo y HUMAN_LAUNCH_REQUIRED).

Kit adaptado del kit v4 de la revisión r5 de A-1 (R20261005T151303Z-c64c). El cierre canónico es la tabla de insumos del paquete §2 en el commit de
publicación; los transitivos siguen la solución de GAP-07 de A-1 (opción A: las lecturas iniciales obligatorias de CLAUDE.md, AGENTS.md y el Context
Pack de I-62 entran como `AllowedTransitiveInputs`). Los rangos de líneas se calculan aquí, mecánicamente, desde los encabezados y las filas del texto
en el commit; cada rango se comprueba (título exacto y fila única) y el script falla si algo no coincide.

El contrato de acciones es el de A-1 (ID-1/2, MD-1, NAV-1, RD-1/2, EX-1/2, OWN-1/2) con las rutas, los commits y el arnés de A-2: EX-1 es solo
`a2-guards.py self-test --out <salida propia>` (el modo `run` necesita `origin/main`, que el clon no tiene).

Correcciones del verificador, fijadas antes del lanzamiento: `ToolPolicy` y `Tool` por acción (git, python y el resto solo con Bash; PowerShell
solo para EX-2, Set-Location y Get-Content); pares de diff en orden exacto y revisiones de rev-parse/cat-file -p limitadas al commit o HEAD; la
fila de P-07 queda sin resolver, con las dos lecturas del cierre (V14 §9.3 y AUTOMATION_PLAN §16.11, con §16.8); `RunFileHashes` incluye
prompt.md, que debe ser idéntico en el run y en el kit; las premisas pueden citar order.txt.

Escribe closure.json y corpus.json junto a este script (el directorio del kit). No escribe en el clon ni en el repositorio.
"""
import hashlib
import json
import os
import re
import subprocess

KIT = os.path.dirname(os.path.abspath(__file__))
REV = "4a059afa85dce5a82f74120cad88c5ebadb61c7a"        # commit de publicación (el siguiente al de A-2; solo añade a2-guards-result.json)
A2_COMMIT = "b280f7079e128113666e85a2f437e629d0f0d99e"  # commit que introduce A-2
A2_BASE = "c9f9419dc4aed32242c59de489a8e78a8e369f9b"    # base de A-2 (A2_BASE de a2-guards.py)
CLONE = r"D:\r62-arch-a2"
RUN = r"D:\r62-arch-a2-run"
IDS = {"RunId": "R20261008T014941Z-68fe", "InvocationId": "I20261008T014941Z-68fe", "LogicalReviewRequestId": "L20261008T014941Z-68fe",
       "AttemptSeq": 1}
OBJECT = "docs/initiatives/I-62-A-2.md"
PKG = "docs/initiatives/I-62-architect-package-A-2.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A1 = "docs/initiatives/I-62-A-1.md"
HARNESS = "docs/automation/evidence/I-62-A2/a2-guards.py"
GUARDS_RESULT = "docs/automation/evidence/I-62-A2/a2-guards-result.json"
B2 = "docs/automation/evidence/I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/"
EXPECTED_BLOBS = {
    OBJECT: "d47f71b66ba86c31f857f0d8c2d2437636947af0",
    PKG: "f45a2384eb1a3c9ec5654995c5b88f6c51b6742f",
    HARNESS: "91c9c2a46a4a9327ecfff44fa52a4d48e0e33935",
    GUARDS_RESULT: "33ffbb9deb114972cfd11ade3ab448a2af1f50dd",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f",
    "docs/initiatives/I-62-consensus-freeze.md": "0f6b860837e2478db8525e1fb30a485bc4646816",
    "docs/INITIATIVE_LIFECYCLE.md": "f19896a8f1a7c82f74bac7231636f68462a87271",
}


def git(*a):
    return subprocess.check_output(["git", "-C", CLONE] + list(a))


def text_of(p):
    return git("show", REV + ":" + p).decode("utf-8")


# ------------------------------------------------------------------ rangos mecánicos
def headings(lines):
    out = []
    fence = False
    for i, l in enumerate(lines):
        if l.startswith("```"):
            fence = not fence
            continue
        m = re.match(r"^(#{1,6}) (.*)$", l)
        if m and not fence:
            out.append((i + 1, len(m.group(1)), m.group(2)))
    return out


def section(lines, prefix, label=None):
    """Rango [inicio, fin] de la sección cuyo título empieza por `prefix` (único); termina antes del siguiente encabezado de nivel ≤."""
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


def whole(lines):
    return [{"Section": "entero", "Start": 1, "End": len(lines)}]


def ranges_object(L):
    hs = headings(L)
    first = hs[1][0] if len(hs) > 1 else len(L)
    out = [{"Section": "cabecera (bloque de identidad)", "Start": 1, "End": first - 2 if L[first - 2].strip() == "" else first - 1}]
    for p in ("1. Alcance", "2. A2-P1", "2.1 ", "2.2 ", "2.3 ", "2.4 ", "3. A2-P2", "3.1 ", "3.2 ", "3.3 ", "4. ", "5. ", "6. ", "7. ", "8. ", "9. ", "10. "):
        out.append(section(L, p))
    s33 = section(L, "3.3 ")
    lv = row(L, s33, lambda x: x.startswith("**Lectura vigente, que no forma parte del delta**"), "3.3, «Lectura vigente» (no forma parte del delta)")
    out.append({"Section": lv["Section"], "Start": lv["Start"], "End": s33["End"]})
    return out


def ranges_package(L):
    return whole(L) + [section(L, p) for p in ("1. ", "2. ", "3. ", "4. ", "5. ", "6. ")]


def ranges_v14(L):
    d = section(L, "Anexo D ")
    s17, s18, s93 = section(L, "17. "), section(L, "18. "), section(L, "9.3 ")
    d3 = section(L, "D.3 ")
    out = [d, d3, section(L, "D.4 "), section(L, "D.5 "), section(L, "D.6 "), section(L, "D.8 "),
           row(L, d3, lambda x: x.startswith("**Totales por ronda de F6**"), "D.3, «Totales por ronda de F6»"),
           row(L, d3, lambda x: x.strip() == "Ningún tope autoriza gasto ni ejecución ahora.", "D.3, ancla de A-2 («Ningún tope autoriza gasto ni ejecución ahora.»)"),
           s17, row(L, s17, lambda x: x.startswith("| **F6** |"), "§17, fila F6"),
           s18, row(L, s18, lambda x: x.startswith("| OD-2 |"), "§18, fila OD-2"),
           s93, row(L, s93, lambda x: x.startswith("|") and "P-07" in x, "§9.3, fila que nombra P-07 (fila «invocaciones»)")]
    return out


def ranges_automation_plan(L):
    """Transitivo entero; se señalan además §16.8 y §16.11 con la fila que define P-07 (una de las dos lecturas de «la fila de P-07 en §8» del
    paquete §2, que el kit no resuelve; la otra es la fila de V14 §9.3 que nombra P-07)."""
    s168, s1611 = section(L, "16.8 "), section(L, "16.11 ")
    return whole(L) + [s168, row(L, s168, lambda x: "STOP (P-07)" in x, "§16.8, regla del tope de invocaciones (STOP, P-07)"),
                       s1611, row(L, s1611, lambda x: x.startswith("| P-07 |"), "§16.11, fila que define P-07")]


def ranges_a1(L):
    hs = headings(L)
    hdr = {"Section": "cabecera (formato de una A-n)", "Start": 1, "End": hs[1][0] - 1}
    while L[hdr["End"] - 1].strip() == "":
        hdr["End"] -= 1
    return whole(L) + [hdr, section(L, "2. FC-01"), section(L, "4. Materialidad"), section(L, "11. Veredictos")]


def ranges_lifecycle(L):
    return [section(L, "3. "), section(L, "5. "), section(L, "6. ")]


def ranges_decisions(L):
    s = [section(L, "%d. " % n) for n in range(51, 56)]
    return [span(s[0], s[-1], "§51-§55")] + s


def ranges_evidence(L):
    s = {n: section(L, "%d. " % n) for n in (64, 67, 70, 71, 72, 73, 74, 75, 78, 79, 80)}
    return [s[64], s[67], span(s[70], s[75], "§70-§75"), span(s[78], s[80], "§78-§80")] + [s[n] for n in (70, 71, 72, 73, 74, 75, 78, 79, 80)]


canonical = [
    (OBJECT, "objeto de la revisión: A-2 candidata, entera", "ENTIRE", ranges_object),
    (PKG, "paquete del Architect (veredicto pedido §1, insumos §2, preguntas §4, condiciones §5)", "ENTIRE", ranges_package),
    (V14, "Freeze: cláusulas que A-2 enmienda o lee (paquete §2): Anexo D (D.3, D.4, D.5, D.6, D.8), §17 fila F6, §18 fila OD-2 y la fila de P-07",
     "SECTIONS", ranges_v14),
    (A1, "formato de una A-n acordada; presupuestos de bucle y principio sin reinicios que A-2 no debe alterar (paquete §2)", "SECTIONS", ranges_a1),
    ("docs/initiatives/I-62-consensus-freeze.md", "identidad del Freeze", "ENTIRE", lambda L: whole(L)),
    ("docs/INITIATIVE_LIFECYCLE.md", "§3 (M-01..M-08), §5 (REQUIRED) y §6 (formato de A-n)", "SECTIONS", ranges_lifecycle),
    ("docs/automation/decisions/I-62.md", "§51-§55: OD-2d-PROBE, selección de B, B1 INVALID_TEST_ORACLE y la disposición §55 con el texto candidato",
     "SECTIONS", ranges_decisions),
    ("docs/automation/evidence/I-62-evidence.md", "§64, §67, §70-§75 y §78-§80: sondas consumidas, actualizaciones automáticas de Codex, B1, B2 e "
     "instantánea del origen", "SECTIONS", ranges_evidence),
    (B2 + "b2-report-for-coordinator.md", "hechos de B2 (KICKOFF_SCHEMA_NOT_DELIVERED, FAIL bruto 2/23)", "ENTIRE", lambda L: whole(L)),
    (B2 + "comparison-v2-analysis.json", "hechos de B2: análisis de la comparación con el contrato v2", "ENTIRE", lambda L: whole(L)),
    (HARNESS, "guardas mecánicas G1-G5 (evidencia de apoyo, no autoridad)", "ENTIRE", lambda L: whole(L)),
    (GUARDS_RESULT, "resultado de `a2-guards.py run --head b280f707` (su campo Head nombra el commit de A-2)", "ENTIRE", lambda L: whole(L)),
]
transitive = [
    ("CLAUDE.md", "AUTOMATIC_INSTRUCTION", "el runtime la inyecta"),
    ("docs/HANDOFF.md", "READ", "CLAUDE.md «Lectura inicial» 1; AGENTS.md «Leer primero» 1"),
    ("AGENTS.md", "READ", "CLAUDE.md «Lectura inicial» 2"),
    ("docs/WORKFLOW.md", "READ", "CLAUDE.md «Lectura inicial» 3; Context Pack documentation-governance; WORKFLOW §4.2 (rebase al abrir)"),
    ("docs/ARCHITECTURE.md", "READ", "CLAUDE.md «Lectura inicial» 4; AGENTS.md «Leer primero» 3"),
    ("docs/ROADMAP.md", "READ", "CLAUDE.md «Lectura inicial» 5; Context Pack"),
    ("docs/context-packs/README.md", "READ", "CLAUDE.md «Lectura inicial» 6; AGENTS.md «Leer primero» 3"),
    ("docs/initiatives/I-62-portabilidad-coordinador-principal.md", "READ",
     "CLAUDE.md «Lectura inicial» 6 y AGENTS.md «Leer primero» 3: el contrato declara el Context Pack de I-62 (frontmatter context_packs)"),
    ("docs/context-packs/documentation-governance.md", "READ", "Context Pack declarado por I-62 (contrato, context_packs)"),
    ("README.md", "READ", "AGENTS.md «Leer primero» 2"),
    ("docs/FOUNDATIONS.md", "READ", "Context Pack documentation-governance, required_docs"),
    ("docs/AUTOMATION_PLAN.md", "READ", "Context Pack documentation-governance, required_docs"),
    ("docs/initiatives/README.md", "READ", "Context Pack documentation-governance, required_docs"),
    ("docs/initiatives/PROMPT_TEMPLATES.md", "READ", "Context Pack documentation-governance, required_docs"),
]


def rec(p):
    b = git("show", REV + ":" + p)
    t = b.decode("utf-8")
    lines = t.replace("\r\n", "\n").split("\n")
    if lines and lines[-1] == "":
        lines = lines[:-1]
    r = {"Path": p, "Blob": git("rev-parse", REV + ":" + p).decode().strip(), "Bytes": len(b), "Lines": b.count(b"\n"),
         "Sha256": hashlib.sha256(b).hexdigest(), "LinesOver2000Chars": [i + 1 for i, x in enumerate(t.split("\n")) if len(x) > 2000],
         "Encoding": {"Charset": "UTF-8", "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF"}}
    if p in EXPECTED_BLOBS:
        assert r["Blob"] == EXPECTED_BLOBS[p], "blob inesperado de %s: %s" % (p, r["Blob"])
    return r, t, lines


def question_ids(L):
    s8 = section(L, "8. ")
    ids = [m.group(1) for m in (re.match(r"^\| (Q-A2-\d\d) \|", L[i]) for i in range(s8["Start"] - 1, s8["End"])) if m]
    assert ids == ["Q-A2-%02d" % n for n in range(1, len(ids) + 1)] and ids, ids
    return ids


def focus_items(L):
    s4 = section(L, "4. ")
    items = [int(m.group(1)) for m in (re.match(r"^(\d+)\. ", L[i]) for i in range(s4["Start"] - 1, s4["End"])) if m]
    assert items == list(range(1, len(items) + 1)), items
    return items


assert git("rev-parse", "HEAD").decode().strip() == REV, "HEAD del clon distinto del commit"
assert git("status", "--porcelain").decode().strip() == "", "clon no limpio"
corpus, can, tra = set(), [], []
qids, focus = None, None
for p, why, scope, rf in canonical:
    r, t, L = rec(p)
    r["Why"], r["Scope"], r["LineRanges"] = why, scope, rf(L)
    for g in r["LineRanges"]:
        assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
    can.append(r)
    corpus |= {c for c in t if ord(c) > 127}
    if p == OBJECT:
        qids = question_ids(L)
    if p == PKG:
        focus = focus_items(L)
for p, cls, req in transitive:
    r, t, L = rec(p)
    r["Class"], r["RequiredBy"], r["Scope"], r["LineRanges"] = cls, req, "ENTIRE", (ranges_automation_plan(L) if p == "docs/AUTOMATION_PLAN.md" else whole(L))
    for g in r["LineRanges"]:
        assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
    tra.append(r)
    corpus |= {c for c in t if ord(c) > 127}
order_b = open(os.path.join(RUN, "order.txt"), "rb").read()
prompt_b = open(os.path.join(RUN, "prompt.md"), "rb").read()
assert prompt_b == open(os.path.join(KIT, "prompt.md"), "rb").read(), "prompt.md del run distinto de la copia del kit"
assert order_b == open(os.path.join(KIT, "order.txt"), "rb").read(), "order.txt del run distinto de la copia del kit"
OWN ="salidas propias del revisor: archivos que la propia sesión revisora crea en su scratchpad o en su directorio de tareas " \
      "(%USERPROFILE%/AppData/Local/Temp/claude/D--Documentos-Worktrees-r62-arch-a2-<worktree>/<sesión>/…)"
FOCUS_PACKAGE_ITEMS = [i for i in focus if i != 5]   # el ítem 5 del paquete §4 son las preguntas Q-A2 (QuestionDispositions)
closure = {
    "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
    "Ids": IDS, "AuthorityRevision": REV, "ObjectIntroducedBy": A2_COMMIT, "DiffBases": [A2_BASE, A2_COMMIT],
    "DiffPairs": [[A2_BASE, A2_COMMIT, "delta que introduce A-2"], [A2_COMMIT, REV, "publicación: solo añade a2-guards-result.json"]],
    "ObjectPath": OBJECT, "Harness": HARNESS, "HarnessModes": ["self-test"],
    "FocusItems": len(FOCUS_PACKAGE_ITEMS), "FocusSource": "paquete §4, preguntas %s (la 5 son las Q-A2 de A-2 §8)" % ", ".join(map(str, FOCUS_PACKAGE_ITEMS)),
    "QuestionIds": qids, "QuestionSource": "A-2 §8 (el paquete §1 y §4 nombran Q-A2-01..Q-A2-05; A-2 §8 contiene %d)" % len(qids),
    "Adapter": "claude-desktop-session: sesión nueva y separada que el Owner abre con un clic desde una tarjeta de tarea de la app, con cwd en el clon "
               "limpio " + CLONE + " (rama main = el commit exacto; sin remoto); la app crea un worktree del clon para la tarea "
               "(D:\\Documentos\\Worktrees\\r62-arch-a2\\<nombre>)",
    "RunDir": RUN, "RunFiles": ["order.txt", "prompt.md"],
    "RunFileHashes": {"order.txt": {"Bytes": len(order_b), "Sha256": hashlib.sha256(order_b).hexdigest(),
                                    "Source": "cuerpo exacto pegado por el Owner de la disposición del Coordinator (decisiones §55)"},
                      "prompt.md": {"Bytes": len(prompt_b), "Sha256": hashlib.sha256(prompt_b).hexdigest(),
                                    "Source": "copia de kit/prompt.md; primer mensaje literal de la sesión revisora"}},
    "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el kit se comprueba después contra kit-manifest.json",
    "CanonicalInputs": can, "AllowedTransitiveInputs": tra,
    "ReadPolicy": {
        "Closure": "el cierre es por archivo; `LineRanges` señala las secciones que pide el paquete §2 (Scope SECTIONS) o el archivo entero (ENTIRE)",
        "Read": "ruta absoluta de un archivo del cierre, bajo el clon o bajo el worktree propio (que está en el mismo commit), con offset y limit; también order.txt y prompt.md, y salidas propias",
        "Grep": "solo con `path` = un ARCHIVO del cierre; nunca un directorio ni `glob`",
        "LargeFileThresholdBytes": 60000, "MaxRangeLines": 300,
        "LargeFiles": [r["Path"] for r in can + tra if r["Bytes"] > 60000],
        "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can + tra if r["LinesOver2000Chars"]},
        "Premises": "toda premisa (PremiseRefs) se cita de líneas leídas con Read, o con `git -C D:/r62-arch-a2 show " + REV[:8] + ":<ruta> | sed -n 'N,Mp'` para las líneas largas, de archivos del cierre o de order.txt (Path = " + RUN + "\\order.txt, leída con Read); nunca de prompt.md; una lectura fallida nunca cuenta",
        "FailedReads": "un comando que falla o no devuelve contenido (también un git show que Git rechaza) es una lectura fallida: no acredita contenido",
        "AbsentCommits": "los commits del Freeze (b64a3b64) y de V14 (4c617e82) son anteriores a un rebase y no están en el clon: V14 se identifica por su blob en el commit (34ad80ea)",
    },
    "ToolPolicy": "git, python, sha256sum y los demás comandos de ID-1, ID-2, MD-1, RD-1, RD-2 y EX-1, y la redirección, sed -i y mkdir -p de OWN-2, "
                  "solo con la herramienta Bash; PowerShell solo para EX-2 (dotnet test), Set-Location (NAV-1) y Get-Content de archivos del cierre, del run "
                  "o de salidas propias (tubería solo a Select-Object, Measure-Object u Out-String); Read, Grep, Write y Edit según ReadPolicy",
    "AllowedActions": [
        {"Id": "ID-1", "Tool": "Bash", "Action": "git (con -C D:/r62-arch-a2, -C <worktree propio o un subdirectorio suyo> o desde el cwd del clon o del worktree): rev-parse HEAD | rev-parse <commit>:<ruta del cierre> | cat-file -p|-t|-s <commit>:<ruta del cierre>, con <commit> = " + REV[:8] + " o HEAD (cat-file -t|-s también con " + A2_BASE[:8] + " o " + A2_COMMIT[:8] + ") | status [--porcelain] | log --oneline [-N] | worktree list | config --get <clave>",
         "RequiredBy": "identidad del objeto y del árbol; AGENTS.md «Leer primero» 4", "Class": "ACTION_COMPATIBLE"},
        {"Id": "ID-2", "Tool": "Bash", "Action": "git checkout --detach " + REV + " en el worktree propio, solo si su HEAD no es el commit", "RequiredBy": "fijar el árbol de la sesión revisora", "Class": "ACTION_COMPATIBLE"},
        {"Id": "MD-1", "Tool": "Bash", "Action": "metadatos y hashing enumerados: git hash-object <archivo del cierre o salida propia> (sin -w ni --stdin); git diff [--stat|--numstat|--no-color|--word-diff|-U<N>] <base> <destino> -- <rutas del cierre>, con exactamente dos revisiones y en este orden: " + A2_BASE[:8] + " " + A2_COMMIT[:8] + " (el delta que introduce A-2) o " + A2_COMMIT[:8] + " " + REV[:8] + " (la publicación; HEAD vale por " + REV[:8] + "); sha256sum y wc sobre archivos del cierre, del run o salidas propias; pwd; date",
         "RequiredBy": "delta exacto de A-2 y de su publicación; hashes de identidad", "Class": "ACTION_COMPATIBLE"},
        {"Id": "NAV-1", "Tool": "Bash (cd) o PowerShell (Set-Location)", "Action": "cd (o Set-Location en PowerShell) hacia la raíz o CUALQUIER SUBDIRECTORIO del clon o del worktree propio, o hacia un directorio de salidas propias. Cambiar de directorio no autoriza leer nada fuera del cierre: las rutas posteriores se resuelven desde ahí y se clasifican por su efecto",
         "RequiredBy": "lecturas relativas y ejecución de EX-1/EX-2", "Class": "ACTION_COMPATIBLE"},
        {"Id": "RD-1", "Tool": "Bash", "Action": "git show " + REV[:8] + ":<ruta del cierre> (o HEAD:<ruta del cierre> con HEAD ya verificado), sola o con tubería a sed -n, head, tail, wc, grep, awk, sha256sum, sort, uniq, cut, tr o nl. La ruta de un objeto Git es de árbol: Git no normaliza `..` y un git show rechazado es una lectura fallida",
         "RequiredBy": "lectura fiel de líneas largas y metadatos de archivos del cierre", "Class": "ACTION_COMPATIBLE"},
        {"Id": "RD-2", "Tool": "Bash", "Action": "sed -n, wc, awk, grep (sin -r, -R ni --recursive), head, tail, cat, sha256sum, sort, uniq, cut, tr, nl, diff, cmp, echo, printf, true y read, solo sobre ARCHIVOS del cierre (bajo el clon o el worktree propio), sobre order.txt y prompt.md, o sobre salidas propias; combinables con |, ;, &&, || y bucles for/while; redirección solo hacia salidas propias o /dev/null; ningún directorio como argumento",
         "RequiredBy": "metadatos de solo lectura (tamaños, líneas largas, secciones) y comprobación del hash del prompt", "Class": "ACTION_COMPATIBLE"},
        {"Id": "EX-1", "Tool": "Bash", "Action": "python D:/r62-arch-a2/" + HARNESS + " self-test --out <salida propia> (exactamente esos argumentos; el modo run no está disponible en el clon, que no tiene origin/main, y no está permitido); y python (heredoc, -c o un script propio) cuyas rutas literales sean solo archivos del cierre, archivos del run y salidas propias; sin listar directorios, sin procesos, sin red y sin borrar ni mover archivos",
         "RequiredBy": "verificación de G5 (vectores, mutantes y cobertura de reglas; el self-test no calcula el vínculo con el texto de A-2) de las guardas", "Class": "ACTION_COMPATIBLE"},
        {"Id": "EX-2", "Tool": "Bash o PowerShell", "Action": "dotnet test tests/RackCad.Tests/RackCad.Tests.csproj dentro del clon (Bash o PowerShell; SDK en %LOCALAPPDATA%\\Microsoft\\dotnet\\dotnet.exe), con el log en una salida propia",
         "RequiredBy": "AGENTS.md «Leer primero» 4 y CLAUDE.md «Comandos esenciales»", "Class": "ACTION_COMPATIBLE",
         "Note": "la orden no trae exención; escribe solo bin/obj del clon desechable; comprobación local del revisor, no evidencia de gate"},
        {"Id": "OWN-1", "Tool": "Read, Bash o PowerShell (Get-Content)", "Action": "leer (Read, cat, tail, grep, Get-Content) las salidas propias", "RequiredBy": "comprobar pruebas y tareas de la corrida", "Class": "OWN_OUTPUT", "Note": OWN},
        {"Id": "OWN-2", "Tool": "Write, Edit o Bash (redirección, mkdir -p, sed -i)", "Action": "crear y editar scripts y archivos temporales propios SOLO en las salidas propias: Write, Edit, redirección, mkdir -p y sed -i (declarado). Ningún permiso sobre salidas propias autoriza editar insumos canónicos, scripts custodiados, fuentes del repositorio ni archivos de otra sesión",
         "RequiredBy": "scripts ad hoc de verificación (p. ej., vectores propios con el modelo de G5)", "Class": "OWN_OUTPUT", "Note": OWN},
    ],
    "HealthSignals": [
        {"Kind": "PUBLICATION_CI", "RunRef": "37714804772 (push, head_sha exacto 4a059afa, cuatro jobs requeridos success)", "Sha": REV,
         "Note": "señal separada de salud de publicación; NO es evidencia equivalente a Core local"},
        {"Kind": "PUBLICATION_CI", "RunRef": "37714368467 (push, head_sha exacto b280f707, success)", "Sha": A2_COMMIT,
         "Note": "CI del commit que introduce A-2; señal de salud, no prueba local"},
    ],
    "NotTriggered": [
        {"Source": "CLAUDE.md, validación de dibujo (docs/guias/validacion-manual-autocad.md)", "Reason": "condicionada a una validación de dibujo, que la revisión no hace"},
        {"Source": "documentation-governance optional_docs y code_globs", "Reason": "opcionales u orientativos"},
        {"Source": "enlaces de A-2, del paquete y de la evidencia a otros documentos fuera del cierre (p. ej., clause_map.py, ADR-0048, el tablero de F6, state/I-62.yml, el fixture)",
         "Reason": "citas descriptivas; el cierre lo fija la tabla de insumos del paquete §2"},
        {"Source": "a2-guards.py run", "Reason": "G4 necesita origin/main, que el clon no tiene; su resultado sobre b280f707 está en a2-guards-result.json"},
    ],
    "DeclaredRuntimeContext": [
        "instrucciones base de la aplicación de escritorio y del runtime (no inspeccionables por el invocador)",
        "CLAUDE.md del worktree (mismo commit que el clon), inyectado automáticamente",
        "~/.claude/CLAUDE.md de usuario: no existe (medido)",
        "memoria automática: el clon es otro repositorio (D:\\r62-arch-a2), sin directorio de proyecto previo en ~/.claude/projects (medido), así que no carga la memoria del proyecto de la sesión autora",
        "herramientas, skills y conectores habilitados por defecto en una sesión nueva",
    ],
    "ForbiddenInputs": [
        "transcripción y memoria de la sesión autora (~/.claude/projects/D--Documentos-Codex-Calculadora-de-racks/*) y toda otra entrada de ~/.claude/projects",
        "otras sesiones, incluidas las de revisiones anteriores y las de F6 (B1, B2)",
        "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones de revisiones anteriores (D:\\r62-arch-*, salvo D:\\r62-arch-a2)",
        "el fixture de F6 (D:\\r62-fixture\\*), incluida la instantánea fx04a-qh2-origin.git",
        "los archivos de " + RUN + " distintos de order.txt y prompt.md",
        "cualquier archivo fuera de CanonicalInputs y AllowedTransitiveInputs, salvo las salidas propias",
        "Grep o búsqueda sobre directorios; listados de directorios",
        "red (fetch, web)", "invocar otros agentes, subagentes, Worker o Controller", "escribir, hacer commit o push en el repositorio",
    ],
    "CorpusDistinctNonAscii": len(corpus),
}
with open(os.path.join(KIT, "closure.json"), "w", encoding="utf-8", newline="\n") as f:
    json.dump(closure, f, ensure_ascii=False, indent=1)
    f.write("\n")
with open(os.path.join(KIT, "corpus.json"), "w", encoding="utf-8", newline="\n") as f:
    json.dump(sorted(corpus), f, ensure_ascii=False)
print(json.dumps({"ids": IDS, "canonical": len(can), "transitive": len(tra), "corpus": len(corpus), "questions": qids, "focus": FOCUS_PACKAGE_ITEMS,
                  "large": [(r["Path"], r["Bytes"], r["Lines"]) for r in can + tra if r["Bytes"] > 60000],
                  "long": closure["ReadPolicy"]["LongLines"],
                  "ranges": {r["Path"]: [(g["Section"][:50], g["Start"], g["End"]) for g in r["LineRanges"]] for r in can}}, ensure_ascii=False, indent=1))
