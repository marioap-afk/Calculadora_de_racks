"""I-62 A-1 (corrected for A62-A1R-01..03, kit v3): post-review audit v3 of the formal Architect session (run by the invoking session afterwards).

v3 = v2.1 (custodied with R20261005T044948Z-ac67) plus rules declared BEFORE the run (night order §6: «comprueba rutas y efectos, no solo igualdad
literal de cadenas de comandos»): python code (heredoc, -c or an own script written in the transcript) is checked by its path literals and
forbidden effects; the harness output must be an own output; dotnet path arguments must be own outputs; the clone must stay clean after the run;
calls without a result and side chains fail termination; U+FFFD and truncation markers are recorded; a KNOWN long line (closure LongLines) that
Read delivers as a prefix is TRUNCATED_KNOWN (not delivered, not a violation by itself); prior-authorization.txt is a run file whose hash and
complete reading are checked; the result coherence covers 10 dispositions, 10 optional corrections and focus 1-16.

Reads the reviewer's Claude Code transcript (JSONL) and decides accreditation mechanically, without trusting what the reviewer declares:
  1. RuntimeIdentity  — session ids, cwds, runtime versions, models of the assistant messages, side chains (observed);
  2. Identity order   — the clone HEAD and the reviewer's own worktree HEAD are verified (output contains the commit) before any substantive read;
                        the order and prompt hashes are checked and both files are read faithfully;
  3. ReadAudit        — every tool call against the closed action contract of closure.json:
                          Read: closure files, order/prompt, own outputs; Grep: path = one closure FILE, no glob;
                          Bash: each segment of a compound command (|, ;, &&, ||, newline) must start with an allowed command, and every path token must be a
                          closure file, order/prompt, the clone/worktree root (only as git -C or cd target) or an own output; git sub-commands and
                          `git show <rev>:<path>` paths are checked; grep -r/-R is forbidden; redirections only to own outputs;
                          PowerShell: only dotnet test inside the clone and reads of own outputs; Write: own outputs only; bookkeeping tools allowed;
  4. Fidelity         — every Read result and every pure `git show <rev>:<path> | sed -n 'A,Bp'` output compared line by line with the canonical bytes
                        (end of line is the only normalization); Grep content lines are compared as information;
  5. Result           — last ```json block of the final assistant message, validated with pwsh Test-Json against result.schema.json and with the
                        coherence rules of the order (dispositions, focus 1-12, AGREED / IfAgreed, identity of the object);
  6. PremiseCheck     — every PremiseRefs quote occurs inside its lines at the commit AND all those lines were delivered faithfully to the reviewer;
  7. Accreditation    — ACCREDITED only if 2-6 raise no violation.

Usage: python post-review.py <transcript.jsonl> <clone> <run dir> <result.schema.json> <out.json>
"""
import hashlib
import json
import os
import re
import shlex
import subprocess
import sys
import tempfile

TRANSCRIPT, CLONE, RUN, SCHEMA, OUT = sys.argv[1:6]
CLOSURE = json.load(open(os.path.join(RUN, "closure.json"), encoding="utf-8"))
REV = CLOSURE["AuthorityRevision"]
IDS = CLOSURE["Ids"]
ALLOWED = {r["Path"] for r in CLOSURE["CanonicalInputs"] + CLOSURE["AllowedTransitiveInputs"]}
ALLOWED_LC = {p.lower(): p for p in ALLOWED}
RUN_FILES = {"prompt.md", "order.txt", "prior-authorization.txt"}
PROMPT_SHA = hashlib.sha256(open(os.path.join(RUN, "prompt.md"), "rb").read()).hexdigest()
ORDER_SHA = hashlib.sha256(open(os.path.join(RUN, "order.txt"), "rb").read()).hexdigest()
PRIOR_SHA = hashlib.sha256(open(os.path.join(RUN, "prior-authorization.txt"), "rb").read()).hexdigest()
LONG_LINES = {p: set(v) for p, v in CLOSURE["ReadPolicy"]["LongLines"].items()}
HARNESS = "docs/automation/evidence/I-62-A1/a1-counterexamples.py"
OBJECT_BLOB = next(r["Blob"] for r in CLOSURE["CanonicalInputs"] if r["Path"] == "docs/initiatives/I-62-A-1.md")


def norm(p):
    p = p.strip().strip('"').strip("'").replace("\\", "/")
    m = re.match(r"^/([a-zA-Z])/(.*)$", p)
    if m:
        p = m.group(1) + ":/" + m.group(2)
    return re.sub(r"/+", "/", p).rstrip("/")


