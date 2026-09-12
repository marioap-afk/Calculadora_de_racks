---
schema: rackcad-initiative/v1
id: I-51
title: "ID15 — RACKDUPLICAR: multiples racks origen"
type: feature
status: claimed
branch: feature/rackduplicar-multiples-origenes
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [autocad-plugin, persistence, architecture-kernel]
automation_state_path:
decision_paths: [docs/automation/decisions/I-51.md]
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

# I-51 — ID15 — RACKDUPLICAR: multiples racks origen

> **Fase actual: G0 CERRADA · G1 CERRADA · G2 CERRADA al publicarse este contrato · G3 NO INICIADA.**
> Este documento es el **contrato vinculante** de la implementacion. No hay una sola linea de produccion
> escrita, y G3 no arranca sin **orden explicita** y el preflight de la seccion 12.

```text
Initiative   = I-51
Owner ID     = ID15 — RACKDUPLICAR: multiples racks origen
Branch       = feature/rackduplicar-multiples-origenes
Worktree     = ~/.codex/worktrees/feature-rackduplicar-multiples-origenes
BASE_SHA     = a4d88f18a1f42263d366c44dc05dd18a6786f152
CLAIM_SHA    = 3ffd2ca21778b29bd5ccac2e6d171971769ad8ca   (Claim-Id ad4b9e47-63c0-48c3-9406-9ae2db8a121a)
BOOTSTRAP    = 5a5c12aaf04bb9f3edfd861aad9fc266dfb89cf2
G1_SHA       = c6fbfbccfc898764ffa71793070237bd6f824b8d
Decisiones   = docs/automation/decisions/I-51.md
Discovery    = docs/initiatives/I-51-discovery.md
ROADMAP      = fila «RACKDUPLICAR con multiples origenes»; NO se toca en G2 y se alinea al integrar
```

> **Apertura por autorizacion explicita del dueno sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la preexistencia de la
> fila en ROADMAP; la fila y el contrato nacieron en el bootstrap. La autorizacion de apertura fue la
> instruccion directa del dueno. Las decisiones **posteriores** —PD-1..PD-7 y la reconciliacion de la
> revision de Arquitecto— se versionan en
> [`docs/automation/decisions/I-51.md`](../automation/decisions/I-51.md), y por eso `decision_paths` ya
> apunta a ese archivo.

## 1. Objetivo

Extender `RACKDUPLICAR` de

```text
1 rack origen → M copias
```

a

```text
N racks/vistas origen seleccionados → M grupos de copias
```

preservando **disposicion relativa**, **identidad logica**, **estructura enlazada existente**,
**bindings** y **authored JSON**.

## 2. Problema

Resumen de lo verificado en G1 ([Discovery](I-51-discovery.md), sobre `a4d88f1`):

- Hoy el comando toma **una** referencia (`GetEntity`), y por cada destino abre una transaccion donde
  `RackEnvelopeRestamp.RestampEnvelope(payload, copyName)` genera el GUID **dentro**
  (`RackEnvelopeRestamp.cs:36`), clona la definicion y crea una referencia desplazada.
- La identidad y el payload viven en la **definicion** (`BlockTableRecord`), no en la referencia
  (ADR-0009).
- Cada invocacion produce un rack **de una sola vista**: la vista clicada, decision del 2026-07-09 que
  PD-1 conserva.
- Duplicar varias vistas de un mismo rack en un solo gesto exige que N definiciones compartan **un** id y
  **un** nombre. Tres autoridades comparan esas vistas y abortan si difieren
  (`SelectiveAuthoredAuthority`, `BomAuthoredAuthority`, `ProjectVariableConsumerDiscovery`).
- `RACKLISTA` y `RACKBOMTOTAL` cuentan copias como el **maximo de referencias directas** por RackId
  (`RackInventarioCommands.cs:72`; `RackInventarioCommands.BomTotal.cs:109-113`).

## 3. Alcance vinculante

### 3.1 Decisiones de producto (PD-1..PD-7)

Cerradas; registro y origen en [`decisions/I-51.md`](../automation/decisions/I-51.md) seccion 2. **No se
reabren.**

- **PD-1 — Vistas.** Solo se copian las referencias/vistas **fisicamente seleccionadas**. Una vista
  seleccionada **no** expande a sus hermanas.
- **PD-2 — Referencias enlazadas.** Las referencias seleccionadas de una misma definicion permanecen
  enlazadas: **1 definicion clonada + N referencias**. No se convierten en racks independientes. Los
  enlaces viven dentro de la copia de un destino; nunca cruzan destinos.
- **PD-3 — Seleccion mixta.**
  - objeto que no es `BlockReference` → ignorar + aviso;
  - `BlockReference` sin payload RackCad → ignorar + aviso;
  - payload existente pero ilegible, de MAJOR futuro, con `Kind` vacio o desconocido, o con `Design`
    vacio → **fail-closed** antes de mutar;
  - cero fuentes validas → abortar;
  - **sin `SelectionFilter`** que oculte los objetos ignorados.
