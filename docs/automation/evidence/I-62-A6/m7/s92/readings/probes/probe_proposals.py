# -*- coding: utf-8 -*-
"""probe_proposals.py - evalua las condiciones de fallo cerrado propuestas (C-01, C-04, C-10, C-12, C-13,
C-16/C-18/C-23 y C-24) sobre los datos del banco FX-U1 y sobre la parte de B1 que fija A5-1. Solo lectura.

Implementacion propia a partir del texto de AP 16.5, 16.9 y 16.13 (no usa el codigo del aceptante ni del
instrumento). Lee objetos Git de W (por blob, --no-replace-objects) y del banco D:/r62-a6r2/r3/deliv
(--no-optional-locks, sin configuracion de sistema ni global), las delegaciones y entregas del banco
(D:/r62-a6m7/bench/work/in-*/ y D:/r62-a6m7/kit/inputs/deleg.json), y escribe solo
probes/probe-proposals-result.json.

Uso:  python -I probe_proposals.py
"""

import glob
import hashlib
import json
import os
import re
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "probe-proposals-result.json")
W = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees",
                 "architecture-portabilidad-coordinador-principal")
BENCH = "D:/r62-a6r2/r3/deliv"
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
SCHEMA_PATH = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
AP_PATH = "docs/AUTOMATION_PLAN.md"
WF_PATH = "docs/WORKFLOW.md"
MAP_BLOB = "4d49d3e1a63d2b6128c9d9eeac473156a421571a"
SCHEMA_BLOB = "6b54dda9579d694200d8fc9ec26a4a846eef529b"
WF_EFF_BLOB = "446c32ba6a91f9a712a0758038ae0a46ab1c970f"
AP_EFF_BLOB = "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5"
KEY = "agent-protocol-normative"
TRAILER = "Agent-Protocol-Normative: I-62"
POINTER = "Antes de aplicar esta sección, toda unidad aplica §16.13."
BENCH_MAIN = "fbe25347799e5b801ef708454212335537448bd1"
BENCH_CONTRACT = ("b417d36cb820a27c78d6c7d50c234460469c89f8", "docs/automation/decisions/FX-U1-T1.gate-contract.json")

ENV = dict((k, v) for k, v in os.environ.items() if not k.startswith("GIT_"))
ENV["GIT_CONFIG_NOSYSTEM"] = "1"
ENV["GIT_CONFIG_GLOBAL"] = os.devnull


def git(repo, args, ok=(0,), stdin=b""):
    extra = ["--no-optional-locks"] if repo == BENCH else ["--no-replace-objects"]
    p = subprocess.run(["git", "-C", repo] + extra + args, input=stdin, stdout=subprocess.PIPE,
                       stderr=subprocess.PIPE, env=ENV)
    if p.returncode not in ok:
        return None
    return p.stdout


def blob_w(oid):
    data = git(W, ["cat-file", "blob", oid])
    h = hashlib.sha1(b"blob " + str(len(data)).encode("ascii") + b"\0" + data).hexdigest()
    if h != oid:
        raise SystemExit("blob mismatch " + oid)
    return data


def is_commit(repo, rev):
    out = git(repo, ["cat-file", "-t", rev], ok=(0, 128))
    return out is not None and out.strip() == b"commit"


def tree(repo, rev):
    out = git(repo, ["ls-tree", "-r", "-z", "--full-tree", rev])
    t = {}
    for rec in out.split(b"\0"):
        if rec:
            meta, _, path = rec.partition(b"\t")
            mode, kind, oid = meta.decode("ascii").split(" ")
            t[path.decode("utf-8", "surrogateescape")] = (mode, kind, oid)
    return t


def blob_bench(oid):
    return git(BENCH, ["cat-file", "blob", oid])