CLONE_N, RUN_N = norm(CLONE).lower(), norm(RUN).lower()
WT_ROOT = "d:/documentos/worktrees/" + os.path.basename(CLONE_N) + "/"
OWN_RX = re.compile(r"/appdata/local/temp/claude/d--documentos-worktrees-" + re.escape(os.path.basename(CLONE_N)) + r"-[^/]+/")


def classify(p, cwd=None):
    """→ (kind, ref): CLOSURE rel | RUN name | OWN path | ROOT path | NULLDEV | OUTSIDE path."""
    raw = norm(p)
    if raw in ("/dev/null", "nul", "$null"):
        return ("NULLDEV", raw)
    if not re.match(r"^[a-zA-Z]:/", raw) and not raw.startswith("/") and not raw.startswith("~") and cwd:
        raw = norm(cwd) + "/" + raw
    raw = re.sub(r"/\./", "/", raw)
    n = raw.lower()
    if n.startswith(RUN_N + "/"):
        name = n[len(RUN_N) + 1:]
        return ("RUN", name) if name in RUN_FILES else ("OUTSIDE", raw)
    if OWN_RX.search(n):
        return ("OWN", raw)
    roots = [CLONE_N]
    if n.startswith(WT_ROOT) and n[len(WT_ROOT):]:
        roots.append(WT_ROOT + n[len(WT_ROOT):].split("/", 1)[0])
    for root in roots:
        if n == root:
            return ("ROOT", raw)
        if n.startswith(root + "/"):
            rel = n[len(root) + 1:]
            return ("CLOSURE", ALLOWED_LC[rel]) if rel in ALLOWED_LC else ("OUTSIDE", raw)
    return ("OUTSIDE", raw)


canon_cache = {}


def canon(rel):
    if rel not in canon_cache:
        canon_cache[rel] = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")
    return canon_cache[rel]


def run_file(name):
    return open(os.path.join(RUN, name), encoding="utf-8").read().replace("\r\n", "\n").split("\n")


# ------------------------------------------------------------------ transcript
entries = []
for line in open(TRANSCRIPT, encoding="utf-8"):
    try:
        entries.append(json.loads(line))
    except ValueError:
        pass
identity = {"SessionIds": set(), "Cwds": set(), "Versions": set(), "Models": {}, "SidechainEntries": 0}
uses, results, cwd_of, user_texts, assistant_texts, order = {}, {}, {}, [], [], []
for o in entries:
    for k, key in (("SessionIds", "sessionId"), ("Cwds", "cwd"), ("Versions", "version")):
        if o.get(key):
            identity[k].add(o[key])
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
            cwd_of[part["id"]] = o.get("cwd")
            order.append(part["id"])
        elif part.get("type") == "tool_result":
            c = part.get("content")
            results[part.get("tool_use_id")] = c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))
        elif part.get("type") == "text":
            (assistant_texts if o.get("type") == "assistant" else user_texts).append(part.get("text", ""))

violations = []
checks = {"PromptHashSeen": False, "OrderHashSeen": False, "PriorHashSeen": False, "PromptReadFaithful": False, "OrderReadComplete": False,
          "PriorReadComplete": False,
          "CloneHeadVerifiedBeforeReading": False, "WorktreeHeadVerifiedBeforeReading": False, "FirstSubstantiveCall": None}

# ------------------------------------------------------------------ command audit helpers
ALLOWED_WORDS = {"git", "sed", "wc", "awk", "grep", "head", "tail", "cat", "sha256sum", "echo", "printf", "python", "python3", "cd", "dotnet", "true"}
CONTROL = {"for", "do", "done", "while", "if", "then", "fi", "else", "elif"}
PATHLIKE = re.compile(r"^([a-zA-Z]:[\\/]|/[a-zA-Z]/|\.{1,2}/|~/|docs/|tests/|src/|plugin/|AGENTS\.md$|CLAUDE\.md$|README\.md$)|\.(md|json|py|yml|yaml|cs|csproj|txt|log|jsonl)$")
DRIVE_IN_CODE = re.compile(r"[a-zA-Z]:[\\/][^'\"\s,);|&]+")


