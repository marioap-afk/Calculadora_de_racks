# I-64 F4-T4C2-PANEL: revisión adversarial independiente del código

**Objeto:** todo el cambio de T4C2, `git diff 1cce48c0 b88dabf254645e83d71ec42d4fd621d19c9b87b7`, en la rama `architecture/workspace-persistente-rackcad`:
- **intento 1:** BaseSha `1cce48c0` y RED `a6511c04` (esqueleto inerte y pruebas, sin GREEN);
- **intento 2:** BaseSha `a912f645`, RED `f1f1c7ee` (las tres pruebas de INV-F4T4C-26) y GREEN `b88dabf2` (el producto del parche custodiado);
- entre `a6511c04` y `a912f645`, dos commits del Coordinator (`bbc259dd`, `a912f645`) que solo tocan `docs/`.

**Método:**
- solo lectura: `git -C <worktree>` (`show`, `diff`, `grep`, `log`, `patch-id`) y lectura de archivos;
- ningún `dotnet`, ninguna prueba ejecutada, ninguna escritura en el worktree;
- los TRX de las carpetas de corrida se leyeron con un analizador XML propio;
- toda evidencia `ruta:línea` es de `b88dabf2`, salvo que se diga otra cosa.

**Autoridades leídas en el HEAD:**
- `docs/automation/evidence/I-64-pilot/F4-T4C2-PANEL/coordinator/gate-contract-attempt2.json`;
- `docs/initiatives/I-64-f4-t4c-gate-contract-package.md` v5, parte [2];
- `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md` (H-19, H-21 y D-T4A2-06);
- `docs/automation/evidence/I-64-pilot/F4-T4C2-PANEL/1/R20261115T190629Z-9b55/analysis.md`;
- las dos entregas `artifacts/orchestration/I-64/F4-T4C2-PANEL/{1,2}/…/worker-handoff.json`.

No se leyó la revisión directa de la sesión.

---

## 1. Veredicto

**ACEPTABLE CON MENORES.**

| Bloqueantes | Importantes | Menores | OWNER |
|---|---|---|---|
| 0 | 0 | 5 (M-01 a M-05) | ninguno |

**Lo que sostiene el veredicto:**
- **El alcance:** el Worker tocó exactamente las 14 rutas del `AllowedWriteScope` y ninguna prohibida. El GREEN no toca `tests/` y el RED de la corrección solo toca las dos rutas de INV-F4T4C-26.
- **El GREEN es idéntico al parche custodiado:**
  - `git patch-id --stable` da `085ca4bb…` en los dos;
  - el SHA-256 del blob es `551c31c0…155166f`, el de INV-F4T4C-26.
- **La frontera de INV-F4T4C-23 se respeta.**
- **El gesto y la presentación** cumplen D-T4C-11 y las cinco reglas de D-T4C-03.
- **El diagnóstico** cumple D-T4C-12.
- **Los cambios de guarda** son los de las filas 12, 18 y 21, más D-T4A2-06, con sus negativas. Ninguna fila [1] cambia.
- **No encontré** ninguna petición doble, ningún resultado perdido o presentado dos veces, ninguna reclasificación, ningún falso Aplicado ni ningún falso limpio.

**Los cinco menores no bloquean el cierre:**
- un indicador «Aplicando» que puede quedar obsoleto tras el Error del campo;
- un punto ciego en la guarda de la fila 18;
- un comentario obsoleto;
- cómo calcular el alcance del rango;
- un desfase documental en el paquete de T4A2.

---

