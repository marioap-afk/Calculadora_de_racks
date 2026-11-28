# I-64 F4-T4-HOST-C2: revisión adversarial independiente del código

**Objeto:** `git diff dcf3396f7c0850c6389e9826a1ca3e544f130cc9 9abc1320743d5e4d472c4db6f5fdf4a96b835a46`, en la rama `architecture/workspace-persistente-rackcad`.
- BaseSha `dcf3396f`: hijo de `6d02b485`. Solo añade el contrato, `docs/automation/evidence/I-64-evidence.md` y `docs/automation/state/I-64.yml`.
- RED `fe165a63`.
- GREEN `9abc1320`. El remoto de la rama apunta a este SHA (`ls-remote`).

**Método:**
- Solo lectura, con `git -C`, `git show` y lectura de archivos del worktree y de la carpeta de la corrida. No se ejecutó dotnet ni ninguna prueba.
- La capacidad de discriminación de la guarda se comprobó con una **simulación propia**: los patrones .NET, traducidos literalmente al motor `re` de Python, se aplicaron al texto real de `LateralHeaderDrawer.cs` en el RED y en el GREEN, más 15 mutantes. Es un análisis, no una ejecución de la prueba.
- Para no anclarme, no leí la revisión directa de la sesión.

Las líneas citadas son del GREEN salvo que se indique otra cosa.

---

## 1. Veredicto

**ACEPTABLE CON MENORES.** **0 B / 0 I / 3 M.** No hay ningún hallazgo OWNER.

- El producto son exactamente las dos líneas de D-C2-01, en el sitio fijado y con la sentencia medida en `diag3.patch`.
- El alcance es exacto: 2 de 2 rutas permitidas y ninguna ruta prohibida.
- La guarda cumple D-C2-07. Rechaza todas las colocaciones erróneas que pide la revisión, salvo una sentencia condicional dentro del bucle (M-2).
- Los hallazgos menores tratan del poder de las autopruebas y de la fragilidad de una guarda de texto. Ninguno pide cambiar ni el RED ni el GREEN.

## 2. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| M-1 | Menor | **La autoprueba 4 («moverla fuera del bucle») no ejercita la cláusula de contención del comprobador.** La mutación pone la sentencia justo después de la llave de apertura del método, es decir, antes del `foreach` y antes de la apertura `ForWrite`. El comprobador la rechaza por la cláusula de **orden** (apertura → sentencia), no por la de **contención** (`[^}]*?`, sin `}` en medio). En la simulación, un comprobador sin contención (`[\s\S]*?` en lugar de `[^}]*?`) sigue pasando las autopruebas 3 y 4 en el GREEN, así que ninguna autoprueba fija la contención. Además, el `Assert.NotEqual(body, mutated)` del caso 4 siempre se cumple, porque insertar siempre cambia el texto: no comprueba que la mitad «quitar» de la mutación surtiera efecto. **El impacto real es bajo:** el comprobador real sí rechaza la sentencia colocada tras la llave de cierre del bucle (simulado), y fuera del bucle `reference` ni siquiera compila (la variable es local del bucle; un homónimo en el ámbito del método choca con CS0136). La cláusula literal de D-C2-07 («moverla fuera del bucle») se cumple. | `tests/RackCad.Tests/RedefinedViewReferenceUndoTouchGuardTests.cs:21-25` (comprobador), `:73-79` (mutación e `Insert(1, …)` en `:76`, `NotEqual` en `:78`) | No bloquea y no pide un nuevo RED/GREEN. Si se reabre la guarda en otra tarea: colocar la mutación tras la llave de cierre del bucle (después de `RecordGraphicsModified(true);` y su `}`) y comprobar también que `withoutTouch != body`. |
| M-2 | Menor | **Falso verde posible ante una edición adversarial: la sentencia condicional dentro del bucle pasa.** `[^}]*?` solo prohíbe una llave de cierre ANTES de la sentencia. `if (flag) { reference.Position = reference.Position; }` o `if (flag) reference.Position = reference.Position;` dentro del bucle dan el caso 1 verde y el recuento 1 (simulado). D-C2-07 pide «dentro del bucle», no «incondicional», así que no es un incumplimiento. | `RedefinedViewReferenceUndoTouchGuardTests.cs:22-24` | Ninguna en C2. Si se quiere cerrar el hueco, exigir que entre la apertura y la sentencia no haya `{` ni `if`, o fijar la secuencia exacta de sentencias del cuerpo del bucle. |
| M-3 | Menor | **La guarda es frágil (falla cerrada) ante refactorizaciones inocuas.** Se pone en rojo sin que haya ningún defecto del producto (simulado) si: el molde cambia a `as BlockReference`; `var` pasa a tipo explícito; se renombra `tr`; se añade un bloque con llaves entre la apertura y la sentencia (por ejemplo, `if (reference.IsErased) { continue; }`); o cambian la visibilidad o el tipo de retorno del método, que forman parte del marcador. Es lo esperable de una guarda de texto, y es la misma familia que `InsertRedrawLayerAccessCharacterizationTests.cs:27`, que ya fija `tr.GetObject(referenceId, OpenMode.ForWrite)`. | `RedefinedViewReferenceUndoTouchGuardTests.cs:13`, `:22-24` | Ninguna. Se anota para quien toque `RedefineSystemBlock` en el futuro. |

