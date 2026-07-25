---
schema: rackcad-initiative/v1
id: I-32
title: Correcciones funcionales y geometricas de Push Back
type: fix
status: implementing
branch: fix/correcciones-push-back
base_branch: main
priority:
size: M
depends_on: [I-18]
conflicts_with: []
context_packs: [system-dynamic-flowbed, ui-editors, architecture-kernel, persistence, catalogs-data, delivery-validation]
automation_state_path: docs/automation/state/I-32.yml
decision_paths: [docs/automation/decisions/I-32.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Correcciones funcionales y geometricas de Push Back

> Nace del reporte del Owner sobre el Push Back ya integrado por I-18. El campo `priority` se deja vacio
> por falta de fuente numerica en el ROADMAP, igual que en I-18. Los hallazgos se identifican `PB-NNN`
> por su fila en el reporte del Owner.

## 1. Objetivo

Corregir los diez hallazgos que el Owner levanto sobre Push Back tras usarlo en AutoCAD, sin tocar el
comportamiento del Selectivo ni del Dinamico, y conservando compatibilidad legacy, GUID, metadata I-11,
las cuatro vistas, el BOM, los registros y el shell visual.

## 2. Problema

I-18 dejo Push Back operativo y aprobado, pero el uso real destapo defectos de tres clases:

- **Geometria**: la pendiente de la cama subia 11.2" en un rack de 204" cuando la regla es 7/16" por pie
  (7.4375"). No era la constante: la pendiente no tenia dueno y emergia de dos snaps independientes mas un
  salto entre dos datums de catalogo distintos.
- **Dialogos que ofrecen lo que no aplica**: selector de lado en el desviador, "Compartido"/"Lado" en el
  tope, y una defensa con extremos llamados "Salida"/"Entrada" en un sistema LIFO que carga por un solo
  extremo. Controles inertes que el usuario lee como si decidieran algo.
- **Reglas que no se recalculan o mienten**: la matriz del desviador mostraba menos niveles de los que el
  dibujo coloca, la defensa congelaba los 12"/36" al agregar frentes, y el panel de tarima general ofrecia
  campos que la celda ya gobernaba.

## 3. Alcance

Los diez hallazgos autorizados: **PB-002, PB-003, PB-004, PB-005, PB-006, PB-008, PB-009, PB-010, PB-012
y PB-013** (§8). Cada uno con test de regresion verificado fallando sin el fix.

## 4. Fuera de alcance

- **PB-001** (preview de las tres vistas), **PB-007** (reconfigurador masivo de seguridad), **PB-011**
  (editor avanzado de modulos como el Dinamico) y **PB-014** (frente en blanco): registrados como
  candidatos futuros en [`ideas-futuras.md`](../ideas-futuras.md), no implementados.
- Cambiar el comportamiento del **Selectivo** o del **Dinamico**. Todo arreglo sobre un dialogo compartido
  entra como parametro opcional cuyo default ES el comportamiento vigente.
- Alterar catalogos, bloques DWG, el formato fisico del Xrecord o los registros de sistema.

## 5. Contexto requerido

- Global: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md` §7,
  `docs/guias/agregar-un-sistema.md`.
- Context Packs: `system-dynamic-flowbed`, `ui-editors`, `architecture-kernel`, `persistence`,
  `catalogs-data`, `delivery-validation`.
- Decisiones de I-18: [`decisions/I-18.md`](../automation/decisions/I-18.md) — en particular PB-0.2 §2 y
  §4 (pendiente y snap), que el addendum de I-32 precisa.
- Decisiones de esta iniciativa: [`decisions/I-32.md`](../automation/decisions/I-32.md).

## 6. Dependencias

- **I-18 integrada en `main`** (merge `77031be`, cierre `91eb53c`). ✓
- Ninguna iniciativa activa sobre Push Back, seguridad o los archivos calientes al abrir (I-23 e I-25
  pendientes y sin rama remota).

## 7. Archivos esperados

- **Domain**: `PushBackDefaults`, `PushBackRearTope`, `SelectivePalletDesign` (`SafetyPostDefense`,
  `SelectiveSafetySelection`), `SelectiveSafetyConfig`.
- **Application**: `PushBackBedSlope` (nuevo), `PushBackFlowBedGeometry`, `PushBackLoadBeamGeometry`,
  `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder`, `PushBackRearTopeBuilder`,
  `PushBackBomBuilder`, `PushBackSafetyAuthority`, `PushBackEditorState(.Load)`,
  `PushBackEditorDesignAssembler`, `DynamicFrontGeometry`, `DynamicForkliftDefensePlan`,
  `DynamicSafetyDefaults`, `DynamicSafetyLateralBuilder`, `DynamicSafetyMultiViewBuilder`.
- **Persistencia**: `PushBackDesignDocument`, `SafetySelectionDocuments`, `SelectivePalletDesignDocument`.
- **UI**: `RackPushBackSystemWindow.xaml(.cs)`, `PushBackRearTopeSection`, y los dialogos COMPARTIDOS
  `SelectiveSafetyWindow`, `SafetyDesviadorGridWindow`, `SafetyTopeGridWindow`, `SafetyDefensaGridWindow`
  (solo por parametros opcionales con default = comportamiento actual).
- **NO se tocan**: catalogos, `deploy/`, `.sln`, `.csproj`, bloques, registros de sistema, el shell visual
  ni ninguna ventana del Selectivo o del Dinamico.

## 8. Fases

Una corrida, un commit atomico por hallazgo o por par de hallazgos que comparten superficie.

| ID | Hallazgo | Correccion |
|---|---|---|
| PB-004 | La pendiente subia 11.2" en 204" | `PushBackBedSlope`: 7/16" por pie en UNA funcion pura; el extremo alto se DERIVA del bajo ya ajustado al troquel; el frontal posterior toma la misma elevacion que el lateral (D14) |
| PB-012 | "Alto 1er nivel" abria en 6" | `PushBackDefaults.DefaultFirstLevelHeight` = 4", aplicado SOLO en `LoadNew()` |
| PB-013 | Tarima general modificable pero inerte | Fondo y Unidad globales; Frente/Alto/Peso espejo de la celda, no editables |
| PB-002 | El desviador mostraba menos niveles en un poste | `DynamicFrontGeometry.LoadLevelsPerPost`: maximo de frentes adyacentes, la misma regla del dibujo |
| PB-003 | Selector "Lado" inerte en el desviador | `showSide: false` para Push Back; el lado canonico es el bajo, que la autoridad ya imponia |
| PB-005 | Sin selector de tipo de tope | `PushBackRearTopeConfig.PieceId` + `ResolvePieceId` consumida por las 3 vistas y el BOM, con fallback |
| PB-006 | "Compartido"/"Lado" en el tope | `showSharedAndSide: false` para Push Back |
| PB-008 | "Salida"/"Entrada" en un sistema LIFO | "Entrada/Salida" y "Posterior"; el mapeo fisico no cambia |
| PB-009 | Se dibujaba defensa en el posterior | `SelectiveSafetySelection.LowEndOnly`, impuesta por la autoridad; sin longitud automatica atras |
| PB-010 | 12"/36" no se recalculaba al agregar frentes | Estado Auto por extremo en `SafetyPostDefense`, recalculado del conteo de postes vigente |

## 9. Pruebas y builds

`dotnet test` de las dos suites completas; filtros dirigidos que nunca descubren cero; goldens de las 5
vistas y del BOM; round-trip y legacy de los DTO; validador I-19 sin errores nuevos; build Debug de UI y
Plugin con 0 errores propios; CI verde en la rama.

## 10. Validacion manual

`requires_autocad: true`. Sobre el DLL Debug del worktree, con AutoCAD cerrado antes de compilar:

1. Rack nuevo: "Alto 1er nivel" abre en **4**.
2. Cama: la subida sobre un rack de 204" mide **7 7/16"** (no 11.2"), medida en el dibujo.
3. El larguero posterior aparece a la MISMA altura en el corte lateral y en el frontal posterior.
4. El tope posterior sigue sobre la columna de troqueles del poste, ahora acompanando al larguero.
5. Seguridad → Desviador: el ultimo poste ofrece los mismos niveles que su vecino y **no** hay "Lado".
6. Seguridad → Topes posteriores: hay selector de **tipo de tope**; **no** hay "Compartido" ni "Lado".
7. Seguridad → Defensa: las columnas se llaman **Entrada/Salida** y **Posterior**; la posterior viene
   apagada; hay casilla **Auto** por extremo.
8. Agregar frentes: un poste que era orilla pasa a 36" si su extremo esta en Auto; si se tecleo una
   longitud, la conserva.
9. Tarima (datos generales): Frente/Alto/Peso solo se muestran y siguen a la celda; Fondo y Unidad se
   editan y llegan al diseno.
10. Round-trip: abrir un Push Back guardado ANTES de I-32 conserva su altura de primer nivel, su tipo de
    tope y las longitudes de defensa que tenia.
11. `RACKEDITAR` en sitio con el mismo GUID; BOM sin doble conteo; las cuatro vistas ligadas.
12. Selectivo y Dinamico: sin cambios visibles ni de dibujo.

## 11. Criterios de aceptacion

- Los diez hallazgos corregidos, cada uno con regresion **verificada fallando** sin el fix.
- Suites completas verdes; ningun filtro dirigido en cero; goldens re-fijados SOLO donde el cambio de
  geometria lo exige, con el motivo escrito en el propio archivo.
- Selectivo y Dinamico sin cambio de comportamiento, con pruebas que lo fijan.
- Compatibilidad legacy, GUID, metadata I-11, cuatro vistas, BOM, registros y shell conservados.
- Validacion manual del Owner en AutoCAD (§10) antes de integrar.

## 12. Condiciones para detenerse

- Que un arreglo exija **alterar la logica** del Selectivo o del Dinamico y no baste con parametrizar.
- Que el Owner contradiga una de las decisiones fijadas en [`decisions/I-32.md`](../automation/decisions/I-32.md).
- Cualquier condicion general de `AUTOMATION_PLAN` §12.

## 13. Estado versionado y entrega del Pull Request

Estado canonico: [`docs/automation/state/I-32.yml`](../automation/state/I-32.yml). Merge automatico
prohibido; la integracion es una operacion serializada del Owner. `HANDOFF.md`/`ROADMAP.md` se actualizan
como **ultimo commit de la rama** (WORKFLOW §4.5.4) y **no** se tocan en esta corrida.

## 14. Evidencia final

Commits atomicos por hallazgo, con el motivo de cada pin de golden movido escrito en el propio archivo de
pruebas; conteos de las dos suites; builds; confirmacion de que `main` no fue modificada y de que los
catalogos, los registros y el shell quedaron intactos.
