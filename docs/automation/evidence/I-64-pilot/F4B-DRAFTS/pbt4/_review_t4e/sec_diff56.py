import sys, re, difflib
sys.stdout.reconfigure(encoding="utf-8")
A = sys.argv[1]
B = sys.argv[2]
mode = sys.argv[3] if len(sys.argv) > 3 else "summary"
want = sys.argv[4:]
a = open(A, encoding="utf-8").read().split("\n")
b = open(B, encoding="utf-8").read().split("\n")

def sections(lines):
    out = []
    cur = ("<pre>", 1)
    buf = []
    in_code = False
    for i, l in enumerate(lines):
        if l.startswith("```"):
            in_code = not in_code
        if not in_code and re.match(r"^#{1,4} ", l):
            out.append((cur[0], cur[1], buf))
            cur = (l.strip(), i + 1)
            buf = []
        buf.append(l)
    out.append((cur[0], cur[1], buf))
    return out

sa, sb = sections(a), sections(b)
da = {t: (ln, buf) for t, ln, buf in sa}
db = {t: (ln, buf) for t, ln, buf in sb}
print("sections a", len(sa), "b", len(sb), "dup a", len(sa) - len(da), "dup b", len(sb) - len(db))
same = []
changed = []
new = []
for t, ln, buf in sb:
    if t in da:
        if da[t][1] == buf:
            same.append(t)
        else:
            changed.append((t, ln, da[t][0]))
    else:
        new.append((t, ln))
removed = [t for t in da if t not in db]
if mode == "summary":
    print("same", len(same))
    for t, ln in new:
        print("NEW v56:%d %s" % (ln, t[:130]))
    for t in removed:
        print("REMOVED %s" % t[:130])
    for t, ln, la in changed:
        print("CHANGED new:%d old:%d %s" % (ln, la, t[:130]))
elif mode == "same":
    for t in same:
        print("SAME", t[:130])
elif mode == "diff":
    for t, ln, la in changed:
        if any(t.startswith(w) for w in want):
            print("=" * 10, t[:110])
            for l in difflib.unified_diff(da[t][1], db[t][1], lineterm="", n=0):
                if l.startswith("---") or l.startswith("+++"):
                    continue
                print(l[:2500])
