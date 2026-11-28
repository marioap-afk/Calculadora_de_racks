# I-64 — Paquete del contrato de F4-T3B2-EDIT-CLOSE-SYNC (cierre, descarte, avisos y retención del panel)

**Estado:** v3, tras la revisión adversarial independiente de la v1 (EMITIBLE TRAS CORRECCIONES, 0/5/12; disposición en §7) y la re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/1/6, los 17 de la v1 resueltos; disposición en §8), con la disposición del Owner y del Coordinator incorporada (F4-E43, §9). Por el precedente de F4-T3B (v3 sin tercera revisión), queda lista para emitir. Preparado sobre el código de F4-T3B1 en `1708e764`. F4-T3B1 está CLOSED / PASS WITH LIMITATIONS (F4-E43). La disposición autoriza este paquete y su emisión sin otra decisión del Owner; F4-T3B2 empieza en 1 de 3.

**Base:** la rama `architecture/workspace-persistente-rackcad` tras la custodia del cierre de F4-T3B1 (`a8639489`); `main` en `812baf87`. Desde `1708e764`, solo documentación y evidencia.

## 1. Autoridades

- **Órdenes del Owner** (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/`):
  - `owner-f4t3-authorization.md`: la cobertura mínima 11-14; «Known clean tab: close without authored write» (`:140-141`); protegida: exactamente Discard o Cancel; el descarte explícito es de sesión y rack, sin escritura y sin elegir en silencio un borrador ambiguo; cerrar nunca hace Actualizar;
  - `owner-f4t3b-decomposition-decision.md`: T3B2-EDIT-CLOSE-SYNC tras T3B1, con 1 de 3 intentos;
  - `owner-f4t3a-close-decision.md`: los menores 4 y 6 de T3A siguen en T3C;
  - `owner-overnight-autonomy-2026-11-13.md` y las decisiones delegadas;
  - `owner-f4t3b1-close-disposition.md` (F4-E43): el cierre de F4-T3B1, la autorización de este paquete, el alcance aceptado, el tratamiento aceptado en el contrato (puntos 1-3), N-1 y N-3 vinculados, N-2 declarada, la regla de replanificación y lo que es de F4-T3C (§9).
- **Paquete de F4-T3B** (`docs/initiatives/I-64-f4-t3b-gate-contract-package.md` v3, §6): R-B2-1 a R-B2-6.
- **Paquete de F4-T3** (§7): R-B6 a R-B9, y R-C2 para T3C.
- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`): D-08 (`:390`: sin dirty se cierra sin preguntar; con borrador, Descartar o Cancelar; Error con Recargar o Descartar), D-17 (`:603-612`: el veto y sus dos vías de descarte), P5 `:335-336`.
- **F4-T3B1:** su código en `1708e764`, la re-revisión independiente (N-1, N-2 y N-3) y su paquete de cierre (`F4-T3B1-EDIT-SURFACE/task-close-package.md`, §5). N-1 y N-3 son de esta subtarea por la disposición del Owner (F4-E43); en las versiones anteriores llevaban la marca «si (c)».

## 2. Hechos medidos en `1708e764`

### 2.1 El código que T3B2 cambia o consume

| Id | Hecho | Consecuencia |
|---|---|---|
| H-01 | `WorkspaceBrowserViewModel.CloseTab` (`:171`) es `Interact(s => s.Close(rack))`. `BrowserSession.Close` (`BrowserSession.cs:140-156`) pasa a la siguiente o a la anterior | El cierre cambia en el VM, con el puerto de edición; `BrowserSession.Close` no cambia |
| H-02 | Sin puerto de edición el navegador es el de antes. Las pruebas del navegador sin puerto (`WorkspaceBrowserViewModelTests.cs`, `WorkspaceBrowserViewTests.cs:29`) cierran sin preguntar | Sin puerto, nada cambia (INV-F4T3B2-01) |
| H-03 | `IsProtected` (`WorkspaceBrowserViewModel.cs:524-538`) da `true` ante un fallo. `RebuildTabs` (`:490-522`) marca « *» y fija la preview protegida | El cierre clasifica con la misma regla |
| H-04 | `ApplySelectionContext` (`BrowserSession.cs:78-113`) activa `R` abierta, o crea o reemplaza la preview y la activa; `_lastContext` se guarda antes de mirar el tipo (`:83-84`) | `keepActive` |
| H-05 | El puerto en el host (`WorkspaceHostCore.cs`): `GetField` nunca lanza y su fallo es una vista `Exists`, `Error`, protegida, sin pendiente (`:595-607`); `DiscardRack` (`:722-744`: `Rejected` si la sesión no es la del documento activo, `Busy` con lectura en curso, `Discarded` con listas vacías sin borrador, quita el aviso de veto si la sesión queda sin riesgo); `DiscardDocument` (`:746-762`, devuelve en `RemovedRackIds` también los limpios); `GetRacksAtRisk` (`:765-807`, memoria, sin `Authorize`, su fallo es `default` en `:805`, listas ordenadas); `TakeLossNotices` (`:809-819`) y `TakeCloseNotices` (`:821-826`) vacían sus colas; `AdviseClose` (`:912-933`) encola a lo sumo un veto por sesión y no emite `StateChanged`; `SessionDestroyed` (`:946-965`) quita el veto y encola la pérdida si la hay | D-T3B2-01 a D-T3B2-05 |
| H-06 | El VM del panel llama a `Browser.Present` en su constructor (`WorkspacePanelViewModel.cs:72`) y en cada `Refresh` (`:228`). Su documentación de `pilotEdit` dice «Building the panel never calls it» (`:62-65`) | Consumir en `Present` hace falsa esa frase: se corrige (alcance) |
| H-07 | El doble de UI (`WorkspacePilotEditPortDouble.cs`): `DiscardRack` y `DiscardDocument` responden `Rejected` (`:199-209`), `GetRacksAtRisk` vacío (`:211-215`), colas vacías (`:217-227`), sin `ThrowIfAsked` en esas cinco; su `CrossBoundary` bloqueante aplica un pendiente válido sin mirar `CanEdit` (`:174-197`, N-1); cada operación se registra en `Calls` | El doble crece (D-T3B2-08) |
| H-08 | El code-behind (`WorkspacePanelView.xaml.cs:94-128`): en la caja editable solo Enter y Esc quedan marcados; una prueba fija que la caja no tiene manejador de `KeyUp` (`WorkspacePilotFieldViewTests.cs:488`). `LostFocus` compromete el pendiente (`:131`) | N-3 solo en `PreviewKeyDown`; el compromiso por `LostFocus` antes de «Cerrar» es una limitación declarada |

