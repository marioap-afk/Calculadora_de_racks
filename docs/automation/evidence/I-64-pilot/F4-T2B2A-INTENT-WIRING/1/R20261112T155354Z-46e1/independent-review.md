# Revisión adversarial independiente — I-64 F4-T2B2A-INTENT-WIRING

- **Rango:** `e277dd42..2d743e46`, con el RED `db37e30d` y el GREEN `2d743e46`; 13 archivos.
- **Worktree:** `architecture/workspace-persistente-rackcad`.
- **Modo:** solo lectura (`git show`, `git diff`, `git grep` y lectura de archivos). No compilé ni ejecuté pruebas, ni abrí AutoCAD. El worktree queda limpio.
- **Autoridades leídas:**
  - `gate-contract.json`: INV-F4T2B2A-01 a 12, `AllowedWriteScope` y paradas;
  - paquete v5 de F4-T2B2: §2, §3, §4 (D-T2B2-01 a 08), §5 (D-T2B2-09 a 14, para comprobar el reparto con T2B2B) y §6;
  - `task-close-package.md` §5 de T2B1;
  - D-T2B-03 a 05 del paquete de T2B;
  - Freeze v5: D-10, D-11, D-12, INV-14, INV-22 e INV-27.

## Veredicto

**NO ACEPTABLE**, por un único hallazgo IMPORTANTE (n.º 1). Ese hallazgo está en las **pruebas y guardas nuevas**, no en el producto.

- **El producto** cumple todos los invariantes de T2B2A que pude verificar, sin defectos de corrección (ver «Lo que comprobé»).
- **La corrección** es pequeña y cabe entera en `WorkspaceEditIntentGuardTests.cs` y `WorkspaceEditIntentHostTests.cs`.
- **La alternativa** es que el Coordinator lleve al Owner una enmienda del §3 y el §6 del paquete v5, para que F4-T2B2B pueda cambiar esas guardas como cambios declarados.

**Recuento:** 0 BLOQUEANTE, 1 IMPORTANTE, 8 MENOR.

---

## Hallazgos

### 1. IMPORTANTE — Las guardas y pruebas nuevas convierten en prohibiciones permanentes lo que el paquete asigna a F4-T2B2B, en archivos que el alcance de T2B2B no incluye

**Dónde (GREEN `2d743e46`):**
- `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:870`: `Assert.False(draft.IsPossiblyStale)` tras tres drenajes con `CommandEnds` (prueba `NoOperationIsCalledFromTheDrainAndTheDrainTouchesNeitherTheReaderNorTheDrafts`).
- `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:520`, con el mismo patrón en `:545`: prohíbe el texto `_drafts` en el cuerpo de `WorkspaceHostCore.OnDrained`.
- `WorkspaceEditIntentGuardTests.cs:489-492`: exige exactamente 2 `.ReadIntent(` y 2 usos de `_editIntentReader` en `WorkspaceHostCore.cs`.
- `WorkspaceEditIntentGuardTests.cs:559-571`: exige un único `.StaleBase` (el de `CheckNow`) y un único `pilot.Read`, y prohíbe `DiscardAndReload` en el core. `:580-583` vuelve a afirmar sobre el texto real que el core no contiene `DiscardAndReload`.
- `WorkspaceEditIntentGuardTests.cs:650-658`:
  - `IWorkspaceLifecycleAdvisor.cs` no puede existir;
  - `IWorkspaceLifecycleAdvisor|SetLifecycleAdvisor|AdviseClose|SessionDestroyed(` quedan prohibidos en todo `src`;
  - `EditIntent|Advisor|Pilot|Draft|ReadIntent` quedan prohibidos en `EventBridgeCore.cs` y `AutoCadEventBridge.cs`;
  - `.EvaluateClose(`, `.MarkPossiblyStale(`, `.RetireSession(`, `.Discard*(` y `.DiscardAndReload(` quedan prohibidos en el core, el host, el controlador y los dos archivos nuevos de Application.

