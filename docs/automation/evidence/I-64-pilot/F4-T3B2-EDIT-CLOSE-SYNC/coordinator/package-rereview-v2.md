# I-64 — Re-revisión adversarial independiente del paquete de F4-T3B2-EDIT-CLOSE-SYNC (v2)

**Veredicto: EMITIBLE TRAS CORRECCIONES** — 0 BLOQUEANTE, 1 IMPORTANTE, 6 MENOR (todos nuevos).

**Base revisada:** rama `architecture/workspace-persistente-rackcad` en `1a9445bd`, con el producto y las pruebas de `1708e764`. Solo lectura, sin compilar.

**Resumen:**
- La v2 resuelve de verdad los 17 hallazgos de la v1.
- Introduce un defecto IMPORTANTE (N-1): la extensión de `keepActive` es más amplia de lo que su justificación permite y contradice la disposición N-2 del paquete de F4-T3. Nace de mi propia recomendación M-5 de la v1, que era imprecisa; aquí la corrijo.
- Los seis MENOR son de texto o de precisión.
- No he encontrado ninguna prueba ni guarda existente que la v2 rompa sin declararlo.

---

## 1. Estado de los hallazgos de la v1

| Hallazgo v1 | Estado | Comprobación |
|---|---|---|
| I-1 (prueba de vista rota por las colas) | **Resuelto** | P-01 declara el cambio de `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:218`. El alcance de §5 lo incluye y las paradas lo admiten. Re-barrido: ninguna otra prueba con puerto afirma ausencias de llamadas tras un `Present` sin vaciarlas (`:126` usa el doble del panel sin puerto; `:393` vacía tras `Editable()`; `T15`, `T17` y `AssertNoReadAndNoMutation` toleran las colas) |
| I-2 (la limpia no se cierra) | **Resuelto** | D-T3B2-01: se cierra siempre y avisa. P-02 es correcto. Con el doble por defecto (`Rejected`), `T10_AClosedTabOfACleanDraft…` (`WorkspacePilotFieldViewModelTests.cs:444-464`) sigue pasando: el borrador limpio queda, vuelve como preview y el gesto responde `AlreadyOpen`. `ASelectionThatLeavesATabWithoutADraft…` (`:815-830`) tampoco cambia |
| I-3 (las dos vías de D-17) | **Resuelto** | Desviación transitoria declarada y obligación de T3C, en D-T3B2-04 e INV-F4T3B2-08. Ver N-3, sobre otra frase del mismo texto |
| I-4 (la forma de la UI) | **Resuelto** | D-T3B2-07 y D-T3B2-08 dan la API, el record, los nombres del XAML, la automatización, los manejadores, la vida de la confirmación y del aviso, y la contención. Con ellos, las guardas de §4 se pueden escribir. Quedan precisiones menores (N-4, N-5, N-7) |
| I-5 (RED, filtros, alcance) | **Resuelto** | INV-F4T3B2-14 fija el esqueleto. `MinSelected` 2300 y 352 coincide con lo medido en el GREEN de `1708e764` (`docs/automation/evidence/I-64-pilot/F4-T3B1-EDIT-SURFACE/2/R20261113T062519Z-a807/ci-filter-counts.json`: Core 2300/2300, UI 352/352). `WorkspacePilotCloseSyncViewTests.cs` está en el alcance |
| M-1 | **Resuelto** | Desviación de letra declarada en D-T3B2-01 |
| M-2 | **Resuelto** | INV-F4T3B2-05 reformulada y D-T3B2-01 aclara la lectura del resumen de F3 |
| M-3 | **Resuelto** | §2.3, G-01 a G-09. Dos imprecisiones nuevas en N-6 |
| M-4 | **Resuelto** | INV-F4T3B2-01 cubre `Present`; la documentación de `pilotEdit` está en el alcance (H-06, G-08) |
| M-5 | **Resuelto en lo pedido, con un defecto nuevo** | Hay aviso con `R` abierta y el residual de la preview activa está declarado. Pero el predicado de «vista ilegible», que yo propuse, es demasiado amplio: N-1 |
| M-6 | **Resuelto** | Clasificación al mostrar; purga en el bucle de `PurgeDeadSessions` con una sola llamada a `IsSessionAlive`. Ver N-2 (ambigüedad con los avisos de pérdida) |
| M-7 | **Resuelto** | «Descartar» solo con borrador (D-T3B2-02); el aviso del documento separa los racks confirmados de los limpios (D-T3B2-03) |
| M-8 | **Resuelto** | D-T3B2-08 (comportamiento por defecto, respondedores, `ThrowIfAsked` en las cinco operaciones) y el oráculo de la prueba 11 |
| M-9 | **Resuelto** | Sin `IsDefault` ni `IsCancel`, con guarda; filas estables; dos limitaciones para Smoke-2 en D-T3B2-09 |
| M-10 | **Resuelto** | Prueba negativa y reserva de Smoke-2 (D-T3B2-06, §4) |
| M-11 | **Resuelto** | La guarda de §4 cubre el Plugin y Application. La lista de archivos es exacta: hoy solo nombran las colas `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:69,72`, `WorkspaceHostCore.cs:809,821`, `WorkspacePilotSupport.cs:192,194` y `src/RackCad.Plugin/Workspace/WorkspaceController.cs:265,267` |
| M-12 | **Resuelto** | Declarado en D-T3B2-09 |

