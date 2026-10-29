"""I-66 R1a, AT-1: self-verification of the Principal at the opening of the pilot unit (I-62 decisions §105 point 4).

Reuses the F2 mechanical control of I-62 (docs/automation/evidence/I-62-F2/f2-mc.py, imported, not copied): the §12 aggregate of
docs/automation/agent-execution/README.md, the production of a rackcad-preflight/v1 (§13.1), its two-phase Test-Json validation and the
coherence rules C1-C8 (§13.2), and the §13.4 sanitizer. The I62 texts are read from the I-62 worktree at the shadow authority revision, where
they are materialized and inactive: here they only add STOP checks and never authorize anything.

Inputs are the session's own observations (get_session("self"), host, remote and write facts). Expected values are the oracle rows E1, E3 and
E4 of README §12, written next to each case; the procedure never reads them.

Usage: python -B r1a_at1.py <I-62 worktree> <obs dir> <out dir>
"""
import copy, datetime, hashlib, importlib.util, json, os, subprocess, sys

I62, OBS_DIR, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
NL = chr(10)
AUTH_REV = subprocess.check_output(["git", "-C", I62, "rev-parse", "HEAD"]).decode().strip()
assert subprocess.check_output(["git", "-C", I62, "status", "--porcelain", "--", "docs/automation/agent-execution"]).decode() == ""
F2_EV = os.path.join(I62, "docs", "automation", "evidence", "I-62-F2")
sys.argv = ["f2-mc.py", I62, F2_EV]
spec = importlib.util.spec_from_file_location("f2mc", os.path.join(F2_EV, "f2-mc.py"))
f2 = importlib.util.module_from_spec(spec)
spec.loader.exec_module(f2)

load = lambda n: json.load(open(os.path.join(OBS_DIR, n), encoding="utf-8"))
gs, host, rf = load("obs-get-session.json"), load("obs-host.json"), load("obs-remote.json")
g = gs["Fields"]
utc = g["lastActivityAt"]
win = "ventana UTC [%s, %s]" % tuple(gs["WindowUtc"])
CATALOG = f2.git_blob(f2.PROTO_REL + "/model-catalog.md")
DESCRIPTOR = f2.git_blob(f2.PROTO_REL + "/adapters/claude-desktop-session.md")
# model-catalog.md, entry claude-opus-5-5: «Nivel: Frontera»; effort «Long-horizon → xhigh», «Deep → high» (the client values measured there).
LEVEL = {"claude-opus-5-5": "Frontera"}
EFFORT = {"low": "Routine", "medium": "Balanced", "high": "Deep", "xhigh": "Long-horizon", "max": "Maximum"}
PID = "P%s-%s" % (utc[:19].replace("-", "").replace(":", "") + "Z", hashlib.sha256((g["sessionId"] + utc).encode()).hexdigest()[:4])

f2.UNIT = "I-66"
f2.OBS = {"Host": host, "ObservedUtc": utc}
f2.preflight_id = lambda n: PID


def observations(effort_client):
    rw, rr = rf["RepoWrite"], rf["RemoteFacts"]
    o = {
        "PRINCIPAL_COORDINATION.level": f2.observed(
            "Frontera", "get_session(\"self\").model = %s; nivel por model-catalog.md blob %s; %s" % (g["model"], CATALOG, win), "RUNTIME_OBSERVED", utc),
        "PRINCIPAL_COORDINATION.introspection": f2.observed(
            "RUNTIME_OBSERVED", "get_session(\"self\") respondió con model, effort y sessionId; nivel de la fuente según adapters/claude-desktop-session.md blob %s; %s"
            % (DESCRIPTOR, win), "RUNTIME_OBSERVED", utc),
        "PRINCIPAL_COORDINATION.remote-facts": f2.observed(
            "read", "%s en invocación de esta sesión: rc=%d; %s; ventana UTC [%s, %s]" % (rr["Command"], rr["Rc"], ", ".join("%s=%s" % kv for kv in rr["Refs"].items()),
                                                                                       rr["WindowUtc"][0], rr["WindowUtc"][1]), "RUNTIME_OBSERVED", rr["WindowUtc"][1]),
        "PRINCIPAL_COORDINATION.repo-write": f2.observed(
            "write", "get_session(\"self\").permissionMode=%s; %s: rc=%d, %s; ventana UTC [%s, %s]" % (g["permissionMode"], rw["Command"], rw["Rc"], rw["Result"],
                                                                                                  rw["WindowUtc"][0], rw["WindowUtc"][1]), "RUNTIME_OBSERVED", rw["WindowUtc"][1]),
    }
    if effort_client is not None:
        o["PRINCIPAL_COORDINATION.effort"] = f2.observed(
            EFFORT[effort_client], "get_session(\"self\").effort = %s; traducción por model-catalog.md blob %s; %s" % (effort_client, CATALOG, win), "RUNTIME_OBSERVED", utc)
    return o


