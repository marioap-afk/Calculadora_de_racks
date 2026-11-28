# Revisión adversarial independiente del código de PB-T4e (I-64 F4B, `F4B-PBT4-EDIT-ALL-RELOAD`, intento 1)

**Objeto:** `git diff 7da361ebe8919bf31e2874c42dc946b98c62635a 0eac7911a025a59628b446db8b768035175f96de`, con el RED `265f9e31cb33ce312c04d2d7959c999aee763b3c` y el GREEN `0eac7911a025a59628b446db8b768035175f96de`. WorkRunId `R20261123T045827Z-6c9e`.

**Cómo se leyó:** el código y los documentos, solo por objetos git desde el checkout principal (`git show`, `diff`, `log`, `grep`, `ls-tree`); la entrega, por la copia en `rv/`; la CI, con `gh run view` y `gh api`, solo de lectura. No leí ni listé el worktree de la rama, y ninguna orden mía lo nombra. No ejecuté dotnet ni pruebas. Las comprobaciones mecánicas las hacen unos guiones de Python de solo lectura, escritos con la herramienta de archivos, en `myrev/`, junto a este informe. Las citas `ruta:línea` son de `0eac7911` salvo que diga otra cosa; las de las pruebas, de `265f9e31`, que tiene el mismo blob.

## Veredicto

**ACEPTABLE CON MENORES.**

**Recuento: B 0 / I 0 / M 1** (M-01, que también va al Owner como OWNER-01, solo informativo).

La entrega es el texto literal de las dos piezas, byte a byte, dentro de las 6 rutas. El diseño de D-PBT4-22, D-S17, D-S18 y D-S19 está en el código tal cual. Las 4 CT fijan sus 25 líneas del §5.7. El RED cae por aserción y el GREEN pasa, con los totales exactos de cada rama. No encontré ningún defecto del código entregado. El único menor es una nota que le falta a la guía del Smoke-PB.

## Tabla de hallazgos

| Id | Sev. | Dónde | Qué | Qué hacer |
|---|---|---|---|---|
| **M-01 (OWNER-01)** | M (informativo) | `WorkspaceHostCore.cs:1215-1219` y `:1228`; `WorkspacePilotFieldViewModel.cs:465-490`; `WorkspaceHostCore.cs:176-180`; `src/RackCad.Plugin/Workspace/WorkspaceController.cs:145-149` y `:348-351`; el paquete, §5.5, pasos 3 y 8 (`I-64-f4b-pbt4-gate-contract-package.md:2064` y `:2069`) | La recarga ocurre dentro de `TryEditAll` y el modal se abre justo después, en la misma llamada síncrona. El campo solo se vuelve a pintar (`Render()`, FVM:489) cuando `TryEditAll` vuelve, es decir, al cerrarse el editor. Entrar en el modal (`EnterPanelModal` → `SetModalRackCadActive`) solo pone la marca, sin refrescar el panel. Así que, según el código (no lo ejecuté), con la ventana abierta la paleta, deshabilitada, sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes del cambio de fuera, mientras la ventana enseña el valor recargado. Al cerrar, el campo queda bien (CT-PBT4E-03, `recargado-y-cerrado`). No es un texto ni un estado nuevo, ni un cambio de «Editar». Es la misma falta de repintado que PB-T4c ya tiene mientras el editor entrega cambios al borrador. Pero los pasos 3 y 8 del Smoke-PB no dicen qué se espera ver en la paleta con la ventana abierta. | Que la guía del Smoke-PB lo diga, para que el Owner no lo lea como un fallo de la recarga. No pide código, ni intento, ni decisión. |

**Observaciones que no cuento como hallazgos** (anteriores a PB-T4e, y el alcance no deja tocarlas, C10):
- El resumen del puerto (`IWorkspacePilotEditPort.cs:19`) dice «only the six operations of the edit intention read the drawing». El de `BeginEdit` (FVM:375) dice «It is the only entry that reads the drawing». Los dos son inexactos desde PB-T4c, porque «Editar todo» lee al empezar, y ahora también al recargar. La primera frase la fija `WorkspacePilotEditGuardTests.cs:1083`, y D-PBT4-22, punto 3, decidió conservarla. El comentario nuevo de `TryEditAll` (`IWorkspacePilotEditPort.cs:117-124`) sí lo dice bien. Queda para una tarea de documentación futura.

