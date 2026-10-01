# I-64 — Proposal V3: workspace persistente y modeless de RackCad (ID30, absorbe ID3)

```text
Frozen: NO
Version: V3 (sustituye a V2 como objeto de revisión; V1 y V2 quedan como registro histórico:
         V1 = commit dcc16bed, blob 962fc195; V2 = commit ab4efe86, blob 7bf98cca)
Unit: I-64   Workflow: V2 (T4)   Claim-Id: 614371d5-441f-4f14-bac9-f97017105610
Archetype: NEW ARCHITECTURE
Materiality: M-01, M-04, M-05, M-06, M-07 y M-08 activados; M-02 y M-03 NOT ACTIVATED con condiciones (aceptado por el Coordinator; §14)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-64-discovery.md, versión D1-R1 (blob 3565015c); cerrado
Master: MASTER-I63-I64-02, que sustituye parcialmente a MASTER-I63-I64-01 (evidencia §15; disposición por cláusula en §19)
Review V1: Architect = CHANGES REQUIRED (A64-PV1-01..22), aceptado por el Coordinator; la disposición de V2 se conserva (§18)
Author: sesión principal responsable de I-64 (Claude), redacción directa
Review V3: PENDIENTE — re-revisión por el MISMO Architect y el Coordinator; paquete en docs/initiatives/I-64-architect-package-v3.md
IMPLEMENTATION AUTHORIZATION = NO
```

**Fuentes y precedencia:**
- **Alcance:** el [brief del Owner](I-64-owner-brief.txt), citado por el título de sus secciones.
- **Hechos:** el [Discovery](I-64-discovery.md) D1-R1, citado por sección, y el código de la base `819955d6`, citado por archivo.
- **Decisiones de partida:**
  - la aceptación de D1-R1 por el Coordinator y `MASTER-I63-I64-01` ([evidencia](../automation/evidence/I-64-evidence.md) §13);
  - el veredicto del Architect sobre V1 (A64-PV1-01..22, REQUIRED; O-01..O-17, OPTIONAL) y la orden F0 / Proposal V2 del Coordinator con
    sus 22 precisiones vinculantes, que aquí se citan como **PV2-01..PV2-22** (evidencia §14);
  - la orden F0 / Proposal V3 del Coordinator, con la decisión **MASTER-I63-I64-02** y el plan de gates confirmado (evidencia §15).
- **Precedencia:** prevalecen las autoridades integradas: AGENTS, WORKFLOW, LIFECYCLE, los ADR aceptados, los Freeze integrados (I-48,
  I-55 V5, I-58 e I-60) y AUTOMATION_PLAN §16.
- **Naturaleza:** diseño. No cambia producto, pruebas ni normas.
- **Qué se congela:** autoridad, persistencia, comportamiento observable, semántica de fallo, compatibilidad, no-objetivos, puntos de
  extensión, obligaciones invariante → prueba, la matriz OV y los resultados de los gates. **No** se congelan nombres de tipos, helpers,
  archivos ni mecánica interna ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §6). Los nombres de tipos son hipótesis de trabajo (brief, «KEY
  ARCHITECTURAL BOUNDARY»). Donde esta Proposal declara una «vía implementable», la declara como existente y verificable, no como mecánica
  congelada.

## 0. Glosario

| Término | Significado aquí |
|---|---|
| **Authored persistido** | Los sobres `RackEmbedDocument` de las definiciones de un rack en el DWG, más las dependencias que su edición lee (en Selectivo, el registro de variables). Es la **única** autoridad comprometida (brief, «KEY ARCHITECTURAL BOUNDARY») |
| **StaleBase** | Hechos crudos del authored, capturados para detectar cambios externos (D-12). No se usa para calcular dirty |
| **InitialDraftState** | El estado editable que **esta build** materializa a partir de la misma lectura lógica que produjo la StaleBase |
| **CurrentDraftState** | El estado editable actual del borrador |
| **Dirty** | CurrentDraftState difiere de InitialDraftState, o hay texto pendiente (vivo o conservado). Nunca se calcula comparando JSON crudo (D-10) |
| **Borrador** (`DraftSession`) | La unión de StaleBase, InitialDraftState, CurrentDraftState, texto pendiente conservado y estado; solo en memoria |
| **Texto pendiente** | Lo escrito en un campo y aún no comprometido al borrador (ADR-0032 D5) |
| **Compromiso interno** | Paso del texto pendiente al CurrentDraftState; ADR-0032 D6 lo llama «commit». No toca el DWG |
| **Actualizar** | La única frontera de escritura en el DWG desde el panel (brief, «PRODUCT DECISION ALREADY MADE») |
| **Sesión de documento** | El estado del panel para **una instancia abierta** de `Document`, durante toda su vida (D-03) |
| **Índice runtime** | Estructura de I-64 por sesión: inventario del documento, handles y ObjectIds, invalidación, navegación, selección, pestañas y borradores (D-05) |
| **Hechos neutrales internos** | Representación pura e interna de I-64 de lo que el índice captura por definición (identidad, Kind, vista, nombre, diagnósticos). **No** es una fundación reutilizable ni un contrato para I-63 (MASTER-I63-I64-02 §4) |
| **Pista** | Notificación de AutoCAD encolada por el puente. Nunca es autoridad |
| **Reposo** | El documento activo no ejecuta ningún comando (`IsQuiescent`) |
| **Modal RackCad activo** | Un diálogo modal de RackCad abierto: un editor clásico o un subdiálogo del panel, según el estado propio de RackCad |
| **Eco** | La selección implícita resultante de una acción explícita del panel, reconocida por su contenido (D-07) |
| **Panel RackCad** | Nombre visible propuesto para la workspace, para no confundirla con los «espacios de trabajo» de AutoCAD (`WSCURRENT`) |

La trampa de vocabulario de Discovery §4.3 queda así: lo que ADR-0032 llama «comprometido» es el CurrentDraftState; lo que el brief
llama «committed base» es el authored persistido.

## 1. Objetivo, no-objetivos y criterios de éxito

**Objetivo** (brief, «GOAL»): pasar de ventanas modales a un panel persistente y modeless dentro de AutoCAD, con sincronización de
contexto en los dos sentidos, borradores por rack y Actualizar como única frontera de escritura.

**No-objetivos** (brief, «OUT OF SCOPE» y «PREPARES BUT DOES NOT IMPLEMENT»), más los de V1:
- Insertar vistas y `BindingIntent` desde el panel: siguen en el editor clásico (D-13, D-15). El Coordinator lo trató como decisión de
  diseño compatible; si se considerase cambio de alcance, sería `OWNER-RESERVED`.
- Crear racks nuevos desde el panel: los comandos de creación no cambian.
- Edición por lotes: residual de ID3 (D-20).
- Persistir el panel o sus borradores, incluida la posición de la paleta entre sesiones (brief, «MULTI-DOCUMENT»).
- Cambiar `RACKEDITAR`, `RACKLISTA`, `RACKPROYECTAR` u otros comandos existentes (D-16).

| # | Criterio de éxito (brief) | Mecanismo | Gate | Evidencia |
|---|---|---|---|---|
| 1 | El panel puede quedar abierto sin bloquear AutoCAD | D-01, D-02 | F1 | Smoke-1; OV-01 |
| 2 | AutoCAD se usa con normalidad con el panel abierto | D-01, D-04, D-18 | F1 | INV-01, INV-24, INV-33; OV-02, OV-03, OV-19; escenario 10 |
| 3 | La selección lógica de racks se sincroniza con el panel | D-06 | F1, F6 | INV-03; OV-04, OV-14 |
| 4 | La navegación mueve la selección y la ubicación en AutoCAD | D-07 | F2 | INV-03; Smoke-NAV; OV-05, OV-06 |
| 5 | Sesiones tipo navegador sin un editor pesado por rack | D-08, D-18 | F3, F5 | INV-15; OV-08, OV-09 |
| 6 | Los borradores persisten entre pestañas y no tocan el DWG hasta Actualizar | D-10, D-14 | F4 | INV-04, INV-05; OV-10..OV-12 |
| 7 | Los cambios externos se detectan antes de escribir | D-10, D-12 | F4 | INV-06, INV-27, INV-28, INV-29; OV-13, OV-23 |
| 8 | Los documentos quedan aislados | D-03 | F1, F4 | INV-07, INV-22; OV-15 |
| 9 | La autoridad authored/effective queda intacta | D-11, D-22 | F4, F5 | INV-10, INV-16; trazas DC-08 |
| 10 | No existe una arquitectura de suscripciones por pestaña | D-04 | F1 | INV-02 |
| 11 | El rendimiento queda caracterizado y es aceptable | D-18 | F1..F7 | escenarios 1-10 de D-18; OV-22 |
| 12 | El estado de ID3 queda resuelto | D-20 | F6, READY | evaluación de F6 y clasificación de cierre |

## 2. Mapa de autoridad (M-01)

| Materia | Hoy (Discovery §12.1) | Propuesta | Origen de la decisión |
|---|---|---|---|
| Authored persistido | sobre en el DWG | **sin cambio** | brief |
| Borrador | ventana modal + `RackEditorSession` | `DraftSession` por (sesión de documento, RackId) en Application | D-10; ADR-0029 D11 |
| Dirty | ámbito en 2 de 6 editores | InitialDraftState + texto pendiente, por borrador | D-10; ADR-0029 D8 |
| Contexto de rack | el comando, al empezar | `SelectionContext` por sesión de documento | D-06 |
| Cierre | política de la ventana | política de pestaña, panel y documento | D-08, D-17 |
| Aceptación de Actualizar | el host tras el modal | puerto del panel, ligado a su sesión → comando interno del Plugin; la escritura usa la costura integrada de I-55 | D-03, D-13 |
| Comprobación de base | nadie | relectura dentro de la transacción de MUTATE | D-12 |
| Pertenencia para mutar desde el panel | — | `RackSiblingMembership.Classify` (Freeze I-55 V5 §4.1); RACKEDITAR conserva `FindRackBlocks` (§3.7) | D-11 |
| Identidad de rack y de vista | `RackEmbedDocument.Id`; sobre `(Id, View, Section)` y `RackViewCodec` | **sin cambio**: autoridades integradas (ADR-0009, ADR-0010, ADR-0042, ADR-0044) | MASTER-I63-I64-02 §3 |
| Inventario del documento, índice runtime y navegación | — | I-64, sobre autoridades integradas; sin fundación compartida | MASTER-I63-I64-02 §3, §4 y §6 |
| Métricas, providers, población y agregación | — | I-63 | MASTER-I63-I64-02 §3 |

No aparece una segunda autoridad persistente: el estado del panel nunca se persiste ni otro comando lo lee como autoridad (D-22).

## 3. Host, propiedad y documentos

### D-01 — Host: `PaletteSet`, foco y perfil

- **Host:**
  - un único `PaletteSet` por proceso (API de `acmgd.dll`, Discovery §9.1), que aloja con `AddVisual` un solo control raíz WPF de
    `RackCad.UI`;
  - se construye sin `toolID` y sin manejadores `Load`/`Save`; RackCad no pide a AutoCAD persistir estado de la paleta;
  - acoplable y flotante;
  - se muestra con un comando nuevo, `RACKPANEL` (nombre propuesto; el nombre visible lo elige el Owner y no bloquea);
  - cerrar la paleta la **oculta**: las sesiones siguen en memoria.
- **Alternativas:**
  - (b) `ShowModelessWindow`: contingencia si Smoke-1 muestra un fallo propio de `PaletteSet`; el cambio exige una A-n material con
    Architect y Coordinator;
  - (c) host híbrido: rechazado (duplica host, foco y ciclo de vida).
- **Obligación de foco congelada** (PV2-12, A64-PV1-10):
  - con el foco de teclado en un campo del panel, **ninguna** tecla ni atajo llega a AutoCAD: espacio, Enter, Esc, Supr, Ctrl+Z, Ctrl+C
    y Ctrl+V actúan solo sobre el campo;
  - el foco vuelve al dibujo solo por una acción explícita o por la política definida: al terminar Seleccionar, Ver, Centrar o Localizar
    vista; nunca al terminar una edición;
  - un comando de AutoCAD activo deshabilita las acciones del panel que leen o escriben el DWG.

  **No se congela la mecánica** (`KeepFocus`, manejo de `IKeyboardInputSink`, etc.). Una API interna de foco
  (`Autodesk.AutoCAD.Internal`, por ejemplo `SetFocusToDwgView`) solo se admite aislada en el adaptador de host, con degradación si falta y
  con evidencia registrada (O-10).
- **Perfil** (PV2-19, A64-PV1-19), antes de Smoke-1:
  1. acuse de I-52 sobre el procedimiento, el momento y si serían aceptables claves de estado de paleta;
  2. exportación de solo lectura del subárbol del registro del perfil activo de AutoCAD 2025, antes de abrir AutoCAD y después de
     cerrarlo, con un diff filtrado por las claves que nombren la paleta de RackCad; el resto de diferencias se registra como actividad
     normal de AutoCAD;
  3. si I-52 no acepta, o si la prueba exigiera modificar perfil, `TRUSTEDPATHS`, `SECURELOAD` o ACL: **STOP** al Owner o al CAD manager.

  Si AutoCAD escribe por su cuenta estado de una paleta sin `toolID` es `UNKNOWN`; lo resuelve ese diff.

### D-02 — Propiedad y ciclo de vida

- **Controlador** (hipótesis `WorkspaceController`): uno por proceso, en el Plugin. Se crea con el primer `RACKPANEL` y vive hasta que
  AutoCAD termina. Antes de ese momento hay cero suscripciones y cero trabajo (P-01).