# ------------------------------------------------------------------ C-01: forma del tráiler en RevList(MainSha_eval)
def check_c01(repo, main):
    shas = git(repo, ["rev-list", main]).decode("ascii").split()
    batch = git(repo, ["cat-file", "--batch"], stdin=("\n".join(shas) + "\n").encode("ascii"))
    mentions, pos = [], 0
    for sha in shas:
        nl = batch.index(b"\n", pos)
        head = batch[pos:nl].split(b" ")
        size = int(head[2])
        data = batch[nl + 1:nl + 1 + size]
        pos = nl + 1 + size + 1
        cut = data.find(b"\n\n")
        msg = data[cut + 2:].decode("utf-8", "surrogateescape") if cut >= 0 else ""
        if KEY in msg.casefold():
            mentions.append((sha, msg))
    res = {"Main": main, "CommitsScanned": len(shas), "CommitsMentioningKey": [m[0] for m in mentions]}
    reasons = []
    if len(mentions) != 1:
        reasons.append("key mentioned in %d commits" % len(mentions))
    else:
        sha, msg = mentions[0]
        lines = msg.split("\n")
        if "\r" in msg:
            reasons.append("CR in message")
        if sum(1 for ln in lines if ln == TRAILER) != 1:
            reasons.append("exact line count is not 1")
        if sum(1 for ln in lines if KEY in ln.casefold()) != 1:
            reasons.append("key mentioned on more than one line or in another form")
        tv = git(repo, ["log", "-1", "--format=%(trailers:key=Agent-Protocol-Normative,valueonly,unfold)", sha])
        vals = [v for v in tv.decode("utf-8", "replace").split("\n") if v.strip()]
        res["GitTrailerValues"] = vals
        if vals != ["I-62"]:
            reasons.append("git trailer block does not carry exactly the value I-62")
    res["Reasons"] = reasons
    res["Triggered"] = bool(reasons)
    return res


def derive_eff(repo, main, trailer_commit):
    merges = git(repo, ["rev-list", "--first-parent", "--merges", "--parents", main]).decode("ascii").split("\n")
    cands = []
    for ln in reversed([x for x in merges if x]):
        parts = ln.split()
        m, p1, p2 = parts[0], parts[1], parts[2]
        r2 =subprocess.run(["git", "-C", repo, "--no-optional-locks", "merge-base", "--is-ancestor", trailer_commit, p2],
                            env=ENV).returncode == 0
        r1 = subprocess.run(["git", "-C", repo, "--no-optional-locks", "merge-base", "--is-ancestor", trailer_commit, p1],
                            env=ENV).returncode == 0
        if r2 and not r1:
            cands.append((m, p1))
    return cands


# ------------------------------------------------------------------ C-04, C-10: mapa y esquema; ENTRY en EFF^1
def check_c04(t_eff):
    m = t_eff.get(MAP_PATH)
    s = t_eff.get(SCHEMA_PATH)
    ok = m is not None and s is not None and m[2] == MAP_BLOB and s[2] == SCHEMA_BLOB
    return {"MapBlob": m[2] if m else None, "SchemaBlob": s[2] if s else None, "Triggered": not ok}


def check_c10(t1, t2, mp):
    surf = mp["Surfaces"]

    def under(p):
        return any(p == x or (x.endswith("/") and p.startswith(x)) for x in surf)

    def oid(t, p):
        return t[p][2] if p in t else None

    changed = sorted(p for p in set(t1) | set(t2) if p != MAP_PATH and under(p) and oid(t1, p) != oid(t2, p))
    fpaths = [f["Path"] for f in mp["Files"]]
    mv3_ok = sorted(set(changed)) == sorted(set(fpaths)) and len(fpaths) == len(set(fpaths)) \
        and not [p for p in changed if p in t1 and p not in t2]
    lit = []
    for f in mp["Files"]:
        p, k = f["Path"], f["FileKind"]
        o1, o2 = oid(t1, p), oid(t2, p)
        if k == "MODIFIED" and not (o1 is not None and f["BaseBlob"] == o1 and o2 is not None and f["EffBlob"] == o2):
            lit.append(p)
        if k in ("ADDED", "ENTRY") and not (f["BaseBlob"] is None and o2 is not None and f["EffBlob"] == o2):
            lit.append(p)
    entry_present = [f["Path"] for f in mp["Files"] if f["FileKind"] == "ENTRY" and f["Path"] in t1]
    decisive = bool(entry_present) and mv3_ok and not lit
    return {"EntryFilesPresentInEff1": entry_present, "MV3Holds": mv3_ok, "MV4LiteralViolations": len(lit),
            "Triggered": decisive}


# ------------------------------------------------------------------ lector propio (AP 16.3 / 16.13) y C-12, C-13
HEAD_RE = re.compile(r"^(#{1,6})(?: |$)")


def norm_line(s):
    return " ".join(s.split())


