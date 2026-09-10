---
schema: rackcad-initiative/v1
id: I-48
title: "I-48 Discovery — wiring vigente de selective.verticalClearance"
type: architecture
status: claimed
branch: architecture/generic-linked-property-editing
base_branch: main
priority:
size:
depends_on: [I-47]
conflicts_with: []
context_packs: [ui-editors, system-selective, architecture-kernel, persistence]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-48 Discovery — el wiring vigente de `selective.verticalClearance`

> **Esto es G1: un informe.** No propone mecanismo, no recomienda implementacion y no disena la
> abstraccion. Donde hay una frontera que exige decision, se senala como **tension** y se deja a la
> Proposal (G2), que sigue bloqueada por el consenso de G3.

## 1. Baseline inspeccionado

```text
Rama            = architecture/generic-linked-property-editing
HEAD del informe= dfcd2c88fb8d255d7be55c411f9baf41458eadbd
Codigo auditado = e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main, punta de I-47)
```

Los tres commits de I-48 anteriores a este son **solo docs**, asi que el arbol de `src/` y `tests/`
auditado es **byte-identico** al de `origin/main`. `origin/main` no avanzo durante la sesion, de modo
que **no hubo rebase**.

## 2. Mapa end-to-end

Cada fila: archivo, simbolo, responsabilidad. La clasificacion va en la seccion 3.

