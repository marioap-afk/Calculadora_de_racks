---
schema: rackcad-initiative/v1
id: I-47
title: "Variables de proyecto: fundacion de autoridad drawing-level"
type: architecture
status: integrated
branch: architecture/project-variables-foundation
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [architecture-kernel, persistence, autocad-plugin]
automation_state_path:
decision_paths: [docs/automation/decisions/I-47.md]
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

# Variables de proyecto: fundacion de autoridad drawing-level (ID22A)

> **Fase actual: INTEGRADA y CERRADA el 2026-09-09.**
>
> ```
> PRE_MERGE_MAIN_SHA       = 306e18ed4676e5e96b54d59402c9a230efb137d3
> FUNCTIONAL_CANDIDATE_SHA = af572393dab5c755a8f272c746bdce20848b7dd0
> CLOSURE_DOCS_SHA         = 31d7f3b44d503f32c1b40c809532d5e7193e8a75   (docs-only)
> MERGE_SHA                = 507921f4839f59219928a42beb146d285225579c   (--no-ff, dos padres)
> ```
>
> `G1`-`G18` verdes. Core **5253/5253**, UI **1284 PASS / 17 omitidas / 1301**, Debug de UI y de Plugin
> sin errores (solo los dos `MSB3277` conocidos). **CI de `push` 4/4 `success`** sobre ese SHA exacto
> (corrida **34427341491**) y **cobertura del Candidato** por dispatch **34427649290**, con su artifact
> `rackcad-coverage-cobertura`. **Validacion manual del Owner en AutoCAD 2025: PASS** — `PV-1..PV-5`,
> `PV-17.1..PV-17.8` y `OV-1..OV-9`, incluida la reparacion de un vinculo roto **legitimo** obtenido
> copiando un rack vinculado a un DWG sin registro (`RACKEDITAR` falla cerrado; `RepairBroken` quita el
> vinculo y deja gobernando el literal almacenado).
>
> **Compuertas posteriores al merge PASADAS:** `origin/main` no avanzo desde la base, asi que no hubo
> rebase final; el CI sobre el `MERGE_SHA` —corrida **34430762596**— quedo **4/4 `success`** y produjo el
> artifact `rackcad-coverage-cobertura` por ser trunk, con lo que la comprobacion diferida de cobertura
> tambien queda cubierta; suites locales sobre el merge **Core 5253/5253** y **UI 1284 PASS / 17 omitidas
> / 1301**. Rama y worktree **retirados**. El estado vivo esta en [HANDOFF](../HANDOFF.md) seccion 4. La **UX futura tipo Excel** para
> propiedades vinculables es **direccion NO normativa** y esta registrada en
> [ideas-futuras.md](../ideas-futuras.md); **no forma parte de este contrato**.
>
> El historial de fases previas se conserva abajo sin cambios.
>
> ---
>
> **Fase anterior: CONSENSUS FREEZE COMPLETO.** Este contrato nacio en
> el bootstrap posterior al reclamo atomico —caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2— y
> describia entonces **solo la fase Discovery**. La actualizacion de CF-1 lo puso al dia con el
> **alcance final aprobado**; esta lo cierra: **el Owner acepto ADR-0034 el 2026-09-09**.
>
> ```
> Reclamo atomico:  6e17bd5   (commit vacio, Claim-Id 73672933-f052-46af-b3c0-8091f0add299)
> Base:             origin/main 306e18ed4676e5e96b54d59402c9a230efb137d3
> Proposal:         I-47-proposal-v4.md — VERSION V4.8
> Proposal SHA:     a0621abbd22952ad5a62bf7678212a74526a05ce
> ```
>
> **Estado de las fases y de las compuertas:**
>
> | | Estado |
> |---|---|
> | **Discovery** | **CERRADO** — entregado en [I-47-discovery.md](I-47-discovery.md) |
> | **Proposal V4.8** | **consenso tecnico CERRADO** sobre el SHA de arriba |
> | **CF-1** — el contrato refleja el alcance final | **SATISFECHA** |
> | **CF-2** — el ADR existe antes de implementar, y el Owner lo acepta | **SATISFECHA** — **ADR-0034 nacio `propuesto` y el Owner lo ACEPTO el 2026-09-09** |
> | **CF-3** — prerequisitos reconocidos como trabajo | **SATISFECHA** |
> | **CF-4** — ambos revisores AGREED sobre el mismo SHA | **SATISFECHA** |
> | **CONSENSUS FREEZE** | **COMPLETO** |
> | **IMPLEMENTACION** | **AUTORIZADA A COMENZAR** — **no iniciada** |
>
> **Los dos veredictos, sobre exactamente el mismo SHA:**
>
> ```
> Coordinator=AGREED — Proposal V4.8 / SHA a0621abbd22952ad5a62bf7678212a74526a05ce
> Architect=AGREED   — Proposal V4.8 / SHA a0621abbd22952ad5a62bf7678212a74526a05ce
> ```
>
> **CONSENSUS FREEZE COMPLETO; IMPLEMENTACION AUTORIZADA A COMENZAR.** CF-2 la satisface un acto del
> **Owner**, no de los revisores: el consenso Coordinador ↔ Arquitecto decia que la decision estaba
> **lista** para implementarse, y la aceptacion es lo que la autoriza.
>
> **Congelar el contrato no es haberlo implementado.** La implementacion **no ha comenzado**, no hay
> Candidate, no hay merge, y **la validacion manual del Owner en AutoCAD 2025 no se ha ejecutado**:
> `requires_plugin_build`, `requires_autocad` y `requires_owner_validation` siguen vigentes.
> **I-47 no esta cerrada y la produccion no esta validada.**

