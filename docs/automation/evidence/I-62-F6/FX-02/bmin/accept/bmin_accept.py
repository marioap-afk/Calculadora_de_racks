#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""bmin_accept.py - B-min ACCEPTOR (I-62, decisions 75). EXPERIMENTAL, NOT NORMATIVE.

Reproduces R1-R10 of SPEC section 3 over the envelopes produced by the B-min facts
tool and recomputes the critical controls with an implementation that is independent
from the one the SPEC prescribes for the tool:

  * diff paths      : ls-tree -r -z of both trees (mode + object id), never git diff
  * ancestry/ranges : own parent walk over commits parsed from cat-file --batch
  * trailers        : own parser of the raw commit message trailer block
  * blob ids        : own SHA-1 of "blob <len>\\0<bytes>"; SHA-256 with hashlib
  * scope           : own coverage / containment / U-19 normalization (no globs)
  * Derive (16.14)  : own first-parent walk and exact-line trailer scan
  * MV-1..MV-7      : own JSON-schema subset, own Markdown section parser, own
                      sentence splitter with pointer-only removal

Follows SPEC v0.2 (errata section 5: E1 vocabulary exemptions for verbatim echoes, E2 strict
U19Normalized objects, E3 ':(literal)' pathspecs in R9, E4 ambiguous Derive / distinct claim_id
values must fail closed in the tool, E5 terms after the colon, E6 --no-renames, E7 RevPaths).

Output: one canonical ASCII JSON object (schema rackcad-bmin-accept/0-experimental)
with Checks R1-R10 and Decision ACCEPT_FACTS / REJECT_FACTS. Fail closed.
Zero writes, zero network, standard library only. Run as:

    python -I bmin_accept.py --request REQUEST.json [--request-sha256 HEX]

