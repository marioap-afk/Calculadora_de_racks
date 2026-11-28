# I-64 F4B: la recarga de «Editar todo» (nombre provisional PB-T4e). Propuesta de lista para el Owner

**Estado:** propuesta del analista. No es un paquete ni un contrato. No se escribió código de producto ni pruebas, no se compiló y no se ejecutó nada.

**La base:** `160e019d773ba9095e2d5858cd593a954b8eec67`, el cierre de PB-T4d (F4B-E59). Todo se leyó por objetos git desde el checkout principal (`show`, `grep`, `ls-tree`, `log`, `diff --stat`). No se leyó el worktree de la rama ni los archivos en disco del checkout principal.

**Las citas `ruta:línea`** son de `160e019d`. Los cinco archivos de producto que importan aquí (WHC, FVM, `DraftSession.cs`, WEI y `WorkspaceNavigationHost.cs`) no cambian entre el GREEN de PB-T4c (`7cc1076e`) y `160e019d` (`git diff --stat` vacío). Por eso las líneas de OWNER-02 siguen valiendo.

**Las abreviaturas,** las del paquete:
- **WHC:** `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`.
- **FVM:** `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`.
- **WEI:** `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`.
- **DS:** `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`.
- **NH:** `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`.
- **WCo:** `src/RackCad.Plugin/Workspace/WorkspaceController.cs`.

**Lo que no tiene fuente** va marcado como **(propuesta)**: los identificadores `PB-T4e`, `D-PBT4E-*`, `CT-PBT4E-*`, el `TaskId`, los nombres de las clases y las líneas esperadas. No son oficiales.

**Las fuentes:**
- `F4B-DRAFTS/coordinator/owner-pbt4c-code-decisions.md`, pregunta 2: «Aceptarlo y corregir luego».
- `F4B-DRAFTS/coordinator/owner-pbt4d-list-decisions.md`, pregunta 2: «Tarea propia después».
- La revisión del código de PB-T4c, OWNER-02. En `160e019d` el archivo se llama `independent-review.md`, no `independent-review-code.md`, y está en `docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205731Z-41db/`, junto a `direct-review.md`.
- La medida por texto del redactor de PB-T4d (`F4B-DRAFTS/pbt4/v52/handoff-pbt4d.md:108-113`).
- El paquete `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v5.4).
- `AGENTS.md` y `docs/HANDOFF.md`.

---

## 0. En una página

- **Hoy, «Editar»** recarga en silencio un borrador Clean marcado como posiblemente obsoleto, y después deja la caja editable. **«Editar todo»** no lo mira y abre la ventana sobre la base vieja. Al editar, el borrador queda Dirty y obsoleto, y «Actualizar» acaba en el conflicto y en «Conservar mi borrador».
- **Opción recomendada (A, la mínima en el anfitrión):**
  - `EditAllOfPilot` llama a **la misma función** de la recarga (`TryReloadPilotEdit`) cuando `draft.NeedsLazyReload`, antes de abrir;
  - si la recarga no sale, no abre y devuelve el resultado de la recarga;
  - toca una función de WHC y, para que el aviso sea igual que el de «Editar», unas líneas de FVM.
- **Pruebas:** ninguna prueba existente fija hoy «Editar todo» sobre un borrador obsoleto, y ninguna línea esperada cambia (medido por texto). **La lista de sustituciones sale vacía (K = 0)**, a confirmar al ejecutar. Se proponen 3 CT de Core y 1 de UI, en clases nuevas.
- **Las preguntas para el Owner (4):** el alcance; qué hacer si la recarga falla; el orden respecto a PB-T5; y la emisión condicional.

---

## 1. El comportamiento de hoy, medido en el código

### 1.1 Cómo se pone la marca de «posiblemente obsoleto»

**Leído:**
- **El cambio de fuera.** Un cambio de un objeto del dibujo (`DatabaseObjectChanged`) o el fin de un comando (`CommandEnded`) que no son del panel añaden la operación `MarkDraftsPossiblyStale` al drenaje. El drenaje solo corre con el documento en reposo, sin modal y con el panel visible (`src/RackCad.Application/Workspace/WorkspaceSession.cs:54`, `:66-68`).
- **El drenaje marca.** Marca todos los borradores de la sesión (WHC:191-195, `MarkPilotDraftsPossiblyStale`, WHC:2021-2025, que llama a `MarkPossiblyStale`, DS:408-414). Solo es una pista: no lee el dibujo ni pone el borrador en conflicto.
- **La marca no es un estado** (DS:54-56, DS:302-303). La quitan:
  - `Rebase`, es decir, la recarga o «Conservar mi borrador» (DS:489-499; la línea :493);
  - una comprobación que coincide (`ConfirmBase`, DS:754).
- **`SetCurrentDraftState` no la quita** (DS:345-350).

### 1.2 «Editar» sobre un borrador Clean marcado

**La decisión, en el panel.** `BeginEdit` (FVM:376-424):
- lee el campo de la memoria del puerto (`GetField`, FVM:385);
- calcula `stale = view.Exists && view.State == Clean && view.IsPossiblyStale` (FVM:386);
- y llama a `_port.TryReload(request)` en lugar de `TryBeginEdit`, con la petición sin referencia y sin testigo (FVM:387-388).

El estado y la marca de la vista salen del borrador (WHC:1862, :1913-1915).

**El puerto.** `TryReload` pone la marca de lectura (`_pilotReadInProgress`) con `try/finally` y llama a `TryReloadPilotEdit` (WHC:1103-1115). La compuerta de avisos la reenvía, o responde `Waiting` dentro de un aviso (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:173-174`).

