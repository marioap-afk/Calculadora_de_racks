# Catalogos y plantillas

Esta guia explica como modificar los catalogos y las plantillas **sin programar**. Hay dos formatos por proposito:

- **CSV (se edita en Excel)** para los datos maestros tabulares: perfiles (postes, celosia, largueros), placas, ménsulas, componentes de cama de rodamiento y puntos de conexion. Una fila = una pieza.
- **JSON** para lo anidado/estructurado: plantillas de cabecera y la receta `defaults`.

> Por que esta separacion: los catalogos son tablas que crecen mucho (mejor en Excel/CSV, y luego SQLite); las plantillas/defaults son estructuras anidadas (mejor en JSON). La carga esta detras de una interfaz (`IRackCatalogProvider` → `JsonRackCatalogProvider` → `RackCatalog`), asi que migrar a SQLite despues no cambia el resto de la app.

> **Contexto:** estos catalogos alimentan a los CUATRO tipos de rack que el plugin diseña y dibuja en AutoCAD — CABECERA (marco), SISTEMA DINÁMICO (pallet flow), CAMA DE RODAMIENTO (flow bed) y SELECTIVO (editor avanzado). Cada tipo tiene su ventana editora y su round-trip de edicion (comando `RACKEDITAR`). Los mismos ids de pieza se comparten entre todos; por eso el catalogo es una sola fuente de verdad.

## Donde estan los archivos

Fuente versionada (lo que se edita en el repositorio):

```
assets/catalogs/
  secciones.csv                 TODOS los perfiles estructurales en una hoja (columna rol: POSTE | CELOSIA | LARGUERO)
  mensulas.csv                  Ménsulas (conector de extremo del larguero)   (Excel/CSV)
  base-plates.csv               Placas base (con peralte estandar)            (Excel/CSV)
  flow-bed-profiles.csv         Componentes de cama de rodamiento (riel/rodillo/freno/tope) (Excel/CSV)
  connection-points.csv         Puntos de conexion (definicion)               (Excel/CSV)
  connection-layout.csv         Punto por pieza y vista (posicion 2D)         (Excel/CSV)
  views.csv                     Vistas posibles                              (Excel/CSV)
  blocks.csv                    Bloque por pieza y vista                     (Excel/CSV)
  header-templates.json         Plantillas de cabecera                       (JSON, auto-descriptivas)
  defaults.json                 Receta estandar global                       (JSON)
```

Al compilar, estos archivos se copian a una carpeta `catalogs/` junto al DLL del plugin. La aplicacion los lee al iniciarse. Si para un catalogo existen `.csv` y `.json`, **gana el `.csv`**.

### Como aplicar un cambio

- **Editando el repositorio**: cambia el archivo en `assets/catalogs/` y recompila.
- **En una instalacion ya desplegada** (sin Visual Studio): edita el archivo dentro de la carpeta `catalogs/` que esta junto a `RackCad.Plugin.dll` y **vuelve a ejecutar el comando** — la cache se invalida por la firma de los archivos, asi que no hace falta reiniciar AutoCAD ni recompilar. Los CSV se aceptan tanto en UTF-8 como en ANSI/Windows-1252 (lo que guarda Excel).

> Si un archivo falta, la aplicacion sigue funcionando (usa valores internos por defecto). Un error de formato no tumba la app: una celda mal escrita en CSV deja ese campo en su valor por defecto; un JSON invalido se avisa.

## Catalogos en Excel (CSV)

Cada CSV tiene una **fila de encabezados** que nombra las columnas. Reglas:

- Una columna cuyo nombre coincide con un campo conocido (`id`, `displayName`, `width`, `material`...) se carga en ese campo.
- **Cualquier columna extra** (por ejemplo `Ix`, `Iy`, `area`, `norma`...) se guarda automaticamente en la bolsa `properties` de la pieza, **sin tocar el codigo**. Asi agregas propiedades estructurales o lo que necesites desde Excel.
- Campos con coma o comillas: enciérralos en comillas dobles (`"Acero, A36"`). Excel lo hace solo al guardar como CSV.
- Una celda vacia deja el campo en su valor por defecto.

Ejemplo (filas de `secciones.csv`):

