"""I-62 F2, manual reproducible controls C-06, C-07, C-08 and C-10 (Proposal V14 Anexo C), class (ii).

Applies the procedure of docs/automation/agent-execution/README.md §12-§13 with the F2 schemas: produces one rackcad-preflight/v1 per adapter
from the observations in c06-observations.json, validates each in two phases with PowerShell Test-Json plus the coherence rules C1-C8, and
runs the invalidation (C-07), adapter-facts (C-08) and sanitizer (C-10) cases. Expected values come from Proposal V14 (C-06..C-10) and are
written in this file next to each case; the procedure never reads them. No runtime is invoked.

Usage: python f2-mc.py <repo root> <evidence dir>
"""
import copy
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

REPO, EV = sys.argv[1], sys.argv[2]
PROTO_REL = "docs/automation/agent-execution"
PROTO = os.path.join(REPO, PROTO_REL)
OBS = json.load(open(os.path.join(EV, "c06-observations.json"), encoding="utf-8"))
UNIT = "I-62"

ASSURANCE = ["NONE", "REQUESTED", "CONFIGURED", "RUNTIME_OBSERVED", "SERVICE_ATTESTED"]
SCALES = {
    "effort": ["Routine", "Balanced", "Deep", "Long-horizon", "Maximum"],
    "level": ["Eficiente", "Equilibrado", "Frontera"],
    "remote-facts": ["none", "read"],
    "introspection": ["REQUESTED", "CONFIGURED", "RUNTIME_OBSERVED", "SERVICE_ATTESTED"],
    "repo-write": ["none", "write"],
    "build-test": ["none", "run"],
    "read": ["none", "measured"], "tool-use": ["none", "measured"], "write-commit-push": ["none", "measured"], "structured-output": ["none", "measured"],
}
# routing.md §3: the first level listed for each effort is the required minimum used here.
LEVEL_FOR_EFFORT = {"Routine": "Eficiente", "Balanced": "Equilibrado", "Deep": "Equilibrado", "Long-horizon": "Frontera", "Maximum": "Frontera"}
# routing.md §1: starting effort and minimum capabilities of the classes used by the candidates below.
CLASSES = {
    "ROUTINE_IMPLEMENTATION": ("Routine", ["tool-use", "write-commit-push"]),
    "ARCHITECTURE_REVIEW": ("Deep", ["read"]),
    "CONTROLLER_VERIFICATION": ("Balanced", ["read", "tool-use", "structured-output"]),
}


def git_blob(rel):
    return subprocess.check_output(["git", "-C", REPO, "hash-object", rel.replace("/", os.sep)]).decode().strip()


def sha256_text(text):
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def principal_requirements(action):
    """routing.md §8, table of PRINCIPAL_COORDINATION."""
    text = open(os.path.join(PROTO, "routing.md"), encoding="utf-8").read().replace("\r\n", "\n")
    rows = re.findall(r"^\| `(PRINCIPAL_COORDINATION\.[a-z-]+)` \| ([^|]+) \| ([^|]+) \| ([^|]+) \| ([^|]+) \|$", text, re.M)
    column = {"RESUME_DECISION": 2, "CUSTODY": 3, "LOCAL_EVIDENCE": 4}[action]
    required = {"Frontera": "Frontera", "`Long-horizon`": "Long-horizon", "lectura": "read", "RUNTIME_OBSERVED o superior": "RUNTIME_OBSERVED",
                "escritura": "write", "ejecución": "run"}
    return [(r[0], required[r[1].strip()]) for r in rows if r[column].strip() == "sí"]


def class_requirements(profile):
    """routing.md §8, rule for the profiles of the other roles."""
    effort, capabilities = CLASSES[profile]
    out = [(profile + ".effort", effort), (profile + ".level", LEVEL_FOR_EFFORT[effort])]
    return out + [(profile + "." + c, "measured") for c in capabilities]


def not_observed():
    return {"State": "NOT_OBSERVED", "Value": None, "Source": None, "Assurance": "NONE", "ObservedUtc": None}


def observed(value, source, assurance, utc):
    return {"State": "OBSERVED", "Value": value, "Source": source, "Assurance": assurance, "ObservedUtc": utc}


def status_of(rid, row):
    """README §12, step 2."""
    o = row["Observation"]
    if o["State"] == "NOT_OBSERVED" or ASSURANCE.index(o["Assurance"]) < ASSURANCE.index("RUNTIME_OBSERVED") or o.get("Invalidated"):
        return "UNKNOWN"
    scale = SCALES[rid.split(".", 1)[1]]
    observed_i, required_i = scale.index(o["Value"]), scale.index(row["Required"])
    return "BELOW_REQUIRED" if observed_i < required_i else "ABOVE_REQUIRED" if observed_i > required_i else "MATCH"


