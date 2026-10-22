"""Medición, sobre la evidencia custodiada de la revisión r1 de A-7 (R20261019T010200Z-4a72, commit e71400a2), de lo que Read de claude-cli
2.1.299 entregó fielmente: para cada registro de fidelidad del audit.json custodiado (Kind CLOSURE, no EMPTY) se toman las líneas offset..offset+Lines-1
del texto canónico del clon (commit 5158e4bc, CRLF normalizado), menos las alteradas, y se cuentan sus caracteres y longitudes. No lee la
transcripción (solo el audit.json custodiado y el clon). Salida: JSON con los puntos de código no ASCII medidos, las longitudes máximas y la
presencia de TAB, de espacios finales y de caracteres de fuera del BMP en esas líneas. Fase 2: también los registros de Kind RUN (order.txt, leído
entero y FAITHFUL_NORMALIZED en r1), con el texto de la copia custodiada del kit (igual al del run de r1).

Uso: python -I measure_r1_literals.py <worktree> <clone> <out.json>
"""
import hashlib
import json
import subprocess
import sys

WT, CLONE, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
AUDIT_COMMIT = "e71400a2c7e842b4c8662e51675932623429bedf"
AUDIT_PATH = "docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/audit.json"
REV = "5158e4bc17ad48160d8573b7f48f3923e3197f06"   # el commit del clon de r1 (kit-params.json CloneCommit)


def git(repo, *a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True, check=True).stdout


rev = git(CLONE, "rev-parse", "HEAD").decode().strip()
assert rev == REV, "el clon no está en el commit de r1"
audit_b = git(WT, "show", AUDIT_COMMIT + ":" + AUDIT_PATH)
audit = json.loads(audit_b.decode("utf-8"))
cache = {}


def canon(p):
    if p not in cache:
        cache[p] = git(CLONE, "show", rev + ":" + p).decode("utf-8").replace("\r\n", "\n").split("\n")
    return cache[p]


ORDER_PATH = "docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/kit/order.txt"   # = el order.txt del run de r1 (custodia)


def run_text(name):
    assert name == "order.txt", name
    return git(WT, "show", AUDIT_COMMIT + ":" + ORDER_PATH).decode("utf-8").replace("\r\n", "\n").split("\n")


chars, lines, maxlen, maxbytes, tabs, trailing, astral, leading = {}, 0, (0, None), (0, None), 0, 0, 0, 0
by_kind = {"CLOSURE": 0, "RUN": 0}
for r in audit["Fidelity"]["Records"]:
    if r["Kind"] not in ("CLOSURE", "RUN") or r["Status"] == "EMPTY":
        continue
    L = canon(r["Path"]) if r["Kind"] == "CLOSURE" else run_text(r["Path"])
    off = r["Offset"] or 1
    for i in range(off, off + r["Lines"]):
        if i in r["Altered"] or i - 1 >= len(L):
            continue
        s = L[i - 1]
        lines += 1
        by_kind[r["Kind"]] += 1
        for c in s:
            chars[c] = chars.get(c, 0) + 1
        if len(s) > maxlen[0]:
            maxlen = (len(s), "%s:%d" % (r["Path"], i))
        if len(s.encode("utf-8")) > maxbytes[0]:
            maxbytes = (len(s.encode("utf-8")), "%s:%d" % (r["Path"], i))
        tabs += "\t" in s
        trailing += s != s.rstrip(" ")
        leading += s != s.lstrip(" ")
        astral += any(ord(c) > 0xFFFF for c in s)
non_ascii = sorted(c for c in chars if ord(c) > 127)
doc = {"Source": {"AuditCommit": AUDIT_COMMIT, "AuditPath": AUDIT_PATH, "AuditSha256": hashlib.sha256(audit_b).hexdigest(), "CloneHead": rev},
       "FaithfulLinesMeasured": lines, "FaithfulLinesByKind": by_kind,
       "PrintableAsciiMeasured": sum(1 for i in range(0x20, 0x7F) if chr(i) in chars),
       "PrintableAsciiMissing": [chr(i) for i in range(0x20, 0x7F) if chr(i) not in chars],
       "NonAsciiCodePoints": ["U+%04X" % ord(c) for c in non_ascii], "NonAsciiCount": len(non_ascii),
       "ControlCharsMeasured": ["U+%04X" % ord(c) for c in sorted(chars) if ord(c) < 0x20 or ord(c) == 0x7F],
       "LinesWithTab": tabs, "LinesWithTrailingSpace": trailing, "LinesWithLeadingSpace": leading, "LinesWithNonBmp": astral,
       "MaxLineChars": {"Chars": maxlen[0], "At": maxlen[1]}, "MaxLineBytes": {"Bytes": maxbytes[0], "At": maxbytes[1]},
       "NotDelivered": [{"Path": r["Path"], "Index": r["Index"], "Altered": r["Altered"]} for r in audit["Fidelity"]["Records"] if r["Altered"]],
       "TruncatedKnownLongLines": audit["Fidelity"]["TruncatedKnownLongLines"]}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(doc, ensure_ascii=True, indent=1) + "\n")
print(json.dumps({k: doc[k] for k in ("FaithfulLinesMeasured", "PrintableAsciiMeasured", "NonAsciiCount", "LinesWithTab", "LinesWithTrailingSpace",
                                      "LinesWithNonBmp", "MaxLineChars")}, ensure_ascii=True))
