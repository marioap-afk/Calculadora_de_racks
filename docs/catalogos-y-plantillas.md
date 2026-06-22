# Catalogos y plantillas (JSON)

Esta guia explica como modificar los catalogos y las plantillas de cabecera **sin programar**, editando archivos JSON.

## Donde estan los archivos

Fuente versionada (lo que se edita en el repositorio):

```
assets/catalogs/
  post-profiles.json            Perfiles de poste
  horizontal-profiles.json      Perfiles de horizontal
  diagonal-profiles.json        Perfiles de diagonal/celosia
  reinforcement-profiles.json   Perfiles de refuerzo de poste
  base-plates.json              Placas base
  connection-points.json        Puntos de conexion (troqueles)
  header-templates.json         Plantillas de cabecera (auto-descriptivas)
  defaults.json                 Receta estandar global (piezas/alto por defecto)
```

Al compilar, estos archivos se copian a una carpeta `catalogs/` junto al DLL del plugin. La aplicacion los lee al iniciarse.

### Como aplicar un cambio

- **Editando el repositorio**: cambia el JSON en `assets/catalogs/` y recompila.
- **En una instalacion ya desplegada** (sin Visual Studio): edita el JSON dentro de la carpeta `catalogs/` que esta junto a `RackCad.Plugin.dll`, **reinicia AutoCAD** y vuelve a ejecutar `RACKCABECERA`. No hace falta recompilar.

> Si un archivo falta, la aplicacion sigue funcionando (usa valores internos por defecto). Si un archivo tiene un error de JSON, la aplicacion lo avisa en la barra de estado en vez de aplicar los cambios.

## Reglas generales de JSON

- Cada archivo es una **lista** entre corchetes `[ ... ]` de objetos `{ ... }`.
- Las cadenas van entre comillas dobles `"asi"`.
- Los numeros van sin comillas: `132.0`, `42`.
- Separa los elementos con comas. El ultimo puede o no llevar coma final (ambos se aceptan).
- Respeta mayusculas/minusculas en los valores de lista fijos (por ejemplo el tipo de celosia).

---

## Plantillas de cabecera (`header-templates.json`)

Una plantilla es **auto-descriptiva**: define cuantas horizontales hay, a que altura, **con que perfil y cuantas**, mas que perfil de diagonal, puntos de conexion, placa y poste usa. La factory lee todo de aqui, asi que **no hay ids hardcodeados** en el codigo. Las dimensiones finales (alto/fondo) las elige el usuario.

### Ejemplo

```json
[
  {
    "id": "STD-3P",
    "name": "Estandar (3 paneles)",
    "defaultHeight": 132.0,
    "defaultDepth": 42.0,
    "horizontals": [
      { "elevation": 0.0,   "profile": "HORIZONTAL_INFERIOR",   "quantity": 2 },
      { "elevation": 44.0,  "profile": "HORIZONTAL_INTERMEDIA", "quantity": 1 },
      { "elevation": 88.0,  "profile": "HORIZONTAL_INTERMEDIA", "quantity": 1 },
      { "elevation": 132.0, "profile": "HORIZONTAL_SUPERIOR",   "quantity": 1 }
    ],
    "defaultArrangement": "SingleDiagonal",
    "diagonalProfile": "TRAVESANO_DINAMICO_OMEGA_3X3",
    "braceStartConnectionPoint": "TroquelCelosia_01",
    "braceEndConnectionPoint": "TroquelCelosia_02",
    "basePlate": "PLACA_BASE_ATORNILLABLE",
    "post": "POSTE_OMEGA_3X3"
  }
]
```

### Campos

| Campo | Tipo | Obligatorio | Descripcion |
|-------|------|-------------|-------------|
| `id` | texto | si | Identificador unico interno (ej. `STD-3P`). |
| `name` | texto | si | Nombre que se ve en el desplegable "Tipo de cabecera". |
| `defaultHeight` | numero | si | Alto sugerido en pulgadas. Se precarga al elegir la plantilla. |
| `defaultDepth` | numero | si | Fondo sugerido en pulgadas. Se precarga al elegir la plantilla. |
| `horizontals` | lista | si | Una entrada por horizontal: `elevation` (in), `profile` (id de `horizontal-profiles.json`), `quantity`. Ver abajo. |
| `defaultArrangement` | texto | no | Celosia por defecto de cada panel. Por defecto `SingleDiagonal`. |
| `diagonalProfile` | texto | no | Perfil de diagonal (id de `diagonal-profiles.json`). Vacio = `defaults.json`. |
| `braceStartConnectionPoint` / `braceEndConnectionPoint` | texto | no | Puntos de conexion de la celosia. Vacio = `defaults.json`. |
| `basePlate` | texto | no | Placa base (id de `base-plates.json`). Vacio = `defaults.json`. |
| `post` | texto | no | Poste por defecto (id de `post-profiles.json`). Vacio = `defaults.json`. |

> Los campos opcionales vacios caen a los valores de `defaults.json` (ver mas abajo). Asi no repites el poste/placa/diagonal en cada plantilla si son los mismos.

### Como funcionan las horizontales (`horizontals`)

- Cada entrada tiene `elevation` (altura en pulgadas, de abajo hacia arriba), `profile` (id del perfil) y `quantity`.
- Deben empezar en `0` y ser **ascendentes** por elevacion.
- Cada par de horizontales consecutivas forma un **panel**. Con N horizontales hay N-1 paneles.
- **Escalan con el alto que elija el usuario.** Las elevaciones se usan como proporciones: si defines la cima en 132 y el usuario pide 200, se reparten proporcionalmente y la cima cae exacto en 200. Conviene que la ultima elevacion coincida con `defaultHeight`.

