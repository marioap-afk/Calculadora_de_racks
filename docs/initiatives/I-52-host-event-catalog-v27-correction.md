# I-52 — HEC-V27-C1: managed probe catalog correction

> **NORMATIVE ADDITIVE CORRECTION.** Parent HEC-V27-1 blob:
> `717188aca5e11be9ddfb886f602dcfc47171732c`. Sustituye sólo el catalogo managed y su closure calculation.

## 1. Schema de probe normalizado

```text
ProbeId; PrimaryExecutionAuthority; PrimaryTriggerEventId; SchedulerId; DirectActionId;
LifecycleMarkerEventIds[]; OptionalIncidentalEventIds[]; Threats; S/M/SM; Mutation;
Verifier; DA-P/H-P; PASS; FAIL; UNKNOWN; Cleanup.
```

Exactamente uno de `PrimaryTriggerEventId`, `SchedulerId` y `DirectActionId` es distinto de `NONE`.

Direct actions:

| DirectActionId | Accion sincrona controlada |
|---|---|
| MD-RYOW | writer staged y reader fresh dentro de primary T |
| MD-ABORT | abort explicito de primary T despues de ejecutar probes prerequisito |
| MD-COMMIT | commit explicito de primary T despues de ejecutar probes prerequisito |

Estos IDs no son eventos ni schedulers host y no transfieren evidencia fuera de la accion descrita.

## 2. Matriz normativa ProbeId→authority

`PASS` exige trigger/scheduler/direct action exacto, before/after conforme al verifier, orden total y cleanup. Mutacion
no detectada, estado final incorrecto o autoridad distinta es `FAIL`. Campo obligatorio no observable, callback no
ejecutado o contexto/T no resoluble es `UNKNOWN`.

