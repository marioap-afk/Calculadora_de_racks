---
schema: rackcad-initiative/v1
id: I-53D
title: "Cabeceras multidestino: Dinamico UI para ID6 REUSE + ID7 BATCH DISTRIBUTION (E3 de I-53)"
type: feature
status: integrated
branch: feature/cabeceras-multidestino-dinamico
base_branch: main
priority:
size:
depends_on: [I-53, I-53S]
conflicts_with: []
context_packs: [system-dynamic-flowbed, ui-editors, architecture-kernel, persistence]
automation_state_path:
decision_paths: [docs/automation/decisions/I-53.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-53D — Cabeceras multidestino: Dinamico UI (E3 de I-53)

> **Fase actual: E3-I — cierre documental e integracion (2026-09-13).** G0 y G7 **CLOSED**, E3-C **PASS** —tras un primer
> intento **BLOCKED** porque `main` avanzo durante la validacion, y la orden E3-C.1 del Coordinador, que rebaso la rama
> sobre la integracion de I-54— y E3-V **PASS** del Owner sobre el Candidato `a57bd506172bc70e6f415146dae245662baabd28`.
> **ID6/ID7 quedan visibles en el Dinamico** y, con esta integracion, **la linea conceptual I-53 queda completa**.
>
> ```text
> Initiative = I-53D
> Parent     = I-53
> Delivery   = E3
> Gate       = G7
>
> Scope:
> Dinámico UI para ID6 + ID7
>
> Foundation:
> Dynamic Application/state/reconciliation
> ya integrado por I-53 E1
>
> No redefine:
> Snapshot
> Plan/Outcome
> ModuleId
> ModuleTargets
> generation/signature
> reconciliation
> ADR-0037
> OD-2.b
> OD-8
> ```
>
> Este es un **contrato de ejecucion**. El contrato tecnico vinculante es la [Proposal V2](I-53-proposal-v2.md) de
> I-53, **congelada**, junto con el [contrato de I-53](I-53-cabeceras-configurables-multidestino.md), el
> [registro de decisiones](../automation/decisions/I-53.md),
> [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md), **aceptado**, y el
> [HANDOFF](../HANDOFF.md) vigente. Aqui solo se remite a ellos; nada de lo que dicen se repite ni se reinterpreta.

> **Apertura por autorizacion explicita del Owner sin fila previa** — caso (d) de [WORKFLOW](../WORKFLOW.md)
> seccion 2. La autorizacion es **OD-6 = APPROVED (B′)**: I-53 es una iniciativa conceptual entregada en tres unidades
> Git (I-53 / E1 e I-53S / E2, integradas y cerradas, e **I-53D / E3**, la ultima). **OD-8** —retirar los presets
> «Personalizada N» en G7— es igualmente vinculante. La autorizacion sustituye **unicamente** la preexistencia de la
> fila: esta fila y este contrato nacen en el bootstrap inmediatamente posterior al reclamo atomico. Sin Discovery,
> Proposal ni ADR propios (Proposal V2 §9.2).

```text
Initiative   = I-53D (unidad Git E3 de la iniciativa conceptual I-53: ID6 REUSE + ID7 BATCH DISTRIBUTION)
Branch       = feature/cabeceras-multidestino-dinamico
Worktree     = ~/.codex/worktrees/feature-cabeceras-multidestino-dinamico
BASE_SHA     = 104ef3a1b1df249d6e0a56dc4ad3846912b24f12   (origin/main al reclamar: merge de I-53S E2)
CLAIM_SHA    = c5a14047e1f1e6a36c288afe03a264c0a239cefa   (commit vacio)
Claim-Id     = 352cdd81-8efa-4d72-9423-44bed452ae39
Decisiones   = docs/automation/decisions/I-53.md (registro unico de la linea; no se crea uno propio)
```

```text
BASE_REBASE_SHA     = ba497f14581d81e83a27514852d6ec082ff57635   (origin/main en E3-C.1: merge de I-54; sin avance hasta E3-I)
CLAIM_SHA           = 0b091d64252aef611b4a7708aa8702bf556ed904   (el mismo reclamo vacio y el mismo Claim-Id, reescrito por el rebase; original c5a1404)
BOOTSTRAP_SHA       = c5606787272630460965c500e14bc9dc5370bc87   (original aa0f817, CI de push 34790475923, 4/4)
G7_DYNAMIC_UI_SHA   = a57bd506172bc70e6f415146dae245662baabd28   (original d280197, CI de push 34796429664, 4/4)
E3_CANDIDATE_SHA    = a57bd506172bc70e6f415146dae245662baabd28   (= G7 rebasado; CI de push 34800174492, 4/4; sin commit ni rebase posterior)
E3_CLOSURE_SHA      = el commit que registra esta seccion (docs-only; un commit no puede citar su propio SHA)
E3_MERGE_SHA        = PENDING hasta el merge
```

## 1. Objetivo

Llevar a la ventana del Dinamico la capacidad de usuario de **ID6 REUSE + ID7 BATCH DISTRIBUTION** que I-53 E1 dejo en
Application sin llamador y que I-53S ya entrego en el Selectivo: tomar una cabecera de modulo personalizada como
**origen** y aplicarla a un **conjunto de modulos de cabecera destino**, cada uno como **copia independiente**, con
preparacion **todo o nada**, confirmacion cuando haya un aviso severo, **un solo recompute** por operacion confirmada e
informe del resultado. Incluye el **informe de reconciliacion** visible al reconstruir (OD-2.b), **L-1** y la
**retirada de los presets «Personalizada N»** (OD-8). Con su integracion, la linea conceptual I-53 queda completa.

El texto vinculante de ID6/ID7 esta en el [contrato de I-53](I-53-cabeceras-configurables-multidestino.md) §1.

## 2. Problema

Tras E1 y E2 la fundacion del Dinamico esta en `main`, pero **ningun usuario puede usarla**: la ventana sigue con el par
ordinal `SnapshotHeaderFondos` / `RestoreHeaderFondos`, que pierde personalizaciones en silencio al reconstruir; conserva
los presets «Personalizada N» como segundo mecanismo de reutilizacion (H-05); y abre el configurador sobre la instancia
viva sin leer siempre su resultado real (L-1). La evidencia esta en la Proposal V2 §7.

## 3. Alcance

Solo **Dinamico UI**, sobre la fundacion integrada (Proposal V2 §7, §9.1 y §12, fila G7):

- **Source** = direccion `ModuleId` + firma de la peticion, recordada por la UI; **nunca** una configuracion congelada.
  PREPARE captura el valor ACTUAL (§7.4).
- **Destinos**: **Destination = `ModuleId`**, no `(PostIndex, ModuleId)` (§7.1). `ModuleTargets` = `Actual` (la cabecera
  seleccionada) / `Explicito` (un conjunto de `ModuleId`) / `Todas` (todas las cabeceras vigentes); solo en tiempo de
  ejecucion, sin persistir ni recordar, y sin destino universal (§7.5, §7.7).
- **PREPARE** puro; **Warnings** y **CONFIRM**; **MUTATE** con la verificacion de firma antes de escribir; **un**
  recompute; **Outcome** e informe al usuario (§7.8). **EDIT** por el mismo protocolo.
- **Reconciliacion cableada**: la ventana cambia el par ordinal por `DynamicRackRebuild`, muestra su informe, y el par se
  retira (§7.9).
- **L-1** (§7.12) y **retirada de presets** (OD-8, §7.11).
- Pruebas **D-26..D-39** (Proposal V2 §13.4) y censo de la ventana **actualizado, no relajado**.

### 3.1 Semantica congelada que G7 solo cablea

```text
IsManualOverride              = SOLO longitud manual
personalizacion               = UseCalculatedHeaderConfiguration = false
DISTRIBUTE                    → no toca Length, IsManualOverride, IsCalculated ni Kind
PREPARE                       = puro
MUTATE                        = stale-check de la firma antes de escribir
RECOMPUTE (uno)               = ApplyPostPeralte + Refresh
reconstruccion relevante      → incrementa la generacion
recomposicion sin rebuild     → conserva la generacion
origen / Explicito de generacion vieja → Rejected(StaleTargets)
```

No se reinterpreta ni se modifica: lo fijan ADR-0037 y la Proposal V2 §7.3, §7.6 y §7.8.

### 3.2 L-1 (congelado)

Una cabecera ya personalizada abierta para edicion entra al configurador **sobre una COPIA**
(`RackFrameProjectStore.DeepCopy`), **en editor avanzado** (`ViewModel.IsAdvancedEditor = true`) y **leyendo el
resultado real al cerrar** (`window.Configuration`), con un seam de presentacion al estilo de Push Back
(`Action<RackFrameConfiguratorWindow>`). `RackFrameConfiguratorWindow` **no** se toca por dentro. Pruebas RED vistas
fallar: D-28..D-31.

### 3.3 Retirada de presets (OD-8, cerrada)

Los presets «Personalizada N» se retiran en G7 y **no** se conserva un segundo mecanismo de reutilizacion: el unico modelo
pasa a ser `Source existente → Destinations → copias independientes`. Se conserva el restablecimiento por modulo a
«Calculada», que no es un preset. G0 no toca los presets. Pruebas: D-26 y D-27.

### 3.4 Informe de reconciliacion (OD-2.b)

OD-2.b ya esta implementada en Application por E1. G7 la cablea y hace visible el resultado —Preserved, Adapted, Removed,
Incompatible y Restored, con `Describe` y `LostAnything`—. **No** se crea otra reconciliacion y **no** se cambia
`RackModuleReconciliation`. Pruebas: D-33 y D-39.

## 4. Fuera de alcance

El Selectivo (I-53S, integrada); Push Back; `Application/Systems/Shared`; cambios del contrato de Application del
Dinamico salvo defecto real (seccion 12); `RackModuleReconciliation`, `RackModuleEditSession` y `RackModuleDescriptor`;
persistencia nueva, DTO y `SchemaVersion`; plantillas, portapapeles o biblioteca de configuraciones; Project Variables;
Expression Engine; overrides por linea y L-2. Tampoco el Plugin, `assets/`, `deploy/`, `.github/` ni la lista de la
Proposal V2 §14 «No se tocan en ninguna unidad». La disposicion WPF, los botones, el selector, los dialogos y la
presentacion del informe se deciden en G7 **sin ADR**: ADR-0037 excluye XAML, controles y presets.

## 5. Contexto requerido

- Autoridades: [Proposal V2](I-53-proposal-v2.md) (§3, §7, §9, §12-§14, §16-§18),
  [contrato de I-53](I-53-cabeceras-configurables-multidestino.md) (§12 y §14.5),
  [decisions/I-53.md](../automation/decisions/I-53.md) (secciones 5, 10, 11 y 12),
  [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md) y [HANDOFF](../HANDOFF.md)
  vigente (entradas de I-53 E1 e I-53S E2).
- Precedentes del seam del configurador: I-40 en Push Back e I-53S en el Selectivo.
- [ADR-0029](../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md) (ventanas WPF; el Dinamico sin ambito sucio),
  [WORKFLOW](../WORKFLOW.md) §4.5 y §7 y [AGENTS.md](../../AGENTS.md).
- Solo lectura: `Application/Systems/Dynamic/**` (la fundacion de G6), `DynamicRackSystemResolver`,
  `DynamicEditorDesignAssembler`, `RackModuleReconciliation`, `DynamicRackModule` y el nucleo de G3.

## 6. Dependencias y coordinacion

- **I-53 E1** e **I-53S E2**, integradas y cerradas en `main` con sus compuertas posteriores: la fundacion que G7 cablea y
  el precedente de UI en el Selectivo.
- **Auditoria de archivos calientes de G0** sobre `RackDynamicSystemWindow.xaml(.cs)`, contra `origin/main` y todas las
  ramas activas:

  ```text
  CURRENT_DYNAMIC_UI_OVERLAP_I49   = NO   produccion solo en Application/Expressions y pruebas Core
  PLANNED_DYNAMIC_UI_COLLISION_I49 = NO   su Proposal V6 declara «Ninguno» para esta ventana (P30.15)
  CURRENT_DYNAMIC_UI_OVERLAP_I52   = NO   solo documentacion
  PLANNED_DYNAMIC_UI_COLLISION_I52 = NO   su Proposal V7 excluye los editores WPF y toma la fundacion Dinamica de I-53 como baseline
  CURRENT_DYNAMIC_UI_OVERLAP_I54   = NO   produccion en Application/CustomProperties, Persistence y dos archivos del Plugin
  PLANNED_DYNAMIC_UI_COLLISION_I54 = NO   su Proposal V2 declara que I-54 no toca editores
  ```

  Antes de editar la ventana en G7 se repite este preflight contra el `main` y las ramas vigentes.

  **Preflights de G7, E3-C, E3-C.1 y E3-I**: ninguna rama activa toco `RackDynamicSystemWindow.xaml(.cs)` ni
  `DynamicEditorDesignAssembler.cs`. I-54 se integro durante E3-C (merge `ba497f1`) sin archivos productivos comunes: su
  unico cruce fue documental —las filas de I-54 e I-53D, insertadas ambas tras la de I-53S en `docs/ROADMAP.md`—, resuelto
  en el rebase de E3-C.1 conservando las dos. **I-55** (`feature/creacion-de-vistas`) aparecio al cerrar E3-C.1 con solo
  reclamo y bootstrap, sin archivos productivos; su contrato deja para su G1 medir el cruce con la ventana del Dinamico.
- **Cruce documental previsto**: filas de `docs/ROADMAP.md`, y en `docs/ideas-futuras.md` los follow-ups N-02 y N-03 del
  control de configuracion del Dinamico, cuya forma decide G7 al retirar los presets (sin corregirlos «de paso»). Se
  resuelve al integrar, conservando todo.
- **Entrada del Owner**: ninguna pendiente (`requires_owner_decision: false`).

## 7. Archivos esperados

| Gate | Crear | Modificar | Solo leer |
|---|---|---|---|
| G0 | este contrato | `docs/ROADMAP.md` (su fila, momento 2) | contrato de I-53, Proposal V2, ADR-0037, decisions/I-53.md |
| G7 | pruebas nuevas en `tests/RackCad.UI.Tests/` | `src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml` y `.xaml.cs`; `DynamicShellMigrationTests.cs` y `DynamicEditorWindowTests.cs` si cambian firmas o el censo; `DynamicEditorDesignAssembler.cs` (retirada del par ordinal); `DynamicEditorDesignAssemblerTests.cs` y las aserciones del Dinamico de `PushBackModuleEditorCharacterizationTests.cs`, reapuntadas sin relajar | `RackFrameConfiguratorWindow.xaml.cs`, `RackFrameConfiguratorViewModel.cs`, `RackModuleReconciliation.cs` y la fundacion de `Application/Systems/Dynamic` y `Shared` |
| E3-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (su fila y la de I-53 como linea completa, momento 3) y este contrato | — |

Tabla de la Proposal V2 §14. Una desviacion material de esta tabla obliga a detenerse.

En E3-I la orden del Coordinador anade dos documentos que esta tabla no preveia: `docs/automation/decisions/I-53.md`
(seccion 13, registro unico de la linea) y `docs/ideas-futuras.md` (N-03 resuelto, N-02 vigente, la nota de PB-011 sobre
el par ordinal y los hallazgos **N-05** y **N-06**, que WORKFLOW §5 y §8 mandan registrar ahi). Siguen siendo solo
documentacion.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | reclamo atomico, este contrato y la fila en ROADMAP | **CLOSED** |
| G7 | Dinamico UI + L-1 + retirada de presets + informe de reconciliacion | ventana sobre Plan/Outcome y `DynamicRackRebuild`; D-26..D-39 | **CLOSED** |
| E3-C | Candidato | seccion 9 | **PASS** (via E3-C.1: el primer intento quedo BLOCKED porque `main` avanzo; seccion 14.3) |
| E3-V | Owner Validation del Dinamico | seccion 10 | **PASS** |
| E3-I | Integracion y cierre de la linea | WORKFLOW §4.5 completo; limpieza solo tras las coberturas del `MERGE_SHA` y del Candidato | en curso al registrarlo: cierre documental hecho; merge, CI posterior, coberturas y limpieza pendientes |

Los gates estan congelados en la Proposal V2 §12. Ninguno arranca sin evidencia revisable del anterior y sin orden del
Coordinador.

## 9. Pruebas y builds

- **G7**: RED **por asercion** de comportamiento sobre codigo compilable (nunca por compilacion ni por excepcion) →
  verde, con D-26..D-39 (Proposal V2 §13.4); D-28..D-31 **vistos fallar** antes del arreglo. `Fact5` (x2) y
  `DynamicEditorDesignAssemblerTests` se reapuntan a la reconciliacion **sin relajarse**, y `Fact7` solo si su literal
  deja de existir (D-39). Todo dialogo pasa por seam: ninguna prueba abre un modal real (D-34). Siguen verdes las suites
  del Dinamico, las de Push Back (I-35, I-40) y las firmas de dibujo de I-24. Core y UI en local, y CI de `push` sobre el
  SHA del gate.
- **E3-C (Candidato)**: Core Full local, **UI Full local**, build Debug de UI y de Plugin, y CI de `push` 4/4 sobre el
  SHA exacto (AGENTS, «Pruebas — definicion de terminado»; Proposal V2 §9.4). LC-UI no reduce el Candidato.
- Una seleccion de pruebas que no selecciona nada es FALLO. Los conteos viven solo en HANDOFF.

## 10. Validacion manual

**E3-V**: Owner Validation en AutoCAD 2025 sobre el DLL Debug del SHA exacto del Candidato, con el checklist de la
Proposal V2 §13.8:

- L-1 con «Aplicar» rapido y reapertura avanzada.
- Ausencia de «Personalizada N».
- Tomar como origen; aplicar a una, a varias y a todas.
- Omision por frentes en blanco.
- Independencia de las copias.
- Cambio de tarima con informe: personalizada adaptada y longitud manual de cabecera y de separador.
- Invalidacion informada del origen y de los destinos tras reconstruir.
- «Restaurar estandar» y «Calculada».
- Actualizar, `RACKEDITAR`, guardar y reabrir, y vistas.
- BOM del editor frente a `RACKBOMTOTAL`.

**Ronda E3-V (2026-09-13).** Sobre el Candidato ya rebasado, con el checklist de arriba. El canal del Coordinador transmite
el veredicto sin la redaccion literal del Owner ni el detalle por escenario.

| Campo | Valor |
|---|---|
| Candidato | `a57bd506172bc70e6f415146dae245662baabd28` |
| DLL | `~/.codex/worktrees/feature-cabeceras-multidestino-dinamico/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll`, `InformationalVersion 1.0.0+a57bd506172bc70e6f415146dae245662baabd28`, SHA-256 `703B86BE0C0459F4FFA8131297151F19A4592375676B2265274EB8F0BA5A1DA3` |
| Al recibir el veredicto | el DLL del worktree conserva ese `InformationalVersion` y ese SHA-256 |
| Biblioteca de bloques y build de AutoCAD | no constan en la transmision del Coordinador, y no se afirman |

Veredicto (2026-09-13): E3-V = PASS — Owner, sobre el SHA exacto del Candidato

## 11. Criterios de aceptacion

1. En el Dinamico, una cabecera de modulo personalizada existente se aplica como origen a un conjunto `ModuleTargets`
   elegido por el usuario.
2. Cada destino recibe una **copia independiente**, y DISTRIBUTE no toca `Length`, `IsManualOverride`, `IsCalculated` ni
   `Kind`.
3. **Todo o nada**: un destino aplicable invalido rechaza el lote con mutacion cero; lo no aplicable se omite y se informa.
4. **Un** recompute por operacion confirmada y cero en `Rejected` y `Cancelled`; una peticion de generacion vieja es
   `StaleTargets`.
5. Toda reconstruccion pasa por `DynamicRackRebuild` y su informe es visible: nada se pierde en silencio.
6. L-1 cumplido y sin presets «Personalizada N».
7. El resultado sobrevive a Actualizar, `RACKEDITAR` y guardar y reabrir; BOM y dibujo leen la misma autoridad; el
   Selectivo y Push Back no cambian.
8. D-26..D-39 verdes, censo actualizado sin relajar y E3-V **PASS**. Con E3-I quedan cumplidos los criterios de la linea
   del contrato de I-53 §11.

## 12. Condiciones para detenerse

- **G7 no se abre** sin orden del Coordinador.
- Una **decision material** nueva, o un cambio del contrato duradero (Proposal V2, ADR-0037, OD-2.b, OD-8 o las
  decisiones de I-53): detenerse y volver al Coordinador, y al Arquitecto si es arquitectonico.
- Necesidad de cambiar `Application/Systems/Shared`, el contrato de Application del Dinamico (un defecto real tambien se
  reporta antes de corregirlo), `RackModuleReconciliation`, el Plugin, un DTO, la persistencia o `SchemaVersion`:
  detenerse.
- Necesidad de modificar `RackFrameConfiguratorWindow` o `RackFrameConfiguratorViewModel` para L-1: detenerse.
- Deriva hacia el Selectivo, Push Back o los overrides por linea (L-2), o hacia reabrir I-35, I-40, I-53S o ADR-0037:
  detenerse.
- Una rama activa que empiece a tocar `RackDynamicSystemWindow.xaml(.cs)`, o un avance de `main` cuyo rebase toque la
  ventana o la fundacion: reconciliar segun WORKFLOW y, si hay conflicto productivo, detenerse.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-53D.yml`: la automatizacion esta **desactivada** (`automation.enabled: false`), como en I-53
e I-53S. El estado vivo se deriva de la existencia de `origin/feature/cabeceras-multidestino-dinamico`
([WORKFLOW](../WORKFLOW.md) seccion 2). Las decisiones de la linea van a
[`decisions/I-53.md`](../automation/decisions/I-53.md), sin registro propio. No hay Pull Request: la integracion es
manual (WORKFLOW 4.5). El cierre documental de I-53D registra la linea I-53 como completa en la fila de I-53 (Proposal V2
§9.2).

## 14. Evidencia final

### 14.1 G0

Reclamo atomico `c5a1404` y bootstrap `aa0f817` (CI de `push` `34790475923`, 4/4), reescritos por el rebase de E3-C.1
como `0b091d6` y `c560678` sin cambiar contenido, mensajes, autoria ni trailers. Auditoria de archivos calientes sobre la
ventana del Dinamico contra I-49, I-52 e I-54: sin cruce actual ni previsto (seccion 6).

### 14.2 G7 — Dinamico UI

Commit unico `d280197` (CI de `push` `34796429664`, 4/4), rebasado sin cambios de contenido como `a57bd50`. Solo toca
`RackDynamicSystemWindow.xaml(.cs)`, `DynamicEditorDesignAssembler.cs` y pruebas:

- **Disposicion**: panel «Reutilizar cabecera» dentro de «Modulo seleccionado», bajo «Editar cabecera», con el texto del
  origen, «Tomar como origen», «Cabeceras destino» (popup Actual / Todas / una casilla por cabecera) y «Aplicar origen a
  destinos»; el informe de la ultima reconstruccion va a la banda de estado.
- **Origen**: `DynamicHeaderBatchState.RememberSource` por `ModuleId`; no se persiste, y al abrir otro diseno se olvidan
  origen, destinos e informe.
- **Destinos**: `DynamicModuleTargets` (`FollowCurrentModule`, `FollowAllModules`, `SetTargetModules`), solo en tiempo de
  ejecucion.
- **Gesto** `RunHeaderBatch`, unico para EDIT y DISTRIBUTE: PREPARE → aplicacion de Application, con un recompute por
  `Committed` → informe en la banda de estado. **Sin confirmacion**: el plan del Dinamico no produce avisos, asi que
  `Cancelled` no es alcanzable desde esta ventana, y no se anadio una confirmacion artificial.
- **L-1**: `ShowHeaderConfigurator(copy, alreadyCustom)` abre el configurador sobre `RackFrameProjectStore.DeepCopy`, en
  editor avanzado si la cabecera ya es personalizada, con la costura `HeaderConfiguratorPresenter`, y lee
  `window.Configuration` al cerrar; el literal `var beforeEdit` sigue (Fact7 intacto).
- **Retirada de presets (OD-8)**: fuera `headerPresets`, `HeaderPreset` y el alta de «Personalizada N». «Configuracion de
  cabecera» pasa a mostrar la **procedencia** (Calculada / Personalizada, esta no seleccionable), lo que resuelve **N-03**,
  y «Calculada» conserva el restablecimiento historico por modulo (**N-02**, caracterizado en D-26).
- **Reconstruccion**: `RecomposeCore` reconstruye **solo** por `DynamicRackRebuild.Rebuild`, con
  `RestoreStandard = forceRebuild` para «Restaurar layout»; el par ordinal `SnapshotHeaderFondos` / `RestoreHeaderFondos`
  sale de `DynamicEditorDesignAssembler`. `Fact5` (x2) y `DynamicEditorDesignAssemblerTests` se reapuntaron a la
  reconciliacion sin relajarse; `Fact3` gano una asercion aditiva; `Fact1` y `Fact7` no cambiaron.
- **Costuras de prueba**: `HeaderBatchStateForTest`, `LastHeaderBatchOutcome`, `HeaderBatchLog`, `RecomputeCount` y
  `LastRebuildResult`; el censo de la ventana gana ocho `x:Name`, sin relajarse.
- **RED** por asercion en dos pasos —andamiaje compilable sin comportamiento y captura de intencion con el gesto vacio—
  antes del comportamiento; D-28..D-31 vistos fallar.

### 14.3 E3-C — Candidato

**Primer intento, sobre `d280197`: BLOCKED.** Toda la evidencia paso —E3-INV-1..10, D-26..D-39, Fact1/3/5/7, suites de
impacto, Core y UI Full, builds sin incremental con los ensamblados estampados con ese SHA y el CI de `push` del mismo
SHA—, pero el fetch final encontro `origin/main` en `ba497f1`: la integracion de I-54, ocurrida durante la validacion. El
rebase que exige WORKFLOW §4.2 solo chocaba en `docs/ROADMAP.md`, y la orden E3-C no permitia tocar docs: no se declaro
Candidato ni se hizo rebase, push o commit.

**E3-C.1 (opcion A del Coordinador).** Rebase sobre `ba497f1` con `--keep-empty`. El unico conflicto, al reaplicar el
bootstrap, se resolvio conservando ambas filas intactas —orden I-53, I-53S, I-53D, I-54— y se verifico a nivel de blob:
una linea anadida respecto de cada lado, ninguna borrada y sin marcadores. `range-diff`: reclamo identico, bootstrap solo
con contexto y la fila de I-54, G7 identico; mensajes, autoria y trailers byte a byte, con un unico `Claim-Id`; los trece
blobs de G7 y su `patch-id`, identicos. Publicado con `--force-with-lease` fijado a `d280197`, tras comprobar que el remoto
seguia ahi. La evidencia de `d280197` y su CI no se reutilizaron.

```text
Candidate SHA:        a57bd506172bc70e6f415146dae245662baabd28
Arbol limpio:         SI
SDK resuelto:         8.0.423
Core Full local:      PASS
UI Full local:        PASS
Debug UI build:       PASS
Debug Plugin build:   PASS
CI exact SHA:         GREEN (run 34800174492, job ui-tests success)
Owner validation:     pass
```

- Evidencia local ejecutada sobre el SHA exacto, despues del rebase y con reconstruccion sin incremental; los seis
  ensamblados y sus copias quedaron estampados con `1.0.0+a57bd50...`, ninguno con `d280197` ni `aa0f817`.
- **E3-INV-1..10 = PASS** sobre `c560678..a57bd50`: productivo solo en `RackDynamicSystemWindow.xaml(.cs)` y
  `DynamicEditorDesignAssembler.cs`; cero cambios en Shared, Domain, Plugin, Persistence/DTO, Selectivo, Push Back,
  `assets`, `.github` y docs; ensamblador sin el par ordinal; toda reconstruccion por `DynamicRackRebuild`; sin presets;
  origen por `ModuleId` y destinos solo en tiempo de ejecucion; L-1; sin escrituras directas de la ventana en DISTRIBUTE;
  un recompute por `Committed`; un `MessageBox.Show` y cinco `ShowDialog`, igual que antes.
- I-54 no referencia simbolos del Dinamico ni de la reutilizacion de cabeceras, y su ventana nueva vive fuera del espacio
  de nombres que vigila D-34.
- Cobertura **no requerida** para declarar el Candidato (WORKFLOW §4.5 paso 2.bis; AGENTS, «Pruebas», punto 1): se mide
  en E3-I, despues del merge (paso 7).
- Observaciones no corregidas: **N-05** y **N-06** en [ideas-futuras.md](../ideas-futuras.md).

### 14.4 E3-V

Seccion 10: **PASS**.

### 14.5 Cierre de la linea I-53

Con E3 integrada, la linea conceptual queda completa (Proposal V2 §9.2;
[decisions/I-53.md](../automation/decisions/I-53.md) §13): **E1**, la fundacion —nucleo compartido y Application del
Selectivo y del Dinamico—; **E2 = I-53S**, la UI del Selectivo; y **E3 = I-53D**, la UI del Dinamico. Push Back ya lo
entregaba con I-40 y no se reabrio. No queda ninguna unidad de la linea por reclamar. **N-03** queda resuelto; **N-02**,
**N-05** y **N-06** quedan en [ideas-futuras.md](../ideas-futuras.md) sin corregir.

Los conteos de pruebas viven en [HANDOFF](../HANDOFF.md) §5.