### 2.1 Identidad y modelo

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `src/RackCad.Application/ProjectVariables/PropertyId.cs` | `PropertyId` (struct) | Identidad persistente de una propiedad. Comparacion **Ordinal**, sin alias ni trim. `TryParse` no lanza. |
| `src/RackCad.Application/ProjectVariables/ProjectPropertyIds.cs` | `SelectiveVerticalClearanceToken` (l.20), `SelectiveVerticalClearance` (l.23), `IsKnown` (l.26) | **Las propiedades vinculables que este build conoce.** Hoy: exactamente una. |
| `src/RackCad.Application/ProjectVariables/PropertyValue.cs` | `PropertyValue<T>`, `PropertyValueKind` | Literal vs referencia, discriminado por `Kind`. Pedir el literal a una referencia **lanza**, no devuelve `default(T)`. |
| `src/RackCad.Application/ProjectVariables/VariableId.cs` | `VariableId` | Identidad de la variable (`OrdinalIgnoreCase`, asimetrica respecto a `PropertyId` a proposito). |
| `src/RackCad.Application/ProjectVariables/VariableType.cs` | `VariableType.Length`, `VariableTypes.IsSupported` | Un solo tipo. Sin miembro cero. |
| `src/RackCad.Application/ProjectVariables/VariableDefinition.cs` | `VariableDefinition.Literal` | Un solo caso: literal. Sin formulas. |
| `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | `PalletTolerance` (l.27, def. 4.0), `VerticalClearance` (l.30, def. 6.0) | Los dos escalares globales del diseno. |

> **Observacion de modelo:** `PropertyValue<T>` es el tipo mas limpio del conjunto — y **no lo usa
> nadie** en el camino del Selectivo. Lo persistido es `SelectivePropertyValueDocument`, y lo que el
> resolver produce es un `double` suelto. `PropertyValue<T>` existe y esta probado, pero hoy es
> vocabulario, no camino.

### 2.2 Persistencia y esquema

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `src/RackCad.Application/Persistence/SelectivePropertyValueDocument.cs` | `Kind`, `VariableId`, `ExtensionData`, `ToProjectVariable` | Un vinculo persistido. `Kind` desconocido **no** cae al literal. |
| `.../SelectivePalletDesignDocument.cs` | `PropertyValues` (l.129) | **El mapa de vinculos**, `propertyId -> SelectivePropertyValueDocument`. Generico por construccion. |
| `.../SelectivePalletDesignDocument.cs` | `HasPropertyValues` (l.153), `HasBindingEntry` (l.170), `IsBound` (l.178), `TryGetBinding` (l.186) | PRESENCIA vs INTERPRETABILIDAD, deliberadamente separadas. Todas por `PropertyId`. |
| `.../SelectivePalletDesignDocument.cs` | `WithDesign` (l.304/316), en particular **l.324-327** | Reinyecta el diseno editado conservando `SchemaVersion`, `PropertyValues` y `ExtensionData`, y **preserva el literal congelado**. |
| `.../SelectivePalletDesignDocument.cs` | `CurrentSchemaVersion` `1.0` (l.24), `PromotedSchemaVersion` `2.0` (l.32) | Esquema **pegajoso**: se promueve al aparecer el primer vinculo. |
| `.../SelectivePalletDesignStore.cs` | l.31, usa `HasPropertyValues` | Decide la version de escritura. Generico. |
| `.../ProjectVariablesDocument.cs` | `ToProjectVariables()` (l.67-86) | Proyecta lo persistido al modelo puro. **Ver seccion 6.** |
| `.../ProjectVariablesStore.cs` | `IsKnownType` (l.210-211), uso en l.182 | Rechaza en LECTURA cualquier `Type` que no sea `Length`. |
| `.../ProjectVariablesReadResult.cs` | `Absent` / `Readable` / `PresentButUnreadable` / `IncompatibleMajor` | El registro presente-e-ilegible es un estado propio, nunca "vacio". |

### 2.3 Intents y opciones

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `.../ProjectVariables/SelectiveBindingIntent.cs` | `SelectiveBindingIntent.Link/Unlink` | El gesto, direccionado por identidad. `Unlink` no nombra variable. |
| idem | `ProjectVariableOption` | Lo que la UI muestra: id (autoridad), nombre (solo lectura humana), tipo, valor. |
| idem | `SelectiveBindingOptions.ForLength(registry)` | Filtra las variables **compatibles por tipo**. Lee `entry.Type` de lo persistido. |
| idem | `SelectiveBindingIntentPreflight.Run` | Valida `PropertyId` + `IsKnown` y despacha a `Link`/`Unlink`. **Un solo `switch`.** |
| `.../ProjectVariables/ProjectVariableIntent.cs` | `UnlinkAllAndDelete`, `RepairBroken`, … | Los intents de la ventana central. |

### 2.4 Preflight semantico

`src/RackCad.Application/ProjectVariables/ProjectVariableMutationPreflight.cs` — las ocho operaciones,
puras, con la regla «o plan COMPLETO o nada».

| Simbolo | Familia | Responsabilidad |
|---|---|---|
| `Create` / `Rename` | registry-only | No tocan rack, no barren, no redibujan. |
| `ChangeValue` | target-variable | Replanifica el redibujo de cada consumidor. **No** reescribe el literal autorado. |
| `Delete` | target-variable | **Bloquea** con consumidores y los lista. |
| `UnlinkAllAndDelete` | target-variable | Desvincula materializando y despues borra. Todo o nada. Usa el helper `Materialize` (l.355-380). |
| `Link` (l.205-235) | target-rack | Escribe `PropertyValues[propertyId.Value]`, promueve esquema, **no toca el literal** (lo congela por omision). |
| `Unlink` (l.245-285) | target-rack | Materializa el efectivo en el literal y quita el vinculo. **Linea 273 es el punto critico.** |
| `RepairBroken` (l.300-345) | target-rack | Solo sobre roto, exige `confirmed`, **no** materializa, quita el vinculo y deja gobernar el literal almacenado. |
| `Summarize` | proyeccion | Consumidores por `VariableId`, recorriendo `PropertyValues` genericamente. |

### 2.5 Resolucion efectiva

`src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs` — **el unico punto**
donde una referencia se vuelve numero.

- `Resolve(authored, projectVariables)` (l.44-73): indexa el registro, recorre `PropertyValues`,
  resuelve cada vinculo y construye el diseno efectivo.
- `TryResolveBinding` (l.80-140): cinco fallos nombrados y distinguidos — `UnknownPropertyId`,
  `UnknownReferenceKind`, `MalformedReference`, `BrokenProjectVariableReference` — cada uno nombrando
  rack, propiedad y variable.
- `Index` (l.150-165): registro nulo = **vacio**, que es legado valido; un rack que referencia algo en
  ese caso es un roto normal.

### 2.6 Apertura del editor y RACKEDITAR

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `.../Systems/Selective/SelectiveEditorOpen.cs` | `VerticalClearanceBindingState` (l.26-50) | El DTO que el editor recibe: `IsBound`, `EffectiveValue`, `BoundVariableName` (solo display). |
| idem | `SelectiveEditorOpenResult` (l.52-85), prop. `VerticalClearance` (l.70) | Abrir con estado, o **no abrir** y decir por que. |
| idem | `BoundName` (l.114-133) | Busca el nombre de la variable gobernante, para MOSTRAR. |
| idem | `Resolve` (l.137-180) | Bloquea con registro ilegible/incompatible **aunque el rack no este vinculado**; bloquea con vinculo roto; no repara. |
| `src/RackCad.Plugin/RackMenuCommands.cs` | `[CommandMethod("RACKEDITAR")]` (l.115), alias `RED` (l.18) | Punto de entrada. |
| `src/RackCad.Plugin/RackSelectivoCommands.cs` | l.66-89 | Lee el registro, llama `SelectiveEditorOpen.Resolve`, aborta si no abre, **cablea la ventana** (l.79-80) y despacha el gesto (l.83-88). |
| idem | `ApplyBinding` (l.279-300) | Corre el intent por el preflight y por el ejecutor. Generico. |
| idem | `ReadProjectVariables` (l.251-…) | Lee el registro en transaccion. |

### 2.7 Superficie WPF

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `.../Selective/RackSelectiveWindow.xaml` | `ToleranceBox` (l.131), `ClearanceBox` (l.137) | Los dos escalares, **contiguos, con los mismos handlers**. |
| idem | Panel de vinculo, l.143-167: `ClearanceStateText`, `ClearanceVariableBox`, `LinkClearanceButton`, `UnlinkClearanceButton` | El panel de vinculo. El comentario de l.143-144 declara: *«Especifico de esta propiedad a proposito: no es un panel generico de propiedades.»* |
| `.../RackSelectiveWindow.xaml.cs` | `GlobalScalar_LostFocus` (l.1608), `GlobalScalar_KeyDown` (l.1616) | Handlers **compartidos** por todos los escalares globales. Ambos solo llaman `Recompute()`. |
| idem | `BuildDesign` (l.2238-2270), l.2243-2244 | Parsea `ToleranceBox`/`ClearanceBox` con `UiSupport.TryNum` y valida `>= 0`. |
| idem | `BindingIntent` (l.2316) | **UN** gesto por sesion de ventana. |
| idem | `SetProjectVariables` (l.2323) | **UNA** `ItemsSource`. |
| idem | `LinkClearance_Click` (l.2332), `UnlinkClearance_Click` (l.2344), `AskBinding` (l.2358) | Construyen el intent con el token **literal** y **cierran la ventana**. |
| idem | `UpdateClearanceBindingActions` (l.2364) | Habilita/deshabilita segun `clearanceBound` (campo `bool`, l.2310). |
| idem | `ApplyVerticalClearanceBinding` (l.2586-2604) | Pone `ClearanceBox` en solo-lectura, cambia tooltip y escribe `ClearanceStateText`. |
| idem | `LoadExisting(doc, design, VerticalClearanceBindingState)` (l.2549-2558) | Firma **con el DTO especifico dentro**. |
| idem | `LoadDesign` l.2409-2410 | Siembra ambos TextBox con `ToString("0.###", InvariantCulture)`. |

### 2.8 Plan, ejecutor, dibujo y BOM

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `.../ProjectVariables/MutationPlan.cs` | `RegistryMutation`, `RackMutation`, `MutationPlan`, `VariableConsumerSummary`, `ProjectVariableCloning` | El plan. **Cero** menciones a propiedad concreta. |
| `src/RackCad.Plugin/ProjectVariableMutationExecutor.cs` | `Execute(document, plan)` | Escribe registro + racks en **una** transaccion. **Cero** menciones a `VerticalClearance` o `PropertyId` (verificado por grep vacio). |
| `.../ProjectVariables/ProjectVariableConsumerDiscovery.cs` | `DiscoverConsumers`, `ResolveTargetRack` | Quien consume una variable. Por `VariableId`. |
| `.../ProjectVariables/SelectiveAuthoredAuthority.cs` | `Resolve`, `IsSameAuthority`, `AreEquivalent` | Autoridad multi-vista por **comparacion estructural JSON include-by-default**. |
| `src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs` | `BuildBom` (l.36-70) | Resuelve **una vez** (l.58) y pasa el diseno **efectivo** a la geometria y al BOM. Un roto se reporta como `BrokenProjectVariableReference`, no como payload ilegible. |
| `.../Systems/Selective/SelectiveGeometryResolver.cs` | l.95-96, `ResolveFondo` (l.249), `BayBeamLength` (l.368), `AutoBeamLength` (l.387), `SeparationFor` (l.395), `Separation` (l.409) | Consume el diseno efectivo. **No conoce variables.** |
| `.../Systems/Selective/SelectiveLibraryExport.cs` | l.91 `document.PropertyValues = null` | La exportacion a biblioteca **materializa y desvincula**. Generico. |

### 2.9 RACKVARIABLES

| Archivo | Simbolo | Responsabilidad |
|---|---|---|
| `.../ProjectVariables/ProjectVariablesWorkspace.cs` | `Build` (l.135-…), `Placed`, `FirstPlacedUnclassifiable`, `FindBroken` (l.250-300) | La proyeccion pura de la ventana central. |
| idem | `ProjectVariableRow`, `BrokenBindingRow` | Filas. `BrokenBindingRow.StoredLiteral` = «lo que gobernara tras reparar». |
| `src/RackCad.Plugin/RackVariablesCommands.cs` | `[CommandMethod("RACKVARIABLES")]` (l.33), alias `RVA` (l.31) | Punto de entrada. Generico (l.70 imprime `consumer.PropertyIds`). |
| `src/RackCad.UI/RackProjectVariablesWindow.xaml.cs` | l.39, 59, 87, 112, 183-204 | La ventana. **Generica**: pasa `broken.PropertyId` tal cual (l.204) y lista `consumer.PropertyIds` (l.112). |

### 2.10 Consumidores adicionales encontrados (no estaban en la lista del encargo)

- `SelectiveLibraryExport` (§2.8) — desvincula al exportar.
- `SelectivePalletDesignStore` — decide la version de escritura por `HasPropertyValues`.
- `SelectiveAuthoredAuthority` — la comparacion multi-vista incluye `PropertyValues` y `SchemaVersion`
  **por ser estructural**, sin enumerarlos.
- `ProjectVariablesWriteGuard`, `ProjectVariablesPayload`, `ProjectVariablesRegistry`,
  `ProjectVariablesData` — el limite fisico del registro en el DWG.
- `ProjectVariableScanEntry` / `ProjectVariableScanProjection` — la barrida de racks colocados.

## 3. Tabla generic vs hardcoded

Clasificacion pedida. **`GENERIC`** = sirve a N propiedades sin tocarlo. **`PROPERTY-SPECIFIC`** = habla
de `VerticalClearance`. **`SELECTIVE-SPECIFIC`** = habla del sistema Selectivo. **`UI-SPECIFIC`** /
**`PLUGIN/AUTOCAD-SPECIFIC`** = de capa.

| Pieza | Clasificacion | Evidencia |
|---|---|---|
| `PropertyId` | `GENERIC` | Ningun token dentro del tipo. |
| `PropertyValue<T>` | `GENERIC` (sin uso en el camino) | Generico y probado; no participa. |
| `VariableId`, `VariableType`, `VariableDefinition` | `GENERIC` | — |
| **`ProjectPropertyIds.IsKnown`** | **`PROPERTY-SPECIFIC`** | `id == SelectiveVerticalClearance` (l.26). |
| `SelectivePropertyValueDocument` | `GENERIC` | Sin token. |
| `SelectivePalletDesignDocument.PropertyValues` | `GENERIC` + `SELECTIVE-SPECIFIC` (vive en el doc del Selectivo) | Mapa por clave. |
| `HasBindingEntry` / `IsBound` / `TryGetBinding` | `GENERIC` | Parametrizados por `PropertyId`. |
| **`WithDesign` (preservacion del congelado)** | **`PROPERTY-SPECIFIC`** | l.324-327: `if (HasBindingEntry(SelectiveVerticalClearance)) next.VerticalClearance = VerticalClearance;` |
| `SelectiveBindingIntent` | `GENERIC` + `SELECTIVE-SPECIFIC` (nombre y namespace) | Lleva `propertyId` como `string`. |
| `SelectiveBindingOptions.ForLength` | `GENERIC` **por tipo**, no por propiedad | No recibe `PropertyId`; el tipo esta en el **nombre del metodo**. |
| `SelectiveBindingIntentPreflight.Run` | `GENERIC` | Un `switch`; depende de `IsKnown`. |
| `Preflight.Create/Rename/ChangeValue/Delete` | `GENERIC` | No nombran propiedad. |
| **`Preflight.Unlink`** | **`PROPERTY-SPECIFIC` (firma generica, cuerpo no)** | l.273. |
| **`Preflight.Materialize` (helper de `UnlinkAllAndDelete`)** | **`PROPERTY-SPECIFIC` (doble)** | l.369 y l.370. |
| `Preflight.Link` | `GENERIC` | l.226-228 escriben por clave. |
| `Preflight.RepairBroken` | `GENERIC` | Solo quita la entrada; no materializa. |
| `Preflight.Summarize` | `GENERIC` | Recorre el mapa. |
| **`SelectiveEffectiveDesignResolver.Resolve`** | **`PROPERTY-SPECIFIC`** | l.51 y l.69. |
| `…Resolver.TryResolveBinding` | `GENERIC` | Devuelve un `double` sin decidir destino. |
| **`SelectiveEditorOpen` (DTO + `Resolve` + `BoundName`)** | **`PROPERTY-SPECIFIC`** | Tipo `VerticalClearanceBindingState`; l.121, l.173, l.175-177. |
| **`ProjectVariablesWorkspace.FindBroken`** | **`PROPERTY-SPECIFIC`** | l.292. |
| `ProjectVariablesWorkspace` (resto) | `GENERIC` | — |
| `MutationPlan` / `RackMutation` / `RegistryMutation` | `GENERIC` | — |
| `ProjectVariableMutationExecutor` | `GENERIC` + `PLUGIN/AUTOCAD-SPECIFIC` | grep de propiedad **vacio**. |
| `SelectiveAuthoredAuthority` | `GENERIC` | Comparacion estructural. |
| `SelectiveKindHandler.BuildBom` | `GENERIC` + `PLUGIN/AUTOCAD-SPECIFIC` | Consume efectivo. |
| `SelectiveGeometryResolver` | `GENERIC` respecto a variables + `SELECTIVE-SPECIFIC` | No conoce `VariableId`. |
| `SelectiveLibraryExport` | `GENERIC` | `PropertyValues = null`. |
| **Panel de vinculo del XAML (l.143-167)** | **`PROPERTY-SPECIFIC` + `UI-SPECIFIC`** | Declarado asi en su propio comentario. |
| **`RackSelectiveWindow` (binding: l.2310-2372, 2549-2604)** | **`PROPERTY-SPECIFIC` + `UI-SPECIFIC`** | `clearanceBound`, `BindingIntent` unico, tokens literales. |
| `GlobalScalar_LostFocus` / `_KeyDown` | `GENERIC` **dentro de la ventana** + `UI-SPECIFIC` | Compartidos por todos los escalares. |
| **`RackSelectivoCommands` l.79-80** | **`PROPERTY-SPECIFIC` + `PLUGIN/AUTOCAD-SPECIFIC`** | Pasa `open.VerticalClearance` y una sola lista. |
| `RackSelectivoCommands.ApplyBinding` | `GENERIC` + `PLUGIN/AUTOCAD-SPECIFIC` | — |
| `RackVariablesCommands` / `RackProjectVariablesWindow` | `GENERIC` (+ capa) | Pasan `PropertyId` como dato. |

## 4. Hardcodes ocultos — respuesta punto por punto

Las ocho preguntas del encargo, respondidas con evidencia. **API generica por firma ≠ comportamiento
generico**, y aqui esa distincion decide casi todo.

**4.1 ¿`SelectiveEffectiveDesignResolver` resuelve mas de un `PropertyId`?**
**NO.** `Resolve` (l.44-73) recorre **todos** los bindings, pero cada iteracion escribe en la **misma**
local `effectiveClearance` (l.63) y el resultado se asigna **solo** a `design.VerticalClearance`
(l.69). Todos los `PropertyId` terminan escribiendo `VerticalClearance`. Lo unico que hoy lo hace
seguro es que `TryResolveBinding` rechaza cualquier id que `IsKnown` no reconozca: **la guarda es lo
que protege, no el bucle**. Corolario: con dos ids conocidos, el ultimo del diccionario ganaria, con
orden no determinista.

**4.2 ¿`WithDesign()` preserva bindings de forma generica?**
**Parcialmente, y la parte que falta es la que importa.** El **mapa** se preserva genericamente
(l.321 `next.PropertyValues = PropertyValues`), igual que `SchemaVersion` y `ExtensionData`. Pero el
**literal congelado** se preserva **solo para `VerticalClearance`** (l.324-327), por constante. Para
una segunda propiedad vinculada, cada `WithDesign` sobrescribiria el literal congelado con el valor
efectivo — que es exactamente la perdida que la invariante 1 existe para impedir.

**4.3 ¿`Preflight.Unlink` materializa cualquier propiedad o solo `VerticalClearance`?**
**Solo `VerticalClearance`.** La firma recibe `PropertyId` y lo usa correctamente para **quitar** el
vinculo (l.274 `authored.PropertyValues.Remove(propertyId.Value)`), pero materializa con la linea fija
l.273 `authored.VerticalClearance = current.Design.VerticalClearance;`. Desvincular
`selective.palletTolerance` quitaria el vinculo correcto y escribiria **el campo equivocado**. Es el
caso de libro de «generica por firma, especifica por cuerpo».

**4.4 ¿`UnlinkAllAndDelete`/`Materialize` es verdaderamente generico?**
**NO, y es peor que 4.3.** El helper `Materialize` (l.355-380) **ni siquiera recibe** un `PropertyId`:
materializa `VerticalClearance` (l.369) y quita la entrada por la **constante** (l.370
`PropertyValues?.Remove(ProjectPropertyIds.SelectiveVerticalClearance.Value)`). Sobre una variable
consumida por otra propiedad, borraria la entrada de clearance —que quiza no existe— y dejaria intacto
el vinculo real, mientras el registro pierde la variable. Doble hardcode.

**4.5 ¿`ProjectVariablesWorkspace.FindBroken()` obtiene `StoredLiteral` por `PropertyId`?**
**NO: lo hardcodea.** l.292 pasa `entry.Authored.VerticalClearance` como `StoredLiteral`, sea cual sea
la propiedad rota que `resolution.PropertyId` acaba de nombrar. La ventana muestra ese numero
(`RackProjectVariablesWindow.xaml.cs` l.87) como «lo que gobernara tras reparar»: con una segunda
propiedad, el usuario decidiria una reparacion leyendo **el numero de otra propiedad**.
*Hallazgo adicional:* el resolver retorna en el **primer** fallo, y `FindBroken` deduplica por
`rackId|propertyId`, asi que un rack con dos vinculos rotos solo expone uno hasta reparar el primero.

**4.6 ¿`SelectiveEditorOpen` expone estado generico o un DTO exclusivo?**
**DTO exclusivo.** El tipo se llama `VerticalClearanceBindingState`, la propiedad del resultado se
llama `VerticalClearance` (l.70), `BoundName` (l.121) y `Resolve` (l.173) usan la constante, y
`EffectiveValue` se lee de `resolution.Design.VerticalClearance` (l.176). Anadir una segunda propiedad
**cambia la forma publica** de `SelectiveEditorOpenResult`.

**4.7 ¿Que wiring de `RackSelectiveWindow` existe exclusivamente para Clearance?**
El panel entero del XAML (l.143-167: `ClearanceStateText`, `ClearanceVariableBox`,
`LinkClearanceButton`, `UnlinkClearanceButton`) — cuyo comentario lo declara property-specific **a
proposito** — mas, en el `.cs`: el campo `clearanceBound` (l.2310), `SetProjectVariables` con **una
sola** `ItemsSource` (l.2325), `ClearanceVariable_SelectionChanged` (l.2329), `LinkClearance_Click`
(l.2332), `UnlinkClearance_Click` (l.2344), `UpdateClearanceBindingActions` (l.2364),
`ApplyVerticalClearanceBinding` (l.2586) y la firma de `LoadExisting` (l.2549).
**Ademas, un limite estructural, no cosmetico:** `BindingIntent` (l.2316) es **un** intent y
`AskBinding` (l.2358-2362) **cierra la ventana** al registrarlo. El modelo actual es «un gesto de
vinculo por apertura de editor»; con dos propiedades vinculables eso ya no es una restriccion de
pantalla sino de protocolo.

**4.8 ¿`RackSelectivoCommands` requiere conocimiento explicito de la propiedad?**
**SI, en dos lineas.** l.79 `window.SetProjectVariables(SelectiveBindingOptions.ForLength(...))` —una
lista, elegida por tipo— y l.80 `window.LoadExisting(saved, open.Design, open.VerticalClearance)` —el
DTO especifico—. El resto del comando (`ApplyBinding`, l.279-300) es generico.

**Lo que si resulto generico de verdad** (verificado, no supuesto): el ejecutor (grep de propiedad
**vacio**), `MutationPlan`, `SelectiveAuthoredAuthority`, `RepairBroken`, `Link`, `Summarize`,
`SelectiveLibraryExport`, `SelectiveKindHandler.BuildBom`, `RackVariablesCommands` y
`RackProjectVariablesWindow`.

## 5. Auditoria WPF y estado pendiente/comprometido

### 5.1 Como funcionan hoy los campos

| Elemento | Comportamiento verificado |
|---|---|
| `ClearanceBox` / `ToleranceBox` | `TextBox` **crudos** con `Style=FieldBox`, `Text` inicial `"6"` / `"4"`. **No** son `NumericField`. |
| `GlobalScalar_LostFocus` (l.1608) | `if (!initialized) return; Recompute();` — nada mas. |
| `GlobalScalar_KeyDown` (l.1616) | Solo `Key.Enter`: `Recompute(); e.Handled = true;`. |
| **Enter** | Confirma (LostFocus no dispararia: Enter no mueve el foco). |
| **LostFocus** | Confirma. Es el «live-apply on leave-field» documentado en l.1605-1607. |
| **Escape** | **NO EXISTE** para estos campos. No hay handler de `Key.Escape` que revierta el texto. |
| `BuildDesign` (l.2238) | l.2243-2244: `UiSupport.TryNum(...)` + `< 0.0` → error en espanol («Tolerancia horizontal inválida.» / «Holgura vertical inválida.»), `return null`. |
| Parseo localizado | `UiSupport.TryNum` → `LocalizedNumberParser.TryDouble`: acepta punto **y** coma, **sin** separador de millares, para que `"96,5"` nunca se vuelva `965`. |
| Errores de validacion | **No** hay borde rojo ni error por campo: el error viaja como `string` a `SetStatus(..., true)` en la barra de estado. |
| Recompute | Recorre el pipeline y termina fijando el estado; `pendingWarning` (l.2377) lo sobreescribe cuando algo se auto-reparo. |
| **Pendiente vs comprometido** | **NO EXISTE para estos dos campos.** `pendingAll` (l.446) = `{ pendingFondos, pendingBayCount, pendingDepth, pendingCabecera }`. `ClearanceBox` y `ToleranceBox` **no** estan. El `TextBox.Text` **es** el modelo, leido directamente por `BuildDesign`. |
| Campo gobernado | `ApplyVerticalClearanceBinding` (l.2586) pone `IsReadOnly=true`, `IsEnabled=false` y cambia el tooltip. Se **muestra** deshabilitado, nunca oculto. |

### 5.2 Infraestructura reutilizable que ya existe

| Control | Ubicacion | Estado |
|---|---|---|
| `NumericField : TextBox` | `src/RackCad.UI/Controls/NumericField.cs` | **Existe y esta maduro**: `Value` (salida), `HasError`, `ErrorMessage`, `Status`, `IsOptional`, `IntegerOnly`, `Minimum`/`Maximum` (+ inclusividad), evento `Validated`, `SetNumber(...)`, borde rojo `#B00020` restaurando la procedencia original del brush. Compatible con `FieldBox`. |
| `NumericFieldValidation` | `.../Controls/NumericFieldValidation.cs` | El resultado tipado de la validacion. |
| `LocalizedNumberParser` | `src/RackCad.Application/Formatting/LocalizedNumberParser.cs` | `TryDouble` / `TryInteger`, culture-safe. Ya es la base de `UiSupport.TryNum`. |
| `PendingTextField<T>` + `IPendingTextField` | `.../Controls/PendingTextField.cs` | **Existe** (I-43 / ADR-0032): commit en **dos fases** (`prepare` sin mutar / `apply`), `IsDirty`, `Show` vs `ResetToCommitted`. Declara no saber nada del Selectivo. **Es `internal sealed`.** |
| `CatalogCombo` | `.../Controls/CatalogCombo.cs` | Combo de catalogo. |
| Autocomplete / popup / sugerencias | — | **NO EXISTE.** El grep de `IsEditable\|autocomplete\|Popup\|IsTextSearchEnabled\|suggestion` en `src/RackCad.UI` solo devuelve `RackProjectVariablesWindow` y `RackSelectiveWindow`, y en ninguno es un mecanismo de sugerencias reutilizable. |
| Input «vinculable»/referencia | — | **NO EXISTE.** Lo unico que hay es el panel property-specific de §4.7. |

