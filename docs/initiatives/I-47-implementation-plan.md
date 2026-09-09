# I-47 — Plan de implementacion por gates (ID22A)

```
PLAN VERSION:        V1
Contrato vinculante: docs/initiatives/I-47-proposal-v4.md   — VERSION V4.8
Proposal SHA:        a0621abbd22952ad5a62bf7678212a74526a05ce
ADR:                 docs/adr/0034-project-variables-autoridad-drawing-level.md   — ACEPTADO
Base del plan:       b1205f3ba0da79c8e37f017604ff80e1cde85273
origin/main:         306e18ed4676e5e96b54d59402c9a230efb137d3   (sin avanzar)

CONSENSUS FREEZE = COMPLETE
Implementation   = AUTHORIZED TO START · NOT STARTED
```

> **Este documento planifica. No implementa nada.** No toca `src/`, `tests/`, `assets/`, `eng/`,
> `deploy/` ni `.github/`. No modifica el Proposal, el ADR, `ROADMAP.md` ni `HANDOFF.md`.
>
> **Jerarquia, para que no haya duda de quien manda:** el **Proposal V4.8** es el contrato y ante
> cualquier discrepancia gana el Proposal; el **ADR-0034** es la decision; el
> [contrato de iniciativa](I-47-project-variables-foundation.md) es el alcance; **este documento es la
> secuencia**. Si un gate de aqui contradice a V4.8, el defecto esta aqui.

---

## 0. Verificacion de partida

| Comprobacion | Resultado |
|---|---|
| Rama | `architecture/project-variables-foundation` |
| HEAD local = remoto | `b1205f3ba0da79c8e37f017604ff80e1cde85273` — coinciden |
| Arbol de trabajo | limpio |
| `origin/main` | `306e18ed4676e5e96b54d59402c9a230efb137d3` — **no avanzo** desde el reclamo. Ninguna premisa arquitectonica queda invalidada; **no se rebasea en esta sesion** |
| Guard del Proposal | `git diff a0621ab HEAD -- I-47-proposal-v4.md` = **vacio** |
| Guard del ADR | `git diff b1205f3 HEAD -- 0034-*.md` = **vacio** |

Todo el codigo citado en este plan se leyo contra ese HEAD.

---

## 1. Que decide este plan, y que deja decidido el contrato

El contrato ya decidio **que** se construye. Lo unico que este documento anade es **en que orden**, y
lo hace contra una restriccion dura del repositorio:

```
tests/RackCad.Tests    -> Domain + Application          (suite Core)
tests/RackCad.UI.Tests -> Domain + Application + UI     (suite UI)

NINGUN proyecto de pruebas referencia RackCad.Plugin, y no lo hara (ADR-0003).
```

De ahi sale el principio de ordenacion, y no de una preferencia estetica: **cuanto mas tarde entra
AutoCAD, mas del contrato queda demostrado por CI**. Las 42 pruebas contractuales numeradas de V4.8 §8 lo
confirman: **las 42 son de la suite Core**. Ademas hay **tres** comprobaciones de la suite UI y
**cinco** escenarios que quedan necesariamente para el dueno, y ninguno de esos ocho lleva numero.

---

## 2. Mapa de arquitectura: contrato → simbolos reales → gate

Todos los simbolos de la columna «hoy» se verificaron contra `b1205f3`.

| Area del contrato | Hoy (simbolo real) | Objetivo | Gate |
|---|---|---|---|
| Registro del dibujo | **no existe.** Cero `NamedObjectsDictionary` en `src/` | `ProjectVariablesDocument` + `ProjectVariablesStore` (Application) · `ProjectVariablesData` (Plugin) | **G1, G2, G7** |
| Identidad de variable | — | `VariableId` GUID `OrdinalIgnoreCase`, `Name` sin papel resolutivo | **G1** |
| Forma de la propiedad | `SelectivePalletDesignDocument.VerticalClearance` (`double`, linea 31) | + `PropertyValues : PropertyId → { kind, variableId }` **campo declarado** | **G3** |
| Version del registro | `SchemaGuard.CheckReadable` + `SchemaVersionPolicy.IsReadable/ResolveWriteVersion` | guard propio + **regla de presencia** C4.8-1 | **G2** |
| Sticky del Selectivo | `SelectivePalletDesignDocument.CurrentSchemaVersion = "1.0"`, `SchemaVersion { get; set; } = CurrentSchemaVersion` (lineas 18, 20) | helper monotonico con rama de ERROR; lectura `2.x` | **G3** |
| `ExtensionData` del selectivo | **`grep -c JsonExtensionData` = 0** en `SelectivePalletDesignDocument.cs` | `[JsonExtensionData]` en la raiz — lo exige C4-2 y lo **compara** C4.5-3 | **G3** |
| Portador authored | `RackSelectivoCommands.cs:232` → `SelectivePalletDesignDocument.From(design, id, name)` reconstruye desde Domain | actualizar el documento deserializado, **uno por `RackId`** | **G3** (puro) + **G10** (adopcion) |
| Punto de resolucion | `SelectiveKindHandler.BuildBom` → `Deserialize(...)?.ToDomain()` → `SelectiveGeometryResolver` | `SelectiveEffectiveDesignResolver.Resolve(authored, variables)` **unico** | **G4** |
| Precedencia por celda | `SelectiveGeometryResolver.SeparationFor` (`ClearOverride` gana) | **sin cambios**, y con prueba que lo fija | **G4** |
| Comparador multi-vista | — | `mismaAutoridadAuthored`, igualdad **total** incl. `ExtensionData` | **G5** |
| Probe de consumidores | — | tri-estado `POSITIVE / NEGATIVE / INDETERMINATE` | **G5** |
| Sobre indescifrable | `RackEmbedDocument.Deserialize` → `null`; `RackBlockFinder.cs:81` conserva la entrada con `Embed == null` | `DRAWING_ENVELOPE_INDETERMINATE` (clasificacion pura) | **G5** |
| Preflight | `KindHandlerDispatch.TryResolveAll` · `RackCommandSupport.PreflightInnerSources` | `VariableMutationPreflightResult` / `MutationPlan`, puro | **G6** |
| Conteo de referencias | `RackBlockFinder.cs:78-79` — `includeReferenceCount && embed != null ? … : 0` | conteo desde el `BlockTableRecord`, **independiente del payload** | **G8** |
| Writer frontal/planta | `SystemBlockWriter.RedrawInPlace` (linea 45) — `LockDocument` + transaccion + **commit propio** | transaccion **del llamador** | **G9** |
| Writer lateral | `LateralHeaderDrawService.RedrawInPlace` (lineas 59-98) — `EnsureForPlan` + transaccion + commit + purga + regen **propios** | igual, y **sin `EnsureForPlan` dentro** | **G9** |
| Seam ya existente | `LateralHeaderDrawer.RedefineSystemBlock` **ya recibe la `Transaction`**, y los **dos** writers la usan | no cambia de firma | **G9** |
| Firma del BOM | `IRackKindHandler.BuildBom(RackEmbedDocument, RackCatalog)` — 6 implementaciones | + tercer argumento `ProjectVariablesDocument`; resultado **tipado** | **G13** |
| Autoridad del BOM | `RackInventarioCommands.BomTotal.cs:59-61` — conserva la **primera** hermana | `ResolveBomAuthoredAuthority` puro, 3 resultados | **G13** |
| Canal de aviso del BOM | `RackBomOutputGate.DescribeBlocked` / `.DescribeUnreadable` (Application) | + cuarto canal `BrokenProjectVariableReference` | **G13** |
| `catch` del BOM | `RackInventarioCommands.BomTotal.cs:158` — `catch (System.Exception) { … return null; }` | permanece **solo para lo imprevisto** | **G13** |
| Restamp interior | `SelectiveKindHandler.RestampDesign` (Plugin) · `RackEnvelopeRestamp.cs:52-59` — `catch` ⇒ devuelve el JSON original | `SelectiveDesignRestamp` puro en Application + **fail-closed** | **G14** |
| Guard anidado de biblioteca | `RackProjectStore.cs:281` (proyecto) y `:314` (cabecera); FlowBed/Larguero/PushBack/Cantilever en `SystemRegistry.Default.cs:129,151,175,237`. **`SelectiveRack` NO tiene guard** | guard anidado nuevo | **G15** |
| Listado de biblioteca | `RackDesignLibrary.cs:94` — `catch { }` desnudo | incompatible ⇒ **no abrible + diagnostico visible** | **G15** |
| Superficie de UI | `MainMenuAction` (enum pequeno a proposito) · `RackMenuCommands.cs:36-40` (precedente `GenerateStructuralSection`) | entrada de menu + comando, DTOs puros, intents | **G16** |
| Binding piloto en el editor | `RackSelectiveWindow.BuildDesign` / `BuildSystem` / `lastSystem` (`RackSelectiveWindow.xaml.cs`) | split authored/effective + chip K3 | **G12, G17** |

---

## 3. Descomposicion, y las cuatro desviaciones del orden sugerido

El orden conceptual pedido —datos puros → semantica → persistencia → autoridad → planificacion →
transaccion fisica → consumidores → BOM → edicion → UI → slice → validacion— **se conserva**. Cuatro
cosas se mueven, y cada una tiene su razon en el arbol real.

| # | Desviacion | Por que |
|---|---|---|
| **D1** | El **`ProjectVariablesDocument`** (G2) y el **binding + sticky del Selectivo** (G3) son gates **distintos** | Son **dos lineas de version independientes** (V4.8, D-01-bis §relacion). Fundirlas invita justamente al error que C4.8-1 corrige: aplicar la regla de presencia a los ocho DTO. **C4.8-1 gobierna SOLO el `ProjectVariablesDocument`**; `SelectivePalletDesignDocument` **conserva** su inicializador |
| **D2** | El **portador authored** se parte en **G3** (puro) y **G10** (adopcion real en el camino de guardado) | El portador puro es un DTO y se prueba en Core. Su adopcion vive en `RackSelectivoCommands.cs:232`, en el **Plugin**, que ninguna suite carga. Son dos clases de trabajo con dos clases de evidencia |
| **D3** | **G10 va ANTES de la propagacion (G11), no despues** | Es la unica dependencia **invertida** en el orden sugerido, y es material: G11 «escribe los payloads authored de todas las vistas». Si ese camino sigue haciendo `From(design, id, name)`, **cada redibujo destruye el binding que acaba de crearse**. La propagacion no puede ser correcta antes que el portador |
| **D4** | La **frontera transaccional (G9)** no depende de ningun gate de variables | `SystemBlockWriter` y `LateralHeaderDrawService` son infraestructura de dibujo existente. G9 solo depende de G0. Es el gate **mas caro y mas arriesgado**, y puede empezar en paralelo conceptual con toda la mitad pura — eso cambia el camino critico (§5) |

