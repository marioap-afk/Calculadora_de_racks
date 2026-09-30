# I-52 — CT-21D Baseline Artifact BA-07: Manifest and Baseline Custody Procedure (DRAFT V7)

> **BASELINE ARTIFACT BA-07 V7 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized. No manifest exists, no custody log exists, no pin record exists, no build-equivalence or bound-SHA precondition record exists, and no anchor has been created.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-MANIFEST-CUSTODY
> ARTIFACT_VERSION          = 7-DRAFT (supersedes 6-DRAFT, blob 9cac4c6da20d9289c723698c96efb41f850f529e)
> STATUS AUTHORITY          = the BA-07 entry of the BA-11 registry is the only authority for the status and hashes of this artifact;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 section 2, rule 1)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 2), V2.4 revision 3 and
>                             V2.5 revision 2 textually verified by the Coordinator (decisions sections 218 and 221); + V2.6 (draft,
>                             pending review; AR6-05). The effective contract is not yet agreed (AR6-06)
> BOUND_PRODUCT_SHA         = 95690c28 (AR6-01; recorded by the Coordinator in decisions section 224 as a non-tuple bound item;
>                             never a tuple value, AR5-12). BA-07 states no code fact and binds nothing to this value (Gate 2
>                             relink assessment, BA-07 row: it cites no product code); the record of section 5.4 reads the bound
>                             product SHA in force from the BA-11 registry
> CLAUSES APPLIED           = V2.1 sections 2.1-2.2, 21.3, 27.3, 27.5 and 29; V2.2 PA-13; V2.4 section 1 and note V24-N2; V2.5
>                             section 1 (`CATALOG_FOLDER`)
> DELTA RULING APPLIED      = decisions section 226 (AR7-03: the dedicated precondition verifier detects the precondition for the
>                             union of the cited pairs, section 5.4; AR7-04: U6 widened to every run on a build under test; AR7-05:
>                             the reading of 5.4 is sufficient, with its three text conditions, sections 5.4 and 7; AR7-09: the
>                             minors BA07V6-02 to BA07V6-04, sections 5.3, 5.4, 7 and 10; section 13), section 223 (AR6-02: the `FIXTURE_CONSTRUCTION_BUILD` as a third build role whose identity
>                             lives in the fixture conformance record, conditions FXB-03 and FXB-04, section 5.5; AR6-01, AR6-05 and
>                             AR6-06 as alignment) and section 220 (AR5-04: the reverse direction of the recorded build equivalence,
>                             section 5.3 and U5; AR5-06: the readings (a) to (c) of AQ-V5-04, with the disclosure and the completion
>                             of "consumed", section 5.1; AR5-12: the bound-SHA precondition record, section 5.4 and U6). Sections
>                             217 (AR4-..), 214 and 211 still apply where sections 220, 223 and 226 do not change them
> FILES                     = this file; the normative vector of section 4.1, unchanged from V5 (byte-identical):
>                             docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.py and its output
>                             docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.json. V6 and V7 change no schema, event,
>                             role, transition or case that the vector covers (section 4.1). The V4 vector files stay as history.
>                             The dedicated precondition verifier of AR7-03
>                             (docs/automation/evidence/I-52-ct21d-precondition-verifier.py) is not a file of this artifact: it is
>                             an execution-preparation tool outside the BA-11 registry (no row, no edge), in the custody of this
>                             procedure (section 5.4)
> DEPENDS_ON                = BA-01 (registry entries only; BA-11 section 2.1). No edge to BA-05 (section 4 defines its own encoding
>                             of doubles, AR3-23), none to BA-09 (section 3 cites V2.1 13.1) and none to BA-03, BA-04, BA-06 or
>                             BA-08: sections 5.1, 5.3, 5.4, 5.5 and 7 cite V2.1, V2.4, V2.5, AR4-06, AR5-04, AR5-12 and AR6-02
>                             directly, and every mention of BA-03 V5, BA-04 V6, BA-06 V6 or BA-08 V6, of the sections of BA-03,
>                             BA-06 and BA-08 that declare their precondition sets (5.4), of the runs that AR7-04 names (U6) and of
>                             the precondition verifier of AR7-03 is an informative pointer, not an edge (AR4-17)
> PREREQUISITES             = the Coordinator's designation of the independent copy (location and holder), recorded in the BA-07
>                             candidate record or in the Coordinator's ratification record of BA-07 (gate: before seal; section 2).
>                             AQ-V6-BA07-1 is ruled by AR7-05 and AQ-V6-01 by AR7-03 (section 226), AQ-V5-04 and AQ-V5-01 by AR5-06
>                             and AR5-04 (section 220), all applied in this version; they are no longer prerequisites. Open
>                             Architect question of this artifact: none (section 11.1)
> BLOCKER                   = NB-6  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until this procedure is sealed and the baseline anchor is published)
> MANIFEST_CAPTURED         = NO (execution prerequisite EXEC-11)     CUSTODY_LOG_CREATED = NO     ANCHOR_CREATED = NO (procedure only, section 9)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose and threat model

This procedure gives audit integrity to:

- (a) the **baseline record** (the final hash registry BA-11 and this procedure), anchored once when the baseline is complete, and again for each new baseline version (section 8);
- (b) the **pin records** (AR2-16, AR2-17), written after the baseline and before the first governing run that uses them. Each pin belongs to one baseline version, is sealed by its own Coordinator and Architect ratification records and is custodied here; a slot holds at most one live pin (AR4-13; section 5.1). BA-11 carries only the sealed slot definitions and never receives a pin value (AR3-23, AR3-25);
- (c) the **manifest** of loaded code (E-10, E-11M, E-13), captured later for the exact build and machine (EXEC-11), one for each build under test (section 3);
- (d) the **execution-phase review records** of V2.4 note `V24-N2` (the `REVIEWED` inventory record and any reopened operation-class review);
- (e) the **anchors** themselves: each anchor is a custody event of its own, `ANCHOR_RECORDED` (model A, AR3-23);
- (f) the **build-equivalence record** of AR4-06 (4) and AR5-04 (section 5.3): the verified and recorded equivalence of the `CHARACTERIZATION_BUILD` and the `PRODUCT_BUILD`, under which the pins derived on the characterization build apply to `PRODUCT_BUILD` runs and, in the reverse direction, the `EVM_FROZEN` pin learned and validated on the product build applies to governing `CHARACTERIZATION_BUILD` runs;
- (g) the **bound-SHA precondition record** of AR5-12 (section 5.4): for each build under test, the result of the comparison of the (path, object) pairs that BA-03, BA-06 and BA-08 declare for the precondition, and the W-1 rows of BA-09 section 3.2 (the extension of AR7-03 submitted to the Architect for confirmation), at the bound product SHA with the pairs at the source SHA of that build, as the dedicated precondition verifier of AR7-03 detects it.

The manifest attests the enumerable known set only and proves **no isolation** (`NG-13..NG-18`).

| Property | Statement |
|---|---|
| **tamper evidence** | **yes, for every entry at or before the last anchored index**: accidental change, unrecorded change, truncation and rewriting of the custody chain at or before any anchored index are detectable against the anchors discovered at the remote (section 6.2 steps 1-4) |
| **declared limitation (pending window)** | the entries **after the last anchored index** are **not tamper-evident until the next anchor covers them** (AR3-23). The window always holds at least the `ANCHOR_RECORDED` entry of the last anchor, plus any pin, review-record, manifest, use, supersession or revocation entry appended since. The verifier lists them as `PENDING_REANCHOR`. No check detects an edit, removal or replacement of them that keeps the entries well formed and legal (a recomputed chain). Steps 4 and 5 of 6.2 still check their form and transitions, and check the last `ANCHOR_RECORDED` entry against the remote. V2.1 29.4 re-anchors a later event "in the next baseline or evidence commit", so the window is inherent. What a run relies on is bound **before the run**, outside the window (section 7): the `IN_USE` entry carries the run's **run-start anchor**, fixed when the run's BUILD_BOUND fields are sampled, which must cover the approval of the manifest and every pin the run consumes (and, where U5 applies, the build-equivalence record, 5.3, and, for every run on a build under test, the bound-SHA precondition record of its build, 5.4, U6); and every supersession or revocation appended before the run's `IN_USE` entry must be covered by an anchor recorded before that entry. Two limits remain and are declared: the run's own `IN_USE` entry is inside the window until the next anchor; and a supersession or revocation lost from the window before any anchor covers it is not detectable, although no use could legally be recorded after it while it was in the log (section 10) |
| **non-repudiation and signer identity** | **no**: there is no cryptographic signature and no proof of who acted |
| out of the threat model | a hostile local or repository administrator (`NG-13`); self-review by one person is a declared limit (section 7.1) |
| **declared limitation (remote rewrite)** | branches and tags at the remote can be rewritten or deleted by an account with push rights. A rewrite or deletion of an anchor tag or anchor commit is detectable **only** by comparison with an **independent copy** of the anchor identities, kept by a designated holder outside the repository and outside the rewritten references (section 2; section 6.2 step 6). The designation (location and holder) is a before-seal prerequisite of this artifact and every anchor is created after every seal (section 9), so every conforming anchor record names it; an anchor record that says `UNDESIGNATED` is a violation (`DESIGNATION_MISSING`). Step 6 reads the designation from the anchor records and, as a cross-check and whenever no tag exists, from the primary designation record (section 2), and consults the holder. What stays undetectable: a rewrite while the holder **cannot be consulted** (`NOT_VERIFIABLE`; AR2-39), and a rewrite of the last anchor before the holder has written it (the anchoring window of section 9 step 6: status `LAST_ANCHOR_NOT_IN_COPY`, with the entries after the last copy-confirmed anchor reported as not confirmed). The decisions log and any file inside the same repository are **not** an independent copy |

## 2. What is stored where (designated locations)

| Item | Location | In the repository? |
|---|---|---|
| baseline artifacts and their files | the paths listed in the **BA-11 registry** (the registry is the authority for the location of every baseline artifact) | yes |
| custody log (append-only, section 6) | `docs/automation/evidence/ct21d/custody-log.jsonl` (**planned path; not created by this artifact**); one log for every baseline version | yes (hashes, roles and actors; no machine data) |
| anchor record (section 9) | `docs/automation/evidence/ct21d/baseline-anchor.json` (**planned; not created**). One fixed path: each anchor commit holds the record of that anchor. Verification reads it **at each tag's peeled commit**, never at the verified revision | yes |
| candidate, ratification, designation and carry-over records (section 5.2), including BA-11's own candidate record and its two ratification records, which establish BA-11's seal hash and are never written into BA-11 | `docs/automation/evidence/ct21d/ratifications/<subject>-<hash>-<kind>.json` (**planned**): `<subject>` = the artifact id, the pin slot or the manifest's `captureId`; `<hash>` = the record's `subjectHash`; `<kind>` = `candidate`, `coordinator`, `architect`, `designation-<n>` (`n` = 1, 2, ... in designation order) or `carryover-v<k>` (the baseline version a manifest is carried over to) | yes |
| pin records (section 5.1) | `docs/automation/evidence/ct21d/pins/<pinKind>-<pinRecordHash>.json` (**planned**). BA-11 carries only the sealed slot definitions (BA-11 section 3.1). After the baseline, a pin's value, state and record hash live **only** in its pin record, its two ratification records and this custody log; they are **never** written into BA-11 (AR3-23, AR3-25) | yes (hashes and values; machine-profile content stays local, only its hash enters) |
| execution-phase review records (`V24-N2`) | `docs/automation/evidence/ct21d/review-records/` (**planned**), content-addressed by their file hash (section 4) | yes |
| build-equivalence records (section 5.3; AR4-06 (4), AR5-04) | `docs/automation/evidence/ct21d/build-equivalence/equivalence-<recordHash>.json` (**planned; not created**), content-addressed by the record hash (section 4). A reviewed diff that a record cites is stored in the same folder as `diff-<sha256>` followed by the extension of the diff file, content-addressed by its file hash | yes (hashes, paths and the reviewed diff; no machine data) |
| bound-SHA precondition records (section 5.4; AR5-12) | `docs/automation/evidence/ct21d/bound-sha-precondition/precondition-<recordHash>.json` (**planned; not created**), content-addressed by the record hash (section 4). The output of the dedicated precondition verifier of AR7-03 (`docs/automation/evidence/I-52-ct21d-precondition-verifier.py`, an execution-preparation tool outside the BA-11 registry, in the custody of this procedure; 5.4) and the delta-processing evidence that a record cites are stored in the same folder as `evidence-<sha256>` followed by the extension of the file, content-addressed by its file hash | yes (hashes, paths, blob ids and source SHAs; no machine data) |
| manifest, build layer | `docs/automation/evidence/ct21d/manifest-build-<hash>.json` (planned), one for each build under test (section 3) | yes (the layer and its hash) |
| manifest, machine-profile layer | local evidence store `%LOCALAPPDATA%\RackCad\ct21d-evidence\manifest-machine-<hash>.json` on the characterization machine (planned) | **no**: only its hash enters the repository |
| session-evaluated snapshots | run evidence packages (V2.1 18.2) | per the evidence policy of the run |
| **independent copy of the anchor identities** | **UNDESIGNATED in these bytes.** The Coordinator designates a location outside the repository and a holder. The designation is recorded in the **BA-07 candidate record or in the Coordinator's ratification record of BA-07** (field `independentCopy`, section 5.2). That record is the **primary designation record**: it is found through the BA-07 seal hash (the `baselineCustodyHash` of the `BASELINE_RECORDED` entry in force) and checked against what the BA-07 entry of the BA-11 registry cites for it. The designation is **repeated in the `INDEPENDENT_COPY` field of every anchor record** (section 9); 6.2 step 6 reads it there and in the primary designation record. The holder writes each anchor identity at anchoring time (section 9, step 6) | **no** |

The repository locations of this table are part of these bytes: the Coordinator designates them by ratifying this artifact, and a change of any of them is a new version of this artifact. The independent copy is designated **outside these bytes** (AR3-23). A change of its location or holder is a new Coordinator ruling, recorded the same way in a designation record (`<kind>` = `designation-<n>`, section 5.2) that cites the BA-07 seal hash, and repeated in the `INDEPENDENT_COPY` field of every later anchor record; it is not a new version of this artifact. A newly designated holder receives the full list of earlier anchor identities, checked against the remote at that time.

## 3. Manifest capture (control plane; not executed here)

One manifest is captured for **each build under test** that governing runs use (AR4-06 (1)-(2): the characterization build carries its own manifest build layer, with its own E-10 exact set and WUM set). Each capture is its own subject with its own `captureId`, and a run is compared with the manifest approved for its own build (section 4).

1. The Coordinator **authorizes** the capture: an `AUTHORIZED` entry (`APPROVAL_ROLE`) with a new `captureId` and `subjectHash = null` (the manifest does not exist yet).
2. The **control plane** starts a fresh controlled session on the exact build, machine and profile.
3. The control plane runs the **warm-up of V2.1 13.1**, then the E-10 enumerations and the hasher (V2.1 29.1). The manifest is **generated by the control plane** and is **never hand-edited**.
4. The control plane serializes the manifest canonically (section 4). The SHA-256 of the manifest record file is the **manifest identity**.
5. The manifest layers are written to the designated locations (section 2), read-only.
6. The control plane writes a `CAPTURED` entry (`CAPTURE_ROLE`, actor `CONTROL_PLANE:<build id>`) with the same `captureId` and `subjectHash` = the manifest identity.
7. Review (`REVIEWED`, `REVIEW_ROLE`) and approval (`APPROVED`, `APPROVAL_ROLE`) follow section 7. The manifest is usable only after an anchor covers its approval, and only under the baseline version it was approved under or carried over to (sections 7 and 8).

