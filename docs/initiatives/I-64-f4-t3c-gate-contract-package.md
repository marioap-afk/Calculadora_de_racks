# I-64 — Paquete del contrato de F4-T3C (cierre de la línea de comandos y endurecimiento), con F4-T3C1-HOST-HARDENING

**Estado:** v3. La revisión adversarial independiente de la v1 dio EMITIBLE TRAS CORRECCIONES (1/4/10), con su disposición en el §8. La re-revisión de la v2 dio EMITIBLE TRAS CORRECCIONES (0/1/6), con 13 de los 15 de la v1 resueltos y su disposición en el §9. Todo se acepta. Por el precedente de F4-T3B, la v3 no pasa una tercera revisión.
- Lo prepara la sesión el 2026-11-13, tras el cierre de F4-T3B2 (`7895f397`, F4-E46), sobre el código real de `23f67f56`. Desde ese commit, solo documentación.
- **Orden del Owner (`owner-f4t3c-decomposition-decision.md`):** «Dos subtareas». F4-T3C1-HOST-HARDENING primero y F4-T3C2-COMMAND-ADVISOR después, cada una con 1 de 3 intentos.
- **Este paquete fija:**
  - el diseño de T3C1 (§3 a §5);
  - las reglas vinculantes de T3C2 (§6). Su paquete se fija tras cerrar T3C1, sobre el código real.

## 1. Autoridades

