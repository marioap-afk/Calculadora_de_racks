# I-62 A-6 round 2: observable evidence of tool/acceptor independence (finding A62-A6-R3)

Experimental evidence, not normative. It answers the five demands of `RequiredFindings` A62-A6-R3 in the formal A-6 review
(`docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json`), points (a) to (e), and lists in (f) what is
still undemonstrated. It contains tests, results and a code review. It does not write or edit the amendment text.

**Pinned pieces, never modified.** Facts tool `bmin_facts.py`, blob `9a829b0eeb1210238052b1feac06cfa8f7964aed`. Acceptor
`bmin_accept.py`, blob `434ef1fccb0d15762b30244b4a60928120698411`. Both were taken only with `git cat-file blob` from the
architecture worktree (`%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal`). `mutate.py` (blob
`6967938954219a6a8a2f38dc80ef5a1165966d19`) is read with `ast` only, never executed or edited. The 27 mutants are separate files
under the work root. Line numbers in this README refer to the pinned LF bytes.

**Constraints kept.** No model CLI. No network. The fixture origin is only read (`git clone --no-hardlinks`; its refs are
unchanged, `fx/u1` = `95bdc29d`). Nothing is written in the worktree, in the RackCad checkout or under `D:\r62-fixture`. Work files
live under `D:\r62-a6r2\r3\`. Tool, mutants and acceptor always run with `python -I` (Python 3.13.14, git 2.54.0.windows.1), one
process at a time. Deliverables carry no user name and no e-mail: `scripts/finalize.py` checks this and wrote
`results/manifest.json`.

## 0. Summary

| Item | Result |
|---|---|
| Mutants activated (observable difference in Facts, FactsSha256, Error, GitCalls or Inputs) | **25/27**. M15 and M23 have no CLI path that reaches their code, so they are equivalent at the envelope boundary (§1.3). |
| Activated mutants rejected by the pinned acceptor with the mutant installed **as the pinned tool** (config M: R1 and the R3 re-run comparison cannot detect them) | **25/25** get REJECT_FACTS on some activation. 24 have a detector that is attributable to the mutation. M11 is rejected only by R2, through the tool's own `Runtime.Isolated` report. |
| Critical facts of A6-9 rule 3 (Scope membership, ancestry/Identity, EFF, MV-3, MV-4, MV-6, MV-7, blobs/hashes, scope coverage) | Every critical-fact mutant is **detected at facts level**, by R5, R6 or R7: M01, M03, M04, M05, M06, M07, M08, M09, M12, M13, M16, M18, M20, M21, M22, M24 and M02. Activated but NOT DETECTED critical mutant: **none**. |
| Caught only at decision level (R4), ACCEPT_FACTS at facts level | **M14** (free text) and **M17** (unsupported schema keyword, MV-1). Neither is in the A6-9 critical list. Both are gaps, §7. |
| (b) clause map without the fake tool | The tool equals the hand-derived values on 11/12 maps. The acceptor equals them on 12/12. R4's map part judges "Authority = pass" exactly as hand-derived on 12/12. 17/17 single-field perturbations are rejected. One divergence (D11, §2). |
| (c) R4 on the real scenario, never not applicable | 3 conforming outputs are ACCEPT_FACTS with R4 MATCH. 13/13 flipped Results are rejected by R4, and in each case only the flipped row mismatches. not_run behaves as "not pass". |
| (d) closure derived from an input-closure/v1 | Inside runs: R9 MATCH (11 VERIFY runs). Runs reading outside: 4/4 R9 MISMATCH. Commits, Refs and `--file` names are not mechanical from input-closure/v1 alone, and there is a tension with RAE I6 (§4). |
| Gaps that need an acceptor or tool change (not implemented) | G1 M17 fail-closed bypass. G2 D11 divergent reading (false REJECT). G3 commands not recomputed (M14). G4 self-reported Runtime and GitCalls. G5 one-sided R4 Authority. G6 P2/P3 divergent parsing. See §7. |

## How to reproduce

```powershell
# from the scripts folder; R3_WORK (default D:/r62-a6r2/r3) is the work root
python -I build_repos.py   # pinned bytes by blob, 27 mutants, REAL and DELIV clones, synthetic SYN repo (~75 s)
python -I part_a.py        # (a) ~45 min (28 tool variants x 31 runs; ~170 acceptor runs). Results are cached, so a re-run is fast
python -I part_b.py        # (b) ~6 min
python -I part_c.py        # (c) ~25 min
python -I part_d.py        # (d) ~5 min
python -I review_e.py      # (e) citations + probes, ~1 min
python -I finalize.py      # hygiene + manifest
```

The scripts are standard-library Python. They load each other with `importlib` from their own folder, so `-I` is enough.
Reproducibility check: running `build_repos.py` a second time into another work root gave **identical** build records
(every commit, blob and mutant identity; only the `Path` fields differ). Synthetic commits use fixed identities and dates.

## Inputs and scenarios

| Scenario | Repository | What it is |
|---|---|---|
| S1-REAL-E2E | REAL: fresh clone of the fixture | the previous e2e scenario, 13 runs, unchanged (BASE `1a4fc9c6`, HEADX `ae25b596`, RED `d30fb6a9`, MAIN `fbe25347`) |
| S2-REAL-DELIVERY | DELIV: fresh clone plus a **simulated** T1 delivery | Q0 = fixture tip `95bdc29d`. RED `53436cb2` touches the test. GREEN `b417d36c` touches `Calculator.cs` and adds a note under `FX-U1-agent/**`. Both carry the declared Co-Authored-By. `refs/heads/fx/u1` and `refs/remotes/origin/fx/u1` were moved locally to GREEN (no push). Delegation/v2 and worker-handoff/v1 are schema-valid. 11 runs. Decision level. |
| S2-NOI / S2-DUPKEY | DELIV | the same `diff` started without `-I`; a delegation and a JSON file with a duplicated key |
| SYN-COMMON | SYN: synthetic "mini" repository | valid map V0. identity, diff, scope, cleantree, freetext and clausemap runs. Decision level. |
| SYN-SA ... SYN-SF | SYN | SA: two first-parent merges, a deletion under Surfaces, a preamble change, a heading-like line inside a ``` fence. SA-STATE: two distinct `claim_id`. SB: trailer on the first-parent line. SC: a schema keyword outside the subset. SD: a duplicated map key. SE: the trailer quoted inside a longer line. SF: two exact trailer commits. |
| V0, D1 ... D11 | SYN | part (b): one valid map and 11 maps with one known defect each |

The mini repository has its own AP/WORKFLOW with the fixed headings, the pointer, 16.10 terms, the closed Surfaces paragraph and
a map. Its clause-map **schema is the fixture's own** (`6b54dda9`). It is small enough to derive every expected fact by hand.

**Baseline** (pinned tool envelopes plus pinned acceptor). ACCEPT_FACTS on S1, S2, SYN-COMMON, SA, SD and SE. On the scenarios
where the pinned tool correctly fails closed (SA-STATE, SB, SC and SF with AMBIGUOUS or UNSUPPORTED_SCHEMA_KEYWORD; S2-NOI with
NOT_ISOLATED; S2-DUPKEY with MALFORMED_JSON), the acceptor rejects the error envelope (R3 and R5-R7, plus R2 for NOI).
**Attribution rule:** a MISMATCH counts for a mutant only if the same scenario's baseline does not show it with the same fields.

## 1. (a) Mutation independence with observable effects

Config **M** is the mutant installed as the pinned tool (`Tool.Path` = mutant file, `PinnedBlob` = the mutant's blob,
RunFrom path). R1 matches and the R3 re-run reproduces the mutant, so whatever MISMATCHes is a check other than identity or
re-run. Config **P** is the plain configuration: the real pinned tool and blob, with the mutant's envelopes as received. Decision-
level scenarios (S2, SYN-*) also carry a controller-verification/v2 derived by the **model Controller** (`r3model.py`) from the
**same variant's** facts. That makes R4 judge what a Controller would have decided by trusting the mutant. In the table,
"COMMON" means SYN-COMMON and other SYN names drop the prefix.

| Mutant | Description | Critical fact | Activated: scenario (runs that differ) | Config M: attributable MISMATCH | Config P: attributable MISMATCH |
|---|---|---|---|---|---|
| M01 | coverage rule inverted | scope coverage | S1(scope) S2(scope) COMMON(scope) | S1:R7 S2:R4+R7 COMMON:R4+R7 | +R3 everywhere |
| M02 | sentence boundary off | MV-7 | S1, S2, COMMON, SA, SD (clausemap) | R6 everywhere | R3+R6 |
| M03 | ancestry argument order swapped | ancestry / Identity | S1, S2, COMMON (identity) | S1:R5 S2:R4+R5 COMMON:R4+R5 | +R3 |
| M04 | one diff path dropped | Scope membership | S1, S2 (diff, scope, clausemap) COMMON (diff, scope) | R5+R7 everywhere | R3+R5+R7 |
| M05 | MV-4 lists merged | MV-4 | S1, S2 (clausemap) | R6+R7 | R3+R6+R7 |
| M06 | pointer removed with its whole line | MV-7 | S1, S2, COMMON, SA, SD | R6 | R3+R6 |
| M07 | ENTRY file treated as ADDED | MV-4 | S1, S2 | R6+R7 | R3+R6+R7 |
| M08 | first-parent merge order reversed | EFF derivation | SA (clausemap) | R6 (`Derive.FirstParentMerges`) | R3+R6 |
| M09 | trailer matched by substring | EFF derivation | SE (clausemap: a false EFF) | R4+R6 (`TrailerCommits`, `Candidates`, `Eff`) | R3+R4+R6 |
| M10 | ensure_ascii=False | ASCII fidelity | S1 (clausemap, read-json, read-text x2), S2, COMMON, SA, SD | R3+R6+R7 (the mutant crashes into an error envelope) | same |
| M11 | `-I` check removed | isolation | S2-NOI (diff without -I: facts instead of NOT_ISOLATED) | **R2 only** (`Runtime.Isolated is not true`): the same signal the pinned tool's error envelope raises | R2 only (R3 re-run facts are equal) |
| M12 | U-19 removed | scope coverage | S1, S2, COMMON (scope) | S1:R7 S2:R4+R7 COMMON:R4+R7 | +R3 |
| M13 | blob header without NUL | blobs and hashes | S1, S2 (hashes) | R5 (`hashes.PerPath`) | R3+R5 |
| M14 | free-text word boundary removed | free text (not critical) | S2, COMMON (freetext) | **R4 only**; at facts level **ACCEPT_FACTS** | R3+R4 |
| M15 | closed git list removed | isolation | **not activatable from the CLI** (unit probe: `version` runs) | - | - |
| M16 | duplicated trailer accepted | EFF derivation | SF (facts instead of AMBIGUOUS) | R6 (E4 + `Candidates`, `Eff`) | R3+R6 |
| M17 | unsupported schema keyword accepted | MV-1 (not critical) | SC (facts instead of UNSUPPORTED_SCHEMA_KEYWORD) | **R4 only**; at facts level **ACCEPT_FACTS** | R3+R4 |
| M18 | MV-3 deleted paths dropped | MV-3 | SA | R6 (`ChangedUnderSurfaces`, `OnlyInChanged`) | R3+R6 |
| M19 | two distinct claim_id accepted | Classify | SA-STATE | R6 (E4 Classify) | R3+R6 |
| M20 | last sentence of 16.3 = the first | MV-7 | S1, S2, COMMON, SA, SD | R6 | R3+R6 |
| M21 | candidate without the first-parent condition | EFF derivation | SA (AMBIGUOUS instead of facts), SB (**a false EFF** instead of AMBIGUOUS), SA-STATE (error vs error) | SA:R3+R6+R7, SB:R6 (E4, `Candidates`, `Eff`), SA-STATE: none (both fail closed; no false fact) | SA:R3+R6+R7, SB:R3+R6 |
| M22 | preamble omitted | MV-6 | SA | R6 (`MV6[AP].DerivedModified`) | R3+R6 |
| M23 | `--porcelain` no longer forced | isolation | **not activatable from the CLI** (unit probe: `status --short` passes) | - | - |
| M24 | code fences ignored | MV-6 | SA | R6 (`MV6[AP].DerivedAddedOrEntry`) | R3+R6 |
| M25 | duplicate JSON keys accepted | strict JSON | S2-DUPKEY (scope: facts instead of MALFORMED_JSON; read-json), SD (`MapParses` true) | S2-DUPKEY:R5+R7 (the acceptor cannot parse the input), SD:R4+R6 | +R3 |
| M26 | `--no-ext-diff --no-textconv` not forced | isolation | every scenario with diff, scope or clausemap (GitCalls only) | R9 (`diff/show without --no-ext-diff --no-textconv`) | R3+R9 |
| M27 | claim_id lines counted | Classify | S1, S2 (AMBIGUOUS on the real /v2 state) | R3+R6+R7 | same |

**Notes.**
1. The real fixture alone activates 13 mutants (the same 13 as the previous e2e). Real repository data plus synthetic inputs
   adds M11, M14 and M25. The rest needs the synthetic repository: the fixture has one merge, one trailer, no deletion under
   Surfaces and no duplicates.
2. Decision level. A Controller trusting M01, M03, M12, M09, M25 (SD), M14 or M17 decides wrongly, and R4 catches it. A
   Controller trusting M04 decides correctly (dropping a covered path does not flip Scope), so R4 stays MATCH and R5/R7 catch the
   facts.
3. Config P always adds R3, because the real tool's re-run differs. The exception is M11: Runtime is excluded from the re-run
   comparison.
4. **M15 and M23** (`results/a-mutation.json`, `UnitProbes`). Every `git_exec` call in the pinned tool uses a literal
   subcommand from the list, and both `status` calls already pass `--porcelain`, so no CLI input reaches the mutated code. An
   in-process probe that calls `git_exec` directly shows the effect: with M15, `version` runs; with M23, `status --short` runs
   without porcelain. With the pinned tool both are refused or forced. Envelope injection through an echo tool (R1 and R3 MATCH)
   shows that the acceptor's R9 rejects each class: `git subcommand outside the tool list: version`, `status without
   --porcelain` and `diff without --no-ext-diff --no-textconv`. The control without injection is ACCEPT_FACTS. That detector only
   works if the tool records the call (G4).

## 2. (b) Clause-map facts of the acceptor without the circular fake tool

`part_b.py` does not use `fake_tool.py`, and nothing imports the acceptor. The oracles are:

* **Hand-derived expected facts.** Written by construction for each map: which path lands in which list, the headings, the
  sentences, and line numbers 7 and 19. The harness computes blob ids with hashlib over `git show <commit>:<path>`, a primitive
  neither piece uses for blobs.
* **The real pinned tool.**

An **echo tool** prints a fixed envelope (R1 and R3 MATCH by construction). It carries the hand-derived Facts, so R6/R7 compare
them against the acceptor's own computation.

| Map | Known defect | B1 tool = hand | B2 acceptor = hand (R6, R7) | R4 "Authority pass" | B3 acceptor on real tool |
|---|---|---|---|---|---|
| V0 | none | all 44 compared fields | MATCH, MATCH | MATCH (unconstrained) | ACCEPT |
| D1 | wrong BaseBlob (L905 literal) | yes | MATCH, MATCH | MISMATCH (MV-4 LiteralViolations) | ACCEPT |
| D2 | ADDED file present in EFF^1 (R61 b) | yes | MATCH, MATCH | MISMATCH (AddedPresentInEff1) | ACCEPT |
| D3 | ENTRY file that is not the schema | yes | MATCH, MATCH | MISMATCH (EntryFilesOutsideSchema) | ACCEPT |
| D4 | ENTRY schema present in EFF^1 (informative) | yes | MATCH, MATCH | MATCH (no defect, as hand-derived) | ACCEPT |
| D5 | changed file unlisted; listed file unchanged | yes | MATCH, MATCH | MISMATCH (MV-3 OnlyInChanged/OnlyInFiles) | ACCEPT |
| D6 | file deleted under Surfaces | yes | MATCH, MATCH | MISMATCH (OnlyInChanged, DeletedUnderSurfaces) | ACCEPT |
| D7 | map sections contradict the derivation | yes | MATCH, MATCH | MISMATCH (MV-6) | ACCEPT |
| D8 | pointer not last in 16.3; 16.3 changed | yes | MATCH, MATCH | MISMATCH (MV-7) | ACCEPT |
| D9 | duplicated entry | yes | MATCH, MATCH | MISMATCH (MV-5) | ACCEPT |
| D10 | Surfaces reordered | yes | MATCH, MATCH | MISMATCH (MV-2) | ACCEPT |
| D11 | map violates its schema (FileKind RENAMED) but keeps the shape | **no**: tool emits null MV2.MapSurfaces and MV3-MV6 | MATCH, MATCH (acceptor = SPEC-literal hand values) | MISMATCH (MV-1) | **REJECT (R6, R7)** |

**B4 sensitivity: 17/17.** These single-field perturbations of the hand-derived envelope are all rejected (R6, plus R7 for MV-4):
EFF, FirstParentMerges, EntryMatches, OnlyInFiles, MV-6 DerivedModified, 16.3 minus the whole pointer line, each emptied
defect list, MV-4 lists merged, ENTRY treated as ADDED, the deleted path dropped (the M18 form), MapRemoved, the pointer flag,
Surfaces and SchemaViolations.

**D11 is a divergence between the two pinned pieces.**
* The tool computes MV2.MapSurfaces and MV-3..MV-6 only when `SchemaViolations == []` (`bmin_facts.py:1279, 1286, 1292`).
* The acceptor computes them whenever the map parses and has the basic shape (`bmin_accept.py:1150, 1153-1154`), then compares
  (`1545-1546, 1547-1599`).
* Result: the pinned tool's correct facts on a schema-invalid map are always REJECT_FACTS (gap G2).

**R4 asymmetry.** With the valid map V0, a Controller "Authority = fail" is also MATCH: the map part is "unconstrained"
(`bmin_accept.py:2187-2188`). A false Authority fail is never detected (G5).

## 3. (c) R4 exercised on the real scenario

No request in part (c) declares any NotApplicable. Each conforming controller-verification/v2 is schema-valid and coherent
(Classification, Disposition and FailureClass per AP 16.9). Its Results come from the pinned tool's facts through the model
Controller. Each altered output changes one Result and recomputes the classification so it stays coherent.

| Scenario | Conforming Results (Identity, Scope, CleanTree, Trailer, FreeText, Authority) | Conforming | Flips | not_run |
|---|---|---|---|---|
| C-DELIV (real fixture + delivery) | pass, pass, pass, pass, pass, **fail** (real map: LiteralViolations 7, AddedPresentInEff1 23, MV-6, MV-7) | ACCEPT_FACTS, R4 MATCH; EXECUTION_BLOCKED/STOP, FailureClass Authority | 6/6 MISMATCH, only the flipped row | 5 MISMATCH (acceptor expects pass); Authority MATCH (expects not pass) |
| C-REAL (e2e commits as they are) | fail (HEAD != CurrentSha), fail (out of scope), pass, fail (foreign trailer), fail (`GATE PASS`), fail | ACCEPT_FACTS, R4 MATCH | 6/6 MISMATCH, only the flipped row | CleanTree MISMATCH; the other 5 MATCH |
| C-DELIV-DIRTY (untracked file, a genuine failure) | CleanTree **fail** | ACCEPT_FACTS, R4 MATCH | CleanTree to pass: MISMATCH | MATCH |

A wrong `AuthorityResolution[0].EffectiveSha` is rejected by R6 (R4 unaffected) in both scenarios.

**Inputs R4 needs and how they were built:**
* `Verification.Controller` {Path, Sha256}: the model Controller's output.
* `Verification.Delegation`: delegation/v2. Scopes, authorities, invariants, tests, stop conditions and role requirements are
  copied from the custodied contract (`628d89af`). BaseSha is Q0 or the e2e BASE; ExpectedBranch `fx/u1`; ExpectedWorktree the
  clone.
* `Verification.Handoff`: worker-handoff/v1. BaseSha, RedSha, CurrentSha, Branch and Worktree of the scenario;
  `Worker.Trailer` = the declared Co-Authored-By; the seven free-text fields.
* `IgnorePath`: the VERIFY run's own `artifacts/orchestration/FX-U1/T1/0/<RunId>/output.json` (kit run_dir).
* `TermsRev` = `MainSha` = `fbe25347`.
* `Repo`: R4 recomputes Identity, Scope, CleanTree and Trailer from the clone itself.

R4 does not use the relay facts (R10) or the tool envelopes. It is independent of the tool's facts.

## 4. (d) Mechanical derivation of the acceptor closure from an EffectiveInputClosure

`part_d.py` builds an input-closure/v1 for VERIFY of T1 on the delivery and validates it against the schema. Contents:
* AuthorityRevision `1746b404`.
* 15 CanonicalInputs: the contract; its six authorities at AR; the three delivered files at GREEN; the decision and state
  records at GREEN; the delegation, handoff and TRX as off-Git artifacts with their Git blob ids.
* AGENTS.md as automatic instruction.
* 8 Obligations.
* 9 AllowedTransitiveInputs: WORKFLOW, agent-execution README, map, map schema, handoff schema, and the map's MODIFIED files at
  EFF and EFF^1.

It then derives the closure with this table (quotable as is):

```
DERIVATION  rackcad-input-closure/v1 (E) -> rackcad-bmin-closure/0-experimental
Inputs: E; the delegation D, handoff H and contract K whose blobs E pins; the role invocation I (UnitId, TaskId,
Attempt, RunId, Target); the repository (graph primitives only).

C1 Commits  = {E.AuthorityRevision} U {D.BaseSha, D.MainSha, D.ChainBaseSha, D.ChainRedSha if not null}
              U {H.BaseSha, H.CurrentSha, H.RedSha if not null} U {I.Target.Commit} U {K.AuthorityRevision, K.MainSha}
              U RevList(D.BaseSha..H.CurrentSha)
              U FirstParentChain(D.MainSha) U Parents(m) for every m in FirstParentChain(D.MainSha) with >= 2 parents
              U {MergeBase(D.MainSha, K.AuthorityRevision)}
C2 Refs     = {HEAD, refs/remotes/origin/<D.ExpectedBranch>, refs/remotes/origin/main}
C3 RevPaths = {(c, p) : c in Commits, (p, b) in E.CanonicalInputs U E.AllowedTransitiveInputs U E.AutomaticInstructions,
              Blob(c:p) = b}                                   (blob identity)
C4 Paths    = {p of every E entry} U {o.Target : o in E.Obligations, o.Class = READ, o.Target not null}
              U Surfaces(M) U {f.Path : f in Files(M)} U {a.Path : a in K.Authorities} U {IgnorePath(I)}
              M = the clause map whose blob E pins at its fixed path;
              IgnorePath(I) = artifacts/orchestration/<I.UnitId>/<I.TaskId>/<I.Attempt>/<I.RunId>/output.json
C5 Objects  = {}                                               (trees of Commits and blobs of RevPaths are implicit)
C6 Files    = {(Name(p), SHA256(bytes(p, b))) : (p, b) in E.CanonicalInputs, Name(p) defined}
              Name: contract path -> CONTRACT; artifacts/orchestration/<u>/<t>/<a>/<D.RunId>/output.json -> DELEG;
              .../<H.RunId>/worker-handoff.json -> H; *.trx -> TRX
              bytes(p, b) = the custodied bytes whose Git blob id is b (verified)
```

**Not mechanical from input-closure/v1 alone:**
1. **Commits.** The schema carries only AuthorityRevision. C1 needs the bytes of D, H, K and I (pinned by E) plus three Git
   graph primitives. The first-parent superset avoids depending on any 16.14 Derive implementation, so it has no common mode
   with the fact under check.
2. **Refs.** Not representable in input-closure/v1. They are fixed by 16.9 #5 plus D.ExpectedBranch.
3. **`--file` names.** A fixed path-role table that input-closure/v1 does not carry.
4. **IgnorePath.** The invocation's own output path. It comes from I, not from E.
5. **RevPaths versus RAE I6.** The EIC-I6-STRICT variant keeps every blob equal to the one at AuthorityRevision. That drops the
   GREEN blobs and the window's decision and state records, and the contract does not exist at AR at all. Legitimate VERIFY reads
   then fall outside: R9 MISMATCH on `hashes` (delivered note), `clausemap` (state at GREEN) and `read-text` (decision record at
   GREEN). Settling this is a normative decision, not a derivation step.

**Result.** The derived closure has 7 commits, 89 rev:paths, 3 refs, 48 paths, 0 objects and 4 files. The previous e2e closure
had every commit of the clone and every path of nine commits.

* **Inside:** the 11 VERIFY runs (identity, diff, scope, contract, hashes, cleantree, handoff, freetext, clausemap, trx,
  read-text) give **R9 MATCH**, and the request is ACCEPT_FACTS with every check MATCH except the R8 MANUAL.
* **Outside:** each extra run gives **R9 MISMATCH** and leaves the other runs' R9 untouched:
  * `read-text` of `README.md` at GREEN: path and rev:path outside;
  * `read-text` at commit `54f2a4a8`: commit outside;
  * `hashes` of `FIXTURE-MANIFEST.json`: path and object outside;
  * `read-json --file N` (undeclared): file outside.

## 5. (e) Code review of independence

`review_e.py` checks every citation mechanically: the quoted token is on the cited lines of the pinned bytes, and all are
verified.

| Critical fact | Tool primitive | Acceptor primitive | Shared? | Shared assumptions | Residual common-mode risk |
|---|---|---|---|---|---|
| Scope membership | `git diff --name-only --no-renames -z A..B` (`bmin_facts.py:642-649`) | two full `ls-tree -r -z --full-tree` listings compared by (mode, type, id) (`bmin_accept.py:962-965, 434-449`) | no | E6 renames; byte order; same clone | tree objects trusted by both (the acceptor re-hashes commits and blobs, not trees) |
| Ancestry / Identity | `merge-base --is-ancestor` (608-610); `rev-parse HEAD`, `--abbrev-ref`, `--show-toplevel` (629-631); `rev-parse --verify` of origin refs (613-617) | own walk over re-hashed raw commits (380-401, 349-365); one `rev-parse --show-toplevel HEAD --abbrev-ref HEAD` (284-292); `for-each-ref` (294-307) | **partly**: HEAD, AbbrevRef and Toplevel come from `git rev-parse` in both | reflexive ancestry. Replace refs and grafts diverge rather than coincide: the tool honours replace refs (no `GIT_NO_REPLACE_OBJECTS`, 141-143), the acceptor disables them (217) | a wrong rev-parse answer is reproduced by both; the rebase/patch-id branch exists in neither |
| EFF (16.14) | `rev-list --first-parent --merges` + `log --format=%H%n%B` exact line + `rev-list --parents` (1072-1097) | own first-parent chain, raw-message line scan, children map (1040-1047, 406-432) | no primitive shared | same reading of 16.14 and errata E4 (exact line anywhere in the message). E4 was written after the first integration with both outputs in view. | a 16.14 misreading carried by the SPEC is undetectable; CRLF decoding diverges (probe P3) |
| MV-3 | `ls-tree -r -z --full-tree rev -- :(literal)surfaces` (247-258, 1299-1300) | full `ls-tree -r -z --full-tree` (437, 1162-1166) | **YES** | blob id compared, mode ignored, map excluded | common Git command on the same clone |
| MV-4 | `ls-tree` blob ids (227-244); the four rules (1322-1333) | ids from the full listing (1106-1108); the same rules (1177-1188) | **YES** for observed ids; rules transcribed from the same SPEC text | L905 and R61 b read identically (gitlinks differ only) | R7 is diversity of code, not of reading |
| MV-6 | `Md`: raw-line heading regex, ``` fences (369-385, 90, 413-417) | `MdDoc`: heading on the normalized line, ``` fences (499-518, 490-496, 559-567) | no | SPEC 1.9: only ``` fences, no `~~~`, no setext; `\s+` versus `str.split()` (same Unicode set) | `~~~` fence: both see a heading (probe P1, R6 MATCH: common mode); indented heading: they diverge (probe P2) |
| MV-7 | regex `(?<=\.)\s+` split, last pointer removed (91, 1243-1249) | own character loop and pointer removal (576-600) | no | SPEC 1.10 boundary; "p. ej." splits in both; AP says the pointers are *presentes*, both use first/last position | a sentence rule the SPEC got wrong is common to both |
| Blobs and hashes | `ls-tree` ids, `cat-file blob`, hashlib SHA-1 header (238-243, 273, 794-795) | `ls-tree` ids, `cat-file --batch` re-hashed against the id, own SHA-1 (1013, 459-465, 110-119) | **YES** for tree to blob id; content read differs | the tree entry id is the content identity (only the acceptor verifies content) | trees trusted; same hashlib |
| Scope coverage | `u19`, `covers`, `malformed_reasons` (666-687) | `u19`, `is_malformed`, `covers`, `contained` (701-732) | no code shared; logically identical rules | AP L553 prefix reading, U-19, case-sensitive, no Unicode normalization | if AP L553 meant something else, both are equally wrong |
| R2 / R9 evidence | `Runtime.Isolated = sys.flags.isolated` (1581-1587); `ctx.git_calls.append` (144) | judges the received Runtime and GitCalls (2018-2022, 2563-2570) | **the evidence itself** | the envelope's self-report is honest | a lying or call-hiding tool is invisible to R2 and R9 (G4) |

**Discriminating probes** (synthetic repository, pinned tool plus pinned acceptor):
* **P1, `~~~` fence (shared reading).** Both report `## Falso encabezado...` as a WORKFLOW heading: R6 MATCH. This is common mode
  by construction.
* **P2, three-space indented heading.** The tool does not see a heading; the acceptor does. R6 MISMATCH on the pinned tool's own
  facts.
* **P3, CRLF in the trailer commit's message.** The tool normalizes CRLF and finds the trailer; the acceptor does not. R6
  MISMATCH on Derive (TrailerCommits, Candidates, Eff).

P2 and P3 are observed primitive diversity. They also mean false rejections on such inputs (G6).

## 6. (f) What remains undemonstrated

1. **Authorship independence is declared, not observed.**
   * Nothing here observes who wrote each piece, or that one author did not see the other's code.
   * Both were written in the same worktree.
   * The SPEC errata v0.2 (E1-E7) were written after the first integration with both outputs in view. Reading-level
     harmonization is therefore by design.
   * Provider and model of the authors: not observed.
2. **Same SPEC, same norm reading.** R5-R7 compare code, not readings. A misreading of AP carried by SPEC v0.2 is common to both
   and undetectable. Examples: exact-line trailer, ``` fences only (P1), pointer position instead of presence, the L553 prefix
   rule, MV-4 L905/R61 b. A6-9 rule 2 sends such divergences to the Coordinator, but none of this evidence detects them.
3. **Same environment.**
   * The same Python interpreter: R2 pins one for both.
   * The same git 2.54 binary, OS and clone, so the same object store.
   * `rev-parse` for HEAD, branch and toplevel, and `ls-tree` for MV-3, MV-4 and hashes, are shared primitives.
   * Tree objects are verified by neither.
4. **Self-reported evidence.** R2 (Runtime.Isolated) and R9 (GitCalls, Inputs) judge what the tool says about itself (G4).
   M15 and M23 cannot be activated through the CLI at all. Their class is shown only by a unit probe and envelope injection.
5. **No real Controller output.** R4 was exercised on schema-valid outputs of the R3 model Controller. That is a test device
   that reads the same facts. No Codex or Claude VERIFY output exists here.
6. **Simulated delivery.**
   * The RED and GREEN commits are synthetic, and the origin ref was moved locally (no push).
   * Ci, Tests, Remote, Routing, Termination and Denials were not evaluated (R4 does not cover them).
   * The TRX is an older real artifact, not one of GREEN.
7. **Mutant set.** Only the 27 fixed mutants of `mutate.py` were run. Higher-order or semantic mutants were not tried. This
   review lists the primitives but does not prove the list is exhaustive.
8. **Not exercised.**
   * A `refs/replace` or graft in the clone (finding R1: the tool does not disable replace objects, the acceptor does; a
     divergence is expected).
   * Shallow clones.
   * Gitlinks.
   * Non-UTF-8 content.
9. **A6-9 rule 4 is not demonstrated.** The acceptor ran in this same session as the tool runs. Nothing here shows it running
   outside the fixture Principal's session or outside every cession.
10. **Determinism.** Checked once by an identical rebuild. Each acceptor configuration ran once.

## 7. Gaps that would need an acceptor or tool change (recorded, NOT implemented)

Any of these changes alters a pinned blob, so the Owner's determination that pins `434ef1fc` or `9a829b0e` would no longer cover
it.

| Id | Gap | Evidence | Exact reason | Change that would be needed |
|---|---|---|---|---|
| G1 | **MV-1 fail-closed bypass not detected at facts level** (M17) | SYN-SC: the mutant emits facts where the pinned tool fails with UNSUPPORTED_SCHEMA_KEYWORD. The facts-level request is **ACCEPT_FACTS**; only R4 catches it, when the Controller says Authority pass. | `compare_clausemap` compares SchemaViolations only as non-empty (`n_nonempty`, `bmin_accept.py:1428-1433, 1541-1542`). So null (not computable) equals `[]`, and there is no E4-like rule for an unsupported keyword (`internal["Unsupported"]` is used only by `map_part_defects`, 1318). | Acceptor: MISMATCH when its own `internal["Unsupported"]` is non-empty and the envelope carries Facts; compare SchemaViolations with null distinct from `[]`. |
| G2 | **False REJECT on a schema-invalid map** (D11) | B1: the tool differs from the SPEC-literal hand values on 16 fields. B3: the pinned acceptor rejects the pinned tool's correct facts (R6, R7). | Divergent reading of when MV2.MapSurfaces and MV-3..MV-6 exist: the tool requires `SchemaViolations == []` (`bmin_facts.py:1279-1292`); the acceptor requires parse plus shape (`bmin_accept.py:1150-1154, 1545-1599`). | SPEC erratum fixing one reading, and the matching change in the tool or the acceptor. Until then a real MAP_INVALID-by-schema case cannot get ACCEPT_FACTS. |
| G3 | **Commands without facts-level recomputation** (M14) | S2 and SYN-COMMON: the false freetext facts are ACCEPT_FACTS at facts level; only R4 FreeText catches them. | R5/R7 recompute only identity, diff, scope, hashes, contract A3/A4 and clausemap (`bmin_accept.py:81-83`). freetext, cleantree, trx, handoff, read-text, read-json and contract A5/RequiredTestsPairs/SupersededEqual are only re-run (R3), which cannot catch a pinned-tool defect. | Acceptor: recompute these commands with its own primitives (FreeText and CleanTree logic already exists in `_r4_*` and could be compared field by field). |
| G4 | **Self-reported isolation and reads** (M11, M15, M23 class) | M11 is rejected only through `Runtime.Isolated` in the envelope. R9 rejects injected calls only because they were recorded. | R2 reads `Runtime` from the envelopes (2018-2022). R9 reads `GitCalls` and `Inputs` (2563-2605). The R3 re-run excludes `Runtime` and compares `GitCalls` only with the tool's own report (1982-1984). | Acceptor-side observation: the acceptor itself produces the run whose envelope is judged, or interposes a git shim on PATH in the re-run that records the real argv and compares it with GitCalls. |
| G5 | **One-sided R4 map part** | V0: Controller "Authority = fail" with a valid map is MATCH ("unconstrained"). | `bmin_accept.py:2187-2188`: only "pass with defects" is checked. | Acceptor: expect pass when the map part has no defect, unless A-6 keeps the one-sided rule on purpose. |
| G6 | **Divergent parsing gives false REJECT** (P2, P3) | An indented heading and a CRLF commit message: R6 MISMATCH on the pinned tool's facts. | Tool: raw-line heading regex (`bmin_facts.py:90, 381`) and CRLF normalization of `%B` (1079). Acceptor: heading on the normalized line (`bmin_accept.py:515-516`) and raw `\n` split of the message (1040). | SPEC erratum plus the matching change in one piece. It fails closed, but it is a SPEC-reading divergence that A6-9 rule 2 would send to the Coordinator. |

## Files

| File | Content |
|---|---|
| `scripts/r3lib.py` | shared helpers: pinned extraction by blob, mutant builder (ast), runners, synthetic commit plumbing, broad closure, harness schema validator, hygiene |
| `scripts/r3model.py` | delegation/v2 and worker-handoff/v1 builders, the model Controller (AP 16.9 and 16.13 table from tool facts), controller-verification/v2 builder, echo tool |
| `scripts/build_repos.py` | pinned bytes, mutants, REAL, DELIV (simulated delivery), SYN (mini repository and every scenario) |
| `scripts/part_a.py` ... `part_d.py`, `review_e.py` | the five parts |
| `scripts/finalize.py` | hygiene check and `results/manifest.json` |
| `results/build.json` | identities of every pinned piece, mutant, scenario commit and delivery commit |
| `results/a-mutation.json` | per mutant: activations (runs and fields that differ), config M, P and facts-level judgements with attributable checks, baseline, unit probes, R9 injection |
| `results/b-clausemap.json` | per map: B1 to B3, R4 Authority, sensitivity |
| `results/c-r4.json` | every R4 case with its rows, AcceptorExpects and coherence, plus the R4 input list |
| `results/d-closure.json` | the derivation table, the EIC sample, the derived closure, inside and outside R9 results, the I6-strict variant |
| `results/e-review.json` | the review table with verified citations and the probes |
| `results/manifest.json` | SHA-256 of every deliverable (README included) |

Results are canonical ASCII JSON in the SPEC 1.5 form: sorted keys, compact separators, `ensure_ascii`, LF. Raw envelopes,
requests and acceptor outputs stay in the work root (`D:\r62-a6r2\r3\runs\`). The real envelopes contain commit trailers with
e-mail addresses, so they are not deliverables.
