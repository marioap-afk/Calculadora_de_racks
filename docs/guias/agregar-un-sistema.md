# Agregar un sistema de rack

Guía nacida en **I-18 (Push Back)**, escrita desde la experiencia real de añadir el cuarto sistema al producto.
Sustituye al apéndice temporal de [`ARCHITECTURE.md`](../ARCHITECTURE.md) §«agregar un `Kind`» (DOC-02 de I-06).

No es una lista de deseos: cada paso apunta al archivo que Push Back tocó de verdad, y las advertencias son
errores que costaron una ronda de corrección.

## 0. Antes de escribir código

1. **Decisiones del dueño primero.** Bloques DWG, filas de catálogo y reglas funcionales son datos, no
   suposiciones. Push Back arrancó con una fase **PB-0** dedicada solo a eso
   ([`decisions/I-18.md`](../automation/decisions/I-18.md)); todo lo que se saltó esa fase volvió como rechazo.
2. **Un sistema es una iniciativa**, con su rama, su worktree y su fila en el ROADMAP
   ([`WORKFLOW.md`](../WORKFLOW.md) §1).
3. **Decide qué NO es tuyo.** Push Back reutiliza la estructura del Dinámico entera; lo específico son los dos
   largueros de extremo, la cama y el tope. Cuanto más reutilices, menos golden propio mantienes.

## 0.bis Dónde vive cada archivo (I-23)

Desde **I-23**, cada sistema tiene su propio namespace y su propia carpeta en los tres proyectos, y la
regla es comprobable: `NamespaceFolderGuardTests` falla si un archivo declara un namespace que no
corresponde a su carpeta, si queda suelto en la raíz plana de `Systems/`, o si aparece una subcarpeta que
no es uno de los seis destinos.

```text
src/RackCad.Domain/Systems/<Sistema>/         RackCad.Domain.Systems.<Sistema>
src/RackCad.Application/Systems/<Sistema>/    RackCad.Application.Systems.<Sistema>
src/RackCad.Plugin/Systems/<Sistema>/         RackCad.Plugin.Systems.<Sistema>
```

`<Sistema>` es `Selective`, `Dynamic`, `PushBack`, `FlowBed`, `Larguero` o `Shared`. **Un sistema nuevo
añade su carpeta y necesita antes su fila en el contrato de la iniciativa**: la guarda lo exige.

Dos cosas que NO son de tu sistema:

- `RackCad.Application.Drawing` — el vocabulario de materialización que consumen todos
  (`HeaderBlockInstance`, `LateralHeaderLayout`, `HeaderRunPlan`, `HeaderInstanceGrouper`). Tus builders
  producen instancias de aquí; no declares un plan propio.
- `RackCad.Application.RackFrames` y `RackCad.Domain.RackFrames` — la cabecera física.

Un archivo va al sistema que su tipo de primer nivel **nombra y modela**. **Consumir** un contrato de otro
sistema no lo mueve: componer está permitido y es lo que se espera (ver §0.3). Solo va a `Shared` lo que es
neutral en nombre **y** en contenido.

## 1. Dominio (`RackCad.Domain`)

- Un `*Defaults` con las constantes del sistema (`PushBackDefaults`): ids de pieza, vista de sus puntos,
  valores por regla explícita. **Nunca "el primer valor del catálogo"**: Push Back documenta por qué el peralte
  por defecto es 3.5 y no el primero de la lista.
- Los DTO de diseño con **campos nullable y fallback legacy** (`PushBackDesign`, `PushBackRearTopeConfig`).
- Añade el valor al final de `RackSystemKind` — los previos quedan congelados (I-08).

## 2. Aplicación (`RackCad.Application`)

Aquí vive **toda** la geometría, el BOM y la persistencia; puros y con pruebas.

- **Resolver**: diseño → sistema resuelto (`PushBackResolver`), con `Snapshot` canónico de vuelta, de modo que
  `Design → Resolve → Snapshot → Resolve` sea estable.
- **Geometría por pieza**, en funciones nombradas y testeables (`PushBackFlowBedGeometry`,
  `PushBackLoadBeamGeometry`, `PushBackRearTopeBuilder`).
- **Builders de vista** que **componen** los del sistema base como caja negra: quita por `Role`/`PieceId`,
  agrega lo tuyo y reagrupa. Push Back nunca modificó los builders del Dinámico.
- **BOM** propio (`PushBackBomBuilder`), contando piezas del sistema resuelto, no del dibujo.
- **Estado puro del editor** (`PushBackEditorState`, `PushBackEditorDesignAssembler`): sin WPF ni AutoCAD.

### La regla que más caro sale saltarse

> **Consume geometría resuelta; no la recalcules, y colócala por puntos MEDIDOS del catálogo.**

Push Back perdió tres rondas por colocar piezas con offsets deducidos en vez de con
`connection-layout.csv`. El patrón correcto:

1. Lee la fila del punto por **nombre y vista** (`FindConnectionLayout(pieza, punto, vista)`).
2. **Si la fila no existe, no dibujes la pieza.** Nunca caigas al *insertion point* ni inventes un offset: un
   mate ausente es un contrato físico ausente. Detente y pide la medición.