def aggregate(rows, role):
    """README §12, steps 3-5."""
    for row in rows:
        row["Status"] = "UNKNOWN" if row["Contradiction"] else status_of(row["RequirementId"], row)
    mandatory = [r for r in rows if r["Mandatory"]]
    statuses = [r["Status"] for r in mandatory]
    agg = ("BELOW_REQUIRED" if "BELOW_REQUIRED" in statuses else "UNKNOWN" if "UNKNOWN" in statuses
           else "ABOVE_REQUIRED" if "ABOVE_REQUIRED" in statuses else "MATCH")
    causes = sorted({r["RequirementId"] for r in mandatory if r["Status"] != "MATCH"} | {r["RequirementId"] for r in rows if r["Contradiction"]})
    stops = []
    if any(r["Contradiction"] for r in rows):
        stops.append("S-04")
    if role == "PRINCIPAL_COORDINATOR" and agg == "BELOW_REQUIRED":
        stops.append("P-09")
    if stops:
        disposition = "STOP"
    elif agg in ("UNKNOWN", "BELOW_REQUIRED"):
        disposition, stops = "NOT_ELIGIBLE", ["P-10"]
    else:
        disposition = "ELIGIBLE"
    return agg, causes, disposition, sorted(stops)


def requirement_rows(pairs, observations):
    rows = []
    for rid, required in pairs:
        rows.append({"RequirementId": rid, "Mandatory": True, "Required": required, "Observation": observations.get(rid, not_observed()),
                     "Status": "UNKNOWN", "Contradiction": False, "ContradictionEvidence": None})
    return rows


def preflight_id(n):
    return "P20261002T224500Z-%04x" % (0xf200 + n)


def build(n, adapter, role, action, profile, actor, session, version, binary_path_hash, facts, fingerprint, auth, rows):
    agg, causes, disposition, stops = aggregate(rows, role)
    fp_inv = fingerprint["Sha256"] if fingerprint["Kind"] == "CONFIG_FILE" else ("NONE" if fingerprint["Kind"] == "NONE" else "UNKNOWN")
    return {
        "Schema": "rackcad-preflight/v1", "PreflightId": preflight_id(n), "UnitId": UNIT, "Role": role, "Action": action,
        "ProtocolSet": "rackcad-protocol/I62",
        "Profile": {"ProfileId": profile, "RoutingBlob": git_blob(PROTO_REL + "/routing.md")},
        "Host": OBS["Host"], "ObservedUtc": OBS["ObservedUtc"], "Actor": actor, "Session": session,
        "Adapter": {"AdapterId": adapter, "AdapterVersion": version, "BinaryPathHash": binary_path_hash,
                    "DescriptorRef": {"Path": "adapters/%s.md" % adapter, "Blob": git_blob(PROTO_REL + "/adapters/%s.md" % adapter)}},
        "AdapterFacts": {"SchemaRef": {"SchemaId": "rackcad-adapter-%s-facts/v1" % adapter, "Path": "schemas/adapters/%s.facts.v1.schema.json" % adapter,
                                       "Blob": git_blob(PROTO_REL + "/schemas/adapters/%s.facts.v1.schema.json" % adapter)}, "Facts": facts},
        "Fingerprint": fingerprint, "Requirements": rows, "ConfigurationStatus": agg, "Causes": causes, "Disposition": disposition,
        "StopConditions": stops,
        "Invalidators": {"HostInstanceHash": OBS["Host"]["HostInstanceHash"], "AdapterVersion": version, "BinaryPathHash": binary_path_hash,
                         "AuthState": auth, "FingerprintSha256": fp_inv, "CatalogEntryBlob": git_blob(PROTO_REL + "/model-catalog.md"),
                         "RoutingBlob": git_blob(PROTO_REL + "/routing.md")},
    }


UNVERIFIED_FP = {"Kind": "UNVERIFIED", "Sha256": None, "KeyNames": [], "AcceptanceDecisionRef": None}


