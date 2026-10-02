# I-1 observation protocol, family 2, artifact P2 — compatibility qualification text (draft, not an instrument)

```text
DOCUMENT        = I-1 protocol family 2, artifact P2 (typed AutoLISP compatibility qualification). Protocol text, not an instrument
FAMILY          = I1-F2 (tuple field i1ProtocolFamily). Artifact P2 only. Artifact C2 is NOT WRITTEN and is not implied by this text
CLASSIFICATION  = NON_GOVERNING_PROTOCOL_COMPATIBILITY_QUALIFICATION
ACTIVITY        = H5        RUN ID PATTERN = ^HGP-H5-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$        (future RunIds: HGP-H5-<yyyymmddThhmmssZ>-<nn>)
GOVERNING       = FALSE     NOT an S1-A retry     NOT product evidence     NO RackCad     NO NETLOAD     NO APPLOAD     no LISP loaded from a file
STATUS          = DRAFT FOR ARCHITECT EXACT REVIEW. NOT REVIEWED. NOT AUTHORIZED. NOT EXECUTED ON ANY HOST. NOTHING IN THIS REPOSITORY RUNS IT
AUTHORITY       = Coordinator ruling "I-1 VERSIONING / P2 DESIGN GATE" (decisions sections 254 and 255); Architect micro-review
                  docs/initiatives/I-52-ct21d-i1-protocol-versioning-review-v1.md (sha256 8b99f30d650c2377da8b64977c9bcbc4dff06d601352af7c4d0a9733a2950735),
                  rulings AR11-01..08 accepted. The versioned erratum docs/initiatives/I-52-ct21d-host-gate-execution-package-v3-erratum-1.md carries the
                  package-level consequences (activity H5, OBSERVED_DIFFERS, tuple fields)
HISTORY         = I-1 v1 stays byte-for-byte as it is and is not referenced as text to execute:
                  eng/research/I52Ct21dHostFacts/protocol/I-1-observation-protocol.md   git blob 3a77ba62da77dfa2fc8bd2ffeb176a9454864193   sha256 a608833e72d9a0401d8aa3c33496a6f8f3d098426ac8e72b3a563187df4037c8
                  eng/research/I52Ct21dHostFacts/protocol/i1-os-observation.ps1         git blob a9081cff612b2290c35c86c0ded72f42c62b5e5a   sha256 662dd9ab686427077e8d178466ee2a625d494dad9592fe7f582202148d23ceb3
                  (sha256 = of the stored blob bytes)
RUN -01         = HGP-H1-20261001T225354Z-01 stays RUN_STATUS = INVALID, RETRY = NOT_AUTHORIZED, SEAL = HELD. This text neither repairs nor reinterprets it
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```

## 1. Purpose

Run `HGP-H1-20261001T225354Z-01` stopped with `; error: too many arguments` on the typed I-1 v1 sequence. The command-line screenshot (operator
supplied, sha256 `9837dcd08445a3cc53954dbf91583a0485d76414e0e03d67a17c14d90b089818`) shows these facts and no more:

- the multi-line `defun` forms were pasted and echoed (`CT21D-PUT`, `CT21D-ATTR` are echoed; nested `(_>` prompts appear);
- `(fboundp 'ct21d-attr)` printed `; error: no function definition: FBOUNDP`;
- `(ct21d-attr "AutoCadProduct" (vlax-product-key))`, the same call with `"AutoCadProfile" (getvar "CPROFILE")` and, later, with `"TRUSTEDPATHS"`
  printed `; error: too many arguments`;
- a `_pasteclip` command was started by a paste and its `Specify insertion point:` prompts consumed pasted text.

**The cause is not established.** Candidates: (a) the host's `open` rejects the third (encoding) argument; (b) an arity or definition effect of the paste
(a user function called with more arguments than its parameter list raises the same message); (c) something else. P2 characterizes, on the designated
machine, the behaviours that v1 left `[HOST-TO-CONFIRM]`, **separating (a) from (b)** by an explicit control, so that the later governed text (C2)
contains no construct whose behaviour is unknown. P2 decides nothing about the cause beyond what its own raw results show.

