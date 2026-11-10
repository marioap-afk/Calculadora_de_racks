# I-64 — F2-T2-HOST-C1: paquete de cierre delta

Corrección estrecha autorizada por D-08 (`I-64-overnight/overnight-dispositions.md`) tras el STOP de Smoke-NAV en SN-01. Mismo formato que `F1-T3B-HOST-UI-C1`.

## Causa

- `ExecuteInCommandContextAsync` ejecuta en el comando interno `EXECUTEFUNCTION`, con dos `*Cancel*` que vacían la selección implícita.
- `SetCurrentView` se llamaba dentro de una transacción que se cerraba sin `Commit`, y la vista se revertía.

## Cambio

| Archivo | Delta |
|---|---|
| `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs` | Paso síncrono en contexto de aplicación: documento activo, sin comando en curso y editor en reposo, bloqueo, sesión, política y efectos, foco. Sin `async` ni comando interno |
| `src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs` | La transacción solo lee la extensión; la vista se escribe después de cerrarla |
| `src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs` | Texto propio para «hay un comando en curso» |
| Comentarios en `WorkspaceNavigationFacts.cs` e `IWorkspaceNavigationPort.cs` | Alineados con el mecanismo nuevo |
| Pruebas | Guardas actualizadas en `WorkspaceHostSourceGuardTests`, `WorkspaceNavigationHostTests` y `WorkspaceSourceGuardExceptionTests`; una prueba nueva en `WorkspaceNavigationViewModelTests` |

## Verificación

- RED `ce48067e` (5 fallos por aserción, 777 en verde), `85affa68` y `8050f512`; GREEN `cdef2d44` y `003be859`.
- Sobre `003be859`: Core 16320/16320; UI 1792 superadas y 17 omitidas; builds Debug de UI y Plugin sin errores (2 avisos MSB3277 de la estación); CI 42777768225 4/4.
- Revisión independiente del diff: sin defectos bloqueantes; sus dos observaciones (LISP en pausa y texto de comando en curso) se corrigieron antes del candidato.
- Comportamiento real: Smoke-NAV `R20261110T073451Z-e221`.

## Candidatos

`7bdcbfd9` = `HISTORICAL_SMOKENAV_STOP_CANDIDATE`; no se reutiliza. Candidato vigente: `003be859`.

## Ratificación pendiente

El mecanismo se aparta de la redacción de D-07, INV-40 y O4-06 («en contexto de comando»). La intención se conserva. La ratificación corresponde al Owner en la Owner Validation.