**Observaciones** (no cuentan en B/I/M):
- **O-1.** El comentario fijado dice «repaint every reference of the redefined block». Solo se tocan las referencias **directas**: un contenedor de una referencia anidada no se repinta (R-5), y el efecto sale de una inferencia (R-3). Pero el texto es literal de D-C2-01, y cambiarlo es la parada C2-S01. Sin acción.
- **O-2.** En el RED, el caso 3 falla por su **precondición** (`Assert.NotEqual`: «Strings are equal»), no por el comprobador. Encaja con D-C2-07 («comprueba primero que su mutación cambió el texto») y la entrega lo declara. El requisito lo demuestran en el RED los casos 1 y 2.
- **O-3.** La entrega habla de «los finales de línea CRLF del archivo». En el blob, las dos versiones de `LateralHeaderDrawer.cs` tienen 0 CR y son LF; CRLF es la copia de trabajo, por `core.autocrlf=true`. Es coherente, y el diff no añade ningún CR.

## 3. Alcance (PD-F4-07): comparación de las listas

**Las colecciones del contrato:**
- se comprobó que el contrato es idéntico en `dcf3396f` y en `9abc1320`;
- AllowedWriteScope 2, ForbiddenWriteScope 79 (sin duplicados), Invariants 6, RequiredTests 31 y StopConditions 22, como dice el encargo;
- ninguna ruta permitida queda bajo una prohibida.

| Ruta cambiada (`git diff --name-status dcf3396f 9abc1320`) | ¿En AllowedWriteScope, exacta? | Prohibidas que la cubren | Commit |
|---|---|---|---|
| `M src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs` | sí | ninguna | solo el GREEN |
| `A tests/RackCad.Tests/RedefinedViewReferenceUndoTouchGuardTests.cs` | sí | ninguna | solo el RED |

- **`dcf3396f..fe165a63` (RED):** solo la prueba nueva; `-- src` está vacío.
- **`fe165a63..9abc1320` (GREEN):** solo el drawer; `-- tests` está vacío.
- **`src/RackCad.Plugin/Drawing/Cantilever/`:** sin cambios.
- **La lista prohibida cubre todo `Drawing/` salvo el drawer:** comparada con `git ls-tree` de `Drawing/` y de la raíz del Plugin en la base, `Cantilever/`, `StructuralSections/` y los 11 archivos hermanos están prohibidos uno a uno.

## 4. El producto, frente a D-C2-01 e INV-C2-01

- **`git diff fe165a63 9abc1320 -- src`:** `+2`, `-0`, en un único hunk.
- **Byte a byte:** tras quitar los 16 espacios de sangría, las dos líneas son idénticas al bloque `csharp` de D-C2-01 (`cmp`). Son ASCII, sin espacios finales y en el orden fijado: el comentario en `LateralHeaderDrawer.cs:170` y la sentencia en `:171`.
- **El sitio:** dentro de `foreach (ObjectId referenceId in systemDef.GetBlockReferenceIds(directOnly: true, forceValidity: true))` (`:167`), después de la apertura `ForWrite` de `reference` (`:169`) y antes de `reference.RecordGraphicsModified(true);` (`:172`). Tienen la misma sangría que `:169` y `:172`.
- **Frente a la línea medida:** es la misma sentencia y la misma posición que `diag3.patch` (blob base `65db7060`). Solo cambia el comentario, que pasa del final de la línea a la línea anterior.
- **Ninguna otra línea cambia:** el archivo pasa de 451 a 453 líneas, con 0 CR antes y después.

