# I-52 CT-21D — P2 operator package for the H10 occurrence `HGP-H10-20261002T061000Z-01` (PREPARED; NOT EXECUTED)

```text
RECORD_ID                = CT21D-P2-OPERATOR-PACKAGE-HGP-H10-20261002T061000Z-01
RUN_ID                   = HGP-H10-20261002T061000Z-01     ACTIVITY = H10
P2_EXECUTION            = NOT_AUTHORIZED     (this package is a preparation artifact; the Coordinator's exact authorization starts nothing by itself until the attestation is complete)
FAMILY                   = I1-F2 / artifact P2     LEDGER sha256 = bd49d0f4d7915d6126eadb63c8f80f620c920830d179c11979c184c6b564bd5a
PASTE_SOURCE_FILE        = P2-PASTE-SOURCE-HGP-H10-20261002T061000Z-01.txt   (25 lines, LF; line N is P2-EN; differs from P2-ALLOWLIST.txt ONLY in line 1)
PASTE_SOURCE_SHA256      = a037a20207935bdae7f9aba388dd93327ecb2a891aa05489eb3e006e47012107
P2_E01_SUBSTITUTED_SHA256= a7961abd04d31aa112fd29b4e3c7bbaa0790e5aade698d88d5aabbae2383ea04     (UTF-8 bytes of the line followed by one LF)
ALLOWLIST_TEMPLATE_SHA256= eeb7ffc0f3119457951535003ac67662c955c2139bb43b81c502eb063f0d4b36
MAX_EXPRESSION_LENGTH    = 253     RACKCAD = NONE     NETLOAD = NONE     APPLOAD / load = NONE
```

Authority: P2 text sections 3.1, 4 and 5 (hashed). This package adds **nothing** to the protocol: it lists the 25 approved expressions in the approved order, one per step, with the stop rule. If this package and the hashed protocol ever differ, the hashed protocol governs and the operator STOPS.

## Rules that apply to EVERY step (protocol section 4)

1. One expression per paste; never a multi-line block. Source: `P2-PASTE-SOURCE-HGP-H10-20261002T061000Z-01.txt` (line N = P2-EN) or the exact text below; never chat. Copy one line **without** its terminator.
2. Before each paste: command line visibly focused (click it first), text cursor **visibly active** in it, prompt exactly `Command:`; the previous result has finished printing.
3. **No Ctrl+V unless the cursor is visibly active in the command line.** A paste elsewhere can start `PASTECLIP` (what happened in run -01).
4. A paste executes nothing. Verify the pasted text character by character against the line, **then** press Enter. A result that appears without Enter = `OPERATOR_DEVIATION`.
5. Stop signals: `_pasteclip`, `PASTECLIP`, `Specify insertion point`, any unexpected command or prompt, a `(_>` continuation prompt, an unallowlisted expression, text that differs from the line. Action: **ESC once, STOP, no retry, no recovery**; classify `OPERATOR_DEVIATION`; report to the Coordinator.
6. An `; error:` printed by an allowlisted expression is an **outcome**: do not re-enter, fix or diagnose; enter the next step (except the E01 and gate rules below).
7. **No ad-hoc diagnostics** (no `fboundp`, no `type`/`princ` of a symbol, nothing else). No repetition, no retry, no repair, no fallback.
8. **E01 exception:** if E01 does not echo the authorized root string (for example it printed `; error:`), STOP. Enter nothing further (`OPERATOR_DEVIATION`).
9. **Gate STOP_BEFORE_OPEN:** after E11 read both lines. Continue to E12..E19 only if E10 printed exactly `P2-E10 VALUE: nil` AND E11 printed a line beginning `P2-E11 VALUE: "`. Otherwise STOP: no open candidate executes; E12..E25 are INCOMPLETE; go to the final custody steps.
10. **Screenshots**: raw, unmodified screenshot of the AutoCAD window after E05, E09, E17, E19 and E25; record the SHA-256 of each; store outside the repository; never edit, crop or annotate.
11. **Final custody (after E25 or after a stop, BEFORE closing AutoCAD):** press F2, make the text window show the whole output in order, take raw screenshot(s) covering all of it (same rules). Scrolled-out output is UNKNOWN. Transcribe **result lines only**, nothing that P2 did not print.
12. Do not read, type or copy any value of CPROFILE, SECURELOAD, TRUSTEDPATHS, MachineGuid or OsVersionBuild. Do not copy any product key value (E25 prints only a shape).

