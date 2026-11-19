# Revisión independiente y adversarial del código — I-64 F4B-PBT1B-LOSSLESS-STATE-TYPE, intento 2 (corrección de las pruebas)

**Objeto:** BaseSha `2f5d3decb237913e3980aa0bbd84346a7ff91f19`, RED `126fd65ff138afe839e67788061960c0e911cf1c` (árbol `c6d555b9`), GREEN `a18ba345eddc5333c22b8bacbad5c2ed4dbf5959` (árbol `4936f6fa`). WorkRunId `R20261118T223111Z-4c04`, delegación `R20261118T222843Z-e354`.

**Autoridades, leídas en `a18ba345`:**
- el análisis vinculante (en adelante **AN**), `docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/analysis.md`, con un SHA-256 medido de `9891487e…f0bf` que coincide con el del encargo;
- la revisión del intento 1 (`independent-review.md`);
- el contrato `gate-contract.json` (RequiredTests, AllowedWriteScope y ExpectedEvidence);
- el paquete v2.7 (**PKG**), cuyo blob es el mismo en `4b0498e3`, `2f5d3dec` y `a18ba345`;
- `owner-pbt1b-rework-decisions.md` (F4B-E22).

**Método:**
- El código y los documentos, solo por objetos git desde el checkout principal: `git show`, `diff`, `rev-parse`, `ls-tree` y `grep <sha>`.
- La entrega, desde la copia `rv-a2/`.
- Python de solo lectura, escrito con la herramienta de archivos, para resumir los TRX y el handoff.
- Una consulta de solo lectura, `gh run view`, de los dos runs de CI (`headSha` y conclusión de cada job).
- No he ejecutado dotnet ni ninguna prueba. No he leído ni escrito en el worktree ni en el repositorio. El checkout principal sigue limpio.

**Alias:**
- **GRD** `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs`
- **CPY** `tests/RackCad.Tests/PushBackEditableStateCopyTests.cs`
- **LOAD** `tests/RackCad.Tests/PushBackEditableStateLoadTests.cs`
- **GATE** `tests/RackCad.Tests/PushBackEditorOutputGateTests.cs`
- **OWN** `tests/RackCad.Tests/PushBackEditableStateOwnershipTests.cs`
- **PAR** `tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs`
- **CS** `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`
- **ES** `…/PushBack/PushBackEditorState.cs`
- **DA** `…/PushBack/PushBackEditorDesignAssembler.cs`
- **PESC** `…/PushBack/PushBackEditableStateComparer.cs`

Si no se dice otra cosa, las líneas son de `a18ba345`.

---

## 1. Veredicto

**ACEPTABLE CON MENORES — B/I/M = 0/0/5.**

- **Las nueve correcciones están en el diff y son las que pide AN:26-34.**
  - I01 discrimina de verdad: lo prueba la mutación p3.
  - I02 ya puede fallar, aunque eso se sostiene por lectura.
  - M01 lleva su control.
  - M02, M04, M06, M08 y M13 están completas.
  - M07 está bien en la guarda, pero su autoprueba nueva no discrimina lo que M07 corrige (M01 de esta revisión).
- **El régimen es exacto:**
  - en el RED, los seis archivos de `src/` tienen los blobs de `2d9febc6`;
  - en el GREEN tienen los de `a0a5f699`;
  - el GREEN no toca `tests/`;
  - el RED falla por aserción en los 15 métodos con ExpectRed;
  - las dos guardas de D2 pasan en el RED;
  - la CI del GREEN da 4/4 sobre el SHA exacto, con 19 086 y 2 437 resultados.
- **Los cinco menores son de evidencia o de alcance de las pruebas,** no de producto. Ninguno pide otro intento.

## 2. Recuento

| Severidad | N | Ids |
|---|---|---|
| Bloqueante | 0 | — |
| Importante | 0 | — |
| Menor | 5 | PBT1B2-IR-M01 a M05 |
| OWNER | 0 | — (§8) |

## 3. Tabla de hallazgos

