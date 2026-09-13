---
schema: rackcad-initiative/v1
id: I-53
title: "Cabecera configurable: configuracion origen hacia conjuntos de destinos en todos los sistemas"
type: feature
status: integrated
branch: feature/cabeceras-configurables-multidestino
base_branch: main
priority:
size:
depends_on: [I-40, I-43]
conflicts_with: []
context_packs: [system-dynamic-flowbed, system-selective, ui-editors, persistence, architecture-kernel]
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

# I-53 — Cabecera configurable: configuracion origen hacia conjuntos de destinos en todos los sistemas

> **Fase actual: E1-I — cierre documental para la integracion de E1 (2026-09-13).** La fundacion E1 esta
> terminada y validada; este commit es **solo documentacion** y **no** reemplaza al Candidato. El merge `--no-ff`,
> el CI del `MERGE_SHA` con su cobertura y la limpieza siguen [WORKFLOW](../WORKFLOW.md) 4.5.
>
> ```text
> G0 = CLOSED   G1 = CLOSED   G2 = FROZEN
> G3 = CLOSED   G4 = CLOSED   G6 = CLOSED
> E1-C = PASS        Candidate f271fd578b3e583f1261d9f30e97d90a838bb7fe
> E1-V = PASS        Owner, smoke de E1 en AutoCAD 2025 (Selectivo, Dinamico, Actualizar/Insertar vista, RACKBOMTOTAL)
> E1-I = EN CURSO    cierre documental; merge y compuertas posteriores pendientes
>
> UI capability = NOT YET DELIVERED (ID6/ID7 sin llamador de produccion ni UI)
> Next          = I-53S (Selectivo UI) -> I-53D (Dinamico UI); ninguna reclamada
> ```
>
> I-53 es **una** iniciativa conceptual: la integracion de E1 **no** la completa. El contrato tecnico vinculante sigue
> siendo la [Proposal V2](I-53-proposal-v2.md), congelada; la [Proposal V1](I-53-proposal-v1.md) queda superada.
> Arquitecto V2 **AGREED**; OD-2.b, OD-6 (B′) y OD-8 **aprobadas**;
> [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md) **aceptado**. Los conteos de
> pruebas viven en [HANDOFF](../HANDOFF.md) §5, que es su unico sitio.

> **Apertura por autorizacion explicita del Owner sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la preexistencia de
> la fila en ROADMAP: la fila durable y este contrato nacieron en el bootstrap inmediatamente posterior
> al reclamo atomico. La norma no exige un registro de decisiones para el caso (d); desde G2B, con decisiones del
> Owner y veredictos de Arquitecto que los gates deben verificar desde la rama remota (TEMPLATE seccion 13), el canal
> durable es [`docs/automation/decisions/I-53.md`](../automation/decisions/I-53.md), que tambien sirve a I-53S e I-53D.

```text
Initiative   = I-53 (conceptual: ID6 REUSE + ID7 BATCH DISTRIBUTION; unidad Git: contrato + fundacion E1)
Branch       = feature/cabeceras-configurables-multidestino
Worktree     = ~/.codex/worktrees/feature-cabeceras-configurables-multidestino
BASE_SHA     = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main al reclamar, merge de I-51)
REBASE_BASE  = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (origin/main en G2-F, merge de I-50)
CLAIM_SHA    = e1d5996e70cb75589beb96a256714982fe693be3   (commit vacio; f317ea9 tras el rebase de G2-F)
Claim-Id     = d7144fe8-6921-4a46-9d66-2d723620bea4
G2           = FROZEN

G2_FREEZE_SHA    = 5efaf7e1ecf8b070e06e4bd6fe8aca179afda847   (G2-F; CI push 34738923985 success)
G3_SHARED_SHA    = 4e00a273e22634cb3abbc1b3fb3778edf49dbc7e   (CI push 34741192566, 4/4 success)
G4_SELECTIVE_SHA = 7de424e4f0f69f6e2fb18fd7c2e018479aab3fbe   (CI push 34747886682, 4/4 success)
G6_DYNAMIC_SHA   = f271fd578b3e583f1261d9f30e97d90a838bb7fe   (CI push 34750678118, 4/4 success)
E1_CANDIDATE_SHA = f271fd578b3e583f1261d9f30e97d90a838bb7fe   (= G6; sin commit ni rebase posterior)
CI Candidate     = push 34750678118, head_sha = E1_CANDIDATE_SHA, 4/4 success
Owner Validation = E1-V PASS — AutoCAD 2025, smoke de E1 sobre el DLL Debug de E1_CANDIDATE_SHA
MERGE_SHA        = PENDING hasta el merge
Decisiones       = docs/automation/decisions/I-53.md (secciones 10 y 11)
ROADMAP          = fila de I-53; alineada en el cierre: integrada (2026-09-13), E1 cerrada, linea en I-53S e I-53D
```