## 1. Objetivo

Fundar en RackCad una **autoridad de variables de proyecto de nivel dibujo**: un unico registro por
DWG cuyo valor gobierna a todos los racks que lo referencian, de modo que cambiarlo actualice lo ya
dibujado en **una sola operacion**.

«Variable de proyecto» es un valor que **todos los racks de un mismo dibujo comparten por decision del
proyecto**, no por coincidencia de configuracion. El objetivo no es calcular con esos valores, sino
fijar **donde viven, quien manda cuando discrepan, y que pasa con lo ya dibujado cuando uno cambia**.

El **vertical slice** de ID22A es **una sola propiedad**: `selective.verticalClearance`.

## 2. Alcance FINAL aprobado (Proposal V4.8)

> Las tres preguntas de Discovery —D1 autoridad drawing-level, D2 descubrimiento y redibujo, D3 vertical
> slice— estan **respondidas y cerradas** en [I-47-discovery.md](I-47-discovery.md). Esta seccion las
> sustituye por el **alcance de implementacion**. Es un **resumen normativo**: la fuente completa y
> vinculante es el [Proposal V4.8](I-47-proposal-v4.md), y ante cualquier discrepancia **manda el
> Proposal**.

### 2.1 Registro de nivel dibujo

Un unico **`ProjectVariablesDocument` por DWG**, persistido a nivel dibujo mediante **NOD/Xrecord**
segun el Proposal.

```
entrada del NOD AUSENTE            -> registro VACIO (legado valido)
entrada PRESENTE pero ilegible     -> ERROR VISIBLE · sin registro vacio · SIN WRITE
```

### 2.2 Variables

`VariableId` **GUID estable e inmutable**; `Name` **mutable** y sin papel en la resolucion. **Tipadas
desde el dia 1**; ID22A soporta **unicamente `Length`** con **definicion literal**. **Sin formulas**.

### 2.3 Binding

Una propiedad es conceptualmente `Literal(T)` **o** `ProjectVariableReference(VariableId)`. Propiedad
piloto: **`selective.verticalClearance`**.

- El **literal authored queda congelado** mientras exista binding.
- **La referencia gobierna el valor efectivo.**
- Un **`VariableId` inexistente es ERROR VISIBLE**: **nunca** hay fallback silencioso al literal.

### 2.4 Authored frente a effective