**Dos asimetrias que conviene tener presentes, sin resolverlas aqui:**

1. `NumericField` **ya lo usan** Cantilever y Push Back (7 archivos). `RackSelectiveWindow` **no lo usa
   en absoluto** (grep = 0 en `.xaml` y en `.xaml.cs`). El editor que tiene la propiedad vinculable es
   precisamente el que no adopto el control.
2. `PendingTextField<T>` ya resuelve pendiente-vs-comprometido y esta probado, pero es `internal` y
   los dos campos en cuestion quedaron **fuera** del conjunto pendiente.

*(No se creo ningun control. Esto es inventario, como pedia el encargo.)*

## 6. Auditoria de `selective.palletTolerance`

| Dimension | Hallazgo |
|---|---|
| Declaracion (dominio) | `SelectivePalletDesign.PalletTolerance` — `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:27`. |
| Default | **`4.0`**, literal en el dominio. *(Asimetria menor: `PalletDepth` usa `SelectiveRackDefaults.DefaultPalletDepth`; tolerancia y holgura son literales.)* |
| DTO / persistencia | `SelectivePalletDesignDocument.PalletTolerance` (l.57, `double` **no** anulable), ida y vuelta en `From` (l.214) y `ToDomain` (l.338). |
| Legado | `double` plano sin sentinel: un doc antiguo sin el campo deserializa a `0.0`. **No** hay el `> 0.0 ? … : default` que el Dinamico si aplica (`DynamicRackSystemDocument` l.88-90). |
| Validacion | `RackSelectiveWindow.BuildDesign` l.2243: `TryNum` + `tolerance < 0.0` → «Tolerancia horizontal inválida.» **Admite 0**, igual que la holgura. |
| Editor | `ToleranceBox` (XAML l.131), **contiguo** a `ClearanceBox`, con los **mismos** `GlobalScalar_LostFocus`/`_KeyDown`. Sembrado en `LoadDesign` l.2409. |
| authored/effective hoy | **Solo authored.** No participa del mapa de vinculos; su valor efectivo es siempre su literal. |
| Consumidor geometrico | **Real y central.** `SelectiveGeometryResolver` l.95 lo lee **en la linea anterior** a `clearance` (l.96); ambos se pasan a la **misma** `ResolveFondo` (l.122, l.134, firma l.249). Gobierna `BeamLength` (l.257) via `BayBeamLength` (l.368) → `AutoBeamLength` (l.387): **`frente*count + tolerance*(count+1)`**. |
| BOM | **Si, y mas directo que la holgura.** Determina la **longitud del larguero**, que es un componente del BOM; la holgura actua sobre separaciones y altura de poste. |
| Dibujo / preview | Si: la longitud del larguero cambia la geometria en frontal y planta, y el preview WPF corre el mismo `BuildDesign`. |
| Granularidad | **Escalar global del rack** — exactamente la misma que `VerticalClearance`. |
| Overrides | `cell.BeamLengthOverride` (l.374-376) **puentea** la tolerancia por celda. **Simetrico** con `cell.ClearOverride` (l.397-399), que puentea la holgura. |