- **Capas** (ADR-0006):
  - `RackCad.UI`: vistas y view-models WPF, sin referencias a AutoCAD (guarda existente `UiSystemBoundaryGuardTests`);
  - Application: estado puro (sesiones, pestañas, borradores, contexto de selección, máquinas de estado y políticas), probado sin AutoCAD;
  - Plugin: host de la paleta, puente de eventos, adaptadores de lectura y escritura del DWG y ejecución en contexto de comando.
- **Puerto:** la UI pide acciones a través de un puerto (hipótesis `IWorkspaceHostPort`) implementado en el Plugin. Recibe claves y
  valores; nunca `Document`, `Database`, `Transaction`, `DBObject` ni `ObjectId` (INV-14). Toda petición lleva la identidad de su sesión de
  documento (D-03).
- **Acceso al DWG:** solo en el Plugin, como una operación con el documento bloqueado y transacciones cuyo alcance es esa operación. Nada
  abierto sobrevive a la operación; en Debug se comprueba `TopTransaction == null` al terminar cada operación y cada drenaje (INV-14,
  PV2-17). Todas ocurren en reposo y sin modal RackCad activo:
  - las escrituras y las acciones que cambian la vista o la selección se ejecutan en contexto de comando;
  - las lecturas del drenaje (D-04) se hacen en contexto de aplicación, con el documento bloqueado;
  - si no se cumplen esas condiciones, las acciones del panel aparecen deshabilitadas.
- **Panel oculto:** cero lecturas del DWG; las pistas siguen encolándose con coste constante (P-01b). Al mostrar el panel se reconcilia en
  el siguiente reposo.
- **Sin documento abierto** (O-17): el panel muestra un estado vacío, no hay sesiones de documento, las acciones están deshabilitadas y el
  puente conserva solo las suscripciones de la colección de documentos.
- **Subdiálogos del panel** (O-13): se abren con la API modal de AutoCAD y la ventana principal de AutoCAD como dueña; mientras están
  abiertos cuentan como modal RackCad activo (D-04).
- No hay hilos de fondo que toquen AutoCAD.

### D-03 — Sesiones de documento e identidad

**Semántica congelada** (PV2-09, A64-PV1-03); la mecánica no se congela:
- Una sesión por **instancia abierta** de `Document`, durante toda su vida; nunca por ruta, nombre de archivo ni título.
- Dos aperturas del mismo archivo dan dos sesiones. `SAVEAS` conserva la sesión.
- La sesión se retira al destruirse el documento y **no puede** emparejarse con un documento posterior, aunque se recicle el identificador
  nativo.
- **Toda petición del panel y toda ejecución en contexto de comando** lleva la identidad de su sesión, y se rechaza sin escrituras ni
  cambios de selección o vista si el documento que la ejecuta es otro. Eso incluye «Abrir en editor clásico» (D-16).
- Contenido de la sesión: índice runtime, `SelectionContext`, pestañas (con su preview), pestaña activa y borradores.
- Vida: se crea de forma perezosa cuando el documento se activa con el panel ya creado; se retira en `DocumentToBeDestroyed`, momento en
  que el puente quita sus suscripciones; los borradores siguen D-17.
- Cambio de documento activo: el panel muestra la sesión del nuevo documento; las demás conservan su estado y no leen nada.
- Toda clave de rack es (sesión, RackId). El mismo GUID en dos documentos, o en dos aperturas del mismo archivo, da sesiones
  independientes. Nada cruza de una sesión a otra (INV-07).
- **Oráculos:** un identificador nativo reciclado tras destruir un documento da una sesión vacía; una petición nacida en la sesión A y
  ejecutada con B activo se rechaza sin efectos (INV-22).

## 4. Eventos, inventario y selección

### D-04 — Un puente central de eventos

- **Único suscriptor:** el puente (hipótesis `AutoCADEventBridge`, en el Plugin). Ninguna pestaña, editor, view-model ni adaptador se
  suscribe a eventos de AutoCAD (INV-02).
- **Fuentes** (Discovery §9.2), exactamente una suscripción por fuente:
  - colección de documentos: activación, creación y destrucción;
  - cada documento con sesión: `ImpliedSelectionChanged`, `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed`,
    `BeginDocumentClose` y, en su `Editor`, `EnteringQuiescentState`;
  - la base de datos de cada documento con sesión: `ObjectAppended`, `ObjectErased`, `ObjectModified`, `ObjectUnappended` y
    `ObjectReappended`;
  - `Application.Idle`, solo mientras haya pistas pendientes.
- **Ciclo de vida de las suscripciones** (PV2-16, A64-PV1-14): el número de suscripciones es exactamente función de los documentos con
  sesión, y cero para un documento destruido, en todos estos ciclos: `RACKPANEL` repetido, activar A→B→A, ocultar y mostrar, alta y baja
  dinámica de `Idle` y destrucción del documento.
- **Tres niveles sobre los modales** (PV2-10, A64-PV1-04):
  - **Hecho** (`RECONSTRUCTED`): la guía de Autodesk dice que no se disparan eventos durante un diálogo modal. No está medido en el host.
  - **Inferencia:** el panel probablemente queda inerte durante los modales de RackCad; lo comprueba S1-07.
  - **Obligación congelada:** **ninguna propiedad de corrección depende de que no haya eventos.** Las pistas pueden llegar en cualquier
    momento: durante un modal, entre rondas modales de un mismo comando (por ejemplo `RACKVARIABLES`, que escribe dentro de su bucle de
    rondas, `RackVariablesCommands.cs:47-76`) o durante la mutación propia.
- **Manejadores de notificación:** **solo encolan** (P-03). No leen ni escriben la base de datos, no piden datos al usuario, no abren
  diálogos ni llaman a resolvers. Filtran por tipo con coste constante y anotan una pista con la identidad de la sesión. Capturan y
  registran toda excepción y nunca la propagan a AutoCAD.
- **Drenaje** (P-04): diferido, en contexto de aplicación y con el documento bloqueado, y solo cuando el documento activo está en reposo
  **y** no hay modal RackCad activo. Coalesce: N pistas dan un solo refresco. Operaciones permitidas en el drenaje, y solo estas:
  1. leer la selección implícita del documento activo y los sobres de las definiciones seleccionadas (D-06);
  2. recalcular el contexto de selección;
  3. invalidar cachés de navegación y, desde F6, refrescar el índice;
  4. marcar borradores como «posiblemente obsoletos».

  Ninguna pestaña cambia mientras haya un modal RackCad activo. Oráculo: una fuente simulada emite pistas con un comando o un modal activo
  → quedan en cola y no se drenan hasta que se cumplen las condiciones (INV-23).
- **Pistas, no autoridad:** una pista solo invalida o marca. La obsolescencia la decide la relectura (D-12).
- **Undo/redo:** no hay un evento gestionado específico (Discovery §9.2). `CommandEnded` de `U`, `UNDO`, `REDO` y `MREDO`, junto con los
  eventos de objeto, invalidan y marcan. No bloquea, porque la autoridad es la relectura.
- **Reentrada:**
  - **estructural:** ningún evento ni cambio de contexto produce una escritura de selección o de vista (D-06, D-07);
  - **mutación propia** (O-15): el ámbito de mutación propia se indexa por (sesión, miembros del rack) y cubre desde PREPARE hasta la
    relectura de POST. Sus pistas se coalescen y no se tratan como cambio externo ni disparan otro Actualizar (INV-08). Un Actualizar del
    rack A no marca los borradores de otros racks;
  - ningún manejador ejecuta acciones que vuelvan a disparar el mismo evento.
- **Modo degradado** (O-07): una excepción en el puente pasa la sesión afectada a modo degradado, sin sincronización automática y con
  «Refrescar» manual, y retira las suscripciones de esa sesión.

### D-05 — Inventario runtime del documento (MASTER-I63-I64-02)

Reparto fijado por **MASTER-I63-I64-02**, que retira la fundación compartida de MASTER-I63-I64-01 (tabla de cláusulas en §19):

| Elemento | Dueño | En I-64 |
|---|---|---|
| Identidad de rack (RackId) y de vista (`View`, `Section`, `RackViewCodec`) | autoridades integradas vigentes (ADR-0009, ADR-0010, ADR-0042, ADR-0044) | las consume sin cambios |
| Pertenencia para mutar | autoridades integradas correspondientes (D-11) | las consume sin cambios; el índice no las sustituye |
| Authored persistido | el DWG, única autoridad persistida | sin cambio |
| Captura e índice runtime del documento: inventario, handles y ObjectIds, invalidación, navegación, selección, UI, pestañas y borradores | **I-64** | propio |
| Métricas, providers, población y agregación | I-63 | ninguno |

- **Sin dependencia de I-63:** ningún gate de I-64 espera la integración de I-63 (MASTER-I63-I64-02 §6). I-63 no consume código no
  integrado de I-64 (§7).
- **Hechos neutrales internos** (MASTER-I63-I64-02 §4): el índice puede usar una representación pura e interna, por definición, con:
  - clave estable de la definición (handle), RackId curado (sin Id = `IsNullOrWhiteSpace`, O-08), `Kind`, `View`, `Section` y `Name`;
  - recuento de referencias directas;
  - diagnósticos de captura: ilegible, sin Id, versión futura y Kind divergente entre miembros.

  **No** se declara fundación reutilizable ni contrato consumido por I-63. Convertirla en fundación común exigiría una decisión explícita
  nueva del Master (§5).
- **Captura sobre autoridades integradas:**
  - lectura de los sobres con la travesía integrada (`RackBlockFinder.ScanEnvelopes`, que ya omite layouts, anónimas y xref y devuelve
    también las definiciones cuyo sobre no se interpreta, con su recuento de referencias);
  - agrupación por RackId con la misma igualdad que la regla integrada (`OrdinalIgnoreCase`, como `RackListBuilder`);
  - nombre visible con la regla de presentación existente (primer nombre no vacío del grupo);
  - una definición ilegible solo se atribuye a un rack si el sondeo existente (`RackEnvelopeIdProbe`) da un Id; si no, queda como
    diagnóstico propio.
- **Reglas que se mantienen:** no inventar RackId; no materializar diseños ni resolver geometría o BOM para formar el índice; no retener la
  cadena `Design` más allá de la captura; no persistir el índice.
- **Separación respecto de las métricas:** el índice existe para navegación, selección, UI y sesión. No decide qué racks cuentan en
  ninguna métrica, no es autoridad de `ProjectSummary` ni de ID20 y no se ofrece como fuente a I-63. La duplicación con la enumeración
  propia de I-63 queda aceptada; revisarla corresponde a una decisión futura del Master.
- **Antes de F6** no hay enumeración global del dibujo:
  - el contexto de selección lee solo el sobre de la definición de cada referencia seleccionada, como hace hoy `PickRackBlock`, con una
    caché por definición que las pistas invalidan;
  - la navegación del rack en contexto o en pestaña (F2) obtiene sus objetivos de la pertenencia de D-11 en lectura, con una caché por
    (sesión, RackId) que las pistas invalidan.
- **En F6**, el navegador del documento (lista y búsqueda) se monta sobre el índice, con las decoraciones de navegación (referencias por
  vista en Model Space, orden y generación). Refresco: invalidar y refrescar bajo demanda, solo con el panel visible, el documento activo en
  reposo, sin modal RackCad activo y con el índice inválido. Se recaptura entero; un refresco incremental por definición es un punto de
  extensión.
- **Superficie futura:** `ProjectSummary` (ID20) podrá alojarse después en el panel como consumidor o superficie, sin bloquear a I-64
  (D-23).
- **Sobres sin Id** (O-08: «sin Id» es `IsNullOrWhiteSpace`): entrada de diagnóstico «bloque RackCad sin identidad», por definición; se
  puede ver y seleccionar, no editar en el panel; se ofrece el editor clásico.
- **Sobres ilegibles, de versión futura o divergentes:** diagnóstico visible, nunca ocultos ni contados como éxito (ADR-0044 §8).
- **Estado por rack** (brief, «status»), derivado y no persistido: Normal, Con borrador, Posiblemente obsoleto, Conflicto, Diagnóstico,
  No editable aquí, Error.

### D-06 — Sincronización AutoCAD → RackCad

- **Recorrido:** `ImpliedSelectionChanged` deja una pista; en el drenaje se lee la selección implícita, se prefiltran por clase las
  referencias de bloque sin abrir objetos, se lee la definición de cada una en una transacción corta y se obtiene su RackId por caché.
- **Contextos:** Ninguno; Uno (un RackId); Selección (N racks y M entidades que no son racks); Sin identidad; Diagnóstico.
- **Efectos:**
  - **Uno(R):** activa la pestaña de R si existe; si no, crea o reemplaza la preview (D-08). Si la pestaña activa tiene texto pendiente,
    esa pestaña sigue activa (ya está fijada) y la preview de R se crea o reemplaza en segundo plano, con un aviso;
  - **Selección (N):** vista agregada (D-09); nunca abre N pestañas;
  - **Ninguno:** el contexto se limpia; las pestañas no cambian.
- **Los eventos nunca causan escritura de selección ni de vista** (PV2-11).
- Durante un comando, la selección no se drena hasta el reposo.
- **Exclusiones de V1:** referencias anidadas, xref y espacio papel no se mapean; cuentan como «no rack».
- **Selección grande** (O-06): el trabajo es proporcional a las referencias seleccionadas; el detalle de Selección (N) se calcula por
  tramos en reposo, cancelable, sin transacción entre tramos y con un estado «calculando». Umbrales tras la línea base (D-18).
