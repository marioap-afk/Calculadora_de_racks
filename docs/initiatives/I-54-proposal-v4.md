# I-54 — Proposal V4: Custom Properties Foundation (ID24)

```text
Documento      = PROPOSAL V4 — NOT CONSENSUS
Gate           = G2G — rebase obligatorio y reconciliacion final de la Architect Review exact-SHA de V3
Executor       = PROPOSED V4
Coordinator    = NOT YET AGREED ON V4
Architect      = NOT YET REVIEWED ON V4
Owner          = NOT ASKED
Consensus      = NOT REACHED
Implementation = BLOCKED
ADR            = REQUERIDO; alcance candidato en §15, AGREED en G2F y sin cambios; sin numero y sin texto
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base del Discovery; la evidencia de codigo se cita aqui)
POST_REBASE_BASE_SHA      = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (origin/main usado en el rebase de G2G)
REVIEW_V1      = Architect Review — I-54 Proposal V1 @ 7c197af (pre-rebase): AGREED WITH CHANGES (V2-1..V2-22)
REVIEW_V2      = Architect Review — I-54 Proposal V2 @ 36c337b (pre-rebase): AGREED WITH CHANGES (C-1..C-8)
REVIEW_V3      = Architect Review — I-54 Proposal V3 @ PRE_REBASE_V3_SHA 5d25da89972df2468f1d03243301761f5463e6eb
                 AGREED WITH CHANGES · G2F CLOSED · solo AR-54-V3-01 y AR-54-V3-02 (MINOR)
                 El Coordinador acepta la lista cerrada de cambios de G2F
REBASED_V3_EQUIVALENT_SHA = ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed   (V3 byte a byte identica a la revisada: §2.3)
PROPOSAL_V4    = el commit que introduce este archivo (como obtenerlo: paquete §1)
Historicas     = V1, V2 y V3 no se editan; mapa de SHAs pre y post rebase en §2.2
Discovery      = I-54-discovery.md, sin cambios en G2G (la medicion de G2G esta en §2 y §13)
Paquete        = I-54-architect-review-package.md, reescrito para revisar ESTA version: solo C-F1, C-F2 y el rebase
```

> Esta V4 **no autoriza** nada y **sustituye a V3** como propuesta vigente. V1, V2 y V3 quedan en el arbol como
> historia y no se modifican. V4 es la **V3 rebasada**, byte a byte identica a la que reviso G2F, con **exactamente
> dos** correcciones: **C-F1** (AR-54-V3-01) y **C-F2** (AR-54-V3-02). Todo lo demas es identico a V3 y quedo
> acordado en G2F. Cada punto modificado cita su cambio (**C-F1** o **C-F2**). El consenso exige
> `Coordinator = AGREED` y `Architect = AGREED` sobre el **mismo** SHA de este documento, el ADR en estado
> `propuesto` y la aprobacion del Owner.

**Marcas de evidencia.** **[E]** leido en esta sesion sobre `BASE_SHA`, o sobre el SHA de rama que se cite.
**[L]** lectura de auditoria con cita, no re-verificada linea a linea. **[I]** inferencia no ejecutada.
**[X]** ejecutado en una sonda local de `System.Text.Json` sobre .NET 8.0.29, **fuera del repositorio**, con
copias literales de `RackEmbedDocument.cs`, `RackEmbedComposer.cs` y `SchemaVersionPolicy.cs` de `BASE_SHA`,
del sobre anterior a I-11 (`4b2f4d7^`) y de la variante con el miembro propuesto. Los casos y resultados estan
en el paquete §5, para reproducirlos. `[D §n]` remite al Discovery.

## 0. Que cambia respecto de V3

El delta V2 → V3 (C-1..C-8) sigue en V3 §0 y quedo acordado en G2F. Este es el delta V3 → V4, y **no hay ningun
otro**:

| Cambio | Tema | V3 | V4 | Cierra |
|---|---|---|---|---|
| **C-F1** | `Name` no normalizable | Validar UTF-16 bien formado antes de NFC se daba por suficiente | Un `Name` con un **no-caracter Unicode** es invalido: intent invalido al escribir y `PresentButUnreadable` al leer, siempre **antes** de `Normalize` o `IsNormalized`. `Value` no cambia | AR-54-V3-01; AR-54-V2-03 (3B) |
| **C-F2** | Reescritura de un sobre con surrogate escapado | Solo se declaraba el residual del UTF-16 crudo; INV-07 y D-13 prometian «ningun efecto» | Residual preexistente **F-14b** declarado junto a **F-14a**; INV-07, D-13 y RP-11 acotados a sobres que BASE puede leer **y reserializar**; fallo sin commit en D-22.10; caracterizacion en T-CHR-04 y T-ENV-13 | AR-54-V3-02; AR-54-V2-03 (3C) |
| **Trazas** | Revision y paquete (cambio 3 de G2F) | Matrices de V3 | §0, §1, §2, §3, §10, §12, §13, §14 y §17..§22 actualizados; paquete limitado a C-F1, C-F2 y el rebase; contrato | — |

El rebase de G2G **no cambia ninguna decision**: cambia la base y los SHAs, no el contenido (§2).

## 1. Vocabulario

| Termino | Significado en V4 |
|---|---|
| **Propiedad personalizada** | Entrada `{ Id, Name, Value }` definida por el usuario. No es variable de proyecto, geometria ni BOM |
| **Coleccion** | Documento de propiedades: `SchemaVersion` y la lista **ordenada** `Entries` |
| **Alcance Proyecto** | Una coleccion por `Database` de AutoCAD (un archivo DWG), con metadatos **administrativos o comerciales** del proyecto que ese archivo representa. No es metadato de dibujo u hoja, rotulacion, layout ni vista, ni autoridad de un proyecto de varios DWG (D-03) |
| **Alcance Rack** | Una coleccion por `RackId`, replicada en el sobre de cada miembro |
| **Miembro de un RackId** | Definicion de bloque **no dependiente de xref** cuyo sobre es legible y cuyo `Id`, no vacio, es igual al `RackId` sin distinguir mayusculas (ADR-0009 `:42-45`) |
| **Payload RackCad no interpretable** | Definicion que `RackBlockFinder.ScanEnvelopes` devuelve con `Embed = null`: el Xrecord `RACKCAD_SELECTIVE` trae texto, pero `RackEmbedStore.Deserialize` no lo lee (JSON invalido, truncado, anidado excesivo o major futuro) |
| **Sobre** | `RackEmbedDocument`, en el diccionario de extension de la definicion [D §5.2] |
| **Contenedor** | Entrada `RACKCAD_CUSTOM_PROPERTIES` del NOD (Proyecto) o miembro `CustomProperties` del sobre (Rack) |
| **Forma canonica** | Documento parseado, comparado segun D-09.7 |
| **Estado no escribible** | Resultado del store `PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` o `DepthLimitExceeded` (C-2) |
| **Profundidad** | Niveles de contenedores JSON: el objeto raiz cuenta 1 y cada objeto o array anidado suma 1. La cota del formato 1.x es **16** (D-05.7) |
| **Kind conocido** | `Kind` registrado en este build, segun el predicado `isKnownKind` que el Plugin inyecta en Application (D-09.5, C-1) |
| **UTF-16 bien formado** | Texto sin surrogates sueltos: cada surrogate alto va seguido de uno bajo (D-05.1, C-3). Es necesario, pero **no suficiente**, para `Normalize(FormC)` (P-35, C-F1) |
| **No-caracter Unicode** | U+FDD0–U+FDEF y todo code point cuyos 16 bits bajos son FFFE o FFFF, en cualquier plano: U+FFFE, U+FFFF, U+1FFFE, U+1FFFF, …, U+10FFFE, U+10FFFF (66 en total). Un `Name` que contiene uno es invalido; un `Value`, no (D-05.1, D-05.2, D-05.4, C-F1) |
| **Name acreditado** | `Name` que paso la validacion de escritura de D-05.1 o la de lectura de D-05.2: string decodificable, UTF-16 bien formado y sin no-caracteres. Solo un `Name` acreditado llega a `Normalize` o `IsNormalized` (C-F1) |

## 2. Estado verificado en G2G

### 2.1 Preflight y rebase

```text
Fecha                = 2026-09-12 (preflight y rebase) y 2026-09-13 (re-medicion antes de publicar), con git fetch --all --prune
Rama I-54            = architecture/propiedades-personalizadas; al abrir, HEAD = upstream = 5d25da8; 0/0; arbol limpio
Stash / operaciones  = ninguno; sin MERGE_HEAD, REBASE_HEAD, CHERRY_PICK_HEAD, REVERT_HEAD, BISECT_LOG, sequencer ni rebase-*
Worktrees            = main (f8deb67), I-49, I-52, I-53 e I-54, cada uno en su rama
WORKFLOW en main     = sin cambios desde BASE_SHA: §4.2 rebase al abrir; §4.3 publicar con --force-with-lease
origin/main          = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (21 commits por delante de BASE_SHA: I-50 y su merge)
PRE_REBASE_HEAD      = 5d25da89972df2468f1d03243301761f5463e6eb
Rebase               = git rebase origin/main: 6/6 commits, sin conflictos
POST_REBASE_HEAD     = ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed   (antes de crear V4)
```

### 2.2 Mapa de SHAs pre y post rebase

| Artefacto | Pre-rebase reviewed/published SHA | Post-rebase equivalent SHA | `git range-diff` |
|---|---|---|---|
| CLAIM | `143490d8ecfb1c5f3011cf752c5cfcd11af13784` | `c60c17fa46ef1eb2e9ab696432721d7649fe7c1d` | `=`; sigue vacio, con su `Claim-Id` |
| BOOTSTRAP | `f908b2f4ba508bf365e55adbda78cd4eac505295` | `1c2d17b3de233738d646d82811ddecd8ed673611` | `!` solo por contexto de `ROADMAP.md`: la fila de I-50 que trae `main` queda junto a la de I-54, y la linea añadida de I-54 es identica |
| G1 | `195964b00006d2be74eefb8b6ae4f5f46de1cfc9` | `97e27ea7c50289460a2c69be0e70339c3aa674d0` | `=` |
| V1 | `7c197af81b91df88366c873eddf9e11ddc5e87bb` | `728d11f09e727adda3cf993fd39601fc18614ba6` | `=` |
| V2 | `36c337b84c47f9ac7c97d860fce42d3a5f4370e8` | `2d0d6665ff286a97f25024e5fa800532cf96f624` | `=` |
| V3 | `5d25da89972df2468f1d03243301761f5463e6eb` | `ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed` | `=` |

Los seis conservan mensaje, trailers y fecha de autor. **Los SHAs historicos no se sustituyen**: cada revision
pertenece a su SHA pre-rebase. La Architect Review V3 reviso `PRE_REBASE_V3_SHA = 5d25da8…`, y
`REBASED_V3_EQUIVALENT_SHA = ff98b9e…` es su equivalente, no un SHA revisado.

### 2.3 Integridad del rebase

- **A. Contenido V3 post-rebase verificado equivalente al contenido revisado pre-rebase; nueva revision exact-SHA
  sigue siendo obligatoria.** `docs/initiatives/I-54-proposal-v3.md` tiene el mismo blob (`eb0de53…`) en
  `5d25da8` y en `ff98b9e`, y V4 se creo como copia de ese archivo antes de aplicar C-F1 y C-F2.
- **B.** El paquete de V3, el contrato, el Discovery, V1 y V2 tambien conservan su blob exacto.
- **C.** Frente a `main`, el V3 rebasado solo añade los seis documentos de I-54 y la fila de I-54 en `ROADMAP.md`.
  Cada archivo que cambio `main` tiene el mismo contenido en `main` y en el V3 rebasado.
- **D.** Ningun archivo de `src/`, `tests/`, `assets/`, `eng/`, `deploy/` ni `.github/` entra por I-54, y
  `docs/HANDOFF.md` no cambia.
- **E.** Ningun archivo de codigo del mapa de I-54 (§11) cambia entre `BASE_SHA` y `POST_REBASE_BASE_SHA`, y
  `src/` sigue con un solo `new RackEmbedDocument`, 7 llamadas a `Compose` y 33 `[CommandMethod(`. La evidencia de
  codigo se sigue citando sobre `BASE_SHA`, y en esos archivos es la misma en `main`.

Consecuencias que se mantienen de V3: la guarda T-GRD-01 se limita a `src/`; el espejo de I-52 cumple D-21 y
añadira una llamada a `Compose` (T-GRD-02: 7 → 8); el censo de comandos cambiara con I-52 (33 → 34). ADR-0035 esta
aceptado en `main`; 0036 (I-52) y 0037 (I-53, aceptado en su rama) estan tomados. Las ramas paralelas se miden en
§13.

## 3. Premisas verificadas

| # | Premisa | Fuente |
|---|---|---|
| P-01 | No existe metadato descriptivo de proyecto ni mapa clave→valor editable; el unico metadato de rack es `Name` | [D §3] |
| P-02 | `RACKCAD_PROJECT` es un Xrecord directo del NOD. Una entrada que no sea Xrecord se lee como `PresentButUnreadable` y bloquea tres comandos | `ProjectVariablesData.cs:36,59-68` [E]; [D §4.6] |
| P-03 | `ProjectVariablesDocument` es la unica autoridad de variables, con versionado propio | [D §4.3]; ADR-0034 §1, §13 |
| P-04 | `Compose` crea un sobre nuevo y del origen solo copia `SchemaVersion` y `ExtensionData` | `RackEmbedComposer.cs:21-33` [E] |
| P-05 | `RackEmbedStore.Deserialize` devuelve `null` ante `JsonException` o major futuro: un miembro **tipado** con contenido inesperado dejaria el rack entero ilegible | `RackEmbedDocument.cs:86-119` [E]; P-17 |
| P-06 | Ninguna autoridad compara campos del sobre entre hermanas; `SelectiveAuthoredAuthority` solo ve el diseño interior | [D §6] |
| P-07 | `RACKEDITAR` compone cada hermana con su propio sobre, en una transaccion por vista; el ejecutor de variables usa una sola | [D §7.2] |
| P-08 | `RestampEnvelope` deserializa, muta `Id`, `Name` y `Design` y serializa el **mismo** objeto. `RackCloner` crea un BTR nuevo y escribe solo el payload. Ninguna suite ejecuta ese codigo | `RackEnvelopeRestamp.cs:52-88`; `RackCloner.cs:34-36,66` [E] |
| P-09 | Ningun sistema exporta campos del sobre a `.rackcad.json`; importar acuña GUID nuevo y compone con `source = null` | [D §9] |
| P-10 | El NOD no viaja con un rack copiado a otro dibujo; el sobre si | [D §4.8, §9.4] |
| P-11 | `RackEmbedStore` escribe los nulos: no fija `DefaultIgnoreCondition` | `RackEmbedDocument.cs:70-74` [E]; [X] |
| P-12 | Hay guardas ordinales que prohiben `PropertyValues` (Domain, Plugin, UI) y `rackProperty` y `RackPropertyReference` (Application, Plugin, UI) | `ProjectVariablesConformanceTests.cs:114,129-135,162` [E] |
| P-13 | I-49 reserva para ID20 los nombres `Rack` y `Project`, sus namespaces y el ambito `Rack` | `I-49-proposal-v4.md:628-633,1737-1745` @ `ccf21c6` [E] |
| P-14 | Ningun archivo productivo del mapa de I-54 lo tocan I-49, I-50, I-52 ni I-53. I-52 reutilizara `RestampEnvelope` despues de reflejar y debe conservar las propiedades de rack | §13 |
| P-15 | Existe un patron CRUD reutilizable, el de `RACKVARIABLES`: leer → proyectar → ventana → intent por id → preflight → ejecutor → releer | `RackVariablesCommands.cs:47-82,101-125` [E] |
| P-16 | El texto de ID24 no esta versionado; el alcance lo fijan el contrato y el objetivo del Coordinador | [D §2.4] |
| P-17 | Comportamiento de `JsonElement?` dentro del sobre [X]. (1) Acepta objeto, string, numero, array y booleano, y reemite el texto exacto. (2) Un `null` literal da `HasValue = false` y el miembro se elimina al reescribir. (3) Sin el miembro, los bytes son identicos a BASE, con y sin `ExtensionData` y tambien con `Compose(null)`. (4) Una corrupcion sintactica o un truncado dentro del miembro, o 64 niveles de anidado dentro de el, dejan el sobre en `null` en BASE y en la variante; 63 niveles se leen. (5) `default(JsonElement)` hace lanzar `InvalidOperationException` a `Serialize`. (6) Un elemento de tipo `Null` asignado escribe `"CustomProperties":null`. (7) Con la clave exacta del miembro repetida gana la ultima (con variantes de capitalizacion, ver P-33). (8) Con el miembro y una clave igual en `ExtensionData` se emiten dos claves, y al releer gana la de `ExtensionData`. (9) Una clave con otra capitalizacion se asigna al miembro. (10) Un miembro `string` que recibe un objeto lanza `JsonException` | sonda |
| P-18 | [X] Un build anterior a I-11 lee un sobre major 2 como legible y **pierde** el miembro al reescribir. I-11..I-53 conservan el miembro exacto, con `Compose(source)` y con el restamp del mismo objeto, y tratan un major 2 como ilegible | sonda; `git show 4b2f4d7^:src/RackCad.Application/Persistence/RackEmbedDocument.cs` [E] |
| P-19 | [X] `Guid.TryParseExact(texto, "D")` acepta espacios o tabulador alrededor y acepta `Guid.Empty`; rechaza llaves y el formato `N`. `Guid.ToString("D")` escribe 36 caracteres en minusculas. «Área» en NFC y en NFD no son iguales con `OrdinalIgnoreCase` hasta normalizar a `FormC` | sonda |
| P-20 | [X] `JsonElement` conserva los nombres de miembro repetidos: `EnumerateObject` los devuelve todos y `TryGetProperty` devuelve el ultimo | sonda |
| P-21 | En Project Variables, **toda** definicion con datos RackCad no interpretables aborta la operacion, colocada o no, porque «sin su identidad» no se puede demostrar que no sea hermana. Su proyeccion trata un `Id` o un `Kind` en blanco como sobre ilegible y filtra por Selectivo | `ProjectVariableConsumerDiscovery.cs:87-91,161-216`; `ProjectVariableScanProjection.cs:37-46` [E] |
| P-22 | `ScanEnvelopes` corre en la transaccion del llamador; omite layouts, anonimos y `IsFromExternalReference`; conserva los ilegibles con `Embed = null` y cuenta las referencias desde el registro. `FindRackBlocks` abre su propio bloqueo y su propia transaccion, y descarta los ilegibles | `RackBlockFinder.cs:57-91`; `RackCommandSupport.cs:109-134` [E] |
| P-23 | `Kind` existe en el sobre desde que este nacio (`74d935f`, 2026-07-08), y los siete llamadores de `Compose` pasan un kind constante. Antes, el Selectivo escribia bajo la misma clave `RACKCAD_SELECTIVE` el `SelectivePalletDesignDocument` desnudo (`aad0c5e`..`74d935f^`: `SchemaVersion`, `Id` y `Name`, sin `Kind` ni `Design`). El store de BASE lo lee como sobre **legible** con `Kind` y `Design` nulos [X] | `git show 74d935f`; `74d935f^:src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs:16-22` [E]; sonda |
| P-24 | `Kind` en blanco hoy. Lo tolera `FindRackBlocks`, que busca por GUID. Lo rechazan la proyeccion de variables, `RackDuplicationPlan` («no declara tipo de rack»), los preflights de Push Back y Cantilever, `RACKLISTA`, el inventario y `RACKEDITAR` sobre el propio bloque | `RackCommandSupport.cs:120-127`; `ProjectVariableScanProjection.cs:37-42`; `RackDuplicationPlan.cs:412-415`; `RackPushBackCommands.cs:194-203`; `RackCantileverCommands.cs:243-252`; `RackListBuilder.cs:56`; `RackInventarioCommands.cs:53`; `RackMenuCommands.cs:132-146` [E] |
| P-25 | En `src/`, el unico `new RackEmbedDocument` esta en `RackEmbedComposer.cs:24`. En `tests/` construyen sobres ocho archivos de `BASE_SHA` y el `DimensionViewsRestampTests.cs` de I-50 | `git grep` [E] |
| P-26 | El ejecutor de variables relee y re-acredita en Application dentro de su transaccion unica, sin replanificar. Una guarda fija que compone con el sobre de cada hermana | `ProjectVariableMutationExecutor.cs:250-288`; `ProjectVariableMutationExecutorTests.cs:288-298` [E] |
| P-27 | G-R5 fija la forma del restamp con los ayudantes `PluginSourceCode`: una sola asignacion `.Id =`, un solo `NewGuid(`, ningun `catch (` y `TryGetIgnoreCase(` | `SelectiveDuplicationFailClosedTests.cs:474-526` [E] |
| P-28 | **Todo camino que hoy reescribe un sobre exige un kind conocido** (C-1): `RACKEDITAR` resuelve el handler antes de editar; `RACKDUPLICAR` inyecta `isKnownKind` en el plan puro y declara inutilizable un kind no reconocido; `RACKLAYOUT` resuelve el handler antes de copiar; el restamp lanza con un kind sin handler; el ejecutor de variables solo compone `KindSelective`. La doctrina es «tipo no reconocido = error visible» | `RackMenuCommands.cs:141`; `RackDuplicarCommands.cs:69`; `RackDuplicationPlan.cs:225-228,417-420`; `RackLayoutCommands.cs:64`; `RackEnvelopeRestamp.cs:105-108`; `ProjectVariableMutationExecutor.cs:208-210`; `KindHandlerDispatch.cs:17-29` [E] |
| P-29 | **Un kind nuevo entra sin tocar la version del sobre**: `61cbff1` añadio `KindPushBack` sin cambiar `CurrentSchemaVersion`, asi que un build anterior ve ese kind en un sobre perfectamente legible | `git show 61cbff1 -- src/RackCad.Application/Persistence/RackEmbedDocument.cs` [E] |
| P-30 | [X] **Profundidad**: un documento 1.3 conforme salvo por un campo informativo a profundidad 20 se parsea sin error; con la regla de V2 se leia `Readable` y rechazaba toda escritura, incluida la eliminacion | sonda de G2D |
| P-31 | [X] **UTF-16 mal formado al leer**: `JsonDocument.Parse(string)` con un surrogate suelto crudo lanza `ArgumentException` (no `JsonException`); `JsonProperty.Name`, `JsonElement.GetString()` y `JsonElement.ValueEquals` sobre un surrogate suelto **escapado** lanzan `InvalidOperationException`, tambien anidado dentro de un campo desconocido; `GetRawText()` no lanza; un par de surrogates escapado valido se decodifica bien. El `RackEmbedStore.Deserialize` de BASE solo captura `JsonException` y lanza `ArgumentException` con UTF-16 crudo invalido; con un surrogate escapado en un miembro declarado devuelve `null`, y dentro de `CustomProperties` el sobre sigue legible. Su **reescritura**, en cambio, lanza (P-36) | sondas de G2D y G2E; `RackEmbedDocument.cs:98` [E] |
| P-32 | [X] **UTF-16 mal formado al escribir**: `string.Normalize(FormC)` lanza `ArgumentException` con un surrogate suelto; `JsonSerializer` y `JsonNode` escriben un surrogate suelto como `U+FFFD`, de modo que el valor releido no es el recibido. Recorrer el texto con `Rune.DecodeFromUtf16` distingue el texto bien formado del mal formado. La buena formacion es **necesaria pero no suficiente** para NFC (P-35) | sondas de G2D y G2E |
| P-33 | [X] **Nombres repetidos**: con `{"a":1,"a":2}` frente a `{"a":2}`, un mapa «gana el ultimo» los da por iguales y uno «gana el primero» por distintos. En el sobre, con `CustomProperties` y `customproperties` a la vez, BASE conserva ambas claves en `ExtensionData` y las reemite, y la variante con el miembro declarado se queda con la ultima | sonda de G2D |
| P-34 | [X] El matching sin mayusculas de `System.Text.Json` coincide con `OrdinalIgnoreCase` en los casos limite probados (ı, İ, ſ, K Kelvin, ancho completo, espacio de ancho cero): el store no puede producir una clave de `ExtensionData` que colisione con un miembro declarado | sonda de G2D |
| P-35 | [X] **No-caracteres y NFC** (C-F1). Con texto bien formado, `Normalize(FormC)` e `IsNormalized(FormC)` lanzan `ArgumentException`: con ICU, el modo por defecto (`UseNls = False`), para U+FFFE; con NLS (`DOTNET_SYSTEM_GLOBALIZATION_USENLS=1`), para U+FFFE, U+FFFF, U+FDD0, U+1FFFE y U+10FFFF. En ninguno de los dos modos lanzan U+0378 (sin asignar), U+E000 (uso privado), U+0000 ni «Área». El escape JSON de U+FFFE se decodifica con `GetString()` sin excepcion, y el serializador conserva U+FFFE exacto en ida y vuelta | sonda de G2F |
| P-36 | [X] **Reescritura con surrogate escapado** (C-F2). Un sobre con el escape JSON de un surrogate suelto en un string o en un nombre de propiedad, dentro de `CustomProperties` o en un campo desconocido, **se lee** en BASE y en la variante. En cambio, `Serialize` del mismo objeto, `Compose(source)` seguido de `Serialize` y la mutacion del mismo objeto seguida de `Serialize` **lanzan** `JsonException` («The object or value could not be serialized»), porque `JsonElement.WriteTo` lanza `InvalidOperationException`. Con un par de surrogates valido escapado, las tres reescrituras funcionan. `RackEmbedStore.Serialize` no captura nada | sonda de G2F; `RackEmbedDocument.cs:76-84` [E] |

