#!/usr/bin/env python3
# bmin_facts.py - B-min facts tool (I-62, decisions 75). Experimental, non-normative.
#
# Computes reproducible facts from Git objects pinned by SHA and from files
# pinned by SHA-256. It never emits a verdict: only values, lists, counts and
# explicitly named booleans.
#
# Usage:   python -I bmin_facts.py <command> --repo <clone> ...
#          git cat-file blob <blob> | python -I - <command> --repo <clone> ...
#
# Python 3 standard library only. The source is ASCII-only: non-ASCII
# constants are spelled with \N{...} escapes.

import hashlib
import json
import os
import platform
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

ENVELOPE_SCHEMA = "rackcad-bmin-facts/0-experimental"
EXIT_OK = 0
EXIT_ERR = 2
EXIT_USAGE = 64

# ---------------------------------------------------------------- git runner

GIT_SUBCOMMANDS = (
    "rev-parse", "cat-file", "ls-tree", "rev-list", "diff", "log", "show",
    "merge-base", "status", "check-ignore",
)
GIT_FORBIDDEN_TOKENS = (
    "-w", "--write", "fetch", "pull", "push", "ls-remote", "remote", "config",
    "--ext-diff", "--textconv",
)
GIT_CONFIG_FLAGS = ("-c", "core.quotepath=false", "-c", "core.fsmonitor=false")
GIT_ENV = (("GIT_OPTIONAL_LOCKS", "0"), ("GIT_TERMINAL_PROMPT", "0"), ("LC_ALL", "C"))

# ---------------------------------------------------------- normative constants

ENTRY_HEADING = ("## 12. Coexistencia de protocolos de ejecuci\N{LATIN SMALL LETTER O WITH ACUTE}n"
                 " delegada (I61/I62)")
R163_HEADING = ("### 16.13 Compatibilidad de protocolos de ejecuci\N{LATIN SMALL LETTER O WITH ACUTE}n"
                " delegada")
H1610_HEADING = "### 16.10 T\N{LATIN SMALL LETTER E WITH ACUTE}rminos de gate"
POINTER = ("Antes de aplicar esta secci\N{LATIN SMALL LETTER O WITH ACUTE}n, toda unidad aplica"
           " \N{SECTION SIGN}16.13.")
PREAMBLE = "pre\N{LATIN SMALL LETTER A WITH ACUTE}mbulo"
TRAILER = "Agent-Protocol-Normative: I-62"
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
MAP_SCHEMA_PATH = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
AP_PATH = "docs/AUTOMATION_PLAN.md"
WORKFLOW_PATH = "docs/WORKFLOW.md"
HANDOFF_SCHEMA_PATH = "docs/automation/agent-execution/schemas/worker-handoff.schema.json"
SURFACES_LEAD = "**Superficies** (lista cerrada):"
TERMS_END = "No se aplica"
S16_RE = re.compile(r"## 16\.(?:\s|$)")
S163_RE = re.compile(r"### 16\.3(?:\s|$)")

FREE_TEXT_FIELDS = (
    "WorkCompleted", "Evidence", "UnexpectedFindings", "KnownLimitations",
    "Deviations", "OpenQuestions", "RecommendedNextAction",
)
HANDOFF_FIELDS = (
    "TaskId", "Attempt", "DelegationRunId", "RunId", "BaseSha", "RedSha",
    "CurrentSha", "Branch", "Worktree", "Worker",
)
# Field names of rackcad-gate-contract/v2 ("required"), read from its schema.
GATE_CONTRACT_V2_REQUIRED = (
    "Schema", "Unit", "Gate", "TaskId", "Objective", "AuthorityRevision", "MainSha",
    "Authorities", "AllowedWriteScope", "ForbiddenWriteScope", "Invariants",
    "RequiredTests", "ExpectedEvidence", "StopConditions", "RoutingEnforcement",
    "CorrectionsAuthorized", "IssuedBy", "IssuedUtc", "RoleRequirements",
    "ProtocolSet", "MaterializationCloseSha", "SupersededCommits", "SupersededBaseSha",
)

SCHEMA_KEYWORDS = (
    "type", "enum", "const", "required", "properties", "additionalProperties",
    "items", "minItems", "maxItems", "uniqueItems", "minLength", "pattern",
    "minimum", "maximum",
)
SCHEMA_ANNOTATIONS = ("$schema", "title", "description", "$id", "$comment", "default", "examples")
JSON_TYPES = ("null", "boolean", "object", "array", "number", "string", "integer")

SHA40_RE = re.compile(r"[0-9a-f]{40}")
SHA256_RE = re.compile(r"[0-9a-fA-F]{64}")
WS_RE = re.compile(r"\s+")
HEADING_RE = re.compile(r"(#{1,6})(?:[ \t]|$)")
SENTENCE_RE = re.compile(r"(?<=\.)\s+")
BACKTICK_RE = re.compile(r"`([^`]+)`")
CLAIM_RE = re.compile(r"^\s*claim_id:\s*(\S+)")
LINES_RE = re.compile(r"([0-9]+)-([0-9]+)")
INDEX_RE = re.compile(r"0|[1-9][0-9]*")


class FactsError(Exception):
    def __init__(self, code, detail):
        Exception.__init__(self, detail)
        self.code = code
        self.detail = detail


class Ctx(object):
    def __init__(self, repo):
        self.repo = repo
        self.git_calls = []
        self.inputs = []
        self._input_keys = set()
        self._blob_cache = {}
        self._content_cache = {}
        self._text_cache = {}

    def add_input(self, item):
        key = json.dumps(item, sort_keys=True)
        if key not in self._input_keys:
            self._input_keys.add(key)
            self.inputs.append(item)


def git_exec(ctx, args, ok_rcs=(0,)):
    """The single git runner: closed list, forced flags, fixed environment."""
    args = [str(a) for a in args]
    if not args or args[0] not in GIT_SUBCOMMANDS:
        raise FactsError("FORBIDDEN_OPERATION", "git subcommand outside the closed list: %s"
                         % (args[0] if args else "<none>"))
    sub = args[0]
    rest = args[1:]
    head = rest[:rest.index("--")] if "--" in rest else rest
    for tok in [sub] + head:
        if tok in GIT_FORBIDDEN_TOKENS or tok.startswith("--output"):
            raise FactsError("FORBIDDEN_OPERATION", "forbidden git token: %s" % tok)
    if sub in ("diff", "show"):
        rest = ["--no-ext-diff", "--no-textconv"] + rest
    elif sub == "status" and "--porcelain" not in head:
        rest = ["--porcelain"] + rest
    argv = list(GIT_CONFIG_FLAGS) + [sub] + rest
    if ctx.repo is None or not os.path.isdir(ctx.repo):
        raise FactsError("MISSING_INPUT", "--repo is not a directory")
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    for key, value in GIT_ENV:
        env[key] = value
    ctx.git_calls.append(argv)
    try:
        proc = subprocess.run(["git"] + argv, cwd=ctx.repo, env=env, shell=False,
                              stdin=subprocess.DEVNULL, stdout=subprocess.PIPE,
                              stderr=subprocess.PIPE)
    except OSError as exc:
        raise FactsError("GIT_ERROR", "cannot start git: %s" % exc.__class__.__name__)
    if proc.returncode not in ok_rcs:
        msg = proc.stderr.decode("utf-8", "replace").strip().splitlines()
        raise FactsError("GIT_ERROR", "git %s returned rc %d: %s"
                         % (sub, proc.returncode, msg[0] if msg else ""))
    return proc.returncode, proc.stdout


# ------------------------------------------------------------------- helpers

def git_text(data):
    try:
        return data.decode("utf-8")
    except UnicodeDecodeError:
        raise FactsError("GIT_ERROR", "git output is not UTF-8")


def git_lines(data):
    return [ln for ln in lf(git_text(data)).split("\n") if ln != ""]


def lf(text):
    return text.replace("\r\n", "\n")


def norm(text):
    return WS_RE.sub(" ", text).strip()


def split_eol(text):
    parts = lf(text).split("\n")
    if parts and parts[-1] == "":
        parts.pop()
    return parts


def sentences(normalized):
    if normalized == "":
        return []
    return SENTENCE_RE.split(normalized)


def lit(path):
    return ":(literal)" + path


def canon(obj):
    return json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))


def esc_pointer(token):
    return token.replace("~", "~0").replace("/", "~1")