---

## 2. Hallazgos nuevos

### N-1 (IMPORTANTE). La extensión conservadora de `keepActive` retiene pestañas cuyo pendiente sí se conoce, contra la disposición N-2 de F4-T3

- **Dónde:**
  - Paquete: D-T3B2-05, el VM pasa `keepActive` si la vista es «`Exists`, `State` `Error` o `Undecidable` e `IsProtected`, o `GetField` lanza», con la justificación «con el estado ilegible no se sabe si hay pendiente».
  - Autoridad: `docs/initiatives/I-64-f4-t3-gate-contract-package.md:469` (N-2: «Solo el texto pendiente retiene la pestaña activa») y `:80` («si no, también cuando solo está dirty, se activa…»).
  - Host:
    - `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:846-871`: el estado y la protección;
    - `:882-889`: el pendiente se lee de `draft.PendingText`, sin comparador y para cualquier estado;
    - `:603-606`: la única vista con el pendiente desconocido es la del `catch` (pendiente nulo).
  - `src/RackCad.Application/Workspace/Drafts/DraftSession.cs:285`: `Status` solo lanza a través de `IsDirty`, es decir, del comparador, y eso es lo que da `Undecidable`.
- **Qué está mal:** la premisa solo es cierta para la vista del `catch` de `GetField`. En los otros casos que captura el predicado, el pendiente es conocido y la vista lo trae exacto:
  - **`Undecidable`:** el comparador falló, pero el pendiente se lee igual. Con `PendingText` nulo, la pestaña es «solo dirty, de forma desconocida», sin texto pendiente. N-2 dice que entonces no retiene.
  - **Un `Error` explícito** (`DraftStatus.Error`, que conserva el pendiente, `DraftSession.cs:596-600`): con `IsDirty` y sin pendiente es una pestaña solo dirty, que N-2 manda activar con `Dematerialize`.
- **Por qué importa:** la v2 declara la extensión como «extensión conservadora de P5». En realidad reabre una disposición ya aceptada (N-2 de F4-T3), con una justificación que el código desmiente. El caso `Undecidable` es alcanzable con un comparador que lanza, y el impacto práctico es pequeño, pero una regla vinculante no puede ampliarse por analogía sin decisión.
- **Recomendación:** volver a la letra de P5 y N-2: retener si `PendingText != null`, o si `GetField` lanza (solo pasa con dobles; el host nunca lanza). Para la vista del `catch` del host no hace falta retener:
  - `Dematerialize` es no bloqueante y responde `Kept`;
  - el pendiente, si lo hay, sigue en el borrador y vuelve al regresar a la pestaña;
  - no se pierde nada.

  Si se quiere conservar algún margen, limitarlo a lo que el puerto marca como ilegible y declararlo como pregunta al Owner, no como extensión. Hay que ajustar en consecuencia la prueba «la vista ilegible retiene» de §4.

