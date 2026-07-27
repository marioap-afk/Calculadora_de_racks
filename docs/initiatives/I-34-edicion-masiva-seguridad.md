---
schema: rackcad-initiative/v1
id: I-34
title: Edicion masiva de matrices de seguridad
type: feature
status: implementing
branch: feature/edicion-masiva-seguridad
base_branch: main
priority:
size: M
depends_on: [I-14, I-22, I-32, I-33]
conflicts_with: [I-23, I-25]
context_packs: [ui-editors, architecture-kernel, system-selective, system-dynamic-flowbed, documentation-governance, delivery-validation]
automation_state_path: docs/automation/state/I-34.yml
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

# Edicion masiva de matrices de seguridad

> Implementa **PB-007**, registrado por I-32 en [`ideas-futuras.md`](../ideas-futuras.md) y dejado
> explicitamente fuera de alcance por I-33 (§4) porque «toca los dialogos COMPARTIDOS, asi que afecta a
> Selectivo y Dinamico: necesita decision de alcance del Owner antes de arrancar». Esa decision la da el
> Owner en la instruccion de apertura de esta iniciativa. El campo `priority` se deja vacio por falta de
> fuente numerica en el ROADMAP, igual que en I-18, I-32 e I-33.
>
> **Estado**: el primer incremento (inventario, decisiones, regresiones y fundacion) y la **adopcion
> productiva por los tres dialogos incluidos** estan **hechos**. Lo unico que queda fuera son parrilla y
> defensa, que necesitan decision previa (§15).

## 1. Objetivo

Que aplicar un cambio de seguridad a muchas celdas cueste **una** operacion en vez de N clics, con la
misma gramatica de «Aplicar a:» que los editores ya tienen (`SelectiveApplyScope` en el Selectivo,
`DynamicRackCellScope` en Dinamico y Push Back), disponible para **cada matriz booleana de seguridad**.

Entregado:

1. El **inventario completo y auditado** de matrices booleanas de seguridad (§6).
2. Las **decisiones de UX y arquitectura cerradas** (§7).
3. **Regresiones rojas** que fijan el contrato antes de que exista el codigo (§9).
4. Una **fundacion comun minima y pura** sobre `SelectionMatrixModel` que las satisface (§8).
5. La **superficie WPF compartida** (`SelectionMatrixBulkBar`) y la **adopcion productiva** por los tres
   dialogos que el contrato incluye (§8 fase 4).

Resultado verificable cuando: las regresiones nuevas fallan sin el codigo y pasan con el; las suites
`RackCad.Tests` y `RackCad.UI.Tests` quedan verdes sin regresion; los builds Debug de UI y Plugin en 0
errores propios; CI verde sobre la punta publicada. Con la adopcion, la interaccion visible de los tres
dialogos **si** cambia (ganan la fila «Aplicar a:»), de modo que los gates `autocad` y `owner-validation`
quedan **abiertos** (§10).

## 2. Problema

`ideas-futuras.md`, PB-007 (prioridad alta del Owner, **general**, no solo Push Back):

> Hoy, para quitar el desviador del segundo nivel en 100 frentes hay que hacer 100 clics: las rejillas de
> seguridad son celda a celda y no tienen alcances.

Verificado en el codigo vigente: las cinco rejillas de seguridad ofrecen **solo** los botones «Todos» y
«Ninguno» (`SelectionMatrixModel.SetAll`, o su equivalente manual en parrilla). No existe ningun alcance
intermedio —ni por nivel, ni por frente, ni por poste— en ninguna de ellas. El patron que el usuario ya
conoce («Aplicar a: Celda / Nivel / Frente / Todas») vive en los editores de **diseno**, no en los
dialogos de **seguridad**, y no fue portado.

La deuda es de infraestructura, no de una familia concreta: cada rejilla la sufre igual, y I-22 ya dejo
la pieza compartida (`SelectionMatrixModel` con `SetAll` y celdas ausentes) sobre la que apoyarse.

## 3. Alcance

