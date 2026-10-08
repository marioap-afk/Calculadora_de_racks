"""I-62 A-2 corregida (blob f1e1d6f0, corrección 3bbaabef, publicada en fd411b13): EffectiveInputClosure de la UNA re-revisión formal del
Architect por una celda `claude-cli` medida y elegible (decisiones §56, puntos 3 y 6-9; autorización del Owner CLAUDE-CLI-I62 = A).

Kit adaptado del kit de la revisión anterior de A-2 (R20261008T014941Z-68fe, auditor v4-a2.1, NOT_ACCREDITED por decisiones §56, punto 1). El cierre
canónico es la tabla de insumos del paquete §2 en el commit de publicación. Los rangos de líneas se calculan aquí, mecánicamente, desde los
encabezados y las filas del texto en el commit; cada rango se comprueba (título exacto y fila única) y el script falla si algo no coincide.

Cambios frente al kit anterior, fijados por la sesión principal antes del lanzamiento:
  * herramientas del revisor: SOLO Read, Grep y Glob (más StructuredOutput, que el runtime añade para el resultado); sin Bash, sin PowerShell y sin
    scripts, así que no hay acciones de ejecución (ID-*, MD-1, RD-1, EX-*, OWN-*) ni salidas propias;
  * la identidad del clon la verifica el invocador antes del lanzamiento (verify_clone.py) y la vuelve a verificar el auditor después;
  * archivos del run (clase RUN, con hash en el cierre): order.txt (cuerpo de decisiones §56), prompt.md (stdin literal), delta.diff (salida de
    `git diff b553608c fd411b13 -- docs/initiatives/I-62-A-2.md`) y A-2.d47f71b6.md (`git show b280f707:docs/initiatives/I-62-A-2.md`);
  * sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md (caracterización medida), así que la opción A de GAP-07 no se activa;
  * --add-dir del run no está en la lista que midió C1 (desviación D-01): Transport.ArgsTemplate es la plantilla exacta del lanzamiento, y
    launch.py solo lanza si kit/transport-gate.json ata la medición C3 de esa plantilla o una aceptación explícita registrada.

Uso: python make_closure.py            → escribe closure.json junto a este script (el directorio del kit)
     python make_closure.py --ranges   → solo imprime los rangos (para redactar el prompt); no escribe nada
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
REV = "fd411b136f888ecf301f9edaa7fcc068da8dbf22"          # commit de publicación (solo añade a2-guards-result.json)
FIX_COMMIT = "3bbaabef0346d4146585dbf17ac49a1d3a495b0d"   # commit de la corrección de A-2 (decisiones §56)
DELTA_BASE = "b553608ccdac188c45cef0982be0f7da8b5ab2ec"   # base del delta corregido (contiene d47f71b6 sin cambios desde b280f707)
PREV_COMMIT = "b280f7079e128113666e85a2f437e629d0f0d99e"  # commit que introdujo el objeto anterior
PREV_BLOB = "d47f71b66ba86c31f857f0d8c2d2437636947af0"    # objeto de la revisión R20261008T014941Z-68fe
CLONE = r"D:\r62-arch-a2r"
RUN = r"D:\r62-arch-a2r-run"
IDS = {"RunId": "R20261008T184326Z-f1e2", "InvocationId": "I20261008T184326Z-f1e2", "LogicalReviewRequestId": "L20261008T184326Z-f1e2",
       "AttemptSeq": 1}
SESSION_ID = "26a89860-a107-4135-9981-a46f95816d16"       # --session-id fijado en el kit para el único lanzamiento
OBJECT = "docs/initiatives/I-62-A-2.md"
PKG = "docs/initiatives/I-62-architect-package-A-2.md"
REVIEW_RECORD = "docs/initiatives/I-62-architect-review-A-2.md"
PREV_OUTPUT = "docs/automation/evidence/I-62-architect-A-2/R20261008T014941Z-68fe/output.json"
DECISIONS = "docs/automation/decisions/I-62.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
AP = "docs/AUTOMATION_PLAN.md"
A1 = "docs/initiatives/I-62-A-1.md"
FREEZE = "docs/initiatives/I-62-consensus-freeze.md"
LIFECYCLE = "docs/INITIATIVE_LIFECYCLE.md"
EVIDENCE = "docs/automation/evidence/I-62-evidence.md"
B2 = "docs/automation/evidence/I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/"
HARNESS = "docs/automation/evidence/I-62-A2/a2-guards.py"
GUARDS_RESULT = "docs/automation/evidence/I-62-A2/a2-guards-result.json"
EXPECTED_BLOBS = {
    OBJECT: "f1e1d6f08d3cf677500794c7d0019433a4dc7d4e",
    PKG: "5bd0fa609722d9c94aeab38b0d8b486b1d4f1606",
    HARNESS: "b72d26ea6b30b5cfceaa8140a536a08086562b24",
    GUARDS_RESULT: "d8f4845f853fcba8fe79e468312d4845b1c2e76e",
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    A1: "c01899a72b940503bb85a0fab42bc085c603fd0f",
    FREEZE: "0f6b860837e2478db8525e1fb30a485bc4646816",
    LIFECYCLE: "f19896a8f1a7c82f74bac7231636f68462a87271",
}
RUN_FILES = ["order.txt", "prompt.md", "delta.diff", "A-2.d47f71b6.md"]
PREMISE_RUN_FILES = ["order.txt", "delta.diff", "A-2.d47f71b6.md"]   # prompt.md nunca es premisa
ORDER_SHA = "e8b003284843374ba1e8497434f375f3206da2cd9a9829f2f451ff73af06b081"   # decisiones §56: 8 562 bytes
BINARY = {"Path": r"%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe",
          "Sha256": "8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa",
          "Version": "2.1.293 (Claude Code)", "Authenticode": 'Valid; CN="Anthropic, PBC"'}
MEASURED_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose", "--safe-mode",
                  "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk", "--permission-prompts", "none"]
ADD_DIR = ["--add-dir", RUN]   # no medido en C1: lectura del run, fuera del cwd; el lanzamiento lo ata kit/transport-gate.json (D-01)


def git(*a):
    return subprocess.check_output(["git", "-C", CLONE] + list(a))


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
    first = hs[1][0]
    hdr_end = first - 1
    while L[hdr_end - 1].strip() == "":
        hdr_end -= 1
    out = whole(L) + [{"Section": "cabecera (bloque de identidad)", "Start": 1, "End": hdr_end}]
    for p in ("1. Alcance", "2. A2-P1", "2.1 ", "2.2 ", "2.3 ", "2.4 ", "3. A2-P2", "3.1 ", "3.2 ", "3.3 ", "4. ", "5. ", "6. ", "7. ", "8. ",
              "9. ", "10. ", "11. "):
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
    return [d, d3, section(L, "D.4 "), section(L, "D.5 "), section(L, "D.6 "), section(L, "D.8 "),
            row(L, d3, lambda x: x.startswith("**Totales por ronda de F6**"), "D.3, «Totales por ronda de F6»"),
            row(L, d3, lambda x: x.strip() == "Ningún tope autoriza gasto ni ejecución ahora.", "D.3, ancla de A-2 («Ningún tope autoriza gasto ni ejecución ahora.»)"),
            s17, row(L, s17, lambda x: x.startswith("| **F6** |"), "§17, fila F6"),
            s18, row(L, s18, lambda x: x.startswith("| OD-2 |"), "§18, fila OD-2"),
            s93, row(L, s93, lambda x: x.startswith("|") and "P-07" in x, "§9.3, fila que nombra P-07")]


def ranges_automation_plan(L):
    s168, s1611 = section(L, "16.8 "), section(L, "16.11 ")
    return [s168, row(L, s168, lambda x: "STOP (P-07)" in x, "§16.8, regla del tope de invocaciones (STOP, P-07)"),
            s1611, row(L, s1611, lambda x: x.startswith("| P-07 |"), "§16.11, fila que define P-07")]


def ranges_a1(L):
    hs = headings(L)
    hdr = {"Section": "cabecera (formato de una A-n)", "Start": 1, "End": hs[1][0] - 1}
    while L[hdr["End"] - 1].strip() == "":
        hdr["End"] -= 1
    return [hdr, section(L, "2. FC-01"), section(L, "4. Materialidad"), section(L, "11. Veredictos")]


def ranges_lifecycle(L):
    return [section(L, "3. "), section(L, "5. "), section(L, "6. ")]


def ranges_decisions(L):
    s = [section(L, "%d. " % n) for n in range(51, 57)]
    return [span(s[0], s[-1], "§51-§56")] + s


def ranges_evidence(L):
    nums = (64, 67, 70, 71, 72, 73, 74, 75, 78, 79, 80, 81, 82, 83)
    s = {n: section(L, "%d. " % n) for n in nums}
    return [s[64], s[67], span(s[70], s[75], "§70-§75"), span(s[78], s[83], "§78-§83")] + [s[n] for n in nums if n not in (64, 67)]


canonical = [
    (OBJECT, "objeto de la re-revisión: A-2 corregida, entera, con §11 «Cambios frente al blob d47f71b6» (paquete §2)", "ENTIRE", ranges_object),
    (PKG, "paquete de la re-revisión (veredicto pedido §1, insumos §2, resumen §3, preguntas §4, condiciones §5)", "ENTIRE", ranges_package),
    (REVIEW_RECORD, "registro de la revisión anterior (A62-A2-01, O1..O3; paquete §2)", "ENTIRE", lambda L: whole(L)),
    (PREV_OUTPUT, "resultado literal de la revisión anterior R20261008T014941Z-68fe (paquete §2)", "ENTIRE", lambda L: whole(L)),
    (DECISIONS, "§51-§56: OD-2d-PROBE, selección de B, B1, la disposición §55 y la disposición §56 (hallazgos aceptados, claude-cli)",
     "SECTIONS", ranges_decisions),
    (V14, "cláusulas que A-2 enmienda o lee (paquete §2): Anexo D (D.3 completo, D.4, D.5, D.6, D.8), §17 fila F6, §18 fila OD-2 y la fila que "
          "nombra P-07 en la tabla de §9.3", "SECTIONS", ranges_v14),
    (AP, "§16.8 y §16.11: definición de P-07 (§16.11) y su contexto (§16.8) (paquete §2)", "SECTIONS", ranges_automation_plan),
    (A1, "formato de una A-n acordada; presupuestos de bucle y principio sin reinicios que A-2 no debe alterar (paquete §2)", "SECTIONS", ranges_a1),
    (FREEZE, "identidad del Freeze", "ENTIRE", lambda L: whole(L)),
    (LIFECYCLE, "§3 (M-01..M-08), §5 (REQUIRED) y §6 (formato de A-n)", "SECTIONS", ranges_lifecycle),
    (EVIDENCE, "§64, §67, §70-§75 y §78-§83: sondas consumidas, actualizaciones automáticas de Codex, B1, B2, instantánea del origen y revisiones",
     "SECTIONS", ranges_evidence),
    (B2 + "b2-report-for-coordinator.md", "hechos de B2 (KICKOFF_SCHEMA_NOT_DELIVERED, FAIL bruto 2/23)", "ENTIRE", lambda L: whole(L)),
    (B2 + "comparison-v2-analysis.json", "hechos de B2: análisis de la comparación con el contrato v2", "ENTIRE", lambda L: whole(L)),
    (HARNESS, "guardas mecánicas G1-G5 (evidencia de apoyo, no autoridad)", "ENTIRE", lambda L: whole(L)),
    (GUARDS_RESULT, "resultado de `a2-guards.py run` sobre el commit de la corrección (su campo Head nombra 3bbaabef)", "ENTIRE", lambda L: whole(L)),
]


def lines_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    if lines and lines[-1] == "":
        lines = lines[:-1]
    return lines


def describe(b):
    t = b.decode("utf-8")
    return {"Bytes": len(b), "Lines": len(lines_of(t)), "Sha256": hashlib.sha256(b).hexdigest(),
            "LinesOver2000Chars": [i + 1 for i, x in enumerate(t.split("\n")) if len(x) > 2000],
            "Encoding": {"Charset": "UTF-8", "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF",
                         "FinalNewline": b.endswith(b"\n")}}, t


def rec(p):
    b = git("show", REV + ":" + p)
    r, t = describe(b)
    r = dict({"Path": p, "Blob": git("rev-parse", REV + ":" + p).decode().strip()}, **r)
    if p in EXPECTED_BLOBS:
        assert r["Blob"] == EXPECTED_BLOBS[p], "blob inesperado de %s: %s" % (p, r["Blob"])
    return r, t, lines_of(t)


def question_ids(L):
    s8 = section(L, "8. ")
    ids = [m.group(1) for m in (re.match(r"^\| (Q-A2-\d\d) \|", L[i]) for i in range(s8["Start"] - 1, s8["End"])) if m]
    assert ids and ids == ["Q-A2-%02d" % n for n in range(1, len(ids) + 1)], ids
    return ids


def prior_finding_ids(L):
    s11 = section(L, "11. ")
    ids = [m.group(1) for m in (re.match(r"^\| (A62-A2-(?:O\d+|\d\d)) \|", L[i]) for i in range(s11["Start"] - 1, s11["End"])) if m]
    assert ids == ["A62-A2-01", "A62-A2-O1", "A62-A2-O2", "A62-A2-O3"], ids
    return ids


def focus_items(L):
    s4 = section(L, "4. ")
    items = [(int(m.group(1)), L[i]) for i, m in ((i, re.match(r"^(\d+)\. ", L[i])) for i in range(s4["Start"] - 1, s4["End"])) if m]
    assert [n for n, _ in items] == list(range(1, len(items) + 1)), items
    q = [n for n, text in items if "Q-A2-01..Q-A2-07" in text]
    assert q == [len(items)], "el último ítem del paquete §4 debe ser el de las preguntas Q-A2: %s" % q
    return [n for n, _ in items if n not in q], q[0]


def main(print_only):
    assert git("rev-parse", "HEAD").decode().strip() == REV, "HEAD del clon distinto del commit"
    assert git("status", "--porcelain", "--ignored").decode().strip() == "", "clon no limpio"
    can, corpus = [], set()
    qids = prior = focus = qitem = None
    for p, why, scope, rf in canonical:
        r, t, L = rec(p)
        r["Why"], r["Scope"], r["LineRanges"] = why, scope, rf(L)
        for g in r["LineRanges"]:
            assert 1 <= g["Start"] <= g["End"] <= len(L), (p, g)
        can.append(r)
        corpus |= {c for c in t if ord(c) > 127}
        if p == OBJECT:
            qids, prior = question_ids(L), prior_finding_ids(L)
        if p == PKG:
            focus, qitem = focus_items(L)
    ranges = {r["Path"]: [(g["Section"][:60], g["Start"], g["End"]) for g in r["LineRanges"]] for r in can}
    if print_only:
        print(json.dumps({"files": {r["Path"]: (r["Bytes"], r["Lines"], r["Blob"][:8], r["LinesOver2000Chars"]) for r in can},
                          "ranges": ranges, "questions": qids, "prior": prior, "focus": focus}, ensure_ascii=False, indent=1))
        return
    run = {}
    for n in RUN_FILES:
        b = open(os.path.join(RUN, n), "rb").read()
        assert b == open(os.path.join(KIT, n), "rb").read(), "%s del run distinto de la copia del kit" % n
        d, t = describe(b)
        run[n] = d
        if n != "prompt.md":
            corpus |= {c for c in t if ord(c) > 127}
    assert run["order.txt"]["Sha256"] == ORDER_SHA, "order.txt distinto del cuerpo de decisiones §56"
    delta = git("diff", DELTA_BASE, REV, "--", OBJECT)
    assert open(os.path.join(RUN, "delta.diff"), "rb").read() == delta, "delta.diff distinto de la salida de git diff"
    prev = git("show", PREV_COMMIT + ":" + OBJECT)
    assert open(os.path.join(RUN, "A-2.d47f71b6.md"), "rb").read() == prev, "A-2.d47f71b6.md distinto del objeto anterior"
    assert git("hash-object", os.path.join(RUN, "A-2.d47f71b6.md")).decode().strip() == PREV_BLOB
    sources = {"order.txt": "cuerpo exacto pegado por el Owner de la disposición del Coordinator (decisiones §56; 8 562 bytes)",
               "prompt.md": "copia de kit/prompt.md; stdin literal del proceso claude-cli; nunca es premisa",
               "delta.diff": "salida literal de `git diff %s %s -- %s` en el clon" % (DELTA_BASE, REV, OBJECT),
               "A-2.d47f71b6.md": "salida literal de `git show %s:%s` en el clon (blob %s)" % (PREV_COMMIT[:8], OBJECT, PREV_BLOB)}
    run_hashes = {n: {"Bytes": run[n]["Bytes"], "Lines": run[n]["Lines"], "Sha256": run[n]["Sha256"], "LinesOver2000Chars": run[n]["LinesOver2000Chars"],
                      "Encoding": run[n]["Encoding"], "Source": sources[n], "PremiseAllowed": n in PREMISE_RUN_FILES,
                      "LineRanges": [{"Section": "entero", "Start": 1, "End": run[n]["Lines"]}]} for n in RUN_FILES}
    closure = {
        "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
        "Ids": IDS, "AuthorityRevision": REV, "ObjectIntroducedBy": FIX_COMMIT, "DeltaBase": DELTA_BASE,
        "ObjectPath": OBJECT, "ObjectBlob": EXPECTED_BLOBS[OBJECT], "PackagePath": PKG,
        "PreviousObject": {"Blob": PREV_BLOB, "Commit": PREV_COMMIT, "RunFile": "A-2.d47f71b6.md",
                           "Review": "R20261008T014941Z-68fe (CHANGES REQUIRED; NOT_ACCREDITED, decisiones §56, punto 1)"},
        "Harness": HARNESS, "HarnessExecution": "ninguna: el revisor no tiene herramientas de ejecución; lee el código y el resultado custodiado",
        "FocusItems": len(focus), "FocusSource": "paquete §4, preguntas %s (la %d son las Q-A2 de A-2 §8)" % (", ".join(map(str, focus)), qitem),
        "QuestionIds": qids, "QuestionSource": "A-2 §8 (el paquete §1 indica que Q-A2-02 y Q-A2-05 quedan resueltas en el delta; se disponen las %d)" % len(qids),
        "PriorFindingIds": prior, "PriorFindingSource": "A-2 §11 y paquete §1: A62-A2-01 (REQUIRED) y A62-A2-O1..O3 (OPTIONAL), aceptados en decisiones §56, punto 2",
        "PriorFindingDispositions": ["CLOSED", "STILL_OPEN"],
        "Transport": {"Adapter": "claude-cli", "Binary": BINARY, "Model": "claude-opus-5-5", "Effort": "xhigh",
                      "Flags": MEASURED_FLAGS, "AddDir": ADD_DIR, "SessionId": SESSION_ID, "Cwd": CLONE,
                      "ArgsTemplate": TG.templatize(MEASURED_FLAGS + ADD_DIR + ["--session-id", SESSION_ID, "--json-schema", "{}"]),
                      "AddDirStatus": "NO medido en C1 (desviación D-01): launch.py se niega a lanzar mientras kit/transport-gate.json esté en "
                                      "PENDING; lo habilitan MEASURED (sonda C3 de ArgsTemplate, probe-c3.py) o ACCEPTED (aceptación explícita "
                                      "registrada de la sesión principal o del Coordinator); el auditor re-evalúa la compuerta sobre las copias "
                                      "de launch/",
                      "Gate": "kit/transport-gate.json (transport_gate.py)",
                      "Stdin": "prompt.md del run (bytes literales)",
                      "JsonSchema": "kit/result.schema.json en forma compacta: json.dumps(separators=(',', ':'), ensure_ascii=True)",
                      "ExpectedInit": {"tools": ["Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "dontAsk", "mcp_servers": [],
                                       "claude_code_version": "2.1.293"},
                      "TranscriptPath": "%USERPROFILE%\\.claude\\projects\\D--r62-arch-a2r\\" + SESSION_ID + ".jsonl",
                      "LaunchDir": RUN + "\\launch (preflight.json, transport-characterization.json, transport-acceptance.json solo en ACCEPTED, "
                                   "stdout.jsonl, stderr.txt y run.json)", "TimeoutSeconds": 3600,
                      "Characterization": "claude-cli-characterization.json (decisiones §56, punto 7): C1 completada, C2 cancelada por terminación; "
                                          "C3 (lista con --add-dir) pendiente de la sesión principal si elige la vía (a)"},
        "RunDir": RUN, "RunFiles": RUN_FILES, "PremiseRunFiles": PREMISE_RUN_FILES, "RunFileHashes": run_hashes,
        "RunCustody": "el auditor compara el SHA-256 de cada archivo del run con RunFileHashes y con la copia del kit; el directorio del run solo admite "
                      "esos cuatro archivos y, después del lanzamiento, el subdirectorio launch/, que solo admite los archivos que escribe launch.py",
        "IdentityVerification": "el revisor no ejecuta git: el invocador verifica el clon antes del lanzamiento (verify_clone.py → clone-verification.json) "
                                "y el auditor v5 lo vuelve a verificar después (HEAD, rama única, sin remotos, árbol limpio con ignorados, blobs designados); "
                                "el revisor contrasta la línea `index` de delta.diff con los blobs declarados",
        "CanonicalInputs": can, "AllowedTransitiveInputs": [],
        "TransitivePolicy": "sin transitivos: con --safe-mode el runtime no inyecta CLAUDE.md, memoria ni ganchos (caracterización C1), así que las "
                            "lecturas iniciales de CLAUDE.md y AGENTS.md (opción A de GAP-07 de A-1) no se activan; el prompt es la única instrucción",
        "ReadPolicy": {
            "Closure": "el cierre es por archivo; `LineRanges` señala las secciones que pide el paquete §2 (Scope SECTIONS) o el archivo entero "
                       "(ENTIRE). Los rangos orientan, no limitan: un archivo del cierre puede leerse entero o por otros rangos",
            "Read": "ruta absoluta de un archivo del cierre bajo el clon, o de un archivo del run; en archivos de más de 60 000 bytes, offset y limit de "
                    "300 líneas como máximo",
            "Grep": "solo con `path` = un ARCHIVO del cierre o del run; sin `glob` ni `type`; nunca un directorio ni sin `path`",
            "Glob": "su alcance (path + patrón) solo puede contener archivos del cierre o del run; un patrón que alcance cualquier otro archivo es "
                    "una lectura fuera del cierre aunque no se lea ese archivo",
            "LargeFileThresholdBytes": 60000, "MaxRangeLines": 300,
            "LargeFiles": [r["Path"] for r in can if r["Bytes"] > 60000],
            "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can if r["LinesOver2000Chars"]},
            "Premises": "toda premisa (PremiseRefs) se cita de líneas leídas enteras con Read: de archivos del cierre (Path relativo al clon) o de "
                        "order.txt, delta.diff y A-2.d47f71b6.md (Path absoluto en " + RUN + " o el nombre suelto); nunca de prompt.md; una lectura "
                        "fallida nunca cuenta; una línea larga truncada por Read no cuenta como entregada",
            "FailedReads": "una llamada que falla o no devuelve contenido es una lectura fallida: no acredita contenido",
            "AbsentCommits": "los commits del Freeze (b64a3b64) y de V14 (4c617e82) son anteriores a un rebase y no están en el clon: V14 se "
                             "identifica por su blob en el commit (34ad80ea)",
        },
        "AllowedTools": ["Read", "Grep", "Glob", "StructuredOutput"],
        "ToolPolicy": "solo Read, Grep y Glob, según ReadPolicy, y StructuredOutput para entregar el resultado; cualquier otra herramienta (también "
                      "un intento rechazado por el runtime) y toda denegación de permiso (permission_denials) son violaciones",
        "NotTriggered": [
            {"Source": "CLAUDE.md y AGENTS.md (lecturas iniciales, paso 4 «dotnet test»)", "Reason": "--safe-mode: el runtime no los inyecta; el revisor no tiene herramientas de ejecución"},
            {"Source": "documentation-governance y el Context Pack de I-62", "Reason": "sin CLAUDE.md inyectado no hay obligación transitiva; el paquete §2 fija el cierre"},
            {"Source": "enlaces de A-2, del paquete y de la evidencia a documentos fuera del cierre (clause_map.py, ADR-0048, tablero de F6, state/I-62.yml, el fixture, owner-decision-packets.md)",
             "Reason": "citas descriptivas; el cierre lo fija la tabla de insumos del paquete §2"},
            {"Source": "a2-guards.py self-test y run", "Reason": "sin herramientas de ejecución; el resultado de run sobre 3bbaabef está en a2-guards-result.json"},
        ],
        "DeclaredRuntimeContext": [
            "prompt de sistema propio de claude-cli 2.1.293 en modo -p con --safe-mode: sin CLAUDE.md, memoria, skills de usuario, ganchos ni MCP (medido en C1)",
            "adjunto session_context: correo del usuario (identidad) y estado de Git del cwd (rama main y asuntos de los commits recientes del clon)",
            "adjunto environment: cwd D:\\r62-arch-a2r, plataforma, shell, directorio scratchpad del runtime y directorios adicionales (--add-dir del run)",
            "adjuntos model, date, credential_org (uuid de la organización), prompt_snapshot, total_tokens_reminder y el esquema de structured_output",
            "sin directorio previo en ~/.claude/projects para D:\\r62-arch-a2r (comprobado por el invocador): no hay memoria ni transcripciones del proyecto",
        ],
        "ForbiddenInputs": [
            "~/.claude/projects/* (transcripciones y memoria de la sesión autora, del Coordinator y de cualquier otra sesión)",
            "otras sesiones, incluidas la revisión anterior R20261008T014941Z-68fe (su resultado literal solo como archivo del cierre) y las de F6 (B1, B2)",
            "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones D:\\r62-* distintos de " + CLONE,
            "el fixture de F6 (D:\\r62-fixture\\*), incluida la instantánea fx04a-qh2-origin.git",
            "los archivos de " + RUN + " distintos de los cuatro del run, incluido el subdirectorio launch/",
            "cualquier archivo del clon fuera de CanonicalInputs",
            "Grep o Glob cuyo alcance sea un directorio con archivos fuera del cierre; listados de directorios",
            "red (fetch, web)", "invocar otros agentes o subagentes", "escribir, hacer commit o push",
        ],
        "CorpusDistinctNonAscii": len(corpus),
    }
    with open(os.path.join(KIT, "closure.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(closure, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps({"ids": IDS, "canonical": len(can), "run": {n: (run[n]["Bytes"], run[n]["Sha256"][:12]) for n in RUN_FILES},
                      "questions": qids, "prior": prior, "focus": focus, "large": closure["ReadPolicy"]["LargeFiles"],
                      "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    main("--ranges" in sys.argv[1:])
