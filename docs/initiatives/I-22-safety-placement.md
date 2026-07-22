---
schema: rackcad-initiative/v1
id: I-22
title: Colocación de seguridad
type: refactor
status: in-progress
branch: refactor/safety-placement
base_branch: main
priority:
size: M
depends_on: [I-14, I-20]
conflicts_with: [I-20]
context_packs:
  - architecture-kernel
  - system-selective
  - persistence
  - ui-editors
  - delivery-validation
automation_state_path: docs/automation/state/I-22.yml
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

# I-22 — Colocación de seguridad

## 1. Objetivo

Cerrar los hallazgos **E6** y **E7** de la auditoría 2026-07 sobre la seguridad del selectivo, **sin
cambio de comportamiento observable**:

1. **Servicios puros de colocación por familia, parametrizados por vista** (E6): extraer la aritmética
   de colocación/conteo que hoy está triplicada frontal/lateral/planta+BOM para las familias **tope**
   (larguero tope), **parrilla** (deck), **tarima** (referencia visual) y el **separador**, dejando los
   builders como **orquestadores** que consumen un plan único por familia — el mismo patrón que ya
   ejemplifican `SelectiveDesviadorPlan`/`SelectiveDesviadorDrawing` y `SelectiveParrillaPlan`. La regla
   de cada familia vive en **un solo sitio** (ARQUITECTURA §1.3, AGENTS §2).
2. **Configuraciones y DTO separados por subtipo** (E7): descomponer la God-data-class
   `SelectiveSafetySelection` en **configuraciones por familia** (`SelectiveTopeConfig`,
   `SelectiveDesviadorConfig`, `SelectiveParrillaConfig`, `SelectiveDefensaConfig`,
   `SelectiveGuiaConfig`), cada una con su **`DeepCopy` propio** (mata el DeepCopy campo-a-campo que
   crece con cada familia) y su **mapeo de persistencia por familia** con fallback legacy y
   round-trip explícitos.
3. **Una sola fuente para el paso de troquel**: enrutar los **5 sitios** que hoy hardcodean `2.0` como
   paso de rejilla a la constante de dominio existente `SelectiveRackDefaults.TroquelPaso`.
4. **Adopción de `SelectionMatrix`** (control común de I-14) por las **rejillas de seguridad
   frente×nivel correspondientes** del editor selectivo, extendiendo el control con soporte de **celda
   ausente** para las rejillas dentadas (jagged).

Resultado verificable cuando: la solución compila; `RackCad.Tests` cubre los planes puros por familia,
la equivalencia de diseño resuelto (multiset de instancias + BOM antes/después) y el round-trip por
subtipo, y pasa; `RackCad.UI.Tests` cubre `SelectionMatrix` (incl. celdas ausentes) y la adopción por
las rejillas (STA) y pasa; los builds Debug de UI, Plugin y solución quedan en 0 errores propios; CI
verde sobre la punta publicada. El comportamiento observable —geometría resuelta, planes, BOM, GUID,
identidad, inserción/actualización, persistencia y metadatos I-11, catálogos, nombres de bloque,
mensajes, selección, defaults, interacción visible y comportamiento multivista— es **idéntico** al
vigente.

## 2. Problema

