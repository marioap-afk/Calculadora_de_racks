"""I-62 / FX-02 — comparación y auditoría de la ÚNICA corrida de medición de claude-cli (operaciones 5, 7, 8 y 9). Declarado y ensayado ANTES
del lanzamiento; la sesión principal lo ejecuta DESPUÉS, una vez, y no se corrige tras la corrida.

Veredicto por operación: DEMONSTRATED o NOT_DEMONSTRATED, con los criterios explícitos de abajo (cada criterio fallido es un motivo).
  Clasificación previa del lanzamiento: ABORTED_BEFORE_LAUNCH (run.json.Aborted), INVALID_LAUNCH (TRANSPORT_AUTH_REFRESH_CONFLICT: result con
  «Failed to refresh OAuth token» y 0 tokens, o ese texto en stderr sin init) o LAUNCHED. Fuera de LAUNCHED, las cuatro son NOT_DEMONSTRATED.
  Op 5 (observar el resultado con el contrato canónico):
    R5.1 la CLI aceptó el esquema (sin «--json-schema is not a valid JSON Schema» en stderr/stdout/result; init con StructuredOutput en tools);
    R5.2 exactamente un mensaje result, subtype success, is_error false, session_id = el fijado;
    R5.3 structured_output presente y objeto;
    R5.4 structured_output = input de la última llamada StructuredOutput del stream;
    R5.5 válido contra el esquema canónico VERBATIM (SHA-256 9f4639da…) con el validador stdlib Y con Test-Json (pwsh 7), que coinciden.
    (Informativo, no bloquea: eco de los valores fijos del prompt.)
  Op 7 (confirmar la terminación de TODO el árbol):
    R7.1 corrida representativa: init presente y al menos una llamada Read, Grep o Glob;
    R7.2 salida de la raíz observada por el invocador (código de salida) y su identidad confirmada terminada por handle;
    R7.3 en el último control (S1, o S2 si S1 tenía restos) ninguna identidad del árbol viva (handles Y CIM) y terminación NATURAL (sin
         tope vencido y sin matar restos);
    R7.4 ningún huérfano vivo (regla de 16.4 adaptada; fuentes rápida y CIM);
    R7.5 cobertura: hueco máximo rápido <= MaxFastGapS y CIM <= MaxCimGapS entre el lanzamiento y S1; muestreador CIM sin errores;
    R7.6 coherencia: toda identidad del árbol vista por CIM está en la fuente rápida.
  Op 8 (clasificar procesos):
    R8.1 = R7.1; R8.2 raíz observada, clase launched-root y ExecutablePath = binario fijado; R8.3 todo miembro observado con clase de la tabla
    cerrada y ninguno unclassified; R8.4 coherencia entre fuentes (= R7.6, y mismo nombre y ruta); R8.5 ningún proceso ajeno al árbol con la
    ruta del clon en su línea de órdenes durante la ventana; R8.6 = R7.5.
  Op 9 (declarar la huella):
    R9.1 init presente con claude_code_version = 2.1.293 y session_id = el fijado; R9.2 CLAUDE_CONFIG_DIR ausente (preflight G7);
    R9.3 huella antes y después medida, y estable (mismos SHA-256 y existencia en todo candidato de usuario, proyecto y gestionado; mismos
         nombres de variables); R9.4 declaración derivable: CONFIG_FILES (SHA-256 + nombres de claves de los archivos presentes) o NINGUNA (todos
         ausentes; exige además la aceptación registrada del Coordinator, AUTOMATION_PLAN 16.19).
Salida: <run>/audit.json y <run>/custody/ (copias SANEADAS: nada de stdout.jsonl crudo ni de la transcripción, solo sus SHA-256), con barrido de
fugas; si el barrido encuentra algo, CustodyReady = false y no se custodia.
Uso:  python -I -B audit.py                      (corrida real: run/ del paquete)
      python -I -B audit.py --run-dir <d> --pinned-path <exe> --clone <c> --label dry   (solo el ensayo en seco)
"""
import datetime
import importlib.util
import json
import os
import re
import sys

