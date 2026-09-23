# I-52 — Proposal V27 Correction: managed probe catalog normalization

> **DESIGN / DOCS ONLY.** Corrige exclusivamente el catalogo managed publicado en V27. No implementa harness,
> ObjectARX, CT-DA, RACKMIRROR ni AUTH-15.

## 1. Identidad

```text
Parent V27 SHA = 4299fe52b82c4e66131c519ec274be31da8b9295
Proposal V27 blob = c42e8f42024ea5947413d68b9c8dba8c3f025860
HF-V27-1 blob = 6ccd1b2b170fbaa7704d57c1cc26df0054dfc74a
HEC-V27-1 blob = 717188aca5e11be9ddfb886f602dcfc47171732c
Architect package V27 blob = 0f4e036b71d2520d999872ad7bcca7d94036edb6
Decisions V27 blob = 73bf263dc1a87761739a918a94bf85b6fd90329c
origin/main = 7097057cf8685bf5ecc09083cba37379d4a4aae8
I-55 = 08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d
```

Los artefactos V27 originales permanecen historicos e inmutables. Esta correccion sustituye solo las tablas managed,
la identidad de M-IDLE y el calculo de `ManagedProbeCatalogClosed`.

## 2. Hallazgos Architect registrados

```text
Architect =
CHANGES REQUIRED — PROPOSAL V27 CORRECTION

BLOCKER =
ManagedProbeCatalogClosed = TRUE contradicts the normative catalog

HIGH =
M-IDLE does not identify the exact API type

V18 =
GOVERNING
```

## 3. Delta de correccion

La correccion:

1. reemplaza todo ID agregado o wildcard por ProbeIds concretos;
2. asigna exactamente una autoridad primaria a cada probe;
3. separa trigger, scheduler, accion directa, markers e incidental events;
4. reconstruye EventId→Probe y SchedulerId→Probe desde las definiciones normalizadas;
5. fija `Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle` en `AcCoreMgd.dll`;
6. recalcula el catalogo managed.

No cambia native authority, T2, T2+T16, HostToKind, KindContracts ni formulas productivas.

## 4. Modelo normativo de autoridad primaria

Cada ProbeId declara exactamente una de estas autoridades:

```text
EventAuthority(EventId)
SchedulerAuthority(SchedulerId)
DirectHarnessAuthority(DirectActionId)
```

`DirectHarnessAuthority` sólo cubre una accion sincrona y determinista del research harness; no es evento ni scheduler
host. Se usa para RYOW, commit y abort iniciados directamente por el probe. Un probe no puede declarar mas de una
autoridad primaria.

`LifecycleMarkerEventIds` sólo aportan orden total. `OptionalIncidentalEventIds` registra eventos que pueden observarse
como consecuencia secundaria. Ninguno adquiere autoridad de trigger.

## 5. Inventario concreto

```text
01 02 03 04 05 06 07 08 09C 09E 10S 10M 10SM 11 12 13 14 15
16S-SEND 16M-SEND 16SM-SEND
16S-CONTEXT 16M-CONTEXT 16SM-CONTEXT
16S-IDLE 16M-IDLE 16SM-IDLE
```

La lista anterior es inventario descriptivo. Las tablas normativas del catalogo de correccion contienen una fila
individual por cada ID y no usan ranges, asteriscos ni familias comprimidas.

## 6. Casos corregidos

- `01`: autoridad primaria `M-DB-MOD`; no aparece como primary bajo `M-DB-OPEN`.
- `02`: autoridad primaria `M-DB-OPEN`; no aparece como primary bajo `M-DB-MOD`.
- `06`: primary `M-DB-OPEN`; `M-DB-MOD` es incidental para medir la secuencia del reemplazo.
- `09C`: primary `M-DB-MOD`; object-modification/object-close-associated ordering, nunca commit-phase.
- `09E`: primary `M-DOC-END`; caracteriza una mutacion posterior a la notificacion, no commit interno.
- `10S`, `10M`, `10SM`: primary `M-DB-MOD`; `M-DOC-END` y lock changes son markers.
- `13`: primary `MS-CONTEXT`; no se inventa EventId.
- `15`: `ML-CONTROLLED` carga el modulo, registra `M-DB-MOD` y el callback de probe 15 realiza la mutacion.
- cada variante 16 declara uno de `MS-SEND`, `MS-CONTEXT` o `MS-IDLE`.

