---
schema: rackcad-initiative/v1
id: I-33
title: Frente en blanco para Dinamico y Push Back
type: feature
status: integrated
branch: feature/frente-en-blanco
base_branch: main
priority:
size: M
depends_on: [I-18, I-21, I-30, I-31, I-32]
conflicts_with: [I-23, I-25]
context_packs: [system-dynamic-flowbed, ui-editors, architecture-kernel, persistence, autocad-plugin, documentation-governance, delivery-validation]
automation_state_path: docs/automation/state/I-33.yml
decision_paths: []
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

# Frente en blanco para Dinamico y Push Back

> Implementa **PB-014**, que I-32 dejo diferido en [`ideas-futuras.md`](../ideas-futuras.md) senalando que
> «es compartido con el Dinamico, asi que necesita decision de alcance». Esa decision la da el Owner en la
> instruccion de apertura de esta iniciativa. El campo `priority` se deja vacio por falta de fuente numerica
> en el ROADMAP, igual que en I-18 e I-32.

## 1. Objetivo

Permitir que un frente quede **en blanco**: conserva su claro y su estructura, sigue desplazando a los
frentes posteriores y **no lleva ningun nivel ni componente de carga**. Su configuracion queda **dormida**
para poder reactivarlo tal cual estaba. Aplica al **Dinamico** y a **Push Back**, que comparten la misma
estructura, y no toca ningun otro sistema.

## 2. Problema

Un almacen real tiene tramos que no pueden cargar: una columna del edificio, un paso de montacargas, un
tablero. Hoy el unico modo de representarlos es no dibujar el frente, lo que **encoge el rack** y desplaza
todo lo que va detras, o dejarlo cargado y descontar a mano en el BOM. Falta un estado explicito del frente
que separe «estructura» de «carga».

## 3. Alcance

Un frente tiene estado **Activo** o **En blanco**. En blanco:

1. **Conserva su claro**: `PalletCount`, `Bfr` y el largo de corte IN/OUT no cambian, asi que la retícula de
   postes es identica a la que tendria activo.
2. **Conserva su estructura**: postes y sus alturas, placas, cabeceras, separadores y postes derivados se
   dibujan igual. Su altura resuelta se calcula desde su configuracion dormida, para que la estructura
   «continue» exactamente como continuaria si llevara largueros (la redaccion de PB-014).
3. **Desplaza los frentes posteriores**: ocupa su lugar en la retícula longitudinal; `StartX`/`EndX` de los
   frentes que van detras y el `TotalLength` del rack no cambian.
4. **Cero niveles y componentes de carga efectivos**: sin larguero IN/OUT, sin larguero intermedio, sin cama,
   sin larguero posterior de Push Back, sin tope posterior y sin seguridad indexada por nivel — ni en las
   cuatro vistas ni en el BOM de ninguno de los dos sistemas.
5. **Configuracion dormida**: niveles, celdas, peraltes, posiciones, fondos y las celdas paralelas de Push
   Back se conservan intactas. Reactivar el frente reproduce el rack que tenia, **sin celda falsa**: el
   frente en blanco nunca se modela vaciando la fila ni poniendo contadores en cero.
6. **Compatibilidad legacy**: un documento anterior a I-33 no conoce la bandera y carga **todos los frentes
   activos**. Un rack sin frentes en blanco **serializa exactamente igual que antes** (la bandera se omite
   del wire, no se escribe `null`).
7. **Al menos un frente activo**: un rack que no carga nada no es un rack. La regla tiene **una sola**
   comprobacion canonica, `DynamicFrontActivation.HasActiveFront`, y se aplica en dos formas distintas que
   nunca se contradicen:
   - **Prevencion no destructiva en el editor**: `DynamicFrontMatrix.SetActive` **se niega** a blanquear el
     ultimo frente activo, no cambia nada (ni la seleccion) y devuelve `false` para que la ventana explique
     el motivo. Nunca reescribe la intencion del usuario.
   - **Rechazo con error visible** de un payload que llega explicitamente todo en blanco: lo lanza
     `DynamicRackSystemResolver.Validate` (y por composicion `PushBackResolver`) y lo reporta
     `RackDesignValidation` para los dos sistemas.

   **Nada normaliza en silencio.** Ni el DTO ni el resolver reactivan un frente por su cuenta: un documento
   todo en blanco se carga **verbatim** y se rechaza despues, para que el defecto sea visible en vez de
   quedar enmascarado. Los documentos legacy no traen la bandera, asi que cargan **todos activos** y jamas
   caen en este caso.

