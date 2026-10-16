"""Bounded read-only characterization of claude-cli 2.1.299 (decisiones §87, punto 2; procedure of decisiones §56, punto 7; Owner
CLAUDE-CLI-I62 = A, unchanged). Same probes as R20261008T183600Z-char (char.py there): identity, C1, C2; C3 runs from char2_c3.py.

  python -B char2.py identity          -> identity.json (path, SHA-256, Authenticode, --version, auth status loggedIn/authMethod/apiProvider)
  python -B char2.py C1                -> C1/ (one claude-cli process: read 12 lines + structured output)
  python -B char2.py C2 8              -> C2/ (one claude-cli process terminated by the invoker after 8 s)
  python -B char2.py analyze           -> analysis-C1.json, analysis-C2.json
No product writes, no fixture mutation, no git writes, no configuration, permission, sandbox or credential change; credentials are never read
(auth status is the CLI's own check and only three fields are kept). Output only under this directory.
"""
import datetime, glob, hashlib, json, os, re, subprocess, sys, uuid

HERE = os.path.dirname(os.path.abspath(__file__))
BIN = os.path.join(os.environ["APPDATA"], "Claude", "claude-code", "2.1.299", "f0584be0fbaf", "claude.exe")
BIN_SHA = "295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b"
CWD = "D:/r62-cli-char2"
CLONE_HEAD = "e8a3de0541af0470096943f72df38333cf048f17"
PROJECTS = os.path.join(os.path.expanduser("~"), ".claude", "projects")
now = lambda: datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")

SCHEMA = {"type": "object", "additionalProperties": False,
          "required": ["first_line", "lines_read", "declared_model", "tools_available", "files_read"],
          "properties": {"first_line": {"type": "string"}, "lines_read": {"type": "integer"}, "declared_model": {"type": "string"},
                         "tools_available": {"type": "array", "items": {"type": "string"}},
                         "files_read": {"type": "array", "items": {"type": "string"}}}}
BASE_FLAGS = ["-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose",
              "--safe-mode", "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk",
              "--permission-prompts", "none"]
PROMPTS = {  # byte for byte the prompts of char.py (R20261008T183600Z-char)
    "C1": ("Eres una sonda de caracterización de solo lectura. Usa solo la herramienta Read para leer el archivo docs/initiatives/I-62-A-1.md, "
           "líneas 1 a 12 (offset 1, limit 12). No leas ningún otro archivo. Devuelve el objeto JSON exigido: first_line = el texto exacto de la "
           "línea 1; lines_read = el número de líneas que leíste; declared_model = el identificador de modelo que crees ser; tools_available = "
           "los nombres de las herramientas que tienes disponibles; files_read = las rutas que leíste."),
    "C2": ("Eres una sonda de caracterización de solo lectura. Usa solo la herramienta Read para leer, uno tras otro y enteros, los archivos "
           "docs/initiatives/I-62-proposal-v14.md (en tramos de 400 líneas) y docs/initiatives/I-62-A-1.md, y después resume cada sección en "
           "detalle. Devuelve el objeto JSON exigido."),
}


def sha(b):
    return hashlib.sha256(b).hexdigest()


def portable(p):
    for var in ("APPDATA", "LOCALAPPDATA", "USERPROFILE"):
        v = os.environ.get(var)
        if v and p and os.path.normcase(p).startswith(os.path.normcase(v)):
            return "%" + var + "%" + p[len(v):]
    return p


def decode_strict(b):
    try:
        t = b.decode("utf-8")
        return (t[1:] if t.startswith("\ufeff") else t), True
    except UnicodeDecodeError:
        return None, False