### 2.2 Pruebas existentes que T3B2 toca o respeta

| Id | Hecho | Consecuencia |
|---|---|---|
| P-01 | `OnAReadOnlyBoxEscIsHandledWithNoGestureAndAModifierOrTabLaunchesNothingAndIsNotHandled` (`WorkspacePilotFieldViewTests.cs:206-221`) afirma en `:218` que ninguna llamada al doble es distinta de `GetField`, tras un `Build()` (`:26-36`) que construye y refresca el panel sin vaciar las llamadas | **Cambio declarado:** esa aserción excluye `TakeLossNotices` y `TakeCloseNotices`, como `T15` (`WorkspacePilotFieldViewModelTests.cs:1133-1144`) |
| P-02 | `T10_AClosedTabOfACleanDraft…` (`WorkspacePilotFieldViewModelTests.cs:444-464`) cierra una pestaña limpia y espera que se cierre; con la regla de D-T3B2-01 se cierra aunque el doble responda `Rejected`, y al volver el borrador sigue (`AlreadyOpen`) | **No cambia** |
| P-03 | Ausencias que T3B2 respeta: un clic en la activa no llama a nada (`:724-738`); la presentación no lee ni muta y admite las colas (`:93-120`); cambiar de pestaña no descarta ni pide los riesgos y admite las colas (`T15`, `:1109-1154`); `T17` y `AssertNoReadAndNoMutation` admiten las colas | Colas solo en `Present`; descartes y riesgos solo en sus gestos |
| P-04 | Ninguna presentación de las pruebas de T3B1 ocurre con texto pendiente en la pestaña activa y un contexto nuevo de **otro** rack de la misma sesión: todas son de otra sesión, del mismo rack, de un contexto igual al último o sin pestañas (barrido de las dos clases; confirmado por la revisión) | `keepActive` no cambia ninguna prueba de T3B1 |
| P-05 | La mitad «Kept» de `T06_…` depende del doble actual (N-1, vinculado por F4-E43) | Cambio declarado de `T06_…` |

### 2.3 Trampas de guardas existentes (la ruta natural de implementación las rompe)

| Id | Guarda | Regla para el Worker |
|---|---|---|
| G-01 | La API pública de `BrowserSession` por **nombre** (`WorkspaceBrowserBoundaryTests.cs:176-197`); los enums exactos (`:169-173`); parámetros sin delegados ni interfaces (`:216-243`; `WorkspaceBrowserSessionTests.cs:258-270`) | Solo el parámetro `bool keepActive = false`; ninguna sobrecarga ni valor nuevo de `BrowserChange` |
| G-02 | El vocabulario de `BrowserSession.cs` (`WorkspaceBrowserBoundaryTests.cs:104-113`, sin distinguir mayúsculas): `Draft`, `Dirty`, `Editor`, `Subscription`, `Commit(`, `Transaction`, `Provider`, `Metric`… y (`:55-67`) `static`, `lock`, `Action`, `Func<`, `DateTime`…; ningún tipo de Workspace salvo `SessionId` y `SelectionContext` (`:83-101`; `SelectionContextKind` cae); además lo leen `WorkspaceEditIntentGuardTests.cs:716-745` (código, sin comentarios) y `WorkspaceDraftPilotIntentGuardTests.cs:184-212` (con comentarios) | Nombres y comentarios neutros («retener», «activa», «texto»); sin tipos nuevos |
| G-03 | El VM del navegador (`WorkspaceBrowserHostGuardTests.cs:244-277`): sin `+=` ni `-=` (también al componer textos), sin `event`, `delegate`, `Task`, `Dispatcher`, `\w*Timer`, `Thread`, `.Result`, `.Wait(`; ningún `static` salvo `static readonly string`; `.ReadSummary(` y `.IsSessionAlive(` **exactamente una vez**; `BrowserSessionRegistry` una vez; el record `WorkspaceBrowserTabItem` exacto | Textos con `a = a + b`; la purga de avisos en el mismo bucle de `PurgeDeadSessions` (D-T3B2-04) |
| G-04 | El VM del campo (`WorkspacePilotSurfaceGuardTests.cs:325-336`): sin `+=`, sin evento, sin estado estático | Igual |
| G-05 | La UI del espacio de trabajo (`WorkspacePilotSurfaceGuardTests.cs:293-323`; `WorkspacePaletteHostSourceGuardTests.cs:580-593`; `WorkspaceNavigationHostTests.cs:919`): sin `EnterPanelModal`, `MessageBox`, `ShowDialog`, `Window`, foco (`Keyboard`, `.Focus(`, `e.KeyboardDevice`), `Dispatcher`, temporizadores, `Registry` (con mayúscula), `RackLog` | La confirmación es en línea; N-3 usa solo `e.Key` y `e.IsRepeat` |
| G-06 | La superficie (`WorkspaceBrowserHostGuardTests.cs:325-345`): al menos 3 botones; exactamente un `Content="Editar"`, uno `"Recargar"` y uno `"Volver a leer"` | «Descartar» del campo cabe; ninguno de esos tres textos se repite en la superficie |
| G-07 | La sección del navegador (`WorkspaceBrowserHostGuardTests.cs:381-400`; `WorkspaceBrowserViewTests.cs:367-400`, `:431-447`): sin `<ContentControl`, sin tamaños fijos (`Width`, `Height`, `Min*`, `Max*`), sin ventana; la tira antes que la superficie y la superficie antes que la navegación; las pruebas cuentan los botones **dentro** de `BrowserTabStrip` | Los elementos nuevos van fuera de la tira, entre la tira y la superficie, sin tamaños fijos |
| G-08 | El VM del panel (`WorkspaceBrowserHostGuardTests.cs:222-235`): su firma y la construcción del navegador, exactas | Solo cambia el texto de la documentación de `pilotEdit` |
| G-09 | Las pruebas de UI se seleccionan con `FullyQualifiedName~RackCad.UI.Tests.Workspace`, y las de Core con `FullyQualifiedName~RackCad.Tests.Workspace` | Las clases nuevas en el espacio de nombres de cada proyecto (`RackCad.UI.Tests` y `RackCad.Tests`, la convención), con nombre de clase que empieza por `Workspace`, para que el filtro las seleccione |

