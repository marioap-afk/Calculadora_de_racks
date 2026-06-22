# Generación de cabecera lateral (block-based)

Esta es la lógica nueva para generar una **cabecera en vista lateral** a partir de **bloques
independientes anclados a puntos de conexión** (no una composición visual libre). El **poste es la base
geométrica**; horizontales y diagonales de celosía cuelgan de la línea de troqueles del poste.

> **Handoff:** la **lógica pura ya está implementada y probada** en Linux. El **paso 2 (dibujo en
> AutoCAD)** lo debe hacer Claude local en Windows, porque el proyecto `RackCad.Plugin` solo compila con
> las DLLs de AutoCAD. Ver la sección [Paso 2](#paso-2-lo-que-falta-en-autocad-claude-local).

## Arquitectura (separación pura ↔ AutoCAD)

```
RackCad.Application/Headers/            (PURO, testeable en cualquier SO)
  LateralHeaderParameters     parámetros editables (sin números mágicos)
  HeaderConnectionGeometry    coords locales de los puntos + nombres de bloque
  HeaderBlockInstance         una inserción del plan (qué, dónde, params dinámicos)
  LateralHeaderLayout         el plan completo + totales
  LateralHeaderLayoutBuilder  la lógica (7 pasos)
  HeaderGeometryResolver      catálogo (connection-layout + blocks) → geometría

RackCad.Plugin/Headers/                (AutoCAD, solo Windows)
  LateralHeaderDrawer         adapter: ejecuta el plan (InsertBlock + params dinámicos)
```

Regla de oro: **toda la geometría y los cálculos son puros**; el drawer solo traduce el plan a la API de
AutoCAD. Así la lógica se prueba sin AutoCAD y el drawer queda mínimo.

## Parámetros (`LateralHeaderParameters`)

| Parámetro | Default | Significado | ¿Dónde se edita? |
|---|---|---|---|
| `Height` | 132 | Altura; mueve el parámetro dinámico `LONGITUD` del poste | Editor **clásico** |
| `Depth` | 42 | Fondo: separación entre los dos postes | Editor **clásico** |
| `PasoTroquel` | 2 | Paso entre troqueles del poste | (fijo) |
| `InicioCelosiaTroquel` | 3 | Troquel (1-based) donde va la primera horizontal → `(3-1)*2 = 4"` | Editor **avanzado** |
| `ClaroPanel` | 44 | Claro vertical entre horizontales | Editor **avanzado** (segmentos) |
| `OffsetDiagonalInicioTroqueles` | 2 | La diagonal arranca N troqueles arriba de la horizontal inferior | Editor **avanzado** |
| `OffsetDiagonalFinTroqueles` | 2 | La diagonal termina N troqueles abajo de la horizontal superior | Editor **avanzado** |
| `ValorClaroTravesano` | auto | Claro sobrante arriba; si `>0` se agrega una horizontal de cierre | auto / opcional |

En el editor, estos viven en `RackFrameConfiguration` (`Height`, `Depth`, `CelosiaStartTroquel`,
`DiagonalStartOffsetTroqueles`, `DiagonalEndOffsetTroqueles`) y persisten en el proyecto.

## Puntos de conexión (la lógica se ancla en estos)

| Punto | Vive en | Rol |
|---|---|---|
| `MONTAJE_POSTE` | placa base | El `(0,0)` del poste coincide aquí |
| `TROQUEL_CELOSIA` | poste | Primer troquel = línea de referencia de la celosía |
| `CELOSIA` | travesaño (celosía) | El punto del travesaño que cae sobre la línea de troqueles |

Sus posiciones 2D por vista están en `connection-layout.csv` y los nombres de bloque por vista en
`blocks.csv` (ver [modelo-de-datos.md](modelo-de-datos.md)).

## Algoritmo (`LateralHeaderLayoutBuilder.Build`)

Ejes en vista lateral: **X = fondo (entre postes), Y = altura**.

1. **Poste izquierdo + placa** en X=0: la placa se inserta primero, el poste con su `(0,0)` sobre
   `MONTAJE_POSTE`; se fija `LONGITUD = Height`.
2. **Poste derecho espejeado** con origen en `X = Depth`.
3. **Línea de troqueles** desde `TROQUEL_CELOSIA` (derecho espejeado):
   `LongitudHorizontal = X_troquel_dcho − X_troquel_izq`.
4. **Y de horizontales**: `yFirst = Y_troquel0 + (InicioCelosiaTroquel−1)·PasoTroquel`; luego cada
   `ClaroPanel`. Nº de paneles = `floor((Height − yFirst) / ClaroPanel)`.
5. **Horizontales**: el punto `CELOSIA` cae sobre la línea de troquel a su Y; largo = `LongitudHorizontal`.
6. **Diagonales** (una por panel): arranca `OffsetInicio` troqueles arriba de la horizontal inferior y
   termina `OffsetFin` troqueles abajo de la superior; **largo y ángulo se calculan de los puntos reales**.
7. **Horizontal de cierre**: si sobra claro arriba (`ValorClaroTravesano > 0`), se agrega una horizontal en
   `Y = Height`.

Ejemplo (Height 132, Depth 42, paso 2, inicio 3, claro 44, troquel local X=2):
horizontales en Y = 4 / 48 / 92 (largo 38), cierre en Y = 132 (sobra 40"), 2 diagonales.

## El plan: `HeaderBlockInstance`

Cada inserción trae: `Role` (BasePlate/Post/Horizontal/Diagonal/ClosingHorizontal), `BlockName`, `View`,
`Insertion` (origen del bloque), `ConnectionAnchor` (dónde cae su punto de referencia), `RotationRadians`,
`MirroredX`, y `DynamicParameters` (p. ej. `LONGITUD`, `Distancia1`).

## Paso 2: lo que falta (en AutoCAD, Claude local)

El drawer ya existe (`RackCad.Plugin/Headers/LateralHeaderDrawer.cs`). Falta **cablearlo**:

1. **Comando del Plugin** (p. ej. `RACKCABECERALATERAL`) que, dentro de una transacción:
   - obtenga el `RackCatalog` (`JsonRackCatalogProvider.FromBaseDirectory().Load()`) y la
     `RackFrameConfiguration` actual;
   - arme `LateralHeaderParameters` desde la config:
     `Height`, `Depth`, `InicioCelosiaTroquel = config.CelosiaStartTroquel`,
     `OffsetDiagonalInicioTroqueles = config.DiagonalStartOffsetTroqueles`,
     `OffsetDiagonalFinTroqueles = config.DiagonalEndOffsetTroqueles`,
     `ClaroPanel` (del claro estándar), y los ids reales (`PostId`/`BasePlateId`/`TrussProfileId`);
   - llame `new LateralHeaderDrawer().BuildAndDraw(db, tr, espacioModelo, catalog, parameters)`.
2. **Requisitos de los bloques de AutoCAD** (datos a completar):
   - el bloque del **poste** necesita el parámetro dinámico **`LONGITUD`**;
   - los bloques de **travesaño** necesitan **`Distancia1`** (largo);
   - cada bloque debe tener definidos los **puntos de conexión** correspondientes y, en
     `connection-layout.csv`, su posición **en la vista `LATERAL`**:
     - placa → `MONTAJE_POSTE` (ya está)
     - poste → `TROQUEL_CELOSIA` (ya está)
     - travesaño → **`CELOSIA` (falta agregarlo en `connection-layout.csv` para el id del travesaño)**
3. **Diálogo de parámetros** (opcional pero recomendado): exponer Height/Depth (clásico) e
   InicioCelosia/Claro/offsets (avanzado) — ya están en el editor; el comando puede tomarlos de ahí.
4. **Prueba visual en AutoCAD**: insertar, ver que el poste estira con `LONGITUD`, que las horizontales
   caen en la línea de troquel y abarcan poste a poste, y que las diagonales quedan con el ángulo correcto.

## Supuestos de geometría a verificar (pendiente del usuario)

1. Eje de espejo del poste derecho: vertical en el fondo ⇒ `LongitudHorizontal = Depth − 2·inset_troquel`.
2. Nº de paneles: `floor((Height − yFirst)/ClaroPanel)`; el sobrante arriba es `ValorClaroTravesaño` (auto).
3. Una diagonal por panel (zigzag estándar); si se requiere alternancia/X, se parametriza.

## Dónde está en el código

- Lógica pura: `src/RackCad.Application/Headers/`.
- Adapter AutoCAD: `src/RackCad.Plugin/Headers/LateralHeaderDrawer.cs`.
- Tests (9): `tests/RackCad.Tests/LateralHeaderLayoutBuilderTests.cs`.
- Parámetros en el editor: `RackFrameConfiguration` + `RackFrameConfiguratorViewModel` + el panel
  "Header" del editor avanzado en `RackFrameConfiguratorWindow.xaml`.