- **PD-4 — Atomicidad temporal.** Atomicidad **por destino**. Un fallo en el destino k **termina el
  comando**, k se revierte **completo** y los destinos 1..k-1 ya confirmados permanecen.
- **PD-5 — Nombres.** Nombre logico por ordinal de destino: «&lt;base&gt; - copia», «&lt;base&gt; - copia 2», etc.
  Todo el grupo lleva **el mismo `Name` logico** (sobre e identidad interior). El nombre de
  `BlockTableRecord` puede quedar uniquificado por `RackCloner`.
- **PD-6 — Legacy sin RackId.** `Id` null, vacio o solo espacios = legacy: **grupo propio por
  definicion**. No se infieren hermanas por nombre, geometria ni contenido.
- **PD-7 — Espacio.** Solo **Model Space**. Paper Space se ignora con aviso. Sin cross-space.

### 3.2 Cambios requeridos por la revision de Arquitecto (RC-1..RC-8)

Veredicto `AGREED WITH CHANGES`; los ocho cambios fueron **aceptados** por el Coordinador y quedan
incorporados en este contrato:

| # | Cambio | Donde queda |
|---|---|---|
| RC-1 | `newId` tipado como `Guid`, invariantes en una sola implementacion | §3.3 |
| RC-2 | Clave logica discriminada `RackId` o `Definition(handle)`; `Id` blanco = ausente | §3.4 |
| RC-3 | Clasificacion propia en cuatro clases; **prohibido** usar `ProjectVariableScanProjection` | §3.4, INV-03 |
| RC-4 | Consistencia de grupo con fail-closed | §3.4, INV-05 |
| RC-5 | Planificador puro minimo en Application, **requerido** | §3.6 |
| RC-6 | Nada despues del commit salvo mensajes | INV-14 |
| RC-7 | Guardas reapuntadas a la propiedad, con RED demostrado; guarda nueva de PD-1 | §9.2 |
| RC-8 | Precisiones a PD-3..PD-7 y correcciones D1..D7 al Discovery | §3.1; Discovery §0 |

### 3.3 Identidad: `NewRackId` e invariantes NI-1..NI-6

**API acordada conceptualmente** (nombres no contractuales mas alla de lo que fijan las guardas):

```text
RestampEnvelope(payload, copyName, Guid newId)   ← UNICA implementacion
RestampEnvelope(payload, copyName)               ← firma historica: delega con Guid.NewGuid()
```

La firma de dos argumentos **permanece** para los consumidores existentes (`RACKLAYOUT`), sin cambio de
comportamiento.

| # | Invariante de la implementacion unica |
|---|---|
| **NI-1** | Un **solo** string derivado del `Guid` (`newId.ToString()`) se usa para el sobre **y** para el handler. Motivo: `CantileverKindHandler.cs:105` genera un GUID aleatorio si el id no se interpreta, lo que daria identidades interiores distintas entre vistas |
| **NI-2** | `Guid.Empty` ⇒ `Failure` |
| **NI-3** | No se reutiliza la identidad del origen: `newId` igual al `Id` del sobre de origen (sin distinguir mayusculas) ⇒ `Failure` |
| **NI-4** | Un `Kind` sin handler **conserva el fail-closed actual**: lanza antes de producir copia (`RackEnvelopeRestamp.cs:66-69`); los llamadores ya lo filtran antes |
| **NI-5** | Un sobre que no deserializa devuelve **`Failure`**, nunca `NullReferenceException` |
| **NI-6** | `Success` solo con identidad exterior e interior **coherentes** (mismo id y nombre en ambas mitades). **Nunca** se devuelve el payload original |

**Autoridad de generacion y suministro:**

1. **Genera** `RackDuplicarCommands` (Plugin), inyectando `Guid.NewGuid` como generador al planificador.
2. **Invoca y valida** el planificador puro: exactamente **una** vez por (grupo logico, destino); rechaza
   `Guid.Empty`, repeticiones dentro del lote y coincidencias con cualquier RackId de la seleccion.
3. **Suministra** el plan → Plugin → `RestampEnvelope(payload, copyName, Guid newId)`.
4. Ningun otro componente genera ids de copia en el camino de I-51. La firma de dos argumentos queda como
   unico generador historico.
5. Una colision con un rack **no** seleccionado del dibujo se deja a la aleatoriedad del GUID, como hoy; no
   se escanea el dibujo.

La unicidad dentro del lote **no** es responsabilidad del helper, que no ve el lote.

### 3.4 Clave logica, clasificacion y consistencia de grupo

**Clave logica** — valor **discriminado**:

```text
LogicalSourceKey = RackId(valor)            si Id no es null, vacio ni solo espacios
                                            comparacion sin distinguir mayusculas, sin trim (como FindRackBlocks)
                 | Definition(handle)       en otro caso (legacy, PD-6)
```