def identity():
    b = open(BIN, "rb").read()
    ps = ("$s = Get-AuthenticodeSignature -LiteralPath $env:CHAR2_BIN; "
          "[pscustomobject]@{Status=$s.Status.ToString(); Subject=$s.SignerCertificate.Subject; Thumbprint=$s.SignerCertificate.Thumbprint; "
          "NotAfter=$s.SignerCertificate.NotAfter.ToUniversalTime().ToString('o'); Timestamp=($s.TimeStamperCertificate -ne $null)} | ConvertTo-Json -Compress")
    env = dict(os.environ, CHAR2_BIN=BIN)
    sig = subprocess.run(["powershell.exe", "-NoProfile", "-NonInteractive", "-Command", ps], capture_output=True, env=env, stdin=subprocess.DEVNULL)
    sigd = json.loads(sig.stdout.decode("utf-8", "replace").strip() or "{}")
    v = subprocess.run([BIN, "--version"], capture_output=True, stdin=subprocess.DEVNULL, cwd=CWD)
    vt, vok = decode_strict(v.stdout)
    a = subprocess.run([BIN, "auth", "status", "--json"], capture_output=True, stdin=subprocess.DEVNULL, cwd=CWD)
    at, aok = decode_strict(a.stdout)
    try:
        ad = json.loads(at) if aok else {}
    except ValueError:
        ad = {}
    rec = {"Path": portable(BIN), "RealPath": portable(os.path.realpath(BIN)), "Sha256": sha(b), "Size": len(b), "Sha256Expected": BIN_SHA,
           "Sha256Matches": sha(b) == BIN_SHA, "Modified": datetime.datetime.fromtimestamp(os.path.getmtime(BIN), datetime.timezone.utc).isoformat(),
           "Authenticode": {"Status": sigd.get("Status"), "Subject": sigd.get("Subject"), "Thumbprint": sigd.get("Thumbprint"),
                            "NotAfter": sigd.get("NotAfter"), "TimeStamped": sigd.get("Timestamp")},
           "SignerIsAnthropic": sigd.get("Status") == "Valid" and 'O="Anthropic, PBC"' in (sigd.get("Subject") or "") or
                                (sigd.get("Status") == "Valid" and "Anthropic, PBC" in (sigd.get("Subject") or "")),
           "Version": {"Text": vt.strip() if vok else None, "ReturnCode": v.returncode, "StrictUtf8": vok},
           "AuthStatus": {"ReturnCode": a.returncode, "StrictUtf8": aok, "loggedIn": ad.get("loggedIn"), "authMethod": ad.get("authMethod"),
                          "apiProvider": ad.get("apiProvider"), "subscriptionType": ad.get("subscriptionType"),
                          "FieldsOmitted": "correo, organización y sus identificadores (no se guardan)"},
           "Clone": clone_state(), "Time": now()}
    open(os.path.join(HERE, "identity.json"), "w", encoding="utf-8", newline="\n").write(json.dumps(rec, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({k: rec[k] for k in ("Sha256Matches", "SignerIsAnthropic", "Version", "AuthStatus")}, ensure_ascii=True))


def clone_state():
    g = lambda *a: subprocess.run(["git", "-C", CWD] + list(a), capture_output=True, stdin=subprocess.DEVNULL).stdout.decode("utf-8", "replace").strip()
    return {"Path": CWD, "Head": g("rev-parse", "HEAD"), "HeadExpected": CLONE_HEAD, "StatusPorcelain": g("status", "--porcelain", "--ignored"),
            "Remotes": g("remote")}


def run(name, kill_after=None, timeout=900):
    assert sha(open(BIN, "rb").read()) == BIN_SHA, "binary changed"
    sid = str(uuid.uuid4())
    assert not glob.glob(os.path.join(PROJECTS, "*", sid + ".jsonl"))
    out_dir = os.path.join(HERE, name)
    assert not os.path.exists(out_dir), "evidence of this probe already exists"
    os.makedirs(out_dir)
    prompt = PROMPTS[name]
    args = BASE_FLAGS + ["--session-id", sid, "--json-schema", json.dumps(SCHEMA, separators=(",", ":"))]
    rec = {"Probe": name, "SessionId": sid, "Cwd": CWD, "Binary": {"Path": portable(BIN), "Sha256": BIN_SHA},
           "Args": [("<json-schema>" if i == len(args) - 1 else a) for i, a in enumerate(args)],
           "PromptSha256": sha(prompt.encode("utf-8")), "PromptBytes": len(prompt.encode("utf-8")), "CloneBefore": clone_state(), "Start": now()}
    p = subprocess.Popen([BIN] + args, cwd=CWD, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    rec["Pid"] = p.pid
    p.stdin.write(prompt.encode("utf-8"))
    p.stdin.close()
    killed = timed_out = False
    if kill_after is not None:
        try:
            out, err = p.communicate(timeout=kill_after)
        except subprocess.TimeoutExpired:
            rec["TerminateAt"] = now()
            p.terminate()
            killed = True
            out, err = p.communicate(timeout=60)
    else:
        try:
            out, err = p.communicate(timeout=timeout)
        except subprocess.TimeoutExpired:
            rec["TimeoutAt"] = now()
            p.kill()
            killed = timed_out = True
            out, err = p.communicate(timeout=60)
    rec["End"] = now()
    rec["ExitCode"] = p.returncode
    rec["Killed"] = killed
    rec["TimedOut"] = timed_out
    open(os.path.join(out_dir, "stdout.jsonl"), "wb").write(out)
    open(os.path.join(out_dir, "stderr.txt"), "wb").write(err)
    rec["StdoutSha256"] = sha(out)
    rec["StdoutLines"] = len(out.splitlines())
    rec["StderrBytes"] = len(err)
    q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True, stdin=subprocess.DEVNULL)
    rec["ProcessGoneAfter"] = str(p.pid).encode() not in q.stdout
    rec["CloneAfter"] = clone_state()
    open(os.path.join(out_dir, "run.json"), "w", encoding="utf-8", newline="\n").write(json.dumps(rec, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({k: rec[k] for k in ("Probe", "SessionId", "Pid", "ExitCode", "Killed", "StdoutLines", "ProcessGoneAfter")}))


def jsonl(path):
    out = []
    for line in open(path, encoding="utf-8", errors="replace"):
        line = line.strip()
        if line.startswith("{"):
            try:
                out.append(json.loads(line))
            except ValueError:
                pass
    return out


def text_of(c):
    return c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))