## 1. Alcance y forma

- **El alcance.** Lo comprobé con `git diff --name-status`.
  - `7da361eb..265f9e31`: solo dos `A`, `tests/RackCad.Tests/PushBackEditAllReloadTests.cs` y `tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs`.
  - `265f9e31..0eac7911`: solo cuatro `M`, los 4 archivos de `src/` del alcance.
  - `7da361eb..0eac7911`: exactamente las 6 rutas del AllowedWriteScope, ninguna del ForbiddenWriteScope (352 entradas; `myrev/scope.py`).
  - `src/RackCad.Domain`, `src/RackCad.Plugin` y `docs/` no cambian.
  - El RED es hijo directo de BaseSha, y el GREEN hijo directo del RED. La rama remota está en `0eac7911`.
  - `src/` y `tests/` de BaseSha son los de `160e019d` (`git diff --stat 160e019d 7da361eb -- src tests` vacío).
- **El RED.** El sha256 de cada blob en `265f9e31` es el `NewFileSha` de `RedFiles`: `9e849864709a964a…` (Core) y `a29d7a362cd5de49…` (UI). Los blobs no cambian en `0eac7911`. Tienen LF y no tienen BOM. Los cuatro archivos de `src/` en el RED son los de BaseSha, así que no hay esqueleto.
- **El GREEN.** El sha256 de cada blob es el de `GreenFiles`: `6eb9bf7a…` (WHC), `bbc68822…` (WEI), `49b5245c…` (el puerto) y `c4b5c699…` (FVM).
- **Fidelidad a las piezas,** con un aplicador propio (`myrev/verify_pieces.py`), no la herramienta del paquete:
  - las piezas `pbt4e-green-v56.txt` (`a745c273…`) y `pbt4e-red-v56.txt` (`d052f73d…`), leídas de git, aplicadas a los blobs de BaseSha, dan byte a byte los blobs del GREEN y del RED;
  - cada OLD aparece exactamente una vez;
  - el anexo V56-A del paquete (sha256 `6b8939ae…`, sin cambio entre BaseSha y el GREEN) contiene byte a byte la pieza del GREEN y los dos archivos del RED (`myrev/verify_annex_ct.py`);
  - el generador y las herramientas custodiadas tienen el sha256 que dice el encargo: `8c38e532…`, `b3aba680…`, `59a0fcb3…` y `492aaab1…`.
- **WEI y el puerto, solo comentarios.** Quitadas las líneas `///`, los dos archivos son iguales en BaseSha y en el GREEN. Las líneas de código que cambian son exactamente las de C13:
  - en WHC, `begunHere` (:1195, :1202) y la rama de la recarga (:1215-1219);
  - en FVM, la constante (:70), la línea de `BeginEdit` (:411) y la de `EditAll` (:482).
- **Las declaraciones (C13).** Una sola nueva: `private const string NotReloadedText` (FVM:70). No se quita ninguna, y no hay ninguna sobrecarga ni ningún tipo anidado nuevos en `src/`. Coincide con el censo del Worker (`rv/inv/census/`, FVM 189 → 190, la única diferencia `WorkspacePilotFieldViewModel::const NotReloadedText`). Las copias del censo tienen el sha256 de los blobs. `begunHere` y `reload` son locales, prescritas por C13.
- **El texto de «Editar» no cambia.** El valor de la constante es, byte a byte (`ó` = `C3 B3`), el literal que `BeginEdit` tenía en BaseSha.

## 2. La lista vacía y C02

