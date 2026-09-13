---
schema: rackcad-initiative/v1
id: I-53S
title: "Cabeceras multidestino: Selectivo UI para ID6 REUSE + ID7 BATCH DISTRIBUTION (E2 de I-53)"
type: feature
status: claimed
branch: feature/cabeceras-multidestino-selectivo
base_branch: main
priority:
size:
depends_on: [I-53]
conflicts_with: [I-49]
context_packs: [system-selective, ui-editors, architecture-kernel]
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

# I-53S — Cabeceras multidestino: Selectivo UI (E2 de I-53)

> **Fase actual: G0 — reclamo y bootstrap (2026-09-13).** Solo documentacion. **G5 no esta abierto**: queda
> bloqueado por la coordinacion de archivo caliente con I-49 hasta que el Coordinador fije el orden (seccion 6).
>
> ```text
> Initiative = I-53S
> Parent     = I-53
> Delivery   = E2
> Gate       = G5
>
> Scope:
> Selectivo UI para ID6 + ID7
>
> Foundation:
> ya integrada en main por I-53 E1
>
> No redefine:
> Snapshot
> Plan/Outcome
> ADR-0037
> TargetFondos
> PostTargets
> RR-01
> N-01
> autoridades Height/Depth/PostPeralte
> ```
>
> Este es un **contrato de ejecucion**. El contrato tecnico vinculante es la [Proposal V2](I-53-proposal-v2.md) de
> I-53, **congelada**, junto con el [contrato de I-53](I-53-cabeceras-configurables-multidestino.md), el
> [registro de decisiones](../automation/decisions/I-53.md) y
> [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md), **aceptado**. Aqui solo
> se remite a ellos; nada de lo que dicen se repite ni se reinterpreta.

> **Apertura por autorizacion explicita del Owner sin fila previa** — caso (d) de [WORKFLOW](../WORKFLOW.md)
> seccion 2. La autorizacion es **OD-6 = APPROVED (B′)**: I-53 es una iniciativa conceptual entregada en tres unidades
> Git (I-53 / E1, **I-53S / E2**, I-53D / E3). Sustituye **unicamente** la preexistencia de la fila: esta fila y este
> contrato nacen en el bootstrap inmediatamente posterior al reclamo atomico. Sin Discovery, Proposal ni ADR propios
> (Proposal V2 §9.2).

```text
Initiative   = I-53S (unidad Git E2 de la iniciativa conceptual I-53: ID6 REUSE + ID7 BATCH DISTRIBUTION)
Branch       = feature/cabeceras-multidestino-selectivo
Worktree     = ~/.codex/worktrees/feature-cabeceras-multidestino-selectivo
BASE_SHA     = 1b091bedafceb67ca57054a9eb3bf5259efff774   (origin/main al reclamar: merge de I-53 E1)
CLAIM_SHA    = fddbffecf67105eb7924f3fa4cc7fdf015def2fc   (commit vacio)
Claim-Id     = 28b64301-c860-4fda-8ad8-4b3771995aae
Decisiones   = docs/automation/decisions/I-53.md (registro unico de la linea; no se crea uno propio)
```

## 1. Objetivo

Llevar a la ventana del Selectivo la capacidad de usuario de **ID6 REUSE + ID7 BATCH DISTRIBUTION** que I-53 E1 dejo
en Application sin llamador: tomar una cabecera personalizada existente como **origen** y aplicarla a un **conjunto de
destinos**, cada uno como **copia independiente**, con preparacion **todo o nada**, confirmacion cuando haya un aviso
severo, **un solo recompute** por operacion confirmada e informe del resultado. Incluye N-01 y el cableado de RR-01 en
el gesto.

El texto vinculante de ID6/ID7 esta en el [contrato de I-53](I-53-cabeceras-configurables-multidestino.md) §1.

## 2. Problema

Tras E1 la fundacion esta en `main`, pero **ningun usuario puede usarla**: la ventana sigue con
`ApplyCabeceraToTargets` y `SelectiveCabeceraHeightReview.Of`. Ademas, «Personalizar» abre el configurador en modo
rapido aunque la cabecera ya este personalizada (N-01, Proposal V2 §6.11), y el EDIT escribe `PostPeraltes` antes de
aplicar (L-7, §6.7). La evidencia esta en la Proposal V2 §3 y §6.

