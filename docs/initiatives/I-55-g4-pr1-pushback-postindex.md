# I-55 — G4 PR-1 / Push Back physical PostIndex

Fecha: 2026-09-21

Gate: G4

`G4_START_SHA`: `08cc5dfccc49c3ddcdc15fed5192cebd8583270b`

SHA de producto: `909c4fcba0aa8669049a79e0d74c487dd2e2250d`

Proposal: V5 congelada (`38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Defecto y causa

G3 dejo PR-1 abierto y caracterizado: `RackPushBackSystemWindow` llenaba el selector lateral por cantidad de cortes y
usaba `ComboBox.SelectedIndex` como `Section`. Esa posicion visible solo coincide accidentalmente con el
`DynamicLateralCorte.PostIndex`. Cuando dos frentes en blanco eliminan una frontera fisica, los cortes conservan sus
indices originales, por ejemplo `[0, 1, 3, 4]`, mientras el selector ocupa posiciones `[0, 1, 2, 3]`.

El mismo ordinal tambien elegia el plan de vista previa. Por eso el contrato no podia limitarse a corregir el valor
devuelto: vista previa y persistencia debian resolver el mismo corte por identidad fisica.

## 2. RED verificado

Comando:

```text
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj --filter
  FullyQualifiedName~LateralInsert_PersistsTheSelectedPhysicalPostIndex --no-restore -v:minimal
```

Resultado anterior al fix:

```text
1 seleccionada / 1 FAIL esperado
postes fisicos = [0, 1, 3, 4]
tercer item visible: expected Section = 3; actual = 2
```

El baseline G3 que afirmaba literalmente el defecto se conserva historicamente en
`I-55-g3-product-characterization.md`; su prueba ejecutable se invirtio para proteger la conducta corregida.

## 3. Implementacion

- `LateralViewOption` separa la etiqueta ordinal visible de `PostIndex`.
- Las opciones se derivan directamente de `lastComputation.LateralCortes`; no existe una segunda lista de geometria.
- `SelectedView()` obtiene `Section` del item seleccionado y nunca de `SelectedIndex`.
- `PlanFor()` busca el corte por `PostIndex`, de modo que la vista mostrada y la direccion devuelta conservan la misma identidad.
- Lista vacia, item invalido y seleccion obsoleta producen `-1` o ningun plan; no inventan un poste ni caen al ordinal.
- La primera carga conserva el default vigente: primer corte fisico disponible. Orden y etiquetas visibles se mantienen.

No se modificaron builders, Foundation, codec, `RackViewAddress`, esquema de persistencia, geometria, nombres, `RackId`,
Selectivo, Dinamico, Cantilever, Cabecera, Cama, I-52 ni AUTH-15.

## 4. Cobertura del contrato

La regresion conductual usa cortes reales `[0, 1, 3, 4]`. El mapping puro añade el sentinel exacto `[0, 2, 5]` y
comprueba primer, intermedio y ultimo item; tambien cubre `[0, 1, 2]`, lista vacia, item invalido y seleccion obsoleta.

La frontera de G4 termina en el `Section` del `PushBackInsertionRequest`. El sobre DWG y su lectura posterior usan el
codec y el esquema existentes; G4 no los modifica. Su impacto queda cubierto por `SharedViewCodecTests` y por las suites
completas. Un round-trip DWG focal nuevo requeriria AutoCAD y duplicaria esa autoridad; el escenario manual del Candidato
comprobara dibujar, guardar y reabrir.

## 5. Evidencia GREEN sobre el SHA de producto

| Evidencia | Resultado |
|---|---|
| Focal UI lateral | 3/3 PASS |
| G3 characterization relevante | 31/31 PASS |
| Foundation codec/availability | 4/4 PASS |
| Push Back Core impact | 2013/2013 PASS |
| Push Back UI impact | 481 PASS / 1 historical skipped |
| Core Full local | 8267/8267 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35662635382` / 4/4 SUCCESS / `head_sha=909c4fcba0aa8669049a79e0d74c487dd2e2250d` |

Una primera tentativa de ejecutar Core Full y UI Full en paralelo encontro `CS2012` por escritura concurrente al mismo
`obj`; no ejecuto una suite completa y no se usa como evidencia. UI termino verde y Core se repitio secuencialmente verde.

## 6. Owner Validation y estado

El cambio afecta que lateral se dibuja cuando hay postes no contiguos. Workflow V1 exige validacion manual de cambios de
dibujo sobre el Candidato final, no en cada gate funcional. Se registra `DEFERRED TO CANDIDATE` con escenario OV-G4:

1. abrir Push Back con postes fisicos no contiguos;
2. elegir la lateral de un poste intermedio, dibujar y comprobar que corresponde al poste elegido;
3. guardar y reabrir, y comprobar que la lateral permanece estable;
4. comprobar BOM y RACKLISTA sin regresion.

```text
PR-1 = RESOLVED on product SHA
PR-2 = OPEN / CHARACTERIZED / UNCHANGED
Foundation diff = NONE
Schema diff = NONE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G4 = COMPLETE
G5 = OPEN
```
