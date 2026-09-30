# I-52 — CT-21D Baseline Artifact BA-07: Manifest and Baseline Custody Procedure (DRAFT V4)

> **BASELINE ARTIFACT BA-07 V4 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized. No manifest exists, no custody log exists, no pin record exists, and no anchor has been created.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-MANIFEST-CUSTODY
> ARTIFACT_VERSION          = 4-DRAFT   (supersedes 3-DRAFT, blob 3a67dd482e12247eaf3a5bfbc196512ae5c1d379, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V4 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification. Clauses applied: V2.1 section 29, V2.2 PA-13, V2.4 note V24-N2
> DELTA RULING APPLIED      = decisions section 214: AR3-23 (model A anchor, remote discovery, pending window, new baseline versions,
>                             JCS serialization and vector, pins after the baseline, independent-copy designation), AR3-22 (the EVM_FROZEN
>                             pin cites INVENTORY_REVIEW_RECORDS), AR3-25 (pin values and BA-11's own status live outside BA-11),
>                             AR3-30 (BA-07 minors). Section 211 still applies where section 214 does not change it (AR2-16, AR2-39, AR2-44)
> FILES                     = this file; docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.py and its output
>                             docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.json (normative vector, section 4.1)
> DEPENDS_ON                = BA-01 (registry entries only; BA-11 V4 section 2.1). No edge to BA-05 (section 4 defines its own encoding
>                             of doubles, AR3-23) and none to BA-09 (section 3 cites V2.1 13.1)
> PREREQUISITES             = the Coordinator's designation of the independent copy (location and holder), recorded in the BA-07
>                             candidate record or in the Coordinator's ratification record of BA-07 (gate: before seal; section 2);
>                             the Architect's answer to AQ-V4-12 (the reading of the pin state machine and the carry-over to a new
>                             baseline version), or its recorded deferral (before the candidate)
> BLOCKER                   = NB-6  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until this procedure is sealed and the baseline anchor is published)
> MANIFEST_CAPTURED         = NO (execution prerequisite EXEC-11)     CUSTODY_LOG_CREATED = NO     ANCHOR_CREATED = NO (procedure only, section 9)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose and threat model

This procedure gives audit integrity to:

- (a) the **baseline record** (the final hash registry BA-11 and this procedure), anchored once when the baseline is complete, and again for each new baseline version (section 8);
- (b) the **pin records** (AR2-16, AR2-17), written after the baseline and before the first governing run that uses them. Each pin is sealed by its own Coordinator and Architect ratification records and is custodied here. BA-11 carries only the sealed slot definitions and never receives a pin value (AR3-23, AR3-25; section 5.1);
- (c) the **manifest** of loaded code (E-10, E-11M, E-13), captured later for the exact build and machine (EXEC-11);
- (d) the **execution-phase review records** of V2.4 note `V24-N2` (the `REVIEWED` inventory record and any reopened operation-class review);
- (e) the **anchors** themselves: each anchor is a custody event of its own, `ANCHOR_RECORDED` (model A, AR3-23).

The manifest attests the enumerable known set only and proves **no isolation** (`NG-13..NG-18`).

| Property | Statement |
|---|---|
| **tamper evidence** | **yes, for every entry at or before the last anchored index**: accidental change, unrecorded change, truncation and rewriting of the custody chain at or before any anchored index are detectable against the anchors discovered at the remote (section 6.2 steps 1-4) |
| **declared limitation (pending window)** | the entries **after the last anchored index** are **not tamper-evident until the next anchor covers them** (AR3-23). The window always holds at least the `ANCHOR_RECORDED` entry of the last anchor, plus any pin, review-record, manifest, use or supersession entry appended since. The verifier lists them as `PENDING_REANCHOR`. No check detects an edit, removal or replacement of them that keeps the entries well formed and legal (a recomputed chain). Steps 4 and 5 of 6.2 still check their form and transitions, and check the last `ANCHOR_RECORDED` entry against the remote. V2.1 29.4 re-anchors a later event "in the next baseline or evidence commit", so the window is inherent. The use preconditions of section 7 require the approval of a manifest, and every pin a run uses, to be anchored before the run's `IN_USE` entry, so what a run relies on is never inside the window; the run's own `IN_USE` entry is inside it until the next anchor |
| **non-repudiation and signer identity** | **no**: there is no cryptographic signature and no proof of who acted |
| out of the threat model | a hostile local or repository administrator (`NG-13`); self-review by one person is a declared limit (section 7.1) |
| **declared limitation (remote rewrite)** | branches and tags at the remote can be rewritten or deleted by an account with push rights. A rewrite or deletion of an anchor tag or anchor commit is detectable **only** by comparison with an **independent copy** of the anchor identities, kept outside the repository and outside the rewritten references (section 2; section 6.2 step 6). **Until the Coordinator designates that copy (location and holder), a remote rewrite is not detectable** (AR2-39). The decisions log and any file inside the same repository are **not** an independent copy |

## 2. What is stored where (designated locations)

| Item | Location | In the repository? |
|---|---|---|
| baseline artifacts and their files | the paths listed in the **BA-11 registry** (the registry is the authority for the location of every baseline artifact) | yes |
| custody log (append-only, section 6) | `docs/automation/evidence/ct21d/custody-log.jsonl` (**planned path; not created by this artifact**); one log for every baseline version | yes (hashes, roles and actors; no machine data) |
| anchor record (section 9) | `docs/automation/evidence/ct21d/baseline-anchor.json` (**planned; not created**). One fixed path: each anchor commit holds the record of that anchor. Verification reads it **at each tag's peeled commit**, never at the verified revision | yes |
| candidate, ratification and designation records (section 5.2), including BA-11's own candidate hash, ratifications and seal hash, which are never written into BA-11 | `docs/automation/evidence/ct21d/ratifications/<subject>-<hash>-<kind>.json` (**planned**); `<kind>` = `candidate`, `coordinator`, `architect`, `seal` or `designation` | yes |
| pin records (section 5.1) | `docs/automation/evidence/ct21d/pins/<pinKind>-<pinRecordHash>.json` (**planned**). BA-11 carries only the sealed slot definitions. After the baseline, a pin's value, state and record hash live **only** in its pin record, its two ratification records and this custody log; they are **never** written into BA-11 (AR3-23, AR3-25) | yes (hashes and values; machine-profile content stays local, only its hash enters) |
| execution-phase review records (`V24-N2`) | `docs/automation/evidence/ct21d/review-records/` (**planned**), content-addressed by their file hash (section 4) | yes |
| manifest, build layer | `docs/automation/evidence/ct21d/manifest-build-<hash>.json` (planned) | yes (the layer and its hash) |
| manifest, machine-profile layer | local evidence store `%LOCALAPPDATA%\RackCad\ct21d-evidence\manifest-machine-<hash>.json` on the characterization machine (planned) | **no**: only its hash enters the repository |
| session-evaluated snapshots | run evidence packages (V2.1 18.2) | per the evidence policy of the run |
| **independent copy of the anchor identities** | **UNDESIGNATED in these bytes.** The Coordinator designates a location outside the repository and a holder. The designation is recorded in the **BA-07 candidate record or in the Coordinator's ratification record of BA-07** (field `independentCopy`, section 5.2) and is **repeated in the `INDEPENDENT_COPY` field of every anchor record** (section 9); 6.2 step 6 reads it there. The holder writes each anchor identity at anchoring time (section 9, step 6) | **no** |

The repository locations of this table are part of these bytes: the Coordinator designates them by ratifying this artifact, and a change of any of them is a new version of this artifact. The independent copy is designated **outside these bytes** (AR3-23). A change of its location or holder is a new Coordinator ruling, recorded the same way in a designation record (section 5.2) that cites the BA-07 seal hash, and repeated in the `INDEPENDENT_COPY` field of every later anchor record; it is not a new version of this artifact. A newly designated holder receives the full list of earlier anchor identities, checked against the remote at that time.