**La recarga, `TryReloadPilotEdit`** (WHC:498-576), en este orden:
1. Un `RackId` en blanco da `NotAdmitted`, y un testigo sin clave, `Failed` (:505-516).
2. `PassGate` responde `Unavailable`, `Waiting` o `Rejected`, sin leer (:519-522).
3. Sin borrador, `NotFound`. Si **`!draft.NeedsLazyReload`**, `NotNeeded`, sin leer (:525-527). `NeedsLazyReload` es «registrado, marcado y Clean» (DS:306). Clean quiere decir sin texto pendiente y con `current` igual a `initial` según el comparador (DS:298-300).
4. **Lo que lee:** una sola lectura, `ReadIntentAtOrigin(session, rack, draft.CurrentDraftState.Origin, witness)` si el lector la ofrece; si no, `ReadIntent` (:530-532). El lector del Plugin la ofrece (NH:196, NH:220-231). En el Plugin (NH:233-268):
   - el documento activo, y `Busy` si hay un comando en curso (:235-239);
   - el catálogo, fuera del bloqueo (:241-243);
   - `LockDocument` y la autorización (:245-249);
   - la revalidación de los destinos y el origen (:251-253);
   - un escaneo del dibujo y la lectura pura (`RackPilotIntentAdapter.Read`, `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:122-170`);
   - y la relectura por el texto del origen (`AtOrigin`, :178-183).
5. **Lo que hace con lo leído:** las respuestas de la lectura se traducen (:533-550). Después, `draft.DiscardAndReload(full.Read)` (:553). Sustituye la base, el estado inicial y el actual, vacía el texto pendiente y quita la marca (DS:422-437 y :489-499). El borrador sigue siendo el mismo objeto: lo fija `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostFull.cs:428`.
6. **Lo que compara: nada.** La recarga no compara la base nueva con la vieja. A diferencia de «Comprobar ahora» (`CheckNow` o `ConfirmBase`, WHC:470 y :641), sustituye. La única comparación es la que decide que el borrador está Clean (el comparador, DS:298).
7. **Los resultados** (:554-575):
   - `Reloaded`, que acaba además la marca «known clean»;
   - `NotAdmitted`, con el diagnóstico, el detalle y la pista;
   - `NotAllowed`, que da `Waiting`;
   - otro, `Failed`;
   - una excepción, `Report("WorkspaceHostCore.TryReloadPilotEdit", ex)` y `Failed(ex.Message)`.

**Lo que ve el usuario** (FVM:390-423):

| Resultado | Lo que ve | Fuente |
|---|---|---|
| `Reloaded` (y `Admitted`, `AlreadyOpen`, `NotNeeded`) | Ningún aviso. La caja pasa a editable (`EditableOf`, FVM:807-814, ya no la excluye), el estado dice «Listo para editar» (FVM:1231), sin «(posiblemente obsoleto)» (FVM:1237), y el valor es el del dibujo. **Es la recarga silenciosa** | FVM:401-406; UI `WorkspacePilotFieldViewModelTests.cs:611-631` |
| `NotAdmitted` | El diagnóstico y la pista de RACKEDITAR, y el aviso «No se recargó: el borrador se conserva tal como estaba.». El borrador queda Clean y marcado, y la caja, en solo lectura. El botón «Editar» sigue visible, pero el clic en la caja ya no abre (`CanOpenByGesture` exige `_diagnostic == null`, FVM:244) | FVM:407-411; `WorkspacePilotFieldViewModelTests.cs:648-666` |
| `Waiting`, `Rejected`, `NotFound`, `Failed`, `Unavailable` | «En espera: …», «Rechazado: …», «Sin borrador: …», «Fallo: …» o «No disponible: …» (`Describe`, FVM:1320-1332). Nada cambia | FVM:412-414; `WorkspacePilotFieldViewModelTests.cs:668-680` |
| Excepción en el panel | El estado Error del campo (`EnterFailure`) | FVM:420-423 |

