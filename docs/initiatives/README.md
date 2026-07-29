# Contratos de iniciativas

`docs/ROADMAP.md` es el indice global de fases, dependencias, conflictos y estado integrado. No se
usa para guardar el estado transitorio "en curso": ese estado se deriva de Git y se registra para el
ejecutor en `docs/automation/state/<initiative>.yml`.

`docs/AUTOMATION_PLAN.md` gobierna la seleccion automatica, los limites de concurrencia, el reclamo
atomico, los reintentos y las condiciones de detencion. Cada archivo de esta carpeta es el contrato
detallado de una iniciativa y define su alcance, contexto, validaciones y entrega.

`docs/HANDOFF.md` conserva unicamente el estado vivo del proyecto. Los contratos no copian el
historial general, conteos de pruebas ni hashes: enlazan a las fuentes correspondientes.

Reglas de ejecucion:

- una ejecucion reanuda primero una iniciativa activa sin gate y ejecuta como maximo una fase
  coherente o una correccion de fallo;
- solo si no existe trabajo activo reanudable puede tomar como maximo una iniciativa nueva;
- una rama remota aceptada reclama la iniciativa;
- una iniciativa activa reutiliza su rama, worktree limpio y Pull Request draft; nunca duplica
  ninguno de ellos;
- `docs/automation/state/<initiative>.yml` es el estado transitorio canonico legible por el ejecutor;
- el bloque `automation_state` del Pull Request es una copia opcional y no bloquea la publicacion si
  no puede actualizarse;
- las decisiones del dueno pueden versionarse en `docs/automation/decisions/<initiative>.md`;
- el ejecutor no depende de GitHub CLI: Git basta para commit, push y estado versionado;
- el agente nunca hace merge ni activa auto-merge;
- las validaciones en AutoCAD son responsabilidad del dueno y bloquean la integracion, no
  necesariamente otra iniciativa compatible;
- Git y los resultados verificables prevalecen sobre el estado versionado, la copia del Pull Request
  y el campo `status` del front matter;
- el archivo de estado se actualiza al terminar cada ejecucion; `completed` no significa integrado;
- un contrato se crea desde `TEMPLATE.md` y no inventa alcance ausente del ROADMAP.

Durante el bootstrap, el plan y los contratos se leen desde `origin/docs/reestructura` y solo puede
reanudarse I-06. Despues de integrar el sistema documental, las reglas globales se leen desde la
punta de `origin/main`.

Planes disponibles:

- `I-06-reestructura-context-packs.md`: contrato del piloto documental I-06. Bootstrap, reclamo,
  auditoria y decision de taxonomia ya terminaron; el estado actual vive en
  `../automation/state/I-06.yml`.
- [`I-07-adr-retroactivos.md`](I-07-adr-retroactivos.md): contrato de I-07 (Fase 1, solo documentación,
  sobre I-06 integrada). Retro-documenta como ADRs de una página las trece decisiones vigentes de
  HANDOFF §7: renumera los siete ADRs ya redactados a 0006–0012 tras el rebase (main ocupó 0003–0005),
  los revisa contra código/arquitectura vigentes (rutas de `RackFrameCommands` partidas por I-09,
  excepción NuGet condicional de ADR-0003) y añade 0013–0018 para las decisiones sin ADR, una por
  decisión. **Aceptados por el dueño el 2026-07-22** («Sí, apruebo»;
  [`../automation/decisions/I-07.md`](../automation/decisions/I-07.md)) → estado `aceptado`, conservando
  las limitaciones sobre fecha/decisores/evidencia. La matriz de cobertura HANDOFF §7 → ADR vive en el
  contrato (§11); el cierre de integración retiró las trece decisiones de HANDOFF §7 (ahora en
  `docs/adr/`). Fuera de alcance: el contenido normativo de ADR-0001…0005, la colisión histórica de
  numeración `0002` y cualquier código de producto; no absorbe I-18/I-23/I-25. **Integrada en `main` el
  2026-07-22.** Estado versionado en `../automation/state/I-07.yml`.
- [`I-26-test-catalog-ids.md`](I-26-test-catalog-ids.md): contrato manual para centralizar IDs
  canónicos de pruebas, verificar los catálogos distribuidos y publicar cobertura Cobertura en CI.
- [`I-03-fallos-silenciosos.md`](I-03-fallos-silenciosos.md): contrato de I-03 (Fase 1, sin
  dependencias; estorba con I-11, integrada). Hace diagnosticables los fallos silenciosos (P1/D2):
  logger mínimo a `%AppData%\RackCad\logs` en `RackCad.Application.Diagnostics` (best-effort, nunca
  lanza), `Report()` que registra el stack conservando el mensaje, los 14 `catch` silenciosos del
  Plugin y los stores best-effort de Persistence que registran, escritura atómica (temp +
  `File.Replace`) en los 4 stores (`RackProjectStore`/`RackFrameProjectStore`/`UserTemplateStore`/
  `UserSettingsStore`) y carga que distingue archivo ausente de ilegible (`.bad` + registro). Cambio
  **aditivo**: preserva I-11 (versiones, metadata, geometría, BOM, GUID), comandos, alias y mensajes
  visibles. Fuera de alcance: rediseño UI, cambios de schema, reglas de producto, catálogos, telemetría
  remota, I-17 y las lecturas tolerantes de embeds de I-11. La carga distingue **por excepción**
  (`FileNotFoundException`/`DirectoryNotFoundException` → ausente silencioso; `JsonException` → cuarentena `.bad` +
  log; cualquier otro fallo de lectura → log sin cuarentena), y `CorruptFile` registra el fallo secundario de
  cuarentena. `requires_autocad: false`, `requires_owner_validation: false`. **Integrada en `main` el 2026-07-22**
  (rebasada sobre `b60f142`, Merge I-17, reconciliando **sólo** documentación compartida; el código de I-03 e I-17
  es disjunto salvo `RackFrameProjectStore.cs`, aditivo por ambos lados y auto-fusionado).
- [`I-09-refactor-plugin-commands.md`](I-09-refactor-plugin-commands.md): contrato para partir
  `RackFrameCommands` por área, promover helpers de bloques/clonación/capas/transacciones y unificar
  el escaneo de envelopes triplicado, preservando comandos, geometría, BOM, persistencia y UX. Fuera
  de alcance I-10 e I-16 y cualquier cambio funcional.
- [`I-08-system-registry.md`](I-08-system-registry.md): contrato de I-08. Introduce el descriptor de
  sistema y `SystemRegistry` en Application; `RackProjectStore`, la validación y `RackDesignLibrary`
  consumen el registro (mueren los switches por `RackSystemKind` y el enum paralelo `RackDesignKind`),
  preservando formatos JSON, IDs, nombres, etiquetas, fallback legacy y APIs públicas. Limitada a
  Application/Persistence + adaptación mínima del consumidor de biblioteca en UI. Fuera de alcance
  I-10 (handlers del Plugin), I-16 (DrawServices), y `RackEmbedDocument` con sus discriminadores string.
- [`I-16-refactor-draw-services.md`](I-16-refactor-draw-services.md): contrato del refactor de DrawServices
  del Plugin — extraer la infraestructura compartida (`RackCatalogLoader`, `BlockPlacement`), colapsar la
  orquestación de las siete fachadas en `ViewBlockDraw` y uniformar `regen`, sin cambio de comportamiento.
  Con [línea base de equivalencia](I-16-draw-services-baseline.md) y
  [validación manual en AutoCAD aprobada](I-16-autocad-validation.md). Integrada en `main` el 2026-07-21.