## Sequence (25 steps, exact order)

### Step 01 — P2-E01
Paste exactly this one line (line 1 of the paste source):

```text
(setq ct21d-p2-root "D:/I52-CT21D-HOST/evidence/HGP-H10-20261002T061000Z-01/")
```
- **Expected on screen:** echo of the root string; compare with the authorization (D:/I52-CT21D-HOST/evidence/HGP-H10-20261002T061000Z-01/).
- **E01 check:** the echoed string must equal `D:/I52-CT21D-HOST/evidence/HGP-H10-20261002T061000Z-01/` exactly; otherwise STOP.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 02 — P2-E02
Paste exactly this one line (line 2 of the paste source):

```text
(defun ct21d-p2-ctl (name text / p f) (setq p name f text) (strcat p "-" f))
```
- **Expected on screen:** echo of the function name (CT21D-P2-CTL).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 03 — P2-E03
Paste exactly this one line (line 3 of the paste source):

```text
(progn (princ (strcat "\nP2-E03 " (ct21d-p2-ctl "AB" "CD"))) (princ))
```
- **Expected on screen:** line `P2-E03 AB-CD`, or the host error text verbatim (an outcome).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 04 — P2-E04
Paste exactly this one line (line 4 of the paste source):

```text
(defun ct21d-p2-ctl2 (key text) (ct21d-p2-ctl (strcat "raw-" key ".txt") text))
```
- **Expected on screen:** echo of the function name (CT21D-P2-CTL2).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 05 — P2-E05
Paste exactly this one line (line 5 of the paste source):

```text
(progn (princ (strcat "\nP2-E05 " (ct21d-p2-ctl2 "K" "V"))) (princ))
```
- **Expected on screen:** line `P2-E05 raw-K.txt-V`, or the host error text verbatim (an outcome).
- **SCREENSHOT CHECKPOINT 1:** raw screenshot of the AutoCAD window now; record sha256; store outside the repo; unedited.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 06 — P2-E06
Paste exactly this one line (line 6 of the paste source):

```text
(defun ct21d-p2-show (r) (cond ((vl-catch-all-error-p r) (strcat "ERROR: " (vl-catch-all-error-message r))) ((= (type r) 'FILE) "VALUE: FILE-DESCRIPTOR") (T (strcat "VALUE: " (vl-prin1-to-string r)))))
```
- **Expected on screen:** echo of the function name (CT21D-P2-SHOW).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 07 — P2-E07
Paste exactly this one line (line 7 of the paste source):

```text
(progn (princ (strcat "\nP2-E07 " (ct21d-p2-show (vl-catch-all-apply 'strlen (list "ABC"))))) (princ))
```
- **Expected on screen:** line `P2-E07 VALUE: 3`, or the host error text.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 08 — P2-E08
Paste exactly this one line (line 8 of the paste source):

```text
(progn (princ (strcat "\nP2-E08 " (ct21d-p2-show (vl-catch-all-apply 'ascii (list "\U+00E9"))))) (princ))
```
- **Expected on screen:** line `P2-E08 VALUE: <number>` or the host error text (recorded verbatim).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 09 — P2-E09
Paste exactly this one line (line 9 of the paste source):

```text
(progn (princ (strcat "\nP2-E09 " (ct21d-p2-show (vl-catch-all-apply 'strlen (list "A\U+00E9Z"))))) (princ))
```
- **Expected on screen:** line `P2-E09 VALUE: <number>` or the host error text (recorded verbatim).
- **SCREENSHOT CHECKPOINT 2:** raw screenshot of the AutoCAD window now; record sha256; store outside the repo; unedited.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 10 — P2-E10
Paste exactly this one line (line 10 of the paste source):

