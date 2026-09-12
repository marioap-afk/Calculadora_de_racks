# I-50 — Discovery (G1): cotas por vista

> **G1 CERRADA.** Informe de **solo lectura**: describe el árbol y cierra con evidencia la pregunta de
> autoridad del contrato (§2). **Cero cambios de producción**: la rama solo contiene documentos. Las
> preguntas que este informe dejó abiertas las **decidió el Coordinador** (contrato, sección 12,
> decisiones `CD-01`..`CD-11`) y no se reabren aquí. La forma concreta del dato la fija la Proposal (G2).
>
> ```text
> Rama              = feature/cotas-independientes-por-vista
> Reclamo           = 97cc0ef66bf865175a8976e4e2452aa6a4e0d5e6   Claim-Id 4d9fb22f-3c68-4fa9-b4bf-52ff9d9b8252
> Contrato          = 4e22d91677691e82c6804a1ca5a3bdc1bed51fad
> Bootstrap (fila)  = a2fba4f280027f36f32d04298a48f975c999a3ee
> Código auditado   = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main; src/ y tests/ idénticos en la rama)
> ```

## 0. Resumen

1. **Solo tres sistemas dibujan cotas**: Selectivo, Dinámico y Push Back. Cantilever, Cama, Larguero y
   Cabecera no emiten ninguna (§3.2).
2. **La autoridad vigente es del RACK**: un único `DimensionDetail` y un único `DimensionStyle` por diseño
   gobiernan todas sus vistas (§4).
3. **Dos emisores puros** producen las cotas —`SelectiveDimensions` y `DynamicViewDecorations`, este
   último compartido por Dinámico y Push Back— y **un único materializador** las dibuja **dentro de la
   definición de bloque** (§5).
4. **La identidad de una representación es la definición de bloque más su sobre `(Id, View, Section)`**
   (ADR-0009, ADR-0010). Las referencias comparten la definición, y dos bloques con la misma terna son
   legales (§7).
5. **Toda vista se regenera desde UN diseño**, y las hermanas deben coincidir en el diseño completo
   (§7.4, §8).
6. **Conclusión de G1**: la autoridad correcta es **rack × tipo de vista, dentro del diseño**. La metadata
   por instancia contradice ADR-0009/0010 y la autoridad multivista (§9). El Coordinador la adoptó
   (`CD-01`, `CD-08`).

## 1. Método y límite de la evidencia

- Lectura del árbol en la punta de la rama, cuyo `src/` y `tests/` coincide con `origin/main` `a4d88f1`.
  **Toda cita `archivo:línea` se refiere a `a4d88f1`.**
- Las afirmaciones negativas («X no existe») se sostienen con búsquedas sobre **todo** `src/`; los patrones
  están en §3.2.
- La auditoría se hizo en cuatro barridos paralelos de solo lectura (Selectivo; Dinámico y Push Back;
  sistemas sin cotas, materialización y duplicación; UI, pruebas y fronteras). **Cada cita de este
  documento se re-verificó contra el árbol** antes de versionarlo.
- Lo marcado *(sin verificar en ejecución)* es lectura estática cuyo efecto en AutoCAD no se comprobó.

## 2. Preflight de G0

| Comprobación | Resultado |
|---|---|
| `main` = `origin/main` | `a4d88f1` = BASE_SHA |
| Reclamo | `97cc0ef`, padre `a4d88f1`, commit vacío |
| Contrato | `4e22d91`, padre `97cc0ef` |
| Operaciones a medias, stash | ninguna |
| Fila previa en ROADMAP | no existía (caso (d)); nace en `a2fba4f` con Estado `pendiente` |
| Worktree | `%USERPROFILE%\.codex\worktrees\feature-cotas-independientes-por-vista` |
| I-49 al abrir G1 | sin rama remota, sin fila y sin contrato |

El estado de las iniciativas paralelas al cerrar G1 está en §11.

## 3. Matriz por sistema

### 3.1 Los siete kinds de `SystemRegistry.Default`

`src/RackCad.Application/Systems/Shared/SystemRegistry.Default.cs:23-39` registra Selective (Cabecera),
PalletFlow (Dinámico), SelectiveRack (Selectivo), Cama, Larguero, PushBack y Cantilever.

