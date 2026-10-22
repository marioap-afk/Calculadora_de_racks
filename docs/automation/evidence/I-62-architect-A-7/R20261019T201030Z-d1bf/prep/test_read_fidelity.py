"""Prueba funcional de kit/read_fidelity.py corregido (decisiones §94, punto 5) sobre el clon de prueba de la fase 1: una transcripción
SINTÉTICA con lecturas del TRX de r1 con el BOM eliminado (como lo entregó Read en r1), del TRX entero fiel (con el BOM), de un archivo no UTF-8 del
fixture (antes detenía la comparación) y de un archivo limpio. Comprueba: el TRX sin BOM es DEGRADED [1] con la marca BOM; el fiel es
FAITHFUL_NORMALIZED; el no UTF-8 se compara sin excepción (DEGRADED [2] al entregarse con U+FFFD); el limpio, sin marcas.
Uso: python -I test_read_fidelity.py <kit> <clon> <dir de salida>
"""
import json
import os
import subprocess
import sys

KIT, CLONE, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
TRX = "docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/fixture-tests.trx"
INV, CLEAN = "zz-fidelity-fixture/invalid-utf8.txt", "zz-fidelity-fixture/clean.txt"
rev = subprocess.check_output(["git", "-C", CLONE, "rev-parse", "HEAD"]).decode().strip()


def lines(rel):
    t = subprocess.check_output(["git", "-C", CLONE, "show", rev + ":" + rel]).decode("utf-8", "surrogateescape").replace("\r\n", "\n").split("\n")
    return t[:-1] if t and t[-1] == "" else t


def numbered(L):
    return "\n".join("%d\t%s" % (i + 1, x) for i, x in enumerate(L))


trx = lines(TRX)
inv = lines(INV)
calls = [(TRX, numbered([trx[0].lstrip(chr(0xFEFF))] + trx[1:])), (TRX, numbered(trx)),
         (INV, numbered([inv[0], "byte %s suelto y secuencia truncada %s" % (chr(0xFFFD), chr(0xFFFD)), inv[2]])), (CLEAN, numbered(lines(CLEAN)))]
os.makedirs(OUT, exist_ok=True)
tx = os.path.join(OUT, "synthetic-transcript.jsonl")
with open(tx, "w", encoding="utf-8", newline="\n") as f:
    for k, (rel, out) in enumerate(calls):
        uid = "toolu_%03d" % k
        f.write(json.dumps({"type": "assistant", "message": {"content": [{"type": "tool_use", "id": uid, "name": "Read",
                                                                         "input": {"file_path": CLONE + "\\" + rel.replace("/", "\\")}}]}}) + "\n")
        f.write(json.dumps({"type": "user", "message": {"content": [{"type": "tool_result", "tool_use_id": uid, "content": out}]}}) + "\n")
res = os.path.join(OUT, "read-fidelity.json")
r = subprocess.run([sys.executable, "-I", "-B", os.path.join(KIT, "read_fidelity.py"), tx, CLONE, rev, res], capture_output=True, text=True)
doc = json.load(open(res, encoding="utf-8"))
c = doc["Calls"]
checks = {"TrxBomDroppedDegraded": c[0]["Status"] == "DEGRADED" and c[0]["Altered"] == [1] and c[0]["Flags"] == {"BOM": [1]} and c[0]["FlaggedLinesInCall"] == [1],
          "TrxWithBomFaithful": c[1]["Status"] == "FAITHFUL_NORMALIZED" and c[1]["Flags"] == {"BOM": [1]},
          "NonUtf8ComparedWithoutException": r.returncode == 0 and c[2]["Status"] == "DEGRADED" and c[2]["Altered"] == [2] and
                                             c[2]["Flags"] == {"INVALID_UTF8": [2]},
          "CleanNotFlagged": c[3]["Status"] == "FAITHFUL_NORMALIZED" and c[3]["Flags"] == {},
          "SummaryCounts": doc["Summary"]["Degraded"] == 2 and doc["Summary"]["CallsOnFlaggedFiles"] == 3}
out = {"Test": "read_fidelity.py (decisiones §94, punto 5) sobre una transcripción sintética", "Rev": rev, "ReturnCode": r.returncode,
       "Checks": checks, "AllPass": all(checks.values()), "Calls": [{k: x[k] for k in ("Path", "Status", "Altered", "Flags", "FlaggedLinesInCall")} for x in c]}
with open(os.path.join(OUT, "test-read-fidelity.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
print(json.dumps({"AllPass": out["AllPass"], "Checks": checks}))
