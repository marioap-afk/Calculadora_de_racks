> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: paquete v1 del contrato de PB-T1 (I-64 F4B): el estado editable de Push Back sin pérdidas fuera de la ventana

Eres el redactor del paquete de contrato de la tarea más delicada de F4B. Trabajas **en solo lectura** y **no emites nada**. La sesión principal te da el contexto completo. El paquete pasará después por una revisión independiente adversarial.

## Lo que pide el Owner (literal, [OD], fila de PB-T1 en el §5 del paquete de la fase)

> ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.

**El contenido que la fase asigna a PB-T1** (§5 del paquete de la fase):
- el estado de la ventana pasa a Application, sin pérdidas y sin alias;
- la ventana lo usa en modo CLASSIC sin cambio observable;
- copiador y comparador completos: módulos, cola y A/B;
- el mapa de propiedad (OB-05.1);
- la extracción del criterio de bloqueo de salida (`RackPushBackSystemWindow.Composite.cs:778-782` en la base de la fase; mide su línea actual) para que PB-T3 lo use sin duplicarlo.

**Las demás obligaciones que te tocan** (léelas en el paquete de la fase): OB-03.1 (el rechazo del piloto sigue hasta que PB-T1 pruebe la ida y vuelta sin pérdidas; quitarlo es de PB-T3 o PB-T4, no de PB-T1), OB-05.1, H-PB-27 (el copiador del piloto pierde y `ComposeForWrite` repone desde la base), OB-T1B.1 a OB-T1B.3, R-PB-01, R-PB-02, R-PB-05 y R-PB-09.

## Las decisiones del Owner que condicionan PB-T1 (F4B-E09)

