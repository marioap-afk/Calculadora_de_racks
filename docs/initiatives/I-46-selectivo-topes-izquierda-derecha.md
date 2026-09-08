---
schema: rackcad-initiative/v1
id: I-46
title: "Selectivo: BUG topes de tarima Izquierda/Derecha"
type: fix
status: integrated
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

# Selectivo: BUG topes de tarima Izquierda/Derecha (ID12)

> **INTEGRADA y CERRADA el 2026-09-08.** El `MERGE_SHA`, el CI posterior al merge y la limpieza **todavia no existen**: se registran cuando el merge los produzca (seccion 14.7). Esta iniciativa se abrio por autorizacion explicita del
> dueno (caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2): ID12 vivia en
> [ideas-futuras.md](../ideas-futuras.md) como pendiente conocido del Selectivo, sin fila propia. El
> reclamo atomico se hizo antes que este bootstrap y la fila en [ROADMAP.md](../ROADMAP.md) se crea
> con el, que es el orden que manda el proceso.
>
> El arreglo esta implementado y validado. Las pruebas de contrato que nacieron ROJAS estan VERDES, la
> caracterizacion multi-fondo sigue VERDE con sus doce filas movidas declaradas una a una, y el dueno
> aprobo la validacion manual en AutoCAD 2025 **PASS TOTAL 7/7** sobre el candidato exacto. La
> evidencia completa esta en la seccion 14.
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
> - Con **hueco real** (`c + 1` existe) y `TopeShared = true`, hay **UNA pieza compartida en LOW** y
>   `Side` queda **dormante**: comportamiento **historico que NO cambia**.
>
> La forma ejecutable y completa de esta regla —las nueve clausulas— vive en la **seccion 15.1**, que
> es la que manda sobre cualquier parafrasis de este documento.

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
   `TopeSpots` devuelve una posicion **antes de mirar el lado** cuando `TopeShared` es cierto, y eso
   con `Ninguno` es un defecto real del plan. **Ojo:** que el modo compartido ignore el lado
   **cuando hay hueco** NO es defecto — es el tope compartido historico y la regla lo preserva
   (seccion 15.1, clausula 3).
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
| Contrato multi-fondo (ROJA + centinela VERDE) | `tests/RackCad.Tests/SelectiveTopeSideMultiFondoContractTests.cs` |
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

### 14.1 Cadena de SHAs

| Hito | SHA |
|---|---|
| Base (`origin/main` al reclamar, y sigue ahi) | `e85c588757433592ba05d1533049fe0431dcb808` |
| Reclamo atomico (vacio, `Claim-Id` 7a5bc03c-fd8f-440f-91ca-7b15507614be) | `3fcfc9b13dc15bf68b097c62749453398137200b` |
| Reproduccion ROJA | `0fa0734da16018ba32610405014bc0cf668f2168` |
| Codigo del arreglo | `183f8656646a9d3ca2a850b6067a6eb2f4fa0952` |
| Cobertura de persistencia, editor y aislamiento | `474b6463f8b3107fb06d6f3b771fce37ba595b51` |
| Seam `TopeDialog` y cadena real de `EditTope` | `1310b8b7910ec8287db7b34a41a9a0887d1f4c78` |
| **CANDIDATO final** | **`259aa2e08a1a08c2451cb873c3f634d0fde9b2e6`** |

`origin/main` **no avanzo** en toda la vida de la rama, asi que **no hubo rebase final**: la evidencia
recae sobre el contenido que se integrara, sin SHAs intermedios que ya no existan.

### 14.2 Causa raiz

**Resolver / materializacion geometrica**, con la UI como causa **secundaria**. `TopeSpots` no leia
`Side` como un lado: lo leia como **que fondo del par central** lleva la pieza —`Left` = poste trasero
del fondo `c`, `Right` = poste delantero de `c + 1` **solo si `c + 1 < fondoCount`**—, que es un eje de
**profundidad**. La UI agravaba el defecto sin causarlo: el dialogo ofrecia **tres** opciones y
`SelectiveSafetyWindow` reescribia un `None` guardado a `Both` al reabrir la fila.