- **`PICKFIRST=0`:** no hay selección implícita; el panel lo indica. I-64 no cambia la variable.
- **Idempotencia:** el mismo conjunto seleccionado produce el mismo contexto, y un contexto igual al actual no navega.

### D-07 — Navegación RackCad → AutoCAD

- **Acciones explícitas:** Seleccionar, Ver, Centrar y Localizar vista, sobre el rack del contexto o de una pestaña (F2) y, desde F6, sobre
  el del navegador. Se ejecutan en el Plugin, en contexto de comando, en reposo y sin modal RackCad activo, ligadas a su sesión (D-03).
  Ninguna escribe el authored.
- **Objetivos:** referencias de nivel superior en Model Space de las definiciones miembro. Orden determinista: frontal, lateral, planta y el
  resto de vistas por el orden del codec; dentro de cada vista, por handle.

| Acción | Semántica |
|---|---|
| **Seleccionar** | Selección implícita con **todas** las referencias del rack en Model Space, copias incluidas. Si el espacio actual no es Model Space, no está disponible y se explica; no se cambia `TILEMODE` |
| **Ver** | Zoom a la extensión de la referencia principal (la primera del orden) con margen del 10 %, como `ZoomToRack` (Discovery §9.5); con varias copias, un control «copia k/M» recorre las demás |
| **Centrar** | Mueve el centro de la vista al centro de la referencia principal sin cambiar la escala |
| **Localizar vista** | Ver o Centrar sobre la primera referencia de una vista concreta, con el mismo recorrido de copias |

- **Limitación de V1:** se supone planta XY, como `ZoomToRack`.
- **Eco** (PV2-11, A64-PV1-09), congelado:
  - no existe ningún camino desde un evento o un cambio de contexto hacia una escritura de selección o de vista;
  - activar una pestaña nunca escribe la selección;
  - el eco de una acción del panel se reconoce **por contenido**: mientras la selección implícita de AutoCAD sea igual, como conjunto, a
    la que escribió la acción, el panel no cambia por ella ni la pestaña activa ni la preview ni la lista de Selección (N); puede actualizar
    el indicador de contexto. No se reconoce por «el siguiente evento»;
  - la primera selección distinta reanuda la sincronización normal.
  - **Oráculos:** con un contexto previo distinto (otro rack o Selección (N)), tras la acción y su eco hay cero llamadas de selección o de
    zoom y la pestaña activa, la preview y la lista no cambian; en Selección (N), «Seleccionar solo este rack» deja la lista visible.
- **`UNKNOWN` declarado** (A64-PV1-18): que la selección implícita escrita en contexto de comando sobreviva al terminar el comando interno.
  Lo comprueba Smoke-NAV.
- **Undo:** Ver y Centrar cambian la vista como un zoom (`INFERENCE`). Seleccionar no escribe nada que se pueda deshacer.
- **Foco:** al terminar la acción, el foco vuelve al dibujo (política de D-01).

## 5. Pestañas, multiselección y borradores

### D-08 — Pestañas, preview y carga perezosa

- **Registro ligero por pestaña:** RackId, nombre, Kind, preview o fijada, estado y referencia al borrador si existe. No retiene árbol WPF,
  sistema resuelto, BOM, objetos o transacciones de AutoCAD ni suscripciones.
- **Preview:** como máximo una por sesión. Un clic simple o una selección Uno(R) activa la pestaña de R si existe; si no, crea o reemplaza
  la preview.
- **La primera pulsación en un campo fija la preview** (PV2-14, A64-PV1-12), antes de cualquier compromiso interno; también la fijan «Editar»,
  el doble clic y «Fijar». Una pestaña con texto o con borrador dirty nunca es preview, así que nunca se reemplaza (INV-04).
- **Cerrar:** sin dirty se cierra sin preguntar; con dirty (incluido texto pendiente vivo) pregunta Descartar o Cancelar. Cerrar **nunca**
  escribe el DWG (INV-05, INV-09).
- **Marca dirty:** «Nombre *», que también cuenta el texto pendiente.
- **Materialización perezosa** (O-03): como máximo K superficies de editor vivas, con **K = 1** inicial, ajustable por A-n tras medir. Al
  dejar de estar materializada, una pestaña compromete su texto pendiente según D-10 y destruye su árbol; al volver, se reconstruye desde el
  borrador. Sistema resuelto, previews y BOM solo para las materializadas y bajo demanda.
- **Estado Error** (PV2-13): una pestaña cuya superficie falla queda en Error, conserva su borrador y ofrece Recargar o Descartar.
- **Sistema aún no integrado** (D-15): pestaña de resumen (identidad, vistas, estado y navegación) con «Abrir en editor clásico».

### D-09 — Multiselección

- **Contexto Selección (N):** lista virtualizada de racks (nombre, Kind, referencias seleccionadas y vistas) y recuento de entidades que no
  son racks, calculada por tramos (D-06). Cada fila permite abrir el rack (preview o fijada), seleccionar solo ese rack (con el eco de D-07)
  o verlo.
- Selección (N) nunca crea pestañas ni borradores.
- **Punto de extensión:** el contexto expone un conjunto estable de pares (sesión, RackId) para un futuro borrador por lotes (residual de
  ID3, D-20). No se implementa edición de propiedades comunes ni un Actualizar por lotes.

### D-10 — `DraftSession`: base, estado inicial, estado actual y dirty

- **Clave:** (sesión, RackId). Evoluciona `RackEditorSession`, `EditorPendingWork` y los estados de Application existentes (ADR-0029 D11):
  no hay modelo paralelo.
- **Componentes separados** (PV2-04, A64-PV1-08):

  | Componente | Contenido | Para qué sirve |
  |---|---|---|
  | **StaleBase** | atribución de pertenencia con identidad, `View` y `Section`; texto crudo exacto de cada sobre miembro interpretable; en Selectivo, texto crudo del registro de variables completo (D-12) | solo detectar cambios externos |
  | **InitialDraftState** | el estado editable que esta build materializa desde la misma lectura | referencia de dirty |
  | **CurrentDraftState** | el estado editable actual | lo que Actualizar escribe |
  | **Texto pendiente** | vivo en la superficie o conservado tras una frontera no bloqueable | cuenta para dirty, cierre y veto |
  | **Estado** | Limpio, Dirty, Posiblemente obsoleto, Conflicto, Aplicando, Desconocido, Error | UI y políticas |

- **Dirty** = CurrentDraftState ≠ InitialDraftState, **o** hay texto pendiente. No se calcula comparando JSON crudo: un rack guardado por
  otro build o con otro formato abre Limpio (INV-26). La mecánica (igualdad de estado o seguimiento de ediciones) no se congela.
- **Nacimiento de base y estado inicial** (PV2-05, A64-PV1-05):
  - la **preview de navegación** se muestra desde una lectura ligera del sobre de origen; no captura StaleBase ni ejecuta el barrido
    AUTH-06, y su superficie no acepta entrada hasta la intención de edición;
  - la **intención de edición** (primera pulsación, «Editar» o doble clic) dispara **una única lectura lógica**, con el documento bloqueado,
    que produce a la vez la StaleBase, la admisión (D-11) y el InitialDraftState;
  - si el sobre de origen de esa lectura difiere del que mostraba la preview, la superficie se re-renderiza desde la lectura nueva y la
    pulsación no se aplica, con un aviso («el rack cambió; se recargó»). Nunca se escribe sobre lo nuevo (INV-27);
  - si el documento no está en reposo o hay un modal RackCad activo, la intención espera, con la superficie en solo lectura y un aviso;
  - toda recarga sustituye StaleBase e InitialDraftState **a la vez**, desde una sola lectura.
- **Fronteras de compromiso interno** (PV2-14; complemento de ADR-0032 D6, anexo B):
  - **bloqueables**, iniciadas por el usuario dentro del panel (cambiar de pestaña o de rack, Actualizar): rige D6 tal cual; si un campo es
    inválido, la acción se aborta y el texto permanece en su caja;
  - **no bloqueables** (cambio de documento, ocultar el panel, desmaterializar por K, cierre de documento): el texto pendiente se conserva
    tal cual en el borrador y vuelve a su caja al rematerializar; **no** se compromete ni se auto-repara (INV-18);
  - la captura del texto la hace el host o el adaptador, sin cambiar los gestos de campo que comparte la ventana clásica (O-16).
- **Limpio con pista** (O-01): un borrador Limpio marcado «posiblemente obsoleto» se recarga de forma perezosa al activarse.
- **Vida:** desde la intención de edición hasta cerrar la pestaña (limpia o descartada), retirarse la sesión (D-17) o salir de AutoCAD.
  Solo en memoria.

## 6. Base, obsolescencia y Actualizar

### D-11 — Apertura, admisión y pertenencia

- **Regla de pertenencia** para la StaleBase y para escribir desde el panel: `RackSiblingMembership.Classify` sobre los hechos del barrido
  AUTH-06 (Freeze I-55 V5 §4.1): `BlockingUnreadable` bloquea; xref es `ReadOnly`; `MutableMembers` es el conjunto que se redibuja o borra.
  Es consumo sin cambios de una función integrada, y el Architect acordó que no hace falta elevarlo a quien gobierna I-55 (su respuesta a
  la pregunta 2 del paquete V1); se registra en el ADR propio.
- `FindRackBlocks` sigue gobernando el Actualizar de RACKEDITAR y el ejecutor de variables (I-55 V5 §3.7) y no se toca.
- **Origen de apertura:** la definición de la referencia elegida, si el usuario viene de una referencia; si no, la primera según el orden de
  D-07.
- **Admisión para editar**, evaluada en la lectura de la intención de edición:
  - AUTH-13 `Single` sobre los miembros de la compuerta authored;
  - Cama (comparador `Unreadable`, Discovery §9.7): solo con un único miembro mutable;
  - `Divergent`, `Unreadable`, `BlockingUnreadable` o un sobre sin Id: **No editable aquí**, con el diagnóstico visible, la entrada
    rechazada y el editor clásico disponible sin cambios (INV-31).
- **Coste:** el barrido AUTH-06 abre las entidades de todas las definiciones RackCad (`RackSiblingScan.cs:106-127`). Ocurre en la intención
  de edición, en Actualizar y, cacheado, en la primera navegación de un rack tras un cambio; nunca en reposo ni en una preview. Se mide en
  Smoke-NAV y Smoke-2. La distribución de admisión en los dibujos del Owner se mide en Smoke-2 (O-09, segunda parte).

### D-12 — Detección de obsolescencia

- **Autoridad:** una relectura **dentro de la transacción de MUTATE**, antes de escribir nada (precedente E de Discovery §9.7), comparada
  con la StaleBase:
  1. **pertenencia** (PV2-07, A64-PV1-07): por cada clave de definición del conjunto unión de la base y la relectura, su atribución
     **MUTABLE / READ_ONLY / BLOCKING / NOT_MEMBER** junto con `View` y `Section`. La división Redraw/Erase **no** forma parte de la base:
     se recalcula desde el CurrentDraftState al aplicar (en Selectivo, `eraseByKind` depende del diseño nuevo,
     `RackSelectivoInsertIntegration.cs:81-85`);
  2. el **texto crudo exacto** de cada sobre miembro interpretable;
  3. en **Selectivo**, el **texto crudo del registro de variables completo** (PV2-06, A64-PV1-06). **No** incluye las entradas globales de
     escaneo, que solo usa `BindingIntent` (`RackSelectivoCommands.cs:74`, `:105`, `:233`, `:386-407`). Acotar el registro a las
     variables que usa el rack no es seguro con expresiones transitivas.

  MATCH solo si todo es idéntico. Cualquier diferencia: **Conflicto y cero escrituras authored** (INV-06).
- **Comprobación previa** (O-04): PREPARE repite la comparación en solo lectura antes de importar nada, para no dejar residuo de biblioteca
  ante un Conflicto evidente. No sustituye a la relectura dentro de MUTATE.
- **Sin normalización:** los riesgos de Discovery §9.14 solo pueden dar conflictos falsos, nunca aceptaciones falsas. Con PV2-06 y PV2-07
  desaparecen los conflictos autoinfligidos (otro rack actualizado, quitar un fondo).
- **Alternativas de Discovery §9.7:** C (revisión persistida) rechazada por M-02; B (huella normalizada) rechazada por §9.14; A (AUTH-13
  tipado) solo como admisión (D-11); D (eventos) solo pista.
- **Comprobar ahora:** relectura de solo lectura que deja el borrador en Conflicto o confirma su estado.
- **Reconciliación explícita** (brief, «STALE / EXTERNAL CHANGE»):
  1. **Descartar y recargar:** StaleBase, InitialDraftState y CurrentDraftState se sustituyen desde una sola lectura nueva.
  2. **Conservar mi borrador sobre la versión actual:** tras una confirmación explícita, StaleBase e InitialDraftState se sustituyen desde
     una sola lectura nueva y el CurrentDraftState se conserva; el siguiente Actualizar compara contra esa base. En Selectivo, la
     reconciliación de vínculos usa el authored de la base nueva (Freeze I-48).

  Sin fusión automática.
- **H-LOCK, reformulada** (A64-PV1-01, hipótesis): la importación ocurre en PREPARE, antes de la transacción, y no es authored del rack. La
  relectura y todas las escrituras authored ocurren en la misma transacción de MUTATE, bajo el bloqueo de la operación y en contexto de
  comando, así que otro comando de RackCad no puede intercalar una escritura del mismo rack entre la comprobación y el commit. Quedan
  fuera: los manejadores de otros complementos que escriban durante la transacción (no se instala ninguno en los smokes, así que este caso
  queda `UNKNOWN`), la reentrada propia (cubierta por D-04) y POST, posterior al commit. Comprobación de host en Smoke-2 (§12.4).

