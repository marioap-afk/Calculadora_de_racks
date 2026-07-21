# Catalogos y plantillas

Esta guia explica como modificar los catalogos y las plantillas **sin programar**. Hay dos formatos por proposito:

- **CSV (se edita en Excel)** para los datos maestros tabulares: perfiles (postes, celosia, largueros), placas, ménsulas, componentes de cama de rodamiento y puntos de conexion. Una fila = una pieza.
- **JSON** para lo anidado/estructurado: plantillas de cabecera y la receta `defaults`.

> Por que esta separacion: los catalogos son tablas que crecen mucho (mejor en Excel/CSV); las plantillas/defaults son estructuras anidadas (mejor en JSON). La carga esta detras de una interfaz (`IRackCatalogProvider` → `JsonRackCatalogProvider` → `RackCatalog`), asi que una migracion a otro almacenamiento seria posible si un requisito futuro la justifica; SQLite no forma parte del alcance actual.

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
  seguridad.csv                 Elementos de seguridad (bota/lateral/tope/desviador/parrilla) con costo y peso (Excel/CSV)
  header-templates.json         Plantillas de cabecera                       (JSON, auto-descriptivas)
  defaults.json                 Receta estandar global                       (JSON)
```

Al compilar, estos archivos se copian a una carpeta `catalogs/` junto al DLL del plugin. La aplicacion los lee al iniciarse. Si para un catalogo existen `.csv` y `.json`, **gana el `.csv`**.

### Como aplicar un cambio

- **Editando el repositorio**: cambia el archivo en `assets/catalogs/` y recompila.
- **En una instalacion ya desplegada** (sin Visual Studio): edita el archivo dentro de la carpeta `catalogs/` que esta junto a `RackCad.Plugin.dll` y **vuelve a ejecutar el comando** — la cache se invalida por la firma de los archivos, asi que no hace falta reiniciar AutoCAD ni recompilar. Los CSV se aceptan tanto en UTF-8 como en ANSI/Windows-1252 (lo que guarda Excel).

> Si un archivo falta, la aplicacion sigue funcionando (usa valores internos por defecto). Un error de formato no tumba la app: una celda mal escrita en CSV deja ese campo en su valor por defecto; un JSON invalido se avisa.

> **Validar despues de editar (I-19):** la tolerancia anterior tiene un costo — un `rol` mal escrito descarta la fila en silencio, un id duplicado hace que el lookup tome la primera fila, y un FK colgante sale como dibujo incompleto. El **validador de catalogos** (`CatalogValidator` en `src/RackCad.Application/Catalogs/Validation/`) revisa todo eso en una sola pasada y devuelve un diagnostico con severidades: **error** (ids duplicados, referencias/relaciones invalidas, bloques sin nombre, vistas inexistentes), **advertencia** (filas descartadas por rol, claves de relacion repetidas, bloques genericos sin pieza de catalogo) e **informativa**. Tiene un modo estricto (`IsValid(strict: true)`) que trata las advertencias como fatales, pensado para despliegues. No corrige nada: solo reporta. Ademas construye el **manifiesto esperado** de `blocks-library.dwg`: la lista de bloques, los parametros dinamicos que Application aplica realmente a cada bloque (LONGITUD del riel/postes/separadores, PERALTE, ALTURA, SAQUE, FRENTE/FONDO — desde la misma fuente compartida `CatalogBlockParameters` que usan los productores, sin listas paralelas que diverjan) y una version/huella. La comparacion contra un manifiesto real detecta bloques o parametros faltantes, una version de esquema incompatible y una huella alterada. El validador nunca abre el DWG ni edita un CSV.

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

#### Larguero IN/OUT del dinamico (`LARGUERO_IN_OUT_C6`)

La primera etapa del pallet flow habilita solo `LARGUERO_IN_OUT_C6`, con peralte unico `6` tomado de la columna
`peraltes`. En la vista lateral el bloque es la **pieza completa**: no se inserta ni se contabiliza aparte la
`MENSULA_TROQUEL_REDONDO_CAL_10` referenciada por el perfil. Entrada y salida hacen mate directamente por el origen
del bloque para su colocacion. Esos origenes se ubican en los limites completos del sistema (`X=0` y
`X=TotalLength`): la salida baja izquierda no se espejea y la entrada alta derecha usa el mismo bloque espejeado
en X. La fila LATERAL `TROQUEL_CAMA` no mueve el larguero; describe el mate local que recibe el `TROQUEL_IN` del
riel. Las filas `FRONTAL` y `PLANTA` ya se consumen: los cortes frontal de salida/entrada estiran `LONGITUD` al
largo resuelto de cada frente y `PERALTE` al C6; la planta dibuja un IN/OUT colapsado por niveles en cada extremo.

La regla geometrica no usa la holgura del selectivo. Por cada posicion, `BFR = frente de tarima + 2"`; el corte
automatico completo es `BFR * posiciones + 6"`. Asi, una tarima de 40" produce BFR 42" y un larguero de 48" para
una posicion. `Bfr` se guarda en el documento dinamico; no se muestra como dato comercial del larguero, pero el
componente `Cama` ya lo informa junto con su longitud. Un largo manual sustituye
solo el corte final, no el BFR calculado.