def need_commit(ctx, sha, label):
    if not isinstance(sha, str) or SHA40_RE.fullmatch(sha) is None:
        raise FactsError("BAD_REVISION", "%s is not a full 40-hex lowercase SHA" % label)
    rc, out = git_exec(ctx, ["rev-parse", "--verify", "--quiet", sha + "^{commit}"], (0, 1))
    if rc != 0 or out.decode("ascii", "replace").strip() != sha:
        raise FactsError("BAD_REVISION", "%s %s is not an existing commit" % (label, sha))
    ctx.add_input({"Kind": "commit", "Sha": sha})
    return sha


def parse_ls_tree(out):
    found = {}
    for rec in out.split(b"\0"):
        if not rec:
            continue
        meta, sep, path = rec.partition(b"\t")
        parts = meta.split(b" ")
        if not sep or len(parts) != 3:
            raise FactsError("GIT_ERROR", "unexpected ls-tree record")
        found[git_text(path)] = (parts[1].decode("ascii"), parts[2].decode("ascii"))
    return found


def blobs_at(ctx, rev, paths):
    pending = []
    for p in paths:
        if (rev, p) in ctx._blob_cache or p in pending:
            continue
        if p == "" or p.endswith("/"):
            ctx._blob_cache[(rev, p)] = None
        else:
            pending.append(p)
    for i in range(0, len(pending), 64):
        chunk = pending[i:i + 64]
        _, out = git_exec(ctx, ["ls-tree", "-r", "-z", "--full-tree", rev, "--"]
                          + [lit(p) for p in chunk])
        found = parse_ls_tree(out)
        for p in chunk:
            ent = found.get(p)
            ctx._blob_cache[(rev, p)] = ent[1] if ent is not None and ent[0] == "blob" else None
    return [ctx._blob_cache[(rev, p)] for p in paths]


def list_under(ctx, rev, surfaces):
    specs = [s for s in surfaces if s != ""]
    if not specs:
        return {}
    found = {}
    for i in range(0, len(specs), 64):
        _, out = git_exec(ctx, ["ls-tree", "-r", "-z", "--full-tree", rev, "--"]
                          + [lit(s) for s in specs[i:i + 64]])
        for path, ent in parse_ls_tree(out).items():
            if under(path, surfaces):
                found[path] = ent[1]
    return found


def under(path, surfaces):
    for s in surfaces:
        if path == s or (s.endswith("/") and path.startswith(s)):
            return True
    return False


def read_blob(ctx, rev, path):
    oid = blobs_at(ctx, rev, [path])[0]
    if oid is None:
        return None, None
    if oid not in ctx._content_cache:
        _, out = git_exec(ctx, ["cat-file", "blob", oid])
        ctx._content_cache[oid] = out
    ctx.add_input({"Kind": "git-blob", "Rev": rev, "Path": path, "Blob": oid})
    return oid, ctx._content_cache[oid]


def read_text_at(ctx, rev, path):
    oid, data = read_blob(ctx, rev, path)
    if oid is None:
        return None, None
    if oid not in ctx._text_cache:
        try:
            ctx._text_cache[oid] = data.decode("utf-8")
        except UnicodeDecodeError:
            raise FactsError("MISSING_INPUT", "%s at %s is not UTF-8 text" % (path, rev))
    return oid, ctx._text_cache[oid]


def read_file(ctx, a, name, allow_absent=False):
    path = a["file"][name]
    declared = a["sha256"][name].lower()
    try:
        with open(path, "rb") as fh:
            data = fh.read()
    except FileNotFoundError:
        if allow_absent:
            return None
        raise FactsError("MISSING_INPUT", "file %s not found" % name)
    except OSError:
        raise FactsError("MISSING_INPUT", "file %s is not readable" % name)
    actual = hashlib.sha256(data).hexdigest()
    if actual != declared:
        raise FactsError("INPUT_HASH_MISMATCH", "file %s: sha256 %s, declared %s"
                         % (name, actual, declared))
    ctx.add_input({"Kind": "file", "Name": name, "Sha256": actual})
    return data


def load_json_strict(data):
    """(True, value) for strict RFC 8259 JSON in UTF-8 with unique keys; else (False, None)."""
    try:
        text = data.decode("utf-8")
    except UnicodeDecodeError:
        return False, None

    def pairs_hook(pairs):
        obj = {}
        for key, value in pairs:
            if key in obj:
                raise ValueError("duplicate key")
            obj[key] = value
        return obj

    def bad_constant(name):
        raise ValueError("non-JSON constant")

    try:
        return True, json.loads(text, object_pairs_hook=pairs_hook, parse_constant=bad_constant)
    except (ValueError, RecursionError):
        return False, None


def json_input(data, what):
    ok, value = load_json_strict(data)
    if not ok:
        raise FactsError("MALFORMED_JSON", "%s is not valid JSON" % what)
    return value


def need_obj(value, what):
    if not isinstance(value, dict):
        raise FactsError("MISSING_INPUT", "%s is not a JSON object" % what)
    return value


def need_field(obj, key, kind, what):
    if key not in obj:
        raise FactsError("MISSING_INPUT", "%s lacks field %s" % (what, key))
    value = obj[key]
    if kind == "str-list":
        ok = isinstance(value, list) and all(isinstance(x, str) for x in value)
    elif kind == "obj-list":
        ok = isinstance(value, list) and all(isinstance(x, dict) for x in value)
    elif kind == "str":
        ok = isinstance(value, str)
    elif kind == "list":
        ok = isinstance(value, list)
    else:
        ok = True
    if not ok:
        raise FactsError("MISSING_INPUT", "%s field %s has an unusable type" % (what, key))
    return value


# ----------------------------------------------------------- markdown model

class Md(object):
    def __init__(self, text):
        self.lines = lf(text).split("\n")
        self.levels = []
        in_code = False
        for line in self.lines:
            if line.startswith("```"):
                in_code = not in_code
                self.levels.append(0)
                continue
            level = 0
            if not in_code:
                m = HEADING_RE.match(line)
                if m:
                    level = len(m.group(1))
            self.levels.append(level)
        self.heads = [k for k, lvl in enumerate(self.levels) if lvl > 0]

    def section_end(self, k):
        level = self.levels[k]
        for k2 in self.heads:
            if k2 > k and self.levels[k2] <= level:
                return k2
        return len(self.lines)

    def preamble_end(self):
        for k in self.heads:
            if self.levels[k] == 2:
                return k
        return len(self.lines)

    def find_exact(self, level, heading):
        target = norm(heading)
        return [k for k in self.heads if self.levels[k] == level and norm(self.lines[k]) == target]

    def find_regex(self, level, rx):
        return [k for k in self.heads if self.levels[k] == level and rx.match(norm(self.lines[k]))]

    def section_text(self, k):
        return "\n".join(self.lines[k:self.section_end(k)])

    def body_text(self, k):
        return "\n".join(self.lines[k + 1:self.section_end(k)])

    def heading_map(self):
        result = {PREAMBLE: [norm("\n".join(self.lines[:self.preamble_end()]))]}
        for k in self.heads:
            result.setdefault(norm(self.lines[k]), []).append(norm(self.section_text(k)))
        return result

    def count_heading(self, section):
        target = norm(section)
        if target == PREAMBLE:
            return 1
        return len([k for k in self.heads if norm(self.lines[k]) == target])


def md_or_none(text):
    return Md(text) if text is not None else None


# ------------------------------------------------------- JSON schema subset

def is_num(x):
    return isinstance(x, (int, float)) and not isinstance(x, bool)


def type_ok(x, t):
    if t == "null":
        return x is None
    if t == "boolean":
        return isinstance(x, bool)
    if t == "object":
        return isinstance(x, dict)
    if t == "array":
        return isinstance(x, list)
    if t == "string":
        return isinstance(x, str)
    if t == "number":
        return is_num(x)
    if t == "integer":
        return (isinstance(x, int) and not isinstance(x, bool)) or (isinstance(x, float) and x.is_integer())
    return False


def jeq(a, b):
    if is_num(a) and is_num(b):
        return a == b
    if isinstance(a, bool) or isinstance(b, bool):
        return isinstance(a, bool) and isinstance(b, bool) and a == b
    if isinstance(a, dict) and isinstance(b, dict):
        return set(a) == set(b) and all(jeq(a[k], b[k]) for k in a)
    if isinstance(a, list) and isinstance(b, list):
        return len(a) == len(b) and all(jeq(x, y) for x, y in zip(a, b))
    if a is None or b is None:
        return a is None and b is None
    if isinstance(a, str) and isinstance(b, str):
        return a == b
    return False


def ecma_pattern(pattern):
    out = []
    i = 0
    in_class = False
    while i < len(pattern):
        c = pattern[i]
        if c == "\\" and i + 1 < len(pattern):
            out.append(pattern[i:i + 2])
            i += 2
            continue
        if in_class:
            if c == "]":
                in_class = False
            out.append(c)
        elif c == "[":
            in_class = True
            out.append(c)
        elif c == "$":
            out.append("\\Z")
        else:
            out.append(c)
        i += 1
    return re.compile("".join(out), re.ASCII)


