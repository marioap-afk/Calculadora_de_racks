#!/usr/bin/env python3
"""I-52 CT-21D baseline artifact BA-07 V4: canonical serialization vector and worked custody example.

Usage:
    python I-52-ct21d-ba07-v4-jcs-vector.py <output.json>

What it does (deterministic; no network, no git, no host, no AutoCAD):
  1. Implements the BA-07 V4 section 4 canonical serialization: JCS (RFC 8785) restricted to the
     BA-07 profile (ASCII member names, integers only in [-(2**53-1), 2**53-1], doubles carried as a
     JSON string of 16 lowercase hexadecimal digits of the IEEE-754 binary64 bit pattern, big-endian).
  2. Checks the string escaping against the string example of RFC 8785 section 3.2.2.2, and every
     serialized object against Python json.dumps (equal for this profile, because member names are
     ASCII and no JSON number is a non-integer).
  3. Builds the mandatory worked example of BA-07 V4 section 12 (genesis -> approval -> anchor ->
     manifest -> anchor -> IN_USE x2 -> re-anchor -> pins -> re-anchor), plus a continuation (a use
     with pins and a new baseline version), and prints the JCS bytes and SHA-256 of the example entry
     and the entry hashes of the whole chain.
  4. Runs a model of the verification algorithm of BA-07 V4 section 6.2 on the honest example and on
     four tampered variants.

Every hash, object id, time, actor, run id and value of the example is ILLUSTRATIVE: it is derived
from a label such as "EXAMPLE:BA-11 seal v1" and is not a hash of any real artifact. The normative
content of this file is the serialization profile and its vectors (the bytes and hashes it prints).
"""

import hashlib
import json
import os
import re
import struct
import sys

GENERATOR_PATH = "docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.py"
REMOTE_URL = "https://github.com/marioap-afk/Calculadora_de_racks.git"
LOG_PATH = "docs/automation/evidence/ct21d/custody-log.jsonl"
ANCHOR_PATH = "docs/automation/evidence/ct21d/baseline-anchor.json"
PINS_DIR = "docs/automation/evidence/ct21d/pins/"
RATIFICATIONS_DIR = "docs/automation/evidence/ct21d/ratifications/"
ZERO64 = "0" * 64
MAX_INT = 2 ** 53 - 1


class ProfileError(Exception):
    pass


# ---------------------------------------------------------------------------------------------
# 1. Canonical serialization (BA-07 V4 section 4): JCS, RFC 8785, BA-07 profile
# ---------------------------------------------------------------------------------------------

def jcs_string(s):
    out = ['"']
    for ch in s:
        o = ord(ch)
        if 0xD800 <= o <= 0xDFFF:
            raise ProfileError("lone surrogate in string")
        if ch == '"':
            out.append('\\"')
        elif ch == '\\':
            out.append('\\\\')
        elif ch == '\b':
            out.append('\\b')
        elif ch == '\t':
            out.append('\\t')
        elif ch == '\n':
            out.append('\\n')
        elif ch == '\f':
            out.append('\\f')
        elif ch == '\r':
            out.append('\\r')
        elif o < 0x20:
            out.append('\\u%04x' % o)
        else:
            out.append(ch)
    out.append('"')
    return ''.join(out)


def jcs(v):
    if v is None:
        return 'null'
    if v is True:
        return 'true'
    if v is False:
        return 'false'
    if isinstance(v, int):
        if not -MAX_INT <= v <= MAX_INT:
            raise ProfileError("integer outside [-(2**53-1), 2**53-1]")
        return str(v)
    if isinstance(v, float):
        raise ProfileError("a double must be carried as its 16-hex string (double_hex), never as a JSON number")
    if isinstance(v, str):
        return jcs_string(v)
    if isinstance(v, list):
        return '[' + ','.join(jcs(x) for x in v) + ']'
    if isinstance(v, dict):
        for k in v:
            if not isinstance(k, str) or any(ord(c) > 0x7E or ord(c) < 0x20 for c in k):
                raise ProfileError("member names must be printable ASCII")
        keys = sorted(v.keys(), key=lambda k: k.encode('utf-16-be'))  # RFC 8785 3.2.3: UTF-16 code units
        return '{' + ','.join(jcs_string(k) + ':' + jcs(v[k]) for k in keys) + '}'
    raise ProfileError("type not admitted by the profile: %r" % type(v))


CROSS_CHECKS = {"objects": 0, "equal": 0}


def jcs_bytes(v):
    s = jcs(v)
    ref = json.dumps(v, sort_keys=True, separators=(',', ':'), ensure_ascii=False)
    CROSS_CHECKS["objects"] += 1
    if ref == s:
        CROSS_CHECKS["equal"] += 1
    return s.encode('utf-8')


def sha256_hex(b):
    return hashlib.sha256(b).hexdigest()


def record_file_bytes(obj):
    """A record file (anchor, pin, ratification, manifest layer, tuple record) = JCS bytes + one LF."""
    return jcs_bytes(obj) + b"\n"


def record_hash(obj):
    """The hash of a record = SHA-256 of its file bytes (JCS bytes followed by one LF)."""
    return sha256_hex(record_file_bytes(obj))


def double_hex(x):
    if not isinstance(x, float):
        raise ProfileError("not a double")
    if x != x or x in (float('inf'), float('-inf')):
        raise ProfileError("NaN and infinities are not representable")
    return struct.pack('>d', x).hex()


# ---------------------------------------------------------------------------------------------
# 2. Custody-log entries (BA-07 V4 section 6.1)
# ---------------------------------------------------------------------------------------------

