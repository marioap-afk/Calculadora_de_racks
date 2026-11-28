%%%% OP replace titulo
%%%% ANCHOR
# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v5.4
%%%% TEXT
# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v5.5
%%%% END

%%%% OP replace s0-base
%%%% ANCHOR
**La base:** el cierre de PB-T3c, `7f0d9aa3`. **Cuatro subtareas secuenciales,** cada una con su 1/3.
%%%% TEXT
**La base:** el cierre de PB-T3c, `7f0d9aa3`. **Cuatro subtareas secuenciales** y, desde la v5.5, **una quinta, PB-T4e** (la recarga de «Editar todo», sobre el cierre de PB-T4d, `160e019d`; la 15.ª tanda del Owner), cada una con su 1/3.
%%%% END

%%%% OP after s0-tabla-fila
%%%% ANCHOR
sobre el cierre de PB-T4c (`6c2820ba`) |
%%%% TEXT

| **PB-T4e** | `F4B-PBT4-EDIT-ALL-RELOAD` (24; provisional, lo confirma la sesión) | «Editar todo» recarga, antes de abrir la ventana, un borrador Clean marcado como posiblemente obsoleto con la recarga de «Editar» (`TryReloadPilotEdit`); si la recarga no sale, no abre y el campo muestra lo mismo que «Editar» (D-PBT4-22). Sin esqueleto: el RED solo añade las dos clases de las CT | **0** (la lista vacía: ninguna prueba existente se edita, se salta ni se sustituye; medido, §4.5) | Condicional (`owner-pbt4e-list-decisions.md`, pregunta 4), con la lista vacía, sobre el cierre de PB-T4d (`160e019d`) |
%%%% END

%%%% OP after s0-decisiones
%%%% ANCHOR
y, para PB-T4c, D-S7 a D-S10 (`session-pbt4c-design-decisions.md`).
%%%% TEXT


**Sobre PB-T4e** (desde la v5.5): la 15.ª tanda del Owner (`owner-pbt4e-list-decisions.md`): «Solo la recarga», «No abrir y avisar», «Antes de PB-T5» y «Sí, con condiciones» (§8.5). La sesión decide dónde va la recarga y si se reutiliza `NotBegun` (D-S17 y D-S18, **propuestas** en D-PBT4-22 hasta que la sesión las confirme).
%%%% END

%%%% OP after s1-autoridades
%%%% ANCHOR
| **Las dos decisiones del Owner tras CA-T4d** (`owner-pbt4d-cat4d-decisions.md`, 2026-11-22, la 14.ª) | «Sustituirlas» (la 48 y la 49) y «Sustituirlos siempre» (la regla de los huecos de la cobertura ampliada) (§8.5) |
%%%% TEXT

| **La decisión del Owner sobre la recarga en PB-T4c** (`owner-pbt4c-code-decisions.md`, 2026-11-21, pregunta 2) | «Aceptarlo y corregir luego»: OWNER-02 de la revisión del código de PB-T4c, el origen de PB-T4e (D-PBT4-22) |
| **Las decisiones del Owner sobre la lista de PB-T4e** (`owner-pbt4e-list-decisions.md`, 2026-11-22, la 15.ª tanda) | «Solo la recarga», «No abrir y avisar», «Antes de PB-T5» y «Sí, con condiciones» (§8.5) |
| **Las decisiones de diseño de la sesión para PB-T4e** (D-S17 y D-S18; propuestas en la v5.5, en D-PBT4-22) | La recarga en el anfitrión y `EditAllStatus.NotBegun` reutilizado (§8.5); la sesión las confirma o las cambia al revisar y las registra |
%%%% END

%%%% OP after d01
%%%% ANCHOR
**Los TaskId** empiezan por `F4B-PBT4-` y tienen como mucho 26 caracteres (23, 25, 24 y, el propuesto de PB-T4d, 21), para los sustitutos de nc1 y nc2 (§6.0; V41-11).
%%%% TEXT


**Desde la v5.5, una quinta: PB-T4e** (V55-01), la recarga de «Editar todo», sobre el cierre de PB-T4d (`160e019d`), con su 1/3. El Owner la quiso como tarea propia (`owner-pbt4d-list-decisions.md`, pregunta 2, «Tarea propia después») y antes de PB-T5 (`owner-pbt4e-list-decisions.md`, pregunta 3, «Antes de PB-T5»): una cadena pequeña que no mezcla un cambio visible con el cierre de F4B, y el Smoke-PB ya mira el comportamiento corregido. Su `TaskId`, provisional, es `F4B-PBT4-EDIT-ALL-RELOAD` (24).
%%%% END

