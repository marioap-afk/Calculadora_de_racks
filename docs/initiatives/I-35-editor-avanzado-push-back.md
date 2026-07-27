---
schema: rackcad-initiative/v1
id: I-35
title: Editor avanzado de modulos de Push Back
type: feature
status: implementing
branch: feature/editor-avanzado-push-back
base_branch: main
priority:
size:
depends_on: [I-15, I-17, I-18, I-21, I-30, I-32, I-33]
conflicts_with: [I-34, I-23, I-25]
context_packs: [system-dynamic-flowbed, ui-editors, architecture-kernel, persistence, delivery-validation]
automation_state_path: docs/automation/state/I-35.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Editor avanzado de modulos de Push Back

> Implementa **PB-011**, que I-32 dejo diferido en [`ideas-futuras.md`](../ideas-futuras.md) como
> «prioridad alta del Owner»: el Dinamico permite seleccionar un modulo (cabecera o separador) y
> personalizarlo —medida, cantidad de separadores, cabecera personalizada—; **Push Back no**.
>
> **Gate documental abierto.** El indice de esta carpeta exige que «un contrato no invente alcance
> ausente del ROADMAP», y **I-35 todavia no tiene fila en `docs/ROADMAP.md`**. La apertura la
> autorizo el Owner por instruccion directa, que ademas prohibio expresamente editar ROADMAP y
> HANDOFF en esta sesion (los dos son archivos calientes que WORKFLOW seccion 4.5.4 reserva para la
> sesion de integracion). La procedencia documental del alcance es **PB-011 en `ideas-futuras.md`**.
> **La fila del ROADMAP la escribe el Owner o la sesion de integracion; esta rama no la toca.**

## 1. Objetivo

Que Push Back ofrezca el mismo poder de edicion por modulo que el Dinamico —seleccionar una cabecera
o un separador de la secuencia longitudinal y personalizarlo— **sin copiar el editor Dinamico**, sin
que la personalizacion se pierda en silencio en el siguiente recalculo y sin cambiar una sola linea
de comportamiento del Dinamico ni del Selectivo.

Resultado verificable: sobre un Push Back con una cabecera personalizada, un cambio de tarima o de
fondos **conserva** esa cabecera y su procedencia, las cuatro vistas y los dos BOM la reflejan, el
round-trip la persiste, y existe una restauracion explicita que la descarta a peticion del usuario.

## 2. Problema

La auditoria de la base `7e48b5c` (esta rama, seccion 3 del estado versionado) establece siete
hechos. Los tres primeros son la carencia; los cuatro siguientes son la razon por la que la carencia
no se resuelve copiando el editor Dinamico.

1. **Push Back no tiene superficie de modulos.** `RackPushBackSystemWindow.xaml` (72 `x:Name`) no
   contiene ni un panel avanzado, ni una tabla de modulos, ni edicion de cabecera o separador: la
   busqueda de `avanzado|modulo|separador|cabecera|DataGrid` sobre ese XAML **no devuelve nada**. El
   Dinamico si: `AdvancedToggle` + `AdvancedPanel` con `ManualHeightToggle`/`ManualHeightBox`,
   `SeparatorCountBox`/`SeparatorSpacingBox`, `DerivedReinforceBox`/`DerivedReinforcementBox`,
   `ModulesGrid` y el bloque «Modulo seleccionado» (`KindBox`, `ModuleLengthBox`, `ConfigBox`,
   `ApplyModuleButton`, `EditHeaderButton`).
2. **La fundacion en Application ya existe y no tiene consumidor.**
   `PushBackEditorDesignAssembler` **ya** compone el ciclo de recalculo del Dinamico completo
   —`DynamicEditorDesignAssembler.MustRebuild` / `SnapshotHeaderFondos` / `RestoreHeaderFondos` /
   `UpdateHeaderHeightInPlace`— sobre un `PushBackEditorState.WorkingBaseline` que solo avanza en un
   `AcceptComputation` exitoso. La estructura modular de Push Back **ya se preserva**; lo que falta
   es exclusivamente la superficie que permita cambiarla.
