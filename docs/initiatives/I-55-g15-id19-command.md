# I-55 — G15: ID19 end-to-end, `RACKPROYECTAR`

```text
Gate               = G15 (ID19 COMMAND, END-TO-END)
Estado             = COMPLETE
Alcance            = comando RACKPROYECTAR / RPY sobre el contrato puro de G14 y sobre AUTH-15 integrada; sin edicion de Foundation ni de esquema
PRE_REBASE_I55_SHA = 53d5d99103ae79cb8258016fa3aee16b1dcd75cc
POST_REBASE_BASE   = 3375aadbbf929427a6d106b2fff275d64863b89c   (origin/main con AUTH-15 integrada)
G15_START_SHA      = c3dd4ff8d12404ccf6eeb34fa59c0b12e286bbea
G15_PRODUCT_SHA    = 5b804777fec770b712324ecb27039ee7e62daaee
G15_FINAL_SHA      = el commit de cierre que publica este recibo
Archivo de preservacion = archive/i-55-pre-auth15-integration-53d5d99 (anotado, empujado, nuevo)
```

## 1. Recibo de AUTH-15 verificado antes de consumir

| Requisito | Observado |
|---|---|
| `integration/I-52-AUTH15` existe y es anotado | si (`git cat-file -t` = `tag`) |
| Objeto del tag | `abb135e9946fffce6992875e4a32e4f780d1b11c` |
| Destino pelado | `3375aadbbf929427a6d106b2fff275d64863b89c` = `origin/main` |
| `FINAL_CANDIDATE_SHA` | `0b6abdd5d0e323944225b30832f77f68b7c3c497`, ancestro de `main` |
| `CLOSURE_SHA` | `46ae4c93bdec0901ef2212f25d230b1b6baa8b9d`, ancestro de `main` (segundo padre del merge) |
| `MERGE_SHA` | `3375aadbbf929427a6d106b2fff275d64863b89c` |
| CI post-merge | run `36628580469`, `success`, cuatro trabajos, `headSha = 3375aadb` |
| Cobertura del Candidate | run `36629100845`, `workflow_dispatch`, `success`; el registro del dispatch muestra `candidate_sha = 0b6abdd5…` y el checkout de ese SHA |
| `integration/I-57`, `I-58`, `I-59` | presentes y anotados |

La unica diferencia observada frente al enunciado es la esperable en un dispatch: el `headSha` del run es el de `main`
(`3375aadb`) y el Candidate medido viaja en el input `candidate_sha`, que el log del run confirma. El identificador de run y su
resultado coinciden con la orden.

**Dictamen de host integrado, preservado sin reinterpretar:** `HOST RESULT = PASS UNDER DOCUMENT-AUTHORITY`; `Raw RUN-3 = FAIL`
conservado (causa: solo los controles de caracterizacion SIDE-DB); la autoridad de rollback soportada es la base de datos del
documento con `LockDocument` y `StartTransaction`; `OpenCloseTransaction` no esta soportado. G15 no trata el FAIL crudo de SIDE-DB como
bloqueo y no usa `OpenCloseTransaction` en ninguna parte.

## 2. Reconciliacion de I-55 sobre AUTH-15

- Punta previa preservada en `archive/i-55-pre-auth15-integration-53d5d99` (etiqueta anotada nueva, empujada; las cuatro
  etiquetas de archivo anteriores no se tocaron).
- Rebase de los 46 commits de I-55 sobre `origin/main` (`3375aadb`) sin conflictos. `git range-diff 016bf467..53d5d991
  3375aadb..HEAD` empareja los 46 con `=`: ninguno cambia. El unico archivo comun entre `main` y I-55 era `docs/ROADMAP.md`, y
  git lo fusiono limpio (una fila de I-55 y una de I-52-AUTH15).
- El resultado contiene el trabajo de I-55 hasta G14, I-58, I-59 y AUTH-15. Arbol limpio.
- La evidencia de SHA exacto anterior no se cuenta para la punta rebasada: la linea base, las suites y la CI se repiten sobre ella.