1. **Fundacion comun pura** sobre `SelectionMatrixModel` (capa UI, sin WPF en la logica):
   - aplicacion por alcance **Celda / Nivel / Frente-o-Poste / Todo**, con estado **Activar/Desactivar**;
   - **celda primaria** transitoria, propiedad de la fundacion y **jamas persistida**;
   - **etiquetas y capacidades declaradas por el dialogo** (el eje de columna se llama «Frente» o
     «Poste» segun quien lo abra), sin ningun `RackSystemKind` ni `switch` por sistema;
   - **celdas ausentes ignoradas** (rejilla dentada de I-22/I-33): nunca cambian ni se reportan;
   - **una sola notificacion agregada** por operacion masiva, que enumera **exactamente** las celdas que
     cambiaron;
   - **sin rebuild por celda**: el control actualiza las casillas afectadas, no reconstruye la rejilla
     (AGENTS §6).
2. **Regresiones rojas** que fijan ese contrato: celda, fila, columna, todo, rectangular, dentada,
   ausentes, no-op, idempotencia y notificacion agregada; mas las STA minimas de celda primaria,
   habilitacion, tooltip y etiquetas.
3. **Inventario auditado** de los consumidores reales de `SelectionMatrix`/`SelectionMatrixModel` y de
   todas las matrices booleanas de seguridad (§6), con las exclusiones justificadas (§6.3).
4. **Superficie WPF compartida** (`SelectionMatrixBulkBar`): la fila «Aplicar a:» con el par de radios
   Activar/Desactivar, un boton por alcance declarado, habilitacion y tooltip por alcance, y la celda
   primaria visible. Es la **unica** UI de la edicion masiva; `SelectionMatrix.CellInteracted` es lo que
   convierte la ultima celda pulsada por el USUARIO en la primaria.
5. **Adopcion productiva** por `SafetyDesviadorGridWindow` (eje **Poste**), `SafetyTopeGridWindow` (eje
   **Frente**, que cubre a la vez el tope del Selectivo y el tope posterior de Push Back) y
   `SafetyGuiaEntradaGridWindow` (eje **Frente**).
6. **Guarda fail-closed del enum** (§7.10).

## 4. Fuera de alcance

- **Parrilla y defensa** (§6.3 y §15): necesitan decision previa y no se adoptan.
- **DTO, formato de alambre, stores y persistencia**: el conjunto de `OffCells` que un dialogo persiste
  se calcula igual que hoy; la fundacion opera **antes** de esa traduccion, sobre el modelo de la rejilla.
- **Geometria, BOM, catalogos, bloques DWG y namespaces** (I-23).
- **`DesviadorCellsAreByPost`**: sigue en `false` para el Dinamico; cambiarlo es decision del Owner
  registrada en `ideas-futuras.md` y **no** se toca aqui.
- **Guardas traseras (I-25)** y **shell visual** (I-30/I-31).
- **Parrilla y defensa**: excluidas con justificacion en §6.3. La **matriz estructural de tarimas** de los
  editores tampoco entra: no es una matriz de seguridad.
- Dependencias NuGet nuevas (politica cero-NuGet, ADR-0003).
- Hallazgos adyacentes: se **registran** en `docs/ideas-futuras.md`, no se corrigen «de paso».

## 5. Contexto requerido

- Global: `AGENTS.md` (direccion de dependencias, regla en un solo sitio, copia centralizada de seguridad
  §3, performance §6), `docs/WORKFLOW.md` (ciclo, archivos calientes §7), `docs/ROADMAP.md`,
  `docs/ARCHITECTURE.md`, `docs/ideas-futuras.md` (PB-007).
- Context Packs: `ui-editors`, `architecture-kernel`, `system-selective`, `system-dynamic-flowbed`,
  `documentation-governance`, `delivery-validation`.
- Iniciativas previas: **I-14** (nacimiento de `SelectionMatrix`/`SelectionMatrixModel` y del proyecto
  `RackCad.UI.Tests` con `StaTestRunner`), **I-22** (adopcion del control por tres rejillas + celdas
  ausentes), **I-18** (Push Back y su tope posterior), **I-32** (PB-003/PB-006: parametros opt-in de los
  dialogos compartidos), **I-33** (celdas ausentes por frente en blanco y `SafetyDormantCells`).