### D-13 — Semántica de Actualizar desde el panel (M-04)

- **Resultado congelado** (PV2-03, A64-PV1-01): **todo o nada sobre el authored del rack**, es decir, las definiciones de vista, sus sobres y
  los borrados de fantasmas.
- **Residuo admitido:** las definiciones de biblioteca importadas en PREPARE pueden quedar y son purgables, como en I-55 V5 §4.6 OM-5. No
  son authored del rack ni cambian el significado persistido.
- **La mecánica no se congela.**
- **Vía implementable declarada:** la costura atómica integrada de I-55 (`ISiblingRedrawPort`, `RackSiblingRedrawRun`,
  `SiblingRedrawTransaction`; I-55 V5 §4.4-§4.10), con prueba física del Owner (OV-RED, `I-55-evidence.md:35`):
  1. **Compromiso interno** y construcción del sistema con el builder existente. Si falla, no hay escritura y el campo muestra el error.
  2. **Petición ligada a la sesión** (D-03): comando interno en contexto de comando, en reposo y sin modal RackCad activo. El adaptador
     toma el bloqueo del documento con `using` alrededor de PREPARE, MUTATE y POST.
  3. **PREPARE**, sin escrituras authored: comprobación previa de solo lectura (O-04); pertenencia y admisión (D-11); unidades por vista con
     escritores **caller-owned**, con Redraw/Erase recalculado desde el CurrentDraftState; capas bloqueadas (I-55 V5 §4.5; O-05);
     **importación de biblioteca, fuera de la transacción authored**; lista de piezas faltantes.
  4. **MUTATE:** **una** transacción poseída por el paso de mutación; dentro, primero la relectura y comparación (D-12) y la comprobación de
     destino (las vistas del plan siguen siendo las del dibujo, patrón `MutationDestinationBinding`); después, las unidades con escritores
     que no confirman ni abren transacción (`CallerOwnedFacadeGuardTests`); por último `Commit()`. Tras MUTATE, `TopTransaction == null`
     (INV-TX-1 de I-55 V5 §4.4).
  5. **POST**, fuera del núcleo atómico y según la autoridad existente (I-55 V5 §4.10): renombrado (`SyncName`), purga, un `Regen` y una
     relectura de la que salen a la vez la StaleBase y el InitialDraftState nuevos; CurrentDraftState := InitialDraftState; Limpio.
- **Clasificación por frontera, congelada** (PV2-03):

  | Frontera | Resultado | Escrituras authored | Borrador | Lo que ve el usuario |
  |---|---|---|---|---|
  | Fallo **antes de llamar a `Commit()`** | No aplicado — Conflicto, Admisión, Preflight o Fallo | ninguna; residuo de biblioteca posible | se conserva (Conflicto si aplica) | la causa y, en Conflicto, qué cambió |
  | **`Commit()` lanzó** | **Desconocido**, con relectura obligatoria | desconocidas hasta releer | estado Desconocido | «estado desconocido; comprueba» |
  | **`Commit()` retornó** | Aplicado | todas | rebasado y Limpio | las vistas redibujadas |
  | `Commit()` retornó y POST falló o hubo faltantes | **Aplicado con avisos** | todas | rebasado | avisos visibles en el panel, no solo en el registro |
  | `Commit()` retornó y la relectura de POST falló | Aplicado; borrador Desconocido | todas | Desconocido | «aplicado; recarga para seguir editando» |

  Desde Desconocido, «Comprobar ahora» relee: si coincide con la StaleBase anterior, el commit no ocurrió y el borrador vuelve a Dirty; si
  no, pasa a Conflicto. Nunca se supone aplicado.
- **Obligación de la vía:** la costura integrada convierte hoy cualquier excepción de `Mutate` en `Discarded`. El panel debe distinguir una
  excepción en la frontera de `Commit()` y clasificarla como Desconocido, **sin cambiar** el contrato observable de la costura para
  Insertar (cambio aditivo) (INV-30).
- **Cantilever y Cama:** no tienen escritor caller-owned. Cantilever confirma su propia transacción (`RackCantileverCommands.cs:158-168`);
  Cama redibuja con su propia transacción. Su integración en el panel exige antes, como obligación de su adaptador, una **variante
  caller-owned aditiva** (patrón I-47 G9.1) protegida por las guardas de fachadas caller-owned. Si no se crea, quedan en resumen + editor
  clásico, y ese residual lo decide el **Owner** (D-15, D-20).
- **Escritores caller-owned sin uso en producto** (riesgo R-09 del Architect): los de Dinámico, Push Back y Cabecera existen pero ningún
  camino de producto los usa. Su primera prueba física es Smoke-2 (Push Back) y, para los demás, la OV final.
- **Undo, congelado como observable** (O-11): **un Actualizar del panel es un solo paso de UNDO.** Lo comprueba Smoke-2. Si la prueba
  falla, la alternativa de mecanismo es un comando registrado lanzado con `SendStringToExecute`, mediante una A-n material.
- **Insertar y `BindingIntent`** no se ofrecen en el panel (§1).
- No puede haber un Actualizar mientras otro está en curso; la pestaña muestra el estado Aplicando.

### D-14 — Cambios con borradores dirty

- Cambiar de pestaña, de rack o de documento, ocultar el panel o cerrar una pestaña limpia **nunca** escribe el DWG (INV-05).
- Los borradores se conservan entre pestañas y entre sesiones de documento.
- Una pestaña con dirty o con texto pendiente nunca es preview ni se reemplaza (D-08), y cerrarla pregunta.
- Un Actualizar del rack A no deja en Conflicto ni marca el borrador independiente de B (INV-28).
- El cierre de un documento sigue D-17.

## 7. Editores y comandos existentes

### D-15 — Migración escalonada de los editores

- **Patrón:** migración escalonada sobre A y B de Discovery §9.6, con un **contrato de adaptador común** y adaptadores finos por sistema, en
  secuencia. No hay seis reescrituras paralelas.
- **Contrato de adaptador**, por sistema:
  1. **Superficie alojable:** el contenido del editor (el shell, que es un `Control`) se separa de su `Window`. La ventana clásica envuelve la
     misma superficie y conserva `Owner` = la ventana clásica y sus diccionarios de recursos.
  2. **Intención separada del cierre:** la superficie emite la intención de Actualizar hacia su host. El host clásico la interpreta como hoy
     (cerrar y `EditX`); el del panel, según D-13.
  3. **Ida y vuelta del estado:** guardar y restaurar CurrentDraftState y texto pendiente sin la ventana; materializar InitialDraftState
     desde la lectura de la intención de edición.
  4. **Diálogos con `Owner = this`:** el host decide el dueño (la ventana clásica, o la ventana principal de AutoCAD en el panel).
  5. **Escritor caller-owned** del sistema disponible (D-13).
- **Caracterización previa, por sistema** (PV2-18, A64-PV1-16; ADR-0029 D13 y D9): antes de extraer la superficie, pruebas que fijan en la
  ventana clásica: Enter, Escape, foco inicial, orden de tabulación, todos los caminos de cierre, `Owner` y `CenterOwner` de los
  subdiálogos, diccionarios de recursos, intención y escrituras del puerto. Siguen en verde después de la extracción (INV-19).
- **ADR-0029 D13** («subiniciativa con contrato propio») se cumple con un contrato de gate por sistema (O-14).
- **Criterio de entrada por sistema:** caracterización en verde, ida y vuelta completa y escritor caller-owned disponible.
- **Diferimiento:** si un sistema congelado en alcance no alcanza el criterio, su diferimiento es **`OWNER-RESERVED`** (PV2-20, A64-PV1-20;
  LIFECYCLE §3 y §8, READY-01). No es una A-n del Coordinator. El residual de ID3 lo enumera (D-20).
- **Orden propuesto:**
  - piloto en **F4: Push Back** (shell, sesión, estado en Application, `EditorPendingWork` y escritores caller-owned);
  - **F5-A:** Selectivo (sin `BindingIntent` en el panel), Dinámico y Cantilever (con su variante caller-owned);
  - **F5-B:** Cabecera (sin shell ni sesión: adaptador sobre su ViewModel) y Cama (sin shell, una vista, admisión con un único miembro y su
    variante caller-owned).
- No se reescribe lógica de dominio de ningún editor; el code-behind se traslada a la superficie.

### D-16 — Coexistencia con RACKEDITAR y los comandos clásicos

- **Sin cambio observable** en RACKEDITAR, los comandos de creación, RACKLISTA, RACKPROYECTAR, RACKDUPLICAR, RACKBOMTOTAL y RACKVARIABLES
  (INV-19). RACKEDITAR conserva `FindRackBlocks`, el commit por vista y sus mensajes.
- **Con un modal de RackCad abierto**, el drenaje está suprimido (D-04). Al terminar, las escrituras llegan como pistas: un borrador del
  mismo rack pasa a «posiblemente obsoleto» y su Actualizar dará Conflicto. Ninguna corrección depende de que el modal no emita eventos.
- **«Abrir en editor clásico»:** ejecuta, desde el panel y ligado a la sesión (D-03), el mismo flujo clásico sobre el origen de apertura.
  Si existe un borrador de ese rack, avisa antes de que el editor clásico parte del dibujo, no del borrador.
- **Sobres sin Id:** solo el editor clásico, que conserva su estampado de GUID; el panel no estampa (D-22).
- Si la extracción de superficies (D-15) cambiara algo observable de un comando clásico, sería M-03 y **STOP**.

### D-17 — Fallos, contención y cierre de documento

- **Contención de excepciones** (PV2-13, A64-PV1-11): toda entrada originada en el panel queda contenida en su frontera: eventos de las
  superficies alojadas, comandos de la UI, callbacks del dispatcher, materialización de pestañas y drenaje. Ninguna llega al bucle de
  mensajes de AutoCAD. La pestaña afectada pasa a Error y conserva su borrador (D-08). Oráculo de UI: una excepción en un manejador de una
  superficie alojada no se propaga (INV-25).
- **Excepción en el puente:** modo degradado de la sesión (D-04).
- **Error de host en una lectura:** la operación aborta su transacción y la UI muestra el error; no queda nada abierto.
- **Cierre de un documento con borradores dirty** (PV2-08, A64-PV1-02), incluido el texto pendiente conservado:
  - **todo** intento de cierre del documento (`CLOSE`, `CLOSEALL`, `QUIT`, cerrar la ventana) se **veta** mientras haya borradores dirty.
    `DocumentBeginCloseEventArgs.Veto` existe en `accoremgd.dll` (evidencia §13). Repetir el intento **no** es consentimiento;
  - el mensaje del veto, en la línea de comandos y en el panel, nombra las dos vías de descarte;
  - **acción explícita en el panel:** «Descartar borradores de este documento», con la lista de racks afectados;
  - **vía de comando**, para cuando el panel está oculto: un comando nuevo (nombre propuesto `RACKPANELDESCARTAR`; lo elige el Owner) que
    lista los racks y pide confirmación en la línea de comandos;
  - tras descartar, el cierre sigue normal, con el aviso de guardar propio de AutoCAD;
  - ningún diálogo se abre desde el manejador;
  - **si AutoCAD no respeta el veto** (`DocumentToBeDestroyed` con borradores dirty): la pérdida se hace visible en el panel, si sigue
    abierto en otra sesión, en la línea de comandos y en el registro; nunca es silenciosa;
  - que el veto cancele `QUIT` y su orden respecto del aviso de guardar son `INFERENCE`; los comprueba Smoke-2.
- **Caída de AutoCAD:** los borradores se pierden (solo memoria, aceptado por el brief).
- **Fallo al crear la paleta:** mensaje; los comandos clásicos siguen funcionando.

## 8. Rendimiento, persistencia y extensión

### D-18 — Rendimiento y carga perezosa

**Invariantes** (PV2-15, A64-PV1-13):

| ID | Invariante |
|---|---|
| P-01 | Panel **nunca abierto**: cero suscripciones; misma línea base que hoy |
| P-01b | Panel **oculto**: solo manejadores de coste constante que encolan; cero lecturas del DWG |
| P-02 | Panel visible y ocioso: cero llamadas a resolver, redibujo, `Regen`, recálculo, refresco del índice o lectura del DWG sin pistas pendientes |
| P-03 | Manejadores de notificación: solo encolan |
| P-04 | Drenaje diferido, en reposo, sin modal RackCad activo, coalescido y limitado a las operaciones de D-04 |
| P-05 | Índice (F6) refrescado bajo demanda y solo con el panel visible |
| P-06 | Como máximo K superficies de editor vivas; K = 1 inicial |
| P-07 | Nada de AutoCAD retenido, salvo valores `ObjectId` o handle dentro de su sesión |
| P-08 | Selección: trabajo proporcional a las referencias seleccionadas; Selección (N) por tramos |
| P-09 | Selección (N) virtualizada |

**Medición:**
- **Instrumento:** tiempos y memoria del proceso en el registro existente del Plugin (I-03), con un interruptor del panel en memoria, sin
  tocar configuración del host. El Worker lo implementa; el Owner ejecuta los escenarios y entrega los registros.
- **Umbrales:** ninguno antes de la línea base (brief). Después los fijan el Coordinator y el Owner con una A-n.

