"""CA-T4b recheck, I-01: the self-test WHHG:501-526 at the revision (its Assert.NotEmpty, counted) against the block that the v4.9 copy of
WorkspaceHostHardeningGuardFull.cs adds after the Assert.Empty of NoneOfThe30..., and the v4.9 copy against the v4.8 one (read-only).

Usage: python -I ir4_i01.py <repo> <rev> <copy-v48.cs> <copy-v49.cs> <package-v49.md> <out.txt>
"""
import subprocess
import sys

repo, rev, c48_p, c49_p, pkg_p, out_p = sys.argv[1:7]
orig = subprocess.run(["git", "-C", repo, "show", "%s:tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs" % rev],
                      capture_output=True).stdout.decode("utf-8").replace("\r\n", "\n").split("\n")
c48 = open(c48_p, encoding="utf-8-sig").read().replace("\r\n", "\n").split("\n")
c49 = open(c49_p, encoding="utf-8-sig").read().replace("\r\n", "\n").split("\n")
L = []
self_test = orig[500:526]  # WHHG:501-526
L.append("WHHG:501 %s" % self_test[0].strip())
L.append("WHHG:502 %s" % self_test[1].strip())
L.append("WHHG:526 %s" % self_test[-1].strip())
n_ne = sum(l.count("Assert.NotEmpty(") for l in self_test)
L.append("Assert.NotEmpty in WHHG:501-526: %d (lines %s)" % (n_ne, [501 + i for i, l in enumerate(self_test) if "Assert.NotEmpty(" in l]))
L.append("other assertions in WHHG:501-526: %s" % [l.strip()[:60] for l in self_test if "Assert." in l and "Assert.NotEmpty(" not in l])
body = orig[503:525]  # WHHG:504-525
# v4.9 copy = v4.8 copy + one inserted block
i = 0
while i < len(c48) and c48[i] == c49[i]:
    i += 1
j48, j49 = len(c48) - 1, len(c49) - 1
while j48 >= i and c48[j48] == c49[j49]:
    j48, j49 = j48 - 1, j49 - 1
L.append("v4.9 copy vs v4.8: first difference at line %d; v4.8 lines replaced: %d; v4.9 lines inserted: %d" % (i + 1, j48 - i + 1, j49 - i + 1))
block = c49[i:j49 + 1]
L.append("the line before the block: %s" % c49[i - 1].strip())
L.append("the line after the block: %s" % c49[j49 + 1].strip() if j49 + 1 < len(c49) else "(end)")
# the block: optional blank, the I-01 comment, then WHHG:504-525 byte for byte
k = 0
while k < len(block) and (not block[k].strip() or block[k].strip().startswith("//")) and "var host" not in block[k]:
    k += 1
L.append("block: %d leading lines (blank and the I-01 comment): %s" % (k, [b.strip()[:50] for b in block[:k]]))
L.append("block after them == WHHG:504-525 byte for byte: %s" % (block[k:] == body))
if block[k:] != body:
    import difflib
    L.extend("   " + d for d in difflib.unified_diff(body, block[k:], lineterm="", n=0))
L.append("NotEmpty in the block: %d" % sum(l.count("Assert.NotEmpty(") for l in block))
# the method that holds it
decl = max(n for n in range(i) if "public void " in c49[n])
L.append("method holding the block: %s" % c49[decl].strip())
L.append("the table of the copy, v4.9 == v4.8: %s" % (
    c48[c48.index(next(l for l in c48 if "Table =" in l or "[] Table" in l)):][:40] == c49[c49.index(next(l for l in c49 if "Table =" in l or "[] Table" in l)):][:40]))
pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
a = pkg.index("## Anexo V49-A")
annex = pkg[a:]
blk = "\n".join(block[k:])
L.append("the annex V49-A carries the block (WHHG:504-525) literally: %s" % (blk in annex))
L.append("the annex V49-A carries the I-01 comment literally: %s" % ("\n".join(block[:k]).strip() in annex))
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
