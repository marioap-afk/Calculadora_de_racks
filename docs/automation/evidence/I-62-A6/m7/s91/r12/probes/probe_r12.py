# -*- coding: utf-8 -*-
"""probe_r12.py - sondas dirigidas para r12 (I-62, decisiones 91, punto 4). Solo lectura.

Lee objetos Git del worktree W (por blob, con --no-replace-objects) y del fixture
D:/r62-a6r2/r3/deliv (log, cat-file, ls-tree, diff; con --no-optional-locks), ejecuta funciones
del aceptante fijado desde sus bytes (blob 434ef1fc..., comprobado) y escribe solo
probes/probe-r12-result.json y un repositorio temporal bajo probes/_tmp (borrado al final).

Uso:  python -I probe_r12.py
"""

import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "probe-r12-result.json")
W = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees",
                 "architecture-portabilidad-coordinador-principal")
FIX = "D:/r62-a6r2/r3/deliv"
ACC_BLOB = "434ef1fccb0d15762b30244b4a60928120698411"
ACC_SHA256 = "173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0"
MAP_BLOB = "4d49d3e1a63d2b6128c9d9eeac473156a421571a"
SCHEMA_BLOB = "6b54dda9579d694200d8fc9ec26a4a846eef529b"
WF_EFF = "446c32ba6a91f9a712a0758038ae0a46ab1c970f"
AP_EFF = "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5"
FIX_MAIN = "fbe25347799e5b801ef708454212335537448bd1"
FIX_BASE = "95bdc29d428b35d2188a10e1c5462d6caf2be4df"
FIX_CUR = "b417d36cb820a27c78d6c7d50c234460469c89f8"
TRAILER = "Agent-Protocol-Normative: I-62"


def git(repo, args, stdin=b"", env=None, ok=(0,)):
    p = subprocess.run(["git", "-C", repo] + args, input=stdin, stdout=subprocess.PIPE,
                       stderr=subprocess.PIPE, env=env)
    if p.returncode not in ok:
        raise SystemExit("git failed: " + " ".join(args[:3]) + " rc=" + str(p.returncode))
    return p.returncode, p.stdout


def blob_bytes(oid):
    _, data = git(W, ["--no-replace-objects", "cat-file", "blob", oid])
    h = hashlib.sha1()
    h.update(b"blob " + str(len(data)).encode("ascii") + b"\0" + data)
    if h.hexdigest() != oid:
        raise SystemExit("blob id mismatch " + oid)
    return data


def load_acceptor():
    src = blob_bytes(ACC_BLOB)
    if hashlib.sha256(src).hexdigest() != ACC_SHA256:
        raise SystemExit("acceptor SHA-256 mismatch")
    ns = {"__name__": "bmin_accept_from_blob"}
    exec(compile(src, "bmin_accept.py@" + ACC_BLOB[:8], "exec"), ns)
    return ns


A = load_acceptor()
MdDoc, norm_ws, to_lf = A["MdDoc"], A["norm_ws"], A["to_lf"]
RESULT = {"Schema": "i62-r12-probe/1", "Acceptor": {"Blob": ACC_BLOB, "Sha256": ACC_SHA256}}


def text_of(oid):
    return blob_bytes(oid).decode("utf-8", "surrogateescape")


# ------------------------------------------------------------------ alternative Markdown reader
FENCE_RE = re.compile(r"^ {0,3}(`{3,}|~{3,})(.*)$")
ATX_RE = re.compile(r"^ {0,3}(#{1,6})(?:[ \t]|$)")
SETEXT_RE = re.compile(r"^ {0,3}(=+|-+)[ \t]*$")


def expand_indent(line):
    n = 0
    for ch in line:
        if ch == " ":
            n += 1
        elif ch == "\t":
            n += 4 - (n % 4)
        else:
            break
    return n