| Sistema (kind, sobre) | Vistas reales (`View` / `Section`) | Cotas hoy | Autoridad vigente | Persistencia | Emisor / camino de dibujo | Cambio I-50 |
|---|---|---|---|---|---|---|
| **Selectivo** (`SelectiveRack`, `selective`) | `frontal` / fondo (un bloque por fondo) · `lateral` / `PostIndex` del corte · `planta` / -1 | Sí, 3 vistas | `SelectivePalletDesign.Dimensions` + `DimensionStyle`, copiados a `SelectiveRackSystem` | `SelectivePalletDesignDocument`, `int? Dimensions` | `SelectiveDimensions` ← `SelectiveFrontalBuilder` / `SelectiveLateralBuilder` / `SelectivePlantaBuilder` ← `RackSelectivoCommands` | Sí |
| **Dinámico** (`PalletFlow`, `dynamic`) | `frontal` / 0 salida, 1 entrada · `lateral` / poste · `planta` / -1 | Sí, 3 vistas | `DynamicRackDesign` ⇄ `DynamicRackSystem` | `RackProjectDocument.DynamicSystem` → `DynamicRackSystemDocument`, `int? Dimensions` | `DynamicViewDecorations` ← `DynamicSystem{Frontal,Lateral,Planta}Builder` ← `RackDinamicoCommands` | Sí |
| **Push Back** (`PushBack`, `pushback`) | `frontal` / 0-3 (extremo + lado) · `lateral` / poste (A y B en el mismo bloque) · `planta` / -1 (A y B dentro) | Sí, 3 vistas | la de su estructura dinámica | `PushBackDesignDocument.Structure` = `DynamicRackSystemDocument` | `DynamicViewDecorations` vía los builders Push Back y `PushBackSystemLateralBuilder.SideDecorations` | Sí (hereda el DTO dinámico) |
| Cabecera (`Selective`, `cabecera`) | `lateral` / -1 · `planta` / -1 | No | — | — | builders sin rol `Dimension` | Ninguno |
| Cama (`Cama`, `cama`) | un lateral (`View` nula) / -1 | No | — | — | `FlowBedLateralBuilder` | Ninguno |
| Larguero (`Larguero`) | ninguna vista en AutoCAD | No | — | — | sin camino de dibujo | Ninguno |
| Cantilever (`Cantilever`, `cantilever`) | `frontal` / -1 · `lateral` / estación · `planta` / -1 | No | — | — | `CantileverViewMaterializer` | Ninguno |

### 3.2 Prueba de ausencia

Patrones buscados sobre todo `src/`: `HeaderBlockRole\.Dimension`,
`RotatedDimension|AlignedDimension|OrdinateDimension|ArcDimension`,
`DimStyle|DimensionStyle|DimensionDetail|RACKCAD_COTAS` y `cota` sin distinguir mayúsculas.

- `Role = HeaderBlockRole.Dimension` se asigna en **exactamente dos** sitios:
  `src/RackCad.Application/Systems/Selective/SelectiveDimensions.cs:298` y
  `src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs:383`. `PushBackMirror.cs:50-69` solo
  **copia** instancias ya emitidas. Lectores del rol: `HeaderInstanceGrouper.cs:38`,
  `LateralHeaderDrawer.cs:287`, `PushBackPlanComposer.cs:135` y `PushBackPreviewModel.cs:118`.
- **Única entidad de cota** de AutoCAD en el producto: `new RotatedDimension` en
  `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:354`.
- **Cantilever**: `CantileverViewMaterializer.cs:222` solo construye círculos o polilíneas.
  `CantileverVisualRole.Annotation = 8` está declarado y **sin emisor** («Declared before anything emits
  it», `CantileverVisualRole.cs:56-59`). `CantileverViewKind` está en `CantileverViewPlanBuilder.cs:12`.
- **Cama**: `FlowBedDrawService.cs:30, :44` construye el plan sin instancias extra
  (`new HeaderRunPlan(new List<HeaderGroup>(), builder.Build(config, catalog))`).
- **Cabecera**: `LateralHeaderLayoutBuilder` (BasePlate, Post, Diagonal) y `PlantaHeaderLayoutBuilder`
  (Horizontal, Post, BasePlate) no emiten el rol. Ninguna llamada de `RackCabeceraCommands` al servicio
  lateral compartido pasa instancias extra (`:174, :269-270, :280-281, :302, :310`).
- **Larguero**: `EditorModules.cs:250` (`CanInsert => false`) y `:261` («Visual + BOM only (no AutoCAD
  block yet)»). No tiene constante de kind en `RackEmbedDocument.cs:16-26` ni handler en
  `KindHandlerRegistry.cs:57-62`.

## 4. Autoridad vigente

### 4.1 Un nivel y un estilo por rack

- `DimensionDetail` (`src/RackCad.Domain/Systems/Shared/DimensionDetail.cs:7-20`): None 0, Minimal 1,
  Standard 2, Detailed 3; cada nivel es superconjunto del anterior.
- **Selectivo**: `SelectivePalletDesign.cs:131-135` → `SelectiveGeometryResolver.cs:66-67` →
  `SelectiveRackSystem.cs:83-86` → vista por fondo en `SelectiveDepthLayout.FondoSystemView`
  (`SelectiveDepthLayout.cs:83-84`).
