# I-06 — Auditoría documental

> Evidencia de la fase 1 de I-06. Corte: 2026-07-17, rama `docs/reestructura`,
> `origin/main` en `8e52828c5470af7f09b49a0b0cddce2a03ea3bbe`.
>
> Esta auditoría describe y propone. No mueve, borra, archiva ni reescribe ningún documento
> existente.

## 1. Alcance y método

El inventario se obtuvo con `rg --files -g '*.md'`: había 31 documentos Markdown versionados antes
de crear esta evidencia. Este archivo es el documento 32 y no se usó como entrada de la auditoría.

Para cada documento se revisaron título, secciones, propósito, audiencia, solapamientos y destino
propuesto. El tamaño se expresa como líneas y KiB aproximados. La frecuencia es cambio histórico
observable, no una predicción: **alta** = 10 o más commits que tocaron el archivo; **media** = 4 a 9;
**baja** = 1 a 3. Los conteos proceden de `git log --follow` hasta el corte.

Las referencias se localizaron con búsqueda literal global por cada nombre candidato, incluyendo
Markdown, texto, comentarios y contratos. Además se analizaron los enlaces Markdown relativos: en el
corte no había enlaces locales rotos.

## 2. Inventario completo

| Documento | Propósito actual | Tamaño aprox. | Cambio observado | Audiencia | Tratamiento propuesto |
|---|---|---:|---:|---|---|
| `AGENTS.md` | Convenciones estables y obligatorias para agentes | 129 líneas / 8.5 KiB | media (5) | agentes y desarrolladores | Conservar en raíz; enlazar, no copiar |
| `CLAUDE.md` | Índice de arranque específico para Claude | 33 / 1.7 KiB | baja (2) | agentes Claude | Conservar como índice mínimo y corregir rutas |
| `docs/00-indice-contexto.md` | Índice narrativo del corpus anterior | 121 / 12.6 KiB | alta (22) | agentes y desarrolladores | Retirar a archivo; su navegación la sustituyen README, CLAUDE y packs |
| `docs/01-estado-actual-mvp.md` | Descripción extensa del estado funcional | 263 / 21.1 KiB | alta (23) | dueño, agentes y desarrolladores | Absorber restricciones vigentes y retirar a archivo |
| `docs/02-modelo-tecnico-vigente.md` | Arquitectura técnica vigente por sistema y flujo | 464 / 27.2 KiB | alta (13) | arquitectos y desarrolladores | Fuente principal de `ARCHITECTURE.md`; archivar tras absorción verificada |
| `docs/03-guia-desarrollo-y-validacion.md` | Entorno, build, comandos y validación manual | 235 / 10.9 KiB | alta (15) | desarrolladores y validadores | Extraer contenido único y retirar a archivo |
| `docs/04-roadmap-operativo.md` | Estado y refinamientos del roadmap anterior | 241 / 17.3 KiB | alta (14) | dueño y agentes | Preservar patrón único; retirar a archivo |
| `docs/adr/0001-ramas-por-iniciativa.md` | Decisión aceptada sobre ramas por iniciativa | 45 / 2.5 KiB | baja (1) | dueño, arquitectos y agentes | Conservar como ADR inmutable |
| `docs/adr/0002-paso0-evidencia.md` | Evidencia que sustentó ADR-0002 | 202 / 16.5 KiB | baja (3) | dueño y revisores | Conservar junto al ADR |
| `docs/adr/0002-secuencia-dinamico-modular.md` | Decisión aceptada sobre la secuencia del dinámico | 97 / 5.9 KiB | baja (3) | dueño, arquitectos y agentes | Conservar como ADR inmutable |
| `docs/adr/plantilla.md` | Plantilla normativa para ADRs | 28 / 0.9 KiB | baja (1) | autores de ADR | Conservar |
| `docs/adr/README.md` | Contrato, ciclo e índice de ADRs | 48 / 2.6 KiB | baja (2) | dueño, arquitectos y agentes | Conservar |
| `docs/analisis-macro-vba-cabeceras.md` | Análisis histórico de la macro VBA y sus reglas | 576 / 18.8 KiB | baja (2) | historiadores técnicos y desarrolladores | Mover íntegro a archivo histórico |
| `docs/arquitectura-autocad-racks.md` | Propuesta histórica de plataforma y MVP | 1021 / 44.3 KiB | baja (2) | arquitectos e historiadores técnicos | Mover íntegro a archivo histórico |
| `docs/auditoria-arquitectura-2026-07.md` | Hallazgos y arquitectura objetivo que originan el ROADMAP | 345 / 26.6 KiB | baja (2) | dueño y arquitectos | Mantener vigente; archivar solo al integrarse sus iniciativas |
| `docs/AUTOMATION_PLAN.md` | Contrato global del ejecutor nocturno | 334 / 18.9 KiB | baja (3) | dueño y automatización | Conservar como fuente global |
| `docs/catalogos-y-plantillas.md` | Edición y semántica de catálogos/plantillas | 565 / 42.2 KiB | alta (33) | usuarios técnicos y desarrolladores | Mover a `docs/guias/` sin reescribir su contenido |
| `docs/despliegue.md` | Instalación, Autoloader, actualización y diagnóstico | 269 / 14.6 KiB | alta (17) | usuario final, soporte y release | Mover a `docs/guias/` |
| `docs/generacion-cabecera-lateral.md` | Algoritmo block-based de la cabecera lateral | 235 / 17.6 KiB | media (8) | desarrolladores de Application/Plugin | Mover a `docs/guias/` |
| `docs/HANDOFF.md` | Estado vivo mezclado con arquitectura e historia | 493 / 43.3 KiB | alta (15) | dueño y agentes que reanudan | Reducir a estado verificable; extraer historia y duplicados |
| `docs/ideas-futuras.md` | Backlog largo y deuda diferida | 315 / 28.9 KiB | alta (36) | dueño, producto y arquitectura | Conservar como única fuente de backlog largo |
| `docs/initiatives/I-06-reestructura-context-packs.md` | Contrato detallado de I-06 | 193 / 8.4 KiB | baja (2) | ejecutor, dueño y revisor | Conservar; enlazar esta evidencia |
| `docs/initiatives/README.md` | Contrato de la carpeta de iniciativas | 37 / 2.0 KiB | baja (2) | ejecutor y autores de iniciativas | Conservar |
| `docs/initiatives/TEMPLATE.md` | Plantilla estructurada de iniciativas | 110 / 2.9 KiB | baja (3) | ejecutor y autores de iniciativas | Conservar |
| `docs/modelo-datos-cabecera-rack-selectivo.md` | Modelo conceptual histórico propuesto para cabeceras | 651 / 19.7 KiB | baja (3) | historiadores técnicos y Domain | Mover íntegro a archivo histórico |
| `docs/modelo-de-datos.md` | Relaciones, FKs y carga de catálogos actuales | 226 / 14.7 KiB | alta (11) | editores de catálogo y desarrolladores | Mover a `docs/guias/` |
| `docs/mvp-configurador-cabeceras.md` | Especificación histórica detallada del configurador | 1677 / 69.8 KiB | baja (3) | historiadores técnicos y producto | Mover íntegro a archivo histórico |
| `docs/plan-implementacion-mvp-csharp-autocad.md` | Plan histórico de construcción del primer MVP | 716 / 20.4 KiB | baja (2) | historiadores técnicos | Mover íntegro a archivo histórico |
| `docs/ROADMAP.md` | Plan vigente por fases, iniciativas y dependencias | 263 / 24.2 KiB | media (6) | dueño, agentes y arquitectura | Conservar como única fuente del plan |
| `docs/WORKFLOW.md` | Proceso Git, worktrees, integración y documentación | 203 / 15.2 KiB | media (5) | agentes, revisores e integrador | Conservar como única fuente del proceso |
| `README.md` | Presentación, uso, comandos y quick start | 146 / 14.9 KiB | alta (21) | usuario final y desarrolladores nuevos | Reducir a una pantalla y enlazar fuentes temáticas |