**Correspondencia con la lista sugerida:** G0→G0 · G1→G1 · G2→**G2+G3** · G3→**G3+G4** · G4→G5 ·
G5→G6 · G6→G7 · G7→G8 · G8→G9 · G9→**G10+G11** · G10→G12 · G11→G13 · G12→G14 · G13→G15 · G14→G16 ·
G15→G17 · G16→G18 · Owner Validation→**OV**.

**Diecinueve gates es mas de lo que WORKFLOW §2 llama una iniciativa** («cabe en 1-3 sesiones; si
crece mas, se parte»). Se declara aqui y **no se resuelve por cuenta propia**: partir I-47 es decision
del Owner y exige filas de ROADMAP. Lo unico que este plan hace es dejar la **linea de corte natural**
señalada en §5.

---

## 4. Los gates

### 4.1 Matriz

| Gate | Objetivo (una frase) | Dep. | Riesgo | AutoCAD | Suite |
|---|---|---|---|---|---|
| **G0** | Demostrar el estado real antes del primer cambio | — | LOW | build | — |
| **G1** | El modelo puro de Project Variables existe y es tipado | G0 | LOW | no | Core |
| **G2** | El registro del dibujo es un documento persistido de pleno derecho | G1 | MEDIUM | no | Core |
| **G3** | Una propiedad del Selectivo puede llevar un binding persistido sin perderlo | G1, G2 | **HIGH** | no | Core |
| **G4** | Existe **un** punto de resolucion authored → effective | G1, G3 | **HIGH** | no | Core |
| **G5** | Se sabe, sin adivinar, quien consume una variable y quien es la autoridad | G3 | **HIGH** | no | Core |
| **G6** | Las ocho operaciones producen un plan puro, o nada | G4, G5 | **HIGH** | no | Core |
| **G7** | El registro sobrevive a guardar y reabrir el DWG | G2 | MEDIUM | **si** | — |
| **G8** | El barrido fisico no convierte un sobre ilegible en inexistente | G5 | MEDIUM | **si** | Core (mitad) |
| **G9** | Los dos writers pueden participar en una transaccion ajena | G0 | **HIGH** | **si** | — |
| **G10** | Guardar un Selectivo deja de reconstruir el documento | G3, G4 | **HIGH** | **si** | Core (guard) |
| **G11** | Cambiar una variable redibuja todos sus consumidores en UNA operacion | G6-G10 | **HIGH** | **si** | Core (plan) |
| **G12** | `RACKEDITAR` muestra el efectivo y no desvincula al guardar | G4, G10 | MEDIUM | **si** | UI |
| **G13** | El BOM cotiza el efectivo, con autoridad y sin totales que parezcan completos | G4, G5, G8 | **HIGH** | **si** | Core |
| **G14** | Una copia independiente es completa o no existe | G3 | MEDIUM | **si** | Core |
| **G15** | La biblioteca sale literal-only y nada incompatible desaparece | G3, G4 | MEDIUM | no | Core |
| **G16** | Hay una superficie central donde viven las variables | G6, G7 | MEDIUM | no | UI |
| **G17** | La propiedad piloto se vincula y desvincula desde su editor | G12, G16 | MEDIUM | no | UI |
| **G18** | Lo implementado es lo que V4.8 dice, y nada mas | todos | LOW | no | Core + UI |
| **OV** | El dueno valida en AutoCAD 2025 lo que solo el dibujo puede validar | G18 | — | **si** | — |

`Riesgo` es **metadata de planificacion de I-47**, no un proceso de validacion nuevo: no añade
compuertas, no cambia que evidencia se exige y no aparece en ningun checklist de cierre.

### 4.2 Detalle por gate

Cada gate declara los catorce campos. `Regression` nombra suites **existentes** y verificadas.

---

#### G0 — Baseline / implementation preflight

- **Objetivo.** Fijar el estado medido del que parte la implementacion, para que cualquier regresion
  posterior sea atribuible.
- **Dependencias.** Ninguna.
- **Scope.** Core Full local · UI Full local · build Debug de UI · build Debug de Plugin · corrida de
  CI sobre el SHA · inventario de los simbolos que los gates van a tocar · registro de la deuda roja
  **preexistente** (las advertencias de analizadores xUnit que `docs/ideas-futuras.md` ya declara).
- **Non-scope.** **No se arregla deuda ajena.** No se toca produccion. No se toca `ideas-futuras.md`.
- **Archivos probables.** Ninguno de produccion. Un registro de evidencia bajo `docs/automation/`
  siguiendo el patron de `docs/automation/evidence/`.
- **RED.** No aplica: G0 no cambia comportamiento.
- **GREEN.** `BASELINE_SHA` + conteos de las dos suites + veredicto de los dos builds, todo contra
  **ese SHA exacto** y con el arbol limpio antes de medir (AGENTS, «Orden para la evidencia local»).
- **Regression.** Las dos suites completas — son el baseline mismo.
- **Riesgo.** LOW.
- **AutoCAD.** No (el build del Plugin exige AutoCAD 2025 instalado y **cerrado**; no exige dibujar).
- **Owner validation.** No.
- **Commit boundary.** Un commit documental con la evidencia. Cero produccion.
- **Stop conditions.** Si el baseline no esta verde, **la implementacion no empieza**: se reporta al
  Coordinator y se decide si es deuda ajena (se registra y se sigue) o un fallo real (se detiene).

---

#### G1 — Modelo puro de Project Variables

- **Objetivo.** Que los tres conceptos de V4.8 §1-bis existan como tipos separados y tipados.
- **Dependencias.** G0.
- **Scope.** `VariableId` (GUID, `OrdinalIgnoreCase`) · `VariableType` con **un** caso `Length`
  (pulgadas, ADR-0005; **sin conversion**) · `VariableDefinition` discriminada con **un** caso
  `Literal` · `ProjectVariable { VariableId, Name, Type, Definition }` ·
  `PropertyValue<T> = Literal(T) | ProjectVariableReference(VariableId)` · `PropertyId` como
  **constante en un solo sitio**, comparada **`Ordinal`**, con el unico valor
  `selective.verticalClearance`.
- **Non-scope.** Nada de AutoCAD, WPF, redibujo, NOD ni persistencia. **Ninguna** formula (ID22B),
  **ningun** `kind: rackProperty` (ID21), ningun segundo `PropertyId`.
- **Archivos probables.** `src/RackCad.Domain/` o `src/RackCad.Application/` segun donde caiga la
  frontera — el contrato no lo fija y **la eleccion se justifica en el commit**. Constantes al estilo
  de `SelectiveRackDefaults.LengthParam` (precedente citado por V4.8 §1-bis).
- **RED.** `VariableId` distinto solo en caja ⇒ **misma** identidad · `PropertyId` distinto solo en
  caja ⇒ **distinto** · `VariableType` desconocido ⇒ error semantico · `Definition.kind` desconocido
  ⇒ error semantico · `PropertyValue` sin `kind` explicito ⇒ no compila o no construye.
- **GREEN.** Esas pruebas pasan y **la asimetria `OrdinalIgnoreCase` / `Ordinal` esta cubierta por una
  prueba propia** — es la que impide que alguien la «unifique» mas adelante.
- **Regression.** Ninguna suite existente toca estos tipos: son nuevos. Core completa igualmente.
- **Riesgo.** LOW.
- **AutoCAD / Owner validation.** No / No.
- **Commit boundary.** Los tipos + sus pruebas. Nada mas.
- **Stop conditions.** Si el modelo no se puede expresar sin tocar `SelectivePalletDesign` (Domain),
  **parar**: P7 retiro expresamente ese archivo del alcance.

---

#### G2 — `ProjectVariablesDocument`: schema, store, guard y regla de presencia

- **Objetivo.** Que el registro del dibujo tenga el mismo contrato de persistencia que sus hermanos, y
  que **`missing` nunca se lea como `"1.0"`**.
- **Dependencias.** G1.
- **Scope.** `ProjectVariablesDocument` con `SchemaVersion` **sin inicializador o con centinela
  equivalente** (C4.8-1), `[JsonExtensionData]` en la raiz **desde el dia 1**, y la lista de variables ·
  `ProjectVariablesStore` puro (`Serialize`/`Deserialize`) · guard propio · los **tres errores duros**
  (major superior, `VariableType` desconocido, `Definition.kind` desconocido) · la **version de
  escritura** (nuevo ⇒ `1.0`; mismo major con minor mayor ⇒ se preserva; major superior ⇒ ERROR sin
  escribir).
- **Non-scope.** **El NOD no entra** — G7. **No se toca `SchemaVersionPolicy`**: su politica global no
  cambia (C4.7-6). **No se quita el inicializador a ninguno de los otros siete DTO persistidos**:
  C4.8-1 gobierna **solo** este documento.
- **Archivos probables.** `src/RackCad.Application/Persistence/ProjectVariablesDocument.cs` y
  `ProjectVariablesStore.cs`, junto a `SchemaGuard.cs` / `SchemaVersionPolicy.cs` (que **se consumen**,
  no se editan).
- **RED.** Entrada **ausente** ⇒ registro vacio · presente y corrupta ⇒ **error duro, sin registro
  vacio y sin escritura** · presente **sin la propiedad** `SchemaVersion` ⇒ error duro sin escritura ·
  `SchemaVersion` en blanco o no parseable ⇒ error duro sin escritura · major superior ⇒ ERROR sin
  escritura · presente **con `SchemaVersion = "1.0"`** ⇒ **legible** · `ExtensionData` sobrevive al
  round-trip.
- **GREEN.** Pruebas **1, 30, 41 y 42**. La 41 y la 42 **se leen juntas**: la 41 sola la satisface una
  implementacion que rechace todo documento.
- **Regression.** `PersistenceVersioningTests`, `PersistenceUniformityTests`, `RackEmbedDocumentTests`
  — comprueban que la politica global **no** cambio.
- **Riesgo.** MEDIUM. La tentacion real es «armonizar» el DTO nuevo con el patron de los otros ocho.
- **AutoCAD / Owner validation.** No / No.
- **Commit boundary.** El documento, el store, el guard y las cuatro pruebas.
- **Stop conditions.** Si conservar la distincion `ausente` vs `"1.0"` exigiera cambiar
  `SchemaVersionPolicy`, **parar**: C4.7-6 lo prohibe expresamente.

---

#### G3 — Binding persistido en el Selectivo, promocion pegajosa y portador authored