- `git diff 7da361eb 0eac7911 -- tests/` solo añade los dos archivos nuevos.
- Ninguna prueba ni ningún ayudante existente se edita, se salta, se reactiva ni se borra.
- No se tocan `InternalsVisibleTo`, paquetes, `.csproj` ni `Fixtures/I64`.
- **Los `Skip`:** el conjunto de `NotExecuted` de Core es el mismo en la base local (`rv/tests/b0/core-full.trx`), en la CI del RED y en la del GREEN (403 filas). Las únicas diferencias de nombres son la forma de escribir `∞`/`Infinity` entre Windows y Linux, y las 3 CT nuevas.
- **Las 7 filas de D-1 (6, PB-T3b) y de CT-PBT3A-12 (1, PB-T4a)** siguen saltadas en las tres corridas (`myrev/skips.py`).
- **UI:** 48 `NotExecuted`, el mismo conjunto en el RED y en el GREEN. Con la base de la CI solo pude comparar el recuento (48), no los nombres.
- **La entrada «## PB-T4e»** de `superseding-characterization.md` dice la lista vacía, con K, D y E a 0, y los dos `NewFileSha` iguales.
- **Ninguna prueba existente falla** (C16), ni en la CI del RED ni en la del GREEN.

## 3. El diseño frente al código

| Diseño (D-PBT4-22, D-S17 a D-S19; INV-PBT4E-03 y 04) | Código | ¿Cumple? |
|---|---|---|
| La recarga va en el anfitrión, en `EditAllOfPilot` (D-S17), dentro de la marca de lectura de `TryEditAll` | WHC:1215-1219, en el cuerpo privado; el envoltorio pone `_pilotReadInProgress` (WHC:1163-1175) y la rama llama directamente a `TryReloadPilotEdit`, no por el puerto | Sí |
| Va después de la compuerta, de la actualización en vuelo y del anfitrión del editor | Compuerta en WHC:1185-1188, actualización en vuelo en :1190 y `_fullEditHost` en :1191, todo antes de `FindPilotDraft` (:1194) | Sí |
| Va después de las tres comprobaciones de la vista y antes de `ShowModal` (D-S19) | `Applying` o no registrado (:1206), `!view.CanEdit` (:1207) y el texto pendiente (:1208), luego la recarga (:1215-1219) y luego `ShowModal` (:1228) | Sí |
| Solo sobre un borrador que el gesto no acaba de empezar y con `NeedsLazyReload` | `if (!begunHere && draft.NeedsLazyReload)` (:1215). `begunHere` empieza en falso (:1195) y pasa a verdadero solo tras empezar y encontrar el borrador (:1202). `NeedsLazyReload` es `_registered && IsPossiblyStale && Status == Clean` (`DraftSession.cs:306`) | Sí |
| La llamada es `TryReloadPilotEdit(request)`, no una copia, guardada en `reload` | `var reload = TryReloadPilotEdit(request);` (:1217), el método de :498-576. `return TryReloadPilotEdit(request);` solo aparece en :1109, así que la autoprueba de `WorkspacePilotEditGuardTests.cs:574` sigue mutando la de `TryReload` | Sí |
| Con `Reloaded`, abre sobre el mismo borrador | `DiscardAndReload` cambia el mismo objeto (`DraftSession.cs:422-437`: copia, `Rebase`, `_current` y `_pending.Clear()`). El puerto y el lanzamiento se arman después, con ese `draft` (:1223-1228). CT-01 da `mismo-borrador=True` y `nombre-del-lanzamiento=[Otro nombre]` | Sí |
| Con cualquier otro resultado devuelve `NotBegun(<la recarga>)` y no abre | `if (reload.Status != EditIntentStatus.Reloaded) return EditAllOutcome.NotBegun(reload);` (:1218). La fábrica no filtra nada (WEI:326) | Sí |
| Nada evalúa `NeedsLazyReload` antes de las tres comprobaciones | En WHC solo aparece en :527 (dentro de la recarga) y en :1215. `ViewOfPilotDraft` protege `Status` (:1870-1880) y solo lee la marca como un bool | Sí |
| Como mucho una lectura por gesto, y ninguna durante `ShowModal` | O la de empezar (:1198, con `begunHere` verdadero) o la de recargar (:1217), nunca las dos. La recarga va antes de :1228. En el campo, `GetField` y `CommitField` son de memoria, y `TryEditAll` es la única que lee (FVM:443-465) | Sí, por la estructura |
| El campo, con `NotBegun` y `NotAdmitted` sobre un borrador que ya existía (D-S18), pone el diagnóstico, la pista y `NotReloadedText` | FVM:475-483. `view` es el `GetField` del principio del gesto (:443). Sobre un borrador que existe, el anfitrión nunca lo empieza, así que ese `NotBegun` solo puede ser el de la recarga | Sí |
| `BeginEdit` usa la misma constante sin cambiar lo que muestra | FVM:411, mismo texto byte a byte | Sí |
| Los demás resultados salen como en «Editar» | `EditAllNoticeOf` → `NotBegun` → `Describe(Begin.Status, Begin.Reason)` (FVM:1349 → :1326-1338), la misma función que «Editar» usa en :414 | Sí |
| Los comentarios son solo comentarios | WEI:276-279 y :311-314; el puerto, :117-124; FVM, :427-433 y :480-481; WHC, :1210-1214 | Sí |
| El comentario del puerto conserva las dos frases que fija la guarda y no lleva las dos que prohíbe | «only the six operations of the edit intention read the drawing» (`IWorkspacePilotEditPort.cs:19`) y «while one of the six reads is in progress» (:24), una vez cada una. «only the three operations» y «one of the three reads», ninguna vez. La guarda (`WorkspacePilotEditGuardTests.cs:1078-1087`) pasa en la CI | Sí |
| Sin aviso cuando recarga | Con `Closed` sale el texto de siempre (FVM:1345). CT-03 da `recargado-y-cerrado … aviso=[Editor cerrado sin cambios en el borrador.]` | Sí |
| Dirty, Conflict y sin la marca, como en BaseSha | `NeedsLazyReload` es falso: Dirty por el comparador; Conflict tiene estado explícito y no llama al comparador. CT-01: `marcado-sucio`, `marcado-conflicto` y `sin-marca`, con 0 lecturas, y abren | Sí |
| «Sin decidir», Unknown, Error, Applying y con texto pendiente, como en BaseSha | Responden en :1206-1208, antes de mirar la marca. Además, Unknown, Error, Applying y Conflict tienen estado explícito, así que para ellos `NeedsLazyReload` sería falso en cualquier orden. El único estado que dependía del orden es «Sin decidir», y CT-01 lo fija (`marcado-sin-decidir … estado=Refused … fallos=[]`) | Sí |
| Ningún miembro nuevo del puerto y ningún valor nuevo de `EditAllStatus` ni de `PilotBoundaryKind` | WEI y el puerto, iguales sin los comentarios | Sí |
| El botón no cambia y la recarga es alcanzable | `ShowEditAllButton` (FVM:237-238) no cambia, y se ofrece sobre un borrador Clean marcado, porque `CanEdit` es verdadero | Sí |

