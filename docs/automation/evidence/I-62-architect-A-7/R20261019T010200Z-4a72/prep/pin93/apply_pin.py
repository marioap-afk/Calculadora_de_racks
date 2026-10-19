"""Fija la autorización §93 en kit/owner_gate.py: sustituye SOLO las tres líneas AUTH_SECTION/AUTH_HEADING/AUTH_TEXT_SHA (entregadas en None) por las
PythonLines de pin-authorization.json (salida de `python -B -X utf8 kit/pin_authorization.py <decisiones de la HEAD> 93`, Ready = true), y escribe el
diff. Exige que cada línea antigua aparezca una sola vez y que el resultado difiera del original exactamente en esas tres líneas.
Uso: python -I -B apply_pin.py <kit dir> <pin-authorization.json> <diff de salida>
"""
import difflib
import json
import sys

kit, pin_path, diff_path = sys.argv[1:4]
pin = json.load(open(pin_path, encoding="utf-8"))
assert pin["Ready"] is True and pin["Values"]["AUTH_SECTION"] == 93 and len(pin["PythonLines"]) == 3, "pin no listo"
p = kit + "/owner_gate.py"
old = open(p, encoding="utf-8").read().split("\n")
prefixes = ("AUTH_SECTION = None", "AUTH_HEADING = None", "AUTH_TEXT_SHA = None")
new = list(old)
for pre, line in zip(prefixes, pin["PythonLines"]):
    hits = [i for i, l in enumerate(old) if l.startswith(pre)]
    assert len(hits) == 1, (pre, hits)
    new[hits[0]] = line
changed = [i for i, (a, b) in enumerate(zip(old, new)) if a != b]
assert len(changed) == 3 and len(old) == len(new), changed
open(p, "w", encoding="utf-8", newline="\n").write("\n".join(new))
d = "".join(difflib.unified_diff([l + "\n" for l in old], [l + "\n" for l in new], "kit/owner_gate.py (pendiente)", "kit/owner_gate.py (fijado en §93)"))
open(diff_path, "w", encoding="utf-8", newline="\n").write(d)
print(json.dumps({"ChangedLines": [i + 1 for i in changed], "Pins": pin["Values"]}, ensure_ascii=False))
