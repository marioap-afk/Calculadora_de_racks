# I-54 — G1 Discovery: Custom Properties Foundation (ID24)

> **G1 = Discovery, solo documentacion.** Este informe **no autoriza implementar** y no fija decisiones:
> las decisiones se proponen en [I-54-proposal-v1.md](I-54-proposal-v1.md), que tampoco es consenso.
> Contrato: [I-54-propiedades-personalizadas.md](I-54-propiedades-personalizadas.md).

```text
Initiative   = I-54 — ID24 — Custom Properties Foundation
Branch       = architecture/propiedades-personalizadas
BASE_SHA     = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, merge de I-51)
CLAIM_SHA    = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
BOOTSTRAP    = f908b2f4ba508bf365e55adbda78cd4eac505295
Codigo       = src/, tests/ y assets/ de la rama identicos a BASE_SHA (git diff --stat vacio)
Lineas       = todas las citas `ruta:linea` son relativas a BASE_SHA
```

## 0. Metodo y clases de evidencia

Cada afirmacion lleva una marca de procedencia, porque no todas valen lo mismo para quien revise:

| Marca | Significado |
|---|---|
| **[E]** | El ejecutor **leyo en esta sesion** las lineas citadas del arbol en `BASE_SHA`. |
| **[L]** | Lectura de auditoria con cita exacta, **no re-verificada linea a linea** por el ejecutor. Se recomienda muestrear. |
| **[I]** | Inferencia sobre comportamiento no ejecutado (API de AutoCAD o System.Text.Json). Se declara como tal. |

G1 **no** compilo ni ejecuto pruebas: es documental. Ninguna afirmacion de este informe descansa en una
corrida.

## 1. Resumen: los hallazgos que cambian el planteamiento

1. **No existe hoy ningun metadato descriptivo de proyecto** (cliente, obra, ubicacion, revision, autor,
   fecha, empresa, codigo) **ni ningun mapa clave→valor editable por el usuario**. El unico dato
   descriptivo autorado a nivel rack es el `Name` del rack. ID24 parte de cero. [L, §3]
2. **`RACKCAD_PROJECT` es un `Xrecord` DIRECTO bajo el NOD**, no un sub-`DBDictionary`. Migrarlo a
   sub-diccionario haria que **todo build existente** lo lea como `PresentButUnreadable` y bloquee
   `RACKVARIABLES`, `RACKEDITAR` del Selectivo y `RACKBOMTOTAL`. [E, §4]
3. **`RackEmbedComposer.Compose` reconstruye el sobre** y del sobre de origen solo hereda
   `SchemaVersion` y `ExtensionData`. Un campo **no declarado** sobrevive a redibujos, vistas nuevas y
   propagaciones; un campo **declarado nuevo** se perderia en todos esos caminos si `Compose` no cambia.
   [E, §5.3]
4. **Ninguna autoridad compara campos del SOBRE entre vistas hermanas.** La autoridad estructural existe
   solo para el diseño interior del Selectivo y excluye por diseño lo que vive en el sobre. [E, §6]
5. **`RestampEnvelope` preserva el sobre completo** (muta `Id`, `Name` y `Design` sobre el mismo objeto),
   pero **ninguna prueba lo demuestra**: el Plugin no lo carga ninguna suite. Ademas `RackCloner` crea un
   BTR **nuevo** y solo reescribe el payload RackCad: lo que viva fuera del sobre, en el diccionario de
   extension del BTR, **no llega a la copia**. [E, §8]
6. **Ningun sistema exporta campos del sobre a la biblioteca** `.rackcad.json`, y la importacion siempre
   acuña GUID nuevo y compone un sobre limpio. [E/L, §9]
7. **Un dato de proyecto en el NOD no viaja entre dibujos; un dato en el sobre si** (viaja con la
   definicion). OV-9 de I-47 lo demostro para vinculos rotos legitimos. [L/I, §4.8, §9.4]
8. **Guardas vigentes prohiben substrings de codigo** que un diseño ingenuo usaria: `PropertyValues`
   (Domain, Plugin, UI), `rackProperty` y `RackPropertyReference` (Application, Plugin, UI). [E, §11.6]
9. **I-49 reserva `Rack` y `Project`** —como nombre suelto y como namespace `Rack.X`/`Project.X`— para
   **ID20** (parametros calculados), no para ID24. [L, §2.3]
10. **Cruce productivo actual con I-50 = cero archivos** mientras I-54 no toque DTOs ni Domain de
    sistema. I-52 e I-53 estan activas y solo tienen documentacion. [E, §2]

## 2. Preflight y ramas activas

### 2.1 Estado remoto medido

Ultimo `git fetch --all --prune` antes de redactar este informe. `origin/main` **no se movio** desde el
reclamo. [E]

| Iniciativa | Rama | Tip | merge-base | detras/delante de main | Contenido frente a su base |
|---|---|---|---|---|---|
| I-49 | `architecture/motor-expresiones-parametricas` | `4df9480` | `a4d88f1` | 9 / 6 | 7 archivos, **solo `docs/`** |
| I-50 | `feature/cotas-independientes-por-vista` | `a4806ff` | `a4d88f1` | 9 / 15 | 19 de `src/`, 17 de `tests/`, 10 de `docs/` |
| I-52 | `feature/rackmirror-espejo-semantico` | `7ee7975` | `46fcac2` | 0 / 2 | reclamo + bootstrap, **solo `docs/`** |
| I-53 | `feature/cabeceras-configurables-multidestino` | `405cfc0` | `46fcac2` | 0 / 2 | reclamo + bootstrap, **solo `docs/`** |
| I-54 | `architecture/propiedades-personalizadas` | `f908b2f` | `46fcac2` | 0 / 2 | reclamo + bootstrap, **solo `docs/`** |

I-52 aparecio **durante** el preflight del reclamo de I-54 (su reclamo `5528176`, luego bootstrap `7ee7975`),
e I-53 despues; ninguna reclama I-54 ni ID24. [E]

### 2.2 I-50 — cruce medido

- Archivos de produccion que toca (diff `a4d88f1...a4806ff`): `SelectivePalletDesignDocument.cs`,
  `DynamicRackSystemDocument.cs`, cuatro tipos de Domain (`SelectivePalletDesign`, `SelectiveRackSystem`,
  `DynamicRackDesign`, `DynamicRackSystem`), resolvers y estado de editor de Selectivo/Dinamico/Push Back,
  y `DimensionViewPolicy`/`DimensionViewVisibility` nuevos. [E]
- **No toca** `RackEmbedDocument.cs`, `RackEmbedComposer.cs`, `RackEnvelopeRestamp.cs`, `RackBlockData.cs`,
  `RackCloner.cs`, `RestampResult.cs`, `SelectiveAuthoredAuthority.cs`, `RackProjectStore`/`RackProjectDocument`,
  `RackDesignLibrary.cs` ni nada de `ProjectVariables`. [E por `--name-status`; L por diff]
- Sus pruebas nuevas **usan** `RackEmbedStore`, `SelectiveAuthoredRestamp`, `RackProjectStore`
  (`DimensionViewsRestampTests.cs`) y `SelectiveAuthoredAuthority` (`SelectiveDimensionViewsAuthorityTests.cs`).
  Un cambio **aditivo** en el sobre no las rompe; un cambio de contrato de esas APIs si. [L]
- Su ADR-0035 (aceptado en su rama) rechazo metadata por instancia de vista, entre otras razones porque
  «`RackEmbedComposer` descarta campos nuevos del sobre» (`I50:docs/adr/0035-*.md:97-102`). La frase es
  exacta para campos **declarados**; un campo no declarado sobrevive en `ExtensionData`. [L]
- Su politica vive en el **diseño** (`C50:230-231`, CD-01, CD-08). [L]
- G3 de I-50 (UI de `RackSelectiveWindow`) sigue pendiente: archivo caliente. [L]

### 2.3 I-49 — cruce semantico, no dependencia

- Solo documentacion; Proposal V3 con `Architect = PENDING RE-REVIEW` e implementacion bloqueada
  (`I49:docs/initiatives/I-49-proposal-v3.md:9-15`). [L]
- **Cero menciones** de ID24, propiedades personalizadas o metadatos en toda la rama. [L]
- Reserva `Rack` y `Project` como nombres (sin llaves → `ReservedName`) y como namespace (`Rack.X` →
  `UnknownNamespace`) para **ID20**, «Computed / Built-in Parameters» (`V3:364-366`, `V3:537-542`,
  `V3:1569-1578`; `I49:docs/initiatives/I-49-discovery.md:54-57`). [L]
- Motor de valores `double` finitos; sin cadenas ni booleanos (`V3:318-319`, `V3:543-547`). [L]
- Planea tocar `SelectiveAuthoredAuthority.cs`, `LinkedPropertyReconciler.cs`,
  `SelectiveEffectiveDesignResolver.cs`, `SelectiveLibraryExport.cs` y `SelectivePropertyValueDocument.cs`
  (`V3:2296-2297`). [L]