- **Objetivo.** Que una propiedad del Selectivo pueda llevar una referencia y que ni el round-trip ni
  el schema la pierdan.
- **Dependencias.** G1, G2.
- **Scope.** `PropertyValues : PropertyId → { kind, variableId }` como **campo declarado** de
  `SelectivePalletDesignDocument` (D-14 lo hace obligatorio: el restamp re-serializa por el DTO) ·
  **`[JsonExtensionData]` en ese DTO**, que hoy **no tiene** · el helper
  `resolveStickyWriteVersion(stored, hasPropertyValues)` con sus **cuatro ramas, incluida la de
  ERROR** · la constante de **lectura `2.x`** separada de la de escritura · el **portador**: una forma
  pura de `load → edit → save` que preserva `SchemaVersion`, `PropertyValues`, el literal authored y
  `ExtensionData`.
- **Non-scope.** **No** se resuelve nada (G4). **No** se adopta el portador en el camino real de
  guardado (G10). **No** se toca `SchemaVersionPolicy.ResolveWriteVersion`, que el helper **usa** para
  el caso «mismo major, minor mayor». **No** se añade `[JsonExtensionData]` a
  `RackFrameProjectDocument`: V4.8 lo declara fuera.
- **Archivos probables.** `src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs`
  (lineas 18-20, 31, 87, 164) · `SelectivePalletDesignStore.cs` · un helper sticky nuevo en
  `Persistence/`.
- **RED.** `1.x` sin `PropertyValues` ⇒ sigue `1.x` · introducir `PropertyValues` ⇒ **`2.0`** ·
  almacenado `2.x` ⇒ **siempre** `2.x`, aunque se borre el ultimo binding · major superior al soportado
  ⇒ **ERROR**, sin escribir y sin degradar · `load → edit → save` conserva version, `PropertyValues`,
  literal y `ExtensionData` · `hasPropertyValues` se evalua sobre el documento **a escribir**.
- **GREEN.** Pruebas **10 y 11**. Y el round-trip existente del Selectivo sigue verde con documentos
  `1.x` sin `PropertyValues` — **el caso comun no se degrada**.
- **Regression.** `SelectivePalletDesignDocumentTests`, `PersistenceReopenPreservationTests`,
  `PersistenceEmbedInnerDesignTests`, `PersistenceUniformityTests`, `SystemKindPersistenceCharacterizationTests`.
- **Riesgo.** **HIGH** (P12): el DTO del Selectivo es el mas tocado del repositorio.
- **AutoCAD / Owner validation.** No / No.
- **Commit boundary.** El campo, el atributo, el helper sticky y el portador puro, con sus pruebas.
  **Un commit; no se parte el sticky de su rama de ERROR.**
- **Stop conditions.** Si `[JsonExtensionData]` rompe los golden del Selectivo de forma no trivial,
  **parar y reportar**: el contrato lo exige, pero el coste real es informacion para el Coordinator.

---

#### G4 — `SelectiveEffectiveDesignResolver`

- **Objetivo.** Que exista **un solo** sitio donde una referencia se convierte en un numero.
- **Dependencias.** G1, G3.
- **Scope.** `Resolve(authoredDocument, projectVariables) → SelectivePalletDesign` **puro**: sin
  AutoCAD, sin I/O, sin catalogo · `PropertyId` desconocido ⇒ **error visible** · `kind` desconocido ⇒
  **error visible** · `VariableId` inexistente ⇒ **error visible, sin fallback** · `ClearOverride`
  conserva **exactamente** su precedencia.
- **Non-scope.** **No** se desplaza todavia ningun consumidor (dibujo, BOM, preview): eso son G11, G12
  y G13. `ToDomain()` **sigue existiendo** y es, exactamente, el caso sin binding.
- **Archivos probables.**
  `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs` (nuevo) ·
  `SelectiveGeometryResolver.cs` **solo se lee** (`SeparationFor`, precedencia de `ClearOverride`).
- **RED.** Sin binding ⇒ gana el literal authored · con binding ⇒ gana la variable · cambiar la
  variable cambia el efectivo y **no** el literal authored · `VariableId` ausente ⇒ error que **nombra
  el rack y la variable** · `PropertyId` desconocido ⇒ error, **nunca** caida al literal · `kind`
  desconocido ⇒ error · `ClearOverride` presente ⇒ **gana sobre el efectivo**.
- **GREEN.** Pruebas **2, 8, 9 y 14**.
- **Regression.** `SelectiveGeometryResolverTests`, `SelectiveDimensionsTests`,
  `SelectiveFrontalBuilderTests`, `SelectiveLateralBuilderTests`, `SelectivePlantaPlanTests`,
  `SelectiveBomBuilderTests` — el efectivo sin binding debe ser **identico** al de hoy.
- **Riesgo.** **HIGH**: es la pieza que cierra B4, y un error aqui divide dibujo y BOM.
- **AutoCAD / Owner validation.** No / No.
- **Commit boundary.** El resolver y sus pruebas. **Ningun consumidor cambia todavia.**
- **Stop conditions.** Si el resolver necesitara el catalogo, **parar**: dejaria de ser puro y
  arrastraria I/O a la capa que el contrato exige testeable.

---

#### G5 — Autoridad authored multi-vista, probe tri-estado y clasificacion del sobre

- **Objetivo.** Saber, **demostrando y no suponiendo**, quien consume una variable y cual es la
  autoridad authored de un rack.
- **Dependencias.** G3.
- **Scope.** `mismaAutoridadAuthored(docs)` puro, comparacion **total** del estado authored persistido
  **incluido `ExtensionData`**, excluyendo **solo** `View` y `Section` del sobre · el **probe
  tri-estado** que recorre y valida **el mapa entero** antes de poder decir `NEGATIVE` · la
  clasificacion `DRAWING_ENVELOPE_INDETERMINATE` sobre una **proyeccion pura** del barrido · las dos
  familias: **A target-variable** (probe por vista **primero**, todas `NEGATIVE` ⇒ ignorar) y **B
  target-rack** (sin probe; igualdad authored del rack elegido) · el **diagnostico por
  atribuibilidad**: atribuible ⇒ nombra `RackId` y el dato; no atribuible ⇒ nombra `DefinitionId` y
  dice que el `RackId` **no puede determinarse**.
- **Non-scope.** **Nada de AutoCAD.** No construye `MutationPlan` (G6). No lee el NOD. No toca
  `RackBlockFinder` (G8). **No** se inventa un `RackId` jamas.
- **Archivos probables.** `src/RackCad.Application/` — modulo nuevo de descubrimiento/autoridad. El
  tipo `RackEnvelopeScan` vive en el Plugin (`RackBlockFinder.cs:95`), asi que este gate define su
  **proyeccion pura** y el Plugin la construye en G8.
- **RED.** Hermanas iguales ⇒ una sola autoridad · todas `POSITIVE` + authored divergente ⇒ **ABORTA
  nombrando el `RackId`**, en las **tres** formas de divergencia (literal, `PropertyValues`,
  `SchemaVersion`) · divergente pero **todas `NEGATIVE`** ⇒ **se ignora** · mezcla
  `POSITIVE`/`NEGATIVE` ⇒ ABORTA · Familia B sobre un rack `NEGATIVE` en todo ⇒ **si** entra ·
  `PropertyId` desconocido ⇒ `INDETERMINATE` · `kind` desconocido ⇒ `INDETERMINATE` · `variableId`
  malformado ⇒ `INDETERMINATE` · payload indescifrable ⇒ `INDETERMINATE` en **ambas** familias ·
  `ExtensionData` distinto y todo lo demas igual ⇒ **DIVERGENCIA**.
- **GREEN.** Pruebas **6, 15, 16, 18, 19, 20, 21, 25, 26 y 28**.
- **Regression.** Ninguna suite existente cubre esto. Core completa.
- **Riesgo.** **HIGH**: es donde `UNKNOWN` puede volver a caer en `NEGATIVE` por descuido.
- **AutoCAD / Owner validation.** No / No.
- **Commit boundary.** Comparador + probe + clasificacion + las diez pruebas. **Los tres centinelas
  (25, 26, 28) entran en el mismo commit que el probe**: separados, el probe pasa un dia sin ellos.
- **Stop conditions.** Si el comparador no puede ver `ExtensionData` porque G3 no lo preservo,
  **volver a G3**. No se compara «casi todo».

---

#### G6 — `VariableMutationPreflightResult` / `MutationPlan` y las ocho operaciones

- **Objetivo.** Que cada operacion produzca **un plan completo o nada**, sin tocar AutoCAD.
- **Dependencias.** G4, G5.
- **Scope.** `VariableMutationPreflightResult = Success | Error(mensaje)` + `MutationPlan`
  (`registryMutation` + `rackMutations` por `RackId` con `authoredOutput` y
  `effectiveOutputRequerido`), **sin `ObjectId`, `Database`, `Transaction`, `BlockTableRecord` ni
  `Editor`** · las **ocho** operaciones: `Create`, `Rename`, `ChangeValue`, `Delete`, `Link`,
  `Unlink`, `RepairBroken`, `UnlinkAllAndDelete` · la separacion **registry-only** (`Create`,
  `Rename`: sin scan, sin preflight de racks, sin redibujo, sin `Regen`, **pero escritura
  transaccional**) frente a **rack-affecting** · la union de consumidores para un change-set.
- **Non-scope.** Nada fisico. Ningun `Regen`. Ninguna UI. **No** se ejecuta el plan.
- **Archivos probables.** `src/RackCad.Application/` — modulo de mutacion de variables.
- **RED, por operacion.**
  - **`Link`**: conserva el literal authored · añade el binding · **promueve** el schema · resuelve el
    efectivo · el plan incluye **TODAS** las sibling views presentes.
  - **`Unlink`** sano: **escribe el efectivo en el literal** (paso activo) · quita el binding · el
    schema **no baja**.
  - **`RepairBroken`**, y en **este orden**, que es el que el ADR-0034 §11 fija: **no hay valor
    efectivo** · **no hay fallback automatico al literal authored** · la accion es **explicita y
    avisada** y no es alcanzable sin confirmacion · usa el **literal authored almacenado** · **elimina
    el binding roto** · **mantiene el schema promovido** · **solo despues** de eliminar el binding el
    literal vuelve a gobernar · se redibuja normalmente.
  - **`Delete`** con consumidores ⇒ **BLOQUEADO**: resumenes presentes, **sin** `registryMutation`,
    plan **vacio**.
  - **`Rename`**: registry-only, sin scan y **sin redibujo**.
  - **`Create`**: registry-only, sin consumer scan.
  - Cualquier abort ⇒ **`MutationPlan` VACIO**: ni registro, ni el rack divergente, ni **ningun otro
    consumidor**.
  - Sobre presente no interpretable ⇒ **no hay `Success`** y el plan queda vacio.