## 2. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **M-01** | Menor | **«Aplicando» y el botón oculto pueden quedar obsoletos tras el Error del campo.** `_inFlight` es una copia que solo refrescan `PresentProgress` y el cambio de superficie. Con el campo en Error, `Show` no recoge (decisión conservadora) y «Recargar» (`Rematerialize`) solo repinta. Si el Error llegó con la actualización en vuelo, tras «Recargar» el estado dice «Aplicando» y el botón sigue oculto hasta el siguiente `Show`, aunque la corrida ya terminara. La regla 2 (paquete `:540-542`) dice que, con los dos indicadores falsos, el campo se pinta desde su estado. No hay ni petición doble ni falso Aplicado de un resultado: un clic en esa ventana recibiría `Busy`. Requiere un fallo del puerto durante el vuelo, así que es poco alcanzable | `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:307` (sin recogida con `_failure`), `:442-456` (`Rematerialize`, solo `Render`), `:1126` (`StateOf` usa `_inFlight`), `:918` (`OffersUpdate` exige `!_inFlight`), `:928` y `:664` (los dos únicos puntos de escritura), `:1156-1168` (`EnterFailure` no lo toca) | Poner `_inFlight = false` en `EnterFailure`: el siguiente `Show` de la superficie fijada lo vuelve a sincronizar. No añade ninguna recogida, así que la fila 18 no cambia. Hace falta una prueba, y por 16.8 no cabe en este intento: declararlo como limitación o llevarlo a una tarea posterior |
| **M-02** | Menor | **Punto ciego en la guarda de la fila 18.** «La presentación del campo puede llamar a `TakeUpdateProgress` y a ninguna lectura» se comprueba sobre `NonGestureMembers`, cuyo patrón solo casa nombres exactos (`Present\s*\(`, `Show\s*\(`…). Por eso no examina los cuerpos de `PresentProgress`, `PresentStart` y `PresentStopped`, que son la presentación real del resultado. Las negativas solo inyectan en `Show`. `TryBeginEdit` o `TryReload` dentro de `PresentProgress` no los vería ninguna guarda de fuente: la regla 1 solo fija las cuatro lecturas de la reconciliación, y la de T3B1 solo mira fuera del VM del campo. Lo mitigan pruebas de comportamiento: U32 y U06 exigen `Reads()` vacío, y `Reads()` incluye esas dos | `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:64-66`, `:151-177`, `:186-200`; `WorkspacePilotFieldViewModel.cs:924`, `:937`, `:947`; `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:97-99` | Ampliar el patrón a `Present\w*` en la regla de la fila 18, con una negativa en `PresentProgress`. Es la misma fila [2]. No se puede hacer en este intento (16.8): anotarlo como deuda de la guarda |
| **M-03** | Menor | **Comentario obsoleto.** La cabecera de `PresentCheck` dice que un Recovered o un Conflict «retire the diagnostic». Ya no es así: ahora lo conserva si la respuesta trae diagnóstico. El comentario interior sí es correcto | `WorkspacePilotFieldViewModel.cs:846` frente a `:849-855` | Corregir el comentario cuando se vuelva a tocar el archivo |
| **M-04** | Menor (proceso) | **El alcance del rango no es el alcance del Worker.** `git diff --name-only 1cce48c0 b88dabf2` da 100 rutas: las 14 del permitido y 86 bajo `docs/`, que está en el `ForbiddenWriteScope`. Esas 86 son de los commits de custodia del Coordinator `bbc259dd` y `a912f645` (trailer Opus), no del Worker. La comparación literal que pide el encargo falla, y la de los commits del Worker es exacta (§3). Por PD-F4-07, un oráculo nc2 calculado sobre el rango daría un rojo falso, y uno que filtrara `docs/` sin decirlo sería un verde no acreditado | §3 | En la verificación, calcular el alcance por tramos, `1cce48c0..a6511c04`, `a912f645..f1f1c7ee` y `f1f1c7ee..b88dabf2`, y declarar que el tramo `a6511c04..a912f645` es del Coordinator y solo toca `docs/` |
| **M-05** | Menor (documental, del Coordinator) | **Desfase del paquete de F4-T4A2.** La tabla H-19 y H-21 quedan desactualizadas tras D-T4C-12. H-19 (`A2P:135-145`) dice cero en la columna Comprobación para `pilot.Read` y `pilot.ClassicEditorHint`; el cuerpo tiene ahora 2 y 1. H-21 dice que `EditCheckOutcome` no lleva diagnóstico. No hay ninguna guarda que fije esas cuentas: `PilotForms` no fija cuentas, y la de `pilot.Read` en el cuerpo de la comprobación va por la fila 12. El Worker no podía tocarlo, porque `docs/` está prohibido | `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md:135-147`; `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:432-433`; `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:832-847` | Que el Coordinator lo anote en el cierre de T4C2, o en una enmienda del paquete de T4A2, como consecuencia declarada de D-T4C-12 |