## 3. Duplicaciones y deriva

### 3.1 Navegación y arranque

`README.md`, `AGENTS.md`, `CLAUDE.md`, `docs/00-indice-contexto.md` y `docs/HANDOFF.md` ofrecen órdenes
de lectura parcialmente distintos. El índice 00 repite descripciones que también viven en README,
HANDOFF y las guías. El destino propuesto es:

- README: entrada humana y quick start;
- AGENTS: convenciones estables;
- CLAUDE: índice corto para ese agente;
- HANDOFF: estado vivo;
- Context Packs: selección de contexto por tipo de iniciativa.

### 3.2 Estado del producto

`README.md`, `docs/01-estado-actual-mvp.md`, `docs/02-modelo-tecnico-vigente.md`,
`docs/04-roadmap-operativo.md` y las secciones 1 a 4 de HANDOFF describen los tipos de rack, comandos,
vistas, identidad y round-trip con diferente detalle. Esto explica la deriva ya observada en conteos,
alcance y “qué sigue”. La fuente vigente debe dividirse por propósito: resumen en README, arquitectura
en ARCHITECTURE, estado comprobado en HANDOFF y trabajo futuro en ROADMAP/ideas-futuras.

### 3.3 Arquitectura

`docs/02-modelo-tecnico-vigente.md`, la sección 4 de la auditoría, HANDOFF §4,
`docs/arquitectura-autocad-racks.md`, `docs/modelo-datos-cabecera-rack-selectivo.md` y
`docs/mvp-configurador-cabeceras.md` mezclan arquitectura actual, objetivo e historia. Solo 02 y la
auditoría describen la base del documento vigente nuevo; los otros tres son evidencia histórica y no
deben convertirse en reglas actuales por accidente.