Linea base focal tras el rebase (antes de escribir G15): G14, Foundation, AUTH-15 e I-59, **204 / 204 verdes** (`RackGroupPlacementPlan`,
`CommonTransform2D`, politicas Rigid y Orthographic, remedios, invariantes de geometria y de conteo, consumidores de Foundation,
`RackDefinitionCreatorGuardTests`, `I59F*`). Censo de comandos de partida: **35** registros `[CommandMethod]`, sin `RACKPROYECTAR` ni `RPY`.

## 3. API de AUTH-15 consumida

`RackDefinitionCreator.CreateInTransaction(...)`, con sus dos sobrecargas por familia, y `RackDefinitionCreationResult`:

| Familia | Sobrecarga | Rack kinds de G15 |
|---|---|---|
| `HeaderRunPlan` | `(Database, Transaction, LateralHeaderDrawer, HeaderRunPlan, string requestedBlockName, RackEmbedDocument)` | Selectivo, Dinamico, Push Back, Cabecera |
| `CantileverViewPlan` | `(Database, Transaction, CantileverViewPlan, string requestedBlockName, RackEmbedDocument)` | Cantilever |

`RackProjectionMaterializationFamilies.Of(kind)` fija el despacho por familia de plan y no por kind; Flow Bed no tiene familia y no se
expone. Todo plan de G14 mapea a una familia existente: no se anade ninguna sobrecarga ni hubo contradiccion material.

| Resultado de AUTH-15 | Manejo en G15 |
|---|---|
| `IsSuccess` | se guarda `DefinitionId` y el nombre efectivo, y se crean las referencias |
| `MissingLibraryBlocks` | `DefinitionIncomplete`: fallo de materializacion, sin commit (defensa en profundidad; el preflight estricto y la importacion lo previenen) |
| `TransactionMismatch`, `InvalidPlan`, `InvalidBlockName`, `InvalidEnvelope`, `EnvelopeWriteFailed`, `WriteFailed` | `DefinitionCreationFailed` con el codigo tipado y el diagnostico, sin commit |

G15 no envuelve ni copia su mecanica: no llama `CreateSystemBlock`, `CreateBlockDefinitionNamed` ni `RackBlockData.Write` para crear la
definicion (guarda de fuente) y AUTH-15 sigue sin `LockDocument`, transaccion propia, `Commit`, referencia, importacion, `Regen` ni purga.
El nombre pedido es el nombre base de la autoridad Foundation (AUTH-11); `UniqueBlockName` lo resuelve el Plugin dentro de la familia.

## 4. Comando

`RACKPROYECTAR`, alias `RPY` (OD-1 A). Un solo archivo de comando, `src/RackCad.Plugin/RackProyectarCommands.cs`; ninguna segunda familia y
ningun `RackProyectarCommands` adicional. El comando no decide nada: entrega el lado AutoCAD a `RackProjectionCommandRun` (Application).

```text
ACQUIRE     seleccion + clase de vista (Frontal / Lateral / Planta, en ese orden de palabras clave, OD-5)
SNAPSHOT    UNA lectura: hechos fisicos de cada referencia (AUTH-07, AUTH-08 V2), UNA pasada por la BlockTable, catalogo y registro de variables
G14         CLASSIFY, GROUP, AUTHORITY GATES, REPRESENTATIVE, RESOLVE (una vez por RackId), EDIT PREFLIGHT, AVAILABLE, FRAMES, VALIDATE, PLANS
avisos      superposicion, cercania al limite angular (OM-33), pieza visual opcional: ANTES del punto, sin bloquear
PICK        GetPoint con AllowNone = true, base y destino, SCP -> universal
BeforeWrite aviso de unidades (I-05), una vez
IMPORT      importa las claves de biblioteca del plan y observa el dibujo con la consulta soportada
PLACE       UNA CommonTransform2D para toda la operacion (Place(B, T))
MATERIALIZE UNA transaccion del llamador, una definicion nueva por RackId (AUTH-15), sus referencias, UN commit
informe     vistas enlazadas del mismo rack, no copias: el BOM no cambia (para copiar, RACKDUPLICAR)
```