**Los casos del borde que revisé:**
- **El texto pendiente igual al valor confirmado:** queda Clean tras `CommitField` y se recarga. Está declarado en D-S17 y en D-PBT4-22 (`package.md:1451`), y no pierde nada.
- **Un testigo sin clave en la petición:** con un borrador Clean marcado, ahora la recarga daría `NotBegun(Failed)`. Ningún llamador pasa testigo: el campo usa `null, null` (FVM:465) y el Plugin solo reenvía (`WorkspaceController.cs:332`).
- **`NotNeeded` y `NotFound` de la recarga:** no se alcanzan en la misma llamada síncrona.

Nada de esto es un defecto.

## 4. (a) Las desviaciones y los hallazgos inesperados de la entrega

- `Deviations`, `OpenQuestions` y `TriggeredStopConditions`, vacíos. `WorkerStatus` es `IMPLEMENTATION_COMPLETE`, y no hubo reparación del RED.
- **`UnexpectedFindings`:**
  1. **Las órdenes largas.** Pasaron a segundo plano por la herramienta de shell. En tres llamadas el Worker lanzó `gh run watch` con `&` o con `Start-Process` y lo esperó en la misma llamada. Además, paró una espera hecha con `ping`. El relevo midió 0 procesos persistentes y 0 no atribuibles. No cambia nada que fijen el contrato o el paquete: ni el código, ni la evidencia, ni los totales. No pude contrastarlo con las reglas de proceso de la delegación, que no leí; lo juzga el Controller.
  2. **El aviso LF → CRLF de `git add`.** Los blobs tienen LF y el sha256 de la pieza; lo comprobé yo.
  3. **La prueba de limpieza no cambió.** Lo confirmé: los cuatro `rv/inv/cleanliness-*.txt` tienen el mismo sha256, `66ef03a5…`.