## 3. Decisiones

### D-T3B2-01. Cerrar una pestaña (R-B2-1, D-08)

Sin puerto de edición, `CloseTab` es exactamente el de hoy. Con él, `CloseTab(rack)` clasifica la pestaña leyendo solo la memoria del puerto (`GetField` de la sesión presentada):

| La pestaña | Qué hace |
|---|---|
| Sin borrador (`Exists` falso) | Se cierra como hoy, sin llamar a ningún descarte |
| Con borrador no protegido | Llama a `DiscardRack` y **siempre** se cierra. Si el descarte no da `Discarded` (o lanza), el navegador avisa: «Se cerró la pestaña; su borrador limpio quedó en memoria: motivo» |
| Protegida (la regla de `IsProtected`, también cuando el puerto no puede decirlo) | No se cierra todavía: el navegador abre la confirmación de esa pestaña |

- **Desviación de letra declarada (M-1):** R-B2-1 dice «Limpia: … llama a `DiscardRack`». Sin borrador no hay nada que terminar, el Owner solo exige cerrar sin escribir, y no llamar evita que un `Rejected` estorbe el cierre. **Aceptada por el Owner** (F4-E43, punto 1): vale porque sin borrador no hay borrador ni texto pendiente en memoria que descartar (el pendiente vive en el borrador) y porque cerrar no hace ninguna escritura authored.
- **La confirmación** (D-T3B2-07) nombra la pestaña por su título sin la marca (el `Name` de su registro, o el `RackId`) y ofrece exactamente «Descartar» y «Cancelar»:
  - «Descartar» llama a `DiscardRack`; con `Discarded`, la pestaña se cierra; si no, no se cierra, la confirmación se quita y hay aviso con el motivo;
  - «Cancelar» no llama al puerto y no cambia nada más que quitar la confirmación.
- **Cerrar la pestaña activa** deja activa la siguiente (o la anterior) sin frontera. La nueva superficie pide su resumen, como en F3: es la única lectura y no es de T3B2.
- **Cerrar nunca escribe** y nunca hace Actualizar. No hay subdiálogo.

### D-T3B2-02. «Descartar» del campo en Error (D-08)

- La superficie ofrece «Descartar» **solo** en el Error del campo y **solo** con borrador: `Field.ShowDiscardButton` es `ShowReloadButton` y el borrador mostrado existe.
- `Browser.DiscardField()` sigue la regla de D-T3B2-01 **sin cerrar la pestaña**: confirmación si está protegida (el Error del host lo está); `DiscardRack` directo si no.
- Con `Discarded`: la pestaña se queda, el campo vuelve a mostrarse desde la memoria (sale del Error, «Sin borrador.») y la marca se recalcula. Si no: aviso con el motivo.

### D-T3B2-03. Descartar los borradores del documento (R-B2-2, I-7)

- `Browser.RequestDocumentDiscard()` lee `GetRacksAtRisk` de la sesión presentada:
  - las dos listas vacías: aviso «No hay borradores en riesgo en este documento; no se descartó nada» y **ningún** efecto;
  - si no: la confirmación lista los racks en riesgo por su título (D-T3B2-01; el `RackId` sin pestaña) y, aparte, los que no se pudieron decidir; guarda las dos listas.
- **«Descartar»** vuelve a leer `GetRacksAtRisk` y compara las dos listas, en orden y con comparación ordinal:
  - si cambiaron: no descarta; la confirmación se muestra de nuevo con la lista nueva y el aviso «La lista de borradores en riesgo cambió; revísala»; si la nueva está vacía, el aviso de «no hay borradores en riesgo» y la confirmación se quita;
  - si no cambiaron: `DiscardDocument`. Con `Discarded`, aviso con los racks en riesgo confirmados y, aparte, cuántos borradores limpios se retiraron además; se quita el aviso de veto de esa sesión y se recalculan las marcas y el campo. Si no: aviso con el motivo.
- **«Cancelar»:** sin llamadas al puerto.
- **Las pestañas no se cierran:** quedan sin marca y el campo muestra «Sin borrador.».
- **Limitación declarada (R-B2-5), NO aceptada como comportamiento final** (F4-E43, punto 3): el fallo de `GetRacksAtRisk` es indistinguible de «nada en riesgo» hasta el endurecimiento de T3C, que **debe** hacer que un fallo o un desconocido no pasen por un resultado limpio. Hoy es inalcanzable; en la TOCTOU su dirección es segura, pero el aviso «no hay borradores en riesgo» puede ser falso. T3B2 no añade el contraste opcional con `IsProtected`.

