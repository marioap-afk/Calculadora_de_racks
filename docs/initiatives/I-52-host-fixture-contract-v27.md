# I-52 — Host Fixture Contract V27

> **HF-V27-1 / NOT CLOSED / NOT EXECUTED.** Revision normativa de HF-V26-1 para exact event/probe catalogs y gates
> host→kind. No autoriza research ni producto.

## 1. Identidad

| Campo | Valor |
|---|---|
| Version | `HF-V27-1` |
| Parent contract | HF-V26-1 blob `f016b7f30bb4f552ff2cda493fe9e69f938f2f85` |
| Starting I-52 | `f4ecad56aabaa17b273431594994db9fe66145c4` |
| Main baseline | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Event catalog | `HEC-V27-1` |
| Fixture primitives/resources/verifiers | incorporados sin cambio desde HF-V26-1 |

V27 no muta HF-V26-1. Supera sus agregados `F-EVT/F-NATIVE` mediante el catalogo exacto V27 y agrega gates residuales.

## 2. Host fixture closure

```text
HostFixtureContractClosed(f) iff
  HF-V26FixtureCoreClosed(f)
  AND EventCatalogClosed(HEC-V27-1)
  AND ManagedProbeCatalogClosed(M)
  AND NativeProbeCatalogClosed(N)
  AND every required composed-threat obligation is DEFINED
  AND every representativeness residual has a named mandatory gate
  AND no wildcard, placeholder, BLOCKED_UNDEFINED or UNKNOWN remains
```

Definition completeness y execution result son distintos: un probe exacto puede ejecutar y fallar sin reabrir el
contrato. Un evento/probe no identificado impide cerrar el contrato antes de ejecutar.

Estado:

```text
HF-V26FixtureCoreClosed = TRUE
ManagedProbeCatalogClosed = TRUE
NativeProbeCatalogClosed = FALSE
EventCatalogClosed = FALSE
HostFixtureContractClosed(HF-V27-1) = FALSE
CTDA_HOST_PASS = NOT EVALUATED / NOT EXECUTABLE YET
```

El bloqueador concreto es `16N-S/M/SM`: no existe autoridad exacta disponible para un scheduler nativo 2025, y los
headers ObjectARX 2025 no estan instalados para verificar las demas firmas candidatas.

## 3. PARTIAL

```text
Representativeness(dap,k) = PARTIAL iff
  HostSubproperty(dap) is fully specified and testable
  AND ResidualGate(dap,k) is named and mandatory
  AND CTDA_PASS(k) depends on ResidualGate(dap,k)=PASS
```

Si el host subproperty conserva UNKNOWN, o falta gate, la fila es `UNKNOWN`, no PARTIAL.

## 4. Matriz de representatividad y ResidualGate

| DA-P | Host subproperty HF-V27 | Representatividad | ResidualGate obligatorio |
|---|---|---|---|
| 1 | Xrecord/dictionary same-context affiliation | PARTIAL | `SurfaceWriteCoverage(k)` |
| 2 | commit/close/callback visibility de EventIds catalogados | PARTIAL | `EventClassCoverageCompatible(k) AND PrimitiveSetCoverageCompatible(k)` |
| 3 | nested/secondary containment de estrategia fixture | PARTIAL | `TransactionStrategyCompatible(k)` |
| 4 | RYOW del storage/reader fixture | PARTIAL | `StoragePathEquivalent(k) AND ReaderPathEquivalent(k)` |
| 5 | scan semantico A/B del fixture | PARTIAL | `SemanticScanStrategyCompatible(k)` |
| 6 | fence semantico para EventIds/schedulers catalogados | PARTIAL | `EventClassCoverageCompatible(k) AND SchedulerCoverageCompatible(k) AND LifecycleIntegrationCompatible(k)` |
| 7 | observabilidad de todas las surfaces fisicas fixture | PARTIAL | `DA-P7_KIND_COVERAGE(k)` |
| 8 | semantica NOD del fixture | PARTIAL | `DA-P8_KIND_APPLICABILITY(k) AND StoragePathEquivalent(k)` |
| 9 | scan fisico A/B/nested fixture | PARTIAL | `PhysicalScanStrategyCompatible(k) AND PrimitiveSetCoverageCompatible(k)` |
| 10 | source ordering controlado | PARTIAL | `DA-P10_KIND_READSET(k) AND SourceOrderingModelCompatible(k)` |
| 11 | fence fisico de primitives/eventos fixture | PARTIAL | `PrimitiveSetCoverageCompatible(k) AND EventClassCoverageCompatible(k) AND SchedulerCoverageCompatible(k)` |
| 12 | decision boundary del command fixture | PARTIAL | `LifecycleIntegrationCompatible(k) AND SchedulerCoverageCompatible(k) AND LockingStrategyCompatible(k)` |

No hay residual de DA-P1..12 que viva solo en prosa.

## 5. HostToKindCompatibilityPass

```text
HostToKindCompatibilityPass(k) iff
  SurfaceWriteCoverage(k) = PASS
  AND TransactionStrategyCompatible(k) = PASS
  AND StoragePathEquivalent(k) = PASS
  AND ReaderPathEquivalent(k) = PASS
  AND SemanticScanStrategyCompatible(k) = PASS
  AND PhysicalScanStrategyCompatible(k) = PASS
  AND EventClassCoverageCompatible(k) = PASS
  AND SchedulerCoverageCompatible(k) = PASS
  AND PrimitiveSetCoverageCompatible(k) = PASS
  AND LifecycleIntegrationCompatible(k) = PASS
  AND LockingStrategyCompatible(k) = PASS
  AND SourceOrderingModelCompatible(k) = PASS
```

