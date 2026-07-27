---
schema: rackcad-initiative/v1
id: I-35
title: Editor avanzado de modulos de Push Back
type: feature
status: integration-ready
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
requires_owner_decision: false
requires_owner_validation: true   # APROBADA por el Owner sobre f2be30c
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
> autorizo el Owner por instruccion directa. La procedencia documental del alcance es **PB-011 en
> `ideas-futuras.md`**.
>
> **La regla de ROADMAP y HANDOFF, dicha bien** (WORKFLOW secciones 4.5.4 y 8): los dos son archivos
> calientes que **NO** se tocan desde una sesion de implementacion ni desde una rama paralela, pero
> **SI** se editan **en esta misma rama, como ULTIMO commit, en su sesion de integracion** —ahi se
> marca la iniciativa en el ROADMAP y se actualiza HANDOFF, de modo que el merge lleve los docs
> consigo—. Decir «esta rama no los toca nunca» era incorrecto: lo correcto es que **ninguna sesion
> de implementacion los toca**, y esta sesion es de implementacion. El Owner ademas lo prohibio
> explicitamente para la sesion de apertura.

## 0. Decisiones del Owner (cerradas)

El gate `owner-decision` que la sesion de apertura dejo abierto quedo **resuelto**. Estas cinco
decisiones son normativas para el resto de la iniciativa:

1. **La personalizacion es por MODULO LONGITUDINAL DE RACK, nunca por frente ni por poste.** Es lo
   que el modelo soporta (`DynamicRackSystem.Modules` es una sola secuencia compartida) y lo que el
   Owner quiere. No se introduce ningun eje por frente ni por poste.
2. **En una recomputacion estructural, una personalizacion se conserva unicamente cuando existe
   correspondencia exacta `ModuleId + Kind`.** Se retira el emparejamiento por ordinal.
3. **Un modulo eliminado, o cuyo tipo cambio, pierde su personalizacion de forma explicita y
   reportable; uno nuevo nace calculado.** Nada se pierde en silencio.
4. **No existe una politica ordinaria `Discard`.** El descarte solo ocurre por **restauracion
   explicita** (individual o total) o por **incompatibilidad estructural** (los dos casos del punto
   3). La politica publica `Preserve/Discard` queda **eliminada**.
5. **`RackFrameConfiguratorWindow` no se modifica.** Confirmar y cancelar pertenecen a la **sesion**
   y a la **superficie de Push Back**; el configurador compartido se abre sobre una **copia** y su
   resultado se acepta o se descarta desde fuera.

## 0.a Cierre: el Owner APRUEBA — `integration-ready`

El Owner **aprueba explicitamente** la validacion en **AutoCAD 2025** del candidato tecnico
**`f2be30c20a7ff8958a24ddf078a5310dab5dbfe0`**, sobre el DLL Debug del worktree estampado
`1.0.0+f2be30c20a7ff8958a24ddf078a5310dab5dbfe0`, SHA-256
`4FE530EFA0FFAEF005B20253A1C0F68BF99D321A82766D4FF559A3367E99C101`. Con eso quedan **cerrados** los
gates `autocad` y `owner-validation`, y la rama pasa a **`integration-ready`**.

- **Rebase final: NO necesario.** `origin/main` **no avanzo** desde la base
  `52ce27f8f0e247eee5f4721c0d29b7e005588525`, asi que la validacion del Owner vale sobre el arbol que
  se integra (WORKFLOW seccion 6).
- **Arbol tecnico intacto.** El delta entre el candidato y la punta de la rama es **exclusivamente**
  documental: **cero** archivos de `src/`, `tests/` o `assets/`. El DLL aprobado sigue siendo el valido.
- **CI tecnica** del candidato: run **30293536290**, **verde 4/4**.
- **Suites**: 1612 `RackCad.Tests` + 491 `RackCad.UI.Tests`, cero fallos, cero omitidas.
- **Builds Debug**: UI 0 errores / 0 advertencias; Plugin 0 errores propios y las 2 `MSB3277`
  conocidas, con AutoCAD cerrado.

### Alcance final integrado

1. **Edicion longitudinal de Cabeceras y Separadores** por modulo de RACK, con seleccion unica.
2. **Configuracion transaccional** de cabecera: confirmar / cancelar sobre una **copia**, sin tocar
   `RackFrameConfiguratorWindow`.
