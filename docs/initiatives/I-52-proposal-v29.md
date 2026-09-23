# I-52 — Proposal V29: AcDbObjectReactor y normalizacion de probes nativos

> **DESIGN / AUTHORITY CLOSURE ONLY.** No ejecuta CT-DA, no compila helper, no implementa RACKMIRROR ni AUTH-15.

## 1. Status e identidades

| Campo | Valor |
|---|---|
| Starting I-52 / Proposal V28 | `46a2e15f529d30d65f9524f0ade98d72da980237` |
| Proposal V28 blob | `641a2b783f59a8a68c5d764f6af9f15e663ddd84` |
| OA-V28-1 blob | `1f62ebabdd2598637f2a7f89e5066b763511c242` |
| NEC-V28-1 blob | `e2d1284c5266f5b7f2dd57129ab7552bbd881c74` |
| Architect package V28 blob | `24a5996c7ae39291e63882306ae46f598a193730` |
| Decisions V28 blob | `6b415958eafedfaeca934a0466df87ed11e932e1` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d` |
| I-57 object / target | `a5bc02210f740839e2fac37e63fc00512e5ccee6` / `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Native event catalog | `NEC-V29-1` |
| Native probe matrix | `NPM-V29-1` |
| Host fixture revision | `HF-V29-1` |

Preflight: branch/upstream/I-52 coinciden con Starting I-52, el arbol esta limpio, stash vacio y no hay merge, rebase,
cherry-pick, revert ni bisect incompleto. I-55 se resuelve en `origin/feature/creacion-de-vistas`.

## 2. Findings Architect V28

```text
Architect =
CHANGES REQUIRED — PROPOSAL V29

BLOCKER-1 =
AcDbObjectReactor is omitted from T2/native host coverage

BLOCKER-2 =
Native probes counted for closure do not all satisfy the normative schema;
C15N16N-SM lacks complete authority/oracles/cleanup

V18 =
GOVERNING
```

## 3. Delta V29

V29 incorpora `AcDbObjectReactor`, la especializacion `AcDbEntityReactor` y los lock callbacks de
`AcApDocManagerReactor`; publica un censo HF-bounded de familias reactoras y normaliza cada ProbeId nativo en
NPM-V29-1. No cambia HEC-V27-C1, los residual gates V27, KindContract V25, lifecycle V24 ni arquitectura V20.

## 4. Autoridad `AcDbObjectReactor`

Autodesk 2025 declara `AcDbObjectReactor : AcRxObject` en `dbmain.h`. Un transient reactor se registra en un objeto
concreto mediante `AcDbObject::addReactor(AcDbObjectReactor*) const` y se quita mediante `removeReactor` usando la
misma instancia. El notifier puede estar abierto read o write para registrar/quitar. El helper retiene ownership del
reactor; `goodbye` impide intentar removerlo de un objeto ya destruido.

Callbacks IN para HF:

- `openedForModify(const AcDbObject*)`: antes del cambio, en `assertWriteEnabled`;
- `modified(const AcDbObject*)`: al close/cancel despues de una modificacion;
- `erased(const AcDbObject*, bool)`: al close, despues de erase/unerase;
- `cancelled(const AcDbObject*)` y `modifyUndone(const AcDbObject*)`: cancel/undo del open modificado;
- `objectClosed(const AcDbObjectId)`: close/cancel; reabrir el mismo notifier write no esta soportado;
- `AcDbEntityReactor::modifiedGraphics(const AcDbEntity*)`: close de una entidad cuyo graphics state puede haber
  cambiado.

`goodbye` es IN sólo como cleanup/lifetime marker: ocurre por destruccion del objeto/base y no es mutator credit.
`modifiedXData`, `subObjModified`, `copied`, `unappended` y `reappended` son OUT del fixture actual: HF no escribe
XData, no usa los subobjetos enumerados por Autodesk, no clona y no ejecuta UNDO/REDO. Un cambio de fixture reabre el
censo.

## 5. Registro y lifetime exactos