def split_segments(cmd):
    segs, cur, q, i = [], "", None, 0
    while i < len(cmd):
        ch = cmd[i]
        if q:
            cur += ch
            if ch == q:
                q = None
        elif ch in "'\"":
            q, cur = ch, cur + ch
        elif cmd.startswith("&&", i) or cmd.startswith("||", i):
            segs.append(cur); cur = ""; i += 1
        elif ch in "|;\n":
            segs.append(cur); cur = ""
        else:
            cur += ch
        i += 1
    segs.append(cur)
    return [s.strip() for s in segs if s.strip()]


def tokens_of(seg):
    try:
        toks = shlex.split(seg, posix=False)
    except ValueError:
        toks = seg.split()
    return [t[1:-1] if len(t) >= 2 and t[0] == t[-1] and t[0] in "'\"" else t for t in toks]


TEST_PROJECT = "tests/rackcad.tests/rackcad.tests.csproj"


def is_test_project(c, cwd):
    n = norm(c).lower()
    if not re.match(r"^[a-z]:/", n) and cwd:
        n = norm(cwd).lower() + "/" + n
    return n == CLONE_N + "/" + TEST_PROJECT or (n.startswith(WT_ROOT) and n.endswith("/" + TEST_PROJECT))


PY_FORBIDDEN = re.compile(r"\bos\.(walk|listdir|scandir|system|popen|remove|unlink|rmdir|rename|replace|makedirs|mkdir|chdir)\b|\bglob\b|\.rglob\(|\.iterdir\(|"
                          r"\bsubprocess\b|\burllib\b|\brequests\b|\bsocket\b|\bhttp\.client\b|\bshutil\b|\bctypes\b|\b__import__\b|\bexec\(|\beval\(")
PY_LITERAL = re.compile(r"(?<![A-Za-z0-9_])[rbuRBU]{0,2}('''|\"\"\"|'|\")(.*?)\1", re.S)
own_scripts = {}


def audit_python_code(code, here):
    """Path literals and effects of python code (heredoc body, -c argument or an own script written in the transcript)."""
    probs = []
    for m in PY_FORBIDDEN.finditer(code):
        probs.append("python con un efecto no verificable (%s)" % m.group(0))
    for m in PY_LITERAL.finditer(code):
        lit = m.group(2)
        if "\n" in lit or not (DRIVE_IN_CODE.match(lit) or (PATHLIKE.search(lit) and "/" in lit)):
            continue
        k = classify(lit, here)[0]
        if k == "ROOT":
            probs.append("python: directorio como argumento: %s" % lit)
        elif k not in ("CLOSURE", "RUN", "OWN", "NULLDEV"):
            probs.append("python: ruta fuera del cierre: %s" % lit)
    return probs


HEREDOC = re.compile(r"<<-?\s*(['\"]?)([A-Za-z_][A-Za-z0-9_]*)\1[^\n]*\n(.*?)\n\2[ \t]*(?=\n|$)", re.S)