- El permiso excepcional de paralelismo cubre **solo** el par I-49/I-50 (`I49:docs/automation/decisions/I-49.md:9,82,107`).
  No cubre a I-54. [L]

### 2.4 I-52 e I-53

- **I-52 (RACKMIRROR)**: G1 caracterizara `RackEnvelopeRestamp`, el payload authored, identidad y nombres,
  y declara «reutilizar I-51» (`I52:docs/initiatives/I-52-rackmirror-espejo-semantico.md:76-82,104-107`).
  Se detiene ante cambio de formato persistido o de identidad (`:219-220`). [E] Añadira previsiblemente un
  comando. [I]
- **I-53 (cabeceras multidestino)**: toca autoridad de cabecera por sistema, editores grandes y DTO de
  sistema (archivos calientes), y **excluye expresamente las propiedades personalizadas (I-54)**
  (`I53:docs/initiatives/I-53-cabeceras-configurables-multidestino.md:120-122`); vigila a I-54 (`:143-147`,
  `:207-208`). [E]
- Sus textos de ID del Owner (ID6/ID7) **no estan versionados**; lo mismo ocurre con el texto de **ID24**:
  este Discovery solo conoce ID24 por la instruccion del Coordinador. [E]

### 2.5 Re-medicion antes de publicar G2A (addendum)

Justo antes del commit de la Proposal, tres paralelas habian avanzado. `origin/main` seguia en `46fcac2`. [E]

| Iniciativa | Tip en G1 | Tip al publicar G2A | Que cambio |
|---|---|---|---|
| I-49 | `4df9480` | `4df9480` | nada |
| I-50 | `a4806ff` | `8ceb3a7` | G6: `PushBackCompositeStructure.cs` y dos archivos de pruebas nuevos. **Sigue sin tocar** sobre, compositor, restamp, cloner, autoridad ni biblioteca |
| I-52 | `7ee7975` | `339b3ab` | G1 publicado (`9751849`) y addendum (`339b3ab`), solo `docs/` |
| I-53 | `405cfc0` | `c8476cc` | G1 publicado, solo `docs/` |

Lo que esos Discovery dicen de I-54 [E, `git grep` sobre cada rama]:

- **I-52** clasifica `RackCloner.CloneDefinition` como «**No directa**» para el espejo, porque clona geometria
  sin reflejar y el espejo tiene que **regenerarla** desde el diseño reflejado
  (`I52:docs/initiatives/I-52-discovery.md:127`); reutiliza `RestampEnvelope` **solo para identidad**, sin hook de
  transformacion, y advierte que meter transformacion en `RackEnvelopeRestamp.cs` rompe la guarda G-R5 (`:126`,
  `:156`, `:516`). Registra a I-54 con un cruce de **persistencia**: «una propiedad de alcance Rack viajaria en el
  sobre o en el diseño y el espejo debe conservarla» (`:780`, fila U-18 en `:808`).
- **I-53** califica el cruce con I-54 como **bajo**, «quiza UI en editores» (`I53:docs/initiatives/I-53-discovery.md:688-690`),
  y confirma por su cuenta que `RackEmbedStore` escribe nulos (`:747`, L-4).

Conclusion: ninguna medicion de §2.1-§2.4 cambia; el camino de escritura del espejo de I-52 queda **abierto** y
debe conservar los miembros del sobre (Proposal V1, D-11).

### 2.6 Re-medicion en G2C (addendum)

Antes de publicar la Proposal V2, tras `git fetch --all --prune`: `origin/main` sigue en `46fcac2` y la rama de
I-54 en `7c197af`, sin cambios. Las cuatro paralelas avanzaron desde la publicacion de G2A. [E]

| Iniciativa | Tip en G2A | Tip en G2C | Que cambio |
|---|---|---|---|
| I-49 | `4df9480` | `ccf21c6` | Proposal V4 (`I-49-proposal-v4.md`), **solo `docs/`** |
| I-50 | `8ceb3a7` | `6cd2970` | G3 (A), (B) y (C): `RackSelectiveWindow`, `RackDynamicSystemWindow` y `RackPushBackSystemWindow` (`.xaml` y `.xaml.cs`), mas pruebas de UI. **Sigue sin tocar** sobre, compositor, restamp, cloner, autoridad, biblioteca y censos de comandos o ventanas |
| I-52 | `339b3ab` | `0fc7032` | G2: Proposal V1, `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md` (`propuesto`), `docs/adr/README.md` y `docs/automation/decisions/I-52.md`, **solo `docs/`** |
| I-53 | `c8476cc` | `f38362d` | Proposal V1 y contrato, **solo `docs/`** |

Lo que dicen de I-54 [E, `git show` sobre cada rama]:

- **I-49 V4** sigue reservando `Rack` y `Project`, sus namespaces y el ambito `Rack` para ID20
  (`I49:docs/initiatives/I-49-proposal-v4.md:628-633,1737-1745`). Mide su presupuesto de profundidad JSON con el
  diseño «como cadena dentro del sobre» (`:470-484`), y no cambia los censos de comandos ni de ventanas (`:1447`).
- **I-50**: la prueba `DimensionViewsRestampTests.cs`, en su rama desde `94220fb`, construye `new RackEmbedDocument` en
  `tests/`. En `src/` el unico sigue siendo `RackEmbedComposer.cs:24`, y hay siete llamadas a
  `RackEmbedComposer.Compose(`, tambien en `6cd2970`.
- **I-52 V1**: el espejo compone con `RackEmbedComposer.Compose(sobreFuente, …)` sobre el diseño reflejado y
  despues re-estampa con `RestampEnvelope`, sin modificar `RackEnvelopeRestamp.cs`
  (`I52:docs/initiatives/I-52-proposal-v1.md:316-317,698-702`). Declara que las propiedades de rack de I-54 solo
  sobreviven si `Compose` hereda ese miembro (`:537`, `:735-737`), y preve `RACKMIRROR` sin alias, con el censo de
  comandos en 34 (`:76`).
- **I-53 V1** no añade datos persistidos y declara «sin cruce» con I-54
  (`I53:docs/initiatives/I-53-proposal-v1.md:649-650`).

Conclusion: ninguna medicion de §2.1-§2.5 cambia.

### 2.7 Re-medicion en G2E (addendum)

Dos mediciones con `git fetch --all --prune`: en el preflight de G2E y antes de publicar la Proposal V3. La rama de
I-54 seguia en `36c337b` en las dos. [E]

| Referencia | Tip en G2C | Tip al cerrar G2D | Preflight de G2E | Al publicar V3 | Que cambio |
|---|---|---|---|---|---|
| `origin/main` | `46fcac2` | `46fcac2` | `46fcac2` | `f8deb67` | **Integracion de I-50** (`c9a5f0a`, cierre documental, y merge `f8deb67`), publicada durante la sesion; la rama remota de I-50 se retiro |
| I-49 | `ccf21c6` | `2783acc` | `2783acc` | `048a508` | Proposal V5 (`2783acc`) y V6 (`048a508`), **solo `docs/`** |
| I-50 | `6cd2970` | `6cd2970` | `c9a5f0a` | integrada | Cierre documental (`HANDOFF.md`, `ROADMAP.md`, decisiones, `validacion-manual-autocad.md` y contrato) e integracion |
| I-52 | `0fc7032` | `0fc7032` | `0fc7032` | `0445718` | Proposal V2, ADR-0036 corregido (`propuesto`) y decisiones, **solo `docs/`** |
| I-53 | `f38362d` | `f38362d` | `d7f17ad` | `d7f17ad` | Proposal V2, ADR-0037 (`propuesto`), `adr/README.md`, decisiones y contrato, **solo `docs/`** |

Lo que eso cambia para I-54 [E, `git diff`, `git grep` y `git show` sobre cada referencia]:

- **`main` @ `f8deb67`**:
  - entre `46fcac2` y `f8deb67` cambian 52 archivos de `src/`, `tests/` y `assets/`, ninguno de codigo del mapa de
    I-54 ni de los que cita la Proposal, salvo `DimensionViewsRestampTests.cs`, la prueba de I-50 que ya se citaba
    desde su rama. De los documentos que I-54 tocara al implementar cambian `validacion-manual-autocad.md` e
    `ideas-futuras.md`, sin conflicto;
  - en `src/` sigue habiendo un solo `new RackEmbedDocument` (`RackEmbedComposer.cs:24`), siete llamadas a
    `RackEmbedComposer.Compose(` y 33 `[CommandMethod(`;
  - ADR-0035 queda `aceptado` y su frase sobre `RackEmbedComposer` sigue en `:97-102`;
  - la cita `HANDOFF.md:1133-1136` de la Proposal es `:1199-1202` en `main`, con el mismo texto;
  - `git merge-tree` entre la rama de I-54 y `origin/main` no da conflictos.