- Codigo: `src/RackCad.UI/Controls/SelectionMatrix{,Model}.cs`, `src/RackCad.UI/Safety*GridWindow.cs`,
  `src/RackCad.UI/SelectiveSafetyWindow.cs`, `src/RackCad.UI/PushBackRearTope{Section,DialogAdapter}.cs`,
  `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs`, `src/RackCad.Domain/Systems/PushBackRearTope.cs`,
  `src/RackCad.Application/Systems/SelectiveApplyScope.cs`, `DynamicRackCellScope`.

## 6. Inventario auditado (entregable de esta fase)

### 6.1 Consumidores reales de `SelectionMatrix` / `SelectionMatrixModel`

Barrido completo sobre `src/` y `tests/` en la base `7e48b5c`.

**Produccion (`src/`) — 5 archivos, y solo 3 son adoptantes:**

| Archivo | Papel |
|---|---|
| `Controls/SelectionMatrixModel.cs` | La pieza (estado puro) |
| `Controls/SelectionMatrix.cs` | La pieza (control WPF) |
| `SafetyTopeGridWindow.cs` | **Adoptante** |
| `SafetyDesviadorGridWindow.cs` | **Adoptante** |
| `SafetyGuiaEntradaGridWindow.cs` | **Adoptante** |

No hay ningun otro consumidor de produccion: ni en `RackCad.Application`, ni en `RackCad.Plugin`, ni en
las cuatro ventanas ricas de editor. `SafetyParrillaGridWindow` y `SafetyDefensaGridWindow` **no** lo
usan (§6.3).

**Pruebas (`tests/`) — 10 archivos:** `SelectionMatrixTests`, `SelectionMatrixModelTests`,
`SelectionMatrixAbsentCellTests`, `SafetyGridAdoptionTests`, `BlankFrontSafetyGridTests`,
`PushBackDesviadorGridTests`(via las ventanas), `PushBackTopeOnlyInSafetyTests`,
`PushBackEditorLayoutTests`, `PushBackSidebarCompositionTests`, `DynamicShellMigrationTests`; mas
`SelectiveSafetyEquivalenceTests` en `RackCad.Tests` (que nombra el control en prosa, no lo instancia).

### 6.2 Matrices booleanas de seguridad — inventario completo

Las **cinco** matrices booleanas del producto. «Autoridad» = quien decide la FORMA de la rejilla (cuantas
columnas y cuantos niveles tiene cada una).

