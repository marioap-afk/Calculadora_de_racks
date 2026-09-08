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
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# Selectivo — tope de tarima por lado (ID12)

> **Fase actual: APERTURA Y REPRODUCCION.** Esta iniciativa se abrio por autorizacion explicita del
> dueno (caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2): ID12 vivia en
> [ideas-futuras.md](../ideas-futuras.md) como pendiente conocido del Selectivo, sin fila propia. El
> reclamo atomico se hizo antes que este bootstrap y la fila en [ROADMAP.md](../ROADMAP.md) se crea
> con el, que es el orden que manda el proceso.
>
> **Lo unico entregado hasta aqui es la reproduccion.** No hay cambio de produccion, y la prueba que
> acompana a este contrato esta ROJA a proposito: fija el comportamiento que el dueno pide y muestra
> cual es el de hoy.

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

La **decision del dueno** sobre el significado geometrico de «izquierda» y «derecha» en un rack de un
fondo es entrada obligatoria de la fase de modelo. La reproduccion no la necesita.

## 7. Archivos esperados

| Area | Archivos |
|---|---|
| Reproduccion | `tests/RackCad.Tests/SelectiveTopeSideTests.cs` (nuevo) |
| Modelo/colocacion | `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs`, `SelectiveTopePlan.cs` |
| Dominio | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` (`SelectiveTopeConfig`) |
| Persistencia | `src/RackCad.Application/Persistence/SafetySelectionDocuments.cs`, `SelectivePalletDesignDocument.cs` |
| UI | `src/RackCad.UI/SafetyTopeGridWindow.cs`, `src/RackCad.UI/SelectiveSafetyWindow.cs` |
| Documentacion | este contrato, `docs/ROADMAP.md`, `docs/ideas-futuras.md` (retirar ID12 al cerrar) |

`SelectivePalletDesign.cs` es **archivo caliente** (WORKFLOW seccion 7). Una desviacion material sobre
esta tabla obliga a detenerse.

## 8. Fases

1. **Apertura y reproduccion** *(esta sesion)* — reclamo atomico, bootstrap documental y prueba ROJA
   sobre el resultado fisico con un fondo. Evidencia: el SHA de reproduccion y la salida de la suite
   Core mostrando el fallo.
2. **Decision del dueno** — que son «izquierda» y «derecha» para el tope de un rack de un fondo, y que
   pasa con los documentos existentes cuyo `Side` heredado significaba «fondo del par central».
3. **Modelo y colocacion** — sede de la intencion, resolucion a posiciones fisicas y los cuatro limites
   de la convencion 3.
4. **Dibujo, BOM y persistencia** — las tres vistas y el BOM leyendo la MISMA resolucion, round-trip
   con fallback legado.
5. **Cierre** — dos suites verdes en local, CI verde sobre el SHA exigido, build Debug de UI y Plugin,
   validacion manual del dueno en AutoCAD y ADR si la fase 2 produce una decision de arquitectura.

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

1. En un rack de **un fondo**, el resultado fisico del tope da `Ninguno` = 0 piezas, `Izquierda` = 1,
   `Derecha` = 1 en la posicion **opuesta** a la de `Izquierda`, y `Ambas` = 2.
2. Lateral, planta, frontal y BOM coinciden con esa resolucion, sin repetir la aritmetica.
3. Un documento anterior se lee sin cambiar de significado, con fallback legado explicito y prueba de
   round-trip.
4. El dialogo ofrece las cuatro opciones y ninguna se reescribe en silencio al cargar.
5. Las dos suites verdes y la validacion del dueno APROBADA.

## 12. Condiciones para detenerse

- Falta la decision de la fase 2, o llega ambigua.
- El arreglo obligaria a cambiar `SchemaVersion` o a romper la lectura de documentos anteriores.
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
