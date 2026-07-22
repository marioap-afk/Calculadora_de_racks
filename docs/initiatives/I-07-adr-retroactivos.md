---
schema: rackcad-initiative/v1
id: I-07
title: ADRs retroactivos
type: docs
status: integrated
branch: docs/adr-retroactivos
base_branch: main
priority:
size: S
depends_on: [I-06]
conflicts_with: [I-06]
context_packs:
  - documentation-governance
  - architecture-kernel
  - autocad-plugin
  - catalogs-data
  - persistence
  - system-selective
  - ui-editors
automation_state_path: docs/automation/state/I-07.yml
decision_paths:
  - docs/automation/decisions/I-07.md
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-07 — ADRs retroactivos

## 1. Objetivo

Retro-documentar como ADRs de una página las decisiones vigentes que aún viven en la tabla de
[HANDOFF §7](../HANDOFF.md) («Decisiones vigentes pendientes de I-07»), de modo que cada decisión tenga
un registro estable en `docs/adr/` y no vuelva a re-litigarse sin saber por qué se tomó. Al integrarse,
las filas de HANDOFF §7 efectivamente cubiertas por un ADR podrán retirarse de HANDOFF (trabajo del
integrador, no de esta iniciativa).

## 2. Problema

Trece decisiones que aún gobiernan el proyecto solo constan en una tabla de HANDOFF marcada como
«conservación temporal obligatoria». HANDOFF es el estado vivo, no el registro de decisiones; esas
decisiones necesitan la estructura de ADR (contexto, decisión, alternativas, consecuencias) para dejar
de depender de avisos «NO re-proponer» dispersos. La rama `docs/adr-retroactivos` ya había redactado
siete ADRs, pero fueron numerados cuando `main` solo tenía ADR-0001 y ADR-0002; desde entonces `main`
ocupó ADR-0003, 0004 y 0005, provocando una colisión de numeración que debe resolverse antes de indexar.

## 3. Alcance

Estrictamente limitado a la fila I-07 del ROADMAP («Retro-documentar las ~13 decisiones de HANDOFF §7
como ADRs de una página») y a las trece decisiones de HANDOFF §7:

- rebasar la rama sobre la punta vigente de `origin/main` antes de editar;
- renumerar los siete ADRs ya redactados desde el siguiente número libre tras el rebase (0006–0012),
  actualizando títulos, nombres de archivo, referencias cruzadas e índice;
- revisar esos siete ADRs contra la arquitectura, el código, los commits y la evidencia histórica
  vigentes, corrigiendo referencias obsoletas sin inventar fecha, decisores, alternativas ni evidencia;
- redactar ADRs breves (una decisión por ADR) para las decisiones de HANDOFF §7 aún sin ADR, solo cuando
  exista respaldo suficiente; donde el respaldo sea incompleto, registrar exactamente la limitación;
- actualizar `docs/adr/README.md`, este contrato, el estado versionado y el índice de iniciativas;
- entregar una matriz `decisión de HANDOFF §7 → ADR o limitación` para que el integrador retire después
  solo las filas realmente cubiertas.

## 4. Fuera de alcance

- Modificar el contenido normativo de ADRs ya aceptados (ADR-0001…0005) o cambiar sus estados.
- Marcar como `aceptado` cualquier ADR nuevo o renumerado: los redacta el agente en estado `propuesto`;
  solo el dueño acepta o rechaza.
- Cambiar código de producto, comportamiento, catálogos, pruebas, build, arquitectura objetivo o el
  roadmap de otras iniciativas.
- Editar `docs/HANDOFF.md` y `docs/ROADMAP.md`: son archivos calientes reservados para el último commit
  de la sesión de integración (WORKFLOW §7 y §4.5). La retirada de las filas de HANDOFF §7 la hace el
  integrador con la matriz de cobertura.
- Absorber otras iniciativas (I-18, I-23, I-25) o resolver la colisión histórica de numeración `0002`
  (`0002-paso0-evidencia.md` + `0002-secuencia-dinamico-modular.md`), que pertenece a ADRs aceptados de
  I-01/I-02 y queda fuera de este alcance.

## 5. Contexto requerido

- [AGENTS.md](../../AGENTS.md) y [WORKFLOW.md](../WORKFLOW.md) (proceso, archivos calientes, trailers).
- [ROADMAP.md](../ROADMAP.md) fila I-07; [HANDOFF §7](../HANDOFF.md) (las trece decisiones).
- [ARCHITECTURE.md](../ARCHITECTURE.md) y los Context Packs declarados.
- [docs/adr/README.md](../adr/README.md) y [docs/adr/plantilla.md](../adr/plantilla.md).
- [docs/initiatives/README.md](README.md) y [TEMPLATE.md](TEMPLATE.md).
- Evidencia histórica: [auditoría 2026-07](../auditoria-arquitectura-2026-07.md),
  [historial de HANDOFF preservado](../archivo/transicion-2026-07/handoff-historial-2026-07.md),
  [ideas-futuras.md](../ideas-futuras.md).

