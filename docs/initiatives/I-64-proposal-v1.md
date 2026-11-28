# I-64 — Proposal V1: workspace persistente y modeless de RackCad (ID30, absorbe ID3)

```text
Frozen: NO
Version: V1 (primera Proposal; sin rondas de revisión)
Unit: I-64   Workflow: V2 (T4)   Claim-Id: 614371d5-441f-4f14-bac9-f97017105610
Archetype: NEW ARCHITECTURE (brief; Discovery D1-R1 aceptado)
Materiality: M-01, M-04, M-05, M-06, M-07 y M-08 activados; M-02 y M-03 UNKNOWN tratados como activados, con resolución propuesta (§14)
Base: origin/main 819955d6   Discovery: docs/initiatives/I-64-discovery.md, versión D1-R1 (blob 3565015c, commit c6c44828); aceptado, Discovery cerrado
Master: MASTER-I63-I64-01 (evidencia §13)
Author: sesión principal responsable de I-64 (Claude), redacción directa
Review: PENDIENTE — Coordinator y Architect; paquete en docs/initiatives/I-64-architect-package-v1.md (revisión no realizada)
IMPLEMENTATION AUTHORIZATION = NO
```

**Fuentes y precedencia:**
- **Alcance:** el [brief del Owner](I-64-owner-brief.txt), citado por el título de sus secciones.
- **Hechos:** el [Discovery](I-64-discovery.md) D1-R1, citado por sección.
- **Decisiones de partida:** la aceptación de D1-R1 por el Coordinator y la decisión del Master `MASTER-I63-I64-01`, ambas registradas en la
  [evidencia](../automation/evidence/I-64-evidence.md) §13.
- **Precedencia:** prevalecen las autoridades integradas: AGENTS, WORKFLOW, LIFECYCLE, los ADR aceptados, los Freeze integrados (I-48,
  I-55 V5, I-58 e I-60) y AUTOMATION_PLAN §16.
- **Naturaleza:** esta Proposal es diseño. No cambia producto, pruebas ni normas.
- **Nombres:** los tipos son hipótesis de trabajo (brief, «KEY ARCHITECTURAL BOUNDARY»). El Freeze congela autoridad, persistencia,
  comportamiento observable, semántica de fallo, compatibilidad, no-objetivos, puntos de extensión, obligaciones invariante → prueba, la
  matriz OV y los resultados de los gates. No congela nombres de helpers, archivos ni mecánica interna ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §6).

## 0. Glosario

| Término | Significado aquí |
|---|---|
| **Authored persistido** (base comprometida) | Los sobres `RackEmbedDocument` de las definiciones de un rack en el DWG, más las dependencias que su edición lee (en Selectivo, el registro de variables). Es la **única** autoridad comprometida (brief, «KEY ARCHITECTURAL BOUNDARY») |
| **Borrador** | Estado editable de un rack en la workspace (`DraftSession`), solo en memoria |
| **Texto pendiente** | Lo escrito en un campo y aún no comprometido al borrador (ADR-0032 D5) |
| **Compromiso interno** | Paso del texto pendiente al borrador; ADR-0032 D6 lo llama «commit». No toca el DWG |
| **Actualizar** | La única frontera de escritura en el DWG desde la workspace (brief, «PRODUCT DECISION ALREADY MADE») |
| **Documento** | Un `Document` abierto en AutoCAD, identificado por su objeto nativo, nunca por la ruta del archivo |
| **Snapshot común** | Contrato puro, neutral y de solo lectura por RackId, cuyo autor inicial es I-63 (MASTER-I63-I64-01) |
| **Índice runtime** | Estructura de I-64 por documento: handles y ObjectIds, invalidación, navegación, selección, pestañas y borradores |
| **Pista** | Evento de AutoCAD traducido por el puente. Nunca es autoridad |
| **Reposo** | El documento activo no ejecuta ningún comando (`IsQuiescent`) |
| **Panel RackCad** | Nombre visible propuesto para la workspace. Evita confundirla con los «espacios de trabajo» de AutoCAD (`WSCURRENT`) |

La trampa de vocabulario de Discovery §4.3 queda resuelta así: lo que ADR-0032 llama «comprometido» es el **borrador**, y lo que el brief
llama «committed base» es el **authored persistido**.

## 1. Objetivo, no-objetivos y criterios de éxito

**Objetivo** (brief, «GOAL»): pasar de ventanas modales a un panel persistente y modeless dentro de AutoCAD, con sincronización de
contexto en los dos sentidos, borradores por rack y Actualizar como única frontera de escritura.

**No-objetivos** (brief, «OUT OF SCOPE» y «PREPARES BUT DOES NOT IMPLEMENT»), más los que esta Proposal añade para V1:
- **Insertar vistas y `BindingIntent` desde el panel.** Siguen en el editor clásico (D-13, D-15). Es una propuesta del Coordinator; si la
  considera cambio de alcance, pasa a `OWNER-RESERVED`.
- Crear racks nuevos desde el panel: los comandos de creación no cambian.
- Edición por lotes: es el residual de ID3 (D-20).
- Persistir el panel o sus borradores, incluida la posición de la paleta entre sesiones (brief, «MULTI-DOCUMENT»).
- Cambiar `RACKEDITAR`, `RACKLISTA`, `RACKPROYECTAR` u otros comandos existentes (D-16).

| # | Criterio de éxito (brief) | Mecanismo | Gate | Evidencia |
|---|---|---|---|---|
| 1 | El panel puede quedar abierto sin bloquear AutoCAD | D-01, D-02 | F1 | Smoke-1; OV-01 |
| 2 | AutoCAD se usa con normalidad con el panel abierto | D-01, D-04, D-18 | F1 | INV-01; OV-02, OV-03, OV-19 |
| 3 | La selección lógica de racks se sincroniza con el panel | D-06 | F1 (fina), F2 | INV-03; OV-04 |
| 4 | La navegación mueve la selección y la ubicación en AutoCAD | D-07 | F2 | INV-03; OV-05, OV-06 |
| 5 | Sesiones tipo navegador sin un editor pesado por rack | D-08, D-18 | F3, F5 | INV-15; OV-08, OV-09 |
| 6 | Los borradores persisten entre pestañas y no tocan el DWG hasta Actualizar | D-10, D-14 | F4 | INV-04, INV-05; OV-10..OV-12 |
| 7 | Los cambios externos se detectan antes de escribir | D-12 | F4 | INV-06; OV-13, OV-23 |
| 8 | Los documentos quedan aislados | D-03 | F1, F4 | INV-07; OV-15 |
| 9 | La autoridad authored/effective queda intacta | D-11, D-22 | F4, F5 | INV-10, INV-16; trazas DC-08 |
| 10 | No existe una arquitectura de suscripciones por pestaña | D-04 | F1 | INV-02 |
| 11 | El rendimiento queda caracterizado y es aceptable | D-18 | F1..F7 | mediciones de D-18; OV-22 |
| 12 | El estado de ID3 queda resuelto | D-20 | F6, READY | clasificación de cierre |

## 2. Mapa de autoridad (M-01)

| Materia | Hoy (Discovery §12.1) | Propuesta | Origen de la decisión |
|---|---|---|---|
| Authored persistido | sobre en el DWG | **sin cambio** | brief |
| Borrador | ventana modal + `RackEditorSession` | `DraftSession` por (documento, RackId) en Application | D-10; ADR-0029 D11 |
| Dirty | ámbito en 2 de 6 editores | ámbito por borrador | D-10; ADR-0029 D8 |
| Contexto de rack | el comando, al empezar | `SelectionContext` por documento | D-06 |
| Cierre | política de la ventana | política de pestaña, panel y documento | D-08, D-17 |
| Aceptación de Actualizar | el host tras el modal | puerto del panel → comando interno del Plugin; la escritura sigue en el Plugin con las primitivas existentes | D-13 |
| Comprobación de base | nadie | compuerta dentro de la transacción de escritura | D-12 |
| Pertenencia para mutar desde el panel | — | `RackSiblingMembership.Classify` (Freeze I-55 V5 §4.1); RACKEDITAR conserva `FindRackBlocks` (§3.7) | D-11 (resuelve EXP-02) |
| Enumeración lógica neutral | — | snapshot común de I-63 | MASTER-I63-I64-01 |
| Índice runtime y navegación | — | I-64 | MASTER-I63-I64-01 |
| Métricas, providers, agregación y población | — | I-63 | MASTER-I63-I64-01 |

No aparece una segunda autoridad persistente: el estado del panel nunca se persiste ni otro comando lo lee como autoridad (D-22).

## 3. Host, propiedad y documentos

### D-01 — Host: `PaletteSet`

- **Decisión:**
  - un único `PaletteSet` por proceso (API de `acmgd.dll`, Discovery §9.1);
  - aloja con `AddVisual` un solo control raíz WPF de `RackCad.UI`;
  - se construye con `PaletteSet(name)`, sin `toolID` y sin manejadores `Load`/`Save`, y con `KeepFocus = false`;
  - es acoplable y flotante;
  - se muestra con un comando nuevo, `RACKPANEL` (nombre propuesto; el nombre visible lo elige el Owner y no bloquea);
  - cerrar la paleta la **oculta**: las sesiones siguen en memoria.
