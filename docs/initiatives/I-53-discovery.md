# I-53 — Discovery (G1): cabecera configurable hacia conjuntos de destinos, sistema por sistema

> **Esto es G1: un informe de caracterizacion.** No cambia codigo productivo ni pruebas. Las propuestas de
> la seccion 14 son **insumo para G2**, no un contrato: la implementacion sigue **bloqueada**, G2 **no se
> abre** en esta sesion, y la seccion 18 declara lo que necesita **revision de Arquitecto** y **decision del
> Owner**.
>
> Contrato: [I-53-cabeceras-configurables-multidestino.md](I-53-cabeceras-configurables-multidestino.md).

## 0. Metodo y clases de evidencia

Codigo auditado = `46fcac2`. En la punta de la rama (`405cfc0`) `src/` y `tests/` son byte-identicos: los dos
commits de I-53 solo tocan documentacion.

Cuatro auditorias de solo lectura en paralelo (Push Back/I-40, Selectivo/I-43, Dinamico, e inventario de
sistemas con la infraestructura compartida), seguidas de **verificacion directa, linea a linea, de toda
afirmacion que sostiene un hallazgo, una brecha de la matriz o una propuesta**. No se ejecuto AutoCAD,
ninguna suite ni ningun build.

| Clase | Significado |
|---|---|
| **HECHO** | leido en el codigo, con `archivo:linea` verificado |
| **TRAZA** | ruta de codigo seguida de extremo a extremo; **no ejecutada** |
| **DOC** | lo que afirma un documento del repositorio |
| **INFERENCIA** | deducido de hechos; no comprobado por ejecucion |
| **HIPOTESIS** | lectura sin fuente versionada; **requiere al Owner** |
| **auditoria** | cita tomada de las auditorias paralelas y no re-leida; ninguna sostiene sola un hallazgo |

Rutas abreviadas: `A/` = `src/RackCad.Application/`, `D/` = `src/RackCad.Domain/`, `U/` = `src/RackCad.UI/`,
`P/` = `src/RackCad.Plugin/`, `TC/` = `tests/RackCad.Tests/`, `TU/` = `tests/RackCad.UI.Tests/`.

## 1. Preflight

2026-09-12, desde antes de las 17:41 hasta las 18:25 -06:00; repetido antes del reclamo, despues del reclamo y
antes de cerrar este informe.

- `git fetch --prune origin`: `main` = `origin/main` = `46fcac2b071929d2bd5b07aa28373941417f74a8` (Merge
  I-51), divergencia `0/0`, arbol principal limpio.
- `git stash list` vacio. Sin `MERGE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`, `rebase-merge`,
  `rebase-apply` ni `sequencer`.
- Normas leidas desde `origin/main`: WORKFLOW, AGENTS, ROADMAP, HANDOFF, `initiatives/TEMPLATE.md` e
  `initiatives/README.md`. Sin cambios normativos desde la base de I-51: `git diff a4d88f1 46fcac2` es vacio
  sobre WORKFLOW, AUTOMATION_PLAN, AGENTS, CLAUDE.md, TEMPLATE e `initiatives/README.md` (ROADMAP y HANDOFF
  si cambiaron, por el cierre de I-51).
- Worktrees al empezar: el principal (`main`), I-49 e I-50. Al cerrar este informe hay seis: ademas I-52,
  I-53 e I-54.
- Ramas remotas al empezar: `main`, I-49 e I-50. Durante el preflight aparecieron I-52 (reclamo 17:41:09) e
  I-54 (reclamo 17:42:38).

## 2. Disponibilidad

| Iniciativa | Rama remota | Punta observada | Estado | Relacion con I-53 |
|---|---|---|---|---|
| **I-53** | ninguna antes del reclamo | — | **LIBRE**: cero ramas, filas, contratos, estados, decisiones o commits con `I-53` en cualquier ref (`git log --remotes --grep`, `git grep` en las cinco ramas) | — |
| I-49 | `architecture/motor-expresiones-parametricas` | `4df9480` (G2C, Proposal V3; solo docs; 6 delante / 9 detras de `main`) | Architect PENDING RE-REVIEW; implementacion bloqueada | sin cruce previsto |
| I-50 | `feature/cotas-independientes-por-vista` | `a4806ff` (G5 (C); 15 delante / 9 detras) | G5 hecho; faltan G6, G3, G7 y G8 | **cruce material en implementacion** (§16) |
| I-52 | `feature/rackmirror-espejo-semantico` | `7ee7975` (reclamo `5528176` + bootstrap) | G0 cerrado; ID16 RACKMIRROR | acoplamiento semantico (§16.3) |
| I-54 | `architecture/propiedades-personalizadas` | `f908b2f` (reclamo `143490d` + bootstrap) | G0 cerrado; ID24 Custom Properties | cruce bajo (§16.4) |

I-52 no existia en ningun ref al empezar el preflight; su reclamo aparecio a mitad y **no** es I-53.

## 3. BASE / CLAIM / BOOTSTRAP

```text
BASE_SHA      = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, Merge I-51)
CLAIM_SHA     = e1d5996e70cb75589beb96a256714982fe693be3   (commit vacio; push aceptado sin force: * [new branch])
Claim-Id      = d7144fe8-6921-4a46-9d66-2d723620bea4
BOOTSTRAP_SHA = 405cfc0afe6bb0baaa112a822cd62610eebfc6ea   (contrato + fila ROADMAP; solo docs)
```

Despues del push, `git log --remotes --grep "Initiative-Id: I-53"` devuelve **solo** `e1d5996`.

## 4. Rama y worktree

```text
Rama     = feature/cabeceras-configurables-multidestino   (upstream origin/feature/cabeceras-configurables-multidestino)
Worktree = C:\Users\alejandra-mendoza\.codex\worktrees\feature-cabeceras-configurables-multidestino
```

El worktree principal no se toco, y ninguna rama ni worktree ajeno se modifico.

## 5. Respuestas cortas

| # | Pregunta | Respuesta | Detalle |
|---|---|---|---|
| 1 | Sistemas vigentes | Siete `RackSystemKind`: Cabecera independiente (`Selective`, nombre historico), Dinamico (`PalletFlow`), Selectivo (`SelectiveRack`), Cama, Larguero, Push Back y Cantilever. **Drive-In no existe**: es un boton deshabilitado | §7 |
| 2 | Quien tiene cabecera configurable | Selectivo (por fondo y poste), Dinamico (por modulo), Push Back (por instancia fisica y por modulo) y la Cabecera independiente, que **es** una cabecera. Cantilever, Cama y Larguero: **ninguna** | §7, §8 |
| 3 | ID6 / ID7 | **Sin texto versionado** en ningun ref. Lectura provisional (HIPOTESIS H-ID): ID6 = «PB-006» generalizado, reutilizar la configuracion de OTRA cabecera como copia independiente; ID7 = «PB-007» generalizado, aplicarla a esta cabecera o a varias/todas. Requiere confirmacion del Owner | §6 |
| 4 | Push Back | **YA ENTREGADO** por I-40: instancia `(PostIndex, ModuleId)`, destinos cabeceras × lineas, origen recordado, copia canonica por destino, sesion con Confirmar y Cancelar. Se reconstruye sin reabrirlo | §9 |
| 5 | Selectivo | Autoridad por `(FondoIndex, PostIndex)` y eje de fondos (`TargetFondos`) completos. **Faltan** el eje de postes (hoy un solo poste), «copiar de» y la atomicidad estricta de la escritura | §10 |
| 6 | Dinamico | Modelo, autoridad, persistencia, BOM y dibujo, **compartidos con Push Back, ya soportan** overrides por linea. **El editor no tiene nada**: edita un modulo por referencia viva, sin sesion, sin destinos ni «copiar de», y con la **frontera obsoleta del configurador** que I-40 corrigio solo en Push Back | §11 |
| 7 | Contrato compartido | Viable **en dos piezas**: una instantanea neutral de cabecera y un protocolo PREPARE → MUTATE con informe. **No** en el direccionamiento de destinos, que es propio de cada sistema | §14 |
| 8 | Sistemas a cerrar | **Selectivo** y **Dinamico**. Push Back: nada. El resto: no aplica | §15 |
| 9 | Conflicto con I-50 | Ninguno en G0/G1. **Material en implementacion**: las ventanas del Selectivo y del Dinamico (G3 pendiente de I-50) y sus censos de `x:Name` | §16 |
| 10 | ¿Arquitecto? | **SI**, antes de cerrar G2 (AM-1..AM-4) | §18 |

## 6. ID6 e ID7: que se sabe y que no

