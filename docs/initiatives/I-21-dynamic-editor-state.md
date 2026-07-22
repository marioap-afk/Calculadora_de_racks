---
schema: rackcad-initiative/v1
id: I-21
title: Estado del editor dinamico
type: refactor
status: implemented
branch: refactor/dynamic-editor-state
base_branch: main
priority:
size: M-L
depends_on: [I-15, I-02]
conflicts_with: [I-28]
context_packs:
  - architecture-kernel
  - ui-editors
  - system-dynamic-flowbed
  - persistence
  - delivery-validation
automation_state_path: docs/automation/state/I-21.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-21 — Estado del editor dinamico

## 1. Objetivo

Extraer de `RackDynamicSystemWindow` (`src/RackCad.UI`, ~3,339 lineas de code-behind) hacia
`RackCad.Application` el **estado puro y testeable** del editor dinamico —la matriz de frentes por
nivel con sus celdas, la seleccion, la recomputacion y la construccion del diseno/sistema— dejando la
ventana como coordinadora de controles, eventos, render y dialogo, sobre el Editor Shell (I-15) ya
integrado. Es la contraparte dinamica de I-20 para el selectivo (ROADMAP Fase 5).

El resultado es verificable cuando: la solucion compila; `RackCad.Tests` cubre el estado extraido
(matriz, celdas, recomputacion, construccion del diseno, casos invalidos, carga legacy) y queda
verde; `RackCad.UI.Tests` (incluida la adopcion STA que construye la ventana real) sigue verde; los
builds Debug de UI y Plugin quedan en 0 errores propios; y CI queda verde sobre la punta publicada.
El comportamiento observable (dibujo, planes, BOM, GUID, nombre, `Section`, edicion multivista,
persistencia I-11, metadatos desconocidos, fallbacks legacy, cabeceras legacy, cama integrada,
selecciones de seguridad/IN-OUT/intermedios/fondos/niveles, textos, orden y apariencia) es identico
al vigente.

## 2. Problema

ADR-0002 (opcion A, aceptada) integro el dinamico modular a sabiendas de heredar la deuda de
arquitectura de la ventana: 3,339 lineas de code-behind con la matriz de frentes, la seleccion, la
recomputacion y el armado del diseno mezclados con WPF, sin poder probarse sin una ventana. El
ROADMAP dividio el Editor Shell en tres: controles (I-14), shell (I-15) y **extraccion del estado por
editor en Fase 5 (I-20 selectivo, I-21 dinamico)**. I-15 dejo el estado propio del dinamico
—`frontRows`/celdas, la seleccion, `Recompose` y el armado del diseno— explicitamente reservado para
I-21 (contrato de I-15, seccion 4). Sin esa extraccion, cada regla de la matriz (crecer frentes,
aplicar por celda/nivel/frente, preservar fondos de cabecera al reconstruir) solo se ejercita a mano
en AutoCAD y ningun test la protege.

## 3. Alcance

Autorizado por el ROADMAP (Fase 5, I-21) y por el objetivo de `ARCHITECTURE.md` seccion 7.3
(«el estado del editor se extrae a Application»):

1. Crear en `src/RackCad.Application/Systems/` el estado puro del editor dinamico, separado de la
   vista WPF:
   - `DynamicEditorCell` y `DynamicEditorFront`: las filas/celdas editables (antes tipos privados
     `DynamicCellRow`/`DynamicFrontRow` de la ventana), con `EnsureCellCount`, `Clone`, `Apply`,
     `ToDesign`, `From(DynamicRackLevel)`.
   - `DynamicEditorValues`: el buffer de edicion por celda (antes tipo privado).
   - `DynamicFrontMatrix`: la matriz frente x nivel y **la seleccion** (indice primario y celdas
     multiples) con todas las mutaciones estructurales: alta/baja de frentes, ajuste de posiciones y
     niveles, seleccion/toggle, commit del buffer, aplicar por alcance (celda/seleccion/nivel/frente/
     todo) via `DynamicRackCellScopeResolver`, aplicar datos estructurales a frentes, snapshot/rollback,
     refresco y restauracion desde un sistema resuelto, y la proyeccion a `DynamicRackFrontDesign`.
   - `DynamicEditorSafety`: la regla pura «dibuja» y la copia de selecciones dibujables (antes
     `SafetyDraws`/`ReplaceSafetySelections` privados).
   - `DynamicAnnotationOptions` y `DynamicEditorDesignAssembler`: la **recomputacion y construccion del
     diseno** —decision de reconstruir (`MustRebuild`), preservacion de fondos de cabecera
     (`SnapshotHeaderFondos`/`RestoreHeaderFondos`), actualizacion de altura en sitio
     (`UpdateHeaderHeightInPlace`) y el armado del `DynamicRackDesign` (`BuildDesign`)— componiendo el
     builder y el resolver existentes, sin duplicar su geometria.