PKG = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(PKG, name + ".py"))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


C = _load("ccli_common")
SC = _load("schemacheck")
K = C.load_constants(PKG)
AUDITOR = "ccli-meas audit v1 (declarado y ensayado antes del lanzamiento)"
INIT_KEEP = ["type", "subtype", "session_id", "cwd", "model", "permissionMode", "tools", "mcp_servers", "claude_code_version", "output_style",
             "apiKeySource", "agents", "plugins", "slash_commands", "skills"]


def load_jsonl(path):
    out, raw_bad = [], []
    if not os.path.isfile(path):
        return out, raw_bad
    for line in open(path, encoding="utf-8-sig", errors="replace"):
        s = line.strip()
        if not s:
            continue
        try:
            o = json.loads(s)
            out.append(o if isinstance(o, dict) else {"__non_object__": True})
        except ValueError:
            raw_bad.append(s[:300])
    return out, raw_bad


def read_json(path):
    try:
        return json.load(open(path, encoding="utf-8-sig"))
    except (OSError, ValueError):
        return None


def parse_iso(s):
    if not s:
        return None
    s = s.rstrip("Z")
    if "." in s:
        a, b = s.split(".", 1)
        s = a + "." + (b + "000000")[:6]
    try:
        return datetime.datetime.fromisoformat(s).replace(tzinfo=datetime.timezone.utc)
    except ValueError:
        return None


# ------------------------------------------------------------------ stream-json
def analyze_stream(stream, bad_lines, stderr_text):
    init = next((o for o in stream if o.get("type") == "system" and o.get("subtype") == "init"), None)
    results = [o for o in stream if o.get("type") == "result"]
    tool_uses, models = [], set()
    for o in stream:
        if o.get("type") == "assistant" and isinstance(o.get("message"), dict):
            msg = o["message"]
            if msg.get("model"):
                models.add(msg["model"])
            for b in msg.get("content") or []:
                if isinstance(b, dict) and b.get("type") == "tool_use":
                    tool_uses.append({"Id": b.get("id"), "Name": b.get("name"), "Input": b.get("input")})
    res = results[-1] if results else None
    usage = (res or {}).get("usage") if isinstance((res or {}).get("usage"), dict) else {}
    tokens = sum(int(usage.get(k) or 0) for k in ("input_tokens", "output_tokens", "cache_creation_input_tokens", "cache_read_input_tokens"))
    res_text = str((res or {}).get("result") or "")
    blob = "\n".join(bad_lines) + "\n" + (stderr_text or "") + "\n" + res_text
    auth = K["AuthRefreshMark"] in res_text and tokens == 0 or (K["AuthRefreshMark"] in (stderr_text or "") and init is None)
    return {"Init": init, "Results": results, "Result": res, "ToolUses": tool_uses, "Models": sorted(models), "Tokens": tokens,
            "SchemaRejected": K["SchemaRejectMark"] in blob, "AuthRefreshConflict": bool(auth), "NonJsonLines": len(bad_lines)}


def init_summary(init):
    if not init:
        return None
    s = {k: init.get(k) for k in INIT_KEEP if k in init}
    s["AllKeyNames"] = sorted(init.keys())
    return s


# ------------------------------------------------------------------ op 5
def fixed_block():
    txt = open(os.path.join(PKG, K["PromptFile"]), encoding="utf-8").read()
    m = re.search(r"```json\n(.*?)\n```", txt, re.S)
    return json.loads(m.group(1))