**Escenario que falla.** F4-T2B2B se contrata tal como lo fija el paquete v5:
- **D-T2B2-11:**
  - el host marca los borradores en `OnDrained`, antes de los retornos tempranos, cuando el drenaje trae `MarkDraftsPossiblyStale`;
  - esa operación nace de cualquier `CommandEnded` (`WorkspaceSession.cs`, en `TryDrain`);
  - `MarkPossiblyStale` también marca un borrador Desconocido (`DraftSession.cs:393-399`).

  Fallan la prueba de `:870`, la guarda de `:520` y la de `:656`.
- **D-T2B2-12:** `TryReloadPilotEdit` «sigue el mismo camino que la intención», es decir, una tercera `ReadIntent`, y llama a `DiscardAndReload(result.Read)`. Fallan `:489-492`, `:563-571` y `:583`.
- **D-T2B2-09:** crea `IWorkspaceLifecycleAdvisor.cs`, `AdviseClose`, `SessionDestroyed` y `SetLifecycleAdvisor`, y define `AdviseClose = DraftClosePolicy.EvaluateClose`. Falla `:650-658`.

**Por qué viola lo acordado:**
- **Choca con el §3 del paquete.** El §3 limita los cambios de guardas existentes de T2B2B a uno solo: un nombre exacto más en la lista pública de `EventBridgeCore`.
- **Choca con el §6.** El §6 no incluye estos dos archivos en el `AllowedWriteScope` de T2B2B, y su parada «una guarda o prueba existente tendría que cambiar fuera de las declaradas» se activa con certeza.
- **Consecuencia.** Tal como está, la entrega deja a T2B2B en STOP o exige enmendar el paquete v5, lo que es una decisión del Owner.
- **La guarda de la obligación 10 no es la contratada.**
  - INV-F4T2B2A-08 fija «la base de un resultado solo llega a Begin, CheckNow o DiscardAndReload». Incluye `DiscardAndReload` precisamente para que la guarda sobreviva a T2B2B.
  - La guarda implementada dice otra cosa: «la base solo llega a CheckNow, la lectura solo a Begin, y DiscardAndReload está prohibido».
- **INV-F4T2B2A-09 no pide esto.**
  - El invariante pide que esta tarea no implemente el ciclo de vida, que ninguna operación nueva se llame desde el drenaje y que no se añada ninguna lectura al drenaje.
  - No pide fijar que el drenaje nunca toque los borradores. `Assert.False(draft.IsPossiblyStale)` afirma más que el invariante, y lo contrario de lo que D-T2B2-11 exigirá.

**Corrección sugerida (solo en pruebas):**
- **Drenaje.** Limitar el patrón a lo que es de T2B2A: `TryBeginPilotEdit|TryCheckPilotEdit|_editIntentReader|ReadIntent|PassGate`, sin `_drafts`. Quitar la aserción de `:870` o sustituirla por «ninguna lectura del lector».
- **Obligación 10.** Escribir la guarda tal como está contratada (Begin, CheckNow o DiscardAndReload), sin fijar conteos exactos que la recarga va a cambiar.
- **Ciclo de vida.** Acreditar su ausencia en esta entrega con la revisión del diff, que ya la confirma, y no con una guarda permanente.

**Nota sobre T3.** Lo mismo ocurrirá en F4-T3 con `:531-533`, que exige `Try(Begin|Check)PilotEdit(` solo en el core. Ahí es menos grave: T3 no tiene alcance fijado y el paquete ya prevé que declare sus propios cambios de guarda (H-03).

### 2. MENOR — La guarda de la obligación 10 no cubre el lado del Plugin, que es quien maneja el resultado del piloto

