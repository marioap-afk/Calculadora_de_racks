"""I-62 A-6 ronda 4, preflight: controles de extremo a extremo de a6r4-guards.py (por la línea de órdenes, no en memoria).

Adaptado de docs/automation/evidence/I-62-A6/r3/neg_controls_a6r3.py (ronda 3, sin cambios) con adapt_neg_r4.py. Complementa los mutantes de la autoprueba:
  - positivos: `preview` sobre el texto conforme (sin commit y con un commit candidato hipotético) y `run` sobre ese commit, que deben pasar;
  - negativos de archivo: una copia alterada del texto por caso, `preview`, y deben fallar las guardas esperadas;
  - negativos de commit: un commit candidato que además toca una ruta prohibida, `preview` con el commit, y deben fallar G2 y G3;
  - el borrador tal como está, en un commit candidato y en modo `run`: debe fallar (hallazgos reales y marcadores pendientes);
  - un marcador pendiente en `preview --final`: debe fallar G8.
Solo lectura del worktree de I-62 (su HEAD, refs, estado con ignorados e índice se comparan antes y después); temporales y clones solo bajo
D:/r62-a6r4/guards/neg, que se borra al terminar. Sin red, sin modelos.
Uso: python -I -B neg_controls_a6r4.py --a6 <texto conforme> --selftest <autoprueba del texto conforme>
                                       --draft <borrador> --draft-selftest <autoprueba del borrador> --out <json>
"""
import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
GUARDS = os.path.join(HERE, "a6r4-guards.py")
MAKE = os.path.join(HERE, "make_candidate_commit_a6r4.py")
TMP = "D:/r62-a6r4/guards/neg"
PY = [sys.executable, "-I", "-B"]
OWN_A_TAIL = "puede usar en su verificación solo el instrumento"


def sub1(text, old, new):
    assert text.count(old) >= 1, "caso no aplicable: %s" % old[:50]
    return text.replace(old, new, 1)


def resub1(text, pat, new):
    t, n = re.subn(pat, new, text, count=1, flags=re.M)
    assert n == 1, "caso no aplicable: %s" % pat[:50]
    return t


OLD_B = ('>    (b) `Repo` = el `-C`; `Tool` = el blob fijado, ejecutado desde el objeto del `-C`; `Interpreter` = el intérprete declarado; `Closure` = el\n>    cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256\n>    citado;\n')   # apartado (b) de la regla 6 de A6-3 en la ronda 3


def resub1s(text, pat, new):
    t, n = re.subn(pat, lambda m: new, text, count=1, flags=re.M | re.S)
    assert n == 1, "caso no aplicable: %s" % pat[:50]
    return t


def ins_ev(text, line):
    return sub1(text, "## 10. Evidencia\n", "## 10. Evidencia\n\n" + line + "\n")


