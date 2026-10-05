"""I-62 A-1: post-review audit of the Architect session (preparation kit; run by the invoking session after the reviewer finishes).

Reads the reviewer's Claude Code transcript (JSONL) and produces, without trusting anything the reviewer declares:
  1. RuntimeIdentity   — session ids, cwd, runtime version and the model of every assistant message (observed, not declared);
  2. PromptCheck       — the first user turn, the sha256sum of prompt.md and a faithful Read of prompt.md;
  3. ReadAudit         — every tool call classified against the closure (closure.json) and the allowed actions; anything else is a violation;
  4. Fidelity          — every Read result and every `git show <rev>:<path> | sed -n 'A,Bp'` output compared line by line with the canonical bytes
                         (end of line is the only normalization); lines never delivered are reported per path;
  5. Result            — the last ```json block of the final assistant message, validated with PowerShell Test-Json against result.schema.json
                         and with the coherence rules of the order (AGREED ⇒ zero REQUIRED and IfAgreed all true, …);
  6. PremiseCheck      — every PremiseRefs.Quote must occur, whitespace-collapsed, inside its Path lines LineStart..LineEnd at <rev>;
  7. Accreditation     — ACCREDITED only if 1-6 raise no violation; otherwise NOT_ACCREDITED with the exact reasons.

Usage: python post-review.py <transcript.jsonl> <clone> <run dir with closure.json, prompt.md, order.txt> <result.schema.json> <out.json>
"""
import hashlib
import json
import os
import re
import subprocess
import sys
import tempfile

TRANSCRIPT, CLONE, RUN, SCHEMA, OUT = sys.argv[1:6]
CLOSURE = json.load(open(os.path.join(RUN, "closure.json"), encoding="utf-8"))
REV = CLOSURE["AuthorityRevision"]
ALLOWED = {r["Path"] for r in CLOSURE["CanonicalInputs"] + CLOSURE["AllowedTransitiveInputs"]}
RUN_FILES = {"prompt.md", "order.txt"}
PROMPT_SHA = hashlib.sha256(open(os.path.join(RUN, "prompt.md"), "rb").read()).hexdigest()


def norm(p):
    return p.replace("\\", "/").rstrip("/").lower()


CLONE_N, RUN_N = norm(CLONE), norm(RUN)


def classify_path(p):
    """→ ('CLOSURE', rel) | ('RUN', name) | ('OUTSIDE', p). A worktree under the clone (.claude/worktrees/<name>/) counts as the clone."""
    n = norm(p)
    if n.startswith(RUN_N + "/"):
        name = n[len(RUN_N) + 1:]
        return ("RUN", name) if name in RUN_FILES else ("OUTSIDE", p)
    if n.startswith(CLONE_N + "/"):
        rel = p.replace("\\", "/")[len(CLONE_N) + 1:]
        m = re.match(r"^\.claude/worktrees/[^/]+/(.*)$", rel)
        if m:
            rel = m.group(1)
        if rel in ALLOWED:
            return ("CLOSURE", rel)
        return ("OUTSIDE", p)
    return ("OUTSIDE", p)


canon_cache = {}


def canon(rel):
    if rel not in canon_cache:
        canon_cache[rel] = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")
    return canon_cache[rel]


def run_file(name):
    return open(os.path.join(RUN, name), encoding="utf-8").read().replace("\r\n", "\n").split("\n")


# ------------------------------------------------------------------ parse the transcript
entries = []
for line in open(TRANSCRIPT, encoding="utf-8"):
    try:
        entries.append(json.loads(line))
    except ValueError:
        pass