- **Qué cubre.** `WorkspaceEditIntentGuardTests.cs:559` solo cuenta `.StaleBase` en `WorkspaceHostCore.cs`, `WorkspaceEditIntent.cs` e `IWorkspaceEditIntentReader.cs`.
- **Qué falta.** No mira `RackPilotIntentAdapter.cs` ni `WorkspaceNavigationHost.cs`, que son el acoplamiento declarado en D-T2B2-05.
- **Escenario.**
  - `:384` exige un único `read.Pilot` en el host, pero `read.Pilot.StaleBase` o `read.Pilot.Read.InitialState` seguirían cumpliendo ese conteo.
  - Ninguno de los dos necesita nombrar un tipo del piloto, así que la prohibición de `:386` tampoco los detecta.
- **Estado actual.** Hoy el producto cumple, pero la guarda no lo fijaría ante una regresión en el Plugin.

### 3. MENOR — Testigo con clave nula frente a clave en blanco, y validación que llega después de una lectura completa del dibujo

- **Clave nula.** `WorkspaceNavigationHost.cs:246-247` pasa `witness?.Key`, y `RackPilotIntentAdapter.cs:109` trata `witnessKey == null` como «sin testigo».
  - Un `EditIntentWitness(null!, texto)` acaba en silencio como `NotSupplied`. Su `Key` es `string` no anulable solo por anotación.
  - En cambio, `""` o `" "` dan un fallo tipado (`:111-112`). Una misma entrada inválida recibe dos tratamientos.
- **Clave en blanco.** El fallo se detecta dentro del adaptador, es decir, después de:
  - `LoadCatalog`, con E/S de disco;
  - el bloqueo;
  - la autorización;
  - `Revalidate`, que recorre toda la BlockTable con `ReadDefinitionFacts`.

  Una petición estructuralmente inválida gasta así una lectura del dibujo.
- **Lado seguro.** Ninguno de los dos casos da `Unchanged`, así que INV-27 se mantiene.
- **Sugerencia.** Validar la clave del testigo en el paso 1 del core, o en el lector anidado antes de `LoadCatalog`, y tratar nula y en blanco igual.

### 4. MENOR — La comprobación Debug de `TopTransaction` queda incompleta frente a INV-14

- **En `Read`.** `RackPilotIntentAdapter.cs:104` y `:120`: la comprobación «after» solo corre en el camino de éxito.
  - No corre en el retorno anticipado por testigo (`:112`) ni en ninguna de las tres ramas `catch` (`:123-134`).
  - Es el mismo «solo en éxito» que H-10 señaló en el precedente `WorkspaceSelectionReader`.
- **En `ReadPreviewWitness`.** `:142-174` es una operación con su propio bloqueo y su propia transacción, y no tiene ninguna comprobación.
- **Frente a los invariantes.**
  - INV-14 del Freeze pide la comprobación Debug «al terminar cada operación».
  - La letra de INV-F4T2B2A-02 (antes y después en `Read`) se cumple; la de INV-14, solo en parte.
- **Arreglo.** Un `try/finally` en `Read` y la misma comprobación en `ReadPreviewWitness`.

### 5. MENOR — INV-F4T2B2A-01 dice «ninguna firma cambia», y la guarda que lo afirma lleva un nombre inexacto

- **El cambio.** `RackSiblingScan.cs:49` añade un sexto parámetro opcional, `rawEnvelopes = null`, al constructor `internal` de `RackSiblingDrawingScan`.
  - Es compatible en fuente y tiene un único llamador (`:166`), así que el riesgo es nulo.
  - Aun así, es un cambio de firma, y tanto el invariante como D-T2B2-01 dicen que ninguna firma cambia.
- **La guarda.** Se llama `NoSignatureOfTheScanChangedAndItStillWritesNothing` (`WorkspaceEditIntentGuardTests.cs:79`) y fija literalmente la firma nueva (`:91-93`). Hay que declarar la desviación o renombrar la prueba.
- **Riesgo latente.** Por el valor por defecto (`:56`), un constructor llamado sin `rawEnvelopes` produce en silencio un diccionario vacío, y el lector daría Unreadable para todos los miembros.