def audit_bash(cmd, cwd):
    """→ list of problems (empty = allowed)."""
    probs, here, bodies = [], cwd, {}

    def _keep(m):
        key = "__HEREDOC_%d__" % len(bodies)
        bodies[key] = m.group(3)
        return cmd[m.start():m.start(3)].split("\n")[0] + " " + key
    cmd = HEREDOC.sub(_keep, cmd)
    env = {}
    for seg in split_segments(cmd):
        for name, val in re.findall(r"^([A-Za-z_][A-Za-z0-9_]*)=(\"[^\"]*\"|'[^']*'|\S+)\s*$", seg):
            env[name] = val.strip("\"'")
    for name, val in env.items():
        cmd = cmd.replace("${%s}" % name, val).replace("$" + name, val)
    for seg in split_segments(cmd):
        toks = tokens_of(seg)
        while toks and (toks[0] in CONTROL or re.match(r"^[A-Za-z_][A-Za-z0-9_]*=", toks[0])):
            toks = toks[1:]
        if not toks:
            continue
        if toks[0] == "for":
            continue
        word = toks[0].split("/")[-1].split("\\")[-1].lower().replace(".exe", "")
        if word not in ALLOWED_WORDS:
            probs.append("comando no permitido: %s" % toks[0])
            continue
        seg_bodies = [bodies[t] for t in toks if t in bodies]
        toks = [t for t in toks if t not in bodies and not t.startswith("<<")]
        for body in seg_bodies:
            if word in ("python", "python3"):
                probs += audit_python_code(body, here)
            else:
                for c in DRIVE_IN_CODE.findall(body):
                    if classify(c, here)[0] not in ("CLOSURE", "RUN", "OWN"):
                        probs.append("ruta fuera del cierre en un heredoc: %s" % c)
        rest, redirect_targets, i = toks[1:], [], 0
        clean = []
        while i < len(rest):
            t = rest[i]
            m = re.match(r"^(\d?|&|\*)?>{1,2}(.*)$", t)
            if m and not t.startswith(">&") and t not in ("2>&1",):
                target = m.group(2) or (rest[i + 1] if i + 1 < len(rest) else "")
                if not m.group(2):
                    i += 1
                redirect_targets.append(target)
            elif t != "2>&1":
                clean.append(t)
            i += 1
        for tgt in redirect_targets:
            if classify(tgt, here)[0] not in ("OWN", "NULLDEV"):
                probs.append("redirección fuera de las salidas propias: %s" % tgt)
            elif seg_bodies and classify(tgt, here)[0] == "OWN":
                own_scripts[norm(classify(tgt, here)[1]).lower()] = seg_bodies[-1]
        if word == "cd":
            if clean:
                k = classify(clean[0], here)[0]
                if k not in ("ROOT", "OWN"):
                    probs.append("cd fuera del clon, del worktree o de las salidas propias: %s" % clean[0])
                here = norm(clean[0]) if re.match(r"^[a-zA-Z]:[\\/]|^/[a-zA-Z]/", clean[0]) else norm((here or "") + "/" + clean[0])
            continue
        if word == "git":
            j = 0
            while j < len(clean) and clean[j].startswith("-"):
                if clean[j] in ("-C", "-c"):
                    if clean[j] == "-C" and j + 1 < len(clean) and classify(clean[j + 1], here)[0] != "ROOT":
                        probs.append("git -C fuera del clon o del worktree: %s" % clean[j + 1])
                    j += 2
                else:
                    j += 1
            sub = clean[j] if j < len(clean) else ""
            args = clean[j + 1:]
            if sub == "rev-parse" or sub == "cat-file":
                for a in args:
                    if ":" in a and not re.match(r"^[a-zA-Z]:[\\/]", a):
                        path = a.split(":", 1)[1]
                        if path and path.lower() not in ALLOWED_LC:
                            probs.append("git %s de una ruta fuera del cierre: %s" % (sub, path))
            elif sub == "show":
                spec = [a for a in args if not a.startswith("-")]
                if not spec or any(":" not in a for a in spec):
                    probs.append("git show sin <rev>:<ruta del cierre>")
                for a in spec:
                    if ":" in a:
                        rev, path = a.split(":", 1)
                        if not (REV.startswith(rev.lower()) and len(rev) >= 7 or rev in ("HEAD",)) or path.lower() not in ALLOWED_LC:
                            probs.append("git show fuera del cierre: %s" % a)
            elif sub == "checkout":
                if args[:1] != ["--detach"] or len(args) != 2 or not REV.startswith(args[1].lower()) or len(args[1]) < 7:
                    probs.append("git checkout distinto de --detach <commit>")
            elif sub == "log":
                if any(a for a in args if not re.match(r"^(--oneline|-\d+|-n|\d+)$", a)):
                    probs.append("git log con argumentos no permitidos")
            elif sub == "worktree":
                if args[:1] != ["list"]:
                    probs.append("git worktree distinto de list")
            elif sub == "status":
                pass
            elif sub == "config":
                if args[:1] != ["--get"]:
                    probs.append("git config distinto de --get")
            else:
                probs.append("subcomando git no permitido: %s" % sub)
            continue
        if word == "grep" and any(re.match(r"^-[a-zA-Z]*[rR]", t) or t == "--recursive" for t in clean):
            probs.append("grep recursivo")
        if word == "sed" and "-n" not in clean and not any(re.match(r"^-[a-zA-Z]*n", t) for t in clean):
            probs.append("sed sin -n")
        if word == "dotnet":
            if not (clean[:1] == ["test"] and any(is_test_project(t, here) for t in clean)):
                probs.append("dotnet distinto de test del proyecto Core del clon")
            for t in clean[1:]:
                for c in DRIVE_IN_CODE.findall(t) or ([t] if PATHLIKE.search(t) and not t.startswith("-") else []):
                    if not is_test_project(c, here) and classify(c, here)[0] not in ("OWN", "NULLDEV"):
                        probs.append("dotnet: ruta distinta del proyecto y de las salidas propias: %s" % c)
            continue
        if word in ("python", "python3"):
            if "-c" in clean:
                i_c = clean.index("-c")
                probs += audit_python_code(clean[i_c + 1] if i_c + 1 < len(clean) else "", here)
                continue
            script = next((t for t in clean if not t.startswith("-")), None)
            if script is None or script == "-":
                if not seg_bodies:
                    probs.append("python sin código auditable")
                continue
            kind, ref = classify(script, here)
            rest_args = [t for t in clean[clean.index(script) + 1:] if not t.startswith("-")]
            if kind == "CLOSURE" and ref == HARNESS:
                if len(rest_args) != 1 or classify(rest_args[0], here)[0] != "OWN":
                    probs.append("el arnés exige exactamente una salida propia como argumento")
                continue
            if kind == "OWN":
                body = own_scripts.get(norm(ref).lower())
                if body is None:
                    probs.append("script propio sin contenido auditable en la transcripción: %s" % script)
                else:
                    probs += audit_python_code(body, here)
                for a in rest_args:
                    if classify(a, here)[0] not in ("CLOSURE", "RUN", "OWN"):
                        probs.append("ruta fuera del cierre: %s" % a)
                continue
            probs.append("python de un script distinto del arnés y de los scripts propios: %s" % script)
            continue
        for t in clean:
            if t.startswith("-"):
                continue
            candidates = DRIVE_IN_CODE.findall(t) if word in ("python", "python3") and " " in t else ([t] if PATHLIKE.search(t) else [])
            for c in candidates:
                k = classify(c, here)[0]
                if k == "ROOT":
                    probs.append("directorio como argumento: %s" % c)
                elif k not in ("CLOSURE", "RUN", "OWN", "NULLDEV"):
                    probs.append("ruta fuera del cierre: %s" % c)
    return probs


