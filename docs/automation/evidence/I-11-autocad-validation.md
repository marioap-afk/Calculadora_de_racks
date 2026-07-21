# I-11 — Validación AutoCAD

**Estado vigente: PASS (validada manualmente por el Owner).** El **2026-07-21**, el Owner ejecutó la matriz
manual en AutoCAD 2025 y confirmó explícitamente que **todos los escenarios obligatorios pasan**. El registro del
intento automatizado **BLOCKED** anterior se conserva íntegro más abajo como historial (no se elimina).

**Aprobación final del Owner (2026-07-21):** el Owner aprobó explícitamente la implementación final de I-11 —
la exclusión de `RackFrameProjectDocument`, la matriz AutoCAD completa (incl. B5/B6/S7), la **owner-validation
final (PASS)**, la **integración serializada** en `main`, la actualización de `HANDOFF.md`/`ROADMAP.md` y la
limpieza segura posterior al merge. Estado previo a la integración: **`integration-ready`**.

> Nota de trazabilidad: el ejecutor documental de esta sesión no volvió a abrir AutoCAD. El resultado PASS proviene
> **exclusivamente** de la confirmación manual explícita del Owner; no se fabricaron capturas, rutas de DWG ni
> payloads que el Owner no haya proporcionado.

## Validación manual posterior del Owner — PASS

| Campo | Valor |
|---|---|
| Validador | **Owner** |
| Fecha | 2026-07-21 |
| Aplicación | AutoCAD 2025 |
| Carga | `NETLOAD` del DLL Debug del worktree de I-11 |
| DLL utilizado | `C:\Users\alejandra-mendoza\.claude\worktrees\architecture-persistencia-uniforme\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` |
| Punta de código validada | `eea1c1113dd8a33e33fa31dd61720c24c844ad4f` |
| HEAD documental (registro) | `e5e839a4bdb827db84f974eb3b8b09bc3c6c4844` (los commits posteriores a `eea1c11` son solo docs; el DLL es idéntico en código) |
| Confirmación | El Owner confirmó explícitamente que **todos los escenarios obligatorios pasaron**. |

### Matriz — resultado del Owner

| ID | Familia | Escenario | Resultado | Evidencia | Observaciones |
|---|---|---|---|---|---|
| A1 | cama/selectivo/dinámico/cabecera | Inserción fresca | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| A2 | cama/selectivo/dinámico/cabecera | Edición simple | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| A3 | cama/selectivo/dinámico/cabecera | Metadata desconocida del envelope | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| A4 | cama/selectivo/dinámico/cabecera | Duplicación (RACKDUPLICAR) + GUID | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Original conserva GUID; copia recibe GUID nuevo. |
| A5 | cama/selectivo/dinámico/cabecera | Reapertura | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| M1 | selectivo/dinámico/cabecera | Metadata distinta por vista | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Cada vista conserva la suya. |
| M2 | selectivo/dinámico/cabecera | Vista nueva ligada | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Hereda del iniciador. |
| C1 | cama | FlowBedDocument desconocido | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| B4 | cama | Cama DWG → biblioteca | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| B6 | cama | Cama biblioteca → DWG | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| D1 | dinámico | Wrapper interior compatible | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| B5 | dinámico | Dinámico biblioteca → DWG | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| S7 | dinámico | MAJOR interior incompatible (bloqueante) | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Aborta sin actualización parcial; mensaje visible. |
| S7' | dinámico | Kind interior incorrecto (bloqueante) | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Aborta sin modificar ninguna vista. |
| H1 | cabecera | Wrapper interior compatible | PASS | Confirmación manual explícita del Owner en la sesión de validación. | — |
| H2 | cabecera | MAJOR interior incompatible | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Aborta antes de modificar la primera vista. |
| H3 | cabecera | Cabecera de biblioteca desnuda (fuera de alcance aprobado) | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Abre/guarda; no fabrica metadata de RackProjectDocument (owner-decision aprobada). |
| — | selectivo | Envelope desconocido por vista / versión no degradada / multivista / vista nueva / dup / reapertura / wrapper biblioteca | PASS | Confirmación manual explícita del Owner en la sesión de validación. | Interior `SelectivePalletDesignDocument` no es límite; sin preservación recursiva. |
| — | todas | GUID, duplicación, reapertura, `RACKLISTA`, `RACKBOMTOTAL` | PASS | Confirmación manual explícita del Owner en la sesión de validación. | BOM/RACKLISTA sin cambios inesperados; Xrecord bajo la misma clave. |

**Totales (Owner):** PASS (todos los escenarios obligatorios) · FAIL 0.

---

## Historial — intento automatizado previo (BLOCKED)

> Registro conservado del intento automatizado anterior a la validación del Owner. En ese intento la matriz **no se
> ejecutó**; todas las filas figuraban como `NOT RUN`. Se conserva por trazabilidad; **queda superado** por la
> validación manual del Owner registrada arriba.

## Contexto verificado (real)

| Campo | Valor |
|---|---|
| Iniciativa | I-11 — Persistencia uniforme |
| Rama | `architecture/persistencia-uniforme` |
| HEAD | `0dcb1655b8d6a931410dfe07fdad15fa7ac3055c` (docs del cierre del gate owner-decision) |
| Punta de código | `eea1c1113dd8a33e33fa31dd61720c24c844ad4f` (la diferencia `eea1c11..0dcb165` es **solo docs**; el DLL es idéntico en código) |
| base `origin/main` | `6e18874072d8eb9db01e1c302c89c094b6af804a` (sin avance) |
| PR | #2, draft |

## Preflight y build (ejecutados realmente)