### 3.4 Desarrollo y validación

Los comandos de build/test y las advertencias de AutoCAD aparecen en README, AGENTS,
`docs/03-guia-desarrollo-y-validacion.md`, WORKFLOW y `docs/despliegue.md`. AGENTS debe conservar los
comandos canónicos y la definición de terminado; WORKFLOW, los gates; una guía de validación, el
checklist manual; despliegue, la instalación y el diagnóstico del bundle.

### 3.5 Catálogos y datos

La lista de CSV, su encoding, cache, relaciones y reglas se repite en README, 00, 01, 02, 03,
HANDOFF, `catalogos-y-plantillas.md` y `modelo-de-datos.md`. Las dos últimas son las fuentes temáticas:
la primera explica edición y campos; la segunda, relaciones/FKs y flujo de carga. README, AGENTS,
ARCHITECTURE y HANDOFF solo deben enlazarlas.

### 3.6 Plan, historia y deuda

`docs/04-roadmap-operativo.md`, el historial extenso de HANDOFF, la auditoría, ROADMAP e
`ideas-futuras.md` compiten como cronología o próximos pasos. ROADMAP debe ser el plan ejecutable;
ideas-futuras, el backlog largo; HANDOFF, el estado inmediato; archivo, la historia cerrada.

## 4. Contenido obsoleto, transitorio y único

### 4.1 Obsoleto o transitorio como fuente vigente

- `00-indice-contexto.md`: su taxonomía plana ya no coincide con la arquitectura objetivo.
- `01-estado-actual-mvp.md`: congela estado de producto que cambia con alta frecuencia y compite con
  HANDOFF/ARCHITECTURE.
- `03-guia-desarrollo-y-validacion.md`: contiene una fotografía de entorno (`.NET SDK 10`) y listas
  de proyectos/comandos/tipos que ya se duplican; no debe mantenerse como manual monolítico.
- `04-roadmap-operativo.md`: es una fotografía al 2026-07-10; sus refinamientos cerrados son historia y
  el plan vigente está en ROADMAP.
- Los cinco documentos de MVP/macro ya se declaran históricos y enlazan como vigente a 00/01, rutas
  que I-06 retirará.
- `02-modelo-tecnico-vigente.md` no es desechable: está incompleto en seguridad, layout y cotas, pero
  contiene el detalle que debe absorber ARCHITECTURE antes de archivarlo.
- HANDOFF no es obsoleto completo. Son transitorias sus secciones de arquitectura, mapa, configuración,
  decisiones e historial cuando exista un destino vigente verificado.

### 4.2 Contenido único y destino recomendado