8. **Crecer y reducir frentes**: al crecer, el frente nuevo **nace ACTIVO aunque el template seleccionado
   este en blanco** — agregar un frente es agregar rack, y heredar la blancura crearia tramos muertos que el
   usuario no pidio. Todo lo demas **si** se clona del template (posiciones, niveles, fondos, inicio en
   fondo y celdas). Al reducir se eliminan los frentes finales; los indices se renumeran, la seleccion se
   re-acota y los frentes que sobreviven conservan su estado y su configuracion dormida, junto con las
   celdas paralelas de Push Back.

9. **Seleccion de un frente en blanco**: la seleccion **sigue siendo valida** (celda unica acotada del propio
   frente), pero se **deshabilita** todo control que edite niveles o celdas inexistentes —incluidos los
   alcances/aplicaciones ligados a celda— con el motivo visible en el tooltip. Los controles **estructurales**
   del frente (posiciones, fondos, inicio en fondo y los botones que copian datos del frente) siguen
   disponibles. Reactivar el frente **restaura la edicion de inmediato**.

10. **La frontera compartida por dos frentes en blanco NO existe** (decision del Owner). Los dos bordes
    exteriores del rack existen siempre; una frontera interior existe salvo que sus **dos** frentes
    adyacentes esten en blanco. Una corrida de N blancos conserva solo sus dos fronteras exteriores y pierde
    sus N−1 interiores; un blanco aislado y los blancos alternados no suprimen ninguna. Desaparece el
    **ensamble fisico** —poste, placa, cabecera/separador, postes derivados y refuerzos, el corte lateral
    entero, su parte del BOM y su seguridad por poste—, **nunca el frente logico**: indices, claros, ancho,
    largo total, coordenadas X, configuracion dormida y persistencia se conservan. Detalle en §6.4.

## 4. Fuera de alcance

- El **Selectivo** (no comparte la estructura dinamica) y cualquier otro sistema.
- **PB-001** (preview de las tres vistas), **PB-007** (reconfigurador masivo de seguridad) y **PB-011**
  (editor avanzado de modulos): siguen diferidos en `ideas-futuras.md`.
- **I-23** (namespaces) e **I-25** (guardas traseras).
- Catalogos, bloques DWG, el formato fisico del Xrecord, el shell visual y cualquier refactor oportunista.
- La duplicacion conocida de la regla «niveles en un poste» (`DynamicFrontGeometry.LoadLevelsAtPost` y la
  copia privada de `DynamicSafetyMultiViewBuilder`): I-33 hace que **ambas** respeten la bandera, pero **no**
  las colapsa — sigue registrada en `ideas-futuras.md`.

## 5. Contexto requerido

