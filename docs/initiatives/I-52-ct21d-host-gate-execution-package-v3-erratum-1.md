# I-52 — CT-21D Host-Gate Execution Package V3 — Erratum 1 (I-1 protocol versioning and the P2 compatibility qualification)

```text
DOCUMENT          = versioned erratum to package V3, drafted for the Coordinator's ruling (decisions sections 254 and 255; Architect micro-review AR11-06 and the
                    Coordinator's "I-1 VERSIONING / P2 DESIGN GATE" ruling). A NEW document: package V3 is NOT edited and stays byte-for-byte as it is
AMENDS            = docs/initiatives/I-52-ct21d-host-gate-execution-package-v3.md   (git blob 0c9e331ce6f69537df363668714edb3b91baba12 at HEAD 04692eb7)
                    and, through V3, package V2 sections 8.1 and 8.2. Package V1 and V2 are untouched history
BASIS             = docs/initiatives/I-52-ct21d-i1-protocol-versioning-review-v1.md  (sha256 8b99f30d650c2377da8b64977c9bcbc4dff06d601352af7c4d0a9733a2950735)
                    docs/automation/decisions/I-52.md sections 254 and 255 (run HGP-H1-20261001T225354Z-01, RUN_STATUS = INVALID)
ROLE              = IMPLEMENTER (documents only). Decides nothing, authorizes nothing, runs nothing
STATUS            = DRAFT FOR THE COORDINATOR'S RULING. It has no force until the Coordinator records it in a decisions entry
NOT               = a baseline artifact; not in the BA-11 registry; not a contract
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```

## 1. Amendments

Each amendment names the sentence of package V3 it changes (quoted exactly) and says what replaces it **for the scope stated**. Nothing outside that scope changes.

### E1. Activity H10 is the I-1 P2 compatibility qualification

- **Affects V3 section 2.3** (last paragraph): "Fixtures H5 / H6 / H7 and the I-7 conformance reader remain in a later gate (the PRODUCT_BUILD does not exist; `RACKMIRROR` is not in `src/`)."
  **V3 section 6**: "`HF-G2`; H5, H6, H7 (fixtures); I-7 conformance; I-8; the PRODUCT_BUILD; `RACKMIRROR`; product seams."
  **V3 section 8**: "LATER GATE                       = H5, H6, H7, I-7 (PRODUCT_BUILD absent); HF-G2 NOT OPENED" (the status line, with its alignment spaces).
- **Amendment.** In RunIds, the activity number **H10** is the I-1 P2 compatibility qualification (protocol family 2, artifact P2). Future RunIds have the form
  `HGP-H10-<yyyymmddThhmmssZ>-<nn>`, pattern `^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$` (V3 section 2.2 form, `n` = 10). Classification: `NON_GOVERNING_PROTOCOL_COMPATIBILITY_QUALIFICATION`. Evidence root
  `D:/I52-CT21D-HOST/evidence/<H10-RunId>` (`NeedScratch = false`, `NeedCopy = false`), created by the CAD manager per part 2 of the governed ACL plan.
  No existing activity is renumbered, and no activity number is reserved for the later governed text C2.
  As ruled by the Coordinator, P2 falls within the sponsored host-preparation direction of the authorized non-governing gate; this erratum records that ruling and does not widen the gate.
  P2 is **not an S1-A retry**, not governing, not product evidence; it has no RackCad, no `NETLOAD`, no `APPLOAD`, and loads no LISP from a file. **P2 execution is NOT authorized.**
- **Consequence for the sentences quoted above: none.** The fixture activities H5, H6 and H7 are **not renumbered**; the sentences keep their meaning and their numbers for the later gate (not opened). `OI-E1` is resolved by the Coordinator: H5 is unchanged.

### E2. A P2 compatibility mismatch is a characterization fact, not `INVALID`

- **Affects V3 section 5.1** ("The operator stops, writes the attempt-log line and informs the Coordinator. No step is "fixed on the fly". Package V2 8.1 items 1 to 13 stand."), in particular V2 8.1 item 6
  "An instrument reports an error or an unexpected class/property." and item 7 "The operator deviates from the written steps."; **V3 section 5.2** ("a failed host-run check or any item of the INVALID list is `INVALID` for every fact of the run, all evidence retained");
  and the V2 8.2 row "a valid `UNKNOWN` or `OBSERVED_DIFFERS` | recorded; **never repeated to obtain a better result**".