- **`KnownLimitations`:** el campo `Command` lleva la ruta absoluta del `dotnet.exe` de usuario (205 entradas). Todos los `ResultsFile` y las 21 entradas de `Evidence` son relativos, y `TreeSha` es null, que INV-PBT4E-11 admite. No cambia nada.

**Conclusión:** ninguna cambia lo que fijan el contrato o el paquete.

## 5. D2 y su límite

- **Las líneas añadidas con código** son seis: `var begunHere = false;`, `begunHere = true;`, la condición, `var reload = TryReloadPilotEdit(request);`, el `return EditAllOutcome.NotBegun(reload);` y las dos asignaciones de `_noticeText`, más la constante.
  - Ninguna hace una copia.
  - Ninguna nombra ni muta un `PushBackSystem` ni un `RackModuleReconciliationResult`.
  - Ninguna comparte un objeto mutable.
- **El único camino que se reutiliza** es `DiscardAndReload` (`DraftSession.cs:422-437`), que no cambia.
  - Hace sus dos copias (:431-432) con el copiador del borrador (`PushBackFullDraftStateComparer.Copy`, fijado al empezar, WHC:372-377) antes de asignar nada.
  - Sustituye referencias; no muta ninguna en sitio.
  - Es la condición 6 de D2 («discard/reload does not mutate either shared object»), ya vigilada por las guardas de referencias compartidas del contrato: `PushBackSharedReferenceGuardTests`, `PushBackWindowSharedReferenceGuardTests`, `PushBackDraftPortSharedReferenceGuardTests` y `PushBackWorkspaceSharedReferenceGuardTests`. Las cuatro están en verde en la corrida local del GREEN (`rv/tests/ga/check.txt`) y en la CI.
- PB-T4e solo hace que ese camino se alcance también desde «Editar todo». Desde «Editar» ya se alcanzaba, y después se podía abrir «Editar todo».
- El lanzamiento del editor se arma **después** de la recarga (:1223-1228), sobre el estado nuevo.
- **INV-PBT4E-14 y C17:** se cumplen.

## 6. Las CT

- **Las líneas.** Las 25 líneas esperadas de las dos clases son, en orden y byte a byte, las de la tabla del §5.7 (`myrev/verify_annex_ct.py`):
  - CT-01: 7 líneas, con `marcado-sin-decidir` de D-S19 (`PushBackEditAllReloadTests.cs:260`);
  - CT-02: 4;
  - CT-04: 5;
  - en total, las 16 de Core;
  - CT-03: las 9 de UI.
- **Un `[Fact]` por CT:** 3 en Core y 1 en UI. No hay ninguna `[Theory]`, ningún `Skip` ni `#nullable`. Cada clase acaba en un único `Assert.Equal`, en su `Finish` (Core :49-50, UI :45-46). La regla «EXCEPCION» se aplica con `Paso` (Core :34-47, UI :33-43).
  - El `Assert.True` de `marcado-conflicto` (Core :207) va dentro de `Paso`: si fallara, sería una línea `EXCEPCION`, nunca una excepción que escapa.
- **Los dobles van dentro de su clase**, como clases anidadas privadas: `EditorDouble` (Core :57-96), `Setup` (:99-126) y `PortDouble` (UI :50-167). Comparten el nombre con las anidadas de PB-T4c, pero se resuelven a las propias.
  - No se referencia ninguna clase de PB-T4c (comprobado contra los tipos de sus 9 archivos).
  - Los ayudantes que se usan son los que nombra el §5.7: `FullReadRig`, `FullEditIntentFakeReader`, `FullEditIntentKit` y `FullPrepareKit`, del kit de PB-T4b; `IntentKit`; `FullWriteKit` y `PilotEraseScopeDouble`, de PB-T3b. También `PilotEditThrowingComparerFull` (PB-T4b, `c1e4f458`), que el §5.7 prescribe para `marcado-sin-decidir`.