## 4. Objetivos y no-objetivos

**Objetivos**

- O-01. Metadatos definidos por el usuario en dos alcances, Proyecto y Rack, persistidos en el DWG.
- O-02. Identidad estable por entrada, independiente del nombre.
- O-03. Mismos metadatos en todos los miembros de un rack, sin divergencia silenciosa y sin elegir hermana.
- O-04. Supervivencia a guardar y reabrir, a `RACKEDITAR` de los seis sistemas, a vistas nuevas, a la
  propagacion de variables, a `RACKDUPLICAR` y a `RACKLAYOUT`.
- O-05. Fallo cerrado ante contenido ilegible, ambiguo o futuro: se **conserva** el dato y se **bloquea** la
  escritura. El estado de las propiedades no cambia el resto del rack **dentro del alcance verificado de D-08**.
- O-06. Una identidad estable que un consumidor futuro pueda referenciar, sin fijar nada de ese consumidor.
- O-07. Una UI minima y reutilizable para los dos alcances.

**No-objetivos**

- N-01. Tipos (numero, booleano), unidades, formatos y listas de valores.
- N-02. Herencia o valor por defecto entre Proyecto y Rack.
- N-03. Propiedades por vista, por referencia o por instalacion.
- N-04. Dibujar propiedades, columnas en BOM o en `RACKLISTA`, y campos de AutoCAD.
- N-05. Expresiones, parser, formulas y referencias entre racks (ID20, ID21, ID22B).
- N-06. Plantillas de propiedades.
- N-07. Exportar o importar propiedades por la biblioteca.
- N-08. Transferir o fusionar propiedades de proyecto entre dibujos.
- N-09. Editar propiedades desde los editores de sistema.
- N-10. Cambiar el formato de `RACKCAD_PROJECT`, `ProjectVariablesData`, `ProjectVariablesDocument`,
  `RackBlockData`, `RackBlockFinder`, `RackEnvelopeRestamp`, `RackCloner` o los flujos `Edit*`.
- N-11. **Recuperacion destructiva** de contenedores no escribibles: descartar, reparar o sobrescribir. Queda
  fuera de I-54 y exige una decision explicita del Owner en otra iniciativa (V2-1).
- N-12. Claves de agregacion entre racks, por nombre o por id (V2-12).
- N-13. Metadatos de dibujo, hoja, rotulacion, layout o de proyectos de varios DWG (V2-19).
- N-14. Fijar semantica de plantillas, sintaxis de expresiones o formato de rotulacion (V2-17).

## 5. Representacion

| | **A** — `Dictionary<string,string>` | **B** — registros con id estable | **C** — documentos tipados |
|---|---|---|---|
| Identidad | el nombre | GUID por entrada | GUID por entrada |
| Renombrar | borrar y crear: cambia la clave | cambia `Name`, el id se conserva | el id se conserva |
| Referencia futura | por nombre: se rompe al renombrar | por id | por id |
| Evolucion por entrada | ninguna sin cambiar la forma | `ExtensionData` por entrada | idem, mas un discriminador que hay que hacer cumplir |
| Riesgo en builds desplegados | bajo | bajo | alto: un valor no-string haria fallar a los lectores de texto |
| Precedente | rechazado en I-47 (A4) | `VariableId` + `Name` de I-47 | `VariableType` con un solo tipo real, y la deuda de `ToProjectVariables()` |

**Decision: B, con valor solo texto en V1.** A queda como proyeccion posible, nunca como forma persistida. C
se rechaza para V1 porque introduce tipos sin consumidor. La confrontacion con ADR-0034 §3 esta en D-04.

## 6. Decisiones

Formato: **Decision** · **Cambio desde V1** · **Por que y evidencia** · **Descartado**. Las **MATERIAL** cambian
formato persistido, autoridad o superficie.

### D-01 — Modelo persistido (MATERIAL)

**Decision.** Una sola forma para los dos alcances:

```json
{
  "SchemaVersion": "1.0",
  "Entries": [
    { "Id": "0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10", "Name": "Cliente", "Value": "ACME" },
    { "Id": "5d1c2a90-7b44-4f0e-8a3b-0c9e6d2f1a77", "Name": "Notas", "Value": "Linea 1\nLinea 2" }
  ]
}
```

1. `SchemaVersion` **sin inicializador** en el DTO (C4.8-1 de I-47): ausente no equivale a `"1.0"`. Al leer
   se exige texto con la forma exacta `major.minor` (dos enteros no negativos separados por un punto, sin
   espacios) y `major ≥ 1`.
2. `Entries` **obligatorio**: un array, que puede estar vacio.
3. `[JsonExtensionData]` en la raiz y en cada entrada. Nunca contiene un nombre igual, con `OrdinalIgnoreCase`,
   a un miembro declarado de ese objeto (T-STO-13). La misma regla rige para el sobre (D-08.4, restriccion F).
4. **Todo escritor 1.x emite siempre `SchemaVersion` y `Entries`**, tambien con cero entradas (V2-9).
5. Los nombres de miembro JSON se escriben en PascalCase y se leen sin distinguir mayusculas. **Si cualquier
   objeto JSON del documento** —la raiz, cada entrada de `Entries`, `ExtensionData` de raiz o de entrada, los
   objetos anidados dentro de `ExtensionData`, los objetos dentro de arrays o cualquier otro— contiene dos
   nombres de miembro iguales con `OrdinalIgnoreCase`, el documento es `PresentButUnreadable` (C-4).
   - Se detecta **antes** de mapear a cualquier estructura que colapse claves, recorriendo `EnumerateObject` a
     cualquier profundidad (D-07.4).
   - Ni el store ni el comparador tienen semantica «gana el primero» o «gana el ultimo» [X P-20, P-33].
   - No confundir con dos **entradas** que comparten `Name`, que se leen con diagnostico (D-05.2).

**Cambio desde V1.** Puntos 4 y 5, y la forma exacta de `SchemaVersion`.

**Cambio desde V2 (C-4).** El punto 5 pasa de «la raiz o una entrada» a cualquier objeto y cualquier
profundidad.

**Por que.** Un solo tipo, un solo store, una sola validacion y una sola proyeccion de UI. Con el punto 4, un
build posterior 1.x nunca escribe un documento que I-54 lea como ilegible por falta de miembros obligatorios.

### D-02 — Identidad (MATERIAL · V2-10)

**Decision.**

1. **Forma persistida exacta**: 36 caracteres `8-4-4-4-12` de digitos hexadecimales, con guion en las
   posiciones 8, 13, 18 y 23. Sin espacios alrededor ni dentro, y sin llaves ni parentesis. Al leer se acepta
   el hexadecimal en cualquier caja.
2. Primero se comprueba la forma textual y **despues** se obtiene el valor con `Guid.TryParseExact(texto, "D")`.
   `TryParseExact` solo no basta: acepta espacios y tabuladores alrededor [X P-19].
3. Se escribe con `Guid.ToString("D")`: 36 caracteres en minusculas [X P-19], tambien al reescribir una entrada
   que se leyo en mayusculas. La identidad es el valor, asi que eso no cambia la igualdad canonica.
4. `Guid.Empty` → `PresentButUnreadable`.
5. **Igualdad por valor de `Guid`**, nunca por texto.
6. Dos entradas de la misma coleccion con el mismo valor de `Guid` → **`AmbiguousIdentity`**, un resultado
   **distinto** de `PresentButUnreadable`. Se evalua solo si el documento es valido en todo lo demas (D-07.4).
7. Application acuña el id al crear (`Guid.NewGuid()`); nunca se deriva del nombre.
8. La identidad es **local al alcance**: el mismo GUID puede aparecer en el proyecto y en muchos racks (las
   copias lo conservan, D-11), y **eso no significa nada en V1** (D-03.4).

**Cambio desde V1.** Puntos 1 a 3 y 6 (resultado distinto), y el 8 sin la «correlacion futura por plantilla».

### D-03 — Alcances (V2-19 · V2-12)

**Decision.**

1. Conjunto cerrado `{ Proyecto, Rack }`, sin herencia ni fallback entre ellos. Sin alcance de vista,
   referencia ni instalacion.
2. **Alcance Proyecto** = una coleccion por `Database` de AutoCAD (un archivo DWG), con metadatos
   administrativos o comerciales del proyecto que ese archivo representa (cliente, obra, ubicacion, revision…).
   **No significa**:
   - anotaciones de nivel dibujo;
   - metadatos de hoja, rotulacion o cuadro de titulo;
   - layouts;
   - metadatos de vista;
   - autoridad de un proyecto que abarque varios DWG.

   Un **Drawing-level** futuro sigue fuera de I-54, y nada de V2 lo presupone.
3. **Alcance Rack** = una coleccion por `RackId`, identica canonicamente en cada miembro (D-09).
4. **Sin agregacion entre racks.** V1 no define ninguna clave de agregacion. Ningun consumidor futuro puede
   inferir que «mismo `Name` = misma propiedad conceptual», ni lo mismo por un id repetido entre racks, sin un
   contrato posterior que lo decida. Ninguna API de V1 agrupa ni une colecciones de racks distintos.

**Cambio desde V1.** Los puntos 2 y 4 son nuevos; se retira la pregunta de D-03 sobre consistencia de nombres.

**Descartado.** Definiciones de proyecto con valores por rack: un rack copiado a otro dibujo llevaria valores
cuyas definiciones no existen alli (P-10), la clase de referencia rota que en I-47 obligo a `RepairBroken`.

### D-04 — Solo texto y regla de evolucion normativa (MATERIAL · V2-9)

**Decision.**

1. `Value` es **siempre** un string JSON, y puede estar vacio. Nulo, ausente o de otro tipo JSON →
   `PresentButUnreadable`.
2. No hay campo de tipo en V1.
3. **Regla de evolucion, contractual para el formato:**

   | Cambio | Version |
   |---|---|
   | Cambiar el significado de `Value` | **MAJOR** |
   | Cambiar la validez de `Value` en lectura | **MAJOR** |
   | Cambiar el tipo JSON de `Value` | **MAJOR** |
   | Cambiar la estructura de una entrada (miembros obligatorios, su forma o su significado) | **MAJOR** |
   | Cambiar la estructura de la coleccion (raiz, `Entries`, orden, identidad) | **MAJOR** |
   | Cambiar la cota de profundidad del documento (16, D-05.7) | **MAJOR** (C-2) |
   | Hacer que el significado o la autoridad de la coleccion dependan del `Kind` del rack | **MAJOR** (C-1) |
   | Añadir un campo **informativo** que un lector de minor menor puede ignorar y conservar sin dejar de ser correcto | MINOR |
   | Ajustar los limites de **escritura** de conteo y longitud de D-05 (no son reglas de lectura) | ninguna |

4. Todo escritor 1.x emite siempre `SchemaVersion` y `Entries` (D-01.4).
5. **Nada que un build posterior pueda interpretar validamente se considera destructible.** V1 no tiene
   recuperacion destructiva (D-13) y la unificacion no sobrescribe contenido desconocido (D-09.10).

**Confrontacion con ADR-0034 §3.** ADR-0034 §3 (`:74-78`) persiste el discriminador de tipo «desde el inicio»
porque su consumidor interpreta el valor numericamente y añadirlo despues «romperia el esquema». En V1 de I-54
no hay ningun consumidor que interprete `Value`. La proteccion que §3 busca —que un build viejo no escriba un
valor invalido en una propiedad que un build nuevo considera tipada— la da el **major del documento**:

- introducir tipos es MAJOR (punto 3);
- un build 1.x ve un documento 2.x como `IncompatibleMajor`, bloquea la escritura y conserva los bytes (D-13);
- el sobre no se promociona (D-08), asi que el rack sigue legible y solo la coleccion queda en solo lectura.

Un `Type` con un unico valor seria una promesa sin comprobacion: exactamente la deuda de `ToProjectVariables()`,
que ignoraba el tipo persistido. La diferencia con §3 se declara, no se oculta: el ADR de I-54 debe citarla.

**Cambio desde V1.** La regla pasa de principio a tabla contractual, con los puntos 4 y 5 y la confrontacion.

**Cambio desde V2 (C-1, C-2).** Dos filas MAJOR nuevas: la cota de profundidad y la independencia respecto del
`Kind`. La fila de limites de escritura se precisa a conteo y longitud.

### D-05 — Nombres, valores, limites y profundidad (V2-11 · V2-5)

**Decision.**

1. **Nombre al escribir** (nuevo o renombrado), en este orden:
   1. **validar que es UTF-16 bien formado** (C-3);
   2. **recorrer sus valores escalares Unicode (`Rune`) y rechazar cualquier no-caracter** (C-F1);
   3. recien entonces, normalizar a Unicode **NFC** (`NormalizationForm.FormC`);
   4. recortar;
   5. validar 1..80 caracteres UTF-16, sin caracteres de control (U+0000–U+001F, U+007F);
   6. comprobar unicidad frente a las **otras** entradas, comparando con `OrdinalIgnoreCase` sobre la forma NFC
      de ambos lados. Los nombres de las otras entradas ya estan acreditados (punto 2).

   Si falla el paso 1 o el 2, el intent es invalido y el preflight lo rechaza con un resultado tipado, **nunca** con
   una excepcion. Los dos pasos son un unico recorrido con `Rune.DecodeFromUtf16`. `Normalize(FormC)` lanza
   `ArgumentException` con un surrogate suelto [X P-32] y tambien con no-caracteres bien formados [X P-35], asi que
   solo recibe nombres acreditados. Se persiste la forma NFC recortada [X P-19].
2. **Nombre al leer**: debe ser un string decodificable (D-07.4). Un `Name` que contiene un no-caracter →
   `PresentButUnreadable` (C-F1); vacio tras recortar → `PresentButUnreadable`. Esa acreditacion ocurre **antes**
   de cualquier `Normalize` o `IsNormalized`: el diagnostico de repetidos del store, la unicidad del punto 1 y la
   marca `NombreRepetido` del workspace (D-18.3) solo normalizan nombres acreditados. Los nombres que no se tocan
   **no** se normalizan ni se reescriben. Los repetidos (NFC + `OrdinalIgnoreCase`) → `Readable` con diagnostico.

   **Frontera de excepciones de NFC (C-F1).** Ninguna llamada de I-54 a `Normalize` o `IsNormalized` deja escapar
   `ArgumentException`, y nunca se usa `catch (Exception)`. La garantia es la precondicion: solo se normaliza un
   `Name` acreditado, y con esa precondicion no lanza ninguno de los casos ejecutados, ni con ICU ni con NLS
   [X P-35]. Como defensa ante normalizadores de plataforma mas estrictos, esa unica llamada puede capturar
   `ArgumentException`: al escribir, el intent es invalido; al leer, el diagnostico de repetidos compara ese nombre
   sin normalizar y la clasificacion no cambia. No oculta un error de programacion: la forma es una constante, el
   argumento es un string ya acreditado y la unica causa de `ArgumentException` en esa llamada es un code point que
   el normalizador de la plataforma rechaza.
3. **El nombre solo presenta.** Ninguna API de Application resuelve, busca ni une una propiedad por `Name`. Los
   intents y las referencias usan el id.
4. **Valor al escribir**: primero se valida que es **UTF-16 bien formado** (C-3); si no, el intent es invalido.
   Despues, 0..1000 caracteres UTF-16; se admiten CR, LF y tabulador y ningun otro caracter de control. Se guarda
   tal como llega, sin normalizar: como el texto mal formado se rechaza antes, el serializador **nunca** puede
   sustituir en silencio un surrogate suelto por `U+FFFD` [X P-32]. **Al leer** basta con que sea un string
   decodificable. La regla de no-caracteres aplica **solo** a `Name`: `Value` no se normaliza, y un `Value` bien
   formado con un no-caracter (por ejemplo U+FFFE) es valido y se relee exacto [X P-35] (C-F1).
5. **Coleccion**: como maximo 50 entradas **al crear**.
6. **Los limites de escritura validan lo que la operacion introduce**:
   - crear: nombre, valor y conteo resultante;
   - renombrar: el nombre nuevo;
   - cambiar valor: el valor nuevo;
   - eliminar: **siempre se permite frente a los limites de conteo, longitud de nombre y longitud de valor**
     (C-2). Asi se puede eliminar una entrada que ya los excede.

   Las entradas que la operacion no toca **no** se revalidan contra tamaño ni conteo: pueden venir de un build
   con otros limites, y leer nunca bloquea por tamaño.

   **Ninguna** operacion, eliminar incluida, se permite sobre una coleccion o un rack en estado no escribible:
   `PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor`, `DepthLimitExceeded`, `UnknownKind`,
   `MixedKind`, `IndeterminateMembership`, `NoIdentity`, `XrefRejected` o `Divergent` (salvo unificar, D-09.10).
7. **Cota de profundidad = 16, constante del formato 1.x** (C-2). No es tuning ni un limite de UX configurable, y
   no pertenece a OQ-02.
   - **Todo escritor 1.x** produce documentos de propiedades con profundidad ≤ 16; en el sobre suman 17 de los 64
     que `System.Text.Json` acepta. **Subir la cota exige MAJOR** (D-04.3).
   - Un documento sintacticamente legible, de major 1 y estructuralmente valido cuya profundidad supera 16 **no**
     es `PresentButUnreadable` ni `IncompatibleMajor`: es **`DepthLimitExceeded`**, un resultado propio de solo
     lectura (D-07.4) [X P-30].
   - Sobre `DepthLimitExceeded` no se permite **ninguna** escritura: crear, renombrar, cambiar valor, eliminar,
     vaciar ni unificar. El estado se muestra desde la lectura, no al fallar un commit (D-18.3).
   - Una operacion sobre un documento conforme no puede superar la cota: `Name` y `Value` son strings, sin
     profundidad, y el documento que escribe V1 tiene profundidad 3 [X P-17].

**Cambio desde V1.** NFC (1), el nombre como solo presentacion (3), la validacion por lo introducido (6) y la
cota de profundidad (7).

**Cambio desde V2 (C-2, C-3).**
- Validacion de UTF-16 antes de NFC (1) y antes de serializar (4).
- «Eliminar siempre se permite» se limita a los limites de conteo y longitud; ninguna operacion sobre estados no
  escribibles (6).
- La cota 16 pasa a constante del formato, con el estado `DepthLimitExceeded` en lugar de un rechazo al escribir
  (7).

**Cambio desde V3 (C-F1).**
- Un `Name` con un no-caracter es invalido al escribir, antes de NFC (1), y `PresentButUnreadable` al leer (2).
- NFC solo recibe nombres acreditados, con su frontera de excepciones (2).
- Un `Value` con un no-caracter sigue siendo valido y se relee exacto (4).

**Por que.** Cada coleccion de rack se replica en todos sus miembros (P-07). Los limites de conteo y longitud
acotan el payload sin convertirse en reglas de formato. La cota de profundidad **si** es regla de formato: aleja
cualquier contenido de propiedades del limite que dejaria el sobre ilegible en **todos** los builds [X P-17], y
si fuera tuning, un build posterior podria subirla dentro del major 1 y dejar sin edicion a los builds anteriores
[X P-30].

### D-06 — Orden

**Decision.** El orden persistido es el de presentacion: crear añade al final y V1 no reordena. El orden
participa en la igualdad canonica. *Sin cambios desde V1.*

### D-07 — Persistencia de Proyecto (MATERIAL · V2-13)

**Decision.**

```text
Database.NamedObjectsDictionaryId
 └─ "RACKCAD_CUSTOM_PROPERTIES" ─► Xrecord ─► DxfCode.Text en trozos de 255
```

1. **Clave nueva, Xrecord directo, congelada desde la primera escritura.**
2. **Lectura fisica tri-estado que nunca lanza** (`Absent`, `Present` o `PresentButUnreadable`), en un tipo
   nuevo del Plugin y con **la misma semantica** que `ProjectVariablesData` [D §4.2]:
   - una entrada que no es Xrecord → ilegible;
   - un Xrecord sin `Data` o sin texto → ilegible;
   - una excepcion de AutoCAD → ilegible.

   El troceado se **reimplementa**: esa pequeña duplicacion se mantiene y su extraccion queda como deuda F-13,
   no como trabajo de I-54.
3. **Store en Application**, con resultados `Absent`, `Readable`, `PresentButUnreadable`, `IncompatibleMajor`,
   `AmbiguousIdentity` y **`DepthLimitExceeded`** (C-2). Solo `Absent` y `Readable` permiten escribir.
