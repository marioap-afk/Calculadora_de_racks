#!/usr/bin/env python3
"""I-52 CT-21D baseline artifact BA-07 V5: canonical serialization vector and worked custody example.

Usage:
    python I-52-ct21d-ba07-v5-jcs-vector.py <output.json>

What it does (deterministic; no network, no git, no host, no AutoCAD):
  1. Implements the BA-07 V5 section 4 canonical serialization: JCS (RFC 8785) restricted to the
     BA-07 profile (ASCII member names, integers only in [-(2**53-1), 2**53-1], doubles carried as a
     JSON string of 16 lowercase hexadecimal digits of the IEEE-754 binary64 bit pattern, big-endian).
  2. Checks the string escaping against the string example of RFC 8785 section 3.2.2.2, and every
     serialized object against Python json.dumps (equal for this profile, because member names are
     ASCII and no JSON number is a non-integer).
  3. Builds the mandatory worked example of BA-07 V5 section 12 (genesis -> approval -> anchor ->
     manifest -> anchor -> IN_USE x2 -> re-anchor -> pins -> re-anchor), plus a continuation (a use
     with pins, a new baseline version with its carry-over block, a use under the new version and a
     re-anchor), and prints the JCS bytes and SHA-256 of the example entry and the entry hashes of the
     whole chain.
  4. Runs a model of the verification algorithm of BA-07 V5 section 6.2 on the honest example, on
     tampered variants and on the honest and illegal variants listed in BA-07 V5 section 12.

Every hash, object id, time, actor, run id and value of the example is ILLUSTRATIVE: it is derived
from a label such as "EXAMPLE:BA-11 seal v1" and is not a hash of any real artifact. The normative
content of this file is the serialization profile and its vectors (the bytes and hashes it prints).
The V4 generator and its output stay unchanged as history.
"""

import copy
import hashlib
import json
import os
import re
import struct
import sys

GENERATOR_PATH = "docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.py"
REMOTE_URL = "https://github.com/marioap-afk/Calculadora_de_racks.git"
LOG_PATH = "docs/automation/evidence/ct21d/custody-log.jsonl"
ANCHOR_PATH = "docs/automation/evidence/ct21d/baseline-anchor.json"
PINS_DIR = "docs/automation/evidence/ct21d/pins/"
RATIFICATIONS_DIR = "docs/automation/evidence/ct21d/ratifications/"
ZERO64 = "0" * 64
MAX_INT = 2 ** 53 - 1
BA07_ID = "CT21D-BASE-MANIFEST-CUSTODY"
BA11_ID = "CT21D-BASE-HASH-REGISTRY"
PIN_KINDS = {"E04_ADMITTED_SET", "LOCK_MODE", "EVM_FROZEN", "HOST_DEFAULT_MAP", "FIXTURE_INSTANCE"}


class ProfileError(Exception):
    pass


# ---------------------------------------------------------------------------------------------
# 1. Canonical serialization (BA-07 V5 section 4): JCS, RFC 8785, BA-07 profile
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
# 2. Custody-log entries (BA-07 V5 section 6.1)
# ---------------------------------------------------------------------------------------------