- **GREEN.** Pruebas **3, 4, 5, 7, 17, 27, 29, 31 y 32**.
- **Regression.** Core completa.
- **Riesgo.** **HIGH**.
- **AutoCAD / Owner validation.** No / No.
- **Commit boundary.** El artefacto y las ocho operaciones **juntos**. Partirlo por operacion dejaria
  un `MutationPlan` que unas operaciones vacian y otras no.
- **Stop conditions.** Si alguna operacion necesita un `ObjectId` para decidir **que** cambia, **parar**:
  la frontera esta mal puesta y el preflight semantico dejaria de ser verificable en Core.

---

#### G7 — `ProjectVariablesData`: NOD y Xrecord

- **Objetivo.** Que el registro viva dentro del DWG y sobreviva a guardar y reabrir.
- **Dependencias.** G2.
- **Scope.** `Database.NamedObjectsDictionaryId` abierto como `DBDictionary` → sub-`DBDictionary`
  **`RACKCAD_PROJECT`** → `Xrecord` con el JSON troceado a ≤255 (el troceado ya esta probado en
  `RackBlockData`) · lectura y escritura **transaccionales** · `ABSENT` ⇒ registro vacio ·
  `PRESENT_BUT_UNREADABLE` ⇒ **error visible, sin registro vacio y sin escritura**.
- **Non-scope.** **Application sigue AutoCAD-free.** Cero propagacion, cero redibujo, cero UI. El
  codigo del Plugin es **deliberadamente minimo**: todo lo demas ya vive en G2.
- **Archivos probables.** `src/RackCad.Plugin/` — un helper nuevo al estilo de
  `Systems/Shared/RackBlockData.cs`, que es el precedente de troceado.
- **RED.** Lo probable en Core ya lo cubre G2 (pruebas 1, 30, 41, 42). Lo que **queda
  necesariamente** fuera de CI: que el sub-diccionario se cree y se lea, y que `PURGE` no se lo lleve.
- **GREEN.** Core sigue verde y el helper compila con 0 errores propios; el resto es **developer
  smoke** (§8).
- **Regression.** Ninguna suite alcanza el Plugin. `RackBlockData` **no se modifica**.
- **Riesgo.** MEDIUM. **P1**: esta superficie no la cubre ninguna suite y no la cubrira.
- **AutoCAD.** **Si** — primer gate fisico. **Developer smoke minimo**: crear, guardar, cerrar,
  reabrir, leer.
- **Owner validation.** No todavia; el escenario formal es de **OV**.
- **Commit boundary.** Solo el acceso al NOD.
- **Stop conditions.** Si `PURGE` se lleva la entrada (**P2, no verificado en ningun sentido**),
  **parar y reportar al Coordinator**: es una premisa de D-01 y no se resuelve improvisando.

---

#### G8 — Descubrimiento fisico: el conteo deja de depender del payload

- **Objetivo.** Que un sobre ilegible **colocado** no se comporte como inexistente.
- **Dependencias.** G5.
- **Scope.** `RackBlockFinder.ScanEnvelopes` calcula `DirectReferenceCount` desde el
  `BlockTableRecord` **con independencia de que `RackEmbedDocument` deserialice** —
  `GetBlockReferenceIds` opera sobre el record y no depende del payload · construccion de la
  proyeccion pura que G5 clasifica.
- **Non-scope.** **No se cambia `RackEmbedStore.Deserialize`**: seguira devolviendo `null`, que es
  correcto para un barrido de nivel dibujo (V4.8, nota de alcance de A1). Lo que se gobierna es **que
  hace el contrato con ese `null`**. No se cambia todavia ningun consumidor (G11, G13).
- **Archivos probables.** `src/RackCad.Plugin/RackBlockFinder.cs` (lineas 78-81, 95-106).
- **RED.** La mitad pura: entrada con payload presente y sobre no interpretable ⇒
  `DRAWING_ENVELOPE_INDETERMINATE`, **no** `NEGATIVE` y **no** «rack ajeno» (esto es G5 y ya esta
  probado). Lo nuevo aqui es que **el conteo llegue**.
- **GREEN.** Core verde + **auditoria explicita de los tres call sites**:
  `RackCommandSupport.cs:122` (`includeReferenceCount: false`, no afectado),
  `RackInventarioCommands.cs:46` (**RACKLISTA**) y `RackInventarioCommands.BomTotal.cs:46`. Los dos
  descartan la entrada con `if (embed == null || …) continue;` **antes** de leer
  `DirectReferenceCount` —verificado en `b1205f3`—, asi que hoy el conteo de una entrada
  indescifrable **no les llega**. El cambio no deberia alterarles el comportamiento; **se comprueba
  igualmente**, porque RACKLISTA agrega con `Math.Max` y es el consumidor menos obvio.
- **Regression.** `RackEmbedDocumentTests`. Y **RACKLISTA es el consumidor no obvio**: su
  comportamiento se revisa en el mismo commit.
- **Riesgo.** MEDIUM, y la mayor parte es el efecto lateral sobre RACKLISTA.
- **AutoCAD.** **Si** (el cambio vive en el Plugin).
- **Owner validation.** No.
- **Commit boundary.** El cambio del conteo + la proyeccion + la auditoria de los tres call sites.
- **Stop conditions.** Si RACKLISTA cambia de comportamiento observable, **parar**: seria alcance
  fuera del slice.

---

#### G9 — Frontera transaccional: los dos writers bajo la transaccion del llamador

- **Objetivo.** Demostrar que frontal, planta y lateral pueden redefinirse bajo **una** transaccion
  ajena, **antes** de conectar nada de variables.
- **Dependencias.** G0.
- **Scope.** Una primitiva transaction-aware que cubra **`SystemBlockWriter.RedrawInPlace`** (frontal
  y planta, via `ViewBlockDraw`) **y `LateralHeaderDrawService.RedrawInPlace`** (lateral, **una llamada
  por corte**) · `EnsureForPlan` / cualquier importacion **fuera** de la mutacion · verificacion
  **read-only** de definiciones (`blockTable.Has` sobre `LooseInstances` + `Headers.SelectMany(Instances)`)
  · **purga acumulada despues del commit** · **UN** `Regen` al final.
- **Non-scope.** **Ninguna variable, ningun binding, ningun registro.** No se cambia
  `LateralHeaderDrawer.RedefineSystemBlock`, que **ya recibe la `Transaction`** y no cambia de firma.
- **Archivos probables.** `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs` (45-98) ·
  `Systems/Shared/ViewBlockDraw.cs` (61-84) · `Drawing/LateralHeaderDrawService.cs` (59-98) ·
  `RackSelectivoCommands.cs` (130, 154, 169) como llamador.
- **RED.** No es expresable en CI: **ninguna suite carga el Plugin**. La evidencia es el **developer
  smoke** y, en su forma final, **OV**.
- **GREEN.** Las dos suites siguen verdes · builds Debug limpios · smoke: **un** commit sobre
  frontal + planta + lateral, y **fallo inducido ⇒ cero commits parciales**.
- **Regression.** `SelectiveTransactionBoundaryGuardTests` (UI) es una **guarda de fuentes** sobre las
  firmas de los handlers de `RackSelectiveWindow.xaml.cs`: si este gate mueve una frontera, **la
  guarda se actualiza en la misma rama**.
- **Riesgo.** **HIGH**, el mas alto del plan. Es el unico gate que reescribe infraestructura de
  dibujo compartida por todos los sistemas.
- **AutoCAD.** **Si.** **Developer smoke obligatorio aqui**: es el punto donde el contrato deja de ser
  demostrable por CI y P9 dice explicitamente que hay que verificarlo ejecutando.
- **Owner validation.** No — pero **es el gate cuyo fallo mas caro seria descubrir en OV**.
- **Commit boundary.** La frontera de **los dos** writers en el mismo commit. Migrar uno solo deja un
  slice cuyo lateral sigue commiteando por su cuenta, que es exactamente B3.
- **Stop conditions.** Si redefinir el lateral bajo la transaccion del llamador resulta inviable en
  AutoCAD, **parar y devolver al Coordinator**: C3-6 dejo esta viabilidad fuera del contrato **y
  dentro de este gate**. No se improvisa un mecanismo alternativo.

---

#### G10 — El portador authored en el camino de guardado real

- **Objetivo.** Que guardar un Selectivo **actualice** su documento authored en vez de fabricar otro.
- **Dependencias.** G3, G4.
- **Scope.** `RackSelectivoCommands.SerializeSelectiveDesign` (linea 232) deja de ser
  `SelectivePalletDesignDocument.From(design, id, name)` como **unico** camino de guardado de un rack
  existente · el portador es **UNO por `RackId`** —el documento de la vista que el usuario eligio— y
  **no uno por vista**, para no destruir el restampado que hace de `RACKEDITAR` una reconciliacion ·
  el sobre sigue siendo **por sibling fisica**: `RackEmbedComposer.Compose` recibe como `source` **el
  sobre propio de esa vista**, nunca el representante.
- **Non-scope.** `From(...)` **se conserva** para su caso legitimo: crear un documento **nuevo** desde
  un diseno. No se reescribe la ventana WPF (G12/G17). No hay propagacion todavia (G11).
- **Archivos probables.** `src/RackCad.Plugin/RackSelectivoCommands.cs` (105, 129-169, 232, 239-248) ·
  `src/RackCad.Application/Persistence/RackEmbedComposer.cs` (**solo se consume**).
- **RED.** Prueba **35**, que **si** es de Core porque `RackEmbedComposer` vive en Application: dos
  sobres con `ExtensionData` distinto ⇒ `Compose` de cada uno conserva **el suyo**. Lo demas —que el
  camino real del Plugin use el portador— solo admite **guarda de fuentes**, que es lo unico que una
  suite puede afirmar sobre codigo que no puede ejecutar.
- **GREEN.** Prueba 35 verde · guarda de fuentes al estilo de
  `MainMenuStructuralSectionAccessGuardTests` · smoke: abrir un rack vinculado, guardar sin tocar
  nada, y **el binding sigue ahi**.