4. **Orden de clasificacion, determinista y primero la version**:
   1. **Validacion del texto y del arbol completo** → `PresentButUnreadable` si:
      - el texto no es JSON o no se puede transcodificar;
      - la raiz no es un objeto;
      - **cualquier objeto, a cualquier profundidad**, repite un nombre de miembro con `OrdinalIgnoreCase`
        (D-01.5, C-4);
      - **algun nombre de miembro o valor string, a cualquier profundidad, no se puede decodificar** (C-3);
      - `SchemaVersion` falta o no tiene la forma exacta.

      Es un unico recorrido sobre `EnumerateObject` y `EnumerateArray`, previo a todo mapeo, que tambien mide la
      profundidad.
   2. major mayor que 1 → `IncompatibleMajor`, sin enlazar entradas. La cota 16 es del major 1 y no se aplica a
      otro major;
   3. `Entries` ausente, nulo o no array; entrada que no es objeto; `Id` sin la forma de D-02 o `Guid.Empty`;
      `Name` o `Value` que no son string; `Name` que contiene un **no-caracter** (C-F1); o `Name` vacio →
      `PresentButUnreadable`;
   4. profundidad mayor que 16 → **`DepthLimitExceeded`** (D-05.7, C-2);
   5. ids repetidos por valor → `AmbiguousIdentity`;
   6. en otro caso → `Readable`, con diagnostico de nombres repetidos.

   Entre los pasos 4 y 5 el orden solo decide el diagnostico: los dos resultados son de solo lectura. La cota es
   una regla de formato de todo el documento, y la unicidad de ids es una regla sobre las entradas.

   **Ningun contenido externo produce una excepcion** (V2-5.G, C-3). El store **no** usa `catch (Exception)`:
   captura exactamente las clases que el contenido externo puede provocar en el pipeline elegido, cada una en la
   operacion que la produce, y las convierte en `PresentButUnreadable`, salvo la defensa de NFC de la tercera fila,
   que no cambia la clasificacion (C-F1):

   | Operacion | Clase capturada | Por que no esconde un error de programacion |
   |---|---|---|
   | `JsonDocument.Parse(texto, opciones)` del NOD | `JsonException` (JSON invalido o mas de 64 niveles) y `ArgumentException` (texto no transcodificable: surrogate suelto crudo [X P-31]) | Las opciones son una constante y una prueba fija su validez, asi que un `ArgumentException` de `Parse` solo puede venir del contenido |
   | Decodificar un nombre (`JsonProperty.Name`) o un string (`JsonElement.GetString()`) | `InvalidOperationException` (surrogate suelto escapado [X P-31]) | Se invoca solo tras comprobar `ValueKind`, asi que la excepcion solo puede venir del contenido |
   | `Normalize(FormC)` o `IsNormalized(FormC)` sobre un `Name` acreditado, en el diagnostico de repetidos (C-F1) | `ArgumentException`, solo como defensa ante normalizadores de plataforma mas estrictos: con la precondicion no ocurre en ningun caso ejecutado [X P-35] | La forma es una constante y el argumento ya esta acreditado. La captura no cambia la clasificacion: el diagnostico compara ese nombre sin normalizar (D-05.2) |

   El miembro del sobre llega ya parseado como `JsonElement`, asi que en Rack solo aplica la segunda fila. Tras
   el recorrido del paso 1 ningun nombre ni string del documento lanza al decodificarse, asi que ni el mapeo ni
   la igualdad canonica (D-09.7) pueden lanzar por contenido. Tras el paso 3 todo `Name` de un documento `Readable`
   esta acreditado, asi que el diagnostico del paso 6 solo normaliza nombres acreditados (D-05.2, C-F1). La
   precondicion evita la excepcion de NFC en todos los casos ejecutados; la tercera fila de la tabla es la unica
   captura permitida, y no cambia la clasificacion.
5. **Escritura en una transaccion**: releer → acreditar esa lectura en Application → aplicar el intent por id
   → validar (D-05) → comprobar la guarda → escribir → confirmar. Es el patron `RegistryCommit` de I-47 y
   I-48 [P-26], sin regen. La escritura **solo** ocurre tras `Absent` o `Readable` **de esa misma transaccion**:
   nunca sobre una entrada que no sea Xrecord.
6. **Independencia en ambos sentidos**:
   - La lectura, la escritura y la UI de propiedades **no** leen `RACKCAD_PROJECT` ni dependen de su estado. Un
     registro de variables corrupto o incompatible no bloquea las propiedades de proyecto.
   - Project Variables **no** lee `RACKCAD_CUSTOM_PROPERTIES`. Una coleccion corrupta o incompatible no cambia
     `RACKVARIABLES`, `RACKEDITAR` ni `RACKBOMTOTAL`.
7. `RACKCAD_PROJECT`, `ProjectVariablesData`, `ProjectVariablesDocument` y `RackBlockData` **no se tocan**.

**Cambio desde V1.** Puntos 1, 4 (orden), 5 («misma transaccion») y 6.

**Cambio desde V2 (C-2, C-3, C-4).** Resultado `DepthLimitExceeded`; recorrido completo que detecta nombres
repetidos y strings no decodificables a cualquier profundidad; clases de excepcion capturadas explicitas.

**Cambio desde V3 (C-F1).** Un `Name` con un no-caracter es `PresentButUnreadable` en el paso 3; el diagnostico
del paso 6 solo normaliza nombres acreditados, con la defensa acotada de la tercera fila.

**Descartado.**

| Opcion | Motivo |
|---|---|
| Dentro de `ProjectVariablesDocument` | Acopla la lectura, la acreditacion y el bloqueo de dos autoridades (ADR-0034 §1), e I-49 extiende ese documento |
| Sub-diccionario bajo `RACKCAD_PROJECT` | Todo build existente lo leeria como ilegible (P-02) |
| Diccionario `RACKCAD` que agrupe claves | Incoherente con el Xrecord directo existente; no hace falta hoy |
| `Database.SummaryInfo` como autoridad | Sin id, sin version ni campos desconocidos, y editable fuera de RackCad sin guarda (I-47 A4) |
| Extraer ya el troceado de `ProjectVariablesData` | Tocaria un archivo de I-47 sin necesidad; queda como F-13 |

### D-08 — Persistencia de Rack y contrato de aislamiento (MATERIAL · V2-5 · V2-8)

**Decision.**

1. **Miembro** `CustomProperties` de tipo **`JsonElement?`** con
   `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`. Contiene el documento de D-01 como
   **objeto** JSON.
2. **Solo el store de propiedades lo interpreta**:
   - `null`, ausente o un elemento `Undefined` o `Null` → `Absent`;
   - un valor que no es objeto → `PresentButUnreadable`;
   - un objeto → las mismas reglas que Proyecto (D-07.4).
3. **Compositor.** `RackEmbedComposer.Compose` añade una asignacion, `CustomProperties = source?.CustomProperties`,
   y **ni la firma ni sus siete llamadores cambian**. La funcion pura nueva `WithCustomProperties(source, documento)`
   vive en `RackEmbedComposer` y devuelve un sobre igual a `source` en `Kind`, `Id`, `Name`, `View`, `Section`,
   `Design` y `ExtensionData`, con `SchemaVersion` resuelta sin degradar. Solo cambia `CustomProperties`.
4. **Contrato de aislamiento. Afirmacion permitida:**

   > Dado un sobre JSON sintacticamente valido y dentro del limite de profundidad soportado, el tipo o la forma
   > JSON de `CustomProperties` no vuelve ilegible el sobre.

   Restricciones normativas, cada una con prueba:

   | # | Restriccion | Hecho [X P-17] | Prueba |
   |---|---|---|---|
   | A | Una corrupcion sintactica o un truncado, tambien dentro del miembro, vuelve ilegible el sobre en **todos** los builds. No se afirma ningun aislamiento para ese caso. **Residual preexistente (C-3):** con UTF-16 crudo invalido en el texto del sobre, el `RackEmbedStore` de BASE no devuelve `null` sino que lanza (F-14a, ver abajo) | BASE y variante → `null`; UTF-16 crudo → `ArgumentException` [X P-31] | T-ENV-03 |
   | B | 64 niveles de anidado dentro del miembro vuelven ilegible el sobre en todos los builds. **Cota del formato 1.x: 16** (D-05.7, C-2); un miembro legible mas profundo es `DepthLimitExceeded`, de solo lectura | 63 se leen; 64 → `null` | T-ENV-04, T-STO-09 |
   | C | Nunca se escribe `default(JsonElement)`: `Compose` y `WithCustomProperties` lo normalizan a `null` | `Serialize` lanza | T-ENV-05 |
   | D | Un elemento de tipo `Null` se normaliza a `null` y no se emite | escribe `"CustomProperties":null` | T-ENV-05 |
   | E | Un `null` literal equivale a ausente y se elimina al reescribir | `HasValue = false` | T-ENV-02 |
   | F | `ExtensionData` nunca contiene una clave igual, con `OrdinalIgnoreCase`, a un miembro declarado del sobre. Leyendo, el store no puede producirla. `Compose` y `WithCustomProperties` **rechazan** un origen que la tenga (`InvalidOperationException`, error de programacion) antes de producir nada | dos claves; al releer gana `ExtensionData` | T-ENV-07 |
   | G | El store de propiedades **nunca lanza** por contenido externo: devuelve siempre un resultado tipado, capturando las clases explicitas de D-07.4 y nunca `catch (Exception)` (C-3) | `ArgumentException` e `InvalidOperationException` [X P-31] | T-STO-08, T-STO-20 |

   **Residuales declarados.** Ninguno lo puede recuperar el store de propiedades, porque ocurren en
   `System.Text.Json` o en el `RackEmbedStore` de BASE antes de que el store vea el miembro (C-3, C-4):
   - **Clave exacta del miembro repetida** en el JSON del sobre: gana la ultima, tanto en BASE (valor en
     `ExtensionData`) como en la variante con el miembro declarado [X P-17].
   - **Clave repetida con otra capitalizacion** (`CustomProperties` y `customproperties`): un build anterior
     conserva **ambas** claves en `ExtensionData` y las reemite; un build con el miembro declarado mapea las dos
     al miembro y se queda con la ultima [X P-33]. **No** se afirma «gana la ultima en todos los builds».
   - Ningun escritor RackCad emite claves repetidas (T-ENV-06).
   - **F-14a — UTF-16 crudo invalido en el texto de un sobre** (residual **preexistente**, no introducido por
     I-54): el `RackEmbedStore.Deserialize` de BASE solo captura `JsonException` (`RackEmbedDocument.cs:98`) y
     lanza `ArgumentException` **al leer** [X P-31]. `ScanEnvelopes` lanza con el, asi que los flujos de Rack de
     I-54 —como los demas comandos que barren— fallan cerrados, sin escribir, pero sin el diagnostico por
     definicion de D-09.3.
   - **F-14b — escape JSON de un surrogate suelto en un sobre** (residual **preexistente**, no introducido por
     I-54; C-F2): el sobre **se lee**, en BASE y en la variante, tanto si el escape esta dentro de
     `CustomProperties` como en cualquier otro campo. Pero **toda reescritura** de ese sobre lanza
     `JsonException`: `Serialize` del mismo objeto, `Compose(source)` seguido de `Serialize` y el restamp, que
     serializa el mismo objeto deserializado [X P-36]. Los flujos que reescriben el sobre —`RACKEDITAR`, la
     propagacion de variables, las copias independientes de `RACKDUPLICAR` y `RACKLAYOUT`, y el ejecutor de Rack
     de I-54— fallan sin escribir, con la excepcion dentro de la transaccion y sin resultado tipado, igual que en
     BASE (D-22.10). Si el escape esta dentro de `CustomProperties`, el store de propiedades clasifica el miembro
     `PresentButUnreadable` sin lanzar (D-07.4).
   - Para F-14a y F-14b:
     - **No se arreglan en I-54** y `RackEmbedStore` no se modifica.
     - Forman el hallazgo lateral **F-14**, con dos clases —**F-14a** al leer y **F-14b** al reescribir—, que se
       registra en `ideas-futuras.md` en G2-FREEZE (§14).
     - WORKFLOW §8 pide registrar los hallazgos laterales al detectarlos, pero el contrato de I-54 (§4 y §7)
       reserva ese registro al freeze y limita G2 a `docs/initiatives/I-54-*.md`, igual que con F-01..F-13.
   - **Alcance de la afirmacion (C-F2).** La afirmacion del punto 4 es de **legibilidad**. Para los consumidores
     que reescriben el sobre, la garantia de I-54 es no introducir un modo de fallo nuevo en los sobres que BASE
     puede leer **y reserializar**; F-14a y F-14b quedan fuera (INV-07, D-13).
5. **Sin promocion de major del sobre.** `RackEmbedDocument.CurrentSchemaVersion` sigue en `"1.0"` (V2-8).
   Justificacion por la matriz verificada:

   | Build que abre el dibujo | Lee un sobre con el miembro | Al reescribir | Si el sobre fuera major 2 |
   |---|---|---|---|
   | Anterior a I-11 | legible | **pierde** el miembro: no tiene `ExtensionData` [X P-18] | lo lee como legible: **no tiene guarda de version** [X P-18] |
   | I-11..I-53 | legible; el miembro va a `ExtensionData` | **lo conserva exacto**, con `Compose(source)` y con el restamp [X P-18] | **ilegible**: el rack sale de `RACKEDITAR`, `RACKLISTA`, BOM y duplicado |
   | I-54 | lo interpreta | lo conserva (`Compose` hereda) | ilegible |

   **Conclusion.** Promover no protege a los builds anteriores a I-11, que ignoran el major y pierden el
   miembro de todos modos. En cambio, dejaria ilegible **todo** rack con propiedades para I-11..I-53, que hoy
   las conservan. El riesgo residual de los builds anteriores a I-11 es R-12, y OQ-05 deja de ser pregunta.

**Cambio desde V1.** El punto 4 sustituye la afirmacion absoluta de V1. El punto 5 cambia la justificacion,
aunque la decision se mantiene.

**Cambio desde V3 (C-F2).** Residual F-14b declarado junto a F-14a, y alcance de la afirmacion acotado a los
sobres que BASE puede leer y reserializar.

**Descartado.**

| Opcion | Motivo |
|---|---|
| Miembro tipado (`CustomPropertiesDocument`) | Un contenido inesperado lanza al deserializar el sobre y deja el rack entero ilegible (P-05) |
| Miembro `string` con el JSON dentro | Un cambio de tipo JSON del miembro lanza `JsonException` [X P-17]; `JsonElement?` lo tolera y evita doble escape |
| Solo en `ExtensionData`, sin declarar | Abusa de «campos que este build no conoce» en el build que si los conoce |
| DTO de diseño por sistema | Seis formatos, dos sin `ExtensionData`, cruce con I-50 e I-53 y acoplamiento con la autoridad del Selectivo [D R-08] |
| Xrecord propio en el BTR | `RackCloner` no lo copia: se pierde en toda copia independiente [D §8.3] |
| Mapa `RackId → propiedades` en el NOD | No viaja entre dibujos y obligaria a `RACKDUPLICAR` a escribir el NOD |
| `BlockReference` (XData, atributos) | La referencia solo coloca (ADR-0010) |
| Promocion del major del sobre | Matriz del punto 5 |

### D-09 — Autoridad por RackId (MATERIAL · V2-3 · V2-4 · V2-16 · V2-2 · V2-1)

**Decision.** Todo lo que sigue es **Application pura** sobre la proyeccion plana de D-22.

1. **Barrido.** Dentro de la transaccion del llamador, `RackBlockFinder.ScanEnvelopes(transaction, database,
   includeReferenceCount: true)` recorre las definiciones con payload RackCad, **tambien** las ilegibles [P-22].
   **Nunca** se usa `FindRackBlocks`, que descarta los ilegibles y abre su propia transaccion.
2. **Pertenencia.** Una definicion es miembro de `R` si y solo si:
   - su sobre es legible;
   - su `Id` no esta vacio y es igual a `R` con `OrdinalIgnoreCase`;
   - no es dependiente de xref (`IsDependent`).

   `IsFromExternalReference` ya lo omite el barrido.
3. **Pertenencia indeterminada (V2-3).** Si **alguna** definicion recorrida, **colocada o no** y no
   dependiente de xref, tiene payload RackCad no interpretable, la pertenencia de **todo** `RackId` es
   indeterminada:
   - **se bloquean** todas las escrituras de alcance Rack del dibujo, incluida la unificacion;
   - **las lecturas** siguen, en modo solo lectura, con un diagnostico por definicion: nombre de bloque, handle
     y si esta colocada o no;
   - **no se bloquean** las propiedades de Proyecto, la geometria, el BOM ni ninguna funcion ajena a las
     propiedades: I-54 no añade comprobaciones a otros comandos;
   - un **sobre legible con `Id` en blanco** no pertenece a ningun `RackId` ni provoca bloqueo global. Si es el
     bloque elegido, da `NoIdentity`;
   - **Precedente**: `ProjectVariableConsumerDiscovery.cs:87-91,161-216`, donde toda definicion no interpretable
     aborta porque «sin su identidad» no se puede demostrar que no sea hermana [P-21];
   - **no se reutiliza** `ProjectVariableScanProjection`: filtra por Selectivo y trata un `Kind` en blanco como
     ilegible, mientras que I-54 no depende del kind y su pertenencia descansa solo en el `Id`.

   **Alternativas rechazadas.**
   - **B**, bloquear solo con evidencia de pertenencia: exige interpretar a medias formatos desconocidos o buscar
     el id en el texto, con falsos negativos.
   - **C**, ignorar los ilegibles: escritura parcial silenciosa si la ilegible era un miembro (contra INV-05).

   **Proporcionalidad.** Solo se bloquean las escrituras de propiedades de Rack. ADR-0034 ya acepto este coste
   en «Riesgos» (`:216-217`).
4. **Dependientes de xref.** Una definicion `IsDependent` no es miembro, no bloquea y nunca se escribe:
   pertenece a otro DWG (D-14). Elegir una referencia cuya definicion sea `IsFromExternalReference` o
   `IsDependent` se rechaza con diagnostico. El trato de AutoCAD a los registros dependientes es [I] y lo
   observa OV-14.
5. **Kind (V2-16 opcion A; C-1).** Entre los miembros de `R`, el `Kind` se compara con `OrdinalIgnoreCase`, como
   en `RackDuplicationPlan.cs:514`, `RackPushBackCommands.cs:197` y `RackCantileverCommands.cs:246`. Se evalua
   en este orden, sin otra combinacion posible:

   | Kinds de los miembros | Resultado |
   |---|---|
   | Algun miembro con `Kind` en blanco | **`MixedKind`** (opcion A), con un diagnostico que nombra esa definicion (bloque y handle) y remite a `RACKEDITAR` |
   | Mas de un kind no vacio distinto, conocidos o no | **`MixedKind`** |
   | Un unico kind no vacio, comun a todos, que este build **no conoce** | **`UnknownKind`**: solo lectura (C-1) |
   | Un unico kind no vacio, comun a todos y conocido | continua con el estado de las colecciones (punto 6) |

   **`UnknownKind` (C-1).** Solo lectura. No se permite crear, renombrar, cambiar valor, eliminar ni unificar;
   no se modifica el sobre y no se interpreta el diseño.
   - **Kind conocido**: `isKnownKind`, un predicado que Application **recibe inyectado desde el borde**.
     - Application no depende de `KindHandlerRegistry`.
     - El Plugin construye el predicado con `KindHandlerRegistry.Default.TryGetIgnoreCase` o con la costura
       equivalente vigente.
     - Precedente: `RackDuplicationPlan.Build(…, isKnownKind)` (`RackDuplicationPlan.cs:225-228`), alimentado en
       `RackDuplicarCommands.cs:69` [P-28].
   - **Por que.**
     - Todo camino que hoy reescribe un sobre exige un kind conocido [P-28].
     - Un kind nuevo entra sin tocar la version del sobre [P-29], de modo que un build anterior ve un sobre
       legible con un kind que no entiende.
     - Si I-54 escribiera ahi, seria el **primer** comando que reescribe un kind futuro. Ese kind podria dar otra
       semantica a la identidad o a las propiedades (un `RackId` compartido por subunidades, propiedades por vista),
       y ninguna version lo impediria.
     - Es el mismo principio que decide la opcion A.
   - **PR-12 de V2 queda rechazada**: un kind desconocido **nunca** es escribible.

   **Evidencia historica.**
   - Desde que existe el sobre, todo escritor pone `Kind` [P-23].
   - Un `Kind` en blanco con `Id` solo aparece en el payload Selectivo anterior al sobre, que se lee como sobre
     legible sin `Kind` ni `Design` [X P-23], o en datos ajenos.
   - La mayoria de los consumidores lo rechaza: variables, duplicado, Push Back, Cantilever, lista, inventario y
     `RACKEDITAR` sobre el propio bloque. Solo `FindRackBlocks` lo tolera [P-24].

   **Por que A y no B.**
   - Con **B** (excluir el kind de la comparacion), I-54 seria el unico comando que trata esa definicion como
     miembro **escribible**, en contra de `RACKDUPLICAR`, `RACKVARIABLES`, `RACKLISTA`, Push Back y Cantilever.
     Ademas mutaria un payload de origen incierto y no podria duplicarlo despues.
   - **Coste de A**: las propiedades de ese rack quedan en solo lectura, en un estado que ningun build de
     RackCad produce hoy.
   - **Coherencia con `UnknownKind`**: el mismo argumento («I-54 no puede ser el unico escritor») decide los dos
     casos; V2 lo aplicaba al `Kind` vacio pero no al desconocido, y V3 corrige esa asimetria (C-1).

   **El mensaje no promete reparacion.** Por lectura [L], `RACKEDITAR` → Actualizar desde otra vista reescribe
   el `Kind` en Selectivo, Dinamico y Cabecera, cuyos preflights no comprueban el kind (`FindRackBlocks` +
   `Compose` con kind constante). En Push Back y Cantilever tambien aborta, y sin ningun miembro con kind no
   hay editor que lo abra. I-54 no repara.
6. **Estado de cada coleccion.** Se lee la de cada miembro con el store. Si alguna es no escribible
   (`PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` o `DepthLimitExceeded`), el rack es
   `CustomPropertiesReadOnly`:
   - solo lectura, con el estado de cada vista;
   - sin escritura, sin unificacion y sin descarte;
   - un miembro no escribible **nunca** se omite para seguir con los demas.
7. **Igualdad canonica (V2-4; C-4, C-7C).** El store garantiza que el documento no tiene nombres repetidos a
   ninguna profundidad ni strings no decodificables (D-07.4). Sobre esa base, dos colecciones son iguales si y
   solo si:
   - **las dos son vacias canonicas**: `Absent`, o `Readable` con cero entradas y sin `ExtensionData` de raiz,
     **con cualquier minor**; o
   - **las dos son `Readable` y coinciden en todo su contenido salvo el minor**:
     - mismo **major** (siempre 1 en una coleccion legible);
     - `ExtensionData` de raiz igual en profundidad (objetos por nombre `Ordinal` sin importar el orden de
       miembros, lo que esta bien definido porque no hay nombres repetidos; arrays en orden; strings ordinales;
       numeros por texto crudo);
     - mismo numero de entradas y, entrada a entrada **en orden**: `Id` por valor de `Guid`, `Name` ordinal,
       `Value` ordinal y `ExtensionData` de entrada igual en profundidad.

   La regla vale para **todo** el contenido del documento. Un build futuro compara tambien los campos que declare,
   no solo los que I-54 ve como `ExtensionData`.

   **El minor no participa**: sin campos adicionales no transporta dato (V2-9), y si participase romperia la
   transitividad con «ausente ≡ vacio con cualquier minor». Por ejemplo, `{ausente, vacio@1.0, vacio@1.3}`
   daria divergencia falsa (PR-01, acordada en G2D).
