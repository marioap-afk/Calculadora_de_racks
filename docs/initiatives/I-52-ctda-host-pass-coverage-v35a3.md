# I-52 — CTDA_HOST_PASS(V35-A3): coverage assessment

> **Generated** from `I-52-ctda-host-pass-coverage-v35a3.json` by `docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.gen.py`. Documentation / analysis only: no AutoCAD, no probe, no source change.
> The JSON is authoritative; this file is its reviewable rendering. Decision record: `docs/automation/decisions/I-52.md` section 196.

## 1. Result

```text
CTDA_HOST_PASS(V35-A3) = NOT EVALUABLE   (not TRUE; no disqualifying FAIL/UNKNOWN recorded, so not FALSE)
Clause 1 fixture identity                 INCOMPLETE
Clause 2 complete execution               INCOMPLETE   native 6/100 governing (2 deferred, 92 not executed); managed 0/18
Clause 3 result completeness              INCOMPLETE
Clause 4 observability DA-P7/P8/P10       INCOMPLETE
Clause 5 no FAIL/UNKNOWN                  INCOMPLETE   0 FAIL, 0 UNKNOWN among the 6 governing results
```

## 2. Counts

| Class | Native (100) | Managed literal 18 | Combined 118 |
|---|---|---|---|
| PASS-GOVERNED | 6 | 0 | 6 |
| FAIL-GOVERNED | 0 | 0 | 0 |
| UNKNOWN-GOVERNED | 0 | 0 | 0 |
| EXECUTED-NON-GOVERNING | 0 | 0 | 0 |
| NOT-EXECUTED | 92 | 18 | 110 |
| DEFERRED | 2 | 0 | 2 |

*EXECUTED-NON-GOVERNING* is counted per ROW: a row whose only executions are non-governing. There is none (every ProbeId with a non-governing execution also has a governing PASS). Per EXECUTION there are 11 governing-tuple attempts: 6 governing, 5 non-governing (section 3).

HEC-V27-C1 section 2 holds **27** managed ProbeId rows. The literal definition counts **18** = probes 01..15 with 09C/09E and 10S/10M/10SM split. The nine `16{S,M,SM}-{SEND,CONTEXT,IDLE}` scheduler rows are also in the catalog and are shown separately (all NOT-EXECUTED); see open question Q2.

## 3. The 11 governing-tuple executions

| Id | ProbeId | Package | Freeze of package | R3 execution tuple | Engine result | Governing | Reason / ruling | Section |
|---|---|---|---|---|---|---|---|---|
| E01 | 09N-B | `6e445fa8` | V35-A2 era (no tuple record in the evidence file) | `—` | PASS-T (engine) | no | NOT VALID: the operator confirmed that the save-changes dialog appeared and pressed 'Don't save' by hand (Coordinator ruling, section 181) | 176 |
| E02 | 02NDBMOD-S | `c4037098` | V35-A2 era (no tuple record in the evidence file) | `—` | PASS-S (engine) | no | NOT ACCEPTED: D-2 (corrupt CommandIdentity, use-after-free in I52Ctda_TokenSet) | 178 |
| E03 | 02NDBMOD-S | `2887d6c5` | V35-A2 | `7BC15FC027` | UNKNOWN | no | NOT VALID: D-3 led to an interactive exit and Save changed the scratch DWG (section 181) | 180 |
| E04 | 09N-B | `e864a093` | V35-A2 | `F7506504E6` | PASS-T | YES | VALID PASS-T (Coordinator ruling on H-1, section 184) | 183-184 |
| E05 | 02NDBMOD-S | `e864a093` | V35-A2 | `18249CD1F7` | PASS-S | YES | VALID PASS-S | 184 |
| E06 | 02NO-S | `e864a093` | V35-A2 | `C57CD89511` | PASS-S | YES | VALID PASS-S | 185 |
| E07 | 16N-S | `e864a093` | V35-A2 | `CA83700D91` | UNKNOWN | no | NOT VALID: operator interaction on the host (independent of the engine UNKNOWN) | 186 |
| E08 | 16N-S | `e864a093` | V35-A2 | `7F8D78F557` | UNKNOWN | no | Historical A2 UNKNOWN (UNK-MARKER-BINDING): D-4 was a V35 contract defect (section 187); not re-graded (section 188) | 186 |
| E09 | 16N-S | `afa65bc0` | V35-A3 | `DBAD02491F` | PASS-S | YES | VALID PASS-S under V35-A3 (D-4 SEND anchor) | 191 |
| E10 | 10N-S | `afa65bc0` | V35-A3 | `70C35A6A34` | PASS-S | YES | VALID PASS-S under V35-A3 (D-4 FIXTURE anchor) | 192 |
| E11 | 16C-S | `afa65bc0` | V35-A3 | `E1DF4BA440` | PASS-S | YES | VALID PASS-S under V35-A3 (D-4 CMDCTX anchor) | 193 |