- **Por qué:** es la integración nativa y acoplable que el brief pide evaluar seriamente, y la API está presente. El shell
  `RackEditorVisualShell` ya es un `Control` alojable (ADR-0019). Sin `toolID` ni `Load`/`Save`, el código de RackCad no pide a AutoCAD
  guardar estado de la paleta en el perfil (`INFERENCE`). Eso es coherente con el mandato «I-64 no cambia perfil» del Master y con la
  advertencia de I-52 de que cambiar el registro de AutoCAD altera su máquina ligada (evidencia §13).
- **Alternativas:**
  - (b) `ShowModelessWindow`: ventana flotante no acoplable, con los mismos riesgos de foco. Rechazada como host principal; queda como
    **contingencia** si Smoke-1 muestra un fallo propio de `PaletteSet`. El cambio exigiría una A-n material, con Architect y Coordinator.
  - (c) Híbrida, con la paleta como navegador y una ventana como editor: duplica host, foco y ciclo de vida. Rechazada.
- **UNKNOWN que verifica Smoke-1:**
  - teclado y foco de WPF alojado (`IKeyboardInputSink`, Discovery §9.1);
  - comportamiento de la paleta mientras un modal (RACKEDITAR) está abierto;
  - si AutoCAD escribe por su cuenta estado de una paleta sin `toolID`.

### D-02 — Propiedad y ciclo de vida

- **Controlador** (hipótesis `WorkspaceController`): uno por proceso, en el Plugin. Se crea con el primer `RACKPANEL` y vive hasta que
  AutoCAD termina. Antes de ese momento hay cero suscripciones y cero trabajo, así que la línea base «panel cerrado» no cambia (INV-01).
- **Capas** (ADR-0006):
  - `RackCad.UI` tiene las vistas y view-models WPF, sin referencias a AutoCAD (guarda existente `UiSystemBoundaryGuardTests`);
  - Application tiene el estado puro (sesiones, pestañas, borradores, contexto de selección, máquina de estados del borrador y
    políticas), probado sin AutoCAD;
  - el Plugin tiene el host de la paleta, el puente de eventos, los adaptadores de lectura y escritura del DWG y la ejecución en contexto
    de comando.
- **Puerto:** la UI pide acciones a través de un puerto (hipótesis `IWorkspaceHostPort`) implementado en el Plugin. Recibe claves y
  valores; nunca `Document`, `Database`, `Transaction`, `DBObject` ni `ObjectId` (INV-14).
- **Acceso al DWG:** toda lectura o escritura ocurre en el Plugin, como una operación con el documento bloqueado y una transacción cuyo
  alcance es esa operación. Nada abierto sobrevive a la operación (INV-14). Todas ocurren solo en reposo:
  - las escrituras y las acciones que cambian la vista o la selección se ejecutan en contexto de comando;
  - las lecturas de reposo (selección, sobres, índice) se hacen con el documento bloqueado desde el contexto de aplicación;
  - si AutoCAD no está en reposo, las acciones del panel aparecen deshabilitadas.
- **Panel oculto:** se suspenden las lecturas, y las pistas siguen acumulándose como marcas de coste constante. Al mostrar el panel, se
  reconcilia en el siguiente reposo.
- No hay hilos de fondo que toquen AutoCAD.

### D-03 — Sesiones por documento

- **Clave:** una sesión de documento (hipótesis `WorkspaceDocumentSession`) por `Document`, identificado por su objeto nativo. La
  identidad se compara por el puntero no gestionado, nunca por igualdad de referencias del envoltorio ni por ruta.
- **Contenido:** índice runtime, `SelectionContext`, pestañas (con su preview), pestaña activa y borradores.
- **Vida:**
  - se crea de forma perezosa cuando el documento se activa con el panel ya creado;
  - se destruye en `DocumentToBeDestroyed`: el puente quita sus suscripciones y se descarta el índice; los borradores siguen D-17.
- **Cambio de documento activo:** el panel muestra la sesión del nuevo documento. Las demás conservan su estado y no leen nada.
- **Aislamiento:** toda clave de rack es (documento, RackId). El mismo GUID en dos documentos, por COPY o WBLOCK, da dos sesiones
  independientes (O-06). Ningún borrador, pestaña ni resultado cruza de un documento a otro (INV-07).

## 4. Eventos, inventario y selección

### D-04 — Un puente central de eventos

- **Único suscriptor:** el puente (hipótesis `AutoCADEventBridge`, en el Plugin) es la única parte de RackCad suscrita a eventos de
  AutoCAD. Ninguna pestaña, editor, view-model ni adaptador se suscribe (INV-02).
- **Fuentes** (Discovery §9.2), con exactamente una suscripción por fuente:
  - colección de documentos: activación, creación y destrucción;
  - cada documento con sesión: `ImpliedSelectionChanged`, `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed`,
    `BeginDocumentClose` y, en su `Editor`, `EnteringQuiescentState`;
  - la base de datos de cada documento con sesión: `ObjectAppended`, `ObjectErased`, `ObjectModified`, `ObjectUnappended` y
    `ObjectReappended`;
  - `Application.Idle`, solo mientras haya pistas pendientes.
- **Manejadores** (guía oficial de Autodesk):
  - no escriben la base de datos, no piden datos al usuario, no abren diálogos ni llaman a resolvers;
  - filtran por tipo con coste constante (definición de bloque, Xrecord, referencia de bloque, diccionario) y anotan una pista, una marca
    o una generación con la clave del documento;
  - capturan y registran toda excepción, y nunca la propagan a AutoCAD.
- **Drenaje:** las pistas se procesan una vez por reposo del documento activo, coalescidas: N eventos dan un solo refresco.
- **Pistas, no autoridad:** una pista solo marca el índice como inválido, el contexto como pendiente de recalcular o un borrador como
  «posiblemente obsoleto». La obsolescencia la decide la relectura (D-12).
- **Undo/redo:** no hay un evento gestionado específico (Discovery §9.2, `UNKNOWN`). `CommandEnded` de `U`, `UNDO`, `REDO` y `MREDO`, junto
  con los eventos de objeto, invalidan el índice del documento y marcan sus borradores como «posiblemente obsoletos». No bloquea, porque
  la autoridad es la relectura.
- **Reentrada:**
  - **estructural:** los eventos de selección solo cambian el contexto; nunca escriben la selección (D-06, D-07);
  - **mutación propia:** durante un Actualizar propio, el puente abre un ámbito de mutación propia. Sus pistas se coalescen; tras el commit
    la base se relee (D-13) y no se trata como cambio externo ni dispara otro Actualizar (INV-08);
  - ningún manejador ejecuta acciones que vuelvan a disparar el mismo evento.
- **Modales:** mientras hay un modal abierto (RACKEDITAR o un editor clásico) no se disparan eventos. Al terminar el comando, sus
  escrituras llegan como pistas acumuladas.

### D-05 — Inventario: snapshot común frente a índice runtime

Reparto fijado por MASTER-I63-I64-01:

| Elemento | Dueño | Uso en I-64 |
|---|---|---|
| Contrato puro de snapshot lógico neutral por RackId | I-63, autor inicial | lo consume en F2, **solo** cuando esté integrado en `origin/main` |
| Métricas, providers, agregación y población de métricas | I-63 | ninguno |
| Índice runtime por documento: handles y ObjectIds, invalidación, navegación, selección, pestañas y borradores | I-64 | propio |
| Pertenencia para mutar | autoridades existentes (D-11) | el snapshot no las sustituye |

**Reglas del Master:** no inventar RackId; no resolver diseños, geometría ni BOM para formar el snapshot; no persistirlo.

- **Antes de F2** (sin snapshot integrado) no hay enumeración global. F1 obtiene el contexto de selección leyendo solo el sobre de la
  definición de cada referencia seleccionada, como hace hoy `PickRackBlock`. Usa una caché por definición que las pistas invalidan.
- **En F2**, el índice combina el snapshot de I-63 con las decoraciones runtime de I-64: referencias por vista en Model Space, orden de
  navegación y generación.
  - Refresco: invalidar y refrescar bajo demanda, solo con el panel visible, el documento activo en reposo y el índice inválido.
  - En V1 se recaptura entera. Un refresco incremental por definición es un punto de extensión si la medición lo exige.
- **Necesidades de I-64 sobre el snapshot** (información para I-63, no diseño):
  - igualdad por RackId;
  - `Kind` y `Name`;
  - vistas (`View`, `Section`) con identidad estable de la definición miembro;
  - diagnósticos de captura: ilegible, sin Id, versión futura, xref y Kind divergente;
  - sin `Design` ni resolución.

  Si al contrato integrado le falta algo de esto, I-64 lo eleva al Coordinator. No lo suple con un enumerador propio, que sería STOP al
  Master.
- **Sobres sin Id:** aparecen como entrada de diagnóstico «bloque RackCad sin identidad», por definición. Se pueden ver y seleccionar,
  pero no editar en el panel; el panel ofrece el editor clásico.
- **Sobres ilegibles, de versión futura o divergentes:** se muestran como diagnóstico, nunca se ocultan ni cuentan como éxito (ADR-0044
  §8).
- **Estado por rack** (brief, «status»): se deriva y no se persiste. Valores: Normal, Con borrador, Posiblemente obsoleto, Conflicto,
  Diagnóstico y No editable aquí.
