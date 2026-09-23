# I-52 — Architect review package Proposal V30

> Exact review of Proposal V30 / NEC-V30-1 / NPM-V30-1 / HF-V30-1. No implementation or CT-DA.

## 1. Exact parent authority

```text
Parent I-52 / V29 = e4e506303f81370db75e8e7e8bf1000c0e7920c3
Proposal V29 blob = a36f17c3f3a8947671d4ed5dc1f9346d0a3dbcf2
NEC-V29-1 blob = 350d01e4bfe87791fba11d020282de7e028110a4
NPM-V29-1 blob = 51bf40af1b607fe428263213061555ce323c6742
Architect package V29 blob = 5f855b75721bb324bc65a0b90af670b4cac9d4f3
Decisions V29 blob = 0b6988e1d8f0e4db9496aab4430702c49066a661
```

## 2. Findings under correction

Attempt to prove any of:

```text
NativeProbeCatalogClosed = TRUE AND EventMatrix != EventProjection(NPM)
NativeProbeCatalogClosed = TRUE AND any aggregate occupies a ProbeId cell
NativeProbeCatalogClosed = TRUE AND any ContractId reference is unresolved
NativeProbeCatalogClosed = TRUE AND any ExpectedAfter is conditional
NativeProbeCatalogClosed = TRUE AND C15 cleanup lacks a deterministic no-unload path
HostFixtureContractClosed = TRUE AND EventCatalogClosed = FALSE
```

Any satisfiable conjunction is BLOCKER.

## 3. Mechanical review procedure

Parse the two semicolon-delimited NPM code blocks. Require exactly 29 unique rows and 24 fields per row. Rebuild:

```text
(PrimaryAuthorityId, ProbeId, PRIMARY_FOR)
(ScheduleOriginEventId, ProbeId, SCHEDULE_ORIGIN_FOR)
(each native LifecycleMarker, ProbeId, MARKER_FOR)
```

Compare sorted triples against NEC-V30 with no normalization beyond whitespace and `NONE`. Rebuild scheduler pairs
from rows whose PrimaryAuthorityId is a scheduler. Expected missing and extra sets are both empty.

Extract literal ContractIds from the four NPM definition tables. For each matrix row resolve state/action, header,
T/lock/context, schedule/execution point, verifier, marker, result and cleanup references. `+` is a composition and
each operand must resolve separately. Expected results:

```text
rows = 29
unique = 29
bad field counts = 0
blank cells = 0
zero/multiple primary = 0
ContractId definitions = 55
duplicate ContractIds = 0
missing ContractIds = 0
conditional ExpectedAfter = 0
unresolved Cleanup = 0
Event relation triples expected/actual = 89/89
Event missing/extra = 0/0
Scheduler pairs expected/actual = 10/10
Scheduler missing/extra = 0/0
```

## 4. Targeted oracle review

- `10NDOC-WILL-SM` and `10NDOC-CHANGED-SM`: only `STATE-SM-1` can PASS; inability to obtain a legal T is UNKNOWN.
- `10NDOC-VETO-SM`: observation-only, no write attempt, `STATE-SM-0`; a write using vetoed authority is FAIL.
- `C15N16N-SM`: sole primary NS-SEND, origin N-DB-MOD, expected state from immutable contracts and cleanup by
  `CLEAN-C15-PROCESS` in a dedicated scratch AutoCAD process.
- Confirm process cleanup requires exact PID termination and never touches a user document/session.

## 5. Preserved boundaries

Do not reopen the V29 class census unless the correction exposes a reachable omitted class. HEC-V27-C1 remains
managed authority. HF-V27 HostToKind gates and V25 KindContracts remain mandatory. Closure is definition-only:
every probe is `DEFINED / NOT EXECUTED` and `CTDA_HOST_PASS=NOT EVALUATED`.

## 6. Required verdict

Report BLOCKER/HIGH/MEDIUM/LOW with exact location, counterexample and correction. Choose:

1. `AGREED WITH PROPOSAL V30 — HOST FIXTURE CONTRACT READY`;
2. `CHANGES REQUIRED — PROPOSAL V31`;
3. architecture rejection only if the correction reveals class-set impossibility.

Consensus would lead only to a separate LIMITED CT-DA RESEARCH AUTHORIZATION gate.