- **Sin rebase en G2E.** WORKFLOW §4.2 exige rebasar al abrir una sesion si el trunk avanzo; al abrir G2E no
  habia avanzado. El rebase queda pendiente para la proxima sesion que escriba en la rama (Proposal V3 §2).
- **I-49 V6** sigue reservando `Rack` y `Project` como nombres reservados, y la sintaxis de namespace, para ID20
  (`I49:docs/initiatives/I-49-proposal-v6.md:622-624,2930`); no cambia los censos de 33 comandos y 29 ventanas
  (`:1914,2082,2311`), y su fila de I-54 mantiene el punto de extension solo como restriccion (`:3157`).
- **I-52 V2** mantiene el orden reflejar → `Compose(sobreFuente, …)` → `RestampEnvelope` sin modificar
  `RackEnvelopeRestamp.cs` (`I52:docs/initiatives/I-52-proposal-v2.md:458-459`), `RACKMIRROR` sin alias con el
  censo en 34 (`:96`) y la prueba cruzada T-M20 (`:1243`). Su ADR-0036 corregido declara intactos los portadores
  de las iniciativas integradas, propiedades de rack incluidas (`0036-rackmirror-espejo-semantico-por-copia.md:88-90`),
  y cita D-21 de I-54 (`:162`).
- **I-53 V2** no añade datos persistidos; sus censos son de `x:Name`, no de comandos ni de ventanas
  (`I53:docs/initiatives/I-53-proposal-v2.md:883-884,917,921`), y declara «sin archivos de I-53» en I-54 (`:896`).
- **Numeracion de ADR**: 0035 aceptado en `main`; 0036 (I-52) y 0037 (I-53) tomados en sus ramas.

Conclusion: ninguna medicion de §2.1-§2.6 cambia, y ningun avance invalida los cambios C-1..C-8 de la Proposal V3.

## 3. Metadatos existentes

### 3.1 La lista del Coordinador, uno por uno

| Metadato buscado | ¿Existe como metadato de usuario? | Lo mas parecido que existe |
|---|---|---|
| ProjectName | **No** (0 coincidencias) | «Proyecto» significa archivo `.rackcad.json` o «variables de proyecto» |
| Client / Cliente | **No** | textos de UI («client-facing name») y el ejemplo «Holgura Cliente A» de HANDOFF |
| Area / Zona | **No** | area geometrica de seccion; `DynamicHeaderHeightZone` |
| Location / Ubicacion | **No** | `"Ubicación"` de la bota de Push Back (`UI/Systems/PushBack/PushBackBootSection.cs:46`) |
| Revision | **No** | revision de la **fuente AISC** (`assets/catalogs/structural-section-sources.csv:1`) |
| Notes / Notas | **No a nivel proyecto/rack** | `FrameHorizontal.Notes` (travesaño), `DynamicRackModule(Design).Notes` (sin UI) |
| Description | **No a nivel proyecto/rack** | `PostAssembly.Description`, `BasePlatePlacement.Description`, descripciones de catalogo y de BOM |
| Code / Codigo | **No** | codigos de diagnostico; `PartNumber` de catalogo |

[L] Conteos por termino, falsos positivos y citas en el registro de auditoria resumido en §3.2-§3.6.

### 3.2 Nivel proyecto/dibujo

- **Ningun** metadato descriptivo en `src/`. Lo unico autorado a nivel dibujo es el registro de variables
  de proyecto, numerico `Length` (`src/RackCad.Application/ProjectVariables/VariableType.cs:22`). [L]
- La columna «Modificado» de la biblioteca es la fecha del **sistema de archivos**
  (`src/RackCad.Application/Persistence/RackDesignLibrary.cs:92`). [E]
- **0 usos** de `Database.SummaryInfo`, `AttributeDefinition`, `AttributeReference`, `MText` y XData en
  `src/` y `tests/`. `DBText` solo en 4 sitios (nombre del rack, numeros de frente/nivel, celdas de layout). [L]

### 3.3 Nivel rack

- `RackEmbedDocument.Id` y `.Name` (`src/RackCad.Application/Persistence/RackEmbedDocument.cs:49-53`). [E]
- Identidad **interior** por sistema [L]:

  | Sistema | Id interior | Name interior | Cita |
  |---|---|---|---|
  | Selectivo | si | si | `SelectivePalletDesignDocument.cs:49-53` |
  | Dinamico | no | no (solo `DrawRackName`) | `RackProjectDocument.cs:18-52`, `DynamicRackSystemDocument.cs:60` |
  | Push Back | no | no | `PushBackKindHandler.cs:13-15,75` |
  | Cabecera | no | si (`Header.Name`) | `RackFrameProjectDocument.cs:18` |
  | Cama | no | no | `FlowBedDocument.cs:21-33` |
  | Cantilever | si (`Guid`) | si | `src/RackCad.Domain/Systems/Cantilever/CantileverLineDesign.cs:333-336` |
  | Larguero | no | si (solo biblioteca, nunca embebido) | `LargueroDocument.cs:9-11,24` |

- El nombre se **dibuja** como `DBText` en `RACKCAD_ANOTACIONES` cuando `DrawRackName` esta activo
  (Selectivo frontal/lateral/planta, Dinamico, lateral A de Push Back), y se usa como nombre de
  definicion de bloque (`RackBlockRenamer.SyncName`). [L]
- **No hay comando de renombrado**: el unico camino es `RACKEDITAR`. `SyncName` solo renombra el BTR y no
  toca el sobre. [L]

### 3.4 Nivel pieza (no son metadatos de usuario del rack)

`FrameHorizontal.Notes` con columna «Notas» en el configurador; `PostAssembly.Description` (inicializada
desde catalogo, editable); `BasePlatePlacement.Description` (sin UI); `DynamicRackModule(Design).Notes`
(sin UI); `StandardBaselineId/Version` (sistema, solo lectura). [L]

### 3.5 Instalacion y usuario

`UserSettings` (`BlockLibraryPath`, `DesignLibraryPath` —sin escritor en codigo—, `SelectiveTargetFondos`),
`defaults.json`/`RackDefaults` y `user-templates.json` (plantillas de **cabecera**). Ninguno guarda autor,
empresa ni cliente. [L]

### 3.6 Catalogo, BOM y exportaciones

- Piezas de catalogo: `Id, DisplayName, Description, Material, PartNumber, Manufacturer, Finish, UnitCost,
  Currency, CostUnit` y una **bolsa abierta** `Properties: Dictionary<string,string>` alimentada con las
  columnas extra del CSV y **sin lector** en `src/` (`src/RackCad.Application/Catalogs/CatalogEntries.cs:14-33`). [L]
- Exportaciones del BOM (XLSX escrito a mano y CSV): **sin** «Proyecto», «Cliente» ni fecha; el XLSX **no
  escribe `docProps`**; solo el BOM consolidado tiene columna «Rack» (`src/RackCad.Application/Bom/XlsxWriter.cs:65-73`,
  `ConsolidatedBomXlsxExporter.cs:21`, `ConsolidatedBomCsvExporter.cs:15`). [L]
- `RACKLISTA`: columnas Nombre, Tipo, Vistas, Copias (`src/RackCad.UI/RackListWindow.xaml:46-49`). [L]

### 3.7 Conclusion de §3

El **unico precedente** de metadato de rack es `Name`: vive en el sobre de **cada** vista hermana, cada
`RACKEDITAR` lo reescribe en las hermanas que redibuja con exito [L], `RACKLISTA` **tolera** divergencia (toma
el primero no vacio, `RackListBuilder.cs:62`) [E] y `RACKDUPLICAR` **falla cerrado** ante nombres divergentes
(`RackDuplicationPlan.cs:522-533`) [E]. Dos politicas distintas para el mismo dato.

## 4. Persistencia real de nivel dibujo

### 4.1 Estructura fisica de `RACKCAD_PROJECT` [E]

```text
Database.NamedObjectsDictionaryId (DBDictionary = NOD)
 └─ "RACKCAD_PROJECT" ─► Xrecord
      Data = [TypedValue(DxfCode.Text, json[0..255)), TypedValue(DxfCode.Text, json[255..510)), ...]
```

- Clave: `public const string DictKey = "RACKCAD_PROJECT";` (`src/RackCad.Plugin/ProjectVariablesData.cs:36`).
- Troceado 255 (`:38`, `:115-122`); lectura por concatenacion de los `DxfCode.Text` en orden (`:76-84`).
- Escritura: si la entrada existe reemplaza `Data` completo (`:124-128`); si no, `SetAt` + `AddNewlyCreatedDBObject`
  del Xrecord **directamente bajo el NOD** (`:131-133`). No existe `new DBDictionary` en `src/`. [E/L]

### 4.2 Casos de lectura [E]