8. **Resultados: orden unico** (C-1, C-2). Es el unico orden de V3; cualquier otra seccion remite aqui.
   1. `XrefRejected`: la definicion elegida es `IsFromExternalReference` o `IsDependent`.
   2. `NoIdentity`: el sobre elegido es legible pero su `Id` esta en blanco.
   3. `IndeterminateMembership(definiciones)`: alguna definicion recorrida no dependiente, colocada o no, tiene
      payload no interpretable; incluye el caso de un bloque elegido con payload no interpretable.
   4. `MixedKind(vistas)`: algun `Kind` en blanco, o mas de un kind no vacio distinto.
   5. `UnknownKind(kind)`: un unico kind comun no vacio que el build no conoce.
   6. `CustomPropertiesReadOnly(estado por vista)`: alguna coleccion `PresentButUnreadable`, `AmbiguousIdentity`,
      `IncompatibleMajor` o `DepthLimitExceeded`.
   7. `Divergent(resumen por vista)`.
   8. `Single(coleccion, versionDeEscritura)`.

   **Por que este orden.**
   - Los pasos 1 y 2 se deciden sobre el bloque elegido, antes de mirar el resto del dibujo.
   - El paso 3 va antes de todo lo que exige conocer los miembros, porque sin pertenencia no hay conjunto.
   - `MixedKind` va antes que `UnknownKind`, porque una mezcla de kinds ya es inconsistente sean conocidos o no.
   - El `Kind` va antes que el estado de las colecciones, porque sin kind conocido no hay escritura posible,
     sea cual sea la coleccion.
   - El estado de solo lectura de las colecciones va antes que la comparacion, porque solo se comparan
     colecciones escribibles.

   Todos los resultados distintos de `Single` son de solo lectura; la unica escritura posible fuera de `Single`
   es la unificacion segura desde `Divergent` (punto 10).

   La `versionDeEscritura` de `Single` es el minor mayor entre los miembros, y nunca menor que la version
   actual, porque su contenido es identico. El orden del barrido no cambia el resultado.
9. **Escrituras** solo con `Single` (D-10) o con una unificacion segura (punto 10), siempre por el ejecutor de
   D-22. **Nunca se elige hermana**, y el workspace no presenta los valores de una vista como los del rack (D-10.3).
10. **Unificar desde la vista seleccionada (V2-2; C-5).** Solo existe si se cumple todo lo siguiente:
    - el resultado es `Divergent`, y por tanto no hay pertenencia indeterminada, `NoIdentity`, xref, `MixedKind`
      ni `UnknownKind`;
    - **todos** los miembros son `Readable` o `Absent`: ninguno `PresentButUnreadable`, `AmbiguousIdentity`,
      `IncompatibleMajor` ni `DepthLimitExceeded`;
    - el **usuario** elige explicitamente la vista origen; no hay seleccion por defecto;
    - toda coleccion que **se sobrescribiria** (la de cada miembro no igual canonicamente al origen):
      - **no** tiene `ExtensionData` de raiz;
      - **no** tiene `ExtensionData` en ninguna entrada;
      - **no** tiene un minor mayor que el del origen;
    - antes de confirmar se muestra el **contenido de cada vista**, y la confirmacion exige una casilla.

    **Documento origen (C-5A).**
    - Si la vista origen es `Readable`, el documento origen es su coleccion, con la version resuelta sin degradar.
    - Si la vista origen es `Absent`, el documento origen es el **documento vacio canonico**:
      `SchemaVersion = CurrentSchemaVersion`, `Entries = []`, sin `ExtensionData`. No es `null`, y su minor para la
      condicion anterior es el de `CurrentSchemaVersion`.
    - La ventana muestra ese origen explicitamente como «vacio / sin propiedades» antes de confirmar.

    **Revalidacion fresca (C-5B).** La confirmacion corresponde al contenido mostrado de **todas** las vistas.
    Dentro de la transaccion de escritura, tras el nuevo barrido, se aborta **sin escribir** si ocurre cualquiera
    de estas cosas:
    - el conjunto de miembros no es el mostrado;
    - la forma canonica de **cada** miembro, origen y destinos, no coincide con la mostrada;
    - alguna condicion de este punto ya no se cumple.

    No basta revalidar el origen (D-22.5).

    **Atomicidad logica (C-5C).**
    - Tras un commit exitoso, **todos** los miembros del `RackId` quedan canonicamente iguales al documento
      origen; si no, ninguno cambia.
    - Fisicamente solo se escriben los miembros cuya forma canonica difiere del origen. El origen y los que ya
      son iguales no se reescriben, para no degradar el minor de ninguno (PR-07). Eso no contradice INV-05: la
      atomicidad es del estado logico del conjunto, no exige reescribir un BTR que ya esta en el estado destino.
    - Todo va en **una** transaccion.

    **El sistema nunca elige.**
11. **Descarte: no existe (V2-1).** `PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` y
    `DepthLimitExceeded` (C-2) no tienen salida destructiva en V1, en Rack ni en Proyecto, y un rack `UnknownKind`
    (C-1) tampoco: se conservan los bytes y se bloquea la escritura. Toda recuperacion destructiva futura queda
    fuera de alcance y exige una decision explicita del Owner (N-11).
12. **Flujos existentes.** En `RACKEDITAR`, cada miembro conserva su coleccion y una vista nueva hereda la del
    sobre elegido (D-21). No se añade ninguna comprobacion a esos flujos.

**Cambio desde V1.**
- Punto 3: sin filtro de colocacion.
- Puntos 4, 5 y 7 (sin minor): nuevos.
- Punto 6: sin unificar ni descartar.
- Punto 10: restringido.
- Punto 11: descarte eliminado.

**Cambio desde V2.**
- Punto 5: `UnknownKind` y `isKnownKind` inyectado; PR-12 rechazada (C-1).
- Punto 6: `DepthLimitExceeded` y el nombre `CustomPropertiesReadOnly` (C-2).
- Punto 7: igualdad sobre documentos sin nombres repetidos y sobre todo el contenido salvo el minor (C-4, C-7C).
- Punto 8: orden unico de ocho resultados (C-1, C-2).
- Punto 10: origen `Absent` canonico, revalidacion de todos los miembros y atomicidad logica (C-5).
- Punto 11: `DepthLimitExceeded` y `UnknownKind` tampoco tienen salida destructiva (C-1, C-2).

**Descartado.** Reutilizar `SelectiveAuthoredAuthority`, que solo ve el diseño interior del Selectivo; elegir
la primera vista o la frontal; escribir solo las hermanas legibles; comparar bytes; comparar el minor; escribir
sobre un kind desconocido (PR-12 de V2).

### D-10 — Mutaciones (V2-15 · V2-1)

**Decision.**

1. **Operaciones:**

   | Operacion | Efecto exacto | Se revalida en la lectura fresca | Confirmacion |
   |---|---|---|---|
   | Crear | Id nuevo en minusculas `D`, añadido al final; nombre NFC recortado; valor tal como llega | autoridad `Single` (Rack), o `Absent`/`Readable` (Proyecto); nombre y valor UTF-16 bien formados, validos y nombre unico; conteo resultante ≤ 50 | no |
   | Renombrar | Cambia **solo** `Name` de la entrada con ese id. **`Id` igual por valor; su representacion persistida se escribe en `D` minusculas** (D-02.3). `Value` y el `ExtensionData` de la entrada quedan exactos. Se permite cambiar solo mayusculas | el id existe; nombre nuevo UTF-16 bien formado, valido y sin colision con **otra** entrada | no |
   | Cambiar valor | Cambia **solo** `Value`. **`Id` igual por valor; su representacion persistida se escribe en `D` minusculas** (D-02.3). `Name` y el `ExtensionData` de la entrada quedan exactos | el id existe; valor nuevo UTF-16 bien formado y valido | no |
   | Eliminar | Quita **solo** esa entrada, con su `ExtensionData` | el id existe; permitido frente a limites de conteo y longitud (D-05.6) | no (UNDO de AutoCAD, observado en OV-08) |
   | Vaciar | Eliminar la ultima deja `Entries: []`. **Nunca** se borra el miembro del sobre ni la entrada del NOD | — | — |
   | Unificar | D-09.10 | D-09.10 | casilla |

   **Precondicion comun (C-2).** Toda operacion exige un estado escribible en la lectura fresca: `Single` en Rack,
   o `Absent`/`Readable` en Proyecto. Ninguna, eliminar y vaciar incluidos, se ejecuta sobre
   `DepthLimitExceeded` ni sobre ningun otro estado de solo lectura (D-05.6, D-09.8). Una prueba **no** compara
   byte a byte el texto original del `Id` (C-6).

2. **En toda mutacion** se conservan el `ExtensionData` de raiz y el de las entradas no tocadas. La version se
   resuelve sin degradar.
3. **Workspace cuando la autoridad no es `Single`.** **Nunca** presenta los valores de una hermana como valores
   del rack. Presenta:
   - el diagnostico y el modo solo lectura;
   - **cuando corresponde**, resumenes por vista: bloque, handle, vista y seccion, estado y, si la coleccion es
     legible, sus entradas `Nombre — Valor` etiquetadas con esa vista.

**Cambio desde V1.** Sin la fila «Descartar ilegible»; conservacion explicita del `ExtensionData` de entrada; el
punto 3 es nuevo.

**Cambio desde V2 (C-2, C-3, C-6).**
- `Id` igual por valor en lugar de «exacto».
- Validacion de UTF-16 en nombre y valor.
- Precondicion comun de estado escribible, que sustituye la comprobacion de profundidad al escribir.

**Por que.** Vaciar sin borrar conserva la version sin degradar y los campos que un build posterior hubiera
escrito. Como ausente ≡ vacio con cualquier minor, eso no crea divergencia.

### D-11 — Duplicacion e independencia (V2-7)

**Decision.**

1. `RACKDUPLICAR` y `RACKLAYOUT` (copia independiente) copian `CustomProperties` **tal cual** a traves de
   `RestampEnvelope`: mismos ids y mismos valores, y un contenido no escribible sigue no escribible. **Sin
   cambio de codigo.**
2. La independencia la da el contenedor: el sobre de la copia es otro, y escribir las propiedades de un rack
   nunca toca otro `RackId`.
3. `COPY` de AutoCAD y `RACKRELLENAR` comparten la definicion: **es el mismo rack**, con la misma autoridad.
4. Sin politica de copia por propiedad en V1: se copian todas (OQ-04 → DEFER).
5. **Contrato de prueba.** No se afirma que Core «prueba `RackEnvelopeRestamp`», porque ninguna suite carga el
   Plugin (P-08):

   | Capa | Que demuestra | Que **no** demuestra |
   |---|---|---|
   | **T-CPY-01** (Core, comportamiento) | La **propiedad** del `RackEmbedStore` de produccion de la que depende el restamp: deserializar un sobre con miembro, `View`, `Section`, `SchemaVersion` `1.7` y `ExtensionData`, mutar `Id`, `Name` y `Design` sobre ese objeto y serializarlo conserva `CustomProperties` (texto crudo exacto), `View`, `Section`, `SchemaVersion` y `ExtensionData`. Precedentes: `PersistenceUniformityTests.cs:193-217` en BASE, y el `DimensionViewsRestampTests.cs` de I-50 @ `ed50cbd`, que declara que las lineas del Plugin no se ejecutan | Que `RackEnvelopeRestamp.cs` haga eso |
   | **T-GRD-03** (Core, guarda estructural) | Sobre `RackEnvelopeRestamp.cs` enmascarado, en la sobrecarga con `Guid`: un solo `Deserialize(` asignado a un identificador X, un solo `Serialize(` con argumento X, sin `new RackEmbedDocument` y sin `RackEmbedComposer`. Archivo de prueba **nuevo**; **convive con G-R5** y **no la re-apunta** [P-27]; rojo demostrado con una violacion temporal | Comportamiento; es defensa secundaria |
   | **OV-05 y OV-06** (G9) | `RACKDUPLICAR` con varios origenes y vistas; `RACKLAYOUT` independiente; editar las propiedades de la copia no cambia el original, ni al reves; `COPY` nativo comparte | — |

6. **Mejora futura, registrada, no de I-54**: extraer a Application la mitad del restamp que maneja el sobre.
   Disparador: **cualquier cambio futuro al manejo del sobre en `RackEnvelopeRestamp.cs`**.
7. **I-52** tiene la obligacion de D-21.

**Descartado.** Regenerar ids en la copia: no añade independencia, que ya da el contenedor, y la regla es la de
ADR-0034 §12 (cambia la identidad del rack, no la del dato). Extraer ahora el restamp: tocaria
`RackEnvelopeRestamp.cs` y re-apuntaria G-R5 mientras I-52 lo reutiliza.

### D-12 — Biblioteca y exportaciones (V2-18)

**Decision.**

1. **Frontera de V1**: no se escriben propiedades en `.rackcad.json` ni se leen al importar. BOM, CSV, XLSX y
   `RACKLISTA` no cambian.
2. Es una **frontera de alcance**, no una clasificacion. V1 **no** afirma que las propiedades de rack sean de
   instancia: puede haber metadatos de diseño que en el futuro convenga llevar a la biblioteca.
3. **UX actual, documentada en la guia y observada en OV-11:**
   - guardar un rack en la biblioteca → sus propiedades **no viajan**;
   - abrir o insertar desde la biblioteca → el rack **nace sin propiedades** (importacion con `Compose(null)`, D-21).
4. **Punto de extension aditivo**: un slot en `RackProjectDocument`, que ya tiene `ExtensionData`. Queda sin
   diseñar.

**Cambio desde V1.** Punto 2 (se retira «cliente, area y ubicacion son de instancia») y punto 3.

### D-13 — Estados no escribibles y schema futuro (V2-1)

**Decision.**

| Estado del contenedor | Proyecto | Rack | Efecto en el resto |
|---|---|---|---|
| `Absent` | coleccion vacia, editable | vacia, editable si la autoridad es `Single` | ninguno |
| `Readable` | editable | editable si `Single` | ninguno |
| `Readable` con nombres repetidos | editable, con aviso | idem | ninguno |
| `PresentButUnreadable` | **solo lectura**; diagnostico; bytes intactos; sin «crear la primera» y sin descarte | **solo lectura**; bloquea escritura y unificacion de ese rack | ninguno |
| `AmbiguousIdentity` | **solo lectura**, con diagnostico propio | idem | ninguno |
| `IncompatibleMajor` | **solo lectura**; nunca se sobrescribe | idem | ninguno |
| **`DepthLimitExceeded`** (C-2) | **solo lectura** desde la lectura; ninguna escritura, eliminar y vaciar incluidos; bytes intactos | **solo lectura**; bloquea escritura y unificacion de ese rack | ninguno |

«Ninguno» significa que el dibujo, `RACKEDITAR`, el BOM, `RACKLISTA`, `RACKDUPLICAR`, `RACKLAYOUT` y
`RACKVARIABLES` se comportan igual que con un contenedor ausente, **dentro del alcance de aislamiento de D-08.4**
(INV-07): I-54 no introduce un modo de fallo nuevo en sobres que BASE puede leer **y reserializar**. **No** cubre
los residuales preexistentes del sobre F-14a y F-14b (C-F2). Con ellos, los flujos que reescriben el sobre fallan
sin escribir igual que en BASE, tambien si el miembro es `PresentButUnreadable` por un surrogate escapado. No hay
recuperacion destructiva en V1 (N-11).

Esta tabla clasifica **contenedores**. Los estados de rack que no dependen del contenedor —`XrefRejected`,
`NoIdentity`, `IndeterminateMembership`, `MixedKind` y `UnknownKind`— tambien son de solo lectura y siguen el
orden unico de D-09.8.

**Cambio desde V1.** Se eliminan «descarte explicito» y «unificar o descartar» sobre ilegibles.

**Cambio desde V2 (C-1, C-2).** Fila `DepthLimitExceeded` y remision a D-09.8 para los estados de rack.

**Cambio desde V3 (C-F2).** «Ninguno» se acota a sobres que BASE puede leer y reserializar; F-14a y F-14b quedan
fuera.

### D-14 — Entre dibujos

**Decision.**
- Las propiedades de rack viajan con la definicion por los mecanismos nativos de AutoCAD; las de proyecto no.
  V1 no transfiere, fusiona ni sincroniza. Como las entradas son autodescriptivas, un rack pegado en otro
  dibujo no queda con referencias rotas [I; OV-09].
- Una definicion dependiente de un xref pertenece a otro DWG: no es miembro ni se escribe (D-09.4).

### D-15 — Punto de extension: dibujo (solo restriccion · V2-17)

**Decision.** I-54 solo garantiza una identidad estable y local al alcance, y una autoridad de la que leer. Un
consumidor futuro lee **de la autoridad** (`Single` en Rack; `Absent` o `Readable` en Proyecto) y no del sobre
crudo. Ante cualquier otro estado no presenta ningun valor, y el dibujo nunca pasa a ser autoridad.

**No fija** formato de rotulacion, cuadro de titulo, atributos, campos de AutoCAD, `DWGPROPS` ni ningun destino
concreto.

### D-16 — Punto de extension: expresiones (solo restriccion · V2-17)

**Decision.**
- La unica clave de referencia posible es `CustomPropertyId`; el nombre solo presenta.
- **No fija** sintaxis, tokens, namespaces ni ambitos. `Rack` y `Project` quedan reservados por I-49 para ID20
  (P-13) e I-54 no los reclama.
- Los valores son texto y V1 no define semantica de operando.
- No hay dependencia con I-49 en ningun sentido, y las guardas de formulas quedan intactas.
- La relacion ID24 ↔ ID20 se decide en ID20 (OQ-07).

### D-17 — Punto de extension: plantillas (solo restriccion · V2-17)

**Decision.** I-54 solo garantiza:
- una identidad estable y local al alcance;
- que se pueden añadir campos informativos por minor, en la raiz y por entrada (D-04.3);
- que un consumidor futuro puede referenciar por id.

**No fija** donde vive una plantilla, si reutiliza ids, ni la politica de preservar, derivar o crear ids al
aplicarla. Todo eso pertenece a su iniciativa.

**Cambio desde V1.** Se retira «aplicarla crearia entradas con los ids de la plantilla».

### D-18 — UI minima reutilizable (MATERIAL por superficie · V2-20)

**Decision.**

1. **Un comando** con palabra clave: «Selecciona un rack o [Proyecto]». Es arquitectonicamente aceptable.
   `RACKPROPIEDADES` y el alias `RPR` (libre hoy, [D §11.5]) son **nombres de trabajo**, no congelados: el Owner
   los decide antes de G7 (OQ-03).
2. **Bucle** del patron `RACKVARIABLES` (P-15): leer en una transaccion → proyectar → ventana modal → **un**
   intent por id → preflight puro sobre la instantanea → ejecutor con lectura fresca y decision de Application
   (D-22) → releer.
3. **Workspace puro** en Application, **agnostico al alcance**:
   - estados `Editable`, `ReadOnly(motivo)` y `Divergent(unificacion disponible o no)`;
   - filas `{ Id, Name, Value, NombreRepetido }` **solo** con `Editable`, es decir `Single` en Rack o `Absent`/
     `Readable` en Proyecto;
   - en cualquier otro caso, resumenes por vista (D-10.3), diagnosticos y la etiqueta del alcance;
   - `DepthLimitExceeded` y `UnknownKind` son `ReadOnly(motivo)` **desde la lectura**: la ventana no ofrece ninguna
     operacion que despues falle al confirmar (C-1, C-2).
4. **Ventana** nueva `RackCustomPropertiesWindow`, arquetipo **C**, que adopta `DialogWindowChrome`,
   `EditorActions` y las reglas D6, D7 y D9 de ADR-0029. Contiene:
   - la lista `Nombre — Valor`, el detalle y los botones Nueva, Renombrar, Cambiar valor y Eliminar;
   - un banner de solo lectura con el motivo;
   - un **panel de unificacion solo cuando esta disponible** (D-09.10), con vista origen sin seleccion por
     defecto, el contenido de cada vista y una casilla. Una vista origen `Absent` se muestra explicitamente como
     «vacio / sin propiedades» (C-5A);
   - estado y Cerrar.

   **Sin panel de descarte** en ningun estado y sin `MessageBox`.
5. **Solo lectura** con cualquier resultado de D-09.8 distinto de `Single` (salvo la unificacion segura desde
   `Divergent`) y con los contenedores de Proyecto no escribibles. En concreto: `XrefRejected`, `NoIdentity`,
   pertenencia indeterminada, `MixedKind`, **`UnknownKind`**, `PresentButUnreadable`, `AmbiguousIdentity`,
   `IncompatibleMajor` y **`DepthLimitExceeded`** (C-1, C-2).
6. La ventana **no** ve `Database`, `ObjectId`, NOD ni sobres. Devuelve **intents por id** y **nunca es
   autoridad**: Application revalida cada intent sobre la lectura fresca (D-22).
7. **Censos previstos**: +2 `[CommandMethod]` (33 → 35 sobre `BASE_SHA`) y +1 ventana C (29 → 30; C: 11 → 12),
   mas la ayuda en `RackCommandReference`. **El censo real se verifica antes de G7**: I-52 V1 preve `RACKMIRROR`
   sin alias (33 → 34, `I-52-proposal-v1.md:76` @ `0fc7032`).
8. **Fuera de V1**: seccion en los editores de sistema, boton en el menu principal (OQ-03), columnas en
   `RACKLISTA`, reordenar e importar/exportar.

**Descartado.** Integrarlo en `RACKVARIABLES`, porque serian dos autoridades en una ventana (ADR-0034 §1). En
los editores de sistema: son archivos calientes (I-50 G3, I-53).

**Cambio desde V2 (C-1, C-2, C-5).** Estados de solo lectura `UnknownKind` y `DepthLimitExceeded` presentados
desde la lectura (3, 5); origen `Absent` mostrado como «vacio / sin propiedades» (4).

### D-19 — Nombres de codigo y guardas

**Decision.**

1. **Nombres orientativos** (solo son contractuales los que fijen las guardas):
   - Application: `CustomPropertiesDocument`, `CustomPropertyEntryDocument`, `CustomPropertyId`,
     `CustomPropertiesStore`, `CustomPropertiesReadResult`, `CustomPropertiesWriteGuard`,
     `RackCustomPropertiesAuthority`, `CustomPropertiesWorkspace`, `CustomPropertiesIntent`,
     `CustomPropertiesPreflight` y `CustomPropertiesCommit`;
   - Plugin: `CustomPropertiesData` y `RackPropiedadesCommands`.
2. **Prohibido** usar los substrings de P-12 en identificadores: `CustomPropertyValues` o `rackPropertyId`
   romperian la suite Core.