Los campos comerciales vacios son validos en esta etapa. El BOM del dinamico cuenta el bloque IN/OUT completo por
frente, nivel y extremo, agrupado por longitud/peralte; no agrega ni cotiza una mensula separada.

#### Larguero intermedio del dinamico (`LARGUERO_ESCALON_INFINITO`)

El bloque lateral es la pieza completa con `MENSULA_AJUSTE_INFINITO_CAL_10`; la mensula no se inserta aparte. Se
coloca uno por cada poste interno y nivel; los dos extremos usan `LARGUERO_IN_OUT_C6` y no reciben este bloque. El
poste derivado reforzado central representa una sola posicion y tambien recibe uno solo, normal sobre el perfil principal.
Cuando la posicion corresponde al primer poste de una cabecera, el bloque es normal y usa `INICIO_IZQUIERDO`; en
el segundo poste se espejea en X y usa `INICIO_DERECHO`.

El origen del bloque queda sobre el eje vertical del poste. La altura se obtiene evaluando la linea inclinada del
**origen del bloque de riel** en la X mundial del punto de contacto y restando su `localY`. Esa linea no es
`TROQUEL_IN -> TROQUEL_CAMA`: los troqueles gobiernan el armado de la cama completa, pero el larguero intermedio
apoya contra el origen del riel. No se busca una altura discreta de poste porque la mensula tiene una ranura
vertical.

La columna `peraltes` de `secciones.csv` es la fuente de las opciones por frente y nivel. Para el catalogo actual son
`3;3.5;4;4.5;5;5.5;6`. El editor las muestra en `Frente seleccionado` y el builder lateral envia como parametro
dinamico `PERALTE` el mayor valor de los frentes activos de ese nivel, porque su proyeccion los superpone. La planta
usa el mayor peralte de los niveles del frente dibujado. El DTO acepta la lista comun escrita por la version
inmediatamente anterior; documentos mas antiguos sin ninguna lista usan 3.5" en todos sus niveles.

`connection-points.csv` debe declarar ambos ids y `connection-layout.csv` debe dar sus coordenadas para
`LARGUERO_ESCALON_INFINITO,LATERAL`. Si falta cualquiera de los dos mates o el bloque lateral, el builder omite todos
los apoyos intermedios en vez de dibujar una geometria parcial. El BOM del dinamico cuenta el larguero intermedio
completo por longitud/peralte y cantidad fisica; su mensula integrada no se desglosa ni se cotiza aparte.

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

Para componer una cama dentro del SISTEMA DINAMICO, `connection-points.csv` declara `TROQUEL_IN` (riel) y
`TROQUEL_CAMA` (larguero), y `connection-layout.csv` guarda sus coordenadas LATERAL por pieza. El builder alinea
esos mates y gira la cama completa; los offsets no se duplican ni se codifican en UI/Plugin.

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
| `id` | texto | Codigo de la vista (ej. `FRONTAL`, `LATERAL`, `LATERAL_IZQ`, `LATERAL_DER`, `PLANTA`). Lo referencian `blocks.csv` y `connection-layout.csv`. SELECTIVO y DINAMICO consumen `FRONTAL` + `LATERAL` + `PLANTA`; la cabecera `LATERAL` + `PLANTA`; cama `LATERAL`. |
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

