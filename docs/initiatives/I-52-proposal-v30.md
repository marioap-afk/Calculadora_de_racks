# I-52 — Proposal V30: normalizacion del contrato de probes nativos

> **DOCS / DEFINITION CORRECTION ONLY.** No ejecuta CT-DA, no descarga SDK, no compila helper y no
> implementa RACKMIRROR ni AUTH-15.

## 1. Status e identidades

| Campo | Valor |
|---|---|
| Starting I-52 / Proposal V29 | `e4e506303f81370db75e8e7e8bf1000c0e7920c3` |
| Proposal V29 blob | `a36f17c3f3a8947671d4ed5dc1f9346d0a3dbcf2` |
| NEC-V29-1 blob | `350d01e4bfe87791fba11d020282de7e028110a4` |
| NPM-V29-1 blob | `51bf40af1b607fe428263213061555ce323c6742` |
| Architect package V29 blob | `5f855b75721bb324bc65a0b90af670b4cac9d4f3` |
| Decisions V29 blob | `0b6988e1d8f0e4db9496aab4430702c49066a661` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d` |
| I-57 object / target | `a5bc02210f740839e2fac37e63fc00512e5ccee6` / `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Native event catalog | `NEC-V30-1` |
| Native probe matrix | `NPM-V30-1` |
| Host fixture revision | `HF-V30-1` |

Preflight: rama, HEAD, upstream e I-52 coinciden; el arbol esta limpio, stash vacio y no hay operacion Git
incompleta. Main, I-55 e I-57 coinciden con las identidades anteriores.

## 2. Findings Architect V29

```text
Architect = CHANGES REQUIRED — PROPOSAL V30
BLOCKER-1 = NEC inverse Event-to-Probe matrix does not equal NPM projection and contains aggregate labels
BLOCKER-2 = PASS-S/M/SM and FAIL-S/M/SM do not resolve as concrete ContractIds
HIGH-1 = document-lock probes contain conditional ExpectedAfter values
HIGH-2 = CLEAN-C15 does not determine cleanup when unload is unavailable
LOW-1 = V29 checklist says 31 rows; normative NPM contains 29
V18 = GOVERNING
```

## 3. Delta V30

V30 publica NPM-V30-1 como fuente unica de relaciones, reconstruye NEC-V30-1 desde sus 29 filas, expande los
seis ContractIds de resultado, fija un ExpectedAfter por doc-lock probe y reemplaza cleanup opcional C15 por
terminacion verificada de un proceso AutoCAD scratch dedicado. El `31` historico queda clasificado como typo;
el universo normativo sigue en 29.

No cambia callbacks, class census, HEC-V27-C1, HostToKind, KindContract V25, lifecycle V24 ni arquitectura V20.

## 4. Class set preservado

```text
NativeReactorClassSetClosed(HF-V29-1) = TRUE
```

La correccion no encontro una nueva familia alcanzable. La identidad HF-V30-1 incorpora HF-V29-1 y sustituye
solo sus artefactos de mapping, oraculos y cleanup por NEC/NPM V30.

## 5. NPM como fuente de verdad y relaciones

`EventProjection(NPM)` toma PrimaryAuthorityId, ScheduleOriginEventId y cada NativeEventId de LifecycleMarkers.
`SchedulerProjection(NPM)` toma las filas cuyo PrimaryAuthorityId es scheduler. PRIMARY, ORIGIN, MARKER e
INCIDENTAL son relaciones distintas; NPM-V30 no publica incidentales. NEC no puede agregar, omitir ni reclasificar.

`InverseProjectionClosed` exige igualdad exacta de ambas proyecciones, existencia unica de cada ProbeId, tipos
preservados y cero agregados. NEC-V30 contiene solo ProbeIds concretos.

## 6. ContractIds concretos

NPM-V30 define literalmente `PASS-S`, `PASS-M`, `PASS-SM`, `FAIL-S`, `FAIL-M` y `FAIL-SM`. PASS-SM requiere a
la vez semantica, materializacion y sibling/linkage. FAIL exige contradiccion observable; ilegalidad o falta de
observabilidad sin contradiccion es `UNKNOWN-COMMON`. Todos los expected states, contextos, markers y cleanups
usados por las filas resuelven localmente; `+` es composicion ordenada de operandos definidos.

```text
ContractIdsResolved iff
  every referenced ContractId exists literally once
  AND composite operands resolve literally
  AND no shorthand expansion or circular definition is required
  AND expected authority is independent from the writer.
```

## 7. Document-lock determinista

- `10NDOC-WILL-SM`: mutator probe; ExpectedAfter=`STATE-SM-1`; no legal context/T means UNKNOWN.
- `10NDOC-CHANGED-SM`: mutator probe; ExpectedAfter=`STATE-SM-1`; no legal context/T means UNKNOWN.
- `10NDOC-VETO-SM`: observation/safety probe; no mutation is attempted; ExpectedAfter=`STATE-SM-0`; any write
  attributed to vetoed authority is FAIL.

No ExpectedAfter contiene `or`, `unless`, `if legal`, `if permitted` ni una rama opcional.

