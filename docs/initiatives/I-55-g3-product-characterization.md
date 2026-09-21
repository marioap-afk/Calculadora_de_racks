# I-55 — G3 Product Characterization

Fecha: 2026-09-21  
Gate: G3  
Inicio exacto: `128b263554d83d19b701037b9861356d1cc2f3a5`  
Proposal: V5 congelada (`38bee23a3a886a5026664bfda6d260ba5c65fc26`)  
Foundation: `integration/I-57` anotado, target `7097057cf8685bf5ecc09083cba37379d4a4aae8`  
R3: `EFFECTIVE`, blob `cd42db03becff42f98b047e61c46689c17a69670`

## 1. Frontera

G3 caracteriza el producto vigente. No cambia `src/`, no implementa ID17/ID18/ID19, no corrige PR-1/PR-2 y no crea AUTH-15.
Las CT-04, CT-05, CT-16, CT-RES, CT-PLAN, CT-NAME, CT-SCAN, CT-GEO, CT-BLK y CT-AUTH siguen perteneciendo a I-57; no se duplican.

## 2. Censo REUSE / EXTEND / NEW

| Clasificacion | Evidencia |
|---|---|
| REUSE | `RackEmbedDocumentTests`, `RackListBuilderTests`, `SelectiveBomAuthorityTests`, `RackDuplicationPlanTests`, `CustomPropertiesEnvelopeCharacterizationTests`, `CustomPropertiesEnvelopeGuardTests`, `CallerOwnedFacadeGuardTests`, `SelectiveShellMigrationTests`, `RichEditorBlockedActionTests` |
| EXTEND | Ninguna clase existente: G3 evita mezclar ownership histórico con el receipt de I-55 |
| NEW Core | `LegacyViewPayloadCompositionCharacterizationTests`, `RackCountInvariantCharacterizationTests`, `RackEditPreflightCharacterizationTests`, `InsertBlankIdFlowCharacterizationTests`, `InsertRedrawLayerAccessCharacterizationTests`, `I55DeferredProductFixCharacterizationTests` |
| NEW UI | `FirstViewGateCharacterizationTests` |

Las guardas nuevas son sensibles a mutaciones de orden, predicado, cantidad y seam. No sustituyen una prueba conductual futura cuando G4+
introduzca comportamiento nuevo.

## 3. Matriz vigente de primera vista y exposicion

Esta tabla separa lo que el producto ofrece hoy de lo que Proposal V5 decide para los gates posteriores.

| Sistema | Primera vista vigente | Vistas adicionales vigentes | Exposicion futura V5 |
|---|---|---|---|
| Selectivo | Frontal | Lateral y planta solo al editar un rack existente | F/L/P; lateral y planta podran iniciar tras gates posteriores |
| Dinamico | Lateral | Planta; frontal existe en el contrato del reader/builder pero no como CTA actual del editor | F/L/P |
| Push Back | Vista elegida por el editor: lateral, frontal entrada/salida, frontal posterior o planta | Las mismas cuatro familias | F/L/P; PR-1 debe cambiar indice UI por `PostIndex` fisico |
| Cantilever | Frontal, lateral o planta segun el editor | Las mismas familias | F/L/P; PR-2 debe propagar `PlantaVisibility` en Plugin |
| Cabecera | Lateral | Planta al editar | L/P |
| Cama | Lateral unica (`View=null`, `Section=-1`) | Ninguna | No batch ni ID19 |

La exposicion de ID19, las variantes canonicas y Relative Frame Window permanecen decisiones de producto de Proposal V5; G3 solo fija
sus entradas actuales y no crea la politica.

## 4. `RackId`, hermanas y authored vigente

| Kind | `Id` en blanco al editar | Membresia/redibujo vigente |
|---|---|---|
| Selectivo | `IsNullOrEmpty` adopta `window.RackId` | Busca definiciones por el Id curado; por eso puede adoptar definiciones ya presentes con ese Id (R-30) |
| Dinamico | `IsNullOrWhiteSpace` adopta `window.RackId` | Busca y redibuja el conjunto ligado |
| Push Back | crea fallback desde ventana, pero el preflight exige que cada hermana ya lleve ese Id | Falla cerrado ante kind/Id/descriptor divergente |
| Cantilever | igual que Push Back | Falla cerrado antes de redefinir |
| Cabecera | `IsNullOrEmpty` crea GUID nuevo | Busca las vistas con el Id resultante; lateral/planta conservan su propio sobre fuente |
| Cama | conserva `window.RackId`; no hace barrido de hermanas | Redefine la unica definicion elegida |

