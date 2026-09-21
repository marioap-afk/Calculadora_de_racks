# I-55 — G5 PR-2 / Cantilever PlantaVisibility

Fecha: 2026-09-21

Gate: G5

`G5_START_SHA`: `023020f8228a84d7fc059cd08c7a38d49a117b6a`

SHA de producto: `7cf3b170be319bbc76efe68fddff8c56fd82cb75`

Proposal: V5 congelada (`38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Defecto y causa

G3 dejo PR-2 abierto y caracterizado. `CantileverViewPlanBuilder.Build` ya aceptaba
`CantileverPlantaVisibilityDesign`, y `CantileverLineDesign.PlantaVisibility` ya era la autoridad persistida, pero
los dos callers Plugin omitian el ultimo argumento. El builder recibia `null` y aplicaba el default historico; una
Planta materializada podia por ello ignorar la seleccion que el usuario habia guardado.

Los dos caminos afectados estaban en `src/RackCad.Plugin/RackCantileverCommands.cs`:

1. `DrawCantileverView`, flujo de insercion nueva: tenia `design` y la linea resuelta, pero llamaba al builder solo
   con linea, kind, factory y section.
2. `EditCantilever`, flujo `RACKEDITAR` de redibujo multivista: tenia el `design` producido por la ventana y la linea
   resuelta, pero tambien omitia la visibilidad al reconstruir cada plan.

## 2. RED verificado

Comando:

```text
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter
  "FullyQualifiedName~I55CantileverPlantaVisibilityPropagationTests|
   FullyQualifiedName~I55DeferredProductFixCharacterizationTests.Pr2_CantileverPluginForwardsPlantaVisibilityAtBothPlanCalls"
  --no-restore --logger "console;verbosity=minimal"
```

Resultado anterior al fix:

```text
2 seleccionadas / 2 FAIL esperados
caller uses of design.PlantaVisibility: expected 2; actual 0
```

El baseline G3 que afirmaba literalmente la omision se conserva en el receipt historico
`I-55-g3-product-characterization.md`; su prueba ejecutable se invirtio para proteger la conducta corregida.

## 3. Implementacion

Cada caller entrega directamente `design.PlantaVisibility` al builder existente. No se agrego una copia, inferencia,
fallback ni fuente paralela. El builder, su firma, Domain, Application, Foundation, persistencia y materializador no
cambiaron.

```text
resolved design -> design.PlantaVisibility -> CantileverViewPlanBuilder.Build -> plan -> materializer
```

## 4. Cobertura del contrato

El sentinel nuevo protege simultaneamente los dos call sites y el resultado puro del plan. Cubre:

- default historico: Planta sin brazos ni tensores;
- `null`, permitido por la firma vigente: mismo conjunto de tipos que el default;
- solo brazos: brazos presentes y tensores ausentes;
- solo tensores: tensores presentes y brazos ausentes;
- round-trip de la seleccion solo-brazos por `RackProjectStore` antes de resolver y proyectar.

Frontal y Lateral conservan la semantica caracterizada; las pruebas Cantilever existentes comprueban que esos kinds no
consumen accidentalmente la politica de Planta. BOM, RACKLISTA, nombres, `RackId`, placement, update, insert y payload
mantienen sus authorities vigentes.

## 5. Archivos de producto y prueba

```text
src/RackCad.Plugin/RackCantileverCommands.cs
tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs
tests/RackCad.Tests/I55CantileverPlantaVisibilityPropagationTests.cs
```

No hay cambios en `RackCad.Domain`, `RackCad.Application`, `RackCad.UI`, assets, schema, Foundation, Push Back,
Selective, Dynamic, Cabecera, Cama, I-52 o AUTH-15.

## 6. Evidencia GREEN sobre el SHA de producto

| Evidencia | Resultado |
|---|---|
| Focal PR-2 | 2/2 PASS |
| G3 characterization relevante | 31/31 PASS |
| I-55 por nombre | 4/4 PASS |
| Cantilever impact | 874/874 PASS |
| Core Full local | 8268/8268 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35666081923` / 4/4 SUCCESS / `head_sha=7cf3b170be319bbc76efe68fddff8c56fd82cb75` |

No se agregaron skips. Cada filtro selecciono al menos una prueba.

## 7. Owner Validation y estado

El cambio afecta geometria visible de la Planta Cantilever cuando la configuracion no es default. Workflow V1 exige
validacion manual sobre el Candidato final, no en cada gate funcional. Se registra `DEFERRED TO CANDIDATE` con escenario
OV-G5:

1. configurar un Cantilever con una seleccion `PlantaVisibility` no default;
2. dibujar Planta y comprobar que aparecen u ocultan exactamente brazos y tensores segun la seleccion;
3. ejecutar Update y comprobar que la Planta conserva la misma seleccion;
4. guardar y reabrir, y comprobar el mismo resultado;
5. comprobar BOM y RACKLISTA sin regresion.

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED on product SHA
Foundation diff = NONE
Schema diff = NONE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G5 = COMPLETE
G6 = OPEN
```
