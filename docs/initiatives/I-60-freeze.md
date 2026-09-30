# I-60 — Freeze: nombre lógico automático de los racks nuevos

Frozen: YES

Unit: `I-60`. Workflow: V2. Archetype: EXTENSION de producto. Rama `feature/nombre-automatico-racks`, base `1304101d`.
Contrato: [I-60-nombre-automatico-racks.md](I-60-nombre-automatico-racks.md). Decisiones: [decisions/I-60.md](../automation/decisions/I-60.md).
Caracterizacion (Discovery): [evidencia](../automation/evidence/I-60-evidence.md) §1. Version R2 (tras la revision R1 del Arquitecto).

Una vez congelado es inmutable; todo cambio posterior es una enmienda `A-n` en el registro de decisiones.

## 1. Autoridad

- **Nombre logico** = `RackEmbedDocument.Name`. NO es el nombre de la definicion de bloque, ni el BaseName de AUTH-11, ni el RackId. El RackId sigue siendo la
  identidad; el nombre es metadato editable.
- **Copias persistidas del nombre** en el diseño interno, que algunos editores recargan: Selectivo `SelectivePalletDesignDocument.Name` (el editor recarga
  esta), Cabecera `RackFrameConfiguration.Name` (`RACKEDITAR` construye el configurador desde `project.Header`) y Cantilever `CantileverLineDesign.Name`
  (comparada por AUTH-13). Dinamico y Push Back no persisten copia (sus diseños no tienen `Name`); `DynamicRackSystem.Name` y `PushBackSystem.Name` son objetos
  de ejecucion que solo usa la anotacion del nombre. La cama no tiene copia.
- **Autoridad pura nueva en Application** (sin tipos de AutoCAD): recibe la familia y los nombres logicos del dibujo y devuelve el siguiente nombre; decide
  tambien si el nombre pedido para un rack nuevo esta «sin asignar». El Plugin solo aporta el escaneo (`RackBlockFinder.ScanEnvelopes`, lectura) y aplica el
  resultado. Nombres de simbolos no congelados.
- AUTH-11 (`RackViewBaseName`), AUTH-15, la Foundation, el esquema, `RACKDUPLICAR`, `RACKLAYOUT` y los editores no cambian de politica.

## 2. Terminologia (caracterizada)

El prefijo de cada familia es su etiqueta corta de producto, la del BOM (`IRackKindHandler.BomLabel`), que coincide exactamente con la lista del Owner:
**Selectivo**, **Dinámico**, **Push Back**, **Cantilever**, **Cabecera**; para la cama de rodamiento la misma autoridad da **Cama** (se rechaza «Cama de
rodamiento N», la etiqueta larga del registro y de `RACKLISTA`, porque ninguna de las otras cinco familias usa su etiqueta larga; la palabra «cama» tambien nombra
un componente dentro de Dinamico y Push Back, pero el nombre de rack siempre va seguido de su numero). Una guarda fija que los prefijos de la autoridad pura
son iguales a los `BomLabel`: cambiar uno obliga a decidir el otro (el prefijo funciona como formato persistido en los dibujos).

## 3. Regla de asignacion (definicion a nivel de caracter)

- **Patron exacto** de una familia: sobre el nombre sin blancos exteriores (`string.Trim()`, es decir `Char.IsWhiteSpace`; `RACKLISTA` y AUTH-11 ya recortan
  el nombre igual), el texto es el prefijo, **un** espacio U+0020 y `N`, con `N` hecho solo de digitos ASCII `'0'..'9'`, sin cero a la izquierda y mayor o igual que 1 («Selectivo 0» y «Selectivo 03» no son el patron). El prefijo se
  compara con `StringComparison.OrdinalIgnoreCase`, sin normalizacion Unicode: «dinámico 3» cuenta; «Dinamico 3» (sin acento), un NBSP o un tabulador como
  separador, un espacio de ancho cero o una tilde descompuesta (NFD) NO son el patron.
- **N sin limite de tamaño:** se compara y se incrementa como cadena decimal (sin desbordamiento): «Selectivo 2147483647» → «Selectivo 2147483648»; un `N` de
  25 digitos cuenta y se incrementa igual.
- **Siguiente nombre** = `<Prefijo> <max + 1>`, `max` = el mayor `N` de los nombres logicos del dibujo que siguen el patron exacto de ESA familia (0 si ninguno).
  No se rellenan huecos (1, 2, 7 → 8). Los nombres personalizados no consumen numero salvo que sigan exactamente el patron.