def schema_unsupported(detail):
    return FactsError("UNSUPPORTED_SCHEMA_KEYWORD", detail)


def schema_check(s, ptr, compiled):
    if isinstance(s, bool):
        return
    if not isinstance(s, dict):
        raise schema_unsupported("schema node at %s is not an object or boolean" % (ptr or "#"))
    for key in sorted(s):
        if key in SCHEMA_ANNOTATIONS:
            continue
        if key not in SCHEMA_KEYWORDS:
            raise schema_unsupported("keyword '%s' at %s" % (key, ptr or "#"))
    if "type" in s:
        types = s["type"] if isinstance(s["type"], list) else [s["type"]]
        if not types or any(t not in JSON_TYPES for t in types):
            raise schema_unsupported("invalid 'type' at %s" % (ptr or "#"))
    if "enum" in s and not isinstance(s["enum"], list):
        raise schema_unsupported("invalid 'enum' at %s" % (ptr or "#"))
    if "required" in s:
        req = s["required"]
        if not isinstance(req, list) or not all(isinstance(x, str) for x in req):
            raise schema_unsupported("invalid 'required' at %s" % (ptr or "#"))
    for key in ("minItems", "maxItems", "minLength"):
        if key in s and not (isinstance(s[key], int) and not isinstance(s[key], bool) and s[key] >= 0):
            raise schema_unsupported("invalid '%s' at %s" % (key, ptr or "#"))
    if "uniqueItems" in s and not isinstance(s["uniqueItems"], bool):
        raise schema_unsupported("invalid 'uniqueItems' at %s" % (ptr or "#"))
    for key in ("minimum", "maximum"):
        if key in s and not is_num(s[key]):
            raise schema_unsupported("invalid '%s' at %s" % (key, ptr or "#"))
    if "pattern" in s:
        if not isinstance(s["pattern"], str):
            raise schema_unsupported("invalid 'pattern' at %s" % (ptr or "#"))
        try:
            compiled[s["pattern"]] = ecma_pattern(s["pattern"])
        except re.error:
            raise schema_unsupported("pattern not supported at %s" % (ptr or "#"))
    if "properties" in s:
        props = s["properties"]
        if not isinstance(props, dict):
            raise schema_unsupported("invalid 'properties' at %s" % (ptr or "#"))
        for name in sorted(props):
            schema_check(props[name], ptr + "/properties/" + esc_pointer(name), compiled)
    if "additionalProperties" in s:
        schema_check(s["additionalProperties"], ptr + "/additionalProperties", compiled)
    if "items" in s:
        if isinstance(s["items"], list):
            raise schema_unsupported("array form of 'items' at %s" % (ptr or "#"))
        schema_check(s["items"], ptr + "/items", compiled)


def schema_validate(x, s, ptr, out, compiled):
    if s is True:
        return
    if s is False:
        out.append((ptr, "false"))
        return
    if "type" in s:
        types = s["type"] if isinstance(s["type"], list) else [s["type"]]
        if not any(type_ok(x, t) for t in types):
            out.append((ptr, "type"))
    if "enum" in s and not any(jeq(x, e) for e in s["enum"]):
        out.append((ptr, "enum"))
    if "const" in s and not jeq(x, s["const"]):
        out.append((ptr, "const"))
    if isinstance(x, dict):
        for key in s.get("required", []):
            if key not in x:
                out.append((ptr + "/" + esc_pointer(key), "required"))
        props = s.get("properties", {})
        for key, value in x.items():
            child = ptr + "/" + esc_pointer(key)
            if key in props:
                schema_validate(value, props[key], child, out, compiled)
            elif "additionalProperties" in s:
                extra = s["additionalProperties"]
                if extra is False:
                    out.append((child, "additionalProperties"))
                elif isinstance(extra, dict):
                    schema_validate(value, extra, child, out, compiled)
    if isinstance(x, list):
        if "items" in s:
            item_schema = s["items"]
            for i, value in enumerate(x):
                if item_schema is False:
                    out.append((ptr + "/" + str(i), "items"))
                elif isinstance(item_schema, dict):
                    schema_validate(value, item_schema, ptr + "/" + str(i), out, compiled)
        if "minItems" in s and len(x) < s["minItems"]:
            out.append((ptr, "minItems"))
        if "maxItems" in s and len(x) > s["maxItems"]:
            out.append((ptr, "maxItems"))
        if s.get("uniqueItems") is True:
            for i in range(len(x)):
                if any(jeq(x[i], x[j]) for j in range(i + 1, len(x))):
                    out.append((ptr, "uniqueItems"))
                    break
    if isinstance(x, str):
        if "minLength" in s and len(x) < s["minLength"]:
            out.append((ptr, "minLength"))
        if "pattern" in s and compiled[s["pattern"]].search(x) is None:
            out.append((ptr, "pattern"))
    if is_num(x):
        if "minimum" in s and x < s["minimum"]:
            out.append((ptr, "minimum"))
        if "maximum" in s and x > s["maximum"]:
            out.append((ptr, "maximum"))


# ------------------------------------------------------------------ commands

def is_ancestor(ctx, a, b):
    rc, _ = git_exec(ctx, ["merge-base", "--is-ancestor", a, b], (0, 1))
    return rc == 0


def optional_ref(ctx, ref):
    rc, out = git_exec(ctx, ["rev-parse", "--verify", "--quiet", ref], (0, 1))
    if rc == 1:
        return None
    return git_text(out).strip()


def git_one_line(ctx, args):
    _, out = git_exec(ctx, args)
    return git_text(out).strip()


def cmd_identity(ctx, a):
    base = need_commit(ctx, a["base"], "--base")
    current = need_commit(ctx, a["current"], "--current")
    red = need_commit(ctx, a["red"], "--red") if "red" in a else None
    head = git_one_line(ctx, ["rev-parse", "HEAD"])
    abbrev = git_one_line(ctx, ["rev-parse", "--abbrev-ref", "HEAD"])
    top = git_one_line(ctx, ["rev-parse", "--show-toplevel"]).replace("\\", "/")
    origin_branch = optional_ref(ctx, "refs/remotes/origin/" + a["branch"])
    origin_main = optional_ref(ctx, "refs/remotes/origin/main")
    ancestry = {"base->current": is_ancestor(ctx, base, current)}
    if red is not None:
        ancestry["base->red"] = is_ancestor(ctx, base, red)
        ancestry["red->current"] = is_ancestor(ctx, red, current)
    return {"Head": head, "AbbrevRef": abbrev, "Toplevel": top, "OriginBranch": origin_branch,
            "OriginMain": origin_main, "Ancestry": ancestry}


def diff_paths(ctx, a, b, path=None):
    args = ["diff", "--name-only", "--no-renames", "-z"]
    if path is None:
        args.append(a + ".." + b)
    else:
        args += [a, b, "--", lit(path)]
    _, out = git_exec(ctx, args)
    return [git_text(x) for x in out.split(b"\0") if x]


def cmd_diff(ctx, a):
    frm = need_commit(ctx, a["from"], "--from")
    to = need_commit(ctx, a["to"], "--to")
    paths = diff_paths(ctx, frm, to)
    _, out = git_exec(ctx, ["rev-list", "--reverse", frm + ".." + to])
    commits = git_lines(out)
    coauthors = {}
    for c in commits:
        _, out = git_exec(ctx, ["log", "--no-show-signature", "-1",
                                "--format=%(trailers:key=Co-Authored-By,valueonly)", c])
        coauthors[c] = [ln for ln in lf(git_text(out)).split("\n") if ln.strip() != ""]
    return {"Paths": paths, "Commits": commits, "CoAuthoredBy": coauthors}


def u19(entry):
    if entry.endswith("/**"):
        return entry[:-2]
    return entry


def covers(entry, path):
    return path == entry or (entry.endswith("/") and path.startswith(entry))


def malformed_reasons(entry):
    reasons = []
    if entry.startswith("/") or re.match(r"[A-Za-z]:", entry):
        reasons.append("absolute")
    if ".." in re.split(r"[/\\]", entry):
        reasons.append("dotdot")
    core = entry[:-3] if entry.endswith("/**") else entry
    if any(c in core for c in "*?["):
        reasons.append("wildcard")
    if "\\" in entry:
        reasons.append("backslash")
    return reasons


