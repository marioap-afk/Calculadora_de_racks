# I-60 — Freeze: nombre lógico automático de los racks nuevos

Frozen: NO

Unit: `I-60`. Workflow: V2. Archetype: EXTENSION de producto. Rama `feature/nombre-automatico-racks`, base `1304101d`.
Contrato: [I-60-nombre-automatico-racks.md](I-60-nombre-automatico-racks.md). Decisiones: [decisions/I-60.md](../automation/decisions/I-60.md).
Caracterizacion (Discovery): [evidencia](../automation/evidence/I-60-evidence.md) §1.

Una vez congelado es inmutable; todo cambio posterior es una enmienda `A-n` en el registro de decisiones.

## 1. Autoridad

- **Nombre logico** = `RackEmbedDocument.Name` (y la copia que cada familia guarda en su diseño interno: `SelectivePalletDesignDocument.Name`,
  `DynamicRackSystem.Name`, `PushBackSystem.Name`, `CantileverLineDesign.Name`, `RackFrameConfiguration.Name`; la cama no tiene copia interna). NO es el
  nombre de la definicion de bloque, ni el BaseName de AUTH-11, ni el RackId. El RackId sigue siendo la identidad; el nombre es metadato editable.
- **Autoridad pura nueva en Application** (sin tipos de AutoCAD): recibe la familia y los nombres logicos existentes del dibujo y devuelve el siguiente nombre.
  El Plugin solo aporta el escaneo del dibujo (`RackBlockFinder.ScanEnvelopes`, lectura) y aplica el resultado en las rutas de creacion. Nombres de simbolos no
  congelados.
- AUTH-11 (`RackViewBaseName`), AUTH-15, la Foundation, el esquema, `RACKDUPLICAR`, `RACKLAYOUT` y el editor de cada sistema NO cambian de politica.

## 2. Terminologia (caracterizada)

El prefijo de cada familia es su etiqueta corta de producto, la que ya muestra el BOM (`IRackKindHandler.BomLabel`), que coincide exactamente con la lista del
Owner: **Selectivo**, **Dinámico**, **Push Back**, **Cantilever**, **Cabecera**; y para la cama de rodamiento, la misma autoridad da **Cama**. La etiqueta larga
del registro («Sistema dinámico», «Cama de rodamiento») y la de `RACKLISTA` (que muestra `pushback` crudo) no se usan como prefijo. Una guarda fija que los
prefijos de la autoridad pura son iguales a los `BomLabel` del Plugin.

## 3. Regla de asignacion

- **Patron exacto** de una familia: el nombre, sin espacios al principio ni al final, es `<Prefijo> <N>` con UN espacio y `N` entero positivo sin ceros a la
  izquierda (`[1-9][0-9]*`), comparando el prefijo **sin distinguir mayusculas** (cultura invariante) y con sus acentos («Dinamico 3» sin acento NO es el patron).
- **Siguiente nombre** = `<Prefijo> <max + 1>`, donde `max` es el mayor `N` de los nombres logicos del dibujo que siguen el patron exacto de ESA familia (0 si no
  hay ninguno). No se rellenan huecos (1, 2, 7 → 8). Los nombres personalizados no consumen numero salvo que sigan exactamente el patron. El patron se lee sobre
  **todos** los racks del dibujo por su texto (un rack de otro tipo renombrado «Selectivo 7» tambien consume el 7), de modo que el nombre nuevo nunca repite un
  nombre del patron ya presente. Secuencias independientes por familia.
- **Alcance del escaneo:** todas las definiciones que devuelve `ScanEnvelopes` (incluidas las no colocadas y las dependientes de xref); solo pueden elevar `max`.
- **Estado, no contador:** se calcula del dibujo en el momento de insertar; no hay contador persistido. Cancelar antes de crear no deja nada (el jig ya borra la
  definicion cancelada) y no consume numero.

## 4. Cuando se aplica (solo racks logicos NUEVOS)

Se aplica una sola vez por rack logico nuevo, en la ruta de creacion del Plugin, **antes** de serializar el sobre y el diseño interno (asi la vista, su diseño y
el nombre del bloque salen coherentes), y solo si el nombre pedido esta **sin asignar**:

| Familia | Ruta de creacion (Plugin) | Puerta |
|---|---|---|
| Selectivo | `RackSelectivoCommands.DrawSelectiveView` (solo creacion: RACKSELECTIVO y el menu RACKCAD; `RACKEDITAR` no pasa por aqui) | siempre |
| Dinamico | `RackDinamicoCommands.DrawDynamicView` | `source == null` (el sibling de `RACKEDITAR` pasa `source`) |
| Push Back | `RackPushBackCommands.DrawPushBackView` | `source == null` |
| Cantilever | `RackCantileverCommands.DrawCantileverView` | `source == null` (la insercion de componentes no pasa por aqui) |
| Cabecera | `RackCabeceraCommands.DrawAndPlace` (RACKCABECERA, QUICKCABECERA y el menu) | siempre (solo creacion) |
| Cama | QUICKCAMA y el caso `FlowBedInsertionRequest` del menu | siempre (solo creacion) |

