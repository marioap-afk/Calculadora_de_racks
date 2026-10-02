# I52Ct21dI1Protocol — I-1 observation protocol, family 2, artifact P2 (draft for review)

```text
STATUS      = DRAFT FOR ARCHITECT EXACT REVIEW. P2 EXECUTION = NOT AUTHORIZED. C2 = NOT WRITTEN. NEVER RUN ON A HOST. NOT REVIEWED.
FAMILY      = I1-F2 (tuple field i1ProtocolFamily)      ARTIFACT = P2 only
CLASS       = NON_GOVERNING_PROTOCOL_COMPATIBILITY_QUALIFICATION      ACTIVITY = H5      RunId = HGP-H5-<yyyymmddThhmmssZ>-<nn>
AUTHORITY   = Coordinator ruling "I-1 VERSIONING / P2 DESIGN GATE" (decisions sections 254 and 255); Architect micro-review AR11-01..08
              (docs/initiatives/I-52-ct21d-i1-protocol-versioning-review-v1.md); package-level consequences in
              docs/initiatives/I-52-ct21d-host-gate-execution-package-v3-erratum-1.md
I-1 v1      = UNCHANGED (history). eng/research/I52Ct21dHostFacts/protocol/I-1-observation-protocol.md   git blob 3a77ba62da77dfa2fc8bd2ffeb176a9454864193
                                    eng/research/I52Ct21dHostFacts/protocol/i1-os-observation.ps1         git blob a9081cff612b2290c35c86c0ded72f42c62b5e5a
RUN -01     = HGP-H1-20261001T225354Z-01: RUN_STATUS = INVALID, RETRY = NOT_AUTHORIZED, SEAL = HELD. Nothing here repairs or reinterprets it
GOVERNING   = FALSE      CT21D_AUTHORITY_BASELINE_READY = FALSE      CT21D_EXECUTION_READY = FALSE      CT21D_EXECUTION = NOT_AUTHORIZED
```

This folder is **new**. Nothing under `src/`, `tests/`, an existing `eng/` folder, the I-1 v1 files, package v3 or the decisions file was changed.

## 1. Purpose

P2 is the typed AutoLISP compatibility qualification that the Architect proposed (AR11-04) and the Coordinator ruled after run `HGP-H1-20261001T225354Z-01` stopped with
`; error: too many arguments`. It characterizes, on the designated machine and with no governed attribute value, the behaviours that I-1 v1 left `[HOST-TO-CONFIRM]`
(the signature of `open`, the encoding and terminator that `write-line` produces, `findfile` on absent and present paths, `strlen` characters versus bytes, the **shape** of
`vlax-product-key`), and it separates two explanations of the original error by an explicit control (a user function with v1's parameter shape called with two arguments).
It decides nothing about the cause; C2 (the governed capture text) is written only after an authorized P2 occurrence is reviewed.

## 2. Files

| File | Role |
|---|---|
| `I-1-observation-protocol-v2-P2.md` | the P2 text: classification, preconditions (none satisfied), delivery discipline, the closed allowlist table, interpretation of every outcome, status semantics, what P2 does not decide, the list of unverified constructs |
| `P2-ALLOWLIST.txt` | the same 19 expressions, one per line, no ID prefix, LF: the paste source (line N is P2-EN). A family test checks it equals the table of the P2 text. It is an addition to the list of artifacts requested, justified by the risk of transcribing from rendered Markdown |
| `schemas/ct21d.i1-p2.v1.json` | JSON Schema 2020-12 (closed) of the offline analysis record |
| `Analyze-I1P2.ps1` | PowerShell 7 offline analyzer: reads only `p2-a1.txt` and `p2-a2.txt` of a P2 evidence directory and an optional screen-results JSON, verifies the family against the ledger, prints the record as canonical JSON to stdout; writes nothing; fail-closed (exit 2) |
| `tests/Analyze-I1P2.Tests.ps1` | tests that run without AutoCAD on synthetic fixtures in a temp directory, with negative controls: `pwsh -NoProfile -File .\tests\Analyze-I1P2.Tests.ps1` |
| `HASHES-I1-V2.txt` | ledger: SHA-256 of the LF bytes of every file of the family except itself, `sha256sum -c` format |
| `.gitattributes` | `* -text`: the bytes of this folder are read as exact bytes (core.autocrlf=true in this repository), as in the sibling folder |
| `README.md` | this file |

No C2 file exists. No script of this folder can be loaded into AutoCAD; the AutoLISP is only text typed by the operator.

## 3. How the review and the authorization pin the bytes

1. The Architect's exact review and the independent source review (AR11-08) name the ledger: `HASHES-I1-V2.txt` with its own SHA-256 (the ledger does not list itself; its hash is the family identity).
2. The review records `sha256sum -c HASHES-I1-V2.txt` exit 0 on the reviewed tree and the git blob ids of the files. Any byte change after the review creates a new version of the family (I1-F2.1, ...), never an edit.
3. The Coordinator's per-occurrence authorization names `i1ProtocolFamily = I1-F2`, `i1ProtocolLedgerSha256 = <sha256 of HASHES-I1-V2.txt as reviewed>`, a concrete H5 RunId, the `sessionId`, the `expected-inputs`
   (path and sha256), and the substituted line P2-E01 with its SHA-256. The tuple record and the authorization template of package v3 gain the two protocol fields by the erratum.
4. The operator types only text that comes from a hashed file (`P2-ALLOWLIST.txt`, or the table of the P2 text), never from chat. `Analyze-I1P2.ps1` refuses to analyze unless every file of the family equals the ledger, and records the sha256 of the P2 text and of the ledger it ran against.

## 4. What is not claimed

- Nothing here was executed on a host. **The AutoLISP is unexecuted**; section 11 of the P2 text lists every construct whose behaviour could not be verified from repository facts or from the operator's screenshot.
- The analyzer and its tests prove nothing about AutoCAD. They check that the offline reading of file bytes and of the transcription is deterministic, closed and fail-closed.
- The analyzer cannot see tuple drift, processes, dialogs or writes outside the P2 evidence root; it decides no run status.
- The reading of run -01 is not changed: its cause is not established.
