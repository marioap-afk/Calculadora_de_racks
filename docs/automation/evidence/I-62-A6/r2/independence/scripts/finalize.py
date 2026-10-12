# finalize.py - hygiene check and SHA-256 manifest of the R3 deliverables (run: python -I finalize.py).
# Fails (exit 1) if any deliverable contains the Windows user name, the profile path, the short name, an e-mail
# address or a byte >= 0x80 in a results file. Writes results/manifest.json (README.md is listed but cannot list itself).
import hashlib
import importlib.util
import json
import os
import re
import sys

_d = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("r3lib", os.path.join(_d, "r3lib.py"))
L = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(L)

ROOT = os.path.normpath(os.path.join(_d, ".."))
EMAIL = re.compile(rb"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")


def forbidden():
    prof = os.environ.get("USERPROFILE", "")
    name = os.path.basename(prof.rstrip("\\/"))
    toks = [t for t in (prof, prof.replace("\\", "/"), name, L.short_name(name) if name else "") if len(t) > 3]
    return [t.lower().encode("utf-8") for t in toks]


def main():
    bad = []
    rows = []
    toks = forbidden()
    for sub in ("scripts", "results", ""):
        base = os.path.join(ROOT, sub) if sub else ROOT
        for f in sorted(os.listdir(base)):
            p = os.path.join(base, f)
            if not os.path.isfile(p) or f == "manifest.json" or f.endswith(".pyc"):
                continue
            data = L.read_bytes(p)
            low = data.lower()
            for t in toks:
                if t in low:
                    bad.append((sub + "/" + f, "user-identifying token"))
            if EMAIL.search(data):
                bad.append((sub + "/" + f, "e-mail: " + EMAIL.search(data).group(0)[:3].decode("ascii", "replace") + "..."))
            if sub == "results" and any(b >= 0x80 for b in data):
                bad.append((sub + "/" + f, "non-ASCII byte"))
            rows.append({"File": (sub + "/" if sub else "") + f, "Sha256": hashlib.sha256(data).hexdigest(), "Bytes": len(data)})
    man = {"Schema": "r3-manifest/1", "Files": rows, "HygieneProblems": [list(x) for x in bad]}
    L.write_result("manifest.json", man)
    for r in rows:
        print("%s  %s" % (r["Sha256"], r["File"]))
    if bad:
        print("HYGIENE PROBLEMS:", bad)
        return 1
    print("hygiene: OK")
    return 0


if __name__ == "__main__":
    sys.exit(main())