3. **Altura manual de cabecera** (`ManualHeaderHeightOverride`).
4. **Refuerzo total o parcial del poste derivado** (`DerivedPostReinforced` +
   `DerivedPostReinforcementHeight`), con bloqueo visible en vez de recorte silencioso.
5. **Cantidad y separacion globales de separadores** (`SeparatorCountOverride`,
   `SeparatorSpacingOverride`), independientes entre si.
6. **Restauracion individual** por modulo y **global** del rack.
7. **Preservacion de I-33** (frentes en blanco y fronteras suprimidas) y de **PB-013** (alto de tarima
   general inerte).

## 0.b Primera ronda del Owner: PARCIALMENTE RECHAZADA

El Owner validó en AutoCAD 2025 el candidato `7ceede9` y **aprobó la edición por módulo**, que por tanto
**no se rediseña**. La ronda queda **parcialmente rechazada** por **cuatro residuos**: Push Back seguía sin
las capacidades avanzadas que el Dinámico sí ofrece.

Los cuatro son **parámetros GLOBALES DEL RACK**, no propiedades del módulo `Separator`, y viven en una
**sección independiente** del panel «Módulo seleccionado»:

| # | Residuo | Autoridad reutilizada |
|---|---|---|
| 1 | Altura personalizada de cabecera | `ManualHeaderHeightOverride` |
| 2 | Refuerzo del poste derivado y su longitud opcional | `DerivedPostReinforced` + `DerivedPostReinforcementHeight` |
| 3 | Cantidad personalizada de separadores | `SeparatorCountOverride` |
| 4 | Separación personalizada de separadores | `SeparatorSpacingOverride` |

**No se creó ninguna autoridad nueva ni campo equivalente.** Las cinco propiedades ya existían en
`DynamicRackDesign`/`DynamicRackSystem` y ya las consumían el resolver, `DynamicSeparatorGeometry`, el
builder lateral y el BOM. Push Back solo **transporta** la intención del usuario hasta ellas
(`PushBackAdvancedRackParameters`: validar y asignar, nada más).

### Contrato de los cuatro ámbitos

- altura manual **desactivada o vacía** = cálculo vigente; **activa** debe ser `> 0`;
- una **cabecera personalizada conserva configuración y procedencia** al cambiar la altura global, pasando
  por la adaptación y la validación de I-35 (misma reconciliación por `ModuleId + Kind`);
- **refuerzo desactivado elimina solo el refuerzo**, nunca el poste derivado, que es consecuencia
  estructural de dos separadores consecutivos;
- **longitud de refuerzo vacía = altura completa**; capturada = `> 0` **y no mayor que la altura física
  resuelta del poste**;
- cambiar niveles, altura de cabecera o geometría **revalida** la altura manual del refuerzo; si una
  recomputación la vuelve inválida, **bloquea con error visible** — no se recorta ni se restaura en
  silencio;
- apagar el refuerzo **no persiste una medida muerta**: la estructura guarda «sin refuerzo» y ninguna
  longitud;
- cantidad y separación **vacías = cálculo automático**; la cantidad manual es un **entero** válido y la
  separación manual `> 0`; **son independientes**;
- la **restauración explícita** devuelve los cuatro ámbitos al cálculo/default vigente;
- persistencia **legacy, biblioteca, Xrecord, GUID y campos desconocidos** se conservan;
- **preview, cuatro vistas y BOM** consumen el mismo sistema resuelto.

## 1. Objetivo

Que Push Back ofrezca el mismo poder de edicion por modulo que el Dinamico —seleccionar una cabecera
o un separador de la secuencia longitudinal y personalizarlo— **sin copiar el editor Dinamico**, sin
que la personalizacion se pierda en silencio en el siguiente recalculo y sin cambiar una sola linea
de comportamiento del Dinamico ni del Selectivo.

Resultado verificable: sobre un Push Back con una cabecera personalizada, un cambio de tarima o de
fondos **conserva** esa cabecera y su procedencia, las **cuatro vistas** (lateral, frontal de
entrada/salida, frontal posterior y planta) y **el BOM** de Push Back la reflejan, el round-trip la
persiste, y existe una restauracion explicita que la descarta a peticion del usuario.