**Las preguntas adversariales**

- **Geometría y handles.** El setter recibe el mismo `Point3d` WCS que devuelve el getter.
  - No hay borrado, clonado ni reapuntado, así que los handles no cambian.
  - `ScaleFactors`, `Rotation` y `Normal` no se tocan, así que las simetrías (escala X = -1) se conservan.
  - Los atributos son entidades aparte. `Position` no los mueve, y con delta cero da igual.
- **Bloques dinámicos.** Las referencias de este bucle apuntan a `systemDef`, una BTR con nombre creada por `NewBlock` (`:242-249`), que no es dinámica. Las referencias dinámicas anidadas dentro de la definición no las recorre este bucle: lo que devuelve `GetBlockReferenceIds` son las referencias A la definición, no las que hay DENTRO.
- **Anotativos y normales no Z:** que el valor vuelva idéntico depende del almacenamiento interno de AutoCAD (WCS frente a OCS, representaciones por escala). No es verificable sin AutoCAD (§9).
- **Capas bloqueadas.** `tr.GetObject(referenceId, OpenMode.ForWrite)` ya existía. Con la sobrecarga de dos argumentos, una referencia en una capa bloqueada ya fallaba en la apertura. La sentencia no añade ningún modo de fallo nuevo.
  - El piloto, además, comprueba antes las capas de las referencias directas: `RackSiblingScan.cs:141-147` (`RackSiblingLayerUse.DirectReference`) → `RackPilotUpdateWriter.cs:58`.
  - El clásico no tiene esa comprobación previa, pero tampoco la tenía antes.
- **Referencias en otras definiciones (R-5).** `directOnly: true` incluye referencias cuyo dueño es otra BTR: `RackSiblingScan.cs:144-145` las cuenta como `nestedReferences`. La sentencia registra el Undo de la referencia anidada, pero no el de las referencias del contenedor. Coincide con lo declarado en R-5.
  - Que se reevalúe una red asociativa queda sin verificar (§9).
- **Otros caminos de redibujo (H-07).** H-07 está completo. No hay ningún hallazgo de cobertura.
  - `drawer.RedefineSystemBlock` tiene un único llamador: `SystemBlockWriter.cs:135`. `RedefineInTransaction` (`:126`) la usan `SystemBlockWriter.cs:86`, `LateralHeaderDrawService.cs:99` y `ViewBlockDraw.cs:102`. El ejecutor de variables entra por los grupos de métodos `RedrawInTransaction` (`ProjectVariableMutationExecutor.cs:237-257`).
  - Busqué en `src/` todo `GetBlockReferenceIds`, `RecordGraphicsModified`, `AppendEntity` sobre una BTR existente y `.Erase()`. La única otra redefinición en sitio es `CantileverViewMaterializer.RedefineBlock` (`Cantilever/CantileverViewMaterializer.cs:61`, bucle `:83-86`, que llama `RackCantileverCommands.cs:366`): es R-4, residual declarado.
  - Los demás bucles sobre referencias directas **borran** (`RackCommandSupport.cs:256-261` y `RackSelectivoInsertIntegration.cs:285-287`) o **leen** (`RackSiblingScan.cs:141`, `BlockPlacement.cs:194`, `RackBlockFinder.cs:29` y `WorkspaceNavigationFacts.cs:67`).
  - `StructuralSectionMaterializer.cs:70` y `CantileverViewMaterializer.cs:153` añaden entidades a definiciones **nuevas**, y `RackProjectionWriteScope.cs:103` añade una referencia nueva.