| ProbeId | PrimaryTriggerEventId | SchedulerId | DirectActionId | LifecycleMarkerEventIds | OptionalIncidentalEventIds | Threats | Clase / mutacion | Verifier | DA-P/H-P | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| 01 | M-DB-MOD | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN | T1,T5,T11 | S / destination Xrecord | SemanticVerifier + T ids | P1,P2 | DEFINED |
| 02 | M-DB-OPEN | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-MOD | T1,T11 | S / destination Xrecord con primary T | SemanticVerifier + T ids | P1,P4 | DEFINED |
| 03 | M-DB-MOD | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN | T3 | S,M / nested y nueva T sobre targets fixture | SemanticVerifier; PhysicalVerifier; T ids | P3 | DEFINED |
| 04 | NONE | NONE | MD-ABORT | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN; M-DB-MOD | T3 | S,M / abort tras 02 y 03 | SemanticVerifier; PhysicalVerifier | P3,P4 | DEFINED |
| 05 | NONE | NONE | MD-COMMIT | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN; M-DB-MOD | T2,T3,T5 | S,M / commit tras 02 y 03 | SemanticVerifier; PhysicalVerifier | P2,P3,P4 | DEFINED |
| 06 | M-DB-OPEN | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-MOD | T1,T11 | S / reemplazo Xrecord.Data | SemanticVerifier + event order | P1,P2,P4 | DEFINED |
| 07 | NONE | NONE | MD-RYOW | M-DOC-END | NONE | T11 | S,M / staged definition y Xrecord | SemanticVerifier; PhysicalVerifier | P4 | DEFINED |
| 08 | M-DB-MOD | NONE | NONE | M-DOC-END | M-DB-ERASE; M-DB-APPEND | T7 | S,SM / mutar B entre read A y read B | SemanticVerifier; SiblingVerifier | P5 | DEFINED |
| 09C | M-DB-MOD | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN | T2,T4,T6 | S,M / object-modification-close ordering | SemanticVerifier; PhysicalVerifier; event order | P2 | DEFINED |
| 09E | M-DOC-END | NONE | NONE | M-LOCK-CHANGED | NONE | T4,T6,T8 | S,M / nueva T y write attempt despues de CommandEnded | SemanticVerifier; PhysicalVerifier; lifecycle order | P2,P6,P11,P12 | DEFINED |
| 10S | M-DB-MOD | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN | T8,T9,T10,T11 | S / destination semantic Xrecord | SemanticVerifier; LifecycleVerifier | P6,P12 | DEFINED |
| 10M | M-DB-MOD | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-OPEN | T8,T12 | M / transform o layer | PhysicalVerifier; LifecycleVerifier | P11,P12 | DEFINED |
| 10SM | M-DB-MOD | NONE | NONE | M-DOC-END; M-LOCK-CHANGED | M-DB-ERASE; M-DB-APPEND | T8,T11,T12 | SM / sibling y linkage | SemanticVerifier; SiblingVerifier; PhysicalVerifier | P6,P11,P12 | DEFINED |
| 11 | M-DB-MOD | NONE | NONE | M-DOC-END | M-DB-OPEN | T9 | S / F-NOD Xrecord | NodVerifier | P8 | DEFINED |
| 12 | M-DB-MOD | NONE | NONE | M-DOC-END | M-DB-ERASE; M-DB-APPEND | T7,T12 | M,SM / mutar physical B entre scans | PhysicalVerifier; SiblingVerifier | P9 | DEFINED |
| 13 | NONE | MS-CONTEXT | NONE | M-LOCK-WILL; M-LOCK-CHANGED; M-LOCK-VETO; M-DOC-END | NONE | T3,T13 | S,M,SM / lock, T y write desde otro context | SemanticVerifier; PhysicalVerifier; SiblingVerifier | P3,P12 | DEFINED |
| 14 | M-DB-MOD | NONE | NONE | M-DOC-END | M-DB-OPEN | T10 | S / F-SRC despues de S | SemanticVerifier; LifecycleVerifier | P10,P12 | DEFINED |
| 15 | M-DB-MOD | NONE | NONE | M-DOC-WILL; M-DOC-END | M-DB-OPEN; M-DB-ERASE; M-DB-APPEND | T15 | SM / source + destination/linkage desde callback registrado | SemanticVerifier; SiblingVerifier | P6,P10,P11,P12 | DEFINED |
| 16S-SEND | NONE | MS-SEND | NONE | M-DOC-WILL; M-DOC-END; M-LOCK-CHANGED | NONE | T8,T9,T10,T16 | S / destination Xrecord | SemanticVerifier; LifecycleVerifier | P6,P12 | DEFINED |
| 16M-SEND | NONE | MS-SEND | NONE | M-DOC-WILL; M-DOC-END; M-LOCK-CHANGED | NONE | T8,T12,T16 | M / transform o layer | PhysicalVerifier; LifecycleVerifier | P11,P12 | DEFINED |
| 16SM-SEND | NONE | MS-SEND | NONE | M-DOC-WILL; M-DOC-END; M-LOCK-CHANGED | M-DB-ERASE; M-DB-APPEND | T8,T11,T12,T16 | SM / sibling y linkage | SemanticVerifier; SiblingVerifier; PhysicalVerifier | P6,P11,P12 | DEFINED |
| 16S-CONTEXT | NONE | MS-CONTEXT | NONE | M-DOC-END; M-LOCK-WILL; M-LOCK-CHANGED; M-LOCK-VETO | NONE | T8,T9,T10,T13,T16 | S / destination Xrecord | SemanticVerifier; LifecycleVerifier | P6,P12 | DEFINED |
| 16M-CONTEXT | NONE | MS-CONTEXT | NONE | M-DOC-END; M-LOCK-WILL; M-LOCK-CHANGED; M-LOCK-VETO | NONE | T8,T12,T13,T16 | M / transform o layer | PhysicalVerifier; LifecycleVerifier | P11,P12 | DEFINED |
| 16SM-CONTEXT | NONE | MS-CONTEXT | NONE | M-DOC-END; M-LOCK-WILL; M-LOCK-CHANGED; M-LOCK-VETO | M-DB-ERASE; M-DB-APPEND | T8,T11,T12,T13,T16 | SM / sibling y linkage | SemanticVerifier; SiblingVerifier; PhysicalVerifier | P6,P11,P12 | DEFINED |
| 16S-IDLE | NONE | MS-IDLE | NONE | M-IDLE; M-DOC-END; M-LOCK-CHANGED | NONE | T8,T9,T10,T16 | S / destination Xrecord | SemanticVerifier; LifecycleVerifier | P6,P12 | DEFINED |
| 16M-IDLE | NONE | MS-IDLE | NONE | M-IDLE; M-DOC-END; M-LOCK-CHANGED | NONE | T8,T12,T16 | M / transform o layer | PhysicalVerifier; LifecycleVerifier | P11,P12 | DEFINED |
| 16SM-IDLE | NONE | MS-IDLE | NONE | M-IDLE; M-DOC-END; M-LOCK-CHANGED | M-DB-ERASE; M-DB-APPEND | T8,T11,T12,T16 | SM / sibling y linkage | SemanticVerifier; SiblingVerifier; PhysicalVerifier | P6,P11,P12 | DEFINED |