The `FIXTURE_CONSTRUCTION_BUILD` of AR6-02 is not a build under test: no manifest is captured for it (section 5.5).

## 4. Canonical serialization and hash (AR3-23)

Every record this procedure writes or hashes is serialized with **JCS, RFC 8785** (JSON Canonicalization Scheme), restricted to the profile below. This covers custody-log entries, anchor records, pin records, candidate, ratification, designation and carry-over records, build-equivalence records, bound-SHA precondition records, manifest layers and tuple records. The profile, including the encoding of doubles, is **defined here** and is not taken from any other artifact.

| Aspect | Rule |
|---|---|
| standard | RFC 8785: no insignificant whitespace; object members sorted by name (UTF-16 code units); array order kept; literals `true`, `false`, `null`. Strings: `"` and the backslash are escaped; U+0008, U+0009, U+000A, U+000C and U+000D are written as the two-character escapes `\b`, `\t`, `\n`, `\f`, `\r`; every other code point below U+0020 is written as a backslash, `u` and four **lowercase** hexadecimal digits; every other character is written as itself, including `/` and non-ASCII characters |
| member names | printable ASCII only (U+0020..U+007E), fixed by the schemas of this artifact. For them the RFC 8785 order equals ordinal byte order |
| numbers | integers in [-(2^53-1), 2^53-1] only, written in decimal. A non-integer JSON number is **not admitted** |
| doubles | a double is carried as a JSON **string** of the 16 lowercase hexadecimal digits of its IEEE-754 binary64 bit pattern, big-endian (0.1 is `"3fb999999999999a"`). `-0.0` (`"8000000000000000"`) and `+0.0` (`"0000000000000000"`) differ. NaN and infinities are **refused**: the record is not written |
| strings | valid Unicode (no lone surrogate); no Unicode normalization is applied |
| presence | every field of a schema is present; a field that does not apply is `null`, never absent; no member outside the schema |
| arrays | in the order the schema declares. Arrays that are sets are sorted: `ratificationRecords`, `sourceEvidence` and `evidence` by `path`, the `harnessDlls` of a build identity, `productAssemblies`, `characterizationOnlyAssemblies` and `eventNeutralityEvidence` (5.3), and `citingArtifacts` and `citedPairs` (5.4) by `path`, `catalogFolderFiles` (5.3) by `name`, `pinRecordHashes` ascending, module lists by normalized full path (ordinal = Unicode scalar value order = UTF-8 byte order) |
| encoding | UTF-8, no BOM |
| record file | the JCS bytes of the record followed by **exactly one LF**, nothing else |
| record hash | SHA-256 of the record file bytes, lowercase hexadecimal. This gives the manifest identity, the pin-record hash, the anchor-record hash, the build-equivalence record hash, the bound-SHA precondition record hash, `tupleRecordHash` and the `sha256` of a cited record |
| other files | a file that is not a record of this procedure (a review record, an evidence file) is hashed as the SHA-256 of its bytes after CRLF is normalized to LF |
| log line | the JCS bytes of the complete entry (`entryHash` included) followed by one LF; no blank line, no CR |
| `entryHash` | SHA-256 of the JCS bytes of the entry **without** its `entryHash` member (no LF) |
| association with the tuple | the tuple field *manifest hash and custody-log reference* (V2.1 2.1, BUILD_BOUND) has two parts: the **manifest identity**, and the **custody-log reference** = `{tag, anchorRecordHash}` of the **first** anchor whose `h` is at least the index of that manifest's `APPROVED` entry. Both parts are the same for every run under the manifest, as a BUILD_BOUND field must be (V2.1 2.2). A run's recorded values are compared with the manifest that its `IN_USE` entry names, which must be `APPROVED`, anchored, usable under the baseline version in force and approved for the run's own build (sections 3, 7 and 8). A difference makes the run **`INVALID`** (V2.1 29.1; phase-1 ruling G-2). The anchor that binds a run's inputs at its start is a different value, `runStartAnchor` of the `IN_USE` entry (section 7); it changes from run to run, so it is not the tuple field. There is no manifest at the baseline (section 8) |

### 4.1 Normative vector