### Veredicto

**`selective.palletTolerance` SIGUE SIENDO UNA BUENA PROOF OF GENERALITY.** No es campo muerto: es una
propiedad funcional de primera clase que gobierna la longitud del larguero y llega al BOM y al dibujo.

Lo que la hace **buena** prueba —y no una prueba trucada— es que es **suficientemente parecida para
ser justa y suficientemente distinta para morder**:

- *Parecida*: mismo tipo (`Length`, pulgadas), misma granularidad (escalar global), mismo editor,
  mismos handlers, mismo resolver, misma clase de override por celda. Nada de la semantica de I-47
  necesita reinterpretarse para ella.
- *Distinta donde importa*: **escribe en un campo diferente del diseno**, que es exactamente el eje
  sobre el que colapsan los cuatro hardcodes de §4.1-4.5. Una propiedad que casualmente tambien
  escribiera `VerticalClearance` no probaria nada.

**Riesgo declarado, no resuelto:** su legado deserializa a `0.0` sin sentinel, y `0` es un valor
*valido* segun la validacion actual. Un `0` legado y un `0` tecleado son indistinguibles. Eso no
invalida la eleccion, pero es una diferencia real frente a la holgura y la Proposal debera decidir si
la toca o la declara fuera.

*(Sobre Dinamico y Push Back, solo lo que el encargo autoriza citar: ambos tienen su propio
`PalletTolerance` — `DynamicRackDesign` l.36, `PushBackEditorInputs` l.23 — con **otro** default
(`DynamicRackDefaults.DefaultPalletTolerance = 4.0`) y otro tratamiento del legado. Es la evidencia de
por que el `PropertyId` debe seguir siendo **system-qualified**: `palletTolerance` a secas nombraria
tres propiedades distintas. No se generaliza nada de esos sistemas.)*

