"""CA-T4b item 3 (D-S6): the RequiredTests table of v4.8 against the copy class names, the namespace guard by text on the probe
copies, and clashes of the copies' top-level types with the types of the test projects at the revision (read-only: git show).

Usage: python -I ir4_ds6.py <repo> <rev> <package-v48.md> <copies-dir> <out.txt>
"""
import os
import re
import subprocess
import sys

repo, rev, pkg_p, copies_dir, out_p = sys.argv[1:6]
L = []


def git(*args):
    return subprocess.run(["git", "-C", repo] + list(args), capture_output=True).stdout.decode("utf-8", "replace")


pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
start = pkg.index("**`RequiredTests`** (38 heredados + 8 nuevos")
rows = []
for line in pkg[start:].split("\n"):
    m = re.match(r"^\| (\d+) \| `([^`]+)` \| `FullyQualifiedName~([^`]+)` \| ([^|]+) \| (true|false) \|$", line)
    if m:
        rows.append((int(m.group(1)), m.group(2), m.group(3), m.group(4).strip(), m.group(5)))
    elif rows and not line.startswith("|"):
        break
L.append("rows in the table: %d (numbers %s..%s, contiguous=%s)" % (len(rows), rows[0][0], rows[-1][0],
                                                                    [r[0] for r in rows] == list(range(1, len(rows) + 1))))
inherited = [r for r in rows if not r[2].split(".")[-1].startswith("FullPanelCopy")]
copyrows = [r for r in rows if r[2].split(".")[-1].startswith("FullPanelCopy")]
L.append("non-copy filters: %d; copy filters: %d (Red with true: %d, Green with false: %d)" % (
    len(inherited), len(copyrows),
    sum(1 for r in copyrows if r[2].endswith("RedTests") and r[4] == "true"),
    sum(1 for r in copyrows if r[2].endswith("GreenTests") and r[4] == "false")))

# the copy classes: from the copy filters and from the probe copies (measurement mode)
copy_fqns = set(r[2] for r in copyrows)
probe_types = {}
ns_report = []
for root, _, files in os.walk(copies_dir):
    for f in files:
        if not f.endswith(".cs"):
            continue
        p = os.path.join(root, f)
        text = open(p, encoding="utf-8-sig").read().replace("\r\n", "\n")
        rel = os.path.relpath(p, copies_dir).replace(os.sep, "/")
        project = rel.split("/")[1]
        decls = [ln[len("namespace "):].strip().rstrip(";") for ln in text.split("\n") if ln.startswith("namespace ")]
        ok = len(decls) == 1 and decls[0] == project
        ns_report.append("%-70s %s %s" % (rel, decls, "ok" if ok else "OFFENDER"))
        block = not text.split("namespace ", 1)[1].split("\n", 1)[0].strip().endswith(";")
        indent = "    " if block else ""
        for m in re.finditer(r"^" + indent + r"(?:public |internal |private )?(?:sealed |static |abstract )*(?:partial )?(class|struct|record|interface|enum) (\w+)", text, re.M):
            probe_types.setdefault(m.group(2), []).append((project, rel))
            if m.group(2).startswith("FullPanelCopy"):
                copy_fqns.add(project + "." + m.group(2))
L.append("namespace guard by text on the probe copies:")
L.extend("  " + s for s in sorted(ns_report))

# filters 1..46 against every copy FQN (substring, as FullyQualifiedName~ does; the FQN is <ns>.<class>.<method>)
hits = []
for n, proj, flt, mins, red in inherited:
    for fq in sorted(copy_fqns):
        if flt in fq + ".X":
            hits.append((n, flt, fq))
L.append("non-copy filters selecting a copy class: %d %s" % (len(hits), hits))

# copy filters selecting another copy class or a non-copy class
cross = []
for n, proj, flt, mins, red in copyrows:
    for fq in sorted(copy_fqns):
        if fq != flt and flt in fq + ".X":
            cross.append((n, flt, fq))
L.append("copy filters selecting another copy class: %d %s" % (len(cross), cross))

# existing types in the test projects named like the copies' top-level types, and any existing FullPanelCopy type
existing = git("grep", "-n", "-E", r"(class|struct|record|interface|enum) FullPanelCopy", rev, "--", "tests", "src")
L.append("existing FullPanelCopy types at %s: %r" % (rev, existing.strip()))
clashes = []
for name, where in sorted(probe_types.items()):
    for project in set(w[0] for w in where):
        g = git("grep", "-n", "-E", r"(class|struct|record|interface|enum) %s\b" % name, rev, "--", "tests/" + project)
        if g.strip():
            clashes.append((name, project, g.strip().split("\n")))
L.append("top-level types in the copies: %d" % len(probe_types))
L.append("same-named types in the same test project at the revision: %d" % len(clashes))
for c in clashes:
    L.append("  %s (%s): %s" % (c[0], c[1], c[2][:3]))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
