---
schema: rackcad-initiative/v1
id: I-18
title: Push Back
type: feature
status: en-implementacion
branch: feature/push-back
base_branch: main
priority:
size: L
depends_on: [I-10, I-11, I-15, I-16]
conflicts_with: []
context_packs: [architecture-kernel, system-dynamic-flowbed, ui-editors, autocad-plugin, catalogs-data, persistence, delivery-validation]
automation_state_path: docs/automation/state/I-18.yml
decision_paths: [docs/automation/decisions/I-18.md]
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

# Push Back

> Alcance del ROADMAP (fila I-18, Fase 4) y `ARCHITECTURE.md` §7.1; sin ampliación lateral. El campo
> `priority` se deja vacío por falta de fuente numérica en el ROADMAP. La iniciativa se parte en
> **PB-0 → I-18a → I-18b → I-18c** (§8). Esta rama implementa por entregas; **I-18a** (núcleo puro) es
> la primera entrega de código.

## 1. Objetivo

Agregar **Push Back** como el **primer sistema construido sobre el patrón de módulos** listo tras las
Fases 2-3 (`ARCHITECTURE.md` §7.1: descriptor · documento versionado · resolver diseño→sistema ·
builders por vista→`SystemPlan` · BOM · estado de editor puro + vista WPF · draw adapter), **sin editar
el código de otros sistemas** (criterio de salida de la Fase 4; "prueba de fuego" de §7.1). Compone lo
compartido existente —la cama `FlowBedType.Pushback` (`FlowBedLateralBuilder`) y la geometría del
Dinámico— sin duplicar su aritmética. Al cerrar (I-18c) entrega `docs/guias/agregar-un-sistema.md`
validada por la experiencia real, retirando el patrón temporal de `ARCHITECTURE.md` §8 (DOC-02).

## 2. Problema

RackCad mantiene cuatro familias (cabecera, selectivo, dinámico, cama). Push Back es un producto real
faltante. Las Fases 2-3 construyeron los contratos compartidos precisamente para que el sistema N+1 se
agregue dentro de su módulo; falta la prueba de que el costo esté confinado al módulo.

## 3. Alcance

- **I-18a (esta entrega):** diseño y resolución puros (Domain/Application), builders por vista →
  `SystemPlan`, BOM, y persistencia versionada (documento + round-trip + legacy + campos desconocidos
  I-11). Reutiliza la cama `FlowBedType.Pushback` y la geometría del Dinámico. **No** registra el
  sistema en ningún `Registry`, ni UI/Plugin/dibujo.
- **I-18b:** registro aditivo (enum, slot, tres `Registry`, handler, `RackInsertionRequest`, módulo),
  editor sobre el shell, draw adapter y comando → sistema usable end-to-end (✋ AutoCAD).
- **I-18c:** guía `agregar-un-sistema.md` y cierre.

## 4. Fuera de alcance

- Editar la lógica o los `switch` de otros sistemas para acomodar Push Back (consumir contratos
  compartidos SÍ; alterar su lógica NO → detenerse, §12).
- Inventar bloques DWG o filas de catálogo (los provee el Owner; PB-0).
- En I-18a: registros globales, descriptor visible, handler de Plugin, editor WPF, comandos, dibujo
  AutoCAD, integración con menú/biblioteca y la guía (todo eso es I-18b/I-18c).
- Validación de cargas estructurales (diferida), optimizador de layout IA, I-25, I-23, rediseño visual.

## 5. Contexto requerido