| # | Sistema(s) | Familia | Filas (eje Y) | Columnas (eje X) | Dentada (jagged) | Autoridad de la forma | Traduccion | Dialogo | Dibujo | BOM |
|---|---|---|---|---|---|---|---|---|---|---|
| M1 | Selectivo | **Tope** (larguero tope) | Nivel de larguero (`Larg. n`, invertido) | **Frente** (`F n`) | Si — `WithJaggedColumns` sobre `levelsPerFrente`; cero ⇒ columna ausente | `SelectiveSafetyGrid.LevelCounts(resolved)`, o el conteo de bahias del diseno como respaldo | `TopeResult.OffCells` → `SelectiveTopeConfig.OffCells` (`SelectiveGridCell{Frente,Level}`), fusionado con las dormidas por `SafetyDormantCells.Merge` | `SafetyTopeGridWindow` (via `SelectiveSafetyWindow`) | `SelectiveTopePlan` (frontal/lateral/planta) leyendo `SelectiveSafetyGrid.OffCellKeys(selection.TopeOffCells)` | `SelectiveTopePlan.TallyByTramo` |
| M2 | **Push Back** | **Tope posterior** (extremo alto) | Nivel (`Larg. n`, invertido) | **Frente** (`F n`) | Si — misma ruta; frentes en blanco ⇒ columna ausente (`allowBlankFronts: true`) | `PushBackRearTopeDialogAdapter.LevelsPerFrente(state.Structure.EffectiveLevelCounts())` | `TopeResult.OffCells` → `PushBackRearTopeConfig.OffCells` via `PushBackRearTopeDialogAdapter.Apply` (**no** es una `SelectiveSafetySelection`) | El **mismo** `SafetyTopeGridWindow`, abierto por `PushBackRearTopeSection` (`showSharedAndSide: false`, PB-006) | `PushBackRearTopeBuilder`, `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder` (via `rearTope.At`) | `PushBackBomBuilder` (via `rearTope.At`) |
| M3 | Selectivo, **Dinamico**, **Push Back** | **Desviador** | Nivel de carga (`Nivel 1 (piso)`, `Nivel n`, invertido) | **POSTE** (`P n`) — el unico eje por poste | Si — `WithJaggedColumns` sobre `levelsPerPost` | `SelectiveDesviadorPlan.Build(...).LevelCounts`; si viene vacio, `FallbackCounts` con `desviadorLevelsPerPost` (Dinamico: `DynamicFrontActivation.EffectiveLevelsPerPost`; Push Back: `DesviadorLevelsPerPost()`) | `DesviadorResult.OffCells` → `SelectiveDesviadorConfig.OffCells`. **El `Frente` de la celda guarda el indice de POSTE**; `SelectiveDesviadorPlan.CellKey` decide si el indice se lee por poste (`DesviadorCellsAreByPost`, Push Back) o colapsado sobre el frente (Selectivo/Dinamico) | `SafetyDesviadorGridWindow` (via `SelectiveSafetyWindow`) | `SelectiveDesviadorPlan`/`SelectiveDesviadorDrawing`; `DynamicSafetyLateralBuilder`, `DynamicSafetyMultiViewBuilder` | Las tres rutas anteriores; la nota de holgura viva se recalcula por evento del modelo |
| M4 | **Dinamico** | **Guia de entrada** | Nivel (`Nivel n`, invertido) | **Frente** (`F n`) | Si — `WithJaggedColumns`; con `allowBlankColumns: true` un cero se respeta | `SafetyLevelsPerFrente()` del editor dinamico | `Result` (`IReadOnlyList<SelectiveGridCell>`) → `SelectiveGuiaConfig.OffCells`, via `SafetyDormantCells.Merge` | `SafetyGuiaEntradaGridWindow` (via `SelectiveSafetyWindow`, solo con `includeGuia: true`) | `DynamicEntranceGuidePlan` (via `selection.GuiaEntradaAt`) | La misma ruta |
| M5 | Selectivo | **Parrilla** (deck) | Nivel (`Larg. n`, invertido) | **Frente** (`F n`) | Si, **a mano**: `CheckBox[frente][nivel]` dimensionado por frente | `SelectiveParrillaPlan.Cells(resolved, catalog)` | `ParrillaResult.OffCells` → `SelectiveParrillaConfig.OffCells` | `SafetyParrillaGridWindow` — **NO usa `SelectionMatrix`** | `SelectiveFrontalBuilder`, `SelectiveLateralBuilder` (via `OffCellKeys(ParrillaOffCells)`) | `SelectiveBomBuilder` | **EXCLUIDA, §6.3** |

**Que sistema abre que rejilla** (por el filtro de elementos de cada editor):

| Sistema | Tope | Tope posterior | Desviador | Guia | Parrilla | Defensa |
|---|---|---|---|---|---|---|
| Selectivo (`RackSelectiveWindow`) | **Si** | — | **Si** | no (`includeGuia: false`) | **Si** | no |
| Dinamico (`RackDynamicSystemWindow`) | no (filtrado) | — | **Si** | **Si** | no (filtrado) | si (no booleana) |
| Push Back (`RackPushBackSystemWindow`) | no (filtrado, decision del Owner 2026-07-24) | **Si** (seccion propia) | **Si** | no | no (filtrado) | si (no booleana) |

### 6.3 Exclusiones justificadas

- **Parrilla (M5) — excluida.** No es una matriz booleana *plana*: cada celda lleva ademas un **contador
  vivo** de parrillas (`SelectiveParrillaPlan.CountIn`) pintado junto a la casilla, un total al pie y un
  rechazo en «Aceptar» cuando la cantidad forzada no cabe. Por eso I-22 ya la dejo **fuera** de la
  adopcion de `SelectionMatrix` («no se fuerza; se documenta y se conserva su dialogo», I-22 §4 y §12).
  Migrarla exige antes decidir como el control comparte una decoracion por celda, que es alcance nuevo.