3. **Guardas nuevas**. Todas se demuestran en rojo con una violacion temporal y son defensa **secundaria**;
   el criterio de aceptacion es el comportamiento (leccion del BLOCKER de I-48).

   | Id | Guarda | Gate |
   |---|---|---|
   | T-GRD-01 | Construccion del sobre: en todo `src/`, sobre codigo enmascarado, `new RackEmbedDocument`, y el `new(` con tipo destino declarado `RackEmbedDocument`, solo aparecen en `RackEmbedComposer.cs`. `tests/` queda fuera (P-25) | G4 |
   | T-GRD-02 | Censo de llamadas a `RackEmbedComposer.Compose(` en `src/`, fuera de `RackEmbedComposer.cs`: exactamente las siete de D-21, por archivo. Una llamada nueva falla y obliga a clasificar su `source` en D-21 | G4 |
   | T-GRD-03 | Restamp estructural (D-11.5), sin re-apuntar G-R5 | G4 |
   | T-GRD-04 | Ejecutores de propiedades: una transaccion y un `Commit`; `ScanEnvelopes(` y no `FindRackBlocks(`; ni `Regen(`, `RedefineSystemBlock`, `EnsureForPlan`, `PurgeUnreferenced` ni `PurgeAfterCommit`; sin decisiones propias, es decir, sin referencias a la autoridad o al preflight fuera de la llamada de commit a Application | G6 |
   | T-GRD-05 | Independencia de Proyecto: los archivos de propiedades no referencian `ProjectVariables*` ni `RACKCAD_PROJECT`, y los de Project Variables no referencian `CustomProperties*` ni `RACKCAD_CUSTOM_PROPERTIES` | G6 |
   | T-GRD-06 | Capas: Domain sin propiedades; UI sin `Autodesk`; los tipos de propiedades de Application sin `ObjectId`, `Database` ni `Transaction` | G6 |
   | T-GRD-07 | Constante de clave del NOD exacta; ningun substring de P-12 | G6 |
   | T-GRD-08 | Censos de comandos y ventanas actualizados, sin relajarlos | G7 |

### D-20 — ADR

**Decision.** El ADR es **obligatorio** antes de producir codigo. Su alcance candidato esta en §15; no se
reserva numero ni se redacta. Nace `propuesto` en el Consensus Freeze y **debe existir antes de G3**.

### D-21 — Invariante de preservacion del sobre (MATERIAL · V2-6)

**Decision.** Invariante arquitectonico:

> **Todo sobre derivado de un rack existente se origina en (A) `RackEmbedComposer.Compose(source, …)`, con el
> sobre real de ese rack como `source`, o en (B) el re-estampado del MISMO objeto deserializado.**
> `Compose(null, …)` solo se permite para un **rack nuevo** y para una **importacion desde la biblioteca**.

Se aplica a redibujo, vista nueva, propagacion de Project Variables, duplicado, layout, espejo y **cualquier
camino futuro**.

| Camino hoy (`BASE_SHA`) | Mecanismo | `source` | Evidencia |
|---|---|---|---|
| `RACKEDITAR` → Actualizar, seis sistemas | A | el sobre de cada miembro | llamadas en `RackSelectivoCommands.cs:390`, `RackDinamicoCommands.cs:380`, `RackPushBackCommands.cs:415`, `RackCabeceraCommands.cs:195`, `RackCamaCommands.cs:197` y `RackCantileverCommands.cs:493` [E]; que cada llamador pase el suyo [L, D §5.3] |
| `RACKEDITAR` → vista nueva | A | el sobre elegido | [L, D §5.4] |
| Propagacion de variables | A | `view.Embed` | `ProjectVariableMutationExecutor.cs:208`; guarda `ProjectVariableMutationExecutorTests.cs:288-298` [E] |
| `RACKDUPLICAR` y `RACKLAYOUT` | B | — | `RackEnvelopeRestamp.cs:60,87` [E] |
| Rack nuevo (menu, `Quick*`) | `Compose(null)` | `null` | los constructores de payload declaran `source = null` por defecto [E]; que los caminos de rack nuevo no lo pasen [L, D §5.4] |
| Importacion de biblioteca | `Compose(null)` | `null` | [D §9.3] |
| Espejo de I-52 (futuro) | **A y despues B**: `Compose(sobreFuente, …)` con el diseño reflejado y `RestampEnvelope` sobre ese payload | real | `I-52-proposal-v1.md:316,698-702` @ `0fc7032` [E]: cumple el invariante |
| Escritura de propiedades de I-54 | `WithCustomProperties` sobre el sobre fresco de cada miembro, en el compositor | real | D-22 |

**Como se vigila.**
- **T-ENV-08**: comportamiento de `Compose`.
- **T-GRD-01**: construccion.
- **T-GRD-02**: censo de llamadas a `Compose`.
- **T-GRD-03**: restamp.
- **OV-03, OV-04, OV-07, OV-11 y OV-13**: los caminos del Plugin, que Core no ejecuta.

Las guardas **no** sustituyen las pruebas de comportamiento.

**Limite declarado.** T-GRD-02 no ve a quien llama a los constructores de payload con `source = null`. Esos
caminos los cubren las OV del parrafo anterior.

**Obligacion explicita para I-52.** Todo camino de escritura del espejo cumple el invariante: `Compose` con el
sobre fuente real y/o re-estampado del mismo objeto, nunca `new RackEmbedDocument` ni `Compose(null)` sobre un
rack existente.

Su Proposal V1 (`0fc7032`) ya lo hace (ID-5, §10.1: reflejar → `Compose(sobreFuente, …)` → `RestampEnvelope`).
Tambien prevé la prueba T-M20 («propiedades de alcance Rack sobreviven exactas» si I-54 esta integrada) y declara
que depende de que `Compose` herede el miembro (`:735-737`).

La llamada nueva a `Compose` del espejo cambia el censo de T-GRD-02 de 7 a 8. Quien integre despues re-apunta el
censo y clasifica esa llamada en esta tabla, como A con `source` real. Si I-52 se integra antes que I-54, I-54 lo
re-mide al rebasar y sus guardas lo comprueban; si despues, las guardas de I-54 ya lo vigilan.

### D-22 — Ejecutor de alcance Rack (MATERIAL · V2-14)

**Decision.**

```text
LEER       tx { ScanEnvelopes(tx, db, refs:true) → proyeccion plana } → Application: pertenencia → autoridad → workspace
PREFLIGHT  Application(intent, proyeccion de la instantanea) → aceptado | rechazado   (aviso temprano; no autoritativo)
ESCRIBIR   tx { ScanEnvelopes(tx, db, refs:true) → proyeccion plana FRESCA
                → Application.Commit(intent, proyeccion fresca) → plan | abortar
                → por cada definicion del plan: RackBlockData.Write(tx, definicion, payload)
                → Commit }
RELEER     LEER
```

1. **Una transaccion del llamador** para todo el ciclo: barrido → pertenencia → autoridad → mutacion →
   escritura de los miembros que difieren del estado destino → confirmacion. Todo o nada en el **estado logico**
   del conjunto (INV-05, C-5C).
2. **Proyeccion plana hacia Application**, por definicion:
   - handle como string y nombre de bloque;
   - colocada (referencias directas > 0) y dependiente de xref;
   - si el sobre es legible;
   - el `RackEmbedDocument` deserializado, cuando existe.

   **Sin** `ObjectId`, `Database` ni `Transaction`. El Plugin guarda en la misma transaccion el mapa
   handle → `ObjectId`. Junto a la proyeccion, el Plugin **inyecta** el predicado `isKnownKind`, construido con
   `KindHandlerRegistry.Default.TryGetIgnoreCase` o la costura equivalente vigente. Application no depende de
   `KindHandlerRegistry` (C-1).
3. **Application decide**: pertenencia, kind conocido, autoridad, validacion del intent y **plan de mutacion**.
   El plan es la lista de pares `(handle, payload)`, y cada payload es `WithCustomProperties(sobre fresco de ese
   miembro, documento resultante)` serializado.
4. **El Plugin** toma la instantanea, llama a Application sobre la lectura **fresca**, escribe y confirma. **No
   toma ninguna decision semantica** (T-GRD-04). Precedente: `RegistryCommit` en
   `ProjectVariableMutationExecutor.cs:250-288` [P-26].
5. **Se revalida sobre la lectura fresca**, reevaluando el orden unico de D-09.8, y se aborta sin escribir si
   algo falla:
   - **identidad**: el `RackId` sigue teniendo miembros y el id de la entrada existe;
   - **kind**: sigue siendo unico y conocido (C-1);
   - **autoridad**: sigue `Single`; o, para unificar, sigue `Divergent` segura con la misma vista origen, el
     **mismo conjunto de miembros** y la forma canonica de **cada** miembro igual a la mostrada (C-5B). Para
     eso, el intent de unificacion lleva por handle la forma canonica mostrada, o una huella determinista de ella;
   - **nombre y limites**: D-05 sobre lo que la operacion introduce, con UTF-16 bien formado (C-3) y un `Name`
     sin no-caracteres (C-F1);
   - **estado escribible**: ninguna coleccion `DepthLimitExceeded` ni otro estado de solo lectura (C-2).
6. **Se rechazan las xref**: una definicion `IsFromExternalReference` o `IsDependent` nunca entra en un plan
   (D-09.4).
7. **Sin** redefinir bloques, importar, purgar ni regenerar: solo `RackBlockData.Write`. `InDocumentTransaction`
   basta para un cuerpo de una fase (`InDocumentTransaction.cs:7-15`).
8. **UNDO.** Su granularidad para una escritura de Xrecords sin redefinicion **no se afirma**: OV-08 la observa
   y la registra. I-51 valido `UNDO` tras varios destinos de `RACKDUPLICAR` (`HANDOFF.md:1133-1136`), pero es
   otro camino de escritura.
9. **El ejecutor de Proyecto** sigue el mismo esquema sobre el NOD (D-07.5).
10. **Fallo al serializar un sobre (C-F2).** El plan de mutacion se construye completo, con todos los payloads
    serializados, **antes** de la primera `RackBlockData.Write`. Si serializar el sobre preparado de **cualquier**
    miembro lanza dentro de la transaccion —por ejemplo, con el residual F-14b en otro campo de ese sobre—:
    - **no hay commit**;
    - **no se escribe ningun miembro**: ningun hermano cambia;
    - el error es visible por la ruta general del comando;
    - los datos previos quedan intactos.

    Es atomicidad de **infraestructura**, no recuperacion del residual, y no se promete un resultado tipado: la
    costura actual lo propaga como excepcion (D-08.4).

**Cambio desde V1.** Transaccion explicita, `ScanEnvelopes`, reparto Application/Plugin, revalidacion
enumerada, rechazo de xref y UNDO no afirmado.

**Cambio desde V2 (C-1, C-2, C-3, C-5).**
- Predicado `isKnownKind` inyectado.
- Revalidacion de kind, de estado escribible y de UTF-16.
- Unificacion revalidada sobre todos los miembros mostrados.
- Atomicidad expresada como estado logico.

**Cambio desde V3 (C-F1, C-F2).**
- Revalidacion del `Name` sin no-caracteres (5).
- Punto 10: sin commit ni escritura si serializar el sobre de un miembro lanza.

## 7. Invariantes

| # | Invariante | Cambio |
|---|---|---|
| INV-01 | Una coleccion de Proyecto por `Database`; una coleccion de Rack por `RackId`, canonicamente identica en cada miembro | precisado |
| INV-02 | La identidad de una entrada es su `CustomPropertyId`. El nombre nunca es identidad, clave de busqueda ni clave de agregacion | ampliado |
| INV-03 | Renombrar cambia solo `Name` y cambiar valor solo `Value`; los dos conservan el `ExtensionData` de la entrada | ampliado |
| INV-04 | Escribir propiedades no toca geometria, diseño, `Kind`, `Id`, `Name`, `View`, `Section` ni `ExtensionData` del sobre; su `SchemaVersion` solo se resuelve sin degradar, como en `Compose` | precisado |
| INV-05 | **Estado logico** (C-5C): tras un commit exitoso de alcance Rack, **todos** los miembros del `RackId` quedan canonicamente iguales a la coleccion resultante (edicion) o al origen (unificacion), en **una** transaccion; o no cambia ninguno. No exige reescribir fisicamente un BTR que ya esta en el estado destino | **precisado en V3** |
| INV-06 | **Nada se escribe sobre `PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` ni `DepthLimitExceeded`, y no hay recuperacion destructiva en V1** | **cambiado**; `DepthLimitExceeded` en V3 |
| INV-07 | **Aislamiento acotado**: dado un sobre sintacticamente valido, dentro de la profundidad soportada y que BASE puede leer **y reserializar**, el estado de las propiedades no cambia su legibilidad ni el resultado de `RACKEDITAR`, BOM, `RACKLISTA`, `RACKDUPLICAR`, `RACKLAYOUT` y `RACKVARIABLES`: I-54 no introduce un modo de fallo nuevo en esos consumidores. Los residuales preexistentes del sobre F-14a (UTF-16 crudo invalido) y F-14b (surrogate escapado que impide reserializar) quedan fuera (D-08.4, C-F2) | **acotado**; precisado en V4 |
| INV-08 | Un sobre sin propiedades se serializa byte a byte igual que en `BASE_SHA` | igual |
| INV-09 | **Preservacion del sobre** (D-21) | **nuevo** |
| INV-10 | Una copia independiente conserva ids y valores; una copia enlazada es el mismo rack | igual |
| INV-11 | El sistema nunca elige hermana, ni presenta los valores de una como los del rack | ampliado |
| INV-12 | Un payload RackCad no interpretable, colocado o no, bloquea las escrituras de Rack. Un sobre legible con `Id` en blanco y una definicion dependiente de xref no son miembros ni bloquean | **cambiado** |
| INV-13 | Domain no conoce propiedades; la UI no conoce AutoCAD; Application no conoce `ObjectId`, `Database` ni `Transaction`; el Plugin no toma decisiones semanticas | ampliado |
| INV-14 | Ninguna propiedad se lee, escribe ni resuelve como variable, expresion o formula | igual |
| INV-15 | La biblioteca no exporta ni importa propiedades en V1, como **frontera** | precisado |
| INV-16 | No cambian `RACKCAD_PROJECT`, `ProjectVariablesData`, `ProjectVariablesDocument`, `RackBlockData`, `RackBlockFinder`, `RackEnvelopeRestamp`, `RackCloner` ni los `Edit*` | ampliado |
| INV-17 | Propiedades de Proyecto y Project Variables son independientes en ambos sentidos | **nuevo** |
| INV-18 | Unificar nunca sobrescribe contenido desconocido (`ExtensionData` de raiz o de entrada, minor mayor) y nunca esta disponible con miembros no escribibles | **nuevo** |
| INV-19 | V1 no define claves de agregacion entre racks | **nuevo** |
| INV-20 | Ningun escritor emite `default(JsonElement)`, un elemento `Null`, `ExtensionData` que colisione con un miembro declarado ni un documento de propiedades de mas de 16 niveles. **16 es constante del formato 1.x** y subirla exige MAJOR; un documento legible mas profundo es `DepthLimitExceeded` y nunca se escribe (C-2) | **nuevo**; precisado en V3 |
| INV-21 | La UI nunca es autoridad: los intents por id se revalidan en Application sobre la lectura fresca | **nuevo** |
| INV-22 | Ninguna escritura de propiedades alcanza un rack cuyo `Kind` comun no conoce el build: `UnknownKind` es solo lectura (C-1) | **nuevo en V3** |
| INV-23 | Ningun contenido externo hace lanzar al store de propiedades; ningun `Name` o `Value` que no sea UTF-16 bien formado llega a normalizarse o a persistirse (C-3); y ningun `Name` con un no-caracter se acredita al leer, se normaliza ni se persiste. La buena formacion es necesaria pero no suficiente para NFC (C-F1) | **nuevo en V3**; precisado en V4 |
| INV-24 | Un documento con nombres de miembro repetidos en cualquier objeto, a cualquier profundidad, es `PresentButUnreadable`; ninguna capa aplica «gana el primero» ni «gana el ultimo» (C-4) | **nuevo en V3** |

## 8. Flujos

```text
LEER PROYECTO     tx { NOD → lectura tri-estado → store } → workspace
ESCRIBIR PROYECTO intent → preflight(instantanea) → tx { releer → acreditar → aplicar por id → D-05 → guarda → escribir } → commit → releer
LEER RACK         pick → [xref ⇒ rechazo] → tx { ScanEnvelopes → proyeccion plana } → pertenencia → autoridad → workspace
ESCRIBIR RACK     intent → preflight → tx { ScanEnvelopes → proyeccion fresca → Commit (Application) → por miembro: RackBlockData.Write } → commit → releer   (sin regen)
UNIFICAR          intent(vista origen, formas mostradas de todos los miembros, confirmado) → tx { ScanEnvelopes → proyeccion fresca → Commit: Divergent segura, mismo conjunto y cada miembro igual a lo mostrado → escribir solo los miembros distintos del origen (Absent = vacio canonico) } → commit
NO ESCRIBIBLE     lectura → solo lectura con diagnostico (incluye UnknownKind y DepthLimitExceeded); no existe camino de escritura
RACKEDITAR        sin cambios de codigo → Compose hereda la coleccion de cada miembro
VISTA NUEVA       sin cambios → Compose hereda la del sobre elegido
RACKDUPLICAR      sin cambios → RestampEnvelope conserva el miembro
BIBLIOTECA        sin cambios → ni exporta ni importa; importar compone con null
```

## 9. Compatibilidad

| Build que abre el dibujo | Rack con propiedades: dibujar y editar sistema | Redibujo y duplicado | Editor de propiedades |
|---|---|---|---|
| Anterior a I-11 | normal | **pierde** las propiedades al reescribir el sobre [X P-18] | no existe |
| I-11 … antes de I-54 | normal | **las conserva** sin interpretar [X P-18] | no existe |
| I-54 | normal | las conserva | segun D-13 |
| Futuro, mismo major 1.x (conforme: profundidad ≤ 16) | normal | las conserva; minor sin degradar | editable; campos nuevos conservados; unificar bloqueado sobre ellos |
| Documento 1.x **no conforme** (profundidad > 16) | normal | las conserva | `DepthLimitExceeded`: solo lectura, sin ninguna escritura (C-2) |
| Futuro, coleccion major 2 | normal (el sobre sigue 1.x) | conserva los bytes | `IncompatibleMajor`: solo lectura |
| Rack de un **kind que este build no conoce** (kind posterior) | como hoy: los comandos que reescriben lo rechazan (P-28) | como hoy | `UnknownKind`: solo lectura (C-1) |
| Payload Selectivo anterior al sobre (`Kind` en blanco) | como hoy (P-24) | como hoy | su rack da `MixedKind`: solo lectura |

## 10. Riesgos y mitigaciones

| Riesgo del Discovery | Mitigacion en V4 |
|---|---|
| R-01 campo perdido en redibujo | D-08.3 herencia; D-21; T-ENV-08; T-GRD-02 |
| R-02 sobre ilegible por contenido | D-08.4, afirmacion acotada; restricciones A..G con clases de excepcion explicitas (C-3); T-ENV-02..07 |
| R-03 divergencia silenciosa | D-09 autoridad; D-22 escritura unica; unificacion segura |
| R-04 pertenencia indeterminada | D-09.3, sin filtro de colocacion |
| R-05 migrar `RACKCAD_PROJECT` | D-07: no se migra |
| R-06 acoplar con variables | D-07.6 independencia; T-GRD-05 |
| R-07 dato fuera del sobre | D-08: dentro del sobre |
| R-08 DTO de sistema | D-08: descartado |
| R-09 restamp sin prueba e I-52 | D-11.5 contrato honesto; D-21, obligacion de I-52 |
| R-10 nombres que rompen guardas | D-19.2; T-GRD-07 |
| R-11 sintaxis `Rack.X` | D-16: sin sintaxis |
| R-12 builds anteriores a I-11 | **Riesgo residual declarado**: al reescribir pierden las propiedades de rack y la promocion no lo evitaria (D-08.5). Se informa al Owner en el freeze |
| R-13 nulos en todos los sobres | `WhenWritingNull`; T-CHR-01 y T-ENV-01 |
| R-14 conflictos de censos y docs | Declarados; se resuelven al integrar; censo verificado antes de G7 |
| R-15 expectativa entre dibujos | D-14; OV-09 |

**Riesgos propios de V2, V3 y V4**

| # | Riesgo | Mitigacion |
|---|---|---|
| RP-01 | Una definicion ilegible ajena, **incluso no colocada**, bloquea las escrituras de Rack de todo el dibujo | Diagnostico con bloque, handle y colocacion; Proyecto y el resto de funciones siguen; coste aceptado en ADR-0034 «Riesgos» |
| RP-02 | Un rack `Divergent` no tiene salida en V1 si, elija el usuario la vista origen que elija, alguna coleccion que se sobrescribiria lleva `ExtensionData` o un minor mayor | Fallo cerrado deliberado (V2-2). La salida es un build que entienda esos campos o una recuperacion futura aprobada por el Owner |
| RP-03 | Un fallo a medias de `RACKEDITAR` deja divergentes otros datos del rack | `RACKEDITAR` no cambia la coleccion: cada miembro conserva la suya |
| RP-04 | I-52 escribe el sobre del espejo con `new RackEmbedDocument` o `Compose(null)` | D-21: obligacion y guardas T-GRD-01..03. Su V1 (`0fc7032`) compone con el sobre fuente y re-estampa; el riesgo queda en que su diseño cambie |
| RP-05 | Los censos cambian tambien en I-52: comandos 33 → 34 y una llamada nueva a `Compose` (T-GRD-02: 7 → 8) | Conflicto textual; quien integre despues re-apunta y clasifica; el censo real se verifica antes de G7 |
| RP-06 | Un rack con una vista sin `Kind` queda en solo lectura | D-09.5; un estado que ningun build actual produce |
| RP-07 | Una definicion dependiente de xref con sobre RackCad se comporta de forma distinta a lo inferido | D-09.4 conservador (nunca se escribe); OV-14 |
| RP-08 | La precision PR-01 (el minor no participa en la igualdad) es incorrecta | **Cerrado**: PR-01 = AGREE en G2D |
| RP-09 | Un rack de un kind posterior abierto en este build no admite editar sus propiedades | Deliberado (C-1): solo lectura con diagnostico; se edita con el build que conoce el kind |
| RP-10 | Un documento 1.x no conforme (profundidad > 16) queda en solo lectura, sin salida en V1 | Deliberado (C-2): ningun escritor conforme lo produce; se muestra desde la lectura con `DepthLimitExceeded` |
| RP-11 | UTF-16 invalido en un sobre. **F-14a**: el texto crudo invalido hace lanzar el barrido al leer. **F-14b**: con un surrogate escapado el sobre se lee, pero toda reescritura lanza. En ambos casos, los flujos que reescriben el sobre —incluido el ejecutor de Rack de I-54— fallan sin escribir y sin diagnostico tipado | Residuales **preexistentes** del `RackEmbedStore` de BASE (D-08.4); no se arreglan en I-54; sin commit ni escritura parcial (D-22.10); F-14a y F-14b se registran en G2-FREEZE (C-F2) |

## 11. Mapa previsto de archivos (orientativo, no autorizado)

| Capa | Nuevos | Modificados |
|---|---|---|
| Domain | — | — |
| Application | ~11: documento, entrada, id, store, resultado, guarda, autoridad y pertenencia, intent, preflight, commit y workspace | `Persistence/RackEmbedDocument.cs` (+1 miembro); `Persistence/RackEmbedComposer.cs` (+1 asignacion, `WithCustomProperties`, normalizacion y rechazo de colision) |
| Plugin | `CustomPropertiesData.cs`; `RackPropiedadesCommands.cs` (comando, lectura, ejecutores de Proyecto y Rack) | **ninguno**. No se tocan `RackBlockFinder.cs`, `RackCommandSupport.cs`, `RackEnvelopeRestamp.cs`, `RackCloner.cs`, `RackBlockData.cs` ni `ProjectVariablesData.cs` |
| UI | `RackCustomPropertiesWindow.xaml(.cs)` | `RackCommandReference.cs` |
| Tests Core | Store, sobre y caracterizacion, autoridad, mutaciones, workspace, y guardas T-GRD-01..07 en archivos **nuevos** | `SelectiveEditorOpenTests.cs` (censo de comandos). **No** se toca `SelectiveDuplicationFailClosedTests.cs` (G-R5) |
| Tests UI | Ventana | `WindowCensusGuardTests.cs` |
| Docs | ADR; guia de uso; `README`, `despliegue.md`, `validacion-manual-autocad.md` e `ideas-futuras.md` | Contrato; HANDOFF y ROADMAP solo al integrar |