- **Amendment, for activity H10 (P2) only.** A result of an allowlisted P2 expression that differs from an I-1 v1 `[HOST-TO-CONFIRM]` assumption (including a host error text printed by that expression) is a
  **characterization result**, recorded as `OBSERVED_DIFFERS`, with the host's text verbatim. It is not an item 6 "instrument error" and not an item 7 deviation, and by itself it is not `INVALID`. It is not repeated to obtain a better result.
  An error printed by a definition (`defun`) expression or by a P2 helper is a **recorded protocol result** (`HOST_ERROR` with category `DEFINITION` or `HELPER` in the P2 closed result model), **not** a v1 compatibility mismatch; the items that depend on it are `NOT_OBSERVED`.
  `INVALID` for an H10 run is limited to the stop conditions: tuple drift; another `acad.exe`; an unscripted dialog or prompt; a write outside the P2 evidence root; an expression outside the allowlist (or text that differs from the hashed line); an operator deviation
  (an `OPERATOR_DEVIATION` of the P2 text, section 4 item 5, including a P2-E01 that does not echo the authorized root); a crash
  (the Coordinator rules `CRASH_FINDING` / `CRASH_ENVIRONMENT`). A `REFUSED` result (the create-new guard) is a stop that the Coordinator rules. In an `INVALID` run, facts not read stay `UNKNOWN`; evidence is retained.

### E3. A C2 mismatch is a stop: `INVALID`, unread facts `UNKNOWN`

- **Affects** the same V3 sections 5.1 and 5.2.
- **Amendment, for the later governed text C2 of protocol family 2.** C2 will use only forms that an authorized and reviewed P2 occurrence showed to work, and it will carry no `[HOST-TO-CONFIRM]` item in its write path.
  For C2 the reviewed behaviour is **normative**: any mismatch of the host with it is a stop condition (V2 8.1 item 6), the run is `INVALID`, and the facts C2 could not read stay `UNKNOWN` inside the `INVALID` run.
  A future C2 mismatch against the reviewed P2 behaviour is therefore a **STOP**: the run is `INVALID`. There is **no silent fallback and no manual normalization** (no alternative form, no hand-edited output, no re-typing with another form).

### E4. Run HGP-H1-20261001T225354Z-01 stays as ruled

- **Affects V3 section 5.3** ("No attempt after an `INVALID` or a crash begins without the Coordinator's written authorization for that occurrence.") and the recorded S1-A outcome.
- **Amendment: none to the run.** The run stays immutable historical evidence: `RUN_STATUS = INVALID`; `A1-R3..R6 = UNKNOWN`; `A1-R1/R2 = NOT_OBSERVED`; `HF-M7 = UNSET`; `RETRY = NOT_AUTHORIZED`; `SEAL = HELD`; the operator's screenshot
  (sha256 `9837dcd08445a3cc53954dbf91583a0485d76414e0e03d67a17c14d90b089818`) is its `RAW_COMMAND_LINE_EVIDENCE`. It is never reinterpreted, repaired or re-run under this erratum, and the cause of its error is not established. Run H1 remains immutably `INVALID`; this erratum authorizes **no retry** of it.

### E5. New protocol versions are new artifact families; two tuple fields

- **Affects V3 section 2.3, S1-A** ("Protocol: package V2 section 7.5 (script `protocol/i1-os-observation.ps1`, protocol `protocol/I-1-observation-protocol.md`, hashes in 3.3).");
  **V3 section 3.5.3** (tuple record PHASE 1); **V3 section 4** (the authorization template).
- **Amendment.** I-1 v1 (git blobs `3a77ba62da77dfa2fc8bd2ffeb176a9454864193` and `a9081cff612b2290c35c86c0ded72f42c62b5e5a`) is history and is never edited. Every new version of the I-1 protocol is a **new artifact family** in its own folder with a ledger
  (`HASHES-I1-V2.txt` for family 2: sha256 of the LF bytes of every file of the family, `sha256sum -c` format). A byte change creates a new version of the family.
  The tuple record (PHASE 1) and the authorization template **gain two fields** wherever an I-1 protocol text is used:

  ```text
  i1ProtocolFamily            = <I1-F2 | EMPTY for a session that uses no I-1 protocol>
  i1ProtocolLedgerSha256      = <sha256 of the family ledger as ruled | EMPTY>
  ```

  In the tuple record they follow `rackcadBuild`; in the authorization template they follow `packageReference`. The authorization names both; the operator types only text that comes from a hashed file of that family, never from chat.
  For any S1-A occurrence after this erratum, "the I-1 protocol" of the sentence quoted above means the family named in the authorization (v1 only as history). The scripts of the phase-2 capture, `tools label` / `seal`, the declared set,
  the evidence layout and the TRUSTEDPATHS baseline are not changed by this erratum.

