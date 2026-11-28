# Mapa 3: `GetRacksAtRisk`, colas de avisos y textos (explorador, HEAD `7895f397`)

## 1. `PilotRiskView`, el puerto y los reenvíos

- **El tipo:** `IWorkspacePilotEditPort.cs:272-289`, `readonly struct`.
  - Un solo constructor: `(rackIds, uncertainRackIds)`, que copia con `ToArray` y lanza con null.
  - `default` equivale a dos listas vacías, igual que «nada en riesgo».
- **El puerto, `:66`:** «Memory only: never throws».
- **El host, `WorkspaceHostCore.cs:765-807`:**
  - el `try` interno (770-777) llama a `_drafts.RacksAtRiskOf`; si falla, decide borrador por borrador y lleva a uncertain los que son null o lanzan;
  - el `catch` externo (802-806) hace `Report` y `return default`: es el fallo que se lee como «nada en riesgo».
- **Reenvíos:** `WorkspacePilotSupport.cs:190` y `WorkspaceController.cs:263`.

## 2. Los llamadores y las pruebas del vacío

- **VM:**
  - `RequestDocumentDiscard`, 353-388: en 372, con las dos listas vacías publica `NoRiskText` (86).
  - `ConfirmDiscard`, 396-472: en 429-433, si la segunda lectura llega vacía, quita la confirmación y publica `NoRiskText`.
- **Pruebas que fijan el comportamiento actual:**
  - el doble, 82-83 y 262-268: por defecto, dos listas vacías;
  - `WorkspacePilotCloseSyncTests` 597-610, 711-719, 748-753, 1049-1070, 1073-1099 y 1302;
  - `WorkspacePilotCloseSyncViewTests` 296;
  - `WorkspacePilotEditHostTests` 1269-1276 (`default(PilotRiskView)` no tiene listas null), 1317-1318 (vacía con `faults == 5`), 790, 1254-1257 y 1428.

## 3. Colas, `SessionDestroyed` y `AdviseClose`

- `TakeLossNotices`, 809-819: sin `try`; desencola uno a uno.
- `TakeCloseNotices`, 821-826: sin `try`.
- `DropCloseNotice`, 942.
- `AdviseClose`, 912-934:
  - la vía Veto llama a `QueueCloseNotice(..,false)`;
  - la vía Allow, a `DropCloseNotice`;
  - el `catch`, a `QueueCloseNotice(session, [], true)`.
- `SessionDestroyed`, 947-971:
  - en 950, `_knownClean.Remove` y `DropCloseNotice` quedan fuera de todo `try`, antes de la retirada;
  - después hay dos `try`: la retirada (952-959) y el aviso de pérdida (961-970).
- **Guardas:**
  - `WorkspaceLifecycleGuardTests.cs:376-389`: exactamente 2 `try` y 2 `catch (Exception ex)`, en el orden `_knownClean.Remove(` → `RetireKnownSafe(` → `RetireConservative(` → `HasLoss` → `_lossNotices.Enqueue(` → `"WorkspaceHostCore.DraftLoss"`. `DropCloseNotice` no entra en el orden, así que puede moverse dentro de un `try` existente; un tercer `try` rompe 383-384.
  - Mismo archivo, 322-334: en `AdviseClose`, exactamente 2 `QueueCloseNotice` y 1 `DropCloseNotice`, con la forma del `catch` fijada.
  - Mismo archivo, 401-403: `TakeLossNotices` contiene `_lossNotices.Dequeue()`, el único `Dequeue` del host.
  - `WorkspacePilotEditGuardTests.cs:247-250`: los `Take*` no usan `Authorize`. Ninguna regla exige ni prohíbe que lleven `catch` (282).
- **Comportamiento:** `WorkspacePilotEditHostTests.cs:1209-1216`.

## 4. El texto del veto

- `RowOf` (1087-1100):
  - la cabecera, en 1095-1097;
  - 1098: «Descártalos con "Descartar borradores de este documento"; cada pestaña también se descarta con su "Cerrar".»;
  - 1093: la fila de otra sesión.