- [`I-10-kind-handlers.md`](I-10-kind-handlers.md): contrato de I-10. Introduce `IRackKindHandler` y el
  registro explícito `KindHandlerRegistry` en `RackCad.Plugin` (cuatro Kinds embebidos —selective, dynamic,
  cabecera, cama—, sin reflexión; `Larguero` sin handler) y migra a él RACKEDITAR, RACKBOMTOTAL y el restamp
  de copias independientes, con error visible ante Kind sin handler y sin otro cambio de comportamiento.
  `SystemRegistry` (Application, I-08) y `RackListBuilder`/RACKLISTA quedan fuera de alcance por la dirección
  de dependencias. Cierra la pista B del Plugin. Integrada en `main` el 2026-07-21.
- [`I-14-ui-controls.md`](I-14-ui-controls.md): contrato de I-14 (pista C de UI). Crea cinco controles WPF
  reutilizables en `RackCad.UI` (`SelectionMatrix`, `NumericField`, `CatalogCombo`, `RackDialogWindow` y
  `PreviewCanvas` con proyección/paleta compartidas), separando lógica pura de la vista, más el proyecto
  `tests/RackCad.UI.Tests` (net8.0-windows, runner STA propio) y su gate de CI dedicado. Los controles
  nacen con pruebas y **no** migran ninguna ventana existente (patrón strangler): sin cambio de dibujo,
  BOM ni persistencia. La adopción la harán I-15/I-20/I-21/I-22. Fuera de alcance I-15 y el rediseño visual.
- [`I-15-editor-shell.md`](I-15-editor-shell.md): contrato de I-15 (pista C de UI, sobre I-08 e I-14).
  Introduce la infraestructura del Editor Shell en `RackCad.UI/Editor/` — `RackEditorSession` (catálogo,
  identidad/GUID, recomputación coalescida y contrato de inserción/actualización), `IRackEditorModule` y
  un `EditorModuleRegistry` explícito sin reflexión — y migra el menú principal y la biblioteca a
  consumir el registro en lugar de las ~19 propiedades de payload y los handlers por sistema (mata el
  crecimiento O(N) de `RackMainMenuWindow`, hallazgos E3/E5/U1). El único consumidor del payload en el
  Plugin (`RackMenuCommands.RackCad`) lee un `RackInsertionRequest` y despacha por Kind a los mismos
  `Draw*`. Las cuatro ventanas ricas (selectivo, dinámico, cama, cabecera) **adoptan** el shell para esas
  capacidades compartidas; lo único que queda para I-20/I-21 es extraer su estado interno propio (matriz
  por fondo, `BuildSystem`, `Recompose`). Sin cambio de dibujo, BOM, GUID, persistencia ni formatos.
  Fuera de alcance Push Back (I-18), Draw Services (I-16) y ampliar I-14. Rebasada sobre `main` vigente
  (`646614d`, tras I-12 e I-19); AutoCAD y owner-validation **aprobadas por el dueño**; integrada en
  `main` el 2026-07-21.
- [`I-19-validador-catalogos.md`](I-19-validador-catalogos.md): contrato de I-19. Añade un validador PURO en
  `RackCad.Application.Catalogs.Validation` con severidades para ids duplicados, referencias/relaciones inválidas,
  bloques/vistas faltantes y filas descartadas por rol (con aviso), más el manifiesto esperado de
  `blocks-library.dwg` (lista de bloques + parámetros + huella) y su comparación. Fuera de alcance: corrección
  automática de catálogos, tocar el DWG, Push Back, reglas de producto, logging de I-03, esquema de persistencia y
  el cableado UI/Plugin. Owner-validation aprobada (baseline `TROQUEL_TOPE` + `TARIMA_GENERICA` aceptado);
  rebasada sobre `main` vigente e integrada en `main` el 2026-07-21.
- [`I-20-selective-editor-state.md`](I-20-selective-editor-state.md): contrato de I-20 (Fase 5, sobre
  I-15). Extrae el estado propio del editor selectivo (`FondoMatrix`/`Cell`/`ApplyScope`/`BuildDesign`
  y las transiciones por fondo) a clases puras y testeables de `RackCad.Application`
  (`SelectiveEditorState` + `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`/`SelectiveApplyScope`/
  `SelectiveDesignInputs`), dejando `RackSelectiveWindow` observando el estado y pintando matriz/
  previews (hallazgos U1/U3). Preserva UI, geometría, BOM, GUID, inserción/actualización, persistencia
  y metadatos I-11, catálogos, compatibilidad legacy y round-trip. Fuera de alcance I-22 (colocación de
  seguridad; orden fijo I-20 primero), I-21/el editor dinámico y la asimetría vigente de estilos de
  cota. AutoCAD y owner-validation **aprobadas por el dueño**; integrada en `main` el 2026-07-21.
- [`I-21-dynamic-editor-state.md`](I-21-dynamic-editor-state.md): contrato de I-21 (Fase 5, sobre I-15 e
  I-02). Extrae de `RackDynamicSystemWindow` a `RackCad.Application` el estado puro del editor dinamico
  —`DynamicEditorCell`/`DynamicEditorFront`/`DynamicEditorValues`, la matriz frente x nivel con su
  seleccion (`DynamicFrontMatrix`), la seguridad (`DynamicEditorSafety`) y la recomputacion/construccion
  del diseno (`DynamicAnnotationOptions` + `DynamicEditorDesignAssembler`)— dejando la ventana como
  coordinadora de controles, eventos, render y dialogo sobre el Editor Shell. Con pruebas de
  caracterizacion/equivalencia (matriz, celdas, seguridad, recomputacion, construccion del diseno,
  casos invalidos, carga legacy). **Sin cambio** de geometria, planes, BOM, GUID, `Section`, edicion
  multivista, persistencia I-11, fallbacks legacy, cabeceras legacy ni cama integrada. Fuera de alcance
  Push Back (I-18), Dinamico V2 (I-28), el selectivo (I-20), reglas de producto, catalogos y bloques DWG.
  AutoCAD y owner-validation **aprobadas por el dueño**; integrada en `main` el 2026-07-21.
- [`I-05-guardrail-unidades.md`](I-05-guardrail-unidades.md): contrato de I-05 (Fase 1, sin dependencias
  ni estorbos). Añade una guardia visible y NO bloqueante en el límite de AutoCAD (`RackUnitsGuard` en el
  Plugin): al insertar un sistema o vista nueva, y en `RACKLAYOUT`/`RACKRELLENAR` (con alias), lee
  `INSUNITS` y avisa una sola vez si el dibujo no está en pulgadas, antes de la primera modificación del
  DWG; una actualización pura con `RACKEDITAR` no avisa. La decisión pura vive en
  `RackCad.Application.Drawing.DrawingUnitsAdvisory` (sin AutoCAD); el cableado se fija con source-guards.
  Documenta `ADR-0005` (estrategia de unidades, **aceptado**). Fuera de alcance: conversión, reescalado, la
  columna `units`, almacenar unidades en DTO y cualquier framework general de unidades. ADR-0005 aceptado;
  AutoCAD y owner-validation **aprobados por el dueño**; **integrada en `main` el 2026-07-22**.