## 7. Deuda de tipos heredada de I-47

**Confirmado: `ProjectVariablesDocument.ToProjectVariables()` sigue hardcodeando el tipo.**

`src/RackCad.Application/Persistence/ProjectVariablesDocument.cs` l.67-86; la linea culpable es la
**81**, que pasa `VariableType.Length` fijo. `entry.Type` —que existe (l.97) y esta documentado como
«A value this build does not know is a HARD error»— **no se lee en ningun punto del metodo**.

**Que lo contiene hoy:** `ProjectVariablesStore.IsKnownType` (l.210-211) rechaza en **lectura**
cualquier `Type` que no sea `Length`, y el documento entero se rechaza (uso en l.182). Una variable de
otro tipo **no puede llegar** a `ToProjectVariables()`. El hardcode y la realidad coinciden por
construccion, no por casualidad — pero coinciden por una guarda **externa al metodo**.

### Veredicto: `DEFER SAFE FOR I-48`

Con dos condiciones que hoy se cumplen y hay que vigilar:

1. I-48 **no anade un segundo `VariableType`** (su alcance lo prohibe: sin formulas, sin ID21).
2. `selective.palletTolerance` es **tambien** `Length`, asi que toda variable vinculable en I-48 es
   Length y el valor hardcodeado es el correcto.