| Id | Sev. | Dónde (`a18ba345`) | Qué | Qué hacer |
|---|---|---|---|---|
| PBT1B2-IR-M01 | M | GRD:368-379 frente a GRD:294-295; la comprobación anterior en `2f5d3dec` GRD:299-300 | **La autoprueba `autoprueba-escritor-fuera-de-DrawPushBackView` no discrimina lo que M07 corrige.** Lleva el escritor a *antes* de la declaración (`Insert(declaration, …)`). La comprobación del intento 1 (`declaration < writerFirst`) ya fallaba en ese caso, así que la autoprueba da `falla` con la comprobación vieja y con la nueva. El hueco de M07, un escritor *después* de la declaración pero *fuera* del método, no lo ejerce ninguna autoprueba. Por lectura, `WriterInsideDrawPushBackView` sí acota el método (`MethodText`, GRD:277-291) | Una autoprueba que lleve `system.Name = rackName;` después de la `}` de cierre de `DrawPushBackView`. No hace falta tocar producto |
| PBT1B2-IR-M02 | M | CPY:962-970 (N-M-02) | **I02 está corregido al pie de la letra, pero sin un control de que la sesión sea la única diferencia.** Por lectura, la línea ya puede fallar: con el mismo nombre, `anterior` y `fresh` solo difieren en la lista confirmada de la sesión (PESC:236-243 y ES:77-81). Si se quitara la regla N-M-02, los dos lados serían «sin cambios» y saldría `distintos=False`. Pero ninguna línea lo fija, y a diferencia de I01 (p3) no se corrió ninguna mutación. Si mañana apareciera otra diferencia entre los dos estados, el caso volvería a no poder fallar sin que nadie lo viera | Añadir una línea de control, por ejemplo la de `anterior` con la sesión reabierta sobre su línea base (`AdoptLoadedBaseline`) frente a `fresh`, que debe dar `iguales=True` |
| PBT1B2-IR-M03 | M | `rv-a2/tests/p2/*.log`; `worker-handoff.json` (TestsExecuted p2c, p2a2, p2d) | **La medición previa al RED no tiene una compilación que la respalde.** Las tres corridas (p2c a las 16:42, p2a2 a las 18:14 y p2d a las 18:18, hora local) usan `--no-build`, y la entrega no trae ningún log de compilación Debug de esa fase. La última compilación registrada es la de p1, a las 16:35: es de la UI y lleva la sonda temporal. Además, un TRX de pruebas en verde no contiene las líneas, así que no se pueden cotejar. Lo que sí prueba qué texto se compiló antes del RED (18:38): <br>- **p3** (con compilación, 18:21): CT-15 pasa con las siete sentencias completas, y CT-14, sin la R2b, falla solo en k1 y k2 (`pos 14`); <br>- **r1d** (con compilación, 18:26): la lista esperada ya trae `pb-01-base Restaurar…` en la posición 4, es decir, M08. <br>Las líneas son ciertas sobre la implementación, porque el GREEN no toca `tests/` y pasa. Lo que queda sin probar es solo el «medido antes del RED» de AN:36 para CPY, LOAD, GATE y PAR | Para la verificación: pedir el log de compilación de la fase p2 o aceptar la evidencia de p3, r1 y el GREEN. En adelante, medir sin `--no-build` o guardar el log de compilación |
| PBT1B2-IR-M04 | M | OWN:63, 79, 85, 93, 108 y 112 | **La regla «EXCEPCION» (PKG:829) sigue sin aplicarse en CT-11.** `PushBackEditableStateOwnership.Normalize`, `Of` y `Table` se llaman sin `Paso`. Está fuera de la lista de M13 (AN:34 nombra tres clases) y no es una regresión: OWN no cambia, y la frase del Worker «en las tres clases» es exacta. La revisión del intento 1 no lo señaló | Declararlo como arrastre, o aclararlo en el paquete de PB-T1c. No se corrige en este intento («nada más que eso») |
| PBT1B2-IR-M05 | M | PAR:525-528 y PAR:579; `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs:101-105` | **La línea de declaración de M08 no fija las cifras.** La lista con lado B es literal (`[pb-20-ab, X-01, F-01]`), pero `omitido-en=` y `en=` se calculan de `CorpusNames()`, que lista la carpeta `Fixtures/I64`. El 16 y el 19 de la entrega no están escritos en la prueba: si faltara un fixture, bajarían a la vez lo medido y lo esperado. Es el mismo patrón de `Esperadas()` que ya existía | Opcional: escribir `omitido-en=16` y `en=19` literales |