## 3. Module registration de probe 15

```text
LoadActionId = ML-CONTROLLED
Load = controlled research module load before the research command
RegisteredEventClassId = M-DB-MOD
RegisteredProbeId = 15
Mutation trigger = M-DB-MOD callback, not module load
Unregister = finally/cleanup by the same delegate identity
```

El load marker prueba registro. La mutacion sólo recibe autoridad desde `M-DB-MOD`.

## 4. Matriz inversa EventId→Probe

`PRIMARY FOR` se deriva exclusivamente de `PrimaryTriggerEventId`. `MARKER FOR` e `INCIDENTAL FOR` no son triggers.

| EventId | PRIMARY FOR | MARKER FOR | INCIDENTAL FOR |
|---|---|---|---|
| M-DB-OPEN | 02; 06 | NONE | 01; 03; 04; 05; 09C; 10S; 10M; 11; 14; 15 |
| M-DB-MOD | 01; 03; 08; 09C; 10S; 10M; 10SM; 11; 12; 14; 15 | NONE | 02; 04; 05; 06 |
| M-DB-ERASE | NONE | NONE | 08; 10SM; 12; 15; 16SM-SEND; 16SM-CONTEXT; 16SM-IDLE |
| M-DB-APPEND | NONE | NONE | 08; 10SM; 12; 15; 16SM-SEND; 16SM-CONTEXT; 16SM-IDLE |
| M-DOC-WILL | NONE | 15; 16S-SEND; 16M-SEND; 16SM-SEND | NONE |
| M-DOC-END | 09E | 01; 02; 03; 04; 05; 06; 07; 08; 09C; 10S; 10M; 10SM; 11; 12; 13; 14; 15; 16S-SEND; 16M-SEND; 16SM-SEND; 16S-CONTEXT; 16M-CONTEXT; 16SM-CONTEXT; 16S-IDLE; 16M-IDLE; 16SM-IDLE | NONE |
| M-DOC-CANCEL | NONE | NONE | NONE |
| M-DOC-FAIL | NONE | NONE | NONE |
| M-LOCK-WILL | NONE | 13; 16S-CONTEXT; 16M-CONTEXT; 16SM-CONTEXT | NONE |
| M-LOCK-CHANGED | NONE | 01; 02; 03; 04; 05; 06; 09C; 09E; 10S; 10M; 10SM; 13; 16S-SEND; 16M-SEND; 16SM-SEND; 16S-CONTEXT; 16M-CONTEXT; 16SM-CONTEXT; 16S-IDLE; 16M-IDLE; 16SM-IDLE | NONE |
| M-LOCK-VETO | NONE | 13; 16S-CONTEXT; 16M-CONTEXT; 16SM-CONTEXT | NONE |
| M-IDLE | NONE | 16S-IDLE; 16M-IDLE; 16SM-IDLE | NONE |
| M-MODELESS-WILL | NONE | NONE | NONE |
| M-MODELESS-END | NONE | NONE | NONE |

Los eventos cancel/fail/modeless permanecen EventIds reflejados para observacion futura, pero ninguna obligation managed
actual los usa como evidencia. Agregar un probe que dependa de ellos exige nueva fila concreta e invalida closure.

## 5. SchedulerId→Probe