Notese que el filtro de compatibilidad **no** depende del metodo: `SelectiveBindingOptions.ForLength`
lee `entry.Type` del **documento** y hace `Enum.TryParse` + `IsSupported` + `type != Length`. Es decir,
el camino que hoy decide compatibilidad ya es type-correcto.

### Que haria cambiar el veredicto a `FIX REQUIRED`

Cualquiera de estas decisiones **futuras** —ninguna tomada, y ninguna que este Discovery proponga—:

- **Derivar la compatibilidad del tipo declarado de la PROPIEDAD** en vez de fijarla en el nombre del
  metodo (`ForLength` → algo parametrizado). Si esa derivacion lee el tipo a traves de
  `ProjectVariable.Type`, leeria `Length` **siempre**, y ofreceria variables incompatibles como si lo
  fueran. Esto es lo mas probable que la Proposal se plantee, y es la razon de vigilar la deuda.
- **Anadir un segundo `VariableType`** (p. ej. el `DimensionStyle` que `VariableType.cs` ya anticipa):
  al relajarse `IsKnownType`, la guarda que contiene el hardcode desaparece.
- **Mostrar el tipo como dato de decision** en RACKVARIABLES: `ProjectVariableRow.Type` viene de
  `variable.Type`, o sea del hardcode. Hoy es inofensivo porque solo hay un tipo.

**No se arregla aqui**, conforme al encargo.

## 8. Invariantes heredados

| # | Invariante | Mecanismo que lo protege | Estado |
|---|---|---|---|
| 1 | **Link congela el literal** | `Preflight.Link` (l.226-228) escribe **solo** en `PropertyValues`; nunca toca el campo. Lo conserva `WithDesign` l.324-327. | **PROTEGIDO para clearance.** **EXCEPCION:** la conservacion es property-specific (§4.2); para otra propiedad no habria congelacion. |
| 2 | **Unlink materializa el efectivo** | `Preflight.Unlink` l.271-274: resuelve el efectivo **antes** y lo escribe en el literal. | **PROTEGIDO para clearance.** **EXCEPCION:** materializa el campo fijo (§4.3), y `Materialize` ni recibe la propiedad (§4.4). |
| 3 | **Un roto no cae en silencio al literal** | Triple: `TryResolveBinding` devuelve `BrokenProjectVariableReference`; `SelectiveEditorOpen.Resolve` **bloquea** la apertura; `Unlink` se niega y remite a la reparacion; `RepairBroken` exige `confirmed`. | **PROTEGIDO.** Sin excepcion de propiedad. |
| 4 | **Registro presente-pero-ilegible ≠ vacio** | `ProjectVariablesReadOutcome` separa `Absent` de `PresentButUnreadable`/`IncompatibleMajor`; `SelectiveEditorOpen` (l.152-162) y `Workspace.Build` (l.140-147) bloquean —el editor **incluso para un rack no vinculado**— y la ventana no ofrece «crea tu primera variable». | **PROTEGIDO.** Sin excepcion. |
| 5 | **La UI nunca resuelve por nombre; identidad = `VariableId`** | `LinkClearance_Click` l.2340-2341 envia `option.Id`; `Unlink` no nombra variable; `ProjectVariableOption.Name` esta documentado como no-identidad; `BoundName` es solo display; `SelectiveBindingIntent` lo declara. | **PROTEGIDO.** |
| 6 | **Geometry/BOM consumen efectivo y no conocen `VariableId`** | `SelectiveKindHandler.BuildBom` resuelve **una vez** (l.58) y pasa el efectivo; `SelectiveGeometryResolver` no menciona variables; la guarda `EL_DOMINIO_ENTERO_SIGUE_SIN_CONOCER_LAS_VARIABLES` de `ProjectVariablesConformanceTests` lo fija estructuralmente. | **PROTEGIDO.** |
| 7 | **La mutacion fisica vive en el ejecutor/transaccion** | `ProjectVariableMutationExecutor.Execute` es el unico escritor; la guarda `NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS` impide que Plugin y UI nombren `PropertyValues`; `AskBinding` solo **registra** el gesto. | **PROTEGIDO.** |

**Lectura de conjunto:** los invariantes **3, 4, 5, 6 y 7 son estructuralmente genericos** y no se
degradan al anadir propiedades. Los invariantes **1 y 2 —los dos que tocan el VALOR— estan protegidos
por codigo especifico de una propiedad**, y son exactamente los que se romperian en silencio.