def eval_op5(sa, schema_path, session_id):
    r, info = [], {}
    init, res = sa["Init"], sa["Result"]
    if sa["SchemaRejected"] or not init or "StructuredOutput" not in (init.get("tools") or []):
        r.append("R5.1 la CLI no aceptó el esquema canónico (rechazo explícito o sin init con StructuredOutput)" if sa["SchemaRejected"]
                 else "R5.1 sin init con StructuredOutput en tools")
    if len(sa["Results"]) != 1 or not res or res.get("subtype") != "success" or res.get("is_error") or res.get("session_id") != session_id:
        r.append("R5.2 result ausente, múltiple, con error o de otra sesión")
    so = (res or {}).get("structured_output")
    if not isinstance(so, dict):
        r.append("R5.3 structured_output ausente o no objeto")
    sos = [t["Input"] for t in sa["ToolUses"] if t["Name"] == "StructuredOutput"]
    if isinstance(so, dict) and (not sos or sos[-1] != so):
        r.append("R5.4 structured_output distinto del input de la última llamada StructuredOutput")
    if isinstance(so, dict):
        v = SC.check(so, schema_path)
        info["SchemaValidation"] = v
        if not v["Valid"]:
            r.append("R5.5 structured_output no válido contra el esquema canónico (o validadores en desacuerdo)")
        fb = fixed_block()
        info["FixedFieldEcho"] = {"Mismatches": sorted(k for k, val in fb.items() if so.get(k) != val), "Blocking": False}
    info["StructuredOutputCalls"] = len(sos)
    return ("DEMONSTRATED" if not r else "NOT_DEMONSTRATED"), r, info, so


# ------------------------------------------------------------------ ops 7 y 8
def cim_ticks(path):
    ticks, _ = load_jsonl(path)
    return ticks


def merge_tree(fast, ticks, tol_ms):
    ids = []
    for m in fast.get("Members") or []:
        ids.append({"Pid": m["Pid"], "Ppid": m.get("Ppid"), "Name": m.get("Name"), "Path": m.get("Path"), "CreationUtc": m.get("CreationUtc"),
                    "IsRoot": bool(m.get("IsRoot")), "Sources": ["fast"], "FirstSeenS": m.get("FirstSeenS"), "LastSeenAliveS": m.get("LastSeenAliveS")})
    cim_only = []
    cim_rows = {}
    for t in ticks:
        for row in t.get("Tree") or []:
            key = (row["Pid"], row.get("CreationUtc"))
            if key not in cim_rows:
                cim_rows[key] = dict(row, FirstSeq=t.get("Seq"), LastSeq=t.get("Seq"))
            else:
                cim_rows[key]["LastSeq"] = t.get("Seq")
    mismatches = []
    for (pid, cr), row in cim_rows.items():
        dc = parse_iso(cr)
        hit = None
        for x in ids:
            dx = parse_iso(x["CreationUtc"])
            if x["Pid"] == pid and dc and dx and abs((dc - dx).total_seconds()) * 1000 <= tol_ms:
                hit = x
                break
        if hit is None:
            cim_only.append({"Pid": pid, "Name": row.get("Name"), "CreationUtc": cr})
            ids.append({"Pid": pid, "Ppid": row.get("Ppid"), "Name": row.get("Name"), "Path": row.get("Path"), "CreationUtc": cr,
                        "IsRoot": bool(row.get("Root")), "Sources": ["cim"]})
        else:
            hit["Sources"].append("cim")
            hit["CmdLen"], hit["CmdSha256"], hit["CmdHead"] = row.get("CmdLen"), row.get("CmdSha256"), row.get("CmdHead")
            if (row.get("Name") or "").lower() != (hit.get("Name") or "").lower() or (
                    row.get("Path") and hit.get("Path") and os.path.normcase(row["Path"]) != os.path.normcase(hit["Path"])):
                mismatches.append({"Pid": pid, "Problem": "nombre o ruta distintos entre fuentes"})
            if not hit.get("Path") and row.get("Path"):
                hit["Path"] = row["Path"]
    return ids, cim_only, mismatches


def cim_gaps(ticks, t_from, t_to):
    ts = [parse_iso(t.get("Utc")) for t in ticks if t.get("Utc")]
    ts = [t for t in ts if t and (t_from is None or t >= t_from) and (t_to is None or t <= t_to)]
    if len(ts) < 2:
        return {"Samples": len(ts), "MaxGapS": None}
    return {"Samples": len(ts), "MaxGapS": round(max((b - a).total_seconds() for a, b in zip(ts, ts[1:])), 3)}