- **Regression.** `PersistenceReopenPreservationTests`, `PersistenceEmbedInnerDesignTests`.
- **Riesgo.** **HIGH** (P4: perdida silenciosa del vinculo).
- **AutoCAD.** **Si.**
- **Owner validation.** No; el escenario formal es de **OV**.
- **Commit boundary.** El portador adoptado + la prueba 35 + la guarda.
- **Stop conditions.** Si el flujo real de `RACKEDITAR` no pudiera garantizar que **guardar restampa
  todas las vistas desde la autoridad elegida**, eso se **señala como incumplimiento del requisito
  V4-01** y se decide entonces. **No** se sustituye por otro mecanismo de reconciliacion inventado
  aqui — V4.8 lo prohibe con esas palabras.

---

#### G11 — Propagacion end-to-end

- **Objetivo.** Que cambiar una variable actualice **todo lo ya dibujado** en **una** operacion.
- **Dependencias.** G6, G7, G8, G9, G10.
- **Scope.** El pipeline completo: descubrir → autoridad → **preflight semantico** (puro, G6) →
  **preflight fisico** (Plugin, read-only: `ObjectId`, planes de **todas** las vistas,
  `blockTable.Has`) → resolver → un `DocumentLock` y **una** transaccion → registro + geometria +
  payloads → **UN** commit → purga acumulada → **UN** `Regen`.
- **Non-scope.** Sin UI (G16, G17). Sin BOM (G13). **Sin `EnsureForPlan` dentro** — verificacion
  read-only y **abortar nombrando lo que falta**.
- **Archivos probables.** `src/RackCad.Plugin/` — el orquestador · `RackSelectivoCommands.cs` ·
  la frontera de G9.
- **RED.** Lo probable en Core ya esta en G6 (plan, destinos, abortos). Lo que este gate añade y **no**
  es expresable en CI: multiples `RackId`, multiples vistas, un rack no relacionado, `INDETERMINATE`,
  hermana divergente, definicion sin referencias insertadas, conjunto de consumidores vacio, y **fallo
  antes de mutar**.
- **GREEN.** Las dos suites verdes · **developer smoke** con un fallo inducido que deje **cero
  commits parciales**.
- **Regression.** Core + UI completas: este gate cambia el camino de dibujo del Selectivo.
- **Riesgo.** **HIGH.** **P6**: el coste de propagacion en un dibujo grande **no esta medido**, y una
  transaccion unica sobre N racks **agrava** el perfil.
- **AutoCAD.** **Si.** **Developer smoke.**
- **Owner validation.** No — pero es el gate que **OV** validara mas a fondo.
- **Commit boundary.** El pipeline entero. Un pipeline a medias es exactamente el estado que C-3
  prohibe.
- **Stop conditions.** Si el tiempo de propagacion resulta inaceptable en un dibujo realista,
  **parar y reportar la medicion**: C3-8 lo dejo como **metrica de implementacion**, no como decision
  de arquitectura, y quien decide que hacer con ella es el Owner.

---

#### G12 — `RACKEDITAR` con binding

- **Objetivo.** Que el editor muestre el efectivo y que guardar **no desvincule en silencio**.
- **Dependencias.** G4, G10.
- **Scope.** El editor recibe **valor efectivo + estado authored del binding** · el split
  authored/effective en `BuildDesign`/`BuildSystem`, de modo que lo que se **persiste** y lo que se
  **dibuja** dejen de ser el mismo objeto en un rack vinculado · con binding activo, Save **no** copia
  el textbox efectivo al literal · reconciliacion de la vista elegida · el binding sobrevive a una
  edicion normal.
- **Non-scope.** **No** se rediseña la ventana ni se adopta un patron WPF nuevo — V4.8 lo dice con
  esas palabras. El chip visual es **G17**. `ClearOverride` y ADR-0032 (pendiente vs comprometido)
  **no se tocan**: son autoridades distintas y su interaccion se comprueba, no se cambia.
- **Archivos probables.** `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs`
  (`BuildDesign`, `BuildSystem`, `lastSystem`, `ClearanceBox`) — **archivo caliente**, WORKFLOW §7 ·
  `src/RackCad.Plugin/RackSelectivoCommands.cs` (`EditSelective`, linea 46).
- **RED.** Suite UI sobre los DTO puros: con binding, el campo muestra el efectivo y **no** es
  editable como literal · Save con binding activo **no** escribe el literal · sin binding, todo se
  comporta **exactamente** como hoy.
- **GREEN.** UI verde + Core verde.
- **Regression.** `SelectiveEditorWindowTests`, `SelectivePendingEditorsTests`,
  `SelectiveStructuralCommitRegressionTests`, `SelectiveTransactionBoundaryGuardTests`,
  `SelectiveCommitCoverageTests`, `SelectiveWindowConstructionGuardTests`.
- **Riesgo.** MEDIUM, con P3 (desvinculado silencioso al guardar) como el modo de fallo a batir.
- **AutoCAD.** **Si** (el flujo completo), aunque la logica se prueba en la suite UI.
- **Owner validation.** No; escenario 5 de **OV**.
- **Commit boundary.** El split + sus pruebas de UI.
- **Stop conditions.** `SelectiveWindowTestSupport` es **el unico sitio autorizado** para construir
  `RackSelectiveWindow` (`SelectiveWindowConstructionGuardTests`). Si una prueba nueva necesitara
  saltarselo, **parar**.

---

#### G13 — BOM

- **Objetivo.** Que la cotizacion salga del efectivo, con autoridad demostrada, y que **ningun total
  parezca completo cuando no lo esta**.
- **Dependencias.** G4, G5, G8.
- **Scope, en dos mitades.**
  - **Pura (Application, suite Core).** `ResolveBomAuthoredAuthority(siblingPayloads)` con sus tres
    resultados —`Success(authored, payloadAprobado)`, `NoAuthority(UnreadableSibling)`,
    `NoAuthority(DivergentSiblings)`— y sus **nueve reglas**, de las que la 2 es la razon de ser:
    **jamas se filtra una hermana ilegible para seguir con las legibles** · el resultado tipado
    `BomBuildResult` con `Success` / `UnreadablePayload` / `BrokenProjectVariableReference` · el
    mensaje del cuarto canal en `RackBomOutputGate`, que **nombra rack + `PropertyId` + `VariableId`**.
  - **Plugin.** `IRackKindHandler.BuildBom` gana el tercer argumento `ProjectVariablesDocument` —**los
    cinco kinds no-Selectivo lo ignoran**— · `RACKBOMTOTAL` lee el NOD **una vez** y usa **el mismo
    snapshot** · reune **todas** las hermanas, obtiene la autoridad y entrega al handler el **embed
    representante ya aprobado** · **el resolver corre UNA vez, dentro del handler**.
- **Non-scope.** **`RACKLISTA` no entra y no lee el NOD.** `OutputBlockedReason` **no cambia**. Los
  tres canales historicos —kind sin handler, diagnostico bloqueante, payload ilegible— **se conservan
  exactamente**. **`RACKBOMTOTAL` no ejecuta el resolver** (N9 sigue cerrada).
- **Archivos probables.** `src/RackCad.Application/Bom/RackBomOutputGate.cs` · un artefacto de
  autoridad nuevo en Application · `src/RackCad.Plugin/KindHandlers/IRackKindHandler.cs` y **sus seis
  implementaciones** · `src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs` (46-61, 116, 152-158).
- **RED.** Hermanas iguales ⇒ `Success(authored)` · legibles pero divergentes ⇒
  `NoAuthority(DivergentSiblings)` y **ninguna** entrada de BOM · A legible + B indescifrable ⇒
  `NoAuthority(UnreadableSibling)` y **NO** `Success(A)` —la asercion es **sobre el caso devuelto**—
  · referencia rota ⇒ **ABORTA EL TOTAL** con el mensaje que nombra los tres · sobre indescifrable con
  `count == 0` ⇒ **IGNORABLE**, el total se construye con el resto · con `count > 0` ⇒ **ABORTA**, con
  diagnostico **no atribuible** que nombra `DefinitionId`, dice que `RackId`/`Kind` no pueden
  determinarse e indica que **esta colocado** · `BrokenProjectVariableReference` **nunca** viaja como
  `UnreadablePayload`, `null` ni excepcion generica.
- **GREEN.** Pruebas **22, 23, 24, 33, 36, 37 y 38**.
- **Regression.** `SelectiveBomBuilderTests`, `BomBuilderTests`, `SystemBomBuilderTests`,
  `ConsolidatedBomBuilderTests`, `PushBackBomCommandGuardTests`, `PushBackCompositeBomTests`,
  `FlowBedBomBuilderTests`, `LargueroBomBuilderTests`, `SelectiveGate8ATests`.
- **Riesgo.** **HIGH**: la firma es una **interfaz compartida por seis handlers**, y el `catch` de
  `BuildRackBom` (linea 158) puede volver a tragarse el canal nuevo.
- **AutoCAD.** **Si** (la mitad del comando).
- **Owner validation.** No; escenarios 12 y 13 de **OV**.
- **Commit boundary.** **Dos commits.** Primero la mitad pura con sus siete pruebas; despues la
  firma y el comando. Al reves, la firma cambiaria seis handlers antes de existir lo que justifica el
  cambio.
- **Stop conditions.** Si el `catch` de `BuildRackBom` no puede distinguir el resultado tipado de un
  fallo imprevisto, **parar**: seria §1-ter incumplida en el mismo sitio que A23.

---

#### G14 — Duplicacion y restamp fail-closed

- **Objetivo.** Que una copia independiente sea **completa o inexistente**.
- **Dependencias.** G3.
- **Scope.** `SelectiveDesignRestamp.Restamp(json, newId, copyName) → json` **puro** en Application ·
  `SelectiveKindHandler.RestampDesign` queda como **delegacion de una linea** · **guarda de texto** que
  impida reimplementarlo en el Plugin · **fail-closed**: si la transformacion falla, `RACKDUPLICAR` y
  la copia independiente de `RACKLAYOUT` **ABORTAN y la copia no se crea**.
- **Non-scope.** **No** se admite la alternativa «copia sin payload». **No** se cambian los kinds
  `dynamic`, `cama` ni `pushback`, que devuelven el JSON tal cual.
- **Archivos probables.** `src/RackCad.Application/Persistence/SelectiveDesignRestamp.cs` (nuevo) ·
  `src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs` (`RestampDesign`) ·
  `src/RackCad.Plugin/RackEnvelopeRestamp.cs` (**lineas 52-59: el `catch` que hoy devuelve el JSON
  original**) · `RackDuplicarCommands.cs:181-188` · `RackLayoutCommands.cs:234-235`.
- **RED.** `RackId` cambia · `Name` cambia · `PropertyValues` **se conserva con el mismo
  `VariableId`** · `SchemaVersion` se conserva · `ExtensionData` se conserva · **fallo de la
  transformacion ⇒ ni payload ni copia parcialmente re-estampada**: o la postcondicion indivisible
  completa, o **no hay copia**.
