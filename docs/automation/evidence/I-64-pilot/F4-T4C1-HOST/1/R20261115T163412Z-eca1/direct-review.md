# I-64 F4-T4C1-HOST, intento 1: revisión directa de la sesión

**Objeto:** la entrega del Worker `R20261115T145157Z-efec`:
- BaseSha `44eafbc1`, el commit de emisión del contrato;
- RED `8e57061e`;
- GREEN `271a045c`.

Frente al contrato de F4-T4C1-HOST, el paquete v5 de F4-T4C (parte [1]), IC-09 y los textos ratificados por el Owner (D1).

**Método:**
- comprobaciones mecánicas por programa: las dos listas de alcance, los seis textos byte a byte, el GREEN sin `tests/` y la pureza del corredor;
- lectura completa del diff de producto: `HOSTRUN`, `ADAPTER`, el núcleo, `EventBridgeCore`, `WorkspaceSession`, `AutoCadEventBridge`, `WorkspaceController`, el escritor y la corrida;
- lectura dirigida de las pruebas que cubren la cota del billete.

## 1. Comprobaciones mecánicas

| Comprobación | Resultado |
|---|---|
| `git diff --name-only BaseSha CurrentSha` frente al `AllowedWriteScope` (PD-F4-07) | **iguales**: 30 de 30, sin rutas de más ni de menos |
| Rutas bajo el `ForbiddenWriteScope` (prefijos y archivos exactos) | **ninguna** |
| `git diff --name-only RedSha CurrentSha -- tests` | **vacío** |
| Los seis textos de `PushBackPilotHostTexts` frente a `host-texts.txt` | **los seis coinciden** byte a byte |
| `HOSTRUN`: Transaction, Task, async, await, Dispatcher, Timer, Autodesk, Database, LockDocument | **0 apariciones** de cada uno |
| CI exacta | GREEN `43733257361` 4/4 en success; RED `43731731401` con Core en failure, UI en success, build-ui en success y build-plugin omitido |
| Filtros (TRX de la CI) | GREEN: Core Workspace 3385/3385, UI Workspace 601/601 y Core completo 18966/18966. RED: Core Workspace 216 fallos, todos dentro del filtro |

## 2. Lectura del producto

**Cumplen el paquete:**
- **el adaptador** (`ADAPTER`):
  - `SynchronizationContext.Current` antes de la llamada;
  - una sola `ExecuteInCommandContextAsync`, con una lambda total que devuelve `Task.CompletedTask`;
  - `IsCompleted` con el salto directo;
  - una sola `OnCompleted`;
  - el salto por `Post` en una ayuda única;
  - la continuación y la acción totales;
  - `Target` con su único `_document`, `Lock`, `CreateWriter` y `Retire`;
- **el corredor** (`HOSTRUN`):
  - el cerrojo `Pending`, `Started` y `Ended`;
  - los pasos 1 a 8 en su orden, con `DocumentMismatchReason` en el paso 1;
  - el `finally` exterior con exactamente una llamada terminal;
  - `Retire` solo con un escritor creado;
  - el desbloqueo en su propio `try`;
  - `Completed` una vez;
- **el escritor** (D-1): dos campos, `_spent`, un `Begin` que lanza si ya se usó y `Retire`. `MdiActiveDocument` desaparece;
- **el ámbito propio** (IC-09):
  - `_ownPending` y el drenaje, con lo propio solo para invalidar;
  - el cierre en el primer fin, cancelación o fallo de comando (que cuenta como propio), en `SetQuiescent` antes de su comparación, en `CloseOwnMutation` y en `Degrade`;
  - `NoteForeignCommandStart` en el manejador, condicionada a `EXECUTEFUNCTION` y después de la decisión existente;
- **R-T4-07:** los dos descartes con `Busy`, y `DraftRegistry` sin cambios;
- **I-01 y M-01:** en la corrida y en `TryUpdatePilot`;
- **la frontera:** `PilotBoundaryKind.Update` y su traducción en `CrossBoundary`.

## 3. Hallazgos

| Id | Gravedad | Contenido | Evidencia |
|---|---|---|---|
| D-1 | **Importante** | **Un billete que pasó por `Execute` no caduca nunca.** `Execute` pone `InExecute = true` y nada lo devuelve a `false`, y `ExpireStaleUpdate` sale con `null` para todo billete con `InExecute`. Si la compleción no llega, la sesión queda en `Busy` para Actualizar y para los dos descartes hasta su destrucción. Contradice D-T4C-02 («uno terminado cuya compleción no llegó, también caduca») y D-T4C-04 («si el salto falla … el billete lo libera su caducidad»). Ninguna prueba lo ejerce: la de `WorkspacePilotUpdateRequestTests.cs:471` solo mira un comando ajeno durante `Execute` | `WorkspaceHostCore.cs:1436`, `:1604`; `WorkspacePilotUpdateRequestTests.cs:471-500` |
| D-2 | Menor | **`TryOpenTarget` del adaptador devuelve dos motivos literales** («El documento ya no está abierto.» y «El documento ya no es el activo.») que el corredor descarta, porque usa `DocumentMismatchReason`. Es texto muerto, sin efecto | `RackPilotUpdateCommandContext.cs:71`, `:79`; `PushBackPilotHostRun.cs`, paso 1 |

**Veredicto:** con D-1, la entrega **no se puede cerrar** según la regla D2 del Owner. La revisión independiente califica D-1 como B-01, bloqueante. El análisis conjunto adopta esa calificación, la más estricta.
