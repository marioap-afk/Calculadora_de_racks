---
schema: rackcad-initiative/v1
id: I-49
title: "Motor de expresiones paramétricas"
type: architecture
status: claimed
branch: architecture/motor-expresiones-parametricas
base_branch: main
priority:
size:
depends_on: [I-47, I-48]
conflicts_with: []
context_packs: [architecture-kernel, persistence, ui-editors, autocad-plugin, system-selective]
automation_state_path:
decision_paths: [docs/automation/decisions/I-49.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-49 — Motor de expresiones paramétricas (Expression Engine, ID22B)

> **Fase actual: G0 CERRADA — reclamada y bootstrapeada.** El Discovery (G1) **no ha empezado**, no hay
> Proposal, no hay consenso y **no hay una sola línea de producción escrita**. La implementación
> sustantiva está **BLOQUEADA** por la compuerta de la sección 12.

> **Apertura por autorización explícita del Owner sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) sección 2. Esa autorización sustituye **únicamente** la preexistencia de la
> fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap inmediatamente posterior al
> reclamo atómico. El estado vivo no se lee aquí: se deriva de la existencia de
> `origin/architecture/motor-expresiones-parametricas`.

> **Excepción de proceso vigente: `OWNER_OVERRIDE_I49_I50_PARALLEL`.** I-49 se reclama y se ejecuta **en
> paralelo con I-50** aunque el contrato remoto vigente de I-50 declare `conflicts_with: [I-49]`. Es una
> excepción **para ese par concreto**, no una reinterpretación general de WORKFLOW. Registro durable y
> atribuible en [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md); sus condiciones
> vinculantes se reproducen en la sección 6.2.

## 1. Objetivo

Dar a las **variables de proyecto** la capacidad que I-47 (ID22A) e I-48 dejaron expresamente fuera: que
el valor de una variable pueda **definirse mediante una expresión** sobre otras variables, y no solo
mediante un literal. La dirección está escrita desde I-47: el mismo campo que hoy acepta `6` o
`=Holgura General` «admitiría más adelante `=Holgura General + 2`» ([HANDOFF](../HANDOFF.md) sección 4,
marcada como dirección **no normativa**, que «no autoriza» adelantar nada).

El resultado verificable concreto lo fija la **Proposal congelada** (sección 8). Lo que este contrato fija
desde ya es **dónde vive** la capacidad y **qué no puede romper**:

- [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) §15, **aceptado**: ID22B
  «extenderá `ProjectVariable.Definition`», mientras ID21 extiende `PropertyValue<T>`. Extienden **tipos
  distintos**, y esa separación es lo que les permite avanzar sin estorbarse. Apartarse de ese punto de
  extensión exige ADR nuevo aceptado por el Owner.
- La semántica integrada por I-47 e I-48 es **invariante de entrada** (sección 3.2).

## 2. Problema

Hoy una variable de proyecto tiene **definición literal** y **un único tipo**, `Length`
([ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) §3). Un valor que depende de otro no
puede expresarse como tal: la relación entre ambos no existe en el modelo.

La documentación vigente deja escrito qué **no** resolvió ID22A y tendrá que resolver ID22B **entero**
([Proposal V4 de I-47](I-47-proposal-v4.md) §10):

- el **grafo de dependencias** entre variables, la **detección de ciclos** y el **orden de evaluación**;
- qué ocurre al **borrar una variable usada por una fórmula**: el bloqueo de `Delete` cuenta hoy
  consumidores **propiedad→variable**, no **variable→variable**;
- que «una expresión necesita **operandos tipados** y coherencia de unidades»;
- y un límite común: la propagación de ID22A es de **profundidad 1**, e ID22B «convierte el preflight en
  un recorrido de **grafo**, no de lista».

A eso se suma una regla de persistencia ya decidida: un `Definition.kind` o un `VariableType` que la build
no entiende es **ERROR**, nunca dato ignorado, y **«ID22B decidirá su evolución»** de versión
([decisiones de I-47](../automation/decisions/I-47.md), C4-8 y C4.6-8).