## 7. Cierre managed recalculado

```text
ManagedProbeCatalogClosed(M) iff
  every required managed ProbeId is concrete
  AND every probe has exactly one PrimaryExecutionAuthority
  AND every lifecycle marker is separate from primary execution
  AND every EventId exists in the reflected exact API
  AND every SchedulerId exists in the reflected exact API
  AND every probe defines mutation, verifier and oracle
  AND no wildcard, range or family shorthand is normative
  AND no conflicting primary binding exists
  AND M-IDLE has its exact publisher, assembly, handler and registration contract
```

HEC-V27-C1 satisface esas condiciones. Por tanto:

```text
ManagedProbeCatalogClosed = TRUE
NativeProbeCatalogClosed = FALSE
EventCatalogClosed = FALSE
HostFixtureContractClosed = FALSE
CTDA_HOST_PASS = NOT EVALUATED / NOT EXECUTABLE
```

## 8. Native catalog y host→kind

Sin cambios:

- ObjectARX 2025 exact headers siguen ausentes;
- `16N-S`, `16N-M`, `16N-SM` siguen `BLOCKED_UNDEFINED / UNKNOWN`;
- T2+T16 sigue `UNKNOWN`;
- managed emulation no satisface T2;
- todos los ResidualGate y `HostToKindCompatibilityPass(k)` de HF-V27-1 permanecen normativos;
- `KindSpecificCoveragePass(k)` y `CTDA_PASS(k)` no cambian.

## 9. I-55 y producto

I-55 `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d` permanece: impacto host event catalog `NON-MATERIAL`,
HostToKind/KindContract `MATERIAL`, reconciliacion `REQUIRED`, evidence transfer `NONE`. Su partial placement no
sustituye atomicidad I-52.

```text
KindContractClosed(Selective) = FALSE
Dynamic = OPEN
PushBack = OPEN
Cantilever = OPEN
Header = OPEN
Flow Bed = OPEN
```

Foundation AUTH-01..13 e I-49 permanecen por referencia. AUTH-15 sigue `I-52 OWNED / NOT IMPLEMENTED`.

## 10. Disposiciones y proceso

V27 original queda historica. HEC-V27-C1 sustituye sólo sus mappings managed, wildcard notation, M-IDLE y status
managed. V25 sigue normativa para KindContracts; V24 conserva boundaries, DA-P12 y T16; V20 permanece
`PRESERVED / RESTRICTED`.

V28 no comienza con esta publicacion. Requiere revision exacta y consenso de la correccion; despues abordara autoridad
ObjectARX 2025 y native scheduler.

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
G3 = STOPPED
G3B = NOT OPEN
```

## 11. Self-refutation

1. Normative wildcard: **NO**.
2. Cada probe tiene una autoridad primaria: **YES**.
3. Markers separados de trigger: **YES**.
4. Contradicciones 01/02: **REMOVED**.
5. M-DOC-END como trigger implicito de 10S/M/SM: **NO**.
6. Variantes 16 expandidas: **YES**.
7. M-IDLE publisher exacto: **YES**.
8. NativeProbeCatalog closed: **NO / FALSE**.
9. HostFixtureContract closed: **NO / FALSE**.
10. Esta correccion autoriza V28 o CT-DA: **NO**.

## 12. Review checklist

- [ ] comprobar parent SHA y blobs;
- [ ] comprobar que cada ProbeId aparece una vez en la matriz primaria;
- [ ] buscar wildcard/range en celdas normativas;
- [ ] comprobar EventId→Probe por reconstruccion inversa;
- [ ] comprobar M-IDLE mediante reflection exacta;
- [ ] confirmar native status sin cambio;
- [ ] confirmar HostToKind sin rediseño;
- [ ] confirmar que V28 y CT-DA siguen bloqueados.

```text
PROPOSAL V27 CORRECTION = PUBLISHED / REVIEW REQUIRED
MANAGEDPROBECATALOG = CLOSED
NATIVEPROBECATALOG = OPEN
EVENTCATALOG = OPEN
HOSTFIXTURECONTRACT = NOT CLOSED
CT-DA RESEARCH CONTRACT = NOT READY
V28 = BLOCKED UNTIL CORRECTION CONSENSUS
TECHNICAL CONSENSUS = NOT REACHED

PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