def audit_powershell(cmd, cwd):
    low = cmd.lower()
    probs = []
    if "dotnet" in low and " test " in " " + low.replace("&", " ") + " " and "rackcad.tests.csproj" in low:
        for c in DRIVE_IN_CODE.findall(cmd):
            k = classify(c, cwd)[0]
            if k not in ("ROOT", "CLOSURE", "OWN") and "microsoft/dotnet/dotnet.exe" not in norm(c).lower() and not is_test_project(c, cwd):
                probs.append("ruta fuera del clon o de las salidas propias: %s" % c)
        return probs
    if re.match(r"^\s*get-content\b", low):
        for c in DRIVE_IN_CODE.findall(cmd):
            if classify(c, cwd)[0] != "OWN":
                probs.append("Get-Content fuera de las salidas propias: %s" % c)
        return probs
    return ["PowerShell no permitido: %s" % cmd[:120]]


# ------------------------------------------------------------------ audit loop
audit, fidelity, delivered, grep_info, unicode_anomalies, truncated_known = [], [], {}, [], [], []


def compare(ref, kind, numbered, via="Read"):
    lines = canon(ref) if kind == "CLOSURE" else run_file(ref)
    bad = []
    for n, body in numbered:
        if 1 <= n <= len(lines) and lines[n - 1] == body:
            delivered.setdefault(ref, set()).add(n)
        elif via == "Read" and kind == "CLOSURE" and n in LONG_LINES.get(ref, set()) and len(body) >= 1000 and \
                lines[n - 1].startswith(body.rstrip().rstrip("…").rstrip(".").rstrip()):
            truncated_known.append({"Path": ref, "Line": n, "DeliveredChars": len(body), "CanonicalChars": len(lines[n - 1])})
        else:
            bad.append(n)
    return bad


def substantive(name, inp):
    if name == "Read":
        return classify(inp.get("file_path", ""))[0] == "CLOSURE"
    if name == "Grep":
        return True
    if name == "Bash":
        return bool(re.search(r"\bshow\s+\S+:", inp.get("command", ""))) or bool(re.search(r"\b(sed|cat|grep|awk|head|tail|python3?)\b.*r62-arch-a1r3[/\\]", inp.get("command", "")))
    return False


