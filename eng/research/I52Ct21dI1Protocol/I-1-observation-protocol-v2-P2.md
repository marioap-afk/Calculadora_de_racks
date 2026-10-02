# I-1 observation protocol, family 2, artifact P2 — compatibility qualification text (draft, not an instrument)

```text
DOCUMENT        = I-1 protocol family 2, artifact P2 (typed AutoLISP compatibility qualification). Protocol text, not an instrument
FAMILY          = I1-F2 (tuple field i1ProtocolFamily). Artifact P2 only. Artifact C2 is NOT WRITTEN and is not implied by this text
CLASSIFICATION  = NON_GOVERNING_PROTOCOL_COMPATIBILITY_QUALIFICATION
ACTIVITY        = H10       RUN ID PATTERN = ^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$        (future RunIds: HGP-H10-<yyyymmddThhmmssZ>-<nn>)
GOVERNING       = FALSE     NOT an S1-A retry     NOT product evidence     NO RackCad     NO NETLOAD     NO APPLOAD     no LISP loaded from a file
STATUS          = DRAFT FOR ARCHITECT EXACT REVIEW. NOT REVIEWED. NOT AUTHORIZED. NOT EXECUTED ON ANY HOST. NOTHING IN THIS REPOSITORY RUNS IT
AUTHORITY       = Coordinator ruling "I-1 VERSIONING / P2 DESIGN GATE" (decisions sections 254 and 255); Architect micro-review
                  docs/initiatives/I-52-ct21d-i1-protocol-versioning-review-v1.md (sha256 8b99f30d650c2377da8b64977c9bcbc4dff06d601352af7c4d0a9733a2950735),
                  rulings AR11-01..08 accepted; Architect closed changes and the Coordinator's "P2 REVISION 2" ruling (activity H10, PHASE2_CAPTURE_FOR_P2 = WAIVED,
                  governed-attribute isolation). The versioned erratum docs/initiatives/I-52-ct21d-host-gate-execution-package-v3-erratum-1.md carries the
                  package-level consequences (activity H10, OBSERVED_DIFFERS, tuple fields)
HISTORY         = I-1 v1 stays byte-for-byte as it is and is not referenced as text to execute:
                  eng/research/I52Ct21dHostFacts/protocol/I-1-observation-protocol.md   git blob 3a77ba62da77dfa2fc8bd2ffeb176a9454864193   sha256 a608833e72d9a0401d8aa3c33496a6f8f3d098426ac8e72b3a563187df4037c8
                  eng/research/I52Ct21dHostFacts/protocol/i1-os-observation.ps1         git blob a9081cff612b2290c35c86c0ded72f42c62b5e5a   sha256 662dd9ab686427077e8d178466ee2a625d494dad9592fe7f582202148d23ceb3
                  (sha256 = of the stored blob bytes)
RUN -01         = HGP-H1-20261001T225354Z-01 stays RUN_STATUS = INVALID, RETRY = NOT_AUTHORIZED, SEAL = HELD. This text neither repairs nor reinterprets it
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```

## 1. Purpose

Run `HGP-H1-20261001T225354Z-01` stopped with `; error: too many arguments` on the typed I-1 v1 sequence. The command-line screenshot (operator
supplied, sha256 `9837dcd08445a3cc53954dbf91583a0485d76414e0e03d67a17c14d90b089818`, kept at its governed path) shows these facts and no more:

- the multi-line `defun` forms were pasted and echoed (`CT21D-PUT`, `CT21D-ATTR` are echoed; nested `(_>` prompts appear);
- `(fboundp 'ct21d-attr)` printed `; error: no function definition: FBOUNDP`;
- `(ct21d-attr "AutoCadProduct" (vlax-product-key))`, the same call with `"AutoCadProfile" (getvar "CPROFILE")` and, later, with `"TRUSTEDPATHS"`
  printed `; error: too many arguments`;
- a `_pasteclip` command was started by a paste and its `Specify insertion point:` prompts consumed pasted text.

**Limitation of that screenshot.** The governed screenshot of original H1 is cropped at the top. Therefore the beginning of the function definitions is not observed, no exact
reconstruction of the pasted definitions is possible, and **no causal conclusion about the original `too many arguments` error is permitted**, by this text or by any reading of P2.

**The cause is not established.** Candidates: (a) the host's `open` rejects the third (encoding) argument; (b) an arity or definition effect of the paste
(a user function called with more arguments than its parameter list raises the same message); (c) something else. P2 characterizes, on the designated
machine, the behaviours that v1 left `[HOST-TO-CONFIRM]`, **separating (a) from (b)** by an explicit control, so that the later governed text (C2)
contains no construct whose behaviour is unknown. P2 decides nothing about the cause beyond what its own raw results show.

## 2. What P2 is and is not

- P2 is typed AutoLISP, one expression per paste, from the closed allowlist of section 5. It is read-only except for **two new files** under the
  P2 evidence root.
- P2 **reads and writes no governed S1-A attribute value**. It does not read `CPROFILE`, `SECURELOAD`, `TRUSTEDPATHS`, `MachineGuid` or `OsVersionBuild`, and no allowlisted
  expression names them (a family test checks it). It calls `vlax-product-key` once (P2-E25) **solely to characterize API mechanics**, under the governed-attribute isolation
  rule below.
- **Governed-attribute isolation (Coordinator acceptance).** Calling `vlax-product-key` in memory solely to characterize API mechanics is allowed **only if** the governed value is
  not printed, not written and not persisted. Only the type, the length and predefined boolean predicates may be emitted (P2-E23, P2-E24). The value is passed as an argument to the shape
  helper and is stored in no global. Any transcript or output that appears to reveal the actual governed value (a product-key-like string, a profile name, a semicolon-separated path chain,
  a machine GUID, an OS build number, in any case, escaping, splitting or hex encoding) is **rejected** by the offline analyzer (section 7), which then produces no record.
- P2 produces no label input, no `raw-*.txt`, no `declared-length-*.txt`, no `machine-label.json`. It is not S1-A, not a retry of S1-A, not a governing
  run and not product evidence. No RackCad assembly, no instrument, no `NETLOAD`, no `APPLOAD`, no `load` of a file; the only code it defines is the
  `ct21d-p2-*` AutoLISP symbols typed from section 5. No LISP file exists in this family.
- Evidence root: `D:/I52-CT21D-HOST/evidence/<H10-RunId>` (`NeedScratch = false`, `NeedCopy = false`). The directory is created by the CAD manager per part 2
  of the governed ACL plan. **P2 never creates a directory.** The only files P2 creates are `p2-a1.txt` and `p2-a2.txt` there, create-new.

## 3. Preconditions (none is satisfied now)

A P2 occurrence may start only when **all** of the following hold. Today none holds: P2 is a draft; no review, no authorization, no RunId, no evidence child.

