> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v1

**Estado:** v1, borrador para la revisión independiente adversarial. **No se emite:** la emisión de PB-T4 no está autorizada (§8.2, Q3). Se midió por objetos git en `7dfc7c21`, la BASE de PB-T3c (F4B-E47). PB-T3a y PB-T3b están CERRADAS (F4B-E43 y F4B-E46) y PB-T3c está EMITIDA, con su Worker en curso. Lo que depende del GREEN de PB-T3c lleva el marcador **`<tras el cierre de PB-T3c>`**, y todo se vuelve a medir sobre ese cierre antes de emitir. No declara GATE PASS.

**Los hechos** están en `pbt4-measurements.md` (R-0 a R-12), con la fuente de cada uno. El guion `measure_pbt4.py` escribe `measure_pbt4-out.txt`.

**Abreviaturas:**
- [P], el paquete de la fase (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1);
- [OD], `F4B-DRAFTS/coordinator/owner-f4b-disposition.md`;
- [P2] y [P3], los paquetes de PB-T2 (v2.9) y PB-T3 (v4.7);
- DISP, `F4B-DRAFTS/f4b-dispositions.md`;
- WHC, `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- FVM, `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- PV, `src/RackCad.UI/Workspace/WorkspacePanelView.xaml(.cs)`;
- W, `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`;
- PP y UR, `Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs` y `…UpdateRun.cs`;
- WR, `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
- WCo, `src/RackCad.Plugin/Workspace/WorkspaceController.cs`.

---

## 0. En una página

**Lo que pide el Owner** ([OD], fila de PB-T4): «Add "Editar todo" to the Workspace only after PB-T1/T2 establish the draft-based editor seam. Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»

**Lo que falta en `7dfc7c21`** (R-2 a R-6):
- el panel edita Push Back solo con el piloto (la escala), sobre `DraftSession<PushBackPilotDraftState>`;
- el modelo completo de PB-T3 existe y nadie lo llama;
- la ventana tiene su modo WORKSPACE y nadie la abre en él;
- el puerto de la ventana es sobre `DraftSession<PushBackEditableState>`, no sobre el borrador completo;
- «Conservar mi borrador» es todavía `KeepPilotEditsOn`, sin presentación;
- el rechazo A/B sigue en el panel.

**«Editar todo» exige que el borrador del panel sea el completo.** Convertir un borrador del piloto en uno completo sería la «state translation logic» que [OD] prohíbe. Por eso PB-T4 cambia el tipo del borrador del panel, y eso hace que las pruebas que describen el panel con el piloto dejen de describirlo: es la lista que A-T3-1 manda devolver al Owner (§8.2, Q1).

**Dos subtareas secuenciales,** cada una con su 1/3 (D-PBT4-01):
- **PB-T4a, `F4B-PBT4-FULL-DRAFT-PANEL`:** el panel sobre el borrador completo, sin «Editar todo». El borrador, la lectura (con OB-03.2), el campo de escala con la validación única, «Actualizar» por `ExecuteFull` y «Conservar mi borrador» con los pasos 1 a 3 de D-PB-05, OB-05.4 y `KeepOn`. Application, la UI del panel y una línea del Plugin.
- **PB-T4b, `F4B-PBT4-EDIT-ALL-WINDOW`:** «Editar todo». El gesto y su botón, la frontera del texto pendiente (R-PB-10), K = 1, el puerto generalizado (A-T3-11 con A2-1), el abridor del Plugin con la guarda de OB-01.3, «Cerrar» con una sola pregunta (A-T2b-OWNER-01), y el resumen y «Modificado».

**Tres preguntas para el Owner,** en una sola vuelta (§8.2):
- Q1: qué hace PB-T4 con el piloto;
- Q2: cómo se presenta el conflicto;
- Q3: la emisión.

---

## 1. Autoridades

| Autoridad | Lo que manda aquí |
|---|---|
| **[OD]**, la fila de PB-T4 (OD:219-223) y D-PB-01 (P:133-157) | Un solo editor completo, modal, con borrador dentro y fuera y cero escrituras en el DWG. El panel es la autoridad del «Actualizar» final. Ninguna invocación WORKSPACE llega al camino clásico. Sin traducción de estado |
| **[OD] D-PB-05** (P:267-299) | Los pasos 1 a 7. «Conservar» es una elección destructiva informada, sin diff JSON genérico |
| **[P]** OB-01.1 a OB-01.6, OB-03.2, OB-05.1 a OB-05.5, R-PB-10, §4, §7.3 (T1B), §7.4 (RACKEDITAR) y §9 | R-1 de `pbt4-measurements.md` |
| **[P2] §10**, A2-1 a A2-13 (P2:1477-1493) | El envoltorio del puerto, la guarda de OB-01.3, R-PB-10 y K = 1, D2, la escala, M11, el resumen y `NaN` |
| **[P3] §10**, A-T3-1 a A-T3-11 (P3:1857-1872) | Conectar el modelo completo, OB-03.2, `KeepOn`, D2, el origen, el asiento, las constantes y el adaptador del puerto |
| **F4B-E28 Q3** (`C/owner-pbt2-decisions.md:27-33`) | La validación única, en PB-T4 |
| **F4B-E38** (`C/owner-pbt2b-close-decisions.md:27-36`) | A-T2b-OWNER-01: una sola pregunta al «Cerrar» |
| **F4B-E29** (`C/owner-pbt3-d1-decision.md`) | D-1, «Un solo código»; E-3, E-5 y E-6 si PB-T4 quita el piloto |
| **F4B-E11 D2** con L-5 | Las dos referencias compartidas no se mutan; si hiciera falta, STOP y se reabre L-1 |
| **F4B-E24** | «Basta así»: el rechazo AUTH-13 del panel sigue |
| **La nota del Owner sobre la interfaz** (`C/owner-note-ui-deferred.md:5`) | «la interfaz o botones extra … los arreglamos después …, lo importante es la funcionalidad»: ningún botón ni campo que no pida el contrato |
| **F4B-E03 [DP]**, E15, E21, E25, E35 Q4, E37, E42, E45 y E46 | El procedimiento (§6.0) |
| **D-PB-10** | Cada subtarea empieza en 1/3, y el intento 3 vuelve al Owner |

---

## 2. Hechos medidos en `7dfc7c21`, punto por punto

| Punto de la fila de PB-T4 o arrastre | Qué existe | Qué falta | Fuente |
|---|---|---|---|
| Abrir la ventana en WORKSPACE sobre el borrador (D-PB-01) | `ForDraft(PushBackDraftEditorLaunch)`; la ventana no se muestra ni tiene dueño | Quien la abra: el gesto, el anfitrión, el modal y el dueño | W:170-172, :296; R-5 |
| El puerto generalizado (A-T3-11, A2-1) | `IPushBackDraftStatePort` y `PushBackDraftSessionPort` sobre `DraftSession<PushBackEditableState>` | El adaptador sobre `DraftSession<PushBackFullDraftState>` que conserva `Origin`, y el envoltorio del anfitrión | `PushBackDraftStatePort.cs:8-64`; P3:1870; P2:1481 |
| El borrador del panel | `DraftSession<PushBackPilotDraftState>`, con 1 `Begin` y 9 `Find` | El borrador completo | WHC:347, :403-1909 |
| Pasos 1 a 3 y OB-05.4 | La relectura y el Sí/No en línea de «Conservar» | La presentación clasificada y la confirmación tras ella | WHC:636-694; FVM:583-639 |
| La composición de PB-T3 conectada (OB-05.5) | `ForFullWrite`, `KeepOn`, `PrepareFull` y `ExecuteFull`, sin llamadores | Conectarlos; dejar `KeepPilotEditsOn` | R-4 |
| El borrado de vistas en el dibujo | — | `<tras el cierre de PB-T3c>`: `RackPilotMutationScope : IPushBackPilotViewEraser` | UR:505-506 |
| La guarda de texto de OB-01.3 | La UI-STA y la guarda de la ventana de PB-T2b (CT-PBT2B-03 y 08) | La guarda del abridor | P2:645 |
| R-PB-10 (el texto pendiente antes de abrir; K = 1) | Las fronteras bloqueantes (TabChange, RackChange y Update); el modal del panel (`SetModalRackCadActive`) con `PassGate` y `UpdateGate` | Una frontera para «Editar todo» y alguien que entre en el modal | WHC:1184, :996, :1939; WCo:141 |
| El resumen y «Modificado» | «Modificado» y « *» desde `IsDirty` | Refrescarlos al cerrar la ventana (A2-9) | FVM:1145; `WorkspaceBrowserViewModel.cs:1297` |
| OB-03.2 | El lector completo admite A/B, sin conectar | Quitar el rechazo del panel, con su prueba | P3:402, :530; R-6 |
| A-T2b-OWNER-01 | «Cerrar» pregunta solo por el módulo si los dos están pendientes | Una pregunta que nombre los dos | W:3427-3432 |
| Q-PBT2-3, la validación única | Dos implementaciones de la regla de la escala y la prueba que las compara | Una sola | R-6 |
| D-1 (E-3, E-5 y E-6) | Los seis `Skip` de PB-T3b y E-1 a E-6 | Solo si PB-T4 quita el código del piloto (Q1) | P3:1228-1236 |
| D2 y L-5 | Las condiciones 1, 2, 4 y 8 a 10, probadas en PB-T1 a PB-T3 | Las 3, 5, 6 y 7 sobre el borrador completo del panel (A2-5, A-T3-4) | P2:755-758 |
| AUTH-13 | El rechazo en el lector del piloto, que el lector completo hereda | Nada: sigue (F4B-E24) | R-6 |

---

## 3. El diseño

### D-PBT4-01. Dos subtareas secuenciales

**Por qué dos.**
- «Editar todo» necesita el borrador completo.
- El borrador completo cambia «Conservar» (OB-05.2: `KeepOn` en el mismo paso que la presentación y la elección) y retira el piloto del panel (A-T3-1).
- Ese cambio es grande y toca muchas pruebas que ya existen. «Editar todo», en cambio, es casi todo código nuevo.
- Separarlos deja en PB-T4a todo lo que sustituye pruebas existentes y en PB-T4b una tarea casi solo aditiva.

**La secuencia:**
1. PB-T4a se emite sobre el cierre de PB-T3c.
2. PB-T4b, sobre el cierre de PB-T4a.

Las delegaciones son sucesivas, sin rebase, cada una con su presupuesto de 1/3 (OB-10.1).

**Si la revisión pide partir más PB-T4a:** el corte natural es presentar «Conservar» aparte. No vale, porque OB-05.2 obliga a cambiar la semántica en el mismo paso que la presentación.

### D-PBT4-02. El borrador del panel es el completo (PB-T4a; A-T3-1)

Sujeto a la respuesta de Q1. El texto es el de la opción recomendada, (A).

**El anfitrión:**
- **`Begin`** (WHC:347): con la lectura completa y su comparador:
  `_drafts.Begin(session, rackId, full.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy)`.
  `full` es el `PushBackFullIntentResult` de `PushBackFullDraftReader.Read(scan, catalog)`.
- **Los nueve `Find<PushBackPilotDraftState>`** pasan a `Find<PushBackFullDraftState>`. Ninguno queda con el tipo del piloto.
- **Nada traduce un estado en otro.** Un borrador del piloto no existe tras el GREEN: el registro se vacía al cerrar el documento, como hoy, y el tipo se fija en `Begin`.
- **El código del piloto se queda** en `Systems/PushBack/Pilot/`, sin que el panel lo use: el materializador, el lector, el comparador, el adaptador, `Prepare`, `Precheck` y `Execute`. Sus pruebas propias siguen en verde, y E-3, E-5 y E-6 no cambian (Q1-A).

**Lo que se quita del panel:** el camino del piloto dentro de WHC (`pilot.Read`, `KeepPilotEditsOn`, `PushBackPilotFieldAdapter`, `PushBackPilotUpdateRun.Execute(`). Las pruebas que fijan ese camino se sustituyen con registro (§4 y §8.2).

### D-PBT4-03. La lectura, OB-03.2 y la relectura con el origen (PB-T4a; A-T3-2, A-T3-8)

- **OB-03.2, explícito y con su prueba:**
  - un rack A/B (`pb-20-ab` y los demás compuestos del corpus) abre su borrador en el panel;
  - la cola aparcada (`pb-21-cola`) sigue rechazada, con el texto de hoy;
  - AUTH-13 sigue (F4B-E24): el lector completo pasa siempre primero por el del piloto (`PushBackFullDraftReader.cs:51-59`).
- **El texto del rechazo de una cola** no cambia: `OutOfPilotDiagnostic`, con su `ClassicEditorHint`.
- **La relectura de `Precheck`, de «Conservar» y de POST** busca el miembro del origen del borrador, no `ChosenReference` (A-T3-8; WHC:328, :409 y :675).
  - Cómo: la clave del origen es la de la lectura del `Begin` (`PushBackFullIntentResult.OriginKey`), y se guarda junto al borrador en el anfitrión.
  - `<a medir en la remedición>`: si `DraftSession` ya lo guarda, se usa ese dato; si no, el anfitrión lo guarda en un diccionario privado por (sesión, rack), que entra en la lista cerrada de PBT4A-C13.

### D-PBT4-04. El campo de escala y la validación única (PB-T4a; Q-PBT2-3, A2-6)

- **El adaptador nuevo** del texto pendiente: `PushBackFullFieldAdapter : IDraftPendingTextAdapter<PushBackFullDraftState>`, en `Draft/`.
  - Su clave es la de hoy, el valor de `PushBackPilotFieldAdapter.AnnotationScaleKey`, para que la UI no cambie.
  - `Apply` devuelve `new PushBackFullDraftState(copia del editable con la escala nueva, state.Origin)`, y no toca nada más del estado.
- **Una sola regla de validación:** la de la ventana. Es `NumericFieldValidation.Validate` (`src/RackCad.UI/Controls/NumericFieldValidation.cs:76`) con las reglas que `AnnotationScaleBox` declara (`RackPushBackSystemWindow.xaml:68`).
  - Application no puede llamar a la UI. Por eso el anfitrión recibe la regla por asignación, como recibe el ejecutor de la actualización (`SetPilotUpdateHost`, WCo:110):
    - una interfaz nueva en Application, `IPushBackFieldRule { PushBackFieldCheck Check(string text); }`;
    - su implementación en el Plugin, `WorkspacePushBackFieldRule`, en `src/RackCad.Plugin/Workspace/`, que llama a `NumericFieldValidation.Validate` con los parámetros de la caja;
    - el cableado: `_host.SetPushBackFieldRule(new WorkspacePushBackFieldRule())`, junto a `SetPilotUpdateHost`.
  - El adaptador nuevo no lleva copia de la regla. `PushBackFieldCheck` es un valor con `IsValid`, `Message` y `Value`; su forma exacta sigue la de `Validate` de `IDraftPendingTextAdapter<TState>` (`DraftSession.cs:108`), `<a medir en la remedición>`.
  - **Sin regla asignada**, el campo responde «No editable aquí» y no edita, como sin lector: el cableado es parte del cambio.
- **Una sola fuente de los parámetros.** La prueba CT-PBT4A-04 (UI) compara, en los diez textos de CT-PBT2B-10:
  - lo que da la regla del Plugin;
  - lo que da la propia caja `AnnotationScaleBox` de una ventana real, con sus propiedades leídas de la caja.
  Si alguien cambia la caja, la prueba falla.
- **CT-PBT2B-10** sigue en verde mientras exista `PushBackPilotFieldAdapter` (con Q1-A existe, sin uso): sigue comparando el adaptador del piloto con la caja. No se sustituye.

### D-PBT4-05. «Actualizar» por la escritura completa (PB-T4a; OB-05.5)

- **`TryUpdatePilot`** (WHC:844) conserva su nombre y su firma, para no mover G-10 ni los llamadores que fijan G-4 y `WorkspaceEditIntentGuardTests`. Su cuerpo pasa a:
  - `PushBackPilotUpdateRun.Precheck(draft)` y `PushBackPilotUpdateRun.ExecuteFull(draft, request, port)`, con `draft` de tipo `DraftSession<PushBackFullDraftState>`;
  - las mismas `TryBeginApply` y `CompleteApply`, y los mismos avisos.
- **`<tras el cierre de PB-T3c>`:** el escritor del Plugin borra los laterales obsoletos (`EraseUnit`). Antes de ese cierre, un plan con `Erasures` termina en `EraseUnsupportedReason` (UR:505-506). Por eso PB-T4a se emite sobre el cierre de PB-T3c.
- **G-4** (IntegrationGuard:500-501) exige que WHC nombre `PushBackPilotUpdateRun.Execute(` exactamente una vez. Con `ExecuteFull` ya no lo nombra, así que G-4 no puede seguir en verde: se sustituye con registro (§4, fila G-4).

### D-PBT4-06. «Conservar mi borrador»: los pasos 1 a 3, OB-05.4 y `KeepOn` (PB-T4a; OB-05.3 a OB-05.5, A-T3-3)

**La forma, en dos tiempos.** La elección explícita llega después de la presentación (OB-05.4):

1. **«Conservar mi borrador»** (FVM `RequestKeepMine`) ya no abre solo el Sí/No. Antes llama a una operación nueva del puerto, `PreviewKeepMine(EditIntentRequest)`, que:
   - **paso 1:** relee el rack actual con el origen del borrador (D-PBT4-03), con la detección de conflicto de F4;
   - **paso 2:** clasifica con `PushBackConflictClassifier.Classify(base, actual, borrador, miembros, catálogo)`:
     - `base` = `draft.InitialDraftState`;
     - `actual` = el estado de la relectura;
     - `borrador` = `draft.CurrentDraftState`;
     - `miembros` = los sobres de la relectura;
   - devuelve `EditKeepPreview`: si se puede conservar, las líneas de la presentación (D-PBT4-07) y un testigo `EditIntentWitness(Key, Text)` con la clave y el texto del sobre releído (`WorkspaceEditIntent.cs:13`).
   
   **No cambia nada del borrador.**
2. **La UI** muestra esas líneas encima del Sí/No, en el panel de la elección (`BrowserFieldChoicePanel`, PV:256), en un `TextBlock` nuevo, `BrowserFieldKeepMinePresentationText`, de solo lectura.
3. **«Sí»** (`ConfirmChoice`) llama a `TryKeepMine(request)`, con el testigo en `request.Witness`. El anfitrión relee otra vez (**paso 3**):
   - si el sobre releído no es el del testigo: **no conserva**, y responde «La versión cambió; revisa otra vez» (`KeepStaleWitnessReason`), y la UI vuelve a pedir la presentación;
   - si es el mismo: `draft.KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)`.
4. **Sin «Sí» no se escribe nada:** «No» (`CancelChoice`) deja el borrador como estaba.

**`KeepOn` en lugar de `KeepPilotEditsOn`:**
- con `KeepOn`, la base nueva es la relectura, el editable es el del borrador y el origen es el nuevo (P3:587);
- luego «Actualizar» compone con `ForFullWrite`, que cumple los pasos 4 a 7 (P3:577-583).

**Cuándo se sirve:** como hoy, en Conflict o en Dirty marcado `IsPossiblyStale`; si no, `NotNeeded(NotKeepableReason)`.

**El puerto gana dos miembros con implementación por defecto.** `IWorkspacePilotEditPort` gana `PreviewKeepMine` (y, en PB-T4b, `TryEditAll`) como miembros de interfaz con cuerpo por defecto (`=> EditKeepPreview.NotAvailable`):
- así compila sin tocarse el doble existente `WorkspacePilotEditPortDouble` (`tests/RackCad.UI.Tests/Workspace/`), que no se edita (C02);
- `WorkspaceHostCore` y la compuerta de avisos (`WorkspacePilotEditGate`) los implementan; la compuerta responde `Waiting` dentro de un aviso, como las demás;
- las pruebas nuevas usan su propio doble, en un archivo nuevo.
- `<a medir en la comprobación del §6.0>`: si alguna guarda prohíbe miembros por defecto en una interfaz, es PBT4A-C02 y vuelve al Owner.

### D-PBT4-07. La presentación (PB-T4a; Q2)

Sujeto a Q2. El texto es el de la opción recomendada, (A), «Texto por grupos».

**Una clase pura nueva** en `Draft/`: `PushBackKeepMinePresentation`, con `public static PushBackKeepMinePresentation Of(PushBackConflictClassification classification)` y las propiedades `Lines` y `IsEmpty`.

**Una línea por clase que tenga entradas,** en este orden:
1. «Cambiado fuera: <grupos>.»
2. «Cambiado por ti: <grupos>.»
3. «Cambiado en los dos: <grupos>.»

**Luego:**
- «Cambios estructurales: <grupos estructurales>.» si `Structural` no está vacío;
- «Vistas: se borrarán <n> laterales obsoletos.» si `ViewsToErase` no está vacío;
- «Vistas: <motivo>.» por cada vista de `ViewsRejected`;
- «Todas las vistas quedarían obsoletas.» si `AllViewsObsolete`.

**Los grupos** se nombran con un literal fijo por `PushBackChangeGroup`, en el orden del enumerado:
- Name: «el nombre»; Rack: «el rack»; Annotations: «las anotaciones»; Safety: «la seguridad»;
- Fronts: «los frentes»; Modules: «los módulos»; SideB: «el lado B»; Composite: «la parte compuesta»; OutsideEditor: «fuera del editor».

**Lo que no va:** ni rutas JSON ni valores (D-PB-05: sin diff JSON genérico). La lista de entradas sigue disponible para las pruebas.

**El nombre nulo pasado a `""`** (A-T3-3; V42-09 de [P3]) sale como `$.Name ChangedByUser`. Se presenta como «el nombre» en «Cambiado por ti», igual que lo escribe el escritor. No se filtra.

**Las hojas derivadas:** el clasificador no las distingue (P3:713). PB-T4 las presenta por su grupo, como cualquier otra. Separarlas sería otra clasificación, y no la pide el contrato.

### D-PBT4-08. El piloto fuera del panel, y las sustituciones (PB-T4a; A-T3-1)

**Con Q1-A, el GREEN de PB-T4a:**
- **Las pruebas existentes que fijan el camino del piloto dentro del panel** se sustituyen con registro, como D-1:
  - cada una con `Skip` y un motivo literal;
  - su equivalente sobre el modelo completo, en una clase nueva, `PushBackFullPanelEquivalenceGuardTests` (guardas) o `PushBackFullPanelEquivalenceTests` (comportamiento);
  - una entrada en `F4B-DRAFTS/superseding-characterization.md`, que escribe la sesión en el commit de emisión.
- **La lista cerrada** se mide antes de emitir (precondición 6) y va en el contrato con su total.
- **El criterio de la lista:** una prueba entra si y solo si falla en el GREEN de referencia por una de estas tres causas:
  - (i) el tipo del borrador (`Find<PushBackPilotDraftState>` o el estado del piloto);
  - (ii) el rechazo A/B que OB-03.2 quita;
  - (iii) la semántica de «Conservar» que cambia (`KeepPilotEditsOn` frente a `KeepOn`, y la confirmación en dos tiempos).
  Una prueba que falle por otra causa no entra: es la parada PBT4A-C02 y vuelve al Owner.
- **El motivo literal del `Skip`:** «I-64 F4B PB-T4a: sustituida por <equivalente> de <clase> (registro declarado superseding-characterization.md; decisión del Owner F4B-E<n>, Q1).»
- **Las guardas de PB-T3 «sin conectar»** (CT-PBT3A-12 y CT-PBT3B-13) se sustituyen igual, con su equivalente: el censo nuevo de quién nombra el modelo completo (CT-PBT4A-14). Lo prevé A-T3-3.

**El censo candidato** (R-8): 15 clases de `tests/RackCad.Tests/Workspace/` y las dos guardas de PB-T3, con 644 métodos. Es una cota superior; la mayoría no dependen del tipo del borrador.

### D-PBT4-09. «Editar todo»: el gesto, la frontera y K = 1 (PB-T4b; R-PB-10, A2-3)

- **El botón:** `BrowserFieldEditAllButton`, `Content="Editar todo"`, `AutomationProperties.Name="Editar el rack completo en el editor"`.
  - Va en el mismo `WrapPanel` de los botones del campo (PV:218-254), después de «Editar».
  - Lo maneja `OnBrowserFieldEditAllClick` con `Contained(() => _viewModel.Browser.Field.EditAll())`.
  - Es visible con `Browser.Field.ShowEditAllButton`.
  - Ningún otro botón ni campo nuevo (la nota del Owner sobre la interfaz).
- **El gesto** `EditAll()` en FVM (no en `WorkspaceBrowserViewModel`, cuya forma pública fija `WorkspacePilotCloseSyncGuardTests:442-444`):
  1. cruza la frontera nueva `PilotBoundaryKind.EditAll`, añadida al final del enumerado (`IWorkspacePilotEditPort.cs:181`; «later tasks may add values at the end»);
  2. el anfitrión la trata como bloqueante, igual que `Update` (WHC:1184):
     - un texto pendiente inválido da `Aborted`, y no se abre nada: «No se abrió el editor: <mensaje del campo>.»;
     - uno válido se confirma con el adaptador de D-PBT4-04 y se sigue;
  3. llama a `_port.TryEditAll(request)` y muestra el resultado.
- **K = 1** (R-PB-10: «Mientras el editor está abierto, el panel no edita ese borrador»): el anfitrión abre la ventana dentro del modal del panel (`SetModalRackCadActive(true)` … `false`). Con el modal activo, `PassGate` (WHC:1939) y `UpdateGate` (WHC:996) ya responden `Waiting` a todo el panel. No hace falta otro mecanismo.

### D-PBT4-10. El anfitrión y el puerto generalizado (PB-T4b; A-T3-11, A2-1)

**`TryEditAll(EditIntentRequest request)`** en WHC, con implementación por defecto en el puerto (D-PBT4-06):
- **Sin borrador:** lo empieza como «Editar» (`TryBeginEdit`) y sigue. Si `TryBeginEdit` no lo admite, devuelve su resultado (por ejemplo, la cola aparcada) y no abre nada.
- **Con borrador en Clean, Dirty o Conflict:** sigue. En Applying o Error: `Waiting` o el rechazo de hoy, sin abrir nada.
- **Sin anfitrión del editor asignado:** «No editable aquí» y nada se abre.
- **Abre:** construye el lanzamiento y llama una vez a `IPushBackFullEditorHost.ShowModal(launch)` dentro del modal del panel. A la vuelta devuelve `EditAllOutcome.Closed(isDirty)`, para el resumen (D-PBT4-13).

**El lanzamiento,** en `Draft/` y sin el anfitrión:
- `PushBackFullDraftLaunch.For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)` devuelve `new PushBackDraftEditorLaunch(draft.CurrentDraftState.Editable, draft.RackId, sourceProject, port)` (A-T3-11);
- `sourceProject` sale del sobre de `Origin` con `RackEmbedStore` y `RackProjectStore`, como lee el asiento: «el host de PB-T4 lo saca de la lectura» (P2:475);
- la ventana copia el estado al cargarlo (W:175-189), así que el lanzamiento no copia.

**El adaptador del puerto (A-T3-11),** en `Draft/`: `PushBackFullDraftStatePort : IPushBackDraftStatePort`, con `ctor(DraftSession<PushBackFullDraftState> draft, Action applied)`. `ApplyToDraft(edited)`:
- llama a `draft.SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`, **con el `Origin` actual**; otro daría un `IsDirty` falso;
- traduce `Updated` a `Applied` y `NotEditable` a `NotEditable`;
- si aplicó, llama a `applied()`.

**El envoltorio del anfitrión (A2-1):**
- `applied` es `() => NotePilotEdit(session, rackId)` (WHC:832): cada «Aplicar al borrador» pasa por el anfitrión y acaba la marca *known clean*;
- se declara la excepción de portador, como la de `TryUpdatePilot` (`WorkspaceEditIntentHostTests.cs:1039-1046`).

**`PushBackDraftSessionPort` y `LaunchFor`** (sobre `DraftSession<PushBackEditableState>`) no son el canal del panel. Se conservan sin uso, con sus pruebas de PB-T2a en verde, y no se tocan (A-T3-11 lo deja a PB-T4: se elige conservarlos, sin registro, porque nada cambia).

**`IPushBackFullEditorHost`,** una interfaz nueva en `Application/Workspace/` con `void ShowModal(PushBackDraftEditorLaunch launch);`:
- se asigna como `SetPilotUpdateHost`: `_host.SetFullEditorHost(new WorkspaceFullEditorHost(this))`, en WCo, justo después;
- su nombre no es ninguno de los términos de G-9, de G-6 ni de `WorkspaceModelBoundaryTests:41-42`.

### D-PBT4-11. El abridor del Plugin y la guarda de OB-01.3 (PB-T4b; A2-2)

**`WorkspaceFullEditorHost : IPushBackFullEditorHost`,** en `src/RackCad.Plugin/Workspace/WorkspaceFullEditorHost.cs`. `ShowModal(launch)` hace, en este orden:
1. `using (_controller.EnterPanelModal())`, el único llamador de WCo:141;
2. `var window = RackPushBackSystemWindow.ForDraft(launch);`
3. el dueño: la ventana principal de AutoCAD, como el resto de ventanas modales del Plugin, `<a medir en la remedición: el patrón exacto>`;
4. `AcApplication.ShowModalWindow(window)`.

**Lo que no hace:** ni `LockDocument`, ni transacción, ni lectura del dibujo, ni nada después de cerrar. La ventana no escribe en el DWG (OB-01.2); el DWG cambia solo con «Actualizar» del panel.

**La guarda de texto de OB-01.3** (CT-PBT4B-07), sobre los cuatro archivos del camino:
- el abridor;
- `TryEditAll` y sus ayudantes privados en WHC;
- el gesto `EditAll` de FVM;
- el lanzamiento y el adaptador de `Draft/`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada archivo y salta.

**El abridor tampoco nombra lo que prohíbe a `Plugin/Workspace/` `WorkspaceBridgeSourceGuardTests:139-142`:** `ShowModalDialog`, `ShowModelessDialog`, `ShowDialog`, `MessageBox`, `GetObject`, `OpenCloseTransaction`… `ShowModalWindow` no está en esa lista.

### D-PBT4-12. «Cerrar» con las dos cosas (PB-T4b; A-T2b-OWNER-01)

En `PendingWork()` de W (:3427), **solo en WORKSPACE**:
- **los dos a la vez:** un módulo escenificado sin confirmar y `HasUnappliedDraftChanges()` dan una sola pregunta, el literal nuevo `WorkspaceStagedAndUnappliedQuestion`: «Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?»;
- **uno solo:** la pregunta de hoy;
- **en CLASSIC** no cambia nada (T1B).

W es `UNIT_CHANGE` de PB-T4b (F4B-E25).

### D-PBT4-13. El resumen y «Modificado» al cerrar la ventana (PB-T4b; A2-9)

**Al volver `TryEditAll`,** el gesto vuelve a leer el campo (`GetField`) y refresca:
- el estado («Modificado» si `IsDirty`, FVM:1145);
- la pestaña con « *» (`WorkspaceBrowserViewModel.cs:1297`), por el camino que ya tiene `Rematerialize` (FVM:442), sin miembros públicos nuevos en `WorkspaceBrowserViewModel`.

**El resumen de la superficie** (el nombre, el `Kind` y las vistas, `BrowserRackSummary`) es del dibujo y no cambia hasta «Actualizar».

### D-PBT4-14. D2 sobre el borrador completo (PB-T4a y PB-T4b; A2-5, A-T3-4)

**Las condiciones 3, 5, 6 y 7 de D2,** sobre el borrador completo del panel y sobre el canal de la ventana, con la huella profunda de CT-PBT1B-16 (copiada, no editada):
- PB-T4a, CT-PBT4A-11: `Begin`, `SetCurrentDraftState`, `KeepMineOnCurrentVersion` con `KeepOn`, y el rebase de POST;
- PB-T4b, CT-PBT4B-11: `ApplyToDraft` del adaptador.

**El límite literal del Owner** va en PBT4x-C17: si hiciera falta mutar el rack resuelto o el informe de módulos dentro de la vida del borrador, STOP y se reabre L-1.

**Ni M11 (A2-8) ni el coste de `IsDirty`** (M15, R-T3-07) cambian:
- el anfitrión no llama a `LoadDesignEditor` ni a `RestoreEditor`, y M11 se declara no aplicable;
- el coste se mide en el Smoke-PB, sin caché.

**`IsDirty` con `NaN` (A2-13)** se declara con su línea (CT-PBT4A-12): un `NaN` en el borrador deja `IsDirty` en `true`.

### D-PBT4-15. Lo que PB-T4 no hace

- **La ventana clásica, RACKEDITAR, RPB y el menú** no cambian (OB-RE.1, T1B). W solo cambia, en WORKSPACE, la pregunta de D-PBT4-12.
- **No crea vistas** (D-PB-04; §6.4).
- **No traduce un borrador en otro** ([OD]), ni lee un `RackEmbedDocument` para inventar estado: consume `Draft/`.
- **No usa el asiento en `Recompute`** de la ventana (A-T3-9): queda fuera, con su motivo (D-PBT4-16).
- **No abre AUTH-13** (F4B-E24), ni cambia `DormantTailOutOfPilotReason`, `EditorModules` ni el camino clásico.
- **No borra el código del piloto** (Q1-A).
- **No añade botones ni campos** fuera de «Editar todo», el texto de la presentación y la pregunta de D-PBT4-12.
- **No cachea las pasadas** de un «Actualizar» (R-T3-07).

### D-PBT4-16. Los arrastres y las limitaciones, clasificados

| Arrastre o limitación | Dónde | Por qué |
|---|---|---|
| A-T3-1 (conectar; el piloto) | PB-T4a, con Q1 | D-PBT4-02 y 08 |
| A-T3-2 (OB-03.2) | PB-T4a | D-PBT4-03 |
| A-T3-3 (`KeepOn`; presentación; guardas «sin conectar»; el nombre `""`) | PB-T4a | D-PBT4-06 a 08 |
| A-T3-4 (D2, M11, M15, el coste, F4B-E30) | PB-T4a y PB-T4b (D2); M11, no aplicable; el coste y F4B-E30, al Smoke-PB | D-PBT4-14 |
| A-T3-8 (releer con el origen) | PB-T4a | D-PBT4-03 |
| **A-T3-9** (la ventana usa `Seat.Pass` en `Recompute`) | **Fuera,** a `docs/ideas-futuras.md` | CT-PBT3A-01 y CT-PBT2B-02 ya fijan que la adopción de la ventana y el asiento dan el mismo estado. Cambiar `Recompute` de W es un riesgo para T1B, y «Editar todo» no lo necesita |
| A-T3-10 (las constantes del piloto a `Draft/`) | Fuera con Q1-A: el piloto sigue | Solo hace falta si se borra el piloto (Q1-B) |
| A-T3-11 y A2-1 (el adaptador y el envoltorio) | PB-T4b | D-PBT4-10 |
| A2-2 (la guarda de OB-01.3) | PB-T4b | D-PBT4-11 |
| A2-3 (R-PB-10 y K = 1) | PB-T4b | D-PBT4-09 |
| A2-6 (una sola regla de la escala) | PB-T4a | D-PBT4-04 |
| A2-8 (M11) | Declarado no aplicable | D-PBT4-14 |
| A2-9 (el resumen y «Modificado») | PB-T4b | D-PBT4-13 |
| A2-11 (el paso 4 de D-PB-06 hasta el DWG) | PB-T4b sin AutoCAD (CT-PBT4B-10); lo físico, el Smoke-PB | §5.4 |
| A2-13 (`NaN`) | PB-T4a, declarado | D-PBT4-14 |
| A-T2b-OWNER-01 | PB-T4b | D-PBT4-12 |
| **OWNER-01 de PB-T2c** (un lado B declarado sin frentes) | **PB-T4a, una prueba** (CT-PBT4A-10) | D-PB-08 ya lo decidió: el lado B no se guarda así. El panel escribe lo que escribe CLASSIC, y el borrador conserva la declaración hasta el rebase de POST |
| **OWNER-01 de «Restaurar»** (una pasada por detrás) | **Fuera,** limitación anterior (F4B-E31) | Es de la ventana en los dos modos, y PB-T4 no toca «Restaurar». Va al Smoke-PB como observación |
| **PBT3B-IR-M01 y OWNER-2** (AUTH-13 frente a `SchemaVersion` 1.1 y nombres distintos) | **Fuera** | F4B-E24 mantiene AUTH-13; los pasos 5 a 7 siguen probados por construcción, y el Smoke-PB los observa |
| **El layout del configurador** (F4B-E30) | **Regla de §5.0** | Ninguna prueba nueva crea un `RackFrameConfiguratorWindow` real |

---

## 4. Las guardas

| Guarda | ¿Sigue en verde? | Cómo |
|---|---|---|
| **T1B** (la ventana clásica, RACKEDITAR, el Plugin clásico) | Sí | W solo cambia una pregunta en WORKSPACE |
| **PB-T2** (CT-PBT2B-03, 05, 08 y 10, y las demás) | Sí | CT-PBT2B-08 fija los once literales y las once reglas del modo; un literal nuevo no rompe las reglas `<a medir en la comprobación del §6.0>`. CT-PBT2B-10 sigue (D-PBT4-04) |
| **PB-T3 «sin conectar»** (CT-PBT3A-12 y CT-PBT3B-13) | **No** | Se sustituyen con registro en PB-T4a, como prevé A-T3-3: su equivalente es CT-PBT4A-14 |
| **F4-T4 G-4** (IntegrationGuard:487-501: WHC nombra `PushBackPilotUpdateRun.Execute(` una vez) | **No** | Se sustituye con registro en PB-T4a. Su equivalente: WHC nombra `PushBackPilotUpdateRun.ExecuteFull(` exactamente una vez, desde `TryUpdatePilot` |
| **F4-T4 G-1, G-3, G-5 a G-10** | Sí | Ningún archivo nuevo nombra sus términos (§6.0, la lección del esqueleto); `TryUpdatePilot` conserva su nombre y su llamador |
| **`WorkspaceEditIntentGuardTests`** (:711, la línea de `KeepPilotEditsOn`; :776, :805) | **No** | Se sustituyen con registro: el equivalente fija `KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)` y la relectura con el testigo |
| **Las demás guardas del panel** (Surface, BrowserHost, CloseSync, Reconciliation, Bridge, ModelBoundary, Hardening, PaletteHost, Lifecycle) | Sí, salvo lo que mida la comprobación | Los gestos nuevos van en FVM; el botón nuevo no está en las tablas de cinco y seis; el abridor va en el Plugin; Application no nombra AutoCAD |
| **El comportamiento del piloto en el panel** (R-8) | **No, en la lista de Q1** | Se sustituye con registro (D-PBT4-08) |
| **El código del piloto, probado por sí mismo** (`WorkspacePilotPrepareTests`, `WorkspacePilotUpdateRunTests`, `WorkspaceDraftPilotMaterializationTests`, `WorkspaceDraftPilotIntentReadTests`, `WorkspaceDraftPilotStateTests`…) | Sí, con Q1-A | El código no cambia; sus pruebas no pasan por el anfitrión |
| **D-1: E-1 a E-6** | Sí, con Q1-A | `Execute` y `ExecuteFull` siguen existiendo (E-5), y el piloto conserva `ViewUnavailablePrefix` (E-3) |

**Si cualquier otra prueba existente falla** en el RED o en el GREEN, es C02 o C16 y vuelve al Owner (OB-T1B.3). No hay más sustituciones que las de la lista cerrada.

---

## 5. La red de pruebas

### 5.0 Reglas y recetas comunes

**Las reglas de PB-T1 a PB-T3** (se copian en cada contrato, no se heredan):
- `[Fact]` que acaban en **un** `Assert.Equal` de las líneas unidas por `\n`, con «R», cultura invariante, GUID fijo y orden ordinal;
- un `[Fact]` por CT. **Ninguna `[Theory]` nueva:** la TRX cuenta distinto una `Theory` con `Skip` (F4B-E46);
- la regla «EXCEPCION» para toda llamada al API nuevo;
- **las pruebas de UI:**
  - en `EditorDiscardPromptCollection`, con la carga dentro del `try` y `Cerrar` en `finally`;
  - sin teclado ni foco real, sin `MessageBox` real y sin ventanas que no sean la de Push Back;
  - **ninguna crea un `RackFrameConfiguratorWindow`** (F4B-E30), y ninguna escribe en el perfil;
- sin AutoCAD, sin BOM, sin «Guardar en biblioteca» y sin «Insertar»;
- ningún fixture nuevo en `Fixtures/I64`;
- los ayudantes existentes se usan sin editarlos (`IntentKit`, `PilotKit`, `DraftKit`, `PilotUpdateScopeDouble`, `PilotEraseScopeDouble` de `PushBackFullWriteSupport.cs`, `WorkspaceHostTestSupport`, `WorkspacePilotEditPortDouble`). Lo que falte va en un archivo nuevo de la subtarea.

**El momento de cada lectura** (F4B-E30 y F4B-E33): cada línea se arma justo después de su último gesto. El borrador guarda copias, y el rebase de POST cambia `InitialDraftState` y `CurrentDraftState`.

**Los gestos que no cambian nada** (la lección M-13): una edición escribe un valor distinto del que tiene el diseño, en un campo que el ensamblador conserva. Se usan los valores medidos de PB-T3: `PalletTolerance` 4.5 y `PalletDepth` 48.5. `BeamDepth` no se edita.

**Los archivos nuevos se comprueban contra las guardas existentes antes de fijar el esqueleto** (la lección de F4B-E42 y F4B-E45). Antes de emitir, la sesión:
- compila el esqueleto exacto del RED sobre una copia exportada (fuera del worktree);
- ejecuta la suite completa de Core y los filtros de UI de la subtarea;
- barre los nombres nuevos y los textos del esqueleto contra los literales de todas las guardas que leen `src/`, como el §4.1 de [P3].

**Un solo «Actualizar» lógico:** las recetas de PB-T4b no llaman a «Actualizar» desde la ventana; solo desde el panel.

### 5.1 Lo que sigue en verde sin tocarlo

Todo lo que no está en la lista cerrada de sustituciones (D-PBT4-08 y §4). Además, en los dos contratos:
- las clases de PB-T1, PB-T2 y PB-T3;
- T1B;
- las del piloto que no pasan por el anfitrión;
- E-1 a E-6.

### 5.2 PB-T4a: la lista cerrada CT-PBT4A-01 a 16

| CT | Qué fija | Proyecto | ¿Falla en el RED? |
|---|---|---|---|
| **01** | **El borrador del panel es el completo:** `TryBeginEdit` sobre `pb-01-base` deja `Find<PushBackFullDraftState>` no nulo y `Find<PushBackPilotDraftState>` nulo. Su `Editable` es igual (comparador) al de `PushBackFullDraftReader.Read` del mismo escaneo, y su `Origin` es el texto del sobre. Línea `tipo=PushBackFullDraftState piloto=null editable-igual=True origen-igual=True` | Core | Sí: el esqueleto sigue con el piloto |
| **02** | **OB-03.2:** `pb-20-ab` abre su borrador (`editable=True`), y `pb-21-cola` sigue rechazado con `OutOfPilotDiagnostic` y su pista. Con la autoprueba: un sobre con un miembro desconocido sigue en AUTH-13 | Core | Sí |
| **03** | **El campo de escala sobre el completo:** `SetPendingText` y `CommitField` cambian solo `Editable.AnnotationScale`; el resto del estado queda igual (comparador sobre una copia con la escala de antes) y `IsDirty` pasa a `True` | Core | Sí |
| **04** | **La validación única:** en los diez textos de CT-PBT2B-10, la regla del Plugin (`WorkspacePushBackFieldRule`) y la caja `AnnotationScaleBox` de una ventana real dan el mismo `IsValid` y el mismo `Message` | **UI** | Sí: el esqueleto de la regla responde siempre inválido |
| **05** | **«Actualizar» con el completo:** con `PilotEraseScopeDouble`, la corrida de `TryUpdatePilot` pasa por `PrepareFull` y `ExecuteFull`: `Applied`, un `BeginMutation`, un `Seal`, el borrador en Clean y su base igual a la relectura de POST. Para un plan con un lateral obsoleto, una línea `EraseUnit:<clave>` | Core | Sí |
| **06** | **La relectura usa el origen** (A-T3-8): con dos referencias del mismo rack y `ChosenReference` en la que no es el origen, `Precheck` y «Conservar» releen el miembro del origen | Core | Sí |
| **07** | **Los pasos 1 y 2:** sobre `pb-20-ab`, con los frentes cambiados fuera y el nombre cambiado en el borrador, `PreviewKeepMine` da las líneas exactas de D-PBT4-07 («Cambiado fuera: los frentes.», «Cambiado por ti: el nombre.», «Cambios estructurales: los frentes.»), y el borrador queda sin cambios (comparador y huella) | Core | Sí: el esqueleto da `NotAvailable` |
| **08** | **El paso 3 y OB-05.4:** «No» deja el borrador igual. «Sí» con un testigo viejo (el rack cambió tras la presentación) da `KeepStaleWitnessReason` y no conserva. «Sí» con el testigo bueno conserva con `KeepOn`: `Initial` = la relectura, `Current.Editable` = el del borrador, `Current.Origin` = el origen nuevo, y Dirty | Core | Sí |
| **09** | **Los pasos 4 a 7 por el panel:** tras «Conservar» y «Actualizar» (doble), el sobre del miembro del plan lleva el diseño compuesto del borrador (el editor, los portadores y el nombre) y el resto del sobre del rack actual (los campos no editables, la identidad y la versión) | Core | Sí |
| **10** | **OWNER-01 de PB-T2c:** un lado B declarado sin frentes escribe el diseño sin lado B (D-PB-08), igual que CLASSIC. El borrador conserva la declaración hasta el rebase de POST, y después es la relectura | Core | Sí |
| **11** | **D2, condiciones 3, 5, 6 y 7:** la huella de las dos referencias compartidas, igual antes y después de `Begin`, `SetCurrentDraftState`, «Conservar» y el rebase. Línea `<operacion> huellas-iguales=True` | Core | Sí: el esqueleto no tiene borrador completo |
| **12** | **`IsDirty` con `NaN`** (A2-13), declarado: un `NaN` en `PalletTolerance` del borrador deja `IsDirty` en `True` aunque se vuelva a escribir el valor de la base | Core | Sí |
| **13** | **El panel sin el piloto** (la guarda): WHC no nombra `pilot.Read`, `KeepPilotEditsOn`, `PushBackPilotFieldAdapter`, `PushBackPilotStateComparer`, `Find<PushBackPilotDraftState>` ni `PushBackPilotUpdateRun.Execute(`. Nombra `ExecuteFull(` y `Precheck(` una vez cada uno, en `TryUpdatePilot`. Autoprueba: cada nombre inyectado salta | Core | Sí: el esqueleto aún los nombra |
| **14** | **El censo nuevo del modelo completo** (sustituye a CT-PBT3A-12 y CT-PBT3B-13): solo `Draft/`, PP, UR y WHC nombran los cinco tipos del modelo completo de CT-PBT3A-12 y la escritura completa de CT-PBT3B-13; ningún archivo de la UI ni del Plugin. Autopruebas en WCo, FVM y `WorkspacePilotSupport.cs` | Core | No (`ExpectRed` false) |
| **15** | **La presentación, pura:** `PushBackKeepMinePresentation.Of` sobre una clasificación fabricada con entradas de las tres clases, estructurales y vistas da las líneas exactas en el orden de D-PBT4-07, sin rutas ni valores. Y la UI no compone texto (guarda: FVM solo copia `Lines`) | Core | Sí |
| **16** | **La UI de la elección:** con un doble del puerto (nuevo), `RequestKeepMine` pide la presentación y la muestra en `BrowserFieldKeepMinePresentationText` antes del Sí/No; «Sí» pasa el testigo; sin presentación disponible, no ofrece el Sí | **UI** | Sí |

**Las clases nuevas,** separadas para que cada filtro tenga un solo `ExpectRed`:
- en `tests/RackCad.Tests/`, con `ExpectRed` true (el filtro `PushBackFullPanel`):
  - `PushBackFullPanelHostTests` (01 a 03, 05, 06 y 10 a 12: 8);
  - `PushBackFullPanelKeepMineTests` (07 a 09: 3);
  - `PushBackFullPanelGuardTests` (13: 1);
  - `PushBackFullPanelPresentationTests` (15: 1);
  - `PushBackFullPanelEquivalenceTests`, los E_A equivalentes de la lista cerrada de D-PBT4-08. Fijan el comportamiento del modelo completo en el panel, que el RED todavía no tiene, así que también fallan en el RED;
- en `tests/RackCad.Tests/`, con `ExpectRed` false: `PushBackFullModelCensusGuardTests` (14: 1);
- en `tests/RackCad.UI.Tests/`, con `ExpectRed` true: `PushBackFullPanelFieldTests` (04 y 16: 2).

**Ninguna empieza por `Workspace`**, para no entrar en el filtro `Workspace` de las listas curadas.

### 5.3 PB-T4b: la lista cerrada CT-PBT4B-01 a 11

| CT | Qué fija | Proyecto | ¿Falla en el RED? |
|---|---|---|---|
| **01** | **El botón** (guarda de texto del XAML y del code-behind): `BrowserFieldEditAllButton` una vez, dentro del panel del campo, con su `Content`, su nombre de automatización, su `Click` y su visibilidad (`Browser.Field.ShowEditAllButton`). El manejador delega en `Contained(() => _viewModel.Browser.Field.EditAll())` | Core | Sí |
| **02** | **El gesto y R-PB-10:** con un doble del puerto, `EditAll` cruza `PilotBoundaryKind.EditAll` antes de llamar a `TryEditAll`. Con texto pendiente inválido: `Aborted`, el puerto no se llama y aparece «No se abrió el editor: …». Con texto válido: se confirma y se abre | **UI** | Sí |
| **03** | **El anfitrión:** sin borrador lo empieza y abre; con la cola aparcada no abre (el rechazo de `TryBeginEdit`). En Clean, Dirty y Conflict abre; en Applying, `Waiting`. Sin anfitrión del editor asignado, «No editable aquí». `ShowModal` se llama una vez, y `SetModalRackCadActive` es `true` durante la llamada y `false` después | Core | Sí |
| **04** | **El adaptador del puerto** (A-T3-11, A2-1): `ApplyToDraft` deja el editado con el `Origin` actual; `Applied` e `IsDirty`; `NotEditable` si el borrador no es editable. Por el envoltorio, la marca *known clean* se acaba (`NotePilotEdit`) | Core | Sí |
| **05** | **K = 1:** con el editor abierto (el doble de `IPushBackFullEditorHost` mira el anfitrión desde dentro de `ShowModal`), `SetPendingText`, `CommitField`, `RequestUpdate` y `TryKeepMine` del mismo rack dan `Waiting`; después de cerrar, responden | Core | Sí |
| **06** | **El resumen y «Modificado» al cerrar:** con un aplicar dentro del doble del editor, el campo queda «Modificado» y la pestaña con « *». Sin aplicar, igual que antes | **UI** | Sí |
| **07** | **La guarda de OB-01.3,** sobre los cuatro archivos del camino de «Editar todo», con su autoprueba en negativo por cada nombre y archivo | Core | No (`ExpectRed` false) |
| **08** | **El abridor** (guarda de texto del Plugin): un `EnterPanelModal()` con `using`, un `ForDraft(`, un `ShowModalWindow(` y nada de `LockDocument`, `StartTransaction`, `GetObject` ni `Commit(`. El cableado en WCo: una asignación de `SetFullEditorHost`, después de `SetPilotUpdateHost` | Core | Sí |
| **09** | **A-T2b-OWNER-01:** con un módulo escenificado y cambios sin aplicar a la vez, «Cerrar» pregunta una vez con `WorkspaceStagedAndUnappliedQuestion`; con uno solo, la pregunta de hoy; en CLASSIC, la de hoy | **UI** | Sí |
| **10** | **De punta a punta, sin AutoCAD** (A2-11): empezar, «Editar todo» (el doble del editor aplica «+ frente» por el puerto), «Actualizar» con `PilotEraseScopeDouble`: `Applied`, el plan lleva el redibujo de las vistas del rack, el borrador queda en Clean | Core | Sí |
| **11** | **D2 en el canal de la ventana:** la huella de las dos referencias, igual antes y después de `ApplyToDraft` | Core | Sí |

**Las clases nuevas,** con un solo `ExpectRed` por filtro:
- en `tests/RackCad.Tests/`, con `ExpectRed` true (el filtro `PushBackEditAll`): `PushBackEditAllHostTests` (03 a 05, 10 y 11: 5) y `PushBackEditAllGuardTests` (01 y 08: 2);
- en `tests/RackCad.Tests/`, con `ExpectRed` false: `PushBackOpenerClassicPathGuardTests` (07: 1). Su nombre no empieza por `PushBackEditAll`, para quedar fuera de ese filtro;
- en `tests/RackCad.UI.Tests/`, con `ExpectRed` true: `PushBackEditAllWindowTests` (02, 06 y 09: 3).

### 5.4 Lo que se prueba sin AutoCAD y lo que queda para el Smoke-PB (PB-T5, D-PB-09)

**Sin AutoCAD,** en las dos subtareas: todo lo de §5.2 y §5.3, con dobles del puerto de la actualización y del anfitrión del editor.

**Para el Smoke-PB,** que no es de PB-T4:
- que el botón abre la ventana modal real sobre AutoCAD, con su dueño, y que con ella abierta no se edita el dibujo (OB-01.4);
- el borrado físico de los laterales (PB-T3c);
- el paso 4 de D-PB-06 hasta el DWG desde «Editar todo» (A2-11);
- el coste de las pasadas (R-T3-07);
- OWNER-01 de «Restaurar»;
- los pasos 5 a 7 con sobres reales (OWNER-2).

Los racks A/B fabricados se rotulan «CONSTRUCTED REPRESENTATIVE TEST CASES» (OB-09.1).

### 5.5 El RED, el GREEN y los totales previstos

**La base** es el GREEN de PB-T3c. `<tras el cierre de PB-T3c>`; prevista:
- Core 19 129 = 19 123 `Passed` + 6 `Skipped`;
- UI 2 451 = 2 430 + 21.

**PB-T4a.** Tres números se miden antes de emitir (precondición 6) y el contrato los da por rama (F4B-E46):
- **K_A:** los métodos de la lista cerrada de sustituciones. Con `Skip`, cada uno es una fila de la TRX, también una `[Theory]`;
- **D_A:** las filas que pierde la TRX porque una `[Theory]` de la lista, que daba N filas, pasa a una. Es la suma de N − 1;
- **E_A:** los equivalentes nuevos (`PushBackFullPanelEquivalenceTests`).

Si la lista tiene filas de UI, se cuentan aparte con K_A^UI y D_A^UI. La previsión es 0, porque la UI del panel se prueba con dobles.

| Rama | Core | UI |
|---|---|---|
| **RED** | 19 129 + 14 + E_A − D_A resultados = (19 123 − K_A − D_A + 1) `Passed` + (13 + E_A) `Failed` + (6 + K_A) `Skipped` | 2 453 = 2 430 `Passed` + 2 `Failed` + 21 `Skipped` |
| **GREEN** | 19 129 + 14 + E_A − D_A = (19 123 − K_A − D_A + 14 + E_A) `Passed` + (6 + K_A) `Skipped` | 2 453 = 2 432 + 21 |

En el RED, los `Skip` ya están puestos, y el «+ 1» de `Passed` es CT-14, que no falla.

**PB-T4b.** Sin sustituciones previstas: 8 CT nuevas en Core y 3 en UI.

| Rama | Core | UI |
|---|---|---|
| **RED** | base + 8 = (base `Passed`) + 7 `Failed` + `Skipped` de la base; CT-07 pasa | base + 3, con 3 `Failed` |
| **GREEN** | base + 8, todas `Passed` | base + 3 |

La base de PB-T4b es el GREEN de PB-T4a.

**La CI del RED:** `tests` y `ui-tests` en failure, `build-ui` en success y **`build-plugin` omitido** (needs: [tests, build-ui]). El GREEN, 4/4.

**Las corridas locales:**
- solo los filtros de la subtarea y la suite completa de Core;
- **nunca la suite completa de UI** en local;
- toda corrida temporal de diagnóstico, con `Assert.Fail` fuera del árbol que se entrega.

---

## 6. Los contratos propuestos

### 6.0 Común a las dos subtareas

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- `attempts` 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true;
- evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Los `TaskId`, de 24 a 26 caracteres** (MAX_PATH, R-T1-07), con el prefijo **`F4B-PBT4-`**:
- `F4B-PBT4-FULL-DRAFT-PANEL` (25);
- `F4B-PBT4-EDIT-ALL-WINDOW` (24).

El prefijo es el que liga `--task PBT4` en los sustitutos de nc1 y nc2 (`nc1_cascade_signal_substitute.py:205`; `nc2_exact_whitelist_substitute_f4b_v2.py:410`). Así se usan sin editarlos (§8.3).

**Las colecciones opacas** (F4B-E21): las ocho las pone la sesión, byte a byte; la comparación byte a byte, A1-A8 y nc4 van sobre la delegación final.

**El tamaño:** el de PB-T3c mide 54 162 bytes. Cada contrato de PB-T4 lleva solo sus CT y sus listas; la lista cerrada de sustituciones de PB-T4a va como lista de nombres completos de método, con su total.

**`ExpectedEvidence[0]`** es, **byte a byte**, el literal de PB-T3 de la lista cerrada del sustituto de nc2:

> antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;

Si no, hay que añadir un literal al sustituto y volver a revisarlo (F4B-E43).

**Las rutas `UNIT_CHANGE` y F4B-E25:**
- PB-T4a: WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, FVM, PV y WCo;
- PB-T4b: las de PB-T4a que vuelve a tocar, más W.

En una corrección, F4B-E25 se aplica tal cual.

**La comprobación antes de emitir** (precondición 6; la lección de F4B-E45): la sesión compila sobre una copia exportada el esqueleto exacto del RED y un GREEN de referencia, y ejecuta la suite completa de Core y los filtros de UI. De ahí salen:
- la lista cerrada de sustituciones de PB-T4a, sus totales K_A, D_A y E_A, y sus motivos;
- que ninguna otra prueba falla;
- las ambigüedades de sobrecarga o de reflexión por nombre.

**El GREEN de referencia es de la sesión y no se entrega:** solo mide. El Worker no lo recibe.

**Las paradas estándar** (17), copiadas de [PKC2] §6.7: S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08. P-01 nombra la Snapshot K (F4B-E37) o la vigente.

**Las de [DP], literales:**
- **PBT4x-C08 y C12:** una sola replanificación, solo por la comparación literal; una corrección que sería el intento 3 vuelve al Owner;
- **PBT4x-C11:** con los sustitutos de nc2 y nc1 vigentes:
  - nc2: `PBTx-NC2-EXACT-WHITELIST-F4B-01` v2, sha `C033E96C…`;
  - nc1: `PBT3x-NC1-CASCADE-SIGNAL-01` con PD-F4B-01;
  - nc3 no tiene sustituto (F4B-E46).

**El límite de D2:** **PBT4x-C17,** con el texto literal del Owner.

**La regla RR** (F4B-E35 Q4), con RR-1 a RR-7, C07 con su excepción, C18 y la lista cerrada de literales de RR-3 por clase, como en [P3] §6.0. **Las dos lecturas:**
- el RED de PB-T4a toca `src/` (el esqueleto);
- en PB-T4b, también.

**La lista cerrada de declaraciones nuevas** (INV-13; la lección de PBT1C-C13). Por nombre, en cada contrato:
- los tipos, miembros y constantes del diseño (D-PBT4-04 a 13);
- **los ayudantes privados admitidos por nombre** (por ejemplo, `OriginKeyOf` y `PresentKeep` en WHC; `GroupLabel` en la presentación).

Cualquier otro, público o privado, es la parada C13.

**Nada en el texto del esqueleto lo leen las guardas:**
- ni `Transaction` en `Pilot/`;
- ni `ForWrite` como token;
- ni `ShowDialog` ni `MessageBox` en `Plugin/Workspace/`;
- ni `Autodesk.` en Application.

Lo comprueba el barrido de la precondición 6.

**Las propias comunes** (con el prefijo de cada contrato):
- **C01:** escribir fuera del `AllowedWriteScope`.
- **C02:** editar, saltar, reactivar o borrar una prueba o un ayudante existente fuera de la lista cerrada de sustituciones.
- **C03:** un cambio observable de RACKEDITAR o de la ventana en CLASSIC. STOP y vuelve al Owner.
- **C05:** una CT necesitaría AutoCAD, un `MessageBox` real, una ventana real que no sea la de Push Back, teclado o foco reales, o escribir en el perfil.
- **C06:** la CI del GREEN falla fuera de las pruebas nuevas, de forma reproducible.
- **C07:** una línea esperada no se cumple en el GREEN, con la excepción de RR.
- **C09:** un fixture nuevo.
- **C10:** una prueba de UI crea un `RackFrameConfiguratorWindow`.
- **C13:** la lista cerrada de declaraciones.
- **C14:** un diseño del corpus cambia de bloqueo.
- **C16:** una prueba existente fuera de la lista falla.
- **C17:** D2.
- **C18:** RR.

### 6.1 Contrato de PB-T4a — `F4B-PBT4-FULL-DRAFT-PANEL`

- **Objective** (ASCII):
  > Conectar el panel del Workspace de Push Back al modelo completo de PB-T3: el borrador del panel es DraftSession<PushBackFullDraftState> (lectura completa, sin traducir estados; el codigo del piloto queda sin uso en el panel), los racks A/B se editan en el panel (OB-03.2) y la cola aparcada y AUTH-13 siguen rechazados, la relectura usa el origen del borrador, el campo de escala valida con la unica regla de la ventana entregada por el Plugin, Actualizar escribe con PrepareFull y ExecuteFull, y Conservar mi borrador presenta la clasificacion por grupos y exige la confirmacion despues de la presentacion, con un testigo de la version releida, y conserva con KeepOn, segun el paquete del contrato de PB-T4 v1, la decision del Owner sobre el piloto y la lista cerrada CT-PBT4A-01..16. Las pruebas que describen el piloto en el panel se sustituyen con registro, solo las de la lista cerrada. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni la ventana, ni RACKEDITAR; no se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - en `src/`: WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `WorkspaceEditIntent.cs` (los tipos de resultado nuevos), FVM, PV (XAML y code-behind), WCo; los nuevos `Draft/PushBackFullFieldAdapter.cs`, `Draft/PushBackKeepMinePresentation.cs`, `Application/Workspace/IPushBackFieldRule.cs` y `Plugin/Workspace/WorkspacePushBackFieldRule.cs`;
  - en `tests/`: las clases nuevas de §5.2; **solo para los `Skip`**, los archivos de la lista cerrada.
- **El esqueleto exacto del RED:**
  - los tipos y miembros nuevos, completos en su forma, con cuerpos inertes:
    - `PushBackFullFieldAdapter.Validate` responde inválido y `Apply` devuelve el estado;
    - `PushBackKeepMinePresentation.Of` da `Lines` vacío;
    - `PreviewKeepMine` da `NotAvailable`;
    - la regla del Plugin responde inválido;
  - WHC **no cambia en el RED** (sigue con el piloto);
  - los `Skip` de la lista se ponen ya en el RED.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackFullPanel` (CT-01 a 03, 05 a 13 y 15, más los equivalentes) | **13 + E_A** | true |
  | UI | `PushBackFullPanelFieldTests` (CT-04 y 16) | **2** | true |
  | Core | `PushBackFullModelCensusGuardTests` (CT-14) | **1** | false |
  | Core y UI | `Workspace` | a medir, sin los `Skip` | false |
  | Core y UI | Las de PB-T1, PB-T2 y PB-T3 de los contratos anteriores, con `PushBackFieldValidationConcordanceTests` | a medir | false |
  | Core | `PushBackFullDraft`, `PushBackFullWrite`, `PushBackViewErase` y `PushBackWritePathPluginGuardTests` | a medir | false |
- **Invariantes propios:**
  - **INV-PBT4A-01. Alcance:** el diff dentro del `AllowedWriteScope`. En `tests/`, solo las clases nuevas y las líneas de atributo con `Skip` de la lista.
  - **INV-PBT4A-02. RED y GREEN:** el RED es el esqueleto y las clases nuevas, con los `Skip`; el GREEN solo toca `src/`. Con RR, la cadena RR-6.
  - **INV-PBT4A-03. Sin traducción:** ningún código convierte un `PushBackPilotDraftState` en un `PushBackFullDraftState` ni al revés (CT-13).
  - **INV-PBT4A-04. Sin elección no se escribe:** ningún camino llama a `KeepMineOnCurrentVersion` sin un testigo igual al de la relectura (CT-08).
  - **INV-PBT4A-05. El piloto sin uso en el panel:** el código de `Pilot/` no cambia, salvo lo que nombre la lista cerrada (nada, con Q1-A).
  - **INV-PBT4A-06.** La presentación se compone en Application; la UI solo la muestra (CT-15).
- **`StopConditions` propias:**
  - **PBT4A-C04:** un método con `ExpectRed` pasa en el RED o falla por una excepción;
  - **PBT4A-C13:** la lista cerrada;
  - **PBT4A-C15:** una prueba que falla en el GREEN no cumple el criterio de la lista (D-PBT4-08). STOP: vuelve al Owner;
  - **PBT4A-C17:** D2.
- **`ExpectedEvidence`:** la de [P3] §6.1 en su forma (nueve viñetas), con `ExpectedEvidence[0]` literal (§6.0). Además:
  - la lista de los `Skip` con el motivo literal;
  - la entrada de `superseding-characterization.md` (de la sesión);
  - la CI del RED con `build-plugin` omitido;
  - con RR, la lectura entera de la verificación.
- **La línea base de la CI:** el GREEN de PB-T3c, `<tras el cierre de PB-T3c>`; prevista en §5.5.

### 6.2 Contrato de PB-T4b — `F4B-PBT4-EDIT-ALL-WINDOW`

- **Objective** (ASCII):
  > Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4a: un boton y su gesto, que confirma o rechaza el texto pendiente antes de abrir (R-PB-10); el anfitrion, que empieza el borrador si hace falta y abre la ventana de Push Back en modo WORKSPACE por un anfitrion del editor del Plugin, dentro del modal del panel (K = 1); el adaptador del puerto sobre DraftSession<PushBackFullDraftState> que conserva Origin y acaba la marca known clean; la guarda de texto de OB-01.3 sobre el camino de Editar todo; Cerrar con una sola pregunta si hay a la vez un modulo escenificado y cambios sin aplicar; y el resumen y Modificado al cerrar, segun el paquete del contrato de PB-T4 v1 y la lista cerrada CT-PBT4B-01..11. Ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `WorkspaceEditIntent.cs`, FVM, PV, WCo y W;
  - los nuevos `Application/Workspace/IPushBackFullEditorHost.cs`, `Draft/PushBackFullDraftStatePort.cs`, `Draft/PushBackFullDraftLaunch.cs` y `Plugin/Workspace/WorkspaceFullEditorHost.cs`;
  - las clases nuevas de §5.3.
- **El esqueleto exacto del RED:**
  - `TryEditAll` devuelve `NotAvailable`;
  - el adaptador devuelve `NotEditable`;
  - `For` devuelve un lanzamiento con `SourceProject` nulo;
  - `ShowModal` del Plugin no hace nada;
  - el literal nuevo existe y `PendingWork()` no cambia;
  - el botón no está en el XAML.
- **`RequiredTests`:**
  - Core `PushBackEditAll`: **7**, `ExpectRed` true;
  - UI `PushBackEditAllWindowTests`: **3**, `ExpectRed` true;
  - Core `PushBackOpenerClassicPathGuardTests` (CT-07): **1**, `ExpectRed` false;
  - y todo lo de PB-T4a sin `ExpectRed`, con las clases de PB-T4a.
- **Invariantes propios:**
  - **INV-PBT4B-01. Alcance.**
  - **INV-PBT4B-02. RED y GREEN.**
  - **INV-PBT4B-03. Cero escrituras:** el camino de «Editar todo» no nombra API de escritura del dibujo (CT-07 y CT-08).
  - **INV-PBT4B-04.** W solo cambia `PendingWork()` y su literal nuevo, y solo en WORKSPACE.
  - **INV-PBT4B-05.** `ForDraft` tiene exactamente un llamador en `src/`: el abridor.
- **`StopConditions` propias:**
  - **PBT4B-C03:** un cambio observable de CLASSIC;
  - **PBT4B-C04;**
  - **PBT4B-C13;**
  - **PBT4B-C17.**
- **`ExpectedEvidence`:** como en PB-T4a, sin la lista de `Skip`.
- **La línea base de la CI:** el GREEN de PB-T4a.

---

## 7. Riesgos y lecciones aplicadas

| Riesgo | Qué hace el paquete |
|---|---|
| **R-T4-01. La lista de sustituciones es grande e incierta** (644 métodos candidatos) | Precondición 6: se mide con un GREEN de referencia antes de emitir. El criterio cerrado de D-PBT4-08 y PBT4A-C15. Q1 al Owner |
| **R-T4-02. Latitud del Worker** en la presentación, los textos y los ayudantes | Literales fijos (D-PBT4-07, 09 y 12) y la lista cerrada por nombre, ayudantes privados incluidos |
| **R-T4-03. AutoCAD en el Worker** | Ninguna CT abre AutoCAD; el abridor del Plugin se prueba por texto (CT-PBT4B-08), y lo físico es del Smoke-PB |
| **R-T4-04. Las corridas de diagnóstico** | Solo con `Assert.Fail` y fuera de lo entregado (la lección de PB-T2b) |
| **R-T4-05. MAX_PATH** | `TaskId` de 24 y 25 caracteres; nombres de clase cortos |
| **R-T4-06. Un solo «Actualizar» lógico** | La ventana nunca actualiza; las recetas solo actualizan desde el panel |
| **R-T4-07. Las pruebas de UI crean ventanas reales y escriben el perfil** | C10 y la regla de §5.0: nada de configurador; solo la ventana de Push Back, cerrada en `finally` |
| **R-T4-08. Los miembros por defecto de la interfaz** | La comprobación del §6.0; si una guarda los prohíbe, C02 y vuelta al Owner |
| **R-T4-09. Las sobrecargas y la reflexión por nombre** (F4B-E45) | Nombres nuevos sin sobrecarga (`PreviewKeepMine`, `TryEditAll`, `ForFullWrite` ya existente); la comprobación del §6.0 |
| **R-T4-10. El esqueleto contra las guardas de texto** (F4B-E42) | El barrido de la precondición 6 |
| **R-T4-11. El total exacto de cada rama** (F4B-E46) | Ninguna `Theory` nueva; K_A y D_A medidos y escritos en el contrato |
| **R-T4-12. El coste de un «Actualizar» completo** (R-T3-07) | Sin caché; se mide en el Smoke-PB |
| **R-T4-13. El dueño de la ventana y el modal** | El patrón exacto se mide en la remedición; con el modal del panel, K = 1 no necesita estado nuevo |
| **R-T4-14. El cableado por asignación** (dos asignaciones nuevas en WCo) | Las guardas de los cableados de F4 (EditIntent:1174-1184) se comprueban en la precondición 6; las asignaciones van justo tras `SetPilotUpdateHost` |
| **R-T4-15. El tamaño de PB-T4a** | Si la revisión lo pide, el único corte válido está en D-PBT4-01 |

---

## 8. Las decisiones del Owner

### 8.1 Ya tomadas, que este paquete aplica

- **[OD], la fila de PB-T4,** literal arriba → todo el paquete.
- **D-PB-01, D-PB-05, D-PB-08, D-PB-09 y D-PB-10** → D-PBT4-02 a 16 y §5.4.
- **F4B-E28 Q3** («En PB-T4, con prueba ya (Recomendado)») → D-PBT4-04.
- **F4B-E38** («Corregir en PB-T4 (Recomendado)») → D-PBT4-12.
- **F4B-E29, D-1** («Un solo código (Recomendado)») → D-PBT4-05 y §4: con Q1-A, E-1 a E-6 siguen; con Q1-B, E-3, E-5 y E-6 se sustituyen.
- **F4B-E11, D2,** con L-5 → D-PBT4-14 y C17.
- **F4B-E24** («Basta así (Recomendado)») → D-PBT4-03.
- **La nota del Owner sobre la interfaz** → D-PBT4-09 y 15: solo el botón y los textos que pide el contrato.
- **F4B-E23** (BOM y biblioteca en WORKSPACE) → no cambia.
- **F4B-E21, E25, E35 Q4, E37, E42, E45 y E46** → §5.0 y §6.0.
- **PD-F4B-01** (F4B-E40, la firma nueva de nc1 para PB-T4 y PB-T5) y **F4B-E43** (el sustituto de nc2 v2) → §6.0.

### 8.2 Decisiones materiales nuevas: una sola vuelta, tres preguntas

**Q1. El piloto.**

> Hoy el panel del Workspace edita los racks Push Back con el «piloto», que solo cambia la escala de anotación. Para «Editar todo», el borrador del panel tiene que ser el completo de PB-T3. Convertir un borrador del piloto en uno completo sería la «traducción de estado» que prohibiste, así que PB-T4 cambia el tipo del borrador del panel. Al hacerlo, las pruebas que describen el panel con el piloto dejan de describirlo. Hay como mucho 644 candidatas en 17 clases; es una cota superior, y la lista exacta se mide antes de emitir. Esas pruebas se sustituyen con registro, como en D-1. ¿Qué hace PB-T4 con el piloto?

- **«Un modelo; el piloto queda sin uso (Recomendado)»:**
  - el panel usa el borrador completo para todo rack Push Back, y los racks A/B pasan a editarse en el panel;
  - el código del piloto se queda, sin que el panel lo use: sus pruebas propias siguen en verde y no se tocan las guardas de D-1;
  - solo se sustituyen las pruebas del panel que fallen por el tipo del borrador, el rechazo A/B o el nuevo «Conservar mi borrador»; cualquier otra que falle vuelve a ti.
- **«Un modelo y borrar el piloto»:** lo mismo, y además se borra el código del piloto. Sus pruebas propias también se sustituyen, y otras tres guardas de D-1 (nueve en total). El cambio es mayor y el código queda más limpio.
- **«Dos modelos hasta F5»:**
  - el piloto sigue para el campo de escala del panel;
  - «Editar todo» abre un borrador completo nuevo, leído del dibujo, solo si el del piloto no tiene cambios; si los tiene, pide actualizarlos o descartarlos antes;
  - toca menos pruebas, pero el panel tiene dos códigos y dos «Conservar mi borrador».

**Q2. La presentación de «Conservar mi borrador».**

> Antes de «Conservar mi borrador», pediste mostrar lo bastante para distinguir lo cambiado fuera, lo cambiado por el usuario, lo cambiado en los dos sitios, los cambios estructurales y lo que pasa con las vistas, sin un diff JSON. El panel mide unos 320 px y no abre diálogos. ¿Cómo lo presento, encima del Sí/No?

- **«Por grupos, en el panel (Recomendado)»:**
  - una línea por clase con los grupos afectados, por ejemplo: «Cambiado fuera: los frentes y el nombre.», «Cambiado por ti: los módulos.» y «Cambiado en los dos: el nombre.»;
  - una línea de lo estructural;
  - una de las vistas, por ejemplo: «Vistas: se borrarán 2 laterales obsoletos.»;
  - sin nombres de campo sueltos ni valores.
- **«Con cada campo y sus valores»:** lo mismo, con cada campo con su nombre en español y sus tres valores (el de antes, el de fuera y el tuyo). Es más largo y necesita una tabla de nombres para todos los campos.
- **«Solo recuentos»:** por ejemplo, «3 cambios fuera, 2 tuyos, 1 en los dos; hay cambios estructurales; 2 vistas a borrar».

**Q3. La emisión de PB-T4.**

> PB-T4 va en dos subtareas: PB-T4a pasa el panel al modelo completo, con el nuevo «Conservar mi borrador», y PB-T4b añade «Editar todo». Ninguna autorización tuya cubre PB-T4. ¿Las emito sin volver a preguntarte?

- **«Sí, con esas condiciones (Recomendado)»:**
  - PB-T4a se emite tras el cierre de PB-T3c, y PB-T4b tras el de PB-T4a;
  - cada una, si su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición no cambia nada material, y la lista de pruebas sustituidas que se mida cumple el criterio de tu respuesta a Q1;
  - vuelve a ti por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista.
- **«Solo PB-T4a; PB-T4b me vuelve»:** igual para PB-T4a; PB-T4b vuelve a ti antes de emitirse.
- **«No; cada subtarea me vuelve»:** cada una vuelve a ti, con su lista medida, antes de emitirse.

### 8.3 Lo que no es decisión

- **Las dos subtareas y su orden** (D-PBT4-01): es una forma de ejecutar la fila de PB-T4, que no cambia su contenido.
- **Los sustitutos de nc1 y nc2:**
  - PD-F4B-01 (nc1) y la v2 de nc2 cubren «PB-T4» y ligan `--task PBT4` al prefijo `F4B-PBT4-`;
  - las dos subtareas llevan ese prefijo, así que los sustitutos se usan sin editarlos;
  - es una lectura declarada: la revisión independiente la comprueba. Si la revisión o un control la contradicen, vuelve al Owner por [DP].
- **La validación única por asignación desde el Plugin** (D-PBT4-04): es la forma técnica de «dejar una sola», que el Owner ya decidió.
- **A-T3-9 fuera** (D-PBT4-16): no cambia nada visible; va a `docs/ideas-futuras.md`.
- **`PushBackDraftSessionPort` y `LaunchFor` sin uso:** no cambia nada visible.

---

## 9. Límites

- **Nada se compiló ni se ejecutó.** Todas las cifras de la CI son previsiones.
- **El worktree no se leyó.** Todo se leyó por objetos git en `7dfc7c21`, y las lecturas delegadas siguieron las mismas reglas.
- **PB-T3c no está cerrada:** el borrado en el dibujo, la base de la CI y los totales se vuelven a medir sobre su cierre.
- **La lista exacta de sustituciones** solo sale de la comprobación de la precondición 6; R-8 es una cota superior.
- **Cuatro formas exactas van marcadas `<a medir en la remedición>`:**
  - la clave del origen en `DraftSession`;
  - la forma de `Validate` del adaptador;
  - el patrón del dueño de la ventana modal del Plugin;
  - si una guarda prohíbe los miembros de interfaz con cuerpo por defecto.
- **Las preguntas Q1 a Q3** pueden cambiar D-PBT4-02, 07 y 08, y el §6.
- **Este paquete no autoriza nada.** No se emite sin la respuesta a Q3.

## 10. Lo que queda para PB-T5

- **El Smoke-PB** (§5.4), con los racks fabricados rotulados.
- **La CI exacta del Candidato.**
- **El gate-close de F4B.**
- **Las observaciones del Smoke-PB:** «Restaurar» (OWNER-01), los pasos 5 a 7 con sobres reales (OWNER-2) y el coste de las pasadas.

## 11. Las precondiciones de emisión

1. **PB-T3c CERRADA** (`<tras el cierre de PB-T3c>`), con el escritor que borra vistas.
2. **La remedición sobre ese cierre:** las anclas de `pbt4-measurements.md`, la línea base de la CI y la Snapshot vigente.
3. **La revisión independiente de este paquete** y, por subtarea, su comprobación acotada.
4. **Las respuestas del Owner a Q1, Q2 y Q3,** dentro de la `AuthorityRevision` del contrato.
5. **Las celdas** no están `STALE`, y el `config.toml` coincide con la Snapshot vigente.
6. **La comprobación sobre una copia exportada:**
   - el esqueleto exacto y el GREEN de referencia compilan;
   - la suite completa de Core y los filtros de UI dan la lista cerrada de sustituciones (con K_A, D_A y E_A) y ningún otro fallo;
   - el barrido de nombres y textos nuevos contra las guardas.
7. **La custodia, en el commit de emisión de PB-T4a:**
   - este paquete, como `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`;
   - su registro de mediciones, como `docs/initiatives/I-64-f4b-pbt4-measurements.md`;
   - la revisión, la comprobación acotada, la lista medida y la entrada de `superseding-characterization.md`.
8. **Para PB-T4b:** PB-T4a CERRADA y su remedición.
