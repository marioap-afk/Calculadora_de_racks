---
schema: rackcad-initiative/v1
id: I-46
title: Selectivo — tope de tarima por lado (ID12)
type: fix
status: in-progress
branch: fix/selectivo-topes-izquierda-derecha
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [system-selective, ui-editors, persistence]
automation_state_path:
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

# Selectivo — tope de tarima por lado (ID12)

> **Fase actual: GATE 3 CERRADO — CONTRATO FIJADO Y CARACTERIZADO.** Esta iniciativa se abrio por autorizacion explicita del
> dueno (caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2): ID12 vivia en
> [ideas-futuras.md](../ideas-futuras.md) como pendiente conocido del Selectivo, sin fila propia. El
> reclamo atomico se hizo antes que este bootstrap y la fila en [ROADMAP.md](../ROADMAP.md) se crea
> con el, que es el orden que manda el proceso.
>
> **Lo entregado hasta aqui es reproduccion y caracterizacion. No hay cambio de produccion.** Las
> pruebas de contrato estan ROJAS a proposito y las de caracterizacion VERDES: las primeras fijan lo
> que el dueno pide, las segundas congelan lo que el codigo hace hoy en multi-fondo para que el
> arreglo pueda demostrar que celda preserva y que celda mueve.
>
> **CONTRATO DE LADOS, FIJADO POR EL DUENO (Gate 3). No se re-decide:**
>
> - `Ninguno` = **ningun** extremo.
> - `Izquierda` = extremo **BAJO** del marco **local** del rack.
> - `Derecha` = extremo **ALTO**.
> - `Ambas` = **union exacta** de ambos.
> - **Nunca World X**: bajo/alto se leen sobre el eje de profundidad LOCAL, donde
>   `SelectiveDepthLayout.Offsets` pone el poste frontal en 0 y cada fondo siguiente por encima.
> - `SafetySide` conserva sus ordinales **0/1/2/3**: no cambia el enum ni su orden.

## 1. Objetivo

Que el tope de tarima del Selectivo se pueda pedir **por lado** y que el lado pedido se vea en el
**resultado fisico**: `Ninguno` no coloca ninguna pieza, `Izquierda` coloca una, `Derecha` coloca una
en la posicion **opuesta** a la de `Izquierda`, y `Ambas` coloca las dos. El criterio se verifica en un
rack de **un solo fondo**, que es donde el modelo de hoy no puede expresarlo.

Verificable en el resultado fisico —el que consumen dibujo y BOM—, no en la intencion guardada.

## 2. Problema

ID12, registrado en [ideas-futuras.md](../ideas-futuras.md) («Producto: pendientes conocidos del
Selectivo», punto 9) y dejado **expresamente fuera** de I-43 con la nota de que la parte de **modelo y
colocacion** es una iniciativa propia, no un ajuste del editor.

Hoy el lado del tope **no significa un lado**. `SelectiveSafetyPlacement.TopeSpots(selection, fondoCount)`
lo interpreta como **que fondo del par central** lleva la pieza:

- `Left`  -> el poste **trasero** del fondo `c`;
- `Right` -> el poste **delantero** del fondo `c + 1`, y **solo si `c + 1 < fondoCount`**;
- `Both`  -> los dos anteriores, con la misma condicion sobre el segundo.

Es un eje de **profundidad**, no de lado, y arrastra tres consecuencias:

1. **Con un solo fondo el eje no existe.** `c + 1 < fondoCount` es falso, asi que `Derecha` y `Ambas`
   colapsan sobre lo mismo que `Izquierda` (o sobre nada). Un rack de un fondo no puede pedir el tope
   del otro lado.
2. **`Ninguno` no es alcanzable.** El selector del dialogo (`SafetyTopeGridWindow`) ofrece tres
   opciones —`Izquierda`, `Derecha`, `Ambos`—; `SafetySide.None` no se puede elegir, y si llega de un
   documento anterior `SelectiveSafetyWindow` lo reescribe a `Both` al cargar la fila. Ademas
   `TopeSpots` devuelve una posicion **antes de mirar el lado** cuando `TopeShared` es cierto.
