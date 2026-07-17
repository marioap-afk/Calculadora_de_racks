# Auditoría de arquitectura RackCad — 2026-07-16

> Auditoría completa del proyecto sobre el árbol `eaede44` (punta de `release/claude-review`, el
> estado integrado más avanzado). Método: 7 auditores de dimensión en paralelo (dominio/aplicación,
> WPF/MVVM, plugin AutoCAD, tests, documentación, Git/proceso, config/build) + verificación
> adversarial de cada hallazgo alta/media contra el código real (47 verificados, 47 confirmados,
> 0 refutados) + un crítico de completitud transversal. Total: 55 agentes, ~93 hallazgos.
>
> Este documento es el informe de referencia (registro de la auditoría; no se re-edita). El flujo
> Git/worktrees derivado vive en [WORKFLOW.md](WORKFLOW.md) y el plan ejecutable por fases e
> iniciativas en [ROADMAP.md](ROADMAP.md) (2026-07-16: el roadmap de la sección 6 quedó superado por
> ese plan, que re-validó cada recomendación). Los hallazgos ya conocidos y diferidos siguen en
> [ideas-futuras.md](ideas-futuras.md) (esta auditoría los confirma y NO los duplica).

## 1. Resumen ejecutivo

RackCad está en un estado **notablemente sano para su velocidad de crecimiento**: la dirección de
dependencias es impecable (Domain ← Application ← UI ← Plugin, cero AutoCAD fuera del Plugin, cero
WPF fuera de la UI), los builders son puros y testeables, hay 554 tests reales (no smoke) con
cultura de regresión genuina, y la documentación de continuidad (HANDOFF/AGENTS) está muy por encima
del estándar. Ese fundamento hace viable el plan a 5 años.

Los riesgos dominantes NO son de calidad de código sino de **escala y proceso**:

1. **"Sistema de rack" es una convención, no una abstracción.** Añadir un tipo nuevo (Push Back,
   Drive-In…) exige hoy shotgun surgery en 8-10 puntos (enum, stores con 3 switches, librería con
   enum paralelo, envelope, 5 despachos del Plugin, menú O(N), ventana clonada de 1,000-2,500
   líneas, DrawService copiado). Es también el punto exacto donde los agentes paralelos colisionan.
2. **La UI es el subsistema más grande (13,3k líneas) y el único sin ninguna red**: MVVM nominal
   (cero ICommand, una sola ventana con ViewModel), estado del documento soldado a controles WPF,
   cero tests por construcción (el csproj de tests no puede referenciar net8.0-windows).
3. **Proceso Git sin protocolo de paralelismo**: main muerta en el commit inicial, trunk en una rama
   llamada "release", 2 commits sin push, 6 CSVs sin commitear invisibles para los demás worktrees,
   ramas zombie con worktree montado, cero merges/PRs/tags/CI.
4. **La rama `codex/dinamico-modular` (+12,621 líneas, 85 archivos) es la decisión más urgente**:
   bifurcó antes de fixes de persistencia que afectan su área, toca archivos compartidos que el
   trunk evolucionó y multiplica los anti-patrones que este informe recomienda erradicar. Integrarla
   (o descartarla) va ANTES de cualquier refactor.
5. **Riesgos transversales latentes**: sistema mono-unidad en pulgadas implícito (la columna `units`
   es decorativa y el Plugin nunca consulta `INSUNITS` — escala silenciosamente errónea sobre planos
   en metros/mm), tres copias divergentes del catálogo (repo / worktree principal / bundle
   instalado), escrituras de stores no atómicas con catch silencioso, y `install-bundle.ps1` que
   destruye las ediciones locales de catálogo del usuario en cada actualización.

La estrategia recomendada es **incremental, sin big-bang**: primero orden de casa Git (días),
después la integración del dinámico (la decisión de secuencia), después los tres refactors
habilitadores (registro de sistemas, editor shell, división de comandos) usando **Push Back como
caso piloto** del patrón nuevo. Cada pieza está detallada abajo.

## 2. Fortalezas confirmadas (construir sobre esto, no reemplazarlo)

- **Estratificación por capas real y verificada** (grep de `Autodesk`/`System.Windows` en
  Domain/Application: 0). El patrón plan-de-datos-puros (`HeaderBlockInstance`/`DynamicSystemPlan`)
  mantiene AutoCAD confinado y hace que el BOM cuente re-ejecutando los mismos builders del dibujo:
  plano y BOM coinciden **por construcción**.