| SchedulerId | API exacta | ProbeIds concretos |
|---|---|---|
| MS-SEND | `Autodesk.AutoCAD.ApplicationServices.Document.SendStringToExecute(string,bool,bool,bool)` | 16S-SEND; 16M-SEND; 16SM-SEND |
| MS-CONTEXT | `Autodesk.AutoCAD.ApplicationServices.DocumentCollection.ExecuteInCommandContextAsync(Func<object,Task>,object)` | 13; 16S-CONTEXT; 16M-CONTEXT; 16SM-CONTEXT |
| MS-IDLE | one-shot handler sobre M-IDLE | 16S-IDLE; 16M-IDLE; 16SM-IDLE |

Cada scheduler probe registra SchedulePoint, ExecutionPoint, thread/context, T, lock, `C_decide`, `C_report`, `C_host`,
before/after y cleanup. No espera temporal sustituye la entrega del host.

## 6. M-IDLE exacto

```text
EventId = M-IDLE
Assembly = AcCoreMgd.dll
AssemblyVersion = 25.0.0.0
API type = Autodesk.AutoCAD.ApplicationServices.Core.Application
Exact event = Idle
Publisher = static Autodesk.AutoCAD.ApplicationServices.Core.Application
Handler = System.EventHandler
Signature = (object sender, System.EventArgs e)
Registration = Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle += handler
Deregistration = Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle -= handler
```

El handler queda almacenado por identidad. Se registra en el SchedulePoint exacto, registra la primera invocacion como
ExecutionPoint y se desregistra en `finally`/cleanup. T y lock se observan. La mutacion empieza sólo despues de obtener
contexto y T legales. Si no hay invocacion, contexto legal o medicion obligatoria, el probe es `UNKNOWN` o `FAIL` segun
si falta evidencia o se contradice el comportamiento esperado; nunca PASS por timeout.

## 7. Distinciones 09 y 10

- 09C: `M-DB-MOD`, orden asociado a modificacion/cierre de objeto. No es commit-phase.
- 09E: `M-DOC-END`, intento en nueva T despues de la notificacion. No es commit interno ni `C_host` automatico.
- 10S: `M-DB-MOD`, Xrecord semantic.
- 10M: `M-DB-MOD`, transform/layer.
- 10SM: `M-DB-MOD`, sibling/linkage.

Para los tres probes 10, `M-DOC-END` y `M-LOCK-CHANGED` son markers, no triggers.

## 8. Closure managed

```text
ManagedProbeCatalogClosed(M) iff
  every required managed ProbeId has one concrete row
  AND exactly one of EventId, SchedulerId or DirectActionId is primary
  AND LifecycleMarkerEventIds and OptionalIncidentalEventIds are non-primary
  AND every EventId and SchedulerId exists in the reflected exact API
  AND every probe defines mutation, class, verifier, oracle, DA-P and cleanup
  AND no wildcard, range or compressed family is normative
  AND inverse EventId/Scheduler mappings agree with the primary matrix
  AND M-IDLE exact identity and one-shot contract are present
```

Resultado de definicion:

```text
ManagedProbeCatalogClosed(M) = TRUE
NativeProbeCatalogClosed(N) = FALSE
EventCatalogClosed(H) = FALSE
HostFixtureContractClosed(HF-V27-1) = FALSE
```

## 9. Native y product gates

El catalogo nativo de HEC-V27-1 permanece sin cambio y OPEN. Esta correccion no acredita headers ObjectARX 2025,
native callbacks ni native scheduler. Los ResidualGate, PARTIAL rule, `HostToKindCompatibilityPass(k)`,
`KindSpecificCoveragePass(k)` y `CTDA_PASS(k)` de HF-V27-1 permanecen normativos; sólo sus referencias a managed probes
se interpretan mediante los IDs concretos de HEC-V27-C1.

## 10. Invalidacion

Cambio en DLL/version/hash, API type, event signature, primary authority, marker, scheduler binding, mutation, verifier,
oracle, cleanup o inverse mapping invalida la fila managed afectada y recalcula `ManagedProbeCatalogClosed`.