- Global: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/AUTOMATION_PLAN.md`, `docs/ROADMAP.md` (fila I-18 +
  principios 4/5), `docs/ARCHITECTURE.md` §§3.3-3.4, §4, §7.
- Context Packs: `system-dynamic-flowbed`, `architecture-kernel`, `persistence`, `catalogs-data`,
  `ui-editors`, `autocad-plugin`, `delivery-validation`.
- Código de referencia (componer, NO copiar): `Dynamic*` (resolver, geometría, builders, cama compuesta,
  BOM, documento) y `FlowBed*`; la seguridad `SelectiveTopeConfig` (patrón `OffCells`).

## 6. Dependencias

- Integradas en `main`: I-10, I-11, I-15, I-16. ✓
- **PB-0 (prerrequisito del Owner):** bloques DWG + filas de catálogo + decisiones funcionales. **Provisto
  por el Owner** (ver [`docs/automation/decisions/I-18.md`](../automation/decisions/I-18.md) y los CSV de
  esta rama). Con PB-0 resuelto, I-18a es elegible.

## 7. Archivos esperados

- **Nuevos (I-18a):** `src/RackCad.Domain/Systems/PushBack*.cs`;
  `src/RackCad.Application/Systems/PushBack*.cs` (resolver, geometría de extremos, builders lateral/
  frontal/planta → `SystemPlan`, BOM); `src/RackCad.Application/Persistence/PushBackDesignDocument.cs`;
  `tests/RackCad.Tests/PushBack*Tests.cs`.
- **Datos (Owner, append-only, ya en la rama):** `assets/catalogs/secciones.csv`, `blocks.csv`,
  `connection-layout.csv` (pieza `LARGUERO_ESCALON_TROQUEL_REDONDO`).
- **Baselines de catálogo a actualizar por las filas autorizadas:**
  `tests/RackCad.Tests/ShippedCatalogIntegrityTests.cs` (secciones 6→7, BeamProfiles 3→4).
- **NO en I-18a:** `RackSystemKind`, `RackProject(.Document)`, los tres `*Registry`, UI, Plugin.

## 8. Fases

- **PB-0** (Owner): bloques + catálogo + decisiones funcionales — [decisions/I-18.md](../automation/decisions/I-18.md). **Resuelto.**
- **I-18a** (esta entrega, por incrementos): núcleo puro + plan + BOM + persistencia + pruebas golden/round-trip.
  - **Hecho:** núcleo puro (diseño/resolver/cama/persistencia) y correcciones — **haz alto por frente×nivel**
    (`PushBackFrontConfig`, default 3.5 explícito), **persistencia** con round-trip por dominio + no-degradación
    (`FromDomain(design, source)`), y **rechazo de GUIA** en el resolver. 23 pruebas; suite 1036 verde.
  - **Pendiente (items 4-7):** geometría propia (end beams bajo IN/OUT + alto TROQUEL_REDONDO, snap 2", eje de
    cama TROQUEL_CAMA→INICIO_IZQ/DER, intermedios tangentes, cama full-span sin frenos) + **composición de caja
    negra** de los `SystemPlan` lateral/frontal/planta (quitar por Role/PieceId lo dinámico específico, agregar lo
    Push Back, reagrupar con `HeaderInstanceGrouper`) + `PushBackBomBuilder` + golden. Diseño accionable en el
    informe de la sesión y en `state/I-18.yml`. **I-18a NO se marca completa hasta cubrirlos.**
- **I-18b**: sistema usable end-to-end (registros, editor, handler, dibujo) — ✋ AutoCAD.
- **I-18c**: guía y cierre.

## 9. Pruebas y builds

`dotnet test` (resolver, builders con golden de plan, BOM, round-trip incl. legacy, validador I-19 sin
errores nuevos); build Debug de UI y Plugin (0 errores propios); CI verde. I-18a **no** requiere
validación en AutoCAD (sin cableado de Plugin); lo que se validará en I-18b queda explícito en §10.

## 10. Validación manual

- **I-18a:** `requires_autocad: no` (puro). **Para I-18b se deberá validar en AutoCAD:** inserción de
  Push Back; vistas ligadas (lateral/frontal/planta); pendiente 7/16"/pie con extremo bajo a la
  izquierda; IN/OUT solo en el extremo bajo y `LARGUERO_ESCALON_TROQUEL_REDONDO` solo en el alto; cama
  `Pushback` sin frenos con longitud = span estructural completo (sin −4"); tope posterior por celda;
  `RACKEDITAR` round-trip mismo GUID; BOM sin doble conteo; persistencia/legacy.

## 11. Criterios de aceptación

- I-18a: suite verde (golden de plan lateral/frontal/planta + BOM + round-trip); golden del Dinámico
  **idénticos**; validador I-19 sin errores nuevos; **ningún registro global tocado**; Domain/Application
  puros. Las 20 pruebas obligatorias del encargo cubiertas.

## 12. Condiciones para detenerse

- Falta PB-0 (bloques/filas/decisiones).
- Agregar Push Back exige **alterar la lógica de Selectivo o Dinámico** (no solo componer helpers puros):
  detenerse y reportar el contrato compartido faltante antes de forzar el cambio.
- Cualquier condición general de AUTOMATION_PLAN §12.

## 13. Estado versionado y entrega del Pull Request

Estado canónico: [`docs/automation/state/I-18.yml`](../automation/state/I-18.yml). Decisiones del Owner:
[`docs/automation/decisions/I-18.md`](../automation/decisions/I-18.md). Merge automático prohibido;
integración serializada del Owner. `HANDOFF.md`/`ROADMAP.md` **no** se tocan hasta la integración.

## 14. Evidencia final

Commits atómicos, archivos, pruebas, builds y CI de la rama; confirmación de que `main` no fue
modificada y de que los planes del Dinámico no cambiaron. La validación en AutoCAD corresponde a I-18b.