| Situacion | Resultado | Cita |
|---|---|---|
| transaccion o base nulas | PresentButUnreadable | `ProjectVariablesData.cs:47-51` |
| clave ausente | **Absent** | `:59-62` |
| la entrada existe pero **no es Xrecord** (p. ej. un `DBDictionary`) | PresentButUnreadable | `:64-68` |
| Xrecord sin `Data` | PresentButUnreadable | `:70-74` |
| sin ningun `DxfCode.Text` | PresentButUnreadable | `:86-88` |
| excepcion de AutoCAD | PresentButUnreadable (nunca lanza) | `:91-97` |

### 4.3 Store, resultado de lectura y guarda de escritura [L]

- Estados fisicos `Absent | Present | PresentButUnreadable` (`ProjectVariablesPayload.cs:4-17`); resultados
  `Absent | Readable | PresentButUnreadable | IncompatibleMajor` con `CanWrite` solo para `Absent`/`Readable`
  (`ProjectVariablesReadResult.cs:4-23,60-61`); `Absent()` trae documento vacio valido, los otros traen
  `Document = null` (`:67-80`).
- Clasificacion estricta en `ProjectVariablesStore.Deserialize` (`:101-154`) y `FirstInvalidEntry` (`:157-208`):
  texto vacio, JSON invalido, `SchemaVersion` ausente o no parseable → PresentButUnreadable; major > 1 →
  IncompatibleMajor; minor futuro → Readable y se conserva; campos desconocidos → `ExtensionData` en los
  tres niveles; `VariableId` no GUID, `Name` vacio, `Type` o `Kind` desconocidos, valor no finito →
  PresentButUnreadable. `VariableId` duplicado → Readable en el store pero `AmbiguousIdentity` en la
  acreditacion (`UsableProjectVariablesRegistry.cs:168-175`).
- `SchemaVersion` **sin inicializador** (C4.8-1): con inicializador, «ausente» seria indistinguible de «1.0»
  (`ProjectVariablesDocument.cs:15-23,47`).
- `ProjectVariablesWriteGuard.CanOverwrite` solo mira el `Outcome` de la ultima lectura (`:24-41`); el unico
  `TryWrite` esta en el ejecutor, dentro de su transaccion unica, tras releer y re-acreditar
  (`ProjectVariableMutationExecutor.cs:250-288`, verificado [E]).
- Lecturas: `RACKVARIABLES` (`RackVariablesCommands.cs:109`), `RACKEDITAR` del Selectivo (`RackSelectivoCommands.cs:285`),
  `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs:56`) y el ejecutor (`:254`). Un registro ilegible
  **bloquea** los tres comandos.

### 4.4 Otras claves y mecanismos de nivel dibujo [L]

`RACKCAD_PROJECT` es la **unica** clave RackCad en el NOD. 0 usos de `RegAppTable`, XData, `SummaryInfo`,
`Ldata`, `ExtensionDictionary` sobre `Database` o `Layout`. No hay convencion central de nombres `RACKCAD_*`:
cada constante es local.

### 4.5 Discrepancia documental no registrada

La Proposal V4 de I-47 (D-01, A1: `docs/initiatives/I-47-proposal-v4.md:602`) y su plan (G7,
`docs/initiatives/I-47-implementation-plan.md:418-419,427,969`) describen «sub-`DBDictionary`
`RACKCAD_PROJECT` → `Xrecord`». El codigo implemento un Xrecord directo y **ningun documento registra ese
cambio**. [L] Hallazgo F-03 (§15).

### 4.6 Consecuencia verificable de migrar `RACKCAD_PROJECT`

Si una version futura convirtiera la entrada en un `DBDictionary`, todo build que ya existe entraria por
`ProjectVariablesData.cs:64-68` («existe pero no es un Xrecord de RackCad») → `PresentButUnreadable` →
bloqueo de `RACKVARIABLES`, `RACKEDITAR` del Selectivo y `RACKBOMTOTAL`. [E lectura; I sobre builds
desplegados] **Es evidencia fuerte en contra de migrar.**

### 4.7 ADR-0034 aplicable [L]

§1 «un unico `ProjectVariablesDocument` por DWG… la unica autoridad de variables de proyecto» (`:65-66`);
§8 doctrina `UNKNOWN/UNREADABLE/BROKEN ≠ ABSENT/NEGATIVE/EMPTY/SUCCESS` (`:108-118`); §13 versionado propio
(`:149-157`); §14 el Plugin es el unico dueño del NOD (`:162-163`). El ADR **no prohibe** ni condiciona
expresamente otros datos bajo `RACKCAD_PROJECT`; los condicionan §1, §13 y §15 («tipos distintos»). Cross-DWG
y biblioteca **no** estan en el ADR: estan en el contrato y las decisiones de I-47 (C2-8, C4-10).

### 4.8 Entre dibujos

No hay codigo que copie datos del NOD entre dibujos. OV-9 de I-47 fabrico un vinculo roto legitimo
**copiando un rack vinculado a un DWG sin registro** (`docs/HANDOFF.md:155-158`): el rack (sobre + diseño)
viajo y el registro no. El comportamiento de `PURGE`, `WBLOCK`, `INSERT` y copiar/pegar sobre una entrada
del NOD sin `XrecordMergeStyle` ni `DuplicateRecordCloning` explicitos **no esta verificado**. [L/I]

## 5. Persistencia real de nivel rack

### 5.1 `RackEmbedDocument` y `RackEmbedStore` [E]

- Campos: `SchemaVersion` (inicializado a `"1.0"`, `:33-35`), `Kind` (`:38`), `View` (`:41`), `Section`
  (inicializado a `-1`, `:47`), `Id` (`:50`), `Name` (`:53`), `Design` (JSON **como string**, `:56`) y
  `[JsonExtensionData] Dictionary<string, JsonElement> ExtensionData` (`:63-64`). Ningun otro.
- `RackEmbedStore`: `WriteIndented = false`, `PropertyNameCaseInsensitive = true`, **sin**
  `DefaultIgnoreCondition` (`:70-74`) → los nulos se escriben [I]. `Deserialize` devuelve `null` —nunca
  lanza— ante texto en blanco (`:88-91`), `JsonException` (`:98-101`), resultado nulo (`:103-106`) o major
  futuro (`:112-115`); el comentario explica por que: un bloque futuro no debe abortar el barrido del dibujo
  (`:108-111`).
- `SchemaVersionPolicy.IsReadable`: `MajorOf(stored) <= MajorOf(current)`; ausente o no parseable = major 1
  (`SchemaVersionPolicy.cs:27-28,54`). [L]

### 5.2 `RackBlockData`: el sobre vive en la DEFINICION

- Xrecord con clave `"RACKCAD_SELECTIVE"` **para todos los kinds** (`src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:16`),
  en el diccionario de extension del objeto recibido —que crea si falta (`:28-31`)—, troceado 255 (`:18,35-39`). [E]
- Los **4** `Read` y los **6** `Write` operan sobre `BlockTableRecord`: `RackBlockFinder.cs:71` [E],
  `RackCommandSupport.cs:93`, `RackLayoutCommands.cs:191`, `RackDuplicarCommands.cs:215`;
  `SystemBlockWriter.cs:33,114`, `LateralHeaderDrawService.cs:258`, `RackCloner.cs:66` [E],
  `RackCantileverCommands.cs:156,324`.
- El resumen XML dice «block reference» y es **falso** (`RackBlockData.cs:8-11`; ya registrado como H-01 de
  I-47). `Read` y `Write` hacen un cast duro `(Xrecord)` sin `try` (`:43`, `:73`), a diferencia de
  `ProjectVariablesData`. [E]

### 5.3 `RackEmbedComposer.Compose`: la costura, y su limite [E]

```csharp
return new RackEmbedDocument
{
    SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source?.SchemaVersion, RackEmbedDocument.CurrentSchemaVersion),
    Kind = kind, Id = id, Name = name, View = view, Section = section, Design = design,
    ExtensionData = source?.ExtensionData
};
```

(`src/RackCad.Application/Persistence/RackEmbedComposer.cs:24-33`). Contrato XML (`:10-15`): redibujo → el
sobre **propio** de ese bloque; vista nueva → el sobre **picado**; rack nuevo → `null`.

- **Siete llamadores**, todos en el Plugin: `RackSelectivoCommands.cs:390` [E], `RackDinamicoCommands.cs:380`,
  `RackPushBackCommands.cs:415`, `RackCabeceraCommands.cs:195`, `RackCamaCommands.cs:197`,
  `RackCantileverCommands.cs:493` y `ProjectVariableMutationExecutor.cs:208` [E]. El unico `new RackEmbedDocument`
  de `src/` es el del compositor. [L]
- Pruebas que fijan la herencia de `ExtensionData` y version: `PersistenceReopenPreservationTests.cs:101-150` [E].

### 5.4 Matriz de supervivencia de un campo del sobre