- **Dinámico**: `DynamicRackDesign.cs:75-76` ⇄ `DynamicRackSystem.cs:123-124`, a través de
  `DynamicRackSystemResolver.cs:244-245` (diseño → sistema) y `:357-358` (sistema → diseño).
- **Push Back**: la estructura es un `DynamicRackSystem`. Su único clonador escrito a mano es
  `PushBackMirror.Structure` (`PushBackMirror.cs:139-162`), que usan `PushBackMirror.Clone` (`:298-299`)
  y `PushBackRuns.Clone` (`PushBackRuns.cs:343-344`); la copia a los lados está en
  `PushBackCompositeStructure.cs:856-858`.
- Norma de arquitectura: el detalle de cota «forma parte del diseño» (`docs/ARCHITECTURE.md:139-141`) y
  «detalle, escala y estilo son parte del diseño versionado» (`:171-172`).

### 4.2 Supresiones existentes (no son política de usuario)

- BOM Selectivo: `SelectiveBomBuilder.BuildForCounting` fuerza `None` (`SelectiveBomBuilder.cs:60-68`), y
  el conteo frontal también (`:508`).
- Lateral de un Push Back compuesto: `WithoutDecorations` fuerza `None` sobre la estructura desnuda
  (`PushBackSystemLateralBuilder.cs:234-242`) y `SideDecorations` re-emite las cotas por lado (`:196-231`).

### 4.3 Acoplamiento entre cotas y etiquetas

- Selectivo: el número de frente baja por debajo de las cotas con `SelectiveDimensions.FrontalBottomReach`
  (`SelectiveFrontalBuilder.cs:320-325`; `SelectiveDimensions.cs:32-41`). Es el único acoplamiento del
  Selectivo.
- Dinámico y Push Back: `BottomReach` y `FrontalLeftReach` (`DynamicViewDecorations.cs:320-340`) sitúan
  números y nombre en la frontal (`:55-56`) y en el lateral (`:232`); en planta lo hace `leftReach` (`:133`).
- Consecuencia: apagar las cotas de una vista **mueve** sus etiquetas a la posición que hoy tienen con
  `None`.

## 5. Call sites

### 5.1 Emisores (Application, puros): ocho

| # | Sitio | Tipo.método | Vista | Parámetros de vista |
|---|---|---|---|---|
| S1 | `SelectiveFrontalBuilder.cs:169` | `Build` → `SelectiveDimensions.AddFrontal` | FRONTAL | — |
| S2 | `SelectiveFrontalBuilder.cs:324` | `AddAnnotations` → `FrontalBottomReach` | FRONTAL (etiquetas) | — |
| S3 | `SelectiveLateralBuilder.cs:229` | `Cortes` → `AddLateralCorte` | LATERAL, uno por corte | corte |
| S4 | `SelectivePlantaBuilder.cs:203` | `BuildPlan` → `AddPlanta` | PLANTA | — |
| D1 | `DynamicSystemFrontalBuilder.cs:171` | `AppendFrontal` | FRONTAL | `end`, elevaciones |
| D2 | `DynamicSystemPlantaBuilder.cs:74` | `AppendPlanta` | PLANTA | — |
| D3 | `DynamicSystemLateralBuilder.cs:257` | `AppendLateral` | LATERAL | `postIndex` |
| D4 | `PushBackSystemLateralBuilder.cs:213` | `SideDecorations` → `AppendLateral`, una vez por lado | LATERAL compuesto | `postIndex`; B reflejado en `:225-227` |

Lecturas del nivel dentro de los emisores: `SelectiveDimensions.cs:34-40, :55, :135, :201` y
`DynamicViewDecorations.cs:110-133, :199-222, :267-340`.

### 5.2 Rutas de Push Back hacia D1–D3

- **Frontal de un sentido**: `PushBackSystemFrontalBuilder.cs:198-201` (EntradaSalida → `DynamicRackEnd.Exit`)
  y `:223-226` (Posterior → `DynamicRackEnd.Entrance`).
- **Frontal compuesto**: `PushBackCompositeFrontal.Build` arma cada corte en tres pasadas. «El marco
  —postes, placas, cotas— lo aporta la primera» (`PushBackCompositeFrontal.cs:46-48`), que es la pasada
  EntradaSalida (`:66-71`); las otras dos aportan solo largueros y topes (`:99-103`, `IsPiece` en
  `:141-144`). Los **cuatro** cortes frontales toman sus cotas de la misma clase de emisión.
- **Planta**: en compuesto, `PushBackSystemPlantaBuilder.cs:69` → `PushBackCompositePlanta.Build`; en un
  sentido, D2.
- **Lateral**: en un sentido, `PushBackSystemLateralBuilder.cs:311-312` (D3); en compuesto, `:159-162`
  sobre la estructura desnuda más D4.