**Por qué se empieza por un Discovery y no por código.** ADR-0034 rechazó implementar fórmulas dentro de
ID22A porque «multiplica el alcance y exige ciclos, orden de evaluación y tipado de operandos». Esa
advertencia sigue valiendo: por eso esta iniciativa arranca con un **Discovery read-only** del estado
real y **no** admite implementación sustantiva antes del consenso.

## 3. Alcance

### 3.1 Qué autoriza este contrato, por fases

1. **G1 — Discovery read-only** del estado real de la rama, **sin producción**. Cubre, como mínimo, lo que
   el encargo del Owner enumera:
   - `ProjectVariable`, `VariableDefinition` y `VariableType`;
   - la persistencia de `ProjectVariables` y su store, y la persistencia de property values;
   - `MutationPlan` / preflight y el descubrimiento de consumidores;
   - `Link` / `Unlink` / `Delete` / `RepairBroken`;
   - `LinkedPropertyEditor` y el protocolo C4;
   - `RACKVARIABLES`;
   - parsing y formato numérico, y helpers de unidades;
   - `ProjectPropertyIds`;
   - authored / effective, propagación y save/reload;
   - nombres duplicados;
   - las pruebas de I-47 e I-48.

   **Entregable**: rutas + símbolos + contratos + riesgos. El Discovery **no** crea parser, AST,
   evaluador, grafo, schema nuevo ni UI.
2. **G2 — Proposal** del motor de expresiones, con alternativas y coste. Responde a las preguntas de la
   sección 3.3 **sin** que este contrato prejuzgue la respuesta.
3. **G3 — Consenso**: `Coordinator = AGREED` y `Architect = AGREED` sobre la **MISMA** Proposal, más la
   decisión del Owner sobre ella. Si la Proposal toma una decisión de arquitectura o cambia persistencia o
   schema, el **ADR** correspondiente se acepta **antes** de implementar ([WORKFLOW](../WORKFLOW.md)
   sección 8).
4. **G4+ — Implementación**, solo tras G3, por la secuencia de gates que fije la Proposal congelada.

### 3.2 Invariantes de entrada — I-47 e I-48

La semántica integrada por I-47 e I-48 **no se reabre** desde esta iniciativa. I-49 puede **extenderla**
por el punto previsto; **no** puede **cambiarla** sin ADR nuevo y decisión del Owner. La tabla orienta y
no pretende ser exhaustiva: **las fuentes vinculantes son las enlazadas**.