1. The Coordinator's **per-occurrence written authorization** names: a concrete H10 RunId; the `sessionId`; the `expected-inputs` file (path and sha256); `i1ProtocolFamily`
   and `i1ProtocolLedgerSha256` (the sha256 of `HASHES-I1-V2.txt` as ruled); the substituted line P2-E01 and its SHA-256 (section 4, item 7); the evidence root;
   the custody location of the screenshots. The cause is recorded as `tooling/protocol`.
2. The exact bytes of this family were reviewed by the Architect and by an independent source reviewer (AR11-08), and `HASHES-I1-V2.txt` is the ledger that review named.
3. The **CAD-MANAGER PRE-HOST ATTESTATION** of section 3.1 is delivered, complete, with every line matched.

### 3.1 CAD-MANAGER PRE-HOST ATTESTATION

Coordinator ruling: `PHASE2_CAPTURE_FOR_P2 = WAIVED`, because P2 is `NON_GOVERNING_PROTOCOL_COMPATIBILITY_QUALIFICATION` and captures no governed S1-A product or session facts. The condition that
package v3 5.1 item 16 imposes on a governing session therefore does not apply to P2. It is **replaced** by this attestation, made by the CAD manager before the host is touched. It is an
attestation by the CAD manager from the CAD manager's own records and observations; P2 itself reads none of these facts.

The attestation requires, for the occurrence:

1. the **exact designated machine** (the machine of the machine-class record), identified as the record identifies it;
2. the **expected AutoCAD build and profile**, taken from the machine-class record, and the CAD manager's observation that the installed build and the profile in use match them;
3. the **`acad.exe` process count is 0** before startup;
4. the **`TRUSTEDPATHS` exact observed-baseline match**: the CAD manager's records show that the TRUSTEDPATHS in effect equal the observed baseline, sha256
   `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498`, 19 entries (P2 reads no TRUSTEDPATHS value; this is established outside P2);
5. the **exact P2 protocol, allowlist, schema, analyzer and ledger hashes** equal those named by the Coordinator's authorization, verified with `sha256sum -c HASHES-I1-V2.txt` on the tree that will be used;
6. a **fresh H10 evidence child** exists with the governed ACL (part 2 of the governed ACL plan; `NeedScratch = false`, `NeedCopy = false`), **created by the CAD manager** for this RunId, and it is **EMPTY**;
7. **one fresh AutoCAD process** will be started; **no RackCad** assembly; **no `NETLOAD`**.

Because the evidence child is empty at P2 start, it holds **no phase-2 record files**. The analyzer tolerates other entries anyway: it lists their names only and never opens them.

```text
ATTESTATION TEMPLATE  (the CAD manager fills it in; nothing is pre-filled by the drafter; a hash is never invented)
ATTESTATION_FOR_H10_RUN_ID                      = HGP-H10-________________-__
DESIGNATED_MACHINE (as in the machine-class record) = ____________________          MATCH = YES / NO
EXPECTED_AUTOCAD_BUILD (machine-class record)   = ____________________          OBSERVED = ____________________   MATCH = YES / NO
EXPECTED_PROFILE (machine-class record)         = ____________________          OBSERVED = ____________________   MATCH = YES / NO
ACAD_EXE_PROCESS_COUNT_BEFORE_STARTUP           = 0                            OBSERVED = ___                    MATCH = YES / NO
TRUSTEDPATHS_OBSERVED_BASELINE_SHA256           = a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498   (19 entries)
TRUSTEDPATHS_IN_EFFECT_MATCHES_BASELINE         = YES / NO                     (from the CAD manager's own records)
P2_PROTOCOL_SHA256   (named by the authorization) = ____________________________   VERIFIED = YES / NO
P2_ALLOWLIST_SHA256  (named by the authorization) = ____________________________   VERIFIED = YES / NO
P2_SCHEMA_SHA256     (named by the authorization) = ____________________________   VERIFIED = YES / NO
P2_ANALYZER_SHA256   (named by the authorization) = ____________________________   VERIFIED = YES / NO
P2_LEDGER_SHA256     (named by the authorization) = ____________________________   VERIFIED = YES / NO
EVIDENCE_CHILD_PATH                              = D:/I52-CT21D-HOST/evidence/HGP-H10-________________-__
EVIDENCE_CHILD_ACL                               = governed ACL, part 2 of the governed ACL plan    NeedScratch = false   NeedCopy = false   APPLIED = YES / NO
EVIDENCE_CHILD_CREATED_BY_CAD_MANAGER            = YES / NO
EVIDENCE_CHILD_ENTRY_COUNT_AT_START              = 0                            OBSERVED = ___                    EMPTY = YES / NO
AUTOCAD_PROCESSES_TO_START                       = 1 (fresh)       RACKCAD_ASSEMBLY = NONE       NETLOAD = NONE
ATTESTED_BY (CAD manager) = ____________________     DATE_UTC = ____________________     ANY "NO" ABOVE = P2 DOES NOT START
```

## 4. Operator discipline

1. **One expression per paste.** Never a multi-line block. The source of the text is `P2-ALLOWLIST.txt` (line N is P2-EN) or the table of section 5, both hashed; never chat.
   Copy exactly one line **without its terminator**.
2. **Readiness before every paste.** The AutoCAD command line is visibly focused (click it first); the text cursor is visibly active in it; the prompt is exactly `Command:` and ready for
   the next expression. The previous expression has finished printing before the next one is pasted. **One allowlisted expression per paste; wait for its result before the next.**
3. **Ctrl+V is prohibited unless the text cursor is visibly active in the AutoCAD command line.** A paste into anything else (the drawing area, a dialog, a palette) can start the
   `PASTECLIP` command, which is what happened in run -01.
4. **The paste does not execute anything.** AutoCAD does not execute a paste; Enter is pressed **only after** the operator has verified the pasted text character by character against the
   hashed line. If a paste executes by itself (a result appears without Enter having been pressed), that is an `OPERATOR_DEVIATION`.
5. **Stop signals.** If **any** of the following appears — `_pasteclip`, `PASTECLIP`, `Specify insertion point`, an unexpected command, an unexpected continuation prompt such as `(_>`, an
   unallowlisted expression, or a text that differs from the hashed line — the operator presses **ESC once** and **STOPS**. There is **no retry in the same run** and no same-run recovery.
   The event is classified `OPERATOR_DEVIATION`; the run is stopped; facts not read stay `UNKNOWN`; evidence is retained; the Coordinator rules. An unbalanced continuation prompt means
   an unbalanced paste: press ESC once and STOP.
6. **Closed allowlist.** Only the 25 expressions of section 5, once each, in order. **No ad-hoc diagnostics**: no `fboundp`, no `type` or `princ` of a symbol, nothing else. No
   repetition, no re-entry, no retry, no repair, no fallback.