ENTRY_FIELDS = [
    "actor", "anchorRef", "baselineCustodyHash", "baselineVersion", "captureId", "cause", "entryHash",
    "event", "index", "note", "pinRecordHashes", "pinSlot", "prevEntryHash", "ratificationRecords",
    "role", "runId", "subjectHash", "subjectKind", "supersededBy", "tupleRecordHash", "utc",
]

# where each conditional field is non-null (every other entry carries null)
CONDITIONAL = {
    "anchorRef": {"ANCHOR_RECORDED"},
    "baselineCustodyHash": {"BASELINE_RECORDED"},
    "baselineVersion": {"BASELINE_RECORDED", "BASELINE_APPROVED"},
    "cause": {"REVOKED"},
    "pinRecordHashes": {"IN_USE"},
    "pinSlot": {"PIN_RECORDED"},
    "ratificationRecords": {"BASELINE_APPROVED", "PIN_RECORDED"},
    "runId": {"IN_USE"},
    "supersededBy": {"SUPERSEDED"},
    "tupleRecordHash": {"IN_USE"},
}

EVENT_ROLE = {
    "BASELINE_RECORDED": "CUSTODY_ROLE", "BASELINE_APPROVED": "APPROVAL_ROLE",
    "ANCHOR_RECORDED": "CUSTODY_ROLE", "PIN_RECORDED": "APPROVAL_ROLE",
    "REVIEW_RECORD_ADDED": "CUSTODY_ROLE", "AUTHORIZED": "APPROVAL_ROLE", "CAPTURED": "CAPTURE_ROLE",
    "REVIEWED": "REVIEW_ROLE", "APPROVED": "APPROVAL_ROLE", "IN_USE": "CUSTODY_ROLE",
    "SUPERSEDED": "APPROVAL_ROLE", "REVOKED": "APPROVAL_ROLE",
}

EVENT_KINDS = {
    "BASELINE_RECORDED": {"BASELINE"}, "BASELINE_APPROVED": {"BASELINE"}, "ANCHOR_RECORDED": {"ANCHOR"},
    "PIN_RECORDED": {"PIN"}, "REVIEW_RECORD_ADDED": {"REVIEW_RECORD"}, "AUTHORIZED": {"MANIFEST"},
    "CAPTURED": {"MANIFEST"}, "REVIEWED": {"MANIFEST"}, "APPROVED": {"MANIFEST"}, "IN_USE": {"MANIFEST"},
    "SUPERSEDED": {"BASELINE", "PIN", "MANIFEST"}, "REVOKED": {"PIN", "MANIFEST"},
}

# per-subject transitions: previous event (None = first entry of the subject) -> allowed next events
TRANSITIONS = {
    "BASELINE": {None: {"BASELINE_RECORDED"}, "BASELINE_RECORDED": {"BASELINE_APPROVED"},
                 "BASELINE_APPROVED": {"SUPERSEDED"}},
    "ANCHOR": {None: {"ANCHOR_RECORDED"}},
    "PIN": {None: {"PIN_RECORDED"}, "PIN_RECORDED": {"SUPERSEDED", "REVOKED"}},
    "REVIEW_RECORD": {None: {"REVIEW_RECORD_ADDED"}},
    "MANIFEST": {None: {"AUTHORIZED"}, "AUTHORIZED": {"CAPTURED", "REVOKED"},
                 "CAPTURED": {"REVIEWED", "REVOKED"}, "REVIEWED": {"APPROVED", "REVOKED"},
                 "APPROVED": {"IN_USE", "REVOKED"}, "IN_USE": {"IN_USE", "SUPERSEDED", "REVOKED"}},
}


def entry_hash(entry):
    body = {k: v for k, v in entry.items() if k != "entryHash"}
    return sha256_hex(jcs_bytes(body))


def make_entry(index, utc, role, actor, event, subject_kind, subject_hash, prev, note="", **cond):
    e = {k: None for k in ENTRY_FIELDS}
    e.update(index=index, utc=utc, role=role, actor=actor, event=event, subjectKind=subject_kind,
             subjectHash=subject_hash, prevEntryHash=prev, note=note)
    for k, v in cond.items():
        e[k] = v
    e["entryHash"] = entry_hash(e)
    return e


# ---------------------------------------------------------------------------------------------
# 3. Illustrative values
# ---------------------------------------------------------------------------------------------

def ill(label):
    return sha256_hex(("EXAMPLE:" + label).encode('utf-8'))


def ill_git(label):
    return hashlib.sha1(("EXAMPLE:" + label).encode('utf-8')).hexdigest()