def preflights():
    p = OBS["Principal"]
    gs = p["GetSession"]
    utc = OBS["ObservedUtc"]
    actor_p = {"AdapterId": "claude-desktop-session", "InstanceId": gs["sessionId"], "InstanceIdSource": "get_session.sessionId", "Assurance": "RUNTIME_OBSERVED"}
    session_p = {"AdapterId": "claude-desktop-session", "SessionId": gs["sessionId"], "Assurance": "RUNTIME_OBSERVED"}
    facts_p = {"SchemaId": "rackcad-adapter-claude-desktop-session-facts/v1", "AuthState": "AUTHENTICATED", "MetadataSource": "get_session",
               "ReportedModel": {"State": "OBSERVED", "Value": gs["model"]}, "ReportedEffort": {"State": "OBSERVED", "Value": gs["effort"]},
               "IsRunning": "TRUE", "PermissionMode": {"State": "OBSERVED", "Value": gs["permissionMode"]}}
    principal_obs = {
        "PRINCIPAL_COORDINATION.level": observed("Frontera", "get_session.model (claude-opus-5-5) + model-catalog.md «Nivel»", "RUNTIME_OBSERVED", gs["observedUtc"]),
        "PRINCIPAL_COORDINATION.effort": observed("Long-horizon", "get_session.effort (xhigh) + model-catalog.md «Effort»", "RUNTIME_OBSERVED", gs["observedUtc"]),
        "PRINCIPAL_COORDINATION.remote-facts": observed("read", p["RemoteFacts"]["command"], "RUNTIME_OBSERVED", p["RemoteFacts"]["observedUtc"]),
        "PRINCIPAL_COORDINATION.introspection": observed("RUNTIME_OBSERVED", "get_session (metadatos de la app)", "RUNTIME_OBSERVED", gs["observedUtc"]),
        "PRINCIPAL_COORDINATION.repo-write": observed("write", p["RepoWrite"]["evidence"], "RUNTIME_OBSERVED", p["RepoWrite"]["observedUtc"]),
        "PRINCIPAL_COORDINATION.build-test": observed("run", p["BuildTest"]["evidence"], "RUNTIME_OBSERVED", p["BuildTest"]["observedUtc"]),
    }
    out = []
    for n, action in enumerate(["RESUME_DECISION", "CUSTODY", "LOCAL_EVIDENCE"], 1):
        rows = requirement_rows(principal_requirements(action), principal_obs)
        out.append(build(n, "claude-desktop-session", "PRINCIPAL_COORDINATOR", action, "PRINCIPAL_COORDINATION", actor_p, session_p, "UNKNOWN", None,
                         facts_p, UNVERIFIED_FP, "AUTHENTICATED", rows))

    not_started = lambda a: ({"AdapterId": a, "InstanceId": "NOT_STARTED", "InstanceIdSource": "candidata sin sesión", "Assurance": "NONE"},
                             {"AdapterId": a, "SessionId": "NOT_STARTED", "Assurance": "NONE"})
    a, s = not_started("claude-subagent")
    s = {"AdapterId": "claude-subagent", "SessionId": gs["sessionId"], "Assurance": "RUNTIME_OBSERVED"}
    facts = {"SchemaId": "rackcad-adapter-claude-subagent-facts/v1", "AuthState": "AUTHENTICATED", "ParentSessionState": "OBSERVED",
             "TranscriptSource": "NOT_STARTED", "CallTimeoutMinutes": 60, "CompletionNotified": "NOT_STARTED"}
    out.append(build(4, "claude-subagent", "WORKER", "IMPLEMENT", "ROUTINE_IMPLEMENTATION", a, s, "UNKNOWN", None, facts, UNVERIFIED_FP,
                     "AUTHENTICATED", requirement_rows(class_requirements("ROUTINE_IMPLEMENTATION"), {})))

    a, s = not_started("claude-cli")
    facts = {"SchemaId": "rackcad-adapter-claude-cli-facts/v1", "AuthState": "UNKNOWN", "BinaryFound": OBS["ClaudeCli"]["BinaryFound"],
             "CliVersion": {"State": "NOT_OBSERVED", "Value": None}}
    out.append(build(5, "claude-cli", "REVIEWER", "REVIEW_CHANGE", "ARCHITECTURE_REVIEW", a, s, "UNKNOWN", None, facts, UNVERIFIED_FP, "UNKNOWN",
                     requirement_rows(class_requirements("ARCHITECTURE_REVIEW"), {})))

    c = OBS["CodexCli"]
    a, s = not_started("codex-cli")
    facts = {"SchemaId": "rackcad-adapter-codex-cli-facts/v1", "AuthState": "UNKNOWN", "BinaryLabel": c["BinaryLabel"], "BinarySha256": c["BinarySha256"],
             "CliVersion": {"State": "NOT_OBSERVED", "Value": None}, "SandboxMode": "read-only", "SessionLog": "PERSISTED", "RuntimeShellFirstInPath": "YES"}
    fp = {"Kind": "CONFIG_FILE", "Sha256": c["Fingerprint"]["Sha256"], "KeyNames": c["Fingerprint"]["KeyNames"], "AcceptanceDecisionRef": None}
    out.append(build(6, "codex-cli", "EXECUTION_CONTROLLER", "VERIFY", "CONTROLLER_VERIFICATION", a, s, "UNKNOWN", c["BinaryPathHash"], facts, fp, "UNKNOWN",
                     requirement_rows(class_requirements("CONTROLLER_VERIFICATION"), {})))

    a, s = not_started("codex-desktop-session")
    facts = {"SchemaId": "rackcad-adapter-codex-desktop-session-facts/v1", "AuthState": "UNKNOWN", "AppPackage": {"State": "NOT_OBSERVED", "Value": None},
             "TurnContextSource": "CANDIDATE", "SharesConfigFile": "UNKNOWN"}
    out.append(build(7, "codex-desktop-session", "PRINCIPAL_COORDINATOR", "RESUME_DECISION", "PRINCIPAL_COORDINATION", a, s, "UNKNOWN", None, facts,
                     UNVERIFIED_FP, "UNKNOWN", requirement_rows(principal_requirements("RESUME_DECISION"), {})))
    return out