| Tema | Comportamiento |
|---|---|
| Antes del punto | todo fallo de clasificacion, resolucion, disponibilidad, marcos, validacion, gates o planes, y la politica estricta de biblioteca (abajo) |
| Miembros | objeto que no es bloque, fuera del espacio modelo o sin datos de RackCad: aviso y se ignora (I-51). Cualquier otro miembro no proyectable (xref, kind ajeno, MINSERT, dinamico, anonimo, anotativo, escala o normal no admitidas): **falla toda la operacion** y se listan todos (OD-3) |
| Punto `OK` | continua |
| Punto `None` (Enter) o `Cancel` (Esc) | se detiene, cero escrituras y ninguna importacion; Enter no vuelve a pedir el punto |
| Punto `Error` | fallo del comando, cero escrituras |
| Politica de biblioteca (ID19, no «coloca y reporta») | `Required` con clave invalida, `FileMissing`, `BlockMissing` o `Unknown`: falla antes de los puntos. `OptionalVisual` ausente: aviso. `NotApplicable`: se ignora. Rol desconocido: falla cerrado |
| Importacion | despues de todas las validaciones puras y antes de materializar, fuera de la transaccion de escritura; la observacion posterior es la consulta soportada del dibujo (no la biblioteca externa) y una clave que la observacion no menciona se trata como ausente; el fallo nombra su causa («biblioteca no disponible» o «faltan bloques de biblioteca en el dibujo») y no escribe ningun rack |
| Regen y purga | ninguno (V4 §15, «ID19: ninguno») |

### Propiedad de la transaccion

| Dueno | Pieza |
|---|---|
| G15 (`RackProjectionWriteScope`) | `LockDocument`, **una** `StartTransaction`, el unico `Commit`, las referencias, el orden, el `Dispose` sin commit que descarta todo |
| AUTH-15 | crear una definicion, materializar el plan ya preparado, persistir el sobre ya compuesto y verificarlo, dentro de la transaccion ajena |
| Sin dueno (prohibido) | `OpenCloseTransaction`, commit por rack o por vista, limpieza manual de definiciones, purga, `Regen`, estados de lote parcial |

Secuencia: por cada `RackId` en orden estable, `CreateInTransaction` (definicion + sobre) y despues **una referencia por cada referencia
fuente**, todo en la misma transaccion; solo si todo tuvo exito, **un** `Commit`. Un fallo de AUTH-15 (incluido `MissingLibraryBlocks`),
de una referencia o una excepcion no confirma nada, no continua con los grupos siguientes y deja que el `Dispose` descarte la transaccion.

### Identidad, geometria y sobre

- Cada definicion proyectada lleva el `RackId` de su rack origen: el sobre se compone **antes** de AUTH-15 con `RackProductPreparer`
  (G7) sobre el sobre representante, de modo que `Id`, `Kind`, `View`, `Section`, diseno y `CustomProperties` son los del rack y la vista
  destino. No hay `Guid.NewGuid`, `NewRackId`, restamp ni semantica de `RACKDUPLICAR`; el conteo logico de racks y el BOM no cambian.
- La definicion nueva nace con `Origin = 0`; la posicion sale de las anclas de G14 (`Position = ancla destino − R(ρ)·a_t`), de modo que el
  `Origin` de la fuente no se hornea en el destino (`Origin ≠ 0` cubierto).
- `Rigid` (misma clase, `alpha = 0`, solo traslacion comun) y `Orthographic` (planta ↔ elevaciones, Relative Frame Window) las decide G14;
  el comando no recalcula politica ni transforma entidades crudas ni usa COPY/MIRROR.
- La referencia conserva la presentacion de creacion vigente (OD-8 A) y la `Z` de G14.

### Mapa de familias

| Kind del rack | `RackSystemKind` | Familia AUTH-15 |
|---|---|---|
| Selectivo | `SelectiveRack` | `HeaderRunPlan` |
| Dinamico | `PalletFlow` | `HeaderRunPlan` |
| Push Back | `PushBack` | `HeaderRunPlan` |
| Cabecera | `Selective` | `HeaderRunPlan` |
| Cantilever | `Cantilever` | `CantileverViewPlan` |
| Cama | `Cama` | ninguna: rechazada antes del punto |

## 5. Consumo de Foundation (sin sustitutos locales)