### D-T3B2-04. Avisos de veto y de pérdida (R-B2-3, D-17)

- **Consumo:** con el puerto de edición, `Present` llama al método privado `TakeNotices()`, el único que nombra `TakeCloseNotices` y `TakeLossNotices`, cada una en su `try`. Ningún gesto consume las colas (P-03). Un fallo deja el aviso del navegador «No se pudieron leer los avisos: motivo» y no detiene la presentación. El `Present` del constructor del panel también consume: es memoria (H-06).
- **Las filas** (`Notices`, del tipo `WorkspaceBrowserNoticeItem`) se recalculan desde los avisos guardados en cada reconstrucción y conservan su instancia si no cambian por valor, como la tira (T2-F6). Su clave es única y estable.
- **Veto:** uno por sesión; uno nuevo de la misma sesión sustituye al anterior. Se clasifica **al mostrarlo**, contra la sesión presentada:
  - de la sesión presentada: «No se cerró el documento: hay borradores sin aplicar en A, B.» (o, incierto, «… no se pudo decidir qué borradores están en riesgo.»), seguido de «Descártalos con "Descartar borradores de este documento"; cada pestaña también se descarta con su "Cerrar".» (sin mencionar ninguna vía de aplicar: en T3 no existe) La fila lleva el botón «Descartar borradores de este documento» (la misma vía que D-T3B2-03);
  - de otra sesión: «No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos.», sin botón. Si ese documento pasa a ser el presentado, la fila gana el botón.
- **Desviación transitoria declarada de D-17 (I-3):** D-17 pide que el mensaje del veto nombre las dos vías de descarte: la acción del panel y la vía de comando (`RACKPANELDESCARTAR`). El comando no existe hasta T3C, así que el aviso de T3B2 nombra solo la acción del panel. **Obligación de T3C:** cuando exista el comando, el texto del aviso del panel lo nombra (R-C2). Aceptada para el contrato por el Owner (F4-E43, punto 2): **F4-T3C DEBE añadir la vía del comando** cuando exista `RACKPANELDESCARTAR`.
- **Pérdida:** «Se perdieron borradores al cerrar un documento: R-1, R-2.», y aparte «Sin decidir: R-3 (motivo).».
- **Se quitan:**
  - con «Entendido» en su fila;
  - el de veto, con un `DiscardDocument` con `Discarded` de esa sesión;
  - el de veto, cuando su sesión deja de vivir: la comprobación va en el **mismo bucle** que `PurgeDeadSessions`, sobre las sesiones conocidas y las de los avisos **de veto**, con una sola llamada textual a `IsSessionAlive` (G-03). Los avisos de pérdida no se purgan así (su sesión siempre está muerta): solo «Entendido» los quita;
  - tras un `DiscardRack` con `Discarded`, el rack sale de la lista del veto de su sesión; si queda vacía y el aviso no es incierto, la fila se quita.
- **El veto no emite `StateChanged`:** el panel lo ve en la siguiente presentación, normalmente al fin del comando. Limitación declarada; el host no cambia.

### D-T3B2-05. La retención por selección (R-B2-4, P5 `:335-336`)

- `BrowserSession.ApplySelectionContext(SelectionContext context, bool keepActive = false)`. Sin `keepActive`, o sin pestaña activa, es exactamente el de hoy.
- **Con `keepActive` y una pestaña activa**, tras consumir el contexto como hoy:
  - `R` ya tiene pestaña: `None`, sin activarla;
  - `R` sin pestaña, con una preview que **no** es la activa: se reemplaza en su sitio (`Replaced`) sin cambiar `ActiveRackId`;
  - `R` sin pestaña y la preview es la activa: `None`; **nunca se reemplaza la pestaña activa**. Es casi inalcanzable: una pestaña con pendiente siempre está fijada (residual declarado);
  - `R` sin pestaña y sin preview: se añade como preview al final (`Created`) sin cambiar `ActiveRackId`.
- **El VM** pasa `keepActive` solo con el puerto de edición, y verdadero si la pestaña activa de esa sesión tiene texto pendiente (`PendingText != null`) o si `GetField` lanza. Es la letra de P5 y de la nota N-2 del paquete de F4-T3 («solo el texto pendiente retiene»): un `Error` o un `Undecidable` del host traen su pendiente leído (`WorkspaceHostCore.cs:882-889`). **Residual declarado:** la vista de fallo del `catch` de `GetField` del host (`:603-606`) no trae el pendiente y no retiene; hoy es inalcanzable.
- **El aviso de la retención** (aviso del navegador) sale cuando, con `keepActive`, el contexto es de un solo rack `R` distinto de la activa y es **distinto del contexto de la presentación anterior** (la misma regla que `_lastContext` de `BrowserSession`; una re-selección real, con otro contexto en medio, vuelve a avisar):
  - con `Created` o `Replaced`: «La selección abrió R en segundo plano; la pestaña activa conserva su texto pendiente.»;
  - con `R` ya abierta: «La selección de R no cambió la pestaña activa, que conserva su texto pendiente.».
- **Con la pestaña solo dirty** (sin pendiente), `R` se activa y la anterior cruza `Dematerialize`, como en T3B1.
- **El contexto retenido queda consumido:** volver a presentar la misma selección no la abre (letra de R-B2-4).

### D-T3B2-06. Arrastres de F4-T3B1 (vinculados por la disposición del Owner, F4-E43)

