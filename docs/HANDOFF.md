# Project Handoff

> Estado vivo de RackCad para continuidad entre sesiones. Actualizado: **2026-07-22**.
> La arquitectura se consulta en [ARCHITECTURE.md](ARCHITECTURE.md), el proceso en
> [WORKFLOW.md](WORKFLOW.md), el plan en [ROADMAP.md](ROADMAP.md), los procedimientos en
> [guias/](guias/) y la historia anterior en
> [archivo/transicion-2026-07/handoff-historial-2026-07.md](archivo/transicion-2026-07/handoff-historial-2026-07.md).

## 1. Resumen y estado actual

RackCad es un plugin de AutoCAD 2025 (.NET 8, C#/WPF) para diseñar y dibujar racks industriales
con BOM. El trunk único es `main`; Domain y Application son puros, UI usa WPF sin AutoCAD y Plugin
es el único adaptador de la API de AutoCAD.

El producto mantiene cuatro familias operativas: cabecera, selectivo, dinámico modular y cama de
rodamiento. Comparten identidad por GUID embebida en DWG, edición round-trip y vistas ligadas. El
dinámico modular de I-02 y la instalación segura de I-04 están integrados.

I-06 (`docs/reestructura`) está cerrada e integrada desde el **2026-07-17**. Entregó
`ARCHITECTURE.md`, nueve Context Packs, guías vigentes, archivo histórico y este HANDOFF reducido.
La iniciativa reorganizó documentación y no cambió comportamiento de producto.

I-26 (`refactor/test-catalog-ids`) está integrada desde el **2026-07-19**. Centraliza las
expectativas canónicas de tests, añade un guardián de IDs y relaciones esenciales y publica
cobertura Cobertura como artifact; no cambia producto ni catálogos distribuidos.

I-13 quedó integrada el **2026-07-20** mediante `architecture/referencias-autocad-ci`. CI compila
ahora `RackCad.Plugin` sin AutoCAD instalado con
referencias condicionales compile-only, versiones y origen fijados y guardas que impiden copiar o
publicar material Autodesk. ADR-0003 registra la única excepción autorizada a la política cero
NuGet. I-29 concluyó con decisión B, aprobada con catorce restricciones para uso interno como
aceptación interna de riesgo; no constituye conclusión jurídica ni autorización expresa de Autodesk.

I-09 (`refactor/plugin-commands`) quedó integrada el **2026-07-20**. Partió la clase única
`RackFrameCommands` (12 archivos parciales) en clases de comando públicas por área (`RackMenuCommands`,
`RackCabeceraCommands`, `RackSelectivoCommands`, `RackDinamicoCommands`, `RackCamaCommands`,
`RackDuplicarCommands`, `RackInventarioCommands`, `RackLayoutCommands`, `RackAyudaCommands`) y promovió
los helpers cruzados a tipos `internal static` (`RackBlockFinder` con el escaneo de envelopes
unificado, `RackCloner`, `LayerHelper`, `InDocumentTransaction`, `RackCommandSupport`,
`RackEnvelopeRestamp`). Es un refactor mecánico **sin cambio de comportamiento**: los 26 comandos y
alias, prompts, mensajes, switches por `Kind`, DrawServices, GUID/restamp, layers y flujos se
conservan; solo cambia la clase contenedora. AutoCAD descubre las clases públicas sin
`[assembly: CommandClass]`.

I-08 (`architecture/system-registry`) quedó integrada el **2026-07-21**. Introdujo un `SystemDescriptor`
y un `SystemRegistry` puro en `RackCad.Application` (fuente única de los cinco `RackSystemKind`, sin
reflexión ni escaneo) y migró `RackProjectStore`, la validación genérica y `RackDesignLibrary` para
despachar por el registro: mueren el `if/else` de `Serialize` y los `switch` de
`BuildProject`/`ValidateProject`, y **el enum paralelo `RackDesignKind` y su `MapKind` quedaron
eliminados por completo** (`RackDesignLibraryEntry.Kind` pasa a `RackSystemKind`; la etiqueta visible
proviene del descriptor). Es un cambio **sin comportamiento observable nuevo**: formato JSON PascalCase,
schema `2.0`, nombres de enum, fallback legacy sin `kind`, `Kind: 999` a cabecera, reconstrucción física,
reglas laxas de cama/larguero y etiquetas de biblioteca se conservan idénticos. La adaptación de UI se
limitó a las comparaciones de `RackMainMenuWindow`. I-10 e I-16 quedaron fuera de alcance.

I-16 (`refactor/draw-services`) quedó integrada el **2026-07-21** (merge `--no-ff`; rebaseada sobre el `main`
con I-08 antes de integrar; validada en AutoCAD, ver §2 y §5). Colapsa la duplicación de los `*DrawService`
del Plugin **sin cambio de comportamiento**: extrae la infraestructura compartida (`RackCatalogLoader`,
`BlockPlacement`), uniforma la regeneración condicional (`SystemBlockWriter.ApplyRegen`) y colapsa la
orquestación de los siete servicios de vista en `ViewBlockDraw`, conservando las siete fachadas públicas,
`LateralHeaderDrawService` como servicio especializado, y los invariantes observables (nombres de bloque y
sufijos, mensajes, `postIndex`, `DynamicRackEnd`, caso all-loose, payload/GUID, BOM, geometría, persistencia
y el único `Regen` final multivista).

I-10 (`architecture/kind-handlers`) quedó integrada el **2026-07-21** (merge `--no-ff`; `origin/main` no
avanzó desde la base `c5a4082`, sin rebase final). Introduce `IRackKindHandler` y un registro explícito
`KindHandlerRegistry` en `RackCad.Plugin` (los cuatro Kinds embebidos —`selective`, `dynamic`, `cabecera`,
`cama`— en orden canónico, sin reflexión; `Larguero` no tiene sobre ni handler) y migra a él los tres
despachos por el string del sobre: RACKEDITAR, RACKBOMTOTAL (`BuildRackBom` + `KindLabel`) y el restamp de
copias independientes (consumido por RACKDUPLICAR y RACKLAYOUT). Es un refactor **sin cambio de
comportamiento observable** salvo una excepción autorizada: un `Kind` sin handler produce un error visible
(el mismo mensaje de "tipo de rack no reconocido", inalcanzable con los cuatro Kinds reales). El
`SystemRegistry` de I-08 permanece en Application (persistencia/validación/biblioteca, por `RackSystemKind`);
`KindHandlerRegistry` es el registro del Plugin para las operaciones AutoCAD y **no** se unifica con él ni
con `RackListBuilder` (Application, RACKLISTA), que queda intacto por la dirección de dependencias. Cierra la
pista B del Plugin (I-09→I-16→I-10).

Una **corrección posterior a I-10** (`fix/kind-handler-missing-errors`) quedó integrada el **2026-07-21**
(merge `--no-ff`; base `c9f2d61`, sin rebase). **No reimplementa I-10** —que permanece históricamente
integrada en `c9f2d61`— sino que completa el tratamiento de handlers ausentes: un `RackEmbedDocument.Kind`
sin handler registrado ahora produce **siempre** el error visible histórico y ninguna operación continúa en
silencio. Hallazgos corregidos: (1) **RACKBOMTOTAL** ya no muestra un BOM parcial — un preflight de todos los
racks colocados **aborta** el comando ante cualquier Kind sin handler (el skip best-effort queda solo para el
payload ilegible de un handler conocido); (2) **RACKLAYOUT** valida el handler **antes** de abrir la ventana,
para copias **enlazadas e independientes** (antes solo independientes); (3) el **restamp** lanza ante handler
ausente en vez de devolver el diseño intacto (evita identidades inconsistentes); (4) **inmutabilidad**
completa de `KindHandlerRegistry.Handlers` (extraída a la `KindDispatch<T>` pura de Application, expuesta como
`ReadOnlyCollection`); (5) **cobertura de rutas negativas** verificable sin AutoCAD (`KindDispatch.TryResolveAll`
+ tests puros y source-guards, ADR-0003). El dueño **aprobó la validación manual en AutoCAD** sobre el DLL
Debug de la punta técnica. Sin cambios de geometría, BOM funcional, GUID, persistencia, comandos ni aliases.

**I-11** (`architecture/persistencia-uniforme`) uniforma la **persistencia** de Application: versiona
`FlowBedDocument` y `LargueroDocument` (planos, `SchemaVersion`, `FromDomain`/`ToDomain`, fallback legacy) y
**preserva los campos JSON desconocidos y una versión de esquema no degradada** al cargar, editar, duplicar,
guardar y re-serializar, en los **cuatro límites**: `RackEmbedDocument` (el sobre), `RackProjectDocument` (el
wrapper —incluido el diseño **interior** de los embeds dinámico y de cabecera, y los wrappers de biblioteca—),
`FlowBedDocument` y `LargueroDocument`. Una `SchemaVersionPolicy` central decide legibilidad por MAJOR y una
versión de escritura que **nunca degrada** un minor superior del mismo major; `RackEmbedComposer` (fábrica pura)
hereda `ExtensionData` y versión del `source`; un preflight discriminado
(`ResolveInnerSource`/`PreflightInnerSources`) hace que un MAJOR interior incompatible o un `Kind` incorrecto
**aborten la edición completa** sin actualización parcial. La preservación cruza biblioteca↔DWG por sidecars de
salida que `RackMenuCommands` transporta (transporte mínimo, **sin** cambiar los handlers de I-10). **No** cambia
geometría, BOM, GUID ni el **formato físico del Xrecord** (clave/chunk/`DxfCode` intactos): el sobre se preserva
desde el tipo `RackEmbedDocument`, así I-11 **no** toca `RackEnvelopeRestamp` ni el despacho por `Kind` de I-10.
El **quinto** DTO potencial —`RackFrameProjectDocument` (biblioteca de cabecera desnuda por
`RackFrameProjectStore`)— queda **excluido por decisión aprobada del dueño** (deuda registrada, no cancelada). El
dueño **aprobó la matriz manual en AutoCAD 2025** (incluidos los escenarios B5/B6/S7) y la **owner-validation**;
la rama se integra por `git merge --no-ff` en esta sesión.

**I-14** (`architecture/ui-controls`) abre la **pista C de UI** con cinco controles WPF reutilizables en
`src/RackCad.UI/Controls/`, cada uno con su lógica pura separada de la vista: `SelectionMatrix`
(+`SelectionMatrixModel`, rejilla de casillas con celdas apagadas y actualización por celda **sin rebuild** por
clic), `NumericField` (+`NumericFieldValidation`, entrada localizada sobre `LocalizedNumberParser` con rango y
opcional→auto, que **preserva la procedencia del `BorderBrush` del consumidor** —valor local o binding— en la
transición válido→error→válido), `CatalogCombo` (+`CatalogComboSelection`, sobre `CatalogOption`/
`UiSupport.ToOptions`, con sentinela "(auto)"), `PreviewCanvas` (+`PreviewProjection` + `PreviewPalette`:
proyección mundo→lienzo y paleta congelada **compartidas**, cerrando el hueco que dejaba `PreviewCanvasPainter`) y
la base `RackDialogWindow` (chrome compartido, barra Aceptar/Cancelar, estado). Crea además `tests/RackCad.UI.Tests`
(`net8.0-windows`, runner STA propio **sin dependencias nuevas**) con **85 pruebas** y un **job de CI dedicado**
(`ui-tests`). Es un cambio **confinado a la capa UI**: **no** migra ninguna ventana existente (patrón strangler),
**no** referencia AutoCAD ni el Plugin, y **no** cambia geometría, BOM, persistencia ni el dibujo. Por eso **no**
requiere validación en AutoCAD (`requires_autocad: false`) ni owner-validation; la adopción de los controles la
harán I-15/I-20/I-21/I-22. La rama se integra por `git merge --no-ff` en esta sesión.

**I-12** (`refactor/versionado`) entrega **versionado real** y empaquetado reproducible, **sin cambio de
comportamiento de producto**. Centraliza en `Directory.Build.props` una **versión única** (`RackCadVersion`),
`LangVersion`, `Nullable` y determinismo, más las series de AutoCAD (`RackCadAutoCADSeriesMin`/`Max`); estampa el
**SHA de git** reproducible en `InformationalVersion` (con fallback definido cuando no hay git). El manifiesto del
Autoloader `PackageContents.xml` se **genera** desde una plantilla con la versión y las series (nada duplicado a
mano) y el bundle se arma por **`dotnet publish`** (target `AfterTargets="Publish"`), con `deploy/build-bundle.ps1`
(publish + verificación) y `deploy/verify-bundle.ps1` **fail-closed**: allowlist recursiva, comparación por SHA-256
de los cuatro DLL contra el publish y de los catálogos contra `assets/catalogs`, versión/series del manifiesto y
**cero DLL Autodesk** (ADR-0003 intacto), con su harness `deploy/test-verify-bundle.ps1`. `install-bundle.ps1 -Build`
usa el flujo canónico verificado y **rechaza** `-Build`+`-SourceBundlePath`; la guarda de CI publica y ejecuta
`verify-bundle.ps1` + el harness (**sin tocar** `ci.yml` ni `RackCad.sln`). Documenta **ADR-0004** (una sola serie de
AutoCAD a la vez, hoy `SeriesMin = SeriesMax = R25.0` —solo AutoCAD 2025—, recompilación anual), **aceptado por el
dueño**. Como I-14 ya estaba integrada, el rebase eliminó de `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` el
`LangVersion`/`Nullable` duplicados (ahora heredados). El dueño **aprobó la validación manual de autocarga en
AutoCAD 2025** (bundle autoloaded sin `NETLOAD`, `RACKCAD` PASS; ver §5 y `docs/initiatives/I-12-autocad-validation.md`).
La rama se integra por `git merge --no-ff` en esta sesión.

**I-19** (`feature/validador-catalogos`) entrega un **validador de catálogos puro** en
`RackCad.Application.Catalogs.Validation`, **sin cambio de comportamiento de producto**. Reúne en **un
diagnóstico único** con severidades cinco categorías —ids duplicados, referencias/relaciones inválidas,
bloques/vistas faltantes, filas descartadas por rol (antes silenciosas) y el **manifiesto esperado** de
`blocks-library.dwg` (lista de bloques + parámetros dinámicos reales + huella SHA-256, con comparación de
versión/huella)— más un **modo estricto** para despliegues. Los nombres de parámetro viven en una **fuente única
de dominio** (`SeccionRoles`, `CatalogBlockParameters` sobre `SelectiveRackDefaults`/`SelectiveSafetyDefaults`) que
consumen el proveedor, los productores y el validador; una **guardia por igualdad exacta** cruza, por
`PieceId+View+BlockName`, lo que escriben los builders reales de las **13 familias** contra `ExpectedParameters` y
el manifiesto (ni de menos ni de más), con matriz de cobertura bidireccional. Sobre el catálogo distribuido
reporta el **baseline aprobado por el dueño**: 1 error `DUPLICATE_ID` (`TROQUEL_TOPE`, hallazgo pre-existente que
**no** corrige) y 2 advertencias `UNRESOLVED_BLOCK_PIECE` (`TARIMA_GENERICA`); huella esperada `1a31c1a9…`.
**No** toca catálogos, DWG, geometría, BOM, persistencia ni reglas de producto (el código nunca abre el DWG).
AutoCAD **no requerido** (`requires_autocad: false`); **owner-validation aprobada**. Rebasada sobre `main` vigente
(`e2057d7`, tras I-14 e I-12), reconciliando sólo la entrada de `docs/initiatives/README.md`. La rama se integra
por `git merge --no-ff` en esta sesión.

**I-15** (`architecture/editor-shell`) cierra el **Editor Shell** de la pista C de UI y **adopta** su
infraestructura en las ventanas reales, **sin cambio de comportamiento observable**. Crea en
`src/RackCad.UI/Editor/` una `RackEditorSession<TDesign,TSystem>` (catálogo, identidad GUID+nombre
`RackEditorIdentity`, recomputación coalescida `RecomputeGate`/`RecomputeDebouncer` y el contrato de
inserción/actualización), la jerarquía `RackInsertionRequest` por `Kind`, y un registro **explícito sin
reflexión** `IRackEditorModule`+`EditorModuleRegistry` (mismo patrón que el `SystemRegistry` de I-08). El
**menú principal** y la **biblioteca** consumen ese registro en lugar de las ~19 propiedades de payload y
los cinco handlers por sistema (mata el crecimiento O(N) de `RackMainMenuWindow`, hallazgos E3/E5/U1); el
único consumidor del payload en el Plugin (`RackMenuCommands.RackCad`) lee un `RackInsertionRequest` y
despacha por `Kind` a los **mismos** `Draw*`. Las **cuatro ventanas ricas** (selectivo, dinámico, cama,
cabecera) **adoptan** el shell para esas cuatro capacidades —sus props públicas pasan a getters sobre la
sesión— eliminando la duplicación real vigente (idiom de GUID ×3, la clase `RecomputeDeferral`, el
debounce de la cabecera y el bloque de flags de `RequestDraw` ×3); el larguero, sin identidad ni
inserción, no adopta. El **estado propio** de cada editor (matriz por fondo y `BuildSystem` del selectivo,
`Recompose`/módulos del dinámico) **queda reservado para I-20/I-21**. **No** cambia geometría, BOM, GUID,
edición multivista, persistencia (I-11 intacto: los `*SourceProjectToInsert`/`*SourceDocumentToInsert` se
transportan tal cual), formatos ni la UI/etiquetas/orden del menú (`RackMainMenuWindow.xaml`
byte-idéntico a `main`). El dueño **aprobó la validación manual en AutoCAD** (menú, biblioteca, comandos
directos `RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA` y `RACKEDITAR` round-trip con el mismo GUID) y la
**owner-validation** de comportamiento y apariencia. Rebasada sobre `main` vigente (`646614d`, Merge I-19
sobre I-12), reconciliando `docs/initiatives/README.md` (conflicto manual único, preservando I-14/I-19),
los dos `.csproj` (el `LangVersion`/`Nullable` centralizados por I-12 heredados **+** `InternalsVisibleTo`
y la copia de catálogos de I-15) y `docs/ideas-futuras.md`, **sin** incorporar código funcional de I-19.
La rama se integra por `git merge --no-ff` en esta sesión.

**I-21** (`refactor/dynamic-editor-state`) cierra la **extracción del estado del editor dinámico** a
`RackCad.Application`, **sin cambio de comportamiento observable**. Mueve a
`src/RackCad.Application/Systems/` lo que era code-behind privado de `RackDynamicSystemWindow`:
`DynamicEditorCell`/`DynamicEditorFront`/`DynamicEditorValues` (filas/celdas y buffer de edición),
`DynamicFrontMatrix` (la matriz frente×nivel **y la selección** con todas sus mutaciones —alta/baja de
frentes, ajuste, toggle, commit, aplicar por alcance vía `DynamicRackCellScopeResolver`, snapshot/rollback,
refresco/restauración desde el sistema resuelto y la proyección a `DynamicRackFrontDesign`—),
`DynamicEditorSafety` (la regla «dibuja» y la copia de selecciones) y
`DynamicAnnotationOptions`+`DynamicEditorDesignAssembler` (la **recomputación y construcción del diseño**:
`MustRebuild`, `Snapshot`/`RestoreHeaderFondos`, `UpdateHeaderHeightInPlace`, `BuildDesign`, componiendo el
builder y el resolver existentes sin duplicar geometría). La ventana queda **coordinando controles, eventos,
render y diálogo** sobre el Editor Shell (I-15); `Recompose` conserva su orquestación y el code-behind baja
de ~3,339 a ~2,838 líneas. **No** cambia geometría, planes, BOM, GUID, nombre, `Section`, edición
multivista, persistencia I-11, metadatos desconocidos, fallbacks legacy, cabeceras legacy ni la cama
integrada; el XAML es idéntico. El dueño **aprobó la validación manual en AutoCAD** (matriz, selecciones y
aplicación por alcance; cabeceras calculadas y personalizadas; seguridad/IN-OUT/intermedios; previews y
vistas vinculadas; geometría/BOM; biblioteca/legacy/round-trip; actualización en sitio con el mismo GUID) y
la **owner-validation**. `origin/main` **no avanzó** desde `bfda406` (Merge I-15): **sin rebase**; la rama se
integra por `git merge --no-ff` en esta sesión.

**I-20** (`refactor/selective-editor-state`) entrega el **primer eslabón de la extracción de estado por
editor** de la pista C de UI (hallazgos U1/U3), **sin cambio de comportamiento observable**. Extrae el
**estado propio** del editor selectivo —hoy en campos privados de `RackSelectiveWindow` (~2,452 líneas,
archivo caliente)— a clases **puras y testeables** de `RackCad.Application.Systems`: `SelectiveEditorState`
(dueño de la matriz de trabajo, las matrices por fondo, la selección y las cabeceras/peraltes por poste, con
las operaciones `InitMatrix`, snapshot/restore, save/load fondo, `CloneAligned`, `ResizeBays`, add/remove
nivel, `ClampSelection`, `ApplyScope`, `MaxFrenteCount`, `SyncPostCabeceras`, `BuildBayDesigns`,
`FondoMatrixFromDesignBays` y `BuildDesign`), más `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`
(equivalentes verbatim de los anidados `Cell`/`FondoMatrix`), `SelectiveApplyScope` y `SelectiveDesignInputs`
(el contrato de entradas escalares que la ventana lee de sus controles). La ventana **observa** ese estado
(propiedades de acceso) y **delega** las operaciones; conserva el pintado (matriz + previews), el editor de
celda, los eventos, la recomputación coalescida (shell I-15), la orquestación de carga y el resolve/preview
ligado al catálogo (`BuildSystem`). La **superficie pública que consume el Plugin** (`InsertRequested`,
`SystemToInsert`, `DesignToInsert`, `RackId`, `RackName`, `InsertView`, `UpdateOnly`, `SetDimensionStyles`,
`LoadExisting`, `LoadForNew`, `Session`) **no cambia**. **No** cambia geometría, BOM, GUID, inserción/
actualización, persistencia ni metadatos **I-11**, catálogos, compatibilidad legacy ni round-trip; la
equivalencia queda fijada por una **caracterización STA** que verifica que `load→build` produce el **mismo
dibujo resuelto** (frontal por fondo + planta + cortes laterales + altura) antes y después del refactor.
**No** implementa I-22 (colocación de seguridad; orden fijo, I-20 primero), **no** toca el editor **dinámico**
(I-21) ni la asimetría vigente de estilos de cota. Alineado con `ARCHITECTURE.md` §7.1/§7.3 ("estado de editor
puro + vista WPF"; "el estado del editor se extrae a Application"). El dueño **aprobó la validación manual en
AutoCAD** y la **owner-validation** sin observaciones sobre la punta de implementación `0f43087`. **Rebasada
sobre `main` vigente (`2a30fef`, Merge I-21)** reconciliando sólo la documentación compartida (HANDOFF,
ROADMAP e índice de iniciativas); I-21 sólo toca el editor **dinámico**, no el selectivo, por lo que la
validación se conserva. La rama se integra por `git merge --no-ff` en esta sesión.

**I-05** (`feature/guardrail-unidades`) añade una **guardia de unidades** visible y **NO bloqueante** en el
límite de AutoCAD (hallazgo D4 de la auditoría; **ADR-0005 aceptado**), **sin cambio de comportamiento de
producto** salvo el aviso nuevo. `RackUnitsGuard` (en `RackCad.Plugin`, **único que lee `INSUNITS`**) mapea el
`UnitsValue` del `Database` activo a la categoría neutral `DrawingUnits` y delega la decisión en la política
**pura** `DrawingUnitsAdvisory` de `RackCad.Application` (sin dependencia de AutoCAD): si el dibujo **no** está
en pulgadas —incluido `unitless`— escribe **una** advertencia en la línea de comandos **antes de la primera
modificación del DWG**. **No** convierte, reescala ni reinterpreta geometría: RackCad sigue dibujando en
pulgadas (la conversión real queda **diferida** a una iniciativa futura, ADR-0005). Cableada **una vez por
operación** (sin repetir por alias ni por vista/bloque) en las rutas de inserción: menú `RACKCAD`,
`RACKSELECTIVO`, `RACKSISTEMADINAMICO`, `QUICKCAMA`, `RACKCABECERA`, `QUICKCABECERA`; en `RACKEDITAR` avisa
**solo al insertar una vista nueva** (`!UpdateOnly`, antes del primer `RedrawInPlace`), **no** en una
actualización pura; y `RACKLAYOUT`/`RACKRELLENAR` (con sus alias) avisan antes de sus prompts. `RACKDUPLICAR`
queda fuera (clona geometría ya dibujada a la misma escala). El cableado se fija con **source-guards** (leen el
`.cs` del Plugin como texto, sin cargar AutoCAD) y la decisión pura con pruebas. El dueño **aceptó ADR-0005** y
**aprobó la validación en AutoCAD 2025** y la owner-validation. La rama se integra por `git merge --no-ff` en
esta sesión.

**I-24** (`refactor/ui-tests-editores`) cierra la **pista de UI** con **pruebas de editores** en
`tests/RackCad.UI.Tests` (hallazgo U3), **sin cambio de comportamiento**: es una iniciativa de pruebas más un
**único seam interno** de prueba. Añade **29 pruebas** (139→168 UI): el `RackFrameConfiguratorViewModel` —antes
**sin ninguna prueba**— (mutaciones estructurales altas/bajas/división/combinación de horizontales, recomputación
síncrona del modelo físico, arreglos de bracing, selección múltiple, BOM, persistencia round-trip, rutas negativas
deterministas); la **adopción del estado dinámico** (I-21) por `RackDynamicSystemWindow`, caracterizada por
**punto fijo del doble build** con una **firma COMPLETA del dibujo** —todos los cortes laterales (con su índice) +
frontal de salida + frontal de entrada + planta, por instancia, **incluidas anotaciones y cotas** (`Text`/
`DimensionOffset`/`DimensionStyleName`), normalizando el `Name` del sistema resuelto antes de comparar— sobre un
diseño **no default** con valores por celda/larguero y opciones de anotación verificados en round-trip; y la
**identidad/inserción/actualización round-trip** de las ventanas **selectiva** y de **cama**. Las pruebas de
inserción/actualización recorren los **handlers WPF reales** (Click real vía `RaiseEvent(ButtonBase.ClickEvent)`
sobre los botones de la ventana, **no** `session.RequestInsert/RequestUpdate` directo), verificando
identidad/nombre/vista/sección/`UpdateOnly`, el **tipo concreto** de `InsertionRequest`, la **correspondencia
estricta** del payload (la firma del dibujo construida desde `request.Design` —resolviéndolo y normalizando el
nombre— iguala la construida desde `request.System`) y la metadata de origen **I-11**. El **único cambio de
producción** es el seam interno `RackDynamicSystemWindow.BuildDesignForTest` (reenvía al `Recompose` privado
existente, **sin reglas nuevas**, no usado en producción; +10 líneas). **No** cambia XAML, geometría, BOM,
persistencia, handlers, Draw Services, catálogos, bloques ni reglas de producto; por eso **no** requiere
validación en AutoCAD (`requires_autocad: false`) ni owner-validation (`requires_owner_validation: false`).
**Rebasada sobre `main` vigente (`a50c4ec`, Merge I-05)** reconciliando **sólo** el índice de iniciativas
(`docs/initiatives/README.md`: se conservan íntegras las entradas de I-05 e I-24). La rama se integra por
`git merge --no-ff` en esta sesión.

**I-22** (`refactor/safety-placement`) salda los hallazgos **E6** y **E7** de la auditoría 2026-07 sobre la
**seguridad del selectivo**, **sin cambio de comportamiento observable** (fijado por caracterización **golden**:
multiset de instancias Safety/Tope/Separator/Pallet en frontal/lateral/planta + el BOM de seguridad, en 7
escenarios que incluyen medio frente y cuádruple profundidad). Cuatro entregas: (1) **servicios/planes puros de
colocación por familia parametrizados por vista** —`SelectiveTopePlan` (topes físicos por spot + su resultado
**frontal** propio `BuildFrontal`), `SelectiveTarimaPlacement`, `SelectiveSeparadorPlan`, y la unificación del
consumo de `SelectiveParrillaPlan` (`Cells`/`DeckCells`)— con los builders frontal/lateral/planta y el BOM como
**orquestadores** (mueren las travesías duplicadas por vista `TallyByTramo`/`ParrillaExistsAt` y las fórmulas
copiadas de subida-y-snap); la regla de cada familia vive en **un solo sitio**. (2) **Descomposición por subtipo**:
`SelectiveSafetySelection` compone `SelectiveTopeConfig`/`SelectiveDesviadorConfig`/`SelectiveParrillaConfig`/
`SelectiveDefensaConfig`/`SelectiveGuiaConfig` (cada una con `DeepCopy` propio; las propiedades planas se conservan
como accesos delegados), y la persistencia mapea con **DTO reales por familia** (`SafetySelectionDocuments.cs`,
`From`/`ToDomain`/`WriteInto`/`ReadFrom`) que **aplanan/desaplanan** contra el `SafetySelectionDocument` **plano**
—el formato de alambre queda **byte-idéntico** (compartido con la ruta dinámica), sin JSON anidado ni convertidores,
con fallback legacy y round-trip por subtipo. (3) **Paso de troquel único**: los 5 sitios que hardcodeaban `2.0`
como snap referencian `SelectiveRackDefaults.TroquelPaso` (mismo valor, mismo resultado). (4) **Adopción de
`SelectionMatrix`** (I-14) con soporte de **celda ausente** (rejillas dentadas; `CellCount` cuenta solo presentes,
`Toggle` sobre ausente no reporta cambio) por las rejillas **tope/desviador/guía-entrada**, conservando idénticos
contenido, cabeceras, orden, off-cells y controles auxiliares. El **frontal de tope** conserva su naturaleza
esquemática por frente: `BuildFrontal` resuelve su intención pura (celdas activas, niveles, tramos cargados,
offsets, longitud+allowance, Y fuente del snap) como un resultado **distinto** de los spots físicos —no los
proyecta, para no duplicar en pares por fondo—, y `AddTopes` solo proyecta la vista. **No** cambia geometría,
planes, BOM, GUID, identidad, inserción/actualización, persistencia ni metadatos **I-11**, catálogos, nombres de
bloque, mensajes, selección, defaults, interacción visible ni comportamiento multivista; **fuera de alcance** I-25
(guardas traseras), Push Back/I-18, el editor **dinámico**, el rediseño visual y las reglas de producto (parrilla
con contador por celda y defensa por poste **no** se fuerzan a matriz plana). Alineada con `ARCHITECTURE.md`
§7.3-7.4 (servicios de colocación por familia/vista; configuraciones de seguridad por subtipo con DTO propio).
**Rebasada dos veces sobre el trunk vigente** (`9a895e4`→`a50c4ec` Merge I-05→`27ffdf3` Merge I-24) reconciliando
**sólo** documentación compartida (índice de iniciativas; `ideas-futuras` auto-fusionado); I-05 e I-24 tocan código
**disjunto** de I-22. El dueño **aprobó la validación en AutoCAD 2025 y la owner-validation sin observaciones**
(§2 y §5). La rama se integra por `git merge --no-ff` en esta sesión.

**I-17** (`refactor/clon-unico-cabecera`) unifica las **tres** implementaciones de deep-clone de
`RackFrameConfiguration` (hallazgo **U4** de la auditoría: una manual + dos por serialización, la manual
desincronizada con cada campo nuevo) en **un solo** `RackFrameProjectStore.DeepCopy` —el round-trip del store de
serialización— que el **dinámico** (`RackDynamicSystemWindow.Clone`), el **selectivo**
(`RackSelectiveWindow.CloneCabecera`) y el **configurador** (`RackFrameConfiguratorViewModel`) consumen; se
elimina el clon manual campo-por-campo del configurador (`CopyConfiguration` + 7 ayudantes) y sus dos rutas
reasignan `Configuration`. El clon es **completo**: el modelo **persistido** por el documento, el **derivado**
(`Members`, elevaciones, miembros por panel) reconstruido en la carga por `RefreshPhysicalModel`, y las
**excepciones runtime** (`FrameExceptionOverride`) —que el documento **no** persiste ni `RefreshPhysicalModel`
reconstruye— **reanexadas dentro del propio `DeepCopy`** (con `CloneException`), **sin** tocar el DTO, el formato
de alambre ni `Save`/`Load`. **No** cambia dibujo, geometría, BOM, GUID, persistencia física, DTO, catálogos, los
stores de **I-03** ni la UI: el diseño clonado es **idéntico**, fijado por comparación **profunda** del grafo
(modelo persistido, derivado **miembro-a-miembro** y excepciones), una **guarda por reflexión** que obliga a
clasificar toda propiedad futura de `RackFrameConfiguration` como persistida/derivada/runtime-preservada, y una
**regresión de I-11** que prueba la preservación de `ExtensionData` al clonar la cabecera vía
`WithSourceMetadataFrom`. Por eso **no** requiere validación en AutoCAD (`requires_autocad: false`) ni
owner-validation (`requires_owner_validation: false`; AUTOMATION_PLAN I-17 = no|no|no). `origin/main` **no
avanzó** desde la base `f674bd4`: **sin rebase**; I-03 (`refactor/fallos-silenciosos`) sigue activa en su worktree
pero **no** integrada (sin conflicto en `RackFrameProjectStore.cs`, cuyo cambio de I-17 es aditivo). La rama se
integra por `git merge --no-ff` en esta sesión.

**I-03** (`refactor/fallos-silenciosos`) salda los hallazgos **P1** y **D2** de la auditoría 2026-07 (fallos
silenciosos), **sin cambio de comportamiento funcional** (cambio **aditivo**: añade un rastro, no altera el
flujo). Un **logger mínimo** best-effort en `RackCad.Application.Diagnostics` (`RackLog` fachada +
`RackDiagnosticsLog` escritor + `RackLogFormatter` puro) escribe a `%AppData%\RackCad\logs` (nunca lanza,
thread-safe); `Report()` del Plugin registra la excepción completa **con stack** conservando idéntico su mensaje
de línea de comandos (cubriendo de paso todos los `catch (ex) => Report(ex)`); los **14 `catch`** antes
silenciosos del Plugin y `RackCatalogLoader` (fallo de carga + aviso de catálogo vacío) registran y siguen
tragando igual. Las escrituras de los **4 stores** (`RackProjectStore`, `RackFrameProjectStore`,
`UserTemplateStore`, `UserSettingsStore`) pasan por un helper de **escritura atómica** (`AtomicFile`: temp +
`File.Replace`/`Move`, sin crear el directorio destino, conservando la precondición de cada store). La carga de
los stores best-effort (`UserSettingsStore`/`UserTemplateStore`) **distingue por la excepción** un archivo
ausente (`FileNotFoundException`/`DirectoryNotFoundException` → default silencioso) de uno ilegible
(`JsonException` → cuarentena `.bad` + log; cualquier otro fallo de lectura → log sin cuarentena), y `CorruptFile`
registra también el **fallo secundario** al mover el `.bad`. **Preserva I-11** (versiones, metadata, geometría,
BOM, GUID, formatos, fallback legacy y la clave del Xrecord) y deja idénticos comandos, alias y mensajes visibles;
**no** toca catálogos, `deploy/`, `.sln` ni el `.csproj` del Plugin (solo añade `InternalsVisibleTo(RackCad.Tests)`
en el de Application). **No requiere validación en AutoCAD** (`requires_autocad: false`; ROADMAP no la marca con
✋) ni owner-validation. La rama se integra por `git merge --no-ff` en esta sesión.

## 2. Última validación real

La última validación manual de comportamiento sigue siendo I-02 sobre `b0de31d`, después del rebase
sobre `main`: el dueño cargó el DLL Debug del worktree en AutoCAD 2025 y confirmó el checklist
completo del dinámico modular, incluidos vistas, seguridad, BOM, persistencia, round-trip, escenario
legacy y rendimiento. No se realizó ni se requiere una validación nueva en AutoCAD para I-06 porque
su alcance es documental.

La guía vigente para futuras validaciones está en
[guias/validacion-manual-autocad.md](guias/validacion-manual-autocad.md).

I-26 no requirió validación en AutoCAD. El dueño confirmó el CI de rama, incluidos tests y build UI,
y descargó el artifact de cobertura con el XML esperado antes de autorizar su integración.

I-13 tampoco cambia dibujo ni comportamiento de runtime. El dueño autorizó la integración después
de verificar el build limpio del Plugin y la aplicación documental de I-29; no se requiere una
validación adicional mediante NETLOAD.

I-09 no cambia dibujo ni comportamiento de runtime (refactor de la superficie de comandos del
Plugin). No requirió validación en AutoCAD: la equivalencia se sostiene con el inventario de los 26
`[CommandMethod]`, la revisión mecánica del refactor y el CI verde de la rama. AutoCAD: no ejecutado;
no requerido por contrato al conservar comportamiento mediante equivalencia mecánica, builds y CI.

I-08 no cambia dibujo ni BOM (refactor de persistencia/biblioteca de Application con adaptación mínima
de UI). No requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca con ✋). La
**owner-validation quedó aprobada** por el dueño: recorrió el checklist de la biblioteca de diseños —las
cinco etiquetas, abrir un diseño de cada tipo con su editor correcto, una cabecera legacy sin `Kind`— y
confirmó que no cambió dibujo, BOM ni edición posterior. AutoCAD: no ejecutado como validación formal de
geometría; no requerido por contrato. La equivalencia se sostiene con la caracterización golden/round-trip
y el CI verde de la rama.

I-16 (`refactor/draw-services`) SÍ cambia la superficie de dibujo del Plugin y por eso se validó en AutoCAD:
el dueño cargó por `NETLOAD` el DLL Debug del worktree I-16 (build sobre `2d276a6`, SHA-256 `6AEF0F4D…906B`)
en AutoCAD 2025 y **aprobó** la matriz por familia (selectivo, dinámico con `postIndex` y entrada/salida,
cama, cabecera, cancelación del jig, persistencia y edición posterior) **sin observaciones**; registro en
[initiatives/I-16-autocad-validation.md](initiatives/I-16-autocad-validation.md). El avance del trunk por
I-08 cambia solo Application/UI y **no toca la superficie de dibujo del Plugin**, por lo que esa validación
se conserva tras el rebase (WORKFLOW §6).

I-10 (`architecture/kind-handlers`) **no** cambia dibujo, geometría, BOM funcional, GUID ni persistencia: es
un refactor del despacho por el `Kind` del sobre en el Plugin cuyos handlers son fachadas delgadas hacia el
código de edición/BOM/restamp **sin cambiar** (no toca la superficie de dibujo, a diferencia de I-16). No
requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca con ✋) ni owner-validation
(`requires_owner_validation: false`), por analogía directa con I-09: la equivalencia se sostiene con el
inventario mecánico (26 `[CommandMethod]` idénticos a `origin/main`, 0 `switch`/cadena por el `Kind` del
sobre restante, 5 `Guid.NewGuid`, 7 `Regen`, mensajes y etiquetas verbatim), la suite completa y el CI verde
de la rama. AutoCAD: no ejecutado; no requerido por contrato.

