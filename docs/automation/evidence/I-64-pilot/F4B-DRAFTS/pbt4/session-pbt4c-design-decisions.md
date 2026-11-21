# PB-T4c: decisiones de diseño de la sesión antes de la v5.0 (I-64 F4B)

**Fecha:** 2026-11-21.

**Contexto:** al preparar la v5.0 del paquete de PB-T4, el redactor volvió a medir PB-T4c sobre el cierre de PB-T4b (`6adff90d`, F4B-E53) y encontró una prueba fuera de la lista que vio el Owner: CT-PBT4B-20. Se detuvo y la llevó a la sesión, que la llevó al Owner. El Owner eligió «Añadirla a la lista (Recomendado)» (`owner-pbt4c-list-3-decision.md`): la lista de PB-T4c pasa de 2 a 3. La sesión decide además lo siguiente. No son decisiones del Owner: no cambian la lista de 3 ni el comportamiento visible que el Owner decidió. Se declaran en la emisión de PB-T4c.

Este registro sigue la forma del de PB-T4b (`session-pbt4b-design-decisions.md`, D-S1 a D-S6), que no se toca porque ya está custodiado con la emisión de PB-T4b.

## D-S7. La costura de CT-PBT4C-09: `HeaderConfiguratorDialog`

**El hecho:** CT-PBT4C-09 lee `PendingWork()` de la ventana de Push Back con un módulo escenificado y sin el configurador real. La v4.9 nombraba como costura `HeaderConfiguratorPresenter` con `ConfigureModuleHeaderButton` (`PushBackHeaderAuthorityWindowTests.cs:99-106`). Medido en `6adff90d`, la ventana tiene dos costuras:
- `HeaderConfiguratorPresenter` (W:2373), un `Action<RackFrameConfiguratorWindow>`. Recibe la ventana del configurador ya construida, así que la prueba construiría un `RackFrameConfiguratorWindow` real;
- `HeaderConfiguratorDialog` (W:1488), un `Func<RackFrameConfiguration, RackFrameConfiguration>`. `ConfigureModuleHeader` lo llama en lugar de `ShowHeaderConfigurator` (W:1901-1903), y no se construye ninguna ventana del configurador. Ya la usa `PushBackWorkspaceApplyCloseTests` (PB-T2b).

**La decisión:** CT-PBT4C-09 escenifica el módulo con `HeaderConfiguratorDialog` y `ConfigureModuleHeaderButton`.

**Por qué:** la otra costura construiría un `RackFrameConfiguratorWindow` real, y la parada C05 de los contratos de PB-T4 lo prohíbe («a CT would need … the real configurator»).

**Lo que no cambia:** la pregunta de «Cerrar» y su texto (D-PBT4-17), ni la ventana: W solo cambia `PendingWork()` y su literal nuevo.

## D-S8. Los filtros de las clases nuevas de PB-T4c, por clase exacta

**El hecho:** la v4.9 pedía un filtro por prefijo, `PushBackEditAll`, con `ExpectRed` true. Ese prefijo selecciona también los tres equivalentes de la lista de 3 si su nombre empieza por `PushBackEditAll`. Los equivalentes pasan en el RED, porque sus `Skip` y su texto van en el RED, y una prueba que pasa en un filtro con `ExpectRed` true es la parada C04.

**La decisión:** cada clase nueva de PB-T4c tiene su propio filtro, por su nombre exacto: las clases de las CT y los equivalentes de la lista (tres en Core y, desde `owner-pbt4c-list-4-decision.md`, uno en UI).
- Se compone como `FullyQualifiedName~RackCad.Tests.<Clase>` o `FullyQualifiedName~RackCad.UI.Tests.<Clase>`.
- Con el mismo criterio, **ningún filtro nuevo selecciona una clase con el otro `ExpectRed`, ni ningún filtro heredado selecciona una clase nueva** (medido por texto sobre los nombres completos; la v5.0 lo deja escrito en su §6.3.1).

**Lo que no cambia:** la lista (de 3 y, tras la decisión del Owner, de 4), las CT y lo que fija cada `ExpectRed`.

## D-S9. El host del editor completo se llama `IWorkspaceFullEditHost`