- **Búsqueda:** filtro local sobre el índice por nombre, Kind e Id.

### D-06 — Sincronización AutoCAD → RackCad

- **Recorrido:**
  1. `ImpliedSelectionChanged` deja una pista.
  2. En reposo, el Plugin lee la selección implícita del documento activo.
  3. Prefiltra por clase las referencias de bloque, sin abrir objetos.
  4. Lee la definición de cada referencia en una transacción corta.
  5. Obtiene el RackId por índice o caché y fija el `SelectionContext`.
- **Contextos posibles:** Ninguno; Uno (un RackId); Selección (N racks y M entidades que no son racks); Sin identidad; Diagnóstico.
- **Efectos:**
  - **Uno:** pestaña preview (D-08), salvo la guardia de activación;
  - **Selección (N):** vista agregada (D-09); nunca abre N pestañas;
  - **Ninguno:** el contexto se limpia; las pestañas no cambian.
- **Durante un comando** (por ejemplo, mientras se designa en `MOVE`) la selección se ignora hasta el reposo, así que el contexto no
  cambia a mitad de comando.
- **Exclusiones de V1:** referencias anidadas, xref y espacio papel no se mapean; cuentan como «no rack».
- **Selecciones grandes:** el trabajo es proporcional a las referencias seleccionadas. El detalle de Selección (N) se calcula en reposo sin
  bloquear, con un estado «calculando». Los umbrales se fijan tras la línea base (D-18).
- **`PICKFIRST=0`:** no hay selección implícita y el panel lo indica. I-64 no cambia la variable (mandato del Master: no cambia
  configuración). El comportamiento exacto es `UNKNOWN` y lo verifica Smoke-1.
- **Idempotencia:** el mismo conjunto seleccionado produce el mismo contexto, y un contexto igual al actual no vuelve a navegar (INV-03).

### D-07 — Navegación RackCad → AutoCAD

- **Acciones:** Seleccionar, Ver, Centrar y Localizar vista, sobre el rack del contexto, de la pestaña o del navegador. Se ejecutan en el
  Plugin, en contexto de comando y solo en reposo. Ninguna escribe el authored.
- **Objetivos:** las referencias de nivel superior en Model Space de las definiciones miembro. Vienen del índice en F2; antes, de la
  pertenencia de D-11, en lectura. Orden determinista: frontal, lateral, planta y el resto de vistas por el orden del codec; dentro de cada
  vista, por handle.

| Acción | Semántica |
|---|---|
| **Seleccionar** | Selección implícita con **todas** las referencias del rack en Model Space, copias incluidas. Si el espacio actual no es Model Space, la acción no está disponible y se explica; no se cambia `TILEMODE` |
| **Ver** | Zoom a la extensión de la referencia principal (la primera del orden) con margen del 10 %, como `ZoomToRack` hoy (Discovery §9.5). Con varias copias, un control «copia k/M» recorre las demás |
| **Centrar** | Mueve el centro de la vista al centro de la referencia principal sin cambiar la escala |
| **Localizar vista** | Ver o Centrar sobre la primera referencia de una vista concreta, con el mismo recorrido de copias |

- **Limitación declarada de V1:** se supone planta XY, como `ZoomToRack`. En una vista 3D el resultado no está garantizado.
- **Sin bucle de selección:** Seleccionar cambia la selección implícita; el evento de vuelta se mapea al mismo rack, el contexto no cambia
  y no hay nueva navegación (INV-03). Ningún otro flujo escribe la selección.
- **Undo:** Ver y Centrar cambian la vista como lo haría un zoom (`INFERENCE`: entran en el historial como cualquier zoom). Seleccionar no
  escribe nada que se pueda deshacer.
- **Foco:** tras una acción, la entrada vuelve al dibujo si se consigue sin APIs internas. Usar `Autodesk.AutoCAD.Internal` exige acuerdo
  del Architect. Lo verifica Smoke-1.

## 5. Pestañas, multiselección y borradores

### D-08 — Pestañas, preview y carga perezosa

- **Registro ligero por pestaña:** RackId, nombre, Kind, preview o fijada, y referencia al borrador si existe. No retiene árbol WPF,
  sistema resuelto, BOM, objetos o transacciones de AutoCAD ni suscripciones (brief, «TABS / PERFORMANCE CONTRACT»).
- **Preview:**
  - hay como máximo una preview por sesión de documento;
  - un clic simple en el navegador, o una selección Uno(R), activa la pestaña de R si ya existe; si no, reemplaza la preview o crea una;
  - **la primera edición convierte la preview en fijada.** Por tanto una pestaña dirty nunca es preview y nunca se reemplaza en silencio
    (INV-04).
- **Fijar:** doble clic, «Fijar» o «Editar» convierten la pestaña en fijada.
- **Cerrar:** una pestaña sin cambios se cierra sin preguntar; una dirty pregunta Descartar o Cancelar. Cerrar **nunca** escribe el DWG
  (INV-05, INV-09).
- **Marca dirty:** «Nombre *».
- **Materialización perezosa:**
  - solo la pestaña activa tiene superficie de editor;
  - al cambiar de pestaña se compromete el texto pendiente (D-10) y se destruye el árbol anterior; al volver, se reconstruye desde el
    borrador;
  - sistema resuelto, previews y BOM se calculan solo para la pestaña activa y bajo demanda.
- **Guardia de activación:** una selección no activa otra pestaña si la activa tiene texto pendiente inválido. En ese caso la preview se
  actualiza en segundo plano, con aviso.
- **Límite:** no hay límite fijo de pestañas. Un límite blando, si hace falta, se decide tras medir «10 pestañas ligeras».
- **Sistema aún no integrado** (D-15): pestaña de resumen (identidad, vistas, estado y navegación) con «Abrir en editor clásico».

### D-09 — Multiselección

- **Contexto Selección (N):** lista virtualizada de racks (nombre, Kind, referencias seleccionadas y vistas) y recuento de entidades que no
  son racks. Cada fila permite abrir el rack (preview o fijada), seleccionar solo ese rack o verlo.
- Selección (N) nunca crea pestañas ni borradores.
- **Punto de extensión:** el contexto expone un conjunto estable de pares (documento, RackId) para un futuro borrador por lotes, el residual
  de ID3 (D-20). No se implementa ni la edición de propiedades comunes ni un Actualizar por lotes.

### D-10 — `DraftSession`

- **Clave:** (documento, RackId).
- **No es un modelo paralelo:** evoluciona `RackEditorSession`, `EditorPendingWork` y los estados de Application existentes (ADR-0029 D11).
- **Contenido:**
  - **Base** (D-11, D-12): la pertenencia al abrir, con el texto crudo exacto de cada sobre miembro; el origen de apertura; y las
    dependencias leídas (en Selectivo, el texto crudo del registro de variables y de las entradas de escaneo que se usaron al abrir);
  - **Borrador:** el estado editable del sistema (diseño y estado de Application), que se puede guardar y restaurar sin la ventana;
  - **Texto pendiente conservado:** los valores crudos de los campos no comprometidos, solo cuando una frontera no se pudo bloquear.
- **Dirty:** el borrador difiere de la base o hay texto pendiente conservado. El ámbito es el borrador (ADR-0029 D8).
- **Estados:**

  | Estado | Significado |
  |---|---|
  | Limpio | borrador igual a la base |
  | Dirty | cambios sin aplicar |
  | Posiblemente obsoleto | llegó una pista sobre sus miembros o dependencias; es informativo |
  | Conflicto | la relectura encontró diferencias |
  | Aplicando | Actualizar en curso; no se puede editar |
  | Desconocido | tras un fallo de estado posterior incierto; exige recargar o comprobar |

- **Fronteras de compromiso interno** (complemento de ADR-0032 D6; D-21 y anexo B):
  - cambiar de pestaña, de rack o de documento, ocultar el panel y Actualizar;
  - en las fronteras que el usuario inicia dentro del panel, rige D6 tal cual: si algún campo es inválido, la acción se aborta y el texto
    permanece en su caja;
  - en las fronteras que RackCad **no** puede bloquear (cambio de documento, ocultar el panel o cerrar el documento), el texto pendiente se
    conserva tal cual en el borrador y vuelve a su caja al rematerializar la pestaña. Nunca se pierde ni se auto-repara (INV-18).
- **Vida:** desde que el rack se abre en una pestaña hasta que se cierra la pestaña (limpia o descartada), se destruye el documento (D-17)
  o se sale de AutoCAD. Solo en memoria.

## 6. Base, obsolescencia y Actualizar

### D-11 — Apertura y pertenencia (resuelve la escalada EXP-02)

- **Regla de pertenencia** para la base y para escribir desde el panel: `RackSiblingMembership.Classify` sobre los hechos del barrido
  AUTH-06 (Freeze I-55 V5 §4.1).
  - Un sobre ilegible atribuible al rack es `BlockingUnreadable` y bloquea; una xref es `ReadOnly`.
  - `MutableMembers` es el único conjunto que se compara y se redibuja.
- **Por qué:**
  - el panel es un consumidor **nuevo** que muta;
  - ADR-0044 §8 exige que lo ilegible no se convierta en ausencia;
  - esa regla ya gobierna Insertar y RACKPROYECTAR.

  `FindRackBlocks` sigue gobernando el Actualizar de RACKEDITAR y el ejecutor de variables (I-55 V5 §3.7) y no se toca.
