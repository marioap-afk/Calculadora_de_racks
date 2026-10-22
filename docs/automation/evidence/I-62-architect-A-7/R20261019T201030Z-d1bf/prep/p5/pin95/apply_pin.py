"""Decisiones §95, punto 4: sustituye en kit/owner_gate.py SOLO las tres líneas AUTH_* (las que el kit entrega en None) por las PythonLines que
pin_authorization.py calculó sobre el texto de decisiones de 6b00b2bf (prep/p5/pin95/pin.json). Comprueba el SHA-256 anterior del módulo, que solo
cambian esas tres líneas y que el módulo queda fijado con esos valores. Uso: python -I -B apply_pin.py <kit> <pin.json> <diff de salida>
"""
import difflib
import hashlib
import importlib.util
import json
import os
import sys

KIT, PIN, DIFF = sys.argv[1:4]
p = os.path.join(KIT, "owner_gate.py")
src = open(p, "rb").read()
assert hashlib.sha256(src).hexdigest() == "3d420a9ff560c5bd88f811edff878da3f2d8056a531e9429b6412b82e5e80b19"
pin = json.load(open(PIN, encoding="utf-8"))
assert pin["Ready"] is True and pin["Values"]["AUTH_SECTION"] == 95
old = ["AUTH_SECTION = None   # PENDIENTE: la disposición del Coordinator posterior a §94 que autorice esta revisión (pin_authorization.py)",
       "AUTH_HEADING = None", "AUTH_TEXT_SHA = None"]
lines = src.decode("utf-8").split("\n")
idx = [lines.index(o) for o in old]
assert idx == [idx[0], idx[0] + 1, idx[0] + 2] and all(lines.count(o) == 1 for o in old)
new_lines = list(lines)
new_lines[idx[0]:idx[0] + 3] = pin["PythonLines"]
new = "\n".join(new_lines).encode("utf-8")
open(p, "wb").write(new)
d = "".join(difflib.unified_diff([l + "\n" for l in lines], [l + "\n" for l in new_lines], "kit/owner_gate.py@3d420a9f",
                                 "kit/owner_gate.py@%s" % hashlib.sha256(new).hexdigest()[:8]))
open(DIFF, "w", encoding="utf-8", newline="\n").write(d)
spec = importlib.util.spec_from_file_location("og", p)
og = importlib.util.module_from_spec(spec)
spec.loader.exec_module(og)
assert og.pinned_authorization() == {"Section": 95, "Heading": pin["Values"]["AUTH_HEADING"], "TextSha256": pin["Values"]["AUTH_TEXT_SHA"]}
assert not og.authorization_pending(og.pinned_authorization())
changed = [i for i, (a, b) in enumerate(zip(lines, new_lines)) if a != b]
assert changed == idx and len(lines) == len(new_lines)
print(json.dumps({"OwnerGateSha256": hashlib.sha256(new).hexdigest(), "ChangedLines": [i + 1 for i in changed], "Pins": og.pinned_authorization()},
                 ensure_ascii=False))
