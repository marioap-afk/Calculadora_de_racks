"""CA-T4b items 2 and 6: the 478 list against the 477 one, the copy map v5 against v4, the measured partition of v4.8 (r4q) against the synthetic
one, against the v4.7 measured one (r4j) and against the list, and R_C / R_UI (read-only).

Usage: python -I ir4_part.py <hs65-dir> <out.txt>
"""
import collections
import os
import sys

hs, out_p = sys.argv[1:3]
P4 = os.path.join(hs, "pbt4")
CC = os.path.join(hs, "pbt4bcc", "compile")
L = []


def lines(p):
    return open(p, encoding="utf-8").read().replace("\r\n", "\n").split("\n")


def tsv(p):
    ls = lines(p)
    return ls[0], [l for l in ls[1:] if l]


h477, l477 = tsv(os.path.join(P4, "pbt4b-substitution-list-477.tsv"))
h478, l478 = tsv(os.path.join(P4, "pbt4b-substitution-list-478.tsv"))
L.append("list 477: %d rows; 478: %d rows; same header: %s" % (len(l477), len(l478), h477 == h478))
s477, s478 = set(l477), set(l478)
L.append("rows only in 477: %d %s" % (len(s477 - s478), sorted(s477 - s478)))
L.append("rows only in 478: %d" % len(s478 - s477))
for r in sorted(s478 - s477):
    L.append("  + " + r)
L.append("478 = 477 in the same order once the new row is removed: %s" % ([r for r in l478 if r in s477] == l477))
L.append("the new row is the last one: %s" % (l478[-1] not in s477))
by = collections.Counter(r.split("\t")[0] for r in l478)
L.append("478 by list label: %s" % dict(sorted(by.items())))
core = sum(1 for r in l478 if not r.split("\t")[1].startswith("tests/RackCad.UI.Tests/"))
L.append("478: Core %d, UI %d, files %d" % (core, len(l478) - core, len(set(r.split("\t")[1] for r in l478))))
dup = [k for k, v in collections.Counter((r.split("\t")[1], r.split("\t")[2]) for r in l478).items() if v > 1]
L.append("repeated (file, method) in 478: %s" % dup)

hm4, m4 = tsv(os.path.join(P4, "pbt4b-copy-map-v4.tsv"))
hm5, m5 = tsv(os.path.join(P4, "pbt4b-copy-map-v5.tsv"))
L.append("copy map v4: %d rows; v5: %d rows; same header: %s (%s)" % (len(m4), len(m5), hm4 == hm5, hm5))
d4 = {tuple(r.split("\t")[:2]): r for r in m4}
d5 = {tuple(r.split("\t")[:2]): r for r in m5}
L.append("map keys equal: %s" % (set(d4) == set(d5)))
for k in sorted(set(d4) | set(d5)):
    if d4.get(k) != d5.get(k):
        L.append("  map v4: %s" % d4.get(k))
        L.append("  map v5: %s" % d5.get(k))
kind = collections.Counter(r.split("\t")[2] for r in m5)
L.append("map v5 by kind (column 3): %s" % dict(kind))


def part(p):
    _, rows = tsv(p)
    out = {}
    for r in rows:
        f, m, k, n = r.split("\t")
        assert (f, m, k) not in out, r
        out[(f, m, k)] = int(n)
    return out


meas = part(os.path.join(CC, "out-probe-v48-r4q", "pbt4b-copy-partition.tsv"))
syn = part(os.path.join(P4, "v48", "partition-synthetic-v48.tsv"))
old = part(os.path.join(CC, "out-probe-v47-r4j", "pbt4b-copy-partition.tsv"))
L.append("partition v4.8 measured: %d rows; synthetic: %d; equal as a set: %s" % (len(meas), len(syn), meas == syn))
L.append("partition v4.7 (r4j): %d rows" % len(old))
for k in sorted(set(old) | set(meas)):
    if old.get(k) != meas.get(k):
        L.append("  v4.7 %s -> v4.8 %s : %s" % (old.get(k), meas.get(k), k))
r_c = sum(n for (f, m, k), n in meas.items() if k == "Red" and not f.startswith("tests/RackCad.UI.Tests/"))
r_ui = sum(n for (f, m, k), n in meas.items() if k == "Red" and f.startswith("tests/RackCad.UI.Tests/"))
L.append("R_C=%d R_UI=%d; rows Red %d, Green %d" % (r_c, r_ui, sum(1 for k in meas if k[2] == "Red"), sum(1 for k in meas if k[2] == "Green")))
tests = set((f, m) for f, m, k in meas)
listed = set((r.split("\t")[1], r.split("\t")[2]) for r in l478)
L.append("tests in the partition: %d; in the list but not in the partition: %s" % (len(tests), sorted(listed - tests)))
L.append("in the partition but not in the list: %s" % sorted(tests - listed))
mixed = sorted((f, m) for f, m in tests if (f, m, "Red") in meas and (f, m, "Green") in meas)
L.append("split by rows: %d" % len(mixed))
for f, m in mixed:
    L.append("  %s %s red=%d green=%d" % (f.split("/")[-1], m, meas[(f, m, "Red")], meas[(f, m, "Green")]))
total_rows = sum(meas.values())
L.append("copy results in the partition: %d (Core %d, UI %d)" % (
    total_rows, sum(n for (f, m, k), n in meas.items() if not f.startswith("tests/RackCad.UI.Tests/")),
    sum(n for (f, m, k), n in meas.items() if f.startswith("tests/RackCad.UI.Tests/"))))
classes = collections.Counter((f.split("/")[-1][:-len("Tests.cs")], k) for f, m, k in meas)
L.append("classes with tests: %d of 42" % len(classes))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
