"""CA-T4b item 9: the GREEN of PB-T4b (green_pbt4b_v45.py, in memory) against the base for the three files of src/RackCad.UI: the added
and removed lines, the namespaces and types they name, and the XAML elements and resources they add (read-only).

Usage: python -I ir4_uidiff.py <repo> <rev> <out.txt>
"""
import difflib
import importlib.util
import re
import sys

repo, rev, out_p = sys.argv[1:4]
here = __file__.rsplit("ir4_uidiff.py", 1)[0]
spec = importlib.util.spec_from_file_location("green_pbt4b_v45", here + "green_pbt4b_v45.py")
gb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gb)
base, green = gb.green(repo, rev)
L = []
for path in sorted(green):
    if not path.startswith("src/RackCad.UI/"):
        continue
    b = (base.get(path) or "").replace("\r\n", "\n").split("\n")
    g = green[path].replace("\r\n", "\n").split("\n")
    d = [l for l in difflib.unified_diff(b, g, lineterm="", n=0) if l[:1] in "+-" and l[:3] not in ("+++", "---")]
    added = [l[1:] for l in d if l.startswith("+")]
    L.append("== %s: %d lines added, %d removed" % (path, len(added), sum(1 for l in d if l.startswith("-"))))
    usings = sorted(set(re.findall(r"^\s*using\s+([\w.]+)\s*;", "\n".join(added), re.M)))
    L.append("  usings added: %s" % usings)
    names = sorted(set(re.findall(r"\b(PushBack\w+|RackPilot\w+|Selective\w+|Dynamic\w+|FlowBed\w+|Cantilever\w+|Window\b|ShowModalWindow|ShowDialog|MessageBox|Dispatcher\w*|Timer\w*|Task\b|async\b|event\b)", "\n".join(added))))
    L.append("  names added (system types, windows, async, events): %s" % names)
    if path.endswith(".xaml"):
        L.append("  XAML elements added: %s" % sorted(set(re.findall(r"<(\w[\w:.]*)", "\n".join(added)))))
        L.append("  resources: Static %s; Dynamic %s; literal colours %s" % (
            sorted(set(re.findall(r"StaticResource\s+([\w.]+)", "\n".join(added)))),
            sorted(set(re.findall(r"DynamicResource\s+([\w.]+)", "\n".join(added)))),
            sorted(set(re.findall(r'"(#[0-9A-Fa-f]{3,8})"', "\n".join(added))))))
    for l in d[:400]:
        L.append("    " + l[:220])
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(l for l in L if not l.startswith("    ")))