identity = {"SessionIds": set(), "Cwds": set(), "Versions": set(), "Models": {}, "SidechainEntries": 0}
uses, results, user_texts, assistant_texts = {}, {}, [], []
order = []
for o in entries:
    if o.get("sessionId"):
        identity["SessionIds"].add(o["sessionId"])
    if o.get("cwd"):
        identity["Cwds"].add(o["cwd"])
    if o.get("version"):
        identity["Versions"].add(o["version"])
    if o.get("isSidechain"):
        identity["SidechainEntries"] += 1
    msg = o.get("message") or {}
    if o.get("type") == "assistant" and msg.get("model"):
        identity["Models"][msg["model"]] = identity["Models"].get(msg["model"], 0) + 1
    content = msg.get("content")
    if o.get("type") == "user" and isinstance(content, str):
        user_texts.append(content)
    if not isinstance(content, list):
        continue
    for part in content:
        if not isinstance(part, dict):
            continue
        if part.get("type") == "tool_use":
            uses[part["id"]] = {"name": part.get("name"), "input": part.get("input") or {}}
            order.append(part["id"])
        elif part.get("type") == "tool_result":
            c = part.get("content")
            results[part.get("tool_use_id")] = c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))
        elif part.get("type") == "text":
            if o.get("type") == "assistant":
                assistant_texts.append(part.get("text", ""))
            elif o.get("type") == "user":
                user_texts.append(part.get("text", ""))

violations = []

# ------------------------------------------------------------------ prompt check
first_user = user_texts[0] if user_texts else ""
prompt_check = {"FirstUserTurnSha256": hashlib.sha256(first_user.encode("utf-8")).hexdigest() if first_user else None,
                "ExpectedPromptSha256": PROMPT_SHA, "HashVerifiedByReviewer": False, "PromptReadFaithful": False}

# ------------------------------------------------------------------ read audit and fidelity
ALLOWED_BASH = [
    (r"^git -C [\"']?D:[/\\]r62-arch-a1[\"']? (rev-parse|status --porcelain|log --oneline -10)\b", "git identity"),
    (r"^git (rev-parse|status --porcelain|log --oneline -10)\b", "git identity (cwd)"),
    (r"^git (-C [\"']?D:[/\\]r62-arch-a1[\"']? )?show " + REV[:7] + r"[0-9a-f]*:(\S+)( \| sed -n ['\"]?(\d+)(,(\d+))?p['\"]?)?\s*$", "git show"),
    (r"^sha256sum [\"']?D:[/\\]r62-arch-a1-run[/\\]prompt\.md[\"']?\s*$", "prompt hash"),
    (r"^python3? [\"']?D:[/\\]r62-arch-a1[/\\]docs[/\\]automation[/\\]evidence[/\\]I-62-A1[/\\]a1-counterexamples\.py[\"']? \S+\s*$", "counterexamples"),
    (r"dotnet(\.exe)?[\"']? test tests[/\\]RackCad\.Tests[/\\]RackCad\.Tests\.csproj", "dotnet test (disposable clone)"),
]
audit, fidelity, delivered = [], [], {}


def compare(rel_or_run, kind, numbered_lines):
    lines = canon(rel_or_run) if kind == "CLOSURE" else run_file(rel_or_run)
    bad = []
    for n, body in numbered_lines:
        if 1 <= n <= len(lines) and lines[n - 1] == body:
            delivered.setdefault(rel_or_run, set()).add(n)
        else:
            bad.append(n)
    return bad