Evidence: `docs/automation/evidence/I-52-r3-canary-*.json` (paths and SHA-256 per execution in the JSON). The three A2-package results (E04-E06) are carried forward by decisions section 188 (`ROW_HASH_DELTA = 0/100`); they were not re-executed under the A3 tuple (open question Q3).

## 4. Clause evaluation

| Clause | Status | Why |
|---|---|---|
| 1 — fixture identity | **INCOMPLETE** | Native fixture-v35 / FEC-V35-1: the six governing executions ran packages whose freeze/plan identity declares the frozen V35 fixture (A3: D9FD41B4.., A2: 43DCE809..), but no evidence record carries an explicit fixture-identity field. The managed fixture HF-V31-1 with F-NOD and F-SRC (identities that exist only in the managed HF/HEC fixture contract, not in the eight native PersistentFixtureIdentity entries) has never been instantiated in any governed execution. |
| 2 — complete execution (100 native + 18 managed, exact governed identity/tuple) | **INCOMPLETE** | native 6/100 executed under a governing result (92 not executed, 2 deferred); managed 0/18 executed. The three A2-package results are carried forward, not re-executed under the A3 tuple. |
| 3 — result completeness (H-P1..H-P12, IN T1..T16, T14 OUT, every tagged row PASS) | **INCOMPLETE** | H-P satisfied: none; IN T satisfied: none (rows tagged and all PASS). No FAIL among governing results. |
| 4 — observability (DA-P7 / DA-P8 / DA-P10) | **INCOMPLETE** | DA-P7 surfaces read in a PASS: ['S'] (missing ['M', 'SM', 'NOD']); DA-P8 needs HEC row 11 (NOT-EXECUTED); DA-P10 needs HEC rows 14 and 15 (NOT-EXECUTED). |
| 5 — no FAIL / UNKNOWN anywhere in the governed matrix | **INCOMPLETE** | 0 FAIL-GOVERNED and 0 UNKNOWN-GOVERNED among the 6 governing results; nothing disqualifying is recorded. The absence cannot be established for the 110 rows not executed and 2 deferred. The two historical A2 UNKNOWN executions of 16N-S (E07, E08) are non-governing under A3 (sections 187/188). |

### 4.1 DA-P7 / DA-P8 / DA-P10

- DA-P7: surfaces required S, M, SM, NOD. Read in at least one PASS: **S**. Missing: **M, SM, NOD**. 09N-B is labelled Surface S/M in NPM-V35, but its VerifierIds are VER-T,VER-S: only S (and T) is read. NOD is a surface of managed row 11 only (native rows have no NOD surface). Nothing is inferred from the S/M label.
- DA-P8: satisfied only by HEC row 11 (F-NOD): NOT-EXECUTED.
- DA-P10: satisfied only by HEC rows 14 and 15 (F-SRC): NOT-EXECUTED.

### 4.2 DA-P / H-P tag coverage (native + managed literal 18; every tagged row must PASS)

