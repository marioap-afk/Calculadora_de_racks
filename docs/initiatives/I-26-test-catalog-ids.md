---
schema: rackcad-initiative/v1
id: I-26
title: TestCatalogIds y cobertura de catálogos
type: refactor
status: completed
branch: refactor/test-catalog-ids
base_branch: main
priority: 20
size: S
depends_on: []
conflicts_with: []
context_packs:
  - catalogs-data
  - delivery-validation
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-26 — TestCatalogIds y cobertura de catálogos

## 1. Objetivo

Crear un contrato de pruebas independiente que declare los IDs canónicos usados por la suite,
compruebe su existencia en los catálogos distribuidos y publique cobertura Cobertura como artifact
del job de tests existente en CI.

## 2. Problema

Los tests repiten IDs reales como literales y algunos escenarios pueden continuar verdes si una
pieza distribuida desaparece porque seleccionan por rol, usan el primer elemento disponible o caen
a defaults. Las constantes de producto cubren únicamente una parte de esas expectativas y no deben
convertirse en la fuente de verdad de las pruebas. CI ejecuta la suite, pero no conserva un mapa de
cobertura de Domain y Application.

## 3. Alcance

- Crear `TestCatalogIds`, test-only, con constantes literales independientes agrupadas por las
  colecciones reales de `RackCatalog`.
- Migrar solamente las expectativas canónicas contra datos distribuidos; conservar locales los
  fixtures sintéticos, inválidos deliberados, nombres de bloque y demás textos de comportamiento.
- Crear un guardián que cargue los catálogos copiados al output y acumule IDs, vistas, conexiones,
  relaciones esenciales y divergencias con constantes de producto faltantes.
- Añadir `coverlet.collector` exclusivamente a `RackCad.Tests` y producir Cobertura en la única
  ejecución de la suite del job Ubuntu.
- Normalizar y publicar el XML como artifact de CI con retención acotada.

## 4. Fuera de alcance

- Cambios funcionales en Domain, Application, UI o Plugin.
- Cambios de IDs, filas o contenido bajo `assets/catalogs`.
- Cambios en `blocks-library.dwg`, `deploy` o el build del Plugin.
- El validador exhaustivo de catálogos, severidades, duplicados y manifest de bloques de I-19.
- Umbral mínimo, reporte HTML, badge o servicio externo de cobertura.
- Actualizaciones de `Microsoft.NET.Test.Sdk` u otros paquetes de producto.
- Refactors laterales, automatizaciones, Pull Requests, merge o limpieza de la rama/worktree.
- Actualizar `docs/HANDOFF.md` o `docs/ROADMAP.md` antes de la sesión de integración.

`POSTE_OMEGA_3X3` es un fixture sintético y permanece como literal local. No se trata como ID
canónico distribuido.

## 5. Contexto requerido

- `AGENTS.md`, `docs/ARCHITECTURE.md`, `docs/WORKFLOW.md`, `docs/HANDOFF.md` y `docs/ROADMAP.md`.
- Context Packs `catalogs-data` y `delivery-validation`, con sus guías requeridas.
- `docs/guias/catalogos-y-plantillas.md` y `docs/guias/modelo-de-datos.md`.
- `src/RackCad.Application/Catalogs`, `src/RackCad.Application/RackFrames/CatalogIds.cs`, los
  catálogos distribuidos, el proyecto de tests y `.github/workflows/ci.yml`.

No se cargan Context Packs de sistemas, UI o Plugin porque el alcance no modifica esas capas.

## 6. Dependencias y paralelismo

La iniciativa no tiene dependencias ni conflictos declarados. Puede convivir con I-07. Antes de
editar `ci.yml` se verifica que I-13 no lo haya modificado en remoto; una diferencia concurrente en
ese archivo detiene la fase de cobertura. La validación del dueño es documental y de CI: AutoCAD no
aplica.

## 7. Archivos esperados

Nuevos:

- `tests/RackCad.Tests/TestCatalogIds.cs`.
- `tests/RackCad.Tests/CatalogCanonicalIdsTests.cs`.
- Este contrato.

Modificados:

- Tests que contienen expectativas canónicas auditadas.
- `tests/RackCad.Tests/RackCad.Tests.csproj`.
- `.github/workflows/ci.yml` y `.gitignore`.
- `docs/initiatives/README.md` para enlazar este contrato.

No se esperan cambios bajo `src`, `assets/catalogs`, `deploy`, `docs/HANDOFF.md` o
`docs/ROADMAP.md`.

## 8. Fases