### N-2 (MENOR). «Las de los avisos» en la purga puede borrar los avisos de pérdida

- **Dónde:** paquete D-T3B2-04: «la comprobación va en el mismo bucle que `PurgeDeadSessions`, sobre las sesiones conocidas y las de los avisos»; `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:265-269`.
- **Qué está mal:** la sesión de un aviso de pérdida está muerta por definición. Si el bucle recorre las sesiones de todos los avisos y quita los de sesiones muertas, cada aviso de pérdida desaparece en la presentación siguiente a su consumo. La lista de «Se quitan» sí distingue los tipos, pero la frase del bucle no.
- **Recomendación:** escribir «las de los avisos **de veto**», y añadir a §4 una prueba: un aviso de pérdida sobrevive a varias presentaciones y solo se quita con «Entendido».

### N-3 (MENOR). El texto del veto pide «aplícalos antes de cerrar», algo que en T3 no existe

- **Dónde:**
  - Paquete: D-T3B2-04, «Descártalos con "Descartar borradores de este documento" o aplícalos antes de cerrar».
  - `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t3-authorization.md:70-71` («T3 does NOT own authored mutation. No Actualizar write path yet»).
  - `docs/initiatives/I-64-proposal-v5.md:608` (el veto nombra las vías de **descarte**).
- **Qué está mal:** en T3 no hay ninguna vía para «aplicar» un borrador al dibujo. Comprometer el pendiente (Enter) deja el borrador dirty, y sigue vetando el cierre. El aviso indica una acción que el usuario no puede hacer.
- **Recomendación:** quitar «o aplícalos antes de cerrar» (Actualizar llegará con T4) y dejar solo las vías de descarte.

### N-4 (MENOR). La contención y el cierre de la pestaña activa limpia no dicen en qué orden van

- **Dónde:** paquete D-T3B2-01 («Llama a `DiscardRack` y siempre se cierra. Si el descarte… lanza, el navegador avisa») y D-T3B2-07, contención («Si el rack afectado es el de la superficie viva, además `Field.Fail`»); `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:138-151` y `:299-306`.
- **Qué está mal:** al cerrar la pestaña activa limpia con un `DiscardRack` que lanza, la regla general pide `Field.Fail`, porque el rack afectado es la superficie viva, y la de cierre pide cerrar.
  - Si `Fail` va después del cierre, pone en Error el campo de la **nueva** superficie.
  - Si va antes, el `Show` de la nueva superficie lo borra (`ResetSurfaceState`).
- **Recomendación:** precisar que en `CloseTab` una excepción del descarte de una pestaña limpia solo deja el aviso del navegador, sin `Field.Fail`; o fijar «`Fail` antes de cerrar». Añadir la prueba con la pestaña activa.

### N-5 (MENOR). El aviso del navegador puede quedar desfasado, y el aviso de la retención no cubre una re-selección real

- **Dónde:** paquete D-T3B2-07, vida de `NoticeText` («hasta el siguiente gesto de T3B2 o hasta un cambio de la sesión presentada»), y D-T3B2-05 («nuevo para el VM (distinto del último que avisó)»); `src/RackCad.Application/Workspace/BrowserSession.cs:83-84`.
- **Qué está mal:**
  1. **Avisos que no caducan:** los gestos de T3B1 (activar o fijar una pestaña, «Editar», Enter) y las selecciones sin retención no borran el aviso. «La pestaña activa conserva su texto pendiente» o «su borrador limpio quedó en memoria» siguen visibles cuando ya no son ciertos, hasta un gesto de T3B2.
  2. **Re-selección sin aviso:** con «distinto del último que avisó», la secuencia selección `R` (retenida, avisa), selección vacía, selección `R` de nuevo es una transición real para `BrowserSession`, que aplica de nuevo porque el contexto cambió, y no avisa, porque coincide con el último aviso.