## 1. Objetivo

Que la **configuracion de una cabecera existente** pueda tomarse como **origen** y aplicarse a un **conjunto de
cabeceras destino**, cada una como **copia independiente** y con **aplicacion atomica**.

**OD-1 — RESUELTA por la autorizacion original.** Texto vinculante:

- **ID6 — REUSE**: el usuario selecciona una configuracion de cabecera existente como SOURCE; reutilizar
  significa COPIAR sus valores authored, sin live link, sin instancia mutable compartida y sin referencia viva.
  Despues de aplicar, source y destination son independientes.
- **ID7 — BATCH DISTRIBUTION**: una source configuration se aplica a un CONJUNTO de destinos compatibles.
  Source y Destinations son conceptos separados, sin una lista ambigua que los mezcle. La taxonomia de destinos
  no es universal: cada sistema traduce sus destinos al contrato comun.

**Resultado de G1 y G2.** Sistemas **en alcance**: **Selectivo** y **Dinamico**. Push Back = ALREADY DONE (I-40:
precedente y regresion, sin cambios). Cabecera independiente, Cantilever, Cama, Larguero y Drive-In: **N/A**.

**Estructura de la linea (PA-1B).** I-53 es **una** iniciativa conceptual con **un** contrato, **un** ADR y **un**
registro de decisiones, entregada en **tres integraciones** (OD-6 B′), cada una en su propia unidad Git:

| Unidad | Rama | Entrega |
|---|---|---|
| **I-53** (esta) | `feature/cabeceras-configurables-multidestino` | contrato congelado + **E1 fundacion**: G3 nucleo compartido, G4 Selectivo Application/estado, G6 Dinamico Application/estado/reconciliacion |
| **I-53S** (futura, no reclamada) | `feature/cabeceras-multidestino-selectivo` | **E2**: G5 UI del Selectivo, con N-01 y el cableado de RR-01 |
| **I-53D** (futura, no reclamada) | `feature/cabeceras-multidestino-dinamico` | **E3**: G7 UI del Dinamico, con L-1, retirada de presets e informe de reconciliacion |

**Decisiones del Owner aprobadas**: **OD-2.b** (el Dinamico reconcilia sus personalizaciones al reconstruir e informa
lo que conserva, adapta o pierde), **OD-6 = B′** (tres integraciones de una sola iniciativa conceptual) y **OD-8**
(retirar los presets «Personalizada N» del Dinamico en su gate de UI). **ADR-0037 aceptado.**

## 2. Problema

Lo que constaba al abrir la iniciativa, antes de G1 (se conserva como registro):

- [ROADMAP](../ROADMAP.md), fila de **I-40**: la cabecera personalizada de Push Back pasa a ser
  autoridad efectiva con destinos multiples; el sistema se da por **entregado** y **no se reabre**.
- [ROADMAP](../ROADMAP.md), fila de **I-43** y
  [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md): el Selectivo edita
  por `Scope` × `TargetFondos`, con cabecera personalizada por `(FondoIndex, PostIndex)` y commit
  atomico con un solo recompute por operacion.
- El **Dinamico** comparte con Push Back la estructura dinamica y el configurador de cabecera, pero
  ninguna fila de ROADMAP registraba para el una edicion de cabeceras por destinos.
- **Drive-In**, **Cantilever** y los demas sistemas: si tenian cabecera configurable lo midio G1.

G1 respondio con evidencia ([I-53-discovery.md](I-53-discovery.md)); G2 fijo el contrato
([Proposal V2](I-53-proposal-v2.md)).

## 3. Alcance vinculante

1. **G0, G1 y G2 — hechos**: reclamo y bootstrap; Discovery; Proposals V1 y V2, revisiones de Arquitecto, decisiones
   del Owner, ADR-0037 aceptado y este freeze.