**Hallazgos OWNER: ninguno** (ver §10).

---

## 3. El alcance (PD-F4-07): las listas comparadas

Las listas se calcularon de nuevo con `git diff --name-only` y se compararon, por programa, con el `AllowedWriteScope` (14) y el `ForbiddenWriteScope` (150) del contrato, sin fiarse de ningún Scope en verde.

| Comprobación | Resultado |
|---|---|
| `1cce48c0..b88dabf2`, todo el rango | 100 rutas: las 14 del permitido y 86 bajo `docs/`, todas de `bbc259dd` y `a912f645` (M-04) |
| `a6511c04..a912f645`, fuera de `docs/` | **ninguna** ruta: «solo documentación» se confirma |
| La unión de los tramos del Worker | **exactamente las 14** del permitido, sin que sobre ni falte ninguna |
| Rutas del Worker bajo el prohibido, por igualdad o por prefijo de raíz | **ninguna** |
| Rutas del permitido que estén también en el prohibido | **ninguna** |
| GREEN `f1f1c7ee..b88dabf2`, bajo `tests/` | **ninguna**. Solo `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs` y `WorkspacePilotFieldViewModel.cs`, las tres del parche |
| RED de la corrección `a912f645..f1f1c7ee` | solo `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs` y `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs`, las dos rutas de INV-F4T4C-26 |
| `a6511c04..f1f1c7ee -- src/` | vacío: el producto del RED de la corrección sigue siendo el esqueleto inerte |
| RT = `1cce48c0..f1f1c7ee -- tests/` | 9 rutas, todas del permitido |
| Las rutas de T1B (INV-F4T4C-16) | ninguna tocada: `src/RackCad.UI/Systems/`, `src/RackCad.Plugin/`, `src/RackCad.Application/Systems/`, las fachadas y los `*Commands*.cs` |
| El trailer de los commits del Worker | `Co-Authored-By: Claude Sonnet 5.5` en `a6511c04`, `f1f1c7ee` y `b88dabf2` |

---

## 4. La frontera de INV-F4T4C-23

- **`WorkspaceHostCore.cs`:** un solo bloque, dentro del cuerpo de `TryCheckPilotEdit` (`:382-461`). Añade el `try` del diagnóstico (`:426-440`) y los argumentos de `Recovered` y `Conflict` (`:448`, `:451`).
- **Lo que no cambia en el núcleo:** `RequestUpdate` (`:1376-1435`), `TakeUpdateProgress` (`:1438-1455`), `ExpireStaleUpdate` (`:1465-1484`), la corrida anidada (`PilotUpdateRun`, `:1633-` y siguientes), `UpdateGate`, `TryUpdatePilot` (`:844`) y `CrossBoundary`.
- **`WorkspaceEditIntent.cs`:** solo cambia `EditCheckOutcome` (`:238-298`): el constructor privado, las dos propiedades (`:266-273`), las fábricas `Conflict` y `Recovered` con parámetros opcionales (`:285-292`) y `Without`.
- **Lo que no cambia en el tipo:** `EditCheckStatus` (`:207-232`) y los tipos `EditUpdate*` (`:399-607`).
- **El prohibido:** `IWorkspacePilotEditPort.cs`, `EventBridgeCore.cs` y `DraftSession.cs` no se tocan.

**Conforme.**

---

## 5. La UI frente a D-T4C-11 y D-T4C-03

### 5.1 El botón

