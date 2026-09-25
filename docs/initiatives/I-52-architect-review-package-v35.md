# I-52 — Architect Review Package V35 (delta review of V35-A1)

The Architect reviewed V35 at `0c609269b47adedd0f29d8b4cbbc010fae91d2e0`: AGREED WITH CORRECTIONS, 14 required corrections
(decision §170). This package asks for the delta review of V35-A1: the exact publication commit that adds decision §170.
Try to refute the fourteen closures; do not implement, build or execute CT-DA.

## Exact objects

- `docs/initiatives/I-52-proposal-v35.md` (§4 verdict mapping, §5 result, §6 V35-A1 amendment and RC-14 table)
- `docs/initiatives/I-52-execution-catalog-v35.json` (normative, revision V35-A1, `revisionParent` = the V35 blobs) and its render `I-52-execution-catalog-v35.md`
- `docs/initiatives/I-52-native-probe-matrix-v35.md` (36 fields; new `MarkerStageBindings` and `CompletionTokenIds`)
- `docs/initiatives/I-52-native-event-catalog-v35.md`, `I-52-native-scheduler-catalog-v35.md`, `I-52-fixture-execution-contract-v35.md`
- `eng/research/I52Ctda/fixture-v35.json`, `eng/research/I52Ctda/traceability-v35.json`
- `eng/research/I52Ctda/v35-oracle.json` (architecture-review oracle, RC-13: independent tables A-H and P, approval freeze Z) and its source `eng/research/I52Ctda/oracle/make_oracle.py`, `oracle_predicates.py` (Python 3)
- `eng/research/I52Ctda/validate-v35-catalog.ps1`, `eng/research/I52Ctda/validate-v35-negative-controls.ps1`
- `docs/automation/evidence/I-52-v35-clause-lineage.json`, `I-52-v35-mechanical-validation.json`, `I-52-v35-negative-controls.json`, `I-52-v35-a1-rc-closure.json`
- `docs/automation/decisions/I-52.md` §170

Reproduce with:

```powershell
pwsh -NoProfile -File eng/research/I52Ctda/validate-v35-catalog.ps1 -SdkRoot D:\CDROM1
pwsh -NoProfile -File eng/research/I52Ctda/validate-v35-negative-controls.ps1
python eng/research/I52Ctda/oracle/make_oracle.py   # must reproduce the pinned oracle blob
```

The validator pins the V34 inputs (git blobs at `0c609269`), the oracle and its two source files. The oracle blob pin and
the approval hashes are the author's proposal: approving them is part of this review.

## Required attacks

1. RC-01: find a row whose `CompletionTokenIds` omits an asynchronous stage, a path on which I52CTDA_FINISH can run before a
   delivery, or a FIN-GATE-01 use that earns scheduler credit or satisfies a governed marker.
2. RC-02/RC-03: find a MARK-LOCK-RELEASE or instrumentation-marker record that a FINISH, driver or wrong-stage record could
   satisfy.
3. RC-04/RC-05: find a row where a proven cleanup safety contradiction is still hidden by UNKNOWN, where incomplete evidence
   reaches PASS or FAIL, or where in-process code classifies.
4. RC-06: find a retained V34 ContractId whose status, text or use disagrees with the oracle and with NPM-V34.
5. RC-07/RC-08: find a module, loader, identity or sequencer path that is unspecified for R3, or a payload registration that
   can bind a database other than the row's scratch database.
6. RC-09/RC-10: find a staging value that can mask ExpectedAfter, or a Model Space ownership conflict left in 02NAPP-SM.
7. RC-11/RC-12: find a guard key that lets an unrelated same-family event consume a guard, or a guard released before the
   operation whose events it protects.
8. RC-13: find a coherent corruption (row + lineage + traceability, or catalog + traceability) that the oracle-based checks
   still accept (93 controls, each asserting its check), and decide whether the independent tables A-H and P share a
   derivation assumption with the generator; the approval freeze Z is a snapshot, not an independent derivation.
9. RC-14: find a row missing from the structural-UNKNOWN table, or a table entry that hides an architecture gap.

## Expected decision surface

If all attacks fail: `AGREED — V35 EXECUTION AUTHORITY CLOSED`, which permits the Coordinator to open the R3
implementation gate (executor, payload module, managed observer, migration of the research harness to fixture-v35 and
traceability-v35). Otherwise name the RC, identifier or row, the counterexample and the correction.

```text
V35 = READY FOR ARCHITECT DELTA REVIEW (V35-A1)
R3 IMPLEMENTATION = BLOCKED
GOVERNING PROBES EXECUTED = 0
CTDA_HOST_PASS = NOT EVALUATED
CURRENT ARX = STILL CANONICAL
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
```