## 3. Manifest capture (control plane; not executed here)

1. The Coordinator **authorizes** the capture: an `AUTHORIZED` entry (`APPROVAL_ROLE`) with a new `captureId` and `subjectHash = null` (the manifest does not exist yet).
2. The **control plane** starts a fresh controlled session on the exact build, machine and profile.
3. The control plane runs the **warm-up of V2.1 13.1**, then the E-10 enumerations and the hasher (V2.1 29.1). The manifest is **generated by the control plane** and is **never hand-edited**.
4. The control plane serializes the manifest canonically (section 4). The SHA-256 of the manifest record file is the **manifest identity**.
5. The manifest layers are written to the designated locations (section 2), read-only.
6. The control plane writes a `CAPTURED` entry (`CAPTURE_ROLE`, actor `CONTROL_PLANE:<build id>`) with the same `captureId` and `subjectHash` = the manifest identity.
7. Review (`REVIEWED`, `REVIEW_ROLE`) and approval (`APPROVED`, `APPROVAL_ROLE`) follow section 7. The manifest is usable only after an anchor covers its approval (section 7).

## 4. Canonical serialization and hash (AR3-23)

Every record this procedure writes or hashes is serialized with **JCS, RFC 8785** (JSON Canonicalization Scheme), restricted to the profile below. This covers custody-log entries, anchor records, pin records, ratification and designation records, manifest layers and tuple records. The profile, including the encoding of doubles, is **defined here** and is not taken from any other artifact.

| Aspect | Rule |
|---|---|
| standard | RFC 8785: no insignificant whitespace; object members sorted by name (UTF-16 code units); array order kept; literals `true`, `false`, `null`. Strings: `"` and the backslash are escaped; U+0008, U+0009, U+000A, U+000C and U+000D are written as the two-character escapes `\b`, `\t`, `\n`, `\f`, `\r`; every other code point below U+0020 is written as a backslash, `u` and four **lowercase** hexadecimal digits; every other character is written as itself, including `/` and non-ASCII characters |
| member names | printable ASCII only (U+0020..U+007E), fixed by the schemas of this artifact. For them the RFC 8785 order equals ordinal byte order |
| numbers | integers in [-(2^53-1), 2^53-1] only, written in decimal. A non-integer JSON number is **not admitted** |
| doubles | a double is carried as a JSON **string** of the 16 lowercase hexadecimal digits of its IEEE-754 binary64 bit pattern, big-endian (0.1 is `"3fb999999999999a"`). `-0.0` (`"8000000000000000"`) and `+0.0` (`"0000000000000000"`) differ. NaN and infinities are **refused**: the record is not written |
| strings | valid Unicode (no lone surrogate); no Unicode normalization is applied |
| presence | every field of a schema is present; a field that does not apply is `null`, never absent; no member outside the schema |
| arrays | in the order the schema declares. Arrays that are sets are sorted: `ratificationRecords` and `sourceEvidence` by `path`, `pinRecordHashes` ascending, module lists by normalized full path (ordinal = Unicode scalar value order = UTF-8 byte order) |
| encoding | UTF-8, no BOM |
| record file | the JCS bytes of the record followed by **exactly one LF**, nothing else |
| record hash | SHA-256 of the record file bytes, lowercase hexadecimal. This gives the manifest identity, the pin-record hash, the anchor-record hash, `tupleRecordHash` and the `sha256` of a cited record |
| other files | a file that is not a record of this procedure (a review record, an evidence file) is hashed as the SHA-256 of its bytes after CRLF is normalized to LF |
| log line | the JCS bytes of the complete entry (`entryHash` included) followed by one LF; no blank line, no CR |
| `entryHash` | SHA-256 of the JCS bytes of the entry **without** its `entryHash` member (no LF) |
| association with the tuple | the manifest hash is a tuple field. A run's recorded manifest hash is compared with the manifest identity that is `APPROVED` in the custody log for that tuple and **anchored** (section 7), the one the run records `IN_USE`. A difference makes the run **`INVALID`** (V2.1 29.1; phase-1 ruling G-2). There is no manifest at the baseline (section 8) |

### 4.1 Normative vector

The generator `docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.py` and its output `docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.json` are part of this artifact. `python docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.py <output.json>` regenerates the output exactly: it writes LF-only bytes, equal to the LF-normalized bytes of the published output. It is deterministic and uses no network, git, host or AutoCAD. The hashes below were computed with Python `hashlib.sha256` over the actual bytes after CRLF is normalized to LF. The files as generated contain no CR; a checkout with `core.autocrlf=true` may show CRLF on disk, and the normalized bytes are unchanged.

| Item | SHA-256 |
|---|---|
| output file `I-52-ct21d-ba07-v4-jcs-vector.json` | `e7e6c9bec109dd3bc014dcfa5efac27f48674180c79cfd23f6e7df9222bb705e` |
| generator file (also recorded inside the output) | `35e7dba4f380fd7a889fbe15b051bb303d62b3dd6f72d23da4599a9a9ea27e88` |
| **example entry** (worked example, index 2): its `entryHash` | `07e8bf8442a17638c38ef9a3bb3a2191a6986931ac52d82f51eb4227937ee322` |
| worked example, mandatory sequence: `entryHash` of the head entry (index 13) | `1e145b2915fd43734530d160dcc9f8ddbda7b3f3aa9c79b218c69dd1d509ea6e` |
| worked example, mandatory sequence: log file (indexes 0-13) | `8b9549594d72940a450aaf0cd6504fa77657f7e1760982df6647230826964bb0` |
| worked example with its continuation: `entryHash` of the head entry (index 18) | `801b9663edd8b539a81806f2ae79d9c923b04b014d0d5debdfa056f5e7cddf90` |
| worked example with its continuation: log file (indexes 0-18) | `7647737c3f1a7d0534b940b3b9841a5e831e660ec15972241345cfd0d47ab7d5` |

The example entry serialized without its `entryHash` is 812 UTF-8 bytes; their SHA-256 is the `entryHash` above:

```text
{"actor":"EXAMPLE-COORDINATOR","anchorRef":{"anchoredIndex":1,"peeledCommit":"9cb16b0a7c197ab076a6d163cac057f076c8512b","remote":"https://github.com/marioap-afk/Calculadora_de_racks.git","tag":"ct21d-baseline/v1","tagObjectId":"230e153c655c4c2fe65c039e4d335e428e186a32"},"baselineCustodyHash":null,"baselineVersion":null,"captureId":null,"cause":null,"event":"ANCHOR_RECORDED","index":2,"note":"first anchor of baseline \"v1\" (illustrative) — café","pinRecordHashes":null,"pinSlot":null,"prevEntryHash":"ce4ad0a3e8739b801545935bac6c4f36498437aecff2181096ed0961f6a64c3f","ratificationRecords":null,"role":"CUSTODY_ROLE","runId":null,"subjectHash":"68f2da8eb52a032075e4fbb4a1ddb51c063fb942dbc27567cd5c4aed947025e7","subjectKind":"ANCHOR","supersededBy":null,"tupleRecordHash":null,"utc":"2000-01-01T00:03:00Z"}
```

The output also carries:

- the string example of RFC 8785 section 3.2.2.2, reproduced exactly (its `numbers` member is outside this profile);
- five double encodings, and the refusal of NaN, the two infinities, a non-integer JSON number and the integer 2^53;
- every log line and `entryHash` of the worked example, and its anchor records;
- a cross-check: Python `json.dumps` with sorted keys, compact separators and no ASCII escaping gives the same bytes for every object serialized (320 of 320). This holds for this profile only.