### 6. MENOR — Comentarios que afirman más de lo que comprueban: reaparece el patrón de T2B1/N-4

- **`WorkspaceEditIntentGuardTests.cs:13-15`.** Dice que «every family of prohibition has a detection test that injects an offending line into the real text». Es falso para estas familias, que no tienen prueba de inyección:
  - el campo o la propiedad `static` del adaptador (`:128`);
  - `\bthrow\b` (`:205`);
  - las prohibiciones del lector anidado (`:352`);
  - `PushBackPilot|RackCad.Application.Systems` y los campos de tipos del piloto en el host (`:386-389`);
  - las trampas `.Result`, `+=` y `async` del host (`:398-400`);
  - el ciclo de vida (`:650-664`);
  - las palabras prohibidas en las tres fuentes de Application (`:693-696`);
  - la interfaz (`:711`);
  - el cuerpo de `ChooseOriginDefinition` (`:723`);
  - la compuerta (`:497`);
  - la contención (`:508`).
- **Arrastre N-4, guarda de hilos.** El comentario nuevo de `WorkspaceDraftPilotIntentGuardTests.cs:39-41` dice que esta prohibición es «a SUBSET of what WorkspaceDraftBoundaryTests forbids». No lo es, por dos razones:
  - esta guarda lee el texto crudo, comentarios incluidos (`:26-27`, `Src.Text` en `:69`), mientras que la de frontera lee `CodeOnly` (`WorkspaceDraftBoundaryTests.cs:32`);
  - `System\.Threading\.Tasks` no lo detecta la expresión de frontera (`WorkspaceDraftBoundaryTests.cs:259`): `\bTask\b` y `Task\.` no casan con `using System.Threading.Tasks;`.
- **Arrastre N-4, prueba del par (9,11).** `WorkspaceDraftPilotIntentReadTests.cs:738-746` pasa a llamarse «…ForThePair9And11…». Su comentario afirma que AUTH-13 da Unreadable con un origen Selective solo, pero la prueba no lo comprueba.
  - La prueba nueva del par (9,10) (`:749-773`) sí verifica su premisa con el comparador.
  - Por tanto, el nombre y el comentario siguen apoyándose en un hecho que no se comprueba.

### 7. MENOR — Valores por defecto de los structs públicos

- **`EditCheckOutcome`.** `default(EditCheckOutcome).Differences` vale `null` (`WorkspaceEditIntent.cs:187-201`), contra su documentación: «empty unless the status is Conflict». Un consumidor de T3 que haga `.Count` sobre un valor por defecto lanza una NRE.
- **`EditIntentReadResult`.** `default(EditIntentReadResult)` vale `Status = Read` con `PilotRead = null`: el valor cero del enum es el de éxito.
  - El host lo defiende (`WorkspaceHostCore.cs:293-294` y `:369-370`) y lo prueba (`WorkspaceEditIntentHostTests.cs:420-430`).
  - Aun así, poner un estado de fallo en el cero sería más robusto para otros implementadores de la interfaz.
- **`EditIntentOutcome`.** `default(EditIntentOutcome)` es `Unavailable` / `NotEvaluated`, y está documentado y probado. Está bien.

### 8. MENOR — El esqueleto del RED no era del todo inerte (letra de INV-F4T2B2A-12)

- **Qué traía el RED.** En `db37e30d`, `WorkspaceEditIntent.cs` ya incluía los tipos de valor completos, con guardas que lanzan: `FromPilot` en `:52` y `Conflict` en `:215`. INV-12 pide un esqueleto «que no lanza».
- **Consecuencia.** Varias pruebas nuevas ya pasaban contra el RED:
  - `TheOutcomesAreValuesAndTheStatesHaveTheExactNamesAndOrderOfThePackage`;
  - `TheReadResultCarriesThePilotReadOnlyWhenItIsARead`, incluido su `Assert.Throws`;
  - `TheSignaturesOfTheTwoOperations…`;
  - `WithoutAReaderTheIntentionIsUnavailableAndReadsNothing`;
  - `TheIntentionReadsNothingByItself…`;
  - los casos «0» de `ChooseOriginDefinition` (`WorkspaceEditIntentHostTests.cs:990-1000` y `:1011-1018`), porque el esqueleto devolvía 0.