| Origen | Contenido que no debe perderse | Destino recomendado |
|---|---|---|
| `02-modelo-tecnico-vigente.md` | Flujos y restricciones vigentes por sistema; invariantes cabecera/paneles; diseño→resolver→plan→BOM; identidad y round-trip | `docs/ARCHITECTURE.md`, actualizado contra código y auditoría §4 |
| Auditoría §4 | Kernel + módulos, registros, Editor Shell, contratos de sistema, persistencia uniforme, solución objetivo y política de datos | Sección “Arquitectura objetivo” de `ARCHITECTURE.md`; conservar la auditoría hasta cerrar sus iniciativas |
| Guía 03, líneas 7-125 | Requisitos del entorno, override `AutoCADInstallDir`, bloqueo de DLL, carga NETLOAD y advertencias MSB3277 | AGENTS/README solo en forma canónica; detalles en guía de validación/desarrollo |
| Guía 03, líneas 196-224 | Checklist manual de cabecera, dibujo, round-trip y multivista | Nueva `docs/guias/validacion-manual-autocad.md` (decisión DOC-01) |
| Guía 03, líneas 225-235 | Salidas generadas que nunca se versionan | AGENTS o guía de desarrollo; no duplicar `.gitignore` |
| Guía 04, líneas 234-241 | Patrón actual de cuatro pasos para agregar un `Kind` | Apéndice temporal de ARCHITECTURE hasta que I-18 cree `guias/agregar-un-sistema.md` (DOC-02) |
| HANDOFF §§1-3, 9-13 | Estado comprobado, validación más reciente, problemas, próximo paso, hashes/conteos y preguntas abiertas | HANDOFF reducido |
| HANDOFF §§4-6, 14 | Arquitectura, mapa, entorno y reanudación | ARCHITECTURE, README/guías y WORKFLOW/CLAUDE |
| HANDOFF §7 | Decisiones vigentes aún no convertidas a ADR | Mantener temporalmente accesible hasta I-07; requiere DOC-03 |
| HANDOFF §8 | Historia reciente con evidencia de integración | Archivo histórico fechado; HANDOFF conserva solo el último resultado verificable |
| `analisis-macro-vba-cabeceras.md` | Reglas extraídas del DVB, constantes, decisiones de migración y preguntas de ingeniería | Archivo histórico íntegro |
| `arquitectura-autocad-racks.md` | Razonamiento original de plataforma, capas, datos, bloques y roadmap inicial | Archivo histórico íntegro |
| `modelo-datos-cabecera-rack-selectivo.md` | Crítica del modelo anterior, entidades propuestas y migración gradual | Archivo histórico íntegro |
| `mvp-configurador-cabeceras.md` | Especificación completa del primer configurador, reglas, UI, dibujo y casos de prueba | Archivo histórico íntegro |
| `plan-implementacion-mvp-csharp-autocad.md` | Secuencia y entregables de implementación del MVP inicial | Archivo histórico íntegro |
| ADRs y su evidencia | Contexto y decisión aceptada; valor histórico que sigue gobernando | Permanecer en `docs/adr/`; nunca resumir como sustituto |
| Guías temáticas actuales | Semántica de catálogos, FKs, despliegue y generación block-based | `docs/guias/`, conservando contenido y git history |

No se detectó contenido únicamente valioso en 00: su valor es navegación y resumen. En 01 el detalle
funcional debe usarse como checklist de cobertura al redactar ARCHITECTURE, pero no conservarse como
segunda fuente vigente.

## 5. Referencias a rutas candidatas

La búsqueda literal encontró **71 menciones** sobre 15 nombres candidatos. La tabla es el barrido que
deberá repetirse en la fase 5. Incluye referencias normativas, enlaces, texto histórico, el contrato de
I-06 y una referencia desde código.