for idx, uid in enumerate(order):
    u = uses[uid]
    name, inp, out, cwd = u["name"], u["input"], results.get(uid), cwd_of.get(uid)
    row = {"Index": idx, "Tool": name, "Input": {k: (v if len(str(v)) < 400 else str(v)[:400] + "…") for k, v in inp.items()}, "Class": None, "Problems": []}
    if checks["FirstSubstantiveCall"] is None and substantive(name, inp):
        checks["FirstSubstantiveCall"] = idx
    if name == "Read":
        kind, ref = classify(inp.get("file_path", ""), cwd)
        row["Class"] = kind
        if kind not in ("CLOSURE", "RUN", "OWN"):
            row["Problems"].append("lectura fuera del cierre")
        elif kind != "OWN" and out is not None:
            pairs = [(int(m.group(1)), m.group(2)) for m in (re.match(r"^\s*(\d+)\t(.*)$", raw) for raw in out.replace("\r\n", "\n").split("\n")) if m]
            bad = compare(ref, kind, pairs)
            fidelity.append({"Path": ref, "Via": "Read", "Lines": len(pairs), "Altered": bad[:50],
                             "Status": "FAITHFUL_NORMALIZED" if pairs and not bad else ("DEGRADED" if bad else "EMPTY")})
            if ref == "prompt.md" and pairs and not bad:
                checks["PromptReadFaithful"] = True
    elif name == "Grep":
        p = inp.get("path", "")
        kind, ref = classify(p, cwd) if p else ("OUTSIDE", "(sin path: directorio de trabajo)")
        row["Class"] = kind
        if kind not in ("CLOSURE", "RUN") or inp.get("glob"):
            row["Problems"].append("búsqueda fuera de un archivo del cierre (directorio, glob o ruta no listada)")
        elif out and kind == "CLOSURE":
            for raw in out.replace("\r\n", "\n").split("\n"):
                m = re.match(r"^(?:.*?:)?(\d+)[:-](.*)$", raw)
                if m:
                    n, body = int(m.group(1)), m.group(2)
                    lines = canon(ref)
                    grep_info.append({"Path": ref, "Line": n, "Exact": 1 <= n <= len(lines) and lines[n - 1] == body})
    elif name == "Bash":
        cmd = inp.get("command", "")
        row["Class"] = "BASH"
        row["Problems"] += audit_bash(cmd, cwd)
        if out:
            if PROMPT_SHA in out:
                checks["PromptHashSeen"] = True
            if ORDER_SHA in out:
                checks["OrderHashSeen"] = True
            if PRIOR_SHA in out:
                checks["PriorHashSeen"] = True
            first = checks["FirstSubstantiveCall"]
            if (first is None or idx < first) and REV in out:
                if re.search(r"git\s+-C\s+[\"']?[A-Za-z]:[\\/]r62-arch-a1r3[\"']?\s+rev-parse\s+HEAD\b", cmd) or \
                        re.search(r"cd\s+[\"']?[A-Za-z]:[\\/]r62-arch-a1r3[\"']?\s*(&&|;)\s*git\s+rev-parse\s+HEAD\b", cmd):
                    checks["CloneHeadVerifiedBeforeReading"] = True
                if (re.search(r"(^|[;&|]\s*)git\s+rev-parse\s+HEAD\b", cmd) and cwd and norm(cwd).lower().startswith(WT_ROOT)) or \
                        re.search(r"git\s+-C\s+[\"']?[A-Za-z]:[\\/]Documentos[\\/]Worktrees[\\/]r62-arch-a1r3[\\/][^\s\"']+[\"']?\s+rev-parse\s+HEAD\b", cmd, re.I):
                    checks["WorktreeHeadVerifiedBeforeReading"] = True
        m = re.match(r"^\s*git\s+-C\s+\S+\s+show\s+([0-9a-fA-F]{7,40}|HEAD):(\S+)\s*\|\s*sed\s+-n\s+['\"]?(\d+)(?:,(\d+))?p['\"]?\s*$", cmd)
        if m and out is not None and m.group(2).lower() in ALLOWED_LC:
            rel = ALLOWED_LC[m.group(2).lower()]
            a, b = int(m.group(3)), int(m.group(4) or m.group(3))
            got = out.replace("\r\n", "\n").rstrip("\n").split("\n")
            bad = compare(rel, "CLOSURE", list(zip(range(a, b + 1), got)))
            fidelity.append({"Path": rel, "Via": "git show | sed", "Lines": len(got), "Altered": bad[:50],
                             "Status": "FAITHFUL_NORMALIZED" if not bad and len(got) == b - a + 1 else "DEGRADED"})
    elif name == "PowerShell":
        row["Class"] = "POWERSHELL"
        row["Problems"] += audit_powershell(inp.get("command", ""), cwd)
    elif name in ("Write",):
        kind, ref = classify(inp.get("file_path", ""), cwd)
        row["Class"] = kind
        if kind != "OWN":
            row["Problems"].append("escritura fuera de las salidas propias")
        else:
            own_scripts[norm(ref).lower()] = inp.get("content", "")
    elif name in ("TodoWrite", "TaskCreate", "TaskUpdate", "TaskList", "ToolSearch"):
        row["Class"] = "BOOKKEEPING"
    else:
        row["Class"] = "FORBIDDEN_TOOL"
        row["Problems"].append("herramienta no permitida (%s)" % name)
    if out and re.search(r"(?i)output (was )?truncated|output too large|exceeds maximum", out):
        row["TruncationMarker"] = True
    if out and "\ufffd" in out:
        row["ReplacementChars"] = out.count("\ufffd")
        unicode_anomalies.append({"Index": idx, "Tool": name, "Count": out.count("\ufffd")})
    if uid not in results:
        row["Problems"].append("llamada sin resultado (terminación incompleta)")
    audit.append(row)
    for p in row["Problems"]:
        violations.append("llamada %d (%s): %s" % (idx, name, p))