I-11 (`architecture/persistencia-uniforme`) **sí** requiere validación en AutoCAD (`requires_autocad: true`)
porque toca el round-trip de persistencia. El dueño ejecutó la matriz manual (§10 del contrato) por `NETLOAD`
del DLL Debug (código `eea1c11`) en AutoCAD 2025 y **aprobó todos los escenarios sin observaciones**, incluidos
**B5**, **B6** y **S7**; registro en
[automation/evidence/I-11-autocad-validation.md](automation/evidence/I-11-autocad-validation.md). La
**owner-validation** (biblioteca legacy más preservación de un campo desconocido, con DWG/envelope opcional)
quedó **aprobada** por el dueño (`requires_owner_validation: true`).

I-14 (`architecture/ui-controls`) **no** cambia dibujo, geometría, BOM ni persistencia: crea controles WPF
reutilizables **sin wirearlos** a ninguna ventana existente (patrón strangler), así que no hay comportamiento ni
apariencia nuevos que validar. No requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca
con ✋) ni owner-validation (`requires_owner_validation: false`). La cobertura se sostiene con las **85 pruebas**
de `tests/RackCad.UI.Tests` (lógica pura + instanciación STA de los controles) y el CI verde de la rama, incluido
el nuevo job `ui-tests`. AutoCAD: no ejecutado; no requerido por contrato.