| Ruta candidata | Menciones | Referentes exactos en el corte |
|---|---:|---|
| `00-indice-contexto.md` | 11 | `AGENTS.md:11`; `CLAUDE.md:13`; `README.md:120`; `docs/HANDOFF.md:114,475`; históricos `analisis...:3`, `arquitectura...:3`, `modelo-datos...:1`, `mvp...:3`, `plan...:3`; contrato I-06 `:60` |
| `01-estado-actual-mvp.md` | 8 | `README.md:121`; `docs/00...:35`; los cinco históricos en sus banners (`:1` o `:3`); contrato I-06 `:60` |
| `02-modelo-tecnico-vigente.md` | 5 | `README.md:122`; `docs/00...:38`; auditoría `:142`; contrato I-06 `:56,83` |
| `03-guia-desarrollo-y-validacion.md` | 4 | `README.md:123`; `docs/00...:41`; contrato I-06 `:61,84` |
| `04-roadmap-operativo.md` | 4 | `README.md:124`; `docs/00...:44`; contrato I-06 `:61,85` |
| `catalogos-y-plantillas.md` | 8 | `AGENTS.md:103,125`; `README.md:96`; `docs/00...:86`; `docs/HANDOFF.md:95`; `docs/WORKFLOW.md:173`; `docs/despliegue.md:212`; evidencia ADR-0002 `:139` |
| `modelo-de-datos.md` | 5 | `AGENTS.md:127`; `README.md:96`; `docs/00...:87`; `docs/HANDOFF.md:95`; `docs/generacion-cabecera-lateral.md:78` |
| `despliegue.md` | 8 | `AGENTS.md:128`; `README.md:112`; `docs/00...:47`; `docs/01...:13`; `docs/HANDOFF.md:167`; auditoría `:132,328`; `src/RackCad.UI/RackCommandReference.cs:24` |
| `generacion-cabecera-lateral.md` | 2 | `README.md:110`; `docs/00...:88` |
| `analisis-macro-vba-cabeceras.md` | 2 | `README.md:132`; `docs/00...:102` |
| `arquitectura-autocad-racks.md` | 3 | `AGENTS.md:129`; `README.md:128`; `docs/00...:98` |
| `modelo-datos-cabecera-rack-selectivo.md` | 2 | `README.md:130`; `docs/00...:100` |
| `mvp-configurador-cabeceras.md` | 3 | `README.md:129`; `docs/00...:99`; `docs/arquitectura-autocad-racks.md:653` |
| `plan-implementacion-mvp-csharp-autocad.md` | 2 | `README.md:131`; `docs/00...:101` |
| `auditoria-arquitectura-2026-07.md` | 4 | `docs/00...:54`; `docs/ROADMAP.md:6`; contrato I-06 `:86`; autorreferencia de la auditoría `:322` |

Observaciones para la migración:

- Las menciones en banners históricos deben apuntar a la nueva entrada vigente, sin convertir el
  archivo histórico en fuente actual.
- La mención del ADR-0002 evidencia una ruta que existía en el momento del rebase; puede conservarse
  como hecho histórico aunque no sea un enlace navegable.
- La auditoría arquitectónica no debe moverse todavía: ROADMAP ordena archivarla cuando sus
  iniciativas estén integradas.
- `src/RackCad.UI/RackCommandReference.cs:24` obliga a elegir entre permitir un cambio de comentario
  fuera de `docs/` o mantener compatibilidad en la ruta antigua. El contrato actual exige a la vez
  corregir todos los referentes y no cambiar fuera de documentación; ver DOC-04.
- El parser de enlaces relativos encontró **0 destinos Markdown rotos** antes de cualquier movimiento.

## 6. Mapa documental final propuesto

La siguiente estructura concreta la arquitectura de ROADMAP. Los subdirectorios de `archivo/` son una
propuesta para evitar una segunda carpeta plana.

```text
README.md
AGENTS.md
CLAUDE.md
docs/
  HANDOFF.md
  WORKFLOW.md
  ROADMAP.md
  ARCHITECTURE.md
  AUTOMATION_PLAN.md
  ideas-futuras.md
  adr/
    README.md
    plantilla.md
    0001-*.md
    0002-*.md
  initiatives/
    README.md
    TEMPLATE.md
    I-*.md
  context-packs/
    README.md
    architecture-kernel.md
    catalogs-data.md
    persistence.md
    ui-editors.md
    autocad-plugin.md
    system-selective.md
    system-dynamic-flowbed.md
    delivery-validation.md
    documentation-governance.md
  guias/
    catalogos-y-plantillas.md
    modelo-de-datos.md
    despliegue.md
    generacion-cabecera-lateral.md
    validacion-manual-autocad.md
    glosario.md
  archivo/
    transicion-2026-07/
      00-indice-contexto.md
      01-estado-actual-mvp.md
      02-modelo-tecnico-vigente.md
      03-guia-desarrollo-y-validacion.md
      04-roadmap-operativo.md
      handoff-historial-2026-07.md
    mvp-inicial/
      analisis-macro-vba-cabeceras.md
      arquitectura-autocad-racks.md
      modelo-datos-cabecera-rack-selectivo.md
      mvp-configurador-cabeceras.md
      plan-implementacion-mvp-csharp-autocad.md
    auditorias/
      auditoria-arquitectura-2026-07.md  # solo cuando sus iniciativas estén integradas
```