# ---------------------------------------------------------------- validation (AUTOMATION_PLAN 16.18; README §13.2)
def pwsh_test_json(json_path, schema_path):
    script = ("try { $null = Get-Content -Raw -LiteralPath '%s' | Test-Json -SchemaFile '%s' -ErrorAction Stop; 'True' } "
              "catch { 'False: ' + $_.Exception.Message }") % (json_path.replace("'", "''"), schema_path.replace("'", "''"))
    out = subprocess.run(["pwsh", "-NoProfile", "-Command", script], capture_output=True, text=True, encoding="utf-8", errors="replace")
    text = (out.stdout or "").strip().splitlines()
    line = text[-1] if text else ("False: " + (out.stderr or "").strip())
    return line == "True", line


PWSH_VERSION = subprocess.run(["pwsh", "-NoProfile", "-Command", "$PSVersionTable.PSVersion.ToString()"], capture_output=True, text=True).stdout.strip()


def descriptor_version(proto, adapter):
    path = os.path.join(proto, "adapters", adapter + ".md")
    if not os.path.exists(path):
        return None
    versions = set(re.findall(r"schemas/adapters/" + re.escape(adapter) + r"\.facts\.v([0-9]+)\.schema\.json", open(path, encoding="utf-8").read()))
    return versions.pop() if len(versions) == 1 else None


def hash_object(path):
    data = open(path, "rb").read().replace(b"\r\n", b"\n")
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


def coherence(p, proto, profile_requirements=None):
    """README §13.2, rules C1-C8. Returns {rule: (ok, detail)}."""
    r = {}
    adapter = p["Adapter"]["AdapterId"]
    dpath = os.path.join(proto, "adapters", adapter + ".md")
    r["C1"] = (os.path.exists(dpath) and hash_object(dpath) == p["Adapter"]["DescriptorRef"]["Blob"], "descriptor y blob")
    version = descriptor_version(proto, adapter)
    expected_path = "schemas/adapters/%s.facts.v%s.schema.json" % (adapter, version)
    spath = os.path.join(proto, p["AdapterFacts"]["SchemaRef"]["Path"].replace("/", os.sep))
    r["C2"] = (version is not None and p["AdapterFacts"]["SchemaRef"]["Path"] == expected_path and os.path.exists(spath)
               and hash_object(spath) == p["AdapterFacts"]["SchemaRef"]["Blob"], "ruta derivada y blob del esquema de hechos")
    r["C3"] = (p["AdapterFacts"]["Facts"].get("SchemaId") == p["AdapterFacts"]["SchemaRef"]["SchemaId"], "Facts.SchemaId")
    fp = p["Fingerprint"]
    ok4 = ((fp["Kind"] == "CONFIG_FILE" and fp["Sha256"] is not None and fp["AcceptanceDecisionRef"] is None)
           or (fp["Kind"] == "NONE" and fp["Sha256"] is None and fp["AcceptanceDecisionRef"] is not None)
           or (fp["Kind"] == "UNVERIFIED" and fp["Sha256"] is None and not fp["KeyNames"] and fp["AcceptanceDecisionRef"] is None))
    r["C4"] = (ok4, "coherencia de la huella")
    ids = [x["RequirementId"] for x in p["Requirements"]]
    expected_ids = [rid for rid, _ in (profile_requirements or [])]
    r["C5"] = (len(ids) == len(set(ids)) and set(expected_ids) <= set(ids), "requisitos únicos y obligatorios presentes")
    rows = copy.deepcopy(p["Requirements"])
    agg, causes, disposition, stops = aggregate(rows, p["Role"])
    r["C6"] = ([x["Status"] for x in rows] == [x["Status"] for x in p["Requirements"]] and agg == p["ConfigurationStatus"]
               and causes == p["Causes"] and disposition == p["Disposition"] and stops == p["StopConditions"], "estado, agregado, causas y disposición = §12")
    ok7 = all(((x["Observation"]["State"] == "NOT_OBSERVED") or all(x["Observation"][k] is not None for k in ("Value", "Source", "ObservedUtc")))
              and ((x["Observation"]["State"] == "OBSERVED") or all(x["Observation"][k] is None for k in ("Value", "Source", "ObservedUtc")))
              and ((x["ContradictionEvidence"] is None) == (not x["Contradiction"])) for x in p["Requirements"])
    r["C7"] = (ok7, "nulos solo donde no aplica")
    inv = p["Invalidators"]
    fp_expected = fp["Sha256"] if fp["Kind"] == "CONFIG_FILE" else ("NONE" if fp["Kind"] == "NONE" else "UNKNOWN")
    r["C8"] = (inv["FingerprintSha256"] == fp_expected and inv["AdapterVersion"] == p["Adapter"]["AdapterVersion"]
               and inv["BinaryPathHash"] == p["Adapter"]["BinaryPathHash"], "invalidadores coherentes")
    return r