## 2. What P2 is and is not

- P2 is typed AutoLISP, one expression per paste, from the closed allowlist of section 5. It is read-only except for **two new files** under the
  P2 evidence root.
- P2 **reads and writes no governed S1-A attribute value**. It does not record, print or store `MachineGuid`, `OsVersionBuild`, the value of
  `(vlax-product-key)`, `CPROFILE`, `SECURELOAD` or `TRUSTEDPATHS`. The characterization of `vlax-product-key` records only its **shape** (type, length,
  whether it starts with `Software\`, whether it contains `WOW6432Node`) and never its value (P2-E18, P2-E19).
- P2 produces no label input, no `raw-*.txt`, no `declared-length-*.txt`, no `machine-label.json`. It is not S1-A, not a retry of S1-A, not a governing
  run and not product evidence. No RackCad assembly, no instrument, no `NETLOAD`, no `APPLOAD`, no `load` of a file; the only code it defines is the
  `ct21d-p2-*` AutoLISP symbols typed from section 5.
- Evidence root: `D:/I52-CT21D-HOST/evidence/<H5-RunId>` (`NeedScratch = false`, `NeedCopy = false`). The directory is created by the CAD manager per part 2
  of the governed ACL plan. **P2 never creates a directory.** The only files P2 creates are `p2-a1.txt` and `p2-a2.txt` there, create-new.

## 3. Preconditions (none is satisfied now)

A P2 occurrence may start only when **all** of the following hold. Today none holds: P2 is a draft; no review, no authorization, no RunId, no evidence child.

1. The Coordinator's **per-occurrence written authorization** names: a concrete H5 RunId; the `sessionId`; the `expected-inputs` file (path and sha256); `i1ProtocolFamily`
   and `i1ProtocolLedgerSha256` (the sha256 of `HASHES-I1-V2.txt` as ruled); the substituted line P2-E01 and its SHA-256 (section 4, item 6); the evidence root;
   the custody location of the screenshots. The cause is recorded as `tooling/protocol`.
2. An **empty** evidence child exists, created by the CAD manager with the ACL of part 2 of the governed ACL plan.
3. **Zero** `acad.exe` processes before start; one fresh process.
4. The **phase-2 capture is VALID** before the first expression (package v3 5.1 item 16 by analogy).
5. `TRUSTEDPATHS` and the declared set are **unchanged** (established by the CAD manager's own records; P2 reads none of them).
6. The exact bytes of this family were reviewed by the Architect and by an independent source reviewer (AR11-08), and `HASHES-I1-V2.txt` is the ledger that review named.

## 4. Delivery discipline

1. **One expression per paste.** Never a multi-line block. The source of the text is `P2-ALLOWLIST.txt` (line N is P2-EN) or the table of section 5, both hashed; never chat.
2. Click the AutoCAD **command line** first so that it has focus. Copy exactly one line **without its terminator**.
3. Before pressing Enter, verify the pasted text character by character against the hashed line. If AutoCAD executes a paste without waiting for Enter, verify the echo
   afterwards; a mismatch is an operator deviation (stop).
4. **Closed allowlist.** Only the 19 expressions of section 5, once each, in order. No ad-hoc diagnostics (`fboundp`, `type`, `princ` of a symbol, anything). No
   repetition, no re-entry, no retry, no repair, no fallback.
5. An `; error:` printed by an allowlisted expression is **an outcome** (recorded verbatim), not a deviation, and the operator **does not** re-enter or fix it. The next
   expression is entered anyway; dependent expressions may print their own errors, which are also outcomes.
6. The only variable token in the whole text is `<RUNID>` in P2-E01. The Coordinator's authorization substitutes it; its pattern `^HGP-H5-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$`
   is fixed. The hashed text is the **template**; the authorization names the substituted P2-E01 line and its SHA-256 (UTF-8 bytes of the line followed by one LF).
7. **Stop** (no repair, no retry, log, inform the Coordinator): any command or expression not in the allowlist; any text that differs from the hashed line; any
   `_pasteclip`, dialog or prompt that is not part of the text (a `(_>` continuation prompt means an unbalanced paste); a `REFUSED` result; the conditions of section 8.2.
8. **Custody of screen results.** Results printed on the command line are not files. After P2-E19, **before closing AutoCAD**, the operator opens the text window (F2), makes
   it show the full output of P2-E01..P2-E19 in order, and takes one or more screenshots covering all of it in order. Each screenshot is stored outside the repository with the other raw
   evidence of the run, its SHA-256 is recorded, and it is **not edited, cropped or annotated** (an annotated copy is a separate file and is not evidence). Output lost by scrolling is
   `UNKNOWN`. The operator may transcribe the lines into the optional screen-results JSON of the offline analyzer; the screenshots govern any difference.
9. **Stated plainly:** if neither candidate A1 (P2-E13) nor A2 (P2-E14) is accepted by the host, **no file exists and the results of P2 exist only on the screen**; they are then
   custodied only by the screenshots of item 8.

## 5. The closed allowlist (every expression the operator may type)

Exact text, one expression per paste, in this order. `P2-ALLOWLIST.txt` holds the same 19 lines (a test of the family checks that both sources are identical). Test groups: **A** open signature
and the arity control, **B** write-line terminator and encoding (judged offline), **C** `strlen`, **D** `vlax-product-key` shape, **F** `findfile`. Setup and helper definitions have no group.

| ID | Exact text (AutoLISP, one line) | Purpose |
|---|---|---|
| P2-E01 | `(setq ct21d-p2-root "D:/I52-CT21D-HOST/evidence/<RUNID>/")` | Setup: sets the global `ct21d-p2-root` to the P2 evidence root (the only variable token is `<RUNID>`). Echo shows the string, which the operator compares with the authorization |
| P2-E02 | `(defun ct21d-p2-ctl (name text / p f) (setq p name f text) (strcat p "-" f))` | A control: defines a user function with the same parameter shape as v1 `ct21d-put` (two parameters, then locals after `/`, parameter named `text`) |
| P2-E03 | `(progn (princ (strcat "\nP2-E03 " (ct21d-p2-ctl "AB" "CD"))) (princ))` | A control: calls it with two arguments, directly (no catch), so the host's own error is printed verbatim. Separates the "arity/definition" hypothesis from the `open` hypothesis |
| P2-E04 | `(defun ct21d-p2-ctl2 (key text) (ct21d-p2-ctl (strcat "raw-" key ".txt") text))` | A control: defines a function with the shape of v1 `ct21d-attr` (two parameters, no locals) that calls the first one with a computed first argument |
| P2-E05 | `(progn (princ (strcat "\nP2-E05 " (ct21d-p2-ctl2 "K" "V"))) (princ))` | A control: calls it with two constant arguments (no governed read), directly. Reproduces the v1 nested-call shape that failed |
| P2-E06 | `(defun ct21d-p2-show (r) (if (vl-catch-all-error-p r) (strcat "ERROR: " (vl-catch-all-error-message r)) (strcat "VALUE: " (vl-prin1-to-string r))))` | Helper: returns `ERROR: <host message>` for an error object, otherwise `VALUE: <prin1 text>`. Used by the later probes to print the host's own error text verbatim |
| P2-E07 | `(progn (princ (strcat "\nP2-E07 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-absent.txt")))))) (princ))` | F: `findfile` on an absent path inside the evidence root |
| P2-E08 | `(progn (princ (strcat "\nP2-E08 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list "C:/Windows/System32/kernel32.dll"))))) (princ))` | F: `findfile` on a path that exists on every Windows installation (read-only existence test; nothing is read from the file) |
| P2-E09 | `(progn (princ (strcat "\nP2-E09 " (ct21d-p2-show (vl-catch-all-apply 'strlen (list "ABC"))))) (princ))` | C: `strlen` of `"ABC"` (control) |
| P2-E10 | `(progn (princ (strcat "\nP2-E10 " (ct21d-p2-show (vl-catch-all-apply 'strlen (list "A\U+00E9Z"))))) (princ))` | C: `strlen` of a string with one `\U+00E9` escape (characters versus bytes; also shows whether the escape is interpreted) |
| P2-E11 | `(defun ct21d-p2-lines (f) (write-line "P2-ASCII-LINE-1" f) (write-line "P2-ACCENT-\U+00E9-END" f) (write-line "P2-ASCII-LINE-3" f))` | Helper: writes the three fixed test lines with `write-line` (one ASCII line, one line with the `\U+00E9` escape, one ASCII line); closes nothing |
| P2-E12 | `(defun ct21d-p2-try (id name args / p r) (setq p (strcat ct21d-p2-root name)) (if (findfile p) (princ (strcat "\n" id " REFUSED, exists: " name)) (progn (setq r (vl-catch-all-apply 'open (cons p args))) (princ (strcat "\n" id " open " (ct21d-p2-show r))) (if (= (type r) 'FILE) (progn (princ (strcat "\n" id " write-line " (ct21d-p2-show (vl-catch-all-apply 'ct21d-p2-lines (list r))))) (princ (strcat "\n" id " close " (ct21d-p2-show (vl-catch-all-apply 'close (list r))))))))) (princ))` | Helper: create-new guard with `findfile` (returns REFUSED if the target exists), `open` inside `vl-catch-all-apply`, prints the host's result, writes the lines and closes only if a file descriptor was returned |
| P2-E13 | `(ct21d-p2-try "P2-E13" "p2-a1.txt" '("w"))` | A, candidate A1: `(open <path> "w")` into `p2-a1.txt` |
| P2-E14 | `(ct21d-p2-try "P2-E14" "p2-a2.txt" '("w" "UTF-8"))` | A, candidate A2: `(open <path> "w" "UTF-8")` into `p2-a2.txt` |
| P2-E15 | `(progn (princ (strcat "\nP2-E15 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-a1.txt")))))) (princ))` | F: `findfile` on `p2-a1.txt` after A1 |
| P2-E16 | `(progn (princ (strcat "\nP2-E16 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-a2.txt")))))) (princ))` | F: `findfile` on `p2-a2.txt` after A2 |
| P2-E17 | `(progn (princ (strcat "\nP2-E17 " (ct21d-p2-show (vl-catch-all-apply 'vl-load-com nil)))) (princ))` | D precondition: `vl-load-com` (as v1 did), applied inside the catch so any error text is captured |
| P2-E18 | `(defun ct21d-p2-shape (id k) (princ (cond ((vl-catch-all-error-p k) (strcat "\n" id " ERROR: " (vl-catch-all-error-message k))) ((/= (type k) 'STR) (strcat "\n" id " type=" (vl-prin1-to-string (type k)))) (T (strcat "\n" id " type=STR len=" (itoa (strlen k)) " startsExact=" (if (= (substr k 1 9) "Software\\") "T" "nil") " startsAnyCase=" (if (= (strcase (substr k 1 9)) "SOFTWARE\\") "T" "nil") " hasWowAnyCase=" (if (vl-string-search "WOW6432NODE" (strcase k)) "T" "nil"))))) (princ))` | D helper: prints only the **shape** of its argument (type, length, three booleans), never the value |
| P2-E19 | `(ct21d-p2-shape "P2-E19" (vl-catch-all-apply 'vlax-product-key nil))` | D: `vlax-product-key` inside the catch, passed to the shape helper; the value is neither printed nor stored |

Constraints stated for the reviewer (and checked by the family tests): every line is ASCII, a single line, with balanced parentheses; every defined or set global name starts with `ct21d-p2-`; no
line calls a function that writes anything except `open`/`write-line`/`close` in P2-E12 and `princ`; no `load`, `command`, `setvar`, `vl-registry-*`, `vla-*` or `vlax-*` call other than the one
`vlax-product-key` of P2-E19; the only paths used are the evidence root of P2-E01, `p2-absent.txt`, `p2-a1.txt`, `p2-a2.txt` and the existence test of P2-E08.

## 6. Interpretation of every possible outcome

Terms (the package vocabulary, v2 8.2 / v3 5.2, as the erratum extends it): `OBSERVED` = the result was read and equals the **reference expectation** stated below; `OBSERVED_DIFFERS` =
the result was read and differs from it; `UNKNOWN` = the result could not be read (missing, ambiguous or without custody); `NOT_OBSERVABLE` = no source of this occurrence can show it. The reference
expectation is the assumption of I-1 v1 where v1 stated one; otherwise it is the drafter's expectation, labelled as such. **A difference is a characterization result, never `INVALID` by itself.**

### 6.1 Defining and setup expressions

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E01 | the echo is the string with the authorized RunId | proceed |
| P2-E01 | anything else | operator deviation: stop |
| P2-E02, E04, E06, E11, E12, E18 | the upper-case symbol name is echoed (v1 showed the same for `CT21D-PUT`) | the definition was accepted in this session |
| P2-E02, E04, E06, E11, E12, E18 | `; error: ...` or a `(_>` prompt | `(_>` = unbalanced paste: deviation, stop. An `; error:` is recorded verbatim as `ERROR_TEXT_VERBATIM`; the expressions that depend on that symbol will print their own errors; nothing is repaired |

### 6.2 Group A, the arity control (`userFunctionArityControl`)

Reference expectation (drafter's): both calls return the concatenated string.

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E03 | `P2-E03 AB-CD` | OBSERVED. A user function with v1's parameter shape (two parameters, locals after `/`, parameter `text`) accepts two arguments in this session |
| P2-E03 | `; error: too many arguments` (or any other `; error:` text) | OBSERVED_DIFFERS. The v1 parameter shape itself is rejected in this session: the "arity/definition effect" hypothesis is **supported**; A1/A2 and the helpers then print their own errors and are read with this in mind |
| P2-E05 | `P2-E05 raw-K.txt-V` | OBSERVED. The v1 nested-call shape works with constant arguments |
| P2-E05 | `; error: ...` | OBSERVED_DIFFERS, same reading as P2-E03 for the nested shape |
| P2-E03 / P2-E05 | any other text | OBSERVED_DIFFERS (an unexpected value), recorded verbatim |

`userFunctionArityControl` is `OBSERVED` only when both P2-E03 and P2-E05 are OK; `OBSERVED_DIFFERS` when either is not; `UNKNOWN` when either is missing or has no screenshot custody.

**Reading of combinations (no inference beyond this table; the cause of run -01 is not decided by P2):**

| Control (E03, E05) | A2 (E14) | Reading |
|---|---|---|
| OK | `open ERROR: too many arguments` (any `open ERROR:` text) | consistent with candidate (a): this host's `open` rejects the encoding argument. The host's own text is the evidence; P2 does not extend it to other builds |
| OK | descriptor returned | candidate (a) is **not supported** on this host; the cause of run -01 stays unexplained (paste effect or something else). C2 must still use only forms P2 proved |
| ERROR | any | candidate (b) is supported for the v1 parameter shape in this session; the A results are read with this limit |
| missing | any | `UNKNOWN`; no reading |

### 6.3 Group F, `findfile` (`findfileAbsent`, `findfilePresent`)

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E07 | `P2-E07 VALUE: nil` | `findfileAbsent` OBSERVED (v1 assumed nil for a path that does not exist; v1 left it `[HOST-TO-CONFIRM]`) |
| P2-E07 | `VALUE: "<string>"` | OBSERVED_DIFFERS: `findfile` returned a path for a path that does not exist (the create-new guard of P2-E12 would then also be unreliable) |
| P2-E07 | `ERROR: <text>` | OBSERVED_DIFFERS, text verbatim |
| P2-E08 | `VALUE: "<string>"` | `findfilePresent` OBSERVED (reference expectation, drafter's: a string for an existing file) |
| P2-E08 | `VALUE: nil` | OBSERVED_DIFFERS: `findfile` did not see an existing file at an absolute forward-slash path |
| P2-E08 | `ERROR: <text>` | OBSERVED_DIFFERS, text verbatim |
| P2-E15 / E16 | `VALUE: "<string>"` when the offline analysis finds the file; `VALUE: nil` when it does not | consistent: no change |
| P2-E15 / E16 | the opposite of the offline file state | `findfilePresent` becomes OBSERVED_DIFFERS (the guard sees a different state than the file system) |

### 6.4 Group C, `strlen` (`strlenSemantics`)

Reference expectation (v1 text: the offline helper counts Unicode scalar values and fails closed on a mismatch): characters.

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E09 | `VALUE: 3` | control; any other value or an error is OBSERVED_DIFFERS for the whole item |
| P2-E10 | `VALUE: 3` | OBSERVED: `strlen` counts characters; the escape is interpreted as one character |
| P2-E10 | `VALUE: 4` | OBSERVED_DIFFERS: `strlen` counts bytes (UTF-8 `C3 A9` for U+00E9) |
| P2-E10 | `VALUE: 9` | OBSERVED_DIFFERS: the `\U+00E9` escape was not interpreted (9 literal characters); read with group B |
| P2-E10 | any other value, or `ERROR:` | OBSERVED_DIFFERS, verbatim |

### 6.5 Group A and B, `open`, `write-line`, encoding and terminator

P2-E13 (A1, two arguments) and P2-E14 (A2, three arguments) are independent: both are entered, whatever the other printed. Each prints up to three lines: `open ...`, `write-line ...`, `close ...`.

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E13 | `open VALUE: #<file "...">` (a descriptor) and the file exists offline with the three expected lines | `openTwoArg` OBSERVED (reference expectation, drafter's: accepted) |
| P2-E13 | `open ERROR: <text>` | `openTwoArg` OBSERVED_DIFFERS; the host's text is the evidence; no file exists |
| P2-E13 | `open VALUE: nil` | `openTwoArg` UNKNOWN: `open` returned no descriptor and no error; the cause (path, ACL, host) is not separable on the screen. The directory precondition is rechecked by the CAD manager outside P2 |
| P2-E13 | `REFUSED, exists: p2-a1.txt` | stop (the evidence root was not empty, or `findfile` is unreliable); no retry; the Coordinator rules |
| P2-E14 | `open VALUE: #<file "...">`, and the offline bytes are UTF-8 without BOM with `C3 A9` | `openThreeArgUtf8` OBSERVED (v1 assumed exactly this) |
| P2-E14 | a descriptor, but the offline bytes are anything else (BOM, a single byte, another encoding, the escape not interpreted) | `openThreeArgUtf8` OBSERVED_DIFFERS; the accepted-but-different behaviour is the finding |
| P2-E14 | `open ERROR: <text>` | `openThreeArgUtf8` OBSERVED_DIFFERS, text verbatim (for example `too many arguments`) |
| P2-E14 | `open VALUE: nil` / `REFUSED` | as for P2-E13 |
| P2-E13 / E14 | `write-line ERROR: <text>` or `close ERROR: <text>` after a descriptor | recorded verbatim in the detail of the item; the descriptor may stay open until AutoCAD ends; no repair |
| (offline) | terminator of the files: LF or CRLF | `writeLineTerminator` OBSERVED (the offline label helper accepts exactly one trailing LF or CRLF) |
| (offline) | CR only, no terminator, or mixed | `writeLineTerminator` OBSERVED_DIFFERS |
| (offline) | no file exists | `writeLineTerminator` NOT_OBSERVABLE (the terminator is not shown on the command line) |

The terminator and the encoding / BOM are **not interpreted on the command line**: they are characterized **offline from the file bytes** by `Analyze-I1P2.ps1` (section 7).

### 6.6 Group D, `vlax-product-key` shape (`productKeyShape`)

| ID | Outcome as it appears | Reading |
|---|---|---|
| P2-E17 | any `VALUE:` | informational: `vl-load-com` ran inside the catch |
| P2-E17 | `ERROR: <text>` | recorded verbatim; P2-E19 may then fail with its own text |
| P2-E19 | `type=STR len=<n> startsExact=T startsAnyCase=T hasWowAnyCase=nil` | OBSERVED: a string, starts with exactly `Software\`, holds no `WOW6432Node` in any case (v1 assumption) |
| P2-E19 | `startsExact=nil` and `startsAnyCase=T` | OBSERVED_DIFFERS: the prefix differs in letter case (the digits of the value are not printed; only these booleans) |
| P2-E19 | any other boolean combination, a type other than STR, or `ERROR: <text>` | OBSERVED_DIFFERS, verbatim; the value is never printed |

## 7. Offline characterization from the files and the screenshots

`Analyze-I1P2.ps1` (PowerShell 7, offline) reads only `p2-a1.txt` and `p2-a2.txt` of the P2 evidence root, never writes to disk, and prints the analysis record as canonical JSON to stdout
(schema `schemas/ct21d.i1-p2.v1.json`). For each file it records existence, length, sha256, the hex of the first 64 bytes, whether a BOM is present, the terminator class
(LF, CRLF, CR, NONE, MIXED), the bytes of the accent field, and the encoding interpretation. The expected content (with `<T>` the terminator the host produced) is
`P2-ASCII-LINE-1<T>P2-ACCENT-<accent field>-END<T>P2-ASCII-LINE-3<T>`. The screen results come from an optional JSON file that transcribes the screenshots; without custody (screenshot sha256) the screen-derived items are
`UNKNOWN`. The analyzer refuses a transcription whose text looks like a governed value or names a governed attribute (so that a product key value or a system-variable value can never be copied into the record) and refuses to run unless every file of this family equals `HASHES-I1-V2.txt`. The analyzer decides no run status: it cannot see tuple drift, processes, dialogs or writes outside the root.

## 8. Status semantics

8.1 **A mismatch with a v1 assumption in P2 is a characterization result**, recorded as `OBSERVED_DIFFERS`. It is expected data (it is the purpose of P2). It is not `INVALID`. It is never repeated to obtain a better result.

8.2 **`INVALID` only for stop conditions:** tuple drift; another `acad.exe`; an unscripted dialog or prompt; a write outside the P2 evidence root; an expression outside the allowlist (or a text that differs
from the hashed line); a crash (the Coordinator rules `CRASH_FINDING` / `CRASH_ENVIRONMENT`). A `REFUSED` result is a stop that the Coordinator rules. In an `INVALID` run, facts not read stay `UNKNOWN`; evidence is retained.

8.3 **No retry.** A new P2 occurrence needs a new Coordinator authorization, a new RunId and a new evidence child. A changed byte of this family is a new version of the family.

## 9. Relation to C2

C2 (the governed capture text) is **not written** here. It will be written only after the evidence of an authorized P2 occurrence is reviewed, and will use only the forms P2 showed to work. In C2 the reviewed chosen
behaviour is **normative**: any mismatch of C2 with it is a stop, makes the run `INVALID`, and leaves the unread facts `UNKNOWN`. There is no silent fallback and no manual normalization in C2. If P2 shows that no file-write
form is accepted, C2 is not written and the governed capture needs another transfer mechanism that is itself ruled and versioned (the clipboard and the command-line window stay excluded as transfer mechanisms by v1).

## 10. What P2 does not decide

- The cause of the failure of run -01; whether `open` with an encoding argument is the cause.
- Whether any observed form is acceptable for C2, or what C2 contains.
- Whether the P2 evidence root is sealed, and the custody of the analysis record.
- Whether a new H1 attempt is authorized, the S1-A status, the label, or any governed attribute value.
- The numbers or versions of the host (build, profile); P2 characterizes this session only and extends nothing to other machines or builds.
- Whether the fixture activity that package v3 names "H5" keeps that number (the erratum reserves the RunId activity number H5 for P2; the fixture gate is later and not opened).

## 11. Constructs whose behaviour is NOT verified (flagged for the reviewer)

Nothing in this text was executed. Known only from the repository and from the screenshot: v1's forms `defun`, `setq`, `strcat`, `findfile`, `open`, `write-line`, `close`, `princ`, `strlen`, `itoa`, `getvar`, `vl-load-com`, `vlax-product-key`
were typed; the host echoed `defun` as the upper-case symbol name; `fboundp` is not a function there. Everything below is **unverified**:

1. `vl-catch-all-apply` applied to a built-in by quoted symbol (`'open`, `'strlen`, `'findfile`, `'close`, `'vl-load-com`, `'vlax-product-key` with a `nil` argument list) and to a user function by quoted symbol, and that an arity error is returned as an error object instead of aborting.
2. The text returned by `vl-catch-all-error-message`; that `vl-catch-all-error-p` is false for `nil` and for a file descriptor.
3. `vl-prin1-to-string` of a file descriptor (assumed `#<file "...">`), of `nil`, of a string (quoted).
4. `(type r)` returning `FILE` for a descriptor; `(type k)` returning `STR`.
5. The `\U+00E9` escape in a string literal pasted on the command line (four hex digits; P2-E10 uses `\U+00E9Z` so that no hexadecimal digit follows); what `write-line` does with it.
6. The return values of `write-line` (assumed the string) and `close` (assumed `nil`); `write-line` accepting the descriptor.
7. `findfile` with an absolute forward-slash path, with a path that does not exist (v1 `[HOST-TO-CONFIRM]`), and with `C:/Windows/System32/kernel32.dll`.
8. `(vl-catch-all-apply 'vl-load-com nil)`: whether `vl-load-com` can be applied by symbol and what it returns.
9. `(substr k 1 9)` when `k` is shorter than 9 characters; `strcase`; `vl-string-search` with a pattern and a string.
10. `(princ)` with no argument printing nothing and suppressing the echo of the returned value.
11. The longest lines (487 characters, P2-E12 and P2-E18) pasting intact on the command line; a possible command-line length limit was not verified.
12. Whether `open` with mode `"w"` returns `nil` (no error) when the folder is missing or not writable.
13. `text` and `key` as parameter names (the v1 definitions using them were echoed as accepted by the host, but the later calls failed).
14. The quoted list arguments `'("w")` and `'("w" "UTF-8")`, and `(cons p args)`.

## 12. Status

```text
P2_TEXT                       = DRAFT FOR ARCHITECT EXACT REVIEW     C2_TEXT = NOT_WRITTEN
P2_EXECUTION                  = NOT_AUTHORIZED     H5_RUN_ID = NOT_ASSIGNED     EVIDENCE_CHILD = NOT_CREATED     HOST_RUN_STARTED_BY_THIS_TEXT = FALSE
I1_PROTOCOL_V1                = UNCHANGED (git blobs 3a77ba62da77dfa2fc8bd2ffeb176a9454864193 and a9081cff612b2290c35c86c0ded72f42c62b5e5a)
RUN HGP-H1-20261001T225354Z-01: RUN_STATUS = INVALID     RETRY = NOT_AUTHORIZED     SEAL = HELD
S1_A_RETRY                    = NOT_AUTHORIZED     NEW_H1_RUN_ID = NOT_AUTHORIZED
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```