def cim_orphans(check, tree):
    out = []
    keys = {(x["Pid"], (x["CreationUtc"] or "")[:23]) for x in tree}
    for c in (check.get("CimOnce") or {}).get("ClosedInWindow") or []:
        if (c["Pid"], (c.get("CreationUtc") or "")[:23]) in keys:
            continue
        pc, cc = parse_iso(c.get("ParentCreationUtc")), parse_iso(c.get("CreationUtc"))
        if not c.get("ParentExists") or (pc and cc and pc > cc):
            out.append({"Pid": c["Pid"], "Name": c.get("Name"), "CreationUtc": c.get("CreationUtc")})
    return out


def eval_ops78(sa, run, fast, ticks, term, pinned_path, versions_dir, representative_required=True):
    T = K["Timing"]
    tree, cim_only, mism = merge_tree(fast or {}, ticks, T["CreationToleranceMs"])
    for x in tree:
        x["Class"] = C.classify_process(x.get("Name"), x.get("Path"), x["IsRoot"], pinned_path, versions_dir)
    rep = bool(sa["Init"]) and any(t["Name"] in ("Read", "Grep", "Glob") for t in sa["ToolUses"])
    checks = (term or {}).get("Checks") or []
    last = checks[-1] if checks else {}
    first_ok = next((c for c in checks if not c.get("Residual")), None)
    t_launch, t_s1 = parse_iso(run.get("LaunchStartUtc")), parse_iso(checks[0]["Fast"]["At"]) if checks else None
    gaps_fast = ((fast or {}).get("Gaps") or {}).get("Launch→S1") or {}
    gaps_cim = cim_gaps(ticks, t_launch, t_s1)
    cim_errors = sum(1 for t in ticks if t.get("Error"))
    coverage_ok = (gaps_fast.get("MaxGapS") is not None and gaps_fast["MaxGapS"] <= T["MaxFastGapS"] and gaps_cim.get("MaxGapS") is not None
                   and gaps_cim["MaxGapS"] <= T["MaxCimGapS"] and cim_errors == 0)
    root = next((x for x in tree if x["IsRoot"]), None)
    root_exit = None
    if last:
        for m in last["Fast"]["Members"]:
            if m["Key"] == (fast or {}).get("RootKey"):
                root_exit = m
    orphans = list(last.get("OrphansFast") or []) + cim_orphans(last, tree) if last else []
    clone_refs = {}
    for t in ticks:
        for c in t.get("CloneRefs") or []:
            clone_refs[c["Pid"]] = c.get("Name")
    for c in ((last.get("CimOnce") or {}).get("CloneRefs") or []) if last else []:
        clone_refs[c["Pid"]] = c.get("Name")
    tree_pids = {x["Pid"] for x in tree}
    clone_refs = {k: v for k, v in clone_refs.items() if k not in tree_pids}   # un miembro del árbol (p. ej., rg con ruta absoluta) no es ajeno
    r7, r8 = [], []
    if representative_required and not rep:
        r7.append("R7.1 corrida no representativa (sin init o sin ninguna llamada Read/Grep/Glob)")
        r8.append("R8.1 corrida no representativa (sin init o sin ninguna llamada Read/Grep/Glob)")
    if run.get("ExitCode") is None or not root_exit or root_exit.get("Exited") is not True:
        r7.append("R7.2 salida de la raíz no observada o identidad de la raíz no confirmada terminada")
    if not checks or last.get("Residual") or (term or {}).get("Mode") not in ("NATURAL_BY_S1", "NATURAL_BY_S2") or run.get("TimedOut"):
        r7.append("R7.3 quedan identidades del árbol vivas o la terminación no fue natural (Mode=%s)" % (term or {}).get("Mode"))
    if orphans:
        r7.append("R7.4 huérfanos vivos: %d" % len(orphans))
    if not coverage_ok:
        r7.append("R7.5 cobertura insuficiente (rápida %s s, CIM %s s, errores CIM %d)" % (gaps_fast.get("MaxGapS"), gaps_cim.get("MaxGapS"), cim_errors))
        r8.append("R8.6 cobertura insuficiente")
    if cim_only:
        r7.append("R7.6 identidades vistas por CIM y no por la fuente rápida: %d" % len(cim_only))
        r8.append("R8.4 identidades vistas por CIM y no por la fuente rápida: %d" % len(cim_only))
    if mism:
        r8.append("R8.4 nombre o ruta distintos entre fuentes: %d" % len(mism))
    if not root or root["Class"] != "launched-root" or not root.get("Path") or os.path.normcase(os.path.normpath(root["Path"])) != os.path.normcase(os.path.normpath(pinned_path)):
        r8.append("R8.2 raíz no observada o con una ruta distinta del binario fijado")
    if any(x["Class"] == "unclassified" for x in tree):
        r8.append("R8.3 miembros sin clasificar")
    if clone_refs:
        r8.append("R8.5 procesos ajenos al árbol con la ruta del clon: %d" % len(clone_refs))
    counts = {}
    for x in tree:
        counts[x["Class"]] = counts.get(x["Class"], 0) + 1
    detail = {"Representative": rep, "Tree": tree, "ClassCounts": counts, "CimOnly": cim_only, "SourceMismatches": mism,
              "Coverage": {"Fast": gaps_fast, "Cim": gaps_cim, "CimErrors": cim_errors}, "TerminationMode": (term or {}).get("Mode"),
              "Checks": [{"Label": c["Label"], "Residual": c["Residual"], "FastAlive": c["Fast"]["Alive"],
                          "CimAlive": (c.get("CimOnce") or {}).get("AliveTree"), "OrphansFast": c.get("OrphansFast")} for c in checks],
              "Orphans": orphans, "CloneRefs": [{"Pid": k, "Name": v} for k, v in clone_refs.items()], "RootExit": root_exit,
              "UnexpectedForToolset": sorted({x["Name"] for x in tree if x["Class"] == "shell"}),
              "ClassRules": C.CLASS_RULES}
    return ("DEMONSTRATED" if not r7 else "NOT_DEMONSTRATED"), r7, ("DEMONSTRATED" if not r8 else "NOT_DEMONSTRATED"), r8, detail