- **Defensa — excluida.** No es booleana en absoluto: es un **formulario por poste** con dos longitudes
  independientes (salida/entrada), sus dos casillas de «Auto» y su propio DTO `SafetyPostDefense`. No
  tiene eje de nivel, asi que los alcances «Nivel» y «Celda» no significan nada en ella.
- **Matriz estructural de tarimas — excluida.** Es la matriz de **diseno** de los editores (frente ×
  nivel de carga con medidas, no on/off de seguridad); ya tiene su propio «Aplicar a:»
  (`SelectiveApplyScope`, `DynamicRackCellScope`) y no pasa por `SelectionMatrixModel`.

En las tres, la exclusion es **de este primer incremento**, no una prohibicion permanente: se registran
como candidatos en §15.

## 7. Decisiones cerradas (UX y arquitectura)

1. **Celda primaria NO persistida.** El alcance necesita un ancla (que fila y que columna). Esa **celda
   primaria** vive en la fundacion, es **transitoria** y no llega jamas a `OffCells`, al DTO ni al wire:
   lo que se persiste sigue siendo, exactamente como hoy, el conjunto de celdas apagadas. Solo puede ser
   una celda **presente**: fijarla sobre una celda ausente o fuera de rango se **rechaza** y la primaria
   anterior no se altera.
2. **Estado Activar/Desactivar, no «invertir».** La operacion masiva lleva un valor explicito: activar
   pone en ON todas las celdas del alcance, desactivar las pone en OFF. **No** hay alternancia masiva —
   invertir 40 celdas heterogeneas no es una intencion expresable, y rompe la idempotencia.
3. **Cuatro alcances**: **Celda** (solo la primaria), **Nivel** (la fila de la primaria, todas las
   columnas), **Frente o Poste** (la columna de la primaria, todos los niveles) y **Todo** (la rejilla
   entera). Son la misma gramatica que `SelectiveApplyScope` y `DynamicRackCellScope`, para que el usuario
   no aprenda un segundo idioma. **Todo** es el unico que **no** requiere primaria.
4. **Etiquetas y capacidades declaradas por el dialogo.** El eje de columna se llama «Frente» en tope,
   guia y parrilla, y **«Poste»** en el desviador; el dialogo lo declara al construir la fundacion, junto
   con que alcances ofrece. La fundacion **no interpreta** el sistema.
5. **Infraestructura sin `RackSystemKind`.** No hay enum de sistema, ni `switch`, ni referencia a
   `SystemRegistry` en la fundacion: es una pieza de UI generica sobre una matriz de booleanos, igual que
   `SelectionMatrixModel`. Que Selectivo, Dinamico y Push Back abran la misma rejilla es precisamente lo
   que exige que la pieza sea agnostica (I-33 §6.5 ya sufrio el defecto contrario: derivar una capacidad
   en vez de declararla).
6. **Celdas ausentes ignoradas.** Una celda ausente (rejilla dentada de I-22, columna de frente en blanco
   de I-33) **nunca** cambia y **nunca** aparece en la notificacion. Aplicar «Todo» sobre una rejilla con
   columnas ausentes no las resucita, y la configuracion **dormida** que `SafetyDormantCells` preserva
   queda intacta: la fundacion opera sobre el modelo de la rejilla, aguas arriba de esa fusion.
7. **Una notificacion agregada por operacion masiva.** Una aplicacion emite **un** evento con **exactamente**
   las celdas que cambiaron —no una por celda, no una por rejilla completa—. Un alcance que no cambia nada
   (no-op, idempotencia, alcance enteramente ausente) **no emite ningun evento**. Los observadores vivos
   (la nota de holgura del desviador) recalculan una vez, no N veces.
8. **Sin rebuild por celda.** El control actualiza las casillas de las celdas notificadas y **no**
   reconstruye la rejilla (invariante de rendimiento de AGENTS §6, ya vigente para `CellChanged`).
9. **Habilitacion y motivo visible.** Un alcance no aplicable esta **deshabilitado** con el motivo en el
   tooltip —nunca aplicable-pero-inerte—. Los tres motivos canonicos: el dialogo no declaro ese alcance,
   no hay celda primaria, y la rejilla no tiene ninguna celda presente. Es la misma regla que I-33 §3.9
   fijo para los controles ligados a celda de un frente en blanco.