def analyze():
    out = {}
    for name in ("C1", "C2"):
        d = os.path.join(HERE, name)
        run_rec = json.load(open(os.path.join(d, "run.json"), encoding="utf-8"))
        stream = jsonl(os.path.join(d, "stdout.jsonl"))
        sid = run_rec["SessionId"]
        tps = glob.glob(os.path.join(PROJECTS, "*", sid + ".jsonl"))
        tr = jsonl(tps[0]) if tps else []
        init = next((o for o in stream if o.get("type") == "system" and o.get("subtype") == "init"), None) or {}
        res = ([o for o in stream if o.get("type") == "result"] or [None])[-1] or {}
        asst = [o for o in tr if o.get("type") == "assistant"]
        users = [o for o in tr if o.get("type") == "user"]
        calls, results = [], {}
        for o in stream:
            m = o.get("message")
            c = m.get("content") if isinstance(m, dict) else None
            if not isinstance(c, list) or o.get("parent_tool_use_id"):
                continue
            for part in c:
                if isinstance(part, dict) and part.get("type") == "tool_use":
                    calls.append((part.get("id"), part.get("name"), part.get("input") or {}))
                elif isinstance(part, dict) and part.get("type") == "tool_result":
                    results[part.get("tool_use_id")] = (text_of(part.get("content")), bool(part.get("is_error")))
        first_user = text_of((users[0].get("message") or {}).get("content")) if users else None
        a = {"Probe": name, "SessionId": sid, "ExitCode": run_rec["ExitCode"], "Killed": run_rec["Killed"], "ProcessGoneAfter": run_rec["ProcessGoneAfter"],
             "Init": {k: init.get(k) for k in ("cwd", "session_id", "model", "permissionMode", "tools", "mcp_servers", "claude_code_version",
                                                "apiKeySource", "output_style")},
             "InitSessionIdMatches": init.get("session_id") == sid,
             "Result": ({"subtype": res.get("subtype"), "is_error": res.get("is_error"), "terminal_reason": res.get("terminal_reason"),
                         "num_turns": res.get("num_turns"), "session_id_matches": res.get("session_id") == sid,
                         "modelUsage": sorted((res.get("modelUsage") or {}).keys()), "structured_output_present": res.get("structured_output") is not None,
                         "structured_output": res.get("structured_output"),
                         "permission_denials": [{"Tool": x.get("tool_name"), "Path": (x.get("tool_input") or {}).get("file_path")}
                                                for x in res.get("permission_denials") or []]} if res else None),
             "ResultMessage": bool(res),
             "ObservedModel": sorted({(o.get("message") or {}).get("model") for o in asst if isinstance(o.get("message"), dict)}, key=str),
             "ObservedEffortPerMessage": sorted({o.get("effort") for o in asst}, key=str),
             "PromptFidelity": first_user == PROMPTS[name],
             "ToolCalls": [{"Tool": n, "Input": {k: str(v)[:200] for k, v in inp.items()}} for i, n, inp in calls],
             "Transcript": {"Path": portable(tps[0]) if tps else None, "Sha256": sha(open(tps[0], "rb").read()) if tps else None, "Entries": len(tr)},
             "AttachmentTypes": sorted({(o.get("attachment") or {}).get("type") for o in tr if o.get("type") == "attachment"} - {None}),
             "CloneUnchanged": run_rec["CloneBefore"]["Head"] == run_rec["CloneAfter"]["Head"] == CLONE_HEAD
                               and run_rec["CloneAfter"]["StatusPorcelain"] == "" and run_rec["CloneAfter"]["Remotes"] == ""}
        sp = [o for o in tr if o.get("type") == "system"]
        if name == "C1":
            reads = [(i, results.get(i)) for i, n, inp in calls if n == "Read"]
            src = open(os.path.join(CWD, "docs", "initiatives", "I-62-A-1.md"), "rb").read().decode("utf-8").replace("\r\n", "\n").split("\n")
            got = {}
            for i, r in reads:
                if not r or r[1]:
                    continue
                for line in r[0].split("\n"):
                    m = re.match(r"^\s*(\d+)\t(.*)$", line)
                    if m:
                        got[int(m.group(1))] = m.group(2)
            faithful = [n for n in range(1, 13) if got.get(n) == src[n - 1]]
            a["ReadFidelity"] = {"LinesDelivered": len([n for n in got if 1 <= n <= 12]), "LinesFaithful": len(faithful), "Of": 12,
                                 "OnlyTheRequestedFile": all(n == "StructuredOutput" or (n == "Read" and str(inp.get("file_path", "")).replace("\\", "/").endswith("docs/initiatives/I-62-A-1.md"))
                                                             for _i, n, inp in calls)}
            so = res.get("structured_output") or {}
            a["StructuredOutputCheck"] = {"first_line_matches": so.get("first_line") == src[0], "lines_read": so.get("lines_read"),
                                          "tools_available": so.get("tools_available")}
            a["AutomaticContext"] = {"ProjectMemoryDir": os.path.isdir(os.path.join(PROJECTS, "D--r62-cli-char2", "memory")),
                                     "TranscriptMentions": {k: sum(json.dumps(o, ensure_ascii=False).count(k) for o in tr) for k in ("CLAUDE.md", "MEMORY.md", "AGENTS.md")}}
        if name == "C2":
            a["ToolCallsBeforeTermination"] = len(calls)
        out[name] = a
        open(os.path.join(HERE, "analysis-%s.json" % name), "w", encoding="utf-8", newline="\n").write(json.dumps(a, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({k: {kk: out[k].get(kk) for kk in ("ExitCode", "Killed", "ResultMessage", "ObservedModel", "ObservedEffortPerMessage", "PromptFidelity",
                                                         "ReadFidelity", "CloneUnchanged")} for k in out}, ensure_ascii=True))


if __name__ == "__main__":
    w = sys.argv[1]
    if w == "identity":
        identity()
    elif w in ("C1", "C2"):
        run(w, kill_after=float(sys.argv[2]) if len(sys.argv) > 2 else None)
    elif w == "analyze":
        analyze()
    else:
        print(__doc__)
        sys.exit(2)
