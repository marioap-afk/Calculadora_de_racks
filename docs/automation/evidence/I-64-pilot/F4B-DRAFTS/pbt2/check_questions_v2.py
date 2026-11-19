# -*- coding: utf-8 -*-
# Solo lectura: comprueba en el paquete v2 la forma de Q-PBT2-1..4 para AskUserQuestion
# (texto < 400 caracteres, header <= 12, de 2 a 4 opciones, la recomendada primero).
# Uso: python -I check_questions_v2.py <ruta-del-paquete-v2>. No escribe nada.
import sys

text = open(sys.argv[1], encoding="utf-8").read().split("\n")
blocks = []
cur = None
for line in text:
    if line.startswith("**Q-PBT2-"):
        cur = {"id": line[2:10], "header": None, "question": None, "options": []}
        blocks.append(cur)
        continue
    if cur is None:
        continue
    if line.startswith("### ") or line.startswith("## "):
        cur = None
        continue
    s = line.strip()
    if s.startswith("- **header:**"):
        cur["header"] = s.split("`")[1]
    elif s.startswith("- **question:**"):
        q = s[len("- **question:**"):].strip()
        if q.startswith("«") and q.endswith("»"):
            q = q[1:-1]
        cur["question"] = q
    elif s.startswith("| ") and not s.startswith("| label") and not s.startswith("|---") and cur["question"] is not None and cur.get("done") is None:
        cells = [c.strip() for c in s.strip("|").split("|")]
        if len(cells) == 2:
            cur["options"].append(cells)
    elif s.startswith("- **Lo que cambia") or s.startswith("- **Por qu"):
        cur["done"] = True

for b in blocks:
    q = b["question"] or ""
    labels = [o[0] for o in b["options"]]
    print(b["id"], "header=%r len=%d" % (b["header"], len(b["header"] or "")),
          "question-len=%d" % len(q), "options=%d" % len(labels),
          "first-recommended=%s" % (bool(labels) and labels[0].endswith("(Recomendado)")),
          "other-recommended=%s" % any("(Recomendado)" in l for l in labels[1:]))
    for o in b["options"]:
        print("   label-len=%d desc-len=%d %s" % (len(o[0]), len(o[1]), o[0]))