## 3. Alcance

Solo **Selectivo UI**, sobre la fundacion integrada (Proposal V2 §6, §9.1 y §12, filas de I-53S):

- **Source** = direccion `(FondoIndex, PostIndex)` que la UI recuerda («Tomar como origen»). La configuracion se captura
  en PREPARE sobre el valor ACTUAL (§3.3, §6.2).
- **Destinos**: seleccion y visualizacion; `PostTargets` = `Actual` / `Explicito` / `Todos` (§6.3), combinado con el
  `TargetFondos` existente de I-43 tal cual.
- **PREPARE** desde Application (`SelectiveHeaderBatchPlanner.Prepare`); **Warnings** y **CONFIRM** solo con aviso
  severo (§3.4, §6.5).
- **MUTATE** (`SelectiveEditorState.ApplyHeaderBatch`) y **un** recompute por operacion confirmada; **cero** en
  `Rejected` y `Cancelled` (§3.10).
- **Outcome / informe** al usuario: `Committed(Applied, Omitted)`, `Cancelled` o `Rejected(code)` (§3.4-§3.7).
- **N-01** y **RR-01** (3.1 y 3.2).
- En la ventana: cierre de **L-7** (§6.7); retirada de su copia de la regla del escalar de peralte de EDIT
  ([contrato de I-53](I-53-cabeceras-configurables-multidestino.md) §14.5); decidir si `ApplyCabeceraToTargets` se
  retira o se reimplementa sobre el plan, sin cambiar el resultado observable de un destino valido salvo L-7 (§6.9).
- Pruebas **S-27..S-32** y el **cierre WPF de S-23** (`LoadExisting` de la ventana; G4 probo solo la mitad de
  Application, contrato de I-53 §14.1).
- Censo de la ventana **actualizado, no relajado** (§12, fila G5).

### 3.1 N-01 (= A, resuelto)

- Cabecera del Selectivo **ya personalizada** → «Personalizar» abre el configurador en **editor avanzado**
  (`IsAdvancedEditor = true` o mecanismo equivalente real). Cabecera **estandar** → **modo rapido**.
- Seam de presentacion **testeable**, con el patron de Push Back (§6.11). `RackFrameConfiguratorWindow` **no** se
  modifica internamente.
- Prueba futura obligatoria: **S-31**. No se implementa en G0.

### 3.2 RR-01 (congelado): el gesto de G5

```text
CommitPendingEditors
→ SaveWorkingToSelected
→ resolucion vigente
→ sin recompute pendiente/diferido
→ PREPARE
→ optional CONFIRM
→ DeferRecompute {
     MUTATE
     RequestRecompute
   }
```

- La frontera **C4** y **PREPARE** quedan **fuera** del `DeferRecompute`, que envuelve **exclusivamente** MUTATE y su
  recompute (§3.11, §6.9). Si la frontera o una precondicion fallan, el gesto termina **sin Plan ni Outcome**.
- La resolucion que MUTATE necesita se lee **antes** de abrir el ambito diferido (lectura declarada en G4, contrato de
  I-53 §14.1). Un recompute entre PREPARE y MUTATE produce `Rejected(StaleTargets)`.
- Prueba futura: **S-32**. No se implementa en G0.

## 4. Fuera de alcance

Dinamico (es I-53D); Push Back; cambios en `Application/Systems/Shared`; cambios del contrato de Application salvo
defecto real (seccion 12); persistencia nueva; DTO y `SchemaVersion`; plantillas; Project Variables; Expression Engine;
L-2. Tampoco el Plugin, `assets/`, `deploy/`, `.github/`, `SelectivePalletDesign.cs` (caliente) ni la lista de
Proposal V2 §14 «No se tocan en ninguna unidad». La disposicion WPF, los nombres de controles, la seleccion del origen,
las casillas y listas y los dialogos se deciden en G5 **sin ADR**: ADR-0037 excluye XAML y controles.

## 5. Contexto requerido