- **Alternativa rechazada:** `FindRackBlocks` por paridad con RACKEDITAR, porque omite sin aviso los sobres ilegibles atribuibles.
- **Origen de apertura:** la definición de la referencia elegida, si el usuario viene de una referencia; si no, la primera según el orden
  de D-07.
- **Admisión para editar:**
  - AUTH-13 `Single` sobre los miembros de la compuerta authored;
  - Cama, cuyo comparador da `Unreadable` (Discovery §9.7), solo con un único miembro mutable;
  - `Divergent`, `Unreadable`, `BlockingUnreadable` o un sobre sin Id dejan el rack **no editable en el panel**, con el diagnóstico
    visible y el editor clásico disponible, sin cambio en su comportamiento.
- **Lectura de la base:** una sola operación de lectura, con el documento bloqueado y una transacción.
- **Coste:** el barrido AUTH-06 es más caro que `ScanEnvelopes` (Discovery §7.1). Ocurre al abrir y al Actualizar, nunca en reposo, y se
  mide en F4.

### D-12 — Detección de obsolescencia

- **Autoridad:** una relectura **dentro de la transacción de escritura**, como el precedente E de Discovery §9.7 (`RegistryCommit`,
  `MutationDestinationBinding`). Compara con la base tres cosas:
  1. el conjunto de miembros: clave de la definición, `View`, `Section` y clase de pertenencia;
  2. el texto crudo exacto de cada sobre miembro;
  3. las dependencias registradas (en Selectivo, el texto crudo del registro y de las entradas usadas).

  Solo hay MATCH si todo es idéntico. Cualquier diferencia da **Conflicto y cero escrituras** (INV-06).
- **Sin normalización:** los riesgos de Discovery §9.14 solo pueden producir conflictos falsos, nunca aceptaciones falsas, porque texto
  igual implica authored igual. Es la dirección fail-closed.
- **Alternativas de Discovery §9.7:**
  - C (revisión persistida): rechazada; cambiaría el esquema (M-02);
  - B (huella normalizada): rechazada por los riesgos de §9.14;
  - A (comparación tipada AUTH-13): se usa solo como admisión al abrir (D-11), no como compuerta de escritura, porque no cubre vistas,
    nombre, propiedades ni registro;
  - D (eventos): solo pista.
- **Pistas y comprobación manual:** los eventos solo marcan «posiblemente obsoleto». La acción «Comprobar ahora» hace una relectura de
  solo lectura y deja el borrador en Conflicto o confirma su estado.
- **Reconciliación explícita** (brief, «STALE / EXTERNAL CHANGE»):
  1. **Descartar y recargar:** el borrador se sustituye por una apertura nueva.
  2. **Conservar mi borrador sobre la versión actual:** tras una confirmación explícita, la base pasa a ser la relectura actual y el
     borrador no cambia. El siguiente Actualizar vuelve a comprobar contra esa base. En Selectivo, la reconciliación de vínculos usa el
     authored de la base nueva (Freeze I-48).

  No hay fusión automática en V1.
- **Conflictos falsos esperados:** un Actualizar de RACKEDITAR sin cambios, o la reserialización de otro build, cambian el texto. El
  usuario ve Conflicto y elige. Se acepta ese coste de experiencia a cambio de no sobrescribir en silencio.
- **H-LOCK sigue siendo hipótesis** (Discovery §9.7). Con D-13, la comprobación y las escrituras ocurren bajo un bloqueo y en una
  transacción, con la importación dentro. Quedan fuera: la purga y el `Regen` posteriores (sin efecto en el authored), los manejadores de
  otros complementos que escriban durante la transacción y la reentrada propia, que cubre D-04. Se verifica en F4 con una prueba de host
  (manejador de prueba y segundo comando).

### D-13 — Semántica de Actualizar desde el panel (M-04)

**Recorrido:**
1. **Compromiso interno** del texto pendiente (ADR-0032 D6) y construcción del sistema con el builder existente del editor. Si falla, no hay
   escritura y el campo muestra el error.
2. **Petición al puerto.** El Plugin ejecuta un comando interno en contexto de comando, solo en reposo.
3. **PREPARE**, sin escrituras:
   - payloads por vista con los builders existentes;
   - preflight de interiores y descriptor existentes;
   - disponibilidad de la biblioteca, con la lista de piezas faltantes.
4. **MUTATE**, con un bloqueo y una transacción externa:
   1. relectura y comparación (D-12); si hay Conflicto, se aborta;
   2. comprobación de destino: las vistas del plan siguen siendo las del dibujo (patrón `MutationDestinationBinding`);
   3. importación de bloques;
   4. redibujo por vista con las primitivas existentes, como transacciones anidadas;
   5. renombrado;
   6. borrado de fantasmas según la política actual de cada sistema (Cabecera no borra);
   7. commit.
5. **POST:** purga (best effort), un solo `Regen` y relectura de la base nueva. La base pasa a ser esa relectura y el borrador queda limpio.

**Resultados** (O-12):

| Resultado | Cuándo | Escrituras authored | Borrador | Lo que ve el usuario |
|---|---|---|---|---|
| No aplicado — Conflicto | D-12 | ninguna | se conserva, en Conflicto | qué cambió: miembros, sobres o dependencias |
| No aplicado — Admisión | `Divergent`, `Unreadable`, `BlockingUnreadable` | ninguna | se conserva | el diagnóstico |
| No aplicado — Preflight | en PREPARE | ninguna | se conserva | la causa |
| No aplicado — Fallo | excepción o `Failure` en MUTATE | ninguna: se aborta la transacción externa | se conserva | la causa; nunca «actualizado» |
| Aplicado | commit de MUTATE | todas las vistas del rack | rebasado y limpio | las vistas redibujadas |
| Aplicado con avisos | faltantes de biblioteca, renombrado, purga o `Regen` fallidos | todas | rebasado | avisos visibles, no solo en el registro |
| Aplicado, relectura fallida | POST no pudo releer | todas | Desconocido | «aplicado; recarga para seguir editando» |

- **Todo o nada** para el authored del rack (INV-12). Es la vía nueva. RACKEDITAR conserva su semántica, con commit por vista y faltantes
  invisibles (Discovery §9.12; D-16).
- **Definiciones importadas:** quedan dentro de la transacción externa y se revierten si esta aborta. Es `INFERENCE` sobre las
  transacciones anidadas de AutoCAD; F4 lo verifica con un fallo inyectado en el host.
- **Undo:** un comando interno forma un grupo de deshacer (`INFERENCE`; F4 y Smoke-2). Deshacer después de Actualizar deja obsoleto el
  borrador rebasado, y el siguiente Actualizar da Conflicto (INV-06).
- **Insertar y `BindingIntent`** no se ofrecen en el panel en V1 (§1):
  - Insertar exige jig, AUTH-13 y colocación;
  - `BindingIntent` muta el registro y varios racks a la vez, y dejaría obsoletos otros borradores.

  Los dos siguen en el editor clásico.
- No puede haber un Actualizar mientras otro está en curso; el botón refleja el estado Aplicando.

### D-14 — Cambios con borradores dirty

- Cambiar de pestaña, de rack o de documento, ocultar el panel o cerrar una pestaña limpia **nunca** escribe el DWG (INV-05).
- Los borradores se conservan entre pestañas y entre documentos.
- Una pestaña dirty nunca se reemplaza (D-08), y cerrarla pregunta.
- El cierre de un documento sigue D-17.

## 7. Editores y comandos existentes

### D-15 — Migración escalonada de los editores

- **Patrón:** migración escalonada (D) sobre A y B de Discovery §9.6, con un **contrato de adaptador común** y adaptadores finos por sistema,
  en secuencia. No hay seis reescrituras paralelas.
- **Contrato de adaptador**, por sistema:
  1. **Superficie alojable:** el contenido del editor (el shell, que es un `Control`) se separa de su `Window`. La ventana clásica pasa a
     envolver la misma superficie, sin cambio observable; se caracteriza antes (ADR-0029 D13).
  2. **Intención separada del cierre:** la superficie emite la intención de Actualizar hacia su host. El host clásico la interpreta como
     hoy (cerrar y `EditX`); el host del panel, según D-13.
  3. **Ida y vuelta del estado:** guardar y restaurar el borrador y el texto pendiente sin la ventana.
  4. **Diálogos con `Owner = this`:** el host decide el dueño (la ventana clásica o la ventana principal de AutoCAD).
- **Criterio de entrada por sistema:** caracterización en verde (cierre, intención y `BuildSystem` desde un estado restaurado) e ida y
  vuelta completa. Un sistema que no lo alcance queda en «resumen + editor clásico»; su diferimiento lo decide el Coordinator con una A-n y
  el residual consta en el cierre.
- **Orden propuesto:**
  - **piloto en F4: Push Back**, que ya tiene shell, sesión, estado en Application y ámbito dirty con `EditorPendingWork`;
  - **F5-A:** Selectivo (sin `BindingIntent` en el panel), Dinámico y Cantilever;
  - **F5-B:** Cabecera (sin shell ni sesión: adaptador sobre su ViewModel) y Cama (sin shell; una vista; admisión con un único miembro).