for uid in order:
    u = uses[uid]
    name, inp, out = u["name"], u["input"], results.get(uid)
    row = {"Tool": name, "Input": {k: (v if len(str(v)) < 300 else str(v)[:300] + "…") for k, v in inp.items()}, "Class": None, "Violation": None}
    if name == "Read":
        kind, ref = classify_path(inp.get("file_path", ""))
        row["Class"] = kind
        if kind == "OUTSIDE":
            row["Violation"] = "lectura fuera del cierre"
        elif out is not None:
            pairs = []
            for raw in out.replace("\r\n", "\n").split("\n"):
                m = re.match(r"^\s*(\d+)\t(.*)$", raw)
                if m:
                    pairs.append((int(m.group(1)), m.group(2)))
            bad = compare(ref, kind, pairs)
            fidelity.append({"Path": ref, "Via": "Read", "Lines": len(pairs), "Altered": bad[:50], "Status": "FAITHFUL_NORMALIZED" if pairs and not bad else ("DEGRADED" if bad else "EMPTY")})
            if ref == "prompt.md" and pairs and not bad:
                prompt_check["PromptReadFaithful"] = True
    elif name == "Bash":
        cmd = inp.get("command", "").strip()
        hit = next((label for rx, label in ALLOWED_BASH if re.search(rx, cmd)), None)
        row["Class"] = hit or "UNLISTED"
        if hit is None:
            row["Violation"] = "acción no permitida"
        if hit == "prompt hash" and out and PROMPT_SHA in out:
            prompt_check["HashVerifiedByReviewer"] = True
        if hit == "git show":
            m = re.search(r"show " + REV[:7] + r"[0-9a-f]*:(\S+)( \| sed -n ['\"]?(\d+)(,(\d+))?p['\"]?)?", cmd)
            rel = m.group(1)
            if rel not in ALLOWED:
                row["Violation"] = "git show fuera del cierre"
            elif out is not None and m.group(3):
                a = int(m.group(3)); b = int(m.group(5) or a)
                got = out.replace("\r\n", "\n").rstrip("\n").split("\n")
                bad = compare(rel, "CLOSURE", list(zip(range(a, b + 1), got)))
                fidelity.append({"Path": rel, "Via": "git show | sed", "Lines": len(got), "Altered": bad[:50],
                                 "Status": "FAITHFUL_NORMALIZED" if not bad and len(got) == b - a + 1 else "DEGRADED"})
    elif name == "Grep":
        p = inp.get("path", "")
        kind, ref = classify_path(p) if p else ("OUTSIDE", "(cwd)")
        row["Class"] = kind
        if kind == "OUTSIDE":
            row["Violation"] = "búsqueda fuera del cierre (ruta no listada o directorio entero)"
    elif name in ("TodoWrite", "TaskCreate", "TaskUpdate"):
        row["Class"] = "BOOKKEEPING"
    else:
        row["Class"] = "FORBIDDEN_TOOL"
        row["Violation"] = "herramienta no permitida (" + str(name) + ")"
    if out and re.search(r"(?i)truncat|output too large|exceeds maximum", out):
        row["TruncationMarker"] = True
    audit.append(row)
    if row["Violation"]:
        violations.append("%s: %s" % (row["Violation"], json.dumps(row["Input"], ensure_ascii=False)[:200]))

for f in fidelity:
    if f["Status"] == "DEGRADED":
        violations.append("fidelidad: %s (%s) líneas alteradas %s" % (f["Path"], f["Via"], f["Altered"][:10]))
if not prompt_check["HashVerifiedByReviewer"]:
    violations.append("el revisor no comprobó el SHA-256 de prompt.md")
if not prompt_check["PromptReadFaithful"]:
    violations.append("prompt.md no se leyó de forma fiel")

# ------------------------------------------------------------------ result
final = assistant_texts[-1] if assistant_texts else ""
blocks = re.findall(r"```json\s*\n(.*?)\n```", final, re.S)
result, result_check = None, {"Found": bool(blocks), "SchemaValid": False, "Coherence": []}
if blocks:
    try:
        result = json.loads(blocks[-1])
    except ValueError as e:
        result_check["Coherence"].append("JSON inválido: %s" % e)