- **Pipeline del selectivo bien estratificado**: diseño → resolver (reglas documentadas) → sistema
  resuelto → builders por vista → plan → drawer. Es el patrón a replicar en los sistemas futuros.
- **Suite de 554 tests en <1 s, cross-platform**, con expectativas derivadas del catálogo, tests de
  equivalencia multiset (ARRAY == plano) y regresiones citando el bug que guardan. Montar CI es trivial.
- **Catálogos Excel-first genuinamente extensibles**: tablas normalizadas pieza/vista/bloque,
  mates paramétricos como datos, caché por firma, fallback de encoding.
- **Identidad de racks consistente** (GUID + JSON en la definición del bloque, envelope unificado,
  `RACKEDITAR` despacha por Kind) y patrones de rendimiento aprendidos y sistematizados (ARRAY,
  regen único, purga selectiva, caché del DWG de biblioteca).
- **Comentarios de intención excepcionales** en todo el código — reducen el costo de entrada de
  cada agente nuevo — y documentación de continuidad (HANDOFF con evidencia y fechas) fuera de serie.
- Cultura de verificación: test de regresión verificado FALLANDO antes del fix; validación manual en
  AutoCAD como criterio final de features de dibujo.

## 3. Hallazgos (consolidados, verificados contra el código)

Severidad ALTA = frena o encarece el plan a 5 años; MEDIA = deuda que crece con cada feature;
BAJA = higiene. Se listan los archivos ancla; el detalle línea-a-línea vive en el registro de la
auditoría.

### 3.1 Escalabilidad estructural (el tema central)

| # | Hallazgo | Severidad | Esfuerzo |
|---|---|---|---|
| E1 | Sin abstracción de "sistema de rack": alta de un tipo = tocar `RackSystemKind`, `RackProject/Document`, 3 switches de `RackProjectStore`, `RackDesignLibrary` (enum paralelo), envelope y validación | ALTA | grande |
| E2 | Plugin: el Kind se despacha en 5+ switches dispersos (`RackFrameCommands.cs`, `.BomTotal`, `.Layout`); en `BuildRackBom` un tipo nuevo queda FUERA del BOM total en silencio (default null) | ALTA | grande |
| E3 | Cada editor clona la tubería completa (catálogo + Recompute + preview + Insert/Update + status): escalar así = 6 ventanas más de 1,000-2,500 líneas | ALTA | grande |
| E4 | 5 DrawServices estructuralmente idénticos (~90 líneas c/u) que solo varían builder y nombre de bloque; ya divergieron (flag `regen` solo en 2) | ALTA | medio |
| E5 | `RackMainMenuWindow` crece O(N) por tipo (13 propiedades de payload, 6 handlers idénticos) | MEDIA | medio |
| E6 | Triplicación por vista dentro del selectivo: cada familia de seguridad se implementa 3-4 veces (frontal/lateral/planta+BOM); paso de troquel `2.0` hardcodeado en ≥5 sitios | MEDIA | medio |
| E7 | `SelectiveSafetySelection` God-data-class: secciones TOPE/DESVIADOR/PARRILLA-only + DeepCopy manual campo a campo que crece con cada familia | MEDIA | medio |
| E8 | Namespace `Systems` plano multi-sistema con nombres engañosos (`DynamicSystemPlan` lo usan todos; el selectivo consume constantes `DynamicRackDefaults.Separator*`) | MEDIA | medio |

### 3.2 MVVM / UI