**Every value of the worked example is illustrative** (derived from `EXAMPLE:` labels; it is not the hash of any real artifact, run, tag or commit). The normative content is the profile and the bytes and hashes it produces. An implementation conforms when it reproduces the example entry bytes and hash, and every line and `entryHash` of the output's `logLines`.

## 5. Storage, retrieval and versioning

| Aspect | Rule |
|---|---|
| storage | content-addressed and read-only: `<kind>-<hash>.json` at the designated location. The anchor record is the exception: one fixed path, one version per anchor commit, each bound by its tag |
| retrieval | by hash; the hash is **verified on read**; a mismatch refuses use. An anchor record is read at its tag's peeled commit and verified against the `subjectHash` of its `ANCHOR_RECORDED` entry |
| amendment and versioning | a manifest, a pin record, a review record or a baseline record is **never edited**. A change is a **new capture, a new pin version, a new record or a new baseline version**, with a new hash and a supersession link (pin records: field `supersedes`; custody log: `SUPERSEDED` with `supersededBy`). Superseded items are retained |

### 5.1 Pin records and their own seal (AR2-16, AR3-23)

A pin is a named slot in the sealed catalog; the slot definitions are sealed with BA-11 (BA-11 V4 section 3.1). The pin **value** lives in a pin record:

| Field | Content |
|---|---|
| `recordKind` | `"CT21D_PIN_RECORD"` |
| `pinKind` | `E04_ADMITTED_SET`, `LOCK_MODE`, `EVM_FROZEN`, `HOST_DEFAULT_MAP` or `FIXTURE_INSTANCE` (AR2-16) |
| `pinSlot` | the sealed slot it fills, for example `E04_ADMITTED_SET@R-1` or `FIXTURE_INSTANCE@<FX>` |
| `baselineVersion` | the baseline version whose sealed slot it fills (`v<k>`, section 9) |
| `value` | the pinned value. Content bound to a machine profile is carried as its SHA-256 and stays in local evidence |
| `sourceEvidence` | the evidence the value was frozen from: `[{path, sha256}]`, sorted by `path` |
| `inventoryReviewRecords` | **only for `EVM_FROZEN`**: the SHA-256 of the `REVIEWED` inventory record and of every reopened operation-class review record on which the frozen EVM rests (V2.4 `V24-N2`). It carries the EVM-level field `INVENTORY_REVIEW_RECORDS` of the frozen EVM and must equal it (AR3-22). Otherwise `null` |
| `supersedes` | the pin-record hash of the previous version of the same slot, or `null` |
| `frozenUtc` | UTC time at which the value was frozen |
| `recordedBy` | the actor who wrote the record (a person or `CONTROL_PLANE:<build id>`) |

Lifecycle (the pin machine of AR3-23, `RECORDED -> RATIFIED -> ANCHORED`):

| State | Reached when | Evidence |
|---|---|---|
| `RECORDED` | the pin record is written, after the value is frozen from its own evidence (never from the outcome of a scenario that uses it) | the pin record file, verified by its hash |
| `RATIFIED` | a **Coordinator** ratification record and an **Architect** ratification record, each citing the exact pin-record hash, exist under `ratifications/`. They are the pin's **own seal** (AR2-16, AR3-23). The `PIN_RECORDED` custody entry (`APPROVAL_ROLE`) is then appended and **cites both** in `ratificationRecords` | the two ratification records and the `PIN_RECORDED` entry |
| `ANCHORED` | derived: an anchor covers the `PIN_RECORDED` entry (section 7) | the anchor |

`PIN_RECORDED` is appended only for a `RATIFIED` pin: it records the pin in custody together with its seal. An unratified pin record is not in custody and cannot be used. That the `RECORDED` state has no custody-log entry of its own, and that `PIN_RECORDED` is appended once both ratification records exist, is this draft's reading of the pin state machine, pending AQ-V4-12. Nothing is written into BA-11: a pin write never changes the bytes of BA-11 or of the catalog and never creates a baseline version (section 8). For `EVM_FROZEN`, every hash in `inventoryReviewRecords` must be the `subjectHash` of a `REVIEW_RECORD_ADDED` entry with a lower index than the `PIN_RECORDED` entry.

### 5.2 Candidate, ratification and designation records (fields this procedure reads)

| Field | Content |
|---|---|
| `recordKind` | `"CT21D_RATIFICATION"` |
| `ratifier` | `COORDINATOR` or `ARCHITECT` |
| `actor` | the person |
| `subjectKind` | `ARTIFACT` or `PIN` |
| `subject` | the artifact id (for example `CT21D-BASE-MANIFEST-CUSTODY`) or the pin slot |
| `subjectHash` | the candidate hash (artifact) or the pin-record hash (pin); in a designation record, the BA-07 seal hash |
| `decision` | `CANDIDATE_FOR_SEALING` (the Architect's candidate ruling), `RATIFIED`, or `DESIGNATION` (a later Coordinator designation of the independent copy) |
| `utc` | UTC time |
| `reference` | the decisions-log reference of the ruling |
| `independentCopy` | `{holder, location}` in the BA-07 record that records the Coordinator's designation of the independent copy (the BA-07 candidate record or the Coordinator's ratification record of BA-07) and in a later designation record; otherwise `null` |

The other content of the candidate, ratification and seal records of artifacts is governed by the sealing workflow of BA-11 V4 section 2.

## 6. Hash-chained custody log

An **append-only** JSON-lines log (log lines of section 4). The **genesis entry** (index 0) is the baseline record of the first baseline version (`BASELINE_RECORDED`, section 8). It is followed by the baseline approval (`BASELINE_APPROVED`) and by the `ANCHOR_RECORDED` entry of the first anchor. Anchor, pin, review-record, manifest and use events, and new baseline versions, are appended to the same log; the log is never restarted.

### 6.1 Entry (normative fields)

Every entry carries every field below; a field that does not apply is `null` (section 4).

| Field | Content |
|---|---|
| `index` | integer, 0 for the genesis entry, then +1 per line |
| `utc` | UTC time, `YYYY-MM-DDThh:mm:ssZ` |
| `role` | `CAPTURE_ROLE`, `REVIEW_ROLE`, `APPROVAL_ROLE` or `CUSTODY_ROLE`, fixed per event (section 7) |
| `actor` | a person identifier or `CONTROL_PLANE:<build id>` |
| `event` | `BASELINE_RECORDED`, `BASELINE_APPROVED`, `ANCHOR_RECORDED`, `PIN_RECORDED`, `REVIEW_RECORD_ADDED`, `AUTHORIZED`, `CAPTURED`, `REVIEWED`, `APPROVED`, `IN_USE`, `SUPERSEDED`, `REVOKED`. There is no `ANCHORED` event: anchoring is derived (section 7) |
| `subjectKind` | `BASELINE`, `ANCHOR`, `PIN`, `REVIEW_RECORD` or `MANIFEST` |
| `subjectHash` | `BASELINE`: the BA-11 seal hash; `ANCHOR`: the anchor-record hash; `PIN`: the pin-record hash; `REVIEW_RECORD`: the file hash of the record; `MANIFEST`: the manifest identity (`null` only in `AUTHORIZED`) |
| `captureId` | only in `MANIFEST` entries: the capture identifier `CAP-<n>` given by `AUTHORIZED`; it is the key of the manifest subject |
| `baselineVersion` | only in `BASELINE_RECORDED` and `BASELINE_APPROVED`: `v<k>` |
| `baselineCustodyHash` | only in `BASELINE_RECORDED`: the BA-07 seal hash (this procedure) |
| `ratificationRecords` | only in `BASELINE_APPROVED` (the candidate, Coordinator, Architect and seal records of BA-11) and in `PIN_RECORDED` (the Coordinator and Architect ratification records of the pin): `[{path, sha256}]`, sorted by `path` |
| `pinSlot` | only in `PIN_RECORDED`: the slot the pin record fills |
| `runId` | only in `IN_USE`: the run id of the run that uses the manifest |
| `tupleRecordHash` | only in `IN_USE`: the record hash of the canonical tuple record of that run; `MACHINE_PROFILE_BOUND` values stay in local evidence |
| `pinRecordHashes` | only in `IN_USE`: the pin-record hashes the run consumes, ascending (an empty array when none) |
| `cause` | only in `REVOKED`: `TOOLING`, `ENVIRONMENT`, `CONTAMINATION`, `DEFECT`, `SUPERSEDED_BY_RULING` or `OTHER`, with the explanation in `note` |
| `supersededBy` | only in `SUPERSEDED`: the `subjectHash` of the successor (a new BA-11 seal hash, pin-record hash or manifest identity). The successor must already be approved (baseline, manifest) or recorded (pin) at a lower index |
| `anchorRef` | only in `ANCHOR_RECORDED`: `{anchoredIndex, peeledCommit, remote, tag, tagObjectId}`, that is h, the anchor commit, the remote URL, the tag name and the tag object id |
| `prevEntryHash` | the `entryHash` of the previous entry; the genesis entry uses 64 zero digits |
| `note` | free text, may be empty |
| `entryHash` | SHA-256 of the JCS bytes of all the other fields (section 4) |

