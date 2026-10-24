"""Compare Read tool results recorded in a Claude Code transcript (JSONL) with the canonical bytes of the files at a commit.

Usage: python read_fidelity.py <transcript.jsonl> <clone> <rev> <output json> [path-prefix-filter]
Every Read call on a file under <clone> is paired with its tool_result; each returned line «N<TAB>text» is compared with line N of the canonical
text (git show <rev>:<path>, CRLF normalized to LF). The only normalization allowed is the end of line. Both line formats are accepted: the
padded one of the desktop sessions («     N<TAB>») and the bare one measured on claude-cli 2.1.293 («N<TAB>»). Lines not returned are «not
delivered». A file is under <clone> only when its path continues with a separator after the clone path, so the run directory D:/r62-arch-a6-run
is NOT under the clone D:/r62-arch-a6. (Kit of the first formal review of A-6; unchanged from the accredited A-2 kit except this example.) The output records the filter (or null), the command and the transcript SHA-256; a user directory in the
recorded transcript path is written as %USERPROFILE%.
Read-fidelity correction (decisions §94, point 5; fidelity.py from this directory): the canonical text is decoded with surrogateescape (a non-UTF-8
file no longer stops the comparison), and every call records the detector's flags of its file (BOM, INVALID_UTF8, NUL, BARE_CR, CONTROL, LONG_LINE,
ZWNBSP, LINE_SEPARATOR, SPECIAL_EXTENSION) and the flagged lines it delivered; a silently dropped BOM stays an altered line 1 (DEGRADED), never
faithful. Faithful copies in the run directory are outside the clone and are checked by post-review.py.
"""
import hashlib
import importlib.util
import json
import os
import re
import subprocess
import sys

_spec = importlib.util.spec_from_file_location("fidelity", os.path.join(os.path.dirname(os.path.abspath(__file__)), "fidelity.py"))
FID = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(FID)

TRANSCRIPT, CLONE, REV, OUT = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
FILTER = sys.argv[5] if len(sys.argv) > 5 else ""
clone_norm = CLONE.replace("/", "\\").rstrip("\\").lower()


def under_clone(fp):
    """True only for a path strictly inside the clone (a separator is required right after the clone path)."""
    return fp.replace("/", "\\").lower().startswith(clone_norm + "\\")


def userless(path):
    return re.sub(r"^[A-Za-z]:/Users/[^/]+", "%USERPROFILE%", path.replace("\\", "/"))


calls, results = {}, {}
for line in open(TRANSCRIPT, encoding="utf-8"):
    try:
        o = json.loads(line)
    except ValueError:
        continue
    msg = o.get("message") or {}
    content = msg.get("content")
    if not isinstance(content, list):
        continue
    for part in content:
        if not isinstance(part, dict):
            continue
        if part.get("type") == "tool_use" and part.get("name") == "Read":
            fp = (part.get("input") or {}).get("file_path", "")
            if under_clone(fp):
                calls[part["id"]] = part["input"]
        elif part.get("type") == "tool_result" and part.get("tool_use_id") in calls:
            c = part.get("content")
            text = c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))
            results[part["tool_use_id"]] = text

canon_cache, flags_cache = {}, {}


def canon(rel):
    if rel not in canon_cache:
        b = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel])
        flags_cache[rel] = FID.scan(rel, b)["Flags"]
        canon_cache[rel] = b.decode("utf-8", "surrogateescape").replace("\r\n", "\n").split("\n")
    return canon_cache[rel]


report, delivered = [], {}
for uid, inp in calls.items():
    rel = inp["file_path"].replace("\\", "/")
    rel = rel[len(CLONE.replace("\\", "/").rstrip("/")) + 1:]          # under_clone() already required the separator
    if FILTER and not rel.startswith(FILTER):
        continue
    text = results.get(uid)
    entry = {"Path": rel, "Offset": inp.get("offset"), "Limit": inp.get("limit"), "Lines": 0, "Faithful": 0, "Altered": [], "Unparsed": 0}
    if text is None:
        entry["Status"] = "NO_RESULT"
        report.append(entry)
        continue
    lines = canon(rel)
    for raw in text.replace("\r\n", "\n").split("\n"):
        if "\t" not in raw:
            if raw.strip():
                entry["Unparsed"] += 1
            continue
        num, body = raw.split("\t", 1)
        num = num.strip()
        if not num.isdigit():
            entry["Unparsed"] += 1
            continue
        n = int(num)
        entry["Lines"] += 1
        ok = 1 <= n <= len(lines) and lines[n - 1] == body
        if ok:
            entry["Faithful"] += 1
            delivered.setdefault(rel, set()).add(n)
        else:
            entry["Altered"].append(n)
    entry["Status"] = "FAITHFUL_NORMALIZED" if entry["Lines"] and not entry["Altered"] else ("DEGRADED" if entry["Altered"] else "EMPTY")
    fl = flags_cache.get(rel, {})
    affected = sorted({n for v in fl.values() for n in v if n})
    entry["Flags"] = fl
    entry["FlaggedLinesInCall"] = [n for n in affected if n in set(entry["Altered"]) | set(delivered.get(rel, set()))][:50]
    report.append(entry)

out = {"Transcript": userless(TRANSCRIPT), "TranscriptSha256": hashlib.sha256(open(TRANSCRIPT, "rb").read()).hexdigest(),
       "Clone": CLONE, "Filter": FILTER or None,
       "Command": "python read_fidelity.py <transcript> %s %s <output json>%s" % (CLONE, REV, (" " + FILTER) if FILTER else ""),
       "Rev": REV, "Calls": report,
       "Summary": {"Calls": len(report), "Faithful": sum(1 for r in report if r["Status"] == "FAITHFUL_NORMALIZED"),
                   "Degraded": sum(1 for r in report if r["Status"] == "DEGRADED"),
                   "CallsOnFlaggedFiles": sum(1 for r in report if r.get("Flags")),
                   "DeliveredLinesByPath": {k: len(v) for k, v in sorted(delivered.items())}}}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(out, f, ensure_ascii=False, indent=1)
    f.write("\n")
print(json.dumps(out["Summary"], ensure_ascii=True))