7. **The only variable token** in the whole text is `<RUNID>` in P2-E01. The Coordinator's authorization substitutes it; its pattern `^HGP-H10-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$`
   is fixed. The hashed text is the **template**; the authorization names the substituted P2-E01 line and its SHA-256 (UTF-8 bytes of the line followed by one LF).
8. **An `; error:` printed by an allowlisted expression is an outcome** (recorded verbatim), not a deviation, and the operator **does not** re-enter or fix it. The next expression is entered
   anyway, except as stated by item 9; dependent expressions may print their own errors, which are also outcomes. **Exception, P2-E01:** if P2-E01 does not echo the authorized root string
   (for example it printed `; error: ...`, so `ct21d-p2-root` is `nil`), that is the E01 deviation of section 6.1 (`OPERATOR_DEVIATION`, STOP): the operator enters nothing further. every expression that builds a path from the root (P2-E10, P2-E18, P2-E19 and the
   calls of the helper `ct21d-p2-try`, P2-E16 and P2-E17, whose definition is P2-E15) would otherwise raise a raw `; error: bad argument type` **outside** `vl-catch-all-apply` (`strcat` of `nil` is evaluated before the catch or inside the
   helper body), and that raw error is not an outcome of those expressions but a consequence of the E01 deviation.
9. **Conditional gate before the open candidates (STOP_BEFORE_OPEN).** After P2-E11 the operator reads the printed lines of the two `findfile` controls. P2-E12 to P2-E19 (and the expressions
   after them) may be entered **only if both** of these lines were printed exactly:
   - `P2-E10 VALUE: nil` (the missing-file control), and
   - a line that begins `P2-E11 VALUE: "` (the existing absolute-path control printed a string).

   Otherwise the result is `STOP_BEFORE_OPEN`: **no open candidate executes**, the operator enters nothing further and goes to item 11, and the results of P2-E12 to P2-E25 are `INCOMPLETE`.
   There is no retry. (Whether the product-key shape group may still run after a `STOP_BEFORE_OPEN` is not decided here; this text stops.)
10. **Raw checkpoint screenshots.** The operator takes a **raw, unmodified** screenshot of the AutoCAD window, records its SHA-256, and stores it outside the repository, at each fixed
    checkpoint: **after P2-E05** (the last arity control), **after P2-E09** (the last strlen / ascii expression), **after P2-E17** (A2), **after P2-E19** (the last findfile-after-open
    expression) and **after P2-E25** (the final expression). A screenshot is not edited, cropped or annotated (an annotated copy is a separate file and is not evidence).
11. **Custody of screen results.** Results printed on the command line are not files. After P2-E25 (or after the stop), **before closing AutoCAD**, the operator opens the text window (F2),
    makes it show the full output in order, and takes one or more raw screenshots covering all of it in order, with the same rules as item 10. Output lost by scrolling is `UNKNOWN`. The operator may
    transcribe the **result lines only** (not prompts) into the optional screen-results JSON of the offline analyzer; the screenshots govern any difference.
12. **Stated plainly:** if neither candidate A1 (P2-E16) nor A2 (P2-E17) is accepted by the host, **no file exists and the results of P2 exist only on the screen**; they are then
    custodied only by the screenshots of items 10 and 11.

## 5. The closed allowlist (every expression the operator may type)

Exact text, one expression per paste, in this order. `P2-ALLOWLIST.txt` holds the same 25 lines (a test of the family checks that both sources are identical). **Every expression is at most 255
characters, ASCII, one line, with balanced parentheses and quotes** (a test checks it); the longest is measured in the family report. Test groups: **A** open signature
and the arity control, **B** write-line terminator and encoding (judged offline), **C** the escape and `strlen`, **D** `vlax-product-key` shape, **F** `findfile`. Setup and helper
definitions have no group. Each logical function is split into one-line helpers so that no line is long; there is no LISP file and no `APPLOAD`; one expression is pasted at a time.