| Camino | Campo **no declarado** (`ExtensionData`) | Campo **declarado nuevo**, `Compose` sin cambiar | Campo declarado, `Compose` lo hereda de `source` |
|---|---|---|---|
| `RACKEDITAR` redibuja una vista (6 sistemas) | sobrevive (el suyo) | **se pierde** | sobrevive (el suyo) |
| `RACKEDITAR` inserta vista nueva | hereda el del sobre picado | **se pierde** | hereda el del picado |
| Ejecutor de variables (`Compose(view.Embed,…)`) | sobrevive (el suyo) | **se pierde** | sobrevive (el suyo) |
| Rack nuevo (menu, `Quick*`, importacion de biblioteca) | no hay | no hay | no hay |
| `RACKDUPLICAR` / `RACKLAYOUT` (`RestampEnvelope`) | sobrevive | **sobrevive** (mismo objeto) | sobrevive |
| Guardar en biblioteca | no llega al archivo | no llega | no llega |
| Build anterior a I-54 (con I-11) que redibuja | sobrevive | sobrevive (para el es desconocido) | sobrevive |

[E para `Compose` y `RestampEnvelope`; L para los caminos por sistema; I para builds anteriores, deducido de
que `ExtensionData` y la herencia existen desde I-11.]

### 5.5 Identidad y vista

- El GUID vive en el sobre; se conserva al actualizar y lo comparten las definiciones del rack (ADR-0009
  `:16-20,29-31`); «el nombre visible no es identidad estable» (context pack `persistence`). [L/E]
- `View` nulo se interpreta distinto segun el consumidor: el XML dice frontal (`RackEmbedDocument.cs:40`),
  `RackListBuilder` lo trata como lateral (`:117-118`), Selectivo como frontal, Dinamico como lateral legado,
  y Cama siempre escribe `view: null`. [L] Hallazgo F-04.

### 5.6 Definicion frente a referencia

ADR-0010: «La definicion de bloque contiene la geometria y el sobre de esa representacion; la
`BlockReference` unicamente la coloca» (`:42-43`). `COPY` de AutoCAD comparte la definicion y por tanto el
rack; `RACKDUPLICAR`/`RACKLAYOUT` crean identidad independiente. [L]

## 6. Autoridad entre vistas hermanas

### 6.1 `SelectiveAuthoredAuthority` [E]

- Recibe **solo** documentos `SelectivePalletDesignDocument` (`:96`, `:149`). Comparacion estructural del
  arbol persistido, **incluir por defecto**, sin depender de bytes ni de orden de miembros (`:63-70`).
- Participan `SchemaVersion` y `ExtensionData` del documento interior (`:72-77`). **Se excluye por diseño lo
  que pertenece a la VISTA** —vista y seccion del sobre— «and neither of them lives in this document»
  (`:79-82`).
- Una hermana ilegible **nunca** se filtra para seguir con las legibles (`:124-147`); divergencia → aborta
  (`:57-60`, `:152-156`).

### 6.2 Otros comparadores y portadores de hecho [L]

| Superficie | Regla |
|---|---|
| `BomAuthoredAuthority` | representante = primera hermana en barrido; **si no es Selectivo, se aprueba sin comparar** (`BomAuthoredAuthority.cs:106` [E]) |
| `ProjectVariableConsumerDiscovery` | cualquier sobre no interpretable del dibujo aborta la operacion (`:104-109,197-216`); agrupa solo Selectivos |
| `RackDuplicationPlan` | mezcla de kinds → error; **`Name` del sobre divergente → error para todos los kinds** (`:514-533` [E]); Selectivo con varias definiciones → `IsSameAuthority` |
| Preflights de edicion | Push Back y Cantilever comprueban Kind + Id + descriptor de cada hermana; Selectivo, Dinamico y Cabecera no comprueban Kind |
| `RACKLISTA` | primer `Name` y primer `Kind` no vacios (`RackListBuilder.cs:62-63`) |
| `RACKEDITAR` Selectivo | la vista **picada** es el portador del diseño para todas las hermanas (`RackSelectivoCommands.cs:358-360` [E]) |

### 6.3 `FindRackBlocks` [E]

Filtra `Embed != null` y `Id` igual sin distinguir mayusculas, **sin filtro de Kind** y **descartando en
silencio** los sobres ilegibles (`src/RackCad.Plugin/RackCommandSupport.cs:120-127`). Una hermana cuyo sobre
no se puede leer **no puede identificarse** como hermana.

### 6.4 Conclusion de §6

**No existe autoridad de nivel sobre.** El `Name` del sobre es el unico dato de rack que ya se replica por
hermana, y su divergencia se trata de forma distinta en cada consumidor. Un metadato de rack nuevo necesita
su **propia** autoridad; reutilizar `SelectiveAuthoredAuthority` lo acoplaria al diseño interior, al
Selectivo y a la semantica de variables.

## 7. `RACKEDITAR` y handlers por kind

### 7.1 Flujo comun [L]

`RED`/`RACKEDITAR` (`RackMenuCommands.cs:18,115`) → `PickRackBlock` → `RackBlockData.Read` de
`reference.BlockTableRecord` en `InDocumentTransaction` (`RackCommandSupport.cs:89-94`) → `RackEmbedStore.Deserialize`
→ si nulo o `Design` vacio: «ese bloque no tiene datos de rack editables» (`RackMenuCommands.cs:132-135`) →
`KindHandlerDispatch.TryResolve` (**ordinal**) → `handler.Edit(document, blockId, embed)` (`:141-146`).

`IRackKindHandler` (`KindHandlers/IRackKindHandler.cs:19-75`): `Kind`, `BomLabel`, `Edit`, `BuildBom`,
`OutputBlockedReason`, `RestampDesign`. Seis handlers en orden fijo, con guarda de orden y conteo
(`KindHandlerRegistry.cs:55-63`; `PushBackRoundTripSourceGuardTests.cs:134-162`).

### 7.2 Por sistema [L]

| Sistema | Hermanas | Redibujo | Insertar vista | Borra huerfanas | Transacciones | Regen |
|---|---|---|---|---|---|---|
| Selectivo | `FindRackBlocks(id)` + fallback del picado | frontal/planta via `ViewBlockDraw`; lateral via `LateralHeaderDrawService`; `regen:false` | frontal/lateral/planta con prompt de fondo o poste | si, si quedan vivas | **una por vista** | 1 |
| Dinamico | `FindRackBlocks` + fallback | `Dynamic{Planta,Frontal,System}DrawService` | lateral/frontal/planta | lateral, sin mensaje | una por vista | 1 |
| Push Back | `FindRackBlocks` + preflight Kind/Id | `PushBack*DrawService` | lateral/frontal/planta | lateral, con mensaje | una por vista | 1 |
| Cantilever | `FindRackBlocks` + preflight Kind/Id | `CantileverViewMaterializer.RedefineBlock` | frontal/planta/lateral | lateral, con mensaje | una por vista | 1 + 1 al insertar |
| Cabecera | `FindRackBlocks`; lateral = no planta | `LateralHeaderDrawService` / `PlantaHeaderDrawService` | lateral o planta | no | una por vista | 1 |
| Cama | **solo la definicion picada** | `FlowBedDrawService` (`regen:true`) | no | no | 1 | 1 |

Todos componen el payload de cada hermana con `Compose` y **su propio** sobre (§5.3). Un fallo en una vista
no aborta las demas; solo el ejecutor de variables usa una transaccion unica (`ProjectVariableMutationExecutor.cs:250-288` [E]).

### 7.3 Identidad en el editor [L]

El Selectivo adopta Id y Name del **documento interior** (`RackSelectiveWindow.xaml.cs:2758-2764`); Dinamico,
Push Back, Cantilever y Cama adoptan los del **sobre**; la Cabecera no tiene sesion ni Id (acuña GUID si el
sobre no lo trae, `RackCabeceraCommands.cs:233`); la Cama usa `window.RackId/RackName` sin fallback. El
`Name` editado llega al sobre de todas las hermanas redibujadas con exito; `SyncName` renombra cada BTR. El
ejecutor de variables reescribe el `Name` del sobre y no llama a `SyncName`.

### 7.4 Otros lectores del sobre [L]

| Superficie | Campos que lee | Agrupa | Sobre ilegible |
|---|---|---|---|
| `RACKLISTA` | Id, Kind, Name, View, Section + referencias | por Id | lo ignora en silencio |
| `RACKBOMTOTAL` | Id, Kind, Name, Design + referencias | por Id | colocado: **aborta**; no colocado: ignora |
| `RACKLAYOUT` | Id, Kind, Name, View, payload crudo | Id → planta | picado: avisa |
| `RACKRELLENAR` | Id, Name, View | Id → planta | picado: avisa (sin gate de Kind) |
| `RACKVARIABLES` | Id, Kind, Design, View, Section, handle + referencias | Id, solo Selectivo | operacion: **aborta** ante cualquiera; ventana: bloquea si esta colocado |
| `RACKDUPLICAR` | Id, Kind, Name, Design (y todo el sobre al re-estampar) | Id, o definicion si no hay Id | **aborta** todo |

### 7.5 Consecuencia para I-54