The acceptor never declares a verification result of the Controller.
"""

import hashlib
import json
import os
import platform
import re
import subprocess
import sys

# --------------------------------------------------------------------------- constants

OUT_SCHEMA = "rackcad-bmin-accept/0-experimental"
REQUEST_SCHEMA = "rackcad-bmin-accept-request/0-experimental"
CLOSURE_SCHEMA = "rackcad-bmin-closure/0-experimental"
FACTS_SCHEMA = "rackcad-bmin-facts/0-experimental"
CV_SCHEMA = "rackcad-controller-verification/v2"

AP_PATH = "docs/AUTOMATION_PLAN.md"
WF_PATH = "docs/WORKFLOW.md"
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
SCHEMA_PATH = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
ENTRY_HEADING = "## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)"
R163_HEADING = "### 16.13 Compatibilidad de protocolos de ejecución delegada"
POINTER = "Antes de aplicar esta sección, toda unidad aplica §16.13."
TRAILER = "Agent-Protocol-Normative: I-62"
S1610_HEADING = "### 16.10 Términos de gate"
S1610_END = "No se aplica"
SURFACES_LEAD = "**Superficies** (lista cerrada):"
PREAMBLE = "preámbulo"
S16_LABEL = "## 16."
S163_LABEL = "### 16.3"

FREE_TEXT_FIELDS = ("WorkCompleted", "Evidence", "UnexpectedFindings", "KnownLimitations",
                    "Deviations", "OpenQuestions", "RecommendedNextAction")

TOOL_GIT_SUBCOMMANDS = frozenset(["rev-parse", "cat-file", "ls-tree", "rev-list", "diff", "log",
                                  "show", "merge-base", "status", "check-ignore"])
ACCEPTOR_GIT_SUBCOMMANDS = TOOL_GIT_SUBCOMMANDS | frozenset(["for-each-ref"])
TOOL_GIT_CONFIG = frozenset(["core.quotepath=false", "core.fsmonitor=false"])
TOOL_GIT_GLOBAL_FLAGS = frozenset(["--no-pager", "--literal-pathspecs", "--no-optional-locks"])

ENVELOPE_KEYS = frozenset(["Schema", "Command", "Args", "Inputs", "GitCalls", "Facts",
                           "FactsSha256", "Error", "Runtime"])
KEY_FORBIDDEN_SUBSTRINGS = ("pass", "fail", "PASS", "FAIL", "VERIFIED", "EXECUTION_", "MAP_VALID",
                            "MAP_INVALID", "REWORK", "BLOCKED", "STOP")
VALUE_FORBIDDEN_EXACT = frozenset(["pass", "fail", "PASS", "FAIL", "VERIFIED", "MAP_VALID",
                                   "MAP_INVALID", "REWORK", "BLOCKED", "STOP"])

R4_CHECKS = ("Identity", "Scope", "CleanTree", "Trailer", "FreeText", "Authority")
CHECK_IDS = ("R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10")
WAIVABLE = frozenset(["R4", "R5", "R6", "R7", "R10"])
R5_COMMANDS = frozenset(["identity", "diff", "scope", "hashes"])
R7_COMMANDS = frozenset(["scope", "contract", "clausemap"])
COMPARED_COMMANDS = R5_COMMANDS | R7_COMMANDS

HEX = "0123456789abcdef"
DIGITS = "0123456789"
ASCII_ALNUM = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
ASCII_SPACE = " \t\r\n\f\v"


class AcceptError(Exception):
    """Fail-closed condition with a code and a short ASCII detail."""

    def __init__(self, code, detail=""):
        Exception.__init__(self, code + ": " + detail)
        self.code = code
        self.detail = detail


# --------------------------------------------------------------------------- basics

def canonical(obj):
    return json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))


def sha256_hex(data):
    return hashlib.sha256(data).hexdigest()


def git_object_sha1(kind, data):
    h = hashlib.sha1()
    h.update(kind.encode("ascii") + b" " + str(len(data)).encode("ascii") + b"\0")
    h.update(data)
    return h.hexdigest()


def git_blob_sha1(data):
    """Git blob id computed by the acceptor: SHA-1 of b'blob <len>\\0' + bytes."""
    return git_object_sha1("blob", data)


def is_hex(value, length):
    return isinstance(value, str) and len(value) == length and all(c in HEX for c in value)


def _no_dup_pairs(pairs):
    out = {}
    for k, v in pairs:
        if k in out:
            raise AcceptError("MALFORMED_JSON", "duplicate key")
        out[k] = v
    return out


def _no_constant(_name):
    raise AcceptError("MALFORMED_JSON", "non-finite number")


def parse_json_bytes(data, what):
    try:
        text = data.decode("utf-8")
    except UnicodeDecodeError:
        raise AcceptError("MALFORMED_JSON", what + ": not UTF-8")
    try:
        return json.loads(text, object_pairs_hook=_no_dup_pairs, parse_constant=_no_constant)
    except AcceptError:
        raise
    except (ValueError, RecursionError):
        raise AcceptError("MALFORMED_JSON", what)


def read_file(path):
    with open(path, "rb") as fh:
        return fh.read()


def read_input_file(path, what):
    if not isinstance(path, str) or not path:
        raise AcceptError("MISSING_INPUT", what)
    try:
        return read_file(path)
    except OSError:
        raise AcceptError("MISSING_INPUT", what + ": unreadable")


def read_pinned(spec, what):
    """Reads {Path, Sha256}; the SHA-256 is mandatory and must match."""
    if not isinstance(spec, dict):
        raise AcceptError("MISSING_INPUT", what)
    want = spec.get("Sha256")
    if not is_hex(want, 64):
        raise AcceptError("MISSING_INPUT", what + ".Sha256")
    data = read_input_file(spec.get("Path"), what + ".Path")
    if sha256_hex(data) != want:
        raise AcceptError("INPUT_HASH_MISMATCH", what)
    return data


def same_path(a, b):
    if not isinstance(a, str) or not isinstance(b, str) or not a or not b:
        return False
    return os.path.normcase(os.path.abspath(a)) == os.path.normcase(os.path.abspath(b))


def req_str(obj, key, what):
    v = obj.get(key) if isinstance(obj, dict) else None
    if not isinstance(v, str):
        raise AcceptError("MISSING_INPUT", what + "." + key)
    return v


def req_hex(obj, key, what):
    v = obj.get(key) if isinstance(obj, dict) else None
    if not is_hex(v, 40):
        raise AcceptError("MISSING_INPUT", what + "." + key)
    return v


def req_str_list(obj, key, what):
    v = obj.get(key) if isinstance(obj, dict) else None
    if not isinstance(v, list) or not all(isinstance(x, str) for x in v):
        raise AcceptError("MISSING_INPUT", what + "." + key)
    return list(v)


# --------------------------------------------------------------------------- git executor

def _clean_env():
    """Environment for child processes: inherited GIT_* removed, read-only git settings."""
    env = {}
    for k, v in os.environ.items():
        if k.upper().startswith("GIT_"):
            continue
        env[k] = v
    env["GIT_OPTIONAL_LOCKS"] = "0"
    env["GIT_TERMINAL_PROMPT"] = "0"
    env["GIT_NO_REPLACE_OBJECTS"] = "1"
    env["LC_ALL"] = "C"
    return env


def git_run(where, sub, args, stdin_bytes=b"", ok=(0,)):
    """Single git executor with a closed list of subcommands (SPEC 3)."""
    if sub not in ACCEPTOR_GIT_SUBCOMMANDS:
        raise AcceptError("FORBIDDEN_OPERATION", "git " + str(sub))
    args = [str(a) for a in args]
    for a in args:
        if a in ("-w", "--write") or a.startswith("--output"):
            raise AcceptError("FORBIDDEN_OPERATION", a)
    if sub == "status" and not any(a.startswith("--porcelain") for a in args):
        raise AcceptError("FORBIDDEN_OPERATION", "status without --porcelain")
    if sub in ("diff", "show") and not ("--no-ext-diff" in args and "--no-textconv" in args):
        raise AcceptError("FORBIDDEN_OPERATION", sub + " without --no-ext-diff --no-textconv")
    if not isinstance(where, str) or not where:
        raise AcceptError("MISSING_INPUT", "repository path")
    argv = ["git", "-C", where, "-c", "core.quotepath=false", "-c", "core.fsmonitor=false", sub] + args
    try:
        proc = subprocess.run(argv, input=stdin_bytes, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                              env=_clean_env(), shell=False, timeout=600)
    except (OSError, subprocess.SubprocessError):
        raise AcceptError("GIT_ERROR", "git " + sub + " could not run")
    if proc.returncode not in ok:
        raise AcceptError("GIT_ERROR", "git " + sub + " rc=" + str(proc.returncode))
    return proc.returncode, proc.stdout


def parse_commit(data):
    cut = data.find(b"\n\n")
    if cut < 0:
        header, message = data.rstrip(b"\n"), b""
    else:
        header, message = data[:cut], data[cut + 2:]
    parents = []
    tree = None
    for line in header.split(b"\n"):
        if line.startswith(b" "):
            continue
        if line.startswith(b"parent "):
            p = line[7:].decode("ascii", "replace")
            if not is_hex(p, 40):
                raise AcceptError("GIT_ERROR", "commit parent line")
            parents.append(p)
        elif line.startswith(b"tree "):
            tree = line[5:].decode("ascii", "replace")
    return {"parents": tuple(parents), "tree": tree,
            "message": message.decode("utf-8", "surrogateescape")}


class RepoView(object):
    """Read-only view of a repository built from raw objects."""

    def __init__(self, path):
        if not isinstance(path, str) or not path:
            raise AcceptError("MISSING_INPUT", "Repo")
        self.path = path
        self._commits = None
        self._trees = {}
        self._reach = {}
        self._children = None
        self._format_ok = False
        self._head = None
        self._refs = None

    def head_info(self):
        """Toplevel, HEAD and abbreviated HEAD name from one rev-parse call (cached)."""
        if self._head is None:
            _, out = git_run(self.path, "rev-parse", ["--show-toplevel", "HEAD", "--abbrev-ref", "HEAD"])
            lines = out.decode("utf-8", "surrogateescape").split("\n")
            if len(lines) != 4 or lines[3] != "" or not is_hex(lines[1], 40):
                raise AcceptError("GIT_ERROR", "rev-parse HEAD")
            self._head = {"Toplevel": lines[0], "Head": lines[1], "AbbrevRef": lines[2]}
        return self._head

    def ref_targets(self):
        """refname -> object id for every ref (one for-each-ref call, cached)."""
        if self._refs is None:
            _, out = git_run(self.path, "for-each-ref", ["--format=%(refname)%00%(objectname)"])
            refs = {}
            for line in out.split(b"\n"):
                if not line:
                    continue
                name, sep, oid = line.partition(b"\0")
                if not sep:
                    raise AcceptError("GIT_ERROR", "for-each-ref record")
                refs[name.decode("utf-8", "surrogateescape")] = oid.decode("ascii", "replace")
            self._refs = refs
        return self._refs

    def _check_format(self):
        if not self._format_ok:
            _, out = git_run(self.path, "rev-parse", ["--show-object-format"])
            if out.strip() != b"sha1":
                raise AcceptError("UNSUPPORTED", "object format is not sha1")
            self._format_ok = True

    def batch(self, oids):
        self._check_format()
        uniq = []
        seen = set()
        for o in oids:
            if not is_hex(o, 40):
                raise AcceptError("BAD_REVISION", "object id")
            if o not in seen:
                seen.add(o)
                uniq.append(o)
        if not uniq:
            return {}
        payload = ("\n".join(uniq) + "\n").encode("ascii")
        _, out = git_run(self.path, "cat-file", ["--batch"], stdin_bytes=payload)
        res = {}
        pos = 0
        for o in uniq:
            nl = out.find(b"\n", pos)
            if nl < 0:
                raise AcceptError("GIT_ERROR", "cat-file --batch truncated")
            head = out[pos:nl].decode("ascii", "replace").split(" ")
            if len(head) == 2 and head[1] == "missing":
                raise AcceptError("BAD_REVISION", "missing object")
            if len(head) != 3 or head[0] != o or not head[2].isdigit():
                raise AcceptError("GIT_ERROR", "cat-file --batch header")
            start = nl + 1
            end = start + int(head[2])
            if end > len(out):
                raise AcceptError("GIT_ERROR", "cat-file --batch truncated")
            res[o] = (head[1], out[start:end])
            pos = end + 1
        return res

    def commits(self):
        if self._commits is None:
            self._check_format()
            _, out = git_run(self.path, "cat-file",
                             ["--batch-all-objects", "--batch-check=%(objectname) %(objecttype)"])
            ids = []
            for line in out.decode("ascii", "replace").split("\n"):
                parts = line.split(" ")
                if len(parts) == 2 and parts[1] == "commit":
                    ids.append(parts[0])
            table = {}
            for oid, (kind, data) in self.batch(ids).items():
                if kind != "commit" or git_object_sha1("commit", data) != oid:
                    raise AcceptError("GIT_ERROR", "commit object integrity")
                table[oid] = parse_commit(data)
            self._commits = table
        return self._commits

    def require_commit(self, sha):
        if not is_hex(sha, 40) or sha not in self.commits():
            raise AcceptError("BAD_REVISION", "not an existing commit")
        return sha

    def parents(self, c):
        self.require_commit(c)
        return self._commits[c]["parents"]

    def message(self, c):
        self.require_commit(c)
        return self._commits[c]["message"]

    def reachable(self, start):
        self.require_commit(start)
        if start not in self._reach:
            table = self._commits
            seen = set()
            stack = [start]
            while stack:
                c = stack.pop()
                if c in seen:
                    continue
                info = table.get(c)
                if info is None:
                    raise AcceptError("GIT_ERROR", "missing ancestor (shallow or incomplete repository)")
                seen.add(c)
                stack.extend(info["parents"])
            self._reach[start] = frozenset(seen)
        return self._reach[start]

    def is_ancestor(self, a, b):
        """a is reachable from b (a commit is its own ancestor)."""
        self.require_commit(a)
        return a in self.reachable(b)

    def range_commits(self, a, b):
        return set(self.reachable(b)) - set(self.reachable(a))

    def first_parent_chain(self, s):
        out = []
        c = s
        while c is not None:
            out.append(c)
            p = self.parents(c)
            c = p[0] if p else None
        return out

    def reaching(self, target):
        """Commits from which target is reachable (target included)."""
        self.require_commit(target)
        if self._children is None:
            ch = {}
            for c, info in self._commits.items():
                for p in info["parents"]:
                    ch.setdefault(p, []).append(c)
            self._children = ch
        seen = set()
        stack = [target]
        while stack:
            c = stack.pop()
            if c in seen:
                continue
            seen.add(c)
            stack.extend(self._children.get(c, ()))
        return seen

    def tree(self, commit):
        self.require_commit(commit)
        if commit not in self._trees:
            _, out = git_run(self.path, "ls-tree", ["-r", "-z", "--full-tree", commit])
            ent = {}
            for rec in out.split(b"\0"):
                if not rec:
                    continue
                meta, tab, p = rec.partition(b"\t")
                parts = meta.split(b" ")
                if not tab or len(parts) != 3:
                    raise AcceptError("GIT_ERROR", "ls-tree record")
                ent[p.decode("utf-8", "surrogateescape")] = (
                    parts[0].decode("ascii"), parts[1].decode("ascii"), parts[2].decode("ascii"))
            self._trees[commit] = ent
        return self._trees[commit]

    def oid_at(self, commit, path):
        e = self.tree(commit).get(path)
        return e[2] if e is not None else None

    def blob_at(self, commit, path):
        e = self.tree(commit).get(path)
        return e[2] if e is not None and e[1] == "blob" else None

    def read_blobs(self, oids):
        out = {}
        for oid, (kind, data) in self.batch([o for o in oids if o is not None]).items():
            if kind != "blob" or git_blob_sha1(data) != oid:
                raise AcceptError("GIT_ERROR", "blob object integrity")
            out[oid] = data
        return out

    def text_at(self, commit, path):
        oid = self.blob_at(commit, path)
        if oid is None:
            return None
        return self.read_blobs([oid])[oid].decode("utf-8", "surrogateescape")


def ref_target(repo, refname):
    """Object id of exactly refname (for-each-ref listing, exact name), or None."""
    return repo.ref_targets().get(refname)


# --------------------------------------------------------------------------- text / markdown

def to_lf(text):
    return text.replace("\r\n", "\n")


def norm_ws(text):
    """Whitespace runs collapsed to one space, ends trimmed (AP L873, L896)."""
    return " ".join(text.split())


def heading_level(normalized_line):
    n = 0
    while n < len(normalized_line) and normalized_line[n] == "#":
        n += 1
    if 1 <= n <= 6 and (n == len(normalized_line) or normalized_line[n] == " "):
        return n
    return 0


class MdDoc(object):
    """Markdown reader: code fences (lines starting with ```), headings, sections."""

    def __init__(self, text):
        self.lines = to_lf(text).split("\n")
        self.in_code = []
        self.heads = []
        inside = False
        for idx, line in enumerate(self.lines):
            if line.startswith("```"):
                self.in_code.append(True)
                inside = not inside
                continue
            self.in_code.append(inside)
            if inside:
                continue
            nl = norm_ws(line)
            lv = heading_level(nl)
            if lv:
                self.heads.append((idx, lv, nl))

    def match_lines(self, wanted):
        w = norm_ws(wanted)
        return [i + 1 for i, line in enumerate(self.lines)
                if not self.in_code[i] and norm_ws(line) == w]

    def head_index_at(self, line_idx):
        for k, h in enumerate(self.heads):
            if h[0] == line_idx:
                return k
        return None

    def section_bounds(self, k):
        start, level, _ = self.heads[k]
        end = len(self.lines)
        for j in range(k + 1, len(self.heads)):
            if self.heads[j][1] <= level:
                end = self.heads[j][0]
                break
        return start, end

    def section_text(self, k):
        s, e = self.section_bounds(k)
        return "\n".join(self.lines[s:e])

    def body_text(self, k):
        s, e = self.section_bounds(k)
        return "\n".join(self.lines[s + 1:e])

    def preamble_text(self):
        for idx, level, _ in self.heads:
            if level == 2:
                return "\n".join(self.lines[:idx])
        return "\n".join(self.lines)

    def find_unique_heading(self, level, label):
        found = [k for k, (_i, lv, nl) in enumerate(self.heads)
                 if lv == level and (nl == label or nl.startswith(label + " "))]
        return found[0] if len(found) == 1 else None

    def section_map(self):
        """normalized heading -> normalized section text, plus the preamble; duplicates."""
        texts = {PREAMBLE: norm_ws(self.preamble_text())}
        counts = {}
        for k, (_i, _lv, nl) in enumerate(self.heads):
            counts[nl] = counts.get(nl, 0) + 1
            if counts[nl] == 1:
                texts[nl] = norm_ws(self.section_text(k))
        return texts, sorted(h for h, c in counts.items() if c > 1)

    def section_contains_literal(self, line_number, needle):
        k = self.head_index_at(line_number - 1)
        if k is None:
            return False
        return needle in self.section_text(k)


def split_sentences(text):
    """SPEC 1.10: boundary = '.' followed by whitespace or end of text; keeps the period."""
    out = []
    start = 0
    n = len(text)
    for i in range(n):
        if text[i] == "." and (i + 1 == n or text[i + 1].isspace()):
            piece = text[start:i + 1].strip()
            if piece:
                out.append(piece)
            start = i + 1
    tail = text[start:].strip()
    if tail:
        out.append(tail)
    return out


def remove_pointer_sentence(normalized_text, pointer):
    """Removes only the pointer sentence (its last occurrence as a whole sentence), once."""
    sentences = split_sentences(normalized_text)
    for k in range(len(sentences) - 1, -1, -1):
        if sentences[k] == pointer:
            del sentences[k]
            break
    return " ".join(sentences)


def backticked(text):
    out = []
    i = 0
    while True:
        a = text.find("`", i)
        if a < 0:
            return out
        b = text.find("`", a + 1)
        if b < 0:
            return out
        out.append(text[a + 1:b])
        i = b + 1


def closed_surfaces(ap_text):
    doc = MdDoc(ap_text)
    starts = [i for i, line in enumerate(doc.lines)
              if not doc.in_code[i] and norm_ws(line).startswith(SURFACES_LEAD)]
    if len(starts) != 1:
        return None
    buf = []
    for line in doc.lines[starts[0]:]:
        if line.strip() == "":
            break
        buf.append(line)
    return backticked("\n".join(buf))


def gate_terms(ap_text):
    """Terms of AP 16.10: the backticked list after the field list ('...`: `GATE PASS`, ...')."""
    doc = MdDoc(ap_text)
    found = [i for (i, _lv, nl) in doc.heads if nl == norm_ws(S1610_HEADING)]
    if len(found) != 1:
        raise AcceptError("CONTRADICTORY", "16.10 heading count is not 1")
    buf = []
    ended = False
    for j in range(found[0] + 1, len(doc.lines)):
        line = doc.lines[j]
        if not doc.in_code[j] and heading_level(norm_ws(line)):
            break
        pos = line.find(S1610_END)
        if pos >= 0:
            buf.append(line[:pos])
            ended = True
            break
        buf.append(line)
    if not ended:
        raise AcceptError("CONTRADICTORY", "16.10 closing sentence not found")
    text = "\n".join(buf)
    cut = text.find("`:")
    region = text[cut + 1:] if cut >= 0 else text
    terms = backticked(region)
    if not terms:
        raise AcceptError("CONTRADICTORY", "16.10 yields 0 terms")
    return terms


def _word_char(ch):
    return ch.isalnum() or ch == "_"


def count_whole(haystack, needle):
    if not needle:
        return 0
    count = 0
    i = 0
    while True:
        j = haystack.find(needle, i)
        if j < 0:
            return count
        k = j + len(needle)
        if (j == 0 or not _word_char(haystack[j - 1])) and (k == len(haystack) or not _word_char(haystack[k])):
            count += 1
        i = j + 1


def freetext_matches(handoff, terms):
    found = {}
    for field in FREE_TEXT_FIELDS:
        if not isinstance(handoff, dict) or field not in handoff:
            raise AcceptError("MISSING_INPUT", "handoff." + field)
        value = handoff[field]
        if isinstance(value, str):
            items = [value]
        elif isinstance(value, list) and all(isinstance(x, str) for x in value):
            items = value
        else:
            raise AcceptError("MISSING_INPUT", "handoff." + field + " type")
        for term in terms:
            t = norm_ws(term).casefold()
            c = sum(count_whole(norm_ws(x).casefold(), t) for x in items)
            if c:
                found[(field, term)] = c
    return [{"Field": f, "Term": t, "Count": c} for (f, t), c in sorted(found.items())]


# --------------------------------------------------------------------------- scope rules

def u19(entry):
    """U-19: an entry ending in '/**' is read as the prefix ending in '/'."""
    if entry.endswith("/**"):
        return entry[:-2]
    return entry


def is_malformed(entry):
    if entry.startswith("/") or "\\" in entry:
        return True
    if len(entry) >= 2 and entry[1] == ":" and entry[0] in ASCII_ALNUM and not entry[0] in DIGITS:
        return True
    if ".." in entry.split("/"):
        return True
    core = entry[:-3] if entry.endswith("/**") else entry
    for ch in "*?[":
        if ch in core:
            return True
    return False


def covers(entry, path):
    """AP L553: R is covered by E if R == E, or E ends in '/' and R starts with E (E after U-19)."""
    e = u19(entry)
    if path == e:
        return True
    return e.endswith("/") and path.startswith(e)


def contained(a, b):
    """Entry a is contained by entry b (AP L553-L554), both after U-19."""
    return covers(b, u19(a))


# --------------------------------------------------------------------------- trailers

_GIT_GENERATED_PREFIXES = ("Signed-off-by: ", "(cherry picked from commit ")


def _is_blank(line):
    return all(c in ASCII_SPACE for c in line)


def _separator_pos(line):
    whitespace = False
    for k, c in enumerate(line):
        if c == ":":
            return k
        if not whitespace and (c in ASCII_ALNUM or c == "-"):
            continue
        if k != 0 and c in " \t":
            whitespace = True
            continue
        break
    return -1


def trailer_block(message):
    """Lines of the final-paragraph trailer block of a raw commit message ([] if none)."""
    lines = message.split("\n")
    title_end = len(lines)
    for i, line in enumerate(lines):
        if line.startswith("#"):
            continue
        if _is_blank(line):
            title_end = i
            break
    only_spaces = True
    recognized = False
    trailer_lines = 0
    non_trailer = 0
    possible_cont = 0
    block_start = None
    for i in range(len(lines) - 1, title_end - 1, -1):
        line = lines[i]
        if line.startswith("#"):
            continue
        if _is_blank(line):
            if only_spaces:
                continue
            non_trailer += possible_cont
            if recognized and trailer_lines * 3 >= non_trailer:
                block_start = i + 1
            elif trailer_lines and not non_trailer:
                block_start = i + 1
            break
        only_spaces = False
        if any(line.startswith(p) for p in _GIT_GENERATED_PREFIXES):
            trailer_lines += 1
            possible_cont = 0
            recognized = True
            continue
        if _separator_pos(line) >= 1 and line[0] not in ASCII_SPACE:
            trailer_lines += 1
            possible_cont = 0
        elif line[0] in ASCII_SPACE:
            possible_cont += 1
        else:
            non_trailer += 1 + possible_cont
            possible_cont = 0
    if block_start is None:
        return []
    end = len(lines)
    while end > block_start and _is_blank(lines[end - 1]):
        end -= 1
    return lines[block_start:end]


def coauthored_values(message):
    """Values of Co-Authored-By (key compared ASCII case-insensitively), in order."""
    items = []
    last = None
    for line in trailer_block(message):
        if last is not None and line[:1] in (" ", "\t"):
            items[last][1] += "\n" + line
            continue
        sep = _separator_pos(line)
        if sep >= 1:
            items.append([line[:sep].strip(), line[sep + 1:]])
            last = len(items) - 1
        else:
            items.append([None, line])
            last = None
    out = []
    for token, value in items:
        if token is not None and token.lower() == "co-authored-by":
            for piece in value.strip().split("\n"):
                if piece.strip():
                    out.append(piece.strip())
    return out


def strip_trailer_key(declared):
    v = declared.strip()
    if v.lower().startswith("co-authored-by:"):
        v = v[len("co-authored-by:"):].strip()
    return v


# --------------------------------------------------------------------------- JSON schema subset

_ANNOTATIONS = frozenset(["$schema", "title", "description", "$id", "$comment", "default", "examples"])
_SUPPORTED_KEYWORDS = frozenset(["type", "enum", "const", "required", "properties", "additionalProperties",
                                 "items", "minItems", "maxItems", "uniqueItems", "minLength", "pattern",
                                 "minimum", "maximum"])


def _ptr_escape(token):
    return token.replace("~", "~0").replace("/", "~1")


def schema_unsupported(schema, where="#"):
    if isinstance(schema, bool):
        return []
    if not isinstance(schema, dict):
        return [where]
    out = []
    for k, v in schema.items():
        if k in _ANNOTATIONS:
            continue
        if k not in _SUPPORTED_KEYWORDS:
            out.append(where + "/" + _ptr_escape(k))
            continue
        if k == "properties":
            if not isinstance(v, dict):
                out.append(where + "/properties")
            else:
                for pk, ps in v.items():
                    out.extend(schema_unsupported(ps, where + "/properties/" + _ptr_escape(pk)))
        elif k in ("additionalProperties", "items"):
            if isinstance(v, (dict, bool)):
                out.extend(schema_unsupported(v, where + "/" + k))
            else:
                out.append(where + "/" + k)
    return out


def _type_ok(t, v):
    if t == "null":
        return v is None
    if t == "boolean":
        return isinstance(v, bool)
    if t == "object":
        return isinstance(v, dict)
    if t == "array":
        return isinstance(v, list)
    if t == "string":
        return isinstance(v, str)
    if t == "integer":
        return (isinstance(v, int) and not isinstance(v, bool)) or (isinstance(v, float) and v.is_integer())
    if t == "number":
        return isinstance(v, (int, float)) and not isinstance(v, bool)
    return False


def _pattern_search(pattern, value):
    pat = pattern
    if pat.endswith("$") and not pat.endswith("\\$"):
        pat = pat[:-1] + "\\Z"
    try:
        return re.search(pat, value) is not None
    except re.error:
        raise AcceptError("UNSUPPORTED_SCHEMA_KEYWORD", "pattern")


def schema_validate(schema, inst, ptr, out):
    if schema is True:
        return
    if schema is False:
        out.append((ptr, "false"))
        return
    t = schema.get("type")
    if t is not None:
        ts = t if isinstance(t, list) else [t]
        if not any(_type_ok(x, inst) for x in ts):
            out.append((ptr, "type"))
    if "enum" in schema and not any(canonical(inst) == canonical(e) for e in schema["enum"]):
        out.append((ptr, "enum"))
    if "const" in schema and canonical(inst) != canonical(schema["const"]):
        out.append((ptr, "const"))
    if isinstance(inst, dict):
        for r in schema.get("required", []):
            if r not in inst:
                out.append((ptr, "required"))
        props = schema.get("properties", {})
        for k, v in inst.items():
            sub = ptr + "/" + _ptr_escape(k)
            if k in props:
                schema_validate(props[k], v, sub, out)
            elif "additionalProperties" in schema:
                ap = schema["additionalProperties"]
                if ap is False:
                    out.append((sub, "additionalProperties"))
                elif isinstance(ap, dict):
                    schema_validate(ap, v, sub, out)
    if isinstance(inst, list):
        if "items" in schema:
            for i, v in enumerate(inst):
                schema_validate(schema["items"], v, ptr + "/" + str(i), out)
        if "minItems" in schema and len(inst) < schema["minItems"]:
            out.append((ptr, "minItems"))
        if "maxItems" in schema and len(inst) > schema["maxItems"]:
            out.append((ptr, "maxItems"))
        if schema.get("uniqueItems") is True:
            seen = [canonical(x) for x in inst]
            if len(set(seen)) != len(seen):
                out.append((ptr, "uniqueItems"))
    if isinstance(inst, str):
        if "minLength" in schema and len(inst) < schema["minLength"]:
            out.append((ptr, "minLength"))
        if "pattern" in schema and not _pattern_search(schema["pattern"], inst):
            out.append((ptr, "pattern"))
    if isinstance(inst, (int, float)) and not isinstance(inst, bool):
        if "minimum" in schema and inst < schema["minimum"]:
            out.append((ptr, "minimum"))
        if "maximum" in schema and inst > schema["maximum"]:
            out.append((ptr, "maximum"))


# --------------------------------------------------------------------------- independent facts

def tree_diff_paths(repo, a, b):
    ta = repo.tree(a)
    tb = repo.tree(b)
    return sorted(p for p in set(ta) | set(tb) if ta.get(p) != tb.get(p))


def acceptor_identity(repo, branch, base, current, red):
    if not isinstance(branch, str) or not branch:
        raise AcceptError("USAGE", "branch")
    repo.require_commit(base)
    repo.require_commit(current)
    if red is not None:
        repo.require_commit(red)
    hi = repo.head_info()
    anc = {"base->current": repo.is_ancestor(base, current)}
    if red is not None:
        anc["base->red"] = repo.is_ancestor(base, red)
        anc["red->current"] = repo.is_ancestor(red, current)
    return {"Head": hi["Head"], "AbbrevRef": hi["AbbrevRef"], "Toplevel": hi["Toplevel"],
            "OriginBranch": ref_target(repo, "refs/remotes/origin/" + branch),
            "OriginMain": ref_target(repo, "refs/remotes/origin/main"), "Ancestry": anc}


def acceptor_diff(repo, a, b):
    repo.require_commit(a)
    repo.require_commit(b)
    commits = sorted(repo.range_commits(a, b))
    return {"Paths": tree_diff_paths(repo, a, b), "Commits": commits,
            "CoAuthoredBy": dict((c, coauthored_values(repo.message(c))) for c in commits)}


def acceptor_scope(repo, a, b, deleg):
    allowed = req_str_list(deleg, "AllowedWriteScope", "delegation")
    forbidden = req_str_list(deleg, "ForbiddenWriteScope", "delegation")
    repo.require_commit(a)
    repo.require_commit(b)
    paths = tree_diff_paths(repo, a, b)
    per = [{"Path": p, "AllowedCoveredBy": [e for e in allowed if covers(e, p)],
            "ForbiddenCoveredBy": [e for e in forbidden if covers(e, p)]} for p in paths]
    both = allowed + forbidden
    # Errata E2: {"Entry", "List", "Prefix"}, delegation order, Allowed first then Forbidden.
    u19n = [{"Entry": e, "List": name, "Prefix": u19(e)}
            for name, entries in (("AllowedWriteScope", allowed), ("ForbiddenWriteScope", forbidden))
            for e in entries if e.endswith("/**")]
    return {"Paths": paths, "PerPath": per, "AllowedEntries": allowed, "ForbiddenEntries": forbidden,
            "MalformedEntries": [e for e in both if is_malformed(e)], "U19Normalized": u19n}


def acceptor_hashes(repo, rev, paths):
    repo.require_commit(rev)
    tree = repo.tree(rev)
    rev_oids = dict((p, tree[p][2]) for p in paths if p in tree and tree[p][1] == "blob")
    contents = repo.read_blobs(list(rev_oids.values()))
    per = []
    for p in paths:
        fs = os.path.join(repo.path, *p.split("/"))
        wt1 = wt256 = None
        if os.path.isfile(fs):
            data = read_file(fs)
            wt1 = git_blob_sha1(data)
            wt256 = sha256_hex(data)
        rb = rev_oids.get(p)
        per.append({"Path": p, "WorktreeRawBlobSha1": wt1, "WorktreeSha256": wt256, "RevBlob": rb,
                    "RevBlobSha256": sha256_hex(contents[rb]) if rb else None})
    return {"PerPath": per}


def acceptor_contract(contract, deleg):
    ca = req_str_list(contract, "AllowedWriteScope", "contract")
    cf = req_str_list(contract, "ForbiddenWriteScope", "contract")
    da = req_str_list(deleg, "AllowedWriteScope", "delegation")
    df = req_str_list(deleg, "ForbiddenWriteScope", "delegation")
    return {"A3": [{"Entry": a, "ContainedBy": [b for b in ca if contained(a, b)]} for a in da],
            "A4": [{"Entry": a, "ContainedBy": [b for b in df if contained(a, b)]} for a in cf]}


def acceptor_derive(repo, s):
    repo.require_commit(s)
    trailer = sorted(c for c in repo.reachable(s) if TRAILER in repo.message(c).split("\n"))
    merges = [c for c in reversed(repo.first_parent_chain(s)) if len(repo.parents(c)) >= 2]
    cands = []
    if len(trailer) == 1:
        reach_t = repo.reaching(trailer[0])
        for m in merges:
            p = repo.parents(m)
            if p[1] in reach_t and p[0] not in reach_t:
                cands.append(m)
    eff = cands[0] if cands else None
    ep = repo.parents(eff) if eff else (None, None)
    return {"TrailerCommits": trailer, "FirstParentMerges": merges, "Candidates": cands, "Eff": eff,
            "EffParent1": ep[0], "EffParent2": ep[1]}


def acceptor_entry(repo, rev):
    wf = repo.blob_at(rev, WF_PATH)
    ap = repo.blob_at(rev, AP_PATH)
    data = repo.read_blobs([wf, ap])
    em, rm, names = [], [], None
    if wf is not None:
        doc = MdDoc(data[wf].decode("utf-8", "surrogateescape"))
        em = doc.match_lines(ENTRY_HEADING)
        if len(em) == 1:
            names = doc.section_contains_literal(em[0], R163_HEADING)
    if ap is not None:
        rm = MdDoc(data[ap].decode("utf-8", "surrogateescape")).match_lines(R163_HEADING)
    return {"WorkflowBlob": wf, "ApBlob": ap, "EntryMatches": em, "R163Matches": rm, "EntryNamesR163": names}


def _mv6_file(path, text1, text2, entries):
    rec = {"Path": path}
    keys = ("H1Duplicates", "H2Duplicates", "DerivedModified", "DerivedRemoved", "DerivedAddedOrEntry")
    mine = [e for e in entries if e["Path"] == path]
    rec["MapModified"] = sorted(set(norm_ws(e["Section"]) for e in mine if e.get("Kind") == "MODIFIED"))
    rec["MapRemoved"] = sorted(set(norm_ws(e["Section"]) for e in mine if e.get("Kind") == "REMOVED"))
    rec["MapAddedOrEntry"] = sorted(set(norm_ws(e["Section"]) for e in mine if e.get("Kind") in ("ADDED", "ENTRY")))
    if text1 is None or text2 is None:
        for k in keys:
            rec[k] = None
        return rec
    h1, d1 = MdDoc(text1).section_map()
    h2, d2 = MdDoc(text2).section_map()
    rec["H1Duplicates"] = d1
    rec["H2Duplicates"] = d2
    rec["DerivedModified"] = sorted(h for h in set(h1) & set(h2) if h1[h] != h2[h])
    rec["DerivedRemoved"] = sorted(set(h1) - set(h2))
    rec["DerivedAddedOrEntry"] = sorted(set(h2) - set(h1))
    return rec


def _map_shape_ok(m):
    if not isinstance(m, dict):
        return False
    s, f, e = m.get("Surfaces"), m.get("Files"), m.get("Entries")
    return (isinstance(s, list) and all(isinstance(x, str) for x in s)
            and isinstance(f, list) and all(isinstance(x, dict) and isinstance(x.get("Path"), str) for x in f)
            and isinstance(e, list) and all(isinstance(x, dict) and isinstance(x.get("Path"), str)
                                            and isinstance(x.get("Section"), str) for x in e))


def acceptor_validate(repo, eff):
    eff1 = repo.parents(eff)[0]
    t1 = repo.tree(eff1)
    t2 = repo.tree(eff)

    def oid(tree, p):
        e = tree.get(p)
        return e[2] if e is not None else None

    def blob(tree, p):
        e = tree.get(p)
        return e[2] if e is not None and e[1] == "blob" else None

    map_oid, schema_oid = blob(t2, MAP_PATH), blob(t2, SCHEMA_PATH)
    ap2_oid, ap1_oid, wf2_oid = blob(t2, AP_PATH), blob(t1, AP_PATH), blob(t2, WF_PATH)
    data = repo.read_blobs([map_oid, schema_oid, ap2_oid, ap1_oid, wf2_oid])

    def text(o):
        return data[o].decode("utf-8", "surrogateescape") if o else None

    internal = {"SchemaParses": False, "Unsupported": None, "ShapeOk": False, "R163CountEff": 0}
    map_obj, map_parses = None, False
    if map_oid:
        try:
            map_obj = parse_json_bytes(data[map_oid], "map")
            map_parses = True
        except AcceptError:
            pass
    schema_obj = None
    if schema_oid:
        try:
            schema_obj = parse_json_bytes(data[schema_oid], "schema")
            internal["SchemaParses"] = True
        except AcceptError:
            pass
    violations = None
    if map_parses and internal["SchemaParses"]:
        unsupported = schema_unsupported(schema_obj)
        internal["Unsupported"] = unsupported
        if not unsupported:
            found = []
            try:
                schema_validate(schema_obj, map_obj, "", found)
                violations = [{"Pointer": p, "Keyword": k} for p, k in sorted(set(found))]
            except AcceptError:
                internal["Unsupported"] = ["#/pattern"]
    mv1 = {"MapPresent": map_oid is not None, "MapBlob": map_oid, "MapParses": map_parses,
           "SchemaPresent": schema_oid is not None, "SchemaBlob": schema_oid, "SchemaViolations": violations}
    ap2, ap1, wf2 = text(ap2_oid), text(ap1_oid), text(wf2_oid)
    mv2 = {"MapSurfaces": map_obj.get("Surfaces") if map_parses and isinstance(map_obj, dict) else None,
           "ClosedList": closed_surfaces(ap2) if ap2 is not None else None}
    mv3 = mv4 = mv5 = mv6 = None
    if map_parses and _map_shape_ok(map_obj):
        internal["ShapeOk"] = True
        surf = map_obj["Surfaces"]
        files = map_obj["Files"]
        entries = map_obj["Entries"]

        def under(p):
            return any(p == s or (s.endswith("/") and p.startswith(s)) for s in surf)

        changed = sorted(p for p in set(t1) | set(t2)
                         if p != MAP_PATH and under(p) and oid(t1, p) != oid(t2, p))
        fpaths = [f["Path"] for f in files]
        mv3 = {"ChangedUnderSurfaces": changed,
               "DeletedUnderSurfaces": [p for p in changed if p in t1 and p not in t2],
               "FilesPaths": fpaths,
               "FilesDuplicates": sorted(set(p for p in fpaths if fpaths.count(p) > 1)),
               "OnlyInChanged": sorted(set(changed) - set(fpaths)),
               "OnlyInFiles": sorted(set(fpaths) - set(changed))}
        rows, lit, outside, added1, entry1 = [], [], [], [], []
        for f in files:
            p, kind, base, effb = f["Path"], f.get("FileKind"), f.get("BaseBlob"), f.get("EffBlob")
            o1, o2 = oid(t1, p), oid(t2, p)
            rows.append({"Path": p, "FileKind": kind, "MapBaseBlob": base, "MapEffBlob": effb,
                         "ObservedEff1Blob": o1, "ObservedEffBlob": o2})
            if kind == "MODIFIED":
                if o1 is None or base != o1 or o2 is None or effb != o2:
                    lit.append(p)
            elif kind in ("ADDED", "ENTRY"):
                if base is not None or o2 is None or effb != o2:
                    lit.append(p)
            if kind == "ENTRY" and p != SCHEMA_PATH:
                outside.append(p)
            if kind == "ADDED" and o1 is not None:
                added1.append(p)
            if kind == "ENTRY" and o1 is not None:
                entry1.append(p)
        mv4 = {"Rows": rows, "LiteralViolations": lit, "EntryFilesOutsideSchema": outside,
               "AddedPresentInEff1": added1, "EntryFilePresentInEff1": entry1}
        keys = [(e["Path"], norm_ws(e["Section"])) for e in entries]
        modified = set(f["Path"] for f in files if f.get("FileKind") == "MODIFIED")
        mv5 = {"EntryKeyDuplicates": [{"Path": p, "Section": s}
                                      for p, s in sorted(set(k for k in keys if keys.count(k) > 1))],
               "EntriesOnNonModifiedPath": [{"Path": e["Path"], "Section": e["Section"]}
                                            for e in entries if e["Path"] not in modified]}
        mod_paths = []
        for f in files:
            if f.get("FileKind") == "MODIFIED" and f["Path"] not in mod_paths:
                mod_paths.append(f["Path"])
        pair_oids = dict((p, (blob(t1, p), blob(t2, p))) for p in mod_paths)
        mdata = repo.read_blobs([o for pr in pair_oids.values() for o in pr])

        def mtext(o):
            return mdata[o].decode("utf-8", "surrogateescape") if o else None

        eke = sorted(set((e["Path"], norm_ws(e["Section"])) for e in entries if e.get("Kind") == "ENTRY"))
        exp = sorted([(AP_PATH, norm_ws(R163_HEADING)), (WF_PATH, norm_ws(ENTRY_HEADING))])
        mv6 = {"PerFile": [_mv6_file(p, mtext(pair_oids[p][0]), mtext(pair_oids[p][1]), entries)
                           for p in mod_paths],
               "EntryKindEntries": [{"Path": p, "Section": s} for p, s in eke],
               "ExpectedEntryKind": [{"Path": p, "Section": s} for p, s in exp]}
    # MV-7
    entry_count, entry_names = 0, None
    if wf2 is not None:
        wdoc = MdDoc(wf2)
        em = wdoc.match_lines(ENTRY_HEADING)
        entry_count = len(em)
        if entry_count == 1:
            entry_names = wdoc.section_contains_literal(em[0], R163_HEADING)
    first = last = minus = s1 = None
    if ap2 is not None:
        d2 = MdDoc(ap2)
        internal["R163CountEff"] = len(d2.match_lines(R163_HEADING))
        k16 = d2.find_unique_heading(2, S16_LABEL)
        if k16 is not None:
            sents = split_sentences(norm_ws(d2.body_text(k16)))
            first = sents[0] if sents else None
        k3 = d2.find_unique_heading(3, S163_LABEL)
        if k3 is not None:
            body2 = norm_ws(d2.body_text(k3))
            sents = split_sentences(body2)
            last = sents[-1] if sents else None
            minus = remove_pointer_sentence(body2, POINTER)
    if ap1 is not None:
        d1 = MdDoc(ap1)
        k3 = d1.find_unique_heading(3, S163_LABEL)
        if k3 is not None:
            s1 = norm_ws(d1.body_text(k3))
    mv7 = {"EntryCountEff": entry_count, "EntryNamesR163Eff": entry_names,
           "FirstSentenceS16": first, "PointerIsFirstSentenceS16": first == POINTER,
           "LastSentenceS163": last, "PointerIsLastSentenceS163": last == POINTER,
           "S163EffMinusPointer": minus, "S163Eff1": s1,
           "S163EqualAfterPointerRemoval": minus is not None and s1 is not None and minus == s1}
    return ({"MV1": mv1, "MV2": mv2, "MV3": mv3, "MV4": mv4, "MV5": mv5, "MV6": mv6, "MV7": mv7}, internal)


def claim_id_values(text):
    """Values of the lines 'optional whitespace, claim_id:, optional whitespace, non-space run' (per line)."""
    out = []
    for line in to_lf(text).split("\n"):
        k = 0
        while k < len(line) and line[k].isspace():
            k += 1
        if not line.startswith("claim_id:", k):
            continue
        k += len("claim_id:")
        while k < len(line) and line[k].isspace():
            k += 1
        m = k
        while m < len(line) and not line[m].isspace():
            m += 1
        if m > k:
            out.append(line[k:m])
    return out


def acceptor_classify(repo, rev, path):
    """Classify step 2 facts the acceptor recomputes: the claim_id lines of the state at rev."""
    repo.require_commit(rev)
    text = repo.text_at(rev, path)
    if text is None:
        raise AcceptError("MISSING_INPUT", "state file absent at --state-rev")
    return {"ClaimIdLines": claim_id_values(text)}


def derive_is_ambiguous(derive):
    """Errata E4: two or more trailer commits, or one trailer without a qualifying merge."""
    n = len(derive["TrailerCommits"])
    return n >= 2 or (n == 1 and derive["Eff"] is None)


def acceptor_clausemap(repo, s):
    """Derive, Entry and Validate computed by the acceptor; returns (facts, internal)."""
    derive = acceptor_derive(repo, s)
    entry = acceptor_entry(repo, s)
    validate, internal = (None, None)
    if derive["Eff"] is not None:
        validate, internal = acceptor_validate(repo, derive["Eff"])
    return {"Derive": derive, "Entry": entry, "Validate": validate}, internal


def map_part_defects(cm, internal):
    """Defects that forbid an Authority 'pass' (Evaluate E2-E3 and Validate MV-1..MV-7)."""
    d = []
    der, ent = cm["Derive"], cm["Entry"]
    n_tr = len(der["TrailerCommits"])
    if n_tr > 1:
        d.append("ACTIVATION: more than one trailer commit")
    if n_tr == 1 and der["Eff"] is None:
        d.append("ACTIVATION: trailer present but no derivable EFF")
    if der["Eff"] is None:
        if ent["EntryMatches"]:
            d.append("ACTIVATION: entry point present without EFF")
        return d
    if len(ent["EntryMatches"]) != 1:
        d.append("ENTRY: entry point count is not 1")
    if len(ent["R163Matches"]) != 1:
        d.append("ENTRY: 16.13 heading count is not 1")
    if ent["EntryNamesR163"] is not True:
        d.append("ENTRY: entry point does not name 16.13")
    v = cm["Validate"]
    mv1 = v["MV1"]
    if not mv1["MapPresent"] or not mv1["MapParses"]:
        d.append("MV-1: map absent or unreadable")
    if not mv1["SchemaPresent"] or not internal["SchemaParses"]:
        d.append("MV-1: schema absent or unreadable")
    if internal["Unsupported"]:
        d.append("MV-1: schema keyword outside the supported subset")
    if mv1["SchemaViolations"]:
        d.append("MV-1: map invalid against its schema")
    if v["MV2"]["ClosedList"] is None or canonical(v["MV2"]["MapSurfaces"]) != canonical(v["MV2"]["ClosedList"]):
        d.append("MV-2: Surfaces differ from the closed list")
    if not internal["ShapeOk"]:
        d.append("MV-3..MV-6: map shape not evaluable")
    else:
        m3, m4, m5, m6 = v["MV3"], v["MV4"], v["MV5"], v["MV6"]
        for k in ("OnlyInChanged", "OnlyInFiles", "FilesDuplicates", "DeletedUnderSurfaces"):
            if m3[k]:
                d.append("MV-3: " + k)
        for k in ("LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1"):
            if m4[k]:
                d.append("MV-4: " + k)
        for k in ("EntryKeyDuplicates", "EntriesOnNonModifiedPath"):
            if m5[k]:
                d.append("MV-5: " + k)
        for rec in m6["PerFile"]:
            if rec["DerivedModified"] is None:
                d.append("MV-6: modified file absent in a revision")
                continue
            if rec["H1Duplicates"] or rec["H2Duplicates"]:
                d.append("MV-6: duplicated headings")
            for a, b in (("DerivedModified", "MapModified"), ("DerivedRemoved", "MapRemoved"),
                         ("DerivedAddedOrEntry", "MapAddedOrEntry")):
                if sorted(rec[a]) != sorted(rec[b]):
                    d.append("MV-6: " + a + " != " + b)
        if canonical(m6["EntryKindEntries"]) != canonical(m6["ExpectedEntryKind"]):
            d.append("MV-6: ENTRY entries differ from the fixed pair")
    m7 = v["MV7"]
    if m7["EntryCountEff"] != 1 or m7["EntryNamesR163Eff"] is not True or internal["R163CountEff"] != 1:
        d.append("MV-7: entry point / 16.13 in EFF")
    if not m7["PointerIsFirstSentenceS16"] or not m7["PointerIsLastSentenceS163"]:
        d.append("MV-7: pointer position")
    if not m7["S163EqualAfterPointerRemoval"]:
        d.append("MV-7: 16.3 differs after pointer removal")
    return d


# --------------------------------------------------------------------------- comparison helpers

_MISSING = object()


def dig(obj, *keys):
    for k in keys:
        if isinstance(obj, dict) and k in obj:
            obj = obj[k]
        else:
            return _MISSING
    return obj


def brief(v):
    if v is _MISSING:
        return "<absent>"
    s = canonical(v)
    if len(s) <= 240:
        return v
    return {"Sha256": sha256_hex(s.encode("ascii")), "Length": len(s)}


class Cmp(object):
    def __init__(self):
        self.count = 0
        self.bad = []
        self.skipped = []

    def eq(self, field, mine, theirs, norm=None):
        self.count += 1
        if theirs is _MISSING:
            self.bad.append({"Field": field, "Problem": "absent from the tool Facts"})
            return False
        a, b = mine, theirs
        if norm is not None:
            a = norm(mine)
            try:
                b = norm(theirs)
            except (ValueError, TypeError, AttributeError, KeyError):
                self.bad.append({"Field": field, "Problem": "unrecognized representation", "Tool": brief(theirs)})
                return False
        if canonical(a) != canonical(b):
            self.bad.append({"Field": field, "Acceptor": brief(mine), "Tool": brief(theirs)})
            return False
        return True

    def skip(self, field, why):
        self.skipped.append({"Field": field, "Reason": why})


def n_sorted(v):
    if not isinstance(v, list):
        raise ValueError("list expected")
    return sorted(canonical(x) for x in v)


def n_set(v):
    if not isinstance(v, list):
        raise ValueError("list expected")
    return sorted(set(canonical(x) for x in v))


def n_ws_set(v):
    if not isinstance(v, list) or not all(isinstance(x, str) for x in v):
        raise ValueError("list of strings expected")
    return sorted(set(norm_ws(x) for x in v))


def n_nonempty(v):
    if v is None:
        return False
    if isinstance(v, list):
        return len(v) > 0
    raise ValueError("list or null expected")


def n_u19_list(v):
    if not isinstance(v, list) or not all(isinstance(x, str) for x in v):
        raise ValueError("list of strings expected")
    return sorted(u19(x) for x in v)


def n_entries_flex(v):
    if not isinstance(v, list):
        raise ValueError("list expected")
    out = []
    for x in v:
        if isinstance(x, str):
            out.append(x)
        elif isinstance(x, dict) and isinstance(x.get("Entry"), str):
            out.append(x["Entry"])
        else:
            raise ValueError("element")
    return sorted(out)


def n_pairs(v):
    if not isinstance(v, list):
        raise ValueError("list expected")
    out = []
    for x in v:
        if isinstance(x, dict):
            p, s = x.get("Path"), x.get("Section")
        elif isinstance(x, list) and len(x) == 2:
            p, s = x
        elif isinstance(x, str) and "|" in x:
            p, s = x.split("|", 1)
        else:
            raise ValueError("pair element")
        if not isinstance(p, str) or not isinstance(s, str):
            raise ValueError("pair element")
        out.append([p, norm_ws(s)])
    return sorted(out)


def n_pairs_set(v):
    out = []
    for x in n_pairs(v):
        if x not in out:
            out.append(x)
    return out


def _find_mv6_records(mv6):
    if isinstance(mv6, list):
        return mv6
    if isinstance(mv6, dict):
        for k in ("PerFile", "Files", "Records", "PerPath", "Modified"):
            if isinstance(mv6.get(k), list):
                return mv6[k]
        for k in sorted(mv6):
            v = mv6[k]
            if isinstance(v, list) and v and all(isinstance(e, dict) and "DerivedModified" in e for e in v):
                return v
    return _MISSING


def compare_clausemap(mine, internal, facts, cmp, only_mv4=False):
    if not only_mv4:
        md = mine["Derive"]
        cmp.count += 1
        if derive_is_ambiguous(md):
            cmp.bad.append({"Field": "Derive", "Problem": "errata E4: activation is ambiguous (AMBIGUOUS "
                            "expected) but the tool emitted Facts", "Acceptor": brief(md["TrailerCommits"])})
        if "Classify" in mine:
            lines = mine["Classify"]["ClaimIdLines"]
            cmp.eq("Classify.ClaimIdLines", lines, dig(facts, "Classify", "ClaimIdLines"))
            cmp.count += 1
            if len(set(lines)) >= 2:
                cmp.bad.append({"Field": "Classify", "Problem": "errata E4: two or more distinct claim_id values "
                                "(AMBIGUOUS expected) but the tool emitted Facts", "Acceptor": brief(lines)})
        dd = dig(facts, "Derive")
        cmp.eq("Derive.TrailerCommits", md["TrailerCommits"], dig(dd, "TrailerCommits"), n_set)
        for k in ("FirstParentMerges", "Candidates", "Eff", "EffParent1", "EffParent2"):
            cmp.eq("Derive." + k, md[k], dig(dd, k))
        de = dig(facts, "Entry")
        for k in ("WorkflowBlob", "ApBlob", "EntryMatches", "R163Matches", "EntryNamesR163"):
            cmp.eq("Entry." + k, mine["Entry"][k], dig(de, k))
    mv = mine["Validate"]
    tv = dig(facts, "Validate")
    if mv is None:
        if not only_mv4:
            cmp.eq("Validate", None, tv)
        else:
            cmp.skip("Validate.MV4", "no EFF")
        return
    if not isinstance(tv, dict):
        cmp.eq("Validate", "<object>", tv)
        return
    shape = internal["ShapeOk"]
    if only_mv4:
        if not shape:
            cmp.skip("Validate.MV4", "map shape not evaluable")
            return
        t4 = dig(tv, "MV4")
        for k in ("LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1", "EntryFilePresentInEff1"):
            cmp.eq("Validate.MV4." + k, mv["MV4"][k], dig(t4, k), n_sorted)
        return
    t1 = dig(tv, "MV1")
    for k in ("MapPresent", "MapBlob", "MapParses", "SchemaPresent", "SchemaBlob"):
        cmp.eq("Validate.MV1." + k, mv["MV1"][k], dig(t1, k))
    cmp.eq("Validate.MV1.SchemaViolations[nonempty]", mv["MV1"]["SchemaViolations"],
           dig(t1, "SchemaViolations"), n_nonempty)
    t2 = dig(tv, "MV2")
    cmp.eq("Validate.MV2.ClosedList", mv["MV2"]["ClosedList"], dig(t2, "ClosedList"))
    if mv["MV1"]["MapParses"]:
        cmp.eq("Validate.MV2.MapSurfaces", mv["MV2"]["MapSurfaces"], dig(t2, "MapSurfaces"))
    if not shape:
        cmp.skip("Validate.MV3-MV6", "map shape not evaluable")
    else:
        t3 = dig(tv, "MV3")
        for k in ("ChangedUnderSurfaces", "DeletedUnderSurfaces", "OnlyInChanged", "OnlyInFiles"):
            cmp.eq("Validate.MV3." + k, mv["MV3"][k], dig(t3, k), n_sorted)
        cmp.eq("Validate.MV3.FilesPaths", mv["MV3"]["FilesPaths"], dig(t3, "FilesPaths"))
        cmp.eq("Validate.MV3.FilesDuplicates", mv["MV3"]["FilesDuplicates"], dig(t3, "FilesDuplicates"), n_set)
        t4 = dig(tv, "MV4")
        cmp.eq("Validate.MV4.Rows", mv["MV4"]["Rows"], dig(t4, "Rows"))
        for k in ("LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1", "EntryFilePresentInEff1"):
            cmp.eq("Validate.MV4." + k, mv["MV4"][k], dig(t4, k), n_sorted)
        t5 = dig(tv, "MV5")
        cmp.eq("Validate.MV5.EntryKeyDuplicates", mv["MV5"]["EntryKeyDuplicates"],
               dig(t5, "EntryKeyDuplicates"), n_pairs_set)
        mine_nm = mv["MV5"]["EntriesOnNonModifiedPath"]
        tool_nm = dig(t5, "EntriesOnNonModifiedPath")
        if isinstance(tool_nm, list) and tool_nm and all(isinstance(x, str) and "|" not in x for x in tool_nm):
            cmp.eq("Validate.MV5.EntriesOnNonModifiedPath[paths]", [x["Path"] for x in mine_nm], tool_nm, n_sorted)
        else:
            cmp.eq("Validate.MV5.EntriesOnNonModifiedPath", mine_nm, tool_nm, n_pairs)
        t6 = dig(tv, "MV6")
        recs = _find_mv6_records(t6)
        mine_per = mv["MV6"]["PerFile"]
        if recs is _MISSING and mine_per:
            cmp.eq("Validate.MV6.PerFile", "<records>", _MISSING)
        elif recs is not _MISSING:
            by_path = {}
            for r in recs:
                if isinstance(r, dict) and isinstance(r.get("Path"), str) and r["Path"] not in by_path:
                    by_path[r["Path"]] = r
            cmp.eq("Validate.MV6.Paths", [r["Path"] for r in mine_per],
                   [r.get("Path") if isinstance(r, dict) else None for r in recs], n_set)
            for r in mine_per:
                t = by_path.get(r["Path"], _MISSING)
                where = "Validate.MV6[" + r["Path"] + "]."
                for k in ("MapModified", "MapRemoved", "MapAddedOrEntry"):
                    cmp.eq(where + k, r[k], dig(t, k), n_ws_set)
                if r["DerivedModified"] is None:
                    cmp.skip(where + "Derived*", "file absent in a revision")
                    continue
                for k in ("H1Duplicates", "H2Duplicates"):
                    cmp.eq(where + k, r[k], dig(t, k), n_ws_set)
                if r["H1Duplicates"] or r["H2Duplicates"]:
                    cmp.skip(where + "Derived*", "duplicated headings")
                    continue
                for k in ("DerivedModified", "DerivedRemoved", "DerivedAddedOrEntry"):
                    cmp.eq(where + k, r[k], dig(t, k), n_ws_set)
        for k in ("EntryKindEntries", "ExpectedEntryKind"):
            tv6 = dig(t6, k)
            if tv6 is _MISSING:
                tv6 = dig(tv, k)
            cmp.eq("Validate.MV6." + k, mv["MV6"][k], tv6, n_pairs_set)
    t7 = dig(tv, "MV7")
    for k in ("EntryCountEff", "EntryNamesR163Eff", "FirstSentenceS16", "PointerIsFirstSentenceS16",
              "LastSentenceS163", "PointerIsLastSentenceS163", "S163EffMinusPointer", "S163Eff1",
              "S163EqualAfterPointerRemoval"):
        cmp.eq("Validate.MV7." + k, mv["MV7"][k], dig(t7, k))


# --------------------------------------------------------------------------- tool argv / envelopes

_FLAG_OPTIONS = frozenset(["--allow-absent"])
_MULTI_OPTIONS = frozenset(["--file", "--sha256", "--pointer"])


def parse_tool_argv(argv):
    """Parses the tool argv (SPEC 2) for the commands the acceptor recomputes."""
    if not isinstance(argv, list) or not argv or not all(isinstance(a, str) for a in argv):
        raise AcceptError("USAGE", "argv")
    opts, multi = {}, dict((k, []) for k in _MULTI_OPTIONS)
    i = 1
    while i < len(argv):
        tok = argv[i]
        if not tok.startswith("--"):
            raise AcceptError("USAGE", "unexpected positional argument")
        name, eq, val = tok.partition("=")
        if name in _FLAG_OPTIONS:
            opts[name] = True
            i += 1
            continue
        if name == "--paths":
            vals = [val] if eq else []
            i += 1
            while i < len(argv) and not argv[i].startswith("--"):
                vals.append(argv[i])
                i += 1
            opts[name] = vals
            continue
        if not eq:
            if i + 1 >= len(argv):
                raise AcceptError("USAGE", "missing value for " + name)
            val = argv[i + 1]
            i += 2
        else:
            i += 1
        if name in multi:
            multi[name].append(val)
        elif name in opts:
            raise AcceptError("USAGE", "repeated option " + name)
        else:
            opts[name] = val
    files, shas = {}, {}
    for item in multi["--file"]:
        n, sep, p = item.partition("=")
        if not sep or n in files:
            raise AcceptError("USAGE", "--file")
        files[n] = p
    for item in multi["--sha256"]:
        n, sep, h = item.partition("=")
        if not sep or n in shas:
            raise AcceptError("USAGE", "--sha256")
        shas[n] = h
    return argv[0], opts, files, shas


# Errata E1: verbatim echoes of input data are outside the vocabulary rule. Patterns are
# Facts paths; "*" = any one segment; a trailing "**" exempts the keys and values strictly
# inside that node. Without "**" only the value at exactly that path is exempt (its key,
# chosen by the tool, is still checked).
VOCABULARY_EXEMPT = {
    "trx": (("Counters", "**"), ("Tests", "*", "Outcome")),
    "freetext": (("Terms", "*"), ("Matches", "*", "Term")),
    "read-text": (("Lines", "*", "Text"),),
    "read-json": (("Values", "**"),),
    "handoff": (("Fields", "**"),),
    "clausemap": (("Validate", "MV7", "FirstSentenceS16"), ("Validate", "MV7", "LastSentenceS163"),
                  ("Validate", "MV7", "S163EffMinusPointer"), ("Validate", "MV7", "S163Eff1"),
                  ("Pre", "HeaderCells", "**"), ("Pre", "Rows", "**"), ("Pre", "MalformedRows", "**"),
                  ("Pre", "ClaimIdCounts", "**")),
}


def _segments_match(pattern, path):
    return len(pattern) == len(path) and all(p == "*" or p == s for p, s in zip(pattern, path))


def _vocab_exempt(path, patterns, is_key):
    for pat in patterns:
        if pat and pat[-1] == "**":
            head = pat[:-1]
            if len(path) > len(head) and _segments_match(head, path[:len(head)]):
                return True
        elif not is_key and _segments_match(pat, path):
            return True
    return False


def _walk_vocabulary(obj, path, out, patterns=()):
    where = "/" + "/".join(path)
    if isinstance(obj, dict):
        for k, v in obj.items():
            sub = path + (k,)
            if any(w in k for w in KEY_FORBIDDEN_SUBSTRINGS) and not _vocab_exempt(sub, patterns, True):
                out.append("/" + "/".join(sub))
            _walk_vocabulary(v, sub, out, patterns)
    elif isinstance(obj, list):
        for i, v in enumerate(obj):
            _walk_vocabulary(v, path + (str(i),), out, patterns)
    elif isinstance(obj, str):
        if (obj in VALUE_FORBIDDEN_EXACT or obj.startswith("EXECUTION_")) and not _vocab_exempt(path, patterns, False):
            out.append(where)


def vocabulary_violations(command, facts):
    """SPEC 1.8 with errata E1 (exemptions depend on the command)."""
    out = []
    _walk_vocabulary(facts, (), out, VOCABULARY_EXEMPT.get(command, ()))
    return out


def envelope_problems(raw, argv):
    """Structural conformance of one envelope with SPEC 1.5 / 1.8; returns (env, problems)."""
    probs = []
    if any(b >= 0x80 for b in raw):
        probs.append("envelope has bytes >= 0x80")
    try:
        env = parse_json_bytes(raw, "envelope")
    except AcceptError as e:
        return None, probs + ["envelope unreadable: " + e.code]
    if not isinstance(env, dict):
        return None, probs + ["envelope is not an object"]
    if raw != (canonical(env) + "\n").encode("ascii"):
        probs.append("envelope bytes are not the canonical serialization")
    if set(env) != ENVELOPE_KEYS:
        probs.append("envelope keys differ from SPEC 1.5")
    if env.get("Schema") != FACTS_SCHEMA:
        probs.append("Schema is not " + FACTS_SCHEMA)
    if isinstance(argv, list) and argv and env.get("Command") != argv[0]:
        probs.append("Command differs from the declared argv")
    if not isinstance(env.get("Args"), dict):
        probs.append("Args is not an object")
    inputs = env.get("Inputs")
    if not isinstance(inputs, list) or not all(isinstance(x, dict) for x in inputs):
        probs.append("Inputs is not a list of objects")
    calls = env.get("GitCalls")
    if not isinstance(calls, list) or not all(isinstance(c, list) and all(isinstance(a, str) for a in c)
                                              for c in calls):
        probs.append("GitCalls is not a list of argv lists")
    if env.get("Error") is not None:
        probs.append("Error is not null")
    facts = env.get("Facts")
    if not isinstance(facts, dict):
        probs.append("Facts is not an object")
    else:
        if env.get("FactsSha256") != sha256_hex(canonical(facts).encode("ascii")):
            probs.append("FactsSha256 differs from the SHA-256 of the canonical Facts")
        vocab = vocabulary_violations(env.get("Command"), facts)
        if vocab:
            probs.append("verdict vocabulary in Facts at " + ",".join(vocab[:5]))
    rt = env.get("Runtime")
    if not (isinstance(rt, dict) and set(rt) == set(["Python", "Isolated"])
            and isinstance(rt.get("Python"), str) and isinstance(rt.get("Isolated"), bool)):
        probs.append("Runtime is not {Python, Isolated}")
    return env, probs


class Run(object):
    def __init__(self, idx, spec):
        self.name = "run-" + str(idx)
        self.argv = None
        self.env = None
        self.problems = []
        self.cmd = None
        self.opts, self.files, self.shas = {}, {}, {}
        self.argv_error = None
        if not isinstance(spec, dict):
            self.problems.append("run entry is not an object")
            return
        if isinstance(spec.get("Name"), str) and spec["Name"]:
            self.name = spec["Name"]
        argv = spec.get("Argv")
        if not isinstance(argv, list) or not argv or not all(isinstance(a, str) for a in argv):
            self.problems.append("Argv missing or malformed")
        else:
            self.argv = list(argv)
            self.cmd = argv[0]
            if self.cmd in COMPARED_COMMANDS:
                try:
                    _, self.opts, self.files, self.shas = parse_tool_argv(self.argv)
                except AcceptError as e:
                    self.argv_error = e.code + ": " + e.detail
        try:
            raw = read_input_file(spec.get("Envelope"), "Envelope")
        except AcceptError as e:
            self.problems.append(e.code + ": " + e.detail)
            return
        self.env, probs = envelope_problems(raw, self.argv)
        self.problems.extend(probs)

    def facts(self):
        if self.env is None or not isinstance(self.env.get("Facts"), dict):
            return None
        return self.env["Facts"]

    def file_json(self, name):
        path = self.files.get(name)
        want = self.shas.get(name)
        if path is None or not is_hex(want, 64):
            raise AcceptError("MISSING_INPUT", "--file/--sha256 " + name)
        data = read_input_file(path, name)
        if sha256_hex(data) != want:
            raise AcceptError("INPUT_HASH_MISMATCH", name)
        return parse_json_bytes(data, name)

    def opt(self, name):
        v = self.opts.get(name)
        if v is None:
            raise AcceptError("USAGE", "missing " + name)
        return v


# --------------------------------------------------------------------------- context

class Ctx(object):
    def __init__(self, req):
        self.req = req
        na = req.get("NotApplicable")
        self.na = na if isinstance(na, dict) else {}
        self._repo = None
        runs = req.get("Runs")
        self.runs = [Run(i, s) for i, s in enumerate(runs)] if isinstance(runs, list) else []
        self.tool_ok = False
        self.tool_cfg = None
        self.tool_file_bytes = None
        self.tool_object_bytes = None
        self.runtimes = []
        self._mine = {}
        self.cv = None
        self.r4_clausemap = None

    @property
    def repo(self):
        if self._repo is None:
            self._repo = RepoView(self.req.get("Repo"))
        return self._repo

    def mine_for(self, run):
        """Acceptor facts for one run (cached)."""
        if run.name in self._mine:
            return self._mine[run.name]
        if run.argv_error:
            raise AcceptError("USAGE", run.argv_error)
        if not same_path(run.opts.get("--repo"), self.req.get("Repo")):
            raise AcceptError("USAGE", "run --repo differs from the request Repo")
        repo, cmd = self.repo, run.cmd
        internal = None
        if cmd == "identity":
            mine = acceptor_identity(repo, run.opt("--branch"), run.opt("--base"), run.opt("--current"),
                                     run.opts.get("--red"))
        elif cmd == "diff":
            mine = acceptor_diff(repo, run.opt("--from"), run.opt("--to"))
        elif cmd == "scope":
            mine = acceptor_scope(repo, run.opt("--from"), run.opt("--to"), run.file_json("DELEG"))
        elif cmd == "hashes":
            paths = run.opt("--paths")
            if not isinstance(paths, list) or not paths:
                raise AcceptError("USAGE", "--paths")
            mine = acceptor_hashes(repo, run.opt("--rev"), paths)
        elif cmd == "contract":
            mine = acceptor_contract(run.file_json("CONTRACT"), run.file_json("DELEG"))
        elif cmd == "clausemap":
            mine, internal = acceptor_clausemap(repo, run.opt("--main"))
            if "--state-rev" in run.opts or "--state-path" in run.opts:
                mine["Classify"] = acceptor_classify(repo, run.opt("--state-rev"), run.opt("--state-path"))
        else:
            raise AcceptError("USAGE", "command not recomputed by the acceptor")
        self._mine[run.name] = (mine, internal)
        return self._mine[run.name]


def _status(ok):
    return "MATCH" if ok else "MISMATCH"


# --------------------------------------------------------------------------- R1 / R2 / R3

def check_r1(ctx):
    t = ctx.req.get("Tool")
    if not isinstance(t, dict):
        raise AcceptError("MISSING_INPUT", "Tool")
    ctx.tool_cfg = t
    pinned = t.get("PinnedBlob")
    if not is_hex(pinned, 40):
        raise AcceptError("MISSING_INPUT", "Tool.PinnedBlob")
    run_from = t.get("RunFrom", "path")
    if run_from not in ("path", "object"):
        raise AcceptError("USAGE", "Tool.RunFrom")
    detail = {"PinnedBlob": pinned, "RunFrom": run_from}
    ok = True
    if t.get("Path") is not None:
        ctx.tool_file_bytes = read_input_file(t.get("Path"), "Tool.Path")
        detail["FileBlob"] = git_blob_sha1(ctx.tool_file_bytes)
        ok = ok and detail["FileBlob"] == pinned
    go = t.get("GitObject")
    if go is not None:
        if not isinstance(go, dict) or not is_hex(go.get("Blob"), 40):
            raise AcceptError("MISSING_INPUT", "Tool.GitObject")
        _, data = git_run(go.get("GitDir"), "cat-file", ["blob", go["Blob"]])
        ctx.tool_object_bytes = data
        detail["ObjectName"] = go["Blob"]
        detail["ObjectBlob"] = git_blob_sha1(data)
        ok = ok and detail["ObjectBlob"] == pinned and go["Blob"] == pinned
    if ctx.tool_file_bytes is None and ctx.tool_object_bytes is None:
        raise AcceptError("MISSING_INPUT", "Tool.Path or Tool.GitObject")
    if run_from == "object" and ctx.tool_object_bytes is None:
        raise AcceptError("MISSING_INPUT", "Tool.GitObject for RunFrom object")
    if run_from == "path" and ctx.tool_file_bytes is None:
        raise AcceptError("MISSING_INPUT", "Tool.Path for RunFrom path")
    ctx.tool_ok = ok
    return _status(ok), detail


def interpreter_image_sha256():
    """SHA-256 of sys.executable; when that path is not a readable file (Windows App
    Execution Alias), the interpreter image with the same name under sys.base_prefix."""
    candidates = [("sys.executable", sys.executable)]
    name = os.path.basename(sys.executable or "") or "python"
    candidates.append(("sys.base_prefix", os.path.join(sys.base_prefix, name)))
    for label, path in candidates:
        if not path:
            continue
        try:
            return label, sha256_hex(read_file(path))
        except OSError:
            continue
    return None, None


def rerun_tool(ctx, run):
    exe = sys.executable
    if ctx.tool_cfg.get("RunFrom", "path") == "object":
        cmd = [exe, "-I", "-"] + run.argv
        stdin = ctx.tool_object_bytes
    else:
        cmd = [exe, "-I", ctx.tool_cfg["Path"]] + run.argv
        stdin = b""
    repo = ctx.req.get("Repo")
    cwd = repo if isinstance(repo, str) and os.path.isdir(repo) else None
    try:
        proc = subprocess.run(cmd, input=stdin, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                              env=_clean_env(), shell=False, timeout=900, cwd=cwd)
    except (OSError, subprocess.SubprocessError):
        raise AcceptError("TOOL_ERROR", "the tool re-run could not run")
    return proc.returncode, proc.stdout


def check_r3(ctx):
    if not ctx.runs:
        return None
    details = []
    ok_all = True
    for run in ctx.runs:
        d = {"Name": run.name}
        probs = list(run.problems)
        if run.env is not None and isinstance(run.env.get("Runtime"), dict):
            ctx.runtimes.append((run.name, run.env["Runtime"]))
        if run.argv is None or run.env is None:
            pass
        elif not ctx.tool_ok:
            probs.append("tool not executed: tool identity (R1) not established")
        else:
            try:
                rc, out = rerun_tool(ctx, run)
                d["RerunExit"] = rc
                if rc != 0:
                    probs.append("re-run exit code is not 0")
                env2, probs2 = envelope_problems(out, run.argv)
                probs.extend("re-run: " + x for x in probs2)
                if env2 is not None:
                    if isinstance(env2.get("Runtime"), dict):
                        ctx.runtimes.append((run.name + "/re-run", env2["Runtime"]))
                    f1, f2 = run.env.get("Facts"), env2.get("Facts")
                    d["FactsBytesEqual"] = canonical(f1) == canonical(f2)
                    d["FactsSha256Recomputed"] = sha256_hex(canonical(f1).encode("ascii")) if isinstance(f1, dict) else None
                    for k in ("Schema", "Command", "Args", "Inputs", "GitCalls", "Error", "FactsSha256", "Facts"):
                        if canonical(run.env.get(k)) != canonical(env2.get(k)):
                            probs.append("re-run differs in " + k)
            except AcceptError as e:
                probs.append("re-run: " + e.code)
        d["Problems"] = probs
        ok_all = ok_all and not probs
        details.append(d)
    res = {"Runs": details}
    if ctx.tool_ok and ctx.tool_cfg.get("RunFrom", "path") == "path":
        try:
            after = git_blob_sha1(read_file(ctx.tool_cfg["Path"]))
        except OSError:
            after = None
        res["ToolFileUnchanged"] = after == ctx.tool_cfg.get("PinnedBlob")
        ok_all = ok_all and res["ToolFileUnchanged"]
    return _status(ok_all), res


def check_r2(ctx):
    decl = ctx.req.get("Interpreter")
    if not isinstance(decl, dict) or not isinstance(decl.get("Python"), str) \
            or not is_hex(decl.get("ExecutableSha256"), 64):
        raise AcceptError("MISSING_INPUT", "Interpreter.Python / Interpreter.ExecutableSha256")
    src, exe_sha = interpreter_image_sha256()
    own = {"Isolated": sys.flags.isolated == 1, "Python": platform.python_version(),
           "ExecutableSha256": exe_sha, "ExecutableSource": src}
    probs = []
    if not own["Isolated"]:
        probs.append("acceptor interpreter is not isolated (sys.flags.isolated != 1)")
    if own["Python"] != decl["Python"]:
        probs.append("python version differs from the declared one")
    if exe_sha is None:
        probs.append("interpreter image unreadable")
    elif exe_sha != decl["ExecutableSha256"]:
        probs.append("interpreter SHA-256 differs from the declared one")
    for name, rt in ctx.runtimes:
        if rt.get("Isolated") is not True:
            probs.append(name + ": Runtime.Isolated is not true")
        if rt.get("Python") != decl["Python"]:
            probs.append(name + ": Runtime.Python differs from the declared one")
    return _status(not probs), {"Acceptor": own, "Declared": {"Python": decl["Python"],
                                                              "ExecutableSha256": decl["ExecutableSha256"]},
                                "EnvelopeRuntimesChecked": len(ctx.runtimes), "Problems": probs}


# --------------------------------------------------------------------------- R4

def _r4_identity(ctx, deleg, hand):
    reasons = []
    br_d, br_h = req_str(deleg, "ExpectedBranch", "delegation"), req_str(hand, "Branch", "handoff")
    wt_d, wt_h = req_str(deleg, "ExpectedWorktree", "delegation"), req_str(hand, "Worktree", "handoff")
    base_d, base_h = req_hex(deleg, "BaseSha", "delegation"), req_hex(hand, "BaseSha", "handoff")
    cur = req_hex(hand, "CurrentSha", "handoff")
    if "RedSha" not in hand:
        raise AcceptError("MISSING_INPUT", "handoff.RedSha")
    red = hand["RedSha"]
    if red is not None and not is_hex(red, 40):
        raise AcceptError("MISSING_INPUT", "handoff.RedSha")
    try:
        ident = acceptor_identity(ctx.repo, br_h, base_h, cur, red)
    except AcceptError as e:
        if e.code == "BAD_REVISION":
            return False, ["revision is not an existing commit"]
        raise
    if not (br_h == br_d == ident["AbbrevRef"]):
        reasons.append("branch")
    if not (same_path(wt_h, wt_d) and same_path(wt_h, ident["Toplevel"])):
        reasons.append("worktree")
    if base_h != base_d:
        reasons.append("BaseSha differs between delegation and handoff")
    if not ident["Ancestry"]["base->current"]:
        reasons.append("BaseSha is not an ancestor of CurrentSha")
    if red is not None and not (ident["Ancestry"]["base->red"] and ident["Ancestry"]["red->current"]):
        reasons.append("RedSha is not between BaseSha and CurrentSha")
    if not (ident["Head"] == cur and ident["OriginBranch"] == cur):
        reasons.append("HEAD / refs/remotes/origin/<branch> / CurrentSha differ")
    return not reasons, reasons


def _r4_scope(ctx, deleg, hand):
    allowed = req_str_list(deleg, "AllowedWriteScope", "delegation")
    forbidden = req_str_list(deleg, "ForbiddenWriteScope", "delegation")
    base, cur = req_hex(deleg, "BaseSha", "delegation"), req_hex(hand, "CurrentSha", "handoff")
    try:
        ctx.repo.require_commit(base)
        ctx.repo.require_commit(cur)
    except AcceptError as e:
        if e.code == "BAD_REVISION":
            return False, ["revision is not an existing commit"]
        raise
    paths = tree_diff_paths(ctx.repo, base, cur)
    reasons = []
    bad = [e for e in allowed + forbidden if e == "" or is_malformed(e)]
    if bad:
        reasons.append("malformed scope entries: " + str(len(bad)))
    unc = [p for p in paths if not any(covers(e, p) for e in allowed)]
    if unc:
        reasons.append("paths not covered by AllowedWriteScope: " + str(len(unc)))
    fb = [p for p in paths if any(covers(e, p) for e in forbidden)]
    if fb:
        reasons.append("paths covered by ForbiddenWriteScope: " + str(len(fb)))
    return not reasons, reasons


def _r4_cleantree(ctx, ignore_path):
    if not isinstance(ignore_path, str) or not ignore_path:
        raise AcceptError("MISSING_INPUT", "Verification.IgnorePath")
    _, st = git_run(ctx.repo.path, "status", ["--porcelain", "-z", "--untracked-files=normal"])
    rc, _ = git_run(ctx.repo.path, "check-ignore", ["-q", "--no-index", "--", ignore_path], ok=(0, 1))
    reasons = []
    if st != b"":
        reasons.append("status --porcelain is not empty")
    if rc != 0:
        reasons.append("package path is not ignored")
    return not reasons, reasons


def _r4_trailer(ctx, deleg, hand, declared_override):
    if declared_override is not None:
        if not isinstance(declared_override, str):
            raise AcceptError("MISSING_INPUT", "Verification.DeclaredCoAuthoredBy")
        declared = declared_override
    else:
        declared = req_str(dig(hand, "Worker") if isinstance(dig(hand, "Worker"), dict) else {}, "Trailer",
                           "handoff.Worker")
    declared = strip_trailer_key(declared)
    base, cur = req_hex(deleg, "BaseSha", "delegation"), req_hex(hand, "CurrentSha", "handoff")
    try:
        commits = ctx.repo.range_commits(base, cur)
    except AcceptError as e:
        if e.code == "BAD_REVISION":
            return False, ["revision is not an existing commit"]
        raise
    if not declared:
        return False, ["declared Co-Authored-By is empty"]
    missing = sorted(c for c in commits if declared not in coauthored_values(ctx.repo.message(c)))
    return not missing, (["commits without the declared Co-Authored-By: " + ",".join(missing)] if missing else [])


def _r4_freetext(ctx, hand, terms_rev):
    if not is_hex(terms_rev, 40):
        raise AcceptError("MISSING_INPUT", "Verification.TermsRev")
    ctx.repo.require_commit(terms_rev)
    ap = ctx.repo.text_at(terms_rev, AP_PATH)
    if ap is None:
        raise AcceptError("MISSING_INPUT", "AUTOMATION_PLAN at TermsRev")
    terms = gate_terms(ap)
    matches = freetext_matches(hand, terms)
    return not matches, ["term " + m["Term"] + " in " + m["Field"] for m in matches]


def check_r4(ctx):
    v = ctx.req.get("Verification")
    if v is None:
        return None
    if not isinstance(v, dict):
        raise AcceptError("MISSING_INPUT", "Verification")
    cv = parse_json_bytes(read_pinned(v.get("Controller"), "Verification.Controller"), "controller verification")
    deleg = parse_json_bytes(read_pinned(v.get("Delegation"), "Verification.Delegation"), "delegation")
    hand = parse_json_bytes(read_pinned(v.get("Handoff"), "Verification.Handoff"), "handoff")
    if not isinstance(cv, dict) or cv.get("Schema") != CV_SCHEMA:
        raise AcceptError("MALFORMED_JSON", "controller verification Schema")
    if not isinstance(deleg, dict) or not isinstance(hand, dict):
        raise AcceptError("MALFORMED_JSON", "delegation / handoff")
    ctx.cv = cv
    controller = {}
    for name in R4_CHECKS:
        r = dig(cv, "Checks", name, "Result")
        controller[name] = r if r in ("pass", "fail", "not_run") else None
    mine = {}

    def attempt(name, fn):
        try:
            mine[name] = fn()
        except AcceptError as e:
            mine[name] = (None, ["cannot compute: " + e.code + " " + e.detail])

    attempt("Identity", lambda: _r4_identity(ctx, deleg, hand))
    attempt("Scope", lambda: _r4_scope(ctx, deleg, hand))
    attempt("CleanTree", lambda: _r4_cleantree(ctx, v.get("IgnorePath")))
    attempt("Trailer", lambda: _r4_trailer(ctx, deleg, hand, v.get("DeclaredCoAuthoredBy")))
    attempt("FreeText", lambda: _r4_freetext(ctx, hand, v.get("TermsRev")))

    def authority():
        main = v.get("MainSha")
        if not is_hex(main, 40):
            raise AcceptError("MISSING_INPUT", "Verification.MainSha")
        cm, internal = acceptor_clausemap(ctx.repo, main)
        ctx.r4_clausemap = cm
        return map_part_defects(cm, internal)

    try:
        defects = authority()
    except AcceptError as e:
        defects = None
        mine["Authority"] = (None, ["cannot compute: " + e.code + " " + e.detail])
    rows = []
    ok_all = True
    for name in R4_CHECKS:
        ctl = controller[name]
        row = {"Check": name, "Controller": ctl}
        if name == "Authority" and defects is not None:
            row["Rule"] = "map part: any defect forbids pass"
            row["Defects"] = defects
            good = ctl is not None and not (defects and ctl == "pass")
            row["AcceptorExpects"] = "not pass" if defects else "unconstrained"
        else:
            res, reasons = mine[name]
            row["AcceptorExpects"] = None if res is None else ("pass" if res else "not pass")
            row["Reasons"] = reasons
            good = res is not None and ctl is not None and ((ctl == "pass") == res)
        row["Status"] = _status(good)
        ok_all = ok_all and good
        rows.append(row)
    return _status(ok_all), {"Checks": rows}


# --------------------------------------------------------------------------- R5 / R6 / R7

def _compare_r5(run, mine, facts, cmp):
    cmd = run.cmd
    if cmd == "identity":
        for k in ("Head", "AbbrevRef", "Toplevel", "OriginBranch", "OriginMain", "Ancestry"):
            cmp.eq(k, mine[k], dig(facts, k))
    elif cmd == "diff":
        cmp.eq("Paths", mine["Paths"], dig(facts, "Paths"))
        cmp.eq("Commits[set]", mine["Commits"], dig(facts, "Commits"), n_sorted)
        cmp.eq("CoAuthoredBy", mine["CoAuthoredBy"], dig(facts, "CoAuthoredBy"))
    elif cmd == "scope":
        for k in ("Paths", "AllowedEntries", "ForbiddenEntries"):
            cmp.eq(k, mine[k], dig(facts, k))
    elif cmd == "hashes":
        cmp.eq("PerPath", mine["PerPath"], dig(facts, "PerPath"))


def _compare_r7(run, mine, internal, facts, cmp):
    cmd = run.cmd
    if cmd == "scope":
        tper = dig(facts, "PerPath")
        cmp.eq("PerPath.Path", [x["Path"] for x in mine["PerPath"]],
               [x.get("Path") for x in tper] if isinstance(tper, list) and all(isinstance(x, dict) for x in tper)
               else tper)
        if isinstance(tper, list) and all(isinstance(x, dict) for x in tper):
            for i, row in enumerate(mine["PerPath"]):
                t = tper[i] if i < len(tper) else _MISSING
                for k in ("AllowedCoveredBy", "ForbiddenCoveredBy"):
                    cmp.eq("PerPath[" + row["Path"] + "]." + k, row[k], dig(t, k), n_u19_list)
        cmp.eq("MalformedEntries", mine["MalformedEntries"], dig(facts, "MalformedEntries"), n_entries_flex)
        cmp.eq("U19Normalized", mine["U19Normalized"], dig(facts, "U19Normalized"))
    elif cmd == "contract":
        for part in ("A3", "A4"):
            tl = dig(facts, part)
            ok_shape = isinstance(tl, list) and all(isinstance(x, dict) for x in tl)
            cmp.eq(part + ".Entry", [x["Entry"] for x in mine[part]],
                   [x.get("Entry") for x in tl] if ok_shape else tl)
            if ok_shape:
                for i, row in enumerate(mine[part]):
                    t = tl[i] if i < len(tl) else _MISSING
                    cmp.eq(part + "[" + row["Entry"] + "].ContainedBy", row["ContainedBy"], dig(t, "ContainedBy"),
                           n_u19_list)
    elif cmd == "clausemap":
        compare_clausemap(mine, internal, facts, cmp, only_mv4=True)


def _run_comparisons(ctx, commands, compare):
    selected = [r for r in ctx.runs if r.cmd in commands]
    if not selected:
        return None
    out = []
    ok_all = True
    for run in selected:
        d = {"Name": run.name, "Command": run.cmd}
        facts = run.facts()
        if facts is None:
            d["Problems"] = ["envelope without usable Facts"]
            ok_all = False
            out.append(d)
            continue
        try:
            mine, internal = ctx.mine_for(run)
        except AcceptError as e:
            d["Problems"] = ["acceptor facts not computable: " + e.code + " " + e.detail]
            ok_all = False
            out.append(d)
            continue
        cmp = Cmp()
        compare(run, mine, internal, facts, cmp)
        d["Compared"] = cmp.count
        d["Mismatches"] = cmp.bad
        if cmp.skipped:
            d["NotCompared"] = cmp.skipped
        ok_all = ok_all and not cmp.bad
        out.append(d)
    return ok_all, out


def check_r5(ctx):
    res = _run_comparisons(ctx, R5_COMMANDS, lambda run, m, i, f, c: _compare_r5(run, m, f, c))
    if res is None:
        return None
    return _status(res[0]), {"Runs": res[1]}


def check_r6(ctx):
    res = _run_comparisons(ctx, frozenset(["clausemap"]),
                           lambda run, m, i, f, c: compare_clausemap(m, i, f, c))
    ar = dig(ctx.cv, "AuthorityResolution") if ctx.cv is not None else _MISSING
    ar_rows = None
    ar_ok = True
    if isinstance(ar, list) and ar:
        ar_rows = []
        cm = ctx.r4_clausemap
        if cm is None:
            ar_ok = False
            ar_rows.append({"Problem": "acceptor Derive at Verification.MainSha not available"})
        else:
            eff = cm["Derive"]["Eff"]
            mblob = cm["Validate"]["MV1"]["MapBlob"] if cm["Validate"] is not None else None
            for i, e in enumerate(ar):
                row = {"Index": i, "EffectiveSha": dig(e, "EffectiveSha"), "ClauseMapBlob": dig(e, "ClauseMapBlob")}
                good = row["EffectiveSha"] == eff and eff is not None and row["ClauseMapBlob"] == mblob
                row["Status"] = _status(good)
                for k in ("EffectiveSha", "ClauseMapBlob"):
                    if row[k] is _MISSING:
                        row[k] = "<absent>"
                ar_ok = ar_ok and good
                ar_rows.append(row)
    if res is None and ar_rows is None:
        return None
    detail = {}
    ok = True
    if res is not None:
        detail["Runs"] = res[1]
        ok = ok and res[0]
    if ar_rows is not None:
        detail["AuthorityResolution"] = ar_rows
        ok = ok and ar_ok
    return _status(ok), detail


def check_r7(ctx):
    res = _run_comparisons(ctx, R7_COMMANDS, _compare_r7)
    if res is None:
        return None
    return _status(res[0]), {"Runs": res[1]}


# --------------------------------------------------------------------------- R9 / R10

class Closure(object):
    def __init__(self, obj, repo):
        if not isinstance(obj, dict) or obj.get("Schema") != CLOSURE_SCHEMA:
            raise AcceptError("MALFORMED_JSON", "closure Schema")

        def strs(key):
            v = obj.get(key, [])
            if not isinstance(v, list) or not all(isinstance(x, str) for x in v):
                raise AcceptError("MALFORMED_JSON", "closure." + key)
            return v

        self.commits = set(strs("Commits"))
        if not all(is_hex(c, 40) for c in self.commits):
            raise AcceptError("MALFORMED_JSON", "closure.Commits")
        self.objects = set(strs("Objects"))
        self.refs = set(strs("Refs"))
        self.paths = set(strs("Paths"))
        self.revpaths = set()
        rp = obj.get("RevPaths", [])
        if not isinstance(rp, list):
            raise AcceptError("MALFORMED_JSON", "closure.RevPaths")
        for x in rp:
            if isinstance(x, dict) and isinstance(x.get("Rev"), str) and isinstance(x.get("Path"), str):
                self.revpaths.add((x["Rev"], x["Path"]))
            elif isinstance(x, str) and ":" in x:
                r, p = x.split(":", 1)
                self.revpaths.add((r, p))
            else:
                raise AcceptError("MALFORMED_JSON", "closure.RevPaths")
        self.files = set()
        fl = obj.get("Files", [])
        if not isinstance(fl, list):
            raise AcceptError("MALFORMED_JSON", "closure.Files")
        for x in fl:
            if not (isinstance(x, dict) and isinstance(x.get("Name"), str) and is_hex(x.get("Sha256"), 64)):
                raise AcceptError("MALFORMED_JSON", "closure.Files")
            self.files.add((x["Name"], x["Sha256"]))
        self.revpath_paths = set(p for _r, p in self.revpaths)
        self.allowed_objects = set(self.commits) | set(self.objects)
        for c in self.commits:
            if c in repo.commits():
                tree = repo.commits()[c]["tree"]
                if tree:
                    self.allowed_objects.add(tree)
        for r, p in self.revpaths:
            if is_hex(r, 40) and r in repo.commits():
                o = repo.oid_at(r, p)
                if o:
                    self.allowed_objects.add(o)


def _split_rev(tok):
    peel = None
    if tok.endswith("}"):
        k = tok.rfind("^{")
        if k < 0:
            return None
        peel = tok[k + 2:-1]
        tok = tok[:k]
    cut = len(tok)
    for ch in "^~":
        k = tok.find(ch)
        if 0 <= k < cut:
            cut = k
    base, rest = tok[:cut], tok[cut:]
    steps = []
    while rest:
        op = rest[0]
        if op not in "^~":
            return None
        m = 1
        while m < len(rest) and rest[m] in DIGITS:
            m += 1
        steps.append((op, int(rest[1:m]) if m > 1 else 1))
        rest = rest[m:]
    return base, steps, peel


def _fit_rev(tok, cl, repo):
    if tok in cl.refs:
        return None
    for sep in ("...", ".."):
        if sep in tok:
            a, b = tok.split(sep, 1)
            for side in (a or "HEAD", b or "HEAD"):
                why = _fit_rev(side, cl, repo)
                if why:
                    return why
            return None
    if tok.startswith("^"):
        return _fit_rev(tok[1:], cl, repo)
    if ":" in tok:
        r, p = tok.split(":", 1)
        return None if (r, p) in cl.revpaths else "rev:path outside the closure: " + tok
    parsed = _split_rev(tok)
    if parsed is None:
        return "unrecognized revision: " + tok
    base, steps, _peel = parsed
    if base in cl.refs:
        return None if not steps else "relative revision of a symbolic ref: " + tok
    if not is_hex(base, 40):
        return "revision outside the closure: " + tok
    if not steps:
        return None if base in cl.allowed_objects else "object outside the closure: " + tok
    try:
        c = base
        for op, n in steps:
            if op == "^":
                if n == 0:
                    continue
                ps = repo.parents(c)
                if n > len(ps):
                    return "revision does not resolve: " + tok
                c = ps[n - 1]
            else:
                for _ in range(n):
                    ps = repo.parents(c)
                    if not ps:
                        return "revision does not resolve: " + tok
                    c = ps[0]
    except AcceptError:
        return "revision does not resolve: " + tok
    return None if c in cl.commits else "commit outside the closure: " + tok


def fit_gitcall(argv, cl, repo, repo_path):
    """Violations of one tool GitCalls entry against the declared closure ([] = fits)."""
    if not isinstance(argv, list) or not argv or not all(isinstance(a, str) for a in argv):
        return ["malformed GitCalls entry"]
    v = []
    i = 0
    while i < len(argv) and argv[i].startswith("-"):
        a = argv[i]
        if a == "-c":
            if i + 1 >= len(argv) or argv[i + 1] not in TOOL_GIT_CONFIG:
                v.append("git -c option outside SPEC 1.2")
            i += 2
            continue
        if a == "-C":
            if i + 1 >= len(argv) or not same_path(argv[i + 1], repo_path):
                v.append("git -C outside the declared repository")
            i += 2
            continue
        if a not in TOOL_GIT_GLOBAL_FLAGS:
            v.append("git global option not allowed: " + a)
        i += 1
    if i >= len(argv):
        return v + ["no git subcommand"]
    sub, rest = argv[i], argv[i + 1:]
    if sub not in TOOL_GIT_SUBCOMMANDS:
        return v + ["git subcommand outside the tool list: " + sub]
    pos, after = [], []
    dd = False
    j = 0
    while j < len(rest):
        a = rest[j]
        if dd:
            after.append(a)
        elif a == "--":
            dd = True
        elif a.startswith("-"):
            if a.startswith("--batch") or a.startswith("--stdin"):
                v.append("stdin-driven read not declared in GitCalls: " + a)
            if a in ("-w", "--write", "--ext-diff", "--textconv") or a.startswith("--output"):
                v.append("forbidden git flag: " + a)
            if a in ("-n", "--max-count"):
                j += 1
        else:
            pos.append(a)
        j += 1
    if sub in ("diff", "show") and not ("--no-ext-diff" in rest and "--no-textconv" in rest):
        v.append(sub + " without --no-ext-diff --no-textconv")
    if sub == "status" and not any(a.startswith("--porcelain") for a in rest):
        v.append("status without --porcelain")
    if sub in ("check-ignore", "status"):
        revs, pathlike = [], pos
    elif sub == "ls-tree":
        revs, pathlike = pos[:1], pos[1:]
    elif sub == "cat-file" and len(pos) >= 2 and pos[0] in ("blob", "tree", "commit", "tag"):
        revs, pathlike = pos[1:], []
    else:
        revs, pathlike = pos, []
    # a token with pathspec magic in a revision position is a pathspec, not a revision
    pathlike = pathlike + [r for r in revs if r.startswith(":(")]
    revs = [r for r in revs if not r.startswith(":(")]
    for p in pathlike + after:
        path, why = pathspec_path(p)
        if why:
            v.append(why)
        elif path not in cl.paths and path not in cl.revpath_paths:
            v.append("path outside the closure: " + path)
    for r in revs:
        why = _fit_rev(r, cl, repo)
        if why:
            v.append(why)
    return v


LITERAL_MAGIC = ":(literal)"


def pathspec_path(token):
    """Errata E3: ':(literal)<path>' is normalized to <path>; any other pathspec magic is a violation."""
    if token.startswith(LITERAL_MAGIC):
        return token[len(LITERAL_MAGIC):], None
    if token.startswith(":"):
        return None, "pathspec magic not allowed: " + token
    return token, None


def check_r9(ctx):
    spec = ctx.req.get("Closure")
    if spec is None:
        return None
    if not isinstance(spec, dict):
        raise AcceptError("MISSING_INPUT", "Closure")
    if spec.get("Sha256") is not None:
        data = read_pinned(spec, "Closure")
    else:
        data = read_input_file(spec.get("Path"), "Closure.Path")
    cl = Closure(parse_json_bytes(data, "closure"), ctx.repo)
    if not ctx.runs:
        raise AcceptError("MISSING_INPUT", "no tool envelope to check against the closure")
    out = []
    ok_all = True
    for run in ctx.runs:
        d = {"Name": run.name}
        viol = []
        if run.env is None:
            viol.append("envelope unusable")
        else:
            calls = run.env.get("GitCalls")
            inputs = run.env.get("Inputs")
            if not isinstance(calls, list) or not isinstance(inputs, list):
                viol.append("GitCalls / Inputs unusable")
            else:
                for k, call in enumerate(calls):
                    for x in fit_gitcall(call, cl, ctx.repo, ctx.req.get("Repo")):
                        viol.append("GitCalls[" + str(k) + "]: " + x)
                declared_files = set()
                for k, inp in enumerate(inputs):
                    kind = dig(inp, "Kind")
                    if kind == "commit":
                        if dig(inp, "Sha") not in cl.commits:
                            viol.append("Inputs[" + str(k) + "]: commit outside the closure")
                    elif kind == "git-blob":
                        rev, path, blob = dig(inp, "Rev"), dig(inp, "Path"), dig(inp, "Blob")
                        if (rev, path) not in cl.revpaths:
                            viol.append("Inputs[" + str(k) + "]: rev:path outside the closure")
                        else:
                            try:
                                real = ctx.repo.oid_at(rev, path)
                            except AcceptError:
                                real = _MISSING
                            if real is _MISSING or canonical(real) != canonical(blob):
                                viol.append("Inputs[" + str(k) + "]: Blob differs from the object in the repository")
                    elif kind == "file":
                        name, sha = dig(inp, "Name"), dig(inp, "Sha256")
                        if (name, sha) not in cl.files:
                            viol.append("Inputs[" + str(k) + "]: file outside the closure")
                        declared_files.add((name, sha))
                    else:
                        viol.append("Inputs[" + str(k) + "]: unknown Kind")
                if run.argv is not None and run.cmd in COMPARED_COMMANDS and run.argv_error is None:
                    for name, path in run.files.items():
                        want = run.shas.get(name)
                        try:
                            real = sha256_hex(read_input_file(path, name))
                        except AcceptError:
                            real = None
                        if real is None or real != want:
                            viol.append("argv file " + name + ": SHA-256 differs from --sha256")
                        if (name, want) not in declared_files:
                            viol.append("argv file " + name + ": not declared in Inputs")
        d["Violations"] = viol
        ok_all = ok_all and not viol
        out.append(d)
    return _status(ok_all), {"Runs": out}


def check_r10(ctx):
    spec = ctx.req.get("RelayFacts")
    if spec is None:
        return None
    if not isinstance(spec, dict) or not is_hex(spec.get("CitedSha256"), 64):
        raise AcceptError("MISSING_INPUT", "RelayFacts.CitedSha256")
    got = sha256_hex(read_input_file(spec.get("Path"), "RelayFacts.Path"))
    return _status(got == spec["CitedSha256"]), {"Cited": spec["CitedSha256"], "Computed": got}


def check_r8(_ctx):
    return "MANUAL", {"Reason": "free text of the verification: manual control of the Coordinator (AP 16.10)"}


# --------------------------------------------------------------------------- output hygiene

def _redact_str(s):
    out = s
    prof = os.environ.get("USERPROFILE") or ""
    if len(prof) > 3:
        for form in (prof, prof.replace("\\", "/")):
            low, lf = out.lower(), form.lower()
            k = low.find(lf)
            while k >= 0:
                out = out[:k] + "%USERPROFILE%" + out[k + len(form):]
                low = out.lower()
                k = low.find(lf, k + len("%USERPROFILE%"))
    i = 0
    while True:
        low = out.lower()
        k = low.find("users", i)
        if k < 0:
            break
        j = k + 5
        drive_root = k >= 3 and out[k - 1] in "\\/" and out[k - 2] == ":" and out[k - 3] in ASCII_ALNUM
        if j < len(out) and out[j] in "\\/" and drive_root:
            m = j + 1
            while m < len(out) and out[m] not in "\\/\"' ":
                m += 1
            if m > j + 1 and out[j + 1:m] != "%USERNAME%":
                out = out[:j + 1] + "%USERNAME%" + out[m:]
            i = j + 1 + len("%USERNAME%")
        else:
            i = k + 5
    return out.replace("EXECUTION_", "EXEC-REDACTED_")


def _hygiene(obj):
    if isinstance(obj, dict):
        return dict((_redact_str(k), _hygiene(v)) for k, v in obj.items())
    if isinstance(obj, list):
        return [_hygiene(x) for x in obj]
    if isinstance(obj, str):
        return _redact_str(obj)
    return obj


# --------------------------------------------------------------------------- driver

_CHECK_FUNCS = (("R1", check_r1), ("R3", check_r3), ("R2", check_r2), ("R4", check_r4), ("R5", check_r5),
                ("R6", check_r6), ("R7", check_r7), ("R9", check_r9), ("R10", check_r10), ("R8", check_r8))


def accept(req):
    ctx = Ctx(req)
    results = {}
    for rid, fn in _CHECK_FUNCS:
        na = ctx.na.get(rid)
        try:
            res = fn(ctx)
        except AcceptError as e:
            res = ("MISMATCH", {"Error": e.code, "Detail": e.detail})
        except Exception as e:  # fail closed on anything unexpected
            res = ("MISMATCH", {"Error": "INTERNAL", "Type": type(e).__name__})
        if res is None:
            if na is not None and rid in WAIVABLE:
                res = ("NOT_APPLICABLE", {"Reason": str(na)})
            elif na is not None:
                res = ("MISMATCH", {"Missing": "no data", "Problem": "this check cannot be declared not applicable"})
            else:
                res = ("MISMATCH", {"Missing": "no data for this check and no NotApplicable declaration"})
        elif na is not None and res[0] != "MANUAL":
            res = ("MISMATCH", {"Problem": "declared not applicable but data was supplied", "Result": res[1]})
        results[rid] = res
    checks = [{"Id": rid, "Status": results[rid][0], "Detail": results[rid][1]} for rid in CHECK_IDS]
    decision = "ACCEPT_FACTS" if all(c["Status"] in ("MATCH", "NOT_APPLICABLE", "MANUAL") for c in checks) \
        else "REJECT_FACTS"
    return _hygiene({"Schema": OUT_SCHEMA, "Checks": checks, "Decision": decision})


_CLI_SCALARS = {
    "--repo": ("Repo",),
    "--tool": ("Tool", "Path"),
    "--tool-blob": ("Tool", "PinnedBlob"),
    "--tool-git-dir": ("Tool", "GitObject", "GitDir"),
    "--tool-git-blob": ("Tool", "GitObject", "Blob"),
    "--run-from": ("Tool", "RunFrom"),
    "--python-version": ("Interpreter", "Python"),
    "--python-exe-sha256": ("Interpreter", "ExecutableSha256"),
    "--closure": ("Closure", "Path"),
    "--closure-sha256": ("Closure", "Sha256"),
    "--relay-facts": ("RelayFacts", "Path"),
    "--relay-sha256": ("RelayFacts", "CitedSha256"),
}


def _merge(base, extra, where="request"):
    out = dict(base)
    for k, v in extra.items():
        if k in out:
            if isinstance(out[k], dict) and isinstance(v, dict):
                out[k] = _merge(out[k], v, where + "." + k)
            elif canonical(out[k]) != canonical(v):
                raise AcceptError("USAGE", "conflicting values for " + where + "." + k)
        else:
            out[k] = v
    return out


def build_request(argv):
    argv = list(argv)
    tail = None
    if "--" in argv:
        k = argv.index("--")
        tail, argv = argv[k + 1:], argv[:k]
    cli = {}
    request_path = request_sha = envelope = None
    na = []
    i = 0
    while i < len(argv):
        name, eq, val = argv[i].partition("=")
        if not name.startswith("--"):
            raise AcceptError("USAGE", "unexpected argument")
        if not eq:
            if i + 1 >= len(argv):
                raise AcceptError("USAGE", "missing value for " + name)
            val = argv[i + 1]
            i += 2
        else:
            i += 1
        if name == "--request":
            request_path = val
        elif name == "--request-sha256":
            request_sha = val
        elif name == "--envelope":
            envelope = val
        elif name == "--na":
            na.append(val)
        elif name in _CLI_SCALARS:
            node = cli
            keys = _CLI_SCALARS[name]
            for key in keys[:-1]:
                node = node.setdefault(key, {})
            if keys[-1] in node:
                raise AcceptError("USAGE", "repeated option " + name)
            node[keys[-1]] = val
        else:
            raise AcceptError("USAGE", "unknown option " + name)
    base = {}
    if request_path is not None:
        data = read_input_file(request_path, "request")
        if request_sha is not None and sha256_hex(data) != request_sha:
            raise AcceptError("INPUT_HASH_MISMATCH", "request")
        base = parse_json_bytes(data, "request")
        if not isinstance(base, dict) or base.get("Schema") != REQUEST_SCHEMA:
            raise AcceptError("USAGE", "request Schema")
    elif request_sha is not None:
        raise AcceptError("USAGE", "--request-sha256 without --request")
    req = _merge(base, cli)
    if envelope is not None:
        if not tail:
            raise AcceptError("USAGE", "--envelope needs the tool argv after --")
        runs = list(req.get("Runs") or [])
        runs.append({"Name": "cli", "Envelope": envelope, "Argv": tail})
        req["Runs"] = runs
    elif tail:
        raise AcceptError("USAGE", "tool argv given without --envelope")
    for item in na:
        rid, _, reason = item.partition("=")
        if rid not in CHECK_IDS:
            raise AcceptError("USAGE", "--na " + rid)
        nad = dict(req.get("NotApplicable") or {})
        nad[rid] = reason or "declared"
        req["NotApplicable"] = nad
    if not req:
        raise AcceptError("USAGE", "empty request")
    return req


def main(argv):
    try:
        req = build_request(argv)
        result = accept(req)
        rc = 0 if result["Decision"] == "ACCEPT_FACTS" else 1
    except AcceptError as e:
        result = _hygiene({"Schema": OUT_SCHEMA, "Checks": [], "Decision": "REJECT_FACTS",
                           "Error": {"Code": e.code, "Detail": e.detail}})
        rc = 64
    sys.stdout.buffer.write((canonical(result) + "\n").encode("ascii"))
    sys.stdout.buffer.flush()
    return rc


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