- **GREEN.** Pruebas **13 y 39**, mas la guarda de texto.
- **Regression.** `PersistenceEmbedInnerDesignTests`, `SystemKindPersistenceCharacterizationTests`.
- **Riesgo.** MEDIUM. Atenuante verificado: en `PlaceIndependentCopy`, `RestampEnvelope` se ejecuta
  **antes** de `CloneDefinition` y ambos dentro de la transaccion, asi que un throw aborta sin commit
  — la rama independiente de `RACKLAYOUT` hace lo mismo.
- **AutoCAD.** **Si** (los dos comandos).
- **Owner validation.** No; escenario 11 de **OV**.
- **Commit boundary.** Extraccion + fail-closed + guarda, juntos. Extraer sin cerrar el `catch` deja
  el defecto A24 intacto en su sitio.
- **Stop conditions.** Si abortar la copia rompiera un flujo existente de `RACKLAYOUT`, **parar**:
  seria alcance fuera del slice.

---

#### G15 — Biblioteca

- **Objetivo.** Que lo exportado sea portable y que nada incompatible desaparezca en silencio.
- **Dependencias.** G3, G4.
- **Scope.** Export de un Selectivo vinculado: **resolver el efectivo → materializarlo como literal →
  eliminar `PropertyValues` → escribir `SelectiveRack` literal-only en la linea `1.x`**, **sin**
  heredar el sticky · **invariante: ID22A nunca exporta un `SelectiveRack` promovido ni vinculado** ·
  binding roto ⇒ **fallo visible y ningun artefacto** · el `SchemaGuard` anidado que falta para
  `RackProjectDocument.SelectiveRack` · un archivo de biblioteca con `SelectiveRack` incompatible **no
  se ofrece como diseño abrible normal** pero **si produce diagnostico visible**.
- **Non-scope.** **No se diseña una UI nueva** — el mecanismo exacto del diagnostico **no es
  contractual**. **No** se atribuye al guard nuevo ninguna garantia retroactiva sobre binarios
  pre-I-47. Nada de fusion de variables entre dibujos (H3, sin iniciativa).
- **Archivos probables.** `src/RackCad.Application/Persistence/RackDesignLibrary.cs` (**linea 94: el
  `catch { }` desnudo**) · `RackProjectStore.cs` (junto a los guards de 281 y 314) ·
  `SystemRegistry.Default.cs` (129, 151, 175, 237 — los cuatro hermanos **si** guardados).
- **RED.** Export vinculado sano ⇒ literal-only, sin `PropertyValues`, en `1.x` · export con
  `VariableId` inexistente ⇒ **falla visiblemente** y **no** genera artefacto desde el authored
  congelado · `SelectiveRack` anidado de major superior ⇒ **no aparece como diseño normal Y existe
  diagnostico explicito**, es decir **no desaparece en silencio**.
- **GREEN.** Pruebas **12, 34 y 40**.
- **Regression.** `RackDesignLibraryTests`, `PersistenceLibraryTransportTests`,
  `PersistenceValidationTests`.
- **Riesgo.** MEDIUM. Ojo con la confusion de lineas: `RackProjectDocument.CurrentSchemaVersion` **ya
  es `"2.0"`**, y el `SelectiveRack` **anidado** debe salir en `1.x`. Son documentos distintos.
- **AutoCAD.** No — todo esto vive en Application.
- **Owner validation.** No; escenario 15 de **OV**.
- **Commit boundary.** Export + guard anidado + listado, juntos: el guard sin el listado queda
  **neutralizado** por el `catch`, que es literalmente A25.
- **Stop conditions.** Si el diagnostico del listado exigiera rediseñar la ventana de biblioteca,
  **parar**: el contrato dice expresamente que no se diseña UI nueva aqui.

---

#### G16 — Superficie central de Project Variables

- **Objetivo.** Que exista un sitio, uno solo, donde las variables se crean, se editan y se reparan.
- **Dependencias.** G6, G7.
- **Scope.** Entrada en `RackMainMenuWindow` + miembro nuevo en `MainMenuAction` (**precedente
  exacto**: `RackMenuCommands.cs:36-40`) **y** comando con alias · listar / crear / editar valor /
  renombrar / borrar · **resumenes de consumidores** · `UnlinkAllAndDelete` · **`RepairBroken` con su
  aviso previo**: no hay valor efectivo, se usara el literal authored, **la geometria puede cambiar**,
  y exige **confirmacion explicita** · estado roto visible con rack, `PropertyId` y `VariableId` que
  falta.
- **Non-scope.** **La UI no conoce AutoCAD**: cero `Database`, `ObjectId`, `Xrecord`, `Transaction`.
  Devuelve **intents**, no efectos. **No** se construye un panel generico de propiedades.
- **Archivos probables.** `src/RackCad.UI/` — ventana nueva · `src/RackCad.UI/MainMenuAction.cs` ·
  `src/RackCad.Plugin/RackMenuCommands.cs` · `src/RackCad.UI/RackCommandReference.cs` (el comando
  nuevo se registra ahi).
- **RED.** Suite UI sobre los **cinco DTO** de D-16-frontera: alta, renombrado, **borrado bloqueado
  con lista de consumidores**, `RepairBroken` inalcanzable sin confirmacion, estado roto pintado.
- **GREEN.** UI verde, y la ventana **clasificada** en uno de los cuatro arquetipos.
- **Regression.** `WindowCensusGuardTests` **obliga** a clasificar toda ventana nueva;
  `DialogWindowContractTests` aplica el contrato del arquetipo elegido. `UiSystemBoundaryGuardTests`.
- **Riesgo.** MEDIUM. El arquetipo se elige **al abrir el gate**, no al final.
- **AutoCAD.** No (la ventana). El comando si.
- **Owner validation.** No; escenarios 1, 8, 9 y 10 de **OV**.
- **Commit boundary.** Ventana + DTOs + entrada de menu + comando.
- **Stop conditions.** Si el enum `MainMenuAction` necesitara **payload**, **parar**: su propio
  comentario dice que esa es la señal de darle una jerarquia tipada, no de ensancharlo.

---

#### G17 — El binding piloto en el editor Selectivo

- **Objetivo.** Que `selective.verticalClearance` se pueda vincular y desvincular donde el usuario la
  edita.
- **Dependencias.** G12, G16.
- **Scope.** Literal **o** `ProjectVariable` compatible · `Link` · `Unlink` · mostrar el **efectivo** ·
  mostrar el estado authored/binding sin ambiguedad · **K3**: chip adyacente con el nombre de la
  variable, campo en solo lectura con el valor efectivo y boton **«Desvincular»** explicito ·
  incompatibilidad de tipo **bloqueada**.
- **Non-scope.** **Ninguna otra propiedad.** No se pinta el `BorderBrush` (**K2 colisiona** con
  `NumericField`, que es dueño de su borde y guarda/restaura la procedencia). No se usa el campo vacio
  como estado (**seria el tercer significado** — la leccion del estado mixto de Push Back).
- **Archivos probables.** `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml` y `.xaml.cs`
  (`ClearanceBox`, hoy un `TextBox` plano) — **archivo caliente**.
- **RED.** Chip presente ⇔ hay binding · campo de solo lectura con el efectivo · boton
  «Desvincular» produce un **intent**, no una mutacion · variable de tipo incompatible **no
  seleccionable**.
- **GREEN.** UI verde.
- **Regression.** `SelectiveEditorWindowTests`, `SelectiveShellMigrationTests`,
  `SelectiveTransactionBoundaryGuardTests`, `SelectiveWindowConstructionGuardTests`,
  `SelectionMatrixScopeGuardTests`.
- **Riesgo.** MEDIUM (**P5**).
- **AutoCAD / Owner validation.** No / No; escenarios 2 y 7 de **OV**.
- **Commit boundary.** El chip, el intent y sus pruebas.
- **Stop conditions.** Si el chip obligara a tocar `NumericField`, **parar**: K3 existe precisamente
  para no tocarlo.

---

#### G18 — Conformidad del vertical slice

- **Objetivo.** Comprobar que lo implementado es V4.8, y que no se implemento nada mas.
- **Dependencias.** Todos.
- **Scope.** Recorrer las **42** pruebas contractuales y mapear
  `prueba contractual → prueba ejecutable → costura de implementacion`, sin omitir ninguna · barrido
  adversarial de **§1-ter** buscando: fallback silencioso · `catch` tolerante decidiendo semantica ·
  identidad parcial · autoridad de sibling arbitraria · **un segundo resolver del efectivo** · valor
  por defecto de schema · transaccion parcial · fuga de AutoCAD a la UI · registro en
  `docs/ideas-futuras.md` de lo hallado y no arreglado.
- **Non-scope.** **No se añaden features.** No se arregla nada fuera del slice.
- **Archivos probables.** Documentacion + pruebas de conformidad. Produccion **solo** si aparece un
  incumplimiento del contrato.
- **RED / GREEN.** El mapa completo, sin huecos, y las dos suites verdes.
- **Regression.** Core Full + UI Full.
- **Riesgo.** LOW.
- **AutoCAD / Owner validation.** No / **precede a OV**.
- **Commit boundary.** Conformidad + `ideas-futuras.md`.
- **Stop conditions.** Cualquier prueba contractual sin prueba ejecutable **detiene el paso a OV**.

---

#### OV — Owner Validation en AutoCAD 2025

**No es un gate de codigo.** Es la compuerta formal previa al Candidate, y su alcance es el de V4.8
§8 mas el minimo de D-18. Los dieciseis escenarios estan en §8 de este plan.

---

## 5. Camino critico

El camino minimo que desbloquea el vertical slice —cambiar una variable y ver el dibujo cambiar— es:

```
G0 → G1 → G2 → G3 → G4 → G5 → G6 ──┐
                                    ├→ G11 → (G12) → G18 → OV
G0 → G9 ────────────────────────────┤
G2 → G7 ────────────────────────────┤
G5 → G8 ────────────────────────────┤
G3,G4 → G10 ────────────────────────┘
```

**Once gates hasta el primer efecto visible.** Dos observaciones que cambian como se secuencia:

1. **La cadena pura G1→G6 es larga y serial**, y es la que decide casi todo el contrato. No se puede
   acortar: cada gate consume el artefacto del anterior.
2. **G9 es el unico gate HIGH que no depende de la cadena pura.** Empezarlo tarde concentra el riesgo
   mas alto del plan justo antes de la integracion. Empezarlo pronto **acorta el camino critico
   real**, aunque no acorte la cadena de dependencias.