10. **Un alcance NO DEFINIDO falla cerrado.** Un `enum` de C# es un `int`, asi que un cast o un miembro
    anadido despues sin revisar los `switch` puede traer un valor que nadie conoce. Las dos ramas
    `default:` significaban «Todo»: `ApplyScope` reescribia **toda** la rejilla y `For` rotulaba el
    alcance desconocido «Todo». En una matriz de seguridad ampliar en silencio al maximo es la peor falla
    posible, asi que ahora `ApplyScope` comprueba la pertenencia antes de tocar nada —cero mutacion, cero
    evento— y `For` devuelve cadena vacia en vez de tomar prestada la etiqueta de `All`, que pasa a ser
    un caso explicito.
11. **La celda primaria es la ultima celda VALIDA con la que el usuario interactuo.** La emite
    `SelectionMatrix.CellInteracted`, que solo se dispara ante un clic real; un cambio programatico
    —incluida una edicion masiva— nunca mueve el ancla. Solo las celdas presentes tienen casilla, asi que
    la columna de un frente en blanco jamas puede ser primaria.

## 8. Fases

1. **Reclamo.** Rama + worktree desde `origin/main`, commit vacio de reclamo con `Claim-Id` y push sin
   force. (Evidencia: rama remota aceptada.) — **HECHA**
2. **Gate documental.** Registro minimo de I-34 en `ROADMAP.md`, este contrato desde `TEMPLATE.md`,
   indice e estado versionado, **sin codigo funcional**. (Evidencia: commit documental propio.) — **HECHA**
3. **Regresiones rojas + fundacion comun.** Las pruebas de §9 primero, verificadas **fallando**; luego la
   fundacion minima y pura que las satisface. (Evidencia: §14.) — **HECHA**
4. **Adopcion por los dialogos.** Guarda del enum (regresion roja primero), superficie WPF compartida y
   adopcion por los tres dialogos incluidos, con sus pruebas STA sobre las ventanas reales. — **HECHA**
5. **Cierre de sesion y gates del dueno.** Suites, builds Debug de UI y Plugin, CI sobre la punta
   publicada, DLL Debug candidato y checklist de AutoCAD (§10) **abierto**. — **HECHA**

## 9. Pruebas y builds

Regresiones nuevas (todas en `tests/RackCad.UI.Tests/`, donde vive `SelectionMatrixModel`):

- `SelectionMatrixBulkEditTests` (puras, sobre el modelo): **celda**, **fila (nivel)**, **columna
  (frente/poste)**, **todo**, rejilla **rectangular**, rejilla **dentada**, **celdas ausentes** nunca
  tocadas ni reportadas, **no-op** (aplicar el valor que ya tienen ⇒ cero cambios, cero eventos),
  **idempotencia** (repetir la operacion no cambia ni notifica) y **notificacion agregada** (un evento por
  operacion, con exactamente las celdas cambiadas).
- `SelectionMatrixBulkEditorStaTests` (STA): **celda primaria** (se fija solo sobre celdas presentes, se
  rechaza sobre ausentes/fuera de rango, se limpia), **habilitacion** por alcance, **tooltip** con el
  motivo y **etiquetas** declaradas por el dialogo («Frente» vs «Poste»); mas la guarda de que el control
  actualiza **solo** las casillas cambiadas y **sin rebuild**.
- `SelectionMatrixScopeGuardTests` (§7.10): un alcance **no definido** no muta ni notifica, no «repara»
  una rejilla ya parcialmente apagada y no hereda la etiqueta de «Todo»; las cuatro etiquetas definidas
  siguen intactas y el editor lo rechaza como rechaza uno no declarado.