> **Nota de precision.** Push Back tiene **UN** BOM —`PushBackBomBuilder.Build`, el unico que
> consumen el editor y `PushBackKindHandler`—. La formula «los dos BOM» viene de I-33, donde el
> alcance eran **dos sistemas** (Dinamico y Push Back) con **un** BOM cada uno; trasladarla a I-35,
> cuyo alcance es solo Push Back, era un error de este contrato y queda corregido. El consolidado
> `RACKBOMTOTAL` no es un segundo BOM del rack: agrega los de todos los racks colocados.

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
- `docs/ROADMAP.md` y `docs/HANDOFF.md`: no se tocan desde ninguna sesion de IMPLEMENTACION (esta lo
  es), y
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

Las entradas del Owner exigidas por la seccion 12 ya existen: la seccion 0 las recoge.

## 7. Archivos esperados

Crear (Application, fundacion neutral):

- `src/RackCad.Application/Systems/RackModuleDescriptor.cs`
- `src/RackCad.Application/Systems/RackModuleEditSession.cs` (+ `RackModuleCommit`)
- `src/RackCad.Application/Systems/RackModuleReconciliation.cs` (+ `RackModuleReconciliationResult`)

Crear (pruebas):

- `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs`
- `tests/RackCad.Tests/RackModuleEditSessionTests.cs`
- `tests/RackCad.Tests/PushBackModuleAdoptionTests.cs`
- `tests/RackCad.UI.Tests/PushBackModuleEditorWindowTests.cs`

Modificar:

- `src/RackCad.Application/Systems/PushBackEditorState.cs`, `PushBackEditorState.Load.cs`,
  `PushBackEditorDesignAssembler.cs` y `PushBackEditorInputs.cs`
- `src/RackCad.Application/Systems/PushBackAdvancedRackParameters.cs` (ronda 2: validar y asignar)
- `src/RackCad.UI/RackPushBackSystemWindow.xaml` y `.xaml.cs`
- `tests/RackCad.Tests/PushBackAdvancedRackParametersTests.cs`
- `tests/RackCad.UI.Tests/PushBackAdvancedRackParametersWindowTests.cs`

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
4. **Decision del Owner** (seccion 0). **RESUELTA.**
5. **Correccion de la fundacion a esas decisiones**: reconciliacion por `ModuleId + Kind`, longitud
   manual en cabeceras y separadores, adaptacion de `Depth` y peralte, restauracion individual
   completa, sin politica publica de descarte y con reporte por categoria. **HECHA.**
6. Adopcion de la fundacion por `PushBackEditorState`/`PushBackEditorDesignAssembler`.
7. Superficie de edicion por modulo en `RackPushBackSystemWindow` sobre el shell de I-30, mas
   «Restaurar estandar» consumiendo el `forceRebuild` existente.
8. Revision de la dependencia del hecho 4 (alto de tarima general inerte, PB-013) con su prueba.
9. Round-trip y persistencia: DTO, cuatro vistas, BOM e interaccion con I-33.
10. Cierre: suite completa, builds Debug, CI verde, validacion manual del Owner en AutoCAD 2025.

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
2. Personalizar la **longitud** de un modulo: las **cuatro vistas** y **el BOM** la reflejan.
3. Personalizar una **cabecera**: se conserva al recalcular sin cambio estructural.
4. Cambiar **tarima o fondos** con una cabecera personalizada viva: se conserva y su fondo y peralte
   quedan **adaptados** a la estructura nueva.
5. **Restaurar** un modulo (individual) y **Restaurar estandar** (todo): descartan las
   personalizaciones, explicitamente y solo entonces.
6. **Separador seleccionado**: el editor ofrece **unicamente** su **longitud fisicamente consumida**
   y su restauracion —no cantidad ni separacion, que son overrides de RACK del Dinamico y **no**
   entran en I-35—. Cambiar esa longitud mueve la corrida longitudinal en las cuatro vistas.
7. **Reduccion estructural**: al perder un modulo personalizado, el editor lo **reporta** por su id;
   nada desaparece en silencio.
8. Round-trip `RACKEDITAR` con el **mismo GUID**; biblioteca y documento legacy.
9. Interaccion con **I-33**: un frente en blanco y una frontera suprimida no rompen la edicion por
   modulo ni la reconciliacion, no se reactiva ningun frente y no reaparece ninguna frontera.
10. El **alto de tarima general** sigue inerte como input (PB-013).
11. El **Dinamico** y el **Selectivo** se comportan exactamente como antes.
12. El **configurador de cabecera** se abre desde Push Back, y **Cancelar** en la ventana de Push
    Back deja el diseno como estaba aunque el configurador se haya cerrado con cambios.