La auditoría 2026-07 (`docs/auditoria-arquitectura-2026-07.md` §3.1 y §4.3, recomendación #12) registra:

- **E6 — Triplicación por vista dentro del selectivo.** Cada familia de seguridad se implementa 3-4
  veces (frontal/lateral/planta+BOM), y el paso de troquel `2.0` está hardcodeado en ≥5 sitios. Hoy:
  **tope** tiene 4 implementaciones independientes (la fórmula de subida-y-snap
  `troquel.Y + round((y + TopeYOffset − troquel.Y)/paso)*paso` está copiada en `SelectiveFrontalBuilder`
  y `SelectiveLateralBuilder`); **parrilla** itera las celdas 3 veces (frontal/lateral/BOM) coincidiendo
  solo porque todas llaman `SelectiveFrontalBuilder.ParrillaRow`; **tarima** reparte filas por su cuenta
  en frontal y lateral; **separador** recalcula el gap/anchor de fondos que alcanzan en lateral y planta.
  (bota, protector lateral y desviador **ya** están bien encauzados por `SelectiveSafetyPlacement` y
  `SelectiveDesviadorPlan` — no se tocan.)
- **E7 — `SelectiveSafetySelection` God-data-class.** Una sola clase acumula secciones
  TOPE/DESVIADOR/PARRILLA/DEFENSA/GUÍA con un `DeepCopy` manual campo-a-campo que crece con cada
  familia (`SelectivePalletDesign.cs` §140-325). El comentario del propio `DeepCopy` (§265-268) ya
  señala que cada campo nuevo debe copiarse ahí a mano.

I-22 es la iniciativa de la Fase 5 que salda esa deuda; su dependencia I-20 (estado del editor
selectivo) quedó integrada, por lo que I-22 está **desbloqueada** (orden fijo, I-20 primero — ROADMAP).

## 3. Alcance

Autorizado por el ROADMAP (Fase 5, I-22) y el objetivo de `ARCHITECTURE.md` §7.4 (servicios de
colocación por familia y vista; configuraciones de seguridad por subtipo con DTO propio):

1. **Servicios de colocación por familia (Application, puros)** para las familias triplicadas,
   parametrizados por vista, siguiendo el patrón plan→proyección de `SelectiveDesviadorPlan`/
   `SelectiveDesviadorDrawing`:
   - **tope**: un plan único (spots + geometría de subida-y-snap) proyectado a frontal/lateral/planta y
     al tally del BOM (`TallyByTramo`);
   - **parrilla**: los builders frontal/lateral y el BOM consumen el **mismo** recorrido de celdas/tramos
     (hoy `SelectiveParrillaPlan` ya existe pero los builders lo esquivan) — se unifica sin cambiar la
     regla `ParrillaRow`;
   - **tarima**: reparto de filas de referencia visual compartido por frontal y lateral;
   - **separador**: gap/anchor de fondos que alcanzan compartido por lateral y planta.
   Los builders quedan como orquestadores; la regla de cada familia vive en un solo sitio.
2. **Descomposición por subtipo (Domain + Persistence)**:
   - `SelectiveSafetySelection` compone `SelectiveTopeConfig`/`SelectiveDesviadorConfig`/
     `SelectiveParrillaConfig`/`SelectiveDefensaConfig`/`SelectiveGuiaConfig`, cada una `sealed` con su
     `DeepCopy` propio; `SelectiveSafetySelection.DeepCopy` delega en cada config. Las propiedades
     planas vigentes (`TopeSaque`, `ParrillaFrente`, …) se conservan como **accesos delegados** a las
     configs para preservar la superficie pública que consumen UI y pruebas (compatibilidad y bajo
     riesgo); las predicadas por familia (`TopeAt`/`DesviadorAt`/`GuiaEntradaAt`/`ParrillaAt`,
     `SideForPost`) se conservan.
   - Persistencia: el **formato de alambre permanece byte-idéntico** (el DTO plano
     `SafetySelectionDocument` es compartido con la ruta dinámica y está congelado por
     `SystemKindPersistenceCharacterizationTests`; `Side` como entero; sin `JsonExtensionData`). El
     mapeo `From`/`ToDomain` se **modulariza por familia** (cada familia con su propio mapeo y su
     **fallback legacy** exacto), de modo que "DTO por subtipo" se realiza como unidades de mapeo por
     familia sobre un documento serializado plano y compatible. Cada familia añade su prueba de
     round-trip + escenario legacy.
3. **Paso de troquel único**: los 5 sitios que hardcodean `2.0` como snap de rejilla
   (`SelectiveGeometryResolver.cs:288,:390`; `SelectiveFrontalBuilder.cs:197`;
   `SelectiveLateralBuilder.cs:283,:344`) referencian `SelectiveRackDefaults.TroquelPaso`. Sin cambio de
   valor (todos valen 2.0) ni de resultado.
4. **`SelectionMatrix` (UI)**: extender `SelectionMatrixModel`/`SelectionMatrix` (I-14) con **celdas
   ausentes** (una celda que no existe en una rejilla dentada no se dibuja ni cuenta), preservando el
   comportamiento rectangular vigente y sus pruebas; adoptar el control en las rejillas frente×nivel
   plano-on/off del editor selectivo (**tope**, **desviador**, **guía-entrada**), conservando idénticos
   los controles auxiliares (opciones fuera de la rejilla, nota de holgura del desviador vía
   `CellChanged`, cabeceras, orden y el conjunto de off-cells producido).

## 4. Fuera de alcance

- **I-25 (guardas traseras)**: la última familia de seguridad se construye SOBRE I-22, después; no se
  agrega aquí.
- **Push Back / I-18** y cualquier bloque DWG o fila de catálogo nuevos.
- **El editor Dinámico**: no se toca `RackDynamicSystemWindow`, `DynamicEditorSafety`, los
  `DynamicSafety*Builder`, `DynamicEntranceGuidePlan`, `DynamicForkliftDefensePlan` ni la colocación
  dinámica de defensa/guía. El DTO `SafetySelectionDocument` es compartido con la ruta dinámica: su
  **formato de alambre no cambia**, y el mapeo modularizado se verifica también contra el round-trip
  dinámico existente (`RackProjectStoreTests`).
- **Rediseño visual / cambio de reglas de producto**: sin cambiar geometría, planes, recetas de BOM,
  nombres de bloque, mensajes, etiquetas, tooltips, orden, defaults ni interacción visible. Las
  rejillas que no son matriz plana on/off no se fuerzan a `SelectionMatrix`: **defensa** (formulario por
  poste con dos longitudes) queda con su diálogo propio; **parrilla** conserva su rejilla con contador
  por celda salvo que la adopción preserve ese badge sin cambio visible (se evalúa, no se fuerza).
- **Migraciones adicionales del editor Selectivo** más allá de la adopción de la rejilla: no se
  re-extrae estado del editor (I-20 cerrada) ni se toca la asimetría vigente de estilos de cota.
- Dependencias NuGet nuevas (política cero-NuGet, ADR-0003) o paquetes de test nuevos.
- **Hallazgos adyacentes**: se **registran** en `docs/ideas-futuras.md`, no se corrigen "de paso".

## 5. Contexto requerido

- `AGENTS.md` (dirección de dependencias Domain←Application←UI←Plugin, regla en un solo sitio, copia
  centralizada de seguridad §3, persistencia versionada §4, archivos calientes, definición de
  terminado) y `docs/WORKFLOW.md` (ciclo de iniciativa, archivos calientes §7, cierre §5).
- `docs/ROADMAP.md` (I-22: dependencias I-14/I-20, orden fijo, conflicto I-20),
  `docs/ARCHITECTURE.md` (§3.2 selectivo y seguridad, §4 capacidades compartidas, §7.3-7.4 objetivo) y
  `docs/auditoria-arquitectura-2026-07.md` (E6/E7, recomendación #12).
- Context Packs: `architecture-kernel`, `system-selective`, `persistence`, `ui-editors`,
  `delivery-validation`.
- ADR índice (`docs/adr/README.md`): ADR-0003 (cero-NuGet), decisiones vigentes de HANDOFF §7.
- Precedentes: I-14 (`SelectionMatrix` + proyecto `RackCad.UI.Tests` + runner STA), I-20 (estado del
  editor selectivo, patrón de caracterización STA), I-16 (tests golden de equivalencia de planes).
- Código:
  - Domain: `src/RackCad.Domain/Systems/SelectivePalletDesign.cs`, `SelectiveRackDefaults.cs`.
  - Application: `src/RackCad.Application/Systems/Selective{FrontalBuilder,LateralBuilder,PlantaBuilder,
    BomBuilder,GeometryResolver,SafetyPlacement,SafetyGrid,SafetyFamilies,ParrillaPlan,DesviadorPlan,
    DesviadorDrawing,MedioFrente}.cs`, `SeparatorLevelCalculator.cs`.
  - Persistence: `src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs`,
    `DynamicRackSystemDocument.cs` (consumidor del mismo DTO), stores.
  - UI: `src/RackCad.UI/Controls/SelectionMatrix{,Model}.cs`, `SelectiveSafetyWindow.cs`,
    `Safety{Tope,Parrilla,Defensa,Desviador,GuiaEntrada}GridWindow.cs`.
  - Pruebas: `tests/RackCad.Tests/Selective{Safety,SafetyGrid,Desviador,PerFondo,BomBuilder,
    FrontalBuilder,LateralBuilder,PlantaPlan,TwentyBaysEquivalence}Tests.cs`,
    `SystemKindPersistenceCharacterizationTests.cs`, `RackProjectStoreTests.cs`;
    `tests/RackCad.UI.Tests/SelectionMatrix{,Model}Tests.cs`.

## 6. Dependencias

- **Integradas requeridas:** I-14 (`architecture/ui-controls`, `SelectionMatrix`) e I-20
  (`refactor/selective-editor-state`), ambas en `main` (`abc1a53`, `9a895e4`). I-15/I-08 (transitivas)
  también.
- **Conflictos que deben permanecer inactivos:** I-20 (mismo código del selectivo) — ya integrada, sin
  rama en curso. No arrancar en paralelo otra rama que toque `RackSelectiveWindow`/`SelectivePalletDesign`
  /los builders del selectivo.
- **Aislamiento verificado:** I-24 (`refactor/ui-tests-editores`) e I-05 (`feature/guardrail-unidades`)
  no tienen rama ni merge; permanecen aisladas.
- **Entradas del dueño:** ninguna decisión requerida para arrancar. AutoCAD y owner-validation son
  gates del dueño al cierre (§10).

## 7. Archivos esperados

Crear (producto, `src/RackCad.Application/Systems/`):

- `SelectiveTopePlan.cs` (+ proyección por vista) — plan único de topes (spots + subida-y-snap) para
  frontal/lateral/planta + tally del BOM.
- `SelectiveTarimaPlacement.cs` — reparto de tarima de referencia visual compartido frontal/lateral.
- `SelectiveSeparadorPlacement.cs` — gap/anchor de fondos que alcanzan, compartido lateral/planta.
- (parrilla: se unifica el consumo del `SelectiveParrillaPlan` existente por los builders; si conviene,
  un pequeño `SelectiveParrillaPlacement`/proyección sin cambiar `ParrillaRow`.)

Crear (producto, `src/RackCad.Domain/Systems/`):

- `SelectiveSafetyConfig.cs` — `SelectiveTopeConfig`/`SelectiveDesviadorConfig`/`SelectiveParrillaConfig`/
  `SelectiveDefensaConfig`/`SelectiveGuiaConfig` (cada una con `DeepCopy` y sus predicados).

Crear (pruebas, `tests/RackCad.Tests/`):

- `SelectiveSafetyEquivalenceTests.cs` (golden: multiset de instancias frontal/lateral/planta + BOM por
  escenario, congelado antes del refactor y estable después).
- `SelectiveTopePlanTests.cs`, `SelectiveTarimaPlacementTests.cs`, `SelectiveSeparadorPlacementTests.cs`
  (planes puros por familia).
- `SelectiveSafetyConfigTests.cs` (DeepCopy por config + round-trip por subtipo + escenario legacy).

Crear/ampliar (pruebas, `tests/RackCad.UI.Tests/`):

- `SelectionMatrixAbsentCellTests.cs` (modelo + control con celdas ausentes).
- `SafetyGridAdoptionTests.cs` (STA: las rejillas tope/desviador/guía adoptan el control y producen los
  mismos off-cells/config que hoy).

Modificar (acotado, sin cambio de comportamiento observable):

- Application: `SelectiveFrontalBuilder.cs`, `SelectiveLateralBuilder.cs`, `SelectivePlantaBuilder.cs`,
  `SelectiveBomBuilder.cs` (orquestan los planes por familia; paso de troquel), `SelectiveGeometryResolver.cs`
  (paso de troquel), `SelectiveParrillaPlan.cs`/`SelectiveSafetyPlacement.cs`/`SelectiveDesviadorPlan.cs`
  (consumen configs) según haga falta.
- Domain: `SelectivePalletDesign.cs` (`SelectiveSafetySelection` compone configs + accesos delegados +
  `DeepCopy` delega).
- Persistence: `SelectivePalletDesignDocument.cs` (mapeo `SafetySelectionDocument` modularizado por
  familia, wire format intacto).
- UI: `Controls/SelectionMatrixModel.cs`, `Controls/SelectionMatrix.cs` (celdas ausentes);
  `SafetyTopeGridWindow.cs`, `SafetyDesviadorGridWindow.cs`, `SafetyGuiaEntradaGridWindow.cs` (adopción).
- Docs: `docs/initiatives/README.md` (índice), este contrato, `docs/automation/state/I-22.yml`.

No se espera modificar: el editor **dinámico** ni sus builders/planes, `RackDynamicSystemWindow`, los
DrawServices del Plugin (la seguridad fluye como `HeaderBlockInstance` Role=Safety por el path genérico),
los catálogos, el **formato serializado** de persistencia, `deploy/` ni `.github/workflows`. El XAML no
existe para estas ventanas (se construyen en código). Una desviación material obliga a detenerse (§12).

## 8. Fases

1. **Reclamo y contrato.** Rama + worktree desde `origin/main`, commit de reclamo + push, contrato desde
   `TEMPLATE.md`, índice y estado versionado. (Evidencia: rama remota aceptada, este archivo.)
2. **Caracterización previa (golden).** Tests que congelan el diseño resuelto (multiset de instancias por
   vista + BOM) de escenarios representativos por familia sobre el código vigente. (Evidencia: suite
   verde sobre el código sin refactor.)
3. **Paso de troquel único.** Enrutar los 5 sitios a `TroquelPaso`. (Evidencia: golden + suite verdes.)
4. **Descomposición por subtipo (Domain + Persistence).** Configs por familia con `DeepCopy` propio,
   accesos delegados, `DeepCopy` que delega; mapeo de persistencia modularizado con fallback legacy;
   pruebas por subtipo. (Evidencia: `RackCad.Tests` verde, round-trip por subtipo + dinámico.)
5. **Servicios de colocación por familia.** `SelectiveTopePlan`/`SelectiveTarimaPlacement`/
   `SelectiveSeparadorPlacement` + unificación de parrilla; builders como orquestadores. (Evidencia:
   golden estable; builds Debug de UI y Plugin en 0 errores propios.)
6. **Adopción de `SelectionMatrix`.** Celdas ausentes en el control + adopción en tope/desviador/guía.
   (Evidencia: `RackCad.UI.Tests` verde, incl. STA de adopción.)
7. **Gates automatizados y cierre de sesión.** Suite completa + suite UI + build de la solución + CI
   sobre el SHA publicado; revisión de diff, commits lógicos, push, estado versionado, gates manuales
   documentados como abiertos. (Evidencia: §14.)

## 9. Pruebas y builds

- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` — suite completa verde (sin regresión) + planes
  por familia + equivalencia golden + round-trip por subtipo.
- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` — suite UI verde + celdas ausentes + STA
  de adopción de rejillas.
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (solo los
  `MSB3277` conocidos de AutoCAD; requiere AutoCAD 2025 para el build completo del Plugin).
- `dotnet build RackCad.sln -c Debug` — build completo de la solución.
- CI: los cuatro jobs (Tests, Build UI, Build Plugin sin AutoCAD, UI Tests) verdes sobre la punta
  publicada de la rama.

## 10. Validacion manual

Gates del dueño **abiertos** al cierre de la sesión de implementación. AutoCAD **requerido**
(`requires_autocad: true`) porque el refactor toca el código que produce las piezas de seguridad
dibujadas (aunque el diseño resuelto es idéntico por construcción y queda fijado por la equivalencia
golden). Checklist manual del dueño, sobre el DLL Debug del **worktree de I-22**
(`…-I-22-safety-placement\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025:

- [ ] **Topes (larguero tope):** rejilla nivel×frente (adopta `SelectionMatrix`), `TopeShared` vs
      per-fondo, `TopeFondo` elegible, `SAQUE`, toggle frontal; se dibujan idénticos en frontal/lateral/
      planta; `TROQUEL_TOPE`; BOM por tramo sin diferencias.
- [ ] **Parrilla (deck):** toggles frontal/lateral, `FRENTE`/`FONDO`, frente/cantidad manual, off-cells,
      contador por celda; frontal/lateral y BOM sin diferencias (incl. clamp "no cabe").
- [ ] **Tarima (referencia visual):** "Mostrar tarimas" frontal + lateral; sin BOM; `TARIMA_GENERICA`.
- [ ] **Separadores:** doble/triple/cuádruple profundidad; separadores en lateral y planta; BOM.
- [ ] **Desviador:** rejilla poste×nivel (adopta `SelectionMatrix`), longitud/altura, nota de holgura
      viva, IN/OUT; frontal/lateral/planta y BOM sin diferencias.
- [ ] **Guía-entrada:** rejilla frente×nivel (adopta `SelectionMatrix`); off-cells preservados.
- [ ] **Bota / protector lateral / defensa:** sin cambios (no refactorizados aquí) — verificación de
      no-regresión.
- [ ] **Round-trip / persistencia I-11:** reabrir con `RACKEDITAR` (mismo GUID); reabrir desde
      biblioteca (`LoadForNew`); guardar; BOM total; documento legacy sin campos de seguridad.
- [ ] **Multivista:** Actualizar en sitio e Insertar lateral/planta ligadas con el mismo GUID.

**owner-validation** (apariencia e interacción idénticas: rejillas adoptadas, etiquetas, tooltips,
orden, controles auxiliares) — **abierta**.

## 11. Criterios de aceptacion

- Existen en `RackCad.Application` los servicios puros de colocación por familia (tope, tarima,
  separador) parametrizados por vista, y los builders frontal/lateral/planta + BOM los consumen como
  orquestadores; la parrilla comparte un único recorrido de celdas; la regla de cada familia vive en un
  solo sitio (sin aritmética duplicada por vista).
- El paso de troquel proviene de `SelectiveRackDefaults.TroquelPaso` en los 5 sitios (0 hardcodeos de
  `2.0` como snap de rejilla en el selectivo).
- `SelectiveSafetySelection` compone configuraciones por subtipo con `DeepCopy` propio; su `DeepCopy`
  delega; la persistencia conserva el **formato de alambre byte-idéntico** con mapeo por familia,
  fallback legacy y round-trip por subtipo (selectivo y dinámico).
- `SelectionMatrix` soporta celdas ausentes sin regresión de I-14; las rejillas tope/desviador/guía lo
  adoptan produciendo los mismos off-cells/config y la misma interacción visible.
- La equivalencia de diseño resuelto (multiset de instancias por vista + BOM) es **idéntica** antes/
  después para fondo único, doble/triple/cuádruple profundidad, medio frente, y cada familia.
- La superficie pública que consume el Plugin y la UI no cambia; la dirección de dependencias se
  conserva (Application no referencia UI/AutoCAD); sin dependencias NuGet nuevas.
- `RackCad.Tests` y `RackCad.UI.Tests` verdes; builds Debug de UI, Plugin y solución en 0 errores
  propios; CI verde en la rama.

## 12. Condiciones para detenerse

- Que preservar el comportamiento exija cambiar geometría, recetas de BOM, nombres de bloque, mensajes,
  defaults o el **formato serializado** de persistencia: detenerse antes de ampliar alcance.
- Que la descomposición obligue a cambiar la superficie pública que consume el Plugin, o a tocar el
  editor **dinámico** o sus builders/planes de seguridad.
- Que la adopción de `SelectionMatrix` no pueda preservar la interacción visible de una rejilla (p. ej.
  el contador por celda de parrilla o el formulario de defensa): esa rejilla **no** se fuerza; se
  documenta y se conserva su diálogo.
- Que aparezca en `origin` otra rama tocando el mismo código del selectivo de forma incompatible: no
  sobrescribir; entregar evidencia.
- Cualquier necesidad de un paquete NuGet nuevo (producto o test), de un bloque DWG o fila de catálogo
  nuevos, o de derivar hacia I-25/I-18.

## 13. Estado versionado y entrega del Pull Request

Estado canónico en `docs/automation/state/I-22.yml`. La automatización está pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de la sesión. No
se abre un segundo Pull Request ni se activa auto-merge. Los gates `autocad` y `owner-validation` quedan
**abiertos** (pendientes del dueño); la integración a `main` (`git merge --no-ff`, WORKFLOW §4.5) se
realiza en la sesión de integración, no en esta rama.

## 14. Evidencia final

Se completa al cierre de la sesión: commits lógicos con trailer de procedencia, archivos creados/
modificados, resultados de `dotnet test` (suite completa y UI), builds de UI/Plugin/solución, evidencia
de CI sobre el SHA publicado, SHA base y punta de la rama, confirmación del push, gates manuales
abiertos (AutoCAD + owner-validation), invariantes comprobados (equivalencia de diseño resuelto por
vista + BOM y round-trip por subtipo, selectivo y dinámico) y confirmación de que `main` no fue
modificada. `docs/HANDOFF.md` §8-12 y el estado en `docs/ROADMAP.md` se actualizan **solo** en la sesión
de integración (último commit de la rama), nunca desde esta rama.
