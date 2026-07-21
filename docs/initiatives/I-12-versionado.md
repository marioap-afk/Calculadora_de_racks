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
decision_paths:
  - docs/adr/0004-estrategia-de-versiones-de-autocad.md
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
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
  modo comparación para reproducibilidad, y comparación por SHA-256 de los cuatro DLL contra el
  publish y de los catálogos contra `assets/catalogs` —inventario derivado de la fuente, sin número
  fijo—).
- `deploy/test-verify-bundle.ps1` nuevo: harness versionado del verificador (bundle válido + casos
  negativos), ejecutado dentro de la guarda de CI.
- `deploy/install-bundle.ps1`: `-Build` usa el flujo canónico de `deploy/build-bundle.ps1` (publish +
  verificación fail-closed); ningún bundle de `-Build` llega a staging o destino sin pasar
  `verify-bundle.ps1`.
- `eng/ci/verify-autocad-references.ps1`: publica el Plugin (no solo compila), verifica el bundle en
  el publish e invoca `verify-bundle.ps1` y el harness del verificador, fail-closed. Sin tocar
  `.github/workflows/ci.yml`.
- ADR-0004 (estrategia de versiones de AutoCAD) + índice de ADRs; coherencia "AutoCAD 2025" en
  bundle/instalador/manifiesto.
- `docs/guias/despliegue.md` donde el flujo cambió (README auditado en esta revisión: no necesitó cambios).

## 4. Fuera de alcance

- Actualizar AutoCAD o soportar 2026/2027 (solo se documenta la política; `SeriesMax=R25.0`).
- Cambiar comportamiento del producto, UI, catálogos, persistencia o handlers.
- Redistribuir material Autodesk; agregar caché, feeds privados o artifacts con DLL Autodesk
  (ADR-0003 intacto: versiones, fuente, finalidad y restricciones de las referencias sin cambios).
- Absorber cambios de I-19 (`feature/validador-catalogos`), aún activa.
- Cambiar comportamiento de los controles o pruebas de UI de I-14 (ya integrada); I-12 solo elimina de
  `RackCad.UI.Tests.csproj` las dos propiedades ya centralizadas.
- Reintroducir cambios propios en `RackCad.sln` o `.github/workflows/ci.yml`: I-14 ya añadió ahí su
  proyecto y su job `ui-tests`; I-12 los hereda del rebase sin tocarlos.

## 5. Contexto requerido

`AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md` (fila I-12), `docs/HANDOFF.md`,
`docs/ARCHITECTURE.md`, `docs/adr/0003-referencias-autocad-para-ci.md`, el Context Pack
`delivery-validation`, `docs/guias/despliegue.md`, y los archivos de build/deploy/CI listados en §3 y §7.

## 6. Dependencias

Ninguna dependencia previa (iniciativa de relleno; ROADMAP: "Depende de —", "Se estorba con —").

**I-14 (`architecture/ui-controls`) ya está integrada en `main`** (merge en `origin/main`); no está
activa. Aportó los controles comunes de UI y sus pruebas, el proyecto `tests/RackCad.UI.Tests`, su job
de CI `ui-tests` en `.github/workflows/ci.yml` y su entrada en `RackCad.sln`, y actualizó ROADMAP y
HANDOFF. I-12 se **rebasó sobre ese `main`** y hereda todo eso sin modificarlo (rebase sin conflictos de
archivos: cero solapamiento con los archivos de I-12).

**Conflicto semántico con I-14, resuelto durante el rebase**: `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`
declaraba `<LangVersion>latest</LangVersion>` y `<Nullable>disable</Nullable>`, ya centralizados por I-12
en `Directory.Build.props` con esos mismos valores. Como I-12 integra en segundo lugar, esas dos líneas
se **eliminaron de ese `.csproj`** (ahora hereda ambos valores; sin cambio de comportamiento; se
conservan `TargetFramework`, `UseWPF`, `RollForward` y las demás propiedades específicas). No se tocó
ningún otro entregable de I-14.

I-19 (`feature/validador-catalogos`) sigue **activa**: I-12 **no** absorbe sus cambios.

ADR-0004 fue **aceptado por el dueño** (Mario Pérez, 2026-07-21): RackCad soporta una sola serie de
AutoCAD a la vez; hoy `SeriesMin = SeriesMax = R25.0` (solo AutoCAD 2025). La validación manual de
autocarga en AutoCAD 2025 fue **ejecutada y aprobada** por el dueño el 2026-07-21
([evidencia](I-12-autocad-validation.md)); I-12 queda **lista para integración serializada**.

## 7. Archivos esperados

Nuevos: `Directory.Build.targets`, `deploy/RackCad.bundle/PackageContents.template.xml`,
`deploy/build-bundle.ps1`, `deploy/verify-bundle.ps1`, `deploy/test-verify-bundle.ps1`,
`docs/adr/0004-estrategia-de-versiones-de-autocad.md`, este contrato,
`docs/automation/state/I-12.yml`.
Modificados: `Directory.Build.props`, los cinco `.csproj`, `src/RackCad.Plugin/RackCad.Plugin.csproj`
(target), `deploy/install-bundle.ps1`, `deploy/test-install-bundle.ps1` (regresiones -Build),
`eng/ci/verify-autocad-references.ps1`, `docs/guias/despliegue.md`, `docs/adr/README.md`, y —en el
rebase sobre I-14— `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` (elimina las dos propiedades ya
centralizadas).
Eliminado: `deploy/RackCad.bundle/PackageContents.xml`.
README fue auditado y **no** necesitó cambios (queda fuera de "modificados").
Una desviación material fuera de esta lista exige detenerse.

## 8. Fases

1. Centralizar versión/`LangVersion`/`Nullable`/`Deterministic` y series de AutoCAD en
   `Directory.Build.props`; limpiar los `.csproj`.
2. Estampado de SHA reproducible (`Directory.Build.targets`) con fallback definido.
3. Plantilla de `PackageContents.xml` + generación en el target; bundle por `dotnet publish`.
4. Scripts `build-bundle.ps1`/`verify-bundle.ps1`; actualizar `install-bundle.ps1` y la guarda de CI.
5. ADR-0004 y documentación (despliegue e índice de ADRs; README auditado, sin cambios).
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

**Resultado: APROBADA** por el dueño el 2026-07-21 sobre `5d5f0dc` (publish Release; `verify-bundle.ps1`
105 comprobaciones; DLL idénticos al publish; catálogos idénticos a `assets/catalogs`; cero DLL
Autodesk; instalación en `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle`; **autocarga PASS** y
`RACKCAD` **PASS**). Evidencia: [I-12-autocad-validation.md](I-12-autocad-validation.md).

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
