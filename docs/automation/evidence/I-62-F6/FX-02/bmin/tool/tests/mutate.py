#!/usr/bin/env python3
"""Mutation runner for bmin_facts.py (SPEC section 4, "Mutacion").

Applies a fixed list of textual mutants, one at a time, to a COPY of the tool in
a temporary directory under BMIN_TMP (default D:\\r62-bmin-tmp\\tool) and runs
the unittest suite against the copy (BMIN_TOOL=<copy>). A mutant is KILLED when
the suite fails (or times out) and SURVIVED when the full suite passes.

Order of work per mutant (ordering only, never a shortcut for survivors):
  1. the mutant's hint tests (fast); a failure there kills it;
  2. otherwise the full suite with failfast; it survives only if that passes.

Each anchor must occur exactly once in the original source and the mutated
source must compile; otherwise the mutant is reported INVALID.

Usage: python mutate.py [--workers N] [--only M01,M02] [--no-baseline]
"""

import concurrent.futures
import json
import os
import shutil
import stat
import subprocess
import sys
import tempfile
import time

HERE = os.path.dirname(os.path.abspath(__file__))
TOOL = os.path.abspath(os.path.join(HERE, "..", "bmin_facts.py"))
TMP_ROOT = os.environ.get("BMIN_TMP") or "D:\\r62-bmin-tmp\\tool"
TIMEOUT = 3600