3. **La restauracion existe sin boton.** `PushBackEditorDesignAssembler.BuildDesign(state, inputs,
   forceRebuild)` y su `Build` gemelo aceptan `forceRebuild`, pero **nadie en el Plugin ni en la UI
   lo llama con `true`**: el unico `forceRebuild: true` del repositorio es
   `RackDynamicSystemWindow.RestoreDefault_Click`. Push Back no tiene «Restaurar estandar».
4. **Toda cabecera de Push Back es «calculada».** `DynamicRackSystemBuilder.CreateHeader` fija
   `UseCalculatedHeaderConfiguration = true` y el unico codigo de produccion que lo pone en `false`
   es la ventana del Dinamico (`RackDynamicSystemWindow.EditHeader_Click` y
   `ConfigBox_SelectionChanged`). Es la premisa exacta de la nota tecnica de PB-011.
5. **La reconciliacion de modulos pierde la cabecera personalizada.**
   `DynamicEditorDesignAssembler.SnapshotHeaderFondos` guarda **solo el fondo** por ordinal, y
   `RestoreHeaderFondos` reasigna ese fondo, **fuerza `UseCalculatedHeaderConfiguration = true`** y
   **reconstruye la configuracion desde la fabrica**. Un cambio de tarima o de fondos revierte
   cualquier cabecera personalizada a calculada. Hoy es inerte en Push Back porque ninguna cabecera
   suya es personalizada (hecho 4); **la sesion en que PB-011 se implemente es la sesion en que ese
   camino se vuelve real**. Esta es la inconsistencia de PB-011, registrada en `ideas-futuras.md`.
6. **El clon del resolver no es el clon canonico de I-17.**
   `DynamicRackSystemResolver.CloneHeader` es `RackFrameProjectDocument.FromConfiguration(...)
   .ToConfiguration()`, no `RackFrameProjectStore.DeepCopy`. El documento **no persiste**
   `RackFrameConfiguration.Exceptions` (I-17 lo declara estado runtime), asi que ese round-trip las
   **descarta**. `PushBackEditorDesignAssembler.CopyStructureSystem` recorre exactamente ese camino
   (`Snapshot` + `Resolve`) en **cada** recalculo sin cambio estructural. Tambien inerte hoy por el
   hecho 4, y tambien real desde el momento en que exista una cabecera personalizada.
7. **No existe confirmar/cancelar en ninguna parte.** `RackFrameConfiguratorWindow` recibe la
   `RackFrameConfiguration` **por referencia**, la muta a traves de su ViewModel y **no tiene boton
   Aceptar ni Cancelar**: solo cierra. `RackDynamicSystemWindow.EditHeader_Click` toma un snapshot
   serializado **antes** de abrirlo, pero lo usa unicamente para **detectar si hubo cambio** (y no
   acumular un preset «Personalizada N» duplicado), **nunca para revertir**. Cerrar el configurador
   sin querer el cambio deja el cambio aplicado.

## 3. Alcance

1. **Auditoria y caracterizacion primero** (esta sesion): fijar en pruebas los siete hechos anteriores
   sobre la base, **antes** de cambiar comportamiento.
2. **Fundacion neutral** en `RackCad.Application`, pura y sin consumidor todavia: descriptor de
   modulo de solo lectura, sesion de edicion transaccional con confirmar/cancelar, intencion de
   restauracion y reconciliacion de modulos que preserve **fondo, configuracion y procedencia**.
3. **Superficie de edicion por modulo en Push Back**, construida sobre el
   `RackEditorVisualShell` (I-30) que la ventana ya usa y sobre los controles de I-14.
4. **Revision de la dependencia del hecho 4** en el mismo cambio, como exige PB-011: que el alto de
   tarima general siga siendo un espejo de la celda (PB-013) y no se vuelva un input encubierto por
   la via de una cabecera personalizada.
5. **Restauracion explicita** que consuma el `forceRebuild` ya existente.
6. Pruebas puras en `tests/RackCad.Tests` y pruebas STA en `tests/RackCad.UI.Tests`; documentacion
   tocada si cambia comportamiento visible.

## 4. Fuera de alcance

- `SelectionMatrix`, `SelectionMatrixModel`, `SelectionMatrixBulkBar`, `SelectionMatrixBulkEditor`,
  `SelectionMatrixScope` y **cualquier operacion masiva** — los reclama **I-34**, hoy en
  `validating` con gate `owner-validation`.