- Las dos variantes **nunca** colisionan, aunque un RackId coincida en texto con un handle.
- `Definition(handle)` vive **solo dentro del lote**: no se persiste, no se muestra como RackId y no es
  un RackId inventado (C4.6-3 de I-47).
- **No** se usa `ProjectVariableScanProjection` para clasificar fuentes de I-51: convierte un sobre sin
  `Id` en `UnreadableEnvelope` (`ProjectVariableScanProjection.cs:37-42`), lo que contradice PD-6.

**Consistencia de grupo** — el grupo **falla cerrado** si:

- su `Kind` no es unico semanticamente (comparacion sin distinguir mayusculas);
- existen **nombres logicos no vacios divergentes** (tras trim, comparacion ordinal). Sin nombre alguno, la
  base es «Rack», como hoy;
- es **Selectivo** con dos o mas definiciones y su autoridad authored **no coincide o no puede leerse**
  (`SelectiveAuthoredAuthority.IsSameAuthority`, el mismo comparador de I-47; ADR-0034 §9). Nunca se elige
  la hermana legible.

### 3.5 Invariantes ejecutables (INV-01..INV-18)

**Fuentes**

- **INV-01 — Seleccion fisica.** Solo entran las `BlockReference` fisicamente seleccionadas. El comando
  **no** llama a `FindRackBlocks` ni a `ScanEnvelopes` (PD-1).
- **INV-02 — Espacio.** Una referencia es fuente solo si su `OwnerId` es el Model Space (criterio de
  `RackBlockFinder.cs:22-32`). Las demas se filtran con aviso agregado. Las copias se anaden siempre a
  Model Space (PD-7).
- **INV-03 — Clasificacion**, por definicion distinta y en este orden:
  - (a) no `BlockReference` → ignorar + aviso;
  - (b) sin payload RackCad (`RackBlockData.Read` nulo) → ignorar + aviso;
  - (c) payload presente y sobre no deserializable (JSON invalido o MAJOR futuro), `Kind` vacio o sin
    handler, o `Design` vacio → **FAIL-CLOSED** antes del punto base, con diagnostico que nombra el bloque
    o su handle, y el RackId solo si se conoce;
  - (d) fuente valida.
  - Cero fuentes validas → abortar. **Nada de la clase (c) desaparece en silencio.** Sin `SelectionFilter`
    de tipo (PD-3, C4.6-1 y C4.7-1 de I-47).
- **INV-04 — Clave logica** discriminada segun §3.4 (PD-6).
- **INV-05 — Consistencia de grupo** segun §3.4.

**Identidad y contenido**

- **INV-06 — Deduplicacion.** Por destino, cada definicion distinta del grupo se clona **exactamente
  una vez**; **todas** las referencias seleccionadas (unicas por handle) se recrean, cada una apuntando al
  clon **de su propia** definicion (PD-2).
- **INV-07 — Un Guid por grupo y destino.** Exactamente **un** `NewRackId` por (grupo logico, destino):
  no vacio, distinto entre grupos y de todo RackId de la seleccion, presente en el sobre **y** en la
  identidad interior de todas las definiciones del grupo.
- **INV-08 — Nombre logico.** En el destino k: base + « - copia» si k = 1; base + « - copia k» si k ≥ 2
  (cultura invariante). Identico en todas las definiciones del grupo. Los nombres de BTR siguen la
  politica de `RackCloner`. Las palabras clave `Unica`/`Multiple` no consumen ordinal (PD-5).
- **INV-09 — Restamp por definicion.** Cada clon recibe `RestampEnvelope(su propio payload, CopyName,
  NewRackId)` con NI-1..NI-6, y conserva `View`, `Section`, `SchemaVersion` y `ExtensionData` **de su
  vista** (C4.6-6 de I-47) y el authored: bindings con el **mismo** `VariableId`, literal congelado,
  schema promovido y campos desconocidos. **Nunca** se clona con el payload de origen (C4.7-4 de I-47;
  ADR-0034 §12).
- **INV-10 — Authored y bindings intactos.** No se lee ni escribe el registro de variables; no se crean
  `ProjectVariables`; no se resuelven ni materializan bindings; el authored no cambia salvo `Id`/`Name`
  via restamp. Un binding roto se copia roto, como hoy.

**Transformacion y atomicidad**

- **INV-11 — Desplazamiento comun por destino.** `d = (destino − base).TransformBy(CurrentUserCoordinateSystem)`
  se calcula **una vez** por destino. Cada referencia nueva: `Position` = posicion WCS de su referencia
  origen + `d`; `Rotation`, `ScaleFactors` y `LayerId` de **su** referencia origen. Nada mas: el conjunto
  historico de propiedades (`RackDuplicarCommands.cs:201-206`).
- **INV-12 — Preflight completo** (INV-01..INV-05 y un ensayo de restamp por definicion distinta)
  **antes** del punto base; fallo ⇒ cero mutacion.