def preflight(effort_client):
    assert LEVEL[g["model"]] == "Frontera"
    actor = {"AdapterId": "claude-desktop-session", "InstanceId": g["sessionId"], "InstanceIdSource": "get_session(\"self\").sessionId", "Assurance": "RUNTIME_OBSERVED"}
    session = {"AdapterId": "claude-desktop-session", "SessionId": g["sessionId"], "Assurance": "RUNTIME_OBSERVED"}
    facts = {"SchemaId": "rackcad-adapter-claude-desktop-session-facts/v1", "AuthState": "AUTHENTICATED", "MetadataSource": "get_session",
             "ReportedModel": {"State": "OBSERVED", "Value": g["model"]},
             "ReportedEffort": {"State": "OBSERVED", "Value": effort_client} if effort_client else {"State": "NOT_OBSERVED", "Value": None},
             "IsRunning": "TRUE" if g["isRunning"] else "FALSE", "PermissionMode": {"State": "OBSERVED", "Value": g["permissionMode"]}}
    rows = f2.requirement_rows(f2.principal_requirements("CUSTODY"), observations(effort_client))
    return f2.build(1, "claude-desktop-session", "PRINCIPAL_COORDINATOR", "CUSTODY", "PRINCIPAL_COORDINATION", actor, session, "UNKNOWN", None,
                    facts, f2.UNVERIFIED_FP, "AUTHENTICATED", rows)


def validation(p, path):
    v = f2.validate(p, path, f2.PROTO, f2.principal_requirements("CUSTODY"))
    clean, redactions, rejections = f2.sanitize(p)
    return {"PreflightId": p["PreflightId"], "File": os.path.basename(path), "FileSha256": hashlib.sha256(open(path, "rb").read()).hexdigest(),
            "AuthorityRevision": AUTH_REV, "AuthorityRevisionRole": "revisión en sombra de los textos I62 (materializados e inactivos) de I-62",
            "PowerShell": v["PowerShell"],
            "Phase1": {"Schema": f2.PROTO_REL + "/schemas/preflight.v1.schema.json", "Blob": f2.git_blob(f2.PROTO_REL + "/schemas/preflight.v1.schema.json"),
                       "Command": "Test-Json -SchemaFile", "Result": v["Phase1"]["Ok"], "Detail": v["Phase1"]["Detail"]},
            "Phase2": {"Schema": f2.PROTO_REL + "/" + p["AdapterFacts"]["SchemaRef"]["Path"], "Blob": p["AdapterFacts"]["SchemaRef"]["Blob"],
                       "Command": "Test-Json -SchemaFile", "Result": v["Phase2"]["Ok"], "Detail": v["Phase2"]["Detail"]},
            "Rules": {k: r["Ok"] for k, r in v["Rules"].items()},
            "Sanitization": {"Rule": "README §13.4", "PatternHits": sorted({h for _, h in redactions + rejections}), "Redactions": len(redactions),
                             "Result": "CLEAN" if not redactions and not rejections and clean == p else "REJECTED"},
            "Outcome": v["Outcome"],
            "ValidatedUtc": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "Z",
            "Accepted": v["Outcome"] == "VALID" and not rejections and not redactions}


def dump(obj, name):
    path = os.path.join(OUT, name)
    with open(path, "w", encoding="utf-8", newline=NL) as fh:
        fh.write(json.dumps(obj, ensure_ascii=False, indent=2) + NL)
    return path


def disposition(p):
    return {"ConfigurationStatus": p["ConfigurationStatus"], "Causes": p["Causes"], "Disposition": p["Disposition"], "StopConditions": p["StopConditions"]}


os.makedirs(OUT, exist_ok=True)
real = preflight(g["effort"])
real_path = dump(real, "preflight-1.json")
real_val = validation(real, real_path)
dump(real_val, PID + ".validation.json")