### 6.2 Verification algorithm

Inputs: the custody log at the verified revision, the designated remote `REMOTE_URL` (section 9), and the independent copy when it is designated. The verified revision is the latest revision that carries the custody log; the file content is what is verified, never commit ancestry. **The anchors are taken from the remote.** They are never taken from inside the log, from local tags, from branch ancestry or from the anchor record at the verified revision (AR3-23).

1. **Entries.** Every line ends with LF; there is no CR and no blank line. Each line must equal the JCS bytes of the parsed object and carry exactly the fields of 6.1 (`NON_CANONICAL_ENTRY`). Its `index` must equal its line number, counted from 0 (`INDEX_SEQUENCE`). Recompute every `entryHash`; a difference is a **broken entry** (`BROKEN_ENTRY`).
2. **Chain.** Check each `prevEntryHash` against the preceding entry; index 0 uses 64 zero digits. A difference is a **broken chain** (`BROKEN_CHAIN`).
3. **Anchors discovered at the remote; comparison at every anchored index.**
   - (a) List the anchors with `git ls-remote --tags <REMOTE_URL> 'refs/tags/ct21d-baseline/*'`. Every listed ref must match the tag-name scheme of section 9 (`UNEXPECTED_TAG_NAME`). Every tag must be annotated, that is listed with its peeled line `^{}` (`LIGHTWEIGHT_TAG`). The unpeeled id is the tag object id; the `^{}` id is the peeled commit.
   - (b) Fetch those objects into a private namespace, so that local tags cannot shadow them: `git fetch --no-tags <REMOTE_URL> '+refs/tags/ct21d-baseline/*:refs/ct21d-verify/*'`. Each fetched ref must resolve to the listed tag object id and peel to the listed commit.
   - (c) At each tag's peeled commit, read the anchor record: `git show <peeled commit>:docs/automation/evidence/ct21d/baseline-anchor.json`. It must exist and be a canonical record file (`ANCHOR_RECORD_MISSING`, `ANCHOR_RECORD_NON_CANONICAL`). Its `ANCHOR_TAG_NAME`, `ANCHOR_SEQUENCE` and `BASELINE_VERSION` must match the tag name (`ANCHOR_RECORD_NAME_MISMATCH`). Its record hash is the anchor-record hash. It gives `h` = `CUSTODY_LOG_HEAD_INDEX` and `H` = `MANIFEST_CUSTODY_HEADER_HASH`. The custody log at the same commit must end at index `h`, with `entryHash` = `H` (`ANCHOR_COMMIT_LOG_MISMATCH`).
   - (d) Order the anchors by `h`; `h` must strictly increase (`ANCHOR_ORDER`). In that order the names must follow the scheme `v1`, `v1-a1`, `v1-a2`, ..., then `v2`, `v2-a1`, ..., with no gap and no repetition (`TAG_SEQUENCE`).
   - (e) For **every** anchor, not only the last: `h` must be an index of the verified log (`TRUNCATED_BELOW_ANCHOR`), and the `entryHash` of the entry **at index `h`** must equal `H` (`REWRITTEN_AT_OR_BEFORE_ANCHOR`). The comparison is **never** made against the current head. Every `entryHash` covers `prevEntryHash`, so equality at `h` binds the whole prefix `0..h`.
   - If no tag exists, the status is `UNANCHORED`: no entry is tamper-evident.
4. **One-to-one match of tags and `ANCHOR_RECORDED` entries; pending window.**
   - Every tag of step 3 has exactly one `ANCHOR_RECORDED` entry whose `anchorRef.tag` names it (`UNRECORDED_ANCHOR`, `DUPLICATE_ANCHOR_RECORDED`). Every `ANCHOR_RECORDED` entry names a tag of step 3 (`ANCHOR_WITHOUT_TAG`).
   - For each pair, `anchorRef` (tag object id, peeled commit, anchored index, remote) must equal what step 3 found, and `subjectHash` must equal the anchor-record hash (`ANCHOR_REF_MISMATCH`). The entry's index must be greater than that anchor's `h` (`ANCHOR_RECORDED_BEFORE_HEAD`). For every anchor except the last, it must be at most the `h` of the next anchor, so a later anchor covers it (`ANCHOR_RECORDED_NOT_COVERED`).
   - The **last** tag only may have no entry yet: status `LAST_ANCHOR_NOT_RECORDED`, meaning section 9 step 6 is not complete at the verified revision. It is part of the pending window.
   - Entries with an index greater than the `h` of the last anchor are **`PENDING_REANCHOR`**. They are listed in the result and are **not tamper-evident** (section 1): until the next anchor covers them, no step detects an edit, removal or replacement of them that keeps them well formed and legal. This is declared and is not a failure (AR3-23).
5. **State machines, fields and use preconditions (section 7).** For every subject check: the legal transitions and the role of each event; the conditional fields of 6.1 (`ILLEGAL_TRANSITION`, `ROLE_MISMATCH`, `FIELD_RULE`, `MANIFEST_IDENTITY_CHANGED`); the baseline sequence of section 8 (`BASELINE_SEQUENCE`); and that each anchor record's `BA11_SEAL_HASH`, `BA07_SEAL_HASH` and `BASELINE_VERSION` equal those of the baseline in force at its `h` (`ANCHOR_RECORD_BASELINE_MISMATCH`). Also check the pin seals and the `EVM_FROZEN` review records of 5.1 (`PIN_SEAL_INVALID`, `PIN_REVIEW_RECORDS_MISSING`) and the use preconditions (`USE_BEFORE_ANCHOR`, `PIN_USE_BEFORE_ANCHOR`). Pin, ratification and review records are read at the verified revision and verified by their hashes. The step also reports the derived state of every subject (section 7).
6. **Independent copy.** Read the designation from the `INDEPENDENT_COPY` field of the anchor records (step 3). It repeats the designation recorded in the BA-07 candidate record or in the Coordinator's ratification record of BA-07, or in a later designation record. The cited designation record must exist, match its SHA-256 and name the same location and holder (`DESIGNATION_RECORD_MISMATCH`). Obtain from the holder the list of anchor identities (tag name, tag object id, peeled commit, `h`, anchor-record hash) and compare it with the **whole** tag list of step 3. A tag missing on either side, or any differing value, means the remote references were rewritten (`REMOTE_REWRITE_DETECTED`). While the anchor records carry `INDEPENDENT_COPY = UNDESIGNATED`, or the copy cannot be consulted, this step reports **`NOT_VERIFIABLE`** (section 1).