- **Por texto sobre todos los racks** (interpretacion literal de la regla del Owner): un rack de cualquier tipo cuyo nombre siga exactamente «Selectivo N»
  consume ese N; asi un nombre automatico nuevo nunca repite uno del patron ya presente. Secuencias independientes por familia (prefijos distintos).
- **Alcance del escaneo:** todo lo que devuelve `ScanEnvelopes` (definiciones con sobre legible; incluidas las no colocadas y las dependientes de xref, que solo
  pueden elevar `max`). Una definicion borrada pero no purgada tambien cuenta hasta que se purga (se acepta: nunca genera un nombre repetido).
- **Estado, no contador:** se calcula del dibujo al insertar; no hay contador persistido. Cancelar el jig borra la definicion cancelada, asi que no consume
  numero (salvo el caso ya existente en que la limpieza del jig falle y quede una definicion huerfana, que se trata como cualquier definicion no colocada).

## 4. Cuando se aplica: por procedencia «rack nuevo»

Se asigna **una sola vez por solicitud de creacion cuya procedencia es un rack logico nuevo**, antes de serializar el sobre y el diseño interno, y se comparte
por todas las vistas iniciales de esa solicitud. Nunca por procedencia «rack existente» (`RACKEDITAR` Actualizar o Insertar hermana), ni en `RACKPROYECTAR`,
proyeccion, duplicacion, layout, informes, abrir o guardar. En `main` las rutas con procedencia «rack nuevo» son:

| Familia | Ruta (Plugin) | Puerta de procedencia |
|---|---|---|
| Selectivo | `RackSelectivoCommands.DrawSelectiveView` (RACKSELECTIVO y menu RACKCAD; `RACKEDITAR` no pasa por aqui) | toda llamada |
| Dinamico | `RackDinamicoCommands.DrawDynamicView` (RSD, menu RACKCAD) | `source == null` (la hermana de `RACKEDITAR` pasa `source`) |
| Push Back | `RackPushBackCommands.DrawPushBackView` (RPB, menu RACKCAD) | `source == null` |
| Cantilever | `RackCantileverCommands.DrawCantileverView` (RCT, menu RACKCAD; la insercion de componentes no pasa por aqui) | `source == null` |
| Cabecera | `RackCabeceraCommands.DrawAndPlace` (RACKCABECERA, QUICKCABECERA, menu RACKCAD) | toda llamada |
| Cama | QUICKCAMA y el caso `FlowBedInsertionRequest` del menu RACKCAD | toda llamada |

Aplicacion por familia (N-9): el nombre asignado reemplaza la entrada de nombre de la ruta antes de todo uso, de modo que el sobre, las copias persistidas y el
nombre del bloque salen coherentes: Selectivo lo pasa a `SerializeSelectiveDesign` y a la anotacion; Dinamico y Push Back a `system.Name` y al sobre; Cantilever
al sobre y a `design.Name` cuando el nombre se asigno automaticamente; Cabecera a `configuration.Name` antes de `BuildCabeceraPayload`; Cama: en el menu, un unico
nombre local para el sobre y para el nombre del bloque antes de `BuildCamaPayload`; en QUICKCAMA el mismo nombre va al sobre y a `DrawAndPlace` como nombre del rack.

**Sin asignar** = nulo, vacio o solo blancos. Para la **cabecera**, ademas, un nombre igual (recortado, `OrdinalIgnoreCase`) al de una plantilla integrada de
`RackFrameTemplateCatalog` («Estandar (3 paneles)», «Compacta (2 paneles)», «Alta (4 paneles, X)») cuenta como sin asignar en **toda** ruta de creacion de
cabecera, incluida la biblioteca: ese nombre describe el tipo, no identifica al rack, y hoy toda cabecera nueva nace con el. Consecuencias aceptadas: un usuario
que escriba exactamente el nombre de una plantilla integrada obtiene «Cabecera N»; una cabecera nombrada como una plantilla de usuario o del JSON editable
(`header-templates.json`, `user-templates.json`) conserva ese nombre (solo cuentan las plantillas integradas).

Cualquier otro nombre (escrito por el usuario o traido por un diseño de biblioteca) se respeta tal cual. Biblioteca «como nuevo» por familia: Selectivo conserva
el nombre del documento o recibe el automatico si esta vacio; Dinamico, Push Back, Cantilever y Cama conservan el nombre de la entrada (el nombre del archivo);
Cabecera sigue la regla de plantillas.