%%%% OP after d19
%%%% ANCHOR
- **No cambia el comportamiento de ninguna caja numérica:** `NumericFieldValidation` delega en la misma lógica (D-PBT4-06).
%%%% TEXT

- **PB-T4e no añade ningún texto, botón ni estado visible:** la recarga es la de «Editar», sin aviso, y su fallo se muestra con los textos de «Editar» (D-PBT4-22).
%%%% END

%%%% OP after d20
%%%% ANCHOR
| A-T3-11 y A2-1 | PB-T4c | D-PBT4-13 |
%%%% TEXT

| OWNER-02 de la revisión del código de PB-T4c: la recarga de «Editar todo» (`owner-pbt4c-code-decisions.md`, pregunta 2; `owner-pbt4d-list-decisions.md`, pregunta 2) | PB-T4e | D-PBT4-22 |
%%%% END

%%%% OP before s3-pbt4e
%%%% ANCHOR
### Común
%%%% TEXT
### PB-T4e

**Desde la v5.5, entera y literal** (V55-01 a V55-03): medida por objetos git en el cierre de PB-T4d (`160e019d`, F4B-E59) y compilando y ejecutando en copias exportadas de esa revisión (`/d/r71` en adelante; §6.5.1). Las citas de líneas son de `160e019d`: WHC, FVM, WEI, DS y NH no cambian entre el GREEN de PB-T4c (`7cc1076e`) y `160e019d`, y `src/` y `tests/` de `160e019d` son los del GREEN de PB-T4d (`7db6fd0c`; `git diff --stat` vacío). El texto del GREEN y del RED es el de las piezas `pbt4e-green-v55.txt` y `pbt4e-red-v55.txt` (§6.5.2), copiadas en el anexo V55-A.

#### D-PBT4-22. La recarga de «Editar todo», la de «Editar» (OWNER-02 de PB-T4c; la 15.ª tanda del Owner)

**El origen:** OWNER-02 de la revisión del código de PB-T4c (`F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205731Z-41db/independent-review.md`, :240-246), que el Owner aceptó con «Aceptarlo y corregir luego» (`owner-pbt4c-code-decisions.md`, pregunta 2), y la pregunta 2 de `owner-pbt4d-list-decisions.md`, «Tarea propia después». **El alcance,** la 15.ª tanda (`owner-pbt4e-list-decisions.md`): «Solo la recarga», «No abrir y avisar», «Antes de PB-T5» y «Sí, con condiciones».

**Lo de hoy,** medido en `160e019d` (la propuesta de lista, `pbt4e/list-proposal.md`, §1, con lo deducido marcado):
- **«Editar»** (`BeginEdit`, FVM:376-424) calcula `stale = view.Exists && view.State == Clean && view.IsPossiblyStale` (FVM:386) y entonces llama a `_port.TryReload` (FVM:388). El puerto pone la marca de lectura (WHC:1103-1115) y llama a `TryReloadPilotEdit` (WHC:498-576): sin `NeedsLazyReload` responde `NotNeeded` sin leer (WHC:527); si no, hace una lectura (WHC:530) y `DiscardAndReload` (WHC:553). Con `NotAdmitted`, el campo muestra el diagnóstico, la pista y «No se recargó: el borrador se conserva tal como estaba.» (FVM:407-411); con los demás, `Describe` (FVM:413).
- **«Editar todo»** (`EditAll`, FVM:432-490) llama directamente a `_port.TryEditAll` (FVM:463), y `EditAllOfPilot` (WHC:1179-1223) no mira la marca: abre sobre la base vieja.
- **Medido en el RED de la precondición 6** (`/d/r71`, con `src/` de la base): un borrador Clean marcado abre sin leer, con el nombre de antes del cambio de fuera (`marcado-limpio estado=Closed abierto=True lecturas=0 … nombre-del-lanzamiento=[PB-01] obsoleto=True`); y, de punta a punta, «Actualizar» no aplica: {{SONDA_RED_UPDATE}} (V55-10; R-T4-39).