**Fuera del camino critico**, y por tanto aplazables sin bloquear el slice: **G13** (BOM), **G14**
(duplicacion), **G15** (biblioteca), **G16** y **G17** (UI). Son alcance comprometido —estan **dentro**
del contrato— pero **no** condicionan que la propagacion funcione.

> **La linea de corte natural, si el Owner decide partir I-47** (§3): `G0–G12 + G18 + OV` es un slice
> completo y demostrable —crear, vincular, propagar, editar—; `G13–G17` es el resto del alcance
> comprometido. **No se propone partirla**: se señala donde partiria sin romper nada.

---

## 6. Trabajo conceptualmente separable

**«Paralelizable» aqui significa separable en el diseño, no sesiones concurrentes.** Sigue rigiendo
`1 iniciativa = 1 rama = 1 worktree`, con sesiones **secuenciales** y relevo entre herramientas
(WORKFLOW §2 y §3).

Tres bloques no comparten archivos ni dependen de codigo inexistente, asi que pueden **reordenarse
libremente** dentro de la secuencia:

| Bloque | Gates | Archivos que toca | Solapamiento |
|---|---|---|---|
| **Cadena pura** | G1-G6 | `Application/Persistence/`, `Application/Systems/Selective/` | — |
| **Frontera transaccional** | G9 | `Plugin/Systems/Shared/`, `Plugin/Drawing/` | **cero** con la cadena pura |
| **Registro fisico** | G7 | `Plugin/` (helper nuevo) | **cero** con G9 |

**Lo que NO es separable, y conviene decirlo:** G8 y G13 comparten `RackInventarioCommands.*`; G10,
G11 y G12 comparten `RackSelectivoCommands.cs`; G12 y G17 comparten `RackSelectiveWindow.xaml.cs`.
Los tres son **archivos calientes** de WORKFLOW §7.

**Consecuencia para el ROADMAP:** mientras I-47 este viva, **ninguna otra iniciativa del Selectivo o
de Push Back** puede correr en paralelo (P7). Eso se refleja en la columna «se estorba con» **al
integrar**, que es el momento en que ROADMAP se toca.

---

## 7. Checkpoints de suite completa

Focal por gate; **Full** solo donde la costura compartida queda terminada, que es lo que le da sentido.

| Momento | Que se corre | Por que ahi |
|---|---|---|
| Cada gate | **Focal** + regresion impactada, con **el conteo que produjo** | AGENTS: «una seleccion que no selecciona nada es un FALLO» |
| **Tras G4** | **Full** (Core + UI) | El punto de entrada de dibujo/BOM/preview se desplaza de `ToDomain()` a `Resolve(...)`: es transversal al Selectivo entero |
| **Tras G6** | **Core Full** | Cierra la mitad pura del contrato: 24 de las 42 pruebas ya existen |
| **Tras G9** | **Full** + los dos builds Debug | Transaccion fisica: infraestructura compartida por **todos** los sistemas |
| **Tras G11** | **Full** | Propagacion end-to-end |
| **Tras G13** | **Full** | `IRackKindHandler.BuildBom` cambia de firma en **seis** implementaciones |
| **Tras G18** | **Full** | Slice completo, previo a Candidate |

Durante la iteracion ordinaria rige **LC-UI**: la suite de UI **no es obligatoria en local** antes del
push, y su evidencia la aporta el CI —con las cuatro condiciones exactas: `event = push`,
`job = ui-tests`, `conclusion = success`, `head_sha` = el SHA empujado—. **LC-UI no alcanza al
nucleo**, no reduce el Candidato y no reduce el cierre.

---

## 8. AutoCAD: developer smoke frente a Owner Validation

Son dos cosas distintas y no se sustituyen.

**Developer smoke** — diagnostico del implementador, no evidencia de proceso. Se justifica en tres
gates, y en ninguno mas:

| Gate | Que responde el smoke |
|---|---|
| **G7** | ¿El sub-diccionario del NOD se crea, se lee y sobrevive a guardar/reabrir? ¿Y a `PURGE`? |
| **G9** | ¿Frontal + planta + lateral caben bajo **una** transaccion con **un** commit? ¿Un fallo inducido deja cero commits parciales? |
| **G11** | ¿La propagacion recorre varios `RackId` y varias vistas sin dejar dos valores para la misma variable? ¿Cuanto tarda? (**P6**) |

**Owner Validation** — compuerta formal previa a Candidate/integracion, sobre el SHA **ya rebasado**.
Los dieciseis escenarios:

```
 1. crear variable
 2. vincular VerticalClearance
 3. cambiar la variable -> TODAS las vistas se actualizan
 4. save / close / reopen del mismo DWG conserva registro y bindings
 5. RACKEDITAR preserva el binding
 6. Rename preserva el binding (y no redibuja)
 7. Unlink sin salto geometrico
 8. RepairBroken visible y explicito
 9. Delete bloqueado con consumidores, listandolos
10. UnlinkAllAndDelete
11. RACKDUPLICAR: RackId nuevo + mismo VariableId
12. el BOM usa el efectivo
13. un binding roto bloquea el BOM
14. multi-vista (frontal + planta + lateral)
15. la biblioteca materializa el literal
16. DWG legado sin registro se comporta como siempre
```

Mas los tres que V4.8 §8 pide expresamente y que solo el dibujo puede dar: **lote transaccional con un
commit**, **fallo inducido ⇒ cero commits parciales**, y **definicion de bloque ausente ⇒ cero
mutacion, nombrando la que falta**.

**Fuera:** WBLOCK y cross-DWG. Diferidos por el dueño; el minimo exigible es el escenario 4.

---

## 9. Mapa de las 42 pruebas contractuales

Ninguna se omite.

| # | Que fija | Gate | Suite |
|---|---|---|---|
| 1 | `ProjectVariablesDocument`: schema, store, guard, `ExtensionData`, tres errores duros | **G2** | Core |
| 2 | Resolver: con binding gana la variable; sin binding, el authored | **G4** | Core |
| 3 | `Link`: authored congelado, binding añadido, schema promovido, efectivo resuelto | **G6** | Core |
| 4 | `Unlink`: el efectivo se escribe al literal, binding fuera, schema no baja | **G6** | Core |
| 5 | `RepairBroken`: authored, quita binding, conserva `2.x`, inalcanzable sin accion explicita | **G6** | Core |
| 6 | Probe y clasificacion de consumidores | **G5** | Core |
| 7 | Preflight y aborto: consumidor irresoluble ⇒ ninguna mutacion | **G6** | Core |
| 8 | `PropertyId` desconocido ⇒ error visible | **G4** | Core |
| 9 | `kind` de referencia desconocido ⇒ error visible | **G4** | Core |
| 10 | Monotonicidad del sticky, las cuatro ramas incluida la de ERROR | **G3** | Core |
| 11 | Preservacion authored en `load → edit → save` | **G3** | Core |
| 12 | Materializacion y schema de biblioteca: literal-only, `1.x` | **G15** | Core |
| 13 | El restamp conserva el binding y cambia `RackId`/`Name` | **G14** | Core |
| 14 | Precedencia de `ClearOverride`, sin cambios | **G4** | Core |
| 15 | Hermanas con misma snapshot ⇒ preflight PASA, una autoridad logica | **G5** | Core |
| 16 | Todas POSITIVE + authored divergente ⇒ ABORTA nombrando el `RackId` | **G5** | Core |
| 17 | Con abort, `MutationPlan` VACIO | **G6** | Core |
| 18 | Divergente pero todas NEGATIVE ⇒ se IGNORA y la operacion continua | **G5** | Core |
| 19 | Mezcla POSITIVE/NEGATIVE ⇒ ABORTA | **G5** | Core |
| 20 | `Link` sobre rack todo-NEGATIVE ⇒ SI entra al preflight (Familia B) | **G5** | Core |
| 21 | Payload indescifrable en candidato relevante ⇒ ABORTA, ambas familias | **G5** | Core |
| 22 | BOM: hermanas iguales ⇒ `Success(authored)` | **G13** | Core |
| 23 | BOM: divergentes ⇒ `NoAuthority(DivergentSiblings)`, ninguna entrada | **G13** | Core |
| 24 | BOM: A legible + B indescifrable ⇒ `NoAuthority(UnreadableSibling)`, **no** `Success(A)` | **G13** | Core |
| 25 | Centinela: `PropertyId` desconocido ⇒ `INDETERMINATE`, jamas `NEGATIVE` | **G5** | Core |
| 26 | Centinela: `kind` desconocido ⇒ `INDETERMINATE`, jamas `NEGATIVE` | **G5** | Core |
| 27 | Con cualquiera de los dos, operacion target-variable ABORTA, plan vacio | **G6** | Core |
| 28 | Centinela: `ExtensionData` distinto ⇒ DIVERGENCIA | **G5** | Core |
| 29 | Sobre presente no interpretable ⇒ sin `Success`, plan VACIO | **G6** | Core |
| 30 | Registro corrupto ⇒ error duro, no vacio, sin escritura | **G2** | Core |
| 31 | `Link` multi-vista: el plan incluye TODAS las sibling views | **G6** | Core |
| 32 | `Delete` con consumidores ⇒ BLOQUEADO, sin `registryMutation`, plan vacio | **G6** | Core |
| 33 | Referencia rota en el BOM ⇒ ABORTO TOTAL tipado, nombrando los tres | **G13** | Core |
| 34 | Export con `VariableId` inexistente ⇒ falla visible, sin artefacto | **G15** | Core |
| 35 | Preservacion del sobre por sibling: `Compose` conserva SU `ExtensionData` | **G10** | Core |
| 36 | BOM: indescifrable + `count == 0` ⇒ IGNORABLE, el total no aborta | **G13** | Core |
| 37 | BOM: indescifrable + `count > 0` ⇒ ABORTO TOTAL, diagnostico no atribuible | **G13** | Core |
| 38 | `BrokenProjectVariableReference` viaja como RESULTADO SEMANTICO | **G13** | Core |
| 39 | Fallo del restamp ⇒ ni payload ni copia parcial | **G14** | Core |
| 40 | `SelectiveRack` anidado incompatible ⇒ no abrible **Y** diagnostico | **G15** | Core |
| 41 | Registro sin la propiedad `SchemaVersion` ⇒ error duro sin escritura | **G2** | Core |
| 42 | Centinela positivo: `SchemaVersion = "1.0"` explicito ⇒ LEGIBLE | **G2** | Core |

**Reparto:** G2 → 4 · G3 → 2 · G4 → 4 · G5 → 10 · G6 → **9** · G10 → 1 · G13 → 7 · G14 → 2 ·
G15 → 3. **Suma 42, y las 42 son de la suite Core.**