2. **E1 — fundacion, en esta rama y sin cambio visible** (Proposal V2 §3 a §9, §12 a §14):
   - **G3** — nucleo compartido **provisional** en `Application/Systems/Shared`: `HeaderConfigurationSnapshot`,
     Plan, Outcome, `Warnings`, motivos Omitted y codigos Rejected; sin ejecutor generico ni politica de cobertura.
   - **G4** — Selectivo Application/estado: `PostTargets`, PREPARE puro, MUTATE aditivo, revision de altura
     multi-poste, normalizacion de peralte por `PostPeralteAt`, firma con la generacion del sistema resuelto (RR-01).
   - **G6** — Dinamico Application/estado: `ModuleTargets` con firma `ModuleId:Kind` y generacion, PREPARE, MUTATE,
     RECOMPUTE por el constructor y reconstruccion con `RackModuleReconciliation` **sin cablear**; ademas, alinear el
     XML-doc de `DynamicRackModule.IsManualOverride` con ADR-0037, sin cambio de comportamiento.
   - **E1-C → E1-V → E1-I** (seccion 9, seccion 10 y WORKFLOW §4.5).
   - El nucleo se considera demostrado al pasar G4 **y** G6; si G6 cambia el nucleo, se repiten las suites de G3 y G4.
3. **Continuacion de la linea, fuera de esta rama**: I-53S (E2 / G5) e I-53D (E3 / G7), segun la seccion 12.

## 4. Fuera de alcance

- **En esta rama**: toda ventana, XAML, control o prueba de UI de los editores; el Plugin; G5 y G7, que pertenecen a
  I-53S e I-53D. E1 no cambia ningun comportamiento visible.
- **Push Back**: cero diff de produccion (`Application/Systems/PushBack/**`, `UI/Systems/PushBack/**`,
  `RackModuleEditSession.cs`, `RackModuleReconciliation.cs`); no se migra al contrato comun.
- **L-2** (Push Back: `RACKBOMTOTAL` y overrides por linea sin `Members`): fuera de I-53; ninguna prueba lo canoniza.
- **Reabrir** I-40, I-43, ADR-0032 o ADR-0037.
- Formato persistido, DTO nuevos, `SchemaVersion`, migracion de dibujos, catalogos y `blocks-library.dwg`.
- Territorio de iniciativas paralelas: I-49, I-52 e I-54.
- Los hallazgos laterales estan en [ideas-futuras.md](../ideas-futuras.md) (registrados en G2-F); **no se arreglan
  aqui**.

## 5. Contexto requerido

- [Proposal V2](I-53-proposal-v2.md) (contrato congelado), [Discovery](I-53-discovery.md) y
  [decisiones](../automation/decisions/I-53.md).
- [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md) (aceptado),
  [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md),
  [ADR-0029](../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md) (D8) y
  [ADR-0031](../adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md).
- Contratos de [I-40](I-40-cabeceras-push-back.md), [I-43](I-43-selectivo-scopes-fondos.md),
  [I-35](I-35-editor-avanzado-push-back.md) (reconciliacion por `ModuleId + Kind`),
  [I-17](I-17-clon-unico-cabecera.md) (clon unico) e [I-42](I-42-push-back-compuesto.md).
- [AGENTS.md](../../AGENTS.md), convenciones 2 (regla en un solo sitio), 3 (copia centralizada) y 4
  (persistencia versionada).
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **I-40 e I-43, integradas**: base de la autorizacion.
- **I-50, integrada**: merge `f8deb675c6d1ef0e64693b157d69c4cc170d7b24`; CI posterior al merge **PASS** (run
  `34736306222`, `push`, 4/4, con `rackcad-coverage-cobertura`). I-53 se rebaso sobre ese merge en G2-F. La veda sobre
  las ventanas y sus pruebas quedo **liberada**, pero G5 y G7 no pertenecen a esta rama.
- **I-49** (`architecture/motor-expresiones-parametricas`): su Proposal preve tocar un miembro de
  `RackSelectiveWindow.xaml.cs` en su G10. Si coincide con el arranque de I-53S, se aplica la regla de archivo caliente
  (WORKFLOW §7) y se serializa. Al cerrar E1 (2026-09-13) I-49 ya lleva codigo de produccion en su rama (G5, sin
  archivos de E1): **el preflight de archivos calientes es obligatorio al reclamar I-53S**.