def alt_reader(text):
    """CommonMark-like subset: ``` and ~~~ fences indented 0-3, indented code (>=4 after a blank
    line), ATX headings indented 0-3; setext underlines only reported. Per line: (code, level)."""
    lines = to_lf(text).split("\n")
    out = []
    fence = None
    prev_blank = True
    in_indented = False
    prev_para = False
    setext = []
    for i, line in enumerate(lines):
        blank = line.strip() == ""
        if fence is not None:
            m = re.match(r"^ {0,3}(" + re.escape(fence[0]) + "{" + str(fence[1]) + r",})[ \t]*$", line)
            out.append((True, 0))
            if m:
                fence = None
            prev_blank, prev_para = blank, False
            continue
        m = FENCE_RE.match(line)
        if m and not (m.group(1)[0] == "`" and "`" in m.group(2)):
            fence = (m.group(1)[0], len(m.group(1)))
            out.append((True, 0))
            prev_blank, prev_para, in_indented = False, False, False
            continue
        ind = expand_indent(line)
        if not blank and ind >= 4 and (prev_blank or in_indented) and not prev_para:
            in_indented = True
            out.append((True, 0))
            prev_blank = False
            continue
        if not blank:
            in_indented = False
        h = ATX_RE.match(line)
        if h:
            out.append((False, len(h.group(1))))
            prev_para = False
        else:
            if SETEXT_RE.match(line) and prev_para:
                setext.append(i + 1)
            out.append((False, 0))
            prev_para = not blank
        prev_blank = blank
    return lines, out, setext


def acc_reader(text):
    d = MdDoc(text)
    lv = {}
    for idx, level, _nl in d.heads:
        lv[idx] = level
    return d.lines, [(d.in_code[i], lv.get(i, 0)) for i in range(len(d.lines))]


def esc(s):
    return s.encode("ascii", "backslashreplace").decode("ascii")[:160]


def compare_readers(text):
    lines, a = acc_reader(text)
    _, c, setext = alt_reader(text)
    diffs = []
    code_only = 0
    for i, (x, y) in enumerate(zip(a, c)):
        ha = 0 if x[0] else x[1]
        hc = 0 if y[0] else y[1]
        if ha != hc:
            diffs.append({"Line": i + 1, "AcceptorHeadingLevel": ha, "AltHeadingLevel": hc, "Text": esc(lines[i])})
        elif x[0] != y[0]:
            code_only += 1
    constructs = {
        "TildeFences": sum(1 for ln in lines if re.match(r"^ {0,3}~{3,}", ln)),
        "IndentedBacktickFences": sum(1 for ln in lines if re.match(r"^ {1,3}`{3,}", ln)),
        "IndentedHashLines": sum(1 for ln in lines if re.match(r"^[ \t]+#{1,6}(?:[ \t]|$)", ln)),
        "SetextCandidates": setext[:20],
        "HeadingLinesWithNonAsciiSpace": sum(
            1 for ln in lines if re.match(r"^\s*#{1,6}", ln) and any(ch.isspace() and ord(ch) > 127 for ch in ln)),
        "TabsInHeadingLines": sum(1 for ln in lines if re.match(r"^\s*#{1,6}", ln) and "\t" in ln),
        "CR": text.count("\r"),
        "CodeFlagOnlyDifferences_noHeadingEffect": code_only,
        "BacktickFenceLinesCol0": sum(1 for ln in lines if ln.startswith("```")),
    }
    return diffs, constructs


# ------------------------------------------------------------------ normalizations for MV-6 / MV-7
def norm_lines_b1(text):
    """CRLF -> LF; runs of spaces/tabs collapsed inside each line; line structure kept."""
    return "\n".join(re.sub(r"[ \t]+", " ", ln) for ln in to_lf(text).split("\n"))


def norm_lines_b2(text):
    """As b1, plus each line trimmed and the whole text trimmed."""
    return "\n".join(re.sub(r"[ \t]+", " ", ln).strip() for ln in to_lf(text).split("\n")).strip()


def section_map_with(text, norm):
    d = MdDoc(text)
    texts = {A["PREAMBLE"]: norm(d.preamble_text())}
    counts = {}
    for k, (_i, _lv, nl) in enumerate(d.heads):
        counts[nl] = counts.get(nl, 0) + 1
        if counts[nl] == 1:
            texts[nl] = norm(d.section_text(k))
    return texts, sorted(h for h, c in counts.items() if c > 1)