| # | Escenario | Gate | Cuándo |
|---|---|---|---|
| 1 | Panel nunca abierto (línea base) | F1 | Smoke-1 |
| 2 | Panel abierto y ocioso | F1 | Smoke-1 |
| 3 | Latencia de selección de un rack | F1 | Smoke-1 y OV final |
| 4 | Cambio de rack | F3 | OV final |
| 5 | Cambio de pestaña | F3, F5 | Smoke-2 y OV final |
| 6 | 10 pestañas ligeras abiertas | F3 | OV final |
| 7 | Inventario grande (captura completa del índice del documento) | F6 | DWG reales del Owner y OV final |
| 8 | Selección grande en AutoCAD | F6 | OV final |
| 9 | Cambio de documento activo | F1 | Smoke-1 y OV final |
| 10 | **Comando masivo** de ≥ 10⁴ objetos (por ejemplo COPY o ARRAY, o un Actualizar grande) con el panel **nunca abierto, oculto y visible** | F1 | Smoke-1 y OV final |

La navegación (coste de la primera acción sobre un rack con la caché de D-05 fría) se mide en Smoke-NAV.

### D-19 — Hitos del Owner y matriz OV

Desarrollado en §12.

### D-20 — Fronteras de cierre de ID3

- **ID3 Foundation** es alcance de esta iniciativa:

  | Elemento | Decisión | Gate |
  |---|---|---|
  | Navegar entre varios racks | D-06..D-09; navegador en F6 | F2, F3, F6 |
  | Conservar borradores | D-10, D-14 | F4 |
  | Sesión de edición persistente | D-08, D-10 | F3, F4 |

- **Residual exacto de ID3** (PV2-20, A64-PV1-20): la edición por lotes del brief (propiedades comunes, validación conjunta y mutación
  atómica de varios racks) **y** todo sistema que no quede integrado en el panel.
- **Clasificación esperada:** `PARTIAL`, con ese residual. Los puntos de extensión que lo preparan son el conjunto estable de Selección (N)
  (D-09) y los borradores por RackId (D-10). F6 entrega la evaluación de ID3; la clasificación definitiva se decide al cierre y la acepta el
  Owner.

### D-21 — ADR propio y complementos

- **ADR propio** (borrador en el anexo A): el archivo nace `propuesto` en la primera tarea de F1, tras el Freeze y antes de implementar
  ([WORKFLOW](../WORKFLOW.md) §8 y §11.4); su número se asigna entonces contra `origin/main` y las ramas activas; solo el Owner lo acepta.
- **ADR-0010:** se **complementa**, como hizo ADR-0042: vía del panel, base vigente, todo o nada, sin Insertar. RACKEDITAR conserva íntegras
  sus decisiones.
- **ADR-0032 D6:** se **complementa** con las fronteras del panel; D5 y D6 no cambian.
- **Forma:** los ADR aceptados son inmutables y admiten una nota posterior fechada ([adr/README](../adr/README.md)). Las dos notas
  (anexo B) se escriben al aceptarse el ADR nuevo, en el commit de cierre.
- **ADR-0029:** sin modificación. El ADR nuevo extiende a las superficies alojadas D7, D8 y D11 y, para la ventana clásica envolvente, D9
  y D13; el censo D1 no cambia.
- **ADR-0006, ADR-0009, ADR-0019 y ADR-0044:** se consumen sin cambios.

### D-22 — Persistencia y frontera de autoridad

- El panel no escribe nada en el DWG fuera de Actualizar, y Actualizar usa los builders, escritores y `Compose` existentes, con
  preservación por vista (INV-10).
- No hay campos persistidos nuevos, ni escrituras en el NOD ni XData del panel, ni archivos aparte del registro, ni escrituras en el
  registro de Windows o el perfil desde el código de RackCad (INV-21).
- El panel **no estampa** identidad en sobres sin Id y **no inventa** RackId (INV-16).

### D-23 — Puntos de extensión preparados, no implementados

- Regiones de inspector en la pestaña y en Selección (N), para el resumen de ID20, ID28, ID29, propiedades personalizadas, variables de
  proyecto y diagnósticos.
- Borrador por lotes (residual de ID3).
- Insertar y `BindingIntent` desde el panel.
- Restauración entre arranques.
- Índice incremental.
- K > 1 superficies materializadas.

Ninguno se implementa aquí.

## 9. Comparación con el estado actual (resumen)

| Aspecto | Hoy (Discovery) | Con el panel |
|---|---|---|
| Ventanas | modales, 20 `ShowModalWindow` | panel modeless + los modales clásicos sin cambio |
| Eventos | ninguno | un puente; los manejadores solo encolan; drenaje en reposo |
| Borrador | vive y muere con la ventana | vive por (sesión, RackId) hasta cerrarlo |
| Dirty | ámbito en 2 de 6 editores | estado inicial de esta build + texto pendiente |
| Comprobación de base | ninguna | relectura cruda en la transacción de MUTATE |
| Atomicidad de Actualizar | por vista | todo o nada en la vía del panel, sobre la costura de I-55 |
| Faltantes de biblioteca | invisibles en Actualizar | visibles en la vía del panel |
| Sobres sin Id | Actualizar estampa GUID | el panel no los edita; el editor clásico conserva su conducta |

## 10. Invariantes y obligaciones de prueba

Ninguna se ha ejecutado. «RED esperado» describe el fallo que la prueba debe mostrar antes de implementar el comportamiento (LIFECYCLE §7,
punto 2). «Simulado» significa una fuente de eventos, un lector o un escritor falsos, sin AutoCAD. Las comprobaciones de host no sustituyen
a las de Core/UI ni al revés.

| ID | Invariante | Prueba (clase) | RED esperado | Gate |
|---|---|---|---|---|
| INV-01 | Panel visible y ocioso: cero trabajo sin pistas (P-02) | Core con puente simulado: eventos ajenos y reposos sin pistas → cero lecturas, resolver y `Regen` | un drenaje por cada reposo cuenta lecturas | F1 |
| INV-02 | Suscripciones = función exacta de las sesiones; ninguna por pestaña o editor (PV2-16) | Core/UI con fuente simulada: `RACKPANEL` repetido, A→B→A, ocultar/mostrar, alta y baja de `Idle`, destrucción (cero), 10 pestañas; guarda de fuente: `+=` sobre eventos de AutoCAD solo en la carpeta del puente | el recuento crece en algún ciclo | F1, F3 |
| INV-03 | Ningún evento ni cambio de contexto escribe selección o vista; activar una pestaña no escribe selección; eco por contenido sin cambios de pestaña, preview ni lista (PV2-11) | Core: contexto previo distinto + acción + eco → cero llamadas de selección o zoom y sin cambios; caso de la fila de Selección (N) | un contexto que reescribe o un eco por «siguiente evento» | F1, F2 |
| INV-04 | Una pestaña con texto o dirty nunca es preview ni se reemplaza; la primera pulsación fija (PV2-14) | UI (STA): preview + texto (incluso inválido) + otra selección → el texto sigue y la pestaña queda fijada | la preview se reemplaza o el texto se pierde | F3, F4 |
| INV-05 | Cambiar de pestaña, rack o documento, u ocultar el panel, no escribe | UI/Core con puerto simulado: el cambio ocurre y hay cero escrituras | la aserción sobre la pestaña activa falla mientras el cambio no exista | F4 |
| INV-06 | Un borrador obsoleto no sobrescribe | Core con lector y escritor simulados: diferencia en atribución, en un sobre o en el registro → No aplicado — Conflicto, cero escrituras | sin compuerta, se escribe | F4 |
| INV-07 | Sesiones aisladas, también con el mismo archivo abierto dos veces | Core: el mismo RackId en dos sesiones → borradores, pestañas y contextos independientes | una clave por RackId o por ruta mezcla sesiones | F1, F4 |
| INV-08 | Sin reentrada en el Actualizar propio; ámbito por (sesión, miembros) de PREPARE a POST | Core: pistas durante la mutación propia → un único refresco, sin segundo Actualizar ni conflicto | sin ámbito, la pista se trata como cambio externo | F4 |
| INV-09 | Cerrar sin intención no escribe | existente: `RichEditorCloseContractTests.NoCloseRouteMaterialisesAnything`; nuevas para pestaña y panel | las nuevas fallan mientras no exista el cierre de pestaña | F3, F5 |
| INV-10 | Desconocidos y metadatos por vista preservados en el Actualizar del panel | Core: sobres con `ExtensionData` distintos por vista → cada vista conserva los suyos | un recorrido que reutiliza el sobre elegido | F4, F5 |
| INV-11 | Clasificación de resultados de D-13 en Core | Core con escritor simulado: fallo en PREPARE, antes de `Commit()`, en `Commit()`, en POST y en la relectura | un resultado binario | F4 |
| INV-12 | Todo o nada en el authored de la vía del panel | Core con escritor transaccional simulado: fallo en la unidad k → cero escrituras authored; host: S2-09 | con commit por vista quedan vistas escritas | F4 |
| INV-13 | La UI no referencia AutoCAD (ADR-0006) | existente: `UiSystemBoundaryGuardTests`, ampliada a las carpetas nuevas | guarda existente; sigue en verde | F1 |
| INV-14 | Ninguna transacción, bloqueo ni objeto de AutoCAD sobrevive a su operación (PV2-17) | guarda de fuente en las carpetas del panel del Plugin: `StartTransaction`, `StartOpenCloseTransaction` y `LockDocument` solo dentro de `using`; ningún campo de tipo `Transaction`, `DBObject` ni `Document`; el puerto sin tipos de AutoCAD; comprobación Debug de `TopTransaction == null` al terminar cada operación y drenaje, registrada en los smokes | la guarda falla ante un campo o un `StartTransaction` sin `using` | F1 |
| INV-15 | Como máximo K superficies materializadas (K = 1) | UI: N pestañas → K instancias de superficie | una implementación que conserva los árboles | F3, F5 |
| INV-16 | El panel no estampa identidad ni inventa RackId | Core: sobre sin Id (incluido Id en blanco) → diagnóstico, no editable, sin clave RackId | el sobre desaparece, recibe clave o `Classify` lanza | F1, F4 |
| INV-17 | Lo ilegible, divergente o de versión futura nunca se oculta ni cuenta como éxito | Core: cada caso → diagnóstico visible y no editable | un filtro que lo omite | F1, F4 |
| INV-18 | Fronteras bloqueables según ADR-0032 D6; las no bloqueables conservan el texto sin comprometer ni auto-reparar | UI (STA): campo inválido + cambio de documento u ocultar → el texto vuelve a su caja; + cambio de pestaña → la acción se aborta | el texto se pierde, se compromete o se auto-repara | F4, F5 |
| INV-19 | Comandos clásicos sin cambio observable (ADR-0029 D13 y D9 completos; PV2-18) | caracterización por sistema antes de extraer: Enter, Escape, foco inicial, tabulación, caminos de cierre, `Owner`/`CenterOwner`, recursos, intención y escrituras del puerto | caracterización: en verde antes y después | F4, F5 |
| INV-20 | El cierre de un documento con borradores dirty se veta en **todo** intento hasta el descarte explícito; una pérdida no vetada es visible (PV2-08) | Core: política de veto en intentos sucesivos (CLOSE, CLOSEALL, QUIT) → vetados; descarte explícito → el siguiente cierre procede; destrucción sin veto → aviso de pérdida; host: S2-06 | con veto de un solo intento, el segundo descarta | F4 |
| INV-21 | El panel no persiste nada fuera de Actualizar | guarda de fuente: el puerto no expone escrituras salvo Actualizar y el descarte de borradores; host: diff de perfil de Smoke-1 | la guarda falla si el puerto expone otra escritura | F1, F4 |
| INV-22 | Toda petición y ejecución va ligada a su sesión; identificador reciclado = sesión vacía (PV2-09) | Core: petición de A ejecutada con B activo → rechazada sin efectos; documento destruido y nuevo con el mismo identificador → sesión vacía | una petición sin identidad se ejecuta en el documento activo | F1, F2 |
| INV-23 | Las pistas pueden llegar en cualquier momento y no se drenan con comando o modal RackCad activo (PV2-10) | Core con fuente simulada: pistas durante un modal o un comando → en cola, sin drenar; al cumplirse las condiciones, un drenaje | se drenan en cuanto llegan | F1 |
| INV-24 | Con el foco en un campo, ninguna tecla ni atajo llega a AutoCAD; el foco vuelve solo según la política (PV2-12) | solo host: S1-06a..S1-06h | — (host; sin prueba automatizada posible en Core) | F1 |
| INV-25 | Toda excepción originada en el panel queda contenida; la pestaña pasa a Error con su borrador (PV2-13) | UI: excepción en un manejador de una superficie alojada y en la materialización → no se propaga; estado Error; borrador intacto | la excepción escapa del panel | F1, F4 |
| INV-26 | Dirty no depende de JSON crudo (PV2-04) | Core/UI: fixture con el JSON de otro formato o build → abre Limpio | un dirty calculado contra el texto crudo | F4 |
| INV-27 | Base y estado inicial nacen de la misma lectura; un cambio entre la preview y la primera edición recarga y nunca sobrescribe (PV2-05) | Core con lector simulado: sobre cambiado entre la lectura de la preview y la intención → re-render, pulsación no aplicada, base e inicial de la lectura nueva | base y estado inicial de lecturas distintas | F4 |
| INV-28 | Un Actualizar del panel sobre A no deja en Conflicto ni marca el borrador de B (PV2-06) | Core con dos borradores Selectivo: Actualizar A → B sigue MATCH; COPY o ERASE de una referencia de otro rack → B sigue MATCH | las entradas de escaneo en la base dan Conflicto | F4, F5 |
| INV-29 | Quitar un fondo o una variante en el borrador no causa Conflicto (PV2-07) | Core: borrador sin un fondo → MATCH y borrado del fantasma | Redraw/Erase en la base da Conflicto autoinfligido | F4, F5 |
| INV-30 | Clasificación por frontera de `Commit()` sin cambiar la costura para Insertar (PV2-03) | Core: excepción antes de `Commit()` → No aplicado; en `Commit()` → Desconocido con relectura; pruebas existentes de la costura en verde | la costura clasifica una excepción de `Commit()` como `Discarded` | F4 |
| INV-31 | Resultados visibles: Aplicado con avisos, No editable aquí y Desconocido se muestran en la UI (PV2-22) | UI: el view-model muestra avisos, el diagnóstico de admisión con el editor clásico ofrecido y el estado Desconocido con «Comprobar ahora» | avisos solo en el registro | F4 |
| INV-32 | Un Actualizar del panel es un solo paso de UNDO (O-11) | solo host: S2-05 | — (host) | F4 |
| INV-33 | Panel oculto: cero lecturas del DWG (P-01b) | Core con puente simulado: panel oculto + pistas → cero lecturas; al mostrar, una reconciliación | se lee con el panel oculto | F1 |
| INV-34 | Selección (N) nunca crea pestañas ni borradores | UI: selección de N racks → contexto agregado, cero pestañas y cero borradores nuevos | una implementación que abre una pestaña por rack | F6 |
| INV-35 | La captura del inventario no materializa diseños, no resuelve, no retiene `Design` ni inventa RackId; un sobre ilegible solo se atribuye por el sondeo existente (MASTER-I63-I64-02) | Core con lector simulado: captura de N definiciones → cero llamadas a stores de kind y resolvers; hechos sin `Design`; ilegible sin Id sondeable → diagnóstico propio | la captura deserializa el diseño o asigna un Id | F6 |

