"""CA-T4c item 1: the four equivalents of PB-T4c (make_equivalents_pbt4c_v50.equivalents, in memory, over git objects at the revision)
against their originals: every line of an equivalent that is not a line of its original file; the EquivalentSha of the annex; the annex
text; the sha256 that /d/r5g wrote; and the four Skip lines (read-only).

Usage: python -I ir4_t4c_eq.py <repo> <rev> <package-v50.md> <apply-red.txt> <out.txt>
"""
import collections
import hashlib
import importlib.util
import re
import subprocess
import sys

repo, rev, pkg_p, red_p, out_p = sys.argv[1:6]
here = __file__.rsplit("ir4_t4c_eq.py", 1)[0]


def load(name):
    spec = importlib.util.spec_from_file_location(name + "_ir4", here + name + ".py")
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


eqm = load("make_equivalents_pbt4c_v50")
sk = load("pbt4c_skips_v50")


def show(path):
    return subprocess.run(["git", "-C", repo, "show", "%s:%s" % (rev, path)], capture_output=True).stdout.decode("utf-8-sig").replace("\r\n", "\n")


eq = eqm.equivalents(repo, rev)
orig_of = {eqm.E1: eqm.PEG, eqm.E2: eqm.SRG, eqm.E3: eqm.FCG, eqm.E4: eqm.WAC}
pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
red = {}
for l in open(red_p, encoding="utf-8", errors="replace").read().split("\n"):
    m = re.match(r"^([0-9a-f]{64})\s+\d+\s+(\S+)", l.strip())
    if m:
        red[m.group(2)] = m.group(1)
L = []
for p in eqm.PATHS:
    text = eq[p]
    sha = hashlib.sha256(text.encode("utf-8")).hexdigest()
    o = show(orig_of[p])
    o_lines = collections.Counter(l.rstrip() for l in o.split("\n"))
    extra = [(i + 1, l) for i, l in enumerate(text.split("\n")) if l.strip() and o_lines[l.rstrip()] == 0]
    m = re.search(r"`%s` \(`EquivalentSha` `([0-9a-f]{64})`\)" % re.escape(p), pkg)
    L.append("== %s (from %s): %d lines, sha256 %s" % (p, orig_of[p], text.count("\n"), sha[:12]))
    L.append("   annex EquivalentSha %s: %s; written in /d/r5g: %s" % ((m.group(1)[:12] if m else None), bool(m and m.group(1) == sha),
                                                                     red.get(p) == sha))
    # the annex code block right after the heading, when it carries the whole text
    if m:
        k = pkg.index("```csharp\n", m.end())
        nxt = pkg.find("\n### ", m.end())
        if nxt == -1 or k < nxt:
            blk = pkg[k + len("```csharp\n"):pkg.index("\n```", k)] + "\n"
            L.append("   annex block == the generated text: %s" % (blk == text))
    L.append("   lines that are not lines of the original: %d" % len(extra))
    for i, l in extra:
        L.append("     :%d %s" % (i, l[:240]))
L.append("== the Skip lines")
for s in sk.SKIPS:
    o = show(s[0]).split("\n")
    L.append("  %s:%d was %r; method on the next line: %s" % (s[0], s[1], o[s[1] - 1].strip(), ("public void " + s[2] + "(") in o[s[1]]))
    L.append("     skip: %s" % sk.skip_line(s).strip()[:260])
    L.append("     the equivalent named in the reason exists in the four: %s" % any(p.endswith("/" + s[5] + ".cs") for p in eqm.PATHS))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