3. **La intencion no tiene sede propia.** El lado del tope viaja en `SelectiveSafetySelection.Side`,
   el campo generico de la familia, y no en `SelectiveTopeConfig`, que es donde viven `Shared`,
   `Fondo`, `Saque`, `Frontal` y `OffCells`. `SelectiveTopeConfig` **no tiene** campo de lado.

## 3. Alcance

- **Reproduccion primero**: una prueba de la suite Core que demuestre, sobre el **resultado fisico** de
  un rack de **un fondo**, los cuatro casos `Ninguno / Izquierda / Derecha / Ambas`.
- El **modelo y la colocacion** del tope por lado: que significa el lado, donde vive la intencion y
  como se materializa en posiciones fisicas.
- Los cuatro limites que un flag de seguridad del Selectivo obliga a cruzar (AGENTS.md, convencion 3):
  `DeepCopy`, resolver/vista por fondo, DTO `From`/`ToDomain` con su fallback legado, y la UI.
- Consistencia entre **dibujo (lateral, planta, frontal)**, **BOM** y **persistencia**: la regla en un
  solo sitio (AGENTS.md, convencion 2).

## 4. Fuera de alcance

- **ID13** (frentes en blanco del Selectivo) y el resto de pendientes de esa lista.
- El tope **REAR** de Push Back y su vocabulario de lados, que tienen sede propia.
- El vocabulario de la BOTA del **Dinamico**, pendiente de decision del dueno.
- El marcador de conflicto huerfano que sobrevive en [ideas-futuras.md](../ideas-futuras.md) desde el
  merge `d582dee`: hallazgo ajeno, se reporta y no se arregla de paso.
- Cualquier cambio en las otras familias de seguridad (bota, protector, separador, parrilla, tarima).

## 5. Contexto requerido

- Context Packs: `system-selective`, `ui-editors`, `persistence`.
- [ideas-futuras.md](../ideas-futuras.md) — ID12 y su nota de SPLIT.
- [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md) — frontera
  pendiente/comprometido y autoridades por fondo del Selectivo.
- `AGENTS.md`, convenciones 2 (regla en un solo sitio) y 3 (copia centralizada de flags de seguridad).
- Codigo: `SelectiveSafetyPlacement.TopeSpots`, `SelectiveTopePlan` (`Build` y `BuildFrontal`),
  `SelectiveTopePlacement`, `SelectiveLateralBuilder.AddTopes`, `SelectivePlantaBuilder.AddTopes`,
  `SelectiveFrontalBuilder`, `SelectiveBomBuilder.AddTopeComponents`, `SelectiveTopeConfig`,
  `TopeSelectionDocument`, `SafetyTopeGridWindow`, `SelectiveSafetyWindow`.

## 6. Dependencias

I-22 (colocacion de seguridad por familia) e I-43 (alcance y fondos del Selectivo), ambas integradas.
Ninguna iniciativa en curso: al reclamar, `origin` solo tenia `main`.

**No queda ninguna decision del dueno pendiente para esta iniciativa.** El contrato de lados quedo
fijado en el Gate 3 (cabecera de este documento) y `requires_owner_decision` bajo a `false` por eso.
Lo que sigue vivo es `requires_owner_validation`: el arreglo mueve piezas dibujadas y por tanto exige
la validacion manual del dueno en AutoCAD, que es otra clase de evidencia y no la sustituye ninguna
prueba (AGENTS.md, «Reutilizacion de evidencia»).

## 7. Archivos esperados