def cmd_scope(ctx, a):
    frm = need_commit(ctx, a["from"], "--from")
    to = need_commit(ctx, a["to"], "--to")
    deleg = need_obj(json_input(read_file(ctx, a, "DELEG"), "DELEG"), "DELEG")
    allowed = need_field(deleg, "AllowedWriteScope", "str-list", "DELEG")
    forbidden = need_field(deleg, "ForbiddenWriteScope", "str-list", "DELEG")
    paths = diff_paths(ctx, frm, to)
    per_path = []
    for p in paths:
        per_path.append({
            "Path": p,
            "AllowedCoveredBy": [e for e in allowed if covers(u19(e), p)],
            "ForbiddenCoveredBy": [e for e in forbidden if covers(u19(e), p)],
        })
    malformed = []
    normalized = []
    for list_name, entries in (("AllowedWriteScope", allowed), ("ForbiddenWriteScope", forbidden)):
        for e in entries:
            reasons = malformed_reasons(e)
            if reasons:
                malformed.append({"List": list_name, "Entry": e, "Reasons": reasons})
            if e.endswith("/**"):
                normalized.append({"List": list_name, "Entry": e, "Prefix": u19(e)})
    return {"Paths": paths, "PerPath": per_path, "AllowedEntries": allowed,
            "ForbiddenEntries": forbidden, "MalformedEntries": malformed,
            "U19Normalized": normalized}


def cmd_contract(ctx, a):
    contract = need_obj(json_input(read_file(ctx, a, "CONTRACT"), "CONTRACT"), "CONTRACT")
    deleg = need_obj(json_input(read_file(ctx, a, "DELEG"), "DELEG"), "DELEG")
    for key in GATE_CONTRACT_V2_REQUIRED:
        if key not in contract:
            raise FactsError("MISSING_INPUT", "CONTRACT lacks required field %s" % key)
    kinds = (("AllowedWriteScope", "str-list"), ("ForbiddenWriteScope", "str-list"),
             ("Invariants", "str-list"), ("StopConditions", "obj-list"),
             ("Authorities", "obj-list"), ("RequiredTests", "obj-list"),
             ("RoleRequirements", "obj-list"), ("SupersededCommits", "list"))
    c = {}
    d = {}
    for key, kind in kinds:
        c[key] = need_field(contract, key, kind, "CONTRACT")
        d[key] = need_field(deleg, key, kind, "DELEG")

    def field(obj, key, what):
        if key not in obj or not isinstance(obj[key], str):
            raise FactsError("MISSING_INPUT", "%s item lacks string field %s" % (what, key))
        return obj[key]

    a3 = [{"Entry": e, "ContainedBy": [b for b in c["AllowedWriteScope"] if covers(b, e)]}
          for e in d["AllowedWriteScope"]]
    a4 = [{"Entry": e, "ContainedBy": [g for g in d["ForbiddenWriteScope"] if covers(g, e)]}
          for e in c["ForbiddenWriteScope"]]

    d_inv = set(d["Invariants"])
    d_stop = set(field(x, "Id", "DELEG StopConditions") for x in d["StopConditions"])
    d_auth = set("|".join([field(x, "Path", "DELEG Authorities"), field(x, "Section", "DELEG Authorities"),
                           field(x, "Class", "DELEG Authorities")]) for x in d["Authorities"])
    d_tests = [field(x, "Filter", "DELEG RequiredTests") for x in d["RequiredTests"]]
    d_roles = set(field(x, "Role", "DELEG RoleRequirements") for x in d["RoleRequirements"])

    c_auth = ["|".join([field(x, "Path", "CONTRACT Authorities"), field(x, "Section", "CONTRACT Authorities"),
                        field(x, "Class", "CONTRACT Authorities")]) for x in c["Authorities"]]
    c_tests = [field(x, "Filter", "CONTRACT RequiredTests") for x in c["RequiredTests"]]
    missing = {
        "Invariants": [x for x in c["Invariants"] if x not in d_inv],
        "StopConditions": [i for i in (field(x, "Id", "CONTRACT StopConditions") for x in c["StopConditions"])
                           if i not in d_stop],
        "Authorities": [t for t in c_auth if t not in d_auth],
        "RequiredTests": [f for f in c_tests if f not in d_tests],
        "RoleRequirements": [r for r in (field(x, "Role", "CONTRACT RoleRequirements")
                                         for x in c["RoleRequirements"]) if r not in d_roles],
    }
    pairs = []
    for test in c["RequiredTests"]:
        flt = test["Filter"]
        matches = [x for x in d["RequiredTests"] if x.get("Filter") == flt]
        if len(matches) > 1:
            raise FactsError("AMBIGUOUS", "DELEG RequiredTests repeats Filter %s" % flt)
        dm = matches[0] if matches else {}
        pairs.append({"Filter": flt,
                      "ContractMinSelected": test.get("MinSelected"),
                      "DelegMinSelected": dm.get("MinSelected"),
                      "ContractExpectRed": test.get("ExpectRed"),
                      "DelegExpectRed": dm.get("ExpectRed")})
    return {"A3": a3, "A4": a4, "A5Missing": missing, "RequiredTestsPairs": pairs,
            "SupersededEqual": canon(c["SupersededCommits"]) == canon(d["SupersededCommits"])}


def cmd_hashes(ctx, a):
    rev = need_commit(ctx, a["rev"], "--rev")
    top = git_one_line(ctx, ["rev-parse", "--show-toplevel"])
    oids = blobs_at(ctx, rev, a["paths"])
    rows = []
    for p, oid in zip(a["paths"], oids):
        full = os.path.join(top, *p.split("/"))
        raw_sha1 = None
        wt_sha256 = None
        if os.path.isfile(full):
            try:
                with open(full, "rb") as fh:
                    data = fh.read()
            except OSError:
                raise FactsError("MISSING_INPUT", "worktree file %s is not readable" % p)
            raw_sha1 = hashlib.sha1(b"blob " + str(len(data)).encode("ascii") + b"\0" + data).hexdigest()
            wt_sha256 = hashlib.sha256(data).hexdigest()
        rev_sha256 = None
        if oid is not None:
            _, content = read_blob(ctx, rev, p)
            rev_sha256 = hashlib.sha256(content).hexdigest()
        rows.append({"Path": p, "WorktreeRawBlobSha1": raw_sha1, "WorktreeSha256": wt_sha256,
                     "RevBlob": oid, "RevBlobSha256": rev_sha256})
    return {"PerPath": rows}


def cmd_cleantree(ctx, a):
    _, out = git_exec(ctx, ["status", "--porcelain"])
    plain = git_lines(out)
    _, out = git_exec(ctx, ["status", "--porcelain", "--ignored"])
    ignored = git_lines(out)
    rc, out = git_exec(ctx, ["check-ignore", "-v", "--no-index", a["ignore-path"]], (0, 1))
    return {"StatusPorcelain": plain, "StatusIgnored": ignored,
            "CheckIgnoreOutput": git_lines(out), "CheckIgnoreRc": rc}


def non_ascii_facts(texts):
    counts = {}
    total = 0
    for text in texts:
        for ch in text:
            if ord(ch) > 127:
                counts[ord(ch)] = counts.get(ord(ch), 0) + 1
                total += 1
    chars = [{"CodePoint": "U+%04X" % cp, "Count": counts[cp]} for cp in sorted(counts)]
    return total, chars


def cmd_read_text(ctx, a):
    rev = need_commit(ctx, a["rev"], "--rev")
    oid, text = read_text_at(ctx, rev, a["path"])
    if oid is None:
        raise FactsError("MISSING_INPUT", "path %s is absent at %s" % (a["path"], rev))
    lines = split_eol(text)
    first, last = 1, len(lines)
    if "lines" in a:
        m = LINES_RE.fullmatch(a["lines"])
        first, last = int(m.group(1)), int(m.group(2))
        if last > len(lines):
            raise FactsError("MISSING_INPUT", "requested lines %d-%d, file has %d" % (first, last, len(lines)))
    selected = [{"N": n, "Text": lines[n - 1]} for n in range(first, last + 1)]
    total, chars = non_ascii_facts([x["Text"] for x in selected])
    return {"Blob": oid, "Lines": selected, "NonAsciiCount": total, "NonAsciiChars": chars}


class _Absent(object):
    pass


ABSENT = _Absent()


def parse_pointer(pointer):
    if pointer == "":
        return []
    if not pointer.startswith("/"):
        raise FactsError("USAGE", "JSON pointer must be empty or start with '/'")
    tokens = []
    for tok in pointer[1:].split("/"):
        if re.search(r"~(?![01])", tok):
            raise FactsError("USAGE", "invalid '~' escape in JSON pointer")
        tokens.append(tok.replace("~1", "/").replace("~0", "~"))
    return tokens