order_lines = run_file("order.txt")
checks["OrderReadComplete"] = all(n in delivered.get("order.txt", set()) for n in range(1, len(order_lines) + (0 if order_lines[-1] == "" else 1)))
prior_lines = run_file("prior-authorization.txt")
checks["PriorReadComplete"] = all(n in delivered.get("prior-authorization.txt", set()) for n in range(1, len(prior_lines) + (0 if prior_lines[-1] == "" else 1)))
clone_status = subprocess.run(["git", "-C", CLONE, "status", "--porcelain"], capture_output=True, text=True).stdout
clone_head = subprocess.run(["git", "-C", CLONE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
checks["CloneCleanAfterRun"] = clone_status.strip() == "" and clone_head == REV
checks["NoSidechain"] = identity["SidechainEntries"] == 0
checks["FinalAssistantText"] = bool(assistant_texts)
for f in fidelity:
    if f["Status"] == "DEGRADED":
        violations.append("fidelidad: %s (%s) líneas alteradas %s" % (f["Path"], f["Via"], f["Altered"][:10]))
for key, msg in (("PromptHashSeen", "no se comprobó el SHA-256 de prompt.md"), ("OrderHashSeen", "no se comprobó el SHA-256 de order.txt"),
                 ("PriorHashSeen", "no se comprobó el SHA-256 de prior-authorization.txt"),
                 ("PromptReadFaithful", "prompt.md no se leyó de forma fiel"), ("OrderReadComplete", "order.txt no se leyó entera de forma fiel"),
                 ("PriorReadComplete", "prior-authorization.txt no se leyó entera de forma fiel"),
                 ("CloneCleanAfterRun", "el clon no quedó limpio o cambió de HEAD"),
                 ("NoSidechain", "la transcripción tiene entradas de cadena lateral (subagentes)"),
                 ("FinalAssistantText", "la sesión terminó sin un mensaje final del asistente"),
                 ("CloneHeadVerifiedBeforeReading", "HEAD del clon no verificado antes de la primera lectura sustantiva"),
                 ("WorktreeHeadVerifiedBeforeReading", "HEAD del worktree propio no verificado antes de la primera lectura sustantiva")):
    if not checks[key]:
        violations.append(msg)

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
    ps = "try { $null = Get-Content -Raw -LiteralPath '%s' | Test-Json -SchemaFile '%s' -ErrorAction Stop; 'True' } catch { 'False: ' + $_.Exception.Message }" % (rp, SCHEMA)
    o = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout.strip()
    result_check["SchemaValid"] = o == "True"
    result_check["SchemaDetail"] = o[:400]
    c = result_check["Coherence"]
    v = result.get("Verdict")
    req = result.get("RequiredFindings") or []
    disp = {d.get("FindingId"): d.get("State") for d in result.get("FindingDispositions") or []}
    ia = result.get("IfAgreed") or {}
    expected = ["A62-A1-0%d" % i for i in range(1, 7)] + ["OBS-A1-01"] + ["A62-A1R-0%d" % i for i in range(1, 4)]
    if sorted(d.get("FindingId") for d in result.get("FindingDispositions") or []) != sorted(expected):
        c.append("FindingDispositions debe disponer A62-A1-01..06, OBS-A1-01 y A62-A1R-01..03 una vez cada uno")
    if sorted(x.get("Item") for x in result.get("Focus") or []) != list(range(1, 17)):
        c.append("Focus debe contestar los ítems 1-16 una vez cada uno")
    if sorted(x.get("Id") for x in result.get("OptionalCorrections") or []) != sorted(["A62-A1-O%d" % i for i in range(1, 6)] + ["A62-A1R-O%d" % i for i in range(1, 6)]):
        c.append("OptionalCorrections debe contestar A62-A1-O1..O5 y A62-A1R-O1..O5 una vez cada uno")
    if v == "AGREED" and (req or any(disp.get(k) != "CLOSED" for k in expected) or not all(ia.get(k) is True for k in ia)):
        c.append("AGREED exige cero REQUIRED, los diez hallazgos CLOSED e IfAgreed todo en true")
    if v != "AGREED" and any(x is not None for x in ia.values()):
        c.append("IfAgreed debe ir en null salvo con AGREED")
    if v == "CHANGES REQUIRED" and not req and all(disp.get(k) == "CLOSED" for k in expected):
        c.append("CHANGES REQUIRED sin ningún REQUIRED ni hallazgo STILL_OPEN")
    if v == "BLOCKED — OWNER DECISION" and not (result.get("OwnerAuthorityCheck") or {}).get("OwnerDecisionRequired"):
        c.append("BLOCKED — OWNER DECISION sin OwnerDecisionRequired")
    if v == "NOT_ACCREDITED" and not result.get("KnownLimitations"):
        c.append("NOT_ACCREDITED sin el motivo en KnownLimitations")
    if result.get("ReviewedCommit") != REV or result.get("ReviewedBlob") != OBJECT_BLOB:
        c.append("objeto revisado distinto del designado")
    if any(result.get(k) != IDS[k] for k in ("RunId", "InvocationId", "LogicalReviewRequestId")):
        c.append("identificadores de la invocación distintos de los del cierre")
    for x in c:
        violations.append("resultado: " + x)
    if not result_check["SchemaValid"]:
        violations.append("resultado: no valida contra result.schema.json")
else:
    violations.append("no hay bloque JSON final")

# ------------------------------------------------------------------ premises
premise_check = []
groups = [("RequiredFindings", "FindingId"), ("OptionalFindings", "FindingId"), ("FindingDispositions", "FindingId")]
for key, idk in groups:
    for f in (result or {}).get(key) or []:
        for pr in f.get("PremiseRefs") or []:
            kind, rel = classify(pr.get("Path", ""), CLONE)
            a, b = pr.get("LineStart", 1), pr.get("LineEnd", 0)
            found = delivered_ok = False
            if kind == "CLOSURE":
                seg = " ".join(canon(rel)[max(0, a - 1):b])
                found = " ".join(pr.get("Quote", "").split()) in " ".join(seg.split())
                delivered_ok = all(n in delivered.get(rel, set()) for n in range(a, b + 1))
            premise_check.append({"Group": key, "Finding": f.get(idk), "Path": rel, "Lines": [a, b], "QuoteFound": found, "DeliveredFaithfully": delivered_ok})
            if not found:
                violations.append("premisa no canónica en %s (%s %s-%s)" % (f.get(idk), rel, a, b))
            elif not delivered_ok:
                violations.append("premisa sobre líneas no entregadas fielmente en %s (%s %s-%s)" % (f.get(idk), rel, a, b))

out = {
    "Transcript": {"Path": TRANSCRIPT, "Sha256": hashlib.sha256(open(TRANSCRIPT, "rb").read()).hexdigest(), "Entries": len(entries)},
    "RuntimeIdentity": {k: (sorted(v) if isinstance(v, set) else v) for k, v in identity.items()},
    "AuditorVersion": "v3 (kit v3; rules declared before the run)",
    "IdentityAndOrderChecks": checks,
    "CloneAfterRun": {"Head": clone_head, "StatusPorcelain": clone_status},
    "ReadAudit": {"Calls": len(audit), "WithProblems": [a for a in audit if a["Problems"]], "Rows": audit},
    "Fidelity": {"Records": fidelity, "DeliveredLinesByPath": {k: len(v) for k, v in sorted(delivered.items())},
                 "TruncatedKnownLongLines": truncated_known, "UnicodeReplacementChars": unicode_anomalies,
                 "GrepContentLines": {"Total": len(grep_info), "Exact": sum(1 for g in grep_info if g["Exact"])}},
    "Result": {"Check": result_check, "Literal": result},
    "PremiseCheck": premise_check,
    "Accreditation": {"Status": "ACCREDITED" if not violations else "NOT_ACCREDITED", "Reasons": violations},
}
with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
    json.dump(out, fh, ensure_ascii=False, indent=1)
    fh.write("\n")
print(json.dumps({"Accreditation": out["Accreditation"]["Status"], "Violations": len(violations), "Calls": len(audit),
                  "Verdict": (result or {}).get("Verdict")}, ensure_ascii=True))