2. Adaptar `RackDynamicSystemWindow` para **consumir** ese estado: la ventana lee sus controles a
   primitivos, delega en la matriz y el ensamblador, y escribe los resultados de vuelta a los controles
   y al lienzo. `Recompose` conserva su orquestacion (leer WPF, resolver, dibujar) y ahora llama a los
   nuevos tipos con los mismos argumentos y en el mismo orden. La ventana sigue coordinando el Editor
   Shell (I-15) para catalogo, identidad, contrato de insercion y actualizacion.
3. Añadir pruebas de caracterizacion/equivalencia en `tests/RackCad.Tests` para el estado extraido,
   los casos invalidos, la carga legacy (restaurar desde un sistema resuelto), la recomputacion
   (preservacion de fondos, decision de reconstruir) y la construccion del diseno (incluida su
   resolucion por el pipeline real).

## 4. Fuera de alcance

- **Push Back / I-18** y cualquier sistema nuevo.
- **Dinamico V2 / I-28** (solo si un ADR futuro reemplaza a ADR-0002; no esta activa).
- **Reglas de producto**: geometria, resolvers, builders, BOM, seguridad, alturas de poste, cama.
- **Rediseño visual** o cambio de apariencia/textos/orden de la ventana.
- **Editor selectivo / I-20** y su estado.
- **Catalogos, bloques DWG** y persistencia (formatos, DTO, envelope, Xrecord, round-trip I-11): se
  preservan intactos; el diseno persistido sigue siendo `DynamicRackDesign`.
- **Refactors oportunistas** fuera de la extraccion declarada. (La unica remocion incidental es el
  metodo privado muerto `EnsureIntermediateBeamDepthCount`, que solo delegaba en el helper extraido.)

## 5. Contexto requerido

- `AGENTS.md` (convenciones, direccion de dependencias, definicion de terminado).
- `docs/WORKFLOW.md` (ciclo de iniciativa, archivos calientes, cierre) y `docs/ROADMAP.md` (I-21, sus
  dependencias I-15/I-02 y su estorbo condicional I-28).
- `docs/AUTOMATION_PLAN.md` (reclamo atomico, estado versionado, limites de seguridad).
- `docs/ARCHITECTURE.md` seccion 3.3 (sistema dinamico), seccion 5 (UI) y seccion 7.3 (Editor Shell objetivo).
- [ADR-0002](../adr/0002-secuencia-dinamico-modular.md) (la deuda de la ventana dinamica se paga en I-21).
- Context Packs: `architecture-kernel`, `ui-editors`, `system-dynamic-flowbed`, `persistence`, `delivery-validation`.
- Precedente de logica pura de editor ya en Application: `DynamicRackCellScope.cs` (el resolver de alcance).
- Codigo consumido/adaptado: `src/RackCad.UI/RackDynamicSystemWindow.xaml{,.cs}`, el Editor Shell de
  I-15 (`src/RackCad.UI/Editor/`), `src/RackCad.Application/Systems/Dynamic*` (builder, resolver,
  geometria, defaults) y `src/RackCad.Domain/Systems/Dynamic*` (diseno/sistema/frente/celda).

## 6. Dependencias

