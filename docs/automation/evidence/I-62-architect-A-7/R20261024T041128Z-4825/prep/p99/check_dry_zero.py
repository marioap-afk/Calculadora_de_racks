"""Decisiones §99, puntos 2 y 4 (derivado del de §97): comprueba la salida cruda de `python -B kit/launch.py --dry-run` con el kit FIJADO: cero bloqueos (BlockingChecks = []);
compuerta 11 en Ok con AuthorizationSections = [§99] (encabezado, blob y SHA-256 del texto conformes) y LaterSections = []; sonda de registro y guarda
de la orden congelada en Ok; comprobación 3 con el binario de claude-cli 2.1.299 con su SHA-256 fijado, Authenticode Valid y firmante Anthropic;
comprobación 6 con el clon limpio; 4 y 5 sin ejecutar en seco. Decodifica como finish_kit.frozen y escribe UTF-8 sin el nombre de usuario.
Uso: python -I -B check_dry_zero.py <kit> <crudo> <salida.json>
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
closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
out = USER_RX.sub("%USERPROFILE%", open(RAW, "rb").read().decode(locale.getpreferredencoding(False), "replace"))
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(out)
d = json.loads(out)
c = d["Checks"]
g, b, cl = c["11-OwnerAndAuthorizationGate"], c["3-Binary"], c["6-Clone"]
auth = d.get("AuthorizationSections") or []
pins = og.pinned_authorization()
conds = {
    "ZeroBlocking": d.get("BlockingChecks") == [],
    "Gate11Ok": g.get("Ok") is True and not g.get("Problems") and g.get("AuthorizationPending") is False and g.get("Authorized") is True,
    "AuthorizationSection99": len(auth) == 1 and auth[0].get("Number") == 99 == pins["Section"] and auth[0].get("HeadingMatches") is True and
                              auth[0].get("HasBlob") is True and auth[0].get("TextSha256Matches") is True,
    "NoLaterSections": (d.get("LaterSections") or []) == [] and (g.get("LaterSections") or []) == [],
    "RecordProbeAndGuard": d.get("RecordProbeOk") is True and (d.get("FrozenCommandGuard") or {}).get("Ok") is True,
    "Binary": b.get("Ok") is True and b.get("Sha256") == closure["Transport"]["Binary"]["Sha256"] and b.get("Authenticode") == "Valid" and
              b.get("SignerIsAnthropic") is True,
    "CloneClean": cl.get("Ok") is True and cl.get("Clean") is True and cl.get("Head") == closure["AuthorityRevision"],
    "AllOtherChecksOk": all(v.get("Ok") is True for k, v in c.items() if k not in ("4-Version", "5-Auth")),
    "NotRunInDryRun": sorted(d.get("NotRunInDryRun") or []) == ["4-Version", "5-Auth"],
}
print(json.dumps({"DryRunZeroBlocking": all(conds.values()), "Conditions": conds, "BlockingChecks": d.get("BlockingChecks"),
                  "Gate11Problems": g.get("Problems")}, ensure_ascii=False))
sys.exit(0 if all(conds.values()) else 1)