def resolve_pointer(doc, tokens):
    cur = doc
    for tok in tokens:
        if isinstance(cur, dict):
            if tok not in cur:
                return ABSENT
            cur = cur[tok]
        elif isinstance(cur, list):
            if INDEX_RE.fullmatch(tok) is None or int(tok) >= len(cur):
                return ABSENT
            cur = cur[int(tok)]
        else:
            return ABSENT
    return cur


def cmd_read_json(ctx, a):
    tokens = [(p, parse_pointer(p)) for p in a["pointer"]]
    if "rev" in a:
        rev = need_commit(ctx, a["rev"], "--rev")
        oid, data = read_blob(ctx, rev, a["path"])
        if oid is None:
            raise FactsError("MISSING_INPUT", "path %s is absent at %s" % (a["path"], rev))
    else:
        data = read_file(ctx, a, list(a["file"])[0])
    doc = json_input(data, "input")
    values = {}
    absent = []
    for pointer, toks in tokens:
        value = resolve_pointer(doc, toks)
        if value is ABSENT:
            values[pointer] = {"Absent": True}
            if pointer not in absent:
                absent.append(pointer)
        else:
            values[pointer] = value
    return {"Values": values, "AbsentPointers": sorted(absent)}


def xml_local(tag):
    if not isinstance(tag, str):
        return ""
    return tag.rsplit("}", 1)[-1]


def cmd_trx(ctx, a):
    data = read_file(ctx, a, "TRX")
    if data.startswith(b"\xff\xfe") or data.startswith(b"\xfe\xff"):
        raise FactsError("MALFORMED_XML", "TRX must be UTF-8")
    if b"<!DOCTYPE" in data or b"<!ENTITY" in data:
        raise FactsError("MALFORMED_XML", "TRX with a document type declaration is not accepted")
    try:
        root = ET.fromstring(data)
    except ET.ParseError:
        raise FactsError("MALFORMED_XML", "TRX is not well-formed XML")
    summaries = [e for e in list(root) if xml_local(e.tag) == "ResultSummary"]
    if len(summaries) != 1:
        raise FactsError("MISSING_INPUT" if not summaries else "AMBIGUOUS",
                         "TRX has %d ResultSummary elements" % len(summaries))
    counters = [e for e in list(summaries[0]) if xml_local(e.tag) == "Counters"]
    if len(counters) != 1:
        raise FactsError("MISSING_INPUT" if not counters else "AMBIGUOUS",
                         "TRX has %d ResultSummary/Counters elements" % len(counters))
    counter_values = dict((xml_local(k), v) for k, v in counters[0].attrib.items())
    tests = []
    for e in root.iter():
        if xml_local(e.tag) != "UnitTestResult":
            continue
        if "testName" not in e.attrib or "outcome" not in e.attrib:
            raise FactsError("MISSING_INPUT", "UnitTestResult without testName or outcome")
        tests.append({"TestName": e.attrib["testName"], "Outcome": e.attrib["outcome"]})
    failed = [t["TestName"] for t in tests if t["Outcome"] == "Failed"]
    facts = {"Sha256": hashlib.sha256(data).hexdigest(), "Counters": counter_values, "Tests": tests,
             "FailedTests": failed, "FilterSelected": None, "FilterFailed": None,
             "FilterSelectedCount": None, "FilterFailedCount": None}
    if "filter-substring" in a:
        sub = a["filter-substring"]
        selected = [t["TestName"] for t in tests if sub in t["TestName"]]
        sel_failed = [t["TestName"] for t in tests if sub in t["TestName"] and t["Outcome"] == "Failed"]
        facts.update({"FilterSelected": selected, "FilterFailed": sel_failed,
                      "FilterSelectedCount": len(selected), "FilterFailedCount": len(sel_failed)})
    return facts


def cmd_handoff(ctx, a):
    rev = need_commit(ctx, a["schema-rev"], "--schema-rev")
    data = read_file(ctx, a, "H", allow_absent=bool(a.get("allow-absent")))
    if data is None:
        return {"Present": False}
    oid, schema_bytes = read_blob(ctx, rev, HANDOFF_SCHEMA_PATH)
    if oid is None:
        raise FactsError("MISSING_INPUT", "worker-handoff schema absent at %s" % rev)
    schema = need_obj(json_input(schema_bytes, "worker-handoff schema"), "worker-handoff schema")
    required = need_field(schema, "required", "str-list", "worker-handoff schema")
    sha = hashlib.sha256(data).hexdigest()
    ok, doc = load_json_strict(data)
    if not ok:
        return {"Present": True, "ValidJson": False, "MissingRequiredKeys": None, "Fields": None,
                "AbsentFields": None, "Sha256": sha}
    obj = doc if isinstance(doc, dict) else {}
    fields = {}
    for f in HANDOFF_FIELDS:
        fields[f] = obj[f] if f in obj else {"Absent": True}
    return {"Present": True, "ValidJson": True,
            "MissingRequiredKeys": [k for k in required if k not in obj],
            "Fields": fields, "AbsentFields": [f for f in HANDOFF_FIELDS if f not in obj],
            "Sha256": sha}


def gate_terms(text):
    md = Md(text)
    found = md.find_exact(3, H1610_HEADING)
    if not found:
        raise FactsError("CONTRADICTORY", "16.10 heading not found: 0 terms")
    if len(found) > 1:
        raise FactsError("AMBIGUOUS", "16.10 heading appears %d times" % len(found))
    k = found[0] + 1
    while k < len(md.lines) and md.lines[k].strip() == "":
        k += 1
    para = []
    while k < len(md.lines) and md.lines[k].strip() != "":
        para.append(md.lines[k])
        k += 1
    joined = "\n".join(para)
    cut = joined.find(TERMS_END)
    if cut < 0:
        raise FactsError("CONTRADICTORY", "16.10 paragraph does not reach 'No se aplica'")
    terms = []
    for span in BACKTICK_RE.findall(joined[:cut]):
        term = norm(span)
        if term in FREE_TEXT_FIELDS or term in terms or term == "":
            continue
        terms.append(term)
    if not terms:
        raise FactsError("CONTRADICTORY", "16.10 paragraph yields 0 terms")
    return terms


def word_char(c):
    return c != "" and (c.isalnum() or c == "_")


def count_term(text, term):
    hay = norm(text).casefold()
    needle = norm(term).casefold()
    if needle == "":
        return 0
    count = 0
    i = 0
    while True:
        j = hay.find(needle, i)
        if j < 0:
            return count
        before = hay[j - 1] if j > 0 else ""
        end = j + len(needle)
        after = hay[end] if end < len(hay) else ""
        if not word_char(before) and not word_char(after):
            count += 1
            i = end
        else:
            i = j + 1


def cmd_freetext(ctx, a):
    rev = need_commit(ctx, a["terms-rev"], "--terms-rev")
    doc = need_obj(json_input(read_file(ctx, a, "H"), "H"), "H")
    oid, ap = read_text_at(ctx, rev, AP_PATH)
    if oid is None:
        raise FactsError("MISSING_INPUT", "%s absent at %s" % (AP_PATH, rev))
    terms = gate_terms(ap)
    matches = []
    for f in FREE_TEXT_FIELDS:
        if f not in doc:
            raise FactsError("MISSING_INPUT", "H lacks field %s" % f)
        value = doc[f]
        if isinstance(value, str):
            texts = [value]
        elif isinstance(value, list) and all(isinstance(x, str) for x in value):
            texts = value
        else:
            raise FactsError("MISSING_INPUT", "H field %s is not text" % f)
        for term in terms:
            n = sum(count_term(t, term) for t in texts)
            if n > 0:
                matches.append({"Field": f, "Term": term, "Count": n})
    matches.sort(key=lambda m: (m["Field"], m["Term"]))
    return {"Terms": terms, "Fields": list(FREE_TEXT_FIELDS), "Matches": matches}


# ----------------------------------------------------------------- clausemap

def descendants(parents, root):
    children = {}
    for c, ps in parents.items():
        for p in ps:
            children.setdefault(p, []).append(c)
    seen = set([root])
    stack = [root]
    while stack:
        cur = stack.pop()
        for ch in children.get(cur, []):
            if ch not in seen:
                seen.add(ch)
                stack.append(ch)
    return seen