### Segunda ronda — los cuatro residuos (focalizada)

13. **Altura de cabecera**: vacía dibuja la altura calculada; con un valor, el rack se dibuja a esa
    altura en las cuatro vistas. Un valor `0` o negativo se **rechaza con mensaje**.
14. **Cabecera personalizada + altura global**: personaliza una cabecera, cambia la altura global y
    comprueba que la cabecera **conserva su configuración y su procedencia**.
15. **Refuerzo desactivado**: el poste derivado **sigue dibujándose**; desaparece **solo** su refuerzo,
    en lateral, cortes y BOM. El campo «Altura del refuerzo» queda **deshabilitado con su motivo**.
16. **Refuerzo a toda la altura**: activado y con el campo **vacío**, el refuerzo llega arriba del poste.
17. **Refuerzo parcial**: con un valor menor que el poste, el refuerzo llega **desde la base hasta ahí**,
    y el BOM cambia respecto al refuerzo completo.
18. **Refuerzo inválido**: `0`, negativo o **mayor que el poste** se **rechaza con mensaje**.
19. **Refuerzo revalidado**: con un refuerzo válido, **reduce** la altura de cabecera o los niveles hasta
    que el poste quede más bajo. Debe **bloquear con error visible**, sin recortar el valor capturado.
20. **Separadores**: cantidad y separación vacías = automático; cada una por separado cambia el dibujo;
    fijar una **no altera** la otra. Cantidad no entera o `<= 0`, y separación `<= 0`, se **rechazan**.
21. **Restaurar parámetros globales**: devuelve los cuatro campos a vacío/refuerzo activado, y el rack al
    cálculo vigente.
22. **Round-trip**: guarda en biblioteca y reabre; inserta, cierra y `RACKEDITAR` con el **mismo GUID**.
    Los cuatro ámbitos vuelven tal cual. Un documento **legacy** abre con el cálculo vigente.
23. **Aislamiento**: el **Dinámico** y el **Selectivo** siguen exactamente igual; la edición masiva de
    seguridad de **I-34** se comporta como quedó integrada.

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
- `docs/ROADMAP.md` y `docs/HANDOFF.md` **sin tocar por ninguna sesion de implementacion**; su
  actualizacion es el ULTIMO commit de esta misma rama, en la sesion de integracion (WORKFLOW 4.5.4).

## 12. Condiciones para detenerse

1. **Arbol sucio, claim existente o CI no acreditable** sobre el SHA exacto.
2. **Conflicto con I-34**: si I-34 deja de estar confinada a `SelectionMatrix*`/`Safety*GridWindow`,
   o si I-35 necesitara tocarlos.
3. **Necesidad de editar una superficie reservada**: ROADMAP, HANDOFF, catalogos, bloques DWG,
   Selectivo o comportamiento del Dinamico.
4. **Cualquier ambiguedad que implique personalizacion por frente o por poste.** El gate de decision
   que la sesion de apertura abrio quedo **CERRADO** por la seccion 0 —las tres preguntas (eje,
   politica de reconciliacion y confirmar/cancelar del configurador) estan respondidas—, pero la
   condicion de detencion sigue viva: `DynamicRackSystem.Modules` es **una sola secuencia
   longitudinal de rack** y el modelo no soporta un eje por frente ni por poste. Si una peticion
   futura lo exige, el alcance deja de ser PB-011 y hay que detenerse.
5. **Necesidad de modificar `RackFrameConfiguratorWindow`.** La decision 5 lo prohibe: si confirmar,
   cancelar o restaurar exigieran cambiarlo, hay que detenerse en vez de tocar una ventana compartida
   con el Dinamico, el Selectivo y la cabecera.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en [`../automation/state/I-35.yml`](../automation/state/I-35.yml). **No** hay Pull
Request: la entrega es rama publicada mas estado versionado, como en I-32 e I-33
(`pull_request: none`). El merge automatico esta prohibido. La integracion la ejecuta el Owner en su
workstation, serializada, conforme a WORKFLOW seccion 4.5.

## 14. Evidencia final

La evidencia de cada sesion vive en el cuerpo de sus commits y en el estado versionado. Los conteos
de pruebas y los SHA canonicos viven en `docs/HANDOFF.md`, que **ninguna sesion de implementacion**
toca: lo actualiza el ULTIMO commit de esta misma rama, en su sesion de integracion. `main` no fue
modificada por esta iniciativa.
