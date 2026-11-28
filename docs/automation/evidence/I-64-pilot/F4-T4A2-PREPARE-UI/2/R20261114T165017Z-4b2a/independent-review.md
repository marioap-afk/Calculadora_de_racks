# I-64 F4-T4A2-PREPARE-UI — Re-revisión adversarial independiente de la corrección (intento 2)

## Objeto

- **Diff revisado:** `git diff f8ccef46..ae0a7b40`, que lleva el RED de la corrección `f2bb4d6ff9ca1801be3c0addbf7dec20a01ac219` (solo pruebas y guardas) y el GREEN `ae0a7b404d2799ef690b3771c6083e988637a5a3` (solo `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`).
- **Código revisado:** el de `ae0a7b40`.
- **Autoridad:**
  - `docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/coordinator/gate-contract-attempt2.json`: las correcciones añadidas a INV-02, 03, 04, 05, 12, 13 y 15, el nuevo INV-18, y Core con `ExpectRed` false;
  - `docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/1/R20261114T160452Z-872b/analysis.md`.
- **Afirmaciones del Worker que se verifican:** `artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/2/R20261114T162137Z-b48d/worker-handoff.json`.

## Método

- Solo lectura. No se compiló ni se ejecutó nada.
- Se compararon los dos contratos campo a campo.
- Se leyeron el diff completo, el VM final, las guardas y las pruebas corregidas, `DraftSession.InitialDraftState`, `PushBackPilotStateComparer`, `OnFieldChanged` del navegador y los TRX del RED de la corrección.
- No se verificó la CI del intento 2: el handoff solo aporta ejecuciones locales.

## 1. Alcance y mecánica de la corrección

- **Commits de custodia.** `192103f8` y `f8ccef46` no tocan `src/` ni `tests/`: `git diff --stat 9333396d..f8ccef46 -- src tests` sale vacío.
- **Archivos del diff.** Son 7, todos dentro de las 15 rutas:
  - el VM, solo en el GREEN;
  - en el RED, `WorkspaceEditIntentGuardTests.cs` (una línea retirada, M-9), `WorkspacePilotReconciliationGuardTests.cs`, `WorkspacePilotReconciliationHostTests.cs`, `WorkspaceDraftPilotCanonicalLineEndingTests.cs`, `WorkspacePilotReconciliationTests.cs` y `WorkspacePilotReconciliationViewTests.cs`.
- **Lo que no cambia.** El host, `WorkspaceEditIntent.cs`, el XAML, el código subyacente, el doble, el navegador, el panel y el Plugin quedan igual (diff vacío). Ninguna prueba anterior a T4A2 cambia. La línea retirada de `WorkspaceEditIntentGuardTests.cs:865` la añadió esta tarea en el intento 1, así que no es una guarda existente según C02.
- **El RED de la corrección (INV-18).**
  - Core: 2923/2923 en verde, coherente con `ExpectRed` false.
  - UI: 3 fallos de 601, exactamente las tres pruebas de M-1 (`WorkspacePilotReconciliationTests.cs:459`, `:475` y `:488`). Los tres mensajes del TRX son `Assert.NotEqual() Failure: Strings are equal`: fallan por aserción contra el producto de `9333396d`, como pide INV-18.
- **El GREEN.** Las cuentas locales son Core 2923 y UI 601, con las suites completas en verde y las 17 omitidas de línea base.
- **Cuadre de las cuentas.**
  - Core: +5 pruebas nuevas de guarda.
  - UI: +3 de M-1, +1 de M-2 y +1 de M-6. La prueba tautológica del corpus se sustituye por otra `Fact`, así que el neto ahí es 0.

## 2. Cada hallazgo de la revisión del intento 1