| | Que es |
|---|---|
| **Authored** | `SelectivePalletDesignDocument` — la autoridad **persistida** |
| **Effective** | `SelectivePalletDesign` — el diseno **geometrico** que gobierna |

La resolucion es **central y en Application**: `authored + ProjectVariables -> effective`. **Geometria
y BOM no conocen `VariableId`.**

### 2.5 Operaciones

`CreateVariable` · `Rename` · `ChangeValue` · `Delete` **bloqueado si hay consumidores** · `Link` ·
`Unlink` · `RepairBroken` · `UnlinkAllAndDelete`.

**`Rename` preserva las referencias** (van por `VariableId`). **`Unlink` saludable materializa el
efectivo actual**, de modo que su efecto geometrico es nulo. **`RepairBroken` exige accion explicita y
puede cambiar la geometria**, con aviso previo.

### 2.6 Propagacion

Un cambio de variable: descubre consumidores · fija la autoridad multi-vista · **preflight semantico**
(puro) · **preflight fisico** (AutoCAD, read-only) · **una** operacion transaccional que actualiza
payloads y geometria · **un** commit · **un** `Regen`.

**Ningun import ni reparacion best-effort dentro del lote.**

### 2.7 Multi-vista

**Una autoridad authored logica por `RackId`.** Las operaciones **target-variable** usan un probe
**tri-estado** por sibling —`POSITIVE` / `NEGATIVE` / `INDETERMINATE`— con la regla
**`UNKNOWN != NEGATIVE`**. Las operaciones **target-rack** exigen la autoridad de **todas** las
siblings presentes. Divergencia o ilegible ⇒ **fail-closed**.

### 2.8 BOM

`RACKBOMTOTAL` reune **todas** las siblings, valida la autoridad authored y usa **un unico snapshot**
de `ProjectVariables` en todo el total. El **effective resolver corre UNA sola vez dentro del handler
Selectivo**.

```
BrokenProjectVariableReference  -> resultado semantico TIPADO -> ABORTA EL TOTAL
outer envelope ilegible COLOCADO                              -> ABORTA EL TOTAL
outer envelope ilegible NO colocado                           -> se ignora
```

### 2.9 Duplicacion

Copia independiente: **`RackId` nuevo**, **`Name` nuevo**, **mismo `VariableId`**, `PropertyValues`
preservado, schema y `ExtensionData` preservados. El restamp es **completo o NO COPY**: **nunca
identidad parcial**.

### 2.10 Biblioteca

El export de un Selectivo vinculado **resuelve el efectivo, materializa el literal, elimina
`PropertyValues`** y escribe el `SelectiveRack` anidado **literal-only en linea `1.x`**. Un **binding
roto no produce artefacto**. Un schema anidado incompatible **no se ofrece como diseno abrible normal**
y **si produce diagnostico visible**.

### 2.11 Interfaz de usuario — DENTRO del alcance

La implementacion **si incluye**: superficie **central de Project Variables**; `create` / `edit` /
`rename` / `delete`; seleccion y vinculacion para la **propiedad piloto**; `unlink`; **resumenes de
consumidores**; y **`RepairBroken` explicito**.

**La UI permanece AutoCAD-free**: el Plugin entrega **DTO puros** y recibe **intents**. **No** se crea
un panel generico de propiedades.

### 2.12 Persistencia y schema

`ProjectVariablesDocument`: **`SchemaVersion 1.0`**, las variables, y `ExtensionData`.

Regla **C4.8-1**:

```
READ           : NO inventar una SchemaVersion ausente
CREATE / WRITE : estampar EXPLICITAMENTE CurrentSchemaVersion

missing != explicit "1.0"
```

El **sticky schema del Selectivo** rige segun el Proposal.

### 2.13 Validacion del Owner

La implementacion **requerira validacion manual proporcional en AutoCAD 2025 antes de integrar**
(`requires_autocad: true`, `requires_owner_validation: true`). **Esa validacion NO se ha ejecutado** y
este contrato **no** la da por hecha.

## 3. Fuera de alcance FINAL — explicito