- [`I-24-ui-tests-editores.md`](I-24-ui-tests-editores.md): contrato de I-24 (Fase 5, sobre I-15/I-20/
  I-21). Amplia `tests/RackCad.UI.Tests` con pruebas del `RackFrameConfiguratorViewModel` (headless), de
  la adopcion del estado dinamico por `RackDynamicSystemWindow` (caracterizacion `load->build` por punto
  fijo) y de la identidad round-trip de las ventanas selectiva y de cama (carga nueva vs. existente,
  insercion, actualizacion, `UpdateOnly`), mas rutas negativas deterministas. Todo el codigo de prueba
  vive en `RackCad.UI.Tests`; no duplica las pruebas puras de `RackCad.Tests`. Unico cambio de
  produccion: el seam interno `RackDynamicSystemWindow.BuildDesignForTest` (reenvia a `Recompose`, sin
  reglas nuevas). Las pruebas de insercion/actualizacion recorren los handlers WPF reales
  (`RaiseEvent(ButtonBase.ClickEvent)`), con correspondencia estricta del payload por firma completa del
  dibujo (incluidas anotaciones y cotas, con el `Name` normalizado). `requires_autocad: false`,
  `requires_owner_validation: false`. Rebasada sobre `main` vigente (`a50c4ec`, Merge I-05) reconciliando
  solo este indice; integrada en `main` el 2026-07-22.
- [`I-22-safety-placement.md`](I-22-safety-placement.md): contrato de I-22 (Fase 5, sobre I-14 e I-20).
  Cierra E6/E7 de la seguridad del selectivo: servicios PUROS de colocacion por familia (tope, parrilla,
  tarima, separador) parametrizados por vista con los builders como orquestadores; descompone la
  God-data-class `SelectiveSafetySelection` en configuraciones por subtipo con `DeepCopy` propio y mapeo
  de persistencia por familia (formato de alambre byte-identico, fallback legacy y round-trip); enruta
  los 5 paso-de-troquel hardcodeados a `SelectiveRackDefaults.TroquelPaso`; y adopta `SelectionMatrix`
  (con celdas ausentes para rejillas dentadas) en las rejillas tope/desviador/guia. **Sin cambio** de
  geometria, planes, BOM, GUID, persistencia I-11, catalogos, nombres de bloque, mensajes, seleccion,
  defaults ni interaccion. El frontal de tope conserva su naturaleza esquematica por frente
  (`SelectiveTopePlan.BuildFrontal`, resultado distinto de los spots fisicos). Fuera de alcance I-25
  (guardas traseras), Push Back (I-18), el editor Dinamico, rediseño visual y reglas de producto.
  AutoCAD y owner-validation **aprobadas por el dueño**; integrada en `main` el 2026-07-22.
- [`I-17-clon-unico-cabecera.md`](I-17-clon-unico-cabecera.md): contrato de I-17 (Fase 3, sobre I-02).
  Unifica las tres implementaciones de deep-clone de `RackFrameConfiguration` (hallazgo U4: una manual +
  dos por serializacion) en **un solo** `RackFrameProjectStore.DeepCopy` (round-trip del store de
  serializacion); el dinamico, el selectivo y el configurador lo consumen, y se elimina el clon manual
  campo-por-campo del configurador (`CopyConfiguration` + 7 ayudantes). El documento es la fuente unica de
  los campos **persistidos**; el modelo **derivado** se reconstruye en la carga; y las **excepciones
  runtime** (`FrameExceptionOverride`), que el documento no persiste ni el modelo derivado reconstruye, se
  **reanexan dentro del propio `DeepCopy`** para un clon completo (sin tocar el formato en disco). Con
  pruebas de equivalencia (preservacion del modelo persistido, de cada excepcion sin compartir
  referencias, del modelo derivado **miembro a miembro**, independencia, idempotencia y equivalencia con
  las dos rutas previas), una **guarda por reflexion** de clasificacion de propiedades y una **regresion de
  I-11** (`ExtensionData` via `WithSourceMetadataFrom`). **Sin cambio** de dibujo, geometria, BOM, GUID,
  persistencia fisica, DTO ni UI. Fuera de alcance: los stores de I-03, rediseno de configuradores y
  migraciones adicionales de selectivo/dinamico. `requires_autocad: false`,
  `requires_owner_validation: false`. Candidato validado `28e5cfe` (CI run 29952433309, 4 jobs verdes).
  **Integrada en `main` el 2026-07-22.**
- [`I-30-editor-visual-shell.md`](I-30-editor-visual-shell.md): contrato de I-30 (Fase 5, sobre
  I-14/I-15/I-20/I-21/I-24 integradas). Funda el **shell visual común de editores**
  (`RackEditorVisualShell`): composición por slots con `ContentControl`/`ContentPresenter`, tokens de
  tamaño/color/tipografía/espaciado en `Themes/AppStyles.xaml`, status presenter con severidades y
  action bar con categorías, tooltips y motivos de indisponibilidad; **más la migración real de
  `RackDynamicSystemWindow`**, sin cambio de dibujo, BOM, GUID ni persistencia. El shell es
  **agnóstico a `RackSystemKind`** y no admite ramas por sistema; los editores sin matriz quedan
  soportados por slot opcional. La auditoría midió sobre `main` = `8a1bce5` que las tres ventanas
  ricas **no adoptan ningún control de I-14** (`NumericField`/`CatalogCombo`/`RackDialogWindow` con
  cero consumidores; los `PreviewCanvas` de las ventanas son un `x:Name` homónimo, no el control),
  con **43 brushes privados** duplicados y sin tokens. Fuera de alcance: **Selectivo (es I-31)**,
  `feature/push-back` (**solo lectura** y handoff posterior), cama/configurador/larguero, geometría,
  BOM, persistencia, catálogos, handlers y Plugin.
  [ADR-0019](../adr/0019-shell-visual-de-editores-por-composicion.md) **aceptado por el Owner**. El shell
  es un **control lookless con plantilla** (`Themes/Generic.xaml`), no un `UserControl`, para admitir
  contenido con nombre en los slots sin `MC3093`; `RackDynamicSystemWindow` quedó **compuesto sobre el
  shell** consumiendo el contrato de tamaño común (`EditorShellWindowStyle` + tokens, `ShellMinHeight`
  672) y la paleta de estado por tokens, **sin cambio** de dibujo/BOM/GUID/persistencia. El Owner
  **validó en AutoCAD 2025 los 12 puntos** sobre el DLL del SHA `d443ee2`
  (`1.0.0+d443ee2…`); `autocad` y `owner_validation` **resueltos**. **Integrada en `main` el
  2026-07-24.** Handoff obligatorio: **I-31 → reanudación de I-18**. Estado versionado en
  [`../automation/state/I-30.yml`](../automation/state/I-30.yml).
- [`I-31-selective-visual-shell.md`](I-31-selective-visual-shell.md): contrato de I-31 (Fase 5, sobre
  I-30 integrada; segundo eslabón de la secuencia **I-30 → I-31 → reanudación de I-18**). Migra
  `RackSelectiveWindow` al **shell visual común** (`RackEditorVisualShell`) por composición y slots
  (`SidePanelContent`/`MatrixContent`/`PreviewContent`/`StatusContent` + categorías neutrales
  `Leading`/`Secondary`/`Primary`/`Trailing`), aplicando `EditorShellWindowStyle` y los tokens
  `Shell*`, y **eliminando la segunda composición exterior** del Selectivo (grid 342 px, scroll
  exterior, disposición independiente de matriz/preview/status, barra inferior propia). Conserva los
  **45 `x:Name`**, los 31 handlers, el parsing/`LostFocus`/recomputación, el selector y matrices por
  fondo, el editor de celda, cabeceras/peraltes por poste, seguridad, previews frontal/lateral,
  inserción frontal/lateral/planta, actualización en sitio, BOM, biblioteca, metadata I-11, GUID/
  nombre/round-trip y los estados habilitado/deshabilitado con sus motivos. **Adaptación exclusivamente
  XAML** (el `.cs` no se toca). **Confirmado en lectura: `main` NO tiene multiselección en la matriz
  principal del Selectivo** (selección simple + alcance Celda/Nivel/Frente/Todas); I-31 la conserva y
  **no** agrega multiselección. Fuera de alcance: Push Back/`feature/push-back` (**solo lectura** +
  handoff), I-18/I-23/I-25/PB-VAL-06, geometría/BOM/persistencia/GUID/comandos/handlers/seguridad/
  catálogos/Domain/Application/Plugin, sustitución por `NumericField`/`CatalogCombo`, adopción del
  control `PreviewCanvas` y dependencias NuGet. Cubierta por ADR-0019 **ya aceptado**
  (`requires_owner_decision: false`); `requires_autocad: true`, `requires_owner_validation: true`.
  El Owner **validó en AutoCAD 2025 los 12 puntos** sin observaciones sobre el DLL del SHA `b638653`
  (`1.0.0+b638653…`; CI del candidato run `30108459424`); gates `autocad`/`owner-validation` resueltos.
  **Integrada en `main` el 2026-07-24** (merge `--no-ff` `ad0ea1f`). Estado versionado en
  [`../automation/state/I-31.yml`](../automation/state/I-31.yml).