| Tag | Rows tagged | of which native | of which managed | PASS-GOVERNED | Satisfied |
|---|---|---|---|---|---|
| P1 | 17 | 14 | 3 | 2 (02NO-S, 02NDBMOD-S) | no |
| P2 | 29 | 24 | 5 | 3 (09N-B, 02NO-S, 02NDBMOD-S) | no |
| P3 | 14 | 10 | 4 | 1 (09N-B) | no |
| P4 | 7 | 2 | 5 | 1 (02NO-S) | no |
| P5 | 1 | 0 | 1 | 0 (—) | no |
| P6 | 85 | 81 | 4 | 5 (09N-B, 10N-S, 16N-S, 16C-S, 02NDBMOD-S) | no |
| P7 | 0 | 0 | 0 | 0 (—) | no (cross-cutting property, no row carries it: see 4.1) |
| P8 | 1 | 0 | 1 | 0 (—) | no |
| P9 | 3 | 2 | 1 | 0 (—) | no |
| P10 | 2 | 0 | 2 | 0 (—) | no |
| P11 | 87 | 83 | 4 | 2 (16C-S, 02NDBMOD-S) | no |
| P12 | 82 | 75 | 7 | 3 (10N-S, 16N-S, 16C-S) | no |

### 4.3 Threat coverage (IN T1..T16; T14 OUT). `T2-DOC`, `T2-OBJ`, `T2-ENTITY` are counted under T2.

| Threat | Scope | Rows tagged | native | managed | PASS-GOVERNED | Satisfied |
|---|---|---|---|---|---|---|
| T1 | IN | 3 | 0 | 3 | 0 | no |
| T2 | IN | 98 | 96 | 2 | 5 | no |
| T3 | IN | 11 | 7 | 4 | 0 | no |
| T4 | IN | 11 | 9 | 2 | 1 | no |
| T5 | IN | 3 | 1 | 2 | 0 | no |
| T6 | IN | 4 | 2 | 2 | 0 | no |
| T7 | IN | 3 | 1 | 2 | 0 | no |
| T8 | IN | 16 | 12 | 4 | 2 | no |
| T9 | IN | 3 | 1 | 2 | 1 | no |
| T10 | IN | 2 | 0 | 2 | 0 | no |
| T11 | IN | 9 | 3 | 6 | 0 | no |
| T12 | IN | 6 | 3 | 3 | 0 | no |
| T13 | IN | 6 | 5 | 1 | 1 | no |
| T14 | OUT (diagnostic only; oracle: no state change) | 0 | 0 | 0 | 0 | n/a (OUT) |
| T15 | IN | 18 | 17 | 1 | 0 | no |
| T16 | IN | 63 | 63 | 0 | 2 | no |

## 5. Deferred probes (16A-S, 13A-SM)

- **16A-S** — family `H-APPCTX,H-DB`, chain `CHAIN-APPCTX`, anchor `APPCTX-EXEC-01`, surface `S`, verifiers `VER-S,VER-T`, threats `T2,T8,T16`, tags `P6,P11,P12`. APPCTX anchor characterization (D-4 text unchanged for APPCTX); deferred in decisions sections 194/195 and HANDOFF
- **13A-SM** — family `H-APPCTX`, chain `CHAIN-SYNC`, anchor `APPCTX-EXEC-01`, surface `S+M+SM`, verifiers `VER-ALL,VER-T`, threats `T13`, tags `P6,P11,P12`. APPCTX synchronous chain (lock/T from another context); deferred in decisions sections 194/195 and HANDOFF

## 6. Native matrix (100 rows)

Fixture identity for every native row: `fixture-v35 / FEC-V35-1`. "Anchor" is the row's `ExecutionContextId`; "family" is its `HeaderAuthority`. "D-4" is the A3 anchor characterization (SEND / FIXTURE / CMDCTX). Results are never transferred between families or anchors.