Hoy ninguno de estos archivos de produccion lo toca I-49, I-50, I-52 ni I-53 (§13).

## 12. Evidencia de pruebas y validacion

**Nada de esto se implementa ahora.** Toda seleccion filtrada debe demostrar que ejecuto al menos una prueba
(AGENTS.md). Las guardas son defensa secundaria.

### 12.1 STORE — Core, G3

| Id | Afirma |
|---|---|
| T-STO-01 | **Ausente** (sin texto; miembro nulo) → `Absent`, coleccion vacia, se puede escribir |
| T-STO-02 | **Vacio** `{"SchemaVersion":"1.0","Entries":[]}` → `Readable` con cero entradas |
| T-STO-03 | **Schema ausente**, en blanco, `"1"`, `"1.0.0"`, `" 1.0"` o no numerico → `PresentButUnreadable` |
| T-STO-04 | **Schema malo**: raiz no objeto; `Entries` ausente, nulo o no array; entrada no objeto; `SchemaVersion` no string → `PresentButUnreadable` |
| T-STO-05 | **Major futuro** (`2.0` con `Value` numerico y otra forma) → `IncompatibleMajor`, sin excepcion y sin enlazar entradas |
| T-STO-06 | **Minor futuro** (`1.7` con campos desconocidos) → `Readable`; reescribir conserva `1.7` |
| T-STO-07 | **Campos desconocidos** de raiz y de entrada sobreviven exactos a reescribir |
| T-STO-08 | **Contenido externo invalido → `PresentButUnreadable`, nunca una excepcion** (C-3). Casos: JSON invalido o truncado; **surrogate suelto crudo** en el texto del NOD (`ArgumentException` de `Parse`); **surrogate suelto escapado** en un nombre de miembro, en `Name`, en `Value` y en un string anidado dentro de `ExtensionData` (`InvalidOperationException` al decodificar). El store no captura `Exception`, y las opciones de `Parse` son constantes con una prueba que fija su validez. **`Name` leido con un no-caracter** —el escape JSON de U+FFFE y el de U+FDD0— → `PresentButUnreadable`, sin excepcion y sin llegar a NFC (C-F1) |
| T-STO-09 | **Profundidad** (C-2): con profundidad ≤ 16 el documento es escribible; > 16 (p. ej. 20 o 40) se lee **`DepthLimitExceeded`**, no `PresentButUnreadable`, y rechaza crear, renombrar, cambiar valor, **eliminar**, vaciar y unificar. Un minor futuro con profundidad > 16 sigue siendo `DepthLimitExceeded`, no ilegible generico. Un major 2 mas profundo es `IncompatibleMajor`, porque la cota es del major 1. Lo que escribe V1 tiene profundidad 3 |
| T-STO-10 | **Nulos**: miembro `null` o elemento `Null` → `Absent`; `Value: null` o `Name: null` → `PresentButUnreadable` |
| T-STO-11 | **`JsonElement` Undefined** (`default`) → `Absent`, sin excepcion |
| T-STO-12 | **Nombres de miembro JSON repetidos a cualquier profundidad → `PresentButUnreadable`** (C-4). Casos: duplicado exacto en la raiz; variante de capitalizacion en la raiz (`Entries` y `entries`); duplicado dentro de una entrada (`Value` dos veces); duplicado dentro de un objeto anidado en `ExtensionData`; duplicado dentro de un objeto contenido en un array. En ningun caso el resultado depende de «gana el primero» o «gana el ultimo» |
| T-STO-13 | **Colision con `ExtensionData`**: al leer, ningun miembro declarado, en ninguna capitalizacion, acaba en `ExtensionData` de raiz ni de entrada; un `entries` solo se lee como `Entries` y se reescribe `Entries` |
| T-STO-14 | **Id duplicado** (mismo GUID en otra capitalizacion) → `AmbiguousIdentity`, distinto de ilegible; con otra invalidez estructural gana `PresentButUnreadable` |
| T-STO-15 | **GUID `D` estricto**: se aceptan 36 caracteres en minusculas y en mayusculas. Se rechazan espacio o tabulador alrededor, llaves, parentesis, formato `N`, 35 o 37 caracteres y `Guid.Empty`. Se escribe en minusculas |
| T-STO-16 | **Nombre Unicode NFC**: una entrada en NFD se normaliza al escribir; «Área» NFC frente a NFD colisiona en unicidad; tambien «Área» frente a «área»; el diagnostico de repetidos usa NFC; los nombres no tocados no se reescriben. **`Normalize(FormC)` nunca recibe UTF-16 mal formado** (C-3): un nombre con surrogate suelto se rechaza antes, y al leer ya no llega porque el store lo clasifica. **NFC solo recibe nombres acreditados** (C-F1): un `Name` nuevo con un no-caracter se rechaza antes de NFC, uno leido es `PresentButUnreadable`, y en ningun caso sale una excepcion de `Normalize` ni de `IsNormalized` |
| T-STO-17 | **Nombres repetidos** al leer → `Readable` con diagnostico |
| T-STO-18 | **Limites y validacion de escritura**: nombre de 1..80 tras NFC y recorte, sin controles; valor ≤ 1000 con CR, LF y tabulador; ≤ 50 entradas al crear. Leer nunca aplica limites (60 entradas o un valor de 1500 caracteres son `Readable`). Renombrar una entrada con un valor de 1500 caracteres se permite. **Eliminar una entrada que excede conteo o longitud se permite** (C-2). **`Name` o `Value` con UTF-16 mal formado → intent invalido**, y un valor aceptado se relee exacto: el serializador nunca lo sustituye por `U+FFFD` (C-3). **`Name` nuevo con U+FFFE o con U+1FFFE → intent invalido**, sin excepcion; **`Value` con U+FFFE → valido** y se relee exacto (C-F1) |
| T-STO-19 | **Guarda**: solo se escribe tras `Absent` o `Readable`; nunca tras un estado no escribible; no existe ninguna API de descarte |
| T-STO-20 | **Ida y vuelta**: conserva orden, ids, nombres y valores (Unicode, saltos de linea, pares de surrogates validos); siempre emite `SchemaVersion` y `Entries`; ids en minusculas; la version nunca se degrada. Una tabla de textos externos arbitrarios nunca produce excepcion, e **incluye** surrogate suelto crudo, surrogate escapado, `Name` invalido y `Value` invalido (C-3). Incluye ademas `Name` con U+FFFE y con U+FDD0 (`PresentButUnreadable`) y `Value` con U+FFFE (`Readable`, ida y vuelta exacta) (C-F1) |

### 12.2 ENVELOPE — Core, G4

| Id | Afirma |
|---|---|
| T-CHR-01 | **Caracterizacion de BASE, antes de tocar el sobre**: bytes de sobres representativos sin el miembro, con y sin `ExtensionData`, de cada kind |
| T-CHR-02 | Caracterizacion de BASE: `Compose(source)` hereda `SchemaVersion` y `ExtensionData`; bytes de `Compose(null)` |
| T-CHR-03 | Caracterizacion de BASE: ida y vuelta del store (campos desconocidos, minor mayor, major futuro → `null`) |
| T-CHR-04 | **Caracterizacion de BASE del residual F-14b** (C-F2), sobre el codigo de `BASE_SHA`. Con un sobre que lleva el escape JSON de un surrogate suelto dentro de un objeto con la clave `CustomProperties`, y otro que lo lleva en un campo desconocido: `Deserialize` devuelve un sobre legible; `Serialize` del mismo objeto lanza `JsonException`; `Compose(source)` seguido de `Serialize` lanza `JsonException`. El restamp **no** se prueba directamente, porque el Plugin no carga en las suites. Lo cubren esta propiedad del store (serializar el mismo objeto deserializado, como en T-CPY-01), la guarda estructural T-GRD-03 y el residual declarado en D-08.4 |
| T-ENV-01 | **Nulo omitido**: tras el cambio, los sobres sin miembro son byte a byte T-CHR-01 |
| T-ENV-02 | **Formas de `JsonElement`**: objeto, string, numero, array, `true` y `false` → sobre legible y texto crudo exacto; `null` literal → ausente y eliminado; el store clasifica cada forma |
| T-ENV-03 | **Corrupcion sintactica** o truncado dentro del miembro → sobre `null`, igual que en BASE: documenta el alcance del aislamiento |
| T-ENV-04 | **Profundidad**: 63 niveles en el miembro se leen y 64 dan `null`, en BASE y con el miembro; el escritor nunca pasa de 16 |
| T-ENV-05 | **Undefined y `Null`**: `Compose` y `WithCustomProperties` los normalizan a `null`; `Serialize` nunca lanza con un sobre del compositor |
| T-ENV-06 | **Clave del miembro repetida** en el JSON del sobre (residuales de D-08.4, C-4): con clave exacta gana la ultima, en BASE y con el miembro; con variante de capitalizacion, un DTO con la forma de BASE conserva ambas claves en `ExtensionData` y el sobre con el miembro se queda con la ultima. Ningun escritor RackCad las emite |
| T-ENV-07 | **Colision con `ExtensionData`**: el store nunca pone un miembro declarado del sobre en `ExtensionData`, ni exacto ni con otra capitalizacion; `Compose` y `WithCustomProperties` rechazan un origen con colision antes de producir nada |
| T-ENV-08 | **Preservacion en `Compose`**: con `source` conserva el miembro exacto; dos vistas conservan cada una la suya; con `Compose(null)` no hay miembro |
| T-ENV-09 | **`WithCustomProperties`** cambia solo el miembro y conserva los demas y `ExtensionData`; version sin degradar |
| T-ENV-10 | **Caracterizacion de build anterior**: un DTO local con la forma de BASE guarda el miembro en `ExtensionData` y lo reemite exacto, por `Compose` y por restamp del mismo objeto |
| T-ENV-11 | La exportacion a biblioteca no lee ni escribe el miembro |
| T-ENV-12 | **`Compose(null)` solo en los casos esperados**: T-GRD-02 clasifica las siete llamadas; los caminos del Plugin, en OV-03, OV-04, OV-07, OV-11 y OV-13 |
| T-ENV-13 | **Residual F-14b con el miembro** (C-F2): el caso de T-CHR-04 con la variante de I-54 se comporta igual que en BASE. El sobre se lee; `Serialize` del mismo objeto y `Compose(source)` seguido de `Serialize` lanzan `JsonException`; el store de propiedades clasifica el miembro `PresentButUnreadable` sin lanzar. Documenta que INV-07 excluye F-14b |
| T-GRD-01 | Guarda de construccion (D-19), con rojo |
| T-GRD-02 | Censo de `Compose` (D-19), con rojo |

### 12.3 AUTHORITY — Core, G5

| Id | Afirma |
|---|---|
| T-AUT-01 | Todos los miembros legibles e iguales → `Single` |
| T-AUT-02 | **Ausente ≡ vacio con cualquier minor**: `{ausente, vacio@1.0, vacio@1.3}` → `Single`, transitivo. Caso entre builds: coleccion vacia escrita `1.0` por I-54 frente a vacia leida a `1.1` |
| T-AUT-03 | **Divergencia legible** en valor, nombre, orden, id o conteo → `Divergent`, con resumen por vista |
| T-AUT-04 | **Divergencia de `ExtensionData`** de raiz o de entrada → `Divergent`; igualdad en profundidad sin importar el orden de miembros, con objetos anidados y arrays de objetos; numeros por texto crudo. Un documento con nombres repetidos a cualquier profundidad nunca llega a compararse: es `PresentButUnreadable` (C-4) |
| T-AUT-05 | Solo cambia el minor, con contenido identico → `Single`; version de escritura = minor mayor |
| T-AUT-06 | **Payload ilegible** en cualquier parte, colocado y de otro rack → `IndeterminateMembership` para todo rack; el diagnostico lista bloque, handle y colocacion |
| T-AUT-07 | **Payload ilegible no colocado** → tambien `IndeterminateMembership` (V1 lo ignoraba) |
| T-AUT-08 | **`RackId` en blanco** en un sobre legible → no es miembro ni bloquea; elegido → `NoIdentity` |
| T-AUT-09 | **`Kind` en blanco** en un miembro → `MixedKind`, con diagnostico de la definicion; incluye la forma del payload Selectivo anterior al sobre |
| T-AUT-10 | **Matriz de kind** (C-1), con `isKnownKind` inyectado en la prueba. (a) Kind conocido y comun → continua y, con colecciones iguales, es escribible (`Single`). (b) Algun `Kind` en blanco → `MixedKind`. (c) Dos kinds conocidos distintos → `MixedKind`. (d) Un kind conocido y uno desconocido → `MixedKind`. (e) Un kind desconocido comun → **`UnknownKind`**, solo lectura, sin crear, renombrar, cambiar valor, eliminar ni unificar, y sin tocar el sobre. (f) Una diferencia solo de mayusculas es el mismo kind |
| T-AUT-11 | **xref**: una definicion dependiente no es miembro, no bloquea y no entra en un plan; elegir una xref → `XrefRejected` |
| T-AUT-12 | Coleccion de un miembro `PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` o **`DepthLimitExceeded`** → `CustomPropertiesReadOnly`, solo lectura, sin unificar ni escribir; nunca se omite ese miembro (C-2) |
| T-AUT-13 | **Sin hermana arbitraria**: ningun resultado distinto de `Single` expone filas de una vista como del rack; permutar el orden del barrido no cambia el resultado |
| T-AUT-14 | **Matriz de unificacion**: disponible solo segun D-09.10. La bloquean el `ExtensionData` de raiz o de entrada en una vista que se sobrescribiria, un minor mayor en ella, cualquier miembro no escribible (incluido `DepthLimitExceeded`) y **`UnknownKind`** (C-1, C-2). Se permite como origen una vista con `ExtensionData` si las demas estan limpias. **Origen `Absent`**: el documento origen generado es el vacio canonico (`CurrentSchemaVersion`, `Entries: []`, sin `ExtensionData`) y su minor es el actual (C-5A) |
| T-AUT-15 | **Orden unico** de D-09.8 (C-1, C-2): con varias causas a la vez gana siempre la primera de `XrefRejected` → `NoIdentity` → `IndeterminateMembership` → `MixedKind` → `UnknownKind` → `CustomPropertiesReadOnly` → `Divergent` → `Single`; el resultado es determinista e independiente del orden del barrido |
| T-AUT-16 | **Workspace**: estados y filas por alcance; filas solo con `Editable`; en otro caso, resumenes por vista |

### 12.4 MUTATIONS — Core, G3 puras y G5 commit

| Id | Afirma |
|---|---|
| T-MUT-01 | **Crear**: id nuevo en minusculas `D`, al final, nombre NFC recortado, limites |
| T-MUT-02 | **Renombrar**: solo cambia `Name`; `Value` y `ExtensionData` de la entrada exactos; se permite cambiar solo mayusculas; colision con otra entrada → rechazo. **`Id` igual por valor; su representacion persistida se escribe en `D` minusculas** (C-6): con un id de origen en `D` mayusculas, la identidad compara igual y el resultado persistido esta en minusculas. No hay prueba byte a byte del texto original del `Id` |
| T-MUT-03 | **Cambiar valor**: solo cambia `Value`; `ExtensionData` de la entrada exacto |
| T-MUT-04 | **Eliminar**: quita solo esa entrada y su `ExtensionData`; el resto exacto |
| T-MUT-05 | **Vaciar**: eliminar la ultima deja `Entries: []` y conserva `ExtensionData` de raiz y version; el contenedor permanece |
| T-MUT-06 | **Conservar el `ExtensionData` de entrada** al renombrar y cambiar valor en un documento `1.7` |
| T-MUT-07 | **Instantanea obsoleta**: con el intent calculado sobre la instantanea, la lectura fresca difiere (entrada borrada, nombre ya ocupado, miembro ilegible, payload ilegible nuevo en el dibujo, autoridad `Divergent`, miembro añadido o eliminado, kind que pasa a desconocido, coleccion que pasa a `DepthLimitExceeded`, coleccion origen cambiada o **cualquier coleccion destino cambiada**) → abortar sin plan (C-1, C-2, C-5B) |
| T-MUT-08 | **Plan y atomicidad logica** (C-5C): al editar incluye todos los miembros; al unificar, solo los distintos del origen; nunca dependientes de xref; cada payload se deserializa al sobre fresco de su miembro, que solo cambia en el miembro de propiedades. Un BTR ya igual al origen **no** se reescribe fisicamente, y tras el commit **todos** los miembros son canonicamente iguales al estado destino. **Fallo al serializar** (C-F2, D-22.10): si el sobre fresco de un miembro no se puede reserializar (F-14b en un campo desconocido), `Application.Commit` no devuelve plan y la excepcion se propaga, asi que no se produce ningun payload ni se escribe ningun miembro. En el Plugin, T-GRD-04 fija una sola transaccion con `Commit` al final |
| T-MUT-09 | **Intent por id**: un id inexistente se rechaza; el nombre nunca localiza |
| T-MUT-10 | **Commit de unificacion** (C-5): con todas las vistas mostradas iguales en la lectura fresca → exito; con el origen cambiado → abortar; con **cualquier** destino cambiado → abortar; con el origen `Absent` → se escribe el vacio canonico solo en los miembros con contenido distinto; condiciones de D-09.10 reevaluadas |

### 12.5 COPY

| Id | Afirma | Gate |
|---|---|---|
| T-CPY-01 | Propiedad del store de produccion de la que depende el restamp (D-11.5) | G4 |
| T-CPY-02 | = T-GRD-03, guarda estructural del restamp, con rojo y conviviendo con G-R5 | G4 |
| OV-05 | `RACKDUPLICAR` multivista y con varios origenes; `RACKLAYOUT` independiente; original y copia independientes | G9 |
| OV-06 | `COPY` nativo: misma autoridad | G9 |

### 12.6 PROJECT — Core, G6

| Id | Afirma |
|---|---|
| T-PRJ-01 | Lector del NOD: constante de clave, tri-estado y ningun camino que lance (guarda sobre `CustomPropertiesData.cs`); fisico en OV-01 y OV-10 |
| T-PRJ-02 | **Independiente de un `RACKCAD_PROJECT` corrupto**: lectura, escritura y workspace de propiedades no referencian Project Variables (T-GRD-05), y sus entradas son solo el payload de propiedades |
| T-PRJ-03 | **`RACKCAD_PROJECT` independiente de propiedades corruptas**: el diff del candidato no toca `ProjectVariables*` y esos archivos no referencian propiedades (T-GRD-05) |
| T-PRJ-04 | Ejecutor de Proyecto: releer, acreditar, aplicar por id, guarda, escribir; una transaccion; sin regen |
| T-GRD-04..07 | D-19 |

### 12.7 UI — UI tests, G7

| Id | Afirma |
|---|---|
| U-01 | Filas `Nombre — Valor` de una coleccion `Editable` |
| U-02 | **Estados de solo lectura**: `XrefRejected`, `NoIdentity`, pertenencia indeterminada, `MixedKind`, **`UnknownKind`**, `PresentButUnreadable`, `AmbiguousIdentity`, `IncompatibleMajor` y **`DepthLimitExceeded`** deshabilitan el editor, eliminar incluido, y muestran el motivo desde la apertura (C-1, C-2) |
| U-03 | **Sin descarte destructivo**: ningun estado tiene un control de descarte |
| U-04 | **Unificar seguro**: el panel solo aparece si esta disponible, muestra el contenido por vista, no tiene origen por defecto y exige la casilla; una vista `Absent` elegida como origen se muestra como «vacio / sin propiedades» (C-5A) |
| U-05 | **Intent por id**: Nueva, Renombrar, Cambiar valor y Eliminar producen intents con id; la ventana nunca muta colecciones |
| U-06 | **La UI no es autoridad**: la ventana solo presenta el workspace y no referencia store, autoridad ni commit |
| U-07 | `Divergent`: resumenes por vista, sin filas de rack |
| U-08 | Aviso de nombres repetidos |
| U-09 | Sin `MessageBox`; Cerrar y Escape segun ADR-0029 |
| U-10 | Arquetipo C declarado; T-GRD-08 |

### 12.8 Validacion del Owner — AutoCAD 2025, G9

| # | Escenario |
|---|---|
| OV-01 | Proyecto: crear, renombrar, cambiar y eliminar; guardar, cerrar y reabrir |
| OV-02 | Selectivo con frontal, laterales y planta: crear propiedades; todas las vistas muestran lo mismo; reabrir |
| OV-03 | `RACKEDITAR` → Actualizar en los seis sistemas conserva las propiedades |
| OV-04 | Insertar vista nueva desde `RACKEDITAR` hereda las propiedades |
| OV-05 | `RACKDUPLICAR` multivista y con varios origenes, y `RACKLAYOUT` independiente, llevan las propiedades; editar las de la copia no cambia el original, ni al reves |
| OV-06 | `COPY` nativo comparte las propiedades: editar en una copia se ve en la otra |
| OV-07 | `RACKVARIABLES` con un Selectivo vinculado: cambiar un valor conserva las propiedades |
| OV-08 | `UNDO` tras escribir propiedades de Proyecto y de Rack: se **observa y registra** la granularidad, sin afirmarla antes |
| OV-09 | Copiar un rack a otro DWG: lleva las del rack, no las del proyecto |
| OV-10 | `PURGE` y `AUDIT` tras escribir: la entrada del NOD sigue |
| OV-11 | Biblioteca: guardar → las propiedades no viajan; abrir o insertar → el rack nace sin propiedades |
| OV-12 | `RACKLISTA` y `RACKBOMTOTAL` sin cambios |
| OV-13 | Un rack nuevo desde el menu nace sin propiedades |
| OV-14 | Un DWG con racks adjunto como xref: sus racks no se editan ni bloquean los del dibujo anfitrion |

**No se fabrican para el Owner** divergencia, ilegibilidad, identidad ambigua, major futuro, `Kind` en blanco,
**kind desconocido, documentos de profundidad > 16, UTF-16 mal formado, nombres repetidos, no-caracteres en
`Name`, sobres con surrogate escapado** ni payloads no colocados ilegibles (regla de I-47 y V5-R07 de I-48). Los
cubren T-STO, T-CHR, T-ENV, T-AUT y T-MUT.

### 12.9 Trazabilidad V2 → V3 (C-1..C-6)