# ------------------------------------------------------------------ op 9
LIMITS9 = [
    "Los valores de configuración nunca se leen: no se demuestra QUÉ claves aplicó la CLI, solo qué archivos existen, su SHA-256, los nombres de sus claves y que no cambiaron durante la corrida.",
    "Las variables de entorno heredadas de la sesión que lanza (CLAUDE*/ANTHROPIC*) forman parte de la configuración efectiva; solo se registran sus nombres, no sus valores ni su efecto.",
    "%USERPROFILE%\\.claude.json es un archivo de estado (contadores, proyectos, cuenta): solo metadatos; se espera que cambie y no entra en la huella declarable.",
    "Sin trazas de acceso a archivos (requerirían privilegios de administrador): no se demuestra que la CLI no consulte otro archivo fuera de la lista de candidatos.",
    "Con --safe-mode, CLAUDE.md, la memoria y los ganchos no entran (medido en la caracterización C1); según la ayuda del propio binario 2.1.293, el modo seguro desactiva las personalizaciones (plugins instalados, ganchos, MCP, comandos, agentes, estilos…) pero la configuración gestionada por política sigue aplicando y la autenticación, el modelo, las herramientas y plugins integrados y los permisos funcionan con normalidad: por eso el archivo de usuario sigue siendo candidato a regir la corrida. El init muestra la configuración efectiva que la CLI declara (tools, mcp_servers, permissionMode, plugins, agentes).",
    "Candidatos gestionados por política en Windows tomados de las cadenas del binario: %ProgramFiles%\\ClaudeCode\\managed-settings.json, su carpeta managed-settings.d y las claves HKLM/HKCU\\SOFTWARE\\Policies\\ClaudeCode (de estas solo existencia, nombres y un SHA-256 de los datos, que no salen del proceso)."]


