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
| PB-004 | La pendiente subia 11.2" en 204"; y la primera correccion saco al larguero posterior de su troquel (rechazada en la validacion round 1) | Regla vigente: el 7/16" por pie es un OBJETIVO NOMINAL. El POSTERIOR es el ancla y conserva su troquel; el de ENTRADA/SALIDA se deriva de el y se ajusta al suyo (`PushBackTroquelGrid`); la cama une los dos contactos reales y la pendiente final es la resultante |
| PB-012 | "Alto 1er nivel" abria en 6" | `PushBackDefaults.DefaultFirstLevelHeight` = 4", aplicado SOLO en `LoadNew()` |
| PB-013 | Tarima general modificable pero inerte | Fondo y Unidad globales; Frente/Alto/Peso espejo de la celda, no editables |
| PB-002 | El desviador mostraba menos niveles en un poste, y la celda apagada no llegaba a todas las vistas | `DynamicFrontGeometry.LoadLevelsPerPost` (maximo de frentes adyacentes) **+** `SelectiveDesviadorPlan.CellKey`: la off-cell es POSTE x NIVEL y la leen igual el lateral, los dos frontales, la planta y el BOM |
| PB-003 | Selector "Lado" inerte en el desviador | `showSide: false` para Push Back; el lado canonico es el bajo, que la autoridad ya imponia |
| PB-005 | Sin selector de tipo de tope | `PushBackRearTopeConfig.PieceId` + `ResolvePieceId` consumida por las 3 vistas y el BOM, con fallback |
| PB-006 | "Compartido"/"Lado" en el tope | `showSharedAndSide: false` para Push Back |
| PB-008 | "Salida"/"Entrada" en un sistema LIFO | "Entrada/Salida" y "Posterior"; el mapeo fisico no cambia |
| PB-009 | Se dibujaba defensa en el posterior; y la autoridad borraba de paso la matriz POR POSTE de botas y laterales (rechazado en la validacion round 1) | `LowEndOnly` impuesta por la autoridad, sin longitud automatica atras **+** la matriz por poste se conserva y el extremo se impone donde se decide, con `SelectiveSafetyEnds.EndsForPost` |
| PB-010 | 12"/36" no se recalculaba al agregar frentes, y el dialogo descartaba un registro manual cuyo numero coincidia con el automatico | Estado Auto por extremo en `SafetyPostDefense`, recalculado del conteo de postes vigente **+** `OnOk` guarda siempre que un extremo sea manual: la procedencia se lee del estado Auto, nunca comparando numeros |

## 9. Pruebas y builds

`dotnet test` de las dos suites completas; filtros dirigidos que nunca descubren cero; goldens de las 5
vistas y del BOM; round-trip y legacy de los DTO; validador I-19 sin errores nuevos; build Debug de UI y
Plugin con 0 errores propios; CI verde en la rama.

## 10. Validacion manual

`requires_autocad: true`. Sobre el DLL Debug del worktree, con AutoCAD **cerrado antes de compilar**.

> **Round 1: RECHAZADA (2026-07-25).** El Owner encontro dos defectos —la geometria de anclaje de los
> largueros de extremo y la perdida de la matriz por poste de botas y protectores laterales—, ambos
> corregidos con evidencia fallo->paso. El gate vuelve a estar **cerrado**: un round 2 solo se abre tras
> nueva revision del coordinador. El DLL compilado sobre `2210e67` queda **obsoleto** y no debe
> reutilizarse, porque la geometria cambio.

### Checklist minimo obligatorio — 21 puntos

El minimo exigido son 16; la revision tecnica I-32-CODE-REVIEW-3 pidio restaurar como puntos
**independientes** la altura de cabecera y cada paso del flujo insertar / actualizar / guardar / reabrir /
BOM, que una version anterior habia colapsado en tres lineas. Colapsarlos ocultaba lo que mas importa
comprobar: un flujo que falla en un solo paso se ve verde si ese paso viaja dentro de otro.

Los puntos 1-12 cubren los hallazgos corregidos; 13-17 son el flujo completo, paso a paso; 18-19 las
consecuencias geometricas de PB-004 sobre piezas que el Owner ya habia aprobado en I-18; 20-21 los
invariantes que ninguna correccion puede romper.