- `SafetyGridBulkAdoptionTests` (STA, sobre los **dialogos reales**): la fila existe y esta en el arbol
  visual de las tres ventanas; un clic fija la primaria y el ultimo clic manda; activar/desactivar por
  celda, nivel, columna y todo con el `OffCells` resultante comprobado hasta el resultado del dialogo;
  etiquetas **Frente** y **Poste**; tooltips de alcance deshabilitado y habilitado, siguiendo al estado;
  rejillas dentadas y niveles presentes solo en algunas columnas; frente en blanco cuya columna no puede
  ser primaria, que ningun alcance toca y cuya configuracion **dormida** sobrevive incluso a «Todo» y
  vuelve intacta al reactivarlo; la **nota viva del desviador** recalculada exactamente **una** vez (y
  ninguna cuando la operacion no cambia nada); el **tope posterior de Push Back** de extremo a extremo
  hasta `PushBackRearTopeConfig` y su predicado de dominio; los **tres sistemas** sobre la rejilla del
  desviador; y la conservacion de las **mismas instancias** de `CheckBox`, del **scroll** y del **tamano**
  de la ventana.

Comandos:

- `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` — suite UI verde.
- `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` — suite core verde (sin regresion).
- `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` — 0 errores, 0 advertencias propias.
- `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` — 0 errores propios (solo los
  `MSB3277` conocidos de AutoCAD); produce el DLL candidato para la validacion del dueno.
- CI: los cuatro jobs verdes sobre la punta publicada.

## 10. Validacion manual — gates ABIERTOS

La adopcion cambia la **interaccion visible** de tres dialogos compartidos por los tres sistemas, asi que
`autocad` y `owner-validation` quedan **abiertos**. El dibujo, el BOM y la persistencia **no** cambian por
construccion (lo que cada dialogo devuelve sigue siendo su mismo conjunto de `OffCells`), pero eso es
justo lo que el dueno debe confirmar en AutoCAD 2025 sobre el DLL Debug del **worktree de I-34**:
`…\feature-edicion-masiva-seguridad\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`.

- [ ] **Selectivo → Elementos de seguridad → Larguero tope**: la fila «Aplicar a:» aparece bajo la
      rejilla con Activar/Desactivar y los botones Celda / Nivel / **Frente** / Todo. Sin celda
      seleccionada solo «Todo» esta activo y los demas explican por que en su tooltip.
- [ ] Pulsar una celda: pasa a ser la primaria (la fila lo dice), y los cuatro alcances se habilitan.
      **Nivel** apaga ese nivel en todos los frentes; **Frente** apaga ese frente entero; **Todo** apaga
      la rejilla; con **Activar** vuelven. Aceptar y reabrir: exactamente esas celdas siguen apagadas.
- [ ] **Selectivo → Desviador**: identico, pero el boton de columna se llama **Poste**. Al aplicar un
      alcance, la **nota de holgura** se actualiza una sola vez y dice lo mismo que diria marcando esas
      celdas a mano.
- [ ] **Dinamico → Desviador y Guia de entrada**: lo mismo; la guia usa **Frente**. El selector de cara
      del desviador sigue visible en el Dinamico.
- [ ] **Push Back → Desviador**: lo mismo; su selector de cara sigue **oculto** (PB-003).
- [ ] **Push Back → Seguridad → Tope posterior (extremo alto) → Configurar…**: la fila aparece con eje
      **Frente**; aplicar un alcance y Aceptar deja el tope posterior exactamente en esas celdas, y el
      SAQUE no cambia.
- [ ] **Frente en blanco** (Dinamico o Push Back): la columna del frente en blanco no tiene celdas, no
      puede seleccionarse como primaria y **ningun** alcance —ni «Todo»— la toca. Aceptar y reactivar el
      frente: su configuracion guardada **vuelve intacta**.
- [ ] **Todos/Ninguno** siguen funcionando como siempre, y una rejilla en la que no se usa la fila nueva
      se comporta exactamente como antes.
- [ ] **Dibujo y BOM**: tras usar la edicion masiva, el rack dibujado y el BOM coinciden con lo que
      darian las mismas celdas marcadas a mano, en las cuatro vistas y en los tres sistemas.
- [ ] **Round-trip**: guardar, cerrar y reabrir con `RACKEDITAR` conserva el mismo GUID y las mismas
      celdas.

## 11. Criterios de aceptacion

- Existe una fundacion **pura** sobre `SelectionMatrixModel` con los cuatro alcances, el estado
  Activar/Desactivar y la celda primaria transitoria; no referencia `RackSystemKind` ni ningun sistema.