| ObjectReactorId | Target | Registro | Removal | Lifetime/filter |
|---|---|---|---|---|
| OR-XR | F-XR | open read + `addReactor(OR-XR)` | `removeReactor(OR-XR)` antes de borrar fixture | retained helper instance; exact ObjectId |
| OR-REF-A | F-REF-A | igual | igual | exact BlockReference A |
| OR-REF-B | F-REF-B | igual | igual | exact BlockReference B |
| OR-NEST-A | F-NEST-A | igual | igual | exact nested reference A |
| OR-NEST-B | F-NEST-B | igual | igual | exact nested reference B |
| ER-REF-A | F-REF-A | `addReactor(ER-REF-A)` | same instance removal | `AcDbEntityReactor`; graphics callback only |

Cada callback rechaza cualquier ObjectId/base/document distinto. Registration/removal failure produce UNKNOWN para
todos los probes dependientes. `goodbye` marca la instancia detached and dead; cleanup no vuelve a tocar el notifier.

## 6. T2 y censo de clases

```text
T2NativeClasses(HF) = {
  AcDbDatabaseReactor,
  AcDbObjectReactor,
  AcDbEntityReactor,
  AcTransactionReactor,
  AcEditorReactor,
  AcApDocManagerReactor
}
```

`AcRxDLinkerReactor` queda en T15 como marker de load, no como mutator T2. El censo normativo de NEC-V29-1 clasifica
las demas familias oficiales por las operaciones reales del scratch fixture. No se pretende inventariar plugins.

```text
NativeReactorClassSetClosed(HF) iff
  every reactor family reachable from the fixture object/transaction/command/lock/load paths is IN or OUT
  AND every IN mutator class has exact 2025 authority and a complete NPM probe
  AND every OUT row has an exact primitive/operation reason
  AND no UNKNOWN capable of changing HF truth remains.
```

Result: `NativeReactorClassSetClosed(HF-V29-1)=TRUE` for definition scope.

## 7. Object-reactor probes y composiciones

NPM-V29-1 define `02NO-S`, `02NO-M`, `02NO-SM`, `02NO-CLOSE-SM`, `04NO-S`, `04NO-UNDO-S` y `02NE-M`.
`C2OBJ16N-S/M/SM`, `C2ENT16N-M`, `C2DOCW16N-SM` y `C2DOCC16N-SM` prueban las composiciones diferidas relevantes.

No se crean 10NO: object/entity notifications ocurren dentro de la modificacion/close que las origina y no aportan
por autoridad una fase nueva despues de R/M. El riesgo final-window aparece cuando esas callbacks llaman NS-SEND; por
eso la obligacion correcta es C2OBJ/C2ENT + T16. T2-OBJ+T4 no necesita probe adicional: cancel/undo queda en 04NO y
end/abort de la T se mide independientemente en 04N/09N; no existe callback object-reactor de transaction end.

## 8. Normalizacion completa

NPM-V29-1 contiene 29 ProbeIds concretos. Cada fila llena los 24 campos del schema V28, tiene exactamente un
`PrimaryAuthorityId` y separa `ScheduleOriginEventId`, load/setup y lifecycle markers. No hay `09N`, `10N*`, `16N*`
ni familia wildcard normativa. Los estados y acciones compartidos se resuelven por ContractId inmutable en el mismo
artefacto; ninguna celda depende de narrativa no normativa.

`C15N16N-SM` usa `PrimaryAuthorityId=NS-SEND`, `ScheduleOriginEventId=N-DB-MOD`; `NL-ARX-PATH`, `N-RX-LOADED` y el
registro del payload son setup/markers. Su expected state deriva de `STATE-SM-0/1`, no del writer.

## 9. Closure

```text
NativeProbeCatalogClosed(NEC-V29-1,NPM-V29-1) iff
  every required native reactor class for HF is cataloged
  AND NativeReactorClassSetClosed(HF)=TRUE
  AND every concrete ProbeId has all 24 schema fields
  AND every ProbeId has exactly one primary authority
  AND inverse event/scheduler mappings equal NPM projections
  AND schedulers and material compositions are exact
  AND no wildcard, placeholder or definition UNKNOWN remains.
```

Definition result:

```text
NativeReactorClassSetClosed(HF-V29-1) = TRUE
NativeProbeCatalogClosed = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V29-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

Closed define el instrumento; no afirma que una mutacion sea legal, que un probe pase ni que el host sea admisible.
FAIL/UNKNOWN futuro mantiene `CTDA_HOST_PASS=false`.

## 10. Host→kind y producto

HF-V27 permanece normativo. `EventClassCoverageCompatible`, `PrimitiveSetCoverageCompatible` y
`LifecycleIntegrationCompatible` comparan cada kind con HEC-V27-C1 + NEC-V29-1. Un subtype, persistent reactor,
event o scheduler productivo ausente exige kind evidence. DA-P7/8/10 conservan sus residual gates.

```text
KindContractClosed(Selective) = FALSE
Dynamic = OPEN / NOT ADMISSIBLE
PushBack = OPEN / NOT ADMISSIBLE
Cantilever = OPEN / NOT ADMISSIBLE
Header = OPEN / NOT ADMISSIBLE
Flow Bed = OPEN / NOT ADMISSIBLE
```

## 11. Tuple e invalidacion

El tuple futuro agrega blobs de NEC-V29-1/NPM-V29-1/HF-V29-1, manifest de clases, ObjectReactorIds, ProbeIds y
composition manifest. Header hashes siguen `NOT OBSERVED` hasta adquisicion autorizada del SDK.

Invalidan: firma/callback `AcDbObjectReactor` o `AcDbEntityReactor`, registration model, lock-reactor signature,
class census, mapping Event↔Probe, primary authority/oracle/cleanup de C15, schema NPM, scheduler, helper/harness,
fixture, host tuple o header descargado divergente.

## 12. I-55 y autoridades preservadas

I-55 sigue en `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`: native host contract `NON-MATERIAL`, HostToKind y
KindContract `MATERIAL`, reconciliation `REQUIRED`, evidence transfer `NONE`. Su partial placement no sustituye
I-52 one-T/one-commit/all-or-nothing.

Foundation AUTH-01..13 sigue por referencia. I-49 conserva `SymbolId + complete RootCauses(variable)` por variable
realmente leida. AUTH-15 permanece `I-52 OWNED / NOT IMPLEMENTED`.

## 13. Disposiciones

- V28 queda historica; V29 supera sólo class-set completeness, object/entity/doc reactor coverage, probe schema,
  composiciones afectadas y closure status.
- HEC-V27-C1 sigue normativa para managed y HF-V27 para HostToKind.
- V25 sigue normativa para product KindContracts.
- V24 conserva C points, DA-P12, T16, S/M/SM y resource rules.
- V20 permanece `PRESERVED / RESTRICTED`.
- V18, Freeze V18 y O-1 V18 gobiernan; ADR-0036 sigue `PROPOSED / AMENDED FOR V18`.

## 14. Proceso y G3

El siguiente gate despues de reviews exactas y consenso V29 es **LIMITED CT-DA RESEARCH AUTHORIZATION**. Sólo ese
gate puede permitir aceptar la licencia/obtener SDK, verificar headers/libs, construir helper/harness y ejecutar
probes. No autoriza producto.

```text
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
```

## 15. Self-critique

1. AcDbObjectReactor enumerado o excluido: **YES, IN**.
2. Object reactor muta target sin T2 coverage: **NO**.
3. Cada ProbeId satisface schema: **YES, NPM-V29-1**.
4. C15 es row completa: **YES**.
5. C15 tiene una primary authority: **YES, NS-SEND**.
6. Event/origin/markers separados: **YES**.
7. Wildcard satisface closure: **NO**.
8. Catalog closes con relevant class UNKNOWN: **NO**.
9. Definition closure implica runtime PASS: **NO**.
10. V29 autoriza CT-DA: **NO**.

## 16. Review checklist

- [ ] refutar el class census contra las operaciones exactas HF;
- [ ] verificar autoridad 2025 de object/entity/doc reactors y registration;
- [ ] verificar 31 filas NPM, 24 campos y una primary authority;
- [ ] reconstruir mappings inversos desde NPM;
- [ ] intentar abrir C15 con missing setup/oracle/cleanup;
- [ ] verificar T2+T4/T16 y ausencia de inference a plugins/producto;
- [ ] confirmar closure ≠ execution y todos los estados de gobierno.

```text
PROPOSAL V29 = PUBLISHED / REVIEW REQUIRED
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