def eval_op9(sa, pre, fb, fa, session_id):
    r = []
    init = sa["Init"]
    if not init or str(init.get("claude_code_version")) != K["Binary"]["Version"] or init.get("session_id") != session_id:
        r.append("R9.1 init ausente o con versión/sesión distintas")
    g7 = ((pre or {}).get("Checks") or {}).get("G7-Env") or {}
    if g7.get("ClaudeConfigDirSet") is not False:
        r.append("R9.2 CLAUDE_CONFIG_DIR no demostrada ausente")
    diff = C.fingerprint_diff(fb, fa) if (fb and fa) else None
    if not diff or not diff["Stable"]:
        r.append("R9.3 huella no medida antes y después, o cambiada durante la corrida (P-11)")
    present = [c for c in (fa or {}).get("Candidates", []) if c["Scope"] in ("user", "project", "managed") and c["Exists"]]
    if any(not isinstance(c.get("KeyNames"), list) for c in present):
        r.append("R9.4 algún archivo presente sin nombres de claves analizables")
    decl = None
    if fa:
        if present:
            decl = {"Kind": "CONFIG_FILES", "Files": [{"Id": c["Id"], "Path": c["Path"], "Sha256": c["Sha256"], "KeyNames": c.get("KeyNames")} for c in present],
                    "EnvNames": fa["EnvNames"], "RequiresCoordinatorAcceptance": False}
        else:
            decl = {"Kind": "NINGUNA", "Files": [], "EnvNames": fa["EnvNames"], "RequiresCoordinatorAcceptance": True,
                    "Demonstration": "todos los candidatos de usuario, proyecto y gestionados ausentes antes y después; CLAUDE_CONFIG_DIR ausente; init coherente con una configuración solo de flags"}
    corr = None
    if init and fa:
        u1 = next((c for c in fa["Candidates"] if c["Id"] == "U1"), {})
        corr = {"InitPluginsListed": bool(init.get("plugins")), "U1HasEnabledPluginsKey": "enabledPlugins" in (u1.get("KeyNames") or []),
                "InitMcpServers": init.get("mcp_servers"), "InitApiKeySource": init.get("apiKeySource"),
                "Reading": "si el init lista plugins y U1 tiene la clave enabledPlugins, es evidencia (por nombres, sin valores) de que U1 rige al menos esa parte"}
    env_now = (fa or {}).get("EnvNames", {}).get("ClaudeAnthropic", [])
    ref = set(K["ReferenceEnvNamesA4"])
    envcmp = {"AddedVsA4": sorted(set(env_now) - ref), "MissingVsA4": sorted(ref - set(env_now)), "Blocking": False}
    return ("DEMONSTRATED" if not r else "NOT_DEMONSTRATED"), r, {"Declaration": decl, "Diff": diff, "Correlation": corr, "EnvVsA4": envcmp,
                                                                  "Limits": LIMITS9}


# ------------------------------------------------------------------ cumplimiento (informativo)
def compliance(sa, clone):
    out = {"ToolCounts": {}, "Disallowed": [], "OutsideClone": []}
    cl = os.path.normcase(os.path.realpath(clone))
    for t in sa["ToolUses"]:
        out["ToolCounts"][t["Name"]] = out["ToolCounts"].get(t["Name"], 0) + 1
        if t["Name"] not in K["AllowedTools"]:
            out["Disallowed"].append(t["Name"])
        inp = t["Input"] if isinstance(t["Input"], dict) else {}
        for key in ("file_path", "path"):
            p = inp.get(key)
            if p:
                full = p if os.path.isabs(p) else os.path.join(clone, p)
                rp = os.path.normcase(os.path.realpath(full))
                if not (rp == cl or rp.startswith(cl + os.sep)):
                    out["OutsideClone"].append({"Tool": t["Name"], "Key": key})
    res = sa["Result"] or {}
    out["PermissionDenials"] = len(res.get("permission_denials") or [])
    out["Ok"] = not out["Disallowed"] and not out["OutsideClone"] and out["PermissionDenials"] == 0
    return out