**Si el rack ya no existe.** Esta parte es **deducida**: leí el código, pero no lo ejecuté.
- Sin destinos, `ChooseOriginDefinition` da 0 (WEI:804-814) y la clave del origen queda vacía (`RackPilotIntentAdapter.cs:134`).
- La regla 2 del lector del piloto rechaza con «No editable aquí: el origen ya no está en el dibujo» (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:29`, :163-166).
- `AtOrigin` deja la lectura como estaba si no encuentra otro miembro con ese texto (`RackPilotIntentAdapter.cs:173-183`).
- El lector completo devuelve el mismo rechazo (`src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftReader.cs:56-68`).
- Así que `DiscardAndReload` da `NotAdmitted`, y el usuario ve la fila `NotAdmitted` de la tabla, con la pista «Usa RACKEDITAR para editarlo con el editor clásico.» (`PushBackPilotIntentReader.cs:56`).

### 1.3 Por qué «Editar todo» no recarga

**Leído:**
- **El gesto.** `EditAll` (FVM:432-490):
  - lee el campo (:441);
  - confirma el texto pendiente si lo hay (:442-461);
  - y llama directamente a `_port.TryEditAll` (:463).
  
  **No calcula `stale` ni llama a `TryReload`.**
- **El anfitrión.** `EditAllOfPilot` (WHC:1179-1223), en este orden:
  - la compuerta (:1185-1188);
  - la actualización en vuelo (:1190);
  - el anfitrión del editor (:1191);
  - sin borrador, `TryBeginPilotEdit`, una lectura fresca (:1194-1201);
  - `Applying` o un borrador sin registrar (:1204);
  - `!view.CanEdit` (:1205);
  - el texto pendiente (:1206);
  - el puerto y `ShowModal` (:1209-1216).
  
  **No mira `IsPossiblyStale` ni `NeedsLazyReload`.** Un borrador Clean marcado tiene `CanEdit = true` (WHC:1886-1887), así que abre.
- **El botón se ofrece.** `ShowEditAllButton` (FVM:236-237) pide `!_view.Exists || _view.CanEdit` y no depende de que la caja sea editable. Para un Clean marcado, los dos botones están visibles (FVM:1213-1214).
- **Lo que pasa después:**
  - La ventana entrega sus ediciones con `PushBackFullDraftStatePort.ApplyToDraft` (`src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs:22-29`), que llama a `SetCurrentDraftState` y no quita la marca: el borrador queda **Dirty y obsoleto**.
  - «Actualizar» se ofrece también con la marca (FVM:258-263).
  - La corrida compara la base con el dibujo y da `NotAppliedConflict`, en el PREPARE (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:359-366`) o en la comprobación del MUTATE (:443-454). El borrador pasa a Conflict (DS:661-665).
  - El usuario ve «No se aplicó: el dibujo cambió fuera del panel. Diferencias: …» (FVM:83 y :1109-1110), y las opciones «Conservar mi borrador» (FVM:847-848) y «Descartar y recargar».
  - No se escribe nada sin avisar.
  - **Deducido:** el detalle de la comparación dentro de `PrepareFull` no lo leí en esta pasada. Lo tomo de OWNER-02 y de D-PBT4-08.
- **Con la ventana cerrada sin cambios,** el borrador sigue Clean y marcado, y la caja, en solo lectura. «Editar» todavía recargaría.

### 1.4 Lo leído y lo deducido

| Afirmación | Cómo |
|---|---|
| La decisión de «Editar», la recarga del anfitrión, sus resultados y lo que ve el usuario | Leído (FVM, WHC, DS) y fijado por las pruebas citadas |
| «Editar todo» no mira la marca, y el botón se ofrece | Leído (FVM:432-490, WHC:1179-1223, FVM:236-237) |
| La ventana deja el borrador Dirty y obsoleto | Leído (`PushBackFullDraftStatePort.cs:22-29`, DS:345-350) |
| «Actualizar» da el conflicto y sale «Conservar mi borrador» | El reparto, leído (`PushBackPilotUpdateRun.cs:359-366`, :443-454; DS:661-665; FVM:847-848). La comparación de `PrepareFull`, deducida |
| El rack que ya no existe da `NotAdmitted` con «el origen ya no está en el dibujo» | Deducido de la lectura (NH, WEI, el adaptador y los dos lectores); no se ejecutó |
| Cualquier comando que termina fuera del panel marca el borrador | Leído en `WorkspaceSession.cs:66-68`. No leí si el puente filtra algún comando antes de esa pista |

---

## 2. Las opciones de alcance

**Lo común a todas:**
- La recarga solo lee y cambia la memoria del borrador: `DiscardAndReload` no escribe en el DWG.
- «Actualizar» sigue siendo la única escritura, con su gesto.
- Un borrador Clean no tiene nada propio que perder: ni cambios ni texto pendiente (DS:298-300).

### Opción A. La mínima, en el anfitrión (recomendada)

**Qué:** en `EditAllOfPilot`, con un borrador que ya existe, `if (draft.NeedsLazyReload)` llama a **`TryReloadPilotEdit(request)`**, la misma función que usa «Editar», no una copia.
- **El sitio:** después de la comprobación del anfitrión del editor (WHC:1191) y antes de las del estado y de `ShowModal` (WHC:1203-1215).
- **Si sale:** con `Reloaded` (o `NotNeeded`) sigue y abre sobre el borrador recargado. `PushBackFullDraftLaunch.For` toma el `CurrentDraftState` nuevo (`src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs:14-20`).
- **Si no sale:** devuelve `EditAllOutcome.NotBegun(<el resultado de la recarga>)` y no abre (o abre igualmente, según la pregunta 2).

**Los archivos de `src/`:**
- WHC, un cuerpo existente (UNIT_CHANGE).
- **WEI, solo un comentario:** el de `NotBegun` (WEI:276-277) dice «no había borrador y la intención no lo empezó». Con la recarga, ese texto queda inexacto. La otra salida es un valor nuevo de `EditAllStatus`, que es una declaración más y obliga a tocar FVM.
- **Para que el aviso sea igual que el de «Editar»,** FVM (`EditAll`, FVM:473-477): con `NotBegun` y `NotAdmitted` sobre una vista Clean marcada, el aviso «No se recargó: el borrador se conserva tal como estaba.». Hoy ese texto es un literal de FVM:410, y conviene sacarlo a una constante.
  - **Sin ese toque,** el usuario ve el diagnóstico y la pista, pero no esa frase.
  - **Los demás fallos ya salen igual que en «Editar»:** FVM los presenta con `Describe` (FVM:1343): «En espera: …», «Rechazado: …», «Fallo: …» y «Sin borrador: …».

