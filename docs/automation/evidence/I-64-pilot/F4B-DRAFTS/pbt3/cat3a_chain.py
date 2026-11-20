import json
import subprocess
import sys

# Uso: python -I cat3a_chain.py <checkout> <rev>
# Las delegaciones de las correcciones de F4B: BaseSha, ChainBaseSha, ChainRedSha, ChainRedFiles, CorrectionOf y AuthorityRevision por intento.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO, REV = sys.argv[1], sys.argv[2]


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


paths = [p for p in git("ls-tree", "-r", "--name-only", REV, "docs/automation/evidence/I-64-pilot/").splitlines()
         if p.endswith("/delegation.json") and "-nc" not in p.split("/")[4] and p.split("/")[4].startswith(("F4B-", "F4-T4A1", "F4-T3"))]
for p in sorted(paths):
    try:
        d = json.loads(git("show", REV + ":" + p).lstrip("﻿"))
    except Exception as e:
        print(p, "ERR", e)
        continue
    co = d.get("CorrectionOf")
    print(p.split("/")[4], p.split("/")[5], p.split("/")[6],
          "| Attempt", d.get("Attempt"), "| AuthRev", (d.get("AuthorityRevision") or "")[:8], "| Base", (d.get("BaseSha") or "")[:8],
          "| ChainBase", (d.get("ChainBaseSha") or "")[:8], "| ChainRed", (d.get("ChainRedSha") or "null")[:8],
          "| ChainRedFiles", len(d.get("ChainRedFiles") or []),
          "| CorrectionOf", json.dumps(co, ensure_ascii=False)[:160] if co else None)
