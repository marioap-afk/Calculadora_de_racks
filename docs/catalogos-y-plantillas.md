# Catalogos y plantillas

Esta guia explica como modificar los catalogos y las plantillas **sin programar**. Hay dos formatos por proposito:

- **CSV (se edita en Excel)** para los datos maestros tabulares: perfiles, placas y puntos de conexion. Una fila = una pieza.
- **JSON** para lo anidado/estructurado: plantillas de cabecera y la receta `defaults`.

> Por que esta separacion: los catalogos son tablas que crecen mucho (mejor en Excel/CSV, y luego SQLite); las plantillas/defaults son estructuras anidadas (mejor en JSON). La carga esta detras de una interfaz (`IRackCatalogProvider`), asi que migrar a SQLite despues no cambia el resto de la app.

## Donde estan los archivos

Fuente versionada (lo que se edita en el repositorio):

```
assets/catalogs/
  post-profiles.csv             Perfiles de poste            (Excel/CSV)
  truss-profiles.csv            Perfiles de celosia (horizontales y diagonales) (Excel/CSV)
  base-plates.csv               Placas base                  (Excel/CSV)
  connection-points.csv         Puntos de conexion (definicion)(Excel/CSV)
  connection-layout.csv         Punto por pieza y vista       (Excel/CSV)
  views.csv                     Vistas posibles              (Excel/CSV)
  blocks.csv                    Bloque por pieza y vista      (Excel/CSV)
  header-templates.json         Plantillas de cabecera       (JSON, auto-descriptivas)
  defaults.json                 Receta estandar global       (JSON)
```

Al compilar, estos archivos se copian a una carpeta `catalogs/` junto al DLL del plugin. La aplicacion los lee al iniciarse. Si para un catalogo existen `.csv` y `.json`, **gana el `.csv`**.

### Como aplicar un cambio

- **Editando el repositorio**: cambia el archivo en `assets/catalogs/` y recompila.
- **En una instalacion ya desplegada** (sin Visual Studio): edita el archivo dentro de la carpeta `catalogs/` que esta junto a `RackCad.Plugin.dll`, **reinicia AutoCAD** y vuelve a ejecutar el comando. No hace falta recompilar.

> Si un archivo falta, la aplicacion sigue funcionando (usa valores internos por defecto). Un error de formato no tumba la app: una celda mal escrita en CSV deja ese campo en su valor por defecto; un JSON invalido se avisa.

## Catalogos en Excel (CSV)

Cada CSV tiene una **fila de encabezados** que nombra las columnas. Reglas:

- Una columna cuyo nombre coincide con un campo conocido (`id`, `displayName`, `width`, `material`...) se carga en ese campo.
- **Cualquier columna extra** (por ejemplo `Ix`, `Iy`, `area`, `norma`...) se guarda automaticamente en la bolsa `properties` de la pieza, **sin tocar el codigo**. Asi agregas propiedades estructurales o lo que necesites desde Excel.
- Campos con coma o comillas: enciérralos en comillas dobles (`"Acero, A36"`). Excel lo hace solo al guardar como CSV.
- Una celda vacia deja el campo en su valor por defecto.

Ejemplo (`post-profiles.csv`):

```
id,displayName,width,thickness,material,Ix,Iy
POSTE_OMEGA_3_X_3_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_DE_CINTA_NEGRA_CALIBRE_14,Poste Omega 3x3 cal.14,3,0.105,Acero A36,2.5,2.5
```