| Origen | Qué hace T3B2 |
|---|---|
| N-1 | El doble responde `Kept`, sin mutar, a una frontera bloqueante sobre un borrador con pendiente y `CanEdit` falso, como el host (`DraftSession.cs:370-371`). La mitad «Kept» de `T06_…` recorre `Kept` y afirma que el pendiente sigue en el borrador |
| N-3 | El code-behind recuerda la tecla que lanzó el primer gesto desde la caja de solo lectura. En la caja editable, una repetición (`IsRepeat`) de **esa** tecla queda marcada; la primera pulsación no repetida, de cualquier tecla, borra el recuerdo y sigue la regla de T3B1. Solo con `e.Key` y `e.IsRepeat` (G-05), sin `KeyUp` (H-08). **Cambio declarado de INV-F4T3B1-04:** en la caja editable quedan marcados Enter, Esc y esas repeticiones. El efecto real en AutoCAD (que el `Handled` del túnel suprima el carácter) se comprueba en Smoke-2, como en T3B1 |
| N-2 | Desviación declarada de la letra de INV-F4T3B1-15 (Space), correcta en sustancia; sin cambio |

Los menores 4 y 6 de F4-T3A siguen en F4-T3C, como ordenó el Owner.

### D-T3B2-07. La forma de la UI

**API pública nueva del VM del navegador** (todas contenidas: nada sale de ellas):

| Miembro | Qué es |
|---|---|
| `void CloseTab(string rackId)` | El de hoy, con D-T3B2-01 con el puerto |
| `void DiscardField()` | «Descartar» del campo en Error |
| `void RequestDocumentDiscard()` | «Descartar borradores de este documento», del botón de la sección o de una fila de veto |
| `void ConfirmDiscard()` | «Descartar» de la confirmación |
| `void CancelDiscard()` | «Cancelar» de la confirmación |
| `void DismissNotice(string key)` | «Entendido» de una fila |
| `bool CanDiscardDocument` | Con el puerto de edición y una sesión presentada |
| `bool HasConfirmation`, `string ConfirmationText` | La confirmación pendiente y su texto (vacío sin ella) |
| `string NoticeText` | El aviso del navegador (vacío sin él) |
| `IReadOnlyList<WorkspaceBrowserNoticeItem> Notices`, `bool HasNotices` | Las filas de aviso |

- `public sealed record WorkspaceBrowserNoticeItem(string Key, string Text, bool CanDiscardDocument);` en el mismo archivo del VM del navegador.
- **El VM del campo** gana solo `bool ShowDiscardButton` (D-T3B2-02).
- **La confirmación:** a lo sumo una, de uno de tres tipos (cerrar una pestaña, descartar el campo, descartar el documento). Una petición nueva la sustituye. Se quita con «Cancelar», con su resultado, con un cambio de la sesión presentada, si su pestaña deja de existir, o (la del campo) si cambia la superficie viva.
- **El aviso del navegador (`NoticeText`)** se quita al empezar cada gesto de T3B2 y en la reconstrucción cuando cambia la superficie viva (la pestaña activa o la sesión presentada), también por un gesto de T3B1; si no, dura hasta que otro aviso lo sustituye. Un gesto que cambia la superficie y deja aviso lo pone **después** de su reconstrucción. Una presentación sin cambio de superficie no lo borra.
- **Contención:** una excepción del puerto en un gesto de T3B2 deja el aviso del navegador con el motivo y quita la confirmación. `Field.Fail` (el campo pasa a Error y conserva su borrador, D-17) solo si la pestaña afectada **sigue abierta** y es la superficie viva, y antes de cualquier cierre: en el cierre de una pestaña limpia, que se cierra siempre, no hay `Field.Fail`. Nunca `ContainFailure` ni el Error del panel.

**La vista** (en la sección del navegador, entre la tira y la superficie, fuera de `BrowserTabStrip`, sin tamaños fijos ni `ContentControl`):

| Elemento | Nombre | Contenido y automatización | Manejador |
|---|---|---|---|
| Botón | `BrowserDiscardDocumentButton` | «Descartar borradores de este documento»; visible con `Browser.CanDiscardDocument` | `OnBrowserDiscardDocumentClick` |
| Panel | `BrowserConfirmationPanel` | visible con `Browser.HasConfirmation`; contiene `BrowserConfirmationText` y los dos botones | — |
| Botón | `BrowserConfirmDiscardButton` | «Descartar»; automatización «Confirmar el descarte» | `OnBrowserConfirmDiscardClick` |
| Botón | `BrowserCancelDiscardButton` | «Cancelar»; automatización «Cancelar el descarte» | `OnBrowserCancelDiscardClick` |
| Texto | `BrowserNoticeText` | `Browser.NoticeText` | — |
| Lista | `BrowserNoticeList` | `ItemsControl` de `Browser.Notices`; por fila, el texto, el botón «Descartar borradores de este documento» (visible con `CanDiscardDocument`; automatización «Descartar los borradores del documento del aviso») y el botón «Entendido» (automatización «Quitar el aviso») | `OnBrowserNoticeDiscardDocumentClick`, `OnBrowserNoticeDismissClick` |
| Botón (superficie) | `BrowserFieldDiscardButton` | «Descartar»; automatización «Descartar el borrador»; visible con `Browser.Field.ShowDiscardButton` | `OnBrowserFieldDiscardClick` |

- Cada manejador es `=> Contained(() => …)`. Ningún botón nuevo lleva `IsDefault` ni `IsCancel`.

### D-T3B2-08. El doble

- `DiscardRack` y `DiscardDocument` conservan su respuesta por defecto (`Rejected`, sin mutar) y ganan respondedores opcionales (`DiscardRackResponder`, `DiscardDocumentResponder`). Un ayudante `DiscardAsTheHostDoes()` los configura para imitar al host: quitar el campo y responder `Discarded` con sus listas.
- `GetRacksAtRisk` gana `RisksResponder`; las colas ganan dos colas de prueba (`LossQueue`, `CloseQueue`) que se vacían al tomarlas.
- Las cinco operaciones llaman a `ThrowIfAsked`.
- N-1 (D-T3B2-06).