# Oracle of README §12 (copy of Proposal V14 §4.2): E1 everything = requirement → MATCH, continues; E3 effort < requirement → BELOW_REQUIRED, STOP
# P-09 (Principal); E4 effort without a source → UNKNOWN, P-10.
E1 = {"ConfigurationStatus": "MATCH", "Causes": [], "Disposition": "ELIGIBLE", "StopConditions": []}
E3 = {"ConfigurationStatus": "BELOW_REQUIRED", "Causes": ["PRINCIPAL_COORDINATION.effort"], "Disposition": "STOP", "StopConditions": ["P-09"]}
E4 = {"ConfigurationStatus": "UNKNOWN", "Causes": ["PRINCIPAL_COORDINATION.effort"], "Disposition": "NOT_ELIGIBLE", "StopConditions": ["P-10"]}

cases = []
# N1: the same real observation with the client effort one step below the requirement ('high' = Deep): the evaluator must give E3.
below = preflight("high")
cases.append({"Case": "N1-EFFORT-BELOW", "Oracle": "E3", "Mutation": "ReportedEffort xhigh → high (Deep) sobre la observación real; el resto sin cambios",
              "Expected": E3, "Observed": disposition(below), "Match": disposition(below) == E3})
# N2: a preflight that reports 'high' but claims the MATCH of the real one: the validator must reject it (C6 → P-14), so a lower effort cannot pass
# for an eligible Principal.
forged = copy.deepcopy(below)
for k in ("ConfigurationStatus", "Causes", "Disposition", "StopConditions"):
    forged[k] = copy.deepcopy(real[k])
for row in forged["Requirements"]:
    row["Status"] = "MATCH"
fpath = dump(forged, "neg-forged-match.json")
fval = f2.validate(forged, fpath, f2.PROTO, f2.principal_requirements("CUSTODY"))
os.remove(fpath)
cases.append({"Case": "N2-FORGED-MATCH", "Oracle": "README §13.2 C6", "Mutation": "effort high (Deep) con el agregado MATCH/ELIGIBLE del preflight real",
              "Expected": {"Outcome": "P-14", "C6": False}, "Observed": {"Outcome": fval["Outcome"], "C6": fval["Rules"]["C6"]["Ok"]},
              "Match": fval["Outcome"] == "P-14" and fval["Rules"]["C6"]["Ok"] is False})
# N3: the effort without a source: UNKNOWN never becomes MATCH (E4).
unknown = preflight(None)
cases.append({"Case": "N3-EFFORT-UNOBSERVED", "Oracle": "E4", "Mutation": "fila de effort sin observar (NOT_OBSERVED)",
              "Expected": E4, "Observed": disposition(unknown), "Match": disposition(unknown) == E4})

result = {
    "Scenario": "AT-1 de R1a (I-62 decisiones §105, punto 4): autoverificación del Principal A al abrir I-66",
    "Unit": "I-66", "Role": "PRINCIPAL_COORDINATOR", "Action": "CUSTODY",
    "Authority": "I-62 decisiones §105 (Coordinator) y §106 (conformidad del Owner); I-61 y WORKFLOW gobiernan I-66; este registro no autoriza nada",
    "ShadowAuthorityRevision": AUTH_REV,
    "Producer": "r1a_at1.py sobre docs/automation/evidence/I-62-F2/f2-mc.py (blob %s) de esa revisión" % f2.git_blob("docs/automation/evidence/I-62-F2/f2-mc.py"),
    "Session": {"AdapterId": "claude-desktop-session", "SessionId": g["sessionId"], "Model": g["model"], "Effort": g["effort"],
                "PermissionMode": g["permissionMode"], "ObservedUtc": utc, "WindowUtc": gs["WindowUtc"]},
    "Real": {"PreflightId": PID, "File": "preflight-1.json", "Oracle": "E1", "Expected": E1, "Observed": disposition(real),
             "Match": disposition(real) == E1, "Validation": {"Outcome": real_val["Outcome"], "Accepted": real_val["Accepted"]}},
    "Negatives": cases,
    "Limitations": [
        "los negativos son mecánicos: mutan una copia de la observación real; no hubo una sesión real con effort inferior (FX-01 ya lo demostró con cambios de effort hechos por el Owner)",
        "el contraste independiente con get_session lo hace la propia sesión observada; una sesión B puede repetirlo desde fuera con el sessionId en AT-3",
    ],
}
result["Verdict"] = "PASS" if result["Real"]["Match"] and real_val["Accepted"] and all(c["Match"] for c in cases) else "FAIL"
dump(result, "result.json")
print(result["Verdict"], PID, json.dumps(disposition(real)), real_val["Outcome"], real_val["Rules"], [(c["Case"], c["Match"]) for c in cases])