- **XAML:** `src/RackCad.UI/Workspace/WorkspacePanelView.xaml:239-243`, con `x:Name`, `Content="Actualizar"`, `AutomationProperties.Name` y `Visibility` ligada a `ShowUpdateButton`.
- **El manejador:** `WorkspacePanelView.xaml.cs:93`, `Contained(() => _viewModel.Browser.Field.Update())`.
- **La oferta:** `OffersUpdate` (`WorkspacePilotFieldViewModel.cs:917-918`), que exige:
  - superficie fijada sin `_failure` y con borrador (`OnPinnedSurface`, `:724-725`);
  - ninguna elección pendiente;
  - `!_inFlight`;
  - `State == Dirty`.

  De ahí sale todo lo que pide el paquete:
  - Dirty, también posiblemente obsoleto: sí;
  - con un diagnóstico persistente: sí, porque no se excluye;
  - Clean, Conflict, Unknown, Error, Applying y la vista previa: nunca;
  - un Dirty con `CanEdit` falso solo ocurre con un borrador no registrado (`WorkspaceHostCore.cs:1767-1768`), así que `Kept` solo sale de una carrera, como dice el paquete.

**Conforme.**

### 5.2 El gesto (`:518-577`), paso a paso

| Paso del paquete | Implementación | ¿Conforme? |
|---|---|---|
| 1a. Comprueba su botón | `if (!ShowUpdateButton) return;` (`:520`) | Sí |
| 1b. Recoge lo pendiente; si hay resultado, lo presenta y no pide | `:529-535`. Con un resultado: `PresentProgress`, `_changed` y `return`. Sin resultado sigue | Sí (regla 5) |
| 2. La frontera Update | `:538` con `PilotBoundaryKind.Update`. `IsMine` (`:539-543`) y solo `Proceeded` sigue (`:545-550`). `Aborted` da el mensaje del campo o el motivo; `Busy`, el motivo; `Kept` y cualquier otro valor, un texto fijo | Sí (H-27) |
| 3. `RequestUpdate` una sola vez | `:554`. Es la única aparición en la UI, y la fijan la fila 21 y G-10 | Sí |
| 4. Tras `Accepted`, `TakeUpdateProgress` | `:570`, después de `IsMine` (`:555-559`). Presenta el resultado o «Aplicando». Con `InFlight` falso y sin resultado no toca el aviso | Sí (regla 2) |
| 5. Un inicio no `Accepted`, con su causa | `:562-567` y `PresentStart` (`:937-943`), que da prefijo, causa y « No se actualizó.». Cubre también `Failed(HostNotRunReason)` y un estado que ningún enumerador nombra | Sí (regla 4) |

### 5.3 La presentación

- **`UpdateNoticeOf` (`:992-1033`)** va por la clase del resultado, sin reclasificar:
  - Applied: «Actualizado…»;
  - AppliedWithWarnings: los avisos visibles;
  - NotAppliedConflict: las diferencias;
  - NotApplied\*: su causa;
  - CommitThrew: A2-11 `UnknownCheckText` (`:57`);
  - AppliedPostRereadFailed: A2-12 `UnknownAppliedText` (`:58`);
  - ClosedInError: la clasificación (M-03; `:1035-1059`), con el prefijo «El borrador quedó en error…», nunca un éxito;
  - los demás: su causa;
  - un valor desconocido: un texto neutro.
