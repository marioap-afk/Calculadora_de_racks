"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. I-62 night order §10: portability of tonight's results.

A second process, in a CLEAN clone (`git clone --no-local -c core.autocrlf=false`, so no unreachable object of the first process is inherited and the
working tree has the blob bytes), reconstructs the results ONLY from custodied artifacts of the branch: no global variables, memory or narrative of
the first process. Each step reruns a custodied script and compares its output with the custodied result (byte-for-byte unless a normalization is
declared). Not an FX pilot: no role session is opened.

Usage: python reconstruct.py <clean clone> <work dir> <out.json>
"""
import hashlib
import importlib.util
import json
import os
import subprocess
import sys

CLONE, WORK, OUT = sys.argv[1:4]
os.makedirs(WORK, exist_ok=True)
NIGHT = "docs/automation/evidence/I-62-prep/night-2026-10-05/"
ENV = dict(os.environ, PYTHONIOENCODING="utf-8", PYTHONDONTWRITEBYTECODE="1")


def git(*a):
    return subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, env=ENV)


def blob(rel):
    r = git("show", "HEAD:" + rel)
    return r.stdout if r.returncode == 0 else None


def run(args, cwd=None):
    r = subprocess.run([sys.executable] + args, cwd=cwd or CLONE, capture_output=True, text=True, encoding="utf-8", errors="replace", env=ENV)
    return r.returncode, (r.stdout[-400:] + r.stderr[-400:])


def sha(b):
    return hashlib.sha256(b).hexdigest()


def compare(name, produced_path, custodied_rel, normalize=None):
    got = open(produced_path, "rb").read()
    exp = blob(custodied_rel)
    if normalize:
        got, exp = normalize(got), normalize(exp)
    return {"Step": name, "Custodied": custodied_rel, "ProducedSha256": sha(got), "CustodiedSha256": sha(exp) if exp else None,
            "Result": "IDENTICAL" if exp is not None and got == exp else "DIFFERENT"}


steps = []
head = git("rev-parse", "HEAD").stdout.decode().strip()

# 1. A-1 harness
h = os.path.join(CLONE, "docs/automation/evidence/I-62-A1/a1-counterexamples.py")
p = os.path.join(WORK, "a1.json")
rc, tail = run([h, p])
steps.append(dict(compare("a1-counterexamples", p, "docs/automation/evidence/I-62-A1/a1-counterexamples-result.json"), ExitCode=rc))

# 2. combined sequences (F4 experimental)
p = os.path.join(WORK, "combo.json")
rc, tail = run([os.path.join(CLONE, NIGHT + "f4-exp/combo_sequences.py"), h, p])
# A62-A1T-01 (Coordinator order after R20261005T073911Z-2dfe): the harness now resolves the Target of a LAUNCHING attempt through the chain, so the
# custodied counterpart is the new execution combo-result-a1t01.json. The portability run of c66137b9 compared against combo-result.json, which is
# kept unchanged and reproducible with this script at that commit.
steps.append(dict(compare("f4-exp/combo_sequences", p, NIGHT + "f4-exp/combo-result-a1t01.json"), ExitCode=rc))

# 2b. T8 of A62-A1T-01: two real rebases and a successor in a clean clone (disposable repository; SHAs differ per run, relations must hold)
p = os.path.join(WORK, "t8.json")
rc, tail = run([os.path.join(CLONE, "docs/automation/evidence/I-62-A1/a1t01/t8-clean-clone.py"), os.path.join(WORK, "t8-repo"), p])
t8 = json.load(open(p, encoding="utf-8")) if os.path.exists(p) else {}
steps.append({"Step": "a1t01/t8-clean-clone (relaciones, no SHAs)", "ExitCode": rc, "Result": "PASS" if rc == 0 and t8.get("AllAsExpected") else "FAIL"})

# 3. P4 harness (synthetic and real I-63 objects, reachable through main)
p = os.path.join(WORK, "p4.json")
rc, tail = run([os.path.join(CLONE, NIGHT + "p4/p4-custody-harness.py"), p, CLONE])
steps.append(dict(compare("p4/p4-custody-harness", p, NIGHT + "p4/p4-custody-result.json"), ExitCode=rc))

# 4. P8 prototype
p = os.path.join(WORK, "p8.json")
rc, tail = run([os.path.join(CLONE, NIGHT + "p8/p8-counters-harness.py"), p])
steps.append(dict(compare("p8/p8-counters-harness", p, NIGHT + "p8/p8-counters-result.json"), ExitCode=rc))

# 5. helpers: the gate readings (P1/P2) are reconstructible from the branch; the TRX readings are not (TRX are not versioned by policy, 16.12)
sys.path.insert(0, os.path.join(CLONE, NIGHT + "helpers"))
spec = importlib.util.spec_from_file_location("i62_helpers", os.path.join(CLONE, NIGHT + "helpers/i62_helpers.py"))
H = importlib.util.module_from_spec(spec)
spec.loader.exec_module(H)
custodied = json.loads(blob(NIGHT + "helpers/real-readings.json"))
EV = "docs/automation/evidence/I-63-pilot/"
files = git("ls-tree", "-r", "--name-only", "HEAD", EV).stdout.decode().split()
diffs = []
for gate, g in custodied["Gates"].items():
    real_h, real_d = json.loads(blob(g["Handoff"])), json.loads(blob(g["Delegation"]))
    p1 = H.p1_identity(real_h, CLONE, real_d)
    p2 = H.p2_scope(CLONE, real_d["BaseSha"], real_h["CurrentSha"], real_d)
    if p1["Status"] != g["P1Real"]["Status"] or p2["Status"] != g["P2Real"]["Status"] or len(p2["Paths"]) != g["P2Real"]["Paths"]:
        diffs.append(gate)
steps.append({"Step": "helpers (P1/P2 de G1-G4 desde artefactos de la rama)", "Result": "IDENTICAL" if not diffs else "DIFFERENT", "Different": diffs,
              "NotReconstructible": "lecturas P6: los TRX no se versionan (16.12); sus SHA-256 están en real-readings.json"})
rc, tail = run(["-m", "unittest", "test_i62_helpers"], cwd=os.path.join(CLONE, NIGHT + "helpers"))
steps.append({"Step": "helpers unittest (sin TRX: P6 omitido por la propia prueba)", "ExitCode": rc, "Tail": tail.strip().split("\n")[-1],
              "Result": "PASS" if rc == 0 else "FAIL"})

# 6. B.11: the F3 generator pins the pre-rebase commit of V14, unreachable in a clean clone
orig, image = None, None
rmap = json.loads(blob(NIGHT + "rebase-map.json"))
gen = os.path.join(CLONE, "docs/automation/evidence/I-62-F3/gen-manifest.py")
src = open(gen, encoding="utf-8").read()
orig = "4c617e82b32b6c810b68d75fc19472efed22b393"
reach = git("cat-file", "-e", orig + "^{commit}").returncode == 0
step = {"Step": "B.11 (gen-manifest + b11_precision)", "PinnedCommitReachable": reach}
if not reach:
    image = next(c["Image"] for c in rmap["Commits"] if c["Original"] == orig)
    same_blob = git("rev-parse", image + ":docs/initiatives/I-62-proposal-v14.md").stdout.decode().strip() == "34ad80ea1bfff144bfc5169f62920a4c904c1bfa"
    step.update(Gap="PORTABILITY_GAP: el generador fija el commit histórico 4c617e82, inalcanzable en un clon limpio",
                Resolution="imagen por el rebase-map custodiado %s, con el mismo blob de V14 (%s)" % (image[:8], same_blob))
    shim = os.path.join(WORK, "gen-manifest-mapped.py")
    open(shim, "w", encoding="utf-8").write(src.replace(orig, image))
    gen = shim
man, rep = os.path.join(WORK, "manifest.json"), os.path.join(WORK, "report.json")
rc, tail = run([gen, CLONE, man, rep])
norm = (lambda b: b.replace(image.encode(), orig.encode())) if image else None
canon_report = lambda b: json.dumps(json.loads(b), sort_keys=True).encode()
step["ManifestReport"] = compare("manifest-report", rep, "docs/automation/evidence/I-62-F3/manifest-report.json",
                                 lambda b: canon_report(norm(b) if norm else b))["Result"]
v14 = os.path.join(WORK, "v14.md")
open(v14, "wb").write(git("show", (image or orig) + ":docs/initiatives/I-62-proposal-v14.md").stdout)
raw_manifest = open(man, "rb").read()          # read first: opening for write truncates (defect of the first run, fixed and declared)
open(man, "wb").write(norm(raw_manifest) if norm else raw_manifest)
p = os.path.join(WORK, "b11.json")
rc2, tail = run([os.path.join(CLONE, NIGHT + "b11-precision/b11_precision.py"), man, v14, p])
step["B11Precision"] = compare("b11-precision", p, NIGHT + "b11-precision/b11-precision-result.json")["Result"]
step["Result"] = "IDENTICAL_AFTER_DECLARED_MAPPING" if image and step["ManifestReport"] == "IDENTICAL" and step["B11Precision"] == "IDENTICAL" else \
                 ("IDENTICAL" if step["ManifestReport"] == "IDENTICAL" and step["B11Precision"] == "IDENTICAL" else "DIFFERENT")
steps.append(step)

doc = {"Label": "EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION", "Script": "portability/reconstruct.py", "CloneHead": head,
       "Clone": "git clone --no-local -c core.autocrlf=false (sin remoto); proceso distinto del primero, sin su memoria",
       "Python": sys.version.split()[0], "Git": subprocess.check_output(["git", "--version"], text=True).strip(), "Steps": steps,
       "AllReconstructed": all(s["Result"] in ("IDENTICAL", "PASS", "IDENTICAL_AFTER_DECLARED_MAPPING") for s in steps)}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(doc, f, ensure_ascii=False, indent=1)
    f.write("\n")
for s in steps:
    print("%-60s %s" % (s["Step"][:60], s["Result"]))
print("all", doc["AllReconstructed"])