- **Recomendación:**
  - Que cualquier gesto explícito del navegador y cualquier presentación que cambie la pestaña activa borren el aviso.
  - Decidir el aviso de retención comparando con el contexto anterior **de esa sesión** que el VM pasó (el mismo criterio que `_lastContext`), no con el último aviso. La API de `BrowserSession` no puede crecer (G-01), así que el VM guarda su propio último contexto por sesión.

### N-6 (MENOR). Dos imprecisiones en las tablas de trampas

- **Dónde:**
  - **G-09:** «Las clases nuevas en el espacio de nombres `RackCad.UI.Tests.Workspace`». Los 176 archivos de `tests/RackCad.UI.Tests` usan `namespace RackCad.UI.Tests` (por ejemplo, `WorkspacePilotFieldViewModelTests.cs:9`, `WorkspacePilotFieldViewTests.cs:15`, `WorkspacePilotEditPortDouble.cs:7`). El filtro las selecciona porque el nombre de la clase empieza por `Workspace` (H-09 de `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:44`).
  - **G-02:** dice que `WorkspaceEditIntentGuardTests.cs:716-745` lee `BrowserSession.cs` «con comentarios», pero usa `Src.Code`, sin comentarios (`:735`). Solo `WorkspaceDraftPilotIntentGuardTests.cs:203-212` usa `Src.Text`.
- **Por qué importa:**
  - El espacio de nombres propuesto también entra en el filtro y no rompe la compilación: la búsqueda de nombres simples encuentra antes el método `Workspace(...)` de `tests/RackCad.UI.Tests/ProjectVariablesWindowTests.cs:75`, y el doble `internal` es accesible desde un espacio anidado. Pero crea un espacio de nombres `Workspace` nuevo, contra la convención del proyecto, y un Worker que copie la tabla al pie de la letra la rompe.
  - La imprecisión de G-02 es inocua: pide más neutralidad de la necesaria.
- **Recomendación:** G-09: «en el espacio de nombres `RackCad.UI.Tests`, con un nombre de clase que empieza por `Workspace`». G-02: corregir la nota.

### N-7 (MENOR). Dos detalles para que las guardas de §4 sean deterministas

- **Dónde:** paquete §4 («`TakeNotices(` solo en el cuerpo de `Present`»; «`DiscardDocument` solo en `ConfirmDiscard`») frente a los nombres de D-T3B2-07.
- **Qué está mal:**
  - **La declaración de `TakeNotices`:** `private void TakeNotices()` contiene el texto `TakeNotices(`, así que una guarda literal la cuenta fuera de `Present`.
  - **Nombres que contienen `DiscardDocument`:** varios nombres de la v2 contienen la subcadena `DiscardDocument`: `CanDiscardDocument` (VM y record), `BrowserDiscardDocumentButton`, `OnBrowserDiscardDocumentClick` y `OnBrowserNoticeDiscardDocumentClick`. Una guarda por subcadena daría falsos positivos.
- **Recomendación:** fijar que las guardas buscan **llamadas**, sobre el código sin comentarios (`Src.Code`):
  - `\.(?:DiscardRack|DiscardDocument|GetRacksAtRisk|TakeCloseNotices|TakeLossNotices)\s*\(` para las operaciones del puerto;
  - `(?<!void\s)\bTakeNotices\s*\(` (o excluir la declaración) para el método privado.

  Cada una con su autoprueba sobre esos nombres.

---

## 3. Respuestas a los puntos de §6