- `SafetyTopeGridWindow`, `SafetyDesviadorGridWindow`, `SafetyGuiaEntradaGridWindow`,
  `SafetyDefensaGridWindow`, `SafetyParrillaGridWindow` y cualquier otro `Safety*GridWindow`.
- Topes, desviadores, guias y defensas como familias; `DesviadorCellsAreByPost` (decision del Owner
  aun pendiente, anotada en `ideas-futuras.md`).
- El **Selectivo** entero, los **catalogos** de `assets/catalogs/`, los **bloques DWG** y el formato
  fisico del Xrecord.
- **Cualquier cambio funcional en el Dinamico.** La ventana Dinamica y su editor avanzado se leen
  como referencia y **no se editan**; en particular `RestoreHeaderFondos` **no** se modifica en esta
  iniciativa aunque el hecho 5 lo senale (cambiarlo cambiaria el Dinamico).
- **Copiar el editor Dinamico.** Lo compartido se factoriza neutral en Application; lo que quede en
  la ventana se escribe para Push Back.
- **Ramas por `RackSystemKind`** en controles o tipos compartidos (ADR-0019 y la regla del shell de
  I-30: el shell es agnostico al sistema).
- Cualquier clon de `RackFrameConfiguration` que no sea `RackFrameProjectStore.DeepCopy` (I-17).
- El **preview visual**, diferido por I-18 a una iniciativa transversal futura.
- I-23 (namespaces) e I-25 (guardas traseras).
- `docs/ROADMAP.md` y `docs/HANDOFF.md`: prohibidos por instruccion del Owner en esta sesion y
  reservados por WORKFLOW seccion 4.5.4 a la sesion de integracion.

## 5. Contexto requerido

- Normas: [`AGENTS.md`](../../AGENTS.md), [`WORKFLOW.md`](../WORKFLOW.md).
- Estado y plan: [`HANDOFF.md`](../HANDOFF.md), [`ROADMAP.md`](../ROADMAP.md) (**solo lectura**),
  [`ideas-futuras.md`](../ideas-futuras.md) (PB-011 y su nota tecnica).
- Arquitectura: [`ARCHITECTURE.md`](../ARCHITECTURE.md) secciones 3.3, 4.1, 4.2, 5, 7.1 y 7.3.
- Context Packs: `system-dynamic-flowbed`, `ui-editors`, `architecture-kernel`, `persistence`,
  `delivery-validation`.
- ADR: [0009](../adr/0009-identidad-guid-embebida-en-dwg.md),
  [0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md),
  [0015](../adr/0015-entrada-numerica-localizada.md),
  [0019](../adr/0019-shell-visual-de-editores-por-composicion.md).
- Contratos previos: [I-17](I-17-clon-unico-cabecera.md) (clon canonico),
  [I-18](I-18-push-back.md) (fundacion de Push Back), [I-21](I-21-dynamic-editor-state.md) (estado
  del editor Dinamico), [I-30](I-30-editor-visual-shell.md) (shell visual),
  [I-32](I-32-correcciones-push-back.md) (PB-013 y el espejo de tarima),
  [I-33](I-33-frente-en-blanco.md) (frente en blanco y fronteras fisicas).
- Codigo: `RackDynamicSystemWindow.xaml`/`.xaml.cs` (editor avanzado, **solo lectura**),
  `RackPushBackSystemWindow.xaml`/`.xaml.cs`, `PushBackEditorState(.Load)`,
  `PushBackEditorDesignAssembler`, `PushBackResolver`, `PushBackDesignDocument`,
  `DynamicEditorDesignAssembler`, `DynamicRackSystemBuilder`, `DynamicRackSystemResolver`,
  `DynamicRackSystemDocument`, `DynamicFrontActivation`, `RackFrameProjectStore`.

## 6. Dependencias

Integradas y requeridas: **I-15** (sesion de editor), **I-17** (clon canonico), **I-18** (Push Back),
**I-21** (estado del editor Dinamico), **I-30** (shell visual), **I-32** (PB-013), **I-33** (frente
en blanco). Todas estan en `main` en la base `7e48b5c`.

Conflictos que deben permanecer inactivos sobre el codigo:

- **I-34** (`feature/edicion-masiva-seguridad`, `validating`/`owner-validation`) toca
  `SelectionMatrix*` y tres `Safety*GridWindow`. La interseccion de codigo con I-35 es **vacia**;
  la unica interseccion prevista es **documental** (`docs/initiatives/README.md`), que se reconcilia
  al integrar como en I-05/I-19/I-22/I-24.
- **I-23** (namespaces) e **I-25** (guardas traseras) siguen `pendiente`, sin rama.

Entradas del Owner que deben existir antes de implementar la UI (seccion 12).

## 7. Archivos esperados

Crear (Application, fundacion neutral):

- `src/RackCad.Application/Systems/RackModuleDescriptor.cs`
- `src/RackCad.Application/Systems/RackModuleEditSession.cs`
- `src/RackCad.Application/Systems/RackModuleReconciliation.cs`

Crear (pruebas):

- `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs`
- `tests/RackCad.Tests/RackModuleEditSessionTests.cs`

Modificar mas adelante (fases 3 y siguientes, **no** en esta sesion):

- `src/RackCad.UI/RackPushBackSystemWindow.xaml` y `.xaml.cs`
- `src/RackCad.Application/Systems/PushBackEditorState.cs` y `PushBackEditorDesignAssembler.cs`
- `tests/RackCad.UI.Tests/PushBackEditor*Tests.cs`

Documentacion: este contrato, `docs/initiatives/README.md`,
`docs/automation/state/I-35.yml` y `docs/ideas-futuras.md`.

Una desviacion material —en particular tocar `RackDynamicSystemWindow`, `RackSelectiveWindow`,
`SelectionMatrix*`, `Safety*GridWindow`, catalogos o DWG— exige detenerse.

## 8. Fases

1. **Preflight y reclamo.** Base verificada, worktree unico, rama, commit vacio de reclamo con
   `Claim-Id` y `Co-Authored-By`, primer push sin force. **HECHA.**
2. **Auditoria y caracterizacion.** Los siete hechos de la seccion 2 fijados en pruebas puras que
   **pasan sobre la base**, cada una verificada **en rojo** al invertir deliberadamente lo que
   afirma. **HECHA.**
3. **Fundacion neutral** en Application, pura, cubierta y **sin conectar** a la ventana. **HECHA.**
4. **Decision del Owner** (seccion 12) sobre el eje de personalizacion y la politica de
   reconciliacion. **PENDIENTE — gate.**
5. Adopcion de la fundacion por `PushBackEditorState`/`PushBackEditorDesignAssembler`, con la
   reconciliacion que preserva configuracion y procedencia y con la regresion en rojo previa.
6. Superficie de edicion por modulo en `RackPushBackSystemWindow` sobre el shell de I-30, mas
   «Restaurar estandar» consumiendo el `forceRebuild` existente.
7. Revision de la dependencia del hecho 4 (alto de tarima general inerte, PB-013) con su prueba.
8. Round-trip y persistencia: DTO, cuatro vistas, dos BOM e interaccion con I-33.
9. Cierre: suite completa, builds Debug, CI verde, validacion manual del Owner en AutoCAD 2025.

## 9. Pruebas y builds

```powershell
dotnet test RackCad.sln
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug   # con AutoCAD CERRADO
```

El SDK 8.0.423 vive a nivel de usuario (`%LOCALAPPDATA%\Microsoft\dotnet`); el `dotnet` de
Program Files solo trae runtimes. CI: los cuatro jobs verdes sobre el SHA publicado. Los `MSB3277`
del Plugin son conocidos y no cuentan.

Toda prueba de caracterizacion se verifica **fallando** al invertir la afirmacion que fija; todo
bugfix posterior lleva su regresion **verificada en rojo** sin el fix (AGENTS.md).

## 10. Validacion manual

**Requerida** (`requires_autocad: true`) a partir de la fase 6, porque la edicion por modulo cambia
la estructura que se dibuja. Checklist a ejecutar sobre el DLL Debug del worktree de I-35:

