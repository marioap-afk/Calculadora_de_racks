# I-16 — Registro de validación manual en AutoCAD

> Evidencia de la validación manual del dueño sobre I-16 ([contrato](I-16-refactor-draw-services.md),
> [línea base](I-16-draw-services-baseline.md)). Registro factual del resultado proporcionado por el dueño;
> no incluye capturas ni detalles no proporcionados.

## Artefacto validado

| Campo | Valor |
|---|---|
| AutoCAD | 2025 |
| Rama | `refactor/draw-services` |
| Commit | `2d276a61f24bcc6fdf8451412bfd387e63d2bf6f` |
| Worktree | I-16 (`…-I-16-draw-services`) |
| DLL (Debug) | `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll` |
| SHA-256 del DLL | `6AEF0F4D5A49B89F6F5AAA35D4E287715473641E81D379B4BC671B55CC52906B` |
| Build local | suite completa verde; UI Debug 0 errores/0 advertencias; Plugin Debug 0 errores (solo `MSB3277` conocidas) |
| CI de rama | verde (Tests + Build UI + Build Plugin without AutoCAD) sobre el commit validado |

## Matriz ejecutada y resultado

| Familia | Alcance recorrido | Resultado |
|---|---|---|
| Selectivo | vista frontal + planta; edición del sistema con ambas vistas redibujadas (nombres/sufijos, geometría, único refresco final, sin vistas fantasma, edición posterior) | **Aprobado** |
| Dinámico | lateral + frontal de entrada + frontal de salida + planta; escenario con `postIndex`; edición multivista (entrada/salida y sufijos correctos, corte correspondiente a `postIndex`, único `Regen` final, sin bloques duplicados ni fantasma) | **Aprobado** |
| Cama | creación + edición mediante `RedrawInPlace` (estructura/geometría, regeneración individual, firma y flujo sin cambios visibles) | **Aprobado** |
| Cabecera | cabecera lateral + cabecera planta; edición de vistas (conteos reportados, geometría, sufijo de planta, único `Regen` final multivista) | **Aprobado** |
| Cancelación del jig | inserción iniciada y cancelada (sin referencia insertada, sin definición principal huérfana, sin definición anidada fantasma, sin error visible) | **Aprobado** |
| Persistencia y edición posterior | bloque creado y editado (re-editable, identidad y payload conservados, sin bloque paralelo por pérdida de GUID o nombre) | **Aprobado** |

## Observaciones

Ninguna.

## Resultado global

**Aprobado.**