```text
(progn (princ (strcat "\nP2-E10 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-absent.txt")))))) (princ))
```
- **Expected on screen:** GATE: must print exactly `P2-E10 VALUE: nil`.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 11 — P2-E11
Paste exactly this one line (line 11 of the paste source):

```text
(progn (princ (strcat "\nP2-E11 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list "C:/Windows/System32/kernel32.dll"))))) (princ))
```
- **Expected on screen:** GATE: must print a line that begins `P2-E11 VALUE: "`.
- **GATE (item 9):** read the E10 and E11 lines now. Both conditions true → continue with step 12. Otherwise STOP_BEFORE_OPEN.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 12 — P2-E12
Paste exactly this one line (line 12 of the paste source):

```text
(defun ct21d-p2-lines (f) (write-line "P2-ASCII-LINE-1" f) (write-line "P2-ACCENT-\U+00E9-END" f) (write-line "P2-ASCII-LINE-3" f))
```
- **Expected on screen:** echo of the function name (CT21D-P2-LINES).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 13 — P2-E13
Paste exactly this one line (line 13 of the paste source):

```text
(defun ct21d-p2-say (id w fn r) (princ (strcat "\n" id " " w " " (ct21d-p2-show (vl-catch-all-apply fn (list r))))))
```
- **Expected on screen:** echo of the function name (CT21D-P2-SAY).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 14 — P2-E14
Paste exactly this one line (line 14 of the paste source):

```text
(defun ct21d-p2-after (id r) (princ (strcat "\n" id " open " (ct21d-p2-show r))) (if (= (type r) 'FILE) (progn (ct21d-p2-say id "write-line" 'ct21d-p2-lines r) (ct21d-p2-say id "close" 'close r))))
```
- **Expected on screen:** echo of the function name (CT21D-P2-AFTER).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 15 — P2-E15
Paste exactly this one line (line 15 of the paste source):

```text
(defun ct21d-p2-try (id name args / p) (setq p (strcat ct21d-p2-root name)) (if (findfile p) (princ (strcat "\n" id " REFUSED, exists: " name)) (ct21d-p2-after id (vl-catch-all-apply 'open (cons p args)))) (princ))
```
- **Expected on screen:** echo of the function name (CT21D-P2-TRY).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 16 — P2-E16
Paste exactly this one line (line 16 of the paste source):

```text
(ct21d-p2-try "P2-E16" "p2-a1.txt" '("w"))
```
- **Expected on screen:** line `P2-E16 open ...` then (only if a file was returned) `P2-E16 write-line ...` and `P2-E16 close ...`; or `P2-E16 REFUSED, exists`, or the host error verbatim.
- This step may create `p2-a1.txt` in the evidence child (create-new; the helper refuses if the file exists). Nothing else may be created.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 17 — P2-E17
Paste exactly this one line (line 17 of the paste source):

```text
(ct21d-p2-try "P2-E17" "p2-a2.txt" '("w" "UTF-8"))
```
- **Expected on screen:** same shape as E16 with the id P2-E17.
- This step may create `p2-a2.txt` in the evidence child (create-new; the helper refuses if the file exists). Nothing else may be created.
- **SCREENSHOT CHECKPOINT 3:** raw screenshot of the AutoCAD window now; record sha256; store outside the repo; unedited.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 18 — P2-E18
Paste exactly this one line (line 18 of the paste source):

```text
(progn (princ (strcat "\nP2-E18 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-a1.txt")))))) (princ))
```
- **Expected on screen:** line `P2-E18 VALUE: "<path>"` or `P2-E18 VALUE: nil` or a host error.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 19 — P2-E19
Paste exactly this one line (line 19 of the paste source):