def utc_of(n):
    return "2000-01-01T%02d:%02d:00Z" % (n // 60, n % 60)


COORD = "EXAMPLE-COORDINATOR"
OWNER = "EXAMPLE-OWNER"
CP = "CONTROL_PLANE:EXAMPLE-BUILD"


class Example:
    def __init__(self, designated=True):
        self.entries = []
        self.store = {}            # path -> bytes, at the verified revision
        self.commits = {}          # peeled commit -> {path: bytes}
        self.remote_tags = []      # ls-remote view: {name, tagObjectId, peeledCommit, annotated}
        self.independent_copy = [] # what the designated holder keeps
        self.clock = 0
        self.designation = None
        if designated:
            rec = {"recordKind": "CT21D_RATIFICATION", "ratifier": "COORDINATOR", "actor": COORD,
                   "subjectKind": "ARTIFACT", "subject": "CT21D-BASE-MANIFEST-CUSTODY",
                   "subjectHash": ill("BA-07 candidate"), "decision": "RATIFIED",
                   "utc": utc_of(0), "reference": "EXAMPLE decisions-log reference",
                   "independentCopy": {"holder": "EXAMPLE-HOLDER (illustrative)",
                                       "location": "EXAMPLE-LOCATION (illustrative, outside the repository)"}}
            path = RATIFICATIONS_DIR + "CT21D-BASE-MANIFEST-CUSTODY-%s-coordinator.json" % ill("BA-07 candidate")
            self.store[path] = record_file_bytes(rec)
            self.designation = {"designationRecord": {"path": path, "sha256": record_hash(rec)},
                                "holder": rec["independentCopy"]["holder"],
                                "location": rec["independentCopy"]["location"]}

    def tick(self):
        self.clock += 1
        return utc_of(self.clock)

    def prev(self):
        return self.entries[-1]["entryHash"] if self.entries else ZERO64

    def append(self, role, actor, event, kind, subject_hash, note="", **cond):
        e = make_entry(len(self.entries), self.tick(), role, actor, event, kind, subject_hash, self.prev(),
                       note, **cond)
        self.entries.append(e)
        return e

    def log_bytes(self, upto=None):
        es = self.entries if upto is None else self.entries[:upto + 1]
        return b"".join(jcs_bytes(e) + b"\n" for e in es)

    def current_baseline(self, h):
        rec = app = None
        for e in self.entries[:h + 1]:
            if e["event"] == "BASELINE_RECORDED":
                rec = e
            if e["event"] == "BASELINE_APPROVED":
                app = e
        return rec, app

    def anchor(self, version, seq, note=""):
        h = len(self.entries) - 1
        rec_entry, app_entry = self.current_baseline(h)
        name = "ct21d-baseline/%s" % version + ("" if seq == 0 else "-a%d" % seq)
        record = {
            "RECORD_KIND": "CT21D_BASELINE_ANCHOR",
            "ANCHOR_TAG_NAME": name,
            "ANCHOR_SEQUENCE": seq,
            "BASELINE_VERSION": version,
            "CUSTODY_LOG_HEAD_INDEX": h,
            "MANIFEST_CUSTODY_HEADER_HASH": self.entries[h]["entryHash"],
            "BA11_SEAL_HASH": app_entry["subjectHash"],
            "BA07_SEAL_HASH": rec_entry["baselineCustodyHash"],
            "CONTRACT_COMPOSITE_HASH": ill("contract composite (BA-01 SEAL_HASH)"),
            "REMOTE_URL": REMOTE_URL,
            "INDEPENDENT_COPY": self.designation if self.designation else "UNDESIGNATED",
        }
        rbytes = record_file_bytes(record)
        peeled = ill_git("anchor commit " + name)
        tag_obj = ill_git("tag object " + name)
        self.commits[peeled] = {ANCHOR_PATH: rbytes, LOG_PATH: self.log_bytes(h)}
        self.store[ANCHOR_PATH] = rbytes
        self.remote_tags.append({"name": name, "tagObjectId": tag_obj, "peeledCommit": peeled, "annotated": True})
        ref = {"anchoredIndex": h, "peeledCommit": peeled, "remote": REMOTE_URL, "tag": name, "tagObjectId": tag_obj}
        self.append("CUSTODY_ROLE", COORD, "ANCHOR_RECORDED", "ANCHOR", sha256_hex(rbytes), note, anchorRef=ref)
        if self.designation:
            self.independent_copy.append({"tag": name, "tagObjectId": tag_obj, "peeledCommit": peeled,
                                          "anchoredIndex": h, "anchorRecordSha256": sha256_hex(rbytes)})
        return record

    def pin(self, pin_kind, slot, value, version, sources):
        rec = {"recordKind": "CT21D_PIN_RECORD", "pinKind": pin_kind, "pinSlot": slot, "baselineVersion": version,
               "value": value, "sourceEvidence": sorted(sources, key=lambda s: s["path"]),
               "inventoryReviewRecords": None, "supersedes": None, "frozenUtc": self.tick(),
               "recordedBy": CP}
        ph = record_hash(rec)
        self.store[PINS_DIR + "%s-%s.json" % (pin_kind, ph)] = record_file_bytes(rec)
        rats = []
        for who, actor in (("coordinator", COORD), ("architect", "EXAMPLE-ARCHITECT")):
            r = {"recordKind": "CT21D_RATIFICATION", "ratifier": who.upper(), "actor": actor,
                 "subjectKind": "PIN", "subject": slot, "subjectHash": ph, "decision": "RATIFIED",
                 "utc": self.tick(), "reference": "EXAMPLE decisions-log reference", "independentCopy": None}
            path = RATIFICATIONS_DIR + "PIN-%s-%s-%s.json" % (pin_kind, ph, who)
            self.store[path] = record_file_bytes(r)
            rats.append({"path": path, "sha256": record_hash(r)})
        rats.sort(key=lambda x: x["path"])
        self.append("APPROVAL_ROLE", COORD, "PIN_RECORDED", "PIN", ph, pinSlot=slot, ratificationRecords=rats)
        return ph


def baseline_ratifications(version):
    out = []
    for kind in ("candidate", "coordinator", "architect", "seal"):
        path = RATIFICATIONS_DIR + "CT21D-BASE-HASH-REGISTRY-%s-%s.json" % (ill("BA-11 seal " + version), kind)
        out.append({"path": path, "sha256": ill("BA-11 %s record %s" % (kind, version))})
    out.sort(key=lambda x: x["path"])
    return out


def build_example(designated=True):
    ex = Example(designated)
    # -- mandatory sequence (BA-07 V4 section 12) --
    ex.append("CUSTODY_ROLE", COORD, "BASELINE_RECORDED", "BASELINE", ill("BA-11 seal v1"),
              baselineCustodyHash=ill("BA-07 seal v1"), baselineVersion="v1")                       # 0
    ex.append("APPROVAL_ROLE", COORD, "BASELINE_APPROVED", "BASELINE", ill("BA-11 seal v1"),
              baselineVersion="v1", ratificationRecords=baseline_ratifications("v1"))               # 1
    ex.anchor("v1", 0, note="first anchor of baseline \"v1\" (illustrative) " + chr(0x2014) + " caf" + chr(0xe9))        # A0 h=1 ; 2
    man = ill("manifest identity 1")
    ex.append("APPROVAL_ROLE", COORD, "AUTHORIZED", "MANIFEST", None, captureId="CAP-1")             # 3
    ex.append("CAPTURE_ROLE", CP, "CAPTURED", "MANIFEST", man, captureId="CAP-1")                     # 4
    ex.append("REVIEW_ROLE", OWNER, "REVIEWED", "MANIFEST", man, captureId="CAP-1")                   # 5
    ex.append("APPROVAL_ROLE", COORD, "APPROVED", "MANIFEST", man, captureId="CAP-1")                 # 6
    ex.anchor("v1", 1)                                                                               # A1 h=6 ; 7
    ex.append("CUSTODY_ROLE", CP, "IN_USE", "MANIFEST", man, captureId="CAP-1", runId="EXAMPLE-RUN-1",
              tupleRecordHash=ill("tuple record run 1"), pinRecordHashes=[])                         # 8
    ex.append("CUSTODY_ROLE", CP, "IN_USE", "MANIFEST", man, captureId="CAP-1", runId="EXAMPLE-RUN-2",
              tupleRecordHash=ill("tuple record run 2"), pinRecordHashes=[])                         # 9
    ex.anchor("v1", 2)                                                                               # A2 h=9 ; 10
    p1 = ex.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE", "v1",
                [{"path": "EXAMPLE/evidence/lock-mode-selection.json", "sha256": ill("lock mode evidence")}])  # 11
    p2 = ex.pin("E04_ADMITTED_SET", "E04_ADMITTED_SET@R-1", ["EXAMPLE-MEMBER-1", "EXAMPLE-MEMBER-2"], "v1",
                [{"path": "EXAMPLE/evidence/e4-learn-r1.json", "sha256": ill("e4 learn evidence")}])      # 12
    ex.anchor("v1", 3)                                                                               # A3 h=12 ; 13
    mandatory_len = len(ex.entries)
    # -- continuation: a use with pins, then a new baseline version --
    ex.append("CUSTODY_ROLE", CP, "IN_USE", "MANIFEST", man, captureId="CAP-1", runId="EXAMPLE-RUN-3",
              tupleRecordHash=ill("tuple record run 3"), pinRecordHashes=sorted([p1, p2]))           # 14
    ex.append("CUSTODY_ROLE", COORD, "BASELINE_RECORDED", "BASELINE", ill("BA-11 seal v2"),
              baselineCustodyHash=ill("BA-07 seal v2"), baselineVersion="v2")                       # 15
    ex.append("APPROVAL_ROLE", COORD, "BASELINE_APPROVED", "BASELINE", ill("BA-11 seal v2"),
              baselineVersion="v2", ratificationRecords=baseline_ratifications("v2"))               # 16
    ex.append("APPROVAL_ROLE", COORD, "SUPERSEDED", "BASELINE", ill("BA-11 seal v1"),
              supersededBy=ill("BA-11 seal v2"))                                                     # 17
    ex.anchor("v2", 0)                                                                               # A4 h=17 ; 18
    return ex, mandatory_len