**RED inicial, con su causa observable.** En un rack de UN fondo, `c + 1 < fondoCount` es falso, asi que
`Derecha` no colocaba nada y `Ambas` colocaba la mitad. Medido sobre `0fa0734`: `Ninguno` 0 (correcto),
`Izquierda` 1 (correcto), **`Derecha` 0 donde se pide 1**, **`Ambas` 1 donde se piden 2**; suite Core
4652 con **5 fallos, todos de `SelectiveTopeSideTests`**. Ampliado en el Gate 3 a **26 fallos** de
contrato al cubrir el modo compartido, la identidad BAJO/ALTO, el plan y el acuerdo frontal-BOM.

### 14.3 Contrato final materializado

- `Izquierda` = extremo **BAJO** y `Derecha` = extremo **ALTO**, leidos **siempre** sobre el eje de
  profundidad **LOCAL** (`SelectiveDepthLayout.Offsets`, poste frontal en 0). **Nunca World X.**
- `TopeShared` **historico y PRESERVADO** cuando existe `c + 1`: UNA pieza compartida en LOW y el lado
  **dormante**. Verificado byte a byte —mismo spot, mismo BOM, misma X lateral, mismo frontal y planta—.
- `TopeShared` **inerte** cuando NO existe fondo siguiente: sin hueco no hay nada que compartir, y los
  dos modos son indistinguibles.
- `Ninguno` = **cero spots desde `SelectiveTopePlan`**, no solo tapado aguas abajo por `EnabledOfType`.
- **Frontal = UNA proyeccion por celda, nunca por spot**: es un alzado y los spots caen en el mismo
  sitio; multiplicarlos duplicaria el par por fondo (lo que I-22 evito).

### 14.4 Persistencia

- **`persistence ordinal changed` = NO.** `SafetySide` conserva `None = 0`, `Left = 1`, `Right = 2`,
  `Both = 3`; **ningun archivo de Domain** entra en el diff.
- **`SchemaVersion` = `1.0`**, sin tocar; **ningun archivo de Persistence** entra en el diff.
- **`DTO/schema migration` = NO.** No nace campo alguno: `Side` ya viajaba como `int?` y
  `ToSafetySide` ya aceptaba `0 = None`. Quien destruia el valor era el editor, no el store.
- **Legado `Side` nulo -> `Both` PRESERVADO**, y un valor fuera de rango sigue cayendo en `Both`, no en
  el `None` recien alcanzable.
- **`RACKEDITAR` probado**: cargar los cuatro lados y aceptar sin editar los conserva (1 y 3 fondos);
  guardar y reabrir tambien; y la cadena real —boton de la fila, dialogo real, `EditTope`— propaga
  `Izquierda->Derecha`, `Derecha->Ambas` y `Ambas->Ninguno`, mientras cancelar deja el lado intacto.

### 14.5 Alcance tocado

Produccion, **cuatro archivos y ninguno mas**:

- `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs`
- `src/RackCad.Application/Systems/Selective/SelectiveTopePlan.cs`
- `src/RackCad.UI/SafetyTopeGridWindow.cs`
- `src/RackCad.UI/SelectiveSafetyWindow.cs`

**`Push Back impact` = NO** y **`Dinamico impact` = NO**: cero archivos de produccion de esos sistemas.
El aislamiento del Dinamico no se apoya en el dialogo —los tres editores comparten
`SelectiveSafetyWindow`— sino en que `RackDynamicSystemWindow.Safety_Click` le entrega una lista blanca
de bota, lateral, desviador, defensa y guia que **no incluye TOPE**, y eso queda con guarda propia.

### 14.6 Evidencia del candidato `259aa2e`

| Clase | Resultado |
|---|---|
| `RackCad.Tests` FULL local | **4763 / 4763**, 0 fallos, 0 omitidas |
| `RackCad.UI.Tests` FULL local | **1273 total: 1256 PASS, 17 omitidas**, 0 fallos |
| Build Debug `RackCad.UI` | **PASS** — 0 errores, 0 advertencias |
| Build Debug `RackCad.Plugin` | **PASS** — 0 errores, solo las **2 `MSB3277`** conocidas de AutoCAD |
| CI de `push` sobre el SHA exacto | corrida **34274626718**, **4/4 jobs `success`**, `headSha` = `259aa2e` |
| Validacion manual del dueno, AutoCAD 2025 | **PASS TOTAL 7/7** sobre `259aa2e` |