## 9. Medicion BEFORE

**Pregunta:** que habria que modificar HOY, sin crear ninguna abstraccion nueva, para hacer vinculable
`selective.palletTolerance` con **toda** la semantica de I-47.

### Archivos productivos

| # | Ruta | Simbolo | Cambio que requeriria hoy |
|---|---|---|---|
| 1 | `src/RackCad.Application/ProjectVariables/ProjectPropertyIds.cs` | `SelectivePalletToleranceToken`, `SelectivePalletTolerance`, `IsKnown` (l.20-26) | Declarar el segundo token e id; `IsKnown` deja de ser una igualdad y pasa a ser una disyuncion. |
| 2 | `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs` | `Resolve` (l.51, l.63, l.69) | El bucle debe **despachar por `PropertyId`** al campo destino en vez de acumular en una local y volcarla en `VerticalClearance`. Es el cambio de fondo. |
| 3 | `src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs` | `WithDesign` (l.324-327) | Anadir la preservacion del literal congelado de `PalletTolerance` (segundo `if`, o equivalente). |
| 4 | `src/RackCad.Application/ProjectVariables/ProjectVariableMutationPreflight.cs` | `Unlink` (l.273) y `Materialize` (l.369-370) | Materializar **el campo de la propiedad**; y `Materialize` debe **recibir** el `PropertyId` en vez de usar la constante. |
| 5 | `src/RackCad.Application/ProjectVariables/ProjectVariablesWorkspace.cs` | `FindBroken` (l.292) | `StoredLiteral` debe leerse **del campo de la propiedad rota**, no de `VerticalClearance`. |
| 6 | `src/RackCad.Application/Systems/Selective/SelectiveEditorOpen.cs` | `VerticalClearanceBindingState` (l.26), `SelectiveEditorOpenResult.VerticalClearance` (l.70), `BoundName` (l.121), `Resolve` (l.173-177) | Segundo estado de vinculo. **Cambia la forma publica del resultado.** |
| 7 | `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml` | Panel l.143-167 | Segundo panel completo (estado + combo + dos botones) para la tolerancia. |
| 8 | `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs` | `clearanceBound` (l.2310), `BindingIntent` (l.2316), `SetProjectVariables` (l.2323), `*Clearance_Click` (l.2332/2344), `UpdateClearanceBindingActions` (l.2364), `ApplyVerticalClearanceBinding` (l.2586), `LoadExisting` (l.2549) | Duplicar campo, handlers, habilitacion y aplicacion; **y decidir el protocolo de `BindingIntent`**, que hoy es un gesto unico que cierra la ventana. |
| 9 | `src/RackCad.Plugin/RackSelectivoCommands.cs` | l.79-80 | Pasar la lista de opciones y el estado de vinculo **de la segunda propiedad**. |

**No requieren cambio** (verificado, no supuesto): `ProjectVariableMutationExecutor`, `MutationPlan`,
`SelectivePropertyValueDocument`, `SelectiveAuthoredAuthority`, `SelectiveKindHandler`,
`SelectiveGeometryResolver`, `SelectiveLibraryExport`, `SelectivePalletDesignStore`,
`RackVariablesCommands`, `RackProjectVariablesWindow`, `SelectiveBindingIntent`,
`SelectiveBindingIntentPreflight`, `Preflight.Link/RepairBroken/Summarize/Create/Rename/ChangeValue/Delete`.
`SelectiveBindingOptions.ForLength` tampoco — pero **por coincidencia de tipo**, no por diseno.

### Tests

| # | Archivo / clase | Contrato que habria que extender o duplicar |
|---|---|---|
| 1 | `tests/RackCad.Tests/ProjectVariablesModelTests.cs` | `ElUnicoPropertyIdDeID22A_EsElTokenContractual` (l.100) y `UnPropertyIdDesconocidoNoSeReconoce` (l.107). El **nombre y el contrato** («el unico») dejan de ser ciertos. |
| 2 | `tests/RackCad.Tests/SelectiveEffectiveDesignResolverTests.cs` | Resolucion **por propiedad**; que una vinculada no altere el campo de la otra; dos vinculos simultaneos. |
| 3 | `tests/RackCad.Tests/ProjectVariableMutationPreflightTests.cs` | `Unlink` materializa el campo correcto; `UnlinkAllAndDelete` sobre consumidor de la segunda propiedad; `RepairBroken` por propiedad. |
| 4 | `tests/RackCad.Tests/SelectiveEditorOpenTests.cs` | La forma del resultado cambia: todo el fixture de apertura. |
| 5 | `tests/RackCad.Tests/SelectiveAuthoredBindingTests.cs` | Congelacion del literal en `WithDesign` para la segunda propiedad. |
| 6 | `tests/RackCad.Tests/SelectiveAuthoredBindingPresenceTests.cs` | Presencia vs interpretabilidad con dos claves en el mapa. |
| 7 | `tests/RackCad.Tests/ProjectVariablesWorkspaceTests.cs` | `StoredLiteral` correcto por propiedad rota; rack con dos rotos. |
| 8 | `tests/RackCad.Tests/SelectiveBindingIntentTests.cs` | Intent y opciones para el segundo token. |
| 9 | `tests/RackCad.UI.Tests/SelectiveEditorBindingTests.cs` | Segundo panel; campo gobernado deshabilitado; protocolo de `BindingIntent`. |
| 10 | `tests/RackCad.UI.Tests/SelectiveBindingUiTests.cs` | Habilitacion de los botones de la segunda propiedad. |

```text
BEFORE_PRODUCT_FILES = 9
BEFORE_TEST_FILES    = 10
BEFORE_TOTAL_FILES   = 19
```

**Definicion de la cifra, para que el AFTER sea comparable:** `BEFORE_TEST_FILES` cuenta los archivos
cuyo **contrato deja de ser cierto** y hay que extender o duplicar. Documentacion **no** se cuenta.

**Conjunto extendido, declarado aparte y NO incluido en la cifra:** otros **5** archivos usan el token
como fixture y seguirian pasando sin tocarlos, pero necesitarian un caso de segunda propiedad para
*demostrar* que la semantica se sostiene de punta a punta —
`SelectiveDuplicationFailClosedTests.cs`, `SelectiveLibraryExportTests.cs`,
`SelectiveBomAuthorityTests.cs`, `SelectiveAuthoredCarrierAdoptionTests.cs`,
`ProjectVariableDiscoveryTests.cs`. Si la Proposal decide que la prueba de generalidad los exige, la
cifra comparable pasa a **24**. Se declara para que el AFTER no se mida contra una base distinta.

