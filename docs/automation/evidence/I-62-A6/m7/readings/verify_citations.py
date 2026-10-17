# verify_citations.py - mechanical check of every citation in readings.json (I-62, A-6, A6-5 rule 6 readings).
#
# Every quoted text or code token must be on the cited lines of the PINNED bytes of its source, read by blob with
# `git -C <repo> --no-replace-objects cat-file blob <blob>` (never from a working tree). A "quote" must appear with
# whitespace collapsed (blockquote markers removed) in the cited line range; a "token" must appear verbatim.
# Also checked: each source's own blob SHA-1 and SHA-256 and its identity at the recorded worktree HEAD; the
# AUTOMATION_PLAN blob at the fixture's AuthorityRevision; the clause map and schema at the fixture EFF; that V14
# never mentions U-19; the probe results; the verdict counts; and output hygiene (no user name, no e-mail).
# Read-only on every input; writes only citations-result.json next to this file. Run: python -I verify_citations.py
import hashlib
import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
W = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
WLABEL = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"
FIX = "D:/r62-a6r2/r3/deliv"
AP_BLOB = "f525cb1e9d24db3cf5fa5c8e013be7edbdb5e6a5"
MAP_BLOB = "4d49d3e1a63d2b6128c9d9eeac473156a421571a"
SCHEMA_BLOB = "6b54dda9579d694200d8fc9ec26a4a846eef529b"
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
SCHEMA_PATH = "docs/automation/agent-execution/compatibility/clause-map.schema.json"


def git(repo, *args):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    env["GIT_OPTIONAL_LOCKS"] = "0"
    p = subprocess.run(["git", "-C", repo, "--no-replace-objects"] + list(args), stdout=subprocess.PIPE,
                       stderr=subprocess.PIPE, env=env, shell=False)
    return p.returncode, p.stdout


def blob_id(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode("ascii") + b"\0" + data).hexdigest()


def norm(text):
    return " ".join(text.split())


def unquote_line(line):
    return re.sub(r"^\s*(>\s*)+", "", line)