- **I-52** (`feature/rackmirror-espejo-semantico`): comparte el indice de ADR (0036 en su rama). **I-54**: sin archivos
  de I-53.
- `conflicts_with` queda vacio: no hay conflicto activo con ninguna rama viva; la coordinacion con I-49 es futura y
  vive aqui.
- **Entrada del Owner**: ninguna pendiente (`requires_owner_decision: false`).

## 7. Archivos esperados (E1)

| Gate | Crear | Modificar | Solo leer |
|---|---|---|---|
| G3 | `Application/Systems/Shared/HeaderConfigurationSnapshot.cs` y tipos de Plan, Outcome y codigos; pruebas nuevas en `tests/RackCad.Tests/` | — | `RackFrameProjectStore.cs`, `RackFrameConfiguration.cs`, `RackDesignValidation.cs` |
| G4 | tipos nuevos del Selectivo (`PostTargets`, preparacion, firma); pruebas nuevas | `SelectiveEditorState.cs` (aditivo), `SelectiveCabeceraHeightReview.cs` (aditivo), `SelectivePostGeometry.cs` (sobrecarga, solo si hace falta) | `SelectiveCabeceraAuthority.cs`, `SelectiveFondoTargets.cs`, `SelectiveTargetResolver.cs`, `PlantaHeaderLayoutBuilder.cs` |
| G6 | tipos nuevos del Dinamico (targets, firma, preparacion, reconstruccion); pruebas nuevas | `Domain/Systems/Dynamic/DynamicRackModule.cs` (solo XML-doc de `IsManualOverride`) | `RackModuleReconciliation.cs`, `RackModuleDescriptor.cs`, `DynamicRackSystemBuilder.cs`, `DynamicRackSystemResolver.cs`, `DynamicEditorDesignAssembler.cs` |
| E1-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (fila de I-53, momento 3), este contrato | — |

**No se tocan en esta rama:** `UI/**` (ventanas, XAML, configurador), `Plugin/**`, pruebas existentes,
`Application/Systems/PushBack/**`, `RackModuleEditSession.cs`, `RackModuleReconciliation.cs`,
`RackFrameProjectStore.cs`, DTO y disenos de Selectivo y Dinamico (`SelectivePalletDesign.cs` es caliente),
`assets/`, `deploy/`, `.github/`. **Una desviacion material obliga a detenerse** (seccion 12).

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | [I-53-discovery.md](I-53-discovery.md) | **HECHA** |
| G2 | Contrato | Proposal V1 (superada) y V2; Arquitecto V1 AGREED WITH CHANGES y V2 AGREED; OD-2.b, OD-6, OD-8; ADR-0037 aceptado | **CONGELADA (G2-F)** — `5efaf7e` |
| G3 | Nucleo compartido | Snapshot, Plan, Outcome, codigos y guardas | **CLOSED** — `4e00a27` |
| G4 | Selectivo Application/estado | PREPARE/MUTATE del Selectivo, RR-01 en la firma | **CLOSED** — `7de424e` |
| G6 | Dinamico Application/estado/reconciliacion | PREPARE/MUTATE/RECOMPUTE, firma y reconstruccion sin cablear | **CLOSED** — `f271fd5` |
| E1-C | Candidato E1 | seccion 9 | **PASS** — Candidate `f271fd5` |
| E1-V | Validacion E1 | seccion 10 | **PASS** — Owner, smoke de E1 en AutoCAD 2025 |
| E1-I | Integracion E1 | WORKFLOW §4.5 completo, con limpieza | **EN CURSO** — cierre documental; merge `--no-ff`, CI del `MERGE_SHA` con cobertura y limpieza pendientes |

Ninguna fase arranca sin que la anterior tenga evidencia revisable. I-53S e I-53D siguen su propio cuadro (seccion 12).
Cada SHA de gate se registra en un commit **posterior** al que lo produce: por eso el SHA de este cierre documental no
figura aqui, y el `MERGE_SHA` todavia no existe. El nucleo compartido quedo **demostrado** por sus dos consumidores (G4 y
G6) **sin cambios** desde G3: el diff de `Application/Systems/Shared` entre `4e00a27` y el Candidato es vacio.

## 9. Pruebas y builds (E1)

