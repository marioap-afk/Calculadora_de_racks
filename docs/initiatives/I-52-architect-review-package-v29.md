# I-52 — Architect review package Proposal V29

> Exact review of Proposal V29 / NEC-V29-1 / NPM-V29-1 / HF-V29-1. No file changes, helper build or CT-DA.

## 1. Exact input authority

Review the published V29 SHA, not mutable tip. Parent inputs:

```text
Parent I-52 / V28 = 46a2e15f529d30d65f9524f0ade98d72da980237
Proposal V28 blob = 641a2b783f59a8a68c5d764f6af9f15e663ddd84
OA-V28-1 blob = 1f62ebabdd2598637f2a7f89e5066b763511c242
NEC-V28-1 blob = e2d1284c5266f5b7f2dd57129ab7552bbd881c74
Architect package V28 blob = 24a5996c7ae39291e63882306ae46f598a193730
Decisions V28 blob = 6b415958eafedfaeca934a0466df87ed11e932e1
```

## 2. Central refutations

Attempt each conjunction:

```text
NativeReactorClassSetClosed = TRUE
AND an exact native reactor callback reachable from HF can alter protected truth but is neither probed nor excluded

NativeProbeCatalogClosed = TRUE
AND any one of the 29 ProbeIds lacks one of the 24 schema fields

NativeProbeCatalogClosed = TRUE
AND any ProbeId has zero or more than one PrimaryAuthorityId

NativeProbeCatalogClosed = TRUE
AND C15N16N-SM lacks load, registration, schedule, execution, state, oracle or cleanup

HostFixtureContractClosed = TRUE
AND EventCatalogClosed = FALSE

CTDA_HOST_PASS = TRUE merely because definition contract is CLOSED
```

Any satisfiable case is BLOCKER.

## 3. Exact authority review

1. Resolve every new URL under Autodesk `cloudhelp/2025`.
2. Verify `AcDbObjectReactor` and `AcDbEntityReactor` class/header/signatures.
3. Verify add/remove transient registration and retained-instance lifetime.
4. Verify `objectClosed` forbids reopening the notifying object write; all probes mutate another target.
5. Verify `goodbye` is cleanup marker and prevents later removal from a destroyed notifier.
6. Verify document-lock callback signatures in `AcApDocManagerReactor`.
7. Do not accept cross-version pages, HTML hashes as header hashes, or managed emulation.

## 4. Callback/class-set review

Challenge every census row. In particular:

- Does `AcDbEntityReactor::modifiedGraphics` need independent native evidence? V29 says yes.
- Are cancel/undo/objectClosed relevant object phases present? V29 says yes.
- Can persistent object reactors create a distinct callback phase, rather than only a registration/lifetime residual?
- Can any doc-manager callback reached by the fixture mutate or schedule without an NPM row?
- Do layout, input, selection, long transaction, clone, publish, raster or protocol paths actually occur in HF?
- Is `AcRxDLinkerReactor` correctly limited to T15 marker rather than mutation authority?

The census is HF-bounded, not a universal plugin inventory. A class outside HF may stay at HostToKind, but a class
capable of changing the claimed host subproperty cannot.

## 5. Probe matrix mechanical review

Parse NPM-V29-1 and confirm:

```text
row count = 29
unique ProbeIds = 29
every row fields = 24
exactly one PrimaryAuthorityId per row
no blank cells
no normative wildcard/range
all shared ContractIds resolve locally
```

Reconstruct Event→Probe and Scheduler→Probe mappings from the matrix and compare with NEC-V29-1. Marker,
ScheduleOrigin and Primary cannot be conflated.

## 6. Targeted probe review

- `02N/04N/09N-A/B/C/D/10N-S/M/SM/16N-S/M/SM`: prior narrative gaps must be gone.
- `02NO-S/M/SM`, `02NO-CLOSE-SM`, `04NO-S`, `04NO-UNDO-S`, `02NE-M`: exact object/entity callback and distinct target.
- `10NDOC-*`: no lock or T authority assumed from notification.
- `C2OBJ16N-*`, `C2ENT16N-M`, `C2DOCW16N-SM`, `C2DOCC16N-SM`: NS-SEND is primary; native callback is origin.
- `C15N16N-SM`: NS-SEND is the sole primary; load/linker/registration are setup/markers; N-DB-MOD is origin.

For every probe, distinguish definition PASS oracle from actual unexecuted outcome. A future illegal mutation is
UNKNOWN/FAIL, not evidence that an undefined contract was accepted.

## 7. Threat composition

Refute these claims:

- object callbacks add no unique T4 end phase; 04NO + 04N/09N compose without an extra event;
- T2+T16 scheduler semantics are origin-independent once NS-SEND queues until current command yields, but distinct
  DB/object/entity/doc origins still receive concrete probes;
- controlled T15 payload registers N-DB-MOD exactly; object-reactor behavior is separately covered and need not be
  silently inherited;
- `goodbye`, document veto or cleanup cannot produce a nominal PASS.

## 8. Closure versus execution

V29 claims only:

```text
NativeReactorClassSetClosed = TRUE
NativeProbeCatalogClosed = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

No SDK acceptance, header inspection, helper compilation or probe execution occurred. Consensus would lead only to a
separate LIMITED CT-DA RESEARCH AUTHORIZATION gate.

## 9. Product/residual review

Confirm HF-V27 HostToKind gates remain mandatory and product kinds remain OPEN. Native fixture closure cannot satisfy
DA-P7 kind coverage, DA-P8 applicability, DA-P10 readset, new subtype/event/scheduler coverage or KindContract closure.

## 10. Process/status

- Managed catalog remains HEC-V27-C1.
- V25 remains normative for KindContracts; V24/V20 architecture remains.
- V18/Freeze/O-1 govern; ADR-0036 remains proposed/amended for V18.
- G3 stopped, G3B not open, CT-50 not executed.
- No implementation or CT-DA is authorized.

## 11. Expected report

Report BLOCKER/HIGH/MEDIUM/LOW with exact location, counterexample and correction. Choose exactly one:

1. `AGREED WITH PROPOSAL V29 — HOST FIXTURE CONTRACT READY`;
2. `CHANGES REQUIRED — PROPOSAL V30`;
3. `ALT-21C REJECTED — V18 REMAINS GOVERNING`.
