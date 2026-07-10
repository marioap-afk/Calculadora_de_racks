# Indice de contexto RackCad

Este indice resume como entender rapidamente el proyecto y cual es el estado del repo antes de continuar el desarrollo.

RackCad es un **plugin de AutoCAD** (.NET `net8.0-windows`, WPF) para **disenar y dibujar racks**. Ya **no es solo un configurador de cabeceras**: maneja **cuatro tipos de rack**, cada uno con su ventana editora, su dibujo en AutoCAD y **round-trip de edicion** sobre el DWG. Rama de trabajo: `release/claude-review` (255 tests verdes).

## Los cuatro tipos de rack

El comando `RACKCAD` abre el menu principal (`RackMainMenuWindow`) desde donde se elige el tipo. Cada tipo tiene ademas su comando directo. Comandos en `src/RackCad.Plugin/RackFrameCommands.cs`.

1. **Cabecera (marco)** — `RackFrameConfiguratorWindow`. Un marco = 2 postes + placas base + celosia. Horizontales = fuente de verdad; paneles derivados. El **peralte de la placa base es editable por placa** (`BasePlatePlacement.PeralteOverride`; `null` = derivado, con `StandardPeralte` = base + slope * peralte_poste). Comandos: `RACKCABECERA`, `QUICKCABECERA`.
2. **Sistema dinamico (pallet flow)** — `RackDynamicSystemWindow`. Cabeceras a lo largo del tramo + separadores por nivel. Comando `RACKSISTEMADINAMICO`.
3. **Cama de rodamiento (flow bed)** — `RackFlowBedWindow` (riel + rodillos + frenos + tope; dinamica o pushback). Comando `QUICKCAMA`.
4. **Selectivo (editor avanzado pallet-driven)** — `RackSelectiveWindow`. Matriz **frentes x niveles** (el termino es "frente", no "bahia"). Cada celda = tarima (frente/alto) + tarimas por nivel + larguero + peralte de larguero (combo desde la lista `peraltes` del catalogo). Reglas de geometria en `SelectiveGeometryResolver` (largueros por troquel, claro/separacion por tarima+holgura, altura por frente desde el escalon, datum de piso en Y=0, larguero a piso por frente); overrides manuales opcionales por celda. Cada poste (N frentes -> N+1 postes) puede referenciar una **cabecera por poste** (`RackFrameConfiguration` embebida), de donde sale su placa/peralte. BOM (postes, placas, largueros, mensulas) con `SelectiveBomBuilder` + `RackBomWindow` (grid + export CSV). Comando `RACKSELECTIVO`.

## Identidad y round-trip (los cuatro tipos)

- Cada rack dibujado = **una definicion de bloque**; las copias son referencias a ella.
- En la **definicion** del bloque se embebe (diccionario de extension, Xrecord troceado <=255, `RackBlockData`) un sobre unificado `RackEmbedDocument { SchemaVersion, Kind, View, Section, Id (GUID), Name, Design (JSON del diseno) }`. Kinds: `selective`, `dynamic`, `cabecera`, `cama`. Views: `frontal`, `lateral`, `planta`. `Section` = indice del corte lateral del selectivo (`-1` = vista no seccionada).
- Comando `RACKEDITAR`: seleccionas un rack -> lee el sobre -> **despacha por Kind** -> reabre el editor correcto precargado (`LoadExisting`) -> al confirmar **redefine la definicion en sitio** (`RedefineSystemBlock` + Regen) => todas las copias se actualizan a la vez, ninguna se mueve ni se recoloca. Ademas **redibuja TODAS las vistas** del sistema (frontal/lateral/planta), encontradas por GUID escaneando las definiciones de bloque.
- **Botones Actualizar / Insertar (convencion permanente en las cuatro ventanas):** **Actualizar** = redibuja en sitio las vistas existentes (solo edicion); **Insertar {vista}** = agrega una vista NUEVA enlazada (mismo GUID) y refresca las demas. En racks de una sola vista (dinamico, cama) el boton alterna su etiqueta segun si la vista ya existe. **`RACKDUPLICAR`** hace una copia **independiente** (GUID nuevo), distinta del `COPY` de AutoCAD (que comparte definicion/GUID y edita todas las copias juntas).
- El nombre "Rack A" (campo en cada editor) = nombre del bloque; el GUID va en el sobre (evita colisiones).
- Stores del diseno: `SelectivePalletDesignStore` (selectivo), `RackProjectStore` (dinamico/cabecera), `FlowBedConfigurationStore` (cama). Servicios de dibujo/redraw: `SelectiveFrontalDrawService`, `SelectivePlantaDrawService`, `DynamicSystemDrawService`, `FlowBedDrawService`, `LateralHeaderDrawService`, `PlantaHeaderDrawService`.
- Escalable: agregar un tipo nuevo = su Kind + `Edit<Kind>` en `RackFrameCommands` + `LoadExisting` en su ventana + embed/`RedrawInPlace` en su draw service.