**Observaciones sin severidad (no cuentan):**
- **O1 (M04).** `RichRetired = Enrich(Retired)` (CPY:204 y CPY:238-240) sustituye la cola que aparca `ParkTailOf` en `pb-20-ab` por la cola sintética de `Enrich`, porque `ParkDormantTail` reemplaza (CS:85-95). El texto de CT-06 solo pide «`pb-20-ab` con B retirado» (PKG:908), y la cola es de clase A, que `EditableSlots` no recorre. No se pierde cobertura de ranuras E, pero el comentario «with … a parked tail» describe la cola sintética, no la real.
- **O2 (M06).** La sesión y el commit de la parte 3 se abren tras una `Pasada` (CPY:740-746), sobre la línea base calculada (DA:101-107, ES:77-81) y no sobre la resuelta del diseño. Es la receta de carga del §5.0 (`LoadDesign` y después `Pasada`, PKG:825), y la `Pasada` no reescribe el contenido del editor. No hay pérdida.
- **O3 (I01).** `TokenRegex` no cierra el identificador por la derecha (GRD:52), y la regla 1b tiene `[^;{}]*?` (GRD:64-65). Con alias cortos como `r` o `p`, la regla puede sobredetectar por prefijo (por ejemplo, «`result as List<`»). Ese error cae del lado seguro, y hoy hay 0 coincidencias (`reglas-sin-coincidencias=True` en el RED y en el GREEN).

## 4. Las nueve correcciones