### Variantes de seguridad (`seguridad.csv` + `blocks.csv`)

En seguridad, `type` identifica la **familia y su regla de colocacion**; `id` identifica la variante concreta que se
persiste en el rack. El dialogo muestra un solo combo (`Ninguno` + variantes) para cada familia mutuamente exclusiva:
`BOTA`, `LATERAL`, `TOPE`, `DESVIADOR`, `DEFENSA` y `GUIA`. Cambiar la variante conserva la configuracion comun de la familia y cambia
solo `ElementId`; un documento existente vuelve a seleccionar su id exacto.

Variantes catalogadas actualmente:

| `id` | `type` | Regla reutilizada |
|---|---|---|
| `PROTECTOR_BOTA_H_3_16_18` | `BOTA` | Bota en base de poste |
| `PROTECTOR_BOTA_C_4` | `BOTA` | Exactamente la misma regla de Bota H |
| `PROTECTOR_BOTA_C_6` | `BOTA` | Exactamente la misma regla de Bota H |
| `PROTECTOR_LATERAL_BOTA_H_3_16_18` | `LATERAL` | Protector lateral por poste |
| `PROTECTOR_LATERAL_BOTA_C_4` | `LATERAL` | Exactamente la misma regla del Lateral H |
| `PROTECTOR_LATERAL_BOTA_C_6` | `LATERAL` | Exactamente la misma regla del Lateral H |
| `LARGUERO_ESCALON_TOPE_DE_3` | `TOPE` | Rejilla/configuracion de tope |
| `POSTE_3_1_5_8_TOPE` | `TOPE` | Exactamente la misma rejilla/configuracion del larguero tope |
| `DESVIADOR_A_3` | `DESVIADOR` | Rejilla poste x nivel y colocacion en las dos caras exteriores |
| `DESVIADOR_A_4` | `DESVIADOR` | Exactamente la misma regla del Desviador A3 |
| `DESVIADOR_L_3` | `DESVIADOR` | Exactamente la misma regla del Desviador A3 |
| `DESVIADOR_L_3_5` | `DESVIADOR` | Exactamente la misma regla del Desviador A3 |
| `DESVIADOR_L_4` | `DESVIADOR` | Exactamente la misma regla del Desviador A3 |
| `DESVIADOR_L_4_5` | `DESVIADOR` | Exactamente la misma regla del Desviador A3 |
| `DESVIADOR_L_5` | `DESVIADOR` | Exactamente la misma regla del Desviador A3 |
| `DEFENSA_MONTACARGAS` | `DEFENSA` | Grid por poste con salida/entrada y longitud independientes |
| `GUIA_ENTRADA` | `GUIA` | Pareja espejeada por frente y nivel, solo en la entrada del dinamico |

Cada una de estas variantes tiene filas `FRONTAL`, `LATERAL` y `PLANTA` en `blocks.csv`; el `blockName` debe coincidir
exactamente con la biblioteca DWG. Dibujo y BOM resuelven una sola variante por familia. Si un documento anomalo
contiene dos ids de la misma familia, se usa el primero en el orden persistido de forma consistente en todas las vistas
y en el BOM.

#### Familia de desviadores A/L (`DESVIADOR`)

Las siete variantes A/L comparten exactamente la misma regla y se eligen en un unico selector mutuamente exclusivo.
El `ElementId` persistido decide el bloque; no cambia la geometria. El desviador se configura con una rejilla
**poste x nivel de carga** (todos activos por defecto). Incluye los postes
intermedios creados por medio frente. El selector de lado permite `Izquierdo`, `Derecho` o `Ambas`: por cada celda
activa se cuenta una pieza fisica cuando se elige un lado y dos cuando se eligen ambos. En frontal las dos caras pueden
proyectarse sobre una sola referencia; en planta se colapsan los niveles. El BOM conserva siempre la cantidad fisica
completa derivada por `SelectiveDesviadorPlan`.

