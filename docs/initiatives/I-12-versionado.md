---
schema: rackcad-initiative/v1
id: I-12
title: Versionado real
type: refactor
status: in-progress
branch: refactor/versionado
base_branch: main
priority:
size: S-M
depends_on: []
conflicts_with: []
context_packs:
  - delivery-validation
automation_state_path: docs/automation/state/I-12.yml
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

# I-12 — Versionado real

## 1. Objetivo

Que RackCad tenga **una sola fuente de versión** y trazabilidad reproducible por SHA, y que el bundle
del Autoloader se produzca por un flujo canónico basado en `dotnet publish` con verificación
fail-closed. En concreto (hallazgos G5, G8, G9 de la auditoría):

- versión única `<Version>` en `Directory.Build.props`, más `LangVersion`/`Nullable` centralizados;
- SHA de git estampado de forma reproducible en `InformationalVersion` de cada ensamblado, con
  comportamiento definido cuando el repositorio o el SHA no estén disponibles;
- `PackageContents.xml` generado desde una fuente mantenible (plantilla + versión central), sin
  duplicar versión ni rutas a mano;
- bundle armado por `dotnet publish` y verificado (estructura, nombres, rutas, versiones y contenido
  exacto), con guarda recursiva fail-closed que demuestra que solo se distribuyen archivos RackCad y
  sus datos permitidos (cero DLL Autodesk);
- ADR corto de estrategia de versiones de AutoCAD (`SeriesMin`/`SeriesMax`, recompilación anual,
  límite actual a AutoCAD 2025).

## 2. Problema

La versión `1.0.0` estaba escrita a mano en dos lugares de `PackageContents.xml` y no existía en los
ensamblados; `LangVersion`/`Nullable` estaban duplicados idénticos en los cinco `.csproj`; el bundle
lo armaba un target `AfterTargets="Build"` copiando un `PackageContents.xml` estático; y no había
trazabilidad por SHA ni una verificación fail-closed del bundle que probara la ausencia de material
Autodesk más allá de las guardas de CI de ADR-0003. Sin una fuente única, subir una versión o el
límite de AutoCAD implicaba tocar varios archivos y arriesgar divergencias.

## 3. Alcance

Autorizado por el ROADMAP (fila I-12) y ADR-0003:

- `Directory.Build.props`: `RackCadVersion` + `Version`, `LangVersion`, `Nullable`, `Deterministic`
  y las propiedades de serie de AutoCAD (`RackCadAutoCADSeriesMin`/`Max`). Quitar `LangVersion` y
  `Nullable` (idénticos) de los cinco `.csproj`.
- `Directory.Build.targets` nuevo: resuelve `SourceRevisionId` (env `GITHUB_SHA` → `git rev-parse
  HEAD` → vacío) para estampar `InformationalVersion`.
- `deploy/RackCad.bundle/PackageContents.template.xml` nuevo (fuente única del manifiesto con
  marcadores de versión/serie); se elimina el `PackageContents.xml` estático.
- `src/RackCad.Plugin/RackCad.Plugin.csproj`: el target `AssembleAutoloaderBundle` pasa a
  `AfterTargets="Publish"`, genera el manifiesto desde la plantilla y arma el bundle desde el publish.
- `deploy/build-bundle.ps1` (publish canónico + verificación) y `deploy/verify-bundle.ps1`
  (fail-closed: allowlist, manifiesto/versión, escaneo recursivo Autodesk, inventario + hashes,
  modo comparación para reproducibilidad).
- `deploy/install-bundle.ps1`: `-Build` usa `dotnet publish` y toma el bundle del publish.
- `eng/ci/verify-autocad-references.ps1`: publica el Plugin (no solo compila), verifica el bundle en
  el publish e invoca `verify-bundle.ps1` fail-closed. Sin tocar `.github/workflows/ci.yml`.
- ADR-0004 (estrategia de versiones de AutoCAD) + índice de ADRs.
- `docs/guias/despliegue.md` y `README.md` donde el flujo cambió.

## 4. Fuera de alcance

- Actualizar AutoCAD o soportar 2026/2027 (solo se documenta la política; `SeriesMax=R25.0`).
- Cambiar comportamiento del producto, UI, catálogos, persistencia o handlers.
- Redistribuir material Autodesk; agregar caché, feeds privados o artifacts con DLL Autodesk
  (ADR-0003 intacto: versiones, fuente, finalidad y restricciones de las referencias sin cambios).
- Absorber cambios de I-14 (`architecture/ui-controls`) o I-19 (`feature/validador-catalogos`).
- Tocar `RackCad.sln` o `.github/workflows/ci.yml` (I-14 añadirá ahí su job de UI.Tests).

## 5. Contexto requerido

`AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md` (fila I-12), `docs/HANDOFF.md`,
`docs/ARCHITECTURE.md`, `docs/adr/0003-referencias-autocad-para-ci.md`, el Context Pack
`delivery-validation`, `docs/guias/despliegue.md`, y los archivos de build/deploy/CI listados en §3 y §7.

## 6. Dependencias