## 6. Dependencias

- **I-06** (reestructura documental) debía integrarse antes de desbloquear I-07 y de que existieran los
  Context Packs y `ARCHITECTURE.md`; está **integrada (2026-07-17)**, así que la dependencia y el estorbo
  mutuo I-06↔I-07 están satisfechos.
- No requiere entradas del dueño para proceder: los ADRs se redactan `propuesto` y el dueño los revisa en
  la integración.

## 7. Archivos esperados

- `docs/adr/0006-…0012-*.md`: los siete ADRs existentes renumerados (renombrado + ajustes de referencia).
- `docs/adr/0013-…0018-*.md`: los ADRs nuevos con respaldo suficiente.
- `docs/adr/README.md`: índice y tabla actualizados.
- `docs/initiatives/I-07-adr-retroactivos.md` (este contrato) y `docs/initiatives/README.md` (índice).
- `docs/automation/state/I-07.yml`: estado versionado.
- Matriz de cobertura HANDOFF §7 → ADR/limitación (sección 11 de este contrato).
- Diff vacío bajo `src/`, `assets/` y `deploy/`. Sin tocar `docs/HANDOFF.md` ni `docs/ROADMAP.md`.

## 8. Fases

1. Rebase sobre `origin/main` y renumeración 0006–0012 con referencias cruzadas.
2. Revisión de los siete ADRs contra arquitectura/código/commits (corrección de rutas obsoletas por la
   partición de `RackFrameCommands` de I-09; reformulación de decisores; precisión de la excepción NuGet).
3. Redacción de los ADRs nuevos (0013–0018) para las decisiones 8–13, con evidencia verificada.
4. Contrato, estado versionado, índices y matriz de cobertura.
5. Validaciones (numeración única, enlaces locales, índice↔archivos, `git diff --check`, diff vacío en
   `src`/`assets`/`deploy`), commits en español con trailer de procedencia, push `--force-with-lease` y
   CI verde sobre el SHA publicado.
6. **Integración (integrador, fuera de esta corrida):** rebase final si `origin/main` avanzó, revisión de
   los ADRs `propuesto` por el dueño, retirada de las filas cubiertas de HANDOFF §7 y marca en ROADMAP
   como último commit de la rama, merge `--no-ff` y limpieza segura de rama y worktree.

## 9. Pruebas y builds

Iniciativa solo de documentación: sin build de producto ni AutoCAD. La compuerta técnica es **CI verde
en la rama** sobre el SHA publicado (`requires_ci: true`). `requires_plugin_build: false`,
`requires_autocad: false`.

## 10. Validacion manual

No aplica validación en AutoCAD (no cambia dibujo, comandos ni comportamiento). La revisión pertinente es
documental: numeración y enlaces, correspondencia índice↔archivos y cobertura de las trece decisiones. La
**aceptación de los ADRs `propuesto`** es un acto separado y exclusivo del dueño, en la integración.

## 11. Criterios de aceptacion

- Numeración de ADR única y secuencial (0006–0018) y referencias cruzadas coherentes; la colisión
  histórica `0002` queda documentada como fuera de alcance.
- Todos los enlaces Markdown locales de los ADRs nuevos y renumerados resuelven; el índice corresponde con
  los archivos.
- Las trece decisiones de HANDOFF §7 quedan cubiertas por un ADR o con su limitación registrada; la matriz
  lo hace explícito.
- `git diff --check` limpio; diff vacío bajo `src/`, `assets/` y `deploy/`; `docs/HANDOFF.md` y
  `docs/ROADMAP.md` intactos.
- Commits en español con trailer `Co-Authored-By`; rama publicada con CI verde. Sin merge ni cambios en
  `main`.

### Matriz de cobertura — HANDOFF §7 → ADR

