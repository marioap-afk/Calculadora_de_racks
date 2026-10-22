"""Convierte la salida cruda de `python -B kit/launch.py --record-probe` (tubería en la codificación preferida del locale) a UTF-8 sin el nombre de
usuario, como finish_kit.frozen, y comprueba RecordProbe.Ok y FrozenCommandGuard.Ok. Uso: python -I -B convert_probe.py <crudo> <salida.json>
"""
import json
import locale
import re
import sys

USER_RX = re.compile(r"(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/\"\s]+")
out = USER_RX.sub("%USERPROFILE%", open(sys.argv[1], "rb").read().decode(locale.getpreferredencoding(False), "replace"))
d = json.loads(out)
assert (d.get("RecordProbe") or {}).get("Ok") is True and (d.get("FrozenCommandGuard") or {}).get("Ok") is True, "sonda no conforme"
with open(sys.argv[2], "w", encoding="utf-8", newline="\n") as f:
    f.write(out)
print(json.dumps({"RecordProbeOk": True, "FrozenCommandGuardOk": True, "Argv": d["FrozenCommandGuard"].get("Argv")}))