FILE_CASES = [
    ("N-01", "línea de un bloque literal nuevo de §2 alterada (AP L653)", ["G1"],
     lambda t: sub1(t, "> | Salida conforme pero vacía o falsa | sesión (`INVALID_OUTPUT`)", "> | Salida conforme pero vacía o falsa | sesión (`INVALID_RESULT`)")),
    ("N-02", "revisión de la ronda 1 retirada de las superficies prohibidas", ["G3"],
     lambda t: sub1(t, "`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1), ", "")),
    ("N-03", "§3.16 sin la fila de R3", ["G4"], lambda t: resub1(t, r"^\| R3: [^\n]*\n", "")),
    ("N-04", "M-01 con la columna Ronda 1 en AGREE", ["G4"], lambda t: sub1(t, "| M-01 | no | no | UNKNOWN (DISAGREE) |", "| M-01 | no | no | AGREE |")),
    ("N-05", "A6-2 cambia respecto de la ronda 3", ["G5"], lambda t: sub1(t, "no\n>    aporta obligaciones de lectura;", "no\n>    aporta obligaciones nuevas de lectura;")),
    ("N-06", "A6-3 vuelve a situar el aceptante en un clon del fixture (R4)", ["G5"],
     lambda t: sub1(t, "el aceptante se ejecuta sobre el mismo repositorio y la misma ruta del `-C` en que corrió el instrumento",
                    "el aceptante se ejecuta sobre un clon del fixture en los mismos SHA")),
    ("N-07", "línea (a) del Owner ampliada a la planificación", ["G6"],
     lambda t: sub1(t, OWN_A_TAIL, "puede usar en su planificación y en su verificación solo el instrumento")),
    ("N-08", "blob del instrumento cambiado en §3.3", ["G7"],
     lambda t: sub1(t, "| `9a829b0eeb1210238052b1feac06cfa8f7964aed` |", "| `9a829b0eeb1210238052b1feac06cfa8f7964aea` |")),
    ("N-09", "«A-6 = AGREED» añadido", ["G8"], lambda t: sub1(t, "## 11. Estado de revisión\n", "## 11. Estado de revisión\n\nA-6 = AGREED.\n")),
    ("N-10", "se pide una línea del Owner", ["G8"], lambda t: ins_ev(t, "Se pide al Owner la línea A6-CONTROLLER-MEDICION-CONSUMO.")),
    ("N-11", "A6-1, regla 2, admite PLAN (R5)", ["G9"],
     lambda t: sub1(t, "la planificación (PLAN) no usa el instrumento", "la planificación (PLAN) usa el instrumento")),
    ("N-12", "tabla de A6-5 sin la condición de R4 sobre `Verification`", ["G10"],
     lambda t: sub1(t, "R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera; y R5, R6 y R7 están",
                    "y R4, R5, R6 y R7 están")),
    ("N-13", "R5 de A6-5 sin la orden hashes", ["G10"],
     lambda t: sub1(t, "(R5: una orden `identity`, `diff`, `scope` o `hashes` en `Runs`;", "(R5: una orden `identity`, `diff` o `scope` en `Runs`;")),
    ("N-14", "referencia a una línea fuera de V14", ["G11"], lambda t: ins_ev(t, "Ver V14 L99999.")),
    ("N-15", "cabecera con la ronda 4 lanzada", ["G1", "G8"], lambda t: sub1(t, "Ronda 4: NOT LAUNCHED.", "Ronda 4: LAUNCHED.")),
    ("N-16", "§3.17 sin la fila R2 (a)", ["G4"], lambda t: resub1(t, r"^\| R2 \(a\): [^\n]*\n", "")),
    ("N-17", "A6-3, regla 6 (c): condición de R4 con Authority en lugar de FreeText", ["G10"],
     lambda t: sub1(t, "`Trailer` y `FreeText` está en `not_run`", "`Trailer` y `Authority` está en `not_run`")),
    ("N-18", "A6-5, regla 7: el instrumento obligatorio en toda invocación VERIFY", ["G5", "G10"],
     lambda t: sub1(t, "Ninguna está obligada a ejecutar el instrumento", "Toda invocación ejecuta el instrumento")),
    ("N-19", "A6-1 cambia respecto de la ronda 3", ["G5"], lambda t: sub1(t, "no es un lanzamiento.", "no es un lanzamiento nuevo.")),
    ("N-20", "línea «Cubre» de A6-5 sin O5 de la ronda 2", ["G4", "G5"], lambda t: sub1(t, "R1, R2 y O5 de la ronda 2.", "R1 y R2 de la ronda 2.")),
    ("N-29", "A6-9, regla 4, alterada (admite otra fuente del cierre)", ["G5"],
     lambda t: sub1(t, "con la tabla de A-6 y con operaciones de grafo de Git fijadas,", "con la tabla de A-6, o con la regla 6 (b) de A6-3, y con operaciones de grafo de Git fijadas,")),
    ("N-30", "tabla de derivación de A6-9 alterada (§3.12)", ["G5"],
     lambda t: sub1(t, "U {MergeBase(D.MainSha, K.AuthorityRevision)}", "U {MergeBase(D.MainSha, H.CurrentSha)}")),
    ("N-31", "A6-5 alterado en un byte (espacio doble, mismo texto normalizado)", ["G5"],
     lambda t: sub1(t, "R1, R2, R3, R9 y R10 están en `MATCH`", "R1, R2, R3, R9 y R10  están en `MATCH`")),
    ("N-32", "A6-7 alterado", ["G5"], lambda t: sub1(t, "Sin esa disposición no hay medición", "Sin esa disposición no hay medición alguna")),
    ("N-33", "A6-3, regla 6 (b), sin el apartado (iv)", ["G5"],
     lambda t: resub1(t, r"^>    \(iv\) un término omitido .*\n(?:>    (?!\(|La prueba previa ).*\n)*", "")),
    ("N-34", "A6-3, regla 6 (b), de la ronda 3 reintroducida", ["G5"], lambda t: resub1s(t, r"^>    \(b\) `Repo` = .*?(?=^>    \(c\) `Verification` = )", OLD_B)),
    ("N-35", "A6-3, regla 6 (b): un término omitido se sustituye", ["G5"],
     lambda t: sub1(t, "no se sustituye por ningún otro valor", "se sustituye por otro valor")),
    ("N-36", "§3.18 sin la fila R1", ["G4"], lambda t: resub1(t, r"(^### 3\.18 (?:.*\n)*?)\| R1: .*\n", r"\1")),
    ("N-37", "línea «Cubre» de A6-3 sin el segmento de la ronda 3", ["G4", "G5"],
     lambda t: resub1(t, r"(; R1 y R2 de la ronda 2); [^;\n]* de la ronda 3\.$", r"\1.")),
    ("N-38", "revisión de la ronda 3 retirada de las superficies prohibidas", ["G3"],
     lambda t: sub1(t, "`docs/automation/evidence/I-62-architect-A-6r3/` (la revisión de la ronda 3 y su kit) y ", "y ")),
    ("N-39", "cabecera: la ronda 3 vuelve a NOT LAUNCHED", ["G1"],
     lambda t: sub1(t, "Ronda 3:\nCHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80).", "Ronda 3:\nNOT LAUNCHED.")),
    ("N-41", "§3.17: celda «Prueba» de R1 (b) reescrita (no por añadido)", ["G4"],
     lambda t: sub1(t, "con una ejecución con error, rechazo (con el cierre", "con una ejecución con error, aceptación (con el cierre")),
    ("N-42", "sin la fuente de la ronda 4 docs/automation/agent-execution/README.md:219-220", ["G11"],
     lambda t: sub1(t, "(docs/automation/agent-execution/README.md:219-220)", "(docs/automation/agent-execution/README.md)")),
    ("N-43", "estado PROPUESTA (ronda 3)", ["G1", "G8"],
     lambda t: sub1(t, "State:\nPROPUESTA (ronda 4). No aplicada", "State:\nPROPUESTA (ronda 3). No aplicada")),
]
COMMIT_CASES = [
    ("N-21", "commit candidato que además modifica AUTOMATION_PLAN", ["G2", "G3"], "docs/AUTOMATION_PLAN.md"),
    ("N-22", "commit candidato que además modifica un archivo de la ronda 1 de I-62-A6/", ["G2", "G3"], "docs/automation/evidence/I-62-A6/preflight.md"),
    ("N-23", "commit candidato que además modifica decisiones", ["G2"], "docs/automation/decisions/I-62.md"),
    ("N-24", "commit candidato que además modifica la revisión de la ronda 1", ["G2", "G3"],
     "docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json"),
    ("N-25", "commit candidato que además modifica el instrumento fijado", ["G2", "G3"], "docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py"),
    ("N-26", "commit candidato que además añade un archivo a I-62-A6/ fuera de r3/", ["G2", "G3"], "docs/automation/evidence/I-62-A6/nuevo.md"),
    ("N-27", "commit candidato que además modifica un archivo de r2/ (evidencia de la ronda 2)", ["G2", "G3"], "docs/automation/evidence/I-62-A6/r2/preflight.md"),
    ("N-28", "commit candidato que además modifica la revisión de la ronda 2", ["G2", "G3"],
     "docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json"),
    ("N-44", "commit candidato que además añade un archivo a r3/ (fuera de r4/)", ["G2", "G3"], "docs/automation/evidence/I-62-A6/r3/nuevo.md"),
    ("N-45", "commit candidato que además modifica un archivo de r3/ (evidencia de la ronda 3)", ["G2", "G3"], "docs/automation/evidence/I-62-A6/r3/preflight.md"),
    ("N-46", "commit candidato que además modifica la revisión de la ronda 3", ["G2", "G3"],
     "docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json"),
    ("N-47", "commit candidato que además modifica el kit de la revisión de la ronda 3", ["G2", "G3"],
     "docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/kit/launch.py"),
    ("N-48", "commit candidato que además modifica el aceptante fijado", ["G2", "G3"], "docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py"),
]