| # | Hallazgo | Severidad | Esfuerzo |
|---|---|---|---|
| U1 | MVVM nominal: 0 `ICommand`, ~118 event handlers, una sola ventana con ViewModel; el documento del editor vive en campos privados de la Window | ALTA | grande |
| U2 | `RackFrameConfiguratorViewModel` (2,547 líneas, ~8 responsabilidades: navegación, bulk, plantillas con I/O, persistencia, BOM, validación de dominio, clonación manual de ~270 líneas) | ALTA | grande |
| U3 | UI completa (13,3k líneas) con cero tests POR CONSTRUCCIÓN (tests = net8.0 puro, no puede referenciar net8.0-windows) | ALTA | medio |
| U4 | TRES implementaciones de deep-clone de `RackFrameConfiguration` en la UI (manual + 2 por serialización); la manual se desincroniza con cada campo nuevo | ALTA | pequeño |
| U5 | Dos mundos de construcción (10 ventanas XAML vs 9 code-built) con ~40 líneas de bootstrap copiadas en cada diálogo code-built | MEDIA | medio |
| U6 | 3 ventanas de rejilla de seguridad duplican la matriz frente×nivel (~300 líneas por copia; la 4ª familia pagará otra) | MEDIA | medio |
| U7 | Sin controles reutilizables (campo numérico, combo de catálogo, matriz); extracción del preview canvas a medias; estilos/colores hardcodeados fuera del theme | MEDIA | medio |

### 3.3 Plugin AutoCAD

| # | Hallazgo | Severidad | Esfuerzo |
|---|---|---|---|
| P1 | 14 catch silenciosos y cero logging: un catálogo roto degrada a "faltan bloques" sin pista; `Report()` tira el stack trace | MEDIA | pequeño |
| P2 | Clase única de 13 comandos en 12 partials con helpers estáticos entrelazados entre archivos | MEDIA | medio |
| P3 | `LateralHeaderDrawService` mezcla servicio de cabecera + infraestructura compartida (LoadCatalog global, jig, PlaceAndReport) que 10 sitios consumen | MEDIA | medio |
| P4 | Lógica pura atrapada en comandos (normalización de contornos de RACKRELLENAR, política de edición multi-vista) — testeable si se mueve a Application | MEDIA | medio |
| P5 | Escaneo de tabla de bloques triplicado; `EnsureLayer`/`UniqueBlockName` duplicados con convenciones divergentes; `PluginInitializer` vacío | BAJA | pequeño |

### 3.4 Persistencia y datos

| # | Hallazgo | Severidad | Esfuerzo |
|---|---|---|---|
| D1 | Versionado desigual: FlowBed y Larguero se serializan como POCOs de dominio SIN versión ni SchemaGuard (renombrar una propiedad rompe archivos y embeds en silencio) | MEDIA | medio |
| D2 | Escrituras no atómicas (WriteAllText sin temp+rename) en 4 stores; cargas que resetean a defaults en silencio (settings pierde la ruta de biblioteca sin aviso) | MEDIA | pequeño |
| D3 | Pérdida entre versiones sobre DWGs compartidos: SchemaGuard solo frena MAJOR; un build viejo que re-guarda descarta los campos nuevos del JSON sin aviso | MEDIA | medio |
| D4 | **Mono-unidad implícita en pulgadas**: columna `units` decorativa, cero conversiones, el Plugin jamás lee `INSUNITS` → escala errónea silenciosa sobre planos métricos | ALTA | medio |
| D5 | Catálogo con TRES copias divergentes (repo / 6 CSVs sin commitear en el worktree principal / bundle instalado) + encodings mezclados versionados y sin `.gitattributes` | ALTA | pequeño |
| D6 | Esquema de costos (`unitCost`/`currency`) ya existe en seguridad.csv pero vacío y sin gobernanza; decidir dónde viven los precios ANTES de poblarlos | BAJA | pequeño |

### 3.5 Git, proceso y build

| # | Hallazgo | Severidad | Esfuerzo |
|---|---|---|---|
| G1 | `main`/`origin/main` muertas en el commit inicial; `release/claude-review` es el trunk de facto y sigue siendo NO-default en GitHub; el nombre "release" miente | ALTA | pequeño |
| G2 | Sin protocolo multi-IA: cero merges/PRs, integración por re-aplicación manual, ramas zombie con worktree montado, rama huérfana a 236 commits | ALTA | pequeño |
| G3 | Trunk local 2 commits adelante del remoto (los 6 bugfixes verificados NO están respaldados) + 6 CSVs sin commitear | ALTA | pequeño |
| G4 | **`codex/dinamico-modular` = +12,621 líneas / 85 archivos** bifurcada antes de fixes que tocan su área; infla `RackDynamicSystemWindow` a 3,318 líneas y multiplica los anti-patrones diagnosticados. Decidir su secuencia es prerequisito de todo refactor | ALTA | medio |
| G5 | Cero tags/releases/CI; AppVersion 1.0.0 hardcodeada ×2; imposible mapear bundle instalado → commit | ALTA | pequeño-medio |
| G6 | El Plugin no compila sin AutoCAD instalado (HintPath): no hay CI posible de la solución completa ni artefacto reproducible | ALTA | medio |
| G7 | `install-bundle.ps1` hace `Remove-Item -Recurse` del bundle: destruye las ediciones de catálogo y la blocks-library.dwg que despliegue.md manda guardar AHÍ | ALTA | pequeño |
| G8 | Bundle solo empaqueta `RackCad.*.dll`: la primera dependencia NuGet futura fallará en runtime dentro de AutoCAD (fallo diferido) | MEDIA | pequeño |
| G9 | Sin global.json (tests pueden correr en .NET 9/10 vs .NET 8 embebido en AutoCAD), Directory.Build.props subutilizado, sin .editorconfig/.gitattributes, sin SeriesMax ni estrategia de versión anual de AutoCAD | MEDIA | pequeño |