- `git status --short`: limpio; `branch`: `architecture/persistencia-uniforme`; HEAD = `origin/architecture/persistencia-uniforme`; sin operación Git en curso; `origin/main` sin avance; PR #2 draft. **PASS.**
- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj`: **791/791** verdes.
- `dotnet build RackCad.sln -v:minimal`: 0 errores (solo `MSB3277`).
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug`: **0 advertencias, 0 errores**.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug`: **0 errores** (solo los dos `MSB3277` conocidos de AutoCAD).
- DLL Debug generado: `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll` (recompilado desde el worktree de I-11; corresponde al HEAD actual).

## Bloqueo exacto

La matriz de las secciones §4–§11 exige, para **cada** escenario, una sesión **interactiva** de **AutoCAD
2025 con licencia** operada por una persona:

1. **GUI/WPF y jigs obligatorios.** `RACKCAD` abre la ventana WPF `RackMainMenuWindow`; la inserción usa
   *jigs* (colocación con el ratón en el modelspace); `RACKEDITAR`, `RACKDUPLICAR` y los editores por familia
   (`RackFlowBedWindow`, `RackDynamicSystemWindow`, `RackSelectiveWindow`, `RackFrameConfiguratorWindow`) son
   ventanas WPF modales. Nada de esto se ejecuta headless (`accoreconsole` no tiene GUI/WPF); requiere clics de
   un operador humano.
2. **Juicio visual de geometría.** Varios pasos exigen confirmar que el rack "se dibuja correctamente" y que la
   geometría queda intacta — verificación visual en pantalla.
3. **Inyección/inspección manual de Xrecord** (payload *before/after*), capturas de pantalla y archivos DWG de
   prueba fuera del repositorio.
4. **Validador humano nombrado.** El gate AutoCAD/owner-validation es, por proceso (WORKFLOW §6), responsabilidad
   del **dueño** en la aplicación real.

El ejecutor de esta sesión es un agente automatizado sin acceso a una sesión interactiva de AutoCAD ni a un
validador humano; **no puede** realizar, presenciar ni atestiguar estos pasos, ni producir evidencia auténtica.
Declarar cualquier `PASS` sería fabricar resultados, prohibido por el prompt. Por eso el resultado es **BLOCKED**
(§13, causa *entorno / falta de acceso*), sin simular ninguna prueba.

## Matriz (todos NOT RUN)

| ID | Familia | Escenario | Resultado | Evidencia | Observaciones |
|---|---|---|---|---|---|
| A1 | cama/selectivo/dinámico/cabecera | Inserción fresca | NOT RUN | — | Requiere AutoCAD GUI + jig. |
| A2 | cama/selectivo/dinámico/cabecera | Edición simple | NOT RUN | — | Requiere editor WPF. |
| A3 | cama/selectivo/dinámico/cabecera | Metadata desconocida del envelope | NOT RUN | — | Requiere inyección de Xrecord + RACKEDITAR. |
| A4 | cama/selectivo/dinámico/cabecera | Duplicación (RACKDUPLICAR) | NOT RUN | — | Comando interactivo. |
| A5 | cama/selectivo/dinámico/cabecera | Reapertura | NOT RUN | — | Cierre/apertura de AutoCAD. |
| M1 | selectivo/dinámico/cabecera | Metadata distinta por vista | NOT RUN | — | Multivista GUI. |
| M2 | selectivo/dinámico/cabecera | Vista nueva ligada | NOT RUN | — | Inserción durante edición. |
| C1 | cama | FlowBedDocument desconocido | NOT RUN | — | — |
| B4 | cama | Cama DWG → biblioteca | NOT RUN | — | — |
| B6 | cama | Cama biblioteca → DWG | NOT RUN | — | — |
| D1 | dinámico | Wrapper interior compatible | NOT RUN | — | — |
| B5 | dinámico | Dinámico biblioteca → DWG | NOT RUN | — | — |
| S7 | dinámico | MAJOR interior incompatible (bloqueante) | NOT RUN | — | Verificación de aborto sin actualización parcial. |
| S7' | dinámico | Kind interior incorrecto (bloqueante) | NOT RUN | — | — |
| H1 | cabecera | Wrapper interior compatible | NOT RUN | — | — |
| H2 | cabecera | MAJOR interior incompatible | NOT RUN | — | — |
| H3 | cabecera | Cabecera de biblioteca desnuda (fuera de alcance aprobado) | NOT RUN | — | RackFrameProjectDocument excluido (owner-decision aprobada). |

**Totales:** PASS 0 · FAIL 0 · BLOCKED (matriz) · NOT RUN 17.

## Nota sobre cobertura automatizada (no sustituye la matriz)

Los **mecanismos** puros de persistencia que la matriz valida en la app están cubiertos por la suite
(`RackCad.Tests`, 791/791): política de versión sin degradar, composición del envelope, preservación por los
cuatro límites, resolución/preflight discriminado (`IncompatibleMajor`/`WrongKind` ⇒ aborta sin fuentes), y las
composiciones biblioteca→DWG. Esto **no** sustituye la matriz AutoCAD: el **cableado Plugin/WPF end-to-end** y el
**juicio visual** solo se validan en la aplicación real, por el dueño.

### Estado en aquel intento (histórico)

`state: waiting` · `gate: autocad`. AutoCAD **no** validado en ese intento automatizado. **Superado** por la
validación manual del Owner (PASS) registrada al inicio de este documento.

---

## Estado vigente

`state: waiting` · `gate: **owner-validation**`. **AutoCAD validado (PASS) por el Owner** el 2026-07-21;
owner-validation final pendiente; I-11 **no** integrada.