Los payloads conservan el contrato tipado por kind mediante `RackEmbedComposer`: `Kind`, `Id`, `Name`, `View`, `Section` y `Design`.
Selectivo reconcilia y serializa authored una sola vez para reutilizar exactamente el mismo JSON en todas las hermanas. Los metadatos
desconocidos y `CustomProperties` siguen entrando por el sobre fuente; las suites existentes cubren su round-trip y guardas.

## 5. Insertar, Actualizar, materializacion y cancelacion actuales

- Actualizar/redraw usa los wrappers históricos `RedrawInPlace`; cada vista conserva hoy su ciclo de lock/transaccion/commit.
- Los readers estrictos Push Back/Cantilever ejecutan preflight de kind, Id, authored interior y descriptor antes de la primera escritura.
- Dinamico/Cabecera abortan ante proyecto interior incompatible antes de redibujar.
- La escritura abre definicion, entidades y referencias en `OpenMode.ForWrite`. No existe preflight de capa bloqueada: esa ausencia es el
  baseline de G3 para §4.5 de Proposal V5.
- `BlockPlacement` considera cualquier resultado distinto de `OK` cancelacion, desecha la referencia y confirma una transaccion sin
  entidad añadida; la definicion creada previamente puede permanecer. Es el modo vigente previo a G9b y conserva R-25.
- Los mensajes de exito/error permanecen en el limite de cada comando. G3 no normaliza textos ni decide policy.

## 6. Conteos, BOM y listado

- `RACKLISTA` agrupa identidades sin distinguir mayusculas en `RackId`: tres definiciones F/L/P siguen siendo un rack con tres vistas.
- `BomAuthoredAuthority` toma como representante determinista `siblings[0]` solo despues de validar authored por
  `SelectiveAuthoredAuthority`; no elige una hermana arbitraria cuando divergen.
- Las suites BOM/listado existentes se reutilizan. G3 no duplica CT-RES ni cambia la resolucion efectiva por rack.

## 7. Baselines diferidos

### PR-1 — OPEN / CHARACTERIZED

`RackPushBackSystemWindow.SelectedView()` persiste `Math.Max(0, LateralSectionBox.SelectedIndex)`. El selector se llena por cantidad de
cortes, por lo que no conserva el `PostIndex` fisico cuando los indices no son contiguos. G4 debe invertir esta caracterizacion.

### PR-2 — OPEN / CHARACTERIZED

Los dos llamados Plugin a `CantileverViewPlanBuilder.Build` pasan linea, kind, factory y station, pero no
`design.PlantaVisibility`. G5 debe propagarla sin cambiar el builder neutral ni extraer otra autoridad.

## 8. Entradas de colocacion ID19

El snapshot físico ya observa posición, rotación y escala de cada referencia; `RackTransformFacts` conserva la descomposicion neutral.
G3 no aplica placement policy. Relative Frame Window, Rigid/Orthographic, CQ-01, anclas, ventanas y remedios siguen cerrados hasta G14/G15.

## 9. Evidencia G3

Focal Core:

```text
filter = LegacyViewPayloadCompositionCharacterizationTests | RackCountInvariantCharacterizationTests |
         RackEditPreflightCharacterizationTests | InsertBlankIdFlowCharacterizationTests |
         InsertRedrawLayerAccessCharacterizationTests | I55DeferredProductFixCharacterizationTests
31/31 PASS
```

Focal UI:

```text
filter = FirstViewGateCharacterizationTests
4/4 PASS
```

Impacto reutilizado:

```text
Core = 281/281 PASS
UI = 30/30 PASS
```

Las suites completas, builds Debug y CI exacta se registran como evidencia del SHA de cierre; el reporte de ejecución debe verificar
`src/ = 0` y los cuatro jobs requeridos.

## 10. Estado de salida

```text
G3 = COMPLETE once exact-SHA Full/build/CI evidence is green
G4 = OPEN only after that evidence
G5 = NOT OPEN
PR-1 = OPEN / CHARACTERIZED
PR-2 = OPEN / CHARACTERIZED
Product changes = ZERO
```