- **E1-C (Candidato)**: suite Core completa en local, suite UI completa en local, build Debug de UI y de Plugin, y CI
  de `push` 4/4 sobre el SHA exacto (AGENTS, «Pruebas — definicion de terminado»), mas las invariantes E1-INV-1..5 de la
  Proposal V2 §9.3.
- Matriz congelada en la Proposal V2 §13: en esta rama, C-01..C-10 (G3), S-01..S-26 (G4) y D-01..D-25 (G6), P-01..P-03
  e I-01. S-27..S-32 pertenecen a I-53S; D-26..D-39, a I-53D.
- RED siempre por asercion de comportamiento sobre codigo compilable; una seleccion de pruebas que no selecciona nada
  es FALLO.

## 10. Validacion manual

- **E1-V**: `requires_autocad: true` y `requires_owner_validation: true` son **monotonicos**
  ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md), «La metadata de validacion del dueno es MONOTONICA»), asi que E1 **no**
  queda exenta. Consiste en un **veredicto explicito del Owner** sobre la evidencia de E1-C **mas** un **smoke corto en
  AutoCAD 2025** sobre el SHA exacto: abrir Selectivo; abrir Dinamico; Actualizar o Insertar una vista; comparar o
  verificar `RACKBOMTOTAL`; confirmar que no hay regresion visible. No es validacion funcional de ID6/ID7: E1 no expone
  esa UI.
- **I-53S e I-53D**: Owner Validation en AutoCAD 2025 sobre su propio Candidato, con los checklists de la Proposal V2
  §13.8.

## 11. Criterios de aceptacion

De la linea (ID6 + ID7), cumplidos al integrar I-53D:

1. En Selectivo y Dinamico, una cabecera existente se aplica como origen a un **conjunto de destinos** elegido por el
   usuario, en la taxonomia propia de cada sistema.
2. Cada destino recibe una **copia independiente**: editar despues un destino no altera al origen ni a otro destino.
3. La aplicacion es **atomica**: un solo destino aplicable invalido rechaza el lote; **mutacion parcial cero**.
4. Lo que Push Back ya entrega con I-40 **no cambia**.
5. El resultado sobrevive `RACKEDITAR`, Actualizar y guardar/reabrir, y BOM y dibujo leen la **misma** autoridad.

De E1, cumplidos al integrar esta rama:

6. El nucleo compartido queda demostrado por **dos** consumidores (G4 y G6) con las pruebas de la Proposal V2 §13.
7. E1-INV-1..5: sin cambio visible, sin llamadores de produccion de los tipos nuevos, pruebas existentes intactas,
   modificaciones solo aditivas y Push Back sin diff.

## 12. Condiciones para detenerse

- **Sesion de G2-F — solo documentacion**: no se abre G3 en la misma ejecucion.
- Cualquier cambio **material** del contrato compartido durante G3, G4 o G6 vuelve al Coordinador y, si es
  arquitectonico, al Arquitecto.
- Si E1 necesita tocar una ventana, el configurador, el Plugin, una prueba existente o Push Back: **detenerse**.
- Si cerrar un sistema exige cambiar el formato persistido o `SchemaVersion`: **detenerse**; eso es ADR y decision del
  Owner.
- Si el alcance deriva hacia reabrir I-40, I-43, ADR-0032 o ADR-0037: **detenerse**.
- **I-53S** no se reclama antes de que E1 este integrada y verificada (CI posterior al merge y cobertura). **I-53D** no
  se reclama antes de integrar I-53S. Ambas se reclaman desde `origin/main` por el caso (d) con OD-6 como autorizacion
  explicita, con su propio bootstrap (fila de ROADMAP y contrato minimo que remite a este contrato congelado), sin
  Discovery ni Proposal propios salvo hallazgo material; una desviacion material del contrato vuelve al Coordinador y
  al Arquitecto.
- Numero de ADR: hasta que ADR-0037 llegue a `main`, se comprueba en cada preflight; si otra rama integra antes un
  0037, se renumera sin tocar el contenido aceptado.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-53.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y los precedentes inmediatos —I-47, I-48 e I-51, tambien manuales— tampoco lo llevaron.