### 14.7 Pendientes

**Ninguno funcional de I-46.** Lo que queda son las compuertas POSTERIORES de WORKFLOW §4.5 pasos 6 y 7,
y **ninguna de ellas existe todavia**, asi que este documento no registra sus valores:

- el **`MERGE_SHA`** del merge `--no-ff`, que se anota cuando el merge lo produzca;
- el **CI posterior al merge** sobre ese SHA, con su artifact `rackcad-coverage-cobertura`;
- la **comprobacion diferida de la cobertura del Candidato**;
- la **limpieza** de rama y worktree, que ambas compuertas bloquean hasta pasar.

`integrada` significa que el merge existe en `main`, **no** que la integracion este verificada: mientras
el CI del `MERGE_SHA` no este verde, la integracion sigue sin verificar y la correccion se hace en esta
rama, que por eso no se ha borrado.

## 15. Gate 3 — auditoria: matriz actual, matriz propuesta y diff minimo

Medido sobre `PalletDepth = 48`, un frente y un nivel. Las posiciones son la X del mate del tope en el
eje de profundidad **local** (`Offsets` pone el poste frontal en 0): postes `f0[0, 42]`, `f1[54, 96]`,
`f2[108, 150]`, y el `TROQUEL_TOPE` mata 0.875" dentro de cada uno — un poste TRASERO mata en
`back - 0.875` y uno DELANTERO en `front + 0.875`.

`c` = `TopeFondo` si es valido, si no `CentralFondo(fondoCount) = (fondoCount - 1) / 2`.

### 15.1 Regla VINCULANTE (fijada por el dueno; no se re-decide)

1. Resolver `c` = `TopeFondo` valido, o `CentralFondo(fondoCount) = (fondoCount - 1) / 2`.
2. `Side = None` -> **cero spots, siempre**, en los dos modos y en cualquier fondo.
3. Si **`c + 1` existe**, el vano de referencia es el **hueco central**: `LOW = back(c)`,
   `HIGH = front(c + 1)`.
   - `TopeShared = false`: `Izquierda` = LOW, `Derecha` = HIGH, `Ambas` = LOW + HIGH.
   - `TopeShared = true`: **UNA pieza compartida en LOW** para cualquier `Side` no-`None`. `Side` queda
     **dormante / no aplicable** en ese estado. **Esto es comportamiento historico y NO cambia.**
4. Si **`c + 1` NO existe**, el vano **degenera al propio fondo**: `LOW = front(c)`, `HIGH = back(c)`.
   `TopeShared` es **inerte**: `Izquierda` = LOW, `Derecha` = HIGH, `Ambas` = LOW + HIGH.
5. **Nunca World X**: bajo/alto se leen sobre el eje de profundidad local.
6. `SafetySide` conserva `None = 0`, `Left = 1`, `Right = 2`, `Both = 3`.
7. `Build` devuelve **cero spots** para `None` (el gate vive en el plan, no aguas abajo).
8. `BuildFrontal`: vacio si el conjunto fisico esta vacio; si no, **una sola** proyeccion esquematica por
   celda, **sin multiplicar por spots**.
9. Sin DTO nuevo, sin `SchemaVersion`, sin campo `Side` nuevo.

> **Correccion respecto de la primera redaccion de este gate.** Yo habia marcado
> `TopeShared = true` + `Derecha` **con hueco real** como una celda a arreglar. **Es falso**: con
> `c + 1` existente esa celda es el tope compartido historico, `Side` esta dormante por diseno y la
> regla la **preserva**. El unico eje que se mueve es el de la clausula 4 —`c` sin fondo siguiente—,
> que incluye **todo rack de un solo fondo**.

### 15.2 Matriz FINAL congelada — HOY frente a la REGLA

`=` significa que la celda **no se mueve**. Posiciones en X del mate sobre el eje local: postes
`f0[0, 42]`, `f1[54, 96]`, `f2[108, 150]`; el `TROQUEL_TOPE` mata 0.875" dentro de cada uno, asi que un
poste TRASERO mata en `back - 0.875` y uno DELANTERO en `front + 0.875`.

