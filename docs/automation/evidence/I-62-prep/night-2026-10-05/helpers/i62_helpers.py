"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. I-62 night order §8: deterministic helpers for P1, P2, P3, P5 and P6 (stdlib + git CLI).

These helpers EXTRACT FACTS. They never sign EXECUTION_VERIFIED, GATE PASS or any disposition; the Controller keeps judgement, independence and
verdict (P3). They are not a role, service, daemon or MechanicalVerifier authority. Known gaps are reported in the output, never resolved by new
semantics (case folding, symlink resolution, glob interpretation).
"""
import hashlib
import json
import os
import re
import subprocess
import xml.etree.ElementTree as ET

HEX40 = re.compile(r"^[0-9a-f]{40}$")
VERDICT_KEYS = {"Classification", "Disposition", "VerifiedSha", "GatePass", "Verdict", "FailureClass"}
VERDICT_VALUES = {"EXECUTION_VERIFIED", "EXECUTION_REWORK_REQUIRED", "EXECUTION_BLOCKED", "GATE PASS", "GATE_PASS", "AGREED", "CONFORMING"}


def _git(repo, *args):
    return subprocess.run(["git", "-C", repo] + list(args), capture_output=True, text=True, encoding="utf-8", errors="replace")


def _load(obj_or_path):
    if isinstance(obj_or_path, dict):
        return obj_or_path, "inline"
    with open(obj_or_path, encoding="utf-8") as f:
        return json.load(f), obj_or_path


# ------------------------------------------------------------------ P1: identity from the structured artifact
def p1_identity(handoff, repo, delegation=None):
    """CurrentSha parsed from the structured handoff and checked against Git. There is deliberately NO prompt parameter: a narrative SHA can
    never rescue an invalid handoff, and the prompt SHA is never substituted."""
    data, src = _load(handoff)
    out = {"Helper": "P1", "Source": "%s#CurrentSha" % src, "CurrentSha": data.get("CurrentSha"), "Facts": {}, "Failures": []}
    sha = data.get("CurrentSha")
    if not isinstance(sha, str) or not HEX40.match(sha):
        out["Failures"].append("CurrentSha ausente o no es un SHA de 40 hex en minúscula")
        return dict(out, Status="FAIL")
    r = _git(repo, "cat-file", "-t", sha)
    out["Facts"]["ObjectType"] = r.stdout.strip() if r.returncode == 0 else None
    if r.returncode != 0 or r.stdout.strip() != "commit":
        out["Failures"].append("CurrentSha no es un commit existente en el repositorio")
        return dict(out, Status="FAIL")
    base = (delegation or {}).get("BaseSha") or data.get("BaseSha")
    if base:
        anc = _git(repo, "merge-base", "--is-ancestor", base, sha).returncode
        out["Facts"]["BaseShaIsAncestor"] = anc == 0
        if anc != 0:
            out["Failures"].append("BaseSha no es ancestro de CurrentSha")
    red = data.get("RedSha")
    if red and base:
        between = _git(repo, "merge-base", "--is-ancestor", base, red).returncode == 0 and _git(repo, "merge-base", "--is-ancestor", red, sha).returncode == 0
        out["Facts"]["RedShaBetween"] = between
        if not between:
            out["Failures"].append("RedSha no está entre BaseSha y CurrentSha")
    return dict(out, Status="FAIL" if out["Failures"] else "PASS")


# ------------------------------------------------------------------ P2: scope against the concrete delegation
def _entry_matches(path, entry):
    return path.startswith(entry) if entry.endswith("/") else path == entry


def p2_scope(repo, base, current, delegation):
    """Changed paths of base..current (no rename detection: a rename is a delete plus an add; deletes and mode changes count as writes) compared
    with AllowedWriteScope / ForbiddenWriteScope of THE delegation being verified. Forbidden wins. Exact, case-sensitive comparison (Git paths)."""
    d, src = _load(delegation)
    allowed, forbidden = d.get("AllowedWriteScope"), d.get("ForbiddenWriteScope") or []
    out = {"Helper": "P2", "Source": src, "Range": "%s..%s" % (base, current), "Paths": [], "Gaps": [], "Failures": []}
    if not isinstance(allowed, list) or not allowed:
        out["Failures"].append("la delegación no trae AllowedWriteScope")
        return dict(out, Status="NOT_EVALUATED")
    for e in allowed + forbidden:
        if any(ch in e for ch in "*?["):
            out["Gaps"].append("entrada con comodines no interpretada: %s" % e)
    if out["Gaps"]:
        return dict(out, Status="NOT_EVALUATED")
    r = _git(repo, "diff", "--no-renames", "--raw", "-z", base, current)
    if r.returncode != 0:
        out["Failures"].append("comparación no ejecutada: %s" % r.stderr.strip()[:200])
        return dict(out, Status="NOT_EVALUATED")
    parts = [p for p in r.stdout.split("\0") if p]
    rows = list(zip(parts[0::2], parts[1::2]))
    for meta, path in rows:
        f = meta.split()
        status, new_mode, old_mode = f[-1], f[1], f[0].lstrip(":")
        a = any(_entry_matches(path, e) for e in allowed)
        fb = [e for e in forbidden if _entry_matches(path, e)]
        row = {"Path": path, "Status": status, "Allowed": a, "Forbidden": fb}
        if "120000" in (new_mode, old_mode) or "160000" in (new_mode, old_mode):
            out["Gaps"].append("entrada no regular (enlace o gitlink), sin resolver: %s" % path)
        ci_only = not a and any(path.lower().startswith(e.lower()) if e.endswith("/") else path.lower() == e.lower() for e in allowed)
        if ci_only:
            out["Gaps"].append("coincidencia solo sin distinguir mayúsculas (no se interpreta): %s" % path)
        if not a:
            out["Failures"].append("fuera de AllowedWriteScope: %s (%s)" % (path, status))
        if fb:
            out["Failures"].append("en ForbiddenWriteScope: %s" % path)
        out["Paths"].append(row)
    if not rows:
        out["Facts"] = {"Empty": True}
    status = "FAIL" if out["Failures"] else ("NOT_EVALUATED" if out["Gaps"] else "PASS")
    return dict(out, Status=status)


# ------------------------------------------------------------------ P3: facts, never verdicts
def assert_no_verdict(obj, path="$"):
    """Raises ValueError if a helper output carries a verdict field or value anywhere."""
    if isinstance(obj, dict):
        for k, v in obj.items():
            if k in VERDICT_KEYS:
                raise ValueError("campo de veredicto en la salida de un helper: %s.%s" % (path, k))
            assert_no_verdict(v, path + "." + k)
    elif isinstance(obj, list):
        for i, v in enumerate(obj):
            assert_no_verdict(v, "%s[%d]" % (path, i))
    elif isinstance(obj, str) and obj.strip().upper() in VERDICT_VALUES:
        raise ValueError("valor de veredicto en la salida de un helper: %s = %s" % (path, obj))
    return True


def p3_facts(repo, handoff, delegation):
    """Fact bundle for the Controller: P1 + P2 results. Classification stays with the Controller."""
    h, _ = _load(handoff)
    d, _ = _load(delegation)
    ident = p1_identity(h, repo, d)
    scope = p2_scope(repo, d.get("BaseSha"), h.get("CurrentSha"), d) if ident["Status"] == "PASS" else {"Helper": "P2", "Status": "NOT_EVALUATED",
                                                                                                        "Failures": ["identidad no establecida"]}
    facts = {"Kind": "FACTS", "Producer": "i62_helpers (EXPERIMENTAL)", "Identity": ident, "Scope": scope,
             "Note": "hechos mecánicos; el juicio, la independencia y el veredicto son del Controller"}
    assert_no_verdict(facts)
    return facts


# ------------------------------------------------------------------ P5: configuration and binary baseline gate (fake files only)
def sha256_file(path):
    with open(path, "rb") as f:
        return hashlib.sha256(f.read()).hexdigest()


class ConfigGate:
    """Accepted baseline vs observation. A new preflight is only an OBSERVATION: it never becomes the baseline without a decision reference
    that names that exact observation. Drift is a STOP; resumption only with MATCH against an accepted baseline."""
    DECISION = re.compile(r"^(decisiones §\d+|I62-[A-Z-]+: \S+)$")

    def __init__(self, baseline):
        self.baseline = dict(baseline)          # {"Config": sha, "Binary": sha, "DecisionRef": str}
        self.observations, self.log = [], []

    def observe(self, config_path, binary_path):
        obs = {"Id": "OBS-%d" % (len(self.observations) + 1), "Config": sha256_file(config_path), "Binary": sha256_file(binary_path)}
        self.observations.append(obs)
        return obs

    def evaluate(self, obs):
        drift = [k for k in ("Config", "Binary") if obs[k] != self.baseline[k]]
        state = "MATCH" if not drift else "STOP"
        self.log.append({"Observation": obs["Id"], "State": state, "Drift": drift})
        return {"State": state, "Drift": drift, "Baseline": self.baseline.get("DecisionRef")}

    def accept(self, obs, decision_ref, names_observation):
        if not self.DECISION.match(decision_ref or ""):
            return {"Accepted": False, "Reason": "sin referencia de decisión válida"}
        if names_observation != obs["Id"]:
            return {"Accepted": False, "Reason": "la decisión no nombra esta observación"}
        self.baseline = {"Config": obs["Config"], "Binary": obs["Binary"], "DecisionRef": decision_ref}
        return {"Accepted": True}

    def may_resume(self, obs):
        return self.evaluate(obs)["State"] == "MATCH"


# ------------------------------------------------------------------ P6: TRX normalized reading
NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def p6_trx(path, declared_skip_methods=None):
    with open(path, "rb") as fh:
        raw = fh.read()
    out = {"Helper": "P6", "Sha256": hashlib.sha256(raw).hexdigest(), "Bytes": len(raw), "Notes": [], "Failures": []}
    root = ET.fromstring(raw)
    counters = root.find("t:ResultSummary/t:Counters", NS)
    out["Counters"] = {k: int(v) for k, v in (counters.attrib.items() if counters is not None else []) if v.isdigit()}
    results = root.findall("t:Results/t:UnitTestResult", NS)
    tally, skips, unexpected = {}, [], []
    for r in results:
        oc = r.get("outcome")
        tally[oc] = tally.get(oc, 0) + 1
        if oc == "NotExecuted":
            msg = r.find("t:Output/t:ErrorInfo/t:Message", NS)
            reason = (msg.text or "").strip() if msg is not None else ""
            (skips if reason else unexpected).append({"Test": r.get("testName"), "Reason": reason})
    out["Outcomes"] = tally
    out["Passed"] = tally.get("Passed", 0)                      # skips never counted as passed
    out["Skipped"] = len(skips)
    out["UnexpectedNotExecuted"] = unexpected
    if not results or out["Counters"].get("total", 0) == 0:
        out["Failures"].append("selección vacía")
    if tally.get("Failed", 0) or out["Counters"].get("failed", 0):
        out["Failures"].append("pruebas fallidas: %d" % max(tally.get("Failed", 0), out["Counters"].get("failed", 0)))
    if unexpected:
        out["Failures"].append("NotExecuted sin evidencia de omisión del runner: %d" % len(unexpected))
    if "passed" in out["Counters"] and out["Counters"]["passed"] != tally.get("Passed", 0):
        out["Failures"].append("contradicción: Counters.passed = %d frente a %d resultados Passed" % (out["Counters"]["passed"], tally.get("Passed", 0)))
    if "total" in out["Counters"] and out["Counters"]["total"] != len(results):
        out["Failures"].append("contradicción: Counters.total = %d frente a %d resultados" % (out["Counters"]["total"], len(results)))
    if skips and out["Counters"].get("notExecuted", 0) == 0:
        out["Notes"].append("convención del logger TRX de VSTest: omitidas como NotExecuted con Counters.notExecuted = 0 (I-63 evidencia §37.4)")
    if declared_skip_methods is not None:
        names = {s["Test"].rsplit(".", 1)[-1] for s in skips}
        out["DeclaredSkips"] = {"Declared": len(declared_skip_methods), "Known": sorted(names & declared_skip_methods),
                                "NotDeclared": sorted(names - declared_skip_methods), "DeclaredButExecuted": sorted(declared_skip_methods - names)}
        if out["DeclaredSkips"]["NotDeclared"]:
            out["Failures"].append("omitidas que el código no declara: %s" % out["DeclaredSkips"]["NotDeclared"][:5])
        if out["DeclaredSkips"]["DeclaredButExecuted"]:
            out["Notes"].append("declaradas con Skip pero no omitidas en este TRX: %s" % out["DeclaredSkips"]["DeclaredButExecuted"][:5])
    out["Status"] = "FAIL" if out["Failures"] else "PASS"
    return out


SKIP_ATTR = re.compile(r"\[[A-Za-z]*(?:Fact|Theory)[A-Za-z]*\s*\([^\]]*\bSkip\s*=", re.S)
METHOD = re.compile(r"public\s+(?:async\s+)?(?:void|Task)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(")


def declared_skip_methods(repo, sha, test_dir):
    """Methods whose test attribute declares Skip = …, read from the source at <sha> (framework declaration, not a TRX count)."""
    names = set()
    files = _git(repo, "ls-tree", "-r", "--name-only", sha, test_dir).stdout.split()
    for f in files:
        if not f.endswith(".cs"):
            continue
        text = _git(repo, "show", "%s:%s" % (sha, f)).stdout
        for m in SKIP_ATTR.finditer(text):
            mm = METHOD.search(text, m.end())
            if mm:
                names.add(mm.group(1))
    return names