```
id,displayName,width,thickness,material,Ix,Iy
POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA,Poste Omega 3x3 cal.14,3,0.105,Acero A36,2.5,2.5
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

Una plantilla es **auto-descriptiva**: define que perfil usan las horizontales, que perfil de diagonal, puntos de conexion, placa y poste. La factory lee las piezas de aqui, asi que **no hay ids hardcodeados** en el codigo. Las dimensiones finales (alto/fondo) las elige el usuario, y las **elevaciones** de la celosia las calcula la factory parametricamente (ver abajo); las de la plantilla ya no se usan como posiciones.

### Ejemplo

```json
[
  {
    "id": "STD-3P",
    "name": "Estandar (3 paneles)",
    "defaultHeight": 132.0,
    "defaultDepth": 42.0,
    "horizontals": [
      { "elevation": 0.0,   "profile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14", "quantity": 2 },
      { "elevation": 44.0,  "profile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14", "quantity": 1 },
      { "elevation": 88.0,  "profile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14", "quantity": 1 },
      { "elevation": 132.0, "profile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14", "quantity": 1 }
    ],
    "defaultArrangement": "SingleDiagonal",
    "diagonalProfile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14",
    "braceStartConnectionPoint": "TROQUEL_CELOSIA",
    "braceEndConnectionPoint": "CELOSIA",
    "basePlate": "PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16",
    "post": "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA"
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
| `horizontals` | lista | si | Una entrada por horizontal: `elevation` (in), `profile` (id de `secciones.csv` (rol CELOSIA)), `quantity`. Ver abajo. |
| `defaultArrangement` | texto | no | Celosia por defecto de cada panel. Por defecto `SingleDiagonal`. |
| `diagonalProfile` | texto | no | Perfil de diagonal (id de `secciones.csv` (rol CELOSIA)). Vacio = `defaults.json`. |
| `braceStartConnectionPoint` / `braceEndConnectionPoint` | texto | no | Puntos de conexion de la celosia. Vacio = `defaults.json`. |
| `basePlate` | texto | no | Placa base (id de `base-plates.csv`). Vacio = `defaults.json`. |
| `post` | texto | no | Poste por defecto (id de `secciones.csv` (rol POSTE)). Vacio = `defaults.json`. |

> Los campos opcionales vacios caen a los valores de `defaults.json` (ver mas abajo). Asi no repites el poste/placa/diagonal en cada plantilla si son los mismos.

### Como funcionan las horizontales (`horizontals`)

- Cada entrada tiene `elevation` (altura en pulgadas, de abajo hacia arriba), `profile` (id del perfil) y `quantity`.
- Deben empezar en `0` y ser **ascendentes** por elevacion.
- Cada par de horizontales consecutivas forma un **panel**. Con N horizontales hay N-1 paneles.
- **Las elevaciones de la plantilla ya NO se usan como posiciones** (ni escalan proporcionalmente). La factory calcula las elevaciones parametricamente: el primer travesano cae en el troquel de inicio, los paneles se reparten con `PanelClear` de 44" (con cierres de 0/1/2 travesanos) y la horizontal superior cae **exacto** en `alto − remate` (`PostTopRemate` = 4"), asi el alto construido coincide con el alto pedido (240 → 240). De la plantilla solo se toman los **perfiles** (y placa/poste/puntos de conexion).

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
      { "elevation": 0.0,  "profile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14", "quantity": 2 },
      { "elevation": 60.0, "profile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14", "quantity": 1 }
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

### Plantillas de usuario (desde el configurador)

Ademas de editar `header-templates.json` a mano, ahora se pueden **guardar plantillas de usuario desde el
configurador de cabeceras** sin tocar archivos: en la **configuracion rapida** hay un campo "Nombre de
plantilla" y un boton **"Guardar como plantilla"** que guarda la cabecera actual como una plantilla
reutilizable entre proyectos.

- Se guardan en `%AppData%\RackCad\user-templates.json` (ubicacion **escribible por usuario**, no el
  `header-templates.json` compartido junto al DLL, que es de solo lectura). El formato es el mismo (una lista de
  plantillas auto-descriptivas), con id automatico tipo `USER-xxxxxxxx`.
- El desplegable "Tipo de cabecera" **mezcla** las plantillas del catalogo/internas con las del usuario; si un id
  coincide, **gana la del usuario**.
- Se guarda la **forma actual** (perfiles, poste, placa, diagonal, puntos de conexion, alto/fondo). **No** se
  guardan las **excepciones por panel** (son ajustes por proyecto, no parte de la plantilla estandar): al aplicar
  la plantilla, la factory regenera las elevaciones parametricamente.

## Valores por defecto (`defaults.json`)

`defaults.json` es la "receta estandar" global: que piezas usa la cabecera cuando una plantilla deja un campo vacio, y los defaults de altura y del margen de cabecera. Es un **objeto** (no una lista).

```json
{
  "post": "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA",
  "basePlate": "PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16",
  "diagonalProfile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14",
  "horizontalProfile": "TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14",
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
| `id` | texto | **Obligatorio.** Identificador usado por cabeceras/plantillas y demas tipos de rack (ej. `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA`). |
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

### Perfiles estructurales (`secciones.csv` — UNA hoja para postes, celosia y largueros)

> **Todos los perfiles estructurales viven en un solo CSV** con una columna `rol` que dice que es cada fila:
> `POSTE`, `CELOSIA` o `LARGUERO`. El provider separa las filas en las tres listas de siempre, asi que el
> resto de la app no cambio. Horizontales y diagonales **no son catalogos distintos** (ambas son celosia,
> filas `rol=CELOSIA`); los **refuerzos son postes** (filas `rol=POSTE`). Las columnas exclusivas de
> largueros (`peraltes`, `mensula`) se dejan vacias en las demas filas. Si `secciones.csv` no existe, se
> leen los tres CSV legacy (`post-profiles.csv`, `truss-profiles.csv`, `beam-profiles.csv`) como fallback.

Campos comunes (arriba) **mas** `rol` y, para postes/celosia:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `family` | texto | Familia del perfil (ej. `OMEGA`). |
| `width` | numero | Ancho en pulgadas. **El poste ya lo usa** para su espesor en el dibujo. |
| `depth` | numero | Fondo/peralte en pulgadas. |
| `thickness` | numero | Espesor en pulgadas. |
| `gauge` | texto | Calibre (ej. `cal. 14`). |
| `weightPerMeter` | numero | Peso lineal (kg/m) para BOM/peso. |
| `units` | texto | Unidad de las medidas (ej. `in`). |

### Largueros (filas `rol=LARGUERO` de `secciones.csv`)

Un larguero (viga de carga) = **un bloque dinamico**: tanto su LONGITUD como su PERALTE son parametros del bloque (grips), no filas por medida. Cada fila declara un tipo de larguero, los peraltes que admite y su ménsula de extremo. Es el catalogo que consume el editor SELECTIVO (combo de peralte por celda).

Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `family` | texto | Familia del larguero (ej. `Larguero`). |
| `peraltes` | texto | Valores permitidos del parametro PERALTE, separados por `;` (ej. `3;3.5;4;4.5;5;5.5;6`). El editor selectivo los ofrece en un combo por celda; el numero elegido no crea filas nuevas. |
| `width`, `thickness` | numero | Medidas en pulgadas. |
| `units`, `gauge` | texto | Unidad de medidas y calibre. |
| `mensula` | texto | **FK** a la ménsula de extremo (`id` de `mensulas.csv`). Es el conector fijo de este larguero; se cuentan dos por larguero en el BOM. |
| `weightPerMeter` | numero | Peso lineal base (el peso por peralte se resolvera en el BOM). |

### Ménsulas (`mensulas.csv`)

Conector de extremo del larguero: pieza fija que el BOM cuenta (dos por larguero). Cada larguero apunta a una via la columna `mensula` de su fila en `secciones.csv`.

Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `type` | texto | Tipo de ménsula (ej. `3 Remaches`). |
| `height` | numero | Alto en pulgadas. |
| `units`, `gauge` | texto | Unidad de medidas y calibre. |
| `weightEach` | numero | Peso por pieza (kg). |

### Placas base (`base-plates.csv`)

Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `width`, `length`, `thickness` | numero | Medidas en pulgadas. |
| `weightEach` | numero | Peso por pieza (kg). |
| `units` | texto | Unidad de las medidas. |
| `peralteBase` | numero | Termino base del peralte de la placa: `peralte = peralteBase + peraltePorPeraltePoste * peralte_poste`. |
| `peraltePorPeraltePoste` | numero | Pendiente: peralte de placa ganado por cada unidad de peralte del poste. `1` = "poste + base"; `0` = un peralte fijo. |

> **Peralte estandar de la placa.** El peralte no es una constante: sale de `StandardPeralte(peralte_poste) = peralteBase + peraltePorPeraltePoste * peralte_poste`, es decir depende del poste que la placa recibe. En la CABECERA ese peralte estandar es **editable por placa** en el configurador (`BasePlatePlacement.PeralteOverride`; vacio = derivado). El SELECTIVO toma la placa/peralte desde la cabecera embebida en cada poste.

> La placa **ya no** lleva una columna `connectionPointId`. Una placa puede tener **varios** puntos (mate al poste + barrenos de piso) y su posicion depende de la vista, asi que esa relacion vive en `connection-layout.csv`.

### Componentes de cama de rodamiento (`flow-bed-profiles.csv`)

Piezas fijas de una cama de rodillos (flow bed / pushback): riel, rodillo, freno y tope. La **regla de armado** (cuantos rodillos por capacidad, frenos cada N, paso) vive en el codigo, no aqui; cada fila solo describe una pieza. Las consume la ventana de CAMA DE RODAMIENTO.

Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `role` | texto | Cual pieza es: `RIEL`, `RODILLO`, `FRENO` o `TOPE`. |
| `diameter` | numero | Diametro (in), para rodillos/frenos. |
| `width`, `height`, `length` | numero | Medidas en pulgadas (la longitud del riel es parametrica = fondo del carril, por eso suele ir vacia). |
| `units`, `gauge` | texto | Unidad de medidas y calibre. |
| `capacityKg` | numero | Capacidad de carga (kg) del rodillo; base del futuro conteo de rodillos por capacidad. |
| `weightEach` | numero | Peso por pieza (kg). |

### Separadores (`secciones.csv`, rol `SEPARADOR`)

Perfil del separador que une cabeceras a lo largo del tramo (SISTEMA DINÁMICO) o fondos adyacentes (SELECTIVO doble profundidad). Vive en `secciones.csv` con rol `SEPARADOR` y el provider lo carga en `RackCatalog.SpacerProfiles`; el dibujo sale de `blocks.csv` + `connection-layout.csv` y su `id` es la constante `SeparatorCatalogId`. El BOM del selectivo usa su `displayName` desde ahí. (El antiguo `spacers-profiles.csv`, huérfano, se eliminó.)

Campos comunes **mas**: `family`, `width`, `depth`, `thickness`, `units`, `gauge`, `weightPerMeter`.

### Puntos de conexion (`connection-points.csv`) — definicion

Solo **que es** el punto (no donde esta). Campos comunes **mas**:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `role` | texto | Rol del punto (ej. `BasePlate`, `Poste`, `Truss`, `Spacer`, `FlowBed`, `Larguero`). El factory usa `BasePlate` para elegir el anclaje de la placa. |

### Punto por pieza y vista (`connection-layout.csv`) — ubicacion

Tabla **normalizada**, gemela de `blocks.csv`: relaciona **pieza + punto + vista** con su posicion 2D. Una pieza puede tener **muchos** puntos, y la posicion **depende de la vista** (mismo punto 3D se proyecta distinto en frontal vs planta). El solver de posiciones lee `localX/localY` de aqui.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `pieceId` | texto | Pieza que posee el punto (FK a placa/perfil). |
| `connectionPointId` | texto | Que punto (FK a `connection-points.csv`). |
| `view` | texto | Vista a la que aplica esta posicion (FK a `views.csv`). |
| `localX` | numero | Offset X (in) del punto dentro de la pieza, en esa vista. |
| `localXPorParam` | numero | Pendiente: cuanto se mueve X por cada unidad del parametro nombrado en `paramX`. `X = localX + localXPorParam * valor(paramX)`. `0` (o vacio) = X fija. |
| `paramX` | texto | Nombre del parametro de bloque que mueve X (ej. `PERALTE`); vacio cuando la X es fija. |
| `localY` | numero | Offset Y (in) del punto dentro de la pieza, en esa vista. |
| `localYPorParam` | numero | Pendiente: cuanto se mueve Y por cada unidad del parametro nombrado en `paramY`. `Y = localY + localYPorParam * valor(paramY)`. `0` (o vacio) = Y fija. |
| `paramY` | texto | Nombre del parametro de bloque que mueve Y; vacio cuando la Y es fija. |

> Las pendientes capturan como **dato** un punto que se desliza cuando cambia un parametro del bloque, en lugar de una fila por cada valor. Aplican en **ambos ejes**: el troquel del larguero en FRONTAL desliza en X (`localX=-0.75`, `localXPorParam=0.5`, `paramX=PERALTE`), y ese mismo punto en PLANTA desliza en Y (`localY=-0.75`, `localYPorParam=0.5`, `paramY=PERALTE`).

> **Regla de identidad del `connectionPointId`:** el `id` es el nombre logico del punto; lo que define "misma funcion" es el `role`. Puedes **compartir el mismo `id` entre piezas distintas** (p. ej. `MONTAJE_POSTE` en todas las placas) — la `pieza` desambigua la posicion. Lo unico que NO puedes: repetir el mismo `id` dos veces en la **misma pieza y vista** (la clave `pieza+punto+vista` chocaria). Para varios puntos del mismo tipo en una pieza (p. ej. 4 barrenos), usa ids distintos que comparten el `role`: `ANCLA_1`, `ANCLA_2`, ...

### Vistas (`views.csv`)

Catalogo simple de las vistas en que se puede dibujar una pieza. Campos comunes; en la practica solo:

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `id` | texto | Codigo de la vista (ej. `FRONTAL`, `LATERAL`, `LATERAL_IZQ`, `LATERAL_DER`, `PLANTA`). Lo referencian `blocks.csv` y `connection-layout.csv`. El SELECTIVO se dibuja en `FRONTAL` + `LATERAL` + `PLANTA`; la cabecera en `LATERAL` + `PLANTA`; dinamico y cama en `LATERAL`. |
| `displayName` | texto | Nombre para mostrar (ej. `Frontal`). |

### Bloques por vista (`blocks.csv`)

Tabla **normalizada**: una pieza puede tener **varios bloques**, uno por vista. Por eso *no* hay una columna `vista` en los perfiles ni un bloque unico con todo: cada combinacion pieza+vista es **una fila** aqui. Asi una vista nueva = agregar filas (no columnas), y una pieza solo lista las vistas que existan.

| Campo | Tipo | Descripcion |
|-------|------|-------------|
| `pieceId` | texto | Id de la pieza (perfil/placa/...) a la que pertenece el bloque. |
| `view` | texto | Vista que dibuja (debe existir en `views.csv`). |
| `blockName` | texto | Nombre del bloque de AutoCAD para esa pieza en esa vista. |
| `layer` | texto | Capa de insercion. |
| `color` | numero | Color de indice ACI (opcional). |
| `scale` | numero | Escala de insercion (por defecto 1). |
| `rotation` | numero | Rotacion en grados (por defecto 0). |

> `pieceId` + `view` forman la clave: el codigo busca el bloque con `catalog.Blocks.FindBlock(pieceId, view)`. Esta es exactamente la estructura relacional que usara SQLite mas adelante (tabla `blocks` con FK a la pieza y a la vista). **Es la ruta activa del dibujo**: los cuatro tipos de rack resuelven sus bloques por aqui, y `blockName` debe coincidir **exacto** con el nombre del bloque en la libreria DWG.

> Los `id` que use una cabecera deben existir en estos catalogos. La prueba automatica `CatalogStandardConsistencyTests` verifica que la cabecera estandar no referencie ids inexistentes.

## Donde mirar en el codigo

Catalogos y plantillas:

- Modelo y carga de catalogos de piezas: `src/RackCad.Application/Catalogs/` (`CatalogEntries.cs` = modelo, `JsonRackCatalogProvider.cs` = carga CSV/JSON, `CsvCatalogReader.cs` = lector CSV).
- Modelo y carga de plantillas: `src/RackCad.Application/RackFrames/RackFrameTemplate.cs` y `RackFrameTemplateProvider.cs`.
- Plantillas internas de respaldo: `RackFrameTemplateCatalog.cs`.
- Construccion de la cabecera a partir de una plantilla: `RackFrameConfigurationFactory.cs`.

Los CUATRO tipos de rack (ventana editora + comando):

| Tipo | Ventana (UI) | Comando(s) |
|------|--------------|------------|
| Cabecera (marco) | `RackFrameConfiguratorWindow` | `RACKCABECERA`, `QUICKCABECERA` |
| Sistema dinamico (pallet flow) | `RackDynamicSystemWindow` | `RACKSISTEMADINAMICO` |
| Cama de rodamiento (flow bed) | `RackFlowBedWindow` | `QUICKCAMA` |
| Selectivo (editor avanzado) | `RackSelectiveWindow` (+ `RackBomWindow`) | `RACKSELECTIVO` |

- Menu principal: comando `RACKCAD`. Round-trip de edicion (los cuatro tipos): comando `RACKEDITAR`.
- Geometria del selectivo: `src/RackCad.Application/Systems/SelectiveGeometryResolver.cs`; BOM: `SelectiveBomBuilder.cs`.
- Comandos del plugin: `src/RackCad.Plugin/RackFrameCommands.cs`.
- Identidad + round-trip: el sobre `RackEmbedDocument` (`SchemaVersion`, `Kind` = `selective`/`dynamic`/`cabecera`/`cama`, `View` = `frontal`/`lateral`/`planta`, `Section` = indice de corte lateral del selectivo (`-1` = vista no seccionada), `Id` GUID estable, `Name`, `Design`) se embebe en la definicion del bloque; ver `src/RackCad.Application/Persistence/RackEmbedDocument.cs`. Stores del diseño: `SelectivePalletDesignStore` (selectivo), `RackProjectStore` → `.rackcad.json` (dinamico/cabecera), `FlowBedConfigurationStore` (cama).