**Qué cambia de visible, además de la recarga:**
- la ventana abre con los valores del dibujo actual;
- al cerrarla, la caja queda editable y el estado dice «Limpio», sin «(posiblemente obsoleto)», como tras «Editar»;
- si la recarga falla, la ventana no abre.

**Con un borrador Dirty marcado:** nada cambia (`NeedsLazyReload` es falso). Es lo mismo que hace «Editar»: con Dirty, `BeginEdit` no recarga (FVM:386) y responde `AlreadyOpen`. **Con Conflict, Unknown o Error,** tampoco. **Con uno sin marca,** tampoco: no lee, y la línea `clean … lecturas=0` de CT-PBT4C-03 sigue igual.

**El caso del borde:** un borrador Dirty solo por un texto pendiente igual al valor confirmado queda Clean tras `CommitField`. Si estaba marcado, A lo recarga, porque mira el borrador después de confirmar. B no lo recarga, porque mira la vista de antes. Es deducido y sin pérdida en los dos casos.

**Los riesgos:**
- **UI modal:**
  - la lectura va antes de `ShowModal`, dentro de la misma marca de lectura que envuelve `TryEditAll` (WHC:1163-1175);
  - K = 1 y `lecturas-dentro=0` de CT-PBT4C-05 se mantienen;
  - sigue habiendo como mucho una lectura por gesto: la de empezar o la de recargar, nunca las dos.
- **Foco y paleta:** la lectura no toca el foco (NH:194). Mientras lee, la paleta no está deshabilitada todavía (el modal no ha empezado), pero el gesto es síncrono. Es lo mismo que hoy con «Editar» (deducido).
- **El coste:** una lectura completa más antes de abrir, solo en el caso marcado. Es la misma que paga «Editar», y en un dibujo grande se nota antes de que salga la ventana. Para el Smoke-PB.
- **Que no se escriba sin avisar:** se mantiene. Además, el editor parte del dibujo actual, con el cambio de fuera, que el usuario ve en la ventana antes de editar.
- **Las guardas** (a medir): `EditAllOfPilot` es un sujeto de CT-PBT4C-07, que solo prohíbe ocho nombres que no aparecen. La autoprueba `WorkspacePilotEditGuardTests.cs:574` busca la **primera** aparición de `return TryReloadPilotEdit(request);`. El texto nuevo no debe llevar ese literal, por ejemplo con `var reloaded = TryReloadPilotEdit(request);`.

### Opción B. La mínima, en el panel

**Qué:** `EditAll` copia la decisión de `BeginEdit`. Si `view.Exists && Clean && IsPossiblyStale`, llama antes a `_port.TryReload(request)`:
- si no sale, lo presenta como `BeginEdit` y no llama a `TryEditAll`;
- si sale, llama a `TryEditAll`.

**Los archivos de `src/`:** solo FVM.

**Qué cambia de visible:** lo mismo que A, con el aviso igual que el de «Editar» sin tocar el anfitrión. **Dirty, sin marca:** como en A.

**Los riesgos:**
- **La regla en un segundo sitio de la UI.** La decisión «Clean y marcado» ya está en `NeedsLazyReload` (Application) y en FVM:386. B la repite en FVM, frente a la regla 2 de `AGENTS.md` («Regla en un solo sitio»).
- **El puerto no protege a otro llamador de `TryEditAll`.** Hoy solo lo llama FVM.
- **Entre `TryReload` y `TryEditAll` se suelta la marca de lectura.** Las dos llamadas son síncronas y nada se intercala (deducido).
- **No ve el caso del borde de A.**
- **La guarda `WorkspacePilotSurfaceGuardTests`** (`ReadViolations`, :82-97) solo limita `TryReload` en los miembros que nombra, y `EditAll` no es uno de ellos (:58 y :66). Sigue en verde por lectura; a medir.

### Opción C. Más amplia (cualquiera de estas, sobre A o B)

- **C1. Un aviso visible de la recarga,** por ejemplo «Borrador recargado desde el dibujo.» (ya existe como constante, FVM:69). Cambia un texto visible, y «Editar» recarga sin aviso, así que los dos gestos dejan de ser iguales. Archivos: FVM.
- **C2. Comprobar antes de abrir también con cambios propios** (Dirty marcado): una comprobación de la base (`TryConfirmBase`) antes de abrir, que adelanta el conflicto que hoy sale al «Actualizar».
  - **Va más allá de «Editar»,** que con Dirty no comprueba.
  - **Hay que decidir** si con el conflicto se abre o no: hoy Conflict abre (D-PBT4-13).
  - **Necesita** textos nuevos y más CT, y cambia el camino Dirty.
  - **Archivos:** WHC, FVM y quizá WEI.
- **C3. No ofrecer «Editar todo» mientras se queda el diagnóstico de una recarga fallida** (`_diagnostic`, como hace `CanOpenByGesture`, FVM:244), para no repetir una lectura que ya falló. Cambia cuándo se ve el botón. Archivos: FVM.

### Descartada: no ofrecer «Editar todo» sobre un borrador Clean marcado

Obligaría a pulsar antes «Editar». Es más simple, pero contradice la letra de la decisión: «que «Editar todo» recargue antes como «Editar»» (`owner-pbt4c-code-decisions.md`, «Lo que queda decidido»).

### 2.1 La forma de la subtarea con la opción A (propuesta)