| Id | Lo que pide AN | Lo que hay en el diff (`a18ba345`) | ¿Completa? | ¿Puede fallar de verdad? |
|---|---|---|---|---|
| **I01** | AN:26. `Rule1b(member, aliases.Keys)` en `Violations` y una autoprueba «alias + molde» con su `autoprueba-<x>=falla` | GRD:163-168: la R2 se aplica ahora con la regla 1 y con la 1b (`"R2b "`). GRD:237-239 añade k1 (el ejemplo de AN, alias `r` y `as`) y k2 (alias `p` y molde de estilo C). La línea esperada está en GRD:253 | **Sí**, con PKG:692 («para las reglas 1 y 1b») | **Sí, medido.** En p3 (MUTATION, con compilación) se quitó la R2b y el resultado fue `autoprueba-k1=pasa` y `autoprueba-k2=pasa`, con la primera diferencia en la posición 14 (`rv-a2/tests/p3/p3.trx`). Por lectura, ni la R1, la R1b ni la R1c sobre fichas ni la R2-regla-1 cazan k1 o k2: la ficha va seguida de `;` y el molde está sobre el alias. Solo las caza la R2b (GRD:63-64). No es tautológica |
| **I02** | AN:27. `fresh` con el mismo nombre que `anterior` | CPY:968-969: `NameOfText(PilotKit.Text("pb-31-v2-modulos"))`, que es el mismo nombre que da `Fixture` (CPY:97-101) | **Sí** | **Sí, por lectura** (ver M02). No se ejecutó ninguna mutación |
| **M01** | AN:28. Dos líneas base distintas e iguales en la proyección; se muta `Name` y `Equals` no cambia | CPY:1318-1334 y 1359. `projB` adopta una línea base nueva resuelta del diseño, y `lineas-base-distintas` se comprueba con `ReferenceEquals`. `Name` mutado da `iguales=True`. El **control** (la `Length` de un módulo) da `iguales-tras-mutar-un-modulo=False` (`||` en los dos sentidos) | **Sí** | **Sí.** Si el comparador leyera `Name`, `iguales-tras-mutar-el-nombre` daría `False`, y el control muestra que la comprobación puede cambiar |
| **M02** | AN:29. «editar» muta todos los portadores de D-PBT1-02 | CPY:1194-1223. Están los 21 portadores públicos del estado, es decir, todo lo público salvo `Editor` (`PushBackEditableState.cs:59-95`): `Pallet`, `PalletsDeep`, `PostCatalogId`, `PostPeralte`, `PalletTolerance`, `BeamDepth`, `AnnotationScale`, `FirstLevelDatum`, las tres casillas, `Dimensions`, `DimensionStyle`, `LoadedDimensionViews`, `DimensionViewsTouched`, las tres casillas de cotas, `Advanced` con sus seis avanzados, `Name`, y `SafetySelections` con una añadida y una cambiada. Sobre `Rich`, ninguna asignación deja el valor igual al de `Enrich` (CPY:211-237) | **Sí** | La línea no cambia (`editar huellas-iguales=True`). Lo que discrimina son las huellas profundas, que ya existían. La corrección amplía lo que ejercen |
| **M04** | AN:30. Un segundo estado rico, `pb-20-ab` con B retirado, y la comprobación de que `undo()` deshace cada mutación | CPY:201-243 (`Enrich`, `Rich` y `RichRetired`), CPY:616-645 (el bucle sobre los dos estados) y CPY:644 (tras `undo()`, el estado es igual a `start` en los dos sentidos). Las líneas nuevas están en CPY:662-663 y CPY:676-680 | **Sí** (O1) | **Sí.** Si `undo` no restaurara, el comparador lo vería: el mismo bucle ya comprueba que la mutación no se pierde (`Eq(copy, start)` es falso). La comprobación contra `start` tras cada `undo` cubre además el «B = `Copy(R)` por ranura» de la revisión del intento 1 |
| **M06** | AN:31. Medir desde el contenido, la cola, la sesión y el commit, sin restar lo que alcanza la línea base, con sesión y commit no nulos | CPY:278 y CPY:306 (el parámetro `skip` de `AllObjects`) y CPY:730-756. Se recorre todo el estado salvo los dos campos de clase D, sin `ExceptWith`. La sesión escenificada y el commit se ejercen, y `sesion-no-nula` y `commit-no-nulo` se comprueban | **Sí** (O2) | **Sí.** Un alias del contenido, la sesión o el commit con el diseño cuenta aunque la línea base también lo alcance |
| **M07** | AN:32. La sentencia completa en el censo, y el escritor **dentro** de `DrawPushBackView` | GRD:268-274 (`NameStatement = NameWriter + "[^;]*;"`). GRD:386-392: las siete sentencias, que coinciden con `src/RackCad.Plugin/RackMenuCommands.cs:64-65, 80, 102, 121, 135` y `RackPushBackCommands.cs:98 y 190` (espacios colapsados). GRD:277-295 y GRD:332: `MethodText`, `WriterInsideDrawPushBackView` y el orden | **Sí** en la guarda | **El censo, sí:** un cambio del lado derecho cambia la línea. **«Dentro», sí por lectura.** Pero la autoprueba nueva no lo demuestra (M01) |
| **M08** | AN:33. «Restaurar tras `SetGap(60)`» en los 19 diseños, y «retirar B» solo donde hay B, declarado en una línea | PAR:185-196 (`TieneLadoB`, medido del diseño). PAR:516-523: SetGap en todos los diseños y retirar B si hay B. PAR:525-528: la línea de declaración. PAR:563 y PAR:574-579: lo esperado, con la lista literal | **Sí** (M05) | **Sí.** Lo medido (`TieneLadoB`) se compara con la lista literal (`ConLadoB`), y las 16 líneas nuevas comparan `Persist` de la ventana y del estado. En el RED, r1d muestra el orden nuevo |
| **M13** | AN:34. Toda llamada al API nuevo por `Paso` en CPY, LOAD y GATE, y CT-05 distingue los dos rechazos | CPY:55-56 (`Eq` por `Paso`), CPY:769-776 (`Battery`) y CPY:1059 (`BuildInputs`). LOAD:50-51, LOAD:299-300, LOAD:363 y LOAD:515-517. GATE:48-63 y GATE:83-85. CT-05: `RECHAZADO-POR-EL-ALMACEN` (LOAD:447) y `RECHAZADO-AL-CARGAR-EL-ESTADO` con `<no lanza>` (LOAD:457-460), y las demás cargas por `Paso` (LOAD:464). No queda ninguna llamada al API nuevo sin `Paso` en esas tres clases: busqué el uso de cada miembro nuevo. La única excepción es la que prescribe el paquete: el ayudante `Pasada` (PKG:798-815; CPY:67-80, LOAD:86-100) | **Sí**, en las tres clases (M04 sobre OWN) | **Sí:** c-12 y c-13 ya no comparten rótulo, y si c-13 no lanzara, la línea diría `<no lanza>` |

**«Nada más que eso».** Repasé los hunks de `git diff -U0 2f5d3dec a18ba345 -- tests`:
- Cada uno es de una de las nueve correcciones, más un cambio mecánico que impone M06: el grupo de métodos pasa a lambda en CPY:710 y CPY:724, porque `AllObjects` tiene ahora un parámetro opcional.
- Lo declarado como no corregido no se toca ni se debilita:
  - `Watch` (M03) no cambia;
  - en CT-08 y CT-09 (M05) solo entra `Paso` en `Battery` y en su llamada (CPY:765-789 y CPY:873-874), y una excepción sigue dejando una línea `EXCEPCION`, así que la prueba sigue fallando;
  - `src/` tiene el diff vacío (M09, M11, M12 y M15);
  - M10 y M14 están ratificados (F4B-E22).
