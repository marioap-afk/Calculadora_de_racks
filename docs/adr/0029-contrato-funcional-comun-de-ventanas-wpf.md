# ADR-0029: Contrato funcional común de ventanas WPF

- **Estado:** **aceptado**
- **Fecha:** 2026-08-07 (propuesto); 2026-08-07 (aceptado por el dueño del repo)
- **Decisores:** dueño del repo (aceptó el 2026-08-07); Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-39 (paraguas) / I-39A `architecture/contrato-funcional-ventanas-wpf`
- **No reemplaza a ninguna ADR.** **Complementa**
  [ADR-0019](0019-shell-visual-de-editores-por-composicion.md), que decide **cómo se compone lo
  visual**; ésta decide **cómo se comporta lo funcional**. ADR-0019 permanece `aceptado` e íntegro:
  ninguna de sus seis reglas se reabre.

## Aceptación del Owner (2026-08-07)

- **Decisor:** dueño del repositorio.
- **Gate:** `owner-decision`, resuelto junto a `owner-validation` y `autocad`, con el precedente de
  ADR-0023 en I-36D: un ADR que gobierna lo que se ve no se acepta a ciegas, sino después de ver la
  ventana real.
- **Veredicto normativo registrado:** `OWNER_APPROVED_I39A_MANUAL_VALIDATION`.
- **SHA técnico aprobado:** `16178dfb9c5871a4321d69594a26f67200f28c2f`.
- **Evidencia:** [checklist de validación manual](../automation/evidence/I-39A-checklist-validacion-manual.md)
  — ronda 1 parcialmente rechazada por un único defecto de espaciado, ronda 2 aprobada sobre el
  candidato corregido.
- **Alcance de la aceptación:** la aceptación fija las trece decisiones de abajo como normativas para
  toda la línea I-39. **No** autoriza por sí sola ninguna migración: I-39B, I-39C e I-39D siguen
  necesitando su propia fila, su contrato y su gate.

## Contexto

ADR-0019 unificó la composición visual de los editores ricos por slots y migró el Dinámico y el
Selectivo. Lo que no existe es un contrato de **comportamiento**, y la auditoría de I-39 lo midió
sobre `main`.

RackCad tiene **29 clases concretas derivadas de `System.Windows.Window`** en `src/`: 28 productivas
y `RackDialogWindow`, que es infraestructura. Un censo por nombre de archivo no las encuentra
—`SafetyPerPostWindow` está declarada dentro de `SelectiveSafetyWindow.cs`— y un censo por `x:Name`
tampoco distingue el **tipo** `RackCad.UI.Controls.PreviewCanvas`, que tiene **un** consumidor
productivo, de las diez ventanas que dibujan sobre un `Canvas` ordinario llamado igual.

Las incoherencias no son de estilo. `IsDefault` está presente en unas ventanas, ausente en otras y
**prohibido por comentario** en el Selectivo, con su causa escrita: Enter disparaba el recálculo y
revertía la edición de celda recién tecleada. `RackPushBackSystemWindow` es la **única** de las nueve
ventanas de rack sin `IsCancel` en su botón Cerrar, de modo que Escape no la cierra; y es también la
única con un modelo explícito de cambios pendientes, `RackModuleEditSession.HasPendingChanges`, que
es **local al editor de módulo** y que ningún `Closing` consulta. Los dos hechos juntos impiden
corregirlo mecánicamente: añadir `IsCancel` sin política de cierre convertiría Escape en un descarte
silencioso justo en la única ventana con cambios pendientes.

La infraestructura escrita para esto **no se adoptó**. `EditorAction`, `EditorAction.DisabledReason`,
`EditorActionBar`, `EditorStatusPresenter` y sus cuatro severidades tienen **cero consumidores
productivos**: las ventanas migradas ponen botones crudos y replican a mano la regla del tooltip
visible mientras el botón está deshabilitado. `RackDialogWindow` no tiene **ninguna** subclase
productiva mientras diez diálogos reconstruyen su par Aceptar/Cancelar.