def derive(ctx, main):
    _, out = git_exec(ctx, ["rev-list", "--first-parent", "--merges", main])
    merges = list(reversed(git_lines(out)))
    _, out = git_exec(ctx, ["log", "--no-show-signature", "-z", "--format=%H%n%B", main])
    trailer_commits = []
    for rec in out.split(b"\0"):
        if not rec.strip():
            continue
        sha, _, body = lf(git_text(rec)).lstrip("\n").partition("\n")
        if SHA40_RE.fullmatch(sha) is None:
            raise FactsError("GIT_ERROR", "unexpected log record")
        if TRAILER in body.split("\n"):
            trailer_commits.append(sha)
    if len(trailer_commits) > 1:
        raise FactsError("AMBIGUOUS", "trailer line found in %d commits" % len(trailer_commits))
    _, out = git_exec(ctx, ["rev-list", "--parents", main])
    parents = {}
    for line in git_lines(out):
        parts = line.split(" ")
        parents[parts[0]] = parts[1:]
    candidates = []
    if trailer_commits:
        reach = descendants(parents, trailer_commits[0])
        for m in merges:
            ps = parents.get(m, [])
            if len(ps) >= 2 and ps[1] in reach and ps[0] not in reach:
                candidates.append(m)
        if len(candidates) != 1:
            raise FactsError("AMBIGUOUS", "unique trailer commit with %d qualifying first-parent merges"
                             % len(candidates))
    eff = candidates[0] if candidates else None
    return {"TrailerCommits": trailer_commits, "FirstParentMerges": merges, "Candidates": candidates,
            "Eff": eff, "EffParent1": parents[eff][0] if eff else None,
            "EffParent2": parents[eff][1] if eff else None}


def table_cells(line):
    cells = [c.strip() for c in line.split("|")]
    if cells and cells[0] == "":
        cells = cells[1:]
    if cells and cells[-1] == "":
        cells = cells[:-1]
    return cells


def pre_table(ctx, eff):
    _, out = git_exec(ctx, ["show", "--no-show-signature", "-s", "--format=%B", eff])
    lines = lf(git_text(out)).split("\n")
    header = None
    for i, line in enumerate(lines):
        s = line.lstrip(" \t")
        if s.startswith("|"):
            s = s[1:].lstrip(" \t")
        if s.startswith("Initiative |"):
            header = i
            break
    if header is None:
        return {"HeaderFound": False, "HeaderCells": None, "Rows": [], "MalformedRows": [],
                "ClaimIdColumn": None, "ClaimIdCounts": None}
    cells = table_cells(lines[header])
    rows = []
    k = header + 1
    while k < len(lines) and lines[k].strip() != "" and "|" in lines[k]:
        rows.append(table_cells(lines[k]))
        k += 1
    malformed = [{"RowIndex": i, "CellCount": len(r)} for i, r in enumerate(rows) if len(r) != len(cells)]
    cols = [i for i, c in enumerate(cells) if c == "Claim-Id"]
    if len(cols) > 1:
        raise FactsError("AMBIGUOUS", "PRE header has %d Claim-Id cells" % len(cols))
    col = cols[0] if cols else None
    counts = None
    if col is not None:
        counts = {}
        for r in rows:
            if len(r) > col:
                counts[r[col]] = counts.get(r[col], 0) + 1
    return {"HeaderFound": True, "HeaderCells": cells, "Rows": rows, "MalformedRows": malformed,
            "ClaimIdColumn": col, "ClaimIdCounts": counts}


def classify(ctx, state_rev, state_path, pre):
    oid, text = read_text_at(ctx, state_rev, state_path)
    if oid is None:
        raise FactsError("MISSING_INPUT", "state %s absent at %s" % (state_path, state_rev))
    values = []
    for line in lf(text).split("\n"):
        m = CLAIM_RE.match(line)
        if m:
            values.append(m.group(1))
    distinct = []
    for v in values:
        if v not in distinct:
            distinct.append(v)
    if len(distinct) > 1:
        raise FactsError("AMBIGUOUS", "state has %d distinct claim_id values" % len(distinct))
    in_pre = None
    if len(distinct) == 1 and pre is not None and pre["ClaimIdCounts"] is not None:
        in_pre = pre["ClaimIdCounts"].get(distinct[0], 0)
    return {"ClaimIdLines": values, "ClaimIdInPre": in_pre}


def entry_names_r163(md, k):
    return R163_HEADING in md.section_text(k)


def entry_facts(ctx, rev):
    wf_oid, wf = read_text_at(ctx, rev, WORKFLOW_PATH)
    ap_oid, ap = read_text_at(ctx, rev, AP_PATH)
    wf_md = md_or_none(wf)
    ap_md = md_or_none(ap)
    em = wf_md.find_exact(2, ENTRY_HEADING) if wf_md else []
    rm = ap_md.find_exact(3, R163_HEADING) if ap_md else []
    return {"WorkflowBlob": wf_oid, "ApBlob": ap_oid,
            "EntryMatches": [k + 1 for k in em], "R163Matches": [k + 1 for k in rm],
            "EntryNamesR163": entry_names_r163(wf_md, em[0]) if len(em) == 1 else None}


def closed_list(ap_md):
    starts = [k for k, line in enumerate(ap_md.lines) if line.startswith(SURFACES_LEAD)]
    if not starts:
        raise FactsError("MISSING_INPUT", "closed surface list paragraph not found at Eff")
    if len(starts) > 1:
        raise FactsError("AMBIGUOUS", "closed surface list paragraph appears %d times" % len(starts))
    para = []
    k = starts[0]
    while k < len(ap_md.lines) and ap_md.lines[k].strip() != "":
        para.append(ap_md.lines[k])
        k += 1
    values = [norm(v) for v in BACKTICK_RE.findall("\n".join(para))]
    if not values:
        raise FactsError("CONTRADICTORY", "closed surface list yields 0 values")
    return values


def map_shape_ok(m):
    if not isinstance(m, dict):
        return False
    if not (isinstance(m.get("Surfaces"), list) and all(isinstance(x, str) for x in m["Surfaces"])):
        return False
    files = m.get("Files")
    entries = m.get("Entries")
    if not isinstance(files, list) or not isinstance(entries, list):
        return False
    for f in files:
        if not (isinstance(f, dict) and isinstance(f.get("Path"), str) and isinstance(f.get("FileKind"), str)
                and (f.get("BaseBlob") is None or isinstance(f.get("BaseBlob"), str))
                and isinstance(f.get("EffBlob"), str)):
            return False
    for e in entries:
        if not (isinstance(e, dict) and isinstance(e.get("Path"), str) and isinstance(e.get("Section"), str)
                and isinstance(e.get("Kind"), str)):
            return False
    return True


def mv7_facts(ctx, eff, eff1):
    _, wf_eff = read_text_at(ctx, eff, WORKFLOW_PATH)
    _, ap_eff = read_text_at(ctx, eff, AP_PATH)
    _, ap_eff1 = read_text_at(ctx, eff1, AP_PATH)
    wf_md = md_or_none(wf_eff)
    em = wf_md.find_exact(2, ENTRY_HEADING) if wf_md else []
    ap_md = md_or_none(ap_eff)
    ap1_md = md_or_none(ap_eff1)
    s16 = ap_md.find_regex(2, S16_RE) if ap_md else []
    s163 = ap_md.find_regex(3, S163_RE) if ap_md else []
    s163_1 = ap1_md.find_regex(3, S163_RE) if ap1_md else []
    first16 = None
    if len(s16) == 1:
        sents = sentences(norm(ap_md.body_text(s16[0])))
        first16 = sents[0] if sents else None
    last163 = None
    minus = None
    if len(s163) == 1:
        sents = sentences(norm(ap_md.body_text(s163[0])))
        last163 = sents[-1] if sents else None
        idx = [i for i, s in enumerate(sents) if s == POINTER]
        if idx:
            del sents[idx[-1]]
        minus = " ".join(sents)
    base163 = norm(ap1_md.body_text(s163_1[0])) if len(s163_1) == 1 else None
    return {"EntryCountEff": len(em),
            "EntryNamesR163Eff": entry_names_r163(wf_md, em[0]) if len(em) == 1 else None,
            "S16Count": len(s16), "FirstSentenceS16": first16,
            "PointerIsFirstSentenceS16": first16 == POINTER,
            "S163CountEff": len(s163), "S163CountEff1": len(s163_1),
            "LastSentenceS163": last163, "PointerIsLastSentenceS163": last163 == POINTER,
            "S163EffMinusPointer": minus, "S163Eff1": base163,
            "S163EqualAfterPointerRemoval": minus is not None and base163 is not None and minus == base163}


