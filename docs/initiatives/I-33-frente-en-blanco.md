---
schema: rackcad-initiative/v1
id: I-33
title: Frente en blanco para Dinamico y Push Back
type: feature
status: implementing
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
  `EffectiveLevelsPerFront`, `Active`, `HasActiveFront` y el mensaje unico `AllBlankMessage`. Todos sus
  miembros son **puros**: ninguno normaliza ni reactiva nada.

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
- **La rejilla del dialogo de seguridad** sigue ofreciendo los niveles dormidos del frente en blanco. Es
  coherente con la dormancia y es inocuo: el dibujo y el BOM ya no colocan seguridad indexada por nivel en
  ese frente. Decision consciente, no omision.
- **Un frente nuevo nace activo** aunque el template seleccionado este en blanco (regla de §3.8).

### 6.4 Un solo rechazo, ninguna normalizacion

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

### 6.5 Persistencia

`DynamicRackFrontDocument.IsActive` es `bool?` con `[JsonIgnore(WhenWritingNull)]`:

- Frente activo ⇒ **no se escribe nada** (los stores no ignoran nulos globalmente, asi que sin el atributo
  todo documento existente ganaria `"IsActive": null` en cada frente).
- Frente en blanco ⇒ `"IsActive": false`.
- Documento sin la clave ⇒ `IsActive = true` (fallback legacy).

El mismo DTO sirve al diseno y al sistema resuelto, y `PushBackDesignDocument.Structure` lo compone, asi que
la persistencia de los dos sistemas queda cubierta en un solo sitio.

## 7. Regresiones

`tests/RackCad.Tests/BlankFrontTests.cs` (28) y `tests/RackCad.UI.Tests/BlankFrontEditorTests.cs` (8). El
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

## 8. Validacion manual en AutoCAD (pendiente del Owner)

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
10. Guardar, cerrar y reabrir con `RACKEDITAR`: el frente en blanco sigue en blanco y conserva el mismo GUID.
11. Abrir un rack **guardado antes de I-33** (biblioteca o DWG): todos sus frentes cargan activos y su dibujo
    y BOM son identicos a los de antes.