# ---------------------------------------------------------------------------------------------
# 4. Model of the verification algorithm (BA-07 V4 section 6.2)
# ---------------------------------------------------------------------------------------------

TAG_RE =re.compile(r"^ct21d-baseline/v([1-9][0-9]*)(?:-a([1-9][0-9]*))?$")


def verify(log_bytes, remote_tags, commits, store, independent_copy):
    viol = []
    # step 1: canonical lines, indexes, entry hashes
    entries = []
    if log_bytes and not log_bytes.endswith(b"\n"):
        viol.append(("step1", "NON_CANONICAL_ENTRY", "missing final LF"))
    for i, line in enumerate(log_bytes.split(b"\n")[:-1] if log_bytes else []):
        e = json.loads(line.decode('utf-8'))
        if jcs(e).encode('utf-8') != line or sorted(e.keys()) != sorted(ENTRY_FIELDS):
            viol.append(("step1", "NON_CANONICAL_ENTRY", i))
        if e["index"] != i:
            viol.append(("step1", "INDEX_SEQUENCE", i))
        if entry_hash(e) != e["entryHash"]:
            viol.append(("step1", "BROKEN_ENTRY", i))
        entries.append(e)
    # step 2: chain
    for i, e in enumerate(entries):
        want = ZERO64 if i == 0 else entries[i - 1]["entryHash"]
        if e["prevEntryHash"] != want:
            viol.append(("step2", "BROKEN_CHAIN", i))
    # step 3: anchors discovered at the remote, records read at each tag's peeled commit
    anchors = []
    for t in remote_tags:
        m = TAG_RE.match(t["name"])
        if not m:
            viol.append(("step3", "UNEXPECTED_TAG_NAME", t["name"]))
            continue
        if not t["annotated"]:
            viol.append(("step3", "LIGHTWEIGHT_TAG", t["name"]))
            continue
        snap = commits.get(t["peeledCommit"], {})
        rb = snap.get(ANCHOR_PATH)
        if rb is None:
            viol.append(("step3", "ANCHOR_RECORD_MISSING", t["name"]))
            continue
        rec = json.loads(rb.decode('utf-8'))
        if record_file_bytes(rec) != rb:
            viol.append(("step3", "ANCHOR_RECORD_NON_CANONICAL", t["name"]))
        seq = int(m.group(2) or 0)
        if rec["ANCHOR_TAG_NAME"] != t["name"] or rec["ANCHOR_SEQUENCE"] != seq or \
                rec["BASELINE_VERSION"] != "v" + m.group(1):
            viol.append(("step3", "ANCHOR_RECORD_NAME_MISMATCH", t["name"]))
        h, H = rec["CUSTODY_LOG_HEAD_INDEX"], rec["MANIFEST_CUSTODY_HEADER_HASH"]
        snap_log = snap.get(LOG_PATH, b"")
        snap_lines = snap_log.split(b"\n")[:-1]
        if len(snap_lines) != h + 1 or json.loads(snap_lines[-1].decode('utf-8'))["entryHash"] != H:
            viol.append(("step3", "ANCHOR_COMMIT_LOG_MISMATCH", t["name"]))
        anchors.append({"tag": t, "record": rec, "recordSha256": sha256_hex(rb), "h": h, "H": H,
                        "version": int(m.group(1)), "seq": seq})
    anchors.sort(key=lambda a: a["h"])
    for a, b in zip(anchors, anchors[1:]):
        if b["h"] <= a["h"]:
            viol.append(("step3", "ANCHOR_ORDER", b["tag"]["name"]))
    # naming sequence: versions contiguous and increasing; within a version seq = 0, 1, 2, ...
    expect_v, expect_s = 1, 0
    for a in anchors:
        if a["version"] == expect_v and a["seq"] == expect_s:
            expect_s += 1
        elif a["version"] == expect_v + 1 and a["seq"] == 0 and expect_s > 0:
            expect_v, expect_s = expect_v + 1, 1
        else:
            viol.append(("step3", "TAG_SEQUENCE", a["tag"]["name"]))
    for a in anchors:
        if a["h"] >= len(entries):
            viol.append(("step3", "TRUNCATED_BELOW_ANCHOR", a["tag"]["name"]))
        elif entries[a["h"]]["entryHash"] != a["H"]:
            viol.append(("step3", "REWRITTEN_AT_OR_BEFORE_ANCHOR", a["tag"]["name"]))
    # step 4: one-to-one tags <-> ANCHOR_RECORDED; pending window
    status = []
    by_tag = {}
    for e in entries:
        if e["event"] == "ANCHOR_RECORDED":
            by_tag.setdefault(e["anchorRef"]["tag"], []).append(e)
    names = [a["tag"]["name"] for a in anchors]
    for tag_name, es in by_tag.items():
        if tag_name not in names:
            viol.append(("step4", "ANCHOR_WITHOUT_TAG", es[0]["index"]))
        if len(es) > 1:
            viol.append(("step4", "DUPLICATE_ANCHOR_RECORDED", tag_name))
    for k, a in enumerate(anchors):
        es = by_tag.get(a["tag"]["name"], [])
        if not es:
            if k == len(anchors) - 1:
                status.append("LAST_ANCHOR_NOT_RECORDED")
            else:
                viol.append(("step4", "UNRECORDED_ANCHOR", a["tag"]["name"]))
            continue
        e = es[0]
        ref = e["anchorRef"]
        if (ref["tagObjectId"], ref["peeledCommit"], ref["anchoredIndex"], ref["remote"]) != \
                (a["tag"]["tagObjectId"], a["tag"]["peeledCommit"], a["h"], a["record"]["REMOTE_URL"]) \
                or e["subjectHash"] != a["recordSha256"]:
            viol.append(("step4", "ANCHOR_REF_MISMATCH", e["index"]))
        if e["index"] <= a["h"]:
            viol.append(("step4", "ANCHOR_RECORDED_BEFORE_HEAD", e["index"]))
        if k + 1 < len(anchors) and e["index"] > anchors[k + 1]["h"]:
            viol.append(("step4", "ANCHOR_RECORDED_NOT_COVERED", e["index"]))
    last_h = anchors[-1]["h"] if anchors else -1
    pending = [e["index"] for e in entries if e["index"] > last_h]
    if not anchors:
        status.append("UNANCHORED")
    # step 5: state machines, roles, fields, sequencing, use preconditions; derived ANCHORED
    subj_last, subj_hash = {}, {}
    manifest_approved, pin_recorded, pin_closed = {}, {}, {}
    recorded_anchors = []  # (entry index, anchoredIndex) of ANCHOR_RECORDED entries matched to a tag
    for e in entries:
        ev, kind, i = e["event"], e["subjectKind"], e["index"]
        if ev not in EVENT_ROLE or kind not in EVENT_KINDS[ev]:
            viol.append(("step5", "ILLEGAL_EVENT_OR_KIND", i))
            continue
        if e["role"] != EVENT_ROLE[ev]:
            viol.append(("step5", "ROLE_MISMATCH", i))
        for f, evs in CONDITIONAL.items():
            if (e[f] is not None) != (ev in evs):
                viol.append(("step5", "FIELD_RULE", (i, f)))
        if (e["captureId"] is not None) != (kind == "MANIFEST"):
            viol.append(("step5", "FIELD_RULE", (i, "captureId")))
        if (e["subjectHash"] is None) != (ev == "AUTHORIZED"):
            viol.append(("step5", "FIELD_RULE", (i, "subjectHash")))
        key = (kind, e["captureId"] if kind == "MANIFEST" else e["subjectHash"])
        prev_ev = subj_last.get(key)
        if ev not in TRANSITIONS[kind].get(prev_ev, set()):
            viol.append(("step5", "ILLEGAL_TRANSITION", (i, prev_ev, ev)))
        if kind == "MANIFEST" and ev != "AUTHORIZED":
            if subj_hash.get(key) not in (None, e["subjectHash"]):
                viol.append(("step5", "MANIFEST_IDENTITY_CHANGED", i))
            subj_hash[key] = e["subjectHash"]
        subj_last[key] = ev
        if ev == "ANCHOR_RECORDED" and e["anchorRef"]["tag"] in names:
            recorded_anchors.append((i, e["anchorRef"]["anchoredIndex"]))
        if ev == "APPROVED":
            manifest_approved[key] = i
        if ev == "PIN_RECORDED":
            pin_recorded[e["subjectHash"]] = i
            cands = [p for p in store if p.startswith(PINS_DIR) and p.endswith("-%s.json" % e["subjectHash"])]
            path = cands[0] if len(cands) == 1 else None
            pb = store.get(path) if path else None
            ok = pb is not None and sha256_hex(pb) == e["subjectHash"] and \
                json.loads(pb.decode())["pinSlot"] == e["pinSlot"] and \
                path == PINS_DIR + "%s-%s.json" % (json.loads(pb.decode())["pinKind"], e["subjectHash"])
            ratifiers = set()
            for r in e["ratificationRecords"]:
                rb = store.get(r["path"])
                if rb is None or sha256_hex(rb) != r["sha256"]:
                    ok = False
                    continue
                ro = json.loads(rb.decode())
                if ro["subjectHash"] == e["subjectHash"] and ro["decision"] == "RATIFIED":
                    ratifiers.add(ro["ratifier"])
            if not ok or ratifiers != {"COORDINATOR", "ARCHITECT"} or len(e["ratificationRecords"]) != 2:
                viol.append(("step5", "PIN_SEAL_INVALID", i))
        if kind == "PIN" and ev in ("SUPERSEDED", "REVOKED"):
            pin_closed[e["subjectHash"]] = i
        if ev == "IN_USE":
            a_idx = manifest_approved.get(key)
            if a_idx is None or not any(ri < i and ah >= a_idx for ri, ah in recorded_anchors):
                viol.append(("step5", "USE_BEFORE_ANCHOR", i))
            for p in e["pinRecordHashes"]:
                q = pin_recorded.get(p)
                if q is None or pin_closed.get(p, 10 ** 9) < i or \
                        not any(ri < i and ah >= q for ri, ah in recorded_anchors):
                    viol.append(("step5", "PIN_USE_BEFORE_ANCHOR", (i, p)))
    # baseline sequencing and anchor-record consistency
    versions = {}
    for e in entries:
        if e["event"] == "BASELINE_RECORDED":
            v = int(e["baselineVersion"][1:])
            i = e["index"]
            ok = entries[i + 1:i + 2] and entries[i + 1]["event"] == "BASELINE_APPROVED" and \
                entries[i + 1]["subjectHash"] == e["subjectHash"] and entries[i + 1]["baselineVersion"] == e["baselineVersion"]
            last = i + 1
            if v == 1:
                ok = ok and i == 0
            else:
                prevb = versions.get(v - 1)
                ok = ok and prevb is not None and entries[i + 2:i + 3] and entries[i + 2]["event"] == "SUPERSEDED" \
                    and entries[i + 2]["subjectHash"] == prevb and entries[i + 2]["supersededBy"] == e["subjectHash"]
                last = i + 2
            if not ok:
                viol.append(("step5", "BASELINE_SEQUENCE", i))
            first = [a for a in anchors if a["version"] == v and a["seq"] == 0]
            if first and first[0]["h"] != last:
                viol.append(("step5", "BASELINE_SEQUENCE", ("first anchor h", first[0]["tag"]["name"])))
            versions[v] = e["subjectHash"]
    for a in anchors:
        rec_e = app_e = None
        for e in entries[:min(a["h"], len(entries) - 1) + 1]:
            if e["event"] == "BASELINE_RECORDED":
                rec_e = e
            if e["event"] == "BASELINE_APPROVED":
                app_e = e
        if not rec_e or not app_e or a["record"]["BA11_SEAL_HASH"] != app_e["subjectHash"] or \
                a["record"]["BA07_SEAL_HASH"] != rec_e["baselineCustodyHash"] or \
                a["record"]["BASELINE_VERSION"] != app_e["baselineVersion"]:
            viol.append(("step5", "ANCHOR_RECORD_BASELINE_MISMATCH", a["tag"]["name"]))
    derived = {}
    for key, ev in subj_last.items():
        last_idx = max(e["index"] for e in entries
                       if (e["subjectKind"], e["captureId"] if e["subjectKind"] == "MANIFEST" else e["subjectHash"]) == key)
        derived["%s:%s" % (key[0], key[1] if key[0] == "MANIFEST" else key[1][:12])] = {
            "lastEvent": ev, "lastIndex": last_idx,
            "custody": "ANCHORED" if last_idx <= last_h else "PENDING_REANCHOR"}
    # step 6: independent copy (designation read from INDEPENDENT_COPY of the anchor records)
    designations = {json.dumps(a["record"]["INDEPENDENT_COPY"], sort_keys=True) for a in anchors}
    undesignated = any(a["record"]["INDEPENDENT_COPY"] == "UNDESIGNATED" for a in anchors)
    if independent_copy is None or not anchors or undesignated:
        step6 = "NOT_VERIFIABLE"
    else:
        step6 = "MATCH"
        for d in designations:
            d = json.loads(d)
            rb = store.get(d["designationRecord"]["path"])
            if rb is None or sha256_hex(rb) != d["designationRecord"]["sha256"] or \
                    json.loads(rb.decode())["independentCopy"] != {"holder": d["holder"], "location": d["location"]}:
                viol.append(("step6", "DESIGNATION_RECORD_MISMATCH", d["designationRecord"]["path"]))
        seen = sorted((a["tag"]["name"], a["tag"]["tagObjectId"], a["tag"]["peeledCommit"], a["h"], a["recordSha256"])
                      for a in anchors)
        kept = sorted((c["tag"], c["tagObjectId"], c["peeledCommit"], c["anchoredIndex"], c["anchorRecordSha256"])
                      for c in independent_copy)
        if seen != kept:
            step6 = "DIFFERENCE"
            viol.append(("step6", "REMOTE_REWRITE_DETECTED", sorted(set(kept) ^ set(seen))[0][0]))
    return {
        "violations": [list(v[:2]) + [str(v[2])] for v in viol],
        "overall": "VIOLATION" if viol else "PASS_WITH_DECLARED_LIMITS",
        "anchors": [{"tag": a["tag"]["name"], "h": a["h"]} for a in anchors],
        "lastAnchoredIndex": last_h,
        "pendingReanchor": pending,
        "status": status,
        "step6": step6,
        "derivedStates": derived,
    }