`Ix` e `Iy` no son campos fijos -> entran a `properties`.

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
      { "elevation": 0.0,   "profile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14", "quantity": 2 },
      { "elevation": 44.0,  "profile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14", "quantity": 1 },
      { "elevation": 88.0,  "profile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14", "quantity": 1 },
      { "elevation": 132.0, "profile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14", "quantity": 1 }
    ],
    "defaultArrangement": "SingleDiagonal",
    "diagonalProfile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14",
    "braceStartConnectionPoint": "TROQUEL_CELOSIA",
    "braceEndConnectionPoint": "CELOSIA",
    "basePlate": "PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_DE_4_X_4_13_16",
    "post": "POSTE_OMEGA_3_X_3_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_DE_CINTA_NEGRA_CALIBRE_14"
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
| `horizontals` | lista | si | Una entrada por horizontal: `elevation` (in), `profile` (id de `truss-profiles.csv`), `quantity`. Ver abajo. |
| `defaultArrangement` | texto | no | Celosia por defecto de cada panel. Por defecto `SingleDiagonal`. |
| `diagonalProfile` | texto | no | Perfil de diagonal (id de `truss-profiles.csv`). Vacio = `defaults.json`. |
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
      { "elevation": 0.0,  "profile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14", "quantity": 2 },
      { "elevation": 60.0, "profile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14", "quantity": 1 }
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
  "post": "POSTE_OMEGA_3_X_3_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_DE_CINTA_NEGRA_CALIBRE_14",
  "basePlate": "PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_DE_4_X_4_13_16",
  "diagonalProfile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14",
  "horizontalProfile": "TRAVESAÑO_CINTA_NEGRA_CALIBRE_14_DE_2_X_1_1_8_DE_CINTA_NEGRA_CALIBRE_14",
  "braceStartConnectionPoint": "TROQUEL_CELOSIA",
  "braceEndConnectionPoint": "CELOSIA",
  "basePlateConnectionPoint": "MONTAJE_POSTE",
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

Todas las piezas (perfiles, placas, puntos de conexion) comparten estos **campos comunes**, todos opcionales salvo `id`:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `id` | texto | **Obligatorio.** Identificador usado por cabeceras/plantillas (ej. `POSTE_OMEGA_3_X_3_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_DE_CINTA_NEGRA_CALIBRE_14`). |
| `displayName` | texto | Nombre para mostrar en la UI (si falta, usa `description`, luego `id`). |
| `description` | texto | Descripcion tecnica. |
| `material` | texto | Material (ej. `Acero A36`). |
| `partNumber` | texto | Codigo de parte / SKU (BOM/cotizacion). |
| `manufacturer` | texto | Fabricante. |
| `finish` | texto | Acabado (galvanizado, pintado...). |
| `unitCost` / `currency` / `costUnit` | numero/texto | Costo unitario, moneda y unidad (`m`, `pieza`...). |
| `properties` | objeto | **Bolsa abierta** de pares clave/valor para cualquier propiedad futura, sin tocar el codigo. |

> La pieza describe **qué es** (medidas, material, costo). **El nombre de bloque NO va aqui**: como una pieza se dibuja distinto en cada vista, el bloque (y su capa/escala) vive en `blocks.csv`, una fila por pieza+vista. Asi no hay una columna `blockName` redundante en cada pieza.

> La bolsa `properties` es la costura de **escalabilidad**: agrega ahi atributos que aun no son campos fijos (norma, paso de perforacion, etc.) y se cargan/guardan sin cambiar el modelo. Cuando un atributo se vuelve comun, se promueve a campo tipado.

### Perfiles (`post-profiles.csv`, `truss-profiles.csv`)

> Horizontales y diagonales **no son catalogos distintos**: ambas son miembros de celosia y salen del unico `truss-profiles.csv`. Los **refuerzos son postes**, asi que se toman de `post-profiles.csv` (no hay un catalogo de refuerzos aparte).

Campos comunes (arriba) **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `family` | texto | Familia del perfil (ej. `OMEGA`). |
| `width` | numero | Ancho en pulgadas. **El poste ya lo usa** para su espesor en el dibujo. |
| `depth` | numero | Fondo/peralte en pulgadas. |
| `thickness` | numero | Espesor en pulgadas. |
| `gauge` | texto | Calibre (ej. `cal. 14`). |
| `weightPerMeter` | numero | Peso lineal (kg/m) para BOM/peso. |
| `units` | texto | Unidad de las medidas (ej. `in`). |

### Placas base (`base-plates.json`)

Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `width`, `length`, `thickness` | numero | Medidas en pulgadas. |
| `weightEach` | numero | Peso por pieza (kg). |
| `units` | texto | Unidad de las medidas. |