**El nombre y la base:**
- **`TaskId` (propuesta):** `F4B-PBT4-EDIT-ALL-RELOAD`, de 24 caracteres. La regla de §0 pide `F4B-PBT4-` y como mucho 26.
- **La base:** el cierre de PB-T4d (`160e019d`), con su 1/3 (OB-10.1).

**Las decisiones (propuesta):**
- **D-PBT4E-01. La recarga, la de «Editar».** `EditAllOfPilot` llama a `TryReloadPilotEdit` cuando `NeedsLazyReload`, tras el anfitrión del editor y antes de las comprobaciones del estado. Dentro de la marca de lectura, antes de `ShowModal`, y con como mucho una lectura por gesto. Ningún miembro nuevo del puerto y ningún valor nuevo de `PilotBoundaryKind`.
- **D-PBT4E-02. El fallo** (según la pregunta 2): no abre, y devuelve `NotBegun` con el resultado de la recarga (el comentario de WEI:276-277, al día). FVM lo presenta como «Editar», con «No se recargó: …».
- **D-PBT4E-03. Lo que no hace:**
  - no toca Dirty, Conflict ni un borrador sin marca;
  - no cambia «Editar», el botón, la ventana, RACKEDITAR ni el camino clásico;
  - no escribe en el DWG.

**La lista:** **vacía (K = 0)**: ninguna prueba existente se salta ni se sustituye (§3.1). Las CT van en **clases nuevas**, porque añadir líneas a CT-PBT4C-03 o 14 sería editar una prueba existente (PBT4C-C02).

**El RED y el GREEN:**
- **El RED:** solo `tests/`, las clases nuevas. Usan el API de hoy (`TryEditAll`, `MarkPossiblyStale` y `EditAllOutcome.Begin`), así que no hace falta esqueleto de `src/` (a confirmar al compilar).
- **El GREEN:** WHC (el cuerpo de `EditAllOfPilot`), WEI (el comentario) y FVM (el aviso y la constante).

**La precondición 6 (propuesta),** sobre una copia exportada de `160e019d`:
- el RED y el GREEN literal compilan, sin ningún `warning CS`;
- la suite completa de Core: en el RED, solo caen por aserción las CT con `ExpectRed` true; en el GREEN, 0 `Failed`;
- los filtros de UI de la clase nueva y de las heredadas que miran FVM (`PushBackEditAllWindowTests`, `WorkspacePilotFieldViewModelTests` y `WorkspacePilotReconciliationTests`, al menos): 0 `Failed` en el GREEN;
- el paseo de guardas sobre el GREEN literal, por las clases que leen WHC (28 archivos de `tests/` lo nombran) y FVM (8);
- las sondas de las líneas de las CT;
- **K = 0, D = 0 y E = 0.**

**§5.5:** el Smoke-PB pasa a comprobar la recarga (§4), en lugar del comportamiento aceptado en PB-T4c.

---

## 3. Las pruebas

### 3.1 Las existentes que fijan hoy «Editar todo» y cambiarían: ninguna

**Medido por texto:** ninguna de las nueve clases de PB-T4c nombra la marca ni la recarga más allá de la lista de operaciones del puerto.
- **Las cinco clases de las CT:** `PushBackEditAllTests`, `PushBackEditAllGuardTests`, `PushBackOpenerClassicPathGuardTests`, `PushBackDraftLaunchGuardTests` y `PushBackEditAllWindowTests`.
- **Los cuatro equivalentes:** `PushBackEditAllPortEquivalenceGuardTests`, `PushBackEditAllNameWritersGuardTests`, `PushBackEditAllFullCopyGuardTests` y `PushBackEditAllApplyCloseTests`.

Las únicas apariciones de `TryReload` en ellas son esa lista:
- `PushBackEditAllGuardTests.cs:25`;
- `PushBackEditAllPortEquivalenceGuardTests.cs:27`;
- `PushBackEditAllTests.cs:357` y `:387`, la de K = 1;
- y el doble de `PushBackEditAllWindowTests.cs:82-86`.

Ninguna prepara un borrador marcado: CT-PBT4C-03 «clean» empieza el borrador sin drenaje (`PushBackEditAllTests.cs:160-167`), y las vistas del doble de UI tienen la marca en `false` (`PushBackEditAllWindowTests.cs:162-163`).

**Número de pruebas existentes cuyas líneas cambian: 0, con A o con B.** La medida del redactor de PB-T4d dice lo mismo (`handoff-pbt4d.md:108-113`: «Ninguna línea existente cambia»). C2 y C3 tampoco cambiarían líneas existentes, pero sí añadirían más CT.

### 3.2 Las existentes que deben seguir en verde y conviene vigilar