HECHO: buscar `ID3`..`ID11` en `origin/main` y en las cinco ramas activas no devuelve ni ID6 ni ID7 (si ID20
e ID23 en I-49, sin relacion). Su texto vive en la numeracion del Owner, como ID22A antes de I-47
(`docs/automation/decisions/I-47.md:24-27`).

DOC: el contrato de I-40 fija la correspondencia de los rotulos que el Owner uso entonces
(`I-40-cabeceras-push-back.md:40-43`):

| Rotulo del Owner en I-40 | Rotulo interno | Significado |
|---|---|---|
| «PB-011» | PBH-01 | La cabecera personalizada, incluida su ALTURA, es la autoridad efectiva |
| «PB-007» | PBH-02 | Aplicar la configuracion a esta cabecera o a todas las cabeceras |
| «PB-006» | PBH-03 | Reutilizar la configuracion de OTRA cabecera como COPIA independiente |

**HIPOTESIS H-ID** (no se afirma). La autorizacion de I-53 separa «configurable header» (equivalente a
PBH-01) de «ID6 pendiente» e «ID7 pendiente», y declara Push Back **ya entregado**. Lo mas consistente con
ambas cosas es:

```text
ID6 = «PB-006» generalizado = tomar como ORIGEN la configuracion de otra cabecera y reutilizarla como COPIA independiente
ID7 = «PB-007» generalizado = APLICAR la configuracion origen a esta cabecera o a un CONJUNTO de destinos
```

La coincidencia 6/006 y 7/007 es un indicio, no una prueba. **La matriz de la seccion 8 usa H-ID y ademas
muestra las capacidades atomicas**, asi que sigue siendo util si el Owner fija otro texto (OD-1, §18).

## 7. Inventario de sistemas vigentes

HECHO `D/Systems/Shared/RackSystemKind.cs:4-31`. Registro en `A/Systems/Shared/SystemRegistry.Default.cs`,
handlers en `P/KindHandlers/KindHandlerRegistry.cs:55-63` (auditoria) y menu en `U/RackMainMenuWindow.xaml`.

| Sistema | `RackSystemKind` | Embed / handler | Cabecera | Nota |
|---|---|---|---|---|
| Cabecera independiente | `Selective` (nombre historico, `:6-7`) | `cabecera` / `CabeceraKindHandler` | **es** una `RackFrameConfiguration` | `RACKCABECERA` |
| Dinamico | `PalletFlow` | `dynamic` / `DynamicKindHandler` | por modulo longitudinal; overrides por linea soportados y nunca producidos | §11 |
| Selectivo | `SelectiveRack` | `selective` / `SelectiveKindHandler` | por `(FondoIndex, PostIndex)` | §10 |
| Cama | `Cama` | `cama` / `CamaKindHandler` | ninguna | — |
| Larguero | `Larguero` | sin embed ni handler (auditoria, `KindHandlerRegistry.cs:52-54`) | ninguna | no inserta |
| Push Back (un sentido y compuesto A/B) | `PushBack` | `pushback` / `PushBackKindHandler` | por instancia `(PostIndex, ModuleId)` y por modulo | §9 |
| Cantilever | `Cantilever` | `cantilever` / `CantileverKindHandler` | **ninguna**: cero coincidencias de `RackFrameConfiguration` o «Cabecera» en `**/Cantilever/**/*.cs` | aplica alcances solo a **brazos** |
| **Drive-In** | **no existe** | ninguno | — | HECHO `RackMainMenuWindow.xaml:139-144`: boton con `IsEnabled="False"`, titulo «Diseñar drive-in», rotulo «Próximamente.» y sin handler. Ni modelo, resolver, editor, DTO, BOM, dibujo, comando ni prueba |

## 8. Matriz

Leyenda: **SI** existe · **PARCIAL** · **NO** falta · **N/A** no aplica · **HECHO** entregado. ID6 e ID7 bajo
H-ID (§6).

| Sistema | Cabecera configurable | Autoridad | Alcance / destinos | ID6 pendiente (copiar de) | ID7 pendiente (aplicar a un conjunto) | Adaptacion | Persistencia | UI / editor | RACKEDITAR / Actualizar / guardar-reabrir | BOM | Dibujo |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **Push Back** | SI: instancia `(PostIndex, ModuleId)` y modulo (I-35); frontera correcta `window.Configuration` | `HeaderConfigurationAtPost` (linea, luego modulo, luego calculada), `AtCut`, `DerivedPostHeightAtPost` | cabeceras × lineas (producto cartesiano) | **HECHO** (`CopyHeaderFrom_Click` → `SourceConfigurationCopy`) | **HECHO** (`ApplyHeaderConfigurationToInstances`) | intents por `ModuleId+Kind`; overrides por linea filtrados solo por id de cabecera, sin informe | listas por linea en `DynamicRackSystemDocument`; `SchemaVersion` sin cambio | `RackPushBackSystemWindow`: sesion con Confirmar y Cancelar | `AdoptLoadedBaseline`; dibujar confirma la sesion | editor correcto; **RACKBOMTOTAL: overrides sin `Members` (L-2)** | lateral por linea; frontal y posterior = cortes; planta a nivel modulo |
| **Selectivo** | SI: `(FondoIndex, PostIndex)`; frontera correcta | `SelectiveCabeceraAuthority` (`EffectiveCustomAt` + profundidad del fondo) | 1 poste × `TargetFondos` | **NO**: sin «copiar de»; semilla solo del poste visible; sin cambios = cancelar | **PARCIAL**: eje de fondos SI, eje de postes NO; escritura no estrictamente atomica (L-7) | reduccion destructiva sin resurreccion; un fondo nuevo nace sin personalizadas | `PostCabeceras` + `ExtraFondoPostCabeceras` (DTO 1.0; 2.x solo por Project Variables) | `RackSelectiveWindow`: Personalizar y Restablecer poste | `LoadDesign`; Actualizar por fondo y poste; ida y vuelta probada en Core | `SelectiveBomBuilder` por `(k,i)`; refresca si `Members` esta vacio | lateral, planta y frontal por fondo |
| **Dinamico** | PARCIAL: por **modulo** (todas sus lineas); **frontera obsoleta (L-1)** | la misma que Push Back (compartida); planta y postes a nivel modulo | **NO**: solo el modulo seleccionado | **PARCIAL**: presets «Personalizada N» en memoria, un destino | **NO** | una reconstruccion pierde personalizadas y overrides **sin informe**; sin reconciliacion por `ModuleId+Kind` | mismo DTO; sin `SchemaVersion` propia (envoltorio 2.0) | `RackDynamicSystemWindow`: referencia viva, sin sesion, sin Cancelar, sin seam | `RestoreFrom`; los overrides cargados no se ven en la UI | `SystemBomBuilder` (compartido) | compartido con Push Back |
| **Cabecera independiente** | SI (el sistema es la cabecera) | la configuracion misma | N/A (una cabecera por rack) | N/A | N/A | N/A | `RackProjectDocument.Header` (`RackFrameProjectDocument` 1.0) | `RackFrameConfiguratorWindow` | `EditCabecera` | `BomBuilder.Build` | lateral y planta de cabecera |
| **Cantilever** | N/A (sin cabecera) | — | brazos: `Cell/Station/Level/Side/Line` | N/A | N/A | — | `CantileverLineDocument` 1.0 | `RackCantileverWindow` | `EditCantilever` | por componentes | `CantileverViewMaterializer` |
| **Cama** | N/A | — | — | N/A | N/A | — | `FlowBedDocument` 1.0 | `RackFlowBedWindow` | `EditCama` | `FlowBedBomBuilder` | `FlowBedDrawService` |
| **Larguero** | N/A | — | — | N/A | N/A | — | `LargueroDocument` 1.0 (solo biblioteca) | `RackLargueroWindow` | no soportado | `LargueroBomBuilder` | ninguno |
| **Drive-In** | N/A (no existe) | — | — | N/A | N/A | — | — | boton deshabilitado | — | — | — |

Capacidades atomicas, validas aunque el Owner fije otro texto para ID6 e ID7:

| Capacidad | Push Back | Selectivo | Dinamico |
|---|---|---|---|
| C1 Cabecera personalizada como autoridad efectiva (PBH-01) | SI | SI | PARCIAL (L-1) |
| C2 Unidad = instancia fisica | SI `(PostIndex, ModuleId)` | SI `(FondoIndex, PostIndex)` | NO (modulo); el modelo SI la admite |
| C3 Origen = otra cabecera existente, reutilizable sin reabrir el configurador | SI | NO | PARCIAL (presets) |
| C4 Destinos = conjunto elegido en los dos ejes del sistema | SI | PARCIAL (un poste) | NO |
| C5 Validar todo antes de mutar; Cancelar = mutacion cero | SI (sesion) | PARCIAL (L-7) | NO |
| C6 Copia canonica independiente por destino | SI | SI (delegado opcional) | NO (referencia viva) |
| C7 Ciclo completo (persistencia, RACKEDITAR, BOM, dibujo) sobre la misma autoridad | PARCIAL (L-2; planta) | SI | SI para lo que produce |