```text
(progn (princ (strcat "\nP2-E19 " (ct21d-p2-show (vl-catch-all-apply 'findfile (list (strcat ct21d-p2-root "p2-a2.txt")))))) (princ))
```
- **Expected on screen:** line `P2-E19 VALUE: "<path>"` or `P2-E19 VALUE: nil` or a host error.
- **SCREENSHOT CHECKPOINT 4:** raw screenshot of the AutoCAD window now; record sha256; store outside the repo; unedited.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 20 — P2-E20
Paste exactly this one line (line 20 of the paste source):

```text
(progn (princ (strcat "\nP2-E20 " (ct21d-p2-show (vl-catch-all-apply 'vl-load-com nil)))) (princ))
```
- **Expected on screen:** line `P2-E20 VALUE: ...` or a host error (recorded verbatim).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 21 — P2-E21
Paste exactly this one line (line 21 of the paste source):

```text
(defun ct21d-p2-tf (x) (if x "T" "nil"))
```
- **Expected on screen:** echo of the function name (CT21D-P2-TF).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 22 — P2-E22
Paste exactly this one line (line 22 of the paste source):

```text
(defun ct21d-p2-pre (k) (strcat " startsExact=" (ct21d-p2-tf (= (substr k 1 9) "Software\\")) " startsAnyCase=" (ct21d-p2-tf (= (strcase (substr k 1 9)) "SOFTWARE\\"))))
```
- **Expected on screen:** echo of the function name (CT21D-P2-PRE).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 23 — P2-E23
Paste exactly this one line (line 23 of the paste source):

```text
(defun ct21d-p2-str (k) (strcat " type=STR len=" (itoa (strlen k)) (ct21d-p2-pre k) " hasWowAnyCase=" (ct21d-p2-tf (vl-string-search "WOW6432NODE" (strcase k)))))
```
- **Expected on screen:** echo of the function name (CT21D-P2-STR).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 24 — P2-E24
Paste exactly this one line (line 24 of the paste source):

```text
(defun ct21d-p2-shape (id k) (princ (cond ((vl-catch-all-error-p k) (strcat "\n" id " ERROR: " (vl-catch-all-error-message k))) ((/= (type k) 'STR) (strcat "\n" id " type=" (vl-prin1-to-string (type k)))) (T (strcat "\n" id (ct21d-p2-str k))))) (princ))
```
- **Expected on screen:** echo of the function name (CT21D-P2-SHAPE).
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

### Step 25 — P2-E25
Paste exactly this one line (line 25 of the paste source):

```text
(ct21d-p2-shape "P2-E25" (vl-catch-all-apply 'vlax-product-key nil))
```
- **Expected on screen:** ONE line `P2-E25 ...` with type/length/booleans only (never a value); a leaked-looking value = stop and report, do NOT transcribe it.
- **SCREENSHOT CHECKPOINT 5:** raw screenshot of the AutoCAD window now; record sha256; store outside the repo; unedited.
- **Stop rule:** any item-5 signal ⇒ ESC once, STOP. Otherwise (including an `; error:` outcome) proceed to the next step only after this one finished printing.

## After step 25 (or after a stop)

1. F2 text-window custody screenshots (rule 11), sha256 each, outside the repository.
2. Do **not** type anything else into AutoCAD. Close AutoCAD normally (**NORMAL_MANUAL_CLOSE**) and report to the Coordinator; if AutoCAD ended any other way, say so — no crash classification is made here.
3. Offline analysis (after the Coordinator's review gate, outside AutoCAD): `pwsh -File Analyze-I1P2.ps1 -EvidenceDirectory D:\I52-CT21D-HOST\evidence\HGP-H10-20261002T061000Z-01 -RunId HGP-H10-20261002T061000Z-01 [-ScreenResults ...]` — read-only; it writes nothing.

```text
P2_EXECUTION = NOT_AUTHORIZED     C2_STATUS = WAIT_FOR_P2_EVIDENCE     H1_STATUS = INVALID
```