El contrato de tamaño se incumple **sin producir el tamaño escrito**: las cuatro ventanas de
componente Cantilever aplican el estilo del editor rico y declaran `Width`/`Height` locales sin
mínimos propios, de modo que el mínimo heredado clampea y los cuatro anchos declarados quedan
inertes. La infraestructura transversal filtra hacia sistemas: `Editor/RecomputeGate.cs` declara un
`using` hacia el namespace del Selectivo, y `RecomputeDebouncer` y `DispatcherRecomputeScheduler`
hacia el de la cabecera. Y `tests/RackCad.UI.Tests` **no ejercita** Enter, Escape, foco inicial,
orden de tabulación ni cierre por sistema en ninguna ventana.

La decisión restringe trabajo futuro en toda la capa de UI y es cara de revertir una vez adoptada por
ventanas de miles de líneas: cumple los criterios **1 y 2** de [`docs/adr/README.md`](README.md).

## Decisión

### D1 — La unidad del inventario es el tipo, no el archivo

El censo de ventanas enumera **clases concretas derivadas de `System.Windows.Window`**, incluidas las
construidas en código y las declaradas dentro de un archivo con otro nombre. Ninguna regla, prueba o
documento identifica una ventana o un control por coincidencia de `x:Name`.

### D2 — Cuatro arquetipos, asignación obligatoria

**A — editor rico de sistema**; **B — editor acotado con preview**; **C — diálogo de configuración
transaccional**; **D — ventana utilitaria**. Toda clase del censo pertenece a exactamente uno. La
ausencia de matriz no saca a una ventana de A. El arquetipo B **no se llama «editor de componente»**:
sus consumidores no son necesariamente componentes persistentes ni miembros del BOM. Los casos
discutibles se registran con su motivo. `RackDialogWindow` es infraestructura y no cuenta como
producto.

### D3 — Estados ortogonales, no un enum lineal

El comportamiento se describe con ejes independientes: **cambio** (no aplicable, limpio, sucio);
**entrada** (intacta, incompleta, inválida, válida); **computación** (ociosa, calculando, resuelta,
no resoluble, fallida); **preview** (D4); y **salida** (bloqueada, lista). Un diseño puede estar
simultáneamente modificado, con un campo inválido y mostrando un resultado válido anterior sin que
haya ninguna recomputación en curso. Éstos son nombres de **semántica observable**: los tipos
internos que la implementen son decisión de implementación.

### D4 — El preview tiene dos ejes: autoridad y frescura

**Autoridad** declara de qué habla la imagen: derivada del borrador capturado, representación de un
contexto ya resuelto por el agregado padre, o resultado estático de consulta. **Frescura** declara si
corresponde a la captura actual: actual, último válido obsoleto, no disponible, o fallida.

Un preview de **contexto resuelto** puede seguir visible mientras el borrador es inválido **sin
convertirse en «último válido obsoleto»**: no es un residuo, es otra cosa, y debe identificarse como
representación del contexto resuelto y no de la captura todavía no aplicada. Un preview marcado
**último válido obsoleto** no habilita ninguna acción que materialice. **Una ventana no está obligada
a implementar estados que hoy no exhibe.**

### D5 — Cinco grados en un valor capturado

Texto candidato; valor candidato parseado; valor aplicado al borrador; resultado resuelto; resultado
previamente resuelto. **Una entrada inválida no sobrescribe en silencio un valor aplicado válido.**
El momento de aplicación —al salir del campo, al confirmar, o inmediato para selectores discretos— lo
declara la ventana; no aplicar nunca es una opción válida.

### D6 — Las acciones declaran semántica, no etiqueta

`Aceptar`, `Aplicar`, `Insertar`, `Actualizar`, `Guardar`, `Actualizar vista`, `Restaurar`,
`Cancelar`, `Cerrar` y la consulta o exportación de BOM. **Ninguna ventana está obligada a ofrecerlas
todas.** Cada acción que ofrezca declara: precondiciones, efecto transaccional, si cierra, qué
resultado deja al llamador, si persiste, si materializa, y **el motivo visible cuando está
bloqueada**. `Restaurar` declara además a qué línea base vuelve. Una acción importante deshabilitada
sin motivo es una violación del contrato; una acción habilitada sin efecto es una violación mayor.

### D7 — Cierre, Escape y Enter

`Enter` solo activa una acción por defecto **segura y contextual**; dentro de una celda en edición o
de un campo multilínea confirma la celda o inserta línea, nunca la ventana. **Escape no puede
provocar pérdida silenciosa de cambios.** `IsCancel` no se añade mecánicamente a una ventana con
edición pendiente. El cierre por botón, por Escape, por `Alt+F4` y por el botón de sistema atraviesa
**la misma política**. Ningún camino de cierre inserta, actualiza ni guarda.