Reglas del mapa:

1. Un documento vigente tiene un propósito y un dueño temático; otros documentos enlazan, no copian.
2. `ARCHITECTURE.md` separa explícitamente “vigente” de “objetivo”. Ninguna propuesta histórica se
   presenta como comportamiento actual.
3. Los movimientos usan `git mv` y corrigen todos los referentes en el mismo commit coherente.
4. `archivo/` preserva contenido y trazabilidad; no es fuente de reglas vigentes.
5. README, AGENTS y CLAUDE son entradas, no repositorios paralelos de arquitectura o estado.
6. HANDOFF conserva solo información cuya vigencia cambia entre sesiones y la última evidencia real.
7. La guía `agregar-un-sistema.md` no se crea en I-06; nace en I-18, como ordena el contrato.

## 7. Taxonomía inicial de Context Packs

Los documentos globales que el ejecutor debe leer por contrato —AGENTS, WORKFLOW, ROADMAP,
AUTOMATION_PLAN y el contrato de iniciativa— son la **base**, no un Context Pack. Cada iniciativa
agrega cero o más IDs en `context_packs`.

Un pack debe ser un manifiesto, no una copia narrativa. Su contrato mínimo propuesto contiene:
`id`, propósito, cuándo cargarlo, documentos requeridos/opcionales, globs de código relevantes,
exclusiones y gates manuales habituales. No redefine reglas globales ni fija hashes o conteos.

| ID propuesto | Cuándo se carga | Fuentes y áreas que referencia |
|---|---|---|
| `architecture-kernel` | Contratos compartidos, registros, capas, namespaces o nuevos sistemas | ARCHITECTURE, ADRs aplicables, `src/RackCad.Domain`, abstracciones compartidas de Application |
| `catalogs-data` | CSV/JSON, FKs, bloques, plantillas o validación de catálogo | guías de catálogos/modelo, `assets/catalogs/`, loaders y modelos de catálogo |
| `persistence` | DTO, schema, stores, identidad, round-trip o compatibilidad legacy | ARCHITECTURE, guía de modelo, `*Document`, `*Store`, `RackEmbedDocument`, tests de round-trip |
| `ui-editors` | WPF, ViewModels, controles, previews o Editor Shell | ARCHITECTURE UI, `src/RackCad.UI`, futuros tests UI |
| `autocad-plugin` | Comandos, transacciones, dibujo, jigs, bloques o Xrecords | ARCHITECTURE Plugin, guía block-based, `src/RackCad.Plugin` |
| `system-selective` | Geometría, seguridad, BOM o editor selectivo | Secciones selectivas de ARCHITECTURE, `Selective*`, filas de catálogo aplicables |
| `system-dynamic-flowbed` | Dinámico, camas, IN/OUT, seguridad o BOM de esas familias | Secciones correspondientes de ARCHITECTURE, `Dynamic*`, `FlowBed*`, catálogos aplicables |
| `delivery-validation` | Build, CI, versiones, bundle, NETLOAD o validación manual | AGENTS, guía de despliegue, guía de validación, `.github/workflows`, `deploy/`, proyectos |
| `documentation-governance` | Cambios de documentación, ADRs, iniciativas o enlaces | WORKFLOW, ROADMAP documental, ADR/initiative templates y mapa de fuentes |

Ejemplos de asignación para comprobar utilidad:

- I-06: `documentation-governance`.
- I-11: `architecture-kernel`, `persistence`.
- I-14: `architecture-kernel`, `ui-editors`, `delivery-validation`.
- I-19: `catalogs-data`, `delivery-validation`.
- I-22: `system-selective`, `catalogs-data`, `persistence`.
- I-18: todos los packs de implementación que realmente toque, sin crear un pack “push-back” antes
  de que exista conocimiento estable específico.

## 8. Decisiones exactas requeridas del dueño

Estas decisiones forman el gate `owner-decision` de la fase 2. La respuesta puede aprobar todas las
recomendaciones o indicar cambios por ID.

| ID | Decisión | Recomendación auditada |
|---|---|---|
| CP-01 | Aprobar los nueve IDs y permitir múltiples packs por iniciativa | Aprobar la tabla de §7; separar Selectivo de Dinámico/FlowBed y mantener `delivery-validation` unido inicialmente |
| CP-02 | Definir si los packs son manifiestos o resúmenes autocontenidos | Manifiestos de enlaces/globs, sin copiar reglas; evita recrear la duplicación que I-06 corrige |
| DOC-01 | Destino del checklist y detalles únicos de la guía 03 | Crear `docs/guias/validacion-manual-autocad.md`; AGENTS conserva solo comandos/gates canónicos |
| DOC-02 | Destino temporal del patrón “agregar un tipo” de la guía 04 | Apéndice marcado como temporal en ARCHITECTURE; I-18 lo reemplaza por `guias/agregar-un-sistema.md` |
| DOC-03 | Qué hacer con HANDOFF §7 antes de que I-07 redacte los ADRs | Mantener esa sección temporalmente en HANDOFF con aviso explícito; retirarla solo al integrar I-07 |
| DOC-04 | Cómo corregir la referencia de `src/RackCad.UI/RackCommandReference.cs:24` a despliegue | Autorizar en I-06 un cambio exclusivamente de comentario XML; es preferible a dejar un archivo puente duplicado |
| DOC-05 | Aprobar la jerarquía de archivo y el retiro de 02 después de absorberlo | Usar `transicion-2026-07/`, `mvp-inicial/` y `auditorias/`; archivar 02 solo tras checklist de cobertura |
| DOC-06 | Límite exacto de HANDOFF reducido | Conservar resumen/estado, última validación, problemas activos, siguiente acción, última verificación y preguntas; mover historia y enlazar arquitectura/proceso |

La auditoría arquitectónica no es una decisión abierta: permanece en su ruta hasta que las iniciativas
que deriva estén integradas, conforme a ROADMAP.

## 9. Riesgos y comprobaciones para fases posteriores

- Mover archivos antes de crear ARCHITECTURE o la guía de validación perdería fuentes únicas.
- I-06 e I-07 se estorban. La transición de HANDOFF §7 debe conservar las decisiones vigentes sin
  ejecutar los ADRs retroactivos fuera de alcance.
- Los ADRs aceptados son inmutables. Solo deben corregirse enlaces o notas permitidas; una ruta escrita
  como evidencia histórica puede permanecer literal.
- Toda ruta candidata debe buscarse otra vez después de los movimientos. Un resultado restante debe
  justificarse uno por uno.
- El chequeo de enlaces debe ejecutarse después de cada lote de movimientos y al final; el baseline es
  cero enlaces locales rotos.
- `docs/ARCHITECTURE.md` deberá comprobar cobertura explícita de seguridad, layout y cotas, ausentes en
  02, y distinguir arquitectura implementada de horizonte objetivo.
- La reducción de README/HANDOFF no debe borrar comandos, gates, fallos conocidos ni la evidencia de la
  última validación real.

## 10. Resultado de la fase 1

La fase 1 queda completa con:

- 31 documentos de entrada inventariados y clasificados;
- propósito, tamaño, cambio observable y audiencia registrados;
- duplicaciones, contenido transitorio y contenido único identificados;
- 71 menciones a 15 rutas candidatas localizadas;
- mapa final y taxonomía inicial de nueve Context Packs propuestos;
- ocho decisiones exactas preparadas para el dueño;
- cero movimientos, borrados, archivados o reescrituras de documentos existentes.

La siguiente fase es la 2 y debe detenerse en `owner-decision` hasta recibir
`approve-context-pack-taxonomy`.