def derived(t1, t2, norm):
    h1, d1 = section_map_with(t1, norm)
    h2, d2 = section_map_with(t2, norm)
    return {"DerivedModified": sorted(h for h in set(h1) & set(h2) if h1[h] != h2[h]),
            "DerivedRemoved": sorted(set(h1) - set(h2)),
            "DerivedAddedOrEntry": sorted(set(h2) - set(h1)),
            "Dups": [d1, d2]}


# ------------------------------------------------------------------ P1: map, schema, MV-1, MV-5, MV-6, MV-7, E2-E3
map_bytes = blob_bytes(MAP_BLOB)
schema_bytes = blob_bytes(SCHEMA_BLOB)
p1 = {}
p1["MapBOM"] = map_bytes[:3] == b"\xef\xbb\xbf"
p1["SchemaBOM"] = schema_bytes[:3] == b"\xef\xbb\xbf"
m = A["parse_json_bytes"](map_bytes, "map")
s = A["parse_json_bytes"](schema_bytes, "schema")
p1["MapParsesStrict"] = True
p1["SchemaParsesStrict"] = True
p1["SchemaDialect"] = s.get("$schema")
kw = set()


def walk_kw(o):
    if isinstance(o, dict):
        for k, v in o.items():
            kw.add(k)
            if k == "properties" and isinstance(v, dict):
                for vv in v.values():
                    walk_kw(vv)
            elif isinstance(v, (dict, list)) and k not in ("enum", "const", "required"):
                walk_kw(v)
    elif isinstance(o, list):
        for v in o:
            walk_kw(v)


walk_kw(s)
p1["SchemaKeywords"] = sorted(k for k in kw if k in A["_SUPPORTED_KEYWORDS"] or k in A["_ANNOTATIONS"]
                              or k in ("properties",))
p1["SchemaUnsupportedByAcceptor"] = A["schema_unsupported"](s)
viol = []
A["schema_validate"](s, m, "", viol)
p1["AcceptorSchemaViolations"] = viol
pats = set()


def walk_pat(o):
    if isinstance(o, dict):
        for k, v in o.items():
            if k == "pattern":
                pats.add(v)
            walk_pat(v)
    elif isinstance(o, list):
        for v in o:
            walk_pat(v)


walk_pat(s)
p1["SchemaPatterns"] = sorted(pats)
nums = []


def walk_num(o):
    if isinstance(o, bool):
        return
    if isinstance(o, (int, float)):
        nums.append(o)
    elif isinstance(o, dict):
        for v in o.values():
            walk_num(v)
    elif isinstance(o, list):
        for v in o:
            walk_num(v)


walk_num(m)
p1["MapNumericValueCount"] = len(nums)
p1["MapNumericAllInt"] = all(isinstance(x, int) for x in nums)
p1["MapNumericValues"] = sorted(set(nums))
entries = m["Entries"]
files = m["Files"]
keys_exact = [(e["Path"], e["Section"]) for e in entries]
keys_norm = [(e["Path"], norm_ws(e["Section"])) for e in entries]
p1["MV5"] = {"Entries": len(entries),
             "DupExact": sorted(set(k for k in keys_exact if keys_exact.count(k) > 1)),
             "DupNormalized": sorted(set(k for k in keys_norm if keys_norm.count(k) > 1)),
             "SectionsNotNormalized": [esc(e["Section"]) for e in entries if e["Section"] != norm_ws(e["Section"])],
             "SectionsWithNonAsciiSpace": [esc(e["Section"]) for e in entries
                                           if any(ch.isspace() and ord(ch) > 127 for ch in e["Section"])]}