MUTANTS = [
    ("M01", "coverage rule inverted (E starts with R)",
     r'return path == entry or (entry.endswith("/") and path.startswith(entry))',
     r'return path == entry or (entry.endswith("/") and entry.startswith(path))',
     ["TestScope.test_scope_coverage", "TestContract.test_contract_facts"]),
    ("M02", "sentence boundary off ('.' without following whitespace splits)",
     r'SENTENCE_RE = re.compile(r"(?<=\.)\s+")',
     r'SENTENCE_RE = re.compile(r"(?<=\.)\s*")',
     ["TestClauseMapGood.test_validate_mv7_pointer_shares_line"]),
    ("M03", "ancestry argument order swapped",
     r'ancestry = {"base->current": is_ancestor(ctx, base, current)}',
     r'ancestry = {"base->current": is_ancestor(ctx, current, base)}',
     ["TestIdentity"]),
    ("M04", "one diff path dropped",
     r'return [git_text(x) for x in out.split(b"\0") if x]',
     r'return [git_text(x) for x in out.split(b"\0") if x][:-1]',
     ["TestDiff"]),
    ("M05", "MV-4 lists merged (R61 b list folded into the literal list)",
     "            if kind == \"ADDED\" and o1 is not None:\n                added_in_eff1.append(p)\n",
     "            if kind == \"ADDED\" and o1 is not None:\n                added_in_eff1.append(p)\n"
     "                literal.append(p)\n",
     ["TestClauseMapDefect.test_mv4_four_lists"]),
    ("M06", "pointer removed together with its whole line",
     "        idx = [i for i, s in enumerate(sents) if s == POINTER]\n"
     "        if idx:\n"
     "            del sents[idx[-1]]\n"
     "        minus = \" \".join(sents)\n",
     "        kept = [ln for ln in ap_md.body_text(s163[0]).split(\"\\n\") if POINTER not in ln]\n"
     "        minus = norm(\"\\n\".join(kept))\n",
     ["TestClauseMapGood.test_validate_mv7_pointer_shares_line"]),
    ("M07", "ENTRY file treated as ADDED (R61 b list)",
     r'if kind == "ADDED" and o1 is not None:',
     r'if kind in ("ADDED", "ENTRY") and o1 is not None:',
     ["TestClauseMapDefect.test_mv4_four_lists"]),
    ("M08", "first-parent merge order reversed",
     r'merges = list(reversed(git_lines(out)))',
     r'merges = git_lines(out)',
     ["TestClauseMapGood.test_derive"]),
    ("M09", "trailer matched by substring instead of exact line",
     r'if TRAILER in body.split("\n"):',
     r'if TRAILER in body:',
     ["TestClauseMapGood.test_derive"]),
    ("M10", "ensure_ascii=False in the canonical serialization",
     r'json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))',
     r'json.dumps(obj, ensure_ascii=False, sort_keys=True, separators=(",", ":"))',
     ["TestReadText.test_non_ascii_fidelity"]),
    ("M11", "missing -I check removed",
     "    if not isolated:\n",
     "    if False:\n",
     ["TestEnvelopeAndRuntime.test_not_isolated_is_refused"]),
    ("M12", "U-19 normalization removed",
     "        return entry[:-2]\n",
     "        return entry\n",
     ["TestScope.test_scope_coverage"]),
    ("M13", "raw blob header without the NUL separator",
     r'b"blob " + str(len(data)).encode("ascii") + b"\0" + data',
     r'b"blob " + str(len(data)).encode("ascii") + data',
     ["TestHashesAndCleanTree.test_hashes"]),
    ("M14", "free-text whole-word boundary removed",
     r'if not word_char(before) and not word_char(after):',
     r'if True:',
     ["TestFreeText.test_terms_and_matches"]),
    ("M15", "closed list of git subcommands removed",
     r'if not args or args[0] not in GIT_SUBCOMMANDS:',
     r'if not args:',
     ["TestRunnerUnit", "TestNegativeFailClosed.test_forbidden_git_subcommand"]),
    ("M16", "duplicated trailer accepted",
     r'if len(trailer_commits) > 1:',
     r'if len(trailer_commits) > 99:',
     ["TestNegativeFailClosed.test_duplicate_trailer"]),
    ("M17", "unsupported schema keyword accepted",
     "        if key not in SCHEMA_KEYWORDS:\n",
     "        if False:\n",
     ["TestNegativeFailClosed.test_unsupported_schema_keyword"]),
    ("M18", "MV-3 deleted paths dropped from the changed set",
     r'for p in set(t1) | set(t2) if',
     r'for p in set(t2) if',
     ["TestClauseMapDefect.test_mv2_mv3"]),
    ("M19", "state with two distinct claim_id values accepted",
     r'if len(distinct) > 1:',
     r'if len(distinct) > 2:',
     ["TestNegativeFailClosed.test_state_with_two_claim_ids"]),
    ("M27", "claim_id lines counted instead of distinct values (real /v2 state rejected)",
     r'if len(distinct) > 1:',
     r'if len(values) > 1:',
     ["TestClauseMapGood.test_pre_and_classify"]),
    ("M20", "last sentence of 16.3 taken as the first",
     r'last163 = sents[-1] if sents else None',
     r'last163 = sents[0] if sents else None',
     ["TestClauseMapGood.test_validate_mv7_pointer_shares_line"]),
    ("M21", "EFF candidate without the first-parent condition",
     r'if len(ps) >= 2 and ps[1] in reach and ps[0] not in reach:',
     r'if len(ps) >= 2 and ps[1] in reach:',
     ["TestClauseMapGood.test_derive"]),
    ("M22", "preamble omitted from H1/H2",
     r'result = {PREAMBLE: [norm("\n".join(self.lines[:self.preamble_end()]))]}',
     r'result = {}',
     ["TestClauseMapDefect.test_mv6"]),
    ("M23", "status --porcelain no longer forced",
     "    elif sub == \"status\" and \"--porcelain\" not in head:\n        rest = [\"--porcelain\"] + rest\n",
     "",
     ["TestRunnerUnit.test_forced_flags_and_env"]),
    ("M24", "code fences ignored (lines inside ``` count as headings)",
     r'if line.startswith("```"):',
     r'if False:',
     ["TestClauseMapGood.test_validate_mv6"]),
    ("M25", "duplicate JSON keys accepted",
     "            if key in obj:\n",
     "            if False:\n",
     ["TestReadJson.test_malformed_and_duplicate_keys"]),
    ("M26", "--no-ext-diff --no-textconv no longer forced",
     r'rest = ["--no-ext-diff", "--no-textconv"] + rest',
     r'rest = rest',
     ["TestRunnerUnit.test_forced_flags_and_env"]),
]


def rmtree_force(path):
    def on_exc(func, p, exc):
        os.chmod(p, stat.S_IWRITE)
        func(p)
    if os.path.isdir(path):
        if sys.version_info >= (3, 12):
            shutil.rmtree(path, onexc=on_exc)
        else:
            shutil.rmtree(path, onerror=lambda f, p, e: on_exc(f, p, e))