**El hecho:** la precondición 6 de la v5.0 (en la copia exportada `/d/r5c`, con el nombre que el paquete fijaba desde la v4, `IWorkspaceFullEditorHost`) hizo caer una prueba que no está en la lista, en el RED y en el GREEN:
- **la guarda:** `WorkspaceModelBoundaryTests.ThereIsNoPerTabStateOrPerTabSubscriptions` (`tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs:93-104` en `6adff90d`). Recorre por reflexión los tipos del espacio `RackCad.Application.Workspace`;
- **la expresión que casaba:** `Subscription|Editor|Draft`, sin distinguir mayúsculas (:101), contra el nombre del tipo. Casó con `IWorkspaceFullEditorHost`, por «Editor»;
- **lo que dio:** Core 19 608 = 19 146 `Passed` + 1 `Failed` + 461 `Skipped`, en el RED y en el GREEN. El paseo de guardas no la vio, porque lee literales de texto y esta guarda mira los tipos por reflexión.

**La decisión:** el host del editor completo se llama **`IWorkspaceFullEditHost`**, en `src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs`. Con él cambian su asignación en WHC, **`SetFullEditHost`**, y su campo, **`_fullEditHost`**. La clase anidada de WCo, `FullEditorModal`, se queda: es del espacio del Plugin, que esa guarda no recorre.

**Es el precedente de D-S2 y D-S6:** un diseño que deja en verde una guarda que no está en la lista, sin tocarla. No cambia la lista ni el comportamiento visible.

**La medida** (en la copia exportada `/d/r5e`, con las piezas del GREEN y del esqueleto con los tres nombres cambiados y nada más): la compilación da 0 errores y 0 `warning CS`, y
- el RED, sin las CT: Core **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`**, y los filtros de UI, 0 `Failed`;
- el GREEN, sin las CT: Core **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`**. En los filtros de UI solo cae CT-PBT2B-06, la segunda caída de la precondición 6, que el Owner añadió a la lista (`owner-pbt4c-list-4-decision.md`).

El registro de la medida: `pbt4/v50/variant-rename/driver-redgreen-r5e.txt` y `pbt4/v50/variant-rename/out-redgreen-v50-r5e/`, y su resumen en `pbt4ccc/compile/compile-check.md`.

## D-S10. CT-PBT4C-09 cubre lo que se puede alcanzar

**El hecho:** D-PBT4-17 decía «uno solo: la de hoy», y CT-09 tenía un caso `solo-modulo` en WORKSPACE. La sonda de UI sobre el GREEN literal (en la copia exportada `/d/r5f`, `pbt4c-probe-close-ui.cs`; `pbt4ccc/compile/out-probes-v50b-r5f/probe-ui-lines.txt`) lee `PendingWork()` de ventanas reales, sin mostrarlas:

| Caso | Ventana | Gesto | ¿Módulo escenificado? | Pregunta |
|---|---|---|---|---|
| `ninguna` | WORKSPACE, `pb-01-base` | ninguno | no | ninguna |
| `solo-conmutador` | WORKSPACE, `pb-31-v2-modulos` | `AdvancedModulesToggle` | no | ninguna |
| `solo-seleccion` | WORKSPACE, `pb-31-v2-modulos` | `SelectModule(w, "M3")` | no | ninguna |
| `cabecera` | WORKSPACE, `pb-31-v2-modulos` | la cabecera escenificada con `HeaderConfiguratorDialog` (D-S7) | sí | la combinada |
| `stage-module-edit` | WORKSPACE, `pb-01-base` | `StageModuleEdit` | sí | la combinada |
| `solo-sin-aplicar` | WORKSPACE, `pb-01-base` | `ToleranceBox` a 4.5 | no | la de hoy, «sin aplicar» |
| `cabecera-y-sin-aplicar` | WORKSPACE, `pb-31-v2-modulos` | la cabecera y `ToleranceBox` | sí | la combinada |
| `clasico-cabecera` | CLASSIC, `pb-31-v2-modulos` | la cabecera | sí | la del módulo, la de hoy |

**Lo que dice:** en WORKSPACE, escenificar un módulo deja siempre cambios sin aplicar, porque el estado editable de la ventana lleva la autoridad de módulos (W:177: «el editor, la cola, la autoridad de modulos y los portadores»). Así que en WORKSPACE «solo un módulo escenificado» no se puede alcanzar. La rama de `PendingWork()` que pregunta solo por el módulo se alcanza en CLASSIC, donde no cambia nada.

**La decisión:** CT-PBT4C-09 cubre lo que se puede alcanzar: `ambas` (la cabecera escenificada de D-S7), `solo-sin-aplicar`, `ninguna` y `clasico-modulo`. D-PBT4-17 lo declara, con esta medida.

**Por qué es de la sesión:** no cambia la lista ni el GREEN. La decisión del Owner (A-T2b-OWNER-01, F4B-E38) se cumple con la pregunta combinada.