### 3.6 Documentación

| # | Hallazgo | Severidad | Esfuerzo |
|---|---|---|---|
| C1 | Conteos de tests divergentes (489/503/546/554 conviviendo) — corregido en esta auditoría; regla nueva: el número vive SOLO en HANDOFF §12 | ALTA | hecho |
| C2 | Secciones "Identidad y round-trip" / "Los cuatro tipos" / "Catálogos" copiadas casi verbatim en 6 documentos → causa raíz del drift | ALTA | medio |
| C3 | `02-modelo-tecnico-vigente.md` no cubre seguridad, layout de almacén ni cotas (≈30% del sistema); `01-estado-actual-mvp` y `04-roadmap` congelados compitiendo con HANDOFF §11 e ideas-futuras como TRES fuentes de "qué sigue" | ALTA | medio |
| C4 | Faltan: ADRs, guía "cómo agregar un tipo de rack" completa, proceso de release, protocolo multi-IA (creado en esta auditoría), glosario del dominio | ALTA | medio |
| C5 | seguridad.csv invisible en modelo-de-datos/despliegue/catalogos-y-plantillas — corregido en esta auditoría | MEDIA | hecho |
| C6 | Históricos sin separación física (carpeta plana de 16 archivos); assets/blocks/ y assets/templates/ son directorios muertos del plan original | BAJA | pequeño |

## 4. Arquitectura objetivo (horizonte 5 años)

**Principio rector: kernel + módulos de sistema.** El costo marginal de añadir el sistema N+1 debe
tender a "escribir SU módulo", no a "editar N archivos compartidos". No se reescribe nada: se
formaliza el patrón que el selectivo ya insinúa.

### 4.1 El contrato de un "sistema de rack"

Cada sistema (Selectivo, Dinámico, Cama, Push Back, Drive-In, Cantilever, Mezzanine, Carton/Pallet
Flow…) se convierte en un módulo que aporta:

```
Descriptor        Kind estable (string), etiqueta UI, vistas soportadas
Documento         DTO versionado propio (SchemaVersion + fallbacks legacy + round-trip test)
Diseño → Sistema  resolver puro (reglas de derivación)
Builders          por vista → SystemPlan (la IR actual, renombrada desde DynamicSystemPlan)
BOM builder       cuenta re-ejecutando los builders (patrón actual)
Editor            estado del editor SIN WPF (en Application, testeable) + vista WPF delgada
Draw adapter      instancia del DrawService genérico (builder + formato de nombre)
```

Tres registros consultan esos descriptores y eliminan los switches dispersos:

- **SystemRegistry** (Application): persistencia (`RackProjectStore`), validación y biblioteca de
  diseños dejan de hacer switch por tipo.
- **KindHandlerRegistry** (Plugin): `RACKEDITAR`/`RACKBOMTOTAL`/`RACKLAYOUT`/restamp despachan por
  registro; un Kind no registrado = error visible, no omisión silenciosa.
- **EditorModuleRegistry** (UI): el menú principal y la biblioteca iteran módulos (`IRackEditorModule`)
  en lugar de 13 propiedades + 6 handlers clonados.

**Prueba de fuego**: Push Back se implementa COMO el primer módulo del patrón nuevo. Si su alta
exige editar un store o un switch, el registro está incompleto.

### 4.2 UI: patrón "Editor Shell"