| Prueba | Archivo | Por qué |
|---|---|---|
| CT-PBT4C-03 (`clean … lecturas=0`), CT-PBT4C-05 (`lecturas-dentro=0`), CT-PBT4C-10, CT-PBT4C-12 | `tests/RackCad.Tests/PushBackEditAllTests.cs` | Sin marca no hay recarga, y la recarga va antes del modal |
| CT-PBT4C-07, la guarda de OB-01.3 | `tests/RackCad.Tests/PushBackOpenerClassicPathGuardTests.cs:44-47` | Lee los cuerpos de `TryEditAll`, `EditAllOfPilot` y `EditAll`, que cambian. Solo prohíbe ocho nombres |
| CT-PBT4C-02, 06 y 14 (el orden de llamadas sin marca) | `tests/RackCad.UI.Tests/PushBackEditAllWindowTests.cs` | Sin marca, las llamadas no cambian. Con B y marca, sí, pero en una CT nueva |
| `TheMarkOfReadInProgressWrapsEachOfTheSixReads…` y su autoprueba (`TheReadGuardTripsWithout…`, :574) | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:541-589` | Sigue en verde si el envoltorio de `TryEditAll` no cambia y el texto nuevo no lleva `return TryReloadPilotEdit(request);` |
| `TheReloadTranslatesTheOutcomesOfTheDraft…` y su copia | `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:666-681`; `WorkspaceLifecycleGuardFull.cs:44-80` | Fijan el cuerpo de `TryReloadPilotEdit`, que A reutiliza sin tocarlo |
| Las pruebas de la recarga del anfitrión | `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostFull.cs:411-470` (y :1492-1509) | Son el oráculo de lo que A reutiliza. No cambian |
| Las pruebas de la recarga de «Editar» | `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:611-680` | Son el oráculo de lo que el aviso debe imitar. Con B, `BeginEdit` no cambia |
| `OnlyTheGestureMethodsOfTheFieldNameTheThreeReads…` | `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:82-120` | Con B, `EditAll` nombra `TryReload`. No está en las listas del guarda, a medir |

### 3.3 Las CT candidatas (propuesta, al estilo de §5.3)

Siguen las reglas de §5.0:
- un `[Fact]` por CT, que acaba en un `Assert.Equal` de las líneas unidas por `\n`;
- ninguna `[Theory]` nueva;
- la regla «EXCEPCION»;
- los ayudantes existentes, sin editarlos: `FullReadRig`, `FullEditIntentFakeReader` y `FullEditIntentKit` (con `OriginMissing`, `tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs:27-74`, :309-368). El doble del editor de `PushBackEditAllTests` y el del puerto de UI se copian dentro de la clase nueva, sin referenciarlos.

**Los nombres de las clases (propuesta):** `PushBackEditAllReloadTests` (Core) y `PushBackEditAllReloadWindowTests` (UI). Por texto, ningún nombre existente empieza así, y ningún filtro heredado de clase exacta los selecciona (D-S8). A medir.

**Las líneas son propuesta.** Las de CT-01, 02 y 04 salen de una sonda sobre el GREEN literal en la fase siguiente.

| CT (propuesta) | Qué fija | Líneas (propuesta) | Clase | `ExpectRed` |
|---|---|---|---|---|
| **CT-PBT4E-01** | **La recarga antes de abrir** (D-PBT4E-01). Receta: el borrador empezado, el lector cambiado a otra lectura admitida de otro contenido (el fixture lo fija el paquete), `MarkPossiblyStale()` y `TryEditAll`. **Los contrastes:** sin marca; Dirty marcado; Conflict marcado; marcado sin anfitrión del editor; marcado y de otra sesión | `marcado-limpio estado=Closed abierto=True lecturas=1 lecturas-dentro=0 lanzamiento-de-la-lectura-nueva=True obsoleto=False borrador=Clean`; `sin-marca estado=Closed abierto=True lecturas=0`; `marcado-sucio estado=Closed abierto=True lecturas=0 obsoleto=True borrador=Dirty`; `marcado-conflicto estado=Closed abierto=True lecturas=0`; `marcado-sin-anfitrion estado=NotAvailable abierto=False lecturas=0`; `marcado-otra-sesion estado=Rejected abierto=False lecturas=0` | `PushBackEditAllReloadTests` | true |
| **CT-PBT4E-02** | **Si la recarga no sale, no abre** (D-PBT4E-02, la opción 1 de la pregunta 2). Recetas: el origen que falta (`FullEditIntentKit.OriginMissing`); el lector `Busy`; el lector que lanza | `no-admitida estado=NotBegun inicio=NotAdmitted diagnostico=[No editable aquí: el origen ya no está en el dibujo] pista=True abierto=False lecturas=1 obsoleto=True borrador=Clean`; `ocupado estado=NotBegun inicio=Waiting motivo=[Hay un comando en curso.] abierto=False lecturas=1`; `excepcion estado=NotBegun inicio=Failed motivo=[boom] abierto=False fallo=[WorkspaceHostCore.TryReloadPilotEdit]` | `PushBackEditAllReloadTests` | true |
| **CT-PBT4E-03** | **El aviso, como el de «Editar»** (D-PBT4E-02). La FVM real sobre un doble del puerto, con una vista Clean marcada: `NotBegun` con `NotAdmitted`, con `Waiting`, y `Closed` tras la recarga | `no-recargado diagnostico=[No editable aquí: cambió el rack.] pista=[Usa RACKEDITAR.] aviso=[No se recargó: el borrador se conserva tal como estaba.] estado=[Limpio (posiblemente obsoleto)] editable=False`; `ocupado aviso=[En espera: Hay un comando en curso.]`; `recargado-y-cerrado estado=[Limpio] aviso=[Editor cerrado sin cambios en el borrador.] editable=True llamadas=[GetField,TryEditAll,GetField]` (con B: `[GetField,TryReload,TryEditAll,GetField]`) | `PushBackEditAllReloadWindowTests` (UI) | true (solo cae la primera línea en el RED) |
| **CT-PBT4E-04** | **De punta a punta sin AutoCAD,** el beneficio: el cambio de fuera, la marca, «Editar todo» con un gesto y «Actualizar» con los dobles de CT-PBT4C-10. Se aplica sin el camino del conflicto. **En el RED** da `NotAppliedConflict` | `editar-todo estado=Closed aplicadas=1 lecturas=1 borrador=Dirty obsoleto=False`; `actualizar estado=Applied borrador=Clean` | `PushBackEditAllReloadTests` | true |
| *(opcional)* **CT-PBT4E-05** | **La guarda de texto:** en `EditAllOfPilot`, una sola llamada a `TryReloadPilotEdit`, después de `_fullEditHost == null` y antes de `ShowModal`, y ningún `ReadIntent` nuevo; con autopruebas. **No sustituye a 01 y 02** (`AGENTS.md`, «Guardia de fuente y prueba de comportamiento») | `recarga llamadas=1 tras-anfitrion=True antes-de-ShowModal=True ReadIntent=0`; las autopruebas `=falla` | `PushBackEditAllReloadGuardTests` | false |

**Lo que no está claro de CT-PBT4E-04:** si el reread de los dobles de la actualización puede ver el «cambio de fuera» sin editar los ayudantes. Lo decide la sonda; si no cabe, CT-04 sale de la lista y el beneficio lo cubre el Smoke-PB.

**Si el Owner elige «Abrir igualmente»** en la pregunta 2, las líneas de CT-02 pasan a `estado=Closed abierto=True` con el aviso, y CT-03 cambia en consonancia.

**Los totales (propuesta):**
- **Core:** +3, o +4 con CT-05.
- **UI:** +1.
- **K = 0, D = 0 y E = 0.**
- **La base de Core,** la del GREEN de PB-T4d en su precondición 6: 19 418 = 19 015 + 0 + 403 (§11). A confirmar con la CI exacta de su GREEN.
- **La CI del RED:**
  - `tests` en failure;
  - `ui-tests` en failure, por CT-03;
  - `build-ui` en success;
  - `build-plugin` omitido.

---

## 4. Lo que queda para el Smoke-PB (AutoCAD real)

Sobre una copia de trabajo de un dibujo con un rack Push Back real (OB-09.2). Se propone sumarlo a §5.5 en lugar de la línea aceptada en PB-T4c:

1. **El borrador Clean.** Abrir el panel del Workspace y fijar la pestaña del rack. Pulsar «Editar»: el campo dice «Listo para editar». No cambiar nada.
2. **El cambio de fuera.** Con el panel visible, cambiar el rack fuera del panel. Por ejemplo, RACKEDITAR (el editor clásico): cambiar un valor que la ventana completa enseña, como el número de frentes o la escala de anotación, y aceptar. Al terminar el comando:
   - el campo dice «Limpio (posiblemente obsoleto)»;
   - la caja queda en solo lectura.
   
   Captura.
3. **«Editar todo».**
   - **Lo esperado:** la ventana abre con el valor nuevo del paso 2, sin aviso.
   - **Al cerrarla sin cambios:** «Editor cerrado sin cambios en el borrador.», el estado «Limpio» sin «(posiblemente obsoleto)» y la caja editable.
   - **La línea base, sin la tarea:** abre con el valor viejo.
4. **Con edición.**
   - Repetir 1 y 2. «Editar todo», cambiar algo, «Aplicar al borrador» y cerrar: el estado dice «Modificado».
   - «Actualizar»:
     - **Lo esperado:** se aplica sin «No se aplicó: el dibujo cambió fuera del panel» ni «Conservar mi borrador».
     - **En el dibujo:** se conservan el cambio del paso 2 y el del editor.
5. **El fallo.** Con un borrador Clean, borrar fuera todas las vistas del rack, y después «Editar todo».
   - **Lo esperado (opción 1 de la pregunta 2):** no abre, y el campo muestra «No editable aquí: el origen ya no está en el dibujo», la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.».
   - Deshacer para restaurar el dibujo.
6. **Dirty, sin cambio.** Un borrador con un cambio propio, un cambio de fuera y «Editar todo».
   - **Lo esperado:** abre sobre la base de antes, como hoy y como «Editar».
   - **Después, «Actualizar»:** el conflicto y «Conservar mi borrador», con sus dos «Sí».
7. **Cero escrituras.** «Editar todo» y cerrar sin «Actualizar» no deben cambiar el dibujo. Por ejemplo, la variable `DBMOD` no cambia por ese gesto solo, y no aparece una entrada nueva en Deshacer. Es una propuesta de cómo mirarlo; la guía la fija.
8. **El modal y el coste:**
   - con la ventana abierta, la paleta está deshabilitada (K = 1);
   - al cerrar, el panel responde;
   - el tiempo entre el clic y la ventana en el caso marcado, frente al no marcado, en un dibujo grande.

---

## 5. Las preguntas para el Owner (borrador, para AskUserQuestion)

### Pregunta 1 (encabezado «Alcance»)

> «Editar todo» no vuelve a leer el dibujo cuando cambió fuera del panel y el borrador no tiene cambios tuyos; «Editar» sí lo hace, sin avisar. ¿Qué arreglo preparo?

1. **Solo la recarga, como «Editar» (Recomendado):** antes de abrir la ventana se vuelve a leer el dibujo, sin aviso, igual que con «Editar». Con cambios tuyos, todo sigue como hoy. Toca una función, y ninguna prueba existente cambia.
2. **La recarga con un aviso:** lo mismo, y el campo dice «Borrador recargado desde el dibujo.». «Editar» no avisa, así que los dos botones dejan de comportarse igual.
3. **La recarga y comprobar también con cambios tuyos:** además, si el borrador tiene cambios tuyos y el dibujo cambió fuera, se compara antes de abrir, y el conflicto sale antes de editar y no al «Actualizar». Va más allá de «Editar»: otro camino, textos nuevos y más pruebas.

### Pregunta 2 (encabezado «Si falla»)

> Si no se puede volver a leer el dibujo (el rack ya no está, hay un comando en curso, o el rack ya no es editable aquí), ¿qué hace «Editar todo»?

1. **No abrir y avisar como «Editar» (Recomendado):** la ventana no se abre, y el campo muestra lo mismo que «Editar»: el motivo, la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.». Abrir sobre una base que ya se sabe vieja es justo lo que se corrige, y si el rack ya no está, «Actualizar» tampoco podría escribir.
2. **Abrir igualmente sobre la base de antes:** la ventana se abre como hoy, con un aviso de que no se pudo recargar. El cambio de fuera se detecta al «Actualizar», como hoy.

### Pregunta 3 (encabezado «Orden»)

> ¿Cuándo va esta tarea respecto a PB-T5, que es la CI exacta, el Smoke-PB con tu firma y el cierre de F4B?

1. **Tarea propia, antes de PB-T5 (Recomendado):** una cadena pequeña, con su intento 1 de 3, cerrada antes de PB-T5, y el Smoke-PB ya mira el comportamiento corregido. Es lo que decidiste en PB-T4d («Tarea propia después»): no mezcla un cambio visible con el cierre, y si falla no retiene el cierre.
2. **Fundirla con PB-T5:** ahorra una cadena, pero PB-T5 llevaría un cambio de código junto a su CI, su Smoke-PB y el cierre. Un fallo de la corrección pararía el cierre.
3. **Después de F4B:** el Smoke-PB mira el comportamiento de hoy, como ya estaba previsto, y la corrección queda anotada para más adelante.

### Pregunta 4 (encabezado «Emisión»)

> Una vez decidida la lista, ¿puedo emitir esta tarea sin volver a preguntarte, con las mismas condiciones que PB-T4a a PB-T4d?

1. **Sí, con condiciones (Recomendado):** se emite si la comprobación acotada da 0 bloqueantes y 0 importantes, si la remedición no cambia nada material y si la lista es la elegida (aquí, ninguna prueba existente sustituida). Te vuelve por una decisión nueva, por una prueba que falle fuera de la lista, por un efecto sin medir o por un intento 3.
2. **Vuelve antes de emitir:** preparo el paquete, el contrato y la comprobación acotada, y te vuelvo con todo antes de emitir.

**Lo que no se pregunta,** porque es de diseño y lo propone la sesión: dónde va la recarga, en el anfitrión (A) o en el panel (B). Se recomienda A, por la regla en un solo sitio, la protección en el puerto y el caso del borde. Tampoco se pregunta si se reutiliza `NotBegun` o se crea un valor nuevo de `EditAllStatus`.

---

## 6. Lo no verificado

**Lo que pide compilar o ejecutar** (la fase siguiente, en una copia exportada):
- Nada se compiló ni se ejecutó.
- **Las líneas de las CT son propuesta,** sin sonda. La de `no-admitida` toma el diagnóstico de `PushBackPilotIntentReader.cs:29` sobre `FullEditIntentKit.OriginMissing`: es deducida.
- **K = 0, D = 0 y E = 0** salen de búsquedas por texto. No es un paseo de guardas ni la suite completa:
  - las guardas que leen WHC (28 archivos de `tests/`) y FVM (8) pueden ver algo que solo se ve al ejecutar;
  - la reflexión sobre tipos no se miró, aunque A no añade tipos;
  - todo esto lo mide la precondición 6.
- **Que el RED compile sin esqueleto** es deducido: las clases nuevas solo usan el API de hoy.
- **Si los dobles de la actualización de CT-PBT4C-10 dejan escribir la receta de CT-PBT4E-04** sin editar ayudantes, no se miró.
- **Los totales de UI de la base** (tras las bajas de PB-T4d) no se leyeron; solo los de Core de §11.

**Lo que se leyó en parte o se dedujo:**
- **La comparación dentro de `PrepareFull`.** Solo se leyó el reparto de `PushBackPilotUpdateRun.cs:359-366` y :443-454.
- **El camino del rack que ya no existe:** `Revalidate` y `PushBackOriginByText.ReadAt` no se leyeron por dentro.
- **Si el puente filtra algún comando** antes de la pista de `CommandEnded`. Afecta a la receta del Smoke-PB: quizá cualquier comando marque el borrador.
- **Lo que pasa en el tramo síncrono entre las dos llamadas de B** es deducido. Solo AutoCAD lo enseña.
- **`DBMOD` o la lista de Deshacer como prueba de cero escrituras** es una propuesta, no medida.

**Lo que solo enseña AutoCAD:**
- el modal real;
- la paleta deshabilitada;
- el foco;
- el coste percibido de la lectura más.

**El nombre del archivo de la revisión:** el encargo y `owner-pbt4c-code-decisions.md` lo llaman `independent-review-code.md`. En `160e019d` no existe con ese nombre: la revisión es `F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205731Z-41db/independent-review.md` (OWNER-02 en :240-246).
