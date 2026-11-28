"""CA-T4b: the table of WHHG NoneOfThe30... (H-07) against the base and the GREEN literal of PB-T4b, and the equivalent's table of the
annex V48-A of the package (read-only: git show through green_pbt4b_v45.py and here).

Usage: python -I ir4_whhg.py <repo> <rev> <package-v48.md> <out.txt>
"""
import importlib.util
import re
import subprocess
import sys

repo, rev, pkg_p, out_p = sys.argv[1:5]
here = __file__.rsplit("ir4_whhg.py", 1)[0]
spec = importlib.util.spec_from_file_location("green_pbt4b_v45", here + "green_pbt4b_v45.py")
gb = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gb)
base, green = gb.green(repo, rev)


def show(path):
    return subprocess.run(["git", "-C", repo, "show", "%s:%s" % (rev, path)], capture_output=True).stdout.decode("utf-8").replace("\r\n", "\n")


def code_only(t):
    return re.sub(r"//[^\r\n]*", "", re.sub(r"/\*.*?\*/", "", t, flags=re.S))


ROW = re.compile(r'^\s*\("((?:[^"\\]|\\.)*)",\s*"((?:[^"\\]|\\.)*)",\s*(\d+)\),?(.*)$')


def table(text):
    start = text.index("(string English, string Spanish, int Times)[] Table")
    end = text.index("};", start)
    rows = []
    for line in text[start:end].split("\n"):
        m = ROW.match(line)
        if m:
            rows.append((m.group(1), m.group(2), int(m.group(3)), m.group(4).strip()))
    return rows


orig = table(show("tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs"))
pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")
annex = pkg[pkg.index("## Anexo V48-A"):]
eq = table(annex)
support = show("src/RackCad.Application/Workspace/WorkspacePilotSupport.cs")
whc_p = gb.WHC_P
all_base = code_only(base[whc_p]) + "\n" + code_only(support)
all_green = code_only(green[whc_p]) + "\n" + code_only(support)
L = ["rows original=%d annex=%d" % (len(orig), len(eq))]
diff_rows = 0
for (e1, s1, t1, c1), (e2, s2, t2, c2) in zip(orig, eq):
    fb = len(re.findall(re.escape('"' + s1 + '"'), all_base))
    fg = len(re.findall(re.escape('"' + s2 + '"'), all_green))
    same_text = (e1, s1) == (e2, s2)
    mark = "" if (t1 == t2 and same_text) else "  <== CHANGED"
    if mark:
        diff_rows += 1
    ok_b = "ok" if fb == t1 else "BASE-MISMATCH"
    ok_g = "ok" if fg == t2 else "GREEN-MISMATCH"
    L.append("%-60s orig=%d base=%d %s | annex=%d green=%d %s%s %s" % (e1[:60], t1, fb, ok_b, t2, fg, ok_g, mark, c2))
L.append("rows changed: %d; texts identical: %s" % (diff_rows, all((a[0], a[1]) == (b[0], b[1]) for a, b in zip(orig, eq))))
english_left = [e for e, s, t, c in eq if '"' + e + '"' in all_green]
L.append("English literals left in the GREEN: %s" % english_left)
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
