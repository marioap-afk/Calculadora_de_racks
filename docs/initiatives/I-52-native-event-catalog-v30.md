# I-52 — Native Event Catalog V30

> **NEC-V30-1 / NORMATIVE CORRECTION.** Parent NEC-V29-1 blob
> `350d01e4bfe87791fba11d020282de7e028110a4`. V29 callback authority and HF-bounded class census are
> incorporated unchanged. NPM-V30-1 is the only source of probe relationships.

## 1. Preserved authority and class set

The exact Autodesk 2025 authority, callback signatures, registration/lifetime rules and IN/OUT census in
NEC-V29-1 remain incorporated. Header hashes remain `NOT OBSERVED` until the separately authorized research gate.

```text
NativeReactorClassSetClosed(HF-V29-1) = TRUE
```

## 2. Relation semantics and projection

For each NPM row:

```text
PrimaryAuthorityId matching NativeEventId -> PRIMARY_FOR
ScheduleOriginEventId matching NativeEventId -> SCHEDULE_ORIGIN_FOR
each LifecycleMarkers token matching NativeEventId -> MARKER_FOR
explicit IncidentalEventId -> INCIDENTAL_FOR
```

NPM-V30 has no IncidentalEventId field and publishes no incidental relation. Marker ContractIds are projected in
the separate Event-to-Contract table when relevant; they never enter Event-to-Probe cells.

```text
EventProjection(NPM) = set of (NativeEventId, ProbeId, relation type) derived above
SchedulerProjection(NPM) = set of (NativeSchedulerId, ProbeId, PRIMARY_FOR)

InverseProjectionClosed iff
  EventMatrix = EventProjection(NPM)
  AND SchedulerMatrix = SchedulerProjection(NPM)
  AND every ProbeId exists exactly once in NPM
  AND every relation type is preserved
  AND no aggregate, wildcard or non-ProbeId label occurs in a ProbeId cell.
```

## 3. Exact Event-to-Probe projection

Empty sets are written `NONE`. Every non-NONE value is a concrete ProbeId.

| NativeEventId | PRIMARY_FOR | SCHEDULE_ORIGIN_FOR | MARKER_FOR | INCIDENTAL_FOR |
|---|---|---|---|---|
| N-DB-ERASE | NONE | NONE | 02NO-SM | NONE |
| N-DB-MOD | NONE | 16N-S; 16N-M; 16N-SM; C15N16N-SM | 02N | NONE |
| N-DB-OPEN | 02N | NONE | NONE | NONE |
| N-DOC-LOCK-CHANGED | 10NDOC-CHANGED-SM | C2DOCC16N-SM | 10NDOC-WILL-SM; 10NDOC-VETO-SM; C2DOCW16N-SM | NONE |
| N-DOC-LOCK-VETO | 10NDOC-VETO-SM | NONE | 10NDOC-WILL-SM; 10NDOC-CHANGED-SM | NONE |
| N-DOC-LOCK-WILL | 10NDOC-WILL-SM | C2DOCW16N-SM | 10NDOC-CHANGED-SM; 10NDOC-VETO-SM; C2DOCC16N-SM | NONE |
| N-ED-CANCEL | NONE | NONE | 04N; 10NDOC-VETO-SM | NONE |
| N-ED-END | 09N-D; 10N-S; 10N-M; 10N-SM | NONE | 09N-C; 16N-S; 16N-M; 16N-SM; C15N16N-SM; C2OBJ16N-S; C2OBJ16N-M; C2OBJ16N-SM; C2ENT16N-M; C2DOCW16N-SM; C2DOCC16N-SM | NONE |
| N-ED-FAIL | NONE | NONE | 04N | NONE |
| N-ED-WILL | NONE | NONE | 16N-S; 16N-M; 16N-SM; C15N16N-SM; C2OBJ16N-S; C2OBJ16N-M; C2OBJ16N-SM; C2ENT16N-M; C2DOCW16N-SM; C2DOCC16N-SM | NONE |
| N-ENT-GFX | 02NE-M | C2ENT16N-M | 02NO-M | NONE |
| N-OBJ-CANCEL | 04NO-S | NONE | 04NO-UNDO-S | NONE |
| N-OBJ-CLOSED | 02NO-CLOSE-SM | NONE | 02NO-S; 02NO-M; 02NO-SM; 04NO-S; 04NO-UNDO-S; 02NE-M; C2OBJ16N-S; C2OBJ16N-M; C2OBJ16N-SM; C2ENT16N-M | NONE |
| N-OBJ-ERASE | 02NO-SM | NONE | NONE | NONE |
| N-OBJ-MOD | 02NO-M | C2OBJ16N-S; C2OBJ16N-M; C2OBJ16N-SM | 02NO-S; 02NO-CLOSE-SM; 04NO-S; 02NE-M | NONE |
| N-OBJ-OPEN | 02NO-S | NONE | 02NO-M | NONE |
| N-OBJ-UNDO | 04NO-UNDO-S | NONE | 04NO-S | NONE |
| N-RX-LOADED | NONE | NONE | C15N16N-SM | NONE |
| N-TR-ABORTED | NONE | NONE | 04N | NONE |
| N-TR-ABOUT-ABORT | 04N | NONE | NONE | NONE |
| N-TR-ABOUT-END | 09N-A; 09N-B | NONE | NONE | NONE |
| N-TR-ENDED | 09N-C | NONE | 09N-A; 09N-B; 09N-D; 10N-S; 10N-M; 10N-SM | NONE |

## 4. Exact Scheduler-to-Probe projection

| NativeSchedulerId | PRIMARY_FOR |
|---|---|
| NS-SEND | 16N-S; 16N-M; 16N-SM; C15N16N-SM; C2OBJ16N-S; C2OBJ16N-M; C2OBJ16N-SM; C2ENT16N-M; C2DOCW16N-SM; C2DOCC16N-SM |
| NS-BEGIN-CMDCTX | NONE |

## 5. Event-to-Contract relationships

This table does not provide ProbeId credit and is excluded from EventProjection(NPM).

| NativeEventId | ContractId | Purpose |
|---|---|---|
| N-OBJ-GOODBYE | CLEAN-OBJ | mark notifier association dead so cleanup never calls removeReactor on it |

## 6. Mechanical comparison and closure

The validation script parses NPM-V30, expands only concrete comma-separated EventIds and compares sorted relation
triples with the two published projections.

```text
Event projection missing triples = 0
Event projection extra triples = 0
Scheduler projection missing pairs = 0
Scheduler projection extra pairs = 0
Non-ProbeId labels in ProbeId cells = 0
InverseProjectionClosed = TRUE
```

Together with NPM-V30 validation:

```text
NativeReactorClassSetClosed(HF-V29-1) = TRUE
NativeProbeCatalogClosed(NEC-V30-1,NPM-V30-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V30-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

These are definition statuses only. No callback, scheduler or probe has been executed.
