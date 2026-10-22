"""Pasa el detector de fidelidad (kit/fidelity.py) por los archivos del run que no son copias (order.txt, a7-r1.md, prompt.md) y comprueba que
el run contiene exactamente RunFiles con los bytes de las copias del kit. Uso: python -I -B scan_run_files.py <kit> <run>"""
import hashlib
import importlib.util
import json
import os
import sys

kit, run = sys.argv[1], sys.argv[2]
spec = importlib.util.spec_from_file_location("fidelity", os.path.join(kit, "fidelity.py"))
F = importlib.util.module_from_spec(spec)
spec.loader.exec_module(F)
c = json.load(open(os.path.join(kit, "closure.json"), encoding="utf-8"))
for n in ("order.txt", "a7-r1.md", "prompt.md"):
    data = open(os.path.join(run, n), "rb").read()
    info = F.scan(n, data)
    print(n, "bytes", info["Bytes"], "lines", info["Lines"], "maxUTF16", info["MaxLineUtf16"], "flags", info["Flags"])
rf = F.run_files_of(c)
listing = sorted(os.listdir(run))
print("run listing == RunFiles:", listing == sorted(rf), len(listing), len(rf))
bad = [n for n in rf if hashlib.sha256(open(os.path.join(run, n), "rb").read()).digest() != hashlib.sha256(open(os.path.join(kit, n), "rb").read()).digest()]
print("run files differing from kit copies:", bad)
hashes = c.get("RunFileHashes") or {}
badh = [n for n in rf if isinstance(hashes, dict) and n in hashes and (hashes[n] if isinstance(hashes[n], str) else hashes[n].get("Sha256")) !=
        hashlib.sha256(open(os.path.join(kit, n), "rb").read()).hexdigest()]
print("RunFileHashes type:", type(hashes).__name__, "entries", len(hashes), "mismatching:", badh)
probs = []
for e in c["FaithfulCopies"]:
    cb = open(os.path.join(kit, e["Copy"]), "rb").read()
    if hashlib.sha256(cb).hexdigest() != e["CopySha256"]:
        probs.append(("copy sha", e["Copy"]))
    p, d = F.decode(cb)
    if F.git_blob(d) != e["OriginalBlob"] or p != e["Original"]:
        probs.append(("decode", e["Copy"]))
    if F.encode(p, d) != cb:
        probs.append(("canon", e["Copy"]))
print("faithful copies problems:", probs)
tmpl = open(os.path.join(kit, "prompt.md"), encoding="utf-8").read()
print("placeholders left in prompt.md:", tmpl.count("{{"), "| bytes", len(tmpl.encode("utf-8")))