- **INV-C2-02 y C2-S03.** El diff no añade ningún `Regen`, evento, suscripción, pista, drenaje ni tratamiento de `U`, `UNDO`, `REDO` o `MREDO`. «U» y «REDO» solo aparecen en la prosa del comentario.
  - En ejecución, como mucho se emite un `ObjectModified` por referencia. Llega a `EventBridgeCore.cs:348-352` como `DatabaseObjectChanged`, que es propio dentro del ámbito de mutación y ajeno fuera de él: el mismo tipo de pista que ya producen los demás objetos del redibujo.
  - El tipo de objeto lo clasifica `SubjectOf` (`AutoCadEventBridge.cs:351`). Ninguna pista es nueva (D-C2-03).
- **INV-C2-04.** No se añade ninguna transacción, orden ni marca de Undo. La sentencia va dentro de la transacción del piloto o de cada vista del clásico, y dentro de la misma orden, así que el número de pasos de UNDO no cambia.
- **Compilación.** En el build Release de la solución, los códigos de aviso son idénticos a la base: 29 MSB3277 y los mismos xUnit, 82 en total (`tests/b0` frente a `tests/grel`, `warnings.txt`). No aparece CS1717. El build Debug del Plugin (`tests/gplugin`) da 0 avisos.

## 5. La guarda, frente a D-C2-07 e INV-C2-05

**El montaje:**
- **casos:** exactamente 4 `[Fact]` (`:38`, `:49`, `:57`, `:68`), sin ningún `[Theory]`;
- **lectura del código:** `I55ProductCharacterizationTestSupport.Code` y `Body` (`:27-31`), con el soporte sin cambios (`Support.cs:25-45`, en la lista prohibida y fuera del diff);
- **el marcador:** aparece una sola vez en el código sin comentarios;
- **el bucle:** hay un único `GetBlockReferenceIds(directOnly: true` en el cuerpo;
- **lo que no fija:** no nombra `RecordGraphicsModified`, y la sentencia después de él también da verde (simulado).

| Caso | D-C2-07 | Implementación | RED (CI) | GREEN (CI) | Discriminación (simulada sobre el texto real) |
|---|---|---|---|---|---|
| 1 | La sentencia, dentro del bucle de `GetBlockReferenceIds(directOnly: true`, después de la apertura `ForWrite` de la misma variable | `:21-25` y `:38-47`: un único regex que encadena la cabecera del `foreach`, la apertura `var <ref> = (BlockReference)tr.GetObject(<id>, OpenMode.ForWrite);` con el mismo `<id>` del bucle, y `<ref>.Position = <ref>.Position;` con la misma `<ref>`, sin `}` en medio | Falla por aserción (`Assert.True` con mensaje) | Pasa | **Rechaza:** fuera del bucle, tras su cierre; dentro del bucle antes de la apertura; sobre otra variable; comentada con `//` o `/* */`; en otro método; o con otra sentencia (un desplazamiento). **Acepta:** el formato con espacios y saltos de línea, y la sentencia después de `RecordGraphicsModified`. **Hueco:** la sentencia condicional dentro del bucle (M-2). **Falla cerrada:** con refactorizaciones inocuas (M-3). |
| 2 | Exactamente una vez en el cuerpo de `RedefineSystemBlock` | `:16-17` y `:49-55`: `Assert.Single` sobre `\b(\w+)\.Position = \1.Position;` en el cuerpo | Falla por aserción («The collection was empty») | Pasa | **Rechaza:** dos veces, comentada y ausente. **No mira** fuera del cuerpo: una segunda copia en otro método no se detecta, como fija D-C2-07. Que la variable sea otra lo detecta el caso 1, no el 2. |
| 3 | Autoprueba: quitarla. Comprueba primero que la mutación cambió el texto | `:57-66`: `Replace` → `NotEqual` → `False(comprobador)` | Falla por aserción en la precondición (`NotEqual`: «Strings are equal»), O-2 | Pasa | Prueba que el comprobador rechaza la ausencia. No prueba la contención (M-1). |
| 4 | Autoprueba: moverla fuera del bucle. Comprueba primero que la mutación cambió el texto; en el RED, la inserta fuera | `:68-80`: quita la sentencia y la inserta tras la `{` del método (`:76`) → `NotEqual` → `False(comprobador)` | Pasa, como prevé D-C2-07 | Pasa | La rechaza la cláusula de orden, no la de contención. El `NotEqual` siempre se cumple (M-1). |