- No se reescribe la lógica de dominio de ningún editor. El code-behind se traslada a la superficie; solo cambian la separación de
  intención y el estado.

### D-16 — Coexistencia con RACKEDITAR y los comandos clásicos

- **Sin cambio observable** en RACKEDITAR, los comandos de creación, RACKLISTA, RACKPROYECTAR, RACKDUPLICAR, RACKBOMTOTAL y RACKVARIABLES.
  RACKEDITAR conserva `FindRackBlocks`, el commit por vista y sus mensajes.
- **Con RACKEDITAR abierto** (modal), el panel queda inerte porque no hay eventos. Al terminar, sus escrituras llegan como pistas: un
  borrador del mismo rack pasa a «posiblemente obsoleto» y su Actualizar dará Conflicto.
- **«Abrir en editor clásico»** ejecuta, desde el panel, el mismo flujo de edición clásico sobre el origen de apertura. Si existe un
  borrador de ese rack, avisa antes de que el editor clásico parte del dibujo, no del borrador.
- **Sobres sin Id:** solo el editor clásico, que conserva su estampado de GUID. El panel no estampa (D-22).
- Si la extracción de superficies (D-15) cambiara algo observable de RACKEDITAR, sería M-03 y **STOP**.

### D-17 — Fallos generales y cierre de documento

- **Excepción en un manejador:** se registra y la sesión pasa a **modo degradado**, sin sincronización automática, con aviso visible y un
  «Refrescar» manual. AutoCAD sigue funcionando.
- **Error de host en una lectura:** la operación aborta su transacción y la UI muestra el error; no queda nada abierto.
- **Cierre de un documento con borradores dirty** (propuesta; visible para el Owner en OV-21):
  - en `BeginDocumentClose`, el panel **veta el primer intento**. `DocumentBeginCloseEventArgs.Veto` existe en `accoremgd.dll` (evidencia
    §13);
  - avisa en la línea de comandos y en el panel: aplicar o descartar, y volver a cerrar;
  - un segundo intento sobre el mismo documento, sin cambios en sus borradores, procede y descarta los borradores, con aviso registrado;
  - no se abre ningún diálogo desde el manejador;
  - durante `QUIT` el mismo veto cancelaría la salida (`INFERENCE`). F4 verifica en el host el orden respecto del aviso de guardar de
    AutoCAD.
  - **Alternativa:** no vetar y avisar después. Se rechaza como opción por defecto porque pierde borradores sin consentimiento (ADR-0029
    D7). Queda como opción del Architect o del Owner si el veto interfiere con el cierre de AutoCAD.
- **Caída de AutoCAD:** los borradores se pierden; están solo en memoria, lo que acepta el brief.
- **Fallo al crear la paleta:** se muestra un mensaje y el resto de RackCad sigue; los comandos clásicos no dependen del panel.

## 8. Rendimiento, persistencia y extensión

### D-18 — Rendimiento y carga perezosa

**Invariantes:**

| ID | Invariante |
|---|---|
| P-01 | Panel cerrado: misma línea base que hoy, con cero suscripciones |
| P-02 | Panel ocioso: cero llamadas a resolver, redibujo, `Regen`, recálculo, refresco del índice o lectura del DWG sin pistas pendientes (O-01) |
| P-03 | Manejadores de coste constante, sin lecturas del DWG |
| P-04 | Drenaje solo en reposo y coalescido |
| P-05 | Índice refrescado bajo demanda y solo con el panel visible |
| P-06 | Una sola superficie de editor viva: la de la pestaña activa |
| P-07 | Nada de AutoCAD retenido, salvo valores `ObjectId` o handle dentro de su sesión |
| P-08 | Selección: trabajo proporcional a las referencias seleccionadas |
| P-09 | Selección (N) virtualizada |

**Medición:**
- **Escenarios:** los nueve del brief («PERFORMANCE EVIDENCE»).
- **Instrumento:** tiempos y memoria del proceso en el registro existente del Plugin (I-03), activados con un interruptor del panel en
  memoria, sin tocar configuración del host.
- **Responsables:** el Worker implementa la instrumentación; el Owner ejecuta los escenarios en Smoke-1, Smoke-2 y la OV final y entrega
  los registros.
- **Umbrales:** ninguno antes de la línea base (brief). Después, el Coordinator y el Owner los fijan con una A-n.

| # | Escenario | Gate | Cuándo |
|---|---|---|---|
| 1 | Panel cerrado (línea base) | F1 | Smoke-1 |
| 2 | Panel abierto y ocioso | F1 | Smoke-1 |
| 3 | Latencia de selección de un rack | F1, F2 | Smoke-1 y OV final |
| 4 | Cambio de rack | F3 | OV final |
| 5 | Cambio de pestaña | F3, F5 | Smoke-2 y OV final |
| 6 | 10 pestañas ligeras abiertas | F3 | OV final |
| 7 | Inventario grande | F2 | DWG reales del Owner |
| 8 | Selección grande en AutoCAD | F2, F6 | OV final |
| 9 | Cambio de documento activo | F1, F7 | Smoke-1 y OV final |

### D-19 — Smoke temprano y matriz OV

Desarrollado en §12.

### D-20 — Fronteras de cierre de ID3

- **ID3 Foundation** es alcance de esta iniciativa:

  | Elemento | Decisión | Gate |
  |---|---|---|
  | Navegar entre varios racks | D-05, D-07 | F2 |
  | Conservar borradores | D-10, D-14 | F4 |
  | Sesión de edición persistente | D-08, D-10 | F3, F4 |

- **Edición por lotes** (propiedades comunes, validación conjunta y mutación atómica de varios racks): fuera de esta iniciativa salvo Freeze
  explícito. El brief admite dejarla para una entrega posterior.
- **Clasificación esperada al cierre:** `PARTIAL`, con ese residual exacto. Los puntos de extensión que lo preparan son el conjunto estable
  de Selección (N) (D-09) y los borradores por RackId (D-10). La clasificación definitiva se decide en el cierre y la acepta el Owner.

### D-21 — ADR propio y complementos

- **ADR propio** (borrador en el anexo A):
  - el archivo nace como `propuesto` en la primera tarea de F1, tras el Freeze y antes de implementar ([WORKFLOW](../WORKFLOW.md) §8 y
    §11.4);
  - su número se asigna entonces, contra `origin/main` y las ramas activas;
  - solo el Owner lo acepta.
- **ADR-0010:** se **complementa**, sin reemplazarlo ni modificarlo, como ya hizo ADR-0042. Se añade la vía del panel, con base vigente,
  todo o nada y sin Insertar. RACKEDITAR conserva íntegras sus decisiones.
- **ADR-0032 D6:** se **complementa** con las fronteras del panel. D5 y D6 no cambian.
- **Forma:** los ADR aceptados son inmutables y solo admiten una nota posterior fechada ([adr/README](../adr/README.md)). Las dos notas
  (anexo B) se escriben al aceptarse el ADR nuevo, en el commit de cierre.
- **ADR-0029:** sin modificación. El ADR nuevo extiende su contrato funcional a las superficies alojadas: D7 y D8 aplican a pestañas y
  borradores, D11 se respeta y el censo D1 por clases `Window` no cambia.
- **ADR-0006, ADR-0009, ADR-0019 y ADR-0044:** se consumen sin cambios.

### D-22 — Persistencia y frontera de autoridad

- El panel no escribe nada en el DWG fuera de Actualizar, y Actualizar usa los builders y `Compose` existentes, con preservación por vista
  (O-11).
- No hay campos persistidos nuevos, ni escrituras en el NOD ni XData del panel, ni archivos aparte del registro, ni escrituras en el
  registro de Windows o el perfil desde el código de RackCad.
- El panel **no estampa** identidad en sobres sin Id y **no inventa** RackId (INV-16).

### D-23 — Puntos de extensión preparados, no implementados

- Regiones de inspector en la pestaña y en Selección (N), para el resumen de ID20, ID28, ID29, propiedades personalizadas, variables de
  proyecto y diagnósticos de rack o vista.
- Borrador por lotes (residual de ID3).
- Insertar y `BindingIntent` desde el panel.
- Restauración entre arranques.
- Índice incremental.

Ninguno se implementa aquí.

## 9. Comparación con el estado actual (resumen)

| Aspecto | Hoy (Discovery) | Con el panel (V1) |
|---|---|---|
| Ventanas | modales, 20 `ShowModalWindow` | panel modeless + los modales clásicos sin cambio |
| Eventos | ninguno | un puente con una suscripción por fuente |
| Borrador | vive y muere con la ventana | vive por (documento, RackId) hasta cerrarlo |
| Comprobación de base | ninguna | relectura cruda en la transacción de escritura |
| Atomicidad de Actualizar | por vista | todo o nada en la vía del panel |
| Faltantes de biblioteca | invisibles en Actualizar | visibles en la vía del panel |
| Sobres sin Id | Actualizar estampa GUID | el panel no los edita; el editor clásico conserva su conducta |

## 10. Invariantes y obligaciones de prueba

Ninguna se ha ejecutado. La columna «RED esperado» describe el fallo que la prueba debe mostrar antes de implementar el comportamiento
(LIFECYCLE §7, punto 2). «Simulado» significa una fuente de eventos, un lector o un escritor falsos, sin AutoCAD.