## 8. C15 cleanup determinista

`C15N16N-SM` se vincula exclusivamente a `CLEAN-C15-PROCESS`. El futuro gate ejecuta ese probe en un proceso
AutoCAD scratch dedicado, nunca en la sesion activa del usuario. Tras flush del log se intenta remover el reactor
retenido mientras el notifier sigue vivo y se registra el status; cualquier non-success impide PASS. Se cierra el DWG
sin guardar, se solicita salida limpia, se verifica que el PID exacto termino y
el harness elimina o pone en cuarentena los artefactos. La terminacion del proceso es el limite de residencia del
modulo; no existe rama de unload opcional. Falta de prueba de cleanup es UNKNOWN y nunca PASS.

PrimaryAuthorityId sigue `NS-SEND`; ScheduleOriginEventId sigue `N-DB-MOD`; load y registration son markers/setup.

## 9. Matrices y validacion mecanica

NPM-V30 conserva 29 ProbeIds. NEC-V30 publica la proyeccion completa y una tabla separada Event-to-Contract para
`N-OBJ-GOODBYE -> CLEAN-OBJ`. La validacion local analiza las filas semicolon-separated, los ContractIds y ambas
tablas inversas.

```text
ProbeIds = 29
unique ProbeIds = 29
fields != 24 = 0
zero/multiple primary authority = 0
blank cells = 0
missing ContractIds = 0
duplicate ContractIds = 0
unresolved ExpectedBefore/After operands = 0
conditional ExpectedAfter = 0
unresolved Cleanup = 0
normative wildcard/range = 0
Event projection missing/extra triples = 0/0
Scheduler projection missing/extra pairs = 0/0
InverseProjectionClosed = TRUE
ContractIdsResolved = TRUE
DeterministicExpectedState = TRUE
DeterministicCleanup = TRUE
```

## 10. Closure recalculada

```text
NativeReactorClassSetClosed(HF-V29-1) = TRUE
NativeProbeCatalogClosed(NEC-V30-1,NPM-V30-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V30-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

Closure define el instrumento. Todos los probes siguen `DEFINED / NOT EXECUTED`; un FAIL o UNKNOWN futuro mantiene
`CTDA_HOST_PASS=false`.

## 11. HostToKind, producto e I-55

Los residual gates V27 no cambian: EventClass, Scheduler, PrimitiveSet, LifecycleIntegration, DA-P7, DA-P8 y DA-P10
siguen obligatorios. Selective, Dynamic, PushBack, Cantilever, Header y Flow Bed permanecen OPEN.

I-55 sigue en `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`: native host `NON-MATERIAL`, HostToKind y KindContract
`MATERIAL`, reconciliacion `REQUIRED`, evidence transfer `NONE`. Su partial placement no cambia I-52 one-T,
one-commit, all-or-nothing.

Foundation AUTH-01..13 sigue por referencia. I-49 mantiene `SymbolId + complete RootCauses(variable)` por variable
realmente leida. AUTH-15 permanece `I-52 OWNED / NOT IMPLEMENTED`.

## 12. Disposiciones y proceso

- V29 queda historica; V30 sustituye solo mappings, ContractIds, ExpectedAfter, C15 cleanup, row-count typo y closure.
- HEC-V27-C1 sigue normativa; V25 conserva KindContracts; V24 lifecycle/resources; V20 `PRESERVED / RESTRICTED`.
- V18, Freeze V18 y O-1 V18 gobiernan; ADR-0036 sigue `PROPOSED / AMENDED FOR V18`.
- Tras reviews exactas y consenso V30, el siguiente gate es LIMITED CT-DA RESEARCH AUTHORIZATION; no es ejecucion.
- `G3=STOPPED`, `G3B=NOT OPEN`, `CT-50=NOT EXECUTED`.

## 13. Self-critique y review checklist

1. NEC igual a EventProjection: **YES**.
2. Agregados en Event-to-Probe: **NO**.
3. PASS/FAIL concretos: **YES**.
4. ContractId no resuelto: **NO**.
5. ExpectedAfter condicional: **NO**.
6. VETO acepta mutacion por lock vetado: **NO**.
7. C15 cleanup sin unload: **YES, dedicated process termination**.
8. Closure implica runtime PASS: **NO**.
9. V30 autoriza CT-DA: **NO**.
10. Checklist row count: **29**.

Architect debe reconstruir ambas proyecciones desde NPM, ejecutar la validacion mecanica, intentar romper los tres
doc probes y C15 cleanup, y confirmar que ninguna closure se propaga a producto.

```text
PROPOSAL V30 = PUBLISHED / REVIEW REQUIRED
NATIVEREACTORCLASSSET = CLOSED
NATIVEPROBECATALOG = CLOSED
EVENTCATALOG = CLOSED
HOSTFIXTURECONTRACT = CLOSED
CT-DA-HOST RESEARCH CONTRACT = READY FOR REVIEW
PRODUCT KINDCONTRACTS = STILL OPEN
TECHNICAL CONSENSUS = NOT REACHED

V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