def heads_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    out, inside = [], False
    for i, ln in enumerate(lines):
        if ln.startswith("```"):
            inside = not inside
            continue
        if inside:
            continue
        n = norm_line(ln)
        m = HEAD_RE.match(n)
        if m:
            out.append((i, len(m.group(1)), n))
    return lines, out


def sections(text, norm):
    lines, hs = heads_of(text)
    secs = {}
    first2 = next((i for i, lv, _ in hs if lv == 2), len(lines))
    secs["preámbulo"] = norm("\n".join(lines[:first2]))
    seen = {}
    for k, (i, lv, n) in enumerate(hs):
        end = len(lines)
        for (j, lv2, _) in hs[k + 1:]:
            if lv2 <= lv:
                end = j
                break
        seen[n] = seen.get(n, 0) + 1
        if seen[n] == 1:
            secs[n] = norm("\n".join(lines[i:end]))
    return secs


def n1(t):
    return " ".join(t.replace("\r\n", "\n").split())


def n2(t):
    return "\n".join(re.sub(r"[ \t]+", " ", ln) for ln in t.replace("\r\n", "\n").split("\n"))


def n3(t):
    return "\n".join(re.sub(r"[ \t]+", " ", ln).strip() for ln in t.replace("\r\n", "\n").split("\n")).strip()


def body163(text):
    lines, hs = heads_of(text)
    idx = [k for k, (i, lv, n) in enumerate(hs) if lv == 3 and (n == "### 16.3" or n.startswith("### 16.3 "))]
    if len(idx) != 1:
        return None
    k = idx[0]
    i, lv, _ = hs[k]
    end = next((j for (j, lv2, _) in hs[k + 1:] if lv2 <= lv), len(lines))
    return "\n".join(lines[i + 1:end])


def check_c12(pairs, ap_pair):
    out = []
    for path, t1, t2 in pairs:
        res = {}
        for name, nf in (("N1", n1), ("N2", n2), ("N3", n3)):
            s1, s2 = sections(t1, nf), sections(t2, nf)
            res[name] = sorted(h for h in set(s1) & set(s2) if s1[h] != s2[h])
        out.append({"Path": path, "AgreeN1N2": res["N1"] == res["N2"], "AgreeN1N3": res["N1"] == res["N3"],
                    "ModifiedCount": len(res["N1"])})
    mv7 = None
    if ap_pair is not None:
        b1, b2 = body163(ap_pair[0]), body163(ap_pair[1])
        if b1 is not None and b2 is not None:
            def eq(nf):
                e = nf(b2)
                p = nf(POINTER)
                k = e.rfind(p)
                e2 = nf(e[:k] + e[k + len(p):]) if k >= 0 else e
                return e2 == nf(b1)
            mv7 = {"N1": eq(n1), "N2": eq(lambda t: n2(t).rstrip()), "N3": eq(n3)}
    trig = any(not (r["AgreeN1N2"] and r["AgreeN1N3"]) for r in out) or \
        (mv7 is not None and len(set(mv7.values())) != 1)
    return {"PerFile": out, "MV7Equality": mv7, "Triggered": trig}


CON_RES = {
    "i_indented_heading_like": re.compile(r"^\s+#{1,6}(\s|$)"),
    "ii_tilde_fence": re.compile(r"^ {0,3}~{3,}"),
    "iii_indented_backtick_fence": re.compile(r"^ {1,3}`{3,}"),
    "vi_container_heading": re.compile(r"^\s*(>|[-*+]|\d+[.)])\s+#{1,6}(\s|$)"),
}
SETEXT_RE = re.compile(r"^ {0,3}(=+|-+)[ \t]*$")