- **Integradas requeridas:** I-15 (Editor Shell, integrada 2026-07-21, en `origin/main` = `bfda406`) e
  I-02 (dinamico modular, opcion A de ADR-0002, integrada 2026-07-17). Ambas en `origin/main`.
- **Estorbos que deben permanecer inactivos:** I-28 (`feature/dinamico-v2`) — condicional; solo entra
  si un ADR futuro reemplaza a ADR-0002 (aceptado con A). No tiene rama remota: inactiva. I-20
  (`refactor/selective-editor-state`) toca el selectivo, no el dinamico: sin rama remota al reclamar,
  no se toca su codigo ni su worktree.
- **Entradas del dueño:** ninguna decision requerida para arrancar. La validacion en AutoCAD del
  round-trip del editor dinamico es responsabilidad del dueño al cierre (gate abierto, seccion 10).

## 7. Archivos esperados

Crear (producto, `src/RackCad.Application/Systems/`):

- `DynamicEditorCell.cs`, `DynamicEditorFront.cs`, `DynamicEditorValues.cs`;
- `DynamicFrontMatrix.cs`;
- `DynamicEditorSafety.cs`;
- `DynamicAnnotationOptions.cs`, `DynamicEditorDesignAssembler.cs`.

Crear (pruebas, `tests/RackCad.Tests/`):

- `DynamicEditorCellTests.cs`, `DynamicEditorSafetyTests.cs`, `DynamicFrontMatrixTests.cs`,
  `DynamicEditorDesignAssemblerTests.cs`.

Modificar (acotado, sin cambio de comportamiento observable):

- `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` (consume la matriz y el ensamblador; elimina los
  tipos privados y los helpers movidos; el XAML no cambia);
- `docs/initiatives/README.md` (indice), `docs/automation/state/I-21.yml` (estado),
  `docs/automation/evidence/I-21-autocad-validation.md` (checklist de AutoCAD).