| Invariante | Fuente |
|---|---|
| Un único `ProjectVariablesDocument` por DWG: autoridad de nivel dibujo | ADR-0034 §1 |
| `VariableId` estable e inmutable; `Name` es etiqueta mutable y **no participa en ninguna resolución** | ADR-0034 §2 |
| Variables **tipadas** desde el primer día; el discriminador de tipo se persiste | ADR-0034 §3 |
| Una propiedad vale `Literal(T)` o `ProjectVariableReference(VariableId)`, identificada por `PropertyId` estable | ADR-0034 §4 |
| authored ≠ effective: mientras hay binding, el literal authored queda congelado e inactivo | ADR-0034 §5 |
| **Un único punto de resolución**, en Application; dibujo, BOM y preview consumen solo el efectivo y **no conocen `VariableId`** | ADR-0034 §6 |
| Propagación **atómica**: preflight completo antes de mutar; un preflight fallido deja un plan **vacío, nunca parcial** | ADR-0034 §7 |
| **Fail-closed**: `UNKNOWN` / `UNREADABLE` / `BROKEN` ≠ `ABSENT` / `EMPTY` / `SUCCESS` | ADR-0034 §8 |
| Autoridad multi-vista por `RackId`: divergencia o ilegible ⇒ fail-closed | ADR-0034 §9 |
| Las tres asimetrías: vincular **congela** y desvincular **materializa**; un vínculo roto no se desvincula y solo se repara con aviso explícito; un registro presente e ilegible **nunca** se lee como vacío | ADR-0034 §11; HANDOFF §1 |
| `Delete` bloqueado con consumidores; **sin** fallback automático al literal authored | ADR-0034 §11 |
| Versionado propio del registro; `major` superior, `VariableType` desconocido y `Definition.kind` desconocido = **ERROR** | ADR-0034 §13; decisiones de I-47 C4-8 y C4.6-8 |
| La UI permanece AutoCAD-free; el Plugin es el único dueño del NOD, los `ObjectId` y la `Transaction` | ADR-0034 §14 |
| `Link` congela el último literal **comprometido**; un vínculo roto es fail-closed; reparabilidad **atómica por rack** | decisiones de I-48 §4 (Proposal V8) |
| `VariableId` duplicado = **identidad ambigua ⇒ fail-closed**, sin elegir first/last y sin reparar el registro | decisiones de I-48 §4 |
| Lookup de `VariableId` unificado; semantic-usability acreditada también sobre la re-lectura de commit, antes de `ApplyTo` | decisiones de I-48 §4 |
| El `Type` persistido se interpreta con un **mapping único** compartido | decisiones de I-48 §4; HANDOFF §1 |
| `LinkedPropertyEditor`: el mismo campo acepta literal o `=Nombre`, autocompletado **sin auto-selección**, desambiguador **obligatorio** con nombres duplicados, y campos vinculables dentro del protocolo C4 (`TryStage` / `ApplyStaged`) | HANDOFF §1 |

### 3.3 Preguntas que la Proposal tiene que responder, y este contrato NO responde

Las que la documentación vigente ya atribuye a ID22B (sección 2), más las que el Discovery identifique:

- forma de la expresión y cómo se representa y evalúa;
- grafo de dependencias variable→variable, ciclos y orden de evaluación;
- ciclo de vida de una variable **usada por una expresión** (borrar, renombrar, reparar);
- tipado de operandos y coherencia de unidades;
- evolución de versión de `ProjectVariablesDocument` para el nuevo `Definition.kind` (C4-8);
- propagación de profundidad mayor que 1 y su preflight;
- superficie de entrada: la dirección tipo Excel es **no normativa** y no se impone.

## 4. Fuera de alcance

- **Custom BOM** — fuera, por instrucción del Owner.
- **ID20 productivo** — fuera, por instrucción del Owner.
- **Referencias Rack→Rack (ID21)**: referirse a propiedades de **otro** rack sigue sin existir. ADR-0034
  §15 las sitúa en `PropertyValue<T>`, no en la variable.
- **Transferencia o fusión de variables entre dibujos**: **no pertenece a ID22B** (decisiones de I-47,
  C2-8).
- **El alcance de I-50** (cotas independientes por vista).
- **En G0 y G1**: parser, AST, evaluador, grafo, schema de expresiones y UI productiva, y cualquier cambio
  en `src/`, `tests/`, `assets/`, `eng/`, `deploy/`, `tools/` o `.github/`.
- Los hallazgos laterales se registran en [ideas-futuras.md](../ideas-futuras.md); **no se arreglan aquí**.

> **Nota de trazabilidad.** `ID20` y «Custom BOM» **no tienen definición en el repositorio** en la base de
> esta iniciativa: una búsqueda sobre todo el árbol no devuelve ninguna coincidencia. Se registran **tal
> como los nombra el Owner** y este contrato **no les inventa contenido**. Si el Discovery o la Proposal
> necesitan conocer su frontera exacta, se pregunta al Owner antes de trazarla.

## 5. Contexto requerido

- [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) — autoridad drawing-level y punto de
  extensión de ID22B. **Aceptado, y por tanto inmutable en su contenido.**