Cada predicate compara manifest versionado del fixture con el manifest del kind. `UNKNOWN` o diferencia sin evidencia
adicional produce `FALSE` para admision.

`SurfaceWriteCoverage(k)` enumera cada surface S/M/SM del kind que puede escribirse desde una clase de evento IN y exige
EventId ya caracterizado o probe kind-specific. Una surface omitida o condicional produce `FALSE/UNKNOWN`.

### 5.1 TransactionStrategyCompatible(k)

Compara primary T, nested/new T, secondary T, commit/abort order, lock acquire/release y fronteras preflight/mutation.
Misma clase API con otro orden no es compatible automaticamente.

### 5.2 StoragePathEquivalent(k)

Compara owner object, extension dictionary hierarchy, Xrecord schema/ResultBuffer access, NOD y cualquier storage
alternativo. Cada path diferente requiere kind evidence.

### 5.3 ReaderPathEquivalent(k)

Exige que el reader de producto abra los mismos DBObjects bajo semantica transaccional equivalente. Compartir DTO o
decoder no basta.

### 5.4 Scan compatibility

`SemanticScanStrategyCompatible` y `PhysicalScanStrategyCompatible` comparan enumeracion, orden, scope, discovery,
direct/nested refs, filters y T. Diferencia material exige probe kind-specific.

### 5.5 Event/scheduler coverage

`EventClassCoverageCompatible` exige que toda clase capaz de afectar el command productivo sea un EventId ya
caracterizado o tenga evidencia kind-specific. `SchedulerCoverageCompatible` aplica la misma regla a queues, Idle,
command-context, modeless, sync context u otro scheduler. Ausencia=`FALSE/UNKNOWN`.

### 5.6 Primitive coverage

`PrimitiveSetCoverageCompatible` compara cada DBObject/API relevante. DynamicBlockReferenceProperty, Dimension, Field,
AttributeReference u otro subtipo no heredan evidencia de BlockReference salvo contrato base-class exacto aceptado.

### 5.7 Lifecycle/lock/source

`LifecycleIntegrationCompatible` compara command flags, document context, modal/modeless, sync/async, return y success
semantics. `LockingStrategyCompatible` compara timing/mode. `SourceOrderingModelCompatible` asegura que el orden S del
fixture es aplicable al read-set completo autorizado por I-49.

## 6. Formulas kind y product admission

```text
KindSpecificCoveragePass(k) iff
  DA-P7_KIND_COVERAGE(k) = PASS
  AND DA-P8_KIND_APPLICABILITY(k) is resolved and applicable evidence = PASS
  AND DA-P10_KIND_READSET(k) = PASS
  AND HostToKindCompatibilityPass(k) = PASS
  AND all required product-specific probes = PASS

CTDA_PASS(k) iff
  CTDA_HOST_PASS(HF-V27-1)
  AND KindContractClosed(k)
  AND KindSpecificCoveragePass(k)
  AND all product-kind fixtures = PASS
  AND exact host tuple compatibility = PASS
```

`CTDA_HOST_PASS` no admite kinds. `UNKNOWN` en cualquier compatibility predicate impide product PASS.

## 7. Threat composition

Se incorpora la regla y matriz HEC-V27-1. T2+T16 queda requerida y `UNKNOWN`; por tanto no puede cerrarse el fixture.
T15+T16 usa modulo controlado y schedulers managed exactos. T2+T4 usa 09N-B solo si exact headers confirman
`endCalledOnOutermostTransaction`; no se duplica una misma callback.

## 8. Tuple e invalidacion

Se conserva el tuple HF-V26-1 y se agregan: assembly/file versions y hashes managed, version/hash de headers/SDK
ObjectARX, compiler/toolset del helper, EventCatalog blob y cada probe binding. Cambiar cualquiera invalida evidencia.

## 9. Estado de producto y autoridades

```text
KindContractClosed(Selective) = FALSE
Dynamic/PushBack/Cantilever/Header/Flow Bed = OPEN / NOT ADMISSIBLE
```

I-55 `08e45e0a`/producto `991d1694`: event catalog host `NON-MATERIAL`; future HostToKind/KindContract coordination
`MATERIAL`; reconciliation `REQUIRED`; evidence transfer `NONE`. Partial placement I-55 no sustituye atomicidad I-52.

Foundation AUTH-01..13 sigue por referencia. DA-P10 productivo conserva `SymbolId + complete RootCauses(variable)` por
variable realmente leida, sin union global. AUTH-15 sigue `I-52 OWNED / NOT IMPLEMENTED`.

## 10. Disposiciones y siguiente delta

V26 queda historica; V27 supera event/probe exactness, composition, closure status y residual gates. V25 sigue normativa
para KindContracts. V24 conserva boundaries/DA-P12/T16/S-M-SM/resources. V20 sigue `PRESERVED / RESTRICTED`.

Para V28 se requiere obtener/inspeccionar el SDK ObjectARX 2025 exacto y cerrar un scheduler nativo aplicable o demostrar
con autoridad exacta que T2+T16 es OUT. Hasta entonces no procede autorizacion limitada.

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
G3 = STOPPED
G3B = NOT OPEN
```