def validate_map(ctx, eff, eff1):
    map_oid, map_bytes = read_blob(ctx, eff, MAP_PATH)
    sch_oid, sch_bytes = read_blob(ctx, eff, MAP_SCHEMA_PATH)
    mv1 = {"MapPresent": map_oid is not None, "MapBlob": map_oid, "MapParses": None,
           "SchemaPresent": sch_oid is not None, "SchemaBlob": sch_oid, "SchemaViolations": None}
    doc = None
    if map_oid is not None:
        ok, doc = load_json_strict(map_bytes)
        mv1["MapParses"] = ok
    schema = None
    compiled = {}
    if sch_oid is not None:
        schema = json_input(sch_bytes, "clause-map schema at Eff")
        schema_check(schema, "", compiled)
    if mv1["MapParses"] and schema is not None:
        found = []
        schema_validate(doc, schema, "", found, compiled)
        mv1["SchemaViolations"] = [{"Pointer": p, "Keyword": k} for p, k in sorted(set(found))]
    usable = mv1["SchemaViolations"] == []
    if usable and not map_shape_ok(doc):
        raise FactsError("CONTRADICTORY", "clause map conforms to its schema but lacks the 16.13 shape")
    ap_oid, ap_eff = read_text_at(ctx, eff, AP_PATH)
    if ap_oid is None:
        raise FactsError("MISSING_INPUT", "%s absent at Eff" % AP_PATH)
    closed = closed_list(Md(ap_eff))
    result = {"MV1": mv1, "MV2": {"MapSurfaces": doc["Surfaces"] if usable else None, "ClosedList": closed},
              "MV3": None, "MV4": None, "MV5": None,
              "MV6": {"PerFile": None, "EntryKindEntries": None,
                      "ExpectedEntryKind": [{"Path": AP_PATH, "Section": R163_HEADING},
                                            {"Path": WORKFLOW_PATH, "Section": ENTRY_HEADING}]},
              "MV7": None}
    if usable:
        files = doc["Files"]
        entries = doc["Entries"]
        surfaces = doc["Surfaces"]
        # MV-3
        t1 = list_under(ctx, eff1, surfaces)
        t2 = list_under(ctx, eff, surfaces)
        changed = sorted(p for p in set(t1) | set(t2) if t1.get(p) != t2.get(p) and p != MAP_PATH)
        deleted = sorted(p for p in t1 if p not in t2 and p != MAP_PATH)
        fpaths = [f["Path"] for f in files]
        result["MV3"] = {"ChangedUnderSurfaces": changed, "DeletedUnderSurfaces": deleted,
                         "FilesPaths": fpaths,
                         "FilesDuplicates": sorted(set(p for p in fpaths if fpaths.count(p) > 1)),
                         "OnlyInChanged": sorted(set(changed) - set(fpaths)),
                         "OnlyInFiles": sorted(set(fpaths) - set(changed))}
        # MV-4
        obs1 = dict(zip(fpaths, blobs_at(ctx, eff1, fpaths)))
        obs2 = dict(zip(fpaths, blobs_at(ctx, eff, fpaths)))
        rows = []
        literal = []
        entry_outside = []
        added_in_eff1 = []
        entry_in_eff1 = []
        for f in files:
            p = f["Path"]
            kind = f["FileKind"]
            o1 = obs1[p]
            o2 = obs2[p]
            rows.append({"Path": p, "FileKind": kind, "MapBaseBlob": f["BaseBlob"], "MapEffBlob": f["EffBlob"],
                         "ObservedEff1Blob": o1, "ObservedEffBlob": o2})
            if kind == "MODIFIED":
                if o1 is None or f["BaseBlob"] != o1 or o2 is None or f["EffBlob"] != o2:
                    literal.append(p)
            elif kind in ("ADDED", "ENTRY"):
                if f["BaseBlob"] is not None or o2 is None or f["EffBlob"] != o2:
                    literal.append(p)
            if kind == "ENTRY" and p != MAP_SCHEMA_PATH:
                entry_outside.append(p)
            if kind == "ADDED" and o1 is not None:
                added_in_eff1.append(p)
            if kind == "ENTRY" and o1 is not None:
                entry_in_eff1.append(p)
        result["MV4"] = {"Rows": rows, "LiteralViolations": literal,
                         "EntryFilesOutsideSchema": entry_outside,
                         "AddedPresentInEff1": added_in_eff1,
                         "EntryFilePresentInEff1": entry_in_eff1}
        # MV-5
        modified_paths = set(f["Path"] for f in files if f["FileKind"] == "MODIFIED")
        keys = [(e["Path"], norm(e["Section"])) for e in entries]
        result["MV5"] = {
            "EntryKeyDuplicates": [{"Path": p, "Section": s}
                                   for p, s in sorted(set(k for k in keys if keys.count(k) > 1))],
            "EntriesOnNonModifiedPath": [{"Path": e["Path"], "Section": e["Section"], "Kind": e["Kind"]}
                                         for e in entries if e["Path"] not in modified_paths]}
        # MV-6
        per_file = []
        seen = []
        for f in files:
            p = f["Path"]
            if f["FileKind"] != "MODIFIED" or p in seen:
                continue
            seen.append(p)
            _, text1 = read_text_at(ctx, eff1, p)
            _, text2 = read_text_at(ctx, eff, p)
            h1 = Md(text1).heading_map() if text1 is not None else {}
            h2 = Md(text2).heading_map() if text2 is not None else {}
            mine = [e for e in entries if e["Path"] == p]
            per_file.append({
                "Path": p,
                "H1Duplicates": sorted(h for h in h1 if len(h1[h]) > 1),
                "H2Duplicates": sorted(h for h in h2 if len(h2[h]) > 1),
                "DerivedModified": sorted(h for h in h1 if h in h2 and h1[h] != h2[h]),
                "DerivedRemoved": sorted(h for h in h1 if h not in h2),
                "DerivedAddedOrEntry": sorted(h for h in h2 if h not in h1),
                "MapModified": sorted(set(norm(e["Section"]) for e in mine if e["Kind"] == "MODIFIED")),
                "MapRemoved": sorted(set(norm(e["Section"]) for e in mine if e["Kind"] == "REMOVED")),
                "MapAddedOrEntry": sorted(set(norm(e["Section"]) for e in mine
                                              if e["Kind"] in ("ADDED", "ENTRY"))),
            })
        result["MV6"]["PerFile"] = per_file
        result["MV6"]["EntryKindEntries"] = [{"Path": e["Path"], "Section": e["Section"]}
                                             for e in entries if e["Kind"] == "ENTRY"]
    result["MV7"] = mv7_facts(ctx, eff, eff1)
    return result, (doc if usable else None)


def match_at(ctx, rev, path, section):
    _, text = read_text_at(ctx, rev, path)
    if text is None:
        return 0
    return Md(text).count_heading(section)


def resolve(ctx, main, base, contract, derived, mapdoc):
    ar = need_field(contract, "AuthorityRevision", "str", "CONTRACT")
    contract_main = need_field(contract, "MainSha", "str", "CONTRACT")
    auths = need_field(contract, "Authorities", "obj-list", "CONTRACT")
    need_commit(ctx, ar, "CONTRACT.AuthorityRevision")
    mb = git_one_line(ctx, ["merge-base", main, ar])
    eff = derived["Eff"]
    eff1 = derived["EffParent1"]
    citations = []
    for item in auths:
        for key in ("Path", "Section", "Class"):
            if not isinstance(item.get(key), str):
                raise FactsError("MISSING_INPUT", "CONTRACT Authorities item lacks string field %s" % key)
        path = item["Path"]
        section = item["Section"]
        file_kind = None
        entry_kinds = None
        if mapdoc is not None:
            kinds = [f["FileKind"] for f in mapdoc["Files"] if f["Path"] == path]
            if len(kinds) > 1:
                raise FactsError("AMBIGUOUS", "clause map lists %s %d times" % (path, len(kinds)))
            file_kind = kinds[0] if kinds else None
            entry_kinds = [e["Kind"] for e in mapdoc["Entries"]
                           if e["Path"] == path and norm(e["Section"]) == norm(section)]
        citations.append({
            "Path": path, "Section": section, "Class": item["Class"],
            "MapFileKind": file_kind, "MapEntryKinds": entry_kinds,
            "MatchEff1": match_at(ctx, eff1, path, section) if eff else None,
            "MatchEff": match_at(ctx, eff, path, section) if eff else None,
            "MatchMain": match_at(ctx, main, path, section),
            "ExtDiffMbAr": diff_paths(ctx, mb, ar, path) if item["Class"] == "EXTERNAL" else None,
        })
    return {"Ar": ar, "MainSha": main, "ContractMainSha": contract_main, "Mb": mb,
            "Citations": citations,
            "ArBaseDiff": diff_paths(ctx, ar, base) if base is not None else None}


