# Indice de contexto RackCad

Este indice resume como entender rapidamente el proyecto y cual es el estado del repo antes de continuar el desarrollo.

RackCad es un **plugin de AutoCAD** (.NET `net8.0-windows`, WPF) para **disenar y dibujar racks**. Ya **no es solo un configurador de cabeceras**: maneja **cuatro tipos de rack**, cada uno con su ventana editora, su dibujo en AutoCAD y **round-trip de edicion** sobre el DWG. El trunk de integracion es **`main`**; el trabajo se hace en ramas por iniciativa (`docs/WORKFLOW.md`). El estado vivo (trabajo reciente, bugs conocidos, siguientes tareas y la última corrida real de tests) se mantiene en `docs/HANDOFF.md`.

## Los cuatro tipos de rack

El comando `RACKCAD` abre el menu principal (`RackMainMenuWindow`) desde donde se elige el tipo. Cada tipo tiene ademas su comando directo. Comandos en `src/RackCad.Plugin/RackFrameCommands.cs`.

1. **Cabecera (marco)** — `RackFrameConfiguratorWindow`. Un marco = 2 postes + placas base + celosia. Horizontales = fuente de verdad; paneles derivados. El **peralte de la placa base es editable por placa** (`BasePlatePlacement.PeralteOverride`; `null` = derivado, con `StandardPeralte` = base + slope * peralte_poste). Comandos: `RACKCABECERA`, `QUICKCABECERA`.
2. **Sistema dinamico (pallet flow)** — `RackDynamicSystemWindow`. Matriz frente x nivel, BFR/largueros IN/OUT, cama y apoyos en lateral, cortes frontal salida/entrada, planta sin camas, seguridad y cotas multivista. Comando `RACKSISTEMADINAMICO`.
3. **Cama de rodamiento (flow bed)** — `RackFlowBedWindow` (riel + rodillos + frenos + tope; dinamica o pushback). Comando `QUICKCAMA`.
4. **Selectivo (editor avanzado pallet-driven)** — `RackSelectiveWindow`. Matriz **frentes x niveles** (el termino es "frente", no "bahia"). Cada celda = tarima (frente/alto) + tarimas por nivel + larguero + peralte de larguero (combo desde la lista `peraltes` del catalogo). Reglas de geometria en `SelectiveGeometryResolver` (largueros por troquel, claro/separacion por tarima+holgura, altura por frente desde el escalon, datum de piso en Y=0, larguero a piso por frente); overrides manuales opcionales por celda. Cada poste (N frentes -> N+1 postes) puede referenciar una **cabecera por poste** (`RackFrameConfiguration` embebida), de donde sale su placa/peralte. Soporta **doble profundidad** (espalda con espalda, Fase 1): N fondos (`DepthCount`, 1..4), niveles por fondo (`ExtraFondoBays`/`FondoBays`) y separadores por hueco (`SeparatorLengths`); cada fondo tiene su propio numero de frentes (layout en esquina; `BayCountBox` habilitado en cualquier fondo): el **fondo mas largo define la rejilla horizontal compartida** y los mas cortos son un **prefijo** de ella (`SelectiveDepthLayout.MasterFondoIndex`/`MasterGrid`); un frente vacio = columna (si el frente maestro es una columna vacia, su ancho lo da la bahia real mas ancha de los otros fondos en ese indice), y lateral/planta dibujan todos los fondos con BOM que suma el contenido real de cada uno. Ademas cada fondo tiene su **propio fondo de tarima** (`ExtraFondoDepths`/`FondoDepths`), el marco dibujado usa **fondo de cabecera = fondo de tarima − 6"** (`CabeceraFondoAllowance` / `SelectiveDepthLayout.CabeceraDepthOfFondo`) con override por linea (`CabeceraFondoOverrides`/`FondoCabeceraOverrides`), la **frontal se inserta por fondo** (fondo en `Section`) y el editor trae un toggle **Frontal/Lateral** con vista previa lateral esquematica. El **"medio frente" ya esta hecho, generalizado a N tramos**: un frente se parte en N tramos con N-1 postes intermedios y el ultimo tramo calculado (`SelectiveMedioFrente.Resolve`; si los tramos no caben se dibuja el frente completo), boton "Medio frente..." por frente (`SelectiveSegmentsWindow`) y round-trip via `SelectiveSegmentDocument` (+ fallback del `MedioFrenteLength` legacy); el bloque separador fisico entre fondos ya se dibuja en lateral y planta y entra al BOM (componente "Separador"; en la frontal solo se deja el hueco, a proposito). BOM por **componentes** (`BomComponent`: cabeceras + largueros como componentes expandibles a piezas) con `SelectiveBomBuilder` + `RackBomWindow` (arbol + CSV a dos niveles); `RACKBOMTOTAL` da el BOM consolidado de TODO el dibujo (desglose por rack x copias + gran total por componente, `RackConsolidatedBomWindow`, export CSV) y hay editor de larguero (`RackLargueroWindow`, menu "Disenar larguero"; solo visual/BOM, sin bloque de AutoCAD aun). Comando `RACKSELECTIVO`.