Si el compositor hereda el campo de rack desde `source`, **ninguno de los seis flujos de edicion necesita
cambiar** para preservar metadatos de rack, y las vistas nuevas heredan los del sobre picado. [E sobre
`Compose`; L sobre los flujos]

## 8. Duplicacion (I-51)

### 8.1 `RestampEnvelope` [E]

- La firma historica delega con `Guid.NewGuid()` (`src/RackCad.Plugin/RackEnvelopeRestamp.cs:35-36`); la
  unica implementacion (`:52-88`) falla ante `Guid.Empty`, sobre ilegible o id igual al de origen.
- Deserializa, **muta el mismo objeto** (`embed.Id`, `embed.Name`, `embed.Design`) y serializa ese objeto
  (`:59-60,75-76,86-87`). `Kind`, `View`, `Section`, `SchemaVersion`, `ExtensionData` —y **cualquier campo
  declarado** que el tipo tenga— viajan intactos.

### 8.2 `RestampDesign` por kind [L]

| Kind | Implementacion | Re-serializa | Cambia | Pierde |
|---|---|---|---|---|
| selective | `SelectiveAuthoredRestamp` (`RestampResult.cs:57-83`) | si, por el DTO | Id, Name | claves desconocidas en DTO anidados sin `ExtensionData` |
| dynamic, pushback, cama | no-op | no | nada | nada |
| cabecera | `CabeceraKindHandler.cs:44-66` | si, por dominio | `Header.Name` | `RackFrameProjectDocument` sin `ExtensionData`, version vuelve a 1.0 |
| cantilever | `CantileverKindHandler.cs:80-110` | si | Id, Name | desconocidos dentro de `Line` |

### 8.3 `RackDuplicationPlan`, `RackDuplicarCommands` y `RackCloner`

- El plan puro consume `Id`, `Kind`, `Name` y `Design` del sobre; **no** lee `View`, `Section`,
  `SchemaVersion` ni `ExtensionData` (`src/RackCad.Application/Persistence/RackDuplicationPlan.cs:402-549`). [L]
- `RackDuplicarCommands`: snapshot en una transaccion, preflight con el plan, ensayo de restamp antes del
  punto base, `Prepare` con `RestampEnvelope(RawPayload, CopyName, NewRackId)` y **una** transaccion por
  destino (`:173-313`). [L]
- `RackCloner.CloneDefinition` [E]: crea `new BlockTableRecord { Name = UniqueBlockName(...), Origin = ... }`
  (`:34-36`), clona **solo las entidades** con `DeepCloneObjects` (`:38-50`) y escribe el payload recibido
  (`:66`). El diccionario de extension del BTR de origen **no viaja** [I]: cualquier dato de rack guardado
  fuera del Xrecord del sobre se perderia en una copia independiente. El comentario «replace the cloned
  (old) payload» describe un payload clonado que no existe. Hallazgo F-02.

### 8.4 Hueco de prueba

Ninguna prueba hace pasar un campo desconocido del **sobre exterior** por `RestampEnvelope` ni afirma `View`,
`Section` o `SchemaVersion` tras un restamp: ningun proyecto de pruebas referencia el Plugin
(`tests/RackCad.Tests/RackCad.Tests.csproj:20-27`). La mas cercana replica a mano la mitad-sobre
(`PersistenceUniformityTests.cs:193-217`). El contrato de I-51 declara esa preservacion como INV-09
(`I-51-rackduplicar-multiples-origenes.md:244-248`), pero T1-T15 y M1-M9 no la cubren. [L] Hallazgo F-10.

## 9. Biblioteca, exportacion, importacion y cruce entre dibujos

### 9.1 Biblioteca [E/L]

- `.rackcad.json` en `UserSettings.DesignLibraryPath` o `%APPDATA%\RackCad\Designs`. El envoltorio
  `RackProjectDocument` (version 2.0, un slot por sistema, `ExtensionData`) **nunca contiene el sobre**. La
  cabecera se guarda desnuda con `RackFrameProjectStore`. [L]
- El listado toma `Header?.Name ?? SelectiveRack?.Name ?? Larguero?.Name` o el nombre del archivo, y **omite
  en silencio** lo ilegible (`RackDesignLibrary.cs:86-97`). [E] Hallazgo F-08 (contra C4.7-5 de I-47).

### 9.2 Exportacion por sistema [L]

| Sistema | Parte de | Id en el archivo | Name en el archivo | Campos del sobre |
|---|---|---|---|---|
| Selectivo | diseño **efectivo** de dominio via `SelectiveLibraryExport.FromEffective` (`SelectiveLibraryExport.cs:85-94` [E]; UI `RackSelectiveWindow.xaml.cs:2797`) | Id de sesion | cuadro de nombre | ninguno |
| Dinamico | dominio | no | no | ninguno |
| Push Back | `lastComputation.Design` | no | no | ninguno |
| Cantilever | `lastComputation.Design` | `Line.Id` interior | `Line.Name` | ninguno |
| Cama | dominio | no | no | ninguno |
| Cabecera | configuracion | no | `Name` | ninguno |
| Larguero | dominio | no | `Name` | ninguno |

`SelectiveLibraryExport.Materialize` no tiene llamador en `src/`. La razon escrita del exportador aplica por
analogia a cualquier dato ligado al dibujo: «A `.rackcad.json` lives outside the drawing, and project
variables belong TO the drawing» (`SelectiveLibraryExport.cs:35-40`). [E]

### 9.3 Importacion [L]

`OpenDesignLibrary_Click` → `RackDesignLibraryWindow` → `OpenFromLibrary` → `InsertionRequest` → despacho por
tipo CLR (`RackMenuCommands.cs:53-106`). **GUID nuevo en todos los sistemas** (`Adopt(null,…)` y
`RackEditorSession.EnsureId`), sobre compuesto con `Compose(null, …)`. Dinamico, Push Back y Cantilever
llevan metadatos del envoltorio al **diseño interior** (`innerSource`); el sobre nace limpio.

### 9.4 Cruce real entre dibujos [L/I]

La unica `Database` ajena es `blocks-library.dwg` (`BlockLibraryImporter.cs:127-170`, `WblockCloneObjects` con
`DuplicateRecordCloning.Ignore` solo para nombres ausentes). `WBLOCK`, `PASTECLIP`, `COPYCLIP`, `SaveAs`,
eventos de clonacion e `Insert` entre bases: **0** en `src/`. No hay codigo de copia de racks entre dibujos:
un rack cruza a otro DWG solo con mecanismos nativos de AutoCAD, y con el viaja el payload de su definicion;
el NOD no.

## 10. Schema, versiones y `JsonExtensionData`

### 10.1 Politicas [L]

- `SchemaVersionPolicy.ResolveWriteVersion`: mismo major → el minor mayor; otro caso → `current`
  (`SchemaVersionPolicy.cs:40-51`). `SchemaGuard.CheckReadable` lanza ante major futuro (`SchemaGuard.cs:15-23`).
- Promocion **pegajosa** del Selectivo a 2.0 con vinculos (`SelectiveDesignSchema.cs:43-66`), decidida en
  I-47 porque una version anterior abriria un rack vinculado por el literal y **redibujaria geometria
  incoherente con la autoridad** (F3 rechazada).

### 10.2 DTO relevantes [L]

| DTO | Version | `[JsonExtensionData]` | Ante major futuro | Preserva al re-escribir |
|---|---|---|---|---|
| `RackEmbedDocument` | 1.0 (inicializada) | si | `null`, tolerante | si, via `Compose` |
| `SelectivePalletDesignDocument` | 1.0 / 2.0 pegajosa | si (raiz) | lanza | si |
| `DynamicRackSystemDocument` | sin version propia | **no** | via envoltorio | no aplica |
| `PushBackDesignDocument` | 1.0 | si | lanza | si |
| `RackFrameProjectDocument` | 1.0 | **no** | lanza | **no** |
| `FlowBedDocument` | 1.0 | si | `null` en DWG | si |
| `CantileverLineDocument` | 1.0 | si | lanza | si |
| `RackProjectDocument` | 2.0 | si | lanza | si, con `SourceDocument` |
| `ProjectVariablesDocument` | 1.0 estampada, **sin inicializador** | si, ×3 | `IncompatibleMajor` | si |

`JsonExtensionData` **no es recursivo** (I-11, `I-11-persistencia-uniforme.md:241-247`).

### 10.3 Serializacion y ayudantes [L]

- **No hay `JsonSerializerOptions` compartido**: 13 instancias privadas, todas en Application; Plugin y UI no
  serializan JSON. `RackEmbedStore` escribe nulos [I]; I-47 G15 ya encontro `"PropertyValues": null` en
  disco por esa razon, e I-50 declaro `DimensionViews` con `JsonIgnore(WhenWritingNull)`.
- `AtomicFile` y `CorruptFile` sirven a archivos, no al DWG.
- El troceado de Xrecord esta **duplicado**: `RackBlockData` (lectura nula, cast duro, sin catch) y
  `ProjectVariablesData` (tri-estado, `is Xrecord`, nunca lanza), mas una tercera copia en
  `ProjectVariablesRegistryAccessTests.cs:202-212`.

