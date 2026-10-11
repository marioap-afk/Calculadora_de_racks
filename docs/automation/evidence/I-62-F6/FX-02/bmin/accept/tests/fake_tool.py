#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""fake_tool.py - TEST SCAFFOLDING ONLY. Minimal stand-in for bmin_facts.py.

Emits SPEC 1.5 envelopes for the commands the acceptor recomputes, so that R1, R3,
R5, R7 and R9 can be exercised without the real tool (written in parallel by another
author). For identity / diff / scope / hashes it uses the git primitives the SPEC
prescribes for the tool (rev-parse, merge-base --is-ancestor, git diff --name-only,
rev-list --reverse, %(trailers)), which also cross-checks the acceptor's independent
primitives. For clausemap it imports the acceptor (path in FAKE_TOOL_ACCEPT_PATH):
there it is NOT independent and only serves the plumbing and mutant tests.

Follows SPEC v0.2: --no-renames (E6), U19Normalized objects (E2), ':(literal)' pathspecs (E3),
AMBIGUOUS for an ambiguous Derive or distinct claim_id values (E4).
Mutants (env FAKE_TOOL_MUTANT, comma separated): drop-path, ancestry-swap,
coverage-inverted, u19-off, u19-legacy, mv4-merged, pointer-line, sentence-boundary,
extra-gitcall, verdict, trailer-first, renames-on, e4-off, glob-pathspec.
Does not depend on __file__ (works as `git cat-file blob X | python -I - ...`).
"""

import hashlib
import json
import os
import platform
import subprocess
import sys

SCHEMA = "rackcad-bmin-facts/0-experimental"
ALLOWED = frozenset(["rev-parse", "cat-file", "ls-tree", "rev-list", "diff", "log", "show", "merge-base",
                     "status", "check-ignore"])
MUTANTS = frozenset(x for x in os.environ.get("FAKE_TOOL_MUTANT", "").split(",") if x)


class ToolError(Exception):
    def __init__(self, code, detail=""):
        Exception.__init__(self, code)
        self.code = code
        self.detail = detail


def canonical(obj):
    return json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))


class State(object):
    def __init__(self):
        self.calls = []
        self.inputs = []
        self.repo = None


ST = State()


def env():
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    e.update({"GIT_OPTIONAL_LOCKS": "0", "GIT_TERMINAL_PROMPT": "0", "LC_ALL": "C"})
    return e


def git(sub, args, ok=(0,)):
    if sub not in ALLOWED:
        raise ToolError("FORBIDDEN_OPERATION", sub)
    argv = ["-C", ST.repo, "-c", "core.quotepath=false", "-c", "core.fsmonitor=false", sub] + list(args)
    ST.calls.append(argv)
    p = subprocess.run(["git"] + argv, stdin=subprocess.DEVNULL, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                       env=env(), shell=False)
    if p.returncode not in ok:
        raise ToolError("GIT_ERROR", sub + " rc=" + str(p.returncode))
    return p.returncode, p.stdout


def commits(*shas):
    """Validates full SHAs as existing commits with a single rev-parse call."""
    for sha in shas:
        if not (isinstance(sha, str) and len(sha) == 40 and all(c in "0123456789abcdef" for c in sha)):
            raise ToolError("BAD_REVISION", "sha")
    rc, out = git("rev-parse", [s + "^{commit}" for s in shas], ok=(0, 128))
    if rc != 0 or out.decode().split() != list(shas):
        raise ToolError("BAD_REVISION", "not a commit")
    for sha in shas:
        ST.inputs.append({"Kind": "commit", "Sha": sha})
    return shas


def read_file_input(files, shas, name):
    if name not in files or name not in shas:
        raise ToolError("MISSING_INPUT", name)
    try:
        with open(files[name], "rb") as fh:
            data = fh.read()
    except OSError:
        raise ToolError("MISSING_INPUT", name)
    if hashlib.sha256(data).hexdigest() != shas[name]:
        raise ToolError("INPUT_HASH_MISMATCH", name)
    ST.inputs.append({"Kind": "file", "Name": name, "Sha256": shas[name]})
    try:
        return json.loads(data.decode("utf-8"))
    except ValueError:
        raise ToolError("MALFORMED_JSON", name)


def parse(argv):
    if not argv:
        raise ToolError("USAGE", "no command")
    opts, files, shas = {}, {}, {}
    i = 1
    while i < len(argv):
        name = argv[i]
        if name == "--paths":
            vals = []
            i += 1
            while i < len(argv) and not argv[i].startswith("--"):
                vals.append(argv[i])
                i += 1
            opts["--paths"] = vals
            continue
        if i + 1 >= len(argv):
            raise ToolError("USAGE", name)
        val = argv[i + 1]
        if name == "--file":
            n, _, p = val.partition("=")
            files[n] = p
        elif name == "--sha256":
            n, _, h = val.partition("=")
            shas[n] = h
        else:
            opts[name] = val
        i += 2
    return argv[0], opts, files, shas


def diff_paths(a, b):
    renames = [] if "renames-on" in MUTANTS else ["--no-renames"]
    _, out = git("diff", ["--no-ext-diff", "--no-textconv", "--name-only"] + renames + ["-z", a + ".." + b])
    paths = [p.decode("utf-8") for p in out.split(b"\0") if p]
    if "drop-path" in MUTANTS and paths:
        paths = paths[:-1]
    return paths


def is_anc(a, b):
    rc, _ = git("merge-base", ["--is-ancestor", a, b], ok=(0, 1))
    return rc == 0


def cmd_identity(o, files, shas):
    branch = o["--branch"]
    red = o.get("--red")
    base, cur = o["--base"], o["--current"]
    commits(*([base, cur] + ([red] if red else [])))
    _, hi = git("rev-parse", ["--show-toplevel", "HEAD", "--abbrev-ref", "HEAD"])
    top, head, ab = hi.decode("utf-8").split("\n")[:3]
    rc, ob = git("rev-parse", ["--verify", "--quiet", "refs/remotes/origin/" + branch], ok=(0, 1))
    rc2, om = git("rev-parse", ["--verify", "--quiet", "refs/remotes/origin/main"], ok=(0, 1))
    anc = {"base->current": is_anc(cur, base) if "ancestry-swap" in MUTANTS else is_anc(base, cur)}
    if red is not None:
        anc["base->red"] = is_anc(base, red)
        anc["red->current"] = is_anc(red, cur)
    return {"Head": head, "AbbrevRef": ab, "Toplevel": top,
            "OriginBranch": ob.decode().strip() if rc == 0 else None,
            "OriginMain": om.decode().strip() if rc2 == 0 else None, "Ancestry": anc}


def cmd_diff(o, files, shas):
    a, b = commits(o["--from"], o["--to"])
    paths = diff_paths(a, b)
    _, out = git("log", ["--reverse", "--format=%H%x00%(trailers:key=Co-Authored-By,valueonly)%x01", a + ".." + b])
    order, co = [], {}
    for rec in out.decode("utf-8").split("\x01"):
        rec = rec.lstrip("\n")
        if not rec:
            continue
        c, _, t = rec.partition("\x00")
        vals = [x.strip() for x in t.split("\n") if x.strip()]
        order.append(c)
        co[c] = vals[:1] if "trailer-first" in MUTANTS else vals
    return {"Paths": paths, "Commits": order, "CoAuthoredBy": co}


def u19(e):
    if "u19-off" in MUTANTS:
        return e
    return e[:-2] if e.endswith("/**") else e


def covers(e, p):
    if "coverage-inverted" in MUTANTS:
        return u19(e).startswith(p)
    e = u19(e)
    return p == e or (e.endswith("/") and p.startswith(e))


def malformed(e):
    core = e[:-3] if e.endswith("/**") else e
    return (e.startswith("/") or "\\" in e or ".." in e.split("/") or any(c in core for c in "*?[")
            or (len(e) > 1 and e[1] == ":"))


def cmd_scope(o, files, shas):
    a, b = commits(o["--from"], o["--to"])
    d = read_file_input(files, shas, "DELEG")
    allowed, forbidden = d["AllowedWriteScope"], d["ForbiddenWriteScope"]
    paths = diff_paths(a, b)
    return {"Paths": paths,
            "PerPath": [{"Path": p, "AllowedCoveredBy": [e for e in allowed if covers(e, p)],
                         "ForbiddenCoveredBy": [e for e in forbidden if covers(e, p)]} for p in paths],
            "AllowedEntries": allowed, "ForbiddenEntries": forbidden,
            "MalformedEntries": [e for e in allowed + forbidden if malformed(e)],
            "U19Normalized": u19_list(allowed, forbidden)}


def u19_list(allowed, forbidden):
    if "u19-off" in MUTANTS:
        return []
    if "u19-legacy" in MUTANTS:
        return [e for e in allowed + forbidden if e.endswith("/**")]
    return ([{"Entry": e, "List": "AllowedWriteScope", "Prefix": e[:-2]} for e in allowed if e.endswith("/**")]
            + [{"Entry": e, "List": "ForbiddenWriteScope", "Prefix": e[:-2]} for e in forbidden if e.endswith("/**")])


def cmd_hashes(o, files, shas):
    rev = commits(o["--rev"])[0]
    _, listing = git("ls-tree", ["-z", rev] + [":(literal)" + p for p in o["--paths"]])
    if "glob-pathspec" in MUTANTS:  # recorded, not executed: exercises R9 only
        ST.calls.append(["-C", ST.repo, "-c", "core.quotepath=false", "-c", "core.fsmonitor=false", "ls-tree",
                         "-z", rev, ":(glob)" + o["--paths"][0]])
    blobs = {}
    for rec in listing.split(b"\0"):
        if rec:
            meta, _, path = rec.partition(b"\t")
            parts = meta.split(b" ")
            if parts[1] == b"blob":
                blobs[path.decode("utf-8")] = parts[2].decode()
    per = []
    for p in o["--paths"]:
        fs = os.path.join(ST.repo, *p.split("/"))
        w1 = w256 = None
        if os.path.isfile(fs):
            with open(fs, "rb") as fh:
                data = fh.read()
            w1 = hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()
            w256 = hashlib.sha256(data).hexdigest()
        rb = blobs.get(p)
        rs = None
        if rb:
            _, content = git("cat-file", ["blob", rb])
            rs = hashlib.sha256(content).hexdigest()
        ST.inputs.append({"Kind": "git-blob", "Rev": rev, "Path": p, "Blob": rb})
        per.append({"Path": p, "WorktreeRawBlobSha1": w1, "WorktreeSha256": w256, "RevBlob": rb, "RevBlobSha256": rs})
    return {"PerPath": per}


def cmd_contract(o, files, shas):
    c = read_file_input(files, shas, "CONTRACT")
    d = read_file_input(files, shas, "DELEG")

    def cont(a, b):
        return covers(b, u19(a))

    return {"A3": [{"Entry": a, "ContainedBy": [b for b in c["AllowedWriteScope"] if cont(a, b)]}
                   for a in d["AllowedWriteScope"]],
            "A4": [{"Entry": a, "ContainedBy": [b for b in d["ForbiddenWriteScope"] if cont(a, b)]}
                   for a in c["ForbiddenWriteScope"]],
            "A5Missing": {"Invariants": [], "StopConditions": [], "Authorities": [], "RequiredTests": [],
                          "RoleRequirements": []},
            "RequiredTestsPairs": [], "SupersededEqual": True}


def _load_acceptor():
    import importlib.util
    sys.dont_write_bytecode = True
    path = os.environ.get("FAKE_TOOL_ACCEPT_PATH")
    if not path:
        raise ToolError("USAGE", "FAKE_TOOL_ACCEPT_PATH")
    spec = importlib.util.spec_from_file_location("bmin_accept_for_fake", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    original = mod.git_run

    def recording(where, sub, args, stdin_bytes=b"", ok=(0,)):
        ST.calls.append(["-C", where, "-c", "core.quotepath=false", "-c", "core.fsmonitor=false", sub]
                        + [str(a) for a in args])
        return original(where, sub, args, stdin_bytes=stdin_bytes, ok=ok)

    mod.git_run = recording
    return mod


def cmd_clausemap(o, files, shas):
    ba = _load_acceptor()
    s = commits(o["--main"])[0]
    try:
        facts, _internal = ba.acceptor_clausemap(ba.RepoView(ST.repo), s)
        if "--state-rev" in o:
            facts["Classify"] = ba.acceptor_classify(ba.RepoView(ST.repo), commits(o["--state-rev"])[0],
                                                     o["--state-path"])
            facts["Classify"]["ClaimIdInPre"] = None
    except ba.AcceptError as e:
        raise ToolError(e.code, e.detail)
    if "e4-off" not in MUTANTS:
        if ba.derive_is_ambiguous(facts["Derive"]):
            raise ToolError("AMBIGUOUS", "trailer duplicated or without a qualifying merge")
        if "Classify" in facts and len(set(facts["Classify"]["ClaimIdLines"])) >= 2:
            raise ToolError("AMBIGUOUS", "two or more distinct claim_id values")
    facts["Pre"] = None
    v = facts["Validate"]
    if v is not None:
        if "mv4-merged" in MUTANTS and v["MV4"] is not None:
            v["MV4"]["LiteralViolations"] = v["MV4"]["LiteralViolations"] + v["MV4"]["AddedPresentInEff1"]
            v["MV4"]["AddedPresentInEff1"] = []
        if "pointer-line" in MUTANTS or "sentence-boundary" in MUTANTS:
            repo = ba.RepoView(ST.repo)
            ap = repo.text_at(facts["Derive"]["Eff"], ba.AP_PATH)
            doc = ba.MdDoc(ap)
            if "pointer-line" in MUTANTS:
                k = doc.find_unique_heading(3, ba.S163_LABEL)
                lines = [ln for ln in doc.body_text(k).split("\n") if ba.POINTER not in ln]
                v["MV7"]["S163EffMinusPointer"] = ba.norm_ws("\n".join(lines))
                v["MV7"]["S163EqualAfterPointerRemoval"] = v["MV7"]["S163EffMinusPointer"] == v["MV7"]["S163Eff1"]
            if "sentence-boundary" in MUTANTS:
                k = doc.find_unique_heading(2, ba.S16_LABEL)
                body = ba.norm_ws(doc.body_text(k))
                cut = body.find(".")
                first = body[:cut + 1] if cut >= 0 else body
                v["MV7"]["FirstSentenceS16"] = first
                v["MV7"]["PointerIsFirstSentenceS16"] = first == ba.POINTER
    return facts


COMMANDS = {"identity": cmd_identity, "diff": cmd_diff, "scope": cmd_scope, "hashes": cmd_hashes,
            "contract": cmd_contract, "clausemap": cmd_clausemap}


def main(argv):
    out = {"Schema": SCHEMA, "Command": argv[0] if argv else None, "Args": {}, "Inputs": [], "GitCalls": [],
           "Facts": None, "FactsSha256": None, "Error": None,
           "Runtime": {"Python": platform.python_version(), "Isolated": sys.flags.isolated == 1}}
    rc = 0
    try:
        if sys.flags.isolated != 1:
            raise ToolError("NOT_ISOLATED", "run with python -I")
        cmd, opts, files, shas = parse(argv)
        out["Args"] = dict((k, v) for k, v in sorted(opts.items()))
        if files:
            out["Args"]["--file"] = files
        if cmd not in COMMANDS:
            raise ToolError("USAGE", "command")
        ST.repo = opts.get("--repo")
        if not ST.repo:
            raise ToolError("USAGE", "--repo")
        facts = COMMANDS[cmd](opts, files, shas)
        if "verdict" in MUTANTS:
            facts["Result"] = "pass"
        out["Facts"] = facts
        out["FactsSha256"] = hashlib.sha256(canonical(facts).encode("ascii")).hexdigest()
    except ToolError as e:
        out["Error"] = {"Code": e.code, "Detail": e.detail}
        out["Facts"] = None
        out["FactsSha256"] = None
        rc = 64 if e.code == "USAGE" else 2
    calls = list(ST.calls)
    if "extra-gitcall" in MUTANTS:
        calls.append(["config", "--list"])
    out["GitCalls"] = calls
    out["Inputs"] = ST.inputs
    sys.stdout.buffer.write((canonical(out) + "\n").encode("ascii"))
    sys.stdout.buffer.flush()
    return rc


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