p1["MV6"] = []
mod = [f for f in files if f["FileKind"] == "MODIFIED"]
for f in mod:
    t1, t2 = text_of(f["BaseBlob"]), text_of(f["EffBlob"])
    rec = A["_mv6_file"](f["Path"], t1, t2, entries)
    acc_ok = all(sorted(rec[a]) == sorted(rec[b]) for a, b in
                 (("DerivedModified", "MapModified"), ("DerivedRemoved", "MapRemoved"),
                  ("DerivedAddedOrEntry", "MapAddedOrEntry"))) and not rec["H1Duplicates"] and not rec["H2Duplicates"]
    row = {"Path": f["Path"], "AcceptorEqualsMap": acc_ok,
           "MapModified": len(rec["MapModified"]), "MapRemoved": len(rec["MapRemoved"]),
           "MapAddedOrEntry": len(rec["MapAddedOrEntry"])}
    for name, nf in (("B1_lines_kept", norm_lines_b1), ("B2_lines_kept_trimmed", norm_lines_b2)):
        dv = derived(t1, t2, nf)
        row[name] = {
            "EqualsMap": dv["DerivedModified"] == rec["MapModified"] and dv["DerivedRemoved"] == rec["MapRemoved"]
            and dv["DerivedAddedOrEntry"] == rec["MapAddedOrEntry"],
            "ExtraModified": [esc(h) for h in sorted(set(dv["DerivedModified"]) - set(rec["MapModified"]))][:12],
            "MissingModified": [esc(h) for h in sorted(set(rec["MapModified"]) - set(dv["DerivedModified"]))][:12]}
    rd = {}
    for side, t in (("Base", t1), ("Eff", t2)):
        diffs, cons = compare_readers(t)
        dd = MdDoc(t)
        levels = [lv for (_i, lv, _nl) in dd.heads]
        first2 = levels.index(2) if 2 in levels else None
        cons["Level1Headings"] = levels.count(1)
        cons["HeadingAboveLevel2BeforeFirstLevel2"] = (first2 is not None and any(lv >= 3 for lv in levels[:first2]))
        cons["FirstHeadingLevels"] = levels[:4]
        rd[side] = {"ReaderDisagreements": diffs[:20], "DisagreementCount": len(diffs), "Constructs": cons}
    row["Readers"] = rd
    nonmd = not f["Path"].endswith(".md")
    row["NonMarkdown"] = nonmd
    p1["MV6"].append(row)

# E2-E3 on WORKFLOW and AP at EFF (MC_I62 blobs; also the fixture's EFF blobs)
wf, ap = text_of(WF_EFF), text_of(AP_EFF)
wd, apd = MdDoc(wf), MdDoc(ap)
em = wd.match_lines(A["ENTRY_HEADING"])
rm = apd.match_lines(A["R163_HEADING"])
p1["E2E3"] = {"EntryMatches": em, "R163Matches": rm,
              "EntryNamesR163_raw": wd.section_contains_literal(em[0], A["R163_HEADING"]) if len(em) == 1 else None,
              "EntryHeadingLineOwnLine": any(ln.strip() == A["R163_HEADING"] for ln in
                                             wd.section_text(wd.head_index_at(em[0] - 1)).split("\n")) if len(em) == 1 else None,
              "RawLinesEqualEntry_anyContext": sum(1 for ln in to_lf(wf).split("\n") if norm_ws(ln) == norm_ws(A["ENTRY_HEADING"])),
              "RawLinesEqualR163_anyContext": sum(1 for ln in to_lf(ap).split("\n") if norm_ws(ln) == norm_ws(A["R163_HEADING"]))}
for side, t in (("WORKFLOW_eff", wf), ("AP_eff", ap)):
    diffs, cons = compare_readers(t)
    p1["E2E3"][side + "_ReaderDisagreements"] = len(diffs)
    p1["E2E3"][side + "_Constructs"] = cons