if result is not None:
    tmp = tempfile.mkdtemp()
    rp = os.path.join(tmp, "result.json")
    json.dump(result, open(rp, "w", encoding="utf-8"), ensure_ascii=False)
    ps = ("try { $null = Get-Content -Raw -LiteralPath '%s' | Test-Json -SchemaFile '%s' -ErrorAction Stop; 'True' } catch { 'False: ' + $_.Exception.Message }"
          % (rp, SCHEMA))
    o = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout.strip()
    result_check["SchemaValid"] = o == "True"
    result_check["SchemaDetail"] = o[:300]
    c = result_check["Coherence"]
    v = result.get("Verdict")
    req = result.get("RequiredFindings") or []
    ia = result.get("IfAgreed") or {}
    if v == "AGREED" and (req or not all(ia.get(k) is True for k in ia)):
        c.append("AGREED exige cero REQUIRED y IfAgreed con los cinco campos en true")
    if v != "AGREED" and any(x is not None for x in ia.values()):
        c.append("IfAgreed debe ser null salvo con AGREED")
    if v == "CHANGES REQUIRED" and not req:
        c.append("CHANGES REQUIRED sin ningún REQUIRED")
    if v == "BLOCKED — OWNER DECISION" and not (result.get("OwnerAuthorityCheck") or {}).get("OwnerDecisionRequired"):
        c.append("BLOCKED — OWNER DECISION sin OwnerDecisionRequired")
    if result.get("ReviewedCommit") != REV or result.get("ReviewedBlob") != next(r["Blob"] for r in CLOSURE["CanonicalInputs"] if r["Path"] == "docs/initiatives/I-62-A-1.md"):
        c.append("objeto revisado distinto del designado")
    for key in ("FC01", "FC02"):
        items = sorted(x.get("Item") for x in result.get(key) or [])
        if items != list(range(1, 11)):
            c.append("%s debe contestar los puntos 1-10 una vez cada uno" % key)
    for x in c:
        violations.append("resultado: " + x)
    if not result_check["SchemaValid"]:
        violations.append("resultado: no valida contra result.schema.json")
else:
    violations.append("no hay bloque JSON final")

# ------------------------------------------------------------------ premises
premise_check = []
for f in (result or {}).get("RequiredFindings", []) + (result or {}).get("OptionalFindings", []):
    for pr in f.get("PremiseRefs") or []:
        rel = pr.get("Path", "").replace("\\", "/")
        rel = re.sub(r"^.*?r62-arch-a1/", "", rel)
        ok = False
        if rel in ALLOWED:
            seg = " ".join(canon(rel)[max(0, pr.get("LineStart", 1) - 1):pr.get("LineEnd", 0)])
            ok = " ".join(pr.get("Quote", "").split()) in " ".join(seg.split())
        premise_check.append({"Finding": f.get("FindingId"), "Path": rel, "Lines": [pr.get("LineStart"), pr.get("LineEnd")], "QuoteFound": ok})
        if not ok:
            violations.append("premisa no canónica en %s (%s %s-%s)" % (f.get("FindingId"), rel, pr.get("LineStart"), pr.get("LineEnd")))

out = {
    "Transcript": {"Path": TRANSCRIPT, "Sha256": hashlib.sha256(open(TRANSCRIPT, "rb").read()).hexdigest(), "Entries": len(entries)},
    "RuntimeIdentity": {k: (sorted(v) if isinstance(v, set) else v) for k, v in identity.items()},
    "PromptCheck": prompt_check,
    "ReadAudit": {"Calls": len(audit), "Violations": [a for a in audit if a["Violation"]], "Rows": audit},
    "Fidelity": {"Records": fidelity, "DeliveredLinesByPath": {k: len(v) for k, v in sorted(delivered.items())}},
    "Result": {"Check": result_check, "Literal": result},
    "PremiseCheck": premise_check,
    "Accreditation": {"Status": "ACCREDITED" if not violations else "NOT_ACCREDITED", "Reasons": violations},
}
with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
    json.dump(out, fh, ensure_ascii=False, indent=1)
    fh.write("\n")
print(json.dumps({"Accreditation": out["Accreditation"]["Status"], "Violations": len(violations), "Calls": len(audit),
                  "Verdict": (result or {}).get("Verdict")}, ensure_ascii=True))
