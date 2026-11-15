# I-64 — Revisión adversarial independiente del paquete de F4-T4B (v4)

**Objeto:** `I-64-f4-t4b-gate-contract-package.md` v4, frente a `_package-v3.md`. Los mapas M4 (`map-4-traverse-t1b-coverage.md`) y M5 (`map-5-purge-category.md`) se usan como insumo y se verifican contra el código; no se dan por buenos.

**Base medida:** `7c29a5e55ce623b52ac240665cdf4e795d817d9f`. `git diff c3ae1c97 7c29a5e5 -- src` sale vacío y en `tests/` solo cambian los siete archivos de T1B.

**Rutas:** relativas a la raíz del repositorio. `T4P`, `A2P`, `FRZ` y `TW` como en el paquete. «v4 `:N`» es la línea N del paquete revisado. Los archivos de trabajo de la sesión (paquete, mapas, decisión del Owner sobre `Traverse`) se citan por su nombre.

## Método

- Lectura completa de la v4 y `diff` contra la v3, para aislar el delta: Estado, Autoridades y base; §2.3 (H-15 a H-22) y §2.4 (H-23 a H-26); el `Post` de D-T4B-03; D-T4B-04; D-T4B-09; en el §6, `RequiredTests`, INV-13, INV-16 a INV-18, C08, C09 y el prohibido; §7, §9 y §10.3.
- Lectura de las autoridades:
  - `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4-disposition-reverify-t1b-t4b.md` (completa);
  - `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1b-authorization.md` (completa, con atención al final);
  - `owner-f4t4b-traverse-decision.md`;
  - `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/task-close-package.md`;
  - además, `FRZ` D-13 (`docs/initiatives/I-64-proposal-v5.md:500-545`) y `T4P` §3 y §7 (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:168-183`, `:526-583`).
- Contraste de cada `archivo:línea` del delta con `git show 7c29a5e5:<ruta>` y de cada afirmación sobre guardas con `git grep` en `tests/` y `src/`.
- Conteo estático de `[Fact]`, `[Theory]`, `[InlineData]` y filas de `MemberData` de las 21 clases de la fila por clase.
- Solo lectura: no se compiló, no se ejecutó ninguna prueba y no se escribió en el repositorio.

**Severidad:**
- **BLOQUEANTE:** tal como está escrito, el contrato no se puede cumplir o contradice una autoridad vinculante. Que la corrección sea de texto no le quita la categoría.
- **IMPORTANTE:** deja un hueco de protección, una contradicción con el Owner o una ambigüedad que puede llevar a STOP o a una verificación discutible.
- **MENOR:** precisión, coherencia interna o declaración que falta, sin riesgo material.

## 1. Los hechos H-15 a H-26

| Hecho | Comprobación en `7c29a5e5` | Resultado |
|---|---|---|
| H-15 | 55 UI y 10 Core en verde en `43650040527`: `task-close-package.md:37` y `:68`. Siete clases `PushBackClassicCharacterization*` (6 UI y 1 Core) | Correcto |
| H-16 | CT-27 (1): `TheoryData` en `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:315-350` y la `Theory` en `:352-363`, con las firmas públicas y las dos `internal` exactas de las tres fachadas | Correcto. El rango citado `:322-354` empieza en la primera firma, no en el `TheoryData` (sin peso) |
| H-17 | CT-27 (2) en `:366-382`, con los cuatro archivos y los dos `Lacks` | Correcto |
| H-18 | CT-25 en `:234-270`: el orden `PrepareAll` < `.RedrawInPlace(` < `SyncName(` < `EraseViewBlocks(` < `Regen();` < `Run(`, tres `regen: false` y un `Regen()` | Correcto |
| H-19 | Ningún archivo de T1B nombra `RackSiblingScan`, `RackUnsupportedSiblingInsert`, `RackSelectivoInsertIntegration`, `RackProjectionSnapshotReader`, `Traverse` ni `CaptureDrawing` (`git grep` vacío). El único lector de `RackSiblingScan.cs` en `tests/` es `TW/WorkspaceEditIntentGuardTests.cs` (`:26`, `:77`, `:91`, `:123`). Las anclas `G9bInsertWiringGuardTests.cs:7-35`, `RackViewBatchDriverGuardTests.cs:37-50`, `G16FirstViewCorrectiveTests.cs:158-166` y `RackProjectionCommandGuardTests.cs:118-129` existen | Correcto |
| H-20 | Las cuatro llamadas: `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs:81` (con `eraseByKind` no trivial, `:82-86`), `src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs:43` (`_ => false`), `Views/RackProjectionSnapshotReader.cs:127` y `Views/RackPilotIntentAdapter.cs:116`. No hay más en `src/` | Correcto |
| H-21 | Core 18471 y UI 2310 = 2293 + 17 omitidas: `task-close-package.md:34` y `:37`. La cifra 2923 de `RackCad.Tests.Workspace` y las de cada clase no están en el repositorio; las de cada clase coinciden con el conteo estático (§5) | Plausible; no verificable sin la CI |
| H-22 | `task-close-package.md:129-136` (I-02 e I-03) | Correcto |
| H-23 | `SystemBlockWriter.cs:147-150` reenvía a `LateralHeaderDrawer.cs:193-238`; las fachadas solo devuelven `staleDefinitions` (`PushBackSystemDrawService.cs:64-66`, `PushBackFrontalDrawService.cs:62-64`, `PushBackPlantaDrawService.cs:56-58`) | Las líneas son correctas. «La entrada única» no lo es en todo el Plugin (m-6) |
| H-24 | Candidatas `LateralHeaderDrawer.cs:97-113`, `:128-161`, `:177`; transacción propia `:202`; `db.Purge` `:220`; `Erase` `:221-227`; `catch` `:233-237`; `RackLog.Exception` no lanza (`src/RackCad.Application/Diagnostics/RackLog.cs:23-24`); el payload vive en el diccionario de extensión (`RackBlockData.cs:20-51`) | Correcto |
| H-25 | `SystemBlockWriter.cs:87`/`:91`, `LateralHeaderDrawService.cs:141`/`:145`, `ProjectVariableMutationExecutor.cs:312`/`:316`. I-55: `RackSelectivoInsertIntegration.cs:253-254` (purga y después `SyncName`) | Las líneas son correctas. Lo que «fijan» las guardas está sobredimensionado (m-6) |
| H-26 | Un fallo aborta solo la transacción de la purga; la basura sin payload no la ven los lectores (`RackSiblingScan.cs:119-122`); el precedente anterior a `f2ab66bb` (M5 §2.3) | Correcto |

## 2. La factibilidad de D-T4B-04

### 2.1 Conservación del comportamiento

- **El bucle no usa nada de `document` salvo `document.Database.BlockTableId`** (`RackSiblingScan.cs:115`). El resto entra por `transaction`, `selectedDefinition` (`:145`), `eraseByKind` (`:152`), los almacenes locales (`:109-110`), `RackBlockData.Read(transaction, …)` (`:121`), `RackEnvelopeIdProbe.Probe` (`:141`) y el `AddLayer` estático del mismo tipo (`:134`, `:138`). La firma declarada basta.
- **Mover los diccionarios dentro del bloqueo y construir el resultado antes del `Commit`** no cambia nada observable: son asignaciones puras, sin objetos de AutoCAD (los `ObjectId` siguen siendo válidos tras la transacción). Una excepción del bucle sigue subiendo sin `Commit`, como hoy.
- **Dentro de MUTATE**, el núcleo abre objetos `ForRead` en la transacción poseída y después `RedefineSystemBlock` abre `ForWrite` la tabla y la definición en esa misma transacción (`LateralHeaderDrawer.cs:91-92`). Es válido en AutoCAD; la prueba física sigue siendo de Smoke-2.

### 2.2 ¿El literal es satisfacible con las sustituciones declaradas?

**Solo si se reinterpreta.**
- `:103-161` incluye los dos `using` (`:112-113`) **y la llave `{` de `:114`**, pero no la `}` que la cierra (`:164`). `:161` cierra el `foreach` abierto en `:117`.
- «La retirada … del `Commit`» (v4 `:421`) es vacía: el `Commit` está en `:163`, fuera de los dos rangos.
- El «Por qué así» habla de `:163-169` (v4 `:442`) y el §10.3 de `:166-169` (v4 `:831`).

Con la lectura literal, el núcleo tendría una llave sin pareja o un bloque desnudo que necesita la `}` de `:164`. En los dos casos hay una diferencia no declarada que activa C09 (v4 `:722`) o deja la comparación al criterio del Worker y del Controller (**I-1**).

### 2.3 La accesibilidad del núcleo

**No compila tal como está declarado (B-1).** La firma es `private static` (v4 `:403`), pero la variante caller-owned del escritor lo llama desde `RackPilotUpdateWriter.cs` (`:415`) y el censo exige esa llamada (`:429`). `RackSiblingScan` es `internal static class` y no es `partial` (`RackSiblingScan.cs:77`). La v3 decía «un núcleo **interno**», y M4 §5 (4) dejaba abierta una variante dentro del propio archivo. La única salida que compila sin cambiar la accesibilidad (hacer el tipo `partial` y anidar dentro las tres clases del escritor) contradice D-T4B-03 (v4 `:356-358`) y no está declarada. Ninguna parada lo cubre.

### 2.4 Compatibilidad con las guardas existentes

Con `internal static` (o `private` más una variante interna), el movimiento es compatible con todas las aserciones de `TW/WorkspaceEditIntentGuardTests.cs:74-128`, que usan `Src.Code` sin comentarios:
- `RackBlockData.Read(` 1, `rawEnvelopes.Add(key, json)` 1, `new ReadOnlyDictionary<string, string>(rawEnvelopes)` 1, `new RackSiblingDrawingScan(` 1 y el inicializador de `:110-112`: todo queda en el núcleo, una vez;
- el `AssertInOrder` de `:85` encuentra sus tres anclas solo en el núcleo, así que la primera aparición es la buena;
- un `.Commit(` en el archivo (`:127`): el de `Traverse`;
- la firma de `Traverse` (`:97-99`): el patrón `Traverse\s*\(` no casa con `TraverseCore(`, y el envoltorio la conserva;
- `Capture` y `CaptureDrawing` no cambian (`:93-96`).

Los llamadores no cambian: `SingleScanInsertGuardTests`, `RackViewBatchDriverGuardTests:37-50`, `G16FirstViewCorrectiveTests:158-166`, `RackProjectionCommandGuardTests:118-129` y las guardas del adaptador (`TW/WorkspaceEditIntentGuardTests.cs:115-117`, `:323-365`) siguen en verde. Ningún censo de producto cuenta `CaptureDrawing(` fuera de esos archivos, así que la relectura de POST del escritor no rompe nada.

### 2.5 RED y GREEN de las guardas nuevas

- **(1) núcleo, (2) envoltorio y (4) censo:** si el movimiento va en el GREEN, en el RED fallan por aserción (`Src.BodyOf` hace `Assert.True(match.Success, …)`, `TW/WorkspaceHostSourceSupport.cs:70`) y en el GREEN pasan. Las autopruebas negativas, si mutan el texto real del núcleo, también fallan en el RED (el texto a mutar no existe).
- **(3) `Capture` y `CaptureDrawing`:** su texto no cambia con el movimiento, así que **pasa ya en el RED**. Lo mismo las guardas negativas («sin `StartTransaction`, `LockDocument` ni `Commit`» de INV-39) sobre un cuerpo inerte. Lo admite la regla RED vigente («fallan solo pruebas nuevas o la cambiada», `A2P` INV-14), pero el paquete no declara cuáles pasan en el RED ni si el movimiento va en el RED o en el GREEN, como sí declara para `CallerOwnedFacadeGuardTests` (v4 `:577`) (**m-5**).

### 2.6 Huecos que el movimiento podría cambiar sin que salte nada

`RackSiblingScan.cs` está permitido entero, y las guardas exactas solo cubren el núcleo, el envoltorio, `Capture`/`CaptureDrawing` y el censo. Quedan sin protección:
- **`AddLayer`** (`:172-179`): la deduplicación por uso y por nombre con `OrdinalIgnoreCase` y la captura de `IsLocked`. El núcleo lo llama (`:134`, `:138`). Sus resultados alimentan el preflight de capas de Insertar (`RackSelectivoInsertIntegration.cs:255`, `snapshot.Layers[key]`) y `LockedLayerKeys`/`LOCKED_LAYER` del piloto (D-T4B-02 (b));
- **`MembershipFor`** (`:72-73`) y los cuerpos de los dos constructores (`:16-28`, `:43-55`): `TW/WorkspaceEditIntentGuardTests.cs:104-106` solo prohíbe unos tokens.

M4 §3.1 ya listaba `AddLayer` en «Qué no fijan». C09 habla del núcleo y de los llamadores, no de esto (**I-2**).

## 3. La purga, INV-16 y el `Post` de D-T4B-03

- **La categoría (b) es correcta con el orden declarado.**
  - Las candidatas salen del contenido antiguo menos el nuevo (`LateralHeaderDrawer.cs:97-113`, `:177`).
  - `Database.Purge` filtra por 0 referencias sobre el estado ya confirmado (`:220`), y un fallo aborta solo su transacción y se registra (`:200-237`).
  - Con la unión de las `staleDefinitions` y una sola purga después de todas las unidades, una definición que deja de usar la vista k y usa la k+1 sobrevive al filtro. Por eso intercalarla sería (a) (H-26).
  - Ningún camino clásico convierte hoy el fallo en fallo de la operación (`SystemBlockWriter.cs:91-96`).
- **Coherencia con la tabla de frontera** (v4 `:333-342`):
  - en la fila «`Seal` vuelve y el `Dispose` de `m` lanza → POST», `Commit()` ya cerró la transacción, así que `TopTransaction` es nula y la purga corre. INV-16 («después de que `Seal` vuelva, fuera del ámbito de mutación», v4 `:688`) se cumple porque `Post` va después del `using`;
  - CommitThrew y NotApplied no llaman a `Post`, y por tanto no purgan. El residuo de basura de `SEAL=after` es (b).
- **Coherencia con `CallerOwnedTransactionGuardTests`:** sus 15 hechos solo leen `SystemBlockWriter.cs` y `LateralHeaderDrawService.cs` (`:48-50`), así que no ven al escritor ni chocan con él. Tampoco lo protegen: el orden purga < regen y el regen único no tienen guarda en el piloto, y D-T4B-09 solo fija dónde está la purga (**m-2**).
- **El orden purga → `SyncName` → regen → relectura** es el del clásico (`SystemBlockWriter.cs:87-92` dentro de cada `RedrawInPlace`, y después `RackPushBackCommands.cs:282`, `:300`, `:339`) y el de I-55 (`RackSelectivoInsertIntegration.cs:253-254`). Pero las reglas vinculantes lo enumeran en el otro orden: «`SyncName`, la purga, un `Regen` y la relectura» (`T4P:540`, R-T4-04) y «renombrado (`SyncName`), purga, un `Regen` y una relectura» (`FRZ:519`). La v3 seguía ese texto; la v4 lo cambia sin declararlo (**m-1**).
- **«Omitir y registrar si hay `TopTransaction`»** es correcto para la purga: anidada en una transacción ajena solo cambiaría cuándo persiste la basura. Pero la comprobación solo protege la purga. `SyncName` y la relectura de POST (`CaptureDrawing`) abrirían también transacciones anidadas en esa ajena, y entonces la relectura vería estado sin confirmar. El paquete no dice que el caso es imposible por construcción (`BeginMutation` lo rechaza y `FRZ:518` exige `TopTransaction == null` tras MUTATE) ni qué significa entonces la relectura (**m-3**).
- **La ayuda del nombre releído no tiene fuente** (dentro de **I-3**): `RackSiblingDrawingScan` no lleva nombres de definición (`RackSiblingScan.cs:41-74`), así que «un nombre releído distinto del esperado tras `SyncName`» (v4 `:380`) necesita una lectura más en `Post`.
- **Smoke-2, «si la purga falla, queda el residuo declarado»** (v4 `:739`), no es observable: no hay inyección para la purga, y una que lanzara desde el escritor saldría de `Post` como AppliedPostRereadFailed, contra INV-16 (**m-4**).

## 4. Coherencia con las decisiones del Owner

- **La vía propia, Smoke-2 como primera validación física y «no volver a la costura gruesa»** (`owner-f4t4-disposition-reverify-t1b-t4b.md:126-151`; `owner-f4t1b-authorization.md:129-141`) se recogen bien (§1, §9 I-4).
- **El puerto por ámbitos «accepted in principle»** (`:162-170`): por ejecución (parámetro de `TryUpdatePilot`), sin estado largo (el puerto no retiene objetos), cierre en toda salida (`using`), ningún `ObjectId`/`Transaction` hacia Application (los valores llevan claves y textos) y una sola transacción de mutación. Correcto.
- **«Application owns … POST interpretation. Plugin owns primitive AutoCAD operations only»** (`:152-161`; `owner-f4t1b-authorization.md:136-137`). El `Post` del escritor calcula el nombre esperado, lo compara con el releído y decide que la diferencia es un aviso (v4 `:380-383`). El propio paquete dice que el escritor «solo comprueba el documento y lee» (v4 `:362`). Los avisos de piezas faltantes sí salen como valores (`MissingPieces`) que interpreta la corrida (**I-3**).
- **La distinción A/B/C** (`:203-217`):
  - A, un fallo antes del commit no deja nada authored: excepciones antes de `Seal` → NotAppliedFailure, con el residuo OM-5 de biblioteca, que `FRZ:504-505` admite;
  - B, commit fallido o incierto → Desconocido sin afirmar rollback: una excepción en `Seal` → CommitThrew → Unknown(CommitThrew). `SEAL=before` y `SEAL=after` dan los dos casos;
  - C, commit que vuelve → POST obligatorio, y limpio solo desde la relectura verificada (INV-06; D-T4B-05). Correcto.
  - **El hueco:** no hay fila para el `Dispose` de `m` que lanza **sin** que `Seal` haya vuelto y después de al menos un `WriteUnit`, es decir, el aborto de la transacción authored que falla. Por la fila de `:337` sería NotAppliedFailure, que afirma «cero authored». Un aborto que lanza es persistencia incierta, la B del Owner («do NOT claim rollback unless AutoCAD guarantees it»). Además, en un `using` la excepción del `Dispose` sustituye a la del `WriteUnit`, así que la corrida tiene que separarlas para clasificar (**I-4**).
- **La purga «LOG ONLY … The package must prove which category applies»** (`:182-189`): la prueba de §2.4 es suficiente. «do not invent a false success/failure contract» se respeta, porque no hay estado ni aviso.
- **El movimiento «if and only if»** (`:172-181`) con la condición (3) sustituida (`owner-f4t4b-traverse-decision.md`): la v4 implementa exactamente la lista de la opción elegida. Pero las condiciones (1) «behavior of Insertar remains identical» y (4) «no caller-visible semantics change» siguen vigentes, y el hueco de §2.6 las deja sin guarda en `AddLayer` (**I-2**).

## 5. `RequiredTests`

- **Los espacios de nombres:**
  - las 21 clases de la fila por clase son de `namespace RackCad.Tests` (con bloque), así que `FullyQualifiedName~RackCad.Tests.<Clase>` las selecciona;
  - ningún otro tipo de `tests/` tiene un nombre que empiece por el de una de ellas (`git grep "class <Clase>[A-Za-z0-9_]"` vacío), así que el filtro por subcadena no arrastra otras;
  - `BlankIdPredicateScopeGuardTests`, que comparte archivo con `SingleScanInsertGuardTests`, no entra.
- **Las cuentas, con el conteo estático:**
  - PluginGuard 7 + 3 = 10;
  - `PushBackPluginSourceGuardTests` 8 + 3 = 11;
  - `RackUnitsGuardSourceTests` 5 + 13 = 18;
  - `SingleScanInsertGuardTests` 1 + 4 = 5;
  - `RackViewBatchDriverGuardTests` 4;
  - `RackEnvelopeIdProbeTests` 5;
  - `RackSiblingCustomPropertiesGateTests` 2;
  - `RackSiblingRedrawTests.cs` 20 = 12 + 2 + 6;
  - `SiblingRedrawSeamGuardTests.cs` 5 = 2 + 1 + 2;
  - `CallerOwnedFacadeGuardTests` 4 + 11 × 4 fachadas = 48;
  - `CallerOwnedTransactionGuardTests` 15;
  - `RackProjectionCommandGuardTests` 34;
  - `PushBackRoundTripSourceGuardTests` 24;
  - `NamespaceFolderGuardTests` 7;
  - `I55G15Auth15ConsumerTests` 5;
  - `G16FirstViewCorrectiveTests` 12 hechos + 4 `InlineData` + 13 filas de `FirstViews` + 32 pares de `FirstViewAndSister` = 61;
  - `CustomPropertiesEnvelopeGuardTests` 6 + 12 + 4 = 22.

  Todas son coherentes.
- **El filtro 1** funciona, pero por otra razón que la que da el paréntesis (v4 `:644`): los 68 archivos de `TW` son `namespace RackCad.Tests;` y el filtro selecciona por el **prefijo del nombre de la clase** (`Workspace…`), no por un espacio de nombres `RackCad.Tests.Workspace`, que no existe. Lo mismo en UI: los 24 archivos de `tests/RackCad.UI.Tests/Workspace` son `namespace RackCad.UI.Tests`. «2923 + las nuevas» no es un número que el Controller pueda comprobar (**m-8**).
- **Las clases que faltan:** ninguna esencial. `G16OrientationGuardTests`, `LinkedPropertyViewPayloadTests` e `I60BatchCreationNamingTests` tocan archivos prohibidos que T4B no cambia. `WorkspaceEditIntentGuardTests`, que protege las cuentas del barrido, solo está en el filtro 1 (`ExpectRed` true) porque cambia en el RED. Es aceptable con la CI completa en el GREEN.
- **Trampas de censo de todo `src/` que afectan a los archivos nuevos y no se declaran** (**m-11**):
  - `CustomPropertiesEnvelopeGuardTests` T-GRD-01 y T-GRD-02 (`:24-34`, `:69-75`, `:194-236`): ningún `new RackEmbedDocument` ni `RackEmbedComposer.Compose` fuera del censo. Una «fábrica inerte» del RED (INV-15) que construya un sobre rompería una clase con `ExpectRed` false;
  - `SelectiveEditorOpenTests` (`:565-572`): 40 `[CommandMethod(` en todo el Plugin, contando los comentarios;
  - `RackUnitsGuardSourceTests` (`:87-101`): `Insunits` solo en `RackUnitsGuard.cs`.

## 6. El alcance y las paradas

- **El escritor necesita** `PrepareRedraw` y `RedrawInTransaction` de las fachadas (`internal`, mismo ensamblado), `SystemBlockWriter.PurgeAfterCommit` y `ApplyRegen` (`internal static`), `RackBlockRenamer.SyncName` (público) y `RackLog`. Todo es de lectura: ni las fachadas ni `SystemBlockWriter` necesitan cambios. El `.csproj` del Plugin es de estilo SDK (`src/RackCad.Plugin/RackCad.Plugin.csproj:1`), así que el archivo nuevo entra sin tocarlo.
- **Los llamadores del barrido** no hace falta tocarlos. Están en el prohibido: A por la costura de I-55 (H-14), B y C por «el resto del Plugin», y D por nombre.
- **El prohibido es coherente con el diseño**, salvo por lo que el permitido no protege: `RackSiblingScan.cs` está permitido entero (§2.6).
- **C08 es operable.**
- **C09 es operable, pero:**
  - con el literal ambiguo de §2.2 puede saltar en falso (I-1);
  - no cubre el problema de accesibilidad (B-1);
  - no cubre los cambios fuera del núcleo en el mismo archivo (I-2).

## 7. T1B

- **Ningún cambio previsto de T4B rompe una prueba de T1B:**
  - `PushBackClassicCharacterizationPluginGuardTests` lee `RackPushBackCommands.cs`, `RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs`, `Views/RackViewPlacement.cs`, las tres fachadas y `KindHandlers/PushBackKindHandler.cs`, y ninguno está en el permitido;
  - la guarda de fuente de UI solo barre `src/**/*.cs` en busca de `InsertRequestedRaised +=`;
  - las otras cinco clases de UI ejecutan la ventana y no cargan el Plugin.
- **INV-13, H-16 y H-17 son correctos.** CT-27 (2) excluye a propósito al escritor nuevo (`PluginGuardTests.cs:368-370`), y los cuatro archivos de su lista están en el prohibido.
- **Los menores del cierre de T1B:** su §7 manda M-02 a M-09 a «la reconciliación previa a T4B … y F5» (`task-close-package.md:140-150`). La v4 solo dispone M-02 y M-03 (**m-13**).

## 8. El §9 frente al texto literal del Owner

- **Bien citados y decididos:**
  - I-1 («T4B = write implementation/semantics…», `:191-202`);
  - I-4 («USE A PILOT-SPECIFIC WRITE PATH», «OV-RED … does NOT validate», «Smoke-2 becomes the first physical validation», `:125-151`);
  - I-6 (la autorización «if and only if» más la decisión «Guardas exactas en T4B»).
- **I-5 está decidido solo en parte.** El Owner decide la purga («LOG ONLY unless…»), pero la fila I-5 (v4 `:769`) también fija la política de avisos (piezas faltantes, nombre y `Regen`), que el Owner no decidió. Y su justificación («porque devuelve `void` y no se toca») es la que M5 rechaza como prueba; la prueba buena está en el §2.4 (**m-10**).
- **I-12 se sigue de la C del Owner** («successful commit → POST reread required»). Dejarlo para la orden de emisión no es un error.
- **I-2 e I-11 «compatibles» y a la vez «pendientes»** es coherente.
- Nada que no esté decidido se presenta como decidido, salvo la parte de I-5 citada.

## 9. Lo que la v3 tenía bien y la v4 rompe, y las inconsistencias internas

- **La accesibilidad:** la v3 decía «núcleo interno»; la v4 lo hace `private` (B-1).
- **El orden de POST:** la v3 seguía `T4P:540` y `FRZ:519`; la v4 lo invierte sin declararlo (m-1).
- **F4-E26:** la v3 decía «los tres caminos usan el mismo núcleo y la misma `BuildStaleBase`»; la v4 dice «los cuatro caminos» (v4 `:446`). Pero A, B y C no calculan una `DraftStaleBase`, y F4-E26 habla de los tres caminos de lectura del piloto (`T4P:530`) (**m-7**).
- **Textos que no se actualizaron:**
  - la fila I-6 dice «Lo fijan una guarda y Smoke-2» (v4 `:770`), y ahora son cuatro guardas, cinco clases y Smoke-2;
  - el §10.3 dice que D «se declara por construcción» (v4 `:832`), cuando D entra por «las tres lecturas»;
  - `:163-169` frente a `:166-169` (v4 `:442`, `:831`).

  Todos en **m-10**.
- **«Entre los dos solo cambian los siete archivos de prueba de T1B»** (v4 `:23`) es falso tal cual: el `diff` entre `c3ae1c97` y `7c29a5e5` toca 255 archivos, casi todos de `docs/` (**m-9**).
- **H-23 y H-25** (v4 `:213`, `:232`) sobredimensionan (**m-6**):
  - `PurgeUnreferenced` tiene otras dos entradas directas (`Drawing/BlockPlacement.cs:213` y `RackCommandSupport.cs:274`, las dos después de su commit), que M5 §1.1 sí recoge;
  - `CallerOwnedTransactionGuardTests.cs:197-238` solo fija dos cuerpos, y su «una sola entrada» (`:233-238`) no es un censo;
  - la fila de I-55 no lleva líneas.
- **INV-14 y la transacción:** «la transacción en `using`» (v4 `:567`) no encaja con un ámbito de mutación que tiene un campo `Transaction` (v4 `:358`). Es heredado de la v3 (**m-12**).
- **La numeración** (H-01..H-26, D-T4B-01..09, INV-01..18, C01..C09, I-1..I-14) y las referencias cruzadas del delta son coherentes. La tabla de 21 clases de la v4 coincide con la lista de la v3 más las cinco de D-T4B-04 (5).

## Tabla de hallazgos

| Id | Severidad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| B-1 | BLOQUEANTE | El núcleo se declara `private static`, pero se le exige una llamada desde `RackPilotUpdateWriter.cs` y un censo de dos llamadas que la incluye. No compila; la única salida (tipo `partial` con las clases del escritor anidadas) contradice D-T4B-03 y no está declarada | v4 `:403`, `:415`, `:429`, `:356-358`; `src/RackCad.Plugin/Views/RackSiblingScan.cs:77`; v3 «núcleo interno»; M4 §5 (4) | `internal static` en la firma, fijado por la guarda (2) o (4); o una variante `internal` dentro de `RackSiblingScan.cs`, con el censo «desde `Traverse` y desde la variante» |
| I-1 | IMPORTANTE | El literal congelado no está bien definido: `:103-161` incluye la `{` de `:114` sin su `}` (`:164`); la retirada del `Commit` (`:163`) es vacía; `:163-169` frente a `:166-169`. Con la lectura literal, C09 salta o la equivalencia queda a criterio | `RackSiblingScan.cs:112-117`, `:161-169`; v4 `:406`, `:420-421`, `:442`, `:722`, `:831` | Literal = `:103-110` + `:115-161` + `:166-169`, con la única sustitución `document.Database.BlockTableId` → `database.BlockTableId` en `:115`. Declarar que `:111-114` y `:162-165` pasan al envoltorio. Fijar la función de normalización |
| I-2 | IMPORTANTE | Sin guarda para el resto de `RackSiblingScan.cs`, un archivo permitido: `AddLayer`, al que llama el núcleo, `MembershipFor` y los cuerpos de los constructores. Un cambio en `AddLayer` cambia `Layers` (el preflight de capas de Insertar y `LOCKED_LAYER` del piloto) sin que falle nada | `RackSiblingScan.cs:16-28`, `:43-55`, `:72-73`, `:134`, `:138`, `:172-179`; `RackSelectivoInsertIntegration.cs:255`; `TW/WorkspaceEditIntentGuardTests.cs:104-106`; M4 §3.1; disposición `:174`, `:177` | Una guarda de archivo completo: el texto normalizado del GREEN = el de `c3ae1c97` con `:98-170` sustituido por el envoltorio y el núcleo declarados. Como mínimo, congelar `AddLayer`. Ampliar C09 a cualquier otro cambio en el archivo |
| I-3 | IMPORTANTE | El `Post` del escritor interpreta POST: calcula el nombre esperado y decide el aviso. Contradice «Application owns … POST interpretation» y el propio «el escritor solo comprueba y lee». Además, el nombre releído no tiene fuente (`CaptureDrawing` no devuelve nombres), y leerlo exigiría otra transacción, que choca con «un `StartTransaction` en `BeginMutation`» | v4 `:362`, `:380-383`, `:564`; disposición `:152-161`; `owner-f4t1b-authorization.md:136-137`; `RackSiblingScan.cs:41-74`; `Drawing/RackBlockRenamer.cs:21-58` | Que `Post()` devuelva valores (el nombre de cada definición por clave, leído después de `SyncName`, y el fallo del `Regen`) y que la corrida calcule el esperado (`BlockNaming`, `RackViewBaseName`, de Application) y los avisos. Declarar la lectura de nombres y su transacción en D-T4B-03 y D-T4B-09. Si se mantiene en el Plugin, llevarlo al §9 como excepción para el Owner |
| I-4 | IMPORTANTE | La tabla de frontera no clasifica el aborto que falla: el `Dispose` de `m` lanza sin `Seal` y después de algún `WriteUnit`. Cae en NotAppliedFailure, que afirma «cero authored», contra la B del Owner. En un `using`, esa excepción tapa la del `WriteUnit` | v4 `:333-342` (`:337`); disposición `:203-211`; `FRZ:525-526` | Una fila más: con `sealedReturned` falso y algún `WriteUnit` empezado, si el `Dispose` de `m` lanza, Unknown (CommitThrew o una causa propia). Sin `WriteUnit`, NotAppliedFailure. Con su prueba con el doble y la separación de las excepciones en la corrida |
| m-1 | MENOR | `Post` va en el orden purga → `SyncName`, el del clásico y el de I-55, pero R-T4-04 y D-13 paso 5 lo enumeran como `SyncName` → purga. La v3 los seguía; la v4 lo cambia sin declararlo | `T4P:540`; `FRZ:519`; `SystemBlockWriter.cs:91`; `RackPushBackCommands.cs:282`; `RackSelectivoInsertIntegration.cs:253-254`; v4 `:834` | Declararlo (un I-n nuevo o una nota en D-T4B-03): `FRZ:519` remite a «la autoridad existente» y `FRZ:506` no congela la mecánica |
| m-2 | MENOR | El orden de `Post` no tiene guarda: la comprobación de `TopTransaction` < purga < `SyncName` < un `ApplyRegen` < relectura. `CallerOwnedTransactionGuardTests` no ve al escritor | v4 `:374-379`, `:570-573`, `:692`; `CallerOwnedTransactionGuardTests.cs:48-50`, `:197-231` | Una guarda de orden sobre el cuerpo de `Post`, con cuentas de una aparición, por la debilidad de la primera aparición (M-02 de T1B) |
| m-3 | MENOR | Si hay una `TopTransaction` en `Post`, solo se omite la purga. `SyncName` y la relectura de POST se anidarían en la transacción ajena y la relectura vería estado sin confirmar. No se dice que es imposible por construcción ni qué vale la relectura | v4 `:375`, `:688`; `FRZ:518`; H-25 | Declarar que el caso es imposible (rechazo en `BeginMutation` y `FRZ:518`). Si ocurre, aviso o AppliedPostRereadFailed, no un POST normal |
| m-4 | MENOR | Smoke-2 «si la purga falla, queda el residuo» no es observable: no hay inyección, y una en el escritor saldría de `Post` como AppliedPostRereadFailed, contra INV-16 | v4 `:739`, `:391-395`; `LateralHeaderDrawer.cs:233-237` | Reformularlo como residuo declarado y no observado, o quitarlo del §7 |
| m-5 | MENOR | No se dice si el movimiento va en el RED o en el GREEN, ni qué guardas nuevas pasan ya en el RED (la (3) de D-T4B-04 y las negativas de INV-39) | v4 `:424-427`, `:568`, `:577`; `A2P` INV-14 | Mover `Traverse` en el GREEN y declarar las guardas que pasan en el RED, como se hace con `CallerOwnedFacadeGuardTests` |
| m-6 | MENOR | H-23 «la entrada única» y H-25 «lo fijan … y hay una sola entrada» sobredimensionan: hay dos entradas directas más a `PurgeUnreferenced`, la guarda solo mira dos cuerpos, y a la fila de I-55 le faltan las líneas | v4 `:213`, `:225-232`; `Drawing/BlockPlacement.cs:213`; `RackCommandSupport.cs:274`; `CallerOwnedTransactionGuardTests.cs:233-238`; M5 §1.1 y §1.3 | «La entrada única de los caminos de redibujo»; «fijan los dos escritores clásicos»; las líneas de I-55 (`SiblingRedrawTransaction.cs:23`, `RackSelectivoInsertIntegration.cs:253`) |
| m-7 | MENOR | Regresión: «F4-E26 … los cuatro caminos usan … la misma `BuildStaleBase`». A, B y C no la usan, y F4-E26 habla de los tres caminos de lectura | v4 `:446`; `T4P:530`; v3 «los tres caminos» | Volver a «los tres caminos de lectura del piloto», y dejar los cuatro llamadores para INV-17 |
| m-8 | MENOR | El filtro 1 se explica con un espacio de nombres que no existe: la selección es por prefijo de clase. Y `MinSelected` «2923 + las nuevas» no es un número | v4 `:644`; los 68 archivos `TW` con `namespace RackCad.Tests;`; UI con `namespace RackCad.UI.Tests` | Explicar el prefijo y poner `MinSelected` = 2923 como suelo, o la cifra con las nuevas declaradas |
| m-9 | MENOR | «Entre los dos solo cambian los siete archivos de prueba de T1B» es falso: cambian 255 archivos, sobre todo de `docs/` | v4 `:23`; `git diff --stat c3ae1c97 7c29a5e5` | «En `src/` nada, y en `tests/` solo los siete de T1B» |
| m-10 | MENOR | Textos sin actualizar o que dicen de más: la fila I-5 (su justificación, y presentar como decidida la política de avisos); la fila I-6 («una guarda»); el §10.3 («D por construcción»); `:163-169` frente a `:166-169` | v4 `:753-757`, `:769`, `:770`, `:832`, `:442`, `:831` | Alinearlos con el §2.4, D-T4B-04 (1) a (7) y el §7. En «Lo ya decidido», limitar I-5 a la purga |
| m-11 | MENOR | No se declaran las trampas de censo de todo `src/` que afectan a los archivos nuevos: la construcción y la composición de sobres (una fábrica inerte del RED podría romper una clase con `ExpectRed` false), `[CommandMethod(` y `Insunits` | `CustomPropertiesEnvelopeGuardTests.cs:24-34`, `:69-75`, `:194-236`; `SelectiveEditorOpenTests.cs:565-572`; `RackUnitsGuardSourceTests.cs:87-101` | Listarlas en D-T4B-09 o en INV-15 («ningún archivo nuevo construye ni compone un `RackEmbedDocument`») |
| m-12 | MENOR | «La transacción en `using`» (INV-14) frente a un ámbito de mutación con un campo `Transaction`. Viene de la v3 | v4 `:358`, `:370`, `:567` | «El ámbito de mutación va en un `using` de la corrida, y su `Dispose` libera la transacción», con la guarda redactada así |
| m-13 | MENOR | El cierre de T1B manda M-02 a M-09 a la reconciliación previa a T4B; la v4 solo dispone M-02 y M-03 | `task-close-package.md:140-150`, `:167` | Una línea en el §10.3: M-04 a M-09 son de las pruebas de UI de T1B y no aplican a T4B (van a F5) |

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Recuento: **1 bloqueante, 4 importantes y 13 menores.**

- **B-1 impide emitir tal como está:** la firma `private` y la llamada desde el escritor se excluyen. La corrección es de una palabra (`internal`) y no cambia el diseño.
- **Los importantes no obligan a rediseñar:**
  - I-1 y I-2 completan la protección propia que eligió el Owner, con un literal bien definido y el archivo entero congelado fuera de la región declarada;
  - I-3 devuelve a Application la interpretación de POST que el Owner le asigna, con un ajuste pequeño de `Post()`, o la declara como excepción;
  - I-4 añade una fila a la tabla de frontera para la clase B del Owner.
- **Lo central del delta se sostiene:**
  - los hechos H-15 a H-26 son correctos en sus líneas;
  - la categoría (b) de la purga está probada y es coherente con la tabla de frontera, con `CallerOwnedTransactionGuardTests` y con el orden del clásico;
  - el movimiento es compatible con todas las guardas existentes del barrido y de sus llamadores;
  - los filtros por clase seleccionan lo que dicen, con cuentas coherentes;
  - ningún cambio de T4B rompe T1B.
- **Basta una comprobación dirigida de la v5 contra esta tabla**, sin otra revisión completa, salvo que I-3 se resuelva llevando una excepción al Owner.