# MV-7 on the B1 pair (AP BaseBlob of the map vs AP EffBlob)
ap_base = [f for f in mod if f["Path"] == "docs/AUTOMATION_PLAN.md"][0]["BaseBlob"]
ap1 = text_of(ap_base)
d2, d1 = MdDoc(ap), MdDoc(ap1)
k16 = d2.find_unique_heading(2, A["S16_LABEL"])
k3e = d2.find_unique_heading(3, A["S163_LABEL"])
k3b = d1.find_unique_heading(3, A["S163_LABEL"])
P = A["POINTER"]
body16 = norm_ws(d2.body_text(k16))
body163 = norm_ws(d2.body_text(k3e))
body163b = norm_ws(d1.body_text(k3b))
first = A["split_sentences"](body16)[0]
last = A["split_sentences"](body163)[-1]
minus = A["remove_pointer_sentence"](body163, P)
raw_e, raw_b = d2.body_text(k3e), d1.body_text(k3b)
p1["MV7_B1pair"] = {
    "ApBaseBlob": ap_base,
    "PointerIsFirstSentenceS16": first == P, "PointerIsLastSentenceS163": last == P,
    "PointerOccurrencesIn163Eff": body163.count(P), "PointerOccurrencesIn163Base": body163b.count(P),
    "AcceptorEqualAfterRemoval": minus == body163b,
    "RemoveAllOccurrencesEqual": norm_ws(body163.replace(P, "")) == body163b,
    "LinesKeptTrimmed_SubstringRemovedEqual": norm_lines_b2(raw_e.replace(P, "")) == norm_lines_b2(raw_b),
    "LinesKept_SubstringRemovedEqual": norm_lines_b1(raw_e.replace(P, "")).rstrip() == norm_lines_b1(raw_b).rstrip(),
    "RemoveWholeLineEqual": norm_ws("\n".join(ln for ln in to_lf(raw_e).split("\n") if P not in ln)) == body163b,
}
diffs, cons = compare_readers(ap1)
p1["MV7_B1pair"]["APbase_ReaderDisagreements"] = len(diffs)
RESULT["P1_B1_blobs"] = p1

# ------------------------------------------------------------------ P2: trailer recognition on the fixture main
_, raw = git(FIX, ["--no-optional-locks", "log", "-z", "--format=%H%x01%B", FIX_MAIN])
p2 = {"Main": FIX_MAIN, "Commits": 0, "ExactLineLF": [], "ExactLineAfterCRStrip": [], "Substring": [],
      "GitTrailerKey": []}
for rec in raw.split(b"\0"):
    if not rec.strip():
        continue
    sha, _, body = rec.decode("utf-8", "surrogateescape").lstrip("\n").partition("\x01")
    p2["Commits"] += 1
    if TRAILER in body.split("\n"):
        p2["ExactLineLF"].append(sha)
    if TRAILER in [ln.rstrip("\r") for ln in body.split("\n")]:
        p2["ExactLineAfterCRStrip"].append(sha)
    if TRAILER in body:
        p2["Substring"].append(sha)
    _, tv = git(FIX, ["--no-optional-locks", "log", "-1", "--format=%(trailers:key=Agent-Protocol-Normative,valueonly)", sha])
    if tv.strip():
        p2["GitTrailerKey"].append(sha)
    if "\r" in body:
        p2.setdefault("CRInMessage", []).append(sha)
p2["CRInMessageCount"] = len(p2.get("CRInMessage", []))
RESULT["P2_fixture_trailer"] = p2

# ------------------------------------------------------------------ P3: scope data of the bench delivery
_, ns = git(FIX, ["--no-optional-locks", "diff", "--name-status", "-M", FIX_BASE + ".." + FIX_CUR])
_, nn = git(FIX, ["--no-optional-locks", "diff", "--name-only", "--no-renames", FIX_BASE + ".." + FIX_CUR])
_, kraw = git(FIX, ["--no-optional-locks", "cat-file", "blob",
                    FIX_CUR + ":docs/automation/decisions/FX-U1-T1.gate-contract.json"])
