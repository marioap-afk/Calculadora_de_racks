# I-52 — CT-21D Baseline Artifact BA-07: Manifest and Baseline Custody Procedure (DRAFT V3)

> **BASELINE ARTIFACT BA-07 V3 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized. No manifest exists, no custody log exists, and no anchor has been created.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-MANIFEST-CUSTODY
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob 4acdfc4313c03334d7409e4a78174ecbd03dcfb3, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V3 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 29 + V2.2 PA-13 + V2.3 + V2.4
> PHASE-2 RULING APPLIED    = BA07-01 (verification at the anchored index), BA07-02 (baseline approval entry, AR2-39), BA07-03 (IN_USE and REVOKED fields),
>                             BA07-04 (manifest comparison), BA07-05 (remote-rewrite limit, AR2-39), BA07-06 (reference of record), BA07-07 (locations),
>                             AR2-16/AR2-17 (pin records), AR2-44 (BA-11's own hashes live here), V24-N2 (execution-phase review records)
> BLOCKER                   = NB-6  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until this procedure is sealed and the baseline anchor is published)
> MANIFEST_CAPTURED         = NO (execution prerequisite EXEC-11)      ANCHOR_CREATED = NO (placeholder only, section 9)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose and threat model

This procedure gives audit integrity to: (a) the **baseline record** (the final hash registry BA-11 and this procedure), anchored once when the baseline is complete; (b) the **pin records** (AR2-16, AR2-17), written after the baseline and before the first governing run that uses them; (c) the **manifest** of loaded code (E-10, E-11M, E-13), captured later for the exact build and machine (EXEC-11); (d) the **execution-phase review records** of V2.4 note `V24-N2` (the `REVIEWED` inventory record and any reopened operation-class review). The manifest attests the enumerable known set only and proves **no isolation** (`NG-13..NG-18`).

| Property | Statement |
|---|---|
| **tamper evidence** | **yes**: accidental change, unrecorded change and rewriting of the custody chain are detectable against the published anchor |
| **non-repudiation and signer identity** | **no**: there is no cryptographic signature and no proof of who acted |
| out of the threat model | a hostile local or repository administrator (`NG-13`); self-review by one person is a declared limit (section 7.1) |
| **declared limitation (remote rewrite)** | branches and tags at the remote can be rewritten or deleted by an account with push rights. A rewrite of the anchor commit or tag is detectable **only** by comparison with an **independent copy** of the anchor identity kept outside the repository and outside the rewritten references (section 2 and section 9, step 6). **Until the Coordinator designates that copy (location and holder), a remote rewrite is not detectable.** The decisions log and any file inside the same repository are **not** an independent copy |

## 2. What is stored where (designated locations)

