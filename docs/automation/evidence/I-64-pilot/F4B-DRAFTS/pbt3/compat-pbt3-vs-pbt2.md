# I-64 F4B — Compatibilidad del paquete de PB-T3 (v4.1) con el de PB-T2 (v2.5 para PB-T2b, v2.6 para PB-T2c)

> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`, `<checkout-principal>`). El resto no cambia.

**Qué es:** una revisión de solo lectura. Responde a cinco preguntas:
- si lo que PB-T2b y PB-T2c añaden u obligan choca con el diseño de PB-T3;
- qué alcances se cruzan;
- qué tiene que volver a medir la remedición de PB-T3 cuando se cierre PB-T2c;
- qué le falta al generador de PB-T3a para seguir la v4.1;
- qué decisiones tiene que tomar el Owner.

**Entradas:**
- **PB-T3:**
  - `pbt3/I-64-f4b-pbt3-gate-contract-package-v41.md` (citado **P3**), con `v41-delta.md`;
  - `make_pbt3a_contract.py` (citado **GEN**), con `generator-notes-pbt3a.md`;
  - `owner-pbt3-d1-decision.md`.
- **PB-T2:**
  - `pbt2/I-64-f4b-pbt2-gate-contract-package-v26.md` (citado **P2**), con `v25-delta.md` y `v26-delta.md`;
  - `contract-dry-pbt2b.json` y `contract-dry-pbt2c.json`;
  - `owner-pbt2-decisions.md`, que es F4B-E28.
- **El código:** solo por objetos git desde el checkout principal. La revisión de referencia es `5ddeb3b4`: el GREEN del intento 2 de PB-T2a, todavía sin cerrar. También se leyeron `95373af6` (el cierre de PB-T1b), `d8ccbcb8`, la que cita la v4.1, y `5a8366c1` (el cierre de PB-T1c, F4B-E31).

**Mediciones:** las hace `pbt3/compat_measure.py`, que se corre con `python -I` y no escribe nada. Sus resultados se citan como **[M]**.

**El orden real de la fase:** primero PB-T2a, PB-T2b y PB-T2c, y después PB-T3a, PB-T3b y PB-T3c. En `5ddeb3b4`, PB-T1 está cerrada (F4B-E31) y PB-T2a tiene su GREEN, sin cierre todavía.

---

## 0. Veredicto

**COMPATIBLE CON REMEDICIÓN.** Ninguna regla de PB-T2b ni de PB-T2c rompe una premisa de diseño de PB-T3a, PB-T3b o PB-T3c:
- **el modelo de escritura completa de PB-T3a** se queda igual;
- **la corrida generalizada de PB-T3b,** con las seis sustituciones de D-1, también;
- **el borrado de PB-T3c en el Plugin,** también;
- **las invariantes y las CT** no cambian de sentido.

PB-T2 no toca ninguno de los archivos que escribe PB-T3: ni `Draft/`, ni PP, UR o WR, ni las guardas S-1 a S-6. Tampoco cambia el cuerpo de W que PB-T3 cita, porque INV-PBT2B-04 congela `Recompute`, `LoadFromModel` y `ReadInputs` (P2:1194).

**Lo que hay que hacer antes de emitir PB-T3a no es solo mover números de línea:**
- **El paquete necesita una v4.2, solo de texto** (sin cambio de diseño ni de D-1):
  - la BaseSha pasa a ser el cierre de PB-T2c;
  - el paquete recoge dos arrastres que PB-T2 le asigna y que la v4.1 no menciona: A2-11 y A2-12;
  - declara la reconciliación del tipo del borrador con el puerto de PB-T2a;
  - amplía los alcances con las clases de PB-T2.
- **El generador de PB-T3a** tiene que adaptarse a la v4.1 y a la nueva base.

Dos de estos cambios añaden líneas esperadas a algunas CT (I-02 e I-03). Por la precondición 2 de P3, eso pasa por la comprobación acotada. **No hace falta una v5.**

---

## 1. Choques de diseño, punto por punto

### 1.1 El modo WORKSPACE de la ventana

- **Lo que dice PB-T2:**
  - WORKSPACE solo se abre con `ForDraft(PushBackDraftEditorLaunch)` (P2:41-44);
  - las tres cargas públicas lanzan en WORKSPACE (P2:477);
  - la guarda R8 añade una línea al principio de `LoadNew`, `LoadDesignForNew` y `LoadExisting`;
  - CLASSIC no cambia (P2:40).
- **Lo que dice PB-T3:**
  - PB-T3 no toca la ventana (D-PBT3-14, P3:790; C10);
  - CT-PBT3A-01 abre una ventana CLASSIC con `LoadExisting` y compara su `EditableStateForTest` y su `LastComputation` con el asiento (P3:860, :875).
- **Resultado: sin choque.**
  - En CLASSIC, la guarda R8 no hace nada.
  - `LoadExisting` sigue terminando en `LoadFromModel`, y `LoadFromModel` sigue terminando en `Recompute();` (en `5ddeb3b4`, W:332-367 [M]). INV-PBT2B-04 congela las dos.
  - La costura `EditableStateForTest => editable;` se conserva (INV-PBT2B-06).
- **Un punto a favor:** la receta `Estado(sobre)` de PB-T2 (P2:811) es la misma secuencia que `PushBackEditableStateSeat.LoadExisting` (P3:301-308): `LoadNew`, una pasada, `SeedDefaultSafety`, una pasada, `LoadDesign` y otra pasada. Así, CT-PBT2B-02 ya prueba que `ForDraft` sobre un estado nacido así lo conserva, salvo N-PBT2-01. Es la ida y vuelta que PB-T4 necesitará con «Editar todo».
- **Un arrastre que queda viejo:** A-T3-9 (P3:1464) y V2-22 (P3:107) mandan que la ventana use `Seat.Pass` en «PB-T2 o PB-T4». No puede ser en PB-T2:
  - PB-T2 se emite antes de que exista `Seat.Pass`, que llega con PB-T3a;
  - INV-PBT2B-04 prohíbe tocar `Recompute`.

  Queda solo **PB-T4**. Es M-03.

### 1.2 `ApplyToDraft`, el puerto al borrador y el lanzamiento tipado

- **Lo que dice PB-T2:**
  - **El puerto:** `IPushBackDraftStatePort.ApplyToDraft(PushBackEditableState)`. El adaptador `PushBackDraftSessionPort` y `LaunchFor` van sobre **`DraftSession<PushBackEditableState>`** (P2:367-389). Ya están en el producto, en `5ddeb3b4:src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs:44, :46, :61`.
  - **El borrador del panel en PB-T4:** «Un `DraftSession<PushBackEditableState>`…» (HT2-21, P2:314) y A2-4: «El borrador del panel como `DraftSession<PushBackEditableState>`» (P2:1418).
- **Lo que dice PB-T3:**
  - **El borrador completo** es `PushBackFullDraftState(Editable, Origin)` (P3:261-266), sobre un **`DraftSession<PushBackFullDraftState>`** (P3:838).
  - **`Origin` hace falta:** `ForWrite` rematerializa `initial.Origin` para comparar la base (H-06, P3:411-413), y `KeepOn` toma `newInitial.Origin` (P3:428-430).
  - **El escritor completo** solo acepta `DraftSession<PushBackFullDraftState>` (P3:667-669).
  - **El tipo del borrador del anfitrión** cambia en PB-T4 (P3:243; A-T3-1, P3:1457).
- **Resultado: dos arrastres a PB-T4 que se contradicen (I-04). No bloquea PB-T3.**
  - El puerto de la ventana es una interfaz sobre `PushBackEditableState`, así que PB-T4 puede adaptarlo al borrador completo. Pero PB-T3 tiene que decirlo, y A2-4 queda sustituido.
  - Si PB-T4 siguiera A2-4 al pie de la letra, el escritor de PB-T3b quedaría sin uso.
- **El cambio en PB-T3:** un arrastre nuevo, A-T3-11, y una frase en §8.3:
  - PB-T4 conecta la ventana con un adaptador de `IPushBackDraftStatePort` sobre `DraftSession<PushBackFullDraftState>`. Su `ApplyToDraft(edited)` hace `SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`, **con el `Origin` del estado actual**: si no, el comparador, que mira `Origin` (P3:282), daría un `IsDirty` falso;
  - el lanzamiento usa `State = draft.CurrentDraftState.Editable` y saca `SourceProject` del sobre de `Origin`;
  - `PushBackDraftSessionPort` y `LaunchFor`, tipados sobre `PushBackEditableState`, no son el canal del panel: PB-T4 los conserva o los retira, con su registro;
  - A2-4 de PB-T2 queda sustituido.
- **No es del Owner:** es técnico y no cambia nada que se vea.
- **D2:** sin choque. El adaptador copia lo mismo: `PushBackFullDraftStateComparer.Copy` delega en `PushBackEditableStateComparer.Copy` (P3:284).

### 1.3 La regla de «Aplicar» y «Cerrar»

La regla es la de F4B-E28: Q-PBT2-1 (A), aplica sin cerrar y «Cerrar» avisa; y Q-PBT2-2 (A), «Aplicar» rechaza un diseño bloqueado.

- **Lo que dice PB-T2:**
  - «Aplicar» copia el estado al borrador y no escribe; escribe el «Actualizar» del panel, en PB-T3 (P2:538-547);
  - la ventana es modal (OB-01.4, PB-T4).
- **Lo que dice PB-T3:**
  - `Precheck` y `Prepare` parten de `IsDirty` y del estado del borrador (P3:587-590);
  - la barrera (d2) usa `PushBackEditorOutputGate` para los dos modelos (P3:600).
- **Resultado: sin choque.**
  - Con la ventana modal, el borrador solo cambia con «Aplicar», y el escritor nunca corre con la ventana abierta.
  - Q-PBT2-2 (A) usa la misma compuerta (POG) que la barrera de PREPARE: una sola validación, la que pide el [OD] de PB-T2.
  - **A2-7** (P2:1421) solo valía con la opción (B), así que no aplica. La barrera de PB-T3 sigue haciendo falta: para el piloto y para un rack que ya llega bloqueado desde el dibujo. El materializador no mira la compuerta. Es M-07, solo para declararlo.
- **Un caso límite, que es M-05:** L-PBT2-12 (P2:1371) hace que «Guardar en biblioteca» recorte el nombre de la ventana.
  - **Con un nombre nulo,** el recorte da `""`, y «Aplicar» lo lleva al borrador. Entonces:
    - **el escritor de PB-T3 no renombra,** porque un nombre en blanco toma el del sobre del origen (P3:609);
    - **el clasificador sí informa** `$.Name` como `ChangedByUser`, de `null` a `""` (P3:525 y :538): un cambio que la escritura no hace.
  - **Con espacios en los extremos,** el renombrado es real e igual al del clásico (RPC:190). No hay nada que decidir.

### 1.4 BOM y biblioteca en WORKSPACE (D-PB-07, F4B-E23)

- **Lo que dice PB-T2:** dos costuras nuevas en W, `BomDialog` y `LibraryPathDialog`. Hay dos salidas a disco aceptadas, más la del configurador (P2:641-672).
- **Lo que dice PB-T3:**
  - sus pruebas no usan el BOM ni la biblioteca (§5.0, P3:831; C05);
  - el escritor relee el sobre del dibujo y no lee nada de la biblioteca.
- **Resultado: sin choque.** El único efecto que cruza es el del nombre, en §1.3.

### 1.5 Las CT de PB-T2c, que heredan obligaciones

- **CT-PBT2C-01**, el paso 4 de D-PB-06 (P2:951). Lleva el valor exacto y el estilo de cotas **hasta el borrador**. **A2-11** asigna a PB-T3 «la composición», el tramo hasta el DWG (P2:700, :1425).
  - **Lo que pasa en PB-T3:** no menciona D-PB-06, OB-06.3 ni A2-11. Ninguna CT fija un valor exacto ni un estilo con nombre en el sobre que sale (§5.6, P3:1002-1022). CT-PBT3A-06 solo compara `ForWrite` con `Pass` sobre F-01 (P3:865), y CT-PBT3B-01 solo mira `PalletTolerance`, `BeamDepth` y el nombre (P3:912).
  - **Es I-02.** El cambio en PB-T3: CT-PBT3B-01 (o CT-PBT3A-06) gana una línea por fila de `SC1.FilasF()`, con el valor de CT-PBTC-02 (i) leído del diseño del sobre que sale, y las dos filas de estilo, `pb-06-seguridad` y `pb-07-combinado`, con STANDARD-ISO y SIMPLEX. §5.6 gana la fila «OB-06.3 / A2-11».
- **CT-PBT2C-02**, el aviso de D-PB-08, y **CT-PBT2C-03**, el censo hasta el borrador: solo tocan la ventana. Sin relación con PB-T3.
- **A2-12, M09** (P2:1291 y :1426), que se asigna a PB-T3: «una prueba de que toda propiedad declarada de los tres documentos tiene fila en el mapa, o la equivalencia declarada».
  - **Lo que pasa en PB-T3:**
    - justifica el paso 4 con el mapa (P3:423: «Según el mapa de PB-T1b, todo el estado editable es `Editor`»);
    - el clasificador lleva su propia tabla de prefijos, sin regla por defecto para una propiedad de la carga útil fuera de las rutas que lista (P3:534-547);
    - no recoge el arrastre. `pbt3-measurements.md:145` solo lo nombra.
  - **Es I-03.** El cambio en PB-T3: una CT nueva, o una parte de CT-PBT3A-10, que recorra por reflexión las propiedades de `RackEmbedDocument`, `RackProject` y `PushBackDesign` y exija dos cosas: fila en PESO (`Of` ≠ el caso de M09) y un grupo del clasificador. Otra vía: declarar la equivalencia y añadir a la tabla de D-PBT3-07 la fila por defecto «Payload, lo demás → `Rack`».

### 1.6 Cada pieza de PB-T3, frente a PB-T2

| Pieza de PB-T3 | Frente a PB-T2 | Resultado |
|---|---|---|
| **PB-T3a, el modelo**: `Draft/`, el asiento, el lector, la composición, la disposición y el clasificador | Ningún nombre ni archivo compartido [M: 0 nombres comunes; el namespace `…PushBack.Draft` no existe en `5ddeb3b4`]. Los archivos de PB-T2a están en `Systems/PushBack/` y no en `Draft/`, y no usan `Draft` como nombre simple | Compatible. Pendientes: I-03 y I-04 |
| **PB-T3b, la corrida generalizada y las seis sustituciones de D-1** | PB-T2 no toca PP, UR ni UP. Sus `ForbiddenWriteScope` incluyen `tests/RackCad.Tests/Workspace/`, así que PrepareGuard, UpdateGuard e IntegrationGuard no cambian. G-10 sigue con una sola `.RequestUpdate(` en W (INV-PBT2B-04) | Compatible. S-1 a S-6, E-1 a E-6 y las anclas de las guardas (:107, :134-141, :882, :907-909) no se mueven por PB-T2 |
| **PB-T3c, el borrado en el Plugin** | PB-T2 no toca el Plugin. INV-PBT3C-01 excluye `src/RackCad.UI` | Compatible |
| **Las invariantes comunes y propias** | Ninguna mira archivos de PB-T2. CT-PBT3A-12 y CT-PBT3B-13 («sin conectar») miran tipos que PB-T2 no nombra | Compatibles |
| **AllowedWriteScope, ForbiddenWriteScope y la BaseSha** | Ver §2 y I-01 | Hace falta remedición |

---

## 2. Los alcances: lo que PB-T2b y PB-T2c escriben, frente al ForbiddenWriteScope y las anclas de PB-T3

| Archivo de PB-T2 | En PB-T3 | ¿Rompe algo más que números de línea? |
|---|---|---|
| **W** (PB-T2b; los mutantes de PB-T2c se revierten) | **Prohibido** por el prefijo `src/RackCad.UI/`. Es **ancla** de D-PBT3-03, CT-PBT3A-01, V41-04, V41-05 y A-T3-9 | **Las anclas, no:** el generador busca los bloques por contenido (GEN:469-485), y su contenido queda congelado por INV-PBT2B-04 y PBT2B-C13. **El generador, sí** (I-01): exige que `src/RackCad.UI` solo difiera del GREEN de PB-TC2 en W y WC (GEN:187-188), y que `src` sea igual al cierre de PB-T1c (GEN:201) |
| `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs` (PB-T2b, nuevo) | Prohibido por el prefijo. No cambia ningún recuento [M] | **Sí:** rompe GEN:187-188 |
| `src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs` y `PushBackDraftEditorLaunch.cs` (PB-T2a, ya en el código) | `cover()` los lista uno a uno. El `src/` prohibido de PB-T3a pasa de **87 a 89**, y el de PB-T3b de **96 a 98**. El de PB-T3c sigue en **56** [M] | **Sí:** rompen GEN:184-186, que exige Application igual al cierre de PB-T1b. El recuento no es un `assert` fijo |
| WC (los mutantes de PB-T2c, revertidos) | Prohibido. No es ancla | No |
| **Las 9 clases de prueba de PB-T2:**<br>• Core: `PushBackDraftStatePortTests`, `PushBackDraftPortSharedReferenceGuardTests`, `PushBackWorkspaceModeSourceGuardTests` y `PushBackWorkspaceSharedReferenceGuardTests`;<br>• UI: `PushBackWorkspaceSupport`, `PushBackWorkspaceModeTests`, `PushBackWorkspaceApplyCloseTests`, `PushBackFieldValidationConcordanceTests` y `PushBackWorkspaceCarryoverTests` | **No están en ningún `ForbiddenWriteScope` de PB-T3** [M: 0 de 9 cubiertas]. GEN:283-296 copia las listas del contrato de PB-T1c, y P3 §6.0 (P3:1074) solo nombra las clases de PB-T1 y de PB-T3 | No rompe nada, pero el alcance prohibido queda **incompleto** (M-01). Con las listas del contrato de PB-T2c (Core 29, UI 67) y su propia clase, PB-T3a queda en Core 29 y UI 68 [M] |

**Los filtros:** ningún filtro de PB-T3 selecciona una clase de PB-T2 por prefijo de nombre completo [M]. Por ejemplo, `Workspace` es `RackCad.Tests.Workspace`, y no casa con `RackCad.Tests.PushBackWorkspace…`. Los `MinSelected` de `Workspace` (3 271 y 547) no cambian por PB-T2.

---

## 3. Lo que la remedición de PB-T3 tiene que volver a medir tras el cierre de PB-T2c

**Base y anclaje (I-01):**
1. **El cierre de PB-T2c:** su entrada «Cierre de F4B-PBT2C-WORKSPACE-CARRYOVER» en `f4b-dispositions.md`, que la BaseSha desciende de él y que `git diff <cierre de PB-T2c> <BaseSha> -- src` sale vacío.
2. **Las referencias nuevas del anclaje:**
   - Application = el cierre de PB-T1b más los dos archivos de PB-T2a;
   - Domain y Plugin = el cierre de PB-T1b;
   - `src/RackCad.UI` = el cierre de PB-T2b: W y `PushBackEditorMode.cs`, con WC igual al cierre de PB-T1c.
3. **La línea base de la CI:** la del GREEN de PB-T2c. Prevista por P2:1247: Core 19 094 y UI 2 450 (2 429 `Passed` y 21 `Skipped`). Con eso, **el GREEN previsto de PB-T3a es Core 19 107 y UI 2 451**, no los 19 101 y 2 440 de P3:1191. Las de PB-T3b y PB-T3c se recalculan en cadena.

**Las anclas de W:** el contenido no cambia, pero todas las líneas se mueven. Ya se movieron con el GREEN de PB-T1c, y PB-T2b las moverá otra vez:

| Ancla de P3 (en `d8ccbcb8`) | En `5ddeb3b4` [M] | Tras PB-T2b |
|---|---|---|
| W:145-146, los ensambladores | 145-146 | Medir: PB-T2b añade campos y constantes |
| W:191, `LastComputation` | 191 | Medir |
| W:312, la firma de `LoadExisting` | 308 | Medir: la guarda R8 entra justo después |
| W:440, el `Recompute();` final de `LoadFromModel` (V41-05; CT-01) | 366 (`LoadFromModel` ocupa W:332-367) | Medir y comprobar que sigue siendo lo último |
| W:660-705, la adopción en `Recompute` (D-PBT3-03; A-T3-9) | 586-631 | Medir |
| W:663-671, el `catch` (V41-04) | 589-597 | Medir |
| W:168, la costura `EditableStateForTest` | 168, ya en la forma del GREEN | Medir |

Comprobar también que `Recompute`, `LoadFromModel` y `ReadInputs` tienen el mismo cuerpo que en el cierre de PB-T1c (INV-PBT2B-04).

**Lo que no cambia con PB-T2, pero se vuelve a comprobar:**
- PP, UR, UP, WR, PM, IR, PIR y PIS;
- DS:345, :469-486 y DR:143;
- WHC:328, :347-352, :409 y :675;
- PES, PESC y PESO:142, :165-172;
- RVA, `RackViewCodec`, RPC y RCS;
- PrepareGuard, UpdateGuard e IntegrationGuard: S-1 a S-6, :882, :907-909 y G-10 (:725-767);
- I58F1Matrix e I58F1Fixtures;
- `CustomPropertiesEnvelopeGuardTests`:194-196;
- los kits.

**Los recuentos y los alcances (M-01 y M-02):**
- **El `src/` prohibido:** PB-T3a, 89; PB-T3b, 98; PB-T3c, 56 [M].
- **El `tests/` prohibido de PB-T3a:** Core 29 y UI 68 (con `PushBackWorkspaceCarryoverTests`), tomados del contrato de PB-T2c. Con la raíz (17), el total es **203**, no los 192 del seco de la v4. En PB-T3b y PB-T3c, además, las clases de las subtareas anteriores de PB-T3.
- **La lista explícita de PB-T3b** de `tests/RackCad.Tests/Workspace/`: 79 archivos en `5ddeb3b4` [M], y 76 sin las guardas. PB-T2 no añade nada ahí, pero hay que volver a medirla.
- **Los `RequiredTests` sin `ExpectRed` que entran en la red de PB-T3:** las clases de PB-T2 y sus mínimos de P2:
  - Core: `PushBackDraftStatePortTests` 3, `PushBackDraftPortSharedReferenceGuardTests` 1, `PushBackWorkspaceModeSourceGuardTests` 1 y `PushBackWorkspaceSharedReferenceGuardTests` 1;
  - UI: `PushBackWorkspaceModeTests` 3, `PushBackWorkspaceApplyCloseTests` 4, `PushBackFieldValidationConcordanceTests` 1 y `PushBackWorkspaceCarryoverTests` 3.
- **La comprobación de F4B-E30** (GEN:1005-1008), repetida sobre esas clases de UI. Se espera 0, por INV-08 de PB-T2.
- **Que ningún filtro de PB-T3 seleccione una clase de PB-T2:** hoy, ninguno [M].
- **Los `MinSelected` medidos de PB-T3,** con `Workspace` 3 271 y 547 y los de PB-T1.

**Lo demás:**
- `f4b-dispositions.md` hasta el cierre de PB-T2c, con F4B-E28 y F4B-E31 a F4B-E3x, para las autoridades y el estado;
- que los nombres nuevos de PB-T3 siguen libres;
- el censo de los diseños de las clases de F4 (precondición 2), que no depende de PB-T2;
- la Snapshot vigente y las celdas, que vencen el 2026-12-29.

---

## 4. El generador de PB-T3a frente a la v4.1

Hoy **aborta** si lee la v4.1, por tres `assert`:
- el Objective (GEN:649-657 frente a P3:1138);
- el número de notas del §6.0 (GEN:886);
- el sha256 (GEN:922).

**Lo que tiene que cambiar para fiarse de la v4.1 (I-05):**
1. **La versión:**
   - `PKG_VERSION = 'v4.1'`;
   - `CUSTODY_SHA256` = `91f20c8b0d…a7bbb669` (186 385 bytes, LF [M]);
   - el docstring y los mensajes que dicen «v4».
2. **`OBJECTIVE`:** «…segun el paquete del contrato de PB-T3 **v4.1**…». Es el único cambio de §6.1 en la v4.1.
3. **Las notas del §6.0 (V41-03):** la v4.1 añade la nota de (A′) en el `ForbiddenWriteScope` de PB-T3b (P3:1077). El `assert` de GEN:886 pasa de `== 1` a `== 2`. `bullets()` ya la descarta, porque va con sangría 4 (GEN:612-640).
4. **La custodia (V41-01 y V41-03):**
   - **`CUSTODY`:** la descripción del paquete nombra el registro v4 → v4.1 (V41-01 a V41-06), y la de la revisión, su §12 (LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/1);
   - **`CUSTODY_HEADS`:** la revisión gana `'## §12. '`;
   - **`CUSTODY_EXTRA`:** gana `v41-delta.md`, y el `assert` pasa de 16 a 17.
5. **Las anclas (V41-04):** `W_BLOCKS` mide sobre las líneas del cierre de PB-T1b (659-704) y busca por contenido en la revisión, así que sobrevive a PB-T1c y a PB-T2b. Para seguir la letra de la v4.1 le faltan tres cosas:
   - el `catch` como bloque propio: 662-670 en el cierre de PB-T1b, 663-671 en `d8ccbcb8` y 589-597 en `5ddeb3b4`;
   - el `Recompute();` final de `LoadFromModel` (W:440 de V41-05);
   - informar el desplazamiento **frente a la línea que cita el paquete**, y no solo frente al cierre de PB-T1b.

   Tras la remedición, el paquete citará las líneas de la BaseSha, y el generador tiene que comprobar esas.
6. **IntegrationGuard:882 (V41-04)** no lo usa el generador de PB-T3a. Le toca al generador de PB-T3b, que todavía no existe: `BodyOf` de S-5 en :882 y no en :881.
7. **La variante (A):** el contrato de PB-T3a no lleva texto de la variante.
   - Basta con que nada de (A′) entre en lo que se copia: hoy, la única aparición en el §6.0 es la nota, y `bullets()` la descarta.
   - Conviene una comprobación en negativo: ningún texto copiado contiene `(A')` en su forma ASCII.
   - Los invariantes comunes 07 y C02, con las dos salvedades de PB-T3b, no cambian en la v4.1 (DU-4).

**Lo que tiene que cambiar por PB-T2 (I-01 y M-01):**
- **GEN:183-188:** las referencias pasan a ser el cierre de PB-T2a (Application) y el de PB-T2b (UI).
- **GEN:196-219:** `PT1C_CLOSE` pasa a ser `PT2C_CLOSE`, el cierre de PB-T2c, y `CI_BASE_T1C`, `CI_BASE_T2C`.
- **GEN:736-738 y :1057-1063:**
  - la línea de la CI de la v4.2;
  - `CI_FORECAST = (19094, 2450, 21)`;
  - el GREEN, 19 107 y 2 451.

  El `assert` que la ata al contrato de PB-T1c pasa a atarla al de PB-T2c.
- **GEN:283-296 y :1043-1046:** las listas de `tests/` y los `MinSelected` que no bajan se toman del contrato de PB-T2c, con su `AllowedWriteScope` de pruebas.
- **`REQ_SPEC`:** las 8 filas de PB-T2 de §3.
- **Las autoridades:** el cierre de PB-T2c y su `task-close-package.md`, en lugar del de PB-T1c.

---

## 5. Las decisiones del Owner

**Esta revisión no saca ninguna decisión nueva.** Todo lo que aparece es una corrección del paquete o del generador:
- I-01 a I-05 y M-01 a M-07;
- la reconciliación del tipo del borrador (I-04) es técnica: un adaptador sin efecto visible;
- A2-12 ya ofrece las dos salidas, la prueba o la equivalencia declarada, y elige la sesión.

**Una decisión ya pendiente, que no sale de aquí:** DU-1 de `generator-notes-pbt3a.md`. No hay autorización de emisión de PB-T3a:
- F4B-E28, pregunta 4, solo cubre PB-T2a, PB-T2b y PB-T2c;
- F4B-E29 solo decide D-1.

Sigue haciendo falta antes de emitir PB-T3a.

**Lo que se informa al Owner, sin pregunta, si la sesión quiere** (no es decisión):
- un nombre recortado al guardar en la biblioteca desde WORKSPACE se escribe en el dibujo con el siguiente «Actualizar» del panel, igual que en RACKEDITAR (§1.3).

---

## 6. Hallazgos

**B 0 / I 5 / M 7**

| Id | Grav. | Hallazgo | Cambio |
|---|---|---|---|
| I-01 | I | **La base de PB-T3a sigue atada al cierre de PB-T1c, y la fase pone PB-T2 delante.** En el paquete: la precondición 1 (P3:142-146, «Falta el cierre de PB-T1c, que fija la BaseSha»), el §6.0 («el diff de src frente al cierre de la subtarea anterior», P3:1056-1057) y la línea base de la CI (P3:1191). En el generador, seis `assert` duros abortan con PB-T2 en `src`: GEN:184-186, :187-188, :201, :211-219, :1057-1063 y la fuente de las listas en :283-296. Abortar es seguro: no saca un contrato equivocado | La v4.2:<br>• la BaseSha pasa a ser el cierre de PB-T2c;<br>• línea base prevista: Core 19 094 y UI 2 450, y GREEN Core 19 107 y UI 2 451;<br>• el generador cambia sus referencias, como dice §4 |
| I-02 | I | **A2-11 de PB-T2** (el paso 4 de D-PB-06: el valor exacto y el estilo hasta el DWG; «la composición», a PB-T3) **no está en PB-T3:** no hay ninguna CT ni fila de trazabilidad (P2:1425 frente a P3:1002-1022) | CT-PBT3B-01 (o CT-PBT3A-06) gana las filas de `SC1.FilasF()` y las dos de estilo, leídas del diseño del sobre que sale. §5.6 gana la fila de OB-06.3 y A2-11. Pasa por la comprobación acotada |
| I-03 | I | **A2-12 de PB-T2 (M09) no está en PB-T3,** y PB-T3 usa el mapa para justificar el paso 4 (P3:423). Además, la tabla del clasificador no tiene regla por defecto para una propiedad de la carga útil fuera de las rutas que lista (P3:534-547) | Una prueba por reflexión: cada propiedad de los tres documentos tiene fila en PESO y grupo en el clasificador. Otra vía: declarar la equivalencia y añadir la fila por defecto. Pasa por la comprobación acotada |
| I-04 | I | **Dos arrastres a PB-T4 que se contradicen sobre el tipo del borrador del panel.** PB-T2 dice `DraftSession<PushBackEditableState>` (HT2-21, P2:314; A2-4, P2:1418; el puerto y `LaunchFor` de PB-T2a ya en el código). PB-T3 dice `DraftSession<PushBackFullDraftState>` con `Origin` (P3:261-266, :667-669 y :1457) | A-T3-11 y una frase en §8.3, como dice §1.2:<br>• el adaptador del puerto de la ventana sobre el borrador completo, con el `Origin` del estado actual;<br>• A2-4 queda sustituido;<br>• el destino de `PushBackDraftSessionPort` y `LaunchFor` lo decide PB-T4, con su registro |
| I-05 | I | **El generador de PB-T3a aborta con la v4.1:** el Objective, el número de notas del §6.0 (V41-03) y el sha256. Y la custodia no conoce la v4.1 ni la §12 | Los cambios 1 a 7 de §4 |
| M-01 | M | **Las 9 clases de prueba de PB-T2 no están en ningún `ForbiddenWriteScope` de PB-T3** (P3:1074; GEN:283-296) [M: 0 de 9] | El §6.0 añade «las clases de PB-T2». El generador las toma del contrato de PB-T2c: Core 29 y UI 68 en PB-T3a |
| M-02 | M | **La red de PB-T3 no incluye PB-T2:** ni §5.1 (P3:848), ni las tablas de `RequiredTests`, ni §4. Faltan sus guardas CT-PBT2A-04 y CT-PBT2B-08, 09 y 10 | Las 8 filas de §3, sin `ExpectRed`. La comprobación de F4B-E30 sobre las clases de UI que se añaden |
| M-03 | M | **A-T3-9 y V2-22** mandan la adopción por el asiento a «PB-T2 o PB-T4» (P3:107 y :1464). PB-T2 va antes de PB-T3a y congela `Recompute` (INV-PBT2B-04) | Va solo a «PB-T4» |
| M-04 | M | **La precondición 7 y R-T3-09** («PB-T2 puede ir antes o después», P3:157 y :1308) y el estado de la cabecera (P3:13, «PB-T1c en su intento 2») están desfasados: PB-T1c se cerró (F4B-E31) y PB-T2 va delante | El orden real, con F4B-E31 y las emisiones de PB-T2 |
| M-05 | M | **El nombre nulo pasa a `""` en el borrador** (L-PBT2-12, P2:1371). El escritor lo trata como en blanco y no renombra (P3:609), pero el clasificador informa `$.Name` `ChangedByUser`, un cambio que no se escribe (P3:525 y :538) | Declararlo para la presentación de PB-T4 (A-T3-3), o que el clasificador trate `$.Name` en blanco como el escritor |
| M-06 | M | **Las anclas de W de la v4.1** (`d8ccbcb8`) ya están desplazadas en el cierre de PB-T1c: W:660-705 está en 586-631, W:663-671 en 589-597, W:312 en 308 y W:440 en 366 [M]. PB-T2b las moverá otra vez. El contenido no cambia | La remedición las cita sobre la BaseSha, como dice §3 |
| M-07 | M | **Q-PBT2-2 (A) deja A2-7 sin objeto,** y PB-T3 no lo dice. La compuerta de «Aplicar» y la barrera de PREPARE son la misma (POG) | Una frase en §8.3. F4B-E28 entre las autoridades, por coherencia |

## 7. Límites e incidencias

- **Nada se compiló ni se ejecutó con dotnet.** Las cifras de la CI de PB-T2 y de PB-T3 son previsiones de sus paquetes. Solo la de PB-T1c está medida: `44463212627`, con Core 19 088 y UI 2 439 (F4B-E31).
- **PB-T2b y PB-T2c no existen en el código.** Lo que se dice de W tras PB-T2b sale de INV-PBT2B-03 y 04 y de PBT2B-C13, no de su texto.
- **El worktree no se leyó ni se ejecutó.** Todo se leyó por objetos git desde el checkout principal, y Python solo con `-I`, sobre `compat_measure.py`, escrito con la herramienta de archivos.
- **Una incidencia de procedimiento:**
  - **Qué pasó:** para comparar W, se extrajo W (`d8ccbcb8` y `5ddeb3b4`) y `f4b-dispositions.md` con `git show` a una carpeta temporal del sistema (`/tmp`). La regla del encargo solo admite escribir en `pbt3\`.
  - **Lo que no pasó:** no tocó el repositorio ni el worktree.
  - **Cómo se resolvió:** los archivos se borraron al darse cuenta. Las mediciones citadas se rehicieron después con `compat_measure.py`, que no escribe.
  - **Se declara aquí,** sin clasificarla por analogía.