### D8 — Dirty pertenece a un ámbito, no a la ventana

El estado sucio es propiedad de un **ámbito transaccional editable**. Una ventana puede tener varios,
y su política de cierre **agrega** los ámbitos relevantes. «No aplicable» es un valor legítimo.

### D9 — Ownership, ubicación, foco y tamaño

Una ventana modal recibe `Owner` cuando existe una ventana padre WPF. `CenterOwner` es la norma para
modales; cualquier otra ubicación se documenta con su motivo. El foco inicial y el orden de
tabulación son **deterministas** y no recaen en una acción destructiva ni bloqueada.

Sobre el tamaño:

> El contrato de tamaño se define por arquetipo cuando sus necesidades difieren. Un arquetipo no
> hereda implícitamente restricciones de tamaño de otro. En particular, el arquetipo B no hereda los
> mínimos del editor rico A.

**No se fija todavía una resolución ni un DPI mínimos** como decisión arquitectónica: se caracteriza
y se valida la accesibilidad real de acciones y diagnóstico, sin recorte funcional en las condiciones
validadas.

### D10 — Recomputación observable, estrategia libre

Se declaran cuatro semánticas: inmediata; coalescida por alcance; diferida o con debounce; y las dos
invariantes **una operación agregada causa como máximo una recomputación** y **un resultado obsoleto
no sustituye a uno más reciente**. Qué clase interna las implementa es decisión de cada ventana: este
ADR **no obliga** a unificar los mecanismos existentes.

### D11 — Adoptar antes que abstraer

Ante una necesidad ya cubierta por infraestructura escrita —`EditorAction` y su motivo,
`EditorActionBar`, `EditorStatusPresenter` y sus severidades, `RackDialogWindow`, el tipo
`PreviewCanvas`, `NumericField`, `CatalogCombo`, `SelectionMatrix`, `StructuralSectionPicker`— se
**adopta o se evoluciona** esa infraestructura. **Crear un segundo modelo paralelo está prohibido.**
La adopción es **gradual**, no mecánica, y puede ser **parcial** cuando una sustitución completa
cambiaría comportamiento observable. Recíprocamente, y en continuidad con ADR-0019 D5: adoptar un
shell o este contrato **no exige** sustituir los controles de captura existentes de una ventana.

### D12 — Ninguna infraestructura compartida conoce un sistema

`Shell/`, `Controls/`, `Editor/`, `Preview/` y `Themes/` no referencian `RackSystemKind` ni ningún
namespace de sistema, ni siquiera en un `using` que solo sirva a un comentario. Un shell del
arquetipo B vive en un namespace transversal; una fachada compatible en la ruta de un sistema es
**transitoria y declarada**, con su retirada asignada a una subiniciativa concreta.

### D13 — Adopción incremental, verificada, y sin cambio de producto

La adopción se hace por subiniciativas con contrato propio, empezando por un piloto de bajo riesgo.
Una migración **fija primero el comportamiento observable actual con pruebas de caracterización** —
incluidos Enter, Escape, foco inicial, tabulación y caminos de cierre— y después migra; **la
caracterización no se edita para que pase**, y **un cambio funcional no se disfraza de
caracterización**. Cambiar un comportamiento observable —habilitar o deshabilitar una acción, mover
un foco, alterar un tamaño— **no es consecuencia automática de este ADR**: exige decisión del Owner,
y mientras no exista se conserva el comportamiento actual y se registra la deuda.

## Alternativas consideradas

- **Un único enum lineal de estado (`Clean/Dirty/Invalid/Computing/Ready`).** Descartada: los
  conceptos se solapan. Push Back demuestra que hay cambios pendientes con captura válida y sin
  cómputo en curso; el inspector estructural demuestra que hay entrada inválida con valor aplicado
  válido. Un enum plano obliga a inventar estados compuestos y a re-decidir su precedencia en cada
  ventana.
- **Una jerarquía de `Window` base por arquetipo.** Descartada por la misma razón que ADR-0019
  descartó la herencia para los editores ricos: fija el comportamiento en el ancestro y obliga a
  `protected virtual` por cada variación, degenerando en ramas por sistema dentro de la base. El
  contrato se expresa por composición y helpers pequeños.