**Result.** `VIOLATION` if any step reports a violation code (the codes in parentheses above). Otherwise `PASS_WITH_DECLARED_LIMITS`, always stated with: the last anchored index, the list of `PENDING_REANCHOR` entries, the statuses (`UNANCHORED`, `LAST_ANCHOR_NOT_RECORDED`), the result of step 6 (`MATCH` or `NOT_VERIFIABLE`) and the derived state of every subject. The verifier never reports a pass without these limits. A historical revision is verified the same way; the anchors created after it are then reported as `TRUNCATED_BELOW_ANCHOR`, as expected. The vector generator of 4.1 holds a model of steps 1-6, limited to the checks that the worked example exercises (it has no `EVM_FROZEN` pin and no revocation), and runs it on the example (section 12).

## 7. Roles and transitions

| Role | Holds | May not |
|---|---|---|
| `CAPTURE_ROLE` | runs capture **through the control plane** | approve its own capture |
| `REVIEW_ROLE` | the Owner or CAD manager (or a delegate they record): checks the machine-layer entries and the build layer against the build receipt | capture or approve |
| `APPROVAL_ROLE` | the Coordinator: authorizes a capture, approves the baseline (`BASELINE_APPROVED`), a manifest hash (`APPROVED`) and a ratified pin (`PIN_RECORDED`), and records supersession and revocation | capture |
| `CUSTODY_ROLE` | **the Coordinator logical role by default** (designated by this artifact; a change is a new version): keeps the custody log, records the baseline, the anchors, the review records and each use | alter an entry already anchored |

Role per event (checked by 6.2 step 5):

| Event | Role | Event | Role |
|---|---|---|---|
| `BASELINE_RECORDED` | `CUSTODY_ROLE` | `CAPTURED` | `CAPTURE_ROLE` (actor `CONTROL_PLANE:<build id>`) |
| `BASELINE_APPROVED` | `APPROVAL_ROLE` | `REVIEWED` | `REVIEW_ROLE` |
| `ANCHOR_RECORDED` | `CUSTODY_ROLE` | `APPROVED` | `APPROVAL_ROLE` |
| `PIN_RECORDED` | `APPROVAL_ROLE` | `IN_USE` | `CUSTODY_ROLE` (actor: the control plane of the run or the custody holder) |
| `REVIEW_RECORD_ADDED` | `CUSTODY_ROLE` | `SUPERSEDED` | `APPROVAL_ROLE` |
| `AUTHORIZED` | `APPROVAL_ROLE` | `REVOKED` | `APPROVAL_ROLE` |

State machines per subject (the key of a subject is its `subjectHash`, except a manifest, whose key is its `captureId`):

```text
BASELINE:       BASELINE_RECORDED -> BASELINE_APPROVED -> [SUPERSEDED, by a new baseline version (section 8)]
ANCHOR:         ANCHOR_RECORDED   (exactly one per remote tag, 6.2 step 4; no later event)
PIN:            RECORDED (pin record) -> RATIFIED (two ratification records; PIN_RECORDED cites them) -> ANCHORED (derived)
                PIN_RECORDED -> [SUPERSEDED by a new pin version | REVOKED]
REVIEW_RECORD:  REVIEW_RECORD_ADDED   (no later event; a reopened review is a new record)
MANIFEST:       AUTHORIZED -> CAPTURED -> REVIEWED -> APPROVED -> IN_USE -> IN_USE -> ... -> SUPERSEDED
                every state except SUPERSEDED and REVOKED -> REVOKED (with a recorded cause)
                subjectHash = the manifest identity from CAPTURED on, and it never changes
```

**Derived anchoring (model A, AR3-23).** A subject is **`ANCHORED`** when its last entry has an index at most `h` of a verified anchor (6.2 step 3); otherwise it is **`PENDING_REANCHOR`**. There is no `ANCHORED` entry, so one anchor covers any number of subjects and a re-anchor after `IN_USE`, `SUPERSEDED` or `REVOKED` needs no transition of those subjects. The contract's `APPROVED -> ANCHORED` transition by `CUSTODY_ROLE` (V2.1 29.3, 29.4) is, in model A, the `ANCHOR_RECORDED` entry of an anchor whose `h` is at least the index of the `APPROVED` entry. That custody entry is separate from the approval entry (V2.2 PA-13).

**Use preconditions** (6.2 step 5):

- **`IN_USE`** is recorded **at each use**, with the run id, the tuple record hash and the pin-record hashes the run consumes (V2.1 29.3). **`IN_USE -> IN_USE` is legal** (AR3-23): a second use does not wait for the first to be anchored.
- A manifest that is not anchored cannot be `IN_USE`. For an `IN_USE` entry at index `u`, the manifest's `APPROVED` entry must have an index at most the `anchoredIndex` of an `ANCHOR_RECORDED` entry whose own index is lower than `u`.
- A pin that is not anchored cannot be used by a governing run. For every hash in `pinRecordHashes` of that `IN_USE` entry, the pin's `PIN_RECORDED` entry must have an index at most the `anchoredIndex` of an `ANCHOR_RECORDED` entry with an index lower than `u`. The pin must not be `SUPERSEDED` or `REVOKED` at an index lower than `u`. Every governing run records its pins this way.
- `SUPERSEDED` and `REVOKED` never return to `IN_USE`.
- The approval entries (`BASELINE_APPROVED`, `APPROVED`, `PIN_RECORDED`) and the custody entries (`BASELINE_RECORDED`, `ANCHOR_RECORDED`) are **separate log entries** even when the same person holds the approval and the custody roles (phase-1 ruling item (11); V2.2 PA-13).

### 7.1 Multi-role policy (`YES_WITH_ROLE_SEPARATION_IN_LOG`)

One human may hold several logical roles. Requirements: each role transition is a separate log entry; the actor is recorded in every entry; capture is always generated by the control plane; the role multiplicity is disclosed in Owner Act 2 (count of entries by actor and role). Self-review is a declared limit.

## 8. Baseline records, new baseline versions and the status of BA-11

At the baseline there is **no execution manifest** (it is captured later, EXEC-11). The first entries are:

| Field | Index 0 | Index 1 |
|---|---|---|
| `event` | `BASELINE_RECORDED` | `BASELINE_APPROVED` |
| `subjectKind` | `BASELINE` | `BASELINE` |
| `subjectHash` | the **seal hash of the final BA-11** (the hash registry, which binds every other artifact) | the same |
| `baselineVersion` | `v1` | `v1` |
| `baselineCustodyHash` | the **seal hash of this BA-07** | `null` |
| `ratificationRecords` | `null` | the candidate, Coordinator, Architect and seal records of BA-11 (its candidate hash, both ratifications and its seal hash; these are **never** written into BA-11) |
| `role` / `actor` | `CUSTODY_ROLE` / the Coordinator | `APPROVAL_ROLE` / the Coordinator |
| `prevEntryHash` | 64 zero digits | the `entryHash` of index 0 |

The first anchor (`ct21d-baseline/v1`) has `h = 1`; its `ANCHOR_RECORDED` entry is index 2.

**New baseline version** (AR3-23). A new BA-11 seal, for example after a new version of any sealed artifact (BA-11 V4 section 2), is recorded in the **same log**, never in a new one. Three consecutive entries are appended:

1. a new `BASELINE_RECORDED` (the new BA-11 seal hash, the BA-07 seal hash in force, `baselineVersion = v<k+1>`);
2. a new `BASELINE_APPROVED` (its ratification records);
3. `SUPERSEDED` of the previous baseline (`subjectHash` = the previous BA-11 seal hash, `supersededBy` = the new one).

The first anchor of the new version (`ct21d-baseline/v<k+1>`) is then created with `h` = the index of that `SUPERSEDED` entry. Index 0 stays the genesis entry of `v1`. The ruling that approves the new version decides whether the pins and the manifest in custody stay usable under it. Those that do not are `SUPERSEDED` or `REVOKED` (cause `SUPERSEDED_BY_RULING`) in the entries that follow. This carry-over of the pins and the manifest to a new baseline version is this draft's reading, pending AQ-V4-12.