def rmtree_ro(path):
    def _writable(func, p, exc):
        os.chmod(p, 0o600)
        func(p)
    if os.path.exists(path):
        shutil.rmtree(path, onexc=_writable)


def wt_digest():
    def h(*args):
        r = subprocess.run(["git", "-C", WT] + list(args), capture_output=True)
        return hashlib.sha256(r.stdout).hexdigest()
    head = subprocess.run(["git", "-C", WT, "rev-parse", "HEAD"], capture_output=True, encoding="utf-8").stdout.strip()
    idx = subprocess.run(["git", "-C", WT, "rev-parse", "--path-format=absolute", "--git-path", "index"], capture_output=True, encoding="utf-8").stdout.strip()
    return {"Head": head, "ForEachRef": h("for-each-ref"), "StatusPorcelainIgnored": h("status", "--porcelain", "--ignored"),
            "IndexSha256": hashlib.sha256(open(idx, "rb").read()).hexdigest()}


def guards(mode, out, *args):
    cmd = PY + [GUARDS, mode] + list(args) + ["--out", out, "--tmp", TMP + "/gt"]
    r = subprocess.run(cmd, capture_output=True)
    res = json.load(open(out, encoding="utf-8"))
    return r.returncode, res


def fails(res):
    return sorted(g for g, r in res["Guards"].items() if r["Result"] == "FAIL")


