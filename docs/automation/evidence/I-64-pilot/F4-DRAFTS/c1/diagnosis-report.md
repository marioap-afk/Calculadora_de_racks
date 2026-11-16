# I-64 F4 — Diagnóstico en AutoCAD de los hallazgos de Smoke-2 (2026-11-16)

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktree-de-diagnostico>`, `<carpeta-de-copias-de-Smoke-2>`, `<carpeta-del-DLL-de-Smoke-2>`, `<build-diag1>`, `<build-diag2>`, `<directorio-de-H0>`). El resto no cambia.


**Contexto:** orden del Owner «arregla todos los errores que encontramos» (`owner-f4-smoke2-fix-order.md`). La sesión operó AutoCAD solo para diagnosticar. Usó dos builds de diagnóstico de un worktree desechable sobre `b88dabf2` (`<worktree-de-diagnostico>`, sin commits ni rama) y copias del original de D1 en `<carpeta-de-copias-de-Smoke-2>\diag\`, que se cerraron sin guardar. No se ejecutó ningún paso de Smoke-2. I-52 recibió el aviso de inicio y el de cierre.

**Builds:**
- diag1: solo trazas DIAG en `AutoCadEventBridge`, `WorkspaceController.OnDrained` y `RackSiblingScan.Traverse` (`apply_diag.py`);
- diag2: diag1 más los tres cambios candidatos (`apply_diag2.py`).

**Trazas:** `<carpeta-de-copias-de-Smoke-2>\diag\trazas\rackcad-20261116.log.diag1` (9,0 MB) y `.diag2` (5,2 MB), sin ninguna línea ERROR.

## F4-S2-01: la causa

En el dibujo real (D1), **cada lectura del panel** provoca eventos `Database.ObjectModified` de un objeto de clase **`AssocNetwork`** (h=1AC25E, la red asociativa de AutoCAD):
- «Editar» (`PilotIntentReader.ReadOnActiveDocument`, `WorkspaceNavigationFacts.Scan` y `RackSiblingScan.Traverse`);
- «Comprobar ahora»;
- la relectura posterior a «Actualizar».

La pila los sitúa **dentro** de las propias funciones de lectura de RackCad: AutoCAD reevalúa la red asociativa al bloquear y desbloquear el documento y al confirmar la transacción.

`EventBridgeCore.cs:346-353` los encola como pista **ajena** `DatabaseObjectChanged`, porque no hay ningún ámbito de mutación propia abierto. El drenaje siguiente lleva entonces `MarkDraftsPossiblyStale` (`WorkspaceSession.cs:66-68`).

Lo mismo explica el «Limpio (posiblemente obsoleto)» que deja «Actualizar». Tras `CommandEnded(EXECUTEFUNCTION)`, que cierra el ámbito propio (IC-09), llegan más `ObjectModified` de `AssocNetwork` desde la relectura de presentación.

El dibujo sintético de H0 no tenía red asociativa, y por eso no se vio.

## F4-S2-02

Es consecuencia de S2-01. Con el filtro de diag2, Retroceso en la caja limpia borra a la primera.

## F4-S2-03: el experimento

- Con diag1: `U` deshace el «Actualizar» (`EXECUTEFUNCTION`), pero la pantalla no se refresca. Un `REGEN` lanzado como orden la refresca, pero entonces `REDO` responde «Nothing to redo».
- Con diag2: tras una orden `U`, `UNDO`, `REDO` o `MREDO` que modificó una definición de bloque, el puente programa en reposo **una regeneración por código** (`Editor.Regen()`, que no es una orden). `U` refrescó la pantalla («Regenerating model.») y `REDO` siguió funcionando: «Everything has been redone», más otra regeneración. Después, «Comprobar ahora» dio A2-01, «Limpio».

## F4-E72

Con diag2 (`system.Name = embed.Name` en `RackPilotUpdateWriter.PrepareUnits`, tras `PushBackPilotSystemRebuild.Build`), el rótulo «PB G» se conserva tras «Actualizar», con la escala nueva.

## Rendimiento

El «Actualizar» de PB G tardó unos 14 s en diag2 (`01:54:56` a `01:55:10`), con solo una traza ligera. El redibujo borra y crea miles de referencias (la traza de diag1 cuenta unos 20 000 eventos de objetos), y luego hace un `REGEN` completo (unos 1,5 s en este dibujo). Es la misma mecánica que el redibujo clásico (I-55, OV-RED). No se cambia en esta corrección y se declara.

## Lección de operación

`type` con texto largo usa el portapapeles (Ctrl+V). Si el foco no está en la línea de órdenes, por ejemplo tras usar la paleta, AutoCAD lanza `PASTECLIP`. Hay que hacer dos clics en la línea de órdenes antes de escribir. Pasó dos veces y se canceló con Esc, sin pegar nada.