A pin write, a review record, a manifest event or a use **never** creates a baseline version. They are custody events of the baseline in force and are re-anchored as `ct21d-baseline/v<k>-a<n>` (section 9). BA-11 V4 section 3.1 is a sealed slot table and never receives a value.

**Status of BA-11** (AR2-44, AR3-25). BA-11's own status lives outside BA-11:

- `CANDIDATE_FOR_SEALING` and `RATIFIED` in its candidate and ratification records (section 2);
- `SEALED` as the subject of `BASELINE_RECORDED` and `BASELINE_APPROVED`;
- anchored as derived (section 7);
- `SUPERSEDED` by the `SUPERSEDED` entry of a later baseline version.

## 9. External anchor (procedure; nothing is created here)

**The first anchor** of the first baseline version is created **only after**:

- every baseline artifact is finalized and sealed;
- BA-11 is sealed last, with the seal hash and the ratification records of **every other** artifact. BA-11's own candidate hash, ratifications and seal hash are recorded in section 2's records and in the entries of section 8, never inside BA-11;
- every ratification is recorded;
- entries 0 and 1 of section 8 are appended.

**A final anchor is never created earlier.** **Every later anchor** needs at least one entry after the previous `h`, and the `ANCHOR_RECORDED` entry of every earlier tag must be in the log, so that the new anchor covers it.

| Step | Rule |
|---|---|
| 1 | the `CUSTODY_ROLE` fixes `h` = the index of the last entry of the log and `H` = its `entryHash`. The log is held: no entry is appended between this step and step 6 |
| 2 | it writes the anchor record (schema below) at `docs/automation/evidence/ct21d/baseline-anchor.json` as a canonical record file (section 4) |
| 3 | the anchor record and the custody log, which ends at index `h`, are committed in a **reviewed** commit (the anchor commit) |
| 4 | the commit is **pushed** to the remote `origin` (`https://github.com/marioap-afk/Calculadora_de_racks.git`), and its presence at the remote is **verified** (`git ls-remote` lists a branch ref at that commit) |
| 5 | an **annotated tag** (name below) is created on the anchor commit and pushed. `git ls-remote --tags` must list its tag object id and, on the `^{}` line, the anchor commit. **The annotated tag is the reference of record**: feature branches may be rebased under the repository workflow, so verification uses the **tag's peeled commit**, never branch ancestry (AR2-39) |
| 6 | the `CUSTODY_ROLE` appends the **`ANCHOR_RECORDED`** entry: `subjectKind = ANCHOR`, `subjectHash` = the anchor-record hash, `anchorRef` = `{anchoredIndex: h, peeledCommit, remote, tag, tagObjectId}`. It commits and pushes this entry (the anchor-recording commit) and verifies its presence at the remote. The designated holder writes the anchor identity (tag name, tag object id, peeled commit, `h`, anchor-record hash, UTC time) to the **independent copy** (section 2). Until that copy is designated, the limitation of section 1 applies and 6.2 step 6 reports `NOT_VERIFIABLE`. The `ANCHOR_RECORDED` entry is itself pending until the next anchor |
| 7 | every later custody event (pin, review record, manifest event, use, supersession, revocation, new baseline version) is re-anchored in the next evidence commit with steps 1-6 (V2.1 29.4). The `ANCHOR_RECORDED` entry of an anchor does not by itself call for a new anchor, since anchoring would then never end: the next anchor that another event calls for covers it. Until then these entries form the pending window (section 1) |

**Tag names.** `ct21d-baseline/v<k>` names the first anchor of baseline version `k` (`k` = 1, 2, ..., decimal, no leading zero). `ct21d-baseline/v<k>-a<n>` names the later anchors of that version, `n` = 1, 2, ... in anchoring order. No other ref is created under `ct21d-baseline/`. The baseline version `v<k>` is the `baselineVersion` of the `BASELINE_RECORDED` entry.

Anchor record schema (a canonical record file; **template only; every value is `UNSET` or designated**):

```text
RECORD_KIND                  = "CT21D_BASELINE_ANCHOR"
ANCHOR_TAG_NAME              = ct21d-baseline/v<k> or ct21d-baseline/v<k>-a<n> (designated pattern; reference of record)
ANCHOR_SEQUENCE              = 0 for ct21d-baseline/v<k>; n for ct21d-baseline/v<k>-a<n>
BASELINE_VERSION             = v<k>
CUSTODY_LOG_HEAD_INDEX       = UNSET   (h)
MANIFEST_CUSTODY_HEADER_HASH = UNSET   (entryHash of the entry at index h)
BA11_SEAL_HASH               = UNSET   (subjectHash of the baseline in force at h)
BA07_SEAL_HASH               = UNSET   (baselineCustodyHash of that baseline's BASELINE_RECORDED entry)
CONTRACT_COMPOSITE_HASH      = UNSET   (the BA-01 SEAL_HASH)
REMOTE_URL                   = https://github.com/marioap-afk/Calculadora_de_racks.git (designated)
INDEPENDENT_COPY             = UNDESIGNATED in these bytes; at anchoring time {designationRecord: {path, sha256}, holder, location}
```

The tag object id and the peeled commit are **not** fields of the anchor record: they exist only after the record is committed, and a commit cannot contain its own id. They live in the `ANCHOR_RECORDED` entry and in the independent copy. The time and actor of the anchoring are the `utc` and `actor` of that entry.

## 10. Audit integrity: what each check protects

| Change | Detected by |
|---|---|
| an artifact, pin record, ratification record, anchor record or manifest edited in place | content addressing: the hash is verified on read against BA-11 (artifacts) or the custody log (`subjectHash`, the `sha256` of `ratificationRecords`, the anchor-record hash read at the peeled commit) |
| an entry edited at or before the last anchored index | recomputation of `entryHash` (6.2 step 1); with a recomputed chain, the comparison at the anchored indexes (step 3) |
| an entry inserted, removed or reordered at or before the last anchored index | index sequence (step 1), chain check (step 2), comparison at the anchored indexes (step 3) |
| the log truncated below an anchored index, or truncated and re-appended with a recomputed chain, even with `baseline-anchor.json` reverted at the verified revision | step 3: the anchors are discovered at the remote and their records are read at each tag's peeled commit (section 12, case T1) |
| an `ANCHOR_RECORDED` entry forged, altered or dropped while a later anchor covers it; a tag deleted while its entry is covered | step 4, the one-to-one match (`ANCHOR_REF_MISMATCH`, `UNRECORDED_ANCHOR`, `ANCHOR_WITHOUT_TAG`) (case T3) |
| a change to an entry after the last anchored index (the pending window) that keeps the entries well formed and legal | **not detectable** until the next anchor covers it (declared, section 1; case T2) |
| the last tag deleted at the remote together with its pending `ANCHOR_RECORDED` entry; any tag or anchor commit rewritten at the remote | comparison of the whole tag list with the independent copy (step 6; case T4); **not detectable until the copy is designated** |
| an illegal transition, a wrong role, a missing or extra field | state-machine and field checks (step 5) |
| a manifest or a pin used before it is anchored | use preconditions (step 5) |

## 11. What remains before NB-6 can close

| Id | Item | Gate |
|---|---|---|
| N6-1 | Architect ruling `CANDIDATE_FOR_SEALING` on the exact bytes of this V4 and of its two vector files (the registered paths of BA-07 in BA-11 V4), with the Coordinator's designation of the independent copy recorded in the candidate record or in the Coordinator's ratification record (section 2); then the Coordinator and Architect ratifications; then the seal. Preconditions: BA-01 at least `CANDIDATE_FOR_SEALING` (BA-11 V4 section 2), and the Architect's agreement to BA-11 V4 sections 1, 2 and 2.1 as the sealing workflow in force (AR3-25) | baseline preparation (no host) |
| N6-2 | the baseline anchor published (section 9) | the **last** step of the baseline, after BA-11 is final |