def first(res, gs):
    return {g: (res["Guards"][g]["Findings"] or [""])[0][:160] for g in gs if g in res["Guards"]}


R2_PLACEHOLDERS = ["docs/automation/evidence/I-62-A6/r4/r1probes/README.md", "docs/automation/evidence/I-62-A6/r4/r1probes/table_r4.py"]


def candidate(cid, a6, selftest, extra=None):
    """Commit candidato hipotético con A-6, las guardas, la autoprueba y un marcador por cada evidencia r4/ que A-6 nombra
    (r1probes/ y r1probes/table_r4.py; la evidencia real la aporta el paquete r4/, no este preflight)."""
    dest = "%s/rc-%s" % (TMP, cid)
    outj = "%s/%s-cand.json" % (TMP, cid)
    ph = "%s/r2-placeholder.md" % TMP
    if not os.path.exists(ph):
        with open(ph, "w", encoding="utf-8", newline="\n") as fh:
            fh.write("# Marcador del preflight de las guardas de la ronda 4 (no es evidencia)\n")
    cmd = PY + [MAKE, "--dest", dest, "--out", outj, "--a6", a6, "--selftest", selftest]
    for e in ["%s=%s" % (p, ph) for p in R2_PLACEHOLDERS] + list(extra or []):
        cmd += ["--extra", e]
    subprocess.run(cmd, capture_output=True)
    c = json.load(open(outj, encoding="utf-8"))
    return dest, c


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--a6", required=True)
    ap.add_argument("--selftest", required=True)
    ap.add_argument("--draft", required=True)
    ap.add_argument("--draft-selftest", required=True)
    ap.add_argument("--out", required=True)
    a = ap.parse_args()
    os.environ["GIT_OPTIONAL_LOCKS"] = "0"
    rmtree_ro(TMP)
    os.makedirs(TMP + "/gt")
    before = wt_digest()
    text = open(a.a6, "rb").read().replace(b"\r\n", b"\n").decode("utf-8")
    rows = []
    common = ["--repo", WT, "--pkgdir", HERE]
    # positivos
    rc, res = guards("preview", TMP + "/p0.json", *common, "--a6", a.a6, "--selftest", a.selftest)
    rows.append({"Id": "P-0", "Case": "texto conforme, sin commit candidato (G2 NOT_RUN)", "Expected": "PASS", "Result": res["Result"],
                 "FailingGuards": fails(res), "AsExpected": res["Result"] == "PASS" and rc == 0})
    dest, c = candidate("P1", a.a6, a.selftest)
    rc, res = guards("preview", TMP + "/p1.json", *common, "--a6", a.a6, "--selftest", a.selftest, "--cand-repo", dest, "--cand-head", c["Candidate"])
    g2 = res["Guards"]["G2"]["Result"]
    rows.append({"Id": "P-1", "Case": "texto conforme en un commit candidato hipotético (A-6 modificada y r4/ añadido)", "Expected": "PASS",
                 "Result": res["Result"], "G2": g2, "FailingGuards": fails(res), "Candidate": c["Candidate"], "Status": c["Status"],
                 "AsExpected": res["Result"] == "PASS" and rc == 0 and g2 == "PASS"})
    rc, res = guards("run", TMP + "/p2.json", "--repo", dest, "--base", c["Base"], "--head", c["Candidate"])
    rows.append({"Id": "P-2", "Case": "modo run sobre ese commit candidato (A-6, guardas y autoprueba leídas de head)", "Expected": "PASS",
                 "Result": res["Result"], "FailingGuards": fails(res), "AsExpected": res["Result"] == "PASS" and rc == 0})
    rmtree_ro(dest)
    # el borrador tal como está, en run
    dest, c = candidate("D1", a.draft, a.draft_selftest)
    rc, res = guards("run", TMP + "/d1.json", "--repo", dest, "--base", c["Base"], "--head", c["Candidate"])
    fl = fails(res)
    rows.append({"Id": "D-1", "Case": "el borrador con marcadores sin rellenar, en un commit candidato y en modo run", "Expected": ["G8"],
                 "Result": res["Result"], "FailingGuards": fl, "FirstFinding": first(res, fl),
                 "AsExpected": rc == 1 and res["Result"] == "FAIL" and {"G8"} <= set(fl)})
    rmtree_ro(dest)
    # negativos de archivo
    for cid, name, expected, fn in FILE_CASES:
        path = "%s/%s.md" % (TMP, cid)
        with open(path, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(fn(text))
        rc, res = guards("preview", "%s/%s.json" % (TMP, cid), *common, "--a6", path, "--selftest", a.selftest)
        fl = fails(res)
        rows.append({"Id": cid, "Case": name, "Expected": expected, "Result": res["Result"], "FailingGuards": fl, "FirstFinding": first(res, expected),
                     "AsExpected": rc == 1 and res["Result"] == "FAIL" and set(expected) <= set(fl)})
    # marcador pendiente en preview --final
    path = "%s/N-40.md" % TMP
    with open(path, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(ins_ev(text, "Casos: @@R4.CASES@@."))
    rc, res = guards("preview", "%s/N-40.json" % TMP, *common, "--a6", path, "--selftest", a.selftest, "--final")
    fl = fails(res)
    rows.append({"Id": "N-40", "Case": "marcador pendiente en preview --final", "Expected": ["G8"], "Result": res["Result"], "FailingGuards": fl,
                 "FirstFinding": first(res, ["G8"]), "AsExpected": rc == 1 and res["Result"] == "FAIL" and "G8" in fl})
    # negativos de commit
    for cid, name, expected, path in COMMIT_CASES:
        extra = "%s/%s.extra" % (TMP, cid)
        cur = subprocess.run(["git", "-C", WT, "show", "HEAD:" + path], capture_output=True).stdout.replace(b"\r\n", b"\n")
        with open(extra, "wb") as fh:
            fh.write(cur + b"\nlinea negativa del preflight de la ronda 4 de A-6\n")
        dest, c = candidate(cid, a.a6, a.selftest, ["%s=%s" % (path, extra)])
        rc, res = guards("preview", "%s/%s.json" % (TMP, cid), *common, "--a6", a.a6, "--selftest", a.selftest,
                         "--cand-repo", dest, "--cand-head", c["Candidate"])
        fl = fails(res)
        rows.append({"Id": cid, "Case": name, "Expected": expected, "Result": res["Result"], "FailingGuards": fl, "FirstFinding": first(res, expected),
                     "AsExpected": rc == 1 and res["Result"] == "FAIL" and set(expected) <= set(fl)})
        rmtree_ro(dest)
    rmtree_ro(TMP)
    after = wt_digest()
    summary = {"Cases": len(rows), "AsExpected": sum(r["AsExpected"] for r in rows),
               "WorktreeUnchanged": before == after, "TmpRemoved": not os.path.exists(TMP),
               "Note": "en los casos de archivo G5b también falla: la autoprueba custodiada fija el blob del texto sin alterar"}
    out = {"Tool": "neg_controls_a6r4.py", "Summary": summary, "WorktreeDigest": {"Before": before, "After": after}, "Rows": rows}
    with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps(summary, ensure_ascii=False))
    sys.exit(0 if summary["AsExpected"] == summary["Cases"] and summary["WorktreeUnchanged"] else 1)


if __name__ == "__main__":
    main()