## Identidad y round-trip (los cuatro tipos)

- Cada rack dibujado = **una definicion de bloque**; las copias son referencias a ella.
- En la **definicion** del bloque se embebe (diccionario de extension, Xrecord troceado <=255, `RackBlockData`) un sobre unificado `RackEmbedDocument { SchemaVersion, Kind, View, Section, Id (GUID), Name, Design (JSON del diseno) }`. Kinds: `selective`, `dynamic`, `cabecera`, `cama`. Views: `frontal`, `lateral`, `planta`. `Section` identifica el fondo/corte: en frontal dinamico 0 = salida y 1 = entrada; en lateral dinamico/selectivo es el indice de poste; `-1` = vista no seccionada o legacy.
- Comando `RACKEDITAR`: seleccionas un rack -> lee el sobre -> **despacha por Kind** -> reabre el editor correcto precargado (`LoadExisting`) -> al confirmar **redefine la definicion en sitio** (`RedefineSystemBlock` + Regen) => todas las copias se actualizan a la vez, ninguna se mueve ni se recoloca. Ademas **redibuja TODAS las vistas** del sistema (frontal/lateral/planta), encontradas por GUID escaneando las definiciones de bloque.
- **Botones Actualizar / Insertar (convencion permanente en las cuatro ventanas):** **Actualizar** = redibuja en sitio las vistas existentes (solo edicion); **Insertar {vista}** = agrega una vista NUEVA enlazada (mismo GUID) y refresca las demas. La cama independiente conserva una sola vista; selectivo y dinamico habilitan las vistas enlazadas que correspondan. **`RACKDUPLICAR`** hace una copia **independiente** (GUID nuevo), distinta del `COPY` de AutoCAD (que comparte definicion/GUID y edita todas las copias juntas).
- Comando `RACKLISTA`: tabla de todos los racks del dibujo (nombre, tipo, vistas presentes, numero de copias; `RackListBuilder` agrupa los sobres por GUID) con zoom a la primera referencia en el modelo del rack elegido (frontal si existe).
- El nombre "Rack A" (campo en cada editor) = nombre del bloque; el GUID va en el sobre (evita colisiones).
- Stores del diseno: `SelectivePalletDesignStore` (selectivo), `RackProjectStore` (dinamico/cabecera), `FlowBedConfigurationStore` (cama). Servicios de dibujo/redraw: `SelectiveFrontalDrawService`, `SelectivePlantaDrawService`, `DynamicSystemDrawService`, `DynamicFrontalDrawService`, `DynamicPlantaDrawService`, `FlowBedDrawService`, `LateralHeaderDrawService`, `PlantaHeaderDrawService`.
- Escalable: agregar un tipo nuevo = su Kind + `Edit<Kind>` en `RackFrameCommands` + `LoadExisting` en su ventana + embed/`RedrawInPlace` en su draw service.

## Lectura recomendada

0. `docs/HANDOFF.md`
   - Estado actual y continuidad entre sesiones (leer SIEMPRE primero). Convenciones estables: `AGENTS.md`.

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

6. `docs/despliegue.md`
   - Instalacion, bundle del Autoloader, catalogos en la maquina destino y como compartir la app.

7. `docs/ideas-futuras.md`
   - Backlog priorizado + deuda tecnica diferida CON evidencia. **Leer antes de proponer refactors o
     features nuevas**: evita re-descubrir hallazgos ya diagnosticados o re-proponer trabajo diferido a proposito.