### D-T3B2-09. Lo que no cambia, y las limitaciones declaradas

- El host, el puerto, el Plugin y la paleta no cambian. El VM del panel solo cambia el texto de la documentación de `pilotEdit`.
- Las tres lecturas de la intención siguen solo en los gestos de T3B1. T3B2 no llama a ninguna.
- La regla de edición, el primer gesto, «Nombre *», `AllowTabChange` y `CrossObservedBoundaries` de T3B1 no cambian, salvo N-3.
- **Limitaciones para Smoke-2:**
  - pulsar «Cerrar» con la caja enfocada compromete antes el pendiente por `LostFocus`: un texto válido llega a la confirmación ya aplicado (dirty, protegido), y «Cancelar» lo deja aplicado, no pendiente;
  - el clic en «Cerrar» puede perderse si la tira se regenera entre la pulsación y la liberación (la limitación de T3B1).
- **Declarado (M-12):** en una pestaña sana, la reversión del pendiente es «Cerrar» y «Descartar», o el descarte del documento. Una acción «Revertir» propia queda como mejora de F7.

## 4. Pruebas

- **Core, `WorkspaceBrowserSessionTests.cs` (añadidas):** cada rama de `keepActive` de D-T3B2-05; el contexto consumido también retenido; sin pestañas, `keepActive` crea y activa; el resto sin cambios.
- **Core, `WorkspacePilotCloseSyncGuardTests.cs` (nuevo; cada guarda con su autoprueba en negativo, por cuerpo de miembro, y buscando **llamadas** (`\.Nombre\s*\(`, o `\bNombre\s*\(` que no sea la declaración), nunca subcadenas: `CanDiscardDocument`, `BrowserDiscardDocumentButton` y los manejadores contienen `DiscardDocument`, y la declaración `private void TakeNotices()` contiene `TakeNotices(`):**
  - en `src/RackCad.UI/`, las llamadas a `DiscardRack` solo en los cuerpos de `CloseTab`, `DiscardField` y `ConfirmDiscard` del VM del navegador; a `DiscardDocument`, solo en `ConfirmDiscard`; a `GetRacksAtRisk`, solo en `RequestDocumentDiscard` y `ConfirmDiscard`;
  - las llamadas a `TakeCloseNotices` y `TakeLossNotices`, solo en el cuerpo de `TakeNotices` del VM del navegador, y las llamadas a `TakeNotices`, solo en el cuerpo de `Present`;
  - en `src/RackCad.Plugin/` y `src/RackCad.Application/`, `TakeCloseNotices` y `TakeLossNotices` solo en el puerto, en su implementación del host y en los reenvíos de `WorkspacePilotSupport.cs` y `WorkspaceController.cs` (M-11);
  - el VM del navegador pasa `keepActive` a `ApplySelectionContext`;
  - los botones nuevos del XAML sin `IsDefault` ni `IsCancel`.
- **UI, `WorkspacePilotCloseSyncTests.cs` (nuevo, VM) y `WorkspacePilotCloseSyncViewTests.cs` (nuevo, STA):**
  - **11:** el descarte del campo en Error y el del documento quitan el borrador y el pendiente. El oráculo: el conjunto exacto de operaciones llamadas, como `T15`; cero lecturas de la intención; cero llamadas a la navegación;
  - **12:** cerrar una pestaña protegida muestra la confirmación con exactamente «Descartar» y «Cancelar», y no cierra;
  - **13:** «Cancelar» conserva la pestaña, el borrador, el pendiente y la marca, sin ninguna llamada al puerto;
  - **14:** «Descartar» llama a `DiscardRack` con esa sesión y ese rack, cierra y quita el estado;
  - la pestaña sin borrador se cierra sin descarte; la limpia, con `DiscardRack`, también cuando responde `Rejected`, `Busy`, `Failed` o lanza, con su aviso;
  - el rechazo en la confirmación no cierra y avisa; la confirmación nombra una pestaña no activa, se anula con un cambio de sesión y se sustituye con otra petición;
  - el documento: sin riesgos, el aviso sin efecto; con riesgos, la lista y los inciertos; la TOCTOU que no descarta y muestra la lista nueva; el descarte con `Discarded`, su aviso y la fila de veto quitada;
  - los avisos: veto de la sesión presentada con su botón; de otra sesión sin él, y con él al presentarse; incierto; pérdida con inciertos, que sobrevive a varias presentaciones hasta «Entendido»; filas estables; purga del veto con su sesión; el consumo solo en `Present`; un fallo de las colas que avisa;
  - el aviso del navegador: se quita con un cambio de la pestaña activa (también por un gesto de T3B1) y se conserva en una presentación sin cambio de superficie;
  - la retención: `Created` y `Replaced` detrás con aviso; `R` abierta sin activarla, con aviso; una re-selección de `R` con otro contexto en medio vuelve a avisar; un `GetField` que lanza retiene; un `Error` o un `Undecidable` sin pendiente no retienen; con la pestaña solo dirty, `Dematerialize` como en T3B1;
  - la contención: una excepción al cerrar la pestaña activa limpia cierra, avisa y no pone en Error el campo de la nueva superficie;
  - sin puerto de edición: `CloseTab` y `Present` son los de antes (sin colas ni `keepActive`);
  - la vista: los nombres de D-T3B2-07, sin tamaños fijos, fuera de la tira y entre la tira y la superficie;
  - N-1 en `T06_…`; N-3 con `PreviewKeyDown` repetido: la repetición de la tecla que abrió queda marcada, la de otra tecla no, y una pulsación no repetida borra el recuerdo.