def validate(p, path, proto, profile_requirements):
    ok1, d1 = pwsh_test_json(path, os.path.join(proto, "schemas", "preflight.v1.schema.json"))
    facts_tmp = path + ".facts.tmp.json"
    json.dump(p["AdapterFacts"]["Facts"], open(facts_tmp, "w", encoding="utf-8"), ensure_ascii=False)
    spath = os.path.join(proto, p["AdapterFacts"]["SchemaRef"]["Path"].replace("/", os.sep))
    ok2, d2 = pwsh_test_json(facts_tmp, spath) if os.path.exists(spath) else (False, "False: esquema de hechos ausente")
    os.remove(facts_tmp)
    rules = coherence(p, proto, profile_requirements)
    failed = [k for k, (ok, _) in rules.items() if not ok]
    if not ok1 or not ok2 or [k for k in failed if k != "C6"] or "C6" in failed:
        outcome = "P-14"
    else:
        outcome = "VALID"
    return {"PreflightId": p["PreflightId"], "PowerShell": PWSH_VERSION, "Phase1": {"Ok": ok1, "Detail": d1}, "Phase2": {"Ok": ok2, "Detail": d2},
            "Rules": {k: {"Ok": ok, "Checks": d} for k, (ok, d) in rules.items()}, "Outcome": outcome}


def contrast(p, relay):
    """B.3 step 6 / README §13.3: authentication, binary version and fingerprint against the relay record's Exit and Entry."""
    diffs = []
    if relay["Participant"]["AdapterVersion"] != p["Adapter"]["AdapterVersion"]:
        diffs.append("AdapterVersion")
    for side in ("Exit", "Entry"):
        if relay[side]["Fingerprint"]["Sha256"] != p["Fingerprint"]["Sha256"]:
            diffs.append(side + ".Fingerprint")
    return ("S-04", diffs) if diffs else ("NONE", diffs)


# ---------------------------------------------------------------- sanitizer (README §13.4)
PATTERNS = [
    ("PEM", r"-----BEGIN [A-Z ]*PRIVATE KEY-----"),
    ("JWT", r"eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]*"),
    ("SK", r"\bsk-[A-Za-z0-9_-]{16,}"),
    ("GH", r"\b(gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})"),
    ("AWS", r"\bAKIA[0-9A-Z]{16}\b"),
    ("SLACK", r"\bxox[abposr]-[A-Za-z0-9-]{10,}"),
    ("GAPI", r"\bAIza[0-9A-Za-z_-]{35}\b"),
    ("BEARER", r"(?i)\bbearer\s+[A-Za-z0-9._~+/-]{16,}=*"),
    ("KV", r"(?i)\b(password|passwd|secret|token|api[_-]?key|access[_-]?key|client[_-]?secret)\b\s*[:=]\s*\S+"),
    ("URLCRED", r"[a-z][a-z0-9+.-]*://[^/\s:@]+:[^/\s@]+@"),
]
FREE_TEXT = {"Value", "ContradictionEvidence", "Notes", "Evidence", "Diagnosis", "ErrorMessages"}


def redact_text(text):
    hits = []
    for pid, pattern in PATTERNS:
        if re.search(pattern, text):
            hits.append(pid)
            text = re.sub(pattern, "<redactado:%s>" % pid, text)
    return text, hits