## 9. Contrato de I-40 reconstruido (Push Back = YA ENTREGADO; no se reabre)

Merge `bf327b353fc181d3ca5192641c54b9abf96ea39d` (padres `8a54a4d` y `2673aab`), aprobado por el Owner tras
cinco rondas rechazadas (`HANDOFF.md:3031-3048`). Contrato en `I-40-cabeceras-push-back.md`; decisiones
definitivas del Owner en `:320-341` (instancia fisica) y `:387-417` (frontal y posterior son cortes).

### 9.1 Unidad y modelo

- HECHO `D/Systems/Dynamic/DynamicHeaderLineOverride.cs:32-42`: `{ int PostIndex; string ModuleId;
  RackFrameConfiguration Header }`, en `DynamicRackDesign.HeaderLineOverrides` (`:60`) y
  `DynamicRackSystem.HeaderLineOverrides` (`:80`), anotadas «Solo Push Back las escribe».
- Nivel modulo (I-35): `DynamicRackModuleDesign.HeaderConfiguration` + `UseCalculatedHeaderConfiguration`
  (`DynamicRackDesign.cs:119-121`).
- Poste derivado por linea: `DynamicDerivedPostLineOverride { PostIndex, Height }` y el global
  `DerivedPostHeight`.
- Sesion: `A/Systems/Shared/RackModuleEditSession.cs` (851 lineas), con clave privada `LineKey(PostIndex,
  ModuleId)` (`:96-113`) y estados `committedLines`/`workingLines`.

### 9.2 Autoridad

HECHO `A/Systems/Dynamic/DynamicFrontGeometry.cs`:

```text
HeaderConfigurationAtPost(system, module, catalog, postIndex)       :277-306
  1. LineOverride(system, module, postIndex)                         :374-395  PostIndex + ModuleId ordinal; devuelve la referencia
  2. modulo personalizado o sin configuracion → la del modulo        :296-299
  3. calculada → WithZoneHeight(..., PostHeightAt(post, centroX))   :349-368  instancia NUEVA
HeaderConfigurationAt (lateral GENERAL)                               :330-343  NO consulta overrides
HeaderConfigurationAtCut(..., postIndex, end)                         :480-522  Exit = primera cabecera, Entrance = ultima
HeaderHeightAtPost                                                    :443-473
DerivedPostHeightAtPost(system, postIndex, inherited)                 :402-424  linea → rack → heredada
```

Consumidores: lateral seccionado (`DynamicSystemLateralBuilder.cs:134-136`, auditoria), BOM
(`SystemBomBuilder.cs:38-61`, auditoria), preview y frontal/posterior por corte. **La planta usa la
configuracion del modulo** (HECHO `DynamicSystemPlantaBuilder.cs:46-66`), y el lateral general tampoco
consulta overrides.

### 9.3 Origen

- HECHO `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:1909-1930`: `pendingHeaderConfiguration` es un
  campo de la **ventana**, no del estado. Lo fija `StageHeaderConfiguration`, que aplica de inmediato a la
  seleccion vigente.
- Entrada 1, «Configurar cabecera...» (`:1808-1858`): abre sobre `HeaderConfigurationCopy(modulo,
  SourceLine())` y, si no hay, sobre la instancia **viva** del ultimo computo (`:1837`, `:2360-2363`).
  `ShowHeaderConfigurator` fija `IsAdvancedEditor = alreadyCustom` (`:2346`) y **lee `window.Configuration`**
  (`:2357`): es la correccion de PBH-01.
- Entrada 2, «Tomar como origen»: `CopyHeaderFrom_Click` → `SourceConfigurationCopy`. Solo son elegibles
  cabeceras con `HasAnyPersonalization` (opcion A del Owner, DOC contrato `:169`). Lineas segun la auditoria:
  `CopyHeaderFrom_Click` `:1865-1897`, `SourceConfigurationCopy` (sesion) `:543-567`, `RefreshCopySources`
  `:2275-2314`.
- «Aplicar configuracion a la seleccion» re-aplica el mismo origen sin reabrir el configurador
  (`:1932-1942`; contrato `:352-354`).

### 9.4 Destinos

- HECHO `:2043-2145`: dos listas paralelas de la ventana (`headerTargetIds`/`headerLineIndexes`). Las
  cabeceras son los modulos `IsHeader` de la sesion (`:2096`); las lineas, `DynamicFrontActivation.
  PresentBoundaries` (`:2100`). Atajos «Esta» y «Todas» en cada eje. **No existe un tipo de destino en
  Application.**
- Producto cartesiano: `SelectedTargetModuleIds()` × `SelectedTargetLines()` → sesion.

### 9.5 Aplicar

HECHO `RackModuleEditSession.cs:396-458`, `ApplyHeaderConfigurationToInstances(configuration, moduleIds,
postIndexes)`:

```text
rechaza: configuracion nula, ningun modulo, ninguna linea                :401-420
valida ANTES de escribir: cada id existe e IsHeader; cada linea >= 0      :422-446
escribe: workingLines[(linea, id)] = clone.DeepCopy(configuration)        :449-455   una copia por par
```

- **No valida** la cota superior de la linea, la existencia de la frontera ni la cobertura del modulo en esa
  linea. INFERENCIA: un par sin cobertura se escenifica y persiste inerte, porque el BOM y el lateral solo
  recorren modulos cubiertos.
- Escenificado: aplicar no recalcula. `ConfirmModule_Click` → `CommitModuleEdits` + un `RequestRecompute`;
  `RequestDraw` confirma si hay cambios pendientes (`:3227-3230`); `Cancel` restaura lo confirmado.
- «Todas × Todas» en la UI final escribe **un override por par**: no escribe el nivel modulo ni limpia
  overrides. La semantica «modulo + `ClearLineOverrides`» solo existe en la sesion y solo la usan pruebas
  (`B_ApplyingToAll_MakesTheRackUniformAgain`).

### 9.6 Ciclo de vida y adaptacion

- `PushBackEditorState.AdoptLoadedBaseline` descarta la sesion al cargar; la firma `ModuleId:Kind` solo
  re-siembra en recalculos del mismo rack (ronda 3, contrato `:204-219`).
- Intents de modulo: `RackModuleReconciliation` por `ModuleId+Kind` exacto, con informe.
- HECHO `A/Systems/PushBack/PushBackEditorDesignAssembler.cs:327-365`: los overrides por linea se reconstruyen
  desde el commit o el baseline y se conservan si `headerIds.Contains(line.ModuleId)`, con `DeepCopy`. **No**
  se comprueban el tipo, el rango de `PostIndex` ni la cobertura; **no** se adaptan `Depth` ni `PostPeralte`;
  **no** hay informe. «Restaurar estandar» los descarta.

### 9.7 Persistencia

HECHO `A/Persistence/DynamicRackSystemDocument.cs:43-56`: `DerivedPostHeight`, `HeaderLineOverrides` y
`DerivedPostLineOverrides` (una lista vacia se escribe como `null`). `SchemaVersion` sin cambio
(`RackProjectDocument` 2.0; `PushBackDesignDocument` 1.0). Sin `WhenWritingNull` en esas propiedades ni
`DefaultIgnoreCondition` en el store (`RackProjectStore.cs:402-410`): las claves vacias salen como `null`
(L-4).

### 9.8 Compuesto A/B

DOC ADR-0031 §1: una sola lista, del rack. `CopySharedStructuralIntent` la copia a la estructura compuesta y a
cada lado (`PushBackCompositeStructure.cs:831-874`, auditoria); las cabeceras de B llevan prefijo `B:`; la
cola dormida se aparca (`PushBackCompositeEditorAssembler.cs:120-238`, auditoria). No verificado: el posible
cruce de ids sin prefijo en los marcos locales de B.

### 9.9 Pruebas que lo fijan

Inventario de la auditoria (HECHO solo para `TC/PushBackDerivedPostAndLineTests.cs:330-403` y la existencia
de `TC/PushBackHeaderApplyTests.cs` y `TU/PushBackHeaderAuthorityWindowTests.cs`):

- Core: `TC/PushBackHeaderApplyTests.cs` (PBH01_*, PBH02_*, PBH03_*,
  `AMultipleApply_NeedsASingleCommitAndASingleRecompute`, RONDA2_*, RONDA3_*),
  `TC/PushBackDerivedPostAndLineTests.cs` (A_*, B_*, Caso9_*), `TC/PushBackFrontalRearCutTests.cs`
  (CasoA..G) y `TC/RackModuleEditSessionTests.cs`.