Literales en `<scratchpad>\hs65\pbt1\owner-pbt1-pre-decisions.md`:
- **c-02 = «Como hoy, como regla explícita».** Un `HeaderPostCatalogId` fuera del catálogo pasa al poste por defecto como **regla explícita del estado**, igual que `ResolveBeamId` con las vigas. Lleva una prueba. RACKEDITAR no cambia.
- **`pb-21-cola` = «Dejarlo fuera de la garantía».** Un rack de un sentido con cola compuesta guardada no es un «supported classic rack» para la garantía sin pérdidas. RACKEDITAR sigue como hoy. El Workspace conserva `DormantTailOutOfPilotReason` en todo F4B. Declaralo en el contrato y fíjalo con una prueba.
- **Los demás casos de T-2** (normalizaciones de Application, c-12 y c-13) se reproducen tal cual.
- **H-31 y OWNER-1** se corrigen en **PB-TC2**, que va antes de PB-T1 y ahora está parada en una vuelta al Owner. **PB-T1 depende del cierre de PB-TC2.**
  - Mide el paquete sobre el HEAD actual, `eedb069c` (el producto es el GREEN de PB-TC, `4440f67d`).
  - Ancla cada hecho por **nombre de miembro** además de por línea. Marca las zonas que PB-TC2 tocará, que son `Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, `ReadCellValues`, `OptionalVal` y `Val` (ver su paquete), como «a volver a medir sobre el GREEN de PB-TC2».
- **Autorización condicional:** PB-T1 se emitirá sin volver al Owner si su revisión da 0 bloqueantes y 0 importantes sin resolver y no hay ninguna decisión material nueva. **Por eso, toda decisión material que encuentres va en el §8 de forma explícita.**

## Fuentes que tienes que leer

**El worktree,** en solo lectura: `<worktree-de-I-64>`, HEAD `eedb069c`, con `git -C`.
- **El paquete de la fase:** `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1, entero.
- **La orden del Owner:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`.
- **Las decisiones de procedimiento:** `…/owner-f4b-procedure-decisions.md` y `…/F4B-DRAFTS/f4b-dispositions.md` (F4B-E01 a F4B-E10).
- **Los mapas:** `…/F4B-DRAFTS/package/map-a-classic-write-path.md` (MA) y `map-b-editor-state-testability.md` (MB). **MB es el mapa del estado de la ventana: tu base**, pero mide cada afirmación.
- **PB-T0:**
  - el paquete `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`;
  - el cierre `…/F4B-PBT0-CHARACTERIZATION/task-close-package.md`, con los arrastres M-01, M-04, D-3, D-5 y la observación de CT-16;
  - la entrega `…/1/R20261117T185616Z-3c1b/worker-handoff.json`, con T-1 (el censo control → prueba), T-2 y T-5;
  - sus 21 CT, en `tests/RackCad.UI.Tests/PushBackFullEditorCharacterization*.cs` y `tests/RackCad.Tests/PushBackFullEditorCharacterization*.cs`.
- **PB-TC:** el paquete `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`, el cierre `…/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md` (arrastres M-02 a M-05) y las pruebas `tests/RackCad.UI.Tests/PushBackDeclaredCorrection*.cs`.
- **PB-TC2:** el paquete `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md`, con su M-07, el arrastre a PB-T1 del §10.
- **El expediente de la vuelta:** `…/F4B-DRAFTS/pbt1/ob065-dossier.md`, sobre todo el §3 (huecos de red) y el §4 (las guardas de T1B y de texto, con las condiciones para que sigan en verde).
- **T1B:** `docs/initiatives/I-64-f4-t1b-gate-contract-package.md` y sus pruebas.
- **El piloto del Workspace:**
  - `src/RackCad.Application/Systems/PushBack/Pilot/` (el materializador, el copiador o comparador y el lector de intenciones);
  - el núcleo de borradores `src/RackCad.Application/Workspace/Drafts/`, con su guarda de que no nombre PushBack;
  - `PushBackEditorState` y `PushBackCompositeEditorState`, que ya viven en Application.
- **El protocolo:** `docs/AUTOMATION_PLAN.md` §16; `docs/automation/agent-execution/README.md`; los esquemas; el catálogo de modelos.
- **Las plantillas de contrato:** los contratos de F4 con una API nueva de Application y su RED, en `docs/automation/evidence/I-64-pilot/F4-T*/coordinator/gate-contract.json` y sus paquetes `docs/initiatives/I-64-f4-*`. Mira cómo se hizo el RED cuando la API nueva aún no existe: las pruebas tienen que fallar **por aserción**, no por compilación. Usa la forma ya aceptada en I-64.

## Qué tiene que contener el paquete v1

1. **Los hechos medidos.**
   - **Hoy:** qué estado editable vive en la ventana y qué ya vive en Application. Por ejemplo `generalPallet`, `advanced`, los portadores de PB-TC, `firstLevelDatum`, `loadedDimensionViews`, `loadedDimensionStyle`, `sourceProject`, la sesión de módulos, la identidad y nombre, `composite`, la selección y el modo de lado.
   - **El flujo:** quién siembra, quién lee y quién escribe cada pieza.
   - **Los alias:** dónde la ventana comparte referencias con el modelo cargado.
   - **Los controles fuente de verdad que quedan,** tras PB-TC y PB-TC2.
   - **El criterio de bloqueo de salida.**
   - **El copiador y el comparador del piloto,** y qué pierden.
2. **El diseño.**
   - **El tipo o tipos de estado en Application** (`Systems/PushBack`, R-PB-05):
     - qué contienen;
     - cómo se cargan desde un diseño con la misma semántica que `LoadExisting`, `LoadDesignForNew`, `LoadNew` y «Restaurar»;
     - cómo producen el diseño de salida con la misma semántica que hoy;
     - cómo se copian sin alias y cómo se comparan.
   - **El mapa de propiedad** (OB-05.1): campos del editor frente a sobre o infraestructura.
   - **La ventana** pasa a leer y escribir ese estado. Los controles solo lo editan. Es un cambio mínimo, **sin MVVM ni estética**.
   - **Las costuras internas que usan las pruebas** (`Session`, `State`, `LastComputation` y demás) se conservan como reenvíos.
   - **Los constructores y las llamadas de los cuatro sitios** no cambian (OB-01.1b).
   - **El modo WORKSPACE no se añade:** es de PB-T2.
   - **La regla explícita de c-02 y la exclusión de `pb-21-cola`.**
   - **Si la tarea es demasiado grande para un Worker** (R-PB-01, R-PB-09), propón dividirla en subtareas secuenciales (PB-T1a, PB-T1b…), cada una con su 1/3 (OB-10.1), y di qué cubre cada una.
3. **La red de pruebas.**
   - **Lo que tiene que seguir en verde sin tocarlo:** T1B, PB-T0, PB-TC, PB-TC2 y las demás de la ventana.
   - **Las pruebas nuevas** (CT-PBT1-xx):
     - la ida y vuelta sin pérdidas de los racks soportados, sobre el corpus y sobre diseños fabricados con módulos, cola y A/B;
     - el copiador sin alias;
     - el comparador;
     - el mapa de propiedad;
     - la regla de c-02;
     - la exclusión de `pb-21-cola`;
     - la equivalencia de la ventana sobre el estado frente a la de hoy;
     - los huecos de D-5 y de M-03 que el expediente manda cubrir antes de mover el estado.
   - **El RED y el GREEN,** con su forma exacta.
   - **El censo control → prueba actualizado.**
4. **Las guardas de T1B y de texto** (OB-T1B.3): para cada guarda que dependa del diseño (según el §4 del expediente: S-9, CT-22, CT-25 y G-10, entre otras), si tu diseño la mantiene en verde y con qué invariante. **Si alguna no puede seguir en verde, es una decisión del Owner antes del contrato:** dilo en el §8.
5. **El contrato propuesto,** con el patrón de PB-TC y PB-TC2:
   - `TaskId` `F4B-PBT1-LOSSLESS-STATE` (o uno por subtarea);
   - `AllowedWriteScope`, `ForbiddenWriteScope`, `RequiredTests` e invariantes mecánicos;
   - `ExpectedEvidence`;
   - `StopConditions`, con las estándar, las de [DP] transcritas literales como en PB-TC2 y las propias;
   - `EligibleCells`;
   - la línea base de la CI.
6. **Los riesgos y las lecciones de PB-T0, PB-TC y PB-TC2:**
   - sin latitud para el Worker;
   - evidencia con rutas relativas y `TreeSha` real;
   - corridas temporales con `Assert.Fail`;
   - las ventanas con el aviso de descarte sustituido;
   - nunca la suite completa de UI en local;
   - MAX_PATH;
   - los nombres de sustituciones `SUSx-0n`;
   - la variación de nc1 de PB-TC.
7. **El §8: las decisiones del Owner.** Las ya tomadas, con su cita, y **cualquier decisión material nueva** que el diseño exija. Por ejemplo:
   - una guarda de T1B que no puede seguir en verde;
   - un comportamiento observable que cambia;
   - una pérdida que no se puede reproducir sin fingir;
   - la división en subtareas, si cambia el plan de la fase.
8. **El §9: los límites,** con lo que no pudiste medir.

## Reglas

- **Solo lectura:**
  - sin compilar;
  - sin ejecutar pruebas;
  - sin escribir en el worktree ni en `artifacts/`;
  - sin operaciones de Git que escriban.
- **Python** solo de lectura, escrito con la herramienta de archivos y **nunca con `-c`**.
- **`gh`** solo de lectura.
- **Sin AutoCAD.**
- **Lo que se custodiará:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

## Salidas, en `<scratchpad>\hs65\pbt1\`

- `I-64-f4b-pbt1-gate-contract-package.md`, la v1;
- `pbt1-measurements.md`, con las mediciones y sus órdenes.

**Tu respuesta final** es corta:
- las rutas;
- si propones dividir PB-T1, y en qué;
- los recuentos principales;
- **las decisiones materiales nuevas para el Owner**, si las hay;
- lo que no pudiste medir.