1. **¿Resuelve §7 los cinco IMPORTANTE?** Sí, los cinco quedan resueltos (tabla del §1).
2. **¿Bastan la API y la vista de D-T3B2-07?** Sí. Las guardas de §4 se pueden escribir sobre `CloseTab`, `DiscardField`, `RequestDocumentDiscard`, `ConfirmDiscard`, `TakeNotices` y `Present`, y las pruebas de vista sobre los nombres de la tabla. Con las precisiones de N-7 son deterministas. Nada de lo nuevo choca con las guardas existentes:
   - el record nuevo no toca el de pestaña (`WorkspaceBrowserHostGuardTests.cs:276`; `WorkspaceBrowserViewModelTests.cs:760-786`);
   - los nombres nuevos no caen en los regex de miembros de T3B1 (`WorkspacePilotSurfaceGuardTests.cs:56-75`);
   - el `ItemsControl` nuevo no lleva `BrowserSurface` en su etiqueta (`WorkspaceBrowserHostGuardTests.cs:394`);
   - el primer `<Border` tras `BrowserTabStrip` sigue siendo el de la plantilla (`:285-298`);
   - las pruebas de vista solo cuentan botones dentro de `BrowserTabStrip` (`WorkspaceBrowserViewTests.cs:62-66`, `:134`, `:421`);
   - el recuento de suscriptores del navegador (`WorkspaceBrowserScenarioHarnessTests.cs:149-166`) no cambia: sin puerto no hay filas, y la medida es relativa.
3. **¿Es satisfacible la regla RED?** Sí, por análisis.
   - Con el esqueleto inerte (`CloseTab` el de hoy, `keepActive` ignorado, propiedades vacías, `ShowDiscardButton` falso, XAML enlazado a valores vacíos), el doble ampliado, que conserva sus respuestas por defecto y solo añade respondedores, colas y `ThrowIfAsked` en operaciones que ninguna prueba hace lanzar, y P-01, las 2300 de Core y las 352 de UI siguen pasando.
   - [si (c)] N-1 en el RED solo afecta a `T06_…`, que cambia en el mismo commit (P-05).
   - Fallan solo las nuevas:
     - Core: las ramas de `keepActive` y la guarda de presencia del parámetro;
     - UI: los comportamientos nuevos.
   - Es satisfacible si esas pruebas fallan por aserción y no por excepción (por ejemplo, `Assert.Single` en lugar de `.First()` sobre listas vacías).
4. **¿Es aceptable la extensión de `keepActive`?** Tal como está, no (N-1): solo la vista del `catch` tiene el pendiente desconocido, y retener por `Undecidable` o por un `Error` explícito contradice N-2.
5. **¿Rompe la v2 algo no declarado?** No he encontrado ninguna prueba ni guarda existente que la v2 rompa fuera de P-01 y P-05. Las trampas que ya lista G-01 a G-09 son correctas, salvo las dos imprecisiones de N-6.

---

## 4. Lo verificado y correcto en la v2

**Hechos de §2:**
- H-01 a H-08 y P-01 a P-05 coinciden con el código.
- P-04 se mantiene también con la extensión de `keepActive`: ninguna prueba de T3B1 presenta otro rack de la misma sesión con la pestaña activa en `Error` o `Undecidable`. `WhenThePortCannotSay…` (`WorkspacePilotFieldViewModelTests.cs:1298-1309`) lanza en `GetField`, pero su primera presentación no tiene pestañas.

**Decisiones:**
- La tabla de cierre, la desviación de M-1 y la confirmación con su vida y su contención son coherentes con D-08, R-B2-1 y R-B7.
- El descarte del documento con la TOCTOU ordinal es coherente con R-B2-2, y el host ordena las listas (`src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs:186-195`).
- Los avisos (consumo solo en `TakeNotices` desde `Present`, clasificación al mostrar, filas estables) son coherentes con R-B2-3.
- N-3 solo con `e.Key` y `e.IsRepeat` respeta G-05 y la prueba sin `KeyUp` (`WorkspacePilotFieldViewTests.cs:480-492`).

**Contrato:**
- El alcance de §5 cubre todo lo que las decisiones tocan, incluida la documentación de `WorkspacePanelViewModel.cs:62-65`. G-08 lo admite porque `Src.Code` no lee comentarios.
- Las paradas incluyen cualquier cambio de prueba distinto de P-01 y P-05.