- **Cambios declarados de pruebas existentes:** P-01 y P-05. Ninguno más.

## 5. Contrato propuesto (a fijar en la emisión)

- **AllowedWriteScope:**
  - `src/RackCad.Application/Workspace/BrowserSession.cs`;
  - `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`, `WorkspacePilotFieldViewModel.cs`, `WorkspacePanelView.xaml`, `WorkspacePanelView.xaml.cs` y `WorkspacePanelViewModel.cs` (solo la documentación de `pilotEdit`);
  - `tests/RackCad.Tests/Workspace/WorkspaceBrowserSessionTests.cs` y `WorkspacePilotCloseSyncGuardTests.cs` (nuevo);
  - `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`, `WorkspacePilotFieldViewTests.cs` (solo P-01 y N-3), `WorkspacePilotFieldViewModelTests.cs` (solo `T06_…`), `WorkspacePilotCloseSyncTests.cs` (nuevo) y `WorkspacePilotCloseSyncViewTests.cs` (nuevo).
- **ForbiddenWriteScope:** por prefijos compactos: el resto de `src/`, de `tests/RackCad.Tests/` y de `tests/RackCad.UI.Tests/`, `docs/`, `.github/`.
- **RequiredTests:**
  - Core: `FullyQualifiedName~RackCad.Tests.Workspace`, `MinSelected` 2300, `ExpectRed` true;
  - UI: `FullyQualifiedName~RackCad.UI.Tests.Workspace`, `MinSelected` 352, `ExpectRed` true.
- **Regla RED (INV-F4T3B2-14):** el commit RED lleva el esqueleto completo y compilable, que no lanza:
  - `ApplySelectionContext(…, bool keepActive = false)` que ignora el parámetro;
  - los miembros de D-T3B2-07, inertes (sin llamadas al puerto; propiedades vacías o falsas), y el record;
  - `ShowDiscardButton` falso;
  - los elementos del XAML con sus nombres, enlaces y manejadores;
  - el doble de D-T3B2-08 y los cambios declarados P-01 y P-05.

  En el RED fallan **solo** pruebas nuevas, y por aserción. Todas las existentes pasan (Core 2300, UI 352).
- **Invariantes** (en indicativo o con construcciones nominales):
  - INV-F4T3B2-01: sin puerto de edición, `CloseTab` y `Present` son los de antes;
  - INV-F4T3B2-02: la tabla de D-T3B2-01, con la desviación de letra declarada;
  - INV-F4T3B2-03: la confirmación de D-T3B2-07, con exactamente «Descartar» y «Cancelar», sin subdiálogo, sin `IsDefault` ni `IsCancel`;
  - INV-F4T3B2-04: «Cancelar»: ninguna llamada al puerto;
  - INV-F4T3B2-05: cerrar o descartar: cero llamadas a las tres lecturas de la intención y ninguna escritura; la lectura del resumen de la nueva superficie activa, de F3, no cambia;
  - INV-F4T3B2-06: el descarte del documento con su TOCTOU;
  - INV-F4T3B2-07: el consumo de las colas solo en `TakeNotices`, llamado solo desde `Present`;
  - INV-F4T3B2-08: los avisos de D-T3B2-04, con la desviación transitoria de D-17 y la obligación de T3C;
  - INV-F4T3B2-09: `keepActive` de D-T3B2-05, sin valores nuevos ni sobrecargas, con el vocabulario de G-02;
  - INV-F4T3B2-10: el aviso de la retención;
  - INV-F4T3B2-11: «Descartar» del campo en Error;
  - INV-F4T3B2-12: la forma de la UI y la contención de D-T3B2-07;
  - INV-F4T3B2-13: N-1 y N-3 (F4-E43), con el cambio declarado de INV-F4T3B1-04;
  - INV-F4T3B2-14: la regla RED;
  - INV-F4T3B2-15: las limitaciones declaradas (R-B2-5, el veto sin `StateChanged`, las dos de Smoke-2 y M-12).
- **Paradas:** las de T3B1 adaptadas; cualquier cambio de `BrowserSession` distinto del parámetro opcional; cualquier archivo fuera del alcance; cualquier cambio de una prueba existente distinto de P-01 y P-05. La regla de replanificación de F4 (F4-E31, F4-E43): si la primera planificación falla solo por fidelidad literal de colecciones, una replanificación reforzada, sin consumir un intento de producto; un segundo fallo de fidelidad de la planificación es STOP al Coordinator.

## 6. Puntos para la re-revisión

1. ¿La disposición de §7 resuelve los cinco IMPORTANTE?
2. ¿La API y la vista de D-T3B2-07 son suficientes para escribir las guardas y las pruebas sin inventar nombres?
3. ¿La regla RED es satisfacible: compila el esqueleto, y pasan todas las existentes con el doble ampliado y P-01?
4. ¿La extensión conservadora de `keepActive` (vista ilegible) es aceptable frente a P5 y la nota N-2 del paquete de F4-T3?
5. ¿Algo de la v2 rompe una guarda o una prueba existente que no esté en §2?

## 7. Disposición de la revisión adversarial de la v1 (EMITIBLE TRAS CORRECCIONES, 0/5/12)