El estado vivo se deriva, como manda [WORKFLOW](../WORKFLOW.md) seccion 2, de la existencia de
`origin/feature/cabeceras-configurables-multidestino`; tras E1-I y su limpieza, de la de I-53S y despues I-53D.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5), una por unidad.

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-53 - Cabecera configurable hacia conjuntos de destinos en todos los sistemas
Rama       = feature/cabeceras-configurables-multidestino
Worktree   = ~/.codex/worktrees/feature-cabeceras-configurables-multidestino
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8  (origin/main)
CLAIM_SHA  = e1d5996e70cb75589beb96a256714982fe693be3  (commit vacio)
Claim-Id   = d7144fe8-6921-4a46-9d66-2d723620bea4
```

Primer `git push -u origin feature/cabeceras-configurables-multidestino` **aceptado sin force**
(`* [new branch]`); tras el push, `git log --remotes --grep "Initiative-Id: I-53"` devolvio **solo**
este reclamo. `main` **no fue modificada**.

**G1, G2A y G2B** (SHA publicado → SHA tras el rebase de G2-F; el CI se midio sobre el publicado y no se transfiere):

| Fase | Publicado | CI de `push` | Tras el rebase |
|---|---|---|---|
| Bootstrap | `405cfc0afe6bb0baaa112a822cd62610eebfc6ea` | `34726340028` success | `667f1b73454c0967d21b09a14ecf785342fd35c1` |
| G1 Discovery | `c8476cccc98809fdb7fd0aeccef288521126f442` | `34728573408` success | `df3ac1c0bb52696ff7bf97e36190d567c1881d15` |
| G2A Proposal V1 | `f38362d32a737f62d69a229854dc5c1812adf063` | `34730730052` success | `686df3eafd0ba1397c3612e4f36f3b671a1a90cc` |
| G2B Proposal V2 | `d7f17addb47ea943b61ff50b4b4a5b23010d6fab` | `34735396596` success | `d0db698facf2297d6bd5aaa512f3b3b3f2358a59` |

**G2C.** Re-revision del Arquitecto sobre `d7f17ad`: `ARCHITECT_V2 = AGREED` (decisiones, seccion 10.3).

**G2-F.** Rebase sobre `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` (un conflicto documental en el indice de ADR,
resuelto conservando las filas 0035 y 0037), aceptacion de ADR-0037, PA-1B, N-01, RR-01, follow-ups y este freeze, en
un unico commit documental. Un commit no puede citar su propio SHA: el de G2-F y su CI se registran en la evidencia de
E1.

**G2-F, registrado en E1.** `5efaf7e1ecf8b070e06e4bd6fe8aca179afda847`, CI de `push` `34738923985` **success**.

### 14.1 E1 — fundacion (G3, G4 y G6)

Cada gate se abrio por orden del Coordinador, con RED **por asercion** contra un andamiaje compilable sin
comportamiento (ninguno por excepcion ni por compilacion), GREEN de sus suites focales, de las de los gates anteriores y
de la suite Core completa, y CI de `push` 4/4 sobre su SHA exacto. Los conteos viven en [HANDOFF](../HANDOFF.md) §5; el
detalle de cada RED, en el cuerpo de su commit.

| Gate | SHA | CI de `push` | Entrega | Pruebas |
|---|---|---|---|---|
| G3 | `4e00a273e22634cb3abbc1b3fb3778edf49dbc7e` | `34741192566`, 4/4 | `Application/Systems/Shared`: `HeaderConfigurationSnapshot` (`TryCapture` / `Materialize` por el clon canonico), `HeaderBatchPlan<T>` (`Rejected` / `Prepared`), `HeaderBatchOutcome<T>` (`Rejected` / `Cancelled` / `Committed`), `HeaderBatchSignature`, omisiones, avisos y codigos cerrados; constructores internos | C-01..C-10 |
| G4 | `7de424e4f0f69f6e2fb18fd7c2e018479aab3fbe` | `34747886682`, 4/4 | `Application/Systems/Selective`: `SelectiveHeaderAddress`, `SelectivePostTargets`, `SelectiveHeaderResolution`, `SelectiveHeaderBatchRequest`, `SelectiveHeaderBatchPlanner` (PREPARE puro + firma RR-01) y `SelectiveHeaderBatchPreparation`; aditivos: `SelectiveEditorState.ApplyHeaderBatch` (MUTATE), `SelectiveCabeceraHeightReview.OfDestinations` y `SelectivePostGeometry.PostPeralteFor` | S-01..S-25 + S-26 (suites de I-43 existentes) |
| G6 | `f271fd578b3e583f1261d9f30e97d90a838bb7fe` | `34750678118`, 4/4 | `Application/Systems/Dynamic`: `DynamicHeaderAddress`, `DynamicHeaderSource`, `DynamicModuleTargets`, `DynamicHeaderBatchState`, `DynamicHeaderBatchRequest`, `DynamicHeaderBatchPreparation`, `DynamicHeaderBatch` (`SequenceSignature`, `Prepare`, `Apply` = MUTATE + un recompute) y `DynamicRackRebuild` (Snapshot → `BuildDefault` → `RackModuleReconciliation`); Domain: solo el XML-doc de `DynamicRackModule.IsManualOverride` | D-01..D-25 |

**Lecturas declaradas en G4** (cuerpo de `7de424e`): S-12 con la autoridad real `CabeceraDepthOfFondo` solo da `<= 0`
en un fondo sin slot, asi que el ultimo destino invalido se demuestra con profundidad no finita y el `<= 0` literal con
el estado sin slot; S-23 se prueba en la mitad de Application de `RACKEDITAR` (store + `SelectiveEditorOpen`), sin
`LoadExisting` de la ventana; el escalar de peralte de EDIT reproduce la regla historica de la ventana; y **G5 debe leer
la resolucion para MUTATE antes de abrir el `DeferRecompute` del lote**.

**Lecturas declaradas en G6** (cuerpo de `f271fd5`): la firma del plan agrega la `Length` de cada destino, porque PREPARE
la valida y MUTATE no valida; el plan queda ligado ademas a la instancia del sistema resuelto (§3.9: no cruza
recomputes), mientras la **peticion** sobrevive a una recomposicion sin reconstruccion (§7.6); con firma vigente,
`SourceNotFound` solo lo alcanza un id que no designa cabecera, porque una direccion tomada antes de un cambio de
secuencia es `StaleTargets` por precedencia; un id que designa varios modulos no es direccion (`MalformedTarget`); y el
recompute «exactamente uno» es estructural —Application no tiene contador—, con cero recomputes fijados en PREPARE y en
todo `Rejected`. Mutaciones temporales, revertidas: sus guardas fallan por asercion.

### 14.2 E1-C — Candidato

```text
Candidate SHA:        f271fd578b3e583f1261d9f30e97d90a838bb7fe
Arbol limpio:         SI
SDK resuelto:         8.0.423
Core Full local:      PASS
UI Full local:        PASS
Debug UI build:       PASS
Debug Plugin build:   PASS
CI exact SHA:         GREEN (run 34750678118, job ui-tests success)
Owner validation:     pass
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg  SHA-256 B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
```

La biblioteca se resolvio por el override de `%APPDATA%\RackCad\settings.json` y su hash se volvio a medir al recibir el
veredicto: sin cambios. Los ensamblados de la ronda llevan `InformationalVersion 1.0.0+f271fd578b3e583f1261d9f30e97d90a838bb7fe`.
La cobertura **no** es requisito para declarar el Candidato (AGENTS, «Pruebas», punto 1; WORKFLOW §4.5 paso 2.bis); se
mide despues del merge.

| Invariante | Resultado | Evidencia (`git diff 5efaf7e..f271fd5`) |
|---|---|---|
| E1-INV-1 | **PASS** | cero archivos en `src/RackCad.UI`, `src/RackCad.Plugin`, `assets`, `deploy`, `eng`, `.github` y `docs`; todos los archivos clasificados: Shared, Selectivo, Dinamico, un Domain y pruebas nuevas |
| E1-INV-2 | **PASS** | los simbolos nuevos no existian en `src/` en la base y hoy solo aparecen en los archivos de la fundacion; ninguna ventana, comando, handler ni Plugin los invoca |
| E1-INV-3 | **PASS** | todas las pruebas de E1 son archivos nuevos; ninguna prueba existente modificada; `tests/RackCad.UI.Tests` intacto; omitidas sin aumento |
| E1-INV-4 | **PASS** | `SelectiveEditorState`, `SelectiveCabeceraHeightReview` y `SelectivePostGeometry` solo con adiciones, fuera de `ApplyCabeceraToTargets`, `Of` y `PostPeralteAt`; `DynamicRackModule`, solo el XML-doc; `DynamicEditorDesignAssembler` y las ventanas, sin diff |
| E1-INV-5 | **PASS** | cero archivos de Push Back; su regresion completa, verde |

### 14.3 E1-V — validacion del Owner

```text
Fecha y zona:                 2026-09-13 (-06:00), veredicto recibido por el canal del Coordinador de I-53
Validador:                    Owner del repositorio
Iniciativa / rama:            I-53 E1 / feature/cabeceras-configurables-multidestino
Commit:                       f271fd578b3e583f1261d9f30e97d90a838bb7fe
Worktree:                     ~/.codex/worktrees/feature-cabeceras-configurables-multidestino
Ruta del DLL Debug:           <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
InformationalVersion:         1.0.0+f271fd578b3e583f1261d9f30e97d90a838bb7fe
SHA-256 del DLL:              68F79064076B013C8241B643D54083668C0F3B82BC5C5A77CC03961DC7E8D176
Version de AutoCAD:           AutoCAD 2025 (R25.0.171.0.0)
DWG / escenario:              smoke corto de E1 (contrato, seccion 10)
Bloques reales disponibles:   biblioteca de 14.2
Pruebas automatizadas:        Core Full y UI Full PASS (conteos en HANDOFF §5)
Build UI:                     PASS
Build Plugin:                 PASS