- **Unknown** (CommitThrew o AppliedPostRereadFailed) nunca da «Fallo» ni «Actualizado». Lo fija U20 (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1940`).

### 5.4 Las cinco reglas de F-1

| Regla | Implementación | Prueba | ¿Conforme? |
|---|---|---|---|
| 1. Una vez | La recogida consume en el núcleo (`WorkspaceHostCore.cs:1447`) y en el doble; un repintado sin resultado no toca el aviso (`PresentProgress`, `:929`) | U17 y U22 | Sí |
| 2. El repintado anidado | `PresentProgress(…, true)` sin resultado no cambia `_noticeText`; «Aplicando» solo sale de `_inFlight` (`:1126`) o de Applying (`NameOf`, `:1147`) | U21 y U23 | Sí (salvo M-01) |
| 3. Por superficie, para su rack y tras `IsMine` | `Show` recoge con `Origin()` y `_rack` de la superficie viva (`:311`); el gesto, con los valores capturados y tras `IsMine`. Si la superficie cambió: `PresentSurfaceChange` y el resultado sigue en el almacén | U25, U27 y U28 | Sí, con la concreción de §10 (a) |
| 4. Un inicio que no es `Accepted` | Se presenta y no se recoge después | U15 y U16 | Sí |
| 5. Una petición no borra lo no presentado | El paso 1 | U10 y U29 | Sí |

### 5.5 K = 1, los avisos, los textos y ningún mecanismo nuevo

- **K = 1:** `PresentProgress`, `PresentStart` y `PresentStopped` repintan desde `GetField` (`Render`, `:692-712`). Conforme.
- **Los avisos:** se guardan en `_noticeText` como aviso del gesto. `PilotFieldView` no cambia. Conforme.
- **Los textos nuevos:** 35 constantes `private const` (`:78-116`) y ningún literal nuevo en los cuerpos. Su prueba de tabla son U15 (9 casos de inicio), U17 (22 resultados) y U18 (9 clasificaciones de ClosedInError), con cada texto completo. Conforme.
- **Ningún mecanismo nuevo:** en las líneas añadidas de `src/` no hay `Task`, `async`, `await`, `Dispatcher`, `Timer`, `event`, `+=`, `-=`, `delegate`, `.Result`, `.Wait(`, `GetAwaiter` ni `Thread`. La lista de avisos se compone con `text = text + …`. Conforme.

---

## 6. El diagnóstico de T4A2 (D-T4C-12)

- **El tipo:** dos propiedades opcionales en `EditCheckOutcome` (`WorkspaceEditIntent.cs:266-273`) y ningún valor nuevo de `EditCheckStatus` (`:207-232`). Lo fija `TheAnswerOfCheckNowOnlyCarriesAStateTextAndListsOfTextAndTheStatusesAreStillTheEightOfBefore`.
- **El origen:** la misma lectura, la única llamada a `ReadIntent` (`WorkspaceHostCore.cs:409`). El diagnóstico se calcula **antes** de `CheckNow` (`:428-440` frente a `:443`), en su propio `try`, y su `catch` da `null` a los dos valores y solo registra (`Report`, `:1965-1975`, que no degrada la sesión).
- **La clasificación no cambia:** `Recovered` y `Conflict` siguen saliendo solo de `draft.CheckNow(pilot.StaleBase)` (`:443-451`). Como el diagnóstico va antes de `CheckNow`, su fallo no puede convertir en `Failed` una respuesta que ya cambió el borrador.
- **La interfaz:** `PresentCheck` (`WorkspacePilotFieldViewModel.cs:847-864`) guarda el diagnóstico de la respuesta como persistente y retira solo si no lo trae. Las respuestas que no releen dejan el persistente tal cual. Es la regla nueva de D-T4A2-06. El texto del aviso no cambia (`CheckNoticeOf`, `:866-877`).
- **Ninguna lectura nueva:** `Assert.Single(rig.Reader.Calls)` en las pruebas de núcleo, y `Show` solo lee memoria.

**Las pruebas de núcleo discriminan** (`tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs:559-771`):
- no admitido y misma base: Recovered con el diagnóstico;
- no admitido y otra base: Conflict con el diagnóstico;
- admitido: sin diagnóstico;
- la misma clasificación con y sin admisión;
- un fallo del diagnóstico, provocado con un `pilot.Read` nulo: `null`, sin cambio de la clasificación ni `Failed` (`:683`);
- una sola lectura.

**Conforme.**

---

## 7. El doble de UI (INV-F4T4C-25)

- **El registro propio:** `UpdateLog` (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:64`). Lo alimentan `RequestUpdate` (`:378-385`), `TakeUpdateProgress` (`:387-399`) y también `CrossBoundary` (`:309`), para ver el orden.
- **Lo que no cambia:**
  - `Calls` (`:61`): `CrossBoundary` ya estaba en él;
  - `Operations()`;
  - `Reads()` (`:97-99`).
- **La única variación de comportamiento:** `CrossBoundary` trata `Update` como frontera bloqueante (`:314`), como hace el núcleo (`WorkspaceHostCore.cs:1193`, `:1216`, H-27). No es un cambio de guarda ni toca los conjuntos exactos.
- **U33** lo fija.

**Conforme.**

---

## 8. Los cambios de guarda frente a D-T4C-20

| Fila o regla | Guarda que declara el paquete | Cambio aplicado | Negativa | ¿Conforme? |
|---|---|---|---|---|
| **12** [2] | `WorkspaceEditIntentGuardTests.cs:682-685`, `:821-870` (M-2 y M-3) y `:699-700` | `:681-689`: `CheckBody()` con una expresión, dos `pilot.Read` dentro de ella, ninguno fuera y ningún `.Begin(`. `:704-707`: `CheckBody()` sale del bucle de cero y pasa a uno; Intent, Keep y DiscardAndReload siguen en cero. M-3 (`:871-880`): dos en el núcleo, una en la confirmación y una en la comprobación. El patrón M-2 (`BaseOrReadUse`, `:647`) no cambia | `:883-895`: una tercera en el núcleo, una segunda en `CheckBody()`, una en `KeepBody()` y la forma sin el patrón | Sí. Las negativas comprueban el conteo sobre texto concatenado, en el mismo estilo que la negativa de M-3 que ya existía |
| **18** [2] | `WorkspacePilotSurfaceGuardTests.cs:54-92` | Una regla aditiva en el mismo archivo (`:143-200`). Ninguna de las seis lecturas ni `RequestUpdate` en los miembros de presentación de la carpeta. `TakeUpdateProgress` solo en el VM del campo: exactamente una vez en `Show` y dos en `Update`. `:54-92` no cambia | `:186-200`: siete lecturas en `Show`, una segunda recogida en `Show`, una tercera en `Update`, otra en otro miembro u otro archivo, y ninguna recogida | Sí, con el punto ciego M-02 |
| **21** [2] | Las reglas de reconciliación (`:62-101`, `:155-195` y desde `:256`) e `IntentionReads` (`WorkspacePilotCloseSyncGuardTests.cs:844-867`) | Regla 2b nueva (`WorkspacePilotReconciliationGuardTests.cs:228-342`): `Update()` declarado una vez y llamado solo por su manejador exacto con `Contained`, y `RequestUpdate` solo en el VM del campo, una vez, como `_port.RequestUpdate(request)` dentro de `Update` y en el orden botón, pendiente, frontera, petición y recogida. Regla 3: la tabla pasa de cinco a seis botones (`:370-375`, `:394`, `:465`). `IntentionReads` gana `RequestUpdate` (`WorkspacePilotCloseSyncGuardTests.cs:850`, con su caso en `:874`). Las reglas 1 y 2 no cambian | `:313-325`: segundo llamador en el campo y en el navegador, manejador sin `Contained`, intercambio y llamada doble. `:328-340`: segunda llamada, navegador, grupo de método y orden roto, con dos intercambios. `:496-508`: `Click`, `Content`, nombre de automatización, enlace, `IsDefault` y nombre duplicado | Sí |
| **H-19** (A2P) | La tabla de formas de `pilot` | Sin cambio de código: `PilotForms` (`WorkspaceEditIntentGuardTests.cs:831-847`) ya admite las formas y no fija cuentas. La de `pilot.Read` en la comprobación va por la fila 12 | `:852-862`, sin cambios | Sí. La tabla documental queda desfasada (M-05) |
| **D-T4A2-06**, regla de UI | «Retira ante Recovered o Conflict» pasa a «retira salvo que la respuesta traiga diagnóstico» | `PresentCheck`, `WorkspacePilotFieldViewModel.cs:851-855` | Pruebas UI de `WorkspacePilotReconciliationTests.cs` (+131 líneas): conserva con diagnóstico, retira sin él, reemplaza y deja el persistente ante Waiting | Sí |

**Las filas [1] en archivos permitidos no cambian:**
- la fila 10 (`WorkspaceEditIntentGuardTests.cs:598-641`) y la fila 11 (`:1101-1111`): el diff de ese archivo solo tiene bloques en `:653`, `:681`, `:701` y `:862-895`;
- la fila 20 (los dobles): el cambio del doble lo autoriza INV-F4T4C-25 y no es una guarda.

**Lo que cambia en `tests/` y no es una guarda:**
- las pruebas nuevas de núcleo y de UI;
- el ayudante `UnknownCheck` de `WorkspacePilotReconciliationTests.cs:124-125`, por la aridad nueva del constructor privado.

**Ninguna otra guarda cambia.** No hay parada C07.

---

## 9. El RED

- **El producto de `f1f1c7ee`** es el esqueleto inerte de `a6511c04`; el diff de `src/` entre los dos está vacío. El esqueleto (`1cce48c0..a6511c04 -- src/`):
  - `ShowUpdateButton => false`;
  - `Update() {}`;
  - las fábricas de `EditCheckOutcome` ignoran sus argumentos;
  - `Show` y `PresentCheck` siguen como en la base;
  - el núcleo no se toca.
- **Los TRX del RED de la corrección** (`artifacts/…/2/R20261115T195540Z-468f/red-ci-core/core.trx` y `red-ci-ui/ui.trx`):
  - **Core:** 19018 pruebas, 10 fallan, todas nuevas o cambiadas por la fila 12;
  - **UI:** 2403 pruebas, 75 fallan y 17 no se ejecutan; las 75 son nuevas (U01 a U34, cuatro de vista y cuatro de reconciliación);
  - las 17 no ejecutadas son las mismas omitidas, con motivo «Evidencia de la BASE…», que en el GREEN;
  - todos los mensajes son `Assert.*`, ninguno es una excepción del esqueleto.
- **Ninguna prueba existente que no cambiara falla.**
- **Los TRX del GREEN** (`ci-core`, `ci-ui`): Core 19018 de 19018; UI 2386 pasan y 17 no se ejecutan, las mismas.

**Las tres pruebas corregidas discriminan:**
1. **La vista previa:** la parte de la vista previa vería quitar `!_isPreview` de la oferta, porque el botón se vería. La parte fijada falla en el RED y pasa en el GREEN. Observación: usa un Dirty no protegido, que el núcleo real no produce (O-1).
2. **El orden:** fija el orden de las cuatro primeras entradas y solo admite recogidas después. `Assert.Single` sobre `RequestUpdate` ve una segunda petición.
3. **`Assert.Empty`:** equivale a la aserción anterior y quita xUnit2013. Pasa también en el RED, por diseño: es la invariante de ninguna lectura nueva.

---

## 10. Las dos decisiones conservadoras de la entrega

**(a) Con la superficie viva en vista previa o con el campo en Error, el resultado no se recoge y queda en el almacén** (`WorkspacePilotFieldViewModel.cs:304-307`; U26).

**Veredicto: fiel. Es una concreción conservadora, no una decisión material nueva. No es OWNER.**

- **La parte del Error** sigue el patrón que ya tiene el campo: «the answer is not presented against the old view» (`PresentConfirm :817-818`, `PresentCheck :857-859`, `PresentChoice :905-907`). En Error, la única acción es «Recargar».
- **La parte de la vista previa** restringe la letra de la regla 3 y de D-T4C-02 (paquete `:449`, `:543-545`: «la superficie viva»), porque una vista previa también es la superficie viva (`WorkspaceBrowserViewModel.cs:1192`, `:807`). Pero no es alcanzable para el rack que pidió la actualización:
  - el botón solo se ofrece en una pestaña fijada con un Dirty;
  - un Dirty siempre está protegido (`WorkspaceHostCore.cs:1759`, `isProtected = draft.IsDirty`; `DraftSession.cs:298-300`);
  - el navegador fija toda pestaña protegida (`WorkspaceBrowserViewModel.cs:1295-1302`) y ningún camino la desfija;
  - el único camino es cerrar la pestaña y volver a abrirla como vista previa, y el descarte durante el vuelo responde `Busy` (D-T4C-08, `CloseTab :257-294`).
- **Ninguna de las dos partes** pierde, duplica ni reclasifica un resultado. Sigue en el almacén, que es «sigue guardado» de la regla 3.
- **No toca** la semántica de Actualizar, la autoridad de escritura, Undo, conflicto ni obsoleto (C16).
- **Debe seguir declarada como limitación**, y lo está en `KnownLimitations` de la entrega.
- **Su efecto lateral es M-01.**

**(b) Los avisos se pierden si la superficie cambia.**

**Veredicto: fiel.** D-T4C-11 lo declara literalmente (paquete `:982`, «si la superficie cambia después de presentar el aviso, el aviso se pierde», con la alternativa O-14). La implementación los guarda en `_noticeText` como aviso del gesto, y lo vacían el cambio de superficie (`:657`) y un gesto nuevo (`:671`).

---

## 11. Observaciones sin recuento

- **O-1.** La prueba de vista corregida (1) usa un Dirty no protegido que el núcleo real no produce (§10 (a)). Discrimina igual, y U02 cubre la vista previa en el VM. En producción, la exclusión de la vista previa la garantiza también el anclado.
- **O-2.** El nombre de U09 habla de un repintado anidado que la prueba no simula. Lo simula U21.
- **O-3.** Un inicio que no es `Accepted`, con cambio de superficie durante `RequestUpdate`: su causa no se muestra en ningún sitio (`PresentSurfaceChange`, `:555-559`). Es inherente a `IsMine` y no hubo ninguna escritura. Ninguna prueba lo cubre.
- **O-4.** Cuando el paso 1 no trae resultado, el gesto ignora `pending.InFlight` (`:529-535`). Es prácticamente inalcanzable, porque `Show` sincroniza `_inFlight`, y `RequestUpdate` respondería `Busy`.
- **O-5.** Literales repetidos que no rompen ninguna guarda:
  - «Aplicando» está en `ApplyingText` (`:80`) y en `NameOf` (`:1147`);
  - varios prefijos nuevos repiten literales de `Describe` y `CheckPrefixOf`;
  - CommitThrew muestra A2-11 en el aviso y en la línea de reconciliación a la vez.

---

## 12. Lo no verificado

- **No ejecuté** ninguna prueba ni ningún build. Los conteos salen de los TRX de las carpetas de corrida y de las entregas.
- **El origen de los TRX:**
  - están bajo `artifacts/`, que `.gitignore` excluye;
  - no llevan el SHA dentro: solo las horas, 20:07 UTC el RED y 20:27 UTC el GREEN;
  - la `relay-record.json` del intento 2 no nombra las corridas de CI;
  - no pude acreditar que salgan de la CI push de los SHA exactos `f1f1c7ee` y `b88dabf2`, ni que los cuatro jobs estén en success.
- **Los avisos del build Release:** que no haya ninguno nuevo y que xUnit2013 desaparezca solo lo dice la entrega (`tests/b0`, `tests/b1`). No lo comprobé.
- **La contención de F4-E59** en tiempo de ejecución (ningún modal ni estado persistente vivo): solo leí el código de las pruebas, que no escriben en disco y usan `StaTestRunner`.
- **El comportamiento real en AutoCAD:** que el repintado de `EnteringQuiescentState` llegue siempre (H-43), y los hechos de H0, los tomé del paquete. La primera validación física es Smoke-2.
- **Si `Src.Code` quita los comentarios:** no lo comprobé. Afecta a la robustez de las guardas de conteo frente a menciones en comentarios, no a su resultado actual, que es verde según los TRX.
- **Las 17 pruebas omitidas:** no las comparé con la línea base de `main`. En el RED y en el GREEN son las mismas.