| Hallazgo | Disposición |
|---|---|
| I-1 (`WorkspacePilotFieldViewTests.cs:218`) | **Aceptado:** P-01, cambio declarado y alcance |
| I-2 (la limpia no se cierra si el descarte falla) | **Aceptado:** se cierra siempre y avisa (D-T3B2-01); T10 no cambia (P-02) |
| I-3 (las dos vías de D-17) | **Aceptado:** desviación transitoria declarada y obligación de T3C (D-T3B2-04) |
| I-4 (la forma de la UI) | **Aceptado:** D-T3B2-07 y D-T3B2-08; las guardas derivan sus miembros de ahí |
| I-5 (regla RED, filtros, alcance de la vista) | **Aceptado:** INV-F4T3B2-14, filtros y `MinSelected` medidos en `1708e764` (2300 y 352), y `WorkspacePilotCloseSyncViewTests.cs` en el alcance |
| M-1 (sin borrador, sin `DiscardRack`) | **Aceptado:** desviación de letra declarada |
| M-2 («cero lecturas») | **Aceptado:** INV-F4T3B2-05 reformulada |
| M-3 (trampas) | **Aceptado:** §2.3, G-01 a G-09 |
| M-4 (sin puerto; constructor del panel) | **Aceptado:** INV-F4T3B2-01 ampliada; documentación de `pilotEdit` en el alcance |
| M-5 (retención) | **Aceptado:** aviso con `R` abierta; residual de la preview activa declarado. La retención por una vista ilegible de la v2 se **retira** en la v3 (N-1 de la re-revisión, §8) |
| M-6 (veto de otra sesión) | **Aceptado:** clasificación al mostrar; purga en el bucle de `PurgeDeadSessions` |
| M-7 (campo sin borrador; número del aviso) | **Aceptado:** «Descartar» solo con borrador; el aviso separa los confirmados de los limpios |
| M-8 (el doble; oráculo de 11) | **Aceptado:** D-T3B2-08 y el oráculo de la prueba 11 |
| M-9 (WPF) | **Aceptado:** sin `IsDefault`/`IsCancel`, filas estables, dos limitaciones para Smoke-2 |
| M-10 (N-3) | **Aceptado:** prueba negativa y reserva de Smoke-2 |
| M-11 (guarda del Plugin) | **Aceptado:** la guarda cubre el Plugin y Application |
| M-12 (reversión) | **Aceptado:** declarado; mejora de F7 |

## 8. Disposición de la re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/1/6; los 17 hallazgos de la v1, resueltos)

| Hallazgo | Disposición |
|---|---|
| N-1 (la extensión de `keepActive` contradice «solo el texto pendiente retiene») | **Aceptado:** se retiene solo con pendiente o si `GetField` lanza; la vista del `catch` del host queda como residual declarado (D-T3B2-05) |
| N-2 (la purga podía borrar los avisos de pérdida) | **Aceptado:** la purga es solo de los avisos de veto; la pérdida dura hasta «Entendido», con prueba |
| N-3 («aplícalos antes de cerrar» sin vía de aplicar en T3) | **Aceptado:** frase retirada |
| N-4 (orden de `Field.Fail` y del cierre) | **Aceptado:** `Field.Fail` solo con la pestaña afectada abierta y antes de cualquier cierre; el cierre limpio no lo usa; con prueba |
| N-5 (caducidad del aviso del navegador y de la retención) | **Aceptado:** el aviso se quita con un cambio de la superficie viva; la retención avisa ante un contexto distinto del de la presentación anterior |
| N-6 (espacio de nombres de G-09; G-02 y los comentarios) | **Aceptado:** la convención de cada proyecto, con nombre de clase que empieza por `Workspace`; G-02 corregida |
| N-7 (guardas por llamada, no por subcadena) | **Aceptado:** §4 fija la búsqueda de llamadas y excluye la declaración |

## 9. Disposición del Owner y del Coordinator (F4-E43)

Orden literal: `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t3b1-close-disposition.md`.

- **F4-T3B1 = CLOSED / PASS WITH LIMITATIONS** sobre `1708e764` (intento 2). nc1, nc2 y nc3 se satisfacen con sustitutos deterministas; las corridas ordinarias se conservan.
- **Esta subtarea:** el paquete v3 puede seguir. Emitirlo no requiere otra decisión del Owner. `attempts` empieza en 1 de 3.
- **Alcance aceptado** (D-T3B2-01 a D-T3B2-06):
  - cerrar en línea una pestaña con trabajo sin aplicar, con Descartar o Cancelar;
  - cerrar sin preguntar una pestaña limpia;
  - Descartar en un campo en Error;
  - descartar los borradores del documento, con la comprobación TOCTOU;
  - los avisos de veto y de pérdida;
  - mantener la pestaña activa si cambia la selección con texto sin aplicar;
  - los menores heredados de F4-T3B1 que le correspondan.
- **N-1 y N-3, vinculados** (D-T3B2-06, INV-F4T3B2-13). **N-2** (la tecla Space) es una limitación declarada de F4-T3B1 y aquí no cambia.
- **Tratamiento aceptado en el contrato:**
  1. sin borrador, la pestaña se cierra sin `DiscardRack` (D-T3B2-01), si no hay borrador ni pendiente en memoria y no hay escritura authored;
  2. el aviso de veto nombra solo la vía del panel (D-T3B2-04). F4-T3C **debe** añadir la vía del comando;
  3. el fallo de `GetRacksAtRisk` indistinguible de «nada en riesgo» (D-T3B2-03) **no** se acepta como comportamiento final. F4-T3C **debe** endurecerlo.
- **Regla de replanificación** (se mantiene): si la primera planificación falla solo por fidelidad literal de colecciones, una replanificación reforzada, sin consumir un intento de producto. Un segundo fallo de fidelidad de la planificación es STOP al Coordinator.
- **F4-T3C** (en espera hasta el cierre de esta subtarea) es dueña, como mínimo, de:
  - la exposición del comando `RACKPANELDESCARTAR`;
  - el texto de los avisos con la vía del comando;
  - el endurecimiento del fallo o desconocido de `GetRacksAtRisk`;
  - la integración modal y de foco que quede de F4-T3;
  - los menores arrastrados a F4-T3C.
- **F4-T4** no empieza antes del cierre de F4-T3.
