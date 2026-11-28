# I-64 — Revisión adversarial independiente del paquete del contrato de F4-T4A2 (v1)

**Objeto:** el paquete v1 de F4-T4A2 («Comprobar ahora» y reconciliación), contra la orden literal del Owner, el Freeze y el código.

**Base medida:** HEAD `b9e1ed5a` (producto idéntico a `bbd9a35e`). Revisión de solo lectura: no se modificó ningún archivo del repositorio, no se compiló y no se ejecutó ninguna prueba. Solo se leyó y se buscó con grep en `src/`, `tests/` (sin `bin/` ni `obj/`), `docs/` y `.github/`.

**Material:** el paquete, la orden del Owner y los tres mapas (map-1, map-2, map-3). Los mapas se trataron como afirmaciones que había que verificar. Todas las rutas de este informe son relativas al repositorio.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Cuentas: **B = 1, I = 5, M = 16.**

El paquete está bien construido. Ninguna prueba ni guarda existente del repositorio se rompe con el diseño declarado fuera de lo que el propio paquete declara (§3 de esta revisión). Pero hay dos especificaciones de guarda nuevas que contradicen el código real:

- **B-1** es irrealizable tal como está escrita.
- **I-1** casi seguro acaba en una desviación o en un STOP.

Las dos repiten el tipo de defecto que costó un intento en la tarea anterior. Además, hay cuatro decisiones de diseño o de guarda que no cumplen del todo lo que la orden del Owner y el Freeze piden (I-2 a I-5). Las correcciones son baratas y no cambian la lista blanca.

---

## 1. BLOQUEANTE

### B-1. La regla nueva de M-2 no se puede cumplir sobre el código real

**Dónde:** el paquete, D-T4A2-07, M-2, segunda viñeta. Dice, literalmente, que en el núcleo del host el identificador `pilot` «solo aparece en `var pilot = read.PilotRead;`, en `pilot == null`, en `pilot.StaleBase` y en `pilot.Read`. Quitadas esas formas, no queda ningún `\bpilot\b`».

**Evidencia:** en `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`, `pilot` también aparece, ya hoy, en tres formas que la regla no admite. Son 12 apariciones en cuatro cuerpos (intención, recarga, Conservar y Descartar y recargar):
- `pilot.PreviewWitness`: `:329`, `:488`, `:651` y `:732`;
- `pilot.Detail`: `:348`, `:500`, `:663` y `:744`;
- `pilot.ClassicEditorHint`: `:349`, `:501`, `:664` y `:745`.

**Por qué importa:** si el Worker implementa la regla tal cual, la guarda falla en el RED **y** en el GREEN. Para ponerla en verde tendría que reescribir cuatro cuerpos del host, cosa que prohíbe INV-09 («el único cambio del host es D-T4A2-04»). También podría rebajar la regla, lo que es una desviación del contrato. Es justo el patrón que ya causó un STOP: el paquete especifica una guarda sin medirla contra el texto real.

**Corrección:**
1. Formular la regla como una lista de accesos permitidos y medirla en `b9e1ed5a`: `var pilot = read.PilotRead;` (6), `pilot == null` (6) y `\bpilot\s*\.\s*(?:StaleBase|Read|PreviewWitness|Detail|ClassicEditorHint)\b`. Quitadas esas formas, no queda ningún `\bpilot\b`. El paquete debe escribir las cuentas medidas por cuerpo.
2. Precisar que el `BaseOrReadUse` endurecido **añade** la alternativa `\bpilot\s*[?!]?\s*\.\s*Read\b` y **conserva** la de `result` (`\b(?:pilot|result)\.Read\b`, `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:645`). Si no, la autoprueba existente `var state = result.Read;` (`:773`) deja de fallar.
3. Mantener las autopruebas declaradas (`var p = pilot; …`, `pilot?.Read…`, `pilot!.Read`, `Use(pilot)` y `(pilot).Read`) y añadir `pilot.Other` (un acceso que la lista no admite).

---

## 2. IMPORTANTES

### I-1. La regla 5 (`Authorize` por forma exacta) describe mal `PassGate`

**Dónde:** el paquete, D-T4A2-07, regla 5: «en `PassGate`, `if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted)` seguido del Rejected con `NotThisDocumentReason`».