def cmd_clausemap(ctx, a):
    main = need_commit(ctx, a["main"], "--main")
    state_rev = need_commit(ctx, a["state-rev"], "--state-rev") if "state-rev" in a else None
    base = need_commit(ctx, a["base"], "--base") if "base" in a else None
    contract = None
    if "CONTRACT" in a.get("file", {}):
        contract = need_obj(json_input(read_file(ctx, a, "CONTRACT"), "CONTRACT"), "CONTRACT")
    derived = derive(ctx, main)
    eff = derived["Eff"]
    pre = pre_table(ctx, eff) if eff else None
    cls = classify(ctx, state_rev, a["state-path"], pre) if state_rev else None
    entry = entry_facts(ctx, main)
    validate = None
    mapdoc = None
    if eff:
        validate, mapdoc = validate_map(ctx, eff, derived["EffParent1"])
    res = resolve(ctx, main, base, contract, derived, mapdoc) if contract is not None else None
    return {"Derive": derived, "Pre": pre, "Classify": cls, "Entry": entry, "Validate": validate,
            "Resolve": res}


COMMAND_FUNCS = {
    "identity": cmd_identity, "diff": cmd_diff, "scope": cmd_scope, "contract": cmd_contract,
    "hashes": cmd_hashes, "cleantree": cmd_cleantree, "read-text": cmd_read_text,
    "read-json": cmd_read_json, "trx": cmd_trx, "handoff": cmd_handoff, "freetext": cmd_freetext,
    "clausemap": cmd_clausemap,
}

# option kinds: req, opt, flag, list (one or more values), multi (repeatable)
COMMAND_SPECS = {
    "identity": ({"repo": "req", "branch": "req", "base": "req", "current": "req", "red": "opt"}, (), ()),
    "diff": ({"repo": "req", "from": "req", "to": "req"}, (), ()),
    "scope": ({"repo": "req", "from": "req", "to": "req"}, ("DELEG",), ()),
    "contract": ({"repo": "req"}, ("CONTRACT", "DELEG"), ()),
    "hashes": ({"repo": "req", "rev": "req", "paths": "list"}, (), ()),
    "cleantree": ({"repo": "req", "ignore-path": "req"}, (), ()),
    "read-text": ({"repo": "req", "rev": "req", "path": "req", "lines": "opt"}, (), ()),
    "read-json": ({"repo": "req", "rev": "opt", "path": "opt", "pointer": "multi"}, (), None),
    "trx": ({"repo": "req", "filter-substring": "opt"}, ("TRX",), ()),
    "handoff": ({"repo": "req", "schema-rev": "req", "allow-absent": "flag"}, ("H",), ()),
    "freetext": ({"repo": "req", "terms-rev": "req"}, ("H",), ()),
    "clausemap": ({"repo": "req", "main": "req", "state-rev": "opt", "state-path": "opt", "base": "opt"},
                  (), ("CONTRACT",)),
}
REPO_PATH_OPTIONS = ("path", "paths", "ignore-path", "state-path")


def usage(detail):
    return FactsError("USAGE", detail)


def bad_repo_path(p):
    if p == "" or "\\" in p or "\0" in p or p.startswith("/") or re.match(r"[A-Za-z]:", p):
        return True
    return ".." in p.split("/")


def parse_args(argv):
    if not argv:
        raise usage("missing command")
    cmd = argv[0]
    if cmd not in COMMAND_SPECS:
        raise usage("unknown command")
    opts, required_files, optional_files = COMMAND_SPECS[cmd]
    vals = {}
    files = {}
    shas = {}
    i = 1
    while i < len(argv):
        tok = argv[i]
        if not tok.startswith("--"):
            raise usage("unexpected argument")
        name = tok[2:]
        if name in ("file", "sha256"):
            if i + 1 >= len(argv):
                raise usage("--%s needs NAME=VALUE" % name)
            key, sep, value = argv[i + 1].partition("=")
            target = files if name == "file" else shas
            if not sep or key == "" or value == "" or key in target:
                raise usage("--%s needs a unique NAME=VALUE" % name)
            target[key] = value
            i += 2
            continue
        kind = opts.get(name)
        if kind is None:
            raise usage("unknown option --%s" % name)
        if kind == "flag":
            if name in vals:
                raise usage("repeated option --%s" % name)
            vals[name] = True
            i += 1
            continue
        if kind == "list":
            j = i + 1
            items = []
            while j < len(argv) and not argv[j].startswith("--"):
                items.append(argv[j])
                j += 1
            if not items or name in vals:
                raise usage("--%s needs one or more values" % name)
            vals[name] = items
            i = j
            continue
        if i + 1 >= len(argv):
            raise usage("--%s needs a value" % name)
        if kind == "multi":
            vals.setdefault(name, []).append(argv[i + 1])
        else:
            if name in vals:
                raise usage("repeated option --%s" % name)
            vals[name] = argv[i + 1]
        i += 2
    for name, kind in opts.items():
        if kind in ("req", "list") and name not in vals:
            raise usage("missing option --%s" % name)
    if set(files) != set(shas):
        raise usage("every --file NAME needs a matching --sha256 NAME and vice versa")
    for key, value in shas.items():
        if SHA256_RE.fullmatch(value) is None:
            raise usage("--sha256 %s is not 64 hex digits" % key)
    if optional_files is None:
        if not vals.get("pointer"):
            raise usage("read-json needs at least one --pointer")
        by_rev = "rev" in vals or "path" in vals
        if by_rev and (files or "rev" not in vals or "path" not in vals):
            raise usage("read-json takes either --rev and --path, or one --file")
        if not by_rev and len(files) != 1:
            raise usage("read-json takes either --rev and --path, or one --file")
    else:
        names = set(files)
        if not set(required_files) <= names or not names <= set(required_files) | set(optional_files):
            raise usage("--file names for %s must be %s" % (cmd, ", ".join(required_files + optional_files)))
    if cmd == "clausemap" and (("state-rev" in vals) != ("state-path" in vals)):
        raise usage("--state-rev and --state-path go together")
    if "lines" in vals:
        m = LINES_RE.fullmatch(vals["lines"])
        if m is None or int(m.group(1)) < 1 or int(m.group(1)) > int(m.group(2)):
            raise usage("--lines must be a-b with 1 <= a <= b")
    for name in REPO_PATH_OPTIONS:
        if name in vals:
            for p in (vals[name] if isinstance(vals[name], list) else [vals[name]]):
                if bad_repo_path(p):
                    raise usage("--%s must be a repository-relative path with '/'" % name)
    args = dict(vals)
    if files:
        args["file"] = files
        args["sha256"] = shas
    return cmd, args


def emit(envelope, code):
    sys.stdout.buffer.write(canon(envelope).encode("ascii") + b"\n")
    sys.stdout.buffer.flush()
    return code


def main(argv=None):
    if argv is None:
        argv = sys.argv[1:]
    isolated = sys.flags.isolated == 1
    envelope = {"Schema": ENVELOPE_SCHEMA, "Command": argv[0] if argv else None, "Args": {},
                "Inputs": [], "GitCalls": [], "Facts": None, "FactsSha256": None, "Error": None,
                "Runtime": {"Python": platform.python_version(), "Isolated": isolated}}
    if not isolated:
        envelope["Error"] = {"Code": "NOT_ISOLATED", "Detail": "run the tool with python -I"}
        return emit(envelope, EXIT_ERR)
    ctx = Ctx(None)
    try:
        cmd, args = parse_args(argv)
        envelope["Args"] = args
        ctx = Ctx(args["repo"])
        facts = COMMAND_FUNCS[cmd](ctx, args)
        facts_sha = hashlib.sha256(canon(facts).encode("ascii")).hexdigest()
    except FactsError as exc:
        envelope["Inputs"] = ctx.inputs
        envelope["GitCalls"] = ctx.git_calls
        envelope["Error"] = {"Code": exc.code, "Detail": exc.detail}
        return emit(envelope, EXIT_USAGE if exc.code == "USAGE" else EXIT_ERR)
    except Exception as exc:  # fail closed on any unforeseen condition
        envelope["Inputs"] = ctx.inputs
        envelope["GitCalls"] = ctx.git_calls
        envelope["Error"] = {"Code": "CONTRADICTORY",
                             "Detail": "unexpected internal condition: %s" % exc.__class__.__name__}
        return emit(envelope, EXIT_ERR)
    envelope["Inputs"] = ctx.inputs
    envelope["GitCalls"] = ctx.git_calls
    envelope["Facts"] = facts
    envelope["FactsSha256"] = facts_sha
    return emit(envelope, EXIT_OK)


if __name__ == "__main__":
    sys.exit(main())
