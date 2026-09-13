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

> **Estado actual: G4, alineación documental.** El consenso está **congelado** sobre la Proposal V6, con ADR-0038
> **aceptado** por el Owner. La autoridad de este estado es el [Consensus Freeze](I-49-consensus-freeze.md). **No
> existe todavía ninguna línea de producción de I-49.**

```text
G0 = CLOSED
G1 = CLOSED
G2 = CLOSED
G3 = CLOSED

Proposal = V6
ADR-0038 = ACCEPTED
Consensus = FROZEN

G4 = THIS GATE
G5 = NOT STARTED

Production implementation = NOT STARTED
```

> **Autoridades.** [Proposal V6](I-49-proposal-v6.md) (blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`, commit
> operativo `1ed93a0525ec11c5092df89c55cbf498c99f8e2a`), la autoridad técnica detallada;
> [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md) (aceptado en
> `edafade1188e1defae6edb146e61ca9ace34f3a2`), la decisión de arquitectura aceptada; y el
> [Consensus Freeze](I-49-consensus-freeze.md) (`364d6c06e44273a63a7b6f6509daf357611ea77a`). Este contrato **resume**
> y apunta a ellos: ante cualquier discrepancia de contenido técnico, **la Proposal V6 prevalece** sobre este resumen.
> En el proceso hay una sola desviación registrada frente a V6 §8.1 y al Consensus Freeze §4: la fila de ROADMAP no se
> alinea en G4, sino al cierre (sección 3.1).

> **Apertura por autorización explícita del Owner sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) sección 2. Esa autorización sustituye **únicamente** la preexistencia de la
> fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap inmediatamente posterior al
> reclamo atómico. El estado vivo no se lee aquí: se deriva de la existencia de
> `origin/architecture/motor-expresiones-parametricas`.

> **Excepción de proceso histórica y cumplida: `OWNER_OVERRIDE_I49_I50_PARALLEL`.** Permitió reclamar y ejecutar I-49
> en paralelo con I-50. El orden de integración quedó fijado: **I-50 integró primero** (merge `f8deb67`) e I-49, la
> segunda, se rebasó sobre ese `main` en G3A, con las premisas que I-50 podía afectar re-verificadas
> ([Proposal V6](I-49-proposal-v6.md) §12.1); al integrar, I-49 vuelve a reconciliarse con el `main` vigente. El
> registro del Owner se conserva íntegro en [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md), y la
> sección 6.2 reproduce sus condiciones y cómo se cumplieron. Para el código futuro rige el procedimiento general de
> archivos calientes y ramas activas (sección 6.3).

## 1. Objetivo

I-49 entrega una **fundación común de expresiones** —el núcleo neutral `RackCad.Application.Expressions`— y habilita
expresiones en **dos superficies productivas** ([Proposal V6](I-49-proposal-v6.md) DR-1;
[ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md) D1, D10, D11, D12 y D21):

1. la **definición de una variable de proyecto**, `ProjectVariable.Definition`: `Literal` o
   `Expression(BoundExpression)`;
2. la **fuente de una propiedad vinculable**, en el **mismo** `LinkedPropertyEditor` de I-48: `Literal`,
   `ProjectVariableReference(VariableId)` o `Expression(BoundExpression)`.

Se conservan `VariableId` como identidad, la referencia directa histórica —a la que se canonicaliza `=X`, que así
tiene una sola representación persistida—, la separación authored / effective y la separación respecto de ID21. Así
se concreta la dirección escrita desde I-47: el mismo campo que acepta `6` o `=Holgura General` «admitiría más
adelante `=Holgura General + 2`» ([HANDOFF](../HANDOFF.md) sección 4, dirección **no normativa**).

- [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) §15, **aceptado**, situaba ID22B en
  `ProjectVariable.Definition` e ID21 en `PropertyValue<T>`. Por requisito del Owner, **ADR-0038 extiende también el
  lado de la propiedad** (D12): es exactamente la vía que este contrato preveía para apartarse de ese punto de
  extensión, un ADR nuevo aceptado por el Owner.
- **ID21 —referencias Rack→Rack— sigue fuera.** Ninguna expresión puede referenciar una propiedad, y este contrato no
  define cómo funcionará ID21.
- La semántica integrada por I-47 e I-48 se preserva salvo las extensiones que acepta ADR-0038 (sección 3.2).
- El resultado verificable concreto está en la Proposal V6 congelada; este contrato no la sustituye.

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

1. **G1 — Discovery read-only**: **CERRADA** ([I-49-discovery.md](I-49-discovery.md)). Cubrió el estado real de la
   rama, **sin producción**, con lo que el encargo del Owner enumeraba:
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
2. **G2 — Proposal**: **CERRADA** con la **Proposal V6**; V1–V5 quedan como historial.
3. **G3 — Consenso y freeze**: **CERRADA**. `Coordinator = AGREED WITH V6`, `Architect = AGREED WITH V6`, ADR-0038
   **aceptado** por el Owner antes de implementar ([WORKFLOW](../WORKFLOW.md) sección 8) y
   [Consensus Freeze](I-49-consensus-freeze.md) versionado.
4. **G4 — Alineación documental**: gate actual. Alinea este contrato y registra en
   [`docs/ideas-futuras.md`](../ideas-futuras.md) los hallazgos laterales y los seguimientos que fija V6.
   - **Desviación de proceso registrada.** [Proposal V6](I-49-proposal-v6.md) §8.1 y el
     [Consensus Freeze](I-49-consensus-freeze.md) §4 situaban en G4 también la alineación de la fila de ROADMAP.
     [WORKFLOW](../WORKFLOW.md) sección 2 solo permite editar ROADMAP en tres momentos —al planificar, en el bootstrap
     y al integrar o cerrar—, y no en cada gate. Ante ese conflicto, la decisión tomada en G4 fue respetar WORKFLOW:
     **la fila no se toca en G4** y se alinea con V6 y ADR-0038 al cierre, como hizo I-47
     ([decisiones de I-47](../automation/decisions/I-47.md), C3-7).
   - Es una elección de proceso, registrada en el commit de G4, que no cambia ningún contenido técnico de V6 ni de
     ADR-0038.
5. **G5 en adelante — implementación, candidato, validación e integración**, por la secuencia, las evidencias y las
   compuertas de la [Proposal V6](I-49-proposal-v6.md) §8.

### 3.2 Invariantes de entrada — I-47 e I-48

**Las invariantes de I-47/I-48 se preservan salvo las extensiones explícitamente aceptadas por ADR-0038 / Proposal
V6.** En cada fila, lo marcado como **Extensión** o **Revisión acotada** es lo que acepta ADR-0038; el resto de la
invariante sigue intacto. Cualquier otro cambio de una invariante exige ADR nuevo y decisión del Owner, e invalida el
[Consensus Freeze](I-49-consensus-freeze.md). La tabla orienta y no pretende ser exhaustiva: **las fuentes vinculantes
son las enlazadas**, y el detalle de cada extensión está en V6 (§2.3) y en ADR-0038.

| Invariante | Fuente |
|---|---|
| Un único `ProjectVariablesDocument` por DWG: autoridad de nivel dibujo | ADR-0034 §1 |
| `VariableId` estable e inmutable —y con él `SymbolId`— es la autoridad; `Name` es etiqueta mutable y **nunca** es identidad persistida. **Extensión**: existe una ruta controlada nombre→id **solo al escribir y enlazar**, contra el snapshot que el usuario ve; una vez enlazado y persistido, ningún nombre es autoridad, y un rename no rompe ninguna expresión | ADR-0034 §2; ADR-0038 D5, D6 |
| Variables **tipadas** desde el primer día; el discriminador de tipo se persiste, y `VariableType` sigue siendo solo `Length` | ADR-0034 §3; ADR-0038 D2 |
| **Extensión**: `VariableDefinition = Literal \| Expression(BoundExpression)`; antes, solo literal | ADR-0034 §3; ADR-0038 D10 |
| Una propiedad vinculable vale `Literal`, `ProjectVariableReference(VariableId)` o —**extensión**— `Expression(BoundExpression)`, identificada por `PropertyId` estable. `=X` se canonicaliza a la referencia directa: no tiene dos formas persistidas | ADR-0034 §4; ADR-0038 D11, D12 |
| authored ≠ effective: mientras hay binding, el literal authored queda congelado e inactivo | ADR-0034 §5 |
| **Un único punto de resolución**, en Application; dibujo, BOM y preview consumen solo el efectivo y **no conocen `VariableId`**. **Extensión**: `RegistryEvaluation` es el único dueño de los valores, y las expresiones de propiedad se evalúan en ese mismo resolver | ADR-0034 §6; ADR-0038 D16 |
| Propagación **atómica**: preflight completo antes de mutar; un preflight fallido deja un plan **vacío, nunca parcial**. **Extensión**: la profundidad 1 da paso a dependencias y dependientes transitivos sobre un grafo derivado, con **un** `MutationPlan`, cada rack resuelto **una** vez, **una** transacción, **un** commit y **un** `Regen` | ADR-0034 §7; ADR-0038 D15, D23 |
| **Fail-closed**: `UNKNOWN` / `UNREADABLE` / `BROKEN` ≠ `ABSENT` / `EMPTY` / `SUCCESS`. **Extensión**: los fallos se clasifican en estructurales, que no se leen ni se reparan, y semánticos, que se diagnostican con resultado tipado y sin fallback | ADR-0034 §8; ADR-0038 D14 |
| Autoridad multi-vista por `RackId`: divergencia o ilegible ⇒ fail-closed | ADR-0034 §9 |
| Las tres asimetrías: vincular **congela** y desvincular **materializa**; un vínculo roto no se desvincula y solo se repara con aviso explícito; un registro presente e ilegible **nunca** se lee como vacío | ADR-0034 §11; HANDOFF §1 |
| `Delete` bloqueado con consumidores; **sin** fallback automático al literal authored. **Extensión**: cuentan como consumidores también las expresiones de propiedad que dependen de la variable, y los dependientes variable→variable también bloquean; en `UnlinkAllAndDelete`, las expresiones consumidoras y las definiciones dependientes bloquean, y solo las referencias directas se materializan | ADR-0034 §11; ADR-0038 D17 |
| Reparación **explícita y atómica por rack**. **Extensión**: `RepairBrokenRack` repara también los fallos semánticos entendidos; lo estructural sigue fail-closed y sin reparación destructiva; la confirmación muestra cada fuente, y una `RepairDecisionObservation` protege hasta el commit la premisa de cada fuente que retira | ADR-0034 §11; decisiones de I-48 §4; ADR-0038 D18, D20 |
| Versionado propio del registro; `major` superior, `VariableType` desconocido y `Definition.kind` desconocido = **ERROR**. **Extensión (Schema V-0)**: sin cambio de versión, con `expression` fail-closed en las builds anteriores | ADR-0034 §13; decisiones de I-47 C4-8 y C4.6-8; ADR-0038 D13 |
| La UI permanece AutoCAD-free; el Plugin es el único dueño del NOD, los `ObjectId` y la `Transaction` | ADR-0034 §14 |
| `Link` congela el último literal **comprometido**; un vínculo roto es fail-closed; reparabilidad **atómica por rack** | decisiones de I-48 §4 (Proposal V8) |
| `VariableId` duplicado = **identidad ambigua ⇒ fail-closed**, sin elegir first/last y sin reparar el registro | decisiones de I-48 §4 |
| Lookup de `VariableId` unificado; semantic-usability acreditada también sobre la re-lectura de commit, antes de `ApplyTo`. **Extensión**: esa re-lectura valida además los ids y las observaciones del `PlanReadSet`, y todo plan con observaciones re-lee aunque no lleve `RegistryMutation` | decisiones de I-48 §4; ADR-0038 D19 |
| **Revisión acotada: V8-R05 no queda intacta.** La Proposal V8 de I-48 dejó la concurrencia fuera de alcance; I-49 **la revisa de forma ACOTADA** mediante el `PlanReadSet`, sin hash, token global, comparación completa ni locks. El detalle está en V6 P21.6 | I-48 Proposal V8 §6; ADR-0038 D19, D20 |
| El `Type` persistido se interpreta con un **mapping único** compartido | decisiones de I-48 §4; HANDOFF §1 |
| `LinkedPropertyEditor`: el mismo campo acepta literal o `=Nombre`, con autocompletado **sin auto-selección**, desambiguador **obligatorio** con nombres duplicados y campos vinculables dentro del protocolo C4 (`TryStage` / `ApplyStaged`). **Extensión**: el **mismo** campo acepta `6`, `=Holgura`, `=Holgura + 2` y `=(AlturaBase + Holgura) / 2`, conservando borrador ≠ comprometido, Enter explícito, Escape y LostFocus sin cambio de fuente; `CASO_6` y `CASO_8` cambian deliberadamente | HANDOFF §1; ADR-0038 D21 |

**No se toca** (V6 §2.3): la autoridad y el nodo del registro, la identidad `VariableId`, authored ≠ effective, el
congelado del literal comprometido (regla 20.13), la autoridad multi-vista, la identidad ambigua fail-closed, el mapping
único del `Type`, la reparación atómica por rack, la regla R1 y la decisión C4-13 de I-47, las constantes de dominio de
los sistemas de rack y los censos de comandos y ventanas.

### 3.3 Preguntas que la Proposal tenía que responder

Las que la documentación vigente ya atribuía a ID22B (sección 2), más las que identificó el Discovery. **Las responde la
Proposal V6**, cuya §11.1 enlaza cada pregunta del Discovery con su respuesta; este contrato sigue sin responderlas por
su cuenta:

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
- Todo lo que [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md) D25 declara fuera de
  alcance.
- **De G0 a G4**: parser, AST, evaluador, grafo, schema de expresiones y UI productiva, y cualquier cambio
  en `src/`, `tests/`, `assets/`, `eng/`, `deploy/`, `tools/` o `.github/`.
- Los hallazgos laterales se registran en [ideas-futuras.md](../ideas-futuras.md); **no se arreglan aquí**.

> **Nota de trazabilidad.** En el bootstrap, `ID20` y «Custom BOM» **no tenían definición en el repositorio**: una
> búsqueda sobre todo el árbol de la base no devolvía ninguna coincidencia, y se registraron **tal como los nombraba el
> Owner**. Desde G1 los nombra la entrada vinculante del Owner que recoge el [Discovery](I-49-discovery.md) §1 —ID20,
> Computed / Built-in Parameters; ID23, Custom / Calculated BOM—, y
> [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md) D24–D25 fija qué prepara I-49 y qué
> queda fuera. Este contrato sigue sin inventarles contenido: su frontera productiva es del Owner y de esas
> iniciativas.

## 5. Contexto requerido

- [Proposal V6](I-49-proposal-v6.md), [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md)
  y el [Consensus Freeze](I-49-consensus-freeze.md): autoridad técnica, decisión aceptada y consenso congelado.
- [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) — autoridad drawing-level y punto de
  extensión de ID22B. **Aceptado, y por tanto inmutable en su contenido**; ADR-0038 lo extiende.
- [Contrato de I-47](I-47-project-variables-foundation.md), [Proposal V4](I-47-proposal-v4.md) §10 y
  [decisiones de I-47](../automation/decisions/I-47.md) (C2-1, C2-8, C4-8, C4.6-8).
- [Contrato de I-48](I-48-generic-linked-property-editing.md), [Proposal V8](I-48-proposal-v8.md) y
  [decisiones de I-48](../automation/decisions/I-48.md).
- [HANDOFF](../HANDOFF.md) sección 1 (I-47 e I-48 tal como quedaron integradas) y sección 4 (dirección de
  UX no normativa y compatibilidad con ID22B).
- [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) — `OWNER_OVERRIDE_I49_I50_PARALLEL`, ya
  cumplido, y la aceptación de ADR-0038.
- [WORKFLOW](../WORKFLOW.md) secciones 2, 3, 4 y 7; [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) secciones 6, 11 y
  12; [AGENTS.md](../../AGENTS.md).
- **I-50, integrada en `main`** (merge `f8deb67`): su contrato, su Proposal V1.2 y
  [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md) viven en `main`. Ya no existe rama de I-50 que
  consultar.
- Context Packs declarados en el frontmatter, por el área que cubre el Discovery: `architecture-kernel`
  (resolución, preflight y plan de mutación), `persistence` (registro, schema y property values),
  `ui-editors` (`LinkedPropertyEditor` y C4), `autocad-plugin` (`RACKVARIABLES`, propagación y
  save/reload) y `system-selective` (las propiedades vinculables vigentes).

## 6. Dependencias, paralelismo y coordinación

### 6.1 Dependencias

- **I-47 integrada y cerrada** (2026-09-09): fundación ID22A. Sin ella esta iniciativa no tiene objeto.
- **I-48 integrada y cerrada** (2026-09-12): edición vinculable reusable, `LinkedPropertyEditor` y C4.
- **Entradas del Owner**: ADR-0038 **aceptado** en G3 ([`decisions/I-49.md`](../automation/decisions/I-49.md) sección
  11). Queda su validación manual en G12 ([Proposal V6](I-49-proposal-v6.md) §8.3).
- **I-50 no fue dependencia** en ningún sentido, y ya está integrada en `main`.

### 6.2 `OWNER_OVERRIDE_I49_I50_PARALLEL` — histórico y cumplido

**Qué fue.** Una excepción de proceso del Owner, **solo para el par I-49 / I-50**, que sustituyó el gate de conflicto
que I-50 declaraba entonces (`conflicts_with: [I-49]`) y permitió reclamar y ejecutar I-49 en paralelo con I-50. El
registro completo sigue en [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md), cuyas secciones 1–9
no se modifican.

**Condiciones del Owner**, reproducidas literalmente como registro histórico:

- NO modificar rama ni worktree de I-50.
- NO detener ni alterar su corrida actual.
- Si I-49 detecta que necesita editar un archivo productivo que I-50 está modificando materialmente,
  DETENERSE antes de editarlo y reportar la colisión.
- `LinkedPropertyEditor`, Project Variables y Expression Engine pertenecen funcionalmente al alcance de
  I-49; I-50 tiene prohibido refactorizarlos salvo necesidad demostrada.
- Integración I-49/I-50 será serializada; la segunda que integre deberá reconciliarse con `main`.
- La contradicción documental de I-50 se corregirá posteriormente en su propia iniciativa.
- Registra esta excepción en el contrato/bootstrap de I-49 para que sea durable y auditable.

**Cómo se cumplieron.**

- I-49 no modificó la rama ni el worktree de I-50, ni detuvo ni alteró su corrida.
- No hubo colisión productiva: I-49 no ha escrito producción, así que la condición de colisión no llegó a activarse.
- I-50 integró sin cambiar el editor vinculable, Project Variables ni el núcleo de expresiones
  ([Proposal V6](I-49-proposal-v6.md) §12.1). La propiedad funcional de esas áreas sigue en la sección 6.3.
- El orden de integración quedó fijado: **I-50 integró primero** (merge `f8deb67`), y su rama remota ya no existe.
  I-49, la segunda, se reconcilió con ese `main`: se rebasó en G3A con la Proposal V6 byte-idéntica, y las premisas que
  I-50 podía afectar se re-verificaron contra `f8deb67` (V6 §12.1: `Describe` de `RackSelectiveWindow`, `FootInches` y
  `PropertyValues` junto a `DimensionViews`). Al integrar, I-49 vuelve a reconciliarse con el `main` vigente
  (sección 6.3).
- I-50 corrigió su contradicción documental en su propia iniciativa: su contrato en `main` tiene `conflicts_with: []`
  y su fila de ROADMAP ya no lista I-49 en «Se estorba con» (su decisión `CD-10`). I-49 no la editó.
- La excepción quedó registrada en el bootstrap de este contrato y se conserva en esta sección.

**Por qué `conflicts_with` sigue vacío.** La relación con I-50 nunca fue estorbo ni dependencia, sino coordinación por
archivos compartidos, y con I-50 integrada no queda ninguna rama con la que declararla.

### 6.3 Archivos calientes y ramas activas

- **Para el código futuro rige el procedimiento general.** Antes de editar **cualquier** archivo productivo, cada gate
  de G5 a G10 comprueba el `main` vigente y las ramas activas y aplica el procedimiento de colisión de
  [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) sección 8 contra las iniciativas activas, con
  la serialización de archivos calientes de [WORKFLOW](../WORKFLOW.md) sección 7 ([Proposal V6](I-49-proposal-v6.md)
  §8.2). Una modificación material ⇒ **detenerse antes de editar** y reportar.
- **Cruces conocidos para la implementación**, si aplican al llegar a cada gate
  ([Consensus Freeze](I-49-consensus-freeze.md) §5): `RackSelectiveWindow.xaml.cs` se serializa con el G5 de la unidad
  I-53S de I-53; la prueba de supervivencia de `expression` en la duplicación se escribe contra `RackDuplicationPlan` y
  el restamp vigentes en `main` (I-52); y las guardas de censo de comandos y ventanas se escriben contra el censo
  vigente al rebasar.
- **Composición con `DimensionViews` de I-50**: requisito heredado de implementación. Un Selectivo con una fuente
  `expression` en `PropertyValues` y `DimensionViews` presentes sobrevive a RACKEDITAR y a guardar y reabrir conservando
  ambos conceptos ([ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md), Consecuencias).
- **Propiedad funcional**: `LinkedPropertyEditor`, Project Variables y el Expression Engine son alcance de I-49.
- **Integración serializada**: nunca simultánea con otra iniciativa; la que integre después se reconcilia con `main`
  en su rebase final ([WORKFLOW](../WORKFLOW.md) sección 4.5).
- **`docs/ROADMAP.md`**: cuando varias iniciativas insertan su fila en el mismo punto, el conflicto es documental y
  previsto: quien integre después conserva todas las filas en orden numérico.

### 6.4 I-51

`feature/rackduplicar-multiples-origenes` (**I-51**) está **integrada y cerrada** en `main` (merge `46fcac2`). Su
producción integrada no la cita ni la modifica la Proposal V6 (§12.2).

## 7. Archivos esperados

La superficie de producción la fija la **Proposal V6** —entregables por gate en §8.1 y estimación en §10.2—, no este
contrato. **De G0 a G4 todo es documental**: ningún gate toca `src/`, `tests/` ni `assets/`. G0 tocó exactamente tres
archivos: este contrato, [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) y la fila de I-49 en
[`docs/ROADMAP.md`](../ROADMAP.md). G1–G3 añadieron el Discovery, las Proposals V1–V6, ADR-0038 y el Consensus Freeze,
y G3 actualizó además [`docs/adr/README.md`](../adr/README.md) y
[`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md) (secciones 10 y 11). G4 toca este contrato y
[`docs/ideas-futuras.md`](../ideas-futuras.md). Una desviación material frente a V6 obliga a detenerse (sección 12).

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atómico publicado; contrato, decisión del Owner y fila en ROADMAP | **CERRADA** |
| G1 | Discovery read-only | [I-49-discovery.md](I-49-discovery.md): rutas, símbolos, contratos y riesgos | **CERRADA** |
| G2 | Proposal | Proposals V1–V5 como historial → **Proposal V6** (blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`) | **CERRADA** |
| G3 | Consenso + freeze | `Coordinator = AGREED WITH V6` y `Architect = AGREED WITH V6`; ADR-0038 aceptado (`edafade`); [Consensus Freeze](I-49-consensus-freeze.md) (`364d6c0`) | **CERRADA** |
| G4 | Alineación documental | Este contrato y los registros de `docs/ideas-futuras.md`; la fila de ROADMAP se alinea al cierre ([WORKFLOW](../WORKFLOW.md) sección 2) | **CERRADA cuando el CI de este SHA exacto de G4 esté verde** |
| G5–G12 | Implementación, candidato, validación e integración | Secuencia, evidencias y compuertas de [Proposal V6](I-49-proposal-v6.md) §8 | **NO INICIADAS** |

```text
G4 = CLOSED when CI of this exact G4 SHA is green
G5 = READY only after G4 evidence is green
```

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable. Antes del primer gate productivo, además,
la rama se reconcilia con el `main` vigente según WORKFLOW ([Proposal V6](I-49-proposal-v6.md) §8.2 y §12.2;
[Consensus Freeze](I-49-consensus-freeze.md) §5).

## 9. Pruebas y builds

Las fija la **Proposal V6**: familias y escenarios de P28 y evidencia exigida por gate en §8.1. Lo exigible por norma,
además: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto** y builds Debug de
UI y de Plugin ([AGENTS.md](../../AGENTS.md), «Pruebas — definicion de terminado»;
[WORKFLOW](../WORKFLOW.md) sección 5). Se hereda también la prueba de composición `expression + DimensionViews`
(sección 6.3). De G0 a G4 no hay código: su evidencia es documental.

## 10. Validación manual

**Requerida** en cuanto la implementación cambie comportamiento de dibujo: una variable definida por
expresión gobierna el valor efectivo que consumen geometría y BOM. El **checklist propuesto está en la Proposal V6**
§8.3 (G12), en AutoCAD 2025. De G0 a G4 **no** se requiere: no tocan producto.

## 11. Criterios de aceptación

Los criterios funcionales concretos los fija la Proposal V6 congelada. Con independencia de ella:

1. Las invariantes de la sección 3.2 siguen valiendo, **verificadas**, tras la implementación.
2. La capacidad de expresión vive en `ProjectVariable.Definition` **y** en la fuente `Expression` de una propiedad
   vinculable, según [ADR-0038](../adr/0038-motor-expresiones-parametricas-y-edicion-formula-aware.md): es el ADR nuevo
   aceptado por el Owner que este criterio preveía. Siguen fuera Rack→Rack (ID21), ID20 productivo, Custom BOM
   productivo, sistemas de rack nuevos u otros sistemas, y nuevas propiedades vinculables.
3. No aparece Custom BOM, ni ID20 productivo, ni referencias Rack→Rack.
4. Las condiciones de `OWNER_OVERRIDE_I49_I50_PARALLEL` constan como cumplidas (sección 6.2), y desde la integración de
   I-50 toda colisión productiva material con una iniciativa activa se reporta **antes** de editar (sección 6.3).
5. El orden respecto de I-50 ya está fijado: I-50 integró primero e I-49 se rebasó sobre ese `main` en G3A. Hacia la
   implementación queda preservar y componer `DimensionViews` (sección 6.3); al integrar, I-49 vuelve a reconciliarse
   con el `main` vigente, y su integración sigue serializada respecto de las demás iniciativas activas.

## 12. Condiciones para detenerse

- **Consensus Freeze vigente.** La autoridad es la Proposal V6 congelada con ADR-0038 aceptado. Un cambio material que
  altere V6, cualquiera de las decisiones D1–D25 de ADR-0038 o una decisión cerrada **invalida el freeze**: rige la
  regla de invalidación del [Consensus Freeze](I-49-consensus-freeze.md) §3, y el contrato **no** se corrige en
  silencio desde la implementación. Siguen vigentes, además, las condiciones de la [Proposal V6](I-49-proposal-v6.md)
  §13.
- **Colisión productiva material** con una iniciativa activa: procedimiento de archivos calientes y ramas activas de
  la sección 6.3; **detenerse antes de editar** si el cruce es material.
- Si la implementación muestra que hace falta **cambiar** una invariante de la sección 3.2 más allá de las extensiones
  que acepta ADR-0038: **detenerse**. Eso es ADR nuevo y decisión del Owner, no un ajuste de alcance.
- Si el alcance empieza a incluir Custom BOM, ID20 productivo o referencias Rack→Rack: **detenerse**.
- Si la frontera de `ID20` o de «Custom BOM» resulta necesaria para decidir algo: **preguntar al Owner**, no
  suponerla.
- Parser, AST, evaluador, grafo, schema de expresiones o UI productiva **fuera de la secuencia de gates de V6 §8**
  —antes de G5 o sin la evidencia verde del gate anterior—: prohibido.
- Cualquier edición de la rama o del worktree de otra iniciativa: prohibida.
- **Histórico, ya satisfecho**: la compuerta «NO IMPLEMENTATION BEFORE CONSENSUS» se cumplió con el consenso sobre V6,
  ADR-0038 aceptado y el Consensus Freeze versionado (sección 8).

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-49.yml`: la automatización está **desactivada** (`automation.enabled: false`)
y [WORKFLOW](../WORKFLOW.md) sección 8 exige para el bootstrap del caso (d) exactamente **fila en ROADMAP
más contrato**, no un archivo de estado.

**Sí existe** [`docs/automation/decisions/I-49.md`](../automation/decisions/I-49.md), porque hay decisiones del Owner
que registrar: el override de paralelismo con I-50, ya cumplido (secciones 1–9), y la aceptación de ADR-0038 (sección
11). Es el canal que fijan [initiatives/README](README.md), TEMPLATE sección 13 y
[AUTOMATION_PLAN](../AUTOMATION_PLAN.md) sección 11. `requires_owner_decision: true` lo declara, y esa
metadata solo añade. El consenso congelado vive en [`I-49-consensus-freeze.md`](I-49-consensus-freeze.md).

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
**no fue modificada**. Tras el rebase de G3A, el reclamo es `76b7f6356af1d48a8835c0494a7baee81b1d6df0`, con el mismo
mensaje y el mismo `Claim-Id`.

**Discovery, Proposal y consenso (G1–G3).** SHAs operativos tras el rebase de G3A sobre `main` `f8deb67`:

```text
G1  Discovery          = ac9cceeddb4f3466e7f87b04420121a20be370ae
G2  Proposal V1..V5    = ed0f981 · e7bfc70 · 83d1f9b · 502447d · 6e2b3bc   (historial)
    Proposal V6        = 1ed93a0525ec11c5092df89c55cbf498c99f8e2a   (histórico 048a508)
    V6 blob            = ef4db3aa400483ff25a8f39b2beb93708fa43d1a
G3  ADR-0038 propuesto = a1605600d0951a87ae218724d82bec57bc3be723
    ADR-0038 aceptado  = edafade1188e1defae6edb146e61ca9ace34f3a2
    Consensus Freeze   = 364d6c06e44273a63a7b6f6509daf357611ea77a
```

El resto de la evidencia se acumula al cerrar cada fase.
