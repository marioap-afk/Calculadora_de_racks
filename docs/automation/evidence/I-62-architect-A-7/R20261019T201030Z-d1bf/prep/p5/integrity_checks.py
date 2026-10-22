"""Decisiones §95, punto 3: comprobaciones de integridad y fidelidad ANTES de usar ninguna copia fiel (sesión principal). Solo lectura: el clon (git),
el worktree de I-62 (git, GIT_OPTIONAL_LOCKS=0), el kit y el run; los negativos trabajan en memoria o en un temporal propio que se borra.

  A. Custodia en HEAD: cada blob de la carpeta custodiada del kit, en la HEAD del worktree, tiene el SHA-256 que declara su MANIFEST.sha256 (la
     corrección de CRLF de b23d71b8). Negativos: el mismo verificador sobre 5d5730ba (el commit con 31 blobs normalizados a LF) y sobre la HEAD con un
     SHA-256 del manifiesto alterado tienen que dar discrepancias.
  B. Las 43 copias fieles, reproducidas desde su fuente fijada (git cat-file blob <commit del clon>:<ruta> en el clon y fidelity.encode del kit): blob del
     original = el del cierre; SHA-256 de la copia = cierre = kit-manifest.json; regenerada = copia del kit = copia del RUN (la que leerá el revisor);
     fidelity.verify sin problemas.
  C. Rechazo mecánico (fidelity.verify del kit y run_files_check de launch.py, leído con ast como en el caso X05 de la autoprueba):
     C1 BOM eliminado (las seis copias con BOM, regeneradas sin él, y el escape \\xEF\\xBB\\xBF borrado de la copia); C2 normalización no permitida
     (LF -> CRLF y NFKC/NFD de la copia; el original normalizado a CRLF); C3 copia alterada (un byte en varias posiciones); C4 archivo ajeno al cierre
     (un run con una copia de un archivo que no está en FaithfulCopies, y un run con un archivo extra); C5 discrepancia blob/manifiesto (= A, negativos).
Uso: python -I -B integrity_checks.py <kit> <clon> <run> <worktree> <salida.json>
"""
import ast
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile
import unicodedata

KIT, CLONE, RUN, WT, OUT = sys.argv[1:6]
CUST = "docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf"
ENV = dict(os.environ, GIT_OPTIONAL_LOCKS="0")
sha = lambda b: hashlib.sha256(b).hexdigest()


def load(name, path):
    import importlib.util
    spec = importlib.util.spec_from_file_location(name, path)
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


def git(repo, *a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True, env=ENV, check=True).stdout


FID = load("fidelity", os.path.join(KIT, "fidelity.py"))
closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
man = json.load(open(os.path.join(KIT, "kit-manifest.json"), encoding="utf-8"))
rev = closure["AuthorityRevision"]
out = {"Schema": "a7r2-s95-integrity/1", "CloneCommit": rev, "WorktreeHead": git(WT, "rev-parse", "HEAD").decode().strip()}


# ---------------- A. custody blobs vs MANIFEST
def custody_mismatches(commit, alter=None):
    m = git(WT, "cat-file", "blob", "%s:%s/MANIFEST.sha256" % (commit, CUST)).decode("ascii")
    rows = [l.split("  ", 1) for l in m.splitlines() if l.strip()]
    if alter:
        rows = [(("0" * 64) if r == alter else h, r) for h, r in rows]
    bad = [r for h, r in rows if sha(git(WT, "cat-file", "blob", "%s:%s/%s" % (commit, CUST, r))) != h]
    return len(rows), bad


n_head, bad_head = custody_mismatches("HEAD")
n_old, bad_old = custody_mismatches("5d5730ba")
n_alt, bad_alt = custody_mismatches("HEAD", alter="kit/owner_gate.py")
out["A_Custody"] = {"HeadEntries": n_head, "HeadMismatches": bad_head, "Neg5d5730baMismatches": len(bad_old),
                    "NegAlteredManifestMismatches": bad_alt,
                    "Pass": n_head == 217 and bad_head == [] and len(bad_old) == 31 and bad_alt == ["kit/owner_gate.py"]}

# ---------------- B. the 43 copies from their pinned source
rows, originals = [], {}
for rec in closure["FaithfulCopies"]:
    path = rec["Original"]
    data = git(CLONE, "cat-file", "blob", "%s:%s" % (rev, path))
    originals[path] = data
    blob = hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()
    regen = FID.encode(path, data)
    name = FID.copy_name(path)
    kitb, runb = open(os.path.join(KIT, name), "rb").read(), open(os.path.join(RUN, name), "rb").read()
    rows.append({"Copy": name, "OriginalBlobMatches": blob == rec["OriginalBlob"], "OriginalSha256Matches": sha(data) == rec["OriginalSha256"],
                 "CopySha256": sha(regen), "ClosureMatches": sha(regen) == rec["CopySha256"],
                 "ManifestMatches": man["Files"][name]["Sha256"] == sha(regen), "EqualsKit": regen == kitb, "EqualsRun": regen == runb,
                 "VerifyProblems": FID.verify(path, data, regen)})
okB = len(rows) == 43 and all(r["OriginalBlobMatches"] and r["OriginalSha256Matches"] and r["ClosureMatches"] and r["ManifestMatches"] and
                              r["EqualsKit"] and r["EqualsRun"] and not r["VerifyProblems"] for r in rows)