| Prueba | Cambio | Que añade V3 |
|---|---|---|
| T-STO-08 | C-3 | Surrogate suelto crudo y escapado (nombre de miembro, `Name`, `Value`, anidado); clases capturadas explicitas |
| T-STO-09 | C-2 | `DepthLimitExceeded` en lugar de rechazo al escribir; eliminar bloqueado; minor futuro profundo; major 2 profundo |
| T-STO-12 | C-4 | Duplicados a cualquier profundidad: raiz exacta, raiz por capitalizacion, entrada, `ExtensionData` anidado, objeto en array |
| T-STO-16 | C-3 | NFC nunca recibe UTF-16 mal formado |
| T-STO-18 | C-2, C-3 | Eliminar una entrada que excede limites; `Name`/`Value` mal formados rechazados; sin sustitucion por `U+FFFD` |
| T-STO-20 | C-3 | La tabla externa incluye los cuatro casos de UTF-16 |
| T-ENV-06 | C-4 | Residual por clave exacta y por variante de capitalizacion |
| T-AUT-04 | C-4 | Igualdad con anidados; los duplicados no llegan al comparador |
| T-AUT-10 | C-1 | Matriz de kind con `UnknownKind` (**invertida** respecto de V2) |
| T-AUT-12 | C-2 | `DepthLimitExceeded` y el nombre `CustomPropertiesReadOnly` |
| T-AUT-14 | C-1, C-2, C-5 | Unificacion bloqueada por `UnknownKind` y `DepthLimitExceeded`; origen `Absent` canonico |
| T-AUT-15 | C-1, C-2 | Orden unico de ocho resultados |
| T-MUT-02 | C-6 | Id igual por valor, persistido en minusculas; sin prueba byte a byte |
| T-MUT-07 | C-1, C-2, C-5 | Kind desconocido, profundidad y destino cambiado abortan |
| T-MUT-08 | C-5 | Atomicidad logica: BTR iguales no reescritos |
| T-MUT-10 | C-5 | Todas las vistas revalidadas; origen `Absent` |
| U-02 | C-1, C-2 | `UnknownKind` y `DepthLimitExceeded` |
| U-04 | C-5 | Origen «vacio / sin propiedades» |

### 12.10 Trazabilidad V1 → V2

| V1 | V2 | Disposicion |
|---|---|---|
| T-01 | T-STO-01 | igual |
| T-02 | T-STO-20 | ampliado |
| T-03 | T-STO-03 | precisado (forma exacta) |
| T-04 | T-STO-05 | igual |
| T-05 | T-STO-06 | igual |
| T-06 | T-STO-07 | igual |
| T-07 | T-STO-04, T-STO-08 | dividido |
| T-08 | T-STO-10, T-STO-15 | precisado |
| T-09 | T-STO-14 | precisado (resultado distinto y precedencia) |
| T-10 | T-STO-17 | igual |
| T-11 | T-STO-19 | **sin descarte** |
| T-12 | T-STO-20 | fusionado |
| T-13 | T-CHR-01, T-ENV-01 | caracterizacion antes |
| T-14 | T-ENV-02 | fusionado |
| T-15 | T-ENV-02..05 | **acotado** |
| T-16 | T-ENV-08 | igual |
| T-17 | T-ENV-08 | fusionado |
| T-18 | T-ENV-09 | igual |
| T-19 | T-ENV-10 | igual |
| T-20 | T-CPY-01 | **reformulado**: store de produccion, no restamp |
| T-21 | T-GRD-03 | **estructural**, convive con G-R5 |
| T-22 | T-ENV-11 | igual |
| T-23..T-30 | T-AUT-01..15 | **pertenencia sin filtro de colocacion**; `Kind` en blanco; xref; minor |
| T-31..T-35 | T-STO-16, T-STO-18, T-MUT-01..10, T-AUT-14 | **sin descarte**; unificacion segura; revalidacion |
| T-36 | T-AUT-16 | ampliado |
| T-37..T-44 | T-GRD-04..08, T-PRJ-01..04 | ampliado |
| U-01..U-10 | U-01..U-10 | **sin descarte**; solo lectura; unificacion segura |
| OV-01..OV-12 | OV-01..OV-12 | OV-05, OV-08 y OV-11 precisados |
| — | T-STO-09, -11, -12, -13, -16; T-ENV-03..07, -12; T-AUT-02, -05..11, -13; T-MUT-06, -07; T-GRD-01, -02, -05, -06; OV-13; OV-14 | **nuevos** |

### 12.11 Trazabilidad V3 → V4 (C-F1, C-F2)

| Prueba | Cambio | Que añade V4 |
|---|---|---|
| T-STO-08 | C-F1 | `Name` leido con U+FFFE o con U+FDD0 → `PresentButUnreadable`, sin llegar a NFC |
| T-STO-16 | C-F1 | NFC solo recibe nombres acreditados; ninguna excepcion de `Normalize` ni de `IsNormalized` |
| T-STO-18 | C-F1 | `Name` nuevo con U+FFFE o con U+1FFFE → intent invalido; `Value` con U+FFFE valido y releido exacto |
| T-STO-20 | C-F1 | La tabla de textos externos incluye no-caracteres en `Name` y en `Value` |
| T-CHR-04 | C-F2 | Caracterizacion de BASE del residual F-14b: el sobre se lee y reescribirlo lanza; el restamp queda cubierto por la propiedad del store, T-GRD-03 y el residual declarado |
| T-ENV-13 | C-F2 | El mismo residual con el miembro se comporta igual que en BASE; el store no lanza |
| T-MUT-08 | C-F2 | Si una serializacion lanza: sin plan, sin payloads y sin escrituras |

## 13. Ramas paralelas (medidas en G2G, tras el rebase)

Solo se mide el impacto sobre C-F1 y C-F2 y sobre las costuras que tocan: la validacion de `Name`, NFC y la
reserializacion del sobre. Lo demas quedo medido en V3 §13 y acordado en G2F.

| Rama | SHA al publicar V4 (completo) | En G2F | Cambio desde G2F | Impacto en C-F1 y C-F2 | Conflictos | Supuestos invalidados |
|---|---|---|---|---|---|---|
| `main` | `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` | `f8deb67` | Ninguno; es la base del rebase (§2) | Ninguno: `RackEmbedDocument.cs`, `RackEmbedComposer.cs` y `RackEnvelopeRestamp.cs` son identicos a los de `BASE_SHA` | Ninguno | Ninguno |
| I-49 `architecture/motor-expresiones-parametricas` | `1ed93a0525ec11c5092df89c55cbf498c99f8e2a` | `048a508` | Rebasada sobre `main`; `I-49-proposal-v6.md` conserva su blob; sin `src/`, `tests/` ni `assets/` propios | Ninguno: no cambia ninguna regla que use I-54; sigue reservando `Rack` y `Project` (`I-49-proposal-v6.md:622-624`) | Documental: fila de ROADMAP y numeracion de ADR | Ninguno |
| I-50 | integrada en `main` (`f8deb67`); rama remota retirada | — | — | Ninguno | — | Ninguno |
| I-52 `feature/rackmirror-espejo-semantico` | `545c2229de8d7850e03981d85aced0ac63c24040` | `0445718` | +1 commit **solo docs**: Proposal V3 y ADR-0036 actualizado; base `46fcac2` | Sin cambio en la reserializacion: el espejo sigue reflejando, componiendo con `Compose(sobreFuente, …)`, serializando con `RackEmbedStore` y re-estampando (`I-52-proposal-v3.md:565-566,1315-1317`), asi que F-14a y F-14b le afectan como a los demas flujos que reescriben el sobre. `CustomProperties` sigue siendo portador (`:669,1039`) | Textual: censos de comandos y de `Compose`; ADR-0036 | Ninguno |
| I-53 `feature/cabeceras-configurables-multidestino` | `4e00a273e22634cb3abbc1b3fb3778edf49dbc7e` | `5efaf7e` | +1 commit de G3: 4 archivos nuevos en `src/RackCad.Application/Systems/Shared/` (`HeaderBatch*` y `HeaderConfigurationSnapshot`) y 3 de pruebas; base `f8deb67` | Ninguno: no toca sobre, `Compose`, Custom Properties, NFC, UI ni guardas de censo | Documental al integrar: ROADMAP, `adr/README.md` e `ideas-futuras.md` | Ninguno |

Medido con `git fetch --all --prune` tras el rebase y de nuevo inmediatamente antes de publicar V4.

## 14. Gates propuestos (no autorizados · V2-21)

| Gate | Contenido | Evidencia |
|---|---|---|
| G2B | Architect Review de V1 | **CERRADO**: AGREED WITH CHANGES @ `7c197af` |
| G2C | Proposal V2 | **HECHO**: `36c337b`, CI verde |
| G2D | Revision exact-SHA de V2 | **CERRADO**: Arquitecto AGREED WITH CHANGES @ `36c337b` (C-1..C-8); el Coordinador acepta C-1..C-8 |
| G2E | Proposal V3 | **HECHO**: `5d25da8` (pre-rebase), CI 34737707481 verde; equivalente post-rebase `ff98b9e` (§2.2) |
| G2F | Revision exact-SHA de V3, limitada a C-1..C-8 | **CERRADO**: Arquitecto AGREED WITH CHANGES @ `5d25da8` (AR-54-V3-01, AR-54-V3-02, MINOR); Coordinador AGREED; el Coordinador acepta la lista cerrada de cambios |
| G2G | Rebase obligatorio sobre `origin/main` y esta Proposal V4 (C-F1, C-F2) | commit documental, `--force-with-lease` y CI del SHA exacto |
| G2H | Revision exact-SHA post-rebase de V4 por Coordinador y Arquitecto, **limitada a C-F1, C-F2 y la integridad del rebase** | veredictos sobre el mismo SHA |
| G2-FREEZE | Consenso sobre el mismo SHA; contrato actualizado; **ADR `propuesto`** con el alcance de §15; F-01..F-13, **F-14a y F-14b** (residuales del `RackEmbedStore` al leer y al reescribir, D-08.4) y la mejora de D-11.6 en `ideas-futuras.md`; aprobacion del Owner, informado de R-12 | commit documental y CI |
| G3 | Documento, store, resultados (incluido `DepthLimitExceeded`), clasificacion de contenido externo, guarda y mutaciones puras | T-STO-01..20, T-MUT-01..06, T-MUT-09: rojo → verde |
| G4 | **Primero la caracterizacion de BASE** (T-CHR-01..04, con el residual F-14b de C-F2), verde sobre el codigo de `BASE_SHA` y commiteada; **despues**, produccion: miembro del sobre, herencia en `Compose`, `WithCustomProperties` y restricciones A..G; **guardas** T-GRD-01..03 con rojo demostrado | T-CHR, T-ENV, T-CPY-01 |
| G5 | Pertenencia (indeterminada, `Kind` en blanco, xref), `UnknownKind` con `isKnownKind` inyectado, autoridad con el orden unico de D-09.8, preflight, commit sobre proyeccion fresca y workspace | T-AUT-01..16, T-MUT-07, -08, -10 |
| G6 | Plugin: datos del NOD, ejecutores de Proyecto, Rack y unificar, construccion del predicado `isKnownKind` desde el registro de handlers, y guardas T-GRD-04..07; build del Plugin. **Sin smoke fisico**: el comando aun no existe | T-PRJ-01..04 |
| G7 | Nombre del comando decidido por el Owner; ventana, comando, **censo real verificado** y actualizado, y ayuda | U-01..10, T-GRD-08 |
| G8 | Candidato: Core y UI completas en local, CI 4/4 sobre el SHA exacto, Debug de UI y Plugin | AGENTS.md |
| G9 | **Toda** la validacion fisica, en AutoCAD 2025 | OV-01..14 |
| G10 | Integracion (WORKFLOW 4.5) | CI del merge y cobertura |

## 15. ADR Candidate Scope

> **No es el ADR.** No tiene numero ni texto. Es el material que el Consensus Freeze debe congelar en un ADR
> `propuesto` antes de G3, con este titulo de trabajo: **«Contrato de persistencia y autoridad de Custom Properties»**.
> Cumple los criterios 1, 2 y 3 de `docs/adr/README.md:19-26`.

**Material de freeze.** Dieciseis puntos, en el orden de la revision de G2D; los cambios de C-7 van marcados
**[7A]**, **[7B]** y **[7C]**.

1. **Modelo e identidad.**
   - Registros `{ Id, Name, Value }`, con `ExtensionData` en la raiz y por entrada.
   - `Id` GUID en forma `D` exacta y escrita en minusculas; igualdad por valor; ids duplicados =
     `AmbiguousIdentity`.
   - Identidad local al alcance.
   - El nombre solo presenta: nunca identidad, busqueda ni agregacion.
2. **Regla de evolucion normativa** (D-04.3..5) y su confrontacion con ADR-0034 §3. Incluye que cambiar la cota de
   profundidad es MAJOR **[7A]** y que hacer depender la coleccion del `Kind` es MAJOR **[7B]**.
3. **Contenedor de Proyecto.** Xrecord directo `RACKCAD_CUSTOM_PROPERTIES` en el NOD, con la clave congelada;
   lectura tri-estado; version primero; independiente de `RACKCAD_PROJECT` en ambos sentidos.
4. **Contenedor de Rack.** Miembro `CustomProperties` del sobre, de tipo `JsonElement?`, omitido cuando es
   nulo; `Compose` lo hereda y `WithCustomProperties` escribe.
5. **Contrato de aislamiento de `JsonElement?`.**
   - La afirmacion acotada de D-08.4 y las restricciones A..G.
   - El store de propiedades nunca lanza por contenido externo.
   - Los residuales preexistentes, declarados.
   - **[7A]** La cota de profundidad **16** es una **constante del formato 1.x**, no tuning:
     - todo escritor 1.x produce documentos de profundidad ≤ 16;
     - subirla exige MAJOR;
     - un documento legible mas profundo es `DepthLimitExceeded`, de solo lectura.
6. **Sin promocion de major del sobre**, con la matriz verificada de D-08.5.
7. **Invariante de preservacion del sobre** (D-21): `Compose(source)` o restamp del mismo objeto;
   `Compose(null)` solo para rack nuevo e importacion.
8. **Autoridad entre hermanas por `RackId`.**
   - Pertenencia por `Id`. Nunca se elige hermana, y todo resultado distinto de `Single` es de solo lectura, salvo
     la unificacion segura.
   - **[7B] Kind.** La semantica de una propiedad personalizada **no** depende del `Kind`. **Pero** la autoridad de
     I-54 exige que el `Kind` sea conocido para permitir escrituras:
     - `Kind` en blanco = `MixedKind`;
     - kinds distintos = `MixedKind`;
     - un kind no vacio desconocido = `UnknownKind`, de solo lectura.

     Asi I-54 nunca es el primer comando que reescribe un kind futuro.
   - **[7C] Igualdad canonica.**
     - Se compara el contenido completo del documento.
     - El **minor** queda **excluido**; el **major** si participa.
     - Participan `ExtensionData` de raiz y de entrada y el orden de las entradas.
     - Los ids se comparan por valor de `Guid`, y `Name` y `Value` de forma ordinal.
     - Nombres de **miembro JSON** repetidos en cualquier objeto y a cualquier profundidad = ilegible
       (`PresentButUnreadable`). No confundir con dos entradas que comparten `Name`, que se leen con diagnostico.
     - Ausente ≡ documento vacio canonico compatible.
9. **Pertenencia indeterminada.** Todo payload RackCad no interpretable, colocado o no, bloquea las escrituras
   de Rack. Un `Id` en blanco no es miembro; las dependientes de xref quedan excluidas.
10. **Escritura atomica.**
    - Una transaccion del llamador.
    - Tras el commit, todos los miembros quedan canonicamente iguales al estado destino, o ninguno cambia; es
      atomicidad logica, sin exigir reescribir un BTR que ya esta en el estado destino.
    - Decide Application sobre la lectura fresca.
    - Sin redefinir, importar, purgar ni regenerar.
11. **`UNKNOWN != EMPTY`**, en los dos contenedores.
12. **Sin recuperacion destructiva en V1.** Cualquier recuperacion futura exige una decision explicita del Owner.
13. **Matriz de compatibilidad** (§9).
14. **Fronteras de V1**: biblioteca, dibujo, expresiones, plantillas, Drawing-level, vista y agregacion.
15. **Sin sintaxis de I-49**: no reclama `Rack` ni `Project` de ID20.
16. **Referencias futuras solo por identidad estable.**

**Relacion con otros ADR.** Aplica ADR-0009, ADR-0010 y ADR-0034 sin reemplazarlos. Acota, **sin reabrirla**, la
frase de ADR-0035 sobre `RackEmbedComposer` (aceptado e integrado en `main` @ `f8deb67`, `:97-102`): vale para
campos declarados que `Compose` no hereda, y `CustomProperties` si se hereda.

Es compatible con ADR-0036 de I-52 (`propuesto` y corregido en su rama @ `0445718`). Ese ADR declara intactos los
portadores de las iniciativas integradas, propiedades de rack incluidas (`:88-90`), y su orden reflejar → `Compose`
→ restamp cumple el invariante de preservacion. El numero del ADR de I-54 se asigna al redactarlo; hoy 0035 esta
aceptado en `main`, y 0036 (I-52) y 0037 (I-53) estan tomados en sus ramas.

**Excluido del ADR.** La UI, los nombres de comando, los **limites numericos de tuning** (50 entradas, 80 y 1000
caracteres), los gates y el tuning. **La cota de profundidad 16 no esta excluida**: es constante del formato
**[7A]**.

## 16. Preguntas al Owner reclasificadas (V2-22)

Ninguna bloquea el consenso. **Sin cambios en V3 ni en V4**, salvo la aclaracion de OQ-02 hecha en V3, que no
cambia su clasificacion.

| # | Pregunta de V1 | Clasificacion V2 | Resolucion o motivo | Gate limite |
|---|---|---|---|---|
| OQ-01 | ¿Alguna propiedad de rack viaja con la biblioteca? | **DEFER** | Biblioteca fuera de V1 como frontera (D-12), con camino aditivo | Ninguno en I-54; la iniciativa futura de biblioteca |
| OQ-02 | Limites 50 / 80 / 1000 | **ARCHITECTURE** + **DEFER** | La **existencia** de limites, solo al escribir y sin ser reglas de formato, es arquitectura resuelta (D-05). Los **numeros** son tuning y no bloquean el schema. La cota de profundidad 16 **no** pertenece a esta pregunta: es constante del formato 1.x (D-05.7, C-2) | Valores por defecto en G3; ajuste del Owner a mas tardar en G9 |
| OQ-03 | Nombre y alias del comando, boton de menu | **DEFER** | Nombres de trabajo sin congelar (D-18.1) | **Antes de G7** |
| OQ-04 | ¿Se copian todos los valores al duplicar? | **DEFER** | V1 copia todo (D-11.4, ADR-0034 §12); una politica por propiedad se difiere | Iniciativa futura |
| OQ-05 | ¿Hay builds anteriores a I-11? | **ARCHITECTURE** | Ya no es pregunta: promover no protege a esos builds (D-08.5). Queda como riesgo residual R-12 | — (informado en el freeze) |
| OQ-06 | ¿Reordenar entra en V1? | **DEFER** | El orden ya se persiste y reordenar es aditivo | Iniciativa futura |
| OQ-07 | ¿ID24 y ID20 comparten espacio de nombres? | **DEFER a ID20** | V2 no reclama sintaxis ni namespace (D-16) | ID20 |
| OQ-08 | ¿ID24 es exactamente metadatos de Proyecto y Rack? | **RESOLVED** | Lo fijan el contrato y el objetivo del Coordinador; D-03.2 precisa Proyecto frente a Drawing-level | — |
| OQ-09 | ¿Editar desde los editores de sistema en V1? | **DEFER** | V1 usa UI separada; el workspace es reutilizable | Iniciativa futura |

## 17. Matrices de reconciliacion

### 17.1 Cambios de G2F: C-F1, C-F2 y trazas

| Cambio de G2F | Cambio V4 | Hallazgo | Secciones V4 | Disposicion | Notas |
|---|---|---|---|---|---|
| **1** `Name` no normalizable | **C-F1** | AR-54-V3-01; AR-54-V2-03 (3B) | §1, P-32, P-35, D-05.1, D-05.2, D-05.4, D-07.4 (paso 3 y tabla de excepciones), D-22.5, INV-23, T-STO-08, T-STO-16, T-STO-18, T-STO-20, §12.11 | **INCORPORATED** | Regla determinista de no-caracteres (U+FDD0–U+FDEF y todo U+nFFFE/U+nFFFF), comprobada con `Rune` antes de NFC; `PresentButUnreadable` al leer; `Value` sin cambios; defensa acotada de `ArgumentException` documentada, sin `catch (Exception)` |
| **2** Residual de reescritura con surrogate escapado | **C-F2** | AR-54-V3-02; AR-54-V2-03 (3C) | P-31, P-36, D-08.4 (A y residuales F-14a y F-14b), D-13, D-22.10, INV-07, RP-11, T-CHR-04, T-ENV-13, T-MUT-08, §12.11, §14 (G4 y G2-FREEZE) | **INCORPORATED** | Preexistente y sin arreglar; garantias acotadas a sobres que BASE puede leer y reserializar; sin commit ni escritura si una serializacion lanza; caracterizacion de BASE y de la variante, sin fingir una prueba directa del restamp |
| **3** Trazas y revision limitada | Trazas | — | §0, §2, §13, §14, §17..§22, paquete y contrato | **INCORPORATED** | Rebase con mapa de SHAs pre y post (§2.2) e integridad verificada (§2.3); paquete limitado a C-F1, C-F2 y el rebase |

**Contradicciones entre C-F1, C-F2 y lo acordado en V3: ninguna.** Se revisaron:
- **C-F1/C-4**: la regla de no-caracteres va en la validacion de entradas (D-07.4 paso 3), despues del recorrido que
  detecta duplicados y strings no decodificables.
- **C-F1/C-2**: un documento major 2 con no-caracteres sigue siendo `IncompatibleMajor`, porque el paso 3 va despues
  del major.
- **C-F1/C-6**: la regla afecta a `Name`, no a `Id`.
- **C-F2/C-5**: la unificacion segura sigue en una transaccion; D-22.10 añade que un fallo de serializacion no deja
  escrituras.
- **C-F2/C-3**: F-14a y F-14b son del `RackEmbedStore`; el store de propiedades sigue sin lanzar.
- **C-F1 y C-F2/§15**: el alcance del ADR no cambia; §15.5 ya exige un store que no lanza y residuales declarados.

### 17.2 Cambios de G2D: C-1..C-8 (verificados en G2F)

