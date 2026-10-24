"""Comprueba la salida cruda de `python -B kit/launch.py --dry-run` con la condición de finish_kit.py (probe_and_dry_run): BlockingChecks = solo la
compuerta 11, con Problems = [PENDING_REASON], AuthorizationPending, RecordProbeOk, FrozenCommandGuard.Ok, AuthorizationSections = [], LaterSections = []
y NotRunInDryRun = 4 y 5. La decodifica como finish_kit.frozen (codificación preferida del locale) y la escribe en UTF-8 sin el nombre de usuario.
Uso: python -I -B check_dry.py <kit> <crudo> <salida.json>
"""
import importlib.util
import json
import locale
import os
import re
import sys

KIT, RAW, OUT = sys.argv[1:4]
USER_RX = re.compile(r"(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/\"\s]+")
spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(KIT, "owner_gate.py"))
og = importlib.util.module_from_spec(spec)
spec.loader.exec_module(og)
out = USER_RX.sub("%USERPROFILE%", open(RAW, "rb").read().decode(locale.getpreferredencoding(False), "replace"))
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(out)
d = json.loads(out)
g = d["Checks"]["11-OwnerAndAuthorizationGate"]
ok = (d.get("BlockingChecks") == ["11-OwnerAndAuthorizationGate"] and g.get("Problems") == [og.PENDING_REASON] and
      g.get("AuthorizationPending") is True and d.get("RecordProbeOk") is True and (d.get("FrozenCommandGuard") or {}).get("Ok") is True and
      (d.get("AuthorizationSections") or []) == [] and (d.get("LaterSections") or []) == [] and
      sorted(d.get("NotRunInDryRun") or []) == ["4-Version", "5-Auth"])
others = {k: v.get("Ok") for k, v in d["Checks"].items() if k != "11-OwnerAndAuthorizationGate"}
print(json.dumps({"DryRunCondition": ok, "BlockingChecks": d.get("BlockingChecks"), "Gate11Problems": g.get("Problems"), "OtherChecks": others},
                 ensure_ascii=False))
sys.exit(0 if ok else 1)