- **Un ViewModel universal o MVVM global.** Descartada: exige reescribir de una vez los code-behind
  de las ventanas más grandes, contra el principio 2 del ROADMAP («nada de big-bang») y el 3
  («strangler»). Este contrato no la impide: es un paso hacia ella.
- **Un framework de pruebas de UI (FlaUI, Appium, `xunit.stafact`).** Descartada por **ADR-0012**
  (cero NuGet en producto) y porque el repositorio ya resuelve STA con su propio runner.
- **Corregir los defectos puntuales sin contrato**: añadir `IsCancel` a Push Back, poner tooltips
  donde falten. Descartada: sin política de cierre con cambios pendientes, `IsCancel` en Push Back
  convierte Escape en un descarte silencioso. El defecto es una consecuencia de la ausencia de
  contrato, no su causa.
- **No hacer nada.** Descartada: es el statu quo que produjo `IsDefault` prohibido por comentario,
  once chromes ensamblados a mano, infraestructura escrita sin un solo consumidor y cero cobertura de
  interacción.

## Consecuencias

- **Positivas.** Una sola gramática que el usuario aprende una vez y que deja de depender de qué
  ventana abrió. El contrato pasa a ser **verificable**: cada regla tiene prueba, y las incoherencias
  dejan de descubrirse en el gate manual del Owner. La infraestructura ya pagada empieza a usarse en
  vez de duplicarse. El censo por tipo impide que una ventana quede fuera de toda regla, como ocurrió
  con la declarada dentro del archivo de otra.
- **Negativas, y asumidas.** La adopción toca ventanas grandes con riesgo de regresión, mitigado con
  caracterización previa, guardas y validación del Owner sobre el DLL Debug del SHA exacto. Queda
  **deuda temporal declarada**: entre subiniciativas conviven ventanas con contrato y sin él, y una
  fachada compatible vive en la ruta de un sistema hasta que su subiniciativa la retire. Fijar el
  comportamiento actual por caracterización **congela también defectos conocidos** —una acción que
  nunca se deshabilita, un preview que se vacía, una ventana sin foco inicial— hasta que el Owner
  decida cambiarlos.
- **A vigilar.** Que ningún namespace de sistema se filtre a la infraestructura compartida, con el
  antecedente ya medido de `RecomputeGate`. Que la fachada compatible no se vuelva permanente. Que
  adoptar el contrato **no** derive en una sustitución no escalada de controles de captura. Que las
  pruebas de interacción no degeneren en aserciones estructurales frágiles. Que el contrato de tamaño
  del arquetipo B no se resuelva heredando los mínimos del editor rico.
- **Lo que este ADR NO decide.** No reabre ADR-0019 ni su migración progresiva. No decide MVVM. No
  decide unificar los mecanismos de recomputación. No fija resolución ni DPI mínimos. No autoriza
  cambiar geometría, BOM, persistencia, wire format, catálogos ni bloques DWG. No autoriza corregir
  el defecto de Push Back. No adopta `RackDialogWindow` como ancestro de los **editores ricos** —eso
  lo prohíbe ADR-0019 D2 y sigue vigente—; su posible papel en el arquetipo C es una decisión
  separada de I-39D.

## Referencias

- Contrato: [`docs/initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md`](../initiatives/I-39A-contrato-funcional-piloto-editor-acotado.md).
- Decisiones vinculantes del Owner: [`docs/automation/decisions/I-39.md`](../automation/decisions/I-39.md).
- Censo medido: [`docs/automation/evidence/I-39A-censo-ventanas.md`](../automation/evidence/I-39A-censo-ventanas.md).
- ADRs relacionados: [ADR-0019](0019-shell-visual-de-editores-por-composicion.md) (complementado),
  [ADR-0012](0012-producto-sin-dependencias-nuget.md),
  [ADR-0023](0023-geometria-visual-derivada-perfiles-s.md) (advertencia que el piloto muestra),
  [ADR-0028](0028-cantilever-persistencia-vistas-editor-y-dibujo.md) D4.
- Fundación previa: I-14 (controles), I-15 (`RackEditorSession`), I-20/I-21 (estados puros), I-24
  (pruebas de editores), I-30/I-31 (shell visual), I-37D (shell de componentes y sus guardas).
- ROADMAP Fase 3, filas I-39 e I-39A.
- Criterios de creación: [`docs/adr/README.md`](README.md), «Cuándo crear un ADR», puntos 1 y 2.