8. `docs/auditoria-arquitectura-2026-07.md`
   - Auditoria arquitectonica completa (2026-07-16): hallazgos verificados y arquitectura objetivo.

9. `docs/WORKFLOW.md`
   - Proceso de desarrollo: ramas POR INICIATIVA (ADR-0001), worktrees, integracion y trabajo
     simultaneo de varios agentes (Claude, Codex) y humanos.

10. `docs/ROADMAP.md`
    - Plan de ejecucion por fases e iniciativas independientes (con dependencias y estado).

11. `docs/adr/`
    - Decisiones de arquitectura (ADR): proceso en `adr/README.md`, una decision por archivo.

## Catalogos y datos

Los catalogos viven en `assets/catalogs/*.csv` (mas `defaults.json` y `header-templates.json`) y los carga `JsonRackCatalogProvider` a `RackCatalog`:

- `secciones.csv` — **TODOS los perfiles estructurales en una hoja** con columna `rol` (POSTE | CELOSIA |
  LARGUERO | SEPARADOR). Los refuerzos son postes; horizontales + diagonales comparten la celosia; los largueros llevan
  `peraltes` (valores permitidos) y `mensula` (FK). El provider los separa en las tres listas de siempre
  (`PostProfiles`/`TrussProfiles`/`BeamProfiles`), asi que el codigo consumidor no cambio; los tres CSV
  legacy siguen leyendose como fallback si `secciones.csv` no existe.
- `mensulas.csv` — mensulas.
- `base-plates.csv` — `peralteBase` / `peraltePorPeraltePoste` -> `StandardPeralte`.
- `connection-points.csv` + `connection-layout.csv` — puntos de conexion parametricos en X y Y (X = localX + localXPorParam * valor(paramX); Y = localY + localYPorParam * valor(paramY)).
- `blocks.csv` (bloque por pieza y vista), `views.csv`, `flow-bed-profiles.csv`.
- `seguridad.csv` — elementos de seguridad (bota, protector lateral, tope, desviador, defensa de montacargas, guia de entrada y parrilla) con costo/moneda/unidad.

Ya no existen `diagonal-profiles.csv` ni `reinforcement-profiles.csv`. Persistencia de proyecto: `RackProjectStore` -> `.rackcad.json`.

Documentos de referencia para el modelo de datos:

- `docs/catalogos-y-plantillas.md` — como editar los catalogos (CSV/Excel) y plantillas (JSON).
- `docs/modelo-de-datos.md` — como se conectan las tablas (FK) y como se cargan (`RackCatalog`), con diagrama ASCII + Mermaid.
- `docs/generacion-cabecera-lateral.md` — logica block-based de la cabecera lateral anclada a puntos de conexion.

## Vistas

Todas implementadas y ligadas por GUID. El **selectivo** tiene tres vistas: **frontal** (un bloque: postes + placas + largueros por nivel), **lateral** (cortes: un bloque por poste — cada corte es la cabecera del poste en perfil + las secciones de largueros frente/atras por nivel; al insertar se pregunta que corte por numero de poste y se coloca con jig) y **planta** (un bloque: una cabecera-planta por frente apilada en Y + largueros frente/atras por bahia; X = fondo, Y = frente). La **cabecera** tiene dos: **lateral** y **planta** (2 huellas de poste + placas + celosia colapsada a un miembro con longitud = A-corte del travesano). El **dinamico** tiene lateral, frontal salida, frontal entrada y planta; la **cama independiente** conserva lateral. Las vistas adicionales SOLO se insertan desde `RACKEDITAR` de una vista existente (los botones se deshabilitan con tooltip si no aplica), para que nunca queden huerfanas.

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

Cuatro tipos de rack (cabecera, dinamico, cama, selectivo) con ventana editora, dibujo en AutoCAD y round-trip de edicion; la suite completa de tests esta verde (conteo real: `docs/HANDOFF.md` seccion 12). El selectivo ya dibuja frontal, lateral y planta; la cabecera, lateral y planta.

La carpeta de salida `bin/`, `obj/`, caches locales `.dotnet_home`, `.nuget_packages`, `.appdata` y `.localappdata` no son parte logica del codigo fuente y estan ignoradas por `.gitignore`.