- `Izquierdo` = cara exterior frontal del fondo; `Derecho` = cara exterior posterior espejeada; `Ambas` = las dos.
  Los documentos anteriores conservan `Ambas` como fallback.
- En frontal y lateral, el origen hace mate con `TROQUEL_LARGUERO` del poste. En planta, el origen del
  desviador hace mate directamente con el origen del poste; no se aplica el offset del troquel.
- Nivel 1: altura configurable sobre el primer troquel, `18"` por defecto, incluso sin larguero a piso.
- Niveles superiores: `6"` debajo del troquel del larguero correspondiente.
- Parametro dinamico exacto: `LONGITUD`, `18"` por defecto.
- `LONGITUD` y la altura del primer nivel aceptan solo pulgadas enteras pares mayores de `8"`.
- Si el claro entre posiciones seleccionadas es menor que `LONGITUD`, la rejilla muestra una nota con una longitud
  par menor sugerida; el usuario puede reducirla o desactivar la celda.

Cada `id` requiere sus tres filas exactas de `blocks.csv`, con bloques `{id}_FRONTAL`, `{id}_LATERAL` y
`{id}_PLANTA`. El `ElementId`, el lado, las dos dimensiones y las celdas apagadas sobreviven `RACKEDITAR`.

#### Seguridad multivista del sistema dinamico

El dinamico reutiliza las familias `BOTA`, `LATERAL` y `DESVIADOR` y agrega `DEFENSA_MONTACARGAS` (`DEFENSA`) y
`GUIA_ENTRADA` (`GUIA`). Su editor filtra esas cinco familias y guarda la misma `SelectiveSafetySelection`. La rejilla usa los postes
transversales y el maximo de niveles de los frentes adyacentes; la misma intencion se proyecta en lateral, frontal y
planta con los bloques catalogados de cada vista.

Un sistema dinamico nuevo selecciona por defecto la primera variante catalogada de las cinco familias. BOTA y
DESVIADOR nacen en ambos lados; LATERAL en la primera orilla de salida y ultima orilla de entrada; DEFENSA en ambos
extremos de cada poste; GUIA en todas las celdas frente x nivel. El usuario puede elegir `Ninguno` o apagar posiciones
desde cada grid. Mientras LATERAL no se edite explicitamente, sus dos orillas se recalculan al cambiar los frentes.

- `Izquierda` = extremo de salida (`X=0`); `Derecha` = extremo de entrada (`X=TotalLength`).
- BOTA coincide con el origen real de la placa base del extremo, incluida una cabecera espejeada/custom.
- LATERAL sustituye las botas del corte y recibe `LONGITUD = TotalLength`; el BOM conserva el sobrelargo de 4".
- DESVIADOR conserva el contrato selectivo: primer nivel sobre `TROQUEL_LARGUERO`, superiores 6" debajo de IN/OUT,
  `LONGITUD` configurable y celdas apagadas segun la rejilla. En las frontales de salida y entrada conserva la
  orientacion original del bloque; el corte de entrada no lo espejea.
- DEFENSA ofrece una fila por poste y dos extremos independientes. Salida y entrada pueden apagarse o recibir
  `LONGITUD` distinta; defaults 12"/12" en orillas y 36"/36" en intermedios. En LATERAL/PLANTA su origen queda en el
  offset catalogado `ORIGEN_POSTE = (-4.75, 0)` y la entrada se refleja. En FRONTAL y LATERAL, Y=0 se toma del origen
  real de la placa base (piso), no del origen del poste; X conserva la referencia al poste. En PLANTA no cambia el datum.
- GUIA solo se proyecta en la entrada. Cada celda frente x nivel activa crea una pareja, una pieza en cada poste del
  frente, con la segunda espejeada. Todas las celdas estan activas por defecto. Su origen coincide en X con
  `TROQUEL_LARGUERO` y queda 8" arriba del larguero IN/OUT de entrada del nivel. `LONGITUD` siempre es la longitud del
  ultimo tramo longitudinal (cabecera/separador) que ocupa ese frente. FRONTAL y LATERAL conservan las elevaciones;
  PLANTA colapsa los niveles coincidentes a una referencia por lado.