> La placa **ya no** lleva una columna `connectionPointId`. Una placa puede tener **varios** puntos (mate al poste + barrenos de piso) y su posicion depende de la vista, asi que esa relacion vive en `connection-layout.csv`.

### Puntos de conexion (`connection-points.csv`) — definicion

Solo **que es** el punto (no donde esta). Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `role` | texto | Rol del punto (ej. `Brace`, `BasePlate`, `Anchor`). El factory usa `BasePlate` para elegir el anclaje de la placa. |

### Punto por pieza y vista (`connection-layout.csv`) — ubicacion

Tabla **normalizada**, gemela de `blocks.csv`: relaciona **pieza + punto + vista** con su posicion 2D. Una pieza puede tener **muchos** puntos, y la posicion **depende de la vista** (mismo punto 3D se proyecta distinto en frontal vs planta). El solver de posiciones lee `localX/localY` de aqui (vista `FRONTAL`).

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `pieceId` | texto | Pieza que posee el punto (FK a placa/perfil). |
| `connectionPointId` | texto | Que punto (FK a `connection-points.csv`). |
| `view` | texto | Vista a la que aplica esta posicion (FK a `views.csv`). |
| `localX` | numero | Offset X (in) del punto dentro de la pieza, en esa vista. |
| `localY` | numero | Offset Y (in) del punto dentro de la pieza, en esa vista. |

> **Regla de identidad del `connectionPointId`:** el `id` es el nombre logico del punto; lo que define "misma funcion" es el `role`. Puedes **compartir el mismo `id` entre piezas distintas** (p. ej. `MONTAJE_POSTE` en todas las placas) — la `pieza` desambigua la posicion. Lo unico que NO puedes: repetir el mismo `id` dos veces en la **misma pieza y vista** (la clave `pieza+punto+vista` chocaria). Para varios puntos del mismo tipo en una pieza (p. ej. 4 barrenos), usa ids distintos que comparten el `role`: `ANCLA_1`, `ANCLA_2`, ...

### Vistas (`views.csv`)

Catalogo simple de las vistas en que se puede dibujar una pieza. Campos comunes; en la practica solo:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `id` | texto | Codigo de la vista (ej. `FRONTAL`, `LATERAL_IZQ`, `PLANTA`). Lo referencian `blocks.csv` y `connection-layout.csv`. |
| `displayName` | texto | Nombre para mostrar (ej. `Frontal`). |

### Bloques por vista (`blocks.csv`)

Tabla **normalizada**: una pieza puede tener **varios bloques**, uno por vista. Por eso *no* hay una columna `vista` en los perfiles ni un bloque unico con todo: cada combinacion pieza+vista es **una fila** aqui. Asi una vista nueva = agregar filas (no columnas), y una pieza solo lista las vistas que existan.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `pieceId` | texto | Id de la pieza (perfil/placa/...) a la que pertenece el bloque. |
| `view` | texto | Vista que dibuja (debe existir en `views.csv`). |
| `blockName` | texto | Nombre del bloque de AutoCAD para esa pieza en esa vista. |
| `layer` | texto | Capa de insercion. |
| `scale` | numero | Escala de insercion (por defecto 1). |
| `rotation` | numero | Rotacion en grados (por defecto 0). |

> `pieceId` + `view` forman la clave: el codigo busca el bloque con `catalog.Blocks.FindBlock(pieceId, view)`. Esta es exactamente la estructura relacional que usara SQLite mas adelante (tabla `blocks` con FK a la pieza y a la vista). Aun no se consume en el dibujo; es la base de datos lista para esa fase.

> Los `id` que use una cabecera deben existir en estos catalogos. La prueba automatica `CatalogStandardConsistencyTests` verifica que la cabecera estandar no referencie ids inexistentes.

## Donde mirar en el codigo

- Modelo y carga de catalogos de piezas: `src/RackCad.Application/Catalogs/`.
- Modelo y carga de plantillas: `src/RackCad.Application/RackFrames/RackFrameTemplate.cs` y `RackFrameTemplateProvider.cs`.
- Plantillas internas de respaldo: `RackFrameTemplateCatalog.cs`.
- Construccion de la cabecera a partir de una plantilla: `RackFrameConfigurationFactory.cs`.