- **Alcance.** Ninguna de estas pruebas se presenta como discriminante contra el esqueleto, así que no se repite el N-1 de T2B1. Se señala solo por la letra del invariante.

### 9. MENOR — Un hueco de discriminación y un comentario inexacto

- **Orden «compuerta antes de AlreadyOpen».** En la intención, que el paso 5 vaya antes del 6 solo lo fija el texto (`WorkspaceEditIntentGuardTests.cs:487`).
  - No hay prueba de comportamiento con un borrador de A y B activo que espere `Rejected` en lugar de `AlreadyOpen`.
  - Para «Comprobar ahora» sí existe (`WorkspaceEditIntentHostTests.cs:788-803`).
- **Comentario de `Rejected`.** `EditIntentStatus.Rejected` está documentado como «no effect and no read of the drawing» (`WorkspaceEditIntent.cs:85`).
  - Pero la rama del paso 8 `SessionRetired → Rejected` (`WorkspaceHostCore.cs:316-317`) se alcanza después de la lectura.
  - Hoy no ocurre en el producto, porque nada retira sesiones hasta T2B2B.

---

## Lo que comprobé y encontré correcto

### Alcance
- Los 13 archivos del diff coinciden exactamente con el `AllowedWriteScope`.
- El GREEN no toca `tests/`.
- El RED no toca `RackSiblingScan.cs`, `WorkspaceNavigationHost.cs` ni `WorkspaceController.cs`, así que sus guardas fallan por aserción sobre el texto sin cambiar.
- `Plugin/Workspace` sigue con 9 archivos y sin subcarpetas.
- No se tocan `Workspace/Drafts/`, `EventBridgeCore.cs`, `AutoCadEventBridge.cs`, `IWorkspaceHostPort.cs` ni `WorkspaceHostTestSupport.cs`.
- No cambia ninguna guarda ni prueba existente fuera de los tres archivos de los arrastres.

### Orden y estados de D-T2B2-06 (`WorkspaceHostCore.cs:253-389`)
- **Pasos.** Se siguen los pasos 1 a 8, sin lectura antes del 6.
  - RackId en blanco → `NotAdmitted` con `NoIdentityDiagnostic`, sin tocar el registro.
  - Sin lector → `Unavailable`.
  - Sin documento activo, con modal o con comando → `Waiting`.
  - Preautorización fallida → `Rejected`.
  - `AlreadyOpen` en la intención; `NotFound` o `NotApplicable` en «Comprobar ahora», que exige Unknown con origen `CommitThrew`.
- **Mapeo.** Coincide con la tabla:
  - `NoActiveDocument` o `Busy` → `Waiting`;
  - `Rejected` → `Rejected`;
  - `Failed` o un estado desconocido → `Failed`;
  - `Begin` → `Admitted`, `AlreadyOpen`, `NotAdmitted` o `Rejected`;
  - `CheckNow` → `Conflict` o `Recovered`.
- **Testigo.**
  - `NotEvaluated` en todos los resultados sin lectura.
  - El estado del testigo se traduce desde el piloto, tanto en `Admitted` como en `NotAdmitted`.
  - `WitnessStatusOf` nunca da `Unchanged` por defecto.

### Contención (D-17)
- `catch (Exception)` → `Failed(ex.Message)`, registrado con `_logFault` y sin pasar por `_faulted` ni por `Contain`.
- Un log que lanza no se convierte en un segundo fallo.
- `request == null` se contiene.
- Las pruebas verifican que el drenaje sigue leyendo la selección tras el fallo.

