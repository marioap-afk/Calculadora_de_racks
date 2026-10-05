"""Compare Read tool results recorded in a Claude Code transcript (JSONL) with the canonical bytes of the files at a commit.

Usage: python read_fidelity.py <transcript.jsonl> <clone> <rev> <output json> [path-prefix-filter]
Every Read call on a file under <clone> is paired with its tool_result; each returned line «N<TAB>text» is compared with line N of the canonical
text (git show <rev>:<path>, CRLF normalized to LF). The only normalization allowed is the end of line. Lines not returned are «not delivered».
"""
import json
import subprocess
import sys

TRANSCRIPT, CLONE, REV, OUT = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4]
FILTER = sys.argv[5] if len(sys.argv) > 5 else ""
clone_norm = CLONE.replace("/", "\\").rstrip("\\").lower()

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
            if fp.replace("/", "\\").lower().startswith(clone_norm):
                calls[part["id"]] = part["input"]
        elif part.get("type") == "tool_result" and part.get("tool_use_id") in calls:
            c = part.get("content")
            text = c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))
            results[part["tool_use_id"]] = text

canon_cache = {}


def canon(rel):
    if rel not in canon_cache:
        canon_cache[rel] = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")
    return canon_cache[rel]


report, delivered = [], {}
for uid, inp in calls.items():
    rel = inp["file_path"].replace("\\", "/")
    rel = rel[len(CLONE.replace("\\", "/").rstrip("/")) + 1:] if rel.lower().startswith(CLONE.replace("\\", "/").rstrip("/").lower()) else rel
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
    report.append(entry)

out = {"Transcript": TRANSCRIPT, "Rev": REV, "Calls": report,
       "Summary": {"Calls": len(report), "Faithful": sum(1 for r in report if r["Status"] == "FAITHFUL_NORMALIZED"),
                   "Degraded": sum(1 for r in report if r["Status"] == "DEGRADED"),
                   "DeliveredLinesByPath": {k: len(v) for k, v in sorted(delivered.items())}}}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(out, f, ensure_ascii=False, indent=1)
    f.write("\n")
print(json.dumps(out["Summary"], ensure_ascii=True))