- `RackEditorSession<TDesign,TSystem>` compartida: catálogo, identidad GUID+nombre, Recompute
  coalescido, contrato `InsertRequested`/payload, status.
- Estado del editor extraído a clases puras en Application (`SelectiveEditorState`, …): testeable
  sin AutoCAD ni WPF, reutilizable si algún día hay otra superficie (web/API).
- Controles reutilizables: `NumericField` (vacío=auto, commit al salir/Enter, error inline),
  `CatalogCombo`, `SelectionMatrix` (la rejilla frente×nivel que hoy está triplicada),
  `PreviewCanvas` (proyección fit-to-canvas + paleta compartida congelada).
- Regla de construcción única: diálogos de captura → XAML; solo grids dinámicos → código, sobre una
  clase base `RackDialogWindow` (bootstrap único).
- Un solo deep-clone de `RackFrameConfiguration` (vía store de serialización) y borrar los otros dos.

### 4.3 Application/Domain

- Colocación por familia de accesorio (TopePlacement, ParrillaPlacement, TarimaPlacement…)
  parametrizada por vista/eje: mata la triplicación E6 y deja los builders como orquestadores.
- Familias de seguridad como subtipos (`TopeConfig`/`DesviadorConfig`/…) con clonación y DTO por
  subtipo: mata el DeepCopy campo-a-campo E7.
- Persistencia uniforme: TODO payload persistible = DTO Document + SchemaVersion + SchemaGuard +
  round-trip test (FlowBed y Larguero migran a este patrón). Helper compartido de escritura atómica
  (temp + File.Replace) y carga que distingue "no existe" de "ilegible" (.bad + aviso).
- Namespaces por sistema: `Systems.Selective` / `Systems.Dynamic` / `Systems.FlowBed` /
  `Systems.Shared` (y renombres fósiles: `Headers`→`Drawing`, `DynamicSystemPlan`→`SystemPlan`) —
  refactor mecánico en sesión dedicada.
- **Unidades**: ADR explícito. Corto plazo: validación dura (leer `INSUNITS`, avisar/abortar si el
  DWG no está en pulgadas). Largo plazo: decidir si la columna `units` se honra con conversión real.
- Regla para el roadmap SQL/API/reportes: todo store/provider NUEVO nace instanciable detrás de una
  interfaz (como `IRackCatalogProvider`); lo estático existente se migra solo cuando llegue el
  hosting fuera de AutoCAD (ADR: "estático hasta entonces").

### 4.4 Solución y proyectos futuros

```
RackCad.Domain          (como hoy, + sub-namespaces por sistema)
RackCad.Application     (kernel: catálogos, persistencia, geometría, BOM, layout + Systems.<X>)
RackCad.UI              (shell + controles + módulos de editor)
RackCad.Plugin          (comandos genéricos + registro de handlers + adapters)
RackCad.Tests           (como hoy)  |  RackCad.UI.Tests (net8.0-windows, corre en CI windows)
--- futuros, cuando lleguen ---
RackCad.Data            (SQL: detrás de las interfaces de stores)
RackCad.Api             (REST/gRPC sobre Application; posible gracias a la pureza actual)
RackCad.Reports         (reportes/cotización sobre el BOM)
```

`Directory.Build.props` centraliza TargetFramework/LangVersion/Nullable/Version; Central Package
Management cuando aparezca el segundo PackageReference; `Nullable=enable` obligatorio para proyectos
nuevos; el bundle se genera por `dotnet publish` (cierre transitivo de dependencias, G8).

### 4.5 Datos