def run_suite(tool_path, tests=None):
    env = dict(os.environ)
    env["BMIN_TOOL"] = tool_path
    env["BMIN_TMP"] = TMP_ROOT
    if tests:
        cmd = [sys.executable, "-m", "unittest", "-f"] + ["test_bmin_facts." + t for t in tests]
    else:
        cmd = [sys.executable, "-m", "unittest", "discover", "-f", "-s", HERE, "-p", "test_bmin_facts.py"]
    start = time.time()
    try:
        p = subprocess.run(cmd, cwd=HERE, env=env, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                           timeout=TIMEOUT)
    except subprocess.TimeoutExpired:
        return {"rc": None, "timeout": True, "first": None, "seconds": round(time.time() - start, 1)}
    text = p.stderr.decode("utf-8", "replace")
    first = None
    for line in text.splitlines():
        if line.startswith("FAIL: ") or line.startswith("ERROR: "):
            first = line
            break
    ran = None
    for line in text.splitlines():
        if line.startswith("Ran "):
            ran = line
    return {"rc": p.returncode, "timeout": False, "first": first, "ran": ran,
            "seconds": round(time.time() - start, 1)}


def run_mutant(src, mutant):
    mid, desc, anchor, repl, hints = mutant
    result = {"Id": mid, "Description": desc}
    count = src.count(anchor)
    if count != 1:
        result.update({"Status": "INVALID", "Detail": "anchor occurs %d times" % count})
        return result
    mutated = src.replace(anchor, repl)
    try:
        compile(mutated, "bmin_facts_mutant.py", "exec")
    except SyntaxError as exc:
        result.update({"Status": "INVALID", "Detail": "mutant does not compile: %s" % exc})
        return result
    work = tempfile.mkdtemp(prefix="mut-%s-" % mid, dir=TMP_ROOT)
    try:
        path = os.path.join(work, "bmin_facts.py")
        with open(path, "w", encoding="ascii", newline="\n") as fh:
            fh.write(mutated)
        stage = "hint tests"
        r = run_suite(path, hints)
        if r["rc"] == 0 and not r["timeout"]:
            stage = "full suite"
            r = run_suite(path, None)
        if r["timeout"]:
            result.update({"Status": "KILLED", "By": stage + " (timeout)", "Seconds": r["seconds"]})
        elif r["rc"] != 0:
            result.update({"Status": "KILLED", "By": "%s: %s" % (stage, r["first"]), "Seconds": r["seconds"]})
        else:
            result.update({"Status": "SURVIVED", "By": "full suite passed (%s)" % r.get("ran"),
                           "Seconds": r["seconds"]})
    finally:
        rmtree_force(work)
    return result


def main(argv):
    workers = 3
    only = None
    baseline = True
    i = 0
    while i < len(argv):
        if argv[i] == "--workers":
            workers = int(argv[i + 1])
            i += 2
        elif argv[i] == "--only":
            only = set(argv[i + 1].split(","))
            i += 2
        elif argv[i] == "--no-baseline":
            baseline = False
            i += 1
        else:
            raise SystemExit("usage: mutate.py [--workers N] [--only M01,M02] [--no-baseline]")
    os.makedirs(TMP_ROOT, exist_ok=True)
    with open(TOOL, "r", encoding="ascii") as fh:
        src = fh.read()
    summary = {"Tool": TOOL, "Mutants": []}
    if baseline:
        work = tempfile.mkdtemp(prefix="mut-baseline-", dir=TMP_ROOT)
        try:
            path = os.path.join(work, "bmin_facts.py")
            shutil.copyfile(TOOL, path)
            r = run_suite(path, None)
        finally:
            rmtree_force(work)
        summary["Baseline"] = r
        print("baseline: rc=%s %s (%ss)" % (r["rc"], r.get("ran"), r["seconds"]), flush=True)
        if r["rc"] != 0:
            print(json.dumps(summary, indent=2))
            return 1
    selected = [m for m in MUTANTS if only is None or m[0] in only]
    with concurrent.futures.ThreadPoolExecutor(max_workers=workers) as pool:
        futures = [pool.submit(run_mutant, src, m) for m in selected]
        for fut in concurrent.futures.as_completed(futures):
            res = fut.result()
            summary["Mutants"].append(res)
            print("%s %-8s %s | %s" % (res["Id"], res["Status"], res["Description"], res.get("By", res.get("Detail"))),
                  flush=True)
    summary["Mutants"].sort(key=lambda r: r["Id"])
    killed = sum(1 for r in summary["Mutants"] if r["Status"] == "KILLED")
    survived = [r["Id"] for r in summary["Mutants"] if r["Status"] == "SURVIVED"]
    invalid = [r["Id"] for r in summary["Mutants"] if r["Status"] == "INVALID"]
    summary["Killed"] = killed
    summary["Total"] = len(summary["Mutants"])
    summary["Survived"] = survived
    summary["Invalid"] = invalid
    print(json.dumps(summary, indent=2))
    return 0 if not survived and not invalid else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