out["B_FaithfulCopies"] = {"Copies": len(rows), "Pass": okB, "Rows": rows}

# ---------------- C. mechanical rejection
neg = []


def expect_reject(case, path, data, copy_bytes):
    p = FID.verify(path, data, copy_bytes)
    neg.append({"Case": case, "Original": path, "Rejected": bool(p), "Problems": p[:3]})


bom = [r["Original"] for r in closure["FaithfulCopies"] if "BOM" in (r.get("Flags") or {})]
for path in bom:
    data = originals[path]
    assert data.startswith(b"\xef\xbb\xbf")
    expect_reject("C1-copia-regenerada-sin-BOM", path, data, FID.encode(path, data[3:]))
    cb = FID.encode(path, data)
    assert b"\\xEF\\xBB\\xBF" in cb
    expect_reject("C1-escape-del-BOM-borrado-de-la-copia", path, data, cb.replace(b"\\xEF\\xBB\\xBF", b"", 1))
sample = [r["Original"] for r in closure["FaithfulCopies"]][:: 7]
for path in sample:
    data = originals[path]
    cb = FID.encode(path, data)
    expect_reject("C2-copia-LF-a-CRLF", path, data, cb.replace(b"\n", b"\r\n"))
    for form in ("NFKC", "NFD"):
        nb = unicodedata.normalize(form, cb.decode("utf-8")).encode("utf-8")
        if nb != cb:
            expect_reject("C2-copia-normalizada-" + form, path, data, nb)
    crlf = data.replace(b"\n", b"\r\n")
    expect_reject("C2-original-normalizado-a-CRLF", path, data, FID.encode(path, crlf))
    for pos in (0, len(cb) // 2, len(cb) - 2):
        b = bytearray(cb)
        b[pos] ^= 0x01
        expect_reject("C3-byte-%d-alterado" % pos, path, data, bytes(b))
# C4: run_files_check of launch.py (ast), as the X05 case of the self-test
tree = ast.parse(open(os.path.join(KIT, "launch.py"), encoding="utf-8").read())
node = next(n for n in tree.body if isinstance(n, ast.FunctionDef) and n.name == "run_files_check")
run_files = next(ast.literal_eval(n.value) for n in tree.body if isinstance(n, ast.Assign) and getattr(n.targets[0], "id", None) == "RUN_FILES")
ns = {"os": os, "FID": FID, "RUN_FILES": run_files, "sha_file": lambda p: sha(open(p, "rb").read())}
exec(compile(ast.Module(body=[node], type_ignores=[]), "launch.py:run_files_check", "exec"), ns)
rfc = ns["run_files_check"]
mrun = {"RunFiles": {n: {"Sha256": h["Sha256"]} for n, h in closure["RunFileHashes"].items()}}
real = rfc(closure, mrun, RUN, KIT)
tmp = tempfile.mkdtemp(prefix="s95neg-", dir=os.path.dirname(os.path.abspath(OUT)))
try:
    extra_copy, extra_file = os.path.join(tmp, "a"), os.path.join(tmp, "b")
    for t in (extra_copy, extra_file):
        shutil.copytree(RUN, t, ignore=shutil.ignore_patterns("launch"))
    foreign = "docs/HANDOFF.md"
    assert foreign not in {r["Original"] for r in closure["FaithfulCopies"]}
    fdata = git(CLONE, "cat-file", "blob", "%s:%s" % (rev, foreign))
    open(os.path.join(extra_copy, FID.copy_name(foreign)), "wb").write(FID.encode(foreign, fdata))
    open(os.path.join(extra_file, "notas.txt"), "wb").write(b"x\n")
    r_copy, r_file = rfc(closure, mrun, extra_copy, KIT), rfc(closure, mrun, extra_file, KIT)
finally:
    shutil.rmtree(tmp)
neg.append({"Case": "C4-run-real-conforme", "Rejected": real["Ok"] is not True, "Expected": "accepted"})
neg.append({"Case": "C4-copia-de-un-archivo-ajeno-al-cierre", "Rejected": r_copy["Ok"] is False, "Mismatches": r_copy.get("Mismatches")})
neg.append({"Case": "C4-archivo-extra-en-el-run", "Rejected": r_file["Ok"] is False, "Mismatches": r_file.get("Mismatches")})
okC = all(c["Rejected"] for c in neg if c["Case"] != "C4-run-real-conforme") and real["Ok"] is True
sf = json.load(open(os.path.join(KIT, "selftest-fidelity-result.json"), encoding="utf-8"))
kitneg = {c["Case"]: c["Result"] for c in sf["Cases"] if c["Case"].startswith(("FV02", "FV03", "FV04", "FV05", "FR"))}
out["C_Rejection"] = {"Cases": len(neg), "Pass": okC and out["A_Custody"]["Pass"], "Rows": neg,
                      "KitSelftestFidelity": {"AllPass": sf["AllPass"], "Relevant": kitneg}}
out["AllPass"] = out["A_Custody"]["Pass"] and okB and out["C_Rejection"]["Pass"] and sf["AllPass"]
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
print(json.dumps({"A": out["A_Custody"]["Pass"], "B": okB, "C": out["C_Rejection"]["Pass"], "NegCases": len(neg), "AllPass": out["AllPass"]}))
sys.exit(0 if out["AllPass"] else 1)
