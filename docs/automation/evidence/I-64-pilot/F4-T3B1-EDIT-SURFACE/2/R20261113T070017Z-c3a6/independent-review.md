# Re-revisión adversarial independiente: F4-T3B1-EDIT-SURFACE, intento 2 (corrección)

- **Worktree:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`.
- **Rango:** `1c69fbc4..1708e764`, con RED `40f5727c` y GREEN `1708e764`. Son 9 archivos.
  - El RED toca solo 5 archivos de `tests/`.
  - El GREEN toca solo 4 de `src/RackCad.UI/Workspace/`: el VM del navegador, el VM del campo, el XAML y el code-behind.
- **Autoridades leídas desde el repositorio:**
  - `gate-contract-attempt2.json`: las CORRECCION de INV-F4T3B1-03, 04, 06, 07, 09, 11 y 14, y el nuevo INV-F4T3B1-15;
  - la disposición `1/R20261113T061247Z-2130/analysis.md`;
  - el experimento `keyboard-experiment/` (`Program.cs` y `result.txt`);
  - mi informe del intento 1.
- **Método:** solo lectura, con `git show`, `git diff`, `git grep`, `git ls-tree` y lectura de archivos.
  - No ejecuté build ni pruebas, no verifiqué la CI y no abrí AutoCAD.
  - Que una prueba pasa o falla contra `5de2fab3` o contra `1708e764` lo deduzco de leer su código.

## Veredicto

**ACEPTABLE CON MENORES:** 0 BLOQUEANTE, 0 IMPORTANTE y 3 MENOR.

- El bloqueante del intento 1 queda resuelto, y su prueba nueva discrimina.
- Los menores 2, 3, 4, 5, 7 y 8 y la marca conservadora quedan cerrados como dicen las CORRECCION.
- No hay cambios fuera de lo declarado, y no se rompe nada de lo que el intento 1 hacía bien.
- Los tres menores nuevos son de evidencia y de un caso límite del teclado. Ninguno toca el oráculo ni la regla de edición.

## Comprobación del bloqueante (INV-F4T3B1-04 CORRECCION, INV-F4T3B1-15)

- **Ningún manejador de `KeyDown` de burbuja.** `WorkspacePanelView.xaml` ya no tiene `KeyDown="…"` en la caja, y `OnBrowserFieldBoxKeyDown` ha desaparecido del code-behind. En `src/RackCad.UI/Workspace/` no queda ningún manejador de `KeyDown`, `KeyUp`, `TextInput` ni `PreviewTextInput`; solo el `PreviewKeyDown` de la caja.
- **Caja editable:** el `PreviewKeyDown` marca solo Enter (con `CommitPending`) y Esc. Cualquier otra tecla vuelve sin `Handled`, así que una tecla de carácter llega al `TextBox` y produce su `TextInput`. El experimento de la sesión, con `PostMessage` de `WM_KEYDOWN`, confirma el mecanismo.
- **Caja en solo lectura:**
  - toda tecla que no sea un modificador, Tab o Esc queda `Handled` antes de cualquier otra comprobación, así que no se aplica ni sale del panel;
  - el gesto se lanza solo si la tecla no es una repetición (`!e.IsRepeat`) y si `CanOpenByGesture`;
  - Esc queda `Handled` sin gesto.
- **Las pruebas discriminan:**
  - `ACharacterKeyOnAnEditableBoxIsNotHandledInTheTunnelNorInTheBubbleSoItProducesItsTextInput`, con A, D5, OemComma y OemPeriod: contra `5de2fab3`, el `Bubble(box, A).Handled` vale true por el manejador antiguo y la prueba falla; contra `1708e764` pasa.
  - `TheBoxHasNoHandlerOfTheBubbleKeyDownAttachedToIt` cuenta, por reflexión sobre `EventHandlersStore`, los manejadores de instancia de la caja y de `BrowserFieldPanel`. Contra `5de2fab3` encuentra 1 y falla.
  - `AnEditableBoxAnswersEveryKeyExactlyAsAPlainTextBoxDoes…` compara la caja con un `TextBox` plano. Como `RaiseEvent` recorre todo el árbol de la vista, también detectaría un manejador nuevo en un ancestro (la superficie o el control raíz) que marcara las teclas.

## Los menores del intento 1 y la marca conservadora

| Id | Estado | Comprobación |
|---|---|---|
| 2 | Cerrado | `ThisTaskAddsNoFile…` nombra los 5 archivos de `Drafts/` y los 9 de `Plugin/Workspace` como cota **inferior** (`MissingFiles`), sin techo, con una autoprueba de cada dirección: falta uno o sobra uno. Los nombres coinciden con `git ls-tree`. Que esta tarea no añade archivos lo siguen fijando otras guardas (`WorkspaceBrowserHostGuardTests.cs:38`, `WorkspaceLifecycleGuardTests.cs:794` y `WorkspaceDraftBoundaryTests`) |
| 3 | Cerrado | `TryCheckNow` solo queda prohibido en los cuerpos no gestuales y en los 7 gestos de T3B1; fuera de ellos, solo se prohíben `TryBeginEdit` y `TryReload`. Hay autopruebas que muestran que un `CheckNow` futuro no cae. La guarda ya no cuenta los `<TextBox` (localiza la caja por su nombre) ni exige `KeyDown`. La guarda de la paleta queda en negativo, con un `_controller.PilotEdit` y ningún tipo de puerto, y la autoprueba confirma que otros miembros no caen. La prueba de vista de la caja única filtra por nombre |
| 4 | Cerrado | El clic y la tecla pasan por `BeginFieldEditFromBox`, y el doble clic exige además `CanOpenByGesture` (`_showEdit && !_showReload && _failure == null && _diagnostic == null`). Con la caja editable, `_showEdit` es false y el doble clic no hace nada. Con un `NotAdmitted` vigente, solo «Editar» vuelve a leer. Con un Error, del borrador o un fallo, ningún gesto lee y solo «Recargar» actúa. `IsRepeat` no lanza nada. Las pruebas de VM y de vista de cada caso fallarían contra `5de2fab3` |
| 5 | Cerrado | Prueba 9: el gancho comprueba la fijación también con el clic y la tecla. El doble clic abortado parte de una preview y afirma que sigue siéndolo (VM y vista). El doble aplica y limpia el pendiente de un `Proceeded`. `Dematerialize` parte de una pestaña solo dirty (Dirty sin pendiente, «Nombre R-1 *»). La reentrada se prueba en la misma sesión cambiando solo de rack, y discrimina por el aviso. La prueba 16 edita. El mapa evidencia-por-invariante no está en el rango y no lo pude comprobar |
| 7 | Cerrado | T15 admite `TakeLossNotices` y `TakeCloseNotices`. La prueba del panel afirma solo que no hay lecturas ni mutaciones (`SetPendingText`, `CommitField`, `DiscardRack` y `DiscardDocument`) |
| 8 | Cerrado | El `catch` de `CrossObservedBoundaries` contiene el fallo sin tocar el campo nuevo, con dos pruebas: `DocumentChange` y `Dematerialize` |
| Marca conservadora | Cerrado | `IsProtected` devuelve true si `GetField` lanza, y la prueba lo confirma: «*» y la preview fijada, sin lecturas |
| 6 | A F4-T3C | Lo declara INV-F4T3B1-14 CORRECCION; sin cambio en T3B1 |

## Alcance y cambios declarados

- **Los 9 archivos están dentro del AllowedWriteScope de 19 rutas.**
- **El GREEN no toca `tests/`.**
- **El RED no toca `src/`.** Las pruebas compilan contra `5de2fab3`: no nombran `BeginFieldEditFromBox` ni `CanOpenByGesture`, salvo dentro de una cadena de la guarda.
- **Las guardas de Core pasan en el RED.** Las re-acotadas aceptan también el producto de `5de2fab3`.
- **El único archivo existente que cambia es `WorkspacePilotEditGuardTests.cs`, y solo en la región del menor 2:** la guarda y su autoprueba. Los demás (`WorkspacePilotSurfaceGuardTests.cs`, el doble y las dos pruebas de UI) los creó el intento 1, así que no cuentan como existentes (F4T3B1-C02).

## Lo que no se rompe (intento 1)

- **Oráculo:** las lecturas siguen solo en `Field.BeginEdit`, alcanzado únicamente desde los gestos: «Editar», `FromBox` y `FromTab`.
- **Sin cambios:**
  - `Present`, `Show` y `RebuildTabs`;
  - el binding, el setter y `CommitPending`;
  - `TabChange` y `AllowTabChange`;
  - las fronteras no bloqueantes, salvo el `catch`;
  - «Nombre *», salvo la rama de excepción.
- **Sin nada nuevo:** ni `+=`, ni eventos, ni planificación, ni API de foco.

## Hallazgos

### 1. MENOR — El doble aplica el pendiente de un borrador **no editable** en una frontera bloqueante, y la mitad «Kept» de T06 no prueba `Kept`

**Dónde.**
- `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`: en la rama bloqueante de `CrossBoundary` (alrededor de `:183-193`), un pendiente válido se aplica y se responde `Proceeded` sin mirar `view.CanEdit`.
- `WorkspacePilotFieldViewModelTests.cs`, segunda mitad de `T06_ComingBackToTheTabShowsTheExactTextOfTheDraftAfterAProceededCrossingAndAfterAKeptOne`.

**Problema.**
- El host responde `Kept` ante un borrador no editable con pendiente, que se conserva sin aplicar (D-T3-02, `DraftSession.cs:370-371`).
- En la prueba, R-1 está en Unknown, con `canEdit: false` y pendiente « 1:7 ». Como no hay `BoundaryResponder`, el doble responde `Proceeded` y convierte el pendiente en comprometido. La aserción final, `Field.Text == " 1:7 "`, se cumple igual porque el texto mostrado coincide.
- Así, la mitad que su nombre y su comentario presentan como «through a Kept crossing» nunca recorre `Kept`.
- El cambio del doble, pedido para el `Proceeded` (INV-F4T3B1-15), introduce una divergencia nueva con el host: muta un borrador que el host no tocaría.

**Por qué es MENOR.** El manejo de `Kept` en la UI sí lo prueba, con un `BoundaryResponder` explícito, `T05_ActivatingAnotherTabFromADraftThatCannotBeEditedWithPendingTextKeepsAndTheChangeProceeds`.

**Recomendación.** Que el doble responda `Kept` sin mutar cuando `!view.CanEdit`, o que esa mitad de T06 use un responder `Kept` y afirme que el pendiente sigue en el borrador.

### 2. MENOR — Space no se prueba como pide la letra de INV-F4T3B1-15

**Problema.**
- INV-F4T3B1-15 pide que «el KeyDown de A, D5, Space, OemComma y OemPeriod queda sin Handled».
- La teoría que lo afirma (`ACharacterKeyOnAnEditableBox…`) no incluye Space. Space solo aparece en la comparación con un `TextBox` plano, cuyo comentario dice que el propio editor consume Space en su manejador de clase.
- En sustancia es correcto: si el `TextBox` mismo marca Space, la letra del contrato no se puede cumplir, y la igualdad con un `TextBox` plano es la afirmación adecuada. Aun así, es una desviación de la letra de un invariante vinculante.

**Recomendación.** Que el handoff o el mapa evidencia-por-invariante la declaren. Si ya lo hacen, queda cerrado.

### 3. MENOR — La repetición de la tecla que abrió la caja se escribe en ella

**Dónde.** `WorkspacePanelView.xaml.cs:94-128`.

**Escenario.**
1. El usuario mantiene pulsado `5` sobre la caja en solo lectura.
2. La primera pulsación se marca, lanza el gesto y, si la intención se admite, la caja queda editable sin aplicar la tecla.
3. Las repeticiones automáticas de esa misma tecla (`IsRepeat`) llegan ya a la rama editable: no se marcan, producen su `TextInput` y escriben «5…» por `SetPendingText`.

**Por qué es nuevo.** El comportamiento aparece con la corrección: en el intento 1, el `KeyDown` de burbuja lo tapaba sin querer.

**Por qué es MENOR.**
- D-T3-04 y D-T3B1-03 dicen que el gesto que dispara la intención no se aplica. Con una tecla mantenida, parte de esa misma pulsación física sí se aplica.
- No hay pérdida ni texto fuera del borrador: el invariante del texto vivo se cumple y la pestaña ya está fijada.
- Es un caso límite de UX. Basta con observarlo en Smoke-2 o, si se quiere, ignorar las repeticiones hasta el `KeyUp` de la tecla que abrió la caja.

## Observaciones sin hallazgo

- **Nombre de la guarda del menor 2.** `ThisTaskAddsNoFileToThePluginWorkspaceFolderNorToTheDrafts…` ya no detecta archivos añadidos, porque ahora es una cota inferior. Su nombre describe algo que fijan otras guardas.
- **El `catch` de `CrossObservedBoundaries`.** Ahora el fallo no deja rastro visible. Es lo que pide la CORRECCION, y con el puerto real es inalcanzable.
- **La guarda contra un `KeyDown` futuro en la caja.** Es una prueba de UI (reflexión y comparación con un `TextBox` plano), no una guarda textual de Core. Las dos suites corren en la CI, así que basta.
- **`GetRacksAtRisk` en T15.** La prueba sigue afirmando que no se llama a `GetRacksAtRisk` en un escenario con presentaciones. El §6 lo prevé solo desde un gesto (R-B8), así que hoy no choca con T3B2.

## Riesgos para el Smoke-2 con AutoCAD

1. **D-01, ahora sin la red del `KeyDown` de burbuja.** Lo declara la CORRECCION de INV-F4T3B1-04. Hay que verificar en el host qué hacen las teclas que el `TextBox` no consume con la caja editable:
   - Ctrl+Z o Ctrl+Y sin nada que deshacer;
   - Ctrl+V sin texto en el portapapeles;
   - las teclas F, Supr al final del texto y Alt o F10.

   Si alguna llega a AutoCAD (UNDO, ERASE, PASTECLIP), sería un cambio del DWG nacido de una tecla del panel. Hay que mirarlo con `KeepFocus` y con el preproceso de la paleta.
2. **El teclado de punta a punta.** Teclear un dígito, una coma o un punto en la caja editable y confirmar `SetPendingText` y «Nombre *». Es la cobertura 1-3 con el teclado real.
3. **La tecla mantenida** del hallazgo 3.
4. **Los demás riesgos del intento 1 siguen vigentes:**
   - el foco tras «Editar» o «Recargar», que se colapsan, y tras el doble clic;
   - el clic de pestaña perdido tras un compromiso que cambia «*»;
   - el doble clic sobre una pestaña no activa, cuando la tira se regenera;
   - la reentrada de `ReadIntent` dentro de los manejadores de ratón o de teclado;
   - los textos del host en inglés, ya en F4-T3C.