**¿Puede el regex capturar un bloque equivocado?**
- **El cuerpo del método:** `Body` empareja llaves sobre el código ya sin comentarios, desde la primera `{` después del marcador. La lista de parámetros no tiene llaves, y el método no tiene literales de cadena ni de carácter con llaves. Hoy no captura un bloque equivocado.
- **El riesgo teórico:** un literal con una llave sin pareja añadido en el futuro. Es un riesgo compartido con el soporte existente.
- **Los comentarios:** `Code` quita `//…` también dentro de las cadenas. En este método no hay ninguna.
- **Linux y Windows:** la ruta respeta mayúsculas y minúsculas (CI en Ubuntu en verde), y `\s` absorbe el `\r` de una copia CRLF de Windows.

## 6. El RED (INV-C2-05)

**El producto no cambia:**
- `git diff dcf3396f fe165a63 -- src` está vacío;
- `LateralHeaderDrawer.cs` en `fe165a63` es idéntico (`cmp`) al de `dcf3396f`.

**Los TRX de la CI** de la carpeta de la corrida:

| TRX | Total | Pasan | Fallan | No ejecutadas | Detalle |
|---|---|---|---|---|---|
| `red-ci-core/core.trx` | 19067 | 19064 | 3 | 0 | Las 3 son de la clase nueva: casos 1, 2 y 3, todas por aserción. El caso 4 pasa. **Ninguna otra prueba falla.** |
| `red-ci-ui/ui.trx` | 2403 | 2386 | 0 | 17 | Las 17 son `Skip` preexistentes: BoundedEditor 8, DialogWindow 3, RichEditor 4 y StructuralSectionInspector 2. `tests/RackCad.UI.Tests` no cambia. |
| `ci-core/core.trx` (GREEN) | 19067 | 19067 | 0 | 0 | Los 4 casos de la clase nueva pasan. |
| `ci-ui/ui.trx` (GREEN) | 2403 | 2386 | 0 | 17 | Los mismos `Skip`. |

**Coherencia con la entrega:**
- las corridas locales declaradas (`red0`: 3 fallos y 1 pase; `red1`..`red28`, `redu1` y `redu2` en verde; `gfull` 19067) coinciden con los TRX;
- `ci-filter-counts.json` cuadra: la clase nueva da 4 seleccionadas, con 3 fallos en el RED y 0 en el GREEN; `Workspace` da 3482 en Core y 694 en UI.

## 7. Lo ratificado no cambia (INV-C2-03, INV-C2-04, C2-S02)

**Ninguna prueba existente cambia:**
- `git diff dcf3396f 9abc1320 -- tests` es una sola entrada `A` (la clase nueva);
- `tests/RackCad.Tests/Workspace/`, las guardas de H-09 y su soporte no cambian.

**Las barridas de H-09 y el comentario:**

| Barrida | Lo que cuenta |
|---|---|
| `RackUnitsGuardSourceTests.cs:91-99` | `Insunits` |
| `SelectiveEditorOpenTests.cs:565-571` | `[CommandMethod(` (40) |
| `CustomPropertiesCommandGuardTests.cs:178-189` | `[CommandMethod("…"` |
| `PushBackRoundTripSourceGuardTests.cs:66-84` | `[CommandMethod("…"` |
| `I55G15Auth15ConsumerTests.cs:44-55` | `RackDefinitionCreator.CreateInTransaction(` |

- **Ningún token, ni en el comentario ni en la sentencia:** `LateralHeaderDrawer.cs` no contiene `Pilot`, `Insunits`, `CommandMethod` ni `RackDefinitionCreator`.
- **El `.Position` de `RackUnitsGuardSourceTests.cs:81`** lee solo `RackUnitsGuard.cs` (`:77`), no todo el Plugin.
- **G9** (`WorkspacePilotUpdateIntegrationGuardTests.cs:679-706`) solo cuenta los archivos cuyo código nombra el piloto, y el drawer no lo nombra.

**Otras barridas de todo `src/` o del Plugin, fuera de H-09:**
- son `CantileverPluginSourceGuardTests`, `MainMenuStructuralSectionAccessGuardTests`, `ElevationOverrideSourceGuardTests`, `LengthUnitsAuthorityGuardTests` y, en UI, `CustomPropertiesHelpCensusTests`;
- buscan símbolos que las dos líneas no contienen;
- las suites completas de Core y de UI de la CI del GREEN están en verde.