**Clausula 3 — hay hueco (`c + 1` existe). Nada se mueve salvo el spot rancio del plan.**

| fondos | `TopeFondo` | `c` | shared | Ninguno | Izquierda | Derecha | Ambas | |
|---|---|---|---|---|---|---|---|---|
| 2 | auto / 0 | 0 | no | — | 41.125 | 54.875 | 41.125 + 54.875 | = |
| 2 | auto / 0 | 0 | **si** | — | 41.125 | **41.125** | 41.125 | = *(historico, `Side` dormante)* |
| 3 | auto / 1 | 1 | no | — | 95.125 | 108.875 | 95.125 + 108.875 | = |
| 3 | auto / 1 | 1 | **si** | — | 95.125 | **95.125** | 95.125 | = *(historico)* |
| 3 | 0 | 0 | no | — | 41.125 | 54.875 | 41.125 + 54.875 | = |
| 3 | 0 | 0 | **si** | — | 41.125 | **41.125** | 41.125 | = *(historico)* |

Unica diferencia en este bloque: hoy, con `Ninguno` y `TopeShared = true`, `SelectiveTopePlan.Build`
devuelve **un spot rancio** que solo tapa `EnabledOfType` aguas abajo. La clausula 7 lo elimina **en el
plan**; el dibujo y el BOM ya daban cero, asi que **no cambia ni una pieza dibujada**.

**Clausula 4 — no hay hueco (`c` es el ultimo fondo, incluido todo rack de 1 fondo).**

| fondos | `TopeFondo` | shared | Lado | HOY | REGLA | |
|---|---|---|---|---|---|---|
| 1 | auto / 0 | no | Izquierda | 41.125 | **0.875** | CAMBIA |
| 1 | auto / 0 | no | Derecha | **—** | **41.125** | CAMBIA |
| 1 | auto / 0 | no | Ambas | 41.125 | **0.875 + 41.125** | CAMBIA |
| 1 | auto / 0 | si | Izquierda | 41.125 | **0.875** | CAMBIA *(inerte)* |
| 1 | auto / 0 | si | Derecha | 41.125 | 41.125 | = |
| 1 | auto / 0 | si | Ambas | 41.125 | **0.875 + 41.125** | CAMBIA *(inerte)* |
| 2 | 1 | no | Izquierda | 95.125 | **54.875** | CAMBIA |
| 2 | 1 | no | Derecha | **—** | **95.125** | CAMBIA |
| 2 | 1 | no | Ambas | 95.125 | **54.875 + 95.125** | CAMBIA |
| 2 | 1 | si | Izquierda | 95.125 | **54.875** | CAMBIA *(inerte)* |
| 2 | 1 | si | Derecha | 95.125 | 95.125 | = |
| 2 | 1 | si | Ambas | 95.125 | **54.875 + 95.125** | CAMBIA *(inerte)* |
| 3 | 2 | no | Izquierda | 149.125 | **108.875** | CAMBIA |
| 3 | 2 | no | Derecha | **—** | **149.125** | CAMBIA |
| 3 | 2 | no | Ambas | 149.125 | **108.875 + 149.125** | CAMBIA |
| 3 | 2 | si | Izquierda | 149.125 | **108.875** | CAMBIA *(inerte)* |
| 3 | 2 | si | Derecha | 149.125 | 149.125 | = |
| 3 | 2 | si | Ambas | 149.125 | **108.875 + 149.125** | CAMBIA *(inerte)* |

**Lectura corta.** Se mueve **una sola familia de celdas**: la del fondo elegido **sin fondo siguiente**.
El tope compartido con hueco real, que es el caso por defecto de un doble profundidad, **no se toca**.

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
| `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs` | Reescribir `TopeSpots` con las clausulas 1-4 y 7 de 15.1: salida vacia con `Ninguno`; con hueco, la rama compartida **se conserva tal cual** (una pieza en LOW, `Side` dormante) y la de por-fondo elige LOW/HIGH; sin hueco, el vano degenera al propio fondo y `TopeShared` deja de consultarse | ~18 lineas, sustituyen a ~20 |
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