### E6. Required sequence

```text
1  draft P2 (family I1-F2: P2 text, allowlist, schema, offline analyzer, tests, ledger)          <- done as a draft by the implementer; NOT reviewed
2  independent Architect and source review of the exact bytes (AR11-08); the review names HASHES-I1-V2.txt and its SHA-256
3  Coordinator's per-occurrence written authorization of P2 (concrete H10 RunId, sessionId, expected-inputs, i1ProtocolFamily, i1ProtocolLedgerSha256, cause = tooling/protocol) and the CAD-manager pre-host attestation (E7)
4  execute P2 (CAD manager, designated machine)
5  evidence review of the P2 outcome (files, screenshots, analysis record)
6  draft C2 from the reviewed behaviour
7  Architect review of C2
8  a new H1 attempt only if separately authorized, with a new RunId, sessionId, empty evidence child, expected-inputs, and zero acad.exe
```

No step is implied by the one before it; each needs its own record.

### E7. Phase-2 capture is waived for P2; a CAD-manager pre-host attestation replaces it

- **Affects V3 section 5.1, stop condition 16**: "Phase 2 is incomplete or not validated when the first probe or characterization command is about to be typed."
- **Amendment, for activity H10 (P2) only.** By the Coordinator's ruling `PHASE2_CAPTURE_FOR_P2 = WAIVED`, stop condition 16 does not apply to P2, because P2 is `NON_GOVERNING_PROTOCOL_COMPATIBILITY_QUALIFICATION` and captures no governed S1-A product or session facts.
  In its place the CAD manager delivers, before the host is touched, a **CAD-manager pre-host attestation** (with a fill-in template in the P2 text, section 3.1): the exact designated machine; the expected AutoCAD build and profile from the machine-class record;
  `acad.exe` process count 0 before startup; the exact `TRUSTEDPATHS` observed-baseline match (sha256 `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498`, 19 entries); the exact P2 protocol, allowlist, schema, analyzer and ledger hashes; a fresh, empty H10 evidence child
  with the governed ACL (part 2 of the governed ACL plan; `NeedScratch = false`, `NeedCopy = false`) created by the CAD manager; one fresh AutoCAD process; no RackCad; no `NETLOAD`. The evidence child is therefore empty at P2 start (no phase-2 record files); the offline analyzer tolerates other entries anyway.
  The waiver and the attestation are specific to P2 and to no other session.

## 2. What this erratum does not change

Package V3 sections not quoted above; the Owner's discipline (V2 1.1, V3 1.1 to 1.3); the no-automatic-retry rule (V3 5.3); the stop conditions 14 to 21 of V3 5.1 (they apply to every session that uses them, except condition 16 for P2 as stated in E7); the list of what stays not authorized (V3 section 6), whose fixture numbers H5, H6 and H7 are not renumbered;
`PARAM-04`, `EVIDENCE_REPETITION`; any governing or admission matter.

## 3. Open for the Coordinator

| Id | Item |
|---|---|
| OI-E1 | RESOLVED by the Coordinator: the fixtures activity keeps H5 (V3 2.3, 6, 8); P2 takes H10 |
| OI-E2 | whether this erratum is recorded in a decisions entry as in force, and whether V3's section 8 status line needs a v4 or only this erratum |
| OI-E3 | custody and sealing of the H10 evidence root and of the offline analysis record (not decided by P2) |

## 4. Status

```text
P2_EXECUTION            = NOT_AUTHORIZED
C2_TEXT                 = NOT_WRITTEN
S1_A_RETRY              = NOT_AUTHORIZED
NEW_H1_RUN_ID           = NOT_AUTHORIZED
H10_RUN_ID              = NOT_ASSIGNED
PHASE2_CAPTURE_FOR_P2   = WAIVED (Coordinator ruling), replaced by the CAD-manager pre-host attestation (not delivered)
I1_PROTOCOL_V1          = UNCHANGED
PACKAGE_V3              = UNCHANGED (this erratum is a separate document)
RUN HGP-H1-20261001T225354Z-01: RUN_STATUS = INVALID     RETRY = NOT_AUTHORIZED     SEAL = HELD
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED     HOST_RUN_STARTED = TRUE (S1-A, INVALID)
G3 = STOPPED     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     SCOPE = NONE     FALLBACK = ALT-21E
```
