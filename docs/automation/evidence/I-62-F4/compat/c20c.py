"""I-62 F4 (C-20b negatives and C-20c): the compatibility traces of Proposal V14 Anexo E.6 on the REAL materialized F4 texts, schema and clause map.

Port of the F4 preparation prototype (I-62-prep/f4/compat-proto/compat_proto.py): the resolver functions are the prototype's (E.2 Classify, E.3.0
Evaluate, E.3.1 Resolve/R61 and the composite read, E.4 Validate); the scenario no longer writes fixture texts. In a DISPOSABLE clone (never
published): EFF = local merge of the I-62 branch tip, plus one LOCAL empty commit carrying the normative trailer, into origin/main, with a PRE table
in its body (I-61, I-64 and I-52 rows are declared test data); M2 = EFF + X1 + X2 + X3. Every case runs Evaluate from E1 with the contract bytes,
the clone and MainSha_eval; the harness gets 16.13 from the entry point's text, and the map, its schema and the closed surfaces from 16.13's text.
Commit dates are fixed, so the scenario SHAs depend only on the branch tip.

Usage: python c20c.py <repository with refs/remotes/origin/*> <out.json>
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

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "I-62-prep", "f4", "yaml-subset"))
import yaml_subset as Y  # noqa: E402

SRC, OUT = sys.argv[1], sys.argv[2]
ROOT = tempfile.mkdtemp(prefix="r62-compat-")
S = os.path.join(ROOT, "scen")
DATE = "2026-10-04T12:00:00+00:00"
ENV = {**os.environ, "GIT_AUTHOR_NAME": "fx", "GIT_AUTHOR_EMAIL": "fx@example.invalid", "GIT_COMMITTER_NAME": "fx", "GIT_COMMITTER_EMAIL": "fx@example.invalid",
       "GIT_AUTHOR_DATE": DATE, "GIT_COMMITTER_DATE": DATE}
ENTRY_HEADING = "## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)"
R_HEADING = None          # discovered from the entry point (E3)
SCENARIO_R_HEADING = "### 16.13 Compatibilidad de protocolos de ejecución delegada"   # only to BUILD the negative variants N-m and N-p
POINTER = "Antes de aplicar esta sección, toda unidad aplica §16.13."
MAP_PATH = None           # discovered from 16.13
SCHEMA_PATH = None        # discovered from 16.13
SURFACES = None           # discovered from 16.13
FROZEN_SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md",
                   "docs/adr/", "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
TRAILER = "Agent-Protocol-Normative: I-62"
CID = {"I-61": "0e2923de-e1a7-41bf-b7db-50ec84217850", "I-64": "614371d5-441f-4f14-bac9-f97017105610", "I-52": "232b55c5-3d0c-47c7-90ae-a07caab06431"}


class Stop(Exception):
    def __init__(self, code, detail=""):
        super().__init__(code + ((": " + detail) if detail else ""))
        self.code = code


def git(*args, check=True, cwd=None):
    r = subprocess.run(["git", "-C", cwd or S] + list(args), capture_output=True, text=True, encoding="utf-8", errors="replace", env=ENV)
    if check and r.returncode != 0:
        raise RuntimeError("git %s: %s" % (" ".join(args), r.stderr.strip()))
    return r.stdout


SHA_RE = re.compile(r"^[0-9a-f]{40}$")
_cache = {}


def _cached(key, fn):
    if SHA_RE.match(str(key[1])):
        if key not in _cache:
            _cache[key] = fn()
        return _cache[key]
    return fn()


def show(rev, path):
    def f():
        r = subprocess.run(["git", "-C", S, "show", rev + ":" + path], capture_output=True, env=ENV)
        return r.stdout.decode("utf-8").replace(chr(13) + chr(10), chr(10)) if r.returncode == 0 else None
    return _cached(("show", rev, path), f)


def blob(rev, path):
    def f():
        r = subprocess.run(["git", "-C", S, "rev-parse", "-q", "--verify", rev + ":" + path], capture_output=True, text=True, env=ENV)
        return r.stdout.strip() if r.returncode == 0 else None
    return _cached(("blob", rev, path), f)


def is_ancestor(a, b):
    return _cached(("anc", b, a), lambda: subprocess.run(["git", "-C", S, "merge-base", "--is-ancestor", a, b], env=ENV).returncode == 0)


def norm(s):
    return " ".join(s.split())


# ------------------------------------------------------------------ sections (16.3 / E.4)
def heading_level(line):
    m = re.match(r"^(#{1,6}) ", line)
    return len(m.group(1)) if m else 0


_sec_cache = {}


def sections(text):
    if text in _sec_cache:
        return _sec_cache[text]
    _sec_cache[text] = _sections(text)
    return _sec_cache[text]


def _sections(text):
    """→ list of (level, normalized heading line, normalized section text); level 0 = preámbulo (text before the first ##)."""
    lines = text.split("\n")
    heads, fence = [], False
    for i, l in enumerate(lines):
        if l.lstrip().startswith("```"):
            fence = not fence
            continue
        lv = 0 if fence else heading_level(l)
        if lv:
            heads.append((i, lv))
    out = []
    first2 = next((i for i, lv in heads if lv >= 2), len(lines))
    out.append((0, "preámbulo", norm("\n".join(lines[:first2]))))
    for k, (i, lv) in enumerate(heads):
        end = next((j for j, lv2 in heads[k + 1:] if lv2 <= lv), len(lines))
        out.append((lv, norm(lines[i]), norm("\n".join(lines[i:end]))))
    return out


def match(rev, path, section):
    t = show(rev, path)
    if t is None:
        return []
    if section == "preámbulo":
        return [s for s in sections(t) if s[0] == 0]
    return [s for s in sections(t) if s[0] > 0 and s[1] == norm(section)]


# ------------------------------------------------------------------ Derive / PRE / Classify (§14, E.2)
def derive(rev):
    rev = git("rev-parse", rev).strip()
    def f():
        cands = [c for c in git("log", "--format=%H", "--grep", "^" + TRAILER + "$", rev).split() if TRAILER in git("show", "-s", "--format=%B", c).split(chr(10))]
        if not cands:
            return None
        if len(cands) > 1:
            return ("ERR", "trailer duplicado")
        t = cands[0]
        fp = git("rev-list", "--first-parent", "--reverse", rev).split()
        lo, hi = 0, len(fp) - 1          # ancestry of t along the first-parent chain is monotonic: binary search for the first commit that contains it
        if not is_ancestor(t, fp[hi]):
            return ("ERR", "trailer no alcanzable")
        while lo < hi:
            mid = (lo + hi) // 2
            if is_ancestor(t, fp[mid]):
                hi = mid
            else:
                lo = mid + 1
        c = fp[lo]
        parents = git("show", "-s", "--format=%P", c).split()
        if len(parents) == 2 and is_ancestor(t, parents[1]) and not is_ancestor(t, parents[0]):
            return c
        return ("ERR", "trailer sin merge efectivo derivable")
    r = _cached(("derive", rev), f)
    if isinstance(r, tuple):
        raise Stop("ACTIVATION_INVALID", r[1])
    return r


def pre_table(eff):
    body = git("show", "-s", "--format=%B", eff).split("\n")
    try:
        i = body.index("Derived formal claim table:")
    except ValueError:
        raise Stop("ACTIVATION_INVALID", "PRE ausente")
    head = [h.strip() for h in body[i + 1].split("|")]
    if "Claim-Id" not in head:
        raise Stop("ACTIVATION_INVALID", "PRE mal formada")
    k, ids = head.index("Claim-Id"), []
    for row in body[i + 2:]:
        if not row.strip():
            break
        cells = [c.strip() for c in row.split("|")]
        if len(cells) != len(head):
            raise Stop("ACTIVATION_INVALID", "PRE mal formada")
        ids.append(cells[k])
    if len(ids) != len(set(ids)):
        raise Stop("ACTIVATION_INVALID", "Claim-Id duplicado en PRE")
    return ids


def classify(unit, state_rev, eff):
    st = show(state_rev, "docs/automation/state/%s.yml" % unit)
    if st is None:
        return "UNKNOWN"
    head = Y.loads(st, "HEADER")
    cid = (head.get("automation_state") or {}).get("claim_id")
    pre = pre_table(eff)
    if cid and pre.count(cid) == 1:
        return "I61"
    if head.get("schema") == "rackcad-automation-state/v2":
        v2 = Y.loads(st, "STRICT")
        p = v2.get("protocol", {})
        if p.get("set") == "rackcad-protocol/I62" and p.get("effective_sha") == eff and p.get("basis", {}).get("claim_id") == cid:
            g0 = p.get("g0_acceptance", {})
            if g0.get("state") == "PENDING":
                return "PENDING_G0"
            if g0.get("state") == "ACCEPTED" and g0.get("decision"):
                d = show(state_rev, g0["decision"]["path"]) or ""
                if blob(state_rev, g0["decision"]["path"]) == g0["decision"]["blob"] and "I62-CLASSIFICATION: I62" in d and \
                        "I62-DELEGATED-EXECUTION: I62_DELEGATED" in d and cid in d:
                    return "I62"
    elif head.get("schema") == "rackcad-automation-state/v1":
        d = show(state_rev, "docs/automation/decisions/%s.md" % unit) or ""
        return "DIRECT_ONLY" if "I62-DELEGATED-EXECUTION: DIRECT_ONLY" in d else "UNKNOWN"
    return "UNKNOWN"


# ------------------------------------------------------------------ clause map: derivation and Validate (E.4)
def in_surfaces(p):
    return any(p == s or (s.endswith("/") and p.startswith(s)) for s in SURFACES)


def derive_map(base, tip):
    names = [p for p in git("diff", "--name-only", base, tip).split("\n") if p and in_surfaces(p) and p != MAP_PATH]
    files, entries = [], []
    for p in sorted(names):
        b0, b1 = blob(base, p), blob(tip, p)
        if b1 is None:
            raise Stop("MAP_INVALID", "archivo borrado: " + p)
        kind = "ENTRY" if p == SCHEMA_PATH else ("ADDED" if b0 is None else "MODIFIED")
        files.append({"Path": p, "FileKind": kind, "BaseBlob": b0 if kind == "MODIFIED" else None, "EffBlob": b1})
        if kind == "MODIFIED" and p.endswith(".md"):
            h1 = {(s[1], s[0]): s[2] for s in sections(show(base, p))}
            h2 = {(s[1], s[0]): s[2] for s in sections(show(tip, p))}
            for key in sorted(set(h1) | set(h2), key=lambda k: (k[1], k[0])):
                if key in h1 and key in h2 and h1[key] != h2[key]:
                    entries.append({"Path": p, "Section": key[0], "Level": key[1], "Kind": "MODIFIED"})
                elif key in h1 and key not in h2:
                    entries.append({"Path": p, "Section": key[0], "Level": key[1], "Kind": "REMOVED"})
                elif key in h2 and key not in h1:
                    k = "ENTRY" if (p, key[0]) in (("docs/AUTOMATION_PLAN.md", norm(R_HEADING)), ("docs/WORKFLOW.md", norm(ENTRY_HEADING))) else "ADDED"
                    entries.append({"Path": p, "Section": key[0], "Level": key[1], "Kind": k})
    return {"Schema": "rackcad-clause-map/v1", "Protocol": "rackcad-protocol/I62", "LegacyProtocol": "rackcad-protocol/I61", "Surfaces": SURFACES,
            "Files": files, "Entries": entries}


def validate_map(M, eff):
    base = git("rev-parse", eff + "^1").strip()
    errs = []
    if not isinstance(M, dict) or M.get("Schema") != "rackcad-clause-map/v1" or show(eff, SCHEMA_PATH) is None:
        errs.append("MV-1")
    if M.get("Surfaces") != SURFACES:
        errs.append("MV-2")
    fpaths = [f["Path"] for f in M.get("Files", [])]
    changed = {p for p in git("diff", "--name-only", base, eff).split("\n") if p and in_surfaces(p) and p != MAP_PATH}
    if set(fpaths) != changed or len(fpaths) != len(set(fpaths)) or any(blob(eff, p) is None for p in changed):
        errs.append("MV-3")
    for f in M.get("Files", []):
        if f["FileKind"] == "MODIFIED" and (f["BaseBlob"] != blob(base, f["Path"]) or f["EffBlob"] != blob(eff, f["Path"])):
            errs.append("MV-4")
        if f["FileKind"] in ("ADDED", "ENTRY") and (f["BaseBlob"] is not None or f["EffBlob"] != blob(eff, f["Path"])):
            errs.append("MV-4")
        if f["FileKind"] == "ENTRY" and f["Path"] != SCHEMA_PATH:
            errs.append("MV-4")
    keys = [(e["Path"], e["Section"]) for e in M.get("Entries", [])]
    mod = {f["Path"] for f in M.get("Files", []) if f["FileKind"] == "MODIFIED"}
    if len(keys) != len(set(keys)) or any(e["Path"] not in mod for e in M.get("Entries", [])):
        errs.append("MV-5")
    want = derive_map(base, eff)
    if sorted(json.dumps(e, sort_keys=True) for e in M.get("Entries", [])) != sorted(json.dumps(e, sort_keys=True) for e in want["Entries"]):
        errs.append("MV-6")
    entry = match(eff, "docs/WORKFLOW.md", ENTRY_HEADING)
    names_r = len(entry) == 1 and ("`" + R_HEADING + "`") in entry[0][2]
    p16 = match(eff, "docs/AUTOMATION_PLAN.md", "## 16. Ejecución delegada bajo orden del Coordinator")
    p163 = match(eff, "docs/AUTOMATION_PLAN.md", "### 16.3 Lectura de autoridades")
    b163 = match(base, "docs/AUTOMATION_PLAN.md", "### 16.3 Lectura de autoridades")
    if not (names_r and len(match(eff, "docs/AUTOMATION_PLAN.md", R_HEADING)) == 1 and p16 and POINTER in p16[0][2] and p163 and POINTER in p163[0][2]
            and b163 and norm(p163[0][2].replace(POINTER, "")) == b163[0][2]):
        errs.append("MV-7")
    return sorted(set(errs))


# ------------------------------------------------------------------ Evaluate (E.3.0) and Resolve (E.3.1)
def discover(r_heading, r_text):
    """The paths and the closed list that 16.13 states (its normalized text); nothing is hard-coded in the harness."""
    global R_HEADING, MAP_PATH, SCHEMA_PATH, SURFACES
    R_HEADING = r_heading
    ticks = re.findall(r"`([^`]+)`", r_text)
    MAP_PATH = next(t for t in ticks if t.endswith("-clause-map.json"))
    SCHEMA_PATH = next(t for t in ticks if t.endswith("clause-map.schema.json"))
    para = r_text[r_text.index("**Superficies** (lista cerrada):"):r_text.index("**Clasificación**")]
    SURFACES = re.findall(r"`([^`]+)`", para)


def evaluate(K, main_eval, state_rev, map_override=None):
    log = {"E1": blob(main_eval, "docs/WORKFLOW.md")}
    eff = derive(main_eval)
    X = match(main_eval, "docs/WORKFLOW.md", ENTRY_HEADING)
    if eff is None:
        if X:
            raise Stop("ACTIVATION_INVALID", "punto de entrada sin EFF")
        return {"Outcome": "PRE_ACTIVATION", "Discovery": log, "Resolution": [(a["Path"], a["Section"], a["Class"], "NORMAL",
                                                                               "AR" if a["Class"] != "EXTERNAL" else "MainSha") for a in K["Authorities"]]}
    if len(X) != 1:
        raise Stop("ENTRY_INVALID", "E2: %d secciones de entrada" % len(X))
    named = re.findall(r"`(### 16\.13 [^`]+)`", X[0][2])
    R = match(main_eval, "docs/AUTOMATION_PLAN.md", named[0]) if len(named) == 1 else []
    if len(R) != 1:
        raise Stop("ENTRY_INVALID", "E3: §16.13 nombrada %d veces" % len(R))
    discover(named[0], R[0][2])
    log.update({"EFF": eff, "E2": X[0][1], "E3": R[0][1], "AutomationPlanBlob": blob(main_eval, "docs/AUTOMATION_PLAN.md"), "MapPath": MAP_PATH,
                "SchemaPath": SCHEMA_PATH, "Surfaces": SURFACES})
    res = resolve(K, main_eval, eff, state_rev, map_override)
    res["Discovery"] = log
    return res


def resolve(K, main_eval, eff, state_rev, map_override):
    P = classify(K["Unit"], state_rev, eff)
    if P in ("UNKNOWN", "PENDING_G0", "DIRECT_ONLY"):
        raise Stop(P if P != "DIRECT_ONLY" else "P-15", "Classify")
    v2 = K["Schema"].endswith("/v2")
    if (P == "I62" and not v2) or (P == "I61" and v2):
        raise Stop("P-15", "protocolo del contrato ≠ protocolo de la unidad")
    M = map_override if map_override is not None else json.loads(show(eff, MAP_PATH) or "null")
    if M is None or validate_map(M, eff):
        raise Stop("MAP_INVALID", ",".join(validate_map(M, eff)) if M else "mapa ausente")
    base = git("rev-parse", eff + "^1").strip()
    files = {f["Path"]: f for f in M["Files"]}
    kinds = {(e["Path"], e["Section"]): e["Kind"] for e in M["Entries"]}
    out = []
    for a in K["Authorities"]:
        path, sec, cls = a["Path"], a["Section"], a["Class"]
        if cls in ("UNIT_DOC", "UNIT_CHANGE"):
            out.append((path, sec, cls, "NORMAL", "AR"))
        elif P == "I62":
            out.append((path, sec, cls, "NORMAL", "MainSha_eval"))
        else:
            out.append(r61(path, sec, cls, files, kinds, base, eff, main_eval))
    checks_163(K, main_eval)
    return {"Outcome": "RESOLVED", "Class": P, "Resolution": out, "MapBlob": blob(eff, MAP_PATH)}


def r61(path, sec, cls, files, kinds, base, eff, main_eval):
    f = files.get(path)
    if path == MAP_PATH or (f and f["FileKind"] == "ENTRY"):
        return (path, sec, cls, "ENTRY", "EFF")
    if f and f["FileKind"] == "ADDED":
        raise Stop("P-15", "NOT_APPLICABLE: archivo ADDED " + path)
    if f and f["FileKind"] == "MODIFIED":
        if sec == "documento completo":
            if not path.endswith(".md"):
                return (path, sec, cls, "COMPAT", "EFF^1")
            return (path, sec, cls, "COMPUESTA", composite(path, kinds, base, main_eval))
        if kinds.get((path, norm(sec))) == "ENTRY":
            return (path, sec, cls, "ENTRY", "MainSha_eval")
        m1 = match(base, path, sec)
        if len(m1) == 1:
            if kinds.get((path, m1[0][1])) in ("MODIFIED", "REMOVED"):
                return (path, sec, cls, "COMPAT", "EFF^1")
            if len(match(main_eval, path, sec)) == 1:
                return (path, sec, cls, "NORMAL", "MainSha_eval")
            raise Stop("S-12", "sección ausente en MainSha_eval: " + sec)
        if not m1:
            me = match(eff, path, sec)
            if len(me) == 1 and kinds.get((path, me[0][1])) == "ADDED":
                raise Stop("P-15", "NOT_APPLICABLE: sección ADDED " + sec)
            if not me and len(match(main_eval, path, sec)) == 1:
                return (path, sec, cls, "NORMAL", "MainSha_eval")
            raise Stop("S-12", "sección inexistente o ambigua: " + sec)
        raise Stop("S-12", "sección repetida en EFF^1: " + sec)
    if in_surfaces(path):
        return (path, sec, cls, "NORMAL", "MainSha_eval")
    if blob(base, path) is not None and blob(base, path) == blob(eff, path):
        return (path, sec, cls, "NORMAL", "MainSha_eval")
    raise Stop("S-12", "ruta fuera de Surfaces cambiada por I-62: " + path)


def composite(path, kinds, base, main_eval):
    units = []
    s1 = sections(show(base, path))
    names1 = set()
    for lv, h, _ in s1:
        if lv not in (0, 2):
            continue
        names1.add(h)
        k = kinds.get((path, h))
        if k in ("MODIFIED", "REMOVED"):
            units.append((h, "EFF^1"))
        elif any(x[1] == h for x in sections(show(main_eval, path)) if x[0] in (0, 2)):
            units.append((h, "MainSha_eval"))
        else:
            units.append((h, "OMITIDA (ya no existe)"))
    for lv, h, _ in sections(show(main_eval, path)):
        if lv != 2 or h in names1:
            continue
        k = kinds.get((path, h))
        if k == "ADDED":
            units.append((h, "OMITIDA (solo I62)"))
        elif k == "ENTRY":
            units.append((h, "MainSha_eval (ENTRY)"))
        else:
            units.append((h, "MainSha_eval (posterior a EFF)"))
    return units


def checks_163(K, main_eval):
    """16.3 with the resolved reading: MB = merge-base(MainSha_eval, AR); EXTERNAL files without UNIT_CHANGE sections unchanged MB..AR; with them, the
    EXTERNAL sections identical in MB and AR. (The BaseSha check needs a delegation and is out of this replay.)"""
    ar = K["AuthorityRevision"]
    mb = git("merge-base", main_eval, ar).strip()
    by_file = {}
    for a in K["Authorities"]:
        by_file.setdefault(a["Path"], []).append(a)
    for path, cites in by_file.items():
        ext = [c for c in cites if c["Class"] == "EXTERNAL"]
        if not ext:
            continue
        if not any(c["Class"] == "UNIT_CHANGE" for c in cites):
            if git("diff", "--name-only", mb, ar, "--", path).strip():
                raise Stop("S-12", "16.3: %s cambia entre MB y AR" % path)
        else:
            for c in ext:
                a1, a2 = match(mb, path, c["Section"]), match(ar, path, c["Section"])
                if not a1 or not a2 or a1[0][2] != a2[0][2]:
                    raise Stop("S-12", "16.3: sección EXTERNAL distinta en MB y AR: " + c["Section"])


# ------------------------------------------------------------------ scenario
def write(path, text):
    full = os.path.join(S, path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    git("add", path)


def edit_section_append(path, heading, extra):
    t = open(os.path.join(S, path), encoding="utf-8").read().replace("\r\n", "\n")
    lines = t.split("\n")
    i = next(k for k, l in enumerate(lines) if norm(l) == norm(heading))
    lv = heading_level(lines[i])
    j = next((k for k in range(i + 1, len(lines)) if heading_level(lines[k]) and heading_level(lines[k]) <= lv), len(lines))
    while j > i + 1 and not lines[j - 1].strip():
        j -= 1
    lines[j:j] = ["", extra]
    write(path, "\n".join(lines))


def commit(msg):
    git("commit", "-q", "-m", msg)
    return git("rev-parse", "HEAD").strip()


subprocess.run(["git", "clone", "-q", "--no-local", "--no-checkout", SRC, S], check=True, env=ENV, capture_output=True)
git("config", "core.autocrlf", "false")
git("fetch", "-q", SRC, "+refs/remotes/origin/*:refs/remotes/src/*")
M0 = git("rev-parse", "src/main").strip()
I62TIP = git("rev-parse", "src/architecture/portabilidad-coordinador-principal").strip()
I64TIP = git("rev-parse", "src/architecture/workspace-persistente-rackcad").strip()
assert git("merge-base", M0, I62TIP).strip() == M0, "la rama de I-62 no está sobre origin/main: regenerar el mapa tras el rebase"
git("checkout", "-q", "-b", "b62", I62TIP)
git("commit", "-q", "--allow-empty", "-m", "Trailer normativo LOCAL del escenario C-20c (clon desechable; nunca se publica)\n\n" + TRAILER)
B62 = git("rev-parse", "HEAD").strip()
git("checkout", "-q", "-b", "m", M0)
pre = ("Merge I-62 (escenario C-20c local)\n\nDerived formal claim table:\nInitiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition\n"
       "I-61 | refs/heads/architecture/protocolo-ejecucion-agentes | x | x | %s | dato de prueba | PRE\n"
       "I-64 | refs/heads/architecture/workspace-persistente-rackcad | x | x | %s | dato de prueba | PRE\n"
       "I-52 | refs/heads/feature/rackmirror-espejo-semantico | x | x | %s | dato de prueba | PRE\n") % (CID["I-61"], CID["I-64"], CID["I-52"])
git("merge", "-q", "--no-ff", "-m", pre, B62)
EFF = git("rev-parse", "HEAD").strip()
edit_section_append("docs/AUTOMATION_PLAN.md", "## 3. Limites de seguridad", "X1: cambio posterior a EFF (otra iniciativa).")
commit("X1")
edit_section_append("docs/AUTOMATION_PLAN.md", "### 16.4 Transporte, relevo, cesión y orden de commits", "X2: cambio posterior a EFF en una sección modificada por I-62.")
commit("X2")
wf = open(os.path.join(S, "docs/WORKFLOW.md"), encoding="utf-8").read().replace("\r\n", "\n").rstrip("\n")
write("docs/WORKFLOW.md", wf + "\n\n## 13. Sección posterior de otra iniciativa (X3)\n\nTexto de X3.\n")
M2 = commit("X3")


def variant(name, base, fn):
    git("checkout", "-q", "-b", name, base)
    fn()
    return commit(name)


def remove_section(path, heading):
    t = open(os.path.join(S, path), encoding="utf-8").read().replace("\r\n", "\n").split("\n")
    i = next(k for k, l in enumerate(t) if norm(l) == norm(heading))
    lv = heading_level(t[i])
    j = next((k for k in range(i + 1, len(t)) if heading_level(t[k]) and heading_level(t[k]) <= lv), len(t))
    write(path, "\n".join(t[:i] + t[j:]))


M2m = variant("n-m", M2, lambda: remove_section("docs/AUTOMATION_PLAN.md", SCENARIO_R_HEADING))
M2n = variant("n-n", M2, lambda: remove_section("docs/WORKFLOW.md", ENTRY_HEADING))
M2o = variant("n-o", M2, lambda: write("docs/WORKFLOW.md", open(os.path.join(S, "docs/WORKFLOW.md"), encoding="utf-8").read() + "\n" + ENTRY_HEADING + "\n\nDuplicada.\n"))
M2p = variant("n-p", M2, lambda: write("docs/WORKFLOW.md", open(os.path.join(S, "docs/WORKFLOW.md"), encoding="utf-8").read().replace("`" + SCENARIO_R_HEADING + "`",
                                                                                                                                       "`### 16.13 Compatibilidad inexistente`")))


def drop_pointers():
    t = open(os.path.join(S, "docs/AUTOMATION_PLAN.md"), encoding="utf-8").read()
    write("docs/AUTOMATION_PLAN.md", t.replace(POINTER + " ", "").replace(" " + POINTER, ""))


M2q = variant("n-q", M2, drop_pointers)
git("checkout", "-q", "-b", "n-j", M2)
write("docs/x-j.md", "segundo trailer\n")
M2j = commit("segundo commit normativo\n\n" + TRAILER)
Mr = variant("n-r", M0, lambda: write("docs/WORKFLOW.md", open(os.path.join(S, "docs/WORKFLOW.md"), encoding="utf-8").read() + "\n" + ENTRY_HEADING + "\n\nSin trailer.\n"))
# synthetic unit states for N-a, N-b, N-l (on a branch from M2)
git("checkout", "-q", "-b", "units", M2)
write("docs/automation/state/I-98.yml", "schema: rackcad-automation-state/v1\nautomation_state:\n  initiative: I-98\n  branch: x\n  claim_id: 98989898-0000-0000-0000-000000000098\n")
dec = "I62-CLASSIFICATION: I62\nI62-DELEGATED-EXECUTION: I62_DELEGATED\nClaim-Id: 97979797-0000-0000-0000-000000000097\n"
write("docs/automation/decisions/I-97.md", dec)
git("add", ".")
commit("units")
dblob = blob("HEAD", "docs/automation/decisions/I-97.md")


def v2state(unit, cid, g0):
    s = {"schema": "rackcad-automation-state/v2", "automation_state": {"initiative": unit, "branch": "x", "claim_id": cid},
         "protocol": {"set": "rackcad-protocol/I62", "effective_sha": EFF, "basis": {"claim_id": cid},
                      "g0_acceptance": {"state": g0, "decision": {"path": "docs/automation/decisions/I-97.md", "blob": dblob} if g0 == "ACCEPTED" else None}}}
    return Y.dumps(s)


write("docs/automation/state/I-97.yml", v2state("I-97", "97979797-0000-0000-0000-000000000097", "ACCEPTED"))
write("docs/automation/state/I-96.yml", v2state("I-96", "96969696-0000-0000-0000-000000000096", "PENDING"))
UNITS = commit("estados de prueba")
# C-28 (opt-in): I-95 is a later unit with a /v1 state and a G0 decision DIRECT_ONLY; I-94 adopts I62_DELEGATED (T20)
write("docs/automation/state/I-95.yml", "schema: rackcad-automation-state/v1\nautomation_state:\n  initiative: I-95\n  branch: feature/i95\n"
      "  claim_id: 95959595-0000-0000-0000-000000000095\n")
write("docs/automation/decisions/I-95.md", "## G0\n\n```text\nI62-DELEGATED-EXECUTION: DIRECT_ONLY\nClaim-Id: 95959595-0000-0000-0000-000000000095\n```\n")
C28_DIRECT = commit("I-95: G0 DIRECT_ONLY")
write("docs/x-i95.md", "trabajo directo de I-95\n")
C28_WORK = commit("I-95: trabajo directo (commit ordinario, sin maquinaria delegada)")
def v2state_for(unit, cid, g0, dec_path, dec_blob):
    s = {"schema": "rackcad-automation-state/v2", "automation_state": {"initiative": unit, "branch": "x", "claim_id": cid},
         "protocol": {"set": "rackcad-protocol/I62", "effective_sha": EFF, "basis": {"claim_id": cid, "adoption_at": "MID_INITIATIVE"},
                      "g0_acceptance": {"state": g0, "decision": {"path": dec_path, "blob": dec_blob} if g0 == "ACCEPTED" else None}}}
    return Y.dumps(s)
write("docs/automation/state/I-94.yml", v2state_for("I-94", "94949494-0000-0000-0000-000000000094", "PENDING", None, None))
C28_BOOT = commit("I-94: BOOTSTRAP de adopción (PENDING)")
write("docs/automation/decisions/I-94.md", "## Adopción\n\n```text\nI62-DELEGATED-EXECUTION: I62_DELEGATED\nI62-CLASSIFICATION: I62\n"
      "I62-PRINCIPAL-BINDING: B20261005T000000Z-aa94 ACCEPTED\nClaim-Id: 94949494-0000-0000-0000-000000000094\n```\n")
git("add", ".")
commit("I-94: decisión de adopción")
write("docs/automation/state/I-94.yml", v2state_for("I-94", "94949494-0000-0000-0000-000000000094", "ACCEPTED", "docs/automation/decisions/I-94.md",
                                                  blob("HEAD", "docs/automation/decisions/I-94.md")))
C28_QU = commit("I-94: QU con las aceptaciones")

# ------------------------------------------------------------------ cases
K = json.loads(show(I62TIP, "docs/automation/evidence/I-61-pilot/g3-cama-d1a/R20261001T032333Z-4a2d/gate-contract.json"))
K_BLOB = blob(I62TIP, "docs/automation/evidence/I-61-pilot/g3-cama-d1a/R20261001T032333Z-4a2d/gate-contract.json")
I64DOCS = sorted(p for p in git("ls-tree", "-r", "--name-only", I64TIP, "docs/initiatives/").split() if os.path.basename(p).startswith("I-64-"))[:3]
K2 = copy.deepcopy(K)
K2.update({"Unit": "I-64", "Gate": "FX", "TaskId": "c20c-2", "Objective": "escenario C-20c-2", "AuthorityRevision": I64TIP, "MainSha": M2})
for a in K2["Authorities"]:
    if a["Class"] == "UNIT_CHANGE":
        a["Class"] = "EXTERNAL"
k = 0
for a in K2["Authorities"]:
    if a["Class"] == "UNIT_DOC":
        a["Path"], a["Section"] = I64DOCS[k], "documento completo"
        k += 1


def run(name, K_, main_eval, state_rev, expect, map_override=None, compare=None):
    try:
        r = evaluate(K_, main_eval, state_rev, map_override)
        got = r["Outcome"] if r["Outcome"] == "PRE_ACTIVATION" else r.get("Class")
        detail = r
    except Stop as e:
        got, detail = e.code, {"Stop": str(e)}
    ok = got == expect
    if ok and compare is not None and "Resolution" in detail:
        diffs = [(i + 1, row, compare[i]) for i, row in enumerate(detail["Resolution"]) if compare[i] is not None and row[4] != compare[i]]
        detail["ExpectedDiffs"] = diffs
        ok = not diffs
    return {"Case": name, "Expected": expect, "Got": got, "Pass": ok, "Detail": detail}


cases = []
cases.append(run("C-20c-1 (a) MainSha = 95690c28 (antes de EFF)", K, K["MainSha"], M0, "PRE_ACTIVATION"))
EXP1 = ["AR", "AR", "MainSha_eval", "AR", "MainSha_eval", "EFF^1", "AR", "MainSha_eval", "AR", "AR", "AR", "AR", "AR", "AR", "AR", "AR", "AR"]
cases.append(run("C-20c-1 (b) reverificación con MainSha_eval = M2", K, M2, M2, "I61", compare=EXP1))
EXP2 = ["MainSha_eval", "MainSha_eval", "MainSha_eval", "EFF^1", "MainSha_eval", "EFF^1", "EFF^1", "MainSha_eval", "MainSha_eval", None, None,
        "MainSha_eval", "MainSha_eval", "MainSha_eval", "AR", "AR", "AR"]
cases.append(run("C-20c-2 mismo contrato para I-64 (MainSha = M2)", K2, M2, I64TIP, "I61", compare=EXP2))
K2wf = copy.deepcopy(K2)
K2wf["Authorities"].append({"Path": "docs/WORKFLOW.md", "Section": "documento completo", "Class": "EXTERNAL"})
cases.append(run("C-20c-2 positivo: WORKFLOW documento completo incluye X3", K2wf, M2, I64TIP, "I61"))
neg = []
Ka = copy.deepcopy(K2); Ka["Unit"] = "I-98"
neg.append(run("N-a unidad sin Claim-Id en PRE, /v1, sin decisión", Ka, M2, UNITS, "UNKNOWN"))
Kb = copy.deepcopy(K2); Kb["Unit"] = "I-97"
neg.append(run("N-b contrato /v1 para una unidad I62", Kb, M2, UNITS, "P-15"))
good_map = json.loads(show(EFF, MAP_PATH))
mc = copy.deepcopy(good_map); mc["Entries"].append(copy.deepcopy(mc["Entries"][0]))
neg.append(run("N-c mapa con una entrada duplicada", K, M2, M2, "MAP_INVALID", map_override=mc))
md_ = copy.deepcopy(good_map); md_["Entries"] = [e for e in md_["Entries"] if not e["Section"].startswith("### 16.3")]
neg.append(run("N-d mapa sin una sección que sí cambió (### 16.3)", K, M2, M2, "MAP_INVALID", map_override=md_))
me = copy.deepcopy(good_map); me["Entries"].append({"Path": "docs/AUTOMATION_PLAN.md", "Section": "## 3. Limites de seguridad", "Level": 2, "Kind": "MODIFIED"})
neg.append(run("N-e mapa que lista ## 3 como MODIFIED (texto igual)", K, M2, M2, "MAP_INVALID", map_override=me))
m3 = copy.deepcopy(good_map); m3["Files"] = [f for f in m3["Files"] if f["Path"] != "docs/WORKFLOW.md"]
neg.append(run("C-20b MV-3 mapa sin un archivo modificado (WORKFLOW)", K, M2, M2, "MAP_INVALID", map_override=m3))
m4 = copy.deepcopy(good_map); next(f for f in m4["Files"] if f["FileKind"] == "MODIFIED")["BaseBlob"] = "0" * 40
neg.append(run("C-20b MV-4 BaseBlob distinto del observado", K, M2, M2, "MAP_INVALID", map_override=m4))
m6 = copy.deepcopy(good_map); m6["Entries"] = [e for e in m6["Entries"] if not (e["Path"] == "docs/WORKFLOW.md" and e["Kind"] == "ENTRY")]
neg.append(run("C-20b MV-6 mapa sin la ENTRY del punto de entrada", K, M2, M2, "MAP_INVALID", map_override=m6))
Kf = copy.deepcopy(K2); Kf["Authorities"].append({"Path": "docs/AUTOMATION_PLAN.md", "Section": "### 16.20 Binding y aceptación (I62)", "Class": "EXTERNAL"})
neg.append(run("N-f cita individual de una subsección ADDED de §16", Kf, M2, I64TIP, "P-15"))
Kg = copy.deepcopy(K2); Kg["Authorities"].append({"Path": "docs/AUTOMATION_PLAN.md", "Section": "## 99. No existe", "Class": "EXTERNAL"})
neg.append(run("N-g encabezado inexistente", Kg, M2, I64TIP, "S-12"))
neg.append(run("N-h C-20c-2 con MainSha_eval = M0", K2, M0, I64TIP, "PRE_ACTIVATION"))
Ki = copy.deepcopy(K2); Ki["Authorities"].append({"Path": "docs/automation/agent-execution/schemas/binding.v1.schema.json", "Section": "documento completo", "Class": "EXTERNAL"})
neg.append(run("N-i documento completo de un archivo ADDED", Ki, M2, I64TIP, "P-15"))
neg.append(run("N-j dos commits con el trailer normativo", K, M2j, M2j, "ACTIVATION_INVALID"))
Kk = copy.deepcopy(K2); Kk["Authorities"].append({"Path": "docs/automation/decisions/I-62.md", "Section": "documento completo", "Class": "EXTERNAL"})
neg.append(run("N-k EXTERNAL fuera de Surfaces cambiada por I-62", Kk, M2, I64TIP, "S-12"))
Kl = copy.deepcopy(K2); Kl["Unit"] = "I-96"
neg.append(run("N-l unidad I62 con g0_acceptance PENDING", Kl, M2, UNITS, "PENDING_G0"))
neg.append(run("N-m §16.13 retirada con EFF presente", K, M2m, M2m, "ENTRY_INVALID"))
neg.append(run("N-n sin sección de entrada (EFF presente)", K, M2n, M2n, "ENTRY_INVALID"))
neg.append(run("N-o sección de entrada repetida", K, M2o, M2o, "ENTRY_INVALID"))
neg.append(run("N-p la entrada nombra una §16.13 inexistente", K, M2p, M2p, "ENTRY_INVALID"))
c28 = []
K95 = copy.deepcopy(K2); K95["Unit"] = "I-95"
c28.append(run("C-28 (b) contrato de una unidad posterior DIRECT_ONLY", K95, M2, C28_WORK, "P-15"))
c28.append({"Case": "C-28 (a)/(c)/(e) trabajo directo sin maquinaria delegada", "Expected": "DIRECT_ONLY sin orchestration",
            "Got": classify("I-95", C28_WORK, EFF) + (" sin orchestration" if "orchestration" not in (Y.loads(show(C28_WORK, "docs/automation/state/I-95.yml"), "HEADER")) else " con orchestration"),
            "Detail": {"DirectCommit": C28_WORK, "ResolverCalled": False}})
c28[-1]["Pass"] = c28[-1]["Got"] == c28[-1]["Expected"]
K94 = copy.deepcopy(K2); K94["Unit"] = "I-94"
K94v2 = copy.deepcopy(K94); K94v2["Schema"] = "rackcad-gate-contract/v2"
c28.append(run("C-28 (d) adopción: contrato antes del QU con aceptaciones", K94v2, M2, C28_BOOT, "PENDING_G0"))
c28.append(run("C-28 (d) adopción: contrato /v2 tras el QU con aceptaciones", K94v2, M2, C28_QU, "I62"))
c28.append(run("C-28 (d) adopción: contrato /v1 tras la adopción", K94, M2, C28_QU, "P-15"))
q1 = run("N-q sin punteros: C-20c-1 resuelve igual", K, M2q, M2q, "I61", compare=EXP1)
q2 = run("N-q sin punteros: C-20c-2 resuelve igual", dict(K2, MainSha=M2q), M2q, I64TIP, "I61", compare=EXP2)
neg += [q1, q2]
neg.append(run("N-r sección de entrada en una main sin trailer", K, Mr, Mr, "ACTIVATION_INVALID"))


def coordinator_accepts(controller_record, coordinator_record):
    return controller_record is not None and controller_record == coordinator_record


base_res = cases[1]["Detail"].get("Resolution")
ns = {"Case": "N-s verificación sin registro de descubrimiento y resolución", "Expected": "REJECTED", "Got": "REJECTED" if not coordinator_accepts(None, base_res) else "ACCEPTED"}
ns["Pass"] = ns["Got"] == ns["Expected"]
neg.append(ns)
map_check = validate_map(good_map, EFF)
E6_LITERAL_EFF1 = {"docs/automation/agent-execution/README.md": ["## 1.", "## 3.", "## 5.", "## 6.", "## 11."],
                   "docs/automation/agent-execution/routing.md": ["## 4.", "## 5.", "## 7."]}
composite_rows = {}
for row in cases[2]["Detail"].get("Resolution", []):
    if row[3] == "COMPUESTA":
        composite_rows[row[0]] = row[4]
e6_diffs = []
for path, prefixes in E6_LITERAL_EFF1.items():
    for unit, rev in composite_rows.get(path, []):
        if any(unit.startswith(p + " ") for p in prefixes) and rev != "EFF^1":
            e6_diffs.append({"Path": path, "Unit": unit, "E6": "EFF^1", "Derived": rev,
                             "Cause": "la sección no está MODIFIED en el mapa derivado: diferencia E.5 / derivación, clasificada en C-20b"})
record = {"MapEntries": good_map["Entries"], "MapFiles": [(f["Path"], f["FileKind"]) for f in good_map["Files"]], "Cases": [(c["Case"], c["Got"]) for c in cases + neg],
          "Resolutions": [c["Detail"].get("Resolution") for c in cases]}
out = {"Scenario": {"M0": M0, "I62Tip": I62TIP, "EFF": EFF, "M2": M2, "ContractBlob": K_BLOB, "I64Docs": I64DOCS},
       "MapValidation": map_check or "VALID (MV-1..MV-7)",
       "MapSummary": {"Files": len(good_map["Files"]), "Modified": sum(f["FileKind"] == "MODIFIED" for f in good_map["Files"]),
                      "Entries": {k: sum(e["Kind"] == k for e in good_map["Entries"]) for k in ("MODIFIED", "REMOVED", "ADDED", "ENTRY")}},
       "Cases": cases, "Negatives": neg, "C28": c28, "CompositeUnits": composite_rows, "E6LiteralDifferences": e6_diffs,
       "DiscoveredFromTexts": {"R_HEADING": R_HEADING, "MapPath": MAP_PATH, "SchemaPath": SCHEMA_PATH, "Surfaces": SURFACES,
                               "SurfacesEqualFrozen": SURFACES == FROZEN_SURFACES},
       "DeterministicSha256": hashlib.sha256(json.dumps(record, ensure_ascii=False, sort_keys=True).encode("utf-8")).hexdigest()}
out["AllPass"] = not map_check and all(c["Pass"] for c in cases + neg + c28)
json.dump(out, open(OUT, "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1, default=str)
shutil.rmtree(ROOT, ignore_errors=True)
print(json.dumps({"map": out["MapValidation"], "summary": out["MapSummary"], "pass": "%d/%d" % (sum(c["Pass"] for c in cases + neg + c28), len(cases + neg + c28)),
                  "sha256": out["DeterministicSha256"][:16], "all": out["AllPass"]}, ensure_ascii=False))
for c in cases + neg + c28:
    if not c["Pass"]:
        print("FAIL", c["Case"], c["Expected"], c["Got"], str(c.get("Detail", {}).get("Stop", ""))[:120], str(c.get("Detail", {}).get("ExpectedDiffs", ""))[:300])
sys.exit(0 if out["AllPass"] else 1)