## 11. Plan de gates

Plan confirmado por el Coordinator en la orden de V3, ajustado a [LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7: cada gate tiene un resultado
observable y verificable; no hay micro-gates. **Ningún gate depende de I-63** (MASTER-I63-I64-02 §6). Respecto del brief, la navegación
del rack en contexto sube a **F2** y el inventario del documento con Selección (N) y la evaluación de ID3 quedan en **F6**.

| Gate | Resultado verificable | Entra con | Cierra con | Hito del Owner |
|---|---|---|---|---|
| **F1** — Workspace Foundation | `RACKPANEL`; contexto del documento activo; selección fina (Ninguno, Uno, Selección (N) con recuento, Sin identidad, Diagnóstico) sin enumeración global; puente único con encolado y drenaje; identidad de sesión; política de foco; contención; instrumentación; ADR `propuesto`; procedimiento de diff de perfil | Freeze AGREED; estado `I-64.yml`; precondiciones de §13 | INV-01, 02, 03 (lado de eventos), 07, 13, 14, 16, 17, 21, 22, 23, 25 (panel) y 33; CI exacta; revisión del Coordinator | **Smoke-1** |
| **F2** — Context Navigation | AutoCAD ↔ RackCad para el rack del contexto o de la pestaña: Seleccionar, Ver, Centrar y Localizar vista (objetivos desde D-11 en lectura, con caché); eco por contenido; vuelta del foco | F1 | INV-03 completo e INV-22; pruebas de UI de las acciones | **Smoke-NAV** |
| **F3** — Browser Session | pestañas preview y fijadas, cierre sin escritura, registros ligeros, K superficies (resumen) | F1 | INV-02 con pestañas, la regla de preview de INV-04, INV-09 e INV-15; escenarios 4 y 6 | — |
| **F4** — Borradores y Actualizar, con piloto | `DraftSession` separada; dirty; cambios sin escritura; obsolescencia; Actualizar todo o nada sobre la costura de I-55; reconciliación; veto de cierre y descarte; **Push Back editable en el panel** | F3, **Smoke-1 PASS** y **Smoke-NAV PASS** | ver la secuencia de cierre abajo | **Smoke-2** |
| **F5-A** — Editores con shell | Selectivo, Dinámico y Cantilever (con su variante caller-owned) editables en el panel | **F4 PASS** | por sistema: caracterización, ida y vuelta, INV-10, 18, 19, 28 y 29 | — |
| **F5-B** — Editores sin shell | Cabecera y Cama (con su variante caller-owned) editables en el panel | F5-A | lo mismo que F5-A | — |
| **F6** — Document-wide Inventory, Selección (N) e ID3 | inventario del documento de I-64 sobre autoridades integradas (D-05); navegador (lista y búsqueda) con Kind, vistas y estado; invalidación y refresco bajo demanda; diagnósticos; Selección (N) completa por tramos; evaluación de ID3 | F2 y F3 | INV-16 e INV-17 en el índice, INV-34 e INV-35; escenarios 7 y 8; informe de ID3 | — |
| **F7** — Rendimiento y regresión | escenarios 1-10 caracterizados; regresión; guía de validación manual actualizada (último gate de implementación, WORKFLOW §11.4); matriz OV preparada | F1..F6 | mediciones; evidencia exigida por AGENTS | — |
| **READY** | READY-01..09 (LIFECYCLE §8) | F7 | `FINAL_CANDIDATE_SHA` | **OV final** |

- **Secuencia de cierre de F4** (PV2-01, A64-PV1-17): implementación → **Candidato F4** (suite de UI completa en local y CI del SHA
  exacto) → evidencia automatizada (INV-04..08, 10..12, 16..20, 25..31) → **Smoke-2**, que incluye las comprobaciones de host de rollback,
  H-LOCK, undo y `QUIT` (el Worker prepara el procedimiento y el Owner ejecuta AutoCAD) → revisión del Coordinator → **F4 PASS**.
- **Sin dependencia externa de producto:** ningún gate espera a I-63 ni a otra iniciativa (MASTER-I63-I64-02 §6). Se conserva la regla
  general de A64-PV1-21: no hay cierre ni integración parcial de la iniciativa por resecuenciación; una integración parcial exige decisión
  del Owner.
- **Smoke-1** es condición de entrada de F4. F2 y F3, que no escriben el DWG, pueden avanzar mientras se espera la ventana de host.
  **Smoke-NAV** es condición de entrada de F4 (y por tanto de Smoke-2). Si el Coordinator lo autoriza, Smoke-1 y Smoke-NAV pueden ejecutarse
  en una misma sesión de host sobre un Candidato que contenga F2.
- **Caracterización antes de migrar** (ADR-0029 D13): en F4 para el piloto y en F5 para cada sistema.
- **Conformidad final:** READY-06, no un gate.

## 12. Hitos del Owner y matriz OV (D-19)

### 12.1 Reglas comunes

Fijadas por MASTER-I63-I64-01, cláusulas que MASTER-I63-I64-02 no modifica (§19), con la petición de I-52 (evidencia §13):
- en la **estación principal del Owner**, con AutoCAD 2025;
- solo después de cerrar el gate correspondiente y de un **RELEASE explícito de la ventana de host de I-52**; antes, I-64 avisa a I-52 por el
  canal entre sesiones y no se solapa con sus sesiones;
- **una sola sesión de AutoCAD**;
- I-64 **no cambia** perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL ni la configuración de I-52, ni instala plugins;
- **si cargar el DLL o cualquier comprobación exige modificar la seguridad o el perfil, STOP** al Owner o al CAD manager;
- cada hito es intermedio y necesita su propio Candidato completo: suite de UI completa en local y CI del SHA exacto
  ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §8; [guía](../guias/validacion-manual-autocad.md) §2); no sustituye al `FINAL_CANDIDATE_SHA`;
- se carga el DLL Debug del worktree de I-64, con su SHA-256 registrado (guía §2 y §3);
- las **inyecciones de fallo** son solo Debug y se activan con una variable de entorno **del proceso**, poniéndola al lanzar AutoCAD desde una
  consola y retirándola después; ningún cambio persistente. Se acusan con I-52 antes del hito;
- el Worker nunca usa AutoCAD: prepara el procedimiento y el Owner lo ejecuta;
- en todos los hitos se registra la comprobación Debug `TopTransaction == null` (INV-14).

### 12.2 Smoke-1 (tras F1)

**Antes de abrir AutoCAD:** acuse de I-52 y exportación de solo lectura del subárbol del perfil (D-01).

| ID | Comprobación |
|---|---|
| S1-01 | El panel queda abierto mientras se dibuja |
| S1-02 | Zoom y encuadre funcionan con el panel abierto |
| S1-03 | Los comandos normales funcionan (por ejemplo LINE, MOVE, COPY, ERASE, UNDO) |
| S1-04 | Seleccionar un rack en el dibujo lo refleja en el panel; varios dan Selección (N) con recuento; un bloque sin identidad o ilegible da su diagnóstico |
| S1-05 | Cambiar de documento cambia el contexto sin mezclarlo y al volver se recupera; el mismo archivo abierto dos veces da dos contextos |
| S1-06a | Escribir texto con espacios en un campo del panel con el cursor sobre el dibujo: nada llega a la línea de comandos |
| S1-06b | Ctrl+Z en un campo deshace el texto, no ejecuta UNDO de AutoCAD |
| S1-06c | Ctrl+C y Ctrl+V en un campo copian y pegan texto, no objetos |
| S1-06d | Supr en un campo, con una selección implícita activa en el dibujo, no borra objetos |
| S1-06e | Esc y Enter en un campo no cancelan ni repiten comandos de AutoCAD |
| S1-06f | Con un comando de AutoCAD activo, las acciones del panel que leen o escriben el DWG están deshabilitadas |
| S1-06g | El foco vuelve al dibujo solo según la política de D-01 |
| S1-06h | En todos los casos S1-06a..g, el bit 1 de `DBMOD` no cambia |
| S1-07 | Con RACKEDITAR abierto el panel no interfiere, y al cerrarlo sigue sincronizado |
| S1-08 | Dibujo recién abierto, `DBMOD` leído antes de cualquier comando de dibujo: abrir, usar y cerrar el panel sin Actualizar no cambia su bit 1 (O-12) |
| S1-09 | Panel ocioso: el registro no muestra lecturas, resolver ni `Regen` (escenarios 1, 2, 3 y 9) |
| S1-10 | Escenario 10: comando masivo con el panel nunca abierto, oculto y visible |
| S1-11 | **Después de cerrar AutoCAD:** segunda exportación del perfil y diff filtrado; cualquier clave de la paleta se informa a I-52 y al Owner |
| S1-12 | Con `PICKFIRST=0`, el panel explica que no hay sincronización (solo si el Owner ya la tiene así; no se cambia) |

### 12.3 Smoke-NAV (tras F2)

| ID | Comprobación |
|---|---|
| SN-01 | Seleccionar deja seleccionadas todas las referencias del rack en Model Space, copias incluidas, y la selección sobrevive al terminar el comando interno (`UNKNOWN` de D-07) |
| SN-02 | En un layout, Seleccionar no está disponible y lo explica, sin cambiar de espacio |
| SN-03 | Ver hace zoom a la referencia principal con margen; «copia k/M» recorre las copias |
| SN-04 | Centrar centra sin cambiar la escala |
| SN-05 | Localizar vista va a la vista pedida |
| SN-06 | Eco: con otra pestaña activa o con Selección (N), tras Seleccionar no cambian la pestaña activa, la preview ni la lista, y no hay navegación secundaria |
| SN-07 | El foco vuelve al dibujo al terminar cada acción |
| SN-08 | Una petición iniciada en un documento no se ejecuta en otro (cambiar de documento antes de que se ejecute) |
| SN-09 | Coste de la primera acción sobre un rack con la caché fría y de las siguientes |

### 12.4 Smoke-2 (tras F4, con el piloto Push Back)

| ID | Comprobación |
|---|---|
| S2-01 | Editar en el panel deja el borrador dirty («Nombre *») y no toca el dibujo; la primera pulsación fija la preview |
| S2-02 | Cambiar de pestaña y de documento conserva el borrador y el texto pendiente |
| S2-03 | Actualizar redibuja todas las vistas y deja el borrador Limpio |
| S2-04 | Un cambio externo (RACKEDITAR sobre el mismo rack) da Conflicto y cero escrituras; las dos opciones de reconciliación funcionan |
| S2-05 | Un Actualizar del panel es un solo paso de UNDO; deshacerlo revierte el dibujo y el siguiente Actualizar da Conflicto |
| S2-06 | Cerrar el documento (CLOSE, CLOSEALL y QUIT) con un borrador dirty se veta en todos los intentos; tras «Descartar borradores» o `RACKPANELDESCARTAR`, el cierre sigue con el aviso de guardar de AutoCAD |
| S2-07 | RACKEDITAR de Push Back sigue igual que antes |
| S2-08 | Escenario 5 (cambio de pestaña) |
| S2-09 | **Rollback:** con la inyección Debug de fallo en la unidad 2 (`RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT=2`, la de OV-RED-06, o la equivalente de la vía del panel) → No aplicado; ninguna vista cambió; solo puede quedar biblioteca importada |
| S2-10 | **Desconocido:** con la inyección Debug en la frontera de `Commit()` o en la relectura de POST → estado Desconocido; «Comprobar ahora» lo resuelve sin suponer aplicado |
| S2-11 | **Aplicado con avisos:** con una pieza de biblioteca faltante real o con la inyección Debug en POST → avisos visibles en el panel |
| S2-12 | **No editable aquí:** un rack con sobre sin Id, divergente o Cama con dos miembros (dibujo aportado por el Owner) → diagnóstico y editor clásico ofrecido |
| S2-13 | **H-LOCK:** un comando de RackCad tecleado durante un Actualizar del panel no se intercala; el caso de otros complementos queda `UNKNOWN` |
| S2-14 | Un Actualizar del rack A no deja en Conflicto el borrador de B |
| S2-15 | Distribución de admisión en los dibujos del Owner (recuento del registro) |