The generator `docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.py` and its output `docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.json` are part of this artifact. They were introduced by V5 and are **unchanged in V6 and V7** (byte-identical; the output's own label names BA-07 V5, the version that introduced it). `python docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.py <output.json>` regenerates the output exactly: it writes LF-only bytes, equal to the LF-normalized bytes of the published output. It is deterministic and uses no network, git, host or AutoCAD. The hashes below were computed with Python `hashlib.sha256` over the actual bytes after CRLF is normalized to LF, and were re-computed in rounds V6 and V7 (each with a regeneration of the output) with the same values. The files as generated contain no CR; a checkout with `core.autocrlf=true` may show CRLF on disk, and the normalized bytes are unchanged. The V4 vector files are not changed. **V6 and V7 need no new vector**: they change no field of the custody-log entry (6.1), of the pin record (5.1), of the candidate, ratification, designation and carry-over records (5.2) or of the anchor record (9), and no event, subject kind, role, transition or case of sections 7 and 12. What V6 and V7 add or change lies outside the model, as 6.2 **Result** declares: the build-equivalence record (5.3, extended by AR5-04) and the bound-SHA precondition record (5.4, new in V6 under AR5-12; its fields `runnerOutput`, `citedPairs` and `deltaProcessing` restated in V7 under AR7-03, AR7-05 and BA07V6-02 and BA07V6-03) are custodied as review records with the existing `REVIEW_RECORD_ADDED` event; the use preconditions U5 and U6 are checked by the control plane at run start, not by 6.2 step 5; and the consumption of a pin by the derivation of another pin (5.1 rule 3, AR5-06 (b)) is checked by 6.2 step 5(c) outside the model.

| Item | SHA-256 |
|---|---|
| output file `I-52-ct21d-ba07-v5-jcs-vector.json` | `743302c72a901c2631c4bdbe46e2e6b755901c8427dc3d0af508474ed28dff38` |
| generator file (also recorded inside the output) | `9f9fd14f9c6efb2754f8c98c5aff470b34e525379be6b30dfc4224f253ac4209` |
| **example entry** (worked example, index 2): its `entryHash` | `20716bfb1e9247f5eba6d5a511da623ad37f7fa284bb84fd6ee40b5004c46274` |
| worked example, mandatory sequence: `entryHash` of the head entry (index 13) | `9a2b1edc124c04ae7b64899e0bb80bbcb1914e1a97993ba8e15af8ff2fdf941a` |
| worked example, mandatory sequence: log file (indexes 0-13) | `986ce65ed22e3e55e1dfa010819d785705f36dcefafec1dde980f8f4f77e9111` |
| worked example with its continuation: `entryHash` of the head entry (index 23) | `e3c23c32a1d3e2e0a6fd5766ba0b0b651aa84fee354a265a1e31803ac836f353` |
| worked example with its continuation: log file (indexes 0-23) | `1eafed02e134c5179cf45c6c4a0fa24afab2a6917e4fb310155f65b754ec3e3c` |

The example entry serialized without its `entryHash` is 834 UTF-8 bytes; their SHA-256 is the `entryHash` above:

```text
{"actor":"EXAMPLE-COORDINATOR","anchorRef":{"anchoredIndex":1,"peeledCommit":"9cb16b0a7c197ab076a6d163cac057f076c8512b","remote":"https://github.com/marioap-afk/Calculadora_de_racks.git","tag":"ct21d-baseline/v1","tagObjectId":"230e153c655c4c2fe65c039e4d335e428e186a32"},"baselineCustodyHash":null,"baselineVersion":null,"captureId":null,"cause":null,"event":"ANCHOR_RECORDED","index":2,"note":"first anchor of baseline \"v1\" (illustrative) — café","pinRecordHashes":null,"pinSlot":null,"prevEntryHash":"062cecb186519acdab68d15f8e0f318a16ddee2cb8ae2fa12253138c328b5c94","ratificationRecords":null,"role":"CUSTODY_ROLE","runId":null,"runStartAnchor":null,"subjectHash":"461d3a937898f8345475f97c955229f986dbf8ef093b4929cf8619ddb496fb34","subjectKind":"ANCHOR","supersededBy":null,"tupleRecordHash":null,"utc":"2000-01-01T00:06:00Z"}
```

The output also carries:

- the string example of RFC 8785 section 3.2.2.2, reproduced exactly (its `numbers` member is outside this profile);
- five double encodings, and the refusal of NaN, the two infinities, a non-integer JSON number and the integer 2^53;
- every log line and `entryHash` of the worked example, its anchor records and its three pin-record hashes;
- the result of the verification model on every case of section 12;
- a cross-check: Python `json.dumps` with sorted keys, compact separators and no ASCII escaping gives the same bytes for every object serialized (1675 of 1675). This holds for this profile only.

**Every value of the worked example is illustrative** (derived from `EXAMPLE:` labels; it is not the hash of any real artifact, run, tag or commit). The normative content is the profile and the bytes and hashes it produces. An implementation conforms when it reproduces the example entry bytes and hash, and every line and `entryHash` of the output's `logLines`.

## 5. Storage, retrieval and versioning

| Aspect | Rule |
|---|---|
| storage | content-addressed and read-only at the designated location, with the hash in the file name (the patterns of section 2). The anchor record is the exception: one fixed path, one version per anchor commit, each bound by its tag |
| retrieval | by hash; the hash is **verified on read**; a mismatch refuses use. An anchor record is read at its tag's peeled commit and verified against the `subjectHash` of its `ANCHOR_RECORDED` entry |
| amendment and versioning | a manifest, a pin record, a review record, a build-equivalence record, a bound-SHA precondition record or a baseline record is **never edited**. A change is a **new capture, a new pin version, a new record or a new baseline version**, with a new hash and a supersession link (pin records: field `supersedes`; custody log: `SUPERSEDED` with `supersededBy`). Superseded items are retained. A pin slot holds at most one live pin, and a new pin version follows the rules of section 5.1 |

### 5.1 Pin records and their own seal (AR2-16, AR3-23, AR4-13)

A pin is a named slot in the sealed catalog; the slot definitions are sealed with BA-11 (BA-11 section 3.1). The pin **value** lives in a pin record:

| Field | Content |
|---|---|
| `recordKind` | `"CT21D_PIN_RECORD"` |
| `pinKind` | `E04_ADMITTED_SET`, `LOCK_MODE`, `EVM_FROZEN`, `HOST_DEFAULT_MAP` or `FIXTURE_INSTANCE` (AR2-16); it equals the part of `pinSlot` before `@` |
| `pinSlot` | the sealed slot it fills, for example `E04_ADMITTED_SET@R-1` or `FIXTURE_INSTANCE@<FX>` |
| `baselineVersion` | the baseline version **in force** when its `PIN_RECORDED` entry is appended (`v<k>`, section 9); the pin is usable only under that version (rule 1) |
| `value` | the pinned value. Content bound to a machine profile is carried as its SHA-256 and stays in local evidence |
| `sourceEvidence` | the evidence the value was frozen from: `[{path, sha256}]`, sorted by `path`. For `EVM_FROZEN` it includes the sealed learning-corpus record of the frozen EVM (V2.1 27.3), read by the condition "`REVIEWED` before the first `LEARNING` run" below. For a pin whose value was derived under another pin (for example the `E04_ADMITTED_SET@R-1` pin, learned by runs under the chosen `LOCK_MODE`), it includes the pin record of that other pin, `{path, sha256}` with the path of section 2; that citation is what makes the other pin **consumed by derivation** (rule 3; AR5-06 (b)). For `FIXTURE_INSTANCE@FX-ANN-BLANK` it includes the fixture conformance record, which carries the identity of the `FIXTURE_CONSTRUCTION_BUILD` (section 5.5; AR5-11, AR6-02) |
| `inventoryReviewRecords` | **only for `EVM_FROZEN`**: the SHA-256 of the `REVIEWED` inventory record and of every reopened operation-class review record on which the frozen EVM rests (V2.4 `V24-N2`). It carries the EVM-level field `INVENTORY_REVIEW_RECORDS` of the frozen EVM and must equal it (AR3-22). Otherwise `null` |
| `supersedes` | the pin-record hash of the live pin of the same slot that this record replaces (rule 2), or `null` when the slot has no live pin |
| `frozenUtc` | UTC time at which the value was frozen |
| `recordedBy` | the actor who wrote the record (a person or `CONTROL_PLANE:<build id>`) |

Lifecycle (the pin machine of AR3-23, `RECORDED -> RATIFIED -> ANCHORED`):

| State | Reached when | Evidence |
|---|---|---|
| `RECORDED` | the pin record is written, after the value is frozen from its own evidence (never from the outcome of a scenario that uses it) | the pin record file, verified by its hash |
| `RATIFIED` | the pin's **ratification pair** exists under `ratifications/`: exactly one record with `ratifier = COORDINATOR` and one with `ratifier = ARCHITECT`, each with `decision = RATIFIED`, `subjectKind = PIN`, `subject` = the pin slot and `subjectHash` = the pin-record hash (section 5.2). The pair is the pin's **own seal** (AR2-16, AR3-23). The `PIN_RECORDED` custody entry (`APPROVAL_ROLE`) is then appended and **cites both** in `ratificationRecords` | the two ratification records and the `PIN_RECORDED` entry |
| `ANCHORED` | derived: an anchor covers the `PIN_RECORDED` entry (section 7) | the anchor |

`PIN_RECORDED` is appended only for a `RATIFIED` pin: it records the pin in custody together with its seal. An unratified pin record is not in custody and cannot be used. This reading of the pin state machine (`RECORDED` has no custody-log entry of its own; `PIN_RECORDED` is appended once both ratification records exist; `ANCHORED` is derived) is **ruled by AR4-13** (section 217). Nothing is written into BA-11: a pin write never changes the bytes of BA-11 or of the catalog and never creates a baseline version (section 8). For `EVM_FROZEN`, every hash in `inventoryReviewRecords` must be the `subjectHash` of a `REVIEW_RECORD_ADDED` entry with a lower index than the `PIN_RECORDED` entry (`PIN_REVIEW_RECORDS_MISSING`).

**`REVIEWED` before the first `LEARNING` run** (V2.4 section 1: the inventory becomes `REVIEWED` only after the dynamic trace, "and that is required before the first `LEARNING` run"; V5 correction pass). The `IN_USE` entry of a run is appended **after the run ends** (section 7), so its index does not show that a record was in custody before the run started; the **run-start anchor**, fixed at run start, does. For `EVM_FROZEN`, the sealed learning-corpus record that the pin record's `sourceEvidence` cites names the sessions that ran the learning micro-scenarios (V2.1 27.3), by run id, and the hash of the `REVIEWED` record they rest on. That hash is in `inventoryReviewRecords` (`PIN_REVIEW_RECORDS_MISSING`), and its `REVIEW_RECORD_ADDED` entry has an index **at most the `h` of the run-start anchor** (`runStartAnchor`, section 7, U1) of the `IN_USE` entry of **every** learning run named there (the learning runs are governing runs, AR4-01, so each records its use of the manifest). The `REVIEWED` record was therefore anchored in custody before each learning run started. A `REVIEW_RECORD_ADDED` index greater than that `h`, a learning run named there without an `IN_USE` entry, or a learning-corpus record that names no run id or no `REVIEWED` hash, is `REVIEW_AFTER_LEARNING_START`. The order of a reopened operation-class review relative to the learning runs is not checked: V2.4 section 1 states the order for `REVIEWED` only.

**Rules of a pin slot** (AR4-13; checked by 6.2 step 5):

1. **Baseline version and slot.** The record's `baselineVersion` equals the baseline version in force at its `PIN_RECORDED` entry, that is the `baselineVersion` of the last `BASELINE_APPROVED` entry before it (`PIN_VERSION_MISMATCH`). Its `pinSlot` is a slot of the pin-slot table (BA-11 section 3.1) of the BA-11 sealed as that baseline version (the `subjectHash` of its `BASELINE_APPROVED` entry), with `<FX>` a fixture that table admits, and its `pinKind` equals the slot's kind (`PIN_SLOT_UNKNOWN`). The pin is usable only under that baseline version (section 7, U3).
2. **At most one live pin per slot.** A pin is **live** from its `PIN_RECORDED` entry until its `SUPERSEDED` or `REVOKED` entry. A slot has at most one live pin at every index (`PIN_SLOT_CONFLICT`). A `PIN_RECORDED` entry for a slot that has a live pin `L` is legal only when its record's `supersedes` = `L` and the **next** entry is `SUPERSEDED` of `L` with `supersededBy` = the new pin-record hash; the two entries are one step (the supersession pair), and no other entry lies between them. A `PIN_RECORDED` entry for a slot with no live pin has `supersedes = null`. A pin's `SUPERSEDED` entry is legal only as the second entry of a supersession pair whose first entry's record names it in `supersedes` (`PIN_SUPERSESSION`).
3. **After consumption** (AR4-13; reading ruled by AR5-06 (b)). A pin is **consumed** once an `IN_USE` entry lists it in `pinRecordHashes`, **or** once the pin record of another pin that has a `PIN_RECORDED` entry cites it in `sourceEvidence` (a pin used to derive another pin; AR5-06 (b) completes "consumed" in this way). Once a pin of a slot has been consumed, no new pin of that slot is recorded **under the same baseline version**, whether by supersession or after a revocation (`CONSUMED_PIN_SUPERSEDED`), except for `EVM_FROZEN`: a new model version, reviewed and revalidated, is a new pin version (V2.1 27.5), and the runs that consumed the predecessor are attributed to it by their `pinRecordHashes` and are never evidence for the successor (V2.1 27.5; the event-model hash is a BUILD_BOUND tuple field, V2.1 2.1). Basis for the others: `E04_ADMITTED_SET` is not revised by retrospection (AR2-11: a membership miss makes group E4 `UNKNOWN`); `LOCK_MODE`, `HOST_DEFAULT_MAP` and `FIXTURE_INSTANCE` are fixed before the first governing run that uses them (AR2-11 order, AR2-17, AR3-26); a changed input is a new version, not a retry, and no attempt is dropped (V2.1 21.3). AR5-06 (b) accepts this rule as read: no replacement of a consumed `LOCK_MODE`, `HOST_DEFAULT_MAP` or `FIXTURE_INSTANCE` pin within a baseline version, never for `E04_ADMITTED_SET`, and `EVM_FROZEN` only as a new model version. A new value for such a slot needs a new baseline version, where rule 4 applies. `REVOKED` with a recorded cause is always admitted. **Disclosure (AR5-06 (b)).** A consumed pin found defective can be `REVOKED`, but its slot then stays **empty** until a new baseline version: no pin of that slot can be recorded again under the same version, so no governing run that needs the slot can consume one (U3). A pin derived from a revoked pin is not revoked by this procedure: its fate is for the ruling that records the revocation (declared, section 10). A pin that nothing has consumed, by use or by derivation, may be superseded within its version.
4. **Carry-over to a new baseline version (fail-closed).** A pin is usable only under the baseline version named in its record. Carrying it to `v<k+1>` is a **new pin version for `v<k+1>`** (the same value is admitted; `supersedes` = the `v<k>` pin) with its own ratification pair and its `PIN_RECORDED` entry, immediately followed by `SUPERSEDED` of the `v<k>` pin (rule 2), inside the **carry-over block** of `v<k+1>` (section 8): before the first anchor of `v<k+1>` and therefore before any governing use under it. Every live pin of `v<k>` that is not carried over is `REVOKED` (cause `SUPERSEDED_BY_RULING`) in the same block (`CARRY_OVER_MISSING`). A `PIN_RECORDED` entry that supersedes a pin of an earlier baseline version outside that block is `CARRY_OVER_INVALID`. Without its new version, a `v<k>` pin is not usable under `v<k+1>` (section 7, U3).

### 5.2 Candidate, ratification, designation and carry-over records (complete schema)

| Field | Content |
|---|---|
| `recordKind` | `"CT21D_RATIFICATION"` |
| `ratifier` | `COORDINATOR` or `ARCHITECT` |
| `actor` | the person |
| `subjectKind` | `ARTIFACT`, `PIN` or `MANIFEST` |
| `subject` | the artifact id (for example `CT21D-BASE-MANIFEST-CUSTODY`), the pin slot, or the manifest's `captureId` |
| `subjectHash` | the candidate hash (artifact; in a designation record, the BA-07 seal hash, which equals the BA-07 candidate hash), the pin-record hash (pin), or the manifest identity (manifest) |
| `decision` | `CANDIDATE_FOR_SEALING`, `RATIFIED`, `DESIGNATION` or `MANIFEST_CARRY_OVER` (table below) |
| `utc` | UTC time |
| `reference` | the decisions-log reference of the ruling |
| `independentCopy` | `{holder, location}` in the primary designation record (the BA-07 candidate record or the Coordinator's ratification record of BA-07, section 2) and in a designation record; otherwise `null` |
| `evidence` | the evidence files the ruling cites, `[{path, sha256}]` sorted by `path` (for example the output of a seal-time re-run that the sealing workflow requires for an entry); an empty array when none |

| Record | `ratifier` | `subjectKind` / `subject` | `subjectHash` | `decision` | `<kind>` of its path |
|---|---|---|---|---|---|
| candidate record of an artifact (the Architect's candidate ruling) | `ARCHITECT` | `ARTIFACT` / the artifact id | its candidate hash | `CANDIDATE_FOR_SEALING` | `candidate` |
| ratification records of an artifact (two) | `COORDINATOR`; `ARCHITECT` | `ARTIFACT` / the artifact id | the same candidate hash | `RATIFIED` | `coordinator`; `architect` |
| ratification pair of a pin (5.1) | `COORDINATOR`; `ARCHITECT` | `PIN` / the pin slot | the pin-record hash | `RATIFIED` | `coordinator`; `architect` |
| designation record (a later change of the independent copy) | `COORDINATOR` | `ARTIFACT` / `CT21D-BASE-MANIFEST-CUSTODY` | the BA-07 seal hash | `DESIGNATION` | `designation-<n>` |
| manifest carry-over record (section 8) | `COORDINATOR` | `MANIFEST` / the `captureId` | the manifest identity | `MANIFEST_CARRY_OVER` | `carryover-v<k>` |

**BA-11's seal** (R1:BA07-V4-05, R2:BA07-V4-04). There is **no separate seal record** and no decision `SEALED`. The records that establish BA-11's `SEAL_HASH` are **BA-11's candidate record and its two ratification records** (subject `CT21D-BASE-HASH-REGISTRY`): `SEAL_HASH` = `CANDIDATE_HASH` (BA-11 section 2, `RATIFIED -> SEALED`), the `subjectHash` of all three. The other `RATIFIED -> SEALED` conditions are properties of BA-11's committed bytes: their recomputed hash equals that `subjectHash`, and those bytes carry the seal hash and ratification records of every other artifact entry. The `SEAL_HASH` itself is the `subjectHash` of `BASELINE_RECORDED` and `BASELINE_APPROVED`, and `BASELINE_APPROVED` cites the three records (section 8; checked by 6.2 step 5, `BASELINE_RECORDS_INVALID`).

**Completeness and direction of reference** (R2:BA07-V4-04). The two tables above are the complete schema of these records (section 4: no member outside the schema). The sealing workflow of BA-11 (BA-11 section 2) says when each record is written and what a ruling must cite; what it cites goes into `reference` and `evidence`, never into extra members. This section does not delegate record content to BA-11, so the reference runs one way, from BA-11 section 2 to this section.

### 5.3 Build-equivalence record (AR4-06 (4); AR5-04)

AR4-06 (4) requires that the equivalence "the characterization build differs from the product build only by the harness switches" be **verified and recorded** (the product assemblies byte-identical with the switches in separate harness assemblies, or a reviewed diff), and applies the pins derived on the characterization build (`E04_ADMITTED_SET`, `LOCK_MODE`, `HOST_DEFAULT_MAP`) to `PRODUCT_BUILD` runs only under that recorded equivalence, with the membership post-check of `E4-VAL-R1` as the safeguard. AR5-04 rules, as a reading and without errata, that the same recorded equivalence is also the basis of the **reverse direction**: the `EVM_FROZEN` pin, learned and validated on the `PRODUCT_BUILD` (AR4-01), applies to governing `CHARACTERIZATION_BUILD` runs only under it. This section fixes the record, its location (section 2) and its custody. The record is not a pin, not a ratification record and not a tuple field (AR4-06 names a new tuple field only as a possible V2.6 micro-errata and does not order it; AR5-04 orders none). One record serves both directions for its pair of builds.

| Field | Content |
|---|---|
| `recordKind` | `"CT21D_BUILD_EQUIVALENCE"` |
| `productBuild` | the identity of the `PRODUCT_BUILD`, `{catalogFolderFiles, harnessDlls, pluginSha256}`: `pluginSha256` = the SHA-256 of its Plugin DLL; `harnessDlls` = `[{path, sha256}]` of every harness DLL it declares (V2.1 2.1), sorted by `path`; `catalogFolderFiles` = `[{name, sha256}]` of the files of its `CATALOG_FOLDER` (V2.5 section 1: every `*.csv` and `*.json` file directly in the resolved catalog directory), `name` = the file name without the directory, sorted by `name`. These are values of that build's own BUILD_BOUND profile (AR4-06 (1)) |
| `characterizationBuild` | the same for the `CHARACTERIZATION_BUILD`: the values of `CHARACTERIZATION_TUPLE` (AR4-06 (2)) |
| `method` | `BYTE_IDENTICAL_PRODUCT_ASSEMBLIES` (the product assemblies of the two builds are byte-identical and the switches and injection points live in separate harness assemblies) or `REVIEWED_DIFF` (a reviewed diff shows that the two builds differ only by the switches and injection points) (AR4-06 (4)) |
| `productAssemblies` | for `BYTE_IDENTICAL_PRODUCT_ASSEMBLIES`: `[{path, sha256}]` of every product assembly, whose file hash is the same in both builds, sorted by `path`; otherwise `null` |
| `characterizationOnlyAssemblies` | for both methods: `[{path, role, sha256}]` of every assembly present in the characterization build and absent from the product build, sorted by `path`; `role` = `SWITCH`, `INJECTION_POINT`, `I14_WRITER`, `WITNESS_SUBSCRIPTION`, `DRY_RUN_BRANCH_SWITCH` or `DECLARED_CANARY` (the assemblies that AR5-04 (3) names), or `OTHER_HARNESS` for any other; an empty array when there is none |
| `catalogFolderSetsEqual` | `true` when the `catalogFolderFiles` of `productBuild` and of `characterizationBuild` are equal as sets of `{name, sha256}` pairs; otherwise `false` (AR5-04 (6): the record compares the `CATALOG_FOLDER` hash sets of both profiles) |
| `eventNeutralityEvidence` | `[{path, sha256}]` of the evidence that shows that everything present only in the characterization build (every entry of `characterizationOnlyAssemblies` and, for `REVIEWED_DIFF`, the switch and injection code that the diff shows) is **event-neutral** inside the governed window, that is adds no event that the governed window records, except where a scenario names it (AR5-04 (3)); sorted by `path`; never empty when `characterizationOnlyAssemblies` is not empty or the method is `REVIEWED_DIFF` |
| `reviewedDiff` | for `REVIEWED_DIFF`: `{path, sha256}` of the diff file (section 2); otherwise `null` |
| `reviewer` | the person who verified the equivalence and the event neutrality and, for `REVIEWED_DIFF`, reviewed the diff (self-review is a declared limit, 7.1) |
| `utc` | UTC time of the verification |

In `productBuild`, `characterizationBuild`, `productAssemblies` and `characterizationOnlyAssemblies`, `path` is the path of the file relative to the output directory of its build, with the separator `/`. The record is a canonical record file (section 4); a field that does not apply is `null`. V6 replaces the V5 field `switchAssemblies` (for `BYTE_IDENTICAL_PRODUCT_ASSEMBLIES` only) with `characterizationOnlyAssemblies` (for both methods, with a `role`) and adds `catalogFolderFiles`, `catalogFolderSetsEqual` and `eventNeutralityEvidence` (AR5-04 (3) and (6)); no record exists, so nothing is migrated.

**What a record establishes.** A record establishes the equivalence, in both directions, only when `catalogFolderSetsEqual = true` and `eventNeutralityEvidence` covers everything present only in the characterization build. A record whose sets differ, or whose evidence does not cover an entry, is kept as it was written and establishes no equivalence: U5 fails for every run that would rely on it. The control plane re-computes `catalogFolderSetsEqual` from the two lists; the event neutrality is the reviewer's statement with its evidence, which neither the control plane nor the verifier of 6.2 re-derives (declared, section 10).

**Custody.** The `CUSTODY_ROLE` custodies the record with a **`REVIEW_RECORD_ADDED`** entry: `subjectKind = REVIEW_RECORD`, `subjectHash` = the record hash. The record is custodied **as a review record**: the events, subject kinds and entry fields of 6.1, and the model of 4.1, are unchanged. A new verification, for example after a change of either build, is a new record (section 5), never an edit. A review record has no later event (section 7), so an equivalence found wrong after it was recorded is a finding for the Coordinator, not a custody transition of this procedure (declared, section 10).

**Use** (section 7, U5). (a) **Characterization to product** (AR4-06 (4)): a `PRODUCT_BUILD` run that consumes a pin derived on the characterization build relies on the record whose `productBuild` is its own build and whose `characterizationBuild` is the build of `CHARACTERIZATION_TUPLE`. (b) **Product to characterization** (AR5-04): a governing `CHARACTERIZATION_BUILD` run that consumes the `EVM_FROZEN` pin relies on the record whose `characterizationBuild` is its own build and whose `productBuild` is the `PRODUCT_BUILD` on which the frozen EVM was learned and validated. In both directions the record's `REVIEW_RECORD_ADDED` entry is covered by the run's run-start anchor, and the record hash, with its path, is cited in `RUN_START` (the run's first log record), sampled together with the BUILD_BOUND fields and the run-start anchor before library acquisition; the run's evidence package carries a copy of the record, or its `{path, sha256}`, next to the canonical tuple record (BA07V6-04). For (b), AR5-04 (1) also places `BUILD_EQUIVALENCE_RECORD` in the `RUN_PREREQUISITES` of those runs (the scenario catalog, BA-04 V6; informative pointer), and AR5-04 (5) keeps `EA2-DEFERRED-EVENTS` as the catalog's safeguard. The same record is the recorded equivalence under which AR4-06 (6) admits the contribution of `EA2-DEFERRED-EVENTS`, a characterization-build run, to `PARAM-03` for the product build; that use is not a custody event.

### 5.4 Bound-SHA precondition record (AR5-12)

AR5-12 (a reading, without errata; finding IA-G-03) separates two identities: the **bound product SHA**, from which the static facts of the baseline are taken and which is **never** a tuple value, and the **source SHA of the build under test**, the BUILD_BOUND tuple field "repo source SHA of the build" (V2.1 2.1). It sets a precondition of execution preparation for **each build under test**: every (path, blob) pair of the reused product route that BA-03, BA-06 and BA-08 cite at the bound SHA equals the pair at the source SHA of the build, or the delta is recorded and processed (the class is reopened; a change of the design inventory is a new BA-06 and a new baseline version). The result is saved with the tuple in the custody of BA-07. The fail-closed detection of a delta is AR5-12's. AR7-03 (decisions section 226, the ruling of AQ-V6-01; a reading, without errata) reads "the BA-03 runner detects it" as: the BA-03 runner detects its own pairs; the detector of the **union** of the pairs that BA-03, BA-06 and BA-08 cite is a **dedicated precondition verifier** (`docs/automation/evidence/I-52-ct21d-precondition-verifier.py`), an execution-preparation tool **outside the BA-11 registry** (no row of BA-11 and no edge), in the custody of this procedure, which reads the pair lists of the sealed BA-03, BA-06 and BA-08 and whose output this record cites (`runnerOutput`). The verifier's output enters custody through this record: it is stored as evidence in the folder of section 2, and the record that cites it is custodied below. This section gives the result its record, its location (section 2), its custody and its use (section 7, U6: every run on a build under test, AR7-04). The record is not a pin, not a ratification record and not a tuple field.

| Field | Content |
|---|---|
| `recordKind` | `"CT21D_BOUND_SHA_PRECONDITION"` |
| `boundProductSha` | the full 40-digit lowercase hexadecimal bound product SHA of the baseline version in force when the record is custodied, as the BA-11 registry sealed as that version records it (AR6-01). It is never a tuple value (AR5-12) |
| `buildUnderTest` | `{buildRole, harnessDlls, pluginSha256, sourceSha}`: `buildRole` = `PRODUCT_BUILD` or `CHARACTERIZATION_BUILD` (never the `FIXTURE_CONSTRUCTION_BUILD`, section 5.5); `sourceSha` = the value of the tuple field "repo source SHA of the build" (V2.1 2.1); `pluginSha256` and `harnessDlls` as in 5.3. They are values of that build's own BUILD_BOUND profile (AR4-06 (1), (2)) |
| `citingArtifacts` | `[{path, sha256}]` of the files of BA-03, BA-06, BA-08 and BA-09 from which the pairs are taken (for BA-09, the rows with Role `W-1` of its section 3.2, the W-1 dependency pairs; the verifier reads them as an extension of the AR7-03 union submitted to the Architect for confirmation), as the BA-11 registry lists them for the baseline version in force, sorted by `path` |
| `citedPairs` | `[{boundBlob, path, sourceBlob}]`, one element per repository path of the **union of the precondition sets that each citing artifact declares** (BA07V6-02): the `BOUND` sources of BA-03 section 2; the route rows of BA-06 section 2.1, its precedent rows excluded; the **W** and **C** rows of BA-08 section 3.2 (the reused product route and the fixture construction), its **P** rows excluded; and the **W-1** rows of BA-09 section 3.2 (the W-1 dependency pairs of the warm-up plans, AR7-07; directories bound by their tree ids) (informative pointers). Sorted by `path`: `path` relative to the repository root with the separator `/`; `boundBlob` = the git object id of the path at `boundProductSha` (a blob id, or a tree id for a BA-09 W-1 row); `sourceBlob` = the git object id of the path at `sourceSha`, or `null` when the path does not exist there. The record adds no path of its own |
| `runnerOutput` | `{path, sha256}` of the output of the detector of the precondition for this build, **agnostic of the detector** (AR7-05): under AR7-03, the output of the dedicated precondition verifier run over `citedPairs` for this build (`sourceSha` against `boundProductSha`); the verifier fails closed on a delta (AR5-12). The field names no specific runner; the BA-03 runner detects only BA-03's own pairs (AR7-03) |
| `result` | `EQUAL` (every `sourceBlob` equals its `boundBlob`) or `DELTA_PROCESSED` (at least one differs or is `null`, and the delta was recorded and processed under a ruling) |
| `deltaProcessing` | for `DELTA_PROCESSED`: `{citedArtifactsUnchanged, evidence, reference}`: `reference` = the decisions-log reference of the ruling that processed the delta; `evidence` = `[{path, sha256}]` of the records it cites (the re-review records of the reopened classes), sorted by `path`; `citedArtifactsUnchanged` = `true` **only** when that ruling records that no normative content of BA-03, BA-06 or BA-08 changes, otherwise `false` (BA07V6-03; it replaces the V6 member `inventoryUnchanged`). For `EQUAL`: `null` |
| `reviewer` | the person who checked the comparison and, for `DELTA_PROCESSED`, the processing |
| `utc` | UTC time of the check |

A delta that has not been processed is not written as a record: the precondition is then not met for that build and no run of it can satisfy U6. A record with `DELTA_PROCESSED` and `citedArtifactsUnchanged = false` is kept as it was written and does not satisfy U6: a change of the normative content of a sealed artifact is a new version of it and a new baseline version (BA-11 section 2), under which a new record is custodied. The named example is AR5-12's: a change of the design inventory is a new BA-06 and a new baseline version.

**What the control plane re-derives** (BA07V6-02). Before it accepts U6, the control plane re-derives the path set of `citedPairs` from `citingArtifacts` (the union of the precondition sets that the listed files declare, read at their `sha256`), and re-derives `result` from `citedPairs` (object-id equality, blob or tree; a `null` `sourceBlob` is a delta). On any mismatch it refuses the run. The statement `citedArtifactsUnchanged` of the processing ruling is not re-derived (declared, section 10).

**Custody.** As in 5.3: a **`REVIEW_RECORD_ADDED`** entry by the `CUSTODY_ROLE`, `subjectKind = REVIEW_RECORD`, `subjectHash` = the record hash; the events, subject kinds and entry fields of 6.1 and the model of 4.1 are unchanged. One record serves one pair (build under test, bound product SHA); a change of either, or a new check, is a new record (section 5), never an edit. A record found wrong after it was recorded is a finding for the Coordinator, not a custody transition (declared, section 10).

**Saved with the tuple** (AR5-12; AR7-05). The record is bound to the tuple **by value**: its `buildUnderTest` values equal the BUILD_BOUND values of every run that relies on it, which that run's canonical tuple record carries. It is bound **by reference**: the record hash, with its path, is cited in `RUN_START` (the run's first log record), sampled together with the BUILD_BOUND fields and the run-start anchor before library acquisition, the same formula as U5 and as the `RUN_PREREQUISITE_RULE` of the scenario catalog (BA-04; informative pointer); and the run's evidence package carries a copy of the record, or its `{path, sha256}`, next to the canonical tuple record (BA07V6-04). No tuple field is added (AR5-12: the bound SHA is never a tuple value) and no field of the `IN_USE` entry is added. AR7-05 (decisions section 226, the ruling of AQ-V6-BA07-1) rules this reading sufficient for AR5-12, with no new field in `IN_USE` or in the tuple, under three text conditions: the record hash is cited in `RUN_START`; the evidence package holds the record or its `{path, sha256}`; `runnerOutput` is agnostic of the detector (the output of the verifier of AR7-03). This section states all three.

### 5.5 The fixture construction build (AR6-02)

AR6-02 (decisions section 223.2, conditions FXB-01 to FXB-07) keeps `FX-ANN-BLANK` as a legacy-state fixture built by a declared construction build, and makes that build a **third build role**, `FIXTURE_CONSTRUCTION_BUILD`, distinct from the `CHARACTERIZATION_BUILD` and from every build under test (FXB-03). In this procedure:

- **Where its identity lives.** In the fixture conformance record of `FIXTURE_INSTANCE@FX-ANN-BLANK` (AR5-11: its source SHA and the SHA-256 of its Plugin and UI; the content of that record is the kind baseline's, BA-08 V6, informative pointer). The pin record cites that conformance record in `sourceEvidence` (5.1), so the identity enters custody through the hash of the conformance record. The pin-record schema gains no field. The Coordinator and the Architect give the pin's ratification pair (5.1) only when the cited conformance record carries that identity; 6.2 does not read the conformance record's content (declared, section 10).
- **What it is not** (FXB-03, FXB-04). It is never a `BUILD_UNDER_TEST` value and never part of `CHARACTERIZATION_TUPLE` or of `RUN_START`. It has no BUILD_BOUND profile of AR4-06, no manifest (section 3), no build-equivalence record (5.3), no qualification of V2.1 4.3 (it runs no instrument and no scenario) and no bound-SHA precondition record: it is outside the precondition of AR5-12 (5.4). No other build, the validated build of another initiative included, substitutes it (FXB-04). No run on it can satisfy U6.
- **Not a bound SHA.** Its source is the single exception of AR6-01 and is not a bound product SHA.

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
| `subjectHash` | `BASELINE`: the BA-11 seal hash; `ANCHOR`: the anchor-record hash; `PIN`: the pin-record hash; `REVIEW_RECORD`: the file hash of the record (for a build-equivalence record or a bound-SHA precondition record, its record hash, 5.3, 5.4); `MANIFEST`: the manifest identity. `null` only in `AUTHORIZED` and in a `REVOKED` entry of a manifest whose last event is `AUTHORIZED` (no identity exists yet; the subject key is the `captureId`) |
| `captureId` | only in `MANIFEST` entries: the capture identifier `CAP-<n>` given by `AUTHORIZED`; it is the key of the manifest subject |
| `baselineVersion` | only in `BASELINE_RECORDED` and `BASELINE_APPROVED`: `v<k>` |
| `baselineCustodyHash` | only in `BASELINE_RECORDED`: the BA-07 seal hash (this procedure) |
| `ratificationRecords` | only in `BASELINE_APPROVED` (BA-11's candidate record and its Coordinator and Architect ratification records, section 5.2, plus, from `v2` on, the `MANIFEST_CARRY_OVER` records of section 8) and in `PIN_RECORDED` (the pin's ratification pair): `[{path, sha256}]`, sorted by `path` |
| `pinSlot` | only in `PIN_RECORDED`: the slot the pin record fills |
| `runId` | only in `IN_USE`: the run id of the run that uses the manifest |
| `tupleRecordHash` | only in `IN_USE`: the record hash of the canonical tuple record of that run; `MACHINE_PROFILE_BOUND` values stay in local evidence |
| `pinRecordHashes` | only in `IN_USE`: the pin-record hashes the run consumes, ascending (an empty array when none) |
| `runStartAnchor` | only in `IN_USE`: `{anchorRecordHash, tag}` of the run's **run-start anchor** (section 7): the latest anchor that the control plane found at the remote and matched to its `ANCHOR_RECORDED` entry (6.2 steps 3 and 4) when it sampled the run's BUILD_BOUND fields, before library acquisition |
| `cause` | only in `REVOKED`: `TOOLING`, `ENVIRONMENT`, `CONTAMINATION`, `DEFECT`, `SUPERSEDED_BY_RULING` or `OTHER`, with the explanation in `note` |
| `supersededBy` | only in `SUPERSEDED`: the `subjectHash` of the successor (a new BA-11 seal hash, pin-record hash or manifest identity). A baseline or manifest successor must already be approved at a lower index; a pin successor's `PIN_RECORDED` entry is the entry immediately before (5.1 rule 2) |
| `anchorRef` | only in `ANCHOR_RECORDED`: `{anchoredIndex, peeledCommit, remote, tag, tagObjectId}`, that is h, the anchor commit, the remote URL, the tag name and the tag object id |
| `prevEntryHash` | the `entryHash` of the previous entry; the genesis entry uses 64 zero digits |
| `note` | free text, may be empty |
| `entryHash` | SHA-256 of the JCS bytes of all the other fields (section 4) |

### 6.2 Verification algorithm

Inputs: the custody log at the verified revision; the designated remote `REMOTE_URL` (section 9); the pin, ratification and review records at the verified revision, and the learning-corpus record that an `EVM_FROZEN` pin record cites in `sourceEvidence` (5.1); the BA-11 bytes sealed as each baseline version (the `subjectHash` of its `BASELINE_APPROVED` entry), read at the path of the BA-11 registry and verified by that hash, for their pin-slot table (BA-11 section 3.1); and the holder of the independent copy (step 6). **The anchors are taken from the remote.** They are never taken from inside the log, from local tags, from branch ancestry or from the anchor record at the verified revision (AR3-23).

**Modes** (R2:BA07-V4-05). **`CURRENT`** (the default): the verified revision is the latest revision that carries the custody log; the file content is what is verified, never commit ancestry. **`HISTORICAL`**: selected explicitly for a named earlier revision; `n` = the last index of the log at that revision. The anchors whose `h` is greater than `n` are listed as **`LATER_ANCHORS`** and are left out of step 3(e), step 4 and step 5; steps 3(a)-(d) and step 6 still cover every tag. A historical verification never stands for the current state (see **Result**).

1. **Entries.** Every line ends with LF; there is no CR and no blank line. Each line must equal the JCS bytes of the parsed object and carry exactly the fields of 6.1 (`NON_CANONICAL_ENTRY`). Its `index` must equal its line number, counted from 0 (`INDEX_SEQUENCE`). Recompute every `entryHash`; a difference is a **broken entry** (`BROKEN_ENTRY`).
2. **Chain.** Check each `prevEntryHash` against the preceding entry; index 0 uses 64 zero digits. A difference is a **broken chain** (`BROKEN_CHAIN`).
3. **Anchors discovered at the remote; comparison at every anchored index.**
   - (a) List the anchors with `git ls-remote --tags <REMOTE_URL> 'refs/tags/ct21d-baseline/*'`. Every listed ref must match the tag-name scheme of section 9 (`UNEXPECTED_TAG_NAME`). Every tag must be annotated, that is listed with its peeled line `^{}` (`LIGHTWEIGHT_TAG`). The unpeeled id is the tag object id; the `^{}` id is the peeled commit.
   - (b) Fetch those objects into a private namespace, so that local tags cannot shadow them: `git fetch --no-tags <REMOTE_URL> '+refs/tags/ct21d-baseline/*:refs/ct21d-verify/*'`. Each fetched ref must resolve to the listed tag object id and peel to the listed commit.
   - (c) At each tag's peeled commit, read the anchor record: `git show <peeled commit>:docs/automation/evidence/ct21d/baseline-anchor.json`. It must exist and be a canonical record file (`ANCHOR_RECORD_MISSING`, `ANCHOR_RECORD_NON_CANONICAL`). Its `ANCHOR_TAG_NAME`, `ANCHOR_SEQUENCE` and `BASELINE_VERSION` must match the tag name (`ANCHOR_RECORD_NAME_MISMATCH`). Its record hash is the anchor-record hash. It gives `h` = `CUSTODY_LOG_HEAD_INDEX` and `H` = `MANIFEST_CUSTODY_HEADER_HASH`. The custody log at the same commit must end at index `h`, with `entryHash` = `H` (`ANCHOR_COMMIT_LOG_MISMATCH`).
   - (d) Order the anchors by `h`; `h` must strictly increase (`ANCHOR_ORDER`). In that order the names must follow the scheme `v1`, `v1-a1`, `v1-a2`, ..., then `v2`, `v2-a1`, ..., with no gap and no repetition (`TAG_SEQUENCE`).
   - (e) For **every** anchor (in `HISTORICAL` mode, every anchor that is not a `LATER_ANCHOR`), not only the last: `h` must be an index of the verified log (`TRUNCATED_BELOW_ANCHOR`), and the `entryHash` of the entry **at index `h`** must equal `H` (`REWRITTEN_AT_OR_BEFORE_ANCHOR`). The comparison is **never** made against the current head. Every `entryHash` covers `prevEntryHash`, so equality at `h` binds the whole prefix `0..h`.
   - If no tag exists, the status is `UNANCHORED`: no entry is tamper-evident.
4. **One-to-one match of tags and `ANCHOR_RECORDED` entries; pending window** (in `HISTORICAL` mode, over the anchors that are not `LATER_ANCHORS`).
   - Every tag of step 3 has exactly one `ANCHOR_RECORDED` entry whose `anchorRef.tag` names it (`UNRECORDED_ANCHOR`, `DUPLICATE_ANCHOR_RECORDED`). Every `ANCHOR_RECORDED` entry names a tag of step 3 (`ANCHOR_WITHOUT_TAG`).
   - For each pair, `anchorRef` (tag object id, peeled commit, anchored index, remote) must equal what step 3 found, and `subjectHash` must equal the anchor-record hash (`ANCHOR_REF_MISMATCH`). The entry's index must be greater than that anchor's `h` (`ANCHOR_RECORDED_BEFORE_HEAD`). For every anchor except the last, it must be at most the `h` of the next anchor, so a later anchor covers it (`ANCHOR_RECORDED_NOT_COVERED`).
   - The **last** tag only may have no entry yet: status `LAST_ANCHOR_NOT_RECORDED`, meaning section 9 step 6 is not complete at the verified revision. It is part of the pending window.
   - Entries with an index greater than the `h` of the last anchor are **`PENDING_REANCHOR`**. They are listed in the result and are **not tamper-evident** (section 1): until the next anchor covers them, no step detects an edit, removal or replacement of them that keeps them well formed and legal. This is declared and is not a failure (AR3-23).
5. **State machines, fields, baselines, pins and use preconditions.** For every subject:
   - (a) the event and subject kind, the legal transitions and the role of each event (section 7), and the conditional fields of 6.1 (`ILLEGAL_EVENT_OR_KIND`, `ILLEGAL_TRANSITION`, `ROLE_MISMATCH`, `FIELD_RULE`, `MANIFEST_IDENTITY_CHANGED`);
   - (b) baselines (section 8): the baseline sequence and the carry-over block (`BASELINE_SEQUENCE`); BA-11's candidate record and two ratification records cited by each `BASELINE_APPROVED` entry (5.2; `BASELINE_RECORDS_INVALID`); the completeness and validity of the carry-over (`CARRY_OVER_MISSING`, `CARRY_OVER_INVALID`); and each anchor record's `BA11_SEAL_HASH`, `BA07_SEAL_HASH` and `BASELINE_VERSION` against those of the baseline in force at its `h` (`ANCHOR_RECORD_BASELINE_MISMATCH`);
   - (c) pins (5.1): the pin record and its ratification pair (`PIN_SEAL_INVALID`), the slot and the baseline version (`PIN_SLOT_UNKNOWN`, `PIN_VERSION_MISMATCH`), one live pin per slot and the supersession pair (`PIN_SLOT_CONFLICT`, `PIN_SUPERSESSION`), supersession after consumption, by use or by derivation (`CONSUMED_PIN_SUPERSEDED`; 5.1 rule 3, AR5-06 (b)), and the `EVM_FROZEN` review records (`PIN_REVIEW_RECORDS_MISSING`), including the order "`REVIEWED` before the first `LEARNING` run" against the run-start anchor of every learning run (`REVIEW_AFTER_LEARNING_START`; V5 correction pass);
   - (d) the use preconditions U1 to U4 of section 7 (`USE_REFERENCE_INVALID`, `USE_BEFORE_ANCHOR`, `MANIFEST_VERSION_MISMATCH`, `PIN_USE_BEFORE_ANCHOR`, `PIN_USE_AFTER_CLOSURE`, `PIN_VERSION_MISMATCH`, `USE_WITH_PENDING_CLOSURE`). U5 and U6 are not checked here: the `IN_USE` entry carries neither the build under test nor the hash of the record that U5 or U6 relies on (section 7).

   Pin, ratification, designation, carry-over and review records (the build-equivalence records of 5.3 and the bound-SHA precondition records of 5.4 included), and the learning-corpus record an `EVM_FROZEN` pin record cites, are read at the verified revision and verified by their hashes. The step also reports the derived state of every subject (section 7).
6. **Independent copy** (the AR3-23 reading is kept: the designation is read from the anchor records; R2:BA07-V4-01 adds (a), (b) and (d)).
   - (a) Every anchor record of step 3 must carry `INDEPENDENT_COPY = {designationRecord: {path, sha256}, holder, location}`. The value `UNDESIGNATED` is a violation (`DESIGNATION_MISSING`): the designation is a before-seal prerequisite of BA-07 and every anchor follows every seal (section 9). The cited record must exist, match its SHA-256, name the same holder and location, and be the primary designation record or a designation record (5.2) of the BA-07 seal hash in force at that anchor's `h` (`DESIGNATION_RECORD_MISMATCH`).
   - (b) **Primary designation record (cross-check).** The primary designation record of the BA-07 seal hash in force at the verified revision (the `baselineCustodyHash` of the last `BASELINE_RECORDED` entry) must exist as `ratifications/CT21D-BASE-MANIFEST-CUSTODY-<seal hash>-candidate.json` or `...-coordinator.json`, carry `independentCopy`, and match what the BA-07 entry of the BA-11 registry sealed as that baseline cites for it (`DESIGNATION_MISSING`). It is also the source of the designation when step 3 finds no tag.
   - (c) **Comparison with the holder.** The holder to consult is the one named by the last anchor record; when step 3 finds no tag, the one named by the latest designation record of that BA-07 seal hash, or else by the primary designation record. Obtain from the holder its list of anchor identities (tag name, tag object id, peeled commit, `h`, anchor-record hash) and compare it with the **whole** tag list of step 3. A tag of the holder's list missing at the remote, a tag at the remote missing from the holder's list other than the last one (by `h`), or any differing value means that the remote references were rewritten (`REMOTE_REWRITE_DETECTED`; the step's result is then `DIFFERENCE`).
   - (d) **Anchoring window.** Only the **last** tag may be absent from the holder's list: status `LAST_ANCHOR_NOT_IN_COPY` (section 9 step 6 is not complete), not a violation. The step then reports the **copy-confirmed index** = the `h` of the last anchor that the holder's list holds; the entries after it are not confirmed against a remote rewrite until the holder writes the last anchor. Without the status, the copy-confirmed index is the last anchored index.
   - (e) Only when the holder **cannot be consulted** does this step report **`NOT_VERIFIABLE`** (section 1).

**Result.** In `CURRENT` mode: `VIOLATION` if any step reports a violation code (the codes in parentheses above). Otherwise `PASS_WITH_DECLARED_LIMITS`, always stated with: the last anchored index, the copy-confirmed index, the list of `PENDING_REANCHOR` entries, the statuses (`UNANCHORED`, `LAST_ANCHOR_NOT_RECORDED`, `LAST_ANCHOR_NOT_IN_COPY`), the result of step 6 (`MATCH` or `NOT_VERIFIABLE`) and the derived state of every subject. The verifier never reports a pass without these limits. In `HISTORICAL` mode: `VIOLATION`, or **`HISTORICAL_PASS_WITH_DECLARED_LIMITS`** with the same limits and the list of `LATER_ANCHORS`; it is never a pass of the current state. The same bytes verified in `CURRENT` mode give `VIOLATION`, and these codes are then expected: `TRUNCATED_BELOW_ANCHOR` for every later anchor; `UNRECORDED_ANCHOR` for every anchor other than the last whose `ANCHOR_RECORDED` entry lies after index `n` (the last one gives the status `LAST_ANCHOR_NOT_RECORDED`); and `ANCHOR_RECORD_BASELINE_MISMATCH` for every later anchor of a baseline version that is not in force at `n` (section 12, case T14). The vector generator of 4.1 holds a model of steps 1-6 and runs it on the example and on the cases of section 12. The model is limited to what those cases exercise: the pin-slot table of each sealed BA-11 is an input (it does not read BA-11 bytes); it does not check a designation record against the BA-07 entry of BA-11 and does not look for a later designation record when no tag exists; it has no `EVM_FROZEN` review-record check (`PIN_REVIEW_RECORDS_MISSING`, `REVIEW_AFTER_LEARNING_START`); it has no build-equivalence record (5.3) and no bound-SHA precondition record (5.4); and it does not model the consumption of a pin by the derivation of another pin (5.1 rule 3), which step 5(c) checks.

## 7. Roles and transitions

| Role | Holds | May not |
|---|---|---|
| `CAPTURE_ROLE` | runs capture **through the control plane** | approve its own capture |
| `REVIEW_ROLE` | the Owner or CAD manager (or a delegate they record): checks the machine-layer entries and the build layer against the build receipt | capture or approve |
| `APPROVAL_ROLE` | the Coordinator: authorizes a capture, approves the baseline (`BASELINE_APPROVED`), a manifest hash (`APPROVED`) and a ratified pin (`PIN_RECORDED`), records supersession and revocation, and writes the manifest carry-over records of section 8 | capture |
| `CUSTODY_ROLE` | **the Coordinator logical role by default** (designated by this artifact; a change is a new version): keeps the custody log, records the baseline, the anchors, the review records (the build-equivalence records of 5.3 and the bound-SHA precondition records of 5.4 included) and each use | alter an entry already anchored |

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
PIN:            RECORDED (pin record) -> RATIFIED (ratification pair; PIN_RECORDED cites it) -> ANCHORED (derived)
                PIN_RECORDED -> SUPERSEDED (only as the second entry of a supersession pair, 5.1 rule 2) -> [REVOKED]
                PIN_RECORDED -> REVOKED (with a recorded cause)
REVIEW_RECORD:  REVIEW_RECORD_ADDED   (no later event; a reopened review, a new verification of the build equivalence or a new bound-SHA precondition check is a new record)
MANIFEST:       AUTHORIZED -> CAPTURED -> REVIEWED -> APPROVED -> IN_USE -> IN_USE -> ... -> SUPERSEDED
                every state except REVOKED -> REVOKED (with a recorded cause), SUPERSEDED included (V2.1 29.3: "any state -> REVOKED")
                subjectHash = the manifest identity from CAPTURED on, and it never changes; it is null in AUTHORIZED and in a
                REVOKED entry that directly follows AUTHORIZED
```

A pin may also go from `SUPERSEDED` to `REVOKED`: a superseded pin later found defective is revoked with its cause, which marks the runs that consumed it. `SUPERSEDED` and `REVOKED` never return to use, for a pin or a manifest.

**Derived anchoring (model A, AR3-23).** A subject is **`ANCHORED`** when its last entry has an index at most `h` of a verified anchor (6.2 step 3); otherwise it is **`PENDING_REANCHOR`**. There is no `ANCHORED` entry, so one anchor covers any number of subjects and a re-anchor after `IN_USE`, `SUPERSEDED` or `REVOKED` needs no transition of those subjects. The contract's `APPROVED -> ANCHORED` transition by `CUSTODY_ROLE` (V2.1 29.3, 29.4) is, in model A, the `ANCHOR_RECORDED` entry of an anchor whose `h` is at least the index of the `APPROVED` entry. That custody entry is separate from the approval entry (V2.2 PA-13).

**Use preconditions** (6.2 step 5 checks U1 to U4; AR3-23, AR4-13; U5: AR4-06 (4) and AR5-04; U6: AR5-12, AR7-04 and AR7-05):

- **When `IN_USE` is appended** (R2:BA07-V4-03). **`IN_USE`** is recorded **at each use** (V2.1 29.3), **after the run ends**, once the run's canonical tuple record exists (it holds post-run SESSION_BOUND fields, V2.1 2.1), and before any adjudication uses the run. What binds the run to its inputs is fixed **at run start**: when the control plane samples the run's BUILD_BOUND fields, including the manifest hash and custody-log reference (section 4), before library acquisition, it also finds the latest anchor at the remote and matches it to its `ANCHOR_RECORDED` entry (6.2 steps 3 and 4). That anchor is the run's **run-start anchor**; the control plane records it in the run log, and the `IN_USE` entry carries it (`runStartAnchor`). **`IN_USE -> IN_USE` is legal** (AR3-23): a second use does not wait for the first to be anchored.
- For an `IN_USE` entry at index `u`, with the **version in force at `u`** (the `baselineVersion` of the last `BASELINE_APPROVED` entry before `u`):
  - **U1, run-start anchor.** `runStartAnchor` names an anchor of step 3 with that anchor-record hash, whose `ANCHOR_RECORDED` entry has an index lower than `u`, and whose `BASELINE_VERSION` is the version in force at `u` (`USE_REFERENCE_INVALID`). A run that starts before the first anchor of a new baseline version, or is in flight across a version change, cannot be recorded as a use under it.
  - **U2, manifest.** The manifest's `APPROVED` entry has an index at most the `h` of the run-start anchor (`USE_BEFORE_ANCHOR`): a manifest that is not anchored cannot be `IN_USE` (V2.1 29.3). The manifest is usable under the version in force: that version was in force at its `APPROVED` entry, or the manifest was carried over to it (section 8) (`MANIFEST_VERSION_MISMATCH`).
  - **U3, pins.** For every hash in `pinRecordHashes`: the pin's `PIN_RECORDED` entry has an index at most the `h` of the run-start anchor (`PIN_USE_BEFORE_ANCHOR`); its record's `baselineVersion` is the version in force at `u` (`PIN_VERSION_MISMATCH`); and it has no `SUPERSEDED` or `REVOKED` entry with an index lower than `u` (`PIN_USE_AFTER_CLOSURE`). Every governing run records its pins this way, and `pinRecordHashes` attributes the run to the exact pin versions it consumed (5.1 rule 3).
  - **U4, no pending closure** (R1:BA07-V4-04). Every `SUPERSEDED` or `REVOKED` entry with an index lower than `u` has an index at most the `anchoredIndex` of an `ANCHOR_RECORDED` entry with an index lower than `u` (`USE_WITH_PENDING_CLOSURE`). A supersession or revocation is therefore anchored before any later use is recorded, so the absence of a closure that a run relies on is established by an anchor recorded before its `IN_USE` entry.
  - **U5, build equivalence, both directions** (AR4-06 (4), AR5-04; 5.3). (a) A `PRODUCT_BUILD` run that consumes a pin of kind `E04_ADMITTED_SET`, `LOCK_MODE` or `HOST_DEFAULT_MAP` (the pins derived on the characterization build) relies on a build-equivalence record whose `productBuild` equals its own build values and whose `characterizationBuild` equals those of `CHARACTERIZATION_TUPLE`. (b) A governing `CHARACTERIZATION_BUILD` run that consumes the `EVM_FROZEN` pin relies on a build-equivalence record whose `characterizationBuild` equals its own build values and whose `productBuild` equals the values of the `PRODUCT_BUILD` on which the frozen EVM was learned and validated (the build of the runs that the sealed learning-corpus record cited by the pin record names, 5.1, and of the validation runs). In both directions the record establishes the equivalence (5.3, "What a record establishes"), its `REVIEW_RECORD_ADDED` entry has an index at most the `h` of the run-start anchor, and the record hash, with its path, is cited in `RUN_START` (the run's first log record), sampled together with the BUILD_BOUND fields and the run-start anchor before library acquisition; the run's evidence package carries a copy of the record, or its `{path, sha256}`, next to the canonical tuple record (5.3; BA07V6-04; for (b), AR5-04 (1)). Without such a record the pin does not apply: the control plane does not start the run; and for (b), a run that started without a cited record cannot give `PASS` for E12A or E12C (AR5-04 (4)). The `IN_USE` entry carries neither the build under test nor that hash, so 6.2 step 5 does not check U5 from the custody log: the control plane checks it at run start, when it fixes the run-start anchor, and does not start a run for which it fails; an audit re-checks it from the run log and the custody log (declared, section 10).
  - **U6, bound-SHA precondition** (AR5-12, AR7-04, AR7-05; 5.4). U6 applies to **every run on a build under test** (`PRODUCT_BUILD` or `CHARACTERIZATION_BUILD`), governing or not (AR7-04). It is mandatory for the runs that feed a pin, a selection, the warm-up criterion or the inventory: `E4-LEARN`, `LK`, `HDM-CHAR`, `WU-DRY` and the trace dry runs (AR7-04; the scenario catalog, BA-04, informative pointer). Outside it: synthetic runs (`BUILD_UNDER_TEST = NONE`) and the `FIXTURE_CONSTRUCTION_BUILD` (FXB-03; 5.5). For a non-governing run, which has no `IN_USE` entry, the run-start anchor named in U6 is the latest anchor published at the remote when the control plane samples the run's `RUN_START`, matched to its `ANCHOR_RECORDED` entry as in the first bullet of this list and recorded in the run log only (no field is added to `IN_USE` or to the tuple, AR7-05). Such a run relies on a bound-SHA precondition record whose `buildUnderTest` equals its own build role and BUILD_BOUND values (Plugin and harness DLL SHA-256, source SHA), whose `boundProductSha` is the bound product SHA of the baseline version in force at the run's start (the version of its run-start anchor, U1), and whose `result` is `EQUAL`, or `DELTA_PROCESSED` with `citedArtifactsUnchanged = true` (BA07V6-03). Before it accepts U6, the control plane re-derives the path set of `citedPairs` from `citingArtifacts` and `result` from `citedPairs`, and refuses the run on any mismatch (5.4, BA07V6-02). That record's `REVIEW_RECORD_ADDED` entry has an index at most the `h` of the run-start anchor, and the record hash, with its path, is cited in `RUN_START` (the run's first log record), sampled together with the BUILD_BOUND fields and the run-start anchor before library acquisition; the run's evidence package carries a copy of the record, or its `{path, sha256}`, next to the canonical tuple record (5.4; AR7-05; BA07V6-04). As for U5, the control plane checks U6 at run start and does not start a run for which it fails; 6.2 step 5 does not check it from the custody log, and an audit re-checks it from the run log and the custody log (declared, section 10; AR7-05). No record serves the `FIXTURE_CONSTRUCTION_BUILD` (5.5).
- A manifest `SUPERSEDED` or `REVOKED` before `u` cannot be `IN_USE` (`ILLEGAL_TRANSITION`). A run whose manifest or consumed pin is superseded or revoked before its `IN_USE` entry can be appended is not recorded as a use; its manifest is then no longer the approved and anchored manifest of section 4, so the run is `INVALID` (V2.1 29.1).
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
| `ratificationRecords` | `null` | BA-11's candidate record and its Coordinator and Architect ratification records (5.2). They establish BA-11's seal hash (`SEAL_HASH` = `CANDIDATE_HASH` = this `subjectHash`); there is no separate seal record, and none of them is ever written into BA-11 |
| `role` / `actor` | `CUSTODY_ROLE` / the Coordinator | `APPROVAL_ROLE` / the Coordinator |
| `prevEntryHash` | 64 zero digits | the `entryHash` of index 0 |

The first anchor (`ct21d-baseline/v1`) has `h = 1`; its `ANCHOR_RECORDED` entry is index 2.

**New baseline version** (AR3-23, AR4-13). A new BA-11 seal, for example after a new version of any sealed artifact (BA-11 section 2), is recorded in the **same log**, never in a new one. These entries are appended in this order:

1. a new `BASELINE_RECORDED` (the new BA-11 seal hash, the BA-07 seal hash in force, `baselineVersion = v<k+1>`);
2. a new `BASELINE_APPROVED` (BA-11's three records, plus one `MANIFEST_CARRY_OVER` record for each manifest carried over);
3. `SUPERSEDED` of the previous baseline (`subjectHash` = the previous BA-11 seal hash, `supersededBy` = the new one);
4. the **carry-over block** (it may be empty), which holds only:
   - for each live pin of `v<k>` that is carried over: its new version for `v<k+1>` (`PIN_RECORDED`; the same value is admitted; `supersedes` = the `v<k>` pin; its own ratification pair), immediately followed by `SUPERSEDED` of the `v<k>` pin (5.1 rules 2 and 4);
   - for each live pin of `v<k>` that is not carried over: `REVOKED`, cause `SUPERSEDED_BY_RULING`;
   - for each open manifest (its last event is neither `SUPERSEDED` nor `REVOKED`) that has no `MANIFEST_CARRY_OVER` record: `REVOKED`, cause `SUPERSEDED_BY_RULING`; it is captured again under `v<k+1>` (section 3).

The first anchor of the new version (`ct21d-baseline/v<k+1>`) is then created with `h` = the index of the last entry of the block (of entry 3 when the block is empty). No other entry lies between entry 3 and that anchor (`BASELINE_SEQUENCE`). At that `h`, every live pin of `v<k>` has been carried over or revoked, and every open manifest has been carried over or revoked (`CARRY_OVER_MISSING`). Index 0 stays the genesis entry of `v1`.

**Carry-over of the manifest** (AR4-13). The manifest is bound to the tuple (EXEC-11), not to the baseline. It stays usable under `v<k+1>` only when the ruling that approves `v<k+1>` records that BA-07 and every tuple field bound to the manifest are unchanged. That record is the Coordinator's `MANIFEST_CARRY_OVER` record (5.2: `subjectKind = MANIFEST`, `subject` = the `captureId`, `subjectHash` = the manifest identity, `reference` = that ruling), cited by `BASELINE_APPROVED(v<k+1>)`, so the first anchor of `v<k+1>` covers it. A manifest without an identity (its last event is `AUTHORIZED`) cannot be carried over. The verifier checks that the record names an open manifest with that identity and that the BA-07 seal hash is unchanged (the `baselineCustodyHash` of `BASELINE_RECORDED(v<k+1>)` equals that of `v<k>`); otherwise the record is `CARRY_OVER_INVALID` and the manifest counts as not carried over. That no tuple field changed is the approving ruling's statement: the verifier does not re-derive it (declared, section 10).

**Carry-over of pins** (AR4-13): fail-closed, by 5.1 rules 1 and 4. A pin is usable only under the baseline version named in its record; without its new version for `v<k+1>`, a `v<k>` pin is not usable under `v<k+1>`.

**Unusability from `BASELINE_APPROVED(v<k+1>)` on.** No item of `v<k>` is usable under `v<k+1>` unless it was carried over: a use under `v<k+1>` needs a run-start anchor of `v<k+1>` (section 7, U1), and the first anchor of `v<k+1>` follows the whole block; a pin needs `baselineVersion = v<k+1>` (U3); a manifest needs its carry-over record (U2).

A pin write, a review record, a manifest event or a use **never** creates a baseline version. They are custody events of the baseline in force and are re-anchored as `ct21d-baseline/v<k>-a<n>` (section 9). BA-11 section 3.1 is a sealed slot table and never receives a value.

**Status of BA-11** (AR2-44, AR3-25). BA-11's own status lives outside BA-11:

- `CANDIDATE_FOR_SEALING` and `RATIFIED` in its candidate record and its two ratification records (section 2);
- `SEALED`: its `SEAL_HASH` = its `CANDIDATE_HASH`, established by those three records (there is no separate seal record, 5.2) and recorded as the `subjectHash` of `BASELINE_RECORDED` and `BASELINE_APPROVED`;
- anchored as derived (section 7);
- `SUPERSEDED` by the `SUPERSEDED` entry of a later baseline version.

## 9. External anchor (procedure; nothing is created here)

**The first anchor** of the first baseline version is created **only after**:

- every baseline artifact is finalized and sealed. The seal of BA-07 requires the designation of the independent copy (a before-seal prerequisite), so the copy is designated before the first anchor;
- BA-11 is sealed last, with the seal hash and the ratification records of **every other** artifact. BA-11's own candidate record and ratification records are written under `ratifications/` (section 2) and cited by entry 1 of section 8, never inside BA-11;
- every ratification is recorded;
- entries 0 and 1 of section 8 are appended.

**A final anchor is never created earlier.** **Every later anchor** needs at least one entry after the previous `h`, and the `ANCHOR_RECORDED` entry of every earlier tag must be in the log, so that the new anchor covers it. The first anchor of a later baseline version is created right after its carry-over block (section 8).

| Step | Rule |
|---|---|
| 1 | the `CUSTODY_ROLE` fixes `h` = the index of the last entry of the log and `H` = its `entryHash`. The log is held: no entry is appended between this step and step 6 |
| 2 | it writes the anchor record (schema below) at `docs/automation/evidence/ct21d/baseline-anchor.json` as a canonical record file (section 4) |
| 3 | the anchor record and the custody log, which ends at index `h`, are committed in a **reviewed** commit (the anchor commit) |
| 4 | the commit is **pushed** to the remote `origin` (`https://github.com/marioap-afk/Calculadora_de_racks.git`), and its presence at the remote is **verified** (`git ls-remote` lists a branch ref at that commit) |
| 5 | an **annotated tag** (name below) is created on the anchor commit and pushed. `git ls-remote --tags` must list its tag object id and, on the `^{}` line, the anchor commit. **The annotated tag is the reference of record**: feature branches may be rebased under the repository workflow, so verification uses the **tag's peeled commit**, never branch ancestry (AR2-39) |
| 6 | the `CUSTODY_ROLE` appends the **`ANCHOR_RECORDED`** entry: `subjectKind = ANCHOR`, `subjectHash` = the anchor-record hash, `anchorRef` = `{anchoredIndex: h, peeledCommit, remote, tag, tagObjectId}`. It commits and pushes this entry (the anchor-recording commit) and verifies its presence at the remote. The designated holder then writes the anchor identity (tag name, tag object id, peeled commit, `h`, anchor-record hash, UTC time) to the **independent copy** (section 2). Until the holder has written it, 6.2 step 6 reports the status `LAST_ANCHOR_NOT_IN_COPY`, not a violation, and the entries after the previous copy-confirmed anchor are not confirmed against a remote rewrite (section 1). The `ANCHOR_RECORDED` entry is itself pending until the next anchor |
| 7 | every later custody event (pin, review record, manifest event, use, supersession, revocation, new baseline version with its carry-over block) is re-anchored in the next evidence commit with steps 1-6 (V2.1 29.4). The `ANCHOR_RECORDED` entry of an anchor does not by itself call for a new anchor, since anchoring would then never end: the next anchor that another event calls for covers it. Until then these entries form the pending window (section 1). A supersession or revocation must be anchored before the next use is recorded (section 7, U4) |

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
INDEPENDENT_COPY             = UNSET   (at anchoring time, the designation in force: {designationRecord: {path, sha256}, holder, location};
                                        never UNDESIGNATED in an anchor record: 6.2 step 6, DESIGNATION_MISSING)
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
| a supersession or revocation appended after the last anchored index and lost before any anchor covers it | **not detectable** (declared, section 1). While it was in the log, no use could legally be recorded after it (step 5, `USE_WITH_PENDING_CLOSURE`; case T12) |
| a tag or anchor commit rewritten or deleted at the remote: the last tag deleted together with its pending `ANCHOR_RECORDED` entry, every tag deleted with the log reset, or tags re-created over a forged log | comparison of the whole tag list with the holder's list (step 6; cases T4b, T6, T7). The designation comes from the anchor records and from the primary designation record, so deleting or re-creating the tags does not switch the step off. **Not detectable while the holder cannot be consulted** (`NOT_VERIFIABLE`; case T4a) |
| an anchor record that says `UNDESIGNATED`, or cites a designation record that does not match | step 6 (`DESIGNATION_MISSING`, `DESIGNATION_RECORD_MISMATCH`; case T7) |
| the last anchor rewritten before the holder has written it (the anchoring window of section 9 step 6) | **not detectable** until the holder writes it; reported by the status `LAST_ANCHOR_NOT_IN_COPY` and the copy-confirmed index (case T5) |
| an illegal transition, a wrong role, a missing or extra field | state-machine and field checks (step 5; case T11 shows the legal revocations) |
| a pin with an invalid ratification pair, for a slot the sealed table lacks, or recorded or used under another baseline version; two live pins for one slot; a supersession without its pair; a consumed pin superseded within its version | step 5 (`PIN_SEAL_INVALID`, `PIN_SLOT_UNKNOWN`, `PIN_VERSION_MISMATCH`, `PIN_SLOT_CONFLICT`, `PIN_SUPERSESSION`, `CONSUMED_PIN_SUPERSEDED`; cases T8, T9, T10, T17) |
| an item of the previous baseline version neither carried over nor revoked; a carry-over outside the block; a manifest carry-over although BA-07 changed | step 5 (`CARRY_OVER_MISSING`, `CARRY_OVER_INVALID`, `BASELINE_SEQUENCE`; cases T9, T16) |
| a tuple field changed although a manifest was carried over | **not re-derived by the verifier**: it is the statement of the ruling that approves the new baseline version (section 8) |
| a manifest or a pin used before it is anchored; a use not bound to a run-start anchor that covers its inputs; a use under another baseline version | use preconditions (step 5: `USE_REFERENCE_INVALID`, `USE_BEFORE_ANCHOR`, `PIN_USE_BEFORE_ANCHOR`, `MANIFEST_VERSION_MISMATCH`, `PIN_VERSION_MISMATCH`; cases T3, T9, T13) |
| an earlier revision verified | `HISTORICAL` mode: the later anchors are listed, and the result is never a pass of the current state (case T14) |
| a `REVIEWED` inventory record custodied after a learning run of the frozen EVM started, or a learning run that the learning-corpus record names without an `IN_USE` entry | step 5 (`REVIEW_AFTER_LEARNING_START`, 5.1); not in the model of 4.1 (6.2 **Result**) |
| a `PRODUCT_BUILD` run that consumes a pin derived on the characterization build, or a governing `CHARACTERIZATION_BUILD` run that consumes the `EVM_FROZEN` pin, with no build-equivalence record that establishes the equivalence (5.3) anchored before its start | U5 (section 7), both directions: checked by the control plane at run start and re-checkable from the run log and the custody log; **not** checked by 6.2 step 5, because the `IN_USE` entry does not carry the build under test (declared) |
| the event neutrality of what is present only in the characterization build, stated in a build-equivalence record | **not re-derived**: it is the reviewer's statement with its evidence (5.3); the control plane re-computes only `catalogFolderSetsEqual` (declared) |
| a run on a build under test (governing or not, AR7-04) whose build has no bound-SHA precondition record anchored before its start, or relies on a record of another build or another bound product SHA, or on an unprocessed delta or a processed delta that changes normative content of BA-03, BA-06 or BA-08 (`citedArtifactsUnchanged = false`) | U6 (section 7): checked by the control plane at run start and re-checkable from the run log and the custody log; **not** checked by 6.2 step 5 (declared; AR7-05) |
| a bound-SHA precondition record whose `citedPairs` path set differs from the union of the precondition sets that its `citingArtifacts` declare, or whose `result` does not follow from its `citedPairs` (BA07V6-02) | U6 (section 7): the control plane re-derives the path set from `citingArtifacts` and `result` from `citedPairs` (blob equality; `null` = delta) before it accepts U6, and refuses the run on any mismatch; re-checkable by audit; **not** checked by 6.2 step 5. The statement `citedArtifactsUnchanged` of the processing ruling is **not re-derived** (declared, 5.4) |
| a build-equivalence record or a bound-SHA precondition record found wrong after it was recorded | **not a custody transition** of this procedure: a review record has no later event; it is a finding for the Coordinator (declared, 5.3, 5.4) |
| a pin superseded within its version after another pin was derived from it | step 5 (`CONSUMED_PIN_SUPERSEDED`, consumption by derivation, 5.1 rule 3; AR5-06 (b)); not in the model of 4.1 (6.2 **Result**) |
| a pin derived from a pin that is later revoked | **not a custody transition** of this procedure: the derived pin is not revoked by it; the ruling that records the revocation decides (declared, 5.1 rule 3) |
| the `FIXTURE_CONSTRUCTION_BUILD` used as a build under test; its identity missing from the fixture conformance record of `FIXTURE_INSTANCE@FX-ANN-BLANK` | the first: U6 (no bound-SHA precondition record can serve it, 5.5); the second: the ratification pair of that pin (5.5); neither is checked by 6.2 (declared) |

## 11. What remains before NB-6 can close

| Id | Item | Gate |
|---|---|---|
| N6-1 | Architect ruling `CANDIDATE_FOR_SEALING` on the exact bytes of this V7 and of its two vector files (the V5 vector files, unchanged; the paths that the BA-07 entry of the BA-11 registry lists), with the Coordinator's designation of the independent copy recorded in the candidate record or in the Coordinator's ratification record (section 2); then the Coordinator and Architect ratifications; then the seal. Preconditions: BA-01 at least `CANDIDATE_FOR_SEALING` (BA-11 section 2), which needs the verification of V2.6 and the agreement of the effective contract (AR6-05, AR6-06); the Architect's agreement to BA-11 sections 1, 2 and 2.1 as the sealing workflow in force, made after V2.6 (AR6-06) on the BA-11 blob that the agreement cites (rule 5; AR3-25, AR4-22). AQ-V6-BA07-1 and AQ-V6-01 are ruled by AR7-05 and AR7-03 (section 226) and applied in this version (sections 5.4, 7 and 10), and AQ-V5-04 and AQ-V5-01 by AR5-06 and AR5-04 (section 220) (sections 5.1, 5.3, 7 and 10), so they are no longer preconditions; AQ-V4-12 stays ruled by AR4-13 | baseline preparation (no host) |
| N6-2 | the baseline anchor published (section 9) | the **last** step of the baseline, after BA-11 is final |

The captured manifest, the pin records, the build-equivalence record (5.3), the bound-SHA precondition records (5.4), the output of the precondition verifier that each of them cites (AR7-03) and their anchoring are **execution prerequisites** (EXEC-11, the pin slots of BA-11 section 3.1, AR4-06 (4), AR5-04, AR5-12 and AR7-03) and do not block the baseline.

### 11.1 Architect rulings applied (sections 220 and 226)

| Id | Ruling | Applied in |
|---|---|---|
| AQ-V5-04 | **AR5-06**: the three readings of BA-07 V5 are accepted. (a) The custody-log reference of the tuple field *manifest hash and custody-log reference* (V2.1 2.1) is a per-manifest constant (the first anchor that covers its `APPROVED` entry, computed from the anchors discovered at the remote, unchanged by a carry-over), with `IN_USE.runStartAnchor` apart. (b) No replacement of a consumed `LOCK_MODE`, `HOST_DEFAULT_MAP` or `FIXTURE_INSTANCE` pin within a baseline version (`E04_ADMITTED_SET` never; `EVM_FROZEN` as a new model version), with the disclosure that a defective consumed pin can be revoked but its slot stays empty until a new version, and with "consumed" completed for a pin used to derive another. (c) The carry-over block before the first anchor of `v<k+1>`, with the mandatory `REVOKED` of what is not carried over | (a) section 4 ("association with the tuple"), 6.1 `runStartAnchor`, section 7 (U1 to U3): text kept, now ruled; (b) 5.1 rule 3 (consumption by derivation, the disclosure) and `sourceEvidence`, 6.2 step 5(c) and **Result**, section 10; (c) 5.1 rule 4 and section 8: text kept, now ruled |
| AQ-V5-01 | **AR5-04**: yes, as a reading (without errata): the recorded equivalence of AR4-06 (4) is also the basis of the reverse direction, with conditions (1) to (6) | 1 (f); section 4 (arrays); 5.3 (reverse direction; `characterizationOnlyAssemblies`, `eventNeutralityEvidence`, `catalogFolderFiles`, `catalogFolderSetsEqual`; what a record establishes; use (a) and (b)); 7 U5; section 10 |
| AQ-V6-BA07-1 | **AR7-05** (section 226): the reading of V6 section 5.4 ("saved with the tuple": a review record bound to the tuple by value and by reference) is sufficient for AR5-12, with no new field in `IN_USE` or in the tuple, under three text conditions: the record hash is cited in `RUN_START` (the same formula as U5 (b) and as the rule of the scenario catalog); the run's evidence package holds the record or its `{path, sha256}`; `runnerOutput` is agnostic of the detector (the output of the verifier of AR7-03) | 5.4 ("Saved with the tuple", `runnerOutput`); 5.3 (use); 7 U5 and U6; section 10; 11; the vector is unchanged (4.1) |
| AQ-V6-01 | **AR7-03** (section 226; a reading, without errata): the detection of the precondition of AR5-12 for the **union** of the pairs that BA-03, BA-06 and BA-08 cite lives in a **dedicated precondition verifier**, an execution-preparation tool outside the registry (no row of BA-11, no edge), in the custody of BA-07, which reads the pair lists of the sealed BA-03, BA-06 and BA-08 and whose output the record of 5.4 cites; "the BA-03 runner detects it" = the BA-03 runner detects its own pairs | header (`FILES`, `DEPENDS_ON`); 1 (g); 2 (location of the output); 5.4 (intro, `runnerOutput`); 11 |
| AQ-V6-BA04-1 | **AR7-04** (section 226): the precondition applies to every run on a build under test (`PRODUCT_BUILD` or `CHARACTERIZATION_BUILD`), governing or not; mandatory for the runs that feed a pin, a selection, the warm-up criterion or the inventory (`E4-LEARN`, `LK`, `HDM-CHAR`, `WU-DRY`, the trace dry runs); outside: synthetic runs (`NONE`) and the `FIXTURE_CONSTRUCTION_BUILD` (FXB-03). BA-07 widens U6 | 1 (threat model row); 5.4 (intro); 7 U6; section 10 |

No Architect question of this artifact is open. AQ-V4-12, AQ-V5-04, AQ-V5-01, AQ-V6-BA07-1 and AQ-V6-01 are ruled and are not reopened.

```text
NB-6 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until this procedure is sealed and the baseline anchor is published
MANIFEST = NOT CAPTURED ; CUSTODY LOG = NOT CREATED ; PIN RECORDS = NONE ; BUILD-EQUIVALENCE RECORD = NONE ; BOUND-SHA PRECONDITION RECORD = NONE ; ANCHOR = NOT CREATED ; INDEPENDENT COPY = UNDESIGNATED
OPEN ARCHITECT QUESTIONS = NONE     RULED AND APPLIED = AQ-V5-04 (AR5-06), AQ-V5-01 (AR5-04), AQ-V6-BA07-1 (AR7-05), AQ-V6-01 (AR7-03), AQ-V6-BA04-1 (AR7-04)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 12. Worked example (mandatory, AR3-23; values illustrative)

The example runs the mandatory sequence **genesis -> approval -> anchor -> manifest -> anchor -> IN_USE x2 -> re-anchor -> pins -> re-anchor** (indexes 0-13). A continuation (indexes 14-23) adds a use that consumes both pins, a new baseline version with its carry-over block, a use under the new version and a re-anchor. The generator of 4.1 builds it; the full lines and hashes are in its output. **Every hash, id, time, actor, run id and value is illustrative** (derived from `EXAMPLE:` labels). None is a hash of a real artifact, and none is a decision about any pin value. Hashes are shortened to 12 or 16 hexadecimal digits here; the output holds the full values.

| Index | Event | Role | Subject | Fields that apply | First anchor that covers it | `entryHash` (first 16) |
|---|---|---|---|---|---|---|
| 0 | `BASELINE_RECORDED` | `CUSTODY_ROLE` | BASELINE `a0a1282ca1f6...` (BA-11 seal v1) | `baselineVersion = v1`, `baselineCustodyHash` | `v1` (h = 1) | `343faf889cd416b2` |
| 1 | `BASELINE_APPROVED` | `APPROVAL_ROLE` | BASELINE `a0a1282ca1f6...` | `baselineVersion = v1`, `ratificationRecords` (3: BA-11's candidate, Coordinator and Architect records) | `v1` (h = 1) | `062cecb186519acd` |
| 2 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `461d3a937898...` | `anchorRef`: tag `ct21d-baseline/v1`, anchoredIndex 1 | `v1-a1` (h = 6) | `20716bfb1e9247f5` |
| 3 | `AUTHORIZED` | `APPROVAL_ROLE` | MANIFEST `CAP-1` (`subjectHash = null`) | `captureId` | `v1-a1` | `1ab73109b5302f48` |
| 4 | `CAPTURED` | `CAPTURE_ROLE` | MANIFEST `CAP-1` = `b8ff62b0df19...` | `captureId` | `v1-a1` | `2cfad4604c968d46` |
| 5 | `REVIEWED` | `REVIEW_ROLE` | MANIFEST `CAP-1` | `captureId` | `v1-a1` | `615b2f9954a4274d` |
| 6 | `APPROVED` | `APPROVAL_ROLE` | MANIFEST `CAP-1` | `captureId` | `v1-a1` (h = 6) | `34486ca34921ebfb` |
| 7 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `9d2d0c0f147b...` | tag `ct21d-baseline/v1-a1`, anchoredIndex 6 | `v1-a2` (h = 9) | `0628cbc4207b25e5` |
| 8 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-1`, `tupleRecordHash`, `pinRecordHashes = []`, `runStartAnchor` = `v1-a1` | `v1-a2` | `acd8be200340a15a` |
| 9 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-2`, `tupleRecordHash`, `pinRecordHashes = []`, `runStartAnchor` = `v1-a1` | `v1-a2` (h = 9) | `aba1b9c73ab63d5c` |
| 10 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `b07bd96a75d8...` | tag `ct21d-baseline/v1-a2`, anchoredIndex 9 | `v1-a3` (h = 12) | `0e33b7570c07e469` |
| 11 | `PIN_RECORDED` | `APPROVAL_ROLE` | PIN `1cbe940e66b7...` (record: `baselineVersion = v1`, `supersedes = null`) | `pinSlot = LOCK_MODE`, `ratificationRecords` (2) | `v1-a3` | `3e21b2238d2c5099` |
| 12 | `PIN_RECORDED` | `APPROVAL_ROLE` | PIN `947ee2ce5101...` (record: `baselineVersion = v1`, `supersedes = null`) | `pinSlot = E04_ADMITTED_SET@R-1`, `ratificationRecords` (2) | `v1-a3` (h = 12) | `64810e0d26dcce8f` |
| 13 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `ac8effac5746...` | tag `ct21d-baseline/v1-a3`, anchoredIndex 12 | `v2` (h = 20) | `9a2b1edc124c04ae` |
| 14 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-3`, `pinRecordHashes` = both v1 pins, `runStartAnchor` = `v1-a3` | `v2` | `b0239ce6e93960f8` |
| 15 | `BASELINE_RECORDED` | `CUSTODY_ROLE` | BASELINE `389a7681b132...` (BA-11 seal v2) | `baselineVersion = v2`, `baselineCustodyHash` (the BA-07 seal hash, unchanged) | `v2` | `e41279735efd0bea` |
| 16 | `BASELINE_APPROVED` | `APPROVAL_ROLE` | BASELINE `389a7681b132...` | `baselineVersion = v2`, `ratificationRecords` (4: BA-11's three records and the `MANIFEST_CARRY_OVER` record of `CAP-1`) | `v2` | `3c850b4acce291a8` |
| 17 | `SUPERSEDED` | `APPROVAL_ROLE` | BASELINE `a0a1282ca1f6...` (v1) | `supersededBy = 389a7681b132...` | `v2` | `0f1675305baeb05a` |
| 18 | `PIN_RECORDED` | `APPROVAL_ROLE` | PIN `6ac7e7620eac...` (record: `baselineVersion = v2`, same value, `supersedes = 1cbe940e66b7...`) | `pinSlot = LOCK_MODE`, `ratificationRecords` (2); carry-over block | `v2` | `f3a255bb9674435e` |
| 19 | `SUPERSEDED` | `APPROVAL_ROLE` | PIN `1cbe940e66b7...` (LOCK_MODE v1) | `supersededBy = 6ac7e7620eac...`; carry-over block | `v2` | `5780ab63540a23df` |
| 20 | `REVOKED` | `APPROVAL_ROLE` | PIN `947ee2ce5101...` (E04 v1) | `cause = SUPERSEDED_BY_RULING` (not carried over); carry-over block | `v2` (h = 20) | `8e09690eaf878ebf` |
| 21 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `f00fdfbb57e5...` | tag `ct21d-baseline/v2`, anchoredIndex 20 | `v2-a1` (h = 22) | `bb53cc3783340d82` |
| 22 | `IN_USE` | `CUSTODY_ROLE` | MANIFEST `CAP-1` | `runId = EXAMPLE-RUN-4`, `pinRecordHashes` = the v2 LOCK_MODE pin, `runStartAnchor` = `v2` | `v2-a1` (h = 22) | `3a9536e719908d0a` |
| 23 | `ANCHOR_RECORDED` | `CUSTODY_ROLE` | ANCHOR `6f6a41dda52f...` | tag `ct21d-baseline/v2-a1`, anchoredIndex 22 | pending | `e3c23c32a1d3e2e0` |

| Tag (illustrative remote) | `h` | `MANIFEST_CUSTODY_HEADER_HASH` (first 16) | anchor-record hash (first 16) |
|---|---|---|---|
| `ct21d-baseline/v1` | 1 | `062cecb186519acd` | `461d3a937898f834` |
| `ct21d-baseline/v1-a1` | 6 | `34486ca34921ebfb` | `9d2d0c0f147bfa97` |
| `ct21d-baseline/v1-a2` | 9 | `aba1b9c73ab63d5c` | `b07bd96a75d805b4` |
| `ct21d-baseline/v1-a3` | 12 | `64810e0d26dcce8f` | `ac8effac5746bd1b` |
| `ct21d-baseline/v2` | 20 | `8e09690eaf878ebf` | `f00fdfbb57e5a766` |
| `ct21d-baseline/v2-a1` | 22 | `3a9536e719908d0a` | `6f6a41dda52fd761` |

What the example shows:

- The re-anchor after uses (`v1-a2`, h = 9) is a new `ANCHOR` subject. The manifest makes no transition, so it stays at `IN_USE` and becomes `ANCHORED` by derivation.
- Two uses in a row (8, 9) are `IN_USE -> IN_USE`. Both are bound to the run-start anchor `v1-a1`, recorded at 7, whose h = 6 covers the approval (6).
- One re-anchor (`v1-a3`, h = 12) covers two pins (11, 12) with no per-pin entry.
- The use at 14 consumes both pins; its run-start anchor `v1-a3`, recorded at 13, covers them.
- The new baseline version is recorded by 15-17 in the same log. Its approval (16) cites BA-11's three records and the `MANIFEST_CARRY_OVER` record of `CAP-1`, and the BA-07 seal hash is unchanged (the `baselineCustodyHash` of 0 and 15 are equal), so the manifest stays usable under v2.
- The carry-over block (18-20): `LOCK_MODE` is carried over as a new v2 pin with the same value, followed by its `SUPERSEDED` pair; the consumed `E04_ADMITTED_SET` pin is not carried over and is `REVOKED` (`SUPERSEDED_BY_RULING`). The first anchor of v2 has h = 20, the end of the block. It needs no restart and no new genesis.
- The use at 22 is bound to the run-start anchor `v2`, recorded at 21. It consumes only the v2 `LOCK_MODE` pin.

Derived states at the end: every subject is `ANCHORED` except the `ANCHOR_RECORDED` entry 23, which is `PENDING_REANCHOR`. Baseline v1's last event is `SUPERSEDED` and baseline v2's is `BASELINE_APPROVED`; the manifest's is `IN_USE`; the v1 `LOCK_MODE` pin's is `SUPERSEDED`, the v1 `E04_ADMITTED_SET` pin's is `REVOKED`, and the v2 `LOCK_MODE` pin's is `PIN_RECORDED` (live).

Verification model (the generator's model of 6.2, run on the example, on tampered variants and on legal and illegal variants):

| Case | Result | Codes and limits |
|---|---|---|
| honest, mandatory sequence (indexes 0-13; tags `v1`..`v1-a3`) | `PASS_WITH_DECLARED_LIMITS` | last anchored index 12; copy-confirmed index 12; pending [13]; step 6 `MATCH` |
| honest, with the continuation (indexes 0-23; six tags) | `PASS_WITH_DECLARED_LIMITS` | last anchored index 22; copy-confirmed index 22; pending [23]; step 6 `MATCH` |
| T1: the log truncated after index 2 and re-appended with a recomputed chain (the BA07-V3-02 scenario); `baseline-anchor.json` reverted at the verified revision | `VIOLATION` | step 3 `REWRITTEN_AT_OR_BEFORE_ANCHOR` (`v1-a1`), `TRUNCATED_BELOW_ANCHOR` (`v1-a2`, `v1-a3`, `v2`, `v2-a1`); step 4 `UNRECORDED_ANCHOR` (`v1-a1`, `v1-a2`, `v1-a3`, `v2`); step 5 `ANCHOR_RECORD_BASELINE_MISMATCH` (`v2`, `v2-a1`); status `LAST_ANCHOR_NOT_RECORDED` |
| T2: in the state after index 14 (tags `v1`..`v1-a3`), the pending `IN_USE` entry 14 is edited and its hash recomputed | `PASS_WITH_DECLARED_LIMITS` | pending [13, 14]: the edit is **not detected**; this is the declared window (section 1) |
| T3: tag `ct21d-baseline/v1-a1` deleted at the remote | `VIOLATION` | step 3 `TAG_SEQUENCE` (`v1-a2`, `v1-a3`); step 4 `ANCHOR_WITHOUT_TAG` (7); step 5 `USE_REFERENCE_INVALID` and `USE_BEFORE_ANCHOR` (8, 9: their run-start anchor is gone); step 6 `REMOTE_REWRITE_DETECTED` |
| T4a: the last tag (`v2-a1`) deleted at the remote and its pending entry 23 removed; copy designated, holder **not consulted** | `PASS_WITH_DECLARED_LIMITS` | step 6 `NOT_VERIFIABLE`; pending [21, 22]: **not detected** (section 1, remote rewrite) |
| T4b: as T4a, holder consulted | `VIOLATION` | step 6 `REMOTE_REWRITE_DETECTED` (`v2-a1`) |
| T5: anchoring window: tag `v2-a1` pushed, its `ANCHOR_RECORDED` entry (23) not yet appended, and the holder has not yet written it | `PASS_WITH_DECLARED_LIMITS` | statuses `LAST_ANCHOR_NOT_RECORDED`, `LAST_ANCHOR_NOT_IN_COPY`; last anchored index 22; copy-confirmed index 20; step 6 `MATCH` |
| T6 (R1-type): every `ct21d-baseline` tag deleted at the remote and the log reset to entries [0, 1] | `VIOLATION` | status `UNANCHORED`; step 6, with the designation read from the primary designation record, `REMOTE_REWRITE_DETECTED` |
| T7 (R2-type): tags re-created at the remote with anchor records that say `UNDESIGNATED`, over a forged log with another manifest | `VIOLATION` | step 6 `DESIGNATION_MISSING` (four tags), `REMOTE_REWRITE_DETECTED` |
| T8: after index 13, a second live `LOCK_MODE` pin (`supersedes` = the first, ratified and anchored) with no `SUPERSEDED` pair; two runs consume the old and the new pin | `VIOLATION` | step 5 `PIN_SLOT_CONFLICT` (14) |
| T9: carry-over block without the `REVOKED` entry of the v1 `E04_ADMITTED_SET` pin; a run under v2 consumes that pin | `VIOLATION` | step 5 `PIN_VERSION_MISMATCH` (21), `CARRY_OVER_MISSING` (the v1 E04 pin) |
| T10: after index 23, a pin recorded under v2 whose record names `v1`, and a pin for the slot `NOT-A-SLOT` | `VIOLATION` | step 5 `PIN_VERSION_MISMATCH` (24), `PIN_SLOT_UNKNOWN` (25) |
| T11 (legal): after index 13, `CAP-2` captured and approved, `CAP-1` `SUPERSEDED` and then `REVOKED` (cause `DEFECT`); `CAP-3` `AUTHORIZED` and then `REVOKED` with `subjectHash = null` | `PASS_WITH_DECLARED_LIMITS` | pending [22] |
| T12: after index 13, the E04 pin `REVOKED` (14), then a use (15) recorded before any anchor covers the revocation | `VIOLATION` | step 5 `USE_WITH_PENDING_CLOSURE` (15) |
| T13: after index 13, a run bound at its start to `v1-a2` (h = 9) consumes the pins recorded at 11 and 12; `v1-a3`, which covers them, was recorded (13) before the run's `IN_USE` entry (14) | `VIOLATION` | step 5 `PIN_USE_BEFORE_ANCHOR` (both pins) |
| T14: the revision whose log ends at index 8, with all six current tags, in `HISTORICAL` mode | `HISTORICAL_PASS_WITH_DECLARED_LIMITS` | `LATER_ANCHORS` `v1-a2`, `v1-a3`, `v2`, `v2-a1`; last anchored index 6; pending [7, 8]; step 6 `MATCH` |
| T14, the same bytes verified in `CURRENT` mode | `VIOLATION` | the expected codes of 6.2 **Result**: `TRUNCATED_BELOW_ANCHOR` (the four later anchors), `UNRECORDED_ANCHOR` (`v1-a2`, `v1-a3`, `v2`), `ANCHOR_RECORD_BASELINE_MISMATCH` (`v2`, `v2-a1`); status `LAST_ANCHOR_NOT_RECORDED` |
| T15 (legal): BA-07 changed at v2 (a new BA-07 seal hash with its own primary designation record); `CAP-1` `REVOKED` (`SUPERSEDED_BY_RULING`) in the block, `CAP-2` captured under v2 and used | `PASS_WITH_DECLARED_LIMITS` | pending [27, 28] |
| T16: BA-07 changed at v2, but `BASELINE_APPROVED(v2)` cites a `MANIFEST_CARRY_OVER` record for `CAP-1` | `VIOLATION` | step 5 `CARRY_OVER_INVALID` (16), `CARRY_OVER_MISSING` (`CAP-1`) |
| T17: the E04 pin consumed at 14 superseded within v1 (15, 16) | `VIOLATION` | step 5 `CONSUMED_PIN_SUPERSEDED` (15) |

## 13. Delta V7 (decisions 226)

Sections 1 to 12 keep their V6 numbers; V7 changes text in the header, section 1 (g) and the threat-model row, section 2 (bound-SHA row), 4.1 (prose only), 5.3 (use), 5.4, section 7 (U5, U6), section 10 (one row changed, one added) and section 11 (N6-1, the execution prerequisites, 11.1). The Delta V6 table (decisions sections 220 and 223) stays in the V6 file (blob `9cac4c6da20d9289c723698c96efb41f850f529e`), the Delta V5 table in the V5 file and the Delta V4 table in the V4 file. **Code facts:** BA-07 restates no code fact, and V7 re-derives none; no product SHA is re-cited except in the header line `BOUND_PRODUCT_SHA` (unchanged). The vector files and the hashes of 4.1 are unchanged (4.1). V7 applies decisions section 226 and nothing more; the V6 review evidence is `docs/automation/evidence/I-52-ct21d-relink-v6-architect-review.json`.

| Item | Decision | Fix (section / field) |
|---|---|---|
| AR7-03 (AQ-V6-01; with the finding BA07V6-01) | section 226 | **APPLIED.** 5.4 intro: the sentence "the BA-03 runner detects a delta and fails closed" is replaced by the reading of AR7-03: the fail-closed detection is AR5-12's; the BA-03 runner detects its own pairs; the detector of the union is the dedicated precondition verifier `docs/automation/evidence/I-52-ct21d-precondition-verifier.py`, an execution-preparation tool outside the BA-11 registry (no row, no edge), in the custody of this procedure, whose output the record cites. Header `FILES` (not a file of this artifact) and `DEPENDS_ON` (informative pointer, no edge); 1 (g); 2 (bound-SHA row: where its output is stored); 11 (execution prerequisites); 11.1 (ruled). AQ-V6-01 is ruled, so it is not added as a prerequisite |
| AR7-04 (AQ-V6-BA04-1) | section 226 | **APPLIED.** 7 U6 widened: every run on a build under test (`PRODUCT_BUILD` or `CHARACTERIZATION_BUILD`), governing or not; mandatory for the runs that feed a pin, a selection, the warm-up criterion or the inventory (`E4-LEARN`, `LK`, `HDM-CHAR`, `WU-DRY`, the trace dry runs); outside: synthetic runs (`NONE`) and the `FIXTURE_CONSTRUCTION_BUILD` (FXB-03). 1 (threat-model row: "for every run on a build under test"); 5.4 intro; 10 (U6 row); 11.1. The matching scope of `RUN_PREREQUISITES` in the scenario catalog is a BA-04 matter (informative pointer) |
| AR7-05 (AQ-V6-BA07-1) | section 226 | **APPLIED.** The reading of V6 5.4 stands, with no new field in `IN_USE` or in the tuple and no change of the vector. Its three text conditions: (1) the record hash, with its path, is cited in `RUN_START` (5.4 "Saved with the tuple", 7 U6; the same formula as U5 (b) and the catalog rule); (2) the run's evidence package carries a copy of the record or its `{path, sha256}` next to the canonical tuple record (5.4, 7 U6); (3) `runnerOutput` is agnostic of the detector: the output of the verifier of AR7-03 (5.4; its shape `{path, sha256}` is unchanged). Header `PREREQUISITES` (the ruling is no longer a prerequisite; the designation of the independent copy remains), 10 (U6 row), 11 N6-1, 11.1 and the closing block of section 11 (no open Architect question) |
| BA07V6-02 (minor; AR7-09) | section 226 | **APPLIED.** 5.4 `citedPairs`: the union of the precondition sets that each citing artifact declares (the `BOUND` sources of BA-03 section 2; the route rows of BA-06 section 2.1, precedents excluded; the W and C rows of BA-08 section 3.2, P rows excluded; informative pointers). New paragraph "What the control plane re-derives": the path set from `citingArtifacts` and `result` from `citedPairs` (blob equality, `null` = delta), refusing the run on any mismatch; 7 U6; a new row in section 10 |
| BA07V6-03 (minor; AR7-09) | section 226 | **APPLIED.** 5.4 `deltaProcessing`: the member `inventoryUnchanged` is replaced by `citedArtifactsUnchanged` (`true` only when the processing ruling records that no normative content of BA-03, BA-06 or BA-08 changes); U6 accepts `DELTA_PROCESSED` only when it is `true`; the AR5-12 wording for the BA-06 case is kept as the named example (5.4, paragraph after the table); 7 U6; 10 (U6 row). The record of 5.4 is outside the vector (4.1), so no vector change follows |
| BA07V6-04 (minor; AR7-09) | section 226 | **APPLIED.** One phrasing in 5.3 (use), 5.4 ("Saved with the tuple"), U5 and U6: the record hash, with its path, is cited in `RUN_START` (the run's first log record), sampled together with the BUILD_BOUND fields and the run-start anchor before library acquisition; the run's evidence package carries a copy of the record, or its `{path, sha256}`, next to the canonical tuple record. The V6 phrasing "records the record hash in the run log next to the run-start anchor" is removed |
| AR7-01, AR7-02, AR7-06 to AR7-08; the other minors of AR7-09 | section 226 | **NOT_APPLICABLE** to these bytes: they rule BA-06, V2.6, BA-08, BA-09, BA-10, BA-11 and BA-03. The mirror of AR7-03 in the BA-07 row of BA-11 section 3 is a BA-11 matter |
| vector | — | **UNCHANGED.** V7 changes no schema, event, role, transition or case that the vector covers; the V5 generator was re-run in round V7 and its output is byte-identical to the committed V5 output (SHA-256 `743302c72a901c2631c4bdbe46e2e6b755901c8427dc3d0af508474ed28dff38`); the 4.1 hashes are re-verified (4.1) |
| editorial | — | title, banner and header in the V7 form (`ARTIFACT_VERSION` = 7-DRAFT superseding the V6 blob; `DELTA RULING APPLIED` names section 226); the heading of 11.1 names the rulings applied; N6-1 names V7. The header line `AUTHORITY_CONTRACT` is kept as in V6 |
| correction pass (V7 critic) | AR7-04, AR7-05 | 5.4 `citingArtifacts` and `citedPairs` include the W-1 rows of BA-09 section 3.2 (object ids: blob or tree); the extension of the AR7-03 union to BA-09 is submitted to the Architect for confirmation (prerequisite before candidate); U6 defines the run-start anchor of a non-governing run as the latest published anchor when `RUN_START` is sampled (text only, no new field); 1 (g) updated |