- No queda la sonda temporal (`git grep TmpPbt1b a18ba345` vacío), no hay ayudantes duplicados y no cambia el número de `[Fact]` ni de `Assert.` por clase (7, 5, 1, 2, 1 y 1).

**Las líneas nuevas frente a la medición p2.** Las que la entrega lista en el PASO 1b coinciden literalmente con el código: CT-14 k1 y k2; CT-10; CT-16 `comparar-proyeccion…`; las tres de CT-06; la parte 3 de CT-07; las siete sentencias y la tercera autoprueba de CT-15; los dos rótulos de CT-05; y las 16 líneas y la declaración de CT-01. Las desviaciones frente al paquete están en `Deviations[0]`: 15 autopruebas en lugar de las 13 de PKG:913, y 3 en lugar de las 2 de PKG:914. Que se midieran *antes* del RED solo está probado para la guarda (M03).

## 5. El régimen RED/GREEN

**Los blobs (`git rev-parse <sha>:<ruta>`):**

| Archivo de `src/` | `2d9febc6` = `126fd65f` (RED) | `a0a5f699` = `2f5d3dec` = `a18ba345` (GREEN) |
|---|---|---|
| `PushBackEditableState.cs` | `339af624` | `b9f3dc39` |
| `PushBackEditableStateComparer.cs` | `098fbebc` | `df61d99c` |
| `PushBackEditableStateOwnership.cs` | `10408dc2` | `060ee692` |
| `PushBackEditorOutputGate.cs` | `4f6821bc` | `a454313a` |
| `PushBackEditorState.cs` | `5e243f09` | `7b9ce869` |
| `RackModuleEditSession.cs` | `6ba3bb11` | `bfa98b3a` |

**Los diffs:**
- `git diff 2d9febc6 126fd65f -- src`: vacío.
- `git diff 2f5d3dec a18ba345 -- src`: vacío.
- `git diff 126fd65f a18ba345 -- tests`: vacío. El GREEN toca solo los seis de `src/`.
- `git diff 2f5d3dec a18ba345`: cinco archivos, todos bajo `tests/` y dentro del AllowedWriteScope.
- `rv-a2/tests/r1/red-skeleton-src.patch` es idéntico, salvo los fines de línea, a `git diff 2f5d3dec 126fd65f -- src` (1 385 líneas).
- Los padres: `126fd65f^` = `2f5d3dec` y `a18ba345^` = `126fd65f`. Los árboles coinciden con los TreeSha de la entrega.
- Los dos commits llevan `Co-Authored-By: Claude Sonnet 5.5`, igual que `Worker.Trailer` en el handoff.

**El RED:**
- **El run 44430615074,** consultado con `gh`:
  - `headSha` = `126fd65f`, evento push, rama exacta;
  - Tests: failure; UI Tests: failure; Build UI: success; Build Plugin: skipped (M14, ratificado).
- **`rv-a2/red-ci-core/core.trx`:**
  - 19 086 resultados: 14 Failed (los 13 de `PushBackEditableState*` y el de `PushBackEditorOutputGate`) y 19 072 Passed;
  - las dos guardas, Passed;
  - todos los fallos son `Assert.Equal() Failure: Collections differ`, con la primera línea del seguimiento en el `Assert.Equal` final de cada método. Lo comprobé en `126fd65f`: LOAD:294, 335, 381, 480 y 585; CPY:669, 758, 885, 944, 1010, 1064 y 1352; GATE:95; OWN:118.
- **`rv-a2/red-ci-ui/ui.trx`:** 2 437 resultados: 1 Failed (CT-01, en PAR:559), 2 415 Passed y 21 NotExecuted.
- **Frente al GREEN:** el mismo conjunto de nombres, sin altas ni bajas, y solo esos 15 cambian de resultado. **Ninguna prueba existente falla.**
- **I-06:** ningún método de los filtros con ExpectRed pasa sobre el esqueleto (13/13, 1/1 y 1/1 Failed). Las corridas locales r1a a r1d, de 18:24 a 18:27, dan lo mismo: 13 F, 1 F, 2 P y 1 F. `ct-red-assertions.txt` es coherente: líneas `EXCEPCION` y valores del esqueleto.