## 11. UI, CRUD reutilizable y ayudantes

### 11.1 Patron `RACKVARIABLES` [L]

Bucle `leer → proyectar → ventana modal → intent → preflight → ejecutor → releer` (`RackVariablesCommands.cs:47-82`).
Lectura en **una** transaccion (registro + barrido + proyeccion + `ProjectVariablesWorkspace.Build`,
`:101-125`). La ventana no ve `Database`, `Transaction`, `ObjectId` ni el registro; devuelve **un** intent
por `VariableId` (`RackProjectVariablesWindow.xaml.cs:56,209-213`).

| Pieza | Generica | Especifica de variables |
|---|---|---|
| Estado `Editable`/`Blocked` con `Error` | si | — |
| Acreditacion fail-closed ante id duplicado | patron | tipo interno con `VariableId` |
| Intents por identidad; `Create` sin id | forma | `double`, `Length`, `RackId`, `PropertyId` |
| `RegistryMutation` Add/Rename/ChangeValue/Remove sobre clon | si | `VariableDefinition` |
| `RegistryCommit` (leer → acreditar → aplicar), `WriteGuard`, `ReadResult`, store estricto | patron | — |
| E/S NOD por trozos | mecanismo | clave |
| Lista maestra-detalle, banner, estado, salida por intent | si | etiqueta «Valor (in)», `> 0` |
| Confirmacion por casilla (estado, no clic) | si | — |
| Consumidores, `UnlinkAllAndDelete`, reparacion, redibujo | — | si |

### 11.2 Ventanas [E numeros; L reglas]

Censo por reflexion: **29 = 6 A + 6 B + 11 C + 6 D** (`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:166-170`).
`RackProjectVariablesWindow` es arquetipo **C** («dialogo de configuracion transaccional», ADR-0029 D2). ADR-0029
exige motivo en acciones deshabilitadas (D6), cierre sin perdida silenciosa (D7), `Owner`/foco/tamaño (D9) y
adoptar la infraestructura existente (D11: `DialogWindowChrome`, `EditorActions`); `RackProjectVariablesWindow`
**no** adopta `DialogWindowChrome` (hallazgo F-11). Añadir una ventana obliga a actualizar el censo y declarar
arquetipo.

### 11.3 Controles [L]

No existe control para pares nombre/valor ni para texto libre con pendiente/commit/Escape. `PendingTextField<T>`
es `internal` y sin teclado; `LinkedPropertyEditor` es numerico o referencia. Hay DataGrid editables solo en
`RackFrameConfiguratorWindow` y `RackCantileverWindow`.

### 11.4 Identidad y menu [L]

`NameBox` en 7 editores; `RackEditorIdentity.SetName` no valida; `RackEditorSession` **no tiene capacidad de
metadatos**. No hay panel de «datos del proyecto» ni de «propiedades» en ninguna ventana; `MainMenuAction`
solo tiene `None` y `GenerateStructuralSection`.

### 11.5 Comandos [E numero; L resto]

Censo `GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA`: **33** apariciones de `[CommandMethod(` en el Plugin
(`tests/RackCad.Tests/SelectiveEditorOpenTests.cs:539-547`), 17 primarios + 16 alias; guarda de unicidad en
`PushBackRoundTripSourceGuardTests.cs:360-375`. Nombres registrados hoy [E, grep sobre `src/RackCad.Plugin`]:
`RACKCAD/RK`, `RACKEDITAR/RED`, `RACKSELECTIVO/RS`, `RACKSISTEMADINAMICO/RSD`, `RACKPUSHBACK/RPB`,
`RACKCANTILEVER/RCT`, `RACKCABECERA/RCB`, `QUICKCABECERA/QCB`, `QUICKCAMA/QCM`, `RACKLISTA/RL`,
`RACKBOMTOTAL/RB`, `RACKLAYOUT/RLY`, `RACKRELLENAR/RR`, `RACKDUPLICAR/RD`, `RACKVARIABLES/RVA`,
`RACKAYUDA/RA` y `RACKSECCION`. `RACKVARIABLES` no figura en la ayuda `RackCommandReference.cs:30-47`
(hallazgo F-07).

### 11.6 Guardas que condicionan nombres [E]

`tests/RackCad.Tests/ProjectVariablesConformanceTests.cs` busca por `Contains` **ordinal** en todo el codigo
(salvo lineas que empiezan por `//`):

| Proyecto | Prohibido | Linea |
|---|---|---|
| Domain | `ProjectVariables`, `VariableId`, `PropertyValues` | `:114` |
| Application, Plugin, UI | `ExpressionParser`, `FormulaParser`, `DependencyGraph`, `RackPropertyReference`, `rackProperty` | `:129-135` |
| Plugin, UI | `PropertyValues`, `SelectivePropertyValueDocument` | `:162` |

Un identificador como `CustomPropertyValues` en Plugin o UI, o una variable `rackPropertyId`, **haria fallar
la suite Core**.

### 11.7 Costuras de prompts [L]

`EditorDiscardPrompt.Substitute` y `SelectiveCabeceraHeightPrompt.Substitute`; la suite UI instala
`Substitute(_ => true)` permanente (`StaTestRunner.cs:57`). Un `MessageBox` sin costura en la pila de un test
headless es un cuelgue (leccion de I-48 G4G).

## 12. Hallazgos preliminares del Coordinador: confirmacion o correccion

| # | Hallazgo | Veredicto | Evidencia |
|---|---|---|---|
| H-1 | `origin/main` = `46fcac2b071929d2bd5b07aa28373941417f74a8` | **CONFIRMADO** en el preflight del reclamo y en el ultimo fetch antes de este informe | §2.1 [E] |
| H-2 | `RACKCAD_PROJECT` es Xrecord directo | **CONFIRMADO**, con una correccion documental: los documentos de I-47 describen un sub-diccionario que el codigo no creo, sin registro | §4.1, §4.5 [E/L] |
| H-3 | `RackEmbedDocument` es la costura comun natural | **CONFIRMADO CON CORRECCION MATERIAL**: es la unica costura uniforme de los seis kinds, pero `Compose` **pierde** un campo declarado nuevo en redibujo, vista nueva y propagacion, y **ninguna autoridad** compara campos del sobre entre hermanas | §5.3, §5.4, §6 [E] |
| H-4 | I-51 conserva campos del sobre al re-estampar | **CONFIRMADO EN CODIGO, NO PROBADO CONTRACTUALMENTE**: `RestampEnvelope` muta el mismo objeto; ninguna prueba lo ejercita; INV-09 sin cobertura. Correccion adicional: `RackCloner` no copia el diccionario de extension del BTR | §8 [E/L] |
| H-5 | La exportacion del Selectivo deriva del diseño interior y no llevaria metadata del sobre | **CONFIRMADO Y AMPLIADO**: **ningun** sistema escribe campos del sobre en `.rackcad.json`, y la importacion compone un sobre limpio con GUID nuevo | §9 [E/L] |
| H-6 | I-49 no debe ser dependencia | **CONFIRMADO**: nada de I-54 la requiere. Cruce **semantico**: I-49 reserva `Rack`/`Project` para ID20 y su motor no admite texto | §2.3 [L] |
| H-7 | I-50 puede tocar DTOs del Selectivo | **MEDIDO**: toca `SelectivePalletDesignDocument.cs` y `DynamicRackSystemDocument.cs`; **no** toca sobre, compositor, restamp, cloner ni autoridad. Cruce productivo = 0 si I-54 no toca DTO de sistema | §2.2 [E/L] |
| H-8 | (nuevo) Paralelas surgidas durante G0/G1 | I-52 (RACKMIRROR) reutilizara el restamp; I-53 excluye I-54 y toca editores/DTO de sistema | §2.4 [E/L] |

## 13. Mapa de archivos y simbolos, con cruces

Archivos que una fundacion de propiedades personalizadas **leeria, reutilizaria o tocaria** segun las
opciones abiertas. **No es alcance aprobado**: el alcance lo fija el consenso de G2.