| Id | ¿Resuelto? | Evidencia y discriminación |
|---|---|---|
| **I-1** | **Sí** | `WorkspacePilotReconciliationGuardTests.cs:576-584`: sin cuentas exactas, `inside >= 1` y `inFile - inside >= 1`. Ninguna otra prueba ni guarda de la tarea fija el número de `_authorize(` (búsqueda en los archivos de la tarea). La función de la guarda (`:562-569`) y su autoprueba de no-unicidad siguen igual. T4B ya no queda congelado. |
| **M-1** | **Sí, con un efecto secundario** (m-1 abajo) | `WorkspacePilotFieldViewModel.cs:683`, `:722` y `:770`: si `Render` deja el campo en fallo, no se presenta el resultado. `PresentChoice` ahora fija el aviso después de `Render` (`:743-774`), y su comportamiento en la ruta normal es equivalente al de antes. Las tres pruebas nuevas (`:459-508`) hacen lanzar `GetField` desde el `ReadHook` de la operación y fallaban en `9333396d` (TRX): **discriminan**. |
| **M-2** | **Sí** (declaradas) | Las dos desviaciones están en `Deviations`. La rama A2-34 tiene prueba (`:512-525`): vista mostrada en Clean, memoria del doble en Applying, `Reads()` vacío y A2-34. Discrimina: si la rama llamara a `TryConfirmBase`, `Reads()` no estaría vacío. `ReconcileText` vacío con fallo queda declarado sin prueba (observación O-2). |
| **M-3** | **Sí** (declarada) | `WorkspacePilotReconciliationTests.cs:1101-1104` fija `NoticeText == ""` tras el descarte del navegador. La desviación está declarada y es aceptable: la retirada la hace el `Rematerialize` del navegador, que no se puede tocar (C04). |
| **M-4** | **Sí** | `WorkspacePilotReconciliationGuardTests.cs:88` y `:92-99`: la forma de invocación `_port.TryX(request)`, una vez en el VM y una vez dentro del gesto. La autoprueba de `:128-145` (cuatro `Mutate`, cada uno un `Replace` del texto real) captura el grupo de métodos dentro del cuerpo correcto. Comprueba antes que el token sigue una vez dentro de `ConfirmChoice` (`:138`), así que solo la regla nueva puede detectarlo: **discrimina**. No hay evasión nueva (observación O-1). |
| **M-5** | **Sí** | Las cuatro partes están corregidas:<br>• **Sección del caso** (`:481-505`): un `Replace` del bloque real que mueve `UnchangedText` al caso Conflict sin quitar la etiqueta. Exige la violación «is not referenced inside case …» y que no salga «there is no …». Así ejerce la rama que antes no tenía autoprueba.<br>• **Mitades** (`:508-523`): son un solo `Replace` del texto real de la carpeta (`Content="Comprobar ahora"`), incluidos los dos negativos ordinales.<br>• **Carpeta de la regla** (`:381-405`): `XamlFiles()` enumera todos los `.xaml` de la carpeta, con su prueba.<br>• **Orden de la pregunta** (`:316-346`): `QuestionOrderViolations` se integra en la regla 3, con dos autopruebas por intercambio de `x:Name` que comprueban el mensaje concreto. |
| **M-6** | **Sí** | `WorkspacePilotReconciliationTests.cs:877-894`: «No» sobre Unknown/PostReread da cero llamadas, el borrador igual, A2-36, A2-12 y «Desconocido». |
| **M-7** | **Sí** | Las cuatro pruebas débiles están corregidas:<br>• **Precondición `Assert.True(HasChoice)`** antes de cada retirada (`:904`, `:979`, `:1002`, `:1017`, `:1025`, `:1032`, `:1062` y `:1070`).<br>• **Corpus** (`WorkspaceDraftPilotCanonicalLineEndingTests.cs:66-77`): deja de ser tautológico; exige que existan los ocho archivos y hace la ida y vuelta de cada uno.<br>• **Marca de obsolescencia** (`:230-249`): la prueba afirma ahora el `StateText` de cada caso, incluido «Modificado (posiblemente obsoleto)», y ya no repite el caso sin marca.<br>• **Salir de la caja** (`WorkspacePilotReconciliationViewTests.cs:290-292`): provoca `UIElement.LostFocusEvent` sobre la caja real, cableada a `OnBrowserFieldBoxLostFocus` en el XAML. Si la caja fuera editable con una elección pendiente, `CommitOp` sería 1: discrimina. |
| **M-8** | **Sí** (desviación declarada, **aceptable**) | `WorkspacePilotReconciliationHostTests.cs:88-89`: `Assert.Same` para `CurrentDraftState` (y para la base, como antes), y el estado inicial con `PushBackPilotStateComparer`. Se comprobó la premisa: `InitialDraftState => _copy(_initial)`, copia en cada lectura (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:283-284`, N-2). La comparación por referencia que pedía la corrección es imposible por contrato del núcleo. El comparador es exactamente la igualdad con la que el borrador decide `IsDirty` (`:298`), y una recarga indebida ya la detecta `Assert.Same` sobre la base. El texto de INV-13 del contrato del intento 2 pedía algo imposible; conviene que el cierre lo registre (observación O-3). |
| **M-9** | **Sí** | `WorkspaceEditIntentGuardTests.cs:865` está retirada y la aserción de M-3 del paquete sigue. La nota para el paquete de T4B está en `analysis.md` §2. |

## 3. Desviaciones declaradas

Las cuatro son aceptables:

- **M-2 (1), la rama A2-34 de `CheckNow`:** es conservadora (no lee el dibujo si la oferta dejó de valer) y ahora tiene prueba.
- **M-2 (2), `ReconcileText` vacío con un fallo:** en Error el campo ya muestra su propio estado; mostrar «Qué cambió» junto a un Error sería más confuso que callarlo. No tiene prueba (O-2).
- **M-3, la retirada silenciosa tras un descarte del navegador:** es un efecto del `Rematerialize` existente del navegador. El usuario hizo un descarte explícito y confirmado, y que desaparezca una elección que ya no tiene objeto no le esconde nada que necesite. Está fijada en la prueba.
- **M-8, el estado inicial por comparador:** es lo único posible y es semánticamente correcto (§2).

## 4. Hallazgos nuevos

### MENOR

**m-1. La salida anticipada de M-1 también se salta `_changed?.Invoke()`, aunque la operación del host puede haber cambiado el borrador.**

- **Evidencia:** `WorkspacePilotFieldViewModel.cs:683`, `:722` y `:770` hacen `return` antes de `_changed?.Invoke()` (`:687`, `:726` y `:774`). Es justo el caso que el Coordinator pidió mirar.
- **Cuándo pasa:** cuando el resultado es `Reloaded`, `Kept`, un `Conflict` de cualquiera de las dos vías o `Recovered`, el host ya cambió el borrador antes de que `GetField` lanzara.
- **Qué rompe:**
  - `_changed` está documentado como «Called after an operation that changed the draft, so the browser rebuilds its strip» (`:117`), y es `OnFieldChanged` → `Rebuild` → `Publish` + `RenderField` (`src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:817-821`).
  - Sin esa llamada, la marca de pestaña protegida y la fijación de la tira quedan obsoletas hasta el siguiente `Rebuild`. «Recargar» (`Rematerialize`) no las rehace, porque no llama a `_changed`.
  - Contradice el paso 5 de D-T4A2-01 e INV-03 («… la presentación, Render y _changed»), y no está declarado. El intento 1 sí llamaba a `_changed` en esa ruta.
  - Ninguna de las pruebas de M-1 comprueba `rig.Changed`, así que no está fijado en ningún sentido.
- **Por qué es menor:**
  - La ruta solo existe si el puerto incumple su contrato, porque `GetField` «never throws» (`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:56`).
  - La obsolescencia es visual y tiende a ser conservadora: tras una recarga, la pestaña seguiría marcada como protegida. Las decisiones de cerrar o descartar vuelven a consultar `GetRacksAtRisk` en ese momento.
- **Corrección** (dos opciones):
  - llamar a `_changed?.Invoke()` antes de cada `return` de esas tres líneas; queda contenido, porque cualquier excepción vuelve al `catch` del gesto y a `EnterFailure`; y añadir `Assert.Equal(1, rig.Changed)` a las tres pruebas de M-1;
  - o declarar la omisión como desviación del paso 5.

### Observaciones (no cuentan)

- **O-1, evasión residual de la regla 1 (no es nueva):** guardar un cierre, por ejemplo `_later = () => _port.TryKeepMine(request);`, dentro de `ConfirmChoice` e invocarlo desde otro cuerpo seguiría pasando la regla 1, porque la invocación está una vez y dentro del gesto. Es de la misma clase que M-4, pero exige ofuscación deliberada, y la corrección cubre lo que pedía el contrato (el grupo de métodos). Si se quiere cerrar, prohibir `=>` y `delegate` dentro de los argumentos y asignaciones de esos dos cuerpos.
- **O-2:** la segunda desviación de M-2 (`ReconcileText` vacío con un fallo) está declarada pero sin prueba. Una aserción en `TheChoiceIsWithdrawnInSilence…` (rama `failed`) la fijaría sin coste.
- **O-3:** el texto de la corrección de INV-13 en `gate-contract-attempt2.json` («compara … el InitialDraftState … por referencia (Assert.Same)») es imposible por N-2. El Worker lo declaró bien; el cierre debería anotar que el contrato pedía algo inalcanzable y que se aceptó la comparación por comparador.

## 5. Afirmaciones del handoff

Todas las comprobadas se sostienen:

- los 7 archivos, con un GREEN solo del VM;
- el RED con 3 fallos de UI por aserción y Core en verde;
- las cuatro desviaciones con su motivo real;
- el hallazgo inesperado de N-2, verificado en el código;
- «ninguna prueba existente necesitó cambio».

Lo único que el handoff no menciona es m-1.

## 6. Veredicto

**ACEPTABLE CON MENORES** (B/I/M = **0/0/1**).

- **I-1 y los nueve menores del intento 1 están resueltos.** Las pruebas nuevas discriminan, y las autopruebas corregidas son un `Replace` del texto real o un intercambio explícito. No hay evasiones nuevas.
- **Nada cambió fuera de la corrección declarada,** y el RED cumple INV-18.
- **Las desviaciones declaradas** (dos de M-2, una de M-3 y una de M-8) son aceptables.
- **Solo queda m-1:** la salida anticipada de M-1 se salta `_changed`, en una ruta defensiva que el contrato del puerto hace inalcanzable. Se puede corregir con una línea por rama más su aserción, o declararlo en el cierre.