| Item | Location | In the repository? |
|---|---|---|
| baseline artifacts and their files | the paths listed in the **BA-11 registry** (the registry is the authority for the location of every baseline artifact) | yes |
| custody log (append-only, section 6) | `docs/automation/evidence/ct21d/custody-log.jsonl` (**planned path; not created by this artifact**) | yes (it carries hashes and roles, no machine data) |
| baseline anchor record (section 9) | `docs/automation/evidence/ct21d/baseline-anchor.json` (**planned path; not created**) | yes |
| ratification and candidate records (including BA-11's own candidate hash, ratifications and seal hash, which are never written into BA-11) | `docs/automation/evidence/ct21d/ratifications/<artifact>-<hash>.json` (**planned**) | yes |
| pin records (AR2-16, AR2-17) | `docs/automation/evidence/ct21d/pins/<pinKind>-<hash>.json` (**planned**); each is also a BA-11 entry | yes (hashes and values; machine-profile values stay local) |
| manifest, build layer | `docs/automation/evidence/ct21d/manifest-build-<hash>.json` (planned) | yes (the layer and its hash) |
| manifest, machine-profile layer | local evidence store `%LOCALAPPDATA%\RackCad\ct21d-evidence\manifest-machine-<hash>.json` on the characterization machine (planned) | **no**: only its hash enters the repository |
| session-evaluated snapshots | run evidence packages (V2.1 18.2) | per the evidence policy of the run |
| **independent copy of the anchor identity** | **UNDESIGNATED**: an out-of-repository location and a holder, designated by the Coordinator in the candidate ruling of this artifact; written at anchoring time (section 9, step 6) | **no** |

The Coordinator designates these locations by ratifying this artifact. A change of location is a new version of this artifact.

## 3. Manifest capture (control plane; not executed here)

1. The Coordinator **authorizes** the capture (`AUTHORIZED` custody entry).
2. The **control plane** starts a fresh controlled session on the exact build, machine and profile.
3. The control plane runs the **warm-up** (BA-09 V3) and then the E-10 enumerations and the hasher; the manifest is **generated by the control plane** and is **never hand-edited**.
4. The control plane serializes the manifest canonically (section 4) and computes its SHA-256 (the **manifest identity**).
5. The manifest layers are written to the designated locations (section 2), read-only.
6. The control plane writes a `CAPTURED` custody entry.

## 4. Canonical serialization and hash

| Aspect | Rule |
|---|---|
| format | JSON, UTF-8, LF line endings, no BOM |
| keys | sorted in ordinal (UTF-8 byte) order |
| numbers | integers in decimal; doubles as in BA-05 V3 section 2.1 if any occur |
| arrays | in the declared order of the schema (module lists sorted by normalized full path, ordinal) |
| hash | SHA-256 of the serialized bytes, lowercase hexadecimal |
| association with the tuple | the manifest hash is a tuple field. A run's recorded manifest hash is compared with the manifest hash that is `APPROVED` and `ANCHORED` in the custody log for that tuple (the one the run records `IN_USE`); a difference makes the run **`INVALID`** (V2.1 29.1; phase-1 ruling G-2). There is no manifest at the baseline (section 8) |

## 5. Storage, retrieval and versioning

| Aspect | Rule |
|---|---|
| storage | content-addressed and read-only: `<kind>-<hash>.json` at the designated location |
| retrieval | by hash; the hash is **verified on read**; a mismatch refuses use |
| amendment and versioning | a manifest, a pin record or a baseline record is **never edited**: a change is a **new capture, a new pin version or a new baseline version** with a new hash and a `supersedes: <hash>` link; superseded items are retained |

## 6. Hash-chained custody log

An **append-only** JSON-lines log. The **genesis entry** (index 0) of the CT-21D custody log is the **baseline record** (`BASELINE_RECORDED`, section 8), followed by the baseline approval (`BASELINE_APPROVED`). Pin, manifest and execution-record events follow them.

### 6.1 Entry (normative fields)

| Field | Content |
|---|---|
| `index` | integer, 0 for the genesis entry |
| `utc` | UTC time of the entry |
| `role` | `CAPTURE_ROLE`, `REVIEW_ROLE`, `APPROVAL_ROLE` or `CUSTODY_ROLE` |
| `actor` | a person identifier or `CONTROL_PLANE:<build id>` |
| `event` | `BASELINE_RECORDED`, `BASELINE_APPROVED`, `PIN_RECORDED`, `REVIEW_RECORD_ADDED`, `AUTHORIZED`, `CAPTURED`, `REVIEWED`, `APPROVED`, `ANCHORED`, `IN_USE`, `SUPERSEDED`, `REVOKED` |
| `subjectKind` | `BASELINE`, `PIN`, `REVIEW_RECORD` or `MANIFEST` |
| `subjectHash` | `BASELINE`: the BA-11 seal hash; `PIN`: the pin-record hash; `REVIEW_RECORD`: the hash of the record; `MANIFEST`: the manifest identity |
| `baselineCustodyHash` | only in `BASELINE_RECORDED`: the BA-07 seal hash (this procedure); otherwise `null` |
| `ratificationRecords` | only in `BASELINE_APPROVED`: the paths and SHA-256 of the Coordinator and Architect ratification records of BA-11 (its candidate hash, both ratifications and its seal hash); otherwise `null` |
| `pinSlot` | only in `PIN_RECORDED`: the slot the record fills (for example `E04_ADMITTED_SET@R-1`); otherwise `null` |
| `runId` | only in `IN_USE`: the run id of the run that uses the manifest; otherwise `null` |
| `tupleRecordHash` | only in `IN_USE`: SHA-256 of the canonical tuple record of that run (section 4 serialization); `MACHINE_PROFILE_BOUND` values stay in local evidence; otherwise `null` |
| `cause` | only in `REVOKED`: one of `TOOLING`, `ENVIRONMENT`, `CONTAMINATION`, `DEFECT`, `SUPERSEDED_BY_RULING`, `OTHER`, with the explanation in `note`; otherwise `null` |
| `prevEntryHash` | the `entryHash` of the previous entry; the genesis entry uses 64 zero digits |
| `anchorRef` | for `ANCHORED` entries: remote, reference of record (the annotated tag), tag object id, peeled commit SHA, anchored index; otherwise `null` |
| `note` | free text, may be empty |
| `entryHash` | SHA-256 of the canonical serialization (section 4) of **all other fields** of the entry |

### 6.2 Verification algorithm

1. Recompute every `entryHash`; a difference is a **broken entry**.
2. Check each `prevEntryHash` against the preceding entry; a difference is a **broken chain**.
3. For the **last anchor** (the anchor record, or the latest `ANCHORED` entry): take its `CUSTODY_LOG_HEAD_INDEX` = `h` and its `MANIFEST_CUSTODY_HEADER_HASH`; the `entryHash` of the entry **at index `h`** must equal it; a difference means the log changed at or before the anchored head. The comparison is **never** made against the current head.
4. Entries with an index greater than `h` are **`PENDING_REANCHOR`**, not failures. They may only be: the `ANCHORED` entry that records the anchor of `h` (it necessarily follows `h`, because the anchor identity is known only after the tag exists), and later custody events (`PIN_RECORDED`, `REVIEW_RECORD_ADDED`, manifest events, `IN_USE`, `SUPERSEDED`, `REVOKED`). Every pending entry must be covered by the **next** anchor (section 9, step 7); an entry that a later anchor does not cover is a **violation**.
5. Check the state machine of section 7 for every subject; an illegal transition is a **violation**.
6. Compare the anchor identity (tag object id and its peeled commit SHA) with the **independent copy** (section 2); a difference means the remote references were rewritten. While the copy is undesignated this step reports **`NOT_VERIFIABLE`** (section 1).

## 7. Roles and transitions

| Role | Holds | May not |
|---|---|---|
| `CAPTURE_ROLE` | runs capture **through the control plane** | approve its own capture |
| `REVIEW_ROLE` | the Owner or CAD manager (or a delegate they record): checks the machine-layer entries and the build layer against the build receipt | capture or approve |
| `APPROVAL_ROLE` | the Coordinator: approves the baseline (`BASELINE_APPROVED`) and a manifest hash (`APPROVED`) | capture |
| `CUSTODY_ROLE` | **the Coordinator logical role by default** (designated by this artifact; a change is a new version): keeps the custody log and the anchor | alter an entry already anchored |

```text
BASELINE:       RECORDED (genesis, BASELINE_RECORDED, CUSTODY_ROLE) -> APPROVED (BASELINE_APPROVED, APPROVAL_ROLE) -> ANCHORED -> [SUPERSEDED by a new baseline version]
PIN:            RECORDED (PIN_RECORDED, APPROVAL_ROLE, after the pinned value is frozen from its own evidence) -> ANCHORED -> [SUPERSEDED by a new pin version]
REVIEW_RECORD:  ADDED (REVIEW_RECORD_ADDED, CUSTODY_ROLE) -> ANCHORED
MANIFEST:       AUTHORIZED -> CAPTURED -> REVIEWED -> APPROVED -> ANCHORED -> IN_USE -> SUPERSEDED
                any state -> REVOKED (with a recorded cause)
```

- **`IN_USE`** is recorded **at each use**, with the run id and the tuple record hash (V2.1 29.3). A manifest that is not `ANCHORED` cannot be `IN_USE`. `SUPERSEDED` and `REVOKED` never return to `IN_USE`.
- A pin that is not `ANCHORED` cannot be used by a governing run.
- The `BASELINE_APPROVED` and `APPROVED` transitions and the `ANCHORED` transitions are **separate log entries** even when the same person holds the approval and the custody roles (phase-1 ruling item (11); V2.2 PA-13).

### 7.1 Multi-role policy (`YES_WITH_ROLE_SEPARATION_IN_LOG`)

One human may hold several logical roles. Requirements: each role transition is a separate log entry; the actor is recorded in every entry; capture is always generated by the control plane; the role multiplicity is disclosed in Owner Act 2 (count of entries by actor and role). Self-review is a declared limit.

## 8. The baseline genesis and approval records (no manifest at baseline)

At the baseline there is **no execution manifest** (it is captured later, EXEC-11). The first two entries are:

| Field | Index 0 | Index 1 |
|---|---|---|
| `event` | `BASELINE_RECORDED` | `BASELINE_APPROVED` |
| `subjectKind` | `BASELINE` | `BASELINE` |
| `subjectHash` | the **seal hash of the final BA-11** (the hash registry, which binds every other artifact) | the same |
| `baselineCustodyHash` | the **seal hash of this BA-07** | `null` |
| `ratificationRecords` | `null` | the ratification records of BA-11 (its candidate hash, the Coordinator and Architect ratifications and its seal hash; these are **never** written into BA-11) |
| `role` / `actor` | `CUSTODY_ROLE` / the Coordinator | `APPROVAL_ROLE` / the Coordinator |
| `prevEntryHash` | 64 zero digits | the `entryHash` of index 0 |

Pins, execution review records and the manifest, when they exist, are appended with their own events and are **re-anchored** (section 9, step 7).

## 9. External anchor (procedure; nothing is created here)

The anchor is created **only after**: every baseline artifact is finalized and sealed; BA-11 is complete (the seal hash and the ratification records of **every other** artifact; BA-11's own candidate hash, ratifications and seal hash are recorded in section 2's ratification records and in the entries of section 8, never inside BA-11); every ratification is recorded. **A final anchor is never created earlier.**

| Step | Rule |
|---|---|
| 1 | the `CUSTODY_ROLE` appends the genesis entry and the `APPROVAL_ROLE` appends the approval entry (section 8); `MANIFEST_CUSTODY_HEADER_HASH` = the `entryHash` of the head entry (index 1) and `CUSTODY_LOG_HEAD_INDEX` = 1 |
| 2 | it writes the anchor record `docs/automation/evidence/ct21d/baseline-anchor.json` with: `MANIFEST_CUSTODY_HEADER_HASH`, `CUSTODY_LOG_HEAD_INDEX`, the BA-11 seal hash, the BA-07 seal hash, the contract composite hash |
| 3 | the anchor record and the custody log are committed in a **reviewed** commit |
| 4 | the commit is **pushed** to the remote `origin` (`https://github.com/marioap-afk/Calculadora_de_racks.git`) and its presence at the remote is **verified** (`git ls-remote`) |
| 5 | an **annotated tag** `ct21d-baseline/<baseline version>` is created on that commit and pushed; the tag object id is verified at the remote. **The annotated tag is the reference of record**: feature branches may be rebased under the repository workflow, so verification uses the **tag's peeled commit**, never branch ancestry |
| 6 | the anchor identity (tag name, tag object id, peeled commit SHA, anchored index, remote URL, UTC time) is recorded in an `ANCHORED` custody entry **and** written to the **independent copy** (section 2) by its designated holder; until that copy is designated, the limitation of section 1 applies and step 6 of 6.2 reports `NOT_VERIFIABLE` |
| 7 | every later custody event (pin, execution review record, manifest capture, use, supersession, revocation) is re-anchored in the next evidence commit with the same procedure (a new anchor record version and a new tag `ct21d-baseline/<version>-a<n>`) |

Placeholder of the anchor record (**template only; all values `UNSET`**):

```text
MANIFEST_CUSTODY_HEADER_HASH = UNSET
CUSTODY_LOG_HEAD_INDEX       = UNSET
BA11_SEAL_HASH               = UNSET
BA07_SEAL_HASH               = UNSET
CONTRACT_COMPOSITE_HASH      = UNSET
REMOTE_URL                   = https://github.com/marioap-afk/Calculadora_de_racks.git (designated)
ANCHOR_TAG_NAME              = ct21d-baseline/<version> (designated pattern; reference of record)
ANCHOR_TAG_OBJECT_ID         = UNSET
ANCHOR_PEELED_COMMIT_SHA     = UNSET
INDEPENDENT_COPY             = UNDESIGNATED
VERIFIED_AT_UTC              = UNSET
VERIFIED_BY                  = UNSET
```

## 10. Audit integrity: what each check protects

| Change | Detected by |
|---|---|
| an artifact, pin record or manifest edited in place | the hash is verified on read against BA-11 or the custody log |
| an entry edited in the custody log | recomputation of `entryHash` (6.2 step 1) |
| an entry removed or reordered | chain check (6.2 step 2) |
| the log rewritten at or before the anchored head, with a recomputed chain | comparison of the entry **at the anchored index** with the anchored header hash (6.2 step 3) |
| an entry appended after the anchor and never re-anchored | coverage check of pending entries (6.2 step 4) |
| the anchor tag or commit rewritten at the remote | comparison with the independent copy (6.2 step 6); **not detectable until the copy is designated** |
| an illegal role transition | state-machine check (6.2 step 5) |
| a manifest used without approval or anchor | `IN_USE` requires `ANCHORED` |

## 11. What remains before NB-6 can close

| Id | Item | Gate |
|---|---|---|
| N6-1 | Architect ruling `CANDIDATE_FOR_SEALING` on the exact bytes of this V3 (with the Coordinator's designation of the independent copy), then Coordinator and Architect ratification, then seal | baseline preparation (no host) |
| N6-2 | the baseline anchor published (section 9) | the **last** step of the baseline, after BA-11 is final |

The captured manifest, the pin records and their anchoring are **execution prerequisites** (EXEC-11 and the pins of BA-11) and do not block the baseline.

```text
NB-6 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until this procedure is sealed and the baseline anchor is published
MANIFEST = NOT CAPTURED ; CUSTODY LOG = NOT CREATED ; ANCHOR = NOT CREATED ; INDEPENDENT COPY = UNDESIGNATED
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