# ---------------------------------------------------------------------------------------------
# 5. Main
# ---------------------------------------------------------------------------------------------

def main():
    if len(sys.argv) != 2:
        sys.stderr.write("usage: python I-52-ct21d-ba07-v4-jcs-vector.py <output.json>\n")
        return 2
    out_path = sys.argv[1]
    with open(os.path.abspath(__file__), 'rb') as f:
        gen_bytes = f.read().replace(b"\r\n", b"\n")

    # RFC 8785 section 3.2.2.2 string example (its "numbers" member is outside the BA-07 profile)
    rfc_obj = {"string": chr(0x20ac) + "$\u000f\nA'B\"\\\\\"/", "literals": [None, True, False]}
    rfc_expected = '{"literals":[null,true,false],"string":"' + chr(0x20ac) + '$\\u000f\\nA\'B\\"\\\\\\\\\\"/"}'
    rfc_produced = jcs(rfc_obj)

    doubles = []
    for label, x in (("0.1", 0.1), ("1.0", 1.0), ("-0.0", -0.0), ("+0.0", 0.0), ("2.5e-9", 2.5e-9)):
        doubles.append({"value": label, "hex": double_hex(x), "jcs": jcs(double_hex(x))})
    refused = []
    for label, x in (("NaN", float('nan')), ("+Infinity", float('inf')), ("-Infinity", float('-inf'))):
        try:
            double_hex(x)
            refused.append({"value": label, "result": "ACCEPTED (defect)"})
        except ProfileError:
            refused.append({"value": label, "result": "REFUSED"})
    number_refused = []
    for label, x in (("JSON number 0.1", 0.1), ("integer 2**53", 2 ** 53)):
        try:
            jcs({"x": x})
            number_refused.append({"value": label, "result": "ACCEPTED (defect)"})
        except ProfileError:
            number_refused.append({"value": label, "result": "REFUSED"})

    ex, mandatory_len = build_example(designated=True)
    example_entry = ex.entries[2]
    body = {k: v for k, v in example_entry.items() if k != "entryHash"}
    body_jcs = jcs(body)
    line = jcs(example_entry)
    full_log = ex.log_bytes()

    honest = verify(full_log, ex.remote_tags, ex.commits, ex.store, ex.independent_copy)
    honest_mandatory = verify(ex.log_bytes(mandatory_len - 1), ex.remote_tags[:4], ex.commits, ex.store,
                              ex.independent_copy[:4])

    # T1: truncate after entry 2 (ANCHOR_RECORDED of A0), append recomputed entries, revert the anchor file at HEAD
    t1_entries = [dict(e) for e in ex.entries[:3]]
    prev = t1_entries[-1]["entryHash"]
    for j, (ev, role, actor, sh) in enumerate([("AUTHORIZED", "APPROVAL_ROLE", COORD, None),
                                               ("CAPTURED", "CAPTURE_ROLE", CP, ill("forged manifest")),
                                               ("REVIEWED", "REVIEW_ROLE", OWNER, ill("forged manifest")),
                                               ("APPROVED", "APPROVAL_ROLE", COORD, ill("forged manifest"))]):
        e = make_entry(3 + j, utc_of(100 + j), role, actor, ev, "MANIFEST", sh, prev, captureId="CAP-1")
        t1_entries.append(e)
        prev = e["entryHash"]
    t1_log = b"".join(jcs_bytes(e) + b"\n" for e in t1_entries)
    t1_store = dict(ex.store)
    t1_store[ANCHOR_PATH] = ex.commits[ex.remote_tags[0]["peeledCommit"]][ANCHOR_PATH]  # HEAD copy reverted to A0
    t1 = verify(t1_log, ex.remote_tags, ex.commits, t1_store, ex.independent_copy)

    # T2: in the state after entry 14 (tags A0..A3 only), edit the pending IN_USE entry 14 and recompute its hash
    t2_entries = [dict(e) for e in ex.entries[:15]]
    t2_entries[14]["runId"] = "EXAMPLE-RUN-3-EDITED"
    t2_entries[14]["entryHash"] = entry_hash(t2_entries[14])
    t2_log = b"".join(jcs_bytes(e) + b"\n" for e in t2_entries)
    t2 = verify(t2_log, ex.remote_tags[:4], ex.commits, ex.store, ex.independent_copy[:4])

    # T3: tag ct21d-baseline/v1-a1 deleted at the remote; its ANCHOR_RECORDED entry (7) is covered by A2
    t3_tags = [t for t in ex.remote_tags if t["name"] != "ct21d-baseline/v1-a1"]
    t3 = verify(full_log, t3_tags, ex.commits, ex.store, ex.independent_copy)

    # T4: the last tag (v2) deleted at the remote and its pending ANCHOR_RECORDED entry (18) removed
    t4_log = ex.log_bytes(17)
    t4_tags = ex.remote_tags[:4]
    t4_without_copy = verify(t4_log, t4_tags, ex.commits, ex.store, None)  # copy undesignated
    t4 = verify(t4_log, t4_tags, ex.commits, ex.store, ex.independent_copy)

    anchors_out = []
    for t in ex.remote_tags:
        rec = json.loads(ex.commits[t["peeledCommit"]][ANCHOR_PATH].decode())
        anchors_out.append({"tag": t["name"], "CUSTODY_LOG_HEAD_INDEX": rec["CUSTODY_LOG_HEAD_INDEX"],
                            "MANIFEST_CUSTODY_HEADER_HASH": rec["MANIFEST_CUSTODY_HEADER_HASH"],
                            "anchorRecordSha256": sha256_hex(ex.commits[t["peeledCommit"]][ANCHOR_PATH]),
                            "tagObjectId (illustrative)": t["tagObjectId"],
                            "peeledCommit (illustrative)": t["peeledCommit"]})
    first_anchor_record = ex.commits[ex.remote_tags[0]["peeledCommit"]][ANCHOR_PATH].decode('utf-8')

    out = {
        "artifact": "BA-07 V4 (CT21D-BASE-MANIFEST-CUSTODY), section 4.1 and section 12",
        "status": "the serialization profile and the bytes and hashes below are the NORMATIVE vector of BA-07 V4 "
                  "(as part of BA-07 when sealed); every value of the worked example is ILLUSTRATIVE "
                  "(derived from EXAMPLE labels) and is not a hash of any real artifact, run or tag",
        "generator": {"path": GENERATOR_PATH, "sha256OfLfNormalizedBytes": sha256_hex(gen_bytes)},
        "profile": {
            "standard": "RFC 8785 (JCS)",
            "memberNames": "printable ASCII only (so the RFC 8785 UTF-16 code-unit order equals byte order)",
            "numbers": "integers in [-(2**53-1), 2**53-1] only, written in decimal",
            "doubles": "JSON string of 16 lowercase hexadecimal digits of the IEEE-754 binary64 bit pattern, "
                       "big-endian; -0.0 and +0.0 differ; NaN and infinities are refused",
            "nullVersusAbsent": "every schema field is present; a field that does not apply is null",
            "recordFile": "JCS bytes followed by exactly one LF; record hash = SHA-256 of the file bytes",
            "entryHash": "SHA-256 of the JCS bytes of the entry without its entryHash member (no LF)",
            "logLine": "JCS bytes of the complete entry (entryHash included) followed by one LF",
        },
        "checks": {
            "rfc8785StringExample": {"expected": rfc_expected, "produced": rfc_produced,
                                     "equal": rfc_expected == rfc_produced},
            "doubles": doubles,
            "doublesRefused": refused,
            "numbersRefused": number_refused,
        },
        "exampleEntry": {
            "description": "worked-example entry at index 2: the ANCHOR_RECORDED entry of the first anchor "
                           "(nested anchorRef, null fields, a non-ASCII note, an escaped quote)",
            "jcsWithoutEntryHash": body_jcs,
            "jcsWithoutEntryHashUtf8Length": len(body_jcs.encode('utf-8')),
            "entryHash": example_entry["entryHash"],
            "logLine": line,
        },
        "firstAnchorRecordFile": first_anchor_record,
        "workedExample": {
            "mandatorySequenceEntries": mandatory_len,
            "entries": [{"index": e["index"], "event": e["event"], "role": e["role"],
                         "subjectKind": e["subjectKind"], "entryHash": e["entryHash"]} for e in ex.entries],
            "logLines": [jcs(e) for e in ex.entries],
            "anchors": anchors_out,
            "mandatorySequence": {"headIndex": mandatory_len - 1,
                                  "headEntryHash": ex.entries[mandatory_len - 1]["entryHash"],
                                  "logFileSha256": sha256_hex(ex.log_bytes(mandatory_len - 1))},
            "fullExample": {"headIndex": len(ex.entries) - 1, "headEntryHash": ex.entries[-1]["entryHash"],
                            "logFileSha256": sha256_hex(full_log)},
        },
        "verificationModel": {
            "honestMandatorySequence": honest_mandatory,
            "honestFullExample": honest,
            "T1_truncateAndRewriteAfterFirstAnchor": t1,
            "T2_editPendingEntryAfterLastAnchor": t2,
            "T3_tagDeletedWhileItsEntryIsCovered": t3,
            "T4_lastTagDeletedAndPendingEntryRemoved_copyUndesignated": t4_without_copy,
            "T4_lastTagDeletedAndPendingEntryRemoved_copyDesignated": t4,
        },
        "crossCheckAgainstPythonJsonDumps": {"objectsSerialized": CROSS_CHECKS["objects"],
                                             "equal": CROSS_CHECKS["equal"]},
    }
    data = (json.dumps(out, indent=2, ensure_ascii=False) + "\n").encode('utf-8')
    with open(out_path, 'wb') as f:
        f.write(data)
    print("output sha256 (LF bytes):", sha256_hex(data))
    print("example entry hash:", example_entry["entryHash"])
    print("mandatory head:", ex.entries[mandatory_len - 1]["entryHash"])
    print("full head:", ex.entries[-1]["entryHash"])
    print("generator sha256 (LF):", sha256_hex(gen_bytes))
    return 0


if __name__ == "__main__":
    sys.exit(main())