- [Contrato de I-47](I-47-project-variables-foundation.md), [Proposal V4](I-47-proposal-v4.md) §10 y
  [decisiones de I-47](../automation/decisions/I-47.md) (C2-1, C2-8, C4-8, C4.6-8).
- [Contrato de I-48](I-48-generic-linked-property-editing.md), [Proposal V8](I-48-proposal-v8.md) y
  [decisiones de I-48](../automation/decisions/I-48.md).
- [HANDOFF](../HANDOFF.md) sección 1 (I-47 e I-48 tal como quedaron integradas) y sección 4 (dirección de
  UX no normativa y compatibilidad con ID22B).
- [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) — `OWNER_OVERRIDE_I49_I50_PARALLEL`.
- [WORKFLOW](../WORKFLOW.md) secciones 2, 3, 4 y 7; [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) secciones 6, 11 y
  12; [AGENTS.md](../../AGENTS.md).
- Contrato remoto de **I-50** en `origin/feature/cotas-independientes-por-vista`, **solo lectura**, para la
  coordinación de la sección 6.3.
- Context Packs declarados en el frontmatter, por el área que cubre el Discovery: `architecture-kernel`
  (resolución, preflight y plan de mutación), `persistence` (registro, schema y property values),
  `ui-editors` (`LinkedPropertyEditor` y C4), `autocad-plugin` (`RACKVARIABLES`, propagación y
  save/reload) y `system-selective` (las propiedades vinculables vigentes).

## 6. Dependencias, paralelismo y coordinación

### 6.1 Dependencias

- **I-47 integrada y cerrada** (2026-09-09): fundación ID22A. Sin ella esta iniciativa no tiene objeto.
- **I-48 integrada y cerrada** (2026-09-12): edición vinculable reusable, `LinkedPropertyEditor` y C4.
- **Entradas del Owner requeridas**: la decisión sobre la Proposal en G3 y, si aplica, la aceptación de
  ADR.
- **I-50 NO es dependencia**, en ningún sentido: I-49 no espera a I-50 ni I-50 a I-49.

### 6.2 `OWNER_OVERRIDE_I49_I50_PARALLEL`

**Qué sustituye.** La leyenda del ROADMAP define «Se estorba con» como «no correr en paralelo con esa
iniciativa (mismos archivos)», y [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) sección 6 considera activo un
conflicto mientras la iniciativa conflictiva tenga rama remota no integrada. El contrato remoto vigente de
I-50 declara `conflicts_with: [I-49]`. El Owner **sustituye específicamente ese gate** para este par.