- **INV-13 — PREPARE semantico antes de escribir; una transaccion por destino.** Ids, nombres y restamps
  de **todas** las definiciones del destino se resuelven **antes** de abrir la transaccion de escritura.
  Una **unica** transaccion por destino contiene todos los clones y todas las referencias. Una excepcion
  ⇒ sin commit ⇒ el destino k queda con **cero** mutaciones ⇒ el comando termina; 1..k-1 permanecen
  (PD-4).
- **INV-14 — Nada post-commit salvo mensajes.** Ni `Regen`, ni purga, ni `RackBlockRenamer.SyncName` (abre
  y confirma su propia transaccion, `RackBlockRenamer.cs:32-50`), ni `EnsureLayer`, ni importacion de
  biblioteca.
- **INV-15 — Origen intacto.** Nunca se escribe en definiciones, referencias ni payloads de origen.
- **INV-16 — Una referencia, comportamiento historico.** Seleccionar una sola referencia produce el
  resultado historico: un clon, una referencia, mismo nombre, misma transformacion. `RACKLAYOUT` y
  `RACKRELLENAR` no cambian.
- **INV-17 — Destinos independientes.** Nada se enlaza entre copias de destinos distintos.
- **INV-18 — Dispatch de kind sin distinguir mayusculas.** Sin ramas por kind en el comando ni en el
  planificador; la resolucion no distingue mayusculas (**no** `TryResolveAll`, que es ordinal:
  `KindHandlerDispatch.cs:45-58`).

### 3.6 Modelo minimo

Conceptos, **sin fijar nombres de clases**:

| Concepto | Capa | Contenido |
|---|---|---|
| **SelectedReference** | Plugin | Una referencia fisica seleccionada: handle, definicion, pertenencia a Model Space, posicion, rotacion, escala y capa. **Solo** vive en el Plugin |
| **DefinitionSnapshot** | Plugin → planificador, como datos planos | Handle de la definicion, texto del payload o su ausencia, nombre de bloque para diagnostico |
| **LogicalSourceKey** | Planificador | Valor discriminado `RackId` o `Definition(handle)` (§3.4) |
| **SourceRackGroup** | Planificador | Clave logica, `Kind`, nombre base, definiciones **unicas** y **todas** las referencias seleccionadas, por clave |
| **DestinationAssignment** | Planificador | Por destino k y por grupo: `{LogicalSourceKey, NewRackId, CopyName}` |

**Planificador puro minimo en Application = REQUERIDO** (RC-5). Motivo: INV-03..INV-08 fijan identidad y
conteo de BOM, y en el Plugin —que ninguna suite carga (ADR-0003)— solo podrian vigilarse con guardas de
texto, que no demuestran agrupacion ni deduplicacion.

- Una clase estatica y pocos registros inmutables, en `src/RackCad.Application/Persistence/`, con la forma
  de `RackListBuilder`.
- **Entrada**: claves y datos planos por referencia y por definicion, un predicado «kind conocido» y el
  generador de `Guid` inyectado.
- **Salida**: resultado tipado (plan o fallo con diagnostico atribuible) mas avisos.
- **NO incluye**: posiciones, `ObjectId`, UCS, transacciones, restamp ni ningun tipo de AutoCAD.
- Sin registro, sin interfaces y sin framework general de seleccion o transformacion.

### 3.7 Flujo PREPARE → MUTATE → COMMIT

```text
ACQUIRE    GetSelection sin SelectionFilter de tipo
SNAPSHOT   una transaccion de lectura: datos planos por referencia y por definicion distinta
PREFLIGHT  planificador puro → fallo ⇒ mensaje atribuible y FIN, sin punto base
           ensayo de restamp por definicion distinta → fallo ⇒ FIN
           avisos agregados (no RackCad / fuera de Model Space)
BASE POINT
LOOP k     GetPoint / [Unica | Multiple]
  PREPARE  asignacion del destino k (ids y nombres) → restamp de TODAS las definiciones → alguno falla ⇒ FIN
           d calculado una vez
  MUTATE   UNA transaccion: clonar cada definicion (mapa local definicion → clon) → crear cada referencia
  COMMIT   al salir del cuerpo; excepcion ⇒ sin commit ⇒ k intacto ⇒ informar y FIN
POST       solo mensajes
```

`InDocumentTransaction` **basta** para MUTATE: el cuerpo es de una fase, sin `Regen` ni purga, que es el uso
que su documentacion admite (`InDocumentTransaction.cs:11-15`).

| Error | Momento | Resultado |
|---|---|---|
| E1 — ninguna fuente valida | Preflight | Mensaje y fin, sin punto base |
| E2 — payload RackCad inutilizable (INV-03 c) | Preflight | Fin; diagnostico atribuible; cero mutacion |
| E3 — grupo inconsistente (Kind, nombre, authored Selectivo) | Preflight | Fin; nombra el RackId; se sugiere reconciliar con `RACKEDITAR` |
| E4 — el ensayo de restamp falla | Preflight | Fin; cero mutacion |
| E5 — un restamp del destino k falla | PREPARE(k) | k intacto; fin |
| E6 — fallo de AutoCAD en MUTATE | Transaccion | Rollback completo de k; se informa; fin; 1..k-1 permanecen |
| E7 — Enter o Esc | Bucle | Fin; lo confirmado permanece |

