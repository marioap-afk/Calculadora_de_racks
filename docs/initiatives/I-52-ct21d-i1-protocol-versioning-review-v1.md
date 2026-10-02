# I-52 — CT-21D: Architect micro-review of the I-1 protocol versioning after the S1-A stop

```text
DOCUMENT              = Architect micro-review requested by the operator (CAD manager) after run HGP-H1-20261001T225354Z-01 (decisions section 254)
STATUS                = REVIEW + PROPOSED VERSIONING RULES FOR THE COORDINATOR. NO PROTOCOL TEXT IS CHANGED BY THIS DOCUMENT. NO RUN IS AUTHORIZED BY IT.
SUBJECT               = how the I-1 protocol (eng/research/I52Ct21dHostFacts/protocol/I-1-observation-protocol.md and i1-os-observation.ps1) must be versioned to handle the observed incompatibility in a governed way
NOT IN SCOPE          = reinterpreting, repairing or re-running HGP-H1-20261001T225354Z-01. Its status stays RUN_STATUS = INVALID; A1-R3..R6 UNKNOWN; A1-R1/R2 NOT_OBSERVED; HF-M7 UNSET; RETRY = NOT_AUTHORIZED; SEAL = HELD
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED     HOST_RUN_STARTED = TRUE (S1-A, INVALID)
```

## 1. The observation and its custody

The operator closed the AutoCAD session by hand after the error (`SESSION_TERMINATION = NORMAL_MANUAL_CLOSE`; `CRASH_FINDING = NO`). The only surviving record of the command line is the operator's **screenshot**, which shows (operator's list): the echoes `CT21D-PUT` and `CT21D-ATTR`, the `fboundp` error, the `ct21d-attr` attempts, `PASTECLIP`, and `; error: too many arguments`. No other transcript exists and none will be reconstructed.

Custody rules (rulings AR11-01 and AR11-02 below): the screenshot is retained as the raw evidence of this attempt; it is hashed (SHA-256 of the file bytes as it exists), its path and hash are recorded by the operator in the attempt log, and it is **not edited, cropped or annotated** (an annotated copy, if wanted, is a separate file). The screenshot is operator-supplied evidence, not an instrument output.

## 2. What the observation establishes and what it does not

| Statement | Status |
|---|---|
| On this machine, profile and AutoCAD build (2025, R25.0.171.0.0), the typed v1 sequence raised `; error: too many arguments` when the governed attribute calls were evaluated | ESTABLISHED by the operator's screenshot (as described) |
| The failing function is `open` with the encoding argument | **NOT ESTABLISHED.** The message does not name the function. It is the operator's reading |
| `open` with three arguments is unsupported on this host | **NOT ESTABLISHED** |
| The definitions of `ct21d-put` / `ct21d-attr` on the host were byte-for-byte the protocol text | **NOT ESTABLISHED.** A `too many arguments` error is also what a user function raises when it is called with more arguments than its parameter list; a clipboard mix-up (the run also shows an accidental `PASTECLIP`) could have changed a definition. Without the transcript this cannot be excluded |
| The failure is a defect of the protocol text | NOT ESTABLISHED; it is a **HOST-TO-CONFIRM mismatch** in the protocol's own words: "the raw output of the designated machine governs, and a mismatch makes the attribute `UNKNOWN`, never a corrected guess" |

Consequence for versioning: the new version **must not be built on a guessed cause.** It must either establish the host behaviour beforehand by an observation of its own, or contain no construct whose behaviour is HOST-TO-CONFIRM in the write path.

## 3. Rulings proposed to the Coordinator

**AR11-01 — v1 is immutable history.** `I-1-observation-protocol.md` (git blob `3a77ba62da77dfa2fc8bd2ffeb176a9454864193`, identical at `642ba392` and at the current tip) and `i1-os-observation.ps1` (git blob `a9081cff612b2290c35c86c0ded72f42c62b5e5a`) stay byte-for-byte as they are. They remain the text under which run `-01` was executed and classified. No edit "to fix" them, ever; their hashes stay in the ledgers that cite them.

**AR11-02 — Screenshot custody.** The operator gives the path of the screenshot; the assistant records its SHA-256 (read-only) in the S1-A attempt log (decisions) and the operator keeps the file outside the repository with the other raw evidence of the run. It is cited as `RAW_COMMAND_LINE_EVIDENCE` of run `-01`. Absence of a transcript is a recorded limit, not a reason to reconstruct.

**AR11-03 — A new version is a new artifact family, never an edit.** The next I-1 text is **protocol family 2**: new files named `I-1-observation-protocol-v2-*.md` (and a ledger `HASHES-I1-V2.txt` with the SHA-256 of every file of the family, LF bytes). The family id and its ledger hash are **tuple inputs**: the tuple record gains `i1ProtocolFamily` and `i1ProtocolLedgerSha256`; the authorization names both; the operator types only text that comes from a hashed file (never from chat); any byte change creates a new version (family 2.1…). This is the same rule the rest of the package uses ("changing the activity, the package or an instrument is a new version, not a retry", V2.1 21.3).

