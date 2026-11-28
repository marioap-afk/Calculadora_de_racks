# I-64 F4-T4C1-HOST, intento 2 (corrección): revisión directa de la sesión

**Objeto:** la entrega del Worker `R20261115T165352Z-3e8e`:
- BaseSha `12c248b0`, la emisión del contrato del intento 2, con el producto de `271a045c`;
- RED `1268bb9d`;
- GREEN `b2631084`.

Frente a `gate-contract-attempt2.json` (INV-F4T4C-21 e INV-F4T4C-22) y al análisis del intento 1.

## 1. Comprobaciones mecánicas

| Comprobación | Resultado |
|---|---|
| `git diff --name-only BaseSha CurrentSha` | 15 rutas, **todas del `AllowedWriteScope`** y ninguna del prohibido |
| El RED solo cambia `tests/` (`git diff BaseSha RedSha -- src`) | **vacío** |
| El GREEN solo cambia `src/` (`git diff RedSha CurrentSha -- tests`) | **vacío**. 4 archivos: `PushBackPilotHostRun.cs`, `WorkspaceHostCore.cs`, `RackPilotUpdateCommandContext.cs` y `RackPilotUpdateWriter.cs` |
| CI exacta | GREEN `43741634851` 4/4 en success; RED `43740130830` con Core en failure, UI en success, build-ui en success y build-plugin omitido |
| Filtros (TRX de la CI) | GREEN: Core Workspace 3421/3421, UI Workspace 601/601 y Core completo 19002/19002. RED: 60 fallos, todos dentro del filtro de Workspace |
| El RED discrimina | Fallan, contra el producto de `271a045c`: la prueba de B-01 (un billete terminado cuya compleción no llega caduca con el siguiente comando ajeno), las guardas del orden del vuelo de M-01, el `Lock()` nulo de M-06 (d) y los textos de M-04, entre otras. 27 de los fallos son de IG (I-01 y M-07) |

## 2. Lectura del producto (`RedSha..CurrentSha -- src`)

- **B-01:** el `finally` de `Execute` pone `InExecute = false` junto con `Finished = true`. `ExpireStaleUpdate` alcanza así el billete terminado, que solo se libera, porque `if (ticket.Finished) return null` está después de quitarlo. **Corregido.**
- **M-01:** la comprobación del vuelo sale de `TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload` del puerto. Pasa a sus cuerpos `Try*Pilot*`, después de la autorización. **Corregido.**
- **M-04:**
  - `HostNotRunReason` va sola en el corredor y en el núcleo;
  - desaparece el `" "` de CORE;
  - `TryOpenTarget` devuelve `DocumentMismatchReason`.

  **Corregido.**
- **M-05:** el cierre no aceptado, `NotePilotEdit` y `_knownClean.Remove` entran en el `try` de la respuesta. El respaldo de un cierre no aceptado es `ClosedInError(UpdateNotClosedReason, …)`. **Corregido.**
- **M-06:**
  - (a) `Execute` y `NotRun` no hacen nada con un billete terminado;
  - (b) `RequestUpdate` da `Accepted` si el billete ya empezó o terminó al fallar `TryRequest`;
  - (c) `IsAnyPilotDraftApplying` registra y responde `false`;
  - (d) un `Lock()` nulo lanza antes de `locking = false`, y da `LockFailedPrefix`.

  **Corregido.**
- **M-10:** `WRITER` pierde el `using AcApplication` sin uso, y nada más cambia en el escritor.

## 3. Hallazgos

| Id | Gravedad | Contenido |
|---|---|---|
| D2-1 | Menor | Con un `Lock()` nulo, la causa visible es `LockFailedPrefix` + «The target did not answer a lock.», una frase en inglés. Es la composición prevista para `LockFailedPrefix`, que acompaña el mensaje de la excepción, y el caso es inalcanzable con el adaptador del producto. Se declara |
| D2-2 | Menor | En `RequestUpdate`, si `TryRequest` devuelve falso y el billete ya no existe (`current == null`), el valor de inicio es `Accepted`, aunque puede no haber nada en el almacén. Solo ocurre con un ejecutor que no cumple el contrato: el corredor del producto devuelve verdadero en cuanto el callback empezó. El panel repintaría desde el estado del borrador, que es la autoridad |

**Veredicto:** el producto corrige B-01 y los menores de producto del análisis, sin regresiones visibles en el diff. No hay bloqueantes ni importantes.