> **Actualizado por CF-1.** La version anterior excluia **UI, comando nuevo y BOM**, porque describia
> la fase Discovery. **Los tres estan ahora DENTRO del alcance** (seccion 2). Lo que queda fuera es:

- **Formulas, parser y AST** de cualquier clase.
- **ID22B** completa.
- **Referencias entre variables.**
- **Referencias a propiedades de racks** e **ID21** completa.
- **Grafo de dependencias persistente.**
- **Deteccion de ciclos.**
- **Conversion masiva de propiedades** — el slice es **una**.
- **Conversion de otros sistemas** (Dinamico, Push Back, Cantilever, Cama, Cabecera).
- **Panel generico de «smart properties».**
- **WBLOCK y cualquier escenario cross-DWG.**
- **Transferencia o fusion de variables entre dibujos.**
- **Golden DWG** como parte de I-47.
- **Refactors amplios** no necesarios para este alcance.
- **Optimizaciones** no necesarias para el slice.

Los hallazgos laterales se registran en [ideas-futuras.md](../ideas-futuras.md); **no se arreglan de
paso**.

## 4. Restricciones que ya rigen y no se reabren

- **Direccion de dependencias** (AGENTS, convencion 1): nada de AutoCAD fuera del Plugin. Una
  autoridad de nivel dibujo que se implemente algun dia tendra su **lectura y escritura en el
  Plugin** y su **modelo en Domain/Application**; el Discovery debe reportar contra esa frontera.
- **Regla en un solo sitio** (AGENTS, convencion 2): si dibujo, BOM y UI deben coincidir en un
  numero, la regla vive en UNA funcion de Application. Toda duplicacion que el Discovery encuentre se
  reporta como hallazgo, no se corrige aqui.
- **Persistencia versionada** (AGENTS, convencion 4): todo campo nuevo nace nullable con fallback
  legado y test de round-trip. El Discovery debe decir si las tres variables de D3 cumplen eso hoy.
- La **pulgada** es la unidad geometrica interna ([ADR-0005](../adr/0005-estrategia-de-unidades.md));
  ninguna conversion se introduce aqui.

## 5. Entregables

### 5.1 Fase DISCOVERY — **entregada** ([I-47-discovery.md](I-47-discovery.md))

D1, D2 y D3 respondidas con evidencia del arbol, mas riesgos y hallazgos fuera de alcance.

### 5.2 Fase PROPOSAL — **cerrada en V4.8** ([I-47-proposal-v4.md](I-47-proposal-v4.md))

Contrato completo con sus decisiones, alternativas descartadas, pruebas y condiciones de Freeze.
Recorrio **nueve versiones y cuatro revisiones adversariales del Arquitecto**; el registro completo
vive en [`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md).

**Consenso tecnico alcanzado sobre `a0621abbd22952ad5a62bf7678212a74526a05ce`.**

### 5.3 Fase CONSENSUS FREEZE — **COMPLETA**

- **CF-1** — la actualizacion de alcance de este contrato. **Satisfecha.**
- **CF-2** — [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md), creado en estado
  **`propuesto`** y **ACEPTADO por el Owner el 2026-09-09**, sobre el contenido de
  `6eafd590017278767288b15cdcd9d651c293d5e6`. **Satisfecha.**
- **CF-3** — satisfecha por el Proposal.
- **CF-4** — satisfecha por los dos AGREED.

El registro del acto vive en
[`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md).

### 5.4 Fase IMPLEMENTACION — **AUTORIZADA A COMENZAR**, no iniciada

La aceptacion de ADR-0034 **autoriza el arranque**; **no** lo ejecuta. Cuando comience, arrastra los
gates de ejecucion del frontmatter: build del Plugin, AutoCAD y validacion manual del Owner. **A la
fecha de este documento no se ha escrito ninguna linea de produccion para I-47**: no hay Candidate,
no hay merge y no hay validacion del Owner.

## 6. Gates