- Autoridades: [Proposal V2](I-53-proposal-v2.md) (§3, §6, §9, §12-§14), [contrato de I-53](I-53-cabeceras-configurables-multidestino.md),
  [decisions/I-53.md](../automation/decisions/I-53.md) (secciones 10 y 11), [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md)
  y [HANDOFF](../HANDOFF.md) vigente (entrada de I-53 E1 y sus puntos de continuacion).
- [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md) (frontera C4 y autoridades por
  fondo) y [ADR-0029](../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md) (ventanas WPF).
- [WORKFLOW](../WORKFLOW.md) §4.5 y §7; [AGENTS.md](../../AGENTS.md).
- Context Packs del frontmatter: el subconjunto de los de I-53 que alcanza la UI del Selectivo.

## 6. Dependencias y coordinacion

- **I-53 E1**, integrada y cerrada en `main` con sus compuertas posteriores ([HANDOFF](../HANDOFF.md)): es la fundacion
  que G5 cablea.
- **I-49** (`architecture/motor-expresiones-parametricas`): **estorbo por archivo caliente** (WORKFLOW §7).
  Auditoria de G0 contra `origin/main`:

  ```text
  CURRENT_OVERLAP_I49           = NO    su diff productivo esta solo en Application/Expressions/** y en pruebas Core
  PLANNED_HOTFILE_COLLISION_I49 = YES   su G10 cambia un miembro, Describe (P23.13 / D21), de RackSelectiveWindow.xaml.cs
  ```

  I-49 lo declara en sus propias fuentes: su contrato §6.3, su Consensus Freeze §5, su Proposal V6 §12.1 y ADR-0038
  («Que vigilar») piden **serializar ese cambio con el G5 de I-53S**. **G5 no se abre hasta que el Coordinador fije el
  orden**, y antes de editar la ventana se repite el preflight de archivos calientes contra el `main` y las ramas
  vigentes.
- **I-52** (`feature/rackmirror-espejo-semantico`): solo documentacion, sin archivos de la UI del Selectivo. Su rama ya
  registra una coordinacion semantica: re-medir su C2-4 cuando se conecte `ApplyHeaderBatch`.
- **I-54** (`architecture/propiedades-personalizadas`): produccion en `Application/CustomProperties` y
  `Application/Persistence`, sin archivos de I-53S.
- **Cruce documental previsto**: filas de `docs/ROADMAP.md` (cada rama inserta la suya); se resuelve al integrar
  conservando todas las filas.
- **Entrada del Owner**: ninguna pendiente (`requires_owner_decision: false`).

## 7. Archivos esperados

| Gate | Crear | Modificar | Solo leer |
|---|---|---|---|
| G0 | este contrato | `docs/ROADMAP.md` (su fila, momento 2) | contrato de I-53, Proposal V2, ADR-0037 |
| G5 | pruebas nuevas en `tests/RackCad.UI.Tests/` | `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml` y `.xaml.cs`; `SelectiveShellMigrationTests.cs` y `SelectiveEditorWindowTests.cs` si cambian firmas o el censo | `RackFrameConfiguratorWindow.xaml.cs`, `RackFrameConfiguratorViewModel.cs` y la fundacion de `Application/Systems/Selective` y `Shared` |
| E2-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (su fila, momento 3) y este contrato | — |

Una desviacion material de esta tabla obliga a detenerse.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | reclamo atomico, este contrato y la fila en ROADMAP | **cierra con este bootstrap** |
| G5 | Selectivo UI + N-01 + RR-01 | ventana sobre Plan/Outcome; S-23 (WPF) y S-27..S-32 | **bloqueado** por coordinacion de archivo caliente con I-49 |
| E2-C | Candidato | seccion 9 | pendiente |
| E2-V | Owner Validation del Selectivo | seccion 10 | pendiente |
| E2-I | Integracion | WORKFLOW §4.5 completo; limpieza solo tras las coberturas del `MERGE_SHA` y del Candidato | pendiente |

Los gates estan congelados en la Proposal V2 §12. Ninguno arranca sin evidencia revisable del anterior y sin orden del
Coordinador.

## 9. Pruebas y builds

- **G5**: RED **por asercion** de comportamiento sobre codigo compilable (nunca por compilacion ni por excepcion) →
  verde, con S-27..S-32 y el cierre WPF de S-23 (Proposal V2 §13.3). Siguen verdes las suites del Selectivo y de I-43.
  Core y UI en local, y CI de `push` sobre el SHA del gate.