### INV-22
- **En el core:** preautorización en memoria con el id nativo del activo (`:427`).
- **En el Plugin:** reautorización dentro del bloqueo con `document.UnmanagedObject.ToInt64()` (`WorkspaceNavigationHost.cs:234`), igual que el precedente de navegación.
- **Pruebas:** A con B activo, el id nativo reciclado y la sesión inexistente, todos con cero lecturas y sin borrador; también para «Comprobar ahora».

### Costura y cableado
- `SetEditIntentReader` se llama en el getter `Navigation`, justo después de `new WorkspaceNavigationHost(`.
- No cambian el constructor del controlador ni los argumentos del host.
- El campo se llama `_editIntentReader`, no `_reader`.
- `WorkspaceHostCore` no nombra `NavigationTargets`.
- Hay un único `new NavigationTargets(` y un único `new WorkspaceNavigationFacts(`; los objetivos son un campo del host que la clase anidada reutiliza.

### Lector anidado (`WorkspaceNavigationHost.cs:220-253`)
- **Orden:**
  1. documento activo;
  2. comando y reposo;
  3. `LoadCatalog`, fuera del bloqueo;
  4. `using LockDocument`;
  5. autorización;
  6. `Revalidate` y `ChooseOriginDefinition` sobre `Fresh`;
  7. adaptador.
- **Trampas respetadas:** sin `.Result`, sin `+=` ni `-=`, un solo `RackLog.Exception`, y ningún nombre de tipo anidado con Draft, Editor, Tab o Stable.

### Origen (D-11 y D-T2B2-04)
- **Reglas:**
  - referencia que sigue en los objetivos → su definición;
  - referencia ausente o ahora de otro rack → 0 → clave vacía → `OriginMissingDiagnostic` («ya no está»);
  - sin referencia → `Primary` en el orden determinista;
  - sin objetivos → 0.
- **Formato de clave.** La del adaptador (`new Handle(long).ToString()`) es igual que la del barrido (`definitionId.Handle.ToString()`). `DefinitionHandle` sale de `Handle.Value` (`WorkspaceNavigationFacts.cs:79`).

### Lectura única
- Una sola `ReadIntent` por operación.
- Una sola `CaptureDrawing(` en el adaptador.
- `RawEnvelopes` es el mismo `json` que `Traverse` lee con `RackBlockData.Read`: incluye los sobres no interpretables, usa el orden ordinal y es de solo lectura (`ReadOnlyDictionary`).

### Sin escritura (R6)
- El adaptador no contiene `ForWrite`, `UpgradeOpen`, `Commit`, `Erase`, `RackBlockData.Write`, selección, vista, `GetObject` ni `ProjectVariables`.
- `ReadPreviewWitness` usa su propio `using` de bloqueo y de transacción, sin `Commit`, y resuelve la definición con `ToHandle`.

### Registro
- Un único `DraftRegistry` y su `DraftClosePolicy` en el core.
- `Begin` usa `PushBackPilotStateComparer.Instance` y `.Copy`; las pruebas discriminan un comparador o un copiador equivocados (`IsDirty` y `NotSame`).
- Ningún campo del core es de tipos del piloto ni de `EditIntentReadResult`, comprobado por reflexión.

### Arrastres
- **N-1:** la prueba usa literales y discrimina contra el esqueleto real de T2B1 (`04717913`, donde todo era «provisional»).
- **N-2:** el par (9,10) se construye con un Kind «PushBack» con otras mayúsculas y se verifica la premisa con el comparador.
- **N-3:** `Detail` es null con un Kind vacío o en blanco. La prueba cubre `""`, `" "` y `"\t"` y falla contra el código anterior.
- **N-4:** se corrigen el nombre y los comentarios señalados, con los residuos del hallazgo 6.
- No se elimina ninguna aserción de T2B1.