**Condiciones vinculantes** (texto del Owner; registro íntegro en
[`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md)):

- NO modificar rama ni worktree de I-50.
- NO detener ni alterar su corrida actual.
- Si I-49 detecta que necesita editar un archivo productivo que I-50 está modificando materialmente,
  DETENERSE antes de editarlo y reportar la colisión.
- `LinkedPropertyEditor`, Project Variables y Expression Engine pertenecen funcionalmente al alcance de
  I-49; I-50 tiene prohibido refactorizarlos salvo necesidad demostrada.
- Integración I-49/I-50 será serializada; la segunda que integre deberá reconciliarse con `main`.
- La contradicción documental de I-50 se corregirá posteriormente en su propia iniciativa.
- Registra esta excepción en el contrato/bootstrap de I-49 para que sea durable y auditable.

**Por qué `conflicts_with` queda vacío.** Declarar aquí `[I-50]` afirmaría que ambas **no** pueden correr en
paralelo, que es exactamente lo que el Owner autoriza, y replicaría la declaración que el propio Owner
califica de contradicción documental. La relación entre ambas es **coordinación por archivos
compartidos**, no estorbo ni dependencia.

### 6.3 Coordinación de archivos calientes con I-50

- **El registro vivo es el remoto** ([WORKFLOW](../WORKFLOW.md) sección 2). Antes de editar **cualquier**
  archivo productivo, I-49 comprueba contra la rama remota de I-50 si ese archivo está siendo modificado,
  con el procedimiento de [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) sección 8.
  Una modificación material ⇒ **detenerse antes de editar** y reportar.
- **Posibles zonas de cruce, a medir en G1 y no afirmadas aquí**: los archivos calientes de
  [WORKFLOW](../WORKFLOW.md) sección 7 que están en el camino de ambas —el editor Selectivo
  (`src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs`), `src/RackCad.Plugin/*Commands*.cs` y
  `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs`— y las áreas que el contrato de I-50
  declara para sí: UI de cotas, builders / draw path y persistence / update.
- **Propiedad funcional**: `LinkedPropertyEditor`, Project Variables y el Expression Engine son alcance de
  I-49 (condición del Owner).
- **Integración serializada**: nunca simultánea; la segunda en integrar se reconcilia con `main` en su
  rebase final ([WORKFLOW](../WORKFLOW.md) sección 4.5).
- **`docs/ROADMAP.md`**: I-49, I-50 e I-51 insertan su fila en el mismo punto (tras I-48), cada una en su
  rama. Es un **conflicto documental previsto y trivial**, no una colisión productiva: quien integre
  después conserva todas las filas en orden numérico.
- **La contradicción documental de I-50** (`conflicts_with: [I-49]`, su fila y su sección 11) la corrige
  I-50 en su propia iniciativa. **I-49 no la edita.**

### 6.4 I-51

`feature/rackduplicar-multiples-origenes` (**I-51**) también está activa. Su contrato remoto mantiene
`conflicts_with: []`, su fila anota «vigilar I-49 e I-50», y su Discovery, ya publicado en su rama, **no
prevé ningún archivo productivo en común** con I-49. El override **no** cubre este par y **no** se declara
estorbo sin medir el cruce: se mide en G1, y si aparece una colisión productiva material real se
**detiene** y se reporta ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) sección 12).

## 7. Archivos esperados

**Este contrato no fija todavía la lista de archivos, y no debe fingir que sí.** Enumerar el wiring por
archivo y símbolo **es el entregable del Discovery**, y el Discovery no se ha hecho. Qué se toca, y en qué
capa, lo decide la Proposal congelada.

**G0 toca exactamente tres archivos, todos documentales**: este contrato,
[`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) y la fila de I-49 en
[`docs/ROADMAP.md`](../ROADMAP.md). **G1** produce solo documentación bajo `docs/`. Una desviación material
frente a esto obliga a detenerse.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atómico publicado; contrato, decisión del Owner y fila en ROADMAP | **HECHA** |
| G1 | Discovery read-only | Rutas + símbolos + contratos + riesgos (sección 3.1) | pendiente — **NO INICIADO** |
| G2 | Proposal | Motor de expresiones con alternativas y coste, congelado en una versión | pendiente |
| G3 | Consenso | `Coordinator=AGREED` y `Architect=AGREED` sobre la MISMA Proposal + decisión del Owner | pendiente — **compuerta** |
| G4+ | Implementación | Por definir **en la Proposal**, no aquí | bloqueada por G3 |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **en la Proposal**, cuando exista alcance de código. Lo exigible por norma, que no depende de la
Proposal: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto** y builds Debug de
UI y de Plugin ([AGENTS.md](../../AGENTS.md), «Pruebas — definicion de terminado»;
[WORKFLOW](../WORKFLOW.md) sección 5). G0 y G1 no producen código: su evidencia es documental.

## 10. Validación manual

**Requerida** en cuanto la implementación cambie comportamiento de dibujo: una variable definida por
expresión gobierna el valor efectivo que consumen geometría y BOM. El **checklist concreto se fija en la
Proposal**; aquí solo consta la obligación, en AutoCAD 2025. G0 y G1 **no** la requieren: no tocan
producto.

## 11. Criterios de aceptación

Los criterios funcionales concretos los fija la Proposal congelada. Con independencia de ella:

1. Las invariantes de la sección 3.2 siguen valiendo, **verificadas**, tras la implementación.
2. La capacidad de expresión vive en el punto de extensión de ADR-0034 §15 —`ProjectVariable.Definition`—
   salvo ADR nuevo aceptado por el Owner.
3. No aparece Custom BOM, ni ID20 productivo, ni referencias Rack→Rack.
4. Las condiciones de `OWNER_OVERRIDE_I49_I50_PARALLEL` constan como respetadas: rama y worktree de I-50
   sin tocar, y toda colisión productiva material reportada **antes** de editar.
5. La integración se hizo serializada respecto de I-50.

## 12. Condiciones para detenerse

- **COMPUERTA PRINCIPAL — NO IMPLEMENTATION BEFORE CONSENSUS.** La implementación sustantiva queda
  **bloqueada** hasta que **Coordinator y Architect estén `AGREED` sobre la MISMA Proposal**, sin
  desacuerdos abiertos, y el Owner decida sobre ella. Un hallazgo del Discovery, por concluyente que
  parezca, **no es una autorización**.
- **Colisión productiva material REAL con I-50**: si I-49 necesita editar un archivo productivo que I-50
  está modificando materialmente, **detenerse antes de editarlo** y reportar la colisión (condición del
  Owner).
- **Colisión productiva material real con I-51** u otra iniciativa activa: detenerse y reportar.
- Si el Discovery o la Proposal muestran que el motor exige **cambiar** una invariante de la sección 3.2, o
  salir del punto de extensión de ADR-0034 §15: **detenerse**. Eso es ADR nuevo y decisión del Owner, no un
  ajuste de alcance.
- Si el alcance empieza a incluir Custom BOM, ID20 productivo o referencias Rack→Rack: **detenerse**.
- Si la frontera de `ID20` o de «Custom BOM» resulta necesaria para decidir algo: **preguntar al Owner**, no
  suponerla.
- Parser, AST, evaluador, grafo, schema de expresiones o UI productiva **antes de G3**: prohibido.
- Cualquier edición de la rama o del worktree de I-50: prohibida.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-49.yml`: la automatización está **desactivada** (`automation.enabled: false`)
y [WORKFLOW](../WORKFLOW.md) sección 8 exige para el bootstrap del caso (d) exactamente **fila en ROADMAP
más contrato**, no un archivo de estado.

**Sí existe** [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md), porque hay una
decisión del Owner que registrar: es el canal que fijan [initiatives/README](README.md), TEMPLATE sección 13 y
[AUTOMATION_PLAN](../AUTOMATION_PLAN.md) sección 11. `requires_owner_decision: true` lo declara, y esa
metadata solo añade.

No hay Pull Request. El merge automático está prohibido; la integración es manual y serializada
([WORKFLOW](../WORKFLOW.md) sección 4.5).

## 14. Evidencia final

**Reclamo atómico (G0).**

```text
Iniciativa = I-49 - Motor de expresiones paramétricas (Expression Engine, ID22B)
Rama       = architecture/motor-expresiones-parametricas
Worktree   = ~/.codex/worktrees/architecture-motor-expresiones-parametricas
BASE_SHA   = a4d88f18a1f42263d366c44dc05dd18a6786f152  (origin/main)
CLAIM_SHA  = 77262feb18ebd1bb5a842357bd013ad968f80994  (commit vacío)
Claim-Id   = 06aec3d0-0d07-48d4-8b78-d56daeebe903
Override   = OWNER_OVERRIDE_I49_I50_PARALLEL  (docs/automation/decisions/I-49.md)
```

Primer `git push -u origin architecture/motor-expresiones-parametricas` **aceptado sin force**
(`* [new branch]`), con `origin` sin ninguna rama ni commit `Initiative-Id: I-49` en el preflight. `main`
**no fue modificada**.

El resto de la evidencia se acumula al cerrar cada fase.