- [`I-32-correcciones-push-back.md`](I-32-correcciones-push-back.md): contrato de I-32 (sobre
  `fix/correcciones-push-back`), **correcciones funcionales y geométricas de Push Back** reportadas por el
  Owner tras usar en AutoCAD el sistema que integró I-18. Corrige **diez** hallazgos: la **pendiente de la
  cama** (subía 11.2" en un rack de 204" cuando la regla es 7/16" por pie, 7.4375"), el **primer nivel** de
  un rack nuevo (4"), la **tarima general** (Fondo y Unidad globales; Frente/Alto/Peso por celda), la
  **matriz del desviador** por poste (máximo de frentes adyacentes) y su selector de **lado**, el **tipo de
  tope** desde catálogo y las opciones que no aplican en su diálogo, y la **defensa de montacargas**
  (extremos renombrados a «Entrada/Salida» y «Posterior», posterior apagado por defecto y 12"/36"
  recalculados con Auto por extremo). Las decisiones llegaron fijadas con el encargo
  ([`../automation/decisions/I-32.md`](../automation/decisions/I-32.md)), que además **precisa y deroga
  parcialmente** I-18 PB-0.2 §4 sobre el ajuste al troquel del extremo alto. **No** cambia Selectivo ni
  Dinámico: los cuatro diálogos compartidos solo ganan parámetros opcionales cuyo default es el
  comportamiento vigente. Fuera de alcance y registrados como candidatos futuros en `ideas-futuras.md`:
  **PB-001** (preview), **PB-007** (reconfigurador masivo de seguridad), **PB-011** (editor avanzado de
  módulos) y **PB-014** (frente en blanco). `requires_autocad: true`, `requires_owner_validation: true`;
  `requires_owner_decision: false`. Estado versionado en
  [`../automation/state/I-32.yml`](../automation/state/I-32.yml).
- [`I-33-frente-en-blanco.md`](I-33-frente-en-blanco.md): **Frente en blanco para Dinámico y Push Back**
  (tipo: feature; rama `feature/frente-en-blanco`). Implementa **PB-014**, que I-32 dejó diferido
  señalando que «necesita decisión de alcance» por ser compartido con el Dinámico; esa decisión la da el
  Owner al abrir la iniciativa. Un frente pasa a tener estado **Activo / En blanco**: en blanco **conserva
  su claro y su estructura** (postes, alturas, cabeceras, separadores y postes derivados), **sigue
  desplazando** a los frentes posteriores y **no lleva ningún nivel ni componente de carga** —ni larguero
  IN/OUT, ni intermedio, ni cama, ni larguero posterior o tope de Push Back, ni seguridad indexada por
  nivel— en ninguna de las cuatro vistas ni en los dos BOM. Su configuración queda **dormida** para
  reactivarlo tal cual estaba, **sin celda falsa**. La regla vive en **una** autoridad de Application
  (`DynamicFrontActivation`) sobre la **estructura dinámica que Push Back compone**, así que los dos
  sistemas no pueden divergir; para un frente activo devuelve el histórico `Math.Max(1, LoadLevels)`, de
  modo que un rack sin frentes en blanco **no cambia en nada** y **serializa igual que antes** (la bandera
  se omite del wire). Los documentos legacy cargan **todos los frentes activos**. La regla «al menos un
  frente activo» tiene **una sola** comprobación canónica y **nada la normaliza en silencio**: el editor
  **previene** de forma no destructiva (se niega a blanquear el último activo, sin cambiar nada) y un
  payload explícitamente todo en blanco se **rechaza con error visible** en el resolver y en
  `RackDesignValidation`. Al **crecer** frentes, el nuevo **nace activo aunque el template seleccionado esté
  en blanco**; al **seleccionar** un frente en blanco la selección sigue siendo válida pero se deshabilitan
  los controles de nivel/celda y los alcances, y reactivarlo restaura la edición de inmediato. Fuera de
  alcance: Selectivo, **PB-001**, **PB-007**, **PB-011**, I-23, I-25 (declaradas en `conflicts_with`),
  catálogos, DWG y el shell visual. En los **diálogos de seguridad** las celdas de nivel de un frente en
  blanco son **inexistentes** y su configuración guardada se preserva **dormida**; la forma de la rejilla
  del desviador y la visibilidad del **selector de lado** quedan **desacopladas**. Incorpora además
  —**decisión del Owner**— que la **frontera compartida por dos frentes en blanco NO existe**: los dos
  bordes exteriores existen siempre y una interior existe salvo que sus **dos** frentes adyacentes estén en
  blanco, así que una corrida de N blancos pierde sus **N−1** fronteras interiores; desaparece el **ensamble
  físico** (poste, placa, cabecera/separador, postes derivados y refuerzos, el corte lateral entero, su
  parte del BOM y su seguridad por poste) y **nunca el frente lógico** —índices, claros, ancho, largo total
  y coordenadas X se conservan—, con `DynamicFrontActivation.BoundaryExists` como autoridad única.
  `requires_autocad: true`, `requires_owner_validation: true`; `requires_owner_decision: false`.
  **INTEGRADA (2026-07-27)**: el Owner **aprobó** toda la validación manual, incluida la ronda focalizada de
  fronteras físicas, sobre el candidato `b840cfe` (CI 30240730244, 4/4); **sin rebase** (`origin/main` no
  avanzó desde la base `0e505d8`); integrada por `git merge --no-ff`; rama y worktree eliminados tras la CI
  post-merge verde. Estado versionado en
  [`../automation/state/I-33.yml`](../automation/state/I-33.yml).
- [`I-34-edicion-masiva-seguridad.md`](I-34-edicion-masiva-seguridad.md): **Edición masiva de matrices de
  seguridad** (tipo: feature; rama `feature/edicion-masiva-seguridad`). Implementa **PB-007**, que I-32
  registró y **I-33 dejó explícitamente fuera de alcance** señalando que «toca los diálogos COMPARTIDOS,
  así que afecta a Selectivo y Dinámico: necesita decisión de alcance del Owner»; esa decisión la da el
  Owner al abrir la iniciativa. Hoy las cinco rejillas de seguridad son celda a celda y solo ofrecen
  «Todos»/«Ninguno»: quitar el desviador del segundo nivel en 100 frentes cuesta 100 clics. Añade una
  **fundación común pura sobre `SelectionMatrixModel`** con **celda primaria no persistida**, estado
  **Activar/Desactivar** y alcances **Celda / Nivel / Frente-o-Poste / Todo** —la misma gramática de
  «Aplicar a:» que ya usan los editores de diseño (`SelectiveApplyScope`, `DynamicRackCellScope`)—. La
  infraestructura es **agnóstica a `RackSystemKind`**: cada diálogo **declara** sus etiquetas (el eje de
  columna es «Frente» o **«Poste»**) y sus capacidades, en vez de derivarlas. Las **celdas ausentes**
  (rejilla dentada de I-22, columna de frente en blanco de I-33) nunca cambian ni se reportan y la
  configuración **dormida** de `SafetyDormantCells` queda intacta; cada operación masiva emite **una**
  notificación agregada con exactamente las celdas cambiadas, y el control repinta esas casillas **sin
  rebuild**. **Este primer incremento** entrega el **inventario auditado** de las cinco matrices booleanas
  (tope, tope posterior de Push Back, desviador —el único eje por **poste**—, guía de entrada y parrilla),
  las decisiones cerradas, las **regresiones rojas** y la fundación; **NO migra todavía los diálogos**.
  Fuera de alcance: la adopción por los diálogos, DTO/wire/stores, geometría, BOM, catálogos, DWG,
  namespaces (I-23), guardas traseras (I-25), shell visual, `DesviadorCellsAreByPost`, y —con
  justificación explícita— **parrilla** (lleva un contador vivo por celda; I-22 ya la excluyó de la
  adopción del control), **defensa** (formulario por poste con dos longitudes, sin eje de nivel) y la
  **matriz estructural de tarimas** (es diseño, no seguridad). `requires_autocad: true`,
  `requires_owner_validation: true`; `requires_owner_decision: false`. **INTEGRADA (2026-07-27)**: la
  adopción quedó **completa en las CUATRO matrices** —desviador (eje **Poste**), tope (eje **Frente**, que
  cubre a la vez el tope del Selectivo y el **tope posterior de Push Back**), guía y **parrilla**—. La
  **parrilla entró por addendum normativo del Owner** durante la validación, con la condición de
  **conservar su contador vivo por celda**: se resolvió con un **adorno opt-in y neutral** del control
  (`CellAdornment` + `RefreshAdornments`), de modo que los otros tres diálogos no cambian ni una línea.
  Corrigió además un defecto propio: un valor **no definido** de `SelectionMatrixScope` se interpretaba
  como «Todo» y reescribía la rejilla entera; ahora falla cerrado. La **defensa** no entró y **no
  bloqueó**: pasa a `ideas-futuras.md` como candidato futuro **independiente**. El Owner **aprobó** la
  validación manual en AutoCAD 2025 sobre el candidato `dbdda74` (DLL SHA-256 `5353C298…`, CI 30283957763
  4/4); `origin/main` no avanzó desde la base `7e48b5c`, así que **sin rebase**. Estado versionado en
  [`../automation/state/I-34.yml`](../automation/state/I-34.yml).
- [`I-35-editor-avanzado-push-back.md`](I-35-editor-avanzado-push-back.md): **Editor avanzado de módulos
  de Push Back** (tipo: feature; rama `feature/editor-avanzado-push-back`). Implementa **PB-011**, la
  «prioridad alta del Owner» que I-32 dejó diferida en [`../ideas-futuras.md`](../ideas-futuras.md): el
  Dinámico permite seleccionar un módulo —cabecera o separador— y personalizarlo (medida, cantidad de
  separadores, cabecera personalizada); **Push Back no**. La auditoría de la base `7e48b5c` fija siete
  hechos: el XAML de Push Back **no tiene** superficie de módulos; su `PushBackEditorDesignAssembler`
  **ya compone** el ciclo de recálculo del Dinámico sobre un `WorkingBaseline` que solo avanza en un
  `AcceptComputation` exitoso; `forceRebuild` existe **sin consumidor** (no hay «Restaurar estándar»);
  **toda** cabecera de Push Back es «calculada» porque solo la ventana del Dinámico pone
  `UseCalculatedHeaderConfiguration = false`; `RestoreHeaderFondos` guarda **solo el fondo** y **fuerza
  la vuelta a calculada**, así que un cambio estructural revertiría cualquier cabecera personalizada;
  `DynamicRackSystemResolver.CloneHeader` **no** es el clon canónico de I-17 y descarta las
  `Exceptions` runtime; y **no existe confirmar/cancelar** en ninguna parte —
  `RackFrameConfiguratorWindow` muta por referencia y no tiene Aceptar ni Cancelar—. Los dos últimos
  hechos son **inertes hoy** y se vuelven reales en la misma sesión en que Push Back gane cabeceras
  personalizadas: es la **inconsistencia de PB-011** registrada en `ideas-futuras.md`. Fuera de
  alcance: `SelectionMatrix*`, operaciones masivas y `Safety*GridWindow` (**los reclama I-34**, hoy en
  `validating`/`owner-validation`), topes, desviadores, guías, defensas,
  `DesviadorCellsAreByPost`, el Selectivo, catálogos, bloques DWG, cambios funcionales en el Dinámico,
  copiar el editor Dinámico, ramas por `RackSystemKind`, I-23 e I-25. **Gate abierto**
  (`requires_owner_decision: true`): `DynamicRackSystem.Modules` es **una sola secuencia longitudinal de
  rack** compartida por todos los frentes y postes, así que personalizar un módulo personaliza el rack
  entero; si el Owner espera personalización **por frente o por poste**, el alcance deja de ser PB-011.
  **Decisiones del Owner (gate CERRADO)**: la personalización es por **módulo longitudinal de rack**,
  nunca por frente ni por poste; en una recomputación estructural se conserva **únicamente** con
  correspondencia exacta **`ModuleId + Kind`**; un módulo eliminado o con el tipo cambiado pierde su
  personalización **de forma explícita y reportable** y uno nuevo nace calculado; **no existe política
  ordinaria `Discard`** (solo restauración explícita o incompatibilidad estructural); y
  `RackFrameConfiguratorWindow` **no se modifica** — confirmar y cancelar viven en la sesión y en la
  superficie de Push Back, que abre el configurador compartido sobre una **copia**.
  **Entregado**: reconciliación por `ModuleId + Kind` que conserva longitudes manuales de **cabeceras y
  separadores** y **adapta** `Depth` y el peralte de rack; restauración **individual completa** de
  cualquier módulo y restauración total; reporte por categoría (**preservados, adaptados, eliminados,
  incompatibles y restaurados**); adopción por `PushBackEditorState`/`PushBackEditorDesignAssembler` en
  **los dos caminos** del recálculo; y la superficie avanzada en `RackPushBackSystemWindow` con
  selección **única**, longitud, Calculada/Personalizada, confirmar, cancelar y restaurar, deshabilitando
  **con explicación** los módulos que I-33 dejó sin dibujar y los postes derivados. **Gate documental**:
  I-35 **no tiene fila en `ROADMAP.md`**; la procedencia del alcance es PB-011 y la fila la escribe la
  sesión de integración. `requires_autocad: true`, `requires_owner_validation: true`;
  `requires_owner_decision: false`. **Segunda ronda**: la primera validación del Owner quedó
  **parcialmente rechazada** por **cuatro residuos** —altura manual de cabecera, refuerzo del poste
  derivado con su longitud opcional, y cantidad y separación de separadores—, que son **parámetros
  globales del rack** y no propiedades del módulo `Separator`. Se corrigieron reutilizando
  **exclusivamente** las autoridades existentes (`ManualHeaderHeightOverride`, `DerivedPostReinforced`,
  `DerivedPostReinforcementHeight`, `SeparatorCountOverride`, `SeparatorSpacingOverride`), en una
  **sección propia** separada de «Módulo seleccionado»; el refuerzo admite **total o parcial** y una
  recomputación que invalida una altura antes válida **bloquea con error visible** en vez de recortar.
  **INTEGRADA (2026-07-27)**: el Owner **aprobó** la validación manual en AutoCAD 2025 sobre el candidato
  `f2be30c` (CI 30293536290, 4/4; DLL SHA-256 `4FE530EF…C101`); **sin rebase** (`origin/main` no avanzó
  desde la base `52ce27f`); integrada por `git merge --no-ff`; rama y worktree eliminados tras la CI
  post-merge verde. Estado versionado en
  [`../automation/state/I-35.yml`](../automation/state/I-35.yml).
- [`I-23-namespaces-sistemas.md`](I-23-namespaces-sistemas.md): **Namespaces finales por sistema**
  (tipo: refactor; rama `refactor/namespaces-sistemas`). Cierra el hallazgo **E8** de la auditoría
  2026-07 y **CIERRA LA FASE 5**. Refactor **mecánico** bajo congelación funcional total: **176 archivos
  movidos con `git mv`**, todos registrados como renombre, más la reescritura de `namespace`/`using` en
  sus consumidores, **sin una sola línea de lógica**. Reparte los **cuatro** proyectos de producto
  —Domain, Application, **UI** y Plugin— en `Systems.{Selective, Dynamic, PushBack, FlowBed, Larguero,
  Shared}`: las tres raíces planas de `Systems` quedan **vacías** y se disuelven **cinco** namespaces,
  incluidos `Application.Headers` y `Plugin.Headers`. La cabecera **física** conserva `RackFrames` en
  Domain, Application y UI; lo que **materializa** pasa a `Drawing`, que queda simétrico en Application y
  Plugin. Único renombre autorizado: **`DynamicSystemPlan` a `Drawing.HeaderRunPlan`** — **no** se aplicó
  el `SystemPlan` que anota el ROADMAP, ambiguo en el árbol actual porque colisiona con
  `SystemBomBuilder`/`SystemDescriptor`/`SystemRegistry`/`SystemBlockWriter`, que sí son por sistema;
  `HeaderGroup` y `HeaderPlacement` viajan con él y conservan su nombre. La regla es objetiva: un archivo
  pertenece al sistema que su tipo de primer nivel **nombra y modela**, y **consumir** un contrato ajeno
  no lo mueve. Por eso los **diálogos compartidos de seguridad** (`SelectiveSafetyWindow` y los cinco
  `Safety*GridWindow`) y la infraestructura transversal de UI (`Controls`, `Editor`, `Preview`, `Shell`,
  `Themes`) **no** se reparten: **un diálogo compartido no se asigna a un sistema por número de
  consumidores**. Los **dos proyectos de prueba** conservan un único namespace de ensamblado como
  **excepción explícita y comprobable** —92 de 220 archivos (42 %) ejercitan más de un sistema—, vigilada
  por `TestProjects_KeepExactlyOneAssemblyRootNamespace`. Añade `.editorconfig` y **dos guardas**:
  `NamespaceFolderGuardTests` (7 aserciones) y `UiSystemBoundaryGuardTests` (3, que **construyen de
  verdad** las seis ventanas WPF migradas y validan `x:Class`, pack URIs y recursos), verificadas **en
  rojo** bajo cinco infracciones inyectadas. `EnforceCodeStyleInBuild` **no** se activa: el proyecto WPF
  compila vía un proyecto temporal y `IDE0130` produce 68 advertencias falsas. Equivalencia
  **demostrada**: 7 goldens byte-idénticos, superficie de API **idéntica** a la base y los 28 comandos y
  alias byte-idénticos. Fuera de alcance: I-25, la defensa de I-34, el preview de I-18,
  `DesviadorCellsAreByPost` y toda regla de producto. `requires_autocad: true`,
  `requires_owner_validation: true`; `requires_owner_decision: false`. **INTEGRADA (2026-07-27)**: el Owner
  **aprobó el smoke mínimo** en AutoCAD 2025 —NETLOAD, RACKCAD, un editor de sistema y RACKCABECERA, sin
  errores de carga, comandos, XAML ni recursos— sobre el candidato `5d49a6c` (CI 30304742946, 4/4);
  **sin rebase** (`origin/main` no avanzó desde la base `b43b5d1`); integrada por `git merge --no-ff`.
  Con ella **se cierra la Fase 5**; **Push Back v1 queda estable** e **I-25 sigue en backlog diferido**.
  Inventario completo de los seis proyectos en
  [`I-23-inventario-namespaces.md`](I-23-inventario-namespaces.md) y registro del smoke en
  [`I-23-autocad-smoke.md`](I-23-autocad-smoke.md). Estado versionado en
  [`../automation/state/I-23.yml`](../automation/state/I-23.yml).
- [`I-36A-catalogo-secciones-estructurales.md`](I-36A-catalogo-secciones-estructurales.md): **Nucleo y
  catalogo de secciones estructurales** (tipo: architecture; rama
  `architecture/catalogo-secciones-estructurales`). **Primera iniciativa de la Fase 6.** Funda
  `StructuralSection` como autoridad **neutral** de la seccion transversal —**sin rol de miembro**,
  independiente de `RackCatalog` y de `CatalogEntryBase`— en
  `RackCad.Application.StructuralSections`, e importa **completas** cuatro familias de la **AISC
  Shapes Database v16.0**: W, HSS rectangular y cuadrado, canales C y angulos L (**983** secciones:
  289 + 525 + 32 + 137). El problema es de modelado: hoy una fila de `secciones.csv` es a la vez
  seccion, **rol** (`POSTE`/`CELOSIA`/`LARGUERO`/`SEPARADOR`) y pieza comercial, y Cantilever rompe
  la coincidencia porque la misma `W12X26` puede ser columna, brazo o base. Entrega: identidad
  `AISC-{FAMILIA}-{EDI_NORMALIZADO}` con normalizacion determinista y **sin la revision dentro del
  id**; siete archivos bajo `assets/catalogs/` (cuatro CSV de familia **generados**, fuentes, overlay
  `IsEnabled` y manifiesto con conteos y SHA-256, sin timestamps); un **importador reproducible fuera
  del producto** (`tools/`, .NET 8, **BCL puro**, cero NuGet, cero Office Interop, OOXML por ZIP/XML,
  staging, salida **byte-identica** entre ejecuciones y **cero descartes silenciosos**); un **lector
  CSV estricto dedicado** —la tolerancia historica de `CsvCatalogReader` **no se altera**; solo se
  comparte su parser lexico RFC-4180, con regresiones—; catalogo con busqueda por id, EDI normalizado
  y familia, que devuelve **solo habilitadas** por defecto mientras `GetById` **sigue resolviendo las
  deshabilitadas** para diseños existentes; validador propio que reutiliza las severidades de I-19 sin
  tocar `CatalogValidator`; unidades con peso nativo en `lb/ft`, equivalencia `kg/m` **calculada** y
  formateador dual **puro**; y peso por longitud. Decisiones en
  [ADR-0020](../adr/0020-catalogo-neutral-de-secciones-estructurales.md) —que **reemplaza a ADR-0008**
  **solo en autoridad conceptual**: `secciones.csv` sigue operando **sin cambio** como catalogo legado
  hasta que las migraciones futuras, una por configurador (strangler), lo retiren— y
  [ADR-0021](../adr/0021-identidad-unidades-y-presentacion-de-secciones.md) —que **NO reemplaza a
  ADR-0005**: la pulgada sigue siendo la unidad geometrica interna, no se altera `INSUNITS` y no se
  implementa conversion del DWG—. Las 24 decisiones vinculantes del dueño y las dos discrepancias que
  el ejecutor encontro en el encargo (la URL `globalassets` responde **404** y la pagina oficial
  publica el libro en `cloud.aisc.org`; y el ejemplo de id para HSS del encargo corresponde al **AISC
  Manual Label**, no al **EDI**, de modo que `HSS4X4X1/4` tiene id `AISC-HSS-RECT-HSS4X4X_250`) estan
  versionadas en [`../automation/decisions/I-36A.md`](../automation/decisions/I-36A.md). **Fuera de
  alcance**: I-36B y toda geometria, AutoCAD, WPF/preview, Cantilever (I-37/I-38), persistencia de
  sistemas, migracion de `secciones.csv`, miembros y sus herrajes, BOM de sistemas, calculo
  resistente, overrides distintos de `IsEnabled`, base SQL, NuGet y `blocks-library.dwg`.
  `requires_autocad: **false**` (no cambia dibujo ni bloques), `requires_owner_validation: **true**`
  (siete puntos: fuente y SHA, conteos, sentinelas, etiquetas de peso, politica de ids,
  habilitado/deshabilitado y confirmacion de que los sistemas existentes no cambiaron);
  `requires_owner_decision: false`. Estado versionado en
  [`../automation/state/I-36A.yml`](../automation/state/I-36A.yml).
- [`I-36B-geometria-secciones-estructurales.md`](I-36B-geometria-secciones-estructurales.md):
  **Geometria y representacion prismatica de secciones estructurales** (tipo: architecture; rama
  `architecture/geometria-secciones-estructurales`). **Segunda iniciativa de la Fase 6.** I-36A entrego
  983 secciones y, deliberadamente, ninguna forma de VERLAS; I-36B las convierte en geometria
  **generada en codigo** desde sus dimensiones. El patron vigente no servia: resolver cada pieza a un
  bloque de `blocks-library.dwg` via `blocks.csv` exigiria **983 bloques dibujados a mano** contra un
  archivo que no se versiona, y perderia justo lo que hace util un perfil normalizado —que su contorno
  es derivable—. Entrega: primitivas neutrales **aditivas** en `RackCad.Application.Geometry`
  (vector, limites, transformacion con espejo, arco circular, contorno cerrado con area y centroide
  **analiticos**, marcos 3D); constructores por familia (W, HSS rect./cuad., C, L) en dos niveles de
  detalle con **fidelidad declarada** y diagnosticos; **instancia prismatica** donde vive la longitud
  —la seccion no tiene largo—, con rotacion y espejo; proyeccion ortografica a cinco vistas
  (deliberadamente **no** llamadas frontal/lateral/planta) y teselado determinista por tolerancia de
  cuerda; un **plan neutral unico** con roles semanticos y firma determinista que **el preview de la UI
  y el adaptador de AutoCAD consumen igual** —no puede haber dos generadores geometricos, y hay guardas
  de codigo que lo comprueban—; cache perezosa por seccion y detalle; inspector minimo sobre
  `PreviewCanvas`/`PreviewProjection`; y el comando **`RACKSECCION`**, que materializa el plan como
  **bloque interno del dibujo**, sin `blocks-library.dwg` y sin filas nuevas en `blocks.csv`. La regla
  que no se cruza: **no se inventa un radio que la fuente no publique** —`r = kdes − tf` y la esquina
  del HSS **si** se derivan y estan documentados; el redondeo de punta del ala y la conicidad del
  canal **no**, y por eso C y L declaran `TabulatedDerived` y nunca `TabulatedComplete`—. El error de
  area por familia se **mide y se reporta** sin manipular la geometria: el del HSS (media 8.34 %) es
  una **diferencia de definicion** —AISC calcula `A` con espesor de diseño, la geometria dibuja el
  nominal— acreditada por una prueba que reconstruye el mismo contorno con `tdes` y cae a 1.068 %.
  Resultado medido sobre la v16.0: 289 `TabulatedComplete`, 694 `TabulatedDerived` y **cero**
  degradadas. Decision en
  [ADR-0022](../adr/0022-geometria-parametrica-de-secciones-estructurales.md), que **no reabre** a
  ADR-0020 ni a ADR-0021 y respeta [ADR-0005](../adr/0005-estrategia-de-unidades.md) (pulgada interna,
  cero conversion). Las 25 decisiones vinculantes del dueño estan versionadas en
  [`../automation/decisions/I-36B.md`](../automation/decisions/I-36B.md). **Fuera de alcance**: I-37
  Cantilever e I-38, `StructuralMember` y todo configurador de miembro, conexiones/troqueles/placas/
  soldaduras/cortes de extremo, cargas y calculo resistente, persistencia y round-trip de lo insertado,
  solidos 3D y bloques dinamicos, migracion de catalogos, cambios en sistemas vigentes, en
  `blocks-library.dwg` o filas nuevas en `blocks.csv`. `requires_autocad: **true**` (dibuja, asi que
  las pruebas no cierran el gate), `requires_owner_validation: **true**` (checklist de doce puntos en
  la evidencia); `requires_owner_decision: false`. Guia en
  [`../guias/geometria-secciones-estructurales.md`](../guias/geometria-secciones-estructurales.md);
  estado versionado en [`../automation/state/I-36B.yml`](../automation/state/I-36B.yml).
- [`I-36C-acceso-menu-secciones-estructurales.md`](I-36C-acceso-menu-secciones-estructurales.md):
  **Acceso desde el menu principal al generador de perfiles estructurales** (tipo: fix; rama
  `fix/acceso-menu-secciones-estructurales`). **Fix pequeno de descubribilidad, no de funcionalidad.**
  El generador **ya existe, esta integrado y esta cerrado** —catalogo neutral de I-36A con 983 secciones
  AISC v16.0, geometria parametrica de I-36B para W, HSS rectangular/cuadrado, C y L, inspector, preview,
  materializacion en bloque interno y el comando `RACKSECCION`—, pero la **unica** forma de invocarlo era
  escribir el comando: el menu `RACKCAD`, que es por donde entra un usuario, no lo mencionaba. Entrega un
  boton **«Generar perfil estructural»** entre «Disenar larguero» y «Abrir de la biblioteca de disenos»,
  con el estilo `MenuButton` vigente; una **accion tipada** `MainMenuAction.GenerateStructuralSection`
  que el Plugin lee **despues** de cerrar el modal —porque el flujo pide un punto y el editor de AutoCAD
  tiene que estar libre—; y una **autoridad compartida** `StructuralSectionCommandFlow` que consumen
  igual `RACKSECCION` y el boton. La accion **no** es un `RackInsertionRequest` a proposito: una seccion
  no es un rack —sin `RackSystemKind`, sin payload de diseno y sin round-trip— y un request con un `Kind`
  inventado empujaria esa mentira hasta el `switch` del host. **No crea un segundo generador**, y 25
  guardas de fuente lo comprueban: cada pieza del caso de uso la menciona exactamente un archivo del
  Plugin. **Fuera de alcance**: IPS/S, familias nuevas, cualquier cambio geometrico, la mejora visual de
  los canales C, I-37, solidos 3D, persistencia, round-trip, sistemas existentes, `blocks.csv`,
  `blocks-library.dwg`, los CSV de secciones y el rediseno del menu. `requires_autocad: **true**` y
  `requires_owner_validation: **true**` (el cambio es visible en el menu); `requires_owner_decision:
  false`. **INTEGRADA el 2026-07-28**: el Owner aprobo los siete puntos de la validacion en AutoCAD
  —boton visible, posicion y estilo, cancelacion del inspector, cancelacion del punto, insercion de
  `W12X26`, **equivalencia con `RACKSECCION`** y sistemas existentes sin regresiones— **sin
  observaciones ni bloqueos**. Estado versionado en
  [`../automation/state/I-36C.yml`](../automation/state/I-36C.yml).
- [`I-36D-perfiles-aisc-s.md`](I-36D-perfiles-aisc-s.md): **perfiles AISC S/IPS y geometria visual
  derivada**, cuarta iniciativa de la Fase 6 y la iniciativa separada que I-36B exigio como
  **requisito futuro obligatorio**. Incorpora las **28 filas** `Type = S` que I-36A dejo fuera
  —contadas y declaradas, no perdidas— como **familia propia** con token `S` e id `AISC-S-S10X25_4` (el punto normaliza a `_` por ADR-0021),
  con `SSectionDimensions` como **tipo propio** y no como alias de W. El hecho que la gobierna esta
  **medido contra el libro**, no citado: la AISC Shapes Database v16.0 **no publica la pendiente del
  patin ni ningun radio explicito**, ni para S ni para ninguna familia —el unico encabezado con `tan`
  es `tan(alfa)`, que es de angulos simples y esta vacio en S; `kdes`, `kdet`, `k1` y `T` son
  **distancias al pie del filete** y el Readme nunca las llama radios—. Como una S sin pendiente se
  lee como una **W**, la iniciativa separa dos autoridades: AISC conserva dimensiones, `A`, peso y
  propiedades; **RackCad declara como propia** la convencion visual (pendiente `1:6`, `tf` como
  espesor medio del vuelo libre, radio visual del filete y punta aguda), en un eje **ortogonal** a
  `SectionFidelity` —`TabulatedConstrained` frente a `VisualDerived`—, con advertencia obligatoria de
  geometria aproximada **no apta para CNC ni fabricacion**. La regla **degenera exactamente** en la de
  [ADR-0022](../adr/0022-geometria-parametrica-de-secciones-estructurales.md) cuando la pendiente es
  cero. **Fuera de alcance**: I-37 y cualquier miembro Cantilever, calculo resistente, materiales,
  solidos 3D, persistencia y round-trip, la mejora visual de W/C/L/HSS, `bf/2tf` y `h/tw`,
  `secciones.csv`, catalogos de sistemas, `blocks.csv`, `blocks-library.dwg`, geometria de fabricante,
  CNC y descarga en runtime. `requires_autocad: **true**` y `requires_owner_validation: **true**`
  (el dibujo es el criterio); `requires_owner_decision: false`.
  [ADR-0023](../adr/0023-geometria-visual-derivada-perfiles-s.md) nacio **`propuesto`** y **solo el
  Owner** podia aceptarlo tras ver el dibujo real: lo hizo el **2026-07-28** y quedo **`aceptado`**.
  **INTEGRADA el 2026-07-28**: el Owner aprobo la validacion manual en AutoCAD 2025 **sin
  observaciones** (veredicto `OWNER_APPROVED_ADR_0023`) sobre el SHA tecnico `3ffe4df`. Decisiones vinculantes en
  [`../automation/decisions/I-36D.md`](../automation/decisions/I-36D.md), auditoria medida en
  [`../automation/evidence/I-36D-auditoria-aisc-s.md`](../automation/evidence/I-36D-auditoria-aisc-s.md)
  y estado versionado en [`../automation/state/I-36D.yml`](../automation/state/I-36D.yml).
- [`I-37A-cantilever-base-columna.md`](I-37A-cantilever-base-columna.md): **fundacion Cantilever, base y
  columna**. Primera subiniciativa de **I-37**, autorizada expresamente por el Owner con el precedente de
  I-36A: su primer commit sustantivo escribe la fila del ROADMAP, el contrato, la decision versionada, el
  ADR y el estado. Funda el **primer miembro** de RackCad sobre el catalogo neutral, **puro en Domain y
  Application**. El diseno vive en `RackCad.Domain.Systems.Cantilever` y guarda el id de seccion como
  **texto** —Domain no puede referenciar Application, donde vive `StructuralSectionId`, y los cinco
  sistemas ya guardan asi sus ids de catalogo—; la resolucion vive en
  `RackCad.Application.Systems.Cantilever` como **frontera unica**: parseo, lookup, politica de
  elegibilidad **inyectable por ids exactos**, colocacion y diagnosticos. El resultado es **hibrido**: un
  tipo por **naturaleza fisica** —perfil del catalogo, placa, cartabon, troquel— con el rol como enum
  dentro del plan de miembro, y `PrismaticSectionInstance` como **unica autoridad de colocacion**. El
  patron de agujeros de la conexion tiene **una sola autoridad**, `CantileverColumnBaseConnectionPattern`,
  que consumen igual la placa posterior de la base y la cara de conexion de la columna; su coincidencia se
  prueba sobre un **datum logico** (X, Z y eje de perforacion) y **nunca** comparando centros 3D separados
  por el espesor de una placa. Toda dimension exterior se deriva de `StructuralSectionGeometry.Bounds`:
  **cero** accesos a `d`/`bf`/`tw`/`tf`, cero lectura de CSV y cero `RackCatalog`, con guardas de fuente
  que lo comprueban. `NominalCutLength == Length` por definicion y con prueba, y **no** esta liberada para
  fabricacion. **Fuera de alcance**: brazos y pendiente, niveles, estacion, doble cara, separadores,
  arriostres, linea, BOM, peso, persistencia de `RackProject`, `RackSystemKind`, los tres registros,
  biblioteca, editor, preview, vistas, AutoCAD, comandos, materiales, calculo, soldaduras, anclas,
  tornilleria, CNC, familias nuevas y bloques DWG. `requires_autocad: **false**` y
  `requires_owner_validation: **false**` — no cambia el dibujo ni la interfaz, asi que no hay nada que
  validar. **INTEGRADA el 2026-07-29**:
  [ADR-0024](../adr/0024-fundacion-cantilever-base-columna.md) quedo **`aceptado`** con veredicto
  `OWNER_APPROVED_ADR_0024` sobre el SHA tecnico `1552367`, y la integracion se autorizo con
  `OWNER_AUTHORIZED_INTEGRATION_I_37A`. Es la primera ADR de la Fase 6 aceptada **sobre el codigo** y no
  sobre el dibujo, porque la iniciativa no dibuja: lo verificable de un contrato son sus invariantes y sus
  guardas. Suites 2224 y 544, once regresiones comprobadas en rojo, y los **dos offsets sin default
  aprobado siguen sin default**, como entradas obligatorias que el resolver rechaza si faltan. Decisiones
  vinculantes del Owner para **toda** la linea I-37 en
  [`../automation/decisions/I-37.md`](../automation/decisions/I-37.md) y estado versionado en
  [`../automation/state/I-37A.yml`](../automation/state/I-37A.yml).
- I-13 conserva su evidencia detallada en `archive/i-13-experiment-final-4e084d2`; su promocion fue
  revalidada, autorizada e integrada en `main` el 2026-07-20.
- [`I-29-licencia-procedencia-autocad-ci.md`](I-29-licencia-procedencia-autocad-ci.md): iniciativa
  cerrada documentalmente con decision B y restricciones; no autoriza por si sola el merge de I-13.
  La [decision final del Owner](../automation/decisions/I-29.md) es la fuente canonica vigente.
- La evidencia P1-P4 se conserva en el
  [archivo de auditoria I-29](../archivo/auditorias/I-29/README.md).
