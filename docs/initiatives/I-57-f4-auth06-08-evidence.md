# I-57 F4 — Evidencia AUTH-06..08

```text
Gate                  = F4 / AUTH-06..08 ONLY
F4_START_SHA          = 23e35cd6b990e8cc6c87440a534ba7907fe9f757
F4_IMPLEMENTATION_SHA = 7994b8038f0f28d824677e74591a5a6114e426c5
Proposal              = V5 @ 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
R3                     = EFFECTIVE / UNCHANGED
ADR-0044               = ACCEPTED / UNCHANGED
F4 result              = COMPLETE
F5                     = NOT OPEN
```

## AUTH-06 — snapshot y clasificacion fisica

El probe de `RACKDUPLICAR` permanece en Plugin y proyecta exclusivamente values puros hacia Application.
`RackPhysicalReferenceSnapshot` conserva identity descriptiva estable, definition identity, Model/Paper/Reference
Space, xref, origen fisico y presencia de block reference. `RackPhysicalDefinitionSnapshot` conserva payload crudo,
nombre de definicion y conteo de referencias directas. Ningun contrato publico de AUTH-06 expone `ObjectId`,
`Database`, `Transaction`, `DBObject`, `BlockReference` ni otro tipo de AutoCAD.

`RackPhysicalSelection` clasifica de forma fail-closed y mantiene separados `NotBlock`, `WrongSpace`, `Xref`,
`MissingDefinition`, `MissingRackData`, `Unreadable`, `Foreign` y `UnknownKind`. Para envelopes legibles conserva
RackId presente/ausente, kind, nombre, design, sintaxis View/Section y address neutral decodificada. Un RackId ausente
permanece ausente: no se sintetiza identidad para volver seleccionable o agrupable un miembro.

## AUTH-07 — seleccion factual y paridad COPY

El nucleo neutral deduplica por physical key, conserva el primer orden observado, expone miembros clasificados y
seleccionados y forma grupos por RackId solo cuando ese dato existe. Tambien reporta el numero factual de referencias
fisicas duplicadas. No contiene `NewRackId`, conteo de copias, nombres generados, restamp ni intencion COPY, mirror o
projection.

`RackDuplicationPlan` delega clasificacion, deduplicacion y agrupacion factual a ese nucleo. Conserva dentro del
consumer su fallback legacy por definicion para miembros sin RackId, comparacion authored, errores/notices, identidad
de destino, nombres, numero de copias, restamp y grupos COPY. La delegacion transporta el mismo envelope ya
deserializado; no crea una segunda autoridad de lectura ni altera el resultado observable del plan.

## AUTH-08 — hechos de transformacion

`RackTransformFacts.Describe` recibe el `Transform2D` existente y una tolerancia suministrada por el caller. Para una
descomposicion soportada devuelve translation, rotation en radianes, determinant, `IsReflection` y uniform scale.
Las matrices degeneradas, no uniformes o no ortogonales/shear fallan de forma tipada; una tolerancia negativa o no
finita tambien produce un resultado tipado. Foundation no decide si un mirror se acepta, que eje representa, ni
politicas Rigid/Orthographic, Relative Frame Window, anchor o posicion objetivo. `Transform2D` no fue modificado.

## RED → GREEN y caracterizaciones

La primera corrida focal fallo al compilar porque los contratos AUTH-06..08 aun no existian. Tras la implementacion,
la seleccion focal y de sistemas ejecuto 122 pruebas y paso 122. Incluyo los nuevos casos de snapshot, categorias
fail-closed, identidad ausente, orden/dedupe/grouping, ausencia de policy y descomposicion de transformaciones, junto
con `RackDuplicationPlanTests`, CT-16, CT-SCAN y CT-GEO.

Una corrida Core Full intermedia activo dos guardias existentes contra reconstruir el envelope de persistencia. La
implementacion se corrigio para transportar el envelope ya deserializado; las guardias focales pasaron 56/56 y la
evidencia final se produjo sobre el SHA enmendado `7994b8038f0f28d824677e74591a5a6114e426c5`.

| Oraculo | Resultado final |
|---|---|
| CT-16 / seleccion y paridad `RackDuplicationPlan` | GREEN |
| CT-SCAN / `ProjectVariableScanProjection` | GREEN |
| CT-GEO / primitivas y facts `Transform2D` | GREEN |
| Focal AUTH-06..08 + sistemas | 122/122 PASS |
| Regresion de guardias | 56/56 PASS |
| Core Full local | 8,198 passed; 0 failed; 0 skipped |
| UI Full local | no requerida durante F4 por LC-UI; cubierta por CI exacta |
| Build Debug completo | success; 0 errors; 2 `MSB3277` conocidos y deuda historica xUnit |
| CI push exacta | run `35493026166`; event `push`; branch exacta; head exacto; 4/4 jobs success |

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35493026166>

## Inventario de productores legacy

| Productor | Disposicion F4 | Resultado |
|---|---|---|
| Snapshots privados previos de `RackDuplicationPlan` | REMOVED | sustituidos por los snapshots compartidos, sin autoridad neutral paralela |
| Clasificacion/dedupe fisico en `RackDuplicationPlan` | DELEGATED | usa `RackPhysicalSelection`; policy COPY permanece local |
| Probe de `RackDuplicarCommands` | KEEP / ADAPTED | lee AutoCAD en Plugin y emite snapshots puros con origen fisico |
| `ProjectVariableScanProjection` | KEEP | conserva semantica authored/PV propia; no se promueve a snapshot universal |
| `Transform2D` | KEEP / REUSE | autoridad matematica existente, sin cambios |
| `RackTransformFacts` | NEW ADAPTER | observa una descomposicion tipada con tolerancia del caller |

## Scope guard y cierre

No cambiaron schema DWG, persistencia, BOM, geometria dibujada, comandos, prompts, errores, transacciones, RackIds,
nombres, numero de copias ni restamp. No se implementaron AUTH-09..13 ni `PreparedViewPlan`. Las policies de I-52 e
I-55 permanecen fuera de Foundation. No aparecio contradiccion material ni CR material o menor abierta.

F4 queda `COMPLETE`. `OV-FND-04` permanece reservada para F7/Candidate. Este recibo no abre F5, no declara Candidato,
no integra la rama y no modifica Proposal V5, R3 ni ADR-0044.