| Area | Archivos |
|---|---|
| Reproduccion (contrato, ROJA) | `tests/RackCad.Tests/SelectiveTopeSideTests.cs` |
| Caracterizacion multi-fondo (VERDE) | `tests/RackCad.Tests/SelectiveTopeMultiFondoCharacterizationTests.cs` |
| Modelo/colocacion | `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs`, `SelectiveTopePlan.cs` |
| Dominio | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` (`SelectiveTopeConfig`) |
| Persistencia | `src/RackCad.Application/Persistence/SafetySelectionDocuments.cs`, `SelectivePalletDesignDocument.cs` |
| UI | `src/RackCad.UI/SafetyTopeGridWindow.cs`, `src/RackCad.UI/SelectiveSafetyWindow.cs` |
| Documentacion | este contrato, `docs/ROADMAP.md`, `docs/ideas-futuras.md` (retirar ID12 al cerrar) |

`SelectivePalletDesign.cs` es **archivo caliente** (WORKFLOW seccion 7). Una desviacion material sobre
esta tabla obliga a detenerse.

## 8. Fases

1. **Apertura y reproduccion** *(hecha)* — reclamo atomico, bootstrap documental y prueba ROJA sobre
   el resultado fisico con un fondo.
2. **Gate 3: contrato y caracterizacion** *(hecha)* — contrato de lados fijado por el dueno; matriz
   actual medida sobre `TopeShared` x `Side` x `fondoCount` 1/2/3 con `TopeFondo` automatico y
   explicito; caracterizacion VERDE del multi-fondo **antes** de tocarlo; pruebas de contrato ROJAS
   ampliadas al modo compartido, a la identidad BAJO/ALTO, al plan y al acuerdo frontal-BOM.
   **Sin cambio de produccion.**
3. **Modelo y colocacion** — la regla unica de extremos en `TopeSpots` y el plan como autoridad comun.
4. **Dibujo, BOM y UI** — frontal alineado con el plan y `Ninguno` elegible sin reescritura.
5. **Cierre** — dos suites verdes en local, CI verde sobre el SHA exigido, build Debug de UI y Plugin,
   y validacion manual del dueno en AutoCAD.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj                  # suite Core
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj            # suite UI
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug        # exige AutoCAD 2025 CERRADO
```

Bugfix: la prueba de regresion se verifica **fallando sin el arreglo** (AGENTS.md). La fase 1 la deja
fallando a proposito, y ese fallo es su evidencia.

## 10. Validacion manual

**Requerida**: la iniciativa cambia colocacion y por tanto dibujo. La fase 1 **no** la necesita —no
toca produccion—. El checklist del dueno lo fija la fase 4 y sigue
[guias/validacion-manual-autocad.md](../guias/validacion-manual-autocad.md), con el DLL construido
**dentro del worktree de esta rama**.

## 11. Criterios de aceptacion

1. En un rack de **un fondo**, el resultado fisico del tope da `Ninguno` = 0 piezas, `Izquierda` = 1
   en el extremo **BAJO** del eje local, `Derecha` = 1 en el extremo **ALTO**, y `Ambas` = 2; y
   `TopeShared` **no altera** ninguno de esos cuatro resultados, porque con un solo fondo no hay nada
   que compartir.
2. Lateral, planta, frontal y BOM coinciden con esa resolucion, sin repetir la aritmetica.
3. Un documento anterior se lee sin cambiar de significado, con fallback legado explicito y prueba de
   round-trip.
4. El dialogo ofrece las cuatro opciones y ninguna se reescribe en silencio al cargar.
5. Las dos suites verdes y la validacion del dueno APROBADA.

## 12. Condiciones para detenerse

- El arreglo obligaria a cambiar `SchemaVersion`, a anadir un campo al DTO o a romper la lectura de
  documentos anteriores: el Gate 3 concluyo que **nada de eso hace falta**, asi que necesitarlo
  significa que el modelo propuesto era erroneo.
