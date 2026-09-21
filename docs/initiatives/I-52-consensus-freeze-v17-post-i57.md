# I-52 — Consensus Freeze V17 post-I-57

**Iniciativa:** I-52 — ID16 — RACKMIRROR

**Estado de publicacion:** `PUBLISHED / PENDING EXACT-SHA RECHECK`

**PRE_FREEZE_I52_SHA:** `766841f8f9d5d29f807966d6796cd08c2363676d`

**BASE_MAIN:** `7097057cf8685bf5ecc09083cba37379d4a4aae8`

Este documento congela por referencia exacta el contrato tecnico y de producto acordado para RACKMIRROR. No
implementa el producto, no abre G3 ni O-1 y no acepta ADR-0036. El commit que publique este documento tendra una
identidad nueva; `PRE_FREEZE_I52_SHA` no es el SHA del Freeze.

## 1. Autoridades exactas

| Autoridad | Identidad congelada | Estado |
|---|---|---|
| [Proposal V17](I-52-proposal-v17.md) | SHA original acordado `b7a6d9fe897dae4d29ae29ada7f60127bca365e5`; blob `dd944d8e9c62b083eebc2b1292789977b880c368` | Consenso tecnico alcanzado; byte-for-byte estable tras los rebases |
| [ADR-0036](../adr/0036-rackmirror-espejo-semantico-por-copia.md) | blob `ce5f6fae935c7acdc347e12a7d465bb438923549` | `PROPOSED`; Owner acceptance pendiente despues de G3 si no hay contradiccion material |
| [Reconciliacion de consumo Foundation](I-52-foundation-consumption-reconciliation.md) | publicacion `766841f8f9d5d29f807966d6796cd08c2363676d`; blob `89a21b6f9dc07e72472f3e34598e98049a3e1fb7` | Coordinator `AGREED`; Architect `AGREED` |
| [Shared View Foundation R3](I-57-reconciliation-r3.md) | blob `cd42db03becff42f98b047e61c46689c17a69670` | `EFFECTIVE / UNCHANGED` |
| Recibo I-57 | tag `integration/I-57`; tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` | Target verificado como ancestro de `origin/main` |
| [Paquete RS-3](I-52-rs3-i49-final-authority-review-package.md) | blob `5d4f716614d378e6b1497b243d9acccb3886b739` | `SATISFIED / CLOSED` |
| [I-49 final freeze](I-49-consensus-freeze-v6-a1-a2-a3.md) | commit `239f47c40a6b4a9246dd4ec9e928b7fbe03f79b6`; blob `cba59b7fad9d7af66e9571d95e9ba799ae41b4d9` | Autoridad final de RS-3 / `PlanReadSet`, consumida por referencia |

El Coordinator y el Architect acordaron Proposal V17. Ambos acordaron tambien la reconciliacion de consumo de
Foundation sobre `766841f8f9d5d29f807966d6796cd08c2363676d`. No existe contradiccion material y Proposal V18 no es requerida.

## 2. Precedencia

1. Proposal V17 es el contrato tecnico y de producto base de RACKMIRROR.
2. La reconciliacion de consumo Foundation sustituye prospectivamente solo la propiedad o ubicacion provisional de
   AUTH-01..13 en V17.
3. Shared View Foundation integrada es la fuente de verdad para las autoridades neutrales AUTH-01..13.
4. El final freeze de I-49 es la fuente de verdad para RS-3 y la semantica de `PlanReadSet`.
5. ADR-0036 es el registro arquitectonico propuesto alineado con el contrato tecnico congelado; su aceptacion por el
   Owner sigue pendiente.

Ninguna reconciliacion cambia de forma implicita la politica de producto de RACKMIRROR. Ante conflicto dentro del
alcance expresamente sustituido rige la autoridad posterior de esta lista; fuera de ese alcance, Proposal V17
conserva su significado completo.

## 3. Shared View Foundation consumida por referencia

| ID | Autoridad neutral | Disposicion |
|---|---|---|
| AUTH-01 | `DimensionViewKind` | `CONSUME FROM MAIN` |
| AUTH-02 | `RackViewAddress` | `CONSUME FROM MAIN` |
| AUTH-03 | `RackViewCodec` | `CONSUME FROM MAIN` |
| AUTH-04 | `RackViewAvailability` | `CONSUME FROM MAIN` |
| AUTH-05 | `RackViewFrame` y adapters | `CONSUME FROM MAIN` |
| AUTH-06 | physical snapshot / classification | `CONSUME FROM MAIN` |
| AUTH-07 | neutral factual selection | `CONSUME FROM MAIN` |
| AUTH-08 | `Transform2D` / `RackTransformFacts` | `CONSUME FROM MAIN` |
| AUTH-09 | Resolve ports by kind | `CONSUME FROM MAIN` |
| AUTH-10 | `RackPreparedView<TPayload>` / preparation | `CONSUME FROM MAIN` |
| AUTH-11 | `RackViewBaseName` | `CONSUME FROM MAIN` |
| AUTH-12 | block requirements / query boundary | `CONSUME FROM MAIN` |
| AUTH-13 | authored comparator | `CONSUME FROM MAIN` |

I-52 no crea reemplazos neutrales locales. La semantica, limites y disposicion exactos de estas autoridades se
consumen de Foundation integrada mediante la reconciliacion congelada, sin copiar su arquitectura.

## 4. Autoridades propias de I-52

Quedan congeladas como responsabilidad de I-52:

- reflection semantica `mu_k` y reflection canonica por kind;
- matematicas del eje del espejo y de reflection de sheet/view;
- mirror read-set, exposure policy y combinaciones view/kind admitidas;
- restricciones de fuente, reglas fail-closed y transformacion semantica de la fuente;
- adquisicion del eje del usuario y naming final del espejo;
- politica `NewRackId`, semantica copy-only e identity/restamp;
- protocolo transaccional y de atomicidad;
- UX, mensajes y errores;
- CT-06 y AUTH-15;
- creacion caller-owned de definiciones;
- regeneracion y materializacion especificas del espejo;
- matriz de Owner Validation.

Los hechos neutrales de Foundation alimentan estas politicas, pero no las poseen. El resto del comportamiento
observable, persistencia y compatibilidad legacy, failure semantics, no-goals, puntos de extension, obligaciones
CT/T-M/G-M y pruebas/validacion obligatorias permanece congelado por referencia a Proposal V17 y a la reconciliacion
segun la precedencia anterior.

### AUTH-15

`AUTH-15 = I-52 OWNED / NOT IMPLEMENTED`. Foundation no lo entrego. Este Freeze fija el requisito, no una
implementacion. La creacion caller-owned de definiciones sigue siendo un gate futuro obligatorio de I-52.

## 5. RS-3 / PlanReadSet

`RS-3 = SATISFIED / CLOSED`.

Para cada variable fallida leida realmente, `PlanReadSet.Upstream` conserva por separado:

- su `SymbolId`; y
- su `RootCauses(variable)` completo.

No se admite una union global de causas. `Upstream` no incorpora una cadena representativa ni una `RecoveryUnit`.
La completitud de `RootCauses(variable)` y la precedencia interna de la autoridad corresponden al final freeze de
I-49. I-52 consume esa arquitectura por referencia y no la duplica ni la reinterpreta. Cerrar RS-3 no afirma que
RACKMIRROR haya implementado `PlanReadSet`.

## 6. Gates y resultados pendientes

Este Freeze no declara completados:

- O-1;
- la caracterizacion G3 ni los resultados de CT-49/CT-50;
- la aceptacion de ADR-0036 por el Owner;
- la implementacion de producto;
- AutoCAD Owner Validation;
- un Candidato; ni
- la integracion de I-52.

G3 valida y caracteriza este contrato congelado. Si G3 produce una contradiccion material, el Freeze queda
invalidado para el contrato afectado, Proposal V18 pasa a ser obligatoria, Coordinator y Architect deben acordar de
nuevo, se requiere un Freeze nuevo y se reevalua la disposicion de O-1/ADR cuando corresponda. Si G3 solo produce
los resultados de caracterizacion esperados, este Freeze sigue vigente.

## 7. Invalidation y enmiendas

El Freeze se invalida si cambia materialmente cualquiera de estos elementos:

- el contrato de Proposal V17;
- el objeto Shared View Foundation R3;
- la semantica consumida de AUTH-01..13;
- el target o la identidad de `integration/I-57`;
- la autoridad RS-3 / `PlanReadSet`;
- el alcance de producto de RACKMIRROR;
- la matriz kind/view soportada;
- la semantica de reflection;
- la politica de identidad;
- la atomicidad;
- el contrato AUTH-15;
- las garantias de seguridad o fail-closed; o
- las obligaciones obligatorias de pruebas y validacion.

Una actualizacion diagnostica docs-only que no cambia el significado congelado no crea silenciosamente una nueva
autoridad. Toda modificacion material exige la ruta de invalidacion correspondiente y una publicacion nueva; este
artefacto permanece como registro inmutable de lo congelado en su SHA.

## 8. Recheck exacto de publicacion

El SHA que publique este archivo debe recibir revision exacta e independiente del Coordinator y del Architect. La
revision debe verificar, como minimo:

1. `PRE_FREEZE_I52_SHA`, `BASE_MAIN`, el blob de este archivo y el blob de la nueva entrada del registro;
2. todas las identidades de la tabla de autoridades, incluido tag object y target de `integration/I-57`;
3. precedencia, limites de AUTH-01..13, propiedad de AUTH-15 y consumo por referencia de RS-3;
4. que ADR-0036 sigue `PROPOSED`, y que O-1 y G3 siguen pendientes;
5. que solo cambiaron este archivo y `docs/automation/decisions/I-52.md`; y
6. la CI de `push` sobre rama y SHA exactos con Core, UI Tests, Build UI y Build Plugin en `success`.

Hasta completar ambas revisiones, el estado es exclusivamente:

```text
CONSENSUS FREEZE = PUBLISHED / PENDING EXACT-SHA RECHECK
TECHNICAL CONSENSUS V17 = PRESERVED
FOUNDATION RECONCILIATION = COMPLETE
RS-3 = SATISFIED
R3 = EFFECTIVE
I-57 INTEGRATION = VERIFIED
O-1 = PENDING
G3 = NOT OPEN
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