No se espera modificar el XAML de la ventana, Domain, el resto de Application, el Plugin, catalogos,
`deploy/` ni `.github/workflows`. Una desviacion material obliga a detenerse (seccion 12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato
   desde `TEMPLATE.md`, indice y estado versionado. (Evidencia: rama remota aceptada, este archivo.)
2. **Estado puro + caracterizacion.** `DynamicEditorCell/Front/Values`, `DynamicFrontMatrix`,
   `DynamicEditorSafety`, `DynamicAnnotationOptions`, `DynamicEditorDesignAssembler`, con sus pruebas.
   (Evidencia: suite verde.)
3. **Adopcion en la ventana.** `RackDynamicSystemWindow` delega en la matriz y el ensamblador; se
   eliminan los tipos y helpers movidos. (Evidencia: builds Debug de UI y Plugin en 0 errores propios.)
4. **Gates automatizados.** Suite completa + suite de UI + builds + CI de la rama sobre el SHA
   publicado. (Evidencia: seccion 14.)
5. **Cierre de sesion.** Revision de diff, commits logicos, push de la rama, estado versionado y gates
   manuales documentados como abiertos. (Evidencia: seccion 14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` — suite completa verde (sin regresion) +
  las pruebas nuevas del estado dinamico.
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` — verde, incluida la adopcion STA que
  construye la ventana real.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (solo los
  `MSB3277` conocidos de AutoCAD).
- CI: los cuatro jobs (Tests, Build UI, UI Tests, Build Plugin sin AutoCAD) en verde sobre la punta
  publicada de la rama.

## 10. Validacion manual

AutoCAD **requerido** (`requires_autocad: true`) porque el editor dinamico produce el diseno que se
dibuja; y **owner-validation** de comportamiento/apariencia. Ambos gates quedan **ABIERTOS** para el
dueño; esta sesion **no** los declara aprobados. El checklist y la ruta exacta del DLL Debug del
worktree viven en
[`../automation/evidence/I-21-autocad-validation.md`](../automation/evidence/I-21-autocad-validation.md).

Ruta del DLL a cargar con NETLOAD (dentro del worktree de la iniciativa):
`…-I-21-dynamic-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`.

Escenarios minimos (equivalencia visible con lo vigente): abrir `RACKDINAMICO`; editar la matriz
(crecer/decrecer frentes y niveles, seleccion multiple con Ctrl, aplicar por celda/nivel/frente/todo);
cabeceras por modulo (calculada vs personalizada, preservacion de fondos al cambiar tarimas);
seguridad, IN/OUT, intermedios, fondos y niveles; las tres vistas del preview; BOM; guardar/abrir en
biblioteca; insertar la lateral y luego `RACKEDITAR` (round-trip con el **mismo GUID**), actualizar en
sitio e insertar vistas enlazadas (frontal salida/entrada, planta); escenario legacy (diseno sin
campos nuevos). Geometria, planes y BOM **sin diferencias**.

## 11. Criterios de aceptacion

- Existe el estado puro del editor dinamico en `src/RackCad.Application/Systems/` (matriz, celdas,
  seleccion, seguridad, recomputacion y construccion del diseno), sin referencias WPF ni AutoCAD y sin
  duplicar reglas de geometria/BOM.
- `RackDynamicSystemWindow` consume ese estado: los tipos privados `DynamicFrontRow`/`DynamicCellRow`/
  `DynamicEditorValues` y los helpers movidos ya no viven en la ventana; el code-behind se reduce y la
  ventana solo coordina controles, eventos, render y dialogo sobre el Editor Shell.
- `RackCad.Tests` permanece verde y cubre el estado extraido, los casos invalidos, la carga legacy, la
  recomputacion y la construccion del diseno; `RackCad.UI.Tests` (adopcion STA incluida) pasa; los
  builds de UI y Plugin quedan en 0 errores propios; CI verde en la rama.
- Sin cambio de comportamiento observable: dibujo, planes, BOM, GUID, nombre, `Section`, edicion
  multivista, persistencia I-11, metadatos desconocidos, fallbacks legacy, cabeceras legacy, cama
  integrada, selecciones de seguridad/IN-OUT/intermedios/fondos/niveles, textos, orden y apariencia.
- La direccion de dependencias se conserva (UI no referencia AutoCAD; el estado nuevo vive en
  Application y no depende de UI ni del Plugin) y no hay dependencias NuGet nuevas.

## 12. Condiciones para detenerse

- Que preservar el comportamiento exija tocar geometria, resolvers, builders, BOM, seguridad,
  persistencia (formatos/DTO/envelope/Xrecord) o los Draw Services: detenerse antes de ampliar alcance.
- Que aparezca en `origin` una rama de I-28 activa (ADR que reemplace ADR-0002) o de I-20 tocando el
  dinamico de forma incompatible: no sobrescribir; entregar evidencia.
- Que la extraccion obligue a cambiar la apariencia, los textos, el orden o el XAML de la ventana.
- Cualquier necesidad de un paquete NuGet nuevo (producto o test).

## 13. Estado versionado y entrega del Pull Request

Estado canonico en `docs/automation/state/I-21.yml`. La automatizacion esta pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesion. No
se abre un segundo Pull Request ni se activa auto-merge. Los gates `autocad` y `owner-validation`
quedan **ABIERTOS**; el estado versionado queda en `state: review-ready` / `gate: autocad` a la espera
de la validacion del dueño. `completed` no significa integrada: la integracion a `main`
(`git merge --no-ff`, WORKFLOW seccion 4.5) es una sesion separada.

## 14. Evidencia final

Se completa al cierre de la sesion: commits logicos con trailer de procedencia, archivos creados/
modificados, resultados de `dotnet test` (suite completa y UI), builds de UI/Plugin, evidencia de CI
sobre el SHA publicado, SHA base y punta de la rama, confirmacion del push, gates manuales abiertos
(AutoCAD + owner-validation) y confirmacion de que `main` no fue modificada. El detalle vivo del
proyecto (`docs/HANDOFF.md` secciones 8-12) y la marca de integrada en `docs/ROADMAP.md` se tocan
**solo** en la sesion de integracion, nunca desde esta rama.