- **La CT de UI fija que el campo solo pide `TryEditAll`:** `llamadas=[GetField,TryEditAll,GetField]` en las cuatro líneas de «Editar todo», frente a `[GetField,TryReload,GetField]` en las de «Editar». Va en `EditorDiscardPromptCollection` (UI :19).
- **Las cuatro caen en el RED por su `Assert.Equal`, no por una excepción:**
  - en la CI del RED y en la corrida local `r1`, el mensaje es `Assert.Equal() Failure: Strings differ`;
  - la primera línea del StackTrace es `Finish`, bajo `tests/` (Core :50, UI :46);
  - la primera diferencia es la que da la tabla «CT -> la aserción que falla en el RED» del paquete: CT-01 en `lecturas=0`, CT-02 en `estado=Closed inicio=Unavailable`, CT-03 en `aviso=[]` y CT-04 en `lecturas=0` de `editar-todo`;
  - no hay ninguna línea `EXCEPCION` en las líneas completas de la sonda de la precondición 6 sobre el mismo texto (`cc4e/r82-tr/red-core-lineas.txt` y `red-ui-lineas.txt`).
- **Ningún oráculo es ciego:** la precondición 6 registra los controles r80 y r81, en los que cada CT solo mira su parte.

## 7. El RED y el GREEN (las TRX de `rv/`)

| Rama | Core (Passed + Failed + Skipped) | UI (Passed + Failed + Skipped) | Lo previsto (F4B-E46) |
|---|---|---|---|
| Base, CI `45057868915` sobre `7db6fd0c`; Core también en local, `rv/tests/b0` | 19 418 = 19 015 + 0 + 403 | 2 464 = 2 416 + 0 + 48 (según el contrato; no tengo su TRX) | — |
| **RED**, CI `45119881969` sobre `265f9e31` | **19 421 = 19 015 + 3 + 403** | **2 465 = 2 416 + 1 + 48** | Igual |
| **GREEN**, CI `45123886125` sobre `0eac7911` | **19 421 = 19 018 + 0 + 403** | **2 465 = 2 417 + 0 + 48** | Igual |

- **Los filtros.** En el RED, el sha256 de las TRX es el que registra el relevo.
  - Solo fallan las 3 CT de Core y CT-03: `Failed == Selected` en los dos filtros con `ExpectRed`, 3 de 3 y 1 de 1.
  - Los otros 88 filtros, en local sobre el RED (`r3`) y sobre el GREEN (`ga`), dan las mismas líneas: 0 violaciones.
  - Los dos filtros con `ExpectRed` pasan tres veces seguidas sobre el GREEN (`g1`, `g2` y `g3`).
- **`warning CS`: ninguno.**
  - Los 12 registros de build locales (b0, r0, r4 y g0; `--no-incremental`) tienen 0, y los mismos recuentos por código que la base: xUnit en Core y UI, y MSB3277 × 104 en el Plugin.
  - Los registros de la CI del RED y del GREEN tienen 0 `warning CS` y los mismos avisos xUnit que la CI base `45057868915`.
  - El Plugin de la CI del GREEN dice «succeeded without warnings».
- **Los jobs de la CI:**
  - RED: `Tests` failure, `UI Tests` failure, `Build UI` success y `Build Plugin` skipped;
  - GREEN: los 4 en success, con el Plugin;
  - sin cuelgue: el paso de pruebas de Core duró 4 min 14 s en la base, 4 min 25 s en el RED y 2 min 55 s en el GREEN; el de UI, 5 min 34 s, 4 min 31 s y 5 min 20 s;
  - las CT nuevas tardan 0,7 s o menos en la CI.
  - Las marcas de tiempo de cada TRX caen dentro de la ventana de su job.
- **La cobertura que se queda (INV-PBT4E-06):** la midió la sesión en la precondición 6, sobre el mismo texto literal. Su resultado es **PASA**: 0 líneas sin ejecutar, 0 ramas perdidas y 0 líneas sin pareja; 8 líneas nuevas, todas ejecutadas con todas sus ramas. Con la corrida opcional sin las clases nuevas, también **PASA**.
  - `cov-p6-v56.txt` (`057591b7…`) y `cov-p6-existentes-v56.txt` (`3887c2ef…`) son los que cita `pbt4e-precondition6.json`.
  - Como el GREEN es byte a byte el texto medido y `src/` y `tests/` de BaseSha son los de `160e019d`, la medida aplica. No la volví a medir: no ejecuto.