The captured manifest, the pin records and their anchoring are **execution prerequisites** (EXEC-11 and the pin slots of BA-11 V4 section 3.1) and do not block the baseline.

```text
NB-6 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until this procedure is sealed and the baseline anchor is published
MANIFEST = NOT CAPTURED ; CUSTODY LOG = NOT CREATED ; PIN RECORDS = NONE ; ANCHOR = NOT CREATED ; INDEPENDENT COPY = UNDESIGNATED
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 12. Worked example (mandatory, AR3-23; values illustrative)

The example runs the mandatory sequence **genesis -> approval -> anchor -> manifest -> anchor -> IN_USE x2 -> re-anchor -> pins -> re-anchor** (indexes 0-13). A continuation (indexes 14-18) adds a use that consumes both pins and a new baseline version. The generator of 4.1 builds it; the full lines and hashes are in its output. **Every hash, id, time, actor, run id and value is illustrative** (derived from `EXAMPLE:` labels). None is a hash of a real artifact, and none is a decision about any pin value. Hashes are shortened to 16 hexadecimal digits here; the output holds the full values.

| Index | Event | Role | Subject | Fields that apply | First anchor that covers it | `entryHash` (first 16) |
|---|---|---|---|---|---|---|
| 0 | `BASELINE_RECORDED` | `CUSTODY_ROLE` | BASELINE `a0a1282ca1f6...` (BA-11 seal v1) | `baselineVersion = v1`, `baselineCustodyHash` | `v1` (h = 1) | `31a18c3d6867a0ae` |
| 1 | `BASELINE_APPROVED` | `APPROVAL_ROLE` | BASELINE `a0a1282ca1f6...` | `baselineVersion = v1`, `ratificationRecords` (4) | `v1` (h = 1) | `ce4ad0a3e8739b80` |
| 2 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `68f2da8eb52a...` | `anchorRef`: tag `ct21d-baseline/v1`, anchoredIndex 1 | `v1-a1` (h = 6) | `07e8bf8442a17638` |
| 3 | `AUTHORIZED` | `APPROVAL_ROLE` | MANIFEST `CAP-1` (`subjectHash = null`) | `captureId` | `v1-a1` | `3fc6e8668013e433` |
| 4 | `CAPTURED` | `CAPTURE_ROLE` | MANIFEST `CAP-1` = `b8ff62b0df19...` | `captureId` | `v1-a1` | `a2a77d2accffad2a` |
| 5 | `REVIEWED` | `REVIEW_ROLE` | MANIFEST `CAP-1` | `captureId` | `v1-a1` | `182756a7b672f0d5` |
| 6 | `APPROVED` | `APPROVAL_ROLE` | MANIFEST `CAP-1` | `captureId` | `v1-a1` (h = 6) | `8b4b96877d10f76b` |
| 7 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `db764ef6cddf...` | tag `ct21d-baseline/v1-a1`, anchoredIndex 6 | `v1-a2` (h = 9) | `0291b6e378f8720b` |
| 8 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-1`, `tupleRecordHash`, `pinRecordHashes = []` | `v1-a2` | `fa892145c116a52a` |
| 9 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-2`, `tupleRecordHash`, `pinRecordHashes = []` | `v1-a2` (h = 9) | `a47c333078f8db20` |
| 10 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `3c8b7e26287c...` | tag `ct21d-baseline/v1-a2`, anchoredIndex 9 | `v1-a3` (h = 12) | `f102c94673d0de2f` |
| 11 | `PIN_RECORDED` | `APPROVAL_ROLE` | PIN `d2986250d7cc...` | `pinSlot = LOCK_MODE`, `ratificationRecords` (2) | `v1-a3` | `34fd726a33a4e511` |
| 12 | `PIN_RECORDED` | `APPROVAL_ROLE` | PIN `cba4bc32a7f5...` | `pinSlot = E04_ADMITTED_SET@R-1`, `ratificationRecords` (2) | `v1-a3` (h = 12) | `2bdc992c78fbf56f` |
| 13 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `445b70bda364...` | tag `ct21d-baseline/v1-a3`, anchoredIndex 12 | `v2` (h = 17) | `1e145b2915fd4373` |
| 14 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-3`, `pinRecordHashes` = both pins | `v2` | `453a270c75ea3cde` |
| 15 | `BASELINE_RECORDED` | `CUSTODY_ROLE` | BASELINE `389a7681b132...` (BA-11 seal v2) | `baselineVersion = v2`, `baselineCustodyHash` | `v2` | `a8ab4d024bd3526d` |
| 16 | `BASELINE_APPROVED` | `APPROVAL_ROLE` | BASELINE `389a7681b132...` | `baselineVersion = v2`, `ratificationRecords` (4) | `v2` | `00c762e07159751a` |
| 17 | `SUPERSEDED` | `APPROVAL_ROLE` | BASELINE `a0a1282ca1f6...` (v1) | `supersededBy = 389a7681b132...` | `v2` (h = 17) | `4e468c824ec9d24c` |
| 18 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `c175e6e7614d...` | tag `ct21d-baseline/v2`, anchoredIndex 17 | pending | `801b9663edd8b539` |

| Tag (illustrative remote) | `h` | `MANIFEST_CUSTODY_HEADER_HASH` (first 16) | anchor-record hash (first 16) |
|---|---|---|---|
| `ct21d-baseline/v1` | 1 | `ce4ad0a3e8739b80` | `68f2da8eb52a0320` |
| `ct21d-baseline/v1-a1` | 6 | `8b4b96877d10f76b` | `db764ef6cddf9230` |
| `ct21d-baseline/v1-a2` | 9 | `a47c333078f8db20` | `3c8b7e26287c7d0f` |
| `ct21d-baseline/v1-a3` | 12 | `2bdc992c78fbf56f` | `445b70bda364beab` |
| `ct21d-baseline/v2` | 17 | `4e468c824ec9d24c` | `c175e6e7614d01d0` |

What the example shows (each point is a case V3 rejected, BA07-V3-01):

- The re-anchor after uses (`v1-a2`, h = 9) is a new `ANCHOR` subject. The manifest makes no transition, so it stays at `IN_USE` and becomes `ANCHORED` by derivation.
- Two uses in a row (8, 9) are `IN_USE -> IN_USE`. Both satisfy the use precondition: the approval (6) is covered by `v1-a1`, recorded at 7.
- One re-anchor (`v1-a3`, h = 12) covers two pins (11, 12) with no per-pin entry.
- The use at 14 consumes both pins; they are covered by `v1-a3`, recorded at 13.
- The new baseline version is recorded by 15-17 in the same log, then anchored as `ct21d-baseline/v2` with h = 17. It needs no restart and no new genesis.

Derived states at the end: every subject is `ANCHORED` except the `ANCHOR_RECORDED` entry 18, which is `PENDING_REANCHOR`. Baseline v1's last event is `SUPERSEDED`, baseline v2's is `BASELINE_APPROVED`, and the manifest's is `IN_USE`.