def main():
    if sys.flags.isolated != 1:
        raise SystemExit("run with python -I")
    with open(os.path.join(HERE, "readings.json"), "rb") as fh:
        doc = json.loads(fh.read().decode("utf-8"))
    head = doc["Worktree"]["Head"]
    checks = []
    texts = {}

    def record(cid, ok, **kw):
        item = {"Id": cid, "OK": bool(ok)}
        item.update(kw)
        checks.append(item)

    # 1. sources by blob
    for key, meta in sorted(doc["Sources"].items()):
        rc, data = git(W, "cat-file", "blob", meta["Blob"])
        ok = rc == 0 and blob_id(data) == meta["Blob"] and hashlib.sha256(data).hexdigest() == meta["Sha256"]
        rc2, out = git(W, "rev-parse", head + ":" + meta["Path"])
        at_head = rc2 == 0 and out.decode("ascii", "replace").strip() == meta["Blob"]
        record("source:" + key, ok and at_head, Kind="source", Blob=meta["Blob"], Sha256=meta["Sha256"],
               PathAtHead=at_head)
        texts[key] = data.decode("utf-8", "surrogateescape").split("\n") if ok else None

    # 2. citations
    def check_cite(cid, c):
        lines = texts.get(c["Src"])
        a, b = c["Lines"]
        if lines is None or a < 1 or b < a or b > len(lines):
            record(cid, False, Kind=c["Kind"], Src=c["Src"], Lines=[a, b], Problem="source or range unavailable")
            return
        chunk = lines[a - 1:b]
        if c["Kind"] == "quote":
            hay = norm(" ".join(unquote_line(x) for x in chunk))
            ok = norm(c["Text"]) in hay
        else:
            ok = c["Text"] in "\n".join(chunk)
        record(cid, ok, Kind=c["Kind"], Src=c["Src"], Lines=[a, b], Text=c["Text"])

    for row in doc["Rows"]:
        for field in ("Normative", "AcceptorCode", "InstrumentCode", "Evidence"):
            for i, c in enumerate(row.get(field, [])):
                check_cite("%s/%s/%d" % (row["Id"], field, i), c)
    for j, o in enumerate(doc.get("OutsideRule6Readings", [])):
        for i, c in enumerate(o["Cites"]):
            check_cite("outside/%d/%d" % (j, i), c)
    for x in doc.get("Auxiliary", []):
        check_cite("aux/" + x["Id"], x["Cite"])

    # 3. AUTOMATION_PLAN at the fixture's AuthorityRevision (and EFF, tip); map and schema at EFF
    for rev in ("1746b404", "fbe25347", "95bdc29d"):
        rc, out = git(FIX, "rev-parse", rev + ":docs/AUTOMATION_PLAN.md")
        got = out.decode("ascii", "replace").strip()
        record("fixture:AP@" + rev, rc == 0 and got == AP_BLOB, Kind="identity", Expected=AP_BLOB, Got=got)
    for path, want in ((MAP_PATH, MAP_BLOB), (SCHEMA_PATH, SCHEMA_BLOB)):
        rc, out = git(FIX, "rev-parse", "fbe25347:" + path)
        got = out.decode("ascii", "replace").strip()
        rc2, out2 = git(W, "rev-parse", head + ":" + path)
        got2 = out2.decode("ascii", "replace").strip()
        record("fixture:EFF:" + path.rsplit("/", 1)[1], rc == 0 and rc2 == 0 and got == want == got2, Kind="identity",
               Expected=want, GotFixture=got, GotWorktree=got2)
    rc, out = git(W, "rev-parse", head + ":docs/AUTOMATION_PLAN.md")
    record("worktree:AP@HEAD", out.decode("ascii", "replace").strip() == AP_BLOB, Kind="identity")

    # 4. U-19 is not in V14 (its source is decisiones 62)
    v14 = texts.get("V14")
    record("V14:no-U-19", v14 is not None and not any("U-19" in x for x in v14), Kind="absence")

    # 5. probe results
    ppath = os.path.join(HERE, "probes", "probe-result.json")
    try:
        with open(ppath, "rb") as fh:
            pdata = fh.read()
        probe = json.loads(pdata.decode("ascii"))
        cases = dict((c["Name"], c) for c in probe["Cases"])
        must = {"P1-mirrored-T1": ("ACCEPT_FACTS", "MATCH"), "P2-nc2-like-T1": ("REJECT_FACTS", "MISMATCH"),
                "P3-synthetic-nested": ("REJECT_FACTS", "MISMATCH")}
        for name, (dec, r7) in sorted(must.items()):
            c = cases[name]
            st = c["Statuses"]
            ok = (c["Decision"] == dec and st.get("R7") == r7 and all(st.get(k) == "MATCH" for k in ("R1", "R2", "R3", "R9"))
                  and c["ToolExit"] == 0)
            if r7 == "MISMATCH":
                ok = ok and len(c["R7Mismatches"]) > 0 and all("ContainedBy" in m["Field"] and m["Tool"] == []
                                                               for m in c["R7Mismatches"])
            record("probe:" + name, ok, Kind="probe", Decision=c["Decision"], R7=st.get("R7"),
                   R7Mismatches=len(c["R7Mismatches"]))
        record("probe:pinned", probe["Pinned"]["Tool"]["Blob"] == doc["Instrument"]["Blob"]
               and probe["Pinned"]["Acceptor"]["Blob"] == doc["Acceptor"]["Blob"], Kind="probe",
               Sha256=hashlib.sha256(pdata).hexdigest())
    except (OSError, ValueError, KeyError) as exc:
        record("probe:read", False, Kind="probe", Problem=exc.__class__.__name__)

    # 6. verdict counts
    cnt = {"COINCIDE": 0, "AMBIGUOUS-ACCEPTOR-CHOICE": 0, "DIVERGE": 0}
    for row in doc["Rows"]:
        cnt[row["Verdict"]] += 1
    record("verdict-counts", cnt == doc["VerdictCounts"], Kind="consistency", Counts=cnt)

    # 7. hygiene of the deliverables (the user name itself is never written here)
    user = os.environ.get("USERNAME", "")
    mail = re.compile(rb"[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+\.[A-Za-z0-9.-]+")
    bad = []
    for root, _dirs, files in os.walk(HERE):
        for f in files:
            if f == "citations-result.json":
                continue
            p = os.path.join(root, f)
            with open(p, "rb") as fh:
                data = fh.read()
            rel = os.path.relpath(p, HERE).replace("\\", "/")
            if len(user) > 2 and user.lower().encode("utf-8") in data.lower():
                bad.append(rel + ": user name")
            if mail.search(data):
                bad.append(rel + ": e-mail")
    record("hygiene", not bad, Kind="hygiene", Problems=bad)

    total = len(checks)
    good = sum(1 for c in checks if c["OK"])
    cites = [c for c in checks if c["Kind"] in ("quote", "token")]
    result = {"Schema": "rackcad-i62-a6m7-citations/0-experimental", "Worktree": WLABEL, "Head": head,
              "Summary": {"Checks": total, "Verified": good, "Failed": total - good,
                          "Citations": len(cites), "CitationsVerified": sum(1 for c in cites if c["OK"]),
                          "AllVerified": good == total},
              "Checks": checks}
    text = json.dumps(result, ensure_ascii=True, sort_keys=True, indent=1) + "\n"
    with open(os.path.join(HERE, "citations-result.json"), "wb") as fh:
        fh.write(text.encode("ascii"))
    print("checks %d, verified %d, failed %d" % (total, good, total - good))
    for c in checks:
        if not c["OK"]:
            print("FAIL", c["Id"], c.get("Src", ""), c.get("Lines", ""))
    return 0 if good == total else 1


if __name__ == "__main__":
    sys.exit(main())