Checklist ejecutado (alcance de E1-V, tal como lo declaro el Owner):
- [x] Selectivo
- [x] Dinamico
- [x] Actualizar / Insertar vista
- [x] RACKBOMTOTAL
- [x] sin regresion visible

Resultado por punto:          smoke PASS sobre Selectivo y sobre Dinamico
Fallos y severidad:           ninguno
Resultado global:             aprobado (smoke de E1; no es validacion total ni funcional de ID6/ID7, que E1 no expone)
Confirmacion explicita:       E1-V = PASS
```

Veredicto (2026-09-13): **E1-V = PASS** — smoke de E1 sobre Selectivo y Dinamico, Actualizar/Insertar vista y
`RACKBOMTOTAL`, sin regresion visible.

### 14.4 E1-I — integracion

- `origin/main` **no avanzo** desde `f8deb675c6d1ef0e64693b157d69c4cc170d7b24`: **sin rebase final**, de modo que la
  validacion del Owner corresponde exactamente al contenido que se integra.
- Este cierre es **docs-only**: `git diff --name-only f271fd5..` del commit de cierre lista solo `docs/`. No es un
  Candidato y no reemplaza al funcional.
- Pendiente al escribirlo: merge `--no-ff`, CI del `MERGE_SHA` con su artifact `rackcad-coverage-cobertura`,
  comprobacion diferida de la cobertura del Candidato (WORKFLOW §4.5 pasos 6 y 7) y limpieza de rama y worktree.

### 14.5 Continuacion de la linea (OD-6 B′)

```text
I-53S:
  feature/cabeceras-multidestino-selectivo
  pendiente de claim/bootstrap tras E1-I
  implementa G5 + N-01