**La decisión** (el GREEN literal, anexo V55-A):
1. **El anfitrión** (WHC, `EditAllOfPilot`): con un borrador que ya existe y `draft.NeedsLazyReload` (DS:306: registrado, marcado y Clean), una rama `else if` tras la de «sin borrador» llama a **`TryReloadPilotEdit(request)`**, la misma función que «Editar», no una copia:
   - va después de la compuerta, de la actualización en vuelo y del anfitrión del editor (WHC:1185-1191), y antes de las comprobaciones del estado, del texto pendiente y de `ShowModal` (WHC:1203-1215);
   - **con `Reloaded`, sigue y abre** sobre el borrador recargado: `DiscardAndReload` conserva el objeto (`WorkspaceLifecycleHostFull.cs:428`) y `PushBackFullDraftLaunch.For` toma su estado nuevo;
   - **con cualquier otro resultado, no abre** y devuelve `EditAllOutcome.NotBegun(<el resultado de la recarga>)` (D-S18). Con un borrador marcado y la compuerta ya pasada, los que se alcanzan son `NotAdmitted`, `Waiting`, `Rejected` y `Failed`;
   - **como mucho una lectura por gesto:** la de empezar o la de recargar, nunca las dos (`else if`), y ninguna durante `ShowModal`, porque la recarga va dentro de la marca que envuelve `TryEditAll` (WHC:1163-1175) y antes del modal;
   - la llamada se guarda en una variable (`var reload = TryReloadPilotEdit(request);`), no es `return TryReloadPilotEdit(request);`: la autoprueba de `WorkspacePilotEditGuardTests.cs:574` busca la primera aparición de ese literal, que sigue siendo la de `TryReload`.
2. **El campo** (FVM, `EditAll`): con `NotBegun` y `NotAdmitted` pone el diagnóstico y la pista, como hoy (FVM:473-477), y, **si el borrador ya existía** (`view.Exists`, FVM:441: sobre un borrador que existe el anfitrión nunca lo empieza, así que el `NotBegun` es el de la recarga), el aviso de «Editar», «No se recargó: el borrador se conserva tal como estaba.». El texto pasa a la constante `NotReloadedText`, que usa también `BeginEdit` (FVM:410), sin cambiar lo que muestra. **Los demás resultados ya salen como en «Editar»:** `EditAllNoticeOf` da `Describe(Begin.Status, Begin.Reason)` (FVM:1343), lo mismo que «Editar» da para el mismo resultado (FVM:413).
3. **Los comentarios** que la recarga deja inexactos, solo comentarios: el de `NotBegun` y el de `Begin` en WEI (WEI:276 y :308), el de `TryEditAll` en el puerto (`IWorkspacePilotEditPort.cs:118-121`) y el resumen de `EditAll` en FVM (FVM:427-431). El del puerto conserva las dos frases que fija `WorkspacePilotEditGuardTests.cs:1083-1084` y no lleva las dos que prohíbe (:1085-1086).

**Lo que no hace** (pregunta 1, «Solo la recarga»; PBT4E-C03 y C10):
- **ningún aviso cuando recarga:** «Editar» tampoco avisa (la opción 2 de la pregunta 1 no se hace);
- **no compara antes de abrir un borrador Dirty marcado** (la opción 3): Dirty, Conflict, Unknown, Error, Applying y un borrador sin la marca siguen como hoy (`NeedsLazyReload` es falso para todos ellos);
- no cambia «Editar», el botón (`ShowEditAllButton`), la ventana, RACKEDITAR, el camino clásico ni el Plugin;
- ningún miembro nuevo del puerto, ningún valor nuevo de `EditAllStatus` ni de `PilotBoundaryKind`, ningún texto nuevo;
- **no escribe en el DWG:** `DiscardAndReload` solo cambia la memoria del borrador, y «Actualizar» sigue siendo la única escritura.

**Si la recarga falla** (pregunta 2, «No abrir y avisar»): la ventana no se abre, y el campo muestra lo mismo que «Editar»: con `NotAdmitted`, el motivo, la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.»; con los demás, «En espera: …», «Rechazado: …» o «Fallo: …». El borrador queda Clean y marcado: una lectura no admitida no lo toca (DS:427-428).

**El caso del borde** (deducido, sin pérdida): un borrador Dirty solo por un texto pendiente igual al valor confirmado queda Clean tras el `CommitField` del gesto (FVM:442-461); si estaba marcado, «Editar todo» lo recarga, porque el anfitrión mira el borrador después de confirmar. «Editar» no lo recargaría: mira la vista de antes (FVM:386).