- `RACKPANELDESCARTAR` no existe en `src/`.
- **Pruebas:**
  - `WorkspacePilotCloseSyncTests` 780-782 (texto exacto), 785 (`DoesNotContain("RACKPANELDESCARTAR")`, que hay que invertir), 783-784 (sin «Actualizar» ni «Aplicar»), 797, 805, 813, 827 y 975;
  - `WorkspacePilotCloseSyncViewTests` 365;
  - `WorkspacePilotCloseSyncGuardTests` 335 (el contenido del botón en el XAML).

## 5. Motivos en inglés y Error sin motivo

- **Host (`WorkspaceHostCore`):**
  - 44 «A read of the pilot is in progress.»;
  - 45 y 525 «The request does not belong to the active document.»;
  - 46 «There is no draft for the rack.»;
  - 606 «The state of the field could not be read.»;
  - 617 «The text is required.»;
  - 622, 625, 648 y 661 «The field cannot be edited now.»;
  - 689 «The boundary is not known.»;
  - 879 «The rack has no annotation scale.»;
  - `ex.Message` en 534, 633, 667, 717, 742 y 760;
  - la intención y la recarga, en 299, 318, 320, 322, 326, 350, 466, 467, 481, 485, 502, 504, 1018, 1024 y 1042.
- **Support:** 26 «The value is not a finite number greater than zero.» y 156 «A notice of the panel is being delivered.».
- **Dónde los muestra la UI:**
  - en el VM del campo: 372 y 379 (`CommittedTextReason`), `RefusalOf` 427-436, `Describe` 439-450 y 403;
  - en el VM del navegador: `WhyOf` 904-913 (llamado en 289, 468, 500 y 929), `AbortText` 831 y `LossText` 1113.
- **Error sin motivo:**
  - `ViewOfPilotDraft` (876-891) solo pasa el motivo de la escala;
  - `DraftSession.ErrorReason` (`Drafts/DraftSession.cs:308`) no cruza el puerto, porque `PilotFieldView` (constructor de 9 parámetros, 157-177) no lo lleva;
  - la UI muestra «Error: sin detalle.» (372);
  - el host, en las pruebas 619-622, solo comprueba State y CanEdit.

## 6. Guardas que rompería un campo o un estado nuevo

- **`WorkspacePilotEditGuardTests`:**
  - 54 y 97: la firma exacta, una vez;
  - 153-210: tipos valor, sin anidados, propiedades de solo lectura y campos `readonly`;
  - 75-80 y 101-105: prefijo ordenado de los enums, así que solo se puede añadir al final;
  - 73: el archivo del puerto sin `\bDraft\w*` ni `PushBack\w*`;
  - 747-753 y 802: los nombres de tipo nuevos sin `Draft`, `Editor`, `Subscription` ni `Tab`, y no son palabras de `Drafts/` ni de `BrowserSession.cs` (`Status` falla);
  - 756 y 786-787: sin `Transaction`, `Database`, `Commit(`, `RackLog`, `Provider` ni `Metric` en el puerto, el support y el host, comentarios incluidos;
  - 282-287: `GetRacksAtRisk` lleva `catch (Exception`, sin `_faulted`, `Contain(` ni `throw;`;
  - 431 y 498: delegación literal `=>_inner/_gate.GetRacksAtRisk(origin);`.
- **`WorkspacePilotCloseSyncGuardTests`:**
  - 108-113: una llamada en cada uno de los dos métodos;
  - 146: `TakeNotices` con exactamente 2 `try`;
  - 234-250: los miembros públicos y el record;
  - 263: sin `+=`.
- **El constructor de 2 argumentos** se usa en el host (772 y 800), en el doble (267), en `WorkspacePilotCloseSyncTests` 66 y en `WorkspacePilotCloseSyncViewTests` 300 y 366. Un tercer parámetro obligatorio los rompería; uno opcional, no.
- **Ningún guard** fija el número de miembros de `PilotRiskView`.