| Autoridad | Consumo en G15 |
|---|---|
| AUTH-07 | `RackPhysicalSelection` sobre la seleccion filtrada; los miembros ignorados salen con aviso |
| AUTH-08 V2 | `RackSourcePlacementCaptureAdapter` + `RackSourceTransformClassifier` en el snapshot; los hechos viajan a G14 |
| AUTH-09 | `RackResolvePorts.*`, una vez por `RackId`; el preparador recibe el mismo resultado, nunca resuelve dos veces |
| AUTH-10 / AUTH-11 | `RackViewPreparationPorts.*` y `RackViewBaseName` por `RackProductPreparer`; el nombre base va a AUTH-15 |
| AUTH-12 V2 | `RackHeaderPieceRequirementExtractorV2` y `LibraryPieceAvailabilityFlowV2` con `AutoCadExternalLibraryBlockQuery` (biblioteca externa) antes; la consulta del dibujo despues de importar |
| AUTH-13 | `RackAuthoredComparatorPorts.*` una vez por `RackId`; el resultado se reutiliza |
| AUTH-15 | ver §3 |

**Ediciones de Foundation en G15: NINGUNA.** Tampoco de esquema, DTO ni store. El diff frente a `origin/main` toca solo Application/Views/Placement,
el Plugin de vistas, `RackProyectarCommands.cs`, la ayuda, pruebas y documentacion. Cambios en codigo existente, todos sin efecto de comportamiento:
`RackSiblingScan` gana `CaptureDrawing` sobre el mismo recorrido (refactor a un metodo privado compartido), `RackViewBatchProducts` amplia la
visibilidad de sus constructores de plan y nombre y extrae los dos nombres en linea a `SelectiveName` y `HeaderName`.

## 6. Evidencia

| Suite | Resultado |
|---|---|
| RED de G15 (antes de implementar; los dos puntos de entrada `RackProjectionCommandRun.Execute` y `RackProjectionMaterializationRun.Execute` lanzaban `NotImplementedException` y los archivos del Plugin no existian) | Core nuevo: **75 fallos / 91** (`RackProjectionCommandRunTests` 40/49, `RackProjectionMaterializationRunTests` 4/9, `RackProjectionCommandGuardTests` 31/33); guardas reapuntadas: 3 fallos (censo de comandos en `SelectiveEditorOpenTests`, censo por nombre en `CustomPropertiesCommandGuardTests`, aviso de unidades en `RackUnitsGuardSourceTests`); UI: 3 fallos de 15 en `CustomPropertiesHelpCensusTests`. **81 fallos en total.** |
| Correcciones de pruebas tras el RED | dos defectos de las propias pruebas, no del producto: el oraculo de `G15_U` (desplazamiento esperado) y la fixture de Flow Bed (`RackProjectionPreparedView` rechaza correctamente una familia sin creador) |
| GREEN focal de G15 | `RackProjectionCommandRunTests` 49/49, `RackProjectionMaterializationRunTests` 9/9, `RackProjectionCommandGuardTests` 33/33, `I55G15Auth15ConsumerTests` 5/5, censos y guardas reapuntadas 67/67 |
| G14 focal | 83/83 |
| AUTH-15 (`RackDefinitionCreatorGuardTests`) + I-59 (`I59F*`) | 121/121 |
| Impactos G12 / G11 / G9b / primera vista (`RackViewBatch*`, `G9b*`, `RackSiblingInsert*`, `RackSiblingRedraw*`, `FirstViewFreedom*`, `SingleScanInsert*`) | 51/51 |
| Core completa | **11723 pruebas, 0 fallos, 0 omitidas** (11599 de G14 + 124) |
| UI completa | **1621 pasan, 0 fallos, 17 omitidas** (las mismas 17 de la base; +5 pruebas de ayuda y censo) |
| Build UI Debug | 0 errores, 0 advertencias |
| Build Plugin Debug | 0 errores (solo las advertencias `MSB3277` conocidas) |
| Build Plugin Release | 0 errores (mismas advertencias) |
| CI de `push` del producto | run `36635411537` sobre `5b804777`: `success` en los cuatro trabajos |
| CI de `push` del cierre | el run de `push` sobre el commit de cierre se informa en el reporte del gate; este recibo no se autorreferencia |