3. Resuelve el punto con sus parámetros (`SelectivePostGeometry.Resolve`, que aplica `…PorParam`).
4. Transfórmalo al mundo con la colocación de **su propia pieza** (mirror y rotación).
5. **Verifica numéricamente** la coincidencia después de construir la instancia.

Dos matices que costaron rondas enteras:

- **Ancla contra la pieza correcta.** El tope de Push Back se anclaba a la inserción del *larguero* cuando su
  punto es del *poste*. Compilaba, dibujaba y estaba mal.
- **Algunos bloques matean por su ORIGEN.** Si el bloque no publica punto propio, puede ser que su inserción
  *sea* el mate (así es `LARGUERO_ESCALON_TOPE_DE_3`). Confírmalo con el dueño antes de asumir que falta una
  fila.

## 3. UI (`RackCad.UI`)

- **Compón sobre `RackEditorVisualShell`** (ADR-0019): slots para sidebar, matriz, preview, status y las cuatro
  categorías de acciones. Nada de heredar ventanas.
- **Sigue el patrón estructural del editor Dinámico**, no lo copies: `SectionTitle` + `FieldLabel` sobre control
  a ancho completo, grids de dos columnas con gutter de 10, y un panel resaltado que separa los datos del
  **frente** de los de la **celda**, cada uno con sus alcances.
- **Una sola autoridad por dato.** Si un valor se edita en dos sitios, uno de los dos miente. El tope de Push
  Back terminó existiendo **solo** dentro de Seguridad, y se sacó del buffer de celda para que ningún alcance
  pudiera transportarlo por accidente.
- **Reutiliza los diálogos compartidos.** `SelectiveSafetyWindow` acepta una sección externa
  (`extraSection`) y sigue siendo neutral: Push Back le inyecta su sección de topes sin que la ventana conozca
  el sistema.
- **Preview**: usa la infraestructura compartida (`Preview/EditorPreviewSurface`, `EditorPreviewParts`,
  `EditorPreviewPalette`). No escribas un painter nuevo.

## 4. Plugin (`RackCad.Plugin`)

Único proyecto que toca AutoCAD.

- **Draw services** delgados por vista sobre `ViewBlockDraw`.
- **Comando + alias** (`RACKPUSHBACK` / `RPB`), que solo orquesta: la ventana y su sesión son la autoridad.
- **Handler** (`IRackKindHandler`) y su registro en `KindHandlerRegistry`: con eso `RACKEDITAR`,
  `RACKBOMTOTAL`, `RACKDUPLICAR` y `RACKLAYOUT` adoptan el sistema **sin una sola rama nueva**.
- **Edición multivista**: preflight completo antes de tocar geometría, un único `Regen`, y el mismo GUID en
  todas las vistas ligadas.

## 5. Persistencia

- Slot aditivo en `RackProjectDocument` **sin subir el major**.
- `Kind` propio en el envelope; `RackEmbedStore`/`RackEmbedComposer` no se tocan.
- **Preserva la metadata desconocida y la versión** (I-11) en los cuatro límites, con `WithSourceMetadataFrom`.
- Prueba el round-trip **con un documento antiguo real**, no solo con uno recién creado.

## 6. Pruebas que de verdad protegen

- **Golden con SHA-256** de una firma detallada (vista, rol, pieza, bloque, inserción, ancla, rotación, mirror
  y parámetros ordenados) para cada vista y para el BOM. Cuando cambies geometría a propósito, **re-fija solo
  los pins afectados y explica en el comentario por qué se movieron y por qué los demás no**: esa nota es la
  que acota el cambio.
- **Regresión verificada**: desactiva el fix y comprueba que la prueba falla. Una prueba que nunca se vio
  fallar no prueba nada (AGENTS.md).
- **Pruebas numéricas de mate**: transforma ambos lados y compara coordenadas mundiales.
- **Guards de fuente** por texto para las reglas que el compilador no puede vigilar (que no aparezcan ramas por
  `RackSystemKind`, que un comando no se duplique).
- Cuidado con **fijar valores que dependen del entorno**: una firma de escena WPF depende del layout y del DPI
  del agente y romperá la CI. Afirma invariantes portables y deja la medición documentada.

## 7. Cierre

- Validación manual del dueño en AutoCAD sobre el **DLL Debug del SHA exacto** con CI verde
  ([`validacion-manual-autocad.md`](validacion-manual-autocad.md)).
- Bundle Debug **reproducible** (`deploy/build-bundle.ps1`) con inventario y verificación fail-closed.
- Evidencia, estado y decisiones al día; HANDOFF y ROADMAP como **último commit de la rama**
  ([`WORKFLOW.md`](../WORKFLOW.md) §4.5.4).

## 8. Lo que Push Back haría distinto

1. **Pedir las mediciones al principio.** Casi todas las rondas de corrección fueron por colocar piezas sin el
   punto medido delante.
2. **Preguntar de qué pieza es cada punto**, y si el bloque matea por su origen. Dos preguntas de un minuto.
3. **No dejar dos caminos para el mismo dato.** El tope vivía en cuatro sitios de la UI a la vez.
4. **Extraer lo compartido antes de duplicarlo**, con una caracterización previa que permita demostrar
   equivalencia — no afirmarla.
