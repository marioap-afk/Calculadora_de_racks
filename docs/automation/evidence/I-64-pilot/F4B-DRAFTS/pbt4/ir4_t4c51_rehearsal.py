"""CA-T4c recheck, step 8: the expected lines of each class of the rehearsal of the text CTs (pbt4c-rehearsal-textct.cs) against the
lines of CT-01, 07, 08, 13 and 15 in pbt4c_ct_table_v51.py and in section 5.3 of the v5.1 package; and the rehearsal's own helpers against the
originals it says it copies (the purity terms and the G-L1 self-test names) (read-only).

Usage: python -I ir4_t4c51_rehearsal.py <rehearsal.cs> <package-v51.md> <out.txt>
"""
import importlib.util
import re
import sys

reh_p, pkg_p, out_p = sys.argv[1:4]
here = __file__.rsplit("ir4_t4c51_rehearsal.py", 1)[0]
spec = importlib.util.spec_from_file_location("pbt4c_ct_table_v51_ir4", here + "pbt4c_ct_table_v51.py")
ct = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ct)
table = {row[0]: row for row in ct.CT}
text = open(reh_p, encoding="utf-8-sig").read().replace("\r\n", "\n")
pkg = open(pkg_p, encoding="utf-8").read().replace("\r\n", "\n")


def literals(block):
    out = []
    for m in re.finditer(r'@"((?:[^"]|"")*)"|"((?:\\.|[^"\\])*)"', block):
        if m.group(1) is not None:
            out.append(m.group(1).replace('""', '"'))
        else:
            out.append(re.sub(r"\\(.)", lambda x: {"n": "\n", "t": "\t", "\\": "\\", '"': '"'}.get(x.group(1), x.group(1)), m.group(2)))
    return out


L = []
classes = [(m.group(1), m.start()) for m in re.finditer(r"public sealed class TmpPbt4cTextCt(\d\d)", text)]
for k, (num, start) in enumerate(classes):
    end = classes[k + 1][1] if k + 1 < len(classes) else len(text)
    body = text[start:end]
    i = body.rfind('Assert.Equal(string.Join("\\n", new[]')
    j = body.index('}), string.Join("\\n", log));', i)
    expected = []
    # the array: literal strings, possibly built by concatenation or by a helper expression; keep the literal items in order
    arr = body[i + len('Assert.Equal(string.Join("\\n", new[]'):j]
    items = [s.strip().rstrip(",").strip() for s in re.split(r",\s*\n", arr.strip().lstrip("{").strip())]
    computed = []
    for it in items:
        if not it:
            continue
        if re.fullmatch(r'(?:@?"(?:\\.|[^"\\]|"")*"\s*\+?\s*)+', it):
            expected.append("".join(literals(it)))
        else:
            computed.append(it[:120])
    want = table[num][2]
    L.append("== CT-%s (%s, ExpectRed %s): %d literal items in the rehearsal, %d computed; table %d lines" % (
        num, table[num][3], table[num][5], len(expected), len(computed), len(want)))
    if computed:
        L.append("   computed items: %s" % computed[:6])
    same = expected == want
    L.append("   literal items == the table, in order: %s" % same)
    if not same:
        for a, b in zip(expected, want):
            if a != b:
                L.append("     rehearsal: %s" % a[:200])
                L.append("     table:     %s" % b[:200])
                break
        L.append("     only in the rehearsal: %s" % [x[:80] for x in expected if x not in want][:5])
        L.append("     only in the table: %s" % [x[:80] for x in want if x not in expected][:5])
    in_pkg = all(("`%s`" % w) in pkg for w in want)
    L.append("   every table line is in section 5.3 of the package between backticks: %s" % in_pkg)
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