### Trampas de D-T2B2-08
- No aparecen `Transaction`, `Database`, `Commit(`, `Provider`, `Metric` ni `Aggregat` en las tres fuentes nuevas de Application.
- Los tipos nuevos del namespace no contienen Draft, Editor, Subscription ni Tab, incluido el anidado `EditGate`.
- No hay colisiones de nombres entre `RackCad.Plugin.Views` y los tipos de Workspace con el nuevo `using`.

---

## Riesgos para el smoke con AutoCAD (no son defectos)

### Bloqueos y transacciones
1. **Bloqueos anidados en tres niveles.**
   - Son el de la operación, el de `WorkspaceNavigationFacts.Scan` y el de `RackSiblingScan.Traverse`.
   - Hay que comprobar que no aparecen `LockViolation` ni cuelgues.
   - Un `LockViolation` del bloqueo de la operación ocurre en el lector anidado, fuera del adaptador. Acaba en `Failed`, no en `Waiting`, por el `catch` general.
2. **R6: DBMOD y UNDO.**
   - `Traverse` hace `Commit` de su transacción de lectura bajo un bloqueo Write, que es el modo por defecto de `LockDocument()`.
   - Hay que verificar que DBMOD y la pila de UNDO no cambian tras una intención o un «Comprobar ahora».
3. **`Debug.Assert` en Debug.** Si falla, abre un diálogo modal dentro del bloqueo del documento.

### Coste y dependencias
4. **Coste de cada intención y de cada «Comprobar ahora».** Cada una recorre dos veces la BlockTable completa:
   - `ReadDefinitionFacts`;
   - `CaptureDrawing`, con `GetBlockReferenceIds(forceValidity: true)`, que `RackBlockFinder` evita adrede por caro en dibujos grandes, y con la apertura de las capas de cada entidad.

   Hay que medirlo en Smoke-2.
5. **«Comprobar ahora» depende del catálogo.**
   - Si `LoadCatalog` falla, el resultado es `Failed`, aunque `CheckNow` solo necesita la base.
   - Lo impone el orden contratado de la interfaz, pero puede impedir recuperar un borrador Desconocido.

### Diagnóstico
6. **Los fallos del adaptador no quedan en RackLog.** Solo llega `ex.Message` en `Reason`, así que el smoke pierde la traza.

### Para T3
7. **`ReadPreviewWitness`.**
   - No comprueba reposo, comando, modal ni sesión, y tampoco `TopTransaction`. T3 tiene que poner la compuerta y la autorización.
   - Con una definición borrada pero presente en la tabla de handles, `TryGetObjectId` seguido de `GetObject` lanza `eWasErased`. El resultado es un fallo tipado en vez de un testigo con texto nulo.
8. **Sin defensa en ejecución frente al drenaje.**
   - El core no impide que lo llamen desde el drenaje. No hay un indicador de «dentro del drenaje» como el `_drainNoticeOpen` de los resúmenes.
   - Un manejador de `StateChanged`, que corre dentro de `OnDrained` y de la pila del Idle (H-13), podría disparar la lectura dentro del drenaje.

### Origen y navegación
9. **Origen limitado por la navegación.**
   - Solo cuentan las referencias directas de Model Space (`WorkspaceNavigationFacts.cs:71`). Un rack con referencias solo en layouts o anidadas da `OriginMissing` («ya no está en el dibujo»), un mensaje engañoso.
   - La navegación salta las xref (`RackBlockFinder.cs:66`), así que la regla 6 (`OriginReadOnly`) es inalcanzable desde el host.
10. **Caché de navegación.** Cada intención llama a `Revalidate`, que sustituye o expulsa la entrada de caché de `NavigationTargets` del rack. Es inocuo, pero cambia el estado de navegación como efecto lateral de una acción de edición.

---

**Línea final:** NO ACEPTABLE — 0 BLOQUEANTE, 1 IMPORTANTE, 8 MENOR.