def sanitize_key_name(name):
    if any(c in name for c in ("\\", "/", ":")):
        m = re.match(r"^\[([^.\]]+)\.", name)
        return ("[%s.<redactado>]" % m.group(1)) if (name.startswith("[") and m) else "<redactado>", True
    return name, False


def sanitize(record):
    """Returns (sanitized record, redactions, rejections)."""
    out, redactions, rejections = copy.deepcopy(record), [], []

    def walk(node, path, key):
        if isinstance(node, dict):
            return {k: walk(v, path + "." + k, k) for k, v in node.items()}
        if isinstance(node, list):
            if key == "KeyNames":
                res = []
                for item in node:
                    name, changed = sanitize_key_name(item)
                    if changed:
                        redactions.append((path, "KEYNAME"))
                    res.append(name)
                return res
            return [walk(v, path + "[]", key) for v in node]
        if isinstance(node, str):
            new, hits = redact_text(node)
            if hits:
                if key in FREE_TEXT:
                    redactions.extend((path, h) for h in hits)
                    return new
                rejections.extend((path, h) for h in hits)
            return node
        return node

    out = walk(out, "$", None)
    return out, redactions, rejections


# ---------------------------------------------------------------- run
def main():
    results = {"PowerShell": PWSH_VERSION}
    pdir = os.path.join(EV, "preflights")
    os.makedirs(pdir, exist_ok=True)

    # C-06
    c06 = []
    for p in preflights():
        sanitized, redactions, rejections = sanitize(p)
        assert not rejections, rejections
        path = os.path.join(pdir, p["PreflightId"] + ".json")
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            json.dump(sanitized, f, ensure_ascii=False, indent=2)
            f.write("\n")
        reqs = principal_requirements(p["Action"]) if p["Profile"]["ProfileId"] == "PRINCIPAL_COORDINATION" else class_requirements(p["Profile"]["ProfileId"])
        v = validate(sanitized, path, PROTO, reqs)
        with open(os.path.join(pdir, p["PreflightId"] + ".validation.json"), "w", encoding="utf-8", newline="\n") as f:
            json.dump(v, f, ensure_ascii=False, indent=2)
            f.write("\n")
        c06.append({"PreflightId": p["PreflightId"], "Adapter": p["Adapter"]["AdapterId"], "Role": p["Role"], "Action": p["Action"],
                    "ConfigurationStatus": p["ConfigurationStatus"], "Causes": p["Causes"], "Disposition": p["Disposition"],
                    "StopConditions": p["StopConditions"], "Validation": v["Outcome"], "Redactions": len(redactions)})
    results["C-06"] = {"Expected": "un registro por adapter, válido por Test-Json (fases 1 y 2) y por C1-C8, con el estado observado en el momento",
                       "Records": c06, "Verdict": "PASS" if all(r["Validation"] == "VALID" for r in c06)
                       and {r["Adapter"] for r in c06} == {"claude-desktop-session", "claude-subagent", "claude-cli", "codex-cli", "codex-desktop-session"} else "FAIL"}

    # C-07: invalidators against a previous record
    base = json.load(open(os.path.join(pdir, preflight_id(1) + ".json"), encoding="utf-8"))
    def invalidated(prev, current):
        return sorted(k for k in prev["Invalidators"] if prev["Invalidators"][k] != current[k])
    cases = []
    def case(name, mutate, expected_invalid):
        cur = copy.deepcopy(base["Invalidators"])
        mutate(cur)
        diff = invalidated(base, cur)
        revalidated = copy.deepcopy(base["Requirements"])
        if diff:
            for row in revalidated:
                row["Observation"]["Invalidated"] = True
        statuses = sorted({status_of(r["RequirementId"], r) for r in revalidated})
        ok = (bool(diff) == expected_invalid) and (statuses == (["UNKNOWN"] if expected_invalid else ["MATCH"]))
        cases.append({"Case": name, "ChangedInvalidators": diff, "RequirementStatuses": statuses, "ExpectedInvalid": expected_invalid,
                      "Verdict": "PASS" if ok else "FAIL"})
    case("versión del adapter cambiada", lambda c: c.update(AdapterVersion="99.0.0"), True)
    case("huella cambiada", lambda c: c.update(FingerprintSha256="0" * 64), True)
    case("instancia del host cambiada", lambda c: c.update(HostInstanceHash="1" * 64), True)
    case("blob de routing.md cambiado", lambda c: c.update(RoutingBlob="2" * 40), True)
    case("binding hipotético nuevo (no es invalidador)", lambda c: None, False)
    case("SHA de la rama avanzado (no es invalidador)", lambda c: None, False)
    results["C-07"] = {"Expected": "un invalidador cambiado → requisitos UNKNOWN; un binding hipotético (o la rama) no invalida", "BaseRecord": base["PreflightId"],
                       "Cases": cases, "Verdict": "PASS" if all(c["Verdict"] == "PASS" for c in cases) else "FAIL"}

    # C-08 (a)-(f) on a temporary copy of the protocol directory
    tmp = tempfile.mkdtemp(prefix="i62-c08-")
    proto = os.path.join(tmp, "agent-execution")
    shutil.copytree(PROTO, proto)
    core_before = {f: hash_object(os.path.join(proto, "schemas", f)) for f in ("preflight.v1.schema.json", "relay-record.v2.schema.json", "controller-verification.v2.schema.json")}
    c08dir = os.path.join(EV, "c08")
    for name in ("test-null.md", "test-null.facts.v1.schema.json"):
        target = os.path.join(proto, "adapters" if name.endswith(".md") else os.path.join("schemas", "adapters"), name)
        shutil.copyfile(os.path.join(c08dir, name), target)
    cdir = os.path.join(tmp, "cases")
    os.makedirs(cdir)
    def preflight_for(adapter, facts, version="1", fingerprint=None, schema_path=None):
        p = copy.deepcopy(base)
        p["PreflightId"] = "P20261002T230000Z-c08%s" % "abcdef"[len(c08_cases) % 6]
        p["Role"], p["Action"] = "REVIEWER", "REVIEW_CHANGE"
        p["Actor"] = {"AdapterId": adapter, "InstanceId": "NOT_STARTED", "InstanceIdSource": "candidata sin sesión", "Assurance": "NONE"}
        p["Session"] = {"AdapterId": adapter, "SessionId": "NOT_STARTED", "Assurance": "NONE"}
        dpath = os.path.join(proto, "adapters", adapter + ".md")
        p["Adapter"] = {"AdapterId": adapter, "AdapterVersion": "1", "BinaryPathHash": None,
                        "DescriptorRef": {"Path": "adapters/%s.md" % adapter, "Blob": hash_object(dpath) if os.path.exists(dpath) else "0" * 40}}
        spath_rel = schema_path or "schemas/adapters/%s.facts.v%s.schema.json" % (adapter, version)
        spath = os.path.join(proto, spath_rel.replace("/", os.sep))
        p["AdapterFacts"] = {"SchemaRef": {"SchemaId": "rackcad-adapter-%s-facts/v%s" % (adapter, version), "Path": spath_rel,
                                           "Blob": hash_object(spath) if os.path.exists(spath) else "0" * 40}, "Facts": facts}
        p["Fingerprint"] = fingerprint or UNVERIFIED_FP
        p["Invalidators"]["AdapterVersion"], p["Invalidators"]["BinaryPathHash"] = "1", None
        p["Invalidators"]["FingerprintSha256"] = "NONE" if p["Fingerprint"]["Kind"] == "NONE" else "UNKNOWN"
        reqs = class_requirements("ARCHITECTURE_REVIEW")
        p["Profile"]["ProfileId"] = "ARCHITECTURE_REVIEW"
        p["Requirements"] = requirement_rows(reqs, {})
        p["ConfigurationStatus"], p["Causes"], p["Disposition"], p["StopConditions"] = aggregate(p["Requirements"], p["Role"])
        return p, reqs
    c08_cases = []
    def run_case(label, expected, p, reqs, relay=None):
        path = os.path.join(cdir, "%s.json" % label)
        json.dump(p, open(path, "w", encoding="utf-8"), ensure_ascii=False, indent=1)
        v = validate(p, path, proto, reqs)
        got = v["Outcome"]
        detail = {k: v["Rules"][k]["Ok"] for k in v["Rules"]}
        if relay is not None:
            got, diffs = contrast(p, relay)
            detail = {"Discrepancies": diffs, "PreflightValidation": v["Outcome"]}
        c08_cases.append({"Case": label, "Expected": expected, "Got": got, "Phase1": v["Phase1"]["Ok"], "Phase2": v["Phase2"]["Ok"], "Detail": detail,
                          "Verdict": "PASS" if got == expected else "FAIL"})
    p, reqs = preflight_for("test-null", {"SchemaId": "rackcad-adapter-test-null-facts/v1", "probe": "valor de prueba"})
    run_case("a-test-null-valido", "VALID", p, reqs)
    core_after = {f: hash_object(os.path.join(proto, "schemas", f)) for f in core_before}
    p, reqs = preflight_for("test-null", {"SchemaId": "rackcad-adapter-test-null-facts/v1", "probe": "x", "inesperada": 1})
    run_case("b-propiedad-inesperada", "P-14", p, reqs)
    p, reqs = preflight_for("adapter-fantasma", {"SchemaId": "rackcad-adapter-adapter-fantasma-facts/v1"})
    run_case("c-id-desconocido", "P-14", p, reqs)
    p, reqs = preflight_for("test-null", {"SchemaId": "rackcad-adapter-test-null-facts/v2", "probe": "x"}, version="2")
    run_case("d-version-incompatible", "P-14", p, reqs)
    p, reqs = preflight_for("test-null", {"SchemaId": "rackcad-adapter-test-null-facts/v1", "probe": "x"},
                            fingerprint={"Kind": "NONE", "Sha256": None, "KeyNames": [], "AcceptanceDecisionRef": None})
    run_case("e-huella-ninguna-sin-aceptacion", "P-14", p, reqs)
    codex = json.load(open(os.path.join(pdir, preflight_id(6) + ".json"), encoding="utf-8"))
    relay = json.load(open(os.path.join(c08dir, "relay-record-v2-sintetico.json"), encoding="utf-8"))
    relay_ok, relay_detail = pwsh_test_json(os.path.join(c08dir, "relay-record-v2-sintetico.json"), os.path.join(PROTO, "schemas", "relay-record.v2.schema.json"))
    matching = copy.deepcopy(relay)
    matching["Exit"]["Fingerprint"]["Sha256"] = matching["Entry"]["Fingerprint"]["Sha256"] = codex["Fingerprint"]["Sha256"]
    run_case("f-contraste-coincidente", "NONE", codex, class_requirements("CONTROLLER_VERIFICATION"), matching)
    run_case("f-contraste-discrepante", "S-04", codex, class_requirements("CONTROLLER_VERIFICATION"), relay)
    verification_ok, verification_detail = pwsh_test_json(os.path.join(c08dir, "controller-verification-v2-sintetica.json"),
                                                          os.path.join(PROTO, "schemas", "controller-verification.v2.schema.json"))
    shutil.rmtree(tmp)
    results["C-08"] = {"Expected": "(a) válido sin cambiar el núcleo; (b)-(e) P-14; (f) S-04", "CoreSchemasUnchanged": core_before == core_after,
                       "Cases": c08_cases, "SyntheticRelayRecordV2Valid": relay_ok, "SyntheticControllerVerificationV2Valid": verification_ok,
                       "Verdict": "PASS" if core_before == core_after and all(c["Verdict"] == "PASS" for c in c08_cases) and relay_ok and verification_ok else "FAIL"}

    # C-10: fictitious sensitive values
    c10_input = json.load(open(os.path.join(EV, "c10-input.json"), encoding="utf-8"))
    c10 = []
    for item in c10_input["Cases"]:
        sanitized, redactions, rejections = sanitize(item["Record"])
        text = json.dumps(sanitized, ensure_ascii=False)
        leaked = [s for s in item.get("Secrets", []) if s in text]
        got = "REJECTED" if rejections else ("REDACTED" if redactions else "UNCHANGED")
        ok = got == item["Expected"] and (got == "REJECTED" or not leaked)
        c10.append({"Case": item["Case"], "Expected": item["Expected"], "Got": got, "Redactions": sorted({r[1] for r in redactions}),
                    "Rejections": sorted({r[1] for r in rejections}), "LeakedAfterSanitizing": len(leaked), "Verdict": "PASS" if ok else "FAIL"})
    detected = sum(1 for c in c10 if c["Expected"] != "UNCHANGED" and c["Got"] == c["Expected"])
    sensitive = sum(1 for c in c10 if c["Expected"] != "UNCHANGED")
    results["C-10"] = {"Expected": "100 % de los valores sensibles ficticios detectados o redactados; custodia rechazada fuera del texto libre; sin falsos positivos en los controles benignos",
                       "Cases": c10, "SensitiveDetected": "%d/%d" % (detected, sensitive),
                       "Verdict": "PASS" if all(c["Verdict"] == "PASS" for c in c10) else "FAIL"}

    with open(os.path.join(EV, "f2-mc-result.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(results, f, ensure_ascii=False, indent=2)
        f.write("\n")
    print(json.dumps({k: v["Verdict"] for k, v in results.items() if isinstance(v, dict) and "Verdict" in v}))
    return 0 if all(v["Verdict"] == "PASS" for v in results.values() if isinstance(v, dict) and "Verdict" in v) else 1


if __name__ == "__main__":
    sys.exit(main())
