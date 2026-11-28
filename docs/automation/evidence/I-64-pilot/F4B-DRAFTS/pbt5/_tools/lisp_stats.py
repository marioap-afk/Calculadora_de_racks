import os

here = os.path.dirname(os.path.abspath(__file__))
p = os.path.join(os.path.dirname(here), "pre-run-tonight-v2.2.md")
inside = False
rows = []
for no, ln in enumerate(open(p, encoding="utf-8").read().split(chr(10)), 1):
    s = ln.strip()
    if s.startswith("```"):
        inside = (s == "```lisp") if not inside else False
        continue
    if inside and s.startswith("("):
        name = s.split()[1] if s.startswith("(defun") else s[:24]
        rows.append((no, len(s), name))
for no, n, name in rows:
    print(no, n, name)
print("lineas LISP:", len(rows), "maximo:", max(n for _, n, _ in rows))