k = json.loads(kraw.decode("utf-8"))
deleg = json.loads(open(os.path.join("D:/r62-a6m7/kit/inputs", "deleg.json"), "rb").read().decode("utf-8"))
p3 = {"NameStatusM": ns.decode("utf-8").split("\n")[:-1], "NameOnlyNoRenames": nn.decode("utf-8").split("\n")[:-1]}
p3["RenamesDetected"] = any(ln[:1] in ("R", "C") for ln in p3["NameStatusM"])
p3["ModeOnlyOrTypeChanges"] = any(ln[:1] == "T" for ln in p3["NameStatusM"])
for label, obj in (("Contract", k), ("Delegation", deleg)):
    ent = obj["AllowedWriteScope"] + obj["ForbiddenWriteScope"]
    p3[label] = {"Entries": len(ent),
                 "AcceptorMalformed": [e for e in ent if A["is_malformed"](e)],
                 "Empty": [e for e in ent if e == ""],
                 "DotSegmentEmptySegmentOrEdgeSpaces": [
                     e for e in ent
                     if any(seg in (".", "") for seg in (e[:-1] if e.endswith("/") else e).split("/"))
                     or e != e.strip()],
                 "NonFinalWildcard": [e for e in ent if any(ch in (e[:-3] if e.endswith("/**") else e) for ch in "*?[")],
                 "FinalDoubleStar": [e for e in ent if e.endswith("/**")]}
RESULT["P3_bench_scope"] = p3

# ------------------------------------------------------------------ P4: default rename handling of git diff --name-only
tmp = os.path.join(HERE, "_tmp")


def _force_remove(func, path, _exc):
    os.chmod(path, 0o666)
    func(path)


def rm_tmp():
    if os.path.exists(tmp):
        shutil.rmtree(tmp, onexc=_force_remove)


rm_tmp()
os.makedirs(tmp)
env = dict(os.environ)
for key in list(env):
    if key.startswith("GIT_"):
        del env[key]
env["GIT_CONFIG_NOSYSTEM"] = "1"
env["GIT_CONFIG_GLOBAL"] = os.devnull
env.update({"GIT_AUTHOR_NAME": "probe", "GIT_AUTHOR_EMAIL": "probe", "GIT_COMMITTER_NAME": "probe",
            "GIT_COMMITTER_EMAIL": "probe", "GIT_AUTHOR_DATE": "2026-10-17T00:00:00Z",
            "GIT_COMMITTER_DATE": "2026-10-17T00:00:00Z"})
try:
    git(tmp, ["init", "-q"], env=env)
    with open(os.path.join(tmp, "forbidden.txt"), "wb") as fh:
        fh.write(b"line one\nline two\nline three\nline four\n")
    git(tmp, ["add", "forbidden.txt"], env=env)
    git(tmp, ["commit", "-q", "-m", "a"], env=env)
    git(tmp, ["mv", "forbidden.txt", "allowed.txt"], env=env)
    git(tmp, ["commit", "-q", "-m", "b"], env=env)
    _, v = git(tmp, ["--version"], env=env)
    _, d0 = git(tmp, ["diff", "--name-only", "HEAD~1..HEAD"], env=env)
    _, d1 = git(tmp, ["diff", "--name-only", "--no-renames", "HEAD~1..HEAD"], env=env)
    _, d2 = git(tmp, ["-c", "diff.renames=false", "diff", "--name-only", "HEAD~1..HEAD"], env=env)
    RESULT["P4_git_rename_default"] = {"GitVersion": v.decode("ascii", "replace").strip(),
                                       "DefaultConfig": d0.decode().split("\n")[:-1],
                                       "NoRenames": d1.decode().split("\n")[:-1],
                                       "DiffRenamesFalse": d2.decode().split("\n")[:-1]}
finally:
    rm_tmp()
RESULT["P4_git_rename_default"]["TmpRemoved"] = not os.path.exists(tmp)

data = (json.dumps(RESULT, ensure_ascii=True, sort_keys=True, indent=1) + "\n").encode("ascii")
for bad in (os.environ.get("USERNAME", "\0"), os.environ.get("USERPROFILE", "\0")):
    if bad and bad.encode("utf-8", "replace") in data:
        raise SystemExit("hygiene: user name in output")
with open(OUT, "wb") as fh:
    fh.write(data)
print("wrote " + os.path.basename(OUT) + " sha256 " + hashlib.sha256(data).hexdigest())