- Alguna celda de la matriz de caracterizacion multi-fondo se mueve sin estar declarada como movida.
- Aparece la necesidad de tocar el tope REAR de Push Back o la BOTA del Dinamico.
- La desviacion sobre la tabla de la seccion 7 deja de ser cosmetica.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-46.yml` y sin Pull Request: I-40 a I-45 tampoco los usaron, y el registro
vivo de esta iniciativa es su rama en el remoto (`origin/fix/selectivo-topes-izquierda-derecha`).
Prohibido el merge automatico. `main` no se toca.

## 14. Evidencia final

Se completa al cerrar. Hasta aqui: reclamo atomico aceptado por el remoto sin force, bootstrap
documental versionado antes de todo trabajo sustantivo, y la prueba de reproduccion ROJA con su
medicion del comportamiento de hoy.

## 15. Gate 3 — auditoria: matriz actual, matriz propuesta y diff minimo

Medido sobre `PalletDepth = 48`, un frente y un nivel. Las posiciones son la X del mate del tope en el
eje de profundidad **local** (`Offsets` pone el poste frontal en 0): postes `f0[0, 42]`, `f1[54, 96]`,
`f2[108, 150]`, y el `TROQUEL_TOPE` mata 0.875" dentro de cada uno — un poste TRASERO mata en
`back - 0.875` y uno DELANTERO en `front + 0.875`.

`c` = `TopeFondo` si es valido, si no `CentralFondo(fondoCount) = (fondoCount - 1) / 2`.

### 15.1 Regla propuesta (una sola, tres lineas de intencion)

El tope tiene un **vano de referencia** y `Side` elige sus **extremos**, ordenados sobre el eje local:

- si **hay fondo siguiente** (`c + 1 < fondoCount`), el vano es el **hueco central**:
  `BAJO` = trasero de `c`, `ALTO` = delantero de `c + 1` (espejado);
- si **no lo hay** (`c` es el ultimo fondo, e incluye `fondoCount == 1`), el vano **degenera** al propio
  fondo: `BAJO` = delantero de `c` (espejado), `ALTO` = trasero de `c`.

`Ninguno` = {} · `Izquierda` = {BAJO} · `Derecha` = {ALTO} · `Ambas` = {BAJO, ALTO}.

`TopeShared` conserva su significado —**una barra compartida** frente a una por fondo— y por eso sigue
colapsando el par a UNA pieza **cuando hay hueco**: `Derecha` la monta en el ALTO y cualquier otro lado
en el BAJO, de modo que `Ambas` compartida sigue siendo **una** pieza. Sin hueco no hay nada que
compartir y `TopeShared` es **inerte**.

### 15.2 Matriz — HOY frente a PROPUESTA

`=` significa que la celda **no se mueve**. Las celdas marcadas **CAMBIA** son las unicas que el
arreglo desplaza, y todas ellas son defectos declarados de ID12.

| fondos | shared | `TopeFondo` | Lado | HOY | PROPUESTA | |
|---|---|---|---|---|---|---|
| 1 | no | auto/0 | Ninguno | — | — | = |
| 1 | no | auto/0 | Izquierda | 41.125 | 0.875 | **CAMBIA** |
| 1 | no | auto/0 | Derecha | — | 41.125 | **CAMBIA** |
| 1 | no | auto/0 | Ambas | 41.125 | 0.875 + 41.125 | **CAMBIA** |
| 1 | si | auto/0 | Ninguno | — (plan: 1 spot rancio) | — (plan vacio) | **CAMBIA** (solo el plan) |
| 1 | si | auto/0 | Izquierda | 41.125 | 0.875 | **CAMBIA** |
| 1 | si | auto/0 | Derecha | 41.125 | 41.125 | = |
| 1 | si | auto/0 | Ambas | 41.125 | 0.875 + 41.125 | **CAMBIA** |
| 2 | no | auto/0 | Ninguno / Izq / Der / Ambas | — / 41.125 / 54.875 / ambas | idem | = |
| 2 | no | 1 (ultimo) | Izquierda | 95.125 | 54.875 | **CAMBIA** |
| 2 | no | 1 (ultimo) | Derecha | — | 95.125 | **CAMBIA** |
| 2 | no | 1 (ultimo) | Ambas | 95.125 | 54.875 + 95.125 | **CAMBIA** |
| 2 | si | auto/0 | Izquierda / Ambas | 41.125 | 41.125 | = |
| 2 | si | auto/0 | Derecha | 41.125 | 54.875 | **CAMBIA** |
| 2 | si | 1 (ultimo) | Izquierda | 95.125 | 54.875 | **CAMBIA** |
| 2 | si | 1 (ultimo) | Derecha | 95.125 | 95.125 | = |
| 2 | si | 1 (ultimo) | Ambas | 95.125 | 54.875 + 95.125 | **CAMBIA** |
| 3 | no | auto/1 | Izq / Der / Ambas | 95.125 / 108.875 / ambas | idem | = |
| 3 | no | 0 | Izq / Der / Ambas | 41.125 / 54.875 / ambas | idem | = |
| 3 | no | 2 (ultimo) | Izq / Der / Ambas | 149.125 / — / 149.125 | 108.875 / 149.125 / ambas | **CAMBIA** |
| 3 | si | auto/1, 0 | Derecha | el BAJO | el ALTO | **CAMBIA** |
| 3 | si | auto/1, 0 | Izquierda / Ambas | el BAJO | el BAJO | = |
| 3 | si | 2 (ultimo) | como el caso de 2 fondos con `TopeFondo` ultimo | | | **CAMBIA** |

**Lectura corta**: se mueve exactamente lo roto — todo `TopeFondo` que apunta al **ultimo** fondo
(incluido el unico fondo de un rack sencillo), `Derecha` en modo compartido, y el spot rancio del plan
con `Ninguno`. **Todo lo demas queda byte a byte igual**, y eso es lo que congela la caracterizacion.

### 15.3 Compatibilidad legado

`Side` **puede seguir siendo la autoridad persistida** y no hace falta campo nuevo:

- el tope tiene su **propia** `SelectiveSafetySelection` (una por familia, resuelta por
  `SelectiveSafetyFamilies.SelectedOfType`), asi que su `Side` no lo comparte con la bota;
- el DTO ya guarda `Side` como `int?` y `SafetyDocumentMapping.ToSafetySide` acepta **0 = None** en
  rango. `Ninguno` **ya viaja hoy**: lo que lo destruye es la UI, no la persistencia.

Lo que **si** cambia de significado en disco, y hay que declararlo:

- `ToSafetySide(null)` cae en `Both`. Un documento de un solo fondo con `Ambas` —que es el **valor por
  defecto**— pasa de **una** pieza a **dos**. Es exactamente lo que pide el contrato («Ambas = union
  exacta»), pero es un cambio visible en dibujos existentes al reeditarlos.
- Un documento **multi-fondo con el fondo automatico** y los valores por defecto (`Ambas`,
  `Compartido`) **no cambia**: sigue dando una pieza en la misma X.

### 15.4 Diff minimo de produccion (propuesto, NO implementado)

| Archivo | Cambio | Tamano |
|---|---|---|
| `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs` | Reescribir `TopeSpots` con la regla de 15.1: salida vacia con `Ninguno`, extremos BAJO/ALTO y degeneracion cuando no hay fondo siguiente | ~15 lineas, sustituyen a ~20 |
| `src/RackCad.Application/Systems/Selective/SelectiveTopePlan.cs` | `BuildFrontal` devuelve vacio cuando el conjunto de spots lo esta, consultando **el mismo** `TopeSpots`. **No** itera los spots: seguiria duplicando el par por fondo | ~4 lineas |
| `src/RackCad.UI/SafetyTopeGridWindow.cs` | `Ninguno` como cuarta opcion en `SideLabels` + su ordinal en `SideIndex`/`SideFromIndex` | ~4 lineas |
| `src/RackCad.UI/SelectiveSafetyWindow.cs` | Retirar la coercion `existing.Side == None ? Both : existing.Side` | 1 linea |

**No** se tocan: el Dominio, `SelectiveTopeConfig`, ningun DTO, `SchemaVersion`, `DeepCopy`, la
aritmetica de colocacion de los tres builders ni `SelectiveBomBuilder`. Como no nace ningun campo, la
**convencion 3 de AGENTS.md no se activa**.

### 15.5 El plan como autoridad comun

`SelectiveTopePlan` ya es la autoridad de **lateral, planta y BOM**; el frontal es el unico que se salta
los spots, y por eso hoy dibuja una pieza que el BOM no cobra. La union correcta **no** es proyectar los
spots uno a uno —eso duplicaria el par por fondo, que es justo lo que I-22 evito—, sino que ambas
salidas partan del **mismo conjunto resuelto**:

- `Build` -> las piezas **fisicas**, una por spot (lateral, planta y BOM las cuentan);
- `BuildFrontal` -> la **proyeccion** en alzado, que dibuja **una vez** por celda si el conjunto no esta
  vacio, porque en el alzado todos los spots caen sobre la misma posicion.

Con eso `Ninguno` deja de depender de que `EnabledOfType`/`DrawsSomewhere()` lo tape aguas abajo: el
gate pasa a estar **en el plan**, que es donde el resto de las vistas ya lo leen.
