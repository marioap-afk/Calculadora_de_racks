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
  se omite del wire). Los documentos legacy cargan **todos los frentes activos** y siempre debe quedar
  **al menos un frente activo**. Fuera de alcance: Selectivo, **PB-001**, **PB-007**, **PB-011**, I-23,
  I-25, catálogos, DWG y el shell visual. `requires_autocad: true`, `requires_owner_validation: true`;
  `requires_owner_decision: false`. Estado versionado en
  [`../automation/state/I-33.yml`](../automation/state/I-33.yml).
- I-13 conserva su evidencia detallada en `archive/i-13-experiment-final-4e084d2`; su promocion fue
  revalidada, autorizada e integrada en `main` el 2026-07-20.
- [`I-29-licencia-procedencia-autocad-ci.md`](I-29-licencia-procedencia-autocad-ci.md): iniciativa
  cerrada documentalmente con decision B y restricciones; no autoriza por si sola el merge de I-13.
  La [decision final del Owner](../automation/decisions/I-29.md) es la fuente canonica vigente.
- La evidencia P1-P4 se conserva en el
  [archivo de auditoria I-29](../archivo/auditorias/I-29/README.md).
