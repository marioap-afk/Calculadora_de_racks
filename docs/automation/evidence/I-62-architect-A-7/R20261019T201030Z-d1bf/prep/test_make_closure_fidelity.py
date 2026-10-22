"""Prueba de make_closure.faithful_copies() corregido (decisiones §94, punto 5), pura: registros sintéticos de cierre (BYTES poblado a mano).
  C1 un archivo con BOM → una copia, entrada FaithfulCopies coherente (SHA-256 de la copia = encode; marcas BOM [1]; sin apéndice);
  C2 un archivo limpio → ninguna copia;
  C3 una línea larga → copia con apéndice (ContinuedOriginalLines y AppendixLines);
  C4 un codificador que eliminara el BOM (FID.encode sustituido) → STOP (SystemExit), nunca una copia infiel;
  C5 un archivo no UTF-8 → copia (ya no detiene).
Uso: python -I -B test_make_closure_fidelity.py <kit> <salida.json>
"""
import hashlib
import importlib.util
import json
import os
import sys

KIT, OUT = os.path.abspath(sys.argv[1]), sys.argv[2]
spec = importlib.util.spec_from_file_location("mc", os.path.join(KIT, "make_closure.py"))
MC = importlib.util.module_from_spec(spec)
spec.loader.exec_module(MC)
F = MC.FID


def rec(p, b):
    MC.BYTES[p] = b
    return {"Path": p, "Blob": F.git_blob(b), "Sha256": hashlib.sha256(b).hexdigest(), "Bytes": len(b), "Lines": F.scan(p, b)["Lines"]}


bom = b"\xef\xbb\xbf<a>\n</a>"
clean = "texto limpio, línea\n".encode("utf-8")
long_ = b"x\n" + b"y" * 4000 + b"\n"
bad = b"a\xffb\n"
checks = {}
fid, copies, entries = MC.faithful_copies([rec("t/bom.trx", bom), rec("t/clean.txt", clean), rec("t/long.txt", long_), rec("t/bad.txt", bad)])
by = {e["Original"]: e for e in entries}
checks["C1-bom"] = (by["t/bom.trx"]["Flags"] == {"BOM": [1]} and by["t/bom.trx"]["Copy"] == "fiel__t__bom.trx.txt" and
                    by["t/bom.trx"]["CopySha256"] == hashlib.sha256(F.encode("t/bom.trx", bom)).hexdigest() and by["t/bom.trx"]["AppendixLines"] is None)
checks["C2-limpio-sin-copia"] = "t/clean.txt" not in by and fid["t/clean.txt"]["Flagged"] is False
checks["C3-linea-larga-con-apendice"] = by["t/long.txt"]["ContinuedOriginalLines"] == [2] and by["t/long.txt"]["AppendixLines"] == [4, 5] and \
    by["t/long.txt"]["CopyMaxLineChars"] <= 1900
checks["C5-no-utf8-con-copia"] = by["t/bad.txt"]["Flags"] == {"INVALID_UTF8": [1]}
real_encode = F.encode
F.encode = lambda path, data: real_encode(path, data[3:] if data.startswith(b"\xef\xbb\xbf") else data)
try:
    MC.faithful_copies([rec("t/bom.trx", bom)])
    checks["C4-codificador-infiel-stop"] = False
except SystemExit as e:
    checks["C4-codificador-infiel-stop"] = str(e).startswith("STOP: la copia fiel de t/bom.trx")
finally:
    F.encode = real_encode
doc = {"Test": "make_closure.faithful_copies()", "Checks": checks, "AllPass": all(checks.values()), "Entries": entries}
open(OUT, "w", encoding="utf-8", newline="\n").write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
print(json.dumps({"AllPass": doc["AllPass"], "Checks": checks}))
