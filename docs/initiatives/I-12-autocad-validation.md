# I-12 — Registro de validación manual en AutoCAD

> Evidencia de la validación manual del dueño sobre I-12 ([contrato](I-12-versionado.md),
> [ADR-0004](../adr/0004-estrategia-de-versiones-de-autocad.md)). Registro factual del resultado
> proporcionado por el dueño; no incluye capturas ni detalles no proporcionados. El ejecutor documental
> de esta sesión no abrió AutoCAD: el PASS proviene exclusivamente de la confirmación explícita del dueño.

## Artefacto validado

| Campo | Valor |
|---|---|
| Validador | **Dueño** (Mario Pérez) |
| Fecha | 2026-07-21 |
| AutoCAD | 2025 |
| Rama | `refactor/versionado` |
| Commit validado | `5d5f0dc650bad5aa9ef24b5a49d1d47a58acebd7` |
| Worktree | I-12 (`…-I-12-versionado`) |
| Comando ejecutado | `pwsh deploy\install-bundle.ps1 -Build` (desde árbol limpio) |
| base `origin/main` | `abc1a53309f5e9c5fc6e4dd3fec54ec1992f78e6` (sin avance) |
| CI de rama | verde sobre `5d5f0dc` (run 29872302419): Tests + Build UI + UI Tests + Build Plugin without AutoCAD |

## Flujo de despliegue ejecutado y resultado

| Paso | Resultado |
|---|---|
| `dotnet publish` Release del Plugin | **Completado** |
| Advertencias del build | Solo los `MSB3277` conocidos de las referencias locales de AutoCAD |
| `deploy/verify-bundle.ps1` (fail-closed) | **105 comprobaciones aprobadas** |
| DLL del bundle vs publish (SHA-256) | **Idénticos** |
| Catálogos del bundle vs `assets/catalogs` (SHA-256) | **Idénticos** |
| DLL Autodesk en bundle / artefactos | **Cero** |
| Instalación transaccional | **Completada** en `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle` |
| AutoCAD 2025 abierto sin `NETLOAD` | Sí |
| **Autocarga del bundle** | **PASS** |
| Comando **`RACKCAD`** | **PASS** |

## Observaciones

Ninguna. La validación cubre el flujo de versionado y empaquetado de I-12 (publish canónico, verificación
fail-closed y autocarga del Autoloader); I-12 no cambia comportamiento de dibujo, por lo que no aplica la
matriz de geometría/BOM.

## Resultado global

**Aprobado.** Gate `owner-validation` satisfecho; I-12 queda `integration-ready` (la integración
serializada sigue siendo responsabilidad del dueño y no se ejecuta aquí).