Clases nuevas: `RackProjectionCommandRunTests`, `RackProjectionMaterializationRunTests`, `RackProjectionCommandGuardTests`,
`I55G15Auth15ConsumerTests` y el soporte `G15CommandTestSupport`. Reapuntadas: `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` (35 → 37),
`CustomPropertiesCommandGuardTests` (censo por nombre con `RACKPROYECTAR` y `RPY`, mutaciones incluidas),
`CustomPropertiesHelpCensusTests` (UI) y `RackUnitsGuardSourceTests` (un aviso de unidades por operacion). Ninguna guarda se debilito y no hay omisiones nuevas.

Matriz de pruebas pedida (A..AJ): A/B (comandos y alias), C (snapshot antes de los puntos), D/E (bloqueo puro y bloque requerido ausente sin `GetPoint`), F/G/H
(Cancel, None y Error sin escrituras), I/J/K/L/M/N (escritura solo tras el plan aceptado, transaccion del llamador con `StartTransaction`, sin
`OpenCloseTransaction`, `LockDocument` del llamador, una transaccion, un `Commit`), O/P (fallo de AUTH-15 y `Missing` sin commit), Q/R (definicion antes que la
referencia y la referencia la coloca G15), S/T (mismo `RackId`, sin `Guid.NewGuid` ni restamp), U (transformacion comun compartida), V/W (`Origin ≠ 0` y definicion con
`Origin = 0`), X/Y (Rigid y Orthographic), Z..AD (los cinco kinds por su familia), AE (Flow Bed rechazado), AF/AG (Resolve y Prepare una sola vez, no despues del punto ni
dentro de AUTH-15), AH (sin estados de lote parcial), AI (sin `Regen` ni purga), AJ (censos y ayuda).

## 7. G9b modo 1

AUTH-15 habilita tecnicamente el modo 1 de G9b (crear la definicion en la transaccion del llamador durante Insertar). G15 **no** lo cablea ni lo
reabre: queda registrado como disponible para una validacion de Candidato futura.

## 8. Plan de validacion del Owner: OV-ID19 (no ejecutada)

Disposicion al cierre del gate: **DEFERRED TO CANDIDATE**. No se declara PASS. Requiere el DLL Debug del worktree en AutoCAD 2025.

| # | Pasos | Esperado |
|---|---|---|
| OV-ID19-01 | Varios racks de una fila → **Planta** (misma clase, Rigid) con destino desplazado | Vistas enlazadas con la orientacion de cada rack y la disposicion relativa conservada; conteos identicos; aviso final de que no son copias |
| OV-ID19-02 | Plantas → **Frontal** (Planta→Frontal) | Frontales lado a lado sobre una base comun, en el orden y la separacion de la corrida |
| OV-ID19-03 | Plantas → **Lateral** (Planta→Lateral) | Laterales alineadas por el eje de profundidad, con el orden de la fuente |
| OV-ID19-04 | Frontales → **Planta** (Frontal→Planta) | Plantas en orientacion natural sobre una linea comun, con el orden y la separacion originales |
| OV-ID19-05 | Laterales → **Planta** (Lateral→Planta) | Idem sobre el eje de profundidad |
| OV-ID19-06 | Racks girados 180° en la fuente → cada clase | La corrida conserva su tramo; el sentido lo fija la ventana relativa, no una mayoria |
| OV-ID19-07 | Fila con un rack girado 180° y despues con racks girados suficientes para ser mayoria → Frontal | Con la Relative Frame Window el orden sobre la corrida no cambia en ninguno de los dos pasos; registrar lo observado |
| OV-ID19-08 | Corrida cuyo angulo cae junto al limite `3π/4` de la ventana | Aviso de cercania antes de pedir el punto; no bloquea |
| OV-ID19-09 | Dos filas (2 × N) → Frontal | Aviso de superposicion **antes de pedir puntos**, con los pares; el aviso no bloquea |
| OV-ID19-10 | Planta con `Origin ≠ 0` (`RACKLAYOUT` sobre un bloque redefinido con punto base desplazado) → Planta | Cada vista nueva queda en la posicion del ancla fuente trasladada, sin el desplazamiento del `Origin`; la definicion nueva tiene `Origin = 0` |
| OV-ID19-11 | Pieza requerida cuyo bloque no esta en `blocks-library.dwg` (probar tambien con la biblioteca ausente) | Error **antes de pedir puntos**, con la pieza y la causa («biblioteca no disponible» o «faltan bloques…»); nada escrito |
| OV-ID19-12 | Mezclas invalidas: plantas y una frontal; varias definiciones de un `RackId`; xref; MINSERT; escala ≠ 1; reflexion; Cama; Cantilever y Selectivo juntos en Frontal | Error explicito **sin pedir puntos**, con todos los ofensores; nada escrito |
| OV-ID19-13 | Esc o Enter en punto base o destino | Nada escrito ni importado; Enter no vuelve a pedir |
| OV-ID19-14 | Forzar un fallo de materializacion (capa bloqueada en un destino) | Se deshace la operacion completa: ninguna definicion ni referencia nueva; `PURGE` no encuentra definiciones de rack sin referencias |
| OV-ID19-15 | Guardar, cerrar y reabrir despues de proyectar | Las vistas enlazadas persisten con el mismo `RackId`; `RACKEDITAR` sobre una vista proyectada cambia todas las vistas del rack |
| OV-ID19-16 | `RACKLISTA` y `RACKBOMTOTAL` antes y despues | Mismo numero de racks y mismo BOM; solo cambia el numero de vistas |
| OV-ID19-17 | `RPY` y `RACKAYUDA` | El alias ejecuta el mismo comando; la ayuda documenta `RACKPROYECTAR / RPY` |
| OV-ID19-18 | Selectivo, Dinamico, Push Back, Cantilever y Cabecera, cada uno hacia una clase distinta | Cada kind proyecta por su familia; ninguna vista sale vacia |

