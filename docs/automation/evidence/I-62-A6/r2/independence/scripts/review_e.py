# review_e.py - (e) code review of independence (run: python -I review_e.py).
# 1. The review table: per critical fact, the primitive of the tool and of the acceptor with file:line in the PINNED
#    bytes (blobs 9a829b0e / 434ef1fc, LF), shared primitives, shared assumptions and residual common-mode risk.
#    Every citation is verified mechanically: the cited line range must contain the quoted token.
# 2. Discriminating probes on a separate synthetic repository: one input where both pieces share the same SPEC reading
#    (identical facts, R6 MATCH: common mode, undetectable by construction) and two where their primitives diverge
#    (R6 MISMATCH on the pinned tool's own facts: observable independence, failing closed).
import importlib.util
import json
import os
import shutil
import stat
import sys

_d = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(_d, name + ".py"))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


L = _load("r3lib")
BR = _load("build_repos")
T, A = "bmin_facts.py", "bmin_accept.py"

REVIEW = [
    {"Fact": "Scope membership (paths of BaseSha..CurrentSha)",
     "Tool": [(T, 642, 649, '"diff", "--name-only", "--no-renames", "-z"'), (T, 696, 696, "diff_paths(ctx, frm, to)")],
     "Acceptor": [(A, 962, 965, "ta.get(p) != tb.get(p)"), (A, 434, 449, '"ls-tree", ["-r", "-z", "--full-tree", commit]')],
     "Shared": "no: git diff (diff machinery) versus two full ls-tree listings compared by (mode, type, id)",
     "SharedAssumptions": "SPEC E6 (renames give both paths); byte order of paths; same clone and object store; tree objects "
                          "are not re-hashed by either piece",
     "Residual": "a forged or corrupted tree object in the shared clone misleads both identically (the acceptor re-hashes "
                 "commits and blobs, not trees)"},
    {"Fact": "Ancestry and Identity (BaseSha -> RedSha -> CurrentSha; HEAD, branch, toplevel, origin refs)",
     "Tool": [(T, 608, 610, '"merge-base", "--is-ancestor"'), (T, 629, 631, '"rev-parse", "HEAD"'),
              (T, 613, 617, '"rev-parse", "--verify", "--quiet", ref')],
     "Acceptor": [(A, 380, 401, "def reachable"), (A, 349, 365, "--batch-all-objects"), (A, 284, 292, '"--show-toplevel", "HEAD", "--abbrev-ref", "HEAD"'),
                  (A, 294, 307, "for-each-ref")],
     "Shared": "partly: ancestry differs (merge-base versus own walk over re-hashed commit objects); origin refs differ "
               "(rev-parse --verify versus for-each-ref); HEAD, abbreviated branch and toplevel come from git rev-parse in BOTH",
     "SharedAssumptions": "ancestry is reflexive; the tool honours replace refs and grafts (no GIT_NO_REPLACE_OBJECTS, "
                          "bmin_facts.py:141-143) while the acceptor disables replace objects (bmin_accept.py:217) and parses raw "
                          "commits: those cases diverge (detected), they are not common mode",
     "Residual": "Head/AbbrevRef/Toplevel are a shared primitive: a wrong rev-parse answer is reproduced by both; the "
                 "rebase/patch-id branch of 16.9 #5 is implemented by neither"},
    {"Fact": "Effective point derivation (16.14: trailer, first-parent merges, candidates, EFF)",
     "Tool": [(T, 1072, 1073, '"rev-list", "--first-parent", "--merges", main'), (T, 1074, 1083, 'TRAILER in body.split("\\n")'),
              (T, 1086, 1097, "ps[1] in reach and ps[0] not in reach")],
     "Acceptor": [(A, 1040, 1040, 'TRAILER in repo.message(c).split("\\n")'), (A, 1041, 1041, "reversed(repo.first_parent_chain(s))"),
                  (A, 406, 413, "def first_parent_chain"), (A, 415, 432, "def reaching"), (A, 1047, 1047, "p[1] in reach_t and p[0] not in reach_t")],
     "Shared": "no: rev-list/log output versus own walk over raw commit objects; but the SAME reading of 16.14 and of errata E4 "
               "(exact line anywhere in the message; unique trailer; first candidate)",
     "SharedAssumptions": "the trailer is an exact line of the whole message (not git's trailer block); E4 was written after the "
                          "first integration with both outputs in view, so the two readings were harmonized by the SPEC author",
     "Residual": "a misreading of 16.14 shared through the SPEC (e.g. which candidate is EFF) is undetectable; message "
                 "decoding differs (the tool normalizes CRLF, the acceptor does not: probe P3 below)"},
    {"Fact": "MV-3 (changed paths under Surfaces between EFF^1 and EFF)",
     "Tool": [(T, 247, 258, '"ls-tree", "-r", "-z", "--full-tree", rev, "--"'), (T, 1299, 1300, "changed = sorted(")],
     "Acceptor": [(A, 437, 437, '"ls-tree", ["-r", "-z", "--full-tree", commit]'), (A, 1162, 1166, "changed = sorted(")],
     "Shared": "YES: both list trees with ls-tree -r -z --full-tree (the tool with :(literal) pathspecs, the acceptor without)",
     "SharedAssumptions": "changed = blob id differs (mode ignored), map path excluded, deleted paths included; same SPEC wording",
     "Residual": "common mode on tree listing (same Git command, same clone); only the filtering differs"},
    {"Fact": "MV-4 (four lists: literal L905, ENTRY outside the schema, ADDED present in EFF^1, ENTRY present in EFF^1)",
     "Tool": [(T, 227, 244, "def blobs_at"), (T, 1322, 1333, 'kind == "ADDED" and o1 is not None')],
     "Acceptor": [(A, 1106, 1108, "def oid(tree, p)"), (A, 1177, 1188, 'kind == "ADDED" and o1 is not None')],
     "Shared": "YES for observed blob ids (ls-tree in both); the four rules are transcribed from the same SPEC text with the "
               "same structure",
     "SharedAssumptions": "SPEC 2.12 MV-4 wording (L905 literal rule, R61 b, defect table L916) read identically; the tool "
                          "keeps only type blob, the acceptor any entry id (diverges on gitlinks only)",
     "Residual": "a wrong SPEC transcription of L905/R61 b is common to both by construction; R7 'diversity' is diversity of "
                 "code, not of reading"},
    {"Fact": "MV-6 (sections per MODIFIED file; derived MODIFIED/REMOVED/ADDED-or-ENTRY)",
     "Tool": [(T, 369, 385, 'line.startswith("```")'), (T, 90, 90, 'HEADING_RE = re.compile(r"(#{1,6})(?:[ \\t]|$)")'),
              (T, 413, 417, "def heading_map")],
     "Acceptor": [(A, 499, 518, 'line.startswith("```")'), (A, 490, 496, "def heading_level"), (A, 559, 567, "def section_map")],
     "Shared": "no shared primitive (two parsers), but the same fence rule (a line starting with three backticks toggles code)",
     "SharedAssumptions": "SPEC 1.9: only ``` fences; no ~~~ fences, no indented code blocks, no setext headings; whitespace "
                          "collapse \\s+ versus str.split() (same Unicode whitespace set)",
     "Residual": "a heading-like line inside a ~~~ fence is a heading for BOTH (probe P1: identical facts, R6 MATCH); headings "
                 "are matched on the raw line by the tool and on the normalized line by the acceptor (probe P2: they diverge)"},
    {"Fact": "MV-7 (pointer first sentence of 16. and last of 16.3; 16.3 minus the pointer)",
     "Tool": [(T, 91, 91, 'SENTENCE_RE = re.compile(r"(?<=\\.)\\s+")'), (T, 1243, 1249, "del sents[idx[-1]]")],
     "Acceptor": [(A, 576, 590, "def split_sentences"), (A, 593, 600, "def remove_pointer_sentence")],
     "Shared": "no (regex split versus character loop)",
     "SharedAssumptions": "SPEC 1.10: boundary = '.' followed by whitespace or end; '?' and '!' never; 'p. ej.' always splits; "
                          "AP says the pointers are 'presentes' while the SPEC facts and both pieces use first/last position",
     "Residual": "a sentence rule the SPEC got wrong is common to both; the position reading of 'presentes' is shared"},
    {"Fact": "Blobs and hashes (rev blob ids, content SHA-256, worktree raw blob SHA-1)",
     "Tool": [(T, 238, 243, '"ls-tree", "-r", "-z", "--full-tree", rev, "--"'), (T, 273, 273, '"cat-file", "blob", oid'),
              (T, 794, 795, 'hashlib.sha1(b"blob "')],
     "Acceptor": [(A, 1013, 1013, 'tree[p][1] == "blob"'), (A, 459, 465, "git_blob_sha1(data) != oid"), (A, 110, 119, "def git_object_sha1")],
     "Shared": "YES for the tree -> blob id mapping (ls-tree in both); content read differs (cat-file blob versus cat-file "
               "--batch with re-hash); SHA-1/SHA-256 from the same hashlib with the same SPEC header formula",
     "SharedAssumptions": "the blob id in a tree entry is the identity of the content; the acceptor verifies content against "
                          "the id, the tool does not",
     "Residual": "tree objects are trusted by both; the same hashlib (library-level common mode, negligible)"},
    {"Fact": "Scope coverage (AP L553 prefix rule, U-19, malformed entries, A3/A4 containment)",
     "Tool": [(T, 666, 673, "def u19"), (T, 676, 687, "def malformed_reasons")],
     "Acceptor": [(A, 701, 705, "def u19"), (A, 708, 719, "def is_malformed"), (A, 722, 732, "def covers")],
     "Shared": "no shared code, but logically identical rules (exact match or prefix ending in '/'; '/**' -> '/' prefix; "
               "case-sensitive; no Unicode normalization)",
     "SharedAssumptions": "the SPEC reading of AP L553-L554 and U-19: 'docs/adr' without slash covers only itself; NFC/NFD "
                          "variants are different paths",
     "Residual": "if AP L553 meant something else (e.g. a directory without slash is a prefix) both are equally wrong; R7 "
                 "cannot see it"},
    {"Fact": "Reads and isolation evidence used by R2/R9 (Runtime.Isolated, GitCalls, Inputs)",
     "Tool": [(T, 1581, 1587, "sys.flags.isolated == 1"), (T, 144, 144, "ctx.git_calls.append(argv)")],
     "Acceptor": [(A, 2018, 2022, "Runtime.Isolated is not true"), (A, 2563, 2570, 'calls = run.env.get("GitCalls")')],
     "Shared": "the evidence itself: R2 and R9 judge what the tool REPORTS about itself",
     "SharedAssumptions": "the received envelope's Runtime and GitCalls are honest; R3 re-runs with -I but cannot observe how "
                          "the received envelope was produced",
     "Residual": "a tool that runs a forbidden git call without recording it, or that lies in Runtime, is not detectable by "
                 "R2/R9 (only the facts comparisons can catch the consequence)"},
]