def constructs(text):
    lines = text.replace("\r\n", "\n").split("\n")
    found = {k: [] for k in list(CON_RES) + ["iv_setext", "v_heading_in_html_block", "vii_fence_mismatch",
                                             "viii_heading_nonascii_space_or_tab"]}
    inside, open_len, in_html = False, 0, False
    prev = ""
    for i, ln in enumerate(lines):
        for k, rx in CON_RES.items():
            if rx.match(ln):
                found[k].append(i + 1)
        if ln.startswith("```"):
            run = len(ln) - len(ln.lstrip("`"))
            if not inside:
                inside, open_len = True, run
            else:
                if run != open_len or ln[run:].strip():
                    found["vii_fence_mismatch"].append(i + 1)
                inside = False
            prev = ln
            continue
        if inside:
            prev = ln
            continue
        n = norm_line(ln)
        heading_like = bool(HEAD_RE.match(n))
        if heading_like and (("\t" in ln) or any(ch.isspace() and ord(ch) > 127 for ch in ln)):
            found["viii_heading_nonascii_space_or_tab"].append(i + 1)
        if re.match(r"^ {0,3}<", ln):
            in_html = True
        elif ln.strip() == "":
            in_html = False
        elif in_html and heading_like:
            found["v_heading_in_html_block"].append(i + 1)
        if SETEXT_RE.match(ln) and prev.strip() and not HEAD_RE.match(norm_line(prev)) \
                and not prev.lstrip().startswith(("|", "-", "*", "+", ">")) and not re.match(r"^\s*\d+[.)]\s", prev) \
                and not prev.startswith("```"):
            found["iv_setext"].append(i + 1)
        prev = ln
    if inside:
        found["vii_fence_mismatch"].append(-1)
    return {k: v[:10] for k, v in found.items() if v}


def check_c13(named_texts):
    per = {name: constructs(t) for name, t in named_texts}
    return {"PerFile": per, "Triggered": any(per.values())}


# ------------------------------------------------------------------ C-16 / C-18 / C-23: sintaxis de alcance
def entry_problems(e):
    probs = []
    if not isinstance(e, str) or e == "":
        return ["empty"]
    if e != e.strip() or any(ord(c) < 32 for c in e):
        probs.append("edge-space-or-control")
    if e.startswith("/") or re.match(r"^[A-Za-z]:", e):
        probs.append("absolute")
    if "\\" in e:
        probs.append("backslash")
    core = e[:-3] if e.endswith("/**") else e
    if any(c in core for c in "*?["):
        probs.append("non-final-wildcard")
    body = core[:-1] if core.endswith("/") else core
    if any(seg in ("", ".", "..") for seg in body.split("/")):
        probs.append("dot-or-empty-segment")
    return probs


def check_c16(label_lists):
    out = {}
    trig = False
    for label, obj in label_lists:
        bad = {}
        for key in ("AllowedWriteScope", "ForbiddenWriteScope"):
            for e in obj.get(key, []):
                p = entry_problems(e)
                if p:
                    bad.setdefault(key, []).append({"Entry": e, "Problems": p})
        out[label] = bad
        trig = trig or bool(bad)
    nonfinal = any(any("non-final-wildcard" in x["Problems"] for x in v) for b in out.values() for v in b.values())
    return {"PerInput": out, "Triggered": trig, "C18Triggered": nonfinal}


# ------------------------------------------------------------------ C-24: renombres y rutas imprimibles
def check_c24(repo, cases):
    out = []
    trig = False
    for label, b, c in cases:
        if not (is_commit(repo, b) and is_commit(repo, c)):
            out.append({"Case": label, "Applicable": False, "Why": "BaseSha or CurrentSha is not an existing commit"})
            continue
        d_def = git(repo, ["-c", "diff.renames=true", "diff", "--name-only", b + ".." + c]).decode("utf-8", "replace").split("\n")
        d_nor = git(repo, ["diff", "--name-only", "--no-renames", b + ".." + c]).decode("utf-8", "replace").split("\n")
        s1, s2 = sorted(x for x in d_def if x), sorted(x for x in d_nor if x)
        printable = all(all(32 <= ord(ch) < 127 and ch not in "\"\\" for ch in p) for p in s2)
        t = s1 != s2 or not printable
        trig = trig or t
        out.append({"Case": label, "Applicable": True, "RenameDetectedEqualsNoRenames": s1 == s2,
                    "PrintableAscii": printable, "Paths": len(s2), "Triggered": t})
    return {"PerCase": out, "Triggered": trig}