### Valores validos de `defaultArrangement`

Respeta exactamente estas mayusculas:

| Valor | Significado |
|-------|-------------|
| `NoBracing` | Sin diagonales (solo horizontales). |
| `SingleDiagonal` | Una diagonal por panel. |
| `DoubleDiagonal` | Dos diagonales paralelas (no cruzadas). |
| `XBracing` | Dos diagonales cruzadas en X. |
| `KBracing` | Celosia en K. |
| `Custom` | Reservado; no genera diagonales automaticas. |

### Anadir una nueva plantilla

1. Abre `assets/catalogs/header-templates.json`.
2. Copia un bloque `{ ... }` existente y pegalo antes del corchete final, separando con una coma.
3. Cambia `id` (unico), `name`, dimensiones, `horizontals` y los perfiles. Los campos opcionales que dejes vacios usan `defaults.json`.
4. Guarda y aplica el cambio (recompila o reinicia AutoCAD segun el caso).

Ejemplo de una cabecera baja de 1 panel sin diagonales (poste/placa heredados de `defaults.json`):

```json
  {
    "id": "BASE-1P",
    "name": "Base (1 panel)",
    "defaultHeight": 60.0,
    "defaultDepth": 42.0,
    "horizontals": [
      { "elevation": 0.0,  "profile": "HORIZONTAL_INFERIOR", "quantity": 2 },
      { "elevation": 60.0, "profile": "HORIZONTAL_SUPERIOR", "quantity": 1 }
    ],
    "defaultArrangement": "NoBracing"
  }
```

### Errores comunes

- Olvidar la coma entre dos plantillas, o dejar una coma de mas antes de `]` o `}` que no sea la final.
- Empezar `horizontals` en una elevacion distinta de `0` o ponerlas desordenadas.
- Referenciar un `profile`/`post`/`basePlate` que no existe en su catalogo (la validacion lo avisa).
- Escribir mal `defaultArrangement` (ej. `xbracing` en vez de `XBracing`).
- Repetir un `id` ya usado.

## Valores por defecto (`defaults.json`)

`defaults.json` es la "receta estandar" global: que piezas usa la cabecera cuando una plantilla deja un campo vacio, y los defaults de altura y del margen de cabecera. Es un **objeto** (no una lista).

```json
{
  "post": "POSTE_OMEGA_3X3",
  "basePlate": "PLACA_BASE_ATORNILLABLE",
  "diagonalProfile": "TRAVESANO_DINAMICO_OMEGA_3X3",
  "horizontalProfile": "HORIZONTAL_INTERMEDIA",
  "braceStartConnectionPoint": "TroquelCelosia_01",
  "braceEndConnectionPoint": "TroquelCelosia_02",
  "basePlateConnectionPoint": "PlacaBase_01",
  "defaultHeaderHeight": 132.0,
  "headerEndAllowance": 6.0
}
```

| Campo | Descripcion |
|-------|-------------|
| `post` / `basePlate` / `diagonalProfile` / `horizontalProfile` | Piezas por defecto cuando la plantilla o el editor no especifican. |
| `braceStart/EndConnectionPoint`, `basePlateConnectionPoint` | Puntos de conexion por defecto. |
| `defaultHeaderHeight` | Alto por defecto (lo usa el modo dinamico y la cabecera estandar). |
| `headerEndAllowance` | Las 6" que cada cabecera de extremo agrega al fondo de tarima. |

Si falta `defaults.json` o esta vacio, se usan valores internos de respaldo equivalentes.

---

## Catalogos de piezas

### Perfiles (`post-profiles.json`, `horizontal-profiles.json`, `diagonal-profiles.json`, `reinforcement-profiles.json`)

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `id` | texto | Identificador usado por la cabecera (ej. `POSTE_OMEGA_3X3`). |
| `description` | texto | Texto legible que se muestra como descripcion. |
| `family` | texto | Familia del perfil (ej. `OMEGA`). |
| `width` | numero | Ancho en pulgadas. |
| `depth` | numero | Fondo/peralte en pulgadas. |
| `thickness` | numero | Espesor en pulgadas. |
| `units` | texto | Unidad de las medidas (ej. `in`). |

### Placas base (`base-plates.json`)

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `id` | texto | Identificador (ej. `PLACA_BASE_ATORNILLABLE`). |
| `description` | texto | Texto legible. |
| `width`, `length`, `thickness` | numero | Medidas en pulgadas. |
| `connectionPointId` | texto | Punto de conexion asociado (ver siguiente catalogo). |
| `units` | texto | Unidad de las medidas. |

### Puntos de conexion (`connection-points.json`)

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `id` | texto | Identificador (ej. `TroquelCelosia_01`). |
| `description` | texto | Texto legible. |
| `role` | texto | Rol del punto (ej. `Brace`, `BasePlate`). |
| `localX` | numero | Offset X (in) del punto dentro de su pieza. Lo usa el solver de posiciones. Por defecto 0. |
| `localY` | numero | Offset Y (in) del punto dentro de su pieza. Por defecto 0. |

> Los `id` que use una cabecera deben existir en estos catalogos. La prueba automatica `CatalogStandardConsistencyTests` verifica que la cabecera estandar no referencie ids inexistentes.

## Donde mirar en el codigo

- Modelo y carga de catalogos de piezas: `src/RackCad.Application/Catalogs/`.
- Modelo y carga de plantillas: `src/RackCad.Application/RackFrames/RackFrameTemplate.cs` y `RackFrameTemplateProvider.cs`.
- Plantillas internas de respaldo: `RackFrameTemplateCatalog.cs`.
- Construccion de la cabecera a partir de una plantilla: `RackFrameConfigurationFactory.cs`.
