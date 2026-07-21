# I-11 — Validación AutoCAD (registro de intento)

**Resultado de la sesión: BLOCKED.** La matriz manual de validación en AutoCAD **no se ejecutó**. No hay
ningún escenario aprobado. **AutoCAD NO validado.** El gate permanece `waiting / autocad`.

> Esta sesión NO es una declaración de aprobación. Ningún resultado de la matriz fue producido, simulado ni
> inferido. Toda fila de la tabla figura como `NOT RUN`.

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

## Estado

`state: waiting` · `gate: autocad` (sin cambio). AutoCAD **no** validado; owner-validation pendiente; I-11 **no**
integrada.