- `PushBackPlanComposer.cs:134-138` nunca descarta cotas al deduplicar la proyección.

### 5.3 Materialización (Plugin): un solo camino

`LateralHeaderDrawer.AppendInstance` (`LateralHeaderDrawer.cs:284-291`) → `AppendDimension` (`:338-376`):
añade una `RotatedDimension` a la **definición** que se construye o redefine, en la capa `RACKCAD_COTAS`
(`:327`, `:356`), con el estilo que decide `ResolveDimStyle`; con estilo automático aplica overrides
proporcionales a `TextHeight` (`:362-373`). `HeaderInstanceGrouper.cs:35-43` deja las cotas sueltas, nunca
en ARRAY. Al redefinir, los bloques anónimos `*D` se registran para purga (`:107-111`).

### 5.4 Entradas de dibujo

**Selectivo** (`src/RackCad.Plugin/RackSelectivoCommands.cs`)

- Nuevo: `RACKSELECTIVO`/`RS` (`:22`, `:25`) y `RACKCAD` → `DrawSelectiveView` (`:406`) →
  `DrawSelectiveViewFromAuthored` (`:426`) → `InsertSelectiveFrontal` (`:461-500`).
- `RACKEDITAR` (`RackMenuCommands.cs:115`; `PickRackBlock` en `:127`; despacho por kind en `:141`) →
  `EditSelective` (`:47`):
  - reconcilia el diseño y lo serializa **una sola vez** (`:144-153`);
  - Actualizar: frontales `:166-184`, laterales `:188-210`, plantas `:213-223`, un `Regen` en `:243`;
  - Insertar: tras refrescar, `DrawSelectiveViewFromAuthored(window.InsertView, …, embed)` (`:258`) →
    planta `:446`, frontal `:496` o lateral `:570`.
- Propagación de variables de proyecto: `ProjectVariableMutationExecutor` (PREPARE, MUTATE, POST; `Compose`
  en `:207-208`).

**Dinámico** (`src/RackCad.Plugin/RackDinamicoCommands.cs`)

- Nuevo: `RSD`/`RACKSISTEMADINAMICO` (`:24`, `:30`) → `DrawDynamicView` (`:83`), con `DynamicEnd(section)`
  (`:120`, `:392`).
- `EditDynamic` (`:141`) recorre todas las definiciones del GUID (`:206-266`): planta `:211`, frontal
  `:222-224`, lateral `:237-247`. Un `Regen` (`:273-276`) y vista nueva con el sobre elegido (`:280-281`).
  Corte lateral nuevo: `InsertDynamicLateralSection` (`:294`).

**Push Back** (`src/RackCad.Plugin/RackPushBackCommands.cs`)

- Nuevo: `RPB`/`RACKPUSHBACK` (`:30`, `:34`) → `DrawPushBackView` (`:73`), con `DecodeSection` (`:115`).
- `EditPushBack` (`:139`): `FindRackBlocks` (`:184`), planta `:246`, frontal `:255-260`, lateral
  `:284-293`; vista nueva con `source: embed` (`:330`).

**No ejecutan emisores sobre el rack dibujado**

- Vistas previas WPF: la del Selectivo no pinta el rol; la de Push Back lo excluye expresamente
  (`PushBackPreviewModel.cs:114-121`).
- BOM: §4.2. `SelectiveBomBuilder.cs:219` recorre `Cortes` sin forzar `None`, así que calcula las cotas
  laterales y las descarta (H6).
- `RACKDUPLICAR` (`RackDuplicarCommands.cs:28`) clona la definición elegida (`:190-197`), cotas incluidas
  como entidades.
- `RACKLAYOUT` y `RACKRELLENAR` usan referencias o clones de la planta semilla; la huella sale de
  `reference.GeometricExtents` (`RackLayoutCommands.cs:189-197`), que incluye las cotas.

### 5.5 Sitios de copia del ajuste

| Sistema | Sitio | Dirección |
|---|---|---|
| Selectivo | `RackSelectiveWindow.xaml.cs:2338-2339` y `:2686-2687` | UI → diseño; diseño → UI |
| Selectivo | `SelectiveDesignInputs.cs:43-44` → `SelectiveEditorState.cs:1317-1318` | entradas → diseño |
| Selectivo | `SelectiveGeometryResolver.cs:66-67` | diseño → sistema |
| Selectivo | `SelectiveDepthLayout.cs:83-84` | sistema → vista por fondo |
| Selectivo | `SelectivePalletDesignDocument.cs:268-269` y `:401-402` | diseño ⇄ DTO |
| Dinámico | `RackDynamicSystemWindow.xaml.cs:292-298` y `:2772-2775` | UI ⇄ opciones y diseño |
| Dinámico | `DynamicAnnotationOptions.cs:16-17` → `DynamicEditorDesignAssembler.cs:174-175` | opciones → diseño |
| Dinámico | `DynamicRackSystemResolver.cs:244-245` y `:357-358` | diseño ⇄ sistema |
| Dinámico | `DynamicRackSystemDocument.cs:112-113, :161-162, :215-216, :309-310` | DTO ⇄ sistema y diseño |
| Push Back | `RackPushBackSystemWindow.xaml.cs:378` y `:810-817` | UI ⇄ opciones |
| Push Back | `PushBackEditorState.Load.cs:206-208` | diseño → opciones |
| Push Back | `PushBackEditorDesignAssembler.cs:378` | solo reenvía las opciones |
| Push Back | `PushBackMirror.cs:158-160` | clon de la estructura |
| Push Back | `PushBackCompositeStructure.cs:856-858` | estructura compartida → lados |