| ID | Invariante | Prueba (clase) | RED esperado | Gate |
|---|---|---|---|---|
| INV-01 | Panel ocioso: cero trabajo sin pistas (O-01, P-02) | Core con puente simulado: N eventos ajenos y reposos sin pistas → cero llamadas a lectura, resolver y `Regen` del puerto | con un drenaje ingenuo por reposo, se cuentan lecturas | F1 |
| INV-02 | Una suscripción por fuente, ninguna por pestaña o editor (O-02) | Core/UI: abrir 10 pestañas → recuento de suscripciones constante; guarda de fuente: `+=` sobre eventos de AutoCAD solo en la carpeta del puente | el recuento crece con las pestañas en una implementación por pestaña | F1, F3 |
| INV-03 | Sin bucle de selección; navegación idempotente (O-07) | Core: la acción Seleccionar emite el evento de vuelta → el contexto no cambia y no hay segunda escritura de selección | un contexto que reescribe la selección vuelve a navegar | F1, F2 |
| INV-04 | Una pestaña dirty nunca se reemplaza en silencio (O-03) | UI (STA): preview editada → seleccionar otro rack → la editada sigue, fijada | la preview se reemplaza | F3, F4 |
| INV-05 | Cambiar de pestaña, rack o documento, u ocultar el panel, no escribe (O-04) | UI/Core con puerto simulado: el cambio ocurre y hay cero escrituras | la aserción sobre la pestaña activa falla mientras el cambio no exista | F4 |
| INV-06 | Un borrador obsoleto no sobrescribe (O-05) | Core con lector y escritor simulados: diferencia en miembros, en un sobre o en una dependencia → No aplicado — Conflicto y cero escrituras | sin compuerta, se escribe | F4 |
| INV-07 | Documentos aislados (O-06) | Core: el mismo RackId en dos documentos → borradores, pestañas y contextos independientes | una clave solo por RackId mezcla sesiones | F1, F4 |
| INV-08 | Sin reentrada en el Actualizar propio (O-09) | Core: pistas durante la mutación propia → un único refresco posterior, sin segundo Actualizar ni conflicto | sin ámbito de mutación propia, la pista se trata como cambio externo | F4 |
| INV-09 | Cerrar sin intención no escribe (O-10) | existente: `RichEditorCloseContractTests.NoCloseRouteMaterialisesAnything`; nueva para pestaña y panel | la existente sigue en verde; las nuevas fallan mientras no exista el cierre de pestaña | F3, F5 |
| INV-10 | Desconocidos y metadatos por vista preservados en el Actualizar del panel (O-11) | Core: sobres con `ExtensionData` distintos por vista → cada vista conserva los suyos tras el Actualizar del panel | un recorrido que reutilice el sobre elegido para todas las vistas | F4, F5 |
| INV-11 | Un fallo no se informa como éxito, ni un commit como «no aplicado» (O-12) | Core con escritor simulado: fallo inyectado en PREPARE, MUTATE y POST → los resultados de D-13 | un resultado binario | F4 |
| INV-12 | Todo o nada en el authored de la vía del panel | Core con escritor transaccional simulado: fallo en la vista k → cero escrituras authored; host: fallo inyectado en F4 | con commit por vista quedan vistas escritas | F4 |
| INV-13 | La UI no referencia AutoCAD (ADR-0006) | existente: `UiSystemBoundaryGuardTests`, ampliada a las carpetas nuevas | guarda existente; sigue en verde | F1 |
| INV-14 | No se retiene ninguna transacción, objeto abierto ni tipo de AutoCAD fuera de su operación | guarda de fuente sobre el puerto (sin tipos de AutoCAD) y prueba Core del ciclo abrir → operar → cerrar | la guarda falla si el puerto expone un tipo de AutoCAD | F1 |
| INV-15 | Solo la pestaña activa está materializada | UI: N pestañas → una sola instancia de superficie | una implementación que conserva los árboles | F3, F5 |
| INV-16 | El panel no estampa identidad ni inventa RackId | Core: sobre sin Id → entrada de diagnóstico, no editable y sin clave RackId | sin la entrada de diagnóstico, el sobre desaparece o recibe una clave | F2, F4 |
| INV-17 | Lo ilegible, divergente o de versión futura nunca se oculta ni cuenta como éxito (ADR-0044 §8) | Core: cada caso → diagnóstico visible y no editable | un filtro que lo omite | F2, F4 |
| INV-18 | El texto pendiente inválido se conserva en las fronteras no bloqueables y aborta las bloqueables (ADR-0032 D6) | UI (STA): campo inválido + cambio de documento → el texto vuelve a su caja; + cambio de pestaña → la acción se aborta | el texto se pierde o se auto-repara | F4, F5 |
| INV-19 | RACKEDITAR sin cambio observable | caracterización por sistema antes de extraer su superficie (cierre, intención y escrituras del puerto) | caracterización: fija lo actual y está en verde antes y después | F4, F5 |
| INV-20 | El cierre de un documento con borradores dirty nunca los pierde en silencio | Core: política de veto en el primer intento y descarte con aviso en el segundo; host en OV-21 | sin política, se descartan sin aviso | F4 |
| INV-21 | El panel no persiste nada fuera de Actualizar | guarda de fuente: el puerto no expone escrituras salvo Actualizar; host en Smoke-1 | la guarda falla si el puerto expone otra escritura | F1, F4 |

Las pruebas de host (H-LOCK, transacción anidada, veto en `QUIT`, undo) no sustituyen a las de Core/UI ni al revés. AGENTS manda sobre la
forma de la evidencia.

## 11. Plan de gates