I-53D:
  feature/cabeceras-multidestino-dinamico
  pendiente despues de integrar I-53S
  implementa G7 + L-1 + retirada de presets + reconciliation reporting
```

**Puntos de continuacion para I-53S (G5).** Cablear `SelectiveHeaderBatchPlanner.Prepare` y
`SelectiveEditorState.ApplyHeaderBatch` en la ventana; frontera C4 fuera del `DeferRecompute` del lote y precondiciones
RR-01 antes de PREPARE; leer la resolucion para MUTATE antes de abrir el ambito diferido; un solo recompute por operacion
confirmada; retirar de la ventana la copia de la regla del escalar de peralte de EDIT; N-01 (S-31) y S-27..S-32.
**Preflight de archivos calientes obligatorio**: I-49 puede tocar `RackSelectiveWindow.xaml.cs`.

**Puntos de continuacion para I-53D (G7).** Cablear `DynamicHeaderBatch` y reemplazar en la ventana el par ordinal
`SnapshotHeaderFondos`/`RestoreHeaderFondos` por `DynamicRackRebuild`, mostrando `Describe()`; retirar los presets
«Personalizada N» (OD-8, D-27); L-1 (D-28..D-31); D-26 y D-32..D-39; reapuntar `Fact5` (x2) y
`DynamicEditorDesignAssemblerTests` al contrato de reconciliacion **sin relajarlos** (D-39). D-23 caracteriza que una
reconstruccion del Dinamico **hoy** pierde los overrides por linea: es comportamiento historico, no una golden de L-2.