| ID | Exact text (AutoLISP, one line) | Purpose |
|---|---|---|
| P2-E01 | `(setq ct21d-p2-root "D:/I52-CT21D-HOST/evidence/<RUNID>/")` | Setup: sets the global `ct21d-p2-root` to the P2 evidence root (the only variable token is `<RUNID>`). The echo shows the string, which the operator compares with the authorization |
| P2-E02 | `(defun ct21d-p2-ctl (name text / p f) (setq p name f text) (strcat p "-" f))` | A, control: defines a user function with the same parameter shape as v1 `ct21d-put` (two parameters, then locals after `/`, a parameter named `text`) |
| P2-E03 | `(progn (princ (strcat "\nP2-E03 " (ct21d-p2-ctl "AB" "CD"))) (princ))` | A, control: calls it with two arguments, directly (no catch), so the host's own error is printed verbatim. Separates the "arity/definition" hypothesis from the `open` hypothesis |
| P2-E04 | `(defun ct21d-p2-ctl2 (key text) (ct21d-p2-ctl (strcat "raw-" key ".txt") text))` | A, control: defines a function with the shape of v1 `ct21d-attr` (two parameters, no locals) that calls the first one with a computed first argument |
| P2-E05 | `(progn (princ (strcat "\nP2-E05 " (ct21d-p2-ctl2 "K" "V"))) (princ))` | A, control: calls it with two constant arguments (no governed read), directly. Reproduces the v1 nested-call shape that failed. **Checkpoint 1: raw screenshot after this expression** |
| P2-E06 | `(defun ct21d-p2-show (r) (cond ((vl-catch-all-error-p r) (strcat "ERROR: " (vl-catch-all-error-message r))) ((= (type r) 'FILE) "VALUE: FILE-DESCRIPTOR") (T (strcat "VALUE: " (vl-prin1-to-string r)))))` | Helper: returns `ERROR: <host message>` for an error object, the constant token `VALUE: FILE-DESCRIPTOR` for a file descriptor (never stringified), otherwise `VALUE: <prin1 text>`. Used by the later probes to print the host's own error text verbatim |
| P2-E07 | `(progn (princ (strcat "\nP2-E07 " (ct21d-p2-show (vl-catch-all-apply 'strlen (list "ABC"))))) (princ))` | C, control: `strlen` of `"ABC"` |
| P2-E08 | `(progn (princ (strcat "\nP2-E08 " (ct21d-p2-show (vl-catch-all-apply 'ascii (list "\U+00E9"))))) (princ))` | C, part A (interpretation of the escape): `ascii` of the string `"\U+00E9"` inside the catch, shown through the helper. Records the code the host produces for the first character of the escape. Separate from `strlen` |
| P2-E09 | `(progn (princ (strcat "\nP2-E09 " (ct21d-p2-show (vl-catch-all-apply 'strlen (list "A\U+00E9Z"))))) (princ))` | C, part B (`strlen`): `strlen` of a string with one `\U+00E9` escape (characters versus bytes). **Checkpoint 2: raw screenshot after this expression** |
| P2-E10 | `(progn (princ (strcat "\nP2-E10 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-absent.txt")))))) (princ))` | F, gate control (missing file): `findfile` on an absent path inside the evidence root. Must print `P2-E10 VALUE: nil` for the open candidates to be entered |
| P2-E11 | `(progn (princ (strcat "\nP2-E11 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list "C:/Windows/System32/kernel32.dll"))))) (princ))` | F, gate control (existing absolute path): `findfile` on a path that exists on every Windows installation (read-only existence test; nothing is read from the file). Must print a string. **Gate: read E10 and E11 before entering E12** |
| P2-E12 | `(defun ct21d-p2-lines (f) (write-line "P2-ASCII-LINE-1" f) (write-line "P2-ACCENT-\U+00E9-END" f) (write-line "P2-ASCII-LINE-3" f))` | Helper: writes the three fixed test lines with `write-line` (one ASCII line, one line with the `\U+00E9` escape, one ASCII line); closes nothing |
| P2-E13 | `(defun ct21d-p2-say (id w fn r) (princ (strcat "\n" id " " w " " (ct21d-p2-show (vl-catch-all-apply fn (list r))))))` | Helper: applies a function to the descriptor inside the catch and prints `<id> <step> <result>` (used for the `write-line` and `close` steps) |
| P2-E14 | `(defun ct21d-p2-after (id r) (princ (strcat "\n" id " open " (ct21d-p2-show r))) (if (= (type r) 'FILE) (progn (ct21d-p2-say id "write-line" 'ct21d-p2-lines r) (ct21d-p2-say id "close" 'close r))))` | Helper: prints the `open` result line; if (and only if) a file descriptor was returned, runs the `write-line` step and then the `close` step |
| P2-E15 | `(defun ct21d-p2-try (id name args / p) (setq p (strcat ct21d-p2-root name)) (if (findfile p) (princ (strcat "\n" id " REFUSED, exists: " name)) (ct21d-p2-after id (vl-catch-all-apply 'open (cons p args)))) (princ))` | Helper: create-new guard with `findfile` (prints REFUSED if the target exists), then `open` inside `vl-catch-all-apply`, handing its result to the previous helper. Same semantics and effect order as the former single long helper |
| P2-E16 | `(ct21d-p2-try "P2-E16" "p2-a1.txt" '("w"))` | A, candidate A1: `(open <path> "w")` into `p2-a1.txt` |
| P2-E17 | `(ct21d-p2-try "P2-E17" "p2-a2.txt" '("w" "UTF-8"))` | A, candidate A2: `(open <path> "w" "UTF-8")` into `p2-a2.txt`. **Checkpoint 3: raw screenshot after this expression** |
| P2-E18 | `(progn (princ (strcat "\nP2-E18 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-a1.txt")))))) (princ))` | F: `findfile` on `p2-a1.txt` after A1 |
| P2-E19 | `(progn (princ (strcat "\nP2-E19 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-a2.txt")))))) (princ))` | F: `findfile` on `p2-a2.txt` after A2. **Checkpoint 4: raw screenshot after this expression** |
| P2-E20 | `(progn (princ (strcat "\nP2-E20 " (ct21d-p2-show (vl-catch-all-apply 'vl-load-com nil)))) (princ))` | D precondition: `vl-load-com` (as v1 did), applied inside the catch so any error text is captured |
| P2-E21 | `(defun ct21d-p2-tf (x) (if x "T" "nil"))` | D helper: `T` or `nil` as text |
| P2-E22 | `(defun ct21d-p2-pre (k) (strcat " startsExact=" (ct21d-p2-tf (= (substr k 1 9) "Software\\")) " startsAnyCase=" (ct21d-p2-tf (= (strcase (substr k 1 9)) "SOFTWARE\\"))))` | D helper: the two prefix booleans (exact `Software\` and any-case), computed on its argument and returned as text; never the value |
| P2-E23 | `(defun ct21d-p2-str (k) (strcat " type=STR len=" (itoa (strlen k)) (ct21d-p2-pre k) " hasWowAnyCase=" (ct21d-p2-tf (vl-string-search "WOW6432NODE" (strcase k)))))` | D helper: the shape text of a string argument (type, length, the prefix booleans, the `WOW6432Node` boolean); never the value |
| P2-E24 | `(defun ct21d-p2-shape (id k) (princ (cond ((vl-catch-all-error-p k) (strcat "\n" id " ERROR: " (vl-catch-all-error-message k))) ((/= (type k) 'STR) (strcat "\n" id " type=" (vl-prin1-to-string (type k)))) (T (strcat "\n" id (ct21d-p2-str k))))) (princ))` | D helper: prints only the **shape** of its argument (the error text, the type name, or the shape text), never the value |
| P2-E25 | `(ct21d-p2-shape "P2-E25" (vl-catch-all-apply 'vlax-product-key nil))` | D: `vlax-product-key` inside the catch, passed to the shape helper; the value is neither printed nor stored. **Checkpoint 5: raw screenshot after this final expression** |

Constraints stated for the reviewer (and checked by the family tests): every line is ASCII, a single line of at most 255 characters, with balanced parentheses and quotes; every defined or set global
name starts with `ct21d-p2-`; no line calls a function that writes anything except `open`/`write-line`/`close` through P2-E12 to P2-E15 and `princ`; no `load`, `command`, `setvar`,
`getvar`, `vl-registry-*`, `vla-*` or `vlax-*` call other than the one `vlax-product-key` of P2-E25; no expression names `CPROFILE`, `SECURELOAD`, `TRUSTEDPATHS`, `MachineGuid` or `OsVersionBuild`;
the only paths used are the evidence root of P2-E01, `p2-absent.txt`, `p2-a1.txt`, `p2-a2.txt` and the existence test of P2-E11. **A file descriptor is never stringified**: the display helper
(P2-E06) prints the constant token `FILE-DESCRIPTOR` for it, before any `vl-prin1-to-string` call.

## 6. The closed result model and the interpretation of every possible outcome

### 6.0 The closed result model

Per **expression**, the only results are:

| Result | Meaning |
|---|---|
| SUPPORTED | the host accepted the exact form (a value was returned, or the definition was echoed) |
| UNSUPPORTED | the host rejected that exact form by arity: the text `too many arguments`, `too few arguments`, `wrong number of arguments` or an equivalent arity text |
| HOST_ERROR | the host printed another error for that expression: an error of **argument value** (not arity), or any other host text; recorded **verbatim** |
| OPERATOR_DEVIATION | a stop signal of section 4 item 5, or a text that differs from the hashed line |
| INCOMPLETE | the expression did not complete or was not entered (for example `STOP_BEFORE_OPEN`, a `REFUSED` guard, or a helper error before the line that identifies `open`) |
| UNKNOWN | the result could not be read (missing, ambiguous or without screenshot custody) |

Each recorded expression also carries a category: `NONE`, `ARITY`, `ARGUMENT_VALUE`, `DEFINITION`, `HELPER`, `GUARD` or `OTHER`.
**An error printed by a defun-type expression or by a helper is a recorded PROTOCOL RESULT** (`HOST_ERROR` with category `DEFINITION`, or `HELPER`); it is **not** a v1 compatibility mismatch,
and the items that depend on it are `INCOMPLETE`.

For the two open candidates (A1 = P2-E16, A2 = P2-E17) there is additionally a **form verdict**, one of `SUPPORTED`, `UNSUPPORTED`, `HOST_ERROR`, `DIFFERENT`, `INCOMPLETE`, `UNKNOWN`:

- `UNSUPPORTED` = the host's arity rejection of that exact form (`too many arguments` or equivalent arity text);
- `HOST_ERROR` = an error of **argument value** (not arity), recorded with the host's text verbatim;
- `DIFFERENT` = the form was **accepted**, but the offline bytes differ from the behaviour a future C2 would require. **What C2 would require:** a file in **UTF-8 without BOM**, the accent field
  holding the bytes **C3 A9**, a terminator that is **LF or CRLF**, and **three lines** (`P2-ASCII-LINE-1`, `P2-ACCENT-<accent>-END`, `P2-ASCII-LINE-3`). For A1 a `DIFFERENT` is expected data, not a failure;
- `SUPPORTED` = accepted and the offline bytes meet all four requirements;
- `INCOMPLETE` and `UNKNOWN` as for expressions.

**Mapping to the Host Package vocabulary** (v2 8.2 / v3 5.2 as the erratum extends it). No pass/fail product semantics are invented:

| Closed result or form verdict | Host Package status |
|---|---|
| SUPPORTED, and the printed value equals the reference expectation of the reading table | OBSERVED |
| SUPPORTED, and the printed value differs from the reference expectation | OBSERVED_DIFFERS |
| form verdict SUPPORTED (accepted and meets the four requirements) | OBSERVED |
| form verdict DIFFERENT | OBSERVED_DIFFERS |
| UNSUPPORTED (arity text verbatim) | OBSERVED_DIFFERS |
| HOST_ERROR with category ARGUMENT_VALUE or OTHER | OBSERVED_DIFFERS, text verbatim |
| HOST_ERROR with category DEFINITION or HELPER | no v1 compatibility item is changed by it; the dependent items are NOT_OBSERVED |
| INCOMPLETE | NOT_OBSERVED |
| UNKNOWN | UNKNOWN |
| OPERATOR_DEVIATION | INVALID for the run (a stop condition of section 9); every unread fact stays UNKNOWN |
| a fact that no source of this occurrence can show (for example the terminator when no file exists) | NOT_OBSERVABLE |

`OBSERVED` = the result was read and equals the **reference expectation** stated in the reading tables; `OBSERVED_DIFFERS` = it was read and differs; `UNKNOWN` = it could not be read; `NOT_OBSERVABLE` = no source
can show it; `NOT_OBSERVED` = the expression did not complete or was not entered; `INVALID` = only for the stop conditions of section 9. The reference expectation is the assumption of I-1 v1 where v1 stated one;
otherwise it is the drafter's expectation, labelled as such. **A difference is a characterization result, never `INVALID` by itself.**

### 6.1 Defining and setup expressions

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E01 | the echo is the string with the authorized RunId | SUPPORTED; proceed |
| P2-E01 | anything else, **a printed `; error: ...` included** | OPERATOR_DEVIATION: stop. This is the only row of this section where a host error is a deviation and not a recorded outcome: with `ct21d-p2-root` unset (`nil`), P2-E10, P2-E16, P2-E17, P2-E18 and P2-E19 would raise a raw `bad argument type` outside `vl-catch-all-apply` (section 4 item 8). `Analyze-I1P2.ps1` maps every P2-E01 text other than the exact authorized echo to OPERATOR_DEVIATION |
| P2-E02, E04, E06, E12, E13, E14, E15, E21, E22, E23, E24 | the upper-case symbol name is echoed (v1 showed the same for `CT21D-PUT`) | SUPPORTED: the definition was accepted in this session |
| P2-E02, E04, E06, E12, E13, E14, E15, E21, E22, E23, E24 | `; error: ...` | HOST_ERROR, category DEFINITION, text verbatim: a recorded protocol result, not a v1 compatibility mismatch; the expressions that depend on that symbol will print their own errors (category HELPER); nothing is repaired |
| any | a `(_>` prompt | an unbalanced paste: OPERATOR_DEVIATION, press ESC once and stop (section 4 item 5) |

### 6.2 Group A, the arity control (`userFunctionArityControl`)

Reference expectation (drafter's): both calls return the concatenated string.

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E03 | `P2-E03 AB-CD` | SUPPORTED, OBSERVED. A user function with v1's parameter shape (two parameters, locals after `/`, parameter `text`) accepts two arguments in this session |
| P2-E03 | `; error: too many arguments` (or other arity text) | UNSUPPORTED, OBSERVED_DIFFERS. The v1 parameter shape itself is rejected in this session: the "arity/definition effect" hypothesis is **supported**; A1/A2 and the helpers are read with this in mind |
| P2-E03 | `; error: no function definition: CT21D-P2-CTL` | HOST_ERROR, category HELPER: P2-E02 failed; the control is NOT_OBSERVED, not a mismatch |
| P2-E05 | `P2-E05 raw-K.txt-V` | SUPPORTED, OBSERVED. The v1 nested-call shape works with constant arguments |
| P2-E05 | `; error: ...` | as for P2-E03, for the nested shape |
| P2-E03 / P2-E05 | any other error | HOST_ERROR (argument value or other), OBSERVED_DIFFERS, verbatim |

`userFunctionArityControl` is `OBSERVED` only when both P2-E03 and P2-E05 are OK; `OBSERVED_DIFFERS` when either is not; `NOT_OBSERVED` when a definition or helper error is involved; `UNKNOWN` when either is missing or has no screenshot custody.

**Reading of combinations (no inference beyond this table; the cause of run -01 is not decided by P2):**

| Control (E03, E05) | A2 (E17) | Reading |
|---|---|---|
| OK | `open ERROR: too many arguments` (arity text) | form UNSUPPORTED; consistent with candidate (a): this host's `open` rejects the encoding argument. The host's own text is the evidence; P2 does not extend it to other builds |
| OK | `open VALUE: FILE-DESCRIPTOR` | candidate (a) is **not supported** on this host; the cause of run -01 stays unexplained (paste effect or something else). C2 must still use only forms P2 proved |
| ERROR (arity) | any | candidate (b) is supported for the v1 parameter shape in this session; the A results are read with this limit |
| missing | any | `UNKNOWN`; no reading |

### 6.3 Group C, the escape (`escapeInterpretation`) and `strlen` (`strlenSemantics`)

These are **two separate observations**, recorded separately in the schema, and neither one assumes the other.

**A) Interpretation of `"\U+00E9"` (P2-E08).** `(ascii "\U+00E9")` is applied inside the catch and shown through the helper. The printed code is recorded as `printedCode` with a label. Labels are **characterization labels, not assumptions**:

| P2-E08 prints | Label | Host Package status |
|---|---|---|
| `VALUE: 233` | `ACCENT_CODEPOINT_LIKE` (the escape produced a first character with the code point of e-acute) | OBSERVED (reference expectation, drafter's) |
| `VALUE: 195` | `UTF8_LEAD_BYTE_LIKE` (the first character has the code of the first UTF-8 byte C3) | OBSERVED_DIFFERS |
| `VALUE: 92` | `BACKSLASH_UNINTERPRETED` (the escape appears uninterpreted) | OBSERVED_DIFFERS |
| any other number | `OTHER_CODE` | OBSERVED_DIFFERS, verbatim |
| `ERROR: ...` | `NOT_DETERMINED` | OBSERVED_DIFFERS (UNSUPPORTED if arity text), verbatim |

**B) `strlen` (P2-E07 control, P2-E09 escaped string).** Reference expectation (v1 text: the offline helper counts Unicode scalar values and fails closed on a mismatch): characters. Labels are characterization labels,
not assumptions; **do not assume strlen counts bytes**. The byte encoding is determined **only** by the offline file-byte analysis of section 7.

| ID | Outcome as it appears | Label and reading |
|---|---|---|
| P2-E07 | `VALUE: 3` | control OK; any other value or an error makes the whole item OBSERVED_DIFFERS (`OTHER_VALUE`) |
| P2-E09 | `VALUE: 3` | `CHARACTER_INTERPRETATION_CANDIDATE`: a character-interpretation candidate; OBSERVED |
| P2-E09 | `VALUE: 4` | `UTF8_BYTE_COUNT_LIKE`: a UTF-8-byte-count-like observation (4 = A, C3, A9, Z); OBSERVED_DIFFERS. It is not an assumption that `strlen` counts bytes |
| P2-E09 | `VALUE: 9` | `ESCAPE_UNINTERPRETED`: the escape appears uninterpreted (9 literal characters); OBSERVED_DIFFERS; read with group B |
| P2-E09 | any other value, or `ERROR:` | `OTHER_VALUE`, OBSERVED_DIFFERS, verbatim |

### 6.4 Group F, `findfile` (`findfileAbsent`, `findfilePresent`) and the gate

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E10 | `P2-E10 VALUE: nil` | `findfileAbsent` OBSERVED (v1 assumed nil for a path that does not exist; v1 left it `[HOST-TO-CONFIRM]`); half of the gate |
| P2-E10 | `VALUE: "<string>"` | OBSERVED_DIFFERS: `findfile` returned a path for a path that does not exist (the create-new guard of P2-E15 would also be unreliable); gate fails |
| P2-E10 | `ERROR: <text>` | OBSERVED_DIFFERS, text verbatim; gate fails |
| P2-E11 | `P2-E11 VALUE: "<string>"` | `findfilePresent` OBSERVED (reference expectation, drafter's: a string for an existing file); the other half of the gate |
| P2-E11 | `VALUE: nil` | OBSERVED_DIFFERS: `findfile` did not see an existing file at an absolute forward-slash path; gate fails |
| P2-E11 | `ERROR: <text>` | OBSERVED_DIFFERS, text verbatim; gate fails |
| P2-E18 / E19 | `VALUE: "<string>"` when the offline analysis finds the file; `VALUE: nil` when it does not | consistent: no change |
| P2-E18 / E19 | the opposite of the offline file state | `findfilePresent` becomes OBSERVED_DIFFERS (the guard sees a different state than the file system) |

**Gate.** Gate = PASSED only when P2-E10 printed `VALUE: nil` **and** P2-E11 printed a string. Otherwise `STOP_BEFORE_OPEN` (section 4 item 9): A1 and A2 are `INCOMPLETE`, and any entry for P2-E12 to P2-E25 is an OPERATOR_DEVIATION.

### 6.5 Group A and B, `open`, `write-line`, encoding and terminator

P2-E16 (A1, two arguments) and P2-E17 (A2, three arguments) are independent: both are entered (when the gate passed), whatever the other printed. Each prints up to three lines: `open ...`, `write-line ...`, `close ...`. A
descriptor is printed as the token `FILE-DESCRIPTOR`.

**Open helper error rule.** If the host emits an error **before** a line that identifies `P2-E<id> open ...` (that is, the helper call or definition failed), the result is `INCOMPLETE`
and it is **not evidence about `open`**.

| ID | Outcome as it appears | Result, form verdict and reading |
|---|---|---|
| P2-E16 / E17 | `; error: ...` (any host error) printed **before** an `open` line | result INCOMPLETE (category HELPER), form INCOMPLETE, NOT_OBSERVED: not evidence about open |
| P2-E16 / E17 | `open VALUE: FILE-DESCRIPTOR`, and the offline bytes meet the four requirements (UTF-8 without BOM, accent bytes `C3 A9`, terminator LF or CRLF, three lines) | result SUPPORTED, form SUPPORTED, OBSERVED |
| P2-E16 / E17 | `open VALUE: FILE-DESCRIPTOR`, but the offline bytes differ (BOM, a single byte, another encoding, the escape not interpreted, another terminator, other lines) | result SUPPORTED, form DIFFERENT, OBSERVED_DIFFERS; the accepted-but-different behaviour is the finding |
| P2-E16 / E17 | `open ERROR: too many arguments` (arity text) | result UNSUPPORTED, form UNSUPPORTED, OBSERVED_DIFFERS; no file exists |
| P2-E16 / E17 | `open ERROR: <argument-value text>` | result HOST_ERROR, form HOST_ERROR, OBSERVED_DIFFERS; text verbatim; no file exists |
| P2-E16 / E17 | `open VALUE: nil` | result UNKNOWN, form UNKNOWN: `open` returned no descriptor and no error; the cause (path, ACL, host) is not separable on the screen. The directory precondition is rechecked by the CAD manager outside P2 |
| P2-E16 / E17 | `REFUSED, exists: p2-a1.txt` (or `p2-a2.txt`) | result INCOMPLETE (category GUARD), a stop that the Coordinator rules (the evidence root was not empty, or `findfile` is unreliable); no retry |
| P2-E16 / E17 | a file exists offline but the screen shows no descriptor | UNKNOWN (fail closed) |
| P2-E16 / E17 | `write-line ERROR: <text>` or `close ERROR: <text>` after a descriptor | recorded verbatim in the detail of the item; the descriptor may stay open until AutoCAD ends; no repair |
| P2-E16 / E17 | a `; error: ...` line printed **after** a valid `open` line | HOST_ERROR, category OTHER: recorded verbatim in the detail of the item (the analyzer prefixes it `HOST_ERROR/OTHER after the open line:`); it does not change the reading of the `open` line itself |
| (offline) | terminator of the files: LF or CRLF | `writeLineTerminator` OBSERVED (the offline label helper accepts exactly one trailing LF or CRLF) |
| (offline) | CR only, no terminator, or mixed | `writeLineTerminator` OBSERVED_DIFFERS |
| (offline) | no file exists | `writeLineTerminator` NOT_OBSERVABLE (the terminator is not shown on the command line) |

The terminator and the encoding / BOM are **not interpreted on the command line**: they are characterized **offline from the file bytes** by `Analyze-I1P2.ps1` (section 7).

### 6.6 Group D, `vlax-product-key` shape (`productKeyShape`)

Only the type, the length and predefined boolean predicates are emitted (governed-attribute isolation, section 2).

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E20 | any `VALUE:` (`VALUE: nil`, `VALUE: T`, or `VALUE: <symbol>`: a bare token of letters, digits, `*`, `_` and `-`, starting with a letter, at most 41 characters, with no run of four digits) | informational: `vl-load-com` ran inside the catch. The analyzer accepts that closed token only for P2-E20 and only after the leakage guard; any other `VALUE:` text on P2-E20 is outside the closed vocabulary |
| P2-E20 | `ERROR: <text>` | recorded verbatim; P2-E25 may then fail with its own text |
| P2-E25 | `type=STR len=<n> startsExact=T startsAnyCase=T hasWowAnyCase=nil` | OBSERVED: a string, starts with exactly `Software\`, holds no `WOW6432Node` in any case (v1 assumption) |
| P2-E25 | `startsExact=nil` and `startsAnyCase=T` | OBSERVED_DIFFERS: the prefix differs in letter case (the digits of the value are not printed; only these booleans) |
| P2-E25 | any other boolean combination, a type other than STR, or `ERROR: <text>` | OBSERVED_DIFFERS, verbatim; the value is never printed |
| P2-E25 | `; error: no function definition: CT21D-P2-...` | HOST_ERROR, category HELPER: NOT_OBSERVED, not evidence about the product key |

## 7. Offline characterization from the files and the screenshots

`Analyze-I1P2.ps1` (PowerShell 7, offline) reads only `p2-a1.txt` and `p2-a2.txt` of the P2 evidence root, never writes to disk, and prints the analysis record as canonical JSON to stdout
(schema `schemas/ct21d.i1-p2.v1.json`). For each file it records existence, length, sha256, the hex of the first 64 bytes, whether a BOM is present, the terminator class
(LF, CRLF, CR, NONE, MIXED), the bytes of the accent field, and the encoding interpretation. The expected content (with `<T>` the terminator the host produced) is
`P2-ASCII-LINE-1<T>P2-ACCENT-<accent field>-END<T>P2-ASCII-LINE-3<T>`. The screen results come from an optional JSON file that transcribes the screenshots (result lines only) and the five checkpoint hashes; without
custody (screenshot sha256) the screen-derived items are `UNKNOWN`. It records, per transcribed expression, the closed result and category of section 6.0; the gate; the escape reading and the `strlen` reading separately; and the form verdicts of A1 and A2.

**Rejection of leakage.** The analyzer refuses a transcription (and produces no record) if any view of its text — as typed, with escapes expanded (`\x`, `\u`, `\U+`, `%`, HTML entities, octal, `0x`), with hex runs decoded, and with all
separators removed — looks like a governed value or names a governed attribute: a product-key-like string (including its `ACAD-nnnn:xxx` suffix), a profile-like name, a semicolon-separated path chain, a machine GUID (with or
without hyphens), an OS build number, or the names `CPROFILE`, `SECURELOAD`, `TRUSTEDPATHS`, `MachineGuid`, `OsVersionBuild`. It refuses any line outside the closed P2 output vocabulary (including a stringified file descriptor `#<...>`), and it
refuses to run unless every file of this family equals `HASHES-I1-V2.txt`. The analyzer decides no run status: it cannot see tuple drift, processes, dialogs or writes outside the root.

## 8. Status semantics

8.1 **A mismatch with a v1 assumption in P2 is a characterization result**, recorded as `OBSERVED_DIFFERS`. It is expected data (it is the purpose of P2). It is not `INVALID`. It is never repeated to obtain a better result.
A definition or helper error is likewise a recorded protocol result (section 6.0), not a v1 compatibility mismatch.

8.2 **`INVALID` only for stop conditions:** tuple drift; another `acad.exe`; an unscripted dialog or prompt; a write outside the P2 evidence root; an expression outside the allowlist (or a text that differs
from the hashed line); an `OPERATOR_DEVIATION` of section 4 item 5; a crash (the Coordinator rules `CRASH_FINDING` / `CRASH_ENVIRONMENT`). A `REFUSED` result is a stop that the Coordinator rules. In an `INVALID` run, facts not read stay `UNKNOWN`; evidence is retained.

8.3 **No retry.** A new P2 occurrence needs a new Coordinator authorization, a new RunId and a new evidence child. A changed byte of this family is a new version of the family. Run `HGP-H1-20261001T225354Z-01` stays immutably `INVALID`; nothing here authorizes a retry of it.

## 9. Relation to C2

C2 (the governed capture text) is **not written** here and no activity number is reserved for it. It will be written only after the evidence of an authorized P2 occurrence is reviewed, and will use only the forms P2 showed to work.
What C2 would require of a write form is stated in section 6.0 (UTF-8 without BOM, accent bytes `C3 A9`, a terminator LF or CRLF, three lines). In C2 the reviewed chosen
behaviour is **normative**: any mismatch of C2 with the behaviour reviewed from P2 is a **STOP**, makes the run `INVALID`, and leaves the unread facts `UNKNOWN`. There is no silent fallback and no manual normalization in C2. If P2 shows that no file-write
form is accepted, C2 is not written and the governed capture needs another transfer mechanism that is itself ruled and versioned (the clipboard and the command-line window stay excluded as transfer mechanisms by v1).

## 10. What P2 does not decide

- The cause of the failure of run -01 (the screenshot is cropped at the top; no causal conclusion is permitted); whether `open` with an encoding argument is the cause.
- Whether any observed form is acceptable for C2, or what C2 contains.
- Whether the P2 evidence root is sealed, and the custody of the analysis record.
- Whether a new H1 attempt is authorized, the S1-A status, the label, or any governed attribute value.
- The numbers or versions of the host (build, profile); P2 characterizes this session only and extends nothing to other machines or builds.
- Whether the product-key shape group may run after a `STOP_BEFORE_OPEN`; this text stops the occurrence.
- P2 renumbers no existing activity: the fixture activities that package v3 names H5, H6 and H7 keep those numbers (the erratum records the Coordinator's resolution of OI-E1). No number is reserved for C2.

## 11. Constructs whose behaviour is NOT verified (flagged for the reviewer)

Nothing in this text was executed. Known only from the repository and from the screenshot: v1's forms `defun`, `setq`, `strcat`, `findfile`, `open`, `write-line`, `close`, `princ`, `strlen`, `itoa`, `getvar`, `vl-load-com`, `vlax-product-key`
were typed; the host echoed `defun` as the upper-case symbol name; `fboundp` is not a function there. Everything below is **unverified**:

1. `vl-catch-all-apply` applied to a built-in by quoted symbol (`'open`, `'strlen`, `'ascii`, `'findfile`, `'close`, `'vl-load-com`, `'vlax-product-key` with a `nil` argument list), to a user function by quoted symbol, and **to a function symbol received as a parameter** (`fn` in P2-E13, bound to `'ct21d-p2-lines` or `'close`); and that an arity error is returned as an error object instead of aborting.
2. **`ascii` applied to the string produced by the `\U+00E9` escape**, through `vl-catch-all-apply 'ascii` (P2-E08): whether the escape is interpreted there, and what code `ascii` returns (a code point, a byte, or the backslash) are unknown.
3. The text returned by `vl-catch-all-error-message`; that `vl-catch-all-error-p` is false for `nil` and for a file descriptor.
4. `(type r)` returning `FILE` for a descriptor and the comparison `(= (type r) 'FILE)` (P2-E06, P2-E14); `(type k)` returning `STR`; `(type nil)`; `vl-prin1-to-string` of `nil`, of a string (quoted, backslashes doubled), of a number and of a type symbol. A descriptor is never passed to `vl-prin1-to-string`.
5. The `\U+00E9` escape in a string literal pasted on the command line (four hex digits; P2-E09 uses `\U+00E9Z` so that no hexadecimal digit follows); what `write-line` does with it.
6. The return values of `write-line` (assumed the string) and `close` (assumed `nil`); `write-line` accepting the descriptor.
7. `findfile` with an absolute forward-slash path, with a path that does not exist (v1 `[HOST-TO-CONFIRM]`), and with `C:/Windows/System32/kernel32.dll`; and that `findfile` returns a string whose prin1 text satisfies the gate line `P2-E11 VALUE: "`.
8. `(vl-catch-all-apply 'vl-load-com nil)`: whether `vl-load-com` can be applied by symbol and what it returns.
9. `(substr k 1 9)` when `k` is shorter than 9 characters; `strcase`; `vl-string-search` with a pattern and a string; `itoa`; the string literal `"Software\\"` (an escaped backslash).
10. `(princ)` with no argument printing nothing and suppressing the echo of the returned value; `cond` with a `T` clause; `if` without an else branch.
11. **Line length.** Every expression is at most 255 characters; the command line's behaviour with lines of that length (the longest is P2-E24) was not verified, and neither was a possible limit below 255. An unbalanced continuation prompt such as `(_>` is a stop (section 4 item 5).
12. Whether `open` with mode `"w"` returns `nil` (no error) when the folder is missing or not writable.
13. `text`, `key`, `id`, `w`, `fn`, `r`, `k` and `x` as parameter names (the v1 definitions using `text` and `key` were echoed as accepted by the host, but the later calls failed).
14. The quoted list arguments `'("w")` and `'("w" "UTF-8")`, and `(cons p args)`.
15. **Nested one-line helpers**: a helper defined on one line that calls another defined on an earlier line (P2-E14 calls P2-E13; P2-E15 calls P2-E14; P2-E24 calls P2-E23; P2-E23 calls P2-E22 and P2-E21), with the same effect order as the single long helpers they replace (guard, open, print the open line, write-line, close).
16. One-line `defun` forms with several body forms and `/` local variables (P2-E12, P2-E14, P2-E15, P2-E24).
17. **Argument evaluation versus function lookup.** In the body of `ct21d-p2-try` (P2-E15) the call `(ct21d-p2-after id (vl-catch-all-apply 'open ...))` has the `open` as an argument, and that argument (the `open`) may be evaluated **before** the host discovers that `ct21d-p2-after` is undefined
    (for example because P2-E14 failed). Then a descriptor could be opened and never closed, the file would exist while the screen shows an error **before** any `open` line, and the reading is `UNKNOWN` by the section 6.5 row
    "a file exists offline but the screen shows no descriptor". The order is not verified on the host.
18. **"AutoCAD does not execute a paste"** (section 4 item 4) is a statement about host behaviour that was not verified; it is an operator expectation, and the stop rule of section 4 item 4 (a result without Enter is an OPERATOR_DEVIATION) is what covers its failure.

### 11.1 Residual limits of the leakage guard (undetectable by pattern)

The analyzer folds Unicode look-alikes (NFKC, zero-width and combining characters, dash-like characters) and checks escaped, hex-encoded and split views, but a pattern guard **cannot** see: base64, reversed, ROT13, base32 or
decimal-code encodings of the product key; a lone OS build number such as `26200`; a plain profile name such as `VANILLA_ACME` (without the word `profile`); chains of path fragments separated by any separator other than the folded ones (U+037E, U+FE54 and U+FF1B are folded to a semicolon). The protection against
these is **not** the pattern guard: it is the closed output vocabulary (each P2 line has a fixed shape), the fact that **P2 never prints any of those values**, and the operator rule that **the operator transcribes nothing that P2 did not print**.

## 12. Status

```text
P2_TEXT                       = DRAFT FOR ARCHITECT EXACT REVIEW     C2_TEXT = NOT_WRITTEN
P2_EXECUTION                  = NOT_AUTHORIZED     H10_RUN_ID = NOT_ASSIGNED     EVIDENCE_CHILD = NOT_CREATED     HOST_RUN_STARTED_BY_THIS_TEXT = FALSE
PHASE2_CAPTURE_FOR_P2         = WAIVED (Coordinator ruling), replaced by the CAD-MANAGER PRE-HOST ATTESTATION (not delivered)
I1_PROTOCOL_V1                = UNCHANGED (git blobs 3a77ba62da77dfa2fc8bd2ffeb176a9454864193 and a9081cff612b2290c35c86c0ded72f42c62b5e5a)
RUN HGP-H1-20261001T225354Z-01: RUN_STATUS = INVALID     RETRY = NOT_AUTHORIZED     SEAL = HELD
S1_A_RETRY                    = NOT_AUTHORIZED     NEW_H1_RUN_ID = NOT_AUTHORIZED
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```