def prompt_fidelity(transcript_path):
    if not transcript_path or not os.path.isfile(transcript_path):
        return {"Checked": False}
    prompt = open(os.path.join(PKG, K["PromptFile"]), encoding="utf-8").read()
    for line in open(transcript_path, encoding="utf-8", errors="replace"):
        try:
            o = json.loads(line)
        except ValueError:
            continue
        if o.get("type") == "user" and isinstance(o.get("message"), dict):
            c = o["message"].get("content")
            text = c if isinstance(c, str) else "".join(b.get("text", "") for b in (c or []) if isinstance(b, dict) and b.get("type") == "text")
            return {"Checked": True, "FirstUserMessageEqualsPrompt": text == prompt}
    return {"Checked": True, "FirstUserMessageEqualsPrompt": False}


def templatize(args):
    out, i = [], 0
    while i < len(args):
        out.append(args[i])
        if args[i] in ("--session-id", "--json-schema") and i + 1 < len(args):
            out.append("<session-id>" if args[i] == "--session-id" else "<json-schema>")
            i += 1
        i += 1
    return out


# ------------------------------------------------------------------ principal
def main(argv):
    def opt(name, default=None):
        return argv[argv.index(name) + 1] if name in argv else default
    run_dir = opt("--run-dir", os.path.join(PKG, "run"))
    launch = os.path.join(run_dir, "launch")
    clone = opt("--clone", K["Clone"]["Path"])
    pinned = opt("--pinned-path", C.expand(K["Binary"]["Display"]))
    label = opt("--label", "real")
    if label != "real" and ("--run-dir" not in argv):
        print(json.dumps({"Refused": "las opciones de ensayo exigen --run-dir"}, ensure_ascii=False))
        return 2
    san = C.Sanitizer()
    run = read_json(os.path.join(launch, "run.json")) or {}
    pre = read_json(os.path.join(launch, "preflight.json"))
    stream, bad = load_jsonl(os.path.join(launch, "stdout.jsonl"))
    stderr_text = open(os.path.join(launch, "stderr.txt"), encoding="utf-8", errors="replace").read() if os.path.isfile(os.path.join(launch, "stderr.txt")) else ""
    fast = read_json(os.path.join(launch, "proc-fast.json"))
    ticks = cim_ticks(os.path.join(launch, "proc-cim.jsonl"))
    term = read_json(os.path.join(launch, "termination.json"))
    fb = read_json(os.path.join(launch, "fingerprint-before.json"))
    fa = read_json(os.path.join(launch, "fingerprint-after.json"))
    sid = K["SessionId"] if label == "real" else run.get("SessionId", K["SessionId"])
    sa = analyze_stream(stream, bad, stderr_text)
    marker_ok = os.path.isfile(os.path.join(run_dir, "ONE-SHOT.marker"))
    if run.get("Aborted"):
        launch_class = "ABORTED_BEFORE_LAUNCH"
    elif sa["AuthRefreshConflict"]:
        launch_class = "INVALID_LAUNCH"
    elif run.get("Pid") is None:
        launch_class = "NO_LAUNCH_RECORD"
    else:
        launch_class = "LAUNCHED"
    schema_path = os.path.join(PKG, K["SchemaFile"])
    v5, r5, i5, so = eval_op5(sa, schema_path, sid)
    v7, r7, v8, r8, d78 = eval_ops78(sa, run, fast, ticks, term, pinned, C.expand(K["Binary"]["VersionsDir"]),
                                     representative_required=("--no-representative" not in argv))
    v9, r9, i9 = eval_op9(sa, pre, fb, fa, sid)
    if launch_class != "LAUNCHED":
        why = {"ABORTED_BEFORE_LAUNCH": "lanzamiento abortado antes de ejecutar el binario: %s" % run.get("Aborted"),
               "INVALID_LAUNCH": "TRANSPORT_AUTH_REFRESH_CONFLICT (sin reintento; decide el Coordinator)",
               "NO_LAUNCH_RECORD": "sin registro de lanzamiento"}[launch_class]
        v5 = v7 = v8 = v9 = "NOT_DEMONSTRATED"
        r5, r7, r8, r9 = [why] + r5, [why] + r7, [why] + r8, [why] + r9
    comp = compliance(sa, clone)
    tpath = None
    if label == "real":
        tpath = os.path.join(os.path.expanduser("~"), ".claude", "projects", C.project_dir_name(clone), sid + ".jsonl")
    audit = {
        "Auditor": AUDITOR, "Label": label, "RunId": K["RunId"], "SessionId": sid, "AuditedAt": C.now(),
        "PackageManifestSha256": C.sha_file(os.path.join(PKG, C.MANIFEST)) if os.path.isfile(os.path.join(PKG, C.MANIFEST)) else None,
        "OneShotMarkerPresent": marker_ok, "LaunchClass": launch_class,
        "Verdicts": {"5": {"Verdict": v5, "Reasons": r5}, "7": {"Verdict": v7, "Reasons": r7}, "8": {"Verdict": v8, "Reasons": r8},
                     "9": {"Verdict": v9, "Reasons": r9}},
        "Op5": i5, "Op78": {k: v for k, v in d78.items() if k != "Tree"}, "Op9": i9,
        "Stream": {"Lines": len(stream), "NonJsonLines": sa["NonJsonLines"], "SchemaRejected": sa["SchemaRejected"],
                   "AuthRefreshConflict": sa["AuthRefreshConflict"], "Models": sa["Models"], "Tokens": sa["Tokens"],
                   "ResultSubtype": (sa["Result"] or {}).get("subtype"), "IsError": (sa["Result"] or {}).get("is_error"),
                   "TerminalReason": (sa["Result"] or {}).get("terminal_reason"), "NumTurns": (sa["Result"] or {}).get("num_turns")},
        "Compliance": comp, "PromptFidelity": prompt_fidelity(tpath) if tpath else {"Checked": False},
        "Hashes": {"Stdout": run.get("StdoutSha256"), "Transcript": (run.get("Transcript") or {}).get("Sha256"),
                   "ProcCim": C.sha_file(os.path.join(launch, "proc-cim.jsonl")) if os.path.isfile(os.path.join(launch, "proc-cim.jsonl")) else None},
        "CloneAfter": run.get("CloneAfter"),
    }
    custody = os.path.join(run_dir, "custody")
    os.makedirs(custody, exist_ok=True)
    files = {
        "run.json": {k: v for k, v in run.items() if k != "Args"} | {"ArgsMatchTemplate": templatize(run.get("Args") or []) == K["ArgsTemplate"]},
        "preflight.json": pre,
        "init.json": init_summary(sa["Init"]),
        "result.json": so,
        "proctree.json": {"Tree": [{k: v for k, v in x.items()} for x in d78["Tree"]], "ClassRules": C.CLASS_RULES},
        "termination.json": term,
        "fingerprint.json": {"Before": fb, "After": fa},
        "stream-summary.json": {"ToolUses": [{"Name": t["Name"], "InputKeys": sorted((t["Input"] or {}).keys()) if isinstance(t["Input"], dict) else None,
                                              "Path": (t["Input"] or {}).get("file_path") or (t["Input"] or {}).get("path") or (t["Input"] or {}).get("pattern")
                                              if isinstance(t["Input"], dict) else None} for t in sa["ToolUses"]],
                                "StderrHead": stderr_text[:4000]},
    }
    for name, obj in files.items():
        C.write_json(os.path.join(custody, name), san.obj(obj))
    leaks = san.leak_scan(custody)
    audit_text = json.dumps(san.obj(audit), ensure_ascii=False)
    if san.leak_classes(audit_text):
        leaks.append({"File": "audit.json", "Classes": san.leak_classes(audit_text)})
    audit["CustodyLeakScan"] = leaks
    audit["CustodyReady"] = not leaks
    C.write_json(os.path.join(run_dir, "audit.json"), san.obj(audit))
    C.write_json(os.path.join(custody, "audit.json"), san.obj(audit))
    sums = ["%s  %s" % (C.sha_file(os.path.join(custody, n)), n) for n in sorted(os.listdir(custody)) if n != "SHA256SUMS.txt"]
    with open(os.path.join(custody, "SHA256SUMS.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(sums) + "\n")
    print(json.dumps({"LaunchClass": launch_class, "Verdicts": {k: v["Verdict"] for k, v in audit["Verdicts"].items()},
                      "CustodyReady": audit["CustodyReady"]}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