- **E2-C (Candidato)**: Core Full local, **UI Full local**, build Debug de UI y de Plugin, y CI de `push` 4/4 sobre el
  SHA exacto (AGENTS, «Pruebas — definicion de terminado»; Proposal V2 §9.4). LC-UI no reduce el Candidato.
- Una seleccion de pruebas que no selecciona nada es FALLO. Los conteos viven solo en HANDOFF.

## 10. Validacion manual

**E2-V**: Owner Validation en AutoCAD 2025 sobre el DLL Debug del SHA exacto del Candidato, con el checklist de la
Proposal V2 §13.8:

- «Personalizar» de una cabecera ya personalizada abre el editor avanzado (N-01).
- Tomar como origen; aplicar a uno, a varios y a todos.
- El origen editado despues de tomarlo aplica su valor actual.
- Omisiones por poste ausente y por fondo inexistente.
- Altura severa: confirmar y cancelar.
- Destino con peralte heredado desde un origen con peralte propio, con y sin peralte del tramo.
- Independencia de las copias.
- Actualizar, `RACKEDITAR` y guardar y reabrir.
- Vistas por fondo.
- BOM del editor frente a `RACKBOMTOTAL`.

## 11. Criterios de aceptacion

1. En el Selectivo, una cabecera personalizada existente se aplica como origen a un conjunto `TargetFondos × PostTargets`
   elegido por el usuario.
2. Cada destino recibe una **copia independiente**. `Height` viaja con la receta, `Depth` la impone el fondo destino y
   el peralte sale de `PostPeralteAt` (§6.5-§6.7), sin reglas reescritas en la ventana.
3. **Todo o nada**: un destino aplicable invalido rechaza el lote con mutacion cero. Los omitidos se informan y nunca se
   clampean ni se crean.
4. **Un** recompute por operacion confirmada, cero en `Rejected` y en `Cancelled`, y la frontera C4 y PREPARE fuera del
   ambito diferido (RR-01).
5. N-01: una personalizada reabre en editor avanzado; una estandar, en modo rapido.
6. El resultado sobrevive a Actualizar, `RACKEDITAR` y guardar y reabrir; BOM y dibujo leen la misma autoridad; Push Back
   y Dinamico no cambian.
7. S-23 (WPF) y S-27..S-32 verdes, con S-31 obligatoria; censo actualizado sin relajar; E2-V **PASS**.

## 12. Condiciones para detenerse

- **G5 no se abre** hasta que el Coordinador fije el orden con I-49 sobre `RackSelectiveWindow.xaml.cs`.
- Una **decision material** nueva, o un cambio del contrato duradero (Proposal V2, ADR-0037 o las decisiones de I-53):
  detenerse y volver al Coordinador, y al Arquitecto si es arquitectonico.
- Necesidad de cambiar `Application/Systems/Shared`, el contrato de Application del Selectivo (un defecto real tambien se
  reporta antes de corregirlo), el Plugin, un DTO, la persistencia o `SchemaVersion`: detenerse.
- Necesidad de modificar `RackFrameConfiguratorWindow` o `RackFrameConfiguratorViewModel` para N-01: detenerse.
- Deriva hacia el Dinamico, Push Back, I-53D, o hacia reabrir I-40, I-43, ADR-0032 o ADR-0037: detenerse.
- Un avance de `main` cuyo rebase toque la ventana o la fundacion: reconciliar segun WORKFLOW y, si hay conflicto
  productivo, detenerse.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-53S.yml`: la automatizacion esta **desactivada** (`automation.enabled: false`), como en
I-53. El estado vivo se deriva de la existencia de `origin/feature/cabeceras-multidestino-selectivo`
([WORKFLOW](../WORKFLOW.md) seccion 2). Las decisiones de la linea van a
[`decisions/I-53.md`](../automation/decisions/I-53.md), sin registro propio. No hay Pull Request: la integracion es
manual (WORKFLOW 4.5).

## 14. Evidencia final

Pendiente hasta E2-I. El SHA de este bootstrap se registra en un commit posterior: un commit no puede citar su propio
SHA.