Verification model (the generator's model of 6.2, run on the example and on four tampered variants):

| Case | Result | Codes and limits |
|---|---|---|
| honest, mandatory sequence (indexes 0-13; tags `v1`..`v1-a3`) | `PASS_WITH_DECLARED_LIMITS` | last anchored index 12; pending [13]; step 6 `MATCH` |
| honest, with the continuation (indexes 0-18; five tags) | `PASS_WITH_DECLARED_LIMITS` | last anchored index 17; pending [18]; step 6 `MATCH` |
| T1: the log truncated after index 2 and re-appended with a recomputed chain (the BA07-V3-02 scenario); `baseline-anchor.json` reverted at the verified revision | `VIOLATION` | step 3 `REWRITTEN_AT_OR_BEFORE_ANCHOR` (`v1-a1`), `TRUNCATED_BELOW_ANCHOR` (`v1-a2`, `v1-a3`, `v2`); step 4 `UNRECORDED_ANCHOR`; step 5 `ANCHOR_RECORD_BASELINE_MISMATCH` |
| T2: in the state after index 14 (tags `v1`..`v1-a3`), the pending `IN_USE` entry 14 is edited and its hash recomputed | `PASS_WITH_DECLARED_LIMITS` | pending [13, 14]: the edit is **not detected**; this is the declared window (section 1) |
| T3: tag `ct21d-baseline/v1-a1` deleted at the remote | `VIOLATION` | step 3 `TAG_SEQUENCE`; step 4 `ANCHOR_WITHOUT_TAG` (7); step 5 `USE_BEFORE_ANCHOR` (8, 9); step 6 `REMOTE_REWRITE_DETECTED` |
| T4a: the last tag (`v2`) deleted at the remote and its pending entry 18 removed; copy undesignated | `PASS_WITH_DECLARED_LIMITS` | step 6 `NOT_VERIFIABLE`; pending [13..17]: **not detected** (section 1, remote rewrite) |
| T4b: as T4a, copy designated | `VIOLATION` | step 6 `REMOTE_REWRITE_DETECTED` (`v2`) |

## 13. Delta V4 (decisions section 214)

Sections 1-11 keep their V3 numbers. Sections 4.1, 5.1, 5.2, 12 and 13 are new.

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| CLOSURE BA07-01 (V3 `PARTIALLY_FIXED`) | AR3-23 | **FIXED.** Model A: the `ANCHOR_RECORDED` event (6.1) and derived `ANCHORED` (7). The comparison is made at every anchored index, with the anchors discovered at the remote (6.2 step 3). The coverage rule that could never fire is replaced by the declared pending window (1; 6.2 step 4). The rows of section 10 are rewritten. Honest logs pass (12) |
| BA07-V3-01 | AR3-23 | **FIXED.** The anchor is its own event `ANCHOR_RECORDED` (subject `ANCHOR`, anchor-record hash, `anchorRef`; 6.1). A re-anchor after `IN_USE`, `SUPERSEDED` or `REVOKED` needs no subject transition, and one anchor covers any number of subjects (7). `IN_USE -> IN_USE` is legal (7). A new baseline version is a new `BASELINE_RECORDED` and `BASELINE_APPROVED` plus `SUPERSEDED` of the previous version, in the same log (8). Mandatory worked example (12) |
| BA07-V3-02 | AR3-23 | **FIXED.** 6.2 step 3 lists the anchors with `git ls-remote --tags <REMOTE_URL> 'refs/tags/ct21d-baseline/*'`, reads `baseline-anchor.json` and the log at each tag's peeled commit, orders by `h`, and compares every anchored index. Step 4 requires the one-to-one match of tags and `ANCHOR_RECORDED` entries. Step 6 compares the whole tag list with the independent copy. The header hash is read from the record at the peeled commit, so `anchorRef` does not need it. Section 10 is corrected. Case T1 (12) |
| BA07-V3-03 | AR3-23, AR3-25 | **FIXED.** BA-11 carries only the sealed slots. The pin's own seal is the pair of Coordinator and Architect ratification records that cite the pin-record hash under `ratifications/`, cited by `PIN_RECORDED` (`ratificationRecords`). The pin machine is `RECORDED -> RATIFIED -> ANCHORED` (1(b), 2, 5.1, 7). A pin write never creates a baseline version (8) |
| BA07-V3-04 | AR3-23 | **FIXED** (second option of the finding). The coverage check and its section 10 row are removed. Section 1 declares that entries after the last anchor are not tamper-evident until the next anchor. 6.2 step 4 lists them as `PENDING_REANCHOR`. Case T2 shows an edited pending entry passing (12) |
| BA07-V3-05 | AR3-23 | **FIXED.** The designation (location and holder) is recorded in the BA-07 candidate record or the Coordinator's ratification record (`independentCopy`, 5.2). It is repeated in `INDEPENDENT_COPY` of every anchor record (9) and read there by 6.2 step 6. A change is a new designation record, not a new version of this artifact (2). The V3 sentence that made a change of the copy a new version is corrected |
| BA07-V3-06 | AR3-23, AR3-25 | **FIXED for BA-07.** Doubles are encoded inside BA-07 (4), and the warm-up is cited as V2.1 13.1, not BA-09 (3 step 3). Header `DEPENDS_ON = BA-01`. The BA-10 parts (the PARAM-05 label serialization, the CLEAN count) and the BA-11 edge list belong to BA-10 V4 and BA-11 V4 (cross-artifact notes) |
| BA07-V3-07 | AR3-23 | **FIXED.** JCS (RFC 8785) with the BA-07 profile: member names, integers, doubles as 16-hex strings, presence and `null`, arrays, record files, log lines, `entryHash` (4). Normative vector and generator, with the output hash cited (4.1) |
| BA07-02, BA07-03, BA07-04, BA07-05, BA07-06, BA07-07 (phase 2; `FIXED` in V3) | AR2-39 (02, 05, 06); required fixes (03, 04, 07) | **kept.** `BASELINE_APPROVED` as a separate approval entry (7, 8); `IN_USE` and `REVOKED` fields (6.1); the manifest comparison against the approved and anchored manifest (4); the remote-rewrite limit (1); the annotated tag as reference of record (9 step 5); BA-11 as the authority for artifact locations (2) |
| BA11V3-03 (BA-07 side) | AR3-25, AR3-23 | **FIXED.** Pin values, states and hashes are never written into BA-11 (1, 2, 5.1). Pin events are custody events of the baseline in force, re-anchored as `-a<n>` (8, 9). The BA-11 side belongs to BA-11 V4 |
| BA11V3-08 (BA-07 side) | AR3-25 | **FIXED.** Section 8 states where BA-11's status lives: its candidate and ratification records, `BASELINE_RECORDED` and `BASELINE_APPROVED`, derived anchoring, `SUPERSEDED` |
| BA11V3-02, item (2) (edge BA-07 -> BA-05) | AR3-23, AR3-25 | **FIXED.** The edge is removed: section 4 defines the encoding of doubles |
| AR3-22 pin link (landing of `V24-N2`) | AR3-22 | **FIXED.** The `EVM_FROZEN` pin record field `inventoryReviewRecords` carries the EVM field `INVENTORY_REVIEW_RECORDS`. Each cited record is custodied by `REVIEW_RECORD_ADDED` before `PIN_RECORDED` (5.1; 6.2 step 5) |
| editorial (no finding) | — | The anchor record no longer lists the tag object id, the peeled commit, `VERIFIED_AT_UTC` or `VERIFIED_BY` (9): a commit cannot contain its own id, and these values live in `ANCHOR_RECORDED` and the independent copy. `captureId` is added because `AUTHORIZED` precedes the manifest identity (3; 6.1). A role is fixed per event (7). The `CUSTODY_LOG_CREATED` and `PIN RECORDS` status lines are added |
| AQ-V4-12 citation: pin state machine and carry-over to a new baseline version (correction pass, minor) | AQ-V4-12 (not decided) | **CITED.** Header `PREREQUISITES` adds the Architect's answer to AQ-V4-12, or its recorded deferral (before the candidate). 5.1 marks as this draft's reading, pending AQ-V4-12, that `RECORDED` has no custody-log entry of its own and that `PIN_RECORDED` is appended once both ratification records exist. 8 marks the carry-over of the pins and the manifest to a new baseline version the same way. No field, event, rule or vector changes |