- **Órdenes del Owner** (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/`):
  - `owner-f4t3b1-close-disposition.md` (F4-E43). F4-T3C es dueña, como mínimo, de:
    - exponer `RACKPANELDESCARTAR`;
    - el aviso con la vía del comando;
    - el endurecimiento del fallo o desconocido de `GetRacksAtRisk`, que **no** se acepta como final;
    - la integración modal y de foco que quede de F4-T3;
    - los menores arrastrados.
  - `owner-f4t3-authorization.md`: el oráculo crítico de cero escrituras authored y sin Actualizar.
  - `owner-f4t3a-close-decision.md`: los menores 4 y 6 de F4-T3A van a T3C.
  - `owner-f4t3c-decomposition-decision.md`: las dos subtareas.
- **Paquete de F4-T3** (`docs/initiatives/I-64-f4-t3-gate-contract-package.md`): §8, R-C1 a R-C4, y D-T3-09 (M-1 de F4-T2B2B).
- **Paquete de F4-T3B2** (`docs/initiatives/I-64-f4-t3b2-gate-contract-package.md`): la desviación transitoria de D-17 y R-B2-5.
- **Cierres de las subtareas:**
  - F4-T3A, §5, punto 4: el fallo de `GetRacksAtRisk` como incierto;
  - F4-T3B1, §5: el menor 6 de la revisión del intento 1, los motivos del host en inglés y el Error sin motivo;
  - F4-T3B2, §5, y su revisión `4f9b`: M-1 a M-7 y D-1.
- **Freeze:** `docs/initiatives/I-64-proposal-v5.md`, D-08 y D-17.

## 2. Hechos medidos en `7895f397` (código de `23f67f56`)

### 2.1 El riesgo y las colas

| Id | Hecho |
|---|---|
| H-01 | `PilotRiskView` (`IWorkspacePilotEditPort.cs:272-289`) es un `readonly struct` con un solo constructor `(rackIds, uncertainRackIds)` que copia y lanza con null. `default` da dos listas vacías y **no se distingue de «nada en riesgo»**. Ninguna prueba compara `PilotRiskView` por valor |
| H-02 | `WorkspaceHostCore.GetRacksAtRisk` (765-807): el `try` interno (770-777) y el de cada borrador (790-797) se tragan los fallos del comparador. El `catch` externo (802-806) hace `Report(...)` y `return default;`. **Hoy ese `catch` es inalcanzable:** solo se llegaría si lanzaran `_drafts.RackIdsOf` o `_drafts.Find`, y `DraftRegistry` no lanza con los rack ids que admite (`DraftRegistry.cs:145, 169-175 y 186-189`). Lo dicen el paquete de F4-T3B2 (94) y la revisión de F4-T3A. El host es el **único** productor de `default(PilotRiskView)` |
| H-03 | Consumidores: `RequestDocumentDiscard` (353-388) y `ConfirmDiscard` (396-507), más T3C2. Una respuesta vacía da `NoRiskText` (372 y 429-433). El doble de UI construye siempre sus listas (`WorkspacePilotEditPortDouble.cs:262-268`), y también todos los `RisksResponder` |
| H-04 | `TakeLossNotices` (809-819: desencola y luego convierte) y `TakeCloseNotices` (821-826) no tienen `try` (menor 4 de F4-T3A) |
| H-05 | `SessionDestroyed` (947-971): `_knownClean.Remove` y `DropCloseNotice` van fuera de todo `try` y antes de la retirada (950; menor 6 de F4-T3A). Después vienen dos `try`: la retirada y la pérdida |
| H-06 | La pérdida necesita `GetRacksAtRisk` **antes** de la retirada: después da vacío (T3C2) |

### 2.2 Los motivos y el Error

**H-07. Tabla cerrada de los literales en inglés que devuelve el puerto del piloto.** Las líneas son de `WorkspaceHostCore.cs` y `WorkspacePilotSupport.cs`. Esta lista es también la de la guarda nueva (§4).

| # | Literal | Líneas | Texto en español |
|---|---|---|---|
| 1 | `A read of the pilot is in progress.` | host 44 (`PilotReadInProgressReason`) | `Hay una lectura del piloto en curso.` |
| 2 | `The request does not belong to the active document.` | host 45 (`NotThisDocumentReason`), 320, 401, 479, 525 y 1042 | `La petición no es del documento activo.` |
| 3 | `There is no draft for the rack.` | host 46 (`NoDraftReason`), 387 y 466 | `No hay borrador para el rack.` |
| 4 | `The preview witness has no key.` | host 299, 377 y 456 | `El testigo de la vista previa no tiene clave.` |
| 5 | `The drawing is not ready to be read.` | host 318, 399 y 477 | `El dibujo no está listo para leerse.` |
| 6 | `The read of the edit intention failed.` | host 322 | `Falló la lectura de la intención de edición.` |
| 7 | `The read of the edit intention carries no result.` | host 326 | `La lectura de la intención de edición no trajo resultado.` |
| 8 | `The session was retired.` | host 350 | `La sesión se retiró.` |
| 9 | `A blank rack has no draft.` | host 373 | `Un rack sin identificador no tiene borrador.` |
| 10 | `The draft is not unknown after a commit that threw.` | host 389 y 420 | `El borrador no quedó desconocido tras una aplicación fallida.` |
| 11 | `The reread failed.` | host 403 | `Falló la relectura.` |
| 12 | `The reread carries no result.` | host 407 | `La relectura no trajo resultado.` |
| 13 | `The draft does not need to be reloaded.` | host 467 | `El borrador no necesita recargarse.` |
| 14 | `The read for the reload failed.` | host 481 | `Falló la lectura para la recarga.` |
| 15 | `The read for the reload carries no result.` | host 485 | `La lectura para la recarga no trajo resultado.` |
| 16 | `The draft is being applied or is no longer registered.` | host 502 | `El borrador se está aplicando o ya no está registrado.` |
| 17 | `The draft was not reloaded.` | host 504 | `El borrador no se recargó.` |
| 18 | `The state of the field could not be read.` | host 606 | `No se pudo leer el estado del campo.` |
| 19 | `The text is required.` | host 617 | `El texto es obligatorio.` |
| 20 | `The field cannot be edited now.` | host 622, 625, 648 y 661 | `El campo no se puede editar ahora.` |
| 21 | `The boundary is not known.` | host 689 | `La frontera no se conoce.` |
| 22 | `The rack has no annotation scale.` | host 879 | `El rack no tiene escala de anotación.` |
| 23 | `No reader of the edit intention is assigned.` | host 1018 | `No hay un lector de la intención de edición asignado.` |
| 24 | `There is no active document.` | host 1024 | `No hay documento activo.` |
| 25 | `A RackCad window is open.` | host 1030 | `Hay una ventana de RackCad abierta.` |
| 26 | `A command is in progress.` | host 1036 | `Hay un comando en curso.` |
| 27 | `The value is not a finite number greater than zero.` | support 26 | `El valor no es un número finito mayor que cero.` |
| 28 | `A notice of the panel is being delivered.` | support 156 (`InsideNoticeReason`) | `Se está entregando un aviso del panel.` |

- **Medido:** ninguna prueba existente afirma estos literales; las coincidencias en `tests/` son literales propios de dobles.
- **Identificadores que se conservan:**
  - `PilotReadInProgressReason` (`WorkspacePilotEditGuardTests.cs:385`) e `InsideNoticeReason` (428 y 448-450), que fijan las guardas;
  - `NotThisDocumentReason` y `NoDraftReason`, por decisión: solo se usan en `WorkspaceHostCore.cs`.
- **Se quedan en inglés, por ser registro o diagnóstico y estar fijados:**
  - `DescribeLoss` (977-988): `WorkspaceLifecycleGuardTests.cs:405-408` exige «Uncertain»;
  - `DescribeDifferences` (1050, «(variable registry)»).
- **Fuera de T3C1, en otras capas:**
  - los textos de F3 (resumen y navegación), que no son del campo;
  - los del Plugin que sí llegan al campo (`WorkspaceNavigationHost.cs:224, 226, 236 y 251`, y `RackPilotIntentAdapter.cs:80, 112, 118, 127, 157 y 174`) pasan a **T3C2** (R-C14).
- **Duplicación en la UI:**
  - `WorkspacePilotFieldViewModel.RefusalOf` (427-436) antepone «hay una lectura en curso.» y «el campo no se puede editar ahora.» al motivo;
  - `WorkspaceBrowserViewModel.WhyOf` (904-913) antepone «hay una lectura en curso.».
  - Traducidos, se repetirían.

| Id | Hecho |
|---|---|
| H-08 | `PilotFieldView` tiene un constructor de 9 parámetros (157-177) sin motivo de Error. `DraftSession.ErrorReason` (`Drafts/DraftSession.cs:308`) no cruza el puerto, y la UI muestra «Error: sin detalle.» (`WorkspacePilotFieldViewModel.cs:372`). `DraftSession.EnterError` no tiene llamadores en `src/`: un Error real solo existirá con F4-T4. Ninguna prueba fija «Error: sin detalle.» |

### 2.3 Los menores de F4-T3B2 (revisión de `4f9b`)

| Id | Hecho |
|---|---|
| M-1 | `CloseTab`, rama limpia (VM 291-294): una excepción del puerto no quita una confirmación pendiente de **otra** pestaña, contra la letra de INV-F4T3B2-12 |
| M-2 | VM 1004: con solo inciertos, el texto dice «…de: .». Casos límite del mismo tipo: VM 1000 (confirmación de cierre con `ReadField` null) y VM 657-660 (los dos avisos de la retención cuando `GetField` lanza; el caso real `AFieldThatThrowsRetainsTheActiveTab`, `WorkspacePilotCloseSyncTests.cs:1179-1189`, sale por 659). `KeepsActive` (664-676) devuelve un `bool`, y `ConfirmationTextOf` (995-1007) no sabe si la lectura falló |
| M-3 | El `Present` del constructor del panel (PVM 73) no está contenido. Con un veto en cola, la purga (VM 625-630) llama a `IsSessionAlive` en la construcción. **El comportamiento de F3 está fijado:** `WorkspaceBrowserViewModelTests.cs:1006-1019` (`AFailureOfTheLivenessCheckReachesThePanelAsTheContainedErrorState`) usa `ThrowOnAlive` y exige que un fallo de `IsSessionAlive` en `Refresh` lleve el panel al Error contenido. La documentación de PVM 60 dice «never when the panel is built», y la de `DismissNotice` (516) dice que no pregunta nada, pero `Publish` llama a `GetField` |
| M-4 | `IsDeclaration` (G:46) toma por declaración una llamada tras `=>` o `?` |
| M-5 | Guardas sin autoprueba en negativo: 196-203, 205-213, 215-225, 229-255, 288-299, 323-374, 404-417, 421-438 y 468-485. Las autopruebas de 279-286 y 456-464 están incompletas. La forma pública solo comprueba presencia. El VM tiene 41 líneas `public`: 2 records, la clase, 5 de `StoredNotice` y **33 miembros de la clase exterior** (el constructor, 18 propiedades y 14 métodos) |
| M-6 | G:450-452 y G:389 fijan para todo el panel más de lo que pide T3B2. El cierre de F4-T3B2 condiciona el cambio: «F4-T3C las cambia de forma declarada **si las necesita**» |
| M-7 | «Nunca `ContainFailure`» no tiene prueba ni guarda. `CancelDiscard` y `DismissNotice` no tienen excepción del puerto que inyectar. Cerrar la activa limpia cambia la superficie de forma legítima |

### 2.4 Modal y foco

**Medido:** ningún contrato concreta una obligación de código de modal o de foco para T3C.
- `EnterPanelModal` no tiene llamadores en `src/`.
- D-T3-05, R-B7 y R-B11 condicionan el modal y el foco a que haya subdiálogo o foco tocado.
- D-T3B1-10 deja la pérdida de foco como residual de F7.
- R-C1 a R-C4 no tienen ninguna regla de modal ni de foco.

**Lo que queda:** la validación de Smoke-2 y, en T3C2, la regla de que el comando no abre ningún subdiálogo (R-C13).

### 2.5 Trampas de guardas existentes para T3C1

| Id | Guarda | Efecto |
|---|---|---|
| G-01 | `WorkspacePilotEditGuardTests.cs:153-210` | los tipos del puerto son valores sin anidados, con propiedades de solo lectura y campos `readonly` |
| G-02 | ídem, 73 y 99 | `\bDraft\w*` y `PushBack\w*` se buscan en el **código** del puerto (`Src.Code` quita los comentarios) y distinguen mayúsculas. La documentación ya dice «draft» (93 y 128) |
| G-03 | ídem, 747-753 y 802 | los nombres de tipo nuevos no pueden llevar ciertas palabras. T3C1 no añade tipos |
| G-04 | ídem, 756-758 y 782-788, y `WorkspaceBridgeSourceGuardTests.cs:362-372` | sin distinguir mayúsculas y por subcadena, sobre el código, o sobre el texto con comentarios de `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `WorkspaceHostCore.cs` y `WorkspaceEditIntent.cs`, quedan prohibidos: `Transaction`, `Database`, `Commit(`, `RackLog`, `Provider`, `Metric`, `LockDocument`, `Aggregat`, `\bBom`, `TotalRacks`, `ProjectSummary` y `ComputedParameter`. **En español:** evitar «métrica» o «metrica», «proveedor» y cualquier palabra que contenga esas subcadenas. Distinguir o no mayúsculas: se aplica la regla más estricta. **En la UI**, la lista propia de `WorkspacePaletteHostSourceGuardTests.cs:592-593` se aplica a todo `src/RackCad.UI`: `Registry`, `RackLog` y `XData`, distinguiendo mayúsculas, y `Metric`, `Provider` y `\bBom`, sin distinguirlas. Los textos nuevos la cumplen |
| G-05 | ídem, 282-287 | `GetRacksAtRisk` tiene `catch (Exception`, sin `_faulted`, `Contain(` ni `throw;` |
| G-06 | ídem, 431 y 498 | el gate y el adaptador delegan `GetRacksAtRisk` de forma literal |
| G-07 | `WorkspaceLifecycleGuardTests.cs:376-389` | `SessionDestroyed` tiene exactamente 2 `try` y 2 `catch (Exception ex)`, con su orden. `DropCloseNotice` no entra en el orden |
| G-08 | ídem, 401-403 | `TakeLossNotices` contiene `_lossNotices.Dequeue()`, el único `Dequeue` |
| G-09 | `WorkspacePilotEditGuardTests.cs:247-251` | los `Take*` y `GetRacksAtRisk` no llaman a `Authorize` (reglas de memoria) |
| G-10 | `WorkspacePilotCloseSyncGuardTests.cs:108-113, 146 y 234-250` | una llamada a `GetRacksAtRisk` en cada uno de sus dos métodos; `TakeNotices` con 2 `try`; la forma pública |
| G-11 | G-03 de F3 (también `WorkspaceBrowserHostGuardTests.cs:248` y G:269 y 274) | `.IsSessionAlive(` exactamente una vez en el VM |
| G-12 | `WorkspaceBrowserViewModelTests.cs:1006-1019` (F3) | un fallo de `IsSessionAlive` en `Refresh` lleva el panel al Error contenido. **No cambia** |
| G-13 | `WorkspacePilotCloseSyncGuardTests.cs:288-299` | `ThePanelViewModelOnlyChangesTheDocumentationOfPilotEdit` mira la firma, la construcción del navegador y la ausencia de llamadas a las colas. Contener el `Present` del constructor la sigue cumpliendo. Además, en 298 exige que el archivo, comentarios incluidos, no contenga «Building the panel never calls it»: la documentación nueva de M-3 no usa esa frase |

## 3. Decisiones de F4-T3C1-HOST-HARDENING

### D-T3C1-01. El desconocido explícito de `GetRacksAtRisk`

- **El tipo:** `PilotRiskView` gana un campo `private readonly bool _known`. El constructor existente, que sigue siendo el único, lo pone a `true`, y se añade `public bool IsKnown => _known;`.
- **`default(PilotRiskView)`** queda con `IsKnown` falso: «no se pudo decidir», el incierto sin lista que pedía el cierre de F4-T3A. **Nunca es «nada en riesgo».** Las listas siguen sin ser null y, con `IsKnown` falso, no significan nada.
- **La documentación del puerto** lo dice, y la regla general (21) se ajusta: una excepción de `GetRacksAtRisk` se contiene como `IsKnown` falso, y su motivo va al registro por `Report`.
- **El host no cambia la lógica:**
  - el `catch` externo sigue con `Report(...)` y `return default;` (G-05);
  - la vía de reserva por borrador sigue siendo conocida, con sus inciertos;
  - una sesión sin borradores da listas vacías conocidas.
- **El endurecimiento es estructural:** el `catch` es hoy inalcanzable (H-02).
  - **Cobertura:** una guarda por cuerpo (el `catch` externo es exactamente `Report(...)` y `return default;`) y las pruebas de que `default` da `IsKnown` falso y el constructor da verdadero.
  - **Queda prohibida cualquier costura nueva en el producto** para alcanzarlo, y también la reflexión sobre campos privados del host.
- No hay constructor nuevo ni tipo nuevo.

### D-T3C1-02. Los consumidores de la UI

- **`RequestDocumentDiscard`:** con `IsKnown` falso da el aviso «No se pudo decidir qué borradores están en riesgo en este documento; no se descartó nada.». Quita una confirmación pendiente del documento y no pide confirmación.
- **`ConfirmDiscard`:** si la segunda lectura da `IsKnown` falso, no descarta, quita la confirmación y da el aviso «No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada.».
- `NoRiskText` sale solo con `IsKnown` verdadero y las dos listas vacías.
- **Solo inciertos (M-2):** la confirmación dice «¿Descartar los borradores de este documento? No se pudo decidir si tienen trabajo sin aplicar: R-4.». Con racks en riesgo e inciertos, el texto es el actual.
- **La limitación R-B2-5 de F4-T3B2 se cierra.**
- **El doble** no cambia su respuesta por defecto. Un `RisksResponder` puede devolver `default` para el desconocido.

### D-T3C1-03. Las colas (menor 4 de F4-T3A)

- **`TakeLossNotices`:** desencola cada aviso (G-08) y convierte cada uno dentro de su `try`.
  - Si la conversión de un aviso lanza: `Report("WorkspaceHostCore.TakeLossNotices", ex)` y sigue con el siguiente.
  - **Declarado:** ese aviso se pierde y solo queda en el registro.
- **`TakeCloseNotices`:** envuelve su cuerpo en un `try`. Si falla: `Report("WorkspaceHostCore.TakeCloseNotices", ex)`, devuelve una lista vacía y no vacía la cola, que se vuelve a tomar en la siguiente presentación.
- Las dos siguen sin `Authorize` (G-09).
- **Declarado:** estos `catch` son hoy tan inalcanzables como el de `GetRacksAtRisk`. Se cubren con la guarda por cuerpo. Queda prohibido alcanzarlos con conversiones de tipo sobre `LossNotices` o sobre la cola, con reflexión o con cualquier costura nueva.

### D-T3C1-04. `SessionDestroyed` (menor 6 de F4-T3A)

- `DropCloseNotice(session)` pasa **dentro del primer `try`, después de la retirada**.
- `_knownClean.Remove(session)` se queda donde está.
- Siguen 2 `try` y 2 `catch`, con el mismo orden (G-07).
- Si la retirada lanza, el aviso de veto queda, y el panel lo purga cuando su sesión ya no vive.

### D-T3C1-05. Los motivos en español (menor 6 de la revisión del intento 1 de F4-T3B1)

- **Cada literal de la tabla H-07 se sustituye por su texto en español exacto**, en el mismo sitio. Se conservan los identificadores, `ex.Message` no cambia, y `DescribeLoss` y `DescribeDifferences` no cambian.
- **G-04:** ninguna traducción contiene subcadenas prohibidas. La tabla está comprobada.
- **Sin duplicación en la UI:**
  - `RefusalOf`: para `Busy` y `NotEditable` muestra «El texto no se aplicó: » seguido del motivo. Sin motivo, «hay una lectura en curso.» o «el campo no se puede editar ahora.»;
  - `WhyOf`: para `Busy` muestra el motivo; sin motivo, «hay una lectura en curso.»;
  - los demás estados no cambian.
  - Medido: ninguna prueba fija «El texto no se aplicó: …», y la de `WorkspacePilotCloseSyncTests.cs:336` afirma el motivo del doble, que sigue apareciendo.
- **Fuera de T3C1:**
  - los textos de F3;
  - los del Plugin que llegan al campo, que pasan a T3C2 (R-C14).

### D-T3C1-06. El motivo del Error a través del puerto

- `PilotFieldView` gana un último parámetro opcional `string? errorReason = null` (el archivo del puerto es `#nullable enable`, y la CI rechaza el aviso CS8625) y la propiedad `string? ErrorReason`. Los usos de 9 argumentos no cambian.
- El host lo rellena con `DraftSession.ErrorReason`, solo en el estado Error.
- El VM del campo muestra «Error: motivo» con `ErrorReason`. Si falta, cae en el motivo actual y después en «sin detalle.».
- **Declarado:**
  - es latente hasta F4-T4, porque hoy nada pone un borrador en Error en el producto;
  - el motivo llega tal cual, sin traducir: será el del que lo produzca.
  - Las pruebas lo alcanzan con `EnterError` en el host, como `WorkspacePilotEditHostTests.cs:620`.

### D-T3C1-07. Los menores de F4-T3B2

- **M-1:** el `catch` de la rama limpia de `CloseTab` llama a `RemoveConfirmation()` antes de cerrar.
- **M-2:**
  - **Solo inciertos:** en D-T3C1-02.
  - **La retención:** `KeepsActive` gana un `out bool couldNotCheck` (`unchecked` es palabra reservada de C#), verdadero cuando `GetField` lanza.
    - La variable se declara y se asigna antes de la cadena de `&&` de `ApplySelection`, o la llamada sale de esa cadena, para evitar CS0165.
    - Con ella, los avisos de 657-660 son:
    - con `Created` o `Replaced`: «La selección abrió R en segundo plano; no se pudo comprobar si la pestaña activa tiene texto pendiente, así que se conserva.»;
    - con `R` ya abierta: «La selección de R no cambió la pestaña activa: no se pudo comprobar si tiene texto pendiente.».
  - **La confirmación de cierre sin lectura:** `CloseTab` recuerda en un campo privado que `ReadField` dio null. `Ask` lo pone, y `RemoveConfirmation` y cualquier otra petición lo quitan. El texto es neutral: «No se pudo comprobar la pestaña R; puede tener trabajo sin aplicar. ¿Descartar lo que tenga y cerrarla?».
- **M-3:**
  - **El cambio:** el constructor del VM del panel envuelve su `Present(InitialSnapshot(), null)` en un `try`, y un fallo lleva al Error contenido del panel (`EnterError`).
    - **No se toca** la llamada a `IsSessionAlive` del VM ni el `Refresh` de F3: G-11 y G-12 no cambian.
    - Construir el panel con el puerto del piloto, un veto en cola y un `IsSessionAlive` que lanza ya no lanza: el panel queda en Error con el motivo.
  - **Documentación:**
    - la de `browserSummary` (PVM 60) dice que construir el panel con el puerto del piloto puede preguntar si una sesión vive, una consulta de memoria;
    - la de `DismissNotice` dice que vuelve a pintar las marcas desde la memoria del puerto.
- **M-4:**
  - `IsDeclaration` captura el token anterior al nombre con `([\w>]+(?:\[\])*\??)$`, en lugar de `([\w>\]\?]+)$`, y exige que **contenga al menos un carácter de palabra**.
  - La expresión está medida con el motor de .NET: clasifica bien los siete casos y no cambia ningún resultado actual de las guardas de F4-T3B2.
  - **Autopruebas:**
    - cuentan como declaración: `string[] Name(`, `List<int> Name(`, `int? Name(` y `private void Name(`;
    - cuentan como llamada: `=> Name(`, `c ? Name(` y `a > Name(`.
- **M-5:**
  - cada guarda de la lista del §2.3 recibe su autoprueba en negativo;
  - las de 279-286 y 456-464 se completan;
  - la línea 221 se parte en dos.
  - **La forma pública:**
    - **Extracción:** las líneas `^\s*public\b` del cuerpo de `public sealed class WorkspaceBrowserViewModel`, sin el cuerpo de `private sealed class StoredNotice` y sin los dos records ni la línea de la clase. Cada línea se corta en el primer `=>` o `{`, con los espacios colapsados en uno y recortada;
    - **La lista exacta:** las 33 cadenas que da esa regla en `23f67f56`, escritas literales en la guarda:
      - el constructor con sus tres parámetros;
      - las propiedades `Field`, `IsAvailable`, `Tabs`, `HasTabs`, `ActiveRackId`, `LiveSurfaceCount`, `HasSurface`, `EmptyText`, `SurfaceTitle`, `SurfaceKindText`, `SurfaceViewsText`, `SurfaceStatusText`, `CanDiscardDocument`, `HasConfirmation`, `ConfirmationText`, `NoticeText`, `Notices` y `HasNotices`;
      - los métodos `Present` (5 parámetros), `ActivateTab`, `PinTab`, `CloseTab`, `DiscardField`, `RequestDocumentDiscard`, `ConfirmDiscard`, `CancelDiscard`, `DismissNotice`, `BeginFieldEdit`, `BeginFieldEditFromBox`, `BeginFieldEditFromTab`, `ReloadField` y `ReloadSummary`;
    - se comparan los conjuntos, y la guarda lleva su autoprueba;
    - **declarado:** esto fija también la superficie de F3 y de F4-T3B1, y cualquier cambio posterior será declarado.
- **M-6: sin cambio en T3C.** Ninguna de las dos subtareas añade teclado, foco ni botones con `IsDefault` o `IsCancel`, así que esas guardas no se necesitan cambiar (condición del cierre de F4-T3B2). Declarado como «no necesitado».
- **M-7:**
  - en los cuatro gestos con excepción del puerto (`CloseTab` limpia, `DiscardField`, `RequestDocumentDiscard` y `ConfirmDiscard`), una prueba afirma que la superficie no entra en el Error de `ContainFailure`: `SurfaceStatusText` no contiene el mensaje del gesto;
  - una guarda afirma cero llamadas a `ContainFailure` en los cuerpos de los seis gestos, con su autoprueba.
- **D-1:** sin cambio en T3C1.

### D-T3C1-08. Lo que no cambia y las limitaciones

- **No cambian:**
  - el Plugin, el controlador, el puente, los comandos y `BrowserSession`;
  - el XAML;
  - la API pública del navegador, y la del puerto salvo `IsKnown` y `ErrorReason`;
  - la llamada a `IsSessionAlive` y el `Refresh` de F3.
- **Limitaciones:**
  - los motivos de F3 siguen en inglés (F7);
  - el desconocido no lleva motivo en el valor: va al registro;
  - `ErrorReason` es latente hasta F4-T4;
  - un aviso de pérdida cuya conversión lanza se pierde, y solo queda en el registro.

## 4. Pruebas de F4-T3C1

- **Core, `WorkspacePilotEditHostTests.cs` (añadidas):**
  - `default(PilotRiskView).IsKnown` es falso, y el constructor da verdadero;
  - la vía de reserva da `IsKnown` verdadero con inciertos;
  - una sesión sin borradores da verdadero y vacío;
  - los motivos en español exactos de `GetField` en fallo (#18), de `SetPendingText` no editable (#20), de `CrossBoundary` desconocido (#21) y de una petición de otro documento (#2);
  - `ErrorReason` de un borrador puesto en Error con `EnterError`.
- **Core, `WorkspaceHostHardeningGuardTests.cs` (nuevo; cada guarda con su autoprueba en negativo y por cuerpo de miembro):**
  - `TakeLossNotices` con la conversión de cada aviso dentro de un `try`;
  - `TakeCloseNotices` con su cuerpo en un `try`;
  - `DropCloseNotice` dentro del primer `try` de `SessionDestroyed`, después de la retirada;
  - el `catch` externo de `GetRacksAtRisk` es exactamente `Report(...)` y `return default;`;
  - `_known` se asigna solo en el constructor de `PilotRiskView`;
  - ninguno de los 28 literales en inglés de H-07 queda en `WorkspaceHostCore.cs` ni en `WorkspacePilotSupport.cs`, y los cuatro identificadores se conservan.
- **Core, `WorkspacePilotCloseSyncGuardTests.cs`:** M-4, M-5 y la guarda de M-7, con autopruebas.
- **UI, `WorkspacePilotCloseSyncTests.cs` (añadidas):**
  - el desconocido en `RequestDocumentDiscard` y en `ConfirmDiscard`;
  - el texto de solo inciertos;
  - los dos avisos de la retención con `GetField` que lanza;
  - la confirmación de cierre sin lectura;
  - M-1, con una confirmación previa;
  - M-3: con `Record.Exception`, la construcción no lanza y el panel queda en Error con el motivo;
  - M-7: los cuatro gestos.
- **UI, `WorkspacePilotFieldViewModelTests.cs` (añadidas):**
  - «Error: motivo» con `ErrorReason`;
  - el rechazo `Busy` y el `NotEditable` sin duplicar.
- **UI, `WorkspacePilotEditPortDouble.cs`:** sin cambio de la respuesta por defecto. Acaso un ayudante que devuelva `default`.
- **Cambios declarados de pruebas existentes:** solo las guardas de M-4 y M-5 en `WorkspacePilotCloseSyncGuardTests.cs`. Ninguna prueba existente de comportamiento cambia.

## 5. Contrato propuesto de F4-T3C1 (a fijar en la emisión)

- **AllowedWriteScope (12 archivos):**
  - `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`, `WorkspaceHostCore.cs` y `WorkspacePilotSupport.cs`;
  - `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`, `WorkspacePanelViewModel.cs` (el constructor y la documentación) y `WorkspacePilotFieldViewModel.cs`;
  - `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs`, `WorkspacePilotCloseSyncGuardTests.cs` y `WorkspaceHostHardeningGuardTests.cs` (nuevo);
  - `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`, `WorkspacePilotCloseSyncTests.cs` y `WorkspacePilotFieldViewModelTests.cs`.
- **ForbiddenWriteScope:** por prefijos compactos: el resto de `src/`, de `tests/RackCad.Tests/` y de `tests/RackCad.UI.Tests/`, `docs/`, `.github/`.
- **RequiredTests:** medidos en los TRX de la CI de `23f67f56`.
  - Core: `FullyQualifiedName~RackCad.Tests.Workspace`, `MinSelected` 2331, `ExpectRed` true;
  - UI: `FullyQualifiedName~RackCad.UI.Tests.Workspace`, `MinSelected` 431, `ExpectRed` true.
- **Regla RED:** el commit RED lleva el esqueleto completo y compilable, que no lanza y no cambia el comportamiento:
  - `IsKnown => true` constante, **sin** el campo `_known`;
  - el parámetro `errorReason` aceptado, con `ErrorReason` dando null;
  - los literales sin traducir;
  - la UI y el constructor del panel sin cambio;
  - las pruebas nuevas, las guardas corregidas y la guarda nueva.

  En el RED fallan solo pruebas nuevas, por aserción. Las de construcción usan `Record.Exception`. Todas las existentes pasan.
- **Invariantes:**
  - D-T3C1-01 a 07, con el oráculo crítico (ninguna escritura authored; ninguna lectura de la intención nueva);
  - el desconocido nunca como «nada en riesgo»;
  - sin costuras nuevas en el producto;
  - sin cambios del Plugin, del XAML ni de `IsSessionAlive`/`Refresh` de F3.
- **Paradas:** las de las subtareas anteriores, adaptadas, y la regla de replanificación de F4 (F4-E31, F4-E43).

## 6. F4-T3C2-COMMAND-ADVISOR: reglas vinculantes (su paquete se fija tras cerrar T3C1)

| Regla | Contenido |
|---|---|
| R-C5 | **La lógica en Application, el Plugin como adaptador.** Los dos proyectos de prueba no referencian el Plugin, así que la lista, la confirmación, la TOCTOU, los mensajes y el envoltorio viven en clases de Application probadas con dobles. El comando y el controlador son adaptadores finos, cubiertos por guardas de texto |
| R-C6 | **El acceso al controlador sin crearlo** sustituye el «usa `IsCreated`» de R-C1, como cambio declarado. El comando no llama a `GetOrCreate` (M-12). El controlador gana un acceso estático a la instancia ya creada, o null. Sin instancia, el comando dice que no hay borradores del panel y no tiene efecto |
| R-C7 | **El desconocido (D-T3C1-01)** es un fallo explícito en el comando («No se pudo decidir qué borradores están en riesgo; no se descartó nada.») y en el envoltorio (el mensaje de pérdida lo dice). Nunca «nada en riesgo» |
| R-C8 | **`WriteMessage`:** solo se admite en el archivo del comando (`WorkspaceHostSourceGuardTests.cs:84-89`). Los mensajes del envoltorio se escriben a través de un ayudante de ese archivo, o se declara el cambio de esa guarda. **Un archivo nuevo del Plugin** rompe los censos de 9 archivos (R-C9). Se decide en el paquete de T3C2 |
| R-C9 | **Los censos de R-C3, medidos:** `WorkspaceHostSourceGuardTests.cs:49-56` y 65-73 (el comando no usa `GetObject`, `SelectImplied`, `LockDocument`, `StartTransaction`, `System.Windows`, `PaletteSet` ni `AutoCadEventBridge`); `WorkspacePaletteHostSourceGuardTests.cs:325`, su conteo de 45 pruebas en 659 y la lista de 9 archivos del Plugin en 763-777; `WorkspaceLifecycleGuardTests.cs:791-798` (9 archivos y 45 pruebas); `WorkspaceBridgeSourceGuardTests.cs:340-351` (`DeclaresEntryPoint`) y 139-175 (`GetKeywords` en `ForbiddenCallNames`); `WorkspaceSourceGuardExceptionTests.cs:106-117` y 250-258 (2 excepciones por ruta exacta); `CustomPropertiesCommandGuardTests.cs:55` y 71-75; `SelectiveEditorOpenTests.cs:566` (39 → 40). Los textos no contienen «no muestra», «aun no», «todavia no» ni «not shown» (`WorkspacePaletteHostSourceGuardTests.cs:311-318`) |
| R-C10 | **M-1 de F4-T2B2B:** `WorkspaceLifecycleGuardTests.cs:278-288` fija `_bridge.SetLifecycleAdvisor(_host);`. Cambia a una sola `.SetLifecycleAdvisor(`, con el host construido primero. Los 3 `+=` y el único `event` del controlador no cambian |
| R-C11 | **El aviso del panel nombra `RACKPANELDESCARTAR`** y cierra la desviación transitoria de D-17. Cambio declarado de `WorkspacePilotCloseSyncTests.cs:780-785` y de las pruebas que fijan ese texto (797, 805, 813, 827, 975; vista 365) |
| R-C12 | **La pérdida se escribe en el siguiente relevo** de `OnActiveSessionChanged` **con documento activo**. Ese relevo se dispara también en la destrucción, así que sin documento activo el mensaje espera. Con QUIT solo queda el registro (`Report("WorkspaceHostCore.DraftLoss")`) |
| R-C13 | **Sin subdiálogo:** el comando usa `GetKeywords` en la línea de comandos, sin ventana ni `EnterPanelModal`. Modal y foco: solo Smoke-2 |
| R-C14 | **Los motivos en inglés del Plugin que llegan al campo** se traducen en T3C2, con una tabla cerrada como H-07 y sin tocar los textos de F3 que no son del campo:<br>- `WorkspaceNavigationHost.cs:224, 226, 236 y 251`;<br>- `RackPilotIntentAdapter.cs:80, 85, 112, 118, 127, 131 y 135`. Los de 85, 131 y 135 son prefijos seguidos de `ex.Message`: se traduce el prefijo y `ex.Message` no cambia.<br>Los de `RackPilotIntentAdapter.cs:157 y 174` (`ReadPreviewWitness`) hoy no llegan, porque no tienen llamadores, y quedan fuera |

## 7. Puntos para la re-revisión

1. ¿El §8 resuelve el bloqueante y los cuatro importantes?
2. ¿La contención de M-3 en el constructor conserva G-11, G-12 y G-13?
3. ¿La tabla de H-07 es completa, exacta y libre de las trampas de G-04?
4. ¿Los mecanismos privados de M-2 (`out bool` y el campo de la confirmación) son suficientes sin inventar?
5. ¿Es satisfacible la regla RED?

## 8. Disposición de la revisión adversarial de la v1 (EMITIBLE TRAS CORRECCIONES, 1/4/10)

| Hallazgo | Disposición |
|---|---|
| B-1 (M-3 rompe la prueba de F3; hecho falso) | **Aceptado:** se contiene solo el `Present` del constructor del panel con `EnterError`; `IsSessionAlive` y `Refresh` no cambian; hechos de §2.3 corregidos; G-12 y G-13 nuevas; el alcance de PVM incluye el constructor |
| I-1 (`catch` inalcanzable) | **Aceptado:** declarado como endurecimiento estructural; cobertura con guarda por cuerpo y pruebas de `default` y del constructor; costuras y reflexión prohibidas |
| I-2 (textos sin cerrar) | **Aceptado:** tabla cerrada H-07 de 28 literales con su español exacto; identificadores conservados; `DescribeLoss` y `DescribeDifferences` excluidos; duplicación resuelta en `RefusalOf` y `WhyOf` |
| I-3 (motivos del Plugin a F7) | **Aceptado:** pasan a T3C2 (R-C14); los de F3, que no son del campo, quedan fuera |
| I-4 (M-2 sin texto ni mecanismo) | **Aceptado:** textos de 657-660 para «no se pudo comprobar», `out bool` en `KeepsActive`, campo privado de la confirmación y una pregunta neutral |
| m-1 (G-02) | **Aceptado:** hecho corregido |
| m-2 (G-04) | **Aceptado:** lista completa, sin distinguir mayúsculas y por subcadena |
| m-3 (M-6) | **Aceptado:** sin cambio en T3C, declarado como «no necesitado» |
| m-4 (M-4 y arrays) | **Aceptado:** regla y autopruebas con `string[]` y `List<int>` |
| m-5 (forma pública) | **Aceptado:** lista exacta de 33 firmas, método y declaración |
| m-6 (prueba de M-7) | **Aceptado:** cuatro gestos con excepción del puerto y guarda de seis |
| m-7 (colas) | **Aceptado:** conversión por aviso con `try`, y la pérdida declarada; referencia corregida a G-09 |
| m-8 (`ErrorReason` latente; regla del puerto) | **Aceptado:** declarado latente y sin traducir; la regla 21 se ajusta; `IsKnown` falso es el incierto sin lista |
| m-9 (§6) | **Aceptado:** censos de 9 archivos y 45 pruebas, textos prohibidos completos, prohibiciones del comando y R-C6 declarado como sustituto de R-C1 |
| m-10 (precisiones) | **Aceptado:** `ConfirmDiscard` 396-507; RED sin `_known` y con `Record.Exception`; el aviso de M-3 pasa a ser el Error del panel; `MinSelected` medido en los TRX de la CI de `23f67f56` |

## 9. Disposición de la re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/1/6)

| Hallazgo | Disposición |
|---|---|
| N-1 (la regla de M-4 contradecía su autoprueba `string[]`) | **Aceptado:** la expresión medida `([\w>]+(?:\[\])*\??)$` con al menos un carácter de palabra, y las autopruebas de los siete casos |
| N-2 (`unchecked` es palabra reservada; CS0165) | **Aceptado:** `couldNotCheck`, asignada antes de la cadena de `&&` |
| N-3 (CS8625) | **Aceptado:** `string? errorReason` y `string? ErrorReason` |
| N-4 (los `catch` de las colas son inalcanzables) | **Aceptado:** declarado; cobertura por guarda; conversiones de tipo, reflexión y costuras prohibidas |
| N-5 (método de la forma pública) | **Aceptado:** regla de extracción fijada y lista de las 33 firmas |
| N-6 (R-C14 incompleta) | **Aceptado:** añadidas 85, 131 y 135; 157 y 174 fuera porque no llegan |
| N-7 (precisiones) | **Aceptado:** dos identificadores fijados por guardas y dos por decisión; la frase prohibida de G-13; «Hay un comando en curso.»; G-04 se queda en la regla más estricta; la lista de trampas de la UI añadida |
| m-4 y m-5 de la v1 (resueltos en parte) | **Cerrados** por N-1 y N-5 |
