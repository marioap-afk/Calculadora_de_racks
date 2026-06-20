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
  header-templates.json         Plantillas de cabecera (modo rapido)
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

Una plantilla define la **forma** de una cabecera para el modo rapido: cuantas horizontales hay, a que alturas y que celosia llevan por defecto. Las dimensiones finales (alto/fondo) las elige el usuario.

### Ejemplo

```json
[
  {
    "id": "STD-3P",
    "name": "Estandar (3 paneles)",
    "defaultHeight": 132.0,
    "defaultDepth": 42.0,
    "horizontalElevations": [0.0, 44.0, 88.0, 132.0],
    "defaultArrangement": "SingleDiagonal"
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
| `horizontalElevations` | lista de numeros | si | Alturas de cada horizontal en pulgadas. Ver abajo. |
| `defaultArrangement` | texto | no | Celosia por defecto de cada panel. Por defecto `SingleDiagonal`. |

### Como funcionan las elevaciones (`horizontalElevations`)

- Son las **alturas reales de cada horizontal, en pulgadas**, de abajo hacia arriba.
- Deben empezar en `0` y ser **ascendentes**.
- Cada par de horizontales consecutivas forma un **panel**. Con N horizontales hay N-1 paneles.
- **Escalan con el alto que elija el usuario.** Las elevaciones se usan como proporciones: si la plantilla define `[0, 44, 88, 132]` (cima en 132) y el usuario pide 200 de alto, se reparten a `[0, 66.7, 133.3, 200]`. La cima siempre cae exactamente en el alto pedido.

Por eso conviene que la ultima elevacion coincida con `defaultHeight` (asi al alto por defecto las horizontales quedan en los valores exactos que escribiste).

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
3. Cambia `id` (unico), `name`, dimensiones, `horizontalElevations` y `defaultArrangement`.
4. Guarda y aplica el cambio (recompila o reinicia AutoCAD segun el caso).

Ejemplo de una cabecera baja de 1 panel sin diagonales:

```json
  {
    "id": "BASE-1P",
    "name": "Base (1 panel)",
    "defaultHeight": 60.0,
    "defaultDepth": 42.0,
    "horizontalElevations": [0.0, 60.0],
    "defaultArrangement": "NoBracing"
  }
```

### Errores comunes

- Olvidar la coma entre dos plantillas, o dejar una coma de mas antes de `]` o `}` que no sea la final.
- Empezar `horizontalElevations` en un valor distinto de `0` o ponerlas desordenadas.
- Escribir mal `defaultArrangement` (ej. `xbracing` en vez de `XBracing`).
- Repetir un `id` ya usado.

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