**AR11-04 — Two artifacts in family 2, in this order (no fallback ladders).**
1. **P2, the compatibility rehearsal text.** A short typed sequence that establishes, on the designated machine, the behaviours that v1 left HOST-TO-CONFIRM: the signature of `open` (with and without the encoding argument), the encoding and line terminator that `write-line` produces, whether `strlen` counts characters or bytes, and `vlax-product-key` after `vl-load-com`. Every probing expression is wrapped in `vl-catch-all-apply` so that the host's **own error text is captured verbatim** (`vl-catch-all-error-message`) instead of aborting; it writes only to a **dedicated probe root** (a fresh, empty, ACL-restricted child created by the CAD manager exactly like an evidence child, outside the S1-A evidence child), create-new, and prints each result line to the command line. P2 contains **no governed attribute read** and produces **no label input**.
2. **C2, the governed capture text.** Written **after** the P2 results are reviewed, using **only the forms P2 proved on this host**; it carries no HOST-TO-CONFIRM item in the write path, no fallback and no alternative form. If P2 shows that no file-write form is acceptable, C2 is not written and the governed capture uses a different transfer mechanism that is itself ruled and versioned (the clipboard and the command-line window are excluded as transfer mechanisms by v1; that exclusion stays).
P2 is non-governing, produces no S1-A evidence and does not replace S1-A. C2 is what S1-A (a new run id) executes.

**AR11-05 — Guardrails for the typed delivery (apply to P2 and C2).** (a) One expression per paste, never a multi-line block, so that a partial paste cannot change a definition; the operator verifies the echo of each line against the file before pressing Enter; (b) the paste is performed with the **command line focused**; the invocation of `PASTECLIP` shows that the focus was in the drawing area: if any command other than the listed expressions is invoked, the operator stops and logs it (stop condition 7); (c) **no ad-hoc diagnostics**: the version lists the only read-only checks that are allowed between steps (for example reading back a defined symbol with `(type 'ct21d-put)`; the exact expressions are part of the hashed text), and anything else is a deviation; (d) AutoLISP is not loaded from a `.lsp` file or via `APPLOAD`/`load` (that is a code-load event under `SECURELOAD`/`TRUSTEDPATHS`, excluded by the S1-A scope "no NETLOAD, nothing loaded").

**AR11-06 — Mapping of a HOST-TO-CONFIRM mismatch to the run status must be explicit (erratum request).** Today the protocol says "attribute `UNKNOWN`" and the package says "instrument error or operator deviation ⇒ `INVALID` for every fact of the run" (package v2 8.1 items 6–7, 8.2; v3 5.1–5.2). Run `-01` was ruled `INVALID` because the stop conditions 6 and 7 and the loss of the session coincided. The next package version should say, for the I-1 family: (i) a mismatch observed in **P2** is expected data (it is the purpose of P2), recorded as `OBSERVED_DIFFERS` of the HOST-TO-CONFIRM item, not `INVALID`; (ii) a mismatch in **C2** has no HOST-TO-CONFIRM left to blame and is therefore a stop condition 6 ⇒ `INVALID`; (iii) the facts it could not read stay `UNKNOWN` inside the `INVALID` run. This removes the ambiguity without weakening fail-closed behaviour.

**AR11-07 — What does NOT change.** `i1-os-observation.ps1` (OS-level reads; it was never reached), `tools label`/`seal`, the phase-2 scripts (reviewed bytes, ledger `0207af83…c7f0`), the declared set, the evidence layout, the hash table, the TRUSTEDPATHS baseline. A new run still needs: a new `RunId` (`nn` increments or a new timestamp), a new `sessionId`, a new empty evidence child with the ACL of part 2, new `expected-inputs`, `acad.exe` count 0, and the Coordinator's **per-occurrence written authorization** for the cause recorded as `tooling/protocol` (V2.1 21.3).

**AR11-08 — Review before use.** Both P2 and C2 are text that will be executed on the host: they get the same independent source review as the scripts (a reviewer who did not write them tries to find any write outside the probe root / evidence child, any variable or setting change, any code load, any deviation from the hash-pinned text), with the hashes recorded before the CAD manager types anything.

## 4. Questions for the Coordinator / Owner (the Architect does not decide these)

1. **Is the P2 rehearsal inside the authorized non-governing host gate?** The Owner's gate lists machine/session fact capture and baseline-preparation characterization; a protocol compatibility rehearsal is not named. If the Coordinator rules it outside, the only alternative is a C2 with no host-dependent construct, which cannot be guaranteed from the repository (see section 2).
2. **A probe root.** P2 needs one extra fresh empty child with the same ACL as an evidence child (`probe\<RunId>`), or the use of a scratch child. Authorize its location and ACL (the CAD manager creates it).
3. **Run-id numbering for P2.** The id pattern requires `HGP-H<digits>-…`; the Coordinator assigns an activity number (a new number, not `H1`).
4. **The erratum of AR11-06** to the package (v4) or a ruling that applies it without a new version.
5. **Who drafts P2/C2.** Proposed: the implementer drafts P2 only (C2 waits for the P2 results), the Architect reviews the text, an independent reviewer audits it, the Coordinator authorizes the occurrence.

## 5. Status

```text
ARCHITECT_I1_VERSIONING_REVIEW = DELIVERED (proposed rulings AR11-01..08; questions in section 4)
I1_PROTOCOL_V1 = UNCHANGED (history)     NEW_PROTOCOL_TEXT_WRITTEN = NO     NEXT = COORDINATOR RULING ON SECTION 4, THEN THE IMPLEMENTER DRAFTS P2
RUN HGP-H1-20261001T225354Z-01: RUN_STATUS = INVALID     RETRY = NOT_AUTHORIZED     SEAL = HELD
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```