Las inyecciones Debug que no existan todavía (frontera de `Commit()`, relectura y POST) se crean en F4, solo en Debug, con el patrón de
`SiblingRedrawDebugFaultInjection`; sus nombres se fijan en F4.

### 12.5 Matriz OV final (sobre `FINAL_CANDIDATE_SHA`)

Todos los escenarios se asignan a la unidad **I-64** (única unidad) y se suman al checklist de la guía (LIFECYCLE §8).

| OV | Escenario | Hito previo |
|---|---|---|
| OV-01 | Mantener el panel abierto mientras se dibuja | S1-01 |
| OV-02 | Zoom y encuadre | S1-02 |
| OV-03 | Comandos normales de AutoCAD | S1-03 |
| OV-04 | Seleccionar un rack en el dibujo y que el panel lo siga | S1-04 |
| OV-05 | Elegir un rack en el navegador del documento | — (F6) |
| OV-06 | Seleccionar, ver, centrar y localizar vistas, con el eco sin navegación secundaria | SN-01..SN-07 |
| OV-07 | Varios sistemas (los seis, o los que consten en el residual decidido por el Owner) | S2 (Push Back) |
| OV-08 | Varias pestañas | — |
| OV-09 | Preview y fijado | S2-01 |
| OV-10 | Borrador dirty | S2-01 |
| OV-11 | Cambiar de pestaña sin escribir | S2-02 |
| OV-12 | Actualizar escribe | S2-03 |
| OV-13 | Conflicto por cambio externo | S2-04 |
| OV-14 | Multiselección: Selección (N) sin abrir pestañas | S1-04 |
| OV-15 | Varios documentos, incluido el mismo archivo dos veces | S1-05 |
| OV-16 | Guardar y reabrir: el dibujo conserva lo aplicado y el panel vuelve a leer sin datos propios en el DWG | — |
| OV-17 | Compatibilidad con RACKEDITAR | S1-07, S2-07 |
| OV-18 | Sin regresión visible (checklist de la guía) | — |
| OV-19 | Foco y teclado, con los oráculos adversariales | S1-06a..h |
| OV-20 | Cerrar el panel o una pestaña no modifica el dibujo | S1-08 |
| OV-21 | Cerrar un documento con borradores dirty: veto continuo y descarte explícito | S2-06 |
| OV-22 | Rendimiento percibido y registros de los diez escenarios | S1-09, S1-10, S2-08 |
| OV-23 | UNDO de un Actualizar: un solo paso y obsolescencia detectada | S2-05 |
| OV-24 | **Aplicado con avisos** visible | S2-11 |
| OV-25 | **No editable aquí** con su diagnóstico y el editor clásico ofrecido | S2-12 |
| OV-26 | **Estado Desconocido** y su resolución con «Comprobar ahora» | S2-10 |

Retirar o sustituir un escenario OV exige al Owner; reasignarlo exige una A-n (LIFECYCLE §8).

## 13. Plan inicial de enrutamiento (I-61)

**Reglas:** clasificación según `routing.md` §§1-3, sin elegir ni sondear modelos; las celdas se acreditan en `MainSha` al delegar; una
celda no acreditada queda pendiente de decisión o de una sonda autorizada; la fricción se registra para I-62; mientras I-62 no se integre,
rige I-61.

| Gate | Tarea | Clase (`routing.md` §1) | Effort semántico | Nota |
|---|---|---|---|---|
| F1 | ADR `propuesto` desde el anexo A; procedimientos de smoke y de diff de perfil | Documentación | Routine | — |
| F1 | modelo puro (sesiones e identidad, contexto, pistas, drenaje, políticas) con pruebas | Implementación de pruebas | Balanced | — |
| F1 | puente central con fuente simulada, ciclo de vida de suscripciones y guardas de fuente | Implementación transversal a capas (corta) | Deep | — |
| F1 | host de paleta, raíz de UI, `RACKPANEL`, selección fina, foco, contención e instrumentación | Implementación transversal a capas | Deep; puede ser Long-horizon | el Worker no usa AutoCAD |
| F2 | acciones de navegación, eco y caché de objetivos | Implementación transversal a capas | Deep | — |
| F3 | pestañas, preview y K superficies | Implementación de pruebas y mecánica | Balanced | — |
| F4 | `DraftSession` separada y máquinas de estado | Implementación transversal a capas | Deep | — |
| F4 | Actualizar del panel sobre la costura de I-55 y clasificación por frontera | Implementación transversal a capas | Deep | archivos calientes |
| F4 | inyecciones Debug y procedimiento de Smoke-2 | Implementación de pruebas / Documentación | Balanced | — |
| F4, F5 | caracterización por sistema (D13 y D9) | Caracterización | Balanced | ADR-0029 D13 |
| F5 | variantes caller-owned de Cantilever y Cama | Implementación transversal a capas | Deep | patrón I-47 G9.1 |
| F4, F5 | adaptador por sistema (ventanas de 547 a 3 713 líneas) | Implementación agéntica larga | Long-horizon → Frontera | sin celda Frontera con escritura medida: pendiente |
| F6 | captura del inventario sobre autoridades integradas, navegador, Selección (N) e informe de ID3 | Implementación transversal a capas | Deep | sin dependencia de I-63 |
| F7 | mediciones y regresión | Caracterización | Balanced | escenarios de host: Owner |
| todos | planificación y verificación del Controller | Controller | Balanced | — |
| rondas | revisión del Architect | Revisión de arquitectura | Deep | modo e independencia declarados |

**Precondiciones de la primera delegación:** Freeze AGREED; `docs/automation/state/I-64.yml` (AUTOMATION_PLAN 16.8); línea base nueva de
`config.toml`; autoridades leídas en `MainSha`.

## 14. Materialidad M-01..M-08 (aceptada por el Coordinator para V2; sin cambio en V3)

| M | Estado | Razón y condiciones |
|---|---|---|
| M-01 | **ACTIVATED** | cambian los dueños de borrador, dirty, contexto, cierre, aceptación de Actualizar y comprobación de base (§2) |
| M-02 | **NOT ACTIVATED** | condiciones: sin revisión persistida (D-12); builders, escritores y `Compose` existentes con preservación por vista (D-13, INV-10); sin campos nuevos ni estampado de identidad (D-22); la importación fuera del ámbito atómico no cambia el significado persistido (OM-5). Si una condición se rompe, M-02 se reabre |
| M-03 | **NOT ACTIVATED** | condición: los comandos clásicos permanecen observacionalmente invariantes, acreditado por INV-19 (D13 y D9 completos), OV-17 y la regla STOP de D-16 |
| M-04 | **ACTIVATED** | fallo nuevo en la vía del panel: todo o nada, clasificación por frontera, conflicto fail-closed, veto continuo de cierre y contención |
| M-05 | **ACTIVATED** | contrato de superficie de editor, intención separada del cierre y variantes caller-owned que consumen los sistemas (D-13, D-15) |
| M-06 | **ACTIVATED** | puntos de extensión del panel y contrato de adaptador (D-15, D-23) |
| M-07 | **ACTIVATED** | panel, puente central, sesiones de documento e inventario runtime propio: mecanismo transversal nuevo. La representación de hechos neutrales internos no se declara fundación reutilizable (MASTER-I63-I64-02 §4) |
| M-08 | **ACTIVATED** | complementos de ADR-0010 y ADR-0032 D6 (D-21) |

## 15. Coordinación

Puntas leídas por referencia remota antes de publicar (evidencia §15):

| Rama | Punta | Delta desde V2 | Efecto en I-64 |
|---|---|---|---|
| I-52 | `d8078ef3` | sin cambio | ninguno en el diseño; las reglas de host de §12.1 siguen; su RELEASE de host sigue sin fecha |
| I-62 | `acd88eaf` | Proposal V4 y V5 y sus paquetes; solo documentos | ninguno: I-61 sigue rigiendo hasta que I-62 se integre |
| I-63 | `a0654ae2` | decisiones del Owner P-01..P-05 sobre sus métricas; solo documentos | ninguno: no tocan la frontera |

- **I-63, resuelto por MASTER-I63-I64-02** (evidencia §15; tabla de cláusulas en §19):
  - se retiran la fundación compartida y la autoría inicial de I-63; I-63 sigue con métricas, providers, población y agregación;
  - I-64 captura e indexa por su cuenta, sobre autoridades integradas, para navegación, selección, UI y sesión (D-05);
  - ningún gate de I-64 depende de I-63, y desaparece el STOP por ausencia del snapshot;
  - I-63 no consume código no integrado de I-64; la duplicación de enumeraciones queda aceptada (coincide con la decisión del Owner P-14
    que registró I-63) y unificarla exigiría una decisión nueva del Master;
  - `ProjectSummary` (ID20) podrá alojarse después en el panel como consumidor o superficie, sin bloquear a I-64.
- **I-52:** las reglas de host de §12.1 incluyen su petición. RACKMIRROR (supuesto, no contrato) crearía racks nuevos sin mutar el origen:
  para el panel, un cambio externo que invalida cachés. Si su implementación toca la costura de I-55 o `EditX`, se coordina como archivo
  caliente al integrar.
- **I-62:** solo proceso.

## 16. Incógnitas, riesgos y su tratamiento

| Tema | Estado | Cómo se cierra | Responsable | Fase |
|---|---|---|---|---|
| Teclado y foco de WPF en `PaletteSet` | UNKNOWN | S1-06a..h; contingencia (b) de D-01 | Owner; Architect si cambia el host | Smoke-1 |
| Claves de perfil escritas por AutoCAD para una paleta sin `toolID` | UNKNOWN | diff de S1-11; acuse de I-52 | Owner + I-52 | Smoke-1 |
| Paleta y drenaje durante un modal | INFERENCE | S1-07 | Owner | Smoke-1 |
| La selección implícita sobrevive al comando interno | UNKNOWN | SN-01 | Owner | Smoke-NAV |
| Coste del barrido AUTH-06 en navegación (caché fría) y en la intención de edición | UNKNOWN | SN-09; Smoke-2 | Worker + Owner | F2, F4 |
| Un Actualizar = un paso de UNDO | INFERENCE | S2-05; alternativa por A-n | Owner | Smoke-2 |
| Distinguir la excepción de `Commit()` sin cambiar la costura para Insertar | obligación de diseño | INV-30 | Worker | F4 |
| El veto cancela `QUIT`, `CLOSEALL` y el cierre de ventana, y su orden con el aviso de guardar | INFERENCE | S2-06 | Owner | Smoke-2 |
| H-LOCK con otros complementos | UNKNOWN | no se prueba (no se instalan complementos); se declara | — | — |
| Conflictos falsos por reserialización | INFERENCE | registro de conflictos en Smoke-2 y OV final | Owner | F4..READY |
| Distribución de racks no editables (admisión) | UNKNOWN | S2-15 | Owner | Smoke-2 |
| Variantes caller-owned de Cantilever y Cama | obligación | F5; si no, decisión del Owner | Worker; Owner | F5 |
| Coste de la captura completa del inventario en dibujos grandes (`ScanEnvelopes` con recuento de referencias) | UNKNOWN | escenario 7 en DWG del Owner; refresco incremental como punto de extensión | Worker + Owner | F6 |
| Duplicación con la enumeración de métricas de I-63 | riesgo aceptado (MASTER-I63-I64-02) | revisión futura solo por decisión nueva del Master | Master | después del cierre |
| Ventana de host de I-52 sin fecha | riesgo de plan | F2 y F3 avanzan; F4 espera Smoke-1 y Smoke-NAV | Coordinator | F1..F4 |
| Extracción de editores de unas 3 500 líneas | riesgo de alcance | caracterización D13/D9 previa, adaptadores en secuencia, STOP de D-16 | Worker + Coordinator | F4, F5 |
| `PICKFIRST=0` | UNKNOWN | S1-12, solo si aplica | Owner | Smoke-1 |

## 17. Disposición de las decisiones de partida

| Origen | Disposición |
|---|---|
| Aceptación de D1-R1 y cierre de Discovery | base de diseño: Discovery D1-R1 (blob `3565015c`), sin reabrir |
| MASTER-I63-I64-01 | sustituida parcialmente; disposición por cláusula en §19 |
| MASTER-I63-I64-02 | D-05, §2, §11, §15, §19; anexo A |
| Plan de gates confirmado en la orden de V3 | §11 |
| Reglas de smoke del Master | §12.1 |
| Respuesta de I-52 | §12.1, §15; evidencia §13 |
| Escalada EXP-02 | D-11 (acordado por el Architect) |
| Veredicto del Architect sobre V1 y PV2-01..22 | §18 |
| Materialidad aceptada para V2 | §14 |
| Aviso de I-63 sobre P-14 | resuelto por MASTER-I63-I64-02 (§15); evidencia §14 |

## 18. Disposición de A64-PV1-01..22 y de las opcionales

La disposición de V2 se conserva íntegra; V3 solo cambia la forma de A64-PV1-18 y A64-PV1-21 por MASTER-I63-I64-02 y el plan de gates
confirmado. Cada REQUIRED queda **atendido**; solo su emisor puede cerrarlo.