- UI: `TU/PushBackHeaderAuthorityWindowTests.cs`, `TU/PushBackHeaderLifecycleTests.cs` (L1..L6),
  `TU/PushBackHeaderOwnerRoundTwoTests.cs` (D1..D4), `TU/PushBackPhysicalHeaderScopeTests.cs` (A..G),
  `TU/PushBackHeaderCartesianTests.cs` (Caso1..11) y `TU/PushBackHeaderLineWindowTests.cs`, con el soporte
  `TU/PushBackHeaderTestSupport.cs` (`Staged`, `Drawn`, `IsCustom`).

### 9.10 Discrepancias documento ↔ codigo (se registran; I-40 **no** se reabre)

1. `I-40-cabeceras-push-back.md:171` (ronda 2) promete excluir por dentro las cabeceras que ningun corte dibuja
   e informar las omisiones. El modelo final (`:302-318`) no lo repite y el codigo no lo hace: `DescribeApplied`
   cuenta cabeceras × lineas (`RackPushBackSystemWindow.xaml.cs:2023-2036`) y los destinos no se filtran por
   cobertura (`:2085-2145`).
2. «Ausente cuando esta vacio» (`DynamicRackSystemDocument.cs:47-56`; contrato `:297-298`;
   `HANDOFF.md:2091-2093`) frente a claves `null` (L-4).
3. `HANDOFF.md:2096-2098` dice que el Dinamico comparte `RackModuleEditSession`: **no tiene ningun consumidor**
   en el Dinamico (L-5).
4. `DynamicRackSystemResolver.cs:212` dice «COPIA canonica» y usa `CloneHeader` (L-3): es la raiz de L-2.
5. `SaveLibrary_Click` dice «el click confirma la edicion escenificada» (`:3331-3333`) y solo recalcula (L-6).

## 10. Selectivo

### 10.1 Modelo

HECHO `D/Systems/Selective/SelectivePalletDesign.cs` (776 lineas, archivo caliente): `PostCabeceras` (`:87`,
fila del fondo 0 con forma legacy), `ExtraFondoPostCabeceras` (`:100`, entrada k-1 = fondo k), `PostPeraltes`
(`:107`, global por poste y **sin** eje de fondo) y `CabeceraFondoOverrides` (`:66`). Espejo resuelto en
`SelectiveRackSystem.cs:54/:58/:61` y en el estado `A/Systems/Selective/SelectiveEditorState.cs:53/:60/:70`
(auditoria). Tipo: `RackFrameConfiguration`.

Las instancias se **comparten por referencia** entre estado, diseno y sistema (auditoria:
`SelectiveEditorState.cs:1308`; resolver `:189/:202`; carga `RackSelectiveWindow.xaml.cs:2658/:2671`). La
independencia depende de copiar **al escribir**.

### 10.2 Autoridad

`A/Systems/Selective/SelectiveCabeceraAuthority.cs` (auditoria): `CustomAt` (`:30/:34`), `UsableCustomAt`
(`:51-55`), `EffectiveCustomAt` (`:78-88`, impone la profundidad del fondo **en sitio**; ARQ-43-10 diferida),
`ImposeFondoDepth` (HECHO `:106`: refresca el modelo fisico) y `HasCustomAt` (`:117`). Consumen: lateral,
planta, BOM (HECHO `SelectiveBomBuilder.cs:450`: refresca la personalizada; la condicion «si `Members` esta
vacio» segun la auditoria, `:448-451`) y frontal por fondo via `FondoSystemView`. HECHO `TC/SelectiveFondoCabeceraTests.cs:680`,
`Frontal_RepresentsItsOwnFondo_NotTheMasterFondoZero`: cada frontal representa **su propio** fondo.

### 10.3 Destinos

`SelectiveEditorState.ApplyCabeceraToTargets(postIndex, configuration, deepCopy)` (auditoria `:1193-1228`):
**un** `postIndex` × `TargetFondos.Fondos` en orden ascendente; `PostExistsIn` (`:1166-1170`) omite el fondo
que no tiene ese poste; resultado `SelectiveCabeceraApplyResult(PostIndex, AppliedFondos, OmittedFondos)`.
`TargetFondos` es un `SelectiveFondoTargets` con `SelectiveTargetMode {FollowCurrent, Explicit, All}`.
`SelectiveTargetResolver` y `SelectiveTargetPlan` solo sirven a celdas. **No hay eje de postes** ni tipo de
plan `(fondo, poste)`.

### 10.4 Origen y flujo

HECHO `RackSelectiveWindow.xaml.cs:1167-1214`:

```text
CommitPendingEditors()                                           :1171  frontera C4
semilla = CloneCabecera(visibleCustom) o la estandar              :1188-1189  solo el poste del fondo VISIBLE
seed.Height = resolvedHeight; seed.Depth = fondo                   :1191-1192  sobrescribe incluso una personalizada
seed.PostPeralte = el override del poste o el global               :1196-1197
configurador sobre la semilla; lee window.Configuration            :1202-1205
serializacion igual a la de la semilla → no hace nada              :1206-1211
```

`ApplyCustomizedCabecera` (HECHO `:1221-1271`): revision de altura en **todos** los fondos destino (`:1232`) →
`ConfirmSevere` antes de escribir (`:1237-1241`) → escribe `postPeraltes[i]` (`:1252-1257`) →
`DeferRecompute { SaveWorkingToSelected; ApplyCabeceraToTargets(i, cfg, CloneCabecera); Recompute }`
(`:1261-1270`). `CloneCabecera` es `RackFrameProjectStore.DeepCopy` (`:1280-1281`).

Consecuencias: **no existe «copiar de»** otro poste u otro fondo en la UI, y **no se puede re-aplicar** una
personalizada existente a mas fondos sin editarla, porque un resultado sin cambios se trata como cancelar.

### 10.5 Atomicidad

- Cancelar en el aviso deja mutacion cero (prueba `TU/SelectiveCustomizeTargetsTests.cs`,
  `M_CancellingLeavesNoCustom_NoPostPeralte_AndNoRecompute`).
- Un recompute por aplicacion (`DeferRecompute`); un gesto con campos pendientes puede sumar el recompute de la
  frontera (`:1171`). Segun la auditoria, ninguna prueba fija «exactamente un recompute» al aplicar o
  restablecer una cabecera.
- **No es todo-o-nada** (L-7): `postPeraltes[i]` se escribe antes de `ApplyCabeceraToTargets` y aunque se
  omitan todos los destinos, y el bucle copia destino a destino sin fase de preparacion.

### 10.6 Adaptacion, persistencia, RACKEDITAR, BOM y dibujo

Todo lo de esta subseccion es auditoria salvo marca:

- `SyncPostCabeceras` recorta al reducir frentes o fondos y no resucita al crecer; un fondo nuevo nace sin
  personalizadas; cambiar la profundidad re-impone la del fondo (`SelectiveEditorState.cs:741-761`,
  `:1125-1135`).
- DTO `SelectivePalletDesignDocument` (`From` `:243-259`, `ToDomain` `:376-388`); `SchemaVersion` 1.0, con el
  2.x pegajoso solo por Project Variables; cabeceras como `List<RackFrameProjectDocument>`.
- RACKEDITAR: `SelectiveKindHandler.Edit` → `RackSelectivoCommands.EditSelective` → `LoadExisting` →
  `LoadDesign` (profundidad del fondo 0 con una regla en linea y sin refresco, `:2647-2659`).
- Pruebas: `TC/SelectiveFondoCabeceraTests.cs`, `TC/SelectiveCabeceraViewPurityTests.cs`,
  `TC/SelectiveCabeceraHeightReviewTests.cs`, `TC/SelectiveTargetFondosIntegrationTests.cs`,
  `TU/SelectiveCustomizeTargetsTests.cs`, `TU/SelectiveFondoCabeceraWindowTests.cs`,
  `TU/SelectiveGate8CorrectionTests.cs` y `TU/SelectiveTransactionBoundaryGuardTests.cs`.

### 10.7 Brechas

| Elemento | Estado |
|---|---|
| Autoridad por `(fondo, poste)` y lectura unica en vistas y BOM | SI |
| Eje de fondos elegido por el usuario | SI |
| Eje de postes (este / seleccionados / todos) | **NO** |
| Origen = otra personalizada («copiar de») | **NO** en la UI; la API acepta cualquier configuracion, pero para **un** poste |
| Re-aplicar sin reabrir el configurador | **NO** |
| Omitir e informar, nunca crear | SI (un poste × fondos) |
| Copia independiente por destino | SI en produccion; el delegado es opcional y varias pruebas de UI pasan `c => c` (auditoria) |
| Validar todo antes de mutar | PARCIAL (L-7) |