- En planta, niveles coincidentes se colapsan en una referencia visible por poste/extremo; en frontal conservan sus
  elevaciones. El protector lateral sustituye las botas correspondientes en cada vista.
- El BOM cuenta las instancias que produce el builder; para GUIA toma la frontal de entrada y conserva las dos piezas
  fisicas por celda, sin duplicarlas por las otras vistas. Nunca usa la cantidad manual ni una segunda formula.

### Tarima como referencia visual (`TARIMA_GENERICA`)

La tarima es un **accesorio de referencia visual**: el selectivo la dibuja automaticamente, una por posicion de carga, cuando se enciende el toggle **"Mostrar tarimas"** del editor, en las vistas **frontal y lateral**. **No entra en el BOM** (es solo dibujo; el BOM se arma por componentes: cabeceras + largueros + seguridad, no por las tarimas). Se apoya sobre la superficie de carga del larguero (el escalon `INICIO_PERFIL`) y, si el nivel de piso descansa directo en el suelo, sobre `Y=0`.

Para conectarla se necesita **una fila por vista** en `blocks.csv` con `pieceId = TARIMA_GENERICA` (la constante `SelectiveRackDefaults.PalletPieceId`). Se dibuja en **`FRONTAL` y `LATERAL`** (planta queda para una version futura):

```
pieceId,view,blockName,layer,scale,rotation
TARIMA_GENERICA,FRONTAL,TARIMA_GENERICA,,1,0
TARIMA_GENERICA,LATERAL,TARIMA_GENERICA,,1,0
```

- `blockName` debe coincidir **exacto** con el nombre del bloque en la libreria DWG (`blocks-library.dwg`). En este proyecto el bloque se llama `TARIMA_GENERICA` y es **el mismo para frontal y lateral**. Si en tu libreria tiene otro nombre, ajusta la celda (cambio de solo-datos: recarga en vivo, sin recompilar) o cambia la constante `PalletPieceId`.
- El bloque es **dinamico** con dos parametros de estirado: **`LONGITUD`** (horizontal) y **`ALTURA`** (vertical). El plugin los estira a la tarima de cada celda. Importante: **`LONGITUD` es el frente en la vista FRONTAL y el fondo en la vista LATERAL** — es el mismo parametro con distinto significado por vista (en lateral la tarima se ve de canto y su ancho es el fondo). Los nombres se comparan **sin distinguir mayusculas/minusculas**, asi que el casing no tiene que ser exacto. Si el bloque no fuera parametrico, se inserta a su tamaño nominal. El plugin asume que el **origen del bloque esta en la BASE-CENTRO** de la tarima (centrado a lo ancho, en la base; asi es el bloque del proyecto): la tarima queda centrada en su hueco y su base cae exactamente sobre la superficie de carga del larguero.
- En **frontal** se dibuja una tarima por posicion de carga (frente × niveles + piso). En **lateral** se dibuja una tarima por fondo por nivel (las tarimas de un mismo nivel a lo ancho se solapan de canto en una sola), abarcando el fondo del rack.
- Si **no existe** la fila para una vista en `blocks.csv` (o el `pieceId`/`blockName` no cuadran), el toggle no dibuja nada en esa vista (sin error): la funcion se degrada de forma silenciosa.

### Parrilla / deck (`PARRILLA_GENERICA`)

La parrilla es un **elemento de seguridad** (`seguridad.csv` con `type = PARRILLA`) que **sí entra al BOM**. Va **una parrilla por tarima** (3 tarimas → 3 parrillas), justo **debajo** de cada una. Se configura desde el diálogo **"Elementos de seguridad"** con una **rejilla frente × nivel de larguero resuelto** (elige en qué posiciones va), dos toggles **"Dibujar en frontal / lateral"** (en planta no se dibuja) y campos opcionales de **frente** y **cantidad**. Se coloca con **origen inferior-izquierda coincidente** con el inicio del larguero, sobre la superficie de carga. Necesita **una fila por vista** en `blocks.csv`:

```
PARRILLA_GENERICA,FRONTAL,PARRILLA_GENERICA_FRONTAL,,1,0
PARRILLA_GENERICA,LATERAL,PARRILLA_GENERICA_LATERAL,,1,0
```

- Bloque **por vista** (`_FRONTAL` / `_LATERAL`), con parámetros dinámicos **`FRENTE`** (frontal) y **`FONDO`** (lateral, = el fondo de la cabecera).
- **Por defecto** (ambos campos vacíos): `FRENTE` = el **frente de la tarima** de esa celda, y van **tantas parrillas como tarimas**. En un frente **medio-frente** se reparten por **tramo cargado**, cabiendo las que quepan (`floor(tramo / frente)`) — exactamente las mismas que tarimas, así el dibujo y el BOM concuerdan.
- **Frente manual** (`ParrillaFrente > 0`): fija el ancho y **recalcula** cuántas caben (`floor(claro / frente)`). Así se logra p. ej. 2 parrillas bajo 3 tarimas.
- **Cantidad manual** (`ParrillaCantidad > 0`): fija cuántas van **por posición de carga**; con el frente vacío conservan el **ancho de la tarima** (2 parrillas estándar de 40" bajo 3 tarimas). En un **medio frente** la cantidad es **por tramo** (cada tramo es su propia posición de carga), así que la celda muestra la suma.
- Ambos valores son **uno para todo el rack**. El diálogo muestra la cuenta **en cada casilla** de la rejilla y el **total**, y **rechaza** una cantidad que no quepa diciendo cuántas caben. Aun así el builder **acota** (`Math.Min(cantidad, las-que-caben)`): el diálogo valida contra la matriz de ese momento, y si luego angostas un frente hay que degradar a lo que cabe en vez de dibujar fuera del marco. Donde no quepa ninguna **no se dibuja nada** — ni en el BOM.
- En la **lateral** la fila se ve de canto y colapsa a **una sola parrilla por fondo y altura**, pero primero pregunta si la fila **existe** (`SelectiveFrontalBuilder.ParrillaExistsAt`, la misma regla del frontal y el BOM): si no cabe ninguna, la lateral tampoco dibuja. Cero es cero en las tres salidas.
- El **BOM** cuenta por tarima en **todos los fondos**, independientemente de los toggles de vista — apagar ambas vistas la deja en el BOM pero no la dibuja. La constante del tipo (`SelectiveSafetyPlacement.ParrillaType`) es `PARRILLA`. La regla de conteo vive en un solo sitio, `SelectiveFrontalBuilder.ParrillaRow`, que consumen el builder y el BOM.

## Donde mirar en el codigo

Catalogos y plantillas:

- Modelo y carga de catalogos de piezas: `src/RackCad.Application/Catalogs/` (`CatalogEntries.cs` = modelo, `JsonRackCatalogProvider.cs` = carga CSV/JSON, `CsvCatalogReader.cs` = lector CSV, `SeccionRoles.cs` = clasificacion por rol compartida).
- Validacion de catalogos (I-19): `src/RackCad.Application/Catalogs/Validation/` (`CatalogValidator.cs` = motor con severidades, `CatalogValidationReport.cs` = diagnostico unico, `CatalogBlockManifest.cs` = manifiesto esperado de `blocks-library.dwg` y su comparacion).
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
- Identidad + round-trip: el sobre `RackEmbedDocument` (`SchemaVersion`, `Kind` = `selective`/`dynamic`/`cabecera`/`cama`, `View` = `frontal`/`lateral`/`planta`, `Section` = indice de corte; en dinamico `0=salida` y `1=entrada`; `-1` = vista no seccionada, `Id` GUID estable, `Name`, `Design`) se embebe en la definicion del bloque; ver `src/RackCad.Application/Persistence/RackEmbedDocument.cs`. Stores del diseño: `SelectivePalletDesignStore` (selectivo), `RackProjectStore` → `.rackcad.json` (dinamico/cabecera), `FlowBedConfigurationStore` (cama).