1. Push Back nuevo: la secuencia de modulos y el dibujo son identicos a los de `main`.
2. Personalizar la **medida** de un modulo: las cuatro vistas y los dos BOM la reflejan.
3. Personalizar una **cabecera**: se conserva al recalcular sin cambio estructural.
4. Cambiar **tarima o fondos** con una cabecera personalizada viva: se conserva (hecho 5 corregido).
5. **Restaurar estandar**: descarta las personalizaciones, explicitamente y solo entonces.
6. **Separadores**: cantidad y separacion se respetan.
7. Round-trip `RACKEDITAR` con el **mismo GUID**; biblioteca y documento legacy.
8. Interaccion con **I-33**: un frente en blanco y una frontera suprimida no rompen la edicion por
   modulo ni la reconciliacion.
9. El **alto de tarima general** sigue inerte como input (PB-013).
10. El **Dinamico** y el **Selectivo** se comportan exactamente como antes.

## 11. Criterios de aceptacion

- Los siete hechos de la seccion 2 estan fijados en pruebas y las que describen un defecto quedan
  invertidas por su regresion cuando la fase correspondiente lo corrija.
- Push Back edita modulos con la misma potencia que el Dinamico, **sin** copia del editor Dinamico y
  **sin** ramas por `RackSystemKind` en tipos compartidos.
- Una cabecera personalizada sobrevive a un cambio estructural y solo la desecha una restauracion
  explicita.
- `RackFrameProjectStore.DeepCopy` es el **unico** clon de `RackFrameConfiguration` que I-35 introduce.
- Dinamico y Selectivo, byte a byte iguales en comportamiento; suite completa verde; builds Debug con
  0 errores propios; CI 4/4 sobre el SHA publicado.
- `docs/ROADMAP.md` y `docs/HANDOFF.md` **sin tocar** por esta rama.

## 12. Condiciones para detenerse

1. **Arbol sucio, claim existente o CI no acreditable** sobre el SHA exacto.
2. **Conflicto con I-34**: si I-34 deja de estar confinada a `SelectionMatrix*`/`Safety*GridWindow`,
   o si I-35 necesitara tocarlos.
3. **Necesidad de editar una superficie reservada**: ROADMAP, HANDOFF, catalogos, bloques DWG,
   Selectivo o comportamiento del Dinamico.
4. **Decision del Owner pendiente (gate abierto).** La auditoria establece que
   `DynamicRackSystem.Modules` es **una sola secuencia longitudinal de rack**, compartida por todos
   los frentes y todos los postes: los modulos recorren la profundidad (X) y las secciones laterales
   solo miran un **rango** de esa misma lista (`DynamicDepthGeometry.ModulesInRange`). Por tanto
   **personalizar un modulo personaliza el rack entero**, no un frente ni un poste. Si lo que el
   Owner espera es personalizacion **por frente o por poste**, el modelo no la soporta y el alcance
   deja de ser PB-011: hay que detenerse y decidir. Preguntas abiertas:
   - a) La personalizacion por modulo, ¿es de rack (como en el Dinamico) o el Owner espera poder
     variarla por frente o por poste?
   - b) Cuando un cambio estructural obliga a reconstruir, ¿la cabecera personalizada se **conserva**
     (correccion del hecho 5) o se **descarta** avisando? El Dinamico hoy la descarta en silencio y
     I-35 **no puede cambiar el Dinamico**, asi que Push Back divergiria a proposito.
   - c) ¿El configurador de cabecera debe ganar **Aceptar/Cancelar** (hecho 7)? Ganarlo cambiaria una
     ventana **compartida** con el Dinamico, el Selectivo y la cabecera: fuera de alcance salvo
     autorizacion explicita. La alternativa dentro de alcance es que la **sesion transaccional** de
     Push Back revierta al cancelar, sin tocar el configurador.
5. **Ambiguidad** de cualquier otro tipo que implique personalizacion por frente o por poste.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en [`../automation/state/I-35.yml`](../automation/state/I-35.yml). **No** hay Pull
Request: la entrega es rama publicada mas estado versionado, como en I-32 e I-33
(`pull_request: none`). El merge automatico esta prohibido. La integracion la ejecuta el Owner en su
workstation, serializada, conforme a WORKFLOW seccion 4.5.

## 14. Evidencia final

La evidencia de cada sesion vive en el cuerpo de sus commits y en el estado versionado; los conteos
de pruebas y los SHA canonicos viven en `docs/HANDOFF.md`, que esta rama **no** toca. `main` no fue
modificada por esta iniciativa.