Los gates siguen la sugerencia del brief, ajustada a [LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7. Cada gate tiene un resultado observable
y verificable. No hay micro-gates.

| Gate | Resultado verificable | Entra con | Cierra con | Hito del Owner |
|---|---|---|---|---|
| **F1** — Workspace Foundation | `RACKPANEL` muestra el panel; contexto del documento activo; selección fina (Ninguno, Uno, Selección (N), Sin identidad, Diagnóstico) sin enumeración global; puente único; cero escrituras; instrumentación; ADR `propuesto` | Freeze AGREED por Coordinator y Architect; estado `I-64.yml`; precondiciones de delegación (§13) | INV-01, INV-02, INV-03, INV-07, INV-13, INV-14 e INV-21 en verde; CI exacta; revisión del Coordinator | **Smoke-1** (§12) |
| **F2** — Inventario y navegación | navegador del documento (lista y búsqueda) con Kind, vistas y estado; Seleccionar, Ver, Centrar y Localizar vista; invalidación y refresco bajo demanda; diagnósticos | contrato de snapshot de I-63 **integrado en `origin/main`** y rebase de I-64 sobre él | INV-03, INV-16 e INV-17; mediciones 7 y 8 en DWG del Owner | — |
| **F3** — Sesión tipo navegador | pestañas preview y fijadas, cierre sin escritura, registros ligeros, solo la activa materializada (pestaña de resumen) | F1 | INV-02 con pestañas, la regla de preview de INV-04 (sin borradores), INV-09 e INV-15; mediciones 4 y 6 | — |
| **F4** — Borradores y Actualizar, con piloto | `DraftSession`, dirty, cambios sin escritura, obsolescencia, Actualizar todo o nada, reconciliación explícita, cierre de documento; **Push Back editable en el panel** | F3 y **Smoke-1 PASS** | INV-04..INV-08, INV-10..INV-12 e INV-16..INV-20; caracterización del piloto; H-LOCK, transacción anidada, veto y undo en el host | **Smoke-2** (§12) |
| **F5-A** — Editores con shell | Selectivo, Dinámico y Cantilever editables en el panel | F4 y **Smoke-2 PASS** | por sistema: caracterización, ida y vuelta, INV-10, INV-18 e INV-19 | — |
| **F5-B** — Editores sin shell | Cabecera y Cama editables en el panel | F5-A | lo mismo que F5-A | — |
| **F6** — Multiselección e ID3 | Selección (N) completa; clasificación de ID3 y puntos de extensión | F2 | INV-02 en Selección (N), sin pestañas; informe de ID3 | — |
| **F7** — Rendimiento y regresión | los nueve escenarios caracterizados; regresión; guía de validación manual actualizada (último gate de implementación, WORKFLOW §11.4); matriz OV preparada | F1..F6 | mediciones; evidencia exigida por AGENTS | — |
| **READY** | READY-01..09 (LIFECYCLE §8) | F7 | `FINAL_CANDIDATE_SHA` | **OV final** |

- **Dependencia de I-63:** F2 espera a que el contrato de I-63 esté integrado (MASTER-I63-I64-01). F3 y F4 no dependen del snapshot: usan el
  contexto de selección y la autoridad de pertenencia existente. Si F2 tiene que esperar, el Coordinator puede resecuenciar con una A-n sin
  cambiar resultados (LIFECYCLE §6). F6 necesita el índice de F2.
- **Smoke-1** es condición de entrada de F4 y de F5, los gates que escriben el DWG o integran editores (brief, «OWNER SMOKE EARLY»). F2 y F3,
  de solo lectura y navegación, pueden avanzar mientras se espera la ventana de host. Si Smoke-1 falla en algo de F1, el Coordinator decide
  qué se rehace.
- **Smoke-2** es condición de entrada de F5: no se expanden los editores antes de validar el piloto.
- **Caracterización antes de migrar** (ADR-0029 D13): en F4 para el piloto y en F5 para cada sistema.
- **Conformidad final:** READY-06, no un gate.

## 12. Smoke temprano y matriz OV (D-19)

### 12.1 Reglas de los hitos del Owner

Fijadas por MASTER-I63-I64-01, con la petición de I-52 registrada en la evidencia §13:

- se hacen en la **estación principal del Owner**, con AutoCAD 2025;
- solo **después de cerrar F1** y **después de un RELEASE explícito de la ventana de host de I-52**. Antes, I-64 avisa a I-52 por el canal
  entre sesiones y no se solapa con sus sesiones;
- **una sola sesión de AutoCAD**;
- I-64 **no cambia** perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL ni la configuración de I-52, ni instala plugins;
- **si cargar el DLL exige modificar la seguridad, STOP** al Owner o al CAD manager;
- cada hito es **intermedio** y necesita su propio Candidato completo: suite de UI completa en local y CI del SHA exacto
  ([LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §8; [guía de validación](../guias/validacion-manual-autocad.md) §2). No sustituye al
  `FINAL_CANDIDATE_SHA`;
- se carga el DLL Debug del worktree de I-64 con su SHA-256 registrado (guía §2 y §3);
- el Worker nunca usa AutoCAD; los hitos los ejecuta el Owner.

### 12.2 Smoke-1 (tras F1)

| ID | Comprobación |
|---|---|
| S1-01 | El panel queda abierto mientras se dibuja |
| S1-02 | Zoom y encuadre funcionan con el panel abierto |
| S1-03 | Los comandos normales de AutoCAD funcionan (por ejemplo LINE, MOVE, COPY, ERASE, UNDO) |
| S1-04 | Seleccionar un rack en el dibujo lo refleja en el panel; varios dan Selección (N); un bloque sin identidad o ilegible da su diagnóstico |
| S1-05 | Cambiar de documento cambia el contexto sin mezclarlo, y al volver se recupera |
| S1-06 | Escribir en el panel y volver a la línea de comandos sin perder teclas ni capturar la entrada (O-08) |
| S1-07 | Con RACKEDITAR abierto el panel no interfiere, y al cerrarlo sigue sincronizado |
| S1-08 | Abrir, usar y cerrar el panel sin Actualizar no modifica objetos del dibujo (el bit 1 de `DBMOD` no cambia) |
| S1-09 | Panel ocioso: el registro no muestra lecturas, resolver ni `Regen` (escenarios 1, 2, 3 y 9 de D-18) |
| S1-10 | El panel no pide persistir estado; cualquier cambio de configuración observado se informa (`UNKNOWN` de D-01) |
| S1-11 | Con `PICKFIRST=0`, el panel explica que no hay sincronización (sin cambiar la variable; solo si el Owner ya la tiene así) |

### 12.3 Smoke-2 (tras F4, con el piloto Push Back)

| ID | Comprobación |
|---|---|
| S2-01 | Editar en el panel deja el borrador dirty («Nombre *») y no toca el dibujo |
| S2-02 | Cambiar de pestaña y de documento conserva el borrador |
| S2-03 | Actualizar redibuja todas las vistas y deja el borrador limpio |
| S2-04 | Un cambio externo (RACKEDITAR sobre el mismo rack) produce Conflicto y cero escrituras; las dos opciones de reconciliación funcionan |
| S2-05 | UNDO después de Actualizar revierte el dibujo y el siguiente Actualizar da Conflicto |
| S2-06 | Cerrar el documento con un borrador dirty veta el primer intento con aviso, y el segundo descarta |
| S2-07 | RACKEDITAR de Push Back sigue igual que antes |
| S2-08 | Escenario 5 de D-18 (cambio de pestaña) |

### 12.4 Matriz OV final (sobre `FINAL_CANDIDATE_SHA`)

Todos los escenarios se asignan a la unidad **I-64** (única unidad). Se suman al checklist de la guía; no lo reducen (LIFECYCLE §8).

| OV | Escenario (brief, «OWNER VALIDATION») | Hito previo |
|---|---|---|
| OV-01 | Mantener el panel abierto mientras se dibuja | S1-01 |
| OV-02 | Zoom y encuadre | S1-02 |
| OV-03 | Comandos normales de AutoCAD | S1-03 |
| OV-04 | Seleccionar un rack en el dibujo y que el panel lo siga | S1-04 |
| OV-05 | Elegir un rack en el navegador | — |
| OV-06 | Seleccionar, ver y centrar sus vistas, y localizar una vista concreta | — |
| OV-07 | Varios sistemas (los seis, o los que consten en el residual) | S2 (Push Back) |
| OV-08 | Varias pestañas | — |
| OV-09 | Preview y fijado | — |
| OV-10 | Borrador dirty | S2-01 |
| OV-11 | Cambiar de pestaña sin escribir | S2-02 |
| OV-12 | Actualizar escribe | S2-03 |
| OV-13 | Conflicto por cambio externo | S2-04 |
| OV-14 | Multiselección: Selección (N) sin abrir pestañas | S1-04 |
| OV-15 | Varios documentos | S1-05 |
| OV-16 | Guardar y reabrir: el dibujo conserva lo aplicado y el panel vuelve a indexar sin datos propios en el DWG | — |
| OV-17 | Compatibilidad con RACKEDITAR | S1-07, S2-07 |
| OV-18 | Sin regresión visible (checklist de la guía) | — |
| OV-19 | Foco y teclado | S1-06 |
| OV-20 | Cerrar el panel o una pestaña no modifica el dibujo | S1-08 |
| OV-21 | Cerrar un documento con borradores dirty | S2-06 |
| OV-22 | Rendimiento percibido y registros de los nueve escenarios | S1-09, S2-08 |
| OV-23 | UNDO después de Actualizar y su detección de obsolescencia | S2-05 |

Retirar o sustituir un escenario OV exige al Owner; reasignarlo exige una A-n (LIFECYCLE §8).

## 13. Plan inicial de enrutamiento (I-61)

**Reglas:**
- Clasificación según `routing.md` §§1-3. No se elige ni sondea ningún modelo aquí.
- Las celdas se acreditan en `MainSha` cuando se delega. Una celda no acreditada queda **pendiente de decisión o de una sonda autorizada**;
  nunca se declara elegible por necesidad.
- La fricción se registra como evidencia para I-62.
- Mientras I-62 no se integre, rige I-61.

| Gate | Tarea | Clase (`routing.md` §1) | Effort semántico | Nota |
|---|---|---|---|---|
| F1 | T-F1-0: ADR `propuesto` desde el anexo A | Documentación | Routine | — |
| F1 | T-F1-1: modelo puro (sesiones, contexto, pistas, políticas) con pruebas | Implementación de pruebas | Balanced | — |
| F1 | T-F1-2: puente central con fuente simulada y guarda de fuente | Implementación transversal a capas (corta) | Deep | — |
| F1 | T-F1-3: host de paleta, raíz de UI, `RACKPANEL`, selección fina e instrumentación | Implementación transversal a capas | Deep; puede ser Long-horizon | el Worker no usa AutoCAD |
| F2 | índice runtime sobre el snapshot y navegación | Implementación transversal a capas | Deep | depende de I-63 |
| F3 | pestañas, preview y materialización perezosa | Implementación de pruebas y mecánica | Balanced | — |
| F4 | `DraftSession` y máquina de estados | Implementación transversal a capas | Deep | — |
| F4 | Actualizar del panel (PREPARE, MUTATE, POST) | Implementación transversal a capas | Deep | toca primitivas de redibujo, archivos calientes |
| F4, F5 | caracterización por sistema | Caracterización | Balanced | ADR-0029 D13 |
| F4, F5 | adaptador por sistema (ventanas de 547 a 3 713 líneas) | Implementación agéntica larga | Long-horizon → Frontera | sin celda Frontera con escritura medida: pendiente |
| F6 | Selección (N) e informe de ID3 | Implementación de pruebas | Balanced | — |
| F7 | mediciones y regresión | Caracterización | Balanced | escenarios de host: Owner |
| todos | planificación y verificación del Controller | Controller | Balanced | — |
| rondas | revisión del Architect | Revisión de arquitectura | Deep | modo e independencia declarados |

**Precondiciones de la primera delegación** (Discovery §14):
- Freeze AGREED;
- `docs/automation/state/I-64.yml` (AUTOMATION_PLAN 16.8);
- línea base nueva de `config.toml`;
- autoridades leídas en `MainSha`.

## 14. Materialidad M-01..M-08

| M | Estado | Razón |
|---|---|---|
| M-01 | **activado** | cambian los dueños de borrador, dirty, contexto, cierre, aceptación de Actualizar y comprobación de base (§2). El authored no cambia de dueño |
| M-02 | **UNKNOWN tratado como activado; resolución propuesta: no activado** | con D-12 (sin revisión persistida), D-13 (builders y `Compose` existentes, preservación por vista) y D-22 (sin campos nuevos ni estampado de identidad), el significado persistido no cambia. Deciden Coordinator y Architect |
| M-03 | **UNKNOWN tratado como activado; resolución propuesta: no activado** | RACKEDITAR y los comandos clásicos no cambian (D-16); abrir el panel sobre un DWG solo lee; los diagnósticos son UI nueva. **Riesgo residual:** la extracción de superficies (D-15), protegida por INV-19, OV-17 y la regla STOP de D-16. Deciden Coordinator y Architect; Owner si RACKEDITAR cambia |
| M-04 | **activado** | fallo nuevo en la vía del panel: todo o nada, conflicto fail-closed y veto de cierre (D-13, D-17) |
| M-05 | **activado** | contrato de superficie de editor e intención separada del cierre, que consumen los seis sistemas (D-15) |
| M-06 | **activado** | puntos de extensión del panel y contrato de adaptador (D-15, D-23) |
| M-07 | **activado** | panel, puente central y sesiones por documento: mecanismo transversal nuevo |
| M-08 | **activado** | complementos de ADR-0010 y ADR-0032 D6 (D-21) |

Hasta que el Coordinator y el Architect acepten las resoluciones propuestas, M-02 y M-03 siguen tratados como activados (LIFECYCLE §3).

## 15. Coordinación

- **I-63:**
  - MASTER-I63-I64-01 fija la frontera (D-05);
  - I-64 consume el snapshot en F2 solo cuando esté integrado, y no diseña el contrato;
  - las necesidades de D-05 son información para I-63, no un compromiso suyo;
  - al integrar se espera un conflicto textual por la adyacencia de las filas de ROADMAP.
- **I-52:**
  - su respuesta tardía queda registrada en la evidencia §13;
  - los hitos del Owner siguen §12.1;
  - RACKMIRROR está previsto como un comando sin eventos que crea un rack nuevo sin mutar el origen. Es un supuesto, no un contrato: para
    el panel sería un cambio externo que invalida el índice, sin conflicto de diseño;
  - si su implementación toca las primitivas de redibujo que usa D-13 o `EditX`, se coordina como archivo caliente al integrar.
- **I-62:** solo proceso. Si se integra antes de la primera delegación de I-64, su protocolo rige desde `MainSha`.

## 16. Incógnitas, riesgos y su tratamiento

| Tema | Estado | Cómo se cierra | Responsable | Fase |
|---|---|---|---|---|
| Teclado y foco de WPF en `PaletteSet` | UNKNOWN | S1-06; contingencia (b) de D-01 | Owner; Architect si cambia el host | Smoke-1 |
| Estado de una paleta sin `toolID` en el perfil | INFERENCE | S1-10 | Owner | Smoke-1 |
| Paleta durante un modal | UNKNOWN | S1-07 | Owner | Smoke-1 |
| Undo y grupo de un comando interno | INFERENCE | S2-05; prueba de host en F4 | Worker + Owner | F4 |
| Rollback de las transacciones anidadas, incluida la importación | INFERENCE | fallo inyectado en el host | Worker + Owner | F4 |
| H-LOCK | INFERENCE | manejador de prueba y segundo comando en el host | Worker + Owner | F4 |
| Veto durante `QUIT` y su orden con el aviso de guardar | INFERENCE | S2-06; prueba de host | Worker + Owner | F4 |
| Coste del barrido AUTH-06 al abrir y al Actualizar | UNKNOWN | medición | Worker + Owner | F4 |
| Coste del índice sobre el snapshot | UNKNOWN | escenario 7 | Worker + Owner | F2 |
| Conflictos falsos por reserialización | INFERENCE | registro de conflictos en Smoke-2 y la OV final | Owner | F4..READY |
| Contrato de I-63 sin integrar a tiempo | riesgo de plan | resecuenciar con una A-n (§11) | Coordinator | F2 |
| Ventana de host de I-52 larga | riesgo de plan | F2 y F3 avanzan; F4 espera a Smoke-1 | Coordinator | F1..F4 |
| Extracción de superficies de 3 500 líneas | riesgo de alcance | caracterización previa, adaptadores en secuencia, regla STOP de D-16 | Worker + Coordinator | F4, F5 |
| `PICKFIRST=0` | UNKNOWN | S1-11, solo si aplica | Owner | Smoke-1 |

## 17. Disposición de las decisiones de partida

| Origen | Disposición en esta Proposal |
|---|---|
| Aceptación de D1-R1 y cierre de Discovery | base de diseño: Discovery D1-R1 (blob `3565015c`) |
| MASTER-I63-I64-01, fundación mínima | D-05, §2, §15 |
| MASTER-I63-I64-01, propiedad del índice runtime | D-05, D-06, D-07, D-08 |
| MASTER-I63-I64-01, el snapshot no decide métricas ni sustituye la pertenencia | D-05, D-11 |
| MASTER-I63-I64-01, no inventar RackId ni resolver ni persistir | D-05, D-22, INV-16 |
| MASTER-I63-I64-01, F1 sin la fundación y F2 con el contrato integrado | §11 |
| Smoke: estación, momento, sesión única, sin cambios de seguridad y STOP | §12.1 |
| Registrar la respuesta tardía de I-52 | evidencia §13 |
| Escalada EXP-02 (regla de pertenencia) | D-11 |
| UNKNOWN de Discovery §15 | §16, D-11..D-13, D-17 |

## Anexo A — Borrador del ADR propio

> Borrador para el archivo `docs/adr/NNNN-workspace-persistente-modeless.md`. Nace como `propuesto` en F1 tras el Freeze (D-21); su número
> se asigna entonces. Solo el Owner lo acepta.

**ADR-NNNN: Workspace persistente y modeless de RackCad**

- **Estado:** propuesto.
- **Contexto:**
  - RackCad edita con ventanas modales, y AutoCAD no dispara eventos mientras dura un modal;
  - el borrador vive en la ventana y el host interpreta la intención tras el cierre;
  - Actualizar no compara con la base, escribe por vista y oculta faltantes (Discovery I-64 §§1, 3, 9.12);
  - el Owner pide un panel persistente con borradores por rack y Actualizar como única frontera de escritura.
- **Decisión:**
  1. **Host:** un `PaletteSet` por proceso, que aloja un control WPF; sin estado persistido por RackCad.
  2. **Controlador y sesiones:** uno por proceso, con una sesión por documento; toda clave es (documento, RackId); nada cruza documentos.
  3. **Eventos:** un único puente con una suscripción por fuente. Los eventos son pistas, nunca autoridad, y se drenan coalescidos en reposo.
  4. **Inventario:** el índice runtime de navegación es del panel. La enumeración lógica neutral es un snapshot común cuyo autor es I-63. La
     pertenencia para mutar es `RackSiblingMembership` (Freeze I-55 V5 §4.1).
  5. **Pestañas:** registros ligeros, con una sola superficie materializada; una preview que se fija al editarse.
  6. **Borradores:** `DraftSession` por (documento, RackId) como evolución de `RackEditorSession` y de los estados existentes (ADR-0029
     D11); el texto pendiente nunca se pierde.
  7. **Actualizar desde el panel:** relectura cruda de la base en la transacción de escritura; todo o nada; resultados que distinguen
     conflicto, fallo sin escritura, aplicado con avisos y estado posterior desconocido.
  8. **Coexistencia:** RACKEDITAR y los comandos clásicos no cambian.
  9. **Migración:** escalonada por adaptadores con un contrato común; caracterización antes de cada sistema.
  10. **Persistencia:** ninguna propia; ni identidad estampada ni RackId inventado.
- **Relación con otros ADR:**
  - complementa ADR-0010 (vía del panel) y ADR-0032 D6 (fronteras del panel), sin modificarlos;
  - extiende a las superficies alojadas el contrato de ADR-0029 (D7, D8 y D11), sin cambiar su censo D1;
  - consume ADR-0006, ADR-0009, ADR-0019 y ADR-0044.
- **Alternativas consideradas:**
  - `ShowModelessWindow` (contingencia);
  - host híbrido;
  - revisión persistida en el sobre;
  - huella normalizada;
  - `FindRackBlocks` para el panel;
  - eventos como autoridad;
  - seis reescrituras paralelas de editores.
- **Consecuencias:**
  - el primer uso de eventos de AutoCAD en el producto;
  - conflictos falsos posibles por reserialización, a cambio de no sobrescribir nunca en silencio;
  - dos semánticas de Actualizar convivientes (la clásica y la del panel) hasta que el Owner decida unificarlas;
  - Insertar y `BindingIntent` siguen en el editor clásico.

## Anexo B — Notas posteriores previstas (se escriben al aceptarse ADR-NNNN)

- **ADR-0010:** «AAAA-MM-DD — Complementado por ADR-NNNN. ADR-0010 permanece `aceptado` y no es reemplazado. ADR-NNNN añade una vía de
  apertura, el panel de RackCad, cuyo Actualizar exige una base vigente, escribe todo o nada y no ofrece Insertar. Las decisiones de este
  registro para `RACKEDITAR` siguen vigentes sin cambio.»
- **ADR-0032:** «AAAA-MM-DD — Complementado por ADR-NNNN. En una superficie alojada en el panel de RackCad, cambiar de pestaña, de rack o
  de documento, ocultar el panel y Actualizar son fronteras transaccionales con las fases de D6. En las fronteras que RackCad no puede
  bloquear, el texto pendiente se conserva sin auto-reparar. D5 y D6 no cambian.»