## 6. Persistencia

### 6.1 El sobre y dónde vive

- `RackEmbedDocument` (`src/RackCad.Application/Persistence/RackEmbedDocument.cs:14-65`): `SchemaVersion`
  1.0, `Kind`, `View` (`frontal`, `lateral`, `planta`: `:28-30`), `Section` (-1 = no seccionada, `:47`),
  `Id`, `Name`, `Design` (JSON del diseño) y `[JsonExtensionData]` (`:63-64`).
- Se guarda como Xrecord `RACKCAD_SELECTIVE` (`RackBlockData.cs:16`) en el diccionario de extensión de la
  **definición**: «Payloads go on the block DEFINITION so every reference/copy shares them»
  (`SystemBlockWriter.cs:13`), con escrituras en `:33`, `:114` y `LateralHeaderDrawService.cs:258`. La
  lectura parte de la definición de la referencia elegida (`RackCommandSupport.cs:89-94`). El comentario
  de `RackBlockData.cs:8` («block reference's») está desfasado (H7).
- `RackEmbedComposer.Compose` (`RackEmbedComposer.cs:21-35`) crea un sobre nuevo y del origen **solo**
  hereda `SchemaVersion` y `ExtensionData`.
- Lectura tolerante: un sobre de major más nuevo se lee como `null` y nunca lanza
  (`RackEmbedDocument.cs:108-115`).

### 6.2 Diseño por sistema

| Sistema | `Design` del sobre | Campo de cotas | Fallback legacy | `[JsonExtensionData]` | `SchemaVersion` |
|---|---|---|---|---|---|
| Selectivo | `SelectivePalletDesignDocument` | `int? Dimensions` (`:106`), `DimensionStyle` (`:109`) | nulo o fuera de rango → `None` (`:416-422`); estilo vacío → automático (`:402`) | Sí (`:141-142`) | 1.0 legado / 2.0 promovido por vínculos (`:24-47`) |
| Dinámico | `RackProjectDocument` → `DynamicSystem` | `DynamicRackSystemDocument.cs:62-63` | `ValidDimensions` → `None` (`:415-418`) | solo en el envoltorio (`RackProjectDocument.cs:51-52`), **no** en `DynamicRackSystemDocument` | sin versión propia |
| Push Back | `RackProjectDocument` → `PushBackDesignDocument` | en `Structure` = `DynamicRackSystemDocument` (`PushBackDesignDocument.cs:27`, `:105`) | igual que Dinámico | en `PushBackDesignDocument` (`:75-76`), **no** en `Structure` (I-11: no es recursiva) | 1.0 |

- Serialización del `Design`: Selectivo con `SelectivePalletDesignStore` (`RackSelectivoCommands.cs:153`);
  Dinámico y Push Back con `RackProjectStore` (`RackDinamicoCommands.cs:379`, `RackPushBackCommands.cs:414`).
- Pruebas que fijan el fallback de hoy: `SelectiveDimensionsTests.cs:194-214` y
  `DynamicNullOverrideGoldenTests.cs:46, :81-87` (SHA de lateral, cortes y ambas frontales con `Detailed`).
- Precedente de campo aditivo que se omite cuando es nulo: `DynamicRackSystemDocument.FirstLevelDatum` con
  `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` (`DynamicRackSystemDocument.cs:30-31`).

### 6.3 Biblioteca y duplicación

- Biblioteca: `RackProjectStore` guarda el diseño, nunca las entidades dibujadas.
- `RACKDUPLICAR`: `RackEnvelopeRestamp.RestampEnvelope` (`RackEnvelopeRestamp.cs:32`) cambia `Id`, `Name` y
  `Design`, y conserva `View`, `Section` y `ExtensionData` del sobre. `RestampDesign` por kind:
  - Selectivo: `SelectiveAuthoredRestamp.Restamp` (`RestampResult.cs:57-59`) hace ida y vuelta por el
    documento, así que los campos **declarados** sobreviven.
  - Dinámico, Push Back y Cama devuelven el JSON intacto (`DynamicKindHandler.cs:49`,
    `PushBackKindHandler.cs:75`, `CamaKindHandler.cs:52`).

### 6.4 Builds anteriores

Un build previo que re-guarde un rack pierde cualquier campo nuevo que no declare: siempre en Dinámico y
Push Back (`DynamicRackSystemDocument` no tiene `ExtensionData`), y en Selectivo solo con builds anteriores
a I-47. Si ese campo es una política de visibilidad, el rack vuelve a legacy y las cotas **reaparecen**:
nunca desaparecen.

## 7. Identidad de vista

### 7.1 Modelo normativo

[ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md) y
[ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md), ambos aceptados: una representación
es una **definición de bloque** con su sobre; `View` y `Section` describen qué representación contiene;
una `BlockReference` solo coloca la definición y «puede compartirla con otras copias»; un `ObjectId` «no
sustituye al GUID»; en multivista «varias definiciones comparten el GUID del rack y se reconstruyen desde el
mismo diseño», y «las referencias que comparten una definición reflejan su redefinición».

### 7.2 `View` y `Section` por sistema

| Sistema | `frontal` | `lateral` | `planta` |
|---|---|---|---|
| Selectivo | `Section` = fondo; un bloque por fondo; legacy -1 → fondo 0 (`RackSelectivoCommands.cs:155, :168, :456-460, :496`) | `Section` = `PostIndex` del corte sobre la retícula del fondo maestro; un corte cubre todos los fondos que alcanzan ese poste (`:570`) | -1; un bloque con todos los fondos (`:216`) |
| Dinámico | `Section` = `(int)DynamicRackEnd`: 0 salida, 1 entrada (`RackDinamicoCommands.cs:222-224`, `:392`) | `Section` = poste; legacy < 0 → poste 0 (`:236-239`); nunca se persiste un lateral entero | -1 (`:211`) |
| Push Back | `Section` = `(int)PushBackFrontalEnd + (B ? 2 : 0)`: 0 EntradaSalida A, 1 Posterior A, 2 EntradaSalida B, 3 Posterior B; fuera de rango → EntradaSalida A (`PushBackSystemFrontalBuilder.cs:138-152`) | `Section` = poste (`RackPushBackCommands.cs:284-293`); A y B en el mismo bloque, el sobre no lleva lado | -1; A y B en el mismo bloque |

### 7.3 Duplicados y copias

- Cada inserción crea una definición nueva con nombre único (`LateralHeaderDrawer.cs:242`, `UniqueBlockName`
  en `:395`), y ninguna ruta de inserción comprueba si ya existe esa terna `(Id, View, Section)`: dos
  bloques con la misma terna son legales y se redibujan idénticos.
- Una copia de AutoCAD (`COPY`) comparte la definición y el sobre (ADR-0010).
- Las copias independientes de `RACKDUPLICAR` y `RACKLAYOUT` conservan `View` y `Section` con GUID nuevo.

### 7.4 Autoridad multivista

`SelectiveAuthoredAuthority` (`src/RackCad.Application/ProjectVariables/SelectiveAuthoredAuthority.cs`):

- cada vista «stores the whole design», las hermanas deben coincidir y «Divergence aborts» (`:53-60`,
  `:152-156`);
- la comparación es estructural e *include-by-default*, con `SchemaVersion` y `ExtensionData` dentro
  (`:63-77`);
- lo único propio de la vista son `View` y `Section`, que no viven en el documento (`:79-81`);
- los arrays se comparan **en orden** y las cadenas con comparación **Ordinal** (`:162-226`).

La consumen `RACKBOMTOTAL`, la propagación de variables de proyecto y `RACKVARIABLES`.

## 8. Flujo round-trip

1. **Crear.** El editor arma el diseño; el Plugin lo serializa, compone el sobre sin origen
   (`RackEmbedComposer.Compose(null, …)`), crea la definición con su Xrecord y la coloca con el jig; si se
   cancela, la definición se borra. Un Selectivo nuevo solo inserta la frontal
   (`RackSelectiveWindow.xaml.cs:2081-2092`); un Dinámico nuevo, un lateral
   (`RackDynamicSystemWindow.xaml.cs:2877`).
2. **Insertar otras vistas.** Solo desde un rack que ya existe (ADR-0010), vía `RACKEDITAR`.
3. **`RACKEDITAR`.** `PickRackBlock` lee el sobre de la definición de la referencia elegida y despacha por
   kind. En el Selectivo, el diseño de esa vista es el portador *authored* y su diseño efectivo se carga en
   la ventana.
4. **Actualizar.** `FindRackBlocks` (`RackCommandSupport.cs:109-134`) reúne todas las definiciones del GUID.
   En el Selectivo, `LinkedPropertyReconciler.Reconcile` → `WithDesign` → `From(design)` y una única
   serialización para todas las vistas (`RackSelectivoCommands.cs:144-153`). Cada vista se redibuja con su
   propio `View`/`Section` y se compone desde **su propio** sobre; un solo `Regen`.
5. **Guardar y reabrir.** El Xrecord viaja con el DWG. Al reabrir, `RackEmbedStore.Deserialize` es tolerante.
6. **Vista enlazada nueva.** Primero se refrescan las existentes. La nueva definición lleva el mismo JSON de
   diseño, pero su sobre se compone desde el de la vista **elegida** (`RackSelectivoCommands.cs:257-258`,
   `RackDinamicoCommands.cs:280-281`, `RackPushBackCommands.cs:330`).

**Consecuencia**: toda vista se regenera desde un único diseño, así que cualquier decisión por vista debe
poder calcularse con `(diseño, View)` en el momento de dibujar.

## 9. Alternativas de autoridad evaluadas en G1

| | A — flags | B — conjunto de view-kinds | C — objeto de política | D — por instancia |
|---|---|---|---|---|
| Ausencia = conducta de hoy | por campo; estados parciales ambiguos | un solo nulo | una regla | sin política para vistas nuevas |
| Compatible con ADR-0009/0010 | Sí | Sí | Sí | **No**: la cota vive dentro de la definición y `COPY` la comparte |
| Las hermanas siguen iguales (§7.4) | Sí | Sí | Sí | No: divergen, o el dato vive fuera del diseño |
| La vista enlazada nueva recibe | la del rack | la del rack | la del rack | la de la vista **elegida** (escritura cruzada) |
| Llega a los builders sin tocar el Plugin | Sí | Sí | Sí | No: los builders nunca ven el sobre y `Compose` descarta campos nuevos (§6.1) |
| Coste | tres campos × unos veinte sitios de copia | un campo | un campo más una regla | identidad nueva de instancia y ADR que reemplace 0009/0010 |

**Conclusión de G1**: autoridad **rack × tipo de vista, dentro del diseño**. D queda descartada con la
evidencia de §6.1, §7 y §8. G1 sugirió como forma una regla única (C) sobre una lista de tokens del sobre
(B). Por encargo del Coordinador, la representación concreta se decide en la Proposal V1, que compara esa
lista contra un `[Flags]` con la evidencia adicional de §7.4 (orden de los arrays y comparación Ordinal).

## 10. Contrato legacy propuesto en G1

Adoptado por el Coordinador (`CD-06`, `CD-07`):

- **L1**: sin el campo nuevo, toda vista usa el `Dimensions` del rack; mismas instancias, mismos
  desplazamientos y mismas etiquetas que hoy.
- **L2**: una política legacy **no se escribe**, como `FirstLevelDatum` (§6.2); el JSON queda byte-idéntico
  y ninguna `SchemaVersion` cambia.
- **L3**: abrir un rack legacy muestra todas las vistas activas, y guardarlo sin tocar la visibilidad
  conserva la ausencia del campo.
- **L4**: `Dimensions = None` ⇒ ninguna cota en ninguna vista.
- **L5**: sin cambios en geometría, BOM, GUID, `View`/`Section`, sobre ni `RACKDUPLICAR`.
- **L6**: un build anterior que re-guarde pierde el campo y el rack vuelve a legacy: las cotas reaparecen.
  Limitación declarada; no se sube el major.

## 11. Archivos productivos previstos y solapamientos

### 11.1 Archivos previstos (de la conclusión de G1; los fija la Proposal)

- **Domain**: `SelectivePalletDesign.cs` (archivo caliente, WORKFLOW §7), `SelectiveRackSystem.cs`,
  `DynamicRackDesign.cs`, `DynamicRackSystem.cs`, más un tipo nuevo.
- **Application**: `SelectiveDimensions.cs`, `DynamicViewDecorations.cs`, `SelectiveGeometryResolver.cs`,
  `SelectiveDepthLayout.cs`, `DynamicRackSystemResolver.cs`, `PushBackMirror.cs`,
  `PushBackCompositeStructure.cs`, `PushBackEditorState.Load.cs`, `DynamicAnnotationOptions.cs`,
  `DynamicEditorDesignAssembler.cs`, `SelectiveDesignInputs.cs`, `SelectiveEditorState.cs`,
  `SelectivePalletDesignDocument.cs` (solo `From` y `ToDomain`), `DynamicRackSystemDocument.cs`, más la
  regla nueva.
- **UI**: `RackSelectiveWindow.xaml(.cs)`, `RackDynamicSystemWindow.xaml(.cs)` y
  `RackPushBackSystemWindow.xaml(.cs)`: los tres editores calientes.
- **Plugin**: ninguno.
- **No necesarios**: `LinkedPropertyEditor`, `ProjectVariables/**` (el reconciliador, la autoridad y el
  resolvedor efectivo transportan un campo declarado sin cambios), código de expresiones (no existe ningún
  `Expression*`), el sobre, `RACKDUPLICAR`, catálogos y los sistemas sin cotas.

### 11.2 I-49 (estado observado al cerrar G1)

- **Reclamada y bootstrapeada**: `origin/architecture/motor-expresiones-parametricas`, reclamo `77262fe`,
  bootstrap `f2d28a2`. Su contrato declara `conflicts_with: []` y se ejecuta bajo
  `OWNER_OVERRIDE_I49_I50_PARALLEL`, registrado en `docs/automation/decisions/I-49.md` de **su** rama.
- Su condición vinculante para I-50: `LinkedPropertyEditor`, Project Variables y el Expression Engine son
  alcance funcional de I-49, e I-50 no puede refactorizarlos salvo necesidad demostrada. La integración de
  ambas es serializada.
- **Archivo compartido material para I-50**: `RackSelectiveWindow.xaml` —las cotas (`:36-64`) comparten
  `StackPanel` con `ToleranceEditor` (`:134`) y `ClearanceEditor` (`:142`)— y `RackSelectiveWindow.xaml.cs`
  —`BuildDesign` (`:2302`) lee las cotas en `:2338` junto a `TryEffective` (`:2307`, `:2312`), y
  `LoadDesign` (`:2572`) carga las cotas en `:2686` después de `AttachLinkedEditors` (`:2588`)—.
- **Posible**: `SelectivePalletDesignDocument.cs`, si I-49 cambia `PropertyValues` (`:128-129`) o
  `WithDesign` (`:316-330`); I-50 solo tocaría `From` (`:268`) y `ToDomain` (`:401`).
- I-49 nombra además `SelectivePalletDesign.cs` y `src/RackCad.Plugin/*Commands*.cs` como posibles cruces;
  I-50 prevé tocar el primero y ninguno de los segundos.

### 11.3 I-51 (estado observado al cerrar G1)

- `origin/feature/rackduplicar-multiples-origenes` con Discovery publicado (`c6fbfbc`) y
  `conflicts_with: []`.
- Prevé `RackDuplicarCommands.cs` y `RackEnvelopeRestamp.cs`: **ninguno** está entre los archivos de I-50.
- Su decisión AM-4 depende de dónde vive la política de I-50; §9 la responde: en el **diseño**, nunca en la
  referencia ni en el sobre.
- Cruce documental: su fila de ROADMAP se inserta en el mismo punto que la de I-50 (y la de I-49).

## 12. Riesgos identificados en G1

1. Archivos calientes: los tres editores grandes y `SelectivePalletDesign.cs`; `RackSelectiveWindow`
   comparte métodos con la superficie de I-48/I-49.
2. Acoplamiento de etiquetas (§4.3): una vista apagada debe situar sus etiquetas como con `None`; es un
   cambio visible y debe entrar en la validación del Owner.
3. Push Back compuesto: la política debe llegar a la estructura de cada lado a través de
   `PushBackCompositeStructure` y `PushBackMirror`; omitir un sitio de copia deja mal el lado B sin avisar.
4. Builds anteriores que re-guardan (L6).
5. Huella de `RACKLAYOUT` (§5.4): apagar las cotas de la planta encoge las rejillas futuras.
6. Sin control por copia: una copia `COPY` no puede tener cotas distintas de su origen (ADR-0010); el texto
   de la UI debe decir «por tipo de vista».
7. Censos de UI que fijan nombres de controles (`DynamicShellMigrationTests`, `SelectiveShellMigrationTests`,
   `PushBackModuleEditorCharacterizationTests`, `RichEditorContractTests`): se actualizan, no se relajan.

## 13. Hallazgos fuera de alcance

Registrados **sin corregir** en [ideas-futuras.md](../ideas-futuras.md), sección «I-50»:

- **H1**: Push Back no tiene control de estilo de cota y cada recálculo escribe `DimensionStyle = null`.
- **H2**: el lateral de Push Back envía la posición en la lista de cortes y el Plugin la lee como índice de
  poste.
- **H3**: los bloques `*D` de las cotas no se encolan para purga al cancelar una inserción ni en
  `EraseViewBlocks`.
- **H4**: `HeaderRunPlan.PlacedClone` no copia los campos de texto y cota.
- **H5**: el Dinámico descarta un estilo guardado que el DWG no tiene; el Selectivo lo conserva.
- **H6**: el BOM del Selectivo calcula las cotas laterales y las descarta.
- **H7**: tres comentarios desfasados.

## 14. Cierre de G1

G1 se cierra con las decisiones `CD-01`..`CD-11` del Coordinador, versionadas en el contrato
(sección 12). El siguiente gate es **G2A**: Proposal V1 y ADR `propuesto`, solo documentación.
