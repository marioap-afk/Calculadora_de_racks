# I-52 — Architect Review Package V35

Review the exact publication commit that adds decision §169 and these objects only. Try to refute V35; do not implement,
build or execute CT-DA.

## Exact objects

- `docs/initiatives/I-52-proposal-v35.md`
- `docs/initiatives/I-52-execution-catalog-v35.json` (normative) and `docs/initiatives/I-52-execution-catalog-v35.md` (render)
- `docs/initiatives/I-52-native-probe-matrix-v35.md`
- `docs/initiatives/I-52-native-event-catalog-v35.md`
- `docs/initiatives/I-52-native-scheduler-catalog-v35.md`
- `docs/initiatives/I-52-fixture-execution-contract-v35.md`
- `eng/research/I52Ctda/fixture-v35.json`, `eng/research/I52Ctda/traceability-v35.json`
- `eng/research/I52Ctda/validate-v35-catalog.ps1`
- `docs/automation/evidence/I-52-v35-clause-lineage.json`, `docs/automation/evidence/I-52-v35-mechanical-validation.json`
- `docs/automation/decisions/I-52.md` §168-§169

Reproduce the mechanical result with:

```powershell
pwsh -NoProfile -File eng/research/I52Ctda/validate-v35-catalog.ps1 -SdkRoot D:\CDROM1
```

`-SdkRoot` is optional; without it the ObjectARX header check is skipped and reported as not checked.

## Required attacks

1. Find a V35 row whose TriggerActionId cannot produce its PrimaryAuthority or ScheduleOrigin callback, or a required
   `+` marker that no registered observer can receive.
2. Find a V34 PASS, FAIL or UNKNOWN requirement that V35 weakened or dropped, or a lineage clause classified C that
   carried an executable requirement.
3. Find an identifier whose catalog definition still lets two implementers do different things (runtime selection,
   "or", unspecified parameter).
4. Find a guard, lock, transaction or support action used outside its legal context (FEC-V35 §1, B08, B11).
5. Find a trigger or setup that can mask ExpectedAfter (false PASS), especially the staged MUT-S and the F-XR origin
   triggers.
6. Find a resource, observer, lock obligation or queued work without a removal step or process fence, or a cleanup that
   reopens an object §164 forbids.
7. Find a blocker group R3-B01..R3-B12 or R3-C02 whose closure is nominal.
8. Find a validator check that passes on a corrupted input (the evidence lists 24 negative controls).
9. Decide each of the thirteen V35-introduced decisions of Proposal V35 §4.
10. Find a closure claim that promotes CTDA_HOST_PASS, product admission or runtime executability.

## Expected decision surface

If all attacks fail and §4 is accepted: `AGREED WITH PROPOSAL V35 — EXECUTION CONTRACT MECHANICALLY CLOSED`. Otherwise
name the artifact, identifier or row, the counterexample and the correction; a correction that keeps the frozen counts
is a V35 amendment, anything else is V36. Agreement authorizes only the next R3 implementation gate (executor, payload
module, managed observer, migration of the research harness to fixture-v35/traceability-v35); it does not execute CT-DA.

```text
V35 GOVERNANCE = ARCHITECTURE CANDIDATE
R3 IMPLEMENTATION = BLOCKED PENDING ARCHITECT REVIEW
GOVERNING PROBES EXECUTED = 0
CTDA_HOST_PASS = NOT EVALUATED
CURRENT ARX = STILL CANONICAL
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
```