La **31** conserva su division deliberada: **Core prueba el plan y sus destinos logicos**; la aplicacion fisica de frontal + planta + lateral bajo un commit la prueba el dueño. **No
se duplica la logica del writer dentro de la prueba.**

Y **la 3 y la 5 no se dan por probadas solo con Core**: la parte «se redibuja normalmente» de ambas
vive en G11 y se cierra en OV.

---

## 10. Invariantes del plan

Vinculantes para esta version y para cualquier posterior:

1. **Ningun gate anterior a G9 introduce una escritura sobre mas de un rack.** Antes de la frontera
   transaccional, un lote de N racks deja `k-1` commiteados si falla en el `k`.
2. **Ningun gate cierra en verde con una prueba que ya era verde antes de escribirlo.** RED primero, y
   **verificando el motivo del fallo**, no solo que falla.
3. **Una guarda de fuentes nunca sustituye a una prueba de comportamiento cuando el comportamiento es
   testeable.** Solo se admite donde ninguna suite puede cargar el codigo (Plugin).
4. **Ningun filtro de pruebas cierra un gate sin declarar cuantas selecciono.** Cero seleccionadas es
   un FALLO.
5. **Ningun gate añade un `skip` nuevo.**
6. **`UNKNOWN` no se convierte en `NEGATIVE`, `EMPTY`, `ABSENT` ni `SUCCESS` en ningun gate.** Cada
   gate que toca una capa tolerante lo declara en su commit.
7. **C4.8-1 gobierna solo el `ProjectVariablesDocument`.** Los otros ocho DTO persistidos **conservan**
   su inicializador de `SchemaVersion`.
8. **El Proposal V4.8 no se modifica.** Si la implementacion descubre que el contrato es incorrecto,
   **se para y se devuelve al Coordinator**; no se corrige el contrato desde la implementacion.
9. **`ROADMAP.md` y `HANDOFF.md` no se tocan hasta la sesion de integracion.**

---

## 11. Riesgos concretos del arbol, y stop conditions

Solo lo verificado en `b1205f3`. Los riesgos de contrato (P1-P12) siguen vigentes en V4.8 §13 y no se
repiten aqui.

| # | Riesgo verificado | Gate | Stop condition |
|---|---|---|---|
| **R1** | `SelectivePalletDesignStore.Deserialize` **lanza**, no devuelve `null`. El `?.` de `SelectiveKindHandler.BuildBom` **nunca se dispara**: el camino real es el `catch` de `BuildRackBom` | G13 | Si el resultado tipado no puede distinguirse de un throw historico, parar |
| **R2** | `SelectivePalletDesignDocument` tiene **cero** `[JsonExtensionData]`. C4-2 lo exige y C4.5-3 lo **compara** | G3 | Si añadirlo rompe golden de forma no trivial, reportar el coste |
| **R3** | `RackProjectDocument.CurrentSchemaVersion` ya es **`"2.0"`**, y el `SelectiveRack` anidado debe salir en **`1.x`** | G15 | Confundir las dos lineas invalida D-15 |
| **R4** | `RackDesignLibrary.cs:94` es un `catch { }` **desnudo** que se traga cualquier guard anidado | G15 | El guard sin el listado **no cuenta como implementado** |
| **R5** | `RackEnvelopeRestamp.cs:52-59` devuelve el JSON original tras el `catch`: identidad parcial que parece una copia valida | G14 | Extraer sin cerrar el `catch` deja A24 vivo |
| **R6** | `RackBlockFinder.cs:78-79` ata el conteo a `embed != null`. **RACKLISTA** (`RackInventarioCommands.cs:46`) tambien lo consume | G8 | Si RACKLISTA cambia de comportamiento observable, parar |
| **R7** | `SelectiveTransactionBoundaryGuardTests` es una guarda de **fuentes** sobre firmas de handlers de `RackSelectiveWindow.xaml.cs` | G9, G12, G17 | Mover una frontera sin actualizar la guarda la deja protegiendo nada |
| **R8** | `WindowCensusGuardTests` obliga a clasificar toda ventana nueva en uno de cuatro arquetipos, y el arquetipo arrastra `DialogWindowContractTests` | G16 | El arquetipo se elige al abrir el gate |
| **R9** | `SelectiveWindowConstructionGuardTests`: `SelectiveWindowTestSupport` es el **unico** sitio autorizado para construir `RackSelectiveWindow` | G12, G17 | Ninguna prueba nueva se lo salta |
| **R10** | `NumericField` es dueño de su `BorderBrush` y guarda/restaura la procedencia | G17 | K2 esta descartada; si K3 obliga a tocarlo, parar |
| **R11** | Tres archivos calientes de WORKFLOW §7 quedan dentro del alcance: `RackSelectiveWindow.xaml.cs`, `Plugin/*Commands*.cs` y —**solo de lectura**— `SelectivePalletDesign.cs` | todos | Ninguna iniciativa de Selectivo o Push Back en paralelo |
| **R12** | El build del Plugin **falla si AutoCAD esta abierto** (MSB3021/MSB3027) | G7, G9-G14 | Cerrar AutoCAD antes de cada rebuild |
| **R13** | Diecinueve gates exceden el «1-3 sesiones» de WORKFLOW §2 | plan | Partir I-47 es **decision del Owner**; §5 señala la linea |

---

## 12. Candidate

**Nada de esto se ejecuta en esta sesion.** Tras G18 y OV, el Candidate es un **SHA exacto** que exige,
sobre **ese** SHA y con el arbol limpio **antes** de medir:

```
Core Full local
UI Full local                       (LC-UI no reduce el Candidato)
build Debug de UI
build Debug de Plugin
CI verde sobre el SHA exacto        (event = push, head_sha = ese SHA)
Owner Validation en AutoCAD 2025    (sobre el SHA YA rebasado)
cobertura segun el proceso vigente
```

Y despues, en la sesion de integracion: rebase final → CI → validacion → commit documental de cierre
(HANDOFF §8-12 + ROADMAP) → `merge --no-ff` → **CI posterior al merge** → limpieza. Son **tres SHAs
distintos con papeles distintos**, y ninguna evidencia se hereda entre ellos.

---

## 13. Revision adversarial de este plan

Las catorce preguntas, respondidas contra el plan ya corregido.

| # | Pregunta | Respuesta |
|---|---|---|
| 1 | ¿Algun gate requiere una pieza que no existe? | **No.** G11 necesita la frontera de G9; G13 necesita el conteo de G8; G16 necesita los DTO de G6 y el lector de G7. Todas son gates anteriores |
| 2 | ¿Algun gate mezcla semantica pura y AutoCAD innecesariamente? | **G8 y G13 lo hacen, y por eso estan partidos en mitades nombradas**, con las pruebas del lado puro. G7 deja en el Plugin **solo** el acceso al diccionario |
| 3 | ¿Alguna dependencia esta invertida? | **Habia una, y se corrigio (D3).** El portador (G10) tenia que ir **antes** de la propagacion, no despues: si no, cada redibujo destruye el binding recien creado |
| 4 | ¿El BOM puede implementarse sin segundo resolver? | **Si**, y es obligatorio: `RACKBOMTOTAL` entrega el **embed representante ya aprobado** y **el handler** resuelve **una vez**. G13 lo declara non-scope y G18 lo audita |
| 5 | ¿La UI puede empezar solo con sus DTO/intents disponibles? | **Si**: G16 depende de G6 (resumenes, estado roto) y G7 (lectura del registro). Antes de eso no hay nada que pintar |
| 6 | ¿`RepairBroken` conserva la secuencia final aceptada? | **Si**, y G6 la prueba **en orden**: sin valor efectivo · sin fallback automatico · explicita y avisada · usa el literal authored almacenado · elimina el binding · mantiene el schema · **despues** el literal gobierna · se redibuja |
| 7 | ¿Algun gate permitiria commit parcial? | **Invariante 1**: ningun gate anterior a G9 escribe sobre mas de un rack. G9 es el que hace imposible el parcial |
| 8 | ¿Hay un camino donde `UNKNOWN` vuelva a `NEGATIVE`/`EMPTY`/`SUCCESS`? | Los **seis** sitios estan asignados: probe (G5) · lectura del NOD (G2/G7) · barrido (G8) · listado de biblioteca (G15) · restamp (G14) · `catch` del BOM (G13). **Invariante 6** y auditoria en G18 |
| 9 | ¿La duplicacion puede crear identidad parcial? | **No tras G14**: postcondicion indivisible o **NO COPY**. Y el orden real ayuda — `RestampEnvelope` corre **antes** de `CloneDefinition`, dentro de la transaccion |
| 10 | ¿La biblioteca puede ocultar incompatibilidad? | **No tras G15**, que cierra el `catch` de `RackDesignLibrary.cs:94` **en el mismo commit** que el guard |
| 11 | ¿Un schema ausente puede reaparecer como `"1.0"`? | **No tras G2**: pruebas 41 **y** 42 leidas juntas. Y la **invariante 7** impide extender la regla a los otros ocho DTO |
| 12 | ¿El plan cubre las pruebas 1-42? | **Si**, §9, sin omitir ninguna, con el reparto por gate |
| 13 | ¿Se introdujo trabajo fuera del vertical slice? | **No.** Nada de `ClearHeight`, formulas, ID21, WBLOCK, panel generico ni conversion masiva. `SelectivePalletDesign.cs` (Domain) se lee, no se toca (P7) |
| 14 | ¿Owner Validation llega pronto o tarde? | **En su sitio**: una sola compuerta formal, tras G18. Lo que se adelanta es el **developer smoke** en G7, G9 y G11 — que es diagnostico, no evidencia de proceso |

---

## 14. Lo que este plan NO resuelve

Se declara en vez de disimularse.

1. **El coste de propagacion (P6)** sigue sin medir. G11 lo mide; **este plan no lo estima**.
2. **La viabilidad de la transaccion unica (P9)** solo se demuestra ejecutando. G9 la aborda y su
   stop condition devuelve al Coordinator si no cabe.
3. **`PURGE` sobre la entrada del NOD (P2)** no esta verificado **en ningun sentido**. G7 y OV.
4. **La particion de I-47** en varias iniciativas: se señala la linea de corte (§5) y **no se decide**.
5. **La ubicacion exacta del modelo puro** —Domain o Application— la fija G1 con su justificacion; el
   contrato no la impone.
6. **El mecanismo del diagnostico de biblioteca** no es contractual; G15 elige y lo justifica.
7. **La deuda de advertencias de analizadores xUnit** es **preexistente** y ajena: G0 la registra y
   **ningun gate la arregla de paso**.