**Evidencia:** `PassGate` no usa la constante. Asigna el literal `reason = "La petición no es del documento activo.";` y luego `return EditGate.Rejected;` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:1364-1367`). La constante `NotThisDocumentReason` (`:45`) solo se usa en `:565`, `:644`, `:725`, `:905`, `:932`, `:983`, `:1016` y `:1040`.

**Por qué importa:** el Worker solo tiene dos salidas:
- **Escribe la guarda con `NotThisDocumentReason`.** Falla sobre el código real. Para arreglarlo tendría que cambiar el host, lo que prohíbe INV-09, y el cambio, además, altera la cuenta exacta del literal en la tabla de 30 filas (`tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs:424`, «La petición no es del documento activo.», 6). Ese archivo está fuera del alcance, así que es C02 y STOP.
- **Ajusta la guarda al literal.** Se desvía del texto del contrato.

**Corrección:**
- Escribir la forma real: `if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted) { reason = "La petición no es del documento activo."; return EditGate.Rejected; }`, con espacios flexibles.
- Añadir la mutación que la orden nombra y la lista omite: el **documento** equivocado en el lector del Plugin (`document.UnmanagedObject.ToInt64()` sustituido por otra expresión, por ejemplo `0L`).
- Exigir «toda aparición de `_authorize(` sigue la forma» y no «exactamente una». Así no se congela la lectura en contexto de comando de T4B (R-T4-01 y R-T4-06 del paquete de T4).

### I-2. La guarda de «quién llama» (regla 2) se esquiva con una llamada sin calificar

**Dónde:** el paquete, D-T4A2-07, regla 2. Solo busca `.CheckNow(`, `.ConfirmChoice(`, `.CancelChoice(`, `.RequestDiscardAndReload(` y `.RequestKeepMine(`, **con el punto delante**.

**Evidencia:** dentro de `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`, una llamada `CheckNow();` sin calificar, un grupo de métodos (`Action a = CheckNow;`) o una llamada desde `Show`, `Render`, `Apply` o el setter de `Text` no contiene `.CheckNow(`. La guarda existente de las lecturas tampoco la ve: busca el token `TryCheckNow` o `TryConfirmBase` en el cuerpo del miembro de presentación, no por transitividad (`tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:82-97`). La regla 1 solo exige que `TryConfirmBase` y `TryCheckNow` estén en el cuerpo de `CheckNow`, no quién llama a `CheckNow`.

El único respaldo de ejecución es T15 (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:1115-1160`), y es parcial:
- solo cubre su escenario;
- nada cubre una llamada a `ConfirmChoice` desde la presentación.

**Por qué importa:** la regla 2 es la prueba estática de INV-02 y del punto 1 de la orden («acción explícita del usuario»). Además, el Worker anterior ya esquivó una guarda renombrando una variable.

**Corrección:**
- Hacer la regla por token en todos los `.cs` de `src/RackCad.UI/`: `\b(?:CheckNow|ConfirmChoice|CancelChoice|RequestDiscardAndReload|RequestKeepMine)\b`. Fuera de su declaración `public void X()`, solo pueden aparecer en su manejador exacto del código subyacente. `TryCheckNow` no coincide, porque `\bCheckNow` exige un límite de palabra.
- Añadir autopruebas por `Replace` sobre el texto real:
  - `CheckNow();` dentro de `Show`;
  - `this.ConfirmChoice();` dentro del setter de `Text`;
  - `Action a = CheckNow;`;
  - una llamada desde el navegador.

### I-3. «Conservar» con solo texto pendiente revierte en silencio un cambio externo de la escala

**Dónde:** el paquete, D-T4A2-02 («Conservar» solo con `IsProtected` verdadero), INV-06 y el texto A2-22.

**Evidencia:**
- En Conflict, `IsProtected` es `draft.IsDirty` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:1163-1178`).
- `IsDirty` es verdadero **también solo con texto pendiente** (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:298`).
- `KeepPilotEditsOn` copia siempre la escala del estado actual (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs:242-251`).

Un borrador cuyo único trabajo propio es un pendiente (inválido, por ejemplo) tiene el estado actual igual al inicial anterior. Si RACKEDITAR cambió la escala fuera, «Conservar» pone la escala **anterior** sobre la versión nueva. Es exactamente el caso de H-13 que el paquete cita para justificar la restricción, y no lo evita. A2-22 lo presenta como «¿Conservar **tu** escala de anotación (…)?», aunque el usuario nunca la editó. Ninguna prueba ni escenario de Smoke-2 cubre este caso.

**Por qué importa:** la orden dice «No silent rollback of external changes. Only the pilot-owned editable value(s) come from the user's draft». La justificación de la sesión es internamente incoherente, porque `IsProtected` no equivale a «la escala fue editada».

**Corrección** (sin tocar el puerto, que está fuera del alcance):
1. Declarar el caso.
2. Cambiar A2-22 a un texto honesto, por ejemplo «¿Poner la escala del borrador (X) sobre la versión actual del dibujo? Sustituye a la del dibujo si cambió…».
3. Añadir una prueba de Core: pendiente sin cambio de estado más un cambio externo de la escala, y «Conservar» da la escala anterior. La prueba fija el comportamiento declarado.
4. Añadir una prueba de UI que fije el texto.
5. Añadir una fila S2-R a Smoke-2.

Si la sesión prefiere no ofrecer «Conservar» en ese caso, el VM solo puede distinguirlo por `PendingText != null` con `CommittedText` sin cambio frente a la base, y eso no está en la vista. Habría que declararlo como limitación.

### I-4. «No editable aquí» tras «Comprobar ahora» no rechaza la entrada y se borra al teclear

**Dónde:** el paquete, D-T4A2-01, presentación de `Confirmed` y de `Conflict` con diagnóstico (A2-03 y A2-06).

**Evidencia:**
- `EditableOf` no mira `_diagnostic` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:344-349`). En Clean, Dirty o Conflict con `CanEdit`, la caja sigue editable con el diagnóstico a la vista.
- `ConfirmBase` limpia `IsPossiblyStale` en cualquier MATCH, también si la relectura no está admitida (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:650-653`). Un borrador Limpio «posiblemente obsoleto», de solo lectura, pasa a **editable** justo cuando el resultado dice que ya no es editable.
- La primera tecla llama a `ClearNotices` (`:80`, `:313-319`), que borra `_diagnostic`. Desaparece el «No editable aquí» y reaparecen «Descartar y recargar» y «Conservar», porque su oferta exige «sin diagnóstico».
- `EditConfirmOutcome` no lleva `ClassicEditorHint`, así que A2-03 y A2-06 salen sin el editor clásico ofrecido.

**Por qué importa:**
- Contradice D-11: «No editable aquí, con el diagnóstico visible, la entrada rechazada y el editor clásico disponible» (`docs/initiatives/I-64-proposal-v5.md:457-458`).
- Contradice INV-31: «el diagnóstico de admisión con el editor clásico ofrecido» (`:769`).
- Debilita la clase NOT_FOUND / NO_LONGER_EDITABLE que la orden pide distinguir.

**Corrección:** decidir y declarar una de estas dos opciones.
- **(a)** Con `_diagnostic` presente y borrador existente, la caja es de solo lectura (`EditableOf` lo mira). El diagnóstico solo se retira con una comprobación nueva que no lo traiga o con un cambio de superficie, no con `ClearNotices` de una tecla. Se muestra la pista del editor clásico con un texto del VM o se declara su ausencia. Según lo leído, no rompe ninguna prueba existente, aunque hay que comprobarlo antes de emitir:
  - las pruebas con `NotAdmitted` de la UI son de racks sin borrador (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:569` y `:1220`; `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:302` y `:631`);
  - la única con borrador (`WorkspacePilotFieldViewModelTests.cs:648`) es un Limpio «posiblemente obsoleto» que ya es de solo lectura, y afirma `IsEditable` falso.
- **(b)** Declarar que la edición sigue permitida y que T4B la rechazará en la admisión de Actualizar, con una prueba que fije el comportamiento elegido.

### I-5. La vía Unknown(CommitThrew) de «Comprobar ahora» no tiene tabla de clases y confunde NOT_FOUND con CONFLICT

**Dónde:** el paquete, §3 (solo cubre `EditConfirmStatus`) y D-T4A2-01 (presentación de `TryCheckNow`).

**Evidencia:**
- `EditCheckOutcome` no lleva diagnóstico (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:237-280`).
- `TryCheckPilotEdit` responde `Conflict(DescribeDifferences(...))` aunque la relectura no esté admitida (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:408-423`).
- Un rack borrado tras un commit que lanzó se presenta como A2-04 o A2-05 («El dibujo cambió fuera del panel…, elige…»). Las dos opciones que se ofrecen acabarán en NotAdmitted (A2-32).
- Extenderlo ahora rompería guardas existentes: `CheckBody` exige cero `pilot.Read` y cero del patrón del diagnóstico (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:681` y `:696-697`).

**Por qué importa:** la orden exige que «el contrato de T4A2 distinga explícitamente» las cinco clases para el resultado de «Comprobar ahora», y la vía de `TryCheckNow` es la mitad del despacho de D-T4A2-01 (INV-31: «el estado Desconocido con «Comprobar ahora»»).

**Corrección mínima:**
1. Añadir al §3 las filas de `EditCheckStatus` (Recovered para UNCHANGED, Conflict, Failed para READ_ERROR, y el resto para UNKNOWN).
2. Declarar como limitación que en esta vía NOT_FOUND y NO_LONGER_EDITABLE se ven como CONFLICT y que la elección posterior revela el diagnóstico (A2-32).
3. Asignarlo a T4B, que produce el estado Unknown y tocará el host, o extenderlo ahora con los cambios de guarda declarados.

---

## 3. Viabilidad bajo la lista blanca: lo que se comprobó y no rompe

Se buscó en todo `tests/` cada guarda o prueba que el diseño pudiera tocar. **No se encontró ninguna que tenga que cambiar fuera de lo declarado**, salvo las especificaciones defectuosas B-1 e I-1, que son guardas nuevas del propio paquete. Detalle:

**Los botones nuevos del XAML:**
- G-04 (`tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:323-347`) y su gemela (`tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:728-741`) cuentan `Content="X"` con la comilla de cierre. `Content="Descartar y recargar"`, `"Sí"`, `"No"`, `"Comprobar ahora"` y `"Conservar mi borrador"` no coinciden.
- `Click="OnBrowserField…"` lleva la comilla, así que tampoco hay choque de manejadores.
- Las autopruebas por `Replace` (`:744-756`, `:666-692` y `:766-779`) siguen fallando donde deben.

**Los nombres y los enlaces:**
- `CheckNewXamlElements` (`WorkspacePilotCloseSyncGuardTests.cs:614-663`) busca `x:Name="BrowserFieldDiscardButton"` y `AutomationProperties.Name="Descartar el borrador"`, los dos con la comilla de cierre. `BrowserFieldDiscardAndReloadButton` y «Descartar el borrador y recargarlo» no colisionan.
- Las búsquedas por nombre de automatización de las pruebas de vista son por igualdad exacta (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncViewTests.cs:83-84`).
- El panel del navegador sigue con dos botones (`:180-182`).

**La forma pública:**
- La forma de 33 firmas es solo del navegador, que no cambia.
- El VM del campo solo tiene fijados `ShowDiscardButton` (`WorkspacePilotCloseSyncGuardTests.cs:333`), la cabecera y el constructor (`tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:325-335`). `ShowDiscardAndReloadButton` no coincide con `ShowDiscardButton\s*=>`.
- La regla `NonGestureMembers` exige `\s*\(` tras `Show` o `Clear`, así que `ShowCheckButton` y `ClearNotices` no coinciden (`WorkspacePilotSurfaceGuardTests.cs:64-75`).

**Teclado, tamaños y mecanismos:**
- M-6, `IsDefault` e `IsCancel`, y tamaños fijos (`WorkspacePilotCloseSyncGuardTests.cs:694-709` y `:797-839`): el diseño no añade teclado, foco ni tamaños.
- Las listas fijas de controles con nombre (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:652-675`, `WorkspacePilotCloseSyncViewTests.cs:438-460` y `tests/RackCad.UI.Tests/Workspace/WorkspaceBrowserViewTests.cs:367-401`) no incluyen los controles nuevos.
- Ninguna prueba enumera los botones de la superficie ni las propiedades que levanta `RaiseAll`.
- Ni `+=`, ni `event`, ni `static`, ni `Dispatcher`: `WorkspacePilotCloseSyncGuardTests.cs:473-502` y `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs:757-783`.

**El vocabulario y los caracteres:**
- Se comprobó el vocabulario prohibido (`tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:583-601`, `WorkspaceBrowserHostGuardTests.cs:256-264`) contra todos los textos de T-A2: ninguno coincide.
- No existe ninguna guarda de ASCII ni de `«»` sobre la UI. El XAML y el VM ya contienen caracteres que no son ASCII.

**El doble, M-9:**
- Ninguna prueba fuera del doble usa `ConfirmBaseOp`, `KeepMineOp`, `DiscardAndReloadOp` ni sus respondedores.
- Las 30 apariciones de `.Reads()` (cuenta confirmada) no llaman hoy a ninguna de esas operaciones.
- Ninguna guarda lee el texto del doble.

**`EditConfirmOutcome.Conflict` con un parámetro opcional:**
- No hay ningún grupo de métodos sobre la fábrica (`src/` y `tests/`: solo hay llamadas).
- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs:1336-1338` sigue dando null.
- La reflexión de los portadores (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:982-998`) solo mira espacios de nombres.

**El diagnóstico subido a una variable local en `TryConfirmPilotBase`:**
- Las cuentas de `WorkspaceEditIntentGuardTests.cs:693-695` (1 del patrón y 2 de `pilot.Read`) y la exención (`:658-659`) se conservan.
- Ninguna guarda fija el texto de la línea `:579` ni el del `case` de Conflict.

**Los resúmenes de M-8:**
- `TheDocumentationOfWaitingAndOfNotAdmittedTellsTheReloadCases` toma el **primer** `"        Waiting,"`, que es el de `EditIntentStatus` (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:88`), no el de `EditConfirmStatus` (`:288`).
- Ver M-13: hay prohibiciones de palabras **también en los comentarios** de esos archivos.

**C2-M-1 y C2-M-2:**
- Las formas exactas propuestas coinciden con `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:43-49` y `:60-69`.
- Ninguna guarda lee el texto de `WorkspaceCommandAdvisorGuardTests.cs`.

**D-T4A2-08:** los ocho diseños canónicos llevan `\r\n` **escapado** dentro del JSON del sobre (`tests/RackCad.Tests/Fixtures/I64/pb-0*.json`). La aserción «contiene `\r\n`» no depende de `core.autocrlf` del runner. `ComposeForWrite`, `Materialize` y `BuildDesign` son públicos.

**Filtros de la CI:** los nombres de las clases nuevas empiezan por `Workspace`, así que los selecciona `FullyQualifiedName~RackCad.(UI.)Tests.Workspace`, como fija el precedente `WorkspacePilotCloseSyncGuardTests.cs:964-984` (ver M-15).

## 4. Hechos H-01 a H-18

Todos se verificaron contra el código. Las líneas citadas son correctas, con estos matices:
- **H-07:** las 30 aserciones sobre `Reads()` son correctas.
- **H-10, H-11 y H-13:** correctos. Ver I-3 sobre el caso con solo texto pendiente, que H-13 no menciona.
- **H-16:** correcto en lo que describe. La regla 5, que se apoya en él, lo cita mal (I-1).
- **Ningún hecho mide** las formas de `pilot` del núcleo (B-1) ni el diagnóstico exacto que da `MemberBecameBlocking` (M-01).

## 5. Conformidad con la orden del Owner (resumen)

| Punto de la orden | Estado |
|---|---|
| Diez puntos de «Comprobar ahora» | Se cumplen por la vía existente (H-16), pero sin trazabilidad uno a uno (M-16) |
| Clases de resultado | Completas para `TryConfirmBase`; faltan para `TryCheckNow` (I-5) |
| Sin falso limpio | Diseño correcto; la guarda literal se esquiva y la condición de A2-01 es laxa (M-02, M-03) |
| Opciones sin una cuarta política | Correcto. Se desvía del rótulo «Recargar» de D-T4A1-03 sin declararlo (M-04) |
| Confirmación y prueba de inversión Sí/No | Correctas: VM, vista, fuente y C2-M-1 con las mutaciones M1a a M1f |
| `Authorize` endurecido | C2-M-2 correcto; regla 5 mal medida (I-1) |
| Conservar mi borrador | Regla de IC-07 punto 2 correcta; caso con solo texto pendiente sin cubrir (I-3) |
| Recarga | Correcta |
| Texto pendiente (prueba 13) | La interpretación es sólida, pero hay que reforzar su justificación y avisar de ella (M-14) |
| Cero escrituras | Correcto |
| Las 18 pruebas | Mapeadas; huecos menores (M-01, M-02, M-07) |
| TOCTOU y plan inmutable | Correctos (D-T4A2-09) |
| Asignación de los menores | Correcta y conforme a la regla del Owner; M-1 va a T4B con un motivo defendible |
| Deuda de controles | No figura en el contrato (M-16) |

---

## 6. MENORES

- **M-01. Fijar el valor esperado de `:993`.** El paquete deja a elección del Worker «el diagnóstico exacto que dé `MemberBecameBlocking`». Se puede medir:
  - el miembro bloqueante cae en la regla 7: `UnreadableDiagnostic` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:192-194`, constante en `:35`);
  - el único cambio declarado de una prueba existente no debe tener su oráculo abierto.
- **M-02. La guarda «sin falso limpio» (regla 4) se esquiva con una concatenación o una constante.**
  - Propuesta: el texto en **una** constante, una sola referencia a ella y solo en el caso Confirmed, y ninguna otra aparición de la subcadena `Sin cambios` en `src/RackCad.UI/`.
  - La fila 8 del §5 del paquete debe incluir, de forma explícita, la excepción del puerto, un valor de enumerado desconocido (`(EditConfirmStatus)99`) y la teoría equivalente sobre `EditCheckStatus`: A2-07 solo con Recovered.
- **M-03. La condición de A2-01 es laxa.** «Vista releída fuera de Conflict» admite NotOpen, Applying, Unknown o Error si la lectura bombea mensajes y el borrador cambia o desaparece con la misma superficie. Exigir la vista releída en Clean o Dirty.
- **M-04. Unknown(PostReread):**
  - A2-20 dice «los cambios **sin aplicar**», pero en esa fila de D-13 los cambios **se aplicaron** (`docs/initiatives/I-64-proposal-v5.md:529`). Hace falta un texto propio.
  - El rótulo «Descartar y recargar» se desvía de D-T4A1-03, que dice «Recargar» (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:322`). Es válido, porque la gemela exige un solo `Content="Recargar"` (`tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:738`), pero hay que declararlo con ese motivo.
- **M-05. El Conflicto que sale de `TryCheckNow` usa A2-04 y A2-05, «El dibujo cambió fuera del panel».** Tras un commit que lanzó, la diferencia puede venir del propio commit: D-13 dice «Nunca se supone aplicado» (`docs/initiatives/I-64-proposal-v5.md:531-532`). Usar un texto neutro («el dibujo no coincide con la base del borrador»).
- **M-06. Foco y elección pendiente.**
  - Con texto pendiente y el foco en la caja, el clic en «Sí» o «No» dispara antes `LostFocus` y luego `CommitPending` (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:158`). `CommitPending` llama a `ClearNotices` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:249-251`), que retira la elección, y el clic se pierde sin aviso.
  - Un pendiente **válido** se compromete antes de «Comprobar ahora» o de «Conservar». Por eso el «byte a byte» de D-T4A2-05 solo vale en el VM y con un pendiente inválido.
  - Declararlo, o no retirar la elección en `CommitPending` si nada cambió, y añadirlo a S2-R-09.
- **M-07. La revalidación de `ConfirmChoice`** debe comparar también la base del texto confirmado: `State`, `IsProtected` y si hay pendiente, no solo «la oferta sigue vigente». Además:
  - la presentación debe exigir que el estado corresponda a la operación elegida: Reloaded solo para Descartar y recargar, Kept solo para Conservar;
  - la prueba de «Sí» debe afirmar cero llamadas a la **otra** operación;
  - la elección debería retirarse al renderizar cuando la oferta ya no vale, por ejemplo tras un descarte del navegador.
- **M-08. Los prefijos de los resultados de `ConfirmChoice`.**
  - Para `NotNeeded`, `AlreadyOpen` u otros estados de `EditIntentStatus`, el valor por defecto «No se pudo comprobar:» no describe un «Conservar».
  - Tras `NotFound`, «el borrador no cambió» es incoherente, porque no hay borrador.
  - A2-22 necesita un texto de respaldo cuando el texto comprometido es null (`CommittedTextReason`).
- **M-09. Autopruebas de la regla 5.** Añadir el documento equivocado, según I-1, y no exigir unicidad de la forma del lector del Plugin, para no congelar T4B.
- **M-10. Reglas 2 y 3.**
  - Precisar el alcance: «en `src/RackCad.UI/`».
  - Atar también `AutomationProperties.Name` («Confirmar la elección» y «Cancelar la elección») a la etiqueta de cada botón, con su autoprueba de intercambio.
- **M-11. C06 es ambigua.** No está claro si un enumerado privado del VM para el tipo de elección es «un valor de enumerado nuevo». Hay que prescribir la representación o limitar C06 a Application y al puerto.
- **M-12. D-T4A2-08.**
  - Listar los ocho nombres canónicos: `PilotKit.Canonical` (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:37-40`) es interno de RackCad.Tests y no lo alcanza RackCad.UI.Tests. La carpeta contiene además diseños antiguos y otros que no dan un ida y vuelta idéntico.
  - Decir qué pasa si la igualdad byte a byte falla: es un hallazgo de caracterización, y el contrato debe fijar si es STOP o informe.
- **M-13. M-8 y los comentarios.**
  - El resumen de `EditConfirmStatus.NotApplicable`, «nothing was read» (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:296`), también es inexacto en el caso de un borrador que entra en Applying durante la lectura.
  - Hay que avisar de que `WorkspaceEditIntent.cs` y `WorkspaceHostCore.cs` prohíben palabras **también en los comentarios** (`Transaction`, `Commit(`, `Provider`, …; `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:935-945`).
- **M-14. Prueba 13.** La interpretación (ninguna reconciliación bloquea) es sólida:
  - D-10 enumera las fronteras bloqueables (`docs/initiatives/I-64-proposal-v5.md:434-435`);
  - la tabla de T3 trata el descarte con confirmación como «ninguna frontera; Descartar o Cancelar» (`docs/initiatives/I-64-f4-t3-gate-contract-package.md:79`).

  Hay que citar esa fila y registrar la interpretación en el informe al Owner, porque convierte una prueba que el Owner enumeró en una prueba de conservación.
- **M-15. Clases de prueba nuevas.** Fijar el espacio de nombres (`RackCad.Tests` y `RackCad.UI.Tests`), `public class` y el prefijo `Workspace`, para que los filtros de `RequiredTests` las seleccionen.
- **M-16. Trazabilidad.**
  - Añadir una tabla que lleve cada uno de los diez puntos de «Comprobar ahora» a su evidencia: guarda, prueba o H.
  - Añadir la cláusula de deuda de controles de la orden: los sustitutos PD-F4-01 y PD-F4-04 solo con su firma exacta.
  - Añadir a Smoke-2:
    - Dirty «posiblemente obsoleto» más «Comprobar ahora» da A2-01 y quita la marca;
    - el caso de I-3.

---

## 7. Resumen de las correcciones antes de emitir

1. **B-1:** volver a escribir la regla de M-2 con la lista de accesos permitidos y las cuentas medidas, y conservar la alternativa de `result`.
2. **I-1:** escribir la forma real de `PassGate`, con el literal, y añadir la mutación del documento, sin unicidad.
3. **I-2:** hacer la regla 2 por token en toda la UI, con autopruebas de llamada sin calificar y de grupo de métodos.
4. **I-3:** declarar el caso de «Conservar» con solo texto pendiente, con un texto honesto y su prueba.
5. **I-4:** decidir y declarar la editabilidad y la persistencia del diagnóstico tras «Comprobar ahora».
6. **I-5:** dar las clases de `TryCheckNow` y declarar la limitación de NOT_FOUND en esa vía.

Con esas seis correcciones y las menores que se quieran, el paquete es emitible sin cambiar la lista blanca de 15 rutas.