Ninguna dependencia previa (iniciativa de relleno; ROADMAP: "Depende de —", "Se estorba con —").
Coexiste con I-14 e I-19: ambas eran, al abrir I-12, reclamos vacíos sin cambios en archivos de
build/despliegue. I-12 no toca `RackCad.sln` ni `ci.yml` para no chocar con el job de UI.Tests que
I-14 añadirá; el único solapamiento posible es la eliminación de dos líneas en `RackCad.UI.csproj`
(y `RackCad.Tests.csproj`), resoluble mecánicamente en el rebase de integración. No requiere decisión
del dueño; ADR-0004 nace `propuesto` y solo el dueño lo acepta.

## 7. Archivos esperados

Nuevos: `Directory.Build.targets`, `deploy/RackCad.bundle/PackageContents.template.xml`,
`deploy/build-bundle.ps1`, `deploy/verify-bundle.ps1`,
`docs/adr/0004-estrategia-de-versiones-de-autocad.md`, este contrato,
`docs/automation/state/I-12.yml`.
Modificados: `Directory.Build.props`, los cinco `.csproj`, `src/RackCad.Plugin/RackCad.Plugin.csproj`
(target), `deploy/install-bundle.ps1`, `eng/ci/verify-autocad-references.ps1`,
`docs/guias/despliegue.md`, `README.md`, `docs/adr/README.md`.
Eliminado: `deploy/RackCad.bundle/PackageContents.xml`.
Una desviación material fuera de esta lista exige detenerse.

## 8. Fases

1. Centralizar versión/`LangVersion`/`Nullable`/`Deterministic` y series de AutoCAD en
   `Directory.Build.props`; limpiar los `.csproj`.
2. Estampado de SHA reproducible (`Directory.Build.targets`) con fallback definido.
3. Plantilla de `PackageContents.xml` + generación en el target; bundle por `dotnet publish`.
4. Scripts `build-bundle.ps1`/`verify-bundle.ps1`; actualizar `install-bundle.ps1` y la guarda de CI.
5. ADR-0004 y documentación (despliegue, README, índice de ADRs).
6. Verificación completa (restore, suite, build de solución, UI+Plugin Debug, publish/bundle desde
   árbol limpio, reproducibilidad de dos generaciones, inventario + hashes, ausencia de DLL Autodesk).

Cada fase termina con evidencia revisable en el cuerpo de sus commits.

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` (suite completa).
- `dotnet build RackCad.sln` y build Debug de UI y Plugin (0 errores; los MSB3277 conocidos no cuentan).
- `dotnet publish` del Plugin + `deploy/build-bundle.ps1` (+ `verify-bundle.ps1`) desde árbol limpio.
- Dos generaciones con las mismas entradas → mismo inventario y hashes (reproducibilidad).
- Regresión del instalador (`deploy/test-install-bundle.ps1`).
- CI verde en la rama; la guarda de ADR-0003 corre en CI (no localmente: exige runner sin AutoCAD).

## 10. Validacion manual

Ligera y responsabilidad del dueño antes de integrar: instalar el bundle regenerado
(`deploy/install-bundle.ps1 -Build`) y confirmar que AutoCAD 2025 lo **carga solo** al arranque y que
`RACKCAD` responde (el manifiesto cambió: se agregó `SeriesMax="R25.0"` y la versión se genera). No
cambia comportamiento de dibujo, así que no requiere los escenarios de geometría/BOM.

## 11. Criterios de aceptacion

- Una sola fuente de versión (`RackCadVersion`) alimenta ensamblados y manifiesto; subirla cambia todo.
- `InformationalVersion` = `x.y.z+<sha>`; build reproducible y con fallback limpio sin git/SHA.
- Bundle producido por `dotnet publish` con exactamente los cuatro DLL de RackCad + catálogos +
  manifiesto generado; `verify-bundle.ps1` en verde y fail-closed ante violaciones.
- Dos publish del mismo commit → inventario y hashes idénticos.
- Cero DLL Autodesk en publish, bundle y artifacts (evidencia recursiva); ADR-0003 sin cambios.
- Suite, builds y CI en verde. (Aceptación ≠ integración.)

## 12. Condiciones para detenerse

- Que I-14 o I-19 empiecen a modificar `Directory.Build.props`, `RackCad.sln`, `ci.yml`, o scripts de
  build/despliegue sin poder aislar los cambios con seguridad.
- Cualquier necesidad de cambiar versiones, fuente, finalidad o restricciones de las referencias
  Autodesk (exigiría nueva revisión de ADR-0003).
- Aparición de DLL Autodesk en cualquier salida, o fallo de una guarda fail-closed.
- Expansión de alcance hacia comportamiento de producto, UI, catálogos, persistencia o handlers.

## 13. Estado versionado y entrega del Pull Request

Estado transitorio en `docs/automation/state/I-12.yml`. No se abre Pull Request en esta corrida (el
push de la rama es respaldo; la integración es manual del dueño y no se activa auto-merge). Si el dueño
abre un PR, será único para la iniciativa. La ausencia de PR no bloquea el estado versionado publicado.

## 14. Evidencia final

Commits de la rama `refactor/versionado`, archivos de §7, resultados de §9 (incluida la
reproducibilidad de dos generaciones y el inventario + hashes del bundle), evidencia de ausencia de
DLL Autodesk, y confirmación de que `main` no fue modificada. La guarda de ADR-0003 se valida en CI.