## 11. Dinamico

### 11.1 Modelo

Auditoria salvo marca. `DynamicRackSystem.Modules` es **una** secuencia longitudinal (decision de I-35);
`DynamicRackModuleKind {HeaderStart, HeaderIntermediate, HeaderEnd, Separator, Gap}` (el Dinamico nunca
produce `Gap`); `AssociatedFrameConfiguration` + `UseCalculatedHeaderConfiguration` por modulo
(`D/Systems/Dynamic/DynamicRackModule.cs:32/:38`). Ids `"M"+(i+1)` por posicion
(`DynamicRackSystemBuilder.cs:229-241`). Plantilla siempre `STD-3P`, sin eleccion. Separador: una sola pieza
constante (`DynamicRackDefaults.cs:48-57`), con niveles tomados de la **primera** cabecera
(`DynamicSeparatorGeometry.cs:26-45`). Postes derivados entre dos separadores consecutivos o en las fronteras de
cobertura. Globales del rack: `PostPeralte`, `SeparatorCountOverride`, `SeparatorSpacingOverride`,
`DerivedPostReinforced`, `DerivedPostReinforcementHeight`, `DerivedPostHeight` y `ManualHeaderHeightOverride`.

### 11.2 Frentes, fondos y lineas

Auditoria. N frentes dan N+1 lineas. Cada frente declara `PalletsDeep` y `DepthStartPosition`, con
anidamiento obligatorio (`DynamicDepthGeometry.cs:209-227`). Una linea cubre la union de sus dos frentes
(`CoverageAtPost` `:310-320`) y un modulo aparece en ella si la cobertura contiene su posicion. La
profundidad de una cabecera es la `Length` de su modulo. Frentes en blanco: la frontera solo desaparece si
**ambos** vecinos lo estan (`DynamicFrontActivation.BoundaryExists` `:97-111`).

### 11.3 Autoridad

La misma de §9.2, compartida. El Dinamico no produce overrides: la ventana no nombra `HeaderLineOverride`,
`DerivedPostHeight`, `RackModuleEditSession` ni `RackModuleReconciliation`. Pero HECHO
`DynamicRackSystemResolver.cs:212-238` y el DTO los transportan, asi que **si existieran, el lateral, la
frontal y el BOM del Dinamico ya los obedecerian**.

### 11.4 Editor

HECHO `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs` (2.937 lineas, archivo caliente):

```text
EditHeader_Click                                                             :667-724
  beforeEdit = Serialize(selectedModule.AssociatedFrameConfiguration)        :682
  new RackFrameConfiguratorWindow(selectedModule.AssociatedFrameConfiguration)  :684   ← referencia VIVA
  afterEdit  = Serialize(selectedModule.AssociatedFrameConfiguration)        :694   ← nunca window.Configuration
  iguales → «Cabecera sin cambios.»                                          :695-699
  UseCalculatedHeaderConfiguration = false; Depth → Length; preset «Personalizada N»  :701-716
RestoreDefault_Click → Recompose(forceRebuild: true)                         :726-730
```

Segun la auditoria: `ModulesGrid` esta enlazado a los modulos vivos del sistema (`:1717`) y
`ConfigBox_SelectionChanged` aplica «Calculada» o un preset **solo** al modulo seleccionado (`:1793-1839`).
Sin sesion, sin Confirmar ni Cancelar, sin seam de configurador, sin «Aplicar a» y sin destinos. Los presets
viven en memoria y no se siembran desde las personalizadas cargadas.

### 11.5 Adaptacion

Auditoria (`DynamicEditorDesignAssembler.cs:40-139`; ventana `:451-495`). Una reconstruccion (cambio de tarima
o de rangos de fondo) crea ids M1..MN nuevos, y `SnapshotHeaderFondos`/`RestoreHeaderFondos` conservan **solo**
las longitudes manuales de cabecera. Se pierden **sin informe** las configuraciones personalizadas, las
longitudes de separador, los cambios de tipo, `DerivedPostHeight` y los overrides por linea. Sin
reconstruccion, las personalizadas sobreviven (clonadas con `CloneHeader`) y `ApplyPostPeralte` les impone el
peralte del rack. `RackModuleReconciliation` existe, pero solo lo usa Push Back.

### 11.6 Persistencia, RACKEDITAR, BOM y dibujo

Auditoria:

- `RackProjectDocument.DynamicSystem` (2.0) → `DynamicRackSystemDocument`, **sin `SchemaVersion` propia**.
  `DynamicRackModuleDocument.Header` para toda cabecera, con reparacion de procedencia (`:839/:855`).
- RACKEDITAR: `DynamicKindHandler.Edit` → `RackDinamicoCommands.EditDynamic` → `LoadExisting` → `RestoreFrom`
  → resolver. Actualizar redibuja planta, frontales y laterales por poste, con un solo Regen.
- BOM: `SystemBomBuilder` por linea × cabecera cubierta, via `HeaderConfigurationAtPost`.
- Dibujo: lateral por linea; frontal con la altura del corte; planta con la configuracion del modulo.
- Pruebas: `TC/DynamicRackSystemBuilderTests.cs`, `TC/DynamicRackSystemResolverTests.cs`,
  `TC/DynamicEditorDesignAssemblerTests.cs`, `TC/PushBackModuleEditorCharacterizationTests.cs` (guardas de
  fuente sobre el Dinamico: `Fact1`, `Fact3`, `Fact5`, `Fact6`, `Fact7`), `TU/DynamicShellMigrationTests.cs`
  (censo de 63 `x:Name`), `TU/RichEditorCloseContractTests.cs` (el Dinamico **no** declara `OnClosing`) y
  `TU/DynamicEditorWindowTests.cs`. **Ninguna prueba de UI ejerce `EditHeader_Click`, `ApplyModule_Click` ni
  `ConfigBox`.**

### 11.7 Brechas frente a I-40

| Capacidad | Estado | Pieza compartida ya disponible |
|---|---|---|
| (a) personalizar una instancia `(PostIndex, ModuleId)` | PARCIAL: modelo, autoridad y DTO SI; editor NO; una reconstruccion la perderia | `DynamicHeaderLineOverride`, `HeaderConfigurationAtPost` |
| (b) destinos cabeceras × lineas | NO | `RackModuleEditSession.ApplyHeaderConfigurationToInstances`, `DynamicFrontActivation.PresentBoundaries` |
| (c) copiar un origen a una seleccion | PARCIAL (presets, un destino) | `SourceConfigurationCopy`, `HasAnyPersonalization` |
| (d) aplicacion atomica con Confirmar y Cancelar | NO | `RackModuleEditSession.Commit`/`Cancel` |

Propio de Push Back y **sin equivalente** en el Dinamico: el dueno de la sesion (`PushBackEditorState`), el
transporte y filtrado de overrides (`PushBackEditorDesignAssembler.cs:327-365`), el aparcado del compuesto y
toda la UI. Cerrar el Dinamico **sin duplicar** esa logica es la decision AM-2 (§18).

## 12. Cabecera independiente, Cantilever, Cama, Larguero y Drive-In

- **Cabecera independiente**: una sola `RackFrameConfiguration` por rack y ningun destino interno. Reutilizar
  una cabecera entre racks (plantillas de usuario, `.rackcad.json`) ya existe y **no** es multidestino;
  aplicarla a otros racks del dibujo seria territorio de ID21. **No aplica.**
- **Cantilever**: sin cabecera. Tiene su propia aplicacion por alcance para **brazos**
  (`CantileverLineApplyScope`, `CantileverStationApplyScope`, auditoria) que no entra en I-53. **No aplica.**
- **Cama** y **Larguero**: sin cabecera. **No aplican.**
- **Drive-In**: no existe (§7). **No aplica**; si un dia existe con marcos, heredaria el contrato que fije G2.

## 13. Infraestructura compartida de cabecera

### 13.1 `RackFrameConfiguration` (Domain)

POCO puro (`D/RackFrames/RackFrameConfiguration.cs:5-65`, auditoria) con tres clases de campos: persistidos
(alto, fondo, peralte, parametros de troquel, postes, placas, horizontales y paneles), **derivados** (`Members`,
`:63`, reconstruidos por `BracingPanelMemberBuilder.RefreshPhysicalModel`) y **runtime** (`Exceptions`, `:64`,
no persistidas). Sin UI, sin `ObjectId` y sin `Database`.

### 13.2 Clones