- Global: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md` §7.
- Context Packs: `system-dynamic-flowbed`, `ui-editors`, `architecture-kernel`, `persistence`,
  `autocad-plugin` (el Plugin dibuja las cuatro vistas y arma el BOM que esta iniciativa cambia),
  `documentation-governance` (contrato, estado, indice, glosario e `ideas-futuras`) y `delivery-validation`.
- Iniciativas previas: I-18 (Push Back sobre la estructura dinamica), I-21 (estado del editor dinamico),
  I-30/I-31 (shell visual), I-32 (correcciones de Push Back y el registro de PB-014).

## 6. Diseno

### 6.1 Autoridad compartida

La bandera vive en la **estructura dinamica**, que Push Back **compone** (`PushBackDesign.Structure` es un
`DynamicRackDesign`), de modo que la regla se enuncia una sola vez y los dos sistemas no pueden divergir:

- `DynamicRackFrontDesign.IsActive` (intencion editable, default `true`).
- `DynamicRackFront.IsActive` (frente resuelto, default `true`).
- `DynamicFrontActivation` (Application): **unica** autoridad. Expone `IsBlank`, `EffectiveLoadLevels`,
  `EffectiveLevelsPerFront`, `EffectiveLevelsPerPost`, `Active`, `HasActiveFront`, el mensaje unico
  `AllBlankMessage` y —para la frontera fisica de §6.4— `BoundaryExists`, `FrontActivation` y
  `PresentBoundaries`. Todos sus miembros son **puros**: ninguno normaliza ni reactiva nada.

`EffectiveLoadLevels` devuelve **cero** para un frente en blanco y el historico `Math.Max(1, LoadLevels)`
para uno activo. Por eso **un rack sin frentes en blanco no cambia en nada**: los consumidores que antes
escribian `Math.Max(1, front.LoadLevels)` ahora llaman a la autoridad y obtienen el mismo numero.

### 6.2 Embudo unico de carga

`DynamicFrontGeometry.LoadBeamLevels(system, front)` devuelve lista vacia para un frente en blanco. Por ese
embudo pasan los largueros del frontal, `DynamicLoadBeamGeometry.Placements`, los ejes de cama
(`DynamicFlowBedGeometry.Resolve`), los largueros bajo y alto de Push Back
(`PushBackLoadBeamGeometry.LowBeams`/`HighBeams`) y el tope posterior (`PushBackRearTopeBuilder`). Un solo
punto apaga seis consumidores.

Los que **no** pasan por el embudo consultan la autoridad explicitamente: la planta dinamica (salta el
frente), el conteo por poste (`LoadLevelsAtPost` y la copia privada de la seguridad), las camas del lateral,
los peraltes intermedios, la proyeccion rack-wide del resolver, `PushBackHighEndBeamGeometry`,
`PushBackElevations` y las tres categorias de carga de cada BOM.

### 6.3 Lo que deliberadamente NO se apaga

- **Altura y peralte de postes**: `PostHeight` sigue tomando el maximo de los frentes adyacentes, incluido
  el frente en blanco con su altura dormida. Es lo que hace que la estructura «continue».
- **Numeracion de frentes** (`NumberFronts`): un frente en blanco sigue siendo un frente y conserva su
  numero, para que la secuencia que ve el usuario coincida con la del dibujo.
- **Un frente nuevo nace activo** aunque el template seleccionado este en blanco (regla de §3.8).

### 6.4 La frontera compartida por dos frentes en blanco (decision del Owner)

Un rack de N frentes tiene **N+1 fronteras**. La regla canonica:

| Frontera | Existe |
|---|---|
| Borde exterior inicial (0) y final (N) | **Siempre** |
| Activo / Activo | Si |
| Activo / En blanco y En blanco / Activo | Si |
| **En blanco / En blanco** | **No** |

Una corrida de N frentes en blanco conserva **solo sus dos fronteras exteriores** y pierde sus **N−1**
interiores. Un frente en blanco **aislado** no suprime ninguna, y frentes en blanco **alternados** tampoco.

La autoridad es **una** —`DynamicFrontActivation.BoundaryExists` / `PresentBoundaries`, sobre el estado de
los dos frentes adyacentes— y la consumen **todas** las materializaciones: frontal, lateral (el corte entero
desaparece), planta, postes, placas, cabeceras, separadores, postes derivados y refuerzos, el BOM de los dos
sistemas, la seguridad indexada por poste, y el preview y los planes de insercion/actualizacion. El guardia
del corte lateral vive en `Build(system, catalog, postIndex)` y no solo en `Cortes`, para que dibujo y
preview no puedan divergir.

**Lo que NO cambia.** Desaparece el **ensamble fisico**, nunca el frente logico: se conservan los indices de
frente, los claros, el ancho, la configuracion dormida, la persistencia y —verificado— **el largo total y
todas las coordenadas X**, incluida la de la frontera suprimida, que el layout sigue calculando. Los cortes
que sobreviven conservan su **numero de poste** original; en el selector de vista lateral el poste ausente
sigue listado, marcado «(sin frontera)», para que la numeracion no se mueva.

**La seguridad del poste ausente queda dormida.** No se desplaza a otro poste ni se borra: la seleccion
guardada conserva su indice de poste. Mientras la frontera no exista, ese poste aporta **cero** niveles, asi
que su columna en la rejilla del desviador es **ausente** —no seleccionable— y `SafetyDormantCells` la
preserva a traves del dialogo. Al reactivar cualquiera de los dos frentes reaparecen el poste, todas sus
piezas fisicas y sus celdas de seguridad, y dibujo y BOM vuelven al estado equivalente anterior.

### 6.5 Los dialogos de seguridad

Las rejillas de seguridad que abren el Dinamico y Push Back tratan un frente en blanco como lo que es: un
frente **que existe** pero **sin niveles**.

- **Celdas inexistentes.** Los editores entregan al dialogo los conteos de la **autoridad**
  (`DynamicFrontMatrix.EffectiveLevelCounts` y `DynamicFrontActivation.EffectiveLevelsPerPost`), no un
  `Math.Max(1, LoadLevels)` propio. Un frente en blanco aporta **cero**, y con cero la columna entera se
  construye **ausente** por el soporte de rejilla dentada de `SelectionMatrixModel` (I-22): no se dibuja, no
  se puede seleccionar y `Toggle` no reporta cambio, asi que tampoco se le puede aplicar nada.
- **Dos listas distintas, cada una a lo suyo.** El diseno tiene N frentes y N+1 postes, y el dialogo recibe
  **las dos** listas:
  - `levelsPerFrente` (N) para la **guia** y las demas rejillas frente x nivel;
  - `desviadorLevelsPerPost` (N+1) **exclusivamente** para el desviador, por la regla canonica «el frente
    adyacente mas alto manda» del dibujo.

  El Dinamico entregaba su lista **por frente marcada como por poste**, asi que el ultimo poste caia
  artificialmente a **1** nivel y los interiores vecinos de un frente mas alto perdian niveles que el dibujo
  si colocaba — el mismo defecto de contrato que **PB-002** corrigio en Push Back. Entregar la lista por
  poste corrige la **forma de la rejilla** y **no** toca la lectura de la celda en el dibujo
  (`DesviadorCellsAreByPost` sigue en `false` para el Dinamico, y esa decision sigue siendo del Owner).
- **Configuracion dormida.** Una rejilla solo puede informar de las celdas que muestra, asi que aceptar el
  dialogo **borraria** las celdas guardadas de la columna ausente. Lo impide `SafetyDormantCells.Merge`, una
  regla **pura y unica** que las tres rejillas indexadas por nivel —desviador, guia de entrada y tope—
  consumen: lo que se persiste es lo visible **mas** lo dormido. Al reactivar el frente reaparecen
  intactas y editables.
- **El frente no se oculta ni cambia de indice**: sigue siendo la misma columna, en la misma posicion, con su
  numero. Lo que cambia es que sus celdas de nivel no existen.
- **Forma y selector de lado, desacoplados.** La visibilidad del selector de cara de pasillo del desviador
  se decidia **derivandola** de `desviadorLevelsPerPost == null`, es decir: pedir la forma por poste apagaba
  el selector. A Push Back le servia porque queria las dos cosas, pero el Dinamico necesita la forma por
  poste y **debe conservar** su selector — con la regla derivada lo habria perdido en silencio. Ahora son
  **parametros independientes**: `desviadorLevelsPerPost` gobierna **solo** la forma de la rejilla, y
  `showDesviadorSide` (default `true` = comportamiento vigente) **solo** el selector. Push Back lo apaga de
  forma **explicita** (PB-003); el Dinamico y el Selectivo lo conservan.
- **El Selectivo no cambia.** Todo lo nuevo es **opt-in** con default igual al comportamiento vigente
  (`allowBlankFrontColumns: false`, `showDesviadorSide: true`, y el Selectivo tampoco pasa
  `desviadorLevelsPerPost` ni `fallbackLevelsArePerPost`). No tiene frentes en blanco, nunca entrega un
  cero, y por tanto ni la fusion de dormidas ni el cero-como-ausente le aplican; una columna meramente
  **mas corta** (rejilla dentada) conserva su regla historica de descartar las celdas fuera de rango, y la
  rama `Math.Max(left, right) + 1` del fallback per-front queda limitada al camino legacy/Selectivo: tras
  este arreglo **ninguna** ruta de I-33 la recorre.

### 6.6 Un solo rechazo, ninguna normalizacion

`DynamicFrontActivation.HasActiveFront` es la **unica** comprobacion; el mensaje tambien es unico
(`AllBlankMessage`), asi que el usuario lee la misma frase la atrape quien la atrape:

| Limite | Que hace ante un payload todo en blanco |
|---|---|
| `DynamicFrontMatrix.SetActive` (editor) | **Previene**: se niega, no cambia nada y devuelve `false` |
| `DynamicRackSystemResolver.Validate` | **Lanza** `ArgumentException` con `AllBlankMessage` |
| `PushBackResolver.Resolve` | Hereda el lanzamiento por composicion de la estructura |
| `RackDesignValidation.IsUsableDynamic` / `IsUsablePushBack` | Devuelven `false` |
| `DynamicRackSystemDocument.ToDesign` / `ToDomain` | **Nada**: cargan verbatim, sin reactivar frentes |
| `DynamicFrontMatrix.BuildFrontDesigns` | **Nada**: no reescribe la intencion del usuario |

Las tres normalizaciones silenciosas que tuvo la primera version de I-33 (resolver, DTO y matriz) quedaron
**retiradas**: reactivar un frente por cuenta propia enmascaraba el defecto y creaba guardas divergentes.

### 6.7 Persistencia

`DynamicRackFrontDocument.IsActive` es `bool?` con `[JsonIgnore(WhenWritingNull)]`:

- Frente activo ⇒ **no se escribe nada** (los stores no ignoran nulos globalmente, asi que sin el atributo
  todo documento existente ganaria `"IsActive": null` en cada frente).
- Frente en blanco ⇒ `"IsActive": false`.
- Documento sin la clave ⇒ `IsActive = true` (fallback legacy).

El mismo DTO sirve al diseno y al sistema resuelto, y `PushBackDesignDocument.Structure` lo compone, asi que
la persistencia de los dos sistemas queda cubierta en un solo sitio.

## 7. Regresiones

`tests/RackCad.Tests/BlankFrontTests.cs` (28), `tests/RackCad.Tests/BlankFrontSafetyTests.cs` (9),
`tests/RackCad.UI.Tests/BlankFrontEditorTests.cs` (8) y
`tests/RackCad.UI.Tests/BlankFrontSafetyGridTests.cs` (11) y
`tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs` (14) y
`tests/RackCad.Tests/BlankFrontBoundaryTests.cs` (15). El
estado rojo se verifico con la bandera ya declarada pero **sin ningun consumidor**: 11 de 19 fallaban
—exactamente las de comportamiento— y las 8 restantes eran las de autoridad y las guardas de estructura
(que deben pasar tanto antes como despues, porque afirman que la estructura NO cambia).

Cubren, ademas del contrato de dibujo y BOM:

- **Rechazo del todo-en-blanco** en sus tres formas: DTO que carga verbatim, resolucion que lanza (dinamico
  y Push Back) y carga directa JSON → documento → diseno → validacion canonica.
- **Legacy sin bandera**: nunca se ve como todo-en-blanco y pasa la validacion de los dos sistemas.
- **Crecer y reducir frentes** con el seleccionado activo y en blanco: indices renumerados, seleccion
  re-acotada, configuracion dormida intacta, el frente nuevo activo aunque el template este en blanco, y el
  paralelo de Push Back alineado por indice sin recortar celdas.
- **Editabilidad por seleccion** (STA, las dos ventanas reales): al seleccionar un frente en blanco la
  seleccion sigue siendo valida y los controles de nivel/celda y los alcances quedan deshabilitados con
  motivo visible, mientras los estructurales siguen disponibles; al reactivar se restauran de inmediato.
- **Seguridad** (puras + STA sobre las rejillas reales): frente activo con seguridad editable como siempre;
  activo → en blanco con la columna ausente, no seleccionable y sin cambio al intentar aplicar; en blanco →
  activo con los valores restaurados y editables; rejillas dentadas y frentes consecutivos en blanco; y la
  guarda de que **sin el opt-in** —el caso del Selectivo— el suelo historico y el descarte de celdas fuera
  de rango se conservan verbatim.
- **Handoff del desviador** (STA sobre el camino REAL de `RackDynamicSystemWindow`): frentes activos con
  niveles distintos, frente en blanco inicial / medio / final, dos consecutivos, el ultimo poste heredando
  su frente adyacente sin caer a 1, y la comparacion explicita entre el handoff viejo (que si lo colapsaba)
  y el nuevo sobre la rejilla real. Mas el desacople del selector de lado en sus tres combinaciones y
  guardas de fuente de que el Dinamico lo conserva, Push Back lo apaga explicitamente, la guia recibe la
  lista por FRENTE (no la de N+1 postes) y el Selectivo no opta por ningun parametro.
- **Frontera En blanco/En blanco** (§6.4), verificadas **rojas** contra el candidato anterior (6 de 14
  fallando: exactamente las materializaciones; pasaban las de autoridad, geometria, round-trip y rack sin
  blancos, que deben pasar antes y despues): un blanco aislado no suprime nada; dos consecutivos al inicio,
  medio y final suprimen una; tres suprimen dos; dos corridas independientes suprimen una cada una; los
  alternados no suprimen ninguna; largo total y coordenadas X identicas; frontal, planta y cortes laterales
  omiten **la misma** frontera; el BOM baja exactamente cabecera, poste reforzado, separador, poste y placa;
  la seguridad del poste ausente no se mueve de indice ni se borra y vuelve al reactivar; round-trip de
  persistencia; y un rack sin frentes en blanco intacto.

## 8. Validacion manual en AutoCAD — APROBADA por el Owner (2026-07-27)

El Owner **aprobo toda la validacion manual**, incluida la **ronda focalizada de fronteras fisicas** que el
mismo dirigio (punto 11). Candidato aprobado: **`b840cfe24578bc9faa3b13dad8b11d90d47aad84`**, CI del
candidato run **30240730244** 4/4. Los catorce puntos quedan cerrados.

1. Dinamico con 3 frentes: marcar «En blanco» el del medio ⇒ el rack **no encoge**, los postes siguen a la
   misma altura y en la misma X, los frentes posteriores no se mueven.
2. En ese frente no aparece ningun larguero IN/OUT, ningun larguero intermedio y ninguna cama, en frontal de
   salida, frontal de entrada, lateral (los dos cortes que lo tocan) y planta.
3. Cabeceras, separadores y postes derivados del tramo en blanco siguen dibujados.
4. BOM: bajan largueros IN/OUT, largueros intermedios y camas; cabeceras, separadores y postes no cambian.
5. Desmarcar «En blanco» ⇒ el frente vuelve **exactamente** como estaba (mismos niveles y valores de celda).
6. Repetir 1-5 en Push Back, verificando ademas que el frente en blanco no lleva **larguero posterior** ni
   **tope posterior**.
7. Intentar dejar todos los frentes en blanco ⇒ se rechaza con aviso, el ultimo activo se conserva y **nada
   mas cambia** (la casilla vuelve a su estado, la seleccion no se mueve).
8. Con el frente en blanco **seleccionado**: los campos de nivel y de celda y los botones de «Aplicar a:»
   estan **deshabilitados** y su tooltip explica por que; posiciones, fondos e inicio en fondo siguen
   editables. Al desmarcar «En blanco» todo vuelve a editarse de inmediato.
9. Agregar frentes con un frente **en blanco seleccionado** ⇒ los nuevos nacen **activos** y con el resto de
   los valores del template; quitar frentes ⇒ se van los finales y los que quedan conservan su estado.
10. **Seguridad**: con un frente activo, apagar un par de celdas de desviador (y de guia en el Dinamico) y
    aceptar. Poner ese frente **en blanco** y reabrir «Configurar seguridad…»: su columna aparece **sin
    celdas** —no se puede marcar, desmarcar ni aplicarle un alcance— **sin que el frente desaparezca ni
    cambie de numero**, y los demas frentes siguen editables. Aceptar el dialogo asi. Reactivar el frente y
    reabrir: **las mismas celdas apagadas siguen ahi**, editables e intactas. En Push Back repetir tambien
    sobre el **tope posterior**. Con **dos frentes consecutivos en blanco**, el poste que comparten queda sin
    celdas y los de los extremos conservan las suyas.
11. **Frontera compartida**: con **un solo** frente en blanco, sus **dos** postes siguen ahi. Poner en blanco
    tambien el frente contiguo ⇒ el poste que comparten **desaparece por completo** —sin poste, placa,
    cabecera, separador, postes derivados ni refuerzos— en frontal, planta y lateral (ese corte ya no se
    dibuja), y el BOM baja exactamente esas piezas. **El rack no se encoge**: el ancho de los frentes, el
    largo total y la posicion de todo lo demas son identicos. Con **tres** frentes en blanco seguidos
    desaparecen **dos** postes; con blancos **alternados**, ninguno. Reactivar cualquiera de los dos frentes
    devuelve el poste y todas sus piezas. La seguridad que ese poste tenia configurada **sigue ahi** al
    volver, en el mismo poste.
12. Guardar, cerrar y reabrir con `RACKEDITAR`: el frente en blanco sigue en blanco y conserva el mismo GUID.
13. Abrir un rack **guardado antes de I-33** (biblioteca o DWG): todos sus frentes cargan activos y su dibujo
    y BOM son identicos a los de antes.
14. Abrir el **Selectivo** y su dialogo de seguridad: identico a como estaba (no tiene frentes en blanco).
