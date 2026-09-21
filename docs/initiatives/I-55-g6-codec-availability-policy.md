# I-55 — G6 Product policies over codec + availability

Fecha: 2026-09-21

Gate: G6

`G6_START_SHA`: `ce06d073a5a1b4bca38f53b0cfc4977cc6d15a3b`

SHA de producto: `2a630568146c31d6fe18a1503faf3917d52a4cca`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-52 observado: `faaf709bf6401dcda91b07f41afe44f490fb916a` (avance solo documental, no material para G6)

Proposal: V5 congelada (`38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Frontera entregada

G6 agrega una capa pura de producto en `RackCad.Application.Views.Policy`. Consume directamente
`RackViewAddress`, `RackViewCodec` y `RackViewAvailability`; no crea codec, parser, disposition ni hechos fisicos
paralelos. El orden ejecutable es:

```text
decode/address -> disposition policy -> availability -> exposure policy
```

No hay consumidores nuevos. ID17, ID18 e ID19 siguen sin implementarse; G6 deja lista la decision reutilizable para
esos flujos. RACKEDITAR, readers legacy, persistencia, UI, Plugin, dibujo y AUTH-15 permanecen intactos.

## 2. Resultado tipado y disposition

`ProjectionPolicyDecision` devuelve `Accept`, `AcceptCanonicalized` o `Reject`, con direccion y sintaxis canonicas
solo cuando la decision acepta. Los rechazos no llevan una direccion utilizable. El valor por defecto del resultado
falla cerrado.

| Disposition de Foundation | Decision I-55 | Resultado |
|---|---|---|
| `Canonical` | `Accept` | conserva la direccion y entrega encoding canonico de Foundation |
| `Canonicalizable` | `AcceptCanonicalized` | conserva la semantica y entrega encoding canonico |
| `Coerced` | `Reject` | `CoercedLegacyFormNotAllowed` / `UpdateRack` |
| `Invalid` | `Reject` | `InvalidSyntax` o `UnknownKind`, sin direccion |

Un intent nuevo entra como `RackViewAddress` tipada y solo continua si `RackViewCodec.Encode` puede producir su
sintaxis canonica. Ninguna forma legacy o coerced se escribe desde esta capa.

## 3. Availability y exposure

Availability sigue siendo un hecho fisico de Foundation. La matriz de exposure decide despues si la operacion de
producto publica esa direccion.

| Sistema | Direcciones expuestas | CreateFirst | InsertSibling | Batch/Queue | GroupProjection |
|---|---|---:|---:|---:|---:|
| Selective | `Fondo`, `Post`, `Planta/Whole` | si | si | si | si |
| Dynamic | `FlowEnd`, `Post`, `Planta/Whole` | si | si | si | si |
| Push Back | `PushBackCut`, `Post`, `Planta/Whole` | si | si | si | si |
| Cantilever | `Frontal/Whole`, `Station`, `Planta/Whole` | si | si | si | si |
| Header | `Lateral/Whole`, `Planta/Whole` | si | si | si | si |
| Flow Bed | `Lateral/Whole` | si | no | no | no |

Cantilever conserva `Station(index)` como variante tipada. Push Back consume el `Post(index)` fisico corregido en G4;
no remapea un ordinal de UI. Flow Bed declara expresamente su unica primera vista y rechaza hermanas, cola y grupo.

## 4. Razones y remedios

Reason codes:

```text
InvalidSyntax
CoercedLegacyFormNotAllowed
UnavailableForSystem
NotExposedForOperation
UnsupportedVariant
UnknownKind
```

Remedy categories:

```text
UpdateRack
ChooseAnotherView
RepairLegacyRepresentation
Unsupported
```

Son datos puros. G6 no formatea mensajes, abre dialogos, pide puntos ni ejecuta una accion de UX.

## 5. RED -> GREEN

Los tests se escribieron primero. Para obtener una seleccion positiva se agregaron shells de API sin conducta y se
ejecuto el filtro focal completo:

```text
RED: 22 seleccionadas / 20 FAIL esperados / 2 PASS de guardas
```

Despues de la implementacion minima y de agregar la guarda de resultado default fail-closed:

```text
GREEN focal: 24/24 PASS
```

El RED demostro decisiones incorrectas de codec, disponibilidad y exposure. No se obtuvo mediante un filtro vacio.

## 6. Archivos

Producto:

```text
src/RackCad.Application/Views/Policy/RackViewExposure.cs
src/RackCad.Application/Views/Policy/ProjectionCodecPolicy.cs
src/RackCad.Application/Views/Policy/ProjectionAvailabilityPolicy.cs
```

Pruebas:

```text
tests/RackCad.Tests/RackViewExposureTests.cs
tests/RackCad.Tests/ProjectionCodecPolicyTests.cs
tests/RackCad.Tests/ProjectionAvailabilityPolicyTests.cs
tests/RackCad.Tests/ProductPolicyIndependenceGuardTests.cs
```

Foundation diff = `NONE`. Schema diff = `NONE`. Domain, UI, Plugin, assets, eng y deploy no cambiaron.

## 7. Evidencia sobre el SHA de producto

| Evidencia | Resultado |
|---|---|
| RED focal | 22 seleccionadas / 20 FAIL esperados / 2 PASS |
| GREEN focal policy | 24/24 PASS |
| G3 characterization | 31/31 PASS |
| Foundation codec/availability impact | 11/11 PASS |
| System impact | 7 exposure sentinels + 9 availability/policy sentinels dentro del focal verde |
| Core Full local | 8292/8292 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35669205439` / 4/4 SUCCESS / `head_sha=2a630568146c31d6fe18a1503faf3917d52a4cca` |

No se agregaron skips. Cada filtro selecciono al menos una prueba.

## 8. Cierre

El cambio no altera UX ni dibujo observable y todavia no tiene consumers. Owner Validation = `NOT REQUIRED FOR THIS
GATE`; la validacion del Candidato conserva las obligaciones de los gates que si cambian producto visible.

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
Foundation diff = NONE
Schema diff = NONE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G6 = COMPLETE
G7 = OPEN
```