| ID | Precisión | Disposición (V2, conservada en V3) | Dónde |
|---|---|---|---|
| A64-PV1-01 | PV2-03 | resultado todo o nada congelado sin mecánica; residuo de biblioteca OM-5; vía implementable = costura de I-55 con PREPARE → una transacción caller-owned de MUTATE → POST; importación en PREPARE; renombrado y postproceso en POST; clasificación por frontera de `Commit()`; H-LOCK reformulada; Cantilever y Cama con variante caller-owned o decisión del Owner | D-12, D-13, D-15; INV-12, INV-30 |
| A64-PV1-02 | PV2-08 | veto en todo intento mientras haya dirty; descarte explícito en el panel y por comando; repetir no es consentimiento; pérdida visible si el veto no se respeta | D-17; INV-20; S2-06; OV-21 |
| A64-PV1-03 | PV2-09 | identidad de instancia abierta, no puntero; dos aperturas = dos sesiones; SAVEAS conserva; sin reemparejar; peticiones y ejecuciones ligadas a la sesión | D-03; INV-22; SN-08 |
| A64-PV1-04 | PV2-10 | hecho, inferencia y obligación separados; ninguna corrección depende de la ausencia de eventos; los manejadores solo encolan; drenaje diferido, en reposo y sin modal RackCad; operaciones enumeradas | D-04; INV-23; P-03, P-04 |
| A64-PV1-05 | PV2-05 | base y estado inicial de la misma lectura en la intención de edición; la preview no captura AUTH-06; cambio entre preview y primera edición → re-render, nunca escritura | D-10; INV-27 |
| A64-PV1-06 | PV2-06 | Selectivo compara el registro completo; fuera las entradas globales de escaneo; Actualizar A no invalida B | D-12; INV-28; S2-14 |
| A64-PV1-07 | PV2-07 | se compara la atribución MUTABLE/READ_ONLY/BLOCKING/NOT_MEMBER con identidad, `View` y `Section`; Redraw/Erase se recalcula al aplicar | D-12; INV-29 |
| A64-PV1-08 | PV2-04 | StaleBase, InitialDraftState, CurrentDraftState y dirty separados; dirty sin JSON crudo | D-10; INV-26 |
| A64-PV1-09 | PV2-11 | ningún camino de evento a escritura de selección o vista; eco por contenido sin cambios secundarios; oráculos con contexto previo distinto y fila de Selección (N) | D-06, D-07; INV-03; SN-06 |
| A64-PV1-10 | PV2-12 | obligación de foco congelada sin mecánica; oráculos adversariales en Smoke-1 | D-01; INV-24; S1-06a..h; OV-19 |
| A64-PV1-11 | PV2-13 | contención de toda entrada del panel; pestaña en Error con su borrador | D-08, D-17; INV-25 |
| A64-PV1-12 | PV2-14 | la primera pulsación fija la preview; el texto cuenta para dirty, cierre y veto; nota a ADR-0032 reescrita | D-08, D-10; INV-04, INV-18; anexo B |
| A64-PV1-13 | PV2-15 | P-01 nunca abierto frente a P-01b oculto; escenario 10 masivo con panel nunca abierto, oculto y visible | D-18; INV-33; S1-10 |
| A64-PV1-14 | PV2-16 | recuento de suscripciones en todos los ciclos de vida | D-04; INV-02 |
| A64-PV1-15 | PV2-17 | guardas de fuente de transacciones, bloqueos y campos; comprobación Debug `TopTransaction == null` registrada en los smokes | D-02; INV-14; §12.1 |
| A64-PV1-16 | PV2-18 | INV-19 cubre D13 y D9 completos; la ventana clásica conserva `Owner` y recursos | D-15; INV-19 |
| A64-PV1-17 | PV2-01 | secuencia de cierre de F4 con Smoke-2; comprobaciones de host dentro de Smoke-2 con procedimiento exacto y variables de entorno del proceso | §11; §12.1; §12.4 |
| A64-PV1-18 | PV2-02 | hito Smoke-NAV tras F2 (Context Navigation); navegación del rack en contexto sin inventario global; `UNKNOWN` de la selección tras el comando interno declarado. **V3:** sin cambio de fondo; F2 confirmado por el Coordinator | D-07; §11; §12.3 |
| A64-PV1-19 | PV2-19 | acuse de I-52 y diff de solo lectura del perfil antes y después; STOP si hubiera que modificar | D-01; S1-11; §12.1 |
| A64-PV1-20 | PV2-20 | diferir un sistema es `OWNER-RESERVED`; residual de ID3 = lotes + sistemas no integrados | D-15, D-20; OV-07 |
| A64-PV1-21 | PV2-21 | **V3:** la causa desaparece: MASTER-I63-I64-02 §6 retira la dependencia de I-63 y, con ella, el STOP por ausencia del snapshot; F6 usa un inventario propio sobre autoridades integradas. Se conserva la regla general: ninguna integración parcial sin decisión del Owner | D-05; §11; §15; §19 |
| A64-PV1-22 | PV2-22 | prueba de UI y filas OV para Aplicado con avisos, No editable aquí y Desconocido | D-13; INV-31; S2-10..12; OV-24..26 |

| Opcional | Disposición |
|---|---|
| O-01 | adoptada: recarga perezosa de borradores Limpios con pista (D-10) |
| O-02 | **no adoptada** ahora: quitar los reactores de objeto con el panel oculto añade ciclos de suscripción; se reconsidera con el escenario 10 medido, por A-n |
| O-03 | adoptada: K superficies, K = 1 inicial (D-08, P-06) |
| O-04 | adoptada: comprobación previa de solo lectura antes de importar (D-12, D-13) |
| O-05 | adoptada: capas bloqueadas en PREPARE, como en la costura (D-13) |
| O-06 | adoptada: Selección (N) por tramos cancelables (D-06, D-09) |
| O-07 | adoptada: el modo degradado retira las suscripciones de la sesión (D-04) |
| O-08 | adoptada: «sin Id» = `IsNullOrWhiteSpace` (D-05, INV-16) |
| O-09 | adoptada solo la medición (S2-15); **no** se restringe el recorrido de `RackSiblingScan`, que es integrado y compartido |
| O-10 | adoptada: API interna de foco solo en el adaptador de host, con degradación y evidencia (D-01) |
| O-11 | adoptada: un Actualizar = un paso de UNDO, congelado como observable (D-13, INV-32) |
| O-12 | adoptada: S1-08 con dibujo recién abierto |
| O-13 | adoptada: subdiálogos del panel con la API modal y la ventana principal como dueña (D-02) |
| O-14 | adoptada: contrato de gate por sistema para ADR-0029 D13 (D-15) |
| O-15 | adoptada: ámbito de mutación propia por (sesión, miembros), de PREPARE a POST (D-04) |
| O-16 | adoptada: captura del texto pendiente en el host o adaptador (D-10) |
| O-17 | adoptada: estado sin documento abierto (D-02) |

## 19. MASTER-I63-I64-01 → disposición bajo MASTER-I63-I64-02

| Cláusula de MASTER-I63-I64-01 | Disposición bajo MASTER-I63-I64-02 | Dónde |
|---|---|---|
| I-63 e I-64 comparten solo una fundación mínima de snapshot lógico neutral por RackId | **Retirada** (§1): no se crea ni se exige fundación compartida | D-05; §2 |
| I-63 posee métricas, providers y agregación, y es el autor inicial del contrato puro común | **Propiedad de métricas conservada** (§3: métricas, providers, población y agregación); **autoría retirada** (§2) | D-05; §2 |
| I-64 posee índice runtime por documento, invalidación, ObjectIds y handles, navegación, selección, pestañas y borradores | **Conservada y ampliada**: I-64 posee también la captura del inventario del documento (§3) | D-05; §2 |
| El snapshot no decide la población de métricas ni sustituye la pertenencia para mutar | **Conservada en su fondo**: el índice de I-64 no decide métricas; la pertenencia para mutar sigue en las autoridades integradas (§3) | D-05, D-11 |
| No inventar RackId; no resolver diseños, geometría ni BOM; no persistir | **Conservada** para el índice de I-64; RackId e identidad de vista siguen en las autoridades integradas y el DWG authored es la única autoridad persistida (§3) | D-05, D-22; INV-16, INV-35 |
| F1 avanza sin la fundación; F2 consume el contrato de I-63 cuando esté integrado | **Sustituida** (§6): ningún gate depende de I-63; sin STOP por ausencia del snapshot | §11 |
| Smoke en la estación principal, tras el gate, tras RELEASE de I-52, una sola sesión, sin cambios de seguridad y STOP | **Sin cambio**: MASTER-I63-I64-02 no la modifica | §12.1 |
| Registrar la respuesta tardía de I-52 | **Cumplida** (evidencia §13) | — |
| — (nuevo en -02 §4) | I-64 puede usar una representación pura interna de hechos neutrales; no es fundación reutilizable ni contrato para I-63 | §0; D-05; M-07 |
| — (nuevo en -02 §5) | convertirla en fundación común exige una decisión explícita nueva del Master | D-05; §15 |
| — (nuevo en -02 §7) | I-63 no consume código no integrado de I-64 | §15 |

## Anexo A — Borrador del ADR propio

> Borrador para `docs/adr/NNNN-workspace-persistente-modeless.md`. Nace `propuesto` en F1 tras el Freeze (D-21); su número se asigna
> entonces. Solo el Owner lo acepta.

**ADR-NNNN: Workspace persistente y modeless de RackCad**

- **Estado:** propuesto.
- **Contexto:** RackCad edita con ventanas modales; el borrador vive en la ventana y el host interpreta la intención tras el cierre;
  Actualizar no compara con la base, escribe por vista y oculta faltantes (Discovery I-64 §§1, 3, 9.12). El Owner pide un panel persistente
  con borradores por rack y Actualizar como única frontera de escritura.
- **Decisión:**
  1. **Host:** un `PaletteSet` por proceso, sin estado persistido por RackCad; con el foco en un campo, ninguna tecla llega a AutoCAD.
  2. **Sesiones:** una por instancia abierta de documento; toda petición y ejecución va ligada a su sesión; nada cruza sesiones.
  3. **Eventos:** un único puente; los manejadores solo encolan; el drenaje ocurre en reposo y sin modal RackCad activo; ninguna corrección
     depende de la ausencia de eventos.
  4. **Inventario y pertenencia:** el inventario del documento y el índice runtime son del panel y se capturan sobre las autoridades
     integradas de identidad de rack y de vista; su representación interna de hechos neutrales no es fundación ni contrato para otros
     consumidores, y no decide métricas (MASTER-I63-I64-02). La pertenencia para mutar es `RackSiblingMembership` (Freeze I-55 V5 §4.1).
  5. **Selección:** ningún evento escribe selección ni vista; las acciones explícitas sí; el eco se reconoce por contenido.
  6. **Pestañas:** registros ligeros; K superficies materializadas; la primera pulsación fija la preview.
  7. **Borradores:** StaleBase, InitialDraftState y CurrentDraftState separados; dirty por estado inicial de la build y texto pendiente;
     base y estado inicial de una sola lectura; texto pendiente nunca perdido.
  8. **Actualizar desde el panel:** relectura cruda en la transacción de MUTATE; todo o nada sobre el authored del rack sobre la costura de
     I-55; clasificación por la frontera de `Commit()`; un paso de UNDO.
  9. **Cierre:** veto continuo con borradores dirty hasta el descarte explícito.
  10. **Coexistencia y migración:** comandos clásicos sin cambio; adaptadores escalonados con caracterización D13/D9 y escritores
      caller-owned; diferir un sistema es del Owner.
  11. **Persistencia:** ninguna propia; ni identidad estampada ni RackId inventado.
- **Relación con otros ADR:** complementa ADR-0010 y ADR-0032 D6 sin modificarlos; extiende a las superficies alojadas el contrato de
  ADR-0029 (D7, D8, D9, D11 y D13) sin cambiar su censo D1; consume ADR-0006, ADR-0009, ADR-0019 y ADR-0044.
- **Alternativas consideradas:** `ShowModelessWindow` (contingencia); host híbrido; revisión persistida; huella normalizada;
  `FindRackBlocks` para el panel; eventos como autoridad; veto de un solo intento; primitivas self-owned anidadas; seis reescrituras
  paralelas; un snapshot común con I-63 (retirado por MASTER-I63-I64-02).
- **Consecuencias:** primer uso de eventos de AutoCAD en el producto; conflictos falsos posibles por reserialización, a cambio de no
  sobrescribir nunca en silencio; dos semánticas de Actualizar conviviendo (clásica y del panel); residuo de biblioteca purgable tras un
  fallo; Insertar y `BindingIntent` siguen en el editor clásico.

## Anexo B — Notas posteriores previstas (se escriben al aceptarse ADR-NNNN)

- **ADR-0010:** «AAAA-MM-DD — Complementado por ADR-NNNN. ADR-0010 permanece `aceptado` y no es reemplazado. ADR-NNNN añade una vía de
  apertura, el panel de RackCad, cuyo Actualizar exige una base vigente, escribe todo o nada el authored del rack y no ofrece Insertar. Las
  decisiones de este registro para `RACKEDITAR` siguen vigentes sin cambio.»
- **ADR-0032:** «AAAA-MM-DD — Complementado por ADR-NNNN. En una superficie alojada en el panel de RackCad, D6 rige las fronteras
  bloqueables que inicia el usuario (cambiar de pestaña o de rack, Actualizar). En las fronteras que RackCad no puede bloquear (cambio de
  documento, ocultar el panel, desmaterializar la superficie o cerrar el documento), el texto pendiente se conserva tal cual, sin
  comprometerlo ni auto-repararlo, y vuelve a su caja. D5 y D6 no cambian.»