**Las caracterizaciones que leen el drawer** siguen cumpliéndose sobre el GREEN:
- `InsertRedrawLayerAccessCharacterizationTests.cs:24-28`: se conserva `tr.GetObject(referenceId, OpenMode.ForWrite)` y no aparece `IsLocked`;
- `SharedViewFoundationF1CharacterizationTests.cs:237-239`;
- `StructuralSectionPluginSourceGuardTests.cs:123-124` y `:367`;
- `RackDefinitionCreatorGuardTests.cs:216-254`, cuya lista de firmas no incluye `RedefineSystemBlock`.

**T1B, intacta:** ningún archivo de la clausura (G1..G6) ni de sus fragmentos compartidos (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md:207-225`) cambia, y el drawer no pertenece a ella.

## 8. Contención (INV-C2-06)

- **Solo lee:** la prueba nueva usa `File.ReadAllText` de un archivo del repositorio (`Support.cs:18-23`), localizado subiendo desde `AppContext.BaseDirectory` hasta `RackCad.sln`. No escribe nada, no crea archivos temporales y no usa ninguna configuración del usuario.
- **No carga el Plugin ni AutoCAD:** `RackCad.Tests.csproj:23-31` solo referencia Domain, Application y dos herramientas, y el archivo solo importa `System.Text.RegularExpressions` y `Xunit`.
- **En local**, según la entrega y las carpetas `tests/` de la corrida, solo se corrieron los dos filtros de UI (`gu1`, `gu2`, `redu1` y `redu2`). No hay ninguna carpeta de la suite completa de UI.

## 9. Hallazgos OWNER

**Ninguno.** No encuentro ninguna decisión material nueva fuera de las que ya tomó el Owner («(W), Cantilever fuera, basta, antes»). Las interacciones de ejecución que no están medidas ya están declaradas en R-1..R-5 y en D-C2-03/04.

## 10. Lo no verificado

- **El comportamiento en AutoCAD:**
  - que `U` y `REDO` refresquen (H-06 solo lo midió en Push Back) y los demás sistemas (R-1);
  - el «Nothing to redo» (R-2);
  - cuántos `ObjectModified` se emiten de más (D-C2-03);
  - el coste a escala con muchas copias enlazadas (D-C2-04).
- **La identidad exacta del valor de `Position`** para referencias con una normal distinta de Z o con representaciones anotativas por escala: depende del almacenamiento interno de AutoCAD. Por el código, la geometría no debería cambiar.
- **R-5 ampliado:**
  - si la referencia tocada participa en una red asociativa (un `ARRAY` asociativo), la reevaluación podría modificar entidades del dibujo;
  - `SubjectOf` (`AutoCadEventBridge.cs:351`) solo exime a `AssocAction` y `AssocDependency`, así que esos eventos serían `DatabaseObjectChanged`;
  - si llegasen después del primer `CommandEnded`, que cierra el ámbito propio (`EventBridgeCore.cs:330-345`), marcarían el borrador. No está medido; queda dentro de R-5.
- **La CI:**
  - los TRX no identifican el SHA, así que su correspondencia con RED y GREEN la infiero por el contenido;
  - la conclusión de los jobs `build-ui` y `build-plugin` no está en la carpeta de la corrida;
  - el orden del push (RED antes que GREEN) no lo puedo comprobar con git local.
- **Las corridas locales de la entrega:** solo leí sus resúmenes y logs (`green-summary.txt`, `red-summary.txt`, `b0`, `grel` y `gplugin`). No las repetí.

## 11. Archivos auxiliares de esta revisión

Están en la misma carpeta que este informe:
- `green_lines.txt`, `dc201.txt`, `green_stripped.txt`, `contract.json`, `changed.txt`, `changed_red.txt`, `changed_green.txt`, `scope.py` y `trx.py`;
- la subcarpeta `indep-aux/`, con `sim_guard.py`, `drawer_green.cs` y `drawer_red.cs`.

**Ojo:** si en esta carpeta ya había archivos con esos nombres genéricos, se sobrescribieron. Ninguno se escribió en el worktree ni en ningún repositorio.