### 3.8 Transformacion UCS → WCS

El desplazamiento comun de INV-11 **es suficiente para ID15**: la misma parte lineal aplicada a todas las
referencias conserva distancias y orientacion relativa, como COPY. No es ID16 ni ID19. `Normal` y demas
propiedades de la referencia no se copian (limitacion historica). Como `Editor` y el UCS no son probables en
Core (ADR-0003), la evidencia es la validacion M3 del Owner y, como defensa secundaria, la guarda G-R6.

### 3.9 Efecto en `RACKLISTA` y `RACKBOMTOTAL`

**Sin cambio de codigo.** Cada rack copiado aparece una vez, con copias = maximo de referencias
seleccionadas por definicion de su grupo, igual que el subconjunto de origen. La lista de vistas refleja
solo las copiadas (PD-1). Las copias de racks legacy reciben RackId real y se vuelven visibles, mientras el
origen legacy sigue invisible en `RACKLISTA` y, si esta colocado, sigue abortando `RACKBOMTOTAL`
(`RackInventarioCommands.BomTotal.cs:84-96`), fuera de alcance. Una rejilla enlazada de `RACKLAYOUT`
copiada sigue enlazada, bajo un rack nuevo.

### 3.10 Decision sobre ADR

**Sin ADR**: I-51 aplica ADR-0009 y ADR-0034 §9 y §12 sin cambiarlos. Las condiciones que harian
obligatorio un ADR estan en [`decisions/I-51.md`](../automation/decisions/I-51.md) seccion 4.

## 4. Fuera de alcance

- **ID16** (RACKMIRROR), **ID19** (multi-rack projection), **ID21** (referencias rack a rack), **ID22B** /
  Expression Engine, multi-edit, cross-DWG, WBLOCK y cualquier rediseno general de seleccion o de
  transformaciones.
- Expandir a vistas hermanas (PD-1); Paper Space y cross-space (PD-7).
- Copiar datos de la referencia mas alla del conjunto historico (AM-4, cerrada como no material).
- Modificar `RACKLAYOUT` o `RACKRELLENAR`.
- Corregir L-1..L-4: registrados sin corregir en [ideas-futuras.md](../ideas-futuras.md), seccion «I-51».
- La semantica de Project Variables (ADR-0034) y de la edicion vinculable (I-48).
- El alcance de I-49 y de I-50.
- Editar `docs/ROADMAP.md` o `docs/HANDOFF.md` antes de la integracion.

## 5. Contexto requerido

- [Discovery](I-51-discovery.md), incluida su seccion 0 de reconciliacion.
- [`docs/automation/decisions/I-51.md`](../automation/decisions/I-51.md).
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)
  y [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) §9, §12 y §14.
- [Decisiones de I-47](../automation/decisions/I-47.md): C-4, C2-6, C4.6-1, C4.6-3, C4.6-6, C4.7-1 y C4.7-4.
- Contratos de [I-10](I-10-kind-handlers.md) (restamp por handler) y [I-11](I-11-persistencia-uniforme.md)
  (campos desconocidos en duplicacion).
- Contrato de I-50 en su rama @ `fdaf2bc` (`CD-01`, `CD-08`, `CD-11`) y contrato de I-49 en su rama @
  `f2d28a2` (§4 y §6.4), **solo lectura**.
- [AGENTS.md](../../AGENTS.md): pruebas, guardas y reutilizacion de evidencia.
- Context Packs del frontmatter.

## 6. Dependencias y coordinacion

- **Sin dependencias pendientes.**
- `conflicts_with: []`. Con I-50 solo hay conflicto **documental** (`CD-11` de su contrato); con I-49 no se
  preve archivo productivo comun (§6.4 de su contrato). La excepcion `OWNER_OVERRIDE_I49_I50_PARALLEL`
  **no** cubre a I-51: rige WORKFLOW normal.
- El grupo caliente `src/RackCad.Plugin/*Commands*.cs` (WORKFLOW §7) se interpreta **por archivo**: I-51
  solo toca `RackDuplicarCommands.cs`.
- Conflictos textuales previstos al integrar: la fila de ROADMAP (I-49, I-50 e I-51 en el mismo punto) y la
  seccion final de `docs/ideas-futuras.md` (I-50 e I-51). Quien integre despues conserva todas las filas y
  secciones.
- **Protocolo obligatorio antes de G3, antes de G5 y antes del Candidato**: `git fetch --all --prune`;
  `git diff --name-only origin/main...origin/<rama>` para I-49 e I-50; leer sus contratos en el SHA remoto
  exacto; evaluar S1..S5 (seccion 12).

## 7. Archivos

### 7.1 Produccion probable (G3+)