# ------------------------------------------------------------------ bench
RESULT = {"Schema": "i62-r12-proposals-probe/1"}
bench = {}
c01 = check_c01(BENCH, BENCH_MAIN)
bench["C01"] = c01
tcommit = c01["CommitsMentioningKey"][0] if len(c01["CommitsMentioningKey"]) == 1 else None
cands = derive_eff(BENCH, BENCH_MAIN, tcommit) if tcommit else []
bench["Derive"] = {"Candidates": [x[0] for x in cands]}
eff, eff1 = cands[0] if len(cands) == 1 else (None, None)
t2, t1 = tree(BENCH, eff), tree(BENCH, eff1)
bench["C04"] = check_c04(t2)
mp = json.loads(blob_bench(t2[MAP_PATH][2]).decode("utf-8"))
bench["C10"] = check_c10(t1, t2, mp)
pairs, named = [], []
for f in mp["Files"]:
    if f["FileKind"] != "MODIFIED":
        continue
    p = f["Path"]
    a = blob_bench(t1[p][2]).decode("utf-8", "surrogateescape") if p in t1 else None
    b = blob_bench(t2[p][2]).decode("utf-8", "surrogateescape") if p in t2 else None
    if a is not None and b is not None:
        pairs.append((p, a, b))
    if a is not None:
        named.append(("EFF^1:" + p, a))
    if b is not None:
        named.append(("EFF:" + p, b))
apb = (blob_bench(t1[AP_PATH][2]).decode("utf-8"), blob_bench(t2[AP_PATH][2]).decode("utf-8"))
bench["C12"] = check_c12(pairs, apb)
bench["C13"] = check_c13(named)
k_raw = git(BENCH, ["cat-file", "blob", BENCH_CONTRACT[0] + ":" + BENCH_CONTRACT[1]])
inputs = [("contract-T1", json.loads(k_raw.decode("utf-8"))),
          ("kit-deleg", json.loads(open("D:/r62-a6m7/kit/inputs/deleg.json", "rb").read().decode("utf-8")))]
cases = []
for d in sorted(glob.glob("D:/r62-a6m7/bench/work/in-*")):
    lab = os.path.basename(d)
    dp, hp = os.path.join(d, "deleg.json"), os.path.join(d, "handoff.json")
    if os.path.exists(dp):
        g = json.loads(open(dp, "rb").read().decode("utf-8"))
        inputs.append((lab, g))
        if os.path.exists(hp):
            h = json.loads(open(hp, "rb").read().decode("utf-8"))
            cases.append((lab, g["BaseSha"], h.get("CurrentSha") or ""))
bench["C16"] = check_c16(inputs)
bench["C24"] = check_c24(BENCH, cases)
RESULT["Bench"] = bench

# ------------------------------------------------------------------ B1 (parte fijada por A5-1)
m_b1 = json.loads(blob_w(MAP_BLOB).decode("utf-8"))
b1 = {}
b1["C04"] = {"MapBlob": MAP_BLOB, "SchemaBlob": SCHEMA_BLOB, "Triggered": False,
             "Basis": "A5-1 rule 6: map and schema copied byte for byte"}
b1["C10"] = {"EntryFilesPresentInEff1": [], "Triggered": False, "Basis": "A5-1 rule 1: no ENTRY file in F_seed"}
pairs_b1, named_b1 = [], []
for f in m_b1["Files"]:
    if f["FileKind"] != "MODIFIED":
        continue
    a = blob_w(f["BaseBlob"]).decode("utf-8", "surrogateescape")
    b = blob_w(f["EffBlob"]).decode("utf-8", "surrogateescape")
    pairs_b1.append((f["Path"], a, b))
    named_b1 += [("EFF^1:" + f["Path"], a), ("EFF:" + f["Path"], b)]
named_b1.append(("EFF:" + WF_PATH, blob_w(WF_EFF_BLOB).decode("utf-8")))
ap_base = [f for f in m_b1["Files"] if f["Path"] == AP_PATH][0]["BaseBlob"]
b1["C12"] = check_c12(pairs_b1, (blob_w(ap_base).decode("utf-8"), blob_w(AP_EFF_BLOB).decode("utf-8")))
b1["C13"] = check_c13(named_b1)
RESULT["B1"] = b1

data = (json.dumps(RESULT, ensure_ascii=True, sort_keys=True, indent=1) + "\n").encode("ascii")
for bad in (os.environ.get("USERNAME", "\0"), os.environ.get("USERPROFILE", "\0")):
    if bad and bad.encode("utf-8", "replace") in data:
        raise SystemExit("hygiene: user name in output")
with open(OUT, "wb") as fh:
    fh.write(data)
print("wrote " + os.path.basename(OUT) + " sha256 " + hashlib.sha256(data).hexdigest())