- [x] F0. Reclamo atómico y baseline.
- [x] F1. Publicar este contrato detallado.
- [x] F2. Crear `TestCatalogIds` con sólo expectativas canónicas justificadas.
- [x] F3. Migrar tests por ocurrencia y contexto sin alterar su intención.
- [x] F4. Añadir el guardián y demostrar temporalmente su fallo ante un ID ausente.
- [x] F5. Añadir Coverlet, normalización y artifact de Cobertura al CI existente.
- [x] F6. Ejecutar la validación completa y dejar la rama `review-ready`.
- [x] F7. Preparar la integración manual y dejar documentados sus gates.
- [ ] Paso posterior. Merge manual, CI de `main` y limpieza segura de rama/worktree.

## 9. Pruebas y cobertura

Validaciones requeridas:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
git diff origin/main --check
```

El guardián se ejecuta además de forma dirigida, primero verde, después contra una copia ignorada
del catálogo con un ID retirado y finalmente verde tras restaurarla. La cobertura se genera con el
collector XPlat en formato Cobertura bajo `artifacts/TestResults`, se normaliza a
`artifacts/coverage/coverage.cobertura.xml` y debe incluir módulos de Domain y Application.

I-26 no establece umbral de cobertura. Coverlet es una dependencia privada del proyecto de tests y
no se añade a ningún proyecto de producto.

## 10. Validación manual

AutoCAD: no aplica. El dueño confirmó CI #40 verde sobre la punta de implementación, incluidos los
jobs de tests y build UI, y descargó `rackcad-coverage-cobertura` con el XML normalizado esperado.
El nuevo commit documental de preparación debe recibir su propio CI verde antes del merge.

## 11. Criterios de aceptación

- `TestCatalogIds` agrupa constantes literales test-only y no referencia `CatalogIds` de producto ni
  inicializa expectativas leyendo CSV/JSON.
- Los fixtures sintéticos, valores inválidos deliberados, IDs internos y nombres exactos de bloque
  permanecen locales; `POSTE_OMEGA_3X3` no se centraliza.
- El guardián carga los datos distribuidos con los proveedores reales, acumula todos los faltantes y
  distingue categoría, relación y divergencia de producto en un único mensaje ordenado.
- Las relaciones comprobadas son sólo las esenciales usadas por la suite; el test no sustituye I-19.
- La prueba negativa identifica el ID retirado y no deja modificaciones en datos fuente o generados
  versionados.
- La suite y el build UI Debug terminan sin fallos, errores ni advertencias propias.
- CI ejecuta la suite una sola vez en Ubuntu, produce Cobertura, normaliza un único XML y lo publica
  con retención de 14 días; el job UI de Windows y el trigger `push` conservan su comportamiento.
- No hay cambios bajo `src`, `assets/catalogs`, Plugin, UI, Domain/Application o `deploy`.

## 12. Condiciones para detenerse

- Cumplir una expectativa requiere modificar producto o catálogos distribuidos.
- El guardián necesita validación exhaustiva propia de I-19.
- I-13 modifica concurrentemente `.github/workflows/ci.yml`.
- Coverlet exige actualizar el Test SDK, duplicar la suite, añadir un servicio externo o un secreto.
- `origin/main` avanza con conflictos semánticos, aparece otra sesión en este worktree o una
  validación deja el árbol en estado no recuperable.

## 13. Entrega e integración manual

Cada fase coherente termina en commit con trailer `Co-Authored-By` y push normal de
`refactor/test-catalog-ids`. La implementación queda `completed`; la automatización continúa
deshabilitada, no existe archivo de estado automatizado ni Pull Request y el merge sigue siendo una
operación manual externa a este contrato.

La preparación de integración comprobó la base final, repitió las validaciones y actualizó
HANDOFF/ROADMAP. El merge `--no-ff`, el CI posterior de `main` y la limpieza segura permanecen
pendientes y requieren autorización separada del dueño.

## 14. Evidencia final

F1-F6 están completas. La suite, el guardián, el build UI Debug y la generación/normalización local
de Cobertura terminaron correctamente. La prueba negativa retiró temporalmente el poste estándar
de la copia ignorada de `secciones.csv`: el fallo acumuló el ID y las relaciones de plantillas
afectadas; la reconstrucción restauró la copia antes de repetir el guardián en verde.

La rama no modifica producto, catálogos distribuidos, Plugin ni deploy. La validación del dueño y la
preparación documental de HANDOFF/ROADMAP están completas; la automatización continúa deshabilitada.
Permanecen pendientes el CI remoto del commit documental final, el merge manual, el CI de `main` y
la limpieza de rama/worktree. Los conteos de pruebas y hashes canónicos viven en `docs/HANDOFF.md`
según WORKFLOW.
