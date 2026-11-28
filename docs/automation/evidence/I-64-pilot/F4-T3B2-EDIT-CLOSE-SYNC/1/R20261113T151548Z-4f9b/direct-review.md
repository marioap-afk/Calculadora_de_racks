# Revisión directa de la sesión: F4-T3B2-EDIT-CLOSE-SYNC, intento 1

Rango: `b5ae014d..23f67f56`. RED `c3de6116` y GREEN `23f67f56`. Worker `R20261113T124709Z-b033`.

**Veredicto: producto conforme**, con dos notas.

## Comprobado

- **Alcance:** exactamente las 13 rutas del contrato. El GREEN no toca `tests/`: `git diff c3de6116 23f67f56` solo cambia `src/`.
- **RED:** CI `43436204184`, con Core y UI en failure.
  - Fallan 11 pruebas del filtro de Core y 64 del de UI, y ninguna fuera del filtro.
  - Las 75 son métodos añadidos en el RED (110 métodos nuevos); ninguna prueba existente falla.
- **GREEN:** CI `43440641847`, con los cuatro jobs en success.
  - Filtros: 2331/2331 en Core y 431/431 en UI.
  - Suites completas: Core 17869/17869; UI 2068 pasan, 0 fallan y 17 no se ejecutan, que son las condicionales ya existentes.
- **`BrowserSession`:**
  - `ApplySelectionContext(SelectionContext context, bool keepActive = false)`, sin sobrecarga ni valores nuevos de `BrowserChange`;
  - con `keepActive` y pestaña activa: `R` abierta da `None`; una preview que no es la activa se reemplaza (`Replaced`); la preview activa nunca se reemplaza (`None`); sin preview se añade detrás (`Created`). El contexto se consume como antes.
- **Cierre (D-T3B2-01):**
  - sin borrador, `CloseNow` sin descarte;
  - protegida, o `GetField` que lanza, abre la confirmación en línea;
  - limpia: `DiscardRack` y se cierra siempre; si la respuesta no es `Discarded` o lanza, avisa que el borrador limpio quedó en memoria.
- **Confirmación:** «Descartar» llama a `DiscardRack` con la sesión y el rack de la confirmación y solo cierra con `Discarded`. «Cancelar» no llama al puerto. La confirmación se anula si cambia la sesión, si su pestaña desaparece o, la del campo, si cambia la superficie.
- **Descartar del campo (D-T3B2-02):** protegida, con confirmación; no protegida, directo.
  - Con `Discarded`, `Rebuild` y `Field.Rematerialize()`, que pinta desde la memoria, sin lectura de la intención.
  - `ShowDiscardButton` = `ShowReloadButton` y el borrador mostrado existe.
- **Documento (D-T3B2-03):**
  - `RequestDocumentDiscard` lee `GetRacksAtRisk`; si las dos listas están vacías, aviso sin efecto.
  - `ConfirmDiscard` vuelve a leer y compara las dos listas en orden y por ordinal (`SameList`). Si cambiaron, no descarta y muestra la lista nueva con su aviso; si quedó vacía, quita la confirmación con el aviso de «no hay borradores en riesgo».
  - Con `Discarded`, quita el veto de la sesión y el aviso separa los confirmados de los limpios retirados. Las pestañas no se cierran.
- **Avisos (D-T3B2-04):**
  - `TakeCloseNotices` y `TakeLossNotices` solo en el privado `TakeNotices`, cada una en su `try`, llamado solo desde `Present` y antes de `PurgeDeadSessions`;
  - un veto por sesión, que el nuevo sustituye en su sitio;
  - la purga del veto va en el mismo bucle que la de las sesiones, con una sola llamada textual a `IsSessionAlive`; la pérdida solo se quita con «Entendido»;
  - las filas son estables por valor y por índice;
  - el texto del veto nombra solo la vía del panel, sin ninguna vía de aplicar.
- **Retención (D-T3B2-05):** `KeepsActive` es verdadero solo con `PendingText != null` o si `GetField` lanza.
  - El aviso sale solo con un contexto distinto del anterior de esa sesión (`_lastContexts`) y con `R` distinto de la activa, con los dos textos del paquete.
- **UI (D-T3B2-07):**
  - los miembros públicos, el record `WorkspaceBrowserNoticeItem` y los nombres del XAML y de automatización son los del paquete;
  - los elementos nuevos van entre la tira y la superficie, fuera de `BrowserTabStrip`, sin tamaños fijos ni `ContentControl`;
  - ningún botón nuevo lleva `IsDefault` ni `IsCancel`;
  - los manejadores nuevos delegan dentro de `Contained`;
  - `Field.Fail` solo si la pestaña sigue abierta y es la superficie viva (`FailFieldIfLive`), nunca `ContainFailure` ni el Error del panel.
- **N-1:** en `T06_…`, la mitad `Kept` afirma ahora que el cruce no muta y que el pendiente sigue en el borrador (P-05). Es el único cambio de `WorkspacePilotFieldViewModelTests.cs`.
- **N-3:** `_launchKey` usa solo `e.Key` y `e.IsRepeat` en `PreviewKeyDown`, sin `KeyUp`.
- **P-01:** el único cambio de `WorkspacePilotFieldViewTests.cs` es la aserción que admite `LossOp` y `CloseOp`.
- **Oráculo crítico:** el VM del navegador no nombra `TryBeginEdit`, `TryReload` ni `TryCheckNow`. Cerrar y descartar solo llaman a `GetField`, `DiscardRack`, `GetRacksAtRisk` y `DiscardDocument`, todas de memoria. No hay `+=`, eventos, `Dispatcher`, temporizadores, tareas ni API de foco nuevos.

## Notas

- **D-1, a F4-T3C:** la guarda `TheQueuesOfNoticesAreNamedInTheHostSideOnlyByThePortTheHostAndTheTwoForwarders` fija los cuatro archivos del host que pueden nombrar las colas (M-11 del paquete). Si F4-T3C necesita nombrarlas en otro archivo, por ejemplo en el comando `RACKPANELDESCARTAR`, debe declararlo como cambio de esa guarda. La de los miembros públicos solo exige que cada uno exista una vez y no impide añadir otros.
- **D-2, menor:** si el primer gesto por tecla no llega a abrir la caja, `_launchKey` se conserva hasta la siguiente pulsación no repetida. Una repetición de esa tecla, si la caja se abre después por el botón, quedaría marcada una vez. Es inocuo; cae en la reserva de Smoke-2 de N-3.

**Riesgos para Smoke-2:** el compromiso por `LostFocus` antes de «Cerrar», el clic perdido de la tira, el foco tras la confirmación en línea y el veto sin `StateChanged`. Son las limitaciones declaradas en INV-F4T3B2-15.