ENTRY_FIELDS = [
    "actor", "anchorRef", "baselineCustodyHash", "baselineVersion", "captureId", "cause", "entryHash",
    "event", "index", "note", "pinRecordHashes", "pinSlot", "prevEntryHash", "ratificationRecords",
    "role", "runId", "runStartAnchor", "subjectHash", "subjectKind", "supersededBy", "tupleRecordHash", "utc",
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
    "runStartAnchor": {"IN_USE"},
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
# V5: SUPERSEDED -> REVOKED is legal for a manifest (V2.1 29.3 "any state -> REVOKED") and for a pin
TRANSITIONS = {
    "BASELINE": {None: {"BASELINE_RECORDED"}, "BASELINE_RECORDED": {"BASELINE_APPROVED"},
                 "BASELINE_APPROVED": {"SUPERSEDED"}},
    "ANCHOR": {None: {"ANCHOR_RECORDED"}},
    "PIN": {None: {"PIN_RECORDED"}, "PIN_RECORDED": {"SUPERSEDED", "REVOKED"}, "SUPERSEDED": {"REVOKED"}},
    "REVIEW_RECORD": {None: {"REVIEW_RECORD_ADDED"}},
    "MANIFEST": {None: {"AUTHORIZED"}, "AUTHORIZED": {"CAPTURED", "REVOKED"},
                 "CAPTURED": {"REVIEWED", "REVOKED"}, "REVIEWED": {"APPROVED", "REVOKED"},
                 "APPROVED": {"IN_USE", "REVOKED"}, "IN_USE": {"IN_USE", "SUPERSEDED", "REVOKED"},
                 "SUPERSEDED": {"REVOKED"}},
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
# 3. Illustrative values and records (BA-07 V5 sections 5.1 and 5.2)
# ---------------------------------------------------------------------------------------------

def ill(label):
    return sha256_hex(("EXAMPLE:" + label).encode('utf-8'))


def ill_git(label):
    return hashlib.sha1(("EXAMPLE:" + label).encode('utf-8')).hexdigest()


def utc_of(n):
    return "2000-01-01T%02d:%02d:00Z" % (n // 60, n % 60)


COORD = "EXAMPLE-COORDINATOR"
ARCH = "EXAMPLE-ARCHITECT"
OWNER = "EXAMPLE-OWNER"
CP = "CONTROL_PLANE:EXAMPLE-BUILD"
HOLDER = {"holder": "EXAMPLE-HOLDER (illustrative)",
          "location": "EXAMPLE-LOCATION (illustrative, outside the repository)"}
SLOT_TABLE = ["E04_ADMITTED_SET@R-1", "EVM_FROZEN", "FIXTURE_INSTANCE@EXAMPLE-FX", "HOST_DEFAULT_MAP", "LOCK_MODE"]


def ratification(ratifier, actor, subject_kind, subject, subject_hash, decision, utc, independent_copy=None,
                 evidence=None):
    """A candidate, ratification, designation or carry-over record (BA-07 V5 section 5.2)."""
    return {"recordKind": "CT21D_RATIFICATION", "ratifier": ratifier, "actor": actor,
            "subjectKind": subject_kind, "subject": subject, "subjectHash": subject_hash, "decision": decision,
            "utc": utc, "reference": "EXAMPLE decisions-log reference", "independentCopy": independent_copy,
            "evidence": sorted(evidence or [], key=lambda x: x["path"])}


class Example:
    def __init__(self, forge=None, ba07_label="BA-07 seal"):
        self.entries = []
        self.store = {}            # path -> bytes, at the verified revision
        self.commits = {}          # peeled commit -> {path: bytes}
        self.remote_tags = []      # ls-remote view: {name, tagObjectId, peeledCommit, annotated}
        self.independent_copy = [] # what the designated holder keeps
        self.clock = 0
        self.git_prefix = "" if forge is None else forge + " "
        self.forge_undesignated = forge is not None
        self.designation = None
        self.set_ba07_seal(ba07_label, utc_of(0))

    def set_ba07_seal(self, label, utc):
        """The primary designation record: the Coordinator's ratification record of the sealed BA-07
        (subjectHash = the BA-07 seal hash), carrying independentCopy (BA-07 V5 sections 2 and 5.2)."""
        self.ba07_seal = ill(label)
        rec = ratification("COORDINATOR", COORD, "ARTIFACT", BA07_ID, self.ba07_seal, "RATIFIED", utc,
                           independent_copy=dict(HOLDER))
        path = RATIFICATIONS_DIR + "%s-%s-coordinator.json" % (BA07_ID, self.ba07_seal)
        self.store[path] = record_file_bytes(rec)
        self.designation = {"designationRecord": {"path": path, "sha256": record_hash(rec)},
                            "holder": HOLDER["holder"], "location": HOLDER["location"]}

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
            "INDEPENDENT_COPY": "UNDESIGNATED" if self.forge_undesignated else dict(self.designation),
        }
        rbytes = record_file_bytes(record)
        peeled = ill_git(self.git_prefix + "anchor commit " + name)
        tag_obj = ill_git(self.git_prefix + "tag object " + name)
        self.commits[peeled] = {ANCHOR_PATH: rbytes, LOG_PATH: self.log_bytes(h)}
        self.store[ANCHOR_PATH] = rbytes
        self.remote_tags.append({"name": name, "tagObjectId": tag_obj, "peeledCommit": peeled, "annotated": True})
        ref = {"anchoredIndex": h, "peeledCommit": peeled, "remote": REMOTE_URL, "tag": name, "tagObjectId": tag_obj}
        self.append("CUSTODY_ROLE", COORD, "ANCHOR_RECORDED", "ANCHOR", sha256_hex(rbytes), note, anchorRef=ref)
        self.independent_copy.append({"tag": name, "tagObjectId": tag_obj, "peeledCommit": peeled,
                                      "anchoredIndex": h, "anchorRecordSha256": sha256_hex(rbytes)})
        return {"anchorRecordHash": sha256_hex(rbytes), "tag": name}

    def pin(self, pin_kind, slot, value, version, sources, supersedes=None, record_slot=None):
        rec = {"recordKind": "CT21D_PIN_RECORD", "pinKind": pin_kind, "pinSlot": record_slot or slot,
               "baselineVersion": version, "value": value, "sourceEvidence": sorted(sources, key=lambda s: s["path"]),
               "inventoryReviewRecords": None, "supersedes": supersedes, "frozenUtc": self.tick(),
               "recordedBy": CP}
        ph = record_hash(rec)
        self.store[PINS_DIR + "%s-%s.json" % (pin_kind, ph)] = record_file_bytes(rec)
        rats = []
        for who, actor in (("COORDINATOR", COORD), ("ARCHITECT", ARCH)):
            r = ratification(who, actor, "PIN", slot, ph, "RATIFIED", self.tick())
            path = RATIFICATIONS_DIR + "%s-%s-%s.json" % (slot, ph, who.lower())
            self.store[path] = record_file_bytes(r)
            rats.append({"path": path, "sha256": record_hash(r)})
        rats.sort(key=lambda x: x["path"])
        self.append("APPROVAL_ROLE", COORD, "PIN_RECORDED", "PIN", ph, pinSlot=slot, ratificationRecords=rats)
        return ph

    def baseline_records(self, version, seal, carry=()):
        """BA-11's candidate record and its two ratification records (the records that establish its
        SEAL_HASH; there is no separate seal record), plus one MANIFEST_CARRY_OVER record per manifest
        carried over to this version (BA-07 V5 sections 5.2 and 8)."""
        out = []
        for kind, ratifier, actor, decision in (("candidate", "ARCHITECT", ARCH, "CANDIDATE_FOR_SEALING"),
                                                ("coordinator", "COORDINATOR", COORD, "RATIFIED"),
                                                ("architect", "ARCHITECT", ARCH, "RATIFIED")):
            r = ratification(ratifier, actor, "ARTIFACT", BA11_ID, seal, decision, self.tick())
            path = RATIFICATIONS_DIR + "%s-%s-%s.json" % (BA11_ID, seal, kind)
            self.store[path] = record_file_bytes(r)
            out.append({"path": path, "sha256": record_hash(r)})
        for capture_id, identity in carry:
            r = ratification("COORDINATOR", COORD, "MANIFEST", capture_id, identity, "MANIFEST_CARRY_OVER",
                             self.tick())
            path = RATIFICATIONS_DIR + "%s-%s-carryover-%s.json" % (capture_id, identity, version)
            self.store[path] = record_file_bytes(r)
            out.append({"path": path, "sha256": record_hash(r)})
        out.sort(key=lambda x: x["path"])
        return out

    def new_baseline(self, version, prev_seal, carry=()):
        seal = ill("BA-11 seal " + version)
        self.append("CUSTODY_ROLE", COORD, "BASELINE_RECORDED", "BASELINE", seal,
                    baselineCustodyHash=self.ba07_seal, baselineVersion=version)
        self.append("APPROVAL_ROLE", COORD, "BASELINE_APPROVED", "BASELINE", seal, baselineVersion=version,
                    ratificationRecords=self.baseline_records(version, seal, carry))
        if prev_seal is not None:
            self.append("APPROVAL_ROLE", COORD, "SUPERSEDED", "BASELINE", prev_seal, supersededBy=seal)
        return seal

    def manifest(self, capture_id, identity):
        self.append("APPROVAL_ROLE", COORD, "AUTHORIZED", "MANIFEST", None, captureId=capture_id)
        self.append("CAPTURE_ROLE", CP, "CAPTURED", "MANIFEST", identity, captureId=capture_id)
        self.append("REVIEW_ROLE", OWNER, "REVIEWED", "MANIFEST", identity, captureId=capture_id)
        self.append("APPROVAL_ROLE", COORD, "APPROVED", "MANIFEST", identity, captureId=capture_id)

    def use(self, run_id, pins, rsa, capture_id="CAP-1", identity=None):
        return self.append("CUSTODY_ROLE", CP, "IN_USE", "MANIFEST", identity, captureId=capture_id, runId=run_id,
                           tupleRecordHash=ill("tuple record " + run_id), pinRecordHashes=sorted(pins),
                           runStartAnchor=rsa)

    def close(self, event, kind, subject_hash, capture_id=None, cause=None, superseded_by=None, note=""):
        cond = {}
        if capture_id is not None:
            cond["captureId"] = capture_id
        if cause is not None:
            cond["cause"] = cause
        if superseded_by is not None:
            cond["supersededBy"] = superseded_by
        return self.append("APPROVAL_ROLE", COORD, event, kind, subject_hash, note, **cond)


LOCK_SRC = [{"path": "EXAMPLE/evidence/lock-mode-selection.json", "sha256": ill("lock mode evidence")}]
E04_SRC = [{"path": "EXAMPLE/evidence/e4-learn-r1.json", "sha256": ill("e4 learn evidence")}]
E04_VALUE = ["EXAMPLE-MEMBER-1", "EXAMPLE-MEMBER-2"]


def build_mandatory(ex, manifest_label="manifest identity 1"):
    """BA-07 V5 section 12, mandatory sequence (indexes 0-13)."""
    ex.new_baseline("v1", None)                                                                      # 0, 1
    a0 = ex.anchor("v1", 0, note="first anchor of baseline \"v1\" (illustrative) " + chr(0x2014) + " caf" + chr(0xe9))  # 2
    man = ill(manifest_label)
    ex.manifest("CAP-1", man)                                                                        # 3-6
    a1 = ex.anchor("v1", 1)                                                                          # 7
    ex.use("EXAMPLE-RUN-1", [], a1, identity=man)                                                    # 8
    ex.use("EXAMPLE-RUN-2", [], a1, identity=man)                                                    # 9
    a2 = ex.anchor("v1", 2)                                                                          # 10
    p1 = ex.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE", "v1", LOCK_SRC)                       # 11
    p2 = ex.pin("E04_ADMITTED_SET", "E04_ADMITTED_SET@R-1", E04_VALUE, "v1", E04_SRC)                # 12
    a3 = ex.anchor("v1", 3)                                                                          # 13
    return {"man": man, "p1": p1, "p2": p2, "a0": a0, "a1": a1, "a2": a2, "a3": a3}


def build_example():
    ex = Example()
    ids = build_mandatory(ex)
    mandatory_len = len(ex.entries)
    man, p1, p2 = ids["man"], ids["p1"], ids["p2"]
    # -- continuation: a use with pins, a new baseline version with its carry-over block, a use under it --
    ex.use("EXAMPLE-RUN-3", [p1, p2], ids["a3"], identity=man)                                       # 14
    after14 = copy.deepcopy(ex)
    ex.new_baseline("v2", ill("BA-11 seal v1"), carry=[("CAP-1", man)])                              # 15, 16, 17
    after17 = copy.deepcopy(ex)
    p1b = ex.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE", "v2", LOCK_SRC, supersedes=p1)       # 18 carry-over
    ex.close("SUPERSEDED", "PIN", p1, superseded_by=p1b)                                             # 19
    ex.close("REVOKED", "PIN", p2, cause="SUPERSEDED_BY_RULING", note="not carried over to v2")      # 20
    b0 = ex.anchor("v2", 0)                                                                          # 21 (h = 20)
    ex.use("EXAMPLE-RUN-4", [p1b], b0, identity=man)                                                 # 22
    ex.anchor("v2", 1)                                                                               # 23 (h = 22)
    ids.update(p1b=p1b, b0=b0)
    return ex, mandatory_len, ids, after14, after17


# ---------------------------------------------------------------------------------------------
# 4. Model of the verification algorithm (BA-07 V5 section 6.2)
# ---------------------------------------------------------------------------------------------

TAG_RE = re.compile(r"^ct21d-baseline/v([1-9][0-9]*)(?:-a([1-9][0-9]*))?$")


def load_record(store, ref):
    b = store.get(ref["path"])
    if b is None or sha256_hex(b) != ref["sha256"]:
        return None
    return json.loads(b.decode('utf-8'))


def verify(log_bytes, remote_tags, commits, store, holder_list, slot_tables, historical=False):
    """holder_list = the anchor identities the designated holder keeps, or None when the holder cannot be
    consulted. slot_tables = {BA-11 seal hash: pin-slot table of that sealed BA-11 (section 3.1)}, an input
    of the model (reading BA-11 bytes is not modelled). historical = the explicit historical mode."""
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
    n = len(entries) - 1
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
        snap_lines = snap.get(LOG_PATH, b"").split(b"\n")[:-1]
        if len(snap_lines) != h + 1 or json.loads(snap_lines[-1].decode('utf-8'))["entryHash"] != H:
            viol.append(("step3", "ANCHOR_COMMIT_LOG_MISMATCH", t["name"]))
        anchors.append({"tag": t, "record": rec, "recordSha256": sha256_hex(rb), "h": h, "H": H,
                        "version": int(m.group(1)), "seq": seq})
    anchors.sort(key=lambda a: a["h"])
    for a, b in zip(anchors, anchors[1:]):
        if b["h"] <= a["h"]:
            viol.append(("step3", "ANCHOR_ORDER", b["tag"]["name"]))
    expect_v, expect_s = 1, 0
    for a in anchors:
        if a["version"] == expect_v and a["seq"] == expect_s:
            expect_s += 1
        elif a["version"] == expect_v + 1 and a["seq"] == 0 and expect_s > 0:
            expect_v, expect_s = expect_v + 1, 1
        else:
            viol.append(("step3", "TAG_SEQUENCE", a["tag"]["name"]))
    # historical mode: anchors whose h is beyond the verified revision are LATER_ANCHORS, not codes
    in_scope = [a for a in anchors if not historical or a["h"] <= n]
    later = [a["tag"]["name"] for a in anchors if historical and a["h"] > n]
    for a in in_scope:
        if a["h"] > n:
            viol.append(("step3", "TRUNCATED_BELOW_ANCHOR", a["tag"]["name"]))
        elif entries[a["h"]]["entryHash"] != a["H"]:
            viol.append(("step3", "REWRITTEN_AT_OR_BEFORE_ANCHOR", a["tag"]["name"]))
    # step 4: one-to-one tags <-> ANCHOR_RECORDED; pending window
    status = []
    by_tag = {}
    for e in entries:
        if e["event"] == "ANCHOR_RECORDED":
            by_tag.setdefault(e["anchorRef"]["tag"], []).append(e)
    all_names = [a["tag"]["name"] for a in anchors]
    for tag_name, es in by_tag.items():
        if tag_name not in all_names:
            viol.append(("step4", "ANCHOR_WITHOUT_TAG", es[0]["index"]))
        if len(es) > 1:
            viol.append(("step4", "DUPLICATE_ANCHOR_RECORDED", tag_name))
    anchor_entry = {}  # tag name -> index of its ANCHOR_RECORDED entry (in-scope anchors matched)
    for k, a in enumerate(in_scope):
        es = by_tag.get(a["tag"]["name"], [])
        if not es:
            if k == len(in_scope) - 1:
                status.append("LAST_ANCHOR_NOT_RECORDED")
            else:
                viol.append(("step4", "UNRECORDED_ANCHOR", a["tag"]["name"]))
            continue
        e = es[0]
        anchor_entry[a["tag"]["name"]] = e["index"]
        ref = e["anchorRef"]
        if (ref["tagObjectId"], ref["peeledCommit"], ref["anchoredIndex"], ref["remote"]) != \
                (a["tag"]["tagObjectId"], a["tag"]["peeledCommit"], a["h"], a["record"]["REMOTE_URL"]) \
                or e["subjectHash"] != a["recordSha256"]:
            viol.append(("step4", "ANCHOR_REF_MISMATCH", e["index"]))
        if e["index"] <= a["h"]:
            viol.append(("step4", "ANCHOR_RECORDED_BEFORE_HEAD", e["index"]))
        if k + 1 < len(in_scope) and e["index"] > in_scope[k + 1]["h"]:
            viol.append(("step4", "ANCHOR_RECORDED_NOT_COVERED", e["index"]))
    last_h = in_scope[-1]["h"] if in_scope else -1
    pending = [e["index"] for e in entries if e["index"] > last_h]
    if not in_scope:
        status.append("UNANCHORED")
    recorded_anchors = [(anchor_entry[a["tag"]["name"]], a["h"]) for a in in_scope if a["tag"]["name"] in anchor_entry]
    by_name = {a["tag"]["name"]: a for a in in_scope}
    first_anchor_h = {a["version"]: a["h"] for a in in_scope if a["seq"] == 0}

    # step 5: state machines, roles, fields, baseline sequence and carry-over block, pins, use preconditions
    subj_last, subj_hash = {}, {}
    version_in_force = None
    seal_of = {}          # version -> BA-11 seal hash
    ba07_of = {}          # version -> BA-07 seal hash (baselineCustodyHash)
    recorded_at = {}      # version -> index of its BASELINE_RECORDED entry
    manifests = {}        # captureId -> {identity, approved, version, carried}
    pins = {}             # pin hash -> {slot, kind, version, recorded, closed, consumed, record}
    live = {}             # slot -> set of live pin hashes
    closures = []         # indexes of SUPERSEDED / REVOKED entries
    open_at_change = {}   # version -> ({captureId open at BASELINE_APPROVED}, {live pins of earlier versions})
    carried = {}          # version -> {captureId: identity} of valid MANIFEST_CARRY_OVER records
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
        key = (kind, e["captureId"] if kind == "MANIFEST" else e["subjectHash"])
        prev_ev = subj_last.get(key)
        null_ok = ev == "AUTHORIZED" or (ev == "REVOKED" and kind == "MANIFEST" and prev_ev == "AUTHORIZED")
        if (e["subjectHash"] is None) != null_ok:
            viol.append(("step5", "FIELD_RULE", (i, "subjectHash")))
        if ev == "IN_USE" and (not isinstance(e["runStartAnchor"], dict) or
                               sorted(e["runStartAnchor"].keys()) != ["anchorRecordHash", "tag"]):
            viol.append(("step5", "FIELD_RULE", (i, "runStartAnchor")))
        if ev not in TRANSITIONS[kind].get(prev_ev, set()):
            viol.append(("step5", "ILLEGAL_TRANSITION", (i, prev_ev, ev)))
        if kind == "MANIFEST" and e["subjectHash"] is not None:
            if subj_hash.get(key) not in (None, e["subjectHash"]):
                viol.append(("step5", "MANIFEST_IDENTITY_CHANGED", i))
            subj_hash[key] = e["subjectHash"]
        subj_last[key] = ev

        # -- baseline entries --
        if ev == "BASELINE_RECORDED":
            recorded_at[e["baselineVersion"]] = i
            ba07_of[e["baselineVersion"]] = e["baselineCustodyHash"]
        if ev == "BASELINE_APPROVED":
            v = e["baselineVersion"]
            seal_of[v] = e["subjectHash"]
            want = {("ARCHITECT", "CANDIDATE_FOR_SEALING"), ("COORDINATOR", "RATIFIED"), ("ARCHITECT", "RATIFIED")}
            got, ok = set(), True
            carry_recs = []
            for r in e["ratificationRecords"]:
                ro = load_record(store, r)
                if ro is None:
                    ok = False
                    continue
                if ro["decision"] == "MANIFEST_CARRY_OVER":
                    carry_recs.append(ro)
                    continue
                if ro["subjectKind"] != "ARTIFACT" or ro["subject"] != BA11_ID or ro["subjectHash"] != e["subjectHash"]:
                    ok = False
                got.add((ro["ratifier"], ro["decision"]))
            if not ok or got != want or len(e["ratificationRecords"]) != 3 + len(carry_recs):
                viol.append(("step5", "BASELINE_RECORDS_INVALID", i))
            vnum = int(v[1:])
            open_manifests = {c: m["identity"] for c, m in manifests.items()
                              if subj_last.get(("MANIFEST", c)) not in ("SUPERSEDED", "REVOKED")}
            carried[v] = {}
            for ro in carry_recs:
                prev_v = "v%d" % (vnum - 1)
                if vnum < 2 or ro["ratifier"] != "COORDINATOR" or ro["subjectKind"] != "MANIFEST" or \
                        ro["subject"] not in open_manifests or ro["subjectHash"] is None or \
                        ro["subjectHash"] != open_manifests.get(ro["subject"]) or \
                        ba07_of.get(v) != ba07_of.get(prev_v):
                    viol.append(("step5", "CARRY_OVER_INVALID", (i, ro["subject"])))
                else:
                    carried[v][ro["subject"]] = ro["subjectHash"]
                    manifests[ro["subject"]]["carried"].add(v)
            earlier_live = {p for s in live.values() for p in s if pins[p]["version"] != v}
            open_at_change[v] = (set(open_manifests), earlier_live)
            version_in_force = v

        # -- manifest bookkeeping --
        if kind == "MANIFEST":
            m = manifests.setdefault(e["captureId"], {"identity": None, "approved": None, "version": None,
                                                     "carried": set()})
            if e["subjectHash"] is not None and ev != "SUPERSEDED":
                m["identity"] = e["subjectHash"]
            if ev == "APPROVED":
                m["approved"], m["version"] = i, version_in_force
            if ev == "SUPERSEDED":
                succ = [c for c, mm in manifests.items() if mm["identity"] == e["supersededBy"] and mm["approved"] is not None]
                if not succ:
                    viol.append(("step5", "FIELD_RULE", (i, "supersededBy")))

        # -- pins (section 5.1) --
        if ev == "PIN_RECORDED":
            ph, slot = e["subjectHash"], e["pinSlot"]
            cands = [p for p in store if p.startswith(PINS_DIR) and p.endswith("-%s.json" % ph)]
            pb = store.get(cands[0]) if len(cands) == 1 else None
            prec = json.loads(pb.decode()) if pb is not None and sha256_hex(pb) == ph else None
            seal_ok = prec is not None and prec["pinSlot"] == slot and \
                cands[0] == PINS_DIR + "%s-%s.json" % (prec["pinKind"], ph)
            ratifiers = []
            for r in e["ratificationRecords"]:
                ro = load_record(store, r)
                if ro is None:
                    seal_ok = False
                    continue
                if ro["decision"] == "RATIFIED" and ro["subjectKind"] == "PIN" and ro["subject"] == slot and \
                        ro["subjectHash"] == ph:
                    ratifiers.append(ro["ratifier"])
                else:
                    seal_ok = False
            if not seal_ok or sorted(ratifiers) != ["ARCHITECT", "COORDINATOR"] or len(e["ratificationRecords"]) != 2:
                viol.append(("step5", "PIN_SEAL_INVALID", i))
            if prec is None:
                continue
            pkind = slot.split("@")[0]
            if slot not in slot_tables.get(seal_of.get(version_in_force), []) or prec["pinKind"] != pkind or \
                    pkind not in PIN_KINDS:
                viol.append(("step5", "PIN_SLOT_UNKNOWN", i))
            if prec["baselineVersion"] != version_in_force:
                viol.append(("step5", "PIN_VERSION_MISMATCH", i))
            cur = set(live.get(slot, set()))
            nxt = entries[i + 1] if i + 1 < len(entries) else None
            if cur:
                old = sorted(cur)[0]
                if len(cur) > 1 or prec["supersedes"] != old:
                    viol.append(("step5", "PIN_SUPERSESSION", i))
                if nxt is not None and not (nxt["event"] == "SUPERSEDED" and nxt["subjectKind"] == "PIN" and
                                            nxt["subjectHash"] == old and nxt["supersededBy"] == ph):
                    viol.append(("step5", "PIN_SLOT_CONFLICT", (i, slot)))
                if pins[old]["version"] != prec["baselineVersion"]:  # a carry-over: only in the carry-over block
                    s = recorded_at.get(version_in_force, -1) + 2
                    fh = first_anchor_h.get(int(version_in_force[1:]))
                    if not (s < i and (fh is None or i <= fh)):
                        viol.append(("step5", "CARRY_OVER_INVALID", i))
            elif prec["supersedes"] is not None:
                viol.append(("step5", "PIN_SUPERSESSION", i))
            if pkind != "EVM_FROZEN" and any(q["slot"] == slot and q["version"] == prec["baselineVersion"] and
                                             q["consumed"] for q in pins.values()):
                viol.append(("step5", "CONSUMED_PIN_SUPERSEDED", i))
            pins[ph] = {"slot": slot, "kind": pkind, "version": prec["baselineVersion"], "recorded": i,
                        "closed": None, "consumed": False, "record": prec}
            live.setdefault(slot, set()).add(ph)
        if kind == "PIN" and ev == "SUPERSEDED":
            prv = entries[i - 1] if i > 0 else None
            succ = pins.get(e["supersededBy"])
            if prv is None or prv["event"] != "PIN_RECORDED" or prv["subjectHash"] != e["supersededBy"] or \
                    succ is None or succ["record"]["supersedes"] != e["subjectHash"] or \
                    e["subjectHash"] not in pins or succ["slot"] != pins[e["subjectHash"]]["slot"]:
                viol.append(("step5", "PIN_SUPERSESSION", i))
        if kind == "PIN" and ev in ("SUPERSEDED", "REVOKED") and e["subjectHash"] in pins:
            p = pins[e["subjectHash"]]
            if p["closed"] is None:
                p["closed"] = i
            live.get(p["slot"], set()).discard(e["subjectHash"])
        if ev in ("SUPERSEDED", "REVOKED"):
            closures.append(i)

        # -- use preconditions (section 7) --
        if ev == "IN_USE":
            rsa = e["runStartAnchor"] if isinstance(e["runStartAnchor"], dict) else {}
            a = by_name.get(rsa.get("tag"))
            h_rs = -1
            if a is None or a["recordSha256"] != rsa.get("anchorRecordHash") or \
                    anchor_entry.get(a["tag"]["name"], i) >= i or \
                    a["record"]["BASELINE_VERSION"] != version_in_force:
                viol.append(("step5", "USE_REFERENCE_INVALID", i))
            else:
                h_rs = a["h"]
            m = manifests.get(e["captureId"])
            if m is None or m["approved"] is None or m["approved"] > h_rs:
                viol.append(("step5", "USE_BEFORE_ANCHOR", i))
            if m is None or not (m["version"] == version_in_force or version_in_force in m["carried"]):
                viol.append(("step5", "MANIFEST_VERSION_MISMATCH", i))
            for p in e["pinRecordHashes"]:
                q = pins.get(p)
                if q is None or q["recorded"] > h_rs:
                    viol.append(("step5", "PIN_USE_BEFORE_ANCHOR", (i, p[:12])))
                    continue
                if q["closed"] is not None and q["closed"] < i:
                    viol.append(("step5", "PIN_USE_AFTER_CLOSURE", (i, p[:12])))
                if q["version"] != version_in_force:
                    viol.append(("step5", "PIN_VERSION_MISMATCH", (i, p[:12])))
                q["consumed"] = True
            if any(not any(ri < i and ah >= c for ri, ah in recorded_anchors) for c in closures if c < i):
                viol.append(("step5", "USE_WITH_PENDING_CLOSURE", i))

    # baseline sequence (section 8) and carry-over block
    for v, i in sorted(recorded_at.items(), key=lambda x: x[1]):
        vnum = int(v[1:])
        e = entries[i]
        ok = i + 1 <= n and entries[i + 1]["event"] == "BASELINE_APPROVED" and \
            entries[i + 1]["subjectHash"] == e["subjectHash"] and entries[i + 1]["baselineVersion"] == v
        fh = first_anchor_h.get(vnum)
        if vnum == 1:
            ok = ok and i == 0 and (fh is None or fh == 1)
        else:
            prev_seal = seal_of.get("v%d" % (vnum - 1))
            s = i + 2
            ok = ok and prev_seal is not None and s <= n and entries[s]["event"] == "SUPERSEDED" and \
                entries[s]["subjectHash"] == prev_seal and entries[s]["supersededBy"] == e["subjectHash"] and \
                (fh is None or fh >= s)
            end = fh if fh is not None else n
            for j in range(s + 1, end + 1):
                b = entries[j]
                legal = (b["event"] == "PIN_RECORDED" and any(
                            p["recorded"] == j and p["record"]["supersedes"] in pins and
                            pins[p["record"]["supersedes"]]["version"] != p["version"] for p in pins.values())) or \
                        (b["event"] == "SUPERSEDED" and b["subjectKind"] == "PIN") or \
                        (b["event"] == "REVOKED" and b["cause"] == "SUPERSEDED_BY_RULING")
                if not legal:
                    viol.append(("step5", "BASELINE_SEQUENCE", ("carry-over block", j)))
            if fh is not None:
                open_m, earlier = open_at_change.get(v, (set(), set()))
                for c in sorted(open_m):
                    revoked = any(entries[j]["event"] == "REVOKED" and entries[j]["captureId"] == c and
                                  entries[j]["cause"] == "SUPERSEDED_BY_RULING" for j in range(s + 1, fh + 1))
                    if c not in carried.get(v, {}) and not revoked:
                        viol.append(("step5", "CARRY_OVER_MISSING", ("manifest", c)))
                for p in sorted(earlier):
                    cl = pins[p]["closed"]
                    by_ruling = cl is not None and (entries[cl]["event"] == "SUPERSEDED" or
                                                    entries[cl]["cause"] == "SUPERSEDED_BY_RULING")
                    if cl is None or not (s < cl <= fh) or not by_ruling:
                        viol.append(("step5", "CARRY_OVER_MISSING", ("pin", p[:12])))
        if not ok:
            viol.append(("step5", "BASELINE_SEQUENCE", i))
    for a in in_scope:
        rec_e = app_e = None
        for e in entries[:min(a["h"], n) + 1]:
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

    # step 6: independent copy
    def seal07_at(h):
        s = None
        for e in entries[:min(h, n) + 1]:
            if e["event"] == "BASELINE_RECORDED":
                s = e["baselineCustodyHash"]
        return s

    def is_designation_record(ro, seal):
        return ro is not None and ro["subjectKind"] == "ARTIFACT" and ro["subject"] == BA07_ID and \
            ro["subjectHash"] == seal and ro["independentCopy"] is not None and \
            ((ro["decision"] == "CANDIDATE_FOR_SEALING" and ro["ratifier"] == "ARCHITECT") or
             (ro["decision"] == "RATIFIED" and ro["ratifier"] == "COORDINATOR") or
             (ro["decision"] == "DESIGNATION" and ro["ratifier"] == "COORDINATOR"))

    for a in anchors:
        ic = a["record"]["INDEPENDENT_COPY"]
        if not isinstance(ic, dict):
            viol.append(("step6", "DESIGNATION_MISSING", a["tag"]["name"]))
            continue
        ro = load_record(store, ic["designationRecord"])
        if not is_designation_record(ro, seal07_at(a["h"])) or \
                ro["independentCopy"] != {"holder": ic["holder"], "location": ic["location"]}:
            viol.append(("step6", "DESIGNATION_RECORD_MISMATCH", a["tag"]["name"]))
    seal_now = seal07_at(n) if entries else None
    if seal_now is not None:  # cross-check: the primary designation record of the BA-07 in force
        prim = [RATIFICATIONS_DIR + "%s-%s-%s.json" % (BA07_ID, seal_now, k) for k in ("candidate", "coordinator")]
        found = [json.loads(store[p].decode()) for p in prim if p in store]
        if not any(is_designation_record(ro, seal_now) for ro in found):
            viol.append(("step6", "DESIGNATION_MISSING", "primary designation record"))
    copy_confirmed = None
    if holder_list is None:
        step6 = "NOT_VERIFIABLE"
    else:
        step6 = "MATCH"
        seen = {a["tag"]["name"]: (a["tag"]["tagObjectId"], a["tag"]["peeledCommit"], a["h"], a["recordSha256"])
                for a in anchors}
        kept = {c["tag"]: (c["tagObjectId"], c["peeledCommit"], c["anchoredIndex"], c["anchorRecordSha256"])
                for c in holder_list}
        last_name = anchors[-1]["tag"]["name"] if anchors else None
        diff = sorted([t for t in kept if t not in seen or kept[t] != seen[t]] +
                      [t for t in seen if t not in kept and t != last_name])
        if diff:
            step6 = "DIFFERENCE"
            viol.append(("step6", "REMOTE_REWRITE_DETECTED", diff[0]))
        elif last_name is not None and last_name not in kept:
            status.append("LAST_ANCHOR_NOT_IN_COPY")
            confirmed = [a["h"] for a in anchors if a["tag"]["name"] in kept]
            copy_confirmed = max(confirmed) if confirmed else -1
        else:
            copy_confirmed = last_h
    if viol:
        overall = "VIOLATION"
    else:
        overall = "HISTORICAL_PASS_WITH_DECLARED_LIMITS" if historical else "PASS_WITH_DECLARED_LIMITS"
    return {
        "mode": "HISTORICAL" if historical else "CURRENT",
        "violations": [list(v[:2]) + [str(v[2])] for v in viol],
        "overall": overall,
        "anchors": [{"tag": a["tag"]["name"], "h": a["h"]} for a in in_scope],
        "laterAnchors": later,
        "lastAnchoredIndex": last_h,
        "copyConfirmedIndex": copy_confirmed,
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
        sys.stderr.write("usage: python I-52-ct21d-ba07-v5-jcs-vector.py <output.json>\n")
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

    ex, mandatory_len, ids, after14, after17 = build_example()
    slots = {ill("BA-11 seal v1"): SLOT_TABLE, ill("BA-11 seal v2"): SLOT_TABLE}
    example_entry = ex.entries[2]
    body = {k: v for k, v in example_entry.items() if k != "entryHash"}
    body_jcs = jcs(body)
    line = jcs(example_entry)
    full_log = ex.log_bytes()
    man, p1, p2 = ids["man"], ids["p1"], ids["p2"]

    def run(x, log=None, tags=None, holder="same", store=None, historical=False):
        return verify(x.log_bytes() if log is None else log, x.remote_tags if tags is None else tags, x.commits,
                      x.store if store is None else store, x.independent_copy if holder == "same" else holder,
                      slots, historical)

    cases = {}
    cases["honestMandatorySequence"] = run(ex, ex.log_bytes(mandatory_len - 1), ex.remote_tags[:4],
                                           ex.independent_copy[:4])
    cases["honestFullExample"] = run(ex)

    # T1: truncate after entry 2 (ANCHOR_RECORDED of v1), append recomputed entries, revert the anchor file at HEAD
    t1_entries = [dict(e) for e in ex.entries[:3]]
    prev = t1_entries[-1]["entryHash"]
    for j, (ev, role, actor, sh) in enumerate([("AUTHORIZED", "APPROVAL_ROLE", COORD, None),
                                               ("CAPTURED", "CAPTURE_ROLE", CP, ill("forged manifest")),
                                               ("REVIEWED", "REVIEW_ROLE", OWNER, ill("forged manifest")),
                                               ("APPROVED", "APPROVAL_ROLE", COORD, ill("forged manifest"))]):
        e = make_entry(3 + j, utc_of(100 + j), role, actor, ev, "MANIFEST", sh, prev, captureId="CAP-1")
        t1_entries.append(e)
        prev = e["entryHash"]
    t1_store = dict(ex.store)
    t1_store[ANCHOR_PATH] = ex.commits[ex.remote_tags[0]["peeledCommit"]][ANCHOR_PATH]  # HEAD copy reverted to v1
    cases["T1_truncateAndRewriteAfterFirstAnchor"] = run(ex, b"".join(jcs_bytes(e) + b"\n" for e in t1_entries),
                                                         store=t1_store)

    # T2: in the state after entry 14 (tags v1..v1-a3), edit the pending IN_USE entry 14 and recompute its hash
    t2_entries = [dict(e) for e in ex.entries[:15]]
    t2_entries[14]["runId"] = "EXAMPLE-RUN-3-EDITED"
    t2_entries[14]["entryHash"] = entry_hash(t2_entries[14])
    cases["T2_editPendingEntryAfterLastAnchor"] = run(ex, b"".join(jcs_bytes(e) + b"\n" for e in t2_entries),
                                                      ex.remote_tags[:4], ex.independent_copy[:4])

    # T3: tag ct21d-baseline/v1-a1 deleted at the remote; its ANCHOR_RECORDED entry (7) is covered by v1-a2
    cases["T3_tagDeletedWhileItsEntryIsCovered"] = run(
        ex, tags=[t for t in ex.remote_tags if t["name"] != "ct21d-baseline/v1-a1"])

    # T4: the last tag (v2-a1) deleted at the remote and its pending ANCHOR_RECORDED entry (23) removed
    cases["T4a_lastTagDeletedAndPendingEntryRemoved_copyDesignatedHolderNotConsulted"] = run(
        ex, ex.log_bytes(22), ex.remote_tags[:5], None)
    cases["T4b_lastTagDeletedAndPendingEntryRemoved_copyDesignatedHolderConsulted"] = run(
        ex, ex.log_bytes(22), ex.remote_tags[:5])

    # T5: anchoring window: tag v2-a1 pushed (section 9 step 5), ANCHOR_RECORDED (23) not yet appended,
    #     the holder has not yet written it
    cases["T5_anchoringWindow_lastTagNotYetInCopy"] = run(ex, ex.log_bytes(22), ex.remote_tags,
                                                          ex.independent_copy[:5])

    # T6 (R1-type): every ct21d-baseline tag deleted at the remote and the log reset to entries [0, 1]
    cases["T6_allTagsDeletedAndLogReset"] = run(ex, ex.log_bytes(1), [])

    # T7 (R2-type): the remote rewritten with re-created tags whose anchor records say UNDESIGNATED, and a
    #     forged manifest; the holder keeps the real anchor list
    forged = Example(forge="FORGED")
    build_mandatory(forged, manifest_label="forged manifest")
    t7_store = dict(forged.store)
    cases["T7_tagsRecreatedWithUndesignatedAnchorRecords"] = verify(
        forged.log_bytes(), forged.remote_tags, forged.commits, t7_store, ex.independent_copy, slots)

    # T8: a second live LOCK_MODE pin (supersedes = the first) with no SUPERSEDED pair; runs consume both
    x = Example()
    xi = build_mandatory(x)
    p1x = x.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE-2", "v1", LOCK_SRC, supersedes=xi["p1"])   # 14
    a4 = x.anchor("v1", 4)                                                                             # 15
    x.use("EXAMPLE-RUN-5", [xi["p1"]], a4, identity=xi["man"])                                         # 16
    x.use("EXAMPLE-RUN-6", [p1x], a4, identity=xi["man"])                                              # 17
    cases["T8_twoLivePinsForOneSlot"] = run(x)

    # T9: carry-over block incomplete (the v1 E04 pin neither carried over nor revoked); a run under v2
    #     consumes it
    x = copy.deepcopy(after17)
    p1b = x.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE", "v2", LOCK_SRC, supersedes=p1)          # 18
    x.close("SUPERSEDED", "PIN", p1, superseded_by=p1b)                                                # 19
    b0 = x.anchor("v2", 0)                                                                             # 20
    x.use("EXAMPLE-RUN-4", [p1b, p2], b0, identity=man)                                                # 21
    cases["T9_carryOverIncompleteAndOldPinUsed"] = run(x)

    # T10: after the full example, a pin recorded under v2 whose record names baseline v1, and a pin for a
    #      slot the sealed slot table lacks
    x = copy.deepcopy(ex)
    x.pin("HOST_DEFAULT_MAP", "HOST_DEFAULT_MAP", "EXAMPLE-MAP", "v1",
          [{"path": "EXAMPLE/evidence/hdm.json", "sha256": ill("hdm evidence")}])                     # 24
    x.pin("NOT-A-SLOT", "NOT-A-SLOT", "EXAMPLE", "v2",
          [{"path": "EXAMPLE/evidence/x.json", "sha256": ill("x evidence")}])                         # 25
    x.anchor("v2", 2)                                                                                  # 26
    cases["T10_pinVersionAndSlotChecks"] = run(x)

    # T11 (honest): manifest SUPERSEDED -> REVOKED, and AUTHORIZED -> REVOKED with subjectHash null
    x = Example()
    xi = build_mandatory(x)
    m2 = ill("manifest identity 2")
    x.manifest("CAP-2", m2)                                                                            # 14-17
    x.close("SUPERSEDED", "MANIFEST", xi["man"], capture_id="CAP-1", superseded_by=m2)                 # 18
    x.close("REVOKED", "MANIFEST", xi["man"], capture_id="CAP-1", cause="DEFECT",
            note="illustrative defect found after supersession")                                     # 19
    x.append("APPROVAL_ROLE", COORD, "AUTHORIZED", "MANIFEST", None, captureId="CAP-3")                # 20
    x.close("REVOKED", "MANIFEST", None, capture_id="CAP-3", cause="OTHER",
            note="authorization withdrawn before capture")                                           # 21
    x.anchor("v1", 4)                                                                                  # 22
    cases["T11_manifestRevokedAfterSupersededAndAfterAuthorized"] = run(x)

    # T12: a pin revoked after the last anchor, then a use recorded before any anchor covers the revocation
    x = Example()
    xi = build_mandatory(x)
    x.close("REVOKED", "PIN", xi["p2"], cause="OTHER")                                                 # 14
    x.use("EXAMPLE-RUN-5", [xi["p1"]], xi["a3"], identity=xi["man"])                                   # 15
    cases["T12_useRecordedWhileARevocationIsPending"] = run(x, tags=x.remote_tags, holder=x.independent_copy)

    # T13: a run bound at its start to v1-a2 (h = 9) consumes the pins recorded at 11 and 12; v1-a3, which
    #      covers them, was recorded (13) before the run's IN_USE entry (14)
    x = Example()
    xi = build_mandatory(x)
    x.use("EXAMPLE-RUN-5", [xi["p1"], xi["p2"]], xi["a2"], identity=xi["man"])                          # 14
    cases["T13_runStartAnchorDoesNotCoverItsPins"] = run(x)

    # T14: historical verification of the revision whose log ends at index 8, with every current tag
    cases["T14_historicalRevisionIndexes0to8"] = run(ex, ex.log_bytes(8), historical=True)
    cases["T14_sameRevisionVerifiedAsCurrent"] = run(ex, ex.log_bytes(8))

    # T15 (honest): BA-07 changed at v2, so the manifest is REVOKED (SUPERSEDED_BY_RULING) in the block and
    #      captured again under v2
    x = copy.deepcopy(after14)
    x.set_ba07_seal("BA-07 seal (a later BA-07 version)", x.tick())
    x.new_baseline("v2", ill("BA-11 seal v1"))                                                         # 15-17
    q1 = x.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE", "v2", LOCK_SRC, supersedes=p1)           # 18
    x.close("SUPERSEDED", "PIN", p1, superseded_by=q1)                                                 # 19
    x.close("REVOKED", "PIN", p2, cause="SUPERSEDED_BY_RULING")                                        # 20
    x.close("REVOKED", "MANIFEST", man, capture_id="CAP-1", cause="SUPERSEDED_BY_RULING",
            note="BA-07 changed at v2: no carry-over")                                               # 21
    x.anchor("v2", 0)                                                                                  # 22
    m3 = ill("manifest identity 3")
    x.manifest("CAP-2", m3)                                                                            # 23-26
    c1 = x.anchor("v2", 1)                                                                             # 27
    x.use("EXAMPLE-RUN-4", [q1], c1, capture_id="CAP-2", identity=m3)                                  # 28
    cases["T15_ba07ChangedManifestRevokedAndRecaptured"] = run(x)

    # T16: BA-07 changed at v2 but BASELINE_APPROVED(v2) cites a MANIFEST_CARRY_OVER record for CAP-1
    x = copy.deepcopy(after14)
    x.set_ba07_seal("BA-07 seal (a later BA-07 version)", x.tick())
    x.new_baseline("v2", ill("BA-11 seal v1"), carry=[("CAP-1", man)])                                 # 15-17
    q1 = x.pin("LOCK_MODE", "LOCK_MODE", "EXAMPLE-LOCK-MODE", "v2", LOCK_SRC, supersedes=p1)           # 18
    x.close("SUPERSEDED", "PIN", p1, superseded_by=q1)                                                 # 19
    x.close("REVOKED", "PIN", p2, cause="SUPERSEDED_BY_RULING")                                        # 20
    x.anchor("v2", 0)                                                                                  # 21
    cases["T16_manifestCarriedOverAlthoughBa07Changed"] = run(x)

    # T17: the E04 pin consumed at 14 superseded within the same baseline version
    x = copy.deepcopy(after14)
    q2 = x.pin("E04_ADMITTED_SET", "E04_ADMITTED_SET@R-1", ["EXAMPLE-MEMBER-1"], "v1", E04_SRC, supersedes=p2)  # 15
    x.close("SUPERSEDED", "PIN", p2, superseded_by=q2)                                                 # 16
    x.anchor("v1", 4)                                                                                  # 17
    cases["T17_consumedE04PinSupersededWithinItsVersion"] = run(x)

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
        "artifact": "BA-07 V5 (CT21D-BASE-MANIFEST-CUSTODY), section 4.1 and section 12",
        "status": "the serialization profile and the bytes and hashes below are the NORMATIVE vector of BA-07 V5 "
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
            "pinRecordHashes": {"LOCK_MODE v1": p1, "E04_ADMITTED_SET@R-1 v1": p2, "LOCK_MODE v2": ids["p1b"]},
            "mandatorySequence": {"headIndex": mandatory_len - 1,
                                  "headEntryHash": ex.entries[mandatory_len - 1]["entryHash"],
                                  "logFileSha256": sha256_hex(ex.log_bytes(mandatory_len - 1))},
            "fullExample": {"headIndex": len(ex.entries) - 1, "headEntryHash": ex.entries[-1]["entryHash"],
                            "logFileSha256": sha256_hex(full_log)},
        },
        "verificationModel": cases,
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