1. `src/RackCad.Plugin/RackDuplicarCommands.cs`
2. `src/RackCad.Plugin/RackEnvelopeRestamp.cs` — solo la entrada con `Guid` y la delegacion de la firma
   historica
3. Un planificador nuevo en `src/RackCad.Application/Persistence/`
4. `src/RackCad.UI/RackCommandReference.cs` — **opcional**, solo el texto de ayuda

### 7.2 Pruebas

- Un archivo nuevo en `tests/RackCad.Tests/` para el planificador.
- `tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs` — guardas reapuntadas y T14-T15.
- `tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs` — guarda reapuntada.

### 7.3 Documentacion (G8)

`docs/guias/despliegue.md`, `README.md`, `docs/guias/validacion-manual-autocad.md`, `docs/ideas-futuras.md`
(#5), este contrato y el registro de decisiones. `docs/HANDOFF.md` y `docs/ROADMAP.md` **solo** en la sesion
de integracion.

### 7.4 NO TOCAR salvo nueva revision de Arquitecto

| Area | Archivos |
|---|---|
| Plugin | `RackLayoutCommands.cs`, `RackLayoutCommands.Fill.cs`, `RackCloner.cs`, `InDocumentTransaction.cs`, `RackBlockFinder.cs`, `RackCommandSupport.cs`, `RackInventarioCommands.cs`, `RackInventarioCommands.BomTotal.cs`, `KindHandlers/*` (incluido `IRackKindHandler.cs`), `Systems/Shared/RackBlockData.cs`, `ProjectVariableMutationExecutor.cs`, `Drawing/*` |
| Application | `Persistence/RackEmbedDocument.cs`, `Persistence/RackEmbedComposer.cs`, `Persistence/RestampResult.cs` (`SelectiveAuthoredRestamp`), `Persistence/SelectivePalletDesignDocument.cs` y los demas DTO y stores authored, `Persistence/RackListBuilder.cs`, `Persistence/KindDispatch.cs`, `ProjectVariables/*`, `Bom/*` |
| UI | editores y ventanas; `LinkedPropertyEditor` |
| Otros | `assets/`, catalogos, biblioteca de bloques, `.github/`, ADR aceptados |

Necesitar cualquiera de ellos ⇒ **detenerse** (seccion 12).

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico, contrato y fila en ROADMAP | **CERRADA** |
| G1 | Discovery | [I-51-discovery.md](I-51-discovery.md) @ `c6fbfbc` | **CERRADA** |
| G2 | Contrato | Este contrato, [`decisions/I-51.md`](../automation/decisions/I-51.md), reconciliacion del Discovery y L-1..L-4 en ideas-futuras | **CERRADA al publicarse este commit** |
| G3 | Planificador puro + T1-T15 (solo Core, sin Plugin) | T1-T13 RED → GREEN; T14-T15 de caracterizacion | **NO INICIADA** — orden explicita + protocolo §6 |
| G4 | Entrada con `Guid` en `RackEnvelopeRestamp` + G-R1..G-R6 | RED demostrado por violacion temporal → GREEN | pendiente |
| G5 | Cableado de `RackDuplicarCommands` + build Debug del Plugin | protocolo §6 antes | pendiente |
| G6 | Candidato | Core y UI locales, CI 4/4 sobre el SHA exacto, Debug de UI y Plugin | pendiente |
| G7 | Validacion del Owner en AutoCAD 2025 | M1..M9 | pendiente |
| G8 | Documentacion + integracion serializada | WORKFLOW 4.5 | pendiente |

El SHA de G2 se registra en el reporte y en el siguiente commit de la rama, **no** en este documento: un
documento no puede contener el SHA del commit que lo crea.

## 9. Pruebas y builds — compuertas obligatorias

### 9.1 Core

| Test | Afirma |
|---|---|
| **T1** | Una referencia → 1 grupo, 1 definicion, 1 referencia; nombre «&lt;base&gt; - copia» en k=1 y «&lt;base&gt; - copia 2» en k=2 (historico) |
| **T2** | Frontal, lateral y planta de A (mismo RackId) → 1 grupo, 3 definiciones, 3 referencias; el generador se invoca **exactamente una vez** por destino; mismo `NewRackId` y `CopyName` en las tres |
| **T3** | A y B → 2 grupos con `NewRackId` distintos y nombre por su propia base |
| **T4** | Dos referencias de la misma definicion → 1 definicion y 2 referencias |
| **T5** | Seleccion parcial: solo aparecen las definiciones seleccionadas, sin hermanas |
| **T6** | RackIds que difieren solo en mayusculas → 1 grupo |
| **T7** | Legacy con `Id` null, vacio y solo espacios → clave `Definition`; dos referencias de la misma definicion legacy quedan enlazadas; dos definiciones legacy con igual nombre, kind y contenido → 2 grupos |
| **T8** | `Definition(handle)` no colisiona con un RackId de texto igual al handle |
| **T9** | Sin payload → ignorado con aviso; no `BlockReference` → ignorado con aviso; JSON invalido, MAJOR futuro, `Kind` vacio, `Kind` desconocido o `Design` vacio → fallo sin plan, con diagnostico atribuible; solo ignorados → abortar |
| **T10** | Fuera de Model Space → filtrado con aviso; no participa en grupos |
| **T11** | `Kind` distinto (no solo mayusculas) bajo el mismo RackId → fallo; nombres logicos no vacios divergentes → fallo; todos vacios → base «Rack» |
| **T12** | Selectivo con dos o mas definiciones: authored divergente → fallo que nombra el RackId; una definicion ilegible → fallo, y **nunca** se elige la legible; iguales → exito |
| **T13** | 2 grupos × 3 destinos → 6 `NewRackId` distintos y nombres «- copia», «- copia 2», «- copia 3»; generador que devuelve `Guid.Empty` o un RackId de la seleccion → fallo |
| **T14** | `SelectiveAuthoredRestamp` sobre un documento con **dos** bindings (`selective.verticalClearance` y `selective.palletTolerance`) conserva ambos `VariableId` y ambos literales, con `Id`/`Name` nuevos |
| **T15** | Dos documentos authored iguales re-estampados con el mismo (id, nombre) siguen dando `IsSameAuthority` verdadero; con nombres distintos, falso |

T1-T13 prueban codigo nuevo y deben verse **en rojo** antes de implementarlo. T14 y T15 **caracterizan**
comportamiento que ya existe en Application: pasan desde el primer momento y fijan el invariante que la copia
multivista necesita (INV-08, INV-09); no se les exige RED.

### 9.2 Guardas del Plugin

**Reglas**: las guardas existentes **no se borran** para facilitar el cambio; se **reapuntan** a la
propiedad que protegian. Cada guarda nueva o reapuntada se demuestra en **ROJO** mediante una violacion
**temporal, no commiteada**, y se registra el conteo de la corrida. Una seleccion de pruebas que no
selecciona nada es un **fallo** (AGENTS.md).

| Guarda | Relacion con la guarda existente | Propiedad protegida |
|---|---|---|
| **G-R1** | **Reapunta** `GUARDA_RACKDUPLICAR_DECIDE_ANTES_DE_CLONAR` (`SelectiveDuplicationFailClosedTests.cs:247-254`) | El metodo que contiene `CloneDefinition` no llama a `RestampEnvelope`, y la comprobacion de `IsSuccess` de todos los restamps precede a la transaccion de escritura del destino |
| **G-R2** | **Reapunta** la parte de Duplicar de `GUARDA_NINGUN_CAMINO_DE_COPIA_CAE_AL_PAYLOAD_DE_ORIGEN` (`:265-277`); la parte de `RackLayoutCommands.cs` queda literal | Ningun `CloneDefinition` de `RackDuplicarCommands.cs` recibe un payload de origen |
| **G-R3** | **Reapunta** la mitad de Duplicar de `CopyAndLayout_AcceptPushBackViaIgnoreCaseLookup_WithNoPerKindBranch` (`PushBackRoundTripSourceGuardTests.cs:299-308`); la mitad de Layout queda literal | Kind resuelto sin distinguir mayusculas y sin constantes `Kind*` ni tipos `*KindHandler` en el comando |
| **G-R4** | **Nueva** (PD-1) | `RackDuplicarCommands.cs` no contiene `FindRackBlocks(` ni `ScanEnvelopes(` |
| **G-R5** | **Amplia**, sin retirar, `GUARDA_EL_RESTAMP_COMPARTIDO_NO_TIENE_MEJOR_ESFUERZO` (`:235-245`) y `RackEnvelopeRestamp_ResolvesViaRegistryIgnoreCase_NoPushBackBranch` (`:292-297`) | Un unico `Guid.NewGuid(` en `RackEnvelopeRestamp.cs`, dentro de la firma de dos argumentos; siguen valiendo sin `catch (`, sin `return designJson;` y con `TryGetIgnoreCase(` |
| **G-R6** | **Nueva** (INV-11 e INV-14) | `RackDuplicarCommands.cs` sin `Regen(`, `SyncName(`, `EnsureLayer(`, `EnsureForPlan(` ni `PurgeUnreferenced(`; el desplazamiento se calcula una sola vez por destino |

### 9.3 Suites y builds

- Suite **UI**: no se exige prueba nueva; corre completa en local sobre el Candidato.
- Candidato (G6): **Core y UI completas en local**, **CI 4/4 sobre el SHA exacto**, build Debug de UI y
  de Plugin, con AutoCAD cerrado (AGENTS.md).

## 10. Validacion manual — Owner, AutoCAD 2025 (G7)

| # | Escenario |
|---|---|
| **M1** | Regresion: un rack, una vista, en modos `Multiple` y `Unica` |
| **M2** | A con frontal, lateral y planta seleccionados, 2 destinos: `RACKEDITAR` sobre cada copia redibuja sus 3 vistas juntas; el original queda intacto |
| **M3** | A y B con **UCS girado** 30° sobre Z y origen desplazado; base (0,0) y destino `@120,0` en UCS; en WCS cada copia = origen + (120·cos30°, 120·sin30°, 0); offsets relativos, rotacion, escala y capa intactos |
| **M4** | Dos referencias enlazadas: la copia sigue enlazada (editar una cambia la otra); `RACKLISTA` y `RACKBOMTOTAL` cuentan 2 |
| **M5** | Seleccion mixta con lineas y textos: se ignoran con aviso |
| **M6** | Referencias en Paper Space: se filtran con aviso |
| **M7** | Rack Selectivo con dos bindings: la copia conserva los `VariableId`, aparece como consumidora y un `ChangeValue` la propaga |
| **M8** | Payload de MAJOR futuro (si hay DWG de prueba): fail-closed sin mutacion |
| **M9** | **UNDO** tras un comando de varios destinos: se observa y registra la granularidad |

## 11. Criterios de aceptacion

1. Una invocacion duplica N racks/vistas seleccionados en M destinos.
2. En cada destino, las vistas copiadas de un mismo origen logico comparten **un** `NewRackId` y **un**
   nombre logico; ninguna comparte RackId con su origen ni con otro grupo.
3. Las referencias enlazadas resultan en una definicion con N referencias, y `RACKLISTA`/`RACKBOMTOTAL`
   cuentan igual que el subconjunto de origen.
4. La disposicion relativa se preserva, tambien con UCS girado.
5. Bindings (mismos `VariableId`), literal congelado, `SchemaVersion` y `ExtensionData` preservados; ninguna
   `ProjectVariable` nueva.
6. Fuentes ilegibles o grupos inconsistentes fallan cerrado antes de mutar; un fallo en el destino k no deja
   mutacion de k.
7. Una sola referencia reproduce el comportamiento historico; `RACKLAYOUT` no cambia.
8. T1-T15 y G-R1..G-R6 verdes con RED demostrado; Candidato con dos suites locales, CI 4/4 y builds Debug;
   M1..M9 aprobados por el Owner.

## 12. Condiciones para detenerse

| # | Condicion | Accion |
|---|---|---|
| **S1** | Otra rama activa modifica materialmente `RackEnvelopeRestamp.cs`, `RackCloner.cs`, `RackDuplicarCommands.cs`, `IRackKindHandler.RestampDesign` o `RackBlockData.cs` | Detenerse antes de editar; reportar SHA y diff; serializar |
| **S2** | I-50 mueve estado necesario para la copia a la `BlockReference` (XData, diccionario de extension, atributos) o lo indexa por handle u `ObjectId`, o su contrato deja de sostener `CD-01`/`CD-08` | Detenerse; AM-4 pasa a material; nueva revision de Arquitecto |
| **S3** | Aparecen RackIds dentro del contenido authored (p. ej. ID21) | Detenerse; AM-3 pasa a ADR |
| **S4** | Cambia el comparador authored (`SelectiveAuthoredAuthority`) o la tolerancia de `RackEmbedStore.Deserialize` | Detenerse; revalidar INV-03 e INV-05 |
| **S5** | Aparece perdida de round-trip en el sobre o en el diseno (campos que los stores no preservan) | Detenerse; revalidar INV-09 |

Ademas:

- Implementar sin la orden explicita del gate correspondiente: **prohibido**.
- Necesitar un archivo de la seccion 7.4: **detenerse**.
- Desviarse de AM-1, AM-2, INV-03 o INV-13: **nueva revision de Arquitecto**.
- Cualquiera de las condiciones de ADR de [`decisions/I-51.md`](../automation/decisions/I-51.md) seccion
  4: **ADR antes de implementar**.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-51.yml`: la automatizacion esta **desactivada** y el caso (d) no lo exige. **Si
existe** [`docs/automation/decisions/I-51.md`](../automation/decisions/I-51.md), porque hay decisiones que
registrar. El estado vivo se deriva de `origin/feature/rackduplicar-multiples-origenes`. No hay Pull Request;
la integracion es manual y serializada (WORKFLOW 4.5).

## 14. Evidencia

**G0 — reclamo atomico.** Primer `git push -u origin feature/rackduplicar-multiples-origenes` aceptado sin
force (`* [new branch]`) sobre `BASE_SHA`; `main` no fue modificada. Bootstrap `5a5c12a`.

**G1 — Discovery.** `c6fbfbc`, solo documentacion; CI de push en verde.

**G2 — contrato.** Revision de Arquitecto `AGREED WITH CHANGES`, RC-1..RC-8 aceptados, AM-1..AM-3
reconciliadas y AM-4 cerrada como no material, en
[`decisions/I-51.md`](../automation/decisions/I-51.md). Preflight de G2: I-49 @ `f2d28a2` e I-50 @
`fdaf2bc` solo tocan `docs/`, y ninguna modifica los cinco archivos de S1. Solo documentacion.