| Gate | Estado | Motivo |
|---|---|---|
| `owner-decision` | **RESUELTO — APROBADO** | **ADR-0034 ACEPTADO por el Owner el 2026-09-09.** El campo `requires_owner_decision` se mantiene `true`: declara que la iniciativa **necesitaba** la decision e identifica donde ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) seccion 11); lo que se resuelve es el **gate** |
| `owner-validation` | **aplicara** | El alcance final cambia dibujo, BOM y persistencia ⇒ `requires_owner_validation: true`. **No ejecutada** |
| `autocad` | **aplicara** | `requires_autocad: true` desde esta actualizacion |
| `plugin-build` | **aplicara** | `requires_plugin_build: true` desde esta actualizacion |
| CI | aplica | Toda punta empujada se mide como cualquier otra |

**Consensus Freeze COMPLETO. Implementacion AUTORIZADA A COMENZAR.** Lo que **sigue sin declararse**
es la produccion: no hay implementacion, ni Candidate, ni merge, ni validacion en AutoCAD. Los tres
gates de ejecucion siguen **vigentes**.

## 7. Bitacora

| Fecha | Hito |
|---|---|
| 2026-09-08 | Reclamo atomico `6e17bd5` aceptado por el remoto; bootstrap (contrato + fila en ROADMAP + registro de autorizacion) |
| 2026-09-08 | Discovery entregado (`9e25d52`): D1/D2/D3 con evidencia, 10 riesgos, 11 hallazgos fuera de alcance |
| 2026-09-08 | **Gate C**: el dueno fija C-1..C-6 y autoriza la Proposal V1 documental. WBLOCK/copia entre dibujos queda **diferido**. Sin Architect Review |
| 2026-09-09 | **CF-1 SATISFECHA**: este contrato pasa de Discovery-only al **alcance final** de Proposal V4.8 — UI, comando y BOM entran; gates de ejecucion a `true`. **CF-2**: se crea **ADR-0034** en `propuesto`. **Produccion sigue BLOQUEADA** |
| 2026-09-09 | **Consenso tecnico cerrado sobre `a0621ab`**: `Coordinator=AGREED` y `Architect=AGREED` sobre Proposal **V4.8**. CF-3 y CF-4 satisfechas |
| 2026-09-08 | **Gate C2**: el dueno define **ID22B** (formulas) e **ID21** (refs a propiedades de racks) y fija C2-1..C2-9. **Proposal V2** sustituye a V1: corrige nueve puntos, dos de ellos invalidando afirmaciones de V1 (D-07 y D-13). Sin Architect Review |
| 2026-09-08 | **Gate C3**: el dueno **decide D-07 (F2, promocion pegajosa; F3 rechazada)** y **D-10 (F1)**, saca la biblioteca de bloques del lote de propagacion y corrige el Consensus Freeze conforme a WORKFLOW seccion 2. **Proposal V3** sustituye a V2. **No quedan decisiones de producto abiertas.** Sin Architect Review |
| 2026-09-08 | **Architect Review sobre V3: `DISAGREED`** — 4 BLOCKER, 5 HIGH, 7 MEDIUM, 4 LOW. La afirmacion de V3 «no quedan decisiones de producto abiertas» resulto **falsa**: ni LINK ni el punto de resolucion estaban decididos |
| 2026-09-08 | **Reconciliacion V4**: el Coordinador **retira** su AGREED sobre V3 y acepta los findings. El dueno fija C4-1..C4-14. **Proposal V4** sustituye a V3 y cierra B1-B4, H1-H5, M1-M7 y L1-L4. **Ningun revisor declarado**; ADR **no** escrito |
| 2026-09-09 | **CF-2 SATISFECHA — Consensus Freeze COMPLETO**: el Owner **acepta ADR-0034** («Acepto ADR-0034»). El ADR pasa de `propuesto` a **`aceptado`** sobre el contenido de `6eafd59`, que ya incluye la **correccion material de `RepairBroken`**. Gate `owner-decision` **resuelto**. **Implementacion AUTORIZADA A COMENZAR, no iniciada**; produccion **no validada** |