## 9. Estado

```text
G15 = COMPLETE
G16 = OPEN
ID17 = COMPLETE
ID18 = COMPLETE
ID19 = COMPLETE / END-TO-END
AUTH-15 = INTEGRATED / CONSUMED
G14-CR-01 = RESOLVED BY I-59
G14-CR-02 = RESOLVED BY I-59
G12-CR-01 = RESOLVED BY I-58
PR-1 = RESOLVED
PR-2 = RESOLVED
Owner Validation = DEFERRED TO CANDIDATE (OV-ID19 preparada, no ejecutada, sin PASS declarado)
Foundation diff = NONE
Schema diff = NONE
```

**Lista de cierre:** recibo de AUTH-15 verificado; I-55 reconciliada sobre `main` con AUTH-15; API exacta consumida; sin creador de definicion duplicado; `RACKPROYECTAR` y `RPY`
existen; el plan de G14 maneja el comando; todos los bloqueos antes del punto; politica estricta de biblioteca de ID19 conservada; Cancel, None y Error sin escrituras;
`LockDocument` del documento y `StartTransaction`; `OpenCloseTransaction` ausente; una transaccion de escritura y un `Commit`; AUTH-15 no confirma; un fallo de AUTH-15 deshace todo;
las referencias las coloca G15; mismo `RackId` sin restamp; `CommonTransform2D` conservada; Rigid, Orthographic, Relative Frame Window y `Origin ≠ 0` en verde; definicion con `Origin = 0`;
Flow Bed rechazado; censos y ayuda en verde; conteo de racks y BOM sin cambio; sin semantica de lote parcial; sin ediciones de Foundation ni de esquema; suites completas, builds y CI en verde.

**Open Material:** ninguno propio.
**Open Minor:** (1) el `headSha` del dispatch de cobertura de AUTH-15 es el de `main` y no el Candidate (el Candidate viaja en el input `candidate_sha`); (2) OV-ID19 sigue sin ejecutarse: la primera comprobacion en AutoCAD 2025 llega con el Candidato (el rollback de una transaccion de documento y la creacion de definiciones en la transaccion del llamador estan protegidos por pruebas de forma y por el dictamen de host de AUTH-15, no por una corrida de `RACKPROYECTAR`); (3) G9b modo 1 esta tecnicamente habilitado por AUTH-15 y no se cablea aqui; (4) las 17 omisiones de UI son las de la base.

