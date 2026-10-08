"""Bounded read-only characterization of claude-cli (decisiones §56, punto 7; Owner CLAUDE-CLI-I62 = A).

Probes (each one claude-cli process, launched from this script with an argument list, prompt on stdin):
  C1  main: read-only tools only (Read, Grep, Glob), model/effort requested, stream-json, structured output via --json-schema, fixed session id;
      reads 12 lines of a canonical file -> prompt fidelity, read fidelity, model/effort, init facts, result, exit code.
  C2  cancellation/timeout: same flags, a longer read-only task; the invoker terminates the process after a fixed delay -> exit code,
      process gone, partial output, transcript state.
No product writes, no fixture mutation, no git writes. Credentials are never read. Output only under this directory.
"""
import datetime, hashlib, json, os, subprocess, sys, time, uuid

HERE = os.path.dirname(os.path.abspath(__file__))
CLAUDE = os.environ.get("CLAUDE_BIN") or os.path.expanduser(os.path.join("~", ".local", "bin", "claude.exe"))
CWD = "D:/r62-cli-char"
now = lambda: datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")

SCHEMA = {"type": "object", "additionalProperties": False,
          "required": ["first_line", "lines_read", "declared_model", "tools_available", "files_read"],
          "properties": {"first_line": {"type": "string"}, "lines_read": {"type": "integer"}, "declared_model": {"type": "string"},
                         "tools_available": {"type": "array", "items": {"type": "string"}},
                         "files_read": {"type": "array", "items": {"type": "string"}}}}

BASE = [CLAUDE, "-p", "--model", "claude-opus-5-5", "--effort", "xhigh", "--output-format", "stream-json", "--verbose",
        "--safe-mode", "--strict-mcp-config", "--no-chrome", "--tools", "Read,Grep,Glob", "--permission-mode", "dontAsk",
        "--permission-prompts", "none"]

PROMPTS = {
    "C1": ("Eres una sonda de caracterización de solo lectura. Usa solo la herramienta Read para leer el archivo docs/initiatives/I-62-A-1.md, "
           "líneas 1 a 12 (offset 1, limit 12). No leas ningún otro archivo. Devuelve el objeto JSON exigido: first_line = el texto exacto de la "
           "línea 1; lines_read = el número de líneas que leíste; declared_model = el identificador de modelo que crees ser; tools_available = "
           "los nombres de las herramientas que tienes disponibles; files_read = las rutas que leíste."),
    "C2": ("Eres una sonda de caracterización de solo lectura. Usa solo la herramienta Read para leer, uno tras otro y enteros, los archivos "
           "docs/initiatives/I-62-proposal-v14.md (en tramos de 400 líneas) y docs/initiatives/I-62-A-1.md, y después resume cada sección en "
           "detalle. Devuelve el objeto JSON exigido."),
}


def run(name, kill_after=None, timeout=900):
    sid = str(uuid.uuid4())
    out_dir = os.path.join(HERE, name)
    os.makedirs(out_dir, exist_ok=True)
    prompt = PROMPTS[name]
    args = BASE + ["--session-id", sid, "--json-schema", json.dumps(SCHEMA, separators=(",", ":"))]
    rec = {"Probe": name, "SessionId": sid, "Cwd": CWD, "Args": args[1:], "PromptSha256": hashlib.sha256(prompt.encode("utf-8")).hexdigest(),
           "PromptBytes": len(prompt.encode("utf-8")), "Start": now()}
    p = subprocess.Popen(args, cwd=CWD, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    rec["Pid"] = p.pid
    p.stdin.write(prompt.encode("utf-8")); p.stdin.close()
    killed = False
    if kill_after is not None:
        try:
            out, err = p.communicate(timeout=kill_after)
        except subprocess.TimeoutExpired:
            rec["TerminateAt"] = now()
            p.terminate(); killed = True
            out, err = p.communicate(timeout=60)
    else:
        try:
            out, err = p.communicate(timeout=timeout)
        except subprocess.TimeoutExpired:
            rec["TimeoutAt"] = now(); p.kill(); killed = True
            out, err = p.communicate(timeout=60)
    rec["End"] = now(); rec["ExitCode"] = p.returncode; rec["Killed"] = killed
    open(os.path.join(out_dir, "stdout.jsonl"), "wb").write(out)
    open(os.path.join(out_dir, "stderr.txt"), "wb").write(err)
    rec["StdoutSha256"] = hashlib.sha256(out).hexdigest(); rec["StdoutLines"] = len(out.splitlines()); rec["StderrBytes"] = len(err)
    # process gone?
    q = subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True, text=True)
    rec["ProcessGoneAfter"] = str(p.pid) not in q.stdout
    open(os.path.join(out_dir, "run.json"), "w", encoding="utf-8", newline="\n").write(json.dumps(rec, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({k: rec[k] for k in ("Probe", "SessionId", "Pid", "ExitCode", "Killed", "StdoutLines", "ProcessGoneAfter")}))


if __name__ == "__main__":
    which = sys.argv[1]
    run(which, kill_after=float(sys.argv[2]) if len(sys.argv) > 2 else None)