**Las dos decisiones de diseño, propuestas para la sesión** (el Owner las dejó a la sesión: `owner-pbt4e-list-decisions.md`, «Lo que queda decidido»). **Este paquete sigue las dos recomendaciones; la sesión las confirma o las cambia al revisar y las registra.**

**D-S17 (propuesta para la sesión): la recarga va en el anfitrión** (la opción A de la propuesta de lista), no en el panel (la B).
- **La regla en un solo sitio:** «Clean y marcado» ya vive en Application, `DraftSession.NeedsLazyReload` (DS:306), y `TryReloadPilotEdit` la aplica (WHC:527). En el anfitrión, «Editar todo» la consume tal cual; en el panel sería una tercera copia de la regla, junto a la de «Editar» (FVM:386), frente a la regla 2 de `AGENTS.md`.
- **El puerto protege a cualquier llamador** de `TryEditAll` (`IWorkspacePilotEditPort.cs:123`), no solo a FVM:463.
- **La marca de lectura:** la recarga queda dentro del envoltorio de `TryEditAll` (WHC:1163-1175); con B, entre `TryReload` y `TryEditAll` se soltaría la marca.
- **El caso del borde:** A recarga el borrador que el `CommitField` deja Clean; B miraría la vista de antes.
- **El coste:** WHC cambia un cuerpo existente, y FVM igual necesita su aviso (el punto 2 de arriba), así que A toca dos archivos de producto y B uno.
- **El efecto en las CT:** con A, las CT de comportamiento van en Core sobre el anfitrión real (CT-PBT4E-01, 02 y 04), y la de UI fija que el campo solo pide `TryEditAll` (`llamadas=[GetField,TryEditAll,GetField]`, CT-PBT4E-03). Con B, el anfitrión no cambiaría: CT-01, 02 y 04 pasarían en el RED (C04), tendrían que ir a UI sobre un doble del puerto, y CT-03 fijaría `llamadas=[GetField,TryReload,TryEditAll,GetField]`; además, la guarda `WorkspacePilotSurfaceGuardTests` (`T3B1GestureMembers`, :56-61, y `ReadViolations`, :82-97) se mediría con `TryReload` en `EditAll`.

**D-S18 (propuesta para la sesión): se reutiliza `EditAllStatus.NotBegun`,** sin valor nuevo.
- **El valor ya lleva lo que hace falta:** `EditAllOutcome.Begin` es un `EditIntentOutcome` (WEI:308-309), y el campo ya presenta `NotBegun` como «Editar» presenta el resultado de su intención: con `NotAdmitted`, el diagnóstico y la pista (FVM:473-477); con los demás, `Describe` (FVM:1343, la misma de FVM:413). Solo falta el aviso de la recarga no admitida, que el campo distingue por `view.Exists` (FVM:441).
- **Ninguna prueba enumera `EditAllStatus`** ni nombra sus valores en `tests/` de `160e019d` (por `git grep`): solo la línea `cola estado=NotBegun` de CT-PBT4C-03 (`PushBackEditAllTests.cs:245`), que no cambia.
- **Un valor nuevo** (`NotReloaded`, con su fábrica) serían dos declaraciones más en WEI y un caso más en `EditAllNoticeOf` y en `EditAll`. Sobre todo, **CT-PBT4E-03 necesitaría la fábrica nueva para escribir la respuesta de su doble:** el RED no compilaría sin un esqueleto en `src/` (WEI con el valor), y PB-T4e dejaría de ser un RED solo de `tests/`.
- **El coste de reutilizarlo:** cambian el comentario de `NotBegun` y el de `Begin` (WEI:276 y :308), y el campo infiere la recarga de `view.Exists`, lo que dice el comentario del GREEN.
- **El efecto en las CT:** CT-02 fija `estado=NotBegun inicio=<el de la recarga>` y CT-03 responde `EditAllOutcome.NotBegun(...)`. Con un valor nuevo serían `estado=NotReloaded` y `EditAllOutcome.NotReloaded(...)`, con el esqueleto en el RED.

**Si la sesión cambia una de las dos,** cambian el GREEN, CT-PBT4E-02 y 03 (y, con B, la clase de CT-01, 02 y 04), y la precondición 6 se repite entera: es una versión nueva del paquete.

%%%% END

%%%% OP after s45
%%%% ANCHOR
**Si cualquier otra prueba falla,** fuera de la lista, es PBT4D-C16 y vuelve al Owner (`owner-pbt4d-list-decisions.md`, pregunta 4).
%%%% TEXT



### 4.5 PB-T4e (V55-04)