**Sin asignar** = nulo, vacio o solo espacios. Para la **cabecera** tambien es sin asignar un nombre igual (sin espacios exteriores, sin distinguir mayusculas) al
nombre de una plantilla del catalogo `RackFrameTemplateCatalog` («Estandar (3 paneles)», «Compacta (2 paneles)», «Alta (4 paneles, X)»): hoy toda cabecera nueva
nace con el nombre de su plantilla, que describe el tipo y no identifica al rack; asi una cabecera nueva recibe «Cabecera N». Un nombre que el usuario escribe
(o que trae un diseño abierto de la biblioteca) se respeta tal cual.

Consecuencias aceptadas (no cambian politicas): el nombre del bloque de la vista nueva deriva del nombre logico como hoy (AUTH-11 sin cambios), p. ej.
«Selectivo 3» en lugar de «Selectivo»; y la anotacion opcional «Colocar nombre de rack» (desmarcada por defecto) dibuja el nombre automatico si se marca.

## 5. Lo que NO cambia

- **Racks heredados sin nombre**: siguen validos y «(sin nombre)»; no se renombran al abrir, guardar, `RACKLISTA`, `RACKBOMTOTAL`, `RACKEDITAR` (Actualizar ni
  Insertar hermana), `RACKPROYECTAR` ni al proyectar. Ninguna ruta de edicion llama a la asignacion.
- **Varias vistas iniciales:** en `main` la creacion inserta una sola vista; las hermanas se agregan despues con `RACKEDITAR` → Insertar, que ya reutilizan el
  mismo RackId y el mismo nombre. Asi todas las vistas de un rack comparten el nombre por construccion. (La primera vista libre y la cola de vistas de I-55 no
  estan en `main`; al integrarse I-55 sus rutas de creacion se reconcilian con esta regla.)
- **Biblioteca / abrir un diseño como nuevo:** conserva el nombre de la plantilla (no vacio; editable).
- **`RACKDUPLICAR` y `RACKLAYOUT` independientes:** ya dan nombres no vacios («X - copia», «Rack - copia», «Rack B3»), fijados por el contrato de I-51 (PD-5) y sus
  pruebas; si una copia debe recibir un nombre automatico es una ambiguedad de producto sin resolver: **FUERA DE ALCANCE**, sin cambios.
- Renombrar con `RACKEDITAR` no cambia el RackId (comportamiento vigente).
- `RACKEDITAR` de la cama puede hoy borrar el nombre (escribe el campo tal cual): comportamiento vigente, fuera de alcance.

## 6. Obligaciones invariante → prueba (RED antes de implementar)

| ID | Invariante | Prueba | RED esperado |
|---|---|---|---|
| N-1 | dibujo vacio → «Selectivo 1»; con «Selectivo 1» → «Selectivo 2»; 1, 2, 7 → 8 | autoridad pura | falla (no existe) |
| N-2 | mayusculas: «selectivo 2», «SELECTIVO 5» cuentan | autoridad pura | falla |
| N-3 | personalizados («Rack A», «Selectivo», «Selectivo A», «Selectivo 03», «Selectivo  2», «Dinamico 3») no cuentan | autoridad pura | falla |
| N-4 | familias independientes; prefijos = `BomLabel` | autoridad pura + guarda | falla |
| N-5 | sin asignar: nulo/vacio/espacios; cabecera: nombres de plantilla; un nombre escrito se respeta | autoridad pura | falla |
| N-6 | cada ruta de creacion llama a la asignacion antes de serializar, con su puerta; ninguna ruta de edicion, duplicacion ni layout la llama | guardas de fuente | falla |
| N-7 | el escaneo solo lee (sin escritura, sin Commit de cambios) | guarda de fuente | falla |
| N-8 | legado sin nombre: `RACKLISTA` sigue «(sin nombre)»; guardar y reabrir conserva el nombre asignado | pruebas puras existentes + ida y vuelta del sobre | pasa (se conserva) |

## 7. Matriz de validacion del Owner (OV-I60)

En la evidencia de la unidad §3 (filas `N-01..N-13`): nombre de cada familia nueva, incremento, huecos, mayusculas, personalizados, cabecera con plantilla,
biblioteca, varias vistas por `RACKEDITAR`, legado sin nombre intacto, guardar/reabrir, renombrar conserva el RackId, `RACKDUPLICAR` sin cambios.