## 8. Contención

- **Ningún cambio observable de «Editar»:**
  - la línea :411 cambia el literal por una constante con los mismos bytes;
  - el resto de `BeginEdit` no cambia;
  - CT-03 compara lo que muestran «Editar» y «Editar todo».
- **El botón, la ventana, RACKEDITAR, el camino clásico, el Plugin y la escritura del dibujo no cambian:**
  - el diff no toca el Plugin;
  - `PilotEditAdapter.TryEditAll` y la compuerta de avisos solo reenvían (`WorkspaceController.cs:332`; `WorkspacePilotSupport.cs:209-210`), igual que `TryReload`;
  - el único llamador de producto de `TryEditAll` es FVM:465.
- **Ninguna escritura.** La recarga solo lee y cambia la memoria del borrador; «Actualizar» sigue siendo la única escritura. Las CT no escriben en disco: `RackEmbedStore` solo deserializa en memoria y `PilotEraseScopeDouble` es un doble. La prueba de limpieza del perfil y de la biblioteca es idéntica antes y después.
- **Las corridas de UI:** el Worker solo corrió los 19 filtros de UI del contrato más el nuevo (`TestsExecuted`: 20 filtros de UI, 0 fuera del contrato) y nunca la suite completa de UI.

## 9. Hallazgos para el Owner

- **Decisiones que el contrato no le dejaba al Worker: ninguna.** La entrega es la aplicación literal de las dos piezas, sin elección.
- **OWNER-01 (= M-01, informativo; no pide decisión):** el paso 3 y el paso 8 de la guía del Smoke-PB deberían decir qué se ve en la paleta mientras la ventana de «Editar todo» está abierta. Por el código, la paleta seguirá mostrando «Limpio (posiblemente obsoleto)» y el valor de antes del cambio de fuera hasta cerrar la ventana. Es la misma falta de repintado que ya hay mientras el editor entrega cambios, y al cerrar queda bien. Si en AutoCAD se viera otra cosa, sería una observación del Smoke, no un defecto de esta entrega.
- **Para el Smoke-PB, no verificado:**
  - El paso 5 (borrar fuera todas las vistas del rack y pulsar «Editar todo») supone que la pestaña del rack y su borrador Clean marcado siguen en el panel después del borrado. CT-02 lo modela con el lector (`OriginMissing`) sin cambiar la superficie.
  - Que en AutoCAD el navegador no cambie la pestaña activa al borrar las vistas no lo puedo comprobar sin ejecutar. Si cambiara, el paso no se podría hacer tal cual, pero no sería un fallo del código.

## Lo no verificado

- No ejecuté dotnet ni pruebas. Los resultados son los de las TRX copiadas en `rv/`, de la CI y de la precondición 6.
- La cobertura de INV-PBT4E-06: no la volví a medir. Me apoyo en la precondición 6, que aplica al mismo texto.
- La procedencia de las TRX de la CI: tienen el sha256 del relevo y sus marcas de tiempo caen en los jobs, pero no descargué los artefactos.
- La UI de la base: solo comparé el recuento de `Skipped` (48), no los nombres, porque no hay una TRX de la CI base en `rv/`.
- Las líneas completas que da el RED entregado: xUnit trunca el mensaje. Comprobé la primera diferencia; las líneas completas son las de la sonda de la precondición 6 sobre el mismo texto.
- El comportamiento en AutoCAD (Smoke-PB) y lo que muestra la paleta durante el modal (M-01): solo leído en el código.
- Las reglas de proceso de la delegación (el `gh run watch` en segundo plano y la espera con `ping`): no leí la delegación ni el prompt del Worker.
- El estado del checkout principal tras la cesión: no corrí `git status`, que queda fuera de las órdenes que permite el encargo. Me apoyo en el relevo.
- **Nota de método:** al principio usé una vez `python -c` para listar, solo leyendo, las claves del JSON del contrato desde la salida de `git show`. Es contra la regla del encargo. No escribió nada ni cambia ningún resultado; todo lo demás se hizo con guiones escritos con la herramienta de archivos. La ruta del worktree salió en la salida de un registro de build copiado en `rv/`, al leerlo; ninguna orden mía la nombró.