- Las celdas ausentes nunca cambian ni se reportan; el no-op y la repeticion no emiten evento.
- Cada operacion masiva emite **una** notificacion con exactamente las celdas cambiadas; el control
  repinta esas casillas sin reconstruir la rejilla.
- Las etiquetas del eje de columna y las capacidades las declara el llamador; los alcances no aplicables
  quedan deshabilitados con motivo en el tooltip.
- Un alcance **no definido** no muta ni notifica y no hereda la etiqueta de «Todo».
- Los **tres dialogos incluidos** consumen la fila compartida, declaran su eje («Poste» el desviador,
  «Frente» los otros dos) y **no** contienen ninguna rama por Selectivo, Dinamico o Push Back.
- La nota viva del desviador se recalcula **una** vez por operacion masiva, y ninguna si no cambio nada.
- Una edicion masiva conserva las mismas instancias visuales, el scroll y el tamano de la ventana.
- Las regresiones nuevas se verificaron **rojas** antes del codigo que las satisface.
- Suites core y UI verdes; builds Debug de UI y Plugin en 0 errores propios; CI verde.
- **Ningun DTO, store, geometria, BOM, catalogo, bloque ni shell cambia**, y una rejilla en la que no se
  usan las acciones nuevas se comporta exactamente como antes.

## 12. Condiciones para detenerse

- Que satisfacer un alcance exija cambiar el conjunto de `OffCells` que un dialogo persiste, el DTO o el
  formato de alambre.
- Que la fundacion no pueda mantenerse agnostica al sistema sin un `switch` por `RackSystemKind`.
- Que preservar «una notificacion agregada» obligue a reconstruir la rejilla.
- Que aparezca en `origin` una rama de I-23 o I-25, o cualquier otra tocando los mismos diálogos.
- Cualquier necesidad de un paquete NuGet nuevo, de un bloque DWG o de una fila de catalogo.
- Cualquier deriva hacia la **adopcion** de los dialogos dentro de este incremento.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en `docs/automation/state/I-34.yml`. La automatizacion esta pausada
(`automation.enabled: false`): el ejecutor es manual y mantiene ese archivo al cierre de cada sesion. No
se abre un segundo Pull Request ni se activa auto-merge. La integracion a `main` (`git merge --no-ff`,
WORKFLOW §4.5) se realiza en la sesion de integracion, no en esta rama.

## 14. Evidencia final

Se completa al cierre de cada sesion: commits logicos con trailer de procedencia, archivos creados y
modificados, resultados de `dotnet test` (core y UI), builds, evidencia de CI sobre el SHA publicado, SHA
base y punta de la rama, confirmacion del push, gates abiertos e invariantes comprobados.
`docs/HANDOFF.md` §8-12 y el estado en `docs/ROADMAP.md` se actualizan **solo** en la sesion de
integracion (ultimo commit de la rama), nunca desde esta rama.

## 15. Alcance restante

**Adopcion HECHA** en los tres dialogos incluidos: `SafetyDesviadorGridWindow` (M3, eje **Poste**, el caso
de PB-007 y el de mayor rendimiento, con su nota viva recalculada una vez por operacion),
`SafetyTopeGridWindow` (M1 **y** M2 — un solo dialogo, dos consumidores: el tope del Selectivo y el tope
posterior de Push Back— eje **Frente**) y `SafetyGuiaEntradaGridWindow` (M4, eje **Frente**). Los tres
conservan «Todos»/«Ninguno», el conjunto de `OffCells` que devuelven, la fusion de celdas dormidas
(`SafetyDormantCells`) y su interaccion previa: la edicion masiva se **suma**.

Queda **pendiente de decision del dueno**, no comprometido:

1. **Parrilla** (M5, §6.3): antes hay que decidir **como comparte el control una decoracion por celda**
   —su contador vivo de parrillas— sin perderla. Mientras eso no se decida, conserva su rejilla propia.
2. **Defensa** (§6.3): antes hay que decidir **que significa un alcance en un formulario por poste** con
   dos longitudes independientes y sin eje de nivel. Hoy «Celda» y «Nivel» no tienen sentido en ella.

Y como cierre de la iniciativa: la **validacion manual del dueno en AutoCAD** (§10), que es el gate
abierto.