def verify(ref):
    f, a, b, token = ref
    path = L.PIN + ("/tool/" if f == T else "/accept/") + f
    lines = L.read_bytes(path).decode("utf-8").split("\n")
    seg = "\n".join(lines[a - 1:b])
    return {"Ref": "%s:%d%s" % (f, a, "" if a == b else "-%d" % b), "Token": token, "Verified": token in seg}


# ------------------------------------------------------------------ probes

def rmtree(p):
    def onexc(fn, q, e):
        os.chmod(q, stat.S_IWRITE)
        fn(q)
    if os.path.isdir(p):
        shutil.rmtree(p, onexc=onexc)


def probes():
    R = L.WORK + "/syne"
    rmtree(R)
    os.makedirs(R)
    L.git(R, "init", "-q", "-b", "r3/head")
    L.git(R, "config", "core.autocrlf", "false")
    seq = [100]

    def commit(files, parents, msg):
        seq[0] += 1
        return L.make_commit(R, L.make_tree(R, files), parents, msg, seq[0])

    out = {}
    cases = {
        "P1-tilde-fence (shared reading)": ("\n~~~text\n## Falso encabezado dentro de un bloque\n~~~\n", None),
        "P2-indented-heading (divergent parsers)": ("\n   ## Encabezado sangrado\n", None),
        "P3-crlf-trailer-message (divergent decoding)": ("", "crlf"),
    }
    for name, (wf_extra, mode) in cases.items():
        base, eff, files, ents = BR.v0_parts()
        eff[L.WF_PATH] = (BR.wf_text(True) + wf_extra).encode("utf-8")
        md = BR.map_doc(base, eff, files, ents)
        eff[L.MAP_PATH] = BR.map_bytes(md)
        b = commit(base, [], "R3 probe base (%s)\n" % name)
        msg = BR.TRAILER_MSG % name
        if mode == "crlf":
            msg = msg.replace("\n", "\r\n")
        t = commit(eff, [b], msg)
        m = commit(eff, [b, t], BR.MERGE_MSG % ("probe", name))
        L.git(R, "update-ref", "refs/heads/r3/p%d" % seq[0], m)
        argv = ["clausemap", "--repo", R, "--main", m]
        rc, raw = L.run_tool(L.TOOL, argv, cwd=R)
        envp = L.RUNS + "/e/%s.json" % name.split(" ")[0]
        L.write_bytes(envp, raw)
        env = json.loads(raw.decode("ascii"))
        f = env.get("Facts") or {}
        wf6 = [r for r in ((f.get("Validate") or {}).get("MV6") or {}).get("PerFile") or [] if r["Path"] == L.WF_PATH]
        cl = L.broad_closure(R, BR.SURF, [], "r3/head")
        clp = L.RUNS + "/e/closure.json"
        L.write_json_file(clp, cl)
        req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": R,
               "Tool": {"Path": L.TOOL, "PinnedBlob": L.TOOL_BLOB, "RunFrom": "path"},
               "Interpreter": {"Python": L.platform.python_version(), "ExecutableSha256": L.exe_sha256()},
               "Runs": [{"Name": "clausemap", "Envelope": envp, "Argv": argv}],
               "Closure": {"Path": clp, "Sha256": L.sha256_file(clp)},
               "NotApplicable": {"R4": "probe", "R5": "probe", "R10": "probe"}}
        rp = L.RUNS + "/e/%s.request.json" % name.split(" ")[0]
        L.write_json_file(rp, req)
        arc, res = L.run_acceptor(rp)
        s = L.acceptor_summary(arc, res)
        out[name] = {"ToolTrailerCommits": (f.get("Derive") or {}).get("TrailerCommits"), "ToolEff": (f.get("Derive") or {}).get("Eff"),
                     "ToolWorkflowDerivedAddedOrEntry": wf6[0]["DerivedAddedOrEntry"] if wf6 else None,
                     "Decision": s["Decision"], "R6": s["Status"].get("R6"), "R7": s["Status"].get("R7"),
                     "R6Fields": L.mismatch_fields(res).get("R6", [])}
        L.log(name, out[name]["R6"], out[name]["R6Fields"][:4])
    rmtree(R)
    return out


def main():
    table = []
    ok = True
    for row in REVIEW:
        r = dict(row)
        r["Tool"] = [verify(x) for x in row["Tool"]]
        r["Acceptor"] = [verify(x) for x in row["Acceptor"]]
        ok = ok and all(x["Verified"] for x in r["Tool"] + r["Acceptor"])
        table.append(r)
    pr = probes()
    res = {"Schema": "r3-part-e/1", "Header": L.common_header(), "Review": table, "AllCitationsVerified": ok, "Probes": pr,
           "Note": "line numbers refer to the pinned blobs as LF bytes (git cat-file blob), not to CRLF working-tree files"}
    p, sha = L.write_result("e-review.json", res)
    L.log("written", p, sha, "citations verified:", ok)
    if not ok:
        for r in table:
            for x in r["Tool"] + r["Acceptor"]:
                if not x["Verified"]:
                    L.log("UNVERIFIED", x)


if __name__ == "__main__":
    main()
