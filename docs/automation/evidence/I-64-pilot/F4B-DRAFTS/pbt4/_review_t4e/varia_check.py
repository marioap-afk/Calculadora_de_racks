import sys, os, json, glob
sys.stdout.reconfigure(encoding="utf-8")
C = sys.argv[1]  # pbt4ecc/compile
TARGET = "RackViewBatchDialogPresenter.cs"

def hits(path):
    data = json.load(open(path, encoding="utf-8"))
    out = {}
    for module in data.values():
        for fp, classes in module.items():
            if not fp.replace("\\", "/").endswith("/" + TARGET):
                continue
            for cls, methods in classes.items():
                for m, body in methods.items():
                    for n, h in body.get("Lines", {}).items():
                        if int(n) in (37, 38):
                            out[int(n)] = out.get(int(n), 0) + h
    return out

for d in ("r73-bc", "r79-bc", "r72-p6", "r83-p6"):
    files = sorted(glob.glob(os.path.join(C, d, "*.coverage.json")))
    print("==", d, len(files), "json")
    for f in files:
        name = os.path.basename(f)
        h = hits(f)
        if h or "cov" in name and ("ui-cov" in name or "core" in name):
            print("  %-45s %s" % (name, h))