| Via | Donde | Valida uso | Reconstruye `Members` | `Exceptions` |
|---|---|---|---|---|
| **`RackFrameProjectStore.DeepCopy`** (canonica, I-17) | `A/Persistence/RackFrameProjectStore.cs:87-104` | SI (lanza si `!IsUsableHeader`, `:62-65`) | SI (`:67`) | las re-anexa (`:98-101`) |
| `DynamicRackSystemResolver.CloneHeader` | `DynamicRackSystemResolver.cs:512-516` | NO | **NO** | las pierde |
| `RackFrameProjectDocument.ToConfiguration` | `RackFrameProjectDocument.cs:81-120` | NO | **NO** | — |
| referencia compartida (auditoria) | estado/diseno/sistema del Selectivo; respaldo de Push Back (`:1837`); `EditHeader_Click` del Dinamico; `PushBackMirror` | — | — | — |

HECHO: en `src/` hay 12 llamadas a `RefreshPhysicalModel` (mas su definicion). Ninguna recorre
`HeaderLineOverrides[*].Header`: `SystemRegistry.Default.cs:184` y `DynamicRackSystemBuilder.cs:147/:192`
recorren solo **modulos**, y solo `DeepCopy` refresca un override por linea.

### 13.3 Configurador compartido

HECHO `U/RackFrames/RackFrameConfiguratorViewModel.cs`: `Configuration { get; private set; }` (`:158`) se
**reemplaza** por un clon en `RestoreStandardConfiguration` (`:1185`), en `ApplySimpleConfiguration` via
`ReplaceConfigurationAndReload` (`:1215` → `:1389`) y en `LoadProjectFrom` (`:1376` → `:1389`). La ventana
expone `Configuration => ViewModel.Configuration` (`RackFrameConfiguratorWindow.xaml.cs:76`). No tiene Aceptar,
Cancelar ni `DialogResult` (decision 5 del Owner en I-35; guarda `Fact7`).

| Llamador | Que lee al volver |
|---|---|
| Cabecera independiente (`RackCabeceraCommands`, `EditorModules`) | `window.Configuration` (auditoria) |
| Selectivo, `CustomizePost_Click` | `window.Configuration` (HECHO `:1205`) |
| Push Back, `ShowHeaderConfigurator` | `window.Configuration ?? copy` (HECHO `:2357`) |
| **Dinamico, `EditHeader_Click`** | **la instancia entregada** (HECHO `:684`/`:694`) → L-1 |

### 13.4 Sesion de modulos y otros patrones reutilizables

- `RackModuleEditSession` y `RackModuleReconciliation` (`A/Systems/Shared/`) se declaran neutrales, estan
  atados al modelo dinamico y **solo** los consume Push Back. `RackModuleHeaderScope`
  (`RackModuleHeaderApply.cs:19`) no tiene consumidor (HECHO, `git grep`).
- Destinos existentes: `SelectiveFondoTargets`; `SelectiveTargetResolver`/`SelectiveTargetPlan` (celdas;
  omiten); `DynamicRackCellScopeResolver` (celdas; **clampea**, y por eso I-43 lo rechazo para el Selectivo);
  `SelectionMatrixModel` (seguridad); y las listas de la ventana de Push Back.
- Aplicacion atomica existente: la sesion de modulos (validar, copiar, Commit o Cancel); `RackDuplicationPlan`
  PREPARE → MUTATE (I-51); `MutationPlan`/`ProjectVariableMutationExecutor` (I-47); el protocolo de dos fases
  de campos pendientes (INV-16); y `RecomputeGate` (I-15).
- **No existe** ningun tipo `*HeaderConfiguration*Snapshot*` ni ningun tipo de destino de cabecera en
  Application.

## 14. Propuestas para G2 (evaluadas, **sin implementar**)

### 14.1 Origen compartido: `HeaderConfigurationSnapshot`

**Evaluacion.** Una instantanea neutral es **viable y util**, y cabe en Application sin UI, `ObjectId` ni
`Database`. La observacion decisiva es que `DeepCopy` **ya es** «capturar y materializar» en una sola llamada:

```text
DeepCopy(c) = Deserialize(Serialize(c)) + re-anexar Exceptions
            = materializar( capturar(c) )
```

Propuesta, como insumo y no como decision:

```text
HeaderConfigurationSnapshot                   transitoria, INMUTABLE, nunca persistida
  Recipe     = forma serializada de RackFrameProjectDocument (la misma que usa DeepCopy)
  Exceptions = copia de las excepciones runtime (misma politica que DeepCopy)
  Source     = procedencia para el informe (sistema, direccion, "configurador" o "copiada de")
Capture(configuration)  → valida IsUsableHeader AL CAPTURAR: falla cerrado antes de tocar ningun destino
Materialize()           → instancia NUEVA con Members reconstruidos (Deserialize) + Exceptions
```

- **Se captura `window.Configuration`, nunca la instancia entregada al configurador.** En el Dinamico eso
  cierra L-1 por construccion.
- **Elegibilidad de «copiar de»**: solo cabeceras personalizadas, que es el precedente vinculante de I-40
  (opcion A).
- **Que viaja y que no**: viaja la receta. La **profundidad** la impone el destino (Selectivo:
  `ImposeFondoDepth`; Dinamico: la `Length` del modulo) y el **peralte** es del rack o del poste. La
  **altura** es la pregunta abierta OD-3: Push Back la hace viajar (PBH-01) y el Selectivo la sobrescribe con
  la resuelta al sembrar (`RackSelectiveWindow.xaml.cs:1191`).
- **Alternativa A**, sin tipo nuevo: disciplina sobre `RackFrameConfiguration` + `DeepCopy`, con guardas de
  «propiedad de la operacion». Es mas barata, pero no impide los alias (hoy hay cuatro, §13.2).
  **Alternativa B**, tipo nuevo: la inmutabilidad la da la construccion. Elegir es AM-1.

### 14.2 Conjunto de destinos

**Evaluacion.** Un tipo **unico** de destino para todos los sistemas **no** es viable sin inventar ejes: el
Selectivo direcciona `(FondoIndex, PostIndex)`, y el Dinamico y Push Back `(PostIndex, ModuleId)` o `ModuleId`.
Lo compartible es la **forma**, como ya decidieron I-40 (`I-40-cabeceras-push-back.md:99-104`) e I-43 al
rechazar `DynamicRackCellScopeResolver`.

```text
Destinos  = producto de DOS ejes independientes del sistema, resuelto por una funcion PURA antes de mutar
  Selectivo : TargetFondos (existe) × postes (NUEVO: este / seleccionados / todos de la reticula maestra)
  Dinamico  : cabeceras (ModuleId IsHeader) × lineas (PresentBoundaries)    si OD-2 = instancia fisica
              o solo cabeceras (ModuleId)                                   si OD-2 = modulo
Resultado = Applied (orden determinista) + Omitted (con motivo), o Rejected (con motivo; nada cambia)
Reglas    = omitir, nunca crear ni clampear; transitorio, nunca persistido
```

- **Politica de cobertura** (AM-4, OD-4): Push Back escenifica como inertes los pares sin cobertura (I-40, no
  se reabre) y el Selectivo omite e informa. Para el Dinamico se recomienda **omitir e informar**, y el tipo de
  resultado debe poder expresar ambas conductas sin cambiar Push Back.
- Push Back **conserva** `ApplyHeaderConfigurationToInstances` tal cual. Adoptar un tipo nuevo solo seria
  aceptable con equivalencia byte a byte probada, y **no** hace falta para cerrar I-53.

### 14.3 Semantica de copia

| # | Invariante propuesto | Situacion hoy |
|---|---|---|
| I1 | Cada destino recibe **su** instancia materializada; dos destinos nunca comparten una | Push Back SI; Selectivo SI si el delegado no es la identidad; Dinamico NO |
| I2 | El origen nunca queda aliado con un destino | Push Back: respaldo por referencia (`:1837`, `:2360-2363`); Dinamico NO |
| I3 | Editar un destino despues no alcanza al origen ni a los demas | pruebas PBH03_*; `ApplyingToSeveralFondos_GivesEachAnIndependentCopy_NeverTheSameInstance` |
| I4 | Toda configuracion que cruza una frontera (carga, resolve, snapshot) llega con `Members` reconstruidos | **violado** para overrides por linea fuera del editor (L-2) |
| I5 | `Exceptions` siguen la politica de `DeepCopy` | `CloneHeader` las pierde (`TC/PushBackModuleEditorCharacterizationTests.cs`, `Fact6`) |
| I6 | La normalizacion del destino (profundidad, peralte) ocurre **despues** de materializar y refresca el modelo si cambia geometria | Selectivo SI (`ImposeFondoDepth`); overrides por linea de Push Back NO |

I4 e I5 tocan el resolver compartido: corregirlos cambia el comportamiento de Push Back, y por eso **no**
entran en I-53 sin decision (OD-5).

### 14.4 Atomicidad

Protocolo propuesto, el mismo patron de I-51 e I-47:

```text
PREPARE   (puro; puede fallar)   capturar y validar el origen → resolver destinos → materializar TODAS las copias
                                 → normalizar cada una → revisiones (altura en el Selectivo) → informe
CONFIRM   (UI; opcional)         un solo aviso consolidado ANTES de mutar; cancelar = mutacion cero
MUTATE    (no puede fallar)      asignar lo preparado; escalares asociados (PostPeraltes) solo si Applied no esta vacio
RECOMPUTE                        uno por operacion (DeferRecompute), o escenificado y uno al Confirmar (sesion)
```

- **Selectivo**: sacar `DeepCopy` + `ImposeFondoDepth` del bucle de `ApplyCabeceraToTargets` a PREPARE, y
  mover la escritura del peralte a MUTATE (cierra L-7).
- **Dinamico**: adoptar `RackModuleEditSession`, que ya es compartida, atomica y probada, en vez de mutar el
  modelo vivo.
- **Por lotes**: un lote es una operacion del editor sobre N destinos de **un** rack. Aplicar a otros racks del
  dibujo es ID21 y queda fuera. La atomicidad del redibujo en AutoCAD no cambia: I-53 no toca adaptadores del
  Plugin.

## 15. Sistemas a cerrar

| Sistema | Que cerrar | Archivos previsibles (sin DTO nuevo) |
|---|---|---|
| **Selectivo** | ID6 («copiar de» y re-aplicar sin reabrir); ID7 eje de postes × `TargetFondos`; atomicidad estricta (L-7); regla de la altura (OD-3) | `SelectiveEditorState.cs`, `RackSelectiveWindow.xaml/.cs` (caliente) y pruebas Core y UI. `SelectivePalletDesign.cs` **no** deberia cambiar |
| **Dinamico** | C1 frontera del configurador (L-1); sesion con Confirmar y Cancelar; ID6 e ID7 segun OD-2; adaptacion en la reconstruccion (reconciliacion `ModuleId+Kind` y transporte de overrides) sin duplicar el ensamblador de Push Back | `RackDynamicSystemWindow.xaml/.cs` (caliente), `DynamicEditorDesignAssembler.cs` y, si AM-2 lo decide, una extraccion a `Systems/Shared` |
| Push Back | **Nada** (YA ENTREGADO). L-2 y las discrepancias de §9.10 van por su propio camino (OD-5) | — |
| Cabecera independiente, Cantilever, Cama, Larguero, Drive-In | **No aplica** | — |

Tamano: **L** si se cierran los dos juntos. Recomendacion: **partir** en G2 (por ejemplo I-53A Selectivo e
I-53B Dinamico) sobre un nucleo puro comun previo, conforme a WORKFLOW §2 («Si crece mas, se parte»). Es la
decision OD-6.

## 16. Conflicto con I-50 (y las demas paralelas)

### 16.1 Hoy (G0 y G1)

**Ninguna colision productiva**: I-53 no ha escrito codigo. Hay colisiones **documentales** triviales: la fila
de ROADMAP (I-50 la inserto tras I-48; I-52, I-53 e I-54 tras I-51) y el final de `ideas-futuras.md` (I-50 le
anade 70 lineas).

### 16.2 En implementacion

HECHO `git diff a4d88f1 origin/feature/cotas-independientes-por-vista`: 46 archivos, 5.902 inserciones. Plan de
gates de I-50 en `I-50-proposal-v1.2.md:443-454` y `:628-630` (en `a4806ff`).

| Archivo que I-53 tocaria | I-50 hoy | I-50 despues | Clase |
|---|---|---|---|
| `U/Systems/Selective/RackSelectiveWindow.xaml/.cs` | — | **G3** (C-01, `BuildDesign`/`LoadDesign`) | **MATERIAL** (caliente) |
| `U/Systems/Dynamic/RackDynamicSystemWindow.xaml/.cs` | — | **G3** (C-07, `ReadAnnotationOptions`/`RestoreFrom`) | **MATERIAL** (caliente) |
| Censos de `x:Name`: `TU/DynamicShellMigrationTests.cs`, `TU/SelectiveShellMigrationTests.cs`, `TC/PushBackModuleEditorCharacterizationTests.cs` | — | **G3** (T-20) | **MATERIAL** si I-53 anade controles |
| Firmas de dibujo de I-24: `TU/DynamicEditorWindowTests.cs`, `TU/SelectiveEditorWindowTests.cs` | — | **G3** (T-21) | posible |
| `A/Systems/Selective/SelectiveEditorState.cs` | +1 linea (`:1315`, `DimensionViews` al construir el diseno) | — | textual, no semantica |
| `A/Systems/Dynamic/DynamicEditorDesignAssembler.cs` | +1 (`:172`) | — | textual, no semantica |
| `A/Systems/Dynamic/DynamicRackSystemResolver.cs` | +2 (`:242`, `:356`) | — | textual; solo si OD-5 toca `CloneHeader` |
| DTO y dominio: `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs`, `SelectivePalletDesign.cs`, `DynamicRackDesign.cs`, `DynamicRackSystem.cs` | `DimensionViews` aditivo | — | no previsto para I-53 (sin DTO nuevo) |
| `A/Systems/PushBack/PushBackCompositeStructure.cs` | — | **G6** (C-15, `CopySharedStructuralIntent`) | no previsto (Push Back no se reabre) |

**Veredicto.** Los hunks actuales de I-50 son la propagacion aditiva de `DimensionViews` junto a
`Dimensions`/`DimensionStyle` y **no** tocan campos de cabecera. El cruce material es con el **G3 pendiente**
de I-50, que abre las dos ventanas calientes que I-53 necesita y sus censos de controles; WORKFLOW §7 manda
**un solo agente a la vez por archivo caliente**.

Recomendacion para G2: `conflicts_with: [I-50]` para los gates de UI. Ningun gate de UI de I-53 arranca
mientras I-50 tenga su G3 sin integrar, **salvo** una excepcion explicita del Owner del tipo
`OWNER_OVERRIDE_I49_I50_PARALLEL`, con la condicion de detenerse antes de editar (OD-7). El nucleo puro no
colisiona.

### 16.3 I-52 (RACKMIRROR)

Su contrato incluye las «cabeceras asimetricas» entre las propiedades sensibles al espejo. Todo dato de
cabecera direccionado por indice (`PostCabeceras`, `ExtraFondoPostCabeceras`, `HeaderLineOverrides.PostIndex`,
ids `B:`) debe espejarse. **Si I-53 no anade datos persistidos direccionados por indice** —y la propuesta de
§14 no los anade—, I-52 no se ve afectada. Vigilar, sin `conflicts_with`.

### 16.4 I-54 (Custom Properties) e I-49

- **I-54**: metadatos de proyecto y de rack en el sobre y en el NOD; cruce bajo (quiza UI en editores).
  Vigilar.
- **I-49**: expresiones, Project Variables y `LinkedPropertyEditor`, que I-53 no toca. Si I-49 llega a
  implementacion compartira `RackSelectiveWindow`, con la misma regla de archivo caliente.

## 17. Gates refinados (propuesta; los fija G2)

| Gate | Contenido | Evidencia de cierre | Bloqueos |
|---|---|---|---|
| **G2** | Contrato vinculante: OD-1..OD-7, AM-1..AM-4, decision de ADR, particion y `conflicts_with` | docs + revision de Arquitecto + decisiones del Owner versionadas | — |
| **G3** | Nucleo puro compartido: instantanea (si AM-1 = B), tipo de resultado y resolvers de destinos por sistema; caracterizacion RED y **guardas de invariancia de Push Back** (las pruebas de §9.9 sin tocar) | Core; CI de `push` | — |
| **G4** | Selectivo, estado: eje de postes, «copiar de» y PREPARE → MUTATE en `SelectiveEditorState` | Core; CI | — |
| **G5** | Selectivo, UI: selectores y acciones en `RackSelectiveWindow`; censo de controles actualizado, no relajado | Core + UI (LC-UI); CI | **G3 de I-50** (OD-7) |
| **G6** | Dinamico, estado: sesion de modulos, reconciliacion y transporte de overrides (AM-2), frontera del configurador | Core; CI | — |
| **G7** | Dinamico, UI: Configurar, Tomar como origen, destinos, Confirmar y Cancelar en `RackDynamicSystemWindow` | Core + UI; CI | **G3 de I-50** (OD-7) |
| **G8** | Candidato por SHA exacto | Core Full y UI Full locales, Debug de UI y de Plugin, CI de `push` 4/4 | — |
| **G9** | Owner Validation en AutoCAD 2025 por sistema: personalizar, copiar de, aplicar a un conjunto, cancelar, Actualizar, RACKEDITAR, guardar y reabrir, **BOM del editor frente a RACKBOMTOTAL**, lateral, frontal y planta | veredicto del Owner | G8 |
| **G10** | Cierre e integracion (WORKFLOW §4.5) | CI del merge + cobertura | G9 |