**Medido en el cierre de PB-T4d** (`160e019d`): por objetos git y compilando y ejecutando en copias exportadas, con la suite completa de Core y los 19 filtros de UI del contrato, cada uno en su proceso, sobre el RED exacto y sobre el GREEN literal (la precondición 6, §6.5.1).

**La lista: vacía (K = 0).** Ninguna prueba existente se edita, se salta, se reactiva, se borra ni se sustituye; el RED solo añade las dos clases de las CT. **Medido:** en el RED, la suite completa de Core da {{CORE_RED}} y solo caen las tres CT de Core de PB-T4e; en el GREEN, {{CORE_GREEN}}; los 19 filtros de UI, en el RED solo cae CT-PBT4E-03 y en el GREEN, 0 `Failed`. **K = 0, D = 0 y E = 0:** ningún `Skip`, ninguna prueba que falle fuera de la lista y ningún equivalente.

**Por qué ninguna prueba existente cambia** (la propuesta de lista, §3.1, medido aquí): ninguna de las nueve clases de PB-T4c prepara un borrador marcado (CT-PBT4C-03 «clean» empieza el borrador sin drenaje, `PushBackEditAllTests.cs:160-167`; las vistas del doble de UI llevan la marca en `false`, `PushBackEditAllWindowTests.cs:162-163`), y sin la marca `EditAllOfPilot` no lee: la línea `clean estado=Closed abierto=True lecturas=0` de CT-PBT4C-03 y `k1-cerrado … lecturas-dentro=0` de CT-PBT4C-05 siguen iguales.

**Las que leen el texto que cambia y siguen en verde** (medidas con la suite completa):

| Guarda | Por qué sigue en verde |
|---|---|
| CT-PBT4C-07 (`PushBackOpenerClassicPathGuardTests.cs:44-47`), que lee `TryEditAll`, `EditAllOfPilot` y `EditAll` | El texto nuevo no nombra ninguno de sus ocho nombres |
| `TheMarkOfReadInProgressWrapsEachOfTheSixReads…` y su autoprueba (`WorkspacePilotEditGuardTests.cs:541-589`) | El envoltorio de `TryEditAll` no cambia; la recarga va en el cuerpo privado y sin el literal `return TryReloadPilotEdit(request);`, así que la autoprueba de :574 sigue mutando la de `TryReload` |
| `TheReloadTranslatesTheOutcomesOfTheDraft…` y su copia (`WorkspaceLifecycleGuardTests.cs:666-681`; `WorkspaceLifecycleGuardFull.cs:44-80`) | El cuerpo de `TryReloadPilotEdit` no cambia: se reutiliza |
| `TheCommentOfThePortSaysThatSixOperationsReadTheDrawing` (`WorkspacePilotEditGuardTests.cs:1078-1087`) | El comentario nuevo del puerto conserva las dos frases y no lleva las prohibidas |
| `OnlyTheGestureMethodsOfTheFieldNameTheThreeReads…` (`WorkspacePilotSurfaceGuardTests.cs:82-120`) | `EditAll` no nombra `TryReload` (D-S17) |
| `TheThreeFilesOfTheHostAndTheThreeViewModelsUseNoReflection…` (`WorkspaceHostHardeningGuardTests.cs:779-783`) | El texto nuevo no añade ningún mecanismo ni reflexión |
| Las de la recarga del anfitrión y de «Editar» (`WorkspaceLifecycleHostFull.cs:411-470`; `WorkspacePilotFieldViewModelTests.cs:611-680`) | Son el oráculo de lo que PB-T4e reutiliza, y no cambian |

**La cobertura de todo `src/` que se queda** (la regla permanente del Owner, `owner-pbt4d-cat4d-decisions.md`, pregunta 2): la precondición 6 compara la de toda la suite de Core y la de la UI de los 19 filtros (la unión de tres corridas de todos juntos y de una de cada filtro en su proceso, D-S16), en la base sin cambios y en el GREEN, línea a línea y rama por rama, con coverlet en JSON sobre Domain, Application y UI (`cov_p6_pbt4e_v55.py`). **Lo medido:** {{COV_LINE}}; **{{COV_RESULT}}**; {{COV_BAJAN}}. **Ningún camino que se queda pierde su única prueba: ninguna sustituta.**

**Si cualquier otra prueba falla,** es PBT4E-C16 y vuelve al Owner (`owner-pbt4e-list-decisions.md`, pregunta 4).
%%%% END