| # | Cubre | Que comprobar |
|---|---|---|
| 1 | PB-004 pendiente | La cama sube **7 7/16"** en un rack de 204" (no 11.2"), medido con una cota vertical |
| 2 | PB-004 **cabecera** | La altura de cabecera y la LONGITUD del poste **NO crecen** por corregir la pendiente: la altura sale de la envolvente del nivel superior y la pendiente se cuenta UNA sola vez. Comparar contra el mismo rack antes de I-32 |
| 3 | PB-012 | Un rack nuevo abre con "Alto 1er nivel" = **4** |
| 4 | PB-013 | Tarima (datos generales): Frente/Alto/Peso solo se muestran y siguen a la celda seleccionada; Fondo y Unidad se editan y llegan al diseno |
| 5 | PB-002 rejilla | Seguridad -> Desviador: el ultimo poste ofrece los mismos niveles que su vecino |
| 6 | PB-002 interruptor | Apagar la celda de un poste la quita del corte lateral, del frontal de entrada/salida, de la planta **y del BOM**, y solo en ese poste |
| 7 | PB-003 | El dialogo del desviador **no** tiene selector "Lado", ni en su etiqueta ni en el texto |
| 8 | PB-005 | Seguridad -> Topes posteriores: hay selector de **tipo de tope** con las variantes del catalogo; al elegir otra, cambia la pieza en las tres vistas y en el BOM |
| 9 | PB-006 | El dialogo del tope **no** ofrece "Compartido (uno central)" ni "Lado"; el SAQUE sigue ahi |
| 10 | PB-008 | La defensa nombra sus extremos **Entrada/Salida** y **Posterior**; ninguna columna dice "Salida" o "Entrada" a secas |
| 11 | PB-009 | Un rack nuevo **no** dibuja defensa ni proteccion en el extremo posterior, en ninguna vista ni en el BOM |
| 12 | PB-010 | Con el extremo en **Auto**: agregar frentes lleva un poste de orilla a 36" y **quitarlos lo devuelve a 12"**. Con una longitud **tecleada**, el numero se conserva en ambos sentidos — incluso si coincide con el automatico del momento |
| 13 | Flujo — **insertar** | `RACKPUSHBACK` inserta el rack y sus **cuatro vistas** (lateral, frontal entrada/salida, frontal posterior, planta) con la geometria esperada |
| 14 | Flujo — **actualizar** | `RACKEDITAR` sobre el rack insertado redibuja **en sitio**, conserva el **mismo GUID** y arrastra las vistas ligadas |
| 15 | Flujo — **guardar** | El rack se guarda en la **biblioteca** de disenos sin perder tipo de tope, longitudes de defensa ni configuracion de celdas |
| 16 | Flujo — **reabrir** | Reabrirlo desde la biblioteca y desde el DWG devuelve **exactamente** lo guardado (round-trip), con la metadata I-11 intacta |
| 17 | Flujo — **BOM** | `RACKBOM` no duplica conteo, coincide con lo que muestran las vistas y refleja los cambios de los puntos 6, 8 y 12 |
| 18 | PB-004 (consecuencia) | **El larguero posterior aparece a la MISMA altura** en el corte lateral y en el frontal posterior (antes diferian 1.18") |
| 19 | PB-004 (consecuencia) | **El tope posterior acompana al larguero**: conserva su regla aprobada (sube sobre el, ajusta al troquel del poste, +4") y por eso baja con el |
| 20 | Compatibilidad | Un Push Back guardado **ANTES de I-32** conserva su altura de primer nivel, su tipo de tope y sus longitudes de defensa al abrirlo |
| 21 | Aislamiento | **Selectivo y Dinamico no cambian**, ni en dibujo ni en sus dialogos de seguridad |

### Smoke complementario — NO sustituye ningun punto obligatorio

Los cuatro hallazgos que esta iniciativa **no** implementa. Se miran para confirmar que **no empeoraron**;
que sigan como estaban no es un fallo de I-32 y su ausencia no bloquea el gate.

| Hallazgo | Que observar |
|---|---|
| PB-001 | Los previews de las cuatro vistas siguen igual de limitados que en I-18; su estandarizacion es una iniciativa transversal aparte |
| PB-007 | La modificacion masiva de seguridad sigue siendo celda a celda |
| PB-011 | Push Back sigue sin editor avanzado de modulos |
| PB-014 | Sigue sin existir el frente "en blanco" |

## 11. Criterios de aceptacion

- Los diez hallazgos corregidos, cada uno con regresion **verificada fallando** sin el fix.
- Suites completas verdes; ningun filtro dirigido en cero; goldens re-fijados SOLO donde el cambio de
  geometria lo exige, con el motivo escrito en el propio archivo.
- Selectivo y Dinamico sin cambio de comportamiento, con pruebas que lo fijan.
- Compatibilidad legacy, GUID, metadata I-11, cuatro vistas, BOM, registros y shell conservados.
- Validacion manual del Owner en AutoCAD (§10, **21 puntos obligatorios**) antes de integrar.

### Estado tras el round 3 — la regla de la cama vuelve al mate por `TROQUEL_IN`

> **Correcciones acumuladas.** Esta seccion declaro sucesivamente que «no queda pendiente funcional»
> (round 2 RECHAZADO), que el defecto de la cama estaba **bloqueado** por un contrato de catalogo faltante
> (el Owner corrigio: no faltaba ninguno), y que la cama debia colocarse por su **origen** con
> `LONGITUD = axis.Length` (round 3 RECHAZADO: esa regla era equivocada). Todas quedan corregidas aqui en
> vez de retiradas.

**Contrato fisico vigente de la cama:**

1. mate obligatorio de Entrada/Salida: `LARGUERO_IN_OUT.TROQUEL_CAMA` con
   `RIEL_DE_CINTA_CALIBRE_12.TROQUEL_IN`;
2. la cama se coloca transformando su `TROQUEL_IN` local hasta `ExitMate`;
3. **`LONGITUD` = el fondo estructural completo** (`ResolveBedLength`). **Hay una sola longitud de cama**:
   dibuja el riel, alimenta el BOM y mide la subida nominal.

Es **esperado** que haya riel antes de `TROQUEL_IN` y que sobresalga del larguero posterior. No es
penetracion y no se recorta.

**Conservado sin cambios:** `PushBackElevations`, los contactos, las elevaciones, la pendiente, los
troqueles, los intermedios, el tope posterior — y la correccion del **protector lateral** (primer poste
delante sin espejo, ultimo delante espejado).

El detalle esta en [`decisions/I-32.md`](../automation/decisions/I-32.md).

**Owner-validation:** rounds 1, 2 y 3 **RECHAZADOS**. **No hay round 4 abierto.** Los tres DLL validados
—`2210e67`, `557858d` y `2641830`— quedan **obsoletos** y no deben reutilizarse.

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