- Catálogos CSV Excel-first se mantienen (funcionan y el usuario los domina), con: validador de
  catálogos con severidades (ideas-futuras #14), log de filas descartadas por rol desconocido,
  `.gitattributes` congelando encoding/eol, convención append-only para merges.
- blocks-library.dwg: manifest de compatibilidad (ideas-futuras #15) y distribución como asset de
  release.
- Costos/precios: ADR antes de poblar (¿catálogo versionado o fuente aparte/SQL? vigencia,
  moneda, dueño) — el optimizador beneficio/costo del roadmap depende de esto.

## 5. La decisión de secuencia: `codex/dinamico-modular` primero

Cualquier refactor estructural que arranque antes de resolver esta rama nace muerto: son +12,621
líneas sobre 85 archivos que tocan `SystemBomBuilder`, stores, catálogos compartidos y triplican el
tamaño de `RackDynamicSystemWindow`. Opciones:

- **(a) Integrarla primero (recomendada si su funcionalidad está validada):** push del trunk →
  rebase de la rama sobre `eaede44` → resolver conflictos (SystemBomBuilder, stores, 3 CSVs) con la
  suite como red → validación en AutoCAD → integrar → borrar rama/worktree. Los refactors parten del
  árbol resultante.
- **(b) Descartarla formalmente:** registrar ADR "el dinámico modular se re-implementará sobre el
  registro de sistemas" y borrar la rama (tag `archive/` si se quiere conservar).

Mientras no se decida: **congelar trabajo nuevo sobre el subsistema dinámico** (HANDOFF §11 lo
lista como siguiente prioridad — esa tarea DEBE empezar por esta decisión).

## 6. Roadmap priorizado

Formato: **[beneficio | costo | riesgo | impacto | momento]**.

### ALTA (ahora — próximas 2-4 semanas)

1. **Orden de casa Git** (G1-G3, G5): commitear los 6 CSVs, push del trunk, fast-forward de `main`,
   default branch, borrar zombies, tags a partir de ahora. [Respaldo + base común para todos los
   agentes | horas | nulo | alto | YA — checklist en WORKFLOW.md §9, requiere al dueño]
2. **Decidir e integrar/descartar `codex/dinamico-modular`** (G4, §5). [Desbloquea todos los
   refactors | 1-2 sesiones | medio (conflictos semánticos; mitigado por suite + validación AutoCAD)
   | crítico | antes de cualquier refactor]
3. **CI mínimo** (ya incluido en esta rama: `.github/workflows/ci.yml`) + rama protegida +
   `global.json`. [Árbitro neutral entre agentes; regresiones detectadas en minutos | pequeño |
   nulo | alto | YA]
4. **Adoptar el flujo multi-agente** (doc creado; G2). [Elimina los 3 incidentes ya ocurridos
   (zombie, bifurcación vieja, huérfana) | pequeño | nulo | alto | YA]
5. **Guardrail de unidades** (D4): leer `INSUNITS` al insertar/layout y avisar si ≠ pulgadas + ADR.
   [Evita dibujos a escala 25.4x errónea | pequeño | bajo | alto | próxima sesión de Plugin]
6. **Logging mínimo + catch silenciosos + escrituras atómicas** (P1, D2): logger a
   `%AppData%\RackCad\logs`, `Report()` con stack, temp+rename en stores, aviso de catálogo vacío.
   [Los fallos best-effort se vuelven diagnosticables | pequeño | bajo | alto | próximas 2 semanas]
7. **Fix de `install-bundle.ps1`** (G7): preservar/respaldar `Contents\catalogs` y blocks-library
   antes del Remove-Item. [Evita pérdida de datos de negocio del usuario | pequeño | bajo | alto |
   antes del próximo despliegue a terceros]

### MEDIA (meses 1-3, en este orden)

8. **Registro de sistemas** (E1+E2+E5, §4.1) con **Push Back como piloto**. [Alta de un sistema pasa
   de 8-10 puntos a 1 módulo; menos colisiones entre agentes | grande | medio (mecánico + tests
   round-trip existentes) | crítico a 5 años | después del punto 2, antes de Push Back]
9. **Editor Shell + controles reutilizables + estado de editor en Application** (E3, U1-U3, U6-U7).
   [Ventanas nuevas de ~300 líneas en vez de 2,000; la lógica de captura se vuelve testeable |
   grande | medio | crítico | antes de la 5ª ventana de editor]
10. **Unificar DrawServices + BlockPlacementService + dividir `RackFrameCommands`** (E4, P2-P4).
    [Menos archivos-conflicto; lógica pura testeable | medio | bajo-medio | alto | tras el punto 8]
11. **Persistencia uniforme** (D1, D3): FlowBedDocument/LargueroDocument versionados; versión de app
    en el envelope; preservar campos desconocidos al re-guardar. [Round-trips a prueba de evolución
    del dominio y de bundles desactualizados | medio | bajo | alto | antes del primer release a terceros]
12. **Colocación por familia + subtipos de seguridad** (E6, E7) + único deep-clone (U4). [Nueva
    familia de seguridad = 1 servicio, no 5 ediciones | medio | medio | alto | antes de la próxima familia]
13. **Des-duplicación documental** (C2-C4): dueño único por tema, 02 al día, 04 → apuntador, ADRs
    retroactivos (~12 decisiones de HANDOFF §7), guía completa "agregar un tipo de rack", glosario.
    [Los docs dejan de divergir; una IA nueva arranca sin reconciliar | medio | nulo | alto | continuo]
14. **Referencias de AutoCAD para CI del Plugin** (G6): refs/ o paquete de build (toca política
    cero-NuGet → decisión del dueño) + validación automatizada del bundle. [Solución completa
    compilable en CI; artefacto reproducible | medio | bajo | medio | cuando el CI básico esté rodando]
15. **Versionado real** (G5, G8): `<Version>` en Directory.Build.props, SHA estampado,
    PackageContents generado, bundle por publish, tags semver. [Trazabilidad bundle→commit | pequeño
    | bajo | medio | junto con el punto 14]

### BAJA (meses 3-6+, oportunista)

16. Namespaces `Systems.*` + renombres fósiles (E8; mecánico, sesión dedicada, tras 8-10).
17. `Nullable=enable` en proyectos nuevos, `.editorconfig`, TreatWarningsAsErrors con NoWarn puntual.
18. TestCatalogIds centralizados + test guardián de IDs + coverlet (mapa de cobertura).
19. Validador de catálogos con severidades y manifest de blocks-library (ideas-futuras #14/#15).
20. ADR de costos/precios antes de poblarlos (D6); estrategia de versión anual de AutoCAD (SeriesMax).
21. Limpieza: assets/blocks|templates muertos, higiene .sln, mover históricos a docs/archivo/.
22. Proyecto RackCad.UI.Tests (net8.0-windows) para ViewModels cuando el Editor Shell exista.

**Qué NO hacer** (decisiones vigentes del dueño): validación de cargas (va con RAM Elements),
optimizador IA de layout (meta futura; `RACKLAYOUT` es solo el motor de colocación), integración del
cotizador Excel, SQLite antes de tiempo.

## 7. Cambios aplicados en esta auditoría (pequeños, seguros, reversibles)

- **Nuevo** `docs/auditoria-arquitectura-2026-07.md` (este documento).
- **Nuevo** `docs/flujo-multi-agente.md` (flujo Git/worktrees multi-IA + checklist de migración;
  renombrado después a `docs/WORKFLOW.md` en la fase de planificación).
- **Nuevo** `.github/workflows/ci.yml` (suite en ubuntu + build de UI en windows; inerte hasta el push).
- Consistencia de docs: conteos de tests unificados (el número vive solo en HANDOFF §12; encabezado
  y prompt de reanudación de HANDOFF corregidos a 554/`eaede44`), `seguridad.csv` añadido a
  modelo-de-datos/despliegue/catalogos-y-plantillas, índice 00 ahora lista despliegue.md,
  ideas-futuras.md y los 2 documentos nuevos, ideas-futuras ya no marca como pendientes los
  desviadores/poste tope terminados, AGENTS.md apunta al flujo multi-agente.

## 8. Cambios que requieren aprobación del dueño (NO ejecutados)

1. Migración Git (WORKFLOW.md §9): push del trunk, fast-forward de `main`, default branch,
   borrado de ramas/worktrees zombie, decisión sobre `codex/app-tooling-catalogs-logging`.
2. Integración o descarte de `codex/dinamico-modular` (§5) — la decisión de secuencia.
3. Registro de sistemas / handlers / módulos de editor (refactor grande, §4.1).
4. Editor Shell + extracción del estado de los editores (refactor grande, §4.2).
5. División de `RackFrameCommands` y unificación de DrawServices.
6. DTOs versionados para FlowBed/Larguero (cambia formato en disco; con lectura legacy).
7. Reorganización de namespaces y renombres fósiles.
8. `.gitattributes` + normalización de encodings de los CSV (afecta el diff de todos los worktrees).
9. Referencias de AutoCAD vía refs//NuGet de build (excepción a la política cero-NuGet).
10. Cambio de comportamiento de `install-bundle.ps1` (preservación de datos del usuario).
11. Mover históricos a `docs/archivo/` y des-duplicación documental completa (toca ~10 archivos).