| ProbeId | Class | Family | Chain | Anchor | Surface | Tuple (pkg / freeze / exec) | A3 validity | Result | D-4 | DA-P7 read | Non-governing exec |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 02N | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | — | — | — | — | — | — |
| 04N | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S | — | — | — | — | — | — |
| 09N-A | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | — | — | — | — | — | — |
| 09N-B | PASS-GOVERNED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | `e864a093` / V35-A2 / `F7506504` | carry-fwd §188 | PASS-T | — | S | E01 |
| 09N-O | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | — | — | — | — | — | — |
| 09N-C | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | — | — | — | — | — | — |
| 09N-D | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | S | — | — | — | — | — | — |
| 10N-S | PASS-GOVERNED | H-ED | CHAIN-NONE | CB-EXEC-01 | S | `afa65bc0` / V35-A3 / `70C35A6A` | A3 | PASS-S | FIXTURE | S | — |
| 10N-M | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | M | — | — | — | — | — | — |
| 10N-SM | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | SM | — | — | — | — | — | — |
| 16N-S | PASS-GOVERNED | H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | S | `afa65bc0` / V35-A3 / `DBAD0249` | A3 | PASS-S | SEND | S | E07,E08 |
| 16N-M | NOT-EXECUTED | H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | M | — | — | — | — | — | — |
| 16N-SM | NOT-EXECUTED | H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| C15N16N-SM | NOT-EXECUTED | H-LOAD,H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| 02NO-S | PASS-GOVERNED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | S | `e864a093` / V35-A2 / `C57CD895` | carry-fwd §188 | PASS-S | — | S | — |
| 02NO-M | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NO-SM | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 02NO-CLOSE-SM | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 04NO-S | NOT-EXECUTED | H-OBJ | CHAIN-NONE | EXEC-OBSERVE | S | — | — | — | — | — | — |
| 04NO-UNDO-S | NOT-EXECUTED | H-OBJ | CHAIN-NONE | EXEC-OBSERVE | S | — | — | — | — | — | — |
| 02NE-M | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 10NDOC-WILL-SM | NOT-EXECUTED | H-DOC | CHAIN-NONE | CB-EXEC-01 | SM | — | — | — | — | — | — |
| 10NDOC-CHANGED-SM | NOT-EXECUTED | H-DOC | CHAIN-NONE | CB-EXEC-01 | SM | — | — | — | — | — | — |
| 10NDOC-VETO-SM | NOT-EXECUTED | H-DOC | CHAIN-NONE | EXEC-OBSERVE | SM | — | — | — | — | — | — |
| C2OBJ16N-S | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S | — | — | — | — | — | — |
| C2OBJ16N-M | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | M | — | — | — | — | — | — |
| C2OBJ16N-SM | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| C2ENT16N-M | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | M | — | — | — | — | — | — |
| C2DOCW16N-SM | NOT-EXECUTED | H-DOC,H-SEND | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| C2DOCC16N-SM | NOT-EXECUTED | H-DOC,H-SEND | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| 13A-SM | DEFERRED | H-APPCTX | CHAIN-SYNC | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 16A-S | DEFERRED | H-APPCTX,H-DB | CHAIN-APPCTX | APPCTX-EXEC-01 | S | — | — | — | — | — | — |
| 16A-M | NOT-EXECUTED | H-APPCTX,H-DB | CHAIN-APPCTX | APPCTX-EXEC-01 | M | — | — | — | — | — | — |
| 16A-SM | NOT-EXECUTED | H-APPCTX,H-DB | CHAIN-APPCTX | APPCTX-EXEC-01 | SM | — | — | — | — | — | — |
| 16C-S | PASS-GOVERNED | H-CMDCTX,H-APPCTX | CHAIN-SYNC-CMDCTX | CMDCTX-EXEC-01 | S | `afa65bc0` / V35-A3 / `E1DF4BA4` | A3 | PASS-S | CMDCTX | S | — |
| 16C-M | NOT-EXECUTED | H-CMDCTX,H-APPCTX | CHAIN-SYNC-CMDCTX | CMDCTX-EXEC-01 | M | — | — | — | — | — | — |
| 16C-SM | NOT-EXECUTED | H-CMDCTX,H-APPCTX | CHAIN-SYNC-CMDCTX | CMDCTX-EXEC-01 | SM | — | — | — | — | — | — |
| 02NAPP-S | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | — | — | — | — | — | — |
| 02NAPP-M | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NAPP-SM | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 02NTAS-ALL | NOT-EXECUTED | H-TX | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NTS-ALL | NOT-EXECUTED | H-TX | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NTA-ALL | NOT-EXECUTED | H-TX | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NRXW-ALL | NOT-EXECUTED | H-LOAD | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NRXL-ALL | NOT-EXECUTED | H-LOAD | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NEDW-ALL | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CAPP16SND-ALL | NOT-EXECUTED | H-DB,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CAPP16APP-ALL | NOT-EXECUTED | H-DB,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTAS16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTAS16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTS16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTS16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTA16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTA16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXW16SND-ALL | NOT-EXECUTED | H-LOAD,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXW16APP-ALL | NOT-EXECUTED | H-LOAD,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXL16SND-ALL | NOT-EXECUTED | H-LOAD,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXL16APP-ALL | NOT-EXECUTED | H-LOAD,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDW16SND-ALL | NOT-EXECUTED | H-ED,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDW16APP-ALL | NOT-EXECUTED | H-ED,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| C2APP2CMD-ALL | NOT-EXECUTED | H-DB,H-APPCTX,H-CMDCTX | CHAIN-APPCTX-CMDCTX | CMDCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBERASE16SND-ALL | NOT-EXECUTED | H-DB,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBOPEN16SND-ALL | NOT-EXECUTED | H-DB,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKVETO16SND-ALL | NOT-EXECUTED | H-DOC,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDEND16SND-ALL | NOT-EXECUTED | H-ED,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCANCEL16SND-ALL | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCLOSED16SND-ALL | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJERASE16SND-ALL | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJOPEN16SND-ALL | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJUNDO16SND-ALL | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTABORT16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTEND16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRENDED16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTROUTERMOSTENDCALLED16SND-ALL | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBERASE16APP-ALL | NOT-EXECUTED | H-DB,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBOPEN16APP-ALL | NOT-EXECUTED | H-DB,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKCHANGED16APP-ALL | NOT-EXECUTED | H-DOC,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKVETO16APP-ALL | NOT-EXECUTED | H-DOC,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKWILL16APP-ALL | NOT-EXECUTED | H-DOC,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDEND16APP-ALL | NOT-EXECUTED | H-ED,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CENTGFX16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCANCEL16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCLOSED16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJERASE16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJMOD16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJOPEN16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJUNDO16APP-ALL | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTABORT16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTEND16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRENDED16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTROUTERMOSTENDCALLED16APP-ALL | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NEDC-ALL | NOT-EXECUTED | H-ED | CHAIN-NONE | EDC-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDC16SND-ALL | NOT-EXECUTED | H-ED,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDC16APP-ALL | NOT-EXECUTED | H-ED,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NDBMOD-S | PASS-GOVERNED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | `e864a093` / V35-A2 / `18249CD1` | carry-fwd §188 | PASS-S | — | S | E02,E03 |
| 02NDBMOD-M | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NDBMOD-SM | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 02NDBERASE-S | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | — | — | — | — | — | — |
| 02NDBERASE-M | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NDBERASE-SM | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |

## 7. Managed matrix (HEC-V27-C1 section 2; fixture `HF-V31-1`)

| ProbeId | In literal 18 | Class | Trigger / scheduler / direct action | Surface | Tags | Would contribute if a governing PASS |
|---|---|---|---|---|---|---|
| 01 | yes | NOT-EXECUTED | M-DB-MOD | S | P1,P2 | — |
| 02 | yes | NOT-EXECUTED | M-DB-OPEN | S | P1,P4 | — |
| 03 | yes | NOT-EXECUTED | M-DB-MOD | S,M | P3 | — |
| 04 | yes | NOT-EXECUTED | MD-ABORT | S,M | P3,P4 | — |
| 05 | yes | NOT-EXECUTED | MD-COMMIT | S,M | P2,P3,P4 | — |
| 06 | yes | NOT-EXECUTED | M-DB-OPEN | S | P1,P2,P4 | — |
| 07 | yes | NOT-EXECUTED | MD-RYOW | S,M | P4 | — |
| 08 | yes | NOT-EXECUTED | M-DB-MOD | S,SM | P5 | — |
| 09C | yes | NOT-EXECUTED | M-DB-MOD | S,M | P2 | — |
| 09E | yes | NOT-EXECUTED | M-DOC-END | S,M | P2,P6,P11,P12 | — |
| 10S | yes | NOT-EXECUTED | M-DB-MOD | S | P6,P12 | — |
| 10M | yes | NOT-EXECUTED | M-DB-MOD | M | P11,P12 | — |
| 10SM | yes | NOT-EXECUTED | M-DB-MOD | SM | P6,P11,P12 | — |
| 11 | yes | NOT-EXECUTED | M-DB-MOD | S | P8 | DA-P8 (+NOD surface for DA-P7) |
| 12 | yes | NOT-EXECUTED | M-DB-MOD | M,SM | P9 | — |
| 13 | yes | NOT-EXECUTED | MS-CONTEXT | S,M,SM | P3,P12 | — |
| 14 | yes | NOT-EXECUTED | M-DB-MOD | S | P10,P12 | DA-P10 |
| 15 | yes | NOT-EXECUTED | M-DB-MOD | SM | P6,P10,P11,P12 | DA-P10 |
| 16S-SEND | no (16-family scheduler row) | NOT-EXECUTED | MS-SEND | S | P6,P12 | — |
| 16M-SEND | no (16-family scheduler row) | NOT-EXECUTED | MS-SEND | M | P11,P12 | — |
| 16SM-SEND | no (16-family scheduler row) | NOT-EXECUTED | MS-SEND | SM | P6,P11,P12 | — |
| 16S-CONTEXT | no (16-family scheduler row) | NOT-EXECUTED | MS-CONTEXT | S | P6,P12 | — |
| 16M-CONTEXT | no (16-family scheduler row) | NOT-EXECUTED | MS-CONTEXT | M | P11,P12 | — |
| 16SM-CONTEXT | no (16-family scheduler row) | NOT-EXECUTED | MS-CONTEXT | SM | P6,P11,P12 | — |
| 16S-IDLE | no (16-family scheduler row) | NOT-EXECUTED | MS-IDLE | S | P6,P12 | — |
| 16M-IDLE | no (16-family scheduler row) | NOT-EXECUTED | MS-IDLE | M | P11,P12 | — |
| 16SM-IDLE | no (16-family scheduler row) | NOT-EXECUTED | MS-IDLE | SM | P6,P11,P12 | — |

Every managed row: NOT-EXECUTED. no executable contract or instrument exists for this managed row (the CT-DA harness only hosts RR-MANAGED-CMD, the Document.CommandEnded observer of row 09E, inside native runs)

## 8. Inputs (SHA-256)

- `docs/initiatives/I-52-native-probe-matrix-v35.md` — `79E876597C749CC634F1E75E8E4943FE549C8B762E6B4A9B0A5093F86D83991C`
- `docs/initiatives/I-52-host-event-catalog-v27-correction.md` — `D624156F3461061E62076E8C25792D966CA34A5CF4840E5868ADF4A559506E01`
- `docs/initiatives/I-52-host-event-catalog-v27.md` — `357569C9C057F9C94D77C5E13A1C6E5319FBEBB4E12BDD4895F0FB2A5220F9AB`
- `docs/initiatives/I-52-execution-catalog-v35.json` — `CB858ED5BDA7D99E8F85E2A1BC06A0461162BCA57189DDF59FCBEE9128F46BA8`
- `docs/initiatives/I-52-fixture-execution-contract-v35.md` — `6E21A8E23D838751D8C1540BC7C0940F0BFF448FF8B8631DC6162A96501A477F`