Si OD-6 parte la iniciativa, G4–G5 y G6–G7 pasan a ser las subiniciativas, cada una con su candidato, su
validacion y su integracion.

## 18. Arquitecto y decisiones del Owner

**¿Hace falta Arquitecto? SI, antes de cerrar G2.** Hay decisiones arquitectonicas materiales:

| Id | Decision |
|---|---|
| **AM-1** | Contrato compartido entre sistemas: tipo inmutable `HeaderConfigurationSnapshot` (B) o disciplina sobre `RackFrameConfiguration` + `DeepCopy` (A); donde vive (`Systems/Shared`) y si merece ADR |
| **AM-2** | Como cierra el Dinamico sin duplicar Push Back: adoptar `RackModuleEditSession` y extraer a `Systems/Shared` el transporte de overrides que hoy vive en `PushBackEditorDesignAssembler`, **sin cambiar el comportamiento de Push Back**; y su interaccion con la secuencia unica de modulos de I-35 |
| **AM-3** | Invariantes de frontera I4 e I5 (`CloneHeader` frente a `DeepCopy` en el resolver compartido): corregirlos cambia Push Back y su BOM. Decidir si es I-53, un `fix/` propio o nada |
| **AM-4** | Politica comun de cobertura y omision (Push Back escenifica inertes; el Selectivo omite e informa) y el tipo de resultado que la exprese |

**Decisiones del Owner** (`requires_owner_decision` deberia pasar a `true` en G2):

| Id | Pregunta |
|---|---|
| **OD-1** | Texto de ID6 e ID7: confirmar o corregir H-ID |
| **OD-2** | Unidad de edicion del Dinamico: el modulo longitudinal (hoy) o la instancia fisica `(PostIndex, ModuleId)`, como Push Back |
| **OD-3** | Selectivo: eje de postes (este / seleccionados / todos) y si la **altura** viaja con la receta o se sobrescribe con la resuelta |
| **OD-4** | Cobertura en el Dinamico: omitir e informar (recomendado) o escenificar inertes como Push Back |
| **OD-5** | Destino de L-1 (dentro de I-53 Dinamico o un `fix/` previo) y de L-2 (`fix/` independiente recomendado, con prueba RED) |
| **OD-6** | Partir I-53 en subiniciativas |
| **OD-7** | Serializar con el G3 de I-50 o conceder una excepcion explicita de paralelismo |

**ADR**: probable si AM-1 fija un contrato transversal; lo decide G2 con el Arquitecto. No se numera aqui.

## 19. Hallazgos laterales (no corregidos)

Se registraran en `ideas-futuras.md` al fijar el contrato (G2), como hizo I-51. **Ninguno se arregla en I-53
sin decision.**

| Id | Hallazgo | Evidencia | Clase |
|---|---|---|---|
| **L-1** | **Dinamico: el configurador devuelve una instancia obsoleta.** `EditHeader_Click` compara y conserva la instancia que entrego, pero el ViewModel la reemplaza en «Aplicar» de la configuracion rapida (el modo en que siempre arranca), en «Restaurar estandar» y en «Abrir»: esas ediciones se pierden como «Cabecera sin cambios.» o quedan a medias. Es el defecto PBH-01 que I-40 corrigio **solo** en Push Back (I-40 dejo fuera el Dinamico, `:118`, y declaro la limitacion de la ventana compartida en `:177-181`) | `RackDynamicSystemWindow.xaml.cs:684-699`; `RackFrameConfiguratorViewModel.cs:1185`, `:1215`, `:1376`, `:1389`; el mismo mecanismo medido por I-40 sobre la ventana real de Push Back (132 → 156, `ReferenceEquals` = False, contrato `:59-62`) | HECHO en codigo; no reproducido en el Dinamico |
| **L-2** | **Push Back: `RACKBOMTOTAL` cotizaria sin celosia las cabeceras con override por linea.** `BuildPushBack` refresca solo las cabeceras de modulo; el resolver clona los overrides con `CloneHeader`, sin refresco; `BomBuilder` itera `Members`, vacio, y emite solo postes y placas. El BOM del editor (que si refresca) y el total divergirian. Ninguna prueba cubre esta ruta: la de BOM usa el ensamblador vivo | `PushBackKindHandler.cs:40-50`; `SystemRegistry.Default.cs:172-189`; `DynamicRackSystemResolver.cs:226-238` y `:512-516`; `RackFrameProjectDocument.cs:81-120`; `BomBuilder.cs:74-93`; `TC/PushBackDerivedPostAndLineTests.cs:388-403` | TRAZA; **no ejecutada** |
| L-3 | El comentario del resolver dice «COPIA canonica» y el codigo usa la no canonica | `DynamicRackSystemResolver.cs:212` frente a `:235` | HECHO |
| L-4 | «Ausente cuando esta vacio» es inexacto: las listas vacias se escriben como `null` | `DynamicRackSystemDocument.cs:47-56`; `RackProjectStore.cs:402-410`; `RackEmbedDocument.cs:70-74` | HECHO (opciones) + INFERENCIA (comportamiento por defecto de System.Text.Json) |
| L-5 | HANDOFF afirma que el Dinamico comparte `RackModuleEditSession`, y no la usa | `HANDOFF.md:2096-2098` | HECHO |
| L-6 | Push Back: `SaveLibrary_Click` dice que confirma la sesion y solo recalcula; `Bom_Click` tampoco confirma | `RackPushBackSystemWindow.xaml.cs:3304-3337` | HECHO |
| L-7 | Selectivo: la escritura de cabecera no es todo-o-nada (peralte escrito antes de aplicar y aunque todo se omita; copias dentro del bucle) | `RackSelectiveWindow.xaml.cs:1252-1266`; `SelectiveEditorState.cs:1193-1228` | HECHO (orden); INFERENCIA (que `DeepCopy` lance a mitad, improbable) |
| L-8 | I-40 prometio informar omisiones en la ronda 2 y su modelo final no lo hace | §9.10, punto 1 | HECHO |
| L-9 | Comentarios del Selectivo que siguen hablando de la «representacion master del fondo 0» en la frontal | prueba vigente `TC/SelectiveFondoCabeceraTests.cs:680`; comentarios en `SelectiveEditorState.cs:1304-1305`, `SelectiveBomBuilder.cs:422-423`, `RackSelectiveWindow.xaml:170` y `SelectivePalletDesign.cs:41-42` segun la auditoria | HECHO (prueba); auditoria (comentarios) |
| L-10 | WORKFLOW §7 desactualizado: `RackSelectiveWindow.xaml.cs` tiene **3.159** lineas (dice 2.867) y el patron `*Commands*.cs` del Plugin da **16** archivos (dice 14) | conteo directo | HECHO |
| L-11 | `docs/ideas-futuras.md:813` conserva una linea de marcador de conflicto diff3 (siete barras verticales y `085ca2f`), introducida por el merge `d582dee` (I-43 incorporando `main`) | `git blame -L 813,813` | HECHO |
| L-12 | `RackModuleHeaderScope` no tiene consumidor: solo aparece citado en comentarios | `RackModuleHeaderApply.cs:19`; `git grep` | HECHO |
| L-13 | `RackCommandReference` (RACKAYUDA) no lista `RACKPUSHBACK`, `RACKSECCION` ni `RACKVARIABLES` | busqueda sin coincidencias en `U/RackCommandReference.cs` | HECHO |
| L-14 | Push Back: los overrides por linea no se podan por `PostIndex`, sobreviven a un cambio de tipo de cabecera, no adaptan fondo ni peralte y su descarte no se informa. Es comportamiento integrado de I-40: observacion, **no** reapertura | `PushBackEditorDesignAssembler.cs:348-364` | HECHO |

## 20. Lo que no se verifico

- Ningun comportamiento en AutoCAD ni en ejecucion; ninguna suite ni build.
- L-2 en un DWG real: la traza es completa; la ejecucion, no.
- L-1 sobre la ventana del Dinamico: el mecanismo es el mismo que I-40 midio en Push Back.
- Si el compuesto de Push Back ofrece cabeceras `B:` como destinos, y el posible cruce de ids sin prefijo en los
  marcos locales de B.
- La emision literal de las claves `null` en el JSON, inferida de las opciones del store.
- Las citas marcadas «auditoria» (§7, §9.2, §9.3, §9.8, §9.9, §10.1–§10.3, §10.6, §11.1–§11.6, §12, §13.1,
  §13.2 y §13.3) proceden de las auditorias paralelas y no se re-leyeron linea a linea. Ninguna sostiene sola un hallazgo, una
  brecha de la matriz ni una propuesta.