Consecuencias aceptadas (no cambian politicas): el nombre del bloque de la vista nueva deriva del nombre logico como hoy (AUTH-11 sin cambios), p. ej.
«Selectivo 3» en lugar de «Selectivo», y `RACKEDITAR` puede renombrar esos bloques como hoy (`RackBlockRenamer.SyncName`); la anotacion opcional «Colocar nombre de
rack» (desmarcada por defecto) dibuja el nombre automatico si se marca.

**Disparador de enmienda (I-55):** si I-55 se integra en `main` antes que I-60 (el Owner ordena no integrar I-60 antes), rebasar I-60 sobre ese `main` exige una
enmienda `A-n` antes del Candidato: la ruta de lote de I-55 (`RackViewBatchProducts.*` con `RackProductSourceKind.NewRack`, primera vista libre y cola de vistas)
asigna una vez antes de preparar el lote, aplica el nombre a `RackName` (y a `Configuration.Name` en la cabecera) y lo comparte por todas las vistas; nunca con
`ExistingRack`; con RED nuevo y las filas OV de varias vistas iniciales desde el menu y de `RACKPROYECTAR` sobre un rack heredado sin nombre.

## 5. Lo que NO cambia

- **Racks heredados sin nombre**: siguen validos y «(sin nombre)»; no se renombran al abrir, guardar, `RACKLISTA`, `RACKBOMTOTAL`, `RACKEDITAR` (Actualizar ni
  Insertar hermana) ni al proyectar. Ninguna ruta con procedencia «rack existente» llama a la asignacion.
- **Varias vistas iniciales:** en `main` la creacion inserta una sola vista y las hermanas llegan por `RACKEDITAR` → Insertar con el mismo Id y nombre.
- **`RACKDUPLICAR`** (autorizacion del Owner para dejarlo sin cambios si la ambiguedad persiste) y **`RACKLAYOUT` independiente**, que es duplicacion en lote bajo el
  mismo contrato de reestampado (I-47 G14, «Rack <celda>», «<base> <celda>»), por analogia: ambos ya dan nombres no vacios; si una copia debe recibir un nombre
  automatico es una ambiguedad de producto sin resolver: **FUERA DE ALCANCE**, sin cambios, visible en la validacion (filas N-12 y N-14).
- Renombrar con `RACKEDITAR` no cambia el RackId. `RACKEDITAR` de la cama puede hoy borrar el nombre: comportamiento vigente, fuera de alcance.

## 6. Obligaciones invariante → prueba (RED antes de implementar)

| ID | Invariante | Prueba | RED esperado |
|---|---|---|---|
| N-1 | dibujo vacio → 1; con 1 → 2; 1, 2, 7 → 8; 2147483647 → 2147483648; 25 digitos → +1 sin excepcion | autoridad pura | falla (no existe) |
| N-2 | mayusculas y blancos exteriores; `OrdinalIgnoreCase` sin normalizacion | autoridad pura | falla |
| N-3 | no cuentan: «Rack A», «Selectivo», «Selectivo A», «Selectivo 03», «Selectivo  2», NBSP, tabulador, espacio de ancho cero, NFD, «Dinamico 3» | autoridad pura | falla |
| N-4 | familias independientes; el texto de cualquier rack cuenta; prefijos = `BomLabel` | autoridad pura + guarda | falla |
| N-5 | sin asignar: nulo/vacio/blancos; cabecera: plantillas integradas (tambien si vienen de biblioteca o escritas); otro nombre se respeta | autoridad pura | falla |
| N-6 | cada ruta con procedencia «rack nuevo» asigna antes de serializar, con su puerta; ninguna ruta de edicion, duplicacion, layout, informe o proyeccion | guardas de fuente | falla |
| N-7 | el escaneo solo lee (sin `OpenMode.ForWrite` ni escritura de Xrecord) y usa la autoridad pura | guarda de fuente | falla |
| N-8 | legado sin nombre sigue «(sin nombre)»; guardar y reabrir conserva el nombre asignado | pruebas puras | pasa (se conserva) |
| N-9 | copias persistidas coherentes al crear: Selectivo (documento), Cabecera (`configuration.Name` antes del sobre), Cantilever (`design.Name` al asignar); cama: sobre y bloque con el mismo nombre | guardas de fuente | falla |

## 7. Matriz de validacion del Owner (OV-I60)

En la evidencia de la unidad §3 (filas `N-01..N-16`).