| Cambio | Hallazgo | Secciones V3 | Disposicion | Notas |
|---|---|---|---|---|
| **C-1** `Kind` desconocido | AR-54-V2-01 | §1, P-28, P-29, D-04.3, D-09.5, D-09.8, D-09.10, D-09.11, D-18.3, D-18.5, D-22.2, D-22.3, D-22.5, D-13 (nota), INV-22, §8, §9, RP-09, T-AUT-10, T-AUT-14, T-AUT-15, T-MUT-07, U-02, §15.2, §15.8, §19, §21 | **INCORPORATED** | `UnknownKind` de solo lectura, evaluado tras `XrefRejected`, `NoIdentity`, `IndeterminateMembership` y `MixedKind`, y antes del estado de las colecciones; `isKnownKind` inyectado (precedente `RackDuplicationPlan.Build`); Application sin `KindHandlerRegistry`; **PR-12 rechazada**; `Kind` en blanco = `MixedKind` se mantiene |
| **C-2** Cota de profundidad | AR-54-V2-02, AR-54-04 | §1, P-30, D-04.3, D-05.6, D-05.7, D-07.3, D-07.4, D-08.4 B, D-09.6, D-09.10, D-09.11, D-10.1, D-13, D-18.3, D-18.5, D-22.5, INV-06, INV-20, §9, RP-10, T-STO-09, T-STO-18, T-AUT-12, T-AUT-14, T-MUT-07, U-02, §15.2, §15.5, OQ-02 (aclaracion) | **INCORPORATED** | 16 = constante del formato 1.x, subirla exige MAJOR; `DepthLimitExceeded` no es `PresentButUnreadable` ni `IncompatibleMajor`; ninguna escritura sobre el; «eliminar siempre se permite» solo frente a conteo y longitud |
| **C-3** UTF-16 y excepciones | AR-54-V2-03 | §1, P-31, P-32, D-05.1, D-05.2, D-05.4, D-07.4, D-08.4 A y G con residuales, D-10.1, D-22.5, INV-23, RP-11, T-STO-08, T-STO-16, T-STO-18, T-STO-20, §14 (F-14) | **INCORPORATED** | **3A:** clases capturadas explicitas, sin `catch (Exception)`. **3B:** validacion antes de NFC y de serializar. **3C:** residual preexistente del sobre declarado y no arreglado. F-14 se registra en G2-FREEZE, porque el contrato (§4, §7) reserva el registro al freeze aunque WORKFLOW §8 diga «al detectarlo» |
| **C-4** Nombres repetidos | AR-54-V2-04 | P-33, D-01.5, D-07.4, D-08.4 (residuales), D-09.7, INV-24, T-STO-12, T-AUT-04, T-ENV-06 | **INCORPORATED** | Cualquier objeto y cualquier profundidad, detectado antes de mapear; sin «gana el primero» ni «gana el ultimo»; residuales por clave exacta y por capitalizacion sin afirmar «gana la ultima en todos los builds» |
| **C-5** Unificacion | AR-54-V2-05 | D-09.10, D-18.4, D-22.1, D-22.5, INV-05, §8, T-AUT-14, T-MUT-07, T-MUT-08, T-MUT-10, U-04, §21 (PR-07, PR-13) | **INCORPORATED** | **5A:** origen `Absent` = vacio canonico, mostrado como «vacio / sin propiedades». **5B:** revalidacion de la forma mostrada de cada miembro y del conjunto. **5C:** atomicidad logica |
| **C-6** Id en mutaciones | AR-54-V2-06 | D-10.1, T-MUT-02 | **INCORPORATED** | «`Id` igual por valor; su representacion persistida se escribe en `D` minusculas»; sin prueba byte a byte del texto original |
| **C-7** Alcance del ADR | AR-54-V2-01, -02, -04 | §15 | **INCORPORATED** | 16 puntos alineados con G2D; **[7A]** cota 16 en el ADR; **[7B]** semantica independiente del `Kind` con escritura solo sobre kind conocido; **[7C]** igualdad canonica completa; excluidos UI, nombres de comando, 50/80/1000, gates y tuning |
| **C-8** Trazas y publicacion | — | §0, §2, §3, §12.9, §13, §14, §17, §18, §19, §21, §22, paquete, contrato, Discovery §2.7 | **INCORPORATED** | Paralelas re-medidas en el preflight y antes de publicar; paquete limitado a C-1..C-8 |

**Verificacion de G2F sobre esta matriz**: C-1, C-2 y C-4..C-8 **VERIFIED**; C-3 **DEFECT**, resuelto en V4 por
C-F1 y C-F2.

**Contradicciones entre C-1..C-8: ninguna.** Se revisaron estos pares:
- **C-1/C-2**: los dos estados son de solo lectura y el orden de D-09.8 los separa sin solaparse.
- **C-2/PR-04**: eliminar sigue permitido frente a conteo y longitud, pero no frente a la profundidad ni a otros
  estados de solo lectura.
- **C-5A/C-2**: el vacio canonico tiene profundidad 1 y la version actual.
- **C-3/C-4**: un unico recorrido detecta duplicados, strings no decodificables y profundidad.
- **C-6/D-02.3**: la normalizacion a minusculas es compatible con la igualdad por valor.
- **C-7/§16**: la cota no pertenece a OQ-02.

### 17.3 Cambios de G2B: V2-1..V2-22 (heredados de V2, con su verificacion en G2D)

Sin cambios en V3 salvo los cinco que G2D marco DEFECT, resueltos por C-2..C-6.

| Cambio del Arquitecto | Decision V1 afectada | Seccion V2 | Disposicion | Notas |
|---|---|---|---|---|
| V2-1 Sin descarte destructivo | D-09.8, D-10, D-13, INV-06, T-11, T-35, U, OV, gates | D-09.11, D-10, D-13, INV-06, N-11, T-STO-19, U-03, G6 | **INCORPORATED** | Proyecto y Rack; no queda ninguna API ni control de descarte |
| V2-2 Limitar unificacion | D-09.7, D-10, D-18 | D-09.10, D-22.5, INV-18, T-AUT-14, T-MUT-10, U-04 | **INCORPORATED** | Precision PR-07: solo se escriben los miembros distintos del origen |
| V2-3 Pertenencia indeterminada | D-09.2, RP-02, T-30 | D-09.3, INV-12, RP-01, T-AUT-06..08 | **INCORPORATED** | Cita FAMILY B; B y C rechazadas; no se reutiliza la proyeccion de variables |
| V2-4 Ausente ≡ vacio | D-09.5 | D-09.7, T-AUT-02, T-AUT-05 | **INCORPORATED** | **PR-01**: el minor no participa en la igualdad (§21) |
| V2-5 Aislamiento real | D-08.1, INV-07, O-05, T-15 | D-08.4, D-05.7, INV-07, INV-20, O-05, T-ENV-02..07, T-STO-08..13 | **INCORPORATED** | A..G con prueba cada una; cota de profundidad = 16 (PR-03) |
| V2-6 Invariante de preservacion | D-08.3, D-11, RP-04 | D-21, INV-09, T-GRD-01, T-GRD-02, RP-04 | **INCORPORATED** | Guarda limitada a `src/` (P-25); censo de `Compose` (PR-08); obligacion de I-52 |
| V2-7 Contrato de prueba del restamp | D-11 capas 1..4, T-20, T-21 | D-11.5, T-CPY-01, T-GRD-03, OV-05 | **INCORPORATED** | G-R5 no se re-apunta; extraccion registrada con disparador |
| V2-8 Sin promocion | D-08.5, OQ-05, R-12 | D-08.5, OQ-05, R-12 | **INCORPORATED** | Matriz con [X] |
| V2-9 Regla de evolucion | D-04 | D-04.3..5, D-01.4, §15.1 | **INCORPORATED** | Confrontacion explicita con ADR-0034 §3 |
| V2-10 Id | D-02 | D-02.1..6, T-STO-14, T-STO-15 | **INCORPORATED** | Forma textual antes de `TryParseExact` [X P-19] |
| V2-11 Nombre NFC | D-05 | D-05.1..3, T-STO-16 | **INCORPORATED** | Nombre = presentacion |
| V2-12 Sin agregacion por nombre | D-03 | D-03.4, INV-19, N-12 | **INCORPORATED** | Tambien por id entre racks |
| V2-13 Independencia de Proyecto | D-07 | D-07.1, D-07.2, D-07.6, D-07.7, INV-17, T-PRJ-02, T-PRJ-03 | **INCORPORATED** | Troceado duplicado mantenido; F-13 como deuda |
| V2-14 Ejecutor de Rack | D-09.6, flujos | D-22, D-09.1, T-GRD-04, T-MUT-07, T-MUT-08, OV-08 | **INCORPORATED** | xref: PR-05; UNDO no afirmado |
| V2-15 Mutaciones que preservan | D-10 | D-10.1..3, INV-03, T-MUT-02..06, U-07 | **INCORPORATED** | — |
| V2-16 `Kind` en blanco | (sin definir) | D-09.5, T-AUT-09, T-AUT-10, RP-06 | **INCORPORATED** | **Opcion A** con evidencia historica (P-23, P-24). La nota de V2 sobre un kind desconocido comun (PR-12) queda **sustituida en V3** por C-1: `UnknownKind`, de solo lectura |
| V2-17 Solo hooks | D-15, D-16, D-17 | D-15, D-16, D-17, N-14 | **INCORPORATED** | Se retiran los ids de plantilla y los destinos concretos |
| V2-18 Biblioteca | D-12 | D-12, INV-15, OV-11 | **INCORPORATED** | UX documentada |
| V2-19 Alcance Proyecto | §0 vocabulario, D-03 | §1, D-03.2, N-13 | **INCORPORATED** | — |
| V2-20 UI | D-18, U-01..10 | D-18, U-01..10 | **INCORPORATED** | Nombres de trabajo; censo verificado antes de G7 |
| V2-21 Gates | §12 gates | §14 | **INCORPORATED** | G2D explicito; caracterizacion en G4; fisico solo en G9 |
| V2-22 Preguntas al Owner | §13 | §16 | **INCORPORATED** | Con gate limite |

**Verificacion de G2D sobre esta matriz**:
- **VERIFIED**: V2-1, V2-3, V2-6..V2-9 y V2-12..V2-22.
- **DEFECT, resuelto en V3**:
  - V2-2 (MINOR) → C-5;
  - V2-4 (MINOR) → C-4;
  - V2-5 (MATERIAL) → C-2 y C-3;
  - V2-10 (MINOR) → C-6;
  - V2-11 (MINOR) → C-3.
- **V2-16**: VERIFIED; la opcion A tiene evidencia y su coherencia exigia C-1. Su nota sobre PR-12 queda
  **sustituida** por C-1: un kind desconocido comun es `UnknownKind`, de solo lectura.

La tension interna de V2-4 se resolvio con PR-01, acordada en G2D.

## 18. Matrices de hallazgos

### 18.1 Hallazgos de la revision de G2F sobre V3

| Hallazgo | Severidad | Afecta | Disposicion V4 | Cambio | Estado |
|---|---|---|---|---|---|
| **AR-54-V3-01** `Name` no normalizable con UTF-16 bien formado | MINOR | §1, P-32, D-05.1, D-05.2, D-07.4, INV-23, T-STO-08, -16, -18, -20 | Un `Name` con no-caracteres es invalido al escribir y `PresentButUnreadable` al leer, antes de NFC; `Value` sin cambios | C-F1 | **CLOSED IN V4** (pendiente de re-verificacion) |
| **AR-54-V3-02** Residual de reescritura con surrogate escapado | MINOR | P-31, D-08.4, D-13, D-22, INV-07, RP-11, T-CHR-04, T-ENV-13, T-MUT-08 | F-14b declarado junto a F-14a; garantias acotadas; sin commit si una serializacion lanza | C-F2 | **CLOSED IN V4** (pendiente de re-verificacion) |

### 18.2 Hallazgos de la revision de G2D sobre V2

| Hallazgo | Severidad | Afecta | Disposicion | Cambio | Estado |
|---|---|---|---|---|---|
| **AR-54-V2-01** `Kind` desconocido escribible | MATERIAL | D-09.5, D-09.8, D-18.5, T-AUT-10, §15, PR-12 | `UnknownKind` de solo lectura con `isKnownKind` inyectado; PR-12 rechazada | C-1 | **CLOSED** (verificado en G2F) |
| **AR-54-V2-02** Cota normativa fuera del ADR y contradicciones de edicion | MATERIAL | D-04.3, D-05.6, D-05.7, D-08.4 B, D-13, D-18, §9, §15, INV-20, T-STO-09, U-02 | Constante 16 del formato 1.x en el ADR; `DepthLimitExceeded` de solo lectura; eliminar limitado a conteo y longitud | C-2 | **CLOSED** (verificado en G2F) |
| **AR-54-V2-03** UTF-16 mal formado y excepciones | MINOR | D-05.1, D-05.4, D-07.4, D-08.4 A y G, T-STO-08, -16, -18, -20 | Clases capturadas explicitas; validacion antes de NFC y de serializar; residual preexistente declarado. En V4, no-caracteres en `Name` (3B) y residual F-14b (3C) | C-3; C-F1 y C-F2 | **REOPENED IN G2F → CLOSED IN V4** (pendiente de re-verificacion) |
| **AR-54-V2-04** Nombres repetidos anidados | MINOR | D-01.5, D-07.4, D-09.7, D-08.4, T-STO-12, T-AUT-04 | Cualquier objeto a cualquier profundidad → `PresentButUnreadable`; residuales precisados | C-4 | **CLOSED** (verificado en G2F) |
| **AR-54-V2-05** Huecos de la unificacion | MINOR | D-09.10, D-22.5, INV-05, T-MUT-07, T-MUT-10 | Origen `Absent` canonico; revalidacion de todos los miembros; atomicidad logica | C-5 | **CLOSED** (verificado en G2F) |
| **AR-54-V2-06** Id textual frente a igualdad por valor | MINOR | D-10.1, T-MUT-02 | `Id` igual por valor, persistido en `D` minusculas | C-6 | **CLOSED** (verificado en G2F) |

### 18.3 Hallazgos de la revision de G2B sobre V1

| Hallazgo | Severidad | RD | Disposicion | Seccion | Estado |
|---|---|---|---|---|---|
| AR-54-01 Descarte y unificacion destructivos | MATERIAL | RD-13, RD-19 | Descarte eliminado; unificacion solo segura | D-09.10, D-09.11, D-10, D-13, INV-06, INV-18 | **CLOSED** (verificado en G2D) |
| AR-54-02 Filtro «solo colocadas» | MATERIAL | RD-11 | Cualquier payload no interpretable bloquea las escrituras de Rack | D-09.3, INV-12 | **CLOSED** (verificado en G2D) |
| AR-54-03 Ausente ≡ vacio con version actual | MATERIAL | RD-10 | Igualdad canonica con cualquier minor | D-09.7 | **CLOSED** (verificado en G2D) |
| AR-54-04 Sobreafirmacion del aislamiento | MATERIAL | RD-07, RD-17 | Afirmacion acotada y A..G; en V3, la cota 16 como constante del formato y `DepthLimitExceeded` | D-05.7, D-08.4, INV-07, INV-20, §15.5 | **CLOSED** (reabierto en G2D; verificado cerrado en G2F) |
| AR-54-05 Sin invariante ni guarda de preservacion | MATERIAL | RD-09, RD-15 | Invariante y tres guardas con rojo; obligacion de I-52 | D-21, INV-09, D-19 | **CLOSED** (verificado en G2D) |
| AR-54-06 Justificacion de no promover | MINOR | RD-08 | Matriz verificada; OQ-05 → riesgo residual | D-08.5, §16 | **CLOSED** |
| AR-54-07 ADR-0034 §3 y regla de evolucion | MINOR | RD-01, RD-02 | Tabla normativa y confrontacion | D-04 | **CLOSED** |
| AR-54-08 `TryParseExact` y NFC | MINOR | RD-03, RD-04 | Forma textual exacta; NFC | D-02, D-05 | **CLOSED** |
| AR-54-09 Capa 2 no prueba el restamp; convivencia con G-R5 | MINOR | RD-15 | Contrato de prueba honesto; guarda estructural nueva | D-11.5 | **CLOSED** |
| AR-54-10 Barrido, capas, xref y revalidacion | MINOR | RD-12, RD-19 | Ejecutor definido | D-22, D-09.1, D-09.4 | **CLOSED** |
| AR-54-11 D-17 congela plantillas | MINOR | RD-18 | Solo restricciones | D-17 | **CLOSED** |
| AR-54-12 Proyecto frente a Drawing-level | MINOR | alcance | Definicion y no-objetivo | §1, D-03.2, N-13 | **CLOSED** |
| AR-54-13 `ExtensionData` de entrada y workspace | MINOR | RD-14, RD-19 | Conservacion y resumenes por vista | D-10, INV-03, INV-11 | **CLOSED** |
| AR-54-14 `Kind` en blanco | MINOR | RD-10 | Opcion A con evidencia | D-09.5 | **CLOSED** |
| AR-54-15 Agregacion implicita por nombre | MINOR | RD-05 | Sin clave de agregacion | D-03.4, INV-19 | **CLOSED** |
| AR-54-16 Independencia de `RACKCAD_PROJECT` | MINOR | RD-06 | Ambos sentidos; clave congelada | D-07.1, D-07.6, INV-17 | **CLOSED** |
| AR-54-17 Caracterizacion y smoke de G6 | MINOR | RD-20 | Caracterizacion al abrir G4; fisico solo en G9 | §14 | **CLOSED** |
| AR-54-18 Biblioteca como clasificacion | MINOR | RD-16 | Frontera de V1 | D-12 | **CLOSED** |

Los hallazgos MINOR AR-54-06..18 no se reabrieron en G2D ni en G2F y siguen cerrados sin cambios. «CLOSED IN V4»
es la disposicion del ejecutor: la **re-verificacion** corresponde a la revision exact-SHA de G2H (paquete §6).

## 19. RD-01..RD-20 en V4

Solo cambian RD-04 y RD-07. G2F reviso RD-01, -03, -04, -07, -10, -13, -19 y -20; las demas no se revisaron en
G2F y conservan su estado de V3.

| RD | Estado V3 | G2F (Arquitecto, sobre V3) | V4 | Cambio en V4 | Seccion |
|---|---|---|---|---|---|
| RD-01 | ACCEPTED WITH RECONCILIATION | AGREE | **ACCEPTED WITH RECONCILIATION** | Sin cambios | D-04.3 |
| RD-02 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-01, D-08.2 |
| RD-03 | ACCEPTED WITH RECONCILIATION | AGREE | **ACCEPTED WITH RECONCILIATION** | Sin cambios | D-10.1, T-MUT-02 |
| RD-04 | ACCEPTED WITH RECONCILIATION | AGREE WITH CHANGE | **ACCEPTED WITH FINAL RECONCILIATION** | Un `Name` con no-caracteres es invalido; NFC solo recibe nombres acreditados; `Value` sin cambios (C-F1) | D-05.1, D-05.2, D-05.4 |
| RD-05 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-03 |
| RD-06 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-07 |
| RD-07 | ACCEPTED WITH RECONCILIATION | AGREE WITH CHANGE | **ACCEPTED WITH FINAL RECONCILIATION** | Store sin excepciones de NFC (C-F1). Residuales F-14a y F-14b, garantias acotadas y fallo sin commit (C-F2). Acota tambien el texto de D-13 e INV-07, que cita RD-17, sin cambiar su estado | D-07.4, D-08.4, D-13, D-22.10, INV-07, INV-23 |
| RD-08 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-08.5 |
| RD-09 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-08.3, D-21 |
| RD-10 | ACCEPTED WITH RECONCILIATION | AGREE | **ACCEPTED WITH RECONCILIATION** | Sin cambios | D-09.5..D-09.8 |
| RD-11 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-09.3, D-09.4 |
| RD-12 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-22 |
| RD-13 | ACCEPTED WITH RECONCILIATION | AGREE | **ACCEPTED WITH RECONCILIATION** | Sin cambios | D-09.10, D-22.5, INV-05 |
| RD-14 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-10 |
| RD-15 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-11 |
| RD-16 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-12 |
| RD-17 | ACCEPTED | — | **ACCEPTED** | Sin cambio de estado; el texto de D-13 e INV-07 se acota por RD-07 (C-F2) | D-13, INV-07 |
| RD-18 | ACCEPTED | — | **ACCEPTED** | Sin cambios | D-15..D-17 |
| RD-19 | ACCEPTED WITH RECONCILIATION | AGREE | **ACCEPTED WITH RECONCILIATION** | Sin cambios | D-18.3..D-18.5, U-02, U-04 |
| RD-20 | ACCEPTED WITH RECONCILIATION | AGREE | **ACCEPTED WITH RECONCILIATION** | Sin cambios; §15 identica a V3 | D-20, §14, §15 |

**DEFERRED a nivel de RD: ninguna. OPEN DISAGREEMENT: ninguno.**

## 20. Cambios que V4 no hace

- No reescribe V1, V2 ni V3, y no sustituye sus SHAs historicos (§2.2).
- No cambia C-1, C-2, C-4, C-5 ni C-6, ni el orden de autoridad, ni el alcance candidato del ADR: §15 es identica a
  V3.
- No redacta ni numera el ADR.
- No toca produccion, pruebas productivas, `docs/HANDOFF.md`, `docs/ROADMAP.md` ni `docs/ideas-futuras.md`: F-14a y
  F-14b se registran en G2-FREEZE.
- No modifica `RackEmbedStore` ni arregla F-14a o F-14b.
- No modifica el Discovery: la medicion de G2G esta en §2 y §13.
- No reabre ninguna RD distinta de RD-04 y RD-07.

## 21. Precisiones del ejecutor: estado final

**OPEN DISAGREEMENT = NINGUNO.** Estado de cada precision tras G2F:

| # | Precision | Estado V4 | Nota |
|---|---|---|---|
| PR-01 | En la igualdad canonica, `SchemaVersion` participa **solo por su major**; el minor no participa | **AGREE** | Acordada en G2D; forma parte de [7C] |
| PR-02 | `MixedKind` para `Kind` en blanco, con un mensaje que remite a `RACKEDITAR` **sin prometer reparacion** | **AGREE** | Acordada en G2D |
| PR-03 | Cota de profundidad = 16 | **AGREED** | G2F: C-2 verificado. Constante del formato 1.x en el ADR; subirla exige MAJOR; un documento mas profundo es `DepthLimitExceeded`, de solo lectura |
| PR-04 | Los limites de escritura validan lo que la operacion introduce | **AGREE** | Acordada en G2D; C-2 precisa que eliminar no se permite sobre estados de solo lectura |
| PR-05 | Las definiciones `IsDependent` no son miembros, no bloquean y nunca se escriben | **AGREE** | Acordada en G2D; OV-14 |
| PR-06 | Nombres de miembro repetidos → `PresentButUnreadable`; residual del sobre | **AGREED** | G2F: C-4 verificado. Cualquier objeto y cualquier profundidad, antes de mapear |
| PR-07 | Unificar escribe solo los miembros canonicamente distintos del origen | **AGREED** | G2F: C-5 verificado. Atomicidad del **estado logico**; origen `Absent` = vacio canonico |
| PR-08 | Censo de llamadas a `Compose` (T-GRD-02) | **AGREE** | Acordada en G2D |
| PR-09 | La colision con `ExtensionData` se rechaza en el compositor como error de programacion | **AGREE** | Acordada en G2D; [X P-34] |
| PR-10 | Version de escritura con `Single` = minor mayor entre miembros | **AGREE** | Acordada en G2D |
| PR-11 | `SchemaVersion` de la coleccion con forma exacta `major.minor` | **AGREE** | Acordada en G2D |
| PR-12 | Un `Kind` no vacio, desconocido pero comun, no bloquea | **RECONCILED** | G2F: C-1 verificado. Un kind desconocido comun es **`UnknownKind`, de solo lectura**; la precision de V2 sigue rechazada |
| PR-13 | Unificar revalida la coleccion origen fresca | **AGREED** | G2F: C-5B verificado. Se revalida la forma mostrada de **todos** los miembros y el conjunto |

## 22. Estado de V4

Siguiente gate: **G2H**, revision exact-SHA post-rebase de esta V4 por Coordinador y Arquitecto, limitada a C-F1,
C-F2 y la integridad del rebase. Solo si converge se evalua G2-FREEZE. Nada de esta V4 autoriza G3, el ADR
definitivo, codigo, pruebas, validacion del Owner ni merge.

```text
Coordinator = NOT YET AGREED ON V4
Architect = NOT YET REVIEWED ON V4
Consensus = NOT REACHED
Implementation = BLOCKED
```