| Area | Archivo : simbolo | Papel para I-54 | I-49 | I-50 | I-52 | I-53 |
|---|---|---|---|---|---|---|
| Sobre | `Application/Persistence/RackEmbedDocument.cs` : `RackEmbedDocument`, `RackEmbedStore` | costura de rack; campo aditivo | no | **no** | por medir (G1 pendiente) | por medir |
| Sobre | `Application/Persistence/RackEmbedComposer.cs` : `Compose` | herencia del campo; 7 llamadores | no | **no** | por medir | por medir |
| Sobre | `Plugin/RackEnvelopeRestamp.cs` : `RestampEnvelope` | copia independiente; sin cambio previsto | no | no | **probable lectura/uso** | no previsto |
| Sobre | `Plugin/RackCloner.cs` : `CloneDefinition` | BTR nuevo + payload | no | no | probable uso | no previsto |
| Sobre | `Plugin/Systems/Shared/RackBlockData.cs` : `Read`/`Write` | escritura por hermana | no | no | por medir | por medir |
| Hermanas | `Plugin/RackCommandSupport.cs` : `FindRackBlocks` | localizar hermanas | no | no | por medir | por medir |
| Hermanas | `Plugin/RackBlockFinder.cs` : `ScanEnvelopes` | barrido con conteo de referencias | no | no | por medir | por medir |
| Hermanas | `Application/ProjectVariables/SelectiveAuthoredAuthority.cs` | **no reutilizar** para metadatos | **planea tocar** | usa en pruebas | no | posible |
| Dibujo | `Plugin/ProjectVariablesData.cs`, `ProjectVariablesRegistry.cs` | patron NOD a imitar, **no tocar** | no | no | no | no |
| Dibujo | `Application/Persistence/ProjectVariables*` | patron store/resultado/guarda | no | no | no | no |
| Biblioteca | `Application/Persistence/RackProjectDocument.cs`, `RackProjectStore.cs`, `RackDesignLibrary.cs` | solo si la biblioteca llevara propiedades | no | usa en pruebas | no | posible |
| Biblioteca | `Application/Systems/Selective/SelectiveLibraryExport.cs` | idem | **planea tocar** | no | no | no |
| DTO sistema | `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs`, Domain | **evitar** | toca vinculos | **toca** | no | **probable** |
| UI | `RackProjectVariablesWindow.xaml(.cs)` | patron a imitar, no tocar | no | no | no | no |
| UI | editores de sistema (`RackSelectiveWindow`, etc.) | **evitar** (calientes) | posible | **G3 pendiente** | posible | **probable** |
| UI | `RackCommandReference.cs` | ayuda del comando nuevo | no | no | **probable** (comando nuevo) | posible |
| Tests | `WindowCensusGuardTests.cs`, `SelectiveEditorOpenTests.cs` (censos 29 / 33) | numeros cambian | no | no | **probable** (comando) | posible (ventanas) |
| Tests | `ProjectVariablesConformanceTests.cs` | restricciones de nombres | vigila | no | no | no |
| Docs | `ROADMAP.md` (fila tras I-51), `ideas-futuras.md` (final), `adr/README.md` (numeracion) | conflicto textual al integrar | si | si | si | si |

## 14. Riesgos

| # | Riesgo | Donde se ve | Severidad |
|---|---|---|---|
| R-01 | Campo declarado en el sobre perdido en cada redibujo si `Compose` no lo hereda | §5.3-§5.4 | alta |
| R-02 | Un contenido malformado o de forma futura dentro de un campo **tipado** del sobre haria fallar `RackEmbedStore.Deserialize` → rack entero ilegible (no editable, fuera de `RACKLISTA`, aborta `RACKBOMTOTAL` si esta colocado) | §5.1, §7.4 [I] | alta |
| R-03 | Divergencia de metadatos entre hermanas sin deteccion (transacciones por vista en `RACKEDITAR`; ninguna autoridad de sobre) | §6, §7.2 | media |
| R-04 | Pertenencia indeterminada: una hermana con sobre ilegible no es identificable | §6.3 | media |
| R-05 | Migrar `RACKCAD_PROJECT` bloquea tres comandos en todos los builds existentes | §4.6 | alta si se hiciera |
| R-06 | Mezclar metadatos con el registro de variables acopla su lectura y su bloqueo (un metadato corrupto bloquearia variables) | §4.3, §4.7 | alta si se hiciera |
| R-07 | Guardar metadatos de rack fuera del sobre (diccionario de extension del BTR) los pierde en copias independientes | §8.3 | alta si se hiciera |
| R-08 | Guardarlos en DTO de sistema: 6 formatos, 2 sin `ExtensionData`, cruce con I-50/I-53, y acoplamiento a `SelectiveAuthoredAuthority` | §5.3, §10.2, §2.2 | alta si se hiciera |
| R-09 | Preservacion en restamp sin prueba; I-52 puede cambiar ese archivo | §8.4, §2.4 | media |
| R-10 | Nombres que rompen guardas (`PropertyValues`, `rackProperty`) | §11.6 | baja (evitable) |
| R-11 | Colision futura de sintaxis `Rack.X`/`Project.X` con ID20 | §2.3 | media (futuro) |
| R-12 | Builds anteriores a I-11 (sin `ExtensionData` en el sobre) perderian cualquier campo nuevo al redibujar | §10.1 [I] | depende de lo desplegado |
| R-13 | Serializar nulos cambiaria los bytes de **todo** sobre escrito por I-54 aunque no tenga propiedades | §10.3 [I] | media |
| R-14 | Conflictos textuales de censos (comandos/ventanas), ayuda, ROADMAP e indice ADR con I-50/I-52/I-53 al integrar | §13 | baja |
| R-15 | Metadatos de proyecto no viajan entre dibujos y los de rack si: expectativa del usuario | §4.8, §9.4 | media (producto) |

## 15. Hallazgos fuera de alcance

Se registran aqui y se trasladan a [ideas-futuras.md](../ideas-futuras.md) cuando se fije el contrato (mismo
momento que I-51). **No se arreglan en I-54.**

| # | Hallazgo | Cita |
|---|---|---|
| F-01 | `RackBlockData` dice «block reference» (ya H-01 de I-47, sigue sin corregir) | `RackBlockData.cs:8-11` |
| F-02 | `RackCloner` comenta un «cloned (old) payload» que no existe; el Discovery de I-51 lo repite | `RackCloner.cs:66`; `I-51-discovery.md:119` |
| F-03 | I-47 describe `RACKCAD_PROJECT` como sub-diccionario; el codigo es Xrecord directo; sin registro | §4.5 |
| F-04 | `View` nulo interpretado distinto por consumidor | §5.5 |
| F-05 | Despacho por Kind ordinal (`RACKEDITAR`, `RACKBOMTOTAL`) frente a sin mayusculas (`RACKLAYOUT`, `RACKDUPLICAR`, restamp) | `KindDispatch.cs:75-96` |
| F-06 | `RackListBuilder.KindLabel` sin caso Push Back (el propio codigo ya lo declara hueco adyacente) | `RackListBuilder.cs:92-95` [E] |
| F-07 | `RACKVARIABLES` ausente de la ayuda | `RackCommandReference.cs:30-47` |
| F-08 | La biblioteca omite en silencio un elemento de major incompatible, contra C4.7-5 | `RackDesignLibrary.cs:94-97`; `decisions/I-47.md:557` |
| F-09 | Cantilever importado de biblioteca podria conservar `Line.Id` interior distinto del sobre [I] | `CantileverKindHandler.cs:105` |
| F-10 | INV-09 de I-51 (sobre en restamp) sin prueba | §8.4 |
| F-11 | `RackProjectVariablesWindow` no adopta `DialogWindowChrome`/`EditorActions` (ADR-0029 D11) | §11.2 |
| F-12 | Comentarios desfasados: «five kinds» (`KindHandlerDispatch.cs:12-13`, `RackMenuCommands.cs:138-140`), regen via `DrawAndPlace` en Push Back (`RackPushBackCommands.cs:317-318`), `CantileverLineDesign.cs:335` «shown in the library list» | citados |
| F-13 | Troceado de Xrecord duplicado con conducta distinta: `RackBlockData` (nulo, cast duro, sin catch) frente a `ProjectVariablesData` (tri-estado, nunca lanza), mas una tercera copia en pruebas (addendum de G2A) | `RackBlockData.cs:18,35-39,43,73,79-88` [E]; `ProjectVariablesData.cs:38,59-89,115-122` [E]; `ProjectVariablesRegistryAccessTests.cs:202-212` [L] |

## 16. Preguntas que la Proposal debe cerrar

1. Representacion: mapa `string→string`, registros con id estable o documentos tipados.
2. Identidad de una propiedad y politica de nombres (vacios, repetidos, mayusculas).
3. Alcances y si existe herencia o fallback entre ellos.
4. Solo texto o tipos; y como evolucionar sin romper builds ya desplegados.
5. Contenedor de proyecto: dentro de `RACKCAD_PROJECT`, entrada nueva del NOD u otro.
6. Contenedor de rack: sobre, diseño, diccionario de extension, NOD por `RackId` o referencia.
7. Si el campo del sobre es tipado o aislado, y como lo trata `Compose`.
8. Autoridad entre hermanas: comparacion, divergencia, pertenencia indeterminada y reparacion.
9. Renombrar, borrar y vaciar.
10. Duplicacion: ids, valores, independencia y prueba contractual.
11. Biblioteca y exportaciones.
12. Malformado, major futuro, y aislamiento respecto del rack.
13. Cross-DWG.
14. Puntos de extension futuros (dibujo, expresiones, plantillas) sin implementarlos.
15. UI minima reutilizable, comando y censos.
16. ADR si/no.