**Guarda que NO cambia:** `ProjectVariablesConformanceTests` — sus asertos son sobre formulas
(`LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO`), capas y unicidad del resolver, ninguno
sobre el numero de propiedades. I-48 no anade formulas, asi que siguen valiendo tal cual.

## 10. Hotspots

Ordenados por riesgo de fallo **silencioso**, que es el criterio que importa: un fallo ruidoso se
descubre; uno silencioso dibuja mal y cotiza mal.

| # | Hotspot | Riesgo si se toca mal |
|---|---|---|
| **H1** | `SelectiveEffectiveDesignResolver.Resolve` l.51/63/69 | **Maximo.** Todo binding termina en `VerticalClearance`. Con dos ids conocidos y sin despacho, el ultimo del diccionario gana con orden no determinista. Es el corazon. |
| **H2** | `Preflight.Materialize` l.369-370 | **Maximo.** Doble hardcode y **sin `PropertyId` en la firma**: hoy no hay ni por donde pasarle la propiedad. |
| **H3** | `Preflight.Unlink` l.273 | **Alto.** Quita el vinculo correcto y materializa el campo equivocado: el rack cambia de forma y nada falla. |
| **H4** | `WithDesign` l.324-327 | **Alto.** Sin la preservacion, cada guardado destruye el literal congelado — que es lo unico que la reparacion tiene. Silencioso por definicion. |
| **H5** | `ProjectPropertyIds.IsKnown` l.26 | **Alto por efecto palanca.** Es la guarda que hoy hace inofensivos H1 y H3. **Relajarla antes que ellos activa los demas hotspots.** El orden importa. |
| **H6** | `SelectiveEditorOpen` (DTO) | **Medio-alto.** Cambio de forma publica; propaga a `RackSelectivoCommands` y a la ventana. |
| **H7** | `Workspace.FindBroken` l.292 | **Medio.** No corrompe datos, pero el usuario decide una reparacion leyendo el literal de otra propiedad. |
| **H8** | `RackSelectiveWindow.BindingIntent` + `AskBinding` | **Medio, y de protocolo.** Un gesto por apertura, que ademas cierra la ventana. No es cosmetica. |
| **H9** | `SelectiveBindingOptions.ForLength` | **Bajo hoy, trampa manana.** Correcto por coincidencia de tipo; no recibe `PropertyId` y fija el tipo en el nombre del metodo. |
| **H10** | Legado `0.0` de `PalletTolerance` | **Bajo pero real.** Sin sentinel, y `0` es valido: legado y tecleado son indistinguibles. |

## 11. Hallazgos fuera de alcance

Se registran aqui; **no se arreglan**, y no son de I-48 salvo que la Proposal los reclame
explicitamente. Su destino natural es `docs/ideas-futuras.md` al cierre.

1. **`FindBroken` expone un solo roto por rack.** El resolver retorna en el primer fallo y `FindBroken`
   deduplica por `rackId|propertyId`: un rack con dos vinculos rotos obliga a reparar en dos pasadas.
   Invisible hoy (una sola propiedad).
2. **`PropertyValue<T>` no participa del camino real.** Tipo generico, limpio y probado, que hoy es
   vocabulario: lo persistido es `SelectivePropertyValueDocument` y el resolver produce un `double`.
3. **`RackSelectiveWindow` no adopto `NumericField`** pese a que Cantilever y Push Back si (7
   archivos). Es deuda de I-14/I-31, no de I-48.
4. **`PendingTextField<T>` es `internal`** y los dos escalares vinculables quedaron fuera del conjunto
   pendiente. Cualquier uso fuera de `RackCad.UI` chocaria con la accesibilidad.
5. **Los defaults de `PalletTolerance`/`VerticalClearance` son literales en el dominio** (`4.0`, `6.0`)
   mientras `PalletDepth` usa `SelectiveRackDefaults`. Inconsistencia menor.
6. **Legado sin sentinel de `PalletTolerance`** (H10).
7. **Deuda de tipos de `ToProjectVariables()`** — ya registrada por I-47; §7 la reconfirma y la clasifica.
8. **Tres `PalletTolerance` independientes** (Selectivo, Dinamico, Push Back) con defaults y trato del
   legado distintos. Solo se cita como evidencia de que el `PropertyId` debe seguir siendo
   system-qualified; **unificarlas no es de I-48**.

## 12. Preguntas que la Proposal debera resolver

Fronteras y tensiones detectadas. **Ninguna se responde aqui**: responderlas es disenar.

1. **Destino del valor resuelto.** El resolver produce un numero y hoy sabe a que campo va porque solo
   hay uno. ¿Como se declara ese destino sin que cada propiedad nueva vuelva a editar el resolver?
   (H1 es el hotspot; la forma del contrato es decision de G2.)
2. **Dónde vive el «que campo congela» de `WithDesign`.** Hoy es un `if` con constante (H4). La tension
   es entre la persistencia —que no deberia conocer el catalogo de propiedades— y el hecho de que es
   ella quien tiene el literal.
3. **`Materialize` sin `PropertyId` en la firma** (H2): ¿se le pasa la propiedad, o cambia el reparto de
   responsabilidades entre `Unlink` y `UnlinkAllAndDelete`?
4. **Compatibilidad por tipo.** `ForLength` es correcto por coincidencia (H9). Si la compatibilidad pasa
   a derivarse del tipo declarado **de la propiedad**, la deuda de §7 pasa de `DEFER SAFE` a
   `FIX REQUIRED`. ¿Se declara el tipo en la propiedad, y con que consecuencia sobre la deuda?
5. **Protocolo de gestos de vinculo** (H8): hoy uno por apertura y cerrando la ventana. ¿Sigue siendo
   uno, pasan a ser N, o cambia el ciclo de vida del editor?
6. **Forma publica de `SelectiveEditorOpenResult`** (H6): ¿DTO por propiedad, coleccion indexada, u
   otra cosa? Afecta a `RackSelectivoCommands` y a la ventana.
7. **Orden de las compuertas.** `IsKnown` (H5) es lo que hoy vuelve inofensivos H1 y H3. ¿En que orden
   se levantan, y que prueba acompana cada paso para que no exista un commit intermedio donde una
   segunda propiedad sea reconocible pero se resuelva mal?
8. **Alcance de la reutilizacion en WPF.** Existen `NumericField` y `PendingTextField<T>`; no existe
   nada de autocomplete ni de input de referencia. ¿Que se reutiliza, que se extiende, y queda el
   Selectivo adoptando `NumericField` dentro o fuera de I-48?
9. **`0` legado vs `0` tecleado** en `PalletTolerance` (H10): ¿se toca, o se declara fuera con la
   consecuencia asumida?
10. **Definicion de «coste» para el AFTER.** §9 fija `BEFORE = 19` (must-change) y declara un extendido
    de `24`. ¿Cual de las dos es la cifra contra la que se medira anadir la tercera propiedad?