**El GREEN:**
- **El run 44438060317,** consultado con `gh` y con el log de `gh run watch`: `headSha` = `a18ba345`, evento push, rama exacta, **4/4 success**.
- **`rv-a2/ci-core/core.trx`:** 19 086 Passed.
- **`rv-a2/ci-ui/ui.trx`:** 2 437 resultados: 2 416 Passed y 21 NotExecuted, todos existentes (SUS-01 a SUS-04 y las caracterizaciones de base de I-39B, C y D).
- **Las cifras** son las del intento 1, sin métodos nuevos, como prevé AN:53.
- **En local:**
  - g1, g2 y g3 dan 13, 1 y 1 Passed tres veces (de 18:47 a 19:08), sobre las compilaciones Debug de `rv-a2/tests/gr/build-*-debug.log`;
  - `full-core` da 19 086 Passed;
  - frente a la CI, los nombres solo difieren en 17 casos, por cómo se escribe ∞ frente a `Infinity`, una diferencia de plataforma que ya existía.
- **Determinismo de lo nuevo:**
  - sin reloj ni aleatoriedad;
  - bool e int concatenados, sin formato de cultura;
  - rutas por `RepoRoot` e inyección en memoria;
  - sin estado estático nuevo;
  - M01 muta una línea base resuelta de una deserialización nueva, y `PushBackResolver` no tiene caché (solo campos `readonly`);
  - `CorpusNames` va en orden ordinal;
  - sin dependencia de orden nueva.

  Las duraciones locales varían mucho con la carga: CT-06 tarda entre 58 s y 4 min 5 s, y CT-01 entre 40 s y 1 min 37 s. Pero en ninguna corrida terminada saltó `blame-hang`.

## 6. La interrupción de transporte

- **Está declarada:** en `WorkCompleted`, en `Deviations[1]`, en `Evidence` (`p2a.log`, «interrumpida») y en la nota de la sesión.
- **Concuerda con lo medido:**
  - `p2a.log` se para en «Iniciando la ejecución de pruebas» y no tiene TRX;
  - p2a2 se repite a las 18:14:30, nada más reanudar, y es la única corrida repetida;
  - p2a no está en `TestsExecuted`, porque no tiene TRX, y no se cita como resultado.
- **No queda nada a medias ni duplicado en las pruebas:** las clases compilan en la CI, no hay restos de la sonda ni ayudantes repetidos, y el número de métodos no cambia.
- **Sin explicar:** por qué p2a no terminó en los unos 11 min que hubo entre su lanzamiento (16:43) y el cuelgue (16:54). p2a2 tardó 3 min 41 s. Ver §9.

## 7. Contención

- **El perfil:** `cleanliness-before.txt` y `cleanliness-after.txt` son idénticos. El layout del configurador tiene `sha256=FF7E9F92…` y fecha `22:09:11Z`, anterior al inicio de la invocación (22:31Z) y sin cambios. El listado de la biblioteca tiene `sha256=EC9EA77D…`.
- **Las pruebas nuevas no escriben nada.** La inyección es en memoria (GRD:203-207).
- **El Plugin** se compiló en Release solo hacia `artifacts/…/plugin-out` (b0, r1 y gr), una carpeta que ignora `.gitignore:3`. `src/RackCad.Plugin` no cambia, y CT-15 solo lee su texto.
- **No hay AutoCAD.**

## 8. Hallazgos OWNER

**Ninguno.**
- M08 extiende «Restaurar tras `SetGap(60)`» a los 16 diseños de un sentido, y los 16 dan `iguales=True`, con el mismo veredicto de bloqueo en la ventana y en el estado. Lo prueban la CI y g1 a g3.
- La normalización de `pb-15-sin-composite` sigue siendo la conocida (F4B-E20).
- No aparece ninguna diferencia ni pérdida nueva.

## 9. Lo no verificado

- **Con qué binarios corrió p2** (M03). Puede que p2c, p2a2 y p2d ejecutaran una compilación sin las correcciones. Solo p3, r1 y el GREEN prueban qué texto se compiló.
- **La causa de que p2a no terminara.** No había ningún testhost vivo a las 00:12Z, y no veo indicios de un cuelgue de prueba en ninguna otra corrida. Pero no está demostrado.
- **La discriminación de I02 y de la comprobación «dentro» de M07 en ejecución.** Solo lo he razonado por lectura.
- **El comportamiento en ejecución en general.** No ejecuté pruebas.
- **El origen de la fecha 22:09Z del layout,** que es de una fase anterior. Tampoco los procesos y el perfil más allá de las dos pruebas de limpieza.
- **Que el árbol de trabajo no tuviera nada más que lo comprometido** durante las corridas locales. Solo verifiqué los commits.
- **Las verificaciones del Controller** (F4B-E15) **y nc1 a nc3,** que están fuera de este encargo.