## Lectura recomendada

1. `README.md`
   - Vista rapida del proyecto, build y prueba con `NETLOAD`.

2. `docs/01-estado-actual-mvp.md`
   - Que funciona hoy, que no funciona todavia y como se usan los editores.

3. `docs/02-modelo-tecnico-vigente.md`
   - Modelo actual: horizontales como fuente de verdad, paneles derivados, miembros fisicos y excepciones.

4. `docs/03-guia-desarrollo-y-validacion.md`
   - Entorno, build, AutoCAD 2025, warnings conocidos y checklist de validacion.

5. `docs/04-roadmap-operativo.md`
   - Siguientes pasos recomendados sin mezclar dibujo, BOM, catalogos y persistencia antes de tiempo.

## Catalogos y datos

Los catalogos viven en `assets/catalogs/*.csv` (mas `defaults.json` y `header-templates.json`) y los carga `JsonRackCatalogProvider` a `RackCatalog`:

- `post-profiles.csv` — postes (los refuerzos son postes).
- `truss-profiles.csv` — **una sola lista de celosia** = horizontales + diagonales.
- `beam-profiles.csv` — largueros; la columna `peraltes` = valores permitidos (FK a mensula).
- `mensulas.csv` — mensulas.
- `base-plates.csv` — `peralteBase` / `peraltePorPeraltePoste` -> `StandardPeralte`.
- `connection-points.csv` + `connection-layout.csv` — puntos de conexion parametricos en X y Y (X = localX + localXPorParam * valor(paramX); Y = localY + localYPorParam * valor(paramY)).
- `blocks.csv` (bloque por pieza y vista), `views.csv`, `flow-bed-profiles.csv`, `spacers-profiles.csv`.

Ya no existen `diagonal-profiles.csv` ni `reinforcement-profiles.csv`. Persistencia de proyecto: `RackProjectStore` -> `.rackcad.json`.

Documentos de referencia para el modelo de datos:

- `docs/catalogos-y-plantillas.md` — como editar los catalogos (CSV/Excel) y plantillas (JSON).
- `docs/modelo-de-datos.md` — como se conectan las tablas (FK) y como se cargan (`RackCatalog`), con diagrama ASCII + Mermaid.
- `docs/generacion-cabecera-lateral.md` — logica block-based de la cabecera lateral anclada a puntos de conexion.

## Vistas

Todas implementadas y ligadas por GUID. El **selectivo** tiene tres vistas: **frontal** (un bloque: postes + placas + largueros por nivel), **lateral** (cortes: un bloque por poste — cada corte es la cabecera del poste en perfil + las secciones de largueros frente/atras por nivel; al insertar se pregunta que corte por numero de poste y se coloca con jig) y **planta** (un bloque: una cabecera-planta por frente apilada en Y + largueros frente/atras por bahia; X = fondo, Y = frente). La **cabecera** tiene dos: **lateral** y **planta** (2 huellas de poste + placas + celosia colapsada a un miembro con longitud = A-corte del travesano). Dinamico y cama dibujan **lateral**. Las vistas lateral/planta SOLO se insertan desde `RACKEDITAR` de una vista frontal/lateral existente (los botones se deshabilitan con tooltip si no aplica), para que nunca queden huerfanas.

## Documentos historicos existentes

Utiles para decisiones de arquitectura, pero mas extensos:

- `arquitectura-autocad-racks.md`
- `mvp-configurador-cabeceras.md`
- `modelo-datos-cabecera-rack-selectivo.md`
- `plan-implementacion-mvp-csharp-autocad.md`
- `analisis-macro-vba-cabeceras.md`

## Decisiones clave ya tomadas

- AutoCAD completo, no AutoCAD LT.
- AutoCAD .NET API.
- C# y Visual Studio.
- `net8.0-windows` para UI/plugin.
- WPF para las ventanas editoras (modales).
- El plugin ya dibuja en AutoCAD los cuatro tipos de rack, con round-trip de edicion (`RACKEDITAR`) que redefine el bloque en sitio.
- El usuario parte de una configuracion estandar y modifica excepciones.
- Las horizontales son entidades fisicas y fuente de verdad.
- Los paneles son espacios derivados entre horizontales consecutivas.
- Los offsets y puntos de conexion viven en catalogos/configuracion, no hardcodeados en el dibujo.

## Estado del repositorio

Cuatro tipos de rack (cabecera, dinamico, cama, selectivo) con ventana editora, dibujo en AutoCAD y round-trip de edicion; 255 tests verdes en `release/claude-review`. El selectivo ya dibuja frontal, lateral y planta; la cabecera, lateral y planta.

La carpeta de salida `bin/`, `obj/`, caches locales `.dotnet_home`, `.nuget_packages`, `.appdata` y `.localappdata` no son parte logica del codigo fuente y estan ignoradas por `.gitignore`.