| # | Decisión de HANDOFF §7 | ADR | Respaldo |
|---|---|---|---|
| 1 | Solo `RackCad.Plugin` toca AutoCAD | [ADR-0006](../adr/0006-autocad-solo-en-plugin.md) | Arquitectura + AGENTS + código de capas |
| 2 | Catálogos CSV Excel-first, sin base de datos | [ADR-0007](../adr/0007-catalogos-csv-excel-first.md) | `CsvCatalogReader`/`JsonRackCatalogProvider` + guías |
| 3 | Un solo `secciones.csv` con columna `rol` | [ADR-0008](../adr/0008-secciones-unificadas-por-rol.md) | `SeccionRoles`/`JsonRackCatalogProvider` + pruebas |
| 4 | Identidad por GUID embebido en DWG | [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md) | `RackEmbedDocument`/`RackBlockData` + BOM por Id |
| 5 | `Actualizar` redibuja; `Insertar` agrega vista ligada | [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md) | Comandos por sistema + `RackBlockFinder` |
| 6 | Parámetros dinámicos mediante patrón ARRAY | [ADR-0011](../adr/0011-parametros-dinamicos-con-patron-array.md) | `HeaderInstanceGrouper`/`LateralHeaderDrawer` |
| 7 | Cero dependencias NuGet en producto, salvo ADR-0003 | [ADR-0012](../adr/0012-producto-sin-dependencias-nuget.md) | csproj de producto + `XlsxWriter` + ADR-0003 |
| 8 | Parrilla una por tarima (`SelectiveFrontalBuilder.ParrillaRow`) | [ADR-0013](../adr/0013-parrilla-una-por-tarima.md) | `ParrillaRow` (XML-doc «one deck per tarima») + AGENTS |
| 9 | Copia de `SelectiveSafetySelection` centralizada en `DeepCopy` | [ADR-0014](../adr/0014-copia-central-seguridad-selectivo.md) | `SelectivePalletDesign.DeepCopy` + DTO + round-trip |
| 10 | Entrada numérica localizada sin agrupadores | [ADR-0015](../adr/0015-entrada-numerica-localizada.md) | `LocalizedNumberParser`/`NumericFieldValidation` + pruebas |
| 11 | Cantidad de parrilla: UI rechaza si no cabe y builder acota | [ADR-0016](../adr/0016-cantidad-parrilla-acotada.md) | `ParrillaRow` (clamp) + `SafetyParrillaGridWindow` |
| 12 | Validación de cargas diferida a RAM Elements | [ADR-0017](../adr/0017-validacion-cargas-diferida-ram-elements.md) | Decisión del dueño (auditoría + historial); ver limitación |
| 13 | Optimizador IA de layout diferido | [ADR-0018](../adr/0018-optimizador-layout-ia-diferido.md) | Arquitectura §4.5 + ideas-futuras + `WarehouseGridPlanner` |

**Limitaciones de evidencia registradas:** las trece decisiones tienen respaldo suficiente para el
registro. Los ADRs 0017 y 0018 documentan **diferimientos por decisión explícita del dueño** cuyo respaldo
es documental (auditoría 2026-07, historial de HANDOFF, ideas-futuras); esa evidencia establece el
diferimiento y su motivo, pero **no** la fecha original ni un estudio comparativo detallado, y los ADRs lo
declaran así sin inventarlos. Para los trece ADRs (renumerados y nuevos), la evidencia conservada no
identifica a los decisores originales; ese límite queda escrito en cada registro. **El dueño aceptó los
trece registros el 2026-07-22** («Sí, apruebo»; decisión versionada en
[`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md)), sin modificarlos y conservando
estas limitaciones; su estado pasó de `propuesto` a `aceptado`.

## 12. Condiciones para detenerse

- Necesidad de tocar `docs/HANDOFF.md` o `docs/ROADMAP.md` (reservados a la integración).
- Falta de respaldo verificable para una decisión: se registra la limitación en vez de inventar evidencia.
- Cualquier conflicto de rebase que exija reconciliar contenido de otra iniciativa: preservar el trabajo
  ajeno y detenerse ante ambigüedad.
- Presión para aceptar un ADR sin autorización documental del dueño: se deja `propuesto`.

## 13. Estado versionado y entrega del Pull Request

Estado canónico en [`docs/automation/state/I-07.yml`](../automation/state/I-07.yml)
(`schema: rackcad-automation-state/v1`). Ejecución **manual, solo Git** (sin Pull Request; `automation.enabled: false`).
Git y los resultados verificables prevalecen sobre el estado versionado. `completed` no significa
integrada: la integración sigue siendo manual y la hace el integrador.

## 14. Evidencia final

Rama `docs/adr-retroactivos` rebasada sobre `origin/main` (`6d080eb`) sin conflictos; siete ADRs
renumerados 0006–0012 y seis nuevos 0013–0018; índice de ADR e índice de iniciativas actualizados; matriz
de cobertura completa; sin cambios bajo `src/`, `assets/`, `deploy/`, `docs/HANDOFF.md` ni
`docs/ROADMAP.md`. Commits en español con trailer de procedencia; rama publicada con `--force-with-lease`
y CI verde sobre el SHA final (candidato `600b22e`). `main` no fue modificada durante la implementación.

**Integración (2026-07-22):** el dueño aceptó los trece registros («Sí, apruebo»,
[`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md)); los ADR-0006…0018 pasaron a
`aceptado`; en el último commit de la rama se retiraron las trece decisiones de HANDOFF §7 con su aviso
temporal (cubiertas por los ADRs) y se marcó I-07 `integrada (2026-07-22)` en ROADMAP. Diff vacío bajo
`src/`, `assets/` y `deploy/`. El SHA del merge `--no-ff` queda registrado en `git log --first-parent main`.
