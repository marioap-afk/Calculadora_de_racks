# I-55 — G7 Product Prepare / View Intent Composition

Fecha: 2026-09-21

Gate: G7

`G7_START_SHA`: `4d8f9d3a441958d68e191e737003deb073097988`

SHA de producto: `bf150d0ab3b9859fac45d5650e83b8f271739724`

Commit de implementacion: `a3d3d2657522421d70f715b7893aa94ba1975f35`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-52 observado: `faaf709bf6401dcda91b07f41afe44f490fb916a` (sin cruce material con G7)

Proposal: V5 gobernante y congelada (blob `38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Frontera entregada

G7 agrega una composicion pura en `RackCad.Application.Views.Preparation`. Distingue explicitamente intenciones de
rack nuevo y rack existente, acepta la intencion mediante las policies de G6 y produce un
`RackPreparedProductView<TPayload>` que compone el resultado tipado de Foundation con identidad y metadatos de
producto. No materializa AutoCAD.

El orden ejecutable queda protegido por pruebas:

```text
validar operacion
-> G6 policy / canonical address
-> aceptar identidad y authored gate cuando corresponde
-> AUTH-09 Resolve (una vez)
-> AUTH-10 Prepare (una vez)
-> envelope puro
```

`RackPreparedView<TPayload>` conserva `Address`, `Frame`, `BaseName`, `BlockRequirements` y `Payload`. G7 no llama
builders por fuera de `IRackViewPreparationPort`, no crea BaseName ni requirements paralelos y no consulta ni importa
bloques.

## 2. Contratos de producto

Los contratos principales son:

```text
RackProductViewRequest
NewRackCreationContext<TAuthored>
ExistingRackContext<TComparisonInput>
NewRackViewIntent<TAuthored>
ExistingRackViewIntent<TComparisonInput>
AcceptedNewRackViewIntent<TAuthored>
AcceptedExistingRackViewIntent<TComparisonInput>
RackProductIntentAcceptanceResult<TIntent>
RackPreparedProductView<TPayload>
RackProductPrepareResult<TPayload>
RackProductPreparer<TAuthored, TResolved, TPayload>
```

`RackProductSourceKind` discrimina `NewRack` de `ExistingRack`; no hay `null` ni booleano ambiguo para escoger el
flujo. El payload permanece generico y tipado de extremo a extremo. Las guardas de contrato rechazan `object`,
`dynamic`, JSON, reflection y DTO universal de geometria como escape.

La taxonomia estructurada de fallo es:

```text
UnsupportedIntent
PolicyRejected
InvalidIdentity
AuthoredDivergent
AuthoredUnreadable
ResolveFailed
PrepareFailed
EnvelopeFailed
```

`CauseCode` y `Diagnostic` preservan la causa de policy, comparator, resolve, prepare o composicion. No son strings de
UI ni deciden UX.

## 3. Lifecycle de RackId

`IRackIdFactory` es la unica seam de generacion; `GuidRackIdFactory` concentra el unico `Guid.NewGuid` de G7.

- Una intencion rechazada no llama la factory.
- `RackProductIntentAcceptance.Accept` acuña la identidad al aceptar el contexto nuevo.
- `NewRackCreationContext<TAuthored>` conserva esa identidad; N vistas aceptadas del mismo contexto la comparten.
- Repetir `Prepare` sobre una intencion ya aceptada no genera otra identidad y produce un resultado semanticamente
  equivalente.
- El flujo existente toma `SourceEnvelope.Id`, valida kind/identidad y nunca llama una factory.

Prepare no genera RackId. Materialization futura tampoco recibe esa responsabilidad.

## 4. Authored y envelope

Un rack nuevo usa el authored de su intencion aceptada y no invoca `IRackAuthoredComparatorPort`. Un rack existente
ejecuta el comparator inyectado antes de Resolve:

| Resultado authored | Conducta G7 |
|---|---|
| `Single` | continua con la autoridad devuelta |
| `Divergent` | `AuthoredDivergent`; cero Resolve |
| `Unreadable` | `AuthoredUnreadable`; cero Resolve |

Selective reutiliza su comparator de Foundation. Los kinds sin comparator demostrado permanecen fail-closed; G7 no
elige primera hermana, mayoria ni fallback.

`RackViewEnvelopeComposition` canoniza View/Section con `RackViewCodec.Encode` y usa la autoridad unica
`RackEmbedComposer.Compose`. Para existing preserva `CustomProperties`, `ExtensionData` y metadata del sobre fuente;
para new no inventa hermanas. La nueva llamada pura a `Compose(source)` se agrego explicitamente al censo T-GRD-02;
una llamada adicional sigue haciendo fallar la guarda.

## 5. RED -> GREEN

Los tests se escribieron primero. La primera seleccion focal produjo:

```text
RED: 19 seleccionadas / 18 FAIL esperados / 1 PASS de guarda
```

El verde final agrega idempotencia, preservacion de metadata, fail-closed no Selective y los seis sistemas:

```text
GREEN focal: 23/23 PASS
```

El primer Core Full sobre la implementacion descubrio tres fallos esperados en las guardas historicas de sobres: el
censo aun exigia siete llamadas y su lector no clasificaba metodos genericos. La correccion de prueba
`bf150d0ab3b9859fac45d5650e83b8f271739724` registra la octava llamada autorizada, clasifica
`RackViewEnvelopeComposition.Compose<TPayload>(source, ...)` y conserva el fallo cerrado ante una novena llamada.
La seleccion de sobre quedo 23/23 y el Core Full posterior quedo verde.

## 6. Sentinels de sistemas

| Sistema | Sentinel preparado |
|---|---|
| Selective | PASS |
| Dynamic | PASS |
| Push Back | PASS |
| Cantilever | PASS |
| Header | PASS |
| Flow Bed | PASS |

Cada sentinel atraviesa policy, Resolve y Prepare con payload tipado. No afirma que todas las operaciones esten
expuestas para todos los sistemas; usa la exposure vigente de G6.

## 7. Archivos

Producto:

```text
src/RackCad.Application/Views/Preparation/RackProductIntentAcceptance.cs
src/RackCad.Application/Views/Preparation/RackProductPrepare.cs
src/RackCad.Application/Views/Preparation/RackProductViewIntent.cs
src/RackCad.Application/Views/Preparation/RackViewEnvelopeComposition.cs
```

Pruebas nuevas:

```text
tests/RackCad.Tests/NoProductPlannerGuardTests.cs
tests/RackCad.Tests/RackIdLifecycleTests.cs
tests/RackCad.Tests/RackProductAuthoredGateTests.cs
tests/RackCad.Tests/RackProductPrepareTestSupport.cs
tests/RackCad.Tests/RackProductPrepareTests.cs
tests/RackCad.Tests/RackViewPreparationSystemTests.cs
```

Guardas reapuntadas:

```text
tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs
tests/RackCad.Tests/CustomPropertiesEnvelopeTests.cs
tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs
```

Foundation product diff = `NONE`. Schema diff = `NONE`. Domain, UI, Plugin, assets, eng y deploy no cambiaron.

## 8. Evidencia sobre el SHA de producto

| Evidencia | Resultado |
|---|---|
| RED focal | 19 seleccionadas / 18 FAIL esperados / 1 PASS |
| GREEN focal G7 | 23/23 PASS |
| Censo/guardas de envelope | 23/23 PASS |
| G6 policy impact | 24/24 PASS |
| Foundation Resolve/Prepare/BaseName/requirements/comparator | 38/38 PASS |
| G3-G5 characterization Core | 32/32 PASS |
| G4 UI focal | 2/2 PASS |
| Core Full local | 8315/8315 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo los dos MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35673463598` / 4/4 SUCCESS / `head_sha=bf150d0ab3b9859fac45d5650e83b8f271739724` |

No se agregaron skips. Cada filtro selecciono al menos una prueba.

## 9. Limites y cierre

G7 no crea `Database`, `Transaction`, `ObjectId`, `BlockTableRecord`, `BlockReference`, jig, nombre unico, queue,
materializacion, AUTH-15, ID17 completo, ID18 UI ni ID19. No cambia persistencia, dibujo ni UX; por ello Owner
Validation = `NOT REQUIRED FOR THIS GATE`.

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
Foundation diff = NONE
Schema diff = NONE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G7 = COMPLETE
G8 = OPEN
```
