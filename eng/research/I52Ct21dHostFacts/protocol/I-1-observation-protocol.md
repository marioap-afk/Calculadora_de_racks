# I-1 observation protocol (machine and session layer) — protocol text, not an instrument

```text
STATUS      = PROTOCOL TEXT FOR THE CAD MANAGER. NOT EXECUTED ON ANY HOST. NOTHING IN THIS REPOSITORY RUNS IT.
AUTHORITY   = I-52 design section 3.2 (A1-R1..A1-R6), 3.3, 5.2 row I-1; decisions section 239: R-H accepted (a) proposal (i),
              (b) yes as typed commands and a documented protocol, (c) yes
HF ROWS     = HF-M1..HF-M11 (design 3.4); HF-M7 (the label) is computed offline by the label helper
SCOPE       = BA-11 row 1 "read-only, no instrument, no manifest": reads only. The only writes are NEW files under the evidence
              folder (the disclosed Q-O-0(b) element, ruled inside "solo lectura" by decisions section 239)
```

The CAD manager types or runs the lines below on the designated machine at a host gate that the Coordinator has authorized. Every
raw file is one string in UTF-8 **without BOM** plus **one line terminator**; nothing is trimmed, case-folded or normalized.
`[HOST-TO-CONFIRM]` marks a behaviour of Windows or AutoCAD that no file of this repository establishes: the raw output of the
designated machine governs, and a mismatch makes the attribute `UNKNOWN`, never a corrected guess.

Order (design 3.2 "Ordering consequence"): the trusted folder for every instrument and every RackCad build is decided and set
**before** the six strings are read (decisions section 239, R-F option (a)). Copying text from the command-line window or the clipboard
is **not** a transfer mechanism.

## 1. The six strings

File names are fixed by the label helper (`tools label`): `raw-<Key>.txt`, and for the AutoCAD reads `declared-length-<Key>.txt`.

| Id | Key | How (by the CAD manager) | Raw file | Declared length |
|---|---|---|---|---|
| A1-R1 | `MachineGuid` | `i1-os-observation.ps1 -Mode MachineStrings` (64-bit view of `HKLM\SOFTWARE\Microsoft\Cryptography`, value `MachineGuid`) | `raw-MachineGuid.txt` | optional |
| A1-R2 | `OsVersionBuild` | the same script: ONE read of `Win32_OperatingSystem.Version` (no join, no `UBR`) | `raw-OsVersionBuild.txt` | optional |
| A1-R3 | `AutoCadProduct` | typed AutoLISP (below), `(vlax-product-key)`; cross-check with the registry subkey of `HKLM\SOFTWARE\Autodesk\AutoCAD\R25.0` (64-bit view) whose `AcadLocation` is the running `acad.exe` (comparison rule of design 3.2, A1-R3; the extracted string is never case-folded) | `raw-AutoCadProduct.txt` | required |
| A1-R4 | `AutoCadProfile` | typed AutoLISP, `(getvar "CPROFILE")` | `raw-AutoCadProfile.txt` | required |
| A1-R5 | `SECURELOAD` | typed AutoLISP, `(itoa (getvar "SECURELOAD"))` (the decimal digits) | `raw-SECURELOAD.txt` | required |
| A1-R6 | `TRUSTEDPATHS` | typed AutoLISP, `(getvar "TRUSTEDPATHS")` (an empty string is a valid value) | `raw-TRUSTEDPATHS.txt` | required |

### Typed AutoLISP (one expression per attribute)

Replace `<E>` by the evidence folder with forward slashes. The guard makes the write **create-new**: the expression refuses when the file
exists (AutoLISP `open ... "w"` would otherwise truncate it).

```lisp
(vl-load-com)
(defun ct21d-put (name text / p f)
  (setq p (strcat "<E>/" name))
  (if (findfile p)
    (princ (strcat "\nREFUSED, exists: " name))
    (progn (setq f (open p "w" "UTF-8")) (write-line text f) (close f) (princ (strcat "\nwrote " name)))))
(defun ct21d-attr (key text)
  (ct21d-put (strcat "raw-" key ".txt") text)
  (ct21d-put (strcat "declared-length-" key ".txt") (itoa (strlen text))))
(ct21d-attr "AutoCadProduct"  (vlax-product-key))
(ct21d-attr "AutoCadProfile"  (getvar "CPROFILE"))
(ct21d-attr "SECURELOAD"      (itoa (getvar "SECURELOAD")))
(ct21d-attr "TRUSTEDPATHS"    (getvar "TRUSTEDPATHS"))
```

`[HOST-TO-CONFIRM]`: the encoding argument of `open`; the line terminator of `write-line`; `findfile` on a path that does not exist;
whether `strlen` counts characters or bytes (the offline helper counts Unicode scalar values and fails closed on any mismatch);
`(vlax-product-key)` availability after `(vl-load-com)`; the product key starts with `Software\` and holds no `WOW6432Node`.

Take `SECURELOAD`, `TRUSTEDPATHS` and the profile reads **before and after** the session (HF-M5, HF-M6, stop condition of design 2.4 S0 /
S8): use the same expressions with the file names suffixed by the phase if the CAD manager wants separate raw files; only the first
read (before any `NETLOAD`) enters the label.

## 2. The label (HF-M7), offline

```powershell
dotnet I52Ct21d.HostFacts.Tools.dll label <evidence-folder> --record
```

It prints the status of each attribute and `label: MC-<12 hex>` or `UNSET`; exit code 0 when set, 3 when unset. `--record` writes
`machine-label.json` (create-new; the clear strings are not copied into it). The label is computed only when all six are `OBSERVED`
(RFC 8785 serialization of the six keys, UTF-8, SHA-256, `MC-` + first 12 lowercase hex digits; BA-10 V8 section 2.2).

## 3. The session layer (HF-M8..HF-M11)

`i1-os-observation.ps1 -Mode Session ...` (Get-FileHash, Get-Process, Get-Item only; new files in the evidence folder):

| Phase | When | What |
|---|---|---|
| `SESSION_START` | before any `NETLOAD`, after AutoCAD starts | hashes of `acad.exe`, `AcDbMgd`, `AcMgd`, `AcCoreMgd`, the library and its private copy; the version resource; the process module list; other `acad.exe` processes |
| `AFTER_NETLOAD` | after each declared `NETLOAD` | the process module list (the instrument DLLs are declared; anything else is `INVALID` except host-loaded vendor modules, HF-M9) |
| `END` | before the process ends | the same hashes (the private copy must be byte-identical) and the module list |

The profile registry subtree digest (HF-M4b), the hardware identity, the Windows account and the catalog-folder hashes (HF-M10) are
captured by the CAD manager by their own commands and recorded in the tuple record; the Coordinator has not ruled whether HF-M4b lies
inside the G2 phrase "configuration inputs" (design 3.4, NEEDS-COORDINATOR), so this protocol does not prescribe it.

## 4. Closing the folder

After the instruments have written their records: `dotnet I52Ct21d.HostFacts.Tools.dll seal <evidence-folder>` (I-9).