I-15 (`architecture/editor-shell`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque el
menú, la biblioteca, los comandos directos y `RACKEDITAR` toman ahora la identidad y la inserción de la sesión
compartida. El dueño ejecutó el checklist completo por `NETLOAD` del DLL Debug del worktree I-15 (código
`2bd5703`;
`…-I-15-editor-shell\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025 y
**aprobó todos los escenarios sin observaciones**: menú `RACKCAD` (etiquetas, orden y flujos), apertura e
inserción desde biblioteca para todos los tipos, comandos directos `RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA`,
`RACKEDITAR` (actualización en sitio y vistas enlazadas con el **mismo GUID**), geometría y BOM **sin
diferencias**, metadatos y persistencia **I-11 preservados**, recomputación/previews de selectivo y cabecera
**fluidos**, y larguero que **abre y guarda pero no inserta**. La **owner-validation** de comportamiento y
apariencia quedó **aprobada** (`requires_owner_validation: true`). La confirmación normativa del dueño consta en
esta sesión; el gate manual queda cerrado.

I-21 (`refactor/dynamic-editor-state`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque
el editor dinámico produce el diseño que se dibuja. El dueño probó a profundidad el módulo dinámico por
`NETLOAD` del DLL Debug del worktree I-21 (código `779ee0c`;
`…-I-21-dynamic-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025
y **aprobó sin observaciones**: comportamiento y apariencia de la ventana; matriz, selecciones y aplicación
por alcance; cabeceras calculadas y personalizadas; seguridad, IN/OUT e intermedios; previews y vistas
vinculadas; geometría y BOM; biblioteca, persistencia legacy y round-trip; actualización en sitio y
conservación del GUID; registro en
[automation/evidence/I-21-autocad-validation.md](automation/evidence/I-21-autocad-validation.md). La
**owner-validation** de comportamiento y apariencia quedó **aprobada** (`requires_owner_validation: true`).

I-20 (`refactor/selective-editor-state`) **sí** requiere validación en AutoCAD (`requires_autocad: true`)
porque la ventana selectiva —cuyo estado se reescribió para observar `SelectiveEditorState` y construir el
diseño vía él— dibuja el rack real. El dueño ejecutó el checklist completo por `NETLOAD` del DLL Debug del
worktree I-20 (punta aprobada de implementación `0f43087`;
`…-I-20-selective-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025
y **aprobó todos los escenarios sin observaciones**: la matriz (clic en celda, ±niveles, altura por frente,
`Piso`, medio frente), «Aplicar a:» celda/nivel/frente/todas, «Editando fondo» (doble/triple profundidad con
separadores), previews frontal y lateral, «Insertar frontal», y con `RACKEDITAR` «Actualizar» en sitio e
«Insertar lateral/planta» ligadas con el **mismo GUID**; geometría y BOM **sin diferencias**, metadatos y
persistencia **I-11 preservados** (incluida la reapertura desde biblioteca, `LoadForNew`), round-trip íntegro.
La **owner-validation** de comportamiento y apariencia (idénticos a lo vigente) quedó **aprobada**
(`requires_owner_validation: true`). Los únicos cambios posteriores a esa aprobación son la **corrección de un
comentario obsoleto** (doc-comment, sin efecto en el comportamiento), el **rebase sobre `main` con I-21** (sólo
documentación compartida; I-21 no toca el selectivo) y este cierre documental; por eso la validación **se
conserva** (WORKFLOW §6). El gate manual queda cerrado.

I-05 (`feature/guardrail-unidades`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque el
aviso aparece en la línea de comandos al insertar. El dueño cargó por `NETLOAD` el DLL Debug del worktree I-05
(implementación validada `f78baaf`;
`…-I-05-guardrail-unidades\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025 y
**aprobó sin observaciones** («Ok, todo funciona»): dibujo en **pulgadas** ⇒ **sin** aviso; **no-pulgadas** y
**unitless** ⇒ **una** advertencia por operación (confirmó que apareció la advertencia completa de RackCad); el
aviso **no bloquea** ni convierte/reescala; `RACKEDITAR` diferencia **actualización** (sin aviso) e **inserción
de vista nueva** (con aviso); `RACKLAYOUT`, `RACKRELLENAR` y los alias se comportan correctamente sin doble
aviso; geometría, BOM, GUID, capas, persistencia y round-trip **idénticos**. La **owner-validation** quedó
**aprobada** (`requires_owner_validation: true`) y **ADR-0005** fue **aceptado** por el dueño; evidencia en
[`automation/evidence/I-05-autocad-validation.md`](automation/evidence/I-05-autocad-validation.md) y decisión en
[`automation/decisions/I-05.md`](automation/decisions/I-05.md). `origin/main` no avanzó desde `9a895e4`, así que
la validación se conserva (WORKFLOW §6): sin rebase.

I-22 (`refactor/safety-placement`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque el
refactor toca el código que produce las piezas de seguridad dibujadas (aunque el diseño resuelto es idéntico por
construcción y queda fijado por la equivalencia golden). El dueño cargó por `NETLOAD` el DLL Debug del worktree
I-22 (código validado `3ce7139`, SHA-256 `969580AE…038C`;
`…-I-22-safety-placement\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025 y
**aprobó sin observaciones** («Listo, probé todo, parece estar correcto»): geometría y colocación de topes,
parrillas, tarimas, separadores y elementos relacionados; BOM; vistas frontal/lateral/planta; medio frente y
múltiples fondos; actualización y vistas ligadas con conservación del **mismo GUID**; persistencia, biblioteca y
round-trip; y la **apariencia e interacción** de las rejillas `SelectionMatrix` (tope/desviador/guía). La
**owner-validation** quedó **aprobada** (`requires_owner_validation: true`); registro en
[`automation/evidence/I-22-autocad-validation.md`](automation/evidence/I-22-autocad-validation.md). `origin/main`
está en `27ffdf3` (Merge I-24) y no avanzó desde que la rama quedó rebasada sobre esa punta, así que la validación
se conserva (WORKFLOW §6): sin rebase adicional.

I-17 (`refactor/clon-unico-cabecera`) **no** cambia dibujo, geometría, BOM, GUID ni la persistencia física: es un
refactor que unifica el mecanismo de clonado de `RackFrameConfiguration`, cuyo resultado es **idéntico** por
construcción (fijado por la comparación **profunda** del grafo —persistido + derivado miembro-a-miembro +
excepciones—, la guarda de clasificación por reflexión y la regresión de I-11). No requiere validación en AutoCAD
(`requires_autocad: false`; ROADMAP no la marca con ✋) ni owner-validation (`requires_owner_validation: false`),
por analogía directa con I-09/I-10/I-24: la equivalencia se sostiene con la suite completa (**993** `RackCad.Tests`
+ **184** `RackCad.UI.Tests`) y el CI verde de la rama (run `29952433309` sobre `28e5cfe`, cuatro jobs). AutoCAD:
no ejecutado; no requerido por contrato.

I-03 (`refactor/fallos-silenciosos`) **no** requiere validación en AutoCAD (`requires_autocad: false`; el ROADMAP
no la marca con ✋) ni owner-validation: es un cambio **aditivo** de diagnóstico (logging + escritura atómica) que
**no** altera geometría, BOM, GUID, comandos ni mensajes visibles. La cobertura se sostiene con las pruebas puras
(formatter, escritor real a directorio temporal, `AtomicFile`, distinción de carga por excepción y **negativos
deterministas rojo→verde**) y los builds; el logger se prueba a través de un seam mínimo (`RackLog.RedirectForTests`)
que evita escribir en el `%AppData%` real. `origin/main` **avanzó** de `f674bd4` a `b60f142` (Merge I-17) durante
la integración, así que la rama se **rebasó** sobre `b60f142` (reconciliación **exclusivamente documental**;
el código de I-03 e I-17 es disjunto salvo `RackFrameProjectStore.cs`, aditivo por ambos lados y auto-fusionado).
AutoCAD: no ejecutado; no requerido por contrato.

## 3. Problemas y riesgos activos

- `ParrillaFrente` y `ParrillaCantidad` siguen siendo globales al rack; una configuración
  heterogénea puede requerir overrides por frente o nivel en una iniciativa futura.
- En medio frente, la cantidad de parrilla es por tramo; el comportamiento es intencional, pero
  debe comprobarse contra el uso real.
- El build del Plugin puede emitir los `MSB3277` conocidos de las referencias de AutoCAD y falla al
  copiar DLL si AutoCAD los mantiene cargados.
- Tras **I-20 e I-21**, el **estado interno propio** de los editores **selectivo** (matriz por fondo, celdas,
  `ApplyScope`, `BuildDesign` → `SelectiveEditorState`) y **dinámico** (matriz frente×nivel, selección,
  recomputación/construcción → `DynamicFrontMatrix`/`DynamicEditorDesignAssembler`) ya vive en
  `RackCad.Application`; la ventana observa el estado y pinta. El resolve/preview ligado al catálogo
  (`BuildSystem` del selectivo, la orquestación `Recompose` del dinámico) permanece en cada ventana por
  diseño, consumiendo el diseño puro que producen esos estados — ya no es deuda pendiente.
- El menú `RACKCAD` abre el selectivo **nuevo** sin `SetDimensionStyles` (asimetría vigente que I-15 preservó
  verbatim; `RACKSELECTIVO` y el abrir-desde-biblioteca sí los fijan) — registrado en `docs/ideas-futuras.md`
  como hallazgo diferido, no corregido en I-15.
- El fallback legacy del dinámico conserva cabeceras sin procedencia como personalizadas para evitar
  pérdida de datos.
- Los catálogos de producto y los overrides del usuario aún comparten ubicación; I-04 preserva el
  DWG de bloques, pero la separación de capas de datos sigue diferida.
- La compilación del Plugin en GitHub-hosted runners depende de la excepción limitada de ADR-0003.
  Debe revisarse a más tardar el 2027-07-20 o antes ante cambios de proyecto, versiones, source,
  runner, caching, artifacts, audiencia, finalidad o documentación incompatible de Autodesk.
- GitHub Actions advierte que las acciones actuales basadas en Node.js 20 se ejecutan forzadamente
  sobre Node.js 24; es una deuda de infraestructura separada de I-13.
- `RackFrameProjectDocument` (biblioteca de cabecera desnuda por `RackFrameProjectStore`, un quinto DTO de
  persistencia) quedó **fuera del alcance de I-11 por decisión aprobada del dueño**; no preserva campos JSON
  desconocidos ni versión no degradada. Es deuda para una iniciativa posterior, no cancelación
  (`automation/decisions/I-11.md`).
- RackCad sigue siendo **mono-unidad en pulgadas**: I-05 añadió la guardia que **avisa** cuando `INSUNITS` no
  es pulgadas, pero **no** convierte ni reescala. La conversión real (frontera explícita DWG↔interno) queda
  **diferida** a una iniciativa futura gobernada por **ADR-0005** (aceptado); la columna `units` de los
  catálogos sigue decorativa. `RACKDUPLICAR` no avisa por diseño (clona geometría ya dibujada a la misma escala).

## 4. Siguiente acción

Con I-08, I-09, I-16, I-10, I-11, **I-14**, **I-12**, **I-19**, **I-15**, **I-21** e **I-20** integradas (**I-21**
y **I-20** en esta tanda de integración serializada), la **pista B del Plugin** está cerrada (la serialización
I-09 → I-16 → I-10 está completa), la **pista A de Application** entrega la persistencia uniforme, **I-12**
cierra el **versionado real** (versión única, SHA
estampado, bundle por `dotnet publish` verificado fail-closed, ADR-0004 aceptado), e **I-19** entrega el
validador de catálogos. La **pista C de UI** entrega ya **cuatro** eslabones: los **controles comunes**
(I-14), el **Editor Shell** (I-15, adoptado por las cuatro ventanas ricas: catálogo, identidad, recomputación
coalescida e inserción vía `RackEditorSession`; menú y biblioteca por `EditorModuleRegistry`) y la
**extracción del estado** de los editores **dinámico** (I-21: `DynamicFrontMatrix`/`DynamicEditorDesignAssembler`)
y **selectivo** (I-20: `SelectiveEditorState`) a Application, dejando ambas ventanas observando su estado y
pintando. Con **I-22** (`refactor/safety-placement`) **integrada en esta sesión** (colocación de seguridad del
selectivo; detalle abajo), el siguiente paso natural es **I-25** (`feature/guardas-traseras`, sobre I-22) e
**I-18 (Push Back)**, que ya tiene resueltas sus dependencias I-10, I-11, I-15 e I-16 y solo espera los **bloques
DWG del dueño**. Alternativamente, continuar I-07 (`docs/adr-retroactivos`) en su worktree ya reclamado. Además, **I-05** (`feature/guardrail-unidades`, relleno de Fase 1) queda **integrada**: la
**guardia de unidades** avisa cuando el dibujo no está en pulgadas, **sin conversión ni reescalado** (ADR-0005
aceptado); no desbloquea ni estorba ninguna otra iniciativa. **I-24** (`refactor/ui-tests-editores`, Fase 5) queda
**integrada** en esta sesión: **pruebas de editores** en `tests/RackCad.UI.Tests` (ViewModels + límites reales de
las ventanas por handlers WPF reales; **29** nuevas, 139→168 UI) más un **único seam interno** de prueba, **sin
cambio de comportamiento**; cierra el hallazgo U3 de la pista de UI. **I-22** (`refactor/safety-placement`, Fase 5)
queda **integrada** en esta sesión: salda **E6/E7** de la seguridad del selectivo (planes/servicios de colocación
por familia, configuraciones y DTO por subtipo, paso de troquel único y adopción de `SelectionMatrix`), **sin
cambio de comportamiento** (7 golden idénticos; AutoCAD + owner-validation aprobadas). Con I-22 integrada, el
siguiente paso natural pasa a ser **I-25** (`feature/guardas-traseras`, última familia de seguridad construida
sobre I-22, **ahora desbloqueada**) e **I-18 (Push Back)** (solo espera los **bloques DWG del dueño**); **I-23**
cierra la Fase 5 al final (depende de todas).

**I-17** (`refactor/clon-unico-cabecera`, Fase 3) queda **integrada** en esta sesión: **clon único de cabecera**
(`RackFrameProjectStore.DeepCopy`, hallazgo **U4**), **sin cambio de comportamiento** (detalle en §1 y §5); cierra
U4 y **no** desbloquea ni estorba otra iniciativa. El siguiente paso natural sigue siendo **I-25**/**I-18** (o
continuar **I-07** `docs/adr-retroactivos` en su worktree ya reclamado).

Con **I-03** (`refactor/fallos-silenciosos`, Fase 1) **integrada en esta sesión** —logger mínimo a
`%AppData%\RackCad\logs`, `Report()` con stack, los 14 `catch` del Plugin y los stores best-effort registrando,
escritura atómica en los 4 stores y carga que distingue archivo ausente de ilegible; **aditivo, sin cambio de
comportamiento**, no desbloquea ni estorba otra iniciativa— el relleno de robustez de Fase 1 (P1/D2) queda
cerrado. Continúa disponible **I-07** (`docs/adr-retroactivos`, en su worktree ya reclamado) y las Fases 4-5
(**I-25** sobre I-22, **I-18** Push Back a la espera de sus bloques DWG, **I-23** al final).

La automatización permanece pausada: no hay ejecutor nocturno activo ni horarios programados. El
desarrollo posterior continúa manualmente bajo WORKFLOW hasta que el dueño apruebe otro mecanismo y
un nuevo piloto controlado.

## 5. Última verificación vigente

**Baseline integrada de I-17 — 2026-07-22:**

- candidato de **código** validado por CI: `28e5cfeeccfbfe60ab844f1555d3580405ebfbb8` (CI run `29952433309`,
  **cuatro jobs verdes** —Tests (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build
  Plugin without AutoCAD); el commit documental de cierre recibe su propio CI verde antes del merge; este documento
  **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base `f674bd4` durante el ciclo de I-17: **sin rebase** (merge-base =
  `origin/main`, 0 commits detrás); I-03 (`refactor/fallos-silenciosos`) e I-07 (`docs/adr-retroactivos`) siguen
  activas en remoto pero **no** integradas, así que no hubo conflicto con `RackFrameProjectStore.cs` (el cambio de
  I-17 es aditivo); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **993/993 verdes** (981 base + **12** de I-17: 11 en `RackFrameConfigurationDeepCopyTests`
  —modelo persistido por forma de alambre, **excepciones sin compartir referencias**, **modelo derivado
  miembro-a-miembro** superiores y por panel, independencia, idempotencia, `null`, equivalencia con las dos rutas
  previas y la **guarda por reflexión** de clasificación de propiedades— + **1** regresión de **I-11** en
  `PersistenceReopenPreservationTests`); suite `RackCad.UI.Tests`: **184/184 verdes** (183 base + 1 de restore del
  configurador); filtro persistencia/I-11/DeepCopy **143/143**; la prueba de excepciones se verificó **fallando**
  con el reanexado desactivado (1 fallo/11 ok) y se restauró el fix;
- build UI Debug: **0 errores y 0 advertencias propias**; build solución completa Debug (Plugin incl., AutoCAD
  cerrado): **0 errores**, únicamente las dos familias `MSB3277` conocidas del Plugin;
- diff vs `origin/main`: **10 archivos** (`RackFrameProjectStore.cs` **aditivo**: `DeepCopy` + `CloneException`,
  sin tocar `Serialize`/`Deserialize`/`Save`/`Load`; 3 `.cs` de UI —dinámico/selectivo/configurador: comentarios +
  delegación + reasignación—; 2 archivos de prueba nuevos/modificados con la regresión I-11; 3 docs), **sin** tocar
  XAML, DTO (`RackFrameProjectDocument`), formato físico, catálogos, geometría, BOM, GUID ni los stores de **I-03**;
- objetivo entregado (hallazgo **U4**): **un solo** deep-clone de `RackFrameConfiguration` vía el store de
  serialización, con preservación **completa** del estado (persistido + derivado + excepciones runtime).

**Baseline integrada de I-03 — 2026-07-22:**

- punta de **código** de la rama: `ff6f460` (últimos cambios de código de la revisión de defectos 1-4:
  producción `52da117` + tests `ff6f460`); el commit de estado `c3a9c47` (candidato revisado) y este cierre
  documental **no cambian código**; el commit documental final recibe su propio CI verde antes del merge; este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **avanzó** de `f674bd4` a `b60f142` (Merge I-17) durante la integración: la rama se **rebasó** sobre
  `b60f142` reconciliando **sólo** documentación compartida (HANDOFF/ROADMAP/README/`ideas-futuras`) y **preservando
  el contenido integrado de I-17**; el código de I-03 e I-17 es disjunto salvo `RackFrameProjectStore.cs` (aditivo
  por ambos lados: I-17 añade `DeepCopy`, I-03 cambia `Save`, auto-fusionado); `ff6f460` es la punta de código y
  `c3a9c47` la previa al commit documental (ambos anteriores al rebase); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **1004/1004 verdes** (981 baseline + **19** iniciales de I-03 —`RackLogFormatter`,
  `RackDiagnosticsLog`, `AtomicFile`, distinción de carga en settings/templates— + **4** de la revisión —fachada
  redirigida y **3 negativos deterministas**—; **rojo→verde** demostrado en los 2 negativos de lógica: un error de
  lectura ≠ ausente no se registraba bajo la guarda `File.Exists`, y el fallo de cuarentena era silencioso, ambos
  verdes tras los fixes); suite `RackCad.UI.Tests`: **183/183 verdes** (sin cambio; I-03 no toca UI);
- build UI Debug y solución completa Debug: **0 errores**, únicamente las dos familias `MSB3277` conocidas del
  Plugin; el build del Plugin se ejecutó con **AutoCAD 2025 cerrado** (DLL no bloqueado);
- CI de rama verde sobre `c3a9c47` (run `29952811337`, **cuatro jobs verdes** —Tests (Domain+Application), Build UI,
  UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD—) y re-verde sobre el **commit documental
  final** antes del merge;
- objetivo entregado (P1/D2): logger mínimo best-effort a `%AppData%\RackCad\logs`
  (`RackLog`/`RackDiagnosticsLog`/`RackLogFormatter`, nunca lanza, thread-safe); `Report()` **con stack**
  conservando su mensaje; los **14 `catch`** antes silenciosos del Plugin y `RackCatalogLoader` (fallo de carga +
  aviso de catálogo vacío) registran; **escritura atómica** (`AtomicFile`, temp + `File.Replace`/`Move`, sin crear
  el directorio destino) en los 4 stores; carga que **distingue por excepción** archivo ausente de ilegible
  (`.bad` + log) en `UserSettingsStore`/`UserTemplateStore`, con `CorruptFile` registrando el fallo secundario de
  cuarentena; seam de prueba mínimo `RackLog.RedirectForTests` (**ninguna prueba escribe en el `%AppData%` real**);
- validación manual: **no requerida** (`requires_autocad: false`, `requires_owner_validation: false`; ROADMAP no la
  marca con ✋); **sin validaciones pendientes**;
- invariantes preservados (**compatibilidad I-11**): **sin** cambios de versiones, metadata, geometría, BOM, GUID,
  formatos/DTO persistidos, fallback legacy ni la clave del Xrecord; comandos, alias y mensajes de línea de comandos
  **idénticos**; catálogos, `deploy/`, workflows, `.csproj` del Plugin, `.sln` y DWG **intactos**; **sin**
  dependencias NuGet nuevas (solo `InternalsVisibleTo(RackCad.Tests)` en el `.csproj` de Application); dirección de
  dependencias intacta;
- alcance: `src/RackCad.Application/Diagnostics/{RackLog,RackDiagnosticsLog,RackLogFormatter,CorruptFile}.cs`,
  `Persistence/AtomicFile.cs` + los `Save` de `{RackProjectStore,RackFrameProjectStore}.cs`,
  `RackFrames/UserTemplateStore.cs`, `Settings/UserSettings.cs` y el `.csproj` de Application;
  `src/RackCad.Plugin/{RackCommandSupport,RackCatalogLoader,RackEnvelopeRestamp,RackInventarioCommands.BomTotal,
  RackLayoutCommands,RackLayoutCommands.Fill}.cs` + `Headers/{BlockLibraryImporter,BlockPlacement,
  LateralHeaderDrawer,LateralHeaderDrawService,RackBlockRenamer}.cs`; pruebas (`RackLogTests`, `RackLogTestSupport`,
  `AtomicFileTests`, `UserSettingsStoreTests`, `UserTemplateStoreTests`, `DiagnosticsNegativeTests`); y
  contrato/estado/índice/`ideas-futuras` de I-03.

**Baseline integrada de I-22 — 2026-07-22:**

- punta de **código** validada por CI y por el dueño (AutoCAD): `3ce71394f8858cf600b1e28d042ecebc5ba6a7c2`
  (ancestro de la punta publicada `1e78b2c`; CI run `29944500977` sobre `1e78b2c`, **cuatro jobs verdes**); los
  commits posteriores a `3ce7139` son **solo documentales** (registro de la validación + este cierre de
  integración) y **no cambian código**; el commit documental final recibe su propio CI verde antes del merge; este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **avanzó dos veces** durante el ciclo de I-22 (`9a895e4` Merge I-20 → **`a50c4ec`** Merge I-05 →
  **`27ffdf3`** Merge I-24): la rama quedó **rebasada** sobre la punta vigente `27ffdf3` (merge-base = `origin/main`,
  **0 commits detrás**); la reconciliación fue **exclusivamente documental** (índice de iniciativas;
  `ideas-futuras` auto-fusionado); I-05 (guardia de unidades en el Plugin) e I-24 (pruebas de UI + seam dinámico)
  tocan código **disjunto** de I-22, **cero solapamiento**, por lo que la validación en AutoCAD **se conserva**
  (WORKFLOW §6); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **981/981 verdes** (incluye la caracterización **golden** de 7 baselines —multiset
  frontal/lateral/planta + BOM, con medio frente y cuádruple profundidad—, los planes/DTO por familia, el
  round-trip por subtipo y **5 nuevas** de `SelectiveTopePlan.BuildFrontal`; sin regresión); suite
  `RackCad.UI.Tests`: **183/183 verdes** (154 de I-22 —adopción de rejillas + celdas ausentes— coexistiendo con las
  **29** de I-24 tras el rebase; sin regresión);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `1e78b2c` (run `29944500977`) y re-verde sobre el **commit documental final** antes del
  merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build
  Plugin without AutoCAD— en `success`);
- objetivo entregado (E6/E7 de la seguridad del selectivo): **planes/servicios puros de colocación por familia**
  parametrizados por vista (`SelectiveTopePlan` con su resultado **frontal** propio `BuildFrontal`,
  `SelectiveTarimaPlacement`, `SelectiveSeparadorPlan`, unificación de `SelectiveParrillaPlan` `Cells`/`DeckCells`)
  con los builders y el BOM como orquestadores (mueren `TallyByTramo`/`ParrillaExistsAt` y las fórmulas copiadas de
  subida-y-snap); **configuraciones por subtipo** (`SelectiveSafetyConfig`) con `DeepCopy` propio y **DTO reales por
  familia** (`SafetySelectionDocuments.cs`) que aplanan/desaplanan contra el `SafetySelectionDocument` **plano**
  (wire format byte-idéntico, fallback legacy, round-trip por subtipo); **paso de troquel único**
  (`SelectiveRackDefaults.TroquelPaso` en los 5 sitios); y **adopción de `SelectionMatrix`** con **celda ausente**
  (`CellCount`/`Toggle`) por las rejillas tope/desviador/guía;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño sin observaciones
  («Listo, probé todo, parece estar correcto»): topes/parrillas/tarimas/separadores y elementos relacionados; BOM;
  frontal/lateral/planta; medio frente y múltiples fondos; actualización y vistas ligadas con el **mismo GUID**;
  persistencia/biblioteca/round-trip; y **apariencia e interacción** de las rejillas `SelectionMatrix`;
  **owner-validation aprobada**; DLL Debug del worktree I-22 (código `3ce7139`, **SHA-256**
  `969580AE67EAC69C8018304F3A9DD963C7DDD77307D5A26E913C32CC1A31038C`); evidencia en
  [`automation/evidence/I-22-autocad-validation.md`](automation/evidence/I-22-autocad-validation.md);
- invariantes preservados: **sin** cambios de geometría, coordenadas, planes, BOM, GUID, identidad, inserción/
  actualización, persistencia ni metadatos **I-11**, catálogos, nombres de bloque, mensajes, selección, defaults,
  interacción visible ni comportamiento multivista (fijado por los **7 golden idénticos**); el **formato serializado**
  permanece byte-idéntico; catálogos, `deploy/`, workflows, `.csproj`, `.sln` y DWG **intactos**; **sin** dependencias
  NuGet nuevas; dirección de dependencias intacta (Application no referencia UI/AutoCAD);
- alcance: `src/RackCad.Application/Systems/Selective{TopePlan,TarimaPlacement,SeparadorPlan,SeparadorPlacement,
  ParrillaPlan,ParrillaPlacement,FrontalBuilder,LateralBuilder,PlantaBuilder,BomBuilder,GeometryResolver,TopePlacement}.cs`,
  `src/RackCad.Application/Persistence/{SafetySelectionDocuments,SelectivePalletDesignDocument}.cs`,
  `src/RackCad.Domain/Systems/{SelectivePalletDesign,SelectiveSafetyConfig}.cs`,
  `src/RackCad.UI/Controls/SelectionMatrix{,Model}.cs` + `Safety{Tope,Desviador,GuiaEntrada}GridWindow.cs`, más las
  pruebas (`SelectiveSafetyEquivalenceTests` +2 golden, `SafetySelectionDocumentsTests`, `SelectiveSafetyConfigTests`,
  `Selective{Tope,Tarima,Separador,Parrilla}PlacementTests`, `SelectiveTopePlanFrontalTests`,
  `SelectionMatrixAbsentCellTests`, `SafetyGridAdoptionTests`) y contrato/estado/evidencia/índice de I-22.

**Baseline integrada de I-05 — 2026-07-22:**

- punta de **código** validada por CI y por el dueño (AutoCAD): `f78baaf209c118d168c68620e236341996f9d93e`
  (run `29932135203`, **cuatro jobs verdes**); los commits posteriores de la rama son **solo documentales**
  (registro de aprobaciones + este cierre de integración) y **no cambian código**; el commit documental final
  recibe su propio CI verde antes del merge; este documento **no inventa** el SHA del merge de `main` (vive en
  `git log --first-parent main`);
- `origin/main` **no avanzó** desde `9a895e4` (Merge I-20) durante esta integración: **sin rebase**; `f78baaf`
  (implementación validada) es **ancestro** de la punta final de la rama; la rama se integra por
  `git merge --no-ff`;
- suite `RackCad.Tests`: **936/936 verdes** (base 913 de I-20 + **23 nuevas**: `DrawingUnitsAdvisoryTests` (6,
  decisión pura) y `RackUnitsGuardSourceTests` (17, source-guards del cableado, con demostración **rojo→verde**
  contra la baseline sin cablear); sin regresión); suite `RackCad.UI.Tests`: **139/139 verdes** (sin cambio;
  I-05 no toca UI);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `f78baaf` (run `29932135203`) y re-verde sobre el commit documental de cierre antes del
  merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y
  Build Plugin without AutoCAD— en `success`);
- objetivo entregado: `RackUnitsGuard` en `RackCad.Plugin` (**único lector de `INSUNITS`**, mapeo
  `UnitsValue`→`DrawingUnits`, **una** advertencia no bloqueante antes de la primera modificación) + política
  **pura** `DrawingUnitsAdvisory` en `RackCad.Application.Drawing` (sin AutoCAD); cableada una vez por operación
  en las rutas de inserción (menú, `RACKSELECTIVO`/`RACKSISTEMADINAMICO`/`QUICKCAMA`/`RACKCABECERA`/
  `QUICKCABECERA`), en `RACKEDITAR` solo al insertar vista nueva (`!UpdateOnly`, antes del primer
  `RedrawInPlace`) y en `RACKLAYOUT`/`RACKRELLENAR` antes de sus prompts; `RACKDUPLICAR` fuera por diseño;
  **ADR-0005 aceptado** (`docs/adr/0005-estrategia-de-unidades.md`);
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño sin
  observaciones (pulgadas sin aviso; no-pulgadas y unitless con una advertencia; aviso no bloqueante y sin
  conversión; `RACKEDITAR` actualiza vs inserta; layout/relleno/alias correctos; «Ok, todo funciona»);
  **owner-validation aprobada**; **owner-decision** (ADR-0005) **aprobada**;
- invariantes preservados: **sin** cambios de geometría, coordenadas, BOM, GUID, capas, persistencia/DTO,
  payload/Xrecord, comandos, alias ni mensajes ajenos; catálogos, `deploy/`, workflows, `.csproj` y `.sln`
  **intactos**; **sin** dependencias NuGet nuevas; dirección de dependencias intacta (Application no referencia
  AutoCAD); diff exclusivamente **aditivo**;
- alcance: `src/RackCad.Application/Drawing/DrawingUnitsAdvisory.cs` (nuevo),
  `src/RackCad.Plugin/RackUnitsGuard.cs` (nuevo) + 7 comandos del Plugin (cableado),
  `tests/RackCad.Tests/DrawingUnitsAdvisoryTests.cs` (+6) y `RackUnitsGuardSourceTests.cs` (+17), más ADR-0005,
  contrato/estado/decisión/evidencia e índices de I-05.

**Baseline integrada de I-24 — 2026-07-22:**

- punta de **código** validada por CI: `59dbf0bf5844aa5c228ac3de2d3e16fdcb95763f` (run `29941597964`, **cuatro
  jobs verdes**) antes del rebase final; tras rebasar sobre `origin/main` (`a50c4ec`) el código es **idéntico**
  (el rebase sólo reconcilió el índice de iniciativas), y tanto el **SHA rebasado** como el **commit documental
  de cierre** reciben su propio CI verde antes del merge; este documento **no inventa** el SHA del merge de `main`
  (vive en `git log --first-parent main`);
- `origin/main` **avanzó** de `9a895e4` (Merge I-20) a **`a50c4ec`** (Merge I-05) durante la sesión de I-24: la
  rama se **rebasó** sobre esa punta; la reconciliación fue **exclusivamente documental** (`docs/initiatives/README.md`,
  conservando **íntegras** las entradas de I-05 e I-24); I-05 e I-24 tocan código **disjunto** (I-05 = guardia de
  unidades en el Plugin; I-24 = pruebas de UI + un seam en la ventana dinámica), **cero solapamiento** de código;
- suite `RackCad.Tests`: **936/936 verdes** (sin regresión: I-24 no toca esa suite; las 936 vienen de la base
  rebasada con I-05); suite `RackCad.UI.Tests`: **168/168 verdes** (139 de la base + **29 nuevas**: 13 del
  `RackFrameConfiguratorViewModel`, 8 del dinámico, 4 del selectivo, 4 de la cama);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `59dbf0b` (run `29941597964`, pre-rebase) y re-verde sobre el **SHA rebasado** y el
  **commit documental final** antes del merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests
  (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`);
- objetivo entregado: **29 pruebas** en `tests/RackCad.UI.Tests` que cubren el cableado WPF inalcanzable por la
  suite pura: el `RackFrameConfiguratorViewModel` (antes sin pruebas), la adopción del estado dinámico por su
  ventana caracterizada por **firma COMPLETA del dibujo** (cortes laterales + frontal salida/entrada + planta, por
  instancia, **incluidas anotaciones y cotas**, con el `Name` normalizado antes de comparar) por **punto fijo del
  doble build**, y la identidad/inserción/actualización round-trip de las ventanas selectiva y de cama; las pruebas
  de inserción/actualización recorren los **handlers WPF reales** (`RaiseEvent(ButtonBase.ClickEvent)`) verificando
  identidad/nombre/vista/sección/`UpdateOnly`, el tipo concreto de `InsertionRequest`, la **correspondencia estricta**
  del payload (`request.Design` resuelto == `request.System`) y la metadata de origen **I-11**;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`) y **owner-validation no
  requerida** (`requires_owner_validation: false`): el único cambio de producción es un **seam interno sin
  comportamiento** (`RackDynamicSystemWindow.BuildDesignForTest`, reenvía a `Recompose`, +10 líneas, no usado en
  producción); la cobertura se sostiene con las suites automatizadas y el CI verde de la rama;
- invariantes preservados: **sin** cambios de XAML, geometría, BOM, GUID, inserción/actualización, persistencia,
  handlers, Draw Services, catálogos, bloques ni reglas de producto; el diff de producción vs `a50c4ec` es
  **exclusivamente** el seam de 10 líneas en `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs`; **sin** dependencias
  NuGet nuevas; dirección de dependencias intacta (las pruebas de UI no referencian AutoCAD);
- alcance: `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` (seam +10), `tests/RackCad.UI.Tests/` (5 archivos:
  `EditorWindowTestSupport`, `RackFrameConfiguratorViewModelTests`, `DynamicEditorWindowTests`,
  `SelectiveEditorWindowTests`, `FlowBedEditorWindowTests`), más contrato/estado/índice de I-24 y un hallazgo en
  `docs/ideas-futuras.md` (laguna de cobertura pura `ApplyScope` Level/Front en `DynamicFrontMatrixTests`).

**Baseline integrada de I-20 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño (AutoCAD): `0f430879cdc8f2a369406836db9d8661b8103e3b`
  (run `29888005513`, **cuatro jobs verdes**); tras la aprobación se añadieron una **corrección de comentario
  obsoleto** (doc-comment **sin cambio de comportamiento**) y el **cierre documental**, y la rama se
  **rebasó sobre `main` con I-21** (siguiente viñeta); el **SHA final rebasado** recibe su propio CI verde antes
  del merge; como el comportamiento del selectivo no cambia, la validación en AutoCAD y la owner-validation **se
  conservan** (WORKFLOW §6); este documento **no inventa** el SHA del merge de `main` (vive en
  `git log --first-parent main`);
- `origin/main` **avanzó** de `bfda406` (Merge I-15) a **`2a30fef`** (Merge I-21) durante esta tanda de
  integración serializada: I-20 quedó **rebasada** sobre esa punta (merge-base = `origin/main`); la
  reconciliación fue **sólo de documentación compartida** (HANDOFF, ROADMAP e índice de iniciativas), pues I-21
  sólo toca el editor **dinámico** y su código es **disjunto** del selectivo (cero solapamiento de archivos de
  código/pruebas); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **913/913 verdes** (base con I-21 = 889; + **24 nuevas** de `SelectiveEditorStateTests`;
  sin regresión); suite `RackCad.UI.Tests`: **139/139 verdes** (135 de la base + **4** de
  `SelectiveEditorStateAdoptionTests`, caracterización **STA** que fija el dibujo resuelto `load→build`);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta pre-rebase `0f43087` (run `29888005513`, cuatro jobs) y re-verde sobre el
  **SHA final rebasado** antes del merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests
  (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`);
- objetivo entregado: `SelectiveEditorState` + `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`/
  `SelectiveApplyScope`/`SelectiveDesignInputs` en `RackCad.Application.Systems` (estado + operaciones puras del
  editor selectivo, hallazgos U1/U3); `RackSelectiveWindow` **observa** ese estado (propiedades de acceso) y
  **delega** las operaciones, conservando el pintado, el editor de celda, los eventos, la recomputación
  coalescida (shell I-15) y el resolve/preview ligado al catálogo; la **superficie pública que consume el
  Plugin no cambia**;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño sin
  observaciones (matriz, «Aplicar a:» por alcance, cambios de fondo doble/triple, previews frontal/lateral,
  «Insertar frontal», `RACKEDITAR` «Actualizar»/«Insertar lateral-planta» con **mismo GUID**, geometría/BOM sin
  diferencias, **I-11 preservado**, round-trip y reapertura desde biblioteca); **owner-validation aprobada**;
- invariantes preservados: **sin** cambios de geometría, BOM, GUID, inserción/actualización, persistencia
  (Xrecord/**I-11** intactos), catálogos, formatos ni **XAML visible** (`RackSelectiveWindow.xaml` **byte-idéntico**
  a `main`); el diff de I-20 **no toca** el editor **dinámico** (I-21), I-22, DrawServices, DTO/persistencia ni
  generados; **sin** dependencias NuGet nuevas; equivalencia `load→build` fijada por la caracterización STA
  (antes y después del refactor); asimetría de estilos de cota **no** tocada (fuera de alcance);
- alcance: `src/RackCad.Application/Systems/` (5 archivos nuevos: `SelectiveEditorState`, `SelectiveEditorCell`,
  `SelectiveEditorFondoMatrix`, `SelectiveApplyScope`, `SelectiveDesignInputs`),
  `src/RackCad.UI/RackSelectiveWindow.xaml.cs` (observa/delega; −256 líneas netas),
  `tests/RackCad.Tests/SelectiveEditorStateTests.cs` (+24) y
  `tests/RackCad.UI.Tests/SelectiveEditorStateAdoptionTests.cs` (+4 STA), más contrato/estado/índice de I-20.

**Baseline integrada de I-21 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `779ee0c4ea06f2a84bc2c5738979449ed25c269f` (run
  `29887985687`, **cuatro jobs verdes** sobre la punta publicada `2470de2`); los commits posteriores de la
  rama son **solo documentales** (registro de validación + este cierre) y **no cambian código**; este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base de I-21 (`bfda406`, Merge I-15): **sin rebase**; la rama se integra
  por `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **889/889 verdes** (842 de la base + **47 nuevos** de caracterización/equivalencia:
  `DynamicEditorCell`, `DynamicEditorSafety`, `DynamicFrontMatrix`, `DynamicEditorDesignAssembler`, incluida la
  resolución del diseño armado por el pipeline real); suite `RackCad.UI.Tests`: **135/135 verdes** (la adopción
  STA construye la ventana real y confirma que sigue tomando identidad/inserción de la sesión del shell);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución Debug: **0 errores**, únicamente las
  dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `2470de2` (run `29887985687`): los **cuatro** jobs —Tests
  (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en
  `success`;
- objetivo entregado: estado puro del editor dinámico en `src/RackCad.Application/Systems/`
  (`DynamicEditorCell`/`DynamicEditorFront`/`DynamicEditorValues`; `DynamicFrontMatrix` con la matriz
  frente×nivel, la selección y todas las mutaciones —alta/baja, ajuste, toggle, commit, `ApplyScope` vía
  `DynamicRackCellScopeResolver`, snapshot/rollback, refresco/restauración desde el sistema resuelto,
  `BuildFrontDesigns`—; `DynamicEditorSafety`; `DynamicAnnotationOptions`+`DynamicEditorDesignAssembler` con
  `MustRebuild`/`Snapshot`-`RestoreHeaderFondos`/`UpdateHeaderHeightInPlace`/`BuildDesign`); la ventana
  `RackDynamicSystemWindow` lo **consume** (mueren los tipos privados `DynamicFrontRow`/`DynamicCellRow`/
  `DynamicEditorValues` y los helpers movidos) y solo coordina controles/eventos/render/diálogo sobre el
  Editor Shell; code-behind de ~3,339 a ~2,838 líneas;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño (módulo
  dinámico a profundidad: matriz/selecciones/alcance, cabeceras calculadas y personalizadas,
  seguridad/IN-OUT/intermedios, previews y vistas vinculadas, geometría/BOM, biblioteca/legacy/round-trip,
  actualización en sitio con el mismo GUID); **owner-validation aprobada**;
- invariantes preservados: **sin** cambios de geometría, planes, recetas BOM, GUID, nombre, `Section`, edición
  multivista, persistencia (Xrecord/I-11 intactos), metadatos desconocidos, fallbacks legacy, cabeceras legacy
  ni cama integrada; XAML byte-idéntico; **cero** cambios en Domain, catálogos, `deploy/`, Plugin o `.csproj`;
  **sin** dependencias NuGet nuevas; dirección de dependencias intacta (el estado nuevo vive en Application);
  única remoción incidental: el método privado muerto `EnsureIntermediateBeamDepthCount`;
- alcance: producto en `src/RackCad.Application/Systems/` (7 archivos nuevos) y
  `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` (adopción, −~500 líneas netas); 4 archivos de pruebas nuevos
  en `tests/RackCad.Tests/`; contrato `docs/initiatives/I-21-dynamic-editor-state.md`, estado
  `docs/automation/state/I-21.yml`, evidencia `docs/automation/evidence/I-21-autocad-validation.md` e índice.

**Baseline integrada de I-15 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `2bd5703ee2635019dc15caf3358c6fbdf4d83fa7` (run
  `29879550816`, **cuatro jobs verdes**); el commit posterior de la rama es **solo documental** (este cierre +
  estado versionado) y **no cambia código**, recibiendo su propio CI antes del merge; este documento **no
  inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` en `646614d` (Merge I-19 sobre I-12) al momento del gate: I-15 quedó **linealmente rebasada**
  sobre él (merge-base = `origin/main`); **rebase único** ya aplicado (base previa `abc1a53`, Merge I-14), **sin**
  otro rebase; la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **842/842 verdes** (sin regresión: I-15 no toca Domain/Application; las 842 vienen de la
  base rebasada con I-19); suite `RackCad.UI.Tests`: **135/135 verdes** (85 de I-14 + 45 unitarias del shell + 5
  de **adopción STA** que construyen las ventanas reales);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `2bd5703` (run `29879550816`): los **cuatro** jobs —Tests
  (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en
  `success`;
- objetivo entregado: `RackEditorSession<TDesign,TSystem>` (catálogo + `RackEditorIdentity` + `RecomputeGate`/
  `RecomputeDebouncer` + contrato de inserción/actualización), `RackInsertionRequest` por `Kind`, e
  `IRackEditorModule`+`EditorModuleRegistry` **explícito sin reflexión**; el **menú** y la **biblioteca** consumen
  el registro (mata ~19 props O(N) + 5 handlers de `RackMainMenuWindow`); el Plugin (`RackMenuCommands.RackCad`)
  despacha el `RackInsertionRequest` por `Kind` a los mismos `Draw*`; las **cuatro ventanas ricas** (selectivo,
  dinámico, cama, cabecera) **adoptan** el shell (props públicas = getters sobre la sesión), verificado por
  `EditorShellAdoptionTests`; larguero no adopta;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño (menú,
  biblioteca, `RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA`, `RACKEDITAR` round-trip con mismo GUID, geometría/BOM sin
  diferencias, I-11 preservado, previews fluidos, larguero sin inserción); **owner-validation aprobada**;
- invariantes preservados: **sin** cambios de geometría, BOM, GUID, edición multivista, persistencia (Xrecord/
  I-11 intactos), formatos ni UI; `RackMainMenuWindow.xaml` **byte-idéntico** a `main`; **cero** archivos de
  Domain/Application/catálogos/DrawServices; estado interno de selectivo/dinámico **reservado a I-20/I-21**; **sin**
  dependencias NuGet nuevas; reconciliación del rebase: conflicto **manual único** en `docs/initiatives/README.md`
  (preservando I-14/I-19), auto-merge verificado en los dos `.csproj` (LangVersion/Nullable de I-12 heredados +
  `InternalsVisibleTo`/copia de catálogos de I-15) y `docs/ideas-futuras.md`; **sin** código funcional de I-19 en UI;
- alcance: `src/RackCad.UI/Editor/` (11 archivos nuevos), las cuatro ventanas adoptadas +
  `RackMainMenuWindow.xaml.cs`, `src/RackCad.Plugin/RackMenuCommands.cs`, `RackCad.UI.csproj` (`InternalsVisibleTo`)
  y `RackCad.UI.Tests.csproj` (copia de catálogos), seis clases de pruebas del shell + `EditorShellAdoptionTests`, y
  contrato/estado/índice/ideas-futuras de I-15.

**Baseline integrada de I-19 — 2026-07-21:**

- punta validada por CI: `fcdc287` (run `29876393665`, **cuatro jobs verdes**); el commit documental de cierre
  posterior **no cambia código**; este documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` en `e2057d7` (Merge I-12) al momento del gate: I-19 quedó **linealmente rebasada** sobre él
  (merge-base = `origin/main`); **rebase único** ya aplicado (base previa `de72287`), **sin** otro rebase;
- suite `RackCad.Tests`: **842/842 verdes** (51 nuevas de I-19; sin regresión sobre las 791 de la base); suite
  `RackCad.UI.Tests`: **85/85 verdes** (I-19 no toca UI);
- build `RackCad.Application` Debug: **0 errores y 0 advertencias**; build UI: **0 advertencias**; build Plugin sin
  AutoCAD: **0 errores**, únicamente las `MSB3277` conocidas;
- CI de rama verde sobre la punta rebasada `fcdc287` (run `29876393665`): los **cuatro** jobs —Tests (Domain+Application),
  Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`;
- objetivo entregado: validador puro con severidades (5 categorías) + manifiesto esperado de `blocks-library.dwg`
  (bloques + parámetros dinámicos reales + huella + comparación versión/huella) + modo estricto; guardia por
  **igualdad exacta** builder→manifiesto por `PieceId+View+BlockName` (13 familias) con matriz de cobertura bidireccional;
- diagnóstico del catálogo distribuido (**baseline aprobado por el dueño**): 1 error `DUPLICATE_ID` (`TROQUEL_TOPE`,
  pre-existente, **no** corregido) + 2 advertencias `UNRESOLVED_BLOCK_PIECE` (`TARIMA_GENERICA`); huella esperada
  `1a31c1a91f00a27130b5d8778eacc174adec1e818e78722e814174685e30df40` (90 bloques), fijada por `ShippedCatalogIntegrityTests`;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`); **owner-validation aprobada**
  (baseline aceptado + catálogos/DWG intactos confirmados);
- invariantes preservados: **sin** cambios de catálogos (`git diff` vacío en `assets/catalogs/*`), **ningún** `.dwg`
  (el código nunca abre el DWG), **sin** cambios de geometría, BOM, persistencia ni reglas de producto; I-12, I-14 y su
  proyecto/gate `RackCad.UI.Tests` preservados; reconciliación: **único** conflicto en `docs/initiatives/README.md`
  (I-14 vs I-19), resuelto preservando ambas entradas;
- alcance: `src/RackCad.Application/Catalogs/` (`SeccionRoles`, `CatalogBlockParameters`, `Validation/*`, costura de
  `JsonRackCatalogProvider`), consolidación de nombres de parámetro en el dominio (`SelectiveRackDefaults`/
  `SelectiveSafetyDefaults`, `SelectiveSafetyPlacement`, `LateralHeaderParameters`, `DynamicSystemLateralBuilder`,
  `FlowBedLateralBuilder`), cinco clases de pruebas nuevas y contrato/estado/evidencia de I-19.

**Baseline integrada de I-12 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `5d5f0dc650bad5aa9ef24b5a49d1d47a58acebd7`; el commit posterior de
  la rama (`5e62a42`) es **solo documental** (registro de la validación AutoCAD); este documento **no inventa** el SHA
  del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base rebaseada de I-12 (`abc1a53`, Merge I-14): **sin rebase final** en esta
  sesión; la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **791/791 verdes** (sin regresión: I-12 no toca Domain/Application); suite `RackCad.UI.Tests`:
  **85/85 verdes** (I-12 solo elimina el `LangVersion`/`Nullable` duplicado del `.csproj`, ahora heredado);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución completa Debug: **0 errores**, únicamente las
  dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta `5e62a42` (run `29874100238`): los **cuatro** jobs —Tests (Domain+Application), Build
  UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`; la guarda de ADR-0003 publica
  el Plugin y ejecuta `verify-bundle.ps1` (fail-closed) + el harness del verificador;
- objetivo entregado: **versión única** (`RackCadVersion`) en `Directory.Build.props` que alimenta ensamblados y
  manifiesto; **SHA estampado** reproducible en `InformationalVersion` (fallback definido sin git); `PackageContents.xml`
  **generado** desde plantilla; bundle por **`dotnet publish`** con `deploy/build-bundle.ps1` + `deploy/verify-bundle.ps1`
  fail-closed (DLL≡publish, catálogos≡`assets/catalogs`, versión/series, **cero DLL Autodesk**) y su harness
  `deploy/test-verify-bundle.ps1`; `install-bundle.ps1` usa el flujo verificado y **rechaza** `-Build`+`-SourceBundlePath`;
  **ADR-0004** (una serie a la vez, `SeriesMin = SeriesMax = R25.0`, solo AutoCAD 2025) **aceptado por el dueño**;
- reproducibilidad: dos `dotnet publish` del mismo commit → **inventario y hashes idénticos** (bundle determinista);
- validación manual: el dueño **aprobó la autocarga del bundle en AutoCAD 2025** (instalación en
  `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle`, autocarga sin `NETLOAD` **PASS**, `RACKCAD` **PASS**);
  evidencia en `docs/initiatives/I-12-autocad-validation.md`;
- invariantes preservados: **sin** cambios de producto, UI, catálogos, persistencia, handlers, geometría, BOM ni dibujo;
  ADR-0003 intacto (referencias Autodesk compile-only, cero DLL Autodesk en output/bundle/artifacts); **sin** dependencias
  NuGet nuevas; **sin tocar** `RackCad.sln` ni `.github/workflows/ci.yml`;
- alcance: `Directory.Build.props`/`Directory.Build.targets`, los cinco `.csproj` + `RackCad.UI.Tests.csproj` (rebase),
  `src/RackCad.Plugin/RackCad.Plugin.csproj` (target), `deploy/` (`build-bundle`, `verify-bundle`, `test-verify-bundle`,
  `install-bundle`, `test-install-bundle`, plantilla `PackageContents`, borrado el `.xml` estático),
  `eng/ci/verify-autocad-references.ps1`, `docs/guias/despliegue.md`, ADR-0004 + índice, y contrato/estado/evidencia de I-12.

**Baseline integrada de I-14 — 2026-07-21:**

- punta de **código** validada por CI: `cf8ee1faf7cc71849699a39024e4f709ee5b1cd3` (commit único de corrección de la
  ronda 2 de revisión); el commit posterior de la rama es **solo documental** (estado versionado + este cierre); este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base de I-14 (`de72287`, Merge I-11): **sin rebase final**; la rama se integra
  por `git merge --no-ff` en esta sesión;
- suite `RackCad.UI.Tests`: **85/85 verdes** (lógica pura de los controles + instanciación STA de las vistas), sin
  fallos ni omitidas; suite `RackCad.Tests`: **791/791 verdes** (sin regresión: I-14 no toca Domain/Application);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución completa Debug: **0 errores**, únicamente
  las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `d8ed898` (run `29867946030`): los **cuatro** jobs —Tests
  (Domain+Application), Build UI, **UI Tests (WPF controls, net8.0-windows)** (nuevo) y Build Plugin without AutoCAD—
  en `success`; el commit de cierre documental no altera código y recibe su propio CI antes del merge;
- objetivo entregado: cinco controles WPF reutilizables en `src/RackCad.UI/Controls/` con lógica pura separada de la
  vista (`SelectionMatrix`+`SelectionMatrixModel`; `NumericField`+`NumericFieldValidation`; `CatalogCombo`+
  `CatalogComboSelection`; `PreviewCanvas`+`PreviewProjection`+`PreviewPalette`; base `RackDialogWindow`); el proyecto
  `tests/RackCad.UI.Tests` (`net8.0-windows`, runner STA propio **sin dependencias nuevas**) y su gate de CI `ui-tests`
  en `windows-latest`;
- **sin migración de ventanas** (patrón strangler): ninguna ventana existente cambia de comportamiento ni de
  apariencia; la adopción de los controles la harán I-15/I-20/I-21/I-22;
- invariantes preservados: **sin** cambios de geometría, recetas BOM, GUID, persistencia ni dibujo; **sin** tocar
  Domain, Application ni el Plugin (que sigue compilando: UI lo referencia transitivamente); **sin** dependencias
  NuGet nuevas; dirección de dependencias intacta (UI no referencia AutoCAD; los controles no dependen del Plugin);
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`; ROADMAP no marca I-14 con ✋);
  owner-validation **no requerida** (`requires_owner_validation: false`);
- alcance: producto en `src/RackCad.UI/Controls/` (10 archivos nuevos), pruebas en `tests/RackCad.UI.Tests/` (proyecto
  nuevo); modificados `RackCad.sln` (alta del proyecto), `.github/workflows/ci.yml` (job `ui-tests`, **sin tocar** el
  del Plugin) y `docs/initiatives/README.md`; contrato `docs/initiatives/I-14-ui-controls.md` y estado
  `docs/automation/state/I-14.yml`; sin cambios en Domain, Application, Plugin, catálogos ni deploy.

**Baseline integrada de I-11 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `eea1c1113dd8a33e33fa31dd61720c24c844ad4f`; los commits
  posteriores de la rama son **solo documentales** (no alteran código); este documento **no inventa** el SHA
  del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` no avanzó desde la base rebaseada de I-11 (`6e18874`, que ya incluye el fix posterior a I-10):
  sin rebase final adicional; la rama se integra por `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **791/791 verdes**, sin fallos ni omitidas (sobre `eea1c11`; incluye los 7 archivos
  de pruebas nuevos de persistencia y la caracterización existente);
- build UI Debug: **0 errores y 0 advertencias**; build solución completa Debug: **0 errores**, únicamente las
  dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `eea1c11` (los tres jobs: Tests, Build UI, **Build Plugin without
  AutoCAD**, en `success`); el commit de cierre documental no altera código;
- objetivo entregado: `FlowBedDocument`/`LargueroDocument` versionados + preservación de campos JSON
  desconocidos y de una versión de esquema **no degradada** en los cuatro límites (`RackEmbedDocument`,
  `RackProjectDocument` —incluido el diseño interior de los embeds dinámico/cabecera y los wrappers de
  biblioteca—, `FlowBedDocument`, `LargueroDocument`); `SchemaVersionPolicy` central; `RackEmbedComposer`
  puro; preflight discriminado (`ResolveInnerSource`/`PreflightInnerSources`:
  Success/BenignFallback/IncompatibleMajor/WrongKind) que **aborta la edición completa** ante un MAJOR interior
  incompatible o un `Kind` incorrecto (sin actualización parcial); transporte biblioteca↔DWG por sidecars de
  salida vía `RackMenuCommands` (transporte mínimo);
- invariantes preservados: geometría, recetas BOM, GUID y el **formato físico del Xrecord** (clave
  `RACKCAD_SELECTIVE`, chunk 255, `DxfCode.Text`) **intactos**; sin tocar `RackEnvelopeRestamp` ni el despacho
  por `Kind` de I-10; sin cambios en Draw Services, `RackBlockData` ni Domain; sin dependencias nuevas;
- validación en **AutoCAD 2025 aprobada** por el dueño (matriz §10 del contrato por `NETLOAD`, código
  `eea1c11`, incluidos B5, B6 y S7; `automation/evidence/I-11-autocad-validation.md`); **owner-validation
  aprobada**; gate `owner-decision` resuelto (`automation/decisions/I-11.md`);
- exclusión aprobada: `RackFrameProjectDocument` (biblioteca de cabecera desnuda por `RackFrameProjectStore`,
  quinto DTO) queda fuera de alcance por decisión del dueño; su preservación de desconocidos es deuda posterior;
- alcance: producto en `src/RackCad.Application/Persistence` (12 archivos), `src/RackCad.Plugin` (6, con
  `RackMenuCommands` solo como transporte) y `src/RackCad.UI` (5); 7 archivos de pruebas nuevos y el contrato
  de I-11; sin cambios en Domain, catálogos, deploy ni `.csproj`; sin dependencias nuevas.

**Corrección posterior a I-10 (`fix/kind-handler-missing-errors`) — 2026-07-21:**

- punta técnica revisada de la rama: `5fc631a9830024ce1535fe93a5322820d7e96dab`; este documento **no inventa**
  el SHA del merge de `main` (vive en `git log --first-parent main`);
- **corrección posterior**, no una reimplementación: I-10 permanece históricamente integrada en
  `c9f2d61ee14a1afe85d3d941080405371187670e`;
- `origin/main` no avanzó desde la base de la rama (`c9f2d61`): sin rebase final; se integra por `git merge --no-ff`;
- hallazgos corregidos: BOM parcial ante handler ausente (ahora preflight + abort de todo el comando), layout
  enlazado sin gate (ahora gate incondicional antes de abrir la ventana), restamp silencioso (ahora lanza),
  inmutabilidad del registro (ahora `ReadOnlyCollection` vía la `KindDispatch<T>` pura de Application), y
  cobertura de rutas negativas (`KindDispatch.TryResolveAll` puro + source-guards, sin cargar Autodesk — ADR-0003);
- suite `RackCad.Tests`: **718/718 verdes**, sin fallos ni omitidas; build UI Debug **0 errores y 0 advertencias**;
  builds Plugin y solución Debug **0 errores** (solo las dos familias `MSB3277` conocidas);
- CI de rama verde sobre `5fc631a` (run `29849342527`): los tres jobs (Tests, Build UI, **Build Plugin without
  AutoCAD**) en `success`;
- **validación manual en AutoCAD aprobada por el dueño** sobre el DLL Debug del worktree correspondiente a
  `5fc631a` (SHA-256 `0B39BDE316B9D861C19286C0911A2226433F4ED94CD0EFAD65607CBC9975FFE3`): confirmó que funcionó
  bien; la validación se conserva porque `origin/main` no avanzó (WORKFLOW §6);
- equivalencia mecánica: **26 `[CommandMethod]`** idénticos a `origin/main`, cero duplicados; **5** `Guid.NewGuid`;
  **7** ubicaciones de `Regen`; sin cambios en geometría, recetas BOM, formatos persistidos, catálogos, Draw
  Services ni referencias AutoCAD fuera del Plugin; sin dependencias nuevas;
- alcance: producto en `src/RackCad.Plugin` (`KindHandlers/` + los consumidores migrados) y
  `src/RackCad.Application/Persistence/KindDispatch.cs`; tests en `tests/RackCad.Tests`; sin cambios en
  Domain/UI/catálogos/deploy/csproj.

**Baseline integrada de I-10 — 2026-07-21:**

- punta de implementación revisada de `architecture/kind-handlers`:
  `532eb038306a3de68277496ce47457f270200944`; este documento **no inventa** el SHA del merge de `main` (vive
  en `git log --first-parent main`);
- `origin/main` no avanzó desde la base de I-10 (`c5a4082`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **694/694 verdes**, sin fallos ni omitidas (línea base y post-refactor idénticas;
  Domain+Application no se tocan);
- build UI Debug: **0 errores y 0 advertencias**; build solución completa Debug: **0 errores**, únicamente
  las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `532eb03` (run `29836270208`, `headSha 532eb038…0944`): los tres jobs (Tests,
  Build UI, **Build Plugin without AutoCAD**) en `success`;
- objetivo entregado: `IRackKindHandler` + `KindHandlerRegistry` en `src/RackCad.Plugin/KindHandlers/`
  (registro explícito, inmutable, sin reflexión, que rechaza handlers nulos, claves vacías y duplicadas;
  `TryGet` Ordinal + `TryGetIgnoreCase`) con los cuatro handlers embebidos (`selective`, `dynamic`,
  `cabecera`, `cama`) y `Default`; RACKEDITAR, RACKBOMTOTAL (`BuildRackBom` + `KindLabel`) y el restamp
  despachan por el registro; **0** `switch`/cadena por el `Kind` del sobre restante en alcance; `Larguero` no
  registrado;
- equivalencia mecánica: **26 `[CommandMethod]`** idénticos a `origin/main`, cero duplicados; **5**
  `Guid.NewGuid` (una en el restamp); **7** ubicaciones de `Regen`; mensaje "tipo de rack no reconocido" y
  etiquetas de BOM (Selectivo/Dinámico/Cabecera/Cama) **verbatim**; constantes de Kind/View intactas;
- frontera de registros: `SystemRegistry` (Application, I-08, `RackSystemKind`) y `KindHandlerRegistry`
  (Plugin, string del sobre) **no** se unifican; `RackListBuilder` (Application, RACKLISTA) queda intacto por
  la dirección de dependencias (Application no depende del Plugin);
- alcance: producto solo en `src/RackCad.Plugin` (6 archivos nuevos en `KindHandlers/` + 3 consumidores:
  `RackMenuCommands`, `RackInventarioCommands.BomTotal`, `RackEnvelopeRestamp`); documentación el contrato de
  I-10 y su índice; sin cambios en Domain/Application/UI/catálogos/deploy/csproj; sin dependencias nuevas;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`; ROADMAP no marca I-10
  con ✋); owner-validation no requerida (`requires_owner_validation: false`), por analogía directa con I-09;
  no se declara AutoCAD validado.

**Baseline integrada de I-08 — 2026-07-21:**

- punta de implementación revisada de `architecture/system-registry`:
  `997fb8e459af11f0d42ac0eb13029cb8c4b287d3`; este documento no inventa el SHA futuro del merge de `main`;
- `origin/main` no avanzó desde la base de I-08 (`0849152`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **686/686 verdes**, sin fallos ni omitidas; incluye la caracterización golden
  F1–F1.1 (wire format PascalCase, schema `2.0`, cinco nombres de enum, nulos, legacy sin `Kind`,
  reconstrucción física, `kind` sin payload, degenerado, versión futura, string desconocido, `Kind: 999`,
  reglas laxas de cama/larguero, etiquetas/precedencia/orden/tolerancia) verde tras el refactor;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama verde sobre `997fb8e`: los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) en
  success;
- objetivo entregado: `SystemDescriptor` + `SystemRegistry` en Application como fuente única de los cinco
  `RackSystemKind`; `RackProjectStore`, la validación genérica y `RackDesignLibrary` despachan por el
  registro; **`RackDesignKind` y `MapKind` eliminados por completo** (búsqueda global de `RackDesignKind`
  en `.cs`/`.xaml` = cero); sin `switch`/cadena por `RackSystemKind` en el store ni en la biblioteca; sin
  un segundo registro manual de los cinco sistemas;
- compatibilidad preservada: formato JSON, schema, nombres persistidos del enum, fallback legacy y
  etiquetas visibles idénticos; `RackSystemKind` intacto (sin renombrar ni reordenar);
- owner-validation **aprobada** por el dueño (checklist de biblioteca, cinco etiquetas, editores y
  cabecera legacy); AutoCAD no ejecutado ni requerido (`requires_autocad: false`); no se declara
  validación formal de geometría;
- alcance: producto en `RackProjectStore.cs`, `RackDesignLibrary.cs`, `RackMainMenuWindow.xaml.cs` y los
  tipos nuevos del registro (`SystemDescriptor`, `SystemRegistry`, `SystemRegistry.Default`); tests; el
  contrato de I-08 y su índice; sin cambios en `src/RackCad.Plugin`, `RackEmbedDocument`,
  `RackListBuilder`, DrawServices, DTOs, geometría, BOM ni catálogos; sin dependencias nuevas. **I-10 e
  I-16 fuera de alcance.**

**Baseline integrada de I-16 — 2026-07-21:**

- punta de rama integrada `f3a84bc44faf498c94dcd26b0d469f33e49a697a` (rebaseada sobre `origin/main` `549870b`
  tras integrarse I-08); integrada en `main` por **merge `--no-ff` `2c3bee734511740ab8636894c29a74687ab1cafd`**
  (primer padre `549870b`, segundo padre `f3a84bc`);
- suite `RackCad.Tests`: **694/694 verdes** sobre el árbol combinado (686 de `main`/I-08 + 8 golden de I-16),
  sin fallos ni omitidas (build local con el SDK de usuario);
- build UI Debug: **0 errores y 0 advertencias**; build Plugin Debug: **0 errores**, únicamente las dos
  familias `MSB3277` conocidas;
- CI de rama verde en los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) sobre la punta `f3a84bc`;
- validación manual en **AutoCAD 2025 aprobada** por el dueño sobre el DLL Debug del worktree I-16 (build de
  la punta F4, SHA-256 `6AEF0F4D5A49B89F6F5AAA35D4E287715473641E81D379B4BC671B55CC52906B`), matriz por
  familia (selectivo, dinámico con `postIndex`/entrada-salida, cama, cabecera, cancelación del jig,
  persistencia) sin observaciones (registro en `initiatives/I-16-autocad-validation.md`); el avance por I-08
  no toca la superficie de dibujo, así que la validación se conserva (WORKFLOW §6);
- equivalencia mecánica: las siete fachadas `*DrawService` conservan firmas públicas; infraestructura
  compartida extraída (`RackCatalogLoader`, `BlockPlacement`, `ViewBlockDraw`, `SystemBlockWriter.ApplyRegen`);
  invariantes preservados (nombres de bloque y sufijos, mensajes, `postIndex`, `DynamicRackEnd`, all-loose,
  payload/GUID, BOM, geometría, persistencia y **7 ubicaciones efectivas de `Regen`**);
- alcance: producto solo en `src/RackCad.Plugin`; golden solo en `tests/RackCad.Tests`; documentación el
  contrato, la línea base y el registro de validación de I-16; sin cambios en Domain/UI/catálogos/deploy;
- diff del merge contra su primer padre (`549870b`): únicamente el alcance acumulado de I-16 (19 archivos);
  **I-08 permanece intacta** (`RackDesignKind` eliminado, `SystemRegistry` presente).

**Baseline integrada de I-09 — 2026-07-20:**

- punta de implementación revisada de `refactor/plugin-commands`:
  `09de768cc7dfabdd29e313b4d8798abd783ec4a9`; este documento no inventa el SHA futuro del merge de
  `main`;
- `origin/main` no avanzó desde la base de I-09 (`6136fcb`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama verde sobre `09de768`: los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) en
  success;
- equivalencia mecánica verificada: **26 `[CommandMethod]`** con nombres idénticos a `origin/main`,
  cero duplicados, **13 principales + 13 aliases** con los mismos destinos; conjuntos idénticos de
  literales de código (prompts, keywords, mensajes, `SetRejectMessage`, nombres de bloque), `case`
  por `Kind`, DrawServices, stores, colores ACI, `directOnly`/`forceValidity`; conteos iguales de
  `Regen` (7), `Guid.NewGuid` (5), purgas (6) y `catch`/`Report` (18); el tipo y los archivos
  `RackFrameCommands` quedaron eliminados;
- alcance: producto solo en `src/RackCad.Plugin`; documentación solo el contrato de I-09 y su índice;
  sin cambios en Domain/Application/UI/tests/catálogos/deploy; sin dependencias nuevas; sin
  `[assembly: CommandClass]`;
- AutoCAD: no ejecutado; no requerido por contrato al conservar comportamiento mediante equivalencia
  mecánica, builds y CI (ROADMAP no marca I-09 con ✋).

**Baseline integrada de I-13 — 2026-07-20:**

- punta técnica final `849dff931ac5055c955ea2371c2388ec279b74b4`, contenida en `main` por
  `773feea3732497e04746c45451eb1b4e775d8961`;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama #64: tests, build UI y build Plugin without AutoCAD verdes; único artifact
  `rackcad-coverage-cobertura`, sin artifacts del Plugin ni material Autodesk;
- la validación post-merge #63 detectó que el job conservaba la condición temporal de la rama de
  promoción; I-13 la retiró antes de la limpieza para que el Plugin se compile en cada push;
- CI post-merge #65 verde sobre `773feea3732497e04746c45451eb1b4e775d8961`: ejecutó los tres
  jobs, incluido Build Plugin without AutoCAD, y publicó solo la cobertura Cobertura;
- las tres anotaciones de CI son la deprecación heredada de Node.js 20 en las acciones usadas;
- ADR-0003 aceptado con decisión I-29 B, matriz 14/14, rollback y nueva revisión obligatoria;
- AutoCAD: no ejecutado ni requerido porque la iniciativa cambia infraestructura de compilación y
  documentación, no dibujo ni runtime;
- evidencia experimental y respaldo pre-rebase conservados en las etiquetas
  `archive/i-13-experiment-final-4e084d2` y `archive/i-13-pre-rebase-a6febd2` antes de retirar las
  ramas y worktrees de I-13.

**Baseline integrada de I-26 — 2026-07-19:**

- punta de implementación validada de `refactor/test-catalog-ids`:
  `2cf3f12684dbe495403f0a16eeaa882e4873e3c6`;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- guardián de catálogos canónicos: verde contra IDs, bloques/vistas, conexiones, relaciones
  esenciales, defaults, plantillas y constantes equivalentes de producto;
- build UI Debug: **0 errores y 0 advertencias**;
- cobertura local observada: **91.77 % de líneas** y **75.26 % de ramas** en `RackCad.Domain` y
  `RackCad.Application`; es evidencia, no un umbral contractual;
- CI de rama #40: verde sobre la punta validada, según confirmación del dueño; el artifact
  `rackcad-coverage-cobertura` fue descargado y contiene `coverage.cobertura.xml`;
- diff bajo `src/`, `assets/catalogs/` y `deploy/`: vacío; no hubo cambios de producto ni datos;
- AutoCAD: no ejecutado ni requerido para esta iniciativa de infraestructura de pruebas;
- el commit documental final de integración requiere su propio CI antes del merge y no se declara
  verde anticipadamente.

**Baseline documental de I-06 que lleva este merge — 2026-07-17:**

- punta validada de `docs/reestructura`: `39cd54189457e8737f08cf95dbf948bc2e564dd3`;
- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente los `MSB3277` conocidos;
- `git diff origin/main --check`: limpio;
- documentación Markdown: **52 documentos**, **123 enlaces locales** y **0 enlaces rotos**;
- Context Packs: nueve IDs únicos, con rutas, globs, gates y exclusiones válidos;
- diff bajo `src/`: solo el comentario XML autorizado en `RackCommandReference.cs`;
- CI de rama: verde para `39cd54189457e8737f08cf95dbf948bc2e564dd3`, según la confirmación del
  dueño; la corrección administrativa posterior requiere repetir CI antes del nuevo merge;
- AutoCAD: no ejecutado ni requerido para esta iniciativa documental.

La baseline integrada anterior correspondía a I-04 (`8e52828` como punta de integración):

- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug y Release: **0 errores**, únicamente las familias `MSB3277` conocidas;
- harness del instalador: **25/25 verificaciones** en rutas temporales;
- CI de I-04: Success sobre `f82a49f`.

La evidencia técnica de la rama I-06 se conserva bajo [automation/runs/](automation/runs/). Este
documento no inventa el SHA futuro del merge de `main`.

## 6. Preguntas abiertas

1. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global según el
   uso real?

## 7. Decisiones vigentes pendientes de I-07

> **Conservación temporal obligatoria:** estas decisiones proceden de la antigua sección 7 y aún
> gobiernan el proyecto. Solo podrán retirarse de HANDOFF cuando I-07 integre los ADRs retroactivos
> correspondientes. I-06 no las convierte en ADR ni reabre su contenido.

| Decisión | Motivo / alcance vigente |
|---|---|
| Solo `RackCad.Plugin` toca AutoCAD | Geometría y BOM permanecen puros y testeables en Application. |
| Catálogos CSV Excel-first, sin base de datos | El usuario los edita en Excel; se conserva fallback Windows-1252 y caché por firma. |
| Un solo `secciones.csv` con columna `rol` | Postes, celosía, largueros y separadores comparten una hoja y FKs explícitas. |
| Identidad por GUID embebido en DWG | El nombre visible no es una identidad estable. |
| `Actualizar` redibuja; `Insertar` agrega una vista ligada | Convención permanente de los cuatro editores. |
| Parámetros dinámicos mediante patrón ARRAY | Evita fijar parámetros repetidamente por referencia. |
| Cero dependencias NuGet en producto, salvo ADR-0003 | La única excepción es `AutoCAD.NET [25.0.1]` condicional y compile-only en `RackCad.Plugin`, con versiones transitivas fijadas, sin runtime ni distribución y sujeta a nueva revisión. |
| Parrilla: una por tarima y regla en `SelectiveFrontalBuilder.ParrillaRow` | Dibujo, BOM y UI concuerdan por construcción. |
| Copia de `SelectiveSafetySelection` centralizada en `DeepCopy` | El DTO sigue explícito para compatibilidad y round-trip. |
| Entrada numérica localizada sin agrupadores | Acepta punto o coma decimal sin transformar valores ambiguos. |
| Cantidad de parrilla: UI rechaza si no cabe y builder acota | Evita dibujo fuera del marco y degrada de forma segura tras cambios. |
| Validación de cargas diferida a RAM Elements | No debe re-proponerse dentro del alcance actual. |
| Optimizador IA de layout diferido | `RACKLAYOUT` es el motor determinista vigente, no el optimizador futuro. |
