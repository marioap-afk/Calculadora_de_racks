"""Reproducción de las 43 copias fieles del kit desde el repositorio (sesión principal, 2026-10-22). Las copias no se custodian en el repositorio:
cinco de sus rutas, dentro de un worktree bajo %USERPROFILE%\\.codex\\worktrees\\, pasan de 260 caracteres (MAX_PATH; core.longpaths no está
activado), y cada copia es función pura de su original (la codificación canónica RACKCAD-COPIA-FIEL/1 de fidelity.encode). Para cada entrada de
FaithfulCopies del cierre: lee el original con `git cat-file blob <commit>:<ruta>`, comprueba su blob, lo codifica con kit/fidelity.py, y compara el
resultado byte a byte con la copia del kit y su SHA-256 con kit-manifest.json; además verify() sobre la copia regenerada.
Uso: python -I -B regen_faithful.py <kit> <clon> <salida.json>
"""
import hashlib
import importlib.util
import json
import os
import subprocess
import sys

KIT, CLONE, OUT = sys.argv[1:4]
spec = importlib.util.spec_from_file_location("fidelity", os.path.join(KIT, "fidelity.py"))
FID = importlib.util.module_from_spec(spec)
spec.loader.exec_module(FID)
closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
man = json.load(open(os.path.join(KIT, "kit-manifest.json"), encoding="utf-8"))
rev = closure["AuthorityRevision"]
env = dict(os.environ, GIT_OPTIONAL_LOCKS="0")
rows = []
fc = closure["FaithfulCopies"]
items = fc.items() if isinstance(fc, dict) else ((c.get("Path") or c.get("Original"), c) for c in fc)
for path, rec in sorted(items):
    data = subprocess.run(["git", "-C", CLONE, "cat-file", "blob", "%s:%s" % (rev, path)], capture_output=True, env=env, check=True).stdout
    blob = hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()
    name = FID.copy_name(path)
    regen = FID.encode(path, data)
    kit_bytes = open(os.path.join(KIT, name), "rb").read()
    probs = FID.verify(path, data, regen)
    rows.append({"Original": path, "OriginalBlob": blob, "Copy": name, "Sha256": hashlib.sha256(regen).hexdigest(), "Bytes": len(regen),
                 "BlobMatchesClosure": blob == rec.get("OriginalBlob"), "CopySha256MatchesClosure": hashlib.sha256(regen).hexdigest() == rec.get("CopySha256"),
                 "EqualsKitCopy": regen == kit_bytes, "EqualsManifest": man["Files"].get(name, {}).get("Sha256") == hashlib.sha256(regen).hexdigest(),
                 "VerifyProblems": probs or []})
doc = {"Schema": "a7r2-faithful-regeneration/1", "Commit": rev, "Copies": len(rows),
       "AllReproduced": len(rows) == 43 and all(r["BlobMatchesClosure"] and r["CopySha256MatchesClosure"] and r["EqualsKitCopy"] and r["EqualsManifest"] and not r["VerifyProblems"]
                                                for r in rows), "Rows": rows}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
print(json.dumps({"Copies": doc["Copies"], "AllReproduced": doc["AllReproduced"], "Bytes": sum(r["Bytes"] for r in rows)}))
sys.exit(0 if doc["AllReproduced"] else 1)
